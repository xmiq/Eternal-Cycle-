using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EternalCycle.Rules;

public sealed record RuleSnippetApplicability(
    RuleLayer Layer,
    IReadOnlyList<string> WorldModelIds,
    IReadOnlyList<string> ModuleIds,
    IReadOnlyList<string> CampaignModes,
    IReadOnlyList<string> Operations,
    IReadOnlyList<string> Topics,
    int Priority,
    bool AlwaysInclude,
    RulePreparationTier PreparationTier);

public sealed record RuleSnippetCandidate(
    string SnippetId,
    string RuleSourceId,
    string SourcePath,
    string SourceSha256,
    string? SourceAnchor,
    string Content,
    string ContentSha256,
    int EstimatedTokens,
    RuleSnippetApplicability Applicability,
    IReadOnlyList<string> DependencyRuleSourceIds,
    int SourceOrder,
    int SectionOrder);

public sealed record RuleSnippetSection(
    string? SourceAnchor,
    string Content);

public sealed class RuleSnippetCompilationException : Exception
{
    public RuleSnippetCompilationException(string code, string ruleSourceId, string message)
        : base(message)
    {
        Code = code;
        RuleSourceId = ruleSourceId;
    }

    public string Code { get; }

    public string RuleSourceId { get; }
}

public static class RuleSnippetCompiler
{
    public static IReadOnlyList<RuleSnippetCandidate> Compile(MaterializedRuleSourceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.Sources.Count == 0)
        {
            Fail("RULE_SOURCES_REQUIRED", string.Empty, "At least one materialized Rule Source is required.");
        }

        var candidates = new List<RuleSnippetCandidate>();
        var sourceIds = new HashSet<string>(StringComparer.Ordinal);
        var snippetIds = new HashSet<string>(StringComparer.Ordinal);
        for (var sourceOrder = 0; sourceOrder < snapshot.Sources.Count; sourceOrder++)
        {
            var source = snapshot.Sources[sourceOrder];
            var entry = source.ManifestEntry;
            if (!sourceIds.Add(entry.RuleSourceId))
            {
                Fail("RULE_SOURCE_ID_DUPLICATE", entry.RuleSourceId, "Materialized Rule Source IDs must be unique.");
            }

            var sections = RuleSnippetText.Split(source.DecodedText);
            if (sections.Count == 0)
            {
                Fail("RULE_SOURCE_CONTENT_EMPTY", entry.RuleSourceId, "A Rule Source must produce at least one non-empty snippet.");
            }

            var applicability = new RuleSnippetApplicability(
                entry.Layer,
                [.. entry.WorldModelIds],
                [.. entry.ModuleIds],
                [.. entry.CampaignModes],
                [.. entry.Operations],
                [.. entry.Topics],
                entry.Priority,
                entry.AlwaysInclude,
                entry.PreparationTier);
            var dependencies = entry.Dependencies.ToArray();

            for (var sectionOrder = 0; sectionOrder < sections.Count; sectionOrder++)
            {
                var section = sections[sectionOrder];
                var snippetId = CompiledRulesArtifactContract.CreateSnippetId(
                    entry.RuleSourceId,
                    section.SourceAnchor);
                if (!snippetIds.Add(snippetId))
                {
                    Fail("SNIPPET_ID_DUPLICATE", entry.RuleSourceId, $"Compiled snippet ID '{snippetId}' is not unique.");
                }

                candidates.Add(new(
                    snippetId,
                    entry.RuleSourceId,
                    entry.Path,
                    source.SourceSha256,
                    section.SourceAnchor,
                    section.Content,
                    CompiledRulesArtifactContract.ComputeContentSha256(section.Content),
                    RuleSnippetText.EstimateTokens(section.Content),
                    applicability,
                    dependencies,
                    sourceOrder,
                    sectionOrder));
            }
        }

        return candidates;
    }

    [DoesNotReturn]
    private static void Fail(string code, string ruleSourceId, string message) =>
        throw new RuleSnippetCompilationException(code, ruleSourceId, message);
}

public static class RuleSnippetText
{
    public static IReadOnlyList<RuleSnippetSection> Split(string sourceText)
    {
        ArgumentNullException.ThrowIfNull(sourceText);
        var text = NormalizeLineEndingsAndBom(sourceText);
        var lines = text.Split('\n');
        var sections = new List<RuleSnippetSection>();
        var current = new List<string>();
        var anchors = new AnchorAllocator();
        string? currentAnchor = null;
        var fence = default(FenceState?);

        foreach (var line in lines)
        {
            if (fence is { } activeFence)
            {
                current.Add(line);
                if (IsFenceClose(line, activeFence))
                {
                    fence = null;
                }
                continue;
            }

            if (TryFenceOpen(line, out var openedFence))
            {
                current.Add(line);
                fence = openedFence;
                continue;
            }

            if (!TryHeading(line, out var level, out var heading) || level != 2)
            {
                current.Add(line);
                continue;
            }

            // Eternal Cycle's established subdivision identity is H2-based: H1 is
            // the document title, while H3-H6 remain nested within their H2 section.
            AddSection(sections, currentAnchor, current);
            currentAnchor = anchors.Next(heading);
            current.Add(line);
        }

        AddSection(sections, currentAnchor, current);
        return sections;
    }

    public static int EstimateTokens(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        return Math.Max(1, (Encoding.UTF8.GetByteCount(content) + 2) / 3);
    }

    private static string NormalizeLineEndingsAndBom(string sourceText)
    {
        var start = sourceText.StartsWith('\uFEFF') ? 1 : 0;
        return sourceText[start..]
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
    }

    private static void AddSection(
        ICollection<RuleSnippetSection> sections,
        string? anchor,
        List<string> lines)
    {
        var start = 0;
        while (start < lines.Count && string.IsNullOrWhiteSpace(lines[start]))
        {
            start++;
        }

        var end = lines.Count - 1;
        while (end >= start && string.IsNullOrWhiteSpace(lines[end]))
        {
            end--;
        }

        if (end >= start)
        {
            sections.Add(new(anchor, string.Join('\n', lines.GetRange(start, end - start + 1))));
        }
        lines.Clear();
    }

    private static bool TryHeading(string line, out int level, out string heading)
    {
        level = 0;
        heading = string.Empty;
        var index = CountLeadingSpaces(line);
        if (index > 3 || index >= line.Length || line[index] != '#')
        {
            return false;
        }

        while (index + level < line.Length && line[index + level] == '#')
        {
            level++;
        }
        if (level is < 1 or > 6)
        {
            return false;
        }

        var contentIndex = index + level;
        if (contentIndex < line.Length && line[contentIndex] is not (' ' or '\t'))
        {
            return false;
        }

        heading = StripClosingHashes(line[contentIndex..].Trim());
        return true;
    }

    private static string StripClosingHashes(string heading)
    {
        var hashStart = heading.Length;
        while (hashStart > 0 && heading[hashStart - 1] == '#')
        {
            hashStart--;
        }
        return hashStart > 0 && hashStart < heading.Length && char.IsWhiteSpace(heading[hashStart - 1])
            ? heading[..(hashStart - 1)].TrimEnd()
            : heading;
    }

    private static bool TryFenceOpen(string line, out FenceState fence)
    {
        fence = default;
        var index = CountLeadingSpaces(line);
        if (index > 3 || index >= line.Length || line[index] is not ('`' or '~'))
        {
            return false;
        }

        var marker = line[index];
        var length = 0;
        while (index + length < line.Length && line[index + length] == marker)
        {
            length++;
        }
        if (length < 3)
        {
            return false;
        }

        fence = new(marker, length);
        return true;
    }

    private static bool IsFenceClose(string line, FenceState fence)
    {
        var index = CountLeadingSpaces(line);
        if (index > 3 || index >= line.Length || line[index] != fence.Marker)
        {
            return false;
        }

        var length = 0;
        while (index + length < line.Length && line[index + length] == fence.Marker)
        {
            length++;
        }
        return length >= fence.Length && line[(index + length)..].All(char.IsWhiteSpace);
    }

    private static int CountLeadingSpaces(string line)
    {
        var count = 0;
        while (count < line.Length && line[count] == ' ')
        {
            count++;
        }
        return count;
    }

    private readonly record struct FenceState(char Marker, int Length);

    private sealed class AnchorAllocator
    {
        private const int MaximumAnchorLength = 128;
        private readonly HashSet<string> used = new(StringComparer.Ordinal);
        private readonly Dictionary<string, int> nextSuffix = new(StringComparer.Ordinal);

        public string Next(string heading)
        {
            var baseAnchor = CreateBaseAnchor(heading);
            if (used.Add(baseAnchor))
            {
                nextSuffix[baseAnchor] = 1;
                return baseAnchor;
            }

            var suffixNumber = nextSuffix.TryGetValue(baseAnchor, out var next) ? next : 1;
            while (true)
            {
                var suffix = $"-{suffixNumber++}";
                var prefixLength = Math.Min(baseAnchor.Length, MaximumAnchorLength - suffix.Length);
                var prefix = baseAnchor[..prefixLength].TrimEnd('-');
                var candidate = $"{prefix}{suffix}";
                if (used.Add(candidate))
                {
                    nextSuffix[baseAnchor] = suffixNumber;
                    return candidate;
                }
            }
        }

        private static string CreateBaseAnchor(string heading)
        {
            var normalized = new string(heading
                .Normalize(NormalizationForm.FormC)
                .Trim()
                .ToLowerInvariant()
                .Select(character => char.IsLetterOrDigit(character) || character is ' ' or '-'
                    ? character
                    : '\0')
                .Where(character => character != '\0')
                .ToArray());
            var anchor = string.Join('-', normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Trim('-');
            if (anchor.Length == 0)
            {
                return "section";
            }
            return anchor.Length <= MaximumAnchorLength
                ? anchor
                : anchor[..MaximumAnchorLength].TrimEnd('-');
        }
    }
}
