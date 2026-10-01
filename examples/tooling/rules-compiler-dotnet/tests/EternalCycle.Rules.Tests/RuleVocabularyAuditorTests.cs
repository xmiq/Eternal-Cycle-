using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class RuleVocabularyAuditorTests(ITestOutputHelper output)
{
    [Fact]
    public void AbsentVocabularyHasAccurateZeroCoverageAndNoQualityErrors()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        manifest.Remove("retrievalVocabulary");
        payload.WriteManifest(manifest);
        var report = Audit(payload);
        Assert.True(report.IsValid);
        Assert.Equal(0, report.Summary.ConceptCount);
        Assert.Equal(0, report.Summary.EffectiveAssociationCount);
        Assert.Equal(0, report.Summary.CoveredSnippetCount);
        Assert.Equal(4, report.Summary.UncoveredSnippetCount);
        Assert.Equal(new(0, 0), report.Summary.AverageAssociationsPerCoveredSnippet);
        Assert.Equal(new(0, 0), report.Summary.AverageTargetSnippetsPerTerm);
        Assert.All(report.Snippets, snippet => Assert.Equal(0, snippet.AssociationCount));
        Assert.Single(report.Diagnostics, item => item.Code == "VOCABULARY_NO_COVERAGE");
        Assert.Empty(report.Policy.GenericTerms);
    }

    [Fact]
    public void FixtureReportsExactMetricsAndPreservesArtifactBaseline()
    {
        using var payload = new VocabularyTestPayload();
        var snapshot = payload.Load();
        var compilation = RuleCompilationPipeline.Compile(snapshot);
        var report = RuleVocabularyAuditor.Analyze(snapshot, compilation);
        Assert.True(report.IsValid);
        var summary = report.Summary;
        Assert.Equal((2, 4, 3, 3, 4, 2, 4), (summary.SourceCount, summary.SnippetCount, summary.ConceptCount,
            summary.CanonicalTermCount, summary.AliasCount, summary.PhraseCount, summary.BindingCount));
        Assert.Equal((15, 17, 4, 0, 3, 0), (summary.EffectiveAssociationCount, summary.RetainedOriginCount,
            summary.CoveredSnippetCount, summary.UncoveredSnippetCount, summary.ConceptsWithAliases, summary.ConceptsWithoutAliases));
        Assert.Equal((3, 0, 8, 2, 2), (summary.ConceptsWithTargets, summary.ConceptsWithoutTargets, summary.UniqueTermCount,
            summary.MultiOriginAssociationCount, summary.SourceOnlyAssociationCount));
        Assert.Equal(new(15, 4), summary.AverageAssociationsPerCoveredSnippet);
        Assert.Equal(new(14, 8), summary.AverageTargetSnippetsPerTerm);
        Assert.Equal(6, summary.MaximumAssociationsPerSnippet);
        Assert.Equal(3, summary.MaximumTargetSnippetsPerTerm);
        Assert.Equal((0, 0, 9), (summary.ErrorCount, summary.WarningCount, summary.InformationCount));
        Assert.Equal(3318, compilation.Bytes.Length);
        Assert.Equal("FF48ACB9A2616A948B9CF5504FB76CFFB7165D280690797AF39261DDFA3286E1", compilation.Artifact!.Integrity.ArtifactSha256);
        Assert.Equal("157AC2D47E8801A5CB9A90E3B8387F9D3B0A2EF14F3CEA46B0BACB4ECE9470D1", Hash(compilation.Bytes));
        output.WriteLine(JsonSerializer.Serialize(summary));
        output.WriteLine("Fixture findings: {0}", string.Join(", ", report.Diagnostics.Select(item => item.Code)));
    }

    [Fact]
    public void PartialCoverageDoesNotInferIntentOrGenerateTerms()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        manifest["retrievalVocabulary"]!["bindings"]!.AsArray().RemoveAt(0);
        payload.WriteManifest(manifest);
        var report = Audit(payload);
        Assert.Equal(3, report.Summary.CoveredSnippetCount);
        Assert.Equal(1, report.Summary.UncoveredSnippetCount);
        Assert.Equal(0, report.Snippets.Single(item => item.SnippetId == "core#combat").AssociationCount);
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_PARTIAL_COVERAGE" && item.Severity == RuleVocabularyAuditSeverity.Information);
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_UNUSED_CONCEPT" && item.SubjectId == "combat");
    }

    [Fact]
    public void DeclaredUnusedConceptWithoutAliasesIsVisibleButNotInvalid()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        var concept = manifest["retrievalVocabulary"]!["concepts"]![0]!.DeepClone();
        concept["conceptId"] = "unused";
        concept["canonicalTerm"] = "unused term";
        concept["alternatives"]!.AsArray().Clear();
        manifest["retrievalVocabulary"]!["concepts"]!.AsArray().Add(concept);
        payload.WriteManifest(manifest);
        var report = Audit(payload);
        Assert.True(report.IsValid);
        var unused = report.Concepts.Single(item => item.ConceptId == "unused");
        Assert.Equal(0, unused.BindingCount);
        Assert.Empty(unused.SnippetIds);
        Assert.Equal(1, report.Summary.ConceptsWithoutTargets);
        Assert.Equal(1, report.Summary.ConceptsWithoutAliases);
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_UNUSED_CONCEPT");
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_NO_ALIASES");
    }

    [Fact]
    public void ExactTermTopologyKeepsSharedConceptsKindsWeightsAndDifferentTargets()
    {
        using var payload = new VocabularyTestPayload();
        var report = Audit(payload);
        var shared = report.Terms.Single(term => term.Term == "progress");
        Assert.Equal(["persistence", "progression"], shared.Targets.Select(target => target.ConceptId));
        Assert.Equal([400, 600], shared.Targets.Select(target => target.Weight));
        Assert.Equal(["core#progression", "operations#save", "operations#status"], shared.SnippetIds);
        Assert.Equal(["core", "operations"], shared.SourceIds);
        Assert.True(shared.DifferentConceptTargetSets);
        Assert.Equal(new(3, 4), shared.CorpusCoverage);
        Assert.Equal(["alias"], shared.OriginKinds);
        Assert.Equal(["core#combat"], report.Terms.Single(term => term.Term == "fight").SnippetIds);
        Assert.Equal(["core#progression", "operations#save"], report.Terms.Single(term => term.Term == "experience points").SnippetIds);
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_AMBIGUOUS_TERM" && item.SubjectId == "progress");
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_DIFFERENT_TARGET_SETS" && item.SubjectId == "progress");
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_MULTI_SOURCE_TERM" && item.SubjectId == "progress");
        Assert.Equal(0, report.Summary.WarningCount);
    }

    [Fact]
    public void SourceInheritanceAndMeaningfulOverlappingOriginsRemainDistinct()
    {
        using var payload = new VocabularyTestPayload();
        var report = Audit(payload);
        var association = report.Associations.Single(item => item.SnippetId == "operations#save" && item.ConceptId == "persistence" && item.Term == "progress");
        Assert.Equal(2, association.Origins.Count);
        Assert.Equal([RuleVocabularyBindingScope.Snippet, RuleVocabularyBindingScope.Source], association.Origins.Select(item => item.Scope));
        Assert.Equal(2, report.Snippets.Single(item => item.SnippetId == "operations#status").SourceOnlyAssociationCount);
        Assert.Equal(0, report.Snippets.Single(item => item.SnippetId == "operations#save").SourceOnlyAssociationCount);
        Assert.Contains(report.Bindings, binding => binding.Identity.Scope == RuleVocabularyBindingScope.Snippet);
        Assert.Contains(report.Bindings, binding => binding.Identity.Scope == RuleVocabularyBindingScope.Source);
    }

    [Fact]
    public void BindingWithoutExclusiveAssociationsIsInformationNotDiscardedProvenance()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        var references = manifest["retrievalVocabulary"]!["bindings"]![3]!["conceptIds"]!.AsArray();
        references.Clear();
        references.Add("persistence");
        payload.WriteManifest(manifest);
        var report = Audit(payload);
        Assert.True(report.IsValid);
        var finding = Assert.Single(report.Diagnostics, item => item.Code == "VOCABULARY_BINDING_NO_UNIQUE_ASSOCIATIONS");
        Assert.Equal(RuleVocabularyAuditSeverity.Information, finding.Severity);
        Assert.Equal("operations/Snippet/save", finding.SubjectId);
        Assert.Equal(2, report.Summary.MultiOriginAssociationCount);
        Assert.Equal(13, report.Summary.RetainedOriginCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IllegalNormalizedAliasRedundancyReusesAErrors(bool canonical)
    {
        using var payload = new VocabularyTestPayload();
        var snapshot = payload.Load();
        var compilation = RuleCompilationPipeline.Compile(snapshot);
        snapshot.Manifest.RetrievalVocabulary!.Concepts[0].Alternatives[0] = new()
        {
            Term = canonical ? " COMBAT " : " FIGHTING ", Kind = RuleVocabularyAlternativeKind.Alias, Weight = 1, Rationale = "Duplicate derivative test input."
        };
        var report = RuleVocabularyAuditor.Analyze(snapshot, compilation);
        Assert.False(report.IsValid);
        var error = Assert.Single(report.Diagnostics);
        Assert.Equal("VOCABULARY_TERM_DUPLICATE", error.Code);
        Assert.Equal(RuleVocabularyAuditSeverity.Error, error.Severity);
        Assert.Equal("compilation", error.SubjectId);
        Assert.Empty(report.Associations);
    }

    [Theory]
    [InlineData(3, 75, true)]
    [InlineData(3, 76, false)]
    [InlineData(4, 75, false)]
    [InlineData(2, 50, true)]
    public void BreadthRequiresBothThresholdsInclusively(int minimum, int percent, bool expected)
    {
        using var payload = new VocabularyTestPayload();
        var report = Audit(payload, new() { MinimumBroadTargets = minimum, MinimumBroadCoveragePercent = percent });
        Assert.Equal(expected, report.Diagnostics.Any(item => item.Code == "VOCABULARY_BROAD_TERM" && item.SubjectId == "progress"));
        Assert.Equal(0, report.Summary.ErrorCount);
    }

    [Fact]
    public void ExplicitPolicyExposesFanOutAndGenericOnlyConceptWithoutChangingWeights()
    {
        using var payload = new VocabularyTestPayload();
        var report = Audit(payload, new()
        {
            MinimumBroadTargets = 2, MinimumBroadCoveragePercent = 50,
            GenericTerms = [" COMBAT ", "fight", "fighting", " PROGRESS ", "progress"]
        });
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_GENERIC_TERM" && item.SubjectId == "progress");
        Assert.Single(report.Diagnostics, item => item.Code == "VOCABULARY_GENERIC_TERM" && item.SubjectId == "progress");
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_ONLY_GENERIC_TERMS" && item.SubjectId == "combat");
        Assert.Contains(report.Diagnostics, item => item.Code == "VOCABULARY_BINDING_FAN_OUT" && item.SubjectId == "operations/Source/");
        Assert.Equal(["combat", "fight", "fighting", "progress"], report.Policy.GenericTerms);
        Assert.Equal(800, report.Associations.Single(item => item.Term == "combat").Weight);
    }

    [Fact]
    public void TokenBoundarySignalIsExplicitAndDoesNotJudgeProse()
    {
        using var payload = new VocabularyTestPayload();
        var snapshot = payload.Load();
        var compilation = RuleCompilationPipeline.Compile(snapshot);
        var tokens = compilation.Artifact!.Snippets.Single(item => item.SnippetId == "core#combat").EstimatedTokens;
        var atBoundary = RuleVocabularyAuditor.Analyze(snapshot, compilation, new() { LargeSnippetTokens = tokens });
        var above = RuleVocabularyAuditor.Analyze(snapshot, compilation, new() { LargeSnippetTokens = tokens + 1 });
        Assert.Contains(atBoundary.Diagnostics, item => item.Code == "VOCABULARY_LARGE_SNIPPET" && item.SubjectId == "core#combat");
        Assert.DoesNotContain(above.Diagnostics, item => item.Code == "VOCABULARY_LARGE_SNIPPET" && item.SubjectId == "core#combat");
    }

    [Theory]
    [InlineData("minimum")]
    [InlineData("percent-zero")]
    [InlineData("percent-high")]
    [InlineData("tokens")]
    [InlineData("generic-count")]
    public void InvalidAuditPolicyIsRejected(string defect)
    {
        var policy = defect switch
        {
            "minimum" => new RuleVocabularyAuditPolicy { MinimumBroadTargets = 0 },
            "percent-zero" => new() { MinimumBroadCoveragePercent = 0 },
            "percent-high" => new() { MinimumBroadCoveragePercent = 101 },
            "tokens" => new() { LargeSnippetTokens = 0 },
            _ => new() { GenericTerms = Enumerable.Repeat("term", 257).ToArray() }
        };
        Assert.Throws<ArgumentException>(() => policy.Normalize());
    }

    [Theory]
    [InlineData("origin")]
    [InlineData("weight")]
    [InlineData("candidate")]
    public void TamperedEvidenceCannotMasqueradeAsValidQualityState(string defect)
    {
        using var payload = new VocabularyTestPayload();
        var snapshot = payload.Load();
        var result = RuleCompilationPipeline.Compile(snapshot);
        var snippets = result.Vocabulary.Snippets.ToArray();
        var first = snippets[0];
        var associations = first.Associations.ToArray();
        if (defect == "origin") { associations[0] = associations[0] with { Origins = [] }; }
        if (defect == "weight") { associations[0] = associations[0] with { RetrievalTerm = new() { Term = associations[0].RetrievalTerm.Term, Kind = associations[0].RetrievalTerm.Kind, Weight = 1 } }; }
        snippets[0] = first with { Associations = associations, Candidate = defect == "candidate" ? first.Candidate with { Content = "Not the compiled content." } : first.Candidate };
        var report = RuleVocabularyAuditor.Analyze(snapshot, result with { Vocabulary = result.Vocabulary with { Snippets = snippets } });
        Assert.False(report.IsValid);
        Assert.Equal("VOCABULARY_AUDIT_EVIDENCE_MISMATCH", Assert.Single(report.Diagnostics).Code);
    }

    [Fact]
    public void MismatchedSnapshotAndFailedCompilationHaveStableErrors()
    {
        using var payload = new VocabularyTestPayload();
        var snapshot = payload.Load();
        var result = RuleCompilationPipeline.Compile(snapshot);
        var mismatch = RuleVocabularyAuditor.Analyze(snapshot with { ManifestSha256 = new string('A', 64) }, result);
        Assert.Equal("VOCABULARY_AUDIT_PROVENANCE_MISMATCH", Assert.Single(mismatch.Diagnostics).Code);
        var failed = RuleVocabularyAuditor.Analyze(snapshot, result with { Artifact = null });
        Assert.Equal("VOCABULARY_AUDIT_COMPILATION_INVALID", Assert.Single(failed.Diagnostics).Code);
    }

    [Fact]
    public void RepeatsRelocationCultureAndSetOrderProduceIdenticalModelsAndBytes()
    {
        using var first = new VocabularyTestPayload();
        using var second = new VocabularyTestPayload();
        var snapshot = first.Load();
        var result = RuleCompilationPipeline.Compile(snapshot);
        var policy = new RuleVocabularyAuditPolicy { GenericTerms = ["progress", "save"] };
        var report = RuleVocabularyAuditor.Analyze(snapshot, result, policy);
        Assert.Equivalent(report, RuleVocabularyAuditor.Analyze(snapshot, result, policy));
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var concepts = snapshot.Manifest.RetrievalVocabulary!.Concepts;
            (concepts[0], concepts[2]) = (concepts[2], concepts[0]);
            var reordered = RuleVocabularyAuditor.Analyze(snapshot, result, policy with { GenericTerms = [" SAVE ", "progress", "progress"] });
            Assert.Equal(RuleVocabularyAuditWriter.Write(report).ToArray(), RuleVocabularyAuditWriter.Write(reordered).ToArray());
            Assert.Equal(RuleVocabularyAuditWriter.Write(report).ToArray(), RuleVocabularyAuditWriter.Write(Audit(second, policy)).ToArray());
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Fact]
    public void WarningAuditIsObservationalAndRequiresNoFiles()
    {
        using var payload = new VocabularyTestPayload();
        var snapshot = payload.Load();
        var result = RuleCompilationPipeline.Compile(snapshot);
        var artifactBefore = CompiledRulesArtifactWriter.Write(result.Artifact!).Bytes.ToArray();
        var definitionsBefore = JsonSerializer.Serialize(snapshot.Manifest);
        var evidenceBefore = JsonSerializer.Serialize(result.Vocabulary);
        foreach (var file in Directory.EnumerateFiles(payload.Root)) { File.Delete(file); }
        var report = RuleVocabularyAuditor.Analyze(snapshot, result, new() { MinimumBroadTargets = 1, MinimumBroadCoveragePercent = 1, GenericTerms = ["progress"] });
        Assert.True(report.Summary.WarningCount > 0);
        Assert.True(report.IsValid);
        Assert.Equal(artifactBefore, CompiledRulesArtifactWriter.Write(result.Artifact!).Bytes.ToArray());
        Assert.Equal(artifactBefore, result.Bytes.ToArray());
        Assert.Equal(definitionsBefore, JsonSerializer.Serialize(snapshot.Manifest));
        Assert.Equal(evidenceBefore, JsonSerializer.Serialize(result.Vocabulary));
    }

    [Fact]
    public void SerializationHasStablePropertiesFindingOrderAndNoEnvironmentData()
    {
        using var payload = new VocabularyTestPayload();
        var report = Audit(payload);
        var bytes = RuleVocabularyAuditWriter.Write(report).ToArray();
        Assert.Equal(bytes, RuleVocabularyAuditWriter.Write(report).ToArray());
        Assert.False(bytes.AsSpan().StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }));
        Assert.Equal((byte)'}', bytes[^1]);
        var text = Encoding.UTF8.GetString(bytes);
        Assert.StartsWith("{\"auditFormatVersion\":1,\"artifactSha256\":", text);
        Assert.DoesNotContain(payload.Root, text);
        Assert.DoesNotContain("timestamp", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("generatedAt", text);
        Assert.DoesNotContain("score", text, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(report.Diagnostics.OrderBy(item => item.Severity).ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.SubjectKind, StringComparer.Ordinal).ThenBy(item => item.SubjectId, StringComparer.Ordinal), report.Diagnostics);
        Assert.All(report.Diagnostics, item => { Assert.DoesNotContain("[", item.SubjectId); Assert.True(item.Message.Length < 256); });
        Assert.Equal(report.Terms.OrderBy(item => item.Term, StringComparer.Ordinal), report.Terms);
    }

    [Fact]
    public void HistoricalCanonicalCorpusAuditDoesNotCurateOrChangeTheArtifact()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "docs", "rules", "rule-source-manifest.json"))) { root = root.Parent; }
        Assert.NotNull(root);
        var snapshot = CanonicalVocabularyPayload.LoadHistorical(root.FullName);
        var before = RuleCompilationPipeline.Compile(snapshot);
        var report = RuleVocabularyAuditor.Analyze(snapshot, before);
        var after = RuleCompilationPipeline.Compile(snapshot);
        Assert.True(report.IsValid);
        Assert.Equal((10, 154, 0, 0, 0, 0, 0, 0, 0, 154, 0, 0),
            (report.Summary.SourceCount, report.Summary.SnippetCount, report.Summary.ConceptCount, report.Summary.AliasCount,
                report.Summary.PhraseCount, report.Summary.BindingCount, report.Summary.EffectiveAssociationCount, report.Summary.RetainedOriginCount,
                report.Summary.CoveredSnippetCount, report.Summary.UncoveredSnippetCount, report.Summary.WarningCount, report.Summary.ErrorCount));
        Assert.Equal(before.Bytes.ToArray(), after.Bytes.ToArray());
        Assert.Equal(223929, after.Bytes.Length);
        Assert.Equal("4B583A7904CE514B56CF3B497648DB5D778FC04DF2285D30A2E03823A32EA6A4", after.Artifact!.Integrity.ArtifactSha256);
        Assert.Equal("802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC", Hash(after.Bytes));
        output.WriteLine("Canonical audit: {0}", JsonSerializer.Serialize(report.Summary));
        output.WriteLine("Canonical findings: {0}", string.Join(", ", report.Diagnostics.Select(item => item.Code + ":" + item.SubjectId)));
    }

    private static RuleVocabularyAuditReport Audit(VocabularyTestPayload payload, RuleVocabularyAuditPolicy? policy = null)
    {
        var snapshot = payload.Load();
        return RuleVocabularyAuditor.Analyze(snapshot, RuleCompilationPipeline.Compile(snapshot), policy);
    }
    private static string Hash(ReadOnlyMemory<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes.Span));
}
