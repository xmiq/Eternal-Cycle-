using System.Text.Json.Serialization;

namespace EternalCycle.Rules;

public sealed record CompiledRulePacketSection(string RuleSourceId, string Content);

// The existing compact packet envelope is campaign-bound. This artifact-scoped
// view preserves its format-1 grouped text without inventing a campaign binding.
public sealed class CompiledRulePacket
{
    internal CompiledRulePacket(CompiledRuleStoreScope scope, CompiledRulesRulesetIdentity identity,
        int estimatedTokens, int maximumEstimatedTokens, IReadOnlyList<CompiledRulePacketSection> rules)
    {
        Scope = scope;
        RepositoryVersion = identity.RepositoryVersion;
        SourceIdentity = identity.Source;
        EstimatedTokens = estimatedTokens;
        MaximumEstimatedTokens = maximumEstimatedTokens;
        Rules = rules;
    }

    public int PacketFormatVersion => 1;
    public CompiledRuleStoreScope Scope { get; }
    public string RepositoryVersion { get; }
    public CompiledRulesSourceIdentity SourceIdentity { get; }
    public int EstimatedTokens { get; }
    public int MaximumEstimatedTokens { get; }
    public IReadOnlyList<CompiledRulePacketSection> Rules { get; }
    public override string ToString() => nameof(CompiledRulePacket);
}

public sealed record CompiledRulePacketTotals(int RequestedEstimatedTokens, int UsedEstimatedTokens,
    int RemainingEstimatedTokens, int RequiredEstimatedTokens, int SelectedRoots, int UniqueMembers,
    int DependencyOnlyMembers, int BudgetExcludedRoots);

public sealed record CompiledRulePacketDecision(RankedCompiledRuleCandidate Root, bool Required,
    bool Selected, long IncrementalEstimatedTokens, int RemainingBeforeAdmission)
{
    public string? ExclusionCode => Selected ? null : "RULE_RETRIEVAL_CLOSURE_EXCEEDS_REMAINING_BUDGET";
}

public sealed class CompiledRulePacketMember
{
    internal CompiledRulePacketMember(CompiledRuleClosureMember member, IReadOnlyList<string> requiredByRuleSourceIds)
    {
        Member = member;
        RequiredByRuleSourceIds = requiredByRuleSourceIds;
    }

    [JsonIgnore] public CompiledRuleClosureMember Member { get; }
    public string SnippetId => Member.SnippetId;
    public string RuleSourceId => Member.RuleSourceId;
    public RankedCompiledRuleCandidate? Root => Member.Root;
    public bool IsDependency => RequiredByRuleSourceIds.Count != 0;
    public IReadOnlyList<string> RequiredByRuleSourceIds { get; }
    public override string ToString() => nameof(CompiledRulePacketMember);
}

public sealed class CompiledRulePacketResult
{
    internal CompiledRulePacketResult(CompiledRuleDependencyClosure input, CompiledRulePacket packet,
        CompiledRulePacketTotals totals, IReadOnlyList<CompiledRulePacketDecision> decisions,
        IReadOnlyList<CompiledRulePacketMember> members)
    {
        Input = input;
        Packet = packet;
        Totals = totals;
        Decisions = decisions;
        Members = members;
    }

    [JsonIgnore] public CompiledRuleDependencyClosure Input { get; }
    public CompiledRulePacket Packet { get; }
    public CompiledRulePacketTotals Totals { get; }
    public IReadOnlyList<CompiledRulePacketDecision> Decisions { get; }
    public IReadOnlyList<CompiledRulePacketMember> Members { get; }
    public override string ToString() => nameof(CompiledRulePacketResult);
}

public static class CompiledRulePackets
{
    private const CompiledRuleCandidateReason RequiredReasons = CompiledRuleCandidateReason.RuntimeKernel |
        CompiledRuleCandidateReason.GmRuntimeProcedure | CompiledRuleCandidateReason.RequiredRuleSource |
        CompiledRuleCandidateReason.RequiredSnippet | CompiledRuleCandidateReason.AlwaysInclude;

    public static CompiledRulePacketResult Build(CompiledRuleDependencyClosure input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateMembership(input, cancellationToken);
        var artifact = input.Input.Input.Artifact!;
        var budget = input.Input.Input.Request.MaximumEstimatedTokens;
        var selected = new HashSet<string>(StringComparer.Ordinal);
        var decisions = new CompiledRulePacketDecision[input.Roots.Count];
        var used = 0;

        // Required closure is reserved before optional relevance admission. A
        // positive match must never crowd the zero-score Kernel/procedure out.
        Admit(required: true);
        var requiredCost = used;
        Admit(required: false);

        var selectedSources = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in input.Members)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (selected.Contains(member.SnippetId)) selectedSources.Add(member.RuleSourceId);
        }
        var members = new List<CompiledRulePacketMember>(selected.Count);
        var sections = new List<CompiledRulePacketSection>();
        var contents = new List<string>();
        var parentsBySource = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        string? sourceId = null;
        foreach (var member in input.Members)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!selected.Contains(member.SnippetId)) continue;
            // D also knows about budget-excluded parents. Retain only actual
            // packet support; the full pre-budget evidence remains in Input.
            if (!parentsBySource.TryGetValue(member.RuleSourceId, out var parents))
            {
                var support = new List<string>();
                foreach (var parent in member.RequiredByRuleSourceIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (selectedSources.Contains(parent)) support.Add(parent);
                }
                parents = support.AsReadOnly();
                parentsBySource.Add(member.RuleSourceId, parents);
            }
            members.Add(new(member, parents));
            if (sourceId is not null && sourceId != member.RuleSourceId)
            {
                sections.Add(new(sourceId, string.Join("\n\n", contents)));
                contents.Clear();
            }
            sourceId = member.RuleSourceId;
            contents.Add(member.Snippet.Content);
        }
        if (sourceId is not null) sections.Add(new(sourceId, string.Join("\n\n", contents)));
        cancellationToken.ThrowIfCancellationRequested();
        var selectedRoots = decisions.Count(decision => decision.Selected);
        var totals = new CompiledRulePacketTotals(budget, used, budget - used, requiredCost,
            selectedRoots, members.Count, members.Count(member => member.Root is null), decisions.Length - selectedRoots);
        return new(input, new(input.Scope, artifact.Ruleset, used, budget, sections.AsReadOnly()),
            totals, Array.AsReadOnly(decisions), members.AsReadOnly());

        void Admit(bool required)
        {
            for (var index = 0; index < input.Roots.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var closure = input.Roots[index];
                var root = closure.Root.Root!;
                if (((root.Reasons & RequiredReasons) != 0) != required) continue;
                // Int32 list lengths and positive Int32 estimates fit Int64,
                // even when their sum cannot fit Int32. Only affordable sums
                // are narrowed back to the request's bounded integer budget.
                long cost = selected.Contains(closure.Root.SnippetId) ? 0 : closure.Root.Snippet.EstimatedTokens;
                foreach (var prerequisite in closure.Prerequisites)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!selected.Contains(prerequisite.SnippetId)) cost = checked(cost + prerequisite.Snippet.EstimatedTokens);
                }
                var remaining = budget - used;
                var fits = cost <= remaining;
                if (!fits && required) throw new CompiledRuleRetrievalException(CompiledRuleRetrievalFailure.PacketBudgetInsufficient);
                decisions[index] = new(root, required, fits, cost, remaining);
                if (!fits) continue;
                // Admission commits only after the whole unique closure fits.
                selected.Add(closure.Root.SnippetId);
                foreach (var prerequisite in closure.Prerequisites)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    selected.Add(prerequisite.SnippetId);
                }
                used = checked(used + (int)cost);
            }
        }
    }

    private static void ValidateMembership(CompiledRuleDependencyClosure input, CancellationToken cancellationToken)
    {
        if (input?.Input?.Input?.Artifact is not { } artifact || input.Input.Input.Request is not { } request ||
            input.Roots is null || input.Members is null || input.Input.Candidates is null ||
            artifact.Ruleset is null || artifact.Integrity is null || artifact.RuleSources is null || artifact.Snippets is null)
            throw Inconsistent();
        if (input.Scope != new CompiledRuleStoreScope(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256))
            throw new CompiledRuleRetrievalException(CompiledRuleRetrievalFailure.RequestInvalid);
        if (request.MaximumEstimatedTokens is < 1 or > CompiledRuleRetrieval.MaximumEstimatedTokens ||
            input.Roots.Count != input.Input.Candidates.Count) throw Inconsistent();

        var sources = new Dictionary<string, CompiledRulesArtifactRuleSource>(StringComparer.Ordinal);
        var snippets = new Dictionary<string, CompiledRulesArtifactSnippet>(StringComparer.Ordinal);
        var snippetsBySource = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var source in artifact.RuleSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (source is null || string.IsNullOrEmpty(source.RuleSourceId) || source.DependencyRuleSourceIds is null ||
                !sources.TryAdd(source.RuleSourceId, source)) throw Inconsistent();
            snippetsBySource.Add(source.RuleSourceId, []);
        }
        foreach (var snippet in artifact.Snippets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (snippet is null || string.IsNullOrEmpty(snippet.SnippetId) || snippet.RuleSourceId is null || !snippets.TryAdd(snippet.SnippetId, snippet) ||
                !snippetsBySource.TryGetValue(snippet.RuleSourceId, out var owned)) throw Inconsistent();
            owned.Add(snippet.SnippetId);
        }
        var members = new Dictionary<string, (CompiledRuleClosureMember Member, int Position)>(StringComparer.Ordinal);
        var roots = new Dictionary<string, RankedCompiledRuleCandidate>(StringComparer.Ordinal);
        foreach (var root in input.Input.Candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (root?.Candidate?.Snippet is not { } snippet || root.Candidate.Source is not { } source ||
                string.IsNullOrEmpty(root.SnippetId) || string.IsNullOrEmpty(source.RuleSourceId) ||
                !snippets.TryGetValue(root.SnippetId, out var local) || !ReferenceEquals(local, snippet) ||
                !sources.TryGetValue(source.RuleSourceId, out var owner) || !ReferenceEquals(owner, source) ||
                !roots.TryAdd(root.SnippetId, root)) throw Inconsistent();
        }
        var completedSources = new HashSet<string>(StringComparer.Ordinal);
        string? lastSource = null;
        string? lastSnippet = null;
        foreach (var member in input.Members)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (member?.Snippet is not { EstimatedTokens: > 0 } snippet || string.IsNullOrWhiteSpace(snippet.Content) ||
                member.Source is not { } source || string.IsNullOrEmpty(member.SnippetId) || string.IsNullOrEmpty(member.RuleSourceId) ||
                !snippets.TryGetValue(member.SnippetId, out var local) ||
                !ReferenceEquals(local, snippet) || !sources.TryGetValue(member.RuleSourceId, out var owner) ||
                !ReferenceEquals(owner, source) || snippet.RuleSourceId != source.RuleSourceId ||
                member.RequiredByRuleSourceIds is null || !members.TryAdd(member.SnippetId, (member, members.Count))) throw Inconsistent();
            roots.TryGetValue(member.SnippetId, out var root);
            if (!ReferenceEquals(member.Root, root)) throw Inconsistent();
            if (lastSource != member.RuleSourceId)
            {
                if (!completedSources.Add(member.RuleSourceId)) throw Inconsistent();
                foreach (var dependency in source.DependencyRuleSourceIds)
                    if (!completedSources.Contains(dependency)) throw Inconsistent();
                lastSnippet = null;
            }
            if (lastSnippet is not null && string.CompareOrdinal(lastSnippet, member.SnippetId) >= 0) throw Inconsistent();
            lastSource = member.RuleSourceId;
            lastSnippet = member.SnippetId;
            string? previousParent = null;
            foreach (var parent in member.RequiredByRuleSourceIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(parent) || !sources.TryGetValue(parent, out var requiring) || !requiring.DependencyRuleSourceIds.Contains(member.RuleSourceId) ||
                    previousParent is not null && string.CompareOrdinal(previousParent, parent) >= 0) throw Inconsistent();
                previousParent = parent;
            }
        }

        var expectedParents = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var id in completedSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var dependency in sources[id].DependencyRuleSourceIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!expectedParents.TryGetValue(dependency, out var parents)) expectedParents.Add(dependency, parents = new(StringComparer.Ordinal));
                parents.Add(id);
            }
        }
        foreach (var entry in members.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var member = entry.Member;
            if (expectedParents.TryGetValue(member.RuleSourceId, out var parents)
                ? !parents.SetEquals(member.RequiredByRuleSourceIds)
                : member.RequiredByRuleSourceIds.Count != 0) throw Inconsistent();
        }

        var union = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < input.Roots.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var closure = input.Roots[index];
            if (closure?.Root is null || closure.Prerequisites is null ||
                !ReferenceEquals(closure.Root.Root, input.Input.Candidates[index]) ||
                !members.TryGetValue(closure.Root.SnippetId, out var rootEntry) || !ReferenceEquals(rootEntry.Member, closure.Root)) throw Inconsistent();
            union.Add(closure.Root.SnippetId);
            var group = new HashSet<string>(StringComparer.Ordinal) { closure.Root.SnippetId };
            var groupSources = new HashSet<string>(StringComparer.Ordinal) { closure.Root.RuleSourceId };
            var previousPosition = -1;
            foreach (var prerequisite in closure.Prerequisites)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (prerequisite is null || !members.TryGetValue(prerequisite.SnippetId, out var entry) ||
                    !ReferenceEquals(entry.Member, prerequisite) || !group.Add(prerequisite.SnippetId) ||
                    entry.Position <= previousPosition || entry.Position >= rootEntry.Position ||
                    prerequisite.RuleSourceId == closure.Root.RuleSourceId) throw Inconsistent();
                previousPosition = entry.Position;
                groupSources.Add(prerequisite.RuleSourceId);
                union.Add(prerequisite.SnippetId);
            }
            var supportedSources = new HashSet<string>(StringComparer.Ordinal);
            foreach (var prerequisite in closure.Prerequisites)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var parent in prerequisite.RequiredByRuleSourceIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (groupSources.Contains(parent)) supportedSources.Add(prerequisite.RuleSourceId);
                }
            }
            // Check D's supplied membership locally, not a second traversal or
            // closure computation. Every dependency source must be complete;
            // every prerequisite must have support inside this acyclic group.
            foreach (var id in groupSources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var dependency in sources[id].DependencyRuleSourceIds)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!groupSources.Contains(dependency) || snippetsBySource[dependency].Count == 0) throw Inconsistent();
                    foreach (var snippetId in snippetsBySource[dependency])
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!group.Contains(snippetId)) throw Inconsistent();
                    }
                }
                if (id != closure.Root.RuleSourceId && !supportedSources.Contains(id)) throw Inconsistent();
            }
        }
        if (union.Count != members.Count) throw Inconsistent();
    }

    private static CompiledRuleRetrievalException Inconsistent() => new(CompiledRuleRetrievalFailure.ArtifactInconsistent);
}
