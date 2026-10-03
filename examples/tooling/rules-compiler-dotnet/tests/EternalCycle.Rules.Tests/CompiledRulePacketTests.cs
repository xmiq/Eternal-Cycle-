using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRulePacketTests(ITestOutputHelper output)
{
    private static readonly byte[] CanonicalBytes = CompiledRulesImportFixtures.Canonical();
    private static readonly CompiledRulesArtifact Canonical = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(CanonicalBytes)).Artifact!;
    private static readonly CompiledRuleCandidateIndex CanonicalIndex = CompiledRuleCandidateIndex.Create(Canonical);

    [Theory]
    [InlineData(20, false)]
    [InlineData(21, true)]
    [InlineData(22, true)]
    public void DirectClosureFitsOnlyAsAWholeIncludingExactBoundary(int budget, bool fits)
    {
        var result = Build(Fixture(new("a", ["b"], 900), new("b")), budget);
        Assert.Equal(fits ? new[] { "b#one", "a#one", "runtime-kernel#one" } : new[] { "runtime-kernel#one" }, Ids(result));
        Assert.Equal(fits ? 21 : 1, result.Totals.UsedEstimatedTokens);
        Assert.Equal(fits ? 0 : 1, result.Totals.BudgetExcludedRoots);
        Assert.Equal(fits ? 1 : 0, result.Totals.DependencyOnlyMembers);
        Accounting(result);
    }

    [Theory]
    [InlineData(40, false)]
    [InlineData(41, true)]
    [InlineData(42, true)]
    public void TransitiveChainIsIndivisibleAndPrerequisiteFirst(int budget, bool fits)
    {
        var result = Build(Fixture(new("a", ["b"], 900), new("b", ["c"]), new("c", ["d"]), new("d")), budget);
        Assert.Equal(fits ? new[] { "d#one", "c#one", "b#one", "a#one", "runtime-kernel#one" } : new[] { "runtime-kernel#one" }, Ids(result));
        Assert.Equal(fits ? 41 : 1, result.Totals.UsedEstimatedTokens);
        Accounting(result);
    }

    [Theory]
    [InlineData(40, false)]
    [InlineData(41, true)]
    public void DiamondChargesSharedLeafOnceAndRetainsBothParents(int budget, bool fits)
    {
        var result = Build(Fixture(new("a", ["b", "c"], 900), new("b", ["d"]), new("c", ["d"]), new("d")), budget);
        if (fits)
        {
            Assert.Equal(new[] { "d#one", "b#one", "c#one", "a#one", "runtime-kernel#one" }, Ids(result));
            Assert.Equal(new[] { "b", "c" }, result.Members[0].RequiredByRuleSourceIds);
        }
        else Assert.Equal(new[] { "runtime-kernel#one" }, Ids(result));
        Accounting(result);
    }

    [Fact]
    public void SharedDependencyMakesLaterRootFitAtItsIncrementalCost()
    {
        var result = Build(Fixture(new("a", ["c"], 900), new("b", ["c"], 800), new("c")), 31);
        Assert.Equal(new[] { "c#one", "a#one", "b#one", "runtime-kernel#one" }, Ids(result));
        Assert.Equal(new long[] { 20, 10, 1 }, result.Decisions.Select(decision => decision.IncrementalEstimatedTokens));
        Assert.Equal(new[] { "a", "b" }, result.Members[0].RequiredByRuleSourceIds);
        Assert.Equal(31, result.Totals.UsedEstimatedTokens);
        Accounting(result);
    }

    [Fact]
    public void IndependentlyRankedDependencyRetainsItsOriginalRootWithZeroIncrementalCost()
    {
        var closure = Expand(Fixture(new("a", ["b"], 900), new("b", Weight: 800)), 21);
        var result = CompiledRulePackets.Build(closure);
        var b = Assert.Single(result.Members, member => member.RuleSourceId == "b");
        Assert.Same(closure.Input.Candidates[1], b.Root);
        Assert.Equal(800, b.Root!.VocabularyScore);
        Assert.True(b.IsDependency);
        Assert.Equal(new[] { "a" }, b.RequiredByRuleSourceIds);
        Assert.Equal(0, result.Decisions[1].IncrementalEstimatedTokens);
        Assert.Equal(0, result.Totals.DependencyOnlyMembers);
        Accounting(result);
    }

    [Fact]
    public void RejectedClosureAddsNothingAndLaterSmallerRootStillFits()
    {
        var result = Build(Fixture(new("a", ["c"], 900), new("b", Weight: 800), new("c")), 11);
        Assert.Equal(new[] { "b#one", "runtime-kernel#one" }, Ids(result));
        Assert.Equal(new long[] { 20, 10, 1 }, result.Decisions.Select(decision => decision.IncrementalEstimatedTokens));
        Assert.Equal("RULE_RETRIEVAL_CLOSURE_EXCEEDS_REMAINING_BUDGET", result.Decisions[0].ExclusionCode);
        Assert.Equal(10, result.Decisions[0].RemainingBeforeAdmission);
        Assert.DoesNotContain(result.Members, member => member.RuleSourceId == "c");
        Accounting(result);
    }

    [Fact]
    public void RejectedParentIsNotPresentedAsPacketSupportForSharedDependency()
    {
        var result = Build(Fixture(new("a", ["c"], 900, Cost: 20), new("b", ["c"], 800), new("c")), 21);
        var c = Assert.Single(result.Members, member => member.RuleSourceId == "c");
        Assert.Equal(new[] { "b" }, c.RequiredByRuleSourceIds);
        Assert.Equal(new[] { "a", "b" }, c.Member.RequiredByRuleSourceIds);
        Accounting(result);
    }

    [Theory]
    [InlineData("kernel", 20, false)]
    [InlineData("kernel", 21, true)]
    [InlineData("kernel", 22, true)]
    [InlineData("source", 20, false)]
    [InlineData("source", 21, true)]
    [InlineData("snippet", 20, false)]
    [InlineData("snippet", 21, true)]
    [InlineData("always", 20, false)]
    [InlineData("always", 21, true)]
    public void HardRequirementsCannotBeDroppedOrOverflowTheBudget(string reason, int budget, bool fits)
    {
        var artifact = Fixture(new("a", ["b"], Metadata: new()
        { Layer = reason == "kernel" ? RuleLayer.RuntimeKernel : RuleLayer.Core, AlwaysInclude = reason == "always" }), new("b"));
        var request = Request(artifact, budget) with
        {
            QueryTerms = [], RequiredRuleSourceIds = reason == "source" ? ["a"] : [],
            RequiredSnippetIds = reason == "snippet" ? ["a#one"] : []
        };
        var closure = Expand(artifact, request);
        if (!fits) SafeFailure(() => CompiledRulePackets.Build(closure), CompiledRuleRetrievalFailure.PacketBudgetInsufficient);
        else
        {
            var result = CompiledRulePackets.Build(closure);
            Assert.Equal(21, result.Totals.RequiredEstimatedTokens);
            Assert.All(result.Decisions, decision => Assert.True(decision.Required));
            Assert.Equal(3, result.Totals.UniqueMembers);
            Accounting(result);
        }
    }

    [Fact]
    public void MandatoryZeroScoresAreReservedWithoutChangingOptionalRanking()
    {
        var closure = Expand(Fixture(new("a", Weight: 900, Cost: 20), new("b", Weight: 800)), 11);
        var result = CompiledRulePackets.Build(closure);
        Assert.Equal(closure.Input.Candidates.Select(root => root.SnippetId), result.Decisions.Select(decision => decision.Root.SnippetId));
        Assert.False(result.Decisions[0].Selected);
        Assert.True(result.Decisions[1].Selected);
        Assert.True(result.Decisions[2].Required);
        for (var index = 0; index < result.Decisions.Count; index++) Assert.Same(closure.Input.Candidates[index], result.Decisions[index].Root);
        Accounting(result);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    public void OperationScopedProcedureIsRequiredWithItsKernel(int budget, bool fits)
    {
        var artifact = Fixture(new Definition("gm-runtime-procedure", ["runtime-kernel"], Cost: 1, Metadata: new() { Layer = RuleLayer.Core, AlwaysInclude = true }));
        var closure = Expand(artifact, Request(artifact, budget) with { Operation = "gameplay.resolve", QueryTerms = [] });
        if (!fits) SafeFailure(() => CompiledRulePackets.Build(closure), CompiledRuleRetrievalFailure.PacketBudgetInsufficient);
        else
        {
            var result = CompiledRulePackets.Build(closure);
            Assert.Equal(new[] { "runtime-kernel#one", "gm-runtime-procedure#one" }, Ids(result));
            Assert.True(result.Decisions.Single(decision => decision.Root.SnippetId == "gm-runtime-procedure#one").Root.Reasons.HasFlag(CompiledRuleCandidateReason.GmRuntimeProcedure));
            Accounting(result);
        }
    }

    [Fact]
    public void SmallestLegalBudgetCanReturnOnlyItsOneTokenKernel()
    {
        var result = Build(Fixture(new Definition("a", Weight: 900)), 1);
        Assert.Equal(new[] { "runtime-kernel#one" }, Ids(result));
        Assert.Equal(0, result.Totals.RemainingEstimatedTokens);
        Accounting(result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HugeValidCostsCannotWrapIntoAnAffordableClosure(bool required)
    {
        var artifact = Fixture(new("a", ["b"], 900, Cost: int.MaxValue, Metadata: new() { Layer = RuleLayer.Core, AlwaysInclude = required }), new("b", Cost: int.MaxValue));
        var closure = Expand(artifact, 8000);
        if (required) SafeFailure(() => CompiledRulePackets.Build(closure), CompiledRuleRetrievalFailure.PacketBudgetInsufficient);
        else
        {
            var result = CompiledRulePackets.Build(closure);
            Assert.Equal(4294967294L, result.Decisions[0].IncrementalEstimatedTokens);
            Assert.Equal(1, result.Totals.UsedEstimatedTokens);
            Accounting(result);
        }
    }

    [Theory]
    [InlineData("scope")]
    [InlineData("missing-snapshot")]
    [InlineData("foreign-member")]
    [InlineData("missing-member")]
    [InlineData("duplicate-member")]
    [InlineData("duplicate-prerequisite")]
    [InlineData("missing-prerequisite")]
    [InlineData("foreign-root")]
    [InlineData("root-order")]
    [InlineData("presentation-order")]
    [InlineData("unowned-member")]
    public void MixedOrMalformedClosureIsRejectedWithoutEchoingInputs(string corruption)
    {
        var original = Expand(Fixture(new("a", ["b"], 900), new("b"), new("z", Weight: 800)), 8000);
        var ranked = original.Input;
        IReadOnlyList<CompiledRuleClosureMember> members = original.Members;
        IReadOnlyList<CompiledRuleRootClosure> roots = original.Roots;
        var root = roots[0];
        if (corruption == "scope")
        {
            var request = Request(ranked.Input.Artifact!, 8000) with { Scope = new("fixture", new string('F', 64)) };
            ranked = new(new(CompiledRuleRetrieval.Prepare(request), ranked.Input.Candidates, ranked.Counts, ranked.Input.Artifact), ranked.Candidates);
        }
        if (corruption == "missing-snapshot") ranked = new(new(ranked.Input.Request, ranked.Input.Candidates, ranked.Counts), ranked.Candidates);
        if (corruption == "foreign-member") members = members.Select(member => member == root.Prerequisites[0]
            ? new CompiledRuleClosureMember(new() { RuleSourceId = member.RuleSourceId }, member.Snippet, member.Root, member.RequiredByRuleSourceIds) : member).ToArray();
        if (corruption == "missing-member") members = members.Skip(1).ToArray();
        if (corruption == "duplicate-member") members = members.Append(members[0]).ToArray();
        if (corruption == "duplicate-prerequisite") roots = roots.Select(group => group == root ? new(group.Root, [.. group.Prerequisites, group.Prerequisites[0]]) : group).ToArray();
        if (corruption == "missing-prerequisite") roots = roots.Select(group => group == root ? new(group.Root, []) : group).ToArray();
        if (corruption == "foreign-root") roots = roots.Select(group => group == root ? new(new(group.Root.Source, group.Root.Snippet, ranked.Candidates[1], []), group.Prerequisites) : group).ToArray();
        if (corruption == "root-order") roots = roots.Reverse().ToArray();
        if (corruption == "presentation-order") members = members.Reverse().ToArray();
        if (corruption == "unowned-member") members = members.Append(new(new() { RuleSourceId = "private?secret" }, new() { SnippetId = "secret" }, null, [])).ToArray();
        SafeFailure(() => CompiledRulePackets.Build(new(ranked, roots, members)), corruption == "scope"
            ? CompiledRuleRetrievalFailure.RequestInvalid : CompiledRuleRetrievalFailure.ArtifactInconsistent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MalformedCostFailsRatherThanReducingTotals(int cost)
    {
        var original = Expand(Fixture(new Definition("a", Weight: 900)), 8000);
        var member = original.Members[0];
        var snippet = new CompiledRulesArtifactSnippet { SnippetId = member.SnippetId, RuleSourceId = member.RuleSourceId, Content = member.Snippet.Content, EstimatedTokens = cost };
        var malformed = new CompiledRuleClosureMember(member.Source, snippet, member.Root, member.RequiredByRuleSourceIds);
        SafeFailure(() => CompiledRulePackets.Build(new(original.Input, original.Roots, [malformed, original.Members[1]])), CompiledRuleRetrievalFailure.ArtifactInconsistent);
    }

    [Fact]
    public void CompactModelPayloadGroupsTextAndKeepsAllRichEvidenceServiceSide()
    {
        var artifact = Fixture(new("a", ["b"], 900), new("b", Snippets: 2));
        var result = Build(artifact, 8000);
        Assert.Equal(new[] { "b", "a", "runtime-kernel" }, result.Packet.Rules.Select(section => section.RuleSourceId));
        Assert.Equal(string.Join("\n\n", result.Members.Where(member => member.RuleSourceId == "b").Select(member => member.Member.Snippet.Content)), result.Packet.Rules[0].Content);
        var json = JsonSerializer.Serialize(result.Packet);
        foreach (var unwanted in new[] { "SourcePath", "SourceSha256", "ContentSha256", "SnippetId", "VocabularyScore", "Matches", "RequiredBy", "Compiler", "QueryTerms", "CampaignId", "ManifestPath" }) Assert.DoesNotContain(unwanted, json);
        Assert.Equal(artifact.Ruleset.Source.Value, result.Packet.SourceIdentity.Value);
        Assert.Equal(artifact.Integrity.ArtifactSha256, result.Packet.Scope.SemanticSha256);
        Assert.Equal(1, result.Packet.PacketFormatVersion);
        Assert.True(json.Length < JsonSerializer.Serialize(result).Length);
        Accounting(result);
    }

    [Fact]
    public void PacketAndNestedEvidenceCannotBeMutatedThroughReturnedCollections()
    {
        var artifact = Fixture(new("a", ["b"], 900), new("b"));
        var result = Build(artifact, 8000);
        var before = JsonSerializer.Serialize(result);
        artifact.RuleSources.Clear();
        artifact.Snippets.Clear();
        Assert.Throws<NotSupportedException>(() => ((IList<CompiledRulePacketSection>)result.Packet.Rules).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<CompiledRulePacketMember>)result.Members).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<CompiledRulePacketDecision>)result.Decisions).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<string>)result.Members[0].RequiredByRuleSourceIds).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<CompiledRulesRetrievalTerm>)result.Decisions[0].Root.Matches).Clear());
        Assert.Equal(before, JsonSerializer.Serialize(result));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CancellationBeforeDuringValidationAndDuringPresentationNeverReturnsPartialSuccess(int enumeration)
    {
        var original = Expand(Fixture(new("a", ["b"], 900), new("b")), 8000);
        using var cancellation = new CancellationTokenSource();
        if (enumeration == 0) cancellation.Cancel();
        var input = new CompiledRuleDependencyClosure(original.Input, original.Roots,
            new CancellingList<CompiledRuleClosureMember>(original.Members, cancellation, enumeration));
        Assert.Equal(cancellation.Token, Assert.Throws<OperationCanceledException>(() => CompiledRulePackets.Build(input, cancellation.Token)).CancellationToken);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    [InlineData("fr-FR")]
    public void RepeatedCultureAndEquivalentArtifactEnumerationProduceIdenticalPackets(string culture)
    {
        var artifact = Fixture(new("a", ["b", "c"], 900), new("b"), new("c"));
        var expected = JsonSerializer.Serialize(Build(artifact, 8000));
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Assert.Equal(expected, JsonSerializer.Serialize(Build(artifact, 8000)));
            var ranked = Expand(artifact, 8000).Input;
            ranked = new(ranked.Input, ranked.Candidates.Reverse().ToArray());
            var reordered = CompiledRuleRanking.Rank(new(ranked.Input.Request, ranked.Candidates.Select(root => root.Candidate).ToArray(), ranked.Counts, ranked.Input.Artifact));
            Assert.Equal(expected, JsonSerializer.Serialize(CompiledRulePackets.Build(CompiledRuleDependencies.Expand(reordered))));
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void CancellationDuringRequiredAndOptionalAdmissionReturnsNoPacket(int read)
    {
        var original = Expand(Fixture(new Definition("a", Weight: 900)), 8000);
        using var cancellation = new CancellationTokenSource();
        var roots = new CancellingIndexList<CompiledRuleRootClosure>(original.Roots, cancellation, read);
        Assert.Equal(cancellation.Token, Assert.Throws<OperationCanceledException>(() =>
            CompiledRulePackets.Build(new(original.Input, roots, original.Members), cancellation.Token)).CancellationToken);
    }

    [Fact]
    public void RelocatedSerializedArtifactAndUnrelatedWorkingDirectoryDoNotAffectPacket()
    {
        var root = Path.Combine(Path.GetTempPath(), "EternalCycle.Packet.Tests", Guid.NewGuid().ToString("N"));
        var before = Environment.CurrentDirectory;
        try
        {
            Directory.CreateDirectory(root);
            var expected = JsonSerializer.Serialize(CompiledRulePackets.Build(CanonicalClosure(CanonicalRequest("fighting", "gameplay.resolve", 8000))));
            foreach (var folder in new[] { "first", "relocated" })
            {
                var directory = Directory.CreateDirectory(Path.Combine(root, folder)).FullName;
                var path = Path.Combine(directory, "artifact.json");
                File.WriteAllBytes(path, CanonicalBytes);
                var artifact = CompiledRulesArtifactContract.Read(File.ReadAllText(path)).Artifact!;
                Environment.CurrentDirectory = root;
                Assert.Equal(expected, JsonSerializer.Serialize(CompiledRulePackets.Build(Expand(artifact, CanonicalRequest("fighting", "gameplay.resolve", 8000)))));
            }
        }
        finally { Environment.CurrentDirectory = before; if (Directory.Exists(root)) Directory.Delete(root, true); }
    }

    [Fact]
    public void WholeDependencySourceIncludesUnmatchedSiblingsAtTheirFullCost()
    {
        var artifact = Fixture(new("a", ["b"], 900), new("b", Snippets: 2));
        var below = Build(artifact, 30);
        Assert.Equal(new[] { "runtime-kernel#one" }, Ids(below));
        var exact = Build(artifact, 31);
        Assert.Equal(new[] { "b#one", "b#two", "a#one", "runtime-kernel#one" }, Ids(exact));
        Assert.Equal(2, exact.Totals.DependencyOnlyMembers);
        Accounting(exact);
    }

    [Fact]
    public void PartialMandatoryGroupsCannotEscapeByDroppingOneRequiredRoot()
    {
        var original = CanonicalClosure(CanonicalRequest("", "gameplay.resolve", 8000));
        SafeFailure(() => CompiledRulePackets.Build(new(original.Input, original.Roots.Skip(1).ToArray(), original.Members)), CompiledRuleRetrievalFailure.ArtifactInconsistent);
    }

    [Fact]
    public void CorruptMissingParentEvidenceFailsInsteadOfLosingMultipleSupport()
    {
        var original = Expand(Fixture(new("a", ["c"], 900), new("b", ["c"], 800), new("c")), 31);
        var member = original.Members[0];
        var malformed = new CompiledRuleClosureMember(member.Source, member.Snippet, member.Root, ["a"]);
        SafeFailure(() => CompiledRulePackets.Build(new(original.Input, original.Roots, [malformed, .. original.Members.Skip(1)])), CompiledRuleRetrievalFailure.ArtifactInconsistent);
    }

    [Fact]
    public void ExistingEstimatorAndCompiledNormalizationRemainTheOnlyCostAuthority()
    {
        Assert.Equal(RuleSnippetText.EstimateTokens("caf\u00e9\nrule"), RuleSnippetText.EstimateTokens(Assert.Single(RuleSnippetText.Split("caf\u00e9\r\nrule")).Content));
        var result = Build(Fixture(new Definition("a", Weight: 900, Cost: 37)), 8000);
        Assert.Equal(38, result.Totals.UsedEstimatedTokens);
        // Estimates are format-1 metadata, not an E-specific tokenizer or an
        // envelope/JSON cost. Equal text may legitimately carry another estimate.
        Assert.NotEqual(RuleSnippetText.EstimateTokens(result.Members[0].Member.Snippet.Content), result.Members[0].Member.Snippet.EstimatedTokens);
    }

    [Theory]
    [InlineData("fighting", "gameplay.resolve", 15)]
    [InlineData("conflict", "gameplay.resolve", 15)]
    [InlineData("preparation", "gameplay.resolve", 14)]
    [InlineData("campaign canon", "context.assemble", 7)]
    [InlineData("unreviewed", "gameplay.resolve", 11)]
    [InlineData("", "gameplay.resolve", 11)]
    public void CanonicalSufficientConstrainedExactAndInsufficientPacketsPreserveAuthority(string term, string operation, int rootCount)
    {
        var request = CanonicalRequest(term, operation, 8000);
        var complete = CompiledRulePackets.Build(CanonicalClosure(request));
        Assert.Equal(rootCount, complete.Totals.SelectedRoots);
        Assert.Equal(rootCount, complete.Totals.UniqueMembers);
        Assert.Equal(0, complete.Totals.DependencyOnlyMembers);
        Assert.Equal(0, complete.Totals.BudgetExcludedRoots);
        Assert.Equal(JsonSerializer.Serialize(complete), JsonSerializer.Serialize(CompiledRulePackets.Build(CanonicalClosure(request))));
        var exact = CompiledRulePackets.Build(CanonicalClosure(request with { MaximumEstimatedTokens = complete.Totals.UsedEstimatedTokens }));
        Assert.Equal(rootCount, exact.Totals.SelectedRoots);
        var constrained = CompiledRulePackets.Build(CanonicalClosure(request with { MaximumEstimatedTokens = complete.Totals.RequiredEstimatedTokens }));
        Assert.All(constrained.Decisions.Where(decision => !decision.Required), decision => Assert.False(decision.Selected));
        Assert.All(constrained.Decisions.Where(decision => decision.Required), decision => Assert.True(decision.Selected));
        SafeFailure(() => CompiledRulePackets.Build(CanonicalClosure(request with { MaximumEstimatedTokens = complete.Totals.RequiredEstimatedTokens - 1 })), CompiledRuleRetrievalFailure.PacketBudgetInsufficient);
        if (term == "conflict") Assert.Equal(5, complete.Input.Input.Counts.InapplicableVocabularyMatched);
        if (term == "campaign canon") Assert.Equal(new long[] { 900, 900 }, complete.Decisions.Where(decision => !decision.Required).Select(decision => decision.Root.VocabularyScore));
        foreach (var result in new[] { complete, constrained, exact })
        {
            Accounting(result);
            output.WriteLine($"QUERY={term}; operation={operation}; totals={JsonSerializer.Serialize(result.Totals)}");
            output.WriteLine("SELECTED=" + string.Join(";", result.Decisions.Where(decision => decision.Selected).Select(decision => decision.Root.SnippetId)));
            output.WriteLine("EXCLUDED=" + string.Join(";", result.Decisions.Where(decision => !decision.Selected).Select(decision => $"{decision.Root.SnippetId}:{decision.IncrementalEstimatedTokens}")));
        }
        if (rootCount > (operation == "gameplay.resolve" ? 11 : 5))
        {
            var partial = CompiledRulePackets.Build(CanonicalClosure(request with { MaximumEstimatedTokens = operation == "gameplay.resolve" ? 3000 : 1400 }));
            Assert.Equal(operation == "gameplay.resolve" ? 13 : 6, partial.Totals.SelectedRoots);
            Assert.Equal(term == "preparation" ? 2979 : operation == "gameplay.resolve" ? 2985 : 1394, partial.Totals.UsedEstimatedTokens);
            Accounting(partial);
            output.WriteLine($"PARTIAL={term}; totals={JsonSerializer.Serialize(partial.Totals)}");
            output.WriteLine("SELECTED=" + string.Join(";", partial.Decisions.Where(decision => decision.Selected).Select(decision => decision.Root.SnippetId)));
            output.WriteLine("EXCLUDED=" + string.Join(";", partial.Decisions.Where(decision => !decision.Selected).Select(decision => decision.Root.SnippetId)));
        }
    }

    [Theory]
    [InlineData("save", 12)]
    [InlineData("fighting|conflict|save|preparation", 19)]
    public void CanonicalDependencyFailuresNeverBecomeSuccessfulBudgetExclusions(string terms, int rootCount)
    {
        var ranked = CompiledRuleRanking.Rank(CanonicalIndex.FindCandidates(CompiledRuleRetrieval.Prepare(CanonicalRequest(terms, "gameplay.resolve", 8000))));
        Assert.Equal(rootCount, ranked.Candidates.Count);
        Assert.Equal(600, ranked.Candidates.Single(root => root.SnippetId == "core-context-assembly#manual-persistence-commands").VocabularyScore);
        SafeFailure(() => CompiledRulePackets.Build(CompiledRuleDependencies.Expand(ranked)), CompiledRuleRetrievalFailure.DependencyFailed);
    }

    [Fact]
    public void CanonicalArtifactCountsHashesAndBytesAreNotChangedByPacketConstruction()
    {
        CompiledRulePackets.Build(CanonicalClosure(CanonicalRequest("fighting", "gameplay.resolve", 8000)));
        Assert.Equal(10, Canonical.RuleSources.Count);
        Assert.Equal(154, Canonical.Snippets.Count);
        Assert.Equal(773, Canonical.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count));
        Assert.Equal(275622, CanonicalBytes.Length);
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", Canonical.Integrity.ArtifactSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", Convert.ToHexString(SHA256.HashData(CanonicalBytes)));
        Assert.Equal(CanonicalBytes, CompiledRulesArtifactWriter.Write(Canonical).Bytes.ToArray());
    }

    private sealed record Definition(string Id, string[]? Dependencies = null, int Weight = 0, int Cost = 10,
        CompiledRulesArtifactApplicability? Metadata = null, int Snippets = 1);

    private static CompiledRulesArtifact Fixture(params Definition[] definitions)
    {
        var all = definitions.ToList();
        if (all.All(definition => definition.Id != "runtime-kernel")) all.Add(new("runtime-kernel", Cost: 1, Metadata: new() { Layer = RuleLayer.RuntimeKernel }));
        var artifact = new CompiledRulesArtifact
        {
            ArtifactFormatVersion = 1, Compiler = new() { ContractVersion = "1", ImplementationId = "fixture", ImplementationVersion = "1.0.0" },
            Integrity = new() { Algorithm = "SHA-256" },
            Ruleset = new() { RulesetId = "fixture", RepositoryVersion = "1.0.0", Source = new() { Scheme = "snapshot", Value = "immutable" }, ManifestPath = "rules/manifest.json", ManifestSha256 = new string('B', 64) },
            RuleSources = all.OrderBy(definition => definition.Id, StringComparer.Ordinal).Select(definition => new CompiledRulesArtifactRuleSource
            {
                RuleSourceId = definition.Id, SourcePath = $"rules/{definition.Id}.md", SourceSha256 = new string('A', 64), Applicability = definition.Metadata ?? new() { Layer = RuleLayer.Core },
                DependencyRuleSourceIds = (definition.Dependencies ?? []).Order(StringComparer.Ordinal).ToList()
            }).ToList(),
            Snippets = all.SelectMany(definition => Enumerable.Range(0, definition.Snippets).Select(index => new CompiledRulesArtifactSnippet
            {
                SnippetId = definition.Id + (index == 0 ? "#one" : "#two"), RuleSourceId = definition.Id, SourceAnchor = index == 0 ? "one" : "two",
                Content = "Executable fixture text.", ContentSha256 = CompiledRulesArtifactContract.ComputeContentSha256("Executable fixture text."), EstimatedTokens = definition.Cost,
                Retrieval = new() { Terms = index == 0 && definition.Weight > 0 ? [new() { Term = "select", Kind = "selection", Weight = definition.Weight }] : [] }
            })).OrderBy(snippet => snippet.SnippetId, StringComparer.Ordinal).ToList()
        };
        var result = new CompiledRulesArtifact { ArtifactFormatVersion = 1, Compiler = artifact.Compiler, Ruleset = artifact.Ruleset, RuleSources = artifact.RuleSources, Snippets = artifact.Snippets,
            Integrity = new() { Algorithm = "SHA-256", ArtifactSha256 = CompiledRulesArtifactContract.ComputeArtifactSha256(artifact) } };
        Assert.Empty(CompiledRulesArtifactContract.Validate(result));
        return result;
    }

    private static CompiledRuleRetrievalRequest Request(CompiledRulesArtifact artifact, int budget) => new(new(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256), "context.assemble", "NORMAL", ["select"]) { MaximumEstimatedTokens = budget };
    private static CompiledRuleDependencyClosure Expand(CompiledRulesArtifact artifact, int budget) => Expand(artifact, Request(artifact, budget));
    private static CompiledRuleDependencyClosure Expand(CompiledRulesArtifact artifact, CompiledRuleRetrievalRequest request) => CompiledRuleDependencies.Expand(CompiledRuleRanking.Rank(CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(request))));
    private static CompiledRulePacketResult Build(CompiledRulesArtifact artifact, int budget) => CompiledRulePackets.Build(Expand(artifact, budget));
    private static IEnumerable<string> Ids(CompiledRulePacketResult result) => result.Members.Select(member => member.SnippetId);
    private static CompiledRuleRetrievalRequest CanonicalRequest(string terms, string operation, int budget) => Request(Canonical, budget) with
    { Operation = operation, QueryTerms = terms.Length == 0 ? [] : terms.Split('|'), Topics = Canonical.RuleSources.SelectMany(source => source.Applicability.Topics).Distinct().ToArray() };
    private static CompiledRuleDependencyClosure CanonicalClosure(CompiledRuleRetrievalRequest request) => CompiledRuleDependencies.Expand(CompiledRuleRanking.Rank(CanonicalIndex.FindCandidates(CompiledRuleRetrieval.Prepare(request))));
    private static void Accounting(CompiledRulePacketResult result)
    {
        Assert.Equal(result.Members.Count, Ids(result).Distinct().Count());
        Assert.Equal(result.Members.Sum(member => member.Member.Snippet.EstimatedTokens), result.Totals.UsedEstimatedTokens);
        Assert.Equal(result.Packet.EstimatedTokens, result.Totals.UsedEstimatedTokens);
        Assert.Equal(result.Totals.RequestedEstimatedTokens - result.Totals.UsedEstimatedTokens, result.Totals.RemainingEstimatedTokens);
        Assert.InRange(result.Totals.UsedEstimatedTokens, 0, result.Totals.RequestedEstimatedTokens);
        var ids = Ids(result).ToHashSet(StringComparer.Ordinal);
        foreach (var decision in result.Decisions.Where(decision => decision.Selected))
        {
            Assert.Contains(decision.Root.SnippetId, ids);
            Assert.All(result.Input.Roots.Single(root => root.Root.SnippetId == decision.Root.SnippetId).Prerequisites, prerequisite => Assert.Contains(prerequisite.SnippetId, ids));
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
    private sealed class CancellingList<T>(IReadOnlyList<T> values, CancellationTokenSource cancellation, int when) : IReadOnlyList<T>
    {
        private int enumerations;
        public int Count => values.Count;
        public T this[int index] => values[index];
        public IEnumerator<T> GetEnumerator() { if (++enumerations == when) cancellation.Cancel(); return values.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    private sealed class CancellingIndexList<T>(IReadOnlyList<T> values, CancellationTokenSource cancellation, int when) : IReadOnlyList<T>
    {
        private int reads;
        public int Count => values.Count;
        public T this[int index] { get { if (++reads == when) cancellation.Cancel(); return values[index]; } }
        public IEnumerator<T> GetEnumerator() => values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
