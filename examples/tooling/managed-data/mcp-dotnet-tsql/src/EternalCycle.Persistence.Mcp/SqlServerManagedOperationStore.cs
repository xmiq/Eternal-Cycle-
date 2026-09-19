using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public sealed class SqlServerManagedOperationStore(
    IOptions<SqlServerPersistenceOptions> options) : IManagedOperationStore
{
    private readonly SqlServerPersistenceOptions settings = options.Value;
    private readonly string executionOwnerId = $"WORKER-{Guid.NewGuid():N}";

    public async Task<ManagedOperationStatus> EnqueueOrReuseAsync(
        ManagedOperationEnqueueRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        await RecoverExpiredAsync(connection, transaction, cancellationToken);
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
        await using (var ensureRuleset = Command(connection, transaction, """
            IF NOT EXISTS (
                SELECT 1
                FROM {{schema}}.rulesets WITH (UPDLOCK, HOLDLOCK)
                WHERE ruleset_id = @ruleset_id
            )
                INSERT INTO {{schema}}.rulesets (ruleset_id, display_name)
                VALUES (@ruleset_id, @ruleset_id);
            """))
        {
            ensureRuleset.Parameters.AddWithValue("@ruleset_id", request.RulesetId);
            await ensureRuleset.ExecuteNonQueryAsync(cancellationToken);
        }

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
        await RecoverInterruptedAsync(cancellationToken);
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

    public async Task<ManagedOperationStatus?> GetByCorrelationAsync(
        string correlationId,
        CancellationToken cancellationToken)
    {
        await RecoverInterruptedAsync(cancellationToken);
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, null, """
            SELECT TOP (1) {{columns}}
            FROM {{schema}}.managed_operations
            WHERE correlation_id = @correlation_id
            ORDER BY updated_at DESC, operation_id DESC;
            """);
        command.CommandText = BindColumns(command.CommandText);
        command.Parameters.AddWithValue("@correlation_id", correlationId);
        return await ReadAsync(command, cancellationToken);
    }

    public async Task<IReadOnlyList<ManagedOperationStatus>> ListRecentAsync(
        string? operationKind,
        int maximumCount,
        CancellationToken cancellationToken)
    {
        await RecoverInterruptedAsync(cancellationToken);
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
        return await RecoverExpiredAsync(connection, null, cancellationToken);
    }

    public async Task<ManagedOperationStatus?> ClaimNextAsync(
        TimeSpan executionLeaseDuration,
        CancellationToken cancellationToken)
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
                administrative_intervention_required = 0,
                execution_owner_id = @execution_owner_id,
                execution_lease_expires_at = DATEADD(millisecond, @lease_milliseconds, SYSUTCDATETIME()),
                execution_attempt_count = execution_attempt_count + 1
            WHERE operation_id = @operation_id
              AND operation_state IN (N'Queued', N'Interrupted');
            """))
        {
            update.Parameters.AddWithValue("@operation_id", operationId);
            update.Parameters.AddWithValue("@execution_owner_id", executionOwnerId);
            update.Parameters.AddWithValue("@lease_milliseconds", LeaseMilliseconds(executionLeaseDuration));
            await update.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return await GetAsync(operationId, cancellationToken);
    }

    public async Task<bool> RenewExecutionLeaseAsync(
        string operationId,
        TimeSpan executionLeaseDuration,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, null, """
            UPDATE {{schema}}.managed_operations
            SET execution_lease_expires_at = DATEADD(millisecond, @lease_milliseconds, SYSUTCDATETIME()),
                updated_at = SYSUTCDATETIME()
            WHERE operation_id = @operation_id
              AND operation_state IN (N'Running', N'Cancelling')
              AND execution_owner_id = @execution_owner_id
              AND execution_lease_expires_at > SYSUTCDATETIME();
            """);
        command.Parameters.AddWithValue("@operation_id", operationId);
        command.Parameters.AddWithValue("@execution_owner_id", executionOwnerId);
        command.Parameters.AddWithValue("@lease_milliseconds", LeaseMilliseconds(executionLeaseDuration));
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
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
            releaseOwnership: false,
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
            releaseOwnership: true,
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
            releaseOwnership: true,
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
        bool releaseOwnership,
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
                result_rule_release_id = COALESCE(@result_rule_release_id, result_rule_release_id),
                execution_owner_id = CASE WHEN @release_ownership = 1 THEN NULL ELSE execution_owner_id END,
                execution_lease_expires_at = CASE WHEN @release_ownership = 1 THEN NULL ELSE execution_lease_expires_at END
            WHERE operation_id = @operation_id
              AND execution_owner_id = @execution_owner_id
              AND execution_lease_expires_at > SYSUTCDATETIME();
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
        command.Parameters.AddWithValue("@release_ownership", releaseOwnership);
        command.Parameters.AddWithValue("@execution_owner_id", executionOwnerId);
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new ManagedOperationOwnershipLostException(operationId);
        }
    }

    private async Task<int> RecoverExpiredAsync(
        SqlConnection connection,
        SqlTransaction? transaction,
        CancellationToken cancellationToken)
    {
        await using var command = Command(connection, transaction, """
            UPDATE {{schema}}.managed_operations
            SET operation_state = N'Interrupted',
                updated_at = SYSUTCDATETIME(),
                safe_status_detail = N'Execution ownership was lost before completion; durable retry will resume idempotent work.',
                error_code = N'MANAGED_OPERATION_INTERRUPTED',
                retry_safe = 1,
                administrative_intervention_required = 0,
                execution_owner_id = NULL,
                execution_lease_expires_at = NULL
            WHERE operation_state IN (N'Running', N'Cancelling')
              AND (
                    execution_owner_id IS NULL
                 OR execution_lease_expires_at IS NULL
                 OR execution_lease_expires_at <= SYSUTCDATETIME()
              );
            """);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static int LeaseMilliseconds(TimeSpan value) =>
        (int)Math.Clamp(
            (value > TimeSpan.Zero ? value : TimeSpan.FromSeconds(30)).TotalMilliseconds,
            1,
            int.MaxValue);

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
            reader.IsDBNull(17) ? null : reader.GetString(17),
            reader.GetInt32(18));

    private static string BindColumns(string sql) =>
        sql.Replace(
            "{{columns}}",
            "operation_id, operation_kind, correlation_id, operation_state, current_stage, " +
            "created_at, started_at, updated_at, completed_at, progress_percent, " +
            "safe_status_detail, error_code, retry_safe, user_approval_required, " +
            "administrative_intervention_required, ruleset_id, source_identity, result_rule_release_id, " +
            "execution_attempt_count",
            StringComparison.Ordinal);
}
