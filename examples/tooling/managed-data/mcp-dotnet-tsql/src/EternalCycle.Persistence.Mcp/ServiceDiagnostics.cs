using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public sealed record ManagedServiceCapabilities(
    string Implementation,
    string Interface,
    string PersistenceStrategy,
    string StorageAdapterFamily,
    string[] Capabilities,
    bool AdministrativeCapabilitiesSeparated);

public sealed record SanitizedDiagnosticReport(
    string EternalCycleVersion,
    string Implementation,
    string ImplementationVersion,
    string Interface,
    string PersistenceStrategy,
    string StorageAdapterFamily,
    string CampaignId,
    string WorldModelId,
    string DataNamespaceId,
    string RulesetId,
    string RulesetVersion,
    string? ActiveRuleReleaseId,
    string? CanonicalSourceIdentity,
    string RuleUpdateStatus,
    string? RuleUpdateDetail,
    ManagedReadinessReport Readiness,
    string SanitizationStatement);

public interface IServiceDiagnostics
{
    ManagedServiceCapabilities GetCapabilities();

    Task<SanitizedDiagnosticReport> GetReportAsync(
        string campaignId,
        CancellationToken cancellationToken);
}

public sealed class ServiceDiagnostics(
    ICampaignSchemaResolver schemaResolver,
    IManagedReadinessService readiness) : IServiceDiagnostics
{
    public ManagedServiceCapabilities GetCapabilities() =>
        new(
            ".NET / MCP / T-SQL reference implementation",
            "MCP",
            "MANAGED",
            "Microsoft SQL Server / T-SQL",
            [
                "campaign.status",
                "campaign.read",
                "turn.commit",
                "turn.retry",
                "rules.context",
                "diagnostics.sanitized",
                "readiness.structured",
                "setup.permission-gated",
                "campaign.discovery"
            ],
            true);

    public async Task<SanitizedDiagnosticReport> GetReportAsync(
        string campaignId,
        CancellationToken cancellationToken)
    {
        var route = schemaResolver.Resolve(campaignId);
        var report = await readiness.GetReadinessAsync(campaignId, cancellationToken);
        var implementationVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        return new SanitizedDiagnosticReport(
            "1.0.0+post-release",
            ".NET / MCP / T-SQL reference implementation",
            implementationVersion,
            "MCP",
            "MANAGED",
            "Microsoft SQL Server / T-SQL",
            route.CampaignId,
            route.WorldModelId,
            route.DataNamespaceId,
            route.RulesetId,
            route.RulesetVersion,
            report.ActiveRuleReleaseId,
            report.RuleSourceRevision,
            report.State.ToString(),
            report.ErrorCode,
            report,
            "Credentials, connection strings, locators, Campaign Canon, GM Secrets, and private conversations are omitted.");
    }
}

[McpServerToolType]
public sealed class ServiceDiagnosticTools(IServiceDiagnostics diagnostics)
{
    [McpServerTool(Name = "ec_service_capabilities", ReadOnly = true, Idempotent = true),
     Description("Returns the semantic capabilities of this optional Eternal Cycle Managed Data Service implementation.")]
    public ManagedServiceCapabilities GetCapabilities() => diagnostics.GetCapabilities();

    [McpServerTool(Name = "ec_get_diagnostics", ReadOnly = true, Idempotent = true),
     Description("Returns a sanitized provenance and service-status report without Campaign Canon or backend secrets.")]
    public Task<SanitizedDiagnosticReport> GetReportAsync(
        [Description("Stable campaign identifier from protected Campaign Configuration.")] string campaignId,
        CancellationToken cancellationToken) =>
        diagnostics.GetReportAsync(campaignId, cancellationToken);
}
