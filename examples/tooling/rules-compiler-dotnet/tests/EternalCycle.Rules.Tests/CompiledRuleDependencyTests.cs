using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRuleDependencyTests(ITestOutputHelper output)
{
    private static readonly byte[] CanonicalBytes = CompiledRulesImportFixtures.Canonical();
    private static readonly CompiledRulesArtifact Canonical = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(CanonicalBytes)).Artifact!;
    private static readonly CompiledRuleCandidateIndex CanonicalIndex = CompiledRuleCandidateIndex.Create(Canonical);

    [Fact]
    public void NoDependenciesPreserveEveryRankedRootAndNeverAdmitUnmatchedSiblings()
    {
        var ranked = Select(Fixture(new("a", Weight: 900, Snippets: 2), new("z")));
        var result = CompiledRuleDependencies.Expand(ranked);
        Assert.Equal(ranked.Candidates.Select(root => root.SnippetId).Order(StringComparer.Ordinal), Ids(result.Members).Order(StringComparer.Ordinal));
        Assert.All(result.Roots, root => Assert.Empty(root.Prerequisites));
        Assert.All(result.Members, member => { Assert.True(member.IsRoot); Assert.False(member.IsDependency); });
        Assert.DoesNotContain(result.Members, member => member.SnippetId == "a#two");
        PreserveRoots(ranked, result);
    }

    [Fact]
    public void DirectSourceDependencyIncludesAllItsSnippetsWithZeroRelevance()
    {
        var ranked = Only(Select(Fixture(new("a", ["b"], 900), new("b", Snippets: 2))), "a");
        var result = CompiledRuleDependencies.Expand(ranked);
        Assert.Equal(new[] { "b#one", "b#two", "a#one" }, Ids(result.Members));
        Assert.Equal(new[] { "b#one", "b#two" }, Ids(result.Roots[0].Prerequisites));
        foreach (var member in result.Members.Where(member => !member.IsRoot))
        {
            Assert.Equal(0, member.VocabularyScore);
            Assert.Equal(CompiledRuleCandidateReason.None, member.Reasons);
            Assert.Null(member.Root);
            Assert.True(member.IsDependency);
            Assert.Equal(new[] { "a" }, member.RequiredByRuleSourceIds);
        }
        PreserveRoots(ranked, result);
    }

    [Fact]
    public void TransitiveChainIsCompleteAndPrerequisiteFirstWithoutEnumeratingPaths()
    {
        var result = Expand(Fixture(new("a", ["b"], 900), new("b", ["c"]), new("c", ["d"]), new("d")), "a");
        Assert.Equal(new[] { "d#one", "c#one", "b#one", "a#one" }, Ids(result.Members));
        Assert.Equal(new[] { "d#one", "c#one", "b#one" }, Ids(result.Roots[0].Prerequisites));
        Assert.Equal(new[] { "c" }, result.Members[0].RequiredByRuleSourceIds);
        Assert.Equal(new[] { "b" }, result.Members[1].RequiredByRuleSourceIds);
        Assert.Equal(new[] { "a" }, result.Members[2].RequiredByRuleSourceIds);
    }

    [Fact]
    public void DiamondRetainsBothDirectParentsAndDeduplicatesSharedMember()
    {
        var result = Expand(Fixture(new("a", ["b", "c"], 900), new("b", ["d"]), new("c", ["d"]), new("d")), "a");
        Assert.Equal(new[] { "d#one", "b#one", "c#one", "a#one" }, Ids(result.Members));
        Assert.Equal(new[] { "b", "c" }, result.Members[0].RequiredByRuleSourceIds);
        Assert.Equal(4, result.Members.Count);
        Assert.Equal(3, result.Roots[0].Prerequisites.Count);
    }

    [Fact]
    public void SharedDependencyDoesNotReorderRankedRootsOrGainDuplicateScore()
    {
        var ranked = Only(Select(Fixture(new("a", ["c"], 700), new("b", ["c"], 900), new("c"))), "a", "b");
        var result = CompiledRuleDependencies.Expand(ranked);
        Assert.Equal(new[] { "b#one", "a#one" }, result.Roots.Select(root => root.Root.SnippetId));
        Assert.Equal(new[] { "c#one", "b#one", "a#one" }, Ids(result.Members));
        Assert.Equal(new[] { "a", "b" }, result.Members[0].RequiredByRuleSourceIds);
        Assert.Equal(0, result.Members[0].VocabularyScore);
        Assert.Same(result.Roots[0].Prerequisites[0], result.Roots[1].Prerequisites[0]);
        PreserveRoots(ranked, result);
    }

    [Fact]
    public void RootAlsoDependencyRetainsGenuineScoreAndAddsUnmatchedSourceSiblings()
    {
        var ranked = Only(Select(Fixture(new("a", ["b"], 900), new("b", Weight: 700, Snippets: 2))), "a", "b");
        var result = CompiledRuleDependencies.Expand(ranked);
        var b = Assert.Single(result.Members, member => member.SnippetId == "b#one");
        Assert.True(b.IsRoot);
        Assert.True(b.IsDependency);
        Assert.Equal(700, b.VocabularyScore);
        Assert.Equal(new[] { "a" }, b.RequiredByRuleSourceIds);
        Assert.Equal(new[] { "b#one", "b#two", "a#one" }, Ids(result.Members));
        Assert.False(result.Members[1].IsRoot);
        Assert.Equal(new[] { "a#one", "b#one" }, result.Roots.Select(root => root.Root.SnippetId));
        PreserveRoots(ranked, result);
    }

    [Theory]
    [InlineData("mandatory")]
    [InlineData("explicit-source")]
    [InlineData("explicit-snippet")]
    [InlineData("always-include")]
    [InlineData("procedure")]
    public void EveryQueryIndependentRootExpandsWithoutPassingItsRootFlagsToDependencies(string kind)
    {
        var rootId = kind switch { "mandatory" => "runtime-kernel", "procedure" => "gm-runtime-procedure", _ => "a" };
        var rootMetadata = new CompiledRulesArtifactApplicability
        {
            Layer = kind == "mandatory" ? RuleLayer.RuntimeKernel : RuleLayer.Core,
            AlwaysInclude = kind is "always-include" or "procedure",
            Operations = kind == "procedure" ? ["gameplay.resolve"] : []
        };
        var artifact = Fixture(new(rootId, ["b"], Metadata: rootMetadata), new("b"));
        var request = Request(artifact) with
        {
            QueryTerms = [], Operation = kind == "procedure" ? "gameplay.resolve" : "context.assemble",
            RequiredRuleSourceIds = kind == "explicit-source" ? [rootId] : [],
            RequiredSnippetIds = kind == "explicit-snippet" ? [rootId + "#one"] : []
        };
        var result = CompiledRuleDependencies.Expand(Select(artifact, request));
        var parent = Assert.Single(result.Roots, root => root.Root.RuleSourceId == rootId);
        Assert.Equal(0, parent.Root.VocabularyScore);
        var child = Assert.Single(parent.Prerequisites);
        Assert.False(child.IsRoot);
        Assert.Equal(CompiledRuleCandidateReason.None, child.Reasons);
        Assert.Equal(new[] { rootId }, child.RequiredByRuleSourceIds);
    }

    [Theory]
    [InlineData("topics", true)]
    [InlineData("world", false)]
    [InlineData("world-selected", true)]
    [InlineData("world-wildcard", false)]
    [InlineData("world-wildcard-selected", true)]
    [InlineData("module", false)]
    [InlineData("module-selected", true)]
    [InlineData("module-wildcard", false)]
    [InlineData("module-wildcard-selected", true)]
    [InlineData("mode", false)]
    [InlineData("mode-selected", true)]
    [InlineData("mode-wildcard", true)]
    [InlineData("operation", false)]
    [InlineData("operation-selected", true)]
    [InlineData("operation-wildcard", true)]
    public void DependenciesIgnoreQueryTopicsButCannotBypassExecutableCompatibility(string selector, bool compatible)
    {
        var metadata = new CompiledRulesArtifactApplicability
        {
            Layer = RuleLayer.Core,
            WorldModelIds = selector.StartsWith("world", StringComparison.Ordinal) ? [selector.Contains("wildcard") ? "*" : "world"] : [],
            ModuleIds = selector.StartsWith("module", StringComparison.Ordinal) ? [selector.Contains("wildcard") ? "*" : "module"] : [],
            CampaignModes = selector.StartsWith("mode", StringComparison.Ordinal) ? [selector.Contains("wildcard") ? "*" : "HARD"] : [],
            Operations = selector.StartsWith("operation", StringComparison.Ordinal) ? [selector.Contains("wildcard") ? "*" : "persistence.read"] : [],
            Topics = ["unqueried"]
        };
        var artifact = Fixture(new("a", ["b"], 900), new("b", Metadata: metadata));
        var request = Request(artifact) with
        {
            WorldModelId = selector.Contains("world") && selector.EndsWith("selected") ? "WORLD" : null,
            ModuleIds = selector.Contains("module") && selector.EndsWith("selected") ? ["MODULE"] : [],
            CampaignMode = selector == "mode-selected" ? "HARD" : "NORMAL",
            Operation = selector == "operation-selected" ? "persistence.read" : "context.assemble"
        };
        var ranked = Select(artifact, request);
        Assert.DoesNotContain(ranked.Candidates, root => root.SnippetId == "b#one");
        if (!compatible) SafeFailure(() => CompiledRuleDependencies.Expand(ranked), CompiledRuleRetrievalFailure.DependencyFailed);
        else
        {
            var b = Assert.Single(CompiledRuleDependencies.Expand(ranked).Members, member => member.SnippetId == "b#one");
            Assert.False(b.IsRoot);
            Assert.Equal(0, b.VocabularyScore);
        }
    }

    [Fact]
    public void QueryHitExcludedByTopicCanReturnOnlyAsADeclaredDependency()
    {
        var artifact = Fixture(new("a", ["b"], 900), new("b", Weight: 800, Metadata: new() { Layer = RuleLayer.Core, Topics = ["other"] }));
        var ranked = Select(artifact);
        Assert.Equal(1, ranked.Counts.InapplicableVocabularyMatched);
        Assert.DoesNotContain(ranked.Candidates, root => root.SnippetId == "b#one");
        var b = Assert.Single(CompiledRuleDependencies.Expand(ranked).Members, member => member.SnippetId == "b#one");
        Assert.True(b.IsDependency);
        Assert.False(b.IsRoot);
        Assert.Null(b.Root);
        Assert.Equal(0, b.VocabularyScore);
    }

    [Fact]
    public void TokenBudgetAndPreparationTierCannotTruncateClosureOrChangeRank()
    {
        var artifact = Fixture(new("a", ["b"], 900), new("b", Metadata: new() { Layer = RuleLayer.Core, PreparationTier = RulePreparationTier.OptionalRare }));
        var request = Request(artifact) with { MaximumEstimatedTokens = 1 };
        var first = CompiledRuleDependencies.Expand(Select(artifact, request));
        var second = CompiledRuleDependencies.Expand(Select(artifact, request with { MaximumEstimatedTokens = 8000 }));
        Assert.Equal(Evidence(first), Evidence(second));
        Assert.Contains(first.Members, member => member.SnippetId == "b#one");
        Assert.True(first.Members.Sum(member => member.Snippet.EstimatedTokens) > 1);
    }

    [Fact]
    public void DeepValidatedChainUsesIterativeTraversalAndCompleteReadOnlyPrerequisites()
    {
        const int length = 4096;
        var definitions = Enumerable.Range(0, length).Select(index => new Definition($"s{index:D4}",
            index + 1 < length ? [$"s{index + 1:D4}"] : [], index == 0 ? 900 : 0)).ToArray();
        var result = Expand(Fixture(definitions), "s0000");
        Assert.Equal(length, result.Members.Count);
        Assert.Equal(length - 1, result.Roots[0].Prerequisites.Count);
        Assert.Equal("s4095#one", result.Members[0].SnippetId);
        Assert.Equal("s0000#one", result.Members[^1].SnippetId);
        Assert.Equal(length - 1, result.Members.Sum(member => member.RequiredByRuleSourceIds.Count));
        Assert.All(result.Members.Take(length - 1), member => Assert.Equal(0, member.VocabularyScore));
    }

    [Fact]
    public void DenseGraphEvidenceIsBoundedByDirectEdgesRatherThanNumberOfPaths()
    {
        const int width = 8;
        const int layers = 8;
        var definitions = new List<Definition> { new("a", Enumerable.Range(0, width).Select(i => $"l0-n{i}").ToArray(), 900) };
        for (var layer = 0; layer < layers; layer++)
            for (var node = 0; node < width; node++)
                definitions.Add(new($"l{layer}-n{node}", layer + 1 == layers ? [] :
                    Enumerable.Range(0, width).Select(i => $"l{layer + 1}-n{i}").ToArray()));
        var result = Expand(Fixture(definitions.ToArray()), "a");
        Assert.Equal(1 + width * layers, result.Members.Count);
        Assert.Equal(width + (layers - 1) * width * width, result.Members.Sum(member => member.RequiredByRuleSourceIds.Count));
        Assert.Equal(width, result.Members.Max(member => member.RequiredByRuleSourceIds.Count));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("cycle")]
    [InlineData("self")]
    [InlineData("empty")]
    [InlineData("malformed")]
    [InlineData("duplicate")]
    [InlineData("empty-source")]
    public void DefensiveGraphCorruptionFailsWithoutPartialSuccessOrCrossResolution(string corruption)
    {
        var ranked = Select(Fixture(new("a", ["b"], 900), new("b", ["c"]), new("c")));
        var original = ranked.Input.Artifact!;
        var target = original.RuleSources.Single(source => source.RuleSourceId == "b");
        var deps = corruption switch
        {
            "missing" => new[] { "outside" }, "cycle" => ["a"], "self" => ["b"], "empty" => [""],
            "malformed" => ["../private?secret"], "duplicate" => ["c", "c"], _ => ["c"]
        };
        var sources = original.RuleSources.Select(source => source == target ? new CompiledRulesArtifactRuleSource
        {
            RuleSourceId = source.RuleSourceId, SourcePath = source.SourcePath, SourceSha256 = source.SourceSha256,
            Applicability = source.Applicability, DependencyRuleSourceIds = deps
        } : source).ToArray();
        var artifact = Copy(original, sources, original.Snippets.Where(snippet => corruption != "empty-source" || snippet.RuleSourceId != "c").ToArray());
        SafeFailure(() => CompiledRuleDependencies.Expand(artifact, ranked), CompiledRuleRetrievalFailure.DependencyFailed);
    }

    [Theory]
    [InlineData("scope")]
    [InlineData("absent-root")]
    [InlineData("same-id-different-source")]
    [InlineData("same-id-different-snippet")]
    [InlineData("duplicate-root")]
    [InlineData("missing-snapshot")]
    public void ArtifactAndRootAuthorityCannotBeMixed(string corruption)
    {
        var ranked = Only(Select(Fixture(new("a", ["b"], 900), new("b"))), "a");
        var artifact = ranked.Input.Artifact!;
        if (corruption == "missing-snapshot")
        {
            var detached = new RankedCompiledRuleCandidateSet(new(ranked.Input.Request, ranked.Input.Candidates, ranked.Counts), ranked.Candidates);
            SafeFailure(() => CompiledRuleDependencies.Expand(detached), CompiledRuleRetrievalFailure.ArtifactInconsistent);
            return;
        }
        if (corruption == "duplicate-root") ranked = new(ranked.Input, [ranked.Candidates[0], ranked.Candidates[0]]);
        if (corruption == "scope") artifact = new()
        {
            Ruleset = artifact.Ruleset, Integrity = new() { ArtifactSha256 = new string('F', 64) },
            RuleSources = artifact.RuleSources, Snippets = artifact.Snippets
        };
        if (corruption == "absent-root") artifact = Copy(artifact, artifact.RuleSources, artifact.Snippets.Where(s => s.RuleSourceId != "a").ToArray());
        if (corruption == "same-id-different-source") artifact = Copy(artifact, artifact.RuleSources.Select(source => source.RuleSourceId == "a"
            ? new CompiledRulesArtifactRuleSource { RuleSourceId = "a", SourceSha256 = new string('C', 64) } : source).ToArray(), artifact.Snippets);
        if (corruption == "same-id-different-snippet") artifact = Copy(artifact, artifact.RuleSources, artifact.Snippets.Select(snippet => snippet.RuleSourceId == "a"
            ? new CompiledRulesArtifactSnippet { SnippetId = snippet.SnippetId, RuleSourceId = "a", Content = "Other artifact" } : snippet).ToArray());
        SafeFailure(() => CompiledRuleDependencies.Expand(artifact, ranked), corruption == "scope"
            ? CompiledRuleRetrievalFailure.RequestInvalid : CompiledRuleRetrievalFailure.ArtifactInconsistent);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    [InlineData("fr-FR")]
    public void CultureEnumerationAndRepeatedExpansionDoNotChangeEvidence(string culture)
    {
        var ranked = Only(Select(Fixture(new("a", ["b"], 900), new("b", ["c", "d"]), new("c"), new("d"))), "a");
        var expected = Evidence(CompiledRuleDependencies.Expand(ranked));
        var original = ranked.Input.Artifact!;
        var shuffled = Copy(original, original.RuleSources.Reverse().ToArray(), original.Snippets.Reverse().ToArray());
        var originalA = original.RuleSources.Single(source => source.RuleSourceId == "a");
        // Non-root dependency enumeration is reversed without changing authority
        // references. The internal defensive seam permits noncanonical order.
        var b = original.RuleSources.Single(source => source.RuleSourceId == "b");
        shuffled = Copy(shuffled, shuffled.RuleSources.Select(source => source == b ? new CompiledRulesArtifactRuleSource
        { RuleSourceId = b.RuleSourceId, Applicability = b.Applicability, DependencyRuleSourceIds = b.DependencyRuleSourceIds.Reverse().ToArray() } : source).ToArray(), shuffled.Snippets);
        Assert.Same(originalA, shuffled.RuleSources.Single(source => source.RuleSourceId == "a"));
        var previous = CultureInfo.CurrentCulture;
        var ui = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            Assert.Equal(expected, Evidence(CompiledRuleDependencies.Expand(shuffled, ranked)));
            Assert.Equal(expected, Evidence(CompiledRuleDependencies.Expand(ranked)));
        }
        finally { CultureInfo.CurrentCulture = previous; CultureInfo.CurrentUICulture = ui; }
    }

    [Fact]
    public void SourceSiblingRootsSharePrerequisiteListsAndAllResultCollectionsAreReadOnly()
    {
        var artifact = Fixture(new("a", ["b"], 900, Snippets: 2), new("b", ["c"]), new("c"));
        var ranked = Select(artifact, Request(artifact) with { RequiredRuleSourceIds = ["a"] });
        var result = CompiledRuleDependencies.Expand(ranked);
        var before = Evidence(result);
        artifact.RuleSources[0].DependencyRuleSourceIds.Clear();
        artifact.RuleSources.Clear();
        artifact.Snippets.Clear();
        Assert.Equal(before, Evidence(CompiledRuleDependencies.Expand(ranked)));
        var aRoots = result.Roots.Where(root => root.Root.RuleSourceId == "a").ToArray();
        Assert.Same(aRoots[0].Prerequisites, aRoots[1].Prerequisites);
        Assert.Throws<NotSupportedException>(() => ((IList<CompiledRuleClosureMember>)result.Members).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<CompiledRuleRootClosure>)result.Roots).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<CompiledRuleClosureMember>)aRoots[0].Prerequisites).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<string>)aRoots[0].Prerequisites[0].RequiredByRuleSourceIds).Clear());
        Assert.Throws<NotSupportedException>(() => aRoots[0].Root.Source.DependencyRuleSourceIds.Clear());
        Assert.Same(ranked, result.Input);
        PreserveRoots(ranked, result);
        Assert.Equal(before, Evidence(result));
        Assert.DoesNotContain("Executable", before);
        Assert.DoesNotContain("rules/", before);
        Assert.DoesNotContain("QueryTerms", before);
    }

    [Theory]
    [InlineData("before")]
    [InlineData("sources")]
    [InlineData("snippets")]
    [InlineData("roots")]
    [InlineData("dependencies")]
    public void CancellationAtWorkBoundariesNeverReturnsPartialSuccess(string phase)
    {
        var ranked = Select(Fixture(new("a", ["b"], 900), new("b")));
        var artifact = ranked.Input.Artifact!;
        using var cancellation = new CancellationTokenSource();
        if (phase == "before") cancellation.Cancel();
        if (phase == "sources") artifact = Copy(artifact, new CancellingList<CompiledRulesArtifactRuleSource>(artifact.RuleSources, cancellation), artifact.Snippets);
        if (phase == "snippets") artifact = Copy(artifact, artifact.RuleSources, new CancellingList<CompiledRulesArtifactSnippet>(artifact.Snippets, cancellation));
        if (phase == "roots") ranked = new(ranked.Input, new CancellingList<RankedCompiledRuleCandidate>(ranked.Candidates.ToArray(), cancellation));
        if (phase == "dependencies")
        {
            var b = artifact.RuleSources.Single(source => source.RuleSourceId == "b");
            artifact = Copy(artifact, artifact.RuleSources.Select(source => source == b ? new CompiledRulesArtifactRuleSource
            { RuleSourceId = "b", Applicability = b.Applicability, DependencyRuleSourceIds = new CancellingList<string>(b.DependencyRuleSourceIds, cancellation) } : source).ToArray(), artifact.Snippets);
        }
        Assert.Equal(cancellation.Token, Assert.Throws<OperationCanceledException>(() =>
            CompiledRuleDependencies.Expand(artifact, ranked, cancellation.Token)).CancellationToken);
    }

    [Theory]
    [InlineData("fighting", "gameplay.resolve", 15, false)]
    [InlineData("conflict", "gameplay.resolve", 15, false)]
    [InlineData("save", "gameplay.resolve", 12, true)]
    [InlineData("preparation", "gameplay.resolve", 14, false)]
    [InlineData("campaign canon", "context.assemble", 7, false)]
    [InlineData("unreviewed", "gameplay.resolve", 11, false)]
    [InlineData("", "gameplay.resolve", 11, false)]
    [InlineData("fighting|conflict|save|preparation", "gameplay.resolve", 19, true)]
    public void CanonicalQueriesKeepScoresAndFailClosedForIncompatibleDeclaredPrerequisite(string terms, string operation, int count, bool fails)
    {
        var request = Request(Canonical) with
        {
            Operation = operation, QueryTerms = terms.Length == 0 ? [] : terms.Split('|'),
            Topics = Canonical.RuleSources.SelectMany(source => source.Applicability.Topics).Distinct().ToArray()
        };
        var ranked = CompiledRuleRanking.Rank(CanonicalIndex.FindCandidates(CompiledRuleRetrieval.Prepare(request)));
        var before = JsonSerializer.Serialize(ranked);
        Assert.Equal(count, ranked.Candidates.Count);
        if (fails)
        {
            Assert.Equal(600, ranked.Candidates.Single(root => root.SnippetId == "core-context-assembly#manual-persistence-commands").VocabularyScore);
            SafeFailure(() => CompiledRuleDependencies.Expand(ranked), CompiledRuleRetrievalFailure.DependencyFailed);
            output.WriteLine($"QUERY {terms}: {count} roots; RULE_RETRIEVAL_DEPENDENCY_FAILED; no successful partial closure.");
        }
        else
        {
            var result = CompiledRuleDependencies.Expand(ranked);
            Assert.Equal(count, result.Members.Count);
            Assert.All(result.Members, member => Assert.True(member.IsRoot));
            PreserveRoots(ranked, result);
            if (operation == "gameplay.resolve")
            {
                Assert.Equal(5, result.Members.Count(member => member.IsDependency));
                Assert.All(result.Members.Where(member => member.IsDependency), member =>
                    Assert.Equal(new[] { "gm-runtime-procedure" }, member.RequiredByRuleSourceIds));
            }
            if (terms is "" or "unreviewed") Assert.All(result.Members, member => Assert.Equal(0, member.VocabularyScore));
            if (terms == "conflict") Assert.Equal(5, result.Input.Counts.InapplicableVocabularyMatched);
            Assert.Equal(Evidence(result), Evidence(CompiledRuleDependencies.Expand(ranked)));
            output.WriteLine($"QUERY {terms}: roots={count} members={result.Members.Count} dependencyOnly=0 requiredRoots={result.Members.Count(m => m.IsDependency)}");
        }
        Assert.Equal(before, JsonSerializer.Serialize(ranked));
        output.WriteLine(string.Join("; ", ranked.Candidates.Select(root => $"{root.SnippetId}:{root.VocabularyScore}")));
    }

    [Fact]
    public void CanonicalGraphInventoryAndCompiledArtifactRemainUnchanged()
    {
        Assert.Equal(3, Canonical.RuleSources.Count(source => source.DependencyRuleSourceIds.Count != 0));
        Assert.Equal(3, Canonical.RuleSources.Sum(source => source.DependencyRuleSourceIds.Count));
        Assert.Equal(1, Canonical.RuleSources.Max(source => source.DependencyRuleSourceIds.Count));
        Assert.All(Canonical.RuleSources.SelectMany(source => source.DependencyRuleSourceIds), id =>
            Assert.Empty(Canonical.RuleSources.Single(source => source.RuleSourceId == id).DependencyRuleSourceIds));
        Assert.Equal(2, Canonical.RuleSources.Count(source => source.DependencyRuleSourceIds.Contains("runtime-kernel")));
        Assert.Equal(10, Canonical.RuleSources.Count);
        Assert.Equal(154, Canonical.Snippets.Count);
        Assert.Equal(773, Canonical.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count));
        Assert.Equal(275622, CanonicalBytes.Length);
        Assert.Empty(CompiledRulesArtifactContract.Validate(Canonical));
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", Canonical.Integrity.ArtifactSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", Convert.ToHexString(SHA256.HashData(CanonicalBytes)));
        Assert.Equal(CanonicalBytes, CompiledRulesArtifactWriter.Write(Canonical).Bytes.ToArray());
    }

    private sealed record Definition(string Id, string[]? Dependencies = null, int Weight = 0,
        CompiledRulesArtifactApplicability? Metadata = null, int Snippets = 1);

    private static CompiledRulesArtifact Fixture(params Definition[] definitions)
    {
        var all = definitions.ToList();
        if (all.All(definition => definition.Id != "runtime-kernel")) all.Add(new("runtime-kernel", Metadata: new() { Layer = RuleLayer.RuntimeKernel }));
        var sources = all.OrderBy(definition => definition.Id, StringComparer.Ordinal).Select(definition => new CompiledRulesArtifactRuleSource
        {
            RuleSourceId = definition.Id, SourcePath = $"rules/{definition.Id}.md", SourceSha256 = new string('A', 64),
            Applicability = definition.Metadata ?? new() { Layer = RuleLayer.Core },
            DependencyRuleSourceIds = (definition.Dependencies ?? []).Order(StringComparer.Ordinal).ToList()
        }).ToArray();
        var snippets = all.SelectMany(definition => Enumerable.Range(0, definition.Snippets).Select(index => new CompiledRulesArtifactSnippet
        {
            SnippetId = definition.Id + (index == 0 ? "#one" : "#two"), RuleSourceId = definition.Id, SourceAnchor = index == 0 ? "one" : "two",
            Content = "Executable fixture text.", ContentSha256 = CompiledRulesArtifactContract.ComputeContentSha256("Executable fixture text."), EstimatedTokens = 10,
            Retrieval = new() { Terms = index == 0 && definition.Weight > 0 ? [new() { Term = "select", Kind = "selection", Weight = definition.Weight }] : [] }
        })).OrderBy(snippet => snippet.SnippetId, StringComparer.Ordinal).ToArray();
        var artifact = new CompiledRulesArtifact
        {
            ArtifactFormatVersion = 1, Compiler = new() { ContractVersion = "1", ImplementationId = "fixture", ImplementationVersion = "1.0.0" },
            Ruleset = new() { RulesetId = "fixture", RepositoryVersion = "1.0.0", Source = new() { Scheme = "snapshot", Value = "immutable" },
                ManifestPath = "rules/manifest.json", ManifestSha256 = new string('B', 64) },
            Integrity = new() { Algorithm = "SHA-256" }, RuleSources = sources, Snippets = snippets
        };
        return new()
        {
            ArtifactFormatVersion = artifact.ArtifactFormatVersion, Compiler = artifact.Compiler, Ruleset = artifact.Ruleset,
            RuleSources = sources.ToList(), Snippets = snippets.ToList(),
            Integrity = new() { Algorithm = "SHA-256", ArtifactSha256 = CompiledRulesArtifactContract.ComputeArtifactSha256(artifact) }
        };
    }

    private static CompiledRuleRetrievalRequest Request(CompiledRulesArtifact artifact) => new(
        new(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256), "context.assemble", "NORMAL", ["select"]);
    private static RankedCompiledRuleCandidateSet Select(CompiledRulesArtifact artifact, CompiledRuleRetrievalRequest? request = null) =>
        CompiledRuleRanking.Rank(CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(request ?? Request(artifact))));
    private static RankedCompiledRuleCandidateSet Only(RankedCompiledRuleCandidateSet ranked, params string[] sources) =>
        new(ranked.Input, Array.AsReadOnly(ranked.Candidates.Where(root => sources.Contains(root.Candidate.Source.RuleSourceId, StringComparer.Ordinal)).ToArray()));
    private static CompiledRuleDependencyClosure Expand(CompiledRulesArtifact artifact, params string[] sources) => CompiledRuleDependencies.Expand(Only(Select(artifact), sources));
    private static CompiledRulesArtifact Copy(CompiledRulesArtifact original, IList<CompiledRulesArtifactRuleSource> sources, IList<CompiledRulesArtifactSnippet> snippets) => new()
    { Ruleset = original.Ruleset, Integrity = original.Integrity, RuleSources = sources, Snippets = snippets };
    private static IEnumerable<string> Ids(IEnumerable<CompiledRuleClosureMember> members) => members.Select(member => member.SnippetId);
    private static string Evidence(CompiledRuleDependencyClosure result) => JsonSerializer.Serialize(result);
    private static void PreserveRoots(RankedCompiledRuleCandidateSet ranked, CompiledRuleDependencyClosure result)
    {
        Assert.Equal(ranked.Candidates.Select(root => root.SnippetId), result.Roots.Select(root => root.Root.SnippetId));
        for (var index = 0; index < ranked.Candidates.Count; index++)
        {
            Assert.Same(ranked.Candidates[index], result.Roots[index].Root.Root);
            Assert.Same(ranked.Candidates[index].Matches, result.Roots[index].Root.Root!.Matches);
            Assert.Equal(ranked.Candidates[index].VocabularyScore, result.Roots[index].Root.VocabularyScore);
            Assert.Equal(ranked.Candidates[index].Reasons, result.Roots[index].Root.Reasons);
        }
    }
    private static void SafeFailure(Action action, CompiledRuleRetrievalFailure expected)
    {
        var exception = Assert.Throws<CompiledRuleRetrievalException>(action);
        Assert.Equal(expected, exception.Failure);
        Assert.InRange(exception.Message.Length, 1, 128);
        Assert.Null(exception.InnerException);
        Assert.DoesNotContain("secret", exception.Message);
    }

    private sealed class CancellingList<T>(IList<T> values, CancellationTokenSource cancellation) : IList<T>, IReadOnlyList<T>
    {
        public int Count => values.Count;
        public bool IsReadOnly => true;
        public T this[int index] { get => values[index]; set => throw new NotSupportedException(); }
        public IEnumerator<T> GetEnumerator() { cancellation.Cancel(); return values.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int IndexOf(T item) => values.IndexOf(item);
        public bool Contains(T item) => values.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => values.CopyTo(array, arrayIndex);
        public void Add(T item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public void Insert(int index, T item) => throw new NotSupportedException();
        public bool Remove(T item) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
    }
}
