using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public enum RuleUpdatePolicy
{
    Disabled,
    Startup,
    Periodic,
    Manual
}

public enum RuleActivationPolicy
{
    Automatic,
    Manual
}

public enum RuleReleaseState
{
    Candidate,
    Validated,
    Published,
    Active,
    Failed
}

public sealed class ManagedRuleServiceOptions
{
    public string RulesetId { get; init; } = "eternal-cycle-core";

    public string CompilerVersion { get; init; } = "1";

    public int MaximumEstimatedTokens { get; init; } = RuleCompiler.DefaultContextBudget;

    public string DefaultCampaignMode { get; init; } = "NORMAL";

    public IDictionary<string, string> CampaignModes { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public IDictionary<string, string[]> CampaignOptionalModules { get; init; } =
        new Dictionary<string, string[]>(StringComparer.Ordinal);

    public IDictionary<string, string> CampaignPinnedRuleReleaseIds { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);

    public RuleUpdatePolicy UpdatePolicy { get; init; } = RuleUpdatePolicy.Disabled;

    public RuleActivationPolicy ActivationPolicy { get; init; } = RuleActivationPolicy.Automatic;

    public TimeSpan UpdateInterval { get; init; } = TimeSpan.FromHours(6);

    public GitRuleSourceOptions GitSource { get; init; } = new();
}

public sealed record RuleSourceSnapshot(
    string ProviderKind,
    string SourceIdentity,
    string RepositoryVersion,
    IReadOnlyList<RuleSourceDocument> Documents,
    DateTimeOffset AcquiredAt);

public sealed record PublishedRuleRelease(
    string RuleReleaseId,
    string RulesetId,
    string ProviderKind,
    string SourceIdentity,
    string RepositoryVersion,
    string CompilerVersion,
    RuleReleaseState State,
    CompiledRuleIndex Index,
    DateTimeOffset CreatedAt,
    string? FailureReason = null);

public sealed record RulePublicationResult(
    string Status,
    string? CandidateReleaseId,
    string? ActiveReleaseId,
    string? SourceIdentity,
    bool ActiveReleasePreserved,
    string? FailureReason);

public sealed record RuleUpdateCheck(
    string RulesetId,
    string? SourceIdentity,
    string Outcome,
    string? SanitizedDetail,
    DateTimeOffset CheckedAt);

public interface IRuleSourceProvider
{
    Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}

public interface IPublishedRuleStore
{
    Task<PublishedRuleRelease?> GetActiveAsync(string rulesetId, CancellationToken cancellationToken);

    Task<PublishedRuleRelease?> FindBySourceAsync(
        string rulesetId,
        string sourceIdentity,
        CancellationToken cancellationToken);

    Task<PublishedRuleRelease?> GetByIdAsync(
        string rulesetId,
        string ruleReleaseId,
        CancellationToken cancellationToken);

    Task<RuleUpdateCheck?> GetLatestUpdateCheckAsync(
        string rulesetId,
        CancellationToken cancellationToken);

    Task StageCandidateAsync(PublishedRuleRelease release, CancellationToken cancellationToken);

    Task SetStateAsync(
        string ruleReleaseId,
        RuleReleaseState state,
        string? failureReason,
        CancellationToken cancellationToken);

    Task ActivateAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken);

    Task RecordUpdateCheckAsync(RuleUpdateCheck updateCheck, CancellationToken cancellationToken);
}

public sealed class ManagedRulePublicationCoordinator(
    IRuleSourceProvider sourceProvider,
    IPublishedRuleStore store,
    IOptions<ManagedRuleServiceOptions> options,
    ILogger<ManagedRulePublicationCoordinator> logger)
{
    private readonly ManagedRuleServiceOptions settings = options.Value;

    public async Task<RulePublicationResult> CheckForUpdateAsync(CancellationToken cancellationToken)
    {
        var active = await store.GetActiveAsync(settings.RulesetId, cancellationToken);
        RuleSourceSnapshot snapshot;
        try
        {
            snapshot = await sourceProvider.GetSnapshotAsync(cancellationToken);
        }
        catch (Exception exception) when (active is not null)
        {
            logger.LogWarning(exception, "Rule source check failed; retaining active release {ReleaseId}.", active.RuleReleaseId);
            return await CompleteAsync(new RulePublicationResult(
                "SourceUnavailableUsingActive",
                null,
                active.RuleReleaseId,
                active.SourceIdentity,
                true,
                Sanitize(exception)), "Degraded", cancellationToken);
        }
        catch (Exception exception)
        {
            await store.RecordUpdateCheckAsync(
                new RuleUpdateCheck(
                    settings.RulesetId,
                    null,
                    "Failed",
                    Sanitize(exception),
                    DateTimeOffset.UtcNow),
                cancellationToken);
            throw;
        }

        if (active is not null &&
            string.Equals(active.SourceIdentity, snapshot.SourceIdentity, StringComparison.Ordinal))
        {
            return await CompleteAsync(new RulePublicationResult(
                "Unchanged",
                null,
                active.RuleReleaseId,
                active.SourceIdentity,
                true,
                null), "Unchanged", cancellationToken);
        }

        var prior = await store.FindBySourceAsync(settings.RulesetId, snapshot.SourceIdentity, cancellationToken);
        if (prior is not null && prior.State is RuleReleaseState.Active or RuleReleaseState.Published)
        {
            return await CompleteAsync(new RulePublicationResult(
                "AlreadyPublished",
                prior.RuleReleaseId,
                active?.RuleReleaseId,
                prior.SourceIdentity,
                active is not null,
                null), "Candidate", cancellationToken);
        }

        var releaseId = $"RULE-{Guid.NewGuid():N}";
        try
        {
            var index = RuleCompiler.Compile(snapshot.RepositoryVersion, snapshot.Documents);
            var candidate = new PublishedRuleRelease(
                releaseId,
                settings.RulesetId,
                snapshot.ProviderKind,
                snapshot.SourceIdentity,
                snapshot.RepositoryVersion,
                settings.CompilerVersion,
                RuleReleaseState.Candidate,
                index,
                DateTimeOffset.UtcNow);

            await store.StageCandidateAsync(candidate, cancellationToken);
            ValidateCandidate(index);
            await store.SetStateAsync(releaseId, RuleReleaseState.Validated, null, cancellationToken);
            await store.SetStateAsync(releaseId, RuleReleaseState.Published, null, cancellationToken);

            if (settings.ActivationPolicy == RuleActivationPolicy.Automatic)
            {
                await store.ActivateAsync(settings.RulesetId, releaseId, cancellationToken);
                return await CompleteAsync(new RulePublicationResult(
                    "Activated",
                    releaseId,
                    releaseId,
                    snapshot.SourceIdentity,
                    active is not null,
                    null), "Activated", cancellationToken);
            }

            return await CompleteAsync(new RulePublicationResult(
                "AwaitingAdministratorActivation",
                releaseId,
                active?.RuleReleaseId,
                snapshot.SourceIdentity,
                active is not null,
                null), "Candidate", cancellationToken);
        }
        catch (Exception exception)
        {
            try
            {
                await store.SetStateAsync(
                    releaseId,
                    RuleReleaseState.Failed,
                    Sanitize(exception),
                    cancellationToken);
            }
            catch (Exception stateException)
            {
                logger.LogWarning(stateException, "Could not persist failed state for rule release {ReleaseId}.", releaseId);
            }

            logger.LogError(exception, "Rule release candidate {ReleaseId} failed; the active release is unchanged.", releaseId);
            return await CompleteAsync(new RulePublicationResult(
                "CandidateFailed",
                releaseId,
                active?.RuleReleaseId,
                snapshot.SourceIdentity,
                active is not null,
                Sanitize(exception)), "Failed", cancellationToken);
        }
    }

    public Task ActivateAsync(string ruleReleaseId, CancellationToken cancellationToken) =>
        store.ActivateAsync(settings.RulesetId, ruleReleaseId, cancellationToken);

    private static void ValidateCandidate(CompiledRuleIndex index)
    {
        if (index.Chunks.Count == 0 ||
            !index.Chunks.Any(chunk => chunk.Metadata.Layer == RuleLayer.RuntimeKernel))
        {
            throw new InvalidOperationException("A Rule Release requires a Runtime Rule Kernel and at least one chunk.");
        }

        if (index.Chunks.Any(chunk => chunk.EstimatedTokens <= 0 || string.IsNullOrWhiteSpace(chunk.SourceHash)))
        {
            throw new InvalidOperationException("Every compiled rule chunk requires a token estimate and source hash.");
        }
    }

    private static string Sanitize(Exception exception) =>
        $"{exception.GetType().Name}: rule update operation failed; inspect authorized service logs.";

    private async Task<RulePublicationResult> CompleteAsync(
        RulePublicationResult result,
        string outcome,
        CancellationToken cancellationToken)
    {
        await store.RecordUpdateCheckAsync(
            new RuleUpdateCheck(
                settings.RulesetId,
                result.SourceIdentity,
                outcome,
                result.FailureReason,
                DateTimeOffset.UtcNow),
            cancellationToken);
        return result;
    }
}

public sealed class PublishedRuleContextProvider(
    IPublishedRuleStore store,
    IOptions<ManagedRuleServiceOptions> options,
    ICampaignSchemaResolver schemaResolver) : IRuleContextProvider
{
    private readonly ManagedRuleServiceOptions settings = options.Value;

    public async Task<RuleContextResult> GetContextAsync(
        RuleContextRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var route = schemaResolver.Resolve(request.CampaignId);
        var release = settings.CampaignPinnedRuleReleaseIds.TryGetValue(request.CampaignId, out var pinnedReleaseId)
            ? await store.GetByIdAsync(route.RulesetId, pinnedReleaseId, cancellationToken)
            : await store.GetActiveAsync(route.RulesetId, cancellationToken);
        if (release is null || release.State is not (RuleReleaseState.Active or RuleReleaseState.Published))
        {
            throw new InvalidOperationException(
                $"No compatible published Rule Release exists for RuleSet '{route.RulesetId}'.");
        }

        if (!IsCompatibleVersion(route.RulesetVersion, release.RepositoryVersion))
        {
            throw new InvalidOperationException(
                $"Rule Release '{release.RuleReleaseId}' is incompatible with configured RuleSet version '{route.RulesetVersion}'.");
        }
        var campaignMode = settings.CampaignModes.TryGetValue(request.CampaignId, out var configuredMode)
            ? configuredMode
            : settings.DefaultCampaignMode;
        var modules = settings.CampaignOptionalModules.TryGetValue(request.CampaignId, out var configuredModules)
            ? configuredModules
            : [];
        var configuredMaximum = settings.MaximumEstimatedTokens is > 0 and <= RuleCompiler.DefaultContextBudget
            ? settings.MaximumEstimatedTokens
            : RuleCompiler.DefaultContextBudget;
        var requestedMaximum = request.MaxEstimatedTokens > 0
            ? Math.Min(request.MaxEstimatedTokens, configuredMaximum)
            : configuredMaximum;

        var selected = RuleCompiler.Select(
            release.Index,
            route,
            request with
            {
                CampaignMode = campaignMode,
                OptionalModules = modules,
                MaxEstimatedTokens = requestedMaximum
            });

        return selected with
        {
            RuleReleaseId = release.RuleReleaseId,
            SourceIdentity = release.SourceIdentity
        };
    }

    private static bool IsCompatibleVersion(string configuredVersion, string publishedVersion)
    {
        static string Major(string value) => value
            .Split('+', 2, StringSplitOptions.TrimEntries)[0]
            .Split('-', 2, StringSplitOptions.TrimEntries)[0]
            .Split('.', 2, StringSplitOptions.TrimEntries)[0];

        return string.Equals(Major(configuredVersion), Major(publishedVersion), StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class ManagedRuleUpdateHostedService(
    ManagedRulePublicationCoordinator coordinator,
    IManagedReadinessService readiness,
    IOptions<ManagedRuleServiceOptions> options,
    ILogger<ManagedRuleUpdateHostedService> logger) : BackgroundService
{
    private readonly ManagedRuleServiceOptions settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (settings.UpdatePolicy is RuleUpdatePolicy.Disabled or RuleUpdatePolicy.Manual)
        {
            return;
        }

        await RunCheckAsync(stoppingToken);
        if (settings.UpdatePolicy != RuleUpdatePolicy.Periodic)
        {
            return;
        }

        using var timer = new PeriodicTimer(settings.UpdateInterval > TimeSpan.Zero
            ? settings.UpdateInterval
            : TimeSpan.FromHours(6));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCheckAsync(stoppingToken);
        }
    }

    private async Task RunCheckAsync(CancellationToken cancellationToken)
    {
        try
        {
            var report = await readiness.GetReadinessAsync(null, cancellationToken);
            if (report.State is not (
                ManagedReadinessState.Ready or
                ManagedReadinessState.Degraded or
                ManagedReadinessState.RulePublicationRequired))
            {
                logger.LogInformation(
                    "Managed rule update deferred while readiness state is {ReadinessState}.",
                    report.State);
                return;
            }

            var result = await coordinator.CheckForUpdateAsync(cancellationToken);
            logger.LogInformation(
                "Managed rule update status {Status}; active release {ActiveReleaseId}.",
                result.Status,
                result.ActiveReleaseId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Managed rule update check failed; service readiness remains available.");
        }
    }
}
