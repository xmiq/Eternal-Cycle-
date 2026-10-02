using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using EternalCycle.Rules.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class SqlImportFactAttribute : FactAttribute
{
    public SqlImportFactAttribute() { if (Environment.GetEnvironmentVariable("ETERNAL_CYCLE_RUN_SQL_INTEGRATION") != "1") Skip = "Opt-in disposable LocalDB import integration."; }
}
public sealed class SqlImportTheoryAttribute : TheoryAttribute
{
    public SqlImportTheoryAttribute() { if (Environment.GetEnvironmentVariable("ETERNAL_CYCLE_RUN_SQL_INTEGRATION") != "1") Skip = "Opt-in disposable LocalDB import integration."; }
}

public sealed class CompiledRulesArtifactImportPackagingTests
{
    [Fact]
    public void DefaultMigrationEqualsRoutedTemplateAndBothArePackaged()
    {
        var template = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Schema", "011_compiled_artifact_import.template.sql"));
        var defaultSql = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Schema", "011_compiled_artifact_import.sql"));
        Assert.Equal(defaultSql.ReplaceLineEndings("\n"), SqlServerSchemaMigration.RenderDomain(template, "ec_domain").ReplaceLineEndings("\n"));
        Assert.DoesNotContain("{{schema", SqlServerSchemaMigration.RenderDomain(template, "import_domain"));
    }
}

public sealed partial class CompiledRulesArtifactSqlImportTests : IAsyncLifetime
{
    private readonly string database = "EC_Import_" + Guid.NewGuid().ToString("N");
    private const string Domain = "import_domain";
    private IOptions<SqlServerPersistenceOptions> options = null!;
    private static readonly Regex Batches = new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private SqlServerPublishedRuleStore Legacy => new(options);
    private SqlServerCompiledRulesArtifactStore Store(string ruleset = "fixture-rules", Action<ArtifactImportStage>? after = null) => new(options, ruleset, after);
    private static string MigrationPath(string name) => Path.Combine(AppContext.BaseDirectory, "Schema", name);

    public async Task InitializeAsync()
    {
        await ExecuteAtAsync(Connection("master"), $"CREATE DATABASE [{database}];");
        options = Options.Create(new SqlServerPersistenceOptions { ConnectionString = Connection(database), DomainSchema = Domain, DefaultSchema = "ec_fixture" });
        var route = new ConfiguredCampaignSchemaResolver(options).Resolve("import-test-only");
        await ExecuteAsync(SqlServerSchemaMigration.Render(File.ReadAllText(MigrationPath("001_initial.template.sql")), route));
        foreach (var file in new[] { "002_rule_domain", "004_rule_source_configuration", "005_managed_operation_diagnostics", "006_rule_source_compatibility", "007_durable_managed_operations", "008_diagnostic_operation_correlation", "009_managed_operation_execution_leases", "010_gm_host_configuration" })
            await ExecuteAsync(SqlServerSchemaMigration.RenderDomain(File.ReadAllText(MigrationPath(file + ".template.sql")), Domain));
        var release = new PublishedRuleRelease("RULE-fixture-active", "eternal-cycle-core", "fixture", "fixture-source", "fixture-version", "1",
            RuleReleaseState.Candidate, RuleCompiler.Compile("fixture-version", [new RuleSourceDocument("runtime-kernel", "rules/kernel.md", "# Kernel\n\nRepository Canon remains authoritative.",
                new(RuleLayer.RuntimeKernel, [], [], ["*"], ["*"], [], AlwaysInclude: true))]), DateTimeOffset.UtcNow);
        await Legacy.StageCandidateAsync(release, default);
        await Legacy.SetStateAsync(release.RuleReleaseId, RuleReleaseState.Validated, null, default);
        await Legacy.SetStateAsync(release.RuleReleaseId, RuleReleaseState.Published, null, default);
        await Legacy.ActivateAsync(release.RulesetId, release.RuleReleaseId, default);
    }

    public async Task DisposeAsync()
    {
        SqlConnection.ClearAllPools();
        await ExecuteAtAsync(Connection("master"), $"ALTER DATABASE [{database}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{database}];");
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task UpgradeIsAdditiveRepeatSafeAndPreservesLegacyPublicationAndActiveState()
    {
        var before = JsonSerializer.Serialize(await Legacy.GetActiveAsync("eternal-cycle-core", default));
        var importer = Store();
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative());
        Assert.Equal(CompiledRulesImportFailure.SchemaIncompatible, (await Assert.ThrowsAsync<CompiledRulesImportException>(() => importer.ImportAsync(approved, default))).Failure);
        var bootstrap = new SqlServerSchemaBootstrapExecutor(options, new ConfiguredCampaignSchemaResolver(options));
        Assert.Equal(new[] { "011_compiled_artifact_import" }, (await bootstrap.PlanAsync("import-test-only", default)).MigrationIds);
        Assert.Equal(new[] { "011_compiled_artifact_import" }, await bootstrap.ExecuteAsync("import-test-only", default));
        Assert.Empty((await bootstrap.PlanAsync("import-test-only", default)).MigrationIds);
        await MigrateAsync(); // SQL itself, not merely the planner, is repeat-safe.
        Assert.Equal(before, JsonSerializer.Serialize(await Legacy.GetActiveAsync("eternal-cycle-core", default)));
        var receipt = await importer.ImportAsync(approved, default);
        Assert.True(receipt.Created);
        Assert.Equal(before, JsonSerializer.Serialize(await Legacy.GetActiveAsync("eternal-cycle-core", default)));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].rule_releases;"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM sys.key_constraints WHERE name = 'UQ_ec_domain_rule_releases_source';"));
        // Existing source compilation can still stage/publish/activate after import.
        var next = (await Legacy.GetActiveAsync("eternal-cycle-core", default))! with { RuleReleaseId = "RULE-next", SourceIdentity = "fixture-next-source", State = RuleReleaseState.Candidate };
        await Legacy.StageCandidateAsync(next, default);
        await Legacy.SetStateAsync(next.RuleReleaseId, RuleReleaseState.Published, null, default);
        await Legacy.ActivateAsync(next.RulesetId, next.RuleReleaseId, default);
        Assert.Equal("RULE-next", (await Legacy.GetActiveAsync(next.RulesetId, default))!.RuleReleaseId);
        Assert.Equal(RuleReleaseState.Published, (await Legacy.GetByIdAsync(next.RulesetId, "RULE-fixture-active", default))!.State);
        Assert.NotNull(await importer.ReadAsync(receipt.ImportId, default));
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task CanonicalCDEImportsConvergeAndRoundtripEveryNormativeField()
    {
        await MigrateAsync();
        var bytes = CompiledRulesImportFixtures.Canonical();
        var path = Path.Combine(Path.GetTempPath(), "EC-import-matrix-" + Guid.NewGuid().ToString("N"));
        File.WriteAllBytes(path, bytes);
        try
        {
            using var http = new ReleaseTransport(bytes);
            using var github = new GitHubReleaseCompiledRulesArtifactProvider("fixture", "rules", "v-fixture", "rules.json", transport: http);
            ICompiledRulesArtifactProvider[] providers = [new LocalCompiledRulesArtifactProvider(path), github,
                new MemoryStoreCompiledRulesArtifactProvider("rules/current", "fixture-version", () => new MemoryStream(bytes, false))];
            CompiledRulesImportReceipt? first = null;
            var store = Store("eternal-cycle-core");
            foreach (var provider in providers)
            {
                var acquired = await provider.AcquireAsync(new(), default);
                var approval = (await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new CompiledRulesImportFixtures.Policy())).TrustedArtifact!;
                var receipt = await CompiledRulesArtifactImport.ImportAsync(approval, "eternal-cycle-core", store);
                if (first is null) { Assert.True(receipt.Created); first = receipt; }
                else Assert.Equal(first with { Created = false }, receipt);
                var stored = (await store.ReadAsync(receipt.ImportId, default))!;
                Assert.True(CompiledRulesArtifactImport.Equivalent(approval.Artifact, stored.Artifact));
                Assert.Equal(JsonSerializer.Serialize(approval.Artifact), JsonSerializer.Serialize(stored.Artifact));
                Assert.Equal(bytes, stored.CopyBytes());
                Assert.Equal(773, receipt.RetrievalTermCount);
                Assert.Equal(10, receipt.SourceCount);
                Assert.Equal(154, receipt.SnippetCount);
                Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", receipt.SemanticSha256);
                Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", receipt.RetainedByteSha256);
            }
            Assert.Equal(275622, bytes.Length);
            Assert.Equal(1, await CountAsync("imported_rule_artifacts"));
            Assert.Equal(10, await CountAsync("imported_rule_sources"));
            Assert.Equal(154, await CountAsync("imported_rule_snippets"));
            Assert.Equal("RULE-fixture-active", (await Legacy.GetActiveAsync("eternal-cycle-core", default))!.RuleReleaseId);
        }
        finally { File.Delete(path); }
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ReformattingAndRepeatedImportsKeepFirstExactBytesAndNoDuplicateChildren()
    {
        await MigrateAsync();
        var bytes = CompiledRulesImportFixtures.Representative();
        var approved = await CompiledRulesImportFixtures.Approve(bytes);
        var differentBytes = await CompiledRulesImportFixtures.Approve(Encoding.UTF8.GetBytes(JsonNode.Parse(bytes)!.ToJsonString()), "other-provider");
        var store = Store();
        var first = await store.ImportAsync(approved, default);
        Assert.Equal(first with { Created = false }, await store.ImportAsync(approved, default));
        Assert.Equal(first with { Created = false }, await store.ImportAsync(differentBytes, default));
        Assert.Equal(bytes, (await store.ReadAsync(first.ImportId, default))!.CopyBytes());
        Assert.Equal(1, await CountAsync("imported_rule_artifacts"));
        Assert.Equal(2, await CountAsync("imported_rule_sources"));
        Assert.Equal(2, await CountAsync("imported_rule_snippets"));
        Assert.Equal(1, await CountAsync("imported_rule_dependencies"));
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ChangedSourceContentCoexistsUnderRepeatedSourceIdsWithoutRewritingHistory()
    {
        await MigrateAsync();
        var bytes = CompiledRulesImportFixtures.Representative();
        var changed = CompiledRulesImportFixtures.Change(bytes, node =>
        {
            node["ruleSources"]![0]!["sourceSha256"] = new string('A', 64);
            var snippet = node["snippets"]![0]!;
            snippet["content"] = "Changed fixture rule content.";
            snippet["contentSha256"] = CompiledRulesArtifactContract.ComputeContentSha256("Changed fixture rule content.");
        });
        var a = await CompiledRulesImportFixtures.Approve(bytes);
        var b = await CompiledRulesImportFixtures.Approve(changed);
        var store = Store();
        var first = await store.ImportAsync(a, default);
        var second = await store.ImportAsync(b, default);
        Assert.NotEqual(first.ImportId, second.ImportId);
        Assert.True(CompiledRulesArtifactImport.Equivalent(a.Artifact, (await store.ReadAsync(first.ImportId, default))!.Artifact));
        Assert.True(CompiledRulesArtifactImport.Equivalent(b.Artifact, (await store.ReadAsync(second.ImportId, default))!.Artifact));
        Assert.Equal(4, await CountAsync("imported_rule_sources"));
        Assert.Equal("RULE-fixture-active", (await Legacy.GetActiveAsync("eternal-cycle-core", default))!.RuleReleaseId);
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task TermsKindsWeightsRelationshipsEmptyMetadataSelectorsAndDependenciesAreLossless()
    {
        await MigrateAsync();
        var bytes = CompiledRulesImportFixtures.Change(CompiledRulesImportFixtures.Representative(), node =>
        {
            node["snippets"]![0]!["retrieval"]!["terms"]!.AsArray().Insert(1, JsonNode.Parse("{\"term\":\"past life\",\"kind\":\"phrase\",\"weight\":333}"));
            node["snippets"]![1]!["retrieval"]!["terms"] = JsonNode.Parse("[{\"term\":\"past life\",\"kind\":\"alternative\",\"weight\":9}]");
            node["snippets"]!.AsArray().Add(new JsonObject
            {
                ["snippetId"] = "runtime-kernel#empty", ["ruleSourceId"] = "runtime-kernel", ["sourceAnchor"] = "empty",
                ["content"] = "Empty metadata fixture.", ["contentSha256"] = CompiledRulesArtifactContract.ComputeContentSha256("Empty metadata fixture."),
                ["estimatedTokens"] = 6, ["retrieval"] = new JsonObject { ["terms"] = new JsonArray(), ["relationships"] = new JsonArray() }
            });
        });
        var approved = await CompiledRulesImportFixtures.Approve(bytes);
        var store = Store();
        var receipt = await store.ImportAsync(approved, default);
        var stored = (await store.ReadAsync(receipt.ImportId, default))!;
        Assert.Equal(4, receipt.RetrievalTermCount);
        Assert.Equal(1, receipt.RetrievalRelationshipCount);
        Assert.Empty(stored.Artifact.Snippets[2].Retrieval.Terms);
        Assert.Equal(JsonSerializer.Serialize(approved.Artifact), JsonSerializer.Serialize(stored.Artifact));
        Assert.Equal(new[] { "runtime-kernel" }, stored.Artifact.RuleSources[0].DependencyRuleSourceIds);
        Assert.Equal(new[] { "gameplay.resolve", "reincarnation.resolve" }, stored.Artifact.RuleSources[0].Applicability.Operations);
    }

    [SqlImportTheory]
    [InlineData("Header")]
    [InlineData("Sources")]
    [InlineData("SnippetsAndRetrieval")]
    [InlineData("Dependencies")]
    [InlineData("Verified")]
    [Trait("Category", "SqlServerIntegration")]
    public async Task AnyInjectedStageFailureRollsBackNewImportAndPreservesPriorHistory(string stage)
    {
        await MigrateAsync();
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative());
        var first = await Store().ImportAsync(approved, default);
        var changed = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Change(approved.CopyBytes(), node => node["compiler"]!["implementationVersion"] = "2"));
        var store = Store(after: current => { if (current.ToString() == stage) throw new IOException("secret-connection-details"); });
        var error = await Assert.ThrowsAsync<CompiledRulesImportException>(() => store.ImportAsync(changed, default));
        Assert.Equal(CompiledRulesImportFailure.StorageFailed, error.Failure);
        Assert.DoesNotContain("secret-connection-details", error.ToString());
        Assert.Null(error.InnerException);
        Assert.Null(await Store().ReadAsync(CompiledRulesArtifactImport.CreateImportId(changed.Artifact), default));
        Assert.Equal(1, await CountAsync("imported_rule_artifacts"));
        Assert.Equal(2, await CountAsync("imported_rule_sources"));
        Assert.Equal(2, await CountAsync("imported_rule_snippets"));
        Assert.Equal(1, await CountAsync("imported_rule_dependencies"));
        Assert.True(CompiledRulesArtifactImport.Equivalent(approved.Artifact, (await Store().ReadAsync(first.ImportId, default))!.Artifact));
        Assert.Equal("RULE-fixture-active", (await Legacy.GetActiveAsync("eternal-cycle-core", default))!.RuleReleaseId);
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task CancellationRollsBackAndDoesNotBecomeStorageFailure()
    {
        await MigrateAsync();
        using var cancellation = new CancellationTokenSource();
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative());
        var store = Store(after: stage => { if (stage == ArtifactImportStage.SnippetsAndRetrieval) cancellation.Cancel(); });
        await Assert.ThrowsAsync<OperationCanceledException>(() => store.ImportAsync(approved, cancellation.Token));
        foreach (var table in SqlServerCompiledRulesArtifactStore.Tables) Assert.Equal(0, await CountAsync(table));
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ConcurrentImportsConvergeWithoutDuplicateChildrenOrRawUniqueError()
    {
        await MigrateAsync();
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative());
        var results = await Task.WhenAll(Store().ImportAsync(approved, default), Store().ImportAsync(approved, default));
        Assert.Single(results, result => result.Created);
        Assert.Equal(results[0].ImportId, results[1].ImportId);
        Assert.Equal(1, await CountAsync("imported_rule_artifacts"));
        Assert.Equal(2, await CountAsync("imported_rule_sources"));
        Assert.Equal(2, await CountAsync("imported_rule_snippets"));
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task ProjectionCorruptionFailsReadbackAndIdempotentImportWithoutOverwrite()
    {
        await MigrateAsync();
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative());
        var store = Store();
        var receipt = await store.ImportAsync(approved, default);
        await ExecuteAsync("UPDATE [import_domain].imported_rule_sources SET source_json = JSON_MODIFY(source_json, '$.sourceSha256', REPLICATE('A', 64)) WHERE source_ordinal = 0;");
        Assert.Equal(CompiledRulesImportFailure.IntegrityConflict, (await Assert.ThrowsAsync<CompiledRulesImportException>(() => store.ReadAsync(receipt.ImportId, default))).Failure);
        Assert.Equal(CompiledRulesImportFailure.IntegrityConflict, (await Assert.ThrowsAsync<CompiledRulesImportException>(() => store.ImportAsync(approved, default))).Failure);
        Assert.Equal(1, await CountAsync("imported_rule_artifacts"));
        Assert.Equal("RULE-fixture-active", (await Legacy.GetActiveAsync("eternal-cycle-core", default))!.RuleReleaseId);
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task SqlConstraintsRejectOrphanAndCrossArtifactOwnership()
    {
        await MigrateAsync();
        await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync("INSERT INTO [import_domain].imported_rule_snippets VALUES ('missing', 0, 0, '{}');"));
        await Store().ImportAsync(await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative()), default);
        var smaller = CompiledRulesImportFixtures.Change(CompiledRulesImportFixtures.Representative(), node =>
        {
            node["ruleSources"]!.AsArray().RemoveAt(0);
            node["snippets"]!.AsArray().RemoveAt(0);
        });
        var receipt = await Store().ImportAsync(await CompiledRulesImportFixtures.Approve(smaller), default);
        // Source ordinal 1 exists in the first artifact, but not in this one.
        await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync($"INSERT INTO [import_domain].imported_rule_snippets VALUES ('{receipt.ImportId}', 99, 1, '{{}}');"));
        await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync($"INSERT INTO [import_domain].imported_rule_dependencies VALUES ('{receipt.ImportId}', 0, 99, 1);"));
        Assert.Equal(1, await CountAsync("imported_rule_dependencies"));
    }

    [SqlImportFact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task OrdinalSourceIdentitiesStayDistinctUnderDefaultCaseInsensitiveDatabaseCollation()
    {
        await MigrateAsync();
        var bytes = CompiledRulesImportFixtures.Change(CompiledRulesImportFixtures.Representative(), node =>
        {
            var source = node["ruleSources"]![1]!.DeepClone();
            source["ruleSourceId"] = "Runtime-Kernel";
            node["ruleSources"]!.AsArray().Insert(0, source);
            var snippet = node["snippets"]![1]!.DeepClone();
            snippet["snippetId"] = "Runtime-Kernel";
            snippet["ruleSourceId"] = "Runtime-Kernel";
            node["snippets"]!.AsArray().Insert(0, snippet);
        });
        var approved = await CompiledRulesImportFixtures.Approve(bytes);
        var store = Store();
        var receipt = await store.ImportAsync(approved, default);
        Assert.Equal(3, receipt.SourceCount);
        Assert.True(CompiledRulesArtifactImport.Equivalent(approved.Artifact, (await store.ReadAsync(receipt.ImportId, default))!.Artifact));
    }

    private Task MigrateAsync() => ExecuteAsync(SqlServerSchemaMigration.RenderDomain(File.ReadAllText(MigrationPath("011_compiled_artifact_import.template.sql")), Domain));
    private Task ExecuteAsync(string sql) => ExecuteAtAsync(Connection(database), sql);
    private async Task<int> ScalarAsync(string sql)
    {
        await using var connection = new SqlConnection(Connection(database));
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }
    private Task<int> CountAsync(string table)
    {
        Assert.Contains(table, SqlServerCompiledRulesArtifactStore.Tables);
        return ScalarAsync($"SELECT COUNT(*) FROM [import_domain].[{table}];");
    }
    private static async Task ExecuteAtAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        foreach (var batch in Batches.Split(sql).Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            await using var command = new SqlCommand(batch, connection) { CommandTimeout = 30 };
            await command.ExecuteNonQueryAsync();
        }
    }
    private static string Connection(string database) => new SqlConnectionStringBuilder
    { DataSource = @"(localdb)\MSSQLLocalDB", InitialCatalog = database, IntegratedSecurity = true, TrustServerCertificate = true, ConnectTimeout = 10 }.ConnectionString;

    private sealed class ReleaseTransport(byte[] bytes) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            HttpContent content = request.RequestUri!.AbsolutePath switch
            {
                "/repos/fixture/rules" => new StringContent("{\"id\":10,\"full_name\":\"fixture/rules\"}", Encoding.UTF8, "application/json"),
                "/repos/fixture/rules/releases/tags/v-fixture" => new StringContent("{\"id\":20,\"tag_name\":\"v-fixture\",\"draft\":false,\"prerelease\":false,\"published_at\":\"2026-01-01T00:00:00Z\"}", Encoding.UTF8, "application/json"),
                "/repos/fixture/rules/releases/20/assets" => new StringContent("[{\"id\":30,\"name\":\"rules.json\",\"size\":275622,\"state\":\"uploaded\",\"url\":\"https://api.github.com/repos/fixture/rules/releases/assets/30\"}]", Encoding.UTF8, "application/json"),
                "/repos/fixture/rules/releases/assets/30" => new ByteArrayContent(bytes),
                _ => throw new InvalidOperationException("Unexpected fixture request.")
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }
}
