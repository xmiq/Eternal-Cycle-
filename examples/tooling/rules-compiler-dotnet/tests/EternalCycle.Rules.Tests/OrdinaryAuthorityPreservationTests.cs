using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class OrdinaryAuthorityPreservationTests
{
    private static readonly RuleCompilationResult Compilation = OrdinaryAuthorityCandidate.Compile();
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private static T Fixture<T>(string file) => JsonSerializer.Deserialize<T>(File.ReadAllBytes(CompiledRuleQualityEvidence.FixturePath(file)), Json)!;
    private sealed record Responsibility(string ClauseId, string OldAnchor, string Obligation, string OwnerDocument, string ExecutableOwners, string Classification);
    private sealed record SnippetMove(string OldSnippetId, string? TargetSnippetId, string Disposition, string OwnerDocument, string Reason);
    private sealed record Identities(IReadOnlyList<SnippetMove> Snippets, IReadOnlyList<string> NewSnippetIds);
    private sealed record BindingMove(string OldTarget, IReadOnlyList<string> ConceptIds, string OldRationale, string Target, string Disposition);
    private static readonly CanonicalQualityFixture Quality = Fixture<CanonicalQualityFixture>("r2-quality-expectations.json");
    public static IEnumerable<object[]> QualityCases() => Quality.Expectations.Select(item => new object[] { item.Id });

    [Theory]
    [MemberData(nameof(QualityCases))]
    public void R2_ReviewedTopologyRetainsPositiveAndNegativeConceptBoundaries(string id)
    {
        var item = Quality.Expectations.Single(item => item.Id == id);
        foreach (var target in item.PresentOn)
            Assert.Contains(Compilation.Artifact!.Snippets.Single(snippet => snippet.SnippetId == target).Retrieval.Terms,
                term => term.Kind == item.ConceptId && term.Term == item.Term);
        foreach (var target in item.AbsentFrom)
            Assert.DoesNotContain(Compilation.Artifact!.Snippets.Single(snippet => snippet.SnippetId == target).Retrieval.Terms,
                term => term.Kind == item.ConceptId && term.Term == item.Term);
    }

    [Fact]
    public void R2_All273ResponsibilitiesHaveCanonicalOwnersAndAllRuntimeIdentitiesHaveDispositions()
    {
        var root = CanonicalVocabularyPayload.FindRepositoryRoot();
        var rows = Fixture<Responsibility[]>("r2-responsibility-map.json");
        Assert.Equal(273, rows.Length);
        Assert.Equal(80, rows.Count(row => row.ClauseId.StartsWith("PA-", StringComparison.Ordinal)));
        Assert.Equal(193, rows.Count(row => row.ClauseId.StartsWith("CA-", StringComparison.Ordinal)));
        Assert.Equal(rows.Length, rows.Select(row => row.ClauseId).Distinct(StringComparer.Ordinal).Count());
        foreach (var row in rows)
        {
            var document = File.ReadAllText(Path.Combine(root, row.OwnerDocument));
            Assert.NotEmpty(row.Obligation);
            if (row.ClauseId.StartsWith("PA-", StringComparison.Ordinal))
            {
                // Check every explicit owner, including range notation in the review.
                foreach (Match match in Regex.Matches(row.ExecutableOwners, @"([EG])(\d+)(?:-([EG])?(\d+))?"))
                {
                    var first = int.Parse(match.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
                    var last = match.Groups[4].Success ? int.Parse(match.Groups[4].Value, System.Globalization.CultureInfo.InvariantCulture) : first;
                    for (var number = first; number <= last; number++)
                    {
                        var code = match.Groups[1].Value + number.ToString("00", System.Globalization.CultureInfo.InvariantCulture);
                        var owner = code.StartsWith('G') ? "docs/persistence/AUTHORITY_GOVERNANCE.md" : "docs/persistence/PERSISTENCE_AUTHORITY.md";
                        Assert.Contains(code, File.ReadAllText(Path.Combine(root, owner)));
                    }
                }
            }
            else
            {
                var headings = Regex.Matches(document, @"(?m)^#{1,6} (.+)\r?$")
                    .Select(match => Regex.Replace(match.Groups[1].Value.Trim().ToLowerInvariant(), @"[^a-z0-9 -]", "").Replace(' ', '-'));
                Assert.Contains(row.OldAnchor, headings);
            }
        }
        var identities = Fixture<Identities>("r2-identity-map.json");
        var old = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(CompiledRulesImportFixtures.Canonical())).Artifact!;
        Assert.Equal(old.Snippets.Where(snippet => snippet.RuleSourceId is "core-context-assembly" or "core-persistence-authority").Select(item => item.SnippetId).Order(StringComparer.Ordinal),
            identities.Snippets.Select(item => item.OldSnippetId).Order(StringComparer.Ordinal));
        Assert.Equal(56, identities.Snippets.Count);
        foreach (var item in identities.Snippets)
        {
            Assert.True(File.Exists(Path.Combine(root, item.OwnerDocument)));
            if (item.TargetSnippetId is not null) Assert.Contains(Compilation.Artifact!.Snippets, snippet => snippet.SnippetId == item.TargetSnippetId);
            else Assert.DoesNotContain(Compilation.Artifact!.Snippets, snippet => snippet.SnippetId == item.OldSnippetId);
        }
        Assert.Equal(identities.NewSnippetIds.Order(StringComparer.Ordinal),
            Compilation.Artifact!.Snippets.Select(item => item.SnippetId).Except(old.Snippets.Select(item => item.SnippetId), StringComparer.Ordinal).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void R2_RuntimeContextAndHostCompanionsRetainOriginalSubstanceVerbatim()
    {
        var root = CanonicalVocabularyPayload.FindRepositoryRoot();
        var old = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(CompiledRulesImportFixtures.Canonical())).Artifact!;
        var excluded = new[] { "runtime-boundary", "related-documents", "existing-campaign-adoption", "storage-neutral-logical-guidance", "regression-cases", "acceptance-criteria" };
        foreach (var snippet in old.Snippets.Where(item => item.RuleSourceId == "core-context-assembly"))
        {
            var anchor = snippet.SourceAnchor;
            if (!excluded.Contains(anchor, StringComparer.Ordinal))
                Assert.Equal(snippet.ContentSha256, Compilation.Artifact!.Snippets.Single(item => item.SnippetId == snippet.SnippetId).ContentSha256);
            else if (anchor is not ("runtime-boundary" or "related-documents"))
            {
                var owner = anchor == "existing-campaign-adoption" ? "docs/ai/CONTEXT_ASSEMBLY_ADOPTION.md" : "docs/ai/CONTEXT_ASSEMBLY_CONFORMANCE.md";
                Assert.Contains(snippet.Content.Trim(), File.ReadAllText(Path.Combine(root, owner)).ReplaceLineEndings("\n"));
            }
        }
        var examples = old.Snippets.Single(item => item.SnippetId == "core-persistence-authority#worked-examples");
        Assert.Contains(examples.Content.Trim(), File.ReadAllText(Path.Combine(root, "docs/persistence/PERSISTENCE_AUTHORITY_CONFORMANCE.md")).ReplaceLineEndings("\n"));
    }

    [Fact]
    public void R2_AllReviewedBindingsOriginsAndWeightsAreAccountedFor()
    {
        using var before = new CanonicalVocabularyPayload();
        using var after = new CanonicalVocabularyPayload(current: true);
        var old = before.Load().Manifest.RetrievalVocabulary!;
        var current = after.Load().Manifest.RetrievalVocabulary!;
        Assert.Equal(JsonSerializer.Serialize(old.Concepts), JsonSerializer.Serialize(current.Concepts));
        var map = Fixture<BindingMove[]>("r2-binding-map.json");
        Assert.Equal(42, map.Length);
        foreach (var move in map)
        {
            var prior = old.Bindings.Single(binding => binding.RuleSourceId + "#" + binding.SourceAnchor == move.OldTarget);
            Assert.Equal(prior.ConceptIds.Order(StringComparer.Ordinal), move.ConceptIds.Order(StringComparer.Ordinal));
            Assert.Equal(prior.Rationale, move.OldRationale);
            var target = current.Bindings.Single(binding => binding.RuleSourceId + "#" + binding.SourceAnchor == move.Target);
            Assert.All(move.ConceptIds, concept => Assert.Contains(concept, target.ConceptIds));
        }
        var report = RuleVocabularyAuditor.Analyze(OrdinaryAuthorityCandidate.Load(after), Compilation, CanonicalVocabularyPayload.AuditPolicy());
        Assert.True(report.IsValid);
        Assert.Equal(3, report.Summary.WarningCount);
        Assert.All(report.Diagnostics.Where(item => item.Severity == RuleVocabularyAuditSeverity.Warning),
            item => Assert.Contains(Quality.AcceptedWarnings, warning => warning.Code == item.Code && warning.SubjectId == item.SubjectId));
    }
}
