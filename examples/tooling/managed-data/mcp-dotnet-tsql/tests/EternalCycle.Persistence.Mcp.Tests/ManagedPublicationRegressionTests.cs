using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class ManagedPublicationRegressionTests
{
    private const string CommitA = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

    [Fact]
    public async Task OfficialManifestHasMeasuredBoundedPublicationPlan()
    {
        var repositoryRoot = FindRepositoryRoot();
        var fixtureRoot = Path.Combine(Path.GetTempPath(), $"ec-official-manifest-{Guid.NewGuid():N}");
        Directory.CreateDirectory(fixtureRoot);
        try
        {
            var manifestPath = Path.Combine(repositoryRoot, "docs", "rules", "rule-source-manifest.json");
            using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));
            foreach (var source in manifest.RootElement.GetProperty("sources").EnumerateArray())
            {
                var relativePath = source.GetProperty("path").GetString()
                    ?? throw new InvalidOperationException("Official source path was null.");
                var destination = Path.Combine(fixtureRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)), destination);
            }

            var fixtureManifest = Path.Combine(fixtureRoot, "docs", "rules", "rule-source-manifest.json");
            Directory.CreateDirectory(Path.GetDirectoryName(fixtureManifest)!);
            File.Copy(manifestPath, fixtureManifest);
            RunGit(fixtureRoot, "init");
            RunGit(fixtureRoot, "config", "user.email", "fixture@example.invalid");
            RunGit(fixtureRoot, "config", "user.name", "Fixture");
            RunGit(fixtureRoot, "add", ".");
            RunGit(fixtureRoot, "commit", "-m", "official manifest fixture");
            var provider = new GitRuleSourceProvider(Options.Create(new ManagedRuleServiceOptions
            {
                GitSource = new GitRuleSourceOptions
                {
                    RepositoryRoot = fixtureRoot,
                    Ref = "HEAD",
                    ManifestPath = "docs/rules/rule-source-manifest.json"
                }
            }));

            var snapshot = await provider.GetSnapshotAsync(CancellationToken.None);
            var plan = RulePublicationWritePlan.Create(
                RuleCompiler.Compile(snapshot.RepositoryVersion, snapshot.Documents));

            Assert.Equal(8, snapshot.Documents.Count);
            Assert.Equal(147, plan.ChunkRows);
            Assert.Equal(990, plan.SelectorRows);
            Assert.Equal(768, plan.DependencyRows);
            Assert.Equal(9, plan.StagingCommandCount);
        }
        finally
        {
            DeleteTree(fixtureRoot);
        }
    }

    [Fact]
    public async Task ManagedNoCheckoutCacheReadsRealisticManifestAndPublishes()
    {
        var fixtureRoot = Path.Combine(Path.GetTempPath(), $"ec-managed-source-{Guid.NewGuid():N}");
        var cacheRoot = Path.Combine(Path.GetTempPath(), $"ec-managed-cache-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(fixtureRoot, "docs", "rules"));
        try
        {
            var entries = new List<object>();
            for (var documentIndex = 0; documentIndex < 8; documentIndex++)
            {
                var id = documentIndex == 0 ? "runtime-kernel" : $"rules-{documentIndex}";
                var path = $"docs/rules/{id}.md";
                var content = new StringBuilder($"# {id}\n\nReusable Eternal Cycle rule source.\n");
                for (var section = 0; section < 20; section++)
                {
                    content.Append($"\n## {id} section {section}\n\n");
                    content.Append(string.Join(' ', Enumerable.Repeat(
                        "Canonical ownership, continuity, embodiment, uncertainty, and validation remain explicit.",
                        20)));
                    content.AppendLine();
                }

                File.WriteAllText(Path.Combine(fixtureRoot, path.Replace('/', Path.DirectorySeparatorChar)), content.ToString());
                entries.Add(new
                {
                    ruleSourceId = id,
                    path,
                    layer = documentIndex == 0 ? "RuntimeKernel" : "Core",
                    worldModelIds = Array.Empty<string>(),
                    moduleIds = new[] { "core" },
                    campaignModes = new[] { "NORMAL" },
                    operations = new[] { "gameplay.resolve", "turn.commit" },
                    topics = new[] { "persistence", "continuity" },
                    dependencies = documentIndex > 0 ? new[] { "runtime-kernel" } : Array.Empty<string>(),
                    priority = 100 - documentIndex,
                    alwaysInclude = documentIndex == 0
                });
            }

            var manifestPath = Path.Combine(fixtureRoot, "docs", "rules", "manifest.json");
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(new
            {
                manifestFormatVersion = 1,
                compilerContractVersion = "1",
                rulesetId = "eternal-cycle-core",
                repositoryVersion = "1.0.0+managed-regression",
                sources = entries
            }));
            RunGit(fixtureRoot, "init");
            RunGit(fixtureRoot, "config", "user.email", "fixture@example.invalid");
            RunGit(fixtureRoot, "config", "user.name", "Fixture");
            RunGit(fixtureRoot, "add", ".");
            RunGit(fixtureRoot, "commit", "-m", "realistic managed fixture");
            var commit = RunGit(fixtureRoot, "rev-parse", "HEAD").Trim();
            var sourceUri = new Uri(fixtureRoot + Path.DirectorySeparatorChar).AbsoluteUri;
            var configuration = new RuleSourceConfiguration(
                "eternal-cycle-core",
                "Git",
                sourceUri,
                commit,
                "docs/rules/manifest.json",
                false,
                1,
                DateTimeOffset.UtcNow);
            var provider = new GitRuleSourceProvider(
                Options.Create(new ManagedRuleServiceOptions()),
                new StaticSourceConfigurationStore(configuration),
                Options.Create(new ManagedAdministrationOptions { ManagedRuleCacheDirectory = cacheRoot }));

            var stages = new List<RulePublicationStage>();
            var snapshot = await provider.GetSnapshotAsync(stages.Add, CancellationToken.None);
            var index = RuleCompiler.Compile(snapshot.RepositoryVersion, snapshot.Documents);
            var writePlan = RulePublicationWritePlan.Create(index);
            var store = new MemoryRuleStore();
            var publication = await Coordinator(new StaticSource(snapshot), store)
                .CheckForUpdateAsync(CancellationToken.None);

            var managedRepository = Assert.Single(Directory.GetDirectories(cacheRoot));
            var payload = Assert.Single(Directory.GetDirectories(
                Path.Combine(managedRepository, "snapshots")));
            Assert.False(Directory.Exists(Path.Combine(managedRepository, ".git")));
            Assert.True(File.Exists(Path.Combine(payload, "docs", "rules", "manifest.json")));
            Assert.True(File.Exists(Path.Combine(payload, ".eternal-cycle-rule-source.json")));
            Assert.False(Directory.Exists(Path.Combine(payload, "examples")));
            Assert.False(Directory.Exists(Path.Combine(payload, "design")));
            Assert.Equal(8, snapshot.Documents.Count);
            Assert.Contains(RulePublicationStage.ReadManifest, stages);
            Assert.Contains(RulePublicationStage.ReadRuleDocuments, stages);
            Assert.True(writePlan.ChunkRows >= 120);
            Assert.True(writePlan.SelectorRows > writePlan.ChunkRows);
            Assert.True(writePlan.StagingCommandCount < 20);
            Assert.Equal("Activated", publication.Status);
            Assert.Equal(1, store.StageCalls);
        }
        finally
        {
            DeleteTree(fixtureRoot);
            DeleteTree(cacheRoot);
        }
    }

    [Theory]
    [InlineData(RulePublicationStage.AcquireSource, "RULE_SOURCE_ACQUISITION_FAILED")]
    [InlineData(RulePublicationStage.ResolveRef, "RULE_SOURCE_REF_RESOLUTION_FAILED")]
    [InlineData(RulePublicationStage.ReadManifest, "RULE_SOURCE_READ_FAILED")]
    [InlineData(RulePublicationStage.ReadRuleDocuments, "RULE_SOURCE_READ_FAILED")]
    public async Task SourceFailuresRetainStageAndStructuredCode(
        RulePublicationStage stage,
        string expectedCode)
    {
        var recorder = new CapturingRecorder();
        var result = await Coordinator(
            new StageFailureSource(stage, new IOException("source failed")),
            new MemoryRuleStore(),
            recorder: recorder).CheckForUpdateAsync(CancellationToken.None);

        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(stage.ToString(), result.Stage);
        Assert.NotNull(result.CorrelationId);
        Assert.Equal(result.CorrelationId, recorder.Context?.CorrelationId);
        Assert.Equal(stage, recorder.Context?.Stage);
    }

    [Fact]
    public async Task CompilerFailureIsNotMisclassifiedAsSourceUnavailable()
    {
        var duplicate = HealthySnapshot() with
        {
            Documents = [Document("kernel", RuleLayer.RuntimeKernel), Document("kernel", RuleLayer.Core)]
        };
        var store = new MemoryRuleStore();
        store.SeedActive(Release("active", "existing-source", RuleReleaseState.Active));

        var result = await Coordinator(new StaticSource(duplicate), store)
            .CheckForUpdateAsync(CancellationToken.None);

        Assert.Equal("RULE_COMPILATION_FAILED", result.ErrorCode);
        Assert.Equal(nameof(RulePublicationStage.Compile), result.Stage);
        Assert.False(result.RetrySafe);
        Assert.True(result.ActiveReleasePreserved);
        Assert.Equal("active", result.ActiveReleaseId);
        Assert.Equal("active", store.Active?.RuleReleaseId);
    }

    [Fact]
    public async Task ValidationFailureIsDistinctAndCandidateRemainsRetryable()
    {
        var invalid = HealthySnapshot() with { Documents = [Document("core", RuleLayer.Core)] };
        var store = new MemoryRuleStore();

        var result = await Coordinator(new StaticSource(invalid), store)
            .CheckForUpdateAsync(CancellationToken.None);

        Assert.Equal("RULE_VALIDATION_FAILED", result.ErrorCode);
        Assert.Equal(RuleReleaseState.Failed, Assert.Single(store.Releases).State);
    }

    [Theory]
    [InlineData(RulePublicationStage.Stage, "RULE_STORE_STAGE_FAILED")]
    [InlineData(RulePublicationStage.Publish, "RULE_PUBLICATION_FAILED")]
    [InlineData(RulePublicationStage.Activate, "RULE_ACTIVATION_FAILED")]
    public async Task StoreAndActivationFailuresAreStageSpecific(
        RulePublicationStage failureStage,
        string expectedCode)
    {
        var store = new MemoryRuleStore { FailureStage = failureStage };

        var result = await Coordinator(new StaticSource(HealthySnapshot()), store)
            .CheckForUpdateAsync(CancellationToken.None);

        Assert.Equal(expectedCode, result.ErrorCode);
        Assert.Equal(failureStage.ToString(), result.Stage);
        Assert.True(result.RetrySafe);
    }

    [Fact]
    public async Task ClientCancellationPropagatesToTheDurableOperationOwner()
    {
        var recorder = new CapturingRecorder();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Coordinator(new CancellingSource(), new MemoryRuleStore(), recorder: recorder)
                .CheckForUpdateAsync(cancellation.Token));

        Assert.Null(recorder.Exception);
    }

    [Fact]
    public async Task PrimaryDatabaseDiagnosticSinkReceivesStructuredRedactedEvidence()
    {
        var database = new CapturingDiagnosticStore();
        var file = new CapturingFileSink();
        var recorder = new ManagedDiagnosticRecorder(
            database,
            file,
            Options.Create(new ManagedDiagnosticsOptions()));
        var exception = new InvalidOperationException(
            "Password=super-secret failed",
            new IOException("Bearer abc.def.ghi inner failure"));

        var receipt = await recorder.RecordFailureAsync(
            DiagnosticContext(),
            exception,
            CancellationToken.None);

        Assert.True(receipt.DatabasePersisted);
        Assert.False(receipt.FallbackFilePersisted);
        Assert.NotNull(database.Record);
        Assert.Equal("Stage", database.Record.Operation);
        Assert.Contains("[REDACTED]", database.Record.SanitizedExceptionMessage);
        Assert.DoesNotContain("super-secret", JsonSerializer.Serialize(database.Record));
        Assert.DoesNotContain("abc.def.ghi", JsonSerializer.Serialize(database.Record));
    }

    [Fact]
    public async Task DatabaseDiagnosticFailureFallsBackToPhysicalFile()
    {
        var root = Path.Combine(Path.GetTempPath(), $"ec-diagnostics-{Guid.NewGuid():N}");
        var path = Path.Combine(root, "failure.jsonl");
        try
        {
            var recorder = new ManagedDiagnosticRecorder(
                new ThrowingDiagnosticStore(),
                new PhysicalManagedDiagnosticFileSink(Options.Create(new ManagedDiagnosticsOptions
                {
                    FallbackLogFile = path
                })),
                Options.Create(new ManagedDiagnosticsOptions { FallbackLogFile = path }));

            var receipt = await recorder.RecordFailureAsync(
                DiagnosticContext(),
                new InvalidOperationException("Token=private"),
                CancellationToken.None);

            Assert.True(receipt.FallbackFilePersisted);
            Assert.True(File.Exists(path));
            var content = File.ReadAllText(path);
            Assert.Contains("CORRELATION-1", content);
            Assert.DoesNotContain("private", content);
        }
        finally
        {
            DeleteTree(root);
        }
    }

    [Fact]
    public async Task DiagnosticSubsystemFailureNeverMasksPublicationFailure()
    {
        var recorder = new ManagedDiagnosticRecorder(
            new ThrowingDiagnosticStore(),
            new ThrowingFileSink(),
            Options.Create(new ManagedDiagnosticsOptions()));

        var result = await Coordinator(
            new StageFailureSource(RulePublicationStage.ResolveRef, new IOException("ref failed")),
            new MemoryRuleStore(),
            diagnostics: Options.Create(new ManagedDiagnosticsOptions()),
            recorder: recorder).CheckForUpdateAsync(CancellationToken.None);

        Assert.Equal("RULE_SOURCE_REF_RESOLUTION_FAILED", result.ErrorCode);
        Assert.Equal("Unavailable", result.DiagnosticsAvailability);
    }

    [Fact]
    public async Task NormalFailureIsSanitizedWhileVerboseFailureRetainsRedactedDetail()
    {
        var exception = new IOException("Password=top-secret cannot read manifest");
        var normal = await Coordinator(
            new StageFailureSource(RulePublicationStage.ReadManifest, exception),
            new MemoryRuleStore()).CheckForUpdateAsync(CancellationToken.None);
        var verbose = await Coordinator(
            new StageFailureSource(RulePublicationStage.ReadManifest, exception),
            new MemoryRuleStore(),
            diagnostics: Options.Create(new ManagedDiagnosticsOptions { VerboseErrors = true }))
            .CheckForUpdateAsync(CancellationToken.None);

        Assert.DoesNotContain("cannot read manifest", normal.FailureReason);
        Assert.Contains("cannot read manifest", verbose.FailureReason);
        Assert.Contains("[REDACTED]", verbose.FailureReason);
        Assert.DoesNotContain("top-secret", verbose.FailureReason);
    }

    [Fact]
    public async Task RetryResumesPublishedCandidateWithoutDuplicateStaging()
    {
        var store = new MemoryRuleStore { FailureStage = RulePublicationStage.Activate };
        var source = new StaticSource(HealthySnapshot());
        var first = await Coordinator(source, store).CheckForUpdateAsync(CancellationToken.None);
        store.FailureStage = null;
        var second = await Coordinator(source, store).CheckForUpdateAsync(CancellationToken.None);

        Assert.Equal("RULE_ACTIVATION_FAILED", first.ErrorCode);
        Assert.Equal("Activated", second.Status);
        Assert.Equal(1, store.StageCalls);
        Assert.Single(store.Releases);
    }

    [Fact]
    public async Task CancellationAfterDurableStagingCanResumeWithoutDuplication()
    {
        var store = new MemoryRuleStore { CancelAfterStage = true };
        var source = new StaticSource(HealthySnapshot());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Coordinator(source, store).CheckForUpdateAsync(CancellationToken.None));
        store.CancelAfterStage = false;
        var second = await Coordinator(source, store).CheckForUpdateAsync(CancellationToken.None);

        Assert.Equal("Activated", second.Status);
        Assert.Equal(1, store.StageCalls);
        Assert.Single(store.Releases);
    }

    [Fact]
    public void DiagnosticMigrationAndDefaultFallbackArePackaged()
    {
        var migration = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory,
            "Schema",
            "005_managed_operation_diagnostics.template.sql"));
        var fallback = PhysicalManagedDiagnosticFileSink.ResolvePath(string.Empty);

        Assert.Contains("managed_operation_diagnostics", migration);
        Assert.Contains("correlation_id", migration);
        Assert.Contains("sanitized_stack_trace", migration);
        Assert.Contains("EternalCycle", fallback);
        Assert.EndsWith("managed-diagnostics.jsonl", fallback);
    }

    private static ManagedDiagnosticContext DiagnosticContext() =>
        new(
            "CORRELATION-1",
            "Stage",
            RulePublicationStage.Stage,
            "RULE_STORE_STAGE_FAILED",
            DateTimeOffset.UtcNow,
            "eternal-cycle-core",
            "release-1",
            CommitA,
            "campaign");

    private static ManagedRulePublicationCoordinator Coordinator(
        IRuleSourceProvider source,
        IPublishedRuleStore store,
        IOptions<ManagedDiagnosticsOptions>? diagnostics = null,
        IManagedDiagnosticRecorder? recorder = null) =>
        new(
            source,
            store,
            Options.Create(new ManagedRuleServiceOptions()),
            diagnostics ?? Options.Create(new ManagedDiagnosticsOptions()),
            recorder ?? NullManagedDiagnosticRecorder.Instance,
            NullLogger<ManagedRulePublicationCoordinator>.Instance);

    private static RuleSourceSnapshot HealthySnapshot() =>
        new(
            "Git",
            CommitA,
            "1.0.0+regression",
            [
                Document("kernel", RuleLayer.RuntimeKernel, alwaysInclude: true),
                Document("core", RuleLayer.Core, dependencies: ["kernel"])
            ],
            DateTimeOffset.UtcNow);

    private static PublishedRuleRelease Release(string id, string sourceIdentity, RuleReleaseState state)
    {
        var snapshot = HealthySnapshot();
        return new PublishedRuleRelease(
            id,
            "eternal-cycle-core",
            snapshot.ProviderKind,
            sourceIdentity,
            snapshot.RepositoryVersion,
            "1",
            state,
            RuleCompiler.Compile(snapshot.RepositoryVersion, snapshot.Documents),
            DateTimeOffset.UtcNow);
    }

    private static RuleSourceDocument Document(
        string id,
        RuleLayer layer,
        bool alwaysInclude = false,
        IReadOnlyList<string>? dependencies = null) =>
        new(
            id,
            $"docs/rules/{id}.md",
            $"# {id}\n\nCanonical rule content for {id}.",
            new RuleSourceMetadata(
                layer,
                [],
                [],
                ["NORMAL"],
                ["gameplay.resolve"],
                ["continuity"],
                0,
                alwaysInclude,
                dependencies ?? []));

    private static string RunGit(string root, params string[] arguments)
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
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(error);
        }

        return output;
    }

    private static void DeleteTree(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(path, recursive: true);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "DISTRIBUTION.json")) &&
                File.Exists(Path.Combine(current.FullName, "docs", "rules", "rule-source-manifest.json")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate the Eternal Cycle repository root for the official-manifest regression.");
    }

    private sealed class StaticSource(RuleSourceSnapshot snapshot) : IRuleSourceProvider
    {
        public Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken) =>
            Task.FromResult(snapshot);
    }

    private sealed class StageFailureSource(RulePublicationStage stage, Exception exception) : IRuleSourceProvider
    {
        public Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken) =>
            Task.FromException<RuleSourceSnapshot>(exception);

        public Task<RuleSourceSnapshot> GetSnapshotAsync(
            Action<RulePublicationStage>? onStage,
            CancellationToken cancellationToken)
        {
            onStage?.Invoke(stage);
            return Task.FromException<RuleSourceSnapshot>(exception);
        }
    }

    private sealed class CancellingSource : IRuleSourceProvider
    {
        public Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken) =>
            GetSnapshotAsync(null, cancellationToken);

        public async Task<RuleSourceSnapshot> GetSnapshotAsync(
            Action<RulePublicationStage>? onStage,
            CancellationToken cancellationToken)
        {
            onStage?.Invoke(RulePublicationStage.AcquireSource);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new UnreachableException();
        }
    }

    private sealed class StaticSourceConfigurationStore(
        RuleSourceConfiguration configuration) : IRuleSourceConfigurationStore
    {
        public Task<RuleSourceConfiguration?> GetAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult<RuleSourceConfiguration?>(configuration);

        public Task<RuleSourceConfiguration> SaveAsync(
            RuleSourceConfiguration value,
            CancellationToken cancellationToken) =>
            Task.FromResult(value);
    }

    private sealed class MemoryRuleStore : IPublishedRuleStore
    {
        private readonly Dictionary<string, PublishedRuleRelease> releases = new(StringComparer.Ordinal);
        private string? activeReleaseId;

        public RulePublicationStage? FailureStage { get; set; }

        public bool CancelAfterStage { get; set; }

        public int StageCalls { get; private set; }

        public IReadOnlyCollection<PublishedRuleRelease> Releases => releases.Values;

        public PublishedRuleRelease? Active =>
            activeReleaseId is not null ? releases[activeReleaseId] : null;

        public void SeedActive(PublishedRuleRelease release)
        {
            releases.Add(release.RuleReleaseId, release);
            activeReleaseId = release.RuleReleaseId;
        }

        public Task<PublishedRuleRelease?> GetActiveAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult(activeReleaseId is not null ? releases[activeReleaseId] : null);

        public Task<PublishedRuleRelease?> FindBySourceAsync(
            string rulesetId,
            string sourceIdentity,
            CancellationToken cancellationToken) =>
            Task.FromResult(releases.Values.FirstOrDefault(value => value.SourceIdentity == sourceIdentity));

        public Task<PublishedRuleRelease?> GetByIdAsync(
            string rulesetId,
            string ruleReleaseId,
            CancellationToken cancellationToken) =>
            Task.FromResult(releases.GetValueOrDefault(ruleReleaseId));

        public Task<RuleUpdateCheck?> GetLatestUpdateCheckAsync(
            string rulesetId,
            CancellationToken cancellationToken) =>
            Task.FromResult<RuleUpdateCheck?>(null);

        public Task StageCandidateAsync(PublishedRuleRelease release, CancellationToken cancellationToken)
        {
            if (FailureStage == RulePublicationStage.Stage)
            {
                throw new IOException("SQL candidate staging failed");
            }

            StageCalls++;
            releases.Add(release.RuleReleaseId, release);
            if (CancelAfterStage)
            {
                throw new OperationCanceledException("Client disconnected after candidate commit.");
            }

            return Task.CompletedTask;
        }

        public Task SetStateAsync(
            string ruleReleaseId,
            RuleReleaseState state,
            string? failureReason,
            CancellationToken cancellationToken)
        {
            if (state == RuleReleaseState.Published && FailureStage == RulePublicationStage.Publish)
            {
                throw new IOException("SQL publication state write failed");
            }

            if (releases.TryGetValue(ruleReleaseId, out var release))
            {
                releases[ruleReleaseId] = release with { State = state, FailureReason = failureReason };
            }

            return Task.CompletedTask;
        }

        public Task ActivateAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken)
        {
            if (FailureStage == RulePublicationStage.Activate)
            {
                throw new IOException("SQL activation failed");
            }

            releases[ruleReleaseId] = releases[ruleReleaseId] with { State = RuleReleaseState.Active };
            activeReleaseId = ruleReleaseId;
            return Task.CompletedTask;
        }

        public Task RecordUpdateCheckAsync(RuleUpdateCheck updateCheck, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class CapturingRecorder : IManagedDiagnosticRecorder
    {
        public ManagedDiagnosticContext? Context { get; private set; }

        public Exception? Exception { get; private set; }

        public Task<ManagedDiagnosticReceipt> RecordFailureAsync(
            ManagedDiagnosticContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            Context = context;
            Exception = exception;
            return Task.FromResult(new ManagedDiagnosticReceipt(context.CorrelationId, "Database", true, false));
        }
    }

    private sealed class CapturingDiagnosticStore : IManagedDiagnosticStore
    {
        public ManagedOperationDiagnostic? Record { get; private set; }

        public Task WriteAsync(ManagedOperationDiagnostic diagnostic, CancellationToken cancellationToken)
        {
            Record = diagnostic;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingDiagnosticStore : IManagedDiagnosticStore
    {
        public Task WriteAsync(ManagedOperationDiagnostic diagnostic, CancellationToken cancellationToken) =>
            Task.FromException(new IOException("database diagnostic sink unavailable"));
    }

    private sealed class CapturingFileSink : IManagedDiagnosticFileSink
    {
        public Task WriteAsync(ManagedOperationDiagnostic diagnostic, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class ThrowingFileSink : IManagedDiagnosticFileSink
    {
        public Task WriteAsync(ManagedOperationDiagnostic diagnostic, CancellationToken cancellationToken) =>
            Task.FromException(new IOException("file diagnostic sink unavailable"));
    }
}
