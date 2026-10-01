namespace EternalCycle.Rules;

public static class RuleVocabularyAuditor
{
    public static RuleVocabularyAuditReport Analyze(
        MaterializedRuleSourceSnapshot snapshot, RuleCompilationResult compilation,
        RuleVocabularyAuditPolicy? policy = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(compilation);
        var selectedPolicy = (policy ?? new()).Normalize();
        var findings = new List<RuleVocabularyAuditFinding>();
        RuleRetrievalVocabularyDefinition definition;
        try
        {
            definition = snapshot.Manifest.RetrievalVocabulary is null
                ? new() { VocabularyFormatVersion = 1 }
                : RuleRetrievalVocabulary.NormalizeAndValidate(snapshot.Manifest.RetrievalVocabulary,
                    snapshot.Sources.Select(source => source.ManifestEntry.RuleSourceId).ToHashSet(StringComparer.Ordinal));
            if (!compilation.IsValid || CompiledRulesArtifactContract.Validate(compilation.Artifact!).Count != 0)
            {
                return Invalid("VOCABULARY_AUDIT_COMPILATION_INVALID", "A successfully validated compilation is required.");
            }
            if (!ProvenanceMatches(snapshot, compilation))
            {
                return Invalid("VOCABULARY_AUDIT_PROVENANCE_MISMATCH", "Snapshot, artifact, and evidence provenance must agree.");
            }
            // Public compilation models are mutable. Reuse B to verify reviewed origins;
            // this reads no files, recompiles no text, and never alters supplied evidence.
            var expected = RuleVocabularyEnricher.Enrich(snapshot,
                compilation.Vocabulary.Snippets.Select(snippet => snippet.Candidate).ToArray());
            if (!EvidenceMatches(expected, compilation.Vocabulary) || !ArtifactMatches(compilation))
            {
                return Invalid("VOCABULARY_AUDIT_EVIDENCE_MISMATCH", "Compiled associations and origins must match reviewed definitions and artifact metadata.");
            }
        }
        catch (MaterializedRuleSourceException error) { return Invalid(error.Code, "Reviewed vocabulary violates its input contract."); }
        catch (RuleVocabularyEnrichmentException error) { return Invalid(error.Code, "Reviewed vocabulary cannot resolve against compilation evidence."); }

        var associations = compilation.Vocabulary.Snippets.SelectMany(snippet => snippet.Associations.Select(item =>
            new RuleVocabularyAuditAssociation(snippet.Candidate.SnippetId, item.ConceptId, item.RetrievalTerm.Term,
                item.RetrievalTerm.Kind, item.RetrievalTerm.Weight,
                item.Origins.Select(origin => origin.Binding).OrderBy(BindingKey, StringComparer.Ordinal).ToArray())))
            .OrderBy(item => item.SnippetId, StringComparer.Ordinal).ThenBy(item => item.ConceptId, StringComparer.Ordinal)
            .ThenBy(item => item.Term, StringComparer.Ordinal).ThenBy(item => item.Kind, StringComparer.Ordinal).ToArray();
        var bySnippet = associations.ToLookup(item => item.SnippetId, StringComparer.Ordinal);
        var byConcept = associations.ToLookup(item => item.ConceptId, StringComparer.Ordinal);
        var byTermConcept = associations.ToLookup(item => (item.Term, item.ConceptId));
        var sourceBySnippet = compilation.Artifact!.Snippets.ToDictionary(snippet => snippet.SnippetId, snippet => snippet.RuleSourceId, StringComparer.Ordinal);
        var generic = selectedPolicy.GenericTerms.ToHashSet(StringComparer.Ordinal);
        var snippetCount = compilation.Artifact.Snippets.Count;
        var snippets = compilation.Artifact.Snippets.Select(snippet =>
        {
            var items = bySnippet[snippet.SnippetId].ToArray();
            var inherited = items.Count(item => item.Origins.All(origin => origin.Scope == RuleVocabularyBindingScope.Source));
            if (inherited > 0) { Info("VOCABULARY_INHERITED_ONLY", "snippet", snippet.SnippetId, "Some associations have only explicit source-wide support."); }
            if (items.Any(item => item.Origins.Count > 1)) { Info("VOCABULARY_MULTIPLE_ORIGINS", "snippet", snippet.SnippetId, "Distinct reviewed origins support coalesced associations; none were discarded."); }
            if (snippet.EstimatedTokens >= selectedPolicy.LargeSnippetTokens) { Info("VOCABULARY_LARGE_SNIPPET", "snippet", snippet.SnippetId, "Token estimate reaches the review threshold; inspect the boundary without inferring prose meaning."); }
            return new RuleVocabularyAuditSnippet(snippet.SnippetId, snippet.RuleSourceId, items.Length,
                items.Sum(item => item.Origins.Count), inherited, snippet.EstimatedTokens);
        }).OrderBy(snippet => snippet.SnippetId, StringComparer.Ordinal).ToArray();

        var declaredTerms = definition.Concepts.SelectMany(concept =>
            new[] { (Term: concept.CanonicalTerm, ConceptId: concept.ConceptId, Kind: "canonical", Weight: concept.Weight) }
                .Concat(concept.Alternatives.Select(term => (term.Term, concept.ConceptId,
                    Kind: term.Kind == RuleVocabularyAlternativeKind.Alias ? "alias" : "phrase", term.Weight))));
        var terms = declaredTerms.GroupBy(term => term.Term, StringComparer.Ordinal).OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group =>
            {
                var targets = group.OrderBy(item => item.ConceptId, StringComparer.Ordinal).ThenBy(item => item.Kind, StringComparer.Ordinal)
                    .Select(item => new RuleVocabularyAuditTermTarget(item.ConceptId, item.Kind, item.Weight,
                        Sorted(byTermConcept[(item.Term, item.ConceptId)].Select(item => item.SnippetId)))).ToArray();
                var ids = Sorted(targets.SelectMany(target => target.SnippetIds));
                var sources = Sorted(ids.Select(id => sourceBySnippet[id]));
                var different = targets.Skip(1).Any(target => !target.SnippetIds.SequenceEqual(targets[0].SnippetIds, StringComparer.Ordinal));
                if (targets.Length > 1) { Info("VOCABULARY_AMBIGUOUS_TERM", "term", group.Key, "Reviewed wording belongs to multiple concepts; no winner is selected."); }
                if (different) { Info("VOCABULARY_DIFFERENT_TARGET_SETS", "term", group.Key, "The shared term has distinct concept target sets."); }
                if (sources.Length > 1) { Info("VOCABULARY_MULTI_SOURCE_TERM", "term", group.Key, "Reviewed wording spans multiple Rule Sources."); }
                if (Broad(ids.Length)) { Warn("VOCABULARY_BROAD_TERM", "term", group.Key, "Target count and corpus share reach the configured breadth thresholds."); }
                if (generic.Contains(group.Key)) { Warn("VOCABULARY_GENERIC_TERM", "term", group.Key, "The review policy explicitly marks this wording as potentially generic."); }
                return new RuleVocabularyAuditTerm(group.Key, targets, ids, sources, Sorted(targets.Select(target => target.Kind)), new(ids.Length, snippetCount), different);
            }).ToArray();
        var shared = terms.Where(term => term.Targets.Count > 1).Select(term => term.Term).ToHashSet(StringComparer.Ordinal);
        var concepts = definition.Concepts.Select(concept =>
        {
            var aliases = concept.Alternatives.Count(term => term.Kind == RuleVocabularyAlternativeKind.Alias);
            var phrases = concept.Alternatives.Count - aliases;
            var items = byConcept[concept.ConceptId].ToArray();
            var ids = Sorted(items.Select(item => item.SnippetId));
            var bindingCount = definition.Bindings.Count(binding => binding.ConceptIds.Contains(concept.ConceptId, StringComparer.Ordinal));
            if (aliases == 0) { Info("VOCABULARY_NO_ALIASES", "concept", concept.ConceptId, "No aliases are declared; aliases are not mandatory."); }
            if (ids.Length == 0) { Warn("VOCABULARY_UNUSED_CONCEPT", "concept", concept.ConceptId, "The reviewed concept produces no effective targets."); }
            if (ids.Length == snippetCount) { Info("VOCABULARY_CONCEPT_EVERYWHERE", "concept", concept.ConceptId, "This concept covers every compiled snippet."); }
            if (generic.Contains(concept.CanonicalTerm) && concept.Alternatives.All(term => generic.Contains(term.Term)))
            {
                Warn("VOCABULARY_ONLY_GENERIC_TERMS", "concept", concept.ConceptId, "All reviewed wording is marked generic by the explicit policy.");
            }
            return new RuleVocabularyAuditConcept(concept.ConceptId, concept.CanonicalTerm, aliases, phrases, bindingCount,
                items.Length, ids, Sorted(ids.Select(id => sourceBySnippet[id])),
                Sorted(concept.Alternatives.Select(item => item.Term).Append(concept.CanonicalTerm).Where(shared.Contains)));
        }).ToArray();

        var byBinding = associations.SelectMany(item => item.Origins.Select(identity => (Identity: identity, Association: item)))
            .ToLookup(item => item.Identity, item => item.Association);
        var bindings = definition.Bindings.Select(binding =>
        {
            var identity = new RuleVocabularyBindingIdentity(binding.RuleSourceId, binding.Scope, binding.SourceAnchor);
            var items = byBinding[identity].ToArray();
            var ids = Sorted(items.Select(item => item.SnippetId));
            var unique = items.Count(item => item.Origins.Count == 1);
            if (binding.Scope == RuleVocabularyBindingScope.Source) { Info("VOCABULARY_SOURCE_BINDING", "binding", BindingKey(identity), "Source-wide inheritance is explicit; heterogeneity requires human review."); }
            if (Broad(ids.Length)) { Warn("VOCABULARY_BINDING_FAN_OUT", "binding", BindingKey(identity), "Binding target count and corpus share reach the configured breadth thresholds."); }
            if (unique == 0) { Info("VOCABULARY_BINDING_NO_UNIQUE_ASSOCIATIONS", "binding", BindingKey(identity), "This binding adds no exclusive association, but its reviewed provenance is retained."); }
            return new RuleVocabularyAuditBinding(identity, binding.ConceptIds.ToArray(), ids, items.Length, unique);
        }).ToArray();
        var covered = snippets.Count(snippet => snippet.AssociationCount > 0);
        if (covered < snippetCount) { Info(covered == 0 ? "VOCABULARY_NO_COVERAGE" : "VOCABULARY_PARTIAL_COVERAGE", "corpus", "vocabulary", "Uncovered snippets are a measurement; absence does not establish intent or a defect."); }

        var metrics = new RuleVocabularyAuditMetrics
        {
            SourceCount = compilation.Artifact.RuleSources.Count, SnippetCount = snippetCount,
            ConceptCount = concepts.Length, CanonicalTermCount = concepts.Length,
            AliasCount = concepts.Sum(concept => concept.AliasCount), PhraseCount = concepts.Sum(concept => concept.PhraseCount),
            BindingCount = bindings.Length, EffectiveAssociationCount = associations.Length,
            RetainedOriginCount = associations.Sum(item => item.Origins.Count), CoveredSnippetCount = covered,
            UncoveredSnippetCount = snippetCount - covered, ConceptsWithAliases = concepts.Count(concept => concept.AliasCount > 0),
            ConceptsWithoutAliases = concepts.Count(concept => concept.AliasCount == 0),
            ConceptsWithTargets = concepts.Count(concept => concept.SnippetIds.Count > 0), ConceptsWithoutTargets = concepts.Count(concept => concept.SnippetIds.Count == 0),
            UniqueTermCount = terms.Length, MultiOriginAssociationCount = associations.Count(item => item.Origins.Count > 1),
            SourceOnlyAssociationCount = snippets.Sum(snippet => snippet.SourceOnlyAssociationCount),
            AverageAssociationsPerCoveredSnippet = new(associations.Length, covered), MaximumAssociationsPerSnippet = snippets.Max(snippet => snippet.AssociationCount),
            AverageTargetSnippetsPerTerm = new(terms.Sum(term => term.SnippetIds.Count), terms.Length), MaximumTargetSnippetsPerTerm = terms.Length == 0 ? 0 : terms.Max(term => term.SnippetIds.Count)
        };
        return Report(metrics, snippets, concepts, terms, bindings, associations);

        bool Broad(int targets) => targets >= selectedPolicy.MinimumBroadTargets &&
            (long)targets * 100 >= (long)snippetCount * selectedPolicy.MinimumBroadCoveragePercent;
        void Info(string code, string kind, string id, string message) => findings.Add(new(code, RuleVocabularyAuditSeverity.Information, kind, id, message));
        void Warn(string code, string kind, string id, string message) => findings.Add(new(code, RuleVocabularyAuditSeverity.Warning, kind, id, message));
        RuleVocabularyAuditReport Invalid(string code, string message)
        {
            findings.Add(new(code, RuleVocabularyAuditSeverity.Error, "input", "compilation", message));
            return Report(new(), [], [], [], [], []);
        }
        RuleVocabularyAuditReport Report(RuleVocabularyAuditMetrics metrics, IReadOnlyList<RuleVocabularyAuditSnippet> snippetRows,
            IReadOnlyList<RuleVocabularyAuditConcept> conceptRows, IReadOnlyList<RuleVocabularyAuditTerm> termRows,
            IReadOnlyList<RuleVocabularyAuditBinding> bindingRows, IReadOnlyList<RuleVocabularyAuditAssociation> associationRows) =>
            new(1, compilation.Artifact?.Integrity.ArtifactSha256, snapshot.ManifestSha256, selectedPolicy,
                metrics with { ErrorCount = findings.Count(item => item.Severity == RuleVocabularyAuditSeverity.Error),
                    WarningCount = findings.Count(item => item.Severity == RuleVocabularyAuditSeverity.Warning),
                    InformationCount = findings.Count(item => item.Severity == RuleVocabularyAuditSeverity.Information) },
                snippetRows, conceptRows, termRows, bindingRows, associationRows,
                findings.OrderBy(item => item.Severity).ThenBy(item => item.Code, StringComparer.Ordinal)
                    .ThenBy(item => item.SubjectKind, StringComparer.Ordinal).ThenBy(item => item.SubjectId, StringComparer.Ordinal).ToArray());
    }

    private static string[] Sorted(IEnumerable<string> values) => values.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();

    // Identifiers cannot contain '/', so this composite key is unambiguous even for
    // null anchors. It identifies the reviewed binding, not its array position.
    private static string BindingKey(RuleVocabularyBindingIdentity binding) => $"{binding.RuleSourceId}/{binding.Scope}/{binding.SourceAnchor}";

    private static bool ProvenanceMatches(MaterializedRuleSourceSnapshot snapshot, RuleCompilationResult result) =>
        result.Vocabulary.ManifestSha256 == snapshot.ManifestSha256 && result.Vocabulary.ManifestPath == snapshot.ManifestPath &&
        result.Artifact!.Ruleset.ManifestSha256 == snapshot.ManifestSha256 && result.Artifact.Ruleset.ManifestPath == snapshot.ManifestPath &&
        result.Artifact.Ruleset.RulesetId == snapshot.Manifest.RulesetId && result.Artifact.Ruleset.RepositoryVersion == snapshot.Manifest.RepositoryVersion &&
        result.Artifact.Ruleset.Source.Scheme == snapshot.SourceIdentity.Scheme && result.Artifact.Ruleset.Source.Value == snapshot.SourceIdentity.Value &&
        result.Artifact.Compiler.ContractVersion == snapshot.CompilerIdentity.ContractVersion &&
        result.Artifact.Compiler.ImplementationId == snapshot.CompilerIdentity.ImplementationId &&
        result.Artifact.Compiler.ImplementationVersion == snapshot.CompilerIdentity.ImplementationVersion &&
        result.Artifact.RuleSources.Count == snapshot.Sources.Count && snapshot.Sources.All(source =>
            result.Artifact.RuleSources.Any(item => item.RuleSourceId == source.ManifestEntry.RuleSourceId &&
                item.SourceSha256 == source.SourceSha256 && item.SourcePath == source.ManifestEntry.Path));

    private static bool EvidenceMatches(RuleVocabularyEnrichment expected, RuleVocabularyEnrichment actual)
    {
        if (expected.Snippets.Count != actual.Snippets.Count) { return false; }
        for (var index = 0; index < expected.Snippets.Count; index++)
        {
            var wanted = expected.Snippets[index].Associations;
            var supplied = actual.Snippets[index].Associations.OrderBy(item => item.RetrievalTerm.Term, StringComparer.Ordinal)
                .ThenBy(item => item.RetrievalTerm.Kind, StringComparer.Ordinal).ThenBy(item => item.ConceptId, StringComparer.Ordinal).ToArray();
            if (wanted.Count != supplied.Length) { return false; }
            for (var item = 0; item < wanted.Count; item++)
            {
                var left = wanted[item];
                var right = supplied[item];
                if (left.ConceptId != right.ConceptId || left.CanonicalTerm != right.CanonicalTerm ||
                    left.RetrievalTerm.Term != right.RetrievalTerm.Term || left.RetrievalTerm.Kind != right.RetrievalTerm.Kind ||
                    left.RetrievalTerm.Weight != right.RetrievalTerm.Weight ||
                    !left.Origins.OrderBy(origin => BindingKey(origin.Binding), StringComparer.Ordinal)
                        .SequenceEqual(right.Origins.OrderBy(origin => BindingKey(origin.Binding), StringComparer.Ordinal))) { return false; }
            }
        }
        return true;
    }

    private static bool ArtifactMatches(RuleCompilationResult result)
    {
        if (result.Artifact!.Snippets.Count != result.Vocabulary.Snippets.Count) { return false; }
        var artifact = result.Artifact.Snippets.ToDictionary(snippet => snippet.SnippetId, StringComparer.Ordinal);
        var sources = result.Artifact.RuleSources.ToDictionary(source => source.RuleSourceId, StringComparer.Ordinal);
        foreach (var evidence in result.Vocabulary.Snippets)
        {
            var candidate = evidence.Candidate;
            if (!artifact.Remove(candidate.SnippetId, out var snippet) || snippet.RuleSourceId != candidate.RuleSourceId ||
                snippet.SourceAnchor != candidate.SourceAnchor || snippet.EstimatedTokens != candidate.EstimatedTokens ||
                sources[snippet.RuleSourceId].SourceSha256 != candidate.SourceSha256 || sources[snippet.RuleSourceId].SourcePath != candidate.SourcePath ||
                snippet.Content != candidate.Content || snippet.ContentSha256 != candidate.ContentSha256 ||
                snippet.Retrieval.Terms.Count != evidence.Associations.Count) { return false; }
            var terms = snippet.Retrieval.Terms.ToDictionary(term => (term.Kind, term.Term));
            if (evidence.Associations.Any(item => !terms.TryGetValue((item.ConceptId, item.RetrievalTerm.Term), out var term) ||
                term.Weight != item.RetrievalTerm.Weight)) { return false; }
        }
        return true;
    }
}
