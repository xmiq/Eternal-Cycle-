namespace EternalCycle.Rules;

public enum RuleLayer
{
    RuntimeKernel,
    Core,
    World,
    OptionalModule
}

public enum RulePreparationTier
{
    RuntimeKernel = 0,
    CampaignBootstrap = 1,
    ImmediateGameplayCore = 2,
    CampaignRelevant = 3,
    Standard = 4,
    OptionalRare = 5
}
