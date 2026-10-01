using System.Text.Json.Nodes;

namespace EternalCycle.Rules.Testing;

internal sealed class VocabularyTestPayload : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "EternalCycle.Vocabulary.Integration", Guid.NewGuid().ToString("N"));

    public VocabularyTestPayload()
    {
        Directory.CreateDirectory(Root);
        foreach (var file in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "VocabularyFixture")))
        {
            File.Copy(file, Path.Combine(Root, Path.GetFileName(file)));
        }
    }

    public JsonObject ReadManifest() => JsonNode.Parse(File.ReadAllBytes(Path.Combine(Root, "manifest.json")))!.AsObject();

    public void WriteManifest(JsonObject manifest) => File.WriteAllText(Path.Combine(Root, "manifest.json"), manifest.ToJsonString());

    public MaterializedRuleSourceSnapshot Load() => MaterializedRuleSourceLoader.Load(new(Root, "manifest.json",
        new() { Scheme = "test-source", Value = "immutable-source" },
        new() { ContractVersion = "1", ImplementationId = "eternal-cycle-dotnet", ImplementationVersion = "1.0.0" }));

    public RuleCompilationResult Compile() => RuleCompilationPipeline.Compile(Load());

    public void Dispose() => Directory.Delete(Root, recursive: true);
}
