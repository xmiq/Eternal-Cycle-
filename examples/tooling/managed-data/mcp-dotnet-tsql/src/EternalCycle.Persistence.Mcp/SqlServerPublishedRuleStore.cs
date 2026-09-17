using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public sealed record RulePublicationWritePlan(
    int ChunkRows,
    int SelectorRows,
    int DependencyRows,
    int StagingCommandCount)
{
    public const int ChunkBatchSize = 100;
    public const int SelectorBatchSize = 300;
    public const int DependencyBatchSize = 500;

    public static RulePublicationWritePlan Create(CompiledRuleIndex index)
    {
        var selectors = index.Chunks.Sum(chunk =>
            DistinctCount(chunk.Metadata.WorldModelIds) +
            DistinctCount(chunk.Metadata.ModuleIds) +
            DistinctCount(chunk.Metadata.CampaignModes) +
            DistinctCount(chunk.Metadata.Operations) +
            DistinctCount(chunk.Metadata.Topics));
        var sourceCounts = index.Chunks
            .GroupBy(chunk => chunk.RuleSourceId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var dependencies = index.Chunks.Sum(chunk =>
            (chunk.Metadata.Dependencies ?? [])
                .Distinct(StringComparer.Ordinal)
                .Sum(sourceId => sourceCounts.GetValueOrDefault(sourceId)));
        return new(
            index.Chunks.Count,
            selectors,
            dependencies,
            1 + BatchCount(index.Chunks.Count, ChunkBatchSize) +
            BatchCount(selectors, SelectorBatchSize) +
            BatchCount(dependencies, DependencyBatchSize));
    }

    private static int DistinctCount(IEnumerable<string> values) =>
        values.Distinct(StringComparer.OrdinalIgnoreCase).Count();

    private static int BatchCount(int count, int size) =>
        count == 0 ? 0 : (count + size - 1) / size;
}

public sealed class SqlServerPublishedRuleStore(IOptions<SqlServerPersistenceOptions> options) : IPublishedRuleStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly SqlServerPersistenceOptions settings = options.Value;

    public async Task<PublishedRuleRelease?> GetActiveAsync(
        string rulesetId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateDomainCommand(connection, null, """
            SELECT
                releases.rule_release_id,
                releases.ruleset_id,
                releases.provider_kind,
                releases.source_identity,
                releases.repository_version,
                releases.compiler_version,
                releases.release_state,
                releases.compiled_index_json,
                releases.created_at,
                releases.failure_reason,
                releases.release_channel,
                releases.discovery_ref,
                releases.manifest_format_version,
                releases.compiler_contract_version
            FROM {{schema}}.active_rule_releases AS active
            INNER JOIN {{schema}}.rule_releases AS releases
                ON releases.ruleset_id = active.ruleset_id
               AND releases.rule_release_id = active.rule_release_id
            WHERE active.ruleset_id = @ruleset_id;
            """);
        AddParameter(command, "@ruleset_id", rulesetId);
        return await ReadReleaseAsync(command, cancellationToken);
    }

    public async Task<PublishedRuleRelease?> FindBySourceAsync(
        string rulesetId,
        string sourceIdentity,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateDomainCommand(connection, null, """
            SELECT TOP (1)
                rule_release_id,
                ruleset_id,
                provider_kind,
                source_identity,
                repository_version,
                compiler_version,
                release_state,
                compiled_index_json,
                created_at,
                failure_reason,
                release_channel,
                discovery_ref,
                manifest_format_version,
                compiler_contract_version
            FROM {{schema}}.rule_releases
            WHERE ruleset_id = @ruleset_id AND source_identity = @source_identity
            ORDER BY created_at DESC;
            """);
        AddParameter(command, "@ruleset_id", rulesetId);
        AddParameter(command, "@source_identity", sourceIdentity);
        return await ReadReleaseAsync(command, cancellationToken);
    }

    public async Task<PublishedRuleRelease?> GetByIdAsync(
        string rulesetId,
        string ruleReleaseId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateDomainCommand(connection, null, """
            SELECT
                rule_release_id,
                ruleset_id,
                provider_kind,
                source_identity,
                repository_version,
                compiler_version,
                release_state,
                compiled_index_json,
                created_at,
                failure_reason,
                release_channel,
                discovery_ref,
                manifest_format_version,
                compiler_contract_version
            FROM {{schema}}.rule_releases
            WHERE ruleset_id = @ruleset_id AND rule_release_id = @rule_release_id;
            """);
        AddParameter(command, "@ruleset_id", rulesetId);
        AddParameter(command, "@rule_release_id", ruleReleaseId);
        return await ReadReleaseAsync(command, cancellationToken);
    }

    public async Task<RuleUpdateCheck?> GetLatestUpdateCheckAsync(
        string rulesetId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateDomainCommand(connection, null, """
            SELECT TOP (1)
                ruleset_id,
                source_identity,
                outcome,
                sanitized_detail,
                checked_at
            FROM {{schema}}.rule_update_checks
            WHERE ruleset_id = @ruleset_id
            ORDER BY checked_at DESC, update_check_id DESC;
            """);
        AddParameter(command, "@ruleset_id", rulesetId);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new RuleUpdateCheck(
            reader.GetString(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.GetDateTimeOffset(4));
    }

    public async Task StageCandidateAsync(
        PublishedRuleRelease release,
        CancellationToken cancellationToken)
    {
        if (release.State != RuleReleaseState.Candidate)
        {
            throw new ArgumentException("A staged Rule Release must be a Candidate.", nameof(release));
        }

        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using (var releaseCommand = CreateDomainCommand(connection, transaction, """
            IF NOT EXISTS (SELECT 1 FROM {{schema}}.rulesets WHERE ruleset_id = @ruleset_id)
                INSERT INTO {{schema}}.rulesets (ruleset_id, display_name)
                VALUES (@ruleset_id, @ruleset_id);

            INSERT INTO {{schema}}.rule_releases (
                rule_release_id,
                ruleset_id,
                provider_kind,
                source_identity,
                repository_version,
                compiler_version,
                release_state,
                compiled_index_json,
                created_at,
                release_channel,
                discovery_ref,
                manifest_format_version,
                compiler_contract_version
            ) VALUES (
                @rule_release_id,
                @ruleset_id,
                @provider_kind,
                @source_identity,
                @repository_version,
                @compiler_version,
                N'Candidate',
                @compiled_index_json,
                @created_at,
                @release_channel,
                @discovery_ref,
                @manifest_format_version,
                @compiler_contract_version
            );
            """))
        {
            AddParameter(releaseCommand, "@rule_release_id", release.RuleReleaseId);
            AddParameter(releaseCommand, "@ruleset_id", release.RulesetId);
            AddParameter(releaseCommand, "@provider_kind", release.ProviderKind);
            AddParameter(releaseCommand, "@source_identity", release.SourceIdentity);
            AddParameter(releaseCommand, "@repository_version", release.RepositoryVersion);
            AddParameter(releaseCommand, "@compiler_version", release.CompilerVersion);
            AddParameter(releaseCommand, "@compiled_index_json", JsonSerializer.Serialize(release.Index, JsonOptions));
            AddParameter(releaseCommand, "@created_at", release.CreatedAt);
            AddParameter(releaseCommand, "@release_channel", release.ReleaseChannel.ToString());
            AddParameter(releaseCommand, "@discovery_ref", release.DiscoveryRef);
            AddParameter(releaseCommand, "@manifest_format_version", release.ManifestFormatVersion);
            AddParameter(releaseCommand, "@compiler_contract_version", release.CompilerContractVersion);
            await releaseCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await InsertChunkBatchesAsync(connection, transaction, release, cancellationToken);
        await InsertSelectorBatchesAsync(connection, transaction, release, cancellationToken);
        await InsertDependencyBatchesAsync(connection, transaction, release, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SetStateAsync(
        string ruleReleaseId,
        RuleReleaseState state,
        string? failureReason,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateDomainCommand(connection, null, """
            UPDATE {{schema}}.rule_releases
            SET
                release_state = @release_state,
                failure_reason = @failure_reason,
                validated_at = CASE WHEN @release_state = N'Validated' THEN SYSUTCDATETIME() ELSE validated_at END,
                published_at = CASE WHEN @release_state = N'Published' THEN SYSUTCDATETIME() ELSE published_at END
            WHERE rule_release_id = @rule_release_id;

            IF @@ROWCOUNT <> 1
                THROW 51020, 'Rule Release state target was not found.', 1;
            """);
        AddParameter(command, "@rule_release_id", ruleReleaseId);
        AddParameter(command, "@release_state", state.ToString());
        AddParameter(command, "@failure_reason", failureReason);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task ActivateAsync(
        string rulesetId,
        string ruleReleaseId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using var command = CreateDomainCommand(connection, transaction, """
            UPDATE {{schema}}.rule_releases
            SET release_state = N'Published'
            WHERE ruleset_id = @ruleset_id
              AND release_state = N'Active'
              AND rule_release_id <> @rule_release_id;

            UPDATE {{schema}}.rule_releases
            SET release_state = N'Active', activated_at = SYSUTCDATETIME(), failure_reason = NULL
            WHERE ruleset_id = @ruleset_id
              AND rule_release_id = @rule_release_id
              AND release_state IN (N'Published', N'Active');

            IF @@ROWCOUNT <> 1
                THROW 51021, 'Only a published Rule Release may be activated.', 1;

            MERGE {{schema}}.active_rule_releases AS target
            USING (SELECT @ruleset_id AS ruleset_id) AS source
            ON target.ruleset_id = source.ruleset_id
            WHEN MATCHED THEN
                UPDATE SET rule_release_id = @rule_release_id, activated_at = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (ruleset_id, rule_release_id, activated_at)
                VALUES (@ruleset_id, @rule_release_id, SYSUTCDATETIME());
            """);
        AddParameter(command, "@ruleset_id", rulesetId);
        AddParameter(command, "@rule_release_id", ruleReleaseId);
        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RecordUpdateCheckAsync(
        RuleUpdateCheck updateCheck,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateDomainCommand(connection, null, """
            IF NOT EXISTS (SELECT 1 FROM {{schema}}.rulesets WHERE ruleset_id = @ruleset_id)
                INSERT INTO {{schema}}.rulesets (ruleset_id, display_name)
                VALUES (@ruleset_id, @ruleset_id);

            INSERT INTO {{schema}}.rule_update_checks (
                update_check_id,
                ruleset_id,
                source_identity,
                outcome,
                sanitized_detail,
                checked_at
            ) VALUES (
                @update_check_id,
                @ruleset_id,
                @source_identity,
                @outcome,
                @sanitized_detail,
                @checked_at
            );
            """);
        AddParameter(command, "@update_check_id", $"CHECK-{Guid.NewGuid():N}");
        AddParameter(command, "@ruleset_id", updateCheck.RulesetId);
        AddParameter(command, "@source_identity", updateCheck.SourceIdentity);
        AddParameter(command, "@outcome", updateCheck.Outcome);
        AddParameter(command, "@sanitized_detail", updateCheck.SanitizedDetail);
        AddParameter(command, "@checked_at", updateCheck.CheckedAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task InsertChunkBatchesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        PublishedRuleRelease release,
        CancellationToken cancellationToken)
    {
        foreach (var batch in release.Index.Chunks.Chunk(RulePublicationWritePlan.ChunkBatchSize))
        {
            var sql = new StringBuilder("""
                INSERT INTO {{schema}}.rule_chunks (
                    rule_release_id, chunk_id, rule_source_id, source_path,
                    source_anchor, source_hash, rule_layer, priority,
                    always_include, estimated_tokens, content, metadata_json
                ) VALUES
                """);
            await using var command = CreateDomainCommand(connection, transaction, string.Empty);
            AddParameter(command, "@rule_release_id", release.RuleReleaseId);
            for (var index = 0; index < batch.Length; index++)
            {
                var chunk = batch[index];
                if (index > 0)
                {
                    sql.Append(',');
                }

                sql.Append($"""

                    (@rule_release_id, @chunk_id_{index}, @rule_source_id_{index}, @source_path_{index},
                     @source_anchor_{index}, @source_hash_{index}, @rule_layer_{index}, @priority_{index},
                     @always_include_{index}, @estimated_tokens_{index}, @content_{index}, @metadata_json_{index})
                    """);
                AddParameter(command, $"@chunk_id_{index}", chunk.ChunkId);
                AddParameter(command, $"@rule_source_id_{index}", chunk.RuleSourceId);
                AddParameter(command, $"@source_path_{index}", chunk.SourcePath);
                AddParameter(command, $"@source_anchor_{index}", chunk.SourceAnchor);
                AddParameter(command, $"@source_hash_{index}", chunk.SourceHash);
                AddParameter(command, $"@rule_layer_{index}", chunk.Metadata.Layer.ToString());
                AddParameter(command, $"@priority_{index}", chunk.Metadata.Priority);
                AddParameter(command, $"@always_include_{index}", chunk.Metadata.AlwaysInclude);
                AddParameter(command, $"@estimated_tokens_{index}", chunk.EstimatedTokens);
                AddParameter(command, $"@content_{index}", chunk.Content);
                AddParameter(command, $"@metadata_json_{index}", JsonSerializer.Serialize(chunk.Metadata, JsonOptions));
            }

            sql.Append(';');
            command.CommandText = SqlServerSchemaIdentifier.Bind(sql.ToString(), settings.DomainSchema);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task InsertSelectorBatchesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        PublishedRuleRelease release,
        CancellationToken cancellationToken)
    {
        var rows = release.Index.Chunks.SelectMany(chunk =>
            Selectors(chunk.ChunkId, "WorldModel", chunk.Metadata.WorldModelIds)
                .Concat(Selectors(chunk.ChunkId, "Module", chunk.Metadata.ModuleIds))
                .Concat(Selectors(chunk.ChunkId, "CampaignMode", chunk.Metadata.CampaignModes))
                .Concat(Selectors(chunk.ChunkId, "Operation", chunk.Metadata.Operations))
                .Concat(Selectors(chunk.ChunkId, "Topic", chunk.Metadata.Topics)))
            .ToArray();
        foreach (var batch in rows.Chunk(RulePublicationWritePlan.SelectorBatchSize))
        {
            var sql = new StringBuilder("""
                INSERT INTO {{schema}}.rule_chunk_selectors (
                    rule_release_id, chunk_id, selector_type, selector_value
                ) VALUES
                """);
            await using var command = CreateDomainCommand(connection, transaction, string.Empty);
            AddParameter(command, "@rule_release_id", release.RuleReleaseId);
            for (var index = 0; index < batch.Length; index++)
            {
                if (index > 0)
                {
                    sql.Append(',');
                }

                sql.Append($"\n(@rule_release_id, @chunk_id_{index}, @selector_type_{index}, @selector_value_{index})");
                AddParameter(command, $"@chunk_id_{index}", batch[index].ChunkId);
                AddParameter(command, $"@selector_type_{index}", batch[index].SelectorType);
                AddParameter(command, $"@selector_value_{index}", batch[index].Value);
            }

            sql.Append(';');
            command.CommandText = SqlServerSchemaIdentifier.Bind(sql.ToString(), settings.DomainSchema);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task InsertDependencyBatchesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        PublishedRuleRelease release,
        CancellationToken cancellationToken)
    {
        var chunksBySource = release.Index.Chunks
            .GroupBy(chunk => chunk.RuleSourceId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        var rows = release.Index.Chunks.SelectMany(sourceChunk =>
            (sourceChunk.Metadata.Dependencies ?? [])
                .Distinct(StringComparer.Ordinal)
                .SelectMany(dependencySourceId => chunksBySource.GetValueOrDefault(dependencySourceId) ?? [])
                .Select(requiredChunk => new DependencyRow(sourceChunk.ChunkId, requiredChunk.ChunkId)))
            .ToArray();
        foreach (var batch in rows.Chunk(RulePublicationWritePlan.DependencyBatchSize))
        {
            var sql = new StringBuilder("""
                INSERT INTO {{schema}}.rule_dependencies (
                    rule_release_id, source_chunk_id, required_chunk_id, dependency_reason
                ) VALUES
                """);
            await using var command = CreateDomainCommand(connection, transaction, string.Empty);
            AddParameter(command, "@rule_release_id", release.RuleReleaseId);
            for (var index = 0; index < batch.Length; index++)
            {
                if (index > 0)
                {
                    sql.Append(',');
                }

                sql.Append($"\n(@rule_release_id, @source_chunk_id_{index}, @required_chunk_id_{index}, N'Manifest dependency')");
                AddParameter(command, $"@source_chunk_id_{index}", batch[index].SourceChunkId);
                AddParameter(command, $"@required_chunk_id_{index}", batch[index].RequiredChunkId);
            }

            sql.Append(';');
            command.CommandText = SqlServerSchemaIdentifier.Bind(sql.ToString(), settings.DomainSchema);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static IEnumerable<SelectorRow> Selectors(
        string chunkId,
        string selectorType,
        IEnumerable<string> values) =>
        values
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(value => new SelectorRow(chunkId, selectorType, value));

    private sealed record SelectorRow(string ChunkId, string SelectorType, string Value);

    private sealed record DependencyRow(string SourceChunkId, string RequiredChunkId);

    private async Task<PublishedRuleRelease?> ReadReleaseAsync(
        SqlCommand command,
        CancellationToken cancellationToken)
    {
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var index = JsonSerializer.Deserialize<CompiledRuleIndex>(reader.GetString(7), JsonOptions)
            ?? throw new InvalidOperationException("Published Rule Release contains an unreadable compiled index.");
        return new PublishedRuleRelease(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            Enum.Parse<RuleReleaseState>(reader.GetString(6), ignoreCase: false),
            index,
            reader.GetDateTimeOffset(8),
            reader.IsDBNull(9) ? null : reader.GetString(9),
            Enum.Parse<RuleSourceReleaseChannel>(reader.GetString(10), ignoreCase: true),
            reader.IsDBNull(11) ? null : reader.GetString(11),
            reader.IsDBNull(12) ? null : reader.GetInt32(12),
            reader.IsDBNull(13) ? null : reader.GetString(13));
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private SqlCommand CreateDomainCommand(
        SqlConnection connection,
        SqlTransaction? transaction,
        string text) =>
        new(
            SqlServerSchemaIdentifier.Bind(text, settings.DomainSchema),
            connection,
            transaction)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };

    private static void AddParameter(SqlCommand command, string name, object? value) =>
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
}
