namespace EternalCycle.Rules;

public sealed record RuleCompilationResult(
    CompiledRulesArtifact? Artifact,
    RuleVocabularyEnrichment Vocabulary,
    ReadOnlyMemory<byte> Bytes,
    IReadOnlyList<CompiledRulesArtifactValidationError> Errors)
{
    public bool IsValid => Artifact is not null && Errors.Count == 0;
}

public static class RuleCompilationPipeline
{
    /// <summary>
    /// Compiles a validated materialized snapshot, retaining reviewed evidence separately
    /// from the validated format-1 artifact and its canonical bytes.
    /// </summary>
    public static RuleCompilationResult Compile(MaterializedRuleSourceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        var candidates = RuleSnippetCompiler.Compile(snapshot);
        var vocabulary = RuleVocabularyEnricher.Enrich(snapshot, candidates);
        var projected = new RuleSnippetCandidate[candidates.Count];
        for (var index = 0; index < candidates.Count; index++)
        {
            var associations = vocabulary.Snippets[index].Associations;
            projected[index] = associations.Count == 0 ? candidates[index] : candidates[index] with
            {
                Retrieval = new()
                {
                    // Format 1 keys terms by text/category. Using the reviewed concept ID
                    // as category preserves shared terms and their distinct weights without
                    // invented aggregation. Canonical/Alias/Phrase origin stays in evidence.
                    Terms = associations.Select(association => new CompiledRulesRetrievalTerm
                    {
                        Term = association.RetrievalTerm.Term,
                        Kind = association.ConceptId,
                        Weight = association.RetrievalTerm.Weight
                    }).ToArray()
                }
            };
        }

        var assembly = CompiledRulesArtifactAssembler.Assemble(snapshot, projected);
        if (!assembly.IsValid)
        {
            return new(null, vocabulary, ReadOnlyMemory<byte>.Empty, assembly.Errors);
        }
        var write = CompiledRulesArtifactWriter.Write(assembly.Artifact!);
        return write.IsValid
            ? new(assembly.Artifact, vocabulary, write.Bytes, write.Errors)
            : new(null, vocabulary, ReadOnlyMemory<byte>.Empty, write.Errors);
    }
}
