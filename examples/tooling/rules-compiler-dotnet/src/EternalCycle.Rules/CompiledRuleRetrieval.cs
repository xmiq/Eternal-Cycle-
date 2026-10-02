using System.Text;

namespace EternalCycle.Rules;

public sealed record CompiledRuleStoreScope(string RulesetId, string SemanticSha256);

public sealed record CompiledRuleRetrievalRequest(
    CompiledRuleStoreScope Scope,
    string Operation,
    string CampaignMode,
    IReadOnlyList<string> QueryTerms)
{
    public string? WorldModelId { get; init; }
    public IReadOnlyList<string> ModuleIds { get; init; } = [];
    public IReadOnlyList<string> Topics { get; init; } = [];
    public IReadOnlyList<string> RequiredRuleSourceIds { get; init; } = [];
    public IReadOnlyList<string> RequiredSnippetIds { get; init; } = [];
    public int MaximumEstimatedTokens { get; init; } = CompiledRuleRetrieval.MaximumEstimatedTokens;

    // Query input is untrusted and is not an automatic diagnostic payload.
    public override string ToString() => nameof(CompiledRuleRetrievalRequest);
}

public sealed class PreparedCompiledRuleRetrievalRequest
{
    internal PreparedCompiledRuleRetrievalRequest(
        CompiledRuleRetrievalRequest request,
        IReadOnlyList<string> queryTerms,
        IReadOnlyList<string> moduleIds,
        IReadOnlyList<string> topics,
        IReadOnlyList<string> requiredRuleSourceIds,
        IReadOnlyList<string> requiredSnippetIds)
    {
        Scope = request.Scope;
        Operation = request.Operation;
        CampaignMode = request.CampaignMode;
        WorldModelId = request.WorldModelId;
        MaximumEstimatedTokens = request.MaximumEstimatedTokens;
        QueryTerms = queryTerms;
        ModuleIds = moduleIds;
        Topics = topics;
        RequiredRuleSourceIds = requiredRuleSourceIds;
        RequiredSnippetIds = requiredSnippetIds;
    }

    public CompiledRuleStoreScope Scope { get; }
    public string Operation { get; }
    public string CampaignMode { get; }
    public string? WorldModelId { get; }
    public int MaximumEstimatedTokens { get; }
    public IReadOnlyList<string> QueryTerms { get; }
    public IReadOnlyList<string> ModuleIds { get; }
    public IReadOnlyList<string> Topics { get; }
    public IReadOnlyList<string> RequiredRuleSourceIds { get; }
    public IReadOnlyList<string> RequiredSnippetIds { get; }
    public override string ToString() => nameof(PreparedCompiledRuleRetrievalRequest);
}

public enum CompiledRuleRetrievalFailure
{
    RequestInvalid,
    ArtifactUnavailable,
    ArtifactInconsistent,
    DependencyFailed,
    PacketBudgetInsufficient,
    StorageFailed
}

public sealed class CompiledRuleRetrievalException : Exception
{
    public CompiledRuleRetrievalException(CompiledRuleRetrievalFailure failure) : base(failure switch
    {
        CompiledRuleRetrievalFailure.RequestInvalid => "The rule retrieval request is invalid or exceeds its bounds.",
        CompiledRuleRetrievalFailure.ArtifactUnavailable => "The authorized compiled rule artifact is unavailable.",
        CompiledRuleRetrievalFailure.ArtifactInconsistent => "The stored compiled rule artifact is inconsistent.",
        CompiledRuleRetrievalFailure.DependencyFailed => "The required rule dependency closure is unavailable or inconsistent.",
        CompiledRuleRetrievalFailure.PacketBudgetInsufficient => "The packet budget cannot preserve the required rule closure.",
        CompiledRuleRetrievalFailure.StorageFailed => "Compiled rule storage could not complete retrieval.",
        _ => throw new ArgumentOutOfRangeException(nameof(failure))
    })
    {
        Failure = failure;
        Code = failure switch
        {
            CompiledRuleRetrievalFailure.RequestInvalid => "RULE_RETRIEVAL_REQUEST_INVALID",
            CompiledRuleRetrievalFailure.ArtifactUnavailable => "RULE_RETRIEVAL_ARTIFACT_UNAVAILABLE",
            CompiledRuleRetrievalFailure.ArtifactInconsistent => "RULE_RETRIEVAL_ARTIFACT_INCONSISTENT",
            CompiledRuleRetrievalFailure.DependencyFailed => "RULE_RETRIEVAL_DEPENDENCY_FAILED",
            CompiledRuleRetrievalFailure.PacketBudgetInsufficient => "RULE_RETRIEVAL_BUDGET_INSUFFICIENT",
            _ => "RULE_RETRIEVAL_STORAGE_FAILED"
        };
    }

    public CompiledRuleRetrievalFailure Failure { get; }
    public string Code { get; }
}

public static class CompiledRuleRetrieval
{
    // Reuse the existing normal Rule Packet ceiling, not a new context estimator.
    public const int MaximumEstimatedTokens = 8000;
    public const int MaximumQueryTerms = 16;
    public const int MaximumIdentitySetSize = 32;

    public static PreparedCompiledRuleRetrievalRequest Prepare(
        CompiledRuleRetrievalRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request is null || request.Scope is null || !Identifier(request.Scope.RulesetId) ||
            request.Scope.SemanticSha256 is not { Length: 64 } digest ||
            digest.Any(character => character is not (>= '0' and <= '9' or >= 'A' and <= 'F')) ||
            !Identifier(request.Operation) || !Identifier(request.CampaignMode) ||
            (request.WorldModelId is not null && !Identifier(request.WorldModelId)) ||
            request.MaximumEstimatedTokens is < 1 or > MaximumEstimatedTokens)
        {
            throw Invalid();
        }

        var terms = PrepareSet(request.QueryTerms, MaximumQueryTerms, RuleRetrievalVocabulary.NormalizeTerm, cancellationToken);
        var modules = PrepareSet(request.ModuleIds, MaximumIdentitySetSize, ConcreteId, cancellationToken);
        var topics = PrepareSet(request.Topics, MaximumIdentitySetSize, ConcreteId, cancellationToken);
        var sources = PrepareSet(request.RequiredRuleSourceIds, MaximumIdentitySetSize, ConcreteId, cancellationToken);
        var snippets = PrepareSet(request.RequiredSnippetIds, MaximumIdentitySetSize, SnippetId, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new(request, terms, modules, topics, sources, snippets);
    }

    private static IReadOnlyList<string> PrepareSet(
        IReadOnlyList<string> values, int maximum, Func<string, string> normalize,
        CancellationToken cancellationToken)
    {
        if (values is null || values.Count > maximum) throw Invalid();
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try { set.Add(normalize(value)); }
            catch (ArgumentException) { throw Invalid(); }
        }
        // Bound before deduplication so repeated input cannot evade work limits.
        // Read-only private copies prevent caller mutation during later retrieval.
        return Array.AsReadOnly(set.Order(StringComparer.Ordinal).ToArray());
    }

    private static string ConcreteId(string value) => Identifier(value) ? value : throw Invalid();

    private static string SnippetId(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > 257) throw Invalid();
        var separator = value.IndexOf('#');
        if (separator < 0) return ConcreteId(value);
        if (!Identifier(value[..separator]) || !Identifier(value[(separator + 1)..])) throw Invalid();
        return value;
    }

    // These are format-1 identity rules, not term normalization. Keep caller IDs
    // exact; a wildcard belongs in authored applicability, never caller scope.
    private static bool Identifier(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128 || !char.IsLetterOrDigit(value[0]) ||
            value.Any(character => char.IsControl(character) || char.IsWhiteSpace(character) || character is '#' or '/' or '\\' or ':'))
            return false;
        try { return value == value.Normalize(NormalizationForm.FormC); }
        catch (ArgumentException) { return false; }
    }

    private static CompiledRuleRetrievalException Invalid() => new(CompiledRuleRetrievalFailure.RequestInvalid);
}
