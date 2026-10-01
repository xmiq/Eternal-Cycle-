# Controlled Retrieval Vocabulary

## Purpose and Status

Controlled vocabulary is reviewed navigation input for deterministic compilation, not additional executable rules. It connects precise concepts and approved alternative language to selected snippets without asking a runtime AI to invent synonym sets.

**Implementation checkpoint:** FR-024A validates reviewed input; FR-024B resolves exact targets and produces deterministic associations with term-origin evidence beside unchanged snippet candidates. Artifact/CLI integration, quality reports, and canonical corpus curation remain pending under the [FR-024 execution plan](../../design/FR_024_EXECUTION_PLAN.md). Loading a declaration alone does not establish a snippet target. No retrieval ranking is implemented here.

## Document Control

- **Owner:** reviewed vocabulary input, concept/alternative distinctions, binding scope, conservative normalization, exact target resolution, and term-origin evidence
- **Dependencies:** [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md), [Rule Compilation and Retrieval](RULE_COMPILATION_AND_RETRIEVAL.md), and [accepted retrieval boundaries](../../design/DECISIONS.md#d-1357--retrieval-metadata-cannot-change-rule-meaning)
- **Extensions:** FR-024 deterministic enrichment and audit consume this input; they must preserve format-1 identity and integrity semantics
- **Consumers:** vocabulary reviewers, offline compilers, quality diagnostics, and future FR-026 retrieval tooling
- **Repository boundary:** only reusable rule-navigation metadata; no Campaign Canon, private reasoning, host configuration, credentials, or runtime conversation

## Authority and Representation

A Rule Source manifest may contain an optional `retrievalVocabulary` object. Keeping reviewed input in the manifest makes its exact authoritative bytes part of the existing manifest SHA-256. No extra file lookup or acquisition-provider capability is needed. Omit the property for legacy payloads; explicit `null` is invalid.

This is an additive reference manifest-input extension. It does not change artifact format `1`, compiler contract `"1"`, snippet identity, or the existing artifact carrier. Older compilers are not required to accept this new manifest property; producers using it must select a compiler that supports this input. The current canonical manifest remains unchanged in FR-024A.

The manifest reviewer approves each concept, alternative, weight, and association. An alias is not a new mechanic or a canonical terminology synonym. A weight is a bounded retrieval signal, not a runtime scoring formula, success probability, or mechanical bonus. FR-026 owns query interpretation, ranking, and result selection.

## Input Contract

Every declared object is closed against unknown and duplicate JSON properties. All fields shown below are required within an explicitly supplied vocabulary block. Empty `concepts`, `bindings`, and `alternatives` arrays are permitted; each binding requires at least one concept ID. Null objects or arrays are invalid.

| Object | Fields | Meaning |
| --- | --- | --- |
| Vocabulary | `vocabularyFormatVersion`, `concepts`, `bindings` | Input version `1`, reviewed concepts, and precise associations. |
| Concept | `conceptId`, `canonicalTerm`, `weight`, `alternatives`, `rationale` | Stable concept identity, one preferred retrieval phrase, strength, controlled alternatives, and review basis. |
| Alternative | `term`, `kind`, `weight`, `rationale` | Approved wording with kind `Alias` or `Phrase`, strength, and review basis. |
| Binding | `ruleSourceId`, `scope`, `sourceAnchor`, `conceptIds`, `rationale` | Existing source identity, explicit `Snippet` or `Source` scope, nullable exact anchor, concept references, and association basis. |

Identifiers follow the existing manifest/format-1 identifier rules. Concept IDs are unique. References use exact ordinal identity, not display names. Weights are integers from `1` through `1000`. Rationales are non-empty review text of at most 1,024 characters without control characters; they must explain why the wording or association is valid, not contain private reasoning.

The following small reusable illustration is not canonical corpus curation. It assumes a declared `core` source containing an `authority` snippet anchor:

```json
"retrievalVocabulary": {
  "vocabularyFormatVersion": 1,
  "concepts": [{
    "conceptId": "campaign-canon",
    "canonicalTerm": "campaign canon",
    "weight": 800,
    "alternatives": [{
      "term": "established facts",
      "kind": "Phrase",
      "weight": 600,
      "rationale": "Approved wording for established campaign facts in this authority section."
    }],
    "rationale": "Names campaign authority, not repository rules authority."
  }],
  "bindings": [{
    "ruleSourceId": "core",
    "scope": "Snippet",
    "sourceAnchor": "authority",
    "conceptIds": ["campaign-canon"],
    "rationale": "This section defines campaign fact ownership."
  }]
}
```

## Binding Precision

- `Snippet` binds exactly one source/anchor pair. A null anchor names the source's unanchored snippet, not every snippet in that source.
- `Source` explicitly requests source-wide inheritance and requires a null anchor. Do not use it as a shortcut for indiscriminately copying vocabulary throughout a document.
- One binding is permitted per source/scope/anchor tuple. Combine its concept IDs rather than repeating a target.
- Binding source IDs and concept IDs must exist. Anchor syntax is checked during input loading; FR-024B checks actual target existence against compiled candidates. A loaded binding is therefore not proof that an executable snippet target exists.
- An unused concept, shared wording across concepts, or broad source binding may be legitimate but requires deterministic quality review in FR-024D. No implicit equivalence is introduced between those concepts.

## Deterministic Normalization

Reviewed terms use Unicode NFC, invariant lowercase, and collapsed Unicode whitespace with surrounding whitespace removed. Normalize to NFC again after case conversion. Terms must normalize to 1 through 256 characters; raw input is limited to 2,048 characters. Non-whitespace control characters are invalid.

Punctuation, hyphens, singular/plural forms, and phrases remain meaningful. There is no stemming, punctuation stripping, synonym expansion, n-gram enumeration, or online/LLM lookup. `campaign canon` and `repository canon`, `soul-bound` and `soul bound`, and `level` and `leveling` remain distinct unless a reviewer explicitly declares an appropriate association.

The canonical term and all alternatives within one concept must be distinct after normalization. Shared terms between different concepts remain separate associations for the later ambiguity audit; they are not automatically merged.

Concepts sort by ordinal concept ID, alternatives by normalized term, bindings by source ID then scope name then nullable anchor, and binding references by concept ID. Declaration collections are set-like. These normalized in-memory orders do not rewrite authoritative bytes: changing manifest formatting or declaration order still changes exact-byte manifest provenance, as required by FR-022/FR-023.

## Portable Enrichment and Evidence

[`RuleVocabularyEnricher.Enrich(snapshot, candidates)`](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/RuleVocabularyEnricher.cs) consumes a validated materialized snapshot and its complete FR-023 candidate sequence. It performs no file reads, source recompilation, acquisition, or provider lookup. Because public definition collections can be edited, it reuses A's validation and normalization before enrichment rather than adding a second set of input rules.

`Snippet` resolves by exact ordinal Rule Source ID and nullable anchor to the existing stable snippet ID. `Source` resolves to all supplied candidates from that exact source. A source with no compiled targets, absent anchor, or duplicate candidate identity fails; null does not mean wildcard. Candidate identity, path, and source hash must match snapshot provenance. No name, heading, substring, or semantic guessing is permitted. The caller must supply the complete candidate set: enrichment checks identity/provenance, not executable content by rerunning the compiler; final artifact validation remains the FR-022 gate in C.

The returned `RuleVocabularyEnrichment` retains manifest-relative path and exact manifest SHA-256. Each `RuleSnippetVocabulary` retains the original candidate instance, including its content, identity, hashes, selectors, dependencies, ordering, preparation tier, and retrieval carrier. Its separate associations contain:

| Evidence | Representation |
| --- | --- |
| Target | Original candidate's stable snippet ID and Rule Source provenance. |
| Concept | `ConceptId` and normalized `CanonicalTerm`; shared wording does not merge different concepts. |
| Reviewed term | Existing `CompiledRulesRetrievalTerm` with normalized text, kind `canonical`, `alias`, or `phrase`, and the exact reviewed weight. |
| Binding identity | Existing unique tuple `(RuleSourceId, Scope, SourceAnchor)`, not a generated ID. |
| Origin | Binding identity plus binding, concept, and term rationales. Canonical term rationale is the concept rationale; alternative rationale belongs to that reviewed alternative. |

Association identity is the target plus `(ConceptId, Term, Kind)`; each origin additionally identifies its reviewed binding. Manifest hash and these semantic keys trace back to reviewed definitions without timestamps, random IDs, or physical source-root paths. Term kind is the reviewed origin category, not a score. Source-wide and snippet-specific bindings that produce the same association merge their distinct origins without multiplying the effective term. A already rejects duplicate bindings and within-concept normalized duplicates, including an alias identical to its canonical term. Legitimate shared terms across concepts or targets retain separate associations and weights; B does not select a winner or reduce differing weights to one aggregate score.

Output keeps FR-023 candidate order. Associations sort ordinally by normalized term, kind, then concept ID. Origins follow A's normalized binding order: source ID, scope name, nullable anchor. Repeating enrichment does not mutate input or accumulate origins. Reordering semantically set-like declarations does not change associations; changing authoritative manifest bytes still changes manifest provenance.

This is an intermediate evidence representation for C, not a second artifact format. It reuses format-1 term values but does not populate candidate retrieval carriers, serialize a vocabulary artifact, wire the CLI, or project multiple-concept evidence into final metadata yet. An absent or empty vocabulary yields empty associations and unchanged candidates, with no invented terms or required vocabulary block. Prose and headings never create terms automatically.

## Validation and Failure

The portable [materialized loader](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/MaterializedRuleSource.cs) preserves exact manifest bytes and hashes before producing the normalized [vocabulary model](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/RuleRetrievalVocabulary.cs). Existing strict JSON and identifier validation applies to the nested input.

Unsupported vocabulary version, null structure, duplicate concept/term/binding/reference, undeclared source or concept, invalid scope/term/weight, and missing rationale fail through structured portable input errors. Missing or unknown properties and invalid enum JSON use the existing manifest JSON failure path. These are contract errors, not quality warnings.

Enrichment fails atomically through `RuleVocabularyEnrichmentException` with a bounded message, structured `Code`, and logical `Path`. Reused A checks preserve their codes, including `VOCABULARY_SOURCE_MISSING`, `VOCABULARY_CONCEPT_MISSING`, and invalid identifier/scope/duplicate-definition codes. B adds `VOCABULARY_TARGET_MISSING`, `VOCABULARY_TARGET_AMBIGUOUS`, `VOCABULARY_CANDIDATE_INVALID`, `VOCABULARY_CANDIDATE_SOURCE_MISSING`, and `VOCABULARY_CANDIDATE_INCONSISTENT`. A failed required target never produces partial successful enrichment.

Collision breadth, generic wording, weak coverage, unused definitions, and snippet-boundary quality require the later deterministic audit. That report is Derived evidence, never rules authority. Compilation must never invent a missing binding, silently broaden it, or discard a failed required validation.

## Provenance and Compatibility

Exact manifest bytes preserve the reviewed rationale, concept IDs, scopes, and weights. Source byte hashes and source/anchor snippet IDs remain independent of vocabulary wording. FR-024B/C must retain traceable term origin and use the existing format-1 term/relationship carrier and semantic digest; they must not create a parallel vocabulary artifact or omit vocabulary changes from integrity.

The portable input needs only ordinary materialized files and explicit immutable source/compiler identity. Enrichment operates entirely on supplied memory and adds no Git, network, MCP, SQL, campaign, or host dependency. Existing absent-vocabulary inputs still compile with empty retrieval metadata and unchanged canonical artifact bytes. Managed publication, normal CLI output, and runtime retrieval are not wired to enrichment by A or B.

## Related Documents

- [Rule Compilation Index](README.md)
- [Standalone Rules Compiler](../../examples/tooling/rules-compiler-dotnet/README.md)
- [FR-024 Execution Plan](../../design/FR_024_EXECUTION_PLAN.md)
- [v1.1 Future Revision Plan](../../design/V1_1_FUTURE_REVISION_PLAN.md)
