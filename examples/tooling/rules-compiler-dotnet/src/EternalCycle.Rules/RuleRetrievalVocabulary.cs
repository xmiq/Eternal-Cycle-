using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;

namespace EternalCycle.Rules;

public enum RuleVocabularyAlternativeKind
{
    Alias,
    Phrase
}

public enum RuleVocabularyBindingScope
{
    Snippet,
    Source
}

public sealed class RuleRetrievalVocabularyDefinition
{
    [JsonRequired]
    public int VocabularyFormatVersion { get; init; }

    [JsonRequired]
    public IList<RuleVocabularyConcept> Concepts { get; init; } = [];

    [JsonRequired]
    public IList<RuleVocabularyBinding> Bindings { get; init; } = [];
}

public sealed class RuleVocabularyConcept
{
    [JsonRequired]
    public string ConceptId { get; init; } = string.Empty;

    [JsonRequired]
    public string CanonicalTerm { get; init; } = string.Empty;

    [JsonRequired]
    public int Weight { get; init; }

    [JsonRequired]
    public IList<RuleVocabularyAlternative> Alternatives { get; init; } = [];

    [JsonRequired]
    public string Rationale { get; init; } = string.Empty;
}

public sealed class RuleVocabularyAlternative
{
    [JsonRequired]
    public string Term { get; init; } = string.Empty;

    [JsonRequired]
    public RuleVocabularyAlternativeKind Kind { get; init; }

    [JsonRequired]
    public int Weight { get; init; }

    [JsonRequired]
    public string Rationale { get; init; } = string.Empty;
}

public sealed class RuleVocabularyBinding
{
    [JsonRequired]
    public string RuleSourceId { get; init; } = string.Empty;

    [JsonRequired]
    public RuleVocabularyBindingScope Scope { get; init; }

    [JsonRequired]
    public string? SourceAnchor { get; init; }

    [JsonRequired]
    public IList<string> ConceptIds { get; init; } = [];

    [JsonRequired]
    public string Rationale { get; init; } = string.Empty;
}

public static class RuleRetrievalVocabulary
{
    public const int CurrentVocabularyFormatVersion = 1;

    public static string NormalizeTerm(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 2048)
        {
            throw new ArgumentException("A vocabulary term must be non-empty bounded text.", nameof(value));
        }

        var normalized = value.Normalize(NormalizationForm.FormC).ToLowerInvariant();
        var result = new StringBuilder(normalized.Length);
        var pendingSpace = false;
        foreach (var character in normalized)
        {
            if (char.IsWhiteSpace(character))
            {
                pendingSpace = result.Length != 0;
                continue;
            }
            if (char.IsControl(character))
            {
                throw new ArgumentException("Vocabulary terms cannot contain non-whitespace control characters.", nameof(value));
            }
            if (pendingSpace)
            {
                result.Append(' ');
                pendingSpace = false;
            }
            result.Append(character);
        }

        // Case conversion is followed by NFC so emission always meets the format-1 carrier.
        var term = result.ToString().Normalize(NormalizationForm.FormC);
        if (term.Length is 0 or > 256)
        {
            throw new ArgumentException("Normalized vocabulary terms must contain 1 through 256 characters.", nameof(value));
        }
        return term;
    }

    internal static RuleRetrievalVocabularyDefinition NormalizeAndValidate(
        RuleRetrievalVocabularyDefinition definition,
        IReadOnlySet<string> sourceIds)
    {
        const string root = "$.retrievalVocabulary";
        if (definition.VocabularyFormatVersion != CurrentVocabularyFormatVersion)
        {
            Fail("VOCABULARY_FORMAT_UNSUPPORTED", $"{root}.vocabularyFormatVersion", "Only vocabulary input format 1 is supported.");
        }
        if (definition.Concepts is null || definition.Bindings is null)
        {
            Fail("VOCABULARY_STRUCTURE_INVALID", root, "Vocabulary concept and binding arrays cannot be null.");
        }

        var conceptIds = new HashSet<string>(StringComparer.Ordinal);
        var concepts = new List<RuleVocabularyConcept>(definition.Concepts.Count);
        for (var index = 0; index < definition.Concepts.Count; index++)
        {
            var concept = definition.Concepts[index];
            var path = $"{root}.concepts[{index}]";
            if (concept is null || concept.Alternatives is null)
            {
                Fail("VOCABULARY_STRUCTURE_INVALID", path, "A concept and its alternatives cannot be null.");
            }
            MaterializedRuleSourceLoader.RequiredIdentifier(concept.ConceptId, $"{path}.conceptId");
            if (!conceptIds.Add(concept.ConceptId))
            {
                Fail("VOCABULARY_CONCEPT_DUPLICATE", $"{path}.conceptId", "Vocabulary concept IDs must be unique.");
            }

            var canonicalTerm = Term(concept.CanonicalTerm, $"{path}.canonicalTerm");
            Weight(concept.Weight, $"{path}.weight");
            Rationale(concept.Rationale, $"{path}.rationale");
            var terms = new HashSet<string>(StringComparer.Ordinal) { canonicalTerm };
            var alternatives = new List<RuleVocabularyAlternative>(concept.Alternatives.Count);
            for (var alternativeIndex = 0; alternativeIndex < concept.Alternatives.Count; alternativeIndex++)
            {
                var alternative = concept.Alternatives[alternativeIndex];
                var alternativePath = $"{path}.alternatives[{alternativeIndex}]";
                if (alternative is null)
                {
                    Fail("VOCABULARY_STRUCTURE_INVALID", alternativePath, "An alternative cannot be null.");
                }
                var term = Term(alternative.Term, $"{alternativePath}.term");
                if (!terms.Add(term))
                {
                    Fail("VOCABULARY_TERM_DUPLICATE", $"{alternativePath}.term", "A concept's canonical term and alternatives must be distinct after normalization.");
                }
                if (!Enum.IsDefined(alternative.Kind))
                {
                    Fail("VOCABULARY_KIND_INVALID", $"{alternativePath}.kind", "The alternative kind is unsupported.");
                }
                Weight(alternative.Weight, $"{alternativePath}.weight");
                Rationale(alternative.Rationale, $"{alternativePath}.rationale");
                alternatives.Add(new()
                {
                    Term = term,
                    Kind = alternative.Kind,
                    Weight = alternative.Weight,
                    Rationale = alternative.Rationale
                });
            }
            concepts.Add(new()
            {
                ConceptId = concept.ConceptId,
                CanonicalTerm = canonicalTerm,
                Weight = concept.Weight,
                Alternatives = alternatives.OrderBy(item => item.Term, StringComparer.Ordinal).ToArray(),
                Rationale = concept.Rationale
            });
        }

        var targets = new HashSet<(string SourceId, RuleVocabularyBindingScope Scope, string? Anchor)>();
        var bindings = new List<RuleVocabularyBinding>(definition.Bindings.Count);
        for (var index = 0; index < definition.Bindings.Count; index++)
        {
            var binding = definition.Bindings[index];
            var path = $"{root}.bindings[{index}]";
            if (binding is null || binding.ConceptIds is null || binding.ConceptIds.Count == 0)
            {
                Fail("VOCABULARY_STRUCTURE_INVALID", path, "A binding requires a non-empty concept-ID array.");
            }
            MaterializedRuleSourceLoader.RequiredIdentifier(binding.RuleSourceId, $"{path}.ruleSourceId");
            if (!sourceIds.Contains(binding.RuleSourceId))
            {
                Fail("VOCABULARY_SOURCE_MISSING", $"{path}.ruleSourceId", "The binding references an undeclared Rule Source.");
            }
            if (!Enum.IsDefined(binding.Scope) ||
                (binding.Scope == RuleVocabularyBindingScope.Source && binding.SourceAnchor is not null))
            {
                Fail("VOCABULARY_SCOPE_INVALID", $"{path}.scope", "Source-wide bindings require a null anchor; other bindings must use Snippet scope.");
            }
            if (binding.SourceAnchor is not null)
            {
                MaterializedRuleSourceLoader.RequiredIdentifier(binding.SourceAnchor, $"{path}.sourceAnchor");
            }
            if (!targets.Add((binding.RuleSourceId, binding.Scope, binding.SourceAnchor)))
            {
                Fail("VOCABULARY_BINDING_DUPLICATE", path, "Combine concepts into one binding per source, scope, and anchor.");
            }
            Rationale(binding.Rationale, $"{path}.rationale");
            var referencedConcepts = new HashSet<string>(StringComparer.Ordinal);
            for (var conceptIndex = 0; conceptIndex < binding.ConceptIds.Count; conceptIndex++)
            {
                var id = binding.ConceptIds[conceptIndex];
                var idPath = $"{path}.conceptIds[{conceptIndex}]";
                MaterializedRuleSourceLoader.RequiredIdentifier(id, idPath);
                if (!conceptIds.Contains(id))
                {
                    Fail("VOCABULARY_CONCEPT_MISSING", idPath, "The binding references an undefined concept.");
                }
                if (!referencedConcepts.Add(id))
                {
                    Fail("VOCABULARY_CONCEPT_REFERENCE_DUPLICATE", idPath, "A binding may reference each concept only once.");
                }
            }
            bindings.Add(new()
            {
                RuleSourceId = binding.RuleSourceId,
                Scope = binding.Scope,
                SourceAnchor = binding.SourceAnchor,
                ConceptIds = referencedConcepts.Order(StringComparer.Ordinal).ToArray(),
                Rationale = binding.Rationale
            });
        }

        // All declaration collections are sets. Keep raw order in authoritative bytes only.
        return new()
        {
            VocabularyFormatVersion = definition.VocabularyFormatVersion,
            Concepts = concepts.OrderBy(item => item.ConceptId, StringComparer.Ordinal).ToArray(),
            Bindings = bindings.OrderBy(item => item.RuleSourceId, StringComparer.Ordinal)
                .ThenBy(item => item.Scope.ToString(), StringComparer.Ordinal)
                .ThenBy(item => item.SourceAnchor, StringComparer.Ordinal).ToArray()
        };
    }

    private static string Term(string value, string path)
    {
        try
        {
            return NormalizeTerm(value);
        }
        catch (ArgumentException)
        {
            Fail("VOCABULARY_TERM_INVALID", path, "Vocabulary terms must normalize to non-empty NFC text of at most 256 characters.");
            return string.Empty;
        }
    }

    private static void Weight(int value, string path)
    {
        if (value is < 1 or > 1000)
        {
            Fail("VOCABULARY_WEIGHT_INVALID", path, "Vocabulary weights must be between 1 and 1000.");
        }
    }

    private static void Rationale(string value, string path)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 1024 || value.Any(char.IsControl))
        {
            Fail("VOCABULARY_RATIONALE_INVALID", path, "A bounded, non-empty review rationale without control characters is required.");
        }
    }

    [DoesNotReturn]
    private static void Fail(string code, string path, string message) =>
        throw new MaterializedRuleSourceException(code, path, message);
}
