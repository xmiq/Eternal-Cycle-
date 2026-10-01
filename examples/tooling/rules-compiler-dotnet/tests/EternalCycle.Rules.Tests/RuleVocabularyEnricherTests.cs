using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class RuleVocabularyEnricherTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AbsentOrEmptyVocabularyIsNeutral(bool declared)
    {
        using var payload = new Payload();
        payload.Vocabulary = declared ? Definition([], []) : null;
        var snapshot = payload.Load();
        var candidates = RuleSnippetCompiler.Compile(snapshot);
        var result = RuleVocabularyEnricher.Enrich(snapshot, candidates);

        Assert.All(result.Snippets, snippet => Assert.Empty(snippet.Associations));
        AssertUnchanged(candidates, result);
        Assert.Equal(snapshot.ManifestSha256, result.ManifestSha256);
    }

    [Fact]
    public void ExactSnippetBindingDoesNotLeakIntoSibling()
    {
        using var payload = new Payload();
        var result = payload.Enrich();

        Assert.Equal("combat", Assert.Single(Snippet(result, "core#combat").Associations).RetrievalTerm.Term);
        Assert.Empty(Snippet(result, "core#progression").Associations);
    }

    [Fact]
    public void AlternativesPreserveKindsPhrasesPunctuationAndWeights()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("combat", " Combat ", 801,
            Alternative(" Fight ", RuleVocabularyAlternativeKind.Alias, 601),
            Alternative("  Hand-to-Hand\t Combat! ", RuleVocabularyAlternativeKind.Phrase, 401),
            Alternative("Cafe\u0301", RuleVocabularyAlternativeKind.Phrase, 201))], [Binding("combat")]);
        var terms = Snippet(payload.Enrich(), "core#combat").Associations;

        Assert.Equal(["caf\u00e9", "combat", "fight", "hand-to-hand combat!"], terms.Select(term => term.RetrievalTerm.Term));
        Assert.Equal(["phrase", "canonical", "alias", "phrase"], terms.Select(term => term.RetrievalTerm.Kind));
        Assert.Equal([201, 801, 601, 401], terms.Select(term => term.RetrievalTerm.Weight));
        Assert.All(terms, term => Assert.Equal("combat", term.CanonicalTerm));
    }

    [Fact]
    public void SourceScopeReachesAllAndOnlyItsSourcesCandidates()
    {
        using var payload = new Payload();
        payload.AddSource("operations", "A heading-free rule.");
        payload.Vocabulary = Definition([Concept("combat")], [Binding("combat", anchor: null, scope: RuleVocabularyBindingScope.Source)]);
        var result = payload.Enrich();

        Assert.Single(Snippet(result, "core#combat").Associations);
        Assert.Single(Snippet(result, "core#progression").Associations);
        Assert.Empty(Snippet(result, "operations").Associations);
        Assert.All(result.Snippets.SelectMany(snippet => snippet.Associations), association =>
            Assert.Equal(RuleVocabularyBindingScope.Source, Assert.Single(association.Origins).Binding.Scope));
    }

    [Fact]
    public void NullSnippetAnchorTargetsOnlyUnanchoredSnippet()
    {
        using var payload = new Payload();
        payload.AddSource("operations", "A heading-free rule.");
        payload.Vocabulary = Definition([Concept("persistence")],
            [Binding("persistence", source: "operations", anchor: null)]);
        var result = payload.Enrich();

        Assert.Single(Snippet(result, "operations").Associations);
        Assert.All(result.Snippets.Where(snippet => snippet.Candidate.RuleSourceId == "core"),
            snippet => Assert.Empty(snippet.Associations));
    }

    [Fact]
    public void MultipleExplicitTargetsPreserveOriginalSnippetSequence()
    {
        using var payload = new Payload();
        payload.AddSource("operations", "## Authority\nOne rule.\n## Save\nAnother rule.");
        payload.Vocabulary = Definition([Concept("persistence"), Concept("progression")],
            [Binding("persistence", source: "operations", anchor: "save"), Binding("progression", anchor: "progression")]);
        var snapshot = payload.Load();
        var candidates = RuleSnippetCompiler.Compile(snapshot);
        var result = RuleVocabularyEnricher.Enrich(snapshot, candidates);

        Assert.Equal(["core#combat", "core#progression", "operations#authority", "operations#save"],
            result.Snippets.Select(snippet => snippet.Candidate.SnippetId));
        Assert.Equal(["core#progression", "operations#save"],
            result.Snippets.Where(snippet => snippet.Associations.Count > 0).Select(snippet => snippet.Candidate.SnippetId));
        AssertUnchanged(candidates, result);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("Combat")]
    [InlineData("comb")]
    public void MissingCaseChangedOrSubstringAnchorFailsExactly(string anchor)
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("combat")], [Binding("combat", anchor: anchor)]);
        AssertError("VOCABULARY_TARGET_MISSING", payload.Enrich);
    }

    [Fact]
    public void NullAnchorDoesNotMeanAllAnchoredSnippets()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("combat")], [Binding("combat", anchor: null)]);
        AssertError("VOCABULARY_TARGET_MISSING", payload.Enrich);
    }

    [Fact]
    public void SourceBindingWithoutAnyCompiledTargetFails()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("combat")], [Binding("combat", anchor: null, scope: RuleVocabularyBindingScope.Source)]);
        var snapshot = payload.Load();
        AssertError("VOCABULARY_TARGET_MISSING", () => RuleVocabularyEnricher.Enrich(snapshot, []));
    }

    [Theory]
    [InlineData("duplicate", "VOCABULARY_TARGET_AMBIGUOUS")]
    [InlineData("source", "VOCABULARY_CANDIDATE_SOURCE_MISSING")]
    [InlineData("id", "VOCABULARY_CANDIDATE_INCONSISTENT")]
    [InlineData("path", "VOCABULARY_CANDIDATE_INCONSISTENT")]
    [InlineData("hash", "VOCABULARY_CANDIDATE_INCONSISTENT")]
    [InlineData("anchor", "IDENTIFIER_INVALID")]
    [InlineData("null", "VOCABULARY_CANDIDATE_INVALID")]
    public void InvalidCandidateTargetsFailStructurally(string defect, string code)
    {
        using var payload = new Payload();
        var snapshot = payload.Load();
        var candidates = RuleSnippetCompiler.Compile(snapshot).ToList();
        var first = candidates[0];
        switch (defect)
        {
            case "duplicate": candidates.Add(first); break;
            case "source": candidates[0] = first with { RuleSourceId = "missing" }; break;
            case "id": candidates[0] = first with { SnippetId = "core#invented" }; break;
            case "path": candidates[0] = first with { SourcePath = "wrong.md" }; break;
            case "hash": candidates[0] = first with { SourceSha256 = new string('0', 64) }; break;
            case "anchor": candidates[0] = first with { SourceAnchor = "../escape" }; break;
            case "null": candidates[0] = null!; break;
        }
        AssertError(code, () => RuleVocabularyEnricher.Enrich(snapshot, candidates));
    }

    [Theory]
    [InlineData("source", "VOCABULARY_SOURCE_MISSING")]
    [InlineData("concept", "VOCABULARY_CONCEPT_MISSING")]
    [InlineData("target", "IDENTIFIER_INVALID")]
    [InlineData("scope", "VOCABULARY_SCOPE_INVALID")]
    [InlineData("duplicate-binding", "VOCABULARY_BINDING_DUPLICATE")]
    [InlineData("duplicate-reference", "VOCABULARY_CONCEPT_REFERENCE_DUPLICATE")]
    [InlineData("canonical-alias", "VOCABULARY_TERM_DUPLICATE")]
    public void EditedPublicInputReusesAContractChecks(string defect, string code)
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("combat")], [Binding("combat"), Binding("combat", anchor: "progression")]);
        var snapshot = payload.Load();
        var candidates = RuleSnippetCompiler.Compile(snapshot);
        var definition = snapshot.Manifest.RetrievalVocabulary!;
        switch (defect)
        {
            case "source": definition.Bindings[0] = Binding("combat", source: "missing"); break;
            case "concept": definition.Bindings[0] = Binding("missing"); break;
            case "target": definition.Bindings[0] = Binding("combat", anchor: "../escape"); break;
            case "scope": definition.Bindings[0] = Binding("combat", scope: RuleVocabularyBindingScope.Source, anchor: "combat"); break;
            case "duplicate-binding": definition.Bindings[1] = definition.Bindings[0]; break;
            case "duplicate-reference": definition.Bindings[0] = Binding("combat", conceptIds: ["combat", "combat"]); break;
            case "canonical-alias": definition.Concepts[0] = Concept("combat", alternatives: [Alternative(" COMBAT ", RuleVocabularyAlternativeKind.Alias, 600)]); break;
        }
        AssertError(code, () => RuleVocabularyEnricher.Enrich(snapshot, candidates));
    }

    [Fact]
    public void OverlappingBindingsCoalesceAssociationWithoutLosingOrigins()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("combat", alternatives: [Alternative("fight", RuleVocabularyAlternativeKind.Alias, 601)])],
            [Binding("combat", anchor: null, scope: RuleVocabularyBindingScope.Source), Binding("combat")]);
        var result = payload.Enrich();
        var target = Snippet(result, "core#combat");

        Assert.Equal(2, target.Associations.Count);
        Assert.All(target.Associations, association =>
        {
            Assert.Equal([RuleVocabularyBindingScope.Snippet, RuleVocabularyBindingScope.Source],
                association.Origins.Select(origin => origin.Binding.Scope));
            Assert.Equal(2, association.Origins.Distinct().Count());
        });
        Assert.All(Snippet(result, "core#progression").Associations, association => Assert.Single(association.Origins));
    }

    [Fact]
    public void SharedTermPreservesDistinctConceptsOriginsAndWeights()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([
            Concept("combat", alternatives: [Alternative("conflict", RuleVocabularyAlternativeKind.Alias, 301)]),
            Concept("authority", "rules authority", alternatives: [Alternative("conflict", RuleVocabularyAlternativeKind.Phrase, 101)]),
            Concept("persistence", alternatives: [Alternative("conflict", RuleVocabularyAlternativeKind.Alias, 201)])],
            [Binding("combat", conceptIds: ["persistence", "authority", "combat"])]);
        var associations = Snippet(payload.Enrich(), "core#combat").Associations
            .Where(association => association.RetrievalTerm.Term == "conflict").ToArray();

        Assert.Equal(["combat", "persistence", "authority"], associations.Select(association => association.ConceptId));
        Assert.Equal([301, 201, 101], associations.Select(association => association.RetrievalTerm.Weight));
        Assert.Equal(["alias", "alias", "phrase"], associations.Select(association => association.RetrievalTerm.Kind));
        Assert.All(associations, association => Assert.Single(association.Origins));
    }

    [Fact]
    public void ReviewedTermCanLegitimatelyTargetSeveralSnippets()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("combat")], [Binding("combat"), Binding("combat", anchor: "progression")]);
        var result = payload.Enrich();

        Assert.Equal(2, result.Snippets.Count);
        Assert.All(result.Snippets, snippet => Assert.Equal("combat", Assert.Single(snippet.Associations).ConceptId));
        Assert.NotEqual(result.Snippets[0].Associations[0].Origins[0].Binding,
            result.Snippets[1].Associations[0].Origins[0].Binding);
    }

    [Fact]
    public void EvidenceExplainsConceptTermTargetBindingAndRationales()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("combat", alternatives: [Alternative("fight", RuleVocabularyAlternativeKind.Alias, 609)])],
            [Binding("combat")]);
        var result = payload.Enrich();
        var snippet = Snippet(result, "core#combat");
        var alias = snippet.Associations.Single(association => association.RetrievalTerm.Kind == "alias");
        var origin = Assert.Single(alias.Origins);

        Assert.Equal("combat", alias.ConceptId);
        Assert.Equal("combat", alias.CanonicalTerm);
        Assert.Equal("fight", alias.RetrievalTerm.Term);
        Assert.Equal(609, alias.RetrievalTerm.Weight);
        Assert.Equal(new RuleVocabularyBindingIdentity("core", RuleVocabularyBindingScope.Snippet, "combat"), origin.Binding);
        Assert.Equal("Reviewed concept combat.", origin.ConceptRationale);
        Assert.Equal("Reviewed term fight.", origin.TermRationale);
        Assert.Equal("Reviewed target core/combat.", origin.BindingRationale);
        Assert.Equal("manifest.json", result.ManifestPath);
        Assert.NotEmpty(result.ManifestSha256);
    }

    [Fact]
    public void RepeatedEnrichmentIsIdenticalAndDoesNotMultiplyOrigins()
    {
        using var payload = new Payload();
        var snapshot = payload.Load();
        var candidates = RuleSnippetCompiler.Compile(snapshot);
        var first = RuleVocabularyEnricher.Enrich(snapshot, candidates);
        var second = RuleVocabularyEnricher.Enrich(snapshot, candidates);
        Assert.Equal(Json(first), Json(second));
        Assert.Single(Assert.Single(Snippet(second, "core#combat").Associations).Origins);
    }

    [Fact]
    public void DefinitionInsertionOrderDoesNotChangeEnrichment()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([
            Concept("progression", alternatives: [Alternative("growth", RuleVocabularyAlternativeKind.Alias, 601), Alternative("earned progress", RuleVocabularyAlternativeKind.Phrase, 501)]),
            Concept("combat")], [Binding("combat", conceptIds: ["progression", "combat"]), Binding("combat", anchor: null, scope: RuleVocabularyBindingScope.Source)]);
        var snapshot = payload.Load();
        var candidates = RuleSnippetCompiler.Compile(snapshot);
        var first = RuleVocabularyEnricher.Enrich(snapshot, candidates);
        var definition = snapshot.Manifest.RetrievalVocabulary!;
        Reverse(definition.Concepts);
        Reverse(definition.Bindings);
        foreach (var concept in definition.Concepts) { Reverse(concept.Alternatives); }
        foreach (var binding in definition.Bindings) { Reverse(binding.ConceptIds); }
        var second = RuleVocabularyEnricher.Enrich(snapshot, candidates);
        Assert.Equal(Json(first), Json(second));
    }

    [Fact]
    public void EquivalentReviewedDeclarationOrderRetainsAssociationsButChangesByteProvenance()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("combat"), Concept("progression")],
            [Binding("combat"), Binding("progression", anchor: "progression")]);
        var first = payload.Enrich();
        Reverse(payload.Vocabulary.Concepts);
        Reverse(payload.Vocabulary.Bindings);
        var second = payload.Enrich();
        Assert.Equal(Json(first.Snippets), Json(second.Snippets));
        Assert.NotEqual(first.ManifestSha256, second.ManifestSha256);
    }

    [Fact]
    public void CultureDoesNotChangeEnrichment()
    {
        using var payload = new Payload();
        payload.Vocabulary = Definition([Concept("identity", " IDENTITY ", alternatives: [Alternative("Cafe\u0301", RuleVocabularyAlternativeKind.Phrase, 503)])],
            [Binding("identity")]);
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var first = payload.Enrich();
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var second = payload.Enrich();
            Assert.Equal(Json(first), Json(second));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void RelocationDoesNotEnterEnrichmentOrBindingIdentity()
    {
        using var firstPayload = new Payload();
        using var relocatedPayload = new Payload();
        var first = firstPayload.Enrich();
        var relocated = relocatedPayload.Enrich();

        Assert.Equal(Json(first), Json(relocated));
        Assert.DoesNotContain(firstPayload.Root, Json(first));
        Assert.DoesNotContain(relocatedPayload.Root, Json(relocated));
    }

    [Fact]
    public void EnrichmentReadsOnlySuppliedMemoryAndNeverGeneratesProseOrHeadingTerms()
    {
        using var payload = new Payload();
        var snapshot = payload.Load();
        var candidates = RuleSnippetCompiler.Compile(snapshot);
        File.Delete(Path.Combine(payload.Root, "manifest.json"));
        File.Delete(Path.Combine(payload.Root, "core.md"));
        var result = RuleVocabularyEnricher.Enrich(snapshot, candidates);

        Assert.Equal(["combat"], result.Snippets.SelectMany(snippet => snippet.Associations).Select(association => association.RetrievalTerm.Term));
        Assert.Empty(Snippet(result, "core#progression").Associations);
        AssertUnchanged(candidates, result);
    }

    [Fact]
    public void HistoricalCanonicalNoVocabularyCorpusRetainsLegacyArtifactBytesAndAllCandidateSemantics()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "docs", "rules", "rule-source-manifest.json")))
        {
            directory = directory.Parent;
        }
        Assert.NotNull(directory);
        var snapshot = CanonicalVocabularyPayload.LoadHistorical(directory.FullName);
        var candidates = RuleSnippetCompiler.Compile(snapshot);
        var result = RuleVocabularyEnricher.Enrich(snapshot, candidates);
        var assembly = CompiledRulesArtifactAssembler.Assemble(snapshot, result.Snippets.Select(snippet => snippet.Candidate).ToArray());
        Assert.True(assembly.IsValid);
        var write = CompiledRulesArtifactWriter.Write(assembly.Artifact!);

        Assert.Equal(10, snapshot.Sources.Count);
        Assert.Equal(154, result.Snippets.Count);
        Assert.All(result.Snippets, snippet => Assert.Empty(snippet.Associations));
        AssertUnchanged(candidates, result);
        Assert.True(write.IsValid);
        Assert.Equal("4B583A7904CE514B56CF3B497648DB5D778FC04DF2285D30A2E03823A32EA6A4", assembly.Artifact!.Integrity.ArtifactSha256);
        Assert.Equal("802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC", Convert.ToHexString(SHA256.HashData(write.Bytes.Span)));
    }

    private static void AssertUnchanged(IReadOnlyList<RuleSnippetCandidate> candidates, RuleVocabularyEnrichment result)
    {
        Assert.Equal(candidates.Count, result.Snippets.Count);
        for (var index = 0; index < candidates.Count; index++)
        {
            Assert.Same(candidates[index], result.Snippets[index].Candidate);
            Assert.Empty(candidates[index].Retrieval.Terms);
            Assert.Empty(candidates[index].Retrieval.Relationships);
        }
    }

    private static RuleSnippetVocabulary Snippet(RuleVocabularyEnrichment result, string id) =>
        result.Snippets.Single(snippet => snippet.Candidate.SnippetId == id);

    private static void AssertError(string code, Func<RuleVocabularyEnrichment> action)
    {
        var error = Assert.Throws<RuleVocabularyEnrichmentException>(action);
        Assert.Equal(code, error.Code);
        Assert.StartsWith("$.", error.Path);
        Assert.True(error.Message.Length < 256);
    }

    private static void Reverse<T>(IList<T> items)
    {
        for (var index = 0; index < items.Count / 2; index++)
        {
            var other = items.Count - index - 1;
            (items[index], items[other]) = (items[other], items[index]);
        }
    }

    private static string Json<T>(T value) => JsonSerializer.Serialize(value, Payload.Options);

    private static RuleRetrievalVocabularyDefinition Definition(
        IList<RuleVocabularyConcept> concepts, IList<RuleVocabularyBinding> bindings) =>
        new() { VocabularyFormatVersion = 1, Concepts = concepts, Bindings = bindings };

    private static RuleVocabularyConcept Concept(string id, string? term = null, int weight = 801,
        params RuleVocabularyAlternative[] alternatives) =>
        new() { ConceptId = id, CanonicalTerm = term ?? id, Weight = weight, Alternatives = alternatives.ToList(), Rationale = $"Reviewed concept {id}." };

    private static RuleVocabularyAlternative Alternative(string term, RuleVocabularyAlternativeKind kind, int weight) =>
        new() { Term = term, Kind = kind, Weight = weight, Rationale = $"Reviewed term {RuleRetrievalVocabulary.NormalizeTerm(term)}." };

    private static RuleVocabularyBinding Binding(string concept, string source = "core", string? anchor = "combat",
        RuleVocabularyBindingScope scope = RuleVocabularyBindingScope.Snippet, IList<string>? conceptIds = null) =>
        new()
        {
            RuleSourceId = source,
            Scope = scope,
            SourceAnchor = anchor,
            ConceptIds = conceptIds ?? new List<string> { concept },
            Rationale = $"Reviewed target {source}/{anchor}."
        };

    private sealed class Payload : IDisposable
    {
        internal static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        private readonly List<RuleSourceManifestEntry> sources = [];
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "EternalCycle.Enrichment.Tests", Guid.NewGuid().ToString("N"));
        public RuleRetrievalVocabularyDefinition? Vocabulary { get; set; } = Definition([Concept("combat")], [Binding("combat")]);

        public Payload()
        {
            Directory.CreateDirectory(Root);
            AddSource("core", "## Combat\nCombat and fighting.\n## Progression\nEarned progress and levels.");
        }

        public void AddSource(string id, string text)
        {
            sources.Add(new() { RuleSourceId = id, Path = $"{id}.md", Layer = RuleLayer.Core });
            File.WriteAllText(Path.Combine(Root, $"{id}.md"), text, new UTF8Encoding(false));
        }

        public MaterializedRuleSourceSnapshot Load()
        {
            var manifest = new RuleSourceManifest
            {
                ManifestFormatVersion = 1, CompilerContractVersion = "1", RulesetId = "test-rules", RepositoryVersion = "test",
                Sources = sources, RetrievalVocabulary = Vocabulary
            };
            File.WriteAllText(Path.Combine(Root, "manifest.json"), Json(manifest), new UTF8Encoding(false));
            return MaterializedRuleSourceLoader.Load(new(Root, "manifest.json",
                new() { Scheme = "test", Value = "immutable-input" },
                new() { ContractVersion = "1", ImplementationId = "vocabulary-tests", ImplementationVersion = "1" }));
        }

        public RuleVocabularyEnrichment Enrich()
        {
            var snapshot = Load();
            return RuleVocabularyEnricher.Enrich(snapshot, RuleSnippetCompiler.Compile(snapshot));
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);
    }
}
