using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class ManagedRuleSourceCompatibilityTests
{
    [Fact]
    public async Task StableHistoricalSourceWithoutManifestIsExplicitlyIncompatible()
    {
        var root = CreateFixture(includeManifest: false);
        try
        {
            var configuration = Configuration(root, RuleSourceReleaseChannel.Stable, "HEAD");
            var provider = Provider(configuration);

            var error = await Assert.ThrowsAsync<RulePublicationException>(
                () => provider.GetSnapshotAsync(CancellationToken.None));

            Assert.Equal("RULE_SOURCE_INCOMPATIBLE", error.Code);
            Assert.False(error.RetrySafe);
            Assert.Equal(RuleSourceReleaseChannel.Stable, error.ReleaseChannel);
            Assert.Equal("HEAD", error.DiscoveryRef);
            Assert.Matches("^[A-F0-9]{40}$", error.SourceIdentity);
            Assert.Contains("manifest", error.SafeMessage, StringComparison.OrdinalIgnoreCase);

            var result = await Coordinator(provider, new MemoryRuleStore())
                .CheckForUpdateAsync(CancellationToken.None);
            Assert.Equal("RULE_SOURCE_INCOMPATIBLE", result.ErrorCode);
            Assert.Equal(nameof(RulePublicationStage.ReadManifest), result.Stage);
            Assert.False(result.RetrySafe);
            Assert.Equal(RuleSourceReleaseChannel.Stable, result.ReleaseChannel);
            Assert.Equal("HEAD", result.DiscoveryRef);
            Assert.Equal(error.SourceIdentity, result.SourceIdentity);
        }
        finally
        {
            DeleteTree(root);
        }
    }

    [Fact]
    public async Task PrereleaseDiscoveryRefPublishesImmutableShaAndMovingRefDoesNotRewriteProvenance()
    {
        var root = CreateFixture(includeManifest: true);
        try
        {
            var configuration = Configuration(
                root,
                RuleSourceReleaseChannel.Prerelease,
                "refs/heads/main");
            var provider = Provider(configuration, RuleUpdatePolicy.Manual);
            var firstSnapshot = await provider.GetSnapshotAsync(CancellationToken.None);
            var store = new MemoryRuleStore();
            var first = await Coordinator(firstSnapshot, store).CheckForUpdateAsync(CancellationToken.None);

            File.AppendAllText(Path.Combine(root, "docs", "rules", "kernel.md"), "\nChanged.\n");
            RunGit(root, "add", ".");
            RunGit(root, "commit", "-m", "move discovery ref");
            var secondSnapshot = await provider.GetSnapshotAsync(CancellationToken.None);
            _ = await Coordinator(secondSnapshot, store).CheckForUpdateAsync(CancellationToken.None);

            Assert.Equal(RuleSourceReleaseChannel.Prerelease, firstSnapshot.ReleaseChannel);
            Assert.Equal("refs/heads/main", firstSnapshot.DiscoveryRef);
            Assert.NotEqual(firstSnapshot.SourceIdentity, secondSnapshot.SourceIdentity);
            var firstRelease = Assert.Single(
                store.Releases,
                value => value.SourceIdentity == firstSnapshot.SourceIdentity);
            Assert.Equal(firstSnapshot.SourceIdentity, first.SourceIdentity);
            Assert.Equal(firstSnapshot.SourceIdentity, firstRelease.SourceIdentity);
            Assert.Equal("refs/heads/main", firstRelease.DiscoveryRef);
            Assert.Equal(RuleSourceReleaseChannel.Prerelease, firstRelease.ReleaseChannel);
        }
        finally
        {
            DeleteTree(root);
        }
    }

    [Fact]
    public async Task SupportedManifestContractIsAcceptedAndUnsupportedCompilerIsRejected()
    {
        var compatible = CreateFixture(includeManifest: true);
        var incompatible = CreateFixture(includeManifest: true, compilerContractVersion: "999");
        var unsupportedFormat = CreateFixture(includeManifest: true, manifestFormatVersion: 99);
        try
        {
            var snapshot = await Provider(Configuration(compatible)).GetSnapshotAsync(CancellationToken.None);
            var error = await Assert.ThrowsAsync<RulePublicationException>(
                () => Provider(Configuration(incompatible)).GetSnapshotAsync(CancellationToken.None));
            var formatError = await Assert.ThrowsAsync<RulePublicationException>(
                () => Provider(Configuration(unsupportedFormat)).GetSnapshotAsync(CancellationToken.None));

            Assert.Equal(1, snapshot.ManifestFormatVersion);
            Assert.Equal("1", snapshot.CompilerContractVersion);
            Assert.Equal("eternal-cycle-core", snapshot.RulesetId);
            Assert.Equal("RULE_SOURCE_INCOMPATIBLE", error.Code);
            Assert.False(error.RetrySafe);
            Assert.Contains("compiler", error.SafeMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("RULE_SOURCE_INCOMPATIBLE", formatError.Code);
            Assert.Contains("format", formatError.SafeMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteTree(compatible);
            DeleteTree(incompatible);
            DeleteTree(unsupportedFormat);
        }
    }

    [Fact]
    public async Task RulesetMismatchAndMissingDeclaredSourceAreIncompatible()
    {
        var wrongRuleset = CreateFixture(includeManifest: true, rulesetId: "another-ruleset");
        var missingSource = CreateFixture(
            includeManifest: true,
            declaredSourcePath: "docs/rules/missing.md");
        try
        {
            var rulesetError = await Assert.ThrowsAsync<RulePublicationException>(
                () => Provider(Configuration(wrongRuleset)).GetSnapshotAsync(CancellationToken.None));
            var sourceError = await Assert.ThrowsAsync<RulePublicationException>(
                () => Provider(Configuration(missingSource)).GetSnapshotAsync(CancellationToken.None));

            Assert.Equal("RULE_SOURCE_INCOMPATIBLE", rulesetError.Code);
            Assert.False(rulesetError.RetrySafe);
            Assert.Contains("RuleSet", rulesetError.SafeMessage, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("RULE_SOURCE_INCOMPATIBLE", sourceError.Code);
            Assert.False(sourceError.RetrySafe);
            Assert.Equal(RulePublicationStage.ReadRuleDocuments, sourceError.Stage);
            Assert.Contains("document", sourceError.SafeMessage, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            DeleteTree(wrongRuleset);
            DeleteTree(missingSource);
        }
    }

    [Fact]
    public async Task AcquisitionBudgetContainsIndividuallyBoundedGitProcesses()
    {
        var cache = Path.Combine(Path.GetTempPath(), $"ec-timeout-cache-{Guid.NewGuid():N}");
        Directory.CreateDirectory(cache);
        try
        {
            var runner = new RecordingRunner();
            var acquisition = TimeSpan.FromMinutes(9);
            var local = TimeSpan.FromSeconds(17);
            var configuration = new RuleSourceConfiguration(
                "eternal-cycle-core",
                "Git",
                "https://example.invalid/rules.git",
                "refs/heads/main",
                "docs/rules/manifest.json",
                true,
                1,
                DateTimeOffset.UtcNow,
                RuleSourceReleaseChannel.Prerelease);
            var provider = Provider(configuration, RuleUpdatePolicy.Manual, cache, runner, acquisition, local);

            _ = await provider.GetSnapshotAsync(CancellationToken.None);

            Assert.Contains(runner.Calls, call => call.Command == "init" && call.Timeout == local);
            Assert.Contains(runner.Calls, call => call.Command == "fetch" && call.Timeout == local);
            Assert.Contains(runner.Calls, call => call.Command == "rev-parse" && call.Timeout == local);
            Assert.All(runner.Calls, call => Assert.Equal(local, call.Timeout));
        }
        finally
        {
            DeleteTree(cache);
        }
    }

    [Fact]
    public async Task FetchUsesPerProcessTimeoutWithinOverallAcquisitionBudget()
    {
        var cache = Path.Combine(Path.GetTempPath(), $"ec-fetch-cache-{Guid.NewGuid():N}");
        Directory.CreateDirectory(cache);
        var location = "https://example.invalid/rules.git";
        var repositoryRoot = Path.Combine(cache, CacheKey(location));
        Directory.CreateDirectory(Path.Combine(repositoryRoot, ".git"));
        try
        {
            var runner = new RecordingRunner();
            var acquisition = TimeSpan.FromMinutes(8);
            var configuration = new RuleSourceConfiguration(
                "eternal-cycle-core",
                "Git",
                location,
                "refs/heads/main",
                "docs/rules/manifest.json",
                true,
                1,
                DateTimeOffset.UtcNow,
                RuleSourceReleaseChannel.Prerelease);
            var provider = Provider(
                configuration,
                RuleUpdatePolicy.Manual,
                cache,
                runner,
                acquisition,
                TimeSpan.FromSeconds(20));

            _ = await provider.GetSnapshotAsync(CancellationToken.None);

            Assert.Contains(runner.Calls, call =>
                call.Command == "fetch" && call.Timeout == TimeSpan.FromSeconds(20));
        }
        finally
        {
            DeleteTree(cache);
        }
    }

    [Fact]
    public async Task TimedOutProcessReturnsWithinBoundedCleanupPeriod()
    {
        var runner = new SystemGitProcessRunner();
        var (executable, arguments) = SleepCommand(1000);
        var stopwatch = Stopwatch.StartNew();

        await Assert.ThrowsAsync<GitProcessTimeoutException>(() => runner.RunAsync(
            executable,
            Path.GetTempPath(),
            arguments,
            TimeSpan.FromMilliseconds(75),
            TimeSpan.FromMilliseconds(200),
            CancellationToken.None));

        stopwatch.Stop();
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(3), $"Cleanup took {stopwatch.Elapsed}.");
    }

    [Fact]
    public async Task CleanupFailureDoesNotMaskOriginalTimeout()
    {
        var runner = new SystemGitProcessRunner(_ => throw new InvalidOperationException("kill failed"));
        var (executable, arguments) = SleepCommand(350);

        var error = await Assert.ThrowsAsync<GitProcessTimeoutException>(() => runner.RunAsync(
            executable,
            Path.GetTempPath(),
            arguments,
            TimeSpan.FromMilliseconds(40),
            TimeSpan.FromMilliseconds(40),
            CancellationToken.None));

        Assert.Contains("timeout", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OverallAcquisitionTimeoutIsDistinctFromGitProcessTimeout()
    {
        var cache = Path.Combine(Path.GetTempPath(), $"ec-acquisition-timeout-{Guid.NewGuid():N}");
        Directory.CreateDirectory(cache);
        try
        {
            var provider = Provider(
                RemoteConfiguration("refs/heads/main"),
                RuleUpdatePolicy.Manual,
                cache,
                new BlockingRunner(),
                TimeSpan.FromMilliseconds(40),
                TimeSpan.FromSeconds(30));

            var error = await Assert.ThrowsAsync<RulePublicationException>(() =>
                provider.GetSnapshotAsync(CancellationToken.None));

            Assert.Equal("RULE_SOURCE_ACQUISITION_TIMEOUT", error.Code);
            Assert.True(error.RetrySafe);
        }
        finally
        {
            DeleteTree(cache);
        }
    }

    [Fact]
    public async Task IndividualGitTimeoutHasItsOwnStructuredFailure()
    {
        var cache = Path.Combine(Path.GetTempPath(), $"ec-process-timeout-{Guid.NewGuid():N}");
        Directory.CreateDirectory(cache);
        try
        {
            var provider = Provider(
                RemoteConfiguration("refs/heads/main"),
                RuleUpdatePolicy.Manual,
                cache,
                new ProcessTimeoutRunner());

            var error = await Assert.ThrowsAsync<RulePublicationException>(() =>
                provider.GetSnapshotAsync(CancellationToken.None));

            Assert.Equal("RULE_SOURCE_PROCESS_TIMEOUT", error.Code);
            Assert.True(error.RetrySafe);
        }
        finally
        {
            DeleteTree(cache);
        }
    }

    [Fact]
    public async Task MissingRemoteRefFailsSpecificallyWithoutWaitingForTimeout()
    {
        var cache = Path.Combine(Path.GetTempPath(), $"ec-missing-ref-{Guid.NewGuid():N}");
        Directory.CreateDirectory(cache);
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var provider = Provider(
                RemoteConfiguration("refs/heads/does-not-exist"),
                RuleUpdatePolicy.Manual,
                cache,
                new MissingRefRunner(),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(30));

            var error = await Assert.ThrowsAsync<RulePublicationException>(() =>
                provider.GetSnapshotAsync(CancellationToken.None));

            stopwatch.Stop();
            Assert.Equal("RULE_SOURCE_REF_NOT_FOUND", error.Code);
            Assert.False(error.RetrySafe);
            Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2));
        }
        finally
        {
            DeleteTree(cache);
        }
    }

    [Fact]
    public async Task GitObservationsUseTrueOperationCorrelationAndSafeCommandCategories()
    {
        var cache = Path.Combine(Path.GetTempPath(), $"ec-git-observation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(cache);
        try
        {
            var recorder = new RecordingDiagnosticRecorder();
            var provider = Provider(
                RemoteConfiguration("refs/heads/main"),
                RuleUpdatePolicy.Manual,
                cache,
                new RecordingRunner(),
                diagnostics: recorder);
            var execution = new RulePublicationExecutionContext("OP-TRUE", "CORR-TRUE");

            _ = await provider.GetSnapshotAsync(execution, null, CancellationToken.None);

            Assert.NotEmpty(recorder.Events);
            Assert.All(recorder.Events, value =>
            {
                Assert.Equal("OP-TRUE", value.OperationId);
                Assert.Equal("CORR-TRUE", value.CorrelationId);
                Assert.Contains("CommandCategory=", value.SafeDetail);
                Assert.Contains("TimeoutScope=GitProcess", value.SafeDetail);
                Assert.DoesNotContain("example.invalid", value.SafeDetail, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("refs/heads/main", value.SafeDetail, StringComparison.Ordinal);
            });
        }
        finally
        {
            DeleteTree(cache);
        }
    }

    [Fact]
    public async Task UserSelectedLocalCloneWithCommittedRuleChangesUsesSameValidationPipeline()
    {
        var root = CreateFixture(includeManifest: true);
        try
        {
            File.AppendAllText(Path.Combine(root, "docs", "rules", "kernel.md"), "\nHouse rule.\n");
            RunGit(root, "add", ".");
            RunGit(root, "commit", "-m", "user-selected rule change");

            var snapshot = await Provider(Configuration(root))
                .GetSnapshotAsync(CancellationToken.None);

            Assert.Contains("House rule.", Assert.Single(snapshot.Documents).Content);
            Assert.Matches("^[A-F0-9]{40}$", snapshot.SourceIdentity);
            Assert.Equal("eternal-cycle-core", snapshot.RulesetId);
        }
        finally
        {
            DeleteTree(root);
        }
    }

    [Fact]
    public async Task PartialCacheIsReplacedAndExactImmutableRevisionIsReusedOffline()
    {
        var root = CreateFixture(includeManifest: true);
        var cache = Path.Combine(Path.GetTempPath(), $"ec-reuse-cache-{Guid.NewGuid():N}");
        Directory.CreateDirectory(cache);
        var sourceUri = new Uri(root + Path.DirectorySeparatorChar).AbsoluteUri;
        var cachePath = Path.Combine(cache, CacheKey(sourceUri));
        Directory.CreateDirectory(cachePath);
        File.WriteAllText(Path.Combine(cachePath, "partial.txt"), "incomplete");
        try
        {
            var commit = RunGit(root, "rev-parse", "HEAD").Trim();
            var configuration = new RuleSourceConfiguration(
                "eternal-cycle-core",
                "Git",
                sourceUri,
                commit,
                "docs/rules/manifest.json",
                false,
                1,
                DateTimeOffset.UtcNow,
                RuleSourceReleaseChannel.Prerelease);
            var provider = Provider(configuration, RuleUpdatePolicy.Manual, cache);

            var first = await provider.GetSnapshotAsync(CancellationToken.None);
            DeleteTree(root);
            var second = await provider.GetSnapshotAsync(CancellationToken.None);

            Assert.False(File.Exists(Path.Combine(cachePath, "partial.txt")));
            var payloadPath = Path.Combine(cachePath, "snapshots", commit.ToUpperInvariant());
            Assert.False(Directory.Exists(Path.Combine(cachePath, ".git")));
            Assert.True(File.Exists(Path.Combine(payloadPath, "docs", "rules", "manifest.json")));
            Assert.True(File.Exists(Path.Combine(payloadPath, "docs", "rules", "kernel.md")));
            Assert.True(File.Exists(Path.Combine(payloadPath, ".eternal-cycle-rule-source.json")));
            Assert.False(Directory.Exists(Path.Combine(payloadPath, "tests")));
            Assert.Equal(first.SourceIdentity, second.SourceIdentity);
        }
        finally
        {
            DeleteTree(root);
            DeleteTree(cache);
        }
    }

    [Fact]
    public void ReadinessPreservesCurrentClassificationAndLatestCause()
    {
        var cause = new ManagedCausalDiagnostic(
            "RULE_SOURCE_OPERATION_TIMEOUT",
            "AcquireSource",
            "OP-123",
            true,
            false,
            "Remote acquisition timed out.",
            RuleSourceReleaseChannel.Prerelease,
            "refs/heads/main",
            null,
            DateTimeOffset.UnixEpoch);
        var report = ManagedReadinessEvaluator.Evaluate(
            new ManagedInfrastructureSnapshot(
                ManagedComponentStatus.Ready,
                ManagedComponentStatus.Ready,
                ManagedComponentStatus.Ready,
                ManagedComponentStatus.Unavailable,
                0,
                null,
                null,
                false,
                ManagedComponentStatus.NotRequested,
                "Failed",
                false,
                LatestRelevantFailure: cause),
            campaignRequested: false);

        Assert.Equal("RULE_SOURCE_UNAVAILABLE", report.ErrorCode);
        Assert.Equal("RULE_SOURCE_OPERATION_TIMEOUT", report.LatestRelevantFailure?.ErrorCode);
        Assert.Equal("AcquireSource", report.LatestRelevantFailure?.Stage);
        Assert.Equal("OP-123", report.LatestRelevantFailure?.CorrelationId);
    }

    private static GitRuleSourceProvider Provider(
        RuleSourceConfiguration configuration,
        RuleUpdatePolicy updatePolicy = RuleUpdatePolicy.Disabled,
        string? cache = null,
        IGitProcessRunner? runner = null,
        TimeSpan? acquisitionTimeout = null,
        TimeSpan? processTimeout = null,
        IManagedDiagnosticRecorder? diagnostics = null)
    {
        var options = Options.Create(new ManagedRuleServiceOptions
        {
            UpdatePolicy = updatePolicy,
            GitSource = new GitRuleSourceOptions
            {
                AcquisitionTimeout = acquisitionTimeout ?? TimeSpan.FromMinutes(10),
                ProcessTimeout = processTimeout ?? TimeSpan.FromMinutes(2)
            }
        });
        var administration = Options.Create(new ManagedAdministrationOptions
        {
            ManagedRuleCacheDirectory = cache ?? string.Empty
        });
        return runner is null
            ? new GitRuleSourceProvider(options, new StaticConfigurationStore(configuration), administration)
            : new GitRuleSourceProvider(
                options,
                new StaticConfigurationStore(configuration),
                administration,
                diagnostics ?? NullManagedDiagnosticRecorder.Instance,
                runner);
    }

    private static RuleSourceConfiguration RemoteConfiguration(string requestedRef) =>
        new(
            "eternal-cycle-core",
            "Git",
            "https://example.invalid/rules.git",
            requestedRef,
            "docs/rules/manifest.json",
            false,
            1,
            DateTimeOffset.UtcNow,
            RuleSourceReleaseChannel.Stable);

    private static ManagedRulePublicationCoordinator Coordinator(
        RuleSourceSnapshot snapshot,
        IPublishedRuleStore store) =>
        Coordinator(new StaticSource(snapshot), store);

    private static ManagedRulePublicationCoordinator Coordinator(
        IRuleSourceProvider source,
        IPublishedRuleStore store) =>
        new(
            source,
            store,
            Options.Create(new ManagedRuleServiceOptions()),
            NullLogger<ManagedRulePublicationCoordinator>.Instance);

    private static RuleSourceConfiguration Configuration(
        string root,
        RuleSourceReleaseChannel channel = RuleSourceReleaseChannel.Stable,
        string requestedRef = "HEAD") =>
        new(
            "eternal-cycle-core",
            "Git",
            root,
            requestedRef,
            "docs/rules/manifest.json",
            false,
            1,
            DateTimeOffset.UtcNow,
            channel);

    private static string CreateFixture(
        bool includeManifest,
        string compilerContractVersion = "1",
        int manifestFormatVersion = 1,
        string rulesetId = "eternal-cycle-core",
        string declaredSourcePath = "docs/rules/kernel.md")
    {
        var root = Path.Combine(Path.GetTempPath(), $"ec-rule-source-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(root, "docs", "rules"));
        File.WriteAllText(Path.Combine(root, "docs", "rules", "kernel.md"), "# Kernel\n\nRequired.");
        if (includeManifest)
        {
            File.WriteAllText(Path.Combine(root, "docs", "rules", "manifest.json"), JsonSerializer.Serialize(new
            {
                manifestFormatVersion,
                compilerContractVersion,
                rulesetId,
                repositoryVersion = "1.0.0+test",
                sources = new[]
                {
                    new
                    {
                        ruleSourceId = "kernel",
                        path = declaredSourcePath,
                        layer = "RuntimeKernel",
                        worldModelIds = Array.Empty<string>(),
                        moduleIds = Array.Empty<string>(),
                        campaignModes = new[] { "*" },
                        operations = new[] { "*" },
                        topics = Array.Empty<string>(),
                        dependencies = Array.Empty<string>(),
                        priority = 100,
                        alwaysInclude = true
                    }
                }
            }));
        }

        RunGit(root, "init");
        RunGit(root, "config", "user.email", "fixture@example.invalid");
        RunGit(root, "config", "user.name", "Fixture");
        RunGit(root, "add", ".");
        RunGit(root, "commit", "-m", "fixture");
        RunGit(root, "branch", "-M", "main");
        return root;
    }

    private static string CacheKey(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)))[..24];

    private static (string Executable, IReadOnlyList<string> Arguments) SleepCommand(int milliseconds) =>
        OperatingSystem.IsWindows()
            ? ("powershell.exe", ["-NoProfile", "-Command", $"Start-Sleep -Milliseconds {milliseconds}"])
            : ("/bin/sh", ["-c", $"sleep {Math.Max(1, milliseconds / 1000.0):0.###}"]);

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

    private sealed class StaticConfigurationStore(RuleSourceConfiguration configuration)
        : IRuleSourceConfigurationStore
    {
        public Task<RuleSourceConfiguration?> GetAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult<RuleSourceConfiguration?>(configuration);

        public Task<RuleSourceConfiguration> SaveAsync(
            RuleSourceConfiguration value,
            CancellationToken cancellationToken) =>
            Task.FromResult(value);
    }

    private sealed class StaticSource(RuleSourceSnapshot snapshot) : IRuleSourceProvider
    {
        public Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken) =>
            Task.FromResult(snapshot);
    }

    private sealed class RecordingRunner : IGitProcessRunner
    {
        private const string Sha = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        public List<(string Command, TimeSpan Timeout)> Calls { get; } = [];

        public Task<GitProcessResult> RunAsync(
            string executable,
            string workingDirectory,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            TimeSpan terminationGracePeriod,
            CancellationToken cancellationToken)
        {
            Calls.Add((arguments[0], timeout));
            if (arguments[0] == "clone")
            {
                Directory.CreateDirectory(Path.Combine(arguments[^1], ".git"));
            }

            if (arguments[0] == "rev-parse")
            {
                return Task.FromResult(new GitProcessResult(0, Sha, string.Empty));
            }

            if (arguments[0] == "rev-list")
            {
                return Task.FromResult(new GitProcessResult(0, "47", string.Empty));
            }

            if (arguments[0] == "show" && arguments[1].EndsWith(":docs/rules/manifest.json", StringComparison.Ordinal))
            {
                return Task.FromResult(new GitProcessResult(0, """
                    {
                      "manifestFormatVersion": 1,
                      "compilerContractVersion": "1",
                      "rulesetId": "eternal-cycle-core",
                      "repositoryVersion": "1.0.0+test",
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
                        "priority": 100,
                        "alwaysInclude": true
                      }]
                    }
                    """, string.Empty));
            }

            if (arguments[0] == "show")
            {
                return Task.FromResult(new GitProcessResult(0, "# Kernel\n\nRequired.", string.Empty));
            }

            return Task.FromResult(new GitProcessResult(0, string.Empty, string.Empty));
        }
    }

    private sealed class BlockingRunner : IGitProcessRunner
    {
        public async Task<GitProcessResult> RunAsync(
            string executable,
            string workingDirectory,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            TimeSpan terminationGracePeriod,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Unreachable.");
        }
    }

    private sealed class ProcessTimeoutRunner : IGitProcessRunner
    {
        public Task<GitProcessResult> RunAsync(
            string executable,
            string workingDirectory,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            TimeSpan terminationGracePeriod,
            CancellationToken cancellationToken) =>
            throw new GitProcessTimeoutException("fixture process timeout");
    }

    private sealed class MissingRefRunner : IGitProcessRunner
    {
        public Task<GitProcessResult> RunAsync(
            string executable,
            string workingDirectory,
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            TimeSpan terminationGracePeriod,
            CancellationToken cancellationToken) =>
            Task.FromResult(arguments[0] == "fetch"
                ? new GitProcessResult(128, string.Empty, "fatal: couldn't find remote ref refs/heads/does-not-exist")
                : new GitProcessResult(0, string.Empty, string.Empty));
    }

    private sealed class RecordingDiagnosticRecorder : IManagedDiagnosticRecorder
    {
        public List<ManagedDiagnosticContext> Events { get; } = [];

        public Task<ManagedDiagnosticReceipt> RecordFailureAsync(
            ManagedDiagnosticContext context,
            Exception exception,
            CancellationToken cancellationToken) =>
            RecordEventAsync(context, cancellationToken);

        public Task<ManagedDiagnosticReceipt> RecordEventAsync(
            ManagedDiagnosticContext context,
            CancellationToken cancellationToken)
        {
            Events.Add(context);
            return Task.FromResult(new ManagedDiagnosticReceipt(
                context.CorrelationId,
                "Captured",
                true,
                false));
        }
    }

    private sealed class MemoryRuleStore : IPublishedRuleStore
    {
        private readonly Dictionary<string, PublishedRuleRelease> releases = new(StringComparer.Ordinal);
        private string? active;

        public IReadOnlyCollection<PublishedRuleRelease> Releases => releases.Values;

        public Task<PublishedRuleRelease?> GetActiveAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult(active is null ? null : releases[active]);

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

        public Task<RuleUpdateCheck?> GetLatestUpdateCheckAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult<RuleUpdateCheck?>(null);

        public Task StageCandidateAsync(PublishedRuleRelease release, CancellationToken cancellationToken)
        {
            releases[release.RuleReleaseId] = release;
            return Task.CompletedTask;
        }

        public Task SetStateAsync(
            string ruleReleaseId,
            RuleReleaseState state,
            string? failureReason,
            CancellationToken cancellationToken)
        {
            releases[ruleReleaseId] = releases[ruleReleaseId] with { State = state, FailureReason = failureReason };
            return Task.CompletedTask;
        }

        public Task ActivateAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken)
        {
            if (active is not null && active != ruleReleaseId)
            {
                releases[active] = releases[active] with { State = RuleReleaseState.Published };
            }

            releases[ruleReleaseId] = releases[ruleReleaseId] with { State = RuleReleaseState.Active };
            active = ruleReleaseId;
            return Task.CompletedTask;
        }

        public Task RecordUpdateCheckAsync(RuleUpdateCheck updateCheck, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
