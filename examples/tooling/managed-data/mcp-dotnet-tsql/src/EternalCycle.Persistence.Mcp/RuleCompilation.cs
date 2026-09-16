using System.Security.Cryptography;
using System.Text;

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
    bool AlwaysInclude = false,
    IReadOnlyList<string>? Dependencies = null);

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
    IReadOnlyList<CompiledRuleChunk> Chunks,
    string? RuleReleaseId = null,
    string? SourceIdentity = null);

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

    public IList<string> Dependencies { get; init; } = [];

    public int Priority { get; init; }

    public bool AlwaysInclude { get; init; }
}

public interface IRuleContextProvider
{
    Task<RuleContextResult> GetContextAsync(
        RuleContextRequest request,
        CancellationToken cancellationToken);
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

        var sourceDocuments = documents.ToArray();
        var chunks = new List<CompiledRuleChunk>();
        var sourceIds = new HashSet<string>(StringComparer.Ordinal);
        var chunkIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var document in sourceDocuments)
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

        foreach (var document in sourceDocuments)
        {
            foreach (var dependency in document.Metadata.Dependencies ?? [])
            {
                if (string.Equals(dependency, document.RuleSourceId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Rule source '{document.RuleSourceId}' cannot depend on itself.");
                }

                if (!sourceIds.Contains(dependency))
                {
                    throw new InvalidOperationException(
                        $"Rule source '{document.RuleSourceId}' depends on unknown source '{dependency}'.");
                }
            }
        }

        ValidateDependencyGraph(sourceDocuments);

        return new CompiledRuleIndex(repositoryVersion, DateTimeOffset.UtcNow, chunks);
    }

    private static void ValidateDependencyGraph(IReadOnlyList<RuleSourceDocument> documents)
    {
        var byId = documents.ToDictionary(document => document.RuleSourceId, StringComparer.Ordinal);
        var states = new Dictionary<string, int>(StringComparer.Ordinal);

        void Visit(string sourceId)
        {
            if (states.TryGetValue(sourceId, out var state))
            {
                if (state == 1)
                {
                    throw new InvalidOperationException($"Rule dependency cycle includes source '{sourceId}'.");
                }

                if (state == 2)
                {
                    return;
                }
            }

            states[sourceId] = 1;
            foreach (var dependency in byId[sourceId].Metadata.Dependencies ?? [])
            {
                Visit(dependency);
            }

            states[sourceId] = 2;
        }

        foreach (var sourceId in byId.Keys)
        {
            Visit(sourceId);
        }
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
        var selectedIds = new HashSet<string>(StringComparer.Ordinal);
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

        foreach (var chunk in eligible)
        {
            var closure = BuildDependencyClosure(index, chunk, route, request, modules)
                .Where(candidate => !selectedIds.Contains(candidate.ChunkId))
                .ToArray();
            var closureTokens = closure.Sum(candidate => candidate.EstimatedTokens);
            if (used + closureTokens > request.MaxEstimatedTokens)
            {
                if (IsMandatoryKernel(chunk))
                {
                    throw new InvalidOperationException(
                        "The mandatory Runtime Rule Kernel and its dependencies exceed the configured context budget.");
                }

                continue;
            }

            foreach (var candidate in closure)
            {
                selected.Add(candidate);
                selectedIds.Add(candidate.ChunkId);
                used += candidate.EstimatedTokens;
            }
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

    private static IReadOnlyList<CompiledRuleChunk> BuildDependencyClosure(
        CompiledRuleIndex index,
        CompiledRuleChunk root,
        CampaignSchemaRoute route,
        RuleContextRequest request,
        IReadOnlySet<string> modules)
    {
        var result = new List<CompiledRuleChunk>();
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);

        void Visit(CompiledRuleChunk chunk)
        {
            if (visited.Contains(chunk.ChunkId))
            {
                return;
            }

            if (!visiting.Add(chunk.ChunkId))
            {
                throw new InvalidOperationException(
                    $"Rule dependency cycle includes chunk '{chunk.ChunkId}'.");
            }

            foreach (var dependencySourceId in chunk.Metadata.Dependencies ?? [])
            {
                var dependencyChunks = index.Chunks
                    .Where(candidate =>
                        string.Equals(candidate.RuleSourceId, dependencySourceId, StringComparison.Ordinal) &&
                        IsDependencyApplicable(candidate, route, request, modules))
                    .OrderBy(candidate => candidate.ChunkId, StringComparer.Ordinal)
                    .ToArray();
                if (dependencyChunks.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"Rule dependency '{dependencySourceId}' is unavailable for campaign '{request.CampaignId}'.");
                }

                foreach (var dependencyChunk in dependencyChunks)
                {
                    Visit(dependencyChunk);
                }
            }

            visiting.Remove(chunk.ChunkId);
            visited.Add(chunk.ChunkId);
            result.Add(chunk);
        }

        Visit(root);
        return result;
    }

    private static bool IsDependencyApplicable(
        CompiledRuleChunk chunk,
        CampaignSchemaRoute route,
        RuleContextRequest request,
        IReadOnlySet<string> modules)
    {
        var metadata = chunk.Metadata;
        if (metadata.Layer == RuleLayer.World && !Matches(metadata.WorldModelIds, route.WorldModelId))
        {
            return false;
        }

        if (metadata.Layer == RuleLayer.OptionalModule && !metadata.ModuleIds.Any(modules.Contains))
        {
            return false;
        }

        return Matches(metadata.WorldModelIds, route.WorldModelId) &&
            Matches(metadata.CampaignModes, request.CampaignMode) &&
            Matches(metadata.Operations, request.Operation);
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
