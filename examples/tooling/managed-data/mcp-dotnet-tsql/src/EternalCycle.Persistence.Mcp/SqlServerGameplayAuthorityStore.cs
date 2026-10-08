using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace EternalCycle.Persistence.Mcp;

internal enum GameplayPersistencePhase { Inspect, Admit, Activate, ValidateReceipt }

public sealed partial class SqlServerPlayerInteractionStore
{
    // Called on the persistence store's connection/transaction: the session lock
    // is acquired before campaign/version locks, exactly as B/C/D acquire them.
    internal async Task GatePersistenceAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, CommitCampaignRequest request, string hash, GameplayPersistencePhase phase, CancellationToken token)
    {
        scope.Validate(); RequireEntryEnabled();
        if (!interactionOptions.Value.Enabled) throw Error("TRUSTED_SUBMISSION_REQUIRED", "Trusted interaction support is required for gameplay writes.");
        await SqlServerCampaignBindingStore.LockSessionAsync(connection, transaction, scope.Key, token);
        var value = await ReadAsync(connection, transaction, scope.Key, request.SourceInteractionId, null, token)
            ?? throw Error("TRUSTED_SUBMISSION_REQUIRED", "The write requires the original trusted interaction in this session.");
        await RequireAccessAsync(connection, transaction, scope, value, token);
        if (value.CampaignId != request.CampaignId) throw Error("GAMEPLAY_AUTHORITY_INVALID", "The write cannot target another campaign.");
        var receipt = await RequireGameplayEntryAsync(connection, transaction, scope, value, token);
        var progress = Progress(value, receipt);
        var admission = progress.Admissions.SingleOrDefault(item => item.TransactionId == request.TransactionId);
        if (admission is not null && admission.RequestHash != hash)
            throw Error("GAMEPLAY_REPLAY_CONFLICT", "Recover the original frozen transaction; its payload cannot change.");
        if (phase == GameplayPersistencePhase.Inspect)
        {
            // Historical validated receipts remain readable after yield, but a
            // new proposal never obtains authority through this read-only path.
            if (admission is null) await RequireForwardAsync(connection, transaction, scope, value, receipt, progress, token);
            return;
        }
        if (phase == GameplayPersistencePhase.Admit)
        {
            if (admission is not null) throw Error("GAMEPLAY_REPLAY_CONFLICT", "This operation is already admitted; resume its existing save transaction.");
            await RequireForwardAsync(connection, transaction, scope, value, receipt, progress, token);
            if (request.ExpectedParentVersion != progress.Baseline.CampaignVersion)
                throw Error("GAMEPLAY_ENTRY_BASELINE_STALE", "The proposed parent version is not the interaction's verified baseline.");
            if (progress.Admissions.Count >= 64) throw Error("GAMEPLAY_AUTHORITY_INVALID", "Narrow the interaction; its admitted transaction bound is exhausted.");
            foreach (var mutation in request.Mutations)
            {
                var address = new RecordAddress(mutation.OwnerDomain, mutation.RecordId);
                var action = mutation.Tombstone ? GameplayMutationAction.Tombstone : mutation.ExpectedRevision is null ? GameplayMutationAction.Create : GameplayMutationAction.Update;
                if (!receipt.MutationScope.Contains(new(address, action)))
                    throw Error("GAMEPLAY_MUTATION_SCOPE_DENIED", "This owner/action is not in the canonical scope for the actual input.");
                var read = progress.Reads.SingleOrDefault(item => item.Address == address);
                if (read is null || read.Revision != mutation.ExpectedRevision || read.Tombstone)
                    throw Error("GAMEPLAY_MUTATION_READ_REQUIRED", "Read the exact authorized owner (or verify permitted creation absence) before writing it.");
                foreach (var reference in mutation.References ?? [])
                    if (!progress.Reads.Any(item => item.Address == new RecordAddress(reference.TargetOwnerDomain, reference.TargetRecordId) && item.Revision is not null && !item.Tombstone) &&
                        !request.Mutations.Any(item => item.OwnerDomain == reference.TargetOwnerDomain && item.RecordId == reference.TargetRecordId && !item.Tombstone))
                        throw Error("GAMEPLAY_MUTATION_READ_REQUIRED", "Material references require verified owner evidence or a permitted same-transaction creation.");
            }
            progress = progress with { Admissions = Array.AsReadOnly(progress.Admissions.Append(new GameplayAdmission(request.TransactionId, hash)).ToArray()), PendingTransactionId = request.TransactionId };
            await SaveAsync(connection, transaction, value with { State = PlayerInteractionState.PERSISTING, PersistenceUnknown = true,
                Gameplay = progress, Revision = value.Revision + 1, UpdatedAt = DateTimeOffset.UtcNow }, false, token);
            return;
        }
        if (admission is null || progress.PendingTransactionId != request.TransactionId ||
            value.State != PlayerInteractionState.PERSISTING && !(value.State == PlayerInteractionState.BLOCKED && value.RecoveryState == PlayerInteractionState.PERSISTING))
            throw Error("GAMEPLAY_PERSISTENCE_UNKNOWN", "Only the original admitted transaction can reconcile an incomplete outcome.");
        var current = await RequireProfileAsync(connection, transaction, scope, value, progress, token);
        if (current.PendingTransactionId is not null && current.PendingTransactionId != request.TransactionId)
            throw Error("GAMEPLAY_PERSISTENCE_UNKNOWN", "Another incomplete persistence outcome blocks reconciliation.");
        if (await PendingAsync(connection, transaction, scope.Key, value.BindingId, token) is not null)
            throw Error("GAMEPLAY_DECISION_PENDING", "No unresolved player decision may authorize activation.");
        if (phase == GameplayPersistencePhase.Activate)
        {
            if (current.CampaignVersion != progress.Baseline.CampaignVersion || request.ExpectedParentVersion != current.CampaignVersion)
                throw Error("GAMEPLAY_ENTRY_BASELINE_STALE", "Canonical version changed; do not silently rebase the frozen transaction.");
            await VerifyReadsAsync(connection, transaction, value, progress, token);
            return;
        }
        if (current.CampaignVersion != checked(request.ExpectedParentVersion + 1))
            throw Error("GAMEPLAY_ENTRY_BASELINE_STALE", "The activated version is not the admitted result.");
        var reads = progress.Reads.ToDictionary(item => item.Address);
        var adapter = entryPersistence ?? throw Error("GAMEPLAY_ENTRY_REQUIRED", "Canonical persistence is unavailable.");
        foreach (var mutation in request.Mutations)
        {
            var address = new RecordAddress(mutation.OwnerDomain, mutation.RecordId);
            var actual = await adapter.ReadEffectiveRecordAsync(connection, transaction, value.CampaignId, address.OwnerDomain, address.RecordId, current.CampaignVersion, token);
            if (actual is null || actual.RecordRevision != (mutation.ExpectedRevision ?? 0) + 1 || actual.Tombstone != mutation.Tombstone ||
                GameplayEntryCanonReader.Hash(actual.PayloadJson) != GameplayEntryCanonReader.Hash(mutation.Tombstone ? "{}" : mutation.PayloadJson))
                throw Error("GAMEPLAY_PERSISTENCE_UNKNOWN", "The committed owner state does not match the admitted transaction.");
            reads[address] = new(address, actual.RecordRevision, GameplayEntryCanonReader.Hash(actual.PayloadJson), actual.Tombstone);
        }
        // Called after receipt/header updates, within the same transaction. Only
        // this interaction's proven save can advance its captured baseline.
        progress = progress with { Baseline = current, Reads = Array.AsReadOnly(reads.Values.OrderBy(item => item.Address.OwnerDomain, StringComparer.Ordinal)
            .ThenBy(item => item.Address.RecordId, StringComparer.Ordinal).ToArray()), PendingTransactionId = null };
        await VerifyReadsAsync(connection, transaction, value, progress, token);
        await SaveAsync(connection, transaction, value with { State = PlayerInteractionState.OPEN, PersistenceUnknown = false,
            RecoveryState = null, Gameplay = progress, Revision = value.Revision + 1, UpdatedAt = DateTimeOffset.UtcNow }, false, token);
    }

    internal async Task BlockIncompleteGameplayAsync(CampaignBindingScope scope, string campaign, string transactionId, CancellationToken token)
    {
        await TransactionAsync(scope, async (connection, transaction) =>
        {
            var value = await ReadAsync(connection, transaction, scope.Key, null, null, token);
            if (value?.CampaignId == campaign && value.Gameplay?.PendingTransactionId == transactionId &&
                value.State == PlayerInteractionState.PERSISTING && value.PersistenceUnknown)
                await SaveAsync(connection, transaction, value with { State = PlayerInteractionState.BLOCKED, RecoveryState = PlayerInteractionState.PERSISTING,
                    Revision = value.Revision + 1, UpdatedAt = DateTimeOffset.UtcNow }, false, token);
            return true;
        }, token);
    }

    private async Task<GameplayEntryReceipt> RequireGameplayEntryAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, PlayerInteractionRecord value, CancellationToken token)
    {
        var entry = (await ReadEntryAsync(connection, transaction, scope.Key, value.InteractionId, null, token))?.Receipt;
        if (entry is null) throw Error("GAMEPLAY_ENTRY_REQUIRED", "Mandatory gameplay entry must precede mutation, decision resolution or completion.");
        if (entry.InteractionId != value.InteractionId || entry.BindingId != value.BindingId || entry.CampaignId != value.CampaignId ||
            entry.BindingGeneration != value.BindingGeneration || entry.InteractionRevision > value.Revision ||
            entry.Baseline != value.StartingEvidence || !entry.Rules.RequiredContextComplete || !entry.Rules.DependencyComplete || entry.Rules.Operation != "gameplay.resolve")
            throw Error("GAMEPLAY_AUTHORITY_INVALID", "Durable entry evidence is inconsistent with the original interaction.");
        return entry;
    }

    private static GameplayProgress Progress(PlayerInteractionRecord value, GameplayEntryReceipt receipt) => value.Gameplay ?? new(value.StartingEvidence,
        Array.AsReadOnly(receipt.Reads.Where(read => read.Status == EntryCanonStatus.Completed && read.Address is not null)
            .Select(read => new GameplayOwnerEvidence(read.Address!, read.Revision, read.PayloadSha256, false)).ToArray()), [], null);

    private async Task<CampaignBindingEvidence> RequireProfileAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, PlayerInteractionRecord value, GameplayProgress progress, CancellationToken token)
    {
        var binding = await RequireAccessAsync(connection, transaction, scope, value, token);
        var selected = await bindings.ReadAsync(connection, transaction, scope.Key, null, token);
        if (selected?.BindingId != value.BindingId || binding.State != CampaignBindingState.ACTIVE || binding.SuccessorBindingId is not null)
            throw Error("INTERACTION_BINDING_UNAVAILABLE", "The original binding must remain current and active.");
        var current = await bindings.EvidenceAsync(connection, transaction, scope, value.CampaignId, token)
            ?? throw Error("BINDING_CAMPAIGN_UNAVAILABLE", "The selected canonical authority is unavailable.");
        var baseline = progress.Baseline;
        if (!baseline.SameProfile(current) || baseline.BootstrapHash != current.BootstrapHash || baseline.BootstrapRevision != current.BootstrapRevision ||
            baseline.BootstrapState != current.BootstrapState || !current.MinimumRulesReady)
            throw Error("GAMEPLAY_ENTRY_BASELINE_STALE", "Rules, bootstrap or canonical authority changed after entry.");
        return current;
    }

    private async Task RequireForwardAsync(SqlConnection connection, SqlTransaction transaction, CampaignBindingScope scope,
        PlayerInteractionRecord value, GameplayEntryReceipt receipt, GameplayProgress progress, CancellationToken token, bool response = false)
    {
        if (value.PersistenceUnknown || progress.PendingTransactionId is not null)
            throw Error("GAMEPLAY_PERSISTENCE_UNKNOWN", "Recover the original transaction before further resolution or completion.");
        if (value.State != PlayerInteractionState.OPEN)
            throw Error(value.State == PlayerInteractionState.COMPLETED ? "GAMEPLAY_INTERACTION_COMPLETED" : "GAMEPLAY_INTERACTION_YIELDED", "This interaction has no ordinary forward mutation authority.");
        var current = await RequireProfileAsync(connection, transaction, scope, value, progress, token);
        if (current.PendingTransactionId is not null) throw Error("PLAYER_INTERACTION_PERSISTENCE_UNRESOLVED", "Pending or failed persistence blocks forward work.");
        if (current != progress.Baseline) throw Error("GAMEPLAY_ENTRY_BASELINE_STALE", "The campaign baseline changed outside this interaction; refresh through authorized recovery, never silently rebase.");
        var decision = await PendingAsync(connection, transaction, scope.Key, value.BindingId, token);
        if (decision is not null && (!response || decision.RelatedInteractionId != value.InteractionId || value.RespondingToDecisionId != decision.DecisionId))
            throw Error("GAMEPLAY_DECISION_PENDING", "A new trusted, entered response must address the unresolved decision before advancement.");
        await VerifyReadsAsync(connection, transaction, value, progress, token);
    }

    private async Task VerifyReadsAsync(SqlConnection connection, SqlTransaction transaction, PlayerInteractionRecord value, GameplayProgress progress, CancellationToken token)
    {
        var adapter = entryPersistence ?? throw Error("GAMEPLAY_ENTRY_REQUIRED", "Canonical owner verification is unavailable.");
        foreach (var read in progress.Reads)
        {
            var actual = await adapter.ReadEffectiveRecordAsync(connection, transaction, value.CampaignId, read.Address.OwnerDomain, read.Address.RecordId, progress.Baseline.CampaignVersion, token);
            if (read.Revision is null ? actual is not null : actual is null || actual.RecordRevision != read.Revision || actual.Tombstone != read.Tombstone || GameplayEntryCanonReader.Hash(actual.PayloadJson) != read.PayloadHash)
                throw Error("GAMEPLAY_ENTRY_CANON_STALE", "A required authoritative owner changed after its verified read.");
        }
    }

    internal Task<PlayerInteractionStatus> RequireGameplayTransactionAsync(CampaignBindingScope scope, string id, string transactionId, CancellationToken token) =>
        TransactionAsync(scope, async (connection, transaction) =>
        {
            var value = await RequireInteractionAsync(connection, transaction, scope, id, token);
            if (!PlayerInteractionService.Bounded(transactionId, 128) || value.Gameplay?.Admissions.Any(item => item.TransactionId == transactionId) != true)
                throw Error("GAMEPLAY_AUTHORITY_INVALID", "Only an originally admitted transaction can be reconciled.");
            return await StatusAsync(connection, transaction, scope, value, token);
        }, token);

    internal Task<GameplayOwnerReadResult> ReadGameplayOwnersAsync(CampaignBindingScope scope, GameplayOwnerReadRequest request, CancellationToken token) =>
        TransactionAsync(scope, async (connection, transaction) =>
        {
            if (request is null || request.Records is null || request.Records.Count is < 1 or > 32 || request.Records.Distinct().Count() != request.Records.Count)
                throw Error("GAMEPLAY_AUTHORITY_INVALID", "Use a bounded distinct set of exact owner addresses.");
            var value = await RequireInteractionAsync(connection, transaction, scope, request.InteractionId, token);
            var receipt = await RequireGameplayEntryAsync(connection, transaction, scope, value, token); var progress = Progress(value, receipt);
            await RequireForwardAsync(connection, transaction, scope, value, receipt, progress, token);
            if (value.Revision != request.ExpectedRevision) throw Error("PLAYER_INTERACTION_STALE", "Recover the current interaction revision.");
            var records = new List<CanonicalRecord>(); var reads = progress.Reads.ToDictionary(read => read.Address);
            foreach (var address in request.Records)
            {
                if (!GameplayEntryCanonReader.ValidAddress(address) || !receipt.MutationScope.Any(permission => permission.Address == address))
                    throw Error("GAMEPLAY_MUTATION_SCOPE_DENIED", "Additional owner reads must be in the canonical operation scope.");
                var actual = await entryPersistence!.ReadEffectiveRecordAsync(connection, transaction, value.CampaignId, address.OwnerDomain, address.RecordId, progress.Baseline.CampaignVersion, token);
                if (actual is null && !receipt.MutationScope.Contains(new(address, GameplayMutationAction.Create)) || actual?.Tombstone == true)
                    throw Error("GAMEPLAY_MUTATION_READ_REQUIRED", "Missing Canon is not empty; only an explicitly permitted creation can verify absence.");
                if (actual is not null) records.Add(actual);
                reads[address] = new(address, actual?.RecordRevision, actual is null ? null : GameplayEntryCanonReader.Hash(actual.PayloadJson), false);
            }
            if (reads.Count > 64 || records.Sum(record => (long)System.Text.Encoding.UTF8.GetByteCount(record.PayloadJson)) > GameplayEntryCanonReader.MaximumCanonBytes)
                throw Error("GAMEPLAY_AUTHORITY_INVALID", "Additional owner evidence exceeds its bounded scope.");
            value = value with { Gameplay = progress with { Reads = Array.AsReadOnly(reads.Values.ToArray()) }, Revision = value.Revision + 1, UpdatedAt = DateTimeOffset.UtcNow };
            await SaveAsync(connection, transaction, value, false, token);
            return new GameplayOwnerReadResult(await StatusAsync(connection, transaction, scope, value, token), records.AsReadOnly());
        }, token);

    internal Task<GameplayDisposition> ConcludeGameplayAsync(CampaignBindingScope scope, GameplayConclusionRequest request, CancellationToken token) =>
        TransactionAsync(scope, async (connection, transaction) =>
        {
            ValidateConclusion(request);
            var value = await RequireInteractionAsync(connection, transaction, scope, request.InteractionId, token);
            var fingerprint = PlayerInteractionService.Hash(request);
            if (await RecoverGameplayRequestAsync(connection, transaction, scope, request.RequestId, value.InteractionId, fingerprint, token))
                return await DispositionAsync(connection, transaction, scope, value, token);
            var receipt = await RequireGameplayEntryAsync(connection, transaction, scope, value, token); var progress = Progress(value, receipt);
            await RequireForwardAsync(connection, transaction, scope, value, receipt, progress, token);
            if (value.Revision != request.ExpectedRevision) throw Error("PLAYER_INTERACTION_STALE", "Recover the original interaction revision before concluding.");
            var receipts = await ValidatedReceiptsAsync(connection, transaction, value, token);
            var affected = new HashSet<string>(StringComparer.Ordinal);
            foreach (var admission in progress.Admissions)
            {
                var stored = await ReadSaveAsync(connection, transaction, value.CampaignId, admission.TransactionId, token);
                if (stored.Hash != admission.RequestHash || stored.Request.SourceInteractionId != value.InteractionId || stored.Status != "Completed")
                    throw Error("GAMEPLAY_PERSISTENCE_UNKNOWN", "An admitted required write lacks authoritative completion evidence.");
                affected.UnionWith(stored.Request.AffectedOwnerDomains);
            }
            if (!affected.SetEquals(request.AffectedOwnerDomains!))
                throw Error("GAMEPLAY_AFFECTED_SET_INVALID", "The explicitly determined affected set must match all admitted and validated writes.");
            if (progress.Admissions.Count == 0 && request.AffectedOwnerDomains!.Count != 0 || receipts.Count != progress.Admissions.Count)
                throw Error("GAMEPLAY_COMPLETION_BLOCKED", "Every nonempty affected unit requires its validated receipt.");
            var question = request.QuestionReference is not null;
            if (question)
            {
                var decision = new PlayerDecisionRecord("PD-" + Guid.NewGuid().ToString("N"), scope.Key, value.BindingId, value.CampaignId,
                    value.InteractionId, progress.Baseline.CampaignVersion, request.QuestionReference!, request.ResponseScopeReference!, PlayerDecisionState.Pending, 1, null, value.CorrelationId);
                await SaveDecisionAsync(connection, transaction, decision, true, token);
                value = value with { PendingDecisionId = decision.DecisionId };
            }
            var previous = value.State;
            value = value with { State = question ? PlayerInteractionState.AWAITING_PLAYER_INPUT : PlayerInteractionState.COMPLETED,
                Gameplay = progress with { CompletedNarrationAuthorized = !question, QuestionPresentationAuthorized = question,
                    EmptyAffectedSetValidated = progress.Admissions.Count == 0 }, Revision = value.Revision + 1, UpdatedAt = DateTimeOffset.UtcNow };
            await SaveAsync(connection, transaction, value, false, token);
            await WriteGameplayRequestAsync(connection, transaction, scope, request.RequestId, fingerprint, value, previous, token);
            return await DispositionAsync(connection, transaction, scope, value, token);
        }, token);

    internal Task<GameplayDisposition> ResolveGameplayDecisionAsync(CampaignBindingScope scope, GameplayDecisionRequest request, CancellationToken token) =>
        TransactionAsync(scope, async (connection, transaction) =>
        {
            if (request is null || !PlayerInteractionService.Bounded(request.RequestId, 128) || !PlayerInteractionService.Bounded(request.DecisionId, 128) || request.ExpectedDecisionRevision <= 0)
                throw Error("GAMEPLAY_AUTHORITY_INVALID", "Use bounded original decision and request identities.");
            var value = await RequireInteractionAsync(connection, transaction, scope, request.InteractionId, token);
            var fingerprint = PlayerInteractionService.Hash(request);
            if (await RecoverGameplayRequestAsync(connection, transaction, scope, request.RequestId, value.InteractionId, fingerprint, token))
                return await DispositionAsync(connection, transaction, scope, value, token);
            var receipt = await RequireGameplayEntryAsync(connection, transaction, scope, value, token); var progress = Progress(value, receipt);
            await RequireForwardAsync(connection, transaction, scope, value, receipt, progress, token, response: true);
            var decision = await PendingAsync(connection, transaction, scope.Key, value.BindingId, token);
            if (value.Revision != request.ExpectedRevision || decision?.DecisionId != request.DecisionId || decision.Revision != request.ExpectedDecisionRevision ||
                value.RespondingToDecisionId != request.DecisionId || decision.RelatedInteractionId != value.InteractionId || decision.CampaignId != value.CampaignId ||
                decision.StartingCampaignVersion != progress.Baseline.CampaignVersion)
                throw Error("PLAYER_DECISION_CONFLICT", "The actual response is unrelated, stale or already superseded.");
            var response = value.DecisionResponse;
            if (!GameplayAuthority.ValidResponse(response, decision.DecisionId) || response is null || response.AllowedResponseScopeReference != decision.AllowedResponseScopeReference)
                throw Error("GAMEPLAY_TRUSTED_RESPONSE_REQUIRED", "The trusted host must establish what this actual player response supports. The model cannot supply a choice.");
            var previous = value.State;
            if (response.Disposition == PlayerResponseDisposition.Clarification)
            {
                // No fictional option is executed. Keep the decision pending and
                // this genuine clarification yielded; a later actual reply gets its own ID.
                value = value with { State = PlayerInteractionState.AWAITING_PLAYER_INPUT, PendingDecisionId = decision.DecisionId,
                    Gameplay = progress with { QuestionPresentationAuthorized = true, EmptyAffectedSetValidated = true } };
            }
            else
                await SaveDecisionAsync(connection, transaction, decision with { State = PlayerDecisionState.Resolved, Response = response, Revision = decision.Revision + 1 }, false, token);
            value = value with { Gameplay = value.Gameplay ?? progress, Revision = value.Revision + 1, UpdatedAt = DateTimeOffset.UtcNow };
            await SaveAsync(connection, transaction, value, false, token);
            await WriteGameplayRequestAsync(connection, transaction, scope, request.RequestId, fingerprint, value, previous, token);
            return await DispositionAsync(connection, transaction, scope, value, token);
        }, token);

    private async Task<PlayerInteractionRecord> RequireInteractionAsync(SqlConnection connection, SqlTransaction transaction, CampaignBindingScope scope, string id, CancellationToken token)
    {
        RequireEntryEnabled();
        if (!PlayerInteractionService.Bounded(id, 128)) throw Error("GAMEPLAY_AUTHORITY_INVALID", "Use a bounded original interaction identity.");
        var value = await ReadAsync(connection, transaction, scope.Key, id, null, token) ?? throw Error("TRUSTED_SUBMISSION_REQUIRED", "No trusted interaction exists in this session.");
        await RequireAccessAsync(connection, transaction, scope, value, token); return value;
    }

    private static void ValidateConclusion(GameplayConclusionRequest request)
    {
        if (request is null || !PlayerInteractionService.Bounded(request.RequestId, 128) || request.ExpectedRevision <= 0 ||
            request.AffectedOwnerDomains is null || request.AffectedOwnerDomains.Count > 64 || request.AffectedOwnerDomains.Any(owner => !PlayerInteractionService.Bounded(owner, 128)) ||
            request.AffectedOwnerDomains.Distinct(StringComparer.Ordinal).Count() != request.AffectedOwnerDomains.Count ||
            (request.QuestionReference is null) != (request.ResponseScopeReference is null) || request.QuestionReference is not null &&
                (!PlayerInteractionService.Bounded(request.QuestionReference, 256) || !PlayerInteractionService.Bounded(request.ResponseScopeReference, 256)))
            throw Error("GAMEPLAY_AFFECTED_SET_INVALID", "Explicitly determine the bounded affected set, including empty; question presentation requires protected alternatives/response references.");
    }

    private async Task<bool> RecoverGameplayRequestAsync(SqlConnection connection, SqlTransaction transaction, CampaignBindingScope scope,
        string request, string interaction, string fingerprint, CancellationToken token)
    {
        await using var command = Command(connection, transaction, "SELECT request_hash, interaction_id FROM {{schema}}.player_interaction_receipts WHERE scope_key=@scope AND request_id=@request;");
        command.Parameters.AddWithValue("@scope", scope.Key); command.Parameters.AddWithValue("@request", request);
        await using var reader = await command.ExecuteReaderAsync(token);
        if (!await reader.ReadAsync(token)) return false;
        if (reader.GetString(0) != fingerprint || reader.GetString(1) != interaction)
            throw Error("GAMEPLAY_REPLAY_CONFLICT", "The lifecycle request identity cannot be reused with another payload or interaction.");
        return true;
    }

    private async Task WriteGameplayRequestAsync(SqlConnection connection, SqlTransaction transaction, CampaignBindingScope scope,
        string request, string fingerprint, PlayerInteractionRecord value, PlayerInteractionState previous, CancellationToken token)
    {
        await using var command = Command(connection, transaction, """
            INSERT INTO {{schema}}.player_interaction_receipts(scope_key,request_id,request_hash,interaction_id,transition_owner,previous_state,resulting_state,resulting_revision)
            VALUES(@scope,@request,@hash,@id,N'Yield',@previous,@state,@revision);
            """);
        command.Parameters.AddWithValue("@scope", scope.Key); command.Parameters.AddWithValue("@request", request); command.Parameters.AddWithValue("@hash", fingerprint);
        command.Parameters.AddWithValue("@id", value.InteractionId); command.Parameters.AddWithValue("@previous", previous.ToString());
        command.Parameters.AddWithValue("@state", value.State.ToString()); command.Parameters.AddWithValue("@revision", value.Revision);
        await command.ExecuteNonQueryAsync(token);
    }

    private async Task<(CommitCampaignRequest Request, string Hash, string Status)> ReadSaveAsync(SqlConnection connection, SqlTransaction transaction,
        string campaign, string id, CancellationToken token)
    {
        await using var command = entryPersistence!.CreateCampaignCommand(connection, transaction, campaign,
            "SELECT request_json, request_hash, status FROM {{schema}}.save_transactions WHERE campaign_id=@campaign AND transaction_id=@id;");
        command.Parameters.AddWithValue("@campaign", campaign); command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync(token);
        if (!await reader.ReadAsync(token)) throw Error("GAMEPLAY_PERSISTENCE_UNKNOWN", "The original admitted transaction is missing; do not replace it.");
        return (JsonSerializer.Deserialize<CommitCampaignRequest>(reader.GetString(0)) ?? throw Error("GAMEPLAY_PERSISTENCE_UNKNOWN", "The frozen transaction is incomplete."), reader.GetString(1), reader.GetString(2));
    }

    private async Task<IReadOnlyList<PersistenceReceipt>> ValidatedReceiptsAsync(SqlConnection connection, SqlTransaction transaction, PlayerInteractionRecord value, CancellationToken token)
    {
        var results = new List<PersistenceReceipt>();
        foreach (var admission in value.Gameplay?.Admissions ?? [])
        {
            await using var command = entryPersistence!.CreateCampaignCommand(connection, transaction, value.CampaignId, """
                SELECT receipt_id, campaign_version, validation_evidence, completed_at FROM {{schema}}.persistence_receipts
                WHERE campaign_id=@campaign AND transaction_id=@id AND status=N'Validated';
                """);
            command.Parameters.AddWithValue("@campaign", value.CampaignId); command.Parameters.AddWithValue("@id", admission.TransactionId);
            await using var reader = await command.ExecuteReaderAsync(token);
            if (!await reader.ReadAsync(token)) throw Error("GAMEPLAY_COMPLETION_BLOCKED", "A required write has no validated durable receipt.");
            if (reader.GetInt64(1) != checked(value.StartingEvidence.CampaignVersion + results.Count + 1))
                throw Error("GAMEPLAY_COMPLETION_BLOCKED", "Receipt versions do not prove the interaction's ordered committed results.");
            results.Add(new(reader.GetString(0), value.CampaignId, admission.TransactionId, reader.GetInt64(1), PersistenceMarkers.Saved, reader.GetString(2), reader.GetDateTimeOffset(3)));
        }
        if (value.Gameplay is not null && results.Count != 0 && results[^1].CampaignVersion != value.Gameplay.Baseline.CampaignVersion)
            throw Error("GAMEPLAY_COMPLETION_BLOCKED", "The last validated receipt does not establish the resulting baseline.");
        return results.AsReadOnly();
    }

    private async Task<GameplayDisposition> DispositionAsync(SqlConnection connection, SqlTransaction transaction, CampaignBindingScope scope, PlayerInteractionRecord value, CancellationToken token)
    {
        var status = await StatusAsync(connection, transaction, scope, value, token);
        // A historical question receipt cannot present resolved alternatives as
        // pending again. Its originating interaction still remains yielded.
        return new(status, status.Gameplay?.CompletedNarrationAuthorized == true, status.Gameplay?.QuestionPresentationAuthorized == true,
            value.Gameplay?.EmptyAffectedSetValidated == true, value.Gameplay?.Baseline.CampaignVersion ?? value.StartingEvidence.CampaignVersion,
            await ValidatedReceiptsAsync(connection, transaction, value, token));
    }
}
