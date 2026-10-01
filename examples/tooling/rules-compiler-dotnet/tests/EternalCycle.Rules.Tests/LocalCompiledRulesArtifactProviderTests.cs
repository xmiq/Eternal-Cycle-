using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class LocalCompiledRulesArtifactProviderTests
{
    [Fact]
    public async Task IsolatedOrdinaryArtifactPassesAAndExplicitBApproval()
    {
        using var files = new Files();
        var bytes = Fixture();
        var path = files.Write("manual-artifact.json", bytes);
        var acquired = await new LocalCompiledRulesArtifactProvider(path).AcquireAsync(new(), default);
        Assert.Equal(bytes, acquired.Bytes.ToArray());
        Assert.Equal(Hash(bytes), acquired.ByteSha256);
        Assert.Equal("local-file", acquired.Evidence.ProviderKind);
        Assert.Null(acquired.Evidence.ResolvedIdentity);
        Assert.Empty(acquired.Evidence.Metadata);
        Assert.Single(Directory.GetFiles(files.Root));
        var result = await Validate(acquired);
        Assert.True(result.IsImportEligible);
        Assert.Equal(bytes, result.TrustedArtifact!.CopyBytes());
    }

    [Theory]
    [InlineData("malformed")]
    [InlineData("utf8")]
    [InlineData("empty")]
    public async Task ArbitraryBytesAreAcquiredThenRejectedOnlyByB(string kind)
    {
        using var files = new Files();
        var bytes = kind == "malformed" ? "{broken"u8.ToArray() : kind == "utf8" ? new byte[] { 0xC3, 0x28 } : [];
        var acquired = await new LocalCompiledRulesArtifactProvider(files.Write("input", bytes)).AcquireAsync(new(), default);
        Assert.Equal(bytes, acquired.Bytes.ToArray());
        var policy = new Policy();
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy);
        Assert.Equal(CompiledRulesAcquisitionFailure.MalformedArtifact, result.Failure);
        Assert.Equal(0, policy.Calls);
    }

    [Fact]
    public async Task LocalPlacementCannotOverrideTrustRejection()
    {
        using var files = new Files();
        var acquired = await new LocalCompiledRulesArtifactProvider(files.Write("valid", Fixture())).AcquireAsync(new(), default);
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new Policy(CompiledRulesArtifactTrustReason.ExplicitRejection));
        Assert.True(result.IsArtifactValid);
        Assert.False(result.IsImportEligible);
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustRejected, result.Failure);
    }

    [Fact]
    public async Task MissingFileIsUnavailableWithoutDisclosingLocator()
    {
        using var files = new Files();
        var path = Path.Combine(files.Root, "private-token.json");
        var error = await Failure(new(path), CompiledRulesAcquisitionFailure.Unavailable);
        Assert.DoesNotContain(path, error.Message);
        Assert.DoesNotContain("private-token", error.ToString());
        Assert.Null(error.InnerException);
    }

    [Fact]
    public async Task DirectoryAndNonDirectoryParentAreInvalidLocators()
    {
        using var files = new Files();
        await Failure(new(files.Root), CompiledRulesAcquisitionFailure.LocatorInvalid);
        var path = files.Write("file", [1]);
        await Failure(new(Path.Combine(path, "child")), CompiledRulesAcquisitionFailure.LocatorInvalid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingConfigurationFailsSafely(string? path)
    {
        var error = Assert.Throws<CompiledRulesAcquisitionException>(() => new LocalCompiledRulesArtifactProvider(path!));
        Assert.Equal(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid, error.Failure);
    }

    [Theory]
    [InlineData("artifact.json")]
    [InlineData(".")]
    [InlineData("../artifact.json")]
    public void RelativeLocatorsNeverDiscoverACurrentDirectory(string path)
    {
        var error = Assert.Throws<CompiledRulesAcquisitionException>(() => new LocalCompiledRulesArtifactProvider(path));
        Assert.Equal(CompiledRulesAcquisitionFailure.LocatorInvalid, error.Failure);
    }

    [Fact]
    public void ControlCharacterLocatorFailsWithoutEchoingIt()
    {
        var path = Path.Combine(Path.GetTempPath(), "private-token\0artifact");
        var error = Assert.Throws<CompiledRulesAcquisitionException>(() => new LocalCompiledRulesArtifactProvider(path));
        Assert.Equal(CompiledRulesAcquisitionFailure.LocatorInvalid, error.Failure);
        Assert.DoesNotContain("private-token", error.ToString());
    }

    [WindowsFact]
    public void WindowsDeviceNamespacesAndAlternateStreamsAreNotOrdinaryLocators()
    {
        using var files = new Files();
        foreach (var path in new[] { @"\\.\NUL", @"\\?\" + Path.Combine(files.Root, "file"), Path.Combine(files.Root, "file:stream") })
        {
            var error = Assert.Throws<CompiledRulesAcquisitionException>(() => new LocalCompiledRulesArtifactProvider(path));
            Assert.Equal(CompiledRulesAcquisitionFailure.LocatorInvalid, error.Failure);
        }
    }

    [Fact]
    public async Task AbsoluteLocatorIsIndependentOfWorkingDirectoryAndUsesPlatformSeparators()
    {
        using var files = new Files();
        var directory = Directory.CreateDirectory(Path.Combine(files.Root, "nested"));
        var path = Path.Combine(directory.FullName, "artifact with spaces.json");
        File.WriteAllBytes(path, Fixture());
        var provider = new LocalCompiledRulesArtifactProvider(path);
        var previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = Path.GetTempPath();
            Assert.Equal(Fixture(), (await provider.AcquireAsync(new(), default)).Bytes.ToArray());
        }
        finally { Environment.CurrentDirectory = previous; }
    }

    [Theory]
    [InlineData("lf")]
    [InlineData("crlf")]
    [InlineData("bom")]
    public async Task ExactBinaryPayloadIsNeverDecodedOrNormalized(string kind)
    {
        using var files = new Files();
        var bytes = kind switch
        {
            "lf" => "line\n"u8.ToArray(),
            "crlf" => "line\r\n"u8.ToArray(),
            _ => new byte[] { 0xEF, 0xBB, 0xBF, 0xFF, 0x00, 0x0D, 0x0A }
        };
        var acquired = await new LocalCompiledRulesArtifactProvider(files.Write("data", bytes)).AcquireAsync(new(), default);
        Assert.Equal(bytes, acquired.Bytes.ToArray());
        Assert.Equal(Hash(bytes), acquired.ByteSha256);
    }

    [Fact]
    public async Task DifferentFilesCannotChangeArtifactIdentity()
    {
        using var files = new Files();
        var first = await new LocalCompiledRulesArtifactProvider(files.Write("first", Fixture())).AcquireAsync(new(), default);
        var second = await new LocalCompiledRulesArtifactProvider(files.Write("second", Fixture())).AcquireAsync(new(), default);
        Assert.Equal(first.ByteSha256, second.ByteSha256);
        var a = (await Validate(first)).TrustedArtifact!;
        var b = (await Validate(second)).TrustedArtifact!;
        Assert.Equal(a.SemanticSha256, b.SemanticSha256);
        Assert.Equal(JsonSerializer.Serialize(a.Artifact), JsonSerializer.Serialize(b.Artifact));
    }

    [Fact]
    public async Task SameLocatorIsMutableAndNextAcquisitionUsesNewBytes()
    {
        using var files = new Files();
        var path = files.Write("artifact", Fixture());
        var provider = new LocalCompiledRulesArtifactProvider(path);
        var first = await provider.AcquireAsync(new(), default);
        File.WriteAllBytes(path, "{broken"u8.ToArray());
        var second = await provider.AcquireAsync(new(), default);
        Assert.NotEqual(first.ByteSha256, second.ByteSha256);
        Assert.True((await Validate(first)).IsImportEligible);
        Assert.Equal(CompiledRulesAcquisitionFailure.MalformedArtifact, (await Validate(second)).Failure);
    }

    [Theory]
    [InlineData(128, true)]
    [InlineData(129, false)]
    public async Task LimitUsesSharedStreamProbeNotLengthOrTruncatedSuccess(int length, bool allowed)
    {
        using var files = new Files();
        var path = files.Write("data", new byte[length]);
        var stream = new ObservedFileStream(path);
        var provider = new LocalCompiledRulesArtifactProvider(path, _ => stream);
        if (allowed) Assert.Equal(length, (await provider.AcquireAsync(new(128), default)).Bytes.Length);
        else await Failure(provider, CompiledRulesAcquisitionFailure.PayloadTooLarge, new(128));
        Assert.Equal(length, stream.BytesRead);
        AssertReleased(path);
    }

    [Fact]
    public async Task DefaultSixteenMiBBoundIsEnforcedOnARealFile()
    {
        using var files = new Files();
        var path = files.Write("large", []);
        using (var stream = File.OpenWrite(path)) stream.SetLength(CompiledRulesAcquisitionLimits.DefaultMaximumPayloadBytes + 1L);
        await Failure(new(path), CompiledRulesAcquisitionFailure.PayloadTooLarge);
        AssertReleased(path);
    }

    [Fact]
    public async Task PreCancellationDoesNotOpenTheFile()
    {
        using var files = new Files();
        var calls = 0;
        var provider = new LocalCompiledRulesArtifactProvider(Path.Combine(files.Root, "absent"), path => { calls++; return new ObservedFileStream(path); });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => provider.AcquireAsync(new(), cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task CancellationWhileReadIsBlockedCannotEmitSuccessAndClosesHandle()
    {
        using var files = new Files();
        var path = files.Write("data", Fixture());
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stream = new ObservedFileStream(path)
        {
            BeforeRead = async token => { started.SetResult(); await Task.Delay(Timeout.Infinite, token); }
        };
        var provider = new LocalCompiledRulesArtifactProvider(path, _ => stream);
        using var cancellation = new CancellationTokenSource();
        var pending = provider.AcquireAsync(new(), cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(0, stream.BytesRead);
        AssertReleased(path);
    }

    [Fact]
    public async Task CancellationAfterReadStillCannotEmitSuccess()
    {
        using var files = new Files();
        var path = files.Write("data", [1, 2, 3]);
        using var cancellation = new CancellationTokenSource();
        var stream = new ObservedFileStream(path) { AfterRead = _ => cancellation.Cancel() };
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new LocalCompiledRulesArtifactProvider(path, _ => stream).AcquireAsync(new(), cancellation.Token));
        AssertReleased(path);
    }

    [Theory]
    [InlineData("permission", CompiledRulesAcquisitionFailure.Unavailable)]
    [InlineData("io", CompiledRulesAcquisitionFailure.TransportFailed)]
    public async Task OpenFailureIsStructuredWithoutRawOsText(string kind, CompiledRulesAcquisitionFailure failure)
    {
        using var files = new Files();
        var path = files.Write("data", [1]);
        var error = await Failure(new(path, _ => throw (kind == "permission" ? new UnauthorizedAccessException("private-token") : new IOException("private-token"))), failure);
        Assert.DoesNotContain("private-token", error.ToString());
        Assert.Null(error.InnerException);
    }

    [Fact]
    public async Task ReadFailureCannotReturnPartialSuccessOrLeakOsText()
    {
        using var files = new Files();
        var path = files.Write("data", Fixture());
        var bytesRead = 0;
        var stream = new ObservedFileStream(path)
        {
            MaximumRead = 32,
            BeforeRead = _ => { if (bytesRead > 0) throw new IOException("private-token"); return ValueTask.CompletedTask; },
            AfterRead = count => bytesRead = count
        };
        var error = await Failure(new(path, _ => stream), CompiledRulesAcquisitionFailure.TransportFailed);
        Assert.Equal(32, stream.BytesRead);
        Assert.DoesNotContain("private-token", error.ToString());
        AssertReleased(path);
    }

    [Fact]
    public async Task NonSeekableOpenedObjectIsNotAnOrdinaryArtifact()
    {
        using var files = new Files();
        var path = files.Write("data", [1]);
        var stream = new ObservedFileStream(path) { ReportNonSeekable = true };
        await Failure(new(path, _ => stream), CompiledRulesAcquisitionFailure.LocatorInvalid);
        Assert.Equal(0, stream.BytesRead);
        AssertReleased(path);
    }

    [Fact]
    public async Task ConcurrentMutationIdentityIsTheBytesReadNotPreReadMetadata()
    {
        using var files = new Files();
        var original = Enumerable.Repeat((byte)'a', 200).ToArray();
        var changed = Enumerable.Repeat((byte)'b', 200).ToArray();
        var path = files.Write("data", original);
        var stream = new ObservedFileStream(path, allowWrites: true)
        {
            MaximumRead = 32,
            AfterRead = count => { if (count == 32) File.WriteAllBytes(path, changed); }
        };
        var acquired = await new LocalCompiledRulesArtifactProvider(path, _ => stream).AcquireAsync(new(), default);
        byte[] actual = [.. original[..32], .. changed[32..]];
        Assert.Equal(actual, acquired.Bytes.ToArray());
        Assert.Equal(Hash(actual), acquired.ByteSha256);
        Assert.NotEqual(Hash(original), acquired.ByteSha256);
        Assert.NotEqual(Hash(changed), acquired.ByteSha256);
    }

    [Fact]
    public async Task EvidenceAndProviderDisplayNeverDiscloseAbsoluteLocator()
    {
        using var files = new Files();
        var path = files.Write("private-token", Fixture());
        var provider = new LocalCompiledRulesArtifactProvider(path);
        var acquired = await provider.AcquireAsync(new(), default);
        Assert.DoesNotContain(path, provider.ToString());
        Assert.DoesNotContain("private-token", JsonSerializer.Serialize(provider));
        Assert.DoesNotContain(path, JsonSerializer.Serialize(acquired.Evidence));
        Assert.InRange(acquired.Evidence.ProviderKind.Length, 1, 128);
        Assert.Empty(acquired.Evidence.Metadata);
    }

    [Fact]
    public async Task CanonicalCuratedArtifactRetainsItsExactIdentity()
    {
        using var payload = new CanonicalVocabularyPayload();
        using var files = new Files();
        var compiled = RuleCompilationPipeline.Compile(payload.Load());
        var acquired = await new LocalCompiledRulesArtifactProvider(files.Write("compiled-rules.json", compiled.Bytes.ToArray())).AcquireAsync(new(), default);
        var trusted = (await Validate(acquired)).TrustedArtifact!;
        Assert.Equal(10, trusted.Artifact.RuleSources.Count);
        Assert.Equal(154, trusted.Artifact.Snippets.Count);
        Assert.Equal(275622, acquired.Bytes.Length);
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", trusted.SemanticSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", acquired.ByteSha256);
    }

    [SymbolicLinkFact]
    public async Task FinalFileLinkIsRejected()
    {
        using var files = new Files();
        var target = files.Write("target", Fixture());
        var link = Path.Combine(files.Root, "link");
        File.CreateSymbolicLink(link, target);
        try { await Failure(new(link), CompiledRulesAcquisitionFailure.LocatorInvalid); }
        finally { File.Delete(link); }
    }

    [SymbolicLinkFact]
    public async Task ParentDirectoryLinkIsRejected()
    {
        using var files = new Files();
        using var outside = new Files();
        outside.Write("data", Fixture());
        var link = Path.Combine(files.Root, "linked");
        Directory.CreateSymbolicLink(link, outside.Root);
        try { await Failure(new(Path.Combine(link, "data")), CompiledRulesAcquisitionFailure.LocatorInvalid); }
        finally { Directory.Delete(link); }
    }

    [SymbolicLinkFact]
    public async Task PersistentLinkSwapBetweenCheckAndOpenFailsBeforeRead()
    {
        using var files = new Files();
        var path = files.Write("input", Fixture());
        var target = files.Write("other", "other"u8.ToArray());
        ObservedFileStream? opened = null;
        var provider = new LocalCompiledRulesArtifactProvider(path, _ =>
        {
            File.Delete(path);
            File.CreateSymbolicLink(path, target);
            return opened = new(path);
        });
        try
        {
            await Failure(provider, CompiledRulesAcquisitionFailure.LocatorInvalid);
            Assert.NotNull(opened);
            Assert.Equal(0, opened.BytesRead);
        }
        finally { File.Delete(path); }
    }

    [SymbolicLinkFact]
    public async Task PersistentLinkIntroducedDuringReadCannotReturnSuccess()
    {
        using var files = new Files();
        var bytes = Fixture();
        var path = files.Write("input", bytes);
        var target = files.Write("other", [1]);
        var stream = new ObservedFileStream(path, allowWrites: true)
        {
            AfterRead = count =>
            {
                if (count == bytes.Length && (File.GetAttributes(path) & FileAttributes.ReparsePoint) == 0)
                {
                    File.Delete(path);
                    File.CreateSymbolicLink(path, target);
                }
            }
        };
        try
        {
            await Failure(new(path, _ => stream), CompiledRulesAcquisitionFailure.LocatorInvalid);
            Assert.Equal(bytes.Length, stream.BytesRead);
        }
        finally { File.Delete(path); }
    }

    [WindowsFact]
    public async Task ParentJunctionIsRejectedWithoutReadingTarget()
    {
        using var files = new Files();
        using var outside = new Files();
        outside.Write("data", Fixture());
        var link = Path.Combine(files.Root, "junction");
        using var process = Process.Start(new ProcessStartInfo("cmd.exe", $"/d /c mklink /J \"{link}\" \"{outside.Root}\"") { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true })!;
        await process.WaitForExitAsync();
        Assert.Equal(0, process.ExitCode);
        try
        {
            Assert.True((File.GetAttributes(link) & FileAttributes.ReparsePoint) != 0);
            await Failure(new(Path.Combine(link, "data")), CompiledRulesAcquisitionFailure.LocatorInvalid);
        }
        finally { Directory.Delete(link); }
    }

    private static byte[] Fixture() => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "ArtifactFixture", "valid-minimal.json"));
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static Task<CompiledRulesArtifactValidationResult> Validate(AcquiredCompiledRulesArtifact acquired) => CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new Policy());
    private static async Task<CompiledRulesAcquisitionException> Failure(LocalCompiledRulesArtifactProvider provider, CompiledRulesAcquisitionFailure failure, CompiledRulesAcquisitionLimits? limits = null)
    {
        var error = await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(limits ?? new(), default));
        Assert.Equal(failure, error.Failure);
        Assert.InRange(error.Message.Length, 1, 256);
        return error;
    }
    private static void AssertReleased(string path) { using var file = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None); }

    private sealed class Policy(CompiledRulesArtifactTrustReason reason = CompiledRulesArtifactTrustReason.ExplicitApproval) : ICompiledRulesArtifactTrustPolicy
    {
        public int Calls { get; private set; }
        public ValueTask<CompiledRulesArtifactTrustDecision> EvaluateAsync(ValidatedCompiledRulesArtifact artifact, CancellationToken cancellationToken)
        {
            Calls++;
            return ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(reason));
        }
    }

    // Real handles with deterministic interruption/mutation points. Unbuffered
    // reads make the mutation test independent of FileStream prefetch behavior.
    private sealed class ObservedFileStream(string path, bool allowWrites = false) : FileStream(path, FileMode.Open, FileAccess.Read, allowWrites ? FileShare.ReadWrite | FileShare.Delete : FileShare.Read, 1, FileOptions.Asynchronous)
    {
        public int BytesRead { get; private set; }
        public int MaximumRead { get; init; } = int.MaxValue;
        public bool ReportNonSeekable { get; init; }
        public Func<CancellationToken, ValueTask>? BeforeRead { get; init; }
        public Action<int>? AfterRead { get; init; }
        public override bool CanSeek => !ReportNonSeekable && base.CanSeek;
        public override long Length => throw new InvalidOperationException("Pre-read length must not determine acquisition.");
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (BeforeRead is { } before) await before(cancellationToken);
            var count = await base.ReadAsync(buffer[..Math.Min(buffer.Length, MaximumRead)], cancellationToken);
            BytesRead += count;
            AfterRead?.Invoke(BytesRead);
            return count;
        }
    }

    private sealed class Files : IDisposable
    {
        public string Root { get; } = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "EternalCycle-local-artifact-tests", Guid.NewGuid().ToString("N")));
        public Files() => Directory.CreateDirectory(Root);
        public string Write(string name, byte[] bytes) { var path = Path.Combine(Root, name); File.WriteAllBytes(path, bytes); return path; }
        public void Dispose() => Directory.Delete(Root, recursive: true);
    }

    public sealed class WindowsFactAttribute : FactAttribute
    {
        public WindowsFactAttribute() { if (!OperatingSystem.IsWindows()) Skip = "Requires Windows junction support."; }
    }

    public sealed class SymbolicLinkFactAttribute : FactAttribute
    {
        public SymbolicLinkFactAttribute()
        {
            using var files = new Files();
            var target = files.Write("target", []);
            var link = Path.Combine(files.Root, "probe");
            try { File.CreateSymbolicLink(link, target); }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
            { Skip = "Host cannot create symbolic links; indirection case not executed."; }
            finally { if (File.Exists(link)) File.Delete(link); }
        }
    }
}
