using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace EternalCycle.Persistence.Mcp;

public sealed partial class SqlServerPlayerInteractionStore
{
    private sealed record EntryRecord(string RequestId, string Fingerprint, long PendingRevision,
        GameplayEntryReceipt? Receipt, string? FailureCode, IReadOnlyList<GameplayEntryReadEvidence> Reads);
    internal Action? BeforeEntryCommit { get; init; }

    public Task<PreparedGameplayEntry> PrepareAsync(CampaignBindingScope scope, GameplayEntryRequest request, CancellationToken token) =>
        TransactionAsync<PreparedGameplayEntry>(scope, async (connection, transaction) =>
        {
            RequireEntryEnabled();
            // Recover the stable request before choosing a latest interaction. A
            // retry cannot accidentally attach an old request to newer input.
            var prior = await ReadEntryAsync(connection, transaction, scope.Key, null, request.RequestId, token);
            var value = await ReadAsync(connection, transaction, scope.Key, request.InteractionId ?? prior?.Receipt?.InteractionId,
                null, token) ?? throw Error("TRUSTED_SUBMISSION_REQUIRED", "An actual trusted submission must exist before gameplay entry.");
            var fingerprint = GameplayEntryCanonReader.Hash(JsonSerializer.Serialize(new { request, value.InteractionId }, Json));
            prior ??= await ReadEntryAsync(connection, transaction, scope.Key, value.InteractionId, null, token);
            var binding = await RequireAccessAsync(connection, transaction, scope, value, token);
            if (prior is not null)
            {
                if (prior.RequestId != request.RequestId || prior.Fingerprint != fingerprint)
                    throw Error("GAMEPLAY_ENTRY_CONFLICT", "This interaction already has a different stable entry request. Recover its original request.");
                if (prior.Receipt is not null) return new(value, binding, request, fingerprint, prior.Receipt);
                if (value.State != PlayerInteractionState.ENTRY_PENDING || value.Revision != prior.PendingRevision)
                    throw Error("PLAYER_INTERACTION_STALE", "Entry preparation no longer owns the interaction revision.");
            }
            else if (value.Revision != request.ExpectedRevision || value.State is not (PlayerInteractionState.RECEIVED or PlayerInteractionState.ENTRY_PENDING))
                throw Error("PLAYER_INTERACTION_STALE", "Recover the original pre-entry interaction and expected revision.");

            binding = await RequireEntryAuthorityAsync(connection, transaction, scope, value, token);
            if (value.State == PlayerInteractionState.RECEIVED)
            {
                value = value with { State = PlayerInteractionState.ENTRY_PENDING, Revision = checked(value.Revision + 1), UpdatedAt = DateTimeOffset.UtcNow };
                await SaveAsync(connection, transaction, value, false, token);
            }
            if (prior is null)
                await WriteEntryAsync(connection, transaction, scope.Key, value.InteractionId,
                    new(request.RequestId, fingerprint, value.Revision, null, null, []), true, token);
            return new(value, binding, request, fingerprint, null);
        }, token);

    public Task<GameplayEntryResult> CompleteAsync(CampaignBindingScope scope, PreparedGameplayEntry entry,
        VerifiedGameplayEntryContext? context, CancellationToken token) => TransactionAsync<GameplayEntryResult>(scope, async (connection, transaction) =>
    {
        RequireEntryEnabled();
        var value = await ReadAsync(connection, transaction, scope.Key, entry.InteractionId, null, token)
            ?? throw Error("TRUSTED_SUBMISSION_REQUIRED", "The original trusted input is unavailable.");
        await RequireAccessAsync(connection, transaction, scope, value, token);
        var prior = await ReadEntryAsync(connection, transaction, scope.Key, value.InteractionId, null, token);
        if (prior is null || prior.RequestId != entry.Request.RequestId || prior.Fingerprint != entry.Fingerprint)
            throw Error("GAMEPLAY_ENTRY_CONFLICT", "Recover the original durable entry request.");
        if (prior.Receipt is not null)
            return new(await StatusAsync(connection, transaction, scope, value, token), prior.Receipt, null, true, prior.Receipt.Reads);
        if (context is null || value.State != PlayerInteractionState.ENTRY_PENDING || value.Revision != entry.Interaction.Revision ||
            value.Revision != prior.PendingRevision)
            throw Error("PLAYER_INTERACTION_STALE", "Entry cannot grant OPEN from stale or closed preparation.");
        var binding = await RequireEntryAuthorityAsync(connection, transaction, scope, value, token);
        if (binding.Revision != entry.Binding.Revision || binding.Evidence != entry.Baseline)
            throw Error("GAMEPLAY_ENTRY_BASELINE_STALE", "Canonical binding or profile changed during context preparation. No OPEN grant was committed.");
        var rules = context.Rules;
        if (!GameplayEntryCanonReader.CompleteEvidence(context.Canon))
            throw Error("GAMEPLAY_ENTRY_CANON_INCOMPLETE", "Required owner evidence or its supplied context is incomplete.");
        if (!rules.DependencyComplete || !rules.RequiredContextComplete || rules.Operation != "gameplay.resolve" ||
            rules.Identity != binding.Evidence.ActiveRuleIdentity || rules.Representation != binding.Evidence.RuleRepresentation ||
            rules.ImmutableSourceIdentity != binding.Evidence.ImmutableSourceIdentity)
            throw Error("GAMEPLAY_ENTRY_RULES_INCOMPLETE", "The prepared rules do not match the active authority.");

        // Reuse the canonical effective-record query inside this serializable
        // transaction. Version/profile rows are also locked by EvidenceAsync.
        var persistence = entryPersistence ?? throw Error("GAMEPLAY_ENTRY_DISABLED", "The canonical read adapter is not configured for entry.");
        foreach (var read in context.Canon.Evidence)
        {
            token.ThrowIfCancellationRequested();
            if (read.Status == EntryCanonStatus.LegitimatelyAbsent && read.Address is null) continue;
            if (read.Status != EntryCanonStatus.Completed || read.Address is null)
                throw Error("GAMEPLAY_ENTRY_CANON_INCOMPLETE", "Canonical read evidence is incomplete.");
            var record = await persistence.ReadEffectiveRecordAsync(connection, transaction, value.CampaignId,
                read.Address.OwnerDomain, read.Address.RecordId, binding.Evidence.CampaignVersion, token);
            if (record is null || record.Tombstone || record.RecordRevision != read.Revision || GameplayEntryCanonReader.Hash(record.PayloadJson) != read.PayloadSha256)
                throw Error("GAMEPLAY_ENTRY_CANON_STALE", "An authoritative owner record changed after its entry read.");
        }
        var next = value with { State = PlayerInteractionState.OPEN, Revision = checked(value.Revision + 1), UpdatedAt = DateTimeOffset.UtcNow };
        var status = await StatusAsync(connection, transaction, scope, value, token);
        var decision = await PendingAsync(connection, transaction, scope.Key, value.BindingId, token);
        var receipt = new GameplayEntryReceipt("ENTRY-" + entry.Fingerprint, entry.Request.RequestId, value.InteractionId,
            binding.BindingId, value.CampaignId, binding.Generation, next.Revision, binding.Evidence, rules,
            context.Canon.Evidence, decision is null ? null : new(status.PendingDecision!, decision.ProtectedQuestionReference,
                decision.AllowedResponseScopeReference), next.UpdatedAt, value.CorrelationId)
        { MutationScope = Array.AsReadOnly(context.Canon.Plan.MutationScope.ToArray()) };
        await WriteEntryAsync(connection, transaction, scope.Key, value.InteractionId, prior with { Receipt = receipt, FailureCode = null, Reads = context.Canon.Evidence }, false, token);
        await SaveAsync(connection, transaction, next, false, token);
        BeforeEntryCommit?.Invoke();
        return new(await StatusAsync(connection, transaction, scope, next, token), receipt, null, false, receipt.Reads);
    }, token);

    public async Task RecordFailureAsync(CampaignBindingScope scope, PreparedGameplayEntry entry, string code,
        IReadOnlyList<GameplayEntryReadEvidence> reads, CancellationToken token)
    {
        await TransactionAsync(scope, async (connection, transaction) =>
        {
            var prior = await ReadEntryAsync(connection, transaction, scope.Key, entry.InteractionId, null, token);
            if (prior is not null && prior.Receipt is null && prior.Fingerprint == entry.Fingerprint)
                await WriteEntryAsync(connection, transaction, scope.Key, entry.InteractionId,
                    prior with { FailureCode = code, Reads = Array.AsReadOnly(reads.Take(GameplayEntryCanonReader.MaximumReads).ToArray()) }, false, token);
            return true;
        }, token);
    }

    private void RequireEntryEnabled()
    {
        if (entryOptions?.Value.Enabled != true) throw Error("GAMEPLAY_ENTRY_DISABLED", "The mandatory entry checkpoint is not configured.");
    }

    private async Task<CampaignSessionBinding> RequireEntryAuthorityAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, PlayerInteractionRecord value, CancellationToken token)
    {
        var binding = await RequireAccessAsync(connection, transaction, scope, value, token);
        var currentBinding = await bindings.ReadAsync(connection, transaction, scope.Key, null, token);
        if (binding.State != CampaignBindingState.ACTIVE || binding.SuccessorBindingId is not null || currentBinding?.BindingId != binding.BindingId)
            throw Error("INTERACTION_BINDING_UNAVAILABLE", "The original binding is not current and active.");
        var current = await bindings.EvidenceAsync(connection, transaction, scope, value.CampaignId, token)
            ?? throw Error("BINDING_CAMPAIGN_UNAVAILABLE", "Canonical campaign authority is unavailable.");
        if (value.PersistenceUnknown || current.PendingTransactionId is not null)
            throw Error("PLAYER_INTERACTION_PERSISTENCE_UNRESOLVED", "Reconcile the original persistence outcome before entry.");
        if (current.BootstrapState is not ("UserConfirmed" or "Verified"))
            throw Error("GM_HOST_CONFIGURATION_REQUIRED", "Present and confirm the selected canonical bootstrap before entry.");
        if (!current.Ready) throw Error("BINDING_NOT_READY", "Required rules, validated save and confirmed bootstrap must be ready before entry.");
        if (!value.StartingEvidence.SameProfile(current))
            throw Error("GAMEPLAY_ENTRY_PROFILE_MISMATCH", "Original trusted input belongs to another active rule profile.");
        if (value.StartingEvidence != current || binding.Evidence != current)
            throw Error("GAMEPLAY_ENTRY_BASELINE_STALE", "The original campaign, bootstrap or binding evidence changed. Recover without silently rebasing input.");
        var decision = await PendingAsync(connection, transaction, scope.Key, binding.BindingId, token);
        if (value.RespondingToDecisionId is not null && (decision?.DecisionId != value.RespondingToDecisionId ||
                decision.RelatedInteractionId != value.InteractionId || decision.State != PlayerDecisionState.Pending) ||
            value.RespondingToDecisionId is null && decision is not null)
            throw Error("PLAYER_DECISION_CONFLICT", "The original pending decision linkage is missing or changed; entry cannot infer a choice.");
        return binding;
    }

    private async Task<EntryRecord?> ReadEntryAsync(SqlConnection connection, SqlTransaction transaction,
        string scope, string? interaction, string? request, CancellationToken token)
    {
        await using var command = Command(connection, transaction, """
            SELECT entry_json FROM {{schema}}.gameplay_entries WHERE scope_key = @scope
                AND (@interaction IS NULL OR interaction_id = @interaction) AND (@request IS NULL OR request_id = @request);
            """);
        command.Parameters.AddWithValue("@scope", scope); command.Parameters.AddWithValue("@interaction", (object?)interaction ?? DBNull.Value);
        command.Parameters.AddWithValue("@request", (object?)request ?? DBNull.Value);
        return await command.ExecuteScalarAsync(token) is string json ? JsonSerializer.Deserialize<EntryRecord>(json, Json) : null;
    }

    private async Task WriteEntryAsync(SqlConnection connection, SqlTransaction transaction, string scope,
        string interaction, EntryRecord value, bool insert, CancellationToken token)
    {
        var json = JsonSerializer.Serialize(value, Json);
        if (json.Length > 65_536) throw Error("GAMEPLAY_ENTRY_CANON_INCOMPLETE", "Entry evidence exceeds its bounded durable record.");
        await using var command = Command(connection, transaction, insert ? """
            INSERT INTO {{schema}}.gameplay_entries(scope_key, interaction_id, request_id, request_hash, entry_json)
            VALUES (@scope, @interaction, @request, @hash, @json);
            """ : """
            UPDATE {{schema}}.gameplay_entries SET entry_json = @json
            WHERE scope_key = @scope AND interaction_id = @interaction AND request_id = @request AND request_hash = @hash;
            """);
        command.Parameters.AddWithValue("@scope", scope); command.Parameters.AddWithValue("@interaction", interaction);
        command.Parameters.AddWithValue("@request", value.RequestId); command.Parameters.AddWithValue("@hash", value.Fingerprint);
        command.Parameters.AddWithValue("@json", json);
        if (await command.ExecuteNonQueryAsync(token) != 1) throw Error("GAMEPLAY_ENTRY_CONFLICT", "Entry request ownership changed.");
    }
}
