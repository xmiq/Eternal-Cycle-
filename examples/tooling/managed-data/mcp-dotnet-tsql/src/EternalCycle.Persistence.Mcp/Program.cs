using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

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
    .Bind(builder.Configuration.GetSection("EternalCycle:Persistence"))
    .Validate(
        value => !string.IsNullOrWhiteSpace(value.ConnectionString),
        "EternalCycle:Persistence:ConnectionString is required.")
    .Validate(
        ConfiguredCampaignSchemaResolver.IsValidOptions,
        "EternalCycle persistence schema routing contains an invalid identifier or incomplete world binding.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ManagedRuleServiceOptions>()
    .Bind(builder.Configuration.GetSection("EternalCycle:Rules"));

builder.Services
    .AddOptions<ManagedAdministrationOptions>()
    .Bind(builder.Configuration.GetSection("EternalCycle:Administration"));

builder.Services.AddSingleton<ICampaignSchemaResolver, ConfiguredCampaignSchemaResolver>();
builder.Services.AddSingleton<IRuleSourceConfigurationStore, SqlServerRuleSourceConfigurationStore>();
builder.Services.AddSingleton<IRuleSourceProvider, GitRuleSourceProvider>();
builder.Services.AddSingleton<IPublishedRuleStore, SqlServerPublishedRuleStore>();
builder.Services.AddSingleton<ManagedRulePublicationCoordinator>();
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
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
