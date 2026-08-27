using System.ComponentModel.DataAnnotations;

namespace EternalCycle.Persistence.Mcp;

public static class PersistenceMarkers
{
    public const string Saved = "\ud83d\udcbe";
    public const string Pending = "\u23f3";
    public const string Failed = "\u26a0\ufe0f";
}

public sealed record RecordAddress(string OwnerDomain, string RecordId);

public sealed record RecordReference(
    string RelationType,
    string TargetOwnerDomain,
    string TargetRecordId);

public sealed record RecordMutation(
    string OwnerDomain,
    string RecordId,
    long? ExpectedRevision,
    string PayloadJson,
    bool Tombstone,
    IReadOnlyList<RecordReference>? References = null);

public sealed record CommitCampaignRequest(
    string CampaignId,
    string TransactionId,
    string IdempotencyKey,
    long ExpectedParentVersion,
    IReadOnlyList<string> AffectedOwnerDomains,
    IReadOnlyList<RecordMutation> Mutations,
    string SourceInteractionId,
    string? Reason = null);

public sealed record PersistenceReceipt(
    string ReceiptId,
    string CampaignId,
    string TransactionId,
    long CampaignVersion,
    string Marker,
    string ValidationEvidence,
    DateTimeOffset CompletedAt);

public sealed record CommitResult(
    string Marker,
    string Status,
    bool TurnMayComplete,
    long? CampaignVersion,
    PersistenceReceipt? Receipt,
    string? FailureReason);

public sealed record PersistenceStatus(
    string Marker,
    string Mode,
    string CanonicalAuthority,
    string CampaignId,
    long CampaignVersion,
    bool PendingChanges,
    string? PendingTransactionId,
    DateTimeOffset? LastValidatedCommitAt,
    string? FailureReason);

public sealed record CanonicalRecord(
    string OwnerDomain,
    string RecordId,
    long RecordRevision,
    long CampaignVersion,
    string PayloadJson,
    bool Tombstone);

public sealed record ReadRecordsResult(
    string CampaignId,
    long CampaignVersion,
    IReadOnlyList<CanonicalRecord> Records);

public sealed class SqlServerPersistenceOptions
{
    [Required]
    public string ConnectionString { get; init; } = string.Empty;

    public int CommandTimeoutSeconds { get; init; } = 30;

    public bool RequireRecoveryPointForCompletion { get; init; }

    public string? RecoveryPointDirectory { get; init; }
}

public interface ICampaignPersistenceStore
{
    Task<PersistenceStatus> GetStatusAsync(string campaignId, CancellationToken cancellationToken);

    Task<ReadRecordsResult> ReadRecordsAsync(
        string campaignId,
        IReadOnlyList<RecordAddress> records,
        CancellationToken cancellationToken);

    Task<CommitResult> CommitAsync(
        CommitCampaignRequest request,
        string requestHash,
        CancellationToken cancellationToken);

    Task<CommitResult> RetryAsync(
        string campaignId,
        string transactionId,
        CancellationToken cancellationToken);
}

public interface IDurabilityService
{
    Task<string> VerifyCompletionAsync(
        string campaignId,
        long campaignVersion,
        CancellationToken cancellationToken);
}
