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

public enum RuleSourceReleaseChannel
{
    Stable,
    Prerelease
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

    public RuleSourceReleaseChannel ReleaseChannel { get; init; } = RuleSourceReleaseChannel.Stable;

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
    DateTimeOffset AcquiredAt,
    RuleSourceReleaseChannel ReleaseChannel = RuleSourceReleaseChannel.Stable,
    string DiscoveryRef = "HEAD",
    int ManifestFormatVersion = 1,
    string CompilerContractVersion = "1",
    string RulesetId = "eternal-cycle-core");

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
    string? FailureReason = null,
    RuleSourceReleaseChannel ReleaseChannel = RuleSourceReleaseChannel.Stable,
    string? DiscoveryRef = null,
    int? ManifestFormatVersion = null,
    string? CompilerContractVersion = null);

public sealed record RulePublicationResult(
    string Status,
    string? CandidateReleaseId,
    string? ActiveReleaseId,
    string? SourceIdentity,
    bool ActiveReleasePreserved,
    string? FailureReason,
    string? ErrorCode = null,
    string? Operation = null,
    string? Stage = null,
    string? CorrelationId = null,
    bool RetrySafe = false,
    bool AdministrativeInterventionRequired = false,
    string? DiagnosticsAvailability = null,
    RuleSourceReleaseChannel? ReleaseChannel = null,
    string? DiscoveryRef = null,
    int? ManifestFormatVersion = null,
    string? CompilerContractVersion = null);

public sealed record RuleUpdateCheck(
    string RulesetId,
    string? SourceIdentity,
    string Outcome,
    string? SanitizedDetail,
    DateTimeOffset CheckedAt);

public interface IRuleSourceProvider
{
    Task<RuleSourceSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);

    Task<RuleSourceSnapshot> GetSnapshotAsync(
        Action<RulePublicationStage>? onStage,
        CancellationToken cancellationToken) =>
        GetSnapshotAsync(cancellationToken);
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

public sealed class ManagedRulePublicationCoordinator
{
    private const string OperationName = "PublishInitialRules";
    private readonly IRuleSourceProvider sourceProvider;
    private readonly IPublishedRuleStore store;
    private readonly ManagedRuleServiceOptions settings;
    private readonly ManagedDiagnosticsOptions diagnosticsSettings;
    private readonly IManagedDiagnosticRecorder diagnostics;
    private readonly ILogger<ManagedRulePublicationCoordinator> logger;

    public ManagedRulePublicationCoordinator(
        IRuleSourceProvider sourceProvider,
        IPublishedRuleStore store,
        IOptions<ManagedRuleServiceOptions> options,
        ILogger<ManagedRulePublicationCoordinator> logger)
        : this(
            sourceProvider,
            store,
            options,
            Options.Create(new ManagedDiagnosticsOptions()),
            NullManagedDiagnosticRecorder.Instance,
            logger)
    {
    }

    public ManagedRulePublicationCoordinator(
        IRuleSourceProvider sourceProvider,
        IPublishedRuleStore store,
        IOptions<ManagedRuleServiceOptions> options,
        IOptions<ManagedDiagnosticsOptions> diagnosticsOptions,
        IManagedDiagnosticRecorder diagnostics,
        ILogger<ManagedRulePublicationCoordinator> logger)
    {
        this.sourceProvider = sourceProvider;
        this.store = store;
        settings = options.Value;
        diagnosticsSettings = diagnosticsOptions.Value;
        this.diagnostics = diagnostics;
        this.logger = logger;
    }

    public async Task<RulePublicationResult> CheckForUpdateAsync(CancellationToken cancellationToken)
    {
        var correlationId = $"OP-{Guid.NewGuid():N}";
        var startedAt = DateTimeOffset.UtcNow;
        var stage = RulePublicationStage.AcquireSource;
        PublishedRuleRelease? active = null;
        RuleSourceSnapshot? snapshot = null;
        PublishedRuleRelease? candidate = null;
        try
        {
            active = await store.GetActiveAsync(settings.RulesetId, cancellationToken);
            snapshot = await sourceProvider.GetSnapshotAsync(value => stage = value, cancellationToken);

            if (active is not null &&
                string.Equals(active.SourceIdentity, snapshot.SourceIdentity, StringComparison.Ordinal))
            {
                stage = RulePublicationStage.RecordUpdateCheck;
                return await CompleteAsync(new RulePublicationResult(
                    "Unchanged",
                    null,
                    active.RuleReleaseId,
                    active.SourceIdentity,
                    true,
                    null,
                    Operation: OperationName,
                    Stage: stage.ToString(),
                    CorrelationId: correlationId,
                    ReleaseChannel: snapshot.ReleaseChannel,
                    DiscoveryRef: snapshot.DiscoveryRef,
                    ManifestFormatVersion: snapshot.ManifestFormatVersion,
                    CompilerContractVersion: snapshot.CompilerContractVersion), "Unchanged", cancellationToken);
            }

            candidate = await store.FindBySourceAsync(
                settings.RulesetId,
                snapshot.SourceIdentity,
                cancellationToken);
            if (candidate is null)
            {
                stage = RulePublicationStage.Compile;
                var releaseId = $"RULE-{Guid.NewGuid():N}";
                var index = RuleCompiler.Compile(snapshot.RepositoryVersion, snapshot.Documents);
                candidate = new PublishedRuleRelease(
                    releaseId,
                    settings.RulesetId,
                    snapshot.ProviderKind,
                    snapshot.SourceIdentity,
                    snapshot.RepositoryVersion,
                    settings.CompilerVersion,
                    RuleReleaseState.Candidate,
                    index,
                    DateTimeOffset.UtcNow,
                    ReleaseChannel: snapshot.ReleaseChannel,
                    DiscoveryRef: snapshot.DiscoveryRef,
                    ManifestFormatVersion: snapshot.ManifestFormatVersion,
                    CompilerContractVersion: snapshot.CompilerContractVersion);

                stage = RulePublicationStage.Stage;
                await store.StageCandidateAsync(candidate, cancellationToken);
            }

            var result = await ResumeCandidateAsync(candidate, active, correlationId, value => stage = value, cancellationToken);
            stage = RulePublicationStage.RecordUpdateCheck;
            var outcome = result.Status switch
            {
                "Activated" => "Activated",
                "Unchanged" => "Unchanged",
                _ => "Candidate"
            };
            return await CompleteAsync(result with { Stage = stage.ToString() }, outcome, cancellationToken);
        }
        catch (Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                stage = RulePublicationStage.Cancelled;
            }

            var failure = DescribeFailure(exception, stage);
            var sourceFailure = exception as RulePublicationException;
            var releaseId = candidate?.RuleReleaseId;
            if (releaseId is not null && stage != RulePublicationStage.Activate)
            {
                await TrySetFailedAsync(releaseId, failure.SafeMessage);
            }

            var receipt = await diagnostics.RecordFailureAsync(
                new ManagedDiagnosticContext(
                    correlationId,
                    OperationName,
                    stage,
                    failure.Code,
                    startedAt,
                    settings.RulesetId,
                    releaseId,
                    snapshot?.SourceIdentity ?? sourceFailure?.SourceIdentity,
                    RetrySafe: failure.RetrySafe,
                    AdministrativeInterventionRequired: failure.AdministrativeInterventionRequired,
                    ReleaseChannel: snapshot?.ReleaseChannel ?? sourceFailure?.ReleaseChannel,
                    DiscoveryRef: snapshot?.DiscoveryRef ?? sourceFailure?.DiscoveryRef,
                    SafeDetail: failure.SafeMessage),
                exception,
                CancellationToken.None);

            logger.LogError(
                "Managed rule publication failed at {Stage} with {ErrorCode}; correlation {CorrelationId}; exception {ExceptionType}.",
                stage,
                failure.Code,
                correlationId,
                exception.GetType().FullName);

            var retainedActive = active is not null;
            var status = retainedActive && stage is
                RulePublicationStage.AcquireSource or
                RulePublicationStage.ResolveRef or
                RulePublicationStage.ReadManifest or
                RulePublicationStage.ReadRuleDocuments
                    ? "SourceUnavailableUsingActive"
                    : stage == RulePublicationStage.Cancelled
                        ? "Cancelled"
                        : "CandidateFailed";
            var result = new RulePublicationResult(
                status,
                releaseId,
                active?.RuleReleaseId,
                snapshot?.SourceIdentity ?? sourceFailure?.SourceIdentity ?? active?.SourceIdentity,
                retainedActive,
                VisibleMessage(exception, failure.SafeMessage),
                failure.Code,
                OperationName,
                stage.ToString(),
                correlationId,
                failure.RetrySafe,
                failure.AdministrativeInterventionRequired,
                receipt.Availability);
            result = result with
            {
                ReleaseChannel = snapshot?.ReleaseChannel ?? sourceFailure?.ReleaseChannel,
                DiscoveryRef = snapshot?.DiscoveryRef ?? sourceFailure?.DiscoveryRef,
                ManifestFormatVersion = snapshot?.ManifestFormatVersion,
                CompilerContractVersion = snapshot?.CompilerContractVersion
            };
            await TryRecordUpdateCheckAsync(result, retainedActive ? "Degraded" : "Failed");
            return result;
        }
    }

    public Task ActivateAsync(string ruleReleaseId, CancellationToken cancellationToken) =>
        store.ActivateAsync(settings.RulesetId, ruleReleaseId, cancellationToken);

    private async Task<RulePublicationResult> ResumeCandidateAsync(
        PublishedRuleRelease candidate,
        PublishedRuleRelease? active,
        string correlationId,
        Action<RulePublicationStage> onStage,
        CancellationToken cancellationToken)
    {
        var state = candidate.State;
        if (state == RuleReleaseState.Active)
        {
            return Success("AlreadyPublished", candidate, candidate.RuleReleaseId, active is not null, correlationId);
        }

        if (state == RuleReleaseState.Failed)
        {
            onStage(RulePublicationStage.Stage);
            await store.SetStateAsync(candidate.RuleReleaseId, RuleReleaseState.Candidate, null, cancellationToken);
            state = RuleReleaseState.Candidate;
        }

        if (state == RuleReleaseState.Candidate)
        {
            onStage(RulePublicationStage.Validate);
            ValidateCandidate(candidate.Index);
            await store.SetStateAsync(candidate.RuleReleaseId, RuleReleaseState.Validated, null, cancellationToken);
            state = RuleReleaseState.Validated;
        }

        if (state == RuleReleaseState.Validated)
        {
            onStage(RulePublicationStage.Publish);
            await store.SetStateAsync(candidate.RuleReleaseId, RuleReleaseState.Published, null, cancellationToken);
            state = RuleReleaseState.Published;
        }

        if (state == RuleReleaseState.Published && settings.ActivationPolicy == RuleActivationPolicy.Automatic)
        {
            onStage(RulePublicationStage.Activate);
            await store.ActivateAsync(settings.RulesetId, candidate.RuleReleaseId, cancellationToken);
            return Success("Activated", candidate, candidate.RuleReleaseId, active is not null, correlationId);
        }

        if (state == RuleReleaseState.Published)
        {
            return Success(
                "AwaitingAdministratorActivation",
                candidate,
                active?.RuleReleaseId,
                active is not null,
                correlationId);
        }

        throw new RulePublicationException(
            "RULE_PUBLICATION_STATE_INVALID",
            RulePublicationStage.Publish,
            $"Rule Release '{candidate.RuleReleaseId}' cannot resume from state '{state}'.",
            retrySafe: false,
            administrativeInterventionRequired: true);
    }

    private static RulePublicationResult Success(
        string status,
        PublishedRuleRelease candidate,
        string? activeReleaseId,
        bool activeReleasePreserved,
        string correlationId) =>
        new(
            status,
            candidate.RuleReleaseId,
            activeReleaseId,
            candidate.SourceIdentity,
            activeReleasePreserved,
            null,
            Operation: OperationName,
            CorrelationId: correlationId,
            ReleaseChannel: candidate.ReleaseChannel,
            DiscoveryRef: candidate.DiscoveryRef,
            ManifestFormatVersion: candidate.ManifestFormatVersion,
            CompilerContractVersion: candidate.CompilerContractVersion);

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

    private async Task TrySetFailedAsync(string releaseId, string failureReason)
    {
        using var bounded = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            await store.SetStateAsync(releaseId, RuleReleaseState.Failed, failureReason, bounded.Token);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Could not persist failed state for rule release {ReleaseId}; exception {ExceptionType}.",
                releaseId,
                exception.GetType().FullName);
        }
    }

    private async Task TryRecordUpdateCheckAsync(RulePublicationResult result, string outcome)
    {
        using var bounded = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            await store.RecordUpdateCheckAsync(
                new RuleUpdateCheck(
                    settings.RulesetId,
                    result.SourceIdentity,
                    outcome,
                    result.FailureReason,
                    DateTimeOffset.UtcNow),
                bounded.Token);
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Could not persist rule update outcome for correlation {CorrelationId}; exception {ExceptionType}.",
                result.CorrelationId,
                exception.GetType().FullName);
        }
    }

    private FailureDescriptor DescribeFailure(Exception exception, RulePublicationStage stage)
    {
        if (exception is RulePublicationException publication)
        {
            return new(
                publication.Code,
                publication.SafeMessage,
                publication.RetrySafe,
                publication.AdministrativeInterventionRequired);
        }

        if (exception is OperationCanceledException)
        {
            return new(
                "RULE_PUBLICATION_CANCELLED",
                "Rule publication was cancelled before completion. A retry resumes from any durable completed stage.",
                true,
                false);
        }

        return stage switch
        {
            RulePublicationStage.AcquireSource => new("RULE_SOURCE_ACQUISITION_FAILED", "Rule Source acquisition failed.", true, true),
            RulePublicationStage.ResolveRef => new("RULE_SOURCE_REF_RESOLUTION_FAILED", "The configured Rule Source ref could not be resolved.", true, true),
            RulePublicationStage.ReadManifest or RulePublicationStage.ReadRuleDocuments => new("RULE_SOURCE_READ_FAILED", "The immutable Rule Source content could not be read.", true, true),
            RulePublicationStage.Compile => new("RULE_COMPILATION_FAILED", "Rule compilation failed for the immutable source revision.", false, true),
            RulePublicationStage.Validate => new("RULE_VALIDATION_FAILED", "The compiled Rule Release failed validation.", false, true),
            RulePublicationStage.Stage => new("RULE_STORE_STAGE_FAILED", "The Rule Release candidate could not be staged.", true, false),
            RulePublicationStage.Publish => new("RULE_PUBLICATION_FAILED", "The validated Rule Release could not be published.", true, false),
            RulePublicationStage.Activate => new("RULE_ACTIVATION_FAILED", "The published Rule Release could not be activated.", true, false),
            RulePublicationStage.RecordUpdateCheck => new("RULE_UPDATE_CHECK_RECORD_FAILED", "The Rule Release outcome could not be recorded.", true, false),
            _ => new("RULE_PUBLICATION_FAILED", "Rule publication failed.", true, false)
        };
    }

    private string VisibleMessage(Exception exception, string safeMessage)
    {
        if (!diagnosticsSettings.VerboseErrors)
        {
            return safeMessage;
        }

        var entries = new List<string>();
        for (var current = exception; current is not null; current = current.InnerException)
        {
            entries.Add($"{current.GetType().FullName}: {current.Message}");
        }

        var detail = DiagnosticRedactor.Redact(string.Join(" -> ", entries));
        return $"{safeMessage} Authorized diagnostic: {detail}";
    }

    private async Task<RulePublicationResult> CompleteAsync(
        RulePublicationResult result,
        string outcome,
        CancellationToken cancellationToken)
    {
        try
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
        catch (Exception exception)
        {
            var correlationId = result.CorrelationId ?? $"OP-{Guid.NewGuid():N}";
            var receipt = await diagnostics.RecordFailureAsync(
                new ManagedDiagnosticContext(
                    correlationId,
                    OperationName,
                    RulePublicationStage.RecordUpdateCheck,
                    "RULE_UPDATE_CHECK_RECORD_FAILED",
                    DateTimeOffset.UtcNow,
                    settings.RulesetId,
                    result.CandidateReleaseId,
                    result.SourceIdentity,
                    Outcome: "DiagnosticWriteFailed"),
                exception,
                CancellationToken.None);
            logger.LogWarning(
                "Rule publication completed but its update-check record failed; correlation {CorrelationId}; exception {ExceptionType}.",
                correlationId,
                exception.GetType().FullName);
            return result with
            {
                Status = "UpdateCheckFailed",
                FailureReason = "The Rule Release reached its requested state, but the update-check record failed.",
                ErrorCode = "RULE_UPDATE_CHECK_RECORD_FAILED",
                Stage = RulePublicationStage.RecordUpdateCheck.ToString(),
                RetrySafe = true,
                AdministrativeInterventionRequired = false,
                DiagnosticsAvailability = receipt.Availability
            };
        }
    }

    private sealed record FailureDescriptor(
        string Code,
        string SafeMessage,
        bool RetrySafe,
        bool AdministrativeInterventionRequired);
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
