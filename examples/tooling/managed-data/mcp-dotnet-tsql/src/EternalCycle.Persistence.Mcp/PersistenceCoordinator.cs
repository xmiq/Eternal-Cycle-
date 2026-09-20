using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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

    public async Task<CommitResult> PatchAsync(
        PatchCampaignRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);
        var requestHash = ComputeHash(JsonSerializer.Serialize(request, CanonicalJsonOptions));
        var records = await store.ReadRecordsAsync(
            request.CampaignId,
            request.Patches
                .Select(patch => new RecordAddress(patch.OwnerDomain, patch.RecordId))
                .ToArray(),
            cancellationToken);
        var byAddress = records.Records.ToDictionary(
            record => $"{record.OwnerDomain}\u001f{record.RecordId}",
            StringComparer.Ordinal);
        var mutations = new List<RecordMutation>(request.Patches.Count);
        foreach (var patch in request.Patches)
        {
            if (!byAddress.TryGetValue($"{patch.OwnerDomain}\u001f{patch.RecordId}", out var current) ||
                current.Tombstone)
            {
                throw new InvalidOperationException(
                    $"Merge patch requires an existing canonical record: {patch.OwnerDomain}/{patch.RecordId}.");
            }

            var payload = JsonNode.Parse(current.PayloadJson) as JsonObject
                ?? throw new InvalidOperationException("The authoritative record payload is not a JSON object.");
            var setValues = JsonNode.Parse(patch.SetValuesJson) as JsonObject
                ?? throw new ArgumentException("SetValuesJson must be a JSON object.", nameof(request));
            ApplySetValues(payload, setValues);
            mutations.Add(new RecordMutation(
                patch.OwnerDomain,
                patch.RecordId,
                patch.ExpectedRevision,
                payload.ToJsonString(CanonicalJsonOptions),
                false));
        }

        var expanded = new CommitCampaignRequest(
            request.CampaignId,
            request.TransactionId,
            request.IdempotencyKey,
            request.ExpectedParentVersion,
            request.AffectedOwnerDomains,
            mutations,
            request.SourceInteractionId,
            request.Reason);
        Validate(expanded);
        var result = await store.CommitAsync(expanded, requestHash, cancellationToken);
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

    private static void Validate(PatchCampaignRequest request)
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

        if (request.Patches.Count is < 1 or > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Patches), "A merge-patch transaction must contain between 1 and 200 existing records.");
        }

        var affected = request.AffectedOwnerDomains
            .Select(value => RequireIdentifier(value, "affectedOwnerDomain"))
            .ToHashSet(StringComparer.Ordinal);
        var actual = request.Patches
            .Select(patch => RequireIdentifier(patch.OwnerDomain, nameof(patch.OwnerDomain)))
            .ToHashSet(StringComparer.Ordinal);
        if (!affected.SetEquals(actual))
        {
            throw new ArgumentException("AffectedOwnerDomains must exactly match the owner domains represented by the patches.");
        }

        var addresses = new HashSet<string>(StringComparer.Ordinal);
        foreach (var patch in request.Patches)
        {
            RequireIdentifier(patch.RecordId, nameof(patch.RecordId));
            if (patch.ExpectedRevision < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patch.ExpectedRevision));
            }

            if (!addresses.Add($"{patch.OwnerDomain}\u001f{patch.RecordId}"))
            {
                throw new ArgumentException("A transaction may patch each authoritative record at most once.");
            }

            var setValues = JsonNode.Parse(patch.SetValuesJson) as JsonObject
                ?? throw new ArgumentException("SetValuesJson must be a JSON object.");
            if (setValues.Count == 0)
            {
                throw new ArgumentException("A record merge patch cannot be empty.");
            }

            ValidatePatchKeys(setValues);
        }
    }

    private static void ApplySetValues(JsonObject target, JsonObject patch)
    {
        foreach (var (name, value) in patch)
        {
            if (value is JsonObject patchObject && target[name] is JsonObject targetObject)
            {
                ApplySetValues(targetObject, patchObject);
                continue;
            }

            target[name] = value?.DeepClone();
        }
    }

    private static void ValidatePatchKeys(JsonObject patch)
    {
        foreach (var (name, value) in patch)
        {
            var normalized = name.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
            if (normalized is "id" or "recordid" or "ownerdomain" or "references" ||
                normalized.EndsWith("id", StringComparison.Ordinal) ||
                normalized.EndsWith("ids", StringComparison.Ordinal) ||
                normalized.Contains("reference", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Merge patches cannot alter identity or reference-bearing field '{name}'; use the full mutation path.");
            }

            if (value is JsonObject child)
            {
                ValidatePatchKeys(child);
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
