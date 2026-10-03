using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EternalCycle.Rules.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

// F shares the real pre-011 fixture. The comparison projection intentionally
// includes service evidence omitted from the normal compact packet.
internal static class CompiledRuleEquivalence
{
    internal static CompiledRulePacketResult Reference(CompiledRulesArtifact artifact, CompiledRuleRetrievalRequest request) =>
        CompiledRulePackets.Build(CompiledRuleDependencies.Expand(CompiledRuleRanking.Rank(
            CompiledRuleCandidateIndex.Create(artifact).FindCandidates(CompiledRuleRetrieval.Prepare(request)))));

    internal static string Project(CompiledRulePacketResult result) => JsonSerializer.Serialize(new
    {
        result.Packet, result.Totals, result.Decisions,
        Counts = result.Input.Input.Counts,
        Ranked = result.Input.Input.Candidates,
        Roots = result.Input.Roots.Select(root => new
        {
            root.Root.SnippetId,
            Prerequisites = root.Prerequisites.Select(member => member.SnippetId)
        }),
        Closure = result.Input.Members.Select(member => new
        {
            member.SnippetId, member.RuleSourceId, member.IsRoot, member.IsDependency,
            member.Root, member.RequiredByRuleSourceIds, member.Source, member.Snippet
        }),
        Members = result.Members.Select(member => new
        {
            member.SnippetId, member.RuleSourceId, member.Root, member.IsDependency,
            member.RequiredByRuleSourceIds, member.Member.Snippet
        })
    });
}

public sealed class CompiledRuleRuntimeBoundaryTests
{
    [Fact]
    public void F_RuntimeFailureCodesHaveBoundedSafeRegistryGuidance()
    {
        string[] serviceCodes = ["GM_HOST_CONFIGURATION_REQUIRED", "RULE_ACTIVATION_REQUIRED",
            "RULE_PUBLICATION_REQUIRED", "RULE_RETRIEVAL_UNAUTHORIZED"];
        var retrievalCodes = Enum.GetValues<CompiledRuleRetrievalFailure>()
            .Select(failure => new CompiledRuleRetrievalException(failure).Code);
        foreach (var code in serviceCodes.Concat(retrievalCodes))
        {
            var lookup = EternalCycleErrorRegistry.Get(code);
            Assert.True(lookup.Found);
            Assert.NotNull(lookup.Error);
            Assert.InRange(lookup.Error.SafeDescription.Length, 1, 512);
            Assert.InRange(lookup.Error.RecoveryGuidance.Length, 1, 512);
        }
    }
}

public sealed partial class CompiledRulesArtifactSqlImportTests
{
    private CompiledRuleRuntime Runtime(string ruleset, bool enabled = true, bool admin = true, bool confirmation = false) => new(
        Store(ruleset), new SqlServerGmHostConfigurationStore(options),
        Options.Create(new ManagedRuleServiceOptions { RulesetId = ruleset }),
        Options.Create(new CompiledRuleRuntimeOptions { Enabled = enabled }),
        Options.Create(new ManagedAdministrationOptions { Enabled = admin, RequireOperatorConfirmation = confirmation, ApprovalPhrase = "fixture-consent" }));

    private Task PublicationMigrationAsync() => ExecuteAsync(SqlServerSchemaMigration.RenderDomain(
        File.ReadAllText(MigrationPath("012_compiled_artifact_publication.template.sql")), Domain));

    private static CompiledRuleStoreScope Scope(CompiledRulesArtifact artifact) => new(artifact.Ruleset.RulesetId, artifact.Integrity.ArtifactSha256);
    private static CompiledRuleRetrievalRequest Query(CompiledRulesArtifact artifact, string terms = "", int budget = 8000, string operation = "context.assemble") =>
        new(Scope(artifact), operation, "NORMAL", terms.Length == 0 ? [] : terms.Split('|'))
        {
            Topics = artifact.RuleSources.SelectMany(source => source.Applicability.Topics).Distinct().ToArray(),
            MaximumEstimatedTokens = budget
        };

    private async Task<CompiledRuleRuntime> ReadyAsync(ValidatedTrustedCompiledRulesArtifact approved)
    {
        await MigrateAsync();
        await PublicationMigrationAsync();
        var service = Runtime(approved.Artifact.Ruleset.RulesetId);
        await Store(approved.Artifact.Ruleset.RulesetId).ImportAsync(approved, default);
        await service.PublishAsync(Scope(approved.Artifact), true, null, default);
        await service.ActivateAsync(Scope(approved.Artifact), true, null, default);
        var bootstrap = approved.Artifact.RuleSources.SingleOrDefault(source => source.RuleSourceId == "gm-host-bootstrap");
        if (bootstrap is not null)
            await new SqlServerGmHostConfigurationStore(options).SaveAsync(new(
                approved.Artifact.Ruleset.RulesetId, bootstrap.SourceSha256, GmHostConfigurationState.UserConfirmed,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, 0), default);
        return service;
    }

    [SqlImportTheory]
    [InlineData("fighting", "gameplay.resolve", 8000, 4746, 15, 0)]
    [InlineData("fighting", "gameplay.resolve", 3000, 2985, 13, 2)]
    [InlineData("conflict", "gameplay.resolve", 8000, 4746, 15, 0)]
    [InlineData("conflict", "gameplay.resolve", 3000, 2985, 13, 2)]
    [InlineData("preparation", "gameplay.resolve", 8000, 3430, 14, 0)]
    [InlineData("preparation", "gameplay.resolve", 3000, 2979, 13, 1)]
    [InlineData("campaign canon", "context.assemble", 8000, 1801, 7, 0)]
    [InlineData("campaign canon", "context.assemble", 1400, 1394, 6, 1)]
    [InlineData("unreviewed", "gameplay.resolve", 8000, 2027, 11, 0)]
    [InlineData("", "gameplay.resolve", 8000, 2027, 11, 0)]
    [InlineData("", "gameplay.resolve", 2027, 2027, 11, 0)]
    [InlineData("", "context.assemble", 1010, 1010, 5, 0)]
    public async Task F_CanonicalFullSemanticEquivalence(string term, string operation, int budget, int cost, int roots, int exclusions)
    {
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Canonical());
        var service = await ReadyAsync(approved);
        var request = Query(approved.Artifact, term, budget, operation);
        var expected = CompiledRuleEquivalence.Reference(approved.Artifact, request);
        var actual = await service.GetContextAsync(request, default);
        Assert.Equal(CompiledRuleEquivalence.Project(expected), CompiledRuleEquivalence.Project(actual));
        Assert.Equal(cost, actual.Totals.UsedEstimatedTokens);
        Assert.Equal(budget - cost, actual.Totals.RemainingEstimatedTokens);
        Assert.Equal(roots, actual.Totals.SelectedRoots);
        Assert.Equal(roots, actual.Totals.UniqueMembers);
        Assert.Equal(exclusions, actual.Totals.BudgetExcludedRoots);
        if (term == "conflict") Assert.Equal(5, actual.Input.Input.Counts.InapplicableVocabularyMatched);
        if (term == "campaign canon") Assert.Equal(2, actual.Decisions.Count(decision => decision.Root.VocabularyScore == 900));
        var compact = await new CompiledRuleContextTools(service).GetContextAsync(request, default);
        Assert.True(compact.Success);
        Assert.Equal(JsonSerializer.Serialize(expected.Packet), JsonSerializer.Serialize(compact.Data));
        Assert.DoesNotContain("VocabularyScore", JsonSerializer.Serialize(compact));
    }

    [SqlImportTheory]
    [InlineData("save", "gameplay.resolve", 8000, CompiledRuleRetrievalFailure.DependencyFailed)]
    [InlineData("fighting|conflict|save|preparation", "gameplay.resolve", 8000, CompiledRuleRetrievalFailure.DependencyFailed)]
    [InlineData("", "gameplay.resolve", 2026, CompiledRuleRetrievalFailure.PacketBudgetInsufficient)]
    [InlineData("", "context.assemble", 1009, CompiledRuleRetrievalFailure.PacketBudgetInsufficient)]
    public async Task F_CanonicalFailureEquivalence(string term, string operation, int budget, CompiledRuleRetrievalFailure failure)
    {
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Canonical());
        var service = await ReadyAsync(approved);
        var request = Query(approved.Artifact, term, budget, operation);
        var expected = Assert.Throws<CompiledRuleRetrievalException>(() => CompiledRuleEquivalence.Reference(approved.Artifact, request));
        var actual = await Assert.ThrowsAsync<CompiledRuleRetrievalException>(() => service.GetContextAsync(request, default));
        Assert.Equal(failure, actual.Failure);
        Assert.Equal(expected.Code, actual.Code);
        var response = await service.GetPacketAsync(request, default);
        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal(expected.Code, response.Code);
    }

    [SqlImportTheory]
    [InlineData("direct", 20)]
    [InlineData("direct", 21)]
    [InlineData("direct", 22)]
    [InlineData("chain", 40)]
    [InlineData("chain", 41)]
    [InlineData("chain", 42)]
    [InlineData("shared", 30)]
    [InlineData("shared", 31)]
    [InlineData("shared", 32)]
    [InlineData("diamond", 40)]
    [InlineData("diamond", 41)]
    [InlineData("diamond", 42)]
    [InlineData("root-dependency", 20)]
    [InlineData("root-dependency", 21)]
    [InlineData("root-dependency", 22)]
    public async Task F_SyntheticDependencyAndBudgetEquivalence(string graph, int budget)
    {
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes(graph));
        var service = await ReadyAsync(approved);
        var request = Query(approved.Artifact, "select", budget);
        var expected = CompiledRuleEquivalence.Reference(approved.Artifact, request);
        Assert.Equal(CompiledRuleEquivalence.Project(expected), CompiledRuleEquivalence.Project(await service.GetContextAsync(request, default)));
    }

    [SqlImportFact]
    public async Task F_ReadbackRetainsEveryCanonicalFieldAndExactBytes()
    {
        var bytes = CompiledRulesImportFixtures.Canonical();
        var approved = await CompiledRulesImportFixtures.Approve(bytes);
        await ReadyAsync(approved);
        var read = await Store("eternal-cycle-core").ReadRuntimeAsync(Scope(approved.Artifact), default);
        Assert.Equal(JsonSerializer.Serialize(approved.Artifact), JsonSerializer.Serialize(read.Artifact));
        Assert.Equal(bytes, read.CopyBytes());
        Assert.Equal(10, read.Artifact.RuleSources.Count);
        Assert.Equal(154, read.Artifact.Snippets.Count);
        Assert.Equal(773, read.Artifact.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count));
        Assert.Equal(275622, read.CopyBytes().Length);
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", read.SemanticSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", read.ByteSha256);
    }

    [SqlImportFact]
    public async Task F_ImportPublicationAndActivationAreSeparateAndPreserveLegacy()
    {
        await MigrateAsync();
        await PublicationMigrationAsync();
        var legacy = await LegacySnapshotAsync();
        var context = await CurrentContextAsync();
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes("direct"));
        var scope = Scope(approved.Artifact);
        var service = Runtime(scope.RulesetId);
        await Store(scope.RulesetId).ImportAsync(approved, default);
        Assert.Equal("RULE_PUBLICATION_REQUIRED", (await service.GetPacketAsync(Query(approved.Artifact), default)).Code);
        Assert.Equal("RULE_PUBLICATION_REQUIRED", (await Assert.ThrowsAsync<ManagedServiceException>(() => service.ActivateAsync(scope, true, null, default))).Code);
        await service.PublishAsync(scope, true, null, default);
        Assert.Equal("RULE_ACTIVATION_REQUIRED", (await service.GetPacketAsync(Query(approved.Artifact), default)).Code);
        await service.ActivateAsync(scope, true, null, default);
        Assert.True((await service.GetPacketAsync(Query(approved.Artifact), default)).Success);
        Assert.Equal(legacy, await LegacySnapshotAsync());
        Assert.Equal(context, await CurrentContextAsync());
        Assert.Equal("RULE_RETRIEVAL_UNAUTHORIZED", (await Runtime(scope.RulesetId, enabled: false).GetPacketAsync(Query(approved.Artifact), default)).Code);
        Assert.Equal(context, await CurrentContextAsync());
    }

    [SqlImportFact]
    public async Task F_DenyBeforeSQLAndDistinguishAdministrativeConsent()
    {
        // No 011/012 present. Unauthorized calls must not probe the database.
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes("direct"));
        var request = Query(approved.Artifact);
        foreach (var service in new[] { Runtime("fixture", enabled: false), Runtime("other-rules") })
        {
            var response = await service.GetPacketAsync(request, default);
            Assert.Equal("RULE_RETRIEVAL_UNAUTHORIZED", response.Code);
            Assert.Null(response.Data);
            Assert.DoesNotContain(request.Scope.SemanticSha256, JsonSerializer.Serialize(response));
        }
        Assert.Equal("ADMINISTRATION_DISABLED", (await Assert.ThrowsAsync<ManagedServiceException>(() =>
            Runtime("fixture", admin: false).PublishAsync(request.Scope, true, null, default))).Code);
        Assert.Equal("USER_APPROVAL_REQUIRED", (await Assert.ThrowsAsync<ManagedServiceException>(() =>
            Runtime("fixture").PublishAsync(request.Scope, false, null, default))).Code);
        Assert.Equal("ADMINISTRATIVE_INTERVENTION_REQUIRED", (await Assert.ThrowsAsync<ManagedServiceException>(() =>
            Runtime("fixture", confirmation: true).ActivateAsync(request.Scope, true, "wrong", default))).Code);
        Assert.Equal("MIGRATION_REQUIRED", (await Runtime("fixture").GetPacketAsync(request, default)).Code);
    }

    [SqlImportFact]
    public async Task F_UpgradeRepeatPackagingAndCompositeConstraintsPreserveHistory()
    {
        await MigrateAsync();
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes("direct"));
        var receipt = await Store("fixture").ImportAsync(approved, default);
        var before = await LegacySnapshotAsync();
        var bootstrap = new SqlServerSchemaBootstrapExecutor(options, new ConfiguredCampaignSchemaResolver(options),
            Options.Create(new CompiledRuleRuntimeOptions { Enabled = true }));
        Assert.Equal(new[] { "012_compiled_artifact_publication" }, (await bootstrap.PlanAsync("import-test-only", default)).MigrationIds);
        Assert.Equal(new[] { "012_compiled_artifact_publication" }, await bootstrap.ExecuteAsync("import-test-only", default));
        await PublicationMigrationAsync();
        Assert.Empty((await bootstrap.PlanAsync("import-test-only", default)).MigrationIds);
        Assert.Equal(before, await LegacySnapshotAsync());
        Assert.Equal(approved.CopyBytes(), (await Store("fixture").ReadAsync(receipt.ImportId, default))!.CopyBytes());
        Assert.Equal(File.ReadAllText(MigrationPath("012_compiled_artifact_publication.sql")).ReplaceLineEndings("\n"),
            SqlServerSchemaMigration.RenderDomain(File.ReadAllText(MigrationPath("012_compiled_artifact_publication.template.sql")), "ec_domain").ReplaceLineEndings("\n"));
        await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync($"INSERT INTO [import_domain].published_rule_artifacts (ruleset_id, import_id) VALUES ('wrong', '{receipt.ImportId}');"));
        await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync($"INSERT INTO [import_domain].active_rule_artifacts (ruleset_id, import_id) VALUES ('fixture', '{receipt.ImportId}');"));
    }

    [SqlImportFact]
    public async Task F_HistorySameSourceIdentitiesIdempotencyAndSelectionIsolation()
    {
        var a = await CompiledRulesImportFixtures.Approve(GraphBytes("direct"));
        var service = await ReadyAsync(a);
        var first = await service.GetContextAsync(Query(a.Artifact, "select"), default);
        var changed = CompiledRulesImportFixtures.Change(a.CopyBytes(), node =>
        {
            node["ruleSources"]![0]!["sourceSha256"] = new string('C', 64);
            var snippet = node["snippets"]![0]!;
            snippet["content"] = "Changed historical fixture.";
            snippet["contentSha256"] = CompiledRulesArtifactContract.ComputeContentSha256("Changed historical fixture.");
        });
        var b = await CompiledRulesImportFixtures.Approve(changed);
        var store = Store("fixture");
        var aReceipt = await store.ImportAsync(a, default);
        var bReceipt = await store.ImportAsync(b, default);
        Assert.NotEqual(aReceipt.ImportId, bReceipt.ImportId);
        Assert.Equal(CompiledRuleEquivalence.Project(first), CompiledRuleEquivalence.Project(await service.GetContextAsync(Query(a.Artifact, "select"), default)));
        Assert.Equal("RULE_PUBLICATION_REQUIRED", (await service.GetPacketAsync(Query(b.Artifact), default)).Code);
        await service.PublishAsync(Scope(b.Artifact), true, null, default);
        Assert.Equal("RULE_ACTIVATION_REQUIRED", (await service.GetPacketAsync(Query(b.Artifact), default)).Code);
        await service.ActivateAsync(Scope(b.Artifact), true, null, default);
        var second = await service.GetContextAsync(Query(b.Artifact, "select"), default);
        Assert.Equal(CompiledRuleEquivalence.Project(CompiledRuleEquivalence.Reference(b.Artifact, Query(b.Artifact, "select"))), CompiledRuleEquivalence.Project(second));
        Assert.Contains(second.Packet.Rules, section => section.Content.Contains("Changed historical fixture."));
        Assert.Equal("RULE_ACTIVATION_REQUIRED", (await service.GetPacketAsync(Query(a.Artifact), default)).Code);
        Assert.Equal(a.CopyBytes(), (await store.ReadAsync(aReceipt.ImportId, default))!.CopyBytes());
        Assert.False((await store.ImportAsync(b, default)).Created);
        Assert.Equal(CompiledRuleEquivalence.Project(second), CompiledRuleEquivalence.Project(await service.GetContextAsync(Query(b.Artifact, "select"), default)));
        await service.ActivateAsync(Scope(a.Artifact), true, null, default);
        Assert.Equal(CompiledRuleEquivalence.Project(first), CompiledRuleEquivalence.Project(await service.GetContextAsync(Query(a.Artifact, "select"), default)));
        Assert.Equal(2, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].published_rule_artifacts;"));
    }

    [SqlImportTheory]
    [InlineData("bytes")]
    [InlineData("byte-hash")]
    [InlineData("header")]
    [InlineData("source")]
    [InlineData("terms")]
    [InlineData("dependency")]
    [InlineData("snippet")]
    [InlineData("owner")]
    [InlineData("ordinal")]
    public async Task F_CorruptActiveArtifactFailsWithoutPartialPacketOrLegacyFallback(string damage)
    {
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes("direct"));
        var service = await ReadyAsync(approved);
        var commands = new Dictionary<string, string>
        {
            ["bytes"] = "UPDATE [import_domain].imported_rule_artifacts SET exact_bytes = 0x7B7D;",
            ["byte-hash"] = "UPDATE [import_domain].imported_rule_artifacts SET byte_sha256 = REPLICATE('F',64);",
            ["header"] = "UPDATE [import_domain].imported_rule_artifacts SET header_json = '{}';",
            ["source"] = "UPDATE [import_domain].imported_rule_sources SET source_json = JSON_MODIFY(source_json, '$.sourceSha256', REPLICATE('F',64));",
            ["terms"] = "UPDATE [import_domain].imported_rule_snippets SET snippet_json = JSON_MODIFY(snippet_json, '$.retrieval.terms', JSON_QUERY('[]'));",
            ["dependency"] = "DELETE FROM [import_domain].imported_rule_dependencies;",
            ["snippet"] = "DELETE FROM [import_domain].imported_rule_snippets WHERE snippet_ordinal = 0;",
            ["owner"] = "UPDATE [import_domain].imported_rule_snippets SET source_ordinal = 1 WHERE snippet_ordinal = 0;",
            ["ordinal"] = "UPDATE [import_domain].imported_rule_snippets SET snippet_ordinal = 99 WHERE snippet_ordinal = 0;"
        };
        await ExecuteAsync(commands[damage]);
        var result = await service.GetPacketAsync(Query(approved.Artifact, "select"), default);
        Assert.Equal("RULE_RETRIEVAL_ARTIFACT_INCONSISTENT", result.Code);
        Assert.Null(result.Data);
        Assert.DoesNotContain("SELECT", result.Message);
        Assert.NotNull(await Legacy.GetActiveAsync("eternal-cycle-core", default));
        Assert.Equal("RULE_RETRIEVAL_ARTIFACT_INCONSISTENT", (await Assert.ThrowsAsync<CompiledRuleRetrievalException>(() =>
            service.ActivateAsync(Scope(approved.Artifact), true, null, default))).Code);
    }

    [SqlImportTheory]
    [InlineData("Required", true)]
    [InlineData("InstructionsPresented", true)]
    [InlineData("UserConfirmed", false)]
    [InlineData("Verified", false)]
    public async Task F_HostBootstrapReadinessUsesSelectedArtifactHash(string state, bool blocked)
    {
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Canonical());
        var service = await ReadyAsync(approved);
        await ExecuteAsync($"UPDATE [import_domain].gm_host_configurations SET configuration_state = '{state}';");
        var request = Query(approved.Artifact, "", 8000, "gameplay.resolve");
        var result = await service.GetPacketAsync(request, default);
        Assert.Equal(!blocked, result.Success);
        if (blocked) Assert.Equal("GM_HOST_CONFIGURATION_REQUIRED", result.Code);
        await ExecuteAsync("UPDATE [import_domain].gm_host_configurations SET bootstrap_source_hash = REPLICATE('F',64);");
        Assert.Equal("GM_HOST_CONFIGURATION_REQUIRED", (await service.GetPacketAsync(request, default)).Code);
        Assert.True((await service.GetPacketAsync(Query(approved.Artifact, "", 8000, "context.assemble"), default)).Success);
    }

    [SqlImportFact]
    public async Task F_AbsentWrongRulesetAndMissingRuntimeMigrationAreSafe()
    {
        await MigrateAsync();
        await PublicationMigrationAsync();
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes("direct"));
        var service = Runtime("fixture");
        Assert.Equal("RULE_RETRIEVAL_ARTIFACT_UNAVAILABLE", (await service.GetPacketAsync(Query(approved.Artifact), default)).Code);
        Assert.Equal("RULE_RETRIEVAL_UNAUTHORIZED", (await Runtime("other").GetPacketAsync(Query(approved.Artifact), default)).Code);
        await Store("fixture").ImportAsync(approved, default);
        await ExecuteAsync("DROP TABLE [import_domain].active_rule_artifacts; DROP TABLE [import_domain].published_rule_artifacts;");
        Assert.Equal("MIGRATION_REQUIRED", (await service.GetPacketAsync(Query(approved.Artifact), default)).Code);
    }

    [SqlImportFact]
    public async Task F_ProviderConvergenceAndDisappearanceRequireNoRuntimeAcquisition()
    {
        var bytes = CompiledRulesImportFixtures.Canonical();
        var initial = await CompiledRulesImportFixtures.Approve(bytes);
        var service = await ReadyAsync(initial);
        var before = CompiledRuleEquivalence.Project(await service.GetContextAsync(Query(initial.Artifact, "fighting", operation: "gameplay.resolve"), default));
        var path = Path.GetTempFileName();
        File.WriteAllBytes(path, bytes);
        try
        {
            using var transport = new ReleaseTransport(bytes);
            using var github = new GitHubReleaseCompiledRulesArtifactProvider("fixture", "rules", "v-fixture", "rules.json", transport: transport);
            foreach (var provider in new ICompiledRulesArtifactProvider[] { new LocalCompiledRulesArtifactProvider(path), github, MemoryProvider(bytes) })
            {
                var acquired = await provider.AcquireAsync(new(), default);
                var approved = (await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new CompiledRulesImportFixtures.Policy())).TrustedArtifact!;
                var receipt = await Store("eternal-cycle-core").ImportAsync(approved, default);
                Assert.False(receipt.Created);
                Assert.Equal(CompiledRulesArtifactImport.CreateImportId(initial.Artifact), receipt.ImportId);
            }
        }
        finally { File.Delete(path); }
        Assert.Equal(before, CompiledRuleEquivalence.Project(await service.GetContextAsync(Query(initial.Artifact, "fighting", operation: "gameplay.resolve"), default)));
        Assert.Equal(1, await CountAsync("imported_rule_artifacts"));
    }

    [SqlImportFact]
    public async Task F_ConcurrentReadsAndUnrelatedImportRemainDeterministicAndImmutable()
    {
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes("diamond"));
        var service = await ReadyAsync(approved);
        var request = Query(approved.Artifact, "select");
        var expected = CompiledRuleEquivalence.Project(CompiledRuleEquivalence.Reference(approved.Artifact, request));
        var changed = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Change(approved.CopyBytes(), node => node["compiler"]!["implementationVersion"] = "2"));
        var import = Store("fixture").ImportAsync(changed, default);
        var reads = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => service.GetContextAsync(request, default)));
        await import;
        Assert.All(reads, result => Assert.Equal(expected, CompiledRuleEquivalence.Project(result)));
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.Equal(expected, CompiledRuleEquivalence.Project(await service.GetContextAsync(request, default)));
        }
        finally { CultureInfo.CurrentCulture = oldCulture; }
        Assert.Throws<NotSupportedException>(() => ((IList)reads[0].Packet.Rules).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList)reads[0].Members).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList)reads[0].Decisions[0].Root.Matches).Clear());
        Assert.Equal(expected, CompiledRuleEquivalence.Project(await service.GetContextAsync(request, default)));
    }

    [SqlImportFact]
    public async Task F_ReadCancellationWhileSQLIsBlockedReturnsNoPartialPacket()
    {
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes("direct"));
        var service = await ReadyAsync(approved);
        await using var connection = new SqlConnection(Connection(database));
        await connection.OpenAsync();
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        await using var command = new SqlCommand("SELECT import_id FROM [import_domain].imported_rule_artifacts WITH (TABLOCKX, HOLDLOCK);", connection, transaction);
        await command.ExecuteScalarAsync();
        using var cancellation = new CancellationTokenSource();
        var blocked = service.GetPacketAsync(Query(approved.Artifact), cancellation.Token);
        await Task.Delay(150);
        cancellation.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => blocked);
        await transaction.RollbackAsync();
        Assert.True((await service.GetPacketAsync(Query(approved.Artifact), default)).Success);
        using var preCancelled = new CancellationTokenSource();
        preCancelled.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => service.GetPacketAsync(Query(approved.Artifact), preCancelled.Token));
    }

    [SqlImportTheory]
    [InlineData("world", true)]
    [InlineData("world", false)]
    [InlineData("module", true)]
    [InlineData("module", false)]
    [InlineData("mode", true)]
    [InlineData("mode", false)]
    [InlineData("operation", true)]
    [InlineData("operation", false)]
    [InlineData("topic", true)]
    [InlineData("topic", false)]
    public async Task F_DependencySelectorsRoundtripWithoutSQLSpecificFallback(string selector, bool matches)
    {
        var bytes = CompiledRulesImportFixtures.Change(GraphBytes("direct"), node =>
        {
            var metadata = node["ruleSources"]![1]!["applicability"]!;
            metadata[selector switch { "world" => "worldModelIds", "module" => "moduleIds", "mode" => "campaignModes", "operation" => "operations", _ => "topics" }] =
                new JsonArray(selector switch { "mode" => "NORMAL", "operation" => "context.assemble", _ => "fixture-selection" });
        });
        var approved = await CompiledRulesImportFixtures.Approve(bytes);
        var service = await ReadyAsync(approved);
        var request = Query(approved.Artifact, "select") with
        {
            WorldModelId = matches ? "fixture-selection" : null,
            ModuleIds = matches ? ["fixture-selection"] : [],
            CampaignMode = matches ? "NORMAL" : "DEBUG",
            Operation = matches ? "context.assemble" : "fixture.inspect",
            Topics = [] // Dependency topics intentionally do not rematch the query.
        };
        if (matches || selector == "topic")
            Assert.Equal(CompiledRuleEquivalence.Project(CompiledRuleEquivalence.Reference(approved.Artifact, request)),
                CompiledRuleEquivalence.Project(await service.GetContextAsync(request, default)));
        else
        {
            var expected = Assert.Throws<CompiledRuleRetrievalException>(() => CompiledRuleEquivalence.Reference(approved.Artifact, request));
            Assert.Equal(CompiledRuleRetrievalFailure.DependencyFailed, expected.Failure);
            Assert.Equal(expected.Code, (await service.GetPacketAsync(request, default)).Code);
        }
    }

    [SqlImportFact]
    public async Task F_ReinsertedSnippetRowsAreReadInArtifactOrdinalOrder()
    {
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes("diamond"));
        var service = await ReadyAsync(approved);
        await ExecuteAsync("""
            SELECT * INTO #reorder FROM [import_domain].imported_rule_snippets;
            DELETE FROM [import_domain].imported_rule_snippets;
            INSERT INTO [import_domain].imported_rule_snippets
                SELECT TOP (1000000) * FROM #reorder ORDER BY snippet_ordinal DESC;
            DROP TABLE #reorder;
            """);
        var request = Query(approved.Artifact, "select");
        Assert.Equal(CompiledRuleEquivalence.Project(CompiledRuleEquivalence.Reference(approved.Artifact, request)),
            CompiledRuleEquivalence.Project(await service.GetContextAsync(request, default)));
    }

    [SqlImportFact]
    public async Task F_RepeatedAndConcurrentPublicationIsIdempotent()
    {
        var approved = await CompiledRulesImportFixtures.Approve(GraphBytes("direct"));
        var service = await ReadyAsync(approved);
        var scope = Scope(approved.Artifact);
        var receipts = await Task.WhenAll(service.PublishAsync(scope, true, null, default), service.PublishAsync(scope, true, null, default));
        Assert.Equal(receipts[0], receipts[1]);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].published_rule_artifacts;"));
        await Task.WhenAll(service.ActivateAsync(scope, true, null, default), service.ActivateAsync(scope, true, null, default));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].active_rule_artifacts;"));
        Assert.True((await service.GetPacketAsync(Query(approved.Artifact), default)).Success);
    }

    [SqlImportFact]
    public async Task F_ServiceSchemaReadinessTracksOptedInPublicationMigration()
    {
        await MigrateAsync();
        var rules = Options.Create(new ManagedRuleServiceOptions());
        var admin = Options.Create(new ManagedAdministrationOptions());
        var route = new ConfiguredCampaignSchemaResolver(options);
        var legacy = new SqlServerManagedInfrastructureInspector(options, rules, admin, route);
        var compiled = new SqlServerManagedInfrastructureInspector(options, rules, admin, route,
            compiledRules: Options.Create(new CompiledRuleRuntimeOptions { Enabled = true }));
        Assert.Equal(ManagedComponentStatus.Ready, (await legacy.InspectAsync(null, default)).RuleDomainSchema);
        Assert.Equal(ManagedComponentStatus.Outdated, (await compiled.InspectAsync(null, default)).RuleDomainSchema);
        await PublicationMigrationAsync();
        Assert.Equal(ManagedComponentStatus.Ready, (await compiled.InspectAsync(null, default)).RuleDomainSchema);
    }

    private static byte[] GraphBytes(string graph)
    {
        var edges = graph switch
        {
            "chain" => new Dictionary<string, string[]> { ["a"] = ["b"], ["b"] = ["c"], ["c"] = ["d"], ["d"] = [] },
            "shared" => new() { ["a"] = ["c"], ["b"] = ["c"], ["c"] = [] },
            "diamond" => new() { ["a"] = ["b", "c"], ["b"] = ["d"], ["c"] = ["d"], ["d"] = [] },
            _ => new() { ["a"] = ["b"], ["b"] = [] }
        };
        return CompiledRulesImportFixtures.Change(CompiledRulesImportFixtures.Representative(), node =>
        {
            var kernelSource = node["ruleSources"]![1]!.DeepClone();
            kernelSource["applicability"]!["topics"] = new JsonArray();
            kernelSource["applicability"]!["operations"] = new JsonArray("*");
            var kernelSnippet = node["snippets"]![1]!.DeepClone();
            kernelSnippet["retrieval"] = JsonNode.Parse("{\"terms\":[],\"relationships\":[]}");
            kernelSnippet["estimatedTokens"] = 1;
            var sources = new JsonArray();
            var snippets = new JsonArray();
            foreach (var (id, dependencies) in edges.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            {
                var source = kernelSource.DeepClone();
                source["ruleSourceId"] = id;
                source["sourcePath"] = "rules/" + id + ".md";
                source["applicability"]!["layer"] = "Core";
                source["applicability"]!["alwaysInclude"] = false;
                source["dependencyRuleSourceIds"] = new JsonArray(dependencies.Select(value => (JsonNode?)JsonValue.Create(value)).ToArray());
                sources.Add(source);
                var snippet = kernelSnippet.DeepClone();
                snippet["snippetId"] = id;
                snippet["ruleSourceId"] = id;
                snippet["sourceAnchor"] = null;
                snippet["estimatedTokens"] = 10;
                if (id == "a" || (id == "b" && graph is "shared" or "root-dependency"))
                    snippet["retrieval"] = JsonNode.Parse("{\"terms\":[{\"term\":\"select\",\"kind\":\"selection\",\"weight\":900}],\"relationships\":[]}");
                snippets.Add(snippet);
            }
            sources.Add(kernelSource);
            snippets.Add(kernelSnippet);
            node["ruleset"]!["rulesetId"] = "fixture";
            node["ruleSources"] = sources;
            node["snippets"] = snippets;
        });
    }
}
