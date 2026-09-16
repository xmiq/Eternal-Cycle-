using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class PersistenceCoordinatorTests
{
    [Fact]
    public async Task CommitIsIdempotentAndReturnsOneValidatedReceipt()
    {
        var store = new FakeStore();
        var coordinator = new PersistenceCoordinator(store);
        var request = NewRequest();

        var first = await coordinator.CommitAsync(request, CancellationToken.None);
        var second = await coordinator.CommitAsync(request, CancellationToken.None);

        Assert.Equal(PersistenceMarkers.Saved, first.Marker);
        Assert.Equal(PersistenceMarkers.Saved, second.Marker);
        Assert.True(first.TurnMayComplete);
        Assert.NotNull(first.Receipt);
        Assert.Equal(1, store.AppliedTransactions);
        Assert.Equal(first.Receipt, second.Receipt);
    }

    [Fact]
    public async Task SavedMarkerWithoutReceiptIsRejected()
    {
        var store = new FakeStore
        {
            ForcedResult = new CommitResult(
                PersistenceMarkers.Saved,
                "Completed",
                true,
                2,
                null,
                null)
        };
        var coordinator = new PersistenceCoordinator(store);

        var result = await coordinator.CommitAsync(NewRequest(), CancellationToken.None);

        Assert.Equal(PersistenceMarkers.Failed, result.Marker);
        Assert.False(result.TurnMayComplete);
        Assert.Contains("validated receipt", result.FailureReason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PendingTransactionCannotCompleteTurn()
    {
        var store = new FakeStore
        {
            ForcedResult = new CommitResult(
                PersistenceMarkers.Pending,
                "Pending",
                false,
                2,
                null,
                null)
        };
        var coordinator = new PersistenceCoordinator(store);

        var result = await coordinator.CommitAsync(NewRequest(), CancellationToken.None);

        Assert.Equal(PersistenceMarkers.Pending, result.Marker);
        Assert.False(result.TurnMayComplete);
    }

    [Fact]
    public async Task RetryUsesExistingTransactionWithoutReapplyingMutations()
    {
        var store = new FakeStore();
        var coordinator = new PersistenceCoordinator(store);
        var request = NewRequest();
        await coordinator.CommitAsync(request, CancellationToken.None);
        var writesBeforeRetry = store.AppliedTransactions;

        var result = await coordinator.RetryAsync(
            request.CampaignId,
            request.TransactionId,
            CancellationToken.None);

        Assert.Equal(PersistenceMarkers.Saved, result.Marker);
        Assert.Equal(writesBeforeRetry, store.AppliedTransactions);
    }

    [Fact]
    public async Task AffectedSetMustExactlyMatchMutationOwners()
    {
        var coordinator = new PersistenceCoordinator(new FakeStore());
        var request = NewRequest() with { AffectedOwnerDomains = ["relationships"] };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            coordinator.CommitAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task ReadRequiresStableAddresses()
    {
        var coordinator = new PersistenceCoordinator(new FakeStore());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            coordinator.ReadRecordsAsync(
                "campaign",
                [new RecordAddress("characters", " ")],
                CancellationToken.None));
    }

    [Fact]
    public async Task StatusHidesSqlServerTopology()
    {
        var coordinator = new PersistenceCoordinator(new FakeStore());

        var status = await coordinator.GetStatusAsync("campaign", CancellationToken.None);
        var serialized = System.Text.Json.JsonSerializer.Serialize(status);

        Assert.Equal("MANAGED", status.Mode);
        Assert.DoesNotContain("SqlServer", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ServerManagedScheduleCanSatisfyConfiguredDurabilityBoundary()
    {
        var persistenceOptions = new SqlServerPersistenceOptions
        {
            ConnectionString = "Server=(local);Database=example;Integrated Security=true;",
            RequireRecoveryPointForCompletion = false
        };
        var service = new SqlServerDurabilityService(
            Options.Create(persistenceOptions),
            new ConfiguredCampaignSchemaResolver(Options.Create(persistenceOptions)));

        var evidence = await service.VerifyCompletionAsync("campaign", 2, CancellationToken.None);

        Assert.Contains("server-managed schedule", evidence, StringComparison.OrdinalIgnoreCase);
    }

    private static CommitCampaignRequest NewRequest() =>
        new(
            "campaign",
            "transaction",
            "idempotency-key",
            1,
            ["characters"],
            [new RecordMutation("characters", "entity-1", 1, "{\"name\":\"Example\"}", false)],
            "interaction-1");

    private sealed class FakeStore : ICampaignPersistenceStore
    {
        private readonly Dictionary<string, CommitResult> results = new(StringComparer.Ordinal);

        public int AppliedTransactions { get; private set; }

        public CommitResult? ForcedResult { get; init; }

        public Task<PersistenceStatus> GetStatusAsync(string campaignId, CancellationToken cancellationToken) =>
            Task.FromResult(new PersistenceStatus(
                PersistenceMarkers.Saved,
                "MANAGED",
                "Eternal Cycle Managed Data Service",
                campaignId,
                1,
                false,
                null,
                DateTimeOffset.UnixEpoch,
                null));

        public Task<ReadRecordsResult> ReadRecordsAsync(
            string campaignId,
            IReadOnlyList<RecordAddress> records,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ReadRecordsResult(campaignId, 1, []));

        public Task<CommitResult> CommitAsync(
            CommitCampaignRequest request,
            string requestHash,
            CancellationToken cancellationToken)
        {
            if (ForcedResult is not null)
            {
                return Task.FromResult(ForcedResult);
            }

            if (!results.TryGetValue(request.TransactionId, out var result))
            {
                AppliedTransactions++;
                var receipt = new PersistenceReceipt(
                    "receipt",
                    request.CampaignId,
                    request.TransactionId,
                    2,
                    PersistenceMarkers.Saved,
                    "Validated candidate and active-version read-back.",
                    DateTimeOffset.UnixEpoch);
                result = new CommitResult(
                    PersistenceMarkers.Saved,
                    "Completed",
                    true,
                    2,
                    receipt,
                    null);
                results[request.TransactionId] = result;
            }

            return Task.FromResult(result);
        }

        public Task<CommitResult> RetryAsync(
            string campaignId,
            string transactionId,
            CancellationToken cancellationToken) =>
            Task.FromResult(results[transactionId]);
    }
}
