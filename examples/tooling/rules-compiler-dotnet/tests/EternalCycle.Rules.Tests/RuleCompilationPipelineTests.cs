using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class RuleCompilationPipelineTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbsentOrEmptyVocabularyMatchesLegacyBytes(bool declared)
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        if (declared)
        {
            manifest["retrievalVocabulary"]!["concepts"]!.AsArray().Clear();
            manifest["retrievalVocabulary"]!["bindings"]!.AsArray().Clear();
        }
        else { manifest.Remove("retrievalVocabulary"); }
        payload.WriteManifest(manifest);
        var snapshot = payload.Load();
        var legacy = CompiledRulesArtifactAssembler.Assemble(snapshot, RuleSnippetCompiler.Compile(snapshot));
        Assert.True(legacy.IsValid);
        var oldBytes = CompiledRulesArtifactWriter.Write(legacy.Artifact!);
        var result = RuleCompilationPipeline.Compile(snapshot);

        Assert.True(result.IsValid);
        Assert.Equal(oldBytes.Bytes.ToArray(), result.Bytes.ToArray());
        Assert.All(result.Vocabulary.Snippets, snippet => Assert.Empty(snippet.Associations));
    }

    [Fact]
    public void FixtureCompilesToValidatedFormatOneAndReportsAcceptanceEvidence()
    {
        using var payload = new VocabularyTestPayload();
        var result = payload.Compile();
        AssertValid(result);
        Assert.Equal(2, result.Artifact!.RuleSources.Count);
        Assert.Equal(4, result.Artifact.Snippets.Count);
        Assert.Equal(3, payload.Load().Manifest.RetrievalVocabulary!.Concepts.Count);
        Assert.Equal(15, result.Artifact.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count));
        output.WriteLine("Sources=2 Snippets=4 Concepts=3 Associations=15 Bytes={0}", result.Bytes.Length);
        output.WriteLine("Semantic digest: {0}", result.Artifact.Integrity.ArtifactSha256);
        output.WriteLine("Serialized byte SHA-256: {0}", Sha256(result.Bytes));
    }

    [Theory]
    [InlineData("combat", "combat", "canonical", 800)]
    [InlineData("combat", "fight", "alias", 600)]
    [InlineData("progression", "character advancement", "phrase", 650)]
    public void CanonicalAliasAndPhraseMapToConceptCategoryWithExactWeight(string concept, string term, string origin, int weight)
    {
        using var payload = new VocabularyTestPayload();
        var result = payload.Compile();
        var snippetId = $"core#{concept}";
        var association = result.Vocabulary.Snippets.Single(snippet => snippet.Candidate.SnippetId == snippetId)
            .Associations.Single(item => item.ConceptId == concept && item.RetrievalTerm.Term == term);
        Assert.Equal(origin, association.RetrievalTerm.Kind);
        var emitted = Snippet(result, snippetId).Retrieval.Terms.Single(item => item.Term == term && item.Kind == concept);
        Assert.Equal(weight, emitted.Weight);
        Assert.Equal(concept, association.CanonicalTerm);
    }

    [Fact]
    public void EveryEffectiveAssociationHasExactlyOneArtifactTerm()
    {
        using var payload = new VocabularyTestPayload();
        var result = payload.Compile();
        foreach (var evidence in result.Vocabulary.Snippets)
        {
            var terms = Snippet(result, evidence.Candidate.SnippetId).Retrieval.Terms;
            Assert.Equal(evidence.Associations.Count, terms.Count);
            foreach (var association in evidence.Associations)
            {
                var emitted = Assert.Single(terms, term => term.Term == association.RetrievalTerm.Term && term.Kind == association.ConceptId);
                Assert.Equal(association.RetrievalTerm.Weight, emitted.Weight);
            }
            Assert.Equal(terms.OrderBy(term => term.Term, StringComparer.Ordinal).ThenBy(term => term.Kind, StringComparer.Ordinal), terms);
            Assert.Empty(Snippet(result, evidence.Candidate.SnippetId).Retrieval.Relationships);
        }
    }

    [Fact]
    public void OverlapKeepsAllOriginsButDoesNotDuplicateArtifactTerms()
    {
        using var payload = new VocabularyTestPayload();
        var result = payload.Compile();
        var evidence = result.Vocabulary.Snippets.Single(snippet => snippet.Candidate.SnippetId == "operations#save");
        Assert.Equal(6, evidence.Associations.Count);
        Assert.Equal(6, Snippet(result, "operations#save").Retrieval.Terms.Count);
        Assert.All(evidence.Associations.Where(item => item.ConceptId == "persistence"), item =>
        {
            Assert.Equal(2, item.Origins.Count);
            Assert.All(item.Origins, origin =>
            {
                Assert.NotEmpty(origin.BindingRationale);
                Assert.NotEmpty(origin.ConceptRationale);
                Assert.NotEmpty(origin.TermRationale);
            });
        });
        Assert.Empty(evidence.Candidate.Retrieval.Terms);
    }

    [Fact]
    public void SharedTermKeepsBothConceptsAndDifferingWeights()
    {
        using var payload = new VocabularyTestPayload();
        var terms = Snippet(payload.Compile(), "operations#save").Retrieval.Terms.Where(term => term.Term == "progress").ToArray();
        Assert.Equal(["persistence", "progression"], terms.Select(term => term.Kind));
        Assert.Equal([400, 600], terms.Select(term => term.Weight));
    }

    [Fact]
    public void IdenticalCanonicalWordsAcrossDistinctConceptsDoNotCollide()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        var concept = manifest["retrievalVocabulary"]!["concepts"]![2]!;
        concept["canonicalTerm"] = "progression";
        payload.WriteManifest(manifest);
        var result = payload.Compile();
        AssertValid(result);
        var terms = Snippet(result, "operations#save").Retrieval.Terms.Where(term => term.Term == "progression").ToArray();
        Assert.Equal(["persistence", "progression"], terms.Select(term => term.Kind));
        Assert.Equal([900, 700], terms.Select(term => term.Weight));
    }

    [Fact]
    public void MaximumLengthConceptIdFitsExistingCarrierWithoutEncoding()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        var id = new string('c', 128);
        manifest["retrievalVocabulary"]!["concepts"]![0]!["conceptId"] = id;
        manifest["retrievalVocabulary"]!["bindings"]![0]!["conceptIds"]![0] = id;
        payload.WriteManifest(manifest);
        var result = payload.Compile();
        AssertValid(result);
        Assert.All(Snippet(result, "core#combat").Retrieval.Terms, term => Assert.Equal(id, term.Kind));
    }

    [Fact]
    public void VocabularyDoesNotChangeSourceContentIdentityOrApplicability()
    {
        using var payload = new VocabularyTestPayload();
        var withVocabulary = payload.Compile();
        var manifest = payload.ReadManifest();
        manifest.Remove("retrievalVocabulary");
        payload.WriteManifest(manifest);
        var withoutVocabulary = payload.Compile();
        Assert.Equal(SourceSemantics(withVocabulary), SourceSemantics(withoutVocabulary));
        Assert.Equal(ContentSemantics(withVocabulary), ContentSemantics(withoutVocabulary));
        Assert.Equal(withVocabulary.Artifact!.Ruleset.Source.Value, withoutVocabulary.Artifact!.Ruleset.Source.Value);
        Assert.All(withoutVocabulary.Artifact.Snippets, snippet => Assert.Empty(snippet.Retrieval.Terms));
        Assert.NotEqual(withVocabulary.Artifact.Ruleset.ManifestSha256, withoutVocabulary.Artifact.Ruleset.ManifestSha256);
    }

    [Theory]
    [InlineData("weight")]
    [InlineData("term")]
    [InlineData("concept")]
    public void SemanticVocabularyMutationChangesDigestAndBytes(string mutation)
    {
        using var payload = new VocabularyTestPayload();
        var first = payload.Compile();
        var manifest = payload.ReadManifest();
        var concept = manifest["retrievalVocabulary"]!["concepts"]![0]!;
        switch (mutation)
        {
            case "weight": concept["weight"] = 801; break;
            case "term": concept["alternatives"]![0]!["term"] = "conflict"; break;
            case "concept":
                concept["conceptId"] = "conflict";
                manifest["retrievalVocabulary"]!["bindings"]![0]!["conceptIds"]![0] = "conflict";
                break;
        }
        payload.WriteManifest(manifest);
        var second = payload.Compile();
        AssertValid(second);
        Assert.Equal(SourceSemantics(first), SourceSemantics(second));
        Assert.Equal(ContentSemantics(first), ContentSemantics(second));
        Assert.NotEqual(RetrievalSemantics(first), RetrievalSemantics(second));
        Assert.NotEqual(first.Artifact!.Integrity.ArtifactSha256, second.Artifact!.Integrity.ArtifactSha256);
        Assert.NotEqual(first.Bytes.ToArray(), second.Bytes.ToArray());
    }

    [Theory]
    [InlineData("rationale")]
    [InlineData("normalized-term")]
    [InlineData("formatting")]
    public void EquivalentMetadataStillRetainsExactManifestProvenance(string mutation)
    {
        using var payload = new VocabularyTestPayload();
        var first = payload.Compile();
        var manifest = payload.ReadManifest();
        if (mutation == "rationale") { manifest["retrievalVocabulary"]!["concepts"]![0]!["rationale"] = "Another reviewed explanation."; }
        if (mutation == "normalized-term") { manifest["retrievalVocabulary"]!["concepts"]![0]!["canonicalTerm"] = "  COMBAT  "; }
        payload.WriteManifest(manifest);
        var second = payload.Compile();
        Assert.Equal(RetrievalSemantics(first), RetrievalSemantics(second));
        Assert.NotEqual(first.Artifact!.Ruleset.ManifestSha256, second.Artifact!.Ruleset.ManifestSha256);
        Assert.NotEqual(first.Artifact.Integrity.ArtifactSha256, second.Artifact.Integrity.ArtifactSha256);
        Assert.NotEqual(first.Bytes.ToArray(), second.Bytes.ToArray());
    }

    [Fact]
    public void RetrievalMetadataItselfParticipatesInDigestAndWriterValidation()
    {
        using var payload = new VocabularyTestPayload();
        var result = payload.Compile();
        var artifact = result.Artifact!;
        var terms = artifact.Snippets[0].Retrieval.Terms;
        var original = terms[0];
        terms[0] = new() { Term = original.Term, Kind = original.Kind, Weight = original.Weight + 1 };
        Assert.NotEqual(artifact.Integrity.ArtifactSha256, CompiledRulesArtifactContract.ComputeArtifactSha256(artifact));
        var write = CompiledRulesArtifactWriter.Write(artifact);
        Assert.False(write.IsValid);
        Assert.Empty(write.Bytes.ToArray());
        Assert.Contains(write.Errors, error => error.Code == "ARTIFACT_HASH_MISMATCH");
    }

    [Fact]
    public void RepeatedAndRelocatedCompilationIsByteIdenticalWithoutPhysicalPaths()
    {
        using var firstPayload = new VocabularyTestPayload();
        using var relocated = new VocabularyTestPayload();
        var first = firstPayload.Compile();
        var repeated = firstPayload.Compile();
        var moved = relocated.Compile();
        Assert.Equal(first.Bytes.ToArray(), repeated.Bytes.ToArray());
        Assert.Equal(first.Bytes.ToArray(), moved.Bytes.ToArray());
        Assert.Equal(JsonSerializer.Serialize(first.Vocabulary), JsonSerializer.Serialize(moved.Vocabulary));
        Assert.DoesNotContain(firstPayload.Root, Encoding.UTF8.GetString(first.Bytes.Span));
        Assert.DoesNotContain(relocated.Root, JsonSerializer.Serialize(moved.Vocabulary));
    }

    [Fact]
    public void CultureAndSetInsertionOrderDoNotChangeCompleteCompilation()
    {
        using var payload = new VocabularyTestPayload();
        var snapshot = payload.Load();
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var first = RuleCompilationPipeline.Compile(snapshot);
            var definition = snapshot.Manifest.RetrievalVocabulary!;
            Reverse(definition.Concepts);
            Reverse(definition.Bindings);
            foreach (var concept in definition.Concepts) { Reverse(concept.Alternatives); }
            foreach (var binding in definition.Bindings) { Reverse(binding.ConceptIds); }
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var second = RuleCompilationPipeline.Compile(snapshot);
            Assert.Equal(first.Bytes.ToArray(), second.Bytes.ToArray());
            Assert.Equal(JsonSerializer.Serialize(first.Vocabulary), JsonSerializer.Serialize(second.Vocabulary));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void LfAndCrLfSourcesRetainExactProvenanceWithVocabulary()
    {
        using var lf = new VocabularyTestPayload();
        using var crlf = new VocabularyTestPayload();
        foreach (var name in new[] { "core.txt", "operations.txt" })
        {
            var text = File.ReadAllText(Path.Combine(lf.Root, name)).ReplaceLineEndings("\n");
            File.WriteAllText(Path.Combine(lf.Root, name), text);
            File.WriteAllText(Path.Combine(crlf.Root, name), text.ReplaceLineEndings("\r\n"));
        }
        var first = lf.Compile();
        var second = crlf.Compile();
        Assert.Equal(ContentSemantics(first), ContentSemantics(second));
        Assert.Equal(RetrievalSemantics(first), RetrievalSemantics(second));
        Assert.NotEqual(first.Artifact!.RuleSources[0].SourceSha256, second.Artifact!.RuleSources[0].SourceSha256);
        Assert.NotEqual(first.Artifact.Integrity.ArtifactSha256, second.Artifact.Integrity.ArtifactSha256);
        Assert.NotEqual(first.Bytes.ToArray(), second.Bytes.ToArray());
    }

    [Fact]
    public void InvalidAnchorFailsWithTheExistingBoundedEnrichmentDiagnostic()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        manifest["retrievalVocabulary"]!["bindings"]![0]!["sourceAnchor"] = "not-declared";
        payload.WriteManifest(manifest);
        var error = Assert.Throws<RuleVocabularyEnrichmentException>(() => payload.Compile());
        Assert.Equal("VOCABULARY_TARGET_MISSING", error.Code);
        Assert.StartsWith("$.retrievalVocabulary.bindings", error.Path);
        Assert.True(error.Message.Length < 256);
    }

    [Fact]
    public void ArtifactContractFailureProducesNoArtifactOrBytesButRetainsEvidence()
    {
        using var payload = new VocabularyTestPayload();
        var snapshot = payload.Load() with { CompilerIdentity = new() { ContractVersion = "2", ImplementationId = "test", ImplementationVersion = "1" } };
        var result = RuleCompilationPipeline.Compile(snapshot);
        Assert.False(result.IsValid);
        Assert.Null(result.Artifact);
        Assert.Empty(result.Bytes.ToArray());
        Assert.NotEmpty(result.Vocabulary.Snippets[0].Associations);
        Assert.Contains(result.Errors, error => error.Code == "COMPILER_CONTRACT_UNSUPPORTED");
    }

    [Fact]
    public void CanonicalNoVocabularyCorpusRetainsFullBaseline()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docs", "rules", "rule-source-manifest.json"))) { directory = directory.Parent; }
        Assert.NotNull(directory);
        var snapshot = MaterializedRuleSourceLoader.Load(new(directory.FullName, "docs/rules/rule-source-manifest.json",
            new() { Scheme = "git-commit", Value = "d5028af32cf16fe48878ffef0301867175f98f08" },
            new() { ContractVersion = "1", ImplementationId = "eternal-cycle-dotnet", ImplementationVersion = "1.0.0" }));
        var result = RuleCompilationPipeline.Compile(snapshot);
        AssertValid(result);
        Assert.Equal(10, result.Artifact!.RuleSources.Count);
        Assert.Equal(154, result.Artifact.Snippets.Count);
        Assert.Equal(223929, result.Bytes.Length);
        Assert.Equal("4B583A7904CE514B56CF3B497648DB5D778FC04DF2285D30A2E03823A32EA6A4", result.Artifact.Integrity.ArtifactSha256);
        Assert.Equal("802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC", Sha256(result.Bytes));
    }

    private static void AssertValid(RuleCompilationResult result)
    {
        Assert.True(result.IsValid, string.Join("; ", result.Errors));
        Assert.Empty(CompiledRulesArtifactContract.Validate(result.Artifact!));
        Assert.True(CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(result.Bytes.Span)).IsValid);
    }

    private static CompiledRulesArtifactSnippet Snippet(RuleCompilationResult result, string id) => result.Artifact!.Snippets.Single(snippet => snippet.SnippetId == id);
    private static string SourceSemantics(RuleCompilationResult result) => JsonSerializer.Serialize(result.Artifact!.RuleSources);
    private static string RetrievalSemantics(RuleCompilationResult result) => JsonSerializer.Serialize(result.Artifact!.Snippets.Select(snippet => snippet.Retrieval));
    private static string ContentSemantics(RuleCompilationResult result) => JsonSerializer.Serialize(result.Artifact!.Snippets.Select(snippet => new { snippet.SnippetId, snippet.Content, snippet.ContentSha256, snippet.EstimatedTokens }));
    private static string Sha256(ReadOnlyMemory<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes.Span));
    private static void Reverse<T>(IList<T> items)
    {
        for (var index = 0; index < items.Count / 2; index++)
        {
            var other = items.Count - index - 1;
            (items[index], items[other]) = (items[other], items[index]);
        }
    }
}
