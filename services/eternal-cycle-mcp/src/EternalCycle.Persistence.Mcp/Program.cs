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
    .ValidateOnStart();

builder.Services.AddSingleton<ICampaignPersistenceStore, SqlServerCampaignPersistenceStore>();
builder.Services.AddSingleton<IDurabilityService, SqlServerDurabilityService>();
builder.Services.AddSingleton<PersistenceCoordinator>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
