using System.Text.RegularExpressions;
using EternalCycle.Persistence.Mcp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class PreMigrationControlPlaneIntegrationTests
{
    private const string RunIntegrationVariable = "ETERNAL_CYCLE_RUN_SQL_INTEGRATION";
    private static readonly Regex BatchSeparator = new(
        @"^\s*GO\s*$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task OfficialRuleCandidateStagesPublishesAndActivatesAtomically()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(RunIntegrationVariable),
                "1",
                StringComparison.Ordinal))
        {
            return;
        }

        var databaseName = $"EC_RuleStage_{Guid.NewGuid():N}";
        var masterConnectionString = ConnectionString("master");
        var databaseConnectionString = ConnectionString(databaseName);
        await CreateDatabaseAsync(masterConnectionString, databaseName);
        try
        {
            var persistence = Options.Create(new SqlServerPersistenceOptions
            {
                ConnectionString = databaseConnectionString,
                DomainSchema = "ec_domain",
                DefaultSchema = "ec"
            });
            var resolver = new ConfiguredCampaignSchemaResolver(persistence);
            var bootstrap = new SqlServerSchemaBootstrapExecutor(persistence, resolver);
            await bootstrap.ExecuteAsync("rule-stage-fixture", CancellationToken.None);

            var repositoryRoot = FindRepositoryRoot();
            var provider = new GitRuleSourceProvider(Options.Create(new ManagedRuleServiceOptions
            {
                GitSource = new GitRuleSourceOptions
                {
                    RepositoryRoot = repositoryRoot,
                    Ref = "HEAD",
                    ManifestPath = "docs/rules/rule-source-manifest.json"
                }
            }));
            var snapshot = await provider.GetSnapshotAsync(CancellationToken.None);
            var release = new PublishedRuleRelease(
                $"RULE-{Guid.NewGuid():N}",
                snapshot.RulesetId,
                snapshot.ProviderKind,
                snapshot.SourceIdentity,
                snapshot.RepositoryVersion,
                snapshot.CompilerContractVersion,
                RuleReleaseState.Candidate,
                RuleCompiler.Compile(snapshot.RepositoryVersion, snapshot.Documents),
                DateTimeOffset.UtcNow,
                ReleaseChannel: snapshot.ReleaseChannel,
                DiscoveryRef: snapshot.DiscoveryRef,
                ManifestFormatVersion: snapshot.ManifestFormatVersion,
                CompilerContractVersion: snapshot.CompilerContractVersion,
                VersionMetadata: snapshot.VersionMetadata);
            var plan = RulePublicationWritePlan.Create(release.Index);
            // This test acquires current HEAD, not the frozen pre-R2 corpus.
            // Keep the accepted R2 plan explicit; historical fixtures stay unchanged.
            Assert.Equal(130, plan.ChunkRows);
            Assert.Equal(834, plan.SelectorRows);
            Assert.Equal(95, plan.DependencyRows);

            var store = new SqlServerPublishedRuleStore(persistence);
            await store.StageCandidateAsync(release, CancellationToken.None);
            await store.SetStateAsync(release.RuleReleaseId, RuleReleaseState.Validated, null, CancellationToken.None);
            await store.SetStateAsync(release.RuleReleaseId, RuleReleaseState.Published, null, CancellationToken.None);
            await store.ActivateAsync(release.RulesetId, release.RuleReleaseId, CancellationToken.None);

            var active = await store.GetActiveAsync(release.RulesetId, CancellationToken.None);
            Assert.NotNull(active);
            Assert.Equal(release.RuleReleaseId, active.RuleReleaseId);
            Assert.Equal(RuleReleaseState.Active, active.State);
            Assert.Equal(plan.ChunkRows, await CountReleaseRowsAsync(
                databaseConnectionString,
                "rule_chunks",
                release.RuleReleaseId));
            Assert.Equal(plan.SelectorRows, await CountReleaseRowsAsync(
                databaseConnectionString,
                "rule_chunk_selectors",
                release.RuleReleaseId));
            Assert.Equal(plan.DependencyRows, await CountReleaseRowsAsync(
                databaseConnectionString,
                "rule_dependencies",
                release.RuleReleaseId));

            var duplicateChunk = release.Index.Chunks[0];
            var invalidRelease = release with
            {
                RuleReleaseId = $"RULE-{Guid.NewGuid():N}",
                State = RuleReleaseState.Candidate,
                Index = new CompiledRuleIndex(
                    release.Index.RepositoryVersion,
                    DateTimeOffset.UtcNow,
                    [duplicateChunk, duplicateChunk])
            };
            await Assert.ThrowsAsync<SqlException>(() =>
                store.StageCandidateAsync(invalidRelease, CancellationToken.None));
            Assert.Equal(0, await CountReleaseRowsAsync(
                databaseConnectionString,
                "rule_releases",
                invalidRelease.RuleReleaseId));
        }
        finally
        {
            SqlConnection.ClearAllPools();
            await DropDatabaseAsync(masterConnectionString, databaseName);
        }
    }

    [Fact]
    [Trait("Category", "SqlServerIntegration")]
    public async Task SupportedPre007DatabaseCanRecoverThroughNormalControlPlane()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable(RunIntegrationVariable),
                "1",
                StringComparison.Ordinal))
        {
            return;
        }

        var databaseName = $"EC_Pre007_{Guid.NewGuid():N}";
        var masterConnectionString = ConnectionString("master");
        var databaseConnectionString = ConnectionString(databaseName);
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), databaseName);
        Directory.CreateDirectory(temporaryDirectory);

        await CreateDatabaseAsync(masterConnectionString, databaseName);
        try
        {
            var persistence = Options.Create(new SqlServerPersistenceOptions
            {
                ConnectionString = databaseConnectionString,
                DomainSchema = "ec_domain",
                DefaultSchema = "ec"
            });
            var rules = Options.Create(new ManagedRuleServiceOptions
            {
                ReleaseChannel = RuleSourceReleaseChannel.Prerelease
            });
            var administration = Options.Create(new ManagedAdministrationOptions
            {
                Enabled = true,
                RequireOperatorConfirmation = false
            });
            var configurationRoot = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["EternalCycle:Persistence:ConnectionString"] = databaseConnectionString,
                    ["EternalCycle:Administration:Enabled"] = "true",
                    ["EternalCycle:Diagnostics:VerboseErrors"] = "true",
                    ["EternalCycle:Rules:ReleaseChannel"] = "Prerelease",
                    ["EternalCycle:Rules:GitSource:AcquisitionTimeout"] = "00:05:00"
                })
                .Build();
            var configuration = new ManagedConfigurationService(configurationRoot);
            var fallback = SanitizedDiagnosticFallbackBootstrap.Establish(configurationRoot, temporaryDirectory);
            var resolver = new ConfiguredCampaignSchemaResolver(persistence);

            await ApplyPre007SchemaAsync(databaseConnectionString, resolver.Resolve("the-second-turn"));
            await InsertCampaignAsync(databaseConnectionString);

            var inspector = new SqlServerManagedInfrastructureInspector(
                persistence,
                rules,
                administration,
                resolver,
                configuration,
                fallback);
            var readiness = new ManagedReadinessService(inspector);
            var bootstrap = new SqlServerSchemaBootstrapExecutor(persistence, resolver);
            var sourceStore = new SqlServerRuleSourceConfigurationStore(persistence);
            var operationStore = new SqlServerManagedOperationStore(persistence);
            var operationService = new ManagedOperationService(operationStore, sourceStore, rules);
            var service = new ManagedAdministrationService(
                administration,
                rules,
                bootstrap,
                readiness,
                sourceStore,
                new SqlServerCampaignDirectoryService(persistence, resolver),
                operationService,
                configuration);

            var preMigration = await readiness.GetReadinessAsync("the-second-turn", CancellationToken.None);
            Assert.Equal(ManagedReadinessState.MigrationRequired, preMigration.State);
            Assert.Equal(ManagedComponentStatus.Outdated, preMigration.RuleDomainSchema.Status);
            Assert.Equal(ManagedComponentStatus.Ready, preMigration.Campaign.Status);
            Assert.True(configuration.GetReport().Ready);

            var plan = await service.GetBootstrapPlanAsync("the-second-turn", CancellationToken.None);
        Assert.Equal(
            [
                "007_durable_managed_operations",
                "008_diagnostic_operation_correlation",
                "009_managed_operation_execution_leases",
                "010_gm_host_configuration",
                "011_compiled_artifact_import"
            ],
            plan.MigrationIds);

            var diagnostics = await new ServiceDiagnostics(resolver, readiness, operationStore)
                .GetReportAsync("the-second-turn", CancellationToken.None);
            Assert.Equal("PreMigration", diagnostics.DiagnosticScope);
            Assert.Equal(ManagedReadinessState.MigrationRequired, diagnostics.Readiness.State);

            var dump = await new ErrorDumpService(
                    new BoundedRecentManagedErrorStore(),
                    configuration,
                    fallback,
                    readiness: readiness,
                    databaseDiagnostics: new SqlServerManagedDiagnosticEvidenceReader(persistence),
                    operations: operationStore)
                .GetAsync(new ErrorDumpRequest(OperationId: "OP-PRE-007"), CancellationToken.None);
            Assert.Equal(ErrorDumpService.FormatVersion, dump.ErrorDumpFormatVersion);
            Assert.Contains("Readiness: MigrationRequired", dump.Text, StringComparison.Ordinal);
            Assert.Contains(
                dump.Sources,
                source => source.Source == "ManagedOperation" &&
                    !source.Available &&
                    source.Detail.Contains("not queried", StringComparison.OrdinalIgnoreCase));

            var operationListBeforeMigration = await new ManagedOperationTools(operationService, readiness)
                .ListRecentAsync(null, 10, CancellationToken.None);
            Assert.False(operationListBeforeMigration.Success);
            Assert.Equal("MIGRATION_REQUIRED", operationListBeforeMigration.Code);
            Assert.Equal("ec_get_setup_plan", operationListBeforeMigration.RecommendedNextAction);
            Assert.Contains("ec_initialize_service", operationListBeforeMigration.AllowedNextActions!);

            var denied = await service.BootstrapAsync(
                new BootstrapRequest("the-second-turn", UserApproved: false),
                CancellationToken.None);
            Assert.False(denied.Success);
            Assert.Equal("USER_APPROVAL_REQUIRED", denied.Code);
            Assert.False(await TableExistsAsync(databaseConnectionString, "ec_domain", "managed_operations"));

            var initialized = await service.BootstrapAsync(
                new BootstrapRequest("the-second-turn", UserApproved: true),
                CancellationToken.None);
            Assert.True(initialized.Success);
            Assert.Equal("INITIALIZATION_COMPLETE", initialized.Code);
        Assert.Equal(
            [
                "007_durable_managed_operations",
                "008_diagnostic_operation_correlation",
                "009_managed_operation_execution_leases",
                "010_gm_host_configuration",
                "011_compiled_artifact_import"
            ],
            initialized.Data!.AppliedMigrationIds);
            Assert.True(await CampaignExistsAsync(databaseConnectionString, "the-second-turn"));

            var postMigration = await readiness.GetReadinessAsync("the-second-turn", CancellationToken.None);
            Assert.Equal(ManagedReadinessState.RuleSourceRequired, postMigration.State);
            Assert.Equal(ManagedComponentStatus.Ready, postMigration.RuleDomainSchema.Status);

            var sourceDirectory = Path.Combine(temporaryDirectory, "rule-source");
            Directory.CreateDirectory(sourceDirectory);
            var configured = await service.ConfigureRuleSourceAsync(
                new RuleSourceSelectionRequest(
                    UseOfficialDefault: false,
                    SourceLocation: sourceDirectory,
                    RequestedRef: "HEAD",
                    ManifestPath: "RULES_MANIFEST.json",
                    UserApproved: true,
                    ReleaseChannel: RuleSourceReleaseChannel.Prerelease),
                CancellationToken.None);
            Assert.True(configured.Success);

            var queued = await service.PublishInitialRulesAsync(
                new InitialRulePublicationRequest(UserApproved: true),
                CancellationToken.None);
            Assert.True(queued.Success);
            Assert.Equal("RULE_PUBLICATION_QUEUED", queued.Code);
            Assert.NotNull(queued.Data);
            Assert.Equal(ManagedOperationState.Queued, queued.Data.State);

            var operationList = await new ManagedOperationTools(operationService, readiness)
                .ListRecentAsync(null, 10, CancellationToken.None);
            Assert.True(operationList.Success);
            Assert.Contains(operationList.Data!, item => item.OperationId == queued.Data.OperationId);
        }
        finally
        {
            SqlConnection.ClearAllPools();
            await DropDatabaseAsync(masterConnectionString, databaseName);
            try
            {
                Directory.Delete(temporaryDirectory, recursive: true);
            }
            catch (IOException)
            {
                // A diagnostic file handle may close just after the fixture completes.
            }
        }
    }

    private static async Task ApplyPre007SchemaAsync(
        string connectionString,
        CampaignSchemaRoute route)
    {
        foreach (var (fileName, campaignScoped) in new (string FileName, bool CampaignScoped)[]
        {
            ("001_initial.template.sql", true),
            ("002_rule_domain.template.sql", false),
            ("003_campaign_directory.template.sql", true),
            ("004_rule_source_configuration.template.sql", false),
            ("005_managed_operation_diagnostics.template.sql", false),
            ("006_rule_source_compatibility.template.sql", false)
        })
        {
            var template = await File.ReadAllTextAsync(
                Path.Combine(AppContext.BaseDirectory, "Schema", fileName));
            var sql = campaignScoped
                ? SqlServerSchemaMigration.Render(template, route)
                : SqlServerSchemaMigration.RenderDomain(template, "ec_domain");
            await ExecuteBatchesAsync(connectionString, sql);
        }
    }

    private static async Task InsertCampaignAsync(string connectionString)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("""
            INSERT INTO [ec].campaigns (
                campaign_id, display_name, description,
                repository_version, persistence_model_version
            ) VALUES (
                N'the-second-turn', N'The Second Turn', N'Pre-007 recovery fixture',
                N'1.0.0', N'1'
            );
            """, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<bool> CampaignExistsAsync(string connectionString, string campaignId)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            "SELECT COUNT(*) FROM [ec].campaigns WHERE campaign_id = @campaign_id;",
            connection);
        command.Parameters.AddWithValue("@campaign_id", campaignId);
        return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
    }

    private static async Task<bool> TableExistsAsync(
        string connectionString,
        string schemaName,
        string tableName)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("""
            SELECT COUNT(*)
            FROM sys.tables AS tables
            INNER JOIN sys.schemas AS schemas ON schemas.schema_id = tables.schema_id
            WHERE schemas.name = @schema_name AND tables.name = @table_name;
            """, connection);
        command.Parameters.AddWithValue("@schema_name", schemaName);
        command.Parameters.AddWithValue("@table_name", tableName);
        return Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
    }

    private static async Task<int> CountReleaseRowsAsync(
        string connectionString,
        string tableName,
        string ruleReleaseId)
    {
        var allowedTables = new HashSet<string>(StringComparer.Ordinal)
        {
            "rule_releases",
            "rule_chunks",
            "rule_chunk_selectors",
            "rule_dependencies"
        };
        if (!allowedTables.Contains(tableName))
        {
            throw new ArgumentOutOfRangeException(nameof(tableName));
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            $"SELECT COUNT(*) FROM [ec_domain].[{tableName}] WHERE rule_release_id = @rule_release_id;",
            connection);
        command.Parameters.AddWithValue("@rule_release_id", ruleReleaseId);
        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    private static async Task ExecuteBatchesAsync(string connectionString, string sql)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        foreach (var batch in BatchSeparator.Split(sql).Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            await using var command = new SqlCommand(batch, connection) { CommandTimeout = 30 };
            await command.ExecuteNonQueryAsync();
        }
    }

    private static async Task CreateDatabaseAsync(string masterConnectionString, string databaseName)
    {
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"CREATE DATABASE [{databaseName}];", connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task DropDatabaseAsync(string masterConnectionString, string databaseName)
    {
        await using var connection = new SqlConnection(masterConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"""
            IF DB_ID(N'{databaseName}') IS NOT NULL
            BEGIN
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [{databaseName}];
            END;
            """, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static string ConnectionString(string databaseName) =>
        new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = databaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        }.ConnectionString;

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "docs", "rules", "rule-source-manifest.json")) &&
                File.Exists(Path.Combine(directory.FullName, "VERSION")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the Eternal Cycle repository root.");
    }
}
