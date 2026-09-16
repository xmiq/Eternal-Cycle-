using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

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
                releases.failure_reason
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
                failure_reason
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
                failure_reason
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
                created_at
            ) VALUES (
                @rule_release_id,
                @ruleset_id,
                @provider_kind,
                @source_identity,
                @repository_version,
                @compiler_version,
                N'Candidate',
                @compiled_index_json,
                @created_at
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
            await releaseCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var chunk in release.Index.Chunks)
        {
            await using (var chunkCommand = CreateDomainCommand(connection, transaction, """
                INSERT INTO {{schema}}.rule_chunks (
                    rule_release_id,
                    chunk_id,
                    rule_source_id,
                    source_path,
                    source_anchor,
                    source_hash,
                    rule_layer,
                    priority,
                    always_include,
                    estimated_tokens,
                    content,
                    metadata_json
                ) VALUES (
                    @rule_release_id,
                    @chunk_id,
                    @rule_source_id,
                    @source_path,
                    @source_anchor,
                    @source_hash,
                    @rule_layer,
                    @priority,
                    @always_include,
                    @estimated_tokens,
                    @content,
                    @metadata_json
                );
                """))
            {
                AddParameter(chunkCommand, "@rule_release_id", release.RuleReleaseId);
                AddParameter(chunkCommand, "@chunk_id", chunk.ChunkId);
                AddParameter(chunkCommand, "@rule_source_id", chunk.RuleSourceId);
                AddParameter(chunkCommand, "@source_path", chunk.SourcePath);
                AddParameter(chunkCommand, "@source_anchor", chunk.SourceAnchor);
                AddParameter(chunkCommand, "@source_hash", chunk.SourceHash);
                AddParameter(chunkCommand, "@rule_layer", chunk.Metadata.Layer.ToString());
                AddParameter(chunkCommand, "@priority", chunk.Metadata.Priority);
                AddParameter(chunkCommand, "@always_include", chunk.Metadata.AlwaysInclude);
                AddParameter(chunkCommand, "@estimated_tokens", chunk.EstimatedTokens);
                AddParameter(chunkCommand, "@content", chunk.Content);
                AddParameter(chunkCommand, "@metadata_json", JsonSerializer.Serialize(chunk.Metadata, JsonOptions));
                await chunkCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await InsertSelectorsAsync(
                connection,
                transaction,
                release.RuleReleaseId,
                chunk.ChunkId,
                "WorldModel",
                chunk.Metadata.WorldModelIds,
                cancellationToken);
            await InsertSelectorsAsync(connection, transaction, release.RuleReleaseId, chunk.ChunkId, "Module", chunk.Metadata.ModuleIds, cancellationToken);
            await InsertSelectorsAsync(connection, transaction, release.RuleReleaseId, chunk.ChunkId, "CampaignMode", chunk.Metadata.CampaignModes, cancellationToken);
            await InsertSelectorsAsync(connection, transaction, release.RuleReleaseId, chunk.ChunkId, "Operation", chunk.Metadata.Operations, cancellationToken);
            await InsertSelectorsAsync(connection, transaction, release.RuleReleaseId, chunk.ChunkId, "Topic", chunk.Metadata.Topics, cancellationToken);
        }

        foreach (var sourceChunk in release.Index.Chunks)
        {
            foreach (var dependencySourceId in sourceChunk.Metadata.Dependencies ?? [])
            {
                foreach (var requiredChunk in release.Index.Chunks.Where(candidate =>
                    string.Equals(candidate.RuleSourceId, dependencySourceId, StringComparison.Ordinal)))
                {
                    await InsertDependencyAsync(
                        connection,
                        transaction,
                        release.RuleReleaseId,
                        sourceChunk.ChunkId,
                        requiredChunk.ChunkId,
                        cancellationToken);
                }
            }
        }

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

    private async Task InsertDependencyAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string ruleReleaseId,
        string sourceChunkId,
        string requiredChunkId,
        CancellationToken cancellationToken)
    {
        await using var command = CreateDomainCommand(connection, transaction, """
            INSERT INTO {{schema}}.rule_dependencies (
                rule_release_id,
                source_chunk_id,
                required_chunk_id,
                dependency_reason
            ) VALUES (
                @rule_release_id,
                @source_chunk_id,
                @required_chunk_id,
                N'Manifest dependency'
            );
            """);
        AddParameter(command, "@rule_release_id", ruleReleaseId);
        AddParameter(command, "@source_chunk_id", sourceChunkId);
        AddParameter(command, "@required_chunk_id", requiredChunkId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task InsertSelectorsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string ruleReleaseId,
        string chunkId,
        string selectorType,
        IReadOnlyList<string> values,
        CancellationToken cancellationToken)
    {
        foreach (var value in values.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            await using var command = CreateDomainCommand(connection, transaction, """
                INSERT INTO {{schema}}.rule_chunk_selectors (
                    rule_release_id,
                    chunk_id,
                    selector_type,
                    selector_value
                ) VALUES (
                    @rule_release_id,
                    @chunk_id,
                    @selector_type,
                    @selector_value
                );
                """);
            AddParameter(command, "@rule_release_id", ruleReleaseId);
            AddParameter(command, "@chunk_id", chunkId);
            AddParameter(command, "@selector_type", selectorType);
            AddParameter(command, "@selector_value", value);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

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
            reader.IsDBNull(9) ? null : reader.GetString(9));
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
