using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public sealed partial class SqlServerPlayerInteractionStore(IOptions<SqlServerPersistenceOptions> options,
    SqlServerCampaignBindingStore bindings, IOptions<PlayerInteractionOptions> interactionOptions,
    IOptions<GameplayEntryOptions>? entryOptions = null,
    SqlServerCampaignPersistenceStore? entryPersistence = null) : IPlayerInteractionStore, IGameplayEntryStore
{
    // Intake and switching must agree on missing decision evidence as well as
    // active states. A deleted control row is not proof that a choice was answered.
    internal const string BlockingPredicate = """
        (interactions.interaction_state NOT IN (N'COMPLETED', N'CANCELLED', N'AWAITING_PLAYER_INPUT') OR interactions.persistence_unknown = 1
         OR (interactions.interaction_state = N'AWAITING_PLAYER_INPUT' AND NOT EXISTS (
             SELECT 1 FROM {{schema}}.player_pending_decisions AS decisions
             WHERE decisions.scope_key = interactions.scope_key AND decisions.binding_id = interactions.binding_id
               AND (decisions.origin_interaction_id = interactions.interaction_id OR
                    decisions.decision_id = JSON_VALUE(interactions.interaction_json, '$.RespondingToDecisionId'))
               AND decisions.decision_id = JSON_VALUE(interactions.interaction_json, '$.PendingDecisionId'))))
        """;
    private readonly SqlServerPersistenceOptions settings = options.Value;
    // A valid JSON document is not necessarily a complete durable aggregate.
    // Reject missing/null constructor evidence instead of exposing partial recovery.
    internal static readonly JsonSerializerOptions Json = new()
    {
        RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true,
        Converters = { new JsonStringEnumConverter() }
    };
    internal Action? AfterInteractionInsert { get; init; }

    public Task<PlayerInteractionAcceptance> AcceptAsync(CampaignBindingScope scope, VerifiedPlayerSubmission submission,
        string correlationId, CancellationToken token) => TransactionAsync(scope, async (connection, transaction) =>
    {
        var evidence = submission.Evidence;
        if (evidence.PrincipalId != scope.PrincipalId || evidence.LogicalSessionId != scope.LogicalSessionId || evidence.AuthorityId != scope.AuthorityId)
            throw Error("SUBMISSION_PROVENANCE_INVALID", "The verified submission cannot be used in another authorized session.");
        var submissionHash = PlayerInteractionService.Hash(evidence.SubmissionId);
        var fingerprint = PlayerInteractionService.Hash(evidence);
        var existing = await ReadAsync(connection, transaction, scope.Key, null, submissionHash, token);
        if (existing is not null)
        {
            if (existing.SubmissionFingerprint != fingerprint)
                throw Error("SUBMISSION_IDENTITY_CONFLICT", "The same submission identity cannot be reused with changed input, scope, binding or decision evidence.");
            await RequireAccessAsync(connection, transaction, scope, existing, token);
            return new PlayerInteractionAcceptance(await StatusAsync(connection, transaction, scope, existing, token), true);
        }
        var binding = await bindings.ReadAsync(connection, transaction, scope.Key, evidence.BindingId, token);
        if (binding is null || !scope.Allows(binding.CampaignId) || binding.State != CampaignBindingState.ACTIVE || binding.SuccessorBindingId is not null)
            throw Error("INTERACTION_BINDING_UNAVAILABLE", "The selected binding is unavailable in this authorized session.");
        binding = await bindings.RefreshAsync(connection, transaction, scope, binding, false, token);
        if (binding.State != CampaignBindingState.ACTIVE || !binding.Evidence.Ready || binding.SuccessorBindingId is not null)
            throw Error("INTERACTION_BINDING_UNAVAILABLE", "Recover the original binding's readiness before submitting gameplay; no successor was selected.");
        await RequireNoActiveAsync(connection, transaction, scope.Key, binding.BindingId, token);
        var decision = await PendingAsync(connection, transaction, scope.Key, binding.BindingId, token);
        if (decision is null ? evidence.PendingDecisionId is not null :
            decision.DecisionId != evidence.PendingDecisionId || decision.Revision != evidence.PendingDecisionRevision)
            throw Error("PLAYER_DECISION_CONFLICT", "A pending decision requires its exact trusted response relationship and current revision; no option was inferred.");
        var now = DateTimeOffset.UtcNow;
        var value = new PlayerInteractionRecord("PI-" + Guid.NewGuid().ToString("N"), scope.Key, submissionHash, fingerprint,
            binding.BindingId, binding.Generation, binding.CampaignId, binding.Evidence, evidence.Origin,
            evidence.ProvenanceReference, evidence.InputReference, evidence.InputSha256, evidence.SubmittedAt, correlationId,
            PlayerInteractionState.RECEIVED, 1, null, decision?.DecisionId, null, null, false, now, now)
        { DecisionResponse = evidence.DecisionResponse };
        await SaveAsync(connection, transaction, value, insert: true, token);
        AfterInteractionInsert?.Invoke(); // Deterministic rollback/cancellation tests; no production injection.
        if (decision is not null)
        {
            // Relationship is not an answer. D/E must classify the real response
            // before atomically sealing its answered/superseded disposition.
            decision = decision with { RelatedInteractionId = value.InteractionId, Revision = decision.Revision + 1 };
            await SaveDecisionAsync(connection, transaction, decision, insert: false, token);
            var origin = await ReadAsync(connection, transaction, scope.Key, decision.OriginInteractionId, null, token)
                ?? throw Error("PLAYER_INTERACTION_RECOVERY_FAILED", "The decision's originating interaction requires recovery.");
            if (origin.State != PlayerInteractionState.AWAITING_PLAYER_INPUT || origin.PersistenceUnknown || origin.BindingId != binding.BindingId)
                throw Error("PLAYER_DECISION_CONFLICT", "The original decision is not safely yielded in this binding. No choice was inferred.");
            await SaveAsync(connection, transaction, origin with { SuccessorInteractionId = value.InteractionId,
                Revision = origin.Revision + 1, UpdatedAt = now }, insert: false, token);
        }
        return new PlayerInteractionAcceptance(await StatusAsync(connection, transaction, scope, value, token), false);
    }, token);

    public Task<PlayerInteractionStatus?> RecoverAsync(CampaignBindingScope scope, string? interactionId, CancellationToken token) =>
        TransactionAsync(scope, async (connection, transaction) =>
        {
            var value = await ReadAsync(connection, transaction, scope.Key, interactionId, null, token);
            if (value is null) return null;
            await RequireAccessAsync(connection, transaction, scope, value, token);
            return await StatusAsync(connection, transaction, scope, value, token);
        }, token);

    // These internal owner-specific operations are for trusted service coordinators,
    // never model-facing tools. No method in C can grant OPEN or seal gameplay completion.
    internal Task<PlayerInteractionStatus> PrepareEntryAsync(CampaignBindingScope scope, string interactionId,
        long revision, string requestId, CancellationToken token) => ChangeAsync(scope, interactionId, revision, requestId,
            "EntryPending", null, PlayerInteractionOwner.Entry, PlayerInteractionState.ENTRY_PENDING, token);

    internal Task<PlayerInteractionStatus> BlockAsync(CampaignBindingScope scope, string interactionId,
        long revision, string requestId, CancellationToken token) => ChangeAsync(scope, interactionId, revision, requestId,
            "Block", null, PlayerInteractionOwner.Recovery, PlayerInteractionState.BLOCKED, token);

    internal Task<PlayerInteractionStatus> ResumeEntryAsync(CampaignBindingScope scope, string interactionId,
        long revision, string requestId, CancellationToken token) => ChangeAsync(scope, interactionId, revision, requestId,
            "ResumeEntry", null, PlayerInteractionOwner.Recovery, PlayerInteractionState.ENTRY_PENDING, token);

    internal Task<PlayerInteractionStatus> ClarifyAsync(CampaignBindingScope scope, string interactionId,
        long revision, string requestId, string questionReference, string responseScopeReference, CancellationToken token) =>
        ChangeAsync(scope, interactionId, revision, requestId, "Clarify", new(questionReference, responseScopeReference, null, null),
            PlayerInteractionOwner.Entry, PlayerInteractionState.AWAITING_PLAYER_INPUT, token);

    internal Task<PlayerInteractionStatus> CancelAsync(CampaignBindingScope scope, string interactionId,
        long revision, string requestId, string? abandonDecisionId, long? decisionRevision, CancellationToken token) =>
        ChangeAsync(scope, interactionId, revision, requestId, "Cancel", new(null, null, abandonDecisionId, decisionRevision),
            PlayerInteractionOwner.Recovery, PlayerInteractionState.CANCELLED, token);

    private sealed record ChangeEvidence(string? Question, string? ResponseScope, string? DecisionId, long? DecisionRevision);

    private Task<PlayerInteractionStatus> ChangeAsync(CampaignBindingScope scope, string id, long revision, string requestId,
        string action, ChangeEvidence? evidence, PlayerInteractionOwner owner, PlayerInteractionState target, CancellationToken token) =>
        TransactionAsync(scope, async (connection, transaction) =>
        {
            if (!PlayerInteractionService.Bounded(id, 128) || !PlayerInteractionService.Bounded(requestId, 128) || revision <= 0)
                throw Error("PLAYER_INTERACTION_TRANSITION_INVALID", "A bounded interaction, request identity and expected revision are required.");
            var value = await ReadAsync(connection, transaction, scope.Key, id, null, token)
                ?? throw Error("PLAYER_INTERACTION_RECOVERY_FAILED", "The interaction is unavailable in this authorized session.");
            var binding = await RequireAccessAsync(connection, transaction, scope, value, token);
            var fingerprint = PlayerInteractionService.Hash(new { id, revision, action, evidence });
            await using (var receipt = Command(connection, transaction, """
                SELECT request_hash FROM {{schema}}.player_interaction_receipts WHERE scope_key = @scope AND request_id = @request;
                """))
            {
                receipt.Parameters.AddWithValue("@scope", scope.Key); receipt.Parameters.AddWithValue("@request", requestId);
                if (await receipt.ExecuteScalarAsync(token) is string saved)
                {
                    if (saved != fingerprint) throw Error("SUBMISSION_IDENTITY_CONFLICT", "The lifecycle request identity has a conflicting payload.");
                    return await StatusAsync(connection, transaction, scope, value, token);
                }
            }
            if (value.Revision != revision) throw Error("PLAYER_INTERACTION_STALE", "Recover the interaction's current revision before changing it.");
            if (!PlayerInteractionLifecycle.Allows(value.State, target, owner) ||
                action == "ResumeEntry" && value.RecoveryState is not (PlayerInteractionState.RECEIVED or PlayerInteractionState.ENTRY_PENDING))
                throw Error("PLAYER_INTERACTION_TRANSITION_INVALID", "This owner cannot perform the requested lifecycle transition.");
            if (action is "EntryPending" or "ResumeEntry" or "Clarify")
            {
                var current = await bindings.EvidenceAsync(connection, transaction, scope, value.CampaignId, token);
                if (binding.State != CampaignBindingState.ACTIVE || binding.SuccessorBindingId is not null || current?.Ready != true ||
                    !value.StartingEvidence.SameProfile(current) || current.CampaignVersion != value.StartingEvidence.CampaignVersion)
                    throw Error("PLAYER_INTERACTION_STALE", "The original baseline or binding is no longer valid. Reconcile it without rebasing submitted work.");
            }
            if (action is "Cancel" or "Clarify")
                await RequireKnownPersistenceAsync(connection, transaction, scope, value, token);
            var next = value with { State = target, Revision = value.Revision + 1, UpdatedAt = DateTimeOffset.UtcNow,
                RecoveryState = target == PlayerInteractionState.BLOCKED ? value.State : null,
                PersistenceUnknown = value.PersistenceUnknown || target == PlayerInteractionState.BLOCKED && value.State == PlayerInteractionState.PERSISTING };
            if (action == "Clarify")
            {
                if (evidence is null || !PlayerInteractionService.Bounded(evidence.Question, 256) || !PlayerInteractionService.Bounded(evidence.ResponseScope, 256) ||
                    await PendingAsync(connection, transaction, scope.Key, value.BindingId, token) is not null)
                    throw Error("PLAYER_DECISION_CONFLICT", "A clarification requires protected references and no competing unresolved decision.");
                var decision = new PlayerDecisionRecord("PD-" + Guid.NewGuid().ToString("N"), scope.Key, value.BindingId, value.CampaignId,
                    value.InteractionId, value.StartingEvidence.CampaignVersion, evidence.Question!, evidence.ResponseScope!,
                    PlayerDecisionState.Pending, 1, null, value.CorrelationId);
                next = next with { PendingDecisionId = decision.DecisionId };
                await SaveDecisionAsync(connection, transaction, decision, insert: true, token);
            }
            if (action == "Cancel")
            {
                if (!interactionOptions.Value.AllowRecoveryCancellation)
                    throw Error("PLAYER_INTERACTION_TRANSITION_INVALID", "This deployment has not granted recovery cancellation authority.");
                var decision = await PendingAsync(connection, transaction, scope.Key, value.BindingId, token);
                if (decision?.OriginInteractionId == value.InteractionId)
                {
                    if (decision.DecisionId != evidence?.DecisionId || decision.Revision != evidence.DecisionRevision)
                        throw Error("PLAYER_DECISION_CONFLICT", "Explicit abandonment requires the original pending decision and its revision.");
                    if (decision.RelatedInteractionId is not null)
                    {
                        var related = await ReadAsync(connection, transaction, scope.Key, decision.RelatedInteractionId, null, token);
                        if (related?.State != PlayerInteractionState.CANCELLED || related.PersistenceUnknown)
                            throw Error("PLAYER_DECISION_CONFLICT", "Close the related response safely before abandoning its originating decision.");
                    }
                    await SaveDecisionAsync(connection, transaction, decision with { State = PlayerDecisionState.Abandoned, Revision = decision.Revision + 1 }, false, token);
                }
                else if (evidence?.DecisionId is not null)
                    throw Error("PLAYER_DECISION_CONFLICT", "Cancellation cannot abandon another interaction's decision.");
            }
            await SaveAsync(connection, transaction, next, insert: false, token);
            await using var record = Command(connection, transaction, """
                INSERT INTO {{schema}}.player_interaction_receipts (scope_key, request_id, request_hash, interaction_id, transition_owner,
                    previous_state, resulting_state, resulting_revision)
                VALUES (@scope, @request, @hash, @id, @owner, @previous, @next, @revision);
                """);
            record.Parameters.AddWithValue("@scope", scope.Key); record.Parameters.AddWithValue("@request", requestId);
            record.Parameters.AddWithValue("@hash", fingerprint); record.Parameters.AddWithValue("@id", id);
            record.Parameters.AddWithValue("@owner", owner.ToString()); record.Parameters.AddWithValue("@revision", next.Revision);
            record.Parameters.AddWithValue("@previous", value.State.ToString()); record.Parameters.AddWithValue("@next", next.State.ToString());
            await record.ExecuteNonQueryAsync(token);
            return await StatusAsync(connection, transaction, scope, next, token);
        }, token);

    private async Task RequireKnownPersistenceAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, PlayerInteractionRecord value, CancellationToken token)
    {
        var evidence = await bindings.EvidenceAsync(connection, transaction, scope, value.CampaignId, token);
        if (value.PersistenceUnknown || evidence is null || evidence.PendingTransactionId is not null)
            throw Error("PLAYER_INTERACTION_PERSISTENCE_UNRESOLVED", "Reconcile the original persistence outcome before yielding or cancelling. Committed effects are never erased.");
    }

    private async Task<CampaignSessionBinding> RequireAccessAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, PlayerInteractionRecord value, CancellationToken token)
    {
        if (!scope.Allows(value.CampaignId)) throw Error("INTERACTION_BINDING_UNAVAILABLE", "The interaction is unavailable in this authorized session.");
        var binding = await bindings.ReadAsync(connection, transaction, scope.Key, value.BindingId, token);
        if (binding is null || binding.Generation != value.BindingGeneration || binding.CampaignId != value.CampaignId ||
            binding.Evidence.AuthorityId != scope.AuthorityId)
            throw Error("INTERACTION_BINDING_UNAVAILABLE", "The interaction's immutable binding does not match this authorized session.");
        return binding;
    }

    private async Task<PlayerInteractionStatus> StatusAsync(SqlConnection connection, SqlTransaction transaction,
        CampaignBindingScope scope, PlayerInteractionRecord value, CancellationToken token)
    {
        var binding = await RequireAccessAsync(connection, transaction, scope, value, token);
        var currentBinding = await bindings.ReadAsync(connection, transaction, scope.Key, null, token);
        if (currentBinding is not null && !scope.Allows(currentBinding.CampaignId)) currentBinding = null;
        var current = await bindings.EvidenceAsync(connection, transaction, scope, value.CampaignId, token);
        var valid = binding.State == CampaignBindingState.ACTIVE && binding.SuccessorBindingId is null && current?.Ready == true && binding.Evidence.SameProfile(current);
        var decision = await PendingAsync(connection, transaction, scope.Key, value.BindingId, token);
        var visibleDecision = decision;
        var reference = value.PendingDecisionId ?? value.RespondingToDecisionId;
        if (reference is not null && visibleDecision?.DecisionId != reference)
        {
            // Historical lookup must not substitute a newer pending question for
            // this interaction's own decision, even within the same binding.
            await using var archived = Command(connection, transaction, "SELECT decision_json FROM {{schema}}.player_pending_decisions WHERE scope_key = @scope AND decision_id = @id AND binding_id = @binding;");
            archived.Parameters.AddWithValue("@scope", scope.Key); archived.Parameters.AddWithValue("@id", reference);
            archived.Parameters.AddWithValue("@binding", value.BindingId);
            if (await archived.ExecuteScalarAsync(token) is not string json)
                throw Error("PLAYER_INTERACTION_RECOVERY_FAILED", "The interaction's original decision relationship requires recovery.");
            visibleDecision = JsonSerializer.Deserialize<PlayerDecisionRecord>(json, Json);
        }
        var blocked = await BlockingInteractionAsync(connection, transaction, scope.Key, value.BindingId, token);
        var persistenceKnown = current is not null && current.PendingTransactionId is null && !value.PersistenceUnknown;
        var entryReceipt = entryOptions?.Value.Enabled == true ? (await ReadEntryAsync(connection, transaction, scope.Key, value.InteractionId, null, token))?.Receipt : null;
        return new PlayerInteractionStatus(value.InteractionId, value.BindingId, value.CampaignId, value.BindingGeneration, value.StartingEvidence.CampaignVersion,
            value.State, value.Revision, value.CorrelationId, PlayerInteractionLifecycle.Yielded(value.State), valid,
            valid && !blocked && persistenceKnown, blocked || decision is not null || !persistenceKnown,
            visibleDecision is null ? null : new(visibleDecision.DecisionId, visibleDecision.OriginInteractionId, visibleDecision.State, visibleDecision.Revision, visibleDecision.RelatedInteractionId),
            value.RespondingToDecisionId, value.SuccessorInteractionId, value.Origin, value.SubmissionHash, current?.PendingTransactionId,
            currentBinding?.BindingId, currentBinding?.State)
        { GameplayEntryCompleted = entryReceipt is not null, GameplayEntryReceiptId = entryReceipt?.ReceiptId,
            Gameplay = value.Gameplay is null ? null : new(value.Gameplay.Baseline.CampaignVersion, value.Gameplay.PendingTransactionId,
                Array.AsReadOnly(value.Gameplay.Admissions.Where(item => item.TransactionId != value.Gameplay.PendingTransactionId).Select(item => item.TransactionId).ToArray()),
                value.State == PlayerInteractionState.COMPLETED && value.Gameplay.CompletedNarrationAuthorized,
                value.State == PlayerInteractionState.AWAITING_PLAYER_INPUT && value.Gameplay.QuestionPresentationAuthorized && visibleDecision?.State == PlayerDecisionState.Pending) };
    }

    private async Task<T> TransactionAsync<T>(CampaignBindingScope scope, Func<SqlConnection, SqlTransaction, Task<T>> execute, CancellationToken token)
    {
        scope.Validate();
        if (!interactionOptions.Value.Enabled) throw Error("TRUSTED_SUBMISSION_REQUIRED", "Trusted interaction storage is not configured.");
        try
        {
            await using var connection = new SqlConnection(settings.ConnectionString);
            await connection.OpenAsync(token);
            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.Serializable, token);
            await SqlServerCampaignBindingStore.LockSessionAsync(connection, transaction, scope.Key, token);
            var result = await execute(connection, transaction);
            token.ThrowIfCancellationRequested();
            await transaction.CommitAsync(token);
            return result;
        }
        catch (SqlException) when (token.IsCancellationRequested) { throw new OperationCanceledException(token); }
        catch (SqlException error) when (error.Number is 207 or 208)
        { throw new ManagedServiceException("PLAYER_INTERACTION_SCHEMA_REQUIRED", "Preview and apply the supported trusted-interaction migration before use.", error); }
        catch (SqlException error) when (error.Number is 1205 or 2601 or 2627)
        { throw new ManagedServiceException("PLAYER_INTERACTION_STALE", "Interaction ownership changed concurrently. Recover the same submission and revision.", error); }
        catch (SqlException error)
        { throw new ManagedServiceException("PLAYER_INTERACTION_RECOVERY_FAILED", "Interaction persistence requires recovery using the original trusted identity.", error); }
        catch (JsonException error)
        { throw new ManagedServiceException("PLAYER_INTERACTION_RECOVERY_FAILED", "The stored interaction requires integrity recovery; no gameplay authority was granted.", error); }
    }

    private async Task<PlayerInteractionRecord?> ReadAsync(SqlConnection connection, SqlTransaction transaction,
        string scope, string? id, string? submissionHash, CancellationToken token)
    {
        await using var command = Command(connection, transaction, """
            SELECT TOP (1) interaction_json FROM {{schema}}.player_interactions WHERE scope_key = @scope
              AND (@id IS NULL OR interaction_id = @id) AND (@hash IS NULL OR submission_hash = @hash)
            ORDER BY sequence_number DESC;
            """);
        command.Parameters.AddWithValue("@scope", scope); command.Parameters.AddWithValue("@id", (object?)id ?? DBNull.Value);
        command.Parameters.AddWithValue("@hash", (object?)submissionHash ?? DBNull.Value);
        return await command.ExecuteScalarAsync(token) is string json ? JsonSerializer.Deserialize<PlayerInteractionRecord>(json, Json) : null;
    }

    private async Task<PlayerDecisionRecord?> PendingAsync(SqlConnection connection, SqlTransaction transaction,
        string scope, string binding, CancellationToken token)
    {
        await using var command = Command(connection, transaction, """
            SELECT decision_json FROM {{schema}}.player_pending_decisions
            WHERE scope_key = @scope AND binding_id = @binding AND decision_status = N'Pending';
            """);
        command.Parameters.AddWithValue("@scope", scope); command.Parameters.AddWithValue("@binding", binding);
        return await command.ExecuteScalarAsync(token) is string json ? JsonSerializer.Deserialize<PlayerDecisionRecord>(json, Json) : null;
    }

    private async Task RequireNoActiveAsync(SqlConnection connection, SqlTransaction transaction, string scope, string binding, CancellationToken token)
    {
        if (await BlockingInteractionAsync(connection, transaction, scope, binding, token))
            throw Error("PLAYER_INTERACTION_ACTIVE", "Complete or safely cancel the existing interaction before accepting another actual submission.");
    }

    private async Task<bool> BlockingInteractionAsync(SqlConnection connection, SqlTransaction transaction, string scope, string binding, CancellationToken token)
    {
        await using var command = Command(connection, transaction, """
            SELECT CASE WHEN EXISTS (SELECT 1 FROM {{schema}}.player_interactions AS interactions WHERE scope_key = @scope AND binding_id = @binding
                AND {{blocking}}) THEN 1 ELSE 0 END;
            """.Replace("{{blocking}}", BlockingPredicate, StringComparison.Ordinal));
        command.Parameters.AddWithValue("@scope", scope); command.Parameters.AddWithValue("@binding", binding);
        return Convert.ToInt32(await command.ExecuteScalarAsync(token)) == 1;
    }

    private async Task SaveAsync(SqlConnection connection, SqlTransaction transaction, PlayerInteractionRecord value, bool insert, CancellationToken token)
    {
        if (JsonSerializer.Serialize(value, Json).Length > 65_536)
            throw Error("GAMEPLAY_AUTHORITY_INVALID", "Interaction evidence exceeds its supported bound; narrow the work unit.");
        await using var command = Command(connection, transaction, insert ? """
            INSERT INTO {{schema}}.player_interactions
                (scope_key, interaction_id, submission_hash, submission_fingerprint, binding_id, binding_generation, campaign_id,
                 interaction_state, revision, persistence_unknown, interaction_json)
            VALUES (@scope, @id, @hash, @fingerprint, @binding, @generation, @campaign, @state, @revision, @unknown, @json);
            """ : """
            UPDATE {{schema}}.player_interactions SET interaction_state = @state, revision = @revision,
                persistence_unknown = @unknown, interaction_json = @json
            WHERE scope_key = @scope AND interaction_id = @id AND binding_id = @binding AND binding_generation = @generation
              AND campaign_id = @campaign AND submission_fingerprint = @fingerprint AND revision = @revision - 1;
            """);
        command.Parameters.AddWithValue("@scope", value.ScopeKey); command.Parameters.AddWithValue("@id", value.InteractionId);
        command.Parameters.AddWithValue("@hash", value.SubmissionHash); command.Parameters.AddWithValue("@fingerprint", value.SubmissionFingerprint);
        command.Parameters.AddWithValue("@binding", value.BindingId); command.Parameters.AddWithValue("@generation", value.BindingGeneration);
        command.Parameters.AddWithValue("@campaign", value.CampaignId); command.Parameters.AddWithValue("@state", value.State.ToString());
        command.Parameters.AddWithValue("@revision", value.Revision); command.Parameters.AddWithValue("@unknown", value.PersistenceUnknown);
        command.Parameters.AddWithValue("@json", JsonSerializer.Serialize(value, Json));
        if (await command.ExecuteNonQueryAsync(token) != 1) throw Error("PLAYER_INTERACTION_STALE", "The interaction's immutable ownership or revision no longer matches.");
    }

    private async Task SaveDecisionAsync(SqlConnection connection, SqlTransaction transaction, PlayerDecisionRecord value, bool insert, CancellationToken token)
    {
        await using var command = Command(connection, transaction, insert ? """
            INSERT INTO {{schema}}.player_pending_decisions
                (scope_key, decision_id, binding_id, campaign_id, origin_interaction_id, related_interaction_id, decision_status, revision, decision_json)
            VALUES (@scope, @id, @binding, @campaign, @origin, @related, @status, @revision, @json);
            """ : """
            UPDATE {{schema}}.player_pending_decisions SET related_interaction_id = @related, decision_status = @status, revision = @revision, decision_json = @json
            WHERE scope_key = @scope AND decision_id = @id AND binding_id = @binding AND campaign_id = @campaign AND origin_interaction_id = @origin AND revision = @revision - 1;
            """);
        command.Parameters.AddWithValue("@scope", value.ScopeKey); command.Parameters.AddWithValue("@id", value.DecisionId);
        command.Parameters.AddWithValue("@binding", value.BindingId); command.Parameters.AddWithValue("@origin", value.OriginInteractionId);
        command.Parameters.AddWithValue("@campaign", value.CampaignId);
        command.Parameters.AddWithValue("@related", (object?)value.RelatedInteractionId ?? DBNull.Value);
        command.Parameters.AddWithValue("@status", value.State.ToString()); command.Parameters.AddWithValue("@revision", value.Revision);
        command.Parameters.AddWithValue("@json", JsonSerializer.Serialize(value, Json));
        if (await command.ExecuteNonQueryAsync(token) != 1) throw Error("PLAYER_DECISION_CONFLICT", "The decision's ownership or revision changed.");
    }

    private SqlCommand Command(SqlConnection connection, SqlTransaction transaction, string sql) =>
        new(SqlServerSchemaIdentifier.Bind(sql, settings.DomainSchema), connection, transaction) { CommandTimeout = settings.CommandTimeoutSeconds };
    private static ManagedServiceException Error(string code, string message) => new(code, message);
}

public sealed class SqlPlayerInteractionSwitchSafety(IOptions<SqlServerPersistenceOptions> options,
    IOptions<PlayerInteractionOptions> interactionOptions) : ISqlCampaignBindingSwitchSafety
{
    public async Task<CampaignSwitchSafety> CheckAsync(SqlConnection connection, SqlTransaction transaction, CampaignSessionBinding binding, CancellationToken token)
    {
        if (!interactionOptions.Value.Enabled) return CampaignSwitchSafety.Unknown;
        // Called by B under its session lock, the same lock used by intake and
        // every lifecycle mutation. This is not a racing preflight check.
        await using var schema = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@interactions, 'U') IS NOT NULL AND OBJECT_ID(@decisions, 'U') IS NOT NULL THEN 1 ELSE 0 END;", connection, transaction);
        schema.Parameters.AddWithValue("@interactions", options.Value.DomainSchema + ".player_interactions");
        schema.Parameters.AddWithValue("@decisions", options.Value.DomainSchema + ".player_pending_decisions");
        if (Convert.ToInt32(await schema.ExecuteScalarAsync(token)) != 1) return CampaignSwitchSafety.Unknown;
        await using var command = new SqlCommand(SqlServerSchemaIdentifier.Bind("""
            IF EXISTS (SELECT 1 FROM {{schema}}.player_interactions AS interactions WHERE scope_key = @scope AND binding_id = @binding
                AND {{blocking}}) SELECT 1;
            ELSE IF EXISTS (SELECT 1 FROM {{schema}}.player_pending_decisions WHERE scope_key = @scope AND binding_id = @binding AND decision_status = N'Pending') SELECT 2;
            ELSE SELECT 0;
            """.Replace("{{blocking}}", SqlServerPlayerInteractionStore.BlockingPredicate, StringComparison.Ordinal), options.Value.DomainSchema), connection, transaction) { CommandTimeout = options.Value.CommandTimeoutSeconds };
        command.Parameters.AddWithValue("@scope", binding.ScopeKey); command.Parameters.AddWithValue("@binding", binding.BindingId);
        var result = Convert.ToInt32(await command.ExecuteScalarAsync(token));
        if (result == 1) throw new ManagedServiceException("BINDING_SWITCH_INTERACTION_BLOCKED", "Close or reconcile the original interaction before switching campaigns.");
        if (result == 2) throw new ManagedServiceException("BINDING_SWITCH_DECISION_BLOCKED", "Explicitly resolve or abandon the original pending player decision before switching.");
        return result == 0 ? CampaignSwitchSafety.Clear : CampaignSwitchSafety.Unknown;
    }
}
