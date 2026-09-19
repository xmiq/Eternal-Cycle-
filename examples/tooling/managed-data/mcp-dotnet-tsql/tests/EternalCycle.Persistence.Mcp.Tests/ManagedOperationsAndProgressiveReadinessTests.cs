using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class ManagedOperationsAndProgressiveReadinessTests
{
    [Fact]
    public async Task PublishInitiationReturnsDurableOperationAndIgnoresLaterClientCancellation()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        using var client = new CancellationTokenSource();

        var operation = await service.EnqueueInitialRulePublicationAsync(client.Token);
        client.Cancel();
        var persisted = await service.GetAsync(operation.OperationId, CancellationToken.None);

        Assert.Equal(ManagedOperationState.Queued, operation.State);
        Assert.Equal(operation.OperationId, persisted?.OperationId);
        Assert.Equal(ManagedOperationState.Queued, persisted?.State);
    }

    [Fact]
    public async Task EquivalentActivePublicationIsDeduplicatedAndDiscoverable()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);

        var first = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        var second = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        var recent = await service.ListRecentAsync(
            ManagedOperationKinds.InitialRulePublication,
            10,
            CancellationToken.None);

        Assert.Equal(first.OperationId, second.OperationId);
        Assert.Single(recent);
    }

    [Fact]
    public async Task RunningOperationExposesCurrentStageThenCompletedRelease()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var publisher = new BlockingPublisher();
        var processor = Processor(store, publisher);
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);

        var processing = processor.ProcessNextAsync(CancellationToken.None);
        await publisher.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var running = await service.GetAsync(queued.OperationId, CancellationToken.None);
        publisher.Complete();
        _ = await processing;
        var completed = await service.GetAsync(queued.OperationId, CancellationToken.None);

        Assert.Equal(ManagedOperationState.Running, running?.State);
        Assert.Equal(RulePublicationStage.Compile.ToString(), running?.CurrentStage);
        Assert.Equal(ManagedOperationState.Succeeded, completed?.State);
        Assert.Equal("RULE-RESULT", completed?.ResultRuleReleaseId);
        Assert.Equal("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", completed?.SourceIdentity);
    }

    [Fact]
    public async Task ActiveProcessorRenewsExecutionLeaseAndRemainsDeduplicated()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var publisher = new BlockingPublisher();
        var processor = Processor(
            store,
            publisher,
            executionLeaseDuration: TimeSpan.FromMilliseconds(200));
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);

        var processing = processor.ProcessNextAsync(CancellationToken.None);
        await publisher.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await Task.Delay(600);
        var running = await service.GetAsync(queued.OperationId, CancellationToken.None);
        var duplicate = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        publisher.Complete();
        _ = await processing;

        Assert.Equal(ManagedOperationState.Running, running?.State);
        Assert.Equal(queued.OperationId, duplicate.OperationId);
        Assert.Equal(queued.CorrelationId, duplicate.CorrelationId);
    }

    [Fact]
    public async Task PublicationFailurePreservesStructuredCausalStatus()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var processor = Processor(store, new FailedPublisher());
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);

        _ = await processor.ProcessNextAsync(CancellationToken.None);
        var failed = await service.GetAsync(queued.OperationId, CancellationToken.None);

        Assert.Equal(ManagedOperationState.Failed, failed?.State);
        Assert.Equal("RULE_SOURCE_OPERATION_TIMEOUT", failed?.ErrorCode);
        Assert.Equal(RulePublicationStage.AcquireSource.ToString(), failed?.CurrentStage);
        Assert.True(failed?.RetrySafe);
        Assert.False(failed?.AdministrativeInterventionRequired);
    }

    [Fact]
    public async Task DurableOperationPropagatesItsTrueOperationAndCorrelationIds()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var publisher = new ContextCapturingPublisher();
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);

        _ = await Processor(store, publisher).ProcessNextAsync(CancellationToken.None);

        Assert.Equal(queued.OperationId, publisher.Execution?.OperationId);
        Assert.Equal(queued.CorrelationId, publisher.Execution?.CorrelationId);
    }

    [Fact]
    public async Task HostShutdownLeavesDurableOperationInterruptedAndRecoverable()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var publisher = new BlockingPublisher();
        var diagnostics = new RecordingDiagnosticRecorder();
        var processor = Processor(store, publisher, diagnostics: diagnostics);
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        using var stopping = new CancellationTokenSource();

        var processing = processor.ProcessNextAsync(stopping.Token);
        await publisher.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        stopping.Cancel();
        _ = await processing;
        var result = await service.GetAsync(queued.OperationId, CancellationToken.None);

        Assert.Equal(ManagedOperationState.Interrupted, result?.State);
        Assert.Equal("MANAGED_OPERATION_INTERRUPTED", result?.ErrorCode);
        Assert.True(result?.RetrySafe);
        var diagnostic = Assert.Single(diagnostics.Events);
        Assert.Equal(queued.OperationId, diagnostic.OperationId);
        Assert.Equal(queued.CorrelationId, diagnostic.CorrelationId);
        Assert.Contains("CancellationSource=HostShutdown", diagnostic.SafeDetail);
    }

    [Fact]
    public async Task ManagedOperationTimeoutIsClassifiedAtDurableOwner()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var publisher = new BlockingPublisher();
        var diagnostics = new RecordingDiagnosticRecorder();
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);

        _ = await Processor(
            store,
            publisher,
            TimeSpan.FromMilliseconds(40),
            diagnostics).ProcessNextAsync(CancellationToken.None);
        var result = await service.GetAsync(queued.OperationId, CancellationToken.None);

        Assert.Equal(ManagedOperationState.Failed, result?.State);
        Assert.Equal("MANAGED_OPERATION_TIMEOUT", result?.ErrorCode);
        Assert.Contains("CancellationSource=ManagedOperationTimeout", Assert.Single(diagnostics.Events).SafeDetail);
    }

    [Fact]
    public async Task UnexpectedParentCancellationIsNotMisreportedAsShutdownOrTimeout()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var diagnostics = new RecordingDiagnosticRecorder();
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);

        _ = await Processor(
            store,
            new UnexpectedCancellationPublisher(),
            diagnostics: diagnostics).ProcessNextAsync(CancellationToken.None);
        var result = await service.GetAsync(queued.OperationId, CancellationToken.None);

        Assert.Equal(ManagedOperationState.Failed, result?.State);
        Assert.Equal("MANAGED_OPERATION_CANCELLED_UNEXPECTED", result?.ErrorCode);
        Assert.Contains("CancellationSource=UnexpectedParentCancellation", Assert.Single(diagnostics.Events).SafeDetail);
    }

    [Fact]
    public async Task OperationStatusEnvelopeMatchesInnerRetrySemantics()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        _ = await store.ClaimNextAsync(TimeSpan.FromMinutes(1), CancellationToken.None);
        await store.FailAsync(
            queued.OperationId,
            ManagedOperationState.Failed,
            "AcquireSource",
            "RULE_SOURCE_REF_NOT_FOUND",
            "The requested ref was not found.",
            retrySafe: false,
            administrativeInterventionRequired: true,
            CancellationToken.None);

        var result = await new ManagedOperationTools(service)
            .GetStatusAsync(queued.OperationId, CancellationToken.None);

        Assert.NotNull(result.Data);
        Assert.Equal(result.Data!.RetrySafe, result.RetrySafe);
        Assert.Equal(
            result.Data.AdministrativeInterventionRequired,
            result.AdministrativeInterventionRequired);
    }

    [Fact]
    public async Task RestartTurnsPhantomRunningWorkIntoRetryableInterruptedWork()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        _ = await store.ClaimNextAsync(TimeSpan.FromMinutes(1), CancellationToken.None);
        store.ExpireExecutionLease(queued.OperationId);

        var recovered = await store.RecoverInterruptedAsync(CancellationToken.None);
        var interrupted = await service.GetAsync(queued.OperationId, CancellationToken.None);
        var reclaimed = await store.ClaimNextAsync(TimeSpan.FromMinutes(1), CancellationToken.None);

        Assert.Equal(1, recovered);
        Assert.Equal(ManagedOperationState.Interrupted, interrupted?.State);
        Assert.True(interrupted?.RetrySafe);
        Assert.Equal(queued.OperationId, reclaimed?.OperationId);
        Assert.Equal(queued.CorrelationId, reclaimed?.CorrelationId);
        Assert.Equal(ManagedOperationState.Running, reclaimed?.State);
        Assert.Equal(2, reclaimed?.ExecutionAttempt);
    }

    [Fact]
    public async Task LiveExecutionLeasePreservesRunningDeduplication()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        var running = await store.ClaimNextAsync(TimeSpan.FromMinutes(1), CancellationToken.None);

        var recovered = await store.RecoverInterruptedAsync(CancellationToken.None);
        var duplicate = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);

        Assert.Equal(0, recovered);
        Assert.Equal(ManagedOperationState.Running, running?.State);
        Assert.Equal(queued.OperationId, duplicate.OperationId);
        Assert.Equal(queued.CorrelationId, duplicate.CorrelationId);
    }

    [Fact]
    public async Task ExpiredExecutionLeaseRejectsStaleStateMutation()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        _ = await store.ClaimNextAsync(TimeSpan.FromMinutes(1), CancellationToken.None);
        store.ExpireExecutionLease(queued.OperationId);

        await Assert.ThrowsAsync<ManagedOperationOwnershipLostException>(() =>
            store.CompleteAsync(
                queued.OperationId,
                RulePublicationStage.RecordUpdateCheck.ToString(),
                "A stale worker must not complete this operation.",
                null,
                null,
                CancellationToken.None));

        var recovered = await store.GetAsync(queued.OperationId, CancellationToken.None);

        Assert.Equal(ManagedOperationState.Interrupted, recovered?.State);
        Assert.Equal("MANAGED_OPERATION_INTERRUPTED", recovered?.ErrorCode);
    }

    [Fact]
    public async Task RepeatInitiationRecoversExpiredRunningPublicationWithoutChangingIdentity()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var original = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        _ = await store.ClaimNextAsync(TimeSpan.FromMinutes(1), CancellationToken.None);
        store.ExpireExecutionLease(original.OperationId);

        var recovered = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        var worker = new ManagedOperationWorker(
            store,
            Processor(store, new ImmediatePublisher()),
            Options.Create(new ManagedRuleServiceOptions
            {
                ManagedOperationPollInterval = TimeSpan.FromMilliseconds(5),
                ManagedOperationTimeout = TimeSpan.FromSeconds(10),
                ManagedOperationLeaseDuration = TimeSpan.FromMilliseconds(100)
            }),
            NullLogger<ManagedOperationWorker>.Instance);

        Assert.Equal(original.OperationId, recovered.OperationId);
        Assert.Equal(original.CorrelationId, recovered.CorrelationId);
        Assert.Equal(ManagedOperationState.Interrupted, recovered.State);

        await worker.StartAsync(CancellationToken.None);
        try
        {
            ManagedOperationStatus? completed = null;
            for (var attempt = 0; attempt < 100; attempt++)
            {
                completed = await service.GetAsync(original.OperationId, CancellationToken.None);
                if (completed?.State == ManagedOperationState.Succeeded)
                {
                    break;
                }

                await Task.Delay(10);
            }

            Assert.Equal(ManagedOperationState.Succeeded, completed?.State);
            Assert.Equal(2, completed?.ExecutionAttempt);
            Assert.Single(await service.ListRecentAsync(
                ManagedOperationKinds.InitialRulePublication,
                10,
                CancellationToken.None));
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
            worker.Dispose();
        }
    }

    [Fact]
    public async Task HostedWorkerRecoversPersistedRunningWorkAndCompletesIt()
    {
        var store = new MemoryManagedOperationStore();
        var service = OperationService(store);
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        _ = await store.ClaimNextAsync(TimeSpan.FromMinutes(1), CancellationToken.None);
        store.ExpireExecutionLease(queued.OperationId);
        var worker = new ManagedOperationWorker(
            store,
            Processor(store, new ImmediatePublisher()),
            Options.Create(new ManagedRuleServiceOptions
            {
                ManagedOperationPollInterval = TimeSpan.FromMilliseconds(5),
                ManagedOperationTimeout = TimeSpan.FromSeconds(10),
                ManagedOperationLeaseDuration = TimeSpan.FromMilliseconds(100)
            }),
            NullLogger<ManagedOperationWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        try
        {
            ManagedOperationStatus? completed = null;
            for (var attempt = 0; attempt < 100; attempt++)
            {
                completed = await service.GetAsync(queued.OperationId, CancellationToken.None);
                if (completed?.State == ManagedOperationState.Succeeded)
                {
                    break;
                }

                await Task.Delay(10);
            }

            Assert.Equal(ManagedOperationState.Succeeded, completed?.State);
            Assert.Equal("RULE-RESULT", completed?.ResultRuleReleaseId);
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
            worker.Dispose();
        }
    }

    [Fact]
    public async Task HostedWorkerWaitsForDurableStoreSetupThenRecoversAndCompletes()
    {
        var inner = new MemoryManagedOperationStore();
        var service = OperationService(inner);
        var queued = await service.EnqueueInitialRulePublicationAsync(CancellationToken.None);
        var store = new InitiallyUnavailableOperationStore(inner);
        var worker = new ManagedOperationWorker(
            store,
            Processor(store, new ImmediatePublisher()),
            Options.Create(new ManagedRuleServiceOptions
            {
                ManagedOperationPollInterval = TimeSpan.FromMilliseconds(5),
                ManagedOperationTimeout = TimeSpan.FromSeconds(10)
            }),
            NullLogger<ManagedOperationWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        try
        {
            ManagedOperationStatus? completed = null;
            for (var attempt = 0; attempt < 100; attempt++)
            {
                completed = await service.GetAsync(queued.OperationId, CancellationToken.None);
                if (completed?.State == ManagedOperationState.Succeeded)
                {
                    break;
                }

                await Task.Delay(10);
            }

            Assert.True(store.RecoveryAttempts >= 2);
            Assert.Equal(ManagedOperationState.Succeeded, completed?.State);
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
            worker.Dispose();
        }
    }

    [Fact]
    public async Task ServiceDiagnosticsDoNotRequireCampaignAndCampaignCanEnrichThem()
    {
        var readiness = Ready(fullRulesetReady: false);
        var diagnostics = new ServiceDiagnostics(Resolver(), new StaticReadiness(readiness));

        var service = await diagnostics.GetReportAsync(null, CancellationToken.None);
        var campaign = await diagnostics.GetReportAsync("campaign-a", CancellationToken.None);

        Assert.Null(service.CampaignId);
        Assert.Null(service.WorldModelId);
        Assert.Equal("campaign-a", campaign.CampaignId);
        Assert.NotNull(campaign.WorldModelId);
    }

    [Fact]
    public void GameplayCanBeReadyBeforeFullRuleset()
    {
        var report = Ready(fullRulesetReady: false);

        Assert.True(report.ServiceReady);
        Assert.True(report.PersistenceReady);
        Assert.True(report.RuleKernelReady);
        Assert.True(report.CampaignBootstrapReady);
        Assert.True(report.GameplayReady);
        Assert.False(report.FullRulesetReady);
    }

    [Fact]
    public void KernelOrBootstrapGapBlocksGameplayWithoutClaimingAdministrativeFailure()
    {
        var snapshot = Snapshot(
            ruleKernelReady: true,
            campaignBootstrapReady: false,
            fullRulesetReady: false);

        var report = ManagedReadinessEvaluator.Evaluate(snapshot, campaignRequested: false);

        Assert.Equal(ManagedReadinessState.RulePreparationPending, report.State);
        Assert.Equal("RULE_CLOSURE_PENDING", report.ErrorCode);
        Assert.False(report.GameplayReady);
        Assert.False(report.AdministrativeActionRequired);
    }

    [Fact]
    public async Task UnavailableRequestedClosureBecomesPendingAndRaisesPriorityThenSucceeds()
    {
        var release = Release();
        var rules = new SingleReleaseStore(release);
        var preparation = new MemoryPreparationStore(release, ready: ["kernel"]);
        var provider = new PublishedRuleContextProvider(
            rules,
            preparation,
            Options.Create(new ManagedRuleServiceOptions()),
            Resolver());
        var request = new RuleContextRequest(
            "campaign-a",
            "gameplay.resolve",
            ["exploration"],
            "NORMAL");

        var pending = await Assert.ThrowsAsync<RuleContextPendingException>(() =>
            provider.GetContextAsync(request, CancellationToken.None));
        Assert.Contains("exploration", pending.PendingRuleSourceIds);
        Assert.Contains("exploration", preparation.PriorityRaised);

        await preparation.MarkReadyAsync(
            release.RuleReleaseId,
            ["exploration", "authority"],
            CancellationToken.None);
        var ready = await provider.GetContextAsync(request, CancellationToken.None);

        Assert.Contains(ready.Chunks, chunk => chunk.RuleSourceId == "kernel");
        Assert.Contains(ready.Chunks, chunk => chunk.RuleSourceId == "authority");
        Assert.Contains(ready.Chunks, chunk => chunk.RuleSourceId == "exploration");
    }

    [Fact]
    public void PrereleaseDisplayVersionIsDeterministicButShaRemainsIdentity()
    {
        var first = PrereleaseVersioning.Derive(
            "v1.0.0",
            "v1.1.0-rc",
            "1.1.0",
            47,
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        var sameCountDifferentCommit = PrereleaseVersioning.Derive(
            "v1.0.0",
            "v1.1.0-rc",
            "1.1.0",
            47,
            "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        Assert.Equal("1.1.0-rc.47", first.DisplayVersion);
        Assert.Equal(first.DisplayVersion, sameCountDifferentCommit.DisplayVersion);
        Assert.NotEqual(first.SourceCommit, sameCountDifferentCommit.SourceCommit);
        Assert.Equal("v1.0.0", first.BaseRelease);
        Assert.Equal("v1.1.0-rc", first.DiscoveryTag);
    }

    [Fact]
    public void CampaignRelevantPreparationPrecedesOptionalRareRegardlessOfFileOrder()
    {
        var release = Release();
        var ordered = release.Index.Chunks
            .GroupBy(chunk => chunk.RuleSourceId)
            .Select(group => new { Id = group.Key, Tier = group.First().Metadata.PreparationTier })
            .OrderBy(value => value.Tier)
            .Select(value => value.Id)
            .ToArray();

        Assert.True(Array.IndexOf(ordered, "exploration") < Array.IndexOf(ordered, "rare-module"));
    }

    [Fact]
    public async Task DynamicPriorityBoostChangesTheNextPendingRuleSource()
    {
        var release = Release();
        var preparation = new MemoryPreparationStore(
            release,
            ready: ["kernel", "authority", "exploration"]);

        await preparation.RaisePriorityAsync(
            release.RuleReleaseId,
            ["rare-module"],
            CancellationToken.None);
        var next = await preparation.GetNextPendingAsync(
            release.RuleReleaseId,
            CancellationToken.None);

        Assert.Equal("rare-module", next?.RuleSourceId);
        Assert.True(next?.PriorityBoost > 0);
    }

    private static ManagedOperationService OperationService(MemoryManagedOperationStore store) =>
        new(
            store,
            new StaticSourceConfigurationStore(),
            Options.Create(new ManagedRuleServiceOptions()));

    private static ManagedOperationProcessor Processor(
        IManagedOperationStore store,
        IRulePublicationExecutor publisher,
        TimeSpan? operationTimeout = null,
        IManagedDiagnosticRecorder? diagnostics = null,
        TimeSpan? executionLeaseDuration = null) =>
        new(
            store,
            publisher,
            Options.Create(new ManagedRuleServiceOptions
            {
                ManagedOperationTimeout = operationTimeout ?? TimeSpan.FromSeconds(10),
                ManagedOperationLeaseDuration = executionLeaseDuration ?? TimeSpan.FromSeconds(30)
            }),
            NullLogger<ManagedOperationProcessor>.Instance,
            diagnostics);

    private static ManagedReadinessReport Ready(bool fullRulesetReady) =>
        ManagedReadinessEvaluator.Evaluate(
            Snapshot(true, true, fullRulesetReady),
            campaignRequested: false);

    private static ManagedInfrastructureSnapshot Snapshot(
        bool ruleKernelReady,
        bool campaignBootstrapReady,
        bool fullRulesetReady) =>
        new(
            ManagedComponentStatus.Ready,
            ManagedComponentStatus.Ready,
            ManagedComponentStatus.Ready,
            ManagedComponentStatus.Ready,
            1,
            "RULE-ACTIVE",
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            true,
            ManagedComponentStatus.NotRequested,
            "Activated",
            false,
            RuleKernelReady: ruleKernelReady,
            CampaignBootstrapReady: campaignBootstrapReady,
            FullRulesetReady: fullRulesetReady);

    private static ConfiguredCampaignSchemaResolver Resolver() =>
        new(Options.Create(new SqlServerPersistenceOptions()));

    private static PublishedRuleRelease Release()
    {
        var documents = new[]
        {
            Document("rare-module", RuleLayer.OptionalModule, RulePreparationTier.OptionalRare, ["rare"], []),
            Document("exploration", RuleLayer.Core, RulePreparationTier.CampaignRelevant, ["exploration"], ["authority"]),
            Document("authority", RuleLayer.Core, RulePreparationTier.CampaignBootstrap, ["authority"], []),
            Document("kernel", RuleLayer.RuntimeKernel, RulePreparationTier.RuntimeKernel, [], [], alwaysInclude: true)
        };
        var index = RuleCompiler.Compile("1.0.0", documents);
        return new PublishedRuleRelease(
            "RULE-ACTIVE",
            "eternal-cycle-core",
            "Git",
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            "1.0.0",
            "1",
            RuleReleaseState.Active,
            index,
            DateTimeOffset.UnixEpoch);
    }

    private static RuleSourceDocument Document(
        string id,
        RuleLayer layer,
        RulePreparationTier tier,
        IReadOnlyList<string> topics,
        IReadOnlyList<string> dependencies,
        bool alwaysInclude = false) =>
        new(
            id,
            $"fixtures/{id}.md",
            $"# {id}\n\n{id} rules.",
            new RuleSourceMetadata(
                layer,
                [],
                layer == RuleLayer.OptionalModule ? ["rare"] : [],
                ["NORMAL"],
                ["gameplay.resolve"],
                topics,
                0,
                alwaysInclude,
                dependencies,
                tier));

    private sealed class StaticReadiness(ManagedReadinessReport report) : IManagedReadinessService
    {
        public Task<ManagedReadinessReport> GetReadinessAsync(
            string? campaignId,
            CancellationToken cancellationToken) => Task.FromResult(report);
    }

    private sealed class StaticSourceConfigurationStore : IRuleSourceConfigurationStore
    {
        private readonly RuleSourceConfiguration configuration = new(
            "eternal-cycle-core",
            "Git",
            "https://example.invalid/rules.git",
            "refs/tags/v1.1.0-rc",
            "docs/rules/rule-source-manifest.json",
            true,
            7,
            DateTimeOffset.UnixEpoch,
            RuleSourceReleaseChannel.Prerelease);

        public Task<RuleSourceConfiguration?> GetAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult<RuleSourceConfiguration?>(configuration);

        public Task<RuleSourceConfiguration> SaveAsync(
            RuleSourceConfiguration value,
            CancellationToken cancellationToken) => Task.FromResult(value);
    }

    private sealed class BlockingPublisher : IRulePublicationExecutor
    {
        private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<RulePublicationResult> ExecuteAsync(CancellationToken cancellationToken)
        {
            return await ExecuteAsync(null, cancellationToken);
        }

        public async Task<RulePublicationResult> ExecuteAsync(
            Action<RulePublicationStage>? onStage,
            CancellationToken cancellationToken)
        {
            onStage?.Invoke(RulePublicationStage.Compile);
            Started.SetResult();
            await release.Task.WaitAsync(cancellationToken);
            return Success();
        }

        public void Complete() => release.SetResult();
    }

    private sealed class ImmediatePublisher : IRulePublicationExecutor
    {
        public Task<RulePublicationResult> ExecuteAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Success());

        public Task<RulePublicationResult> ExecuteAsync(
            Action<RulePublicationStage>? onStage,
            CancellationToken cancellationToken)
        {
            onStage?.Invoke(RulePublicationStage.PrepareMinimumClosure);
            onStage?.Invoke(RulePublicationStage.Activate);
            return Task.FromResult(Success());
        }
    }

    private sealed class ContextCapturingPublisher : IRulePublicationExecutor
    {
        public RulePublicationExecutionContext? Execution { get; private set; }

        public Task<RulePublicationResult> ExecuteAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Success());

        public Task<RulePublicationResult> ExecuteAsync(
            RulePublicationExecutionContext execution,
            Action<RulePublicationStage>? onStage,
            CancellationToken cancellationToken)
        {
            Execution = execution;
            return Task.FromResult(Success());
        }
    }

    private sealed class UnexpectedCancellationPublisher : IRulePublicationExecutor
    {
        public Task<RulePublicationResult> ExecuteAsync(CancellationToken cancellationToken) =>
            throw new OperationCanceledException("fixture parent cancellation");
    }

    private sealed class RecordingDiagnosticRecorder : IManagedDiagnosticRecorder
    {
        public List<ManagedDiagnosticContext> Events { get; } = [];

        public Task<ManagedDiagnosticReceipt> RecordFailureAsync(
            ManagedDiagnosticContext context,
            Exception exception,
            CancellationToken cancellationToken) =>
            RecordEventAsync(context, cancellationToken);

        public Task<ManagedDiagnosticReceipt> RecordEventAsync(
            ManagedDiagnosticContext context,
            CancellationToken cancellationToken)
        {
            Events.Add(context);
            return Task.FromResult(new ManagedDiagnosticReceipt(
                context.CorrelationId,
                "Captured",
                true,
                false));
        }
    }

    private sealed class FailedPublisher : IRulePublicationExecutor
    {
        public Task<RulePublicationResult> ExecuteAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new RulePublicationResult(
                "CandidateFailed",
                null,
                null,
                null,
                false,
                "Rule Source acquisition exceeded its Managed Service timeout.",
                "RULE_SOURCE_OPERATION_TIMEOUT",
                ManagedOperationKinds.InitialRulePublication,
                RulePublicationStage.AcquireSource.ToString(),
                "CORR-FAIL",
                RetrySafe: true));
    }

    private static RulePublicationResult Success() =>
        new(
            "Activated",
            "RULE-RESULT",
            "RULE-RESULT",
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            false,
            null,
            Operation: ManagedOperationKinds.InitialRulePublication,
            Stage: RulePublicationStage.RecordUpdateCheck.ToString());

    private sealed class SingleReleaseStore(PublishedRuleRelease release) : IPublishedRuleStore
    {
        public Task<PublishedRuleRelease?> GetActiveAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult<PublishedRuleRelease?>(release);

        public Task<PublishedRuleRelease?> FindBySourceAsync(string rulesetId, string sourceIdentity, CancellationToken cancellationToken) =>
            Task.FromResult<PublishedRuleRelease?>(release);

        public Task<PublishedRuleRelease?> GetByIdAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken) =>
            Task.FromResult<PublishedRuleRelease?>(release);

        public Task<RuleUpdateCheck?> GetLatestUpdateCheckAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult<RuleUpdateCheck?>(null);

        public Task StageCandidateAsync(PublishedRuleRelease value, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task SetStateAsync(string ruleReleaseId, RuleReleaseState state, string? failureReason, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task ActivateAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task RecordUpdateCheckAsync(RuleUpdateCheck updateCheck, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

internal sealed class InitiallyUnavailableOperationStore(IManagedOperationStore inner)
    : IManagedOperationStore
{
    private int recoveryAttempts;

    public int RecoveryAttempts => recoveryAttempts;

    public Task<ManagedOperationStatus> EnqueueOrReuseAsync(
        ManagedOperationEnqueueRequest request,
        CancellationToken cancellationToken) =>
        inner.EnqueueOrReuseAsync(request, cancellationToken);

    public Task<ManagedOperationStatus?> GetAsync(
        string operationId,
        CancellationToken cancellationToken) =>
        inner.GetAsync(operationId, cancellationToken);

    public Task<IReadOnlyList<ManagedOperationStatus>> ListRecentAsync(
        string? operationKind,
        int maximumCount,
        CancellationToken cancellationToken) =>
        inner.ListRecentAsync(operationKind, maximumCount, cancellationToken);

    public Task<int> RecoverInterruptedAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Increment(ref recoveryAttempts) == 1)
        {
            throw new InvalidOperationException("The durable operation store is not initialized yet.");
        }

        return inner.RecoverInterruptedAsync(cancellationToken);
    }

    public Task<ManagedOperationStatus?> ClaimNextAsync(
        TimeSpan executionLeaseDuration,
        CancellationToken cancellationToken) =>
        inner.ClaimNextAsync(executionLeaseDuration, cancellationToken);

    public Task<bool> RenewExecutionLeaseAsync(
        string operationId,
        TimeSpan executionLeaseDuration,
        CancellationToken cancellationToken) =>
        inner.RenewExecutionLeaseAsync(operationId, executionLeaseDuration, cancellationToken);

    public Task UpdateStageAsync(
        string operationId,
        string stage,
        int? progressPercent,
        string safeStatusDetail,
        CancellationToken cancellationToken) =>
        inner.UpdateStageAsync(operationId, stage, progressPercent, safeStatusDetail, cancellationToken);

    public Task CompleteAsync(
        string operationId,
        string stage,
        string safeStatusDetail,
        string? sourceIdentity,
        string? resultRuleReleaseId,
        CancellationToken cancellationToken) =>
        inner.CompleteAsync(
            operationId,
            stage,
            safeStatusDetail,
            sourceIdentity,
            resultRuleReleaseId,
            cancellationToken);

    public Task FailAsync(
        string operationId,
        ManagedOperationState state,
        string stage,
        string errorCode,
        string safeStatusDetail,
        bool retrySafe,
        bool administrativeInterventionRequired,
        CancellationToken cancellationToken) =>
        inner.FailAsync(
            operationId,
            state,
            stage,
            errorCode,
            safeStatusDetail,
            retrySafe,
            administrativeInterventionRequired,
            cancellationToken);
}

internal sealed class MemoryManagedOperationStore : IManagedOperationStore
{
    private readonly object sync = new();
    private readonly string executionOwnerId = $"WORKER-{Guid.NewGuid():N}";
    private readonly Dictionary<string, Entry> operations = new(StringComparer.Ordinal);

    private sealed record Entry(
        ManagedOperationStatus Status,
        string DeduplicationKey,
        string? ExecutionOwnerId = null,
        DateTimeOffset? ExecutionLeaseExpiresAt = null);

    public Task<ManagedOperationStatus> EnqueueOrReuseAsync(
        ManagedOperationEnqueueRequest request,
        CancellationToken cancellationToken)
    {
        lock (sync)
        {
            _ = RecoverExpiredUnsafe();
            var existing = operations.Values.FirstOrDefault(value =>
                value.DeduplicationKey == request.DeduplicationKey &&
                value.Status.State is ManagedOperationState.Queued or ManagedOperationState.Running or
                    ManagedOperationState.Cancelling or ManagedOperationState.Interrupted);
            if (existing is not null)
            {
                return Task.FromResult(existing.Status);
            }

            var now = DateTimeOffset.UtcNow;
            var status = new ManagedOperationStatus(
                $"OP-{Guid.NewGuid():N}",
                request.OperationKind,
                $"CORR-{Guid.NewGuid():N}",
                ManagedOperationState.Queued,
                "Queued",
                now,
                null,
                now,
                null,
                0,
                request.SafeStatusDetail,
                null,
                true,
                false,
                false,
                request.RulesetId,
                null,
                null,
                0);
            operations[status.OperationId] = new(status, request.DeduplicationKey);
            return Task.FromResult(status);
        }
    }

    public Task<ManagedOperationStatus?> GetAsync(string operationId, CancellationToken cancellationToken)
    {
        lock (sync)
        {
            _ = RecoverExpiredUnsafe();
            return Task.FromResult<ManagedOperationStatus?>(
                operations.TryGetValue(operationId, out var entry) ? entry.Status : null);
        }
    }

    public Task<IReadOnlyList<ManagedOperationStatus>> ListRecentAsync(
        string? operationKind,
        int maximumCount,
        CancellationToken cancellationToken)
    {
        lock (sync)
        {
            _ = RecoverExpiredUnsafe();
            return Task.FromResult<IReadOnlyList<ManagedOperationStatus>>(operations.Values
                .Select(value => value.Status)
                .Where(value => operationKind is null || value.OperationKind == operationKind)
                .OrderByDescending(value => value.UpdatedAt)
                .Take(maximumCount)
                .ToArray());
        }
    }

    public Task<int> RecoverInterruptedAsync(CancellationToken cancellationToken)
    {
        lock (sync)
        {
            return Task.FromResult(RecoverExpiredUnsafe());
        }
    }

    public Task<ManagedOperationStatus?> ClaimNextAsync(
        TimeSpan executionLeaseDuration,
        CancellationToken cancellationToken)
    {
        lock (sync)
        {
            var pair = operations.Values
                .Where(value => value.Status.State is ManagedOperationState.Queued or ManagedOperationState.Interrupted)
                .OrderBy(value => value.Status.CreatedAt)
                .FirstOrDefault();
            if (pair is null)
            {
                return Task.FromResult<ManagedOperationStatus?>(null);
            }

            var claimed = pair.Status with
            {
                State = ManagedOperationState.Running,
                CurrentStage = RulePublicationStage.AcquireSource.ToString(),
                StartedAt = pair.Status.StartedAt ?? DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                ProgressPercent = 1,
                ErrorCode = null,
                ExecutionAttempt = pair.Status.ExecutionAttempt + 1
            };
            operations[claimed.OperationId] = pair with
            {
                Status = claimed,
                ExecutionOwnerId = executionOwnerId,
                ExecutionLeaseExpiresAt = DateTimeOffset.UtcNow.Add(executionLeaseDuration)
            };
            return Task.FromResult<ManagedOperationStatus?>(claimed);
        }
    }

    public Task<bool> RenewExecutionLeaseAsync(
        string operationId,
        TimeSpan executionLeaseDuration,
        CancellationToken cancellationToken)
    {
        lock (sync)
        {
            var entry = operations[operationId];
            if (entry.Status.State is not (ManagedOperationState.Running or ManagedOperationState.Cancelling) ||
                entry.ExecutionOwnerId != executionOwnerId ||
                entry.ExecutionLeaseExpiresAt <= DateTimeOffset.UtcNow)
            {
                return Task.FromResult(false);
            }

            operations[operationId] = entry with
            {
                Status = entry.Status with { UpdatedAt = DateTimeOffset.UtcNow },
                ExecutionLeaseExpiresAt = DateTimeOffset.UtcNow.Add(executionLeaseDuration)
            };
            return Task.FromResult(true);
        }
    }

    public Task UpdateStageAsync(string operationId, string stage, int? progressPercent, string safeStatusDetail, CancellationToken cancellationToken) =>
        Mutate(operationId, status => status with
        {
            State = ManagedOperationState.Running,
            CurrentStage = stage,
            ProgressPercent = progressPercent ?? status.ProgressPercent,
            SafeStatusDetail = safeStatusDetail,
            UpdatedAt = DateTimeOffset.UtcNow
        });

    public Task CompleteAsync(string operationId, string stage, string safeStatusDetail, string? sourceIdentity, string? resultRuleReleaseId, CancellationToken cancellationToken) =>
        Mutate(operationId, status => status with
        {
            State = ManagedOperationState.Succeeded,
            CurrentStage = stage,
            ProgressPercent = 100,
            SafeStatusDetail = safeStatusDetail,
            SourceIdentity = sourceIdentity,
            ResultRuleReleaseId = resultRuleReleaseId,
            UpdatedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow,
            ErrorCode = null,
            RetrySafe = false
        });

    public Task FailAsync(string operationId, ManagedOperationState state, string stage, string errorCode, string safeStatusDetail, bool retrySafe, bool administrativeInterventionRequired, CancellationToken cancellationToken) =>
        Mutate(operationId, status => status with
        {
            State = state,
            CurrentStage = stage,
            SafeStatusDetail = safeStatusDetail,
            ErrorCode = errorCode,
            RetrySafe = retrySafe,
            AdministrativeInterventionRequired = administrativeInterventionRequired,
            UpdatedAt = DateTimeOffset.UtcNow,
            CompletedAt = state is ManagedOperationState.Failed or ManagedOperationState.Cancelled
                ? DateTimeOffset.UtcNow
                : null
        });

    private Task Mutate(string operationId, Func<ManagedOperationStatus, ManagedOperationStatus> mutate)
    {
        lock (sync)
        {
            var entry = operations[operationId];
            if (entry.ExecutionOwnerId != executionOwnerId ||
                entry.ExecutionLeaseExpiresAt <= DateTimeOffset.UtcNow)
            {
                throw new ManagedOperationOwnershipLostException(operationId);
            }

            var status = mutate(entry.Status);
            var releaseOwnership = status.State is not (ManagedOperationState.Running or ManagedOperationState.Cancelling);
            operations[operationId] = entry with
            {
                Status = status,
                ExecutionOwnerId = releaseOwnership ? null : entry.ExecutionOwnerId,
                ExecutionLeaseExpiresAt = releaseOwnership ? null : entry.ExecutionLeaseExpiresAt
            };
            return Task.CompletedTask;
        }
    }

    public void ExpireExecutionLease(string operationId)
    {
        lock (sync)
        {
            var entry = operations[operationId];
            operations[operationId] = entry with
            {
                ExecutionLeaseExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1)
            };
        }
    }

    private int RecoverExpiredUnsafe()
    {
        var count = 0;
        foreach (var id in operations.Keys.ToArray())
        {
            var entry = operations[id];
            if (entry.Status.State is not (ManagedOperationState.Running or ManagedOperationState.Cancelling) ||
                (entry.ExecutionOwnerId is not null &&
                 entry.ExecutionLeaseExpiresAt is not null &&
                 entry.ExecutionLeaseExpiresAt > DateTimeOffset.UtcNow))
            {
                continue;
            }

            operations[id] = entry with
            {
                Status = entry.Status with
                {
                    State = ManagedOperationState.Interrupted,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    ErrorCode = "MANAGED_OPERATION_INTERRUPTED",
                    RetrySafe = true
                },
                ExecutionOwnerId = null,
                ExecutionLeaseExpiresAt = null
            };
            count++;
        }

        return count;
    }
}

internal sealed class MemoryPreparationStore : IRulePreparationStore
{
    private readonly Dictionary<string, RuleSourcePreparation> sources;
    public HashSet<string> PriorityRaised { get; } = new(StringComparer.Ordinal);
    public List<string> PreparedOrder { get; } = [];

    public MemoryPreparationStore(PublishedRuleRelease release, IReadOnlyCollection<string> ready)
    {
        sources = release.Index.Chunks
            .GroupBy(chunk => chunk.RuleSourceId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => new RuleSourcePreparation(
                    group.Key,
                    group.Min(chunk => chunk.Metadata.PreparationTier),
                    ready.Contains(group.Key) ? RulePreparationState.Ready : RulePreparationState.Pending,
                    0,
                    BasePriority: group.Max(chunk => chunk.Metadata.Priority)),
                StringComparer.Ordinal);
    }

    public Task InitializeAsync(PublishedRuleRelease release, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<RuleReleasePreparation> GetAsync(string ruleReleaseId, CancellationToken cancellationToken) =>
        Task.FromResult(new RuleReleasePreparation(ruleReleaseId, sources.Values.ToArray()));

    public Task<RuleSourcePreparation?> GetNextPendingAsync(
        string ruleReleaseId,
        CancellationToken cancellationToken) =>
        Task.FromResult<RuleSourcePreparation?>(sources.Values
            .Where(source => source.State == RulePreparationState.Pending)
            .OrderByDescending(source => source.PriorityBoost)
            .ThenBy(source => source.Tier)
            .ThenByDescending(source => source.BasePriority)
            .ThenBy(source => source.RuleSourceId, StringComparer.Ordinal)
            .FirstOrDefault());

    public Task MarkReadyAsync(string ruleReleaseId, IReadOnlyCollection<string> ruleSourceIds, CancellationToken cancellationToken)
    {
        foreach (var id in ruleSourceIds)
        {
            sources[id] = sources[id] with { State = RulePreparationState.Ready };
            PreparedOrder.Add(id);
        }

        return Task.CompletedTask;
    }

    public Task RaisePriorityAsync(string ruleReleaseId, IReadOnlyCollection<string> ruleSourceIds, CancellationToken cancellationToken)
    {
        foreach (var id in ruleSourceIds)
        {
            PriorityRaised.Add(id);
            sources[id] = sources[id] with { PriorityBoost = sources[id].PriorityBoost + 1000 };
        }

        return Task.CompletedTask;
    }
}
