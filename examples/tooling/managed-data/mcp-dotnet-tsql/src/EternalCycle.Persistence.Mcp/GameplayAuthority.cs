using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public enum GameplayMutationAction { Update, Create, Tombstone }
public sealed record GameplayMutationPermission(RecordAddress Address, GameplayMutationAction Action);
public enum PlayerResponseDisposition { Choice, Rejection, Clarification, Correction, ChangeDirection }
public sealed record TrustedDecisionResponse(PlayerResponseDisposition Disposition, string ResponseReference,
    string AllowedResponseScopeReference, string? SelectionReference = null);

public sealed class GameplayAuthorityOptions
{
    // A separate administrative deployment, not a caller-selectable bypass.
    // Never combine this grant with an enabled gameplay session.
    public string[] AdministrativeCampaignIds { get; init; } = [];
}

public sealed record GameplayConclusionRequest(string RequestId, string InteractionId, long ExpectedRevision,
    IReadOnlyList<string>? AffectedOwnerDomains)
{
    public string? QuestionReference { get; init; }
    public string? ResponseScopeReference { get; init; }
}
public sealed record GameplayDecisionRequest(string RequestId, string InteractionId, long ExpectedRevision,
    string DecisionId, long ExpectedDecisionRevision);
public sealed record GameplayOwnerReadRequest(string InteractionId, long ExpectedRevision, IReadOnlyList<RecordAddress> Records);
public sealed record GameplayOwnerReadResult(PlayerInteractionStatus Interaction, IReadOnlyList<CanonicalRecord> Records);
public sealed record GameplayDisposition(PlayerInteractionStatus Interaction, bool CompletedNarrationAuthorized,
    bool QuestionPresentationAuthorized, bool EmptyAffectedSetValidated, long CampaignVersion,
    IReadOnlyList<PersistenceReceipt> Receipts);
public sealed record GameplayProgressStatus(long CampaignVersion, string? PendingTransactionId,
    IReadOnlyList<string> ValidatedTransactionIds, bool CompletedNarrationAuthorized, bool QuestionPresentationAuthorized);

internal sealed record GameplayOwnerEvidence(RecordAddress Address, long? Revision, string? PayloadHash, bool Tombstone);
internal sealed record GameplayAdmission(string TransactionId, string RequestHash);
internal sealed record GameplayProgress(CampaignBindingEvidence Baseline, IReadOnlyList<GameplayOwnerEvidence> Reads,
    IReadOnlyList<GameplayAdmission> Admissions, string? PendingTransactionId, bool CompletedNarrationAuthorized = false,
    bool QuestionPresentationAuthorized = false, bool EmptyAffectedSetValidated = false);

internal static class GameplayAuthority
{
    internal static CampaignBindingScope Scope(CampaignBindingOptions policy)
    {
        if (!policy.Enabled) throw Error("GAMEPLAY_ENTRY_REQUIRED", "A trusted gameplay session and mandatory entry are required before Canon mutation.");
        var scope = new CampaignBindingScope(policy.PrincipalId, policy.LogicalSessionId, policy.AuthorityId,
            Array.AsReadOnly(policy.AuthorizedCampaignIds.ToArray()), policy.AllowSoleCampaignResume, policy.AllowSelection, policy.AllowSwitch);
        scope.Validate(); return scope;
    }
    internal static bool ValidPermissions(IReadOnlyList<GameplayMutationPermission>? permissions, RecordAddress session) =>
        permissions is not null && permissions.Count <= 32 && permissions.All(permission => permission is not null &&
            GameplayEntryCanonReader.ValidAddress(permission.Address) && permission.Address != session && Enum.IsDefined(permission.Action)) &&
        permissions.Distinct().Count() == permissions.Count;
    internal static bool ValidResponse(TrustedDecisionResponse? response, string? decision) => response is null ||
        decision is not null && Enum.IsDefined(response.Disposition) && PlayerInteractionService.Bounded(response.ResponseReference, 256) &&
        PlayerInteractionService.Bounded(response.AllowedResponseScopeReference, 256) &&
        (response.Disposition == PlayerResponseDisposition.Choice ? PlayerInteractionService.Bounded(response.SelectionReference, 256) : response.SelectionReference is null);
    internal static ManagedServiceException Error(string code, string message) => new(code, message);
}

public sealed class GameplayAuthorityService(SqlServerPlayerInteractionStore store, PersistenceCoordinator persistence,
    IOptions<CampaignBindingOptions> options, IManagedDiagnosticRecorder? diagnostics = null)
{
    public Task<ManagedOperationResult<GameplayDisposition>> ConcludeAsync(GameplayConclusionRequest request, CancellationToken token)
    {
        if (request is not null) request = request with { AffectedOwnerDomains = request.AffectedOwnerDomains is null ? null : Array.AsReadOnly(request.AffectedOwnerDomains.ToArray()) };
        return RunAsync("Conclude", request?.InteractionId, request?.RequestId, scope => store.ConcludeGameplayAsync(scope, request!, token), token);
    }
    public Task<ManagedOperationResult<GameplayDisposition>> ResolveDecisionAsync(GameplayDecisionRequest request, CancellationToken token) =>
        RunAsync("DecisionResponse", request?.InteractionId, request?.RequestId, scope => store.ResolveGameplayDecisionAsync(scope, request!, token), token);
    public Task<ManagedOperationResult<GameplayOwnerReadResult>> ReadOwnersAsync(GameplayOwnerReadRequest request, CancellationToken token)
    {
        if (request is not null && request.Records is not null) request = request with { Records = Array.AsReadOnly(request.Records.ToArray()) };
        return RunAsync("OwnerRead", request?.InteractionId, null, scope => store.ReadGameplayOwnersAsync(scope, request!, token), token);
    }
    public Task<ManagedOperationResult<CommitResult>> ReconcileAsync(string interactionId, string transactionId, CancellationToken token) =>
        RunAsync("Reconcile", interactionId, transactionId, async scope =>
        {
            var status = await store.RequireGameplayTransactionAsync(scope, interactionId, transactionId, token);
            return await persistence.RetryAsync(status.CampaignId, transactionId, token);
        }, token);

    private async Task<ManagedOperationResult<T>> RunAsync<T>(string phase, string? interaction, string? request,
        Func<CampaignBindingScope, Task<T>> execute, CancellationToken token)
    {
        var correlation = McpToolFailureContract.NewCorrelationId(); var started = DateTimeOffset.UtcNow;
        Exception? failure = null; ManagedOperationResult<T> result;
        try { result = new(true, "GAMEPLAY_AUTHORITY_VALIDATED", "The service validated the requested interaction disposition.", await execute(GameplayAuthority.Scope(options.Value)), RetrySafe: true); }
        catch (OperationCanceledException) { throw; }
        catch (ManagedServiceException error) { failure = error; result = new(false, error.Code, error.SafeMessage, default, RetrySafe: true); }
        if (result.Data is CommitResult { Marker: not PersistenceMarkers.Saved })
            result = result with { Success = false, Code = "PLAYER_INTERACTION_PERSISTENCE_UNRESOLVED",
                Message = "The original persistence outcome remains unvalidated. Reconcile it before dependent completion." };
        result = result with { CorrelationId = correlation, Operation = "GameplayAuthority", Stage = phase };
        if (diagnostics is not null)
        {
            try
            {
                var disposition = result.Data as GameplayDisposition;
                var ownerRead = result.Data as GameplayOwnerReadResult;
                var commit = result.Data as CommitResult;
                var status = disposition?.Interaction ?? ownerRead?.Interaction;
                var context = new ManagedDiagnosticContext(correlation, "GameplayAuthority", RulePublicationStage.ToolInvocation, result.Code, started,
                    CampaignId: status?.CampaignId ?? commit?.Receipt?.CampaignId, OperationId: interaction,
                    Outcome: failure is not null || commit?.Marker == PersistenceMarkers.Failed ? "Failed" : result.Success ? "Succeeded" : "Blocked", RetrySafe: true,
                    SafeDetail: JsonSerializer.Serialize(new { RequestId = request, InteractionId = interaction, Phase = phase,
                        status?.BindingId, status?.State, status?.Revision,
                        status?.GameplayEntryReceiptId, OriginalCorrelationId = status?.CorrelationId,
                        status?.Yielded, status?.PendingDecision, PersistenceOutcome = commit?.Status,
                        PersistenceFailureType = commit?.FailureCause?.GetType().Name,
                        OwnerIds = ownerRead?.Records.Select(record => new { record.OwnerDomain, record.RecordId, record.RecordRevision }),
                        disposition?.CompletedNarrationAuthorized, disposition?.QuestionPresentationAuthorized, disposition?.CampaignVersion,
                        ReceiptIds = disposition?.Receipts.Select(receipt => receipt.ReceiptId) }));
                if (failure is null) await diagnostics.RecordEventAsync(context, token); else await diagnostics.RecordFailureAsync(context, failure, token);
            }
            catch (Exception error) when (error is not OutOfMemoryException) { /* Diagnostic loss cannot manufacture or revoke authority. */ }
        }
        return result;
    }
}

[McpServerToolType]
public sealed class GameplayAuthorityTools(GameplayAuthorityService service)
{
    [McpServerTool(Name = "ec_conclude_gameplay_interaction", Idempotent = true),
     Description("Verify the explicit affected set and every durable receipt before completed narration or question presentation. Question references record a pending decision and revoke forward authority before presentation. No caller completion flag is accepted.")]
    public Task<ManagedOperationResult<GameplayDisposition>> ConcludeAsync(GameplayConclusionRequest request, CancellationToken token) => service.ConcludeAsync(request, token);
    [McpServerTool(Name = "ec_resolve_player_decision", Idempotent = true),
     Description("Classify only an existing trusted response after mandatory entry. Cannot accept a model-authored choice. Clarification leaves alternatives pending; the original interaction stays yielded.")]
    public Task<ManagedOperationResult<GameplayDisposition>> ResolveAsync(GameplayDecisionRequest request, CancellationToken token) => service.ResolveDecisionAsync(request, token);
    [McpServerTool(Name = "ec_read_gameplay_owners"),
     Description("Read exact additional owners already authorized by the canonical interaction scope. Records service-owned version/hash evidence; never expands intent or chooses an alternative.")]
    public Task<ManagedOperationResult<GameplayOwnerReadResult>> ReadAsync(GameplayOwnerReadRequest request, CancellationToken token) => service.ReadOwnersAsync(request, token);
    [McpServerTool(Name = "ec_reconcile_gameplay_persistence", Idempotent = true),
     Description("Reconcile the original admitted transaction. No replacement effect, campaign or interaction is created. A validated save does not by itself authorize completed narration.")]
    public Task<ManagedOperationResult<CommitResult>> ReconcileAsync(string interactionId, string transactionId, CancellationToken token) => service.ReconcileAsync(interactionId, transactionId, token);
}
