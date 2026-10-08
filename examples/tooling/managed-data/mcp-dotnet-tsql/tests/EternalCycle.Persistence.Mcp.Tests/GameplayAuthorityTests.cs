using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class GameplayAuthorityContractTests
{
    [Fact]
    public void E_ToolsCannotSupplyAChoiceOrCompleteFlagOrCreateTrustedInput()
    {
        Assert.Equal(new[] { "RequestId", "InteractionId", "ExpectedRevision", "DecisionId", "ExpectedDecisionRevision" }, typeof(GameplayDecisionRequest).GetProperties().Select(property => property.Name));
        Assert.DoesNotContain(typeof(GameplayConclusionRequest).GetProperties(), property => property.Name == "Completed");
        Assert.All(typeof(GameplayAuthorityTools).GetMethods().Where(method => method.DeclaringType == typeof(GameplayAuthorityTools)), method =>
            Assert.Single(method.GetCustomAttributes(typeof(McpServerToolAttribute), false)));
        Assert.Empty(new GameplayAuthorityOptions().AdministrativeCampaignIds);
        Assert.Empty(typeof(VerifiedPlayerSubmission).GetConstructors());
    }

    [Theory]
    [InlineData("ENTRY_REQUIRED")][InlineData("AUTHORITY_INVALID")][InlineData("MUTATION_SCOPE_DENIED")][InlineData("MUTATION_READ_REQUIRED")]
    [InlineData("DECISION_PENDING")][InlineData("TRUSTED_RESPONSE_REQUIRED")][InlineData("INTERACTION_YIELDED")][InlineData("INTERACTION_COMPLETED")]
    [InlineData("PERSISTENCE_UNKNOWN")][InlineData("COMPLETION_BLOCKED")][InlineData("AFFECTED_SET_INVALID")][InlineData("REPLAY_CONFLICT")]
    public void E_ErrorsAreRegistered(string suffix) => Assert.True(EternalCycleErrorRegistry.Get("GAMEPLAY_" + suffix).Found);

    [Fact]
    public void E_LegacySubmissionFingerprintIsUnchangedWhenNoResponseEvidenceExists()
    {
        var input = PlayerInteractionContractTests.Submission("binding");
        Assert.DoesNotContain("DecisionResponse", JsonSerializer.Serialize(input));
        Assert.DoesNotContain("DecisionResponse", JsonSerializer.Serialize(input, SqlServerPlayerInteractionStore.Json));
    }
}

public sealed partial class CompiledRulesArtifactSqlImportTests
{
    private static readonly RecordAddress EScene = new("entry-fixture", "Scene");
    private static readonly RecordAddress ENew = new("entry-fixture", "new-entity");
    private static readonly RecordAddress EInventory = new("inventory", "resources");
    private SqlServerCampaignPersistenceStore EPersistence(IDurabilityService? durability = null, CampaignBindingOptions? policy = null, Func<CancellationToken, Task>? beforeActivation = null) =>
        new(options, new ConfiguredCampaignSchemaResolver(options), durability ?? new SqlServerDurabilityService(options, new ConfiguredCampaignSchemaResolver(options)),
            NullLogger<SqlServerCampaignPersistenceStore>.Instance, DBindingStore(), Options.Create(policy ?? BindingOptions()),
            Options.Create(PlayerInteractionContractTests.Enabled), Options.Create(DOptions())) { BeforeGameplayActivation = beforeActivation };
    private SqlServerPlayerInteractionStore EStore(IDurabilityService? durability = null) => new(options, DBindingStore(),
        Options.Create(PlayerInteractionContractTests.Enabled), Options.Create(DOptions()), EPersistence(durability));
    private GameplayAuthorityService EService(IDurabilityService? durability = null, IManagedDiagnosticRecorder? diagnostics = null) => new(EStore(durability),
        new PersistenceCoordinator(EPersistence(durability)), Options.Create(BindingOptions()), diagnostics);
    private async Task<(CampaignSessionBinding Binding, GameplayEntryResult Entry)> ESetupAsync(int campaigns = 1)
    {
        var setup = await DSetupAsync(campaigns: campaigns);
        await DInsertAsync(EInventory, "{\"remaining\":5}");
        await DReplacePlanAsync(DPlan() with { Reads = DPlan().Reads.Append(new(EntryCanonRole.RelevantOwner, EInventory, ExpectedRevision: 1)).ToArray(),
            MutationScope = [new(EScene, GameplayMutationAction.Update), new(EScene, GameplayMutationAction.Tombstone), new(ENew, GameplayMutationAction.Create), new(EInventory, GameplayMutationAction.Update)] });
        return (setup.Binding, DSuccess(await DService().BeginAsync(DRequest(setup.Interaction), default)));
    }
    private static CommitCampaignRequest EWrite(string id, long version = 1, long? revision = 1, string transaction = "effect-one", RecordAddress? address = null, bool tombstone = false) =>
        new("campaign-a", transaction, transaction, version, ["entry-fixture"],
            [new((address ?? EScene).OwnerDomain, (address ?? EScene).RecordId, revision, "{\"canonical\":\"resolved\",\"certainty\":\"Not Yet Verified\"}", tombstone)], id);
    private static GameplayConclusionRequest EConclusion(PlayerInteractionStatus value, string request = "finish", string[]? affected = null) =>
        new(request, value.InteractionId, value.Revision, affected ?? []);
    private Task<PlayerInteractionStatus?> EStatus(string id) => EStore().RecoverAsync(PlayerInteractionContractTests.Scope(), id, default);
    private async Task<CampaignBindingChange> ESwitchRequestAsync(string request) =>
        Select((await new CampaignBindingService(DBindingStore(), Options.Create(BindingOptions())).ResolveAsync(default)).Data!, 1, request);
    private async Task EUnchangedAsync(long records)
    {
        Assert.Equal(1, await ScalarAsync("SELECT active_version FROM [ec_fixture].campaigns WHERE campaign_id=N'campaign-a';"));
        Assert.Equal(records, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions;"));
    }
    private static GameplayDisposition ESuccess(ManagedOperationResult<GameplayDisposition> result)
    { Assert.True(result.Success, result.Code + ": " + result.Message); return Assert.IsType<GameplayDisposition>(result.Data); }

    [SqlImportTheory]
    [InlineData("unbound")][InlineData("unknown-input")][InlineData("wrong-session")][InlineData("wrong-campaign")]
    [InlineData("before-entry")][InlineData("suspended")][InlineData("superseded")][InlineData("version")]
    [InlineData("owner")][InlineData("bootstrap")][InlineData("scope")][InlineData("completed")][InlineData("cancelled")]
    public async Task E_LegacyWritesFailClosedWithoutCurrentBcdAuthorityAndLeaveCanonUnchanged(string fault)
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId; var request = EWrite(id);
        var records = await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions;");
        var policy = BindingOptions();
        switch (fault)
        {
            case "unbound": policy = new(); break;
            case "unknown-input": request = request with { SourceInteractionId = "fabricated" }; break;
            case "wrong-session": policy = BindingOptions(session: "another"); break;
            case "wrong-campaign": request = request with { CampaignId = "campaign-b" }; break;
            case "before-entry": await ExecuteAsync("DELETE FROM [import_domain].gameplay_entries;"); break;
            case "suspended": await new CampaignBindingService(DBindingStore(), Options.Create(BindingOptions())).ChangeAsync(CampaignBindingAction.Suspend,
                new("suspend", null, setup.Binding.BindingId, setup.Binding.Revision, true), default); break;
            case "superseded": await ExecuteAsync("UPDATE [import_domain].campaign_session_bindings SET binding_status=N'CLOSED',binding_json=JSON_MODIFY(binding_json,'$.State','CLOSED');"); break;
            case "version": await ExecuteAsync("UPDATE [ec_fixture].campaigns SET active_version=2 WHERE campaign_id=N'campaign-a';"); break;
            case "owner": await ExecuteAsync("UPDATE [ec_fixture].canonical_record_versions SET record_revision=2 WHERE record_id=N'Scene';"); break;
            case "bootstrap": await ExecuteAsync("UPDATE [import_domain].gm_host_configurations SET configuration_revision=configuration_revision+1;"); break;
            case "scope": request = EWrite(id, address: new("entry-fixture", "PlayerState")); break;
            case "completed": ESuccess(await EService().ConcludeAsync(EConclusion(setup.Entry.Interaction), default)); break;
            case "cancelled": await DStore().CancelAsync(PlayerInteractionContractTests.Scope(), id, 3, "cancel", null, null, default); break;
        }
        var error = await Assert.ThrowsAsync<ManagedServiceException>(() => new PersistenceTools(new(EPersistence(policy: policy))).CommitAsync(request, default));
        Assert.True(EternalCycleErrorRegistry.Contains(error.Code), error.Code);
        Assert.Equal(fault == "version" ? 2 : 1, await ScalarAsync("SELECT active_version FROM [ec_fixture].campaigns WHERE campaign_id=N'campaign-a';"));
        Assert.Equal(records, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions;"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions;"));
    }

    [SqlImportFact]
    public async Task E_MultiDomainReceiptCompletionAndHistoricalDuplicateDoNotReplayEffects()
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId; var request = EWrite(id) with
        { AffectedOwnerDomains = ["entry-fixture", "inventory"], Mutations = EWrite(id).Mutations.Append(new(EInventory.OwnerDomain, EInventory.RecordId, 1, "{\"remaining\":4}", false)).ToArray() };
        var tool = new PersistenceTools(new(EPersistence())); var result = await tool.CommitAsync(request, default);
        Assert.Equal(PersistenceMarkers.Saved, result.Marker); Assert.NotNull(result.Receipt);
        Assert.False(result.TurnMayComplete); Assert.True(result.GameplayCompletionRequired);
        var status = (await EStatus(id))!; Assert.Equal(PlayerInteractionState.OPEN, status.State); Assert.False(status.Gameplay!.CompletedNarrationAuthorized);
        Assert.Equal(2, status.Gameplay.CampaignVersion); Assert.Equal(new[] { request.TransactionId }, status.Gameplay.ValidatedTransactionIds);
        var conclusion = EConclusion(status, affected: ["entry-fixture", "inventory"]);
        var diagnostics = new BindingDiagnostics();
        var final = ESuccess(await EService(diagnostics: diagnostics).ConcludeAsync(conclusion, default));
        Assert.True(final.CompletedNarrationAuthorized); Assert.True(final.Interaction.Yielded); Assert.False(final.EmptyAffectedSetValidated);
        var diagnostic = Assert.Single(diagnostics.Contexts);
        Assert.Equal(id, diagnostic.OperationId); Assert.Equal("campaign-a", diagnostic.CampaignId);
        using (var detail = JsonDocument.Parse(diagnostic.SafeDetail!))
        {
            Assert.Equal(setup.Entry.Receipt!.ReceiptId, detail.RootElement.GetProperty("GameplayEntryReceiptId").GetString());
            Assert.Equal(setup.Entry.Interaction.CorrelationId, detail.RootElement.GetProperty("OriginalCorrelationId").GetString());
        }
        Assert.DoesNotContain("Not Yet Verified", diagnostic.SafeDetail!);
        Assert.Equal(result.Receipt, Assert.Single(final.Receipts));
        var duplicate = await tool.CommitAsync(request, default); Assert.Equal(result.Receipt, duplicate.Receipt);
        Assert.Equal(2, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions;"));
        Assert.Equal(JsonSerializer.Serialize(final), JsonSerializer.Serialize(ESuccess(await EService().ConcludeAsync(conclusion, default))));
        Assert.Equal("GAMEPLAY_INTERACTION_COMPLETED", (await Assert.ThrowsAsync<ManagedServiceException>(() => tool.CommitAsync(EWrite(id, 2, 2, "new-after-completion"), default))).Code);
    }

    [SqlImportFact]
    public async Task E_SetOnlyPatchIsGatedAndDuplicateRecoversItsFrozenOriginal()
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId;
        var request = new PatchCampaignRequest("campaign-a", "patch", "patch", 1, ["entry-fixture"], [new(EScene.OwnerDomain, EScene.RecordId, 1, "{\"canonical\":\"patched\"}")], id);
        var tool = new PersistenceTools(new(EPersistence())); var first = await tool.PatchAsync(request, default);
        var repeat = await tool.PatchAsync(request, default); Assert.Equal(first.Receipt, repeat.Receipt);
        var records = await DPersistence().ReadRecordsAsync("campaign-a", [EScene], default);
        Assert.Contains("Not Yet Verified", Assert.Single(records.Records).PayloadJson); Assert.Equal(2, Assert.Single(records.Records).RecordRevision);
        Assert.Equal("GAMEPLAY_REPLAY_CONFLICT", (await Assert.ThrowsAsync<ManagedServiceException>(() => tool.PatchAsync(request with { Patches = [new(EScene.OwnerDomain, EScene.RecordId, 1, "{\"canonical\":\"different\"}")] }, default))).Code);
    }

    [SqlImportFact]
    public async Task E_CreateRequiresExactPermissionAndVerifiedAbsenceAndDeleteRequiresItsOwnPermission()
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId; var write = EWrite(id, revision: null, address: ENew);
        Assert.Equal("GAMEPLAY_MUTATION_READ_REQUIRED", (await Assert.ThrowsAsync<ManagedServiceException>(() => new PersistenceCoordinator(EPersistence()).CommitAsync(write, default))).Code);
        var read = await EService().ReadOwnersAsync(new(id, 3, [ENew]), default); Assert.True(read.Success, read.Code); Assert.Empty(read.Data!.Records);
        var created = await new PersistenceCoordinator(EPersistence()).CommitAsync(write, default); Assert.Equal(PersistenceMarkers.Saved, created.Marker);
        var deleted = await new PersistenceCoordinator(EPersistence()).CommitAsync(EWrite(id, 2, 1, "tombstone", tombstone: true), default);
        Assert.Equal(PersistenceMarkers.Saved, deleted.Marker);
        Assert.Empty((await DPersistence().ReadRecordsAsync("campaign-a", [EScene], default)).Records);
    }

    [SqlImportTheory]
    [InlineData("missing")][InlineData("invented-affected")][InlineData("nonempty-without-write")]
    public async Task E_MissingOrInconsistentAffectedSetIsNotEmptyEvidence(string fault)
    {
        var setup = await ESetupAsync(); var request = EConclusion(setup.Entry.Interaction);
        request = request with { AffectedOwnerDomains = fault == "missing" ? null : ["invented-owner"] };
        var result = await EService().ConcludeAsync(request, default); Assert.False(result.Success);
        Assert.Equal("GAMEPLAY_AFFECTED_SET_INVALID", result.Code); Assert.False((await EStatus(request.InteractionId))!.Yielded);
    }

    [SqlImportFact]
    public async Task E_ExplicitEmptyAffectedSetCompletesWithoutAFictitiousSaveAndCompetingConclusionsSerialize()
    {
        var setup = await ESetupAsync(); var request = EConclusion(setup.Entry.Interaction);
        var results = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => EService().ConcludeAsync(request, default)));
        Assert.All(results, result => { var data = ESuccess(result); Assert.True(data.EmptyAffectedSetValidated); Assert.True(data.CompletedNarrationAuthorized); Assert.Empty(data.Receipts); });
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions;"));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interaction_receipts;"));
        Assert.Equal("GAMEPLAY_REPLAY_CONFLICT", (await EService().ConcludeAsync(request with { AffectedOwnerDomains = ["entry-fixture"] }, default)).Code);
        Assert.Equal("GAMEPLAY_INTERACTION_COMPLETED", (await EService().ConcludeAsync(request with { RequestId = "other-complete" }, default)).Code);
    }

    [SqlImportTheory]
    [InlineData(false)][InlineData(true)]
    public async Task E_UnknownOrFailedReadbackBlocksCompletionAndRestartRetriesOnlyOriginalOutcome(bool fail)
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId;
        var durability = new EPausedDurability(fail); using var cancel = new CancellationTokenSource();
        var write = EWrite(id); var task = new PersistenceCoordinator(EPersistence(durability)).CommitAsync(write, cancel.Token);
        await durability.Reached.Task.WaitAsync(TimeSpan.FromSeconds(15));
        var pending = (await EStatus(id))!; Assert.True(pending.Gameplay!.PendingTransactionId == write.TransactionId);
        Assert.False((await EService().ConcludeAsync(EConclusion(pending, affected: ["entry-fixture"]), default)).Success);
        Assert.False((await EService().ConcludeAsync(EConclusion(pending), default)).Success);
        Assert.Equal("GAMEPLAY_PERSISTENCE_UNKNOWN", (await Assert.ThrowsAsync<ManagedServiceException>(() => new PersistenceCoordinator(EPersistence()).CommitAsync(EWrite(id, 2, 2, "replacement"), default))).Code);
        if (fail) { durability.Continue.TrySetResult(); Assert.Equal(PersistenceMarkers.Failed, (await task).Marker); }
        else { cancel.Cancel(); await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task); }
        var blocked = (await EStatus(id))!; Assert.True(blocked.Yielded); Assert.Equal(PlayerInteractionState.BLOCKED, blocked.State);
        var recovered = await EService().ReconcileAsync(id, write.TransactionId, default); Assert.True(recovered.Success, recovered.Code); Assert.Equal(PersistenceMarkers.Saved, recovered.Data!.Marker);
        Assert.Equal(2, await ScalarAsync("SELECT active_version FROM [ec_fixture].campaigns WHERE campaign_id=N'campaign-a';"));
        Assert.Equal(2, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions;"));
        var final = ESuccess(await EService().ConcludeAsync(EConclusion((await EStatus(id))!, affected: ["entry-fixture"]), default)); Assert.True(final.CompletedNarrationAuthorized);
    }

    [SqlImportFact]
    public async Task E_PersistenceFailureRetainsInternalCauseWithoutDisclosingRawPayloadOrCredentials()
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId;
        var cause = new IOException("secret=fixture-only-sensitive; private Canon payload (not diagnostic evidence)");
        var durability = new EPausedDurability(true, cause); durability.Continue.TrySetResult();
        var result = await new PersistenceCoordinator(EPersistence(durability)).CommitAsync(EWrite(id), default);
        Assert.Equal(PersistenceMarkers.Failed, result.Marker); Assert.Same(cause, result.FailureCause);
        Assert.DoesNotContain(cause.Message, JsonSerializer.Serialize(result));
        Assert.DoesNotContain("fixture-only-sensitive", (await new PersistenceCoordinator(EPersistence()).GetStatusAsync("campaign-a", default)).FailureReason ?? "");
        var diagnostics = new BindingDiagnostics();
        var retry = await EService(durability, diagnostics).ReconcileAsync(id, "effect-one", default);
        Assert.False(retry.Success); Assert.Equal("PLAYER_INTERACTION_PERSISTENCE_UNRESOLVED", retry.Code);
        Assert.Empty(diagnostics.Failures); Assert.Equal("Failed", Assert.Single(diagnostics.Contexts).Outcome);
        Assert.DoesNotContain("fixture-only-sensitive", diagnostics.Contexts[0].SafeDetail!);
        Assert.False((await EService().ConcludeAsync(EConclusion((await EStatus(id))!, affected: ["entry-fixture"]), default)).Success);
    }

    private sealed class EPausedDurability(bool fail = false, Exception? failure = null) : IDurabilityService
    {
        internal TaskCompletionSource Reached { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Continue { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<string> VerifyCompletionAsync(string campaign, long version, CancellationToken token)
        { Reached.TrySetResult(); await Continue.Task.WaitAsync(token); if (fail) throw failure ?? new IOException("Durability verification failed (test substitute)."); return "Deterministic verification substitute."; }
    }

    private async Task<GameplayDisposition> EQuestionAsync(PlayerInteractionStatus interaction, string[]? affected = null) => ESuccess(await EService().ConcludeAsync(
        EConclusion(interaction, "question", affected) with { QuestionReference = "protected:three-alternatives", ResponseScopeReference = "protected:choice-scope" }, default));

    [SqlImportFact]
    public async Task E_ThreeAlternativesAssistantContinuationCannotChooseOrAdvanceAcrossReconnect()
    {
        var setup = await ESetupAsync(2); var id = setup.Entry.Interaction.InteractionId;
        var switchRequest = await ESwitchRequestAsync("unsafe-switch");
        var question = await EQuestionAsync(setup.Entry.Interaction); Assert.True(question.QuestionPresentationAuthorized); Assert.False(question.CompletedNarrationAuthorized);
        var decision = question.Interaction.PendingDecision!; Assert.Equal(PlayerDecisionState.Pending, decision.State);
        var before = await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions;");
        for (var attempt = 0; attempt < 3; attempt++)
        {
            Assert.Equal("GAMEPLAY_INTERACTION_YIELDED", (await Assert.ThrowsAsync<ManagedServiceException>(() => new PersistenceTools(new(EPersistence())).CommitAsync(EWrite(id, transaction: "assistant-option-two"), default))).Code);
            var response = await EService().ResolveDecisionAsync(new("invented-choice", id, question.Interaction.Revision, decision.DecisionId, decision.Revision), default);
            Assert.False(response.Success); Assert.Equal(PlayerDecisionState.Pending, (await EStatus(id))!.PendingDecision!.State);
            await DPersistence().ReadRecordsAsync("campaign-a", [EScene], default); await new PersistenceCoordinator(EPersistence()).GetStatusAsync("campaign-a", default);
            await DService().BeginAsync(DRequest(setup.Entry.Interaction) with { ExpectedRevision = 1 }, default);
        }
        await EUnchangedAsync(before); Assert.Equal(1, await CountAsync("player_interactions"));
        var binding = await new CampaignBindingService(DBindingStore(), Options.Create(BindingOptions())).ChangeAsync(CampaignBindingAction.Switch,
            switchRequest, default);
        Assert.False(binding.Success); Assert.Equal("BINDING_SWITCH_DECISION_BLOCKED", binding.Code);
    }

    private async Task<PlayerInteractionStatus> EResponseAsync(CampaignSessionBinding binding, PlayerDecisionStatus decision,
        PlayerResponseDisposition? disposition, string input = "response")
    {
        var submission = PlayerInteractionContractTests.Submission(binding.BindingId, input) with { PendingDecisionId = decision.DecisionId,
            PendingDecisionRevision = decision.Revision, DecisionResponse = disposition is null ? null :
                new(disposition.Value, "protected:actual-response", "protected:choice-scope", disposition == PlayerResponseDisposition.Choice ? "protected:alternative-two" : null) };
        var service = new PlayerInteractionService(EStore(), Options.Create(BindingOptions()), Options.Create(PlayerInteractionContractTests.Enabled), new TestPlayerIngress(submission));
        var value = Interaction(await service.AcceptAsync("host-delivery", default));
        Assert.Equal(value.InteractionId, Interaction(await service.AcceptAsync("host-delivery", default)).InteractionId);
        return DSuccess(await DService().BeginAsync(new("entry-" + input, value.Revision, value.InteractionId), default)).Interaction;
    }

    [SqlImportTheory]
    [InlineData(PlayerResponseDisposition.Choice)][InlineData(PlayerResponseDisposition.Rejection)]
    [InlineData(PlayerResponseDisposition.Correction)][InlineData(PlayerResponseDisposition.ChangeDirection)]
    public async Task E_ActualTrustedResponseResolvesWithoutExecutingAFictionalOptionAndNeverReopensOrigin(PlayerResponseDisposition kind)
    {
        var setup = await ESetupAsync(); var question = await EQuestionAsync(setup.Entry.Interaction);
        var response = await EResponseAsync(setup.Binding, question.Interaction.PendingDecision!, kind);
        Assert.Equal("GAMEPLAY_DECISION_PENDING", (await Assert.ThrowsAsync<ManagedServiceException>(() => new PersistenceCoordinator(EPersistence()).CommitAsync(EWrite(response.InteractionId), default))).Code);
        var pending = response.PendingDecision!;
        var request = new GameplayDecisionRequest("resolve", response.InteractionId, response.Revision, pending.DecisionId, pending.Revision);
        var resolved = ESuccess(await EService().ResolveDecisionAsync(request, default));
        Assert.Equal(PlayerDecisionState.Resolved, resolved.Interaction.PendingDecision!.State); Assert.False(resolved.CompletedNarrationAuthorized);
        Assert.Equal(JsonSerializer.Serialize(resolved), JsonSerializer.Serialize(ESuccess(await EService().ResolveDecisionAsync(request, default))));
        var origin = (await EStatus(setup.Entry.Interaction.InteractionId))!;
        Assert.Equal(PlayerInteractionState.AWAITING_PLAYER_INPUT, origin.State); Assert.False(origin.Gameplay!.QuestionPresentationAuthorized);
        Assert.Equal(1, await ScalarAsync("SELECT active_version FROM [ec_fixture].campaigns WHERE campaign_id=N'campaign-a';"));
        Assert.Equal(PersistenceMarkers.Saved, (await new PersistenceCoordinator(EPersistence()).CommitAsync(EWrite(response.InteractionId), default)).Marker);
    }

    [SqlImportFact]
    public async Task E_TrustedClarificationYieldsWithoutChoiceAndNextActualReplyCanStillAnswer()
    {
        var setup = await ESetupAsync(); var question = await EQuestionAsync(setup.Entry.Interaction);
        var response = await EResponseAsync(setup.Binding, question.Interaction.PendingDecision!, PlayerResponseDisposition.Clarification);
        var pending = response.PendingDecision!;
        var clarified = ESuccess(await EService().ResolveDecisionAsync(new("clarify", response.InteractionId, response.Revision, pending.DecisionId, pending.Revision), default));
        Assert.True(clarified.Interaction.Yielded); Assert.True(clarified.QuestionPresentationAuthorized); Assert.Equal(PlayerDecisionState.Pending, clarified.Interaction.PendingDecision!.State);
        var later = await EResponseAsync(setup.Binding, clarified.Interaction.PendingDecision, PlayerResponseDisposition.Rejection, "later-answer");
        Assert.NotEqual(later.InteractionId, response.InteractionId);
        var decision = later.PendingDecision!;
        Assert.Equal(PlayerDecisionState.Resolved, ESuccess(await EService().ResolveDecisionAsync(new("reject", later.InteractionId, later.Revision, decision.DecisionId, decision.Revision), default)).Interaction.PendingDecision!.State);
        Assert.Equal(3, await CountAsync("player_interactions"));
    }

    [SqlImportFact]
    public async Task E_ActualInputWithoutTrustedInterpretationDoesNotGuessChoice()
    {
        var setup = await ESetupAsync(); var question = await EQuestionAsync(setup.Entry.Interaction);
        var response = await EResponseAsync(setup.Binding, question.Interaction.PendingDecision!, null);
        var decision = response.PendingDecision!;
        Assert.Equal("GAMEPLAY_TRUSTED_RESPONSE_REQUIRED", (await EService().ResolveDecisionAsync(new("resolve", response.InteractionId, response.Revision, decision.DecisionId, decision.Revision), default)).Code);
        Assert.Equal("PLAYER_DECISION_CONFLICT", (await EService().ResolveDecisionAsync(new("wrong-revision", response.InteractionId, response.Revision, decision.DecisionId, decision.Revision - 1), default)).Code);
        Assert.Equal(PlayerDecisionState.Pending, (await EStatus(response.InteractionId))!.PendingDecision!.State);
    }

    [SqlImportFact]
    public async Task E_DuplicateMutationsConvergeEvenWhenBothPreflightsRace()
    {
        var setup = await ESetupAsync(); var request = EWrite(setup.Entry.Interaction.InteractionId);
        var results = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => new PersistenceCoordinator(EPersistence()).CommitAsync(request, default)));
        Assert.All(results, result => Assert.Equal(PersistenceMarkers.Saved, result.Marker)); Assert.Single(results.Select(result => result.Receipt!.ReceiptId).Distinct());
        Assert.Equal(2, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions;"));
        Assert.Equal(2, await ScalarAsync("SELECT MAX(record_revision) FROM [ec_fixture].canonical_record_versions WHERE record_id=N'Scene';"));
    }

    [SqlImportTheory]
    [InlineData("yield")][InlineData("complete")][InlineData("cancel")][InlineData("suspend")][InlineData("switch")]
    public async Task E_AdmittedWriteSerializesAgainstYieldCancelSwitchAndSuspension(string competing)
    {
        var setup = await ESetupAsync(competing == "switch" ? 2 : 1); var id = setup.Entry.Interaction.InteractionId; var pause = new EPausedDurability();
        var switchRequest = competing == "switch" ? await ESwitchRequestAsync("competing") : null;
        var work = new PersistenceCoordinator(EPersistence(pause)).CommitAsync(EWrite(id), default);
        await pause.Reached.Task.WaitAsync(TimeSpan.FromSeconds(15)); var status = (await EStatus(id))!;
        if (competing is "yield" or "complete")
        {
            var proposal = EConclusion(status, affected: ["entry-fixture"]);
            if (competing == "yield") proposal = proposal with { QuestionReference = "protected:options", ResponseScopeReference = "protected:scope" };
            Assert.False((await EService().ConcludeAsync(proposal, default)).Success);
        }
        else if (competing == "cancel") await Assert.ThrowsAsync<ManagedServiceException>(() => DStore().CancelAsync(PlayerInteractionContractTests.Scope(), id, status.Revision, "cancel", null, null, default));
        else
        {
            var result = await new CampaignBindingService(DBindingStore(), Options.Create(BindingOptions())).ChangeAsync(competing == "switch" ? CampaignBindingAction.Switch : CampaignBindingAction.Suspend,
                switchRequest ?? new("competing", null, setup.Binding.BindingId, setup.Binding.Revision, true), default);
            if (competing == "switch") { Assert.False(result.Success); Assert.Equal("BINDING_SWITCH_BLOCKED", result.Code); } else Assert.True(result.Success, result.Code);
        }
        pause.Continue.TrySetResult();
        if (competing == "suspend")
        {
            Assert.Equal("INTERACTION_BINDING_UNAVAILABLE", (await Assert.ThrowsAsync<ManagedServiceException>(() => work)).Code);
            Assert.False((await EService().ConcludeAsync(EConclusion((await EStatus(id))!, affected: ["entry-fixture"]), default)).Success);
        }
        else Assert.Equal(PersistenceMarkers.Saved, (await work).Marker);
    }

    [SqlImportFact]
    public async Task E_QuestionVersusMutationAndCompletionVersusQuestionHaveOneSerializedWinner()
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId;
        async Task<bool> Mutate() { try { return (await new PersistenceCoordinator(EPersistence()).CommitAsync(EWrite(id), default)).Marker == PersistenceMarkers.Saved; } catch (ManagedServiceException) { return false; } }
        var question = EConclusion(setup.Entry.Interaction, "question") with { QuestionReference = "protected:options", ResponseScopeReference = "protected:scope" };
        var mutation = Mutate(); var yielded = EService().ConcludeAsync(question, default);
        var wrote = await mutation; var waiting = await yielded; Assert.NotEqual(wrote, waiting.Success);
        if (waiting.Success) Assert.Equal(PlayerInteractionState.AWAITING_PLAYER_INPUT, waiting.Data!.Interaction.State);
        else Assert.Equal(PlayerInteractionState.OPEN, (await EStatus(id))!.State);
    }

    [SqlImportFact]
    public async Task E_SeparateExplicitAdministrationWorksButCannotOverrideGameplaySession()
    {
        var setup = await ESetupAsync(); var request = EWrite("administrative-task");
        var admin = Options.Create(new GameplayAuthorityOptions { AdministrativeCampaignIds = ["campaign-a"] });
        var store = new SqlServerCampaignPersistenceStore(options, new ConfiguredCampaignSchemaResolver(options),
            new SqlServerDurabilityService(options, new ConfiguredCampaignSchemaResolver(options)), NullLogger<SqlServerCampaignPersistenceStore>.Instance,
            authorityOptions: admin);
        Assert.Equal(PersistenceMarkers.Saved, (await new PersistenceCoordinator(store).CommitAsync(request, default)).Marker);
        var combined = new SqlServerCampaignPersistenceStore(options, new ConfiguredCampaignSchemaResolver(options),
            new SqlServerDurabilityService(options, new ConfiguredCampaignSchemaResolver(options)), NullLogger<SqlServerCampaignPersistenceStore>.Instance,
            DBindingStore(), Options.Create(BindingOptions()), Options.Create(PlayerInteractionContractTests.Enabled), Options.Create(DOptions()), admin);
        await Assert.ThrowsAsync<ManagedServiceException>(() => new PersistenceCoordinator(combined).CommitAsync(EWrite("administrative-task", 2, 2, "admin-bypass"), default));
        Assert.Equal("GAMEPLAY_ENTRY_BASELINE_STALE", (await EService().ConcludeAsync(EConclusion(setup.Entry.Interaction), default)).Code);
    }

    [SqlImportFact]
    public async Task E_ActualMcpLegacyWriteWithoutEntryFailsAndEnteredWriteCompletionRecoverAcrossProcesses()
    {
        var setup = await DSetupAsync(); var id = setup.Interaction.InteractionId; var request = EWrite(id);
        var denied = await DProcessAsync(request, "ec_commit_changes");
        Assert.Contains("GAMEPLAY_ENTRY_REQUIRED", denied.GetRawText());
        await DReplacePlanAsync(DPlan() with { MutationScope = [new(EScene, GameplayMutationAction.Update)] });
        var entered = await DProcessAsync(DRequest(setup.Interaction)); Assert.True(entered.GetProperty("success").GetBoolean());
        var written = await DProcessAsync(request, "ec_commit_changes"); Assert.Equal(PersistenceMarkers.Saved, written.GetProperty("marker").GetString());
        var duplicate = await DProcessAsync(request, "ec_commit_changes"); Assert.Equal(written.GetProperty("receipt").GetRawText(), duplicate.GetProperty("receipt").GetRawText());
        var conclusion = EConclusion((await EStatus(id))!, affected: ["entry-fixture"]);
        var result = await DProcessAsync(conclusion, "ec_conclude_gameplay_interaction"); Assert.True(result.GetProperty("success").GetBoolean(), result.GetRawText());
        Assert.True(result.GetProperty("data").GetProperty("completedNarrationAuthorized").GetBoolean());
        var recovered = await DProcessAsync(conclusion, "ec_conclude_gameplay_interaction"); Assert.Equal(result.GetProperty("data").GetRawText(), recovered.GetProperty("data").GetRawText());
    }

    [SqlImportFact]
    public async Task E_ActualMcpDecisionCannotSelectFromAssistantContinuation()
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId;
        var request = EConclusion(setup.Entry.Interaction) with { QuestionReference = "protected:three-alternatives", ResponseScopeReference = "protected:choice-scope" };
        var pending = await DProcessAsync(request, "ec_conclude_gameplay_interaction"); Assert.True(pending.GetProperty("success").GetBoolean(), pending.GetRawText());
        var current = (await EStatus(id))!; var decision = current.PendingDecision!;
        var forged = await DProcessAsync(new GameplayDecisionRequest("invented-choice", id, current.Revision, decision.DecisionId, decision.Revision), "ec_resolve_player_decision");
        Assert.False(forged.GetProperty("success").GetBoolean()); Assert.Equal("GAMEPLAY_INTERACTION_YIELDED", forged.GetProperty("code").GetString());
        var write = await DProcessAsync(EWrite(id), "ec_commit_changes"); Assert.Contains("GAMEPLAY_INTERACTION_YIELDED", write.GetRawText());
        Assert.Equal(PlayerDecisionState.Pending, (await EStatus(id))!.PendingDecision!.State); Assert.Equal(1, await CountAsync("player_interactions"));
    }

    [SqlImportFact]
    public async Task E_ActualMcpUnknownOutcomeBlocksNarrationThenSameTransactionReconcilesAndCompletes()
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId; var pause = new EPausedDurability();
        using var cancel = new CancellationTokenSource(); var write = EWrite(id);
        var work = new PersistenceCoordinator(EPersistence(pause)).CommitAsync(write, cancel.Token);
        await pause.Reached.Task.WaitAsync(TimeSpan.FromSeconds(15)); cancel.Cancel(); await Assert.ThrowsAnyAsync<OperationCanceledException>(() => work);
        var blocked = await DProcessAsync(EConclusion((await EStatus(id))!, affected: ["entry-fixture"]), "ec_conclude_gameplay_interaction");
        Assert.False(blocked.GetProperty("success").GetBoolean());
        var reconciled = await DProcessAsync(new { interactionId = id, transactionId = write.TransactionId }, "ec_reconcile_gameplay_persistence", false);
        Assert.True(reconciled.GetProperty("success").GetBoolean(), reconciled.GetRawText());
        Assert.False(reconciled.GetProperty("data").GetProperty("turnMayComplete").GetBoolean());
        var completed = await DProcessAsync(EConclusion((await EStatus(id))!, affected: ["entry-fixture"]), "ec_conclude_gameplay_interaction");
        Assert.True(completed.GetProperty("success").GetBoolean(), completed.GetRawText());
        Assert.True(completed.GetProperty("data").GetProperty("completedNarrationAuthorized").GetBoolean());
        Assert.Equal(2, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].save_transactions;"));
    }

    [SqlImportTheory]
    [InlineData("suspend")][InlineData("version")][InlineData("owner")][InlineData("profile")]
    public async Task E_ActivationRechecksAuthorityAfterAdmissionAndNeverReportsAnInvalidCommit(string fault)
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId;
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var resume = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var work = new PersistenceCoordinator(EPersistence(beforeActivation: async token => { reached.TrySetResult(); await resume.Task.WaitAsync(token); })).CommitAsync(EWrite(id), default);
        await reached.Task.WaitAsync(TimeSpan.FromSeconds(15));
        switch (fault)
        {
            case "suspend": await new CampaignBindingService(DBindingStore(), Options.Create(BindingOptions())).ChangeAsync(CampaignBindingAction.Suspend,
                new("suspend", null, setup.Binding.BindingId, setup.Binding.Revision, true), default); break;
            // Disposable administrative fault injection, not a gameplay bypass.
            case "version": await ExecuteAsync("UPDATE [ec_fixture].campaigns SET active_version=3 WHERE campaign_id=N'campaign-a';"); break;
            case "owner": await ExecuteAsync("UPDATE [ec_fixture].canonical_record_versions SET record_revision=2 WHERE campaign_version=1 AND record_id=N'Scene';"); break;
            case "profile": await ExecuteAsync("UPDATE [import_domain].gm_host_configurations SET configuration_revision=configuration_revision+1;"); break;
        }
        resume.TrySetResult(); await Assert.ThrowsAsync<ManagedServiceException>(() => work);
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].persistence_receipts WHERE status=N'Validated';"));
        Assert.Equal(fault == "version" ? 3 : 1, await ScalarAsync("SELECT active_version FROM [ec_fixture].campaigns WHERE campaign_id=N'campaign-a';"));
        Assert.Equal(PlayerInteractionState.BLOCKED, (await EStatus(id))!.State);
        Assert.False((await EService().ConcludeAsync(EConclusion((await EStatus(id))!, affected: ["entry-fixture"]), default)).Success);
    }

    [SqlImportFact]
    public async Task E_CompetingCompletionAndQuestionCannotBothAuthorizeDelivery()
    {
        var setup = await ESetupAsync();
        var completion = EService().ConcludeAsync(EConclusion(setup.Entry.Interaction), default);
        var question = EService().ConcludeAsync(EConclusion(setup.Entry.Interaction, "question") with
            { QuestionReference = "protected:options", ResponseScopeReference = "protected:scope" }, default);
        var results = await Task.WhenAll(completion, question); Assert.Single(results, result => result.Success);
        var winner = results.Single(result => result.Success).Data!;
        Assert.True(winner.CompletedNarrationAuthorized ^ winner.QuestionPresentationAuthorized);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM [import_domain].player_interaction_receipts;"));
    }

    [SqlImportFact]
    public async Task E_CompetingDecisionResolutionsAndCampaignSwitchRemainScopedAndIdempotent()
    {
        var setup = await ESetupAsync(2); var switchRequest = await ESwitchRequestAsync("switch");
        var question = await EQuestionAsync(setup.Entry.Interaction);
        var response = await EResponseAsync(setup.Binding, question.Interaction.PendingDecision!, PlayerResponseDisposition.Choice);
        var pending = response.PendingDecision!; var request = new GameplayDecisionRequest("resolve", response.InteractionId, response.Revision, pending.DecisionId, pending.Revision);
        var switched = new CampaignBindingService(DBindingStore(), Options.Create(BindingOptions())).ChangeAsync(CampaignBindingAction.Switch,
            switchRequest, default);
        var resolutions = await Task.WhenAll(EService().ResolveDecisionAsync(request, default), EService().ResolveDecisionAsync(request with { RequestId = "competing-response" }, default));
        Assert.Single(resolutions, result => result.Success);
        Assert.Contains((await switched).Code, new[] { "BINDING_SWITCH_DECISION_BLOCKED", "BINDING_SWITCH_INTERACTION_BLOCKED" });
        Assert.Equal(PlayerDecisionState.Resolved, (await EStatus(response.InteractionId))!.PendingDecision!.State);
        Assert.Equal(2, await CountAsync("player_interactions"));
    }

    [SqlImportFact]
    public async Task E_AdditiveJsonEvidenceWorksOn015AndRepeatedMigrationsPreserveCompletedState()
    {
        var setup = await ESetupAsync(); var request = EConclusion(setup.Entry.Interaction); var result = ESuccess(await EService().ConcludeAsync(request, default));
        await CSchemaAsync(); await DSchemaAsync();
        var planner = new SqlServerSchemaBootstrapExecutor(options, new ConfiguredCampaignSchemaResolver(options),
            campaignBinding: Options.Create(BindingOptions()), playerInteraction: Options.Create(PlayerInteractionContractTests.Enabled), gameplayEntry: Options.Create(DOptions()));
        Assert.Empty((await planner.PlanAsync(null, default)).MigrationIds);
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(ESuccess(await EService().ConcludeAsync(request, default))));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM sys.tables WHERE name LIKE N'gameplay%';"));
    }

    [SqlImportFact]
    public async Task E_DefaultLegacyDeploymentHasNoImplicitAdministrativeCanonWriteGrant()
    {
        var setup = await ESetupAsync(); var records = await ScalarAsync("SELECT COUNT(*) FROM [ec_fixture].canonical_record_versions;");
        Assert.Equal("GAMEPLAY_ENTRY_REQUIRED", (await Assert.ThrowsAsync<ManagedServiceException>(() => new PersistenceCoordinator(DPersistence()).CommitAsync(EWrite(setup.Entry.Interaction.InteractionId), default))).Code);
        await EUnchangedAsync(records);
    }

    [SqlImportFact]
    public async Task E_AdditionalOwnerReadRefusesUnseenOrUnpermittedOwnerAndNeverUsesConversation()
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId;
        Assert.Equal("GAMEPLAY_MUTATION_SCOPE_DENIED", (await EService().ReadOwnersAsync(new(id, 3, [new("unseen", "guessed")]), default)).Code);
        var denied = EWrite(id, address: EInventory, tombstone: true) with { AffectedOwnerDomains = [EInventory.OwnerDomain] };
        Assert.Equal("GAMEPLAY_MUTATION_SCOPE_DENIED", (await Assert.ThrowsAsync<ManagedServiceException>(() => new PersistenceCoordinator(EPersistence()).CommitAsync(denied, default))).Code);
        Assert.Equal(3, (await EStatus(id))!.Revision);
    }

    [SqlImportFact]
    public async Task E_OnlyOwnValidatedWritesAdvanceTheAlreadyDeclaredScopeAndEveryReceiptIsRequired()
    {
        var setup = await ESetupAsync(); var id = setup.Entry.Interaction.InteractionId; var persistence = new PersistenceCoordinator(EPersistence());
        var first = await persistence.CommitAsync(EWrite(id), default);
        var second = await persistence.CommitAsync(EWrite(id, 2, 2, "effect-two"), default);
        Assert.Equal(PersistenceMarkers.Saved, first.Marker); Assert.Equal(PersistenceMarkers.Saved, second.Marker);
        var current = (await EStatus(id))!; Assert.Equal(3, current.Gameplay!.CampaignVersion); Assert.Equal(1, current.StartingCampaignVersion);
        var final = ESuccess(await EService().ConcludeAsync(EConclusion(current, affected: ["entry-fixture"]), default));
        Assert.Equal(new long[] { 2, 3 }, final.Receipts.Select(receipt => receipt.CampaignVersion));
        Assert.Equal(first.Receipt, (await persistence.CommitAsync(EWrite(id), default)).Receipt);
        await ExecuteAsync("DELETE FROM [ec_fixture].persistence_receipts WHERE transaction_id=N'effect-two';");
        var recovered = await EService().ConcludeAsync(EConclusion(current, affected: ["entry-fixture"]), default);
        Assert.False(recovered.Success); Assert.Equal("GAMEPLAY_COMPLETION_BLOCKED", recovered.Code);
    }
}
