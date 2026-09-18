using System.Text.Json;
using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class ManagedControlPlaneRepairTests
{
    [Fact]
    public void ConfigurationDiscoveryRequiresNoDatabaseAndNeverReturnsSensitiveValues()
    {
        const string secret = "Server=localhost;Initial Catalog=EC;User Id=test;Password=do-not-return";
        var service = ConfigurationService(new Dictionary<string, string?>
        {
            ["EternalCycle:Persistence:ConnectionString"] = secret,
            ["EternalCycle:Rules:ReleaseChannel"] = "Prerelease",
            ["EternalCycle:Diagnostics:VerboseErrors"] = "true",
            ["EternalCycle:Rules:GitSource:AcquisitionTimeout"] = "00:05:00"
        });

        var report = service.GetReport();
        var connection = Assert.Single(report.Settings, value =>
            value.Key == "EternalCycle:Persistence:ConnectionString");
        var channel = Assert.Single(report.Settings, value =>
            value.Key == "EternalCycle:Rules:ReleaseChannel");
        var verbose = Assert.Single(report.Settings, value =>
            value.Key == "EternalCycle:Diagnostics:VerboseErrors");
        var fallback = Assert.Single(report.Settings, value =>
            value.Key == "EternalCycle:Diagnostics:FallbackLogFile");

        Assert.True(report.Ready);
        Assert.Equal("EternalCycle__Persistence__ConnectionString", connection.EnvironmentVariableName);
        Assert.True(connection.Sensitive);
        Assert.True(connection.Configured);
        Assert.Null(connection.DefaultValue);
        Assert.DoesNotContain("do-not-return", JsonSerializer.Serialize(report), StringComparison.Ordinal);
        Assert.Equal(["Stable", "Prerelease"], channel.AllowedValues);
        Assert.True(channel.RestartRequired);
        Assert.Equal("Development", verbose.Audience);
        Assert.False(fallback.Required);
        Assert.Equal(ManagedConfigurationValidationStatus.Defaulted, fallback.ValidationStatus);
    }

    [Fact]
    public void ConfigurationDiscoveryReportsMissingAndInvalidSettingsStructurally()
    {
        var missing = ConfigurationService([]).GetReport();
        var invalid = ConfigurationService(new Dictionary<string, string?>
        {
            ["EternalCycle:Persistence:ConnectionString"] = "not-a-connection-string",
            ["EternalCycle:Rules:ReleaseChannel"] = "Newest"
        }).GetReport();

        Assert.Equal("ConfigurationRequired", missing.State);
        Assert.Contains(missing.Settings, value =>
            value.Required && value.ValidationStatus == ManagedConfigurationValidationStatus.Missing);
        Assert.Equal("ConfigurationInvalid", invalid.State);
        Assert.Contains(invalid.Settings, value =>
            value.Key == "EternalCycle:Persistence:ConnectionString" &&
            value.ValidationStatus == ManagedConfigurationValidationStatus.Invalid);
        Assert.Contains(invalid.Settings, value =>
            value.Key == "EternalCycle:Rules:ReleaseChannel" &&
            value.ValidationStatus == ManagedConfigurationValidationStatus.Invalid);
    }

    [Fact]
    public void AutomaticDiagnosticFallbackIsZeroConfigurationAndFailureIsExplicit()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ec-fallback-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var ready = SanitizedDiagnosticFallbackBootstrap.Establish(
                new ConfigurationBuilder().Build(),
                root);
            Assert.Equal(SanitizedDiagnosticFallbackStatus.ConfiguredAutomatically, ready.Status);
            Assert.True(ready.Available);
            Assert.True(File.Exists(ready.ResolvedPath));

            var blockingFile = Path.Combine(root, "not-a-directory");
            File.WriteAllText(blockingFile, "fixture");
            var failed = SanitizedDiagnosticFallbackBootstrap.Establish(
                new ConfigurationBuilder().Build(),
                blockingFile);
            Assert.Equal(SanitizedDiagnosticFallbackStatus.InitializationFailed, failed.Status);
            Assert.False(failed.Available);

            var lookup = new EternalCycleErrorCodeTools().GetErrorCode("INTERNAL_ERROR");
            Assert.True(lookup.Found);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ErrorRegistryListLookupAndUnknownLookupRequireNoDatabase()
    {
        var tools = new EternalCycleErrorCodeTools();

        var list = tools.ListErrorCodes(null);
        var known = tools.GetErrorCode("migration_required");
        var unknown = tools.GetErrorCode("unknown-fixture-code");

        Assert.Contains(list, value => value.Code == "INTERNAL_ERROR");
        Assert.Contains(list, value => value.Code == "MIGRATION_REQUIRED");
        Assert.Equal(list.Count, list.Select(value => value.Code).Distinct(StringComparer.Ordinal).Count());
        Assert.True(known.Found);
        Assert.Equal("MIGRATION_REQUIRED", known.RequestedCode);
        Assert.False(unknown.Found);
        Assert.Contains("not registered", unknown.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task VerboseOffStillReturnsCodeCorrelationAndRecovery()
    {
        var recorder = new CapturingRecorder();
        var recent = new BoundedRecentManagedErrorStore();
        var boundary = new McpToolExceptionBoundary(
            recorder,
            recent,
            Options.Create(new ManagedDiagnosticsOptions { VerboseErrors = false }));

        var result = await boundary.CreateFailureAsync(
            "ec_fixture",
            "EC-FIXTURE",
            new InvalidOperationException("unexpected"),
            CancellationToken.None);
        var failure = DeserializeFailure(result);

        Assert.Equal("INTERNAL_ERROR", failure.Code);
        Assert.Equal("EC-FIXTURE", failure.CorrelationId);
        Assert.Null(failure.DeveloperDiagnostics);
        Assert.NotNull(failure.RequiredAction);
        Assert.Contains("ec_get_error_dump", failure.AllowedNextActions);
        Assert.Equal("EC-FIXTURE", recorder.Context?.CorrelationId);
        Assert.Single(recent.Find("EC-FIXTURE", 5));
    }

    [Fact]
    public async Task VerboseOnAddsSanitizedDeveloperDetail()
    {
        var boundary = new McpToolExceptionBoundary(
            NullManagedDiagnosticRecorder.Instance,
            new BoundedRecentManagedErrorStore(),
            Options.Create(new ManagedDiagnosticsOptions { VerboseErrors = true }));

        var result = await boundary.CreateFailureAsync(
            "ec_fixture",
            "EC-VERBOSE",
            new InvalidOperationException("password=supersecret"),
            CancellationToken.None);
        var failure = DeserializeFailure(result);
        var serialized = JsonSerializer.Serialize(failure);

        Assert.NotNull(failure.DeveloperDiagnostics);
        Assert.Contains("InvalidOperationException", failure.DeveloperDiagnostics.ExceptionType);
        Assert.Contains("[REDACTED]", failure.DeveloperDiagnostics.ExceptionMessage);
        Assert.DoesNotContain("supersecret", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ActualInvocationGuardConvertsUnexpectedFailuresAndPreservesKnownFailures()
    {
        var recorder = new CapturingRecorder();
        var recent = new BoundedRecentManagedErrorStore();
        var guard = new McpToolInvocationGuard(
            new McpToolExceptionBoundary(
                recorder,
                recent,
                Options.Create(new ManagedDiagnosticsOptions())),
            recent);

        var unexpected = await guard.InvokeAsync(
            "ec_fault_injection",
            () => throw new InvalidOperationException("fault"),
            CancellationToken.None);
        var unexpectedFailure = DeserializeFailure(unexpected);
        Assert.Equal("INTERNAL_ERROR", unexpectedFailure.Code);
        Assert.StartsWith("EC-", unexpectedFailure.CorrelationId, StringComparison.Ordinal);
        Assert.Equal(unexpectedFailure.CorrelationId, recorder.Context?.CorrelationId);

        var knownPayload = new ManagedOperationResult<object>(
            false,
            "MIGRATION_REQUIRED",
            "Migration is required.",
            null);
        var known = await guard.InvokeAsync(
            "ec_publish_initial_rules",
            () => Task.FromResult(new CallToolResult
            {
                Content = [new TextContentBlock { Text = JsonSerializer.Serialize(knownPayload) }],
                StructuredContent = JsonSerializer.SerializeToElement(knownPayload)
            }),
            CancellationToken.None);
        var knownFailure = DeserializeFailure(known);
        Assert.Equal("MIGRATION_REQUIRED", knownFailure.Code);
        Assert.Equal("ec_get_setup_plan", knownFailure.RecommendedNextAction);
        Assert.DoesNotContain("ec_publish_initial_rules", knownFailure.AllowedNextActions);
        Assert.True(known.IsError);
    }

    [Fact]
    public async Task EmergencyStringSurvivesBoundaryFailureWithoutPersistence()
    {
        var guard = new McpToolInvocationGuard(
            new ThrowingBoundary(),
            new BoundedRecentManagedErrorStore());

        var result = await guard.InvokeAsync(
            "ec_fault_injection",
            () => throw new InvalidOperationException("primary"),
            CancellationToken.None);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.True(result.IsError);
        Assert.Contains("ETERNAL_CYCLE_ERROR INTERNAL_ERROR", text, StringComparison.Ordinal);
        Assert.Contains("Reference: EC-", text, StringComparison.Ordinal);
        Assert.Contains("ec_get_error_dump", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ErrorDumpIsPortableBoundedSanitizedAndPartialWithoutDatabase()
    {
        const string secret = "Server=localhost;Initial Catalog=EC;User Id=test;Password=do-not-return";
        var recent = new BoundedRecentManagedErrorStore();
        recent.Add(new RecentManagedError(
            DateTimeOffset.UtcNow,
            "ec_fixture",
            Failure("EC-DUMP", new string('x', 6000))));
        var service = new ErrorDumpService(
            recent,
            ConfigurationService(new Dictionary<string, string?>
            {
                ["EternalCycle:Persistence:ConnectionString"] = secret
            }),
            new SanitizedDiagnosticFallbackState(
                SanitizedDiagnosticFallbackStatus.InitializationFailed,
                null,
                "fixture failure"),
            new StaticDistributionIdentityProvider(),
            databaseDiagnostics: new ThrowingDiagnosticReader());

        var dump = await service.GetAsync(
            new ErrorDumpRequest("EC-DUMP", MaximumCount: 500),
            CancellationToken.None);
        var serialized = JsonSerializer.Serialize(dump);

        Assert.Equal(ErrorDumpService.FormatVersion, dump.ErrorDumpFormatVersion);
        Assert.Equal("Partial", dump.State);
        Assert.True(dump.DiagnosticsTruncated);
        Assert.True(dump.DiagnosticEventsIncluded <= ErrorDumpService.MaximumEvidenceRecords * 2);
        Assert.True(dump.Text.Length <= ErrorDumpService.MaximumTextCharacters);
        Assert.Contains("Eternal Cycle Error Dump", dump.Text, StringComparison.Ordinal);
        Assert.Contains("Format: 1", dump.Text, StringComparison.Ordinal);
        Assert.Contains("EC-DUMP", dump.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("do-not-return", serialized, StringComparison.Ordinal);
        Assert.Contains(dump.Sources, value => value.Source == "DatabaseDiagnostics" && !value.Available);
        Assert.Null(dump.Distribution.OfficialDistribution);
    }

    [Fact]
    public async Task ErrorDumpToolHasItsOwnEmergencyFallback()
    {
        var tools = new ErrorDumpTools(new ThrowingDumpService());

        var result = await tools.GetErrorDumpAsync(
            new ErrorDumpRequest("EC-ORIGINAL"),
            CancellationToken.None);
        var text = Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text;

        Assert.True(result.IsError);
        Assert.Contains("Eternal Cycle Error Dump", text, StringComparison.Ordinal);
        Assert.Contains("Reference: EC-ORIGINAL", text, StringComparison.Ordinal);
        Assert.Contains("Secondary reference: EC-", text, StringComparison.Ordinal);
    }

    [Fact]
    public void DistributionSupportIsIndependentFromOriginalAuthorAttribution()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ec-derivative-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, "distribution.json");
        File.WriteAllText(path, """
            {
              "project": "Derivative Cycle",
              "originalProjectAuthor": "xmiq",
              "license": "Apache-2.0",
              "distributionIdentity": "Example derivative distribution",
              "supportPage": "https://support.example.invalid/issues",
              "supportDestinationType": "IssueTracker",
              "officialRepository": "https://example.invalid/derivative.git",
              "stableReleaseTag": "v1.0.0",
              "developmentRef": "main",
              "ruleSourceManifest": "docs/rules/rule-source-manifest.json"
            }
            """);
        try
        {
            var provider = new PackagedManagedDistributionIdentityProvider(
                Options.Create(new ManagedAdministrationOptions { DistributionMetadataFile = path }));

            var identity = provider.Get();

            Assert.Equal("xmiq", identity.OriginalProjectAuthor);
            Assert.Equal("https://support.example.invalid/issues", identity.SupportDestination);
            Assert.DoesNotContain("github.com/xmiq", identity.SupportDestination, StringComparison.OrdinalIgnoreCase);
            Assert.Null(identity.OfficialDistribution);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ErrorDumpHasNoSupportBundleOrNetworkDependency()
    {
        var dependencies = typeof(ErrorDumpService).GetConstructors().Single().GetParameters();

        Assert.DoesNotContain(dependencies, value => value.ParameterType.Name.Contains("SupportBundle", StringComparison.Ordinal));
        Assert.DoesNotContain(dependencies, value => value.ParameterType.Name.Contains("Http", StringComparison.Ordinal));
        Assert.Equal(1, ErrorDumpService.FormatVersion);
        Assert.Equal("1.0.0", File.ReadAllText(Path.Combine(RepositoryRoot(), "VERSION")).Trim());
    }

    [Fact]
    public void LicenseNoticeAndDistributionMetadataAreConsistent()
    {
        var root = RepositoryRoot();
        var license = File.ReadAllText(Path.Combine(root, "LICENSE"));
        var notice = File.ReadAllText(Path.Combine(root, "NOTICE"));
        var distribution = File.ReadAllText(Path.Combine(root, "DISTRIBUTION.json"));

        Assert.StartsWith("                                 Apache License", license, StringComparison.Ordinal);
        Assert.Contains("Version 2.0, January 2004", license, StringComparison.Ordinal);
        Assert.Contains("Copyright 2026 xmiq", notice, StringComparison.Ordinal);
        Assert.Contains("informational and does not modify the License", notice, StringComparison.Ordinal);
        Assert.Contains("\"license\": \"Apache-2.0\"", distribution, StringComparison.Ordinal);
        Assert.Contains("\"originalProjectAuthor\": \"xmiq\"", distribution, StringComparison.Ordinal);
    }

    private static ManagedConfigurationService ConfigurationService(
        IEnumerable<KeyValuePair<string, string?>> values) =>
        new(new ConfigurationBuilder().AddInMemoryCollection(values).Build());

    private static ManagedToolFailure DeserializeFailure(CallToolResult result)
    {
        Assert.True(result.IsError);
        var element = Assert.IsType<JsonElement>(result.StructuredContent);
        return JsonSerializer.Deserialize<ManagedToolFailure>(element.GetRawText())
            ?? throw new InvalidOperationException("Failure result was not deserializable.");
    }

    private static ManagedToolFailure Failure(string correlationId, string message) =>
        new(
            false,
            "INTERNAL_ERROR",
            message,
            false,
            false,
            false,
            correlationId,
            "Fixture",
            "Fixture",
            "InProcess",
            null,
            null,
            "Use error lookup.",
            ["ec_get_error_code", "ec_get_error_dump"],
            "ec_get_error_code",
            null);

    private static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "AGENTS.md")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }

    private sealed class CapturingRecorder : IManagedDiagnosticRecorder
    {
        public ManagedDiagnosticContext? Context { get; private set; }

        public Task<ManagedDiagnosticReceipt> RecordFailureAsync(
            ManagedDiagnosticContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            Context = context;
            return Task.FromResult(new ManagedDiagnosticReceipt(context.CorrelationId, "Fixture", false, true));
        }
    }

    private sealed class ThrowingBoundary : IMcpToolExceptionBoundary
    {
        public Task<CallToolResult> CreateFailureAsync(
            string toolName,
            string correlationId,
            Exception exception,
            CancellationToken cancellationToken) =>
            throw new IOException("fault-injected boundary failure");
    }

    private sealed class ThrowingDiagnosticReader : IManagedDiagnosticEvidenceReader
    {
        public Task<IReadOnlyList<ManagedOperationDiagnostic>> ReadAsync(
            string? correlationId,
            int maximumCount,
            CancellationToken cancellationToken) =>
            throw new IOException("fault-injected evidence failure");
    }

    private sealed class ThrowingDumpService : IErrorDumpService
    {
        public Task<EternalCycleErrorDump> GetAsync(
            ErrorDumpRequest request,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("fault-injected dump failure");
    }

    private sealed class StaticDistributionIdentityProvider : IManagedDistributionIdentityProvider
    {
        public ManagedDistributionIdentity Get() =>
            new(
                "Eternal Cycle",
                "xmiq",
                "Offline test distribution",
                null,
                null,
                OfficialDistribution: null,
                "Apache-2.0");
    }
}
