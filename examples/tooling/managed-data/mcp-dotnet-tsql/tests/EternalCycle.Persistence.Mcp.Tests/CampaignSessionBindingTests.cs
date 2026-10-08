using System.Text.Json;
using System.Diagnostics;
using EternalCycle.Rules.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class CampaignBindingContractTests
{
    [Theory]
    [InlineData("", "session", "authority")]
    [InlineData("principal", "", "authority")]
    [InlineData("principal", "session", "")]
    public void MissingTrustedScopeFails(string principal, string session, string authority) =>
        Assert.Equal("BINDING_SESSION_INVALID", Assert.Throws<ManagedServiceException>(() =>
            new CampaignBindingScope(principal, session, authority, [], true, true, true).Validate()).Code);

    [Fact]
    public void ScopeIsUnambiguousPrincipalSessionIdentityNotAuthorityTransportOrProse()
    {
        var first = new CampaignBindingScope("a", "bc", "authority", [], true, true, true);
        Assert.NotEqual(first.Key, (first with { PrincipalId = "ab", LogicalSessionId = "c" }).Key);
        Assert.NotEqual(first.Key, (first with { PrincipalId = "other" }).Key);
        Assert.Equal(first.Key, (first with { AuthorityId = "new-authority" }).Key);
        Assert.DoesNotContain("Story", string.Join(',', typeof(CampaignBindingChange).GetProperties().Select(value => value.Name)));
        Assert.DoesNotContain("CampaignId", string.Join(',', typeof(CampaignBindingChange).GetProperties().Select(value => value.Name)));
        Assert.False(new CampaignBindingResolution(null, [], "unresolved", false).GameplayInteractionAuthorized);
    }

    [Fact]
    public async Task DisabledOrMissingIdentityDeniesBeforeAnyStoreLookup()
    {
        var store = new RefusingStore();
        Assert.Equal("BINDING_DISABLED", (await new CampaignBindingService(store, Options.Create(new CampaignBindingOptions())).ResolveAsync(default)).Code);
        Assert.Equal("BINDING_SESSION_INVALID", (await new CampaignBindingService(store, Options.Create(new CampaignBindingOptions { Enabled = true })).ResolveAsync(default)).Code);
        Assert.Equal(0, store.Calls);
    }

    [Theory]
    [InlineData(CampaignBindingAction.Select, false)]
    [InlineData(CampaignBindingAction.Switch, true)]
    [InlineData(CampaignBindingAction.Suspend, true)]
    public async Task ChangeRequiresGrantAndActualSelectionApproval(CampaignBindingAction action, bool approval)
    {
        var store = new RefusingStore();
        var service = new CampaignBindingService(store, Options.Create(new CampaignBindingOptions
            { Enabled = true, PrincipalId = "principal", LogicalSessionId = "session", AuthorityId = "authority" }));
        Assert.Equal("BINDING_UNAUTHORIZED", (await service.ChangeAsync(action, new("request", "choice", null, 0, approval), default)).Code);
        Assert.Equal(0, store.Calls);
    }

    [Fact]
    public void MigrationIsPackagedAndRendersExactlyWithoutGuessedBindings()
    {
        var schema = Path.Combine(AppContext.BaseDirectory, "Schema");
        var template = File.ReadAllText(Path.Combine(schema, "013_campaign_session_binding.template.sql"));
        Assert.Equal(File.ReadAllText(Path.Combine(schema, "013_campaign_session_binding.sql")).ReplaceLineEndings("\n"),
            SqlServerSchemaMigration.RenderDomain(template, "ec_domain").ReplaceLineEndings("\n"));
        Assert.DoesNotContain("{{schema", SqlServerSchemaMigration.RenderDomain(template, "binding_domain"));
        Assert.DoesNotContain("INSERT", template);
    }

    [Fact]
    public void FailureCodesHaveStableSafeRecoveryGuidance()
    {
        foreach (var code in new[] { "BINDING_DISABLED", "BINDING_SESSION_INVALID", "BINDING_REQUEST_INVALID", "BINDING_UNAUTHORIZED",
            "BINDING_UNBOUND", "CAMPAIGN_SELECTION_REQUIRED", "BINDING_CAMPAIGN_UNAVAILABLE", "BINDING_CONFLICT",
            "BINDING_SUSPENDED", "BINDING_NOT_READY", "BINDING_PROFILE_CHANGED", "BINDING_SWITCH_CONFLICT",
            "BINDING_SWITCH_BLOCKED", "BINDING_SCHEMA_REQUIRED", "BINDING_STORAGE_FAILED" })
            Assert.True(EternalCycleErrorRegistry.Get(code).Found, code);
    }

    private sealed class RefusingStore : ICampaignBindingStore
    {
        public int Calls;
        public Task<ManagedOperationResult<CampaignBindingResolution>> ExecuteAsync(CampaignBindingScope scope,
            CampaignBindingAction action, CampaignBindingChange? change, string correlationId, CancellationToken token)
        { Calls++; throw new InvalidOperationException("Unauthorized store lookup."); }
    }
}

// Every test creates and destroys a real disposable LocalDB via the existing
// pre-011 fixture. No deployment/campaign database is touched.
public sealed partial class CompiledRulesArtifactSqlImportTests
{
    private CampaignBindingOptions BindingOptions(string session = "session", string[]? campaigns = null,
        bool auto = true, string principal = "principal") => new()
    {
        Enabled = true, PrincipalId = principal, LogicalSessionId = session, AuthorityId = "reference-authority",
        AuthorizedCampaignIds = campaigns ?? ["campaign-a", "campaign-b", "campaign-c"],
        AllowSoleCampaignResume = auto, AllowSelection = true, AllowSwitch = true
    };
    private CampaignBindingService Bindings(CampaignBindingOptions? policy = null, bool safety = false,
        bool compiled = false, IManagedDiagnosticRecorder? diagnostics = null) => new(
        new SqlServerCampaignBindingStore(options, new ConfiguredCampaignSchemaResolver(options),
            new SqlServerCampaignDirectoryService(options, new ConfiguredCampaignSchemaResolver(options)),
            Options.Create(new CompiledRuleRuntimeOptions { Enabled = compiled }), safety ? new SafeSwitch() : null),
        Options.Create(policy ?? BindingOptions()), diagnostics);

    private async Task BindingSetupAsync(int campaigns = 1, bool installBindingSchema = true)
    {
        if (installBindingSchema)
            await ExecuteAsync(SqlServerSchemaMigration.RenderDomain(File.ReadAllText(MigrationPath("013_campaign_session_binding.template.sql")), Domain));
        var chunks = RuleCompiler.Compile("1.0.0", [
            new RuleSourceDocument("runtime-kernel", "rules/kernel.md", "# Kernel\n\nRepository Canon remains authoritative.",
                new(RuleLayer.RuntimeKernel, [], [], ["*"], ["*"], [], AlwaysInclude: true, PreparationTier: RulePreparationTier.RuntimeKernel)),
            new RuleSourceDocument("gm-host-bootstrap", "rules/bootstrap.txt", "Use canonical rules and preserve player agency.",
                new(RuleLayer.RuntimeKernel, [], [], ["*"], ["*"], [], AlwaysInclude: true, PreparationTier: RulePreparationTier.RuntimeKernel))]);
        var release = new PublishedRuleRelease("RULE-binding", "eternal-cycle-core", "fixture", "binding-source", "1.0.0", "1",
            RuleReleaseState.Candidate, chunks, DateTimeOffset.UtcNow);
        await Legacy.StageCandidateAsync(release, default);
        var preparation = new SqlServerRulePreparationStore(options);
        await preparation.InitializeAsync(release, default);
        await preparation.MarkReadyAsync(release.RuleReleaseId, chunks.Chunks.Select(value => value.RuleSourceId).Distinct().ToArray(), default);
        await Legacy.SetStateAsync(release.RuleReleaseId, RuleReleaseState.Published, null, default);
        await Legacy.ActivateAsync(release.RulesetId, release.RuleReleaseId, default);
        await new SqlServerGmHostConfigurationStore(options).SaveAsync(new(release.RulesetId,
            chunks.Chunks.Single(value => value.RuleSourceId == "gm-host-bootstrap").SourceHash,
            GmHostConfigurationState.UserConfirmed, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, null, 0), default);
        for (var index = 0; index < campaigns; index++)
            await ExecuteAsync($"""
                INSERT INTO [ec_fixture].campaigns (campaign_id, display_name, description, repository_version, persistence_model_version, active_version, last_validated_commit_at)
                VALUES (N'campaign-{(char)('a' + index)}', N'Campaign {(char)('A' + index)}', N'Generic independent continuity', N'1.0.0', N'managed-sql-1', 1, SYSUTCDATETIME());
                """);
    }

    private Task SeedBindingCanonAsync() => ExecuteAsync("""
        INSERT INTO [ec_fixture].save_transactions (transaction_id, campaign_id, idempotency_key, request_hash, request_json,
            parent_version, candidate_version, source_interaction_id, affected_set_json, status, started_at)
        SELECT N'initial-' + campaign_id, campaign_id, N'initial', REPLICATE('A',64), N'{}',
            0, 1, N'fixture-initial-save', N'[]', N'Completed', SYSUTCDATETIME() FROM [ec_fixture].campaigns;
        INSERT INTO [ec_fixture].canonical_record_versions (campaign_id, owner_domain, record_id, campaign_version,
            record_revision, payload_json, payload_hash, is_tombstone, transaction_id, recorded_at)
        SELECT fixtures.campaign_id, N'entities', N'protagonist', 1, 1, payload,
            CONVERT(varchar(64), HASHBYTES('SHA2_256', CONVERT(varchar(max), payload)), 2), 0,
            N'initial-' + fixtures.campaign_id, SYSUTCDATETIME()
        FROM (VALUES
            (N'campaign-a', N'{"name":"Familiar traveller","location":"Familiar valley"}'),
            (N'campaign-b', N'{"name":"Different traveller","location":"Distant harbour"}')
        ) AS fixtures(campaign_id, payload)
        INNER JOIN [ec_fixture].campaigns AS campaigns ON campaigns.campaign_id = fixtures.campaign_id;
        """);

    private static CampaignBindingChange Select(CampaignBindingResolution result, int choice = 0, string request = "selection") =>
        new(request, result.Choices[choice].Handle, result.Binding?.BindingId, result.Binding?.Revision ?? 0, true);
    private static CampaignSessionBinding Bound(ManagedOperationResult<CampaignBindingResolution> result)
    { Assert.True(result.Success, result.Code); Assert.NotNull(result.Data?.Binding); return result.Data.Binding; }
    private sealed class SafeSwitch : ISqlCampaignBindingSwitchSafety
    {
        public Task<CampaignSwitchSafety> CheckAsync(SqlConnection connection, SqlTransaction transaction,
            CampaignSessionBinding binding, CancellationToken token) => Task.FromResult(CampaignSwitchSafety.Clear);
    }

    [SqlImportFact]
    public async Task B_ZeroCampaignsStayUnboundAndDoNotCreateCampaignOrBinding()
    {
        await BindingSetupAsync(0);
        var result = await Bindings().ResolveAsync(default);
        Assert.Equal("BINDING_UNBOUND", result.Code); Assert.Null(result.Data!.Binding);
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].campaigns;"));
        Assert.Equal(0, await CountAsync("campaign_session_bindings"));
    }

    [SqlImportFact]
    public async Task B_SoleCampaignRecoversExactRecordAcrossServiceAndMcpAdapterRecreation()
    {
        await BindingSetupAsync();
        var first = Bound(await new CampaignBindingTools(Bindings()).ResolveAsync(default));
        var recovered = Bound(await new CampaignBindingTools(Bindings()).ResolveAsync(default));
        Assert.Equal(first, recovered); Assert.Equal("campaign-a", recovered.CampaignId);
        Assert.Equal("sole-campaign", first.ResolutionMode); Assert.Equal("UserConfirmed", first.Evidence.BootstrapState);
        Assert.Equal(1, await CountAsync("campaign_session_bindings"));
        Assert.NotNull((await Bindings().ResolveAsync(default)).CorrelationId);
    }

    [SqlImportFact]
    public async Task B_CampaignLossWithResemblingCanonStillRequiresSelectionAndNoCanonWrite()
    {
        await BindingSetupAsync(2);
        // Distinct Canon includes familiar story cues, but binding has no narrative
        // input and must not inspect these records to choose a campaign.
        await ExecuteAsync("UPDATE [ec_fixture].campaigns SET description = N'A familiar protagonist and location' WHERE campaign_id = N'campaign-a';");
        await SeedBindingCanonAsync();
        var before = await ScalarAsync("SELECT SUM(active_version) FROM [ec_fixture].campaigns;");
        var service = Bindings();
        var discovery = await new CampaignBindingTools(service).ResolveAsync(default);
        Assert.Equal("CAMPAIGN_SELECTION_REQUIRED", discovery.Code); Assert.Null(discovery.Data!.Binding);
        Assert.Equal(2, discovery.Data.Choices.Count); Assert.Equal("Campaign A", discovery.Data.Choices[0].DisplayName);
        Assert.Equal(0, await CountAsync("campaign_session_bindings"));
        var selected = Bound(await new CampaignBindingTools(service).SelectAsync(Select(discovery.Data, 1), default));
        Assert.Equal("campaign-b", selected.CampaignId);
        Assert.Equal(selected, Bound(await Bindings().ResolveAsync(default)));
        Assert.Equal(before, await ScalarAsync("SELECT SUM(active_version) FROM [ec_fixture].campaigns;"));
        Assert.Equal(2, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions;"));
        Assert.Equal(2, await ScalarAsync("""
            SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions
            WHERE (campaign_id = N'campaign-a' AND payload_json = N'{"name":"Familiar traveller","location":"Familiar valley"}')
               OR (campaign_id = N'campaign-b' AND payload_json = N'{"name":"Different traveller","location":"Distant harbour"}');
            """));
    }

    [SqlImportFact]
    public async Task B_InvalidAndStaleChoiceDoNotFallBackOrMutate()
    {
        await BindingSetupAsync(2);
        var service = Bindings(); var discovery = (await service.ResolveAsync(default)).Data!;
        Assert.Equal("BINDING_CAMPAIGN_UNAVAILABLE", (await service.ChangeAsync(CampaignBindingAction.Select,
            Select(discovery) with { ChoiceHandle = "nonexistent" }, default)).Code);
        await ExecuteAsync("UPDATE [ec_fixture].campaigns SET active_version = 2 WHERE campaign_id = N'campaign-a';");
        Assert.Equal("BINDING_CAMPAIGN_UNAVAILABLE", (await service.ChangeAsync(CampaignBindingAction.Select, Select(discovery), default)).Code);
        Assert.Equal(0, await CountAsync("campaign_session_bindings"));
    }

    [SqlImportFact]
    public async Task B_AutomaticResumeRequiresPolicyAndValidatedInitialSave()
    {
        await BindingSetupAsync();
        Assert.Equal("CAMPAIGN_SELECTION_REQUIRED", (await Bindings(BindingOptions(auto: false)).ResolveAsync(default)).Code);
        await ExecuteAsync("UPDATE [ec_fixture].campaigns SET active_version = 0, last_validated_commit_at = NULL;");
        Assert.Equal("BINDING_UNBOUND", (await Bindings().ResolveAsync(default)).Code);
        Assert.Equal(0, await CountAsync("campaign_session_bindings"));
    }

    [SqlImportFact]
    public async Task B_NewSaveRefreshesVersionNotCampaignAndOtherCampaignActivityDoesNotSwitch()
    {
        await BindingSetupAsync(); var first = Bound(await Bindings().ResolveAsync(default));
        await ExecuteAsync("UPDATE [ec_fixture].campaigns SET active_version = 2, last_validated_commit_at = SYSUTCDATETIME();");
        var next = Bound(await Bindings().ResolveAsync(default));
        Assert.Equal(first.BindingId, next.BindingId); Assert.Equal(2, next.Evidence.CampaignVersion); Assert.Equal(2, next.Revision);
        await ExecuteAsync("INSERT INTO [ec_fixture].campaigns (campaign_id, display_name, repository_version, persistence_model_version, active_version, last_validated_commit_at) VALUES (N'campaign-b', N'Newer campaign', N'1.0.0', N'managed-sql-1', 999, SYSUTCDATETIME());");
        Assert.Equal(next, Bound(await Bindings().ResolveAsync(default)));
    }

    [SqlImportFact]
    public async Task B_DeletedSelectedCampaignSuspendsAndNeverSubstitutesAnother()
    {
        await BindingSetupAsync(); var first = Bound(await Bindings().ResolveAsync(default));
        await ExecuteAsync("DELETE FROM [ec_fixture].campaigns; INSERT INTO [ec_fixture].campaigns (campaign_id, display_name, repository_version, persistence_model_version, active_version, last_validated_commit_at) VALUES (N'campaign-b', N'Other campaign', N'1.0.0', N'managed-sql-1', 1, SYSUTCDATETIME());");
        var result = await Bindings().ResolveAsync(default);
        Assert.Equal("BINDING_CAMPAIGN_UNAVAILABLE", result.Code); Assert.Equal(first.BindingId, result.Data!.Binding!.BindingId);
        Assert.Equal(CampaignBindingState.SUSPENDED, result.Data.Binding.State); Assert.False(result.Data.BindingValid);
    }

    [SqlImportFact]
    public async Task B_AccessRevocationSuspendsWithoutDisclosingBindingOrOtherCandidates()
    {
        await BindingSetupAsync(); await Bindings().ResolveAsync(default);
        var revoked = await Bindings(BindingOptions(campaigns: [])).ResolveAsync(default);
        Assert.Equal("BINDING_UNAUTHORIZED", revoked.Code); Assert.Null(revoked.Data);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].campaign_session_bindings WHERE binding_status = N'SUSPENDED';"));
    }

    [SqlImportFact]
    public async Task B_PrincipalAndLogicalSessionsAreIsolatedEvenWithSharedRulesAndNames()
    {
        await BindingSetupAsync(2);
        var a = Bindings(BindingOptions(campaigns: ["campaign-a"]));
        var b = Bindings(BindingOptions(principal: "other", campaigns: ["campaign-b"]));
        Assert.Equal("campaign-a", Bound(await a.ResolveAsync(default)).CampaignId);
        Assert.Equal("campaign-b", Bound(await b.ResolveAsync(default)).CampaignId);
        Assert.Equal("campaign-a", Bound(await a.ResolveAsync(default)).CampaignId);
        Assert.NotEqual(Bound(await a.ResolveAsync(default)).BindingId, Bound(await Bindings(BindingOptions(session: "other-session", campaigns: ["campaign-a"])).ResolveAsync(default)).BindingId);
    }

    [SqlImportFact]
    public async Task B_ProfileChangeSuspendsUntilExplicitVerifiedReconfirmation()
    {
        await BindingSetupAsync(); var first = Bound(await Bindings().ResolveAsync(default));
        await ExecuteAsync("UPDATE [ec_fixture].campaigns SET repository_version = N'approved-new-profile';");
        var blocked = await Bindings().ResolveAsync(default);
        Assert.Equal("BINDING_PROFILE_CHANGED", blocked.Code); Assert.Equal(first.Evidence, blocked.Data!.Binding!.Evidence);
        var request = Select(blocked.Data);
        Assert.Equal("BINDING_SWITCH_BLOCKED", (await Bindings().ChangeAsync(CampaignBindingAction.Select, request, default)).Code);
        var refreshed = Bound(await Bindings(safety: true).ChangeAsync(CampaignBindingAction.Select, request, default));
        Assert.Equal(first.BindingId, refreshed.BindingId); Assert.Equal("approved-new-profile", refreshed.Evidence.RepositoryVersion);
    }

    [SqlImportFact]
    public async Task B_ReadinessLossSuspendsAndRecoversSameBindingWithoutFalseVerification()
    {
        await BindingSetupAsync(); var first = Bound(await Bindings().ResolveAsync(default));
        await ExecuteAsync("UPDATE [import_domain].gm_host_configurations SET configuration_state = N'Required';");
        var blocked = await Bindings().ResolveAsync(default); Assert.Equal("BINDING_NOT_READY", blocked.Code);
        Assert.Equal("Required", blocked.Data!.Binding!.Evidence.BootstrapState);
        await ExecuteAsync("UPDATE [import_domain].gm_host_configurations SET configuration_state = N'UserConfirmed';");
        Assert.Equal(first.BindingId, Bound(await Bindings().ResolveAsync(default)).BindingId);
    }

    [SqlImportFact]
    public async Task B_ExplicitSuspensionPersistsAcrossRestartAndResolveDoesNotUndoIt()
    {
        await BindingSetupAsync(); var first = Bound(await Bindings().ResolveAsync(default));
        var request = new CampaignBindingChange("suspend", null, first.BindingId, first.Revision, true);
        Assert.Equal(CampaignBindingState.SUSPENDED, Bound(await new CampaignBindingTools(Bindings()).SuspendAsync(request, default)).State);
        Assert.Equal("BINDING_SUSPENDED", (await Bindings().ResolveAsync(default)).Code);
        Assert.Equal("BINDING_REPLAY", (await Bindings().ChangeAsync(CampaignBindingAction.Suspend, request, default)).Code);
    }

    [SqlImportFact]
    public async Task B_SwitchWithoutFutureInteractionEvidenceFailsSafeAndKeepsPredecessor()
    {
        await BindingSetupAsync(2); var service = Bindings(); var initial = (await service.ResolveAsync(default)).Data!;
        var first = Bound(await service.ChangeAsync(CampaignBindingAction.Select, Select(initial), default));
        var current = (await service.ResolveAsync(default)).Data!;
        var result = await new CampaignBindingTools(service).SwitchAsync(Select(current, 1, "switch"), default);
        Assert.Equal("BINDING_SWITCH_BLOCKED", result.Code);
        Assert.Equal(first, Bound(await service.ResolveAsync(default))); Assert.Equal(1, await CountAsync("campaign_session_bindings"));
    }

    [SqlImportFact]
    public async Task B_SafeSwitchCreatesAtomicSuccessorAndLostAckRetryDoesNotCreateAnother()
    {
        await BindingSetupAsync(2); var service = Bindings(safety: true);
        var first = Bound(await service.ChangeAsync(CampaignBindingAction.Select, Select((await service.ResolveAsync(default)).Data!), default));
        var request = Select((await service.ResolveAsync(default)).Data!, 1, "switch");
        var next = Bound(await service.ChangeAsync(CampaignBindingAction.Switch, request, default));
        Assert.Equal("campaign-b", next.CampaignId); Assert.Equal(2, next.Generation); Assert.Equal(first.BindingId, next.PredecessorBindingId);
        Assert.Equal(next.BindingId, Bound(await Bindings(safety: true).ChangeAsync(CampaignBindingAction.Switch, request, default)).BindingId);
        Assert.Equal(next, Bound(await service.ResolveAsync(default)));
        Assert.Equal(2, await CountAsync("campaign_session_bindings"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].campaign_session_bindings WHERE binding_status = N'CLOSED' AND successor_id IS NOT NULL;"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].campaign_session_bindings WHERE binding_status = N'ACTIVE';"));
        var replay = Bound(await service.ChangeAsync(CampaignBindingAction.Select,
            Select(new(null, (await service.ResolveAsync(default)).Data!.Choices, "unresolved", false)), default));
        Assert.Equal(first.BindingId, replay.BindingId); Assert.Equal(CampaignBindingState.CLOSED, replay.State);
    }

    [SqlImportFact]
    public async Task B_RequestHashConflictStaleRevisionAndCompetingSwitchesHaveOneWinner()
    {
        await BindingSetupAsync(3); var service = Bindings(safety: true);
        var selection = Select((await service.ResolveAsync(default)).Data!);
        await service.ChangeAsync(CampaignBindingAction.Select, selection, default);
        Assert.Equal("BINDING_CONFLICT", (await service.ChangeAsync(CampaignBindingAction.Select, selection with { ChoiceHandle = "changed" }, default)).Code);
        var current = (await service.ResolveAsync(default)).Data!;
        Assert.Equal("BINDING_CONFLICT", (await service.ChangeAsync(CampaignBindingAction.Switch, Select(current, 1, "stale") with { ExpectedRevision = 0 }, default)).Code);
        var results = await Task.WhenAll(service.ChangeAsync(CampaignBindingAction.Switch, Select(current, 1, "switch-b"), default),
            Bindings(safety: true).ChangeAsync(CampaignBindingAction.Switch, Select(current, 2, "switch-c"), default));
        Assert.Single(results, value => value.Success); Assert.Equal("BINDING_CONFLICT", results.Single(value => !value.Success).Code);
        Assert.Equal(2, await CountAsync("campaign_session_bindings"));
    }

    [SqlImportFact]
    public async Task B_ConcurrentUniqueResumeAndSelectionRetryCreateExactlyOneCurrentBinding()
    {
        await BindingSetupAsync();
        var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Bindings().ResolveAsync(default)));
        Assert.Single(results.Select(value => Bound(value).BindingId).Distinct());
        Assert.Equal(1, await CountAsync("campaign_session_bindings"));
        var current = (await Bindings().ResolveAsync(default)).Data!; var request = Select(current);
        var first = Bound(await Bindings().ChangeAsync(CampaignBindingAction.Select, request, default));
        Assert.Equal(first, Bound(await Bindings().ChangeAsync(CampaignBindingAction.Select, request, default)));
    }

    [SqlImportFact]
    public async Task B_PersistenceUncertaintyBlocksSwitchAndDoesNotTransferSaveOwnership()
    {
        await BindingSetupAsync(2); var service = Bindings(safety: true);
        var first = Bound(await service.ChangeAsync(CampaignBindingAction.Select, Select((await service.ResolveAsync(default)).Data!), default));
        var request = Select((await service.ResolveAsync(default)).Data!, 1, "switch");
        await ExecuteAsync("""
            INSERT INTO [ec_fixture].save_transactions (transaction_id, campaign_id, idempotency_key, request_hash, request_json,
                parent_version, candidate_version, source_interaction_id, affected_set_json, status, started_at)
            VALUES (N'tx-pending', N'campaign-a', N'key-pending', REPLICATE('A',64), N'{}', 1, 2, N'existing-interaction', N'[]', N'ActivatedPendingReadback', SYSUTCDATETIME());
            """);
        Assert.Equal("BINDING_SWITCH_BLOCKED", (await service.ChangeAsync(CampaignBindingAction.Switch, request, default)).Code);
        Assert.Equal(first.BindingId, (await service.ResolveAsync(default)).Data!.Binding!.BindingId);
        Assert.Equal(1, await CountAsync("campaign_session_bindings"));
    }

    [SqlImportFact]
    public async Task B_UpgradeFromTwelveIsOptInRepeatSafeAndPreservesSaveAndRuleHistory()
    {
        await MigrateAsync(); await PublicationMigrationAsync();
        await BindingSetupAsync(installBindingSchema: false);
        await SeedBindingCanonAsync();
        var active = await Legacy.GetActiveAsync("eternal-cycle-core", default);
        Assert.NotNull(active);
        var before = JsonSerializer.Serialize(active);
        var rules = await ScalarAsync("SELECT COUNT(*) FROM [import_domain].rule_releases;");
        var legacy = new SqlServerSchemaBootstrapExecutor(options, new ConfiguredCampaignSchemaResolver(options));
        Assert.Empty((await legacy.PlanAsync(null, default)).MigrationIds);
        var opted = new SqlServerSchemaBootstrapExecutor(options, new ConfiguredCampaignSchemaResolver(options),
            campaignBinding: Options.Create(BindingOptions()));
        Assert.Equal(new[] { "013_campaign_session_binding" }, (await opted.PlanAsync(null, default)).MigrationIds);
        var diagnostics = new BindingDiagnostics();
        Assert.Equal("BINDING_SCHEMA_REQUIRED", (await Bindings(diagnostics: diagnostics).ResolveAsync(default)).Code);
        Assert.IsType<SqlException>(Assert.Single(diagnostics.Failures).InnerException);
        await opted.ExecuteAsync(null, default); await opted.ExecuteAsync(null, default);
        Assert.Empty((await opted.PlanAsync(null, default)).MigrationIds);
        Assert.Equal(0, await CountAsync("campaign_session_bindings"));
        Assert.Equal(before, JsonSerializer.Serialize(await Legacy.GetActiveAsync("eternal-cycle-core", default)));
        Assert.Equal(rules, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].rule_releases;"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions WHERE status = N'Completed';"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].campaigns WHERE campaign_id = N'campaign-a' AND active_version = 1;"));
        Assert.Equal(1, await ScalarAsync("""
            SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions
            WHERE campaign_id = N'campaign-a' AND campaign_version = 1 AND record_revision = 1
              AND payload_json = N'{"name":"Familiar traveller","location":"Familiar valley"}';
            """));
    }

    [SqlImportFact]
    public async Task B_CompiledProfileUsesAuthorizedActiveArtifactNotImportedNewestOrLegacy()
    {
        await BindingSetupAsync();
        var approved = await CompiledRulesImportFixtures.Approve(CompiledRulesArtifactWriter.Write(OrdinaryAuthorityCandidate.Compile().Artifact!).Bytes.ToArray());
        await ReadyAsync(approved);
        var first = Bound(await Bindings(compiled: true).ResolveAsync(default));
        Assert.Equal("CompiledArtifact", first.Evidence.RuleRepresentation);
        Assert.Equal(approved.Artifact.Integrity.ArtifactSha256, first.Evidence.ActiveRuleIdentity);
        Assert.Equal(approved.Artifact.Ruleset.Source.Scheme + ":" + approved.Artifact.Ruleset.Source.Value, first.Evidence.ImmutableSourceIdentity);
        await Store("fixture-rules").ImportAsync(await CompiledRulesImportFixtures.Approve(CompiledRulesImportFixtures.Representative()), default);
        Assert.Equal(first, Bound(await Bindings(compiled: true).ResolveAsync(default)));
    }

    [SqlImportFact]
    public async Task B_CancelledBindingLeavesNoPartialState()
    {
        await BindingSetupAsync(); using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Bindings().ResolveAsync(cancelled.Token));
        Assert.Equal(0, await CountAsync("campaign_session_bindings"));
    }

    [SqlImportFact]
    public async Task B_DuplicateDisplayLabelsHaveDistinctSafeChoicesNotStoryDisambiguation()
    {
        await BindingSetupAsync(2);
        await ExecuteAsync("UPDATE [ec_fixture].campaigns SET display_name = N'Shared name';");
        var result = (await Bindings().ResolveAsync(default)).Data!;
        Assert.Equal(2, result.Choices.Select(value => value.DisplayName).Distinct().Count());
        Assert.Equal(2, result.Choices.Select(value => value.Handle).Distinct().Count());
        Assert.Equal("campaign-b", Bound(await Bindings().ChangeAsync(CampaignBindingAction.Select, Select(result, 1), default)).CampaignId);
    }

    [SqlImportTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task B_FailureOrCancellationAfterClosingPredecessorRollsBackEverything(bool cancel)
    {
        await BindingSetupAsync(2); var service = Bindings(safety: true);
        var first = Bound(await service.ChangeAsync(CampaignBindingAction.Select, Select((await service.ResolveAsync(default)).Data!), default));
        var request = Select((await service.ResolveAsync(default)).Data!, 1, "switch");
        using var cancellation = new CancellationTokenSource();
        var store = new SqlServerCampaignBindingStore(options, new ConfiguredCampaignSchemaResolver(options),
            new SqlServerCampaignDirectoryService(options, new ConfiguredCampaignSchemaResolver(options)),
            Options.Create(new CompiledRuleRuntimeOptions()), new SafeSwitch())
        { AfterPredecessorClosed = () => { if (cancel) cancellation.Cancel(); else throw new ManagedServiceException("BINDING_STORAGE_FAILED", "Injected storage failure."); } };
        var failing = new CampaignBindingService(store, Options.Create(BindingOptions()));
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => failing.ChangeAsync(CampaignBindingAction.Switch, request, cancellation.Token));
        else Assert.Equal("BINDING_STORAGE_FAILED", (await failing.ChangeAsync(CampaignBindingAction.Switch, request, default)).Code);
        Assert.Equal(first, Bound(await Bindings().ResolveAsync(default)));
        Assert.Equal(1, await CountAsync("campaign_session_bindings"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].campaign_binding_receipts;"));
        Assert.Equal(2, Bound(await service.ChangeAsync(CampaignBindingAction.Switch, request, default)).Generation);
    }

    [SqlImportFact]
    public async Task B_DiagnosticsAreCorrelatedBoundedAndNeverContainRawSessionOrStory()
    {
        await BindingSetupAsync(); var recorder = new BindingDiagnostics();
        var binding = Bound(await Bindings(BindingOptions(session: "private-logical-session"), diagnostics: recorder).ResolveAsync(default));
        var context = Assert.Single(recorder.Contexts);
        Assert.Equal(binding.CampaignId, context.CampaignId); Assert.NotNull(context.CorrelationId);
        Assert.Contains(binding.BindingId, context.SafeDetail!);
        Assert.DoesNotContain("private-logical-session", context.SafeDetail!);
        Assert.DoesNotContain("Generic independent continuity", context.SafeDetail!);
        Assert.InRange(context.SafeDetail!.Length, 1, 2000);
    }

    private sealed class BindingDiagnostics : IManagedDiagnosticRecorder
    {
        public List<ManagedDiagnosticContext> Contexts { get; } = [];
        public List<Exception> Failures { get; } = [];
        public Task<ManagedDiagnosticReceipt> RecordFailureAsync(ManagedDiagnosticContext context, Exception error, CancellationToken token)
        { Failures.Add(error); return RecordEventAsync(context, token); }
        public Task<ManagedDiagnosticReceipt> RecordEventAsync(ManagedDiagnosticContext context, CancellationToken token)
        { Contexts.Add(context); return Task.FromResult(new ManagedDiagnosticReceipt(context.CorrelationId, "Test", true, false)); }
    }

    [SqlImportFact]
    public async Task B_ActualStdioMcpRegistersBindingsAndRecoversAfterProcessRestart()
    {
        await BindingSetupAsync();
        var first = await QueryMcpBindingAsync();
        var next = await QueryMcpBindingAsync();
        Assert.Equal(first, next);
        Assert.Equal(1, await CountAsync("campaign_session_bindings"));
    }

    private async Task<string> QueryMcpBindingAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var start = new ProcessStartInfo("dotnet")
        { UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "EternalCycle.Persistence.Mcp.dll"));
        start.Environment["EternalCycle__Persistence__ConnectionString"] = options.Value.ConnectionString;
        start.Environment["EternalCycle__Persistence__DefaultSchema"] = "ec_fixture";
        start.Environment["EternalCycle__Persistence__DomainSchema"] = Domain;
        start.Environment["EternalCycle__CampaignBinding__Enabled"] = "true";
        start.Environment["EternalCycle__CampaignBinding__PrincipalId"] = "principal";
        start.Environment["EternalCycle__CampaignBinding__LogicalSessionId"] = "session";
        start.Environment["EternalCycle__CampaignBinding__AuthorityId"] = "reference-authority";
        start.Environment["EternalCycle__CampaignBinding__AuthorizedCampaignIds__0"] = "campaign-a";
        start.Environment["EternalCycle__CampaignBinding__AllowSoleCampaignResume"] = "true";
        start.Environment["EternalCycle__Diagnostics__DisableAutomaticFallback"] = "true";
        using var process = Process.Start(start)!;
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await Send("""{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"binding-test","version":"1"}}} """);
            await Reply(1);
            await Send("""{"jsonrpc":"2.0","method":"notifications/initialized"}""");
            await Send("""{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}""");
            var tools = (await Reply(2)).GetRawText();
            Assert.Contains("ec_resolve_campaign_binding", tools); Assert.Contains("ec_select_campaign_binding", tools);
            Assert.Contains("ec_switch_campaign_binding", tools); Assert.Contains("ec_begin_gameplay_interaction", tools);
            await Send("""{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"ec_resolve_campaign_binding","arguments":{}}}""");
            var response = await Reply(3);
            Assert.False(response.TryGetProperty("error", out _), response.GetRawText());
            var text = response.GetProperty("result").GetProperty("content")[0].GetProperty("text").GetString()!;
            using var data = JsonDocument.Parse(text);
            var binding = data.RootElement.GetProperty("data").GetProperty("binding");
            Assert.Equal("campaign-a", binding.GetProperty("campaignId").GetString());
            return binding.GetProperty("bindingId").GetString()!;
        }
        finally
        {
            process.StandardInput.Close();
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { if (!process.HasExited) process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
            await stderr;
        }
        async Task Send(string value) { await process.StandardInput.WriteLineAsync(value.AsMemory(), timeout.Token); await process.StandardInput.FlushAsync(timeout.Token); }
        async Task<JsonElement> Reply(int id)
        {
            while (await process.StandardOutput.ReadLineAsync(timeout.Token) is { } line)
            {
                using var document = JsonDocument.Parse(line);
                if (document.RootElement.TryGetProperty("id", out var value) && value.ValueKind == JsonValueKind.Number && value.GetInt32() == id)
                    return document.RootElement.Clone();
            }
            throw new InvalidOperationException("MCP ended before returning binding evidence.");
        }
    }
}
