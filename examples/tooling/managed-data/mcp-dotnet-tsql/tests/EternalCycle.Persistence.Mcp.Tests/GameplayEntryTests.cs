using System.Diagnostics;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class GameplayEntryContractTests
{
    [Fact]
    public void D_ToolConsumesOnlyExistingInteractionAndNoCallerGrant()
    {
        var method = typeof(GameplayEntryTools).GetMethod("BeginAsync")!;
        Assert.Equal("ec_begin_gameplay_interaction", method.GetCustomAttributes(typeof(McpServerToolAttribute), false).Cast<McpServerToolAttribute>().Single().Name);
        Assert.Equal(new[] { "RequestId", "ExpectedRevision", "InteractionId", "QueryTerms" }, typeof(GameplayEntryRequest).GetProperties().Select(property => property.Name));
        Assert.Empty(typeof(PreparedGameplayEntry).GetConstructors()); Assert.Empty(typeof(VerifiedGameplayEntryContext).GetConstructors());
        Assert.False(new GameplayEntryOptions().Enabled);
    }

    [Fact]
    public void D_MigrationIsPackagedAndMatchesRoutedTemplate()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Schema");
        Assert.Equal(File.ReadAllText(Path.Combine(path, "015_gameplay_entry.sql")).ReplaceLineEndings("\n"),
            SqlServerSchemaMigration.RenderDomain(File.ReadAllText(Path.Combine(path, "015_gameplay_entry.template.sql")), "ec_domain").ReplaceLineEndings("\n"));
    }

    [Theory]
    [InlineData("DISABLED")][InlineData("REQUEST_INVALID")][InlineData("CONFLICT")][InlineData("BASELINE_STALE")]
    [InlineData("PROFILE_MISMATCH")][InlineData("RULES_INCOMPLETE")][InlineData("CANON_INCOMPLETE")]
    [InlineData("CANON_UNAVAILABLE")][InlineData("CANON_STALE")][InlineData("CANON_FAILED")]
    public void D_BlockingErrorsAreRegisteredAndBounded(string suffix)
    {
        var result = EternalCycleErrorRegistry.Get("GAMEPLAY_ENTRY_" + suffix);
        Assert.True(result.Found); Assert.InRange(result.Error!.SafeDescription.Length, 1, 512);
        Assert.InRange(result.Error.RecoveryGuidance.Length, 1, 512);
    }
}

public sealed partial class CompiledRulesArtifactSqlImportTests
{
    private static readonly RecordAddress DSession = new("current-session", "active");
    private static GameplayEntryOptions DOptions(int budget = 8000) => new()
    { Enabled = true, MaximumEstimatedRuleTokens = budget, CurrentSessions = new Dictionary<string, RecordAddress> { ["campaign-a"] = DSession } };
    private SqlServerCampaignPersistenceStore DPersistence() => new(options, new ConfiguredCampaignSchemaResolver(options),
        new SqlServerDurabilityService(options, new ConfiguredCampaignSchemaResolver(options)), NullLogger<SqlServerCampaignPersistenceStore>.Instance);
    private SqlServerCampaignBindingStore DBindingStore(bool compiled = true) => new(options, new ConfiguredCampaignSchemaResolver(options),
        new SqlServerCampaignDirectoryService(options, new ConfiguredCampaignSchemaResolver(options)), Options.Create(new CompiledRuleRuntimeOptions { Enabled = compiled }),
        new SqlPlayerInteractionSwitchSafety(options, Options.Create(PlayerInteractionContractTests.Enabled)));
    private SqlServerPlayerInteractionStore DStore(bool compiled = true, Action? before = null) => new(options, DBindingStore(compiled),
        Options.Create(PlayerInteractionContractTests.Enabled), Options.Create(DOptions()), DPersistence()) { BeforeEntryCommit = before };
    private GameplayEntryService DService(IGameplayEntryCanonReader? reader = null, int budget = 8000,
        bool compiled = true, Action? before = null, CampaignBindingOptions? policy = null, IManagedDiagnosticRecorder? diagnostics = null) => new(
        DStore(compiled, before), reader ?? new GameplayEntryCanonReader(DPersistence(), Options.Create(DOptions())), Runtime("eternal-cycle-core"),
        new PublishedRuleContextProvider(Legacy, new SqlServerRulePreparationStore(options), Options.Create(new ManagedRuleServiceOptions()), new ConfiguredCampaignSchemaResolver(options)),
        Options.Create(policy ?? BindingOptions()), Options.Create(PlayerInteractionContractTests.Enabled), Options.Create(DOptions(budget)),
        Options.Create(new ManagedRuleServiceOptions()), diagnostics);
    private Task DSchemaAsync() => ExecuteAsync(SqlServerSchemaMigration.RenderDomain(File.ReadAllText(MigrationPath("015_gameplay_entry.template.sql")), Domain));
    private static GameplayEntrySessionPlan DPlan() => new(new string('A', 64), "NORMAL", [], ["fighting"], [], [],
        new[] { EntryCanonRole.PlayerState, EntryCanonRole.Controller, EntryCanonRole.Protagonist, EntryCanonRole.Incarnation, EntryCanonRole.Time, EntryCanonRole.Scene, EntryCanonRole.Placement }
            .Select(role => new GameplayEntryReadRequirement(role, new("entry-fixture", role.ToString()), ExpectedRevision: 1)).ToArray());
    private async Task<(CampaignSessionBinding Binding, PlayerInteractionStatus Interaction)> DSetupAsync(bool compiled = true, int campaigns = 1, byte[]? artifact = null)
    {
        await BindingSetupAsync(campaigns); await CSchemaAsync(); await DSchemaAsync();
        if (compiled) await ReadyAsync(await CompiledRulesImportFixtures.Approve(artifact ?? OrdinaryAuthorityCandidate.Compile().Bytes.ToArray()));
        var service = new CampaignBindingService(DBindingStore(compiled), Options.Create(BindingOptions()));
        var resolution = await service.ResolveAsync(default);
        var binding = Bound(campaigns == 1 ? resolution : await service.ChangeAsync(CampaignBindingAction.Select, Select(resolution.Data!), default));
        var intake = new PlayerInteractionService(DStore(compiled), Options.Create(BindingOptions()), Options.Create(PlayerInteractionContractTests.Enabled),
            new TestPlayerIngress(PlayerInteractionContractTests.Submission(binding.BindingId)));
        var interaction = Interaction(await intake.AcceptAsync("host-delivery", default));
        await ExecuteAsync("""
            INSERT INTO [ec_fixture].save_transactions(transaction_id,campaign_id,idempotency_key,request_hash,request_json,parent_version,candidate_version,source_interaction_id,affected_set_json,status,started_at)
            VALUES(N'd-entry-initial',N'campaign-a',N'd-entry-initial',REPLICATE('A',64),N'{}',0,1,N'fixture-initial',N'[]',N'Completed',SYSUTCDATETIME());
            """);
        await DInsertAsync(DSession, JsonSerializer.Serialize(DPlan(), SqlServerPlayerInteractionStore.Json));
        foreach (var read in DPlan().Reads) await DInsertAsync(read.Address!, "{\"canonical\":\"present\",\"certainty\":\"Not Yet Verified\"}");
        return (binding, interaction);
    }

    private async Task DInsertAsync(RecordAddress address, string payload)
    {
        await using var connection = new SqlConnection(options.Value.ConnectionString); await connection.OpenAsync();
        await using var command = new SqlCommand("""
            INSERT INTO [ec_fixture].canonical_record_versions(campaign_id,owner_domain,record_id,campaign_version,record_revision,payload_json,payload_hash,is_tombstone,transaction_id,recorded_at)
            VALUES(N'campaign-a',@owner,@id,1,1,@payload,@hash,0,N'd-entry-initial',SYSUTCDATETIME());
            """, connection);
        command.Parameters.AddWithValue("@owner", address.OwnerDomain); command.Parameters.AddWithValue("@id", address.RecordId);
        command.Parameters.AddWithValue("@payload", payload); command.Parameters.AddWithValue("@hash", GameplayEntryCanonReader.Hash(payload));
        await command.ExecuteNonQueryAsync();
    }
    private async Task DReplacePlanAsync(GameplayEntrySessionPlan plan)
    {
        await using var connection = new SqlConnection(options.Value.ConnectionString); await connection.OpenAsync();
        await using var command = new SqlCommand("UPDATE [ec_fixture].canonical_record_versions SET payload_json=@payload, payload_hash=@hash WHERE owner_domain=N'current-session';", connection);
        var payload = JsonSerializer.Serialize(plan, SqlServerPlayerInteractionStore.Json);
        command.Parameters.AddWithValue("@payload", payload); command.Parameters.AddWithValue("@hash", GameplayEntryCanonReader.Hash(payload));
        await command.ExecuteNonQueryAsync();
    }
    private static GameplayEntryRequest DRequest(PlayerInteractionStatus interaction) => new("entry-request", interaction.Revision, interaction.InteractionId);
    private static GameplayEntryResult DSuccess(ManagedOperationResult<GameplayEntryResult> result)
    { Assert.True(result.Success, result.Code + ": " + result.Message); return Assert.IsType<GameplayEntryResult>(result.Data); }
    private async Task DNonOpenAsync(string id, bool compiled = true)
    { var current = await DStore(compiled).RecoverAsync(PlayerInteractionContractTests.Scope(), id, default); Assert.NotEqual(PlayerInteractionState.OPEN, current!.State); Assert.False(current.GameplayEntryCompleted); }

    [SqlImportFact]
    public async Task D_MandatoryEntryAutomaticallyRetrievesReadsAndAtomicallyOpensWithoutCanonWrites()
    {
        var setup = await DSetupAsync(); var request = DRequest(setup.Interaction);
        await DNonOpenAsync(setup.Interaction.InteractionId);
        var before = await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions;");
        var result = DSuccess(await new GameplayEntryTools(DService()).BeginAsync(request, default));
        Assert.Equal(PlayerInteractionState.OPEN, result.Interaction.State); Assert.True(result.Interaction.GameplayEntryCompleted);
        Assert.False(result.GameplayMutationAuthorized); Assert.False(result.Interaction.GameplayMutationAuthorized);
        Assert.Equal(3, result.Interaction.Revision); Assert.Equal(1, result.Receipt!.Baseline.CampaignVersion);
        Assert.Equal(setup.Binding.BindingId, result.Receipt.BindingId); Assert.Equal(setup.Interaction.CorrelationId, result.Receipt.CorrelationId);
        Assert.Equal("gameplay.resolve", result.Receipt.Rules.Operation); Assert.True(result.Receipt.Rules.DependencyComplete);
        Assert.Contains(result.Context!.CompiledRules!.Rules, source => source.RuleSourceId == RuleCompiler.RuntimeKernelSourceId);
        Assert.Contains(result.Context.CompiledRules.Rules, source => source.RuleSourceId == RuleCompiler.GmRuntimeProcedureSourceId);
        Assert.Equal(8, result.Reads.Count); Assert.All(result.Reads, read => Assert.Equal(EntryCanonStatus.Completed, read.Status));
        Assert.Equal(before, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions;"));
        Assert.Equal(1, await ScalarAsync("SELECT active_version FROM [ec_fixture].campaigns WHERE campaign_id=N'campaign-a';"));
        var reference = await Runtime("eternal-cycle-core").GetContextAsync(new(new("eternal-cycle-core", setup.Binding.Evidence.ActiveRuleIdentity!), "gameplay.resolve", "NORMAL", ["fighting"])
            { WorldModelId = setup.Binding.Evidence.WorldModelId }, default);
        Assert.Equal(JsonSerializer.Serialize(reference.Packet), JsonSerializer.Serialize(result.Context.CompiledRules));
    }

    [SqlImportFact]
    public async Task D_LostAckRestartAndConcurrentDuplicateRecoverOneOriginalReceipt()
    {
        var setup = await DSetupAsync(); var request = DRequest(setup.Interaction);
        var results = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => DService().BeginAsync(request, default)));
        Assert.All(results, result => Assert.True(result.Success, result.Code));
        Assert.Single(results, result => !result.Data!.Recovered);
        var first = results.Single(result => !result.Data!.Recovered).Data!;
        var recovered = DSuccess(await DService().BeginAsync(request, default));
        Assert.True(recovered.Recovered); Assert.Null(recovered.Context);
        Assert.Equal(JsonSerializer.Serialize(first.Receipt), JsonSerializer.Serialize(recovered.Receipt));
        Assert.Equal(1, await CountAsync("gameplay_entries")); Assert.Equal(1, await CountAsync("player_interactions"));
        Assert.Equal(1, recovered.Receipt!.Baseline.CampaignVersion);
        Assert.Equal("GAMEPLAY_ENTRY_CONFLICT", (await DService().BeginAsync(request with { QueryTerms = ["save"] }, default)).Code);
        Assert.Equal("GAMEPLAY_ENTRY_CONFLICT", (await DService().BeginAsync(request with { RequestId = "competing" }, default)).Code);
    }

    [SqlImportTheory]
    [InlineData("missing", "GAMEPLAY_ENTRY_CANON_UNAVAILABLE")]
    [InlineData("stale", "GAMEPLAY_ENTRY_CANON_STALE")]
    [InlineData("role", "GAMEPLAY_ENTRY_CANON_INCOMPLETE")]
    [InlineData("visibility", "GAMEPLAY_ENTRY_CANON_UNAVAILABLE")]
    [InlineData("input", "GAMEPLAY_ENTRY_CANON_STALE")]
    [InlineData("malformed", "GAMEPLAY_ENTRY_CANON_INCOMPLETE")]
    [InlineData("bounds", "GAMEPLAY_ENTRY_CANON_INCOMPLETE")]
    public async Task D_IncompleteCanonNeverBecomesEmptyOrOpen(string fault, string expected)
    {
        var setup = await DSetupAsync(); var plan = DPlan();
        switch (fault)
        {
            case "missing": await ExecuteAsync("DELETE FROM [ec_fixture].canonical_record_versions WHERE record_id=N'Scene';"); break;
            case "stale": plan = plan with { Reads = plan.Reads.Select(read => read.Role == EntryCanonRole.Scene ? read with { ExpectedRevision = 9 } : read).ToArray() }; break;
            case "role": plan = plan with { Reads = plan.Reads.Where(read => read.Role != EntryCanonRole.Scene).ToArray() }; break;
            case "visibility": plan = plan with { Reads = plan.Reads.Select(read => read.Role == EntryCanonRole.Scene ? read with { Visibility = EntryCanonVisibility.Restricted } : read).ToArray() }; break;
            case "input": plan = plan with { InputSha256 = new string('B', 64) }; break;
            case "malformed": await ExecuteAsync("UPDATE [ec_fixture].canonical_record_versions SET payload_json=N'{}' WHERE owner_domain=N'current-session';"); break;
            case "bounds": plan = plan with { Reads = Enumerable.Repeat(plan.Reads[0], 33).ToArray() }; break;
        }
        if (fault is not ("missing" or "malformed")) await DReplacePlanAsync(plan);
        var result = await DService().BeginAsync(DRequest(setup.Interaction), default);
        Assert.False(result.Success); Assert.Equal(expected, result.Code); await DNonOpenAsync(setup.Interaction.InteractionId);
        Assert.Equal(1, await CountAsync("gameplay_entries"));
        Assert.Equal(1, await ScalarAsync("SELECT active_version FROM [ec_fixture].campaigns WHERE campaign_id=N'campaign-a';"));
    }

    [SqlImportFact]
    public async Task D_LegitimateAbsenceIsExplicitOwnerEvidenceNotAMissingRecord()
    {
        var setup = await DSetupAsync(); var plan = DPlan();
        await DReplacePlanAsync(plan with { Reads = plan.Reads.Select(read => read.Role == EntryCanonRole.Incarnation ?
            read with { Address = null, ExpectedRevision = null, LegitimatelyAbsent = true, AbsenceReason = "Owning Current Session explicitly records no current embodiment." } : read).ToArray() });
        var result = DSuccess(await DService().BeginAsync(DRequest(setup.Interaction), default));
        Assert.Equal(EntryCanonStatus.LegitimatelyAbsent, result.Reads.Single(read => read.Role == EntryCanonRole.Incarnation).Status);
        Assert.Equal(7, result.Context!.Canon.Count);
    }

    [SqlImportTheory]
    [InlineData("procedure", "RULE_RETRIEVAL_ARTIFACT_INCONSISTENT")]
    [InlineData("required", "RULE_RETRIEVAL_REQUEST_INVALID")]
    [InlineData("budget", "RULE_RETRIEVAL_BUDGET_INSUFFICIENT")]
    public async Task D_MissingRuleAuthorityOrBudgetFailsWithoutFallback(string fault, string expected)
    {
        // A technically valid artifact without the mandatory procedure cannot
        // authorize D even though B's profile/bootstrap preflight is ready.
        var missing = fault == "procedure" ? CompiledRulesImportFixtures.Change(OrdinaryAuthorityCandidate.Compile().Bytes.ToArray(), node =>
        {
            foreach (var key in new[] { "ruleSources", "snippets" })
            {
                var rows = node[key]!.AsArray();
                foreach (var row in rows.Where(row => row!["ruleSourceId"]!.GetValue<string>() == "gm-runtime-procedure").ToArray()) rows.Remove(row);
            }
        }) : null;
        var setup = await DSetupAsync(artifact: missing);
        if (fault == "required") await DReplacePlanAsync(DPlan() with { RequiredRuleSourceIds = ["missing-specialist"] });
        var result = await DService(budget: fault == "budget" ? 1 : 8000).BeginAsync(DRequest(setup.Interaction), default);
        Assert.Equal(expected, result.Code); Assert.False(result.Success); await DNonOpenAsync(setup.Interaction.InteractionId);
    }

    [SqlImportFact]
    public async Task D_InterruptedPreparationRemainsDurableAndSameRequestCanFinishAfterRestart()
    {
        var setup = await DSetupAsync(); var request = DRequest(setup.Interaction);
        var reader = new DPausedReader(new GameplayEntryCanonReader(DPersistence(), Options.Create(DOptions())));
        using var cancel = new CancellationTokenSource(); var work = DService(reader).BeginAsync(request, cancel.Token);
        await reader.Reached.Task.WaitAsync(TimeSpan.FromSeconds(15)); cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => work);
        await DNonOpenAsync(setup.Interaction.InteractionId); Assert.Equal(1, await CountAsync("gameplay_entries"));
        var restarted = DSuccess(await DService().BeginAsync(request, default));
        Assert.False(restarted.Recovered); Assert.Equal(PlayerInteractionState.OPEN, restarted.Interaction.State);
    }

    [SqlImportTheory]
    [InlineData("version", "GAMEPLAY_ENTRY_BASELINE_STALE")]
    [InlineData("record", "GAMEPLAY_ENTRY_CANON_STALE")]
    [InlineData("bootstrap", "GAMEPLAY_ENTRY_BASELINE_STALE")]
    [InlineData("rules", "RULE_ACTIVATION_REQUIRED")]
    [InlineData("suspend", "INTERACTION_BINDING_UNAVAILABLE")]
    [InlineData("cancel", "PLAYER_INTERACTION_STALE")]
    [InlineData("persistence", "PLAYER_INTERACTION_PERSISTENCE_UNRESOLVED")]
    public async Task D_FinalTransactionRejectsAuthorityChangesDuringPreparation(string fault, string expected)
    {
        var setup = await DSetupAsync(); var request = DRequest(setup.Interaction);
        var reader = new DPausedReader(new GameplayEntryCanonReader(DPersistence(), Options.Create(DOptions())));
        var work = DService(reader).BeginAsync(request, default);
        await reader.Reached.Task.WaitAsync(TimeSpan.FromSeconds(15));
        switch (fault)
        {
            case "version": await ExecuteAsync("UPDATE [ec_fixture].campaigns SET active_version=2 WHERE campaign_id=N'campaign-a';"); break;
            case "record": await ExecuteAsync("UPDATE [ec_fixture].canonical_record_versions SET record_revision=2 WHERE record_id=N'Scene';"); break;
            case "bootstrap": await ExecuteAsync("UPDATE [import_domain].gm_host_configurations SET configuration_revision=configuration_revision+1;"); break;
            case "rules":
                var changed = CompiledRulesImportFixtures.Change(OrdinaryAuthorityCandidate.Compile().Bytes.ToArray(), node => node["ruleset"]!["source"]!["value"] = "changed-immutable-source");
                await ReadyAsync(await CompiledRulesImportFixtures.Approve(changed)); break;
            case "suspend":
                var bindings = new CampaignBindingService(DBindingStore(), Options.Create(BindingOptions()));
                Assert.True((await bindings.ChangeAsync(CampaignBindingAction.Suspend, new("suspend", null, setup.Binding.BindingId, setup.Binding.Revision, true), default)).Success); break;
            case "cancel": await DStore().CancelAsync(PlayerInteractionContractTests.Scope(), setup.Interaction.InteractionId, 2, "cancel-entry", null, null, default); break;
            case "persistence": await ExecuteAsync("UPDATE [ec_fixture].save_transactions SET status=N'FailedReadback' WHERE transaction_id=N'd-entry-initial';"); break;
        }
        reader.Continue.TrySetResult(); var result = await work;
        Assert.Equal(expected, result.Code); await DNonOpenAsync(setup.Interaction.InteractionId);
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].gameplay_entries WHERE JSON_VALUE(entry_json,'$.Receipt.ReceiptId') IS NOT NULL;"));
    }

    [SqlImportFact]
    public async Task D_EntrySwitchRaceKeepsOriginalCampaignAndCBlocker()
    {
        var setup = await DSetupAsync(campaigns: 2);
        var reader = new DPausedReader(new GameplayEntryCanonReader(DPersistence(), Options.Create(DOptions())));
        var work = DService(reader).BeginAsync(DRequest(setup.Interaction), default);
        await reader.Reached.Task.WaitAsync(TimeSpan.FromSeconds(15));
        var bindings = new CampaignBindingService(DBindingStore(), Options.Create(BindingOptions()));
        var discovery = await bindings.ResolveAsync(default);
        var change = await bindings.ChangeAsync(CampaignBindingAction.Switch, Select(discovery.Data!, 1, "switch-during-entry"), default);
        Assert.Equal("BINDING_SWITCH_INTERACTION_BLOCKED", change.Code);
        reader.Continue.TrySetResult(); var result = DSuccess(await work);
        Assert.Equal("campaign-a", result.Receipt!.CampaignId); Assert.Equal(setup.Binding.BindingId, result.Receipt.BindingId);
        Assert.Equal(1, await CountAsync("campaign_session_bindings"));
    }

    [SqlImportFact]
    public async Task D_FinalCommitFailureRollsBackReceiptAndOpenTogether()
    {
        var setup = await DSetupAsync(); var request = DRequest(setup.Interaction);
        using var cancel = new CancellationTokenSource();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => DService(before: cancel.Cancel).BeginAsync(request, cancel.Token));
        await DNonOpenAsync(setup.Interaction.InteractionId);
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].gameplay_entries WHERE JSON_VALUE(entry_json,'$.Receipt.ReceiptId') IS NOT NULL;"));
        Assert.Equal(PlayerInteractionState.OPEN, DSuccess(await DService().BeginAsync(request, default)).Interaction.State);
    }

    [SqlImportFact]
    public async Task D_PendingDecisionReplyEntersWithoutChoosingOrReopeningOriginalInput()
    {
        var setup = await DSetupAsync(); var scope = PlayerInteractionContractTests.Scope();
        await DStore().PrepareEntryAsync(scope, setup.Interaction.InteractionId, 1, "clarify-prep", default);
        var question = await DStore().ClarifyAsync(scope, setup.Interaction.InteractionId, 2, "question", "protected-question", "protected-response-scope", default);
        var submission = PlayerInteractionContractTests.Submission(setup.Binding.BindingId, "actual-reply") with
        { PendingDecisionId = question.PendingDecision!.DecisionId, PendingDecisionRevision = question.PendingDecision.Revision };
        var reply = Interaction(await new PlayerInteractionService(DStore(), Options.Create(BindingOptions()), Options.Create(PlayerInteractionContractTests.Enabled),
            new TestPlayerIngress(submission)).AcceptAsync("host-delivery", default));
        var result = DSuccess(await DService().BeginAsync(DRequest(reply), default));
        Assert.Equal(PlayerDecisionState.Pending, result.Receipt!.Decision!.Status.State);
        Assert.Equal(reply.InteractionId, result.Receipt.Decision.Status.RelatedInteractionId);
        Assert.Equal("protected-question", result.Receipt.Decision.ProtectedQuestionReference);
        Assert.Equal(PlayerInteractionState.AWAITING_PLAYER_INPUT, (await DStore().RecoverAsync(scope, setup.Interaction.InteractionId, default))!.State);
    }

    [SqlImportFact]
    public async Task D_OtherSessionAndStaleRevisionCannotBorrowInput()
    {
        var setup = await DSetupAsync(); var request = DRequest(setup.Interaction);
        Assert.Equal("TRUSTED_SUBMISSION_REQUIRED", (await DService(policy: BindingOptions(session: "other")).BeginAsync(request, default)).Code);
        Assert.Equal("PLAYER_INTERACTION_STALE", (await DService().BeginAsync(request with { ExpectedRevision = 99 }, default)).Code);
        Assert.Equal(0, await CountAsync("gameplay_entries")); await DNonOpenAsync(setup.Interaction.InteractionId);
    }

    [SqlImportFact]
    public async Task D_MigrationUpgradeRepeatPreserves014AndPlannerRemainsOptIn()
    {
        var setup = await CSetupAsync(); await CAcceptAsync(setup);
        var prior = JsonSerializer.Serialize(await CStore().RecoverAsync(PlayerInteractionContractTests.Scope(), null, default));
        await MigrateAsync();
        var planner = new SqlServerSchemaBootstrapExecutor(options, new ConfiguredCampaignSchemaResolver(options),
            campaignBinding: Options.Create(BindingOptions()), playerInteraction: Options.Create(PlayerInteractionContractTests.Enabled), gameplayEntry: Options.Create(DOptions()));
        Assert.Equal(new[] { "015_gameplay_entry" }, (await planner.PlanAsync(null, default)).MigrationIds);
        await planner.ExecuteAsync(null, default); await DSchemaAsync();
        Assert.Empty((await planner.PlanAsync(null, default)).MigrationIds);
        Assert.Equal(prior, JsonSerializer.Serialize(await CStore().RecoverAsync(PlayerInteractionContractTests.Scope(), null, default)));
        Assert.Equal(0, await CountAsync("gameplay_entries"));
    }

    [SqlImportTheory]
    [InlineData(null)][InlineData("not-a-revision")][InlineData("0")][InlineData("-1")]
    public async Task D_DatabaseRejectsMissingMalformedOrNonpositivePreparationRevision(string? revision)
    {
        var setup = await DSetupAsync();
        await DStore().PrepareAsync(PlayerInteractionContractTests.Scope(), DRequest(setup.Interaction), default);
        await using var connection = new SqlConnection(options.Value.ConnectionString); await connection.OpenAsync();
        await using var command = new SqlCommand("""
            UPDATE [import_domain].gameplay_entries SET entry_json = JSON_MODIFY(entry_json, '$.PendingRevision', @revision);
            """, connection);
        command.Parameters.AddWithValue("@revision", (object?)revision ?? DBNull.Value);
        var error = await Assert.ThrowsAsync<SqlException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal(547, error.Number);
        await DNonOpenAsync(setup.Interaction.InteractionId);
        Assert.Equal(PlayerInteractionState.OPEN, DSuccess(await DService().BeginAsync(DRequest(setup.Interaction), default)).Interaction.State);
    }

    private sealed class DPausedReader(IGameplayEntryCanonReader inner) : IGameplayEntryCanonReader
    {
        internal TaskCompletionSource Reached { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Continue { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<GameplayEntryCanonContext> ReadAsync(PreparedGameplayEntry entry, CancellationToken token)
        {
            var result = await inner.ReadAsync(entry, token); Reached.TrySetResult();
            await Continue.Task.WaitAsync(token); return result;
        }
    }

    [SqlImportFact]
    public async Task D_ExplicitLegacyRouteHasMandatorySafetyAndNeverFallsBackFromCompiled()
    {
        var setup = await DSetupAsync(compiled: false); var request = DRequest(setup.Interaction);
        Assert.Equal("GAMEPLAY_ENTRY_RULES_INCOMPLETE", (await DService(compiled: false).BeginAsync(request, default)).Code);
        await DNonOpenAsync(setup.Interaction.InteractionId, compiled: false);
        var previous = (await Legacy.GetActiveAsync("eternal-cycle-core", default))!;
        var documents = new[]
        {
            new RuleSourceDocument("runtime-kernel", "rules/kernel.md", "# Kernel\n\nRepository Canon remains authoritative.", new(RuleLayer.RuntimeKernel, [], [], ["*"], ["*"], [], AlwaysInclude: true, PreparationTier: RulePreparationTier.RuntimeKernel)),
            new RuleSourceDocument("gm-host-bootstrap", "rules/bootstrap.txt", "Use canonical rules and preserve player agency.", new(RuleLayer.RuntimeKernel, [], [], ["*"], ["*"], [], AlwaysInclude: true, PreparationTier: RulePreparationTier.RuntimeKernel)),
            new RuleSourceDocument("gm-runtime-procedure", "rules/procedure.md", "# Procedure\n\nRead Canon, respect input, persist before completion.", new(RuleLayer.Core, [], [], ["*"], ["*"], [], AlwaysInclude: true, PreparationTier: RulePreparationTier.ImmediateGameplayCore))
        };
        var candidate = previous with { RuleReleaseId = "RULE-d-legacy", SourceIdentity = "d-legacy-source", State = RuleReleaseState.Candidate, Index = RuleCompiler.Compile("1.0.0", documents) };
        await Legacy.StageCandidateAsync(candidate, default);
        var preparation = new SqlServerRulePreparationStore(options); await preparation.InitializeAsync(candidate, default);
        await preparation.MarkReadyAsync(candidate.RuleReleaseId, documents.Select(document => document.RuleSourceId).ToArray(), default);
        await Legacy.SetStateAsync(candidate.RuleReleaseId, RuleReleaseState.Published, null, default); await Legacy.ActivateAsync(candidate.RulesetId, candidate.RuleReleaseId, default);
        // A new profile cannot silently reuse original input. It must be freshly
        // bound/attested through existing B/C policy, never a D fallback.
        Assert.Equal("GAMEPLAY_ENTRY_PROFILE_MISMATCH", (await DService(compiled: false).BeginAsync(request, default)).Code);
        await DStore(false).CancelAsync(PlayerInteractionContractTests.Scope(), setup.Interaction.InteractionId, 2, "cancel-old-profile", null, null, default);
        var bindings = new CampaignBindingService(DBindingStore(false), Options.Create(BindingOptions()));
        var changed = await bindings.ResolveAsync(default);
        var binding = Bound(await bindings.ChangeAsync(CampaignBindingAction.Select, Select(changed.Data!, request: "reconfirm"), default));
        var input = Interaction(await new PlayerInteractionService(DStore(false), Options.Create(BindingOptions()), Options.Create(PlayerInteractionContractTests.Enabled),
            new TestPlayerIngress(PlayerInteractionContractTests.Submission(binding.BindingId, "new-profile-input"))).AcceptAsync("host-delivery", default));
        var result = DSuccess(await DService(compiled: false).BeginAsync(DRequest(input) with { RequestId = "new-profile-entry" }, default));
        Assert.Null(result.Context!.CompiledRules); Assert.NotNull(result.Context.SourceRules);
        Assert.Equal("SourceRelease", result.Receipt!.Rules.Representation);
    }

    [SqlImportFact]
    public async Task D_CanonFailureIsBoundedStructuredAndDoesNotLeakInnerException()
    {
        var setup = await DSetupAsync(); var diagnostics = new BindingDiagnostics();
        var reader = new GameplayEntryCanonReader(new DFailingPersistence(), Options.Create(DOptions()));
        var result = await DService(reader, diagnostics: diagnostics).BeginAsync(DRequest(setup.Interaction), default);
        Assert.Equal("GAMEPLAY_ENTRY_CANON_FAILED", result.Code); Assert.Contains(result.Data!.Reads, read => read.Status == EntryCanonStatus.Failed);
        Assert.DoesNotContain("private-fixture-detail", result.Message);
        Assert.Contains(diagnostics.Failures, error => error.InnerException is IOException);
        await DNonOpenAsync(setup.Interaction.InteractionId);
    }

    private sealed class DFailingPersistence : ICampaignPersistenceStore
    {
        public Task<ReadRecordsResult> ReadRecordsAsync(string campaign, IReadOnlyList<RecordAddress> addresses, CancellationToken token) => throw new IOException("private-fixture-detail");
        public Task<PersistenceStatus> GetStatusAsync(string campaign, CancellationToken token) => throw new NotSupportedException();
        public Task<CommitResult> CommitAsync(CommitCampaignRequest request, string hash, CancellationToken token) => throw new NotSupportedException();
        public Task<CommitResult> RetryAsync(string campaign, string transaction, CancellationToken token) => throw new NotSupportedException();
    }

    [SqlImportTheory]
    [InlineData("missing-evidence")][InlineData("extra-record")]
    public async Task D_FinalBoundaryRejectsIncompleteProviderEvidenceOrExtraDisclosure(string fault)
    {
        var setup = await DSetupAsync();
        var reader = new DAlteredReader(new GameplayEntryCanonReader(DPersistence(), Options.Create(DOptions())), fault);
        var result = await DService(reader).BeginAsync(DRequest(setup.Interaction), default);
        Assert.Equal("GAMEPLAY_ENTRY_CANON_INCOMPLETE", result.Code); await DNonOpenAsync(setup.Interaction.InteractionId);
        Assert.Null(result.Data!.Context);
    }
    private sealed class DAlteredReader(IGameplayEntryCanonReader inner, string fault) : IGameplayEntryCanonReader
    {
        public async Task<GameplayEntryCanonContext> ReadAsync(PreparedGameplayEntry entry, CancellationToken token)
        {
            var result = await inner.ReadAsync(entry, token);
            return fault == "missing-evidence" ? result with { Evidence = result.Evidence.Take(1).ToArray() } :
                result with { Records = result.Records.Append(new("unrequested", "private", 1, 1, "{}", false)).ToArray() };
        }
    }

    [SqlImportFact]
    public async Task D_ReadinessAndSelectorMismatchPreventAnyOpenGrant()
    {
        var setup = await DSetupAsync(); var request = DRequest(setup.Interaction);
        await DReplacePlanAsync(DPlan() with { CampaignMode = "HARD" });
        Assert.Equal("GAMEPLAY_ENTRY_PROFILE_MISMATCH", (await DService().BeginAsync(request, default)).Code);
        await DReplacePlanAsync(DPlan());
        await ExecuteAsync("UPDATE [import_domain].gm_host_configurations SET configuration_state=N'Required';");
        Assert.Equal("GM_HOST_CONFIGURATION_REQUIRED", (await DService().BeginAsync(request, default)).Code);
        await DNonOpenAsync(setup.Interaction.InteractionId);
    }

    [SqlImportFact]
    public async Task D_RecoveredReceiptDoesNotReopenCancelledInputOrRefreshBaseline()
    {
        var setup = await DSetupAsync(); var request = DRequest(setup.Interaction);
        var first = DSuccess(await DService().BeginAsync(request, default));
        await DStore().CancelAsync(PlayerInteractionContractTests.Scope(), setup.Interaction.InteractionId, 3, "cancel-after-entry", null, null, default);
        await ExecuteAsync("UPDATE [ec_fixture].campaigns SET active_version=2 WHERE campaign_id=N'campaign-a';");
        var recovered = DSuccess(await DService().BeginAsync(request, default));
        Assert.True(recovered.Recovered); Assert.Equal(PlayerInteractionState.CANCELLED, recovered.Interaction.State);
        Assert.Equal(1, recovered.Receipt!.Baseline.CampaignVersion); Assert.Equal(first.Receipt!.ReceiptId, recovered.Receipt.ReceiptId);
        Assert.Null(recovered.Context); Assert.False(recovered.GameplayMutationAuthorized);
    }

    [SqlImportFact]
    public async Task D_ActualMcpConsumesTestIngressThenRecoversReceiptAcrossProcessRestart()
    {
        var setup = await DSetupAsync(); var request = DRequest(setup.Interaction);
        var first = await DProcessAsync(request);
        Assert.True(first.GetProperty("success").GetBoolean(), first.GetRawText());
        var data = first.GetProperty("data"); Assert.Equal("OPEN", data.GetProperty("interaction").GetProperty("state").GetString());
        Assert.True(data.GetProperty("interaction").GetProperty("gameplayEntryCompleted").GetBoolean());
        Assert.Equal("gameplay.resolve", data.GetProperty("receipt").GetProperty("rules").GetProperty("operation").GetString());
        Assert.Equal(8, data.GetProperty("reads").GetArrayLength());
        Assert.False(data.GetProperty("gameplayMutationAuthorized").GetBoolean());
        var second = await DProcessAsync(request);
        Assert.True(second.GetProperty("success").GetBoolean(), second.GetRawText());
        var recovered = second.GetProperty("data"); Assert.True(recovered.GetProperty("recovered").GetBoolean());
        Assert.Equal(data.GetProperty("receipt").GetRawText(), recovered.GetProperty("receipt").GetRawText());
        Assert.False(recovered.TryGetProperty("context", out var context) && context.ValueKind != JsonValueKind.Null);
        Assert.Equal(1, await CountAsync("gameplay_entries")); Assert.Equal(1, await CountAsync("player_interactions"));
    }

    [SqlImportFact]
    public async Task D_ActualMcpWithoutTrustedIntakeFailsClosedAndCannotMintInput()
    {
        var binding = await CSetupAsync(); await DSchemaAsync();
        var result = await DProcessAsync(new("missing-input", 1));
        Assert.False(result.GetProperty("success").GetBoolean()); Assert.Equal("TRUSTED_SUBMISSION_REQUIRED", result.GetProperty("code").GetString());
        Assert.Equal(0, await CountAsync("player_interactions")); Assert.Equal(0, await CountAsync("gameplay_entries"));
        Assert.Equal(1, await CountAsync("campaign_session_bindings")); Assert.NotNull(binding);
    }

    private async Task<JsonElement> DProcessAsync(GameplayEntryRequest request)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
        var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "EternalCycle.Persistence.Mcp.dll"));
        foreach (var item in new Dictionary<string, string>
        {
            ["Persistence__ConnectionString"] = options.Value.ConnectionString, ["Persistence__DefaultSchema"] = "ec_fixture", ["Persistence__DomainSchema"] = Domain,
            ["CampaignBinding__Enabled"] = "true", ["PlayerInteraction__Enabled"] = "true", ["GameplayEntry__Enabled"] = "true", ["CompiledRules__Enabled"] = "true",
            ["CampaignBinding__PrincipalId"] = "principal", ["CampaignBinding__LogicalSessionId"] = "session", ["CampaignBinding__AuthorityId"] = "reference-authority",
            ["CampaignBinding__AuthorizedCampaignIds__0"] = "campaign-a", ["GameplayEntry__CurrentSessions__campaign-a__OwnerDomain"] = DSession.OwnerDomain,
            ["GameplayEntry__CurrentSessions__campaign-a__RecordId"] = DSession.RecordId, ["Diagnostics__DisableAutomaticFallback"] = "true"
        }) start.Environment["EternalCycle__" + item.Key] = item.Value;
        using var process = Process.Start(start)!;
        var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await Send("""{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"entry-test","version":"1"}}}"""); await Reply(1);
            await Send("""{"jsonrpc":"2.0","method":"notifications/initialized"}""");
            await Send("""{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}""");
            var listed = (await Reply(2)).GetProperty("result").GetProperty("tools");
            var tool = Assert.Single(listed.EnumerateArray(), item => item.GetProperty("name").GetString() == "ec_begin_gameplay_interaction");
            Assert.DoesNotContain("campaignId", tool.GetProperty("inputSchema").GetRawText());
            Assert.DoesNotContain("ec_accept_player_submission", listed.GetRawText());
            await Send(JsonSerializer.Serialize(new { jsonrpc = "2.0", id = 3, method = "tools/call", @params = new { name = "ec_begin_gameplay_interaction", arguments = new { request } } }));
            var response = await Reply(3);
            Assert.False(response.TryGetProperty("error", out _), response.GetRawText());
            using var result = JsonDocument.Parse(response.GetProperty("result").GetProperty("content")[0].GetProperty("text").GetString()!);
            Assert.DoesNotContain(options.Value.ConnectionString, result.RootElement.GetRawText());
            return result.RootElement.Clone();
        }
        finally
        {
            process.StandardInput.Close();
            try { await process.WaitForExitAsync(timeout.Token); }
            catch (OperationCanceledException) { if (!process.HasExited) process.Kill(true); await process.WaitForExitAsync(); }
            await stderr;
        }
        async Task Send(string value) { await process.StandardInput.WriteLineAsync(value.AsMemory(), timeout.Token); await process.StandardInput.FlushAsync(timeout.Token); }
        async Task<JsonElement> Reply(int id)
        {
            while (await process.StandardOutput.ReadLineAsync(timeout.Token) is { } line)
            {
                using var document = JsonDocument.Parse(line);
                if (document.RootElement.TryGetProperty("id", out var value) && value.ValueKind == JsonValueKind.Number && value.GetInt32() == id) return document.RootElement.Clone();
            }
            throw new InvalidOperationException("MCP ended before returning entry evidence.");
        }
    }
}
