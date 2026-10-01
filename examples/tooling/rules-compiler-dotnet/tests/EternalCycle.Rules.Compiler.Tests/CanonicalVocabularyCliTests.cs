using System.Globalization;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Compiler.Tests;

public sealed partial class RulesCompilerCliTests
{
    [Fact]
    public async Task CanonicalVocabularyCompileAuditCompileAuditMatchesLibraryAndFrozenEvidence()
    {
        using var payload = new CanonicalVocabularyPayload();
        using var destination = TestPayload.Empty();
        var artifactPath = Path.Combine(destination.Root, "artifact.json");
        var reportPath = Path.Combine(destination.Root, "audit.json");
        var compile = CanonicalArguments(payload.Root, artifactPath);
        var audit = CanonicalArguments(payload.Root, reportPath, audit: true);
        var snapshot = payload.Load();
        var direct = RuleCompilationPipeline.Compile(snapshot);
        var report = RuleVocabularyAuditor.Analyze(snapshot, direct, CanonicalVocabularyPayload.AuditPolicy());
        var expected = CanonicalVocabularyPayload.Fixture<CanonicalAfterE>("after-e.json");
        for (var iteration = 0; iteration < 2; iteration++)
        {
            var compiled = await RunProcessAsync(compile, destination.Root);
            Assert.Equal(0, compiled.ExitCode);
            Assert.Empty(compiled.StandardError);
            var audited = await RunProcessAsync(audit, destination.Root);
            Assert.Equal(0, audited.ExitCode);
            Assert.Empty(audited.StandardError);
            Assert.Contains("0 errors, 3 warnings, 105 information", audited.StandardOutput);
            Assert.Equal(direct.Bytes.ToArray(), await File.ReadAllBytesAsync(artifactPath));
            Assert.Equal(RuleVocabularyAuditWriter.Write(report).ToArray(), await File.ReadAllBytesAsync(reportPath));
            Assert.Equal(expected.SerializedSha256, Sha256(await File.ReadAllBytesAsync(artifactPath)));
            Assert.Equal(expected.AuditSha256, Sha256(await File.ReadAllBytesAsync(reportPath)));
        }
        Assert.Empty(Directory.EnumerateFiles(destination.Root, "*.tmp"));
    }

    [Fact]
    public async Task CanonicalVocabularyRelocationAndUnrelatedWorkingDirectoryPreserveBothOutputs()
    {
        using var first = new CanonicalVocabularyPayload();
        using var second = new CanonicalVocabularyPayload();
        using var destination = TestPayload.Empty();
        foreach (var audit in new[] { false, true })
        {
            var firstPath = Path.Combine(destination.Root, audit ? "first-audit.json" : "first-artifact.json");
            var secondPath = Path.Combine(destination.Root, audit ? "second-audit.json" : "second-artifact.json");
            Assert.Equal(0, (await RunProcessAsync(CanonicalArguments(first.Root, firstPath, audit), destination.Root)).ExitCode);
            Assert.Equal(0, (await RunProcessAsync(CanonicalArguments(second.Root, secondPath, audit), second.Root)).ExitCode);
            Assert.Equal(await File.ReadAllBytesAsync(firstPath), await File.ReadAllBytesAsync(secondPath));
        }
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("fr-FR")]
    public async Task CanonicalVocabularyCliCultureMatchesPortableOutputs(string culture)
    {
        using var payload = new CanonicalVocabularyPayload();
        using var destination = TestPayload.Empty();
        var previous = CultureInfo.CurrentCulture;
        var previousUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
            var snapshot = payload.Load();
            var compilation = RuleCompilationPipeline.Compile(snapshot);
            foreach (var audit in new[] { false, true })
            {
                var path = Path.Combine(destination.Root, audit ? "audit.json" : "artifact.json");
                Assert.Equal(0, await RunInProcessAsync(CanonicalArguments(payload.Root, path, audit)));
                Assert.Equal(audit ? RuleVocabularyAuditWriter.Write(RuleVocabularyAuditor.Analyze(snapshot, compilation,
                    CanonicalVocabularyPayload.AuditPolicy())).ToArray() : compilation.Bytes.ToArray(), await File.ReadAllBytesAsync(path));
            }
        }
        finally { CultureInfo.CurrentCulture = previous; CultureInfo.CurrentUICulture = previousUi; }
    }

    private static string[] CanonicalArguments(string root, string path, bool audit = false)
    {
        var arguments = Arguments(root, path, CanonicalVocabularyPayload.ManifestPath, "git-commit", CanonicalVocabularyPayload.SourceIdentity);
        if (!audit) { return arguments; }
        arguments[0] = "audit";
        return arguments.Concat(["--audit-policy", CanonicalVocabularyPayload.FixturePath("audit-policy.json")]).ToArray();
    }
}
