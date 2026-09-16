using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public enum RuleLayer
{
    RuntimeKernel,
    Core,
    World,
    OptionalModule
}

public sealed record RuleSourceMetadata(
    RuleLayer Layer,
    IReadOnlyList<string> WorldModelIds,
    IReadOnlyList<string> ModuleIds,
    IReadOnlyList<string> CampaignModes,
    IReadOnlyList<string> Operations,
    IReadOnlyList<string> Topics,
    int Priority = 0,
    bool AlwaysInclude = false);

public sealed record RuleSourceDocument(
    string RuleSourceId,
    string SourcePath,
    string Content,
    RuleSourceMetadata Metadata);

public sealed record CompiledRuleChunk(
    string ChunkId,
    string RuleSourceId,
    string SourcePath,
    string? SourceAnchor,
    string SourceHash,
    RuleSourceMetadata Metadata,
    int EstimatedTokens,
    string Content);

public sealed record CompiledRuleIndex(
    string RepositoryVersion,
    DateTimeOffset CompiledAt,
    IReadOnlyList<CompiledRuleChunk> Chunks);

public sealed record RuleContextRequest(
    string CampaignId,
    string Operation,
    IReadOnlyList<string> Topics,
    string CampaignMode,
    IReadOnlyList<string>? OptionalModules = null,
    int MaxEstimatedTokens = RuleCompiler.DefaultContextBudget);

public sealed record RuleContextResult(
    string CampaignId,
    string WorldModelId,
    string RulesetVersion,
    string RepositoryVersion,
    int EstimatedTokens,
    int MaximumEstimatedTokens,
    IReadOnlyList<CompiledRuleChunk> Chunks);

public sealed class RuleSourceManifest
{
    public string RepositoryVersion { get; init; } = string.Empty;

    public IList<RuleSourceManifestEntry> Sources { get; init; } = [];
}

public sealed class RuleSourceManifestEntry
{
    public string RuleSourceId { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public RuleLayer Layer { get; init; }

    public IList<string> WorldModelIds { get; init; } = [];

    public IList<string> ModuleIds { get; init; } = [];

    public IList<string> CampaignModes { get; init; } = ["*"];

    public IList<string> Operations { get; init; } = ["*"];

    public IList<string> Topics { get; init; } = [];

    public int Priority { get; init; }

    public bool AlwaysInclude { get; init; }
}

public sealed class RuleRetrievalOptions
{
    public string RepositoryRoot { get; init; } = string.Empty;

    public string ManifestPath { get; init; } = string.Empty;

    public int MaximumEstimatedTokens { get; init; } = RuleCompiler.DefaultContextBudget;

    public string DefaultCampaignMode { get; init; } = "NORMAL";

    public IDictionary<string, string> CampaignModes { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public IDictionary<string, string[]> CampaignOptionalModules { get; init; } =
        new Dictionary<string, string[]>(StringComparer.Ordinal);
}

public interface IRuleContextProvider
{
    Task<RuleContextResult> GetContextAsync(
        RuleContextRequest request,
        CancellationToken cancellationToken);
}

public sealed class RepositoryRuleContextProvider(
    IOptions<RuleRetrievalOptions> options,
    ICampaignSchemaResolver schemaResolver) : IRuleContextProvider
{
    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly RuleRetrievalOptions settings = options.Value;
    private readonly SemaphoreSlim compilationLock = new(1, 1);
    private CompiledRuleIndex? compiledIndex;

    public async Task<RuleContextResult> GetContextAsync(
        RuleContextRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var route = schemaResolver.Resolve(request.CampaignId);
        var index = await GetIndexAsync(cancellationToken);
        var campaignMode = settings.CampaignModes.TryGetValue(request.CampaignId, out var configuredMode)
            ? configuredMode
            : settings.DefaultCampaignMode;
        var optionalModules = settings.CampaignOptionalModules.TryGetValue(
            request.CampaignId,
            out var configuredModules)
            ? configuredModules
            : [];
        var configuredMaximum = settings.MaximumEstimatedTokens is > 0 and <= RuleCompiler.DefaultContextBudget
            ? settings.MaximumEstimatedTokens
            : RuleCompiler.DefaultContextBudget;
        var requestedMaximum = request.MaxEstimatedTokens > 0
            ? Math.Min(request.MaxEstimatedTokens, configuredMaximum)
            : configuredMaximum;

        return RuleCompiler.Select(
            index,
            route,
            request with
            {
                CampaignMode = campaignMode,
                OptionalModules = optionalModules,
                MaxEstimatedTokens = requestedMaximum
            });
    }

    private async Task<CompiledRuleIndex> GetIndexAsync(CancellationToken cancellationToken)
    {
        if (compiledIndex is not null)
        {
            return compiledIndex;
        }

        await compilationLock.WaitAsync(cancellationToken);
        try
        {
            if (compiledIndex is not null)
            {
                return compiledIndex;
            }

            if (string.IsNullOrWhiteSpace(settings.RepositoryRoot) ||
                string.IsNullOrWhiteSpace(settings.ManifestPath))
            {
                throw new InvalidOperationException(
                    "Rule retrieval requires trusted RepositoryRoot and ManifestPath configuration.");
            }

            var root = Path.GetFullPath(settings.RepositoryRoot);
            var manifestPath = Path.IsPathRooted(settings.ManifestPath)
                ? Path.GetFullPath(settings.ManifestPath)
                : Path.GetFullPath(Path.Combine(root, settings.ManifestPath));
            EnsureInsideRoot(root, manifestPath);

            var manifestJson = await File.ReadAllTextAsync(manifestPath, cancellationToken);
            var manifest = JsonSerializer.Deserialize<RuleSourceManifest>(manifestJson, ManifestJsonOptions)
                ?? throw new InvalidOperationException("The configured rule-source manifest is unreadable.");
            var documents = new List<RuleSourceDocument>(manifest.Sources.Count);

            foreach (var source in manifest.Sources)
            {
                var sourcePath = Path.GetFullPath(Path.Combine(root, source.Path));
                EnsureInsideRoot(root, sourcePath);
                var content = await File.ReadAllTextAsync(sourcePath, cancellationToken);
                documents.Add(new RuleSourceDocument(
                    RequireIdentifier(source.RuleSourceId, nameof(source.RuleSourceId)),
                    Path.GetRelativePath(root, sourcePath).Replace('\\', '/'),
                    content,
                    new RuleSourceMetadata(
                        source.Layer,
                        source.WorldModelIds.ToArray(),
                        source.ModuleIds.ToArray(),
                        source.CampaignModes.ToArray(),
                        source.Operations.ToArray(),
                        source.Topics.ToArray(),
                        source.Priority,
                        source.AlwaysInclude)));
            }

            compiledIndex = RuleCompiler.Compile(
                RequireIdentifier(manifest.RepositoryVersion, nameof(manifest.RepositoryVersion)),
                documents);
            return compiledIndex;
        }
        finally
        {
            compilationLock.Release();
        }
    }

    private static void EnsureInsideRoot(string root, string path)
    {
        var prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("A rule-source manifest path escapes the configured repository root.");
        }
    }

    private static string RequireIdentifier(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
        {
            throw new ArgumentException("Rule identifiers and versions must contain 1 to 128 non-whitespace characters.", name);
        }

        return value;
    }
}

public static class RuleCompiler
{
    public const int DefaultContextBudget = 8_000;

    public static CompiledRuleIndex Compile(
        string repositoryVersion,
        IEnumerable<RuleSourceDocument> documents)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryVersion);
        ArgumentNullException.ThrowIfNull(documents);

        var chunks = new List<CompiledRuleChunk>();
        var sourceIds = new HashSet<string>(StringComparer.Ordinal);
        var chunkIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var document in documents)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(document.RuleSourceId);
            ArgumentException.ThrowIfNullOrWhiteSpace(document.SourcePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(document.Content);
            if (!sourceIds.Add(document.RuleSourceId))
            {
                throw new InvalidOperationException($"Duplicate Rule Source ID '{document.RuleSourceId}'.");
            }

            if (document.Metadata.Layer == RuleLayer.World && document.Metadata.WorldModelIds.Count == 0)
            {
                throw new InvalidOperationException($"World rule source '{document.RuleSourceId}' must identify at least one World Model.");
            }

            if (document.Metadata.Layer == RuleLayer.OptionalModule && document.Metadata.ModuleIds.Count == 0)
            {
                throw new InvalidOperationException($"Optional-module rule source '{document.RuleSourceId}' must identify at least one module.");
            }

            var sourceHash = ComputeHash(document.Content);
            foreach (var section in SplitMarkdown(document.Content))
            {
                var chunkId = section.Anchor is null
                    ? document.RuleSourceId
                    : $"{document.RuleSourceId}#{section.Anchor}";
                if (!chunkIds.Add(chunkId))
                {
                    throw new InvalidOperationException($"Duplicate compiled Rule Chunk ID '{chunkId}'.");
                }

                chunks.Add(new CompiledRuleChunk(
                    chunkId,
                    document.RuleSourceId,
                    document.SourcePath,
                    section.Anchor,
                    sourceHash,
                    document.Metadata,
                    EstimateTokens(section.Content),
                    section.Content));
            }
        }

        return new CompiledRuleIndex(repositoryVersion, DateTimeOffset.UtcNow, chunks);
    }

    public static RuleContextResult Select(
        CompiledRuleIndex index,
        CampaignSchemaRoute route,
        RuleContextRequest request)
    {
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(request);

        if (!string.Equals(route.CampaignId, request.CampaignId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Rule retrieval route and requested Campaign ID do not match.");
        }

        if (request.MaxEstimatedTokens is <= 0 or > DefaultContextBudget)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.MaxEstimatedTokens),
                $"Normal rule context must use 1 to {DefaultContextBudget} estimated tokens.");
        }

        var topics = Normalize(request.Topics);
        var modules = Normalize(request.OptionalModules ?? []);
        var selected = new List<CompiledRuleChunk>();
        var used = 0;

        var eligible = index.Chunks
            .Where(chunk => IsEligible(chunk, route, request, topics, modules))
            .OrderBy(chunk => LayerOrder(chunk.Metadata.Layer))
            .ThenByDescending(chunk => Score(chunk, request, topics))
            .ThenBy(chunk => chunk.ChunkId, StringComparer.Ordinal)
            .ToArray();

        if (!eligible.Any(chunk => chunk.Metadata.Layer == RuleLayer.RuntimeKernel))
        {
            throw new InvalidOperationException("The compiled index has no applicable Runtime Rule Kernel.");
        }

        foreach (var chunk in eligible.Where(IsMandatoryKernel))
        {
            if (used + chunk.EstimatedTokens > request.MaxEstimatedTokens)
            {
                throw new InvalidOperationException(
                    "The Runtime Rule Kernel exceeds the configured context budget and must be reduced before play.");
            }

            selected.Add(chunk);
            used += chunk.EstimatedTokens;
        }

        foreach (var chunk in eligible.Where(chunk => !IsMandatoryKernel(chunk)))
        {
            if (used + chunk.EstimatedTokens > request.MaxEstimatedTokens)
            {
                continue;
            }

            selected.Add(chunk);
            used += chunk.EstimatedTokens;
        }

        return new RuleContextResult(
            request.CampaignId,
            route.WorldModelId,
            route.RulesetVersion,
            index.RepositoryVersion,
            used,
            request.MaxEstimatedTokens,
            selected);
    }

    private static bool IsEligible(
        CompiledRuleChunk chunk,
        CampaignSchemaRoute route,
        RuleContextRequest request,
        IReadOnlySet<string> topics,
        IReadOnlySet<string> modules)
    {
        var metadata = chunk.Metadata;
        if (metadata.Layer == RuleLayer.RuntimeKernel)
        {
            return true;
        }

        if (metadata.Layer == RuleLayer.World &&
            !Matches(metadata.WorldModelIds, route.WorldModelId))
        {
            return false;
        }

        if (metadata.Layer == RuleLayer.OptionalModule &&
            !metadata.ModuleIds.Any(modules.Contains))
        {
            return false;
        }

        if (metadata.WorldModelIds.Count > 0 &&
            !Matches(metadata.WorldModelIds, route.WorldModelId))
        {
            return false;
        }

        if (!Matches(metadata.CampaignModes, request.CampaignMode) ||
            !Matches(metadata.Operations, request.Operation))
        {
            return false;
        }

        if (metadata.AlwaysInclude)
        {
            return true;
        }

        return metadata.Topics.Count == 0 ||
            metadata.Topics.Contains("*", StringComparer.OrdinalIgnoreCase) ||
            metadata.Topics.Any(topics.Contains);
    }

    private static bool Matches(IReadOnlyList<string> values, string value) =>
        values.Count == 0 ||
        values.Contains("*", StringComparer.OrdinalIgnoreCase) ||
        values.Contains(value, StringComparer.OrdinalIgnoreCase);

    private static bool IsMandatoryKernel(CompiledRuleChunk chunk) =>
        chunk.Metadata.Layer == RuleLayer.RuntimeKernel || chunk.Metadata.AlwaysInclude;

    private static int LayerOrder(RuleLayer layer) => layer switch
    {
        RuleLayer.RuntimeKernel => 0,
        RuleLayer.Core => 1,
        RuleLayer.World => 2,
        RuleLayer.OptionalModule => 3,
        _ => 4
    };

    private static int Score(
        CompiledRuleChunk chunk,
        RuleContextRequest request,
        IReadOnlySet<string> topics)
    {
        var score = chunk.Metadata.Priority;
        if (chunk.Metadata.Operations.Contains(request.Operation, StringComparer.OrdinalIgnoreCase))
        {
            score += 100;
        }

        score += chunk.Metadata.Topics.Count(topics.Contains) * 10;
        return score;
    }

    private static IReadOnlySet<string> Normalize(IEnumerable<string> values) =>
        values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static IEnumerable<(string? Anchor, string Content)> SplitMarkdown(string content)
    {
        var lines = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var current = new StringBuilder();
        var anchors = new Dictionary<string, int>(StringComparer.Ordinal);
        string? anchor = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("## ", StringComparison.Ordinal) && current.Length > 0)
            {
                yield return (anchor, current.ToString().Trim());
                current.Clear();
                anchor = NextAnchor(line[3..], anchors);
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                anchor = NextAnchor(line[3..], anchors);
            }

            current.AppendLine(line);
        }

        if (current.Length > 0)
        {
            yield return (anchor, current.ToString().Trim());
        }
    }

    private static string ToAnchor(string heading)
    {
        var normalized = new string(heading
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) || character is ' ' or '-'
                ? character
                : '\0')
            .Where(character => character != '\0')
            .ToArray());
        return string.Join('-', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string NextAnchor(string heading, IDictionary<string, int> anchors)
    {
        var baseAnchor = ToAnchor(heading);
        if (!anchors.TryGetValue(baseAnchor, out var count))
        {
            anchors[baseAnchor] = 0;
            return baseAnchor;
        }

        count++;
        anchors[baseAnchor] = count;
        return $"{baseAnchor}-{count}";
    }

    private static int EstimateTokens(string content) =>
        Math.Max(1, (Encoding.UTF8.GetByteCount(content) + 2) / 3);

    private static string ComputeHash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
