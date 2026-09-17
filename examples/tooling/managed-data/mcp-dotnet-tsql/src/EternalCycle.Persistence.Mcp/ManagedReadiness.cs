using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public enum ManagedReadinessState
{
    Ready,
    SetupRequired,
    MigrationRequired,
    RuleSourceRequired,
    RulePublicationRequired,
    RuleActivationRequired,
    RulePreparationPending,
    CampaignRequired,
    Degraded,
    Error
}

public enum ManagedComponentStatus
{
    Ready,
    Missing,
    Outdated,
    NotConfigured,
    Unavailable,
    Empty,
    Inactive,
    Incompatible,
    NotRequested,
    Degraded,
    Error
}

public sealed record ReadinessComponent(
    ManagedComponentStatus Status,
    string Detail);

public sealed record ManagedCausalDiagnostic(
    string ErrorCode,
    string Stage,
    string CorrelationId,
    bool? RetrySafe,
    bool? AdministrativeInterventionRequired,
    string? SafeDetail,
    RuleSourceReleaseChannel? ReleaseChannel,
    string? DiscoveryRef,
    string? SourceIdentity,
    DateTimeOffset RecordedAt);

public sealed record ManagedReadinessReport(
    ManagedReadinessState State,
    bool GameplayReady,
    ReadinessComponent Transport,
    ReadinessComponent PersistenceConnection,
    ReadinessComponent CampaignSchema,
    ReadinessComponent RuleDomainSchema,
    ReadinessComponent RuleSource,
    ReadinessComponent RulePublication,
    ReadinessComponent RuleActivation,
    ReadinessComponent Campaign,
    string? RuleSourceRevision,
    string? ActiveRuleReleaseId,
    string? ErrorCode,
    string Message,
    bool AdministrativeActionRequired,
    bool SanitizedFileLogConfigured,
    ManagedCausalDiagnostic? LatestRelevantFailure = null,
    bool ServiceReady = true,
    bool PersistenceReady = false,
    bool RuleKernelReady = false,
    bool CampaignBootstrapReady = false,
    bool FullRulesetReady = false);

public sealed record ManagedInfrastructureSnapshot(
    ManagedComponentStatus PersistenceConnection,
    ManagedComponentStatus CampaignSchema,
    ManagedComponentStatus RuleDomainSchema,
    ManagedComponentStatus RuleSource,
    int PublishedReleaseCount,
    string? ActiveRuleReleaseId,
    string? ActiveRuleSourceIdentity,
    bool ActiveRuleReleaseCompatible,
    ManagedComponentStatus Campaign,
    string? LatestUpdateOutcome,
    bool SanitizedFileLogConfigured,
    string? FailureCode = null,
    ManagedCausalDiagnostic? LatestRelevantFailure = null,
    bool? RuleKernelReady = null,
    bool? CampaignBootstrapReady = null,
    bool? FullRulesetReady = null);

public interface IManagedInfrastructureInspector
{
    Task<ManagedInfrastructureSnapshot> InspectAsync(
        string? campaignId,
        CancellationToken cancellationToken);
}

public interface IManagedReadinessService
{
    Task<ManagedReadinessReport> GetReadinessAsync(
        string? campaignId,
        CancellationToken cancellationToken);
}

public sealed class ManagedReadinessService(IManagedInfrastructureInspector inspector) : IManagedReadinessService
{
    public async Task<ManagedReadinessReport> GetReadinessAsync(
        string? campaignId,
        CancellationToken cancellationToken) =>
        ManagedReadinessEvaluator.Evaluate(
            await inspector.InspectAsync(campaignId, cancellationToken),
            campaignId is not null);
}

public static class ManagedReadinessEvaluator
{
    public static ManagedReadinessReport Evaluate(
        ManagedInfrastructureSnapshot snapshot,
        bool campaignRequested)
    {
        var campaignPublication = snapshot.PublishedReleaseCount > 0
            ? new ReadinessComponent(ManagedComponentStatus.Ready, "At least one validated Rule Release is published.")
            : new ReadinessComponent(ManagedComponentStatus.Empty, "No validated Rule Release has been published.");
        var activation = snapshot.ActiveRuleReleaseId is null
            ? new ReadinessComponent(ManagedComponentStatus.Inactive, "No compatible Rule Release is active.")
            : snapshot.ActiveRuleReleaseCompatible
                ? new ReadinessComponent(ManagedComponentStatus.Ready, "An active compatible Rule Release is available.")
                : new ReadinessComponent(ManagedComponentStatus.Incompatible, "The active Rule Release is incompatible with the configured RuleSet version.");
        var persistenceReady = snapshot.PersistenceConnection == ManagedComponentStatus.Ready &&
            snapshot.CampaignSchema == ManagedComponentStatus.Ready &&
            snapshot.RuleDomainSchema == ManagedComponentStatus.Ready;
        var ruleKernelReady = snapshot.RuleKernelReady ??
            (snapshot.ActiveRuleReleaseId is not null && snapshot.ActiveRuleReleaseCompatible);
        var campaignBootstrapReady = snapshot.CampaignBootstrapReady ?? ruleKernelReady;
        var fullRulesetReady = snapshot.FullRulesetReady ?? campaignBootstrapReady;

        ManagedReadinessState state;
        string? errorCode;
        string message;
        bool gameplayReady;
        bool administrativeActionRequired;

        if (snapshot.PersistenceConnection is not ManagedComponentStatus.Ready)
        {
            state = ManagedReadinessState.Error;
            errorCode = snapshot.FailureCode ?? "PERSISTENCE_UNAVAILABLE";
            message = "The configured persistence service cannot be reached. Verify authorized service logs and connection configuration.";
            gameplayReady = false;
            administrativeActionRequired = true;
        }
        else if (snapshot.CampaignSchema is ManagedComponentStatus.Missing ||
                 snapshot.RuleDomainSchema is ManagedComponentStatus.Missing)
        {
            state = ManagedReadinessState.SetupRequired;
            errorCode = snapshot.RuleDomainSchema is ManagedComponentStatus.Missing
                ? "RULE_SCHEMA_MISSING"
                : "CAMPAIGN_SCHEMA_MISSING";
            message = "Eternal Cycle-owned database structures have not been initialized. An authorized administrator may preview and approve bootstrap.";
            gameplayReady = false;
            administrativeActionRequired = true;
        }
        else if (snapshot.CampaignSchema is ManagedComponentStatus.Outdated ||
                 snapshot.RuleDomainSchema is ManagedComponentStatus.Outdated)
        {
            state = ManagedReadinessState.MigrationRequired;
            errorCode = "MIGRATION_REQUIRED";
            message = "Eternal Cycle-owned database structures require a supported migration before gameplay can continue.";
            gameplayReady = false;
            administrativeActionRequired = true;
        }
        else if (snapshot.RuleSource is ManagedComponentStatus.NotConfigured)
        {
            state = ManagedReadinessState.RuleSourceRequired;
            errorCode = "RULE_SOURCE_NOT_CONFIGURED";
            message = "No Rule Source is selected. An administrator may select the official Eternal Cycle repository or another compatible source.";
            gameplayReady = false;
            administrativeActionRequired = true;
        }
        else if (snapshot.PublishedReleaseCount == 0)
        {
            state = snapshot.RuleSource is ManagedComponentStatus.Unavailable
                ? ManagedReadinessState.Error
                : ManagedReadinessState.RulePublicationRequired;
            errorCode = snapshot.RuleSource is ManagedComponentStatus.Unavailable
                ? "RULE_SOURCE_UNAVAILABLE"
                : "NO_PUBLISHED_RULE_RELEASE";
            message = snapshot.RuleSource is ManagedComponentStatus.Unavailable
                ? "The configured Rule Source is unavailable and no prior valid Rule Release can be used."
                : "The selected Rule Source has not yet produced a validated published Rule Release.";
            gameplayReady = false;
            administrativeActionRequired = true;
        }
        else if (snapshot.ActiveRuleReleaseId is null)
        {
            state = ManagedReadinessState.RuleActivationRequired;
            errorCode = "NO_ACTIVE_RULE_RELEASE";
            message = "A Rule Release is published but none is active for the configured RuleSet.";
            gameplayReady = false;
            administrativeActionRequired = true;
        }
        else if (!snapshot.ActiveRuleReleaseCompatible)
        {
            state = ManagedReadinessState.Error;
            errorCode = "RULESET_INCOMPATIBLE";
            message = "The active Rule Release is incompatible with the configured campaign RuleSet version.";
            gameplayReady = false;
            administrativeActionRequired = true;
        }
        else if (!ruleKernelReady || !campaignBootstrapReady)
        {
            state = ManagedReadinessState.RulePreparationPending;
            errorCode = "RULE_CLOSURE_PENDING";
            message = "The minimum authoritative gameplay rule closure is still being prepared. Query the active Managed Operation rather than guessing missing rules.";
            gameplayReady = false;
            administrativeActionRequired = false;
        }
        else if (campaignRequested && snapshot.Campaign is ManagedComponentStatus.Missing)
        {
            state = ManagedReadinessState.CampaignRequired;
            errorCode = "CAMPAIGN_NOT_FOUND";
            message = "The requested campaign does not exist. List existing campaigns or use the authorized new-campaign path.";
            gameplayReady = false;
            administrativeActionRequired = true;
        }
        else if (snapshot.RuleSource is ManagedComponentStatus.Unavailable)
        {
            state = ManagedReadinessState.Degraded;
            errorCode = "RULE_SOURCE_UNAVAILABLE";
            message = "Gameplay may continue using the active validated Rule Release, but update acquisition is degraded.";
            gameplayReady = true;
            administrativeActionRequired = false;
        }
        else
        {
            state = ManagedReadinessState.Ready;
            errorCode = null;
            message = fullRulesetReady
                ? "The Managed service and full selected RuleSet are ready for gameplay."
                : "The minimum authoritative closure is ready for gameplay while remaining rules continue preparation.";
            gameplayReady = true;
            administrativeActionRequired = false;
        }

        return new ManagedReadinessReport(
            state,
            gameplayReady,
            new ReadinessComponent(ManagedComponentStatus.Ready, "Managed service transport responded."),
            Component(snapshot.PersistenceConnection, "Persistence connection"),
            Component(snapshot.CampaignSchema, "Campaign persistence schema"),
            Component(snapshot.RuleDomainSchema, "Rule Domain schema"),
            Component(snapshot.RuleSource, "Rule Source"),
            campaignPublication,
            activation,
            Component(campaignRequested ? snapshot.Campaign : ManagedComponentStatus.NotRequested, "Campaign"),
            snapshot.ActiveRuleSourceIdentity,
            snapshot.ActiveRuleReleaseId,
            errorCode,
            message,
            administrativeActionRequired,
            snapshot.SanitizedFileLogConfigured,
            snapshot.LatestRelevantFailure,
            ServiceReady: true,
            PersistenceReady: persistenceReady,
            RuleKernelReady: ruleKernelReady,
            CampaignBootstrapReady: campaignBootstrapReady,
            FullRulesetReady: fullRulesetReady);
    }

    private static ReadinessComponent Component(ManagedComponentStatus status, string name) =>
        new(status, $"{name}: {status}.");
}

public sealed class SqlServerManagedInfrastructureInspector(
    IOptions<SqlServerPersistenceOptions> persistenceOptions,
    IOptions<ManagedRuleServiceOptions> ruleOptions,
    IOptions<ManagedAdministrationOptions> administrationOptions,
    ICampaignSchemaResolver schemaResolver) : IManagedInfrastructureInspector
{
    internal static readonly string[] CampaignTables =
    [
        "campaigns",
        "save_transactions",
        "canonical_record_versions",
        "record_references",
        "validation_runs",
        "persistence_receipts",
        "recovery_points"
    ];

    internal static readonly string[] RuleDomainTables =
    [
        "rulesets",
        "rule_releases",
        "rule_chunks",
        "rule_chunk_selectors",
        "rule_dependencies",
        "active_rule_releases",
        "rule_update_checks"
    ];

    private readonly SqlServerPersistenceOptions persistence = persistenceOptions.Value;
    private readonly ManagedRuleServiceOptions rules = ruleOptions.Value;
    private readonly ManagedAdministrationOptions administration = administrationOptions.Value;

    public async Task<ManagedInfrastructureSnapshot> InspectAsync(
        string? campaignId,
        CancellationToken cancellationToken)
    {
        CampaignSchemaRoute route;
        try
        {
            route = schemaResolver.Resolve(campaignId ?? "readiness-default");
        }
        catch (Exception)
        {
            return Failure("ROUTING_CONFIGURATION_INVALID");
        }

        try
        {
            await using var connection = new SqlConnection(persistence.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            var campaignTables = await ReadTablesAsync(connection, route.SchemaName, cancellationToken);
            var domainTables = await ReadTablesAsync(connection, persistence.DomainSchema, cancellationToken);
            var campaignCoreReady = CampaignTables.All(campaignTables.Contains);
            var domainCoreReady = RuleDomainTables.All(domainTables.Contains);
            var campaignSchema = Classify(campaignTables, CampaignTables);
            var domainSchema = Classify(domainTables, RuleDomainTables);

            if (campaignCoreReady && !await CampaignDirectoryColumnsReadyAsync(connection, route.SchemaName, cancellationToken))
            {
                campaignSchema = ManagedComponentStatus.Outdated;
            }

            var ruleSourceCompatibilityReady = domainCoreReady &&
                domainTables.Contains("rule_source_configurations") &&
                domainTables.Contains("managed_operation_diagnostics") &&
                await RuleSourceCompatibilityColumnsReadyAsync(connection, cancellationToken);
            var durableOperationsReady = domainCoreReady &&
                domainTables.Contains("managed_operations") &&
                domainTables.Contains("rule_source_preparation") &&
                await DurableManagedOperationColumnsReadyAsync(connection, cancellationToken);
            if (domainCoreReady && (!ruleSourceCompatibilityReady || !durableOperationsReady))
            {
                domainSchema = ManagedComponentStatus.Outdated;
            }

            var source = await DetermineRuleSourceAsync(connection, domainCoreReady, domainTables, cancellationToken);
            var publishedCount = 0;
            string? activeReleaseId = null;
            string? activeSourceIdentity = null;
            var activeCompatible = false;
            bool? ruleKernelReady = null;
            bool? campaignBootstrapReady = null;
            bool? fullRulesetReady = null;
            string? latestUpdateOutcome = null;
            ManagedCausalDiagnostic? latestRelevantFailure = null;

            if (domainCoreReady)
            {
                (publishedCount, activeReleaseId, activeSourceIdentity, activeCompatible, latestUpdateOutcome,
                    ruleKernelReady, campaignBootstrapReady, fullRulesetReady) =
                    await ReadRuleStateAsync(connection, route, durableOperationsReady, cancellationToken);
                if (latestUpdateOutcome is "Failed" or "Degraded")
                {
                    source = ManagedComponentStatus.Unavailable;
                    if (ruleSourceCompatibilityReady)
                    {
                        latestRelevantFailure = await ReadLatestRelevantFailureAsync(connection, cancellationToken);
                    }
                }
            }

            var campaign = ManagedComponentStatus.NotRequested;
            if (campaignId is not null && campaignCoreReady)
            {
                campaign = await CampaignExistsAsync(connection, route.SchemaName, campaignId, cancellationToken)
                    ? ManagedComponentStatus.Ready
                    : ManagedComponentStatus.Missing;
            }

            return new ManagedInfrastructureSnapshot(
                ManagedComponentStatus.Ready,
                campaignSchema,
                domainSchema,
                source,
                publishedCount,
                activeReleaseId,
                activeSourceIdentity,
                activeCompatible,
                campaign,
                latestUpdateOutcome,
                !string.IsNullOrWhiteSpace(administration.SanitizedLogFile),
                LatestRelevantFailure: latestRelevantFailure,
                RuleKernelReady: ruleKernelReady,
                CampaignBootstrapReady: campaignBootstrapReady,
                FullRulesetReady: fullRulesetReady);
        }
        catch (SqlException)
        {
            return Failure("PERSISTENCE_UNAVAILABLE");
        }
        catch (InvalidOperationException)
        {
            return Failure("PERSISTENCE_INSPECTION_FAILED");
        }
    }

    private ManagedInfrastructureSnapshot Failure(string code) =>
        new(
            ManagedComponentStatus.Unavailable,
            ManagedComponentStatus.Error,
            ManagedComponentStatus.Error,
            ManagedComponentStatus.Error,
            0,
            null,
            null,
            false,
            ManagedComponentStatus.Error,
            null,
            !string.IsNullOrWhiteSpace(administration.SanitizedLogFile),
            code);

    private async Task<bool> RuleSourceCompatibilityColumnsReadyAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("""
            SELECT COUNT(*)
            FROM sys.columns AS columns
            INNER JOIN sys.tables AS tables ON tables.object_id = columns.object_id
            INNER JOIN sys.schemas AS schemas ON schemas.schema_id = tables.schema_id
            WHERE schemas.name = @schema_name
              AND (
                    (tables.name = N'rule_source_configurations' AND columns.name = N'release_channel')
                 OR (tables.name = N'rule_releases' AND columns.name IN (
                        N'release_channel', N'discovery_ref', N'manifest_format_version', N'compiler_contract_version'))
                 OR (tables.name = N'managed_operation_diagnostics' AND columns.name IN (
                        N'safe_detail', N'retry_safe', N'administrative_intervention_required',
                        N'source_channel', N'discovery_ref'))
              );
            """, connection)
        {
            CommandTimeout = persistence.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@schema_name", persistence.DomainSchema);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 10;
    }

    private async Task<bool> DurableManagedOperationColumnsReadyAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("""
            SELECT COUNT(*)
            FROM sys.columns AS columns
            INNER JOIN sys.tables AS tables ON tables.object_id = columns.object_id
            INNER JOIN sys.schemas AS schemas ON schemas.schema_id = tables.schema_id
            WHERE schemas.name = @schema_name
              AND (
                    (tables.name = N'managed_operations' AND columns.name IN (
                        N'operation_id', N'operation_state', N'current_stage', N'deduplication_key',
                        N'user_approval_required', N'administrative_intervention_required',
                        N'result_rule_release_id'))
                   OR (tables.name = N'rule_source_preparation' AND columns.name IN (
                          N'rule_release_id', N'rule_source_id', N'preparation_tier',
                          N'preparation_state', N'base_priority', N'priority_boost'))
                 OR (tables.name = N'rule_releases' AND columns.name IN (
                        N'base_release', N'discovery_tag', N'commits_since_base', N'display_version'))
              );
            """, connection)
        {
            CommandTimeout = persistence.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@schema_name", persistence.DomainSchema);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 17;
    }

    private async Task<ManagedCausalDiagnostic?> ReadLatestRelevantFailureAsync(
        SqlConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = DomainCommand(connection, """
            SELECT TOP (1)
                error_code,
                operation_stage,
                correlation_id,
                retry_safe,
                administrative_intervention_required,
                safe_detail,
                source_channel,
                discovery_ref,
                source_identity,
                recorded_at
            FROM {{schema}}.managed_operation_diagnostics
            WHERE ruleset_id = @ruleset_id
              AND operation_name = N'PublishInitialRules'
            ORDER BY recorded_at DESC, diagnostic_id DESC;
            """);
        command.Parameters.AddWithValue("@ruleset_id", rules.RulesetId);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        RuleSourceReleaseChannel? channel = null;
        if (!reader.IsDBNull(6) &&
            Enum.TryParse<RuleSourceReleaseChannel>(reader.GetString(6), ignoreCase: true, out var parsedChannel))
        {
            channel = parsedChannel;
        }

        return new ManagedCausalDiagnostic(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetBoolean(3),
            reader.IsDBNull(4) ? null : reader.GetBoolean(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            channel,
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.GetDateTimeOffset(9));
    }

    private static ManagedComponentStatus Classify(
        ISet<string> actual,
        IReadOnlyList<string> expected)
    {
        var count = expected.Count(actual.Contains);
        return count switch
        {
            0 => ManagedComponentStatus.Missing,
            _ when count == expected.Count => ManagedComponentStatus.Ready,
            _ => ManagedComponentStatus.Outdated
        };
    }

    private async Task<ManagedComponentStatus> DetermineRuleSourceAsync(
        SqlConnection connection,
        bool domainCoreReady,
        ISet<string> domainTables,
        CancellationToken cancellationToken)
    {
        if (domainCoreReady && domainTables.Contains("rule_source_configurations"))
        {
            await using var command = DomainCommand(connection, """
                SELECT TOP (1) provider_kind, source_location
                FROM {{schema}}.rule_source_configurations
                WHERE ruleset_id = @ruleset_id;
                """);
            command.Parameters.AddWithValue("@ruleset_id", rules.RulesetId);
            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                var sourceLocation = reader.GetString(1);
                return IsRemoteSource(sourceLocation) || Directory.Exists(sourceLocation)
                    ? ManagedComponentStatus.Ready
                    : ManagedComponentStatus.Unavailable;
            }
        }

        if (!string.IsNullOrWhiteSpace(rules.GitSource.RepositoryRoot))
        {
            return Directory.Exists(rules.GitSource.RepositoryRoot)
                ? ManagedComponentStatus.Ready
                : ManagedComponentStatus.Unavailable;
        }

        return ManagedComponentStatus.NotConfigured;
    }

    private async Task<(
        int Count,
        string? ReleaseId,
        string? SourceIdentity,
        bool Compatible,
        string? UpdateOutcome,
        bool? RuleKernelReady,
        bool? CampaignBootstrapReady,
        bool? FullRulesetReady)>
        ReadRuleStateAsync(
            SqlConnection connection,
            CampaignSchemaRoute route,
            bool preparationAvailable,
            CancellationToken cancellationToken)
    {
        await using var command = DomainCommand(connection, """
            SELECT
                (SELECT COUNT(*) FROM {{schema}}.rule_releases
                 WHERE ruleset_id = @ruleset_id AND release_state IN (N'Published', N'Active')),
                active.rule_release_id,
                releases.source_identity,
                releases.repository_version,
                (SELECT TOP (1) outcome FROM {{schema}}.rule_update_checks
                 WHERE ruleset_id = @ruleset_id ORDER BY checked_at DESC, update_check_id DESC)
            FROM (SELECT 1 AS singleton) AS seed
            LEFT JOIN {{schema}}.active_rule_releases AS active
                ON active.ruleset_id = @ruleset_id
            LEFT JOIN {{schema}}.rule_releases AS releases
                ON releases.rule_release_id = active.rule_release_id;
            """);
        command.Parameters.AddWithValue("@ruleset_id", route.RulesetId);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        _ = await reader.ReadAsync(cancellationToken);
        var count = reader.GetInt32(0);
        var releaseId = reader.IsDBNull(1) ? null : reader.GetString(1);
        var sourceIdentity = reader.IsDBNull(2) ? null : reader.GetString(2);
        var repositoryVersion = reader.IsDBNull(3) ? null : reader.GetString(3);
        var updateOutcome = reader.IsDBNull(4) ? null : reader.GetString(4);
        bool? ruleKernelReady = null;
        bool? campaignBootstrapReady = null;
        bool? fullRulesetReady = null;
        if (preparationAvailable && releaseId is not null)
        {
            await reader.DisposeAsync();
            await using var preparation = DomainCommand(connection, """
                SELECT
                    SUM(CASE WHEN preparation_tier = N'RuntimeKernel' THEN 1 ELSE 0 END),
                    SUM(CASE WHEN preparation_tier = N'RuntimeKernel' AND preparation_state = N'Ready' THEN 1 ELSE 0 END),
                    SUM(CASE WHEN preparation_tier IN (N'RuntimeKernel', N'CampaignBootstrap', N'ImmediateGameplayCore') THEN 1 ELSE 0 END),
                    SUM(CASE WHEN preparation_tier IN (N'RuntimeKernel', N'CampaignBootstrap', N'ImmediateGameplayCore') AND preparation_state = N'Ready' THEN 1 ELSE 0 END),
                    COUNT(*),
                    SUM(CASE WHEN preparation_state = N'Ready' THEN 1 ELSE 0 END)
                FROM {{schema}}.rule_source_preparation
                WHERE rule_release_id = @rule_release_id;
                """);
            preparation.Parameters.AddWithValue("@rule_release_id", releaseId);
            await using var preparationReader = await preparation.ExecuteReaderAsync(
                CommandBehavior.SingleRow,
                cancellationToken);
            _ = await preparationReader.ReadAsync(cancellationToken);
            var kernelCount = preparationReader.IsDBNull(0) ? 0 : preparationReader.GetInt32(0);
            var kernelReadyCount = preparationReader.IsDBNull(1) ? 0 : preparationReader.GetInt32(1);
            var bootstrapCount = preparationReader.IsDBNull(2) ? 0 : preparationReader.GetInt32(2);
            var bootstrapReadyCount = preparationReader.IsDBNull(3) ? 0 : preparationReader.GetInt32(3);
            var totalCount = preparationReader.GetInt32(4);
            var totalReadyCount = preparationReader.IsDBNull(5) ? 0 : preparationReader.GetInt32(5);
            ruleKernelReady = kernelCount > 0 && kernelCount == kernelReadyCount;
            campaignBootstrapReady = ruleKernelReady.Value && bootstrapCount == bootstrapReadyCount;
            fullRulesetReady = totalCount > 0 && totalCount == totalReadyCount;
        }

        return (
            count,
            releaseId,
            sourceIdentity,
            releaseId is not null && repositoryVersion is not null && Compatible(route.RulesetVersion, repositoryVersion),
            updateOutcome,
            ruleKernelReady,
            campaignBootstrapReady,
            fullRulesetReady);
    }

    private async Task<HashSet<string>> ReadTablesAsync(
        SqlConnection connection,
        string schemaName,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("""
            SELECT tables.name
            FROM sys.tables AS tables
            INNER JOIN sys.schemas AS schemas ON schemas.schema_id = tables.schema_id
            WHERE schemas.name = @schema_name;
            """, connection)
        {
            CommandTimeout = persistence.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@schema_name", schemaName);
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    private async Task<bool> CampaignDirectoryColumnsReadyAsync(
        SqlConnection connection,
        string schemaName,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("""
            SELECT columns.name
            FROM sys.columns AS columns
            INNER JOIN sys.tables AS tables ON tables.object_id = columns.object_id
            INNER JOIN sys.schemas AS schemas ON schemas.schema_id = tables.schema_id
            WHERE schemas.name = @schema_name
              AND tables.name = N'campaigns'
              AND columns.name IN (N'display_name', N'description');
            """, connection)
        {
            CommandTimeout = persistence.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@schema_name", schemaName);
        var count = 0;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            count++;
        }

        return count == 2;
    }

    private async Task<bool> CampaignExistsAsync(
        SqlConnection connection,
        string schemaName,
        string campaignId,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(
            SqlServerSchemaIdentifier.Bind(
                "SELECT COUNT(*) FROM {{schema}}.campaigns WHERE campaign_id = @campaign_id;",
                schemaName),
            connection)
        {
            CommandTimeout = persistence.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@campaign_id", campaignId);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    private SqlCommand DomainCommand(SqlConnection connection, string text) =>
        new(SqlServerSchemaIdentifier.Bind(text, persistence.DomainSchema), connection)
        {
            CommandTimeout = persistence.CommandTimeoutSeconds
        };

    private static bool IsRemoteSource(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        uri.Scheme is "https" or "http" or "ssh" or "git" or "file";

    private static bool Compatible(string configured, string published)
    {
        static string Major(string value) => value
            .Split('+', 2, StringSplitOptions.TrimEntries)[0]
            .Split('-', 2, StringSplitOptions.TrimEntries)[0]
            .Split('.', 2, StringSplitOptions.TrimEntries)[0];
        return string.Equals(Major(configured), Major(published), StringComparison.OrdinalIgnoreCase);
    }
}
