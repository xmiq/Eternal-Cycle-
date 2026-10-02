using System.Text.Json.Serialization;

namespace EternalCycle.Rules;

[Flags]
public enum CompiledRuleCandidateReason
{
    None = 0,
    ControlledVocabulary = 1,
    RequiredRuleSource = 2,
    RequiredSnippet = 4,
    RuntimeKernel = 8,
    GmRuntimeProcedure = 16,
    AlwaysInclude = 32
}

public sealed class CompiledRuleCandidate
{
    internal CompiledRuleCandidate(CompiledRulesArtifactRuleSource source, CompiledRulesArtifactSnippet snippet,
        CompiledRuleCandidateReason reasons, IReadOnlyList<CompiledRulesRetrievalTerm> matches)
    {
        Source = source;
        Snippet = snippet;
        Reasons = reasons;
        Matches = matches;
    }

    [JsonIgnore] public CompiledRulesArtifactRuleSource Source { get; }
    [JsonIgnore] public CompiledRulesArtifactSnippet Snippet { get; }
    public string SnippetId => Snippet.SnippetId;
    public CompiledRuleCandidateReason Reasons { get; }
    public IReadOnlyList<CompiledRulesRetrievalTerm> Matches { get; }
    public bool ApplicabilitySatisfied => true;
    public override string ToString() => nameof(CompiledRuleCandidate);
}

public sealed record CompiledRuleCandidateCounts(
    int ArtifactSnippets, int VocabularyMatched, int ApplicableVocabularyMatched,
    int InapplicableVocabularyMatched, int VocabularyUnmatched);

public sealed class CompiledRuleCandidateSet
{
    internal CompiledRuleCandidateSet(PreparedCompiledRuleRetrievalRequest request,
        IReadOnlyList<CompiledRuleCandidate> candidates, CompiledRuleCandidateCounts counts)
    {
        Request = request;
        Candidates = candidates;
        Counts = counts;
    }

    [JsonIgnore] public PreparedCompiledRuleRetrievalRequest Request { get; }
    public CompiledRuleStoreScope Scope => Request.Scope;
    public IReadOnlyList<CompiledRuleCandidate> Candidates { get; }
    public CompiledRuleCandidateCounts Counts { get; }
    public override string ToString() => nameof(CompiledRuleCandidateSet);
}

public sealed class CompiledRuleCandidateIndex
{
    private const string GmProcedureSourceId = "gm-runtime-procedure";
    private readonly CompiledRulesArtifact artifact;
    private readonly Dictionary<string, CompiledRulesArtifactRuleSource> sources;
    private readonly Dictionary<string, int> snippetIndices;
    private readonly Dictionary<string, List<TermHit>> terms = new(StringComparer.Ordinal);

    private readonly record struct TermHit(int SnippetIndex, CompiledRulesRetrievalTerm Term);

    private CompiledRuleCandidateIndex(CompiledRulesArtifact artifact, CancellationToken cancellationToken)
    {
        this.artifact = artifact;
        Scope = new(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256);
        sources = artifact.RuleSources.ToDictionary(source => source.RuleSourceId, StringComparer.Ordinal);
        snippetIndices = new(StringComparer.Ordinal);
        for (var index = 0; index < artifact.Snippets.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snippet = artifact.Snippets[index];
            snippetIndices.Add(snippet.SnippetId, index);
            foreach (var term in snippet.Retrieval.Terms)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!terms.TryGetValue(term.Term, out var hits)) terms.Add(term.Term, hits = []);
                hits.Add(new(index, term));
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    public CompiledRuleStoreScope Scope { get; }

    public static CompiledRuleCandidateIndex Create(CompiledRulesArtifact artifact, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (artifact is null) throw Inconsistent();
        // Validate once at the artifact boundary, not on every query. The same
        // deep read-only copy used by approval prevents aliases changing matches.
        CompiledRulesArtifact frozen;
        try
        {
            frozen = ValidatedCompiledRulesArtifact.Freeze(artifact);
            cancellationToken.ThrowIfCancellationRequested();
            if (CompiledRulesArtifactContract.Validate(frozen).Count != 0) throw Inconsistent();
        }
        catch (ArgumentException) { cancellationToken.ThrowIfCancellationRequested(); throw Inconsistent(); }
        catch (NullReferenceException) { cancellationToken.ThrowIfCancellationRequested(); throw Inconsistent(); }
        cancellationToken.ThrowIfCancellationRequested();
        return new(frozen, cancellationToken);
    }

    public CompiledRuleCandidateSet FindCandidates(PreparedCompiledRuleRetrievalRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request is null || request.Scope != Scope) throw InvalidRequest();
        var modules = request.ModuleIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var topics = request.Topics.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var applicable = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in artifact.RuleSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsApplicable(source.Applicability, request, modules, topics)) applicable.Add(source.RuleSourceId);
        }

        var requiredSources = request.RequiredRuleSourceIds.ToHashSet(StringComparer.Ordinal);
        var requiredSnippets = request.RequiredSnippetIds.ToHashSet(StringComparer.Ordinal);
        foreach (var id in requiredSources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!applicable.Contains(id) || !artifact.Snippets.Any(snippet => snippet.RuleSourceId == id)) throw InvalidRequest();
        }
        foreach (var id in requiredSnippets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!snippetIndices.TryGetValue(id, out var index) || !applicable.Contains(artifact.Snippets[index].RuleSourceId))
                throw InvalidRequest();
        }

        // Format-1 validity is not runtime readiness. Required runtime structure
        // must exist and be applicable; neither a query nor an import grants it.
        if (!artifact.Snippets.Any(snippet => applicable.Contains(snippet.RuleSourceId) &&
            sources[snippet.RuleSourceId].Applicability.Layer == RuleLayer.RuntimeKernel)) throw Inconsistent();
        var gameplay = string.Equals(request.Operation, "gameplay.resolve", StringComparison.OrdinalIgnoreCase);
        if (gameplay && (!sources.TryGetValue(GmProcedureSourceId, out var procedure) ||
            !procedure.Applicability.AlwaysInclude || !applicable.Contains(GmProcedureSourceId) ||
            !artifact.Snippets.Any(snippet => snippet.RuleSourceId == GmProcedureSourceId))) throw Inconsistent();

        var matches = new List<CompiledRulesRetrievalTerm>?[artifact.Snippets.Count];
        // Prepared terms and artifact terms are already ordinally ordered. Lookup
        // keeps whole phrases intact and preserves all distinct concept matches.
        foreach (var query in request.QueryTerms)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!terms.TryGetValue(query, out var hits)) continue;
            foreach (var hit in hits)
            {
                cancellationToken.ThrowIfCancellationRequested();
                (matches[hit.SnippetIndex] ??= []).Add(hit.Term);
            }
        }

        var result = new List<CompiledRuleCandidate>();
        var matched = 0;
        var excluded = 0;
        for (var index = 0; index < artifact.Snippets.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snippet = artifact.Snippets[index];
            var evidence = matches[index];
            if (evidence is not null) matched++;
            if (!applicable.Contains(snippet.RuleSourceId))
            {
                if (evidence is not null) excluded++;
                continue;
            }
            var source = sources[snippet.RuleSourceId];
            var reasons = CompiledRuleCandidateReason.None;
            if (evidence is not null) reasons |= CompiledRuleCandidateReason.ControlledVocabulary;
            if (requiredSources.Contains(snippet.RuleSourceId)) reasons |= CompiledRuleCandidateReason.RequiredRuleSource;
            if (requiredSnippets.Contains(snippet.SnippetId)) reasons |= CompiledRuleCandidateReason.RequiredSnippet;
            if (source.Applicability.Layer == RuleLayer.RuntimeKernel) reasons |= CompiledRuleCandidateReason.RuntimeKernel;
            if (gameplay && snippet.RuleSourceId == GmProcedureSourceId) reasons |= CompiledRuleCandidateReason.GmRuntimeProcedure;
            if (source.Applicability.AlwaysInclude) reasons |= CompiledRuleCandidateReason.AlwaysInclude;
            if (reasons != CompiledRuleCandidateReason.None) result.Add(new(source, snippet, reasons,
                evidence is null ? Array.Empty<CompiledRulesRetrievalTerm>() : evidence.AsReadOnly()));
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(request, result.AsReadOnly(), new(artifact.Snippets.Count, matched,
            matched - excluded, excluded, artifact.Snippets.Count - matched));
    }

    private static bool IsApplicable(CompiledRulesArtifactApplicability metadata,
        PreparedCompiledRuleRetrievalRequest request, IReadOnlySet<string> modules, IReadOnlySet<string> topics) =>
        (metadata.WorldModelIds.Count == 0 || request.WorldModelId is not null && Matches(metadata.WorldModelIds, request.WorldModelId)) &&
        (metadata.ModuleIds.Count == 0 || modules.Count > 0 && MatchesSet(metadata.ModuleIds, modules)) &&
        Matches(metadata.CampaignModes, request.CampaignMode) &&
        Matches(metadata.Operations, request.Operation) &&
        MatchesSet(metadata.Topics, topics);

    private static bool Matches(IList<string> selectors, string value) =>
        selectors.Count == 0 || selectors.Contains("*", StringComparer.Ordinal) || selectors.Contains(value, StringComparer.OrdinalIgnoreCase);

    // A wildcard allows any concrete selection, not permission to enable a
    // module implicitly. Topics keep their existing unrestricted '*' meaning.
    private static bool MatchesSet(IList<string> selectors, IReadOnlySet<string> values) =>
        selectors.Count == 0 || selectors.Contains("*", StringComparer.Ordinal) || selectors.Any(values.Contains);

    private static CompiledRuleRetrievalException InvalidRequest() => new(CompiledRuleRetrievalFailure.RequestInvalid);
    private static CompiledRuleRetrievalException Inconsistent() => new(CompiledRuleRetrievalFailure.ArtifactInconsistent);
    public override string ToString() => nameof(CompiledRuleCandidateIndex);
}
