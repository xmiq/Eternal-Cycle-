using System.Text;
using System.Text.Json.Nodes;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Compiler.Tests;

public sealed partial class RulesCompilerCliTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task CanonicalAcceptanceFailuresAreBoundedAndPublishNoArtifactOrReport(bool invalidTarget, bool audit)
    {
        using var payload = new CanonicalVocabularyPayload();
        using var destination = TestPayload.Empty();
        var manifestPath = Path.Combine(payload.Root, CanonicalVocabularyPayload.ManifestPath);
        var manifest = JsonNode.Parse(await File.ReadAllBytesAsync(manifestPath))!;
        if (invalidTarget) { manifest["retrievalVocabulary"]!["bindings"]![0]!["sourceAnchor"] = "not-an-anchor"; }
        else { manifest["retrievalVocabulary"]!["concepts"]![0]!["weight"] = 0; }
        await File.WriteAllTextAsync(manifestPath, manifest.ToJsonString(), new UTF8Encoding(false));
        var path = Path.Combine(destination.Root, "absent", audit ? "audit.json" : "artifact.json");
        var result = await RunProcessAsync(CanonicalArguments(payload.Root, path, audit), destination.Root);
        Assert.Equal(invalidTarget ? 4 : 3, result.ExitCode);
        Assert.Contains(invalidTarget ? "VOCABULARY_TARGET_MISSING" : "VOCABULARY_WEIGHT_INVALID", result.StandardError);
        Assert.InRange(result.StandardError.Length, 1, 1024);
        Assert.DoesNotContain(" at ", result.StandardError);
        Assert.Empty(result.StandardOutput);
        Assert.False(File.Exists(path));
        Assert.False(Directory.Exists(Path.GetDirectoryName(path)));
    }
}
