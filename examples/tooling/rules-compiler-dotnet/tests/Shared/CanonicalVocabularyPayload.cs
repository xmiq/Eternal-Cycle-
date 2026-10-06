using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

namespace EternalCycle.Rules.Testing;

internal sealed class CanonicalVocabularyPayload : IDisposable
{
    public const string ManifestPath = "docs/rules/rule-source-manifest.json";
    public const string SourceIdentity = "d5028af32cf16fe48878ffef0301867175f98f08";
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "EternalCycle.CanonicalVocabulary", Guid.NewGuid().ToString("N"));

    public CanonicalVocabularyPayload(string? sourceRoot = null, bool historical = false, bool current = false)
    {
        var baseline = Baseline();
        if (current)
        {
            var source = Load(sourceRoot ?? FindRepositoryRoot());
            // An explicit LF test materialization makes checkout policy irrelevant.
            // The compiler still hashes every exact byte of this materialized payload.
            Write(ManifestPath, Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(source.AuthoritativeManifestBytes.Span).ReplaceLineEndings("\n")));
            foreach (var item in source.Sources)
            {
                Write(item.ManifestEntry.Path, Encoding.UTF8.GetBytes(item.DecodedText.ReplaceLineEndings("\n")));
            }
            return;
        }
        // Historical FR-023/024/026 evidence is immutable, not a golden silently
        // regenerated from changed current authority. R2 has separate live-corpus tests.
        Write(ManifestPath, historical ? Convert.FromBase64String(baseline.ManifestBytesBase64) :
            File.ReadAllBytes(FixturePath("before-r2-manifest.json")));
        using var manifest = JsonDocument.Parse(Convert.FromBase64String(baseline.ManifestBytesBase64));
        var paths = manifest.RootElement.GetProperty("sources").EnumerateArray().ToDictionary(
            entry => entry.GetProperty("ruleSourceId").GetString()!, entry => entry.GetProperty("path").GetString()!, StringComparer.Ordinal);
        foreach (var item in baseline.SourceBytes)
        {
            Write(paths[item.RuleSourceId], Convert.FromBase64String(item.BytesBase64));
        }
    }

    public MaterializedRuleSourceSnapshot Load() => Load(Root);

    public MaterializedRuleSourceSnapshot LoadContentAddressed()
    {
        var source = Load();
        // Delimited IDs and exact byte hashes avoid ambiguous byte concatenation.
        var evidence = source.ManifestSha256 + "\n" + string.Join("\n", source.Sources.Select(
            item => item.ManifestEntry.RuleSourceId + ":" + item.SourceSha256));
        var identity = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(evidence)));
        return MaterializedRuleSourceLoader.Load(new(Root, ManifestPath,
            new() { Scheme = "materialized-sha256", Value = identity }, source.CompilerIdentity));
    }

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
