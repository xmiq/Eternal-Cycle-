using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public sealed class ManagedAdministrationOptions
{
    public bool Enabled { get; init; }

    public string ApprovalPhrase { get; init; } = "INITIALIZE ETERNAL CYCLE";

    public bool RequireOperatorConfirmation { get; init; }

    public string DistributionMetadataFile { get; init; } = "distribution-metadata.json";

    public string ManagedRuleCacheDirectory { get; init; } = string.Empty;

    public string? SanitizedLogFile { get; init; }
}

public sealed record ManagedOperationResult<T>(
    bool Success,
    string Code,
    string Message,
    T? Data,
    string? Operation = null,
    string? Stage = null,
    string? CorrelationId = null,
    bool RetrySafe = false,
    bool AdministrativeInterventionRequired = false,
    string? DiagnosticsAvailability = null,
    bool UserApprovalRequired = false);

public sealed record BootstrapRequest(
    string? CampaignId,
    bool UserApproved,
    string? OperatorConfirmation = null);

public sealed record BootstrapPlan(
    bool ChangesRequired,
    IReadOnlyList<string> MigrationIds,
    IReadOnlyList<string> OwnedScopes,
    string AuthorizationGuidance,
    string Summary);

public sealed record BootstrapExecution(
    IReadOnlyList<string> AppliedMigrationIds,
    bool IdempotentNoOp,
    ManagedReadinessReport Readiness);

public sealed record RuleSourceSelectionRequest(
    bool UseOfficialDefault,
    string? SourceLocation,
    string? RequestedRef,
    string? ManifestPath,
    bool UserApproved,
    string? OperatorConfirmation = null,
    RuleSourceReleaseChannel? ReleaseChannel = null);

public sealed record RuleSourceSelection(
    string RulesetId,
    string ProviderKind,
    string RequestedRef,
    string ManifestPath,
    bool IsOfficial,
    long ConfigurationRevision,
    DateTimeOffset ConfiguredAt,
    RuleSourceReleaseChannel ReleaseChannel = RuleSourceReleaseChannel.Stable);

public sealed record InitialRulePublicationRequest(
    bool UserApproved,
    string? OperatorConfirmation = null);

public sealed record CampaignDescriptor(
    string CampaignId,
    string DisplayName,
    string? Description,
    string WorldModelId,
    string DataNamespaceId);

public sealed record CreateCampaignRequest(
    string DisplayName,
    string? Description,
    bool UserApproved,
    string? OperatorConfirmation = null);

public sealed record CampaignResolution(
    string Status,
    CampaignDescriptor? SelectedCampaign,
    IReadOnlyList<CampaignDescriptor> Choices,
    string Message);

public static class CampaignDiscovery
{
    public static CampaignResolution ResolveResume(IReadOnlyList<CampaignDescriptor> campaigns) =>
        campaigns.Count switch
        {
            0 => new(
                "CAMPAIGN_REQUIRED",
                null,
                [],
                "No campaign exists. Offer the permission-gated new-campaign path."),
            1 => new(
                "CAMPAIGN_SELECTED",
                campaigns[0],
                [],
                "The only available campaign may be resumed without asking for its internal ID."),
            _ => new(
                "CAMPAIGN_SELECTION_REQUIRED",
                null,
                campaigns,
                "Present meaningful campaign names and descriptions while retaining stable IDs internally.")
        };
}

public sealed record RuleSourceConfiguration(
    string RulesetId,
    string ProviderKind,
    string SourceLocation,
    string RequestedRef,
    string ManifestPath,
    bool IsOfficial,
    long ConfigurationRevision,
    DateTimeOffset ConfiguredAt,
    RuleSourceReleaseChannel ReleaseChannel = RuleSourceReleaseChannel.Stable);

public interface IRuleSourceConfigurationStore
{
    Task<RuleSourceConfiguration?> GetAsync(string rulesetId, CancellationToken cancellationToken);

    Task<RuleSourceConfiguration> SaveAsync(
        RuleSourceConfiguration configuration,
        CancellationToken cancellationToken);
}

public interface ISchemaBootstrapExecutor
{
    Task<BootstrapPlan> PlanAsync(string? campaignId, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ExecuteAsync(string? campaignId, CancellationToken cancellationToken);
}

public interface ICampaignDirectoryService
{
    Task<IReadOnlyList<CampaignDescriptor>> ListAsync(CancellationToken cancellationToken);

    Task<CampaignDescriptor> CreateAsync(
        string displayName,
        string? description,
        CancellationToken cancellationToken);
}

public interface IManagedAdministrationService
{
    Task<BootstrapPlan> GetBootstrapPlanAsync(string? campaignId, CancellationToken cancellationToken);

    Task<ManagedOperationResult<BootstrapExecution>> BootstrapAsync(
        BootstrapRequest request,
        CancellationToken cancellationToken);

    Task<ManagedOperationResult<RuleSourceSelection>> ConfigureRuleSourceAsync(
        RuleSourceSelectionRequest request,
        CancellationToken cancellationToken);

    Task<ManagedOperationResult<ManagedOperationStatus>> PublishInitialRulesAsync(
        InitialRulePublicationRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CampaignDescriptor>> ListCampaignsAsync(CancellationToken cancellationToken);

    Task<ManagedOperationResult<CampaignDescriptor>> CreateCampaignAsync(
        CreateCampaignRequest request,
        CancellationToken cancellationToken);
}

public sealed class ManagedAdministrationService(
    IOptions<ManagedAdministrationOptions> administrationOptions,
    IOptions<ManagedRuleServiceOptions> ruleOptions,
    ISchemaBootstrapExecutor bootstrapExecutor,
    IManagedReadinessService readiness,
    IRuleSourceConfigurationStore sourceConfigurations,
    ICampaignDirectoryService campaigns,
    IManagedOperationService? managedOperations = null) : IManagedAdministrationService
{
    private readonly ManagedAdministrationOptions administration = administrationOptions.Value;
    private readonly ManagedRuleServiceOptions rules = ruleOptions.Value;

    public Task<BootstrapPlan> GetBootstrapPlanAsync(
        string? campaignId,
        CancellationToken cancellationToken) =>
        bootstrapExecutor.PlanAsync(campaignId, cancellationToken);

    public async Task<ManagedOperationResult<BootstrapExecution>> BootstrapAsync(
        BootstrapRequest request,
        CancellationToken cancellationToken)
    {
        var denied = Authorize(request.UserApproved, request.OperatorConfirmation);
        if (denied is not null)
        {
            return Denied<BootstrapExecution>(denied);
        }

        try
        {
            var applied = await bootstrapExecutor.ExecuteAsync(request.CampaignId, cancellationToken);
            var report = await readiness.GetReadinessAsync(request.CampaignId, cancellationToken);
            return new(
                true,
                applied.Count == 0 ? "ALREADY_INITIALIZED" : "INITIALIZATION_COMPLETE",
                applied.Count == 0
                    ? "All supported Eternal Cycle-owned migrations were already applied."
                    : "Eternal Cycle-owned database structures were initialized and validated.",
                new BootstrapExecution(applied, applied.Count == 0, report));
        }
        catch (ManagedServiceException exception)
        {
            return new(false, exception.Code, exception.SafeMessage, null);
        }
        catch (SqlException)
        {
            return new(false, "INITIALIZATION_FAILED", "Database initialization failed; inspect authorized sanitized service logs.", null);
        }
    }

    public async Task<ManagedOperationResult<RuleSourceSelection>> ConfigureRuleSourceAsync(
        RuleSourceSelectionRequest request,
        CancellationToken cancellationToken)
    {
        var denied = Authorize(request.UserApproved, request.OperatorConfirmation);
        if (denied is not null)
        {
            return Denied<RuleSourceSelection>(denied);
        }

        try
        {
            var releaseChannel = request.ReleaseChannel ?? rules.ReleaseChannel;
            var metadata = request.UseOfficialDefault ? OfficialDistributionMetadata.Load(administration) : null;
            var sourceLocation = request.UseOfficialDefault
                ? metadata!.OfficialRepository
                : Require(request.SourceLocation, nameof(request.SourceLocation));
            var requestedRef = request.UseOfficialDefault
                ? OfficialRuleSourceRef(metadata!, releaseChannel)
                : request.RequestedRef ?? "HEAD";
            var manifestPath = request.UseOfficialDefault
                ? metadata!.RuleSourceManifest
                : request.ManifestPath ?? rules.GitSource.ManifestPath;

            var saved = await sourceConfigurations.SaveAsync(
                new RuleSourceConfiguration(
                    rules.RulesetId,
                    "Git",
                    sourceLocation,
                    Require(requestedRef, nameof(request.RequestedRef)),
                    Require(manifestPath, nameof(request.ManifestPath)),
                    request.UseOfficialDefault,
                    1,
                    DateTimeOffset.UtcNow,
                    releaseChannel),
                cancellationToken);
            return new(
                true,
                "RULE_SOURCE_CONFIGURED",
                request.UseOfficialDefault
                    ? "The official Eternal Cycle Git source was selected and will be acquired by the Managed service."
                    : "The compatible custom Git source was selected and will be acquired by the Managed service.",
                ToSelection(saved));
        }
        catch (ManagedServiceException exception)
        {
            return new(false, exception.Code, exception.SafeMessage, null);
        }
        catch (Exception exception) when (exception is IOException or JsonException or ArgumentException)
        {
            return new(false, "RULE_SOURCE_CONFIGURATION_FAILED", "Rule Source configuration could not be validated; inspect authorized sanitized service logs.", null);
        }
    }

    public async Task<ManagedOperationResult<ManagedOperationStatus>> PublishInitialRulesAsync(
        InitialRulePublicationRequest request,
        CancellationToken cancellationToken)
    {
        var denied = Authorize(request.UserApproved, request.OperatorConfirmation);
        if (denied is not null)
        {
            return Denied<ManagedOperationStatus>(denied);
        }

        var report = await readiness.GetReadinessAsync(null, cancellationToken);
        if (report.State is ManagedReadinessState.SetupRequired or ManagedReadinessState.MigrationRequired)
        {
            return new(false, report.ErrorCode ?? "SETUP_REQUIRED", report.Message, null);
        }

        if (report.State == ManagedReadinessState.RuleSourceRequired)
        {
            return new(false, "RULE_SOURCE_NOT_CONFIGURED", report.Message, null);
        }

        if (managedOperations is null)
        {
            return new(
                false,
                "MANAGED_OPERATIONS_UNAVAILABLE",
                "Durable Managed Operations are unavailable in this host configuration.",
                null,
                AdministrativeInterventionRequired: true);
        }

        try
        {
            var operation = await managedOperations.EnqueueInitialRulePublicationAsync(cancellationToken);
            return new(
                true,
                operation.State == ManagedOperationState.Queued
                    ? "RULE_PUBLICATION_QUEUED"
                    : "RULE_PUBLICATION_ALREADY_ACTIVE",
                "Durable rule publication was queued or an equivalent active operation was reused. Query its operation ID for progress.",
                operation,
                ManagedOperationKinds.InitialRulePublication,
                operation.CurrentStage,
                operation.CorrelationId,
                RetrySafe: true);
        }
        catch (ManagedServiceException exception)
        {
            return new(false, exception.Code, exception.SafeMessage, null);
        }
    }

    public Task<IReadOnlyList<CampaignDescriptor>> ListCampaignsAsync(CancellationToken cancellationToken) =>
        campaigns.ListAsync(cancellationToken);

    public async Task<ManagedOperationResult<CampaignDescriptor>> CreateCampaignAsync(
        CreateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var denied = Authorize(request.UserApproved, request.OperatorConfirmation);
        if (denied is not null)
        {
            return Denied<CampaignDescriptor>(denied);
        }

        try
        {
            var descriptor = await campaigns.CreateAsync(request.DisplayName, request.Description, cancellationToken);
            return new(true, "CAMPAIGN_CREATED", "The campaign identity was created in the configured Data Namespace.", descriptor);
        }
        catch (ManagedServiceException exception)
        {
            return new(false, exception.Code, exception.SafeMessage, null);
        }
    }

    private AuthorizationDenial? Authorize(bool userApproved, string? operatorConfirmation)
    {
        if (!administration.Enabled)
        {
            return new(
                "ADMINISTRATION_DISABLED",
                "Administrative setup tools are disabled in service configuration.",
                UserApprovalRequired: false,
                AdministrativeInterventionRequired: true,
                RetrySafe: false);
        }

        if (!userApproved)
        {
            return new(
                "USER_APPROVAL_REQUIRED",
                "Explain the proposed administrative action and obtain explicit informed user approval before retrying.",
                UserApprovalRequired: true,
                AdministrativeInterventionRequired: false,
                RetrySafe: true);
        }

        if (administration.RequireOperatorConfirmation &&
            !string.Equals(operatorConfirmation, administration.ApprovalPhrase, StringComparison.Ordinal))
        {
            return new(
                "ADMINISTRATIVE_INTERVENTION_REQUIRED",
                "This deployment requires an operator confirmation in addition to conversational user approval.",
                UserApprovalRequired: false,
                AdministrativeInterventionRequired: true,
                RetrySafe: false);
        }

        return null;
    }

    private static ManagedOperationResult<T> Denied<T>(AuthorizationDenial denial) =>
        new(
            false,
            denial.Code,
            denial.Message,
            default,
            RetrySafe: denial.RetrySafe,
            AdministrativeInterventionRequired: denial.AdministrativeInterventionRequired,
            UserApprovalRequired: denial.UserApprovalRequired);

    private sealed record AuthorizationDenial(
        string Code,
        string Message,
        bool UserApprovalRequired,
        bool AdministrativeInterventionRequired,
        bool RetrySafe);

    private static RuleSourceSelection ToSelection(RuleSourceConfiguration value) =>
        new(
            value.RulesetId,
            value.ProviderKind,
            value.RequestedRef,
            value.ManifestPath,
            value.IsOfficial,
            value.ConfigurationRevision,
            value.ConfiguredAt,
            value.ReleaseChannel);

    private static string Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 2000)
        {
            throw new ArgumentException("Rule Source values must contain 1 to 2000 non-whitespace characters.", name);
        }

        return value;
    }

    private static string OfficialRuleSourceRef(
        OfficialDistributionMetadata metadata,
        RuleSourceReleaseChannel releaseChannel)
    {
        var configured = releaseChannel switch
        {
            RuleSourceReleaseChannel.Stable => metadata.StableRuleSourceRef,
            RuleSourceReleaseChannel.Prerelease => metadata.PrereleaseRuleSourceRef ?? metadata.DevelopmentRef,
            _ => null
        };
        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new ManagedServiceException(
                "RULE_SOURCE_INCOMPATIBLE",
                releaseChannel == RuleSourceReleaseChannel.Stable
                    ? "No compatible Stable Managed Rule Source is published. An administrator may explicitly select Prerelease for unreleased testing."
                    : "No compatible Prerelease Managed Rule Source is configured in official distribution metadata.");
        }

        var value = Require(configured, nameof(configured));
        if (value.StartsWith("refs/", StringComparison.Ordinal))
        {
            return value;
        }

        return releaseChannel == RuleSourceReleaseChannel.Stable
            ? $"refs/tags/{value}"
            : $"refs/heads/{value}";
    }
}

public sealed class SqlServerRuleSourceConfigurationStore(
    IOptions<SqlServerPersistenceOptions> persistenceOptions) : IRuleSourceConfigurationStore
{
    private readonly SqlServerPersistenceOptions settings = persistenceOptions.Value;

    public async Task<RuleSourceConfiguration?> GetAsync(
        string rulesetId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            SELECT ruleset_id, provider_kind, source_location, requested_ref,
                   manifest_path, is_official, config_revision, configured_at,
                   release_channel
            FROM {{schema}}.rule_source_configurations
            WHERE ruleset_id = @ruleset_id;
            """);
        command.Parameters.AddWithValue("@ruleset_id", rulesetId);
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Read(reader) : null;
    }

    public async Task<RuleSourceConfiguration> SaveAsync(
        RuleSourceConfiguration configuration,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            MERGE {{schema}}.rule_source_configurations AS target
            USING (SELECT @ruleset_id AS ruleset_id) AS source
            ON target.ruleset_id = source.ruleset_id
            WHEN MATCHED THEN UPDATE SET
                provider_kind = @provider_kind,
                source_location = @source_location,
                requested_ref = @requested_ref,
                manifest_path = @manifest_path,
                is_official = @is_official,
                release_channel = @release_channel,
                config_revision = target.config_revision + 1,
                configured_at = @configured_at
            WHEN NOT MATCHED THEN INSERT (
                ruleset_id, provider_kind, source_location, requested_ref,
                manifest_path, is_official, config_revision, configured_at,
                release_channel
            ) VALUES (
                @ruleset_id, @provider_kind, @source_location, @requested_ref,
                @manifest_path, @is_official, 1, @configured_at,
                @release_channel
            );

            SELECT ruleset_id, provider_kind, source_location, requested_ref,
                   manifest_path, is_official, config_revision, configured_at,
                   release_channel
            FROM {{schema}}.rule_source_configurations
            WHERE ruleset_id = @ruleset_id;
            """);
        command.Parameters.AddWithValue("@ruleset_id", configuration.RulesetId);
        command.Parameters.AddWithValue("@provider_kind", configuration.ProviderKind);
        command.Parameters.AddWithValue("@source_location", configuration.SourceLocation);
        command.Parameters.AddWithValue("@requested_ref", configuration.RequestedRef);
        command.Parameters.AddWithValue("@manifest_path", configuration.ManifestPath);
        command.Parameters.AddWithValue("@is_official", configuration.IsOfficial);
        command.Parameters.AddWithValue("@configured_at", configuration.ConfiguredAt);
        command.Parameters.AddWithValue("@release_channel", configuration.ReleaseChannel.ToString());
        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow, cancellationToken);
        _ = await reader.ReadAsync(cancellationToken);
        return Read(reader);
    }

    private static RuleSourceConfiguration Read(SqlDataReader reader) =>
        new(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetBoolean(5),
            reader.GetInt64(6),
            reader.GetDateTimeOffset(7),
            Enum.Parse<RuleSourceReleaseChannel>(reader.GetString(8), ignoreCase: true));

    private async Task<SqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private SqlCommand Command(SqlConnection connection, string text) =>
        new(SqlServerSchemaIdentifier.Bind(text, settings.DomainSchema), connection)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };
}

public sealed class SqlServerSchemaBootstrapExecutor(
    IOptions<SqlServerPersistenceOptions> persistenceOptions,
    ICampaignSchemaResolver schemaResolver) : ISchemaBootstrapExecutor
{
    private static readonly Regex BatchSeparator = new(
        @"^\s*GO\s*$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private readonly SqlServerPersistenceOptions settings = persistenceOptions.Value;

    public async Task<BootstrapPlan> PlanAsync(
        string? campaignId,
        CancellationToken cancellationToken)
    {
        var plan = await BuildPlanAsync(campaignId, cancellationToken);
        return new BootstrapPlan(
            plan.Count > 0,
            plan.Select(item => item.Id).ToArray(),
            plan.Select(item => item.Scope).Distinct(StringComparer.Ordinal).ToArray(),
            "Obtain explicit informed user approval. A deployment may additionally require an operator confirmation.",
            plan.Count == 0
                ? "No supported EC-owned schema changes are required."
                : "Only packaged, versioned Eternal Cycle campaign and rule-domain migrations will be applied.");
    }

    public async Task<IReadOnlyList<string>> ExecuteAsync(
        string? campaignId,
        CancellationToken cancellationToken)
    {
        var plan = await BuildPlanAsync(campaignId, cancellationToken);
        if (plan.Count == 0)
        {
            return [];
        }

        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        foreach (var migration in plan)
        {
            foreach (var batch in BatchSeparator.Split(migration.Sql).Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                await using var command = new SqlCommand(batch, connection, transaction)
                {
                    CommandTimeout = settings.CommandTimeoutSeconds
                };
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await transaction.CommitAsync(cancellationToken);
        var remaining = await BuildPlanAsync(campaignId, cancellationToken);
        if (remaining.Count > 0)
        {
            throw new ManagedServiceException(
                "MIGRATION_VALIDATION_FAILED",
                "Schema migration completed without producing the expected Eternal Cycle structures.");
        }

        return plan.Select(item => item.Id).ToArray();
    }

    private async Task<List<MigrationItem>> BuildPlanAsync(
        string? campaignId,
        CancellationToken cancellationToken)
    {
        var route = schemaResolver.Resolve(campaignId ?? "bootstrap-default");
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        var campaignTables = await ReadTablesAsync(connection, route.SchemaName, cancellationToken);
        var domainTables = await ReadTablesAsync(connection, settings.DomainSchema, cancellationToken);
        var campaignCount = SqlServerManagedInfrastructureInspector.CampaignTables.Count(campaignTables.Contains);
        var domainCount = SqlServerManagedInfrastructureInspector.RuleDomainTables.Count(domainTables.Contains);
        if (campaignCount is > 0 && campaignCount < SqlServerManagedInfrastructureInspector.CampaignTables.Length)
        {
            throw new ManagedServiceException(
                "MIGRATION_REQUIRED",
                "The campaign schema is partially initialized and requires administrator review before automated migration.");
        }

        if (domainCount is > 0 && domainCount < SqlServerManagedInfrastructureInspector.RuleDomainTables.Length)
        {
            throw new ManagedServiceException(
                "MIGRATION_REQUIRED",
                "The Rule Domain schema is partially initialized and requires administrator review before automated migration.");
        }

        var migrations = new List<MigrationItem>();
        if (campaignCount == 0)
        {
            migrations.Add(new(
                "001_campaign_persistence",
                $"Campaign schema {route.SchemaName}",
                Render("001_initial.template.sql", route)));
        }
        else if (!await CampaignColumnsReadyAsync(connection, route.SchemaName, cancellationToken))
        {
            migrations.Add(new(
                "003_campaign_directory",
                $"Campaign schema {route.SchemaName}",
                Render("003_campaign_directory.template.sql", route)));
        }

        if (domainCount == 0)
        {
            migrations.Add(new(
                "002_rule_domain",
                $"Domain schema {settings.DomainSchema}",
                RenderDomain("002_rule_domain.template.sql")));
            migrations.Add(new(
                "004_rule_source_configuration",
                $"Domain schema {settings.DomainSchema}",
                RenderDomain("004_rule_source_configuration.template.sql")));
            migrations.Add(new(
                "005_managed_operation_diagnostics",
                $"Domain schema {settings.DomainSchema}",
                RenderDomain("005_managed_operation_diagnostics.template.sql")));
            migrations.Add(new(
                "006_rule_source_compatibility",
                $"Domain schema {settings.DomainSchema}",
                RenderDomain("006_rule_source_compatibility.template.sql")));
        }
        else
        {
            var compatibilityFoundationAdded = false;
            if (!domainTables.Contains("rule_source_configurations"))
            {
                migrations.Add(new(
                    "004_rule_source_configuration",
                    $"Domain schema {settings.DomainSchema}",
                    RenderDomain("004_rule_source_configuration.template.sql")));
                compatibilityFoundationAdded = true;
            }

            if (!domainTables.Contains("managed_operation_diagnostics"))
            {
                migrations.Add(new(
                    "005_managed_operation_diagnostics",
                    $"Domain schema {settings.DomainSchema}",
                    RenderDomain("005_managed_operation_diagnostics.template.sql")));
                compatibilityFoundationAdded = true;
            }

            if (compatibilityFoundationAdded ||
                !await RuleSourceCompatibilityColumnsReadyAsync(connection, cancellationToken))
            {
                migrations.Add(new(
                    "006_rule_source_compatibility",
                    $"Domain schema {settings.DomainSchema}",
                    RenderDomain("006_rule_source_compatibility.template.sql")));
            }
        }

        if (domainCount == 0 ||
            !domainTables.Contains("managed_operations") ||
            !domainTables.Contains("rule_source_preparation") ||
            !await DurableManagedOperationColumnsReadyAsync(connection, cancellationToken))
        {
            migrations.Add(new(
                "007_durable_managed_operations",
                $"Domain schema {settings.DomainSchema}",
                RenderDomain("007_durable_managed_operations.template.sql")));
        }

        return migrations;
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
            CommandTimeout = settings.CommandTimeoutSeconds
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

    private async Task<bool> CampaignColumnsReadyAsync(
        SqlConnection connection,
        string schemaName,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("""
            SELECT COUNT(*)
            FROM sys.columns AS columns
            INNER JOIN sys.tables AS tables ON tables.object_id = columns.object_id
            INNER JOIN sys.schemas AS schemas ON schemas.schema_id = tables.schema_id
            WHERE schemas.name = @schema_name
              AND tables.name = N'campaigns'
              AND columns.name IN (N'display_name', N'description');
            """, connection)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@schema_name", schemaName);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 2;
    }

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
            CommandTimeout = settings.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@schema_name", settings.DomainSchema);
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
            CommandTimeout = settings.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@schema_name", settings.DomainSchema);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 17;
    }

    private string Render(string fileName, CampaignSchemaRoute route) =>
        SqlServerSchemaMigration.Render(File.ReadAllText(SchemaFile(fileName)), route);

    private string RenderDomain(string fileName) =>
        SqlServerSchemaMigration.RenderDomain(File.ReadAllText(SchemaFile(fileName)), settings.DomainSchema);

    private static string SchemaFile(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Schema", fileName);

    private sealed record MigrationItem(string Id, string Scope, string Sql);
}

public sealed class SqlServerCampaignDirectoryService(
    IOptions<SqlServerPersistenceOptions> persistenceOptions,
    ICampaignSchemaResolver schemaResolver) : ICampaignDirectoryService
{
    private readonly SqlServerPersistenceOptions settings = persistenceOptions.Value;

    public async Task<IReadOnlyList<CampaignDescriptor>> ListAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        var campaigns = new Dictionary<string, CampaignDescriptor>(StringComparer.Ordinal);
        foreach (var schema in KnownSchemas())
        {
            if (!await CampaignTableExistsAsync(connection, schema, cancellationToken))
            {
                continue;
            }

            await using var command = new SqlCommand(
                SqlServerSchemaIdentifier.Bind("""
                    SELECT campaign_id, display_name, description
                    FROM {{schema}}.campaigns
                    ORDER BY COALESCE(display_name, campaign_id), campaign_id;
                    """, schema),
                connection)
            {
                CommandTimeout = settings.CommandTimeoutSeconds
            };
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetString(0);
                var route = schemaResolver.Resolve(id);
                campaigns[id] = new CampaignDescriptor(
                    id,
                    reader.IsDBNull(1) ? id : reader.GetString(1),
                    reader.IsDBNull(2) ? null : reader.GetString(2),
                    route.WorldModelId,
                    route.DataNamespaceId);
            }
        }

        return campaigns.Values.OrderBy(value => value.DisplayName, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public async Task<CampaignDescriptor> CreateAsync(
        string displayName,
        string? description,
        CancellationToken cancellationToken)
    {
        displayName = Require(displayName, 256, nameof(displayName));
        if (description is not null)
        {
            description = Require(description, 1000, nameof(description));
        }

        var campaignId = $"{Slug(displayName)}-{Guid.NewGuid():N}"[..Math.Min(96, Slug(displayName).Length + 9)];
        var route = schemaResolver.Resolve(campaignId);
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        if (!await CampaignTableExistsAsync(connection, route.SchemaName, cancellationToken))
        {
            throw new ManagedServiceException("CAMPAIGN_SCHEMA_MISSING", "Campaign persistence must be initialized before creating a campaign.");
        }

        await using var command = new SqlCommand(
            SqlServerSchemaIdentifier.Bind("""
                INSERT INTO {{schema}}.campaigns (
                    campaign_id, display_name, description, repository_version, persistence_model_version
                ) VALUES (
                    @campaign_id, @display_name, @description, @repository_version, N'managed-sql-1'
                );
                """, route.SchemaName),
            connection)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@campaign_id", campaignId);
        command.Parameters.AddWithValue("@display_name", displayName);
        command.Parameters.AddWithValue("@description", (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("@repository_version", route.RulesetVersion);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return new CampaignDescriptor(campaignId, displayName, description, route.WorldModelId, route.DataNamespaceId);
    }

    private IEnumerable<string> KnownSchemas() =>
        new[] { settings.DefaultSchema }
            .Concat(settings.DataNamespaces.Values.Select(value => value.SchemaName))
            .Concat(settings.WorldSchemas.Values.Select(value => value.SchemaName))
            .Select(SqlServerSchemaIdentifier.Validate)
            .Distinct(StringComparer.OrdinalIgnoreCase);

    private async Task<bool> CampaignTableExistsAsync(
        SqlConnection connection,
        string schema,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("""
            SELECT COUNT(*)
            FROM sys.tables AS tables
            INNER JOIN sys.schemas AS schemas ON schemas.schema_id = tables.schema_id
            WHERE schemas.name = @schema_name AND tables.name = N'campaigns';
            """, connection)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@schema_name", schema);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) == 1;
    }

    private static string Slug(string value)
    {
        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "campaign" : slug[..Math.Min(slug.Length, 64)];
    }

    private static string Require(string value, int maximum, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximum)
        {
            throw new ManagedServiceException("INVALID_CAMPAIGN_METADATA", $"{name} must contain 1 to {maximum} characters.");
        }

        return value.Trim();
    }
}

public class ManagedServiceException(string code, string safeMessage) : Exception(safeMessage)
{
    public string Code { get; } = code;

    public string SafeMessage { get; } = safeMessage;
}

public sealed record OfficialDistributionMetadata(
    string Project,
    string OfficialRepository,
    string StableReleaseTag,
    string DevelopmentRef,
    string RuleSourceManifest,
    string? StableRuleSourceRef = null,
    string? PrereleaseRuleSourceRef = null,
    string? PrereleaseBaseRelease = null,
    string? PrereleaseDiscoveryTag = null,
    string? PrereleaseTargetVersion = null)
{
    public static OfficialDistributionMetadata Load(ManagedAdministrationOptions options)
    {
        var path = Path.IsPathRooted(options.DistributionMetadataFile)
            ? options.DistributionMetadataFile
            : Path.Combine(AppContext.BaseDirectory, options.DistributionMetadataFile);
        if (!File.Exists(path))
        {
            throw new ManagedServiceException(
                "OFFICIAL_SOURCE_METADATA_MISSING",
                "Official distribution metadata is unavailable; configure a compatible custom Rule Source or repair the installation.");
        }

        var metadata = JsonSerializer.Deserialize<OfficialDistributionMetadata>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return metadata is not null &&
               Uri.TryCreate(metadata.OfficialRepository, UriKind.Absolute, out _) &&
               !string.IsNullOrWhiteSpace(metadata.StableReleaseTag) &&
               !string.IsNullOrWhiteSpace(metadata.DevelopmentRef) &&
               !string.IsNullOrWhiteSpace(metadata.RuleSourceManifest)
            ? metadata
            : throw new ManagedServiceException(
                "OFFICIAL_SOURCE_METADATA_INVALID",
                "Official distribution metadata is invalid; configure a compatible custom Rule Source or repair the installation.");
    }
}

[McpServerToolType]
public sealed class ManagedSetupTools(
    IManagedReadinessService readiness,
    IManagedAdministrationService administration)
{
    [McpServerTool(Name = "ec_get_readiness", ReadOnly = true, Idempotent = true),
     Description("Returns structured Managed-service readiness without mutating infrastructure or exposing secrets.")]
    public Task<ManagedReadinessReport> GetReadinessAsync(
        [Description("Optional stable campaign identifier when campaign-specific readiness is required.")] string? campaignId,
        CancellationToken cancellationToken) =>
        readiness.GetReadinessAsync(campaignId, cancellationToken);

    [McpServerTool(Name = "ec_get_setup_plan", ReadOnly = true, Idempotent = true),
     Description("Previews only the packaged Eternal Cycle-owned migrations required for first-run setup.")]
    public Task<BootstrapPlan> GetSetupPlanAsync(
        [Description("Optional campaign identifier used only to resolve its trusted Data Namespace.")] string? campaignId,
        CancellationToken cancellationToken) =>
        administration.GetBootstrapPlanAsync(campaignId, cancellationToken);

    [McpServerTool(Name = "ec_initialize_service", Destructive = true, Idempotent = true),
     Description("After explicit informed user approval, applies only packaged Eternal Cycle-owned migrations and validates the result. Set userApproved only after the user actually authorizes the explained action.")]
    public Task<ManagedOperationResult<BootstrapExecution>> InitializeAsync(
        [Description("Explicit informed user approval and optional operator-only deployment confirmation plus a trusted campaign route.")] BootstrapRequest request,
        CancellationToken cancellationToken) =>
        administration.BootstrapAsync(request, cancellationToken);

    [McpServerTool(Name = "ec_configure_rule_source", Destructive = true, Idempotent = true),
     Description("After explicit informed user approval, persists a semantic Stable/Prerelease official selection or an advanced compatible custom source. Ordinary players do not provide refs or SHAs.")]
    public Task<ManagedOperationResult<RuleSourceSelection>> ConfigureRuleSourceAsync(
        RuleSourceSelectionRequest request,
        CancellationToken cancellationToken) =>
        administration.ConfigureRuleSourceAsync(request, cancellationToken);

    [McpServerTool(Name = "ec_publish_initial_rules", Destructive = true, Idempotent = true),
     Description("After explicit informed user approval, quickly creates or reuses a durable initial-publication operation. Use ec_get_operation_status to follow background progress.")]
    public Task<ManagedOperationResult<ManagedOperationStatus>> PublishRulesAsync(
        InitialRulePublicationRequest request,
        CancellationToken cancellationToken) =>
        administration.PublishInitialRulesAsync(request, cancellationToken);

    [McpServerTool(Name = "ec_list_campaigns", ReadOnly = true, Idempotent = true),
     Description("Lists stable campaign identities with player-meaningful names without returning Campaign Canon.")]
    public Task<IReadOnlyList<CampaignDescriptor>> ListCampaignsAsync(CancellationToken cancellationToken) =>
        administration.ListCampaignsAsync(cancellationToken);

    [McpServerTool(Name = "ec_resolve_resume_campaign", ReadOnly = true, Idempotent = true),
     Description("Selects the sole campaign automatically or returns meaningful choices without requiring an opaque ID.")]
    public async Task<CampaignResolution> ResolveResumeCampaignAsync(CancellationToken cancellationToken) =>
        CampaignDiscovery.ResolveResume(await administration.ListCampaignsAsync(cancellationToken));

    [McpServerTool(Name = "ec_create_campaign", Destructive = true, Idempotent = false),
     Description("After explicit informed user approval, creates one campaign identity in the trusted default Data Namespace. Set userApproved only after the user actually authorizes creation.")]
    public Task<ManagedOperationResult<CampaignDescriptor>> CreateCampaignAsync(
        CreateCampaignRequest request,
        CancellationToken cancellationToken) =>
        administration.CreateCampaignAsync(request, cancellationToken);
}
