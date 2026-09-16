using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EternalCycle.Persistence.Mcp;

public sealed class PersistenceCoordinator(ICampaignPersistenceStore store)
{
    private static readonly JsonSerializerOptions CanonicalJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public Task<PersistenceStatus> GetStatusAsync(
        string campaignId,
        CancellationToken cancellationToken) =>
        store.GetStatusAsync(RequireIdentifier(campaignId, nameof(campaignId)), cancellationToken);

    public Task<ReadRecordsResult> ReadRecordsAsync(
        string campaignId,
        IReadOnlyList<RecordAddress> records,
        CancellationToken cancellationToken)
    {
        RequireIdentifier(campaignId, nameof(campaignId));
        ArgumentNullException.ThrowIfNull(records);

        if (records.Count is < 1 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(records), "Read requests must contain between 1 and 200 stable record addresses.");
        }

        foreach (var record in records)
        {
            RequireIdentifier(record.OwnerDomain, nameof(record.OwnerDomain));
            RequireIdentifier(record.RecordId, nameof(record.RecordId));
        }

        return store.ReadRecordsAsync(campaignId, records, cancellationToken);
    }

    public async Task<CommitResult> CommitAsync(
        CommitCampaignRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);
        var requestHash = ComputeHash(JsonSerializer.Serialize(request, CanonicalJsonOptions));
        var result = await store.CommitAsync(request, requestHash, cancellationToken);
        return EnforceReceiptBoundary(result);
    }

    public async Task<CommitResult> RetryAsync(
        string campaignId,
        string transactionId,
        CancellationToken cancellationToken)
    {
        var result = await store.RetryAsync(
            RequireIdentifier(campaignId, nameof(campaignId)),
            RequireIdentifier(transactionId, nameof(transactionId)),
            cancellationToken);
        return EnforceReceiptBoundary(result);
    }

    private static void Validate(CommitCampaignRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireIdentifier(request.CampaignId, nameof(request.CampaignId));
        RequireIdentifier(request.TransactionId, nameof(request.TransactionId));
        RequireIdentifier(request.IdempotencyKey, nameof(request.IdempotencyKey));
        RequireIdentifier(request.SourceInteractionId, nameof(request.SourceInteractionId));

        if (request.ExpectedParentVersion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.ExpectedParentVersion));
        }

        if (request.Mutations.Count is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Mutations), "A commit must contain between 1 and 500 mutations.");
        }

        var affected = request.AffectedOwnerDomains
            .Select(value => RequireIdentifier(value, "affectedOwnerDomain"))
            .ToHashSet(StringComparer.Ordinal);
        var actual = request.Mutations
            .Select(mutation => RequireIdentifier(mutation.OwnerDomain, nameof(mutation.OwnerDomain)))
            .ToHashSet(StringComparer.Ordinal);

        if (!affected.SetEquals(actual))
        {
            throw new ArgumentException("AffectedOwnerDomains must exactly match the owner domains represented by the mutations.");
        }

        var addresses = new HashSet<string>(StringComparer.Ordinal);
        foreach (var mutation in request.Mutations)
        {
            RequireIdentifier(mutation.RecordId, nameof(mutation.RecordId));
            if (!addresses.Add($"{mutation.OwnerDomain}\u001f{mutation.RecordId}"))
            {
                throw new ArgumentException("A transaction may mutate each authoritative record at most once.");
            }

            if (mutation.ExpectedRevision is < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mutation.ExpectedRevision));
            }

            if (!mutation.Tombstone)
            {
                using var document = JsonDocument.Parse(mutation.PayloadJson);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new ArgumentException("Canonical record payloads must be JSON objects.");
                }
            }

            foreach (var reference in mutation.References ?? [])
            {
                RequireIdentifier(reference.RelationType, nameof(reference.RelationType));
                RequireIdentifier(reference.TargetOwnerDomain, nameof(reference.TargetOwnerDomain));
                RequireIdentifier(reference.TargetRecordId, nameof(reference.TargetRecordId));
            }
        }
    }

    private static string RequireIdentifier(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
        {
            throw new ArgumentException("Stable identifiers must contain 1 to 128 non-whitespace characters.", parameterName);
        }

        return value;
    }

    private static string ComputeHash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static CommitResult EnforceReceiptBoundary(CommitResult result)
    {
        if (result.Marker == PersistenceMarkers.Saved &&
            (!result.TurnMayComplete || result.Receipt is null || result.Status != "Completed"))
        {
            return new CommitResult(
                PersistenceMarkers.Failed,
                "Failed",
                false,
                result.CampaignVersion,
                null,
                "The persistence service claimed completion without a validated receipt.");
        }

        if (result.Marker is not (PersistenceMarkers.Saved or PersistenceMarkers.Pending or PersistenceMarkers.Failed))
        {
            return new CommitResult(
                PersistenceMarkers.Failed,
                "Failed",
                false,
                result.CampaignVersion,
                null,
                "The persistence service returned an unknown status marker.");
        }

        if (result.Marker != PersistenceMarkers.Saved && result.TurnMayComplete)
        {
            return result with
            {
                Marker = PersistenceMarkers.Failed,
                Status = "Failed",
                TurnMayComplete = false,
                Receipt = null,
                FailureReason = "An incomplete persistence transaction cannot close a state-changing turn."
            };
        }

        return result;
    }
}
