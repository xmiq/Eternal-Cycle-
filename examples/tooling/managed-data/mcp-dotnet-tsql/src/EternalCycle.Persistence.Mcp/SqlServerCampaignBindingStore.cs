using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

// C's implementation must read interaction/decision state under this transaction
// and participate in the same session lock. A model-supplied 'clear' flag is forbidden.
public interface ISqlCampaignBindingSwitchSafety
{
    Task<CampaignSwitchSafety> CheckAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignSessionBinding binding, CancellationToken token);
}

public sealed class SqlServerCampaignBindingStore(IOptions<SqlServerPersistenceOptions> options,
    ICampaignSchemaResolver routes, ICampaignDirectoryService directory,
    IOptions<CompiledRuleRuntimeOptions> compiledOptions,
    ISqlCampaignBindingSwitchSafety? switchSafety = null) : ICampaignBindingStore
{
    private readonly SqlServerPersistenceOptions settings = options.Value;
    private static readonly JsonSerializerOptions Json = new() { Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } };
    internal Action? AfterPredecessorClosed { get; init; }

    public async Task<ManagedOperationResult<CampaignBindingResolution>> ExecuteAsync(CampaignBindingScope scope,
        CampaignBindingAction action, CampaignBindingChange? change, string correlationId, CancellationToken token)
    {
        scope.Validate();
        try
        {
            await using var connection = new SqlConnection(settings.ConnectionString);
            await connection.OpenAsync(token);
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, token);
            await LockSessionAsync(connection, transaction, scope.Key, token);
            var current = await ReadAsync(connection, transaction, scope.Key, null, token);
            if (current is not null && action == CampaignBindingAction.Resolve)
            {
                current = await RefreshAsync(connection, transaction, scope, current, false, token);
                if (!scope.Allows(current.CampaignId))
                {
                    await transaction.CommitAsync(token);
                    return Failure("BINDING_UNAUTHORIZED");
                }
            }
            // Recover the selected authority first. Choices are optional routing
            // evidence, not a new discovery decision for an existing binding.
            var descriptors = (await directory.ListAsync(token)).Where(value => scope.Allows(value.CampaignId))
                .OrderBy(value => value.CampaignId, StringComparer.Ordinal).ToArray();
            var candidates = new List<(CampaignDescriptor Descriptor, CampaignBindingEvidence Evidence)>();
            foreach (var descriptor in descriptors)
            {
                var evidence = await EvidenceAsync(connection, transaction, scope, descriptor.CampaignId, token);
                if (evidence?.Ready == true) candidates.Add((descriptor, evidence));
            }
            var choices = candidates.Select(value => Choice(scope.Key, value.Descriptor, value.Evidence)).ToArray();
            for (var index = 0; index < choices.Length; index++)
                if (choices.Count(value => value.DisplayName == choices[index].DisplayName) > 1)
                {
                    var name = choices[index].DisplayName;
                    for (var match = index; match < choices.Length; match++)
                        if (choices[match].DisplayName == name)
                            choices[match] = choices[match] with { DisplayName = $"{name} (choice {match + 1})" };
                }

            if (change is not null)
            {
                var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { action, change }, Json)));
                await using var receipt = Command(connection, transaction, """
                    SELECT request_hash, binding_id FROM {{schema}}.campaign_binding_receipts
                    WHERE scope_key = @scope AND request_id = @request;
                    """);
                receipt.Parameters.AddWithValue("@scope", scope.Key);
                receipt.Parameters.AddWithValue("@request", change.RequestId);
                string? existingId = null;
                await using (var reader = await receipt.ExecuteReaderAsync(token))
                {
                    if (await reader.ReadAsync(token))
                    {
                        if (reader.GetString(0) != hash) return Failure("BINDING_CONFLICT");
                        existingId = reader.GetString(1);
                    }
                }
                if (existingId is not null)
                {
                    var replay = await ReadAsync(connection, transaction, scope.Key, existingId, token);
                    if (replay is null || !scope.Allows(replay.CampaignId)) return Failure("BINDING_UNAUTHORIZED");
                    if (replay.State != CampaignBindingState.CLOSED)
                        replay = await RefreshAsync(connection, transaction, scope, replay, false, token);
                    await transaction.CommitAsync(token);
                    return Success("BINDING_REPLAY", replay, choices, "recovered");
                }
                if (current?.BindingId != change.ExpectedBindingId || (current?.Revision ?? 0) != change.ExpectedRevision)
                    return Failure("BINDING_CONFLICT");
            }

            ManagedOperationResult<CampaignBindingResolution> result;
            if (action == CampaignBindingAction.Resolve && current is not null)
            {
                var refreshed = current;
                // Access revocation is persisted as suspension but does not disclose
                // the inaccessible campaign, binding or choice details to this caller.
                result = scope.Allows(current.CampaignId)
                    ? refreshed.State == CampaignBindingState.ACTIVE
                        ? Success("BINDING_RECOVERED", refreshed, choices, "recovered")
                        : Failure(refreshed.SuspensionCode ?? "BINDING_SUSPENDED", refreshed, choices)
                    : Failure("BINDING_UNAUTHORIZED");
            }
            else if (action == CampaignBindingAction.Suspend)
            {
                if (current is null) return Failure("BINDING_UNBOUND");
                if (!scope.Allows(current.CampaignId)) return Failure("BINDING_UNAUTHORIZED");
                current = current with { State = CampaignBindingState.SUSPENDED, Revision = current.Revision + 1, SuspensionCode = "BINDING_SUSPENDED" };
                await UpdateAsync(connection, transaction, current, token);
                result = Success("BINDING_SUSPENDED", current, choices, "explicit-suspension");
            }
            else
            {
                (CampaignDescriptor Descriptor, CampaignBindingEvidence Evidence) selected;
                if (action == CampaignBindingAction.Resolve)
                {
                    if (candidates.Count != 1 || !scope.AllowSoleCampaignResume)
                    {
                        await transaction.CommitAsync(token);
                        return Failure(candidates.Count == 0 ? "BINDING_UNBOUND" : "CAMPAIGN_SELECTION_REQUIRED", choices: choices);
                    }
                    selected = candidates[0];
                }
                else
                {
                    var index = Array.FindIndex(choices, value => value.Handle == change!.ChoiceHandle);
                    if (index < 0) return Failure("BINDING_CAMPAIGN_UNAVAILABLE");
                    selected = candidates[index];
                }

                if (current is not null)
                {
                    if (!scope.Allows(current.CampaignId)) return Failure("BINDING_UNAUTHORIZED");
                    if (action == CampaignBindingAction.Select && selected.Descriptor.CampaignId == current.CampaignId)
                    {
                        // Explicit reconfirmation can refresh a changed validated profile,
                        // but not while old interaction/receipt ownership is unknown.
                        if (!current.Evidence.SameProfile(selected.Evidence) || current.SuspensionCode == "BINDING_SUSPENDED")
                            await RequireSwitchSafetyAsync(connection, transaction, scope, current, token);
                        current = await RefreshAsync(connection, transaction, scope, current, true, token);
                        result = current.State == CampaignBindingState.ACTIVE
                            ? Success("BINDING_SELECTED", current, choices, "explicit-selection")
                            : Failure(current.SuspensionCode ?? "BINDING_SUSPENDED", current, choices);
                    }
                    else
                    {
                        if (action != CampaignBindingAction.Switch || selected.Descriptor.CampaignId == current.CampaignId)
                            return Failure("BINDING_SWITCH_CONFLICT");
                        await RequireSwitchSafetyAsync(connection, transaction, scope, current, token);
                        var successor = Create(scope, selected, current.Generation + 1, "explicit-switch", change!.RequestId, correlationId, current.BindingId);
                        current = current with { State = CampaignBindingState.CLOSED, Revision = current.Revision + 1, SuccessorBindingId = successor.BindingId };
                        // Close before insertion for the unique current-scope index; set
                        // the successor FK only after that row exists. All steps are atomic.
                        await UpdateAsync(connection, transaction, current with { SuccessorBindingId = null }, token);
                        AfterPredecessorClosed?.Invoke(); // Test-only failure/cancellation injection proves atomic successor handoff.
                        await InsertAsync(connection, transaction, successor, token);
                        await UpdateAsync(connection, transaction, current, token);
                        result = Success("BINDING_SWITCHED", successor, choices, "explicit-switch");
                    }
                }
                else
                {
                    if (action == CampaignBindingAction.Switch) return Failure("BINDING_UNBOUND");
                    var binding = Create(scope, selected, 1, action == CampaignBindingAction.Resolve ? "sole-campaign" : "explicit-selection",
                        change?.RequestId ?? "policy-unique-resume", correlationId, null);
                    await InsertAsync(connection, transaction, binding, token);
                    result = Success("BINDING_SELECTED", binding, choices, binding.ResolutionMode);
                }
            }
            if (change is not null && result.Success)
            {
                await using var receipt = Command(connection, transaction, """
                    INSERT INTO {{schema}}.campaign_binding_receipts (scope_key, request_id, request_hash, binding_id, correlation_id)
                    VALUES (@scope, @request, @hash, @binding, @correlation);
                    """);
                receipt.Parameters.AddWithValue("@scope", scope.Key);
                receipt.Parameters.AddWithValue("@request", change.RequestId);
                receipt.Parameters.AddWithValue("@hash", Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { action, change }, Json))));
                receipt.Parameters.AddWithValue("@binding", result.Data!.Binding!.BindingId);
                receipt.Parameters.AddWithValue("@correlation", correlationId);
                await receipt.ExecuteNonQueryAsync(token);
            }
            token.ThrowIfCancellationRequested();
            await transaction.CommitAsync(token);
            return result;
        }
        catch (SqlException) when (token.IsCancellationRequested) { throw new OperationCanceledException(token); }
        catch (SqlException error) when (error.Number is 207 or 208)
        { throw new ManagedServiceException("BINDING_SCHEMA_REQUIRED", "Preview and apply the supported binding migration before using durable bindings.", error); }
        catch (SqlException error) when (error.Number is 1205 or 2601 or 2627)
        { throw new ManagedServiceException("BINDING_CONFLICT", "The binding changed concurrently. Recover its current revision and retry safely.", error); }
        catch (SqlException error)
        { throw new ManagedServiceException("BINDING_STORAGE_FAILED", "Binding persistence could not be completed. Recover the same request identity before retrying.", error); }
    }

    internal async Task<CampaignSessionBinding> RefreshAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, CampaignSessionBinding binding, bool explicitSelection, CancellationToken token)
    {
        var evidence = scope.Allows(binding.CampaignId) ? await EvidenceAsync(connection, transaction, scope, binding.CampaignId, token) : null;
        var code = evidence is null ? "BINDING_CAMPAIGN_UNAVAILABLE" :
            evidence.CampaignVersion < binding.Evidence.CampaignVersion ? "BINDING_CONFLICT" : !evidence.Ready ? "BINDING_NOT_READY" :
            !binding.Evidence.SameProfile(evidence) && !explicitSelection ? "BINDING_PROFILE_CHANGED" :
            binding.SuspensionCode == "BINDING_SUSPENDED" && !explicitSelection ? "BINDING_SUSPENDED" : null;
        var next = binding with
        {
            State = code is null ? CampaignBindingState.ACTIVE : CampaignBindingState.SUSPENDED,
            Evidence = evidence ?? binding.Evidence, SuspensionCode = code
        };
        // A changed profile must retain the previously adopted evidence until an
        // explicit authorized reconfirmation; a normal save version refresh is safe.
        if (evidence is not null && !binding.Evidence.SameProfile(evidence) && !explicitSelection)
            next = next with { Evidence = binding.Evidence };
        if (next != binding)
        {
            next = next with { Revision = binding.Revision + 1, VerifiedAt = DateTimeOffset.UtcNow };
            await UpdateAsync(connection, transaction, next, token);
        }
        return next;
    }

    private async Task RequireSwitchSafetyAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, CampaignSessionBinding binding, CancellationToken token)
    {
        var evidence = await EvidenceAsync(connection, transaction, scope, binding.CampaignId, token);
        if (evidence is null || evidence.PendingTransactionId is not null)
            throw new ManagedServiceException("BINDING_SWITCH_BLOCKED", "Reconcile the current campaign's persistence outcome before switching.");
        var safety = switchSafety is null ? CampaignSwitchSafety.Unknown : await switchSafety.CheckAsync(connection, transaction, binding, token);
        if (safety != CampaignSwitchSafety.Clear)
            throw new ManagedServiceException("BINDING_SWITCH_BLOCKED", "Switching requires verified closed interaction, decision and recovery evidence from the configured safety check.");
    }

    internal async Task<CampaignBindingEvidence?> EvidenceAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, string campaignId, CancellationToken token)
    {
        CampaignSchemaRoute route;
        try { route = routes.Resolve(campaignId); }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException) { return null; }
        await using (var exists = new SqlCommand("SELECT OBJECT_ID(@table, 'U');", connection, transaction))
        {
            exists.Parameters.AddWithValue("@table", route.SchemaName + ".campaigns");
            if (await exists.ExecuteScalarAsync(token) is DBNull) return null;
        }
        await using var campaign = new SqlCommand(SqlServerSchemaIdentifier.Bind("""
            SELECT active_version, last_validated_commit_at, repository_version,
                (SELECT TOP (1) transaction_id FROM {{schema}}.save_transactions
                 WHERE campaign_id = @campaign AND status <> N'Completed' ORDER BY transaction_id)
            FROM {{schema}}.campaigns WHERE campaign_id = @campaign;
            """, route.SchemaName), connection, transaction) { CommandTimeout = settings.CommandTimeoutSeconds };
        campaign.Parameters.AddWithValue("@campaign", campaignId);
        long version; DateTimeOffset? committed; string repository; string? pending;
        await using (var reader = await campaign.ExecuteReaderAsync(token))
        {
            if (!await reader.ReadAsync(token)) return null;
            version = reader.GetInt64(0); committed = reader.IsDBNull(1) ? null : reader.GetDateTimeOffset(1);
            repository = reader.GetString(2); pending = reader.IsDBNull(3) ? null : reader.GetString(3);
        }
        string? identity = null, source = null, bootstrap = null;
        var minimumReady = false;
        var compiled = compiledOptions.Value.Enabled;
        await using var rules = Command(connection, transaction, compiled ? """
            SELECT artifacts.semantic_sha256,
                CONCAT(JSON_VALUE(artifacts.header_json, '$.ruleset.source.scheme'), ':', JSON_VALUE(artifacts.header_json, '$.ruleset.source.value')),
                JSON_VALUE(sources.source_json, '$.sourceSha256')
            FROM {{schema}}.active_rule_artifacts AS active
            INNER JOIN {{schema}}.published_rule_artifacts AS published ON published.ruleset_id = active.ruleset_id AND published.import_id = active.import_id
            INNER JOIN {{schema}}.imported_rule_artifacts AS artifacts ON artifacts.import_id = active.import_id AND artifacts.ruleset_id = active.ruleset_id
            INNER JOIN {{schema}}.imported_rule_sources AS sources ON sources.import_id = active.import_id AND sources.rule_source_id = N'gm-host-bootstrap'
            WHERE active.ruleset_id = @ruleset;
            """ : """
            SELECT releases.rule_release_id, releases.source_identity, chunks.source_hash,
                CASE WHEN EXISTS (SELECT 1 FROM {{schema}}.rule_source_preparation AS preparation
                    WHERE preparation.rule_release_id = releases.rule_release_id AND preparation.preparation_tier = N'RuntimeKernel')
                    AND NOT EXISTS (SELECT 1 FROM {{schema}}.rule_source_preparation AS preparation
                    WHERE preparation.rule_release_id = releases.rule_release_id
                      AND preparation.preparation_tier IN (N'RuntimeKernel', N'CampaignBootstrap', N'ImmediateGameplayCore')
                      AND preparation.preparation_state <> N'Ready') THEN 1 ELSE 0 END
            FROM {{schema}}.active_rule_releases AS active
            INNER JOIN {{schema}}.rule_releases AS releases ON releases.rule_release_id = active.rule_release_id AND releases.ruleset_id = active.ruleset_id
            INNER JOIN {{schema}}.rule_chunks AS chunks ON chunks.rule_release_id = releases.rule_release_id AND chunks.rule_source_id = N'gm-host-bootstrap'
            WHERE active.ruleset_id = @ruleset AND releases.release_state = N'Active';
            """);
        rules.Parameters.AddWithValue("@ruleset", route.RulesetId);
        await using (var reader = await rules.ExecuteReaderAsync(token))
        {
            if (await reader.ReadAsync(token))
            {
                identity = reader.GetString(0); source = reader.IsDBNull(1) ? null : reader.GetString(1);
                bootstrap = reader.IsDBNull(2) ? null : reader.GetString(2);
                minimumReady = compiled || reader.GetInt32(3) == 1;
                // The heading-free bootstrap has one authoritative source hash.
                while (await reader.ReadAsync(token))
                    if (reader.GetString(2) != bootstrap) minimumReady = false;
            }
        }
        string state = "Required"; long revision = 0;
        await using var host = Command(connection, transaction, """
            SELECT bootstrap_source_hash, configuration_state, configuration_revision
            FROM {{schema}}.gm_host_configurations WHERE ruleset_id = @ruleset;
            """);
        host.Parameters.AddWithValue("@ruleset", route.RulesetId);
        await using (var reader = await host.ExecuteReaderAsync(token))
            if (await reader.ReadAsync(token) && reader.GetString(0) == bootstrap)
            { state = reader.GetString(1); revision = reader.GetInt64(2); }
        return new(scope.AuthorityId, route.DataNamespaceId, route.WorldModelId, route.SchemaModelVersion,
            route.RulesetId, route.RulesetVersion, repository, version, committed,
            compiled ? "CompiledArtifact" : "SourceRelease", identity, source, bootstrap, state, revision, minimumReady, pending);
    }

    internal static string ChoiceHandle(string scope, string campaign, CampaignBindingEvidence evidence) =>
        Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new { scope, campaign, evidence }, Json)));

    private static CampaignBindingChoice Choice(string scope, CampaignDescriptor descriptor, CampaignBindingEvidence evidence) =>
        new(ChoiceHandle(scope, descriptor.CampaignId, evidence), descriptor.DisplayName, descriptor.Description,
            descriptor.WorldModelId, descriptor.DataNamespaceId);

    private static CampaignSessionBinding Create(CampaignBindingScope scope,
        (CampaignDescriptor Descriptor, CampaignBindingEvidence Evidence) candidate, long generation, string mode,
        string request, string correlation, string? predecessor) => new("BIND-" + Guid.NewGuid().ToString("N"), scope.Key, generation,
            candidate.Descriptor.CampaignId, CampaignBindingState.ACTIVE, 1, candidate.Evidence, mode, request, correlation,
            predecessor, null, null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

    private static ManagedOperationResult<CampaignBindingResolution> Success(string code, CampaignSessionBinding binding,
        IReadOnlyList<CampaignBindingChoice> choices, string mode) => new(true, code,
            "The durable campaign binding is recorded. This does not authorize a player interaction.",
            new(binding, Array.AsReadOnly(choices.ToArray()), mode, binding.State == CampaignBindingState.ACTIVE && binding.Evidence.Ready), RetrySafe: true);

    private static ManagedOperationResult<CampaignBindingResolution> Failure(string code, CampaignSessionBinding? binding = null,
        IReadOnlyList<CampaignBindingChoice>? choices = null) => new(false, code, code switch
        {
            "BINDING_UNBOUND" => "No eligible validated campaign is bound. Use the existing approved setup path.",
            "CAMPAIGN_SELECTION_REQUIRED" => "Choose a campaign using its displayed name and description; no story matching is performed.",
            "BINDING_CONFLICT" => "The request identity or expected binding revision conflicts. Recover current binding evidence.",
            "BINDING_UNAUTHORIZED" => "The session is not authorized for this binding operation.",
            "BINDING_PROFILE_CHANGED" => "The campaign profile changed. Revalidate readiness and explicitly reconfirm before use.",
            "BINDING_NOT_READY" => "The selected campaign requires readiness or persistence recovery before use.",
            "BINDING_SUSPENDED" => "The binding is suspended; no gameplay authority is available.",
            _ => "The requested campaign binding is unavailable or requires an explicit safe switch. No substitute was selected."
        }, code == "BINDING_UNAUTHORIZED" ? null : new(binding, Array.AsReadOnly((choices ?? []).ToArray()), "unresolved", false), RetrySafe: true);

    private SqlCommand Command(SqlConnection connection, SqlTransaction transaction, string sql) =>
        new(SqlServerSchemaIdentifier.Bind(sql, settings.DomainSchema), connection, transaction) { CommandTimeout = settings.CommandTimeoutSeconds };

    internal static async Task LockSessionAsync(SqlConnection connection, SqlTransaction transaction, string key, CancellationToken token)
    {
        // All later interaction mutations must take this same transaction-owned lock
        // to make switch-versus-admission ordering enforceable without a new lock system.
        await using var command = new SqlCommand("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource = @resource, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
            SELECT @result;
            """, connection, transaction);
        command.Parameters.AddWithValue("@resource", "EC:CampaignBinding:" + key);
        if (Convert.ToInt32(await command.ExecuteScalarAsync(token)) < 0)
            throw new ManagedServiceException("BINDING_CONFLICT", "The session is busy. Recover current binding evidence and retry.");
    }

    internal async Task<CampaignSessionBinding?> ReadAsync(SqlConnection connection, SqlTransaction transaction,
        string scope, string? id, CancellationToken token)
    {
        await using var command = Command(connection, transaction, """
            SELECT binding_json FROM {{schema}}.campaign_session_bindings
            WHERE scope_key = @scope AND ((@id IS NULL AND binding_status <> N'CLOSED') OR binding_id = @id);
            """);
        command.Parameters.AddWithValue("@scope", scope);
        command.Parameters.AddWithValue("@id", (object?)id ?? DBNull.Value);
        var json = await command.ExecuteScalarAsync(token);
        return json is string value ? JsonSerializer.Deserialize<CampaignSessionBinding>(value, Json) : null;
    }

    private async Task InsertAsync(SqlConnection connection, SqlTransaction transaction, CampaignSessionBinding binding, CancellationToken token)
    {
        await using var command = Command(connection, transaction, """
            INSERT INTO {{schema}}.campaign_session_bindings
                (scope_key, binding_id, generation, campaign_id, binding_status, revision, predecessor_id, successor_id, binding_json)
            VALUES (@scope, @id, @generation, @campaign, @status, @revision, @predecessor, @successor, @json);
            """);
        AddBinding(command, binding);
        await command.ExecuteNonQueryAsync(token);
    }

    private async Task UpdateAsync(SqlConnection connection, SqlTransaction transaction, CampaignSessionBinding binding, CancellationToken token)
    {
        await using var command = Command(connection, transaction, """
            UPDATE {{schema}}.campaign_session_bindings SET binding_status = @status, revision = @revision,
                successor_id = @successor, binding_json = @json
            WHERE scope_key = @scope AND binding_id = @id AND campaign_id = @campaign AND generation = @generation;
            """);
        AddBinding(command, binding);
        if (await command.ExecuteNonQueryAsync(token) != 1)
            throw new ManagedServiceException("BINDING_CONFLICT", "The durable binding no longer matches its immutable identity.");
    }

    private static void AddBinding(SqlCommand command, CampaignSessionBinding binding)
    {
        command.Parameters.AddWithValue("@scope", binding.ScopeKey);
        command.Parameters.AddWithValue("@id", binding.BindingId);
        command.Parameters.AddWithValue("@generation", binding.Generation);
        command.Parameters.AddWithValue("@campaign", binding.CampaignId);
        command.Parameters.AddWithValue("@status", binding.State.ToString());
        command.Parameters.AddWithValue("@revision", binding.Revision);
        command.Parameters.AddWithValue("@predecessor", (object?)binding.PredecessorBindingId ?? DBNull.Value);
        command.Parameters.AddWithValue("@successor", (object?)binding.SuccessorBindingId ?? DBNull.Value);
        command.Parameters.AddWithValue("@json", JsonSerializer.Serialize(binding, Json));
    }
}
