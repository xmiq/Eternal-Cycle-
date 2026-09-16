using System.Diagnostics;
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

public sealed partial class GitRuleSourceProvider(IOptions<ManagedRuleServiceOptions> options) : IRuleSourceProvider
{
    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly GitRuleSourceOptions settings = options.Value.GitSource;

    public async Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.RepositoryRoot))
        {
            throw new InvalidOperationException("Git rule source requires a trusted RepositoryRoot.");
        }

        var repositoryRoot = Path.GetFullPath(settings.RepositoryRoot);
        if (!Directory.Exists(repositoryRoot))
        {
            throw new DirectoryNotFoundException("The configured Git rule-source repository is unavailable.");
        }

        if (settings.FetchBeforeCheck)
        {
            _ = await RunGitAsync(
                repositoryRoot,
                ["fetch", "--prune", RequireValue(settings.Remote, nameof(settings.Remote))],
                cancellationToken);
        }

        var sourceIdentity = (await RunGitAsync(
            repositoryRoot,
            ["rev-parse", $"{RequireValue(settings.Ref, nameof(settings.Ref))}^{{commit}}"],
            cancellationToken)).Trim();
        if (!CommitSha().IsMatch(sourceIdentity))
        {
            throw new InvalidOperationException("Git source did not resolve to an immutable commit SHA.");
        }

        var manifestPath = ValidateRepositoryPath(settings.ManifestPath);
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
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = RequireValue(settings.GitExecutable, nameof(settings.GitExecutable)),
            WorkingDirectory = repositoryRoot,
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
            throw new InvalidOperationException(
                $"Git rule-source operation failed with exit code {process.ExitCode}: {Sanitize(error)}");
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

    private static string Sanitize(string value)
    {
        var singleLine = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return singleLine.Length <= 500 ? singleLine : singleLine[..500];
    }

    [GeneratedRegex("^[0-9a-fA-F]{40,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex CommitSha();
}
