using System.Text;
using System.Text.Json.Nodes;

namespace EternalCycle.Rules.Testing;

internal static class CompiledRulesImportFixtures
{
    internal static byte[] Representative()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "ArtifactFixture", "valid-representative.json");
        if (!File.Exists(path)) path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "CompiledRulesArtifact", "valid-representative.json");
        return File.ReadAllBytes(path);
    }
    internal static byte[] Canonical()
    {
        using var payload = new CanonicalVocabularyPayload();
        return RuleCompilationPipeline.Compile(payload.Load()).Bytes.ToArray();
    }
    internal static byte[] Change(byte[] bytes, Action<JsonObject> edit)
    {
        var node = JsonNode.Parse(bytes)!.AsObject();
        edit(node);
        var model = CompiledRulesArtifactContract.Read(node.ToJsonString()).Artifact!;
        node["integrity"]!["artifactSha256"] = CompiledRulesArtifactContract.ComputeArtifactSha256(model);
        var result = Encoding.UTF8.GetBytes(node.ToJsonString());
        if (!CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(result)).IsValid)
            throw new InvalidOperationException("Invalid import fixture mutation.");
        return result;
    }
    internal static async Task<ValidatedTrustedCompiledRulesArtifact> Approve(byte[] bytes, string provider = "fixture")
    {
        var result = await CompiledRulesArtifactValidation.ValidateAsync(new(bytes, new(provider), new()), new(), new Policy());
        return result.TrustedArtifact ?? throw new InvalidOperationException("Fixture approval failed.");
    }
    internal sealed class Policy : ICompiledRulesArtifactTrustPolicy
    {
        public ValueTask<CompiledRulesArtifactTrustDecision> EvaluateAsync(ValidatedCompiledRulesArtifact artifact, CancellationToken cancellationToken) =>
            ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason.ExplicitApproval));
    }
}
