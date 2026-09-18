using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;

var builder = Host.CreateApplicationBuilder(args);
var diagnosticFallback = SanitizedDiagnosticFallbackBootstrap.Establish(builder.Configuration);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});
var sanitizedLogFile = builder.Configuration["EternalCycle:Administration:SanitizedLogFile"];
if (!string.IsNullOrWhiteSpace(sanitizedLogFile))
{
    builder.Logging.AddProvider(new SanitizedFileLoggerProvider(sanitizedLogFile));
}

builder.Services
    .AddOptions<SqlServerPersistenceOptions>()
    .Bind(builder.Configuration.GetSection("EternalCycle:Persistence"));

builder.Services
    .AddOptions<ManagedRuleServiceOptions>()
    .Bind(builder.Configuration.GetSection("EternalCycle:Rules"));

builder.Services
    .AddOptions<ManagedAdministrationOptions>()
    .Bind(builder.Configuration.GetSection("EternalCycle:Administration"));

builder.Services
    .AddOptions<ManagedDiagnosticsOptions>()
    .Bind(builder.Configuration.GetSection("EternalCycle:Diagnostics"));

builder.Services.AddSingleton(diagnosticFallback);
builder.Services.AddSingleton<IManagedConfigurationService, ManagedConfigurationService>();
builder.Services.AddSingleton<ICampaignSchemaResolver, ConfiguredCampaignSchemaResolver>();
builder.Services.AddSingleton<IRuleSourceConfigurationStore, SqlServerRuleSourceConfigurationStore>();
builder.Services.AddSingleton<IRuleSourceProvider, GitRuleSourceProvider>();
builder.Services.AddSingleton<SqlServerPublishedRuleStore>();
builder.Services.AddSingleton<IPublishedRuleStore>(services =>
    services.GetRequiredService<SqlServerPublishedRuleStore>());
builder.Services.AddSingleton<IRulePreparationStore, SqlServerRulePreparationStore>();
builder.Services.AddSingleton<IManagedOperationStore, SqlServerManagedOperationStore>();
builder.Services.AddSingleton<IManagedOperationService, ManagedOperationService>();
builder.Services.AddSingleton<IManagedDiagnosticStore, SqlServerManagedDiagnosticStore>();
builder.Services.AddSingleton<IManagedDiagnosticFileSink, PhysicalManagedDiagnosticFileSink>();
builder.Services.AddSingleton<IManagedDiagnosticRecorder, ManagedDiagnosticRecorder>();
builder.Services.AddSingleton<IRecentManagedErrorStore, BoundedRecentManagedErrorStore>();
builder.Services.AddSingleton<IManagedDiagnosticEvidenceReader, SqlServerManagedDiagnosticEvidenceReader>();
builder.Services.AddSingleton<IManagedDistributionIdentityProvider, PackagedManagedDistributionIdentityProvider>();
builder.Services.AddSingleton<IErrorDumpService, ErrorDumpService>();
builder.Services.AddSingleton<IMcpToolExceptionBoundary, McpToolExceptionBoundary>();
builder.Services.AddSingleton<IMcpToolInvocationGuard, McpToolInvocationGuard>();
builder.Services.AddSingleton<ManagedRulePublicationCoordinator>();
builder.Services.AddSingleton<IRulePublicationExecutor, ManagedRulePublicationExecutor>();
builder.Services.AddSingleton<ManagedOperationProcessor>();
builder.Services.AddHostedService<ManagedOperationWorker>();
builder.Services.AddHostedService<ManagedRuleUpdateHostedService>();
builder.Services.AddSingleton<IRuleContextProvider, PublishedRuleContextProvider>();
builder.Services.AddSingleton<IManagedInfrastructureInspector, SqlServerManagedInfrastructureInspector>();
builder.Services.AddSingleton<IManagedReadinessService, ManagedReadinessService>();
builder.Services.AddSingleton<ISchemaBootstrapExecutor, SqlServerSchemaBootstrapExecutor>();
builder.Services.AddSingleton<ICampaignDirectoryService, SqlServerCampaignDirectoryService>();
builder.Services.AddSingleton<IManagedAdministrationService, ManagedAdministrationService>();
builder.Services.AddSingleton<ICampaignPersistenceStore, SqlServerCampaignPersistenceStore>();
builder.Services.AddSingleton<IDurabilityService, SqlServerDurabilityService>();
builder.Services.AddSingleton<PersistenceCoordinator>();
builder.Services.AddSingleton<IServiceDiagnostics, ServiceDiagnostics>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithRequestFilters(filters =>
    {
        filters.AddCallToolFilter(next => async (context, cancellationToken) =>
        {
            var toolName = context.Params?.Name ?? "unknown-tool";
            var guard = context.Services?.GetService<IMcpToolInvocationGuard>();
            if (guard is not null)
            {
                return await guard.InvokeAsync(
                    toolName,
                    () => next(context, cancellationToken).AsTask(),
                    cancellationToken);
            }

            var correlationId = McpToolFailureContract.NewCorrelationId();
            try
            {
                return await next(context, cancellationToken);
            }
            catch
            {
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = EmergencyErrorText.Create("INTERNAL_ERROR", correlationId, toolName) }],
                    IsError = true
                };
            }
        });
    })
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
