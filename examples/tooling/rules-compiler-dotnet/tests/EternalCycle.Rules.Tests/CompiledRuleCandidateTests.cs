using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRuleCandidateTests(ITestOutputHelper output)
{
    private static readonly byte[] CanonicalBytes = CompiledRulesImportFixtures.Canonical();
    private static readonly CompiledRulesArtifact Canonical = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(CanonicalBytes)).Artifact!;
    private static readonly CompiledRuleCandidateIndex CanonicalIndex = CompiledRuleCandidateIndex.Create(Canonical);
    private static readonly CanonicalQualityFixture Quality = CanonicalVocabularyPayload.Fixture<CanonicalQualityFixture>("quality-expectations.json");

    [Theory]
    [InlineData("combat", "combat", "combat", 900)]
    [InlineData("FIGHTING", "fighting", "combat", 700)]
    [InlineData("  Campaign\t\nCanon  ", "campaign canon", "campaign-canon", 800)]
    [InlineData("EXPERIENCE\u00A0POINTS", "experience points", "progression", 800)]
    [InlineData("Soul-Bound", "soul-bound", "bond", 800)]
    [InlineData("save?!", "save?!", "save", 400)]
    [InlineData("CAFE\u0301", "caf\u00E9", "culture", 500)]
    public void WholeReviewedEntriesUsePreparedNormalization(string query, string term, string kind, int weight)
    {
        var artifact = Fixture();
        var result = Find(artifact, query);
        var candidate = Assert.Single(result.Candidates, item => item.Snippet.SnippetId == "a-source#one");
        var match = Assert.Single(candidate.Matches);
        Assert.Equal((term, kind, weight), (match.Term, match.Kind, match.Weight));
        Assert.True(candidate.ApplicabilitySatisfied);
        Assert.Equal(CompiledRuleCandidateReason.ControlledVocabulary, candidate.Reasons);
    }

    [Theory]
    [InlineData("saved")]
    [InlineData("saving")]
    [InlineData("combative")]
    [InlineData("fight")]
    [InlineData("campaign")]
    [InlineData("canon")]
    [InlineData("canon campaign")]
    [InlineData("campaign canon elsewhere")]
    [InlineData("campaign-canon")]
    [InlineData("soul bound")]
    [InlineData("save!")]
    [InlineData("points")]
    [InlineData("unreviewed prose")]
    [InlineData("# a-source")]
    public void SimilarWordsProseIdentitiesAndPartialPhrasesAreNotSearchable(string query)
    {
        var result = Find(Fixture(), query);
        Assert.Equal(0, result.Counts.VocabularyMatched);
        Assert.All(result.Candidates, candidate => Assert.Empty(candidate.Matches));
        Assert.Equal(new[] { "gm-runtime-procedure#one", "runtime-kernel#one" }, Ids(result));
    }

    [Fact]
    public void MultipleConceptsAndTermsKeepAllWeightsWithoutDuplicatingCandidates()
    {
        var result = Find(Fixture(), "conflict", "save", "conflict", "combat");
        var first = Assert.Single(result.Candidates, candidate => candidate.Snippet.SnippetId == "a-source#one");
        Assert.Equal(new[] { ("combat", "combat", 900), ("conflict", "canon", 600), ("conflict", "combat", 500), ("save", "save", 700) },
            first.Matches.Select(term => (term.Term, term.Kind, term.Weight)));
        Assert.Equal(result.Candidates.Count, result.Candidates.Select(candidate => candidate.Snippet.SnippetId).Distinct().Count());
        Assert.Equal(3, result.Counts.VocabularyMatched);
        Assert.Equal(0, result.Counts.InapplicableVocabularyMatched);
    }

    [Fact]
    public void ValidManyConceptMatchesAreNotArbitrarilyTruncated()
    {
        var artifact = Fixture();
        artifact.Snippets[0] = Snippet("a-source", "one", Terms(Enumerable.Range(0, 100)
            .Select(index => ("save", "concept-" + index.ToString("D3", CultureInfo.InvariantCulture), index + 1)).ToArray()));
        artifact = Seal(artifact);
        var candidate = Assert.Single(Find(artifact, "save").Candidates, candidate => candidate.SnippetId == "a-source#one");
        Assert.Equal(100, candidate.Matches.Count);
        Assert.Equal(Enumerable.Range(1, 100), candidate.Matches.Select(match => match.Weight));
    }

    [Fact]
    public void RetrievalRelationshipsDoNotGenerateAdditionalQueryTerms()
    {
        var artifact = Fixture();
        artifact.Snippets[0].Retrieval.Relationships.Add(new() { FromTerm = "save", ToTerm = "combat", Kind = "related", Weight = 1000 });
        artifact = Seal(artifact);
        var candidate = Assert.Single(Find(artifact, "save").Candidates, candidate => candidate.SnippetId == "a-source#one");
        Assert.Equal("save", Assert.Single(candidate.Matches).Term);
        Assert.Single(candidate.Snippet.Retrieval.Relationships);
    }

    [Fact]
    public void ArtifactOrderIsNeutralNotWeightPriorityPreparationOrLayerRanking()
    {
        var result = Find(Fixture(), "save");
        Assert.Equal(new[] { "a-source#one", "a-source#two", "gm-runtime-procedure#one", "runtime-kernel#one" }, Ids(result));
        Assert.Equal(new[] { 700, 1000 }, result.Candidates.Where(candidate => candidate.Matches.Count > 0).Select(candidate => candidate.Matches[0].Weight));
        Assert.Equal(-10000, result.Candidates[0].Source.Applicability.Priority);
        Assert.Equal(RulePreparationTier.OptionalRare, result.Candidates[0].Source.Applicability.PreparationTier);
        Assert.Contains(result.Candidates, candidate => candidate.Source.Applicability.Layer == RuleLayer.RuntimeKernel);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RequiredIdentitiesAreExactAndHaveIndependentReasons(bool wholeSource)
    {
        var artifact = Fixture();
        var request = Request(artifact) with
        {
            RequiredRuleSourceIds = wholeSource ? ["a-source"] : [],
            RequiredSnippetIds = wholeSource ? [] : ["a-source#two"]
        };
        var result = CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(request));
        var explicitCandidates = result.Candidates.Where(candidate => candidate.Source.RuleSourceId == "a-source").ToArray();
        Assert.Equal(wholeSource ? 2 : 1, explicitCandidates.Length);
        Assert.All(explicitCandidates, candidate =>
        {
            Assert.Empty(candidate.Matches);
            Assert.Equal(wholeSource ? CompiledRuleCandidateReason.RequiredRuleSource : CompiledRuleCandidateReason.RequiredSnippet, candidate.Reasons);
        });
    }

    [Fact]
    public void OverlappingInclusionReasonsDoNotDuplicateEvidenceOrSnippets()
    {
        var artifact = Fixture(new() { Layer = RuleLayer.Core, AlwaysInclude = true });
        var request = Request(artifact, "save", "SAVE") with { RequiredRuleSourceIds = ["a-source"], RequiredSnippetIds = ["a-source#one"] };
        var result = CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(request));
        var candidate = Assert.Single(result.Candidates, candidate => candidate.SnippetId == "a-source#one");
        Assert.Single(candidate.Matches);
        Assert.Equal(CompiledRuleCandidateReason.ControlledVocabulary | CompiledRuleCandidateReason.RequiredRuleSource |
            CompiledRuleCandidateReason.RequiredSnippet | CompiledRuleCandidateReason.AlwaysInclude, candidate.Reasons);
    }

    [Fact]
    public void ExplicitSourceWithoutExecutableSnippetsCannotSucceed()
    {
        var artifact = Fixture();
        artifact.RuleSources.Add(Source("zz-empty"));
        artifact = Seal(artifact);
        var request = Request(artifact) with { RequiredRuleSourceIds = ["zz-empty"] };
        SafeFailure(() => CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(request)), CompiledRuleRetrievalFailure.RequestInvalid);
    }

    [Theory]
    [InlineData("missing", false)]
    [InlineData("A-source", false)]
    [InlineData("missing#one", true)]
    [InlineData("a-source#missing", true)]
    [InlineData("a-source#ONE", true)]
    public void MissingOrWrongCaseRequiredIdentityFailsSafely(string id, bool snippet)
    {
        var artifact = Fixture();
        var request = Request(artifact) with { RequiredRuleSourceIds = snippet ? [] : [id], RequiredSnippetIds = snippet ? [id] : [] };
        SafeFailure(() => CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(request)), CompiledRuleRetrievalFailure.RequestInvalid);
    }

    public static IEnumerable<object[]> SelectorCases()
    {
        foreach (var selector in new[] { "world", "module", "mode", "operation", "topic" })
        {
            yield return [selector, "exact", true];
            yield return [selector, "case", true];
            yield return [selector, "multiple", true];
            yield return [selector, "wrong", false];
            yield return [selector, "wildcard", true];
        }
        yield return ["world", "missing", false];
        yield return ["module", "missing", false];
        yield return ["topic", "missing", false];
        yield return ["world", "wildcard-missing", false];
        yield return ["module", "wildcard-missing", false];
        yield return ["topic", "wildcard-missing", true];
    }

    [Theory]
    [MemberData(nameof(SelectorCases))]
    public void EverySelectorGatesStrongMatchesAndAlwaysInclude(string selector, string selection, bool expected)
    {
        foreach (var always in new[] { false, true })
        {
            var values = selection.StartsWith("wildcard", StringComparison.Ordinal) ? new[] { "*" } : new[] { "other", "wanted" };
            var metadata = new CompiledRulesArtifactApplicability
            {
                Layer = selector == "world" ? RuleLayer.World : selector == "module" ? RuleLayer.OptionalModule : RuleLayer.Core,
                WorldModelIds = selector == "world" ? values : [], ModuleIds = selector == "module" ? values : [],
                CampaignModes = selector == "mode" ? values : [], Operations = selector == "operation" ? values : [],
                Topics = selector == "topic" ? values : [], AlwaysInclude = always
            };
            var artifact = Fixture(metadata);
            var value = selection is "missing" or "wildcard-missing" ? null : selection == "wrong" ? "wrong" : selection == "case" ? "WANTED" : "wanted";
            var request = Request(artifact, "save") with
            {
                Operation = selector == "operation" ? value! : "gameplay.resolve",
                CampaignMode = selector == "mode" ? value! : "NORMAL",
                WorldModelId = selector == "world" ? value : null,
                ModuleIds = selector == "module" && value is not null ? [value] : [],
                Topics = selector == "topic" && value is not null ? [value] : []
            };
            var result = CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(request));
            Assert.Equal(expected, result.Candidates.Any(candidate => candidate.Source.RuleSourceId == "a-source"));
            Assert.Equal(expected ? 2 : 0, result.Counts.ApplicableVocabularyMatched);
            Assert.Equal(expected ? 0 : 2, result.Counts.InapplicableVocabularyMatched);
            Assert.Equal(artifact.Snippets.Count - 2, result.Counts.VocabularyUnmatched);
            if (expected) Assert.All(result.Candidates.Where(candidate => candidate.Source.RuleSourceId == "a-source"),
                candidate => Assert.Equal(always, candidate.Reasons.HasFlag(CompiledRuleCandidateReason.AlwaysInclude)));
        }
    }

    [Fact]
    public void ModuleScopeAlsoGatesCoreAndKernelSources()
    {
        foreach (var layer in new[] { RuleLayer.Core, RuleLayer.RuntimeKernel })
        {
            var artifact = Fixture(new() { Layer = layer, ModuleIds = ["enabled"] });
            Assert.DoesNotContain(Find(artifact, "save").Candidates, candidate => candidate.Source.RuleSourceId == "a-source");
        }
    }

    [Fact]
    public void InapplicableExplicitIdentityDoesNotOverrideSelectors()
    {
        var artifact = Fixture(new() { Layer = RuleLayer.World, WorldModelIds = ["world-a"] });
        var index = CompiledRuleCandidateIndex.Create(artifact);
        foreach (var request in new[] { Request(artifact) with { RequiredRuleSourceIds = ["a-source"] },
            Request(artifact) with { RequiredSnippetIds = ["a-source#one"] } })
            SafeFailure(() => index.FindCandidates(CompiledRuleRetrieval.Prepare(request)), CompiledRuleRetrievalFailure.RequestInvalid);
    }

    [Fact]
    public void NoQueryOrUnknownQueryIncludesOnlyApplicableStructuralRoots()
    {
        var artifact = Fixture(new() { Layer = RuleLayer.Core, AlwaysInclude = true });
        var index = CompiledRuleCandidateIndex.Create(artifact);
        var empty = index.FindCandidates(CompiledRuleRetrieval.Prepare(Request(artifact)));
        var unknown = index.FindCandidates(CompiledRuleRetrieval.Prepare(Request(artifact, "unreviewed")));
        Assert.Equal(Ids(empty), Ids(unknown));
        Assert.DoesNotContain("z-source#one", Ids(empty));
        Assert.All(empty.Candidates, candidate => Assert.Empty(candidate.Matches));
        var procedure = Assert.Single(empty.Candidates, candidate => candidate.Source.RuleSourceId == "gm-runtime-procedure");
        Assert.Equal(CompiledRuleCandidateReason.GmRuntimeProcedure | CompiledRuleCandidateReason.AlwaysInclude, procedure.Reasons);
        Assert.Equal(CompiledRuleCandidateReason.RuntimeKernel | CompiledRuleCandidateReason.AlwaysInclude,
            Assert.Single(empty.Candidates, candidate => candidate.Source.RuleSourceId == "runtime-kernel").Reasons);
        var otherOperation = index.FindCandidates(CompiledRuleRetrieval.Prepare(Request(artifact) with { Operation = "context.assemble" }));
        Assert.DoesNotContain(otherOperation.Candidates, candidate => candidate.Source.RuleSourceId == "gm-runtime-procedure");
    }

    [Theory]
    [InlineData("kernel-missing")]
    [InlineData("kernel-inapplicable")]
    [InlineData("procedure-missing")]
    [InlineData("procedure-inapplicable")]
    [InlineData("procedure-not-always")]
    public void MissingOrInapplicableMandatoryRuntimeStructureFails(string issue)
    {
        var artifact = Fixture();
        var id = issue.StartsWith("kernel", StringComparison.Ordinal) ? "runtime-kernel" : "gm-runtime-procedure";
        var previous = artifact.RuleSources.Single(source => source.RuleSourceId == id);
        if (issue.EndsWith("missing", StringComparison.Ordinal))
        {
            artifact.RuleSources.Remove(previous);
            artifact.Snippets.Remove(artifact.Snippets.Single(snippet => snippet.RuleSourceId == id));
        }
        else artifact.RuleSources[artifact.RuleSources.IndexOf(previous)] = Source(id, new()
        {
            Layer = previous.Applicability.Layer,
            Operations = issue.EndsWith("inapplicable", StringComparison.Ordinal) ? ["setup.only"] : ["gameplay.resolve"],
            AlwaysInclude = issue != "procedure-not-always"
        });
        artifact = Seal(artifact);
        SafeFailure(() => Find(artifact), CompiledRuleRetrievalFailure.ArtifactInconsistent);
    }

    [Fact]
    public void DependenciesAreCarriedButNotExpandedOrScored()
    {
        var artifact = Fixture();
        var source = artifact.RuleSources.Single(source => source.RuleSourceId == "a-source");
        source.DependencyRuleSourceIds.Add("z-source");
        artifact = Seal(artifact);
        var result = Find(artifact, "save");
        Assert.DoesNotContain(result.Candidates, candidate => candidate.Source.RuleSourceId == "z-source");
        Assert.Equal(new[] { "z-source" }, result.Candidates[0].Source.DependencyRuleSourceIds);
        Assert.Equal(RulePreparationTier.OptionalRare, result.Candidates[0].Source.Applicability.PreparationTier);
        Assert.DoesNotContain(typeof(CompiledRuleCandidate).GetProperties(), property => property.Name is "Score" or "Rank" or "Ready");
    }

    [Fact]
    public void SameIdsInDifferentArtifactHistoriesCannotMixContentTermsOrSelectors()
    {
        var first = Fixture();
        var second = Fixture(new() { Layer = RuleLayer.World, WorldModelIds = ["world-b"] });
        var firstIndex = CompiledRuleCandidateIndex.Create(first);
        var secondIndex = CompiledRuleCandidateIndex.Create(second);
        var request = CompiledRuleRetrieval.Prepare(Request(first, "save"));
        Assert.NotEqual(firstIndex.Scope, secondIndex.Scope);
        SafeFailure(() => secondIndex.FindCandidates(request), CompiledRuleRetrievalFailure.RequestInvalid);
        Assert.Equal(2, firstIndex.FindCandidates(request).Counts.ApplicableVocabularyMatched);
        Assert.Equal(0, Find(second, "save").Counts.ApplicableVocabularyMatched);
        SafeFailure(() => firstIndex.FindCandidates(CompiledRuleRetrieval.Prepare(Request(first) with
            { Scope = firstIndex.Scope with { RulesetId = "another-ruleset" } })), CompiledRuleRetrievalFailure.RequestInvalid);
    }

    [Theory]
    [InlineData("duplicate-source")]
    [InlineData("duplicate-snippet")]
    [InlineData("duplicate-term")]
    [InlineData("conflicting-weight")]
    [InlineData("missing-source")]
    [InlineData("bad-weight")]
    [InlineData("bad-selector")]
    [InlineData("null-retrieval")]
    [InlineData("null-source")]
    [InlineData("missing-dependency")]
    [InlineData("hash-mismatch")]
    [InlineData("source-order")]
    [InlineData("snippet-order")]
    public void InvalidArtifactFailsInsteadOfOverwritingOrReturningPartialCandidates(string issue)
    {
        var artifact = Fixture();
        var first = artifact.Snippets[0];
        switch (issue)
        {
            case "duplicate-source": artifact.RuleSources.Insert(1, artifact.RuleSources[0]); break;
            case "duplicate-snippet": artifact.Snippets.Insert(1, first); break;
            case "duplicate-term": first.Retrieval.Terms.Insert(1, first.Retrieval.Terms[0]); break;
            case "conflicting-weight": first.Retrieval.Terms.Insert(1, new() { Term = first.Retrieval.Terms[0].Term, Kind = first.Retrieval.Terms[0].Kind, Weight = 1 }); break;
            case "missing-source": artifact.RuleSources.RemoveAt(0); break;
            case "bad-weight": first.Retrieval.Terms.Add(new() { Term = "zzz", Kind = "invalid", Weight = 1001 }); break;
            case "bad-selector": artifact.RuleSources[0].Applicability.Topics.Add("../private-path"); break;
            case "null-retrieval": artifact.Snippets[0] = Snippet("a-source", "one", null!); break;
            case "null-source": artifact.RuleSources[0] = null!; break;
            case "missing-dependency": artifact.RuleSources[0].DependencyRuleSourceIds.Add("absent"); break;
            case "hash-mismatch": artifact.Snippets.RemoveAt(0); break;
            case "source-order": (artifact.RuleSources[0], artifact.RuleSources[1]) = (artifact.RuleSources[1], artifact.RuleSources[0]); break;
            case "snippet-order": (artifact.Snippets[0], artifact.Snippets[1]) = (artifact.Snippets[1], artifact.Snippets[0]); break;
        }
        SafeFailure(() => CompiledRuleCandidateIndex.Create(artifact), CompiledRuleRetrievalFailure.ArtifactInconsistent);
    }

    [Fact]
    public void CandidateGraphIsReadOnlyAndInputAliasesCannotChangeLaterQueries()
    {
        var artifact = Fixture();
        var index = CompiledRuleCandidateIndex.Create(artifact);
        artifact.Snippets[0].Retrieval.Terms.Clear();
        artifact.RuleSources[0].Applicability.Topics.Add("private");
        artifact.RuleSources[0].DependencyRuleSourceIds.Add("absent");
        artifact.Snippets.Clear();
        artifact.RuleSources.Clear();
        var result = index.FindCandidates(CompiledRuleRetrieval.Prepare(new(index.Scope, "gameplay.resolve", "NORMAL", ["save"])));
        Assert.Equal(2, result.Counts.VocabularyMatched);
        Assert.Throws<NotSupportedException>(() => ((IList<CompiledRuleCandidate>)result.Candidates).Clear());
        foreach (var candidate in result.Candidates)
        {
            Assert.Throws<NotSupportedException>(() => ((IList<CompiledRulesRetrievalTerm>)candidate.Matches).Clear());
            Assert.Throws<NotSupportedException>(() => candidate.Snippet.Retrieval.Terms.Clear());
            Assert.Throws<NotSupportedException>(() => candidate.Snippet.Retrieval.Relationships.Clear());
            Assert.Throws<NotSupportedException>(() => candidate.Source.DependencyRuleSourceIds.Clear());
            Assert.Throws<NotSupportedException>(() => candidate.Source.Applicability.Topics.Clear());
        }
        Assert.Equal("CompiledRuleCandidateSet", result.ToString());
        Assert.Equal("CompiledRuleCandidateIndex", index.ToString());
        var diagnostic = JsonSerializer.Serialize(result);
        Assert.DoesNotContain("Test a-source", diagnostic);
        Assert.DoesNotContain("rules/a-source.md", diagnostic);
        Assert.DoesNotContain("QueryTerms", diagnostic);
    }

    [Fact]
    public void RepetitionCulturesAndSetInsertionOrdersDoNotChangeEvidence()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            string? expected = null;
            var artifact = Fixture();
            var index = CompiledRuleCandidateIndex.Create(artifact);
            foreach (var culture in new[] { "en-US", "tr-TR", "fr-FR" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                foreach (var reverse in new[] { false, true })
                {
                    var request = Request(artifact, reverse ? ["save", "CONFLICT", "FIGHTING"] : ["FIGHTING", "conflict", "save"]) with
                    {
                        ModuleIds = reverse ? ["b", "a"] : ["a", "b"], Topics = reverse ? ["two", "one"] : ["one", "two"],
                        RequiredRuleSourceIds = reverse ? ["z-source", "a-source"] : ["a-source", "z-source"]
                    };
                    var actual = Evidence(index.FindCandidates(CompiledRuleRetrieval.Prepare(request)));
                    expected ??= actual;
                    Assert.Equal(expected, actual);
                }
            }
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void CancellationAndNullInputsRemainSafeWithoutPartialSuccess()
    {
        var artifact = Fixture();
        var index = CompiledRuleCandidateIndex.Create(artifact);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Equal(cancellation.Token, Assert.Throws<OperationCanceledException>(() => index.FindCandidates(null!, cancellation.Token)).CancellationToken);
        Assert.Equal(cancellation.Token, Assert.Throws<OperationCanceledException>(() => CompiledRuleCandidateIndex.Create(null!, cancellation.Token)).CancellationToken);
        SafeFailure(() => index.FindCandidates(null!), CompiledRuleRetrievalFailure.RequestInvalid);
        SafeFailure(() => CompiledRuleCandidateIndex.Create(null!), CompiledRuleRetrievalFailure.ArtifactInconsistent);
        using var during = new CancellationTokenSource();
        var incoming = new CompiledRulesArtifact
        {
            ArtifactFormatVersion = artifact.ArtifactFormatVersion, Compiler = artifact.Compiler, Ruleset = artifact.Ruleset,
            RuleSources = artifact.RuleSources, Snippets = new CancellingList<CompiledRulesArtifactSnippet>(artifact.Snippets, during), Integrity = artifact.Integrity
        };
        Assert.Equal(during.Token, Assert.Throws<OperationCanceledException>(() => CompiledRuleCandidateIndex.Create(incoming, during.Token)).CancellationToken);
    }

    public static IEnumerable<object[]> CanonicalCases() => Quality.Expectations.Select(item => new object[] { item.Id });

    [Theory]
    [MemberData(nameof(CanonicalCases))]
    public void CuratedTopologyYieldsOnlyReviewedMatchesAfterApplicability(string id)
    {
        var item = Quality.Expectations.Single(item => item.Id == id);
        foreach (var positive in item.PresentOn)
        {
            var snippet = Canonical.Snippets.Single(snippet => snippet.SnippetId == positive);
            var source = Canonical.RuleSources.Single(source => source.RuleSourceId == snippet.RuleSourceId);
            var request = Request(Canonical, item.Term) with
            {
                Operation = source.Applicability.Operations.FirstOrDefault(operation => operation != "*") ?? "gameplay.resolve",
                Topics = source.Applicability.Topics.ToArray()
            };
            var result = CanonicalIndex.FindCandidates(CompiledRuleRetrieval.Prepare(request));
            Assert.Contains(Assert.Single(result.Candidates, candidate => candidate.Snippet.SnippetId == positive).Matches,
                match => match.Term == item.Term && match.Kind == item.ConceptId);
            foreach (var negative in item.AbsentFrom)
                Assert.DoesNotContain(result.Candidates.Where(candidate => candidate.Snippet.SnippetId == negative).SelectMany(candidate => candidate.Matches),
                    match => match.Term == item.Term && match.Kind == item.ConceptId);
            foreach (var candidate in result.Candidates)
                Assert.All(candidate.Matches, match => Assert.Contains(Canonical.Snippets.Single(snippet => snippet.SnippetId == candidate.Snippet.SnippetId).Retrieval.Terms,
                    term => term.Term == match.Term && term.Kind == match.Kind && term.Weight == match.Weight));
        }
    }

    [Fact]
    public void CanonicalQueriesHaveBoundedReviewedAmbiguityAndNoWholeCorpusFallback()
    {
        Assert.Equal(10, Canonical.RuleSources.Count);
        Assert.Equal(154, Canonical.Snippets.Count);
        Assert.Equal(773, Canonical.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count));
        Assert.Equal(275622, CanonicalBytes.Length);
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", Canonical.Integrity.ArtifactSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", Convert.ToHexString(SHA256.HashData(CanonicalBytes)));
        var topics = Canonical.RuleSources.SelectMany(source => source.Applicability.Topics).Distinct().ToArray();
        foreach (var query in new[] { "fighting", "conflict", "save", "preparation", "campaign canon", "character knowledge", "gm adjudication", "unreviewed", "" })
        {
            var request = Request(Canonical, query.Length == 0 ? [] : [query]) with { Topics = topics };
            var result = CanonicalIndex.FindCandidates(CompiledRuleRetrieval.Prepare(request));
            Assert.InRange(result.Candidates.Count, 1, 153);
            Assert.Equal(Canonical.Snippets.Count(snippet => snippet.Retrieval.Terms.Any(term => term.Term == query)), result.Counts.VocabularyMatched);
            if (query is "" or "unreviewed") Assert.All(result.Candidates, candidate => Assert.Empty(candidate.Matches));
            output.WriteLine($"{query,-20} candidates={result.Candidates.Count} matched={result.Counts.VocabularyMatched} applicable={result.Counts.ApplicableVocabularyMatched} excluded={result.Counts.InapplicableVocabularyMatched}");
        }
        Assert.Equal(CanonicalBytes, CompiledRulesArtifactWriter.Write(Canonical).Bytes.ToArray());
    }

    private static CompiledRulesArtifact Fixture(CompiledRulesArtifactApplicability? metadata = null)
    {
        var sources = new[]
        {
            Source("a-source", metadata ?? new() { Layer = RuleLayer.Core, Priority = -10000, PreparationTier = RulePreparationTier.OptionalRare }),
            Source("gm-runtime-procedure", new() { Layer = RuleLayer.Core, AlwaysInclude = true, Operations = ["gameplay.resolve"] }),
            Source("runtime-kernel", new() { Layer = RuleLayer.RuntimeKernel, AlwaysInclude = true }), Source("z-source")
        };
        return Seal(new()
        {
            ArtifactFormatVersion = 1,
            Integrity = new() { Algorithm = "SHA-256" },
            Compiler = new() { ContractVersion = "1", ImplementationId = "fixture", ImplementationVersion = "1.0.0" },
            Ruleset = new() { RulesetId = "fixture-rules", RepositoryVersion = "1.0.0", Source = new() { Scheme = "snapshot", Value = "immutable-fixture" }, ManifestPath = "rules/manifest.json", ManifestSha256 = new string('B', 64) },
            RuleSources = sources.ToList(),
            Snippets = [Snippet("a-source", "one", Terms(("campaign canon", "campaign-canon", 800), ("combat", "combat", 900),
                ("conflict", "canon", 600), ("conflict", "combat", 500), ("experience points", "progression", 800), ("fighting", "combat", 700),
                ("soul-bound", "bond", 800), ("save", "save", 700), ("save?!", "save", 400), ("caf\u00E9", "culture", 500))),
                Snippet("a-source", "two", Terms(("save", "save", 1000))), Snippet("gm-runtime-procedure", "one", new()),
                Snippet("runtime-kernel", "one", new()), Snippet("z-source", "one", Terms(("combat", "combat", 800), ("conflict", "canon", 700)))]
        });
    }

    private static CompiledRulesArtifactRuleSource Source(string id, CompiledRulesArtifactApplicability? metadata = null) =>
        new() { RuleSourceId = id, SourcePath = $"rules/{id}.md", SourceSha256 = new string('A', 64), Applicability = metadata ?? new() { Layer = RuleLayer.Core } };

    private static CompiledRulesArtifactSnippet Snippet(string source, string anchor, CompiledRulesRetrievalMetadata retrieval)
    {
        var content = $"Test {source}#{anchor}: saved combative unreviewed prose.";
        return new() { SnippetId = source + "#" + anchor, RuleSourceId = source, SourceAnchor = anchor, Content = content,
            ContentSha256 = CompiledRulesArtifactContract.ComputeContentSha256(content), EstimatedTokens = 10, Retrieval = retrieval };
    }

    private static CompiledRulesRetrievalMetadata Terms(params (string Term, string Kind, int Weight)[] terms) => new()
    {
        Terms = terms.OrderBy(term => term.Term, StringComparer.Ordinal).ThenBy(term => term.Kind, StringComparer.Ordinal)
            .Select(term => new CompiledRulesRetrievalTerm { Term = term.Term, Kind = term.Kind, Weight = term.Weight }).ToList()
    };

    private static CompiledRulesArtifact Seal(CompiledRulesArtifact artifact) => new()
    {
        ArtifactFormatVersion = artifact.ArtifactFormatVersion, Compiler = artifact.Compiler, Ruleset = artifact.Ruleset,
        RuleSources = artifact.RuleSources, Snippets = artifact.Snippets,
        Integrity = new() { Algorithm = "SHA-256", ArtifactSha256 = CompiledRulesArtifactContract.ComputeArtifactSha256(artifact) }
    };

    private static CompiledRuleRetrievalRequest Request(CompiledRulesArtifact artifact, params string[] terms) =>
        new(new(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256), "gameplay.resolve", "NORMAL", terms);

    private static CompiledRuleCandidateSet Find(CompiledRulesArtifact artifact, params string[] terms) =>
        CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(Request(artifact, terms)));

    private static IEnumerable<string> Ids(CompiledRuleCandidateSet result) => result.Candidates.Select(candidate => candidate.Snippet.SnippetId);
    private static string Evidence(CompiledRuleCandidateSet result) => JsonSerializer.Serialize(new { result.Scope, result.Counts,
        Candidates = result.Candidates.Select(candidate => new { candidate.Snippet.SnippetId, candidate.Reasons, candidate.Matches, candidate.ApplicabilitySatisfied }) });

    private static void SafeFailure(Action action, CompiledRuleRetrievalFailure failure)
    {
        var exception = Assert.Throws<CompiledRuleRetrievalException>(action);
        Assert.Equal(failure, exception.Failure);
        Assert.InRange(exception.Message.Length, 1, 128);
        Assert.StartsWith("RULE_RETRIEVAL_", exception.Code);
        Assert.Null(exception.InnerException);
    }

    private sealed class CancellingList<T>(IList<T> values, CancellationTokenSource cancellation) : IList<T>
    {
        public IEnumerator<T> GetEnumerator() { cancellation.Cancel(); return values.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => values.Count;
        public bool IsReadOnly => true;
        // LINQ may traverse IList by index rather than its enumerator.
        public T this[int index] { get { cancellation.Cancel(); return values[index]; } set => throw new NotSupportedException(); }
        public bool Contains(T item) => values.Contains(item);
        public void CopyTo(T[] array, int index) => values.CopyTo(array, index);
        public int IndexOf(T item) => values.IndexOf(item);
        public void Add(T item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public void Insert(int index, T item) => throw new NotSupportedException();
        public bool Remove(T item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
    }
}
