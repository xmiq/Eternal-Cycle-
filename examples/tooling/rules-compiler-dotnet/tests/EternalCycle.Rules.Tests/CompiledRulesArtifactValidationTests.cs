using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRulesArtifactValidationTests
{
    [Theory]
    [InlineData("valid-minimal.json")]
    [InlineData("valid-representative.json")]
    public async Task ValidFixtureNeedsExplicitApprovalAndPreservesCompleteFormatOne(string fixture)
    {
        var bytes = Fixture(fixture);
        var policy = new TestPolicy();
        var result = await Validate(bytes, policy);
        Assert.True(result.IsArtifactValid);
        Assert.True(result.IsImportEligible);
        Assert.Null(result.Failure);
        Assert.Equal(1, policy.Calls);
        Assert.Empty(result.Diagnostics);
        Assert.Equal(bytes, result.TrustedArtifact!.CopyBytes());
        Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)), result.TrustedArtifact.ByteSha256);
        Assert.NotEqual(result.TrustedArtifact.ByteSha256, result.TrustedArtifact.SemanticSha256);
        var written = CompiledRulesArtifactWriter.Write(result.TrustedArtifact.Artifact);
        Assert.True(written.IsValid);
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(bytes), JsonNode.Parse(written.Bytes.Span)));
        Assert.Empty(CompiledRulesArtifactContract.Validate(result.TrustedArtifact.Artifact));
        Assert.Same(policy.Seen, result.TrustedArtifact.Validated);
    }

    [Fact]
    public async Task ValidDoesNotMeanTrustedAndTrustFailureIsNotRejection()
    {
        var rejected = await Validate(Fixture(), new TestPolicy((_, _) => ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason.ExplicitRejection))));
        Assert.True(rejected.IsArtifactValid);
        Assert.False(rejected.IsImportEligible);
        Assert.Null(rejected.TrustedArtifact);
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustRejected, rejected.Failure);
        Assert.Equal("ARTIFACT_TRUST_REJECTED", rejected.Code);
        var failed = await Validate(Fixture(), new TestPolicy((_, _) => ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason.PolicyUnavailable))));
        Assert.True(failed.IsArtifactValid);
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustPolicyFailed, failed.Failure);
        Assert.Equal("ARTIFACT_TRUST_POLICY_FAILED", failed.Code);
        Assert.Null(failed.TrustedArtifact);
    }

    [Fact]
    public async Task NoImplicitDefaultTrustPolicyExists()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => CompiledRulesArtifactValidation.ValidateAsync(Acquire(Fixture()), new(), null!));
    }

    [Theory]
    [InlineData("empty")]
    [InlineData("whitespace")]
    [InlineData("syntax")]
    [InlineData("utf8")]
    [InlineData("unknown")]
    [InlineData("duplicate")]
    public async Task MalformedInputFailsBeforePolicy(string kind)
    {
        var bytes = kind switch
        {
            "empty" => Array.Empty<byte>(),
            "whitespace" => " \r\n"u8.ToArray(),
            "syntax" => "{broken"u8.ToArray(),
            "utf8" => new byte[] { 0xC3, 0x28 },
            "unknown" => Mutate(node => node["unknown"] = "private-token"),
            _ => Encoding.UTF8.GetBytes("{\"artifactFormatVersion\":1," + Encoding.UTF8.GetString(Fixture())[1..])
        };
        var policy = new TestPolicy();
        var result = await Validate(bytes, policy);
        AssertFailure(result, CompiledRulesAcquisitionFailure.MalformedArtifact, policy);
        Assert.NotEmpty(result.Diagnostics);
        Assert.DoesNotContain("private-token", JsonSerializer.Serialize(result));
    }

    [Theory]
    [InlineData("format")]
    [InlineData("compiler")]
    public async Task UnsupportedContractsNeverReachPolicy(string kind)
    {
        var bytes = Mutate(node =>
        {
            if (kind == "format") node["artifactFormatVersion"] = 99;
            else node["compiler"]!["contractVersion"] = "99";
        });
        var policy = new TestPolicy();
        var result = await Validate(bytes, policy);
        AssertFailure(result, CompiledRulesAcquisitionFailure.UnsupportedArtifactFormat, policy);
        Assert.Contains(result.Diagnostics, error => error.Code == (kind == "format" ? "FORMAT_VERSION_UNSUPPORTED" : "COMPILER_CONTRACT_UNSUPPORTED"));
    }

    [Theory]
    [InlineData("digest", "ARTIFACT_HASH_MISMATCH")]
    [InlineData("content", "CONTENT_HASH_MISMATCH")]
    [InlineData("hash-shape", "SHA256_INVALID")]
    [InlineData("algorithm", "INTEGRITY_ALGORITHM_UNSUPPORTED")]
    public async Task IntegrityFailurePreservesFr022CodesAndNeverRunsPolicy(string mutation, string code)
    {
        var bytes = Mutate(node =>
        {
            if (mutation == "content") node["snippets"]![0]!["content"] = "Changed rule data";
            else if (mutation == "algorithm") node["integrity"]!["algorithm"] = "SHA-1";
            else node["integrity"]!["artifactSha256"] = mutation == "digest" ? new string('0', 64) : "invalid";
        });
        var policy = new TestPolicy();
        var result = await Validate(bytes, policy);
        AssertFailure(result, CompiledRulesAcquisitionFailure.IntegrityMismatch, policy);
        Assert.Contains(result.Diagnostics, error => error.Code == code);
        Assert.Contains(result.Diagnostics, error => error.Path == (mutation == "content"
            ? "$.snippets[0].contentSha256"
            : mutation == "algorithm" ? "$.integrity.algorithm" : "$.integrity.artifactSha256"));
    }

    [Theory]
    [InlineData("missing", "DEPENDENCY_TARGET_MISSING")]
    [InlineData("self", "DEPENDENCY_SELF_REFERENCE")]
    [InlineData("cycle", "DEPENDENCY_CYCLE")]
    public async Task InvalidDependencyGraphNeverReachesTrust(string kind, string code)
    {
        var node = JsonNode.Parse(Fixture("valid-representative.json"))!;
        if (kind == "missing") node["ruleSources"]![0]!["dependencyRuleSourceIds"] = new JsonArray("absent");
        else if (kind == "self") node["ruleSources"]![0]!["dependencyRuleSourceIds"] = new JsonArray("core-reincarnation");
        else node["ruleSources"]![1]!["dependencyRuleSourceIds"] = new JsonArray("core-reincarnation");
        var policy = new TestPolicy();
        var result = await Validate(Encoding.UTF8.GetBytes(node.ToJsonString()), policy);
        AssertFailure(result, CompiledRulesAcquisitionFailure.SemanticValidationFailed, policy);
        Assert.Contains(result.Diagnostics, error => error.Code == code);
    }

    [Theory]
    [InlineData("source", "STRUCTURE_REQUIRED")]
    [InlineData("snippet", "STRUCTURE_REQUIRED")]
    [InlineData("term", "STRUCTURE_REQUIRED")]
    [InlineData("relationship", "STRUCTURE_REQUIRED")]
    [InlineData("source-path", "PATH_INVALID")]
    [InlineData("manifest-path", "PATH_INVALID")]
    [InlineData("source-id", "IDENTIFIER_INVALID")]
    [InlineData("duplicate-source", "DUPLICATE_RULE_SOURCE_ID")]
    public async Task MalformedDtoShapeIsRejectedByFr022WithoutThrowing(string kind, string code)
    {
        var node = JsonNode.Parse(Fixture("valid-representative.json"))!;
        switch (kind)
        {
            case "source": node["ruleSources"]![0] = null; break;
            case "snippet": node["snippets"]![0] = null; break;
            case "term": node["snippets"]![0]!["retrieval"]!["terms"]![0] = null; break;
            case "relationship": node["snippets"]![0]!["retrieval"]!["relationships"]![0] = null; break;
            case "source-path": node["ruleSources"]![0]!["sourcePath"] = null; break;
            case "manifest-path": node["ruleset"]!["manifestPath"] = null; break;
            case "source-id": node["ruleSources"]![0]!["ruleSourceId"] = null; break;
            default: node["ruleSources"]!.AsArray().Add(node["ruleSources"]![0]!.DeepClone()); break;
        }
        var json = node.ToJsonString();
        var read = CompiledRulesArtifactContract.Read(json);
        Assert.False(read.IsValid);
        Assert.Contains(read.Errors, error => error.Code == code);
        var policy = new TestPolicy();
        var result = await Validate(Encoding.UTF8.GetBytes(json), policy);
        AssertFailure(result, CompiledRulesAcquisitionFailure.SemanticValidationFailed, policy);
        Assert.Contains(result.Diagnostics, error => error.Code == code);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DeepFlatDependencyGraphUsesBoundedIterativeTraversal(bool cycle)
    {
        var node = JsonNode.Parse(Fixture())!;
        var template = node["ruleSources"]![0]!.DeepClone();
        var sources = new JsonArray();
        for (var index = 0; index < 5000; index++)
        {
            var source = template.DeepClone();
            source["ruleSourceId"] = "D" + index.ToString("D4", CultureInfo.InvariantCulture);
            source["dependencyRuleSourceIds"] = index < 4999
                ? new JsonArray("D" + (index + 1).ToString("D4", CultureInfo.InvariantCulture))
                : cycle ? new JsonArray("D0000") : new JsonArray();
            sources.Add(source);
        }
        node["ruleSources"] = sources;
        node["snippets"]![0]!["ruleSourceId"] = "D0000";
        node["snippets"]![0]!["snippetId"] = "D0000";
        if (!cycle) Rehash(node);
        var policy = new TestPolicy();
        var result = await Validate(Encoding.UTF8.GetBytes(node.ToJsonString()), policy);
        if (cycle)
        {
            AssertFailure(result, CompiledRulesAcquisitionFailure.SemanticValidationFailed, policy);
            Assert.Contains(result.Diagnostics, error => error.Code == "DEPENDENCY_CYCLE");
        }
        else Assert.True(result.IsImportEligible);
    }

    [Fact]
    public async Task SharedLimitIsEnforcedIndependentlyOfAcquisitionLimit()
    {
        var bytes = Fixture();
        var policy = new TestPolicy();
        var result = await CompiledRulesArtifactValidation.ValidateAsync(Acquire(bytes), new(bytes.Length - 1), policy);
        AssertFailure(result, CompiledRulesAcquisitionFailure.PayloadTooLarge, policy);
    }

    [Fact]
    public async Task AcquiredBackingArrayTamperingFailsBeforeParsingOrPolicy()
    {
        var acquired = Acquire(Fixture());
        Assert.True(MemoryMarshal.TryGetArray(acquired.Bytes, out var backing));
        backing.Array![backing.Offset] ^= 1;
        var policy = new TestPolicy();
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy);
        AssertFailure(result, CompiledRulesAcquisitionFailure.IntegrityMismatch, policy);
    }

    [Fact]
    public async Task TrustExceptionCannotExposeSecretsOrGrantApproval()
    {
        var policy = new TestPolicy((_, _) => throw new InvalidOperationException("Authorization: private-token; password=secret; C:\\private\\file"));
        var result = await Validate(Fixture(), policy);
        Assert.True(result.IsArtifactValid);
        Assert.False(result.IsImportEligible);
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustPolicyFailed, result.Failure);
        Assert.Equal(CompiledRulesArtifactTrustReason.PolicyException, result.TrustDecision!.Reason);
        Assert.DoesNotContain("private-token", JsonSerializer.Serialize(result));
        Assert.DoesNotContain("secret", result.Message!);
        Assert.DoesNotContain("private", result.ToString());
    }

    [Fact]
    public async Task NullDecisionIsPolicyFailureNotDefaultApproval()
    {
        var result = await Validate(Fixture(), new TestPolicy((_, _) => ValueTask.FromResult<CompiledRulesArtifactTrustDecision>(null!)));
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustPolicyFailed, result.Failure);
        Assert.Equal(CompiledRulesArtifactTrustReason.InvalidPolicyDecision, result.TrustDecision!.Reason);
        Assert.False(result.IsImportEligible);
    }

    [Fact]
    public async Task AsynchronousPolicyExceptionIsContainedWithoutRawText()
    {
        var policy = new TestPolicy(async (_, _) =>
        {
            await Task.Yield();
            throw new InvalidOperationException("Authorization: private-token");
        });
        var result = await Validate(Fixture(), policy);
        Assert.True(result.IsArtifactValid);
        Assert.False(result.IsImportEligible);
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustPolicyFailed, result.Failure);
        Assert.Equal(CompiledRulesArtifactTrustReason.PolicyException, result.TrustDecision!.Reason);
        Assert.DoesNotContain("private-token", JsonSerializer.Serialize(result));
    }

    [Fact]
    public async Task InvalidPolicyReasonCannotProduceApproval()
    {
        var result = await Validate(Fixture(), new TestPolicy((_, _) =>
            ValueTask.FromResult(new CompiledRulesArtifactTrustDecision((CompiledRulesArtifactTrustReason)999))));
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustPolicyFailed, result.Failure);
        Assert.Equal(CompiledRulesArtifactTrustReason.PolicyException, result.TrustDecision!.Reason);
        Assert.False(result.IsImportEligible);
    }

    [Fact]
    public async Task PolicyCancellationPreservesTokenWithoutLeakingItsExceptionText()
    {
        using var ownCancellation = new CancellationTokenSource();
        ownCancellation.Cancel();
        var policy = new TestPolicy((_, _) => throw new OperationCanceledException("private-token", ownCancellation.Token));
        var cancelled = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Validate(Fixture(), policy));
        Assert.Equal(ownCancellation.Token, cancelled.CancellationToken);
        Assert.DoesNotContain("private-token", cancelled.Message);
        Assert.Null(cancelled.InnerException);
    }

    [Fact]
    public async Task CallerCancellationStopsWaitingForUncooperativePolicy()
    {
        var completion = new TaskCompletionSource<CompiledRulesArtifactTrustDecision>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var policy = new TestPolicy((_, _) =>
        {
            started.SetResult();
            return new(completion.Task);
        });
        using var cancellation = new CancellationTokenSource();
        var pending = CompiledRulesArtifactValidation.ValidateAsync(Acquire(Fixture()), new(), policy, cancellation.Token);
        await started.Task;
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        completion.SetResult(new(CompiledRulesArtifactTrustReason.ExplicitApproval));
        Assert.True(pending.IsCanceled);
    }

    [Fact]
    public async Task CancellationAfterPolicyReturnCannotBecomeApproval()
    {
        using var cancellation = new CancellationTokenSource();
        var policy = new TestPolicy((_, _) =>
        {
            cancellation.Cancel();
            return ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason.ExplicitApproval));
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CompiledRulesArtifactValidation.ValidateAsync(Acquire(Fixture()), new(), policy, cancellation.Token));
    }

    [Fact]
    public async Task PreCancelledValidationNeverCallsPolicy()
    {
        var policy = new TestPolicy();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => CompiledRulesArtifactValidation.ValidateAsync(Acquire(Fixture()), new(), policy, cancellation.Token));
        Assert.Equal(0, policy.Calls);
    }

    [Fact]
    public async Task PolicyCannotMutateValidatedCollectionsOrApprovedBytes()
    {
        var bytes = Fixture("valid-representative.json");
        var acquired = Acquire(bytes);
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new TestPolicy((validated, _) =>
        {
            var artifact = validated.Artifact;
            Assert.Throws<NotSupportedException>(() => artifact.RuleSources.Clear());
            Assert.Throws<NotSupportedException>(() => artifact.Snippets.Clear());
            Assert.Throws<NotSupportedException>(() => artifact.Snippets[0] = artifact.Snippets[0]);
            foreach (var source in artifact.RuleSources)
            {
                Assert.Throws<NotSupportedException>(() => source.DependencyRuleSourceIds.Clear());
                foreach (var selectors in new[] { source.Applicability.WorldModelIds, source.Applicability.ModuleIds, source.Applicability.CampaignModes, source.Applicability.Operations, source.Applicability.Topics })
                    Assert.Throws<NotSupportedException>(() => selectors.Clear());
            }
            foreach (var snippet in artifact.Snippets)
            {
                Assert.Throws<NotSupportedException>(() => snippet.Retrieval.Terms.Clear());
                Assert.Throws<NotSupportedException>(() => snippet.Retrieval.Relationships.Clear());
            }
            validated.CopyBytes()[0] ^= 1;
            return ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason.ExplicitApproval));
        }));
        Assert.True(result.IsImportEligible);
        Assert.True(MemoryMarshal.TryGetArray(acquired.Bytes, out var backing));
        backing.Array![backing.Offset] ^= 1;
        bytes[0] ^= 1;
        var trusted = result.TrustedArtifact!;
        var expected = Fixture("valid-representative.json");
        Assert.Equal(expected, trusted.CopyBytes());
        trusted.CopyBytes()[0] ^= 1;
        Assert.Equal(expected, trusted.CopyBytes());
        Assert.Equal(trusted.SemanticSha256, CompiledRulesArtifactContract.ComputeArtifactSha256(trusted.Artifact));
    }

    [Fact]
    public async Task CallerOwnedEvidenceCannotChangePolicyOrApprovedImportInput()
    {
        var metadata = new Dictionary<string, string> { ["review"] = "approved-fixture" };
        var evidence = new CompiledRulesAcquisitionEvidence("test-memory", metadata: metadata);
        var policy = new TestPolicy((validated, _) =>
        {
            metadata["review"] = "changed-during-policy";
            Assert.Equal("approved-fixture", validated.AcquisitionEvidence.Metadata["review"]);
            return ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason.ExplicitApproval));
        });
        var result = await CompiledRulesArtifactValidation.ValidateAsync(Acquire(Fixture(), evidence), new(), policy);
        metadata.Clear();
        Assert.True(result.IsImportEligible);
        var trusted = result.TrustedArtifact!;
        Assert.Equal("approved-fixture", trusted.AcquisitionEvidence.Metadata["review"]);
        Assert.Same(policy.Seen!.AcquisitionEvidence, trusted.AcquisitionEvidence);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<string, string>)trusted.AcquisitionEvidence.Metadata).Clear());
        Assert.Equal(Fixture(), trusted.CopyBytes());
    }

    [Fact]
    public async Task MutationAttemptAsUnexpectedPolicyExceptionCannotApprove()
    {
        var result = await Validate(Fixture(), new TestPolicy((validated, _) =>
        {
            validated.Artifact.RuleSources.Clear();
            return ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason.ExplicitApproval));
        }));
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustPolicyFailed, result.Failure);
        Assert.Single(result.ValidatedArtifact!.Artifact.RuleSources);
    }

    [Theory]
    [InlineData("local")]
    [InlineData("github")]
    [InlineData("custom")]
    public async Task ProviderChoiceCannotChangeValidityOrArtifactIdentity(string provider)
    {
        var bytes = Fixture();
        var result = await CompiledRulesArtifactValidation.ValidateAsync(Acquire(bytes, new(provider, new("custom-object", "resolved-value"))), new(), new TestPolicy());
        Assert.True(result.IsImportEligible);
        Assert.Equal("content-tree-sha256", result.TrustedArtifact!.Artifact.Ruleset.Source.Scheme);
        Assert.Equal("fixture-source-minimal", result.TrustedArtifact.Artifact.Ruleset.Source.Value);
        Assert.Equal("resolved-value", result.TrustedArtifact.AcquisitionEvidence.ResolvedIdentity!.Value);
        Assert.Equal(provider, result.TrustedArtifact.AcquisitionEvidence.ProviderKind);
        Assert.Equal("32F5B20E3DFD288A0BC15768920BED718E92369971CAC32D652EA6081FE54016", result.TrustedArtifact.SemanticSha256);
    }

    [Theory]
    [InlineData("local")]
    [InlineData("github")]
    [InlineData("custom")]
    public async Task ProviderEvidenceCannotMakeInvalidBytesValid(string provider)
    {
        var policy = new TestPolicy();
        var result = await CompiledRulesArtifactValidation.ValidateAsync(Acquire("{broken"u8.ToArray(), new(provider)), new(), policy);
        AssertFailure(result, CompiledRulesAcquisitionFailure.MalformedArtifact, policy);
        Assert.Equal("JSON_INVALID", Assert.Single(result.Diagnostics).Code);
    }

    [Fact]
    public async Task ExplicitEvidencePolicyMayChangeTrustWithoutChangingValidity()
    {
        var policy = new TestPolicy((artifact, _) => ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(
            artifact.AcquisitionEvidence.ProviderKind == "approved-test-provider"
                ? CompiledRulesArtifactTrustReason.ExplicitApproval : CompiledRulesArtifactTrustReason.ExplicitRejection)));
        var accepted = await CompiledRulesArtifactValidation.ValidateAsync(Acquire(Fixture(), new("approved-test-provider")), new(), policy);
        var rejected = await CompiledRulesArtifactValidation.ValidateAsync(Acquire(Fixture(), new("other-provider")), new(), policy);
        Assert.True(accepted.IsArtifactValid);
        Assert.True(rejected.IsArtifactValid);
        Assert.True(accepted.IsImportEligible);
        Assert.False(rejected.IsImportEligible);
        Assert.Equal(accepted.ValidatedArtifact!.SemanticSha256, rejected.ValidatedArtifact!.SemanticSha256);
    }

    [Fact]
    public async Task ExplicitExpectedHashPolicyIsReplaceableNotPublisherAuthentication()
    {
        var bytes = Fixture();
        var expected = Convert.ToHexString(SHA256.HashData(bytes));
        ICompiledRulesArtifactTrustPolicy Policy(string hash) => new TestPolicy((artifact, _) => ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(
            artifact.ByteSha256 == hash ? CompiledRulesArtifactTrustReason.ExpectedByteHashMatched : CompiledRulesArtifactTrustReason.ExpectedByteHashMismatch)));
        var accepted = await Validate(bytes, Policy(expected));
        var rejected = await Validate(bytes, Policy(new string('0', 64)));
        Assert.True(accepted.IsImportEligible);
        Assert.Equal(CompiledRulesArtifactTrustReason.ExpectedByteHashMatched, accepted.TrustDecision!.Reason);
        Assert.True(rejected.IsArtifactValid);
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustRejected, rejected.Failure);
    }

    [Fact]
    public async Task RepeatCultureAndUnrelatedWorkingDirectoryPreserveResults()
    {
        var bytes = Fixture();
        var first = await Validate(bytes, new TestPolicy());
        var previousCulture = CultureInfo.CurrentCulture;
        var previousDirectory = Environment.CurrentDirectory;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Environment.CurrentDirectory = Path.GetTempPath();
            var second = await Validate(bytes, new TestPolicy());
            Assert.Equal(first.TrustedArtifact!.CopyBytes(), second.TrustedArtifact!.CopyBytes());
            Assert.Equal(first.TrustedArtifact.ByteSha256, second.TrustedArtifact.ByteSha256);
            Assert.Equal(first.TrustedArtifact.SemanticSha256, second.TrustedArtifact.SemanticSha256);
            Assert.Equal(first.TrustDecision!.Reason, second.TrustDecision!.Reason);
            Assert.Equal(first.TrustDecision.Outcome, second.TrustDecision.Outcome);
            Assert.Equal(JsonSerializer.Serialize(first), JsonSerializer.Serialize(second));
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Fact]
    public async Task ReformattingChangesByteIdentityNotSemanticIdentity()
    {
        var bytes = Fixture();
        var compact = Encoding.UTF8.GetBytes(JsonNode.Parse(bytes)!.ToJsonString());
        var first = await Validate(bytes, new TestPolicy());
        var second = await Validate(compact, new TestPolicy());
        Assert.NotEqual(first.TrustedArtifact!.ByteSha256, second.TrustedArtifact!.ByteSha256);
        Assert.Equal(first.TrustedArtifact.SemanticSha256, second.TrustedArtifact.SemanticSha256);
        Assert.Equal(bytes, first.TrustedArtifact.CopyBytes());
        Assert.Equal(compact, second.TrustedArtifact.CopyBytes());
    }

    [Fact]
    public async Task DiagnosticPathsNeverEchoUnknownPropertyNamesOrHugePayloads()
    {
        var secret = "password=private-token&Authorization=secret" + new string('x', 10000);
        var json = "{" + JsonSerializer.Serialize(secret) + ":null," + JsonSerializer.Serialize(secret) + ":null," + Encoding.UTF8.GetString(Fixture())[1..];
        var policy = new TestPolicy();
        var result = await Validate(Encoding.UTF8.GetBytes(json), policy);
        AssertFailure(result, CompiledRulesAcquisitionFailure.MalformedArtifact, policy);
        Assert.All(result.Diagnostics, error =>
        {
            Assert.Equal("$", error.Path);
            Assert.InRange(error.Path.Length, 1, 256);
            Assert.InRange(error.Message.Length, 1, 256);
        });
        var diagnostics = JsonSerializer.Serialize(result);
        Assert.InRange(diagnostics.Length, 1, 2000);
        Assert.DoesNotContain("private-token", diagnostics);
        Assert.DoesNotContain("Authorization", diagnostics);
    }

    [Fact]
    public async Task DiagnosticCountIsBoundedAndTruncationIsExplicit()
    {
        var json = "{" + string.Concat(Enumerable.Repeat("\"ruleset\":null,", 64)) + Encoding.UTF8.GetString(Fixture())[1..];
        var result = await Validate(Encoding.UTF8.GetBytes(json), new TestPolicy());
        Assert.Equal(16, result.Diagnostics.Count);
        Assert.True(result.DiagnosticsTruncated);
        Assert.All(result.Diagnostics, error => Assert.Equal("DUPLICATE_JSON_PROPERTY", error.Code));
        Assert.Throws<NotSupportedException>(() => ((IList<CompiledRulesArtifactValidationError>)result.Diagnostics).Clear());
    }

    [Fact]
    public async Task SafeResultSerializationNeverEchoesRawProviderEvidenceOrRulePayload()
    {
        var evidence = new CompiledRulesAcquisitionEvidence("private-token", new("opaque", "password=secret"), [KeyValuePair.Create("Authorization", "Bearer private-token")]);
        var result = await CompiledRulesArtifactValidation.ValidateAsync(Acquire(Fixture(), evidence), new(), new TestPolicy());
        Assert.True(result.IsImportEligible);
        foreach (var serialized in new[] { JsonSerializer.Serialize(result), JsonSerializer.Serialize(result.TrustedArtifact), JsonSerializer.Serialize(result.ValidatedArtifact) })
        {
            Assert.DoesNotContain("private-token", serialized);
            Assert.DoesNotContain("password", serialized);
            Assert.DoesNotContain("Authority remains", serialized);
            Assert.DoesNotContain("manifestPath", serialized);
        }
        Assert.Equal("password=secret", result.TrustedArtifact!.AcquisitionEvidence.ResolvedIdentity!.Value);
    }

    [Fact]
    public async Task CurrentCanonicalArtifactIsAcceptedWithoutCompilerOrVocabularyChanges()
    {
        using var payload = new CanonicalVocabularyPayload();
        var compiled = RuleCompilationPipeline.Compile(payload.Load());
        var result = await Validate(compiled.Bytes.ToArray(), new TestPolicy());
        Assert.True(result.IsImportEligible);
        var trusted = result.TrustedArtifact!;
        Assert.Equal(10, trusted.Artifact.RuleSources.Count);
        Assert.Equal(154, trusted.Artifact.Snippets.Count);
        Assert.Equal(275622, trusted.CopyBytes().Length);
        Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", trusted.SemanticSha256);
        Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", trusted.ByteSha256);
        Assert.Equal(compiled.Bytes.ToArray(), CompiledRulesArtifactWriter.Write(trusted.Artifact).Bytes.ToArray());
    }

    [Theory]
    [InlineData(CompiledRulesAcquisitionFailure.TrustPolicyFailed, "ARTIFACT_TRUST_POLICY_FAILED")]
    [InlineData(CompiledRulesAcquisitionFailure.ValidationFailed, "ARTIFACT_VALIDATION_FAILED")]
    public void NewFailureCodesAreBoundedAndAppendToExistingTaxonomy(CompiledRulesAcquisitionFailure failure, string code)
    {
        var error = new CompiledRulesAcquisitionException(failure);
        Assert.Equal(code, error.Code);
        Assert.InRange(error.Message.Length, 1, 256);
        Assert.Null(error.InnerException);
        Assert.Equal(11, (int)CompiledRulesAcquisitionFailure.StorageFailed);
    }

    private static byte[] Fixture(string name = "valid-minimal.json") => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "ArtifactFixture", name));
    private static AcquiredCompiledRulesArtifact Acquire(byte[] bytes, CompiledRulesAcquisitionEvidence? evidence = null) => new(bytes, evidence ?? new("test-memory"), new());
    private static Task<CompiledRulesArtifactValidationResult> Validate(byte[] bytes, ICompiledRulesArtifactTrustPolicy policy) => CompiledRulesArtifactValidation.ValidateAsync(Acquire(bytes), new(), policy);
    private static byte[] Mutate(Action<JsonNode> mutation)
    {
        var node = JsonNode.Parse(Fixture())!;
        mutation(node);
        return Encoding.UTF8.GetBytes(node.ToJsonString());
    }
    private static void Rehash(JsonNode node)
    {
        var parsed = CompiledRulesArtifactContract.Read(node.ToJsonString());
        Assert.NotNull(parsed.Artifact);
        node["integrity"]!["artifactSha256"] = CompiledRulesArtifactContract.ComputeArtifactSha256(parsed.Artifact);
    }
    private static void AssertFailure(CompiledRulesArtifactValidationResult result, CompiledRulesAcquisitionFailure failure, TestPolicy policy)
    {
        Assert.Equal(failure, result.Failure);
        Assert.False(result.IsImportEligible);
        Assert.False(result.IsArtifactValid);
        Assert.Null(result.TrustedArtifact);
        Assert.Null(result.TrustDecision);
        Assert.Equal(0, policy.Calls);
    }
    private sealed class TestPolicy(Func<ValidatedCompiledRulesArtifact, CancellationToken, ValueTask<CompiledRulesArtifactTrustDecision>>? evaluate = null) : ICompiledRulesArtifactTrustPolicy
    {
        public int Calls { get; private set; }
        public ValidatedCompiledRulesArtifact? Seen { get; private set; }
        public ValueTask<CompiledRulesArtifactTrustDecision> EvaluateAsync(ValidatedCompiledRulesArtifact artifact, CancellationToken cancellationToken)
        {
            Calls++;
            Seen = artifact;
            return evaluate?.Invoke(artifact, cancellationToken) ?? ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason.ExplicitApproval));
        }
    }
}
