using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public sealed class SqlServerManagedOperationStore(
    IOptions<SqlServerPersistenceOptions> options) : IManagedOperationStore
{
    private readonly SqlServerPersistenceOptions settings = options.Value;

    public async Task<ManagedOperationStatus> EnqueueOrReuseAsync(
        ManagedOperationEnqueueRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        await using (var existing = Command(connection, transaction, """
            SELECT TOP (1) {{columns}}
            FROM {{schema}}.managed_operations WITH (UPDLOCK, HOLDLOCK)
            WHERE deduplication_key = @deduplication_key
              AND operation_state IN (N'Queued', N'Running', N'Cancelling', N'Interrupted')
            ORDER BY created_at DESC;
            """))
        {
            existing.CommandText = BindColumns(existing.CommandText);
            existing.Parameters.AddWithValue("@deduplication_key", request.DeduplicationKey);
            var found = await ReadAsync(existing, cancellationToken);
            if (found is not null)
            {
                await transaction.CommitAsync(cancellationToken);
                return found;
            }
        }

        var now = DateTimeOffset.UtcNow;
        var operationId = $"OP-{Guid.NewGuid():N}";
        var correlationId = $"CORR-{Guid.NewGuid():N}";
        await using (var insert = Command(connection, transaction, """
            INSERT INTO {{schema}}.managed_operations (
                operation_id, operation_kind, deduplication_key, correlation_id,
                operation_state, current_stage, created_at, updated_at,
                progress_percent, safe_status_detail, retry_safe,
                user_approval_required, administrative_intervention_required,
                ruleset_id
            ) VALUES (
                @operation_id, @operation_kind, @deduplication_key, @correlation_id,
                N'Queued', N'Queued', @now, @now,
                0, @safe_status_detail, 1,
                0, 0, @ruleset_id
            );
            """))
        {
            insert.Parameters.AddWithValue("@operation_id", operationId);
            insert.Parameters.AddWithValue("@operation_kind", request.OperationKind);
            insert.Parameters.AddWithValue("@deduplication_key", request.DeduplicationKey);
            insert.Parameters.AddWithValue("@correlation_id", correlationId);
            insert.Parameters.AddWithValue("@now", now);
            insert.Parameters.AddWithValue("@safe_status_detail", request.SafeStatusDetail);
            insert.Parameters.AddWithValue("@ruleset_id", request.RulesetId);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return (await GetAsync(operationId, cancellationToken))!;
    }

    public async Task<ManagedOperationStatus?> GetAsync(
        string operationId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, null, """
            SELECT {{columns}}
            FROM {{schema}}.managed_operations
            WHERE operation_id = @operation_id;
            """);
        command.CommandText = BindColumns(command.CommandText);
        command.Parameters.AddWithValue("@operation_id", operationId);
        return await ReadAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<ManagedOperationStatus>> ListRecentAsync(
        string? operationKind,
        int maximumCount,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, null, """
            SELECT TOP (@maximum_count) {{columns}}
            FROM {{schema}}.managed_operations
            WHERE @operation_kind IS NULL OR operation_kind = @operation_kind
            ORDER BY updated_at DESC, operation_id DESC;
            """);
        command.CommandText = BindColumns(command.CommandText);
        command.Parameters.AddWithValue("@maximum_count", maximumCount);
        command.Parameters.AddWithValue("@operation_kind", (object?)operationKind ?? DBNull.Value);
        var result = new List<ManagedOperationStatus>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    public async Task<int> RecoverInterruptedAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, null, """
            UPDATE {{schema}}.managed_operations
            SET operation_state = N'Interrupted',
                current_stage = N'Interrupted',
                updated_at = SYSUTCDATETIME(),
                safe_status_detail = N'The service restarted while this operation was running; durable retry will resume idempotent publication.',
                error_code = N'MANAGED_OPERATION_INTERRUPTED',
                retry_safe = 1,
                administrative_intervention_required = 0
            WHERE operation_state IN (N'Running', N'Cancelling');
            """);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<ManagedOperationStatus?> ClaimNextAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);
        string? operationId;
        await using (var select = Command(connection, transaction, """
            SELECT TOP (1) operation_id
            FROM {{schema}}.managed_operations WITH (UPDLOCK, READPAST, ROWLOCK)
            WHERE operation_state IN (N'Queued', N'Interrupted')
            ORDER BY CASE operation_state WHEN N'Interrupted' THEN 0 ELSE 1 END,
                     created_at,
                     operation_id;
            """))
        {
            operationId = (string?)await select.ExecuteScalarAsync(cancellationToken);
        }

        if (operationId is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        await using (var update = Command(connection, transaction, """
            UPDATE {{schema}}.managed_operations
            SET operation_state = N'Running',
                current_stage = N'AcquireSource',
                started_at = COALESCE(started_at, SYSUTCDATETIME()),
                updated_at = SYSUTCDATETIME(),
                progress_percent = 1,
                safe_status_detail = N'Durable background execution has started.',
                error_code = NULL,
                retry_safe = 1,
                administrative_intervention_required = 0
            WHERE operation_id = @operation_id;
            """))
        {
            update.Parameters.AddWithValue("@operation_id", operationId);
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(operationId, cancellationToken);
    }

    public Task UpdateStageAsync(
        string operationId,
        string stage,
        int? progressPercent,
        string safeStatusDetail,
        CancellationToken cancellationToken) =>
        UpdateAsync(
            operationId,
            ManagedOperationState.Running,
            stage,
            progressPercent,
            safeStatusDetail,
            null,
            retrySafe: true,
            administrativeInterventionRequired: false,
            null,
            null,
            completed: false,
            cancellationToken);

    public Task CompleteAsync(
        string operationId,
        string stage,
        string safeStatusDetail,
        string? sourceIdentity,
        string? resultRuleReleaseId,
        CancellationToken cancellationToken) =>
        UpdateAsync(
            operationId,
            ManagedOperationState.Succeeded,
            stage,
            100,
            safeStatusDetail,
            null,
            retrySafe: false,
            administrativeInterventionRequired: false,
            sourceIdentity,
            resultRuleReleaseId,
            completed: true,
            cancellationToken);

    public Task FailAsync(
        string operationId,
        ManagedOperationState state,
        string stage,
        string errorCode,
        string safeStatusDetail,
        bool retrySafe,
        bool administrativeInterventionRequired,
        CancellationToken cancellationToken) =>
        UpdateAsync(
            operationId,
            state,
            stage,
            null,
            safeStatusDetail,
            errorCode,
            retrySafe,
            administrativeInterventionRequired,
            null,
            null,
            completed: state is ManagedOperationState.Failed or ManagedOperationState.Cancelled,
            cancellationToken);

    private async Task UpdateAsync(
        string operationId,
        ManagedOperationState state,
        string stage,
        int? progressPercent,
        string safeStatusDetail,
        string? errorCode,
        bool retrySafe,
        bool administrativeInterventionRequired,
        string? sourceIdentity,
        string? resultRuleReleaseId,
        bool completed,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, null, """
            UPDATE {{schema}}.managed_operations
            SET operation_state = @operation_state,
                current_stage = @current_stage,
                updated_at = SYSUTCDATETIME(),
                completed_at = CASE WHEN @completed = 1 THEN SYSUTCDATETIME() ELSE completed_at END,
                progress_percent = COALESCE(@progress_percent, progress_percent),
                safe_status_detail = @safe_status_detail,
                error_code = @error_code,
                retry_safe = @retry_safe,
                user_approval_required = 0,
                administrative_intervention_required = @administrative_intervention_required,
                source_identity = COALESCE(@source_identity, source_identity),
                result_rule_release_id = COALESCE(@result_rule_release_id, result_rule_release_id)
            WHERE operation_id = @operation_id;

            IF @@ROWCOUNT <> 1
                THROW 51040, 'Managed Operation target was not found.', 1;
            """);
        command.Parameters.AddWithValue("@operation_id", operationId);
        command.Parameters.AddWithValue("@operation_state", state.ToString());
        command.Parameters.AddWithValue("@current_stage", stage);
        command.Parameters.AddWithValue("@completed", completed);
        command.Parameters.AddWithValue("@progress_percent", (object?)progressPercent ?? DBNull.Value);
        command.Parameters.AddWithValue("@safe_status_detail", safeStatusDetail);
        command.Parameters.AddWithValue("@error_code", (object?)errorCode ?? DBNull.Value);
        command.Parameters.AddWithValue("@retry_safe", retrySafe);
        command.Parameters.AddWithValue("@administrative_intervention_required", administrativeInterventionRequired);
        command.Parameters.AddWithValue("@source_identity", (object?)sourceIdentity ?? DBNull.Value);
        command.Parameters.AddWithValue("@result_rule_release_id", (object?)resultRuleReleaseId ?? DBNull.Value);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<SqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private SqlCommand Command(
        SqlConnection connection,
        SqlTransaction? transaction,
        string text) =>
        new(SqlServerSchemaIdentifier.Bind(text, settings.DomainSchema), connection, transaction)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };

    private static async Task<ManagedOperationStatus?> ReadAsync(
        SqlCommand command,
        CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Read(reader) : null;
    }

    private static ManagedOperationStatus Read(SqlDataReader reader) =>
        new(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            Enum.Parse<ManagedOperationState>(reader.GetString(3), ignoreCase: false),
            reader.GetString(4),
            reader.GetDateTimeOffset(5),
            reader.IsDBNull(6) ? null : reader.GetDateTimeOffset(6),
            reader.GetDateTimeOffset(7),
            reader.IsDBNull(8) ? null : reader.GetDateTimeOffset(8),
            reader.IsDBNull(9) ? null : reader.GetInt32(9),
            reader.GetString(10),
            reader.IsDBNull(11) ? null : reader.GetString(11),
            reader.GetBoolean(12),
            reader.GetBoolean(13),
            reader.GetBoolean(14),
            reader.IsDBNull(15) ? null : reader.GetString(15),
            reader.IsDBNull(16) ? null : reader.GetString(16),
            reader.IsDBNull(17) ? null : reader.GetString(17));

    private static string BindColumns(string sql) =>
        sql.Replace(
            "{{columns}}",
            "operation_id, operation_kind, correlation_id, operation_state, current_stage, " +
            "created_at, started_at, updated_at, completed_at, progress_percent, " +
            "safe_status_detail, error_code, retry_safe, user_approval_required, " +
            "administrative_intervention_required, ruleset_id, source_identity, result_rule_release_id",
            StringComparison.Ordinal);
}
