using System.Diagnostics.CodeAnalysis;

namespace EternalCycle.Rules;

public sealed record RuleVocabularyBindingIdentity(
    string RuleSourceId,
    RuleVocabularyBindingScope Scope,
    string? SourceAnchor);

public sealed record RuleVocabularyOriginEvidence(
    RuleVocabularyBindingIdentity Binding,
    string BindingRationale,
    string ConceptRationale,
    string TermRationale);

public sealed record RuleVocabularyAssociation(
    string ConceptId,
    string CanonicalTerm,
    CompiledRulesRetrievalTerm RetrievalTerm,
    IReadOnlyList<RuleVocabularyOriginEvidence> Origins);

public sealed record RuleSnippetVocabulary(
    RuleSnippetCandidate Candidate,
    IReadOnlyList<RuleVocabularyAssociation> Associations);

public sealed record RuleVocabularyEnrichment(
    string ManifestPath,
    string ManifestSha256,
    IReadOnlyList<RuleSnippetVocabulary> Snippets);

public sealed class RuleVocabularyEnrichmentException(
    string code,
    string path,
    string message,
    Exception? innerException = null) : Exception(message, innerException)
{
    public string Code { get; } = code;

    public string Path { get; } = path;
}

public static class RuleVocabularyEnricher
{
    /// <summary>
    /// Resolves reviewed bindings against a validated snapshot's complete candidate sequence.
    /// Returns intermediate evidence without rereading sources or modifying candidates.
    /// </summary>
    public static RuleVocabularyEnrichment Enrich(
        MaterializedRuleSourceSnapshot snapshot,
        IReadOnlyList<RuleSnippetCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(candidates);

        try
        {
            return EnrichValidated(snapshot, candidates);
        }
        catch (MaterializedRuleSourceException exception)
        {
            throw new RuleVocabularyEnrichmentException(
                exception.Code, exception.Path, exception.Message, exception);
        }
    }

    private static RuleVocabularyEnrichment EnrichValidated(
        MaterializedRuleSourceSnapshot snapshot,
        IReadOnlyList<RuleSnippetCandidate> candidates)
    {
        if (snapshot.Manifest.RetrievalVocabulary is null)
        {
            return Empty(snapshot, candidates);
        }

        var sources = snapshot.Sources.ToDictionary(
            source => source.ManifestEntry.RuleSourceId, StringComparer.Ordinal);
        // Public input models remain mutable. Reuse A's checks and normalization instead
        // of trusting a previously loaded definition that a caller may have edited.
        var vocabulary = RuleRetrievalVocabulary.NormalizeAndValidate(
            snapshot.Manifest.RetrievalVocabulary,
            sources.Keys.ToHashSet(StringComparer.Ordinal));
        if (vocabulary.Bindings.Count == 0)
        {
            return Empty(snapshot, candidates);
        }

        var bySnippet = new Dictionary<string, int>(StringComparer.Ordinal);
        var bySource = new Dictionary<string, List<int>>(StringComparer.Ordinal);
        for (var index = 0; index < candidates.Count; index++)
        {
            var candidate = candidates[index];
            var path = $"$.candidates[{index}]";
            if (candidate is null)
            {
                Fail("VOCABULARY_CANDIDATE_INVALID", path, "A snippet candidate cannot be null.");
            }
            MaterializedRuleSourceLoader.RequiredIdentifier(candidate.RuleSourceId, $"{path}.ruleSourceId");
            if (candidate.SourceAnchor is not null)
            {
                MaterializedRuleSourceLoader.RequiredIdentifier(candidate.SourceAnchor, $"{path}.sourceAnchor");
            }
            if (!sources.TryGetValue(candidate.RuleSourceId, out var source))
            {
                Fail("VOCABULARY_CANDIDATE_SOURCE_MISSING", path, "The candidate references an undeclared materialized source.");
            }
            if (!string.Equals(candidate.SnippetId,
                    CompiledRulesArtifactContract.CreateSnippetId(candidate.RuleSourceId, candidate.SourceAnchor),
                    StringComparison.Ordinal) ||
                !string.Equals(candidate.SourcePath, source.ManifestEntry.Path, StringComparison.Ordinal) ||
                !string.Equals(candidate.SourceSha256, source.SourceSha256, StringComparison.Ordinal))
            {
                Fail("VOCABULARY_CANDIDATE_INCONSISTENT", path, "Candidate identity and source provenance must match the supplied snapshot.");
            }
            if (!bySnippet.TryAdd(candidate.SnippetId, index))
            {
                Fail("VOCABULARY_TARGET_AMBIGUOUS", path, "A source/anchor target resolves to more than one candidate.");
            }
            if (!bySource.TryGetValue(candidate.RuleSourceId, out var sourceCandidates))
            {
                sourceCandidates = [];
                bySource.Add(candidate.RuleSourceId, sourceCandidates);
            }
            sourceCandidates.Add(index);
        }

        var concepts = vocabulary.Concepts.ToDictionary(concept => concept.ConceptId, StringComparer.Ordinal);
        var associations = new Dictionary<(string ConceptId, string Term, string Kind), AssociationBuilder>?[candidates.Count];
        for (var bindingIndex = 0; bindingIndex < vocabulary.Bindings.Count; bindingIndex++)
        {
            var binding = vocabulary.Bindings[bindingIndex];
            var path = $"$.retrievalVocabulary.bindings[{bindingIndex}]";
            var identity = new RuleVocabularyBindingIdentity(binding.RuleSourceId, binding.Scope, binding.SourceAnchor);
            if (binding.Scope == RuleVocabularyBindingScope.Source)
            {
                if (!bySource.TryGetValue(binding.RuleSourceId, out var targets))
                {
                    Fail("VOCABULARY_TARGET_MISSING", path, "The source-wide binding has no compiled candidates.");
                }
                foreach (var target in targets)
                {
                    AddBinding(target, binding, identity);
                }
            }
            else
            {
                var id = CompiledRulesArtifactContract.CreateSnippetId(binding.RuleSourceId, binding.SourceAnchor);
                if (!bySnippet.TryGetValue(id, out var target))
                {
                    Fail("VOCABULARY_TARGET_MISSING", path, "The exact source/anchor binding has no compiled candidate.");
                }
                AddBinding(target, binding, identity);
            }
        }

        var snippets = new RuleSnippetVocabulary[candidates.Count];
        for (var index = 0; index < candidates.Count; index++)
        {
            var effective = associations[index];
            var ordered = effective is null
                ? []
                : effective.Values.OrderBy(item => item.Term.Term, StringComparer.Ordinal)
                    .ThenBy(item => item.Term.Kind, StringComparer.Ordinal)
                    .ThenBy(item => item.Concept.ConceptId, StringComparer.Ordinal)
                    .Select(item => new RuleVocabularyAssociation(
                        item.Concept.ConceptId, item.Concept.CanonicalTerm, item.Term, item.Origins.ToArray()))
                    .ToArray();
            snippets[index] = new(candidates[index], ordered);
        }
        return new(snapshot.ManifestPath, snapshot.ManifestSha256, snippets);

        void AddBinding(int target, RuleVocabularyBinding binding, RuleVocabularyBindingIdentity identity)
        {
            foreach (var conceptId in binding.ConceptIds)
            {
                var concept = concepts[conceptId];
                AddTerm(concept.CanonicalTerm, "canonical", concept.Weight, concept.Rationale);
                foreach (var alternative in concept.Alternatives)
                {
                    var kind = alternative.Kind == RuleVocabularyAlternativeKind.Alias ? "alias" : "phrase";
                    AddTerm(alternative.Term, kind, alternative.Weight, alternative.Rationale);
                }

                void AddTerm(string term, string kind, int weight, string rationale)
                {
                    var effective = associations[target] ??= [];
                    var key = (concept.ConceptId, term, kind);
                    if (!effective.TryGetValue(key, out var association))
                    {
                        association = new(concept, new() { Term = term, Kind = kind, Weight = weight });
                        effective.Add(key, association);
                    }
                    // A forbids duplicate binding identities and within-concept terms.
                    // Overlapping Source/Snippet scopes share an association, not its origins.
                    association.Origins.Add(new(identity, binding.Rationale, concept.Rationale, rationale));
                }
            }
        }
    }

    private static RuleVocabularyEnrichment Empty(
        MaterializedRuleSourceSnapshot snapshot,
        IReadOnlyList<RuleSnippetCandidate> candidates) =>
        new(snapshot.ManifestPath, snapshot.ManifestSha256,
            candidates.Select(candidate => new RuleSnippetVocabulary(candidate, [])).ToArray());

    private sealed class AssociationBuilder(RuleVocabularyConcept concept, CompiledRulesRetrievalTerm term)
    {
        public RuleVocabularyConcept Concept { get; } = concept;
        public CompiledRulesRetrievalTerm Term { get; } = term;
        public List<RuleVocabularyOriginEvidence> Origins { get; } = [];
    }

    [DoesNotReturn]
    private static void Fail(string code, string path, string message) =>
        throw new RuleVocabularyEnrichmentException(code, path, message);
}
