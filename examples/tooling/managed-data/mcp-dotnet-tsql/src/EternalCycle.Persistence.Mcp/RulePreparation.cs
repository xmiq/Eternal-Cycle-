using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public enum RulePreparationTier
{
    RuntimeKernel = 0,
    CampaignBootstrap = 1,
    ImmediateGameplayCore = 2,
    CampaignRelevant = 3,
    Standard = 4,
    OptionalRare = 5
}

public enum RulePreparationState
{
    Pending,
    Ready,
    Failed
}

public sealed record RuleSourcePreparation(
    string RuleSourceId,
    RulePreparationTier Tier,
    RulePreparationState State,
    int PriorityBoost,
    string? FailureCode = null,
    int BasePriority = 0);

public sealed record RuleReleasePreparation(
    string RuleReleaseId,
    IReadOnlyList<RuleSourcePreparation> Sources)
{
    public bool RuleKernelReady => Sources
        .Where(source => source.Tier == RulePreparationTier.RuntimeKernel)
        .All(source => source.State == RulePreparationState.Ready) &&
        Sources.Any(source => source.Tier == RulePreparationTier.RuntimeKernel);

    public bool CampaignBootstrapReady => RuleKernelReady && Sources
        .Where(source => source.Tier <= RulePreparationTier.ImmediateGameplayCore)
        .All(source => source.State == RulePreparationState.Ready);

    public bool FullRulesetReady => Sources.Count > 0 &&
        Sources.All(source => source.State == RulePreparationState.Ready);
}

public interface IRulePreparationStore
{
    Task InitializeAsync(PublishedRuleRelease release, CancellationToken cancellationToken);

    Task<RuleReleasePreparation> GetAsync(
        string ruleReleaseId,
        CancellationToken cancellationToken);

    Task<RuleSourcePreparation?> GetNextPendingAsync(
        string ruleReleaseId,
        CancellationToken cancellationToken);

    Task MarkReadyAsync(
        string ruleReleaseId,
        IReadOnlyCollection<string> ruleSourceIds,
        CancellationToken cancellationToken);

    Task RaisePriorityAsync(
        string ruleReleaseId,
        IReadOnlyCollection<string> ruleSourceIds,
        CancellationToken cancellationToken);
}

public sealed class SqlServerRulePreparationStore(
    IOptions<SqlServerPersistenceOptions> options) : IRulePreparationStore
{
    private readonly SqlServerPersistenceOptions settings = options.Value;

    public async Task InitializeAsync(
        PublishedRuleRelease release,
        CancellationToken cancellationToken)
    {
        var sources = release.Index.Chunks
            .GroupBy(chunk => chunk.RuleSourceId, StringComparer.Ordinal)
            .Select(group => new
            {
                RuleSourceId = group.Key,
                Tier = group.Min(chunk => chunk.Metadata.PreparationTier),
                BasePriority = group.Max(chunk => chunk.Metadata.Priority)
            })
            .OrderBy(source => source.Tier)
            .ThenBy(source => source.RuleSourceId, StringComparer.Ordinal)
            .ToArray();
        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        foreach (var source in sources)
        {
            await using var command = Command(connection, transaction, """
                IF NOT EXISTS (
                    SELECT 1
                    FROM {{schema}}.rule_source_preparation
                    WHERE rule_release_id = @rule_release_id
                      AND rule_source_id = @rule_source_id
                )
                BEGIN
                    INSERT INTO {{schema}}.rule_source_preparation (
                        rule_release_id, rule_source_id, preparation_tier,
                        preparation_state, base_priority, priority_boost, updated_at
                    ) VALUES (
                        @rule_release_id, @rule_source_id, @preparation_tier,
                        N'Pending', @base_priority, 0, SYSUTCDATETIME()
                    );
                END;
                """);
            command.Parameters.AddWithValue("@rule_release_id", release.RuleReleaseId);
            command.Parameters.AddWithValue("@rule_source_id", source.RuleSourceId);
            command.Parameters.AddWithValue("@preparation_tier", source.Tier.ToString());
            command.Parameters.AddWithValue("@base_priority", source.BasePriority);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<RuleReleasePreparation> GetAsync(
        string ruleReleaseId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, null, """
            SELECT rule_source_id, preparation_tier, preparation_state,
                   priority_boost, failure_code, base_priority
            FROM {{schema}}.rule_source_preparation
            WHERE rule_release_id = @rule_release_id
            ORDER BY
                CASE preparation_tier
                    WHEN N'RuntimeKernel' THEN 0
                    WHEN N'CampaignBootstrap' THEN 1
                    WHEN N'ImmediateGameplayCore' THEN 2
                    WHEN N'CampaignRelevant' THEN 3
                    WHEN N'Standard' THEN 4
                    ELSE 5
                END,
                priority_boost DESC,
                base_priority DESC,
                rule_source_id;
            """);
        command.Parameters.AddWithValue("@rule_release_id", ruleReleaseId);
        var sources = new List<RuleSourcePreparation>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sources.Add(new RuleSourcePreparation(
                reader.GetString(0),
                Enum.Parse<RulePreparationTier>(reader.GetString(1), ignoreCase: false),
                Enum.Parse<RulePreparationState>(reader.GetString(2), ignoreCase: false),
                reader.GetInt32(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(5)));
        }

        return new RuleReleasePreparation(ruleReleaseId, sources);
    }

    public async Task<RuleSourcePreparation?> GetNextPendingAsync(
        string ruleReleaseId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, null, """
            SELECT TOP (1) rule_source_id, preparation_tier, preparation_state,
                           priority_boost, failure_code, base_priority
            FROM {{schema}}.rule_source_preparation WITH (UPDLOCK, READPAST)
            WHERE rule_release_id = @rule_release_id
              AND preparation_state = N'Pending'
            ORDER BY
                priority_boost DESC,
                CASE preparation_tier
                    WHEN N'RuntimeKernel' THEN 0
                    WHEN N'CampaignBootstrap' THEN 1
                    WHEN N'ImmediateGameplayCore' THEN 2
                    WHEN N'CampaignRelevant' THEN 3
                    WHEN N'Standard' THEN 4
                    ELSE 5
                END,
                base_priority DESC,
                rule_source_id;
            """);
        command.Parameters.AddWithValue("@rule_release_id", ruleReleaseId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new RuleSourcePreparation(
            reader.GetString(0),
            Enum.Parse<RulePreparationTier>(reader.GetString(1), ignoreCase: false),
            Enum.Parse<RulePreparationState>(reader.GetString(2), ignoreCase: false),
            reader.GetInt32(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.GetInt32(5));
    }

    public Task MarkReadyAsync(
        string ruleReleaseId,
        IReadOnlyCollection<string> ruleSourceIds,
        CancellationToken cancellationToken) =>
        UpdateSourcesAsync(
            ruleReleaseId,
            ruleSourceIds,
            "preparation_state = N'Ready', failure_code = NULL",
            cancellationToken);

    public Task RaisePriorityAsync(
        string ruleReleaseId,
        IReadOnlyCollection<string> ruleSourceIds,
        CancellationToken cancellationToken) =>
        UpdateSourcesAsync(
            ruleReleaseId,
            ruleSourceIds,
            "priority_boost = CASE WHEN priority_boost < 1000000 THEN priority_boost + 1000 ELSE priority_boost END",
            cancellationToken);

    private async Task UpdateSourcesAsync(
        string ruleReleaseId,
        IReadOnlyCollection<string> ruleSourceIds,
        string update,
        CancellationToken cancellationToken)
    {
        if (ruleSourceIds.Count == 0)
        {
            return;
        }

        await using var connection = await OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        foreach (var ruleSourceId in ruleSourceIds.Distinct(StringComparer.Ordinal))
        {
            var sql = """
                UPDATE {{schema}}.rule_source_preparation
                SET {{update}}, updated_at = SYSUTCDATETIME()
                WHERE rule_release_id = @rule_release_id
                  AND rule_source_id = @rule_source_id;
                """.Replace("{{update}}", update, StringComparison.Ordinal);
            await using var command = Command(connection, transaction, sql);
            command.Parameters.AddWithValue("@rule_release_id", ruleReleaseId);
            command.Parameters.AddWithValue("@rule_source_id", ruleSourceId);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
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
}

public sealed class ImmediateRulePreparationStore : IRulePreparationStore
{
    private readonly Dictionary<string, RuleReleasePreparation> releases = new(StringComparer.Ordinal);

    public Task InitializeAsync(PublishedRuleRelease release, CancellationToken cancellationToken)
    {
        releases[release.RuleReleaseId] = new RuleReleasePreparation(
            release.RuleReleaseId,
            release.Index.Chunks
                .GroupBy(chunk => chunk.RuleSourceId, StringComparer.Ordinal)
                .Select(group => new RuleSourcePreparation(
                    group.Key,
                    group.Min(chunk => chunk.Metadata.PreparationTier),
                    RulePreparationState.Ready,
                    0,
                    BasePriority: group.Max(chunk => chunk.Metadata.Priority)))
                .ToArray());
        return Task.CompletedTask;
    }

    public Task<RuleReleasePreparation> GetAsync(string ruleReleaseId, CancellationToken cancellationToken) =>
        Task.FromResult(releases.GetValueOrDefault(ruleReleaseId) ?? new RuleReleasePreparation(ruleReleaseId, []));

    public Task<RuleSourcePreparation?> GetNextPendingAsync(
        string ruleReleaseId,
        CancellationToken cancellationToken) =>
        Task.FromResult<RuleSourcePreparation?>(null);

    public Task MarkReadyAsync(
        string ruleReleaseId,
        IReadOnlyCollection<string> ruleSourceIds,
        CancellationToken cancellationToken) => Task.CompletedTask;

    public Task RaisePriorityAsync(
        string ruleReleaseId,
        IReadOnlyCollection<string> ruleSourceIds,
        CancellationToken cancellationToken) => Task.CompletedTask;
}

public sealed class RuleContextPendingException(IReadOnlyList<string> pendingRuleSourceIds)
    : ManagedServiceException(
        "RULE_CONTEXT_PENDING",
        "The requested authoritative rule closure is still being prepared and has been raised in priority.")
{
    public IReadOnlyList<string> PendingRuleSourceIds { get; } = pendingRuleSourceIds;
}
