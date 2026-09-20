using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.Options;
using System.Text.Json;
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

    [Fact]
    public async Task MergePatchPreservesUnrelatedAuthoritativeState()
    {
        var store = new FakeStore(new CanonicalRecord(
            "scene-state",
            "scene-1",
            4,
            7,
            "{\"npc\":{\"alive\":true,\"name\":\"Mira\"},\"player\":{\"position\":\"gate\",\"inventory\":[\"torch\"]},\"weather\":\"rain\"}",
            false));
        var coordinator = new PersistenceCoordinator(store);
        var request = PatchRequest("{\"npc\":{\"alive\":false},\"player\":{\"position\":\"courtyard\",\"inventory\":[\"torch\",\"sword\"]}}");

        var result = await coordinator.PatchAsync(request, CancellationToken.None);

        Assert.Equal(PersistenceMarkers.Saved, result.Marker);
        var mutation = Assert.Single(store.LastRequest!.Mutations);
        using var payload = JsonDocument.Parse(mutation.PayloadJson);
        Assert.False(payload.RootElement.GetProperty("npc").GetProperty("alive").GetBoolean());
        Assert.Equal("Mira", payload.RootElement.GetProperty("npc").GetProperty("name").GetString());
        Assert.Equal("rain", payload.RootElement.GetProperty("weather").GetString());
        Assert.Equal("courtyard", payload.RootElement.GetProperty("player").GetProperty("position").GetString());
        Assert.Equal(2, payload.RootElement.GetProperty("player").GetProperty("inventory").GetArrayLength());
        Assert.Equal(7, store.LastRequest.ExpectedParentVersion);
        Assert.Equal(4, mutation.ExpectedRevision);
    }

    [Fact]
    public async Task MergePatchRetryIsIdempotentAndDoesNotReapplyGameplayEffects()
    {
        var store = new FakeStore(new CanonicalRecord(
            "scene-state", "scene-1", 4, 7, "{\"patrolAlert\":false,\"counter\":1}", false));
        var coordinator = new PersistenceCoordinator(store);
        var request = PatchRequest("{\"patrolAlert\":true}");

        var first = await coordinator.PatchAsync(request, CancellationToken.None);
        var second = await coordinator.PatchAsync(request, CancellationToken.None);

        Assert.Equal(first.Receipt, second.Receipt);
        Assert.Equal(1, store.AppliedTransactions);
    }

    [Fact]
    public async Task FailedMergePatchDoesNotPartiallyMutateCanonicalState()
    {
        var original = new CanonicalRecord(
            "scene-state", "scene-1", 4, 7, "{\"alive\":true,\"weather\":\"rain\"}", false);
        var store = new FakeStore(original)
        {
            ForcedResult = new CommitResult(
                PersistenceMarkers.Failed,
                "Failed",
                false,
                7,
                null,
                "validation failed")
        };
        var coordinator = new PersistenceCoordinator(store);

        var result = await coordinator.PatchAsync(PatchRequest("{\"alive\":false}"), CancellationToken.None);
        var read = await store.ReadRecordsAsync(
            "campaign",
            [new RecordAddress("scene-state", "scene-1")],
            CancellationToken.None);

        Assert.Equal(PersistenceMarkers.Failed, result.Marker);
        Assert.Equal(original.PayloadJson, Assert.Single(read.Records).PayloadJson);
    }

    [Fact]
    public async Task MergePatchRejectsIdentityAndReferenceChanges()
    {
        var store = new FakeStore(new CanonicalRecord(
            "scene-state", "scene-1", 4, 7, "{\"locationId\":\"old\"}", false));
        var coordinator = new PersistenceCoordinator(store);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            coordinator.PatchAsync(PatchRequest("{\"locationId\":\"new\"}"), CancellationToken.None));

        Assert.Contains("full mutation path", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, store.AppliedTransactions);
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

    private static PatchCampaignRequest PatchRequest(string patchJson) =>
        new(
            "campaign",
            "patch-transaction",
            "patch-idempotency",
            7,
            ["scene-state"],
            [new RecordMergePatch("scene-state", "scene-1", 4, patchJson)],
            "interaction-1");

    private sealed class FakeStore : ICampaignPersistenceStore
    {
        private readonly Dictionary<string, CommitResult> results = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CanonicalRecord> records = new(StringComparer.Ordinal);

        public FakeStore(params CanonicalRecord[] initialRecords)
        {
            foreach (var record in initialRecords)
            {
                records[$"{record.OwnerDomain}\u001f{record.RecordId}"] = record;
            }
        }

        public int AppliedTransactions { get; private set; }

        public CommitResult? ForcedResult { get; init; }

        public CommitCampaignRequest? LastRequest { get; private set; }

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
            CancellationToken cancellationToken)
        {
            var found = records
                .Select(address => this.records.GetValueOrDefault($"{address.OwnerDomain}\u001f{address.RecordId}"))
                .Where(record => record is not null)
                .Cast<CanonicalRecord>()
                .ToArray();
            var version = found.Length == 0 ? 1 : found.Max(record => record.CampaignVersion);
            return Task.FromResult(new ReadRecordsResult(campaignId, version, found));
        }

        public Task<CommitResult> CommitAsync(
            CommitCampaignRequest request,
            string requestHash,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            if (ForcedResult is not null)
            {
                return Task.FromResult(ForcedResult);
            }

            if (!results.TryGetValue(request.TransactionId, out var result))
            {
                AppliedTransactions++;
                foreach (var mutation in request.Mutations)
                {
                    var key = $"{mutation.OwnerDomain}\u001f{mutation.RecordId}";
                    var previous = records.GetValueOrDefault(key);
                    records[key] = new CanonicalRecord(
                        mutation.OwnerDomain,
                        mutation.RecordId,
                        (previous?.RecordRevision ?? 0) + 1,
                        request.ExpectedParentVersion + 1,
                        mutation.PayloadJson,
                        mutation.Tombstone);
                }
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
