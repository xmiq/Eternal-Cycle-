using System.Globalization;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Compiler.Tests;

public sealed partial class RulesCompilerCliTests
{
    [Fact]
    public async Task AuditCliMatchesPortableSerializationAndRepeatedRelocatedProcesses()
    {
        using var payload = new VocabularyTestPayload();
        using var relocated = new VocabularyTestPayload();
        using var unrelated = TestPayload.Empty();
        var path = Path.Combine(payload.Root, "audit", "report.json");
        var movedPath = Path.Combine(relocated.Root, "report.json");
        var arguments = AuditArguments(payload.Root, path);
        var first = await RunProcessAsync(arguments, unrelated.Root);
        Assert.Equal(0, first.ExitCode);
        Assert.Empty(first.StandardError);
        var bytes = await File.ReadAllBytesAsync(path);
        var snapshot = payload.Load();
        var direct = RuleVocabularyAuditor.Analyze(snapshot, RuleCompilationPipeline.Compile(snapshot));
        Assert.Equal(RuleVocabularyAuditWriter.Write(direct).ToArray(), bytes);
        Assert.Equal(0, (await RunProcessAsync(arguments)).ExitCode);
        Assert.Equal(0, (await RunProcessAsync(AuditArguments(relocated.Root, movedPath), unrelated.Root)).ExitCode);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(path));
        Assert.Equal(bytes, await File.ReadAllBytesAsync(movedPath));
        Assert.Contains("0 errors, 0 warnings, 9 information", first.StandardOutput);
        Assert.Contains("Serialized audit byte SHA-256", first.StandardOutput);
        Assert.DoesNotContain("Compiled Rules artifact written", first.StandardOutput);
        Assert.Equal((byte)'}', bytes[^1]);
        Assert.False(bytes.AsSpan().StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }));
        Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(path)!, "*.tmp"));
    }

    [Fact]
    public async Task ExplicitAuditPolicyHasLibraryEqualityAndWarningsDoNotFailCompilation()
    {
        using var payload = new VocabularyTestPayload();
        var policyPath = Path.Combine(payload.Root, "policy.json");
        await File.WriteAllTextAsync(policyPath, "{\"genericTerms\":[\" PROGRESS \",\"progress\"],\"minimumBroadTargets\":2}");
        var reportPath = Path.Combine(payload.Root, "report.json");
        var run = await RunProcessAsync(AuditArguments(payload.Root, reportPath, policyPath));
        Assert.Equal(0, run.ExitCode);
        Assert.Empty(run.StandardError);
        var snapshot = payload.Load();
        var compilation = RuleCompilationPipeline.Compile(snapshot);
        var report = RuleVocabularyAuditor.Analyze(snapshot, compilation, new() { MinimumBroadTargets = 2, GenericTerms = ["progress"] });
        Assert.True(report.Summary.WarningCount > 0);
        Assert.Equal(RuleVocabularyAuditWriter.Write(report).ToArray(), await File.ReadAllBytesAsync(reportPath));
        var artifactPath = Path.Combine(payload.Root, "artifact.json");
        Assert.Equal(0, (await RunProcessAsync(Arguments(payload.Root, artifactPath, "manifest.json"))).ExitCode);
        Assert.Equal(compilation.Bytes.ToArray(), await File.ReadAllBytesAsync(artifactPath));
    }

    [Fact]
    public async Task AuditCultureDoesNotAffectBytes()
    {
        using var payload = new VocabularyTestPayload();
        var path = Path.Combine(payload.Root, "report.json");
        var original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(0, await RunInProcessAsync(AuditArguments(payload.Root, path)));
            var first = await File.ReadAllBytesAsync(path);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.Equal(0, await RunInProcessAsync(AuditArguments(payload.Root, path)));
            Assert.Equal(first, await File.ReadAllBytesAsync(path));
        }
        finally { CultureInfo.CurrentCulture = original; }
    }

    [Theory]
    [InlineData("manifest.json")]
    [InlineData("core.txt")]
    [InlineData(".")]
    public async Task AuditCannotReplaceAuthoritativeInputOrDirectory(string name)
    {
        using var payload = new VocabularyTestPayload();
        var before = Directory.EnumerateFiles(payload.Root).ToDictionary(path => Path.GetFileName(path)!, File.ReadAllBytes);
        var run = await RunProcessAsync(AuditArguments(payload.Root, Path.Combine(payload.Root, name)));
        Assert.Equal(6, run.ExitCode);
        foreach (var item in before) { Assert.Equal(item.Value, await File.ReadAllBytesAsync(Path.Combine(payload.Root, item.Key))); }
    }

    [Fact]
    public async Task ArtifactAndAuditCannotReplaceOneAnotherAndPolicyIsProtected()
    {
        using var payload = new VocabularyTestPayload();
        var artifactPath = Path.Combine(payload.Root, "artifact.json");
        Assert.Equal(0, (await RunProcessAsync(Arguments(payload.Root, artifactPath, "manifest.json"))).ExitCode);
        var originalArtifact = await File.ReadAllBytesAsync(artifactPath);
        Assert.Equal(6, (await RunProcessAsync(AuditArguments(payload.Root, artifactPath))).ExitCode);
        Assert.Equal(originalArtifact, await File.ReadAllBytesAsync(artifactPath));
        var reportPath = Path.Combine(payload.Root, "report.json");
        Assert.Equal(0, (await RunProcessAsync(AuditArguments(payload.Root, reportPath))).ExitCode);
        var originalReport = await File.ReadAllBytesAsync(reportPath);
        Assert.Equal(6, (await RunProcessAsync(Arguments(payload.Root, reportPath, "manifest.json"))).ExitCode);
        Assert.Equal(originalReport, await File.ReadAllBytesAsync(reportPath));
        var policyPath = Path.Combine(payload.Root, "policy.json");
        await File.WriteAllTextAsync(policyPath, "{}");
        Assert.Equal(6, (await RunProcessAsync(AuditArguments(payload.Root, policyPath, policyPath))).ExitCode);
        Assert.Equal("{}", await File.ReadAllTextAsync(policyPath));
    }

    [Theory]
    [InlineData("previous unrelated output")]
    [InlineData("{}")]
    [InlineData("{\"artifactFormatVersion\":1")]
    [InlineData("{\"auditFormatVersion\":\"1\"}")]
    public async Task AuditRefusesUnrecognizedOrTruncatedPriorOutputs(string prior)
    {
        using var payload = new VocabularyTestPayload();
        var path = Path.Combine(payload.Root, "output.json");
        await File.WriteAllTextAsync(path, prior);
        Assert.Equal(6, (await RunProcessAsync(AuditArguments(payload.Root, path))).ExitCode);
        Assert.Equal(prior, await File.ReadAllTextAsync(path));
    }

    [Theory]
    [InlineData("target", 4, "VOCABULARY_TARGET_MISSING")]
    [InlineData("json", 3, "MANIFEST_JSON_INVALID")]
    public async Task InvalidAuditCompilationIsBoundedAndPreservesPriorReport(string defect, int exit, string code)
    {
        using var payload = new VocabularyTestPayload();
        var reportPath = Path.Combine(payload.Root, "report.json");
        await File.WriteAllTextAsync(reportPath, "previous output");
        if (defect == "json") { await File.WriteAllTextAsync(Path.Combine(payload.Root, "manifest.json"), "not json"); }
        else
        {
            var manifest = payload.ReadManifest();
            manifest["retrievalVocabulary"]!["bindings"]![0]!["sourceAnchor"] = "missing";
            payload.WriteManifest(manifest);
        }
        var run = await RunProcessAsync(AuditArguments(payload.Root, reportPath));
        Assert.Equal(exit, run.ExitCode);
        Assert.Contains(code, run.StandardError);
        Assert.True(run.StandardError.Length < 1024);
        Assert.Empty(run.StandardOutput);
        Assert.Equal("previous output", await File.ReadAllTextAsync(reportPath));
        var absent = Path.Combine(payload.Root, "absent", "report.json");
        Assert.Equal(exit, (await RunProcessAsync(AuditArguments(payload.Root, absent))).ExitCode);
        Assert.False(Directory.Exists(Path.GetDirectoryName(absent)));
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{\"unknown\":true}")]
    [InlineData("{\"minimumBroadTargets\":0}")]
    [InlineData("{\"minimumBroadTargets\":1,\"minimumBroadTargets\":2}")]
    [InlineData("{\"genericTerms\":null}")]
    public async Task InvalidPolicyUsesOneBoundedDiagnosticWithoutWriting(string policy)
    {
        using var payload = new VocabularyTestPayload();
        var policyPath = Path.Combine(payload.Root, "policy.json");
        var reportPath = Path.Combine(payload.Root, "absent", "report.json");
        await File.WriteAllTextAsync(policyPath, policy);
        var run = await RunProcessAsync(AuditArguments(payload.Root, reportPath, policyPath));
        Assert.Equal(3, run.ExitCode);
        Assert.Contains("AUDIT_POLICY_INVALID", run.StandardError);
        Assert.True(run.StandardError.Length < 256);
        Assert.DoesNotContain(payload.Root, run.StandardError);
        Assert.False(File.Exists(reportPath));
        Assert.False(Directory.Exists(Path.GetDirectoryName(reportPath)));
    }

    [Fact]
    public async Task CompileDoesNotAcquireAuditOptionsAndAuditHelpExplainsSeparateOutput()
    {
        using var payload = new VocabularyTestPayload();
        var arguments = Arguments(payload.Root, Path.Combine(payload.Root, "artifact.json"), "manifest.json").Concat(["--audit-policy", "policy.json"]).ToArray();
        Assert.Equal(2, (await RunProcessAsync(arguments)).ExitCode);
        var help = await RunProcessAsync(["audit", "--help"]);
        Assert.Equal(0, help.ExitCode);
        Assert.Contains("observational audit JSON only", help.StandardOutput);
    }

    [Fact]
    public async Task HistoricalCanonicalCompileAuditCompileIsObservationalThroughRealCli()
    {
        using var destination = TestPayload.Empty();
        using var historical = new CanonicalVocabularyPayload(historical: true);
        var root = historical.Root;
        var artifactPath = Path.Combine(destination.Root, "canonical.json");
        var reportPath = Path.Combine(destination.Root, "audit.json");
        const string manifestPath = "docs/rules/rule-source-manifest.json";
        const string identity = "d5028af32cf16fe48878ffef0301867175f98f08";
        var compile = Arguments(root, artifactPath, manifestPath, "git-commit", identity);
        Assert.Equal(0, (await RunProcessAsync(compile)).ExitCode);
        var first = await File.ReadAllBytesAsync(artifactPath);
        var audit = Arguments(root, reportPath, manifestPath, "git-commit", identity);
        audit[0] = "audit";
        var audited = await RunProcessAsync(audit);
        Assert.Equal(0, audited.ExitCode);
        Assert.Equal(0, (await RunProcessAsync(compile)).ExitCode);
        Assert.Equal(first, await File.ReadAllBytesAsync(artifactPath));
        Assert.Equal(223929, first.Length);
        Assert.Equal("802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC", Sha256(first));
        using var report = JsonDocument.Parse(await File.ReadAllBytesAsync(reportPath));
        Assert.Equal(154, report.RootElement.GetProperty("summary").GetProperty("uncoveredSnippetCount").GetInt32());
        Assert.Equal(0, report.RootElement.GetProperty("summary").GetProperty("conceptCount").GetInt32());
        output.WriteLine(audited.StandardOutput);
    }

    private static string[] AuditArguments(string root, string outputPath, string? policyPath = null)
    {
        var arguments = Arguments(root, outputPath, "manifest.json");
        arguments[0] = "audit";
        return policyPath is null ? arguments : arguments.Concat(["--audit-policy", policyPath]).ToArray();
    }
}
