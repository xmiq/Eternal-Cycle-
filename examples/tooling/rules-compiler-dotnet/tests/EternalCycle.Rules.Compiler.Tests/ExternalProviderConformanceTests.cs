using System.Reflection;
using System.Runtime.CompilerServices;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Compiler.Tests;

public sealed class ExternalProviderConformanceTests
{
    [Fact]
    public async Task CustomProviderCompilesAndPassesSharedGateWithoutFriendAccess()
    {
        var consumer = typeof(MemoryStoreCompiledRulesArtifactProvider).Assembly.GetName().Name;
        Assert.DoesNotContain(typeof(ICompiledRulesArtifactProvider).Assembly.GetCustomAttributes<InternalsVisibleToAttribute>(),
            attribute => attribute.AssemblyName == consumer);
        using var payload = new CanonicalVocabularyPayload();
        var bytes = RuleCompilationPipeline.Compile(payload.Load()).Bytes.ToArray();
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("rules/current", "version-1",
            () => new MemoryStream(bytes, writable: false));
        var acquired = await provider.AcquireAsync(new(), default);
        var result = await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new Approval());
        Assert.True(result.IsImportEligible);
        Assert.Equal(bytes, result.TrustedArtifact!.CopyBytes());
        Assert.Equal(154, result.TrustedArtifact.Artifact.Snippets.Count);
    }

    [Fact]
    public async Task CustomAcquisitionNeedsNoArtifactFileOrRepositoryLayout()
    {
        byte[] bytes = [0, 255, 13, 10];
        ICompiledRulesArtifactProvider provider = new MemoryStoreCompiledRulesArtifactProvider("logical-object", "version-2",
            () => new MemoryStream(bytes, writable: false));
        var previous = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = Path.GetTempPath();
            var acquired = await provider.AcquireAsync(new(), default);
            Assert.Equal(bytes, acquired.Bytes.ToArray());
            Assert.Equal(CompiledRulesAcquisitionFailure.MalformedArtifact,
                (await CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), new Approval())).Failure);
        }
        finally { Environment.CurrentDirectory = previous; }
    }

    private sealed class Approval : ICompiledRulesArtifactTrustPolicy
    {
        public ValueTask<CompiledRulesArtifactTrustDecision> EvaluateAsync(ValidatedCompiledRulesArtifact artifact,
            CancellationToken cancellationToken) => ValueTask.FromResult(new CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason.ExplicitApproval));
    }
}
