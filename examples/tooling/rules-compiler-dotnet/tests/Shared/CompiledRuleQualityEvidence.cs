using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace EternalCycle.Rules.Testing;

internal sealed record QualityHit(string SnippetId, string Concept, string Term, int Weight = 0);

internal sealed record RetrievalQualityCase
{
    public required string Id { get; init; }
    public required string Category { get; init; }
    public string Operation { get; init; } = "gameplay.resolve";
    public string[] QueryTerms { get; init; } = [];
    public string[] EvidenceIds { get; init; } = [];
    public string[] ExcludedPositiveSources { get; init; } = [];
    public QualityHit[] PositiveHits { get; init; } = [];
    public QualityHit[] NegativeHits { get; init; } = [];
    public string[] RequiredRuleSourceIds { get; init; } = [];
    public string[] RequiredSnippetIds { get; init; } = [];
    public string[] PositiveRequiredIds { get; init; } = [];
    public int Budget { get; init; } = 8000;
    public int? ExpectedCost { get; init; }
    public int? ExpectedRoots { get; init; }
    public int? ExpectedExclusions { get; init; }
    public int? ExpectedInapplicable { get; init; }
    public bool ExpectNoHits { get; init; }
    public CompiledRuleRetrievalFailure? ExpectedFailure { get; init; }
}

internal sealed record RetrievalQualityMeasurement(RetrievalQualityCase Fixture, CompiledRuleRetrievalRequest Request,
    RankedCompiledRuleCandidateSet Ranked, CompiledRulePacketResult? Result, CompiledRuleRetrievalFailure? Failure,
    int PositiveExpectations, int NegativeExpectations, int DependencyExpectations, int RequiredExpectations, object Evidence);

// Test-only evidence: all decisions come from A-E. Assertions observe those
// decisions; neither measurements nor report serialization feed back into them.
internal static class CompiledRuleQualityEvidence
{
    private static readonly IReadOnlyList<CanonicalQualityExpectation> Reviewed =
        CanonicalVocabularyPayload.Fixture<CanonicalQualityFixture>("quality-expectations.json").Expectations;
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    internal static string FixturePath(string name) => Path.Combine(AppContext.BaseDirectory, "RetrievalQualityFixture", name);
    internal static RetrievalQualityCase[] Cases() => JsonSerializer.Deserialize<RetrievalQualityCase[]>(
        File.ReadAllBytes(FixturePath("quality-cases.json")), Options)!;
    internal static string Hash(ReadOnlySpan<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    internal static CompiledRuleRetrievalRequest Request(CompiledRulesArtifact artifact, RetrievalQualityCase fixture) =>
        new(new(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256), fixture.Operation, "NORMAL", fixture.QueryTerms)
        {
            Topics = artifact.RuleSources.SelectMany(source => source.Applicability.Topics).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            MaximumEstimatedTokens = fixture.Budget,
            RequiredRuleSourceIds = fixture.RequiredRuleSourceIds,
            RequiredSnippetIds = fixture.RequiredSnippetIds
        };

    internal static RetrievalQualityMeasurement Measure(CompiledRulesArtifact artifact, RetrievalQualityCase fixture)
    {
        var request = Request(artifact, fixture);
        var prepared = CompiledRuleRetrieval.Prepare(request);
        var candidates = CompiledRuleCandidateIndex.Create(artifact).FindCandidates(prepared);
        var ranked = CompiledRuleRanking.Rank(candidates);
        var positives = fixture.PositiveHits.ToList();
        var negatives = fixture.NegativeHits.ToList();
        var justifications = new List<string>();
        foreach (var id in fixture.EvidenceIds)
        {
            var authority = Assert.Single(Reviewed, item => item.Id == id);
            justifications.Add(authority.Rationale);
            foreach (var snippet in authority.PresentOn)
            {
                var hit = new QualityHit(snippet, authority.ConceptId, authority.Term);
                // Explicit fixture dispositions describe operation exclusions;
                // expected positives are never inferred from matcher output.
                var source = artifact.Snippets.Single(item => item.SnippetId == snippet).RuleSourceId;
                if (fixture.ExcludedPositiveSources.Contains(source, StringComparer.Ordinal)) negatives.Add(hit);
                else positives.Add(hit);
            }
            negatives.AddRange(authority.AbsentFrom.Select(snippet => new QualityHit(snippet, authority.ConceptId, authority.Term)));
        }
        positives = positives.Distinct().OrderBy(hit => hit.SnippetId, StringComparer.Ordinal).ThenBy(hit => hit.Concept, StringComparer.Ordinal).ThenBy(hit => hit.Term, StringComparer.Ordinal).ToList();
        negatives = negatives.Distinct().OrderBy(hit => hit.SnippetId, StringComparer.Ordinal).ThenBy(hit => hit.Concept, StringComparer.Ordinal).ThenBy(hit => hit.Term, StringComparer.Ordinal).ToList();
        foreach (var hit in positives)
        {
            var root = Assert.Single(ranked.Candidates, root => root.SnippetId == hit.SnippetId);
            var match = Assert.Single(root.Matches, term => term.Kind == hit.Concept && term.Term == RuleRetrievalVocabulary.NormalizeTerm(hit.Term));
            if (hit.Weight != 0) Assert.Equal(hit.Weight, match.Weight);
        }
        foreach (var hit in negatives)
            Assert.DoesNotContain(ranked.Candidates, root => root.SnippetId == hit.SnippetId && root.Matches.Any(
                term => term.Kind == hit.Concept && term.Term == RuleRetrievalVocabulary.NormalizeTerm(hit.Term)));
        foreach (var id in fixture.PositiveRequiredIds)
        {
            var root = Assert.Single(ranked.Candidates, root => root.SnippetId == id);
            Assert.True(root.Reasons.HasFlag(CompiledRuleCandidateReason.RequiredSnippet) || root.Reasons.HasFlag(CompiledRuleCandidateReason.RequiredRuleSource));
            Assert.Equal(0, root.VocabularyScore);
        }
        if (fixture.ExpectNoHits) Assert.Equal(0, candidates.Counts.VocabularyMatched);
        if (fixture.ExpectedInapplicable is { } excluded) Assert.Equal(excluded, candidates.Counts.InapplicableVocabularyMatched);

        var expectedOrder = ranked.Candidates.OrderByDescending(root => root.VocabularyScore).ThenByDescending(root => root.Priority)
            .ThenBy(root => root.SnippetId, StringComparer.Ordinal).Select(root => root.SnippetId);
        Assert.Equal(expectedOrder, ranked.Candidates.Select(root => root.SnippetId));
        foreach (var root in ranked.Candidates) Assert.Equal(root.Matches.Sum(term => (long)term.Weight), root.VocabularyScore);

        CompiledRuleDependencyClosure? closure = null;
        CompiledRulePacketResult? result = null;
        CompiledRuleRetrievalFailure? failure = null;
        try { closure = CompiledRuleDependencies.Expand(ranked); result = CompiledRulePackets.Build(closure); }
        catch (CompiledRuleRetrievalException error) { failure = error.Failure; }
        Assert.Equal(fixture.ExpectedFailure, failure);
        var dependencyExpectations = 0;
        var requiredExpectations = 0;
        long optionalCost = 0, dependencyIncrement = 0;
        if (result is not null)
        {
            var members = result.Members.Select(member => member.SnippetId).ToHashSet(StringComparer.Ordinal);
            Assert.Equal(result.Members.Count, members.Count);
            Assert.InRange(result.Totals.UsedEstimatedTokens, 1, fixture.Budget);
            Assert.Equal(fixture.Budget - result.Totals.UsedEstimatedTokens, result.Totals.RemainingEstimatedTokens);
            Assert.Equal(result.Members.Sum(member => member.Member.Snippet.EstimatedTokens), result.Totals.UsedEstimatedTokens);
            Assert.True(result.Members.Count < artifact.Snippets.Count); // Ordinary cases must not dump the entire corpus.
            foreach (var decision in result.Decisions)
            {
                if (decision.Required) { Assert.True(decision.Selected); requiredExpectations++; }
                if (!decision.Selected) continue;
                var group = closure!.Roots.Single(group => group.Root.SnippetId == decision.Root.SnippetId);
                Assert.Contains(group.Root.SnippetId, members);
                foreach (var dependency in group.Prerequisites) { Assert.Contains(dependency.SnippetId, members); dependencyExpectations++; }
            }
            foreach (var id in fixture.PositiveRequiredIds)
            {
                Assert.Contains(id, members);
                var root = Assert.Single(result.Decisions, item => item.Root.SnippetId == id);
                Assert.True(root.Required);
                Assert.True(root.Root.Reasons.HasFlag(CompiledRuleCandidateReason.RequiredSnippet) || root.Root.Reasons.HasFlag(CompiledRuleCandidateReason.RequiredRuleSource));
                Assert.Equal(0, root.Root.VocabularyScore);
            }
            if (fixture.ExpectedCost is { } cost) Assert.Equal(cost, result.Totals.UsedEstimatedTokens);
            if (fixture.ExpectedRoots is { } roots) Assert.Equal(roots, result.Totals.SelectedRoots);
            if (fixture.ExpectedExclusions is { } exclusions) Assert.Equal(exclusions, result.Totals.BudgetExcludedRoots);
            optionalCost = result.Totals.UsedEstimatedTokens - result.Totals.RequiredEstimatedTokens;
            // Dependency-only incremental cost excludes members already reserved
            // by required closures, avoiding double counting shared prerequisites.
            var reserved = new HashSet<string>(StringComparer.Ordinal);
            foreach (var decision in result.Decisions.Where(item => item.Required))
            {
                var group = closure!.Roots.Single(group => group.Root.SnippetId == decision.Root.SnippetId);
                reserved.Add(group.Root.SnippetId);
                reserved.UnionWith(group.Prerequisites.Select(item => item.SnippetId));
            }
            dependencyIncrement = result.Members.Where(member => member.Root is null && !reserved.Contains(member.SnippetId))
                .Sum(member => (long)member.Member.Snippet.EstimatedTokens);
        }
        var wholeCost = artifact.Snippets.Sum(snippet => (long)snippet.EstimatedTokens);
        var evidence = new
        {
            fixture.Id, fixture.Category,
            Purpose = justifications.Count == 0 ? "Protect exact matching, required identity or budget boundaries without inferred relevance." : string.Join(" ", justifications),
            SourceEvidence = fixture.EvidenceIds,
            Request = new { prepared.Operation, prepared.CampaignMode, prepared.WorldModelId, prepared.ModuleIds, prepared.Topics,
                prepared.QueryTerms, prepared.RequiredRuleSourceIds, prepared.RequiredSnippetIds, prepared.MaximumEstimatedTokens },
            PositiveExpectations = positives, NegativeExpectations = negatives, fixture.PositiveRequiredIds, fixture.ExpectNoHits,
            Failure = failure, Counts = ranked.Counts,
            Ranked = ranked.Candidates.Select(root => new { root.SnippetId, root.VocabularyScore, root.Priority,
                PreparationTier = root.Candidate.Source.Applicability.PreparationTier, root.Reasons, root.Matches }),
            Closure = closure?.Members.Select(member => new { member.SnippetId, member.IsRoot, member.IsDependency, member.RequiredByRuleSourceIds }),
            MaximumDependencyDepth = closure is null ? (int?)null : Depth(artifact, closure.Members.Select(member => member.RuleSourceId)),
            SharedDependencyMembers = closure?.Members.Count(member => member.RequiredByRuleSourceIds.Count > 1),
            RequiredExpectations = requiredExpectations, DependencyExpectations = dependencyExpectations,
            Totals = result?.Totals, Decisions = result?.Decisions,
            PacketIdentities = result?.Members.Select(member => member.SnippetId),
            PacketTextSha256 = result is null ? null : Hash(JsonSerializer.SerializeToUtf8Bytes(result.Packet.Rules, Options)),
            WholeCorpusEstimatedTokens = wholeCost,
            ContextCost = result is null ? null : new { RequiredClosure = result.Totals.RequiredEstimatedTokens,
                QuerySelectedIncrement = optionalCost - dependencyIncrement, DependencyIncrement = dependencyIncrement,
                TokensAvoided = wholeCost - result.Totals.UsedEstimatedTokens,
                PacketRatio = new { Numerator = (long)result.Totals.UsedEstimatedTokens, Denominator = wholeCost },
                AvoidedRatio = new { Numerator = wholeCost - result.Totals.UsedEstimatedTokens, Denominator = wholeCost } },
            ReferenceCost = new { MaterializedSources = artifact.RuleSources.Count, MaterializedSnippets = artifact.Snippets.Count,
                MaterializedTerms = artifact.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count), RankedRoots = ranked.Candidates.Count,
                ClosureMembers = closure?.Members.Count, PacketMembers = result?.Members.Count }
        };
        return new(fixture, request, ranked, result, failure, positives.Count, negatives.Count + (fixture.ExpectNoHits ? 1 : 0), dependencyExpectations, requiredExpectations, evidence);
    }

    // Report-only DAG depth inventory. It never selects or orders executable
    // members; D's complete prerequisite groups remain the assertion authority.
    private static int Depth(CompiledRulesArtifact artifact, IEnumerable<string> selectedSources)
    {
        var sources = artifact.RuleSources.ToDictionary(source => source.RuleSourceId, StringComparer.Ordinal);
        var depths = new Dictionary<string, int>(StringComparer.Ordinal);
        int Visit(string id) => depths.TryGetValue(id, out var depth) ? depth : depths[id] = sources[id].DependencyRuleSourceIds.Count == 0
            ? 0 : 1 + sources[id].DependencyRuleSourceIds.Max(Visit);
        return selectedSources.Distinct(StringComparer.Ordinal).Max(Visit);
    }

    internal static byte[] Report(CompiledRulesArtifact artifact, byte[] artifactBytes, IEnumerable<RetrievalQualityCase> fixtures)
    {
        var measurements = fixtures.OrderBy(fixture => fixture.Id, StringComparer.Ordinal).Select(fixture => Measure(artifact, fixture)).ToArray();
        Assert.Equal(measurements.Length, measurements.Select(item => item.Fixture.Id).Distinct(StringComparer.Ordinal).Count());
        var successes = measurements.Where(item => item.Result is not null).ToArray();
        return JsonSerializer.SerializeToUtf8Bytes(new
        {
            ReportFormatVersion = 1,
            artifact.Integrity.ArtifactSha256,
            ArtifactByteSha256 = Hash(artifactBytes), ArtifactBytes = artifactBytes.Length,
            Sources = artifact.RuleSources.Count, Snippets = artifact.Snippets.Count,
            RetrievalTerms = artifact.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count),
            WholeCorpusEstimatedTokens = artifact.Snippets.Sum(snippet => (long)snippet.EstimatedTokens),
            Summary = new { Fixtures = measurements.Length, SuccessfulPackets = successes.Length, ExpectedFailures = measurements.Length - successes.Length,
                PositiveSatisfied = measurements.Sum(item => item.PositiveExpectations), PositiveTotal = measurements.Sum(item => item.PositiveExpectations),
                NegativeSatisfied = measurements.Sum(item => item.NegativeExpectations), NegativeTotal = measurements.Sum(item => item.NegativeExpectations),
                RequiredSatisfied = measurements.Sum(item => item.RequiredExpectations), RequiredTotal = measurements.Sum(item => item.RequiredExpectations),
                DependencySatisfied = measurements.Sum(item => item.DependencyExpectations), DependencyTotal = measurements.Sum(item => item.DependencyExpectations),
                ApplicableHits = measurements.Sum(item => item.Ranked.Counts.ApplicableVocabularyMatched),
                InapplicableHits = measurements.Sum(item => item.Ranked.Counts.InapplicableVocabularyMatched),
                BudgetExclusions = successes.Sum(item => item.Result!.Totals.BudgetExcludedRoots),
                Categories = measurements.GroupBy(item => item.Fixture.Category).OrderBy(group => group.Key, StringComparer.Ordinal).Select(group => new {
                    Category = group.Key, Fixtures = group.Count(), Positive = group.Sum(item => item.PositiveExpectations), Negative = group.Sum(item => item.NegativeExpectations) }),
                PacketCosts = Distribution(successes.Select(item => item.Result!.Totals.UsedEstimatedTokens)),
                RootCounts = Distribution(successes.Select(item => item.Result!.Totals.SelectedRoots)),
                MemberCounts = Distribution(successes.Select(item => item.Result!.Totals.UniqueMembers)) },
            Fixtures = measurements.Select(item => item.Evidence)
        }, Options);
    }

    private static object Distribution(IEnumerable<int> values)
    {
        var sorted = values.Order().ToArray();
        return new { Minimum = sorted[0], MedianNumerator = (long)sorted[(sorted.Length - 1) / 2] + sorted[sorted.Length / 2], MedianDenominator = 2,
            Maximum = sorted[^1], Sum = sorted.Sum(value => (long)value), Count = sorted.Length };
    }
}
