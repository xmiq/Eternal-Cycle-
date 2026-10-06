using System.Globalization;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class OrdinaryAuthorityCorrectionTests(ITestOutputHelper output)
{
    private static readonly RuleCompilationResult Compilation = OrdinaryAuthorityCandidate.Compile();
    public static IEnumerable<object[]> Cases() => OrdinaryAuthorityCandidate.Cases().Select(item => new object[] { item.Id });

    [Theory]
    [MemberData(nameof(Cases))]
    public void R2_ActualOrdinaryRequestHasCompleteDeterministicClosure(string id)
    {
        Assert.True(Compilation.IsValid);
        var fixture = OrdinaryAuthorityCandidate.Cases().Single(item => item.Id == id);
        var measured = CompiledRuleQualityEvidence.Measure(Compilation.Artifact!, fixture);
        output.WriteLine(JsonSerializer.Serialize(measured.Evidence));
        if (id is "save" or "combined")
            Assert.Contains(measured.Result!.Members, member => member.SnippetId == "core-context-assembly#manual-persistence-commands");
        if (id is "save" or "combined" or "adjudication" or "agency" or "secrets" or "ecology" or "direct-authority" or "explicit-authority")
        {
            Assert.Contains(measured.Result!.Members, member => member.SnippetId == "core-persistence-authority#executable-authority");
            // The single matched body carries all definitions/qualifications, not
            // a sibling assumed in memory or obtained by a selective dependency.
            var body = Compilation.Artifact!.Snippets.Single(item => item.SnippetId == "core-persistence-authority#executable-authority");
            foreach (var clause in new[] { "E01", "E02", "E03", "E04", "E05", "E06", "E07", "E08", "E09", "E10", "E11", "E12", "E13", "E15" })
                Assert.Contains(clause, body.Content);
        }
    }

    [Fact]
    public void R2_CurrentCorpusAndEvidenceAreSeparateFromImmutableHistoricalCorpus()
    {
        Assert.True(Compilation.IsValid);
        var artifact = Compilation.Artifact!;
        Assert.Empty(CompiledRulesArtifactContract.Validate(artifact));
        Assert.Equal(11, artifact.RuleSources.Count);
        Assert.Equal(130, artifact.Snippets.Count);
        var old = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(CompiledRulesImportFixtures.Canonical())).Artifact!;
        Assert.Equal(154, old.Snippets.Count);
        Assert.Equal(773, old.Snippets.Sum(item => item.Retrieval.Terms.Count));
        Assert.NotEqual(old.Integrity.ArtifactSha256, artifact.Integrity.ArtifactSha256);
        Assert.DoesNotContain(artifact.RuleSources, source => source.RuleSourceId == "core-context-adoption");
        foreach (var id in new[] { "core-context-assembly", "core-authority-governance" })
            Assert.Equal(new[] { "core-persistence-authority" }, artifact.RuleSources.Single(source => source.RuleSourceId == id).DependencyRuleSourceIds);
        // Only the explicitly reviewed partition may alter executable content.
        foreach (var snippet in old.Snippets.Where(item => item.RuleSourceId is not ("core-context-assembly" or "core-persistence-authority")))
        {
            var current = artifact.Snippets.Single(item => item.SnippetId == snippet.SnippetId);
            Assert.Equal(snippet.ContentSha256, current.ContentSha256);
            Assert.Equal(JsonSerializer.Serialize(snippet.Retrieval), JsonSerializer.Serialize(current.Retrieval));
        }
        using var historical = new CanonicalVocabularyPayload();
        Assert.Equal(CompiledRulesImportFixtures.Canonical(), RuleCompilationPipeline.Compile(historical.Load()).Bytes.ToArray());
        output.WriteLine($"Sources={artifact.RuleSources.Count} snippets={artifact.Snippets.Count} terms={artifact.Snippets.Sum(item => item.Retrieval.Terms.Count)} bytes={Compilation.Bytes.Length} semantic={artifact.Integrity.ArtifactSha256} byteHash={CompiledRuleQualityEvidence.Hash(Compilation.Bytes.Span)}");
    }

    [Fact]
    public void R2_RelocationCultureAndCwdPreserveActualCompilationAndReports()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalDirectory = Environment.CurrentDirectory;
        var report = CompiledRuleQualityEvidence.Report(Compilation.Artifact!, Compilation.Bytes.ToArray(), OrdinaryAuthorityCandidate.Cases());
        Assert.Equal(File.ReadAllBytes(CompiledRuleQualityEvidence.FixturePath("r2-expected-report.json")), report);
        using var first = new CanonicalVocabularyPayload(current: true);
        using var second = new CanonicalVocabularyPayload(current: true);
        try
        {
            foreach (var payload in new[] { first, second })
            {
                Environment.CurrentDirectory = payload.Root;
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
                var actual = RuleCompilationPipeline.Compile(OrdinaryAuthorityCandidate.Load(payload));
                Assert.Equal(Compilation.Bytes.ToArray(), actual.Bytes.ToArray());
                Assert.Equal(report, CompiledRuleQualityEvidence.Report(actual.Artifact!, actual.Bytes.ToArray(), OrdinaryAuthorityCandidate.Cases()));
            }
        }
        finally { CultureInfo.CurrentCulture = originalCulture; Environment.CurrentDirectory = originalDirectory; }
        // Explicit evidence export, never replacement of a historical golden.
        var destination = Environment.GetEnvironmentVariable("ETERNAL_CYCLE_R2_EVIDENCE");
        if (destination is not null)
        {
            Directory.CreateDirectory(destination);
            File.WriteAllBytes(Path.Combine(destination, "r2-retrieval-report.json"), report);
            File.WriteAllBytes(Path.Combine(destination, "r2-artifact.json"), Compilation.Bytes.ToArray());
            var snapshot = OrdinaryAuthorityCandidate.Load(first);
            var audit = RuleVocabularyAuditor.Analyze(snapshot, Compilation, CanonicalVocabularyPayload.AuditPolicy());
            Assert.True(audit.IsValid);
            File.WriteAllBytes(Path.Combine(destination, "r2-vocabulary-audit.json"), RuleVocabularyAuditWriter.Write(audit).ToArray());
        }
    }
}
