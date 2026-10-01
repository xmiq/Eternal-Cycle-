using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace EternalCycle.Rules.Testing;

internal sealed class CanonicalVocabularyPayload : IDisposable
{
    public const string ManifestPath = "docs/rules/rule-source-manifest.json";
    public const string SourceIdentity = "d5028af32cf16fe48878ffef0301867175f98f08";
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "EternalCycle.CanonicalVocabulary", Guid.NewGuid().ToString("N"));

    public CanonicalVocabularyPayload(string? sourceRoot = null, bool historical = false)
    {
        var source = Load(sourceRoot ?? FindRepositoryRoot());
        var baseline = Baseline();
        // Fixture preparation, not compiler normalization: retain the exact captured
        // materialization across Git checkout newline policies. Any prose change fails.
        foreach (var item in source.Sources)
        {
            var bytes = Convert.FromBase64String(baseline.SourceBytes.Single(entry => entry.RuleSourceId == item.ManifestEntry.RuleSourceId).BytesBase64);
            if (Encoding.UTF8.GetString(bytes).ReplaceLineEndings("\n") != item.DecodedText.ReplaceLineEndings("\n"))
            {
                throw new InvalidOperationException("Canonical source text changed since the pre-E checkpoint: " + item.ManifestEntry.RuleSourceId);
            }
        }
        // Validate before creating files, so rejected prose does not leak a partial fixture.
        Write(ManifestPath, historical ? Convert.FromBase64String(baseline.ManifestBytesBase64) :
            Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(source.AuthoritativeManifestBytes.Span).ReplaceLineEndings("\n")));
        foreach (var item in source.Sources)
        {
            Write(item.ManifestEntry.Path, Convert.FromBase64String(baseline.SourceBytes.Single(entry =>
                entry.RuleSourceId == item.ManifestEntry.RuleSourceId).BytesBase64));
        }
    }

    public MaterializedRuleSourceSnapshot Load() => Load(Root);

    public static MaterializedRuleSourceSnapshot Load(string root) => MaterializedRuleSourceLoader.Load(new(root, ManifestPath,
        new() { Scheme = "git-commit", Value = SourceIdentity },
        new() { ContractVersion = "1", ImplementationId = "eternal-cycle-dotnet", ImplementationVersion = "1.0.0" }));

    public static MaterializedRuleSourceSnapshot LoadHistorical(string root)
    {
        // Preserve the exact pre-E manifest bytes, including CRLF, without relying on
        // Git or platform checkout newline policy. Compilation needs only this memory.
        using var payload = new CanonicalVocabularyPayload(root, historical: true);
        return payload.Load();
    }

    public static MaterializedRuleSourceSnapshot LoadCurated(string root)
    {
        using var payload = new CanonicalVocabularyPayload(root);
        return payload.Load();
    }

    public static CanonicalBeforeE Baseline() => JsonSerializer.Deserialize<CanonicalBeforeE>(
        File.ReadAllBytes(FixturePath("before-e.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

    public static T Fixture<T>(string name) => JsonSerializer.Deserialize<T>(File.ReadAllBytes(FixturePath(name)),
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } })!;

    public static RuleVocabularyAuditPolicy AuditPolicy() => Fixture<RuleVocabularyAuditPolicy>("audit-policy.json");

    public static string FixturePath(string name) => Path.Combine(AppContext.BaseDirectory, "CanonicalVocabularyFixture", name);

    public static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, ManifestPath.Replace('/', Path.DirectorySeparatorChar)))) { directory = directory.Parent; }
        return directory?.FullName ?? throw new InvalidOperationException("Canonical Rule Source payload was not found.");
    }

    private void Write(string relativePath, byte[] bytes)
    {
        var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
    }

    public void Dispose() => Directory.Delete(Root, recursive: true);
}

internal sealed record CanonicalBeforeE(string ManifestBytesBase64, IReadOnlyList<CanonicalSourceHash> SourceHashes, IReadOnlyList<CanonicalSourceBytes> SourceBytes);
internal sealed record CanonicalSourceHash(string RuleSourceId, string SourceSha256);
internal sealed record CanonicalSourceBytes(string RuleSourceId, string BytesBase64);
internal sealed record CanonicalQualityFixture(IReadOnlyList<CanonicalQualityExpectation> Expectations, IReadOnlyList<CanonicalWarningReview> AcceptedWarnings);
internal sealed record CanonicalQualityExpectation(string Id, string ConceptId, string Term, IReadOnlyList<string> PresentOn, IReadOnlyList<string> AbsentFrom, string Rationale);
internal sealed record CanonicalWarningReview(string Code, string SubjectId, string Rationale);
internal sealed record CanonicalAfterE(RuleVocabularyAuditMetrics Summary, int ArtifactBytes, string SemanticDigest,
    string SerializedSha256, string AuditSha256, IReadOnlyList<string> UncoveredSnippetIds, IReadOnlyList<CanonicalFindingCount> FindingCounts);
internal sealed record CanonicalFindingCount(string Code, int Count);
