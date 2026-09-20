using System.Text.Json;
using System.Text.Json.Serialization;
using EternalCycle.Persistence.Mcp;
using Microsoft.Extensions.Options;
using Xunit;

namespace EternalCycle.Persistence.Mcp.Tests;

public sealed class GmRuntimeCorrectiveTests
{
    [Fact]
    public void CanonicalBootstrapIsCompactAndContainsIrreducibleInvariants()
    {
        var root = FindRepositoryRoot();
        var bootstrap = File.ReadAllText(Path.Combine(root, "docs", "rules", "GM_HOST_BOOTSTRAP.txt"));
        var words = bootstrap.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

        Assert.InRange(words, 100, 220);
        Assert.Contains("authoritative GM Runtime Procedure", bootstrap, StringComparison.Ordinal);
        Assert.Contains("conversation history is never a substitute", bootstrap, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Never invent or choose a player-controlled decision", bootstrap, StringComparison.Ordinal);
        Assert.Contains("Persist and validate", bootstrap, StringComparison.Ordinal);
        Assert.Contains("not new player turns", bootstrap, StringComparison.Ordinal);
        Assert.Contains("stop and wait for actual new player input", bootstrap, StringComparison.Ordinal);
    }

    [Fact]
    public void OfficialManifestVersionsBootstrapAndMandatoryRuntimeProcedure()
    {
        var root = FindRepositoryRoot();
        var manifest = JsonSerializer.Deserialize<RuleSourceManifest>(
            File.ReadAllText(Path.Combine(root, "docs", "rules", "rule-source-manifest.json")),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            })!;

        var procedure = Assert.Single(manifest.Sources, source =>
            source.RuleSourceId == RuleCompiler.GmRuntimeProcedureSourceId);
        var bootstrap = Assert.Single(manifest.Sources, source =>
            source.RuleSourceId == RuleCompiler.GmHostBootstrapSourceId);
        Assert.True(procedure.AlwaysInclude);
        Assert.Contains("gameplay.resolve", procedure.Operations);
        Assert.Contains(RuleCompiler.RuntimeKernelSourceId, procedure.Dependencies);
        Assert.False(bootstrap.AlwaysInclude);
        Assert.Contains("setup.gm-host", bootstrap.Operations);
    }

    [Fact]
    public void GameplayResolveIncludesProcedureWithoutCallerTopic()
    {
        var index = Index();
        var result = RuleCompiler.Select(
            index,
            Route(),
            new RuleContextRequest("campaign", "gameplay.resolve", ["combat"], "NORMAL"));

        Assert.Contains(result.Chunks, chunk => chunk.RuleSourceId == RuleCompiler.RuntimeKernelSourceId);
        Assert.Contains(result.Chunks, chunk => chunk.RuleSourceId == RuleCompiler.GmRuntimeProcedureSourceId);
        Assert.DoesNotContain(result.Chunks, chunk => chunk.RuleSourceId == RuleCompiler.GmHostBootstrapSourceId);
    }

    [Fact]
    public void MissingGameplayProcedureIsRejectedRatherThanSilentlyOmitted()
    {
        var index = RuleCompiler.Compile("1.0.0", [Kernel(), Bootstrap()]);

        var exception = Assert.Throws<InvalidOperationException>(() => RuleCompiler.Select(
            index,
            Route(),
            new RuleContextRequest("campaign", "gameplay.resolve", [], "NORMAL")));

        Assert.Contains("GM Runtime Procedure", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CompactPacketRetainsIdentityAndContentWithoutPerChunkMetadataEnvelope()
    {
        var selected = RuleCompiler.Select(
            Index(),
            Route(),
            new RuleContextRequest("campaign", "gameplay.resolve", ["combat"], "NORMAL")) with
        {
            RuleReleaseId = "RULE-1",
            SourceIdentity = new string('A', 40)
        };
        var compact = RulePacketFormatter.Compact(selected);
        var fullJson = JsonSerializer.Serialize(selected);
        var compactJson = JsonSerializer.Serialize(compact);

        Assert.Equal("RULE-1", compact.RuleReleaseId);
        Assert.Equal(new string('A', 40), compact.SourceIdentity);
        Assert.Contains(compact.Rules, section => section.RuleSourceId == RuleCompiler.GmRuntimeProcedureSourceId);
        Assert.DoesNotContain("sourcePath", compactJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sourceHash", compactJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("preparationTier", compactJson, StringComparison.OrdinalIgnoreCase);
        Assert.True(compactJson.Length < fullJson.Length * 0.65,
            $"Expected compact packet under 65% of full envelope; compact={compactJson.Length}, full={fullJson.Length}.");
    }

    [Fact]
    public void HostReadinessBlocksGameplayUntilUserConfirmationOrVerification()
    {
        var required = ManagedReadinessEvaluator.Evaluate(
            ReadySnapshot(GmHostConfigurationState.Required),
            campaignRequested: false);
        var presented = ManagedReadinessEvaluator.Evaluate(
            ReadySnapshot(GmHostConfigurationState.InstructionsPresented),
            campaignRequested: false);
        var confirmed = ManagedReadinessEvaluator.Evaluate(
            ReadySnapshot(GmHostConfigurationState.UserConfirmed),
            campaignRequested: false);
        var verified = ManagedReadinessEvaluator.Evaluate(
            ReadySnapshot(GmHostConfigurationState.Verified),
            campaignRequested: false);

        Assert.Equal(ManagedReadinessState.GmHostConfigurationRequired, required.State);
        Assert.Equal(ManagedReadinessState.GmHostConfigurationRequired, presented.State);
        Assert.False(required.GameplayReady);
        Assert.False(presented.GameplayReady);
        Assert.True(confirmed.GameplayReady);
        Assert.True(verified.GameplayReady);
        Assert.Equal(GmHostConfigurationState.UserConfirmed, confirmed.GmHostConfiguration);
        Assert.NotEqual(GmHostConfigurationState.Verified, confirmed.GmHostConfiguration);
    }

    [Fact]
    public async Task SetupPresentsCanonicalTextAndNaturalConfirmationIsNotVerification()
    {
        var root = FindRepositoryRoot();
        var expected = File.ReadAllText(Path.Combine(root, "docs", "rules", "GM_HOST_BOOTSTRAP.txt")).Trim();
        var release = Release(expected);
        var configurationStore = new MemoryHostConfigurationStore();
        var service = new GmHostConfigurationService(
            new SinglePublishedRuleStore(release),
            configurationStore,
            Options.Create(new ManagedRuleServiceOptions()));

        var presented = await service.PresentInstructionsAsync(CancellationToken.None);
        var confirmed = await service.ConfirmAsync(
            new GmHostConfigurationConfirmation(true),
            CancellationToken.None);
        var repeated = await service.ConfirmAsync(
            new GmHostConfigurationConfirmation(true),
            CancellationToken.None);

        Assert.True(presented.Success);
        Assert.Equal(
            expected.Replace("\r\n", "\n", StringComparison.Ordinal),
            presented.Data?.Bootstrap.Text.Trim().Replace("\r\n", "\n", StringComparison.Ordinal));
        Assert.Equal(GmHostConfigurationState.InstructionsPresented, presented.Data?.State);
        Assert.True(confirmed.Success);
        Assert.Equal(GmHostConfigurationState.UserConfirmed, confirmed.Data?.State);
        Assert.Null(confirmed.Data?.VerifiedAt);
        Assert.True(repeated.Success);
        Assert.Equal(GmHostConfigurationState.UserConfirmed, repeated.Data?.State);
        Assert.Equal(2, configurationStore.SaveCount);
    }

    [Fact]
    public async Task CapableHostCanRecordVerifiedWithoutChangingGenericConfirmationContract()
    {
        var store = new MemoryHostConfigurationStore();
        var service = new GmHostConfigurationService(
            new SinglePublishedRuleStore(Release("Canonical bootstrap.")),
            store,
            Options.Create(new ManagedRuleServiceOptions()));

        var verified = await service.RecordVerifiedAsync("test-host-attestation", CancellationToken.None);

        Assert.Equal(GmHostConfigurationState.Verified, verified.State);
        Assert.NotNull(verified.VerifiedAt);
    }

    [Fact]
    public void RuntimeProcedureDefinesOneInputInteractionAndAgencyYield()
    {
        var root = FindRepositoryRoot();
        var procedure = File.ReadAllText(Path.Combine(root, "docs", "rules", "GM_RUNTIME_PROCEDURE.md"));

        Assert.Contains("actual new player input", procedure, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("remain inside the same interaction", procedure, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("it must yield", procedure, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("may not select an unresolved option", procedure, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("validation and read-back evidence", procedure, StringComparison.OrdinalIgnoreCase);
    }

    private static CompiledRuleIndex Index() => RuleCompiler.Compile(
        "1.0.0",
        [
            Kernel(),
            Procedure(),
            Bootstrap(),
            Document("combat", "Combat consequences remain contextual.", ["combat"])
        ]);

    private static RuleSourceDocument Kernel() => new(
        RuleCompiler.RuntimeKernelSourceId,
        "docs/rules/kernel.md",
        "# Kernel\n\nAuthority and canonical state are mandatory.",
        Metadata(RuleLayer.RuntimeKernel, ["*"], [], true));

    private static RuleSourceDocument Procedure() => new(
        RuleCompiler.GmRuntimeProcedureSourceId,
        "docs/rules/gm-runtime.md",
        "# GM Runtime Procedure\n\nRead, resolve, persist, validate, narrate, then yield.",
        Metadata(RuleLayer.Core, ["gameplay.resolve"], [], true, [RuleCompiler.RuntimeKernelSourceId]));

    private static RuleSourceDocument Bootstrap() => new(
        RuleCompiler.GmHostBootstrapSourceId,
        "docs/rules/gm-host-bootstrap.txt",
        "Canonical host bootstrap.",
        Metadata(RuleLayer.Core, ["setup.gm-host"], [], false, [RuleCompiler.RuntimeKernelSourceId]));

    private static RuleSourceDocument Document(string id, string content, IReadOnlyList<string> topics) => new(
        id,
        $"docs/rules/{id}.md",
        $"# {id}\n\n{content}",
        Metadata(RuleLayer.Core, ["gameplay.resolve"], topics));

    private static RuleSourceMetadata Metadata(
        RuleLayer layer,
        IReadOnlyList<string> operations,
        IReadOnlyList<string> topics,
        bool alwaysInclude = false,
        IReadOnlyList<string>? dependencies = null) =>
        new(layer, [], [], ["NORMAL"], operations, topics, 0, alwaysInclude, dependencies ?? []);

    private static CampaignSchemaRoute Route() =>
        new("campaign", "world", "namespace", "ec", "1", "eternal-cycle-core", "1.0.0");

    private static ManagedInfrastructureSnapshot ReadySnapshot(GmHostConfigurationState state) =>
        new(
            ManagedComponentStatus.Ready,
            ManagedComponentStatus.Ready,
            ManagedComponentStatus.Ready,
            ManagedComponentStatus.Ready,
            1,
            "RULE-1",
            new string('A', 40),
            true,
            ManagedComponentStatus.NotRequested,
            "Activated",
            false,
            RuleKernelReady: true,
            CampaignBootstrapReady: true,
            FullRulesetReady: true,
            GmHostConfiguration: state);

    private static PublishedRuleRelease Release(string bootstrapText)
    {
        var index = RuleCompiler.Compile(
            "1.0.0",
            [Kernel(), Procedure(), Bootstrap() with { Content = bootstrapText }]);
        return new PublishedRuleRelease(
            "RULE-1",
            "eternal-cycle-core",
            "Git",
            new string('A', 40),
            "1.0.0",
            "1",
            RuleReleaseState.Active,
            index,
            DateTimeOffset.UnixEpoch);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "VERSION")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }

    private sealed class MemoryHostConfigurationStore : IGmHostConfigurationStore
    {
        private StoredGmHostConfiguration? current;

        public int SaveCount { get; private set; }

        public Task<StoredGmHostConfiguration?> GetAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult(current);

        public Task<StoredGmHostConfiguration> SaveAsync(
            StoredGmHostConfiguration configuration,
            CancellationToken cancellationToken)
        {
            SaveCount++;
            current = configuration with { Revision = (current?.Revision ?? 0) + 1 };
            return Task.FromResult(current);
        }
    }

    private sealed class SinglePublishedRuleStore(PublishedRuleRelease release) : IPublishedRuleStore
    {
        public Task<PublishedRuleRelease?> GetActiveAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult<PublishedRuleRelease?>(release);

        public Task<PublishedRuleRelease?> FindBySourceAsync(string rulesetId, string sourceIdentity, CancellationToken cancellationToken) =>
            Task.FromResult<PublishedRuleRelease?>(null);

        public Task<PublishedRuleRelease?> GetByIdAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken) =>
            Task.FromResult<PublishedRuleRelease?>(release.RuleReleaseId == ruleReleaseId ? release : null);

        public Task<RuleUpdateCheck?> GetLatestUpdateCheckAsync(string rulesetId, CancellationToken cancellationToken) =>
            Task.FromResult<RuleUpdateCheck?>(null);

        public Task StageCandidateAsync(PublishedRuleRelease candidate, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task SetStateAsync(string ruleReleaseId, RuleReleaseState state, string? failureReason, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task ActivateAsync(string rulesetId, string ruleReleaseId, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task RecordUpdateCheckAsync(RuleUpdateCheck updateCheck, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
