using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class RuleVocabularyAcceptanceTests(ITestOutputHelper output)
{
    [Theory]
    [InlineData("canonical-term", true)]
    [InlineData("alias-addition", true)]
    [InlineData("phrase-removal", true)]
    [InlineData("weight", true)]
    [InlineData("binding-target", true)]
    [InlineData("rationale", false)]
    [InlineData("manifest-formatting", false)]
    public void CanonicalMutationRetainsSourceSemanticsAndChangesExactProvenance(string mutation, bool metadataChanges)
    {
        using var payload = new CanonicalVocabularyPayload();
        var before = payload.Load();
        var original = RuleCompilationPipeline.Compile(before);
        var originalReport = AuditBytes(before, original);
        var path = Path.Combine(payload.Root, CanonicalVocabularyPayload.ManifestPath);
        var manifest = JsonNode.Parse(before.AuthoritativeManifestBytes.Span)!.AsObject();
        var definition = manifest["retrievalVocabulary"]!;
        var concept = definition["concepts"]!.AsArray().Single(item => item!["conceptId"]!.GetValue<string>() == "combat-context")!;
        switch (mutation)
        {
            case "canonical-term": concept["canonicalTerm"] = "combat capability"; break;
            case "alias-addition":
                concept["alternatives"]!.AsArray().Add(new JsonObject
                {
                    ["term"] = "brawling", ["kind"] = "Alias", ["weight"] = 700,
                    ["rationale"] = "Acceptance fixture alternative for contextual combat capability."
                });
                break;
            case "phrase-removal":
                var alternatives = concept["alternatives"]!.AsArray();
                alternatives.Remove(alternatives.Single(item => item!["kind"]!.GetValue<string>() == "Phrase"));
                break;
            case "weight": concept["weight"] = 901; break;
            case "binding-target":
                // Move one existing precise binding to an unbound, real anchor. This is
                // a mutation fixture, not an assertion that the new topology is curated.
                definition["bindings"]!.AsArray().Single(item => item!["ruleSourceId"]!.GetValue<string>() == "core-development" &&
                    item["sourceAnchor"]?.GetValue<string>() == "power-preparation-and-victory")!["sourceAnchor"] = "scope-boundaries";
                break;
            case "rationale": concept["rationale"] = "Revised review evidence without changing retrieval metadata."; break;
        }
        File.WriteAllText(path, manifest.ToJsonString(new() { WriteIndented = mutation == "manifest-formatting" }), new UTF8Encoding(false));
        var after = payload.Load();
        var changed = RuleCompilationPipeline.Compile(after);
        Assert.True(original.IsValid);
        Assert.True(changed.IsValid);
        Assert.Empty(CompiledRulesArtifactContract.Validate(changed.Artifact!));
        Assert.NotEqual(before.ManifestSha256, after.ManifestSha256);
        Assert.Equal(SourceAndContent(original), SourceAndContent(changed));
        Assert.Equal(JsonSerializer.Serialize(original.Artifact!.Ruleset.Source), JsonSerializer.Serialize(changed.Artifact!.Ruleset.Source));
        Assert.Equal(metadataChanges, Retrieval(original) != Retrieval(changed));
        Assert.NotEqual(original.Artifact.Integrity.ArtifactSha256, changed.Artifact.Integrity.ArtifactSha256);
        Assert.NotEqual(Hash(original.Bytes), Hash(changed.Bytes));
        Assert.NotEqual(Hash(originalReport), Hash(AuditBytes(after, changed)));
        Assert.Equal(changed.Bytes.ToArray(), RuleCompilationPipeline.Compile(after).Bytes.ToArray());
        Assert.Equal(AuditBytes(after, changed).ToArray(), AuditBytes(after, changed).ToArray());

        if (mutation == "rationale")
        {
            Assert.NotEqual(JsonSerializer.Serialize(original.Vocabulary.Snippets), JsonSerializer.Serialize(changed.Vocabulary.Snippets));
            Assert.All(changed.Vocabulary.Snippets.SelectMany(item => item.Associations).Where(item => item.ConceptId == "combat-context"),
                item => Assert.All(item.Origins, origin => Assert.Equal(concept["rationale"]!.GetValue<string>(), origin.ConceptRationale)));
        }
        if (mutation == "manifest-formatting")
        {
            Assert.Equal(JsonSerializer.Serialize(original.Vocabulary.Snippets), JsonSerializer.Serialize(changed.Vocabulary.Snippets));
        }
    }

    [Theory]
    [InlineData("concept", "VOCABULARY_CONCEPT_MISSING")]
    [InlineData("target", "VOCABULARY_TARGET_MISSING")]
    public void InvalidCanonicalMutationFailsBeforeAnyArtifactIsProduced(string mutation, string code)
    {
        using var payload = new CanonicalVocabularyPayload();
        var path = Path.Combine(payload.Root, CanonicalVocabularyPayload.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllBytes(path))!;
        var binding = manifest["retrievalVocabulary"]!["bindings"]![0]!;
        if (mutation == "concept") { binding["conceptIds"]![0] = "not-reviewed"; }
        else { binding["sourceAnchor"] = "not-a-real-anchor"; }
        File.WriteAllText(path, manifest.ToJsonString());
        if (mutation == "concept")
        {
            Assert.Equal(code, Assert.Throws<MaterializedRuleSourceException>(() => payload.Load()).Code);
        }
        else
        {
            var snapshot = payload.Load();
            Assert.Equal(code, Assert.Throws<RuleVocabularyEnrichmentException>(() => RuleCompilationPipeline.Compile(snapshot)).Code);
        }
    }

    [Fact]
    public void AmbiguousCandidateMutationIsRejectedAtExactBindingBoundary()
    {
        using var payload = new CanonicalVocabularyPayload();
        var snapshot = payload.Load();
        var candidates = RuleSnippetCompiler.Compile(snapshot).ToList();
        candidates.Add(candidates.Single(item => item.SnippetId == "core-development#power-preparation-and-victory"));
        var failure = Assert.Throws<RuleVocabularyEnrichmentException>(() => RuleVocabularyEnricher.Enrich(snapshot, candidates));
        Assert.Equal("VOCABULARY_TARGET_AMBIGUOUS", failure.Code);
        Assert.True(failure.Message.Length < 256);
    }

    [Fact]
    public void CanonicalSetConstructionOrderCannotChangeArtifactEvidenceOrAudit()
    {
        using var payload = new CanonicalVocabularyPayload();
        var snapshot = payload.Load();
        var first = RuleCompilationPipeline.Compile(snapshot);
        var report = AuditBytes(snapshot, first);
        var vocabulary = snapshot.Manifest.RetrievalVocabulary!;
        Reverse(vocabulary.Concepts);
        Reverse(vocabulary.Bindings);
        foreach (var concept in vocabulary.Concepts) { Reverse(concept.Alternatives); }
        foreach (var binding in vocabulary.Bindings) { Reverse(binding.ConceptIds); }
        var second = RuleCompilationPipeline.Compile(snapshot);
        Assert.Equal(first.Bytes.ToArray(), second.Bytes.ToArray());
        Assert.Equal(JsonSerializer.Serialize(first.Vocabulary), JsonSerializer.Serialize(second.Vocabulary));
        Assert.Equal(report.ToArray(), AuditBytes(snapshot, second).ToArray());
        // Authoritative manifest bytes did not change: this tests internal set order,
        // not reauthoring JSON, whose exact hash intentionally participates in identity.
        Assert.Equal(first.Artifact!.Ruleset.ManifestSha256, second.Artifact!.Ruleset.ManifestSha256);
    }

    [Fact]
    public void CanonicalLfCrLfVariantsPreserveContentButNotExactSourceProvenance()
    {
        using var lf = new CanonicalVocabularyPayload();
        using var crlf = new CanonicalVocabularyPayload();
        foreach (var source in lf.Load().Sources)
        {
            var text = source.DecodedText.ReplaceLineEndings("\n");
            File.WriteAllText(Path.Combine(lf.Root, source.ManifestEntry.Path), text, new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(crlf.Root, source.ManifestEntry.Path), text.ReplaceLineEndings("\r\n"), new UTF8Encoding(false));
        }
        // Load the altered ordinary files directly; re-preparing the frozen checkpoint
        // would undo the newline mutation and would not test the compiler's provenance.
        var lfSnapshot = lf.Load();
        var crlfSnapshot = crlf.Load();
        var a = RuleCompilationPipeline.Compile(lfSnapshot);
        var b = RuleCompilationPipeline.Compile(crlfSnapshot);
        Assert.True(a.IsValid && b.IsValid);
        Assert.Equal(lfSnapshot.ManifestSha256, crlfSnapshot.ManifestSha256);
        Assert.Equal(JsonSerializer.Serialize(lfSnapshot.Manifest), JsonSerializer.Serialize(crlfSnapshot.Manifest));
        Assert.All(lfSnapshot.Sources, source => Assert.NotEqual(source.SourceSha256,
            crlfSnapshot.Sources.Single(item => item.ManifestEntry.RuleSourceId == source.ManifestEntry.RuleSourceId).SourceSha256));
        Assert.Equal(ContentAndApplicability(a), ContentAndApplicability(b));
        Assert.Equal(Retrieval(a), Retrieval(b));
        Assert.NotEqual(a.Artifact!.Integrity.ArtifactSha256, b.Artifact!.Integrity.ArtifactSha256);
        Assert.NotEqual(Hash(a.Bytes), Hash(b.Bytes));
        foreach (var snapshot in new[] { lfSnapshot, crlfSnapshot })
        {
            var first = RuleCompilationPipeline.Compile(snapshot);
            var repeat = RuleCompilationPipeline.Compile(snapshot);
            Assert.Equal(first.Bytes.ToArray(), repeat.Bytes.ToArray());
            Assert.Equal(AuditBytes(snapshot, first).ToArray(), AuditBytes(snapshot, repeat).ToArray());
            output.WriteLine("{0}: semantic={1} bytes={2} audit={3}", snapshot == lfSnapshot ? "LF" : "CRLF",
                first.Artifact!.Integrity.ArtifactSha256, Hash(first.Bytes), Hash(AuditBytes(snapshot, first)));
        }
    }

    [Fact]
    public void CanonicalAuditIsObservationalAndIsolatedMaterializationNeedsOnlyOrdinaryFiles()
    {
        using var payload = new CanonicalVocabularyPayload();
        Assert.False(Directory.Exists(Path.Combine(payload.Root, ".git")));
        Assert.Equal(11, Directory.EnumerateFiles(payload.Root, "*", SearchOption.AllDirectories).Count());
        var snapshot = payload.Load();
        var first = RuleCompilationPipeline.Compile(snapshot);
        var definitions = JsonSerializer.Serialize(snapshot.Manifest);
        var evidence = JsonSerializer.Serialize(first.Vocabulary);
        var artifact = JsonSerializer.Serialize(first.Artifact);
        var report = RuleVocabularyAuditor.Analyze(snapshot, first, CanonicalVocabularyPayload.AuditPolicy());
        var bytes = RuleVocabularyAuditWriter.Write(report);
        Assert.Equal(435079, bytes.Length);
        var repeat = RuleCompilationPipeline.Compile(snapshot);
        Assert.True(report.IsValid);
        Assert.Equal(first.Bytes.ToArray(), repeat.Bytes.ToArray());
        Assert.Equal(definitions, JsonSerializer.Serialize(snapshot.Manifest));
        Assert.Equal(evidence, JsonSerializer.Serialize(first.Vocabulary));
        Assert.Equal(artifact, JsonSerializer.Serialize(first.Artifact));
        Assert.Equal(bytes.ToArray(), RuleVocabularyAuditWriter.Write(report).ToArray());
        Assert.False(bytes.Span.StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }));
        Assert.Equal((byte)'}', bytes.Span[^1]);
        var text = Encoding.UTF8.GetString(bytes.Span);
        Assert.StartsWith("{\"auditFormatVersion\":1,\"artifactSha256\":", text);
        Assert.DoesNotContain(payload.Root, text);
        Assert.DoesNotContain("generatedAt", text);
        Assert.DoesNotContain("timestamp", text, StringComparison.OrdinalIgnoreCase);
        output.WriteLine("Canonical artifact: {0} bytes, semantic={1}, byteSHA256={2}", first.Bytes.Length,
            first.Artifact!.Integrity.ArtifactSha256, Hash(first.Bytes));
        output.WriteLine("Canonical audit: {0} bytes, SHA256={1}", bytes.Length, Hash(bytes));
    }

    [Theory]
    [InlineData("combat-context", "combat", "canonical", "core-development#power-preparation-and-victory", 900)]
    [InlineData("combat-context", "fighting", "alias", "core-development#power-preparation-and-victory", 700)]
    [InlineData("character-development", "character advancement", "phrase", "core-development#gaining-development", 800)]
    [InlineData("host-bootstrap", "host instructions", "phrase", "gm-host-bootstrap", 800)]
    public void CanonicalArtifactTermTracesToReviewedDefinitionAndExactOrigin(string conceptId, string term, string kind, string snippetId, int weight)
    {
        using var payload = new CanonicalVocabularyPayload();
        var snapshot = payload.Load();
        var result = RuleCompilationPipeline.Compile(snapshot);
        var snippet = result.Vocabulary.Snippets.Single(item => item.Candidate.SnippetId == snippetId);
        var association = Assert.Single(snippet.Associations, item => item.ConceptId == conceptId && item.RetrievalTerm.Term == term);
        Assert.Equal(kind, association.RetrievalTerm.Kind);
        Assert.Equal(weight, association.RetrievalTerm.Weight);
        var emitted = Assert.Single(result.Artifact!.Snippets.Single(item => item.SnippetId == snippetId).Retrieval.Terms,
            item => item.Kind == conceptId && item.Term == term);
        Assert.Equal(weight, emitted.Weight);
        var concept = snapshot.Manifest.RetrievalVocabulary!.Concepts.Single(item => item.ConceptId == conceptId);
        var origin = Assert.Single(association.Origins);
        var binding = snapshot.Manifest.RetrievalVocabulary.Bindings.Single(item => item.RuleSourceId == origin.Binding.RuleSourceId &&
            item.Scope == origin.Binding.Scope && item.SourceAnchor == origin.Binding.SourceAnchor);
        Assert.Equal(concept.Rationale, origin.ConceptRationale);
        Assert.Equal(binding.Rationale, origin.BindingRationale);
        Assert.Equal(kind == "canonical" ? concept.Rationale : concept.Alternatives.Single(item => item.Term == term).Rationale, origin.TermRationale);
        Assert.Equal(snippet.Candidate.RuleSourceId, binding.RuleSourceId);
        Assert.Equal(conceptId == "host-bootstrap" ? RuleVocabularyBindingScope.Source : RuleVocabularyBindingScope.Snippet, binding.Scope);
    }

    private static ReadOnlyMemory<byte> AuditBytes(MaterializedRuleSourceSnapshot snapshot, RuleCompilationResult result)
    {
        var report = RuleVocabularyAuditor.Analyze(snapshot, result, CanonicalVocabularyPayload.AuditPolicy());
        Assert.True(report.IsValid);
        return RuleVocabularyAuditWriter.Write(report);
    }

    private static string SourceAndContent(RuleCompilationResult result) =>
        JsonSerializer.Serialize(result.Artifact!.RuleSources) + ContentAndApplicability(result);

    private static string ContentAndApplicability(RuleCompilationResult result) => JsonSerializer.Serialize(
        result.Artifact!.Snippets.Select(item => new
        {
            item.SnippetId, item.RuleSourceId, item.SourceAnchor, item.Content, item.ContentSha256, item.EstimatedTokens
        }));

    private static string Retrieval(RuleCompilationResult result) => JsonSerializer.Serialize(result.Artifact!.Snippets.Select(item => item.Retrieval));
    private static string Hash(ReadOnlyMemory<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes.Span));
    private static void Reverse<T>(IList<T> items)
    {
        for (var i = 0; i < items.Count / 2; i++) { (items[i], items[items.Count - i - 1]) = (items[items.Count - i - 1], items[i]); }
    }
}
