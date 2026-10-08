using System.Diagnostics;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

// Test assembly only. No credential, environment switch or production MCP tool
// can instantiate this human-event source. Recreated adapters redeliver immutable evidence.
internal sealed class TestPlayerIngress(TrustedPlayerSubmission? evidence) : ITrustedPlayerSubmissionIngress
{
    public Task<TrustedPlayerSubmission?> ResolveAsync(string reference, CancellationToken token) =>
        Task.FromResult(reference == "host-delivery" ? evidence : null);
}

public sealed class PlayerInteractionContractTests
{
    internal static CampaignBindingOptions Policy(string session = "session") => new()
    {
        Enabled = true, PrincipalId = "principal", LogicalSessionId = session, AuthorityId = "reference-authority",
        AuthorizedCampaignIds = ["campaign-a", "campaign-b"], AllowSoleCampaignResume = true, AllowSelection = true, AllowSwitch = true
    };
    internal static CampaignBindingScope Scope(CampaignBindingOptions? policy = null)
    {
        var value = policy ?? Policy();
        return new(value.PrincipalId, value.LogicalSessionId, value.AuthorityId, Array.AsReadOnly(value.AuthorizedCampaignIds), true, true, true);
    }
    internal static readonly PlayerInteractionOptions Enabled = new() { Enabled = true, AllowRecoveryCancellation = true };
    internal static TrustedPlayerSubmission Submission(string binding, string id = "submission-1") =>
        new("principal", "session", "reference-authority", id, binding, TrustedSubmissionOrigin.HostUserEvent,
            "protected-event", "protected-input", new string('A', 64), DateTimeOffset.Parse("2026-01-01T00:00:00+00:00"));

    [Fact]
    public async Task NoTrustedHostOrUnrecognizedDeliveryCannotReachStorage()
    {
        var store = new RefusingInteractionStore();
        var absent = new PlayerInteractionService(store, Options.Create(Policy()), Options.Create(Enabled));
        Assert.Equal("TRUSTED_SUBMISSION_REQUIRED", (await absent.AcceptAsync("model-claim", default)).Code);
        var ingress = new PlayerInteractionService(store, Options.Create(Policy()), Options.Create(Enabled), new TestPlayerIngress(Submission("binding")));
        Assert.Equal("TRUSTED_SUBMISSION_REQUIRED", (await ingress.AcceptAsync("model-claim", default)).Code);
        Assert.Equal(0, store.Calls);
    }

    [Theory]
    [InlineData("principal")]
    [InlineData("session")]
    [InlineData("authority")]
    [InlineData("origin")]
    [InlineData("submission")]
    [InlineData("hash")]
    [InlineData("reference")]
    [InlineData("timestamp")]
    [InlineData("decision")]
    public async Task ForgedOrMalformedProvenanceFailsBeforeDisclosure(string fault)
    {
        var evidence = Submission("binding");
        evidence = fault switch
        {
            "principal" => evidence with { PrincipalId = "other" },
            "session" => evidence with { LogicalSessionId = "other" },
            "authority" => evidence with { AuthorityId = "other" },
            "origin" => evidence with { Origin = (TrustedSubmissionOrigin)999 },
            "submission" => evidence with { SubmissionId = "" },
            "hash" => evidence with { InputSha256 = "not-a-hash" },
            "reference" => evidence with { InputReference = new string('x', 257) },
            "timestamp" => evidence with { SubmittedAt = default },
            _ => evidence with { PendingDecisionId = "decision", PendingDecisionRevision = null }
        };
        var store = new RefusingInteractionStore();
        var result = await new PlayerInteractionService(store, Options.Create(Policy()), Options.Create(Enabled), new TestPlayerIngress(evidence)).AcceptAsync("host-delivery", default);
        Assert.Equal("SUBMISSION_PROVENANCE_INVALID", result.Code); Assert.Null(result.Data); Assert.Equal(0, store.Calls);
    }

    [Theory]
    [InlineData(PlayerInteractionState.RECEIVED, false)]
    [InlineData(PlayerInteractionState.ENTRY_PENDING, false)]
    [InlineData(PlayerInteractionState.OPEN, false)]
    [InlineData(PlayerInteractionState.PERSISTING, false)]
    [InlineData(PlayerInteractionState.AWAITING_PLAYER_INPUT, true)]
    [InlineData(PlayerInteractionState.COMPLETED, true)]
    [InlineData(PlayerInteractionState.BLOCKED, true)]
    [InlineData(PlayerInteractionState.CANCELLED, true)]
    public void YieldIsDerivedNotMutableState(PlayerInteractionState state, bool yielded) =>
        Assert.Equal(yielded, PlayerInteractionLifecycle.Yielded(state));

    [Theory]
    [InlineData(PlayerInteractionState.RECEIVED, PlayerInteractionState.ENTRY_PENDING, "Entry", true)]
    [InlineData(PlayerInteractionState.RECEIVED, PlayerInteractionState.OPEN, "Entry", false)]
    [InlineData(PlayerInteractionState.ENTRY_PENDING, PlayerInteractionState.OPEN, "Entry", true)]
    [InlineData(PlayerInteractionState.ENTRY_PENDING, PlayerInteractionState.OPEN, "Recovery", false)]
    [InlineData(PlayerInteractionState.ENTRY_PENDING, PlayerInteractionState.AWAITING_PLAYER_INPUT, "Entry", true)]
    [InlineData(PlayerInteractionState.OPEN, PlayerInteractionState.PERSISTING, "Persistence", true)]
    [InlineData(PlayerInteractionState.OPEN, PlayerInteractionState.COMPLETED, "Yield", true)]
    [InlineData(PlayerInteractionState.OPEN, PlayerInteractionState.AWAITING_PLAYER_INPUT, "Yield", true)]
    [InlineData(PlayerInteractionState.OPEN, PlayerInteractionState.COMPLETED, "Entry", false)]
    [InlineData(PlayerInteractionState.PERSISTING, PlayerInteractionState.COMPLETED, "Persistence", true)]
    [InlineData(PlayerInteractionState.PERSISTING, PlayerInteractionState.OPEN, "Persistence", true)]
    [InlineData(PlayerInteractionState.PERSISTING, PlayerInteractionState.BLOCKED, "Recovery", true)]
    [InlineData(PlayerInteractionState.BLOCKED, PlayerInteractionState.ENTRY_PENDING, "Recovery", true)]
    [InlineData(PlayerInteractionState.AWAITING_PLAYER_INPUT, PlayerInteractionState.OPEN, "Entry", false)]
    [InlineData(PlayerInteractionState.AWAITING_PLAYER_INPUT, PlayerInteractionState.CANCELLED, "Recovery", true)]
    [InlineData(PlayerInteractionState.COMPLETED, PlayerInteractionState.OPEN, "Entry", false)]
    [InlineData(PlayerInteractionState.CANCELLED, PlayerInteractionState.OPEN, "Recovery", false)]
    public void CanonicalTransitionsRequireTheirOwner(PlayerInteractionState from, PlayerInteractionState to, string owner, bool allowed) =>
        Assert.Equal(allowed, PlayerInteractionLifecycle.Allows(from, to, Enum.Parse<PlayerInteractionOwner>(owner)));

    [Fact]
    public void McpSurfaceCannotMintTransitionOrResolveAndVerifiedInputHasNoPublicConstructor()
    {
        var tool = Assert.Single(typeof(PlayerInteractionTools).GetMethods(), value => value.GetCustomAttributes(typeof(McpServerToolAttribute), false).Length != 0);
        Assert.Equal("GetAsync", tool.Name);
        Assert.Equal(new[] { "interactionId", "token" }, tool.GetParameters().Select(value => value.Name));
        Assert.Empty(typeof(VerifiedPlayerSubmission).GetConstructors());
        Assert.DoesNotContain(typeof(SqlServerPlayerInteractionStore).GetMethods(), value => value.Name.Contains("Open", StringComparison.Ordinal) || value.Name.Contains("Transition", StringComparison.Ordinal));
        Assert.DoesNotContain("YIELDED", Enum.GetNames<PlayerInteractionState>());
    }

    [Fact]
    public void MigrationAndRegistryArePackagedAndDoNotInferOldInput()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "Schema");
        var template = File.ReadAllText(Path.Combine(root, "014_player_interactions.template.sql"));
        Assert.Equal(File.ReadAllText(Path.Combine(root, "014_player_interactions.sql")).ReplaceLineEndings("\n"), SqlServerSchemaMigration.RenderDomain(template, "ec_domain").ReplaceLineEndings("\n"));
        Assert.DoesNotContain("INSERT", template);
        foreach (var code in new[] { "TRUSTED_SUBMISSION_REQUIRED", "SUBMISSION_PROVENANCE_INVALID", "SUBMISSION_IDENTITY_CONFLICT",
            "INTERACTION_BINDING_UNAVAILABLE", "PLAYER_INTERACTION_ACTIVE", "PLAYER_INTERACTION_TRANSITION_INVALID", "PLAYER_DECISION_CONFLICT",
            "PLAYER_INTERACTION_STALE", "PLAYER_INTERACTION_PERSISTENCE_UNRESOLVED", "PLAYER_INTERACTION_SCHEMA_REQUIRED", "PLAYER_INTERACTION_RECOVERY_FAILED",
            "BINDING_SWITCH_INTERACTION_BLOCKED", "BINDING_SWITCH_DECISION_BLOCKED" })
            Assert.True(EternalCycleErrorRegistry.Get(code).Found, code);
    }

    private sealed class RefusingInteractionStore : IPlayerInteractionStore
    {
        public int Calls;
        public Task<PlayerInteractionAcceptance> AcceptAsync(CampaignBindingScope scope, VerifiedPlayerSubmission submission, string correlationId, CancellationToken token)
        { Calls++; throw new InvalidOperationException("Untrusted lookup"); }
        public Task<PlayerInteractionStatus?> RecoverAsync(CampaignBindingScope scope, string? id, CancellationToken token)
        { Calls++; throw new InvalidOperationException("Untrusted lookup"); }
    }
}

public sealed partial class CompiledRulesArtifactSqlImportTests
{
    private SqlServerCampaignBindingStore CBindingStore() => new(options, new ConfiguredCampaignSchemaResolver(options),
        new SqlServerCampaignDirectoryService(options, new ConfiguredCampaignSchemaResolver(options)), Options.Create(new CompiledRuleRuntimeOptions()),
        new SqlPlayerInteractionSwitchSafety(options, Options.Create(PlayerInteractionContractTests.Enabled)));
    private CampaignBindingService CBindings() => new(CBindingStore(), Options.Create(BindingOptions()));
    private SqlServerPlayerInteractionStore CStore() => new(options, CBindingStore(), Options.Create(PlayerInteractionContractTests.Enabled));
    private PlayerInteractionService CService(TrustedPlayerSubmission? evidence = null, string session = "session", IManagedDiagnosticRecorder? diagnostics = null) =>
        new(CStore(), Options.Create(BindingOptions(session: session)), Options.Create(PlayerInteractionContractTests.Enabled),
            evidence is null ? null : new TestPlayerIngress(evidence), diagnostics);
    private Task CSchemaAsync() => ExecuteAsync(SqlServerSchemaMigration.RenderDomain(File.ReadAllText(MigrationPath("014_player_interactions.template.sql")), Domain));
    private async Task<CampaignSessionBinding> CSetupAsync(int campaigns = 1)
    {
        await BindingSetupAsync(campaigns); await CSchemaAsync();
        var service = CBindings(); var result = await service.ResolveAsync(default);
        return Bound(campaigns == 1 ? result : await service.ChangeAsync(CampaignBindingAction.Select, Select(result.Data!), default));
    }
    private static PlayerInteractionStatus Interaction(ManagedOperationResult<PlayerInteractionStatus> result)
    { Assert.True(result.Success, result.Code + ": " + result.Message); return Assert.IsType<PlayerInteractionStatus>(result.Data); }
    private async Task<PlayerInteractionStatus> CAcceptAsync(CampaignSessionBinding binding, string id = "submission-1") =>
        Interaction(await CService(PlayerInteractionContractTests.Submission(binding.BindingId, id)).AcceptAsync("host-delivery", default));

    [SqlImportFact]
    public async Task C_TrustedIntakeIsImmutablePreEntryAndRestartRedeliveryIsIdempotent()
    {
        var binding = await CSetupAsync(); var evidence = PlayerInteractionContractTests.Submission(binding.BindingId);
        var first = Interaction(await CService(evidence).AcceptAsync("host-delivery", default));
        Assert.Equal(PlayerInteractionState.RECEIVED, first.State); Assert.False(first.GameplayEntryCompleted); Assert.False(first.GameplayMutationAuthorized);
        Assert.Equal(binding.BindingId, first.BindingId); Assert.Equal(binding.Generation, first.BindingGeneration);
        Assert.Equal(binding.CampaignId, first.CampaignId); Assert.Equal(1, first.StartingCampaignVersion);
        Assert.Equal(first, Interaction(await CService(evidence).AcceptAsync("host-delivery", default)));
        Assert.Equal(first, Interaction(await CService().RecoverAsync(null, default)));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions WHERE source_interaction_id LIKE N'PI-%';"));
    }

    [SqlImportTheory]
    [InlineData("input")]
    [InlineData("binding")]
    [InlineData("origin")]
    [InlineData("decision")]
    public async Task C_ConflictingIdentityCannotReplayOrRetarget(string fault)
    {
        var binding = await CSetupAsync(); var evidence = PlayerInteractionContractTests.Submission(binding.BindingId);
        var first = Interaction(await CService(evidence).AcceptAsync("host-delivery", default));
        var changed = fault switch
        {
            "input" => evidence with { InputSha256 = new string('B', 64) },
            "binding" => evidence with { BindingId = "other-binding" },
            "origin" => evidence with { ProvenanceReference = "different-event" },
            _ => evidence with { PendingDecisionId = "other-decision", PendingDecisionRevision = 1 }
        };
        Assert.Equal("SUBMISSION_IDENTITY_CONFLICT", (await CService(changed).AcceptAsync("host-delivery", default)).Code);
        Assert.Equal(first, Interaction(await CService().RecoverAsync(first.InteractionId, default)));
    }

    [SqlImportTheory]
    [InlineData("missing")]
    [InlineData("suspended")]
    [InlineData("wrong-session")]
    [InlineData("unavailable")]
    [InlineData("closed")]
    public async Task C_InvalidBindingsCannotAcceptOrTransferInput(string fault)
    {
        var binding = await CSetupAsync(2);
        if (fault == "suspended") await CBindings().ChangeAsync(CampaignBindingAction.Suspend, new("suspend", null, binding.BindingId, binding.Revision, true), default);
        if (fault == "unavailable") await ExecuteAsync("DELETE FROM [ec_fixture].campaigns WHERE campaign_id = N'campaign-a';");
        if (fault == "closed")
            Assert.True((await CBindings().ChangeAsync(CampaignBindingAction.Switch, Select((await CBindings().ResolveAsync(default)).Data!, 1, "switch"), default)).Success);
        var evidence = PlayerInteractionContractTests.Submission(fault == "missing" ? "missing" : binding.BindingId);
        if (fault == "wrong-session") evidence = evidence with { LogicalSessionId = "another-session" };
        Assert.Equal("INTERACTION_BINDING_UNAVAILABLE", (await CService(evidence, session: fault == "wrong-session" ? "another-session" : "session").AcceptAsync("host-delivery", default)).Code);
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
    }

    [SqlImportFact]
    public async Task C_RevisionAndNarrowEntryRecoveryAreDurableButCannotGrantOpen()
    {
        var binding = await CSetupAsync(); var first = await CAcceptAsync(binding); var scope = PlayerInteractionContractTests.Scope();
        var pending = await CStore().PrepareEntryAsync(scope, first.InteractionId, 1, "entry", default);
        Assert.Equal(PlayerInteractionState.ENTRY_PENDING, pending.State); Assert.False(pending.GameplayEntryCompleted);
        Assert.Equal(pending, await CStore().PrepareEntryAsync(scope, first.InteractionId, 1, "entry", default));
        Assert.Equal("PLAYER_INTERACTION_STALE", (await Assert.ThrowsAsync<ManagedServiceException>(() => CStore().BlockAsync(scope, first.InteractionId, 1, "block", default))).Code);
        var blocked = await CStore().BlockAsync(scope, first.InteractionId, 2, "block", default);
        Assert.True(blocked.Yielded); Assert.True(blocked.SwitchBlocked);
        var resumed = await CStore().ResumeEntryAsync(scope, first.InteractionId, 3, "resume", default);
        Assert.Equal(PlayerInteractionState.ENTRY_PENDING, resumed.State);
        Assert.Equal(resumed, Interaction(await CService().RecoverAsync(first.InteractionId, default)));
        Assert.Equal("PLAYER_INTERACTION_TRANSITION_INVALID", (await Assert.ThrowsAsync<ManagedServiceException>(() => CStore().PrepareEntryAsync(scope, first.InteractionId, 4, "entry-again", default))).Code);
        Assert.Equal(3, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interaction_receipts;"));
    }

    [SqlImportFact]
    public async Task C_StaleCampaignBaselineCannotBeSilentlyRebased()
    {
        var binding = await CSetupAsync(); var first = await CAcceptAsync(binding);
        await ExecuteAsync("UPDATE [ec_fixture].campaigns SET active_version = 2 WHERE campaign_id = N'campaign-a';");
        Assert.Equal("PLAYER_INTERACTION_STALE", (await Assert.ThrowsAsync<ManagedServiceException>(() => CStore().PrepareEntryAsync(PlayerInteractionContractTests.Scope(), first.InteractionId, 1, "entry", default))).Code);
        Assert.Equal(1, Interaction(await CService().RecoverAsync(first.InteractionId, default)).StartingCampaignVersion);
    }

    [SqlImportFact]
    public async Task C_IdenticalTextInTwoActualSubmissionsIsNotTextDeduplication()
    {
        var binding = await CSetupAsync(); var first = await CAcceptAsync(binding);
        var cancelled = await CStore().CancelAsync(PlayerInteractionContractTests.Scope(), first.InteractionId, 1, "cancel", null, null, default);
        Assert.Equal(PlayerInteractionState.CANCELLED, cancelled.State);
        var next = await CAcceptAsync(binding, "submission-2");
        Assert.NotEqual(first.InteractionId, next.InteractionId);
        Assert.Equal(2, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
        // A lost acknowledgement of the old submission recovers history, not current input.
        var historical = Interaction(await CService(PlayerInteractionContractTests.Submission(binding.BindingId)).AcceptAsync("host-delivery", default));
        Assert.Equal(cancelled.InteractionId, historical.InteractionId); Assert.Equal(cancelled.State, historical.State);
        Assert.False(historical.MayReceiveSubmission); Assert.True(historical.SwitchBlocked);
    }

    private async Task<PlayerInteractionStatus> CQuestionAsync(CampaignSessionBinding binding)
    {
        var first = await CAcceptAsync(binding); var scope = PlayerInteractionContractTests.Scope();
        await CStore().PrepareEntryAsync(scope, first.InteractionId, 1, "entry", default);
        return await CStore().ClarifyAsync(scope, first.InteractionId, 2, "question", "protected-question", "protected-allowed-response-scope", default);
    }

    [SqlImportFact]
    public async Task C_RealResponseLinksPendingDecisionWithoutSelectingAnyAlternative()
    {
        var binding = await CSetupAsync(); var question = await CQuestionAsync(binding); var decision = question.PendingDecision!;
        Assert.Equal(PlayerInteractionState.AWAITING_PLAYER_INPUT, question.State); Assert.True(question.Yielded); Assert.True(question.MayReceiveSubmission);
        Assert.Equal("PLAYER_DECISION_CONFLICT", (await CService(PlayerInteractionContractTests.Submission(binding.BindingId, "reply")).AcceptAsync("host-delivery", default)).Code);
        var evidence = PlayerInteractionContractTests.Submission(binding.BindingId, "reply") with { PendingDecisionId = decision.DecisionId, PendingDecisionRevision = decision.Revision };
        var next = Interaction(await CService(evidence).AcceptAsync("host-delivery", default));
        Assert.Equal(decision.DecisionId, next.RespondingToDecisionId); Assert.Equal(PlayerDecisionState.Pending, next.PendingDecision!.State);
        Assert.Equal(next.InteractionId, next.PendingDecision.RelatedInteractionId); Assert.False(next.GameplayEntryCompleted);
        Assert.Equal(next, Interaction(await CService(evidence).AcceptAsync("host-delivery", default)));
        var original = Interaction(await CService().RecoverAsync(question.InteractionId, default));
        Assert.Equal(PlayerInteractionState.AWAITING_PLAYER_INPUT, original.State); Assert.Equal(next.InteractionId, original.SuccessorInteractionId);
        Assert.Equal(1, await ScalarAsync("""
            SELECT COUNT(*) FROM [import_domain].player_pending_decisions
            WHERE JSON_VALUE(decision_json, '$.SelectedOption') IS NULL
              AND JSON_VALUE(decision_json, '$.AllowedResponseScopeReference') = N'protected-allowed-response-scope';
            """));
    }

    [SqlImportFact]
    public async Task C_AssistantContinuationAndConversationProposalLeaveDecisionUnresolved()
    {
        var binding = await CSetupAsync(2); var question = await CQuestionAsync(binding);
        var untrusted = CService();
        Assert.Equal("TRUSTED_SUBMISSION_REQUIRED", (await untrusted.AcceptAsync("assistant-chose-option-a", default)).Code);
        Assert.Equal(question, Interaction(await untrusted.RecoverAsync(null, default)));
        Assert.Equal("BINDING_SWITCH_DECISION_BLOCKED", (await CBindings().ChangeAsync(CampaignBindingAction.Switch, Select((await CBindings().ResolveAsync(default)).Data!, 1, "switch"), default)).Code);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_pending_decisions WHERE decision_status = N'Pending';"));
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions WHERE source_interaction_id LIKE N'PI-%';"));
    }

    [SqlImportFact]
    public async Task C_ExplicitRecoveryCancellationAndAbandonmentNeverChooseAndPermitSafeSwitch()
    {
        var binding = await CSetupAsync(2); var question = await CQuestionAsync(binding); var scope = PlayerInteractionContractTests.Scope();
        Assert.Equal("PLAYER_DECISION_CONFLICT", (await Assert.ThrowsAsync<ManagedServiceException>(() => CStore().CancelAsync(scope, question.InteractionId, 3, "abandon", null, null, default))).Code);
        var cancelled = await CStore().CancelAsync(scope, question.InteractionId, 3, "abandon", question.PendingDecision!.DecisionId, 1, default);
        Assert.Equal(PlayerDecisionState.Abandoned, cancelled.PendingDecision!.State); Assert.Equal(PlayerInteractionState.CANCELLED, cancelled.State);
        var successor = Bound(await CBindings().ChangeAsync(CampaignBindingAction.Switch, Select((await CBindings().ResolveAsync(default)).Data!, 1, "switch"), default));
        Assert.Equal("campaign-b", successor.CampaignId); Assert.Equal(binding.BindingId, successor.PredecessorBindingId);
        var historical = Interaction(await CService().RecoverAsync(question.InteractionId, default));
        Assert.Equal(binding.BindingId, historical.BindingId); Assert.False(historical.BindingValid);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_pending_decisions WHERE decision_status = N'Abandoned';"));
    }

    [SqlImportTheory]
    [InlineData(PlayerInteractionState.RECEIVED)]
    [InlineData(PlayerInteractionState.ENTRY_PENDING)]
    [InlineData(PlayerInteractionState.OPEN)]
    [InlineData(PlayerInteractionState.PERSISTING)]
    [InlineData(PlayerInteractionState.BLOCKED)]
    public async Task C_EveryUnresolvedCanonicalStateBlocksSwitch(PlayerInteractionState state)
    {
        var binding = await CSetupAsync(2); var first = await CAcceptAsync(binding);
        // Disposable storage fixtures represent future D/E states, not a production grant.
        await ExecuteAsync($"UPDATE [import_domain].player_interactions SET interaction_state = N'{state}', interaction_json = JSON_MODIFY(interaction_json, '$.State', N'{state}');");
        Assert.Equal("BINDING_SWITCH_INTERACTION_BLOCKED", (await CBindings().ChangeAsync(CampaignBindingAction.Switch, Select((await CBindings().ResolveAsync(default)).Data!, 1, "switch"), default)).Code);
        Assert.Equal(first.BindingId, Bound(await CBindings().ResolveAsync(default)).BindingId);
    }

    [SqlImportFact]
    public async Task C_UnknownPersistenceCannotBeConcealedByTerminalState()
    {
        var binding = await CSetupAsync(2); var first = await CAcceptAsync(binding);
        await ExecuteAsync("""
            UPDATE [import_domain].player_interactions SET interaction_state = N'COMPLETED', persistence_unknown = 1,
              interaction_json = JSON_MODIFY(JSON_MODIFY(interaction_json, '$.State', N'COMPLETED'), '$.PersistenceUnknown', CAST(1 AS bit));
            """);
        var status = Interaction(await CService().RecoverAsync(first.InteractionId, default));
        Assert.False(status.MayReceiveSubmission); Assert.True(status.SwitchBlocked);
        Assert.Equal("PLAYER_INTERACTION_ACTIVE", (await CService(PlayerInteractionContractTests.Submission(binding.BindingId, "new")).AcceptAsync("host-delivery", default)).Code);
        Assert.Equal("BINDING_SWITCH_INTERACTION_BLOCKED", (await CBindings().ChangeAsync(CampaignBindingAction.Switch, Select((await CBindings().ResolveAsync(default)).Data!, 1, "switch"), default)).Code);
    }

    [SqlImportFact]
    public async Task C_ConcurrentDeliveriesConvergeAndDistinctInputsDoNotMerge()
    {
        var binding = await CSetupAsync(); var evidence = PlayerInteractionContractTests.Submission(binding.BindingId);
        var repeats = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => CService(evidence).AcceptAsync("host-delivery", default)));
        Assert.Single(repeats.Select(value => Interaction(value).InteractionId).Distinct());
        Assert.Equal("PLAYER_INTERACTION_ACTIVE", (await CService(evidence with { SubmissionId = "distinct" }).AcceptAsync("host-delivery", default)).Code);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
    }

    [SqlImportFact]
    public async Task C_SubmissionAndSwitchShareAuthoritativeLockAndCannotBothWin()
    {
        var binding = await CSetupAsync(2); var service = CBindings();
        var switchRequest = Select((await service.ResolveAsync(default)).Data!, 1, "switch-race");
        var intake = CService(PlayerInteractionContractTests.Submission(binding.BindingId)).AcceptAsync("host-delivery", default);
        var switching = service.ChangeAsync(CampaignBindingAction.Switch, switchRequest, default);
        await Task.WhenAll(intake, switching);
        Assert.NotEqual(intake.Result.Success, switching.Result.Success);
        if (intake.Result.Success)
        {
            Assert.Equal(binding.BindingId, Interaction(intake.Result).BindingId);
            Assert.Equal("BINDING_SWITCH_INTERACTION_BLOCKED", switching.Result.Code);
            Assert.Equal(binding.BindingId, Bound(await service.ResolveAsync(default)).BindingId);
        }
        else
        {
            Assert.Equal("INTERACTION_BINDING_UNAVAILABLE", intake.Result.Code);
            Assert.Equal("campaign-b", Bound(switching.Result).CampaignId);
            Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
        }
    }

    [SqlImportFact]
    public async Task C_SafeSwitchWithoutInputAndOldDuplicateNeverRetargetsSuccessor()
    {
        var binding = await CSetupAsync(2); var first = await CAcceptAsync(binding);
        await CStore().CancelAsync(PlayerInteractionContractTests.Scope(), first.InteractionId, 1, "cancel", null, null, default);
        var next = Bound(await CBindings().ChangeAsync(CampaignBindingAction.Switch, Select((await CBindings().ResolveAsync(default)).Data!, 1, "switch"), default));
        var replay = await CAcceptAsync(binding);
        Assert.Equal(first.InteractionId, replay.InteractionId); Assert.Equal(binding.BindingId, replay.BindingId); Assert.False(replay.BindingValid);
        Assert.Equal(next.BindingId, Bound(await CBindings().ResolveAsync(default)).BindingId);
    }

    [SqlImportTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task C_InterruptedAdmissionRollsBackIdentityAndDecisionLinks(bool cancel)
    {
        var binding = await CSetupAsync(); var question = await CQuestionAsync(binding);
        using var cancellation = new CancellationTokenSource();
        var store = new SqlServerPlayerInteractionStore(options, CBindingStore(), Options.Create(PlayerInteractionContractTests.Enabled))
        { AfterInteractionInsert = () => { if (cancel) cancellation.Cancel(); else throw new ManagedServiceException("PLAYER_INTERACTION_RECOVERY_FAILED", "Injected failure."); } };
        var evidence = PlayerInteractionContractTests.Submission(binding.BindingId, "reply") with { PendingDecisionId = question.PendingDecision!.DecisionId, PendingDecisionRevision = 1 };
        var service = new PlayerInteractionService(store, Options.Create(BindingOptions()), Options.Create(PlayerInteractionContractTests.Enabled), new TestPlayerIngress(evidence));
        if (cancel) await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.AcceptAsync("host-delivery", cancellation.Token));
        else Assert.Equal("PLAYER_INTERACTION_RECOVERY_FAILED", (await service.AcceptAsync("host-delivery", default)).Code);
        Assert.Equal(question, Interaction(await CService().RecoverAsync(null, default)));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
        Assert.NotNull(Interaction(await CService(evidence).AcceptAsync("host-delivery", default)).RespondingToDecisionId);
    }

    [SqlImportFact]
    public async Task C_UpgradeFromThirteenIsOptInRepeatSafeAndHistoryPreserving()
    {
        await MigrateAsync(); await PublicationMigrationAsync(); await BindingSetupAsync(); await SeedBindingCanonAsync();
        var binding = Bound(await Bindings().ResolveAsync(default)); var active = JsonSerializer.Serialize(await Legacy.GetActiveAsync("eternal-cycle-core", default));
        var baseline = new SqlServerSchemaBootstrapExecutor(options, new ConfiguredCampaignSchemaResolver(options), campaignBinding: Options.Create(BindingOptions()));
        Assert.Empty((await baseline.PlanAsync(null, default)).MigrationIds);
        var opted = new SqlServerSchemaBootstrapExecutor(options, new ConfiguredCampaignSchemaResolver(options), campaignBinding: Options.Create(BindingOptions()), playerInteraction: Options.Create(PlayerInteractionContractTests.Enabled));
        Assert.Equal(new[] { "014_player_interactions" }, (await opted.PlanAsync(null, default)).MigrationIds);
        var diagnostics = new BindingDiagnostics();
        Assert.Equal("PLAYER_INTERACTION_SCHEMA_REQUIRED", (await CService(diagnostics: diagnostics).RecoverAsync(null, default)).Code);
        Assert.IsType<SqlException>(Assert.Single(diagnostics.Failures).InnerException);
        await opted.ExecuteAsync(null, default); await opted.ExecuteAsync(null, default);
        Assert.Empty((await opted.PlanAsync(null, default)).MigrationIds);
        Assert.Equal(binding, Bound(await CBindings().ResolveAsync(default)));
        Assert.Equal(active, JsonSerializer.Serialize(await Legacy.GetActiveAsync("eternal-cycle-core", default)));
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions WHERE status = N'Completed';"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions WHERE campaign_version = 1;"));
    }

    [SqlImportFact]
    public async Task C_DiagnosticsAreBoundedSanitizedAndCorrelatedNotPrivateInput()
    {
        var binding = await CSetupAsync(); var recorder = new BindingDiagnostics(); var evidence = PlayerInteractionContractTests.Submission(binding.BindingId);
        var result = await CService(evidence, diagnostics: recorder).AcceptAsync("host-delivery", default);
        var status = Interaction(result); var context = Assert.Single(recorder.Contexts);
        Assert.Equal(result.CorrelationId, context.CorrelationId); Assert.Equal(status.InteractionId, context.OperationId);
        Assert.DoesNotContain(evidence.InputReference, context.SafeDetail!); Assert.DoesNotContain(evidence.ProvenanceReference, context.SafeDetail!);
        Assert.DoesNotContain(evidence.SubmissionId, context.SafeDetail!); Assert.DoesNotContain(options.Value.ConnectionString, context.SafeDetail!);
        Assert.InRange(context.SafeDetail!.Length, 1, 2000);
        Assert.Null((await CService(session: "other-session").RecoverAsync(status.InteractionId, default)).Data);
    }

    [SqlImportFact]
    public async Task C_ActualMcpProcessOnlyReadsAndRecoversTrustedInteractionAcrossRestart()
    {
        var binding = await CSetupAsync(); var question = await CQuestionAsync(binding);
        Assert.Equal(question.InteractionId, await QueryMcpInteractionAsync());
        Assert.Equal(question.InteractionId, await QueryMcpInteractionAsync());
        Assert.Equal(question, Interaction(await CService().RecoverAsync(null, default)));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
    }

    [SqlImportFact]
    public async Task C_TwoConcurrentActualInputsHaveOneAcceptedWinnerAndNoMerge()
    {
        var binding = await CSetupAsync();
        var results = await Task.WhenAll(CService(PlayerInteractionContractTests.Submission(binding.BindingId, "first")).AcceptAsync("host-delivery", default),
            CService(PlayerInteractionContractTests.Submission(binding.BindingId, "second")).AcceptAsync("host-delivery", default));
        Assert.Single(results, value => value.Success);
        Assert.Equal("PLAYER_INTERACTION_ACTIVE", Assert.Single(results, value => !value.Success).Code);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
    }

    [SqlImportFact]
    public async Task C_PendingExistingTransactionBlocksCancelNewInputAndSwitch()
    {
        var binding = await CSetupAsync(2); var first = await CAcceptAsync(binding);
        await ExecuteAsync($$"""
            INSERT INTO [ec_fixture].save_transactions (transaction_id, campaign_id, idempotency_key, request_hash, request_json,
                parent_version, candidate_version, source_interaction_id, affected_set_json, status, started_at)
            VALUES (N'pending', N'campaign-a', N'pending', REPLICATE('A',64), N'{}', 1, 2, N'{{first.InteractionId}}', N'[]', N'Staged', SYSUTCDATETIME());
            """);
        var status = Interaction(await CService().RecoverAsync(first.InteractionId, default));
        Assert.Equal("pending", status.PendingTransactionId); Assert.False(status.MayReceiveSubmission); Assert.True(status.SwitchBlocked);
        Assert.Equal("PLAYER_INTERACTION_PERSISTENCE_UNRESOLVED", (await Assert.ThrowsAsync<ManagedServiceException>(() =>
            CStore().CancelAsync(PlayerInteractionContractTests.Scope(), first.InteractionId, 1, "cancel", null, null, default))).Code);
        Assert.False((await CService(PlayerInteractionContractTests.Submission(binding.BindingId, "new")).AcceptAsync("host-delivery", default)).Success);
        Assert.Equal("BINDING_SWITCH_BLOCKED", (await CBindings().ChangeAsync(CampaignBindingAction.Switch, Select((await CBindings().ResolveAsync(default)).Data!, 0, "switch"), default)).Code);
        Assert.Equal(PlayerInteractionState.RECEIVED, Interaction(await CService().RecoverAsync(first.InteractionId, default)).State);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions WHERE transaction_id = N'pending';"));
    }

    [SqlImportFact]
    public async Task C_CancellationRequiresTrustedRecoveryGrantAndNeverErasesHistory()
    {
        var binding = await CSetupAsync(); var first = await CAcceptAsync(binding);
        var denied = new SqlServerPlayerInteractionStore(options, CBindingStore(), Options.Create(new PlayerInteractionOptions { Enabled = true }));
        Assert.Equal("PLAYER_INTERACTION_TRANSITION_INVALID", (await Assert.ThrowsAsync<ManagedServiceException>(() =>
            denied.CancelAsync(PlayerInteractionContractTests.Scope(), first.InteractionId, 1, "cancel", null, null, default))).Code);
        Assert.Equal(first, Interaction(await CService().RecoverAsync(first.InteractionId, default)));
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interaction_receipts;"));
    }

    [SqlImportFact]
    public async Task C_TrustedRelatedReplyCannotAbandonOrReopenOriginImplicitly()
    {
        var binding = await CSetupAsync(); var question = await CQuestionAsync(binding);
        var evidence = PlayerInteractionContractTests.Submission(binding.BindingId, "reply") with { PendingDecisionId = question.PendingDecision!.DecisionId, PendingDecisionRevision = 1 };
        var reply = Interaction(await CService(evidence).AcceptAsync("host-delivery", default)); var scope = PlayerInteractionContractTests.Scope();
        Assert.Equal("PLAYER_DECISION_CONFLICT", (await Assert.ThrowsAsync<ManagedServiceException>(() =>
            CStore().CancelAsync(scope, question.InteractionId, 4, "abandon", question.PendingDecision.DecisionId, 2, default))).Code);
        Assert.Equal("PLAYER_INTERACTION_TRANSITION_INVALID", (await Assert.ThrowsAsync<ManagedServiceException>(() =>
            CStore().PrepareEntryAsync(scope, question.InteractionId, 4, "reopen", default))).Code);
        await CStore().CancelAsync(scope, reply.InteractionId, 1, "cancel-reply", null, null, default);
        var closed = await CStore().CancelAsync(scope, question.InteractionId, 4, "abandon", question.PendingDecision.DecisionId, 2, default);
        Assert.Equal(PlayerDecisionState.Abandoned, closed.PendingDecision!.State);
        Assert.Equal(2, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
    }

    [SqlImportFact]
    public async Task C_BlockingReceivedResumesOnlyItsOriginalEntryGate()
    {
        var binding = await CSetupAsync(); var first = await CAcceptAsync(binding); var scope = PlayerInteractionContractTests.Scope();
        await CStore().BlockAsync(scope, first.InteractionId, 1, "block", default);
        var resumed = await CStore().ResumeEntryAsync(scope, first.InteractionId, 2, "resume", default);
        Assert.Equal(PlayerInteractionState.ENTRY_PENDING, resumed.State); Assert.False(resumed.GameplayEntryCompleted);
        Assert.Equal("SUBMISSION_IDENTITY_CONFLICT", (await Assert.ThrowsAsync<ManagedServiceException>(() =>
            CStore().PrepareEntryAsync(scope, first.InteractionId, 2, "resume", default))).Code);
    }

    [SqlImportFact]
    public async Task C_McpWithoutAnyHumanEventFailsClosedAndCreatesNothing()
    {
        await CSetupAsync();
        Assert.Equal("", await QueryMcpInteractionAsync(expectMissing: true));
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
    }

    [SqlImportFact]
    public async Task C_HistoricalDecisionIsNotSubstitutedByLaterQuestionOnSameBinding()
    {
        var binding = await CSetupAsync(); var original = await CQuestionAsync(binding); var scope = PlayerInteractionContractTests.Scope();
        await CStore().CancelAsync(scope, original.InteractionId, 3, "abandon", original.PendingDecision!.DecisionId, 1, default);
        var next = await CAcceptAsync(binding, "later-submission");
        await CStore().PrepareEntryAsync(scope, next.InteractionId, 1, "later-entry", default);
        var question = await CStore().ClarifyAsync(scope, next.InteractionId, 2, "later-question", "protected-later-question", "protected-later-response", default);
        var historical = Interaction(await CService().RecoverAsync(original.InteractionId, default));
        Assert.Equal(original.PendingDecision.DecisionId, historical.PendingDecision!.DecisionId);
        Assert.Equal(PlayerDecisionState.Abandoned, historical.PendingDecision.State); Assert.True(historical.SwitchBlocked);
        Assert.NotEqual(historical.PendingDecision.DecisionId, question.PendingDecision!.DecisionId);
        Assert.Equal(question, Interaction(await CService().RecoverAsync(null, default)));
    }

    [SqlImportTheory]
    [InlineData("revision")]
    [InlineData("generation")]
    [InlineData("campaign")]
    [InlineData("decision-projection")]
    public async Task C_StorageConstraintsRejectContradictoryOwnershipAndInvalidJsonNumbers(string fault)
    {
        var binding = await CSetupAsync(); var question = await CQuestionAsync(binding);
        var sql = fault switch
        {
            "revision" => "UPDATE [import_domain].player_interactions SET interaction_json = JSON_MODIFY(interaction_json, '$.Revision', N'invalid');",
            "generation" => "UPDATE [import_domain].player_interactions SET binding_generation = 9;",
            "campaign" => "UPDATE [import_domain].player_interactions SET campaign_id = N'other-campaign';",
            _ => "UPDATE [import_domain].player_pending_decisions SET decision_json = JSON_MODIFY(decision_json, '$.RelatedInteractionId', N'forged-related-input');"
        };
        Assert.Equal(547, (await Assert.ThrowsAsync<SqlException>(() => ExecuteAsync(sql))).Number);
        Assert.Equal(question, Interaction(await CService().RecoverAsync(null, default)));
    }

    [SqlImportFact]
    public async Task C_IncompleteAggregateFailsSafelyWithInternalCauseNotPartialAuthorization()
    {
        var binding = await CSetupAsync(); await CAcceptAsync(binding);
        await ExecuteAsync("UPDATE [import_domain].player_interactions SET interaction_json = JSON_MODIFY(interaction_json, '$.StartingEvidence', NULL);");
        var recorder = new BindingDiagnostics();
        var result = await CService(diagnostics: recorder).RecoverAsync(null, default);
        Assert.Equal("PLAYER_INTERACTION_RECOVERY_FAILED", result.Code); Assert.Null(result.Data);
        Assert.IsType<JsonException>(Assert.Single(recorder.Failures).InnerException);
        Assert.DoesNotContain("StartingEvidence", result.Message); Assert.DoesNotContain(options.Value.ConnectionString, result.Message);
    }

    [SqlImportFact]
    public async Task C_MissingDecisionEvidenceIsNotPermissionForAnotherInputOrSwitch()
    {
        var binding = await CSetupAsync(2); await CQuestionAsync(binding);
        await ExecuteAsync("DELETE FROM [import_domain].player_pending_decisions;");
        Assert.Equal("PLAYER_INTERACTION_RECOVERY_FAILED", (await CService().RecoverAsync(null, default)).Code);
        Assert.Equal("PLAYER_INTERACTION_ACTIVE", (await CService(PlayerInteractionContractTests.Submission(binding.BindingId, "new-input")).AcceptAsync("host-delivery", default)).Code);
        Assert.Equal("BINDING_SWITCH_INTERACTION_BLOCKED", (await CBindings().ChangeAsync(CampaignBindingAction.Switch, Select((await CBindings().ResolveAsync(default)).Data!, 1, "switch"), default)).Code);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interactions;"));
        Assert.Equal(1, await CountAsync("campaign_session_bindings"));
    }

    private async Task<string> QueryMcpInteractionAsync(bool expectMissing = false)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "EternalCycle.Persistence.Mcp.dll"));
        start.Environment["EternalCycle__Persistence__ConnectionString"] = options.Value.ConnectionString;
        start.Environment["EternalCycle__Persistence__DefaultSchema"] = "ec_fixture"; start.Environment["EternalCycle__Persistence__DomainSchema"] = Domain;
        start.Environment["EternalCycle__CampaignBinding__Enabled"] = "true"; start.Environment["EternalCycle__PlayerInteraction__Enabled"] = "true";
        start.Environment["EternalCycle__CampaignBinding__PrincipalId"] = "principal"; start.Environment["EternalCycle__CampaignBinding__LogicalSessionId"] = "session";
        start.Environment["EternalCycle__CampaignBinding__AuthorityId"] = "reference-authority"; start.Environment["EternalCycle__CampaignBinding__AuthorizedCampaignIds__0"] = "campaign-a";
        start.Environment["EternalCycle__Diagnostics__DisableAutomaticFallback"] = "true";
        using var process = Process.Start(start)!; var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
        try
        {
            await Send("""{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"interaction-test","version":"1"}}}"""); await Reply(1);
            await Send("""{"jsonrpc":"2.0","method":"notifications/initialized"}""");
            await Send("""{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}""");
            var listed = (await Reply(2)).GetProperty("result").GetProperty("tools");
            Assert.Single(listed.EnumerateArray(), tool => tool.GetProperty("name").GetString() == "ec_get_player_interaction_status");
            Assert.DoesNotContain("ec_begin_gameplay_interaction", listed.GetRawText()); Assert.DoesNotContain("ec_accept_player_submission", listed.GetRawText());
            await Send("""{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"ec_accept_player_submission","arguments":{"isNewPlayerMessage":true,"origin":"user","choice":"A"}}}""");
            var forged = await Reply(3); Assert.True(forged.TryGetProperty("error", out _) || forged.GetProperty("result").GetProperty("isError").GetBoolean());
            await Send("""{"jsonrpc":"2.0","id":4,"method":"tools/call","params":{"name":"ec_get_player_interaction_status","arguments":{}}}""");
            var response = await Reply(4); Assert.False(response.TryGetProperty("error", out _), response.GetRawText());
            using var document = JsonDocument.Parse(response.GetProperty("result").GetProperty("content")[0].GetProperty("text").GetString()!);
            if (expectMissing)
            {
                Assert.Equal("TRUSTED_SUBMISSION_REQUIRED", document.RootElement.GetProperty("code").GetString());
                Assert.True(!document.RootElement.TryGetProperty("data", out var absent) || absent.ValueKind == JsonValueKind.Null);
                return "";
            }
            var data = document.RootElement.GetProperty("data");
            Assert.False(data.GetProperty("gameplayEntryCompleted").GetBoolean()); Assert.Equal("Pending", data.GetProperty("pendingDecision").GetProperty("state").GetString());
            Assert.DoesNotContain("protected-question", response.GetRawText()); Assert.DoesNotContain(options.Value.ConnectionString, response.GetRawText());
            return data.GetProperty("interactionId").GetString()!;
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
            throw new InvalidOperationException("MCP ended before returning interaction status.");
        }
    }
}
