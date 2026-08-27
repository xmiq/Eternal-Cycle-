using System.ComponentModel;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

[McpServerToolType]
public sealed class PersistenceTools(PersistenceCoordinator coordinator)
{
    [McpServerTool(Name = "ec_persistence_status", ReadOnly = true, Idempotent = true),
     Description("Returns evidence-based status for an Eternal Cycle campaign without exposing database topology or credentials.")]
    public Task<PersistenceStatus> GetStatusAsync(
        [Description("Stable campaign identifier from Campaign Configuration.")] string campaignId,
        CancellationToken cancellationToken) =>
        coordinator.GetStatusAsync(campaignId, cancellationToken);

    [McpServerTool(Name = "ec_read_records", ReadOnly = true, Idempotent = true),
     Description("Reads authoritative campaign records by owner domain and stable record ID at the active campaign version.")]
    public Task<ReadRecordsResult> ReadRecordsAsync(
        [Description("Stable campaign identifier from Campaign Configuration.")] string campaignId,
        [Description("The exact owner-domain and stable-record addresses required by the Read Set.")] IReadOnlyList<RecordAddress> records,
        CancellationToken cancellationToken) =>
        coordinator.ReadRecordsAsync(campaignId, records, cancellationToken);

    [McpServerTool(Name = "ec_commit_changes", Destructive = true, Idempotent = true),
     Description("Stages, validates, atomically activates, and reads back one complete owner-routed campaign transaction.")]
    public Task<CommitResult> CommitAsync(
        [Description("Complete Affected Set, mutations, stable transaction identity, and expected parent version.")] CommitCampaignRequest request,
        CancellationToken cancellationToken) =>
        coordinator.CommitAsync(request, cancellationToken);

    [McpServerTool(Name = "ec_retry_persistence", Destructive = true, Idempotent = true),
     Description("Resumes validation or activation for an existing transaction without replaying gameplay effects.")]
    public Task<CommitResult> RetryAsync(
        [Description("Stable campaign identifier.")] string campaignId,
        [Description("Existing transaction identifier to resume.")] string transactionId,
        CancellationToken cancellationToken) =>
        coordinator.RetryAsync(campaignId, transactionId, cancellationToken);
}
