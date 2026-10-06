using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public enum CampaignBindingState { ACTIVE, SUSPENDED, CLOSED }
public enum CampaignBindingAction { Resolve, Select, Switch, Suspend }
public enum CampaignSwitchSafety { Unknown, Blocked, Clear }

// This reference adapter is configured by the deployment, never by tool arguments.
// It is not an attestation of a human submission (FR-027C).
public sealed class CampaignBindingOptions
{
    public bool Enabled { get; init; }
    public string PrincipalId { get; init; } = string.Empty;
    public string LogicalSessionId { get; init; } = string.Empty;
    public string AuthorityId { get; init; } = string.Empty;
    public string[] AuthorizedCampaignIds { get; init; } = [];
    public bool AllowSoleCampaignResume { get; init; }
    public bool AllowSelection { get; init; }
    public bool AllowSwitch { get; init; }
}

public sealed record CampaignBindingScope(string PrincipalId, string LogicalSessionId, string AuthorityId,
    IReadOnlyList<string> AuthorizedCampaignIds, bool AllowSoleCampaignResume, bool AllowSelection, bool AllowSwitch)
{
    // Length-delimited JSON avoids ambiguous scope concatenation. Diagnostics retain only this hash.
    public string Key => Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(
        new[] { PrincipalId, LogicalSessionId })));

    public bool Allows(string campaignId) => AuthorizedCampaignIds.Contains(campaignId, StringComparer.Ordinal);

    public void Validate()
    {
        if (new[] { PrincipalId, LogicalSessionId, AuthorityId }.Any(value =>
                string.IsNullOrWhiteSpace(value) || value.Length > 128) ||
            AuthorizedCampaignIds.Count > 256 || AuthorizedCampaignIds.Any(value =>
                string.IsNullOrWhiteSpace(value) || value.Length > 128))
            throw new ManagedServiceException("BINDING_SESSION_INVALID", "A trusted logical session and campaign access policy are required.");
    }
}

public sealed record CampaignBindingEvidence(string AuthorityId, string DataNamespaceId, string WorldModelId,
    string SchemaModelVersion, string RulesetId, string RulesetVersion, string RepositoryVersion,
    long CampaignVersion, DateTimeOffset? LastValidatedCommitAt, string RuleRepresentation,
    string? ActiveRuleIdentity, string? ImmutableSourceIdentity, string? BootstrapHash,
    string BootstrapState, long BootstrapRevision, bool MinimumRulesReady, string? PendingTransactionId)
{
    public bool Ready => CampaignVersion > 0 && LastValidatedCommitAt is not null && MinimumRulesReady &&
        ActiveRuleIdentity is not null && ImmutableSourceIdentity is not null && BootstrapHash is not null &&
        BootstrapState is "UserConfirmed" or "Verified" && PendingTransactionId is null;

    public bool SameProfile(CampaignBindingEvidence other) =>
        AuthorityId == other.AuthorityId && DataNamespaceId == other.DataNamespaceId &&
        WorldModelId == other.WorldModelId && SchemaModelVersion == other.SchemaModelVersion &&
        RulesetId == other.RulesetId && RulesetVersion == other.RulesetVersion &&
        RepositoryVersion == other.RepositoryVersion && RuleRepresentation == other.RuleRepresentation &&
        ActiveRuleIdentity == other.ActiveRuleIdentity && ImmutableSourceIdentity == other.ImmutableSourceIdentity;
}

public sealed record CampaignSessionBinding(string BindingId, string ScopeKey, long Generation,
    string CampaignId, CampaignBindingState State, long Revision, CampaignBindingEvidence Evidence,
    string ResolutionMode, string CreationRequestId, string CorrelationId,
    string? PredecessorBindingId, string? SuccessorBindingId, string? SuspensionCode,
    DateTimeOffset CreatedAt, DateTimeOffset VerifiedAt);

public sealed record CampaignBindingChoice(string Handle, string DisplayName, string? Description,
    string WorldModelId, string DataNamespaceId);

public sealed record CampaignBindingResolution(CampaignSessionBinding? Binding,
    IReadOnlyList<CampaignBindingChoice> Choices, string ResolutionMode, bool BindingValid)
{
    public bool GameplayInteractionAuthorized => false;
}

public sealed record CampaignBindingChange(string RequestId, string? ChoiceHandle,
    string? ExpectedBindingId, long ExpectedRevision, bool UserApproved);

// Stores must serialize the session scope and verify canonical eligibility within
// the same transaction as selection/successor creation. No prose input is accepted.
public interface ICampaignBindingStore
{
    Task<ManagedOperationResult<CampaignBindingResolution>> ExecuteAsync(CampaignBindingScope scope,
        CampaignBindingAction action, CampaignBindingChange? change, string correlationId, CancellationToken token);
}

public sealed class CampaignBindingService(ICampaignBindingStore store,
    IOptions<CampaignBindingOptions> options, IManagedDiagnosticRecorder? diagnostics = null)
{
    public Task<ManagedOperationResult<CampaignBindingResolution>> ResolveAsync(CancellationToken token) =>
        RunAsync(CampaignBindingAction.Resolve, null, token);

    public Task<ManagedOperationResult<CampaignBindingResolution>> ChangeAsync(CampaignBindingAction action,
        CampaignBindingChange change, CancellationToken token) => RunAsync(action, change, token);

    private async Task<ManagedOperationResult<CampaignBindingResolution>> RunAsync(CampaignBindingAction action,
        CampaignBindingChange? change, CancellationToken token)
    {
        var correlation = McpToolFailureContract.NewCorrelationId();
        var started = DateTimeOffset.UtcNow;
        CampaignBindingScope? scope = null;
        Exception? failure = null;
        ManagedOperationResult<CampaignBindingResolution> result;
        try
        {
            var settings = options.Value;
            if (!settings.Enabled) throw new ManagedServiceException("BINDING_DISABLED", "Durable campaign binding is not enabled for this deployment.");
            scope = new(settings.PrincipalId, settings.LogicalSessionId, settings.AuthorityId,
                Array.AsReadOnly(settings.AuthorizedCampaignIds.ToArray()), settings.AllowSoleCampaignResume,
                settings.AllowSelection, settings.AllowSwitch);
            scope.Validate();
            if (!Enum.IsDefined(action) || (action == CampaignBindingAction.Resolve) != (change is null))
                throw new ManagedServiceException("BINDING_REQUEST_INVALID", "The binding action is invalid.");
            if (change is not null)
            {
                if (!change.UserApproved || (action == CampaignBindingAction.Switch ? !scope.AllowSwitch : !scope.AllowSelection))
                    throw new ManagedServiceException("BINDING_UNAUTHORIZED", "This binding change requires authorized explicit user selection.");
                if (string.IsNullOrWhiteSpace(change.RequestId) || change.RequestId.Length > 128 ||
                    change.ExpectedRevision < 0 || change.ExpectedBindingId?.Length > 128 || change.ChoiceHandle?.Length > 128)
                    throw new ManagedServiceException("BINDING_REQUEST_INVALID", "Use a bounded stable request identity and the expected binding revision.");
            }
            result = await store.ExecuteAsync(scope, action, change, correlation, token);
        }
        catch (OperationCanceledException) { throw; }
        catch (ManagedServiceException error)
        { failure = error; result = new(false, error.Code, error.SafeMessage, null, RetrySafe: true); }

        result = result with { CorrelationId = correlation, Operation = "CampaignBinding", Stage = action.ToString() };
        if (diagnostics is not null)
        {
            var binding = result.Data?.Binding;
            // Diagnostics are bounded operational evidence, not another binding authority.
            try
            {
                var context = new ManagedDiagnosticContext(correlation, "CampaignBinding", RulePublicationStage.ToolInvocation,
                    result.Code, started, CampaignId: binding?.CampaignId, Outcome: result.Success ? "Succeeded" : "Blocked",
                    RetrySafe: result.RetrySafe, SafeDetail: JsonSerializer.Serialize(new
                    {
                        ScopeHash = scope?.Key, binding?.BindingId, binding?.State, binding?.Revision,
                        ExpectedRevision = change?.ExpectedRevision, binding?.PredecessorBindingId,
                        binding?.SuccessorBindingId, result.Data?.ResolutionMode, result.Data?.BindingValid
                    }), OperationId: change?.RequestId);
                if (failure is null) await diagnostics.RecordEventAsync(context, token);
                else await diagnostics.RecordFailureAsync(context, failure, token);
            }
            catch (Exception error) when (error is not OutOfMemoryException) { /* A lost diagnostic cannot undo a committed binding. */ }
        }
        return result;
    }
}

[McpServerToolType]
public sealed class CampaignBindingTools(CampaignBindingService service)
{
    [McpServerTool(Name = "ec_resolve_campaign_binding", Idempotent = true),
     Description("Recover the configured durable session binding or return meaningful authorized campaign choices. No story text or player action is accepted; this is not gameplay entry.")]
    public Task<ManagedOperationResult<CampaignBindingResolution>> ResolveAsync(CancellationToken token) => service.ResolveAsync(token);

    [McpServerTool(Name = "ec_select_campaign_binding", Idempotent = true),
     Description("After explicit user selection, bind a current choice handle within the trusted configured session. Does not authorize gameplay.")]
    public Task<ManagedOperationResult<CampaignBindingResolution>> SelectAsync(CampaignBindingChange request, CancellationToken token) =>
        service.ChangeAsync(CampaignBindingAction.Select, request, token);

    [McpServerTool(Name = "ec_switch_campaign_binding", Idempotent = true),
     Description("Explicitly switch to a verified successor binding only when durable interaction and persistence safety evidence permits it. Missing safety evidence blocks switching.")]
    public Task<ManagedOperationResult<CampaignBindingResolution>> SwitchAsync(CampaignBindingChange request, CancellationToken token) =>
        service.ChangeAsync(CampaignBindingAction.Switch, request, token);

    [McpServerTool(Name = "ec_suspend_campaign_binding", Idempotent = true),
     Description("Explicitly suspend the configured binding without deleting history, cancelling player intent or changing Campaign Canon.")]
    public Task<ManagedOperationResult<CampaignBindingResolution>> SuspendAsync(CampaignBindingChange request, CancellationToken token) =>
        service.ChangeAsync(CampaignBindingAction.Suspend, request, token);
}
