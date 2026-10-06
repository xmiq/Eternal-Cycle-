using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed partial class CompiledRulesArtifactSqlImportTests
{
    public static IEnumerable<object[]> R2Cases() => OrdinaryAuthorityCandidate.Cases().Select(item => new object[] { item.Id });

    [SqlImportTheory]
    [MemberData(nameof(R2Cases))]
    public async Task R2_RealSqlAndReferenceAgreeForCorrectedOrdinaryCorpus(string id)
    {
        var compiled = OrdinaryAuthorityCandidate.Compile();
        Assert.True(compiled.IsValid);
        var approved = await CompiledRulesImportFixtures.Approve(compiled.Bytes.ToArray());
        var service = await ReadyAsync(approved);
        var fixture = OrdinaryAuthorityCandidate.Cases().Single(item => item.Id == id);
        var measured = CompiledRuleQualityEvidence.Measure(approved.Artifact, fixture);
        if (measured.Failure is { } failure)
        {
            var error = await Assert.ThrowsAsync<CompiledRuleRetrievalException>(() => service.GetContextAsync(measured.Request, default));
            Assert.Equal(failure, error.Failure);
        }
        else
        {
            var actual = await service.GetContextAsync(measured.Request, default);
            Assert.Equal(CompiledRuleEquivalence.Project(measured.Result!), CompiledRuleEquivalence.Project(actual));
            var readback = await Store(approved.Artifact.Ruleset.RulesetId).ReadRuntimeAsync(Scope(approved.Artifact), default);
            Assert.Equal(compiled.Bytes.ToArray(), readback.CopyBytes());
            Assert.Equal(JsonSerializer.Serialize(approved.Artifact), JsonSerializer.Serialize(readback.Artifact));
        }
    }

    [SqlImportFact]
    public async Task R2_HistoricalImportAndPublicationRemainIsolatedWithoutMigration()
    {
        var historicalBytes = CompiledRulesImportFixtures.Canonical();
        var historical = await CompiledRulesImportFixtures.Approve(historicalBytes);
        var service = await ReadyAsync(historical);
        var store = Store(historical.Artifact.Ruleset.RulesetId);
        var historyReceipt = await store.ImportAsync(historical, default);
        var newBytes = OrdinaryAuthorityCandidate.Compile().Bytes.ToArray();
        var corrected = await CompiledRulesImportFixtures.Approve(newBytes);
        var newReceipt = await store.ImportAsync(corrected, default);
        Assert.NotEqual(historyReceipt.ImportId, newReceipt.ImportId);
        Assert.Equal("RULE_PUBLICATION_REQUIRED", (await service.GetPacketAsync(Query(corrected.Artifact), default)).Code);
        // Merely importing R2 leaves historical selection and its known fail-closed
        // dependency behavior untouched. Activation is still separately authorized.
        var oldSave = Query(historical.Artifact, "save", operation: "gameplay.resolve");
        Assert.Equal("RULE_RETRIEVAL_DEPENDENCY_FAILED", (await service.GetPacketAsync(oldSave, default)).Code);
        await service.PublishAsync(Scope(corrected.Artifact), true, null, default);
        await service.ActivateAsync(Scope(corrected.Artifact), true, null, default);
        var newSave = Query(corrected.Artifact, "save", operation: "gameplay.resolve");
        Assert.Equal(CompiledRuleEquivalence.Project(CompiledRuleEquivalence.Reference(corrected.Artifact, newSave)),
            CompiledRuleEquivalence.Project(await service.GetContextAsync(newSave, default)));
        Assert.Equal(historicalBytes, (await store.ReadAsync(historyReceipt.ImportId, default))!.CopyBytes());
        Assert.Equal(newBytes, (await store.ReadAsync(newReceipt.ImportId, default))!.CopyBytes());
        Assert.False((await store.ImportAsync(corrected, default)).Created);
        await service.ActivateAsync(Scope(historical.Artifact), true, null, default);
        Assert.Equal("RULE_RETRIEVAL_DEPENDENCY_FAILED", (await service.GetPacketAsync(oldSave, default)).Code);
        Assert.Equal(2, await CountAsync("imported_rule_artifacts"));
    }
}
