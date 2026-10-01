using System.Security;

namespace EternalCycle.Rules;

public sealed class LocalCompiledRulesArtifactProvider : ICompiledRulesArtifactProvider
{
    private readonly string artifactPath;
    private readonly string[] pathComponents;
    private readonly Func<string, FileStream> openFile;

    public LocalCompiledRulesArtifactProvider(string artifactPath) : this(artifactPath, OpenFile) { }

    // A narrow internal seam exercises cancellation, failures and path replacement
    // at the real file boundary without timing-dependent filesystem tests.
    internal LocalCompiledRulesArtifactProvider(string artifactPath, Func<string, FileStream> openFile)
    {
        if (string.IsNullOrWhiteSpace(artifactPath))
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
        ArgumentNullException.ThrowIfNull(openFile);
        try
        {
            if (!Path.IsPathFullyQualified(artifactPath) || artifactPath.Any(char.IsControl) ||
                (OperatingSystem.IsWindows() && (artifactPath.StartsWith(@"\\?\", StringComparison.Ordinal) || artifactPath.StartsWith(@"\\.\", StringComparison.Ordinal))))
                throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.LocatorInvalid);

            this.artifactPath = Path.GetFullPath(artifactPath);
            // Windows device namespaces and alternate streams are not ordinary
            // artifact files. A drive's colon belongs only to the explicit root.
            if (OperatingSystem.IsWindows() && this.artifactPath[Path.GetPathRoot(this.artifactPath)!.Length..].Contains(':'))
                throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.LocatorInvalid);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.LocatorInvalid);
        }
        var root = Path.GetPathRoot(this.artifactPath)!;
        var components = new List<string> { root };
        foreach (var segment in this.artifactPath[root.Length..].Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries))
            components.Add(Path.Combine(components[^1], segment));
        pathComponents = components.ToArray();
        this.openFile = openFile;
    }

    public async Task<AcquiredCompiledRulesArtifact> AcquireAsync(CompiledRulesAcquisitionLimits limits, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(limits);
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            CheckPath(cancellationToken);
            await using var stream = openFile(artifactPath);
            cancellationToken.ThrowIfCancellationRequested();
            CheckFileAttributes(File.GetAttributes(stream.SafeFileHandle));
            if (!stream.CanRead || !stream.CanSeek)
                throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.LocatorInvalid);

            // Inspect the opened object, not a second opened path. Rechecking
            // components catches persistent link swaps, not all hostile races.
            CheckPath(cancellationToken);
            var acquired = await AcquiredCompiledRulesArtifact.ReadAsync(
                stream, new("local-file"), limits, cancellationToken);
            CheckPath(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return acquired;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.LocatorInvalid);
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException or UnauthorizedAccessException or SecurityException)
        {
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.Unavailable);
        }
        catch (IOException)
        {
            // OS errors can disclose private paths or other uncontrolled text.
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.TransportFailed);
        }
    }

    public override string ToString() => nameof(LocalCompiledRulesArtifactProvider);

    private static FileStream OpenFile(string path) => new(path, new FileStreamOptions
    {
        Mode = FileMode.Open,
        Access = FileAccess.Read,
        Share = FileShare.Read,
        Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
        BufferSize = 8192
    });

    private void CheckPath(CancellationToken cancellationToken)
    {
        for (var index = 0; index < pathComponents.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CheckComponent(pathComponents[index], isFile: index == pathComponents.Length - 1);
        }
    }

    private static void CheckComponent(string path, bool isFile)
    {
        var attributes = File.GetAttributes(path);
        if (isFile) CheckFileAttributes(attributes);
        else if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0 || (attributes & FileAttributes.Directory) == 0)
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.LocatorInvalid);
    }

    private static void CheckFileAttributes(FileAttributes attributes)
    {
        if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Directory | FileAttributes.Device)) != 0)
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.LocatorInvalid);
    }
}
