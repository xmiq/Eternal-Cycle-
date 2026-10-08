using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public sealed class GameplayEntryOptions
{
    public bool Enabled { get; init; }
    // Exact owner addresses supplied by deployment, never guessed or supplied by a tool.
    public IDictionary<string, RecordAddress> CurrentSessions { get; init; } = new Dictionary<string, RecordAddress>(StringComparer.Ordinal);
    public int MaximumEstimatedRuleTokens { get; init; } = CompiledRuleRetrieval.MaximumEstimatedTokens;
}

public sealed record GameplayEntryRequest(string RequestId, long ExpectedRevision, string? InteractionId = null)
{
    public IReadOnlyList<string> QueryTerms { get; init; } = [];
}

public enum EntryCanonRole { CurrentSession, PlayerState, Controller, Protagonist, Incarnation, Time, Scene, Placement, RelevantOwner }
public enum EntryCanonStatus { Completed, LegitimatelyAbsent, Unavailable, Stale, Incomplete, Failed }
public enum EntryCanonVisibility { GmAuthorized, PlayerVisible, Restricted }

public sealed record GameplayEntryReadRequirement(EntryCanonRole Role, RecordAddress? Address,
    bool LegitimatelyAbsent = false, string? AbsenceReason = null,
    EntryCanonVisibility Visibility = EntryCanonVisibility.GmAuthorized, long? ExpectedRevision = null);

// Reference projection of a canonical Current Session, not a new Canon owner.
// Other providers can adapt their existing session representation at the reader seam.
public sealed record GameplayEntrySessionPlan(string InputSha256, string CampaignMode,
    IReadOnlyList<string> ModuleIds, IReadOnlyList<string> QueryTerms,
    IReadOnlyList<string> RequiredRuleSourceIds, IReadOnlyList<string> RequiredSnippetIds,
    IReadOnlyList<GameplayEntryReadRequirement> Reads);

public sealed record GameplayEntryReadEvidence(EntryCanonRole Role, RecordAddress? Address, long? Revision,
    string? PayloadSha256, EntryCanonStatus Status, EntryCanonVisibility Visibility, string? AbsenceReason = null);

public sealed record GameplayEntryRuleEvidence(string Representation, string Identity, string ImmutableSourceIdentity,
    string Operation, string CampaignMode, string WorldModelId, IReadOnlyList<string> ModuleIds,
    IReadOnlyList<string> IncludedRuleIds, int UsedEstimatedTokens, int MaximumEstimatedTokens,
    int BudgetExcludedRoots, bool DependencyComplete, bool RequiredContextComplete,
    IReadOnlyList<string> CanonicalSignals, IReadOnlyList<string> ModelHintSignals);

public sealed record GameplayEntryReceipt(string ReceiptId, string RequestId, string InteractionId,
    string BindingId, string CampaignId, long BindingGeneration, long InteractionRevision,
    CampaignBindingEvidence Baseline, GameplayEntryRuleEvidence Rules,
    IReadOnlyList<GameplayEntryReadEvidence> Reads, GameplayEntryDecisionContext? Decision,
    DateTimeOffset EnteredAt, string CorrelationId);

public sealed record GameplayEntryDecisionContext(PlayerDecisionStatus Status,
    string ProtectedQuestionReference, string AllowedResponseScopeReference);

public sealed record GameplayEntryContext(CompiledRulePacket? CompiledRules, CompactRulePacket? SourceRules,
    IReadOnlyList<CanonicalRecord> Canon);

public sealed record GameplayEntryResult(PlayerInteractionStatus Interaction, GameplayEntryReceipt? Receipt,
    GameplayEntryContext? Context, bool Recovered, IReadOnlyList<GameplayEntryReadEvidence> Reads)
{
    // E must enforce every write/yield path before the route becomes supported gameplay.
    public bool GameplayMutationAuthorized => false;
}

public sealed class PreparedGameplayEntry
{
    internal PreparedGameplayEntry(PlayerInteractionRecord interaction, CampaignSessionBinding binding,
        GameplayEntryRequest request, string fingerprint, GameplayEntryReceipt? receipt)
    { Interaction = interaction; Binding = binding; Request = request; Fingerprint = fingerprint; Receipt = receipt; }
    internal PlayerInteractionRecord Interaction { get; }
    internal CampaignSessionBinding Binding { get; }
    internal string Fingerprint { get; }
    internal GameplayEntryRequest Request { get; }
    internal GameplayEntryReceipt? Receipt { get; }
    public string CampaignId => Interaction.CampaignId;
    public string InteractionId => Interaction.InteractionId;
    public string InputSha256 => Interaction.InputSha256;
    public CampaignBindingEvidence Baseline => Binding.Evidence;
}

public sealed record GameplayEntryCanonContext(GameplayEntrySessionPlan Plan,
    IReadOnlyList<CanonicalRecord> Records, IReadOnlyList<GameplayEntryReadEvidence> Evidence);

public interface IGameplayEntryCanonReader
{
    Task<GameplayEntryCanonContext> ReadAsync(PreparedGameplayEntry entry, CancellationToken token);
}

// Construction is service-only: a model cannot submit a bare "context complete" flag.
public sealed class VerifiedGameplayEntryContext
{
    internal VerifiedGameplayEntryContext(GameplayEntryCanonContext canon, GameplayEntryRuleEvidence rules)
    { Canon = canon; Rules = rules; }
    internal GameplayEntryCanonContext Canon { get; }
    internal GameplayEntryRuleEvidence Rules { get; }
}

public interface IGameplayEntryStore
{
    Task<PlayerInteractionStatus?> RecoverAsync(CampaignBindingScope scope, string? interactionId, CancellationToken token);
    Task<PreparedGameplayEntry> PrepareAsync(CampaignBindingScope scope, GameplayEntryRequest request, CancellationToken token);
    Task<GameplayEntryResult> CompleteAsync(CampaignBindingScope scope, PreparedGameplayEntry entry,
        VerifiedGameplayEntryContext? context, CancellationToken token);
    Task RecordFailureAsync(CampaignBindingScope scope, PreparedGameplayEntry entry, string code,
        IReadOnlyList<GameplayEntryReadEvidence> reads, CancellationToken token);
}

internal sealed class EntryCanonException(string code, IReadOnlyList<GameplayEntryReadEvidence> reads, Exception? cause = null)
    : ManagedServiceException(code, "Required canonical entry context is unavailable or incomplete. Recover the original owner reads; no gameplay entry was granted.", cause)
{
    internal IReadOnlyList<GameplayEntryReadEvidence> Reads { get; } = reads;
}

public sealed class GameplayEntryService(IGameplayEntryStore store, IGameplayEntryCanonReader canonReader,
    CompiledRuleRuntime compiledRules, IRuleContextProvider sourceRules,
    IOptions<CampaignBindingOptions> bindings, IOptions<PlayerInteractionOptions> interactions,
    IOptions<GameplayEntryOptions> options, IOptions<ManagedRuleServiceOptions> ruleOptions,
    IManagedDiagnosticRecorder? diagnostics = null)
{
    public async Task<ManagedOperationResult<GameplayEntryResult>> BeginAsync(GameplayEntryRequest request, CancellationToken token)
    {
        var correlation = McpToolFailureContract.NewCorrelationId(); var started = DateTimeOffset.UtcNow;
        PreparedGameplayEntry? entry = null; GameplayEntryRuleEvidence? ruleEvidence = null;
        IReadOnlyList<GameplayEntryReadEvidence> reads = [];
        var phase = "Authority"; Exception? failure = null;
        ManagedOperationResult<GameplayEntryResult> result;
        CampaignBindingScope? scope = null;
        try
        {
            if (!bindings.Value.Enabled || !interactions.Value.Enabled || !options.Value.Enabled)
                throw Error("GAMEPLAY_ENTRY_DISABLED", "Mandatory entry is not configured; no gameplay authority was granted.");
            var policy = bindings.Value;
            scope = new(policy.PrincipalId, policy.LogicalSessionId, policy.AuthorityId,
                Array.AsReadOnly(policy.AuthorizedCampaignIds.ToArray()), policy.AllowSoleCampaignResume, policy.AllowSelection, policy.AllowSwitch);
            scope.Validate();
            if (request is null || !PlayerInteractionService.Bounded(request.RequestId, 128) || request.ExpectedRevision <= 0 ||
                request.InteractionId is not null && !PlayerInteractionService.Bounded(request.InteractionId, 128) ||
                options.Value.MaximumEstimatedRuleTokens is < 1 or > CompiledRuleRetrieval.MaximumEstimatedTokens)
                throw Error("GAMEPLAY_ENTRY_REQUEST_INVALID", "Use the original interaction revision and bounded stable entry request.");
            // FR-026 owns normalization/bounds. Freeze caller collections before any await.
            var hints = CompiledRuleRetrieval.Prepare(new(new("entry-validation", new string('A', 64)), "gameplay.resolve", "NORMAL", request.QueryTerms), token).QueryTerms;
            request = request with { QueryTerms = hints };
            phase = "Prepare"; entry = await store.PrepareAsync(scope, request, token);
            if (entry.Receipt is not null)
            {
                phase = "Recovery";
                var recovered = await store.CompleteAsync(scope, entry, null, token);
                result = new(true, "GAMEPLAY_ENTRY_RECOVERED", "Original entry evidence recovered. No new input, resolution or gameplay write occurred.", recovered, RetrySafe: true);
            }
            else
            {
                phase = "CanonRead";
                var canon = await canonReader.ReadAsync(entry, token); reads = canon.Evidence;
                var plan = canon.Plan; var baseline = entry.Baseline;
                var terms = plan.QueryTerms.Concat(hints).ToArray();
                phase = "Rules";
                CompiledRulePacket? compiled = null; CompactRulePacket? legacy = null;
                IReadOnlyList<string> ids; int used; int excluded = 0;
                var settings = ruleOptions.Value;
                var mode = settings.CampaignModes.TryGetValue(entry.CampaignId, out var configuredMode) ? configuredMode : settings.DefaultCampaignMode;
                var modules = settings.CampaignOptionalModules.TryGetValue(entry.CampaignId, out var configuredModules) ? configuredModules : [];
                if (mode != plan.CampaignMode || !modules.Order(StringComparer.Ordinal).SequenceEqual(plan.ModuleIds.Order(StringComparer.Ordinal)))
                    throw Error("GAMEPLAY_ENTRY_PROFILE_MISMATCH", "Canonical entry selectors conflict with the explicitly configured rule profile.");
                if (baseline.RuleRepresentation == "CompiledArtifact")
                {
                    var retrieval = new CompiledRuleRetrievalRequest(new(baseline.RulesetId, baseline.ActiveRuleIdentity!), "gameplay.resolve", plan.CampaignMode, terms)
                    {
                        WorldModelId = baseline.WorldModelId, ModuleIds = plan.ModuleIds,
                        RequiredRuleSourceIds = plan.RequiredRuleSourceIds, RequiredSnippetIds = plan.RequiredSnippetIds,
                        MaximumEstimatedTokens = options.Value.MaximumEstimatedRuleTokens
                    };
                    var packet = await compiledRules.GetContextAsync(retrieval, token);
                    if (!packet.Members.Any(member => member.RuleSourceId == RuleCompiler.RuntimeKernelSourceId) ||
                        !packet.Members.Any(member => member.RuleSourceId == RuleCompiler.GmRuntimeProcedureSourceId))
                        throw Error("GAMEPLAY_ENTRY_RULES_INCOMPLETE", "The selected artifact lacks required Kernel or GM Runtime Procedure authority.");
                    compiled = packet.Packet; ids = Array.AsReadOnly(packet.Members.Select(member => member.SnippetId).ToArray());
                    used = packet.Totals.UsedEstimatedTokens; excluded = packet.Totals.BudgetExcludedRoots;
                }
                else
                {
                    // Explicit legacy selection only. Never fallback from failed compiled retrieval.
                    RuleContextResult packet;
                    try
                    {
                        packet = await sourceRules.GetContextAsync(new(entry.CampaignId, "gameplay.resolve", terms, mode, modules,
                            options.Value.MaximumEstimatedRuleTokens), token);
                    }
                    catch (InvalidOperationException error)
                    { throw new ManagedServiceException("GAMEPLAY_ENTRY_RULES_INCOMPLETE", "Required selected source-release authority is unavailable; no alternate profile was used.", error); }
                    if (packet.RuleReleaseId != baseline.ActiveRuleIdentity || packet.SourceIdentity != baseline.ImmutableSourceIdentity ||
                        !packet.Chunks.Any(chunk => chunk.RuleSourceId == RuleCompiler.RuntimeKernelSourceId) ||
                        !packet.Chunks.Any(chunk => chunk.RuleSourceId == RuleCompiler.GmRuntimeProcedureSourceId) ||
                        plan.RequiredRuleSourceIds.Any(id => !packet.Chunks.Any(chunk => chunk.RuleSourceId == id)) ||
                        plan.RequiredSnippetIds.Any(id => !packet.Chunks.Any(chunk => chunk.ChunkId == id)))
                        throw Error("GAMEPLAY_ENTRY_RULES_INCOMPLETE", "Required selected source-release authority is unavailable; no alternate profile was used.");
                    legacy = RulePacketFormatter.Compact(packet); used = packet.EstimatedTokens;
                    ids = Array.AsReadOnly(packet.Chunks.Select(chunk => chunk.ChunkId).ToArray());
                }
                if (ids.Count > 512) throw Error("GAMEPLAY_ENTRY_RULES_INCOMPLETE", "Required rule evidence exceeds the supported bounded entry record.");
                ruleEvidence = new(baseline.RuleRepresentation, baseline.ActiveRuleIdentity!, baseline.ImmutableSourceIdentity!, "gameplay.resolve",
                    plan.CampaignMode, baseline.WorldModelId, plan.ModuleIds, ids, used, options.Value.MaximumEstimatedRuleTokens,
                    excluded, true, true, plan.QueryTerms, hints);
                phase = "Finalize";
                var completed = await store.CompleteAsync(scope, entry, new(canon, ruleEvidence), token);
                // Only the winning first acknowledgment supplies context. Concurrent retries
                // recover the receipt, not a competing or newly interpreted context.
                if (!completed.Recovered) completed = completed with { Context = new(compiled, legacy, canon.Records) };
                result = new(true, completed.Recovered ? "GAMEPLAY_ENTRY_RECOVERED" : "GAMEPLAY_ENTRY_OPEN",
                    "Mandatory entry evidence is durable. Legacy mutation/yield enforcement remains a separate gate.", completed, RetrySafe: true);
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception error) when (error is ManagedServiceException or CompiledRuleRetrievalException)
        {
            failure = error;
            var code = error is ManagedServiceException managed ? managed.Code : ((CompiledRuleRetrievalException)error).Code;
            if (error is EntryCanonException readError) reads = readError.Reads;
            if (entry is not null && scope is not null)
            {
                try { await store.RecordFailureAsync(scope, entry, code, reads, token); }
                catch (ManagedServiceException) { /* The original blocking result survives diagnostic persistence failure. */ }
            }
            GameplayEntryResult? blocked = null;
            if (entry is not null && scope is not null)
            {
                try
                {
                    var status = await store.RecoverAsync(scope, entry.InteractionId, token);
                    if (status is not null) blocked = new(status, null, null, false, reads);
                }
                catch (ManagedServiceException) { /* Unavailable authority is not disclosed through partial recovery. */ }
            }
            result = new(false, code, error is ManagedServiceException safe ? safe.SafeMessage : error.Message, blocked, RetrySafe: true);
        }
        result = result with { CorrelationId = correlation, Operation = "GameplayEntry", Stage = phase };
        if (diagnostics is not null)
        {
            try
            {
                var evidence = new ManagedDiagnosticContext(correlation, "GameplayEntry", RulePublicationStage.ToolInvocation, result.Code, started,
                    CampaignId: entry?.CampaignId, OperationId: entry?.InteractionId, Outcome: result.Success ? "Succeeded" : "Blocked", RetrySafe: true,
                    SafeDetail: JsonSerializer.Serialize(new { BindingId = entry?.Binding.BindingId, entry?.InteractionId, Phase = phase,
                        ArtifactIdentity = ruleEvidence?.Identity, RetrievalMode = ruleEvidence?.Operation,
                        RuleClosureComplete = ruleEvidence?.DependencyComplete, CanonReadStatuses = reads.Select(read => new { read.Role, read.Status, read.Revision }),
                        ExpectedRevision = request?.ExpectedRevision, ActualRevision = result.Data?.Interaction.Revision,
                        CanonicalVersion = entry?.Baseline.CampaignVersion, InputCorrelationId = entry?.Interaction.CorrelationId,
                        Readiness = entry?.Baseline.Ready, FinalState = result.Data?.Interaction.State, IdempotencyOutcome = result.Code }));
                if (failure is null) await diagnostics.RecordEventAsync(evidence, token); else await diagnostics.RecordFailureAsync(evidence, failure, token);
            }
            catch (Exception error) when (error is not OutOfMemoryException) { /* Diagnostic loss cannot revoke a committed entry. */ }
        }
        return result;
    }

    internal static ManagedServiceException Error(string code, string message) => new(code, message);
}

[McpServerToolType]
public sealed class GameplayEntryTools(GameplayEntryService service)
{
    [McpServerTool(Name = "ec_begin_gameplay_interaction", Idempotent = true),
     Description("Validate mandatory rules and minimum Canon for an existing trusted player interaction. Cannot create input, select a campaign, choose an option or apply gameplay effects. Entry integration checkpoint; legacy writes are not yet interaction-gated.")]
    public Task<ManagedOperationResult<GameplayEntryResult>> BeginAsync(GameplayEntryRequest request, CancellationToken token) => service.BeginAsync(request, token);
}
