using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRulesProviderConformanceTests
{
    [Fact]
    public async Task CustomProviderReturnsStandardResultDirectlyThroughPublicContract()
    {
        var bytes = Fixture();
        ICompiledRulesArtifactProvider provider = Provider(bytes);
        Assert.Equal(new[] { typeof(ICompiledRulesArtifactProvider) }, provider.GetType().GetInterfaces());
        var acquired = await provider.AcquireAsync(new(), default);
        Assert.IsType<AcquiredCompiledRulesArtifact>(acquired);
        Assert.Equal(bytes, acquired.Bytes.ToArray());
        Assert.Equal(Hash(bytes), acquired.ByteSha256);
        Assert.Equal("conformance-memory-store", acquired.Evidence.ProviderKind);
        Assert.Equal("fixture-object-version", acquired.Evidence.ResolvedIdentity!.Scheme);
        Assert.Equal("version-1", acquired.Evidence.ResolvedIdentity.Value);
        Assert.Equal("rules/current", acquired.Evidence.Metadata["logicalObjectKey"]);
        var policy = new Policy();
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy);
        Assert.True(result.IsImportEligible);
        Assert.Equal(1, policy.Calls);
        Assert.Same(acquired.Evidence, result.TrustedArtifact!.AcquisitionEvidence);
        Assert.Equal(bytes, result.TrustedArtifact.CopyBytes());
    }

    [Theory]
    [InlineData("json")]
    [InlineData("utf8")]
    [InlineData("empty")]
    public async Task AcquisitionDoesNotInterpretMalformedArtifactBytes(string kind)
    {
        var bytes = kind switch { "json" => "{broken"u8.ToArray(), "utf8" => new byte[] { 0xC3, 0x28 }, _ => [] };
        var acquired = await Provider(bytes).AcquireAsync(new(), default);
        Assert.Equal(bytes, acquired.Bytes.ToArray());
        var policy = new Policy();
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy);
        Assert.Equal(CompiledRulesAcquisitionFailure.MalformedArtifact, result.Failure);
        Assert.False(result.IsImportEligible);
        Assert.Equal(0, policy.Calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExplicitPolicyControlsTrustNotCustomPlacement(bool approve)
    {
        var acquired = await Provider(Fixture()).AcquireAsync(new(), default);
        var policy = new Policy(_ => approve);
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy);
        Assert.True(result.IsArtifactValid);
        Assert.Equal(approve, result.IsImportEligible);
        Assert.Equal(1, policy.Calls);
        Assert.Equal(approve ? null : CompiledRulesAcquisitionFailure.TrustRejected, result.Failure);
    }

    [Fact]
    public async Task CustomSuccessCannotSupplyImplicitTrust()
    {
        var acquired = await Provider(Fixture()).AcquireAsync(new(), default);
        await Assert.ThrowsAsync<ArgumentNullException>(() => CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), null!));
    }

    [Fact]
    public async Task CanonicalLocalGitHubAndCustomMatrixHasEqualBytesValidityProvenanceAndApproval()
    {
        using var payload = new CanonicalVocabularyPayload();
        var bytes = RuleCompilationPipeline.Compile(payload.Load()).Bytes.ToArray();
        // Only C's comparison input uses a file. The custom provider receives
        // bytes/streams and cannot inspect this path or the canonical fixture.
        var path = Path.Combine(Path.GetTempPath(), "EternalCycle-provider-matrix-" + Guid.NewGuid().ToString("N"));
        File.WriteAllBytes(path, bytes);
        try
        {
            using var http = new ReleaseFixtureTransport(bytes);
            using var github = new GitHubReleaseCompiledRulesArtifactProvider("fixture", "rules", "v-fixture", "rules.json", transport: http);
            ICompiledRulesArtifactProvider[] providers = [new LocalCompiledRulesArtifactProvider(path), github, Provider(bytes)];
            string? semanticModel = null;
            var kinds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var provider in providers)
            {
                var acquired = await provider.AcquireAsync(new(), default);
                Assert.Equal(bytes, acquired.Bytes.ToArray());
                Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", acquired.ByteSha256);
                Assert.True(CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(acquired.Bytes.Span)).IsValid);
                var policy = new Policy();
                var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy);
                Assert.True(result.IsArtifactValid);
                Assert.True(result.IsImportEligible);
                Assert.Equal(1, policy.Calls);
                var trusted = result.TrustedArtifact!;
                Assert.Equal(bytes, trusted.CopyBytes());
                Assert.Equal(275622, trusted.CopyBytes().Length);
                Assert.Equal(10, trusted.Artifact.RuleSources.Count);
                Assert.Equal(154, trusted.Artifact.Snippets.Count);
                Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", trusted.SemanticSha256);
                var model = JsonSerializer.Serialize(trusted.Artifact);
                semanticModel ??= model;
                Assert.Equal(semanticModel, model); // includes every source/content hash, selector, dependency and term
                Assert.True(kinds.Add(acquired.Evidence.ProviderKind));
            }
            Assert.Equal(new[] { "conformance-memory-store", "github-release", "local-file" }, kinds.Order(StringComparer.Ordinal));
            Assert.Equal(4, http.Requests);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task OpaqueEvidenceChangesOnlyConfiguredTrustNotArtifactSemantics()
    {
        var bytes = Fixture();
        var approvedPolicy = new Policy();
        var selectivePolicy = new Policy(artifact => artifact.AcquisitionEvidence.ResolvedIdentity?.Value == "version-1" &&
            artifact.AcquisitionEvidence.Metadata.TryGetValue("storageGeneration", out var generation) && generation == "42");
        var first = await Provider(bytes, metadata: new Dictionary<string, string> { ["storageGeneration"] = "42" }).AcquireAsync(new(), default);
        var second = await Provider(bytes, "version-2", new Dictionary<string, string>
        {
            ["storageGeneration"] = "43", ["rulesetId"] = "not-artifact-identity", ["source"] = "not-artifact-provenance"
        }).AcquireAsync(new(), default);
        var a = await CompiledRulesArtifactValidation.ValidateAsync(first, new(), approvedPolicy);
        var b = await CompiledRulesArtifactValidation.ValidateAsync(second, new(), approvedPolicy);
        Assert.True(a.IsImportEligible);
        Assert.True(b.IsImportEligible);
        Assert.Equal(a.TrustedArtifact!.ByteSha256, b.TrustedArtifact!.ByteSha256);
        Assert.Equal(a.TrustedArtifact.SemanticSha256, b.TrustedArtifact.SemanticSha256);
        Assert.Equal(JsonSerializer.Serialize(a.TrustedArtifact.Artifact), JsonSerializer.Serialize(b.TrustedArtifact.Artifact));
        Assert.True((await CompiledRulesArtifactValidation.ValidateAsync(first, new(), selectivePolicy)).IsImportEligible);
        var rejected = await CompiledRulesArtifactValidation.ValidateAsync(second, new(), selectivePolicy);
        Assert.True(rejected.IsArtifactValid);
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustRejected, rejected.Failure);
    }

    [Theory]
    [InlineData("key")]
    [InlineData("version")]
    [InlineData("control")]
    public void ConfigurationUsesPublicBoundedEvidenceChecks(string kind)
    {
        var error = Assert.Throws<CompiledRulesAcquisitionException>(() => new MemoryStoreCompiledRulesArtifactProvider(
            kind == "key" ? "" : "rules/current", kind == "version" ? "" : kind == "control" ? "private-token\n" : "v1", null));
        AssertFailure(error, CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
    }

    [Theory]
    [InlineData("count")]
    [InlineData("key")]
    [InlineData("value")]
    [InlineData("duplicate")]
    public void CustomMetadataExtensibilityDoesNotWeakenBounds(string kind)
    {
        IEnumerable<KeyValuePair<string, string>> metadata = kind switch
        {
            "count" => Enumerable.Range(0, 16).Select(index => KeyValuePair.Create("key" + index, "value")),
            "key" => [KeyValuePair.Create(new string('k', 129), "value")],
            "value" => [KeyValuePair.Create("key", new string('v', 1025))],
            _ => [KeyValuePair.Create("logicalObjectKey", "duplicate")]
        };
        var error = Assert.Throws<CompiledRulesAcquisitionException>(() => new MemoryStoreCompiledRulesArtifactProvider("rules/current", "v1", null, metadata));
        AssertFailure(error, CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
    }

    [Fact]
    public async Task UnavailableLogicalObjectUsesExistingFailure()
    {
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("missing-object", "v1", null);
        AssertFailure(await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(new(), default)), CompiledRulesAcquisitionFailure.Unavailable);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OpenAndPartialReadFailuresHaveNoRawTransportDiagnostic(bool duringRead)
    {
        var stream = new ProbeStream(Fixture());
        stream.BeforeRead = _ => stream.BytesRead > 0 ? throw new IOException("private-token") : ValueTask.CompletedTask;
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("rules/current", "v1",
            () => duringRead ? stream : throw new IOException("private-token"));
        var error = await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(new(), default));
        AssertFailure(error, CompiledRulesAcquisitionFailure.TransportFailed);
        if (duringRead) { Assert.Equal(7, stream.BytesRead); Assert.True(stream.Disposed); }
    }

    [Theory]
    [InlineData(128, true)]
    [InlineData(129, false)]
    public async Task NonseekableUnknownLengthIsBoundedWithoutTruncation(int length, bool allowed)
    {
        var bytes = new byte[length];
        var stream = new ProbeStream(bytes);
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("rules/current", "v1", () => stream);
        if (allowed)
        {
            var acquired = await provider.AcquireAsync(new(128), default);
            Assert.Equal(bytes, acquired.Bytes.ToArray());
            Assert.Equal(Hash(bytes), acquired.ByteSha256);
        }
        else AssertFailure(await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(new(128), default)), CompiledRulesAcquisitionFailure.PayloadTooLarge);
        Assert.Equal(length, stream.BytesRead);
        Assert.True(stream.Disposed);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1000000000000")]
    public async Task ProviderLengthClaimsCannotOverrideActualStreamBound(string declaredLength)
    {
        var stream = new ProbeStream(new byte[500]);
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("rules/current", "v1", () => stream,
            new Dictionary<string, string> { ["declaredLength"] = declaredLength });
        AssertFailure(await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(new(128), default)), CompiledRulesAcquisitionFailure.PayloadTooLarge);
        Assert.Equal(129, stream.BytesRead);
        Assert.True(stream.Disposed);
    }

    [Fact]
    public async Task PreCancellationDoesNotOpenObject()
    {
        var opens = 0;
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("rules/current", "v1",
            () => { opens++; return new MemoryStream(); });
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => provider.AcquireAsync(new(), cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(0, opens);
    }

    [Fact]
    public async Task CancellationDuringPartialStreamingPreventsSuccessAndDisposesStream()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stream = new ProbeStream(Fixture());
        stream.BeforeRead = async token =>
        {
            if (stream.BytesRead == 0) return;
            started.TrySetResult();
            await Task.Delay(Timeout.Infinite, token);
        };
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("rules/current", "v1", () => stream);
        using var cancellation = new CancellationTokenSource();
        var pending = provider.AcquireAsync(new(), cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.Equal(7, stream.BytesRead);
        Assert.True(stream.Disposed);
        Assert.Null(error.InnerException);
    }

    [Fact]
    public async Task EndOfStreamCancellationCannotBecomeSuccess()
    {
        using var cancellation = new CancellationTokenSource();
        var stream = new ProbeStream([1, 2, 3]) { AfterRead = count => { if (count == 0) cancellation.Cancel(); } };
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("rules/current", "v1", () => stream);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => provider.AcquireAsync(new(), cancellation.Token));
        Assert.True(stream.Disposed);
    }

    [Fact]
    public async Task UnreadableStreamIsConfigurationFailureAndStillDisposed()
    {
        var stream = new ProbeStream([]) { Readable = false };
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("rules/current", "v1", () => stream);
        AssertFailure(await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(new(), default)), CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
        Assert.Equal(0, stream.BytesRead);
        Assert.True(stream.Disposed);
    }

    [Fact]
    public async Task ProviderSourceBufferMutationCannotAlterAcquiredOrApprovedBytes()
    {
        var source = Fixture();
        var expected = source.ToArray();
        var acquired = await Provider(source).AcquireAsync(new(), default);
        Array.Fill(source, (byte)0);
        Assert.Equal(expected, acquired.Bytes.ToArray());
        var approved = (await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new Policy())).TrustedArtifact!;
        Assert.Equal(expected, approved.CopyBytes());
        approved.CopyBytes()[0] ^= 1;
        Assert.Equal(expected, approved.CopyBytes());
    }

    [Fact]
    public async Task DeliberateAcquisitionAliasTamperingBeforeValidationIsRejected()
    {
        var acquired = await Provider(Fixture()).AcquireAsync(new(), default);
        Assert.True(MemoryMarshal.TryGetArray(acquired.Bytes, out var buffer));
        buffer.Array![buffer.Offset] ^= 1;
        var policy = new Policy();
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy);
        Assert.Equal(CompiledRulesAcquisitionFailure.IntegrityMismatch, result.Failure);
        Assert.Equal(0, policy.Calls);
        Assert.Null(result.TrustedArtifact);
    }

    [Fact]
    public async Task ApprovalDoesNotExposeItsByteBufferThroughAcquisitionAliases()
    {
        var expected = Fixture();
        var acquired = await Provider(expected).AcquireAsync(new(), default);
        var approved = (await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new Policy())).TrustedArtifact!;
        Assert.True(MemoryMarshal.TryGetArray(acquired.Bytes, out var buffer));
        buffer.Array![buffer.Offset] ^= 1;
        Assert.Equal(expected, approved.CopyBytes());
        Assert.Equal(approved.SemanticSha256, CompiledRulesArtifactContract.ComputeArtifactSha256(approved.Artifact));
    }

    [Fact]
    public async Task EvidenceInputAndPublicCollectionsCannotMutateApprovedObservation()
    {
        var metadata = new Dictionary<string, string> { ["storageGeneration"] = "42" };
        var acquired = await Provider(Fixture(), metadata: metadata).AcquireAsync(new(), default);
        metadata["storageGeneration"] = "43";
        var policy = new Policy(artifact =>
        {
            metadata.Clear();
            Assert.Equal("42", artifact.AcquisitionEvidence.Metadata["storageGeneration"]);
            Assert.Throws<NotSupportedException>(() => ((IDictionary<string, string>)artifact.AcquisitionEvidence.Metadata).Clear());
            return true;
        });
        var approved = (await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy)).TrustedArtifact!;
        Assert.Same(acquired.Evidence, approved.AcquisitionEvidence);
        Assert.Equal("42", approved.AcquisitionEvidence.Metadata["storageGeneration"]);
        Assert.Equal("version-1", approved.AcquisitionEvidence.ResolvedIdentity!.Value);
        Assert.Equal(Fixture(), approved.CopyBytes());
    }

    [Fact]
    public async Task SharedValidationEnforcesItsOwnSmallerLimitBeforePolicy()
    {
        var bytes = Fixture();
        var acquired = await Provider(bytes).AcquireAsync(new(), default);
        var policy = new Policy();
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(bytes.Length - 1), policy);
        Assert.Equal(CompiledRulesAcquisitionFailure.PayloadTooLarge, result.Failure);
        Assert.Equal(0, policy.Calls);
    }

    [Fact]
    public async Task RepeatCultureAndWorkingDirectoryDoNotInterpretLogicalKeysAsPaths()
    {
        var bytes = Fixture();
        ICompiledRulesArtifactProvider provider = Provider(bytes);
        var first = await provider.AcquireAsync(new(), default);
        var culture = CultureInfo.CurrentCulture;
        var directory = Environment.CurrentDirectory;
        try
        {
            CultureInfo.CurrentCulture = new("tr-TR");
            Environment.CurrentDirectory = Path.GetTempPath();
            var second = await provider.AcquireAsync(new(), default);
            Assert.Equal(first.Bytes.ToArray(), second.Bytes.ToArray());
            Assert.Equal(first.ByteSha256, second.ByteSha256);
            Assert.Equal(JsonSerializer.Serialize(first.Evidence), JsonSerializer.Serialize(second.Evidence));
        }
        finally { CultureInfo.CurrentCulture = culture; Environment.CurrentDirectory = directory; }
    }

    [Fact]
    public void ProductionAssemblyCannotDependOnTestOnlyProviderOrManagedInfrastructure()
    {
        var assembly = typeof(ICompiledRulesArtifactProvider).Assembly;
        Assert.NotEqual(assembly, typeof(MemoryStoreCompiledRulesArtifactProvider).Assembly);
        Assert.DoesNotContain(assembly.GetTypes(), type => type.Name == nameof(MemoryStoreCompiledRulesArtifactProvider));
        Assert.All(assembly.GetReferencedAssemblies(), reference => Assert.DoesNotContain(reference.Name!, new[]
        {
            "EternalCycle.Rules.Tests", "EternalCycle.Rules.Compiler.Tests", "EternalCycle.Persistence.Mcp", "Microsoft.Data.SqlClient",
            "ModelContextProtocol", "Microsoft.Extensions.Hosting"
        }));
    }

    private static MemoryStoreCompiledRulesArtifactProvider Provider(byte[] bytes, string version = "version-1", Dictionary<string, string>? metadata = null) =>
        new("rules/current", version, () => new MemoryStream(bytes, writable: false), metadata);
    private static byte[] Fixture() => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "ArtifactFixture", "valid-minimal.json"));
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static void AssertFailure(CompiledRulesAcquisitionException error, CompiledRulesAcquisitionFailure failure)
    {
        Assert.Equal(failure, error.Failure);
        Assert.Equal(new CompiledRulesAcquisitionException(failure).Code, error.Code);
        Assert.InRange(error.Message.Length, 1, 256);
        Assert.DoesNotContain("private-token", error.ToString());
        Assert.Null(error.InnerException);
    }

    private sealed class Policy(Func<ValidatedCompiledRulesArtifact, bool>? approve = null) : ICompiledRulesArtifactTrustPolicy
    {
        public int Calls { get; private set; }
        public ValueTask<CompiledRulesArtifactTrustDecision> EvaluateAsync(ValidatedCompiledRulesArtifact artifact, CancellationToken cancellationToken)
        {
            Calls++;
            return ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(approve?.Invoke(artifact) != false
                ? CompiledRulesArtifactTrustReason.ExplicitApproval : CompiledRulesArtifactTrustReason.ExplicitRejection));
        }
    }

    // Small public-HTTP substitute for the C/D/E matrix, not a third transport.
    // D's full adversarial HTTP matrix remains in its dedicated test class.
    private sealed class ReleaseFixtureTransport(byte[] bytes) : HttpMessageHandler
    {
        public int Requests { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests++;
            var path = request.RequestUri!.AbsolutePath;
            HttpContent content = path switch
            {
                "/repos/fixture/rules" => new StringContent("{\"id\":10,\"full_name\":\"fixture/rules\"}", Encoding.UTF8, "application/json"),
                "/repos/fixture/rules/releases/tags/v-fixture" => new StringContent("{\"id\":20,\"tag_name\":\"v-fixture\",\"draft\":false,\"prerelease\":false,\"published_at\":\"2026-01-01T00:00:00Z\"}", Encoding.UTF8, "application/json"),
                "/repos/fixture/rules/releases/20/assets" => new StringContent("[{\"id\":30,\"name\":\"rules.json\",\"size\":275622,\"state\":\"uploaded\",\"url\":\"https://api.github.com/repos/fixture/rules/releases/assets/30\"}]", Encoding.UTF8, "application/json"),
                "/repos/fixture/rules/releases/assets/30" => new ByteArrayContent(bytes),
                _ => throw new InvalidOperationException("Unexpected fixture request.")
            };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
        }
    }

    // Fragmented, unknown-length stream makes accidental length/seek assumptions
    // fail. Hooks synchronize partial-read failures/cancellation without sleeps.
    private sealed class ProbeStream(byte[] bytes) : MemoryStream(bytes)
    {
        public int BytesRead { get; private set; }
        public bool Disposed { get; private set; }
        public bool Readable { get; init; } = true;
        public Func<CancellationToken, ValueTask>? BeforeRead { get; set; }
        public Action<int>? AfterRead { get; init; }
        public override bool CanRead => Readable && !Disposed;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (BeforeRead is { } before) await before(cancellationToken);
            var count = await base.ReadAsync(buffer[..Math.Min(buffer.Length, 7)], cancellationToken);
            BytesRead += count;
            AfterRead?.Invoke(count);
            return count;
        }
        protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
    }
}
