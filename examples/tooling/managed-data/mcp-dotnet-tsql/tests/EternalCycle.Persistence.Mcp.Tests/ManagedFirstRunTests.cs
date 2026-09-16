using EternalCycle.Persistence.Mcp;
using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class ManagedFirstRunTests
{
    private const string Approval = "APPROVE TEST SETUP";

    [Fact]
    public void CleanDatabaseIsSetupRequiredInsteadOfGenericFailure()
    {
        var report = Evaluate(
            campaignSchema: ManagedComponentStatus.Missing,
            domainSchema: ManagedComponentStatus.Missing,
            source: ManagedComponentStatus.NotConfigured);

        Assert.Equal(ManagedReadinessState.SetupRequired, report.State);
        Assert.Equal("RULE_SCHEMA_MISSING", report.ErrorCode);
        Assert.False(report.GameplayReady);
        Assert.True(report.AdministrativeActionRequired);
    }

    [Fact]
    public void InitializedEmptyRuleStoreRequiresPublication()
    {
        var report = Evaluate(
            campaignSchema: ManagedComponentStatus.Ready,
            domainSchema: ManagedComponentStatus.Ready,
            source: ManagedComponentStatus.Ready,
            published: 0);

        Assert.Equal(ManagedReadinessState.RulePublicationRequired, report.State);
        Assert.Equal("NO_PUBLISHED_RULE_RELEASE", report.ErrorCode);
    }

    [Fact]
    public void MissingRuleSourceIsDistinctFromMissingRelease()
    {
        var report = Evaluate(
            campaignSchema: ManagedComponentStatus.Ready,
            domainSchema: ManagedComponentStatus.Ready,
            source: ManagedComponentStatus.NotConfigured,
            published: 0);

        Assert.Equal(ManagedReadinessState.RuleSourceRequired, report.State);
        Assert.Equal("RULE_SOURCE_NOT_CONFIGURED", report.ErrorCode);
    }

    [Fact]
    public void ActiveReleaseAllowsDegradedOfflineSourceFallback()
    {
        var report = Evaluate(
            campaignSchema: ManagedComponentStatus.Ready,
            domainSchema: ManagedComponentStatus.Ready,
            source: ManagedComponentStatus.Unavailable,
            published: 1,
            activeReleaseId: "active",
            compatible: true,
            campaign: ManagedComponentStatus.Ready);

        Assert.Equal(ManagedReadinessState.Degraded, report.State);
        Assert.True(report.GameplayReady);
        Assert.Equal("RULE_SOURCE_UNAVAILABLE", report.ErrorCode);
    }

    [Fact]
    public async Task DiagnosticsRemainUsefulBeforeRuleSchemaExists()
    {
        var report = Evaluate(
            campaignSchema: ManagedComponentStatus.Ready,
            domainSchema: ManagedComponentStatus.Missing,
            source: ManagedComponentStatus.NotConfigured);
        var diagnostics = new ServiceDiagnostics(Resolver(), new StaticReadiness(report));

        var result = await diagnostics.GetReportAsync("campaign", CancellationToken.None);

        Assert.Equal("SetupRequired", result.RuleUpdateStatus);
        Assert.Equal("RULE_SCHEMA_MISSING", result.RuleUpdateDetail);
        Assert.Equal(ManagedComponentStatus.Missing, result.Readiness.RuleDomainSchema.Status);
    }

    [Fact]
    public async Task RuleContextReturnsActionableSetupResultWithoutCallingProvider()
    {
        var provider = new CountingContextProvider();
        var readiness = new StaticReadiness(Evaluate(
            campaignSchema: ManagedComponentStatus.Ready,
            domainSchema: ManagedComponentStatus.Missing,
            source: ManagedComponentStatus.NotConfigured));
        var tools = new RuleContextTools(provider, readiness);

        var result = await tools.GetRuleContextAsync(
            "campaign",
            "gameplay.resolve",
            ["turn"],
            8000,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("RULE_SCHEMA_MISSING", result.Code);
        Assert.Equal(0, provider.Calls);
    }

    [Fact]
    public async Task RuleContextReturnsActionableNoPublishedReleaseResult()
    {
        var provider = new CountingContextProvider();
        var tools = new RuleContextTools(
            provider,
            new StaticReadiness(Evaluate(
                ManagedComponentStatus.Ready,
                ManagedComponentStatus.Ready,
                ManagedComponentStatus.Ready)));

        var result = await tools.GetRuleContextAsync(
            "campaign",
            "gameplay.resolve",
            ["turn"],
            8000,
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("NO_PUBLISHED_RULE_RELEASE", result.Code);
        Assert.Equal(0, provider.Calls);
    }

    [Fact]
    public async Task BootstrapRequiresEnabledAdministrationAndExactApproval()
    {
        var executor = new FakeBootstrapExecutor(["001_campaign_persistence", "002_rule_domain"]);
        var disabled = Administration(executor, enabled: false);
        var noApproval = await disabled.BootstrapAsync(
            new BootstrapRequest(null, true, Approval),
            CancellationToken.None);

        var enabled = Administration(executor, enabled: true);
        var wrongApproval = await enabled.BootstrapAsync(
            new BootstrapRequest(null, true, "wrong"),
            CancellationToken.None);

        Assert.Equal("ADMINISTRATION_DISABLED", noApproval.Code);
        Assert.Equal("ADMINISTRATIVE_APPROVAL_REQUIRED", wrongApproval.Code);
        Assert.Equal(0, executor.Executions);
    }

    [Fact]
    public async Task ApprovedBootstrapIsRepeatSafeAndUsesOnlyPackagedMigrations()
    {
        var executor = new FakeBootstrapExecutor(["001_campaign_persistence", "002_rule_domain", "004_rule_source_configuration"]);
        var service = Administration(executor, enabled: true);

        var first = await service.BootstrapAsync(
            new BootstrapRequest(null, true, Approval),
            CancellationToken.None);
        var second = await service.BootstrapAsync(
            new BootstrapRequest(null, true, Approval),
            CancellationToken.None);

        Assert.True(first.Success);
        Assert.Equal("INITIALIZATION_COMPLETE", first.Code);
        Assert.True(second.Success);
        Assert.Equal("ALREADY_INITIALIZED", second.Code);
        Assert.Equal(2, executor.Executions);
        Assert.Equal(
            ["001_campaign_persistence", "002_rule_domain", "004_rule_source_configuration"],
            first.Data?.AppliedMigrationIds);
    }

    [Fact]
    public async Task OfficialSourceDefaultCanBeOverriddenAndSelectionPersists()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ec-distribution-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var metadata = Path.Combine(root, "distribution.json");
        await File.WriteAllTextAsync(metadata, """
            {
              "project": "Eternal Cycle",
              "officialRepository": "https://example.invalid/official.git",
              "stableReleaseTag": "v1.0.0",
              "developmentRef": "main",
              "ruleSourceManifest": "docs/rules/rule-source-manifest.json"
            }
            """);
        try
        {
            var store = new MemorySourceConfigurationStore();
            var service = Administration(
                new FakeBootstrapExecutor([]),
                enabled: true,
                sourceStore: store,
                metadataPath: metadata);

            var official = await service.ConfigureRuleSourceAsync(
                new RuleSourceSelectionRequest(true, null, null, null, true, Approval),
                CancellationToken.None);
            var custom = await service.ConfigureRuleSourceAsync(
                new RuleSourceSelectionRequest(
                    false,
                    "https://example.invalid/custom.git",
                    "stable",
                    "manifest.json",
                    true,
                    Approval),
                CancellationToken.None);

            Assert.True(official.Success);
            Assert.True(official.Data?.IsOfficial);
            Assert.Equal("refs/tags/v1.0.0", official.Data?.RequestedRef);
            Assert.True(custom.Success);
            Assert.False(custom.Data?.IsOfficial);
            Assert.Equal("https://example.invalid/custom.git", store.Current?.SourceLocation);
            Assert.Equal(2, store.Current?.ConfigurationRevision);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ExplicitInitialPublicationCanReachActiveStateWhileUpdatesRemainManualOrDisabled()
    {
        var store = new MemoryRuleStore();
        var source = new StaticSourceProvider(ValidSnapshot());
        var service = Administration(
            new FakeBootstrapExecutor([]),
            enabled: true,
            ruleStore: store,
            ruleSource: source,
            readiness: new StaticReadiness(Evaluate(
                campaignSchema: ManagedComponentStatus.Ready,
                domainSchema: ManagedComponentStatus.Ready,
                source: ManagedComponentStatus.Ready)));

        var result = await service.PublishInitialRulesAsync(
            new InitialRulePublicationRequest(true, Approval),
            CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Activated", result.Data?.Status);
        Assert.NotNull(await store.GetActiveAsync("eternal-cycle-core", CancellationToken.None));
    }

    [Fact]
    public async Task PersistedLocalSourceIsReusedWithoutRepositoryRootConfiguration()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ec-persisted-rules-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(root, "docs", "rules"));
        try
        {
            await File.WriteAllTextAsync(Path.Combine(root, "docs", "rules", "kernel.md"), "# Kernel\n\nRequired.");
            await File.WriteAllTextAsync(Path.Combine(root, "docs", "rules", "manifest.json"), """
                {
                  "repositoryVersion": "1.0.0",
                  "sources": [{
                    "ruleSourceId": "kernel",
                    "path": "docs/rules/kernel.md",
                    "layer": "RuntimeKernel",
                    "worldModelIds": [],
                    "moduleIds": [],
                    "campaignModes": ["*"],
                    "operations": ["*"],
                    "topics": [],
                    "dependencies": [],
                    "alwaysInclude": true
                  }]
                }
                """);
            RunGit(root, "init");
            RunGit(root, "config", "user.email", "fixture@example.invalid");
            RunGit(root, "config", "user.name", "Fixture");
            RunGit(root, "add", ".");
            RunGit(root, "commit", "-m", "fixture");
            var store = new MemorySourceConfigurationStore();
            _ = await store.SaveAsync(
                new RuleSourceConfiguration(
                    "eternal-cycle-core",
                    "Git",
                    root,
                    "HEAD",
                    "docs/rules/manifest.json",
                    false,
                    1,
                    DateTimeOffset.UtcNow),
                CancellationToken.None);
            var provider = new GitRuleSourceProvider(
                Options.Create(new ManagedRuleServiceOptions()),
                store,
                Options.Create(new ManagedAdministrationOptions()));

            var snapshot = await provider.GetSnapshotAsync(CancellationToken.None);

            Assert.Equal("1.0.0", snapshot.RepositoryVersion);
            Assert.Single(snapshot.Documents);
            Assert.Matches("^[A-F0-9]{40,64}$", snapshot.SourceIdentity);
        }
        finally
        {
            foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }

            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ResumeDiscoveryHidesOpaqueIdsWhenSelectionIsUnnecessary()
    {
        var none = CampaignDiscovery.ResolveResume([]);
        var only = new CampaignDescriptor("internal-123", "Second Dawn", null, "world", "namespace");
        var one = CampaignDiscovery.ResolveResume([only]);
        var many = CampaignDiscovery.ResolveResume([
            only,
            new CampaignDescriptor("internal-456", "Falling Stars", "Later campaign", "world", "namespace")]);

        Assert.Equal("CAMPAIGN_REQUIRED", none.Status);
        Assert.Equal("internal-123", one.SelectedCampaign?.CampaignId);
        Assert.Empty(one.Choices);
        Assert.Equal("CAMPAIGN_SELECTION_REQUIRED", many.Status);
        Assert.All(many.Choices, choice => Assert.False(string.IsNullOrWhiteSpace(choice.DisplayName)));
    }

    [Fact]
    public void MigrationAssetsAreBoundedAndAdditive()
    {
        var schemaDirectory = Path.Combine(AppContext.BaseDirectory, "Schema");
        var files = Directory.GetFiles(schemaDirectory, "*.sql").Select(Path.GetFileName).Order().ToArray();
        var combined = string.Join('\n', Directory.GetFiles(schemaDirectory, "*.sql").Select(File.ReadAllText));

        Assert.Contains("001_initial.template.sql", files);
        Assert.Contains("002_rule_domain.template.sql", files);
        Assert.Contains("003_campaign_directory.template.sql", files);
        Assert.Contains("004_rule_source_configuration.template.sql", files);
        Assert.DoesNotContain("DROP TABLE", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DROP SCHEMA", combined, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IF COL_LENGTH", File.ReadAllText(Path.Combine(schemaDirectory, "003_campaign_directory.template.sql")));
        Assert.Contains("IF OBJECT_ID", File.ReadAllText(Path.Combine(schemaDirectory, "004_rule_source_configuration.template.sql")));
    }

    private static ManagedReadinessReport Evaluate(
        ManagedComponentStatus campaignSchema,
        ManagedComponentStatus domainSchema,
        ManagedComponentStatus source,
        int published = 0,
        string? activeReleaseId = null,
        bool compatible = false,
        ManagedComponentStatus campaign = ManagedComponentStatus.NotRequested) =>
        ManagedReadinessEvaluator.Evaluate(
            new ManagedInfrastructureSnapshot(
                ManagedComponentStatus.Ready,
                campaignSchema,
                domainSchema,
                source,
                published,
                activeReleaseId,
                activeReleaseId is null ? null : "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                compatible,
                campaign,
                null,
                false),
            campaignRequested: campaign is not ManagedComponentStatus.NotRequested);

    private static IManagedAdministrationService Administration(
        FakeBootstrapExecutor executor,
        bool enabled,
        IRuleSourceConfigurationStore? sourceStore = null,
        string? metadataPath = null,
        IPublishedRuleStore? ruleStore = null,
        IRuleSourceProvider? ruleSource = null,
        IManagedReadinessService? readiness = null)
    {
        sourceStore ??= new MemorySourceConfigurationStore();
        ruleStore ??= new MemoryRuleStore();
        ruleSource ??= new StaticSourceProvider(ValidSnapshot());
        readiness ??= new StaticReadiness(Evaluate(
            ManagedComponentStatus.Ready,
            ManagedComponentStatus.Ready,
            ManagedComponentStatus.Ready,
            published: 1,
            activeReleaseId: "active",
            compatible: true));
        var ruleOptions = Options.Create(new ManagedRuleServiceOptions());
        var coordinator = new ManagedRulePublicationCoordinator(
            ruleSource,
            ruleStore,
            ruleOptions,
            NullLogger<ManagedRulePublicationCoordinator>.Instance);
        return new ManagedAdministrationService(
            Options.Create(new ManagedAdministrationOptions
            {
                Enabled = enabled,
                ApprovalPhrase = Approval,
                DistributionMetadataFile = metadataPath ?? "missing-test-metadata.json"
            }),
            ruleOptions,
            executor,
            readiness,
            sourceStore,
            coordinator,
            new FakeCampaignDirectory());
    }

    private static ConfiguredCampaignSchemaResolver Resolver() =>
        new(Options.Create(new SqlServerPersistenceOptions()));

    private static void RunGit(string root, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start Git fixture process.");
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(process.StandardError.ReadToEnd());
        }
    }

    private static RuleSourceSnapshot ValidSnapshot()
    {
        var documents = new[]
        {
            new RuleSourceDocument(
                "kernel",
                "kernel.md",
                "# Kernel\n\nRequired runtime rule.",
                new RuleSourceMetadata(
                    RuleLayer.RuntimeKernel,
                    [],
                    [],
                    ["NORMAL"],
                    ["gameplay.resolve"],
                    [],
                    0,
                    true))
        };
        return new RuleSourceSnapshot(
            "Git",
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            "1.0.0+rc",
            documents,
            DateTimeOffset.UnixEpoch);
    }

    private sealed class StaticReadiness(ManagedReadinessReport report) : IManagedReadinessService
    {
        public Task<ManagedReadinessReport> GetReadinessAsync(
            string? campaignId,
            CancellationToken cancellationToken) => Task.FromResult(report);
    }

    private sealed class CountingContextProvider : IRuleContextProvider
    {
        public int Calls { get; private set; }

        public Task<RuleContextResult> GetContextAsync(
            RuleContextRequest request,
            CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("Provider should not have been called.");
        }
    }

    private sealed class FakeBootstrapExecutor(IReadOnlyList<string> firstResult) : ISchemaBootstrapExecutor
    {
        public int Executions { get; private set; }

        public Task<BootstrapPlan> PlanAsync(string? campaignId, CancellationToken cancellationToken) =>
            Task.FromResult(new BootstrapPlan(
                Executions == 0 && firstResult.Count > 0,
                Executions == 0 ? firstResult : [],
                ["Eternal Cycle-owned schemas"],
                Approval,
                "Test plan"));

        public Task<IReadOnlyList<string>> ExecuteAsync(string? campaignId, CancellationToken cancellationToken)
        {
            Executions++;
            return Task.FromResult(Executions == 1 ? firstResult : (IReadOnlyList<string>)[]);
        }
    }

    private sealed class MemorySourceConfigurationStore : IRuleSourceConfigurationStore
    {
        public RuleSourceConfiguration? Current { get; private set; }

        public Task<RuleSourceConfiguration?> GetAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult(Current);

        public Task<RuleSourceConfiguration> SaveAsync(
            RuleSourceConfiguration configuration,
            CancellationToken cancellationToken)
        {
            Current = configuration with { ConfigurationRevision = (Current?.ConfigurationRevision ?? 0) + 1 };
            return Task.FromResult(Current);
        }
    }

    private sealed class StaticSourceProvider(RuleSourceSnapshot snapshot) : IRuleSourceProvider
    {
        public Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken) =>
            Task.FromResult(snapshot);
    }

    private sealed class MemoryRuleStore : IPublishedRuleStore
    {
        private readonly Dictionary<string, PublishedRuleRelease> releases = new(StringComparer.Ordinal);
        private string? active;

        public Task<PublishedRuleRelease?> GetActiveAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult(active is null ? null : releases[active]);

        public Task<PublishedRuleRelease?> FindBySourceAsync(string rulesetId, string sourceIdentity, CancellationToken cancellationToken) =>
            Task.FromResult(releases.Values.FirstOrDefault(value => value.SourceIdentity == sourceIdentity));

        public Task<PublishedRuleRelease?> GetByIdAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken) =>
            Task.FromResult(releases.GetValueOrDefault(ruleReleaseId));

        public Task<RuleUpdateCheck?> GetLatestUpdateCheckAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult<RuleUpdateCheck?>(null);

        public Task StageCandidateAsync(PublishedRuleRelease release, CancellationToken cancellationToken)
        {
            releases[release.RuleReleaseId] = release;
            return Task.CompletedTask;
        }

        public Task SetStateAsync(string ruleReleaseId, RuleReleaseState state, string? failureReason, CancellationToken cancellationToken)
        {
            releases[ruleReleaseId] = releases[ruleReleaseId] with { State = state, FailureReason = failureReason };
            return Task.CompletedTask;
        }

        public Task ActivateAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken)
        {
            releases[ruleReleaseId] = releases[ruleReleaseId] with { State = RuleReleaseState.Active };
            active = ruleReleaseId;
            return Task.CompletedTask;
        }

        public Task RecordUpdateCheckAsync(RuleUpdateCheck updateCheck, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeCampaignDirectory : ICampaignDirectoryService
    {
        public Task<IReadOnlyList<CampaignDescriptor>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<CampaignDescriptor>>([]);

        public Task<CampaignDescriptor> CreateAsync(string displayName, string? description, CancellationToken cancellationToken) =>
            Task.FromResult(new CampaignDescriptor("campaign-id", displayName, description, "world", "namespace"));
    }
}
