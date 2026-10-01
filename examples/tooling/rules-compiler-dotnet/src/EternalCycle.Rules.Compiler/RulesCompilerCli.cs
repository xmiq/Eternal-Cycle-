using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using EternalCycle.Rules;

namespace EternalCycle.Rules.Compiler;

public enum RulesCompilerExitCode
{
    Success = 0,
    Usage = 2,
    MaterializedInput = 3,
    SnippetCompilation = 4,
    ArtifactValidation = 5,
    Output = 6,
    Internal = 70
}

public static class RulesCompilerCli
{
    private const int MaximumReportedErrors = 20;

    private static readonly string[] RequiredOptions =
    [
        "--source-root",
        "--manifest",
        "--source-scheme",
        "--source-value",
        "--compiler-id",
        "--compiler-version",
        "--output"
    ];

    public static async Task<int> RunAsync(
        IReadOnlyList<string> arguments,
        TextWriter standardOutput,
        TextWriter standardError,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(standardOutput);
        ArgumentNullException.ThrowIfNull(standardError);

        if (arguments.Count == 1 && IsHelp(arguments[0]))
        {
            await standardOutput.WriteAsync(HelpText.AsMemory(), cancellationToken);
            return (int)RulesCompilerExitCode.Success;
        }

        if (arguments.Count == 0 || arguments[0] is not ("compile" or "audit"))
        {
            await WriteUsageErrorAsync(standardError, "The required command is 'compile' or 'audit'.", cancellationToken);
            return (int)RulesCompilerExitCode.Usage;
        }

        if (arguments.Count == 2 && IsHelp(arguments[1]))
        {
            await standardOutput.WriteAsync(HelpText.AsMemory(), cancellationToken);
            return (int)RulesCompilerExitCode.Success;
        }

        var parse = ParseOptions(arguments);
        if (parse.Error is not null)
        {
            await WriteUsageErrorAsync(standardError, parse.Error, cancellationToken);
            return (int)RulesCompilerExitCode.Usage;
        }

        var options = parse.Options!;
        try
        {
            RuleVocabularyAuditPolicy? auditPolicy = null;
            if (options.AuditPolicyPath is not null)
            {
                try { auditPolicy = ReadAuditPolicy(options.AuditPolicyPath); }
                catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
                {
                    await standardError.WriteLineAsync("Audit input failed [AUDIT_POLICY_INVALID]: Supply a closed policy object with valid thresholds and bounded generic terms.".AsMemory(), cancellationToken);
                    return (int)RulesCompilerExitCode.MaterializedInput;
                }
            }
            MaterializedRuleSourceSnapshot snapshot;
            try
            {
                snapshot = MaterializedRuleSourceLoader.Load(new(
                    options.SourceRoot,
                    options.ManifestPath,
                    new CompiledRulesSourceIdentity
                    {
                        Scheme = options.SourceScheme,
                        Value = options.SourceValue
                    },
                    new CompiledRulesCompilerIdentity
                    {
                        ContractVersion = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                        ImplementationId = options.CompilerId,
                        ImplementationVersion = options.CompilerVersion
                    }));
            }
            catch (MaterializedRuleSourceException exception)
            {
                await WriteFailureAsync(
                    standardError,
                    "Materialized Rule Source input failed",
                    exception.Code,
                    exception.Path,
                    exception.Message,
                    cancellationToken);
                return (int)RulesCompilerExitCode.MaterializedInput;
            }

            RuleCompilationResult compilation;
            try
            {
                compilation = RuleCompilationPipeline.Compile(snapshot);
            }
            catch (RuleSnippetCompilationException exception)
            {
                await WriteFailureAsync(
                    standardError,
                    "Rule snippet compilation failed",
                    exception.Code,
                    exception.RuleSourceId,
                    exception.Message,
                    cancellationToken);
                return (int)RulesCompilerExitCode.SnippetCompilation;
            }
            catch (RuleVocabularyEnrichmentException exception)
            {
                await WriteFailureAsync(
                    standardError,
                    "Reviewed vocabulary enrichment failed",
                    exception.Code,
                    exception.Path,
                    exception.Message,
                    cancellationToken);
                return (int)RulesCompilerExitCode.SnippetCompilation;
            }
            if (!compilation.IsValid)
            {
                await WriteValidationErrorsAsync(standardError, "Artifact compilation failed", compilation.Errors, cancellationToken);
                return (int)RulesCompilerExitCode.ArtifactValidation;
            }

            var artifact = compilation.Artifact!;
            RuleVocabularyAuditReport? audit = null;
            var bytes = compilation.Bytes;
            if (options.Audit)
            {
                audit = RuleVocabularyAuditor.Analyze(snapshot, compilation, auditPolicy);
                if (!audit.IsValid)
                {
                    await WriteValidationErrorsAsync(standardError, "Vocabulary audit failed",
                        audit.Diagnostics.Where(item => item.Severity == RuleVocabularyAuditSeverity.Error)
                            .Select(item => new CompiledRulesArtifactValidationError(item.Code, item.SubjectId, item.Message)).ToArray(), cancellationToken);
                    return (int)RulesCompilerExitCode.ArtifactValidation;
                }
                bytes = RuleVocabularyAuditWriter.Write(audit);
            }
            string outputPath;
            try
            {
                outputPath = await WriteOutputAsync(options, snapshot, bytes, cancellationToken);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or PathTooLongException)
            {
                var detail = exception.Message;
                if (options.Audit && detail.Length > 512) { detail = detail[..512]; }
                await standardError.WriteLineAsync($"Output failed [OUTPUT_WRITE_FAILED]: {detail}".AsMemory(), cancellationToken);
                return (int)RulesCompilerExitCode.Output;
            }

            var serializedHash = Convert.ToHexString(SHA256.HashData(bytes.Span));
            await standardOutput.WriteLineAsync((options.Audit ? "Vocabulary audit written." : "Compiled Rules artifact written.").AsMemory(), cancellationToken);
            await standardOutput.WriteLineAsync($"Output: {outputPath}".AsMemory(), cancellationToken);
            await standardOutput.WriteLineAsync($"Rule Sources: {artifact.RuleSources.Count}".AsMemory(), cancellationToken);
            await standardOutput.WriteLineAsync($"Snippets: {artifact.Snippets.Count}".AsMemory(), cancellationToken);
            await standardOutput.WriteLineAsync($"Semantic digest: {artifact.Integrity.ArtifactSha256}".AsMemory(), cancellationToken);
            if (audit is not null)
            {
                await standardOutput.WriteLineAsync($"Audit findings: {audit.Summary.ErrorCount} errors, {audit.Summary.WarningCount} warnings, {audit.Summary.InformationCount} information.".AsMemory(), cancellationToken);
            }
            await standardOutput.WriteLineAsync($"Serialized {(options.Audit ? "audit" : "artifact")} byte SHA-256: {serializedHash}".AsMemory(), cancellationToken);
            return (int)RulesCompilerExitCode.Success;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await standardError.WriteLineAsync("Compilation cancelled [CANCELLED].".AsMemory(), CancellationToken.None);
            return (int)RulesCompilerExitCode.Internal;
        }
        catch (Exception exception)
        {
            await standardError.WriteLineAsync($"Compilation failed [INTERNAL_ERROR]: {exception.Message}".AsMemory(), cancellationToken);
            return (int)RulesCompilerExitCode.Internal;
        }
    }

    private static ParseResult ParseOptions(IReadOnlyList<string> arguments)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 1; index < arguments.Count; index += 2)
        {
            var option = arguments[index];
            if (!RequiredOptions.Contains(option, StringComparer.Ordinal) && !(arguments[0] == "audit" && option == "--audit-policy"))
            {
                return new(null, $"Unknown argument '{option}'.");
            }
            if (index + 1 >= arguments.Count || arguments[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                return new(null, $"Option '{option}' requires a value.");
            }
            if (!values.TryAdd(option, arguments[index + 1]))
            {
                return new(null, $"Option '{option}' may be supplied only once.");
            }
        }

        var missing = RequiredOptions.Where(option => !values.ContainsKey(option)).ToArray();
        if (missing.Length != 0)
        {
            return new(null, $"Missing required option(s): {string.Join(", ", missing)}.");
        }

        return new(new(
            values["--source-root"],
            values["--manifest"],
            values["--source-scheme"],
            values["--source-value"],
            values["--compiler-id"],
            values["--compiler-version"],
            values["--output"], arguments[0] == "audit", values.GetValueOrDefault("--audit-policy")), null);
    }

    private static RuleVocabularyAuditPolicy ReadAuditPolicy(string path)
    {
        if (new FileInfo(path).Length > 1024 * 1024) { throw new ArgumentException("Audit policy exceeds the input limit."); }
        var bytes = File.ReadAllBytes(path);
        using var json = JsonDocument.Parse(bytes);
        if (json.RootElement.ValueKind != JsonValueKind.Object) { throw new JsonException("A policy object is required."); }
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in json.RootElement.EnumerateObject())
        {
            if (!names.Add(property.Name)) { throw new JsonException("Duplicate policy property."); }
        }
        var policy = JsonSerializer.Deserialize<RuleVocabularyAuditPolicy>(bytes, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        }) ?? throw new JsonException("A policy object is required.");
        return policy.Normalize();
    }

    private static async Task<string> WriteOutputAsync(
        CompilerOptions options,
        MaterializedRuleSourceSnapshot snapshot,
        ReadOnlyMemory<byte> bytes,
        CancellationToken cancellationToken)
    {
        var outputPath = Path.GetFullPath(options.OutputPath);
        if (Directory.Exists(outputPath))
        {
            throw new IOException("The requested output path is a directory.");
        }

        var sourceRoot = Path.GetFullPath(options.SourceRoot);
        var protectedPaths = snapshot.Sources
            .Select(source => Path.GetFullPath(Path.Combine(
                sourceRoot,
                source.ManifestEntry.Path.Replace('/', Path.DirectorySeparatorChar))))
            .Append(Path.GetFullPath(Path.Combine(
                sourceRoot,
                snapshot.ManifestPath.Replace('/', Path.DirectorySeparatorChar))));
        if (protectedPaths.Any(path => PathsEqual(path, outputPath)))
        {
            throw new IOException("The output path cannot replace the Rule Source manifest or a declared Rule Source file.");
        }
        if (options.AuditPolicyPath is not null && PathsEqual(Path.GetFullPath(options.AuditPolicyPath), outputPath))
        {
            throw new IOException("The output path cannot replace the audit policy.");
        }
        if (File.Exists(outputPath))
        {
            // Separate commands emit separate formats. Refuse cross-format replacement
            // even when both requested paths have an ordinary .json extension.
            using var stream = File.OpenRead(outputPath);
            try
            {
                using var existing = JsonDocument.Parse(stream);
                if (existing.RootElement.ValueKind == JsonValueKind.Object &&
                    existing.RootElement.TryGetProperty(options.Audit ? "artifactFormatVersion" : "auditFormatVersion", out _))
                {
                    throw new IOException("Artifact and audit outputs cannot replace one another.");
                }
                if (options.Audit && (existing.RootElement.ValueKind != JsonValueKind.Object ||
                    !existing.RootElement.TryGetProperty("auditFormatVersion", out var version) ||
                    version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out var number) || number != 1))
                {
                    throw new IOException("An audit output may replace only an existing format-1 audit report.");
                }
            }
            catch (JsonException) when (!options.Audit) { /* Preserve legacy replacement of non-JSON artifact output. */ }
            catch (JsonException) { throw new IOException("An audit output cannot replace an unrecognized existing file."); }
        }

        var parent = Path.GetDirectoryName(outputPath)
            ?? throw new IOException("The output path has no writable parent directory.");
        Directory.CreateDirectory(parent);
        var temporaryPath = Path.Combine(parent, $".{Path.GetFileName(outputPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, bytes, cancellationToken);
            File.Move(temporaryPath, outputPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
        return outputPath;
    }

    private static bool PathsEqual(string left, string right) =>
        string.Equals(
            left,
            right,
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

    private static bool IsHelp(string argument) => argument is "--help" or "-h";

    private static async Task WriteUsageErrorAsync(
        TextWriter error,
        string message,
        CancellationToken cancellationToken)
    {
        await error.WriteLineAsync($"Usage error: {message}".AsMemory(), cancellationToken);
        await error.WriteLineAsync("Run with --help for usage.".AsMemory(), cancellationToken);
    }

    private static async Task WriteFailureAsync(
        TextWriter error,
        string category,
        string code,
        string path,
        string message,
        CancellationToken cancellationToken) =>
        await error.WriteLineAsync($"{category} [{code}] {path}: {message}".AsMemory(), cancellationToken);

    private static async Task WriteValidationErrorsAsync(
        TextWriter error,
        string category,
        IReadOnlyList<CompiledRulesArtifactValidationError> errors,
        CancellationToken cancellationToken)
    {
        await error.WriteLineAsync($"{category} ({errors.Count} error(s)):".AsMemory(), cancellationToken);
        foreach (var validationError in errors.Take(MaximumReportedErrors))
        {
            await error.WriteLineAsync($"- [{validationError.Code}] {validationError.Path}: {validationError.Message}".AsMemory(), cancellationToken);
        }
        if (errors.Count > MaximumReportedErrors)
        {
            await error.WriteLineAsync($"- {errors.Count - MaximumReportedErrors} additional error(s) omitted.".AsMemory(), cancellationToken);
        }
    }

    private const string HelpText = """
Eternal Cycle standalone rules compiler

Usage:
  EternalCycle.Rules.Compiler compile [options]
  EternalCycle.Rules.Compiler audit [options] [--audit-policy <path>]

Required options:
  --source-root <path>       Root of the already-materialized Rule Source payload.
  --manifest <path>          Normalized source-root-relative manifest path.
  --source-scheme <id>       Immutable source identity scheme.
  --source-value <value>     Immutable source identity value.
  --compiler-id <id>         Compiler implementation identifier.
  --compiler-version <value> Compiler implementation version.
  --output <path>            Artifact or separate audit report output file.

Commands:
  compile                    Compile the materialized payload to format-1 JSON.
  audit                      Compile in memory and write observational audit JSON only.

Options:
  -h, --help                 Show this help.
  --audit-policy <path>      Audit-only closed JSON thresholds/generic-term policy.

The compiler uses local ordinary files only. It performs no acquisition, import,
network, Git, MCP, SQL, campaign, publication, or runtime operation.
""";

    private sealed record CompilerOptions(
        string SourceRoot,
        string ManifestPath,
        string SourceScheme,
        string SourceValue,
        string CompilerId,
        string CompilerVersion,
        string OutputPath,
        bool Audit,
        string? AuditPolicyPath);

    private sealed record ParseResult(CompilerOptions? Options, string? Error);
}
