namespace EternalCycle.Rules;

public sealed record CompiledRulesArtifactAssemblyResult(
    CompiledRulesArtifact? Artifact,
    IReadOnlyList<CompiledRulesArtifactValidationError> Errors)
{
    public bool IsValid => Artifact is not null && Errors.Count == 0;
}

public static class CompiledRulesArtifactAssembler
{
    public static CompiledRulesArtifactAssemblyResult Assemble(
        MaterializedRuleSourceSnapshot snapshot,
        IReadOnlyList<RuleSnippetCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(candidates);

        var candidateErrors = ValidateCandidates(snapshot, candidates);
        if (candidateErrors.Count != 0)
        {
            return new(null, candidateErrors);
        }

        var ruleSources = snapshot.Sources
            .Select(source => new CompiledRulesArtifactRuleSource
            {
                RuleSourceId = source.ManifestEntry.RuleSourceId,
                SourcePath = source.ManifestEntry.Path,
                SourceSha256 = source.SourceSha256,
                Applicability = Applicability(source.ManifestEntry),
                DependencyRuleSourceIds = Ordered(source.ManifestEntry.Dependencies)
            })
            .OrderBy(source => source.RuleSourceId, StringComparer.Ordinal)
            .ToList();
        var snippets = candidates
            .Select(candidate => new CompiledRulesArtifactSnippet
            {
                SnippetId = candidate.SnippetId,
                RuleSourceId = candidate.RuleSourceId,
                SourceAnchor = candidate.SourceAnchor,
                Content = candidate.Content,
                ContentSha256 = candidate.ContentSha256,
                EstimatedTokens = candidate.EstimatedTokens,
                Retrieval = Retrieval(candidate.Retrieval)
            })
            .OrderBy(snippet => snippet.SnippetId, StringComparer.Ordinal)
            .ToList();

        var artifactWithoutDigest = Artifact(snapshot, ruleSources, snippets, new()
        {
            Algorithm = CompiledRulesArtifactContract.IntegrityAlgorithm,
            ArtifactSha256 = new string('0', 64)
        });
        var artifact = Artifact(snapshot, ruleSources, snippets, new()
        {
            Algorithm = CompiledRulesArtifactContract.IntegrityAlgorithm,
            ArtifactSha256 = CompiledRulesArtifactContract.ComputeArtifactSha256(artifactWithoutDigest)
        });
        var errors = CompiledRulesArtifactContract.Validate(artifact);
        return errors.Count == 0
            ? new(artifact, errors)
            : new(null, errors);
    }

    private static List<CompiledRulesArtifactValidationError> ValidateCandidates(
        MaterializedRuleSourceSnapshot snapshot,
        IReadOnlyList<RuleSnippetCandidate> candidates)
    {
        IReadOnlyList<RuleSnippetCandidate> expected;
        try
        {
            expected = RuleSnippetCompiler.Compile(snapshot);
        }
        catch (RuleSnippetCompilationException exception)
        {
            return
            [
                new(
                    exception.Code,
                    $"$.ruleSources[{exception.RuleSourceId}]",
                    exception.Message)
            ];
        }

        var errors = new List<CompiledRulesArtifactValidationError>();
        var expectedById = expected.ToDictionary(candidate => candidate.SnippetId, StringComparer.Ordinal);
        var actualIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < candidates.Count; index++)
        {
            var candidate = candidates[index];
            var path = $"$.candidates[{index}]";
            if (candidate is null)
            {
                errors.Add(new("CANDIDATE_REQUIRED", path, "A compiled snippet candidate cannot be null."));
                continue;
            }
            if (string.IsNullOrEmpty(candidate.SnippetId) ||
                candidate.Applicability is null ||
                candidate.Applicability.WorldModelIds is null ||
                candidate.Applicability.ModuleIds is null ||
                candidate.Applicability.CampaignModes is null ||
                candidate.Applicability.Operations is null ||
                candidate.Applicability.Topics is null ||
                candidate.DependencyRuleSourceIds is null ||
                candidate.Retrieval is null ||
                candidate.Retrieval.Terms is null ||
                candidate.Retrieval.Relationships is null ||
                candidate.Retrieval.Terms.Any(term => term is null) ||
                candidate.Retrieval.Relationships.Any(relationship => relationship is null))
            {
                errors.Add(new("CANDIDATE_STRUCTURE_INVALID", path, "Candidate identity, applicability, dependencies, and retrieval collections are required."));
                continue;
            }
            if (!actualIds.Add(candidate.SnippetId))
            {
                errors.Add(new("CANDIDATE_DUPLICATE", $"{path}.snippetId", "Compiled snippet candidate IDs must be unique."));
                continue;
            }
            if (!expectedById.TryGetValue(candidate.SnippetId, out var expectedCandidate))
            {
                errors.Add(new("CANDIDATE_UNEXPECTED", $"{path}.snippetId", "The candidate was not produced by the supplied materialized Rule Source snapshot."));
                continue;
            }
            if (!Equivalent(candidate, expectedCandidate))
            {
                errors.Add(new("CANDIDATE_MISMATCH", path, "The candidate does not match the deterministic FR-023C output for the supplied snapshot."));
            }
        }

        foreach (var missing in expectedById.Keys.Except(actualIds, StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            errors.Add(new("CANDIDATE_MISSING", "$.candidates", $"Required compiled snippet candidate '{missing}' is missing."));
        }
        return errors;
    }

    private static bool Equivalent(RuleSnippetCandidate candidate, RuleSnippetCandidate expected) =>
        string.Equals(candidate.SnippetId, expected.SnippetId, StringComparison.Ordinal) &&
        string.Equals(candidate.RuleSourceId, expected.RuleSourceId, StringComparison.Ordinal) &&
        string.Equals(candidate.SourcePath, expected.SourcePath, StringComparison.Ordinal) &&
        string.Equals(candidate.SourceSha256, expected.SourceSha256, StringComparison.Ordinal) &&
        string.Equals(candidate.SourceAnchor, expected.SourceAnchor, StringComparison.Ordinal) &&
        string.Equals(candidate.Content, expected.Content, StringComparison.Ordinal) &&
        string.Equals(candidate.ContentSha256, expected.ContentSha256, StringComparison.Ordinal) &&
        candidate.EstimatedTokens == expected.EstimatedTokens &&
        candidate.SourceOrder == expected.SourceOrder &&
        candidate.SectionOrder == expected.SectionOrder &&
        ApplicabilityEquivalent(candidate.Applicability, expected.Applicability) &&
        candidate.DependencyRuleSourceIds.SequenceEqual(expected.DependencyRuleSourceIds, StringComparer.Ordinal);

    private static bool ApplicabilityEquivalent(
        RuleSnippetApplicability candidate,
        RuleSnippetApplicability expected) =>
        candidate.Layer == expected.Layer &&
        candidate.WorldModelIds.SequenceEqual(expected.WorldModelIds, StringComparer.Ordinal) &&
        candidate.ModuleIds.SequenceEqual(expected.ModuleIds, StringComparer.Ordinal) &&
        candidate.CampaignModes.SequenceEqual(expected.CampaignModes, StringComparer.Ordinal) &&
        candidate.Operations.SequenceEqual(expected.Operations, StringComparer.Ordinal) &&
        candidate.Topics.SequenceEqual(expected.Topics, StringComparer.Ordinal) &&
        candidate.Priority == expected.Priority &&
        candidate.AlwaysInclude == expected.AlwaysInclude &&
        candidate.PreparationTier == expected.PreparationTier;

    private static CompiledRulesArtifact Artifact(
        MaterializedRuleSourceSnapshot snapshot,
        IList<CompiledRulesArtifactRuleSource> ruleSources,
        IList<CompiledRulesArtifactSnippet> snippets,
        CompiledRulesArtifactIntegrity integrity) => new()
    {
        ArtifactFormatVersion = CompiledRulesArtifactContract.CurrentArtifactFormatVersion,
        Compiler = new()
        {
            ContractVersion = snapshot.CompilerIdentity.ContractVersion,
            ImplementationId = snapshot.CompilerIdentity.ImplementationId,
            ImplementationVersion = snapshot.CompilerIdentity.ImplementationVersion
        },
        Ruleset = new()
        {
            RulesetId = snapshot.Manifest.RulesetId,
            RepositoryVersion = snapshot.Manifest.RepositoryVersion,
            Source = new()
            {
                Scheme = snapshot.SourceIdentity.Scheme,
                Value = snapshot.SourceIdentity.Value
            },
            ManifestPath = snapshot.ManifestPath,
            ManifestSha256 = snapshot.ManifestSha256
        },
        RuleSources = ruleSources,
        Snippets = snippets,
        Integrity = integrity
    };

    private static CompiledRulesArtifactApplicability Applicability(RuleSourceManifestEntry source) => new()
    {
        Layer = source.Layer,
        WorldModelIds = Ordered(source.WorldModelIds),
        ModuleIds = Ordered(source.ModuleIds),
        CampaignModes = Ordered(source.CampaignModes),
        Operations = Ordered(source.Operations),
        Topics = Ordered(source.Topics),
        Priority = source.Priority,
        AlwaysInclude = source.AlwaysInclude,
        PreparationTier = source.PreparationTier
    };

    private static CompiledRulesRetrievalMetadata Retrieval(CompiledRulesRetrievalMetadata retrieval) => new()
    {
        Terms = retrieval.Terms
            .Select(term => new CompiledRulesRetrievalTerm
            {
                Term = term.Term,
                Kind = term.Kind,
                Weight = term.Weight
            })
            .OrderBy(term => term.Term, StringComparer.Ordinal)
            .ThenBy(term => term.Kind, StringComparer.Ordinal)
            .ToList(),
        Relationships = retrieval.Relationships
            .Select(relationship => new CompiledRulesRetrievalRelationship
            {
                FromTerm = relationship.FromTerm,
                ToTerm = relationship.ToTerm,
                Kind = relationship.Kind,
                Weight = relationship.Weight
            })
            .OrderBy(relationship => relationship.FromTerm, StringComparer.Ordinal)
            .ThenBy(relationship => relationship.ToTerm, StringComparer.Ordinal)
            .ThenBy(relationship => relationship.Kind, StringComparer.Ordinal)
            .ToList()
    };

    private static List<string> Ordered(IEnumerable<string> values) =>
        values.Order(StringComparer.Ordinal).ToList();
}
