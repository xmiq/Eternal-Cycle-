using System.Text.Json.Nodes;
using EternalCycle.Persistence.Mcp;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class CompiledRulesArtifactContractTests
{
    [Theory]
    [InlineData("valid-minimal.json")]
    [InlineData("valid-representative.json")]
    public void ValidConformanceFixturesPass(string fixture)
    {
        var result = CompiledRulesArtifactContract.Read(ReadFixture(fixture));

        Assert.True(
            result.IsValid,
            $"{string.Join(Environment.NewLine, result.Errors)}{Environment.NewLine}" +
            $"Calculated={GetCalculatedHash(result)}");
    }

    [Fact]
    public void StableSnippetIdentityUsesSourceAndAnchorOnly()
    {
        Assert.Equal("core-reincarnation", CompiledRulesArtifactContract.CreateSnippetId("core-reincarnation", null));
        Assert.Equal("core-reincarnation#continuity", CompiledRulesArtifactContract.CreateSnippetId("core-reincarnation", "continuity"));
    }

    [Fact]
    public void DuplicateSnippetIdentityIsRejected()
    {
        var root = Representative();
        root["snippets"]!.AsArray().Add(root["snippets"]![0]!.DeepClone());

        AssertCode(root, "DUPLICATE_SNIPPET_ID");
    }

    [Fact]
    public void MissingDependencyTargetIsRejected()
    {
        var root = Representative();
        root["ruleSources"]![0]!["dependencyRuleSourceIds"]![0] = "missing-source";

        AssertCode(root, "DEPENDENCY_TARGET_MISSING");
    }

    [Fact]
    public void UnsupportedFormatAndCompilerContractsAreRejected()
    {
        var root = Representative();
        root["artifactFormatVersion"] = 2;
        root["compiler"]!["contractVersion"] = "2";

        AssertCodes(root, "FORMAT_VERSION_UNSUPPORTED", "COMPILER_CONTRACT_UNSUPPORTED");
    }

    [Fact]
    public void MalformedIdentityAndProvenanceAreRejected()
    {
        var root = Representative();
        root["ruleset"]!["rulesetId"] = "bad ruleset";
        root["ruleset"]!["manifestPath"] = "../outside.json";
        root["ruleSources"]![0]!["sourceSha256"] = "not-a-hash";

        AssertCodes(root, "IDENTIFIER_INVALID", "PATH_INVALID", "SHA256_INVALID");
    }

    [Fact]
    public void MalformedSelectorsAreRejected()
    {
        var root = Representative();
        root["ruleSources"]![0]!["applicability"]!["operations"] =
            new JsonArray("reincarnation.resolve", "gameplay.resolve");

        AssertCode(root, "CANONICAL_ORDER_INVALID");
    }

    [Fact]
    public void RetrievalMetadataMustBeNormalizedAndInternallyResolved()
    {
        var root = Representative();
        root["snippets"]![0]!["retrieval"]!["terms"]![0]!["term"] = " past  life ";
        root["snippets"]![0]!["retrieval"]!["relationships"]![0]!["toTerm"] = "missing term";

        AssertCodes(root, "RETRIEVAL_TERM_INVALID", "RETRIEVAL_RELATION_TARGET_MISSING");
    }

    [Fact]
    public void ContentNormalizationAndHashAreValidated()
    {
        var root = Representative();
        root["snippets"]![0]!["content"] = "# Reincarnation\r\n\r\nChanged.";

        AssertCodes(root, "CONTENT_NOT_NORMALIZED", "CONTENT_HASH_MISMATCH");
    }

    [Fact]
    public void ArtifactIntegrityMismatchIsRejected()
    {
        var root = Representative();
        root["snippets"]![0]!["estimatedTokens"] = 29;

        AssertCode(root, "ARTIFACT_HASH_MISMATCH");
    }

    [Fact]
    public void UnknownAndDuplicateJsonPropertiesAreRejected()
    {
        var unknown = Representative();
        unknown["providerUrl"] = "https://example.invalid/artifact";
        AssertCode(unknown, "JSON_INVALID");

        var duplicate = ReadFixture("valid-minimal.json")
            .Replace("\"artifactFormatVersion\": 1,", "\"artifactFormatVersion\": 1,\n  \"artifactFormatVersion\": 1,", StringComparison.Ordinal);
        Assert.Contains(
            CompiledRulesArtifactContract.Read(duplicate).Errors,
            error => error.Code == "DUPLICATE_JSON_PROPERTY");
    }

    [Fact]
    public void SourceIdentitySchemeDoesNotSelectAnAcquisitionProvider()
    {
        var root = Representative();
        root["ruleset"]!["source"]!["scheme"] = "custom-immutable";
        root["ruleset"]!["source"]!["value"] = "provider-neutral-fixture";
        Rehash(root);

        var result = CompiledRulesArtifactContract.Read(root.ToJsonString());

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors));
    }

    [Fact]
    public void NullRequiredStructureIsRejectedWithoutThrowing()
    {
        var root = Representative();
        root["ruleset"]!["source"] = null;

        AssertCode(root, "STRUCTURE_REQUIRED");
    }

    [Fact]
    public void MissingRequiredPropertyIsRejected()
    {
        var root = Representative();
        root["snippets"]![0]!.AsObject().Remove("retrieval");

        AssertCode(root, "JSON_INVALID");
    }

    [Fact]
    public void ProgrammaticEnumValuesOutsideTheContractAreRejected()
    {
        var artifact = CompiledRulesArtifactContract.Read(ReadFixture("valid-minimal.json")).Artifact!;
        var source = artifact.RuleSources[0];
        artifact.RuleSources[0] = new CompiledRulesArtifactRuleSource
        {
            RuleSourceId = source.RuleSourceId,
            SourcePath = source.SourcePath,
            SourceSha256 = source.SourceSha256,
            DependencyRuleSourceIds = source.DependencyRuleSourceIds,
            Applicability = new CompiledRulesArtifactApplicability
            {
                Layer = (RuleLayer)999,
                PreparationTier = (RulePreparationTier)999
            }
        };

        var errors = CompiledRulesArtifactContract.Validate(artifact);

        Assert.Contains(errors, error => error.Code == "RULE_LAYER_UNSUPPORTED");
        Assert.Contains(errors, error => error.Code == "PREPARATION_TIER_UNSUPPORTED");
    }

    private static JsonObject Representative() =>
        JsonNode.Parse(ReadFixture("valid-representative.json"))!.AsObject();

    private static string ReadFixture(string name) =>
        File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            "CompiledRulesArtifact",
            name));

    private static void AssertCode(JsonObject root, string code) =>
        Assert.Contains(
            CompiledRulesArtifactContract.Read(root.ToJsonString()).Errors,
            error => error.Code == code);

    private static void AssertCodes(JsonObject root, params string[] codes)
    {
        var errors = CompiledRulesArtifactContract.Read(root.ToJsonString()).Errors;
        foreach (var code in codes)
        {
            Assert.Contains(errors, error => error.Code == code);
        }
    }

    private static void Rehash(JsonObject root)
    {
        var parsed = CompiledRulesArtifactContract.Read(root.ToJsonString()).Artifact!;
        root["integrity"]!["artifactSha256"] = CompiledRulesArtifactContract.ComputeArtifactSha256(parsed);
    }

    private static string GetCalculatedHash(CompiledRulesArtifactReadResult result) =>
        result.Artifact is null
            ? "unavailable"
            : CompiledRulesArtifactContract.ComputeArtifactSha256(result.Artifact);
}
