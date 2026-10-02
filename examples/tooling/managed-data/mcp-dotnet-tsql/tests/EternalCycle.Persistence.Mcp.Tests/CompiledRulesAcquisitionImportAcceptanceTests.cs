using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

// Reuse F's disposable pre-011 database and failure hooks; G tests composition,
// not another storage/provider implementation or runtime import endpoint.
public sealed partial class CompiledRulesArtifactSqlImportTests
{
    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task IntegratedCanonicalProvidersRetryAfterUpgradeWithoutPublishingOrChangingCurrentContext()
    {
        var active = (await Legacy.GetActiveAsync("eternal-cycle-core", default))!;
        var historical = active with
        {
            RuleReleaseId = "RULE-fixture-history", SourceIdentity = "fixture-history", State = RuleReleaseState.Candidate,
            Index = RuleCompiler.Compile("fixture-version",
            [
                new("history-base", "rules/base.md", "# Base\n\nHistorical fixture foundation.", new(RuleLayer.Core, [], [], ["*"], ["fixture.inspect"], [])),
                new("history-dependent", "rules/dependent.md", "# Dependent\n\nHistorical dependent fixture.", new(RuleLayer.Core, [], [], ["*"], ["fixture.inspect"], [], Dependencies: ["history-base"]))
            ])
        };
        await Legacy.StageCandidateAsync(historical, default);
        await Legacy.SetStateAsync(historical.RuleReleaseId, RuleReleaseState.Published, null, default);
        Assert.True(await ScalarAsync("SELECT COUNT(*) FROM [import_domain].rule_dependencies;") > 0);
        var legacyBefore = await LegacySnapshotAsync();
        var contextBefore = await CurrentContextAsync();
        var bootstrap = new SqlServerSchemaBootstrapExecutor(options, new ConfiguredCampaignSchemaResolver(options));
        Assert.Equal(new[] { "011_compiled_artifact_import" }, await bootstrap.ExecuteAsync("import-test-only", default));
        Assert.Empty((await bootstrap.PlanAsync("import-test-only", default)).MigrationIds);
        await MigrateAsync();
        Assert.Equal(legacyBefore, await LegacySnapshotAsync());

        var bytes = CompiledRulesImportFixtures.Canonical();
        var path = Path.GetTempFileName();
        File.WriteAllBytes(path, bytes);
        try
        {
            using var transport = new ReleaseTransport(bytes);
            using var github = new GitHubReleaseCompiledRulesArtifactProvider("fixture", "rules", "v-fixture", "rules.json", transport: transport);
            var local = new LocalCompiledRulesArtifactProvider(path);
            ICompiledRulesArtifactProvider[] providers = [local, local, github, MemoryProvider(bytes)];
            var store = new CountingImportStore(Store("eternal-cycle-core"));
            CompiledRulesImportReceipt? first = null;
            CompiledRulesArtifact? approvedModel = null;
            var providerKinds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var provider in providers)
            {
                var acquired = await provider.AcquireAsync(new(), default);
                Assert.Equal(bytes, acquired.Bytes.ToArray());
                Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)), acquired.ByteSha256);
                providerKinds.Add(acquired.Evidence.ProviderKind);
                var approval = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new CompiledRulesImportFixtures.Policy());
                Assert.True(approval.IsImportEligible);
                var approved = approval.TrustedArtifact!;
                Assert.Equal(bytes, approved.CopyBytes());
                approvedModel ??= approved.Artifact;
                Assert.Equal(JsonSerializer.Serialize(approvedModel), JsonSerializer.Serialize(approved.Artifact));
                var receipt = await CompiledRulesArtifactImport.ImportAsync(approved, "eternal-cycle-core", store);
                if (first is null) { Assert.True(receipt.Created); first = receipt; }
                else Assert.Equal(first with { Created = false }, receipt);
                var readback = (await store.ReadAsync(receipt.ImportId, default))!;
                // Full ordered model equality covers provenance, selectors, terms,
                // kind/concept identities, weights, relationships and dependencies.
                Assert.Equal(JsonSerializer.Serialize(approved.Artifact), JsonSerializer.Serialize(readback.Artifact));
                Assert.Equal(bytes, readback.CopyBytes());
                Assert.Equal(acquired.ByteSha256, readback.ByteSha256);
            }
            Assert.Equal(3, providerKinds.Count);
            Assert.Equal(4, store.ImportCalls);
            Assert.Equal(10, first!.SourceCount);
            Assert.Equal(154, first.SnippetCount);
            Assert.Equal(773, first.RetrievalTermCount);
            Assert.Equal(275622, bytes.Length);
            Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", first.SemanticSha256);
            Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", first.RetainedByteSha256);
            Assert.Equal(1, await CountAsync("imported_rule_artifacts"));
            Assert.Equal(10, await CountAsync("imported_rule_sources"));
            Assert.Equal(154, await CountAsync("imported_rule_snippets"));
            Assert.Equal(773, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].imported_rule_snippets CROSS APPLY OPENJSON(snippet_json, '$.retrieval.terms');"));
            Assert.Equal(approvedModel!.RuleSources.Sum(source => source.DependencyRuleSourceIds.Count), await CountAsync("imported_rule_dependencies"));
            Assert.Equal(legacyBefore, await LegacySnapshotAsync());
            Assert.Equal(contextBefore, await CurrentContextAsync());
        }
        finally { File.Delete(path); }
    }

    [SqlImportTheory]
    [InlineData("local")]
    [InlineData("github")]
    [InlineData("custom")]
    [Trait("Category", "SqlServerIntegration")]
    public async Task IntegratedMalformedAcquisitionNeverReachesPolicyOrImporter(string providerKind)
    {
        await MigrateAsync();
        var before = await LegacySnapshotAsync();
        // Match the existing simulated Release asset size while deliberately
        // supplying malformed JSON; acquisition succeeds without semantic trust.
        var bytes = Encoding.UTF8.GetBytes("{" + new string(' ', 275621));
        var path = Path.GetTempFileName();
        File.WriteAllBytes(path, bytes);
        try
        {
            using var transport = new ReleaseTransport(bytes);
            using var github = new GitHubReleaseCompiledRulesArtifactProvider("fixture", "rules", "v-fixture", "rules.json", transport: transport);
            ICompiledRulesArtifactProvider provider = providerKind switch
            {
                "local" => new LocalCompiledRulesArtifactProvider(path),
                "github" => github,
                _ => MemoryProvider(bytes)
            };
            var store = new CountingImportStore(Store());
            var policy = new AcceptancePolicy();
            var (validation, receipt) = await AcquireApproveImportAsync(provider, store, policy);
            Assert.False(validation.IsArtifactValid);
            Assert.False(validation.IsImportEligible);
            Assert.Null(receipt);
            Assert.Equal(0, policy.Calls);
            Assert.Equal(0, store.ImportCalls);
            Assert.InRange(validation.Diagnostics.Count, 1, CompiledRulesArtifactValidation.MaximumDiagnostics);
            Assert.DoesNotContain(path, JsonSerializer.Serialize(validation));
            await AssertNoImportsAsync();
            Assert.Equal(before, await LegacySnapshotAsync());
        }
        finally { File.Delete(path); }
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task IntegratedEvidenceSensitiveRejectionDoesNotChangeArtifactOrOtherProviderImports()
    {
        await MigrateAsync();
        var bytes = CompiledRulesImportFixtures.Canonical();
        var before = await LegacySnapshotAsync();
        var store = new CountingImportStore(Store("eternal-cycle-core"));
        var policy = new AcceptancePolicy(artifact => artifact.AcquisitionEvidence.ProviderKind != "conformance-memory-store");
        var rejected = await AcquireApproveImportAsync(MemoryProvider(bytes), store, policy, "eternal-cycle-core");
        Assert.True(rejected.Validation.IsArtifactValid);
        Assert.False(rejected.Validation.IsImportEligible);
        Assert.Equal(CompiledRulesArtifactTrustOutcome.Rejected, rejected.Validation.TrustDecision!.Outcome);
        Assert.Null(rejected.Receipt);
        Assert.Equal(0, store.ImportCalls);
        await AssertNoImportsAsync();
        Assert.Equal(before, await LegacySnapshotAsync());

        var path = Path.GetTempFileName();
        File.WriteAllBytes(path, bytes);
        try
        {
            using var transport = new ReleaseTransport(bytes);
            using var github = new GitHubReleaseCompiledRulesArtifactProvider("fixture", "rules", "v-fixture", "rules.json", transport: transport);
            var local = await AcquireApproveImportAsync(new LocalCompiledRulesArtifactProvider(path), store, policy, "eternal-cycle-core");
            var remote = await AcquireApproveImportAsync(github, store, policy, "eternal-cycle-core");
            Assert.True(local.Receipt!.Created);
            Assert.Equal(local.Receipt with { Created = false }, remote.Receipt);
            var rejectedArtifact = rejected.Validation.ValidatedArtifact!;
            Assert.Equal(rejectedArtifact.ByteSha256, local.Validation.TrustedArtifact!.ByteSha256);
            Assert.Equal(JsonSerializer.Serialize(rejectedArtifact.Artifact), JsonSerializer.Serialize(remote.Validation.TrustedArtifact!.Artifact));
            Assert.Equal(1, await CountAsync("imported_rule_artifacts"));
            Assert.Equal(before, await LegacySnapshotAsync());
        }
        finally { File.Delete(path); }
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task IntegratedChangedSourceHistoryRemainsArtifactOwned()
    {
        await MigrateAsync();
        var before = await LegacySnapshotAsync();
        var bytes = CompiledRulesImportFixtures.Representative();
        var changed = CompiledRulesImportFixtures.Change(bytes, node =>
        {
            node["ruleSources"]![0]!["sourceSha256"] = new string('A', 64);
            node["snippets"]![0]!["content"] = "Changed fixture content.";
            node["snippets"]![0]!["contentSha256"] = CompiledRulesArtifactContract.ComputeContentSha256("Changed fixture content.");
        });
        var store = new CountingImportStore(Store());
        var first = await AcquireApproveImportAsync(MemoryProvider(bytes), store, new AcceptancePolicy());
        var second = await AcquireApproveImportAsync(MemoryProvider(changed), store, new AcceptancePolicy());
        Assert.NotEqual(first.Receipt!.ImportId, second.Receipt!.ImportId);
        var oldRead = (await store.ReadAsync(first.Receipt.ImportId, default))!;
        var newRead = (await store.ReadAsync(second.Receipt.ImportId, default))!;
        Assert.Equal(bytes, oldRead.CopyBytes());
        Assert.Equal(changed, newRead.CopyBytes());
        Assert.Equal(oldRead.Artifact.RuleSources[0].RuleSourceId, newRead.Artifact.RuleSources[0].RuleSourceId);
        Assert.NotEqual(oldRead.Artifact.RuleSources[0].SourceSha256, newRead.Artifact.RuleSources[0].SourceSha256);
        Assert.NotEqual(oldRead.Artifact.Snippets[0].ContentSha256, newRead.Artifact.Snippets[0].ContentSha256);
        Assert.Equal(JsonSerializer.Serialize(first.Validation.TrustedArtifact!.Artifact), JsonSerializer.Serialize(oldRead.Artifact));
        Assert.Equal(JsonSerializer.Serialize(second.Validation.TrustedArtifact!.Artifact), JsonSerializer.Serialize(newRead.Artifact));
        Assert.Equal(2, await CountAsync("imported_rule_artifacts"));
        Assert.Equal(4, await CountAsync("imported_rule_sources"));
        Assert.Equal(4, await CountAsync("imported_rule_snippets"));
        Assert.Equal(before, await LegacySnapshotAsync());
    }

    [SqlImportTheory]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Category", "SqlServerIntegration")]
    public async Task IntegratedApprovedFailureOrCancellationRollsBackWithoutDisclosure(bool cancel)
    {
        await MigrateAsync();
        var original = await AcquireApproveImportAsync(MemoryProvider(CompiledRulesImportFixtures.Representative()), new CountingImportStore(Store()), new AcceptancePolicy());
        var legacyBefore = await LegacySnapshotAsync();
        var changed = CompiledRulesImportFixtures.Change(original.Validation.TrustedArtifact!.CopyBytes(), node => node["compiler"]!["implementationVersion"] = "2");
        using var cancellation = new CancellationTokenSource();
        var reached = false;
        var store = new CountingImportStore(Store(after: stage =>
        {
            if (stage != ArtifactImportStage.SnippetsAndRetrieval) return;
            reached = true;
            if (cancel) cancellation.Cancel();
            else throw new IOException("private-sql-details-and-token");
        }));
        var approved = (await CompiledRulesArtifactValidation.ValidateAsync(await MemoryProvider(changed).AcquireAsync(new(), default), new(), new AcceptancePolicy())).TrustedArtifact!;
        if (cancel)
            await Assert.ThrowsAsync<OperationCanceledException>(() => CompiledRulesArtifactImport.ImportAsync(approved, "fixture-rules", store, cancellation.Token));
        else
        {
            var error = await Assert.ThrowsAsync<CompiledRulesImportException>(() => CompiledRulesArtifactImport.ImportAsync(approved, "fixture-rules", store));
            Assert.Equal(CompiledRulesImportFailure.StorageFailed, error.Failure);
            Assert.DoesNotContain("private-sql-details-and-token", error.ToString());
            Assert.Null(error.InnerException);
        }
        Assert.True(reached);
        Assert.Equal(1, store.ImportCalls);
        Assert.Null(await Store().ReadAsync(CompiledRulesArtifactImport.CreateImportId(approved.Artifact), default));
        Assert.Equal(1, await CountAsync("imported_rule_artifacts"));
        Assert.Equal(2, await CountAsync("imported_rule_sources"));
        Assert.Equal(2, await CountAsync("imported_rule_snippets"));
        Assert.Equal(1, await CountAsync("imported_rule_dependencies"));
        Assert.Equal(original.Validation.TrustedArtifact.CopyBytes(), (await Store().ReadAsync(original.Receipt!.ImportId, default))!.CopyBytes());
        Assert.Equal(legacyBefore, await LegacySnapshotAsync());
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task IntegratedOfflineApprovedArtifactSurvivesSourceAndProviderDisappearance()
    {
        await MigrateAsync();
        byte[] bytes;
        string sourceRoot;
        using (var payload = new CanonicalVocabularyPayload())
        {
            sourceRoot = payload.Root;
            bytes = RuleCompilationPipeline.Compile(payload.Load()).Bytes.ToArray();
        }
        Assert.False(Directory.Exists(sourceRoot));
        var path = Path.GetTempFileName();
        File.WriteAllBytes(path, bytes);
        var provider = new LocalCompiledRulesArtifactProvider(path);
        ValidatedTrustedCompiledRulesArtifact approved;
        try
        {
            approved = (await CompiledRulesArtifactValidation.ValidateAsync(await provider.AcquireAsync(new(), default), new(), new AcceptancePolicy())).TrustedArtifact!;
        }
        finally { File.Delete(path); }
        await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(new(), default));
        Array.Clear(bytes); // The approved buffer is independent of caller memory.
        var receipt = await CompiledRulesArtifactImport.ImportAsync(approved, "eternal-cycle-core", Store("eternal-cycle-core"));
        var stored = (await Store("eternal-cycle-core").ReadAsync(receipt.ImportId, default))!;
        Assert.Equal(approved.CopyBytes(), stored.CopyBytes());
        Assert.True(CompiledRulesArtifactImport.Equivalent(approved.Artifact, stored.Artifact));
        Assert.Equal(773, receipt.RetrievalTermCount);
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task IntegratedUnauthorizedScopeCannotReachStorage()
    {
        await MigrateAsync();
        var before = await LegacySnapshotAsync();
        var store = new CountingImportStore(Store());
        var approval = (await CompiledRulesArtifactValidation.ValidateAsync(await MemoryProvider(CompiledRulesImportFixtures.Representative()).AcquireAsync(new(), default), new(), new AcceptancePolicy())).TrustedArtifact!;
        var error = await Assert.ThrowsAsync<CompiledRulesImportException>(() => CompiledRulesArtifactImport.ImportAsync(approval, "other-ruleset", store));
        Assert.Equal(CompiledRulesImportFailure.InputInvalid, error.Failure);
        Assert.Equal(0, store.ImportCalls);
        // The configured SQL boundary also independently enforces its scope.
        Assert.Equal(CompiledRulesImportFailure.InputInvalid, (await Assert.ThrowsAsync<CompiledRulesImportException>(() => Store("other-ruleset").ImportAsync(approval, default))).Failure);
        await AssertNoImportsAsync();
        Assert.Equal(before, await LegacySnapshotAsync());
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task IntegratedPrivateEvidenceAndPolicyExceptionRemainOutsideDiagnostics()
    {
        await MigrateAsync();
        var before = await LegacySnapshotAsync();
        const string secret = "private-path-token-signed-url";
        var bytes = CompiledRulesImportFixtures.Representative();
        var provider = new MemoryStoreCompiledRulesArtifactProvider("private/object", "opaque-version", () => new MemoryStream(bytes, false),
            [KeyValuePair.Create("privateLocator", "https://fixture.invalid/private?token=" + secret)]);
        var store = new CountingImportStore(Store());
        var policy = new AcceptancePolicy(_ => throw new IOException(secret));
        var (validation, receipt) = await AcquireApproveImportAsync(provider, store, policy);
        Assert.True(validation.IsArtifactValid);
        Assert.False(validation.IsImportEligible);
        Assert.Equal(CompiledRulesArtifactTrustOutcome.Failed, validation.TrustDecision!.Outcome);
        Assert.Null(receipt);
        Assert.Equal(0, store.ImportCalls);
        var diagnostic = JsonSerializer.Serialize(validation) + validation;
        Assert.DoesNotContain(secret, diagnostic);
        Assert.DoesNotContain("private/object", diagnostic);
        Assert.DoesNotContain("opaque-version", diagnostic);
        Assert.DoesNotContain(Encoding.UTF8.GetString(bytes), diagnostic);
        Assert.InRange(diagnostic.Length, 1, 1024);
        await AssertNoImportsAsync();
        Assert.Equal(before, await LegacySnapshotAsync());
    }

    private static MemoryStoreCompiledRulesArtifactProvider MemoryProvider(byte[] bytes) =>
        new("rules/current", "fixture-version", () => new MemoryStream(bytes, false));

    private static async Task<(CompiledRulesArtifactValidationResult Validation, CompiledRulesImportReceipt? Receipt)> AcquireApproveImportAsync(
        ICompiledRulesArtifactProvider provider, CountingImportStore store, AcceptancePolicy policy, string ruleset = "fixture-rules")
    {
        var acquired = await provider.AcquireAsync(new(), default);
        var validation = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy);
        var receipt = validation.TrustedArtifact is { } approved
            ? await CompiledRulesArtifactImport.ImportAsync(approved, ruleset, store) : null;
        return (validation, receipt);
    }

    private async Task AssertNoImportsAsync()
    {
        foreach (var table in SqlServerCompiledRulesArtifactStore.Tables) Assert.Equal(0, await CountAsync(table));
    }

    private async Task<string> CurrentContextAsync()
    {
        // Match the deliberately non-product legacy fixture version, not imported
        // provenance. This exercises the existing public provider, not FR-026.
        var routing = Options.Create(new SqlServerPersistenceOptions { DefaultRulesetVersion = "fixture-version" });
        var provider = new PublishedRuleContextProvider(Legacy, Options.Create(new ManagedRuleServiceOptions()), new ConfiguredCampaignSchemaResolver(routing));
        return JsonSerializer.Serialize(await provider.GetContextAsync(new("import-test-only", "fixture.inspect", [], "default"), default));
    }

    private async Task<string[]> LegacySnapshotAsync()
    {
        (string Table, string Order)[] tables =
        [
            ("rulesets", "ruleset_id"), ("rule_releases", "rule_release_id"),
            ("rule_chunks", "rule_release_id, chunk_id"),
            ("rule_chunk_selectors", "rule_release_id, chunk_id, selector_type, selector_value"),
            ("rule_dependencies", "rule_release_id, source_chunk_id, required_chunk_id"),
            ("active_rule_releases", "ruleset_id"), ("rule_update_checks", "update_check_id")
        ];
        await using var connection = new SqlConnection(Connection(database));
        await connection.OpenAsync();
        var snapshots = new List<string>();
        foreach (var (table, order) in tables)
        {
            // Only the fixed fixture-owned table/key list is interpolated. Scalar
            // wrapping prevents FOR JSON's large-result row fragmentation.
            await using var command = new SqlCommand($"SELECT COALESCE((SELECT * FROM [import_domain].[{table}] ORDER BY {order} FOR JSON PATH, INCLUDE_NULL_VALUES), N'[]');", connection);
            snapshots.Add((string)(await command.ExecuteScalarAsync())!);
        }
        return snapshots.ToArray();
    }

    private sealed class AcceptancePolicy(Func<ValidatedCompiledRulesArtifact, bool>? accept = null) : ICompiledRulesArtifactTrustPolicy
    {
        public int Calls { get; private set; }
        public ValueTask<CompiledRulesArtifactTrustDecision> EvaluateAsync(ValidatedCompiledRulesArtifact artifact, CancellationToken cancellationToken)
        {
            Calls++;
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(accept?.Invoke(artifact) != false
                ? CompiledRulesArtifactTrustReason.ExplicitApproval : CompiledRulesArtifactTrustReason.ExplicitRejection));
        }
    }

    private sealed class CountingImportStore(ICompiledRulesArtifactImportStore inner) : ICompiledRulesArtifactImportStore
    {
        public int ImportCalls { get; private set; }
        public Task<CompiledRulesImportReceipt> ImportAsync(ValidatedTrustedCompiledRulesArtifact approved, CancellationToken cancellationToken)
        {
            ImportCalls++;
            return inner.ImportAsync(approved, cancellationToken);
        }
        public Task<StoredCompiledRulesArtifact?> ReadAsync(string importId, CancellationToken cancellationToken) => inner.ReadAsync(importId, cancellationToken);
    }
}
