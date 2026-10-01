using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using EternalCycle.Rules;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class RuleRetrievalVocabularyInputTests
{
    [Theory]
    [InlineData("  Campaign\t\nCanon  ", "campaign canon")]
    [InlineData("Cafe\u0301", "caf\u00e9")]
    [InlineData("Soul-Bound", "soul-bound")]
    [InlineData("experience points", "experience points")]
    [InlineData(" Save! ", "save!")]
    public void NormalizationPreservesPhrasesAndPunctuation(string input, string expected) =>
        Assert.Equal(expected, RuleRetrievalVocabulary.NormalizeTerm(input));

    [Theory]
    [InlineData("")]
    [InlineData(" \t")]
    [InlineData("save\0status")]
    public void InvalidTermsAreRejected(string input) =>
        Assert.Throws<ArgumentException>(() => RuleRetrievalVocabulary.NormalizeTerm(input));

    [Fact]
    public void NormalizationIsInvariantAndIdempotent()
    {
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var normalized = RuleRetrievalVocabulary.NormalizeTerm("  IDENTITY   Cafe\u0301  ");
            Assert.Equal("identity caf\u00e9", normalized);
            Assert.Equal(normalized, RuleRetrievalVocabulary.NormalizeTerm(normalized));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public void NormalizationDoesNotInventSemanticEquivalence()
    {
        Assert.NotEqual(Normalize("campaign canon"), Normalize("repository canon"));
        Assert.NotEqual(Normalize("soul-bound"), Normalize("soul bound"));
        Assert.NotEqual(Normalize("level"), Normalize("leveling"));
        Assert.NotEqual(Normalize("save"), Normalize("saving"));
        Assert.NotEqual(Normalize("conflict"), Normalize("combat"));
    }

    [Fact]
    public void LoadsReviewedConceptAlternativesAndPreciseBinding()
    {
        using var payload = new Payload();
        var snapshot = payload.Load();
        var vocabulary = snapshot.Manifest.RetrievalVocabulary!;

        Assert.Equal(1, vocabulary.VocabularyFormatVersion);
        Assert.Equal("campaign-canon", vocabulary.Concepts[0].ConceptId);
        Assert.Equal("campaign canon", vocabulary.Concepts[0].CanonicalTerm);
        Assert.Equal("established facts", vocabulary.Concepts[0].Alternatives[0].Term);
        Assert.Equal(RuleVocabularyAlternativeKind.Phrase, vocabulary.Concepts[0].Alternatives[0].Kind);
        Assert.Equal(600, vocabulary.Concepts[0].Alternatives[0].Weight);
        Assert.Equal("authority", vocabulary.Bindings[0].SourceAnchor);
        Assert.Equal(RuleVocabularyBindingScope.Snippet, vocabulary.Bindings[0].Scope);
        Assert.Contains("campaign", vocabulary.Bindings[0].Rationale);
    }

    [Fact]
    public void AbsentVocabularyPreservesLegacyInputAndEmptyRetrieval()
    {
        using var payload = new Payload();
        payload.Manifest.Remove("retrievalVocabulary");
        var snapshot = payload.Load();
        var serializedManifest = JsonSerializer.Serialize(snapshot.Manifest, Payload.JsonOptions);
        var candidates = RuleSnippetCompiler.Compile(snapshot);

        Assert.Null(snapshot.Manifest.RetrievalVocabulary);
        Assert.DoesNotContain("retrievalVocabulary", serializedManifest);
        Assert.All(candidates, candidate => Assert.Empty(candidate.Retrieval.Terms));
        var artifact = CompiledRulesArtifactAssembler.Assemble(snapshot, candidates);
        Assert.True(artifact.IsValid);
    }

    [Fact]
    public void InputPackageDoesNotEnrichCandidatesPrematurely()
    {
        using var payload = new Payload();
        var candidates = RuleSnippetCompiler.Compile(payload.Load());
        Assert.All(candidates, candidate =>
        {
            Assert.Empty(candidate.Retrieval.Terms);
            Assert.Empty(candidate.Retrieval.Relationships);
        });
    }

    [Fact]
    public void RetainsExactManifestBytesAndHashesIncludingVocabulary()
    {
        using var payload = new Payload();
        var first = payload.Load();
        Assert.Equal(payload.LastManifestBytes, first.AuthoritativeManifestBytes.ToArray());
        Assert.Equal(MaterializedRuleSourceLoader.ComputeSha256(payload.LastManifestBytes), first.ManifestSha256);
        payload.Concept["rationale"] = "Revised reviewed navigation rationale.";
        var second = payload.Load();

        Assert.NotEqual(first.ManifestSha256, second.ManifestSha256);
        Assert.Equal(first.Sources[0].SourceSha256, second.Sources[0].SourceSha256);
        Assert.Equal(RuleSnippetCompiler.Compile(first)[0].SnippetId, RuleSnippetCompiler.Compile(second)[0].SnippetId);
    }

    [Fact]
    public void DefinitionOrderingIsDeterministicWithoutRewritingProvenance()
    {
        using var payload = new Payload();
        var second = payload.Concept.DeepClone().AsObject();
        second["conceptId"] = "a-concept";
        second["canonicalTerm"] = "rules authority";
        payload.Vocabulary["concepts"]!.AsArray().Add(second);
        payload.Binding["conceptIds"]!.AsArray().Add("a-concept");
        var first = payload.Load();
        payload.Vocabulary["concepts"] = new JsonArray(second.DeepClone(), payload.Concept.DeepClone());
        payload.Binding["conceptIds"] = new JsonArray("a-concept", "campaign-canon");
        var reordered = payload.Load();

        Assert.Equal(
            JsonSerializer.Serialize(first.Manifest.RetrievalVocabulary, Payload.JsonOptions),
            JsonSerializer.Serialize(reordered.Manifest.RetrievalVocabulary, Payload.JsonOptions));
        Assert.NotEqual(first.ManifestSha256, reordered.ManifestSha256);
    }

    [Fact]
    public void RepeatedAndRelocatedPayloadPreservesBytesAndDefinition()
    {
        using var firstPayload = new Payload();
        using var secondPayload = new Payload();
        var first = firstPayload.Load();
        var repeat = firstPayload.Load();
        var relocated = secondPayload.Load();
        Assert.Equal(first.ManifestSha256, repeat.ManifestSha256);
        Assert.Equal(first.ManifestSha256, relocated.ManifestSha256);
        Assert.Equal(
            JsonSerializer.Serialize(first.Manifest.RetrievalVocabulary, Payload.JsonOptions),
            JsonSerializer.Serialize(relocated.Manifest.RetrievalVocabulary, Payload.JsonOptions));
    }

    [Fact]
    public void ExplicitSourceInheritanceAndUnanchoredSnippetAreDistinct()
    {
        using var payload = new Payload();
        payload.Binding["scope"] = "Source";
        payload.Binding["sourceAnchor"] = null;
        var unanchored = payload.Binding.DeepClone().AsObject();
        unanchored["scope"] = "Snippet";
        payload.Vocabulary["bindings"]!.AsArray().Add(unanchored);
        var vocabulary = payload.Load().Manifest.RetrievalVocabulary!;
        Assert.Equal(2, vocabulary.Bindings.Count);
        Assert.All(vocabulary.Bindings, binding => Assert.Null(binding.SourceAnchor));
    }

    [Fact]
    public void SharedTermsAcrossConceptsRemainAvailableForLaterAmbiguityAudit()
    {
        using var payload = new Payload();
        var other = payload.Concept.DeepClone().AsObject();
        other["conceptId"] = "repository-canon";
        other["canonicalTerm"] = "repository canon";
        payload.Vocabulary["concepts"]!.AsArray().Add(other);
        Assert.Equal(2, payload.Load().Manifest.RetrievalVocabulary!.Concepts.Count);
    }

    [Theory]
    [InlineData("null-block", "VOCABULARY_STRUCTURE_INVALID")]
    [InlineData("version", "VOCABULARY_FORMAT_UNSUPPORTED")]
    [InlineData("null-concepts", "VOCABULARY_STRUCTURE_INVALID")]
    [InlineData("null-bindings", "VOCABULARY_STRUCTURE_INVALID")]
    [InlineData("null-binding", "VOCABULARY_STRUCTURE_INVALID")]
    [InlineData("null-references", "VOCABULARY_STRUCTURE_INVALID")]
    [InlineData("null-concept", "VOCABULARY_STRUCTURE_INVALID")]
    [InlineData("null-alternatives", "VOCABULARY_STRUCTURE_INVALID")]
    [InlineData("null-alternative", "VOCABULARY_STRUCTURE_INVALID")]
    [InlineData("duplicate-concept", "VOCABULARY_CONCEPT_DUPLICATE")]
    [InlineData("duplicate-term", "VOCABULARY_TERM_DUPLICATE")]
    [InlineData("canonical-alias", "VOCABULARY_TERM_DUPLICATE")]
    [InlineData("invalid-term", "VOCABULARY_TERM_INVALID")]
    [InlineData("long-term", "VOCABULARY_TERM_INVALID")]
    [InlineData("canonical-weight", "VOCABULARY_WEIGHT_INVALID")]
    [InlineData("alternative-weight", "VOCABULARY_WEIGHT_INVALID")]
    [InlineData("rationale", "VOCABULARY_RATIONALE_INVALID")]
    [InlineData("rationale-control", "VOCABULARY_RATIONALE_INVALID")]
    [InlineData("concept-id", "IDENTIFIER_INVALID")]
    [InlineData("missing-source", "VOCABULARY_SOURCE_MISSING")]
    [InlineData("missing-concept", "VOCABULARY_CONCEPT_MISSING")]
    [InlineData("duplicate-reference", "VOCABULARY_CONCEPT_REFERENCE_DUPLICATE")]
    [InlineData("duplicate-binding", "VOCABULARY_BINDING_DUPLICATE")]
    [InlineData("source-anchor", "VOCABULARY_SCOPE_INVALID")]
    [InlineData("anchor-shape", "IDENTIFIER_INVALID")]
    [InlineData("empty-binding", "VOCABULARY_STRUCTURE_INVALID")]
    [InlineData("unknown-property", "MANIFEST_JSON_INVALID")]
    [InlineData("missing-property", "MANIFEST_JSON_INVALID")]
    [InlineData("missing-anchor-property", "MANIFEST_JSON_INVALID")]
    [InlineData("unknown-scope", "MANIFEST_JSON_INVALID")]
    [InlineData("numeric-scope", "MANIFEST_JSON_INVALID")]
    [InlineData("unknown-kind", "MANIFEST_JSON_INVALID")]
    [InlineData("numeric-kind", "MANIFEST_JSON_INVALID")]
    public void RejectsInvalidVocabularyWithStructuredFailure(string defect, string expectedCode)
    {
        using var payload = new Payload();
        var concept = payload.Concept;
        var alternative = concept["alternatives"]![0]!.AsObject();
        switch (defect)
        {
            case "null-block": payload.Manifest["retrievalVocabulary"] = null; break;
            case "version": payload.Vocabulary["vocabularyFormatVersion"] = 2; break;
            case "null-concepts": payload.Vocabulary["concepts"] = null; break;
            case "null-bindings": payload.Vocabulary["bindings"] = null; break;
            case "null-binding": payload.Vocabulary["bindings"]![0] = null; break;
            case "null-references": payload.Binding["conceptIds"] = null; break;
            case "null-concept": payload.Vocabulary["concepts"]![0] = null; break;
            case "null-alternatives": concept["alternatives"] = null; break;
            case "null-alternative": concept["alternatives"]![0] = null; break;
            case "duplicate-concept": payload.Vocabulary["concepts"]!.AsArray().Add(concept.DeepClone()); break;
            case "duplicate-term": concept["alternatives"]!.AsArray().Add(alternative.DeepClone()); break;
            case "canonical-alias": alternative["term"] = " CAMPAIGN\tCANON "; break;
            case "invalid-term": alternative["term"] = "bad\0term"; break;
            case "long-term": alternative["term"] = new string('a', 257); break;
            case "canonical-weight": concept["weight"] = 0; break;
            case "alternative-weight": alternative["weight"] = 1001; break;
            case "rationale": payload.Binding["rationale"] = ""; break;
            case "rationale-control": concept["rationale"] = "bad\nreview"; break;
            case "concept-id": concept["conceptId"] = "bad/id"; break;
            case "missing-source": payload.Binding["ruleSourceId"] = "absent"; break;
            case "missing-concept": payload.Binding["conceptIds"] = new JsonArray("absent"); break;
            case "duplicate-reference": payload.Binding["conceptIds"]!.AsArray().Add("campaign-canon"); break;
            case "duplicate-binding": payload.Vocabulary["bindings"]!.AsArray().Add(payload.Binding.DeepClone()); break;
            case "source-anchor": payload.Binding["scope"] = "Source"; break;
            case "anchor-shape": payload.Binding["sourceAnchor"] = "../escape"; break;
            case "empty-binding": payload.Binding["conceptIds"] = new JsonArray(); break;
            case "unknown-property": alternative["generatedAt"] = "unwanted"; break;
            case "missing-property": alternative.Remove("weight"); break;
            case "missing-anchor-property": payload.Binding.Remove("sourceAnchor"); break;
            case "unknown-scope": payload.Binding["scope"] = "Automatic"; break;
            case "numeric-scope": payload.Binding["scope"] = 0; break;
            case "unknown-kind": alternative["kind"] = "Thesaurus"; break;
            case "numeric-kind": alternative["kind"] = 0; break;
            default: throw new ArgumentOutOfRangeException(nameof(defect));
        }
        var error = Assert.Throws<MaterializedRuleSourceException>(() => payload.Load());
        Assert.Equal(expectedCode, error.Code);
        Assert.StartsWith("$.retrievalVocabulary", error.Path);
    }

    [Fact]
    public void DuplicateNestedJsonPropertyIsRejected()
    {
        using var payload = new Payload();
        var text = payload.Manifest.ToJsonString().Replace(
            "\"kind\":\"Phrase\"", "\"kind\":\"Phrase\",\"kind\":\"Alias\"", StringComparison.Ordinal);
        var error = Assert.Throws<MaterializedRuleSourceException>(() => payload.Load(text));
        Assert.Equal("JSON_PROPERTY_DUPLICATE", error.Code);
    }

    private static string Normalize(string value) => RuleRetrievalVocabulary.NormalizeTerm(value);

    private sealed class Payload : IDisposable
    {
        public static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public Payload()
        {
            Directory.CreateDirectory(Root);
            File.WriteAllText(Path.Combine(Root, "core.md"), "# Core\n\n## Authority\nCampaign facts have an owner.\n", new UTF8Encoding(false));
        }

        public string Root { get; } = Path.Combine(Path.GetTempPath(), "EternalCycle.Vocabulary.Tests", Guid.NewGuid().ToString("N"));
        public byte[] LastManifestBytes { get; private set; } = [];
        public JsonObject Vocabulary => Manifest["retrievalVocabulary"]!.AsObject();
        public JsonObject Concept => Vocabulary["concepts"]![0]!.AsObject();
        public JsonObject Binding => Vocabulary["bindings"]![0]!.AsObject();

        public JsonObject Manifest { get; } = JsonNode.Parse("""
        {
          "manifestFormatVersion": 1,
          "compilerContractVersion": "1",
          "rulesetId": "generic-rules",
          "repositoryVersion": "test",
          "sources": [{
            "ruleSourceId": "core", "path": "core.md", "layer": "Core",
            "worldModelIds": [], "moduleIds": [], "campaignModes": ["*"],
            "operations": ["*"], "topics": [], "priority": 0,
            "alwaysInclude": false, "preparationTier": "Standard"
          }],
          "retrievalVocabulary": {
            "vocabularyFormatVersion": 1,
            "concepts": [{
              "conceptId": "campaign-canon", "canonicalTerm": " Campaign   Canon ", "weight": 800,
              "alternatives": [{
                "term": "Established Facts", "kind": "Phrase", "weight": 600,
                "rationale": "Reviewed alternate wording for established campaign facts."
              }],
              "rationale": "Names the campaign authority concept."
            }],
            "bindings": [{
              "ruleSourceId": "core", "scope": "Snippet", "sourceAnchor": "authority",
              "conceptIds": ["campaign-canon"], "rationale": "This section defines campaign fact ownership."
            }]
          }
        }
        """)!.AsObject();

        public MaterializedRuleSourceSnapshot Load(string? text = null)
        {
            LastManifestBytes = Encoding.UTF8.GetBytes(text ?? Manifest.ToJsonString());
            File.WriteAllBytes(Path.Combine(Root, "manifest.json"), LastManifestBytes);
            return MaterializedRuleSourceLoader.Load(new(
                Root, "manifest.json",
                new() { Scheme = "content-tree-sha256", Value = "immutable-test-payload" },
                new() { ContractVersion = "1", ImplementationId = "vocabulary-tests", ImplementationVersion = "1" }));
        }

        public void Dispose() => Directory.Delete(Root, recursive: true);
    }
}
