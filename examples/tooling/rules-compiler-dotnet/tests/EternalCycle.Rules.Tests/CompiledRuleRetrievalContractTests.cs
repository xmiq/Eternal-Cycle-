using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRuleRetrievalContractTests
{
    private static CompiledRuleRetrievalRequest Request(params string[] terms) =>
        new(new("fixture-rules", new string('A', 64)), "gameplay.resolve", "standard", terms);

    [Fact]
    public void MinimalRequestNeedsOnlyExplicitRulesScopeAndSelectors()
    {
        var prepared = CompiledRuleRetrieval.Prepare(Request());
        Assert.Equal("fixture-rules", prepared.Scope.RulesetId);
        Assert.Equal(new string('A', 64), prepared.Scope.SemanticSha256);
        Assert.Equal("gameplay.resolve", prepared.Operation);
        Assert.Equal("standard", prepared.CampaignMode);
        Assert.Null(prepared.WorldModelId);
        Assert.Equal(8000, prepared.MaximumEstimatedTokens);
        Assert.Empty(prepared.QueryTerms);
        Assert.Empty(prepared.RequiredSnippetIds);
        Assert.DoesNotContain(typeof(CompiledRuleRetrievalRequest).GetProperties(),
            property => property.Name is "CampaignId" or "Provider" or "SourceRoot" or "PreparationTier" or "TopK");
    }

    [Theory]
    [InlineData("  FIGHTING\t\n ", "fighting")]
    [InlineData("CAMPAIGN\u00A0\u2003CANON", "campaign canon")]
    [InlineData("CAFE\u0301", "caf\u00E9")]
    [InlineData("Soul-Bound", "soul-bound")]
    [InlineData("Soul Bound", "soul bound")]
    [InlineData("SAVE?!", "save?!")]
    [InlineData("Experience Points", "experience points")]
    public void QueryUsesReviewedNormalizationExactly(string raw, string expected)
    {
        var prepared = CompiledRuleRetrieval.Prepare(Request(raw));
        Assert.Equal(expected, Assert.Single(prepared.QueryTerms));
        Assert.Equal(RuleRetrievalVocabulary.NormalizeTerm(raw), prepared.QueryTerms[0]);
    }

    [Fact]
    public void PreparationDoesNotGenerateOrSplitTermsAndPreservesDistinctForms()
    {
        var prepared = CompiledRuleRetrieval.Prepare(Request("fighting", "level", "leveling", "soul-bound", "soul bound", "campaign canon"));
        Assert.Equal(new[] { "campaign canon", "fighting", "level", "leveling", "soul bound", "soul-bound" }, prepared.QueryTerms);
        Assert.DoesNotContain("combat", prepared.QueryTerms);
        Assert.DoesNotContain("campaign", prepared.QueryTerms);
        Assert.DoesNotContain("canon", prepared.QueryTerms);
    }

    [Fact]
    public void UnorderedDuplicateSetsHaveDeterministicPrivateReadOnlyCopies()
    {
        var terms = new List<string> { " SAVE ", "fighting", "save" };
        var ids = new List<string> { "z", "A", "z" };
        var request = Request() with
        {
            QueryTerms = terms, ModuleIds = ids, Topics = ids, RequiredRuleSourceIds = ids,
            RequiredSnippetIds = ["z#section", "A", "z#section"], WorldModelId = "World-A"
        };
        var first = CompiledRuleRetrieval.Prepare(request);
        var second = CompiledRuleRetrieval.Prepare(request with
        {
            QueryTerms = terms.AsEnumerable().Reverse().ToArray(), ModuleIds = ["z", "A"],
            Topics = ["A", "z"], RequiredRuleSourceIds = ["A", "z"], RequiredSnippetIds = ["A", "z#section"]
        });
        Assert.Equal(first.QueryTerms, second.QueryTerms);
        Assert.Equal(first.ModuleIds, second.ModuleIds);
        Assert.Equal(first.Topics, second.Topics);
        Assert.Equal(first.RequiredRuleSourceIds, second.RequiredRuleSourceIds);
        Assert.Equal(first.RequiredSnippetIds, second.RequiredSnippetIds);
        terms.Clear();
        ids.Clear();
        Assert.Equal(new[] { "fighting", "save" }, first.QueryTerms);
        Assert.Equal(new[] { "A", "z" }, first.ModuleIds);
        Assert.Equal("World-A", first.WorldModelId);
        foreach (var collection in new[] { first.QueryTerms, first.ModuleIds, first.Topics, first.RequiredRuleSourceIds, first.RequiredSnippetIds })
            Assert.Throws<NotSupportedException>(() => ((IList<string>)collection)[0] = "changed");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8000)]
    public void BudgetBoundsAreInclusive(int maximum) =>
        Assert.Equal(maximum, CompiledRuleRetrieval.Prepare(Request() with { MaximumEstimatedTokens = maximum }).MaximumEstimatedTokens);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("save\0secret")]
    public void InvalidTermsFailWithSafeStructuredDiagnostic(string? term) => Invalid(Request(term!));

    [Fact]
    public void InvalidUnicodeIsConstructedAtExecutionRatherThanSerializedInTestMetadata() =>
        Invalid(Request(new string((char)0xD800, 1)));

    [Fact]
    public void TermByteIndependentCharacterBoundsReuseVocabularyContract()
    {
        Assert.Single(CompiledRuleRetrieval.Prepare(Request(new string('a', 256))).QueryTerms);
        Assert.Single(CompiledRuleRetrieval.Prepare(Request(new string(' ', 1792) + new string('a', 256))).QueryTerms);
        Invalid(Request(new string('a', 257)));
        Invalid(Request(new string(' ', 1793) + new string('a', 256)));
    }

    [Theory]
    [InlineData("scope")]
    [InlineData("ruleset")]
    [InlineData("hash-empty")]
    [InlineData("hash-short")]
    [InlineData("hash-lowercase")]
    [InlineData("hash-nonhex")]
    [InlineData("operation")]
    [InlineData("mode")]
    [InlineData("world")]
    [InlineData("zero-budget")]
    [InlineData("negative-budget")]
    [InlineData("over-budget")]
    [InlineData("null-queries")]
    [InlineData("null-modules")]
    [InlineData("null-topics")]
    [InlineData("null-sources")]
    [InlineData("null-snippets")]
    public void InvalidRequestFieldsAreRejected(string field)
    {
        var request = Request();
        Invalid(field switch
        {
            "scope" => request with { Scope = null! },
            "ruleset" => request with { Scope = new("../secret", new string('A', 64)) },
            "hash-empty" => request with { Scope = new("fixture-rules", null!) },
            "hash-short" => request with { Scope = new("fixture-rules", "AAA") },
            "hash-lowercase" => request with { Scope = new("fixture-rules", new string('a', 64)) },
            "hash-nonhex" => request with { Scope = new("fixture-rules", new string('G', 64)) },
            "operation" => request with { Operation = "*" },
            "mode" => request with { CampaignMode = "" },
            "world" => request with { WorldModelId = "*" },
            "zero-budget" => request with { MaximumEstimatedTokens = 0 },
            "negative-budget" => request with { MaximumEstimatedTokens = -1 },
            "over-budget" => request with { MaximumEstimatedTokens = 8001 },
            "null-queries" => request with { QueryTerms = null! },
            "null-modules" => request with { ModuleIds = null! },
            "null-topics" => request with { Topics = null! },
            "null-sources" => request with { RequiredRuleSourceIds = null! },
            _ => request with { RequiredSnippetIds = null! }
        });
    }

    [Theory]
    [InlineData("*")]
    [InlineData("../source")]
    [InlineData("source\\secret")]
    [InlineData("source#anchor")]
    [InlineData("urn:source")]
    [InlineData(" source")]
    [InlineData("source\0")]
    [InlineData("cafe\u0301")]
    public void ConcreteIdentifiersDoNotPermitPathsWildcardsOrTermNormalization(string id)
    {
        Invalid(Request() with { ModuleIds = [id] });
        Invalid(Request() with { Topics = [id] });
        Invalid(Request() with { RequiredRuleSourceIds = [id] });
    }

    [Theory]
    [InlineData("#anchor")]
    [InlineData("source#")]
    [InlineData("source#anchor#other")]
    [InlineData("source#../anchor")]
    [InlineData("source#*")]
    public void SnippetIdentityMustHaveValidSourceAndOptionalSingleAnchor(string id) =>
        Invalid(Request() with { RequiredSnippetIds = [id] });

    [Fact]
    public void IdentifierAndSetBoundsCannotBeEvadedByDuplicateInput()
    {
        var longest = new string('x', 128);
        Assert.Single(CompiledRuleRetrieval.Prepare(Request() with { RequiredSnippetIds = [$"{longest}#{longest}"] }).RequiredSnippetIds);
        Invalid(Request() with { Operation = longest + "x" });
        Invalid(Request() with { RequiredSnippetIds = [$"{longest}#{longest}x"] });
        Assert.Single(CompiledRuleRetrieval.Prepare(Request(Enumerable.Repeat("save", 16).ToArray())).QueryTerms);
        Invalid(Request(Enumerable.Repeat("save", 17).ToArray()));
        var maximum = Enumerable.Repeat("source", 32).ToArray();
        var excess = Enumerable.Repeat("source", 33).ToArray();
        Assert.Single(CompiledRuleRetrieval.Prepare(Request() with { ModuleIds = maximum }).ModuleIds);
        Invalid(Request() with { ModuleIds = excess });
        Invalid(Request() with { Topics = excess });
        Invalid(Request() with { RequiredRuleSourceIds = excess });
        Invalid(Request() with { RequiredSnippetIds = excess });
        Invalid(null!);
    }

    [Fact]
    public void CulturesProduceIdenticalTermsAndPreserveConcreteIdentityCase()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            foreach (var culture in new[] { "en-US", "tr-TR", "fr-FR" })
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                var prepared = CompiledRuleRetrieval.Prepare(Request("FIGHTING", "CAFE\u0301") with { RequiredRuleSourceIds = ["SOURCE", "source"] });
                Assert.Equal(new[] { "caf\u00E9", "fighting" }, prepared.QueryTerms);
                Assert.Equal(new[] { "SOURCE", "source" }, prepared.RequiredRuleSourceIds);
            }
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void ExplicitArtifactHistoriesRemainDistinctWithoutAuthorizingOrLoadingThem()
    {
        var first = CompiledRuleRetrieval.Prepare(Request("save"));
        var second = CompiledRuleRetrieval.Prepare(Request("save") with { Scope = new("fixture-rules", new string('B', 64)) });
        Assert.NotEqual(first.Scope, second.Scope);
        Assert.Equal(first.QueryTerms, second.QueryTerms);
        Assert.Equal("fixture-rules", second.Scope.RulesetId);
    }

    [Fact]
    public void CancellationBeforeAndDuringPreparationRemainsCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        var request = Request() with { QueryTerms = new CancellingTerms(cancellation) };
        var during = Assert.Throws<OperationCanceledException>(() => CompiledRuleRetrieval.Prepare(request, cancellation.Token));
        Assert.Equal(cancellation.Token, during.CancellationToken);
        var before = Assert.Throws<OperationCanceledException>(() => CompiledRuleRetrieval.Prepare(null!, cancellation.Token));
        Assert.Equal(cancellation.Token, before.CancellationToken);
    }

    [Theory]
    [InlineData(CompiledRuleRetrievalFailure.RequestInvalid)]
    [InlineData(CompiledRuleRetrievalFailure.ArtifactUnavailable)]
    [InlineData(CompiledRuleRetrievalFailure.ArtifactInconsistent)]
    [InlineData(CompiledRuleRetrievalFailure.DependencyFailed)]
    [InlineData(CompiledRuleRetrievalFailure.PacketBudgetInsufficient)]
    [InlineData(CompiledRuleRetrievalFailure.StorageFailed)]
    public void FailureCategoriesHaveBoundedStableSafeCodes(CompiledRuleRetrievalFailure failure)
    {
        var exception = new CompiledRuleRetrievalException(failure);
        Assert.Equal(failure, exception.Failure);
        Assert.StartsWith("RULE_RETRIEVAL_", exception.Code);
        Assert.InRange(exception.Message.Length, 1, 128);
        Assert.Null(exception.InnerException);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CompiledRuleRetrievalException((CompiledRuleRetrievalFailure)999));
    }

    [Fact]
    public void CanonicalCompilerBytesRemainUnchangedAndScopeUsesSemanticNotByteIdentity()
    {
        var bytes = CompiledRulesImportFixtures.Canonical();
        var read = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(bytes));
        Assert.True(read.IsValid);
        var artifact = read.Artifact!;
        Assert.Equal(10, artifact.RuleSources.Count);
        Assert.Equal(154, artifact.Snippets.Count);
        Assert.Equal(773, artifact.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count));
        Assert.Equal(275622, bytes.Length);
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", artifact.Integrity.ArtifactSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", Convert.ToHexString(SHA256.HashData(bytes)));
        var prepared = CompiledRuleRetrieval.Prepare(Request("FIGHTING") with { Scope = new(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256) });
        Assert.Equal(artifact.Integrity.ArtifactSha256, prepared.Scope.SemanticSha256);
        Assert.Equal(bytes, CompiledRulesArtifactWriter.Write(artifact).Bytes.ToArray());
    }

    private static void Invalid(CompiledRuleRetrievalRequest request)
    {
        var exception = Assert.Throws<CompiledRuleRetrievalException>(() => CompiledRuleRetrieval.Prepare(request));
        Assert.Equal(CompiledRuleRetrievalFailure.RequestInvalid, exception.Failure);
        Assert.Equal("RULE_RETRIEVAL_REQUEST_INVALID", exception.Code);
        Assert.Equal("The rule retrieval request is invalid or exceeds its bounds.", exception.Message);
        Assert.Null(exception.InnerException);
        Assert.Equal("CompiledRuleRetrievalRequest", request?.ToString() ?? nameof(CompiledRuleRetrievalRequest));
    }

    private sealed class CancellingTerms(CancellationTokenSource cancellation) : IReadOnlyList<string>
    {
        public int Count => 2;
        public string this[int index] => "save";
        public IEnumerator<string> GetEnumerator()
        {
            yield return "save";
            cancellation.Cancel();
            yield return "fighting";
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
