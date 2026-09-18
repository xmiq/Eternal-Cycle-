using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace EternalCycle.Persistence.Mcp;

public sealed record RecentManagedError(
    DateTimeOffset RecordedAt,
    string ToolName,
    ManagedToolFailure Failure);

public interface IRecentManagedErrorStore
{
    void Add(RecentManagedError error);

    IReadOnlyList<RecentManagedError> Find(string? correlationId, int maximumCount);
}

public sealed class BoundedRecentManagedErrorStore : IRecentManagedErrorStore
{
    private const int Capacity = 100;
    private readonly ConcurrentQueue<RecentManagedError> errors = new();

    public void Add(RecentManagedError error)
    {
        errors.Enqueue(error);
        while (errors.Count > Capacity && errors.TryDequeue(out _))
        {
        }
    }

    public IReadOnlyList<RecentManagedError> Find(string? correlationId, int maximumCount) =>
        errors
            .Where(value => string.IsNullOrWhiteSpace(correlationId) ||
                string.Equals(value.Failure.CorrelationId, correlationId, StringComparison.Ordinal))
            .OrderByDescending(value => value.RecordedAt)
            .Take(Math.Clamp(maximumCount, 1, 20))
            .ToArray();
}

public sealed record ErrorDumpRequest(
    string? CorrelationId = null,
    string? OperationId = null,
    int MaximumCount = 5);

public sealed record ManagedDistributionIdentity(
    string ProjectName,
    string OriginalProjectAuthor,
    string DistributionIdentity,
    string? SupportDestination,
    string? SupportDestinationType,
    bool? OfficialDistribution,
    string License);

public interface IManagedDistributionIdentityProvider
{
    ManagedDistributionIdentity Get();
}

public sealed class PackagedManagedDistributionIdentityProvider(
    IOptions<ManagedAdministrationOptions> options) : IManagedDistributionIdentityProvider
{
    private readonly ManagedAdministrationOptions settings = options.Value;

    public ManagedDistributionIdentity Get()
    {
        try
        {
            var metadata = OfficialDistributionMetadata.Load(settings);
            return new(
                metadata.Project,
                metadata.OriginalProjectAuthor ?? "xmiq",
                metadata.DistributionIdentity ?? "Packaged Eternal Cycle distribution",
                metadata.SupportPage,
                metadata.SupportDestinationType,
                OfficialDistribution: null,
                metadata.License ?? "Apache-2.0");
        }
        catch (ManagedServiceException)
        {
            return new(
                "Eternal Cycle",
                "xmiq",
                "Unverified Eternal Cycle distribution",
                null,
                null,
                OfficialDistribution: null,
                "Apache-2.0");
        }
    }
}

public sealed record ErrorDumpSourceStatus(
    string Source,
    bool Available,
    int MatchingRecords,
    string Detail);

public sealed record EternalCycleErrorDump(
    int ErrorDumpFormatVersion,
    string State,
    string GeneratedAt,
    string EternalCycleVersion,
    string Implementation,
    ManagedDistributionIdentity Distribution,
    string? RequestedCorrelationId,
    string? RequestedOperationId,
    IReadOnlyList<RecentManagedError> RecentErrors,
    IReadOnlyList<ManagedOperationDiagnostic> Diagnostics,
    ManagedOperationStatus? ManagedOperation,
    ManagedReadinessReport? Readiness,
    ManagedConfigurationReport Configuration,
    EternalCycleErrorLookup? ErrorLookup,
    IReadOnlyList<ErrorDumpSourceStatus> Sources,
    bool DiagnosticsTruncated,
    int DiagnosticEventsIncluded,
    string SanitizationStatement,
    string Text);

public interface IManagedDiagnosticEvidenceReader
{
    Task<IReadOnlyList<ManagedOperationDiagnostic>> ReadAsync(
        string? correlationId,
        string? operationId,
        int maximumCount,
        CancellationToken cancellationToken);
}

public sealed class SqlServerManagedDiagnosticEvidenceReader(
    IOptions<SqlServerPersistenceOptions> options) : IManagedDiagnosticEvidenceReader
{
    private readonly SqlServerPersistenceOptions settings = options.Value;

    public async Task<IReadOnlyList<ManagedOperationDiagnostic>> ReadAsync(
        string? correlationId,
        string? operationId,
        int maximumCount,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new SqlCommand(
            SqlServerSchemaIdentifier.Bind("""
                IF OBJECT_ID(N'{{schema}}.managed_operation_diagnostics', N'U') IS NULL
                BEGIN
                    SELECT TOP (0)
                        CAST(N'' AS nvarchar(128)) AS diagnostic_id,
                        CAST(N'' AS nvarchar(128)) AS correlation_id,
                        SYSUTCDATETIME() AS recorded_at,
                        CAST(N'' AS nvarchar(128)) AS operation_name,
                        CAST(N'' AS nvarchar(64)) AS operation_stage,
                        CAST(N'' AS nvarchar(128)) AS error_code,
                        CAST(N'' AS nvarchar(512)) AS exception_type,
                        CAST(N'' AS nvarchar(2000)) AS sanitized_exception_message,
                        CAST(NULL AS nvarchar(max)) AS sanitized_inner_exception_chain,
                        CAST(NULL AS nvarchar(max)) AS sanitized_stack_trace,
                        CAST(N'' AS nvarchar(64)) AS eternal_cycle_version,
                        CAST(N'' AS nvarchar(64)) AS implementation_version,
                        CAST(NULL AS nvarchar(128)) AS ruleset_id,
                        CAST(NULL AS nvarchar(128)) AS rule_release_id,
                        CAST(NULL AS nvarchar(256)) AS source_identity,
                        CAST(NULL AS nvarchar(128)) AS campaign_id,
                        CAST(0 AS bigint) AS duration_ms,
                        CAST(N'' AS nvarchar(32)) AS outcome,
                        CAST(NULL AS bit) AS retry_safe,
                        CAST(NULL AS bit) AS administrative_intervention_required,
                        CAST(NULL AS nvarchar(32)) AS source_channel,
                        CAST(NULL AS nvarchar(256)) AS discovery_ref,
                        CAST(NULL AS nvarchar(2000)) AS safe_detail,
                        CAST(NULL AS nvarchar(128)) AS operation_id;
                    RETURN;
                END;

                SELECT TOP (@maximum_count)
                    diagnostic_id, correlation_id, recorded_at, operation_name,
                    operation_stage, error_code, exception_type,
                    sanitized_exception_message, sanitized_inner_exception_chain,
                    sanitized_stack_trace, eternal_cycle_version,
                    implementation_version, ruleset_id, rule_release_id,
                    source_identity, campaign_id, duration_ms, outcome,
                    retry_safe, administrative_intervention_required,
                    source_channel, discovery_ref, safe_detail, operation_id
                FROM {{schema}}.managed_operation_diagnostics
                WHERE (@correlation_id IS NULL AND @operation_id IS NULL)
                   OR correlation_id = @correlation_id
                   OR operation_id = @operation_id
                ORDER BY recorded_at DESC, diagnostic_id DESC;
                """, settings.DomainSchema),
            connection)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };
        command.Parameters.AddWithValue("@maximum_count", Math.Clamp(maximumCount, 1, 20));
        command.Parameters.AddWithValue("@correlation_id", (object?)correlationId ?? DBNull.Value);
        command.Parameters.AddWithValue("@operation_id", (object?)operationId ?? DBNull.Value);
        var result = new List<ManagedOperationDiagnostic>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(Read(reader));
        }

        return result;
    }

    private static ManagedOperationDiagnostic Read(SqlDataReader reader) =>
        new(
            reader.GetString(0),
            reader.GetString(1),
            reader.GetDateTimeOffset(2),
            reader.GetString(3),
            reader.GetString(4),
            reader.GetString(5),
            reader.GetString(6),
            reader.GetString(7),
            OptionalString(reader, 8),
            OptionalString(reader, 9),
            reader.GetString(10),
            reader.GetString(11),
            OptionalString(reader, 12),
            OptionalString(reader, 13),
            OptionalString(reader, 14),
            OptionalString(reader, 15),
            reader.GetInt64(16),
            reader.GetString(17),
            reader.IsDBNull(18) ? null : reader.GetBoolean(18),
            reader.IsDBNull(19) ? null : reader.GetBoolean(19),
            OptionalString(reader, 20),
            OptionalString(reader, 21),
            OptionalString(reader, 22),
            OptionalString(reader, 23));

    private static string? OptionalString(SqlDataReader reader, int ordinal) =>
        reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
}

public interface IErrorDumpService
{
    Task<EternalCycleErrorDump> GetAsync(ErrorDumpRequest request, CancellationToken cancellationToken);
}

public sealed partial class ErrorDumpService(
    IRecentManagedErrorStore recent,
    IManagedConfigurationService configuration,
    SanitizedDiagnosticFallbackState fallback,
    IManagedDistributionIdentityProvider? distribution = null,
    IManagedReadinessService? readiness = null,
    IManagedDiagnosticEvidenceReader? databaseDiagnostics = null,
    IManagedOperationStore? operations = null) : IErrorDumpService
{
    public const int FormatVersion = 1;
    public const int MaximumEvidenceRecords = 10;
    public const int MaximumTextCharacters = 24000;
    private const int MaximumDetailCharacters = 2000;

    public async Task<EternalCycleErrorDump> GetAsync(
        ErrorDumpRequest request,
        CancellationToken cancellationToken)
    {
        var maximumCount = Math.Clamp(request.MaximumCount, 1, MaximumEvidenceRecords);
        var correlationId = NormalizeSelector(request.CorrelationId);
        var operationId = NormalizeSelector(request.OperationId);
        var sourceStatuses = new List<ErrorDumpSourceStatus>();
        ManagedReadinessReport? readinessReport = null;
        if (readiness is not null)
        {
            try
            {
                readinessReport = await readiness.GetReadinessAsync(null, cancellationToken);
                sourceStatuses.Add(new("Readiness", true, 1, "Service-level readiness was obtained without a campaign."));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                sourceStatuses.Add(new("Readiness", false, 0, $"Readiness evidence was unavailable ({exception.GetType().Name})."));
            }
        }

        ManagedOperationStatus? operation = null;
        if (operations is not null && (operationId is not null || correlationId is not null))
        {
            if (readinessReport?.State is
                ManagedReadinessState.ConfigurationRequired or
                ManagedReadinessState.ConfigurationInvalid or
                ManagedReadinessState.SetupRequired or
                ManagedReadinessState.MigrationRequired)
            {
                sourceStatuses.Add(new(
                    "ManagedOperation",
                    false,
                    0,
                    "Durable operation evidence was not queried before its required schema migration."));
            }
            else
            {
                try
                {
                    operation = operationId is not null
                        ? await operations.GetAsync(operationId, cancellationToken)
                        : await operations.GetByCorrelationAsync(correlationId!, cancellationToken);
                    sourceStatuses.Add(new("ManagedOperation", true, operation is null ? 0 : 1, "Durable operation lookup completed."));
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    sourceStatuses.Add(new("ManagedOperation", false, 0, $"Managed Operation evidence was unavailable ({exception.GetType().Name})."));
                }
            }
        }

        var effectiveCorrelationId = correlationId ?? operation?.CorrelationId;
        var recentErrors = recent.Find(effectiveCorrelationId, maximumCount).Select(Bound).ToArray();
        sourceStatuses.Add(new("InProcess", true, recentErrors.Length, "Bounded process-local error evidence was queried."));

        var fileDiagnostics = ReadFallback(effectiveCorrelationId, operationId, maximumCount, out var fileStatus);
        sourceStatuses.Add(fileStatus);

        IReadOnlyList<ManagedOperationDiagnostic> databaseEvidence = [];
        if (databaseDiagnostics is not null && readinessReport?.State is not (
            ManagedReadinessState.ConfigurationRequired or
            ManagedReadinessState.ConfigurationInvalid or
            ManagedReadinessState.SetupRequired or
            ManagedReadinessState.MigrationRequired))
        {
            try
            {
                databaseEvidence = await databaseDiagnostics.ReadAsync(
                    effectiveCorrelationId,
                    operationId,
                    maximumCount,
                    cancellationToken);
                sourceStatuses.Add(new("DatabaseDiagnostics", true, databaseEvidence.Count, "Diagnostic lookup by durable operation or correlation completed."));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                sourceStatuses.Add(new("DatabaseDiagnostics", false, 0, $"Database diagnostic evidence was unavailable ({exception.GetType().Name})."));
            }
        }
        else
        {
            sourceStatuses.Add(new(
                "DatabaseDiagnostics",
                false,
                0,
                databaseDiagnostics is null
                    ? "No database diagnostic reader is registered."
                    : "Diagnostic evidence was not queried before its required schema migration."));
        }

        var diagnostics = fileDiagnostics
            .Concat(databaseEvidence)
            .DistinctBy(value => value.DiagnosticId)
            .OrderByDescending(value => value.RecordedAt)
            .Take(maximumCount)
            .Select(Bound)
            .ToArray();
        var primaryCode = recentErrors.FirstOrDefault()?.Failure.Code ?? diagnostics.FirstOrDefault()?.ErrorCode;
        var lookup = primaryCode is null ? null : EternalCycleErrorRegistry.Get(primaryCode);
        var evidenceCount = recentErrors.Length + diagnostics.Length + (operation is null ? 0 : 1);
        var unavailable = sourceStatuses.Any(value => !value.Available);
        var state = evidenceCount == 0 ? "NoMatchingEvidence" : unavailable ? "Partial" : "Complete";
        var config = configuration.GetReport();
        var generatedAt = DateTimeOffset.UtcNow;
        var wasTruncated = request.MaximumCount > maximumCount ||
            recentErrors.Any(value => value.Failure.DeveloperDiagnostics?.StackTrace?.Length == MaximumDetailCharacters) ||
            diagnostics.Any(value =>
                value.SanitizedExceptionMessage.Length == MaximumDetailCharacters ||
                value.SanitizedStackTrace?.Length == MaximumDetailCharacters);
        var identity = distribution?.Get() ?? new(
            "Eternal Cycle",
            "xmiq",
            "Unverified Eternal Cycle distribution",
            null,
            null,
            OfficialDistribution: null,
            "Apache-2.0");
        var text = BuildText(
            state,
            generatedAt,
            effectiveCorrelationId,
            operationId,
            primaryCode,
            readinessReport,
            config,
            recentErrors,
            diagnostics,
            operation,
            sourceStatuses,
            identity,
            wasTruncated);
        return new EternalCycleErrorDump(
            FormatVersion,
            state,
            generatedAt.ToString("O"),
            "1.0.0+post-release",
            ".NET / MCP / T-SQL reference implementation",
            identity,
            effectiveCorrelationId,
            operationId,
            recentErrors,
            diagnostics,
            operation,
            readinessReport,
            config,
            lookup,
            sourceStatuses,
            wasTruncated,
            recentErrors.Length + diagnostics.Length,
            "Secrets, credentials, connection strings, tokens, Campaign Canon, GM Secrets, private conversations, and arbitrary environment values are excluded.",
            text);
    }

    private IReadOnlyList<ManagedOperationDiagnostic> ReadFallback(
        string? correlationId,
        string? operationId,
        int maximumCount,
        out ErrorDumpSourceStatus status)
    {
        if (!fallback.Available || string.IsNullOrWhiteSpace(fallback.ResolvedPath))
        {
            status = new("FallbackFile", false, 0, fallback.Detail);
            return [];
        }

        try
        {
            if (!File.Exists(fallback.ResolvedPath))
            {
                status = new("FallbackFile", true, 0, "The sanitized fallback is initialized but contains no diagnostic records.");
                return [];
            }

            var parsed = File.ReadLines(fallback.ResolvedPath)
                .TakeLast(200)
                .Select(TryParse)
                .Where(value => value is not null)
                .Select(value => value!)
                .Where(value =>
                    (correlationId is null && operationId is null) ||
                    string.Equals(value.CorrelationId, correlationId, StringComparison.Ordinal) ||
                    string.Equals(value.OperationId, operationId, StringComparison.Ordinal))
                .OrderByDescending(value => value.RecordedAt)
                .Take(maximumCount)
                .ToArray();
            status = new("FallbackFile", true, parsed.Length, "The automatic sanitized fallback was queried.");
            return parsed;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            status = new("FallbackFile", false, 0, $"Fallback evidence could not be read ({exception.GetType().Name}).");
            return [];
        }
    }

    private static ManagedOperationDiagnostic? TryParse(string line)
    {
        try
        {
            return JsonSerializer.Deserialize<ManagedOperationDiagnostic>(line);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? NormalizeSelector(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= 128 && SafeSelector().IsMatch(trimmed) ? trimmed : $"invalid:{trimmed[..Math.Min(trimmed.Length, 32)]}";
    }

    private static string BuildText(
        string state,
        DateTimeOffset generatedAt,
        string? correlationId,
        string? operationId,
        string? primaryCode,
        ManagedReadinessReport? readiness,
        ManagedConfigurationReport configuration,
        IReadOnlyList<RecentManagedError> recent,
        IReadOnlyList<ManagedOperationDiagnostic> diagnostics,
        ManagedOperationStatus? operation,
        IReadOnlyList<ErrorDumpSourceStatus> sources,
        ManagedDistributionIdentity distribution,
        bool diagnosticsTruncated)
    {
        var text = new StringBuilder();
        text.AppendLine("Eternal Cycle Error Dump");
        text.AppendLine($"Format: {FormatVersion}");
        text.AppendLine($"Project: {distribution.ProjectName}");
        text.AppendLine($"Original project author: {distribution.OriginalProjectAuthor}");
        text.AppendLine($"Distribution: {distribution.DistributionIdentity}");
        text.AppendLine($"License: {distribution.License}");
        text.AppendLine($"State: {state}");
        text.AppendLine($"Generated: {generatedAt:O}");
        text.AppendLine($"Error code: {primaryCode ?? "unknown"}");
        text.AppendLine($"Correlation ID: {correlationId ?? "not specified"}");
        text.AppendLine($"Operation ID: {operationId ?? "not specified"}");
        text.AppendLine($"Readiness: {readiness?.State.ToString() ?? "unavailable"}");
        text.AppendLine($"Configuration: {configuration.State}");
        text.AppendLine($"Fallback: {readiness?.SanitizedFileLogStatus ?? "unknown"}");
        text.AppendLine($"In-process errors: {recent.Count}");
        text.AppendLine($"Diagnostic records: {diagnostics.Count}");
        text.AppendLine($"Managed Operation: {operation?.State.ToString() ?? "unavailable"}");
        text.AppendLine($"Diagnostics truncated: {diagnosticsTruncated}");
        if (!string.IsNullOrWhiteSpace(distribution.SupportDestination))
        {
            text.AppendLine($"Support destination: {distribution.SupportDestinationType ?? "Configured"} ({distribution.SupportDestination})");
        }
        text.AppendLine("Evidence sources:");
        foreach (var source in sources)
        {
            text.AppendLine($"- {source.Source}: {(source.Available ? "available" : "unavailable")}; records={source.MatchingRecords}; {source.Detail}");
        }

        text.AppendLine("Sanitization: secrets, private Canon, GM Secrets, conversations, and raw environment values are excluded.");
        var result = text.ToString();
        return result.Length <= MaximumTextCharacters ? result : result[..MaximumTextCharacters];
    }

    private static RecentManagedError Bound(RecentManagedError error) =>
        error with
        {
            Failure = error.Failure with
            {
                Message = Bound(error.Failure.Message) ?? string.Empty,
                DeveloperDiagnostics = error.Failure.DeveloperDiagnostics is null
                    ? null
                    : error.Failure.DeveloperDiagnostics with
                    {
                        ExceptionMessage = Bound(error.Failure.DeveloperDiagnostics.ExceptionMessage) ?? string.Empty,
                        InnerExceptionChain = Bound(error.Failure.DeveloperDiagnostics.InnerExceptionChain),
                        StackTrace = Bound(error.Failure.DeveloperDiagnostics.StackTrace)
                    }
            }
        };

    private static ManagedOperationDiagnostic Bound(ManagedOperationDiagnostic diagnostic) =>
        diagnostic with
        {
            SanitizedExceptionMessage = Bound(diagnostic.SanitizedExceptionMessage) ?? string.Empty,
            SanitizedInnerExceptionChain = Bound(diagnostic.SanitizedInnerExceptionChain),
            SanitizedStackTrace = Bound(diagnostic.SanitizedStackTrace),
            SafeDetail = Bound(diagnostic.SafeDetail)
        };

    private static string? Bound(string? value)
    {
        var sanitized = DiagnosticRedactor.Redact(value);
        return sanitized is null || sanitized.Length <= MaximumDetailCharacters
            ? sanitized
            : sanitized[..MaximumDetailCharacters];
    }

    [GeneratedRegex("^[A-Za-z0-9._:-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex SafeSelector();
}

[McpServerToolType]
public sealed class ErrorDumpTools(IErrorDumpService dumps)
{
    [McpServerTool(Name = "ec_get_error_dump", ReadOnly = true, Idempotent = true),
     Description("Returns a bounded copy/paste-friendly sanitized error dump by correlation ID, operation ID, or recent failure. It requires no campaign and degrades across in-process, fallback-file, database, readiness, and durable-operation evidence sources.")]
    public async Task<CallToolResult> GetErrorDumpAsync(
        ErrorDumpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dump = await dumps.GetAsync(request, cancellationToken);
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = dump.Text }],
                StructuredContent = JsonSerializer.SerializeToElement(dump),
                IsError = false
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var secondary = McpToolFailureContract.NewCorrelationId();
            var original = string.IsNullOrWhiteSpace(request.CorrelationId)
                ? "not specified"
                : request.CorrelationId.Replace('\r', ' ').Replace('\n', ' ').Trim();
            var text =
                "Eternal Cycle Error Dump\n" +
                $"Format: {ErrorDumpService.FormatVersion}\n" +
                "Error Code: INTERNAL_ERROR\n" +
                $"Reference: {original}\n" +
                $"Secondary reference: {secondary}\n" +
                "Detailed dump generation failed; use ec_get_error_code with INTERNAL_ERROR.";
            return new CallToolResult
            {
                Content = [new TextContentBlock { Text = text }],
                IsError = true
            };
        }
    }
}
