using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

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
    .AddOptions<RuleRetrievalOptions>()
    .Bind(builder.Configuration.GetSection("EternalCycle:Rules"));

builder.Services.AddSingleton<ICampaignSchemaResolver, ConfiguredCampaignSchemaResolver>();
builder.Services.AddSingleton<IRuleContextProvider, RepositoryRuleContextProvider>();
builder.Services.AddSingleton<ICampaignPersistenceStore, SqlServerCampaignPersistenceStore>();
builder.Services.AddSingleton<IDurabilityService, SqlServerDurabilityService>();
builder.Services.AddSingleton<PersistenceCoordinator>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
