using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRulesArtifactImportTests
{
    [Fact]
    public void WriteSurfaceCannotAcceptUntrustedOrProviderSpecificInput()
    {
        var method = typeof(ICompiledRulesArtifactImportStore).GetMethod("ImportAsync")!;
        Assert.Equal(typeof(ValidatedTrustedCompiledRulesArtifact), method.GetParameters()[0].ParameterType);
        Assert.Empty(typeof(ValidatedTrustedCompiledRulesArtifact).GetConstructors());
        Assert.DoesNotContain(typeof(CompiledRulesArtifactImport).GetMethods(), method => method.Name == "ImportAsync" && method.GetParameters()[0].ParameterType != typeof(ValidatedTrustedCompiledRulesArtifact));
    }
    [Theory]
    [InlineData("")]
    [InlineData("other-rules")]
    public async Task InvalidScopeMakesNoStorageCall(string scope)
    {
        var store = new MemoryStore();
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative());
        Assert.Equal(CompiledRulesImportFailure.InputInvalid, (await Assert.ThrowsAsync<CompiledRulesImportException>(() => CompiledRulesArtifactImport.ImportAsync(approved, scope, store))).Failure);
        Assert.Equal(0, store.Calls);
    }
    [Fact]
    public async Task NullAndPrecancelledInputDoNotWrite()
    {
        var store = new MemoryStore();
        await Assert.ThrowsAsync<CompiledRulesImportException>(() => CompiledRulesArtifactImport.ImportAsync(null!, "fixture-rules", store));
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative());
        await Assert.ThrowsAsync<OperationCanceledException>(() => CompiledRulesArtifactImport.ImportAsync(approved, "fixture-rules", store, new(true)));
        Assert.Equal(0, store.Calls);
    }
    [Fact]
    public async Task SequentialAndFormattingOnlyImportsReuseSemanticIdentityAndFirstBytes()
    {
        var bytes = CompiledRulesImportFixtures.Representative();
        var first = await CompiledRulesImportFixtures.Approve(bytes);
        var reformatted = await CompiledRulesImportFixtures.Approve(Encoding.UTF8.GetBytes(JsonNode.Parse(bytes)!.ToJsonString()));
        Assert.NotEqual(first.ByteSha256, reformatted.ByteSha256);
        var store = new MemoryStore();
        var receipt = await CompiledRulesArtifactImport.ImportAsync(first, "fixture-rules", store);
        var repeat = await CompiledRulesArtifactImport.ImportAsync(reformatted, "fixture-rules", store);
        Assert.True(receipt.Created);
        Assert.Equal(receipt with { Created = false }, repeat);
        Assert.Single(store.Items);
        Assert.Equal(bytes, (await store.ReadAsync(receipt.ImportId, default))!.CopyBytes());
    }
    [Fact]
    public async Task CanonicalRoundtripRetainsAll773TermsAndCompleteModel()
    {
        var bytes = CompiledRulesImportFixtures.Canonical();
        var approved = await CompiledRulesImportFixtures.Approve(bytes);
        var store = new MemoryStore();
        var receipt = await CompiledRulesArtifactImport.ImportAsync(approved, approved.Artifact.Ruleset.RulesetId, store);
        var stored = (await store.ReadAsync(receipt.ImportId, default))!;
        Assert.Equal(10, receipt.SourceCount);
        Assert.Equal(154, receipt.SnippetCount);
        Assert.Equal(773, receipt.RetrievalTermCount);
        Assert.Equal(275622, bytes.Length);
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", receipt.SemanticSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", receipt.RetainedByteSha256);
        Assert.True(CompiledRulesArtifactImport.Equivalent(approved.Artifact, stored.Artifact));
        Assert.Equal(bytes, stored.CopyBytes());
    }
    [Fact]
    public async Task EvidenceAndCultureDoNotEnterIdentityAndApprovalCannotDrift()
    {
        var bytes = CompiledRulesImportFixtures.Representative();
        var approved = await CompiledRulesImportFixtures.Approve(bytes, "custom-one");
        var second = await CompiledRulesImportFixtures.Approve(bytes, "custom-two");
        var id = CompiledRulesArtifactImport.CreateImportId(approved.Artifact);
        var culture = CultureInfo.CurrentCulture;
        try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR"); Assert.Equal(id, CompiledRulesArtifactImport.CreateImportId(second.Artifact)); }
        finally { CultureInfo.CurrentCulture = culture; }
        Array.Fill(approved.CopyBytes(), (byte)0);
        Array.Fill(bytes, (byte)0);
        Assert.Throws<NotSupportedException>(() => approved.Artifact.RuleSources.Clear());
        Assert.Throws<NotSupportedException>(() => approved.Artifact.Snippets[0].Retrieval.Terms.Clear());
        var store = new MemoryStore();
        var receipt = await CompiledRulesArtifactImport.ImportAsync(approved, "fixture-rules", store);
        var stored = (await store.ReadAsync(receipt.ImportId, default))!;
        Array.Fill(stored.CopyBytes(), (byte)0);
        Assert.Equal(approved.CopyBytes(), stored.CopyBytes());
        Assert.Throws<NotSupportedException>(() => stored.Artifact.Snippets.Clear());
    }
    [Theory]
    [InlineData("id")]
    [InlineData("bytes")]
    [InlineData("hash")]
    [InlineData("projection")]
    public async Task CorruptReadbackNeverUsesKeyMatchAsProof(string kind)
    {
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative());
        var bytes = approved.CopyBytes();
        var json = Encoding.UTF8.GetString(bytes);
        var id = CompiledRulesArtifactImport.CreateImportId(approved.Artifact);
        var hash = approved.ByteSha256;
        switch (kind)
        {
            case "id": id = "ARTIFACT-" + new string('0', 64); break;
            case "bytes": bytes[0] = 0; break;
            case "hash": hash = new string('0', 64); break;
            default: json = Encoding.UTF8.GetString(CompiledRulesImportFixtures.Change(bytes, node => node["compiler"]!["implementationVersion"] = "2")); break;
        }
        Assert.Equal(CompiledRulesImportFailure.IntegrityConflict, Assert.Throws<CompiledRulesImportException>(() => CompiledRulesArtifactImport.VerifyReadback(id, json, bytes, hash)).Failure);
    }
    [Theory]
    [InlineData(CompiledRulesImportFailure.InputInvalid)]
    [InlineData(CompiledRulesImportFailure.IntegrityConflict)]
    [InlineData(CompiledRulesImportFailure.SchemaIncompatible)]
    [InlineData(CompiledRulesImportFailure.StorageFailed)]
    public void FixedFailureTaxonomyIsBoundedAndHasNoInnerException(CompiledRulesImportFailure failure)
    {
        var error = new CompiledRulesImportException(failure);
        Assert.StartsWith("ARTIFACT_", error.Code);
        Assert.True(error.Message.Length < 200);
        Assert.Null(error.InnerException);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StoreFailureIsSanitizedWhileCancellationRemainsCancellation(bool cancel)
    {
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative());
        var store = new MemoryStore { Failure = cancel ? new OperationCanceledException("private-secret") : new IOException("private-secret") };
        var error = await Assert.ThrowsAnyAsync<Exception>(() => CompiledRulesArtifactImport.ImportAsync(approved, "fixture-rules", store));
        Assert.DoesNotContain("private-secret", error.ToString());
        if (cancel) Assert.IsType<OperationCanceledException>(error);
        else Assert.Equal(CompiledRulesImportFailure.StorageFailed, Assert.IsType<CompiledRulesImportException>(error).Failure);
        Assert.Empty(store.Items);
    }
    [Fact]
    public async Task ChangedArtifactsWithTheSameSourceIdsRetainIndependentHistory()
    {
        var bytes = CompiledRulesImportFixtures.Representative();
        var a = await CompiledRulesImportFixtures.Approve(bytes);
        var b = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Change(bytes, node => node["ruleSources"]![0]!["sourceSha256"] = new string('A', 64)));
        var store = new MemoryStore();
        var first = await store.ImportAsync(a, default);
        var second = await store.ImportAsync(b, default);
        Assert.NotEqual(first.ImportId, second.ImportId);
        Assert.Equal(2, store.Items.Count);
        Assert.True(CompiledRulesArtifactImport.Equivalent(a.Artifact, (await store.ReadAsync(first.ImportId, default))!.Artifact));
        Assert.True(CompiledRulesArtifactImport.Equivalent(b.Artifact, (await store.ReadAsync(second.ImportId, default))!.Artifact));
    }
    private sealed class MemoryStore : ICompiledRulesArtifactImportStore
    {
        internal Dictionary<string, StoredCompiledRulesArtifact> Items { get; } = new(StringComparer.Ordinal);
        internal Exception? Failure { get; init; }
        internal int Calls { get; private set; }
        public Task<CompiledRulesImportReceipt> ImportAsync(ValidatedTrustedCompiledRulesArtifact approved, CancellationToken token)
        {
            Calls++;
            token.ThrowIfCancellationRequested();
            if (Failure is not null) throw Failure;
            var id = CompiledRulesArtifactImport.CreateImportId(approved.Artifact);
            var created = !Items.TryGetValue(id, out var stored);
            if (created)
            {
                stored = CompiledRulesArtifactImport.VerifyReadback(id, Encoding.UTF8.GetString(approved.CopyBytes()), approved.CopyBytes(), approved.ByteSha256);
                Items.Add(id, stored);
            }
            else if (!CompiledRulesArtifactImport.Equivalent(approved.Artifact, stored!.Artifact)) throw new CompiledRulesImportException(CompiledRulesImportFailure.IntegrityConflict);
            return Task.FromResult(CompiledRulesArtifactImport.Receipt(stored!, created));
        }
        public Task<StoredCompiledRulesArtifact?> ReadAsync(string id, CancellationToken token) => Task.FromResult(Items.GetValueOrDefault(id));
    }
}
