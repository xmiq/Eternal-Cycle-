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
    string? DiagnosticsAvailability = null);

public sealed record BootstrapRequest(
    string? CampaignId,
    bool Approved,
    string ApprovalPhrase);

public sealed record BootstrapPlan(
    bool ChangesRequired,
    IReadOnlyList<string> MigrationIds,
    IReadOnlyList<string> OwnedScopes,
    string ApprovalPhrase,
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
    bool Approved,
    string ApprovalPhrase);

public sealed record RuleSourceSelection(
    string RulesetId,
    string ProviderKind,
    string RequestedRef,
    string ManifestPath,
    bool IsOfficial,
    long ConfigurationRevision,
    DateTimeOffset ConfiguredAt);

public sealed record InitialRulePublicationRequest(
    bool Approved,
    string ApprovalPhrase);

public sealed record CampaignDescriptor(
    string CampaignId,
    string DisplayName,
    string? Description,
    string WorldModelId,
    string DataNamespaceId);

public sealed record CreateCampaignRequest(
    string DisplayName,
    string? Description,
    bool Approved,
    string ApprovalPhrase);

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
    DateTimeOffset ConfiguredAt);

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

    Task<ManagedOperationResult<RulePublicationResult>> PublishInitialRulesAsync(
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
    ManagedRulePublicationCoordinator publicationCoordinator,
    ICampaignDirectoryService campaigns) : IManagedAdministrationService
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
        var denied = Authorize(request.Approved, request.ApprovalPhrase);
        if (denied is not null)
        {
            return new(false, denied.Value.Code, denied.Value.Message, null);
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
        var denied = Authorize(request.Approved, request.ApprovalPhrase);
        if (denied is not null)
        {
            return new(false, denied.Value.Code, denied.Value.Message, null);
        }

        try
        {
            var metadata = request.UseOfficialDefault ? OfficialDistributionMetadata.Load(administration) : null;
            var sourceLocation = request.UseOfficialDefault
                ? metadata!.OfficialRepository
                : Require(request.SourceLocation, nameof(request.SourceLocation));
            var requestedRef = request.RequestedRef
                ?? (request.UseOfficialDefault ? OfficialStableRef(metadata!.StableReleaseTag) : "HEAD");
            var manifestPath = request.ManifestPath
                ?? (request.UseOfficialDefault ? metadata!.RuleSourceManifest : rules.GitSource.ManifestPath);

            var saved = await sourceConfigurations.SaveAsync(
                new RuleSourceConfiguration(
                    rules.RulesetId,
                    "Git",
                    sourceLocation,
                    Require(requestedRef, nameof(request.RequestedRef)),
                    Require(manifestPath, nameof(request.ManifestPath)),
                    request.UseOfficialDefault,
                    1,
                    DateTimeOffset.UtcNow),
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

    public async Task<ManagedOperationResult<RulePublicationResult>> PublishInitialRulesAsync(
        InitialRulePublicationRequest request,
        CancellationToken cancellationToken)
    {
        var denied = Authorize(request.Approved, request.ApprovalPhrase);
        if (denied is not null)
        {
            return new(false, denied.Value.Code, denied.Value.Message, null);
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

        try
        {
            var result = await publicationCoordinator.CheckForUpdateAsync(cancellationToken);
            var success = result.Status is "Activated" or "Unchanged" or "AlreadyPublished" or "AwaitingAdministratorActivation";
            return new(
                success,
                success ? "RULE_PUBLICATION_COMPLETE" : result.ErrorCode ?? "RULE_PUBLICATION_FAILED",
                success
                    ? "The initial Rule Release workflow completed. Check readiness to confirm activation policy."
                    : result.FailureReason ?? "The Rule Release candidate failed. Any previous active release was preserved.",
                result,
                result.Operation,
                result.Stage,
                result.CorrelationId,
                result.RetrySafe,
                result.AdministrativeInterventionRequired,
                result.DiagnosticsAvailability);
        }
        catch (OperationCanceledException)
        {
            return new(
                false,
                "RULE_PUBLICATION_CANCELLED",
                "Rule publication was cancelled before the Managed publication coordinator could return a structured result.",
                null,
                "PublishInitialRules",
                RulePublicationStage.Cancelled.ToString(),
                null,
                true,
                false,
                "Unavailable");
        }
    }

    public Task<IReadOnlyList<CampaignDescriptor>> ListCampaignsAsync(CancellationToken cancellationToken) =>
        campaigns.ListAsync(cancellationToken);

    public async Task<ManagedOperationResult<CampaignDescriptor>> CreateCampaignAsync(
        CreateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var denied = Authorize(request.Approved, request.ApprovalPhrase);
        if (denied is not null)
        {
            return new(false, denied.Value.Code, denied.Value.Message, null);
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

    private (string Code, string Message)? Authorize(bool approved, string approvalPhrase)
    {
        if (!administration.Enabled)
        {
            return ("ADMINISTRATION_DISABLED", "Administrative setup tools are disabled in service configuration.");
        }

        if (!approved || !string.Equals(approvalPhrase, administration.ApprovalPhrase, StringComparison.Ordinal))
        {
            return ("ADMINISTRATIVE_APPROVAL_REQUIRED", "Explicit user approval with the configured confirmation phrase is required.");
        }

        return null;
    }

    private static RuleSourceSelection ToSelection(RuleSourceConfiguration value) =>
        new(
            value.RulesetId,
            value.ProviderKind,
            value.RequestedRef,
            value.ManifestPath,
            value.IsOfficial,
            value.ConfigurationRevision,
            value.ConfiguredAt);

    private static string Require(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 2000)
        {
            throw new ArgumentException("Rule Source values must contain 1 to 2000 non-whitespace characters.", name);
        }

        return value;
    }

    private static string OfficialStableRef(string stableReleaseTag)
    {
        var value = Require(stableReleaseTag, nameof(stableReleaseTag));
        return value.StartsWith("refs/", StringComparison.Ordinal) ? value : $"refs/tags/{value}";
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
                   manifest_path, is_official, config_revision, configured_at
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
                config_revision = target.config_revision + 1,
                configured_at = @configured_at
            WHEN NOT MATCHED THEN INSERT (
                ruleset_id, provider_kind, source_location, requested_ref,
                manifest_path, is_official, config_revision, configured_at
            ) VALUES (
                @ruleset_id, @provider_kind, @source_location, @requested_ref,
                @manifest_path, @is_official, 1, @configured_at
            );

            SELECT ruleset_id, provider_kind, source_location, requested_ref,
                   manifest_path, is_official, config_revision, configured_at
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
            reader.GetDateTimeOffset(7));

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
            "Use the configured administrative approval phrase.",
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
        }
        else
        {
            if (!domainTables.Contains("rule_source_configurations"))
            {
                migrations.Add(new(
                    "004_rule_source_configuration",
                    $"Domain schema {settings.DomainSchema}",
                    RenderDomain("004_rule_source_configuration.template.sql")));
            }

            if (!domainTables.Contains("managed_operation_diagnostics"))
            {
                migrations.Add(new(
                    "005_managed_operation_diagnostics",
                    $"Domain schema {settings.DomainSchema}",
                    RenderDomain("005_managed_operation_diagnostics.template.sql")));
            }
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

public sealed class ManagedServiceException(string code, string safeMessage) : Exception(safeMessage)
{
    public string Code { get; } = code;

    public string SafeMessage { get; } = safeMessage;
}

public sealed record OfficialDistributionMetadata(
    string Project,
    string OfficialRepository,
    string StableReleaseTag,
    string DevelopmentRef,
    string RuleSourceManifest)
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
     Description("After explicit user approval, applies only packaged Eternal Cycle-owned migrations and validates the result.")]
    public Task<ManagedOperationResult<BootstrapExecution>> InitializeAsync(
        [Description("Explicit approval, configured confirmation phrase, and optional trusted campaign route.")] BootstrapRequest request,
        CancellationToken cancellationToken) =>
        administration.BootstrapAsync(request, cancellationToken);

    [McpServerTool(Name = "ec_configure_rule_source", Destructive = true, Idempotent = true),
     Description("After explicit approval, persists the official default or a compatible custom Git Rule Source selection.")]
    public Task<ManagedOperationResult<RuleSourceSelection>> ConfigureRuleSourceAsync(
        RuleSourceSelectionRequest request,
        CancellationToken cancellationToken) =>
        administration.ConfigureRuleSourceAsync(request, cancellationToken);

    [McpServerTool(Name = "ec_publish_initial_rules", Destructive = true, Idempotent = true),
     Description("After explicit approval, acquires, compiles, validates, publishes, and activates initial rules under configured policy.")]
    public Task<ManagedOperationResult<RulePublicationResult>> PublishRulesAsync(
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
     Description("After explicit user approval, creates one campaign identity in the trusted default Data Namespace.")]
    public Task<ManagedOperationResult<CampaignDescriptor>> CreateCampaignAsync(
        CreateCampaignRequest request,
        CancellationToken cancellationToken) =>
        administration.CreateCampaignAsync(request, cancellationToken);
}
