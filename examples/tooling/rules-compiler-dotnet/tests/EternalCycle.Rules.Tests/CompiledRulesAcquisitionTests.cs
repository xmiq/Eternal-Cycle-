using System.Security.Cryptography;
using System.Text;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRulesAcquisitionTests
{
    private static readonly CompiledRulesAcquisitionEvidence Evidence = new("test-memory");

    [Fact]
    public void AcquisitionPreservesAndOwnsExactBytesWithoutClaimingArtifactValidity()
    {
        byte[] input = [0, 255, 13, 10, 32];
        var expected = input.ToArray();
        var acquired = new AcquiredCompiledRulesArtifact(input, Evidence, new());
        input[0] = 1;
        Assert.Equal(expected, acquired.Bytes.ToArray());
        Assert.Equal(Convert.ToHexString(SHA256.HashData(expected)), acquired.ByteSha256);
        Assert.Same(Evidence, acquired.Evidence);
        Assert.Equal(nameof(AcquiredCompiledRulesArtifact), acquired.ToString());
        // Arbitrary bytes are deliberately accepted: A is not a second parser.
        Assert.False(CompiledRulesArtifactContract.Read("{}").IsValid);
    }

    [Fact]
    public void ByteIdentityIsDeterministicAndSensitiveToExactBytes()
    {
        var first = new AcquiredCompiledRulesArtifact("text\n"u8, Evidence, new());
        var repeat = new AcquiredCompiledRulesArtifact("text\n"u8, Evidence, new());
        var changed = new AcquiredCompiledRulesArtifact("text\r\n"u8, Evidence, new());
        Assert.Equal(first.ByteSha256, repeat.ByteSha256);
        Assert.NotEqual(first.ByteSha256, changed.ByteSha256);
        Assert.Equal(64, first.ByteSha256.Length);
    }

    [Fact]
    public void EmptyAcquisitionStillHasByteIdentityButNoSemanticApproval()
    {
        var acquired = new AcquiredCompiledRulesArtifact([], Evidence, new());
        Assert.Empty(acquired.Bytes.ToArray());
        Assert.Equal(Convert.ToHexString(SHA256.HashData(Array.Empty<byte>())), acquired.ByteSha256);
    }

    [Fact]
    public async Task DifferentConfiguredProvidersUseTheSameByteContract()
    {
        ICompiledRulesArtifactProvider first = new MemoryProvider("local-test", "not artifact JSON"u8.ToArray());
        ICompiledRulesArtifactProvider second = new MemoryProvider("custom-test", "not artifact JSON"u8.ToArray());
        var a = await first.AcquireAsync(new(), CancellationToken.None);
        var b = await second.AcquireAsync(new(), CancellationToken.None);
        Assert.Equal(a.Bytes.ToArray(), b.Bytes.ToArray());
        Assert.Equal(a.ByteSha256, b.ByteSha256);
        Assert.NotEqual(a.Evidence.ProviderKind, b.Evidence.ProviderKind);
        Assert.Null(a.Evidence.ResolvedIdentity);
    }

    [Fact]
    public async Task NonSeekableFragmentedStreamAtLimitIsReadExactlyAndLeftOpen()
    {
        using var stream = new ProbeStream([0, 1, 2, 3, 4]);
        var acquired = await AcquiredCompiledRulesArtifact.ReadAsync(stream, Evidence, new(5), CancellationToken.None);
        Assert.Equal(new byte[] { 0, 1, 2, 3, 4 }, acquired.Bytes.ToArray());
        Assert.Equal(5, stream.BytesRead);
        Assert.True(stream.CanRead);
        Assert.False(stream.Disposed);
    }

    [Fact]
    public async Task UnknownLengthOversizeStopsAtLimitPlusOneWithoutPublishingPayload()
    {
        using var stream = new ProbeStream(new byte[100]);
        var failure = await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() =>
            AcquiredCompiledRulesArtifact.ReadAsync(stream, Evidence, new(5), CancellationToken.None));
        Assert.Equal("ARTIFACT_PAYLOAD_TOO_LARGE", failure.Code);
        Assert.Equal(6, stream.BytesRead);
        Assert.False(stream.Disposed);
    }

    [Fact]
    public void AlreadyBufferedOversizeIsRejectedBeforeOwnershipCopy()
    {
        var failure = Assert.Throws<CompiledRulesAcquisitionException>(() =>
            new AcquiredCompiledRulesArtifact(new byte[6], Evidence, new(5)));
        Assert.Equal(CompiledRulesAcquisitionFailure.PayloadTooLarge, failure.Failure);
    }

    [Fact]
    public async Task PreCancelledAcquisitionDoesNotRead()
    {
        using var stream = new ProbeStream([1]);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            AcquiredCompiledRulesArtifact.ReadAsync(stream, Evidence, new(), cancellation.Token));
        Assert.Equal(0, stream.ReadCalls);
    }

    [Fact]
    public async Task CancellationInterruptsBlockedReadWithoutBecomingTransportFailure()
    {
        using var stream = new ProbeStream([], block: true);
        using var cancellation = new CancellationTokenSource();
        var pending = AcquiredCompiledRulesArtifact.ReadAsync(stream, Evidence, new(), cancellation.Token);
        await stream.ReadStarted.Task;
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.False(stream.Disposed);
    }

    [Fact]
    public async Task StreamFailureDoesNotEchoRawTransportSecrets()
    {
        using var stream = new ProbeStream([], throwOnRead: true);
        var failure = await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() =>
            AcquiredCompiledRulesArtifact.ReadAsync(stream, Evidence, new(), CancellationToken.None));
        Assert.Equal("ARTIFACT_TRANSPORT_FAILED", failure.Code);
        Assert.Null(failure.InnerException);
        Assert.DoesNotContain("private-token", failure.ToString());
        Assert.DoesNotContain("password", failure.Message);
    }

    [Fact]
    public async Task UnreadableStreamFailsWithStructuredConfigurationError()
    {
        using var stream = new ProbeStream([], readable: false);
        var failure = await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() =>
            AcquiredCompiledRulesArtifact.ReadAsync(stream, Evidence, new(), CancellationToken.None));
        Assert.Equal("ARTIFACT_PROVIDER_CONFIGURATION_INVALID", failure.Code);
        Assert.Equal(0, stream.ReadCalls);
    }

    [Fact]
    public void EvidenceIsSeparateBoundedCopiedAndOrdinallyOrdered()
    {
        var metadata = new Dictionary<string, string> { ["release-id"] = "12", ["asset-id"] = "34" };
        var identity = new CompiledRulesAcquisitionIdentity("custom-object-digest", "opaque-immutable-value");
        var evidence = new CompiledRulesAcquisitionEvidence("third-party", identity, metadata);
        metadata["release-id"] = "changed";
        Assert.Equal(new[] { "asset-id", "release-id" }, evidence.Metadata.Keys);
        Assert.Equal("12", evidence.Metadata["release-id"]);
        Assert.Same(identity, evidence.ResolvedIdentity);
        Assert.Equal("custom-object-digest", identity.Scheme);
        Assert.Equal("opaque-immutable-value", identity.Value);
        Assert.Equal(nameof(CompiledRulesAcquisitionEvidence), evidence.ToString());
        Assert.Equal(nameof(CompiledRulesAcquisitionIdentity), identity.ToString());
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, string>)evidence.Metadata).Add("extra", "value"));
    }

    [Theory]
    [InlineData("count")]
    [InlineData("key")]
    [InlineData("value")]
    [InlineData("control")]
    [InlineData("duplicate")]
    public void InvalidMetadataFailsWithoutEchoingItsValues(string mutation)
    {
        IEnumerable<KeyValuePair<string, string>> metadata = mutation switch
        {
            "count" => Enumerable.Range(0, 17).Select(i => KeyValuePair.Create($"key-{i}", "private-token")),
            "key" => [KeyValuePair.Create(new string('x', 129), "private-token")],
            "value" => [KeyValuePair.Create("key", new string('x', 1025))],
            "control" => [KeyValuePair.Create("key", "private-token\n")],
            _ => [KeyValuePair.Create("key", "first"), KeyValuePair.Create("key", "private-token")]
        };
        var failure = Assert.Throws<CompiledRulesAcquisitionException>(() =>
            new CompiledRulesAcquisitionEvidence("custom", metadata: metadata));
        Assert.Equal("ARTIFACT_PROVIDER_CONFIGURATION_INVALID", failure.Code);
        Assert.DoesNotContain("private-token", failure.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PayloadLimitMustBePositive(int limit) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new CompiledRulesAcquisitionLimits(limit));

    [Theory]
    [InlineData(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid, "ARTIFACT_PROVIDER_CONFIGURATION_INVALID")]
    [InlineData(CompiledRulesAcquisitionFailure.LocatorInvalid, "ARTIFACT_LOCATOR_INVALID")]
    [InlineData(CompiledRulesAcquisitionFailure.Unavailable, "ARTIFACT_UNAVAILABLE")]
    [InlineData(CompiledRulesAcquisitionFailure.TransportFailed, "ARTIFACT_TRANSPORT_FAILED")]
    [InlineData(CompiledRulesAcquisitionFailure.PayloadTooLarge, "ARTIFACT_PAYLOAD_TOO_LARGE")]
    [InlineData(CompiledRulesAcquisitionFailure.IntegrityMismatch, "ARTIFACT_INTEGRITY_MISMATCH")]
    [InlineData(CompiledRulesAcquisitionFailure.MalformedArtifact, "ARTIFACT_MALFORMED")]
    [InlineData(CompiledRulesAcquisitionFailure.UnsupportedArtifactFormat, "ARTIFACT_FORMAT_UNSUPPORTED")]
    [InlineData(CompiledRulesAcquisitionFailure.SemanticValidationFailed, "ARTIFACT_SEMANTIC_INVALID")]
    [InlineData(CompiledRulesAcquisitionFailure.TrustRejected, "ARTIFACT_TRUST_REJECTED")]
    [InlineData(CompiledRulesAcquisitionFailure.ImportConflict, "ARTIFACT_IMPORT_CONFLICT")]
    [InlineData(CompiledRulesAcquisitionFailure.StorageFailed, "ARTIFACT_STORAGE_FAILED")]
    public void FailuresHaveStableBoundedCodes(CompiledRulesAcquisitionFailure failure, string code)
    {
        var exception = new CompiledRulesAcquisitionException(failure);
        Assert.Equal(failure, exception.Failure);
        Assert.Equal(code, exception.Code);
        Assert.InRange(exception.Message.Length, 1, 256);
        Assert.Null(exception.InnerException);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid\n")]
    public void InvalidProviderIdentityFailsStructurally(string kind) =>
        Assert.Throws<CompiledRulesAcquisitionException>(() => new CompiledRulesAcquisitionEvidence(kind));

    [Fact]
    public void CurrentCanonicalArtifactFitsDefaultBoundAndRetainsItsIdentity()
    {
        using var payload = new CanonicalVocabularyPayload();
        var compiled = RuleCompilationPipeline.Compile(payload.Load());
        var acquired = new AcquiredCompiledRulesArtifact(compiled.Bytes.Span, Evidence, new());
        Assert.Equal(compiled.Bytes.ToArray(), acquired.Bytes.ToArray());
        Assert.Equal(275622, acquired.Bytes.Length);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", acquired.ByteSha256);
        var read = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(acquired.Bytes.Span));
        Assert.True(read.IsValid);
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", read.Artifact!.Integrity.ArtifactSha256);
        Assert.True(new CompiledRulesAcquisitionLimits().MaximumPayloadBytes > acquired.Bytes.Length * 10);
    }

    private sealed class MemoryProvider(string kind, byte[] bytes) : ICompiledRulesArtifactProvider
    {
        public async Task<AcquiredCompiledRulesArtifact> AcquireAsync(CompiledRulesAcquisitionLimits limits, CancellationToken cancellationToken)
        {
            using var stream = new MemoryStream(bytes, writable: false);
            return await AcquiredCompiledRulesArtifact.ReadAsync(stream, new(kind), limits, cancellationToken);
        }
    }

    private sealed class ProbeStream(byte[] bytes, bool block = false, bool throwOnRead = false, bool readable = true) : Stream
    {
        public int BytesRead { get; private set; }
        public int ReadCalls { get; private set; }
        public bool Disposed { get; private set; }
        public TaskCompletionSource ReadStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override bool CanRead => readable && !Disposed;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ReadCalls++;
            ReadStarted.TrySetResult();
            cancellationToken.ThrowIfCancellationRequested();
            if (throwOnRead) { throw new IOException("password=private-token; private transport locator"); }
            if (block) { await Task.Delay(Timeout.Infinite, cancellationToken); }
            var count = Math.Min(2, Math.Min(buffer.Length, bytes.Length - BytesRead));
            bytes.AsMemory(BytesRead, count).CopyTo(buffer);
            BytesRead += count;
            return count;
        }
        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
