using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public sealed class CompiledRuleRuntimeOptions
{
    // Deployment opt-in, not a permission inferred from import rights.
    public bool Enabled { get; init; }
}

public sealed record CompiledRulePublicationReceipt(CompiledRuleStoreScope Scope, string ImportId, bool ActivationCompleted);

public sealed class CompiledRuleRuntime(
    SqlServerCompiledRulesArtifactStore store,
    IGmHostConfigurationStore hostConfigurations,
    IOptions<ManagedRuleServiceOptions> rules,
    IOptions<CompiledRuleRuntimeOptions> runtime,
    IOptions<ManagedAdministrationOptions> administration)
{
    public async Task<CompiledRulePacketResult> GetContextAsync(CompiledRuleRetrievalRequest request, CancellationToken token)
    {
        // Deny before preparing caller data or probing existence/content in SQL.
        AuthorizeRuntime(request?.Scope);
        var prepared = CompiledRuleRetrieval.Prepare(request!, token);
        var stored = await store.ReadRuntimeAsync(prepared.Scope, token);
        if (prepared.Operation == "gameplay.resolve")
        {
            var bootstrap = stored.Artifact.RuleSources.SingleOrDefault(source => source.RuleSourceId == "gm-host-bootstrap");
            StoredGmHostConfiguration? configuration;
            try { configuration = await hostConfigurations.GetAsync(prepared.Scope.RulesetId, token); }
            catch (SqlException) when (token.IsCancellationRequested)
            { throw new OperationCanceledException("Compiled rule access was cancelled.", token); }
            catch (SqlException error) when (error.Number is 207 or 208)
            { throw new ManagedServiceException("MIGRATION_REQUIRED", "Host readiness storage requires a supported migration."); }
            catch (Exception error) when (error is not (OperationCanceledException or OutOfMemoryException))
            { throw new CompiledRuleRetrievalException(CompiledRuleRetrievalFailure.StorageFailed); }
            if (bootstrap is null || configuration?.BootstrapSourceHash != bootstrap.SourceSha256 ||
                configuration.State is not (GmHostConfigurationState.UserConfirmed or GmHostConfigurationState.Verified))
                throw new ManagedServiceException("GM_HOST_CONFIGURATION_REQUIRED", "Install and confirm the selected canonical GM Host Bootstrap before gameplay resolution.");
        }
        var candidates = CompiledRuleCandidateIndex.Create(stored.Artifact, token).FindCandidates(prepared, token);
        var ranked = CompiledRuleRanking.Rank(candidates, token);
        var closure = CompiledRuleDependencies.Expand(ranked, token);
        return CompiledRulePackets.Build(closure, token);
    }

    // Readiness is successful selected-closure retrieval, not a global boolean
    // or an assumption that all unrelated rules are usable.
    public async Task<ManagedOperationResult<CompiledRulePacket>> GetPacketAsync(CompiledRuleRetrievalRequest request, CancellationToken token)
    {
        try
        {
            var result = await GetContextAsync(request, token);
            return new(true, "RULE_CONTEXT_READY", "A compact, dependency-complete compiled Rule Packet is available.", result.Packet);
        }
        catch (OperationCanceledException) { throw; }
        catch (ManagedServiceException error) { return new(false, error.Code, error.SafeMessage, null); }
        catch (CompiledRuleRetrievalException error) { return new(false, error.Code, error.Message, null); }
    }

    // No new administrative MCP surface: integration callers use separate
    // consent gates, never an importer side effect or a client-supplied grant.
    public Task<CompiledRulePublicationReceipt> PublishAsync(CompiledRuleStoreScope scope, bool userApproved,
        string? operatorConfirmation, CancellationToken token) => ChangePublicationAsync(scope, userApproved, operatorConfirmation, false, token);

    public Task<CompiledRulePublicationReceipt> ActivateAsync(CompiledRuleStoreScope scope, bool userApproved,
        string? operatorConfirmation, CancellationToken token) => ChangePublicationAsync(scope, userApproved, operatorConfirmation, true, token);

    private async Task<CompiledRulePublicationReceipt> ChangePublicationAsync(CompiledRuleStoreScope scope, bool userApproved,
        string? operatorConfirmation, bool active, CancellationToken token)
    {
        if (!administration.Value.Enabled)
            throw new ManagedServiceException("ADMINISTRATION_DISABLED", "Administrative rule changes are disabled.");
        if (!userApproved)
            throw new ManagedServiceException("USER_APPROVAL_REQUIRED", "Explicit informed user approval is required before changing rule publication.");
        if (administration.Value.RequireOperatorConfirmation && operatorConfirmation != administration.Value.ApprovalPhrase)
            throw new ManagedServiceException("ADMINISTRATIVE_INTERVENTION_REQUIRED", "The configured operator safeguard must also be satisfied.");
        if (scope?.RulesetId != rules.Value.RulesetId)
            throw new ManagedServiceException("RULE_RETRIEVAL_UNAUTHORIZED", "Compiled rule access is not authorized.");
        CompiledRuleRetrieval.Prepare(new(scope, "context.assemble", "NORMAL", []), token);
        var stored = active ? await store.ActivateAsync(scope, token) : await store.PublishAsync(scope, token);
        return new(scope, stored.ImportId, active);
    }

    private void AuthorizeRuntime(CompiledRuleStoreScope? scope)
    {
        if (!runtime.Value.Enabled || scope?.RulesetId != rules.Value.RulesetId)
            throw new ManagedServiceException("RULE_RETRIEVAL_UNAUTHORIZED", "Compiled rule access is not authorized.");
    }
}

[McpServerToolType]
public sealed class CompiledRuleContextTools(CompiledRuleRuntime runtime)
{
    [McpServerTool(Name = "ec_get_compiled_rule_context", ReadOnly = true, Idempotent = true),
     Description("Returns a compact Rule Packet from one explicitly selected, authorized, published and active compiled artifact. Does not bind a campaign or fall back to legacy rules.")]
    public Task<ManagedOperationResult<CompiledRulePacket>> GetContextAsync(
        [Description("Explicit artifact scope and bounded selectors/query entries; no inferred campaign or acquisition state.")] CompiledRuleRetrievalRequest request,
        CancellationToken cancellationToken) => runtime.GetPacketAsync(request, cancellationToken);
}
