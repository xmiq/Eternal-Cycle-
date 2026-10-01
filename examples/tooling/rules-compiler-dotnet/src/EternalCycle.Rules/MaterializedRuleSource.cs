using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EternalCycle.Rules;

public sealed record MaterializedRuleSourceRequest(
    string SourceRoot,
    string ManifestPath,
    CompiledRulesSourceIdentity SourceIdentity,
    CompiledRulesCompilerIdentity CompilerIdentity);

public sealed record MaterializedRuleSourceDocument(
    RuleSourceManifestEntry ManifestEntry,
    ReadOnlyMemory<byte> AuthoritativeBytes,
    string DecodedText,
    string SourceSha256);

public sealed record MaterializedRuleSourceSnapshot(
    string ManifestPath,
    ReadOnlyMemory<byte> AuthoritativeManifestBytes,
    string ManifestSha256,
    RuleSourceManifest Manifest,
    CompiledRulesSourceIdentity SourceIdentity,
    CompiledRulesCompilerIdentity CompilerIdentity,
    IReadOnlyList<MaterializedRuleSourceDocument> Sources);

public sealed class MaterializedRuleSourceException : Exception
{
    public MaterializedRuleSourceException(
        string code,
        string path,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        Path = path;
    }

    public string Code { get; }

    public string Path { get; }
}

public static class MaterializedRuleSourceLoader
{
    public const int CurrentManifestFormatVersion = 1;

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    private static readonly string[] RequiredManifestProperties =
    [
        "manifestFormatVersion",
        "compilerContractVersion",
        "rulesetId",
        "repositoryVersion",
        "sources"
    ];

    private static readonly string[] RequiredSourceProperties =
    [
        "ruleSourceId",
        "path",
        "layer",
        "worldModelIds",
        "moduleIds",
        "campaignModes",
        "operations",
        "topics",
        "priority",
        "alwaysInclude",
        "preparationTier"
    ];

    public static MaterializedRuleSourceSnapshot Load(MaterializedRuleSourceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateIdentity(request.SourceIdentity, request.CompilerIdentity);

        var sourceRoot = ResolveSourceRoot(request.SourceRoot);
        var manifestPath = NormalizeRelativePath(request.ManifestPath, "$.manifestPath");
        var manifestBytes = ReadAuthorizedFile(sourceRoot, manifestPath, "MANIFEST");
        var manifestText = DecodeUtf8(manifestBytes, "$.manifestPath");
        var manifest = ParseManifest(manifestText);
        ValidateManifest(manifest, request.CompilerIdentity);

        var sourceIds = new HashSet<string>(StringComparer.Ordinal);
        // Case-insensitive comparison prevents one portable payload from naming the same
        // filesystem object twice when moved onto a case-insensitive platform.
        var sourcePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedEntries = new List<RuleSourceManifestEntry>(manifest.Sources.Count);
        for (var index = 0; index < manifest.Sources.Count; index++)
        {
            var entry = manifest.Sources[index];
            var path = $"$.sources[{index}]";
            ValidateManifestEntry(entry, path);
            var normalizedPath = NormalizeRelativePath(entry.Path, $"{path}.path");
            if (!sourceIds.Add(entry.RuleSourceId))
            {
                Fail("RULE_SOURCE_ID_DUPLICATE", $"{path}.ruleSourceId", "Rule Source IDs must be unique.");
            }
            if (!sourcePaths.Add(normalizedPath))
            {
                Fail("SOURCE_PATH_DUPLICATE", $"{path}.path", "Normalized Rule Source paths must be unique.");
            }

            normalizedEntries.Add(CloneEntry(entry, normalizedPath));
        }

        ValidateDependencies(normalizedEntries, sourceIds);
        var vocabulary = manifest.RetrievalVocabulary is null
            ? null
            : RuleRetrievalVocabulary.NormalizeAndValidate(manifest.RetrievalVocabulary, sourceIds);

        var sources = new List<MaterializedRuleSourceDocument>(normalizedEntries.Count);
        foreach (var entry in normalizedEntries)
        {
            var bytes = ReadAuthorizedFile(sourceRoot, entry.Path, "SOURCE");
            sources.Add(new(
                entry,
                bytes,
                DecodeUtf8(bytes, entry.Path),
                ComputeSha256(bytes)));
        }

        var snapshotManifest = new RuleSourceManifest
        {
            ManifestFormatVersion = manifest.ManifestFormatVersion,
            CompilerContractVersion = manifest.CompilerContractVersion,
            RulesetId = manifest.RulesetId,
            RepositoryVersion = manifest.RepositoryVersion,
            Sources = normalizedEntries,
            RetrievalVocabulary = vocabulary
        };

        return new(
            manifestPath,
            manifestBytes,
            ComputeSha256(manifestBytes),
            snapshotManifest,
            CloneSourceIdentity(request.SourceIdentity),
            CloneCompilerIdentity(request.CompilerIdentity),
            sources);
    }

    public static string ComputeSha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

    private static RuleSourceManifest ParseManifest(string manifestText)
    {
        try
        {
            using var document = JsonDocument.Parse(manifestText, new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 64
            });
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                Fail("MANIFEST_JSON_INVALID", "$", "The Rule Source manifest must be a JSON object.");
            }

            RejectDuplicateProperties(document.RootElement, "$");
            RequireProperties(document.RootElement, "$", RequiredManifestProperties);
            if (document.RootElement.TryGetProperty("retrievalVocabulary", out var vocabulary) &&
                vocabulary.ValueKind != JsonValueKind.Object)
            {
                Fail("VOCABULARY_STRUCTURE_INVALID", "$.retrievalVocabulary", "An explicit vocabulary declaration must be an object; omit it for legacy payloads.");
            }
            if (!document.RootElement.TryGetProperty("sources", out var sources) ||
                sources.ValueKind != JsonValueKind.Array)
            {
                Fail("MANIFEST_STRUCTURE_INVALID", "$.sources", "The sources property must be an array.");
            }
            for (var index = 0; index < sources.GetArrayLength(); index++)
            {
                var source = sources[index];
                if (source.ValueKind != JsonValueKind.Object)
                {
                    Fail("MANIFEST_STRUCTURE_INVALID", $"$.sources[{index}]", "Each Rule Source declaration must be an object.");
                }
                RequireProperties(source, $"$.sources[{index}]", RequiredSourceProperties);
            }

            return JsonSerializer.Deserialize<RuleSourceManifest>(manifestText, JsonOptions)
                ?? throw new JsonException("The manifest did not produce an object.");
        }
        catch (MaterializedRuleSourceException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new MaterializedRuleSourceException(
                "MANIFEST_JSON_INVALID",
                exception.Path ?? "$",
                "The Rule Source manifest is not valid contract JSON.",
                exception);
        }
    }

    private static void ValidateIdentity(
        CompiledRulesSourceIdentity sourceIdentity,
        CompiledRulesCompilerIdentity compilerIdentity)
    {
        ArgumentNullException.ThrowIfNull(sourceIdentity);
        ArgumentNullException.ThrowIfNull(compilerIdentity);
        RequiredIdentifier(sourceIdentity.Scheme, "$.sourceIdentity.scheme");
        RequiredValue(sourceIdentity.Value, "$.sourceIdentity.value");
        if (!string.Equals(
                compilerIdentity.ContractVersion,
                CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                StringComparison.Ordinal))
        {
            Fail("COMPILER_CONTRACT_UNSUPPORTED", "$.compilerIdentity.contractVersion", "The compiler contract version is not supported.");
        }
        RequiredIdentifier(compilerIdentity.ImplementationId, "$.compilerIdentity.implementationId");
        RequiredValue(compilerIdentity.ImplementationVersion, "$.compilerIdentity.implementationVersion");
    }

    private static void ValidateManifest(
        RuleSourceManifest manifest,
        CompiledRulesCompilerIdentity compilerIdentity)
    {
        if (manifest.ManifestFormatVersion != CurrentManifestFormatVersion)
        {
            Fail("MANIFEST_FORMAT_UNSUPPORTED", "$.manifestFormatVersion", "Only Rule Source manifest format 1 is supported.");
        }
        if (!string.Equals(
                manifest.CompilerContractVersion,
                CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                StringComparison.Ordinal) ||
            !string.Equals(
                manifest.CompilerContractVersion,
                compilerIdentity.ContractVersion,
                StringComparison.Ordinal))
        {
            Fail("COMPILER_CONTRACT_UNSUPPORTED", "$.compilerContractVersion", "The manifest compiler contract version is not supported by the declared compiler.");
        }

        RequiredIdentifier(manifest.RulesetId, "$.rulesetId");
        RequiredValue(manifest.RepositoryVersion, "$.repositoryVersion");
        if (manifest.Sources is null || manifest.Sources.Count == 0)
        {
            Fail("MANIFEST_SOURCES_REQUIRED", "$.sources", "The manifest must declare at least one Rule Source.");
        }
    }

    private static void ValidateManifestEntry(RuleSourceManifestEntry entry, string path)
    {
        if (entry is null)
        {
            Fail("MANIFEST_STRUCTURE_INVALID", path, "A Rule Source declaration cannot be null.");
        }

        RequiredIdentifier(entry.RuleSourceId, $"{path}.ruleSourceId");
        if (!Enum.IsDefined(entry.Layer))
        {
            Fail("RULE_LAYER_INVALID", $"{path}.layer", "The Rule Source layer is not supported.");
        }
        if (!Enum.IsDefined(entry.PreparationTier))
        {
            Fail("PREPARATION_TIER_INVALID", $"{path}.preparationTier", "The preparation tier is not supported.");
        }
        if (entry.Priority is < -10_000 or > 10_000)
        {
            Fail("SOURCE_PRIORITY_INVALID", $"{path}.priority", "Priority must be between -10000 and 10000.");
        }

        SelectorList(entry.WorldModelIds, $"{path}.worldModelIds");
        SelectorList(entry.ModuleIds, $"{path}.moduleIds");
        SelectorList(entry.CampaignModes, $"{path}.campaignModes");
        SelectorList(entry.Operations, $"{path}.operations");
        SelectorList(entry.Topics, $"{path}.topics");
        IdentifierList(entry.Dependencies, $"{path}.dependencies");
        if (entry.Layer == RuleLayer.World && entry.WorldModelIds.Count == 0)
        {
            Fail("WORLD_MODEL_REQUIRED", $"{path}.worldModelIds", "World Rule Sources must identify at least one World Model.");
        }
        if (entry.Layer == RuleLayer.OptionalModule && entry.ModuleIds.Count == 0)
        {
            Fail("MODULE_REQUIRED", $"{path}.moduleIds", "Optional-module Rule Sources must identify at least one module.");
        }
    }

    private static void ValidateDependencies(
        IReadOnlyList<RuleSourceManifestEntry> entries,
        IReadOnlySet<string> sourceIds)
    {
        var byId = entries.ToDictionary(entry => entry.RuleSourceId, StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            foreach (var dependency in entry.Dependencies)
            {
                if (!sourceIds.Contains(dependency))
                {
                    Fail("DEPENDENCY_MISSING", entry.RuleSourceId, $"Rule Source '{entry.RuleSourceId}' depends on missing Rule Source '{dependency}'.");
                }
                if (string.Equals(entry.RuleSourceId, dependency, StringComparison.Ordinal))
                {
                    Fail("DEPENDENCY_CYCLE", entry.RuleSourceId, $"Rule Source '{entry.RuleSourceId}' cannot depend on itself.");
                }
            }
        }

        var state = new Dictionary<string, byte>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            Visit(entry.RuleSourceId);
        }

        return;

        void Visit(string sourceId)
        {
            if (state.TryGetValue(sourceId, out var currentState))
            {
                if (currentState == 1)
                {
                    Fail("DEPENDENCY_CYCLE", sourceId, "Rule Source dependencies must be acyclic.");
                }
                return;
            }

            state[sourceId] = 1;
            foreach (var dependency in byId[sourceId].Dependencies)
            {
                Visit(dependency);
            }
            state[sourceId] = 2;
        }
    }

    private static byte[] ReadAuthorizedFile(string sourceRoot, string normalizedPath, string kind)
    {
        var segments = normalizedPath.Split('/');
        var current = sourceRoot;
        for (var index = 0; index < segments.Length; index++)
        {
            current = Path.Combine(current, segments[index]);
            var fullPath = Path.GetFullPath(current);
            EnsureContained(sourceRoot, fullPath, normalizedPath);
            var isLast = index == segments.Length - 1;
            if (!isLast)
            {
                if (!Directory.Exists(fullPath))
                {
                    Fail($"{kind}_NOT_FOUND", normalizedPath, $"Required {kind.ToLowerInvariant()} path '{normalizedPath}' does not exist.");
                }
                RejectReparsePoint(new DirectoryInfo(fullPath), normalizedPath);
                continue;
            }

            if (Directory.Exists(fullPath))
            {
                Fail($"{kind}_NOT_FILE", normalizedPath, $"Required {kind.ToLowerInvariant()} path '{normalizedPath}' is not an ordinary file.");
            }
            var file = new FileInfo(fullPath);
            if (!file.Exists)
            {
                Fail($"{kind}_NOT_FOUND", normalizedPath, $"Required {kind.ToLowerInvariant()} file '{normalizedPath}' does not exist.");
            }
            RejectReparsePoint(file, normalizedPath);
            return File.ReadAllBytes(fullPath);
        }

        throw new InvalidOperationException("An authorized path must contain at least one segment.");
    }

    private static string ResolveSourceRoot(string sourceRoot)
    {
        if (string.IsNullOrWhiteSpace(sourceRoot))
        {
            Fail("SOURCE_ROOT_INVALID", "$.sourceRoot", "A materialized Rule Source root is required.");
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(sourceRoot);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new MaterializedRuleSourceException("SOURCE_ROOT_INVALID", "$.sourceRoot", "The materialized Rule Source root is invalid.", exception);
        }

        var directory = new DirectoryInfo(fullPath);
        if (!directory.Exists)
        {
            Fail("SOURCE_ROOT_NOT_FOUND", "$.sourceRoot", "The materialized Rule Source root does not exist.");
        }
        RejectReparsePoint(directory, "$.sourceRoot");
        return directory.FullName;
    }

    private static string NormalizeRelativePath(string value, string path)
    {
        var segments = value?.Split('/') ?? [];
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 512 ||
            Path.IsPathRooted(value) ||
            Path.IsPathFullyQualified(value) ||
            value.StartsWith('/') ||
            value.Contains('\\') ||
            value.Contains(':') ||
            value.Any(character => char.IsControl(character) || Path.GetInvalidPathChars().Contains(character)) ||
            segments.Any(segment => segment is "" or "." or ".."))
        {
            Fail("SOURCE_PATH_INVALID", path, "Paths must be normalized source-root-relative paths using forward slashes without traversal.");
        }
        return string.Join('/', segments);
    }

    private static void EnsureContained(string sourceRoot, string candidate, string logicalPath)
    {
        var relative = Path.GetRelativePath(sourceRoot, candidate);
        if (Path.IsPathRooted(relative) ||
            relative == ".." ||
            relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
            relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal))
        {
            Fail("SOURCE_ROOT_ESCAPE", logicalPath, "The declared path escapes the materialized Rule Source root.");
        }
    }

    private static void RejectReparsePoint(FileSystemInfo item, string logicalPath)
    {
        item.Refresh();
        if ((item.Attributes & FileAttributes.ReparsePoint) != 0)
        {
            Fail("FILESYSTEM_INDIRECTION_REJECTED", logicalPath, "Filesystem indirection is not accepted in a materialized Rule Source path.");
        }
    }

    private static string DecodeUtf8(ReadOnlySpan<byte> bytes, string path)
    {
        try
        {
            return StrictUtf8.GetString(bytes);
        }
        catch (DecoderFallbackException exception)
        {
            throw new MaterializedRuleSourceException("SOURCE_ENCODING_INVALID", path, "Rule Source text must be valid UTF-8.", exception);
        }
    }

    private static void RejectDuplicateProperties(JsonElement element, string path)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var property in element.EnumerateObject())
                {
                    if (!names.Add(property.Name))
                    {
                        Fail("JSON_PROPERTY_DUPLICATE", $"{path}.{property.Name}", "JSON object properties must be unique.");
                    }
                    RejectDuplicateProperties(property.Value, $"{path}.{property.Name}");
                }
                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    RejectDuplicateProperties(item, $"{path}[{index++}]");
                }
                break;
        }
    }

    private static void RequireProperties(JsonElement element, string path, IEnumerable<string> requiredProperties)
    {
        foreach (var property in requiredProperties)
        {
            if (!element.TryGetProperty(property, out _))
            {
                Fail("MANIFEST_PROPERTY_REQUIRED", $"{path}.{property}", "A required manifest property is missing.");
            }
        }
    }

    internal static void RequiredIdentifier(string value, string path)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 128 ||
            value != value.Normalize(NormalizationForm.FormC) ||
            !char.IsLetterOrDigit(value[0]) ||
            value.Any(character => char.IsControl(character) || char.IsWhiteSpace(character) || character is '#' or '/' or '\\' or ':'))
        {
            Fail("IDENTIFIER_INVALID", path, "Identifiers must be NFC, bounded, start with a letter or digit, and contain no whitespace or separators.");
        }
    }

    private static void RequiredValue(string value, string path)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 512 || value.Any(char.IsControl))
        {
            Fail("VALUE_INVALID", path, "The value is required, bounded, and cannot contain control characters.");
        }
    }

    private static void SelectorList(IList<string>? values, string path)
    {
        if (values is null)
        {
            Fail("SELECTOR_LIST_INVALID", path, "Selector lists cannot be null.");
        }

        var unique = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < values.Count; index++)
        {
            var value = values[index];
            if (!unique.Add(value))
            {
                Fail("SELECTOR_DUPLICATE", $"{path}[{index}]", "Selector values must be unique.");
            }
            if (value != "*")
            {
                RequiredIdentifier(value, $"{path}[{index}]");
            }
        }
    }

    private static void IdentifierList(IList<string>? values, string path)
    {
        if (values is null)
        {
            Fail("IDENTIFIER_LIST_INVALID", path, "Identifier lists cannot be null.");
        }

        var unique = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < values.Count; index++)
        {
            RequiredIdentifier(values[index], $"{path}[{index}]");
            if (!unique.Add(values[index]))
            {
                Fail("IDENTIFIER_DUPLICATE", $"{path}[{index}]", "Identifier-list values must be unique.");
            }
        }
    }

    private static RuleSourceManifestEntry CloneEntry(RuleSourceManifestEntry entry, string normalizedPath) => new()
    {
        RuleSourceId = entry.RuleSourceId,
        Path = normalizedPath,
        Layer = entry.Layer,
        WorldModelIds = [.. entry.WorldModelIds],
        ModuleIds = [.. entry.ModuleIds],
        CampaignModes = [.. entry.CampaignModes],
        Operations = [.. entry.Operations],
        Topics = [.. entry.Topics],
        Dependencies = [.. entry.Dependencies],
        Priority = entry.Priority,
        AlwaysInclude = entry.AlwaysInclude,
        PreparationTier = entry.PreparationTier
    };

    private static CompiledRulesSourceIdentity CloneSourceIdentity(CompiledRulesSourceIdentity identity) => new()
    {
        Scheme = identity.Scheme,
        Value = identity.Value
    };

    private static CompiledRulesCompilerIdentity CloneCompilerIdentity(CompiledRulesCompilerIdentity identity) => new()
    {
        ContractVersion = identity.ContractVersion,
        ImplementationId = identity.ImplementationId,
        ImplementationVersion = identity.ImplementationVersion
    };

    [DoesNotReturn]
    private static void Fail(string code, string path, string message) =>
        throw new MaterializedRuleSourceException(code, path, message);
}
