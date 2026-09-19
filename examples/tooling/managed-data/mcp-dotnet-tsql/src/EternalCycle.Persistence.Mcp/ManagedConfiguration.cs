using System.ComponentModel;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public enum ManagedConfigurationValidationStatus
{
    Valid,
    Defaulted,
    Missing,
    Invalid
}

public sealed record ManagedConfigurationDescriptor(
    string Key,
    string EnvironmentVariableName,
    string DisplayName,
    string Description,
    string Category,
    bool Required,
    bool Sensitive,
    bool Configured,
    string ConfigurationSource,
    string ValueType,
    string? EffectiveValue,
    string? ExampleValue,
    IReadOnlyList<string> AllowedValues,
    string? DefaultValue,
    bool RestartRequired,
    string Audience,
    ManagedConfigurationValidationStatus ValidationStatus,
    string ValidationMessage);

public sealed record ManagedConfigurationReport(
    string State,
    bool Ready,
    IReadOnlyList<ManagedConfigurationDescriptor> Settings,
    IReadOnlyList<string> RequiredActions,
    string Message);

public interface IManagedConfigurationService
{
    ManagedConfigurationReport GetReport();
}

public sealed class ManagedConfigurationService(IConfiguration configuration) : IManagedConfigurationService
{
    private static readonly IReadOnlyList<ConfigurationDefinition> Definitions =
    [
        Required(
            "EternalCycle:Persistence:ConnectionString",
            "Managed persistence connection",
            "Connection string for the Managed service database. The value is never returned by discovery.",
            "Persistence",
            "connection-string",
            "Server=localhost;Initial Catalog=EternalCycle;Integrated Security=true;TrustServerCertificate=true",
            "Operator",
            ValidateConnectionString,
            sensitive: true),
        Optional("EternalCycle:Persistence:CommandTimeoutSeconds", "Database command timeout", "Maximum duration for one database command.", "Persistence", "integer", "30", "30", "AdvancedOperator", ValidatePositiveInteger),
        Optional("EternalCycle:Persistence:RequireRecoveryPointForCompletion", "Require recovery point", "Requires configured recovery-point durability before a turn may complete.", "Persistence", "boolean", "false", "false", "AdvancedOperator", ValidateBoolean),
        Optional("EternalCycle:Persistence:RecoveryPointDirectory", "Recovery-point directory", "Optional deployment-owned recovery-point destination.", "Persistence", "path", null, null, "AdvancedOperator", ValidateOptionalText),
        Optional("EternalCycle:Persistence:DomainSchema", "Rule Domain schema", "Validated SQL schema used by the reference Managed rule domain.", "Persistence", "identifier", "ec_domain", "ec_domain", "AdvancedOperator", ValidateSchemaIdentifier),
        Optional("EternalCycle:Persistence:DefaultSchema", "Default campaign schema", "Validated SQL schema used by the default Data Namespace.", "Persistence", "identifier", "ec", "ec", "AdvancedOperator", ValidateSchemaIdentifier),
        Optional("EternalCycle:Persistence:DefaultDataNamespaceId", "Default Data Namespace", "Stable default logical Data Namespace identifier.", "Persistence", "identifier", "eternal-cycle-mainworld", "eternal-cycle-mainworld", "AdvancedOperator", ValidateRequiredText),
        Optional("EternalCycle:Persistence:DefaultWorldModelId", "Default World Model", "Stable default World Model identifier.", "Persistence", "identifier", "eternal-cycle-standard", "eternal-cycle-standard", "AdvancedOperator", ValidateRequiredText),
        Optional("EternalCycle:Persistence:DefaultRulesetId", "Default RuleSet", "Stable default RuleSet identifier.", "Persistence", "identifier", "eternal-cycle-core", "eternal-cycle-core", "AdvancedOperator", ValidateRequiredText),
        Optional("EternalCycle:Persistence:DefaultSchemaModelVersion", "Schema model version", "Expected campaign schema-model version.", "Persistence", "string", "1", "1", "AdvancedOperator", ValidateRequiredText),
        Optional("EternalCycle:Persistence:DefaultRulesetVersion", "RuleSet version", "Expected campaign RuleSet compatibility version.", "Persistence", "version", "1.0.0", "1.0.0", "AdvancedOperator", ValidateRequiredText),
        Optional("EternalCycle:Administration:Enabled", "Administrative tools", "Enables permission-gated setup, migration, source selection, publication, and campaign creation.", "Administration", "boolean", "true", "false", "Operator", ValidateBoolean),
        Optional("EternalCycle:Administration:RequireOperatorConfirmation", "Operator confirmation safeguard", "Adds a deployment-only literal confirmation after normal conversational user approval.", "Administration", "boolean", "false", "false", "AdvancedOperator", ValidateBoolean),
        Optional("EternalCycle:Administration:ApprovalPhrase", "Operator confirmation phrase", "Optional deployment safeguard used only when operator confirmation is enabled. The configured value is never returned.", "Administration", "string", null, "INITIALIZE ETERNAL CYCLE", "AdvancedOperator", ValidateRequiredText, sensitive: true),
        Optional("EternalCycle:Administration:DistributionMetadataFile", "Distribution metadata file", "Packaged official-source metadata filename or advanced override path.", "Administration", "path", "distribution-metadata.json", "distribution-metadata.json", "AdvancedOperator", ValidateRequiredText),
        Optional("EternalCycle:Administration:ManagedRuleCacheDirectory", "Managed Rule Source cache", "Service-owned cache root for minimal manifest-defined Rule Source payloads and provenance metadata.", "Administration", "path", null, null, "AdvancedOperator", ValidateOptionalText),
        Optional("EternalCycle:Rules:ReleaseChannel", "Rule Source release channel", "Selects stable releases or the approved prerelease discovery channel without asking users for refs or SHAs.", "Rules", "enum", "Prerelease", "Stable", "Operator", Allowed("Stable", "Prerelease"), ["Stable", "Prerelease"]),
        Optional("EternalCycle:Rules:GitSource:AcquisitionTimeout", "Rule acquisition timeout", "Overall bounded duration for one source acquisition attempt.", "Rules", "timespan", "00:05:00", "00:04:00", "AdvancedOperator", ValidatePositiveTimeSpan),
        Optional("EternalCycle:Rules:GitSource:ProcessTimeout", "Git process timeout", "Bound for an individual Git subprocess within source acquisition.", "Rules", "timespan", "00:02:00", "00:02:00", "AdvancedOperator", ValidatePositiveTimeSpan),
        Optional("EternalCycle:Rules:ManagedOperationTimeout", "Managed Operation timeout", "Service-owned bound for a durable administrative operation.", "Rules", "timespan", "00:30:00", "00:30:00", "AdvancedOperator", ValidatePositiveTimeSpan),
        Optional("EternalCycle:Rules:ManagedOperationPollInterval", "Managed Operation poll interval", "Worker delay while no durable operation is ready to run.", "Rules", "timespan", "00:00:01", "00:00:01", "Internal", ValidatePositiveTimeSpan),
        Optional("EternalCycle:Rules:ManagedOperationLeaseDuration", "Managed Operation execution lease", "Bounded durable ownership period renewed by the active worker so interrupted work can be reclaimed after host loss.", "Rules", "timespan", "00:00:30", "00:00:30", "AdvancedOperator", ValidatePositiveTimeSpan),
        Optional("EternalCycle:Diagnostics:VerboseErrors", "Verbose diagnostics", "Adds sanitized technical exception detail to structured tool failures for development and testing.", "Diagnostics", "boolean", "true", "false", "Development", ValidateBoolean),
        Optional("EternalCycle:Diagnostics:PersistenceTimeout", "Diagnostic persistence timeout", "Bound for database and fallback diagnostic writes.", "Diagnostics", "timespan", "00:00:10", "00:00:10", "AdvancedOperator", ValidatePositiveTimeSpan),
        Optional("EternalCycle:Diagnostics:FallbackLogFile", "Diagnostic fallback override", "Optional advanced override for the automatically established sanitized diagnostic safety net.", "Diagnostics", "path", null, null, "AdvancedOperator", ValidateOptionalText),
        Optional("EternalCycle:Diagnostics:DisableAutomaticFallback", "Disable automatic diagnostic fallback", "Explicitly disables the zero-configuration diagnostic safety net. Ordinary users should leave this false.", "Diagnostics", "boolean", "false", "false", "AdvancedOperator", ValidateBoolean)
    ];

    public ManagedConfigurationReport GetReport()
    {
        var settings = Definitions.Select(Evaluate).ToArray();
        var missing = settings.Where(value => value.ValidationStatus == ManagedConfigurationValidationStatus.Missing).ToArray();
        var invalid = settings.Where(value => value.ValidationStatus == ManagedConfigurationValidationStatus.Invalid).ToArray();
        var state = missing.Length > 0
            ? "ConfigurationRequired"
            : invalid.Length > 0
                ? "ConfigurationInvalid"
                : "Ready";
        var actions = missing
            .Select(value => $"Configure {value.EnvironmentVariableName} and restart the MCP service.")
            .Concat(invalid.Select(value => $"Correct {value.EnvironmentVariableName} and restart the MCP service."))
            .ToArray();
        return new ManagedConfigurationReport(
            state,
            state == "Ready",
            settings,
            actions,
            state switch
            {
                "ConfigurationRequired" => "Required Managed host configuration is missing. Use the returned exact environment-variable names rather than guessing keys.",
                "ConfigurationInvalid" => "Managed host configuration is present but invalid. Correct the reported settings before database setup.",
                _ => "The base Managed host configuration is valid. Database and rule readiness may now be evaluated."
            });
    }

    private ManagedConfigurationDescriptor Evaluate(ConfigurationDefinition definition)
    {
        var raw = configuration[definition.Key];
        var configured = raw is not null;
        var source = Environment.GetEnvironmentVariable(definition.EnvironmentVariableName) is not null
            ? "Environment"
            : configured
                ? "Configuration"
                : definition.DefaultValue is not null
                    ? "Default"
                    : "Missing";
        var effective = configured ? raw : definition.DefaultValue;
        ManagedConfigurationValidationStatus status;
        string message;
        if (string.IsNullOrWhiteSpace(effective) && definition.Required)
        {
            status = ManagedConfigurationValidationStatus.Missing;
            message = "A required value is missing.";
        }
        else
        {
            var validation = definition.Validator(effective);
            status = validation.Valid
                ? configured
                    ? ManagedConfigurationValidationStatus.Valid
                    : ManagedConfigurationValidationStatus.Defaulted
                : ManagedConfigurationValidationStatus.Invalid;
            message = validation.Message;
        }

        return new ManagedConfigurationDescriptor(
            definition.Key,
            definition.EnvironmentVariableName,
            definition.DisplayName,
            definition.Description,
            definition.Category,
            definition.Required,
            definition.Sensitive,
            configured,
            source,
            definition.ValueType,
            definition.Sensitive ? null : effective,
            definition.ExampleValue,
            definition.AllowedValues,
            definition.Sensitive ? null : definition.DefaultValue,
            RestartRequired: true,
            definition.Audience,
            status,
            message);
    }

    private static ConfigurationDefinition Required(
        string key,
        string displayName,
        string description,
        string category,
        string valueType,
        string? exampleValue,
        string audience,
        Func<string?, Validation> validator,
        bool sensitive = false) =>
        Definition(key, displayName, description, category, required: true, sensitive, valueType, exampleValue, null, audience, validator, []);

    private static ConfigurationDefinition Optional(
        string key,
        string displayName,
        string description,
        string category,
        string valueType,
        string? exampleValue,
        string? defaultValue,
        string audience,
        Func<string?, Validation> validator,
        IReadOnlyList<string>? allowedValues = null,
        bool sensitive = false) =>
        Definition(key, displayName, description, category, required: false, sensitive, valueType, exampleValue, defaultValue, audience, validator, allowedValues ?? []);

    private static ConfigurationDefinition Definition(
        string key,
        string displayName,
        string description,
        string category,
        bool required,
        bool sensitive,
        string valueType,
        string? exampleValue,
        string? defaultValue,
        string audience,
        Func<string?, Validation> validator,
        IReadOnlyList<string> allowedValues) =>
        new(
            key,
            key.Replace(":", "__", StringComparison.Ordinal),
            displayName,
            description,
            category,
            required,
            sensitive,
            valueType,
            exampleValue,
            defaultValue,
            audience,
            validator,
            allowedValues);

    private static Func<string?, Validation> Allowed(params string[] values) => value =>
        values.Contains(value, StringComparer.OrdinalIgnoreCase)
            ? Valid("The configured semantic value is supported.")
            : Invalid($"Expected one of: {string.Join(", ", values)}.");

    private static Validation ValidateConnectionString(string? value)
    {
        try
        {
            var parsed = new SqlConnectionStringBuilder(value);
            return string.IsNullOrWhiteSpace(parsed.DataSource) || string.IsNullOrWhiteSpace(parsed.InitialCatalog)
                ? Invalid("The connection string must identify both a server and database catalog.")
                : Valid("A sensitive connection string is configured and structurally valid.");
        }
        catch (ArgumentException)
        {
            return Invalid("The configured connection string is not structurally valid.");
        }
    }

    private static Validation ValidatePositiveInteger(string? value) =>
        int.TryParse(value, out var parsed) && parsed > 0
            ? Valid("The configured positive integer is valid.")
            : Invalid("Expected a positive integer.");

    private static Validation ValidateBoolean(string? value) =>
        bool.TryParse(value, out _)
            ? Valid("The configured boolean is valid.")
            : Invalid("Expected true or false.");

    private static Validation ValidatePositiveTimeSpan(string? value) =>
        TimeSpan.TryParse(value, out var parsed) && parsed > TimeSpan.Zero
            ? Valid("The configured duration is valid.")
            : Invalid("Expected a positive TimeSpan value such as 00:05:00.");

    private static Validation ValidateSchemaIdentifier(string? value)
    {
        try
        {
            _ = SqlServerSchemaIdentifier.Validate(value ?? string.Empty);
            return Valid("The configured SQL identifier is valid.");
        }
        catch (ArgumentException)
        {
            return Invalid("Expected a SQL identifier beginning with a letter or underscore and containing only letters, digits, or underscores.");
        }
    }

    private static Validation ValidateRequiredText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Invalid("A non-empty value is required.") : Valid("The configured value is valid.");

    private static Validation ValidateOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Valid("No override is configured.") : Valid("An optional override is configured.");

    private static Validation Valid(string message) => new(true, message);

    private static Validation Invalid(string message) => new(false, message);

    private sealed record ConfigurationDefinition(
        string Key,
        string EnvironmentVariableName,
        string DisplayName,
        string Description,
        string Category,
        bool Required,
        bool Sensitive,
        string ValueType,
        string? ExampleValue,
        string? DefaultValue,
        string Audience,
        Func<string?, Validation> Validator,
        IReadOnlyList<string> AllowedValues);

    private sealed record Validation(bool Valid, string Message);
}

[McpServerToolType]
public sealed class ManagedConfigurationTools(IManagedConfigurationService configuration)
{
    [McpServerTool(Name = "ec_get_configuration_requirements", ReadOnly = true, Idempotent = true),
     Description("Returns the running MCP executable's safe structured configuration contract, exact environment-variable names, validation state, defaults, audiences, and restart requirements without exposing secret values or requiring a database.")]
    public ManagedConfigurationReport GetConfigurationRequirements() => configuration.GetReport();
}
