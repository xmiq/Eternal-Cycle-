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
}

public sealed partial class GitRuleSourceProvider : IRuleSourceProvider
{
    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ManagedRuleServiceOptions ruleSettings;
    private readonly GitRuleSourceOptions settings;
    private readonly IRuleSourceConfigurationStore? configurations;
    private readonly ManagedAdministrationOptions administration;

    public GitRuleSourceProvider(IOptions<ManagedRuleServiceOptions> options)
        : this(options, null, Options.Create(new ManagedAdministrationOptions()))
    {
    }

    public GitRuleSourceProvider(
        IOptions<ManagedRuleServiceOptions> options,
        IRuleSourceConfigurationStore? configurations,
        IOptions<ManagedAdministrationOptions> administrationOptions)
    {
        ruleSettings = options.Value;
        settings = ruleSettings.GitSource;
        this.configurations = configurations;
        administration = administrationOptions.Value;
    }

    public async Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
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

        var requestedRef = useAdvancedLocal ? settings.Ref : configuration!.RequestedRef;
        var manifestSetting = useAdvancedLocal ? settings.ManifestPath : configuration!.ManifestPath;
        var resolved = useAdvancedLocal
            ? new ResolvedGitSource(Path.GetFullPath(settings.RepositoryRoot), false)
            : await ResolveManagedRepositoryAsync(configuration!, cancellationToken);
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
                cancellationToken);
        }

        var sourceIdentity = (await RunGitAsync(
            repositoryRoot,
            ["rev-parse", $"{ResolveRef(requestedRef, resolved.ManagedRemote)}^{{commit}}"],
            cancellationToken)).Trim();
        if (!CommitSha().IsMatch(sourceIdentity))
        {
            throw new InvalidOperationException("Git source did not resolve to an immutable commit SHA.");
        }

        var manifestPath = ValidateRepositoryPath(manifestSetting);
        var manifestJson = await ReadAtCommitAsync(
            repositoryRoot,
            sourceIdentity,
            manifestPath,
            cancellationToken);
        var manifest = JsonSerializer.Deserialize<RuleSourceManifest>(manifestJson, ManifestJsonOptions)
            ?? throw new InvalidOperationException("The configured rule-source manifest is unreadable.");
        var documents = new List<RuleSourceDocument>(manifest.Sources.Count);

        foreach (var source in manifest.Sources)
        {
            var sourcePath = ValidateRepositoryPath(source.Path);
            var content = await ReadAtCommitAsync(
                repositoryRoot,
                sourceIdentity,
                sourcePath,
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
            sourceIdentity.ToUpperInvariant(),
            RequireValue(manifest.RepositoryVersion, nameof(manifest.RepositoryVersion)),
            documents,
            DateTimeOffset.UtcNow);
    }

    private async Task<ResolvedGitSource> ResolveManagedRepositoryAsync(
        RuleSourceConfiguration configuration,
        CancellationToken cancellationToken)
    {
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
        if (!Directory.Exists(Path.Combine(repositoryRoot, ".git")))
        {
            if (Directory.Exists(repositoryRoot))
            {
                throw new ManagedServiceException(
                    "RULE_SOURCE_CACHE_INVALID",
                    "The managed Rule Source cache exists but is not a valid Git checkout.");
            }

            await RunProcessAsync(
                cacheRoot,
                ["clone", "--no-checkout", configuration.SourceLocation, repositoryRoot],
                cancellationToken);
        }
        else
        {
            await RunProcessAsync(repositoryRoot, ["fetch", "--prune", "origin"], cancellationToken);
        }

        return new ResolvedGitSource(repositoryRoot, true);
    }

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

    private async Task<string> ReadAtCommitAsync(
        string repositoryRoot,
        string sourceIdentity,
        string repositoryPath,
        CancellationToken cancellationToken) =>
        await RunGitAsync(
            repositoryRoot,
            ["show", $"{sourceIdentity}:{repositoryPath}"],
            cancellationToken);

    private async Task<string> RunGitAsync(
        string repositoryRoot,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken) =>
        await RunProcessAsync(repositoryRoot, arguments, cancellationToken);

    private async Task<string> RunProcessAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = RequireValue(settings.GitExecutable, nameof(settings.GitExecutable)),
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
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var output = await outputTask;
        var error = await errorTask;
        if (process.ExitCode != 0)
        {
            throw new ManagedServiceException(
                "RULE_SOURCE_UNAVAILABLE",
                $"Git Rule Source operation failed with exit code {process.ExitCode}; inspect authorized sanitized service logs.");
        }

        return output;
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
