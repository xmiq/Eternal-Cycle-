using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EternalCycle.Rules;

public sealed class CompiledRulesArtifact
{
    [JsonRequired]
    public int ArtifactFormatVersion { get; init; }

    [JsonRequired]
    public CompiledRulesCompilerIdentity Compiler { get; init; } = new();

    [JsonRequired]
    public CompiledRulesRulesetIdentity Ruleset { get; init; } = new();

    [JsonRequired]
    public IList<CompiledRulesArtifactRuleSource> RuleSources { get; init; } = [];

    [JsonRequired]
    public IList<CompiledRulesArtifactSnippet> Snippets { get; init; } = [];

    [JsonRequired]
    public CompiledRulesArtifactIntegrity Integrity { get; init; } = new();
}

public sealed class CompiledRulesCompilerIdentity
{
    [JsonRequired]
    public string ContractVersion { get; init; } = string.Empty;

    [JsonRequired]
    public string ImplementationId { get; init; } = string.Empty;

    [JsonRequired]
    public string ImplementationVersion { get; init; } = string.Empty;
}

public sealed class CompiledRulesRulesetIdentity
{
    [JsonRequired]
    public string RulesetId { get; init; } = string.Empty;

    [JsonRequired]
    public string RepositoryVersion { get; init; } = string.Empty;

    [JsonRequired]
    public CompiledRulesSourceIdentity Source { get; init; } = new();

    [JsonRequired]
    public string ManifestPath { get; init; } = string.Empty;

    [JsonRequired]
    public string ManifestSha256 { get; init; } = string.Empty;
}

public sealed class CompiledRulesSourceIdentity
{
    [JsonRequired]
    public string Scheme { get; init; } = string.Empty;

    [JsonRequired]
    public string Value { get; init; } = string.Empty;
}

public sealed class CompiledRulesArtifactRuleSource
{
    [JsonRequired]
    public string RuleSourceId { get; init; } = string.Empty;

    [JsonRequired]
    public string SourcePath { get; init; } = string.Empty;

    [JsonRequired]
    public string SourceSha256 { get; init; } = string.Empty;

    [JsonRequired]
    public CompiledRulesArtifactApplicability Applicability { get; init; } = new();

    [JsonRequired]
    public IList<string> DependencyRuleSourceIds { get; init; } = [];
}

public sealed class CompiledRulesArtifactApplicability
{
    [JsonRequired]
    public RuleLayer Layer { get; init; }

    [JsonRequired]
    public IList<string> WorldModelIds { get; init; } = [];

    [JsonRequired]
    public IList<string> ModuleIds { get; init; } = [];

    [JsonRequired]
    public IList<string> CampaignModes { get; init; } = [];

    [JsonRequired]
    public IList<string> Operations { get; init; } = [];

    [JsonRequired]
    public IList<string> Topics { get; init; } = [];

    [JsonRequired]
    public int Priority { get; init; }

    [JsonRequired]
    public bool AlwaysInclude { get; init; }

    [JsonRequired]
    public RulePreparationTier PreparationTier { get; init; } = RulePreparationTier.Standard;
}

public sealed class CompiledRulesArtifactSnippet
{
    [JsonRequired]
    public string SnippetId { get; init; } = string.Empty;

    [JsonRequired]
    public string RuleSourceId { get; init; } = string.Empty;

    [JsonRequired]
    public string? SourceAnchor { get; init; }

    [JsonRequired]
    public string Content { get; init; } = string.Empty;

    [JsonRequired]
    public string ContentSha256 { get; init; } = string.Empty;

    [JsonRequired]
    public int EstimatedTokens { get; init; }

    [JsonRequired]
    public CompiledRulesRetrievalMetadata Retrieval { get; init; } = new();
}

public sealed class CompiledRulesRetrievalMetadata
{
    [JsonRequired]
    public IList<CompiledRulesRetrievalTerm> Terms { get; init; } = [];

    [JsonRequired]
    public IList<CompiledRulesRetrievalRelationship> Relationships { get; init; } = [];
}

public sealed class CompiledRulesRetrievalTerm
{
    [JsonRequired]
    public string Term { get; init; } = string.Empty;

    [JsonRequired]
    public string Kind { get; init; } = string.Empty;

    [JsonRequired]
    public int Weight { get; init; }
}

public sealed class CompiledRulesRetrievalRelationship
{
    [JsonRequired]
    public string FromTerm { get; init; } = string.Empty;

    [JsonRequired]
    public string ToTerm { get; init; } = string.Empty;

    [JsonRequired]
    public string Kind { get; init; } = string.Empty;

    [JsonRequired]
    public int Weight { get; init; }
}

public sealed class CompiledRulesArtifactIntegrity
{
    [JsonRequired]
    public string Algorithm { get; init; } = string.Empty;

    [JsonRequired]
    public string ArtifactSha256 { get; init; } = string.Empty;
}

public sealed record CompiledRulesArtifactValidationError(
    string Code,
    string Path,
    string Message);

public sealed record CompiledRulesArtifactReadResult(
    CompiledRulesArtifact? Artifact,
    IReadOnlyList<CompiledRulesArtifactValidationError> Errors)
{
    public bool IsValid => Artifact is not null && Errors.Count == 0;
}

public static class CompiledRulesArtifactContract
{
    public const int CurrentArtifactFormatVersion = 1;
    public const string CurrentCompilerContractVersion = "1";
    public const string IntegrityAlgorithm = "SHA-256";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    public static CompiledRulesArtifactReadResult Read(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var errors = new List<CompiledRulesArtifactValidationError>();
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 64
            });
            FindDuplicateProperties(document.RootElement, "$", errors);

            var artifact = JsonSerializer.Deserialize<CompiledRulesArtifact>(json, JsonOptions);
            if (artifact is null)
            {
                errors.Add(new("JSON_INVALID", "$", "The artifact JSON did not produce an artifact object."));
                return new(null, errors);
            }

            Validate(artifact, errors);
            return new(artifact, errors);
        }
        catch (JsonException exception)
        {
            errors.Add(new(
                "JSON_INVALID",
                exception.Path ?? "$",
                "The artifact is not valid contract JSON."));
            return new(null, errors);
        }
    }

    public static IReadOnlyList<CompiledRulesArtifactValidationError> Validate(
        CompiledRulesArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        var errors = new List<CompiledRulesArtifactValidationError>();
        Validate(artifact, errors);
        return errors;
    }

    public static string CreateSnippetId(string ruleSourceId, string? sourceAnchor) =>
        string.IsNullOrEmpty(sourceAnchor)
            ? ruleSourceId
            : $"{ruleSourceId}#{sourceAnchor}";

    public static string ComputeContentSha256(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
    }

    public static string ComputeArtifactSha256(CompiledRulesArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var writer = new DigestWriter(hash);

        writer.Value("EC-COMPILED-RULES-V1");
        writer.Value(artifact.ArtifactFormatVersion);
        writer.Value(artifact.Compiler.ContractVersion);
        writer.Value(artifact.Compiler.ImplementationId);
        writer.Value(artifact.Compiler.ImplementationVersion);
        writer.Value(artifact.Ruleset.RulesetId);
        writer.Value(artifact.Ruleset.RepositoryVersion);
        writer.Value(artifact.Ruleset.Source.Scheme);
        writer.Value(artifact.Ruleset.Source.Value);
        writer.Value(artifact.Ruleset.ManifestPath);
        writer.Value(artifact.Ruleset.ManifestSha256);
        writer.Value(artifact.RuleSources.Count);
        foreach (var source in artifact.RuleSources)
        {
            writer.Value(source.RuleSourceId);
            writer.Value(source.SourcePath);
            writer.Value(source.SourceSha256);
            writer.Value(source.Applicability.Layer.ToString());
            writer.Values(source.Applicability.WorldModelIds);
            writer.Values(source.Applicability.ModuleIds);
            writer.Values(source.Applicability.CampaignModes);
            writer.Values(source.Applicability.Operations);
            writer.Values(source.Applicability.Topics);
            writer.Value(source.Applicability.Priority);
            writer.Value(source.Applicability.AlwaysInclude);
            writer.Value(source.Applicability.PreparationTier.ToString());
            writer.Values(source.DependencyRuleSourceIds);
        }

        writer.Value(artifact.Snippets.Count);
        foreach (var snippet in artifact.Snippets)
        {
            writer.Value(snippet.SnippetId);
            writer.Value(snippet.RuleSourceId);
            writer.Value(snippet.SourceAnchor ?? string.Empty);
            writer.Value(snippet.Content);
            writer.Value(snippet.ContentSha256);
            writer.Value(snippet.EstimatedTokens);
            writer.Value(snippet.Retrieval.Terms.Count);
            foreach (var term in snippet.Retrieval.Terms)
            {
                writer.Value(term.Term);
                writer.Value(term.Kind);
                writer.Value(term.Weight);
            }

            writer.Value(snippet.Retrieval.Relationships.Count);
            foreach (var relationship in snippet.Retrieval.Relationships)
            {
                writer.Value(relationship.FromTerm);
                writer.Value(relationship.ToTerm);
                writer.Value(relationship.Kind);
                writer.Value(relationship.Weight);
            }
        }

        writer.Value(artifact.Integrity.Algorithm);
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static void Validate(
        CompiledRulesArtifact artifact,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (artifact.Compiler is null ||
            artifact.Ruleset is null ||
            artifact.RuleSources is null ||
            artifact.Snippets is null ||
            artifact.Integrity is null)
        {
            Error(errors, "STRUCTURE_REQUIRED", "$", "Required artifact objects and arrays cannot be null.");
            return;
        }

        if (artifact.Ruleset.Source is null)
        {
            Error(errors, "STRUCTURE_REQUIRED", "$.ruleset.source", "The immutable Rule Source identity is required.");
            return;
        }

        if (artifact.ArtifactFormatVersion != CurrentArtifactFormatVersion)
        {
            Error(errors, "FORMAT_VERSION_UNSUPPORTED", "$.artifactFormatVersion", "The artifact format version is not supported.");
        }

        RequiredIdentifier(artifact.Compiler.ContractVersion, "$.compiler.contractVersion", errors);
        if (!string.Equals(artifact.Compiler.ContractVersion, CurrentCompilerContractVersion, StringComparison.Ordinal))
        {
            Error(errors, "COMPILER_CONTRACT_UNSUPPORTED", "$.compiler.contractVersion", "The compiler contract version is not supported.");
        }

        RequiredIdentifier(artifact.Compiler.ImplementationId, "$.compiler.implementationId", errors);
        RequiredValue(artifact.Compiler.ImplementationVersion, "$.compiler.implementationVersion", errors);
        RequiredIdentifier(artifact.Ruleset.RulesetId, "$.ruleset.rulesetId", errors);
        RequiredValue(artifact.Ruleset.RepositoryVersion, "$.ruleset.repositoryVersion", errors);
        RequiredIdentifier(artifact.Ruleset.Source.Scheme, "$.ruleset.source.scheme", errors);
        RequiredValue(artifact.Ruleset.Source.Value, "$.ruleset.source.value", errors);
        RelativePath(artifact.Ruleset.ManifestPath, "$.ruleset.manifestPath", errors);
        Hash(artifact.Ruleset.ManifestSha256, "$.ruleset.manifestSha256", errors);

        ValidateRuleSources(artifact.RuleSources, errors);
        ValidateSnippets(artifact.Snippets, artifact.RuleSources, errors);

        if (!string.Equals(artifact.Integrity.Algorithm, IntegrityAlgorithm, StringComparison.Ordinal))
        {
            Error(errors, "INTEGRITY_ALGORITHM_UNSUPPORTED", "$.integrity.algorithm", "Only SHA-256 is supported by artifact format 1.");
        }

        Hash(artifact.Integrity.ArtifactSha256, "$.integrity.artifactSha256", errors);
        if (IsSha256(artifact.Integrity.ArtifactSha256) &&
            errors.Count == 0 &&
            !CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(artifact.Integrity.ArtifactSha256),
                Convert.FromHexString(ComputeArtifactSha256(artifact))))
        {
            Error(errors, "ARTIFACT_HASH_MISMATCH", "$.integrity.artifactSha256", "The artifact integrity digest does not match its semantic content.");
        }
    }

    private static void ValidateRuleSources(
        IList<CompiledRulesArtifactRuleSource> sources,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (sources.Count == 0)
        {
            Error(errors, "RULE_SOURCES_REQUIRED", "$.ruleSources", "At least one Rule Source is required.");
            return;
        }

        if (sources.Any(source => source is null))
        {
            Error(errors, "STRUCTURE_REQUIRED", "$.ruleSources", "Rule Source entries cannot be null.");
            return;
        }

        StrictlyOrdered(sources.Select(source => source.RuleSourceId), "$.ruleSources", errors);
        var sourceIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < sources.Count; index++)
        {
            var source = sources[index];
            var path = $"$.ruleSources[{index}]";
            RequiredIdentifier(source.RuleSourceId, $"{path}.ruleSourceId", errors);
            if (!sourceIds.Add(source.RuleSourceId))
            {
                Error(errors, "DUPLICATE_RULE_SOURCE_ID", $"{path}.ruleSourceId", "Rule Source IDs must be unique.");
            }

            RelativePath(source.SourcePath, $"{path}.sourcePath", errors);
            Hash(source.SourceSha256, $"{path}.sourceSha256", errors);
            if (source.Applicability is null || source.DependencyRuleSourceIds is null)
            {
                Error(errors, "STRUCTURE_REQUIRED", path, "Rule Source applicability and dependencies are required.");
            }
            else
            {
                ValidateApplicability(source.Applicability, path, errors);
                StrictlyOrdered(source.DependencyRuleSourceIds, $"{path}.dependencyRuleSourceIds", errors);
            }
        }

        if (sources.Any(source => source.Applicability is null || source.DependencyRuleSourceIds is null))
        {
            return;
        }

        for (var index = 0; index < sources.Count; index++)
        {
            var source = sources[index];
            foreach (var dependency in source.DependencyRuleSourceIds)
            {
                if (string.Equals(source.RuleSourceId, dependency, StringComparison.Ordinal))
                {
                    Error(errors, "DEPENDENCY_SELF_REFERENCE", $"$.ruleSources[{index}].dependencyRuleSourceIds", "A Rule Source cannot depend on itself.");
                }
                else if (!sourceIds.Contains(dependency))
                {
                    Error(errors, "DEPENDENCY_TARGET_MISSING", $"$.ruleSources[{index}].dependencyRuleSourceIds", "A dependency target does not exist in the artifact.");
                }
            }
        }

        ValidateDependencyCycles(sources, errors);
    }

    private static void ValidateApplicability(
        CompiledRulesArtifactApplicability applicability,
        string sourcePath,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (applicability.WorldModelIds is null ||
            applicability.ModuleIds is null ||
            applicability.CampaignModes is null ||
            applicability.Operations is null ||
            applicability.Topics is null)
        {
            Error(errors, "STRUCTURE_REQUIRED", $"{sourcePath}.applicability", "Applicability selector arrays are required.");
            return;
        }

        if (!Enum.IsDefined(applicability.Layer))
        {
            Error(errors, "RULE_LAYER_UNSUPPORTED", $"{sourcePath}.applicability.layer", "The Rule Layer is not supported by artifact format 1.");
        }
        if (!Enum.IsDefined(applicability.PreparationTier))
        {
            Error(errors, "PREPARATION_TIER_UNSUPPORTED", $"{sourcePath}.applicability.preparationTier", "The preparation tier is not supported by artifact format 1.");
        }

        SelectorList(applicability.WorldModelIds, $"{sourcePath}.applicability.worldModelIds", errors);
        SelectorList(applicability.ModuleIds, $"{sourcePath}.applicability.moduleIds", errors);
        SelectorList(applicability.CampaignModes, $"{sourcePath}.applicability.campaignModes", errors);
        SelectorList(applicability.Operations, $"{sourcePath}.applicability.operations", errors);
        SelectorList(applicability.Topics, $"{sourcePath}.applicability.topics", errors);

        if (applicability.Layer == RuleLayer.World && applicability.WorldModelIds.Count == 0)
        {
            Error(errors, "WORLD_SELECTOR_REQUIRED", $"{sourcePath}.applicability.worldModelIds", "A World Rule Source must identify at least one World Model.");
        }

        if (applicability.Layer == RuleLayer.OptionalModule && applicability.ModuleIds.Count == 0)
        {
            Error(errors, "MODULE_SELECTOR_REQUIRED", $"{sourcePath}.applicability.moduleIds", "An Optional Module Rule Source must identify at least one module.");
        }

        if (applicability.Priority is < -10_000 or > 10_000)
        {
            Error(errors, "PRIORITY_OUT_OF_RANGE", $"{sourcePath}.applicability.priority", "Priority must be between -10000 and 10000.");
        }
    }

    private static void ValidateSnippets(
        IList<CompiledRulesArtifactSnippet> snippets,
        IList<CompiledRulesArtifactRuleSource> sources,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (snippets.Count == 0)
        {
            Error(errors, "SNIPPETS_REQUIRED", "$.snippets", "At least one compiled snippet is required.");
            return;
        }

        if (snippets.Any(snippet => snippet is null))
        {
            Error(errors, "STRUCTURE_REQUIRED", "$.snippets", "Snippet entries cannot be null.");
            return;
        }

        StrictlyOrdered(snippets.Select(snippet => snippet.SnippetId), "$.snippets", errors);
        var sourceIds = sources.Select(source => source.RuleSourceId).ToHashSet(StringComparer.Ordinal);
        var snippetIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < snippets.Count; index++)
        {
            var snippet = snippets[index];
            var path = $"$.snippets[{index}]";
            RequiredValue(snippet.SnippetId, $"{path}.snippetId", errors);
            RequiredIdentifier(snippet.RuleSourceId, $"{path}.ruleSourceId", errors);
            if (!snippetIds.Add(snippet.SnippetId))
            {
                Error(errors, "DUPLICATE_SNIPPET_ID", $"{path}.snippetId", "Snippet IDs must be unique.");
            }

            if (!sourceIds.Contains(snippet.RuleSourceId))
            {
                Error(errors, "SNIPPET_SOURCE_MISSING", $"{path}.ruleSourceId", "The snippet's Rule Source does not exist in the artifact.");
            }

            if (snippet.SourceAnchor is not null)
            {
                RequiredIdentifier(snippet.SourceAnchor, $"{path}.sourceAnchor", errors);
            }

            var expectedId = CreateSnippetId(snippet.RuleSourceId, snippet.SourceAnchor);
            if (!string.Equals(snippet.SnippetId, expectedId, StringComparison.Ordinal))
            {
                Error(errors, "SNIPPET_ID_MISMATCH", $"{path}.snippetId", "Snippet ID must be the Rule Source ID plus its optional source anchor.");
            }

            if (string.IsNullOrWhiteSpace(snippet.Content) ||
                snippet.Content.Contains('\r') ||
                snippet.Content.StartsWith('\n') ||
                snippet.Content.EndsWith('\n') ||
                snippet.Content.StartsWith('\uFEFF'))
            {
                Error(errors, "CONTENT_NOT_NORMALIZED", $"{path}.content", "Snippet content must be non-empty UTF-8 text with LF line endings, no BOM, and no leading or trailing newline.");
            }

            Hash(snippet.ContentSha256, $"{path}.contentSha256", errors);
            if (snippet.Content is not null &&
                IsSha256(snippet.ContentSha256) &&
                !string.Equals(snippet.ContentSha256, ComputeContentSha256(snippet.Content), StringComparison.Ordinal))
            {
                Error(errors, "CONTENT_HASH_MISMATCH", $"{path}.contentSha256", "The snippet content hash does not match its normalized content.");
            }

            if (snippet.EstimatedTokens <= 0)
            {
                Error(errors, "TOKEN_ESTIMATE_INVALID", $"{path}.estimatedTokens", "Estimated tokens must be positive.");
            }

            if (snippet.Retrieval is null)
            {
                Error(errors, "STRUCTURE_REQUIRED", $"{path}.retrieval", "Snippet retrieval metadata is required.");
            }
            else
            {
                ValidateRetrieval(snippet.Retrieval, path, errors);
            }
        }
    }

    private static void ValidateRetrieval(
        CompiledRulesRetrievalMetadata retrieval,
        string snippetPath,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (retrieval.Terms is null || retrieval.Relationships is null)
        {
            Error(errors, "STRUCTURE_REQUIRED", $"{snippetPath}.retrieval", "Retrieval term and relationship arrays are required.");
            return;
        }

        var orderedTerms = retrieval.Terms
            .Select(term => $"{term.Term}\0{term.Kind}")
            .ToArray();
        StrictlyOrdered(orderedTerms, $"{snippetPath}.retrieval.terms", errors);
        var termValues = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < retrieval.Terms.Count; index++)
        {
            var term = retrieval.Terms[index];
            var path = $"{snippetPath}.retrieval.terms[{index}]";
            NormalizedTerm(term.Term, $"{path}.term", errors);
            RequiredIdentifier(term.Kind, $"{path}.kind", errors);
            if (term.Weight is < 1 or > 1000)
            {
                Error(errors, "RETRIEVAL_WEIGHT_INVALID", $"{path}.weight", "Retrieval weight must be between 1 and 1000.");
            }
            termValues.Add(term.Term);
        }

        var orderedRelationships = retrieval.Relationships
            .Select(relationship => $"{relationship.FromTerm}\0{relationship.ToTerm}\0{relationship.Kind}")
            .ToArray();
        StrictlyOrdered(orderedRelationships, $"{snippetPath}.retrieval.relationships", errors);
        for (var index = 0; index < retrieval.Relationships.Count; index++)
        {
            var relationship = retrieval.Relationships[index];
            var path = $"{snippetPath}.retrieval.relationships[{index}]";
            NormalizedTerm(relationship.FromTerm, $"{path}.fromTerm", errors);
            NormalizedTerm(relationship.ToTerm, $"{path}.toTerm", errors);
            RequiredIdentifier(relationship.Kind, $"{path}.kind", errors);
            if (!termValues.Contains(relationship.FromTerm) || !termValues.Contains(relationship.ToTerm))
            {
                Error(errors, "RETRIEVAL_RELATION_TARGET_MISSING", path, "Retrieval relationships must reference terms declared by the same snippet.");
            }
            if (string.Equals(relationship.FromTerm, relationship.ToTerm, StringComparison.Ordinal))
            {
                Error(errors, "RETRIEVAL_RELATION_SELF_REFERENCE", path, "A retrieval relationship cannot point to the same term.");
            }
            if (relationship.Weight is < 1 or > 1000)
            {
                Error(errors, "RETRIEVAL_WEIGHT_INVALID", $"{path}.weight", "Retrieval weight must be between 1 and 1000.");
            }
        }
    }

    private static void ValidateDependencyCycles(
        IList<CompiledRulesArtifactRuleSource> sources,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        var byId = sources.ToDictionary(source => source.RuleSourceId, StringComparer.Ordinal);
        var states = new Dictionary<string, int>(StringComparer.Ordinal);

        bool Visit(string sourceId)
        {
            if (states.TryGetValue(sourceId, out var state))
            {
                return state == 1;
            }

            states[sourceId] = 1;
            foreach (var dependency in byId[sourceId].DependencyRuleSourceIds)
            {
                if (byId.ContainsKey(dependency) && Visit(dependency))
                {
                    return true;
                }
            }
            states[sourceId] = 2;
            return false;
        }

        foreach (var sourceId in byId.Keys)
        {
            if (Visit(sourceId))
            {
                Error(errors, "DEPENDENCY_CYCLE", "$.ruleSources", "The Rule Source dependency graph contains a cycle.");
                return;
            }
        }
    }

    private static void RequiredIdentifier(
        string value,
        string path,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 128 ||
            value != value.Normalize(NormalizationForm.FormC) ||
            !char.IsLetterOrDigit(value[0]) ||
            value.Any(character => char.IsControl(character) || char.IsWhiteSpace(character) || character is '#' or '/' or '\\' or ':'))
        {
            Error(errors, "IDENTIFIER_INVALID", path, "Identifiers must be NFC, start with a letter or digit, and contain no whitespace, control, path, fragment, or scheme separators.");
        }
    }

    private static void RequiredValue(
        string value,
        string path,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 512 || value.Any(char.IsControl))
        {
            Error(errors, "VALUE_INVALID", path, "The value is required, bounded, and cannot contain control characters.");
        }
    }

    private static void RelativePath(
        string value,
        string path,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        var segments = value.Split('/');
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 512 ||
            value.StartsWith('/') ||
            value.Contains('\\') ||
            value.Contains(':') ||
            segments.Any(segment => segment is "" or "." or "..") ||
            value.Any(char.IsControl))
        {
            Error(errors, "PATH_INVALID", path, "Paths must be normalized repository-relative paths using forward slashes without traversal.");
        }
    }

    private static void SelectorList(
        IList<string> values,
        string path,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        StrictlyOrdered(values, path, errors);
        for (var index = 0; index < values.Count; index++)
        {
            if (values[index] == "*")
            {
                continue;
            }
            RequiredIdentifier(values[index], $"{path}[{index}]", errors);
        }
    }

    private static void NormalizedTerm(
        string value,
        string path,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 256 ||
            value != value.Normalize(NormalizationForm.FormC) ||
            value != value.Trim() ||
            value.Contains("  ", StringComparison.Ordinal) ||
            value.Any(character => char.IsControl(character) || (char.IsWhiteSpace(character) && character != ' ')))
        {
            Error(errors, "RETRIEVAL_TERM_INVALID", path, "Retrieval terms must be NFC, trimmed, bounded text with internal whitespace represented by single spaces.");
        }
    }

    private static void StrictlyOrdered(
        IEnumerable<string> values,
        string path,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        string? previous = null;
        foreach (var value in values)
        {
            if (previous is not null && string.CompareOrdinal(previous, value) >= 0)
            {
                Error(errors, "CANONICAL_ORDER_INVALID", path, "Set-like arrays must be unique and strictly ordered by ordinal comparison.");
                return;
            }
            previous = value;
        }
    }

    private static void Hash(
        string value,
        string path,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (!IsSha256(value))
        {
            Error(errors, "SHA256_INVALID", path, "SHA-256 values must use 64 uppercase hexadecimal characters.");
        }
    }

    private static bool IsSha256(string? value) =>
        value is { Length: 64 } && value.All(character => character is >= '0' and <= '9' or >= 'A' and <= 'F');

    private static void FindDuplicateProperties(
        JsonElement element,
        string path,
        ICollection<CompiledRulesArtifactValidationError> errors)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                var propertyPath = $"{path}.{property.Name}";
                if (!names.Add(property.Name))
                {
                    Error(errors, "DUPLICATE_JSON_PROPERTY", propertyPath, "JSON object property names must be unique.");
                }
                FindDuplicateProperties(property.Value, propertyPath, errors);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                FindDuplicateProperties(item, $"{path}[{index++}]", errors);
            }
        }
    }

    private static void Error(
        ICollection<CompiledRulesArtifactValidationError> errors,
        string code,
        string path,
        string message) =>
        errors.Add(new(code, path, message));

    private sealed class DigestWriter(IncrementalHash hash)
    {
        public void Value(bool value) => Value(value ? "true" : "false");

        public void Value(int value) => Value(value.ToString(CultureInfo.InvariantCulture));

        public void Values(IList<string> values)
        {
            Value(values.Count);
            foreach (var value in values)
            {
                Value(value);
            }
        }

        public void Value(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            hash.AppendData(Encoding.ASCII.GetBytes(bytes.Length.ToString(CultureInfo.InvariantCulture)));
            hash.AppendData(":"u8);
            hash.AppendData(bytes);
        }
    }
}
