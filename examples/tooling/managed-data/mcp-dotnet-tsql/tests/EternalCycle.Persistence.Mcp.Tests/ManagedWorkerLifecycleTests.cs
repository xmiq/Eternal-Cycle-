using System.Diagnostics;
using System.ComponentModel;
using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class ManagedWorkerLifecycleTests
{
    [Fact]
    public async Task EnqueueLaunchesIndependentWorkerWithDurableIdentity()
    {
        var root = TemporaryDirectory();
        var executable = Path.Combine(root, "EternalCycle.ManagedWorker.exe");
        await File.WriteAllTextAsync(executable, "fixture");
        var starter = new RecordingProcessStarter();
        var workerOptions = Options.Create(new ManagedWorkerOptions
            {
                ExecutablePath = executable,
                ControlDirectory = root,
                FallbackLauncherDirectory = root
            });
        var fallback = new ManagedWorkerFallbackLauncher(
            workerOptions,
            new ConfigurationBuilder().Build(),
            NullLogger<ManagedWorkerFallbackLauncher>.Instance);
        var launcher = new IndependentManagedWorkerLauncher(
            workerOptions,
            starter,
            fallback,
            NullLogger<IndependentManagedWorkerLauncher>.Instance);
        var store = new MemoryManagedOperationStore();
        var service = new ManagedOperationService(
            store,
            new StaticSourceConfigurationStore(),
            Options.Create(new ManagedRuleServiceOptions()),
            launcher,
            new ManagedWorkerControlFiles(root, NullLogger<ManagedWorkerControlFiles>.Instance));

        var initiation = await service.InitiateInitialRulePublicationAsync(CancellationToken.None);
        var operation = initiation.Operation;

        var request = Assert.Single(starter.Requests);
        Assert.Equal(executable, request.ExecutablePath);
        Assert.Contains(ManagedWorkerCommand.WorkerSwitch, request.Arguments);
        Assert.Contains(operation.OperationId, request.Arguments);
        Assert.Contains(request.Arguments, value => value.StartsWith("LAUNCH-", StringComparison.Ordinal));
        Assert.True(initiation.WorkerLaunch.IndependentWorkerStarted);
        Assert.False(initiation.WorkerLaunch.UserInterventionRequired);
        Assert.False(File.Exists(Path.Combine(root, ManagedWorkerFallbackLauncher.LauncherFileName)));
    }

    [Fact]
    public async Task RefusedIndependentLaunchPreparesOneClickFallbackForSameOperation()
    {
        var root = TemporaryDirectory();
        var executable = Path.Combine(root, "EternalCycle.ManagedWorker.exe");
        await File.WriteAllTextAsync(executable, "fixture");
        const string secret = "Server=secret.invalid;Password=do-not-expose";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EternalCycle:Persistence:ConnectionString"] = secret,
                ["EternalCycle:Rules:RulesetId"] = "eternal-cycle-core"
            })
            .Build();
        var options = Options.Create(new ManagedWorkerOptions
        {
            ExecutablePath = executable,
            ControlDirectory = root,
            FallbackLauncherDirectory = root,
            FallbackHandoffTimeout = TimeSpan.FromSeconds(1)
        });
        var fallbackLog = new RecordingLogger<ManagedWorkerFallbackLauncher>();
        var launcherLog = new RecordingLogger<IndependentManagedWorkerLauncher>();
        var fallback = new ManagedWorkerFallbackLauncher(
            options,
            configuration,
            fallbackLog);
        var launcher = new IndependentManagedWorkerLauncher(
            options,
            new RefusingProcessStarter(),
            fallback,
            launcherLog);
        var store = new MemoryManagedOperationStore();
        var service = new ManagedOperationService(
            store,
            new StaticSourceConfigurationStore(),
            Options.Create(new ManagedRuleServiceOptions()),
            launcher,
            new ManagedWorkerControlFiles(root, NullLogger<ManagedWorkerControlFiles>.Instance));

        var initiation = await service.InitiateInitialRulePublicationAsync(CancellationToken.None);

        Assert.False(initiation.WorkerLaunch.IndependentWorkerStarted);
        Assert.True(initiation.WorkerLaunch.UserInterventionRequired);
        Assert.Equal("WORKER_INDEPENDENT_LAUNCH_BLOCKED", initiation.WorkerLaunch.Code);
        Assert.Equal(
            Path.Combine(root, ManagedWorkerFallbackLauncher.LauncherFileName),
            initiation.WorkerLaunch.PreparedLauncherPath);
        var launcherText = await File.ReadAllTextAsync(initiation.WorkerLaunch.PreparedLauncherPath!);
        Assert.Contains(initiation.Operation.OperationId, launcherText, StringComparison.Ordinal);
        Assert.Contains(ManagedWorkerCommand.FallbackConfigurationSwitch, launcherText, StringComparison.Ordinal);
        Assert.Contains(ManagedWorkerCommand.HandoffSwitch, launcherText, StringComparison.Ordinal);
        Assert.DoesNotContain(secret, launcherText, StringComparison.Ordinal);
        Assert.DoesNotContain(secret, initiation.WorkerLaunch.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(fallbackLog.Messages, message => message.Contains(secret, StringComparison.Ordinal));
        Assert.DoesNotContain(launcherLog.Messages, message => message.Contains(secret, StringComparison.Ordinal));
        Assert.Single(await store.ListRecentAsync(null, 50, CancellationToken.None));
    }

    [Fact]
    public async Task SuccessfulFallbackHandoffSelfDeletesLauncherAndTemporaryConfiguration()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var root = TemporaryDirectory();
        var marker = Path.Combine(root, "same-operation.txt");
        var fakeWorker = Path.Combine(root, "fake-worker.cmd");
        await File.WriteAllTextAsync(fakeWorker, $$"""
            @echo off
            setlocal DisableDelayedExpansion
            set "handoff="
            set "configuration="
            set "operation="
            :parse
            if "%~1"=="" goto :ready
            if /I "%~1"=="{{ManagedWorkerCommand.OperationSwitch}}" set "operation=%~2"
            if /I "%~1"=="{{ManagedWorkerCommand.FallbackConfigurationSwitch}}" set "configuration=%~2"
            if /I "%~1"=="{{ManagedWorkerCommand.HandoffSwitch}}" set "handoff=%~2"
            shift
            goto :parse
            :ready
            if not defined handoff exit /b 4
            if not exist "%configuration%" exit /b 5
            > "{{marker}}" echo %operation%
            > "%handoff%" echo ready
            exit /b 0
            """);
        var options = Options.Create(new ManagedWorkerOptions
        {
            ControlDirectory = root,
            FallbackLauncherDirectory = root,
            FallbackHandoffTimeout = TimeSpan.FromSeconds(5)
        });
        var fallback = new ManagedWorkerFallbackLauncher(
            options,
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["EternalCycle:Persistence:ConnectionString"] = "Server=temporary-secret"
                })
                .Build(),
            NullLogger<ManagedWorkerFallbackLauncher>.Instance);
        var operation = Operation("OP-FALLBACK-SAME", "CORR-FALLBACK-SAME");
        var commandInterpreter = Environment.GetEnvironmentVariable("ComSpec")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        var prepared = fallback.Prepare(
            operation,
            commandInterpreter,
            ["/d", "/c", fakeWorker, ManagedWorkerCommand.OperationSwitch, operation.OperationId],
            root);

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = commandInterpreter,
            UseShellExecute = false,
            CreateNoWindow = true,
            ArgumentList = { "/d", "/c", prepared.LauncherPath }
        }) ?? throw new InvalidOperationException("The generated fallback launcher did not start.");
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
        await WaitForMissingAsync(prepared.LauncherPath, TimeSpan.FromSeconds(3));

        Assert.Equal(0, process.ExitCode);
        Assert.Equal(operation.OperationId, (await File.ReadAllTextAsync(marker)).Trim());
        Assert.False(File.Exists(prepared.ConfigurationPath));
        Assert.False(File.Exists(prepared.HandoffPath));
    }

    [Fact]
    public async Task FailedFallbackLaunchPreservesLauncherAndRetryConfiguration()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var root = TemporaryDirectory();
        var options = Options.Create(new ManagedWorkerOptions
        {
            ControlDirectory = root,
            FallbackLauncherDirectory = root,
            FallbackHandoffTimeout = TimeSpan.FromSeconds(1)
        });
        var fallback = new ManagedWorkerFallbackLauncher(
            options,
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["EternalCycle:Persistence:ConnectionString"] = "Server=retry-secret"
                })
                .Build(),
            NullLogger<ManagedWorkerFallbackLauncher>.Instance);
        var operation = Operation("OP-FALLBACK-RETRY", "CORR-FALLBACK-RETRY");
        var failedWorker = Path.Combine(root, "failed-worker.cmd");
        await File.WriteAllTextAsync(failedWorker, "@echo off\r\nexit /b 9\r\n");
        var commandInterpreter = Environment.GetEnvironmentVariable("ComSpec")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        var prepared = fallback.Prepare(
            operation,
            commandInterpreter,
            ["/d", "/c", failedWorker, ManagedWorkerCommand.OperationSwitch, operation.OperationId],
            root);

        using var process = Process.Start(new ProcessStartInfo
        {
            FileName = commandInterpreter,
            UseShellExecute = false,
            CreateNoWindow = true,
            ArgumentList = { "/d", "/c", prepared.LauncherPath }
        }) ?? throw new InvalidOperationException("The generated fallback launcher did not start.");
        await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(6));

        Assert.NotEqual(0, process.ExitCode);
        Assert.True(File.Exists(prepared.LauncherPath));
        Assert.True(File.Exists(prepared.ConfigurationPath));
        Assert.False(File.Exists(prepared.HandoffPath));
    }

    [Fact]
    public async Task IndependentExecutionKeepsSameAttemptAfterInitiatingRequestEnds()
    {
        var store = new MemoryManagedOperationStore();
        var publisher = new BlockingPublisher();
        var processor = Processor(store, publisher);
        var service = OperationService(store);
        using var initiatingRequest = new CancellationTokenSource();
        var operation = await service.EnqueueInitialRulePublicationAsync(initiatingRequest.Token);

        var execution = processor.ProcessAsync(operation.OperationId, CancellationToken.None);
        await publisher.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        initiatingRequest.Cancel();
        publisher.Complete();
        Assert.True(await execution);

        var completed = await store.GetAsync(operation.OperationId, CancellationToken.None);
        var reconnected = await OperationService(store).GetAsync(operation.OperationId, CancellationToken.None);
        Assert.Equal(ManagedOperationState.Succeeded, completed?.State);
        Assert.Equal(ManagedOperationState.Succeeded, reconnected?.State);
        Assert.Equal(1, completed?.ExecutionAttempt);
        Assert.Equal(operation.OperationId, reconnected?.OperationId);
        Assert.Equal(operation.CorrelationId, completed?.CorrelationId);
    }

    [Fact]
    public async Task DuplicateExecutorCannotClaimSameOperation()
    {
        var store = new MemoryManagedOperationStore();
        var publisher = new BlockingPublisher();
        var operation = await OperationService(store)
            .EnqueueInitialRulePublicationAsync(CancellationToken.None);
        var first = Processor(store, publisher).ProcessAsync(operation.OperationId, CancellationToken.None);
        await publisher.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var duplicate = await Processor(store, new ImmediatePublisher())
            .ProcessAsync(operation.OperationId, CancellationToken.None);
        var running = await store.GetAsync(operation.OperationId, CancellationToken.None);
        publisher.Complete();
        Assert.True(await first);

        Assert.False(duplicate);
        Assert.Equal(1, running?.ExecutionAttempt);
    }

    [Fact]
    public async Task WorkerEntrypointStopsBlockedWorkWithoutWaitingForForwardProgress()
    {
        var root = TemporaryDirectory();
        var store = new MemoryManagedOperationStore();
        var publisher = new BlockingPublisher();
        var operation = await OperationService(store)
            .EnqueueInitialRulePublicationAsync(CancellationToken.None);
        var command = new ManagedWorkerCommand(operation.OperationId, "LAUNCH-STOP", root);
        await using var services = Services(store, publisher);
        var worker = ManagedWorkerEntrypoint.RunAsync(services, command);
        await publisher.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var control = new ManagedWorkerControlFiles(root, NullLogger<ManagedWorkerControlFiles>.Instance);

        var stop = await control.RequestStopAsync(operation.OperationId, CancellationToken.None);
        Assert.True(stop.Accepted);
        Assert.Equal(0, await worker.WaitAsync(TimeSpan.FromSeconds(3)));

        var result = await store.GetAsync(operation.OperationId, CancellationToken.None);
        Assert.Equal(ManagedOperationState.Cancelled, result?.State);
        Assert.Equal("MANAGED_OPERATION_CANCELLED", result?.ErrorCode);
    }

    [Fact]
    public async Task StaleStopSignalCannotCancelLaterLaunch()
    {
        var root = TemporaryDirectory();
        var control = new ManagedWorkerControlFiles(root, NullLogger<ManagedWorkerControlFiles>.Instance);
        var oldLaunch = new ManagedWorkerCommand("OP-STABLE", "LAUNCH-OLD", root);
        var newLaunch = new ManagedWorkerCommand("OP-STABLE", "LAUNCH-NEW", root);
        control.RegisterActive(oldLaunch);
        Assert.True((await control.RequestStopAsync(oldLaunch.OperationId, CancellationToken.None)).Accepted);
        control.RegisterActive(newLaunch);
        using var stop = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        var observed = false;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            control.MonitorStopAsync(newLaunch, () => observed = true, stop.Token));

        Assert.False(observed);
    }

    [Fact]
    public async Task ProgressFileFailureDoesNotChangeCanonicalOperationOutcome()
    {
        var root = TemporaryDirectory();
        var store = new MemoryManagedOperationStore();
        var operation = await OperationService(store)
            .EnqueueInitialRulePublicationAsync(CancellationToken.None);
        var launchId = "LAUNCH-PROGRESS";
        Directory.CreateDirectory(Path.Combine(root, $"{operation.OperationId}.{launchId}.progress.json"));
        await using var services = Services(store, new ImmediatePublisher());

        var exitCode = await ManagedWorkerEntrypoint.RunAsync(
            services,
            new ManagedWorkerCommand(operation.OperationId, launchId, root));

        Assert.Equal(0, exitCode);
        Assert.Equal(
            ManagedOperationState.Succeeded,
            (await store.GetAsync(operation.OperationId, CancellationToken.None))?.State);
    }

    [Fact]
    public async Task WorkerConfirmsFallbackHandoffOnlyForExistingDurableOperation()
    {
        var root = TemporaryDirectory();
        var store = new MemoryManagedOperationStore();
        var operation = await OperationService(store)
            .EnqueueInitialRulePublicationAsync(CancellationToken.None);
        var handoff = Path.Combine(root, "handoff.ready");
        await using var services = Services(store, new ImmediatePublisher());

        var exitCode = await ManagedWorkerEntrypoint.RunAsync(
            services,
            new ManagedWorkerCommand(
                operation.OperationId,
                "LAUNCH-FALLBACK",
                root,
                Path.Combine(root, "configuration.json"),
                handoff));

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(handoff));
        var evidence = await File.ReadAllTextAsync(handoff);
        Assert.Contains(operation.OperationId, evidence, StringComparison.Ordinal);
        Assert.Contains("LAUNCH-FALLBACK", evidence, StringComparison.Ordinal);
        Assert.Equal(
            ManagedOperationState.Succeeded,
            (await store.GetAsync(operation.OperationId, CancellationToken.None))?.State);
        Assert.Single(await store.ListRecentAsync(null, 50, CancellationToken.None));
    }

    [Fact]
    public async Task FallbackWorkerRefusesUnknownReplacementOperation()
    {
        var root = TemporaryDirectory();
        var handoff = Path.Combine(root, "handoff.ready");
        await using var services = Services(new MemoryManagedOperationStore(), new ImmediatePublisher());

        var exitCode = await ManagedWorkerEntrypoint.RunAsync(
            services,
            new ManagedWorkerCommand(
                "OP-NOT-EXISTING",
                "LAUNCH-UNKNOWN",
                root,
                Path.Combine(root, "configuration.json"),
                handoff));

        Assert.Equal(2, exitCode);
        Assert.False(File.Exists(handoff));
    }

    [Fact]
    public void WorkerProgramLoadsPreparedConfigurationWithoutCommandLineSecrets()
    {
        var program = File.ReadAllText(Path.Combine(Path.GetDirectoryName(ProjectPath())!, "Program.cs"));

        Assert.Contains("FallbackConfigurationFile", program, StringComparison.Ordinal);
        Assert.Contains("AddInMemoryCollection", program, StringComparison.Ordinal);
        Assert.DoesNotContain("ConnectionString,", program, StringComparison.Ordinal);
    }

    [Fact]
    public void PackagedBuildDefinesDistinctManagedWorkerAppHost()
    {
        var project = File.ReadAllText(ProjectPath());

        Assert.Contains("EternalCycle.ManagedWorker.exe", project, StringComparison.Ordinal);
        Assert.Contains("CreateIndependentManagedWorkerAppHost", project, StringComparison.Ordinal);
        Assert.Contains("CreatePublishedIndependentManagedWorkerAppHost", project, StringComparison.Ordinal);
    }

    [Fact]
    public void McpHostDoesNotOwnManagedOperationExecutionLoop()
    {
        var program = File.ReadAllText(Path.Combine(Path.GetDirectoryName(ProjectPath())!, "Program.cs"));

        Assert.DoesNotContain("AddHostedService<ManagedOperationWorker>", program, StringComparison.Ordinal);
        Assert.Contains("ManagedWorkerEntrypoint.RunAsync", program, StringComparison.Ordinal);
        Assert.Contains("AddHostedService<ManagedWorkerRecoveryHostedService>", program, StringComparison.Ordinal);
    }

    [Fact]
    public void WindowsLaunchUsesBreakawayTrampolineAndNewProcessGroup()
    {
        var runtime = File.ReadAllText(Path.Combine(Path.GetDirectoryName(ProjectPath())!, "ManagedWorkerRuntime.cs"));

        Assert.Contains("CreateBreakawayFromJob", runtime, StringComparison.Ordinal);
        Assert.Contains("CreateNewProcessGroup", runtime, StringComparison.Ordinal);
        Assert.Contains("cmd.exe trampoline", runtime, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WaitForSingleObject", runtime, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WindowsWorkerSurvivesEphemeralLauncherProcessTreeTermination()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var root = TemporaryDirectory();
        var statusPath = Path.Combine(root, "worker-status.txt");
        var errorPath = Path.Combine(root, "launcher-error.txt");
        var workerScript = $"""
            [IO.File]::WriteAllText('{PowerShellLiteral(statusPath)}', ([Diagnostics.Process]::GetCurrentProcess().Id).ToString())
            Start-Sleep -Milliseconds 750
            [IO.File]::AppendAllText('{PowerShellLiteral(statusPath)}', '|completed')
            Start-Sleep -Seconds 10
            """;
        var workerEncoded = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(workerScript));
        var assemblyPath = typeof(ManagedWorkerOptions).Assembly.Location;
        var powerShell = PowerShellExecutable();
        var parentScript = $$"""
            $ErrorActionPreference = 'Stop'
            try {
            $assembly = [Reflection.Assembly]::LoadFrom('{{PowerShellLiteral(assemblyPath)}}')
            $starterType = $assembly.GetType('EternalCycle.Persistence.Mcp.PlatformIndependentProcessStarter', $true)
            $requestType = $assembly.GetType('EternalCycle.Persistence.Mcp.IndependentProcessStartRequest', $true)
            $flags = [Reflection.BindingFlags]'Instance,Public,NonPublic'
            $starter = $starterType.GetConstructors($flags)[0].Invoke(@())
            [string[]]$workerArgs = @('-NoProfile', '-EncodedCommand', '{{workerEncoded}}')
            [object[]]$constructorArguments = @('{{PowerShellLiteral(powerShell)}}', $null, '{{PowerShellLiteral(root)}}')
            $constructorArguments[1] = $workerArgs
            $request = $requestType.GetConstructors($flags)[0].Invoke($constructorArguments)
            $launchProcessId = $starterType.GetMethod('Start').Invoke($starter, @($request))
            Write-Output "LaunchProcessId=$launchProcessId"
            Start-Sleep -Seconds 20
            } catch {
              [IO.File]::WriteAllText('{{PowerShellLiteral(errorPath)}}', ($_ | Out-String))
              exit 1
            }
            """;
        var parentEncoded = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(parentScript));
        using var parent = Process.Start(new ProcessStartInfo
        {
            FileName = powerShell,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            ArgumentList =
            {
                "-NoProfile",
                "-EncodedCommand",
                parentEncoded
            }
        }) ?? throw new InvalidOperationException("The ephemeral launcher fixture did not start.");

        try
        {
            await WaitForFileAsync(statusPath, TimeSpan.FromSeconds(8));
        }
        catch
        {
            if (!parent.HasExited)
            {
                parent.Kill(entireProcessTree: true);
                await parent.WaitForExitAsync();
            }

            var output = await parent.StandardOutput.ReadToEndAsync();
            var error = await parent.StandardError.ReadToEndAsync();
            var detail = File.Exists(errorPath) ? await File.ReadAllTextAsync(errorPath) : error;
            Assert.Fail($"The production launcher probe did not start its worker. stdout={output} stderr={detail}");
        }
        var workerProcessId = int.Parse(await File.ReadAllTextAsync(statusPath));
        parent.Kill(entireProcessTree: true);
        await parent.WaitForExitAsync();
        await WaitForTextAsync(statusPath, "|completed", TimeSpan.FromSeconds(5));

        Assert.Contains("|completed", await File.ReadAllTextAsync(statusPath), StringComparison.Ordinal);
        try
        {
            Process.GetProcessById(workerProcessId).Kill();
        }
        catch (ArgumentException)
        {
            // The probe may already have exited after proving independent progress.
        }
    }

    private static ManagedOperationService OperationService(MemoryManagedOperationStore store) =>
        new(
            store,
            new StaticSourceConfigurationStore(),
            Options.Create(new ManagedRuleServiceOptions()));

    private static ManagedOperationProcessor Processor(
        IManagedOperationStore store,
        IRulePublicationExecutor publisher) =>
        new(
            store,
            publisher,
            Options.Create(new ManagedRuleServiceOptions
            {
                ManagedOperationTimeout = TimeSpan.FromSeconds(10),
                ManagedOperationLeaseDuration = TimeSpan.FromSeconds(1),
                ManagedOperationPollInterval = TimeSpan.FromMilliseconds(10)
            }),
            NullLogger<ManagedOperationProcessor>.Instance);

    private static ServiceProvider Services(
        IManagedOperationStore store,
        IRulePublicationExecutor publisher)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(store);
        services.AddSingleton(publisher);
        services.AddSingleton(Options.Create(new ManagedRuleServiceOptions
        {
            ManagedOperationTimeout = TimeSpan.FromSeconds(10),
            ManagedOperationLeaseDuration = TimeSpan.FromSeconds(1),
            ManagedOperationPollInterval = TimeSpan.FromMilliseconds(10)
        }));
        services.AddSingleton(serviceProvider => new ManagedOperationProcessor(
            serviceProvider.GetRequiredService<IManagedOperationStore>(),
            serviceProvider.GetRequiredService<IRulePublicationExecutor>(),
            serviceProvider.GetRequiredService<IOptions<ManagedRuleServiceOptions>>(),
            serviceProvider.GetRequiredService<ILogger<ManagedOperationProcessor>>()));
        return services.BuildServiceProvider();
    }

    private static string TemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "EternalCycle.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string PowerShellLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    private static string PowerShellExecutable()
    {
        var installed = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "PowerShell",
            "7",
            "pwsh.exe");
        if (File.Exists(installed))
        {
            return installed;
        }

        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                     .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var candidate = Path.Combine(directory, "pwsh.exe");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("PowerShell 7 is required by the Windows process-lifetime fixture.");
    }

    private static async Task WaitForFileAsync(string path, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (!File.Exists(path) && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(50);
        }

        Assert.True(File.Exists(path), $"Timed out waiting for {path}.");
    }

    private static async Task WaitForTextAsync(string path, string expected, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (DateTimeOffset.UtcNow < deadline)
        {
            if (File.Exists(path) &&
                (await File.ReadAllTextAsync(path)).Contains(expected, StringComparison.Ordinal))
            {
                return;
            }

            await Task.Delay(50);
        }

        Assert.Fail($"Timed out waiting for {expected} in {path}.");
    }

    private static async Task WaitForMissingAsync(string path, TimeSpan timeout)
    {
        var deadline = DateTimeOffset.UtcNow.Add(timeout);
        while (File.Exists(path) && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(50);
        }

        Assert.False(File.Exists(path), $"Timed out waiting for {path} to be removed.");
    }

    private static ManagedOperationStatus Operation(string operationId, string correlationId) =>
        new(
            operationId,
            ManagedOperationKinds.InitialRulePublication,
            correlationId,
            ManagedOperationState.Queued,
            "Queued",
            DateTimeOffset.UtcNow,
            null,
            DateTimeOffset.UtcNow,
            null,
            null,
            "Queued for fixture execution.",
            null,
            true,
            false,
            false,
            "eternal-cycle-core",
            null,
            null,
            0);

    private static string ProjectPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(
                directory.FullName,
                "src",
                "EternalCycle.Persistence.Mcp",
                "EternalCycle.Persistence.Mcp.csproj");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Reference MCP project root was not found.");
    }

    private sealed class RecordingProcessStarter : IIndependentProcessStarter
    {
        public List<IndependentProcessStartRequest> Requests { get; } = [];

        public int Start(IndependentProcessStartRequest request)
        {
            Requests.Add(request);
            return 4242;
        }
    }

    private sealed class RefusingProcessStarter : IIndependentProcessStarter
    {
        public int Start(IndependentProcessStartRequest request) =>
            throw new Win32Exception(5, "Fixture host refused Job Object breakaway.");
    }

    private sealed class RecordingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Messages.Add(formatter(state, exception));
    }

    private sealed class StaticSourceConfigurationStore : IRuleSourceConfigurationStore
    {
        public Task<RuleSourceConfiguration?> GetAsync(
            string rulesetId,
            CancellationToken cancellationToken) =>
            Task.FromResult<RuleSourceConfiguration?>(new(
                rulesetId,
                "Git",
                "https://example.invalid/rules.git",
                "refs/tags/v1.1.0-rc",
                "docs/rules/rule-source-manifest.json",
                true,
                7,
                DateTimeOffset.UnixEpoch,
                RuleSourceReleaseChannel.Prerelease));

        public Task<RuleSourceConfiguration> SaveAsync(
            RuleSourceConfiguration value,
            CancellationToken cancellationToken) => Task.FromResult(value);
    }

    private sealed class BlockingPublisher : IRulePublicationExecutor
    {
        private readonly TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<RulePublicationResult> ExecuteAsync(CancellationToken cancellationToken) =>
            ExecuteAsync(null, cancellationToken);

        public async Task<RulePublicationResult> ExecuteAsync(
            Action<RulePublicationStage>? onStage,
            CancellationToken cancellationToken)
        {
            onStage?.Invoke(RulePublicationStage.AcquireSource);
            Started.TrySetResult();
            await release.Task.WaitAsync(cancellationToken);
            return Success();
        }

        public void Complete() => release.TrySetResult();
    }

    private sealed class ImmediatePublisher : IRulePublicationExecutor
    {
        public Task<RulePublicationResult> ExecuteAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Success());
    }

    private static RulePublicationResult Success() =>
        new(
            "Activated",
            "RULE-RESULT",
            "RULE-RESULT",
            "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            false,
            null,
            Operation: ManagedOperationKinds.InitialRulePublication,
            Stage: RulePublicationStage.RecordUpdateCheck.ToString());
}
