using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRuleRankingTests(ITestOutputHelper output)
{
    private static readonly byte[] CanonicalBytes = CompiledRulesImportFixtures.Canonical();
    private static readonly CompiledRulesArtifact Canonical = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(CanonicalBytes)).Artifact!;
    private static readonly CompiledRuleCandidateIndex CanonicalIndex = CompiledRuleCandidateIndex.Create(Canonical);

    [Theory]
    [InlineData("combat", 900)]
    [InlineData("fighting", 700)]
    [InlineData("save", 700)]
    [InlineData("preparation", 700)]
    [InlineData("conflict", 1100)]
    public void EveryDistinctControlledAssociationContributesExactlyItsReviewedWeight(string query, long expected)
    {
        var ranked = Rank(Fixture(), query);
        var first = Assert.Single(ranked.Candidates, item => item.SnippetId == "a-source#one");
        Assert.Equal(expected, first.VocabularyScore);
        Assert.Equal(first.Matches.Sum(match => (long)match.Weight), first.VocabularyScore);
        Assert.Equal(CompiledRuleCandidateReason.ControlledVocabulary, first.Reasons);
        Assert.Equal(query == "conflict" ? 2 : 1, first.Matches.Count);
    }

    [Fact]
    public void MultipleQueryTermsAndConceptsSumWithoutCoverageOrOriginBonuses()
    {
        var ranked = Rank(Fixture(), "combat", "conflict", "save");
        Assert.Equal(new[] { "a-source#one", "z-source#one", "runtime-kernel#one", "gm-runtime-procedure#one" }, Ids(ranked));
        Assert.Equal(new long[] { 2700, 2500, 0, 0 }, ranked.Candidates.Select(item => item.VocabularyScore));
        Assert.Equal(new[] { ("combat", "combat", 900), ("conflict", "canon", 600), ("conflict", "combat", 500), ("save", "save", 700) },
            ranked.Candidates[0].Matches.Select(match => (match.Term, match.Kind, match.Weight)));
        Assert.Equal(4, ranked.Candidates[0].Matches.Count);
    }

    [Fact]
    public void RepeatedNormalizedQueryEntriesCannotIncreaseScore()
    {
        var fixture = Fixture();
        Assert.Equal(Evidence(Rank(fixture, "fighting", "conflict")), Evidence(Rank(fixture, "FIGHTING", " fighting ", "CONFLICT", "conflict")));
    }

    [Theory]
    [InlineData(-10000, 10000)]
    [InlineData(-1, 0)]
    [InlineData(0, 1)]
    [InlineData(1, -1)]
    [InlineData(10000, -10000)]
    [InlineData(-10000, -10000)]
    [InlineData(10000, 10000)]
    [InlineData(0, 0)]
    public void HigherPriorityBreaksEqualScoresThenExactOrdinalSnippetIdentity(int a, int z)
    {
        var ranked = Rank(Fixture(a, z), "fighting");
        Assert.Equal(a >= z ? "a-source#one" : "z-source#one", ranked.Candidates[0].SnippetId);
        Assert.Equal(700, ranked.Candidates[0].VocabularyScore);
        Assert.Equal(Math.Max(a, z), ranked.Candidates[0].Priority);
    }

    [Theory]
    [InlineData(-10000, 10000)]
    [InlineData(0, 0)]
    [InlineData(10000, -10000)]
    public void PriorityCannotOverpowerStrongerReviewedWeight(int a, int z)
    {
        Assert.Equal("a-source#one", Rank(Fixture(a, z), "combat").Candidates[0].SnippetId);
        Assert.Equal("z-source#one", Rank(Fixture(a, z), "save").Candidates[0].SnippetId);
    }

    public static IEnumerable<object[]> Tiers() => Enum.GetValues<RulePreparationTier>().Select(tier => new object[] { tier });

    [Theory]
    [MemberData(nameof(Tiers))]
    public void PreparationUrgencyDoesNotChangeRelevanceEvenForPreparationQuery(RulePreparationTier tier)
    {
        var fixture = Fixture(tier: tier);
        // Tier is artifact-semantic metadata, so changing it changes scope,
        // but must not change the candidate ranking or its score explanation.
        Assert.Equal(JsonSerializer.Serialize(Rank(Fixture(), "preparation").Candidates), JsonSerializer.Serialize(Rank(fixture, "preparation").Candidates));
        Assert.Equal(new long[] { 700, 700, 0, 0 }, Rank(fixture, "preparation").Candidates.Select(item => item.VocabularyScore));
    }

    [Theory]
    [InlineData("source", false)]
    [InlineData("source", true)]
    [InlineData("snippet", false)]
    [InlineData("snippet", true)]
    public void ExplicitIdentitiesAreRetainedWithoutSyntheticScores(string target, bool matching)
    {
        var artifact = Fixture();
        var request = Request(artifact, matching ? ["combat"] : []) with
        {
            RequiredRuleSourceIds = target == "source" ? ["a-source"] : [],
            RequiredSnippetIds = target == "snippet" ? ["a-source#one"] : [], MaximumEstimatedTokens = 1
        };
        var input = Find(artifact, request);
        var ranked = CompiledRuleRanking.Rank(input);
        var first = Assert.Single(ranked.Candidates, item => item.SnippetId == "a-source#one");
        Assert.Equal(matching ? 900 : 0, first.VocabularyScore);
        Assert.True(first.Reasons.HasFlag(target == "source" ? CompiledRuleCandidateReason.RequiredRuleSource : CompiledRuleCandidateReason.RequiredSnippet));
        Assert.Equal(input.Candidates.Count, ranked.Candidates.Count);
        if (target == "source") Assert.Equal(0, Assert.Single(ranked.Candidates, item => item.SnippetId == "a-source#two").VocabularyScore);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AlwaysIncludeAddsNoScoreAndKeepsItsOwnReason(bool matching)
    {
        var artifact = Fixture(alwaysInclude: true);
        var ranked = Rank(artifact, matching ? ["save"] : []);
        var first = Assert.Single(ranked.Candidates, item => item.SnippetId == "a-source#one");
        Assert.Equal(matching ? 700 : 0, first.VocabularyScore);
        Assert.True(first.Reasons.HasFlag(CompiledRuleCandidateReason.AlwaysInclude));
        Assert.False(first.Reasons.HasFlag(CompiledRuleCandidateReason.RuntimeKernel));
        Assert.Equal(0, Assert.Single(ranked.Candidates, item => item.SnippetId == "a-source#two").VocabularyScore);
    }

    [Theory]
    [InlineData("gm-runtime-procedure")]
    [InlineData("runtime-kernel")]
    public void MandatoryRootsWithGenuineMatchesContributeNormallyWithoutExtraBoosts(string source)
    {
        var artifact = Fixture();
        var index = artifact.Snippets.ToList().FindIndex(snippet => snippet.RuleSourceId == source);
        artifact.Snippets[index] = Snippet(source, "one", Terms(("combat", "combat", 1)));
        var ranked = Rank(Seal(artifact), "combat");
        var item = Assert.Single(ranked.Candidates, candidate => candidate.Candidate.Source.RuleSourceId == source);
        Assert.Equal(1, item.VocabularyScore);
        Assert.True(item.Reasons.HasFlag(CompiledRuleCandidateReason.ControlledVocabulary));
        Assert.True(item.Reasons.HasFlag(source == "runtime-kernel" ? CompiledRuleCandidateReason.RuntimeKernel : CompiledRuleCandidateReason.GmRuntimeProcedure));
        Assert.Equal("a-source#one", ranked.Candidates[0].SnippetId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1000)]
    public void ValidWeightExtremesRemainExact(int weight)
    {
        var artifact = Fixture();
        artifact.Snippets[0] = Snippet("a-source", "one", Terms(("combat", "combat", weight)));
        Assert.Equal(weight, Assert.Single(Rank(Seal(artifact), "combat").Candidates, item => item.SnippetId == "a-source#one").VocabularyScore);
    }

    [Fact]
    public void LegalAssociationCapacityFitsInt64WithoutAnArbitraryMatchCap()
    {
        Assert.True((long)int.MaxValue * 1000 > int.MaxValue);
        Assert.True((long)int.MaxValue * 1000 < long.MaxValue);
        Assert.Equal(typeof(long), typeof(RankedCompiledRuleCandidate).GetProperty(nameof(RankedCompiledRuleCandidate.VocabularyScore))!.PropertyType);
        var artifact = Fixture();
        artifact.Snippets[0] = Snippet("a-source", "one", Terms(Enumerable.Range(0, 1000)
            .Select(index => ("combat", "concept-" + index.ToString("D4", CultureInfo.InvariantCulture), 1000)).ToArray()));
        var item = Assert.Single(Rank(Seal(artifact), "combat").Candidates, item => item.SnippetId == "a-source#one");
        Assert.Equal(1000000L, item.VocabularyScore);
        Assert.Equal(1000, item.Matches.Count);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    [InlineData("fr-FR")]
    public void RepetitionAndCandidateEnumerationRemainIdenticalAcrossCultures(string culture)
    {
        var input = Find(Fixture(), Request(Fixture(), ["combat", "conflict", "save"]));
        var expected = Evidence(CompiledRuleRanking.Rank(input));
        var previous = CultureInfo.CurrentCulture;
        var ui = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            var shuffled = new CompiledRuleCandidateSet(input.Request, Array.AsReadOnly(input.Candidates.Reverse().ToArray()), input.Counts);
            Assert.Equal(expected, Evidence(CompiledRuleRanking.Rank(shuffled)));
            Assert.Equal(expected, Evidence(CompiledRuleRanking.Rank(input)));
        }
        finally { CultureInfo.CurrentCulture = previous; CultureInfo.CurrentUICulture = ui; }
    }

    [Fact]
    public void RelocatedCompiledSourcesAndUnrelatedWorkingDirectoryDoNotChangeRanking()
    {
        using var first = new CanonicalVocabularyPayload();
        using var second = new CanonicalVocabularyPayload();
        var before = Environment.CurrentDirectory;
        try
        {
            var one = RuleCompilationPipeline.Compile(first.Load());
            Environment.CurrentDirectory = Path.GetTempPath();
            var two = RuleCompilationPipeline.Compile(second.Load());
            Assert.Equal(one.Bytes.ToArray(), two.Bytes.ToArray());
            Assert.Equal(Evidence(Rank(one.Artifact!, "fighting", "conflict")), Evidence(Rank(two.Artifact!, "conflict", "fighting")));
        }
        finally { Environment.CurrentDirectory = before; }
    }

    [Fact]
    public void TiedIdentitiesCompareOrdinallyRatherThanCurrentCulture()
    {
        var input = Find(Fixture(alwaysInclude: true), Request(Fixture(alwaysInclude: true), []));
        // Internal malformed-set fixtures exercise C's local guards without
        // opening a public bypass of B. These identities are valid ordinal ties.
        var items = new[] { "z", "I", "a", "i" }.Select(id => new CompiledRuleCandidate(
            Source(id), Snippet(id, "one", new()), CompiledRuleCandidateReason.RequiredSnippet, Array.Empty<CompiledRulesRetrievalTerm>())).ToArray();
        var ranked = CompiledRuleRanking.Rank(new(input.Request, Array.AsReadOnly(items), input.Counts));
        Assert.Equal(new[] { "I#one", "a#one", "i#one", "z#one" }, Ids(ranked));
    }

    [Theory]
    [InlineData("null-candidate")]
    [InlineData("null-source")]
    [InlineData("null-metadata")]
    [InlineData("null-snippet")]
    [InlineData("empty-id")]
    [InlineData("wrong-source")]
    [InlineData("null-matches")]
    [InlineData("null-match")]
    [InlineData("empty-term")]
    [InlineData("empty-kind")]
    [InlineData("zero-weight")]
    [InlineData("negative-weight")]
    [InlineData("large-weight")]
    [InlineData("same-key")]
    [InlineData("conflicting-weight")]
    [InlineData("unordered-matches")]
    [InlineData("low-priority")]
    [InlineData("high-priority")]
    [InlineData("unknown-reason")]
    [InlineData("no-reason")]
    [InlineData("unexplained-matches")]
    [InlineData("missing-matches")]
    [InlineData("duplicate-id")]
    public void MalformedInternalEvidenceFailsWithFixedBoundedDiagnostic(string corruption)
    {
        var input = Find(Fixture(), Request(Fixture(), ["combat"]));
        var original = input.Candidates[0];
        var source = original.Source;
        var snippet = original.Snippet;
        var reasons = original.Reasons;
        IReadOnlyList<CompiledRulesRetrievalTerm> matches = original.Matches;
        var term = new CompiledRulesRetrievalTerm { Term = "combat", Kind = "combat", Weight = 900 };
        switch (corruption)
        {
            case "null-source": source = null!; break;
            case "null-metadata": source = new() { RuleSourceId = "a-source", Applicability = null! }; break;
            case "null-snippet": snippet = null!; break;
            case "empty-id": snippet = new() { RuleSourceId = "a-source" }; break;
            case "wrong-source": source = Source("other"); break;
            case "null-matches": matches = null!; break;
            case "null-match": matches = new CompiledRulesRetrievalTerm[] { null! }; break;
            case "empty-term": matches = [new() { Kind = "combat", Weight = 900 }]; break;
            case "empty-kind": matches = [new() { Term = "combat", Weight = 900 }]; break;
            case "zero-weight": matches = [new() { Term = "combat", Kind = "combat", Weight = 0 }]; break;
            case "negative-weight": matches = [new() { Term = "combat", Kind = "combat", Weight = -1 }]; break;
            case "large-weight": matches = [new() { Term = "combat", Kind = "combat", Weight = int.MaxValue }]; break;
            case "same-key": matches = [term, term]; break;
            case "conflicting-weight": matches = [term, new() { Term = "combat", Kind = "combat", Weight = 1000 }]; break;
            case "unordered-matches": matches = [new() { Term = "save", Kind = "save", Weight = 700 }, term]; break;
            case "low-priority": source = Source("a-source", new() { Priority = -10001 }); break;
            case "high-priority": source = Source("a-source", new() { Priority = int.MaxValue }); break;
            case "unknown-reason": reasons = (CompiledRuleCandidateReason)64; break;
            case "no-reason": reasons = CompiledRuleCandidateReason.None; break;
            case "unexplained-matches": reasons = CompiledRuleCandidateReason.RequiredSnippet; break;
            case "missing-matches": matches = []; break;
        }
        var malformed = corruption == "null-candidate" ? null! : new CompiledRuleCandidate(source, snippet, reasons, matches);
        IReadOnlyList<CompiledRuleCandidate> candidates = corruption == "duplicate-id" ? [original, original] : [malformed];
        SafeFailure(() => CompiledRuleRanking.Rank(new(input.Request, candidates, input.Counts)));
    }

    [Fact]
    public void NullInputAndCancellationDoNotProducePartialRankedSuccess()
    {
        SafeFailure(() => CompiledRuleRanking.Rank(null!));
        var input = Find(Fixture(), Request(Fixture(), ["combat"]));
        using var before = new CancellationTokenSource();
        before.Cancel();
        Assert.Equal(before.Token, Assert.Throws<OperationCanceledException>(() => CompiledRuleRanking.Rank(null!, before.Token)).CancellationToken);
        using var during = new CancellationTokenSource();
        var interrupted = new CompiledRuleCandidateSet(input.Request, new CancellingList<CompiledRuleCandidate>(input.Candidates, during), input.Counts);
        Assert.Equal(during.Token, Assert.Throws<OperationCanceledException>(() => CompiledRuleRanking.Rank(interrupted, during.Token)).CancellationToken);
        using var evidence = new CancellationTokenSource();
        var original = input.Candidates[0];
        var candidate = new CompiledRuleCandidate(original.Source, original.Snippet, original.Reasons,
            new CancellingList<CompiledRulesRetrievalTerm>(original.Matches, evidence));
        Assert.Equal(evidence.Token, Assert.Throws<OperationCanceledException>(() => CompiledRuleRanking.Rank(new(input.Request, [candidate], input.Counts), evidence.Token)).CancellationToken);
    }

    [Fact]
    public void RankingPreservesFrozenGraphScopeCountsAndReadOnlyExplanation()
    {
        var artifact = Fixture();
        var input = Find(artifact, Request(artifact, ["combat", "save"]));
        var ranked = CompiledRuleRanking.Rank(input);
        var expected = Evidence(ranked);
        artifact.RuleSources.Clear();
        artifact.Snippets.Clear();
        Assert.Same(input, ranked.Input);
        Assert.Same(input.Scope, ranked.Scope);
        Assert.Same(input.Counts, ranked.Counts);
        Assert.Throws<NotSupportedException>(() => ((IList<RankedCompiledRuleCandidate>)ranked.Candidates).Clear());
        foreach (var candidate in ranked.Candidates)
        {
            Assert.Same(input.Candidates.Single(item => item.SnippetId == candidate.SnippetId), candidate.Candidate);
            Assert.Same(candidate.Candidate.Matches, candidate.Matches);
            Assert.Throws<NotSupportedException>(() => ((IList<CompiledRulesRetrievalTerm>)candidate.Matches).Clear());
        }
        Assert.Equal(expected, Evidence(ranked));
        Assert.Equal("RankedCompiledRuleCandidateSet", ranked.ToString());
        Assert.All(ranked.Candidates, candidate => Assert.Equal("RankedCompiledRuleCandidate", candidate.ToString()));
        var diagnostic = JsonSerializer.Serialize(ranked);
        Assert.DoesNotContain("Unreviewed prose", diagnostic);
        Assert.DoesNotContain("rules/a-source.md", diagnostic);
        Assert.DoesNotContain("QueryTerms", diagnostic);
    }

    [Fact]
    public void DependenciesAndTokenExpenseNeverAddCandidatesOrAffectScores()
    {
        var artifact = Fixture();
        artifact.RuleSources[0].DependencyRuleSourceIds.Add("z-source");
        artifact.Snippets[0] = Snippet("a-source", "one", Terms(("experience points", "progression", 800)));
        artifact = Seal(artifact);
        var request = Request(artifact, ["experience points"]) with { MaximumEstimatedTokens = 1 };
        var input = Find(artifact, request);
        var ranked = CompiledRuleRanking.Rank(input);
        Assert.Equal(input.Candidates.Select(item => item.SnippetId).Order(StringComparer.Ordinal), Ids(ranked).Order(StringComparer.Ordinal));
        Assert.Equal(800, ranked.Candidates[0].VocabularyScore);
        Assert.Equal(new[] { "z-source" }, ranked.Candidates[0].Candidate.Source.DependencyRuleSourceIds);
        Assert.DoesNotContain(ranked.Candidates, candidate => candidate.SnippetId == "a-source#two");
        Assert.DoesNotContain(ranked.Candidates, candidate => candidate.SnippetId == "z-source#one");
        Assert.Equal(Evidence(ranked), Evidence(CompiledRuleRanking.Rank(Find(artifact, request with { MaximumEstimatedTokens = 8000 }))));
    }

    [Theory]
    [InlineData("fighting", "gameplay.resolve", 15, 4, 0)]
    [InlineData("conflict", "gameplay.resolve", 15, 4, 5)]
    [InlineData("save", "gameplay.resolve", 12, 1, 0)]
    [InlineData("preparation", "gameplay.resolve", 14, 3, 0)]
    [InlineData("campaign canon", "context.assemble", 7, 2, 0)]
    [InlineData("unreviewed", "gameplay.resolve", 11, 0, 0)]
    [InlineData("", "gameplay.resolve", 11, 0, 0)]
    [InlineData("fighting|conflict|save|preparation", "gameplay.resolve", 19, 8, 5)]
    public void CanonicalRankedEvidenceRetainsExactlyBEligibility(string terms, string operation, int count, int matched, int excluded)
    {
        var request = Request(Canonical, terms.Length == 0 ? [] : terms.Split('|')) with
        {
            Operation = operation, Topics = Canonical.RuleSources.SelectMany(source => source.Applicability.Topics).Distinct().ToArray()
        };
        var input = CanonicalIndex.FindCandidates(CompiledRuleRetrieval.Prepare(request));
        var ranked = CompiledRuleRanking.Rank(input);
        Assert.Equal(input.Candidates.Count, ranked.Candidates.Count);
        Assert.Equal(input.Candidates.Select(item => item.SnippetId).Order(StringComparer.Ordinal), Ids(ranked).Order(StringComparer.Ordinal));
        Assert.Equal(count, ranked.Candidates.Count);
        Assert.Equal(matched, ranked.Counts.ApplicableVocabularyMatched);
        Assert.Equal(excluded, ranked.Counts.InapplicableVocabularyMatched);
        Assert.All(ranked.Candidates, item => Assert.Equal(item.Matches.Sum(match => (long)match.Weight), item.VocabularyScore));
        if (terms is "" or "unreviewed") Assert.All(ranked.Candidates, item => Assert.Equal(0, item.VocabularyScore));
        else Assert.True(ranked.Candidates[0].VocabularyScore > 0);
        if (terms is "fighting" or "conflict" or "save" or "preparation") Assert.Equal(11, ranked.Candidates.Count(item => item.VocabularyScore == 0));
        Assert.Equal(Evidence(ranked), Evidence(CompiledRuleRanking.Rank(new(input.Request, Array.AsReadOnly(input.Candidates.Reverse().ToArray()), input.Counts))));
        output.WriteLine($"QUERY {terms} operation={operation} candidates={ranked.Candidates.Count} applicable={ranked.Counts.ApplicableVocabularyMatched} excluded={excluded}");
        foreach (var candidate in ranked.Candidates)
            output.WriteLine(FormattableString.Invariant($"{candidate.SnippetId} score={candidate.VocabularyScore} priority={candidate.Priority} reasons={candidate.Reasons} contributions={string.Join(",", candidate.Matches.Select(match => FormattableString.Invariant($"{match.Term}/{match.Kind}:{match.Weight}")))}"));
    }

    [Fact]
    public void CanonicalCompilationBytesAndSourceSnippetIdentityRemainUnchanged()
    {
        var before = JsonSerializer.Serialize(Canonical);
        _ = Rank(Canonical, "save", "preparation");
        Assert.Equal(before, JsonSerializer.Serialize(Canonical));
        Assert.Equal(10, Canonical.RuleSources.Count);
        Assert.Equal(154, Canonical.Snippets.Count);
        Assert.Equal(773, Canonical.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count));
        Assert.Equal(275622, CanonicalBytes.Length);
        Assert.Empty(CompiledRulesArtifactContract.Validate(Canonical));
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", Canonical.Integrity.ArtifactSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", Convert.ToHexString(SHA256.HashData(CanonicalBytes)));
        Assert.Equal(CanonicalBytes, CompiledRulesArtifactWriter.Write(Canonical).Bytes.ToArray());
    }

    private static CompiledRulesArtifact Fixture(int a = 0, int z = 0, RulePreparationTier tier = RulePreparationTier.Standard, bool alwaysInclude = false) => Seal(new()
    {
        ArtifactFormatVersion = 1, Compiler = new() { ContractVersion = "1", ImplementationId = "fixture", ImplementationVersion = "1.0.0" },
        Ruleset = new() { RulesetId = "fixture", RepositoryVersion = "1.0.0", Source = new() { Scheme = "snapshot", Value = "immutable" }, ManifestPath = "rules/manifest.json", ManifestSha256 = new string('B', 64) },
        Integrity = new() { Algorithm = "SHA-256" },
        RuleSources = [Source("a-source", new() { Layer = RuleLayer.Core, Priority = a, PreparationTier = tier, AlwaysInclude = alwaysInclude }),
            Source("gm-runtime-procedure", new() { Layer = RuleLayer.Core, AlwaysInclude = true, Operations = ["gameplay.resolve"] }),
            Source("runtime-kernel", new() { Layer = RuleLayer.RuntimeKernel, AlwaysInclude = true, Priority = 10000 }), Source("z-source", new() { Layer = RuleLayer.Core, Priority = z })],
        Snippets = [Snippet("a-source", "one", Terms(("combat", "combat", 900), ("conflict", "canon", 600), ("conflict", "combat", 500), ("fighting", "combat", 700), ("preparation", "preparation", 700), ("save", "save", 700))),
            Snippet("a-source", "two", new()), Snippet("gm-runtime-procedure", "one", new()), Snippet("runtime-kernel", "one", new()),
            Snippet("z-source", "one", Terms(("combat", "combat", 800), ("conflict", "canon", 700), ("fighting", "combat", 700), ("preparation", "preparation", 700), ("save", "save", 1000)))]
    });

    private static CompiledRulesArtifactRuleSource Source(string id, CompiledRulesArtifactApplicability? applicability = null) => new()
    { RuleSourceId = id, SourcePath = $"rules/{id}.md", SourceSha256 = new string('A', 64), Applicability = applicability ?? new() { Layer = RuleLayer.Core } };

    private static CompiledRulesArtifactSnippet Snippet(string source, string anchor, CompiledRulesRetrievalMetadata terms)
    {
        var content = "Unreviewed prose should not create relevance.";
        return new() { SnippetId = source + "#" + anchor, RuleSourceId = source, SourceAnchor = anchor, Content = content,
            ContentSha256 = CompiledRulesArtifactContract.ComputeContentSha256(content), EstimatedTokens = 10, Retrieval = terms };
    }

    private static CompiledRulesRetrievalMetadata Terms(params (string Term, string Kind, int Weight)[] terms) => new()
    { Terms = terms.OrderBy(term => term.Term, StringComparer.Ordinal).ThenBy(term => term.Kind, StringComparer.Ordinal)
        .Select(term => new CompiledRulesRetrievalTerm { Term = term.Term, Kind = term.Kind, Weight = term.Weight }).ToList() };

    private static CompiledRulesArtifact Seal(CompiledRulesArtifact artifact) => new()
    { ArtifactFormatVersion = artifact.ArtifactFormatVersion, Compiler = artifact.Compiler, Ruleset = artifact.Ruleset, RuleSources = artifact.RuleSources,
        Snippets = artifact.Snippets, Integrity = new() { Algorithm = "SHA-256", ArtifactSha256 = CompiledRulesArtifactContract.ComputeArtifactSha256(artifact) } };

    private static CompiledRuleRetrievalRequest Request(CompiledRulesArtifact artifact, params string[] terms) =>
        new(new(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256), "gameplay.resolve", "NORMAL", terms)
        { Topics = artifact.RuleSources.SelectMany(source => source.Applicability.Topics).Distinct().ToArray() };
    private static CompiledRuleCandidateSet Find(CompiledRulesArtifact artifact, CompiledRuleRetrievalRequest request) =>
        CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(request));
    private static RankedCompiledRuleCandidateSet Rank(CompiledRulesArtifact artifact, params string[] terms) =>
        CompiledRuleRanking.Rank(Find(artifact, Request(artifact, terms)));
    private static IEnumerable<string> Ids(RankedCompiledRuleCandidateSet ranked) => ranked.Candidates.Select(item => item.SnippetId);
    private static string Evidence(RankedCompiledRuleCandidateSet ranked) => JsonSerializer.Serialize(ranked);
    private static void SafeFailure(Action action)
    {
        var exception = Assert.Throws<CompiledRuleRetrievalException>(action);
        Assert.Equal(CompiledRuleRetrievalFailure.ArtifactInconsistent, exception.Failure);
        Assert.Equal("RULE_RETRIEVAL_ARTIFACT_INCONSISTENT", exception.Code);
        Assert.InRange(exception.Message.Length, 1, 128);
        Assert.Null(exception.InnerException);
    }

    private sealed class CancellingList<T>(IReadOnlyList<T> values, CancellationTokenSource cancellation) : IReadOnlyList<T>
    {
        public int Count => values.Count;
        public T this[int index] => values[index];
        public IEnumerator<T> GetEnumerator() { cancellation.Cancel(); return values.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
