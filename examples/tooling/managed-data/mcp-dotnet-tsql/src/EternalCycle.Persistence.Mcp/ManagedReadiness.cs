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
    bool SanitizedFileLogConfigured);

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
    string? FailureCode = null);

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
            message = "The Managed service is ready for gameplay.";
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
            snapshot.SanitizedFileLogConfigured);
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

            if (domainCoreReady && !domainTables.Contains("rule_source_configurations"))
            {
                domainSchema = ManagedComponentStatus.Outdated;
            }

            var source = await DetermineRuleSourceAsync(connection, domainCoreReady, domainTables, cancellationToken);
            var publishedCount = 0;
            string? activeReleaseId = null;
            string? activeSourceIdentity = null;
            var activeCompatible = false;
            string? latestUpdateOutcome = null;

            if (domainCoreReady)
            {
                (publishedCount, activeReleaseId, activeSourceIdentity, activeCompatible, latestUpdateOutcome) =
                    await ReadRuleStateAsync(connection, route, cancellationToken);
                if (latestUpdateOutcome is "Failed" or "Degraded")
                {
                    source = ManagedComponentStatus.Unavailable;
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
                !string.IsNullOrWhiteSpace(administration.SanitizedLogFile));
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

    private async Task<(int Count, string? ReleaseId, string? SourceIdentity, bool Compatible, string? UpdateOutcome)>
        ReadRuleStateAsync(
            SqlConnection connection,
            CampaignSchemaRoute route,
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
        return (
            count,
            releaseId,
            sourceIdentity,
            releaseId is not null && repositoryVersion is not null && Compatible(route.RulesetVersion, repositoryVersion),
            updateOutcome);
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
