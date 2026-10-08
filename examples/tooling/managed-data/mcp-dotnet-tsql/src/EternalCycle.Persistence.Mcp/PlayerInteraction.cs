using System.ComponentModel;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public enum PlayerInteractionState { RECEIVED, ENTRY_PENDING, OPEN, PERSISTING, AWAITING_PLAYER_INPUT, COMPLETED, BLOCKED, CANCELLED }
public enum TrustedSubmissionOrigin { HostUserEvent, HumanIntegration }
public enum PlayerDecisionState { Pending, Resolved, Abandoned }

public sealed class PlayerInteractionOptions
{
    public bool Enabled { get; init; }
    public bool AllowRecoveryCancellation { get; init; }
}

// Only an authorized host adapter implements this boundary. A tool argument,
// configured session identity, or a model-authored token is not an attestation.
public interface ITrustedPlayerSubmissionIngress
{
    Task<TrustedPlayerSubmission?> ResolveAsync(string deliveryReference, CancellationToken token);
}

public sealed record TrustedPlayerSubmission(string PrincipalId, string LogicalSessionId, string AuthorityId,
    string SubmissionId, string BindingId, TrustedSubmissionOrigin Origin, string ProvenanceReference,
    string InputReference, string InputSha256, DateTimeOffset SubmittedAt,
    string? PendingDecisionId = null, long? PendingDecisionRevision = null);

// The normal storage entry point cannot take raw bytes or an unverified assertion.
// Only the service, after invoking the trusted adapter, constructs this input.
public sealed class VerifiedPlayerSubmission
{
    internal VerifiedPlayerSubmission(TrustedPlayerSubmission evidence) => Evidence = evidence;
    internal TrustedPlayerSubmission Evidence { get; }
}

public sealed record PlayerDecisionStatus(string DecisionId, string OriginInteractionId,
    PlayerDecisionState State, long Revision, string? RelatedInteractionId);

public sealed record PlayerInteractionStatus(string InteractionId, string BindingId, string CampaignId,
    long BindingGeneration, long StartingCampaignVersion, PlayerInteractionState State, long Revision,
    string CorrelationId, bool Yielded, bool BindingValid, bool MayReceiveSubmission, bool SwitchBlocked,
    PlayerDecisionStatus? PendingDecision, string? RespondingToDecisionId, string? SuccessorInteractionId,
    TrustedSubmissionOrigin TrustedOrigin, string SubmissionIdentityHash, string? PendingTransactionId,
    string? CurrentBindingId, CampaignBindingState? CurrentBindingState)
{
    // A durable D receipt records entry, never a general mutation grant (E).
    public bool GameplayEntryCompleted { get; init; }
    public bool GameplayMutationAuthorized => false;
}

public sealed record PlayerInteractionAcceptance(PlayerInteractionStatus Status, bool Reused);

public interface IPlayerInteractionStore
{
    Task<PlayerInteractionAcceptance> AcceptAsync(CampaignBindingScope scope, VerifiedPlayerSubmission submission,
        string correlationId, CancellationToken token);
    Task<PlayerInteractionStatus?> RecoverAsync(CampaignBindingScope scope, string? interactionId, CancellationToken token);
}

internal sealed record PlayerInteractionRecord(string InteractionId, string ScopeKey, string SubmissionHash,
    string SubmissionFingerprint, string BindingId, long BindingGeneration, string CampaignId,
    CampaignBindingEvidence StartingEvidence, TrustedSubmissionOrigin Origin, string ProvenanceReference,
    string InputReference, string InputSha256, DateTimeOffset SubmittedAt, string CorrelationId,
    PlayerInteractionState State, long Revision, string? PendingDecisionId, string? RespondingToDecisionId,
    string? SuccessorInteractionId, PlayerInteractionState? RecoveryState, bool PersistenceUnknown,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

internal sealed record PlayerDecisionRecord(string DecisionId, string ScopeKey, string BindingId,
    string CampaignId, string OriginInteractionId, long StartingCampaignVersion, string ProtectedQuestionReference,
    string AllowedResponseScopeReference, PlayerDecisionState State, long Revision, string? RelatedInteractionId,
    string CorrelationId);

internal enum PlayerInteractionOwner { Entry, Persistence, Yield, Recovery }

internal static class PlayerInteractionLifecycle
{
    internal static bool Yielded(PlayerInteractionState state) => state is PlayerInteractionState.AWAITING_PLAYER_INPUT
        or PlayerInteractionState.COMPLETED or PlayerInteractionState.BLOCKED or PlayerInteractionState.CANCELLED;

    // This validates ownership; it is not a public state setter. C's narrow store
    // methods cannot issue the OPEN/persistence/completion grants reserved for D/E.
    internal static bool Allows(PlayerInteractionState from, PlayerInteractionState to, PlayerInteractionOwner owner) => (from, to, owner) switch
    {
        (PlayerInteractionState.RECEIVED, PlayerInteractionState.ENTRY_PENDING, PlayerInteractionOwner.Entry) => true,
        (PlayerInteractionState.ENTRY_PENDING, PlayerInteractionState.OPEN, PlayerInteractionOwner.Entry) => true,
        (PlayerInteractionState.ENTRY_PENDING, PlayerInteractionState.AWAITING_PLAYER_INPUT, PlayerInteractionOwner.Entry) => true,
        (PlayerInteractionState.OPEN, PlayerInteractionState.PERSISTING, PlayerInteractionOwner.Persistence) => true,
        (PlayerInteractionState.PERSISTING, PlayerInteractionState.OPEN or PlayerInteractionState.COMPLETED or PlayerInteractionState.AWAITING_PLAYER_INPUT, PlayerInteractionOwner.Persistence) => true,
        (PlayerInteractionState.OPEN, PlayerInteractionState.COMPLETED or PlayerInteractionState.AWAITING_PLAYER_INPUT, PlayerInteractionOwner.Yield) => true,
        (PlayerInteractionState.RECEIVED or PlayerInteractionState.ENTRY_PENDING or PlayerInteractionState.OPEN or PlayerInteractionState.PERSISTING,
            PlayerInteractionState.BLOCKED, PlayerInteractionOwner.Recovery) => true,
        (PlayerInteractionState.BLOCKED, PlayerInteractionState.ENTRY_PENDING or PlayerInteractionState.OPEN or PlayerInteractionState.PERSISTING, PlayerInteractionOwner.Recovery) => true,
        (PlayerInteractionState.RECEIVED or PlayerInteractionState.ENTRY_PENDING or PlayerInteractionState.OPEN or PlayerInteractionState.PERSISTING or
            PlayerInteractionState.BLOCKED or PlayerInteractionState.AWAITING_PLAYER_INPUT, PlayerInteractionState.CANCELLED, PlayerInteractionOwner.Recovery) => true,
        _ => false
    };
}

public sealed class PlayerInteractionService(IPlayerInteractionStore store, IOptions<CampaignBindingOptions> bindingOptions,
    IOptions<PlayerInteractionOptions> options, ITrustedPlayerSubmissionIngress? ingress = null,
    IManagedDiagnosticRecorder? diagnostics = null)
{
    // Host-facing API only: deliberately not decorated or registered as an MCP tool.
    public Task<ManagedOperationResult<PlayerInteractionStatus>> AcceptAsync(string deliveryReference, CancellationToken token) =>
        ExecuteAsync("TrustedSubmission", async (scope, correlation) =>
        {
            if (ingress is null) throw new ManagedServiceException("TRUSTED_SUBMISSION_REQUIRED", "An authorized host must supply an actual player submission. Tool calls cannot create one.");
            if (!Bounded(deliveryReference, 128)) throw InvalidProvenance();
            var evidence = await ingress.ResolveAsync(deliveryReference, token);
            if (evidence is null) throw new ManagedServiceException("TRUSTED_SUBMISSION_REQUIRED", "No authenticated player submission was supplied by the host integration.");
            if (evidence.PrincipalId != scope.PrincipalId || evidence.LogicalSessionId != scope.LogicalSessionId || evidence.AuthorityId != scope.AuthorityId ||
                !Enum.IsDefined(evidence.Origin) || !Bounded(evidence.SubmissionId, 128) || !Bounded(evidence.BindingId, 128) ||
                !Bounded(evidence.ProvenanceReference, 256) || !Bounded(evidence.InputReference, 256) ||
                !Bounded(evidence.InputSha256, 64) || evidence.InputSha256.Length != 64 || evidence.InputSha256.Any(value => !char.IsAsciiHexDigit(value)) ||
                evidence.InputSha256 != evidence.InputSha256.ToUpperInvariant() || evidence.SubmittedAt == default ||
                (evidence.PendingDecisionId is null) != (evidence.PendingDecisionRevision is null) ||
                evidence.PendingDecisionId is not null && (!Bounded(evidence.PendingDecisionId, 128) || evidence.PendingDecisionRevision <= 0))
                throw InvalidProvenance();
            return await store.AcceptAsync(scope, new VerifiedPlayerSubmission(evidence), correlation, token);
        }, token);

    public Task<ManagedOperationResult<PlayerInteractionStatus>> RecoverAsync(string? interactionId, CancellationToken token) =>
        ExecuteAsync("Recover", async (scope, _) =>
        {
            if (interactionId is not null && !Bounded(interactionId, 128)) throw InvalidProvenance();
            var status = await store.RecoverAsync(scope, interactionId, token);
            if (status is null) throw new ManagedServiceException("TRUSTED_SUBMISSION_REQUIRED", "No player interaction is available in this authorized session. A trusted submission is required.");
            return new PlayerInteractionAcceptance(status, true);
        }, token);

    private async Task<ManagedOperationResult<PlayerInteractionStatus>> ExecuteAsync(string stage,
        Func<CampaignBindingScope, string, Task<PlayerInteractionAcceptance>> execute, CancellationToken token)
    {
        var correlation = McpToolFailureContract.NewCorrelationId();
        var started = DateTimeOffset.UtcNow;
        Exception? failure = null;
        ManagedOperationResult<PlayerInteractionStatus> result;
        try
        {
            if (!options.Value.Enabled || !bindingOptions.Value.Enabled)
                throw new ManagedServiceException("TRUSTED_SUBMISSION_REQUIRED", "Trusted player interaction support is not configured. No gameplay authority was granted.");
            var configuration = bindingOptions.Value;
            var scope = new CampaignBindingScope(configuration.PrincipalId, configuration.LogicalSessionId, configuration.AuthorityId,
                Array.AsReadOnly(configuration.AuthorizedCampaignIds.ToArray()), configuration.AllowSoleCampaignResume, configuration.AllowSelection, configuration.AllowSwitch);
            scope.Validate();
            var accepted = await execute(scope, correlation);
            result = new(true, stage == "Recover" ? "PLAYER_INTERACTION_RECOVERED" : accepted.Reused ? "PLAYER_INTERACTION_REUSED" : "PLAYER_INTERACTION_RECORDED",
                "Durable interaction evidence recovered; mandatory gameplay entry is not yet authorized.", accepted.Status, RetrySafe: true);
        }
        catch (OperationCanceledException) { throw; }
        catch (ManagedServiceException error)
        { failure = error; result = new(false, error.Code, error.SafeMessage, null, RetrySafe: true); }
        result = result with { CorrelationId = correlation, Operation = "PlayerInteraction", Stage = stage };
        if (diagnostics is not null)
        {
            var value = result.Data;
            try
            {
                var context = new ManagedDiagnosticContext(correlation, "PlayerInteraction", RulePublicationStage.ToolInvocation,
                    result.Code, started, CampaignId: value?.CampaignId, Outcome: result.Success ? "Succeeded" : "Blocked",
                    RetrySafe: true, OperationId: value?.InteractionId, SafeDetail: JsonSerializer.Serialize(new
                    { value?.BindingId, value?.InteractionId, value?.State, value?.Revision, value?.Yielded, value?.SwitchBlocked,
                        value?.PendingDecision, value?.TrustedOrigin, value?.SubmissionIdentityHash, value?.PendingTransactionId, IdempotencyOutcome = result.Code }));
                if (failure is null) await diagnostics.RecordEventAsync(context, token);
                else await diagnostics.RecordFailureAsync(context, failure, token);
            }
            catch (Exception error) when (error is not OutOfMemoryException) { /* Diagnostic loss cannot undo durable intake. */ }
        }
        return result;
    }

    internal static string Hash<T>(T value) => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(value)));
    internal static bool Bounded(string? value, int maximum) => !string.IsNullOrWhiteSpace(value) && value.Length <= maximum && !value.Any(char.IsControl);
    private static ManagedServiceException InvalidProvenance() => new("SUBMISSION_PROVENANCE_INVALID", "Trusted submission evidence is invalid or does not belong to this authorized session.");
}

[McpServerToolType]
public sealed class PlayerInteractionTools(PlayerInteractionService service)
{
    [McpServerTool(Name = "ec_get_player_interaction_status", ReadOnly = true, Idempotent = true),
     Description("Recover authorized durable player-interaction and pending-decision status. Cannot create a submission, select an option, transition state or grant gameplay entry.")]
    public Task<ManagedOperationResult<PlayerInteractionStatus>> GetAsync(string? interactionId = null, CancellationToken token = default) => service.RecoverAsync(interactionId, token);
}
