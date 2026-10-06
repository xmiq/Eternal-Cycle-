using System.Text;
using System.Text.Json;

namespace EternalCycle.Rules.Testing;

// Shared acceptance data, not a new retrieval implementation. The explicit caller
// identity commits to the LF materialized test bytes, not an uncommitted Git SHA.
internal static class OrdinaryAuthorityCandidate
{
    internal static MaterializedRuleSourceSnapshot Load(CanonicalVocabularyPayload payload) => payload.LoadContentAddressed();

    internal static RuleCompilationResult Compile()
    {
        using var payload = new CanonicalVocabularyPayload(current: true);
        return RuleCompilationPipeline.Compile(Load(payload));
    }

    internal static RetrievalQualityCase[] Cases() =>
    [
        Case("save", "save"),
        Case("combined", "fighting", "conflict", "save", "preparation"),
        Case("adjudication", "gm adjudication"),
        Case("agency", "wait for player input"),
        Case("secrets", "gm secrets"),
        Case("ecology", "population and ecology"),
        Case("fighting", "fighting"),
        Case("fighting-3000", "fighting") with { Budget = 3000, ExpectedCost = 2985 },
        Case("conflict", "conflict"),
        Case("conflict-3000", "conflict") with { Budget = 3000 },
        Case("preparation", "preparation"),
        Case("preparation-3000", "preparation") with { Budget = 3000, ExpectedCost = 2979 },
        Case("canon", "campaign canon") with { Operation = "context.assemble" },
        Case("canon-1400", "campaign canon") with { Operation = "context.assemble", Budget = 1400 },
        Case("unknown", "unreviewed") with { ExpectNoHits = true, ExpectedCost = 2027 },
        Case("empty") with { ExpectNoHits = true, ExpectedCost = 2027 },
        Case("exact-gameplay") with { Budget = 2027, ExpectedCost = 2027 },
        Case("below-gameplay") with { Budget = 2026, ExpectedFailure = CompiledRuleRetrievalFailure.PacketBudgetInsufficient },
        Case("exact-context") with { Operation = "context.assemble", Budget = 1010, ExpectedCost = 1010 },
        Case("below-context") with { Operation = "context.assemble", Budget = 1009, ExpectedFailure = CompiledRuleRetrievalFailure.PacketBudgetInsufficient },
        Case("direct-authority", "conflict") with { Operation = "context.assemble" },
        Case("explicit-authority") with { RequiredRuleSourceIds = ["core-persistence-authority"] },
        Case("governance") with { Operation = "context.assemble", RequiredRuleSourceIds = ["core-authority-governance"] }
    ];

    private static RetrievalQualityCase Case(string id, params string[] terms) =>
        new() { Id = id, Category = "ordinary-authority-r2", QueryTerms = terms };
}
