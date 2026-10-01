# Controlled Retrieval Vocabulary

## Purpose and Status

Controlled vocabulary is reviewed navigation input for deterministic compilation, not additional executable rules. It connects precise concepts and approved alternative language to selected snippets without asking a runtime AI to invent synonym sets.

**Implementation status:** FR-024A-F are complete: reviewed input, exact targets and term-origin evidence, shared artifact/CLI integration, observational quality reports, canonical curation, and integrated acceptance. The [closure audit](../../design/audits/FR_024_CONTROLLED_RETRIEVAL_VOCABULARY_AUDIT.md) records behavior, provenance, compatibility, and quality evidence beneath the single [FR-024 objective](../../design/FR_024_EXECUTION_PLAN.md). Loading a declaration alone does not establish a snippet target. No retrieval ranking is implemented here.

## Document Control

- **Owner:** reviewed vocabulary input, concept/alternative distinctions, binding scope, conservative normalization, exact target resolution, and term-origin evidence
- **Dependencies:** [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md), [Rule Compilation and Retrieval](RULE_COMPILATION_AND_RETRIEVAL.md), and [accepted retrieval boundaries](../../design/DECISIONS.md#d-1357--retrieval-metadata-cannot-change-rule-meaning)
- **Extensions:** FR-024 deterministic enrichment and audit consume this input; they must preserve format-1 identity and integrity semantics
- **Consumers:** vocabulary reviewers, offline compilers, quality diagnostics, and future FR-026 retrieval tooling
- **Repository boundary:** only reusable rule-navigation metadata; no Campaign Canon, private reasoning, host configuration, credentials, or runtime conversation

## Authority and Representation

A Rule Source manifest may contain an optional `retrievalVocabulary` object. Keeping reviewed input in the manifest makes its exact authoritative bytes part of the existing manifest SHA-256. No extra file lookup or acquisition-provider capability is needed. Omit the property for legacy payloads; explicit `null` is invalid.

This is an additive reference manifest-input extension. It does not change artifact format `1`, compiler contract `"1"`, snippet identity, or the existing artifact carrier. Older compilers are not required to accept this new manifest property; producers using it must select a compiler that supports this input. The canonical manifest was vocabulary-free through D; E adds explicitly reviewed corpus metadata, not new mechanics.

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

This B-stage representation is compiler evidence, not a second artifact format. B itself does not modify candidates or serialize evidence. C consumes it as described below. An absent or empty vocabulary yields empty associations and unchanged candidates, with no invented terms or required vocabulary block. Prose and headings never create terms automatically.

## Artifact and CLI Integration

The portable [`RuleCompilationPipeline.Compile(snapshot)`](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/RuleCompilationPipeline.cs) owns the complete in-memory path:

```text
validated materialized snapshot
  -> RuleSnippetCompiler
  -> RuleVocabularyEnricher
  -> reviewed association projection
  -> CompiledRulesArtifactAssembler
  -> FR-022 validation and semantic digest
  -> CompiledRulesArtifactWriter
  -> validated artifact + canonical bytes + vocabulary evidence
```

The normal standalone `compile` command loads the snapshot and calls this same pipeline. A declared vocabulary is integrated automatically, without a new flag or command. Managed runtime retrieval is not wired to this path by C.

Format 1 deliberately leaves retrieval `kind` categories open. This reviewed projection uses the **concept ID as the artifact term category**, not B's origin category:

| Reviewed association | Format-1 semantic metadata |
| --- | --- |
| Target snippet | Existing owning snippet ID; content and boundaries remain unchanged. |
| Concept ID | Retrieval term `kind`, a controlled concept category with the existing identifier bounds. |
| Normalized term or phrase | Retrieval term `term`, unchanged from A/B. |
| Reviewed strength | Retrieval term `weight`, unchanged; not a score or aggregate. |
| Canonical/alias/phrase origin, preferred term, bindings, rationales | Retained in `RuleCompilationResult.Vocabulary` for D, not repeated in every runtime term. |

This mapping is one-to-one for effective associations: A forbids repeated normalized terms within one concept, so `(snippet, concept ID, term)` is unique. Different concepts using the same text remain different `(term, kind)` entries, even with different weights or identical preferred words. No prefix, hash, synthetic term, or category encoding is needed; even a 128-character concept ID fits the existing kind field. Source/snippet overlap still produces one entry with all origins retained in evidence. No reviewed association disappears, no weight is chosen as a winner, and no unreviewed term relationships are synthesized. Relationship arrays remain empty for this input model.

The assembler sorts final terms by text then concept category, sources by source ID, and snippets by snippet ID using FR-022 ordering. B evidence retains original FR-023 candidate order and B origin categories/order. Evidence candidates remain unchanged; only separate projected candidates carry the artifact metadata. `RuleCompilationResult` contains the validated artifact, canonical bytes, B evidence, and existing structured artifact-validation errors. Failed artifact validation returns no artifact or bytes; input/snippet/enrichment failures retain their existing structured exception types.

Rationales and expanded origins are compiler/audit evidence rather than a new runtime artifact field. D can consume the result without reparsing JSON, consulting Managed persistence, or rereading sources. The artifact's exact manifest hash still binds all reviewed input, including preferred terms, origin declarations, rationale, and binding scope.

## Identity and Byte Compatibility

Vocabulary does not change Rule Source IDs, source-byte hashes, snippet IDs, executable text/content hashes, source dependencies, or applicability. Concept, term, or weight changes alter emitted retrieval metadata and therefore the existing semantic digest and canonical bytes. The digest calculation itself remains FR-022's length-prefixed projection, not the JSON file hash.

Exact manifest SHA-256 also participates in that projection. Two differently formatted, ordered, normalized-equivalent, or differently explained declarations may emit identical terms but still have different manifest hashes, artifact digests, and bytes. This is required provenance, not nondeterminism. Identical authoritative bytes and explicit identities produce identical output across repeated processes, source-root relocation, working directories, cultures, and internal set insertion order.

LF/CRLF source variants retain equivalent normalized snippets where appropriate, but different authoritative source hashes and consequently different artifact digests/bytes. An absent vocabulary retains the exact FR-023 canonical 10-source/154-snippet artifact baseline, including the 223,929-byte output; explicit empty vocabulary emits no terms but still has its own exact manifest provenance.

## Validation and Failure

The portable [materialized loader](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/MaterializedRuleSource.cs) preserves exact manifest bytes and hashes before producing the normalized [vocabulary model](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/RuleRetrievalVocabulary.cs). Existing strict JSON and identifier validation applies to the nested input.

Unsupported vocabulary version, null structure, duplicate concept/term/binding/reference, undeclared source or concept, invalid scope/term/weight, and missing rationale fail through structured portable input errors. Missing or unknown properties and invalid enum JSON use the existing manifest JSON failure path. These are contract errors, not quality warnings.

Enrichment fails atomically through `RuleVocabularyEnrichmentException` with a bounded message, structured `Code`, and logical `Path`. Reused A checks preserve their codes, including `VOCABULARY_SOURCE_MISSING`, `VOCABULARY_CONCEPT_MISSING`, and invalid identifier/scope/duplicate-definition codes. B adds `VOCABULARY_TARGET_MISSING`, `VOCABULARY_TARGET_AMBIGUOUS`, `VOCABULARY_CANDIDATE_INVALID`, `VOCABULARY_CANDIDATE_SOURCE_MISSING`, and `VOCABULARY_CANDIDATE_INCONSISTENT`. A failed required target never produces partial successful enrichment.

The CLI preserves normal exit categories: `3` for invalid materialized/vocabulary input, `4` for snippet compilation or vocabulary enrichment, and `5` for artifact validation/emission. Structured codes and bounded messages are retained; failure creates no partial artifact, does not replace existing output, and does not create output parents before validation. Normal output still uses atomic ordinary-file replacement, compact UTF-8 without BOM or trailing newline, and refuses manifest/source/directory collisions.

Collision breadth, generic wording, weak coverage, unused definitions, and numeric snippet-boundary signals are exposed by the audit below. That report is Derived evidence, never rules authority. Compilation must never invent a missing binding, silently broaden it, or discard a failed required validation.

## Observational Quality Audit

[`RuleVocabularyAuditor.Analyze(snapshot, compilation, policy)`](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/RuleVocabularyAuditor.cs) consumes A's validated definitions and C's successful artifact plus original B candidates/origins. It performs no file reads, text recompilation, prose classification, acquisition, or runtime query evaluation. A/B validation is reused to detect invalid mutable input, stale origins, missing targets, or evidence inconsistent with the artifact; it does not implement a second vocabulary validator.

The structured `RuleVocabularyAuditReport` is a separate analysis report, not a second compiled-rules artifact. It carries report format `1`, artifact/manifest hashes, normalized review policy, summary, snippet coverage, concepts, term target sets, binding support, association origins, and findings. Invalid evidence returns Error findings and empty analysis rows; zero metrics on an invalid report are not successful measurements. Invalid caller policy throws `ArgumentException`; the CLI reports `AUDIT_POLICY_INVALID` as input failure.

Report rows use ordinal semantic ordering: snippet ID; concept ID; term; binding source/scope/anchor; association snippet/concept/term/origin kind. Source/snippet/term target sets are distinct ordinal arrays. Diagnostic identity is `(code, subjectKind, subjectId)`, not an array index. A binding subject is the unambiguous source/scope/nullable-anchor tuple separated by `/`; identifiers cannot contain that separator. Findings sort Error, Warning, Information, then ordinal code/subject kind/subject ID. Messages are fixed and bounded.

`RuleVocabularyAuditWriter.Write(report)` emits one compact UTF-8 JSON representation, without BOM or trailing newline. Record declaration order defines property order; arrays are already canonical, enum names are strings, integers are invariant, and required null/empty values remain explicit. No dictionaries, timestamps, random IDs, acquisition paths, machine identity, or private reasoning enter the report. Only reviewed terms and logical identities are copied; full rationale remains in the original compiler evidence. No additional semantic audit digest is introduced: a report byte SHA-256 can be used for comparison, separately from artifact semantic/byte hashes.

### Metric Definitions

| Metric | Exact meaning |
| --- | --- |
| `sourceCount`, `snippetCount` | Validated artifact source and snippet counts. |
| `conceptCount`, `canonicalTermCount` | Reviewed concepts and their one canonical term each, including unused definitions. |
| `aliasCount`, `phraseCount`, `bindingCount` | Reviewed alternative declarations by kind and unique source/scope/anchor bindings, not multiplied by target count. |
| `effectiveAssociationCount`, `retainedOriginCount` | Unique snippet/concept/term/origin-kind associations and the sum of all distinct binding origins supporting them. |
| `coveredSnippetCount`, `uncoveredSnippetCount` | Snippets with at least one association versus none. Absence does not declare intentional exclusion or defective prose. |
| `conceptsWithAliases`, `conceptsWithoutAliases` | Concepts with at least one Alias versus zero; a Phrase is not an Alias. |
| `conceptsWithTargets`, `conceptsWithoutTargets` | Concepts producing at least one effective target versus none. |
| `uniqueTermCount` | Distinct normalized reviewed text across all concepts, including unused terms. |
| `multiOriginAssociationCount`, `sourceOnlyAssociationCount` | Associations with multiple binding origins, and associations whose origins are all explicitly source-wide. |
| `averageAssociationsPerCoveredSnippet`, `maximumAssociationsPerSnippet` | Exact association-count/covered-snippet-count ratio, and largest per-snippet association count. |
| `averageTargetSnippetsPerTerm`, `maximumTargetSnippetsPerTerm` | Sum of distinct target counts per normalized term divided by unique term count, and largest term target count. Shared concept targets are counted once per term. |
| `errorCount`, `warningCount`, `informationCount` | Finding counts by severity, not acceptance scores. |

Ratios retain integer `numerator` and `denominator` without reduction or rounding; `0/0` means the average is undefined, not zero. Term `corpusCoverage` is distinct target count / all snippet count; a percentage is that ratio multiplied by 100. Concept rows include canonical wording, aliases/phrases, binding/association counts, target snippets/sources, and shared terms. Binding rows include concepts, target snippets, supported association count, and exclusive association count (origins consisting only of that binding). Term rows expose per-concept origin kinds, exact weights, target sets, source sets, and whether concept target sets differ. Association rows retain every supporting binding identity.

### Severity and Review Policy

Errors invalidate the quality result because input or compiled evidence violates an established invariant. Warnings request review of valid metadata. Information measures or explains valid topology; it is not a penalty. No finding modifies a term, weight, binding, snippet, artifact, or digest, and no warning makes ordinary `compile` fail.

`RuleVocabularyAuditPolicy` has explicit portable defaults:

- `minimumBroadTargets = 8` and `minimumBroadCoveragePercent = 50`: a term or binding warns only when **both** inclusive thresholds hold. Requiring an absolute count and corpus share avoids labelling every small fixture or a small slice of a large corpus broad. These are review triage heuristics, not universal quality laws, scores, or fatal cutoffs. Comparison uses integer cross-multiplication without floating-point rounding.
- `largeSnippetTokens = 2048`: an informational boundary-review marker based solely on the existing token estimate (one quarter of the ordinary 8K packet target), not a prose judgement or enforced split.
- `genericTerms = []`: no implicit English stop-word list. Reviewers may explicitly supply up to 256 terms; A's normalization and ordinal deduplication apply. Only exact reviewed wording is flagged; no dictionaries, synonyms, stems, substring matching, or LLMs are consulted.

| Severity | Codes and meaning |
| --- | --- |
| Error | Existing A/B codes such as `VOCABULARY_TERM_DUPLICATE`; duplicates of canonical/alias wording are already invalid, not a new subjective warning. |
| Error | `VOCABULARY_AUDIT_COMPILATION_INVALID`, `VOCABULARY_AUDIT_PROVENANCE_MISMATCH`, `VOCABULARY_AUDIT_EVIDENCE_MISMATCH`: compilation validity, snapshot provenance, or reviewed/artifact support failed. |
| Warning | `VOCABULARY_UNUSED_CONCEPT`: no effective targets. |
| Warning | `VOCABULARY_BROAD_TERM`, `VOCABULARY_BINDING_FAN_OUT`: both configured breadth thresholds reached. |
| Warning | `VOCABULARY_GENERIC_TERM`, `VOCABULARY_ONLY_GENERIC_TERMS`: explicit generic policy matched a term or all wording of a concept. |
| Information | `VOCABULARY_NO_COVERAGE`, `VOCABULARY_PARTIAL_COVERAGE`, `VOCABULARY_NO_ALIASES`, `VOCABULARY_CONCEPT_EVERYWHERE`: coverage/definition observations; aliases are not mandatory. |
| Information | `VOCABULARY_AMBIGUOUS_TERM`, `VOCABULARY_DIFFERENT_TARGET_SETS`, `VOCABULARY_MULTI_SOURCE_TERM`: shared normalized wording across concepts, differing exact target sets, or multiple sources. No semantic winner or similarity threshold is inferred. |
| Information | `VOCABULARY_SOURCE_BINDING`, `VOCABULARY_INHERITED_ONLY`, `VOCABULARY_MULTIPLE_ORIGINS`, `VOCABULARY_BINDING_NO_UNIQUE_ASSOCIATIONS`: explicit inheritance and overlap; lack of exclusive associations does not erase distinct reviewed provenance. |
| Information | `VOCABULARY_LARGE_SNIPPET`: existing token estimate reaches the review marker; heterogeneous English content requires a human, not metadata inference. |

### CLI and Curation Handoff

The standalone `audit` command uses the same required inputs as `compile`, compiles in memory using C, and writes **only** a separate report to `--output`. Optional `--audit-policy <path>` reads a closed JSON object with the four policy fields above (omitted fields use defaults). Duplicate/unknown properties, null/invalid values, and policy files larger than 1 MiB fail. `compile` does not accept this option or silently generate reports.

Audit writes reuse atomic temporary-file replacement and source/manifest/directory collision guards. The policy input is also protected. Audit may replace an existing format-1 audit object, but never a compiled artifact or unrecognized/truncated file. Compile refuses to replace an audit object; thus the two output types cannot accidentally share a requested path. Invalid compilation/audit input creates no report, preserves prior output, and creates no output parents. Warnings exit successfully without dumping the report to stderr; only compact counts and a separately labelled audit byte hash accompany success.

Corpus curation inspects precise positive/negative targets, uncovered snippets, and overlap/breadth/generic findings with retained B rationale. D never authors alternatives or infers intent. The historical pre-E corpus had 10 sources, 154 uncovered snippets, zero associations/origins, zero errors/warnings, and two information findings. The C fixture remains 2 sources, 4 covered snippets, 3 concepts, 4 aliases, 2 phrases, 4 bindings, 15 associations, 17 origins, zero errors/warnings, and nine information findings. Audit itself changes neither corpus nor artifact bytes.

### Canonical Curation Convention

The canonical manifest now explicitly reviews 62 concepts, 67 aliases, 72 phrases, and 122 bindings across the same 10 sources/154 snippets. Canonical terms use weight 900, useful phrases 800, aliases 700, and three justified generic aliases 600. These weights are not retrieval scores. Concepts distinguish campaign/repository authority, experience/rule preparation, and evolution convergence/divergence/regression; they never change executable meaning.

Bindings are snippet-specific except the single-snippet heading-free bootstrap. Generic wording is normally replaced by discriminating phrases. Retained ambiguity requires precise target sets and concise rationale. Positive/negative fixtures assert association presence/absence, not a runtime ranking. Curation aims at useful precision, not universal coverage. See the [E audit](../../design/audits/FR_024E_CANONICAL_VOCABULARY_AUDIT.md) for before/after identities, 115 positive/130 negative expectations, 32 reviewed residual gaps, and all three unsuppressed warning dispositions under the explicit reviewer policy. This does not authorize runtime matching or ranking.

## Provenance and Compatibility

Exact manifest bytes preserve the reviewed rationale, concept IDs, scopes, and weights. Source byte hashes and source/anchor snippet IDs remain independent of vocabulary wording. C retains traceable term origin alongside the existing format-1 term/relationship carrier and semantic digest; it creates no parallel vocabulary artifact or separate integrity algorithm.

The portable input needs only ordinary materialized files and explicit immutable source/compiler identity. Enrichment and assembly operate entirely on supplied memory and add no Git, network, MCP, SQL, campaign, or host dependency. Existing absent-vocabulary inputs still compile with empty retrieval metadata and unchanged canonical artifact bytes. C changes standalone compilation only; Managed publication and runtime retrieval remain unchanged.

## Related Documents

- [Rule Compilation Index](README.md)
- [Standalone Rules Compiler](../../examples/tooling/rules-compiler-dotnet/README.md)
- [FR-024 Execution Plan](../../design/FR_024_EXECUTION_PLAN.md)
- [v1.1 Future Revision Plan](../../design/V1_1_FUTURE_REVISION_PLAN.md)
