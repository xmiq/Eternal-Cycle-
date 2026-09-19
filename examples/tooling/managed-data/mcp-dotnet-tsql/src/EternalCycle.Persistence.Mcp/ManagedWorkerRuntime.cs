using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public static class ManagedWorkerCancellationSources
{
    public const string WorkerShutdown = "WorkerShutdown";
    public const string ExplicitWorkerStop = "ExplicitWorkerStop";
}

public sealed class ManagedWorkerOptions
{
    public string? ExecutablePath { get; init; }

    public string ControlDirectory { get; init; } = Path.Combine(
        Path.GetTempPath(),
        "EternalCycle",
        "managed-workers");

    public TimeSpan StopPollInterval { get; init; } = TimeSpan.FromMilliseconds(250);

    public string? FallbackLauncherDirectory { get; init; }

    public TimeSpan FallbackHandoffTimeout { get; init; } = TimeSpan.FromSeconds(30);
}

public sealed record ManagedWorkerStopResult(bool Accepted, string Code, string Message);

public sealed record ManagedWorkerLaunchResult(
    bool IndependentWorkerStarted,
    bool UserInterventionRequired,
    string Code,
    string Message,
    string? PreparedLauncherPath = null)
{
    public static ManagedWorkerLaunchResult Started() => new(
        true,
        false,
        "MANAGED_WORKER_STARTED",
        "Independent durable execution started.");

    public static ManagedWorkerLaunchResult NotRequired() => new(
        false,
        false,
        "MANAGED_WORKER_NOT_REQUIRED",
        "The Managed Operation does not require a worker launch.");
}

public sealed record ManagedOperationInitiation(
    ManagedOperationStatus Operation,
    ManagedWorkerLaunchResult WorkerLaunch);

public interface IManagedWorkerLauncher
{
    Task<ManagedWorkerLaunchResult> EnsureLaunchedAsync(
        ManagedOperationStatus operation,
        CancellationToken cancellationToken);
}

public interface IManagedWorkerControl
{
    Task<ManagedWorkerStopResult> RequestStopAsync(
        string operationId,
        CancellationToken cancellationToken);
}

internal sealed record IndependentProcessStartRequest(
    string ExecutablePath,
    IReadOnlyList<string> Arguments,
    string WorkingDirectory);

internal interface IIndependentProcessStarter
{
    int Start(IndependentProcessStartRequest request);
}

internal sealed class PlatformIndependentProcessStarter : IIndependentProcessStarter
{
    private const uint CreateNewProcessGroup = 0x00000200;
    private const uint CreateBreakawayFromJob = 0x01000000;
    private const uint CreateNoWindow = 0x08000000;
    private const uint WaitObject0 = 0x00000000;
    private const uint WaitTimeout = 0x00000102;
    private const uint LaunchWaitMilliseconds = 5000;

    public int Start(IndependentProcessStartRequest request)
    {
        if (!OperatingSystem.IsWindows())
        {
            return StartPortable(request);
        }

        // A short-lived cmd.exe trampoline breaks the Windows parent-process chain.
        // The trampoline itself is created outside the MCP host job; its child therefore
        // inherits the independent job context, and cmd exits before this method returns.
        var commandInterpreter = Environment.GetEnvironmentVariable("ComSpec");
        if (string.IsNullOrWhiteSpace(commandInterpreter))
        {
            commandInterpreter = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                "cmd.exe");
        }

        var commandLine = new StringBuilder(Quote(commandInterpreter))
            .Append(" /d /s /c start \"\" /b /d ")
            .Append(CommandQuote(request.WorkingDirectory))
            .Append(' ')
            .Append(CommandQuote(request.ExecutablePath));
        foreach (var argument in request.Arguments)
        {
            commandLine.Append(' ').Append(CommandQuote(argument));
        }

        var startup = new StartupInfo { Size = Marshal.SizeOf<StartupInfo>() };
        if (!CreateProcess(
                commandInterpreter,
                commandLine,
                IntPtr.Zero,
                IntPtr.Zero,
                inheritHandles: false,
                CreateNewProcessGroup | CreateBreakawayFromJob | CreateNoWindow,
                IntPtr.Zero,
                request.WorkingDirectory,
                ref startup,
                out var process))
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                "The independent Managed Worker launch trampoline could not be created outside the MCP host job.");
        }

        try
        {
            var waitResult = WaitForSingleObject(process.ProcessHandle, LaunchWaitMilliseconds);
            if (waitResult == WaitTimeout)
            {
                throw new TimeoutException("The independent Managed Worker launch trampoline did not finish promptly.");
            }

            if (waitResult != WaitObject0 ||
                !GetExitCodeProcess(process.ProcessHandle, out var exitCode) ||
                exitCode != 0)
            {
                throw new InvalidOperationException("The independent Managed Worker launch trampoline failed.");
            }

            return unchecked((int)process.ProcessId);
        }
        finally
        {
            CloseHandle(process.ThreadHandle);
            CloseHandle(process.ProcessHandle);
        }
    }

    private static int StartPortable(IndependentProcessStartRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            WorkingDirectory = request.WorkingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The independent Managed Worker process did not start.");
        return process.Id;
    }

    internal static string Quote(string value)
    {
        if (value.Length > 0 && value.All(character =>
                !char.IsWhiteSpace(character) && character is not '"' and not '\\'))
        {
            return value;
        }

        var result = new StringBuilder("\"");
        var backslashes = 0;
        foreach (var character in value)
        {
            if (character == '\\')
            {
                backslashes++;
                continue;
            }

            if (character == '"')
            {
                result.Append('\\', backslashes * 2 + 1).Append('"');
                backslashes = 0;
                continue;
            }

            result.Append('\\', backslashes).Append(character);
            backslashes = 0;
        }

        result.Append('\\', backslashes * 2).Append('"');
        return result.ToString();
    }

    private static string CommandQuote(string value) =>
        $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        public int Size;
        public string? Reserved;
        public string? Desktop;
        public string? Title;
        public int X;
        public int Y;
        public int XSize;
        public int YSize;
        public int XCountChars;
        public int YCountChars;
        public int FillAttribute;
        public int Flags;
        public short ShowWindow;
        public short Reserved2;
        public IntPtr ReservedPointer;
        public IntPtr StandardInput;
        public IntPtr StandardOutput;
        public IntPtr StandardError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr ProcessHandle;
        public IntPtr ThreadHandle;
        public uint ProcessId;
        public uint ThreadId;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateProcess(
        string? applicationName,
        StringBuilder commandLine,
        IntPtr processAttributes,
        IntPtr threadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandles,
        uint creationFlags,
        IntPtr environment,
        string currentDirectory,
        ref StartupInfo startupInfo,
        out ProcessInformation processInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetExitCodeProcess(IntPtr process, out uint exitCode);
}

internal sealed record PreparedManagedWorkerFallback(
    string LauncherPath,
    string ConfigurationPath,
    string HandoffPath);

internal sealed class ManagedWorkerFallbackLauncher(
    IOptions<ManagedWorkerOptions> options,
    IConfiguration configuration,
    ILogger<ManagedWorkerFallbackLauncher> logger)
{
    internal const string LauncherFileName = "Continue Eternal Cycle Setup.bat";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private readonly ManagedWorkerOptions settings = options.Value;

    internal PreparedManagedWorkerFallback Prepare(
        ManagedOperationStatus operation,
        string workerExecutable,
        IReadOnlyList<string> workerArguments,
        string workingDirectory)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new ManagedServiceException(
                "WORKER_INDEPENDENT_LAUNCH_BLOCKED",
                "Independent Managed Worker launch was blocked and this deployment does not provide a manual launcher.");
        }

        var launcherDirectory = ResolveLauncherDirectory();
        var controlDirectory = Path.GetFullPath(settings.ControlDirectory);
        Directory.CreateDirectory(launcherDirectory);
        Directory.CreateDirectory(controlDirectory);

        var recoveryId = Guid.NewGuid().ToString("N");
        var configurationPath = Path.Combine(controlDirectory, $"worker-recovery-{recoveryId}.json");
        var handoffPath = Path.Combine(controlDirectory, $"worker-handoff-{recoveryId}.ready");
        var launcherPath = Path.Combine(launcherDirectory, LauncherFileName);
        var temporaryLauncher = $"{launcherPath}.{recoveryId}.tmp";
        try
        {
            var effectiveConfiguration = configuration
                .AsEnumerable()
                .Where(value =>
                    value.Value is not null &&
                    value.Key.StartsWith("EternalCycle:", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(
                    value => value.Key,
                    value => value.Value,
                    StringComparer.OrdinalIgnoreCase);
            File.WriteAllText(
                configurationPath,
                JsonSerializer.Serialize(effectiveConfiguration, JsonOptions),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.SetAttributes(configurationPath, File.GetAttributes(configurationPath) | FileAttributes.Hidden);

            var fallbackArguments = workerArguments
                .Concat([
                    ManagedWorkerCommand.FallbackConfigurationSwitch,
                    configurationPath,
                    ManagedWorkerCommand.HandoffSwitch,
                    handoffPath
                ])
                .ToArray();
            File.WriteAllText(
                temporaryLauncher,
                BuildBatch(
                    workerExecutable,
                    fallbackArguments,
                    workingDirectory,
                    configurationPath,
                    handoffPath,
                    settings.FallbackHandoffTimeout),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            File.Move(temporaryLauncher, launcherPath, overwrite: true);
            logger.LogWarning(
                "Automatic independent Managed Worker launch was blocked for operation {OperationId}; a one-click recovery launcher was prepared.",
                operation.OperationId);
            return new(launcherPath, configurationPath, handoffPath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            TryDelete(temporaryLauncher);
            TryDelete(configurationPath);
            logger.LogWarning(
                "One-click Managed Worker recovery preparation failed after {ExceptionType}.",
                exception.GetType().FullName);
            throw new ManagedServiceException(
                "MANAGED_WORKER_FALLBACK_PREPARATION_FAILED",
                "Independent worker launch was blocked and the one-click recovery launcher could not be prepared.");
        }
    }

    internal static string BuildBatch(
        string workerExecutable,
        IReadOnlyList<string> workerArguments,
        string workingDirectory,
        string configurationPath,
        string handoffPath,
        TimeSpan handoffTimeout)
    {
        var waitSeconds = Math.Clamp(
            (int)Math.Ceiling((handoffTimeout > TimeSpan.Zero
                ? handoffTimeout
                : TimeSpan.FromSeconds(30)).TotalSeconds),
            1,
            300);
        var command = new StringBuilder("start \"\" /min /d ")
            .Append(BatchQuote(workingDirectory))
            .Append(' ')
            .Append(BatchQuote(workerExecutable));
        foreach (var argument in workerArguments)
        {
            command.Append(' ').Append(BatchQuote(argument));
        }

        return $$"""
            @echo off
            setlocal DisableDelayedExpansion
            {{command}}
            if errorlevel 1 goto :launch_failed
            for /L %%I in (1,1,{{waitSeconds}}) do (
              if exist {{BatchQuote(handoffPath)}} goto :handed_off
              >nul 2>&1 ping -n 2 127.0.0.1
            )
            echo Eternal Cycle could not confirm the Managed Worker handoff. This launcher was kept so setup can be retried.
            exit /b 1
            :launch_failed
            echo Eternal Cycle could not start the Managed Worker. This launcher was kept so setup can be retried.
            exit /b 1
            :handed_off
            del /f /q /a {{BatchQuote(configurationPath)}} >nul 2>&1
            if exist {{BatchQuote(configurationPath)}} goto :cleanup_failed
            del /q {{BatchQuote(handoffPath)}} >nul 2>&1
            (goto) 2>nul & del /q "%~f0"
            :cleanup_failed
            echo Eternal Cycle started the Managed Worker, but temporary setup cleanup was incomplete. This launcher was kept for diagnostics.
            exit /b 1
            """;
    }

    private string ResolveLauncherDirectory()
    {
        if (!string.IsNullOrWhiteSpace(settings.FallbackLauncherDirectory))
        {
            return Path.GetFullPath(settings.FallbackLauncherDirectory);
        }

        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        return string.IsNullOrWhiteSpace(desktop)
            ? Path.GetFullPath(settings.ControlDirectory)
            : desktop;
    }

    private static string BatchQuote(string value) =>
        $"\"{value.Replace("%", "%%", StringComparison.Ordinal)}\"";

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Best-effort rollback. The generated file contains no campaign state.
        }
    }
}

internal sealed class IndependentManagedWorkerLauncher(
    IOptions<ManagedWorkerOptions> options,
    IIndependentProcessStarter processStarter,
    ManagedWorkerFallbackLauncher fallbackLauncher,
    ILogger<IndependentManagedWorkerLauncher> logger) : IManagedWorkerLauncher
{
    private readonly ManagedWorkerOptions settings = options.Value;

    public Task<ManagedWorkerLaunchResult> EnsureLaunchedAsync(
        ManagedOperationStatus operation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var executable = ResolveExecutable();
        var launchId = $"LAUNCH-{Guid.NewGuid():N}";
        var controlDirectory = Path.GetFullPath(settings.ControlDirectory);
        Directory.CreateDirectory(controlDirectory);
        string[] arguments =
        [
            ManagedWorkerCommand.WorkerSwitch,
            ManagedWorkerCommand.OperationSwitch,
            operation.OperationId,
            ManagedWorkerCommand.LaunchSwitch,
            launchId,
            ManagedWorkerCommand.ControlDirectorySwitch,
            controlDirectory
        ];
        try
        {
            var processId = processStarter.Start(new(
                executable,
                arguments,
                AppContext.BaseDirectory));
            logger.LogInformation(
                "Independent Managed Worker launch {LaunchProcessId} completed for operation {OperationId} with launch {LaunchId}.",
                processId,
                operation.OperationId,
                launchId);
            return Task.FromResult(ManagedWorkerLaunchResult.Started());
        }
        catch (Exception exception) when (exception is
            Win32Exception or
            TimeoutException or
            InvalidOperationException)
        {
            logger.LogWarning(
                "Automatic independent Managed Worker launch was blocked after {ExceptionType}; preparing the one-click fallback.",
                exception.GetType().FullName);
            var fallback = fallbackLauncher.Prepare(
                operation,
                executable,
                arguments,
                AppContext.BaseDirectory);
            return Task.FromResult(new ManagedWorkerLaunchResult(
                false,
                true,
                "WORKER_INDEPENDENT_LAUNCH_BLOCKED",
                $"The hosting application prevented independent background execution. Double-click the prepared '{Path.GetFileName(fallback.LauncherPath)}' file to continue the existing setup operation; it only needs to be run once.",
                fallback.LauncherPath));
        }
    }

    private string ResolveExecutable()
    {
        var path = string.IsNullOrWhiteSpace(settings.ExecutablePath)
            ? Path.Combine(
                AppContext.BaseDirectory,
                OperatingSystem.IsWindows()
                    ? "EternalCycle.ManagedWorker.exe"
                    : "EternalCycle.ManagedWorker")
            : Path.GetFullPath(settings.ExecutablePath);
        if (!File.Exists(path))
        {
            throw new ManagedServiceException(
                "MANAGED_WORKER_NOT_FOUND",
                "The independent Managed Worker executable is not available beside the Managed Service distribution.");
        }

        return path;
    }
}

internal sealed record ManagedWorkerCommand(
    string OperationId,
    string LaunchId,
    string ControlDirectory,
    string? FallbackConfigurationFile = null,
    string? HandoffFile = null)
{
    public const string WorkerSwitch = "--managed-worker";
    public const string OperationSwitch = "--operation-id";
    public const string LaunchSwitch = "--launch-id";
    public const string ControlDirectorySwitch = "--control-directory";
    public const string FallbackConfigurationSwitch = "--fallback-configuration";
    public const string HandoffSwitch = "--handoff-file";

    public static bool TryParse(string[] arguments, out ManagedWorkerCommand? command)
    {
        command = null;
        if (!arguments.Contains(WorkerSwitch, StringComparer.Ordinal))
        {
            return false;
        }

        var operationId = Value(arguments, OperationSwitch);
        var launchId = Value(arguments, LaunchSwitch);
        var controlDirectory = Value(arguments, ControlDirectorySwitch);
        var fallbackConfiguration = Value(arguments, FallbackConfigurationSwitch);
        var handoff = Value(arguments, HandoffSwitch);
        if (!SafeIdentity(operationId, "OP-") ||
            !SafeIdentity(launchId, "LAUNCH-") ||
            string.IsNullOrWhiteSpace(controlDirectory) ||
            ((fallbackConfiguration is null) != (handoff is null)))
        {
            throw new ArgumentException("Managed Worker command arguments are invalid.");
        }

        command = new(
            operationId!,
            launchId!,
            Path.GetFullPath(controlDirectory!),
            fallbackConfiguration is null ? null : Path.GetFullPath(fallbackConfiguration),
            handoff is null ? null : Path.GetFullPath(handoff));
        return true;
    }

    private static string? Value(string[] arguments, string name)
    {
        var index = Array.IndexOf(arguments, name);
        return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
    }

    private static bool SafeIdentity(string? value, string prefix) =>
        value is { Length: <= 128 } &&
        value.StartsWith(prefix, StringComparison.Ordinal) &&
        value.All(character => char.IsLetterOrDigit(character) || character == '-');
}

internal sealed record ManagedWorkerDescriptor(
    string OperationId,
    string LaunchId,
    int ProcessId,
    DateTimeOffset StartedAt,
    string ProgressFile);

internal sealed record ManagedWorkerProgress(
    string OperationId,
    string LaunchId,
    int ProcessId,
    ManagedOperationState State,
    string Stage,
    int? ProgressPercent,
    int ExecutionAttempt,
    DateTimeOffset ObservedAt);

internal sealed class ManagedWorkerControlFiles(
    IOptions<ManagedWorkerOptions> options,
    ILogger<ManagedWorkerControlFiles> logger) : IManagedWorkerControl
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ManagedWorkerOptions settings = options.Value;

    internal ManagedWorkerControlFiles(string controlDirectory, ILogger<ManagedWorkerControlFiles> logger)
        : this(Options.Create(new ManagedWorkerOptions { ControlDirectory = controlDirectory }), logger)
    {
    }

    public async Task<ManagedWorkerStopResult> RequestStopAsync(
        string operationId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var descriptorPath = ActivePath(operationId);
        if (!File.Exists(descriptorPath))
        {
            return new(false, "MANAGED_WORKER_NOT_ACTIVE", "No active independent worker is currently registered for that operation.");
        }

        try
        {
            var descriptor = JsonSerializer.Deserialize<ManagedWorkerDescriptor>(
                await File.ReadAllTextAsync(descriptorPath, cancellationToken),
                JsonOptions);
            if (descriptor is null || !string.Equals(descriptor.OperationId, operationId, StringComparison.Ordinal))
            {
                return new(false, "MANAGED_WORKER_CONTROL_INVALID", "The independent worker control record is invalid.");
            }

            await File.WriteAllTextAsync(
                StopPath(operationId, descriptor.LaunchId),
                DateTimeOffset.UtcNow.ToString("O"),
                cancellationToken);
            return new(true, "MANAGED_WORKER_STOP_REQUESTED", "Orderly independent worker cancellation was requested.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            logger.LogWarning(
                "Independent worker stop request for {OperationId} failed after {ExceptionType}.",
                operationId,
                exception.GetType().FullName);
            return new(false, "MANAGED_WORKER_CONTROL_FAILED", "The independent worker stop request could not be recorded.");
        }
    }

    internal FileStream? TryAcquireExecutorLock(string operationId)
    {
        try
        {
            Directory.CreateDirectory(settings.ControlDirectory);
            return new FileStream(
                LockPath(operationId),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None,
                1,
                FileOptions.WriteThrough);
        }
        catch (IOException)
        {
            return null;
        }
    }

    internal void RegisterActive(ManagedWorkerCommand command)
    {
        var descriptor = new ManagedWorkerDescriptor(
            command.OperationId,
            command.LaunchId,
            Environment.ProcessId,
            DateTimeOffset.UtcNow,
            ProgressPath(command.OperationId, command.LaunchId));
        TryWriteJson(ActivePath(command.OperationId), descriptor);
    }

    internal void WriteProgress(ManagedWorkerCommand command, ManagedOperationStatus status) =>
        TryWriteJson(
            ProgressPath(command.OperationId, command.LaunchId),
            new ManagedWorkerProgress(
                status.OperationId,
                command.LaunchId,
                Environment.ProcessId,
                status.State,
                status.CurrentStage,
                status.ProgressPercent,
                status.ExecutionAttempt,
                DateTimeOffset.UtcNow));

    internal async Task MonitorStopAsync(
        ManagedWorkerCommand command,
        Action onStop,
        CancellationToken cancellationToken)
    {
        var delay = settings.StopPollInterval > TimeSpan.Zero
            ? settings.StopPollInterval
            : TimeSpan.FromMilliseconds(250);
        var stopPath = StopPath(command.OperationId, command.LaunchId);
        while (!cancellationToken.IsCancellationRequested)
        {
            if (File.Exists(stopPath))
            {
                onStop();
                return;
            }

            await Task.Delay(delay, cancellationToken);
        }
    }

    internal void Cleanup(ManagedWorkerCommand command)
    {
        TryDelete(StopPath(command.OperationId, command.LaunchId));
        try
        {
            var activePath = ActivePath(command.OperationId);
            if (!File.Exists(activePath))
            {
                return;
            }

            var descriptor = JsonSerializer.Deserialize<ManagedWorkerDescriptor>(
                File.ReadAllText(activePath),
                JsonOptions);
            if (string.Equals(descriptor?.LaunchId, command.LaunchId, StringComparison.Ordinal))
            {
                TryDelete(activePath);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            logger.LogDebug(
                "Independent worker control cleanup for {OperationId} was incomplete after {ExceptionType}.",
                command.OperationId,
                exception.GetType().FullName);
        }
    }

    private void TryWriteJson<T>(string path, T value)
    {
        try
        {
            Directory.CreateDirectory(settings.ControlDirectory);
            var temporary = $"{path}.{Guid.NewGuid():N}.tmp";
            File.WriteAllText(temporary, JsonSerializer.Serialize(value, JsonOptions));
            File.Move(temporary, path, overwrite: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // Files are operator convenience only. Durable operation state remains in SQL.
            logger.LogDebug(
                "Independent worker convenience file {FileName} was unavailable after {ExceptionType}.",
                Path.GetFileName(path),
                exception.GetType().FullName);
        }
    }

    private void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogDebug(
                "Independent worker convenience file cleanup for {FileName} was unavailable after {ExceptionType}.",
                Path.GetFileName(path),
                exception.GetType().FullName);
        }
    }

    private string ActivePath(string operationId) => Path.Combine(settings.ControlDirectory, $"{operationId}.active.json");

    private string LockPath(string operationId) => Path.Combine(settings.ControlDirectory, $"{operationId}.executor.lock");

    private string StopPath(string operationId, string launchId) => Path.Combine(settings.ControlDirectory, $"{operationId}.{launchId}.stop");

    private string ProgressPath(string operationId, string launchId) => Path.Combine(settings.ControlDirectory, $"{operationId}.{launchId}.progress.json");
}

internal sealed class ManagedWorkerRecoveryHostedService(
    IManagedOperationStore store,
    IManagedWorkerLauncher launcher,
    IOptions<ManagedRuleServiceOptions> options,
    ILogger<ManagedWorkerRecoveryHostedService> logger) : BackgroundService
{
    private readonly ManagedRuleServiceOptions settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var delay = settings.ManagedOperationPollInterval > TimeSpan.Zero
            ? settings.ManagedOperationPollInterval
            : TimeSpan.FromSeconds(1);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await store.RecoverInterruptedAsync(stoppingToken);
                var operations = await store.ListRecentAsync(null, 50, stoppingToken);
                foreach (var operation in operations.Where(value => value.State is
                             ManagedOperationState.Queued or
                             ManagedOperationState.Running or
                             ManagedOperationState.Cancelling or
                             ManagedOperationState.Interrupted))
                {
                    await launcher.EnsureLaunchedAsync(operation, CancellationToken.None);
                }

                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    "Independent worker recovery launch is waiting for its durable store after {ExceptionType}.",
                    exception.GetType().FullName);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}

internal static class ManagedWorkerEntrypoint
{
    public static async Task<int> RunAsync(
        IServiceProvider services,
        ManagedWorkerCommand command,
        CancellationToken externalCancellation = default)
    {
        var store = services.GetRequiredService<IManagedOperationStore>();
        var processor = services.GetRequiredService<ManagedOperationProcessor>();
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EternalCycle.ManagedWorker");
        var control = new ManagedWorkerControlFiles(
            command.ControlDirectory,
            loggerFactory.CreateLogger<ManagedWorkerControlFiles>());
        using var executorLock = control.TryAcquireExecutorLock(command.OperationId);
        if (executorLock is null)
        {
            logger.LogInformation(
                "Another local independent worker already monitors operation {OperationId}.",
                command.OperationId);
            return 0;
        }

        if (command.HandoffFile is not null)
        {
            var operation = await store.GetAsync(command.OperationId, externalCancellation);
            if (operation is null)
            {
                logger.LogError("The one-click Managed Worker launcher referenced an unknown durable operation.");
                return 2;
            }

            if (!TryConfirmFallbackHandoff(command.HandoffFile, command.OperationId, command.LaunchId, logger))
            {
                return 3;
            }
        }

        using var stop = CancellationTokenSource.CreateLinkedTokenSource(externalCancellation);
        using var monitorStop = new CancellationTokenSource();
        var cancellationSource = ManagedWorkerCancellationSources.WorkerShutdown;
        void RequestExplicitStop()
        {
            Volatile.Write(
                ref cancellationSource,
                ManagedWorkerCancellationSources.ExplicitWorkerStop);
            stop.Cancel();
        }

        var monitor = control.MonitorStopAsync(command, RequestExplicitStop, monitorStop.Token);
        ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            stop.Cancel();
        };
        EventHandler exitHandler = (_, _) => stop.Cancel();
        Console.CancelKeyPress += cancelHandler;
        AppDomain.CurrentDomain.ProcessExit += exitHandler;
        try
        {
            var delay = services.GetRequiredService<IOptions<ManagedRuleServiceOptions>>().Value.ManagedOperationPollInterval;
            if (delay <= TimeSpan.Zero)
            {
                delay = TimeSpan.FromSeconds(1);
            }

            while (!stop.IsCancellationRequested)
            {
                try
                {
                    await store.RecoverInterruptedAsync(stop.Token);
                    var processed = await processor.ProcessAsync(
                        command.OperationId,
                        stop.Token,
                        () => Volatile.Read(ref cancellationSource),
                        operation =>
                        {
                            control.RegisterActive(command);
                            control.WriteProgress(command, operation);
                        });
                    var status = await store.GetAsync(command.OperationId, CancellationToken.None);
                    if (status is not null)
                    {
                        control.WriteProgress(command, status);
                    }

                    if (processed || status is null || status.State is
                        ManagedOperationState.Succeeded or
                        ManagedOperationState.Failed or
                        ManagedOperationState.Cancelled)
                    {
                        return 0;
                    }
                }
                catch (OperationCanceledException) when (stop.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(
                        "Independent Managed Worker is waiting for its durable control plane after {ExceptionType}.",
                        exception.GetType().FullName);
                }

                await Task.Delay(delay, stop.Token);
            }

            return 0;
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested)
        {
            return 0;
        }
        finally
        {
            Console.CancelKeyPress -= cancelHandler;
            AppDomain.CurrentDomain.ProcessExit -= exitHandler;
            monitorStop.Cancel();
            try
            {
                await monitor;
            }
            catch (OperationCanceledException) when (monitorStop.IsCancellationRequested)
            {
                // Normal worker completion stops the independent control monitor.
            }

            control.Cleanup(command);
        }
    }

    private static bool TryConfirmFallbackHandoff(
        string handoffFile,
        string operationId,
        string launchId,
        ILogger logger)
    {
        try
        {
            var directory = Path.GetDirectoryName(handoffFile);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var temporary = $"{handoffFile}.{Guid.NewGuid():N}.tmp";
            File.WriteAllText(
                temporary,
                JsonSerializer.Serialize(new
                {
                    operationId,
                    launchId,
                    processId = Environment.ProcessId,
                    acknowledgedAt = DateTimeOffset.UtcNow
                }));
            File.Move(temporary, handoffFile, overwrite: true);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogError(
                "The one-click Managed Worker handoff could not be confirmed after {ExceptionType}.",
                exception.GetType().FullName);
            return false;
        }
    }
}
