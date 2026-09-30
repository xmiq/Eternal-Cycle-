namespace EternalCycle.Rules;

public sealed class RuleSourceManifest
{
    public int ManifestFormatVersion { get; init; }

    public string CompilerContractVersion { get; init; } = string.Empty;

    public string RulesetId { get; init; } = string.Empty;

    public string RepositoryVersion { get; init; } = string.Empty;

    public IList<RuleSourceManifestEntry> Sources { get; init; } = [];
}

public sealed class RuleSourceManifestEntry
{
    public string RuleSourceId { get; init; } = string.Empty;

    public string Path { get; init; } = string.Empty;

    public RuleLayer Layer { get; init; }

    public IList<string> WorldModelIds { get; init; } = [];

    public IList<string> ModuleIds { get; init; } = [];

    public IList<string> CampaignModes { get; init; } = ["*"];

    public IList<string> Operations { get; init; } = ["*"];

    public IList<string> Topics { get; init; } = [];

    public IList<string> Dependencies { get; init; } = [];

    public int Priority { get; init; }

    public bool AlwaysInclude { get; init; }

    public RulePreparationTier PreparationTier { get; init; } = RulePreparationTier.Standard;
}
