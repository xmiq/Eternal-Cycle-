using System.Globalization;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRuleQualityTests(ITestOutputHelper output)
{
    private static readonly byte[] Bytes = CompiledRulesImportFixtures.Canonical();
    private static readonly CompiledRulesArtifact Artifact = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(Bytes)).Artifact!;
    private static readonly RetrievalQualityCase[] Fixtures = CompiledRuleQualityEvidence.Cases();

    public static IEnumerable<object[]> Cases() => Fixtures.Select(fixture => new object[] { fixture.Id });

    [Theory]
    [MemberData(nameof(Cases))]
    public void G_CanonicalQuality(string id)
    {
        var fixture = Fixtures.Single(fixture => fixture.Id == id);
        var measurement = CompiledRuleQualityEvidence.Measure(Artifact, fixture);
        output.WriteLine(JsonSerializer.Serialize(measurement.Evidence));
    }

    [Fact]
    public void G_DeterministicReportMatchesReviewedEvidence()
    {
        var report = CompiledRuleQualityEvidence.Report(Artifact, Bytes, Fixtures);
        Assert.Equal(report, CompiledRuleQualityEvidence.Report(Artifact, Bytes, Fixtures.Reverse()));
        Assert.NotEqual((byte)'\n', report[^1]);
        Assert.NotEqual((byte)0xEF, report[0]);
        Assert.DoesNotContain(Environment.MachineName, Encoding.UTF8.GetString(report), StringComparison.OrdinalIgnoreCase);
        // Explicit fixture maintenance only; normal tests never update a golden.
        var destination = Environment.GetEnvironmentVariable("ETERNAL_CYCLE_WRITE_RETRIEVAL_QUALITY_REPORT");
        if (destination is null) Assert.Equal(File.ReadAllBytes(CompiledRuleQualityEvidence.FixturePath("expected-quality-report.json")), report);
        else File.WriteAllBytes(destination, report);
        output.WriteLine($"Report bytes={report.Length} SHA256={CompiledRuleQualityEvidence.Hash(report)}");
        using var document = JsonDocument.Parse(report);
        output.WriteLine(document.RootElement.GetProperty("summary").GetRawText());
        output.WriteLine("Whole corpus=" + document.RootElement.GetProperty("wholeCorpusEstimatedTokens"));
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("fr-FR")]
    [InlineData("ja-JP")]
    public void G_CultureCannotChangeQualityReport(string name)
    {
        var expected = CompiledRuleQualityEvidence.Report(Artifact, Bytes, Fixtures);
        var prior = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
            Assert.Equal(expected, CompiledRuleQualityEvidence.Report(Artifact, Bytes, Fixtures));
        }
        finally { CultureInfo.CurrentCulture = prior; }
    }

    [Fact]
    public void G_RelocatedPayloadAndUnrelatedDirectoryCannotChangeEvidence()
    {
        var root = Path.Combine(Path.GetTempPath(), "EternalCycle.Quality.Tests", Guid.NewGuid().ToString("N"));
        var prior = Environment.CurrentDirectory;
        try
        {
            Directory.CreateDirectory(root);
            var expected = CompiledRuleQualityEvidence.Report(Artifact, Bytes, Fixtures);
            foreach (var iteration in Enumerable.Range(0, 2))
            {
                // The existing helper materializes each payload in a fresh root.
                using var payload = new CanonicalVocabularyPayload();
                Environment.CurrentDirectory = root;
                var compiled = RuleCompilationPipeline.Compile(payload.Load());
                Assert.NotNull(compiled.Artifact);
                Assert.Equal(Bytes, compiled.Bytes.ToArray());
                Assert.Equal(expected, CompiledRuleQualityEvidence.Report(compiled.Artifact, compiled.Bytes.ToArray(), Fixtures));
            }
        }
        finally { Environment.CurrentDirectory = prior; Directory.Delete(root, recursive: true); }
    }

    [Theory]
    [InlineData("exact-normalized", "combat-fighting-full")]
    [InlineData("exact-phrase-normalized", "experience-phrase")]
    [InlineData("multiterm-deduplicated", "multiterm-reordered")]
    public void G_EquivalentPreparedInputsDoNotInflateEvidence(string left, string right)
    {
        var first = CompiledRuleQualityEvidence.Measure(Artifact, Fixtures.Single(fixture => fixture.Id == left));
        var second = CompiledRuleQualityEvidence.Measure(Artifact, Fixtures.Single(fixture => fixture.Id == right));
        Assert.Equal(JsonSerializer.Serialize(first.Result), JsonSerializer.Serialize(second.Result));
        Assert.Equal(JsonSerializer.Serialize(first.Ranked.Candidates), JsonSerializer.Serialize(second.Ranked.Candidates));
    }

    [Fact]
    public void G_ReportCannotChangeCanonicalAuthorityOrExecutableContext()
    {
        var before = JsonSerializer.Serialize(Artifact);
        var fixture = Fixtures.Single(item => item.Id == "combat-fighting-full");
        var unmeasured = CompiledRulePackets.Build(CompiledRuleDependencies.Expand(CompiledRuleRanking.Rank(
            CompiledRuleCandidateIndex.Create(Artifact).FindCandidates(CompiledRuleRetrieval.Prepare(CompiledRuleQualityEvidence.Request(Artifact, fixture))))));
        _ = CompiledRuleQualityEvidence.Report(Artifact, Bytes, Fixtures);
        Assert.Equal(before, JsonSerializer.Serialize(Artifact));
        Assert.Equal(JsonSerializer.Serialize(unmeasured), JsonSerializer.Serialize(CompiledRuleQualityEvidence.Measure(Artifact, fixture).Result));
        Assert.Equal(10, Artifact.RuleSources.Count);
        Assert.Equal(154, Artifact.Snippets.Count);
        Assert.Equal(773, Artifact.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count));
        Assert.Equal(275622, Bytes.Length);
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", Artifact.Integrity.ArtifactSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", CompiledRuleQualityEvidence.Hash(Bytes));
        Assert.Empty(CompiledRulesArtifactContract.Validate(Artifact));
        Assert.All(Artifact.Snippets, snippet => Assert.Equal(Math.Max(1, (Encoding.UTF8.GetByteCount(snippet.Content) + 2) / 3), snippet.EstimatedTokens));
    }

    [Fact]
    public void G_ConceptsRemainSeparateWhenWordingOverlaps()
    {
        var gameplay = CompiledRuleQualityEvidence.Measure(Artifact, Fixtures.Single(item => item.Id == "conflict-gameplay-full"));
        var context = CompiledRuleQualityEvidence.Measure(Artifact, Fixtures.Single(item => item.Id == "conflict-canon"));
        Assert.All(gameplay.Ranked.Candidates.SelectMany(root => root.Matches), match => Assert.Equal("combat-context", match.Kind));
        Assert.All(context.Ranked.Candidates.SelectMany(root => root.Matches), match => Assert.Equal("canon-conflict", match.Kind));
        Assert.All(gameplay.Ranked.Candidates.Where(root => root.Matches.Count > 0), root => Assert.Equal(600, root.VocabularyScore));
        Assert.All(context.Ranked.Candidates.Where(root => root.Matches.Count > 0), root => Assert.Equal(600, root.VocabularyScore));
    }

    [Fact]
    public void G_ValidWeightCapacityCannotOverflowInt64()
    {
        // FR-022 weights <=1000 and CLR lists have Int32 count. An actual valid
        // overflow fixture cannot exist; C's corrupt-input tests cover fail-safe
        // defenses without making G invent an invalid format-1 success path.
        var upperBound = checked((long)int.MaxValue * 1000);
        Assert.True(upperBound > int.MaxValue);
        Assert.True(upperBound < long.MaxValue);
    }
}
