using System.Security.Cryptography;
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

        if (arguments.Count == 0 || !string.Equals(arguments[0], "compile", StringComparison.Ordinal))
        {
            await WriteUsageErrorAsync(standardError, "The required command is 'compile'.", cancellationToken);
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
            string outputPath;
            try
            {
                outputPath = await WriteOutputAsync(options, snapshot, compilation.Bytes, cancellationToken);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or PathTooLongException)
            {
                await standardError.WriteLineAsync($"Output failed [OUTPUT_WRITE_FAILED]: {exception.Message}".AsMemory(), cancellationToken);
                return (int)RulesCompilerExitCode.Output;
            }

            var serializedHash = Convert.ToHexString(SHA256.HashData(compilation.Bytes.Span));
            await standardOutput.WriteLineAsync("Compiled Rules artifact written.".AsMemory(), cancellationToken);
            await standardOutput.WriteLineAsync($"Output: {outputPath}".AsMemory(), cancellationToken);
            await standardOutput.WriteLineAsync($"Rule Sources: {artifact.RuleSources.Count}".AsMemory(), cancellationToken);
            await standardOutput.WriteLineAsync($"Snippets: {artifact.Snippets.Count}".AsMemory(), cancellationToken);
            await standardOutput.WriteLineAsync($"Semantic digest: {artifact.Integrity.ArtifactSha256}".AsMemory(), cancellationToken);
            await standardOutput.WriteLineAsync($"Serialized artifact byte SHA-256: {serializedHash}".AsMemory(), cancellationToken);
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
            if (!RequiredOptions.Contains(option, StringComparer.Ordinal))
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
            values["--output"]), null);
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

Required options:
  --source-root <path>       Root of the already-materialized Rule Source payload.
  --manifest <path>          Normalized source-root-relative manifest path.
  --source-scheme <id>       Immutable source identity scheme.
  --source-value <value>     Immutable source identity value.
  --compiler-id <id>         Compiler implementation identifier.
  --compiler-version <value> Compiler implementation version.
  --output <path>            Compiled Rules artifact output file.

Commands:
  compile                    Compile the materialized payload to format-1 JSON.

Options:
  -h, --help                 Show this help.

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
        string OutputPath);

    private sealed record ParseResult(CompilerOptions? Options, string? Error);
}
