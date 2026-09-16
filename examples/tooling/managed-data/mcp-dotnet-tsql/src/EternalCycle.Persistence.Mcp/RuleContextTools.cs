using System.ComponentModel;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

[McpServerToolType]
public sealed class RuleContextTools(
    IRuleContextProvider provider,
    IManagedReadinessService readiness)
{
    [McpServerTool(Name = "ec_get_rule_context", ReadOnly = true, Idempotent = true),
     Description("Returns an already-published, provenance-bearing, world-isolated Rule Packet within the configured normal-play budget.")]
    public async Task<ManagedOperationResult<RuleContextResult>> GetRuleContextAsync(
        [Description("Stable campaign identifier used to resolve the trusted world/ruleset binding.")] string campaignId,
        [Description("Current semantic operation, such as gameplay.resolve or persistence.commit.")] string operation,
        [Description("Material rule topics for the current operation.")] IReadOnlyList<string> topics,
        [Description("Maximum estimated rule tokens; normal retrieval cannot exceed 8000.")] int maxEstimatedTokens,
        CancellationToken cancellationToken)
    {
        var report = await readiness.GetReadinessAsync(campaignId, cancellationToken);
        if (!report.GameplayReady)
        {
            return new(false, report.ErrorCode ?? "RULE_CONTEXT_UNAVAILABLE", report.Message, null);
        }

        try
        {
            var context = await provider.GetContextAsync(
                new RuleContextRequest(
                    campaignId,
                    operation,
                    topics,
                    string.Empty,
                    [],
                    maxEstimatedTokens),
                cancellationToken);
            return new(true, "RULE_CONTEXT_READY", "A bounded, provenance-bearing Rule Packet is available.", context);
        }
        catch (ManagedServiceException exception)
        {
            return new(false, exception.Code, exception.SafeMessage, null);
        }
        catch (Exception)
        {
            return new(
                false,
                "RULE_CONTEXT_UNAVAILABLE",
                "Rule context retrieval failed; inspect authorized sanitized service logs.",
                null);
        }
    }
}
