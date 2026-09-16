using System.ComponentModel;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

[McpServerToolType]
public sealed class RuleContextTools(IRuleContextProvider provider)
{
    [McpServerTool(Name = "ec_get_rule_context", ReadOnly = true, Idempotent = true),
     Description("Returns an already-published, provenance-bearing, world-isolated Rule Packet within the configured normal-play budget.")]
    public Task<RuleContextResult> GetRuleContextAsync(
        [Description("Stable campaign identifier used to resolve the trusted world/ruleset binding.")] string campaignId,
        [Description("Current semantic operation, such as gameplay.resolve or persistence.commit.")] string operation,
        [Description("Material rule topics for the current operation.")] IReadOnlyList<string> topics,
        [Description("Maximum estimated rule tokens; normal retrieval cannot exceed 8000.")] int maxEstimatedTokens,
        CancellationToken cancellationToken) =>
        provider.GetContextAsync(
            new RuleContextRequest(
                campaignId,
                operation,
                topics,
                string.Empty,
                [],
                maxEstimatedTokens),
            cancellationToken);
}
