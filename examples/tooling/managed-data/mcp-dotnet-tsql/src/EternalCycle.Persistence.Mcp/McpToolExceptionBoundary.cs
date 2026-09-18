using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;

namespace EternalCycle.Persistence.Mcp;

public sealed record ManagedDeveloperFailureDetail(
    string ExceptionType,
    string ExceptionMessage,
    string? InnerExceptionChain,
    string? StackTrace);

public sealed record ManagedToolFailure(
    bool Success,
    string Code,
    string Message,
    bool RetrySafe,
    bool UserApprovalRequired,
    bool AdministrativeInterventionRequired,
    string CorrelationId,
    string Component,
    string Stage,
    string DiagnosticsAvailability,
    string? OperationId,
    string? BlockingCondition,
    string? RequiredAction,
    IReadOnlyList<string> AllowedNextActions,
    string? RecommendedNextAction,
    ManagedDeveloperFailureDetail? DeveloperDiagnostics);

public interface IMcpToolExceptionBoundary
{
    Task<CallToolResult> CreateFailureAsync(
        string toolName,
        string correlationId,
        Exception exception,
        CancellationToken cancellationToken);
}

public sealed class McpToolExceptionBoundary(
    IManagedDiagnosticRecorder diagnostics,
    IRecentManagedErrorStore recentErrors,
    IOptions<ManagedDiagnosticsOptions> options) : IMcpToolExceptionBoundary
{
    private readonly ManagedDiagnosticsOptions settings = options.Value;

    public async Task<CallToolResult> CreateFailureAsync(
        string toolName,
        string correlationId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var failure = Classify(exception);
        var receipt = await diagnostics.RecordFailureAsync(
            new ManagedDiagnosticContext(
                correlationId,
                toolName,
                RulePublicationStage.ToolInvocation,
                failure.Code,
                DateTimeOffset.UtcNow,
                RetrySafe: failure.RetrySafe,
                AdministrativeInterventionRequired: failure.AdministrativeInterventionRequired,
                SafeDetail: failure.Message),
            exception,
            cancellationToken);
        var recovery = RecoveryFor(failure.Code);
        var developer = settings.VerboseErrors
            ? new ManagedDeveloperFailureDetail(
                exception.GetType().FullName ?? exception.GetType().Name,
                DiagnosticRedactor.Redact(exception.Message) ?? string.Empty,
                BuildInnerChain(exception.InnerException),
                DiagnosticRedactor.Redact(exception.StackTrace))
            : null;
        var result = new ManagedToolFailure(
            false,
            failure.Code,
            failure.Message,
            failure.RetrySafe,
            UserApprovalRequired: false,
            failure.AdministrativeInterventionRequired,
            correlationId,
            "ManagedTool",
            toolName,
            receipt.Availability,
            OperationId: null,
            recovery.BlockingCondition,
            recovery.RequiredAction,
            recovery.AllowedNextActions,
            recovery.RecommendedNextAction,
            developer);
        recentErrors.Add(new RecentManagedError(DateTimeOffset.UtcNow, toolName, result));
        var json = JsonSerializer.Serialize(result);
        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = json }],
            StructuredContent = JsonSerializer.SerializeToElement(result),
            IsError = true
        };
    }

    private static ClassifiedFailure Classify(Exception exception) => exception switch
    {
        ManagedServiceException managed => new(
            managed.Code,
            managed.SafeMessage,
            RetrySafe: false,
            AdministrativeInterventionRequired: false),
        OperationCanceledException => new(
            "TOOL_OPERATION_CANCELLED",
            "The Managed operation was cancelled before it completed.",
            RetrySafe: true,
            AdministrativeInterventionRequired: false),
        _ => new(
            "INTERNAL_ERROR",
            "Eternal Cycle encountered an internal error. Use the correlation ID with error lookup or a sanitized error dump.",
            RetrySafe: false,
            AdministrativeInterventionRequired: false)
    };

    private static RecoveryGuidance RecoveryFor(string code) => code switch
    {
        "MIGRATION_REQUIRED" => new(
            "MigrationRequired",
            "Preview the supported migration and initialize the service after explicit user approval.",
            ["ec_get_setup_plan", "ec_initialize_service", "ec_get_diagnostics"],
            "ec_get_setup_plan"),
        "CONFIGURATION_REQUIRED" or "CONFIGURATION_INVALID" => new(
            code == "CONFIGURATION_REQUIRED" ? "ConfigurationRequired" : "ConfigurationInvalid",
            "Read the executable's configuration requirements, correct host configuration, and restart the MCP service.",
            ["ec_get_configuration_requirements", "ec_service_capabilities"],
            "ec_get_configuration_requirements"),
        "RULE_SOURCE_UNAVAILABLE" or "RULE_SOURCE_ACQUISITION_FAILED" or "RULE_SOURCE_OPERATION_TIMEOUT" => new(
            "RuleSourceUnavailable",
            "Inspect causal diagnostics or the durable operation; do not retry unrelated setup or publication tools blindly.",
            ["ec_get_diagnostics", "ec_get_operation_status", "ec_get_error_dump"],
            "ec_get_diagnostics"),
        _ => new(
            null,
            EternalCycleErrorRegistry.Get(code).Error?.RecoveryGuidance,
            ["ec_get_error_code", "ec_get_error_dump"],
            "ec_get_error_code")
    };

    private static string? BuildInnerChain(Exception? exception)
    {
        if (exception is null)
        {
            return null;
        }

        var entries = new List<string>();
        for (var current = exception; current is not null; current = current.InnerException)
        {
            entries.Add($"{current.GetType().FullName}: {current.Message}");
        }

        return DiagnosticRedactor.Redact(string.Join(" -> ", entries));
    }

    private sealed record ClassifiedFailure(
        string Code,
        string Message,
        bool RetrySafe,
        bool AdministrativeInterventionRequired);

    private sealed record RecoveryGuidance(
        string? BlockingCondition,
        string? RequiredAction,
        IReadOnlyList<string> AllowedNextActions,
        string? RecommendedNextAction);
}

public static class McpToolFailureContract
{
    public static string NewCorrelationId() => $"EC-{Guid.NewGuid():N}";

    public static CallToolResult EnrichHandledFailure(
        CallToolResult result,
        string toolName,
        string correlationId,
        IRecentManagedErrorStore? recentErrors)
    {
        if (result.StructuredContent is not JsonElement element ||
            element.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        var node = JsonNode.Parse(element.GetRawText())?.AsObject();
        if (node is null || !TryBoolean(node, "success", out var success) || success)
        {
            return result;
        }

        var code = ReadString(node, "code") ?? "INTERNAL_ERROR";
        var definition = EternalCycleErrorRegistry.Get(code).Error ?? EternalCycleErrorRegistry.Get("INTERNAL_ERROR").Error!;
        var recovery = RecoveryFor(code, definition);
        SetIfMissing(node, "correlationId", correlationId);
        SetIfMissing(node, "retrySafe", definition.RetrySemantics is "RetrySafe" or "RetryLater" or "RetryAfterRecovery");
        SetIfMissing(node, "administrativeInterventionRequired", definition.AdministrativeActionMayBeRequired);
        SetIfMissing(node, "blockingCondition", recovery.BlockingCondition);
        SetIfMissing(node, "requiredAction", recovery.RequiredAction);
        SetIfMissing(
            node,
            "allowedNextActions",
            new JsonArray(recovery.AllowedNextActions
                .Select(action => (JsonNode?)JsonValue.Create(action))
                .ToArray()));
        SetIfMissing(node, "recommendedNextAction", recovery.RecommendedNextAction);
        var enriched = JsonSerializer.SerializeToElement(node);
        var json = enriched.GetRawText();
        result.StructuredContent = enriched;
        result.Content = [new TextContentBlock { Text = json }];
        result.IsError = true;

        var failure = new ManagedToolFailure(
            false,
            code,
            ReadString(node, "message") ?? definition.SafeDescription,
            ReadBoolean(node, "retrySafe"),
            ReadBoolean(node, "userApprovalRequired"),
            ReadBoolean(node, "administrativeInterventionRequired"),
            ReadString(node, "correlationId") ?? correlationId,
            "ManagedTool",
            toolName,
            ReadString(node, "diagnosticsAvailability") ?? "NotRequested",
            ReadString(node, "operationId"),
            ReadString(node, "blockingCondition"),
            ReadString(node, "requiredAction"),
            ReadStrings(node, "allowedNextActions"),
            ReadString(node, "recommendedNextAction"),
            null);
        recentErrors?.Add(new RecentManagedError(DateTimeOffset.UtcNow, toolName, failure));
        return result;
    }

    private static bool TryBoolean(JsonObject node, string name, out bool value)
    {
        var property = Find(node, name);
        if (property?.GetValueKind() is JsonValueKind.True or JsonValueKind.False)
        {
            value = property.GetValue<bool>();
            return true;
        }

        value = false;
        return false;
    }

    private static bool ReadBoolean(JsonObject node, string name) =>
        TryBoolean(node, name, out var value) && value;

    private static string? ReadString(JsonObject node, string name)
    {
        var property = Find(node, name);
        return property?.GetValueKind() == JsonValueKind.String ? property.GetValue<string>() : null;
    }

    private static IReadOnlyList<string> ReadStrings(JsonObject node, string name) =>
        Find(node, name) is JsonArray values
            ? values.Select(value => value?.GetValue<string>()).Where(value => value is not null).Select(value => value!).ToArray()
            : [];

    private static JsonNode? Find(JsonObject node, string name) =>
        node.FirstOrDefault(pair => string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase)).Value;

    private static void SetIfMissing(JsonObject node, string name, JsonNode? value)
    {
        var pair = node.FirstOrDefault(entry => string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase));
        if (pair.Key is null || pair.Value is null || pair.Value.GetValueKind() == JsonValueKind.Null)
        {
            node[pair.Key ?? name] = value;
        }
    }

    private static RecoveryMetadata RecoveryFor(
        string code,
        EternalCycleErrorDefinition definition) => code switch
    {
        "MIGRATION_REQUIRED" or "SETUP_REQUIRED" or "CAMPAIGN_SCHEMA_MISSING" or "RULE_SCHEMA_MISSING" => new(
            code == "MIGRATION_REQUIRED" ? "MigrationRequired" : "SetupRequired",
            definition.RecoveryGuidance,
            ["ec_get_setup_plan", "ec_initialize_service", "ec_get_diagnostics"],
            "ec_get_setup_plan"),
        "CONFIGURATION_REQUIRED" or "CONFIGURATION_INVALID" => new(
            code == "CONFIGURATION_REQUIRED" ? "ConfigurationRequired" : "ConfigurationInvalid",
            definition.RecoveryGuidance,
            ["ec_get_configuration_requirements", "ec_service_capabilities"],
            "ec_get_configuration_requirements"),
        "RULE_SOURCE_UNAVAILABLE" or "RULE_SOURCE_ACQUISITION_FAILED" or "RULE_SOURCE_OPERATION_TIMEOUT" => new(
            "RuleSourceUnavailable",
            definition.RecoveryGuidance,
            ["ec_get_diagnostics", "ec_get_operation_status", "ec_get_error_dump"],
            "ec_get_diagnostics"),
        _ => new(
            null,
            definition.RecoveryGuidance,
            ["ec_get_error_code", "ec_get_error_dump"],
            "ec_get_error_code")
    };

    private sealed record RecoveryMetadata(
        string? BlockingCondition,
        string RequiredAction,
        IReadOnlyList<string> AllowedNextActions,
        string RecommendedNextAction);
}

public interface IMcpToolInvocationGuard
{
    Task<CallToolResult> InvokeAsync(
        string toolName,
        Func<Task<CallToolResult>> invocation,
        CancellationToken cancellationToken);
}

public sealed class McpToolInvocationGuard(
    IMcpToolExceptionBoundary boundary,
    IRecentManagedErrorStore recentErrors) : IMcpToolInvocationGuard
{
    public async Task<CallToolResult> InvokeAsync(
        string toolName,
        Func<Task<CallToolResult>> invocation,
        CancellationToken cancellationToken)
    {
        var correlationId = McpToolFailureContract.NewCorrelationId();
        try
        {
            var result = await invocation();
            return McpToolFailureContract.EnrichHandledFailure(
                result,
                toolName,
                correlationId,
                recentErrors);
        }
        catch (Exception exception)
        {
            try
            {
                return await boundary.CreateFailureAsync(
                    toolName,
                    correlationId,
                    exception,
                    cancellationToken);
            }
            catch
            {
                return new CallToolResult
                {
                    Content = [new TextContentBlock { Text = EmergencyErrorText.Create("INTERNAL_ERROR", correlationId, toolName) }],
                    IsError = true
                };
            }
        }
    }
}
