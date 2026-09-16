using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class SchemaRoutingAndRuleCompilationTests
{
    [Fact]
    public void A_DefaultSchemaUsesEc()
    {
        var route = Resolver(new SqlServerPersistenceOptions()).Resolve("campaign-a");

        Assert.Equal("ec", route.SchemaName);
        Assert.Equal("eternal-cycle-mainworld", route.DataNamespaceId);
        Assert.Equal("eternal-cycle-standard", route.WorldModelId);
    }

    [Fact]
    public void B_ConfiguredSchemaRouteIsStableForProvisionAndResumeWithoutEc()
    {
        var resolver = Resolver(ConfiguredOptions());
        var provisionRoute = resolver.Resolve("campaign-d");
        var resumeRoute = resolver.Resolve("campaign-d");

        Assert.Equal(provisionRoute, resumeRoute);
        Assert.Equal("fantasy_world", resumeRoute.SchemaName);
        Assert.Equal("namespace-fantasy", resumeRoute.DataNamespaceId);
        Assert.Equal("world-fantasy", resumeRoute.WorldModelId);
    }

    [Fact]
    public void C_CompatibleCampaignsCanShareSchemaAndRemainDistinct()
    {
        var resolver = Resolver(ConfiguredOptions());
        var first = resolver.Resolve("campaign-d");
        var second = resolver.Resolve("campaign-e");

        Assert.Equal(first.SchemaName, second.SchemaName);
        Assert.NotEqual(first.CampaignId, second.CampaignId);
    }

    [Fact]
    public void D_DifferentWorldModelsCanUseDifferentSchemas()
    {
        var resolver = Resolver(ConfiguredOptions());

        Assert.Equal("fantasy_world", resolver.Resolve("campaign-d").SchemaName);
        Assert.Equal("scifi_world", resolver.Resolve("campaign-f").SchemaName);
    }

    [Fact]
    public void E_SharedSchemaDoesNotCollapseCampaignIdentity()
    {
        var resolver = Resolver(ConfiguredOptions());
        var first = resolver.Resolve("campaign-d");
        var second = resolver.Resolve("campaign-e");

        Assert.Equal("campaign-d", first.CampaignId);
        Assert.Equal("campaign-e", second.CampaignId);
        Assert.Equal("fantasy_world", first.SchemaName);
        Assert.Equal("fantasy_world", second.SchemaName);
    }

    [Fact]
    public void F_CrossSchemaRoutesRemainIsolated()
    {
        var resolver = Resolver(ConfiguredOptions());
        var fantasy = resolver.Resolve("campaign-d");
        var scienceFiction = resolver.Resolve("campaign-f");

        Assert.NotEqual(fantasy.WorldModelId, scienceFiction.WorldModelId);
        Assert.NotEqual(fantasy.SchemaName, scienceFiction.SchemaName);
        Assert.Equal("[fantasy_world]", SqlServerSchemaIdentifier.Quote(fantasy.SchemaName));
        Assert.Equal("[scifi_world]", SqlServerSchemaIdentifier.Quote(scienceFiction.SchemaName));
    }

    [Fact]
    public void G_MigrationRenderingTargetsOnlyTheSelectedSchema()
    {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "Schema",
            "001_initial.template.sql");
        var template = File.ReadAllText(templatePath);
        var route = Resolver(ConfiguredOptions()).Resolve("campaign-d");

        var rendered = SqlServerSchemaMigration.Render(template, route);

        Assert.Contains("CREATE TABLE [fantasy_world].campaigns", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("CREATE TABLE [ec].campaigns", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain(SqlServerSchemaIdentifier.Token, rendered, StringComparison.Ordinal);
        Assert.DoesNotContain(SqlServerSchemaMigration.SchemaNameToken, rendered, StringComparison.Ordinal);
    }

    [Fact]
    public void H_DefaultSchemaIsNotMandatory()
    {
        var route = Resolver(ConfiguredOptions()).Resolve("campaign-f");

        Assert.Equal("scifi_world", route.SchemaName);
        Assert.NotEqual("ec", route.SchemaName);
    }

    [Fact]
    public void LogicalNamespaceIdentityIsIndependentOfPhysicalSchemaName()
    {
        var original = Resolver(ConfiguredOptions()).Resolve("campaign-d");
        var remappedOptions = ConfiguredOptions();
        remappedOptions.DataNamespaces["namespace-fantasy"] = new SqlDataNamespaceOptions
        {
            SchemaName = "ec_fantasyworld",
            SchemaModelVersion = "3",
            RulesetId = "ruleset-fantasy",
            RulesetVersion = "fantasy-rules-4"
        };
        var remapped = Resolver(remappedOptions).Resolve("campaign-d");

        Assert.Equal(original.DataNamespaceId, remapped.DataNamespaceId);
        Assert.NotEqual(original.SchemaName, remapped.SchemaName);
        Assert.Equal(original.WorldModelId, remapped.WorldModelId);
    }

    [Fact]
    public void DomainRuleMigrationTargetsEcDomainIndependentlyOfWorldSchemas()
    {
        var templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "Schema",
            "002_rule_domain.template.sql");
        var rendered = SqlServerSchemaMigration.RenderDomain(
            File.ReadAllText(templatePath),
            "ec_domain");

        Assert.Contains("CREATE TABLE [ec_domain].rule_releases", rendered, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE [ec_domain].rule_chunks", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain("fantasy_world", rendered, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("ec]; DROP TABLE campaigns;--")]
    [InlineData("world.name")]
    [InlineData("world name")]
    [InlineData("")]
    public void UnsafeSchemaIdentifiersAreRejected(string schemaName)
    {
        Assert.Throws<ArgumentException>(() => SqlServerSchemaIdentifier.Quote(schemaName));
    }

    [Fact]
    public void I_WorldSpecificRetrievalExcludesOtherWorldAndStaysWithinEightK()
    {
        var documents = new[]
        {
            Document("kernel", RuleLayer.RuntimeKernel, "Runtime gate and authority order.", alwaysInclude: true),
            Document("core", RuleLayer.Core, "Core action and consequence rules.", topics: ["combat"]),
            Document("world-a", RuleLayer.World, "World A gravity and magic rules.", worlds: ["world-a"], topics: ["combat"]),
            Document("world-b", RuleLayer.World, new string('B', 18_000), worlds: ["world-b"], topics: ["combat"]),
            Document("unused-core", RuleLayer.Core, new string('U', 18_000), topics: ["trade"])
        };
        var index = RuleCompiler.Compile("post-v1", documents);
        var route = new CampaignSchemaRoute(
            "campaign-a",
            "world-a",
            "namespace-a",
            "world_a",
            "3",
            "ruleset-a",
            "world-a-rules-2");
        var request = new RuleContextRequest(
            "campaign-a",
            "gameplay.resolve",
            ["combat"],
            "NORMAL",
            [],
            RuleCompiler.DefaultContextBudget);

        var result = RuleCompiler.Select(index, route, request);

        Assert.Equal("campaign-a", result.CampaignId);
        Assert.Equal("world-a", result.WorldModelId);
        Assert.Contains(result.Chunks, chunk => chunk.RuleSourceId == "kernel");
        Assert.Contains(result.Chunks, chunk => chunk.RuleSourceId == "core");
        Assert.Contains(result.Chunks, chunk => chunk.RuleSourceId == "world-a");
        Assert.DoesNotContain(result.Chunks, chunk => chunk.RuleSourceId == "world-b");
        Assert.DoesNotContain(result.Chunks, chunk => chunk.RuleSourceId == "unused-core");
        Assert.True(result.EstimatedTokens <= RuleCompiler.DefaultContextBudget);
        Assert.All(result.Chunks, chunk => Assert.Matches("^[A-F0-9]{64}$", chunk.SourceHash));
    }

    [Fact]
    public void CampaignModeAndOptionalModulesFilterRuleSources()
    {
        var documents = new[]
        {
            Document("kernel", RuleLayer.RuntimeKernel, "Kernel.", alwaysInclude: true),
            Document("normal", RuleLayer.Core, "Normal rule.", topics: ["combat"]),
            new RuleSourceDocument(
                "validation",
                "fixtures/validation.md",
                "# validation\n\nValidation-only rule.",
                new RuleSourceMetadata(
                    RuleLayer.Core,
                    [],
                    [],
                    ["VALIDATION"],
                    ["gameplay.resolve"],
                    ["combat"])),
            new RuleSourceDocument(
                "module-a",
                "fixtures/module-a.md",
                "# module-a\n\nOptional module rule.",
                new RuleSourceMetadata(
                    RuleLayer.OptionalModule,
                    [],
                    ["module-a"],
                    ["NORMAL"],
                    ["gameplay.resolve"],
                    ["combat"]))
        };
        var index = RuleCompiler.Compile("post-v1", documents);
        var route = new CampaignSchemaRoute(
            "campaign-a",
            "world-a",
            "namespace-a",
            "world_a",
            "3",
            "ruleset-a",
            "rules-2");
        var request = new RuleContextRequest(
            "campaign-a",
            "gameplay.resolve",
            ["combat"],
            "NORMAL",
            ["module-a"]);

        var result = RuleCompiler.Select(index, route, request);

        Assert.Contains(result.Chunks, chunk => chunk.RuleSourceId == "normal");
        Assert.Contains(result.Chunks, chunk => chunk.RuleSourceId == "module-a");
        Assert.DoesNotContain(result.Chunks, chunk => chunk.RuleSourceId == "validation");
    }

    private static ConfiguredCampaignSchemaResolver Resolver(SqlServerPersistenceOptions options) =>
        new(Options.Create(options));

    private static SqlServerPersistenceOptions ConfiguredOptions() =>
        new()
        {
            DefaultSchema = "ec",
            DomainSchema = "ec_domain",
            DefaultDataNamespaceId = "eternal-cycle-mainworld",
            DefaultWorldModelId = "eternal-cycle-standard",
            DefaultRulesetId = "eternal-cycle-core",
            DefaultSchemaModelVersion = "1",
            DefaultRulesetVersion = "1.0.0",
            CampaignWorldModels = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["campaign-d"] = "world-fantasy",
                ["campaign-e"] = "world-fantasy",
                ["campaign-f"] = "world-scifi"
            },
            WorldDataNamespaces = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["world-fantasy"] = "namespace-fantasy",
                ["world-scifi"] = "namespace-scifi"
            },
            DataNamespaces = new Dictionary<string, SqlDataNamespaceOptions>(StringComparer.Ordinal)
            {
                ["namespace-fantasy"] = new()
                {
                    SchemaName = "fantasy_world",
                    SchemaModelVersion = "2",
                    RulesetId = "ruleset-fantasy",
                    RulesetVersion = "fantasy-rules-4"
                },
                ["namespace-scifi"] = new()
                {
                    SchemaName = "scifi_world",
                    SchemaModelVersion = "5",
                    RulesetId = "ruleset-scifi",
                    RulesetVersion = "scifi-rules-7"
                }
            }
        };

    private static RuleSourceDocument Document(
        string id,
        RuleLayer layer,
        string content,
        IReadOnlyList<string>? worlds = null,
        IReadOnlyList<string>? topics = null,
        bool alwaysInclude = false) =>
        new(
            id,
            $"fixtures/{id}.md",
            $"# {id}\n\n{content}",
            new RuleSourceMetadata(
                layer,
                worlds ?? [],
                [],
                ["NORMAL"],
                ["gameplay.resolve"],
                topics ?? [],
                0,
                alwaysInclude));
}
