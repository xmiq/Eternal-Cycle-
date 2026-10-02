using System.Data;
using System.Text;
using System.Text.Json.Nodes;
using EternalCycle.Rules;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

internal enum ArtifactImportStage { Header, Sources, SnippetsAndRetrieval, Dependencies, Verified }

public sealed class SqlServerCompiledRulesArtifactStore : ICompiledRulesArtifactImportStore
{
    internal static readonly string[] Tables = ["imported_rule_artifacts", "imported_rule_sources", "imported_rule_snippets", "imported_rule_dependencies"];
    private readonly SqlServerPersistenceOptions settings;
    private readonly string authorizedRulesetId;
    private readonly Action<ArtifactImportStage>? afterStage;

    public SqlServerCompiledRulesArtifactStore(IOptions<SqlServerPersistenceOptions> options, string authorizedRulesetId)
        : this(options, authorizedRulesetId, null) { }

    // Deterministic transaction-failure/cancellation injection is test-only.
    internal SqlServerCompiledRulesArtifactStore(IOptions<SqlServerPersistenceOptions> options, string authorizedRulesetId, Action<ArtifactImportStage>? afterStage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authorizedRulesetId);
        settings = options.Value;
        SqlServerSchemaIdentifier.Validate(settings.DomainSchema);
        this.authorizedRulesetId = authorizedRulesetId;
        this.afterStage = afterStage;
    }

    public async Task<CompiledRulesImportReceipt> ImportAsync(ValidatedTrustedCompiledRulesArtifact approved, CancellationToken cancellationToken)
    {
        CompiledRulesArtifactImport.RequireScope(approved, authorizedRulesetId);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var artifact = approved.Artifact;
            var id = CompiledRulesArtifactImport.CreateImportId(artifact);
            var written = CompiledRulesArtifactWriter.Write(artifact);
            if (!written.IsValid) throw new CompiledRulesImportException(CompiledRulesImportFailure.InputInvalid);
            var json = Encoding.UTF8.GetString(written.Bytes.Span);
            await using var connection = new SqlConnection(settings.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            // Range-lock the deterministic primary key before insertion. Competing
            // equal imports wait and then verify/reuse, never leak a unique crash.
            await using (var lookup = Command(connection, transaction, """
                SELECT import_id FROM {{schema}}.imported_rule_artifacts WITH (UPDLOCK, HOLDLOCK)
                WHERE import_id = @id;
                """))
            {
                AddId(lookup, id);
                if (await lookup.ExecuteScalarAsync(cancellationToken) is not null)
                {
                    var stored = await ReadCoreAsync(connection, transaction, id, cancellationToken)
                        ?? throw new CompiledRulesImportException(CompiledRulesImportFailure.IntegrityConflict);
                    if (!CompiledRulesArtifactImport.Equivalent(artifact, stored.Artifact))
                        throw new CompiledRulesImportException(CompiledRulesImportFailure.IntegrityConflict);
                    await transaction.CommitAsync(cancellationToken);
                    return CompiledRulesArtifactImport.Receipt(stored, false);
                }
            }

            await ExecuteAsync(connection, transaction, """
                INSERT INTO {{schema}}.imported_rule_artifacts
                    (import_id, ruleset_id, semantic_sha256, byte_sha256, exact_bytes, header_json)
                VALUES (@id, @ruleset, @semantic, @byte_hash, @bytes,
                    JSON_MODIFY(JSON_MODIFY(@json, '$.ruleSources', JSON_QUERY(N'[]')), '$.snippets', JSON_QUERY(N'[]')));
                """, id, json, cancellationToken, command =>
                {
                    command.Parameters.AddWithValue("@ruleset", artifact.Ruleset.RulesetId);
                    command.Parameters.AddWithValue("@semantic", approved.SemanticSha256);
                    command.Parameters.AddWithValue("@byte_hash", approved.ByteSha256);
                    command.Parameters.Add("@bytes", SqlDbType.VarBinary, -1).Value = approved.CopyBytes();
                });
            Completed(ArtifactImportStage.Header, cancellationToken);
            await ExecuteAsync(connection, transaction, """
                INSERT INTO {{schema}}.imported_rule_sources (import_id, source_ordinal, rule_source_id, source_json)
                SELECT @id, CONVERT(int, items.[key]), fields.rule_source_id,
                    JSON_MODIFY(items.value, '$.dependencyRuleSourceIds', JSON_QUERY(N'[]'))
                FROM OPENJSON(@json, '$.ruleSources') AS items
                CROSS APPLY OPENJSON(items.value) WITH (rule_source_id nvarchar(128) '$.ruleSourceId') AS fields;
                """, id, json, cancellationToken);
            Completed(ArtifactImportStage.Sources, cancellationToken);
            await ExecuteAsync(connection, transaction, """
                INSERT INTO {{schema}}.imported_rule_snippets (import_id, snippet_ordinal, source_ordinal, snippet_json)
                SELECT @id, CONVERT(int, items.[key]), sources.source_ordinal, items.value
                FROM OPENJSON(@json, '$.snippets') AS items
                CROSS APPLY OPENJSON(items.value) WITH (rule_source_id nvarchar(128) '$.ruleSourceId') AS fields
                INNER JOIN {{schema}}.imported_rule_sources AS sources
                    ON sources.import_id = @id
                    AND sources.rule_source_id = fields.rule_source_id COLLATE Latin1_General_100_BIN2;
                """, id, json, cancellationToken);
            Completed(ArtifactImportStage.SnippetsAndRetrieval, cancellationToken);
            await ExecuteAsync(connection, transaction, """
                INSERT INTO {{schema}}.imported_rule_dependencies
                    (import_id, source_ordinal, dependency_ordinal, target_source_ordinal)
                SELECT @id, CONVERT(int, items.[key]), CONVERT(int, dependencies.[key]), targets.source_ordinal
                FROM OPENJSON(@json, '$.ruleSources') AS items
                CROSS APPLY OPENJSON(items.value, '$.dependencyRuleSourceIds') AS dependencies
                INNER JOIN {{schema}}.imported_rule_sources AS targets
                    ON targets.import_id = @id
                    AND targets.rule_source_id = dependencies.value COLLATE Latin1_General_100_BIN2;
                """, id, json, cancellationToken);
            Completed(ArtifactImportStage.Dependencies, cancellationToken);
            var verified = await ReadCoreAsync(connection, transaction, id, cancellationToken)
                ?? throw new CompiledRulesImportException(CompiledRulesImportFailure.IntegrityConflict);
            if (!CompiledRulesArtifactImport.Equivalent(artifact, verified.Artifact))
                throw new CompiledRulesImportException(CompiledRulesImportFailure.IntegrityConflict);
            Completed(ArtifactImportStage.Verified, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return CompiledRulesArtifactImport.Receipt(verified, true);
        }
        catch (CompiledRulesImportException) { throw; }
        catch (OperationCanceledException) { throw new OperationCanceledException("Artifact import was cancelled.", cancellationToken); }
        catch (SqlException error) { throw SafeSqlFailure(error); }
        catch (Exception error) when (error is not OutOfMemoryException)
        { throw new CompiledRulesImportException(CompiledRulesImportFailure.StorageFailed); }
    }

    public async Task<StoredCompiledRulesArtifact?> ReadAsync(string importId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importId);
        if (importId.Length != 73 || !importId.StartsWith("ARTIFACT-", StringComparison.Ordinal) ||
            importId.AsSpan(9).ContainsAnyExcept("0123456789ABCDEF"))
            throw new CompiledRulesImportException(CompiledRulesImportFailure.InputInvalid);
        try
        {
            await using var connection = new SqlConnection(settings.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            // A consistent read sees either the committed whole import or none.
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var stored = await ReadCoreAsync(connection, transaction, importId, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return stored;
        }
        catch (CompiledRulesImportException) { throw; }
        catch (OperationCanceledException) { throw new OperationCanceledException("Artifact readback was cancelled.", cancellationToken); }
        catch (SqlException error) { throw SafeSqlFailure(error); }
        catch (Exception error) when (error is not OutOfMemoryException)
        { throw new CompiledRulesImportException(CompiledRulesImportFailure.StorageFailed); }
    }

    private async Task<StoredCompiledRulesArtifact?> ReadCoreAsync(SqlConnection connection, SqlTransaction transaction, string id, CancellationToken token)
    {
        JsonObject header;
        byte[] exactBytes;
        string byteHash, ruleset, semantic;
        await using (var command = Command(connection, transaction, """
            SELECT header_json, exact_bytes, byte_sha256, ruleset_id, semantic_sha256
            FROM {{schema}}.imported_rule_artifacts WHERE import_id = @id;
            """))
        {
            AddId(command, id);
            await using var reader = await command.ExecuteReaderAsync(token);
            if (!await reader.ReadAsync(token)) return null;
            header = JsonNode.Parse(reader.GetString(0))!.AsObject();
            exactBytes = (byte[])reader.GetValue(1);
            byteHash = reader.GetString(2);
            ruleset = reader.GetString(3);
            semantic = reader.GetString(4);
        }
        if (ruleset != authorizedRulesetId)
            throw new CompiledRulesImportException(CompiledRulesImportFailure.InputInvalid);
        var sources = new JsonArray();
        var sourceIds = new List<string>();
        await using (var command = Command(connection, transaction, """
            SELECT source_ordinal, rule_source_id, source_json FROM {{schema}}.imported_rule_sources
            WHERE import_id = @id ORDER BY source_ordinal;
            """))
        {
            AddId(command, id);
            await using var reader = await command.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token))
            {
                RequireOrdinal(reader.GetInt32(0), sources.Count);
                var source = JsonNode.Parse(reader.GetString(2))!.AsObject();
                if (source["ruleSourceId"]!.GetValue<string>() != reader.GetString(1)) Conflict();
                sourceIds.Add(reader.GetString(1));
                sources.Add(source);
            }
        }
        await using (var command = Command(connection, transaction, """
            SELECT source_ordinal, dependency_ordinal, target_source_ordinal FROM {{schema}}.imported_rule_dependencies
            WHERE import_id = @id ORDER BY source_ordinal, dependency_ordinal;
            """))
        {
            AddId(command, id);
            await using var reader = await command.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token))
            {
                var source = reader.GetInt32(0);
                var target = reader.GetInt32(2);
                if (source < 0 || source >= sources.Count || target < 0 || target >= sources.Count) Conflict();
                var dependencies = sources[source]!["dependencyRuleSourceIds"]!.AsArray();
                RequireOrdinal(reader.GetInt32(1), dependencies.Count);
                dependencies.Add(sourceIds[target]);
            }
        }
        var snippets = new JsonArray();
        await using (var command = Command(connection, transaction, """
            SELECT snippet_ordinal, source_ordinal, snippet_json FROM {{schema}}.imported_rule_snippets
            WHERE import_id = @id ORDER BY snippet_ordinal;
            """))
        {
            AddId(command, id);
            await using var reader = await command.ExecuteReaderAsync(token);
            while (await reader.ReadAsync(token))
            {
                RequireOrdinal(reader.GetInt32(0), snippets.Count);
                var owner = reader.GetInt32(1);
                var snippet = JsonNode.Parse(reader.GetString(2))!.AsObject();
                if (owner < 0 || owner >= sourceIds.Count || snippet["ruleSourceId"]!.GetValue<string>() != sourceIds[owner]) Conflict();
                snippets.Add(snippet);
            }
        }
        header["ruleSources"] = sources;
        header["snippets"] = snippets;
        var verified = CompiledRulesArtifactImport.VerifyReadback(id, header.ToJsonString(), exactBytes, byteHash);
        if (verified.SemanticSha256 != semantic || verified.Artifact.Ruleset.RulesetId != ruleset) Conflict();
        return verified;
    }

    private void Completed(ArtifactImportStage stage, CancellationToken token)
    {
        afterStage?.Invoke(stage);
        token.ThrowIfCancellationRequested();
    }

    private async Task ExecuteAsync(SqlConnection connection, SqlTransaction transaction, string sql, string id, string json, CancellationToken token, Action<SqlCommand>? configure = null)
    {
        await using var command = Command(connection, transaction, sql);
        AddId(command, id);
        command.Parameters.Add("@json", SqlDbType.NVarChar, -1).Value = json;
        configure?.Invoke(command);
        await command.ExecuteNonQueryAsync(token);
    }

    private SqlCommand Command(SqlConnection connection, SqlTransaction transaction, string sql) => new(
        SqlServerSchemaIdentifier.Bind(sql, settings.DomainSchema), connection, transaction)
    { CommandTimeout = settings.CommandTimeoutSeconds };

    private static void RequireOrdinal(int actual, int expected) { if (actual != expected) Conflict(); }
    // Match the indexed SQL key type rather than forcing an implicit conversion.
    private static void AddId(SqlCommand command, string id) => command.Parameters.Add("@id", SqlDbType.VarChar, 73).Value = id;
    private static void Conflict() => throw new CompiledRulesImportException(CompiledRulesImportFailure.IntegrityConflict);
    private static CompiledRulesImportException SafeSqlFailure(SqlException error) => new(error.Number is 207 or 208
        ? CompiledRulesImportFailure.SchemaIncompatible : CompiledRulesImportFailure.StorageFailed);
}
