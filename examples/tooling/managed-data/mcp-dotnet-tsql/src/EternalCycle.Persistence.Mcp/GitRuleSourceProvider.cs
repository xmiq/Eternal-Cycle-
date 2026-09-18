using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public sealed class GitRuleSourceOptions
{
    public string RepositoryRoot { get; init; } = string.Empty;
    public string Ref { get; init; } = "HEAD";
    public string ManifestPath { get; init; } = "docs/rules/rule-source-manifest.json";
    public bool FetchBeforeCheck { get; init; }
    public string Remote { get; init; } = "origin";
    public string GitExecutable { get; init; } = "git";
    public TimeSpan AcquisitionTimeout { get; init; } = TimeSpan.FromMinutes(4);
    public TimeSpan ProcessTimeout { get; init; } = TimeSpan.FromMinutes(2);
    public TimeSpan TerminationGracePeriod { get; init; } = TimeSpan.FromSeconds(5);
}

internal sealed record GitProcessResult(int ExitCode, string StandardOutput, string StandardError);

internal sealed class GitProcessTimeoutException(string message, Exception? innerException = null)
    : TimeoutException(message, innerException);

internal interface IGitProcessRunner
{
    Task<GitProcessResult> RunAsync(
        string executable,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        TimeSpan terminationGracePeriod,
        CancellationToken cancellationToken);
}

internal sealed class SystemGitProcessRunner : IGitProcessRunner
{
    private readonly Action<Process> terminate;

    public SystemGitProcessRunner()
        : this(process =>
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        })
    {
    }

    internal SystemGitProcessRunner(Action<Process> terminate)
    {
        this.terminate = terminate;
    }

    public async Task<GitProcessResult> RunAsync(
        string executable,
        string workingDirectory,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        TimeSpan terminationGracePeriod,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.Environment["GIT_TERMINAL_PROMPT"] = "0";
        startInfo.Environment["GCM_INTERACTIVE"] = "Never";
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The configured Git executable could not be started.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        using var timeoutSource = new CancellationTokenSource(Positive(timeout, TimeSpan.FromMinutes(2)));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        try
        {
            await process.WaitForExitAsync(linked.Token);
        }
        catch (OperationCanceledException exception)
        {
            var callerCancelled = cancellationToken.IsCancellationRequested;
            await TerminateBoundedAsync(process, outputTask, errorTask, Positive(terminationGracePeriod, TimeSpan.FromSeconds(5)));
            if (callerCancelled)
            {
                throw new OperationCanceledException(
                    "The Git operation was cancelled by its parent operation.",
                    exception,
                    cancellationToken);
            }

            throw new GitProcessTimeoutException("The Git process exceeded its configured timeout.", exception);
        }

        var readers = Task.WhenAll(outputTask, errorTask);
        if (await Task.WhenAny(readers, Task.Delay(Positive(terminationGracePeriod, TimeSpan.FromSeconds(5)))) != readers)
        {
            await TerminateBoundedAsync(process, outputTask, errorTask, Positive(terminationGracePeriod, TimeSpan.FromSeconds(5)));
            throw new GitProcessTimeoutException("Git exited but redirected output did not close within the cleanup grace period.");
        }

        return new GitProcessResult(process.ExitCode, outputTask.Result, errorTask.Result);
    }

    private async Task TerminateBoundedAsync(
        Process process,
        Task<string> outputTask,
        Task<string> errorTask,
        TimeSpan gracePeriod)
    {
        try
        {
            terminate(process);
        }
        catch (Exception)
        {
            // Cleanup failure must not replace the original timeout or cancellation.
        }

        using var grace = new CancellationTokenSource(gracePeriod);
        try
        {
            await process.WaitForExitAsync(grace.Token);
        }
        catch (Exception)
        {
            // The original operation failure remains authoritative.
        }

        _ = await Task.WhenAny(Task.WhenAll(outputTask, errorTask), Task.Delay(gracePeriod));
    }

    private static TimeSpan Positive(TimeSpan value, TimeSpan fallback) =>
        value > TimeSpan.Zero ? value : fallback;
}

public sealed partial class GitRuleSourceProvider : IRuleSourceProvider
{
    private const int SupportedManifestFormatVersion = 1;
    private const string PayloadMetadataFile = ".eternal-cycle-rule-source.json";
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> CacheLocks = new(StringComparer.Ordinal);
    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ManagedRuleServiceOptions ruleSettings;
    private readonly GitRuleSourceOptions settings;
    private readonly IRuleSourceConfigurationStore? configurations;
    private readonly ManagedAdministrationOptions administration;
    private readonly IGitProcessRunner processRunner;
    private readonly IManagedDiagnosticRecorder diagnostics;

    public GitRuleSourceProvider(IOptions<ManagedRuleServiceOptions> options)
        : this(options, null, Options.Create(new ManagedAdministrationOptions()), NullManagedDiagnosticRecorder.Instance, new SystemGitProcessRunner())
    {
    }

    public GitRuleSourceProvider(
        IOptions<ManagedRuleServiceOptions> options,
        IRuleSourceConfigurationStore? configurations,
        IOptions<ManagedAdministrationOptions> administrationOptions)
        : this(options, configurations, administrationOptions, NullManagedDiagnosticRecorder.Instance, new SystemGitProcessRunner())
    {
    }

    public GitRuleSourceProvider(
        IOptions<ManagedRuleServiceOptions> options,
        IRuleSourceConfigurationStore? configurations,
        IOptions<ManagedAdministrationOptions> administrationOptions,
        IManagedDiagnosticRecorder diagnostics)
        : this(options, configurations, administrationOptions, diagnostics, new SystemGitProcessRunner())
    {
    }

    internal GitRuleSourceProvider(
        IOptions<ManagedRuleServiceOptions> options,
        IRuleSourceConfigurationStore? configurations,
        IOptions<ManagedAdministrationOptions> administrationOptions,
        IGitProcessRunner processRunner)
        : this(options, configurations, administrationOptions, NullManagedDiagnosticRecorder.Instance, processRunner)
    {
    }

    internal GitRuleSourceProvider(
        IOptions<ManagedRuleServiceOptions> options,
        IRuleSourceConfigurationStore? configurations,
        IOptions<ManagedAdministrationOptions> administrationOptions,
        IManagedDiagnosticRecorder diagnostics,
        IGitProcessRunner processRunner)
    {
        ruleSettings = options.Value;
        settings = ruleSettings.GitSource;
        this.configurations = configurations;
        administration = administrationOptions.Value;
        this.diagnostics = diagnostics;
        this.processRunner = processRunner;
    }

    public Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken) =>
        GetSnapshotAsync(RulePublicationExecutionContext.CreateStandalone(), null, cancellationToken);

    public Task<RuleSourceSnapshot> GetSnapshotAsync(
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken) =>
        GetSnapshotAsync(RulePublicationExecutionContext.CreateStandalone(), onStage, cancellationToken);

    public async Task<RuleSourceSnapshot> GetSnapshotAsync(
        RulePublicationExecutionContext execution,
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.StartNew();
        using var acquisitionTimeout = new CancellationTokenSource(Positive(settings.AcquisitionTimeout, TimeSpan.FromMinutes(4)));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, acquisitionTimeout.Token);
        try
        {
            return await GetSnapshotCoreAsync(execution, onStage, linked.Token);
        }
        catch (OperationCanceledException exception)
            when (acquisitionTimeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            await RecordObservationAsync(
                execution,
                RulePublicationStage.AcquireSource,
                "Acquire",
                started.Elapsed,
                "OverallAcquisition",
                "Timeout",
                "OverallAcquisitionTimeout",
                "RULE_SOURCE_ACQUISITION_TIMEOUT");
            throw new RulePublicationException(
                "RULE_SOURCE_ACQUISITION_TIMEOUT",
                RulePublicationStage.AcquireSource,
                "Rule Source acquisition exceeded its configured overall timeout.",
                retrySafe: true,
                administrativeInterventionRequired: false,
                exception);
        }
    }

    private async Task<RuleSourceSnapshot> GetSnapshotCoreAsync(
        RulePublicationExecutionContext execution,
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken)
    {
        onStage?.Invoke(RulePublicationStage.AcquireSource);
        var persisted = configurations is null
            ? null
            : await configurations.GetAsync(ruleSettings.RulesetId, cancellationToken);
        var legacyLocal = !string.IsNullOrWhiteSpace(settings.RepositoryRoot);
        if (!legacyLocal && persisted is null)
        {
            throw new ManagedServiceException(
                "RULE_SOURCE_NOT_CONFIGURED",
                "No Rule Source is configured. Select the official default or another compatible source.");
        }

        var configuration = legacyLocal
            ? new RuleSourceConfiguration(
                ruleSettings.RulesetId,
                "Git",
                Path.GetFullPath(settings.RepositoryRoot),
                settings.Ref,
                settings.ManifestPath,
                false,
                0,
                DateTimeOffset.UtcNow,
                ruleSettings.ReleaseChannel)
            : persisted!;
        if (!string.Equals(configuration.ProviderKind, "Git", StringComparison.OrdinalIgnoreCase))
        {
            throw new ManagedServiceException(
                "RULE_SOURCE_PROVIDER_UNSUPPORTED",
                "The selected Rule Source Provider is not supported by this reference implementation.");
        }

        var requestedRef = RequireValue(configuration.RequestedRef, nameof(configuration.RequestedRef));
        var manifestPath = ValidateRepositoryPath(configuration.ManifestPath);
        var materialized = await MaterializePayloadAsync(
            configuration,
            requestedRef,
            manifestPath,
            execution,
            onStage,
            cancellationToken);
        var manifestJson = await File.ReadAllTextAsync(ResolvePayloadPath(materialized.PayloadRoot, manifestPath), cancellationToken);
        var manifest = ParseManifest(manifestJson, configuration.ReleaseChannel, requestedRef, materialized.SourceIdentity);
        ValidateManifestCompatibility(manifest, configuration.ReleaseChannel, requestedRef, materialized.SourceIdentity);

        onStage?.Invoke(RulePublicationStage.ReadRuleDocuments);
        var documents = new List<RuleSourceDocument>(manifest.Sources.Count);
        foreach (var source in manifest.Sources)
        {
            var sourcePath = ValidateRepositoryPath(source.Path);
            var contentPath = ResolvePayloadPath(materialized.PayloadRoot, sourcePath);
            if (!File.Exists(contentPath))
            {
                throw Incompatible(
                    "The materialized Rule Source payload is missing a required manifest document.",
                    configuration.ReleaseChannel,
                    requestedRef,
                    materialized.SourceIdentity,
                    stage: RulePublicationStage.ReadRuleDocuments);
            }

            documents.Add(new RuleSourceDocument(
                RequireValue(source.RuleSourceId, nameof(source.RuleSourceId)),
                sourcePath,
                await File.ReadAllTextAsync(contentPath, cancellationToken),
                new RuleSourceMetadata(
                    source.Layer,
                    source.WorldModelIds.ToArray(),
                    source.ModuleIds.ToArray(),
                    source.CampaignModes.ToArray(),
                    source.Operations.ToArray(),
                    source.Topics.ToArray(),
                    source.Priority,
                    source.AlwaysInclude,
                    source.Dependencies.ToArray(),
                    source.PreparationTier)));
        }

        return new RuleSourceSnapshot(
            "Git",
            materialized.SourceIdentity,
            RequireValue(manifest.RepositoryVersion, nameof(manifest.RepositoryVersion)),
            documents,
            DateTimeOffset.UtcNow,
            configuration.ReleaseChannel,
            requestedRef,
            manifest.ManifestFormatVersion,
            manifest.CompilerContractVersion,
            manifest.RulesetId,
            materialized.VersionMetadata);
    }

    private async Task<MaterializedGitPayload> MaterializePayloadAsync(
        RuleSourceConfiguration configuration,
        string requestedRef,
        string manifestPath,
        RulePublicationExecutionContext execution,
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken)
    {
        var cacheRoot = ResolveCacheRoot();
        var sourceKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(configuration.SourceLocation)))[..24];
        var sourceCache = Path.Combine(cacheRoot, sourceKey);
        Directory.CreateDirectory(sourceCache);
        var cacheLock = CacheLocks.GetOrAdd(sourceCache, _ => new SemaphoreSlim(1, 1));
        await cacheLock.WaitAsync(cancellationToken);
        try
        {
            CleanupLegacyCacheArtifacts(sourceCache);
            if (CommitSha().IsMatch(requestedRef))
            {
                var cachedIdentity = requestedRef.ToUpperInvariant();
                var cachedPayload = Path.Combine(sourceCache, "snapshots", cachedIdentity);
                if (File.Exists(Path.Combine(cachedPayload, PayloadMetadataFile)))
                {
                    return new MaterializedGitPayload(cachedPayload, cachedIdentity, null);
                }
            }

            var local = Directory.Exists(configuration.SourceLocation);
            if (local && !Directory.Exists(Path.Combine(configuration.SourceLocation, ".git")))
            {
                throw new RulePublicationException(
                    "RULE_SOURCE_UNAVAILABLE",
                    RulePublicationStage.AcquireSource,
                    "The selected local Git Rule Source is not a Git working copy.",
                    retrySafe: false,
                    administrativeInterventionRequired: true,
                    releaseChannel: configuration.ReleaseChannel,
                    discoveryRef: requestedRef);
            }

            string repositoryRoot;
            string? temporaryRoot = null;
            var managedRemote = false;
            if (local)
            {
                repositoryRoot = Path.GetFullPath(configuration.SourceLocation);
            }
            else
            {
                ValidateRemoteLocator(configuration.SourceLocation);
                temporaryRoot = Path.Combine(sourceCache, $".acquire-{Guid.NewGuid():N}");
                Directory.CreateDirectory(temporaryRoot);
                repositoryRoot = temporaryRoot;
                try
                {
                    await RunGitRequiredAsync(repositoryRoot, ["init", "--quiet"], RulePublicationStage.AcquireSource, "Inspect", execution, cancellationToken);
                    await RunGitRequiredAsync(repositoryRoot, ["remote", "add", "origin", configuration.SourceLocation], RulePublicationStage.AcquireSource, "Inspect", execution, cancellationToken);
                    var fetch = await RunGitResultAsync(repositoryRoot, ["fetch", "--no-tags", "origin", requestedRef], RulePublicationStage.AcquireSource, "Fetch", execution, cancellationToken);
                    if (fetch.ExitCode != 0)
                    {
                        var missingRef = IsMissingRef(fetch.StandardError);
                        throw GitFailure(
                            missingRef ? "RULE_SOURCE_REF_NOT_FOUND" : "RULE_SOURCE_ACQUISITION_FAILED",
                            RulePublicationStage.AcquireSource,
                            fetch,
                            missingRef
                                ? "The requested Git Rule Source ref does not exist or is not advertised by the selected source."
                                : "The selected Git Rule Source could not be fetched.",
                            administrativeInterventionRequired: false,
                            configuration.ReleaseChannel,
                            requestedRef);
                    }

                    managedRemote = true;
                }
                catch
                {
                    TryDeleteDirectory(temporaryRoot);
                    throw;
                }
            }

            try
            {
                onStage?.Invoke(RulePublicationStage.ResolveRef);
                var resolveTarget = managedRemote ? "FETCH_HEAD^{commit}" : $"{requestedRef}^{{commit}}";
                var resolve = await RunGitResultAsync(repositoryRoot, ["rev-parse", "--verify", resolveTarget], RulePublicationStage.ResolveRef, "ResolveRef", execution, cancellationToken);
                if (resolve.ExitCode != 0 || !CommitSha().IsMatch(resolve.StandardOutput.Trim()))
                {
                    throw GitFailure(
                        "RULE_SOURCE_REF_NOT_FOUND",
                        RulePublicationStage.ResolveRef,
                        resolve,
                        "The requested Git Rule Source ref was not found and could not be resolved to immutable provenance.",
                        administrativeInterventionRequired: false,
                        configuration.ReleaseChannel,
                        requestedRef);
                }

                var sourceIdentity = resolve.StandardOutput.Trim().ToUpperInvariant();
                var version = await ResolveVersionMetadataAsync(repositoryRoot, sourceIdentity, configuration, execution, cancellationToken);
                var payloadRoot = Path.Combine(sourceCache, "snapshots", sourceIdentity);
                if (!File.Exists(Path.Combine(payloadRoot, PayloadMetadataFile)))
                {
                    await CreatePayloadAsync(repositoryRoot, payloadRoot, sourceIdentity, manifestPath, configuration, execution, onStage, cancellationToken);
                }

                return new MaterializedGitPayload(payloadRoot, sourceIdentity, version);
            }
            finally
            {
                if (temporaryRoot is not null)
                {
                    TryDeleteDirectory(temporaryRoot);
                }
            }
        }
        finally
        {
            cacheLock.Release();
        }
    }

    private async Task CreatePayloadAsync(
        string repositoryRoot,
        string payloadRoot,
        string sourceIdentity,
        string manifestPath,
        RuleSourceConfiguration configuration,
        RulePublicationExecutionContext execution,
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken)
    {
        onStage?.Invoke(RulePublicationStage.ReadManifest);
        var manifestJson = await ReadGitObjectAsync(repositoryRoot, sourceIdentity, manifestPath, RulePublicationStage.ReadManifest, configuration, execution, cancellationToken);
        var manifest = ParseManifest(manifestJson, configuration.ReleaseChannel, configuration.RequestedRef, sourceIdentity);
        ValidateManifestCompatibility(manifest, configuration.ReleaseChannel, configuration.RequestedRef, sourceIdentity);

        var partial = $"{payloadRoot}.partial-{Guid.NewGuid():N}";
        try
        {
            WritePayloadFile(partial, manifestPath, manifestJson);
            onStage?.Invoke(RulePublicationStage.ReadRuleDocuments);
            foreach (var source in manifest.Sources)
            {
                var sourcePath = ValidateRepositoryPath(source.Path);
                var content = await ReadGitObjectAsync(repositoryRoot, sourceIdentity, sourcePath, RulePublicationStage.ReadRuleDocuments, configuration, execution, cancellationToken);
                WritePayloadFile(partial, sourcePath, content);
            }

            var metadata = JsonSerializer.Serialize(new
            {
                provider = "Git",
                sourceIdentity,
                discoveryRef = configuration.RequestedRef,
                releaseChannel = configuration.ReleaseChannel.ToString(),
                manifestPath,
                rulesetId = manifest.RulesetId,
                manifestFormatVersion = manifest.ManifestFormatVersion,
                compilerContractVersion = manifest.CompilerContractVersion
            });
            await File.WriteAllTextAsync(Path.Combine(partial, PayloadMetadataFile), metadata, cancellationToken);
            Directory.CreateDirectory(Path.GetDirectoryName(payloadRoot)!);
            if (Directory.Exists(payloadRoot))
            {
                TryDeleteDirectory(payloadRoot);
            }

            Directory.Move(partial, payloadRoot);
        }
        catch
        {
            TryDeleteDirectory(partial);
            throw;
        }
    }

    private async Task<string> ReadGitObjectAsync(
        string repositoryRoot,
        string sourceIdentity,
        string repositoryPath,
        RulePublicationStage stage,
        RuleSourceConfiguration configuration,
        RulePublicationExecutionContext execution,
        CancellationToken cancellationToken)
    {
        var result = await RunGitResultAsync(repositoryRoot, ["show", $"{sourceIdentity}:{repositoryPath}"], stage, "Materialize", execution, cancellationToken);
        if (result.ExitCode == 0)
        {
            return result.StandardOutput;
        }

        throw Incompatible(
            stage == RulePublicationStage.ReadManifest
                ? "The selected immutable Rule Source does not contain the required manifest."
                : "The selected Rule Source manifest references a required document that is absent.",
            configuration.ReleaseChannel,
            configuration.RequestedRef,
            sourceIdentity,
            GitFailure(CodeFor(stage), stage, result, "A required Rule Source object could not be materialized.", true, configuration.ReleaseChannel, configuration.RequestedRef),
            stage);
    }

    private async Task<RuleSourceVersionMetadata?> ResolveVersionMetadataAsync(
        string repositoryRoot,
        string sourceIdentity,
        RuleSourceConfiguration configuration,
        RulePublicationExecutionContext execution,
        CancellationToken cancellationToken)
    {
        if (!configuration.IsOfficial || configuration.ReleaseChannel != RuleSourceReleaseChannel.Prerelease)
        {
            return null;
        }

        var metadata = OfficialDistributionMetadata.Load(administration);
        var baseRelease = metadata.PrereleaseBaseRelease ?? metadata.StableReleaseTag;
        var discoveryTag = metadata.PrereleaseDiscoveryTag ?? configuration.RequestedRef.Replace("refs/tags/", string.Empty, StringComparison.Ordinal);
        var targetVersion = metadata.PrereleaseTargetVersion;
        if (string.IsNullOrWhiteSpace(targetVersion) || string.IsNullOrWhiteSpace(baseRelease) || string.IsNullOrWhiteSpace(discoveryTag))
        {
            throw new RulePublicationException(
                "RULE_SOURCE_VERSION_METADATA_INVALID",
                RulePublicationStage.ResolveRef,
                "Official Prerelease version metadata is incomplete.",
                retrySafe: false,
                administrativeInterventionRequired: true,
                releaseChannel: configuration.ReleaseChannel,
                discoveryRef: configuration.RequestedRef,
                sourceIdentity: sourceIdentity);
        }

        var canonicalBase = baseRelease.StartsWith('v') ? baseRelease : $"v{baseRelease}";
        if (!Directory.Exists(configuration.SourceLocation))
        {
            var fetch = await RunGitResultAsync(repositoryRoot, ["fetch", "--no-tags", "origin", $"refs/tags/{canonicalBase}:refs/tags/{canonicalBase}"], RulePublicationStage.ResolveRef, "Fetch", execution, cancellationToken);
            if (fetch.ExitCode != 0)
            {
                throw GitFailure(
                    "RULE_SOURCE_VERSION_METADATA_INVALID",
                    RulePublicationStage.ResolveRef,
                    fetch,
                    "The immutable prerelease base tag could not be acquired for version provenance.",
                    true,
                    configuration.ReleaseChannel,
                    configuration.RequestedRef);
            }
        }

        var count = await RunGitResultAsync(repositoryRoot, ["rev-list", "--count", $"refs/tags/{canonicalBase}..{sourceIdentity}"], RulePublicationStage.ResolveRef, "Inspect", execution, cancellationToken);
        if (count.ExitCode != 0 || !int.TryParse(count.StandardOutput.Trim(), out var commitsSinceBase))
        {
            throw GitFailure(
                "RULE_SOURCE_VERSION_METADATA_INVALID",
                RulePublicationStage.ResolveRef,
                count,
                "The Prerelease commit distance from its immutable base release could not be determined.",
                false,
                configuration.ReleaseChannel,
                configuration.RequestedRef);
        }

        return PrereleaseVersioning.Derive(canonicalBase, discoveryTag, targetVersion, commitsSinceBase, sourceIdentity);
    }

    private async Task RunGitRequiredAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        RulePublicationStage stage,
        string category,
        RulePublicationExecutionContext execution,
        CancellationToken cancellationToken)
    {
        var result = await RunGitResultAsync(workingDirectory, arguments, stage, category, execution, cancellationToken);
        if (result.ExitCode != 0)
        {
            throw GitFailure(CodeFor(stage), stage, result, $"Git Rule Source operation {category} failed.", stage == RulePublicationStage.AcquireSource, null, null);
        }
    }

    private async Task<GitProcessResult> RunGitResultAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        RulePublicationStage stage,
        string category,
        RulePublicationExecutionContext execution,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.StartNew();
        try
        {
            var result = await processRunner.RunAsync(
                RequireValue(settings.GitExecutable, nameof(settings.GitExecutable)),
                workingDirectory,
                arguments,
                Positive(settings.ProcessTimeout, TimeSpan.FromMinutes(2)),
                settings.TerminationGracePeriod,
                cancellationToken);
            await RecordObservationAsync(
                execution,
                stage,
                category,
                started.Elapsed,
                "GitProcess",
                result.ExitCode == 0 ? "Succeeded" : "Exited",
                null,
                result.ExitCode == 0 ? "GIT_PROCESS_COMPLETED" : "GIT_PROCESS_EXITED");
            return result;
        }
        catch (GitProcessTimeoutException exception)
        {
            await RecordObservationAsync(execution, stage, category, started.Elapsed, "GitProcess", "Timeout", "GitProcessTimeout", "RULE_SOURCE_PROCESS_TIMEOUT");
            throw new RulePublicationException(
                "RULE_SOURCE_PROCESS_TIMEOUT",
                stage,
                $"Git Rule Source operation {category} exceeded the configured individual process timeout.",
                retrySafe: true,
                administrativeInterventionRequired: false,
                exception);
        }
        catch (OperationCanceledException)
        {
            await RecordObservationAsync(execution, stage, category, started.Elapsed, "GitProcess", "Cancelled", "ParentToken", "GIT_PROCESS_CANCELLED");
            throw;
        }
        catch (Exception exception)
        {
            await RecordObservationAsync(execution, stage, category, started.Elapsed, "GitProcess", "Failed", null, CodeFor(stage));
            throw new RulePublicationException(
                CodeFor(stage),
                stage,
                "The configured Git executable could not complete the Rule Source operation.",
                retrySafe: true,
                administrativeInterventionRequired: true,
                exception);
        }
    }

    private async Task RecordObservationAsync(
        RulePublicationExecutionContext execution,
        RulePublicationStage stage,
        string category,
        TimeSpan elapsed,
        string timeoutScope,
        string outcome,
        string? cancellationSource,
        string code)
    {
        try
        {
            await diagnostics.RecordEventAsync(
                new ManagedDiagnosticContext(
                    execution.CorrelationId,
                    "GitRuleSource",
                    stage,
                    code,
                    DateTimeOffset.UtcNow - elapsed,
                    ruleSettings.RulesetId,
                    Outcome: outcome,
                    RetrySafe: outcome is "Timeout" or "Cancelled",
                    SafeDetail: string.Join(
                        ';',
                        $"CommandCategory={category}",
                        $"ElapsedMs={Math.Max(0, (long)elapsed.TotalMilliseconds)}",
                        $"TimeoutScope={timeoutScope}",
                        $"Outcome={outcome}",
                        $"CancellationSource={cancellationSource ?? "None"}"),
                    OperationId: execution.OperationId),
                CancellationToken.None);
        }
        catch (Exception)
        {
            // Diagnostic recording cannot replace source acquisition semantics.
        }
    }

    private static RuleSourceManifest ParseManifest(
        string json,
        RuleSourceReleaseChannel channel,
        string requestedRef,
        string sourceIdentity)
    {
        try
        {
            return JsonSerializer.Deserialize<RuleSourceManifest>(json, ManifestJsonOptions)
                ?? throw new JsonException("The manifest deserialized to null.");
        }
        catch (JsonException exception)
        {
            throw Incompatible("The selected Rule Source contains an unreadable Managed rule-source manifest.", channel, requestedRef, sourceIdentity, exception);
        }
    }

    private void ValidateManifestCompatibility(
        RuleSourceManifest manifest,
        RuleSourceReleaseChannel channel,
        string requestedRef,
        string sourceIdentity)
    {
        if (manifest.ManifestFormatVersion != SupportedManifestFormatVersion)
        {
            throw Incompatible($"Managed manifest format '{manifest.ManifestFormatVersion}' is unsupported; expected '{SupportedManifestFormatVersion}'.", channel, requestedRef, sourceIdentity);
        }

        if (!string.Equals(manifest.CompilerContractVersion, ruleSettings.CompilerVersion, StringComparison.Ordinal))
        {
            throw Incompatible($"Managed compiler contract '{manifest.CompilerContractVersion}' is incompatible with compiler '{ruleSettings.CompilerVersion}'.", channel, requestedRef, sourceIdentity);
        }

        if (!string.Equals(manifest.RulesetId, ruleSettings.RulesetId, StringComparison.Ordinal))
        {
            throw Incompatible($"Managed manifest RuleSet '{manifest.RulesetId}' does not match selected RuleSet '{ruleSettings.RulesetId}'.", channel, requestedRef, sourceIdentity);
        }

        if (string.IsNullOrWhiteSpace(manifest.RepositoryVersion) || manifest.Sources.Count == 0)
        {
            throw Incompatible("The Managed rule-source manifest must declare a repository version and at least one source document.", channel, requestedRef, sourceIdentity);
        }
    }

    private static RulePublicationException Incompatible(
        string message,
        RuleSourceReleaseChannel channel,
        string requestedRef,
        string sourceIdentity,
        Exception? innerException = null,
        RulePublicationStage stage = RulePublicationStage.ReadManifest) =>
        new(
            "RULE_SOURCE_INCOMPATIBLE",
            stage,
            message,
            retrySafe: false,
            administrativeInterventionRequired: true,
            innerException,
            channel,
            requestedRef,
            sourceIdentity);

    private static RulePublicationException GitFailure(
        string code,
        RulePublicationStage stage,
        GitProcessResult result,
        string safeMessage,
        bool administrativeInterventionRequired,
        RuleSourceReleaseChannel? channel,
        string? requestedRef)
    {
        var detail = DiagnosticRedactor.Redact(result.StandardError)?.Trim();
        var bounded = string.IsNullOrWhiteSpace(detail)
            ? "No Git diagnostic text was returned."
            : detail.Length <= 1000 ? detail : detail[..1000];
        return new RulePublicationException(
            code,
            stage,
            safeMessage,
            retrySafe: code is not ("RULE_SOURCE_INCOMPATIBLE" or "RULE_SOURCE_REF_NOT_FOUND"),
            administrativeInterventionRequired,
            new InvalidOperationException($"Git exited with code {result.ExitCode}: {bounded}"),
            channel,
            requestedRef);
    }

    private string ResolveCacheRoot()
    {
        var root = string.IsNullOrWhiteSpace(administration.ManagedRuleCacheDirectory)
            ? Path.Combine(Path.GetTempPath(), "EternalCycle", "rule-cache")
            : Path.GetFullPath(administration.ManagedRuleCacheDirectory);
        Directory.CreateDirectory(root);
        return root;
    }

    private static void ValidateRemoteLocator(string sourceLocation)
    {
        if (!Uri.TryCreate(sourceLocation, UriKind.Absolute, out var sourceUri) ||
            sourceUri.Scheme is not ("https" or "http" or "ssh" or "git" or "file"))
        {
            throw new ManagedServiceException(
                "RULE_SOURCE_UNAVAILABLE",
                "The configured Git Rule Source is unavailable or uses an unsupported locator.");
        }
    }

    private static void WritePayloadFile(string root, string relativePath, string content)
    {
        var path = ResolvePayloadPath(root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string ResolvePayloadPath(string root, string relativePath)
    {
        var normalized = ValidateRepositoryPath(relativePath);
        var fullRoot = Path.GetFullPath(root);
        var fullPath = Path.GetFullPath(Path.Combine(fullRoot, normalized.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Rule-source payload path escaped its materialization root.");
        }

        return fullPath;
    }

    private static bool IsMissingRef(string value) =>
        value.Contains("couldn't find remote ref", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("not our ref", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("unknown revision", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("bad revision", StringComparison.OrdinalIgnoreCase);

    private static string CodeFor(RulePublicationStage stage) => stage switch
    {
        RulePublicationStage.AcquireSource => "RULE_SOURCE_ACQUISITION_FAILED",
        RulePublicationStage.ResolveRef => "RULE_SOURCE_REF_RESOLUTION_FAILED",
        RulePublicationStage.ReadManifest or RulePublicationStage.ReadRuleDocuments => "RULE_SOURCE_READ_FAILED",
        _ => "RULE_SOURCE_OPERATION_FAILED"
    };

    private static void TryDeleteDirectory(string path)
    {
        try
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
        catch (Exception)
        {
            // A later acquisition uses a fresh temporary directory.
        }
    }

    private static void CleanupLegacyCacheArtifacts(string sourceCache)
    {
        foreach (var file in Directory.EnumerateFiles(sourceCache, "*", SearchOption.TopDirectoryOnly))
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (var directory in Directory.EnumerateDirectories(sourceCache, "*", SearchOption.TopDirectoryOnly))
        {
            if (string.Equals(Path.GetFileName(directory), "snapshots", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            TryDeleteDirectory(directory);
        }
    }

    private static string ValidateRepositoryPath(string value)
    {
        var normalized = RequireValue(value, "repositoryPath").Replace('\\', '/');
        if (Path.IsPathRooted(normalized) ||
            normalized.Contains(':', StringComparison.Ordinal) ||
            normalized.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            throw new InvalidOperationException("Rule-source paths must be safe repository-relative paths.");
        }

        return normalized;
    }

    private static string RequireValue(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 2000)
        {
            throw new ArgumentException("Configured rule-source values must contain 1 to 2000 non-whitespace characters.", name);
        }

        return value;
    }

    private static TimeSpan Positive(TimeSpan value, TimeSpan fallback) =>
        value > TimeSpan.Zero ? value : fallback;

    [GeneratedRegex("^[0-9a-fA-F]{40,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex CommitSha();

    private sealed record MaterializedGitPayload(
        string PayloadRoot,
        string SourceIdentity,
        RuleSourceVersionMetadata? VersionMetadata);
}
