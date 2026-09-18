using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public enum ManagedOperationState
{
    Queued,
    Running,
    Succeeded,
    Failed,
    Cancelling,
    Cancelled,
    Interrupted
}

public static class ManagedOperationKinds
{
    public const string InitialRulePublication = "InitialRulePublication";
}

public sealed record ManagedOperationStatus(
    string OperationId,
    string OperationKind,
    string CorrelationId,
    ManagedOperationState State,
    string CurrentStage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt,
    int? ProgressPercent,
    string SafeStatusDetail,
    string? ErrorCode,
    bool RetrySafe,
    bool UserApprovalRequired,
    bool AdministrativeInterventionRequired,
    string? RulesetId,
    string? SourceIdentity,
    string? ResultRuleReleaseId);

public sealed record ManagedOperationEnqueueRequest(
    string OperationKind,
    string DeduplicationKey,
    string RulesetId,
    string SafeStatusDetail);

public interface IManagedOperationStore
{
    Task<ManagedOperationStatus> EnqueueOrReuseAsync(
        ManagedOperationEnqueueRequest request,
        CancellationToken cancellationToken);

    Task<ManagedOperationStatus?> GetAsync(string operationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ManagedOperationStatus>> ListRecentAsync(
        string? operationKind,
        int maximumCount,
        CancellationToken cancellationToken);

    Task<int> RecoverInterruptedAsync(CancellationToken cancellationToken);

    Task<ManagedOperationStatus?> ClaimNextAsync(CancellationToken cancellationToken);

    Task UpdateStageAsync(
        string operationId,
        string stage,
        int? progressPercent,
        string safeStatusDetail,
        CancellationToken cancellationToken);

    Task CompleteAsync(
        string operationId,
        string stage,
        string safeStatusDetail,
        string? sourceIdentity,
        string? resultRuleReleaseId,
        CancellationToken cancellationToken);

    Task FailAsync(
        string operationId,
        ManagedOperationState state,
        string stage,
        string errorCode,
        string safeStatusDetail,
        bool retrySafe,
        bool administrativeInterventionRequired,
        CancellationToken cancellationToken);
}

public interface IManagedOperationService
{
    Task<ManagedOperationStatus> EnqueueInitialRulePublicationAsync(CancellationToken cancellationToken);

    Task<ManagedOperationStatus?> GetAsync(string operationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ManagedOperationStatus>> ListRecentAsync(
        string? operationKind,
        int maximumCount,
        CancellationToken cancellationToken);
}

public sealed class ManagedOperationService(
    IManagedOperationStore store,
    IRuleSourceConfigurationStore sourceConfigurations,
    IOptions<ManagedRuleServiceOptions> ruleOptions) : IManagedOperationService
{
    private readonly ManagedRuleServiceOptions rules = ruleOptions.Value;

    public async Task<ManagedOperationStatus> EnqueueInitialRulePublicationAsync(
        CancellationToken cancellationToken)
    {
        var source = await sourceConfigurations.GetAsync(rules.RulesetId, cancellationToken)
            ?? throw new ManagedServiceException(
                "RULE_SOURCE_NOT_CONFIGURED",
                "No Rule Source is configured. Select Stable or Prerelease before publishing rules.");
        var deduplicationKey = string.Join(
            ':',
            ManagedOperationKinds.InitialRulePublication,
            rules.RulesetId,
            source.ConfigurationRevision,
            source.ReleaseChannel);
        return await store.EnqueueOrReuseAsync(
            new ManagedOperationEnqueueRequest(
                ManagedOperationKinds.InitialRulePublication,
                deduplicationKey,
                rules.RulesetId,
                "Initial rule publication is queued for durable background execution."),
            cancellationToken);
    }

    public Task<ManagedOperationStatus?> GetAsync(
        string operationId,
        CancellationToken cancellationToken) =>
        store.GetAsync(RequireOperationId(operationId), cancellationToken);

    public Task<IReadOnlyList<ManagedOperationStatus>> ListRecentAsync(
        string? operationKind,
        int maximumCount,
        CancellationToken cancellationToken) =>
        store.ListRecentAsync(
            string.IsNullOrWhiteSpace(operationKind) ? null : operationKind.Trim(),
            Math.Clamp(maximumCount, 1, 50),
            cancellationToken);

    private static string RequireOperationId(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
        {
            throw new ManagedServiceException("INVALID_OPERATION_ID", "Operation ID is invalid.");
        }

        return value.Trim();
    }
}

public interface IRulePublicationExecutor
{
    Task<RulePublicationResult> ExecuteAsync(CancellationToken cancellationToken);

    Task<RulePublicationResult> ExecuteAsync(
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken) =>
        ExecuteAsync(cancellationToken);
}

public sealed class ManagedRulePublicationExecutor(ManagedRulePublicationCoordinator coordinator)
    : IRulePublicationExecutor
{
    public Task<RulePublicationResult> ExecuteAsync(CancellationToken cancellationToken) =>
        coordinator.CheckForUpdateAsync(cancellationToken);

    public Task<RulePublicationResult> ExecuteAsync(
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken) =>
        coordinator.CheckForUpdateAsync(onStage, cancellationToken);
}

public sealed class ManagedOperationProcessor(
    IManagedOperationStore store,
    IRulePublicationExecutor rulePublication,
    IOptions<ManagedRuleServiceOptions> options,
    ILogger<ManagedOperationProcessor> logger)
{
    private readonly ManagedRuleServiceOptions settings = options.Value;

    public async Task<bool> ProcessNextAsync(CancellationToken stoppingToken)
    {
        var operation = await store.ClaimNextAsync(stoppingToken);
        if (operation is null)
        {
            return false;
        }

        var currentStage = operation.CurrentStage;
        try
        {
            if (!string.Equals(
                    operation.OperationKind,
                    ManagedOperationKinds.InitialRulePublication,
                    StringComparison.Ordinal))
            {
                await store.FailAsync(
                    operation.OperationId,
                    ManagedOperationState.Failed,
                    operation.CurrentStage,
                    "MANAGED_OPERATION_KIND_UNSUPPORTED",
                    "The persisted Managed Operation kind is unsupported by this service version.",
                    retrySafe: false,
                    administrativeInterventionRequired: true,
                    CancellationToken.None);
                return true;
            }

            using var timeout = new CancellationTokenSource(
                settings.ManagedOperationTimeout > TimeSpan.Zero
                    ? settings.ManagedOperationTimeout
                    : TimeSpan.FromMinutes(30));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeout.Token);
            void ReportStage(RulePublicationStage stage)
            {
                currentStage = stage.ToString();
                store.UpdateStageAsync(
                        operation.OperationId,
                        currentStage,
                        ProgressFor(stage),
                        SafeStatusFor(stage),
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }

            ReportStage(RulePublicationStage.AcquireSource);
            var result = await rulePublication.ExecuteAsync(ReportStage, linked.Token);
            if (result.Status is "Activated" or "Unchanged" or "AlreadyPublished" or "AwaitingAdministratorActivation")
            {
                await store.CompleteAsync(
                    operation.OperationId,
                    result.Stage ?? RulePublicationStage.RecordUpdateCheck.ToString(),
                    "The durable initial Rule Release workflow completed and its result is available.",
                    result.SourceIdentity,
                    result.CandidateReleaseId ?? result.ActiveReleaseId,
                    CancellationToken.None);
            }
            else
            {
                await store.FailAsync(
                    operation.OperationId,
                    result.Status == "Cancelled" ? ManagedOperationState.Cancelled : ManagedOperationState.Failed,
                    result.Stage ?? "Unknown",
                    result.ErrorCode ?? "RULE_PUBLICATION_FAILED",
                    result.FailureReason ?? "Rule publication failed; inspect sanitized diagnostics.",
                    result.RetrySafe,
                    result.AdministrativeInterventionRequired,
                    CancellationToken.None);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
                await store.FailAsync(
                    operation.OperationId,
                    ManagedOperationState.Interrupted,
                    currentStage,
                "MANAGED_OPERATION_INTERRUPTED",
                "Service shutdown interrupted the operation. It is eligible for deterministic recovery.",
                retrySafe: true,
                administrativeInterventionRequired: false,
                CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
                await store.FailAsync(
                    operation.OperationId,
                    ManagedOperationState.Failed,
                    currentStage,
                "MANAGED_OPERATION_TIMEOUT",
                "The operation exceeded the Managed Service timeout and may be retried safely.",
                retrySafe: true,
                administrativeInterventionRequired: false,
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Managed operation {OperationId} failed with {ExceptionType}.",
                operation.OperationId,
                exception.GetType().FullName);
                await store.FailAsync(
                    operation.OperationId,
                    ManagedOperationState.Failed,
                    currentStage,
                "MANAGED_OPERATION_FAILED",
                "The Managed Operation failed; inspect authorized sanitized diagnostics.",
                retrySafe: true,
                administrativeInterventionRequired: false,
                CancellationToken.None);
        }

        return true;
    }

    private static int ProgressFor(RulePublicationStage stage) => stage switch
    {
        RulePublicationStage.AcquireSource => 5,
        RulePublicationStage.ResolveRef => 12,
        RulePublicationStage.ReadManifest => 20,
        RulePublicationStage.ReadRuleDocuments => 30,
        RulePublicationStage.Compile => 45,
        RulePublicationStage.Stage => 55,
        RulePublicationStage.Validate => 65,
        RulePublicationStage.PrepareMinimumClosure => 72,
        RulePublicationStage.Publish => 80,
        RulePublicationStage.Activate => 88,
        RulePublicationStage.PrepareRemainingRules => 94,
        RulePublicationStage.RecordUpdateCheck => 98,
        _ => 1
    };

    private static string SafeStatusFor(RulePublicationStage stage) => stage switch
    {
        RulePublicationStage.AcquireSource => "Rule Source acquisition is running under Managed Service policy.",
        RulePublicationStage.ResolveRef => "The configured discovery reference is being resolved to immutable provenance.",
        RulePublicationStage.ReadManifest => "The Rule Source manifest is being read and checked.",
        RulePublicationStage.ReadRuleDocuments => "Canonical rule documents are being read from the resolved source.",
        RulePublicationStage.Compile => "Canonical rules are being compiled into a derived Rule Release.",
        RulePublicationStage.Stage => "The candidate Rule Release is being durably staged.",
        RulePublicationStage.Validate => "The candidate Rule Release is being validated.",
        RulePublicationStage.PrepareMinimumClosure => "The minimum authoritative gameplay closure is being prepared.",
        RulePublicationStage.Publish => "The validated Rule Release is being published.",
        RulePublicationStage.Activate => "The published Rule Release is being activated atomically.",
        RulePublicationStage.PrepareRemainingRules => "Remaining rule modules are being prepared by dependency priority.",
        RulePublicationStage.RecordUpdateCheck => "The durable publication outcome is being recorded.",
        _ => "The Managed Operation is running."
    };
}

public sealed class ManagedOperationWorker(
    IManagedOperationStore store,
    ManagedOperationProcessor processor,
    IOptions<ManagedRuleServiceOptions> options,
    ILogger<ManagedOperationWorker> logger) : BackgroundService
{
    private readonly ManagedRuleServiceOptions settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = settings.ManagedOperationPollInterval > TimeSpan.Zero
            ? settings.ManagedOperationPollInterval
            : TimeSpan.FromSeconds(1);
        var recoveryComplete = false;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!recoveryComplete)
                {
                    var recovered = await store.RecoverInterruptedAsync(stoppingToken);
                    recoveryComplete = true;
                    if (recovered > 0)
                    {
                        logger.LogWarning(
                            "Recovered {RecoveredCount} Managed Operations that were Running when the service stopped.",
                            recovered);
                    }
                }

                if (!await processor.ProcessNextAsync(stoppingToken))
                {
                    await Task.Delay(delay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                // A fresh service may start before administrative migrations create
                // the operation store. Re-run recovery after any store outage so a
                // claimed operation cannot remain phantom-Running when it returns.
                recoveryComplete = false;
                logger.LogWarning(
                    "Managed-operation polling is waiting for its durable store after {ExceptionType}.",
                    exception.GetType().FullName);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}

[McpServerToolType]
public sealed class ManagedOperationTools(
    IManagedOperationService operations,
    IManagedReadinessService? readiness = null)
{
    [McpServerTool(Name = "ec_get_operation_status", ReadOnly = true, Idempotent = true),
     Description("Returns durable Managed Operation state, current stage, safe causal status, and result identity without requiring a campaign.")]
    public async Task<ManagedOperationResult<ManagedOperationStatus>> GetStatusAsync(
        [Description("Operation ID returned by a Managed administrative action.")] string operationId,
        CancellationToken cancellationToken)
    {
        var blocked = await PreMigrationBlockAsync<ManagedOperationStatus>(cancellationToken);
        if (blocked is not null)
        {
            return blocked;
        }

        var operation = await operations.GetAsync(operationId, cancellationToken);
        return operation is null
            ? new(false, "MANAGED_OPERATION_NOT_FOUND", "No Managed Operation with that ID exists.", null)
            : new(true, "MANAGED_OPERATION_STATUS", "Durable Managed Operation status is available.", operation);
    }

    [McpServerTool(Name = "ec_list_managed_operations", ReadOnly = true, Idempotent = true),
     Description("Lists recent safe Managed Operation summaries after the durable-operation schema exists. If migration is required, returns the supported migration recovery route rather than querying post-migration tables.")]
    public async Task<ManagedOperationResult<IReadOnlyList<ManagedOperationStatus>>> ListRecentAsync(
        [Description("Optional operation kind filter; omit for all kinds.")] string? operationKind,
        [Description("Maximum results from 1 through 50.")] int maximumCount,
        CancellationToken cancellationToken)
    {
        var blocked = await PreMigrationBlockAsync<IReadOnlyList<ManagedOperationStatus>>(cancellationToken);
        if (blocked is not null)
        {
            return blocked;
        }

        var result = await operations.ListRecentAsync(operationKind, maximumCount, cancellationToken);
        return new(
            true,
            "MANAGED_OPERATION_LIST",
            "Recent durable Managed Operations are available.",
            result);
    }

    private async Task<ManagedOperationResult<T>?> PreMigrationBlockAsync<T>(CancellationToken cancellationToken)
    {
        if (readiness is null)
        {
            return null;
        }

        var report = await readiness.GetReadinessAsync(null, cancellationToken);
        if (report.State is not (
            ManagedReadinessState.ConfigurationRequired or
            ManagedReadinessState.ConfigurationInvalid or
            ManagedReadinessState.SetupRequired or
            ManagedReadinessState.MigrationRequired))
        {
            return null;
        }

        return new(
            false,
            report.ErrorCode ?? "SETUP_REQUIRED",
            report.Message,
            default,
            RetrySafe: false,
            AdministrativeInterventionRequired: report.AdministrativeActionRequired,
            BlockingCondition: report.BlockingCondition,
            RequiredAction: report.RequiredAction,
            AllowedNextActions: report.AllowedNextActions,
            RecommendedNextAction: report.RecommendedNextAction);
    }
}
