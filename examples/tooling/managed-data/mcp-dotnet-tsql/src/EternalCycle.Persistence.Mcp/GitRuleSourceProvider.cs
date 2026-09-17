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
            await TerminateBoundedAsync(
                process,
                outputTask,
                errorTask,
                Positive(terminationGracePeriod, TimeSpan.FromSeconds(5)));
            if (callerCancelled)
            {
                throw new OperationCanceledException(
                    "The Git operation was cancelled by the caller.",
                    exception,
                    cancellationToken);
            }

            throw new GitProcessTimeoutException("The Git process exceeded its configured timeout.", exception);
        }

        var readers = Task.WhenAll(outputTask, errorTask);
        if (await Task.WhenAny(readers, Task.Delay(Positive(terminationGracePeriod, TimeSpan.FromSeconds(5)))) != readers)
        {
            await TerminateBoundedAsync(
                process,
                outputTask,
                errorTask,
                Positive(terminationGracePeriod, TimeSpan.FromSeconds(5)));
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
            // The caller receives the original operation failure after this bounded grace period.
        }

        _ = await Task.WhenAny(Task.WhenAll(outputTask, errorTask), Task.Delay(gracePeriod));
    }

    private static TimeSpan Positive(TimeSpan value, TimeSpan fallback) =>
        value > TimeSpan.Zero ? value : fallback;
}

public sealed partial class GitRuleSourceProvider : IRuleSourceProvider
{
    private const int SupportedManifestFormatVersion = 1;
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

    public GitRuleSourceProvider(IOptions<ManagedRuleServiceOptions> options)
        : this(options, null, Options.Create(new ManagedAdministrationOptions()), new SystemGitProcessRunner())
    {
    }

    public GitRuleSourceProvider(
        IOptions<ManagedRuleServiceOptions> options,
        IRuleSourceConfigurationStore? configurations,
        IOptions<ManagedAdministrationOptions> administrationOptions)
        : this(options, configurations, administrationOptions, new SystemGitProcessRunner())
    {
    }

    internal GitRuleSourceProvider(
        IOptions<ManagedRuleServiceOptions> options,
        IRuleSourceConfigurationStore? configurations,
        IOptions<ManagedAdministrationOptions> administrationOptions,
        IGitProcessRunner processRunner)
    {
        ruleSettings = options.Value;
        settings = ruleSettings.GitSource;
        this.configurations = configurations;
        administration = administrationOptions.Value;
        this.processRunner = processRunner;
    }

    public Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken) =>
        GetSnapshotAsync(null, cancellationToken);

    public async Task<RuleSourceSnapshot> GetSnapshotAsync(
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken)
    {
        onStage?.Invoke(RulePublicationStage.AcquireSource);
        var configuration = configurations is null
            ? null
            : await configurations.GetAsync(ruleSettings.RulesetId, cancellationToken);
        var useAdvancedLocal = !string.IsNullOrWhiteSpace(settings.RepositoryRoot);
        if (!useAdvancedLocal && configuration is null)
        {
            throw new ManagedServiceException(
                "RULE_SOURCE_NOT_CONFIGURED",
                "No Rule Source is configured. Select the official source or an approved compatible source.");
        }

        if (configuration is not null &&
            !string.Equals(configuration.ProviderKind, "Git", StringComparison.OrdinalIgnoreCase))
        {
            throw new ManagedServiceException(
                "RULE_SOURCE_PROVIDER_UNSUPPORTED",
                "The selected Rule Source Provider is not supported by this reference implementation.");
        }

        var releaseChannel = useAdvancedLocal ? ruleSettings.ReleaseChannel : configuration!.ReleaseChannel;
        var requestedRef = useAdvancedLocal ? settings.Ref : configuration!.RequestedRef;
        var manifestSetting = useAdvancedLocal ? settings.ManifestPath : configuration!.ManifestPath;
        var resolved = useAdvancedLocal
            ? new ResolvedGitSource(Path.GetFullPath(settings.RepositoryRoot), false)
            : await ResolveManagedRepositoryAsync(configuration!, requestedRef, onStage, cancellationToken);
        var repositoryRoot = resolved.RepositoryRoot;
        if (!Directory.Exists(repositoryRoot))
        {
            throw new ManagedServiceException(
                "RULE_SOURCE_UNAVAILABLE",
                "The configured Git Rule Source is unavailable; inspect authorized sanitized service logs.");
        }

        if (useAdvancedLocal && settings.FetchBeforeCheck)
        {
            _ = await RunGitAsync(
                repositoryRoot,
                ["fetch", "--prune", RequireValue(settings.Remote, nameof(settings.Remote))],
                RulePublicationStage.AcquireSource,
                networkOperation: true,
                cancellationToken);
        }

        onStage?.Invoke(RulePublicationStage.ResolveRef);
        var sourceIdentity = (await RunGitAsync(
            repositoryRoot,
            ["rev-parse", $"{ResolveRef(requestedRef, resolved.ManagedRemote)}^{{commit}}"],
            RulePublicationStage.ResolveRef,
            networkOperation: false,
            cancellationToken)).Trim();
        if (!CommitSha().IsMatch(sourceIdentity))
        {
            throw new RulePublicationException(
                "RULE_SOURCE_REF_RESOLUTION_FAILED",
                RulePublicationStage.ResolveRef,
                "The configured Rule Source ref did not resolve to an immutable commit SHA.",
                retrySafe: true,
                administrativeInterventionRequired: true,
                releaseChannel: releaseChannel,
                discoveryRef: requestedRef);
        }

        sourceIdentity = sourceIdentity.ToUpperInvariant();
        onStage?.Invoke(RulePublicationStage.ReadManifest);
        var manifestPath = ValidateRepositoryPath(manifestSetting);
        var manifestJson = await ReadRequiredArtifactAsync(
            repositoryRoot,
            sourceIdentity,
            manifestPath,
            RulePublicationStage.ReadManifest,
            releaseChannel,
            requestedRef,
            "The selected immutable Rule Source does not contain the required Managed rule-source manifest.",
            cancellationToken);
        RuleSourceManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<RuleSourceManifest>(manifestJson, ManifestJsonOptions)
                ?? throw new JsonException("The manifest deserialized to null.");
        }
        catch (JsonException exception)
        {
            throw Incompatible(
                "The selected Rule Source contains an unreadable Managed rule-source manifest.",
                releaseChannel,
                requestedRef,
                sourceIdentity,
                exception);
        }

        ValidateManifestCompatibility(manifest, releaseChannel, requestedRef, sourceIdentity);
        var documents = new List<RuleSourceDocument>(manifest.Sources.Count);

        onStage?.Invoke(RulePublicationStage.ReadRuleDocuments);
        foreach (var source in manifest.Sources)
        {
            var sourcePath = ValidateRepositoryPath(source.Path);
            var content = await ReadRequiredArtifactAsync(
                repositoryRoot,
                sourceIdentity,
                sourcePath,
                RulePublicationStage.ReadRuleDocuments,
                releaseChannel,
                requestedRef,
                "The selected Rule Source manifest references a required document that is absent from the immutable snapshot.",
                cancellationToken);
            documents.Add(new RuleSourceDocument(
                RequireValue(source.RuleSourceId, nameof(source.RuleSourceId)),
                sourcePath,
                content,
                new RuleSourceMetadata(
                    source.Layer,
                    source.WorldModelIds.ToArray(),
                    source.ModuleIds.ToArray(),
                    source.CampaignModes.ToArray(),
                    source.Operations.ToArray(),
                    source.Topics.ToArray(),
                    source.Priority,
                    source.AlwaysInclude,
                    source.Dependencies.ToArray())));
        }

        return new RuleSourceSnapshot(
            "Git",
            sourceIdentity,
            RequireValue(manifest.RepositoryVersion, nameof(manifest.RepositoryVersion)),
            documents,
            DateTimeOffset.UtcNow,
            releaseChannel,
            requestedRef,
            manifest.ManifestFormatVersion,
            manifest.CompilerContractVersion,
            manifest.RulesetId);
    }

    private async Task<ResolvedGitSource> ResolveManagedRepositoryAsync(
        RuleSourceConfiguration configuration,
        string requestedRef,
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken)
    {
        onStage?.Invoke(RulePublicationStage.AcquireSource);
        if (Directory.Exists(configuration.SourceLocation))
        {
            return new ResolvedGitSource(Path.GetFullPath(configuration.SourceLocation), false);
        }

        if (!Uri.TryCreate(configuration.SourceLocation, UriKind.Absolute, out var sourceUri) ||
            sourceUri.Scheme is not ("https" or "http" or "ssh" or "git" or "file"))
        {
            throw new ManagedServiceException(
                "RULE_SOURCE_UNAVAILABLE",
                "The configured Git Rule Source is unavailable or uses an unsupported locator.");
        }

        var cacheRoot = string.IsNullOrWhiteSpace(administration.ManagedRuleCacheDirectory)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EternalCycle",
                "rule-cache")
            : Path.GetFullPath(administration.ManagedRuleCacheDirectory);
        var cacheKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(configuration.SourceLocation)))[..24];
        var repositoryRoot = Path.Combine(cacheRoot, cacheKey);
        Directory.CreateDirectory(cacheRoot);
        var cacheLock = CacheLocks.GetOrAdd(repositoryRoot, _ => new SemaphoreSlim(1, 1));
        await cacheLock.WaitAsync(cancellationToken);
        try
        {
            if (!Directory.Exists(Path.Combine(repositoryRoot, ".git")))
            {
                CleanupInvalidCache(repositoryRoot);
                var partialRoot = $"{repositoryRoot}.partial-{Guid.NewGuid():N}";
                try
                {
                    await RunProcessAsync(
                        cacheRoot,
                        ["clone", "--no-checkout", configuration.SourceLocation, partialRoot],
                        RulePublicationStage.AcquireSource,
                        networkOperation: true,
                        cancellationToken);
                    if (!Directory.Exists(Path.Combine(partialRoot, ".git")))
                    {
                        throw new RulePublicationException(
                            "RULE_SOURCE_CACHE_INVALID",
                            RulePublicationStage.AcquireSource,
                            "Git acquisition completed without producing a valid managed no-checkout cache.",
                            retrySafe: true,
                            administrativeInterventionRequired: false,
                            releaseChannel: configuration.ReleaseChannel,
                            discoveryRef: requestedRef);
                    }

                    Directory.Move(partialRoot, repositoryRoot);
                }
                catch
                {
                    TryDeleteDirectory(partialRoot);
                    throw;
                }
            }
            else if (!await CanReuseCachedRefAsync(repositoryRoot, requestedRef, cancellationToken))
            {
                await RunProcessAsync(
                    repositoryRoot,
                    ["fetch", "--prune", "origin"],
                    RulePublicationStage.AcquireSource,
                    networkOperation: true,
                    cancellationToken);
            }
        }
        finally
        {
            cacheLock.Release();
        }

        return new ResolvedGitSource(repositoryRoot, true);
    }

    private async Task<bool> CanReuseCachedRefAsync(
        string repositoryRoot,
        string requestedRef,
        CancellationToken cancellationToken)
    {
        var immutableRef = CommitSha().IsMatch(requestedRef) ||
                           requestedRef.StartsWith("refs/tags/", StringComparison.Ordinal);
        if (!immutableRef && ruleSettings.UpdatePolicy != RuleUpdatePolicy.Disabled)
        {
            return false;
        }

        var result = await RunProcessResultAsync(
            repositoryRoot,
            ["rev-parse", $"{ResolveRef(requestedRef, managedRemote: true)}^{{commit}}"],
            RulePublicationStage.ResolveRef,
            networkOperation: false,
            cancellationToken);
        return result.ExitCode == 0 && CommitSha().IsMatch(result.StandardOutput.Trim());
    }

    private void ValidateManifestCompatibility(
        RuleSourceManifest manifest,
        RuleSourceReleaseChannel releaseChannel,
        string requestedRef,
        string sourceIdentity)
    {
        if (manifest.ManifestFormatVersion != SupportedManifestFormatVersion)
        {
            throw Incompatible(
                $"Managed manifest format '{manifest.ManifestFormatVersion}' is unsupported; expected '{SupportedManifestFormatVersion}'.",
                releaseChannel,
                requestedRef,
                sourceIdentity);
        }

        if (!string.Equals(manifest.CompilerContractVersion, ruleSettings.CompilerVersion, StringComparison.Ordinal))
        {
            throw Incompatible(
                $"Managed compiler contract '{manifest.CompilerContractVersion}' is incompatible with compiler '{ruleSettings.CompilerVersion}'.",
                releaseChannel,
                requestedRef,
                sourceIdentity);
        }

        if (!string.Equals(manifest.RulesetId, ruleSettings.RulesetId, StringComparison.Ordinal))
        {
            throw Incompatible(
                $"Managed manifest RuleSet '{manifest.RulesetId}' does not match selected RuleSet '{ruleSettings.RulesetId}'.",
                releaseChannel,
                requestedRef,
                sourceIdentity);
        }

        if (string.IsNullOrWhiteSpace(manifest.RepositoryVersion) || manifest.Sources.Count == 0)
        {
            throw Incompatible(
                "The Managed rule-source manifest does not declare a repository version and at least one required source document.",
                releaseChannel,
                requestedRef,
                sourceIdentity);
        }
    }

    private async Task<string> ReadRequiredArtifactAsync(
        string repositoryRoot,
        string sourceIdentity,
        string repositoryPath,
        RulePublicationStage stage,
        RuleSourceReleaseChannel releaseChannel,
        string requestedRef,
        string missingMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            return await RunGitAsync(
                repositoryRoot,
                ["show", $"{sourceIdentity}:{repositoryPath}"],
                stage,
                networkOperation: false,
                cancellationToken);
        }
        catch (RulePublicationException exception) when (exception.Code == CodeFor(stage))
        {
            throw Incompatible(missingMessage, releaseChannel, requestedRef, sourceIdentity, exception, stage);
        }
    }

    private static RulePublicationException Incompatible(
        string message,
        RuleSourceReleaseChannel releaseChannel,
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
            releaseChannel,
            requestedRef,
            sourceIdentity);

    private static string ResolveRef(string requestedRef, bool managedRemote)
    {
        var value = RequireValue(requestedRef, nameof(requestedRef));
        if (!managedRemote || CommitSha().IsMatch(value) || value.StartsWith("refs/tags/", StringComparison.Ordinal))
        {
            return value;
        }

        if (value.StartsWith("refs/heads/", StringComparison.Ordinal))
        {
            return $"refs/remotes/origin/{value[11..]}";
        }

        return value is "HEAD" ? "refs/remotes/origin/HEAD" : $"refs/remotes/origin/{value}";
    }

    private Task<string> RunGitAsync(
        string repositoryRoot,
        IReadOnlyList<string> arguments,
        RulePublicationStage stage,
        bool networkOperation,
        CancellationToken cancellationToken) =>
        RunProcessAsync(repositoryRoot, arguments, stage, networkOperation, cancellationToken);

    private async Task<string> RunProcessAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        RulePublicationStage stage,
        bool networkOperation,
        CancellationToken cancellationToken)
    {
        var result = await RunProcessResultAsync(
            workingDirectory,
            arguments,
            stage,
            networkOperation,
            cancellationToken);
        if (result.ExitCode != 0)
        {
            var detail = DiagnosticRedactor.Redact(result.StandardError)?.Trim();
            var boundedDetail = string.IsNullOrWhiteSpace(detail)
                ? "No Git diagnostic text was returned."
                : detail.Length <= 1000 ? detail : detail[..1000];
            throw new RulePublicationException(
                CodeFor(stage),
                stage,
                $"Git Rule Source operation failed at stage {stage}; inspect the correlated authorized diagnostic.",
                retrySafe: true,
                administrativeInterventionRequired: stage == RulePublicationStage.AcquireSource,
                innerException: new InvalidOperationException(
                    $"Git exited with code {result.ExitCode}: {boundedDetail}"));
        }

        return result.StandardOutput;
    }

    private async Task<GitProcessResult> RunProcessResultAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        RulePublicationStage stage,
        bool networkOperation,
        CancellationToken cancellationToken)
    {
        try
        {
            return await processRunner.RunAsync(
                RequireValue(settings.GitExecutable, nameof(settings.GitExecutable)),
                workingDirectory,
                arguments,
                networkOperation ? settings.AcquisitionTimeout : settings.ProcessTimeout,
                settings.TerminationGracePeriod,
                cancellationToken);
        }
        catch (GitProcessTimeoutException exception)
        {
            throw new RulePublicationException(
                "RULE_SOURCE_OPERATION_TIMEOUT",
                stage,
                $"Git Rule Source operation exceeded the configured {(networkOperation ? "acquisition" : "local process")} timeout at stage {stage}.",
                retrySafe: true,
                administrativeInterventionRequired: false,
                exception);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new RulePublicationException(
                CodeFor(stage),
                stage,
                "The configured Git executable could not complete the Rule Source operation.",
                retrySafe: true,
                administrativeInterventionRequired: true,
                exception);
        }
    }

    private static string CodeFor(RulePublicationStage stage) => stage switch
    {
        RulePublicationStage.AcquireSource => "RULE_SOURCE_ACQUISITION_FAILED",
        RulePublicationStage.ResolveRef => "RULE_SOURCE_REF_RESOLUTION_FAILED",
        RulePublicationStage.ReadManifest or RulePublicationStage.ReadRuleDocuments => "RULE_SOURCE_READ_FAILED",
        _ => "RULE_SOURCE_OPERATION_FAILED"
    };

    private static void CleanupInvalidCache(string repositoryRoot)
    {
        if (!Directory.Exists(repositoryRoot))
        {
            return;
        }

        try
        {
            Directory.Delete(repositoryRoot, recursive: true);
        }
        catch (Exception exception)
        {
            throw new RulePublicationException(
                "RULE_SOURCE_CACHE_INVALID",
                RulePublicationStage.AcquireSource,
                "The managed Rule Source cache is incomplete and could not be safely replaced.",
                retrySafe: true,
                administrativeInterventionRequired: true,
                exception);
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception)
        {
            // A later retry uses a fresh partial directory and can recover independently.
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
        if (string.IsNullOrWhiteSpace(value) || value.Length > 512)
        {
            throw new ArgumentException("Configured rule-source values must contain 1 to 512 non-whitespace characters.", name);
        }

        return value;
    }

    [GeneratedRegex("^[0-9a-fA-F]{40,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex CommitSha();

    private sealed record ResolvedGitSource(string RepositoryRoot, bool ManagedRemote);
}
