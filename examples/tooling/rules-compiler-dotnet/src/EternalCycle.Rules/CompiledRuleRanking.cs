using System.Text.Json.Serialization;

namespace EternalCycle.Rules;

public sealed class RankedCompiledRuleCandidate
{
    internal RankedCompiledRuleCandidate(CompiledRuleCandidate candidate, long vocabularyScore)
    {
        Candidate = candidate;
        VocabularyScore = vocabularyScore;
    }

    [JsonIgnore] public CompiledRuleCandidate Candidate { get; }
    public string SnippetId => Candidate.SnippetId;
    public CompiledRuleCandidateReason Reasons => Candidate.Reasons;
    public IReadOnlyList<CompiledRulesRetrievalTerm> Matches => Candidate.Matches;
    public long VocabularyScore { get; }
    public int Priority => Candidate.Source.Applicability.Priority;
    public override string ToString() => nameof(RankedCompiledRuleCandidate);
}

public sealed class RankedCompiledRuleCandidateSet
{
    internal RankedCompiledRuleCandidateSet(CompiledRuleCandidateSet input, IReadOnlyList<RankedCompiledRuleCandidate> candidates)
    {
        Input = input;
        Candidates = candidates;
    }

    [JsonIgnore] public CompiledRuleCandidateSet Input { get; }
    public CompiledRuleStoreScope Scope => Input.Scope;
    public CompiledRuleCandidateCounts Counts => Input.Counts;
    public IReadOnlyList<RankedCompiledRuleCandidate> Candidates { get; }
    public override string ToString() => nameof(RankedCompiledRuleCandidateSet);
}

public static class CompiledRuleRanking
{
    private const CompiledRuleCandidateReason AllReasons = CompiledRuleCandidateReason.ControlledVocabulary |
        CompiledRuleCandidateReason.RequiredRuleSource | CompiledRuleCandidateReason.RequiredSnippet |
        CompiledRuleCandidateReason.RuntimeKernel | CompiledRuleCandidateReason.GmRuntimeProcedure |
        CompiledRuleCandidateReason.AlwaysInclude;

    public static RankedCompiledRuleCandidateSet Rank(CompiledRuleCandidateSet input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (input is null || input.Request is null || input.Candidates is null || input.Counts is null) throw Inconsistent();
        var result = new List<RankedCompiledRuleCandidate>(input.Candidates.Count);
        var identities = new HashSet<string>(StringComparer.Ordinal);
        foreach (var candidate in input.Candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // B owns eligibility and freezes the graph. These local invariants
            // protect scoring, not a second artifact validation or query match.
            if (candidate is null || candidate.Source?.Applicability is not { } metadata || candidate.Snippet is null ||
                string.IsNullOrEmpty(candidate.SnippetId) || !identities.Add(candidate.SnippetId) ||
                candidate.Snippet.RuleSourceId != candidate.Source.RuleSourceId || candidate.Matches is null ||
                metadata.Priority is < -10000 or > 10000 || candidate.Reasons == CompiledRuleCandidateReason.None ||
                (candidate.Reasons & ~AllReasons) != 0 ||
                candidate.Reasons.HasFlag(CompiledRuleCandidateReason.ControlledVocabulary) != (candidate.Matches.Count > 0))
                throw Inconsistent();

            long score = 0;
            CompiledRulesRetrievalTerm? previous = null;
            foreach (var match in candidate.Matches)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (match is null || string.IsNullOrEmpty(match.Term) || string.IsNullOrEmpty(match.Kind) ||
                    match.Weight is < 1 or > 1000) throw Inconsistent();
                if (previous is not null)
                {
                    var termOrder = string.CompareOrdinal(previous.Term, match.Term);
                    // B guarantees strict term/kind order. A duplicated key is
                    // corruption even if its weights agree; different kinds count.
                    if (termOrder > 0 || termOrder == 0 && string.CompareOrdinal(previous.Kind, match.Kind) >= 0)
                        throw Inconsistent();
                }
                // IList's maximum count * 1000 fits Int64, but not Int32. No
                // valid association cap or saturation changes relative relevance.
                try { score = checked(score + match.Weight); }
                catch (OverflowException) { throw Inconsistent(); }
                previous = match;
            }
            result.Add(new(candidate, score));
        }
        cancellationToken.ThrowIfCancellationRequested();
        // Priority only breaks equal vocabulary scores. Inclusion, preparation,
        // layer and dependency cost never manufacture query relevance. Sorting
        // is synchronous; cancellation is checked on both sides, never wrapped
        // inside a framework comparer exception or returned as partial success.
        result.Sort(static (left, right) =>
        {
            var comparison = right.VocabularyScore.CompareTo(left.VocabularyScore);
            if (comparison == 0) comparison = right.Priority.CompareTo(left.Priority);
            return comparison != 0 ? comparison : string.CompareOrdinal(left.SnippetId, right.SnippetId);
        });
        cancellationToken.ThrowIfCancellationRequested();
        return new(input, result.AsReadOnly());
    }

    private static CompiledRuleRetrievalException Inconsistent() => new(CompiledRuleRetrievalFailure.ArtifactInconsistent);
}
