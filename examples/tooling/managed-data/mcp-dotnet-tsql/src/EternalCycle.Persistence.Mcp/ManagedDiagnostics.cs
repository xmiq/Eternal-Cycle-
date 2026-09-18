using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public enum RulePublicationStage
{
    ToolInvocation,
    AcquireSource,
    ResolveRef,
    ReadManifest,
    ReadRuleDocuments,
    Compile,
    Validate,
    Stage,
    PrepareMinimumClosure,
    Publish,
    Activate,
    PrepareRemainingRules,
    RecordUpdateCheck,
    Cancelled
}

public sealed class ManagedDiagnosticsOptions
{
    public bool VerboseErrors { get; init; }

    public string FallbackLogFile { get; init; } = string.Empty;

    public bool DisableAutomaticFallback { get; init; }

    public TimeSpan PersistenceTimeout { get; init; } = TimeSpan.FromSeconds(10);
}

public enum SanitizedDiagnosticFallbackStatus
{
    ConfiguredAutomatically,
    ConfiguredByOverride,
    ExplicitlyDisabled,
    InitializationFailed
}

public sealed record SanitizedDiagnosticFallbackState(
    SanitizedDiagnosticFallbackStatus Status,
    string? ResolvedPath,
    string Detail)
{
    public bool Available => Status is
        SanitizedDiagnosticFallbackStatus.ConfiguredAutomatically or
        SanitizedDiagnosticFallbackStatus.ConfiguredByOverride;
}

public static class SanitizedDiagnosticFallbackBootstrap
{
    public static SanitizedDiagnosticFallbackState Establish(
        IConfiguration configuration,
        string? automaticBaseDirectory = null)
    {
        if (bool.TryParse(
                configuration["EternalCycle:Diagnostics:DisableAutomaticFallback"],
                out var disabled) &&
            disabled)
        {
            return new(
                SanitizedDiagnosticFallbackStatus.ExplicitlyDisabled,
                null,
                "The deployment explicitly disabled the automatic sanitized diagnostic fallback.");
        }

        var configured = configuration["EternalCycle:Diagnostics:FallbackLogFile"];
        var path = !string.IsNullOrWhiteSpace(configured)
            ? Path.GetFullPath(configured)
            : automaticBaseDirectory is null
                ? PhysicalManagedDiagnosticFileSink.ResolvePath(string.Empty)
                : Path.Combine(
                    Path.GetFullPath(automaticBaseDirectory),
                    "EternalCycle",
                    "logs",
                    "managed-diagnostics.jsonl");
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var stream = new FileStream(
                path,
                FileMode.OpenOrCreate,
                FileAccess.Write,
                FileShare.ReadWrite);
            return new(
                string.IsNullOrWhiteSpace(configured)
                    ? SanitizedDiagnosticFallbackStatus.ConfiguredAutomatically
                    : SanitizedDiagnosticFallbackStatus.ConfiguredByOverride,
                path,
                string.IsNullOrWhiteSpace(configured)
                    ? "A writable per-user sanitized diagnostic fallback was established automatically."
                    : "A writable operator-overridden sanitized diagnostic fallback was established.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return new(
                SanitizedDiagnosticFallbackStatus.InitializationFailed,
                path,
                $"Automatic sanitized diagnostic fallback initialization failed with {exception.GetType().Name}.");
        }
    }
}

public sealed record ManagedDiagnosticContext(
    string CorrelationId,
    string Operation,
    RulePublicationStage Stage,
    string ErrorCode,
    DateTimeOffset StartedAt,
    string? RulesetId = null,
    string? RuleReleaseId = null,
    string? SourceIdentity = null,
    string? CampaignId = null,
    string Outcome = "Failed",
    bool? RetrySafe = null,
    bool? AdministrativeInterventionRequired = null,
    RuleSourceReleaseChannel? ReleaseChannel = null,
    string? DiscoveryRef = null,
    string? SafeDetail = null,
    string? OperationId = null);

public sealed record ManagedOperationDiagnostic(
    string DiagnosticId,
    string CorrelationId,
    DateTimeOffset RecordedAt,
    string Operation,
    string Stage,
    string ErrorCode,
    string ExceptionType,
    string SanitizedExceptionMessage,
    string? SanitizedInnerExceptionChain,
    string? SanitizedStackTrace,
    string EternalCycleVersion,
    string ImplementationVersion,
    string? RulesetId,
    string? RuleReleaseId,
    string? SourceIdentity,
    string? CampaignId,
    long DurationMilliseconds,
    string Outcome,
    bool? RetrySafe = null,
    bool? AdministrativeInterventionRequired = null,
    string? ReleaseChannel = null,
    string? DiscoveryRef = null,
    string? SafeDetail = null,
    string? OperationId = null);

public sealed record ManagedDiagnosticReceipt(
    string CorrelationId,
    string Availability,
    bool DatabasePersisted,
    bool FallbackFilePersisted);

public interface IManagedDiagnosticStore
{
    Task WriteAsync(ManagedOperationDiagnostic diagnostic, CancellationToken cancellationToken);
}

public interface IManagedDiagnosticFileSink
{
    Task WriteAsync(ManagedOperationDiagnostic diagnostic, CancellationToken cancellationToken);
}

public interface IManagedDiagnosticRecorder
{
    Task<ManagedDiagnosticReceipt> RecordFailureAsync(
        ManagedDiagnosticContext context,
        Exception exception,
        CancellationToken cancellationToken);

    Task<ManagedDiagnosticReceipt> RecordEventAsync(
        ManagedDiagnosticContext context,
        CancellationToken cancellationToken) =>
        Task.FromResult(new ManagedDiagnosticReceipt(
            context.CorrelationId,
            "Unavailable",
            false,
            false));
}

public sealed class ManagedDiagnosticRecorder(
    IManagedDiagnosticStore store,
    IManagedDiagnosticFileSink fallback,
    IOptions<ManagedDiagnosticsOptions> options) : IManagedDiagnosticRecorder
{
    private readonly ManagedDiagnosticsOptions settings = options.Value;

    public async Task<ManagedDiagnosticReceipt> RecordFailureAsync(
        ManagedDiagnosticContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var diagnostic = CreateDiagnostic(context, exception);
        return await PersistAsync(context.CorrelationId, diagnostic, cancellationToken);
    }

    public async Task<ManagedDiagnosticReceipt> RecordEventAsync(
        ManagedDiagnosticContext context,
        CancellationToken cancellationToken)
    {
        var diagnostic = CreateDiagnostic(context, null);
        return await PersistAsync(context.CorrelationId, diagnostic, cancellationToken);
    }

    private async Task<ManagedDiagnosticReceipt> PersistAsync(
        string correlationId,
        ManagedOperationDiagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        using var bounded = CreateBoundedToken(cancellationToken);
        try
        {
            await store.WriteAsync(diagnostic, bounded.Token);
            return new(correlationId, "Database", true, false);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            // Diagnostic persistence must never replace the operation failure.
        }
        catch (OperationCanceledException)
        {
            // A cancelled caller still receives a best-effort independent file fallback.
        }

        using var fallbackBounded = CreateBoundedToken(CancellationToken.None);
        try
        {
            await fallback.WriteAsync(diagnostic, fallbackBounded.Token);
            return new(correlationId, "FallbackFile", false, true);
        }
        catch (Exception)
        {
            return new(correlationId, "Unavailable", false, false);
        }
    }

    private CancellationTokenSource CreateBoundedToken(CancellationToken parent)
    {
        var timeout = settings.PersistenceTimeout > TimeSpan.Zero
            ? settings.PersistenceTimeout
            : TimeSpan.FromSeconds(10);
        var source = CancellationTokenSource.CreateLinkedTokenSource(parent);
        source.CancelAfter(timeout);
        return source;
    }

    private static ManagedOperationDiagnostic CreateDiagnostic(
        ManagedDiagnosticContext context,
        Exception? exception)
    {
        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        return new(
            $"DIAG-{Guid.NewGuid():N}",
            context.CorrelationId,
            DateTimeOffset.UtcNow,
            context.Operation,
            context.Stage.ToString(),
            context.ErrorCode,
            exception?.GetType().FullName ?? string.Empty,
            DiagnosticRedactor.Redact(exception?.Message ?? context.SafeDetail) ?? string.Empty,
            BuildInnerChain(exception?.InnerException),
            DiagnosticRedactor.Redact(exception?.StackTrace),
            "1.0.0+post-release",
            assemblyVersion,
            context.RulesetId,
            context.RuleReleaseId,
            context.SourceIdentity,
            context.CampaignId,
            Math.Max(0, (long)(DateTimeOffset.UtcNow - context.StartedAt).TotalMilliseconds),
            context.Outcome,
            context.RetrySafe,
            context.AdministrativeInterventionRequired,
            context.ReleaseChannel?.ToString(),
            context.DiscoveryRef,
            DiagnosticRedactor.Redact(context.SafeDetail),
            context.OperationId);
    }

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
}

public sealed class SqlServerManagedDiagnosticStore(
    IOptions<SqlServerPersistenceOptions> options) : IManagedDiagnosticStore
{
    private readonly SqlServerPersistenceOptions settings = options.Value;

    public async Task WriteAsync(
        ManagedOperationDiagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            SqlServerSchemaIdentifier.Bind("""
                INSERT INTO {{schema}}.managed_operation_diagnostics (
                    diagnostic_id, correlation_id, operation_id, recorded_at, operation_name,
                    operation_stage, error_code, exception_type,
                    sanitized_exception_message, sanitized_inner_exception_chain,
                    sanitized_stack_trace, eternal_cycle_version,
                    implementation_version, ruleset_id, rule_release_id,
                    source_identity, campaign_id, duration_ms, outcome,
                    retry_safe, administrative_intervention_required,
                    source_channel, discovery_ref, safe_detail
                ) VALUES (
                    @diagnostic_id, @correlation_id, @operation_id, @recorded_at, @operation_name,
                    @operation_stage, @error_code, @exception_type,
                    @sanitized_exception_message, @sanitized_inner_exception_chain,
                    @sanitized_stack_trace, @eternal_cycle_version,
                    @implementation_version, @ruleset_id, @rule_release_id,
                    @source_identity, @campaign_id, @duration_ms, @outcome,
                    @retry_safe, @administrative_intervention_required,
                    @source_channel, @discovery_ref, @safe_detail
                );
                """, settings.DomainSchema),
            connection)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };
        Add(command, "@diagnostic_id", diagnostic.DiagnosticId);
        Add(command, "@correlation_id", diagnostic.CorrelationId);
        Add(command, "@operation_id", diagnostic.OperationId);
        Add(command, "@recorded_at", diagnostic.RecordedAt);
        Add(command, "@operation_name", diagnostic.Operation);
        Add(command, "@operation_stage", diagnostic.Stage);
        Add(command, "@error_code", diagnostic.ErrorCode);
        Add(command, "@exception_type", diagnostic.ExceptionType);
        Add(command, "@sanitized_exception_message", diagnostic.SanitizedExceptionMessage);
        Add(command, "@sanitized_inner_exception_chain", diagnostic.SanitizedInnerExceptionChain);
        Add(command, "@sanitized_stack_trace", diagnostic.SanitizedStackTrace);
        Add(command, "@eternal_cycle_version", diagnostic.EternalCycleVersion);
        Add(command, "@implementation_version", diagnostic.ImplementationVersion);
        Add(command, "@ruleset_id", diagnostic.RulesetId);
        Add(command, "@rule_release_id", diagnostic.RuleReleaseId);
        Add(command, "@source_identity", diagnostic.SourceIdentity);
        Add(command, "@campaign_id", diagnostic.CampaignId);
        Add(command, "@duration_ms", diagnostic.DurationMilliseconds);
        Add(command, "@outcome", diagnostic.Outcome);
        Add(command, "@retry_safe", diagnostic.RetrySafe);
        Add(command, "@administrative_intervention_required", diagnostic.AdministrativeInterventionRequired);
        Add(command, "@source_channel", diagnostic.ReleaseChannel);
        Add(command, "@discovery_ref", diagnostic.DiscoveryRef);
        Add(command, "@safe_detail", diagnostic.SafeDetail);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void Add(SqlCommand command, string name, object? value) =>
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);
}

public sealed class PhysicalManagedDiagnosticFileSink(
    IOptions<ManagedDiagnosticsOptions> options,
    SanitizedDiagnosticFallbackState? startupState = null) : IManagedDiagnosticFileSink
{
    private static readonly SemaphoreSlim WriteLock = new(1, 1);
    private readonly ManagedDiagnosticsOptions settings = options.Value;

    public async Task WriteAsync(
        ManagedOperationDiagnostic diagnostic,
        CancellationToken cancellationToken)
    {
        if (startupState is not null && !startupState.Available)
        {
            throw new IOException(startupState.Detail);
        }

        var path = startupState?.ResolvedPath ?? ResolvePath(settings.FallbackLogFile);
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new IOException("No sanitized diagnostic fallback path is available.");
        }
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var line = JsonSerializer.Serialize(diagnostic) + Environment.NewLine;
        await WriteLock.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(path, line, cancellationToken);
        }
        finally
        {
            WriteLock.Release();
        }
    }

    public static string ResolvePath(string configuredPath)
    {
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "EternalCycle", "logs", "managed-diagnostics.jsonl");
    }
}

public sealed class NullManagedDiagnosticRecorder : IManagedDiagnosticRecorder
{
    public static NullManagedDiagnosticRecorder Instance { get; } = new();

    private NullManagedDiagnosticRecorder()
    {
    }

    public Task<ManagedDiagnosticReceipt> RecordFailureAsync(
        ManagedDiagnosticContext context,
        Exception exception,
        CancellationToken cancellationToken) =>
        Task.FromResult(new ManagedDiagnosticReceipt(context.CorrelationId, "Unavailable", false, false));

    public Task<ManagedDiagnosticReceipt> RecordEventAsync(
        ManagedDiagnosticContext context,
        CancellationToken cancellationToken) =>
        Task.FromResult(new ManagedDiagnosticReceipt(context.CorrelationId, "Unavailable", false, false));
}

public static partial class DiagnosticRedactor
{
    public static string? Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var redacted = ConnectionString().Replace(value, " [REDACTED_CONNECTION_STRING]");
        redacted = SensitiveAssignment().Replace(redacted, match => $"{match.Groups[1].Value}=[REDACTED]");
        redacted = BearerToken().Replace(redacted, "Bearer [REDACTED]");
        redacted = UriUserInfo().Replace(redacted, "$1[REDACTED]@");
        return redacted.Length <= 32000 ? redacted : redacted[..32000];
    }

    [GeneratedRegex(@"(?i)(?:^|[\s""'])\s*(?:Server|Data\s+Source)\s*=\s*[^;\r\n]+(?:\s*;\s*[^;\r\n=]+\s*=\s*[^;\r\n]+)+")]
    private static partial Regex ConnectionString();

    [GeneratedRegex(@"(?i)\b(password|pwd|user\s*id|uid|access[_\s-]*token|token|secret|api[_\s-]*key|authorization)\s*=\s*[^;\s,]+")]
    private static partial Regex SensitiveAssignment();

    [GeneratedRegex(@"(?i)Bearer\s+[A-Za-z0-9._~+/=-]+")]
    private static partial Regex BearerToken();

    [GeneratedRegex(@"([A-Za-z][A-Za-z0-9+.-]*://)[^/@\s]+@")]
    private static partial Regex UriUserInfo();
}

public sealed class RulePublicationException(
    string code,
    RulePublicationStage stage,
    string safeMessage,
    bool retrySafe,
    bool administrativeInterventionRequired,
    Exception? innerException = null,
    RuleSourceReleaseChannel? releaseChannel = null,
    string? discoveryRef = null,
    string? sourceIdentity = null)
    : Exception(safeMessage, innerException)
{
    public string Code { get; } = code;

    public RulePublicationStage Stage { get; } = stage;

    public string SafeMessage { get; } = safeMessage;

    public bool RetrySafe { get; } = retrySafe;

    public bool AdministrativeInterventionRequired { get; } = administrativeInterventionRequired;

    public RuleSourceReleaseChannel? ReleaseChannel { get; } = releaseChannel;

    public string? DiscoveryRef { get; } = discoveryRef;

    public string? SourceIdentity { get; } = sourceIdentity;
}
