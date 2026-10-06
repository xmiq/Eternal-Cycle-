using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class CanonicalVocabularyTests
{
    // One immutable in-memory checkpoint avoids copying the ten-source corpus for each
    // topology case. Relocation tests deliberately load fresh ordinary-file payloads.
    private static readonly MaterializedRuleSourceSnapshot Snapshot = CanonicalVocabularyPayload.LoadCurated(CanonicalVocabularyPayload.FindRepositoryRoot());
    private static readonly RuleCompilationResult Compilation = RuleCompilationPipeline.Compile(Snapshot);
    private static readonly RuleVocabularyAuditReport Report = RuleVocabularyAuditor.Analyze(Snapshot, Compilation, CanonicalVocabularyPayload.AuditPolicy());
    private static readonly CanonicalQualityFixture Quality = CanonicalVocabularyPayload.Fixture<CanonicalQualityFixture>("quality-expectations.json");
    private static readonly CanonicalAfterE Expected = CanonicalVocabularyPayload.Fixture<CanonicalAfterE>("after-e.json");

    public static IEnumerable<object[]> Cases() => Quality.Expectations.Select(item => new object[] { item.Id });

    [Theory]
    [MemberData(nameof(Cases))]
    public void ReviewedPositiveAndNegativeTopology(string id)
    {
        var item = Quality.Expectations.Single(item => item.Id == id);
        Assert.NotEmpty(item.Rationale);
        Assert.Equal(item.Term, RuleRetrievalVocabulary.NormalizeTerm(item.Term));
        foreach (var snippetId in item.PresentOn)
        {
            var snippet = Assert.Single(Compilation.Artifact!.Snippets, snippet => snippet.SnippetId == snippetId);
            Assert.Contains(snippet.Retrieval.Terms, term => term.Kind == item.ConceptId && term.Term == item.Term);
        }
        foreach (var snippetId in item.AbsentFrom)
        {
            var snippet = Assert.Single(Compilation.Artifact!.Snippets, snippet => snippet.SnippetId == snippetId);
            Assert.DoesNotContain(snippet.Retrieval.Terms, term => term.Kind == item.ConceptId && term.Term == item.Term);
        }
    }

    [Fact]
    public void FixtureBreadthCoversEveryConceptWithoutRanking()
    {
        Assert.Equal(65, Quality.Expectations.Count);
        Assert.Equal(115, Quality.Expectations.Sum(item => item.PresentOn.Count));
        Assert.Equal(130, Quality.Expectations.Sum(item => item.AbsentFrom.Count));
        Assert.Equal(Quality.Expectations.Count, Quality.Expectations.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(Snapshot.Manifest.RetrievalVocabulary!.Concepts.Select(item => item.ConceptId).Order(StringComparer.Ordinal),
            Quality.Expectations.Select(item => item.ConceptId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        Assert.All(Quality.Expectations, item => { Assert.NotEmpty(item.PresentOn); Assert.NotEmpty(item.AbsentFrom); });
    }

    [Fact]
    public void CanonicalManifestAndArtifactValidateWithExactReviewedMetrics()
    {
        Assert.True(Compilation.IsValid);
        Assert.Empty(Compilation.Errors);
        Assert.Empty(CompiledRulesArtifactContract.Validate(Compilation.Artifact!));
        Assert.True(CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(Compilation.Bytes.Span)).IsValid);
        Assert.True(Report.IsValid);
        Assert.Equal(JsonSerializer.Serialize(Expected.Summary), JsonSerializer.Serialize(Report.Summary));
        Assert.Equal(Expected.ArtifactBytes, Compilation.Bytes.Length);
        Assert.Equal(Expected.SemanticDigest, Compilation.Artifact!.Integrity.ArtifactSha256);
        Assert.Equal(Expected.SerializedSha256, Hash(Compilation.Bytes));
        Assert.Equal(Expected.AuditSha256, Hash(RuleVocabularyAuditWriter.Write(Report)));
        Assert.Equal(Expected.UncoveredSnippetIds, Report.Snippets.Where(item => item.AssociationCount == 0).Select(item => item.SnippetId));
        Assert.Equal(Expected.FindingCounts.OrderBy(item => item.Code, StringComparer.Ordinal),
            Report.Diagnostics.GroupBy(item => item.Code).Select(group => new CanonicalFindingCount(group.Key, group.Count())).OrderBy(item => item.Code, StringComparer.Ordinal));
    }

    [Fact]
    public void EveryAssociationHasReviewedOriginWeightAndRationaleOnly()
    {
        var vocabulary = Snapshot.Manifest.RetrievalVocabulary!;
        Assert.Equal(62, vocabulary.Concepts.Select(item => item.ConceptId).Distinct(StringComparer.Ordinal).Count());
        foreach (var snippet in Compilation.Vocabulary.Snippets)
        {
            foreach (var association in snippet.Associations)
            {
                var concept = vocabulary.Concepts.Single(item => item.ConceptId == association.ConceptId);
                Assert.Equal(concept.CanonicalTerm, association.CanonicalTerm);
                var alternative = concept.Alternatives.SingleOrDefault(item => item.Term == association.RetrievalTerm.Term);
                Assert.True(association.RetrievalTerm.Term == concept.CanonicalTerm || alternative is not null);
                Assert.Equal(alternative?.Weight ?? concept.Weight, association.RetrievalTerm.Weight);
                Assert.Equal(association.RetrievalTerm.Term, RuleRetrievalVocabulary.NormalizeTerm(association.RetrievalTerm.Term));
                Assert.NotEmpty(association.Origins);
                foreach (var origin in association.Origins)
                {
                    var binding = vocabulary.Bindings.Single(item => item.RuleSourceId == origin.Binding.RuleSourceId &&
                        item.Scope == origin.Binding.Scope && item.SourceAnchor == origin.Binding.SourceAnchor);
                    Assert.Contains(concept.ConceptId, binding.ConceptIds);
                    Assert.Equal(binding.Rationale, origin.BindingRationale);
                    Assert.Equal(concept.Rationale, origin.ConceptRationale);
                    Assert.Equal(alternative?.Rationale ?? concept.Rationale, origin.TermRationale);
                    Assert.Equal(snippet.Candidate.RuleSourceId, binding.RuleSourceId);
                    if (binding.Scope == RuleVocabularyBindingScope.Snippet) { Assert.Equal(snippet.Candidate.SourceAnchor, binding.SourceAnchor); }
                }
            }
        }
        Assert.All(vocabulary.Concepts, concept =>
        {
            Assert.Equal(900, concept.Weight);
            Assert.All(concept.Alternatives, alternative => Assert.Equal(
                alternative.Term is "conflict" or "preparation" or "save" ? 600 : alternative.Kind == RuleVocabularyAlternativeKind.Phrase ? 800 : 700,
                alternative.Weight));
        });
    }

    [Fact]
    public void SourceAndSnippetSemanticsRemainExactlyHistoricalWhileRetrievalChanges()
    {
        var historical = CanonicalVocabularyPayload.LoadHistorical(CanonicalVocabularyPayload.FindRepositoryRoot());
        var before = RuleCompilationPipeline.Compile(historical);
        Assert.True(before.IsValid);
        Assert.Equal(223929, before.Bytes.Length);
        Assert.Equal("802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC", Hash(before.Bytes));
        Assert.Equal("4B583A7904CE514B56CF3B497648DB5D778FC04DF2285D30A2E03823A32EA6A4", before.Artifact!.Integrity.ArtifactSha256);
        Assert.NotEqual(before.Artifact.Integrity.ArtifactSha256, Compilation.Artifact!.Integrity.ArtifactSha256);
        Assert.NotEqual(Hash(before.Bytes), Hash(Compilation.Bytes));
        Assert.NotEqual(before.Artifact.Ruleset.ManifestSha256, Compilation.Artifact.Ruleset.ManifestSha256);
        Assert.Equal(JsonSerializer.Serialize(before.Artifact.RuleSources), JsonSerializer.Serialize(Compilation.Artifact.RuleSources));
        Assert.Equal(WithoutRetrieval(before.Artifact), WithoutRetrieval(Compilation.Artifact));
        Assert.Equal(CanonicalVocabularyPayload.Baseline().SourceHashes,
            Snapshot.Sources.Select(source => new CanonicalSourceHash(source.ManifestEntry.RuleSourceId, source.SourceSha256)));
    }

    [Fact]
    public void AllWarningsHaveExplicitReviewAndInformationTopologyIsDeliberate()
    {
        Assert.Equal(Quality.AcceptedWarnings.Select(item => (item.Code, item.SubjectId)).Order(),
            Report.Diagnostics.Where(item => item.Severity == RuleVocabularyAuditSeverity.Warning).Select(item => (item.Code, item.SubjectId)).Order());
        Assert.All(Quality.AcceptedWarnings, item => Assert.NotEmpty(item.Rationale));
        Assert.DoesNotContain(Report.Diagnostics, item => item.Code is "VOCABULARY_BROAD_TERM" or "VOCABULARY_BINDING_FAN_OUT");
        var conflict = Report.Terms.Single(item => item.Term == "conflict");
        Assert.True(conflict.DifferentConceptTargetSets);
        Assert.Equal(9, conflict.SnippetIds.Count);
        Assert.Equal(2, conflict.Targets.Count);
        Assert.Equal(3, Report.Terms.Single(item => item.Term == "preparation").SnippetIds.Count);
        Assert.Equal(["core-context-assembly#manual-persistence-commands"], Report.Terms.Single(item => item.Term == "save").SnippetIds);
        var sourceBinding = Assert.Single(Report.Bindings, item => item.Identity.Scope == RuleVocabularyBindingScope.Source);
        Assert.Equal("gm-host-bootstrap", sourceBinding.Identity.RuleSourceId);
        Assert.Equal(["gm-host-bootstrap"], sourceBinding.SnippetIds);
    }

    [Fact]
    public void RepeatedAndRelocatedCompilationHasIdenticalArtifactAndAuditBytes()
    {
        using var relocated = new CanonicalVocabularyPayload();
        Compare(Snapshot);
        Compare(relocated.Load());
        Assert.DoesNotContain(relocated.Root, Encoding.UTF8.GetString(Compilation.Bytes.Span));
        Assert.DoesNotContain(relocated.Root, Encoding.UTF8.GetString(RuleVocabularyAuditWriter.Write(Report).Span));
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("fr-FR")]
    public void CultureDoesNotAffectCuratedArtifactOrReport(string culture)
    {
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            Compare(Snapshot);
        }
        finally { CultureInfo.CurrentCulture = previous; CultureInfo.CurrentUICulture = previousUi; }
    }

    [Fact]
    public void AuditPolicyDoesNotRewriteArtifactOrHideUncoveredRules()
    {
        var defaultReport = RuleVocabularyAuditor.Analyze(Snapshot, Compilation);
        Assert.Equal(0, defaultReport.Summary.WarningCount);
        Assert.Equal(32, defaultReport.Summary.UncoveredSnippetCount);
        Assert.Equal(Compilation.Artifact!.Integrity.ArtifactSha256, defaultReport.ArtifactSha256);
        Assert.Equal(Expected.SerializedSha256, Hash(Compilation.Bytes));
    }

    [Fact]
    public void CapturedByteFixtureSurvivesCheckoutNewlineConversions()
    {
        using var checkout = new CanonicalVocabularyPayload();
        foreach (var path in Directory.EnumerateFiles(checkout.Root, "*", SearchOption.AllDirectories))
        {
            File.WriteAllText(path, File.ReadAllText(path).ReplaceLineEndings("\r\n"), new UTF8Encoding(false));
        }
        using var curated = new CanonicalVocabularyPayload(checkout.Root);
        using var historical = new CanonicalVocabularyPayload(checkout.Root, historical: true);
        Compare(curated.Load());
        Assert.Equal("802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC", Hash(RuleCompilationPipeline.Compile(historical.Load()).Bytes));
    }

    [Fact]
    public void LiveMaterializationExposesProseChangesWithoutRewritingHistoricalEvidence()
    {
        using var checkout = new CanonicalVocabularyPayload();
        var path = Path.Combine(checkout.Root, Snapshot.Sources[0].ManifestEntry.Path.Replace('/', Path.DirectorySeparatorChar));
        File.AppendAllText(path, "\nAltered rule prose.");
        using var changed = new CanonicalVocabularyPayload(checkout.Root, current: true);
        Assert.NotEqual(Compilation.Artifact!.Integrity.ArtifactSha256,
            RuleCompilationPipeline.Compile(changed.Load()).Artifact!.Integrity.ArtifactSha256);
        using var frozen = new CanonicalVocabularyPayload();
        Assert.Equal(Compilation.Bytes.ToArray(), RuleCompilationPipeline.Compile(frozen.Load()).Bytes.ToArray());
    }

    private static void Compare(MaterializedRuleSourceSnapshot snapshot)
    {
        var result = RuleCompilationPipeline.Compile(snapshot);
        Assert.True(result.IsValid);
        Assert.Equal(Compilation.Bytes.ToArray(), result.Bytes.ToArray());
        Assert.Equal(RuleVocabularyAuditWriter.Write(Report).ToArray(), RuleVocabularyAuditWriter.Write(
            RuleVocabularyAuditor.Analyze(snapshot, result, CanonicalVocabularyPayload.AuditPolicy())).ToArray());
    }

    private static string WithoutRetrieval(CompiledRulesArtifact artifact) => JsonSerializer.Serialize(artifact.Snippets.Select(item => new
    { item.SnippetId, item.RuleSourceId, item.SourceAnchor, item.Content, item.ContentSha256, item.EstimatedTokens }));
    private static string Hash(ReadOnlyMemory<byte> bytes) => Convert.ToHexString(SHA256.HashData(bytes.Span));
}
