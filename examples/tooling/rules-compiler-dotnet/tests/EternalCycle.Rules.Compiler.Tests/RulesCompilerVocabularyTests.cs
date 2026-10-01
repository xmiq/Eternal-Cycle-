using System.Globalization;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules;
using EternalCycle.Rules.Compiler;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Compiler.Tests;

public sealed partial class RulesCompilerCliTests
{
    [Fact]
    public async Task VocabularyCliMatchesPortablePipelineAndValidates()
    {
        using var payload = new VocabularyTestPayload();
        var path = Path.Combine(payload.Root, "output", "artifact.json");
        var cli = await RunProcessAsync(Arguments(payload.Root, path, "manifest.json"));
        Assert.Equal(0, cli.ExitCode);
        Assert.Empty(cli.StandardError);
        var bytes = await File.ReadAllBytesAsync(path);
        var direct = payload.Compile();
        Assert.True(direct.IsValid);
        Assert.Equal(direct.Bytes.ToArray(), bytes);
        var read = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(bytes));
        Assert.True(read.IsValid);
        Assert.Equal(15, read.Artifact!.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count));
        output.WriteLine("Vocabulary fixture CLI/library byte equality: PASS");
        output.WriteLine("Sources={0} Snippets={1} Concepts=3 Associations=15 Bytes={2}", read.Artifact.RuleSources.Count, read.Artifact.Snippets.Count, bytes.Length);
        output.WriteLine("Semantic digest: {0}", read.Artifact.Integrity.ArtifactSha256);
        output.WriteLine("Serialized byte SHA-256: {0}", Sha256(bytes));
    }

    [Fact]
    public async Task VocabularyAbsentCliAlsoMatchesNewPortablePipeline()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        manifest.Remove("retrievalVocabulary");
        payload.WriteManifest(manifest);
        var path = Path.Combine(payload.Root, "artifact.json");
        Assert.Equal(0, (await RunProcessAsync(Arguments(payload.Root, path, "manifest.json"))).ExitCode);
        Assert.Equal(payload.Compile().Bytes.ToArray(), await File.ReadAllBytesAsync(path));
    }

    [Fact]
    public async Task VocabularyCliRepeatsRelocatesAndIgnoresWorkingDirectory()
    {
        using var original = new VocabularyTestPayload();
        using var relocated = new VocabularyTestPayload();
        using var unrelated = TestPayload.Empty();
        var firstPath = Path.Combine(original.Root, "artifact.json");
        var relocatedPath = Path.Combine(relocated.Root, "artifact.json");
        Assert.Equal(0, (await RunProcessAsync(Arguments(original.Root, firstPath, "manifest.json"))).ExitCode);
        var firstBytes = await File.ReadAllBytesAsync(firstPath);
        await File.WriteAllTextAsync(firstPath, "obsolete output");
        Assert.Equal(0, (await RunProcessAsync(Arguments(original.Root, firstPath, "manifest.json"), unrelated.Root)).ExitCode);
        Assert.Equal(0, (await RunProcessAsync(Arguments(relocated.Root, relocatedPath, "manifest.json"), unrelated.Root)).ExitCode);
        Assert.Equal(firstBytes, await File.ReadAllBytesAsync(firstPath));
        Assert.Equal(firstBytes, await File.ReadAllBytesAsync(relocatedPath));
        Assert.Empty(Directory.EnumerateFiles(original.Root, "*.tmp"));
    }

    [Fact]
    public async Task VocabularyCliCultureDoesNotChangeOutput()
    {
        using var payload = new VocabularyTestPayload();
        var firstPath = Path.Combine(payload.Root, "first.json");
        var secondPath = Path.Combine(payload.Root, "second.json");
        var original = CultureInfo.CurrentCulture;
        var originalUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(0, await RunInProcessAsync(Arguments(payload.Root, firstPath, "manifest.json")));
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ja-JP");
            Assert.Equal(0, await RunInProcessAsync(Arguments(payload.Root, secondPath, "manifest.json")));
        }
        finally { CultureInfo.CurrentCulture = original; CultureInfo.CurrentUICulture = originalUi; }
        Assert.Equal(await File.ReadAllBytesAsync(firstPath), await File.ReadAllBytesAsync(secondPath));
    }

    [Theory]
    [InlineData("anchor", 4, "VOCABULARY_TARGET_MISSING")]
    [InlineData("concept", 3, "VOCABULARY_CONCEPT_MISSING")]
    [InlineData("source", 3, "VOCABULARY_SOURCE_MISSING")]
    [InlineData("weight", 3, "VOCABULARY_WEIGHT_INVALID")]
    [InlineData("kind", 3, "MANIFEST_JSON_INVALID")]
    [InlineData("duplicate", 3, "VOCABULARY_BINDING_DUPLICATE")]
    public async Task VocabularyFailuresUseNormalExitCodesAndNeverWriteArtifacts(string defect, int exitCode, string code)
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        var definition = manifest["retrievalVocabulary"]!;
        switch (defect)
        {
            case "anchor": definition["bindings"]![0]!["sourceAnchor"] = "not-declared"; break;
            case "concept": definition["bindings"]![0]!["conceptIds"]![0] = "not-declared"; break;
            case "source": definition["bindings"]![0]!["ruleSourceId"] = "not-declared"; break;
            case "weight": definition["concepts"]![0]!["weight"] = 0; break;
            case "kind": definition["concepts"]![0]!["alternatives"]![0]!["kind"] = "Guessed"; break;
            case "duplicate": definition["bindings"]!.AsArray().Add(definition["bindings"]![0]!.DeepClone()); break;
        }
        payload.WriteManifest(manifest);
        var path = Path.Combine(payload.Root, "artifact.json");
        var result = await RunProcessAsync(Arguments(payload.Root, path, "manifest.json"));
        Assert.Equal(exitCode, result.ExitCode);
        Assert.Contains(code, result.StandardError);
        Assert.Empty(result.StandardOutput);
        Assert.True(result.StandardError.Length < 1024);
        Assert.DoesNotContain(" at ", result.StandardError);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task VocabularyFailurePreservesExistingOutputAndDoesNotCreateParentDirectories()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        manifest["retrievalVocabulary"]!["bindings"]![0]!["sourceAnchor"] = "not-declared";
        payload.WriteManifest(manifest);
        var path = Path.Combine(payload.Root, "artifact.json");
        await File.WriteAllTextAsync(path, "previous valid artifact");
        Assert.Equal(4, (await RunProcessAsync(Arguments(payload.Root, path, "manifest.json"))).ExitCode);
        Assert.Equal("previous valid artifact", await File.ReadAllTextAsync(path));
        var absent = Path.Combine(payload.Root, "absent", "artifact.json");
        Assert.Equal(4, (await RunProcessAsync(Arguments(payload.Root, absent, "manifest.json"))).ExitCode);
        Assert.False(Directory.Exists(Path.GetDirectoryName(absent)));
    }

    [Theory]
    [InlineData("manifest.json")]
    [InlineData("core.txt")]
    [InlineData(".")]
    public async Task VocabularyOutputSafetyPreservesAllAuthoritativeInputs(string relativeOutput)
    {
        using var payload = new VocabularyTestPayload();
        var before = Directory.EnumerateFiles(payload.Root).ToDictionary(path => Path.GetFileName(path)!, File.ReadAllBytes);
        var result = await RunProcessAsync(Arguments(payload.Root, Path.Combine(payload.Root, relativeOutput), "manifest.json"));
        Assert.Equal((int)RulesCompilerExitCode.Output, result.ExitCode);
        foreach (var file in before) { Assert.Equal(file.Value, await File.ReadAllBytesAsync(Path.Combine(payload.Root, file.Key!))); }
    }

    [Fact]
    public async Task VocabularyOutputHasNoBomNewlineOrDetailedAuditPayload()
    {
        using var payload = new VocabularyTestPayload();
        var path = Path.Combine(payload.Root, "artifact.json");
        Assert.Equal(0, (await RunProcessAsync(Arguments(payload.Root, path, "manifest.json"))).ExitCode);
        var bytes = await File.ReadAllBytesAsync(path);
        Assert.False(bytes.AsSpan().StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }));
        Assert.Equal((byte)'}', bytes[^1]);
        var text = Encoding.UTF8.GetString(bytes);
        Assert.DoesNotContain("rationale", text);
        Assert.DoesNotContain("Origins", text);
        Assert.DoesNotContain(payload.Root, text);
        using var json = JsonDocument.Parse(bytes);
        Assert.False(json.RootElement.TryGetProperty("retrievalVocabulary", out _));
    }

    [Fact]
    public async Task VocabularyDoesNotGenerateTermsForAnUnboundHeading()
    {
        using var payload = new VocabularyTestPayload();
        var manifest = payload.ReadManifest();
        manifest["retrievalVocabulary"]!["bindings"]!.AsArray().RemoveAt(0);
        payload.WriteManifest(manifest);
        var path = Path.Combine(payload.Root, "artifact.json");
        Assert.Equal(0, (await RunProcessAsync(Arguments(payload.Root, path, "manifest.json"))).ExitCode);
        var read = CompiledRulesArtifactContract.Read(await File.ReadAllTextAsync(path));
        Assert.True(read.IsValid);
        Assert.Empty(read.Artifact!.Snippets.Single(snippet => snippet.SnippetId == "core#combat").Retrieval.Terms);
    }
}
