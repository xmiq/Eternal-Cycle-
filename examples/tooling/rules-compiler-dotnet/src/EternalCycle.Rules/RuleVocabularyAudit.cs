using System.Text.Json;
using System.Text.Json.Serialization;

namespace EternalCycle.Rules;

public enum RuleVocabularyAuditSeverity { Error, Warning, Information }

public sealed record RuleVocabularyAuditPolicy
{
    public int MinimumBroadTargets { get; init; } = 8;
    public int MinimumBroadCoveragePercent { get; init; } = 50;
    public int LargeSnippetTokens { get; init; } = 2048;
    public IReadOnlyList<string> GenericTerms { get; init; } = [];

    public RuleVocabularyAuditPolicy Normalize()
    {
        if (MinimumBroadTargets < 1 || MinimumBroadCoveragePercent is < 1 or > 100 || LargeSnippetTokens < 1 ||
            GenericTerms is null || GenericTerms.Count > 256)
        {
            throw new ArgumentException("Audit thresholds must be positive, coverage at most 100, and generic terms at most 256.");
        }
        return this with { GenericTerms = GenericTerms.Select(RuleRetrievalVocabulary.NormalizeTerm)
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray() };
    }
}

public sealed record RuleVocabularyAuditRatio(int Numerator, int Denominator);

public sealed record RuleVocabularyAuditMetrics
{
    public int SourceCount { get; init; }
    public int SnippetCount { get; init; }
    public int ConceptCount { get; init; }
    public int CanonicalTermCount { get; init; }
    public int AliasCount { get; init; }
    public int PhraseCount { get; init; }
    public int BindingCount { get; init; }
    public int EffectiveAssociationCount { get; init; }
    public int RetainedOriginCount { get; init; }
    public int CoveredSnippetCount { get; init; }
    public int UncoveredSnippetCount { get; init; }
    public int ConceptsWithAliases { get; init; }
    public int ConceptsWithoutAliases { get; init; }
    public int ConceptsWithTargets { get; init; }
    public int ConceptsWithoutTargets { get; init; }
    public int UniqueTermCount { get; init; }
    public int MultiOriginAssociationCount { get; init; }
    public int SourceOnlyAssociationCount { get; init; }
    public RuleVocabularyAuditRatio AverageAssociationsPerCoveredSnippet { get; init; } = new(0, 0);
    public int MaximumAssociationsPerSnippet { get; init; }
    public RuleVocabularyAuditRatio AverageTargetSnippetsPerTerm { get; init; } = new(0, 0);
    public int MaximumTargetSnippetsPerTerm { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int InformationCount { get; init; }
}

public sealed record RuleVocabularyAuditFinding(
    string Code, RuleVocabularyAuditSeverity Severity, string SubjectKind, string SubjectId, string Message);

public sealed record RuleVocabularyAuditSnippet(
    string SnippetId, string RuleSourceId, int AssociationCount, int OriginCount,
    int SourceOnlyAssociationCount, int EstimatedTokens);

public sealed record RuleVocabularyAuditConcept(
    string ConceptId, string CanonicalTerm, int AliasCount, int PhraseCount, int BindingCount,
    int AssociationCount, IReadOnlyList<string> SnippetIds, IReadOnlyList<string> SourceIds, IReadOnlyList<string> SharedTerms);

public sealed record RuleVocabularyAuditTermTarget(
    string ConceptId, string Kind, int Weight, IReadOnlyList<string> SnippetIds);

public sealed record RuleVocabularyAuditTerm(
    string Term, IReadOnlyList<RuleVocabularyAuditTermTarget> Targets, IReadOnlyList<string> SnippetIds,
    IReadOnlyList<string> SourceIds, IReadOnlyList<string> OriginKinds,
    RuleVocabularyAuditRatio CorpusCoverage, bool DifferentConceptTargetSets);

public sealed record RuleVocabularyAuditAssociation(
    string SnippetId, string ConceptId, string Term, string Kind, int Weight,
    IReadOnlyList<RuleVocabularyBindingIdentity> Origins);

public sealed record RuleVocabularyAuditBinding(
    RuleVocabularyBindingIdentity Identity, IReadOnlyList<string> ConceptIds, IReadOnlyList<string> SnippetIds,
    int AssociationCount, int UniqueAssociationCount);

public sealed record RuleVocabularyAuditReport(
    int AuditFormatVersion, string? ArtifactSha256, string ManifestSha256, RuleVocabularyAuditPolicy Policy,
    RuleVocabularyAuditMetrics Summary, IReadOnlyList<RuleVocabularyAuditSnippet> Snippets,
    IReadOnlyList<RuleVocabularyAuditConcept> Concepts, IReadOnlyList<RuleVocabularyAuditTerm> Terms,
    IReadOnlyList<RuleVocabularyAuditBinding> Bindings, IReadOnlyList<RuleVocabularyAuditAssociation> Associations,
    IReadOnlyList<RuleVocabularyAuditFinding> Diagnostics)
{
    [JsonIgnore]
    public bool IsValid => Summary.ErrorCount == 0;
}

public static class RuleVocabularyAuditWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    // Reports contain ordered records/arrays, never dictionaries. Declaration order is
    // the sole JSON property order; compact UTF-8 has no BOM or trailing newline.
    public static ReadOnlyMemory<byte> Write(RuleVocabularyAuditReport report) =>
        JsonSerializer.SerializeToUtf8Bytes(report, Options);
}
