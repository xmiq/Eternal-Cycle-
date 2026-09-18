using System.ComponentModel;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public sealed record EternalCycleErrorDefinition(
    string Code,
    string Title,
    string SafeDescription,
    string Category,
    string Severity,
    string RetrySemantics,
    bool UserActionMayResolve,
    bool AdministrativeActionMayBeRequired,
    string RecoveryGuidance,
    IReadOnlyList<string> RelatedCodes,
    string DocumentationGuidance,
    string IntroducedVersion,
    string? DeprecatedBy = null);

public sealed record EternalCycleErrorLookup(
    bool Found,
    string RequestedCode,
    string Message,
    EternalCycleErrorDefinition? Error);

public static class EternalCycleErrorRegistry
{
    private static readonly IReadOnlyDictionary<string, EternalCycleErrorDefinition> Entries =
        CreateEntries().ToDictionary(value => value.Code, StringComparer.Ordinal);

    public static IReadOnlyList<EternalCycleErrorDefinition> List(string? category = null, int maximumCount = 200) =>
        Entries.Values
            .Where(value => string.IsNullOrWhiteSpace(category) ||
                string.Equals(value.Category, category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(value => value.Code, StringComparer.Ordinal)
            .Take(Math.Clamp(maximumCount, 1, 500))
            .ToArray();

    public static EternalCycleErrorLookup Get(string? code)
    {
        var requested = string.IsNullOrWhiteSpace(code) ? string.Empty : code.Trim().ToUpperInvariant();
        return Entries.TryGetValue(requested, out var definition)
            ? new(true, requested, "The error code is registered by this Eternal Cycle build.", definition)
            : new(false, requested, "This error code is not registered by this Eternal Cycle build.", null);
    }

    public static bool Contains(string code) => Entries.ContainsKey(code);

    private static IEnumerable<EternalCycleErrorDefinition> CreateEntries()
    {
        yield return Def("ADMINISTRATION_DISABLED", "Administrative tools disabled", "Permission-gated administration is disabled by deployment configuration.", "Administration", "Error", "RetryAfterConfiguration", true, true, "Enable administration in the MCP host configuration, restart, and retry after user approval.");
        yield return Def("ADMINISTRATIVE_INTERVENTION_REQUIRED", "Operator intervention required", "The deployment requires an operator-only action in addition to ordinary user approval.", "Administration", "Warning", "NotRetrySafe", false, true, "Ask the deployment operator to complete the stated action.");
        yield return Def("CAMPAIGN_NOT_FOUND", "Campaign not found", "The requested stable campaign identity does not exist in the selected Data Namespace.", "Campaign", "Error", "RetryAfterInput", true, false, "List campaigns or use the permission-gated campaign-creation path.");
        yield return Def("CAMPAIGN_SCHEMA_MISSING", "Campaign schema missing", "Required Eternal Cycle campaign persistence structures are absent.", "Migration", "Error", "RetryAfterMigration", true, true, "Preview setup and initialize after explicit user approval.", ["MIGRATION_REQUIRED"]);
        yield return Def("CONFIGURATION_REQUIRED", "Host configuration required", "A required MCP host setting is missing.", "Configuration", "Error", "RetryAfterConfiguration", true, true, "Use ec_get_configuration_requirements, configure the exact returned key, and restart the MCP.");
        yield return Def("CONFIGURATION_INVALID", "Host configuration invalid", "A configured MCP host setting has an invalid value or format.", "Configuration", "Error", "RetryAfterConfiguration", true, true, "Use ec_get_configuration_requirements, correct the reported setting, and restart the MCP.");
        yield return Def("DIAGNOSTIC_PERSISTENCE_FAILED", "Diagnostic persistence failed", "Neither the preferred diagnostic store nor the sanitized fallback could preserve complete evidence.", "Diagnostics", "Warning", "RetryAfterRecovery", false, true, "Use the correlation ID and error dump; inspect fallback initialization status.");
        yield return Def("INITIALIZATION_FAILED", "Initialization failed", "A supported Eternal Cycle initialization or migration did not complete.", "Migration", "Error", "RetryAfterRecovery", false, true, "Use diagnostics and the error dump before retrying initialization.");
        yield return Def("INTERNAL_ERROR", "Internal error", "An unexpected failure reached the final Eternal Cycle tool boundary.", "Internal", "Error", "Unknown", false, false, "Look up this code and request a sanitized error dump using the returned correlation ID.");
        yield return Def("INVALID_CAMPAIGN_METADATA", "Invalid campaign metadata", "Campaign identity metadata did not satisfy the reference service contract.", "Campaign", "Error", "RetryAfterInput", true, false, "Correct the campaign name or description and retry without changing unrelated state.");
        yield return Def("INVALID_OPERATION_ID", "Invalid operation identifier", "The supplied Managed Operation identifier is empty, malformed, or too long.", "ManagedOperation", "Error", "RetryAfterInput", true, false, "Use the exact operation ID returned by the initiating action or list recent operations.");
        yield return Def("MANAGED_OPERATION_FAILED", "Managed Operation failed", "A durable Managed Operation ended in failure.", "ManagedOperation", "Error", "DependsOnOperation", false, false, "Inspect operation status, error lookup, and the correlated error dump before retrying.");
        yield return Def("MANAGED_OPERATION_INTERRUPTED", "Managed Operation interrupted", "Service shutdown interrupted durable work and left it eligible for deterministic recovery.", "ManagedOperation", "Warning", "RetrySafe", false, false, "Restart the service and query the same operation ID.");
        yield return Def("MANAGED_OPERATION_KIND_UNSUPPORTED", "Managed Operation kind unsupported", "The running service cannot execute the persisted operation kind.", "ManagedOperation", "Error", "NotRetrySafe", false, true, "Upgrade or repair the service deployment without deleting the durable operation record.");
        yield return Def("MANAGED_OPERATION_NOT_FOUND", "Managed Operation not found", "No durable Managed Operation matches the supplied identifier.", "ManagedOperation", "Error", "RetryAfterInput", true, false, "List recent operations and use a returned stable identifier.");
        yield return Def("MANAGED_OPERATION_TIMEOUT", "Managed Operation timeout", "Durable work exceeded the service-owned execution bound.", "ManagedOperation", "Error", "RetrySafe", false, false, "Inspect causal diagnostics, then retry the durable operation if status permits.");
        yield return Def("MANAGED_OPERATIONS_UNAVAILABLE", "Managed Operations unavailable", "The deployment does not currently expose the durable operation service.", "ManagedOperation", "Error", "RetryAfterRecovery", false, true, "Repair or migrate the Managed service before publishing rules.");
        yield return Def("MIGRATION_REQUIRED", "Migration required", "Supported older Eternal Cycle structures must be upgraded before dependent operations continue.", "Migration", "Error", "RetryAfterMigration", true, true, "Call ec_get_setup_plan, obtain explicit user approval, then call ec_initialize_service.");
        yield return Def("MIGRATION_VALIDATION_FAILED", "Migration validation failed", "Migration execution did not produce the expected current Eternal Cycle structures.", "Migration", "Error", "NotRetrySafe", false, true, "Stop dependent operations and inspect the correlated diagnostic dump.");
        yield return Def("NO_ACTIVE_RULE_RELEASE", "No active Rule Release", "A published compatible Rule Release has not been activated.", "Rules", "Error", "RetryAfterRecovery", true, true, "Complete or inspect the durable rule-publication operation.");
        yield return Def("NO_PUBLISHED_RULE_RELEASE", "No published Rule Release", "The selected Rule Source has not produced a validated Rule Release.", "Rules", "Error", "RetryAfterRecovery", true, true, "Queue initial publication only after configuration and migration are ready.");
        yield return Def("OFFICIAL_SOURCE_METADATA_INVALID", "Official source metadata invalid", "Packaged official Rule Source metadata is present but invalid.", "Rules", "Error", "NotRetrySafe", false, true, "Repair the installation or configure a compatible custom Rule Source.");
        yield return Def("OFFICIAL_SOURCE_METADATA_MISSING", "Official source metadata missing", "The packaged official Rule Source metadata file could not be found.", "Rules", "Error", "NotRetrySafe", false, true, "Repair the installation or configure a compatible custom Rule Source.");
        yield return Def("PERSISTENCE_INSPECTION_FAILED", "Persistence inspection failed", "The service reached persistence but could not classify its structures safely.", "Persistence", "Error", "RetryAfterRecovery", false, true, "Use service diagnostics and the correlation ID before changing schema state.");
        yield return Def("PERSISTENCE_UNAVAILABLE", "Persistence unavailable", "The configured Managed persistence target cannot be reached.", "Persistence", "Error", "RetryAfterRecovery", true, true, "Verify host configuration and service availability, then retry readiness.");
        yield return Def("ROUTING_CONFIGURATION_INVALID", "Routing configuration invalid", "Trusted World Model or Data Namespace routing configuration is incomplete or invalid.", "Configuration", "Error", "RetryAfterConfiguration", true, true, "Inspect configuration requirements and correct trusted routing values.");
        yield return Def("RULE_ACTIVATION_FAILED", "Rule activation failed", "A validated Rule Release could not be activated atomically.", "Rules", "Error", "RetryAfterRecovery", false, true, "Inspect the durable operation and correlated diagnostics before retrying.");
        yield return Def("RULE_CLOSURE_PENDING", "Rule closure pending", "The minimum authoritative rule closure is still being prepared.", "Rules", "Warning", "RetryLater", false, false, "Wait for the active Managed Operation; never guess missing rules.");
        yield return Def("RULE_COMPILATION_FAILED", "Rule compilation failed", "Canonical source material could not be compiled into a valid candidate Rule Release.", "Rules", "Error", "DependsOnCause", false, true, "Inspect source compatibility and correlated diagnostics.");
        yield return Def("RULE_CONTEXT_PENDING", "Rule context pending", "Required authoritative rule sources are known but not yet prepared.", "Rules", "Warning", "RetryLater", false, false, "Query operation progress and retry context retrieval later.");
        yield return Def("RULE_CONTEXT_UNAVAILABLE", "Rule context unavailable", "Required authoritative rule context cannot currently be retrieved.", "Rules", "Error", "DependsOnCause", false, false, "Do not guess; inspect readiness and causal diagnostics.");
        yield return Def("RULE_MINIMUM_CLOSURE_FAILED", "Minimum rule closure failed", "Preparation of the minimum authoritative gameplay closure failed.", "Rules", "Error", "DependsOnCause", false, true, "Inspect the durable publication operation before gameplay.");
        yield return Def("RULE_PUBLICATION_CANCELLED", "Rule publication cancelled", "Rule publication was cancelled before activation.", "Rules", "Warning", "RetrySafe", true, false, "Retry through a new or recovered durable operation when appropriate.");
        yield return Def("RULE_PUBLICATION_FAILED", "Rule publication failed", "The durable rule-publication workflow failed.", "Rules", "Error", "DependsOnCause", false, false, "Inspect operation status and correlated diagnostics.");
        yield return Def("RULE_PUBLICATION_STATE_INVALID", "Rule publication state invalid", "A stored Rule Release state violates the publication lifecycle.", "Rules", "Error", "NotRetrySafe", false, true, "Stop activation and inspect persistence integrity.");
        yield return Def("RULE_REMAINDER_PREPARATION_FAILED", "Remaining rule preparation failed", "Minimum gameplay closure may exist, but remaining rule preparation failed.", "Rules", "Warning", "RetrySafe", false, false, "Continue only if readiness permits and retry background preparation.");
        yield return Def("RULE_SCHEMA_MISSING", "Rule Domain schema missing", "Required Rule Domain structures are absent.", "Migration", "Error", "RetryAfterMigration", true, true, "Preview setup and initialize after explicit user approval.", ["MIGRATION_REQUIRED"]);
        yield return Def("RULE_SOURCE_ACQUISITION_FAILED", "Rule Source acquisition failed", "The configured source could not be acquired.", "RuleSource", "Error", "DependsOnCause", false, false, "Inspect causal diagnostics; do not infer a network cause without evidence.");
        yield return Def("RULE_SOURCE_CACHE_INVALID", "Rule Source cache invalid", "The service-owned Rule Source cache is incomplete or inconsistent.", "RuleSource", "Error", "RetryAfterRecovery", false, true, "Allow the service to repair or recreate its managed cache.");
        yield return Def("RULE_SOURCE_CONFIGURATION_FAILED", "Rule Source configuration failed", "The requested Rule Source selection could not be validated or persisted.", "RuleSource", "Error", "DependsOnCause", true, true, "Confirm migration readiness and inspect the returned diagnostic reference.");
        yield return Def("RULE_SOURCE_INCOMPATIBLE", "Rule Source incompatible", "The acquired source does not satisfy Eternal Cycle manifest or compiler compatibility.", "RuleSource", "Error", "NotRetrySafe", true, true, "Select a compatible source or approved channel.");
        yield return Def("RULE_SOURCE_NOT_CONFIGURED", "Rule Source not configured", "No approved Rule Source selection exists.", "RuleSource", "Error", "RetryAfterConfiguration", true, true, "Select Stable or Prerelease after database setup completes.");
        yield return Def("RULE_SOURCE_OPERATION_FAILED", "Rule Source operation failed", "A source-control subprocess failed with sanitized causal evidence.", "RuleSource", "Error", "DependsOnCause", false, false, "Inspect the error dump; retry only when its recovery metadata permits.");
        yield return Def("RULE_SOURCE_OPERATION_TIMEOUT", "Rule Source operation timeout", "A source-control subprocess exceeded its configured bound.", "RuleSource", "Error", "RetrySafe", true, false, "Increase the supported acquisition timeout when appropriate or retry after checking service conditions.");
        yield return Def("RULE_SOURCE_PROVIDER_UNSUPPORTED", "Rule Source provider unsupported", "The configured provider kind is not supported by this service build.", "RuleSource", "Error", "NotRetrySafe", true, true, "Choose a provider supported by this executable.");
        yield return Def("RULE_SOURCE_READ_FAILED", "Rule Source read failed", "Required source objects could not be read from the resolved immutable source.", "RuleSource", "Error", "DependsOnCause", false, false, "Inspect causal diagnostics and source integrity.");
        yield return Def("RULE_SOURCE_REF_RESOLUTION_FAILED", "Rule Source ref resolution failed", "The configured discovery reference could not be resolved to immutable provenance.", "RuleSource", "Error", "DependsOnCause", true, false, "Verify the selected channel/source and retry when diagnostics permit.");
        yield return Def("RULE_SOURCE_UNAVAILABLE", "Rule Source unavailable", "The configured Rule Source is unavailable under current observed conditions.", "RuleSource", "Error", "DependsOnCause", false, false, "Use authoritative diagnostics; do not guess whether network, credentials, or source state caused it.");
        yield return Def("RULE_SOURCE_VERSION_METADATA_INVALID", "Rule Source version metadata invalid", "Prerelease or release provenance metadata is incomplete or inconsistent.", "RuleSource", "Error", "NotRetrySafe", false, true, "Repair source metadata without changing immutable source identity.");
        yield return Def("RULE_STORE_STAGE_FAILED", "Rule store staging failed", "A candidate Rule Release could not be staged durably.", "Rules", "Error", "DependsOnCause", false, true, "Inspect persistence and correlated diagnostics before retrying.");
        yield return Def("RULE_UPDATE_CHECK_RECORD_FAILED", "Rule update check recording failed", "The service could not preserve the outcome of a rule update check.", "Rules", "Warning", "RetrySafe", false, true, "Inspect diagnostic persistence before scheduling another update.");
        yield return Def("RULE_VALIDATION_FAILED", "Rule validation failed", "A compiled candidate did not satisfy Eternal Cycle validation.", "Rules", "Error", "NotRetrySafe", false, true, "Correct the source or compiler incompatibility; do not activate the candidate.");
        yield return Def("RULESET_INCOMPATIBLE", "RuleSet incompatible", "The active or candidate Rule Release is incompatible with the configured RuleSet version.", "Rules", "Error", "NotRetrySafe", true, true, "Select and publish a compatible Rule Source.");
        yield return Def("SETUP_REQUIRED", "Setup required", "Required Eternal Cycle-owned structures or setup state are absent.", "Migration", "Error", "RetryAfterMigration", true, true, "Preview setup and initialize after explicit user approval.");
        yield return Def("TOOL_OPERATION_CANCELLED", "Tool operation cancelled", "The caller or host cancelled the tool before completion.", "Runtime", "Warning", "RetrySafe", true, false, "Retry only if the underlying operation is idempotent or query durable status first.");
        yield return Def("USER_APPROVAL_REQUIRED", "User approval required", "The requested administrative mutation requires explicit informed user approval.", "Administration", "Warning", "RetryAfterApproval", true, false, "Explain the action, obtain natural conversational approval, then set userApproved=true.");
    }

    private static EternalCycleErrorDefinition Def(
        string code,
        string title,
        string description,
        string category,
        string severity,
        string retry,
        bool userAction,
        bool administrativeAction,
        string recovery,
        IReadOnlyList<string>? related = null) =>
        new(
            code,
            title,
            description,
            category,
            severity,
            retry,
            userAction,
            administrativeAction,
            recovery,
            related ?? [],
            "Use ec_get_error_code for current safe guidance and ec_get_error_dump with a correlation ID when evidence is needed.",
            "post-v1");
}

[McpServerToolType]
public sealed class EternalCycleErrorCodeTools
{
    [McpServerTool(Name = "ec_list_error_codes", ReadOnly = true, Idempotent = true),
     Description("Lists the safe error-code registry built into this MCP executable without requiring a database, campaign, rule source, or diagnostics store.")]
    public IReadOnlyList<EternalCycleErrorDefinition> ListErrorCodes(
        [Description("Optional subsystem/category filter.")] string? category,
        [Description("Maximum results from 1 through 500.")] int maximumCount = 200) =>
        EternalCycleErrorRegistry.List(category, maximumCount);

    [McpServerTool(Name = "ec_get_error_code", ReadOnly = true, Idempotent = true),
     Description("Looks up one stable Eternal Cycle error code and returns safe recovery guidance. Unknown codes return found=false instead of throwing.")]
    public EternalCycleErrorLookup GetErrorCode(
        [Description("Stable machine-readable Eternal Cycle error code.")] string? code) =>
        EternalCycleErrorRegistry.Get(code);
}

public static class EmergencyErrorText
{
    public static string Create(string code, string? correlationId, string? component) =>
        $"ETERNAL_CYCLE_ERROR {Safe(code, "INTERNAL_ERROR")}\n" +
        $"Reference: {Safe(correlationId, "unavailable")}\n" +
        $"Component: {Safe(component, "MCP tool boundary")}\n" +
        "Help: use ec_get_error_code or ec_get_error_dump.";

    private static string Safe(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var safe = value.Replace('\r', ' ').Replace('\n', ' ').Trim();
        return safe.Length <= 256 ? safe : safe[..256];
    }
}
