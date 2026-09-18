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
    string? CampaignId,
    string? WorldModelId,
    string? DataNamespaceId,
    string? RulesetId,
    string? RulesetVersion,
    string? ActiveRuleReleaseId,
    string? CanonicalSourceIdentity,
    string RuleUpdateStatus,
    string? RuleUpdateDetail,
    ManagedReadinessReport Readiness,
    string SanitizationStatement,
    ManagedOperationStatus? LatestManagedOperation = null,
    string DiagnosticScope = "Full",
    string? SanitizedFileLogStatus = null,
    string? SanitizedFileLogLocation = null);

public interface IServiceDiagnostics
{
    ManagedServiceCapabilities GetCapabilities();

    Task<SanitizedDiagnosticReport> GetReportAsync(
        string? campaignId,
        CancellationToken cancellationToken);
}

public sealed class ServiceDiagnostics(
    ICampaignSchemaResolver schemaResolver,
    IManagedReadinessService readiness,
    IManagedOperationStore? operations = null) : IServiceDiagnostics
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
                "operations.durable",
                "rules.progressive-readiness",
                "setup.permission-gated",
                "campaign.discovery",
                "configuration.discovery",
                "errors.registry",
                "errors.dump",
                "diagnostics.automatic-fallback"
            ],
            true);

    public async Task<SanitizedDiagnosticReport> GetReportAsync(
        string? campaignId,
        CancellationToken cancellationToken)
    {
        var report = await readiness.GetReadinessAsync(campaignId, cancellationToken);
        CampaignSchemaRoute? route = null;
        if (campaignId is not null && report.Configuration?.Ready is not false)
        {
            try
            {
                route = schemaResolver.Resolve(campaignId);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                // Readiness already carries the safe routing/configuration failure.
            }
        }

        ManagedOperationStatus? latestOperation = null;
        var diagnosticScope = report.RuleDomainSchema.Status == ManagedComponentStatus.Ready
            ? "Full"
            : "PreMigration";
        if (operations is not null && diagnosticScope == "Full")
        {
            try
            {
                latestOperation = (await operations.ListRecentAsync(null, 1, cancellationToken)).FirstOrDefault();
            }
            catch (Exception exception) when (exception is Microsoft.Data.SqlClient.SqlException or InvalidOperationException)
            {
                diagnosticScope = "Degraded";
            }
        }

        var implementationVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        return new SanitizedDiagnosticReport(
            "1.0.0",
            ".NET / MCP / T-SQL reference implementation",
            implementationVersion,
            "MCP",
            "MANAGED",
            "Microsoft SQL Server / T-SQL",
            route?.CampaignId,
            route?.WorldModelId,
            route?.DataNamespaceId,
            route?.RulesetId,
            route?.RulesetVersion,
            report.ActiveRuleReleaseId,
            report.RuleSourceRevision,
            report.State.ToString(),
            report.ErrorCode,
            report,
            "Credentials, connection strings, locators, Campaign Canon, GM Secrets, and private conversations are omitted.",
            latestOperation,
            diagnosticScope,
            report.SanitizedFileLogStatus,
            report.SanitizedFileLogLocation);
    }
}

[McpServerToolType]
public sealed class ServiceDiagnosticTools(IServiceDiagnostics diagnostics)
{
    [McpServerTool(Name = "ec_service_capabilities", ReadOnly = true, Idempotent = true),
     Description("Returns the semantic capabilities of this optional Eternal Cycle Managed Data Service implementation.")]
    public ManagedServiceCapabilities GetCapabilities() => diagnostics.GetCapabilities();

    [McpServerTool(Name = "ec_get_diagnostics", ReadOnly = true, Idempotent = true),
     Description("Returns schema-tolerant sanitized service diagnostics without Campaign Canon or backend secrets. It works without a campaign and degrades safely before supported migrations; follow its recommended action rather than retrying unrelated rule operations.")]
    public Task<SanitizedDiagnosticReport> GetReportAsync(
        [Description("Optional stable campaign identifier. Omit it for service/bootstrap diagnostics before a campaign exists.")] string? campaignId,
        CancellationToken cancellationToken) =>
        diagnostics.GetReportAsync(campaignId, cancellationToken);
}
