using EternalCycle.Persistence.Mcp;
using System.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class ManagedRulePublicationTests
{
    private const string CommitA = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    private const string CommitB = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";

    [Fact]
    public async Task CanonicalSnapshotBecomesValidatedPublishedActiveRelease()
    {
        var source = new FakeSourceProvider(Snapshot(CommitA));
        var store = new InMemoryRuleStore();
        var coordinator = Coordinator(source, store);

        var result = await coordinator.CheckForUpdateAsync(CancellationToken.None);
        var active = await store.GetActiveAsync("eternal-cycle-core", CancellationToken.None);

        Assert.Equal("Activated", result.Status);
        Assert.NotNull(active);
        Assert.Equal(CommitA, active.SourceIdentity);
        Assert.Equal(
            [RuleReleaseState.Candidate, RuleReleaseState.Validated, RuleReleaseState.Published, RuleReleaseState.Active],
            store.StateHistory);
    }

    [Fact]
    public async Task InvalidCandidatePreservesPreviousActiveRelease()
    {
        var store = new InMemoryRuleStore();
        store.SeedActive(Release("active", CommitA, ValidIndex()));
        var invalid = new RuleSourceSnapshot(
            "Git",
            CommitB,
            "post-v1",
            [Document("core-only", RuleLayer.Core, "No kernel.")],
            DateTimeOffset.UnixEpoch);
        var coordinator = Coordinator(new FakeSourceProvider(invalid), store);

        var result = await coordinator.CheckForUpdateAsync(CancellationToken.None);
        var active = await store.GetActiveAsync("eternal-cycle-core", CancellationToken.None);

        Assert.Equal("CandidateFailed", result.Status);
        Assert.True(result.ActiveReleasePreserved);
        Assert.Equal("active", active?.RuleReleaseId);
        Assert.Contains(RuleReleaseState.Failed, store.StateHistory);
    }

    [Fact]
    public async Task SourceFailureAndOfflineOperationPreserveActiveRelease()
    {
        var store = new InMemoryRuleStore();
        store.SeedActive(Release("active", CommitA, ValidIndex()));
        var coordinator = Coordinator(new FakeSourceProvider(new IOException("offline")), store);

        var result = await coordinator.CheckForUpdateAsync(CancellationToken.None);
        var updateCheck = await store.GetLatestUpdateCheckAsync("eternal-cycle-core", CancellationToken.None);

        Assert.Equal("SourceUnavailableUsingActive", result.Status);
        Assert.Equal("active", result.ActiveReleaseId);
        Assert.True(result.ActiveReleasePreserved);
        Assert.Equal("Degraded", updateCheck?.Outcome);
    }

    [Fact]
    public async Task UnchangedSourceDoesNotRepublish()
    {
        var store = new InMemoryRuleStore();
        store.SeedActive(Release("active", CommitA, ValidIndex()));
        var coordinator = Coordinator(new FakeSourceProvider(Snapshot(CommitA)), store);

        var result = await coordinator.CheckForUpdateAsync(CancellationToken.None);

        Assert.Equal("Unchanged", result.Status);
        Assert.Equal(1, store.ReleaseCount);
    }

    [Fact]
    public async Task ChangedSourceCreatesNewCandidateAndAtomicActivation()
    {
        var store = new InMemoryRuleStore();
        store.SeedActive(Release("old", CommitA, ValidIndex()));
        var coordinator = Coordinator(new FakeSourceProvider(Snapshot(CommitB)), store);

        var result = await coordinator.CheckForUpdateAsync(CancellationToken.None);
        var active = await store.GetActiveAsync("eternal-cycle-core", CancellationToken.None);

        Assert.Equal("Activated", result.Status);
        Assert.NotEqual("old", active?.RuleReleaseId);
        Assert.Equal(CommitB, active?.SourceIdentity);
    }

    [Fact]
    public async Task ManualActivationPolicyLeavesValidatedPublicationInactive()
    {
        var store = new InMemoryRuleStore();
        store.SeedActive(Release("old", CommitA, ValidIndex()));
        var coordinator = Coordinator(
            new FakeSourceProvider(Snapshot(CommitB)),
            store,
            RuleActivationPolicy.Manual);

        var result = await coordinator.CheckForUpdateAsync(CancellationToken.None);
        var active = await store.GetActiveAsync("eternal-cycle-core", CancellationToken.None);

        Assert.Equal("AwaitingAdministratorActivation", result.Status);
        Assert.Equal("old", active?.RuleReleaseId);
        Assert.Contains(RuleReleaseState.Published, store.StateHistory);
    }

    [Fact]
    public async Task RuntimeRetrievalUsesPublishedStoreWithoutCallingSourceProvider()
    {
        var store = new InMemoryRuleStore();
        store.SeedActive(Release("active", CommitA, ValidIndex()));
        var provider = new PublishedRuleContextProvider(
            store,
            Options.Create(new ManagedRuleServiceOptions()),
            new ConfiguredCampaignSchemaResolver(Options.Create(new SqlServerPersistenceOptions())));

        var result = await provider.GetContextAsync(
            new RuleContextRequest("campaign", "gameplay.resolve", ["combat"], string.Empty),
            CancellationToken.None);

        Assert.Equal("active", result.RuleReleaseId);
        Assert.Equal(CommitA, result.SourceIdentity);
        Assert.True(result.EstimatedTokens <= RuleCompiler.DefaultContextBudget);
        Assert.DoesNotContain(result.Chunks, chunk => chunk.RuleSourceId == "world-b");
    }

    [Fact]
    public async Task RuntimeRetrievalIncludesExplicitDependencies()
    {
        var store = new InMemoryRuleStore();
        var snapshot = new RuleSourceSnapshot(
            "Git",
            CommitA,
            "1.0.0+fr019",
            [
                Document("kernel", RuleLayer.RuntimeKernel, "Mandatory kernel.", alwaysInclude: true),
                Document("foundation", RuleLayer.Core, "Required foundation.", topics: ["foundation"]),
                Document("combat", RuleLayer.Core, "Combat rule.", topics: ["combat"], dependencies: ["foundation"])
            ],
            DateTimeOffset.UnixEpoch);
        store.SeedActive(Release("active", CommitA, RuleCompiler.Compile(snapshot.RepositoryVersion, snapshot.Documents)));
        var provider = new PublishedRuleContextProvider(
            store,
            Options.Create(new ManagedRuleServiceOptions()),
            new ConfiguredCampaignSchemaResolver(Options.Create(new SqlServerPersistenceOptions())));

        var result = await provider.GetContextAsync(
            new RuleContextRequest("campaign", "gameplay.resolve", ["combat"], string.Empty),
            CancellationToken.None);

        Assert.Contains(result.Chunks, chunk => chunk.RuleSourceId == "combat");
        Assert.Contains(result.Chunks, chunk => chunk.RuleSourceId == "foundation");
    }

    [Fact]
    public async Task IncompatibleActiveRuleReleaseIsRejected()
    {
        var store = new InMemoryRuleStore();
        store.SeedActive(Release("active", CommitA, RuleCompiler.Compile("2.0.0", Snapshot(CommitA).Documents)));
        var provider = new PublishedRuleContextProvider(
            store,
            Options.Create(new ManagedRuleServiceOptions()),
            new ConfiguredCampaignSchemaResolver(Options.Create(new SqlServerPersistenceOptions())));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetContextAsync(
            new RuleContextRequest("campaign", "gameplay.resolve", ["combat"], string.Empty),
            CancellationToken.None));

        Assert.Contains("incompatible", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DependencyCycleIsRejectedBeforePublication()
    {
        var documents = new[]
        {
            Document("kernel", RuleLayer.RuntimeKernel, "Kernel.", alwaysInclude: true),
            Document("one", RuleLayer.Core, "One.", dependencies: ["two"]),
            Document("two", RuleLayer.Core, "Two.", dependencies: ["one"])
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            RuleCompiler.Compile("1.0.0", documents));

        Assert.Contains("cycle", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DiagnosticsAreSanitizedAndIdentifyReferenceImplementation()
    {
        var store = new InMemoryRuleStore();
        store.SeedActive(Release("active", CommitA, ValidIndex()));
        var diagnostics = new ServiceDiagnostics(
            new ConfiguredCampaignSchemaResolver(Options.Create(new SqlServerPersistenceOptions())),
            store);

        var report = await diagnostics.GetReportAsync("campaign", CancellationToken.None);
        var serialized = System.Text.Json.JsonSerializer.Serialize(report);

        Assert.Contains("reference implementation", report.Implementation, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("MANAGED", report.PersistenceStrategy);
        Assert.Equal(CommitA, report.CanonicalSourceIdentity);
        Assert.Equal("ActiveValidatedRelease", report.RuleUpdateStatus);
        Assert.DoesNotContain("ConnectionString", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"campaignCanon\"", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GitSourceProviderRecordsImmutableCommitShaFromLocalRepository()
    {
        var root = Path.Combine(Path.GetTempPath(), $"eternal-cycle-rules-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(root, "docs", "rules"));
        try
        {
            File.WriteAllText(Path.Combine(root, "docs", "rules", "kernel.md"), "# Kernel\n\nRequired.");
            File.WriteAllText(
                Path.Combine(root, "docs", "rules", "manifest.json"),
                """
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

            var provider = new GitRuleSourceProvider(Options.Create(new ManagedRuleServiceOptions
            {
                GitSource = new GitRuleSourceOptions
                {
                    RepositoryRoot = root,
                    Ref = "HEAD",
                    ManifestPath = "docs/rules/manifest.json"
                }
            }));

            var snapshot = await provider.GetSnapshotAsync(CancellationToken.None);

            Assert.Matches("^[A-F0-9]{40,64}$", snapshot.SourceIdentity);
            Assert.Equal("1.0.0", snapshot.RepositoryVersion);
            Assert.Single(snapshot.Documents);
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

    private static ManagedRulePublicationCoordinator Coordinator(
        IRuleSourceProvider source,
        IPublishedRuleStore store,
        RuleActivationPolicy activationPolicy = RuleActivationPolicy.Automatic) =>
        new(
            source,
            store,
            Options.Create(new ManagedRuleServiceOptions { ActivationPolicy = activationPolicy }),
            NullLogger<ManagedRulePublicationCoordinator>.Instance);

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

    private static RuleSourceSnapshot Snapshot(string sourceIdentity) =>
        new(
            "Git",
            sourceIdentity,
            "1.0.0+fr019",
            [
                Document("kernel", RuleLayer.RuntimeKernel, "Mandatory kernel.", alwaysInclude: true),
                Document("core", RuleLayer.Core, "Core combat.", topics: ["combat"]),
                Document("world-a", RuleLayer.World, "World A.", worlds: ["eternal-cycle-standard"], topics: ["combat"]),
                Document("world-b", RuleLayer.World, "World B.", worlds: ["world-b"], topics: ["combat"])
            ],
            DateTimeOffset.UnixEpoch);

    private static CompiledRuleIndex ValidIndex() =>
        RuleCompiler.Compile("1.0.0+fr019", Snapshot(CommitA).Documents);

    private static PublishedRuleRelease Release(
        string releaseId,
        string sourceIdentity,
        CompiledRuleIndex index) =>
        new(
            releaseId,
            "eternal-cycle-core",
            "Git",
            sourceIdentity,
            index.RepositoryVersion,
            "1",
            RuleReleaseState.Active,
            index,
            DateTimeOffset.UnixEpoch);

    private static RuleSourceDocument Document(
        string id,
        RuleLayer layer,
        string content,
        IReadOnlyList<string>? worlds = null,
        IReadOnlyList<string>? topics = null,
        bool alwaysInclude = false,
        IReadOnlyList<string>? dependencies = null) =>
        new(
            id,
            $"fixtures/{id}.md",
            $"# {id}\n\n{content}",
            new RuleSourceMetadata(
                layer,
                worlds ?? [],
                [],
                ["NORMAL"],
                ["gameplay.resolve"],
                topics ?? [],
                0,
                alwaysInclude,
                dependencies ?? []));

    private sealed class FakeSourceProvider : IRuleSourceProvider
    {
        private readonly RuleSourceSnapshot? snapshot;
        private readonly Exception? exception;

        public FakeSourceProvider(RuleSourceSnapshot snapshot) => this.snapshot = snapshot;

        public FakeSourceProvider(Exception exception) => this.exception = exception;

        public Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken) =>
            exception is not null
                ? Task.FromException<RuleSourceSnapshot>(exception)
                : Task.FromResult(snapshot!);
    }

    private sealed class InMemoryRuleStore : IPublishedRuleStore
    {
        private readonly Dictionary<string, PublishedRuleRelease> releases = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> activeByRuleset = new(StringComparer.Ordinal);
        private readonly Dictionary<string, RuleUpdateCheck> updateChecks = new(StringComparer.Ordinal);

        public List<RuleReleaseState> StateHistory { get; } = [];

        public int ReleaseCount => releases.Count;

        public void SeedActive(PublishedRuleRelease release)
        {
            releases[release.RuleReleaseId] = release with { State = RuleReleaseState.Active };
            activeByRuleset[release.RulesetId] = release.RuleReleaseId;
        }

        public Task<PublishedRuleRelease?> GetActiveAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult(activeByRuleset.TryGetValue(rulesetId, out var id)
                ? releases[id]
                : null);

        public Task<PublishedRuleRelease?> FindBySourceAsync(
            string rulesetId,
            string sourceIdentity,
            CancellationToken cancellationToken) =>
            Task.FromResult(releases.Values.FirstOrDefault(release =>
                release.RulesetId == rulesetId && release.SourceIdentity == sourceIdentity));

        public Task<PublishedRuleRelease?> GetByIdAsync(
            string rulesetId,
            string ruleReleaseId,
            CancellationToken cancellationToken) =>
            Task.FromResult(releases.TryGetValue(ruleReleaseId, out var release) && release.RulesetId == rulesetId
                ? release
                : null);

        public Task<RuleUpdateCheck?> GetLatestUpdateCheckAsync(
            string rulesetId,
            CancellationToken cancellationToken) =>
            Task.FromResult(updateChecks.TryGetValue(rulesetId, out var updateCheck) ? updateCheck : null);

        public Task StageCandidateAsync(PublishedRuleRelease release, CancellationToken cancellationToken)
        {
            releases.Add(release.RuleReleaseId, release);
            StateHistory.Add(RuleReleaseState.Candidate);
            return Task.CompletedTask;
        }

        public Task SetStateAsync(
            string ruleReleaseId,
            RuleReleaseState state,
            string? failureReason,
            CancellationToken cancellationToken)
        {
            if (releases.TryGetValue(ruleReleaseId, out var release))
            {
                releases[ruleReleaseId] = release with { State = state, FailureReason = failureReason };
                StateHistory.Add(state);
            }

            return Task.CompletedTask;
        }

        public Task ActivateAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken)
        {
            var release = releases[ruleReleaseId];
            if (release.State != RuleReleaseState.Published)
            {
                throw new InvalidOperationException("Only a published release may activate.");
            }

            if (activeByRuleset.TryGetValue(rulesetId, out var oldId))
            {
                releases[oldId] = releases[oldId] with { State = RuleReleaseState.Published };
            }

            releases[ruleReleaseId] = release with { State = RuleReleaseState.Active };
            activeByRuleset[rulesetId] = ruleReleaseId;
            StateHistory.Add(RuleReleaseState.Active);
            return Task.CompletedTask;
        }

        public Task RecordUpdateCheckAsync(
            RuleUpdateCheck updateCheck,
            CancellationToken cancellationToken)
        {
            updateChecks[updateCheck.RulesetId] = updateCheck;
            return Task.CompletedTask;
        }
    }
}
