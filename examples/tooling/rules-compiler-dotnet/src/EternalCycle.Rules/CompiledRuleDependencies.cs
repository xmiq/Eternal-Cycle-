using System.Text.Json.Serialization;

namespace EternalCycle.Rules;

public sealed class CompiledRuleClosureMember
{
    internal CompiledRuleClosureMember(CompiledRulesArtifactRuleSource source, CompiledRulesArtifactSnippet snippet,
        RankedCompiledRuleCandidate? root, IReadOnlyList<string> requiredByRuleSourceIds)
    {
        Source = source;
        Snippet = snippet;
        Root = root;
        RequiredByRuleSourceIds = requiredByRuleSourceIds;
    }

    [JsonIgnore] public CompiledRulesArtifactRuleSource Source { get; }
    [JsonIgnore] public CompiledRulesArtifactSnippet Snippet { get; }
    [JsonIgnore] public RankedCompiledRuleCandidate? Root { get; }
    public string RuleSourceId => Source.RuleSourceId;
    public string SnippetId => Snippet.SnippetId;
    public bool IsRoot => Root is not null;
    public bool IsDependency => RequiredByRuleSourceIds.Count != 0;
    public long VocabularyScore => Root?.VocabularyScore ?? 0;
    public CompiledRuleCandidateReason Reasons => Root?.Reasons ?? CompiledRuleCandidateReason.None;
    public IReadOnlyList<string> RequiredByRuleSourceIds { get; }
    public override string ToString() => nameof(CompiledRuleClosureMember);
}

public sealed class CompiledRuleRootClosure
{
    internal CompiledRuleRootClosure(CompiledRuleClosureMember root, IReadOnlyList<CompiledRuleClosureMember> prerequisites)
    {
        Root = root;
        Prerequisites = prerequisites;
    }

    public CompiledRuleClosureMember Root { get; }
    public IReadOnlyList<CompiledRuleClosureMember> Prerequisites { get; }
    public override string ToString() => nameof(CompiledRuleRootClosure);
}

public sealed class CompiledRuleDependencyClosure
{
    internal CompiledRuleDependencyClosure(RankedCompiledRuleCandidateSet input,
        IReadOnlyList<CompiledRuleRootClosure> roots, IReadOnlyList<CompiledRuleClosureMember> members)
    {
        Input = input;
        Roots = roots;
        Members = members;
    }

    [JsonIgnore] public RankedCompiledRuleCandidateSet Input { get; }
    public CompiledRuleStoreScope Scope => Input.Scope;
    public IReadOnlyList<CompiledRuleRootClosure> Roots { get; }
    public IReadOnlyList<CompiledRuleClosureMember> Members { get; }
    public override string ToString() => nameof(CompiledRuleDependencyClosure);
}

public static class CompiledRuleDependencies
{
    public static CompiledRuleDependencyClosure Expand(RankedCompiledRuleCandidateSet input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (input?.Input?.Artifact is not { } artifact) throw Inconsistent();
        return Expand(artifact, input, cancellationToken);
    }

    // Internal entry supports defensive invariant tests without a public bypass
    // of B's artifact validation/freeze. Production always uses that snapshot.
    internal static CompiledRuleDependencyClosure Expand(CompiledRulesArtifact artifact,
        RankedCompiledRuleCandidateSet input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (artifact?.Ruleset is null || artifact.Integrity is null || artifact.RuleSources is null || artifact.Snippets is null ||
            input?.Input?.Request is null ||
            input.Candidates is null) throw Inconsistent();
        if (input.Scope != new CompiledRuleStoreScope(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256))
            throw new CompiledRuleRetrievalException(CompiledRuleRetrievalFailure.RequestInvalid);

        var sources = new Dictionary<string, CompiledRulesArtifactRuleSource>(StringComparer.Ordinal);
        var snippets = new Dictionary<string, CompiledRulesArtifactSnippet>(StringComparer.Ordinal);
        var sourceSnippets = new Dictionary<string, List<CompiledRulesArtifactSnippet>>(StringComparer.Ordinal);
        foreach (var source in artifact.RuleSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (source is null || string.IsNullOrEmpty(source.RuleSourceId) || source.Applicability is null ||
                source.DependencyRuleSourceIds is null || !sources.TryAdd(source.RuleSourceId, source)) throw Inconsistent();
            sourceSnippets.Add(source.RuleSourceId, []);
        }
        foreach (var snippet in artifact.Snippets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (snippet is null || string.IsNullOrEmpty(snippet.SnippetId) || snippet.RuleSourceId is null ||
                !sourceSnippets.TryGetValue(snippet.RuleSourceId, out var owned) || !snippets.TryAdd(snippet.SnippetId, snippet))
                throw Inconsistent();
            owned.Add(snippet);
        }
        foreach (var owned in sourceSnippets.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            owned.Sort(static (left, right) => string.CompareOrdinal(left.SnippetId, right.SnippetId));
        }

        var roots = new Dictionary<string, RankedCompiledRuleCandidate>(StringComparer.Ordinal);
        foreach (var root in input.Candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Object identity proves these roots came from this exact frozen
            // authority, rather than trusting a claimed digest or matching ID.
            if (root?.Candidate?.Source is not { } source || root.Candidate.Snippet is not { } snippet ||
                !sources.TryGetValue(source.RuleSourceId, out var localSource) || !ReferenceEquals(source, localSource) ||
                !snippets.TryGetValue(root.SnippetId, out var localSnippet) || !ReferenceEquals(snippet, localSnippet) ||
                snippet.RuleSourceId != source.RuleSourceId || !roots.TryAdd(root.SnippetId, root)) throw Inconsistent();
        }

        var modules = input.Input.Request.ModuleIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var parents = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        var state = new Dictionary<string, byte>(StringComparer.Ordinal);
        var dependencies = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var orderedSources = new List<string>();
        foreach (var root in input.Candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rootId = root.Candidate.Source.RuleSourceId;
            if (state.ContainsKey(rootId)) continue;
            Enter(rootId);
            var stack = new Stack<(string SourceId, int Next)>();
            stack.Push((rootId, 0));
            while (stack.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var (id, next) = stack.Pop();
                var required = dependencies[id];
                if (next == required.Length)
                {
                    state[id] = 2;
                    orderedSources.Add(id);
                    continue;
                }
                stack.Push((id, next + 1));
                var dependency = required[next];
                if (!sources.TryGetValue(dependency, out var source) || sourceSnippets[dependency].Count == 0 ||
                    !CompiledRuleCandidateIndex.IsDependencyCompatible(source.Applicability, input.Input.Request, modules))
                    throw DependencyFailed();
                if (!parents.TryGetValue(dependency, out var requiring)) parents.Add(dependency, requiring = new(StringComparer.Ordinal));
                requiring.Add(id);
                if (state.TryGetValue(dependency, out var seen))
                {
                    if (seen == 1) throw DependencyFailed();
                    continue;
                }
                Enter(dependency);
                stack.Push((dependency, 0));
            }
        }

        var members = new List<CompiledRuleClosureMember>();
        var memberById = new Dictionary<string, CompiledRuleClosureMember>(StringComparer.Ordinal);
        var membersBySource = new Dictionary<string, List<CompiledRuleClosureMember>>(StringComparer.Ordinal);
        foreach (var id in orderedSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // One direct-parent set per source bounds explanations by graph
            // edges; never enumerate exponentially many root-to-member paths.
            IReadOnlyList<string> requiring = parents.TryGetValue(id, out var support)
                ? Array.AsReadOnly(support.Order(StringComparer.Ordinal).ToArray()) : Array.Empty<string>();
            var owned = new List<CompiledRuleClosureMember>();
            foreach (var snippet in sourceSnippets[id])
            {
                cancellationToken.ThrowIfCancellationRequested();
                roots.TryGetValue(snippet.SnippetId, out var root);
                if (root is null && requiring.Count == 0) continue;
                var member = new CompiledRuleClosureMember(sources[id], snippet, root, requiring);
                members.Add(member);
                owned.Add(member);
                memberById.Add(snippet.SnippetId, member);
            }
            membersBySource.Add(id, owned);
        }

        // E needs complete prerequisites for each ranked root. Share the list
        // for roots from one source and references to union members, not copies
        // of content or graph paths. Cost/selection remains E's responsibility.
        var prerequisitesBySource = new Dictionary<string, IReadOnlyList<CompiledRuleClosureMember>>(StringComparer.Ordinal);
        var resultRoots = new List<CompiledRuleRootClosure>(input.Candidates.Count);
        foreach (var root in input.Candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = root.Candidate.Source.RuleSourceId;
            if (!prerequisitesBySource.TryGetValue(id, out var prerequisites))
            {
                var reachable = new HashSet<string>(StringComparer.Ordinal);
                var pending = new Stack<string>(dependencies[id]);
                while (pending.TryPop(out var dependency))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!reachable.Add(dependency)) continue;
                    foreach (var child in dependencies[dependency])
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        pending.Push(child);
                    }
                }
                var requiredMembers = new List<CompiledRuleClosureMember>();
                foreach (var sourceId in orderedSources)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (reachable.Contains(sourceId)) requiredMembers.AddRange(membersBySource[sourceId]);
                }
                prerequisites = requiredMembers.AsReadOnly();
                prerequisitesBySource.Add(id, prerequisites);
            }
            resultRoots.Add(new(memberById[root.SnippetId], prerequisites));
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(input, resultRoots.AsReadOnly(), members.AsReadOnly());

        void Enter(string id)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var declared = sources[id].DependencyRuleSourceIds;
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var dependency in declared)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrWhiteSpace(dependency) || !unique.Add(dependency)) throw DependencyFailed();
            }
            dependencies.Add(id, unique.Order(StringComparer.Ordinal).ToArray());
            state.Add(id, 1);
        }
    }

    private static CompiledRuleRetrievalException Inconsistent() => new(CompiledRuleRetrievalFailure.ArtifactInconsistent);
    private static CompiledRuleRetrievalException DependencyFailed() => new(CompiledRuleRetrievalFailure.DependencyFailed);
}
