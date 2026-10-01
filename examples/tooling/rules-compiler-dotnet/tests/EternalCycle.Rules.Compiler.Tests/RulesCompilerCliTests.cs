using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EternalCycle.Rules;
using EternalCycle.Rules.Compiler;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Compiler.Tests;

public sealed partial class RulesCompilerCliTests(ITestOutputHelper output)
{
    private const string SourceScheme = "test-source";
    private const string SourceValue = "immutable-source";
    private const string CompilerId = "eternal-cycle-dotnet";
    private const string CompilerVersion = "1.0.0";

    private static readonly JsonSerializerOptions ManifestJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) }
    };

    [Fact]
    public async Task HelpSucceedsAndDescribesRequiredInputs()
    {
        var result = await RunProcessAsync(["--help"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.StandardError);
        Assert.Contains("--source-root", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--manifest", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--source-scheme", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--source-value", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--compiler-id", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--compiler-version", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--output", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MissingRequiredArgumentsFailPredictably()
    {
        var result = await RunProcessAsync(["compile", "--source-root", "."]);

        Assert.Equal((int)RulesCompilerExitCode.Usage, result.ExitCode);
        Assert.Contains("Missing required option", result.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnknownArgumentsFailPredictably()
    {
        var result = await RunProcessAsync(["compile", "--unknown", "value"]);

        Assert.Equal((int)RulesCompilerExitCode.Usage, result.ExitCode);
        Assert.Contains("Unknown argument '--unknown'", result.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidMinimalPayloadCompilesThroughExecutable()
    {
        using var payload = TestPayload.Minimal("# Core\n\nRule.");
        var outputPath = Path.Combine(payload.Root, "output", "compiled-rules.json");

        var result = await RunProcessAsync(Arguments(payload.Root, outputPath));

        Assert.Equal(0, result.ExitCode);
        Assert.True(File.Exists(outputPath));
        Assert.Contains("Rule Sources: 1", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Snippets: 1", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ProducedArtifactPassesFormatOneValidation()
    {
        using var payload = TestPayload.Minimal("## Rule\nText.");
        var outputPath = Path.Combine(payload.Root, "compiled.json");

        var result = await RunProcessAsync(Arguments(payload.Root, outputPath));
        var read = CompiledRulesArtifactContract.Read(await File.ReadAllTextAsync(outputPath));

        Assert.Equal(0, result.ExitCode);
        Assert.True(read.IsValid, Errors(read.Errors));
    }

    [Fact]
    public async Task CliOutputExactlyMatchesDirectPortableWriter()
    {
        using var payload = TestPayload.Minimal("## Rule\nText.");
        var outputPath = Path.Combine(payload.Root, "compiled.json");

        var result = await RunProcessAsync(Arguments(payload.Root, outputPath));
        var expected = DirectCompile(payload.Root);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(expected.Bytes.ToArray(), await File.ReadAllBytesAsync(outputPath));
    }

    [Fact]
    public async Task RepeatedCliInvocationIsByteIdenticalAndReplacesExistingOutput()
    {
        using var payload = TestPayload.Minimal("## Rule\nText.");
        var outputPath = Path.Combine(payload.Root, "compiled.json");

        var first = await RunProcessAsync(Arguments(payload.Root, outputPath));
        var firstBytes = await File.ReadAllBytesAsync(outputPath);
        await File.WriteAllTextAsync(outputPath, "obsolete");
        var second = await RunProcessAsync(Arguments(payload.Root, outputPath));

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(0, second.ExitCode);
        Assert.Equal(firstBytes, await File.ReadAllBytesAsync(outputPath));
    }

    [Fact]
    public async Task RelocatedIdenticalPayloadProducesByteIdenticalArtifact()
    {
        using var original = TestPayload.Minimal("## Rule\nText.");
        using var relocated = original.Clone();
        var firstPath = Path.Combine(original.Root, "compiled.json");
        var secondPath = Path.Combine(relocated.Root, "compiled.json");

        var first = await RunProcessAsync(Arguments(original.Root, firstPath));
        var second = await RunProcessAsync(Arguments(relocated.Root, secondPath));

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(0, second.ExitCode);
        Assert.Equal(await File.ReadAllBytesAsync(firstPath), await File.ReadAllBytesAsync(secondPath));
    }

    [Fact]
    public async Task UnrelatedWorkingDirectoryDoesNotAffectArtifact()
    {
        using var payload = TestPayload.Minimal("## Rule\nText.");
        using var workingDirectory = TestPayload.Empty();
        var firstPath = Path.Combine(payload.Root, "first.json");
        var secondPath = Path.Combine(payload.Root, "second.json");

        var first = await RunProcessAsync(Arguments(payload.Root, firstPath));
        var second = await RunProcessAsync(Arguments(payload.Root, secondPath), workingDirectory.Root);

        Assert.Equal(0, first.ExitCode);
        Assert.Equal(0, second.ExitCode);
        Assert.Equal(await File.ReadAllBytesAsync(firstPath), await File.ReadAllBytesAsync(secondPath));
    }

    [Fact]
    public async Task CurrentCultureDoesNotAffectArtifact()
    {
        using var payload = TestPayload.Minimal("## Échos\nRègle.");
        var turkishPath = Path.Combine(payload.Root, "turkish.json");
        var japanesePath = Path.Combine(payload.Root, "japanese.json");
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(0, await RunInProcessAsync(Arguments(payload.Root, turkishPath)));
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ja-JP");
            Assert.Equal(0, await RunInProcessAsync(Arguments(payload.Root, japanesePath)));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }

        Assert.Equal(await File.ReadAllBytesAsync(turkishPath), await File.ReadAllBytesAsync(japanesePath));
    }

    [Fact]
    public async Task LfAndCrLfRemainExecutableEquivalentButArtifactDistinct()
    {
        using var lf = TestPayload.Minimal("# Title\n\n## Rule\nText.\n");
        using var crlf = TestPayload.Minimal("# Title\r\n\r\n## Rule\r\nText.\r\n");
        var lfPath = Path.Combine(lf.Root, "compiled.json");
        var crlfPath = Path.Combine(crlf.Root, "compiled.json");

        Assert.Equal(0, (await RunProcessAsync(Arguments(lf.Root, lfPath))).ExitCode);
        Assert.Equal(0, (await RunProcessAsync(Arguments(crlf.Root, crlfPath))).ExitCode);
        var lfArtifact = CompiledRulesArtifactContract.Read(await File.ReadAllTextAsync(lfPath)).Artifact!;
        var crlfArtifact = CompiledRulesArtifactContract.Read(await File.ReadAllTextAsync(crlfPath)).Artifact!;

        Assert.Equal(SnippetSemantics(lfArtifact), SnippetSemantics(crlfArtifact));
        Assert.NotEqual(lfArtifact.RuleSources[0].SourceSha256, crlfArtifact.RuleSources[0].SourceSha256);
        Assert.NotEqual(lfArtifact.Integrity.ArtifactSha256, crlfArtifact.Integrity.ArtifactSha256);
        Assert.NotEqual(await File.ReadAllBytesAsync(lfPath), await File.ReadAllBytesAsync(crlfPath));
    }

    [Fact]
    public async Task MalformedManifestMapsToInputFailureAndWritesNoOutput()
    {
        using var payload = TestPayload.Empty();
        payload.WriteBytes(TestPayload.ManifestPath, "not json"u8.ToArray());
        var outputPath = Path.Combine(payload.Root, "compiled.json");

        var result = await RunProcessAsync(Arguments(payload.Root, outputPath));

        Assert.Equal((int)RulesCompilerExitCode.MaterializedInput, result.ExitCode);
        Assert.Contains("MANIFEST_JSON_INVALID", result.StandardError, StringComparison.Ordinal);
        Assert.False(File.Exists(outputPath));
    }

    [Fact]
    public async Task EmptySourceMapsToSnippetCompilationFailure()
    {
        using var payload = TestPayload.Minimal(" \r\n\t\r\n");
        var outputPath = Path.Combine(payload.Root, "compiled.json");

        var result = await RunProcessAsync(Arguments(payload.Root, outputPath));

        Assert.Equal((int)RulesCompilerExitCode.SnippetCompilation, result.ExitCode);
        Assert.Contains("RULE_SOURCE_CONTENT_EMPTY", result.StandardError, StringComparison.Ordinal);
        Assert.False(File.Exists(outputPath));
    }

    [Fact]
    public async Task DirectoryOutputMapsToOutputFailure()
    {
        using var payload = TestPayload.Minimal("Rule.");

        var result = await RunProcessAsync(Arguments(payload.Root, payload.Root));

        Assert.Equal((int)RulesCompilerExitCode.Output, result.ExitCode);
        Assert.Contains("OUTPUT_WRITE_FAILED", result.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SourceCollisionIsRejectedWithoutChangingSource()
    {
        using var payload = TestPayload.Minimal("Original source.");
        var sourcePath = Path.Combine(payload.Root, TestPayload.SourcePath.Replace('/', Path.DirectorySeparatorChar));
        var before = await File.ReadAllBytesAsync(sourcePath);

        var result = await RunProcessAsync(Arguments(payload.Root, sourcePath));

        Assert.Equal((int)RulesCompilerExitCode.Output, result.ExitCode);
        Assert.Equal(before, await File.ReadAllBytesAsync(sourcePath));
    }

    [Fact]
    public async Task OutputHasNoBomOrCliAddedTrailingNewline()
    {
        using var payload = TestPayload.Minimal("# Core\n\nRule.");
        var outputPath = Path.Combine(payload.Root, "compiled.json");

        Assert.Equal(0, (await RunProcessAsync(Arguments(payload.Root, outputPath))).ExitCode);
        var bytes = await File.ReadAllBytesAsync(outputPath);

        Assert.False(bytes.AsSpan().StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }));
        Assert.NotEqual((byte)'\n', bytes[^1]);
        Assert.NotEqual((byte)'\r', bytes[^1]);
    }

    [Fact]
    public async Task CanonicalCorpusMatchesDirectPortablePipeline()
    {
        var root = FindRepositoryRoot();
        using var outputDirectory = TestPayload.Empty();
        var outputPath = Path.Combine(outputDirectory.Root, "eternal-cycle-compiled-rules.json");
        var arguments = Arguments(
            root,
            outputPath,
            manifestPath: "docs/rules/rule-source-manifest.json",
            sourceScheme: "git-commit",
            sourceValue: "d5028af32cf16fe48878ffef0301867175f98f08");

        var result = await RunProcessAsync(arguments);
        var bytes = await File.ReadAllBytesAsync(outputPath);
        var artifact = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(bytes)).Artifact!;
        var direct = DirectCompile(
            root,
            "docs/rules/rule-source-manifest.json",
            "git-commit",
            "d5028af32cf16fe48878ffef0301867175f98f08");

        output.WriteLine("Semantic digest: {0}", artifact.Integrity.ArtifactSha256);
        output.WriteLine("Serialized artifact byte SHA-256: {0}", Sha256(bytes));
        Assert.Equal(0, result.ExitCode);
        Assert.Equal(10, artifact.RuleSources.Count);
        Assert.Equal(154, artifact.Snippets.Count);
        Assert.Equal(223929, bytes.Length);
        Assert.Equal("4B583A7904CE514B56CF3B497648DB5D778FC04DF2285D30A2E03823A32EA6A4", artifact.Integrity.ArtifactSha256);
        Assert.Equal("802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC", Sha256(bytes));
        Assert.Equal(direct.Bytes.ToArray(), bytes);
    }

    [Fact]
    public void CliDependencyBoundaryExcludesManagedRuntimeAndPersistence()
    {
        var references = typeof(RulesCompilerCli).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("EternalCycle.Rules", references);
        Assert.DoesNotContain("EternalCycle.Persistence.Mcp", references);
        Assert.DoesNotContain("Microsoft.Data.SqlClient", references);
        Assert.DoesNotContain("ModelContextProtocol", references);
    }

    private static async Task<int> RunInProcessAsync(IReadOnlyList<string> arguments)
    {
        using var standardOutput = new StringWriter(CultureInfo.InvariantCulture);
        using var standardError = new StringWriter(CultureInfo.InvariantCulture);
        return await RulesCompilerCli.RunAsync(arguments, standardOutput, standardError);
    }

    private static async Task<ProcessResult> RunProcessAsync(
        IReadOnlyList<string> arguments,
        string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = workingDirectory ?? FindRepositoryRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(CompilerDllPath());
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.Environment["DOTNET_NOLOGO"] = "1";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The standalone compiler process could not be started.");
        var standardOutput = process.StandardOutput.ReadToEndAsync();
        var standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return new(process.ExitCode, await standardOutput, await standardError);
    }

    private static string[] Arguments(
        string sourceRoot,
        string outputPath,
        string manifestPath = TestPayload.ManifestPath,
        string sourceScheme = SourceScheme,
        string sourceValue = SourceValue) =>
    [
        "compile",
        "--source-root", sourceRoot,
        "--manifest", manifestPath,
        "--source-scheme", sourceScheme,
        "--source-value", sourceValue,
        "--compiler-id", CompilerId,
        "--compiler-version", CompilerVersion,
        "--output", outputPath
    ];

    private static CompiledRulesArtifactWriteResult DirectCompile(
        string sourceRoot,
        string manifestPath = TestPayload.ManifestPath,
        string sourceScheme = SourceScheme,
        string sourceValue = SourceValue)
    {
        var snapshot = MaterializedRuleSourceLoader.Load(new(
            sourceRoot,
            manifestPath,
            new CompiledRulesSourceIdentity { Scheme = sourceScheme, Value = sourceValue },
            new CompiledRulesCompilerIdentity
            {
                ContractVersion = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                ImplementationId = CompilerId,
                ImplementationVersion = CompilerVersion
            }));
        var assembly = CompiledRulesArtifactAssembler.Assemble(snapshot, RuleSnippetCompiler.Compile(snapshot));
        Assert.True(assembly.IsValid, Errors(assembly.Errors));
        var write = CompiledRulesArtifactWriter.Write(assembly.Artifact!);
        Assert.True(write.IsValid, Errors(write.Errors));
        return write;
    }

    private static string SnippetSemantics(CompiledRulesArtifact artifact) =>
        JsonSerializer.Serialize(artifact.Snippets.Select(snippet => new
        {
            snippet.SnippetId,
            snippet.SourceAnchor,
            snippet.Content,
            snippet.ContentSha256
        }));

    private static string Errors(IReadOnlyList<CompiledRulesArtifactValidationError> errors) =>
        string.Join(Environment.NewLine, errors.Select(error => $"{error.Code} {error.Path}: {error.Message}"));

    private static string Sha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

    private static string CompilerDllPath() => Path.Combine(
        FindRepositoryRoot(),
        "examples", "tooling", "rules-compiler-dotnet", "src", "EternalCycle.Rules.Compiler",
        "bin", "Release", "net10.0", "EternalCycle.Rules.Compiler.dll");

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "VERSION")) &&
                File.Exists(Path.Combine(directory.FullName, "docs", "rules", "rule-source-manifest.json")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new InvalidOperationException("The Eternal Cycle repository root could not be found from the test output directory.");
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

    private sealed class TestPayload : IDisposable
    {
        public const string ManifestPath = "rules/manifest.json";
        public const string SourcePath = "rules/core.md";

        private TestPayload()
        {
            Root = Path.Combine(Path.GetTempPath(), "EternalCycle.Compiler.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public static TestPayload Empty() => new();

        public static TestPayload Minimal(string content)
        {
            var payload = new TestPayload();
            payload.WriteBytes(SourcePath, Encoding.UTF8.GetBytes(content));
            payload.WriteManifest(
            [
                new RuleSourceManifestEntry
                {
                    RuleSourceId = "core",
                    Path = SourcePath,
                    Layer = RuleLayer.Core,
                    CampaignModes = ["*"],
                    Operations = ["*"],
                    PreparationTier = RulePreparationTier.Standard
                }
            ]);
            return payload;
        }

        public TestPayload Clone()
        {
            var clone = new TestPayload();
            foreach (var file in Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(Root, file);
                clone.WriteBytes(relativePath.Replace(Path.DirectorySeparatorChar, '/'), File.ReadAllBytes(file));
            }
            return clone;
        }

        public void WriteManifest(IList<RuleSourceManifestEntry> sources) =>
            WriteBytes(ManifestPath, JsonSerializer.SerializeToUtf8Bytes(new RuleSourceManifest
            {
                ManifestFormatVersion = MaterializedRuleSourceLoader.CurrentManifestFormatVersion,
                CompilerContractVersion = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                RulesetId = "test-rules",
                RepositoryVersion = "test",
                Sources = sources
            }, ManifestJson));

        public void WriteBytes(string relativePath, byte[] bytes)
        {
            var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
