using System.Text;
using EternalCycle.Rules;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class PortableRuleSnippetCompatibilityTests
{
    [Fact]
    public void ManagedCompilerUsesPortableSnippetBoundariesAndNormalization()
    {
        const string content = "# Title\r\n\r\n## First\r\n```markdown\r\n### Not a heading\r\n```\r\n### Nested\r\nRule.\r\n";
        var metadata = new RuleSourceMetadata(
            RuleLayer.Core,
            [],
            [],
            ["*"],
            ["gameplay.resolve"],
            ["rules"],
            Priority: 25,
            AlwaysInclude: true,
            Dependencies: [],
            PreparationTier: RulePreparationTier.ImmediateGameplayCore);
        var document = new RuleSourceDocument("portable", "rules/portable.md", content, metadata);
        var managed = RuleCompiler.Compile("test", [document]).Chunks;

        var bytes = Encoding.UTF8.GetBytes(content);
        var entry = new RuleSourceManifestEntry
        {
            RuleSourceId = document.RuleSourceId,
            Path = document.SourcePath,
            Layer = metadata.Layer,
            WorldModelIds = [.. metadata.WorldModelIds],
            ModuleIds = [.. metadata.ModuleIds],
            CampaignModes = [.. metadata.CampaignModes],
            Operations = [.. metadata.Operations],
            Topics = [.. metadata.Topics],
            Dependencies = [.. metadata.Dependencies ?? []],
            Priority = metadata.Priority,
            AlwaysInclude = metadata.AlwaysInclude,
            PreparationTier = metadata.PreparationTier
        };
        var source = new MaterializedRuleSourceDocument(
            entry,
            bytes,
            content,
            MaterializedRuleSourceLoader.ComputeSha256(bytes));
        var portable = RuleSnippetCompiler.Compile(new(
            "manifest.json",
            "{}"u8.ToArray(),
            MaterializedRuleSourceLoader.ComputeSha256("{}"u8),
            new RuleSourceManifest
            {
                ManifestFormatVersion = 1,
                CompilerContractVersion = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                RulesetId = "test",
                RepositoryVersion = "test",
                Sources = [entry]
            },
            new CompiledRulesSourceIdentity { Scheme = "test", Value = "immutable" },
            new CompiledRulesCompilerIdentity
            {
                ContractVersion = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                ImplementationId = "tests",
                ImplementationVersion = "1"
            },
            [source]));

        Assert.Equal(portable.Select(candidate => candidate.SnippetId), managed.Select(chunk => chunk.ChunkId));
        Assert.Equal(portable.Select(candidate => candidate.SourceAnchor), managed.Select(chunk => chunk.SourceAnchor));
        Assert.Equal(portable.Select(candidate => candidate.Content), managed.Select(chunk => chunk.Content));
        Assert.Equal(portable.Select(candidate => candidate.SourceSha256), managed.Select(chunk => chunk.SourceHash));
        Assert.Equal(portable.Select(candidate => candidate.EstimatedTokens), managed.Select(chunk => chunk.EstimatedTokens));
    }
}
