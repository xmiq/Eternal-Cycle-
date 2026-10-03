using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace EternalCycle.Persistence.Mcp;

public sealed partial class SqlServerCompiledRulesArtifactStore
{
    internal static readonly string[] PublicationTables = ["published_rule_artifacts", "active_rule_artifacts"];

    internal Task<StoredCompiledRulesArtifact> ReadRuntimeAsync(CompiledRuleStoreScope scope, CancellationToken token) =>
        AccessScopeAsync(scope, requireActive: true, publication: null, token);

    // Internal primitives: the service checks administrative consent separately
    // from the configured runtime read grant before touching SQL.
    internal Task<StoredCompiledRulesArtifact> PublishAsync(CompiledRuleStoreScope scope, CancellationToken token) =>
        AccessScopeAsync(scope, requireActive: false, publication: false, token);

    internal Task<StoredCompiledRulesArtifact> ActivateAsync(CompiledRuleStoreScope scope, CancellationToken token) =>
        AccessScopeAsync(scope, requireActive: false, publication: true, token);

    private async Task<StoredCompiledRulesArtifact> AccessScopeAsync(
        CompiledRuleStoreScope scope, bool requireActive, bool? publication, CancellationToken token)
    {
        if (scope.RulesetId != authorizedRulesetId)
            throw new ManagedServiceException("RULE_RETRIEVAL_UNAUTHORIZED", "Compiled rule access is not authorized.");
        token.ThrowIfCancellationRequested();
        try
        {
            await using var connection = new SqlConnection(settings.ConnectionString);
            await connection.OpenAsync(token);
            // Keep selection, publication/activation and complete projection in
            // one coherent read; concurrent activation cannot mix artifacts.
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, token);
            string id;
            await using (var lookup = Command(connection, transaction, publication is null ? """
                SELECT import_id FROM {{schema}}.imported_rule_artifacts
                WHERE ruleset_id = @ruleset AND semantic_sha256 = @semantic;
                """ : """
                SELECT import_id FROM {{schema}}.imported_rule_artifacts WITH (UPDLOCK, HOLDLOCK)
                WHERE ruleset_id = @ruleset AND semantic_sha256 = @semantic;
                """))
            {
                AddScope(lookup, scope);
                id = await lookup.ExecuteScalarAsync(token) as string
                    ?? throw new CompiledRuleRetrievalException(CompiledRuleRetrievalFailure.ArtifactUnavailable);
            }
            if (requireActive || publication == true)
            {
                await using var eligibility = Command(connection, transaction, """
                    SELECT CASE WHEN EXISTS (
                        SELECT 1 FROM {{schema}}.published_rule_artifacts
                        WHERE ruleset_id = @ruleset AND import_id = @id) THEN 1 ELSE 0 END,
                        CASE WHEN EXISTS (
                        SELECT 1 FROM {{schema}}.active_rule_artifacts
                        WHERE ruleset_id = @ruleset AND import_id = @id) THEN 1 ELSE 0 END;
                    """);
                AddScope(eligibility, scope);
                AddId(eligibility, id);
                await using var reader = await eligibility.ExecuteReaderAsync(token);
                await reader.ReadAsync(token);
                if (reader.GetInt32(0) != 1)
                    throw new ManagedServiceException("RULE_PUBLICATION_REQUIRED", "The selected compiled artifact is not published.");
                if (requireActive && reader.GetInt32(1) != 1)
                    throw new ManagedServiceException("RULE_ACTIVATION_REQUIRED", "The selected compiled artifact is not active.");
            }
            // FR-025 compares every normative field against exact approved
            // byte/hash evidence. No SQL-specific matcher or artifact model.
            var stored = await ReadCoreAsync(connection, transaction, id, token)
                ?? throw new CompiledRuleRetrievalException(CompiledRuleRetrievalFailure.ArtifactInconsistent);
            if (stored.Artifact.Ruleset.RulesetId != scope.RulesetId || stored.SemanticSha256 != scope.SemanticSha256)
                throw new CompiledRuleRetrievalException(CompiledRuleRetrievalFailure.ArtifactInconsistent);
            if (publication is not null)
            {
                await using var command = Command(connection, transaction, publication.Value ? """
                    UPDATE {{schema}}.active_rule_artifacts WITH (UPDLOCK, HOLDLOCK)
                    SET import_id = @id WHERE ruleset_id = @ruleset;
                    IF @@ROWCOUNT = 0
                        INSERT INTO {{schema}}.active_rule_artifacts (ruleset_id, import_id) VALUES (@ruleset, @id);
                    """ : """
                    IF NOT EXISTS (SELECT 1 FROM {{schema}}.published_rule_artifacts WITH (UPDLOCK, HOLDLOCK)
                        WHERE ruleset_id = @ruleset AND import_id = @id)
                        INSERT INTO {{schema}}.published_rule_artifacts (ruleset_id, import_id) VALUES (@ruleset, @id);
                    """);
                AddScope(command, scope);
                AddId(command, id);
                await command.ExecuteNonQueryAsync(token);
            }
            await transaction.CommitAsync(token);
            return stored;
        }
        catch (OperationCanceledException) { throw new OperationCanceledException("Compiled rule access was cancelled.", token); }
        catch (ManagedServiceException) { throw; }
        catch (CompiledRuleRetrievalException) { throw; }
        catch (CompiledRulesImportException error)
        {
            throw new CompiledRuleRetrievalException(error.Failure == CompiledRulesImportFailure.IntegrityConflict
                ? CompiledRuleRetrievalFailure.ArtifactInconsistent : CompiledRuleRetrievalFailure.StorageFailed);
        }
        // SqlClient can report a cancelled blocked command as SqlException.
        // The supplied token, not the driver's exception type, proves cancellation.
        catch (SqlException) when (token.IsCancellationRequested)
        { throw new OperationCanceledException("Compiled rule access was cancelled.", token); }
        catch (SqlException error) when (error.Number is 207 or 208)
        { throw new ManagedServiceException("MIGRATION_REQUIRED", "Compiled rule storage requires a supported migration."); }
        catch (Exception error) when (error is JsonException or InvalidOperationException or IndexOutOfRangeException or NullReferenceException)
        { throw new CompiledRuleRetrievalException(CompiledRuleRetrievalFailure.ArtifactInconsistent); }
        catch (Exception error) when (error is not OutOfMemoryException)
        { throw new CompiledRuleRetrievalException(CompiledRuleRetrievalFailure.StorageFailed); }
    }

    private static void AddScope(SqlCommand command, CompiledRuleStoreScope scope)
    {
        command.Parameters.Add("@ruleset", SqlDbType.NVarChar, 128).Value = scope.RulesetId;
        command.Parameters.Add("@semantic", SqlDbType.Char, 64).Value = scope.SemanticSha256;
    }
}
