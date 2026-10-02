# Compiled Rule Store Retrieval

## Document Control

- **Owner:** portable compiled Rule Store request scope, controlled-query preparation, exact candidate matching, applicability and retrieval-stage failure boundaries.
- **Dependencies:** [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md), [Controlled Retrieval Vocabulary](CONTROLLED_RETRIEVAL_VOCABULARY.md), [Acquisition and Import](COMPILED_RULES_ACQUISITION_AND_IMPORT.md), and [existing retrieval/packet rules](RULE_COMPILATION_AND_RETRIEVAL.md).
- **Extensions:** FR-026 ranking, dependency, packet and storage checkpoints; FR-027 may bind an authorized Campaign to this machinery later.
- **Consumers:** Managed providers, portable retrieval implementations, diagnostics and conformance tests.
- **Boundary:** Repository Canon retrieval only; no Campaign Canon, provider/source-file access, compiler invocation, trust approval or implicit activation.

## Implementation State

[FR-026 execution plan](../../design/FR_026_EXECUTION_PLAN.md) separates the work. A establishes request preparation; B adds artifact-local applicable **unranked** candidates. Ranking, dependency expansion, compact packet construction and durable retrieval remain pending. Existing Managed legacy retrieval remains unchanged until its authorized adapter checkpoint. The [B audit](../../design/audits/FR_026B_CANDIDATE_MATCHING_AUDIT.md) records reference semantics and executed compatibility evidence, not ranking quality.

## Explicit Rule Store Scope

A request names **Ruleset ID + semantic artifact SHA-256** from format-1. This identifies one imported semantic artifact, not a provider locator, discovery tag, acquired byte hash, source path, Campaign ID or current clock. Same snippet/source IDs in different historical artifacts cannot establish shared storage ownership.

The configured service must authorize that exact scope before lookup. A syntactically valid scope is not authorization, proof of existence, trust, readiness or publication. Imported candidates do not become active simply because they are queried. Existing active Rule Release lookup is distinct from imported-artifact identity; later integration must resolve an authorized scope explicitly and preserve publication/activation gates. FR-027 owns stable Campaign binding.

## Bounded Request

The portable reference accepts `CompiledRuleRetrievalRequest` and prepares an immutable `PreparedCompiledRuleRetrievalRequest` through `CompiledRuleRetrieval.Prepare`:

- explicit scope, concrete operation and Campaign mode (mode is a selector, not Campaign state);
- optional concrete World Model ID;
- enabled module IDs and topics;
- a set of explicit query terms/phrases;
- optional exact required Rule Source IDs and snippet IDs;
- maximum estimated tokens, 1 through the existing normal ceiling of 8,000.

No preparation-tier, ranking-boost, top-K, provider, Campaign, source-root or connection-string knob is exposed. Preparation/readiness is service state, not an excuse to skip rules or assign relevance. An empty query set is valid for structural/mandatory/explicit requests; it does not request the entire corpus.

Reference safety bounds: at most 16 query entries; at most 32 entries in each module/topic/required-source/required-snippet set. Each raw query entry is at most 2,048 characters and normalizes to 1-256 characters, exactly the reviewed vocabulary bounds. Concrete IDs follow format-1 identifier syntax/limits; snippet IDs use the established source ID plus optional `#anchor`. Semantic SHA-256 uses the contract's uppercase 64-hex representation. These bounds prevent pathological input, not permission to omit mandatory dependency closures later. Caller wildcards are rejected; `*` in compiled source selectors retains its established applicability meaning.

## Normalization and Matching Boundary

Preparation calls the existing FR-024 `RuleRetrievalVocabulary.NormalizeTerm`: NFC, invariant lowercase, Unicode whitespace collapse/trim and final NFC. Phrases stay whole. Punctuation, hyphens and singular/plural distinctions remain unchanged. No stemming, token/ngram expansion, substring guessing, embeddings, semantic similarity or synonym generation occurs.

The query is an explicit set of terms/phrases, not free-form prose to be interpreted. Candidate lookup compares entire prepared entries to compiled term wording using ordinal equality. Thus `soul-bound` differs from `soul bound`, and `campaign canon` is not reduced to `campaign` or `canon`. The caller may formulate multiple entries; the engine does not infer them. A only normalizes and validates; B matches without another normalization algorithm.

Normalized query entries are deduplicated and ordered ordinally. Concrete identifier sets are also deduplicated/ordered ordinally but preserve identifier case and exact identity; normalization must not alter semantic source/snippet IDs. Applicability selector comparison remains owned by the existing selector contract, not query normalization. Prepared collections are private copies with read-only views, so later caller mutation cannot change an in-flight request. No time, process, culture or physical path contributes to it.

## Applicable Unranked Candidates

`CompiledRuleCandidateIndex.Create(artifact)` makes a private deep read-only copy and validates that snapshot using FR-022, once per index rather than on each query. It indexes existing term wording, not executable text, paths, anchors or source documents. No acquisition, approval, import, publication or service authorization is performed. Consuming services must still authorize the selected scope. Invalid/duplicate/unsorted artifact identities or metadata fail safely rather than dictionary overwrite or silent repair.

`FindCandidates(preparedRequest)` requires exact index/request Ruleset and semantic digest equality. It returns `CompiledRuleCandidateSet`: the prepared request, scope, read-only candidates and compact snippet counts. Each candidate carries its immutable format-1 snippet/source, all matching `(term, kind, weight)` associations, applicability satisfaction and independent flags for controlled vocabulary, required source, required snippet, Runtime Kernel, operation-scoped GM Runtime Procedure and always-include. Overlapping reasons never duplicate a snippet. Same wording under different kinds/concepts retains separate evidence and weights; no alias-origin label or inferred relationship expansion is added.

Explicit source identity selects all its applicable snippets; explicit snippet identity selects exactly that snippet. Missing, wrong-case, empty-source or inapplicable required identities fail as invalid requests. They are not fake vocabulary hits. Applicable Runtime Kernel snippets are mandatory without query hits; at least one must exist for runtime lookup. `gameplay.resolve` additionally requires the existing `gm-runtime-procedure` source, always-included, applicable and containing snippets. Missing/inapplicable mandatory runtime structure is inconsistent for retrieval even if the general interchange artifact is format-valid. This is not a new format-1 requirement, readiness acknowledgement or complete-packet guarantee.

## Applicability Reference Semantics

All source selectors gate all candidate reasons, including strong matches, Kernel and always-include. Comparisons are ordinal case-insensitive, distinct from exact case-sensitive artifact/source/snippet identity and ordinal normalized term matching. Selector entries are alternatives within a dimension; dimensions are conjunctive:

- Empty arrays are unrestricted. A scoped World requires a concrete selected World Model; `*` matches any such World, not an absent selection.
- Scoped module selectors require at least one explicitly enabled matching module. `*` matches any enabled module, never implicit enabling. This applies to scoped Core/Kernel sources too, not just the Optional Module layer.
- Mode and operation match the concrete request, or authored `*`.
- Scoped topics require a matching request topic, or authored `*`; an empty request does not satisfy a concrete topic scope. Always-include means query-independent inclusion, not a topic/filter bypass. The canonical Kernel and mandatory procedure are topic-free and therefore need no caller topic.

This follows FR-026's explicit applicability gates. The legacy unconditional Kernel eligibility and always-include topic bypass are deliberately not copied; the legacy implementation itself remains unchanged. No priority/preparation/readiness policy enters these filters. Preparation tier, priority, selectors and source dependencies remain available on the frozen source for later packages.

Candidates preserve the artifact's validated ordinal snippet order; matches preserve ordinal term/kind order. Neither order is relevance ranking. No weight aggregation, priority sorting, preparation bonus, dependency expansion, truncation, top-K or packet construction occurs. Counts distinguish vocabulary-matched, applicable matched, inapplicable matched and unmatched snippets without returning a rejected-candidate dump. Evidence is bounded by the valid artifact and A's prepared query, with no arbitrary cap that loses legitimate associations. Empty/no-match queries return only applicable mandatory/always-include/explicit roots, never the whole corpus.

## Result and Failure Boundary

A returns a prepared request; B returns unranked candidates, neither a packet, score nor readiness acknowledgement. Expected failures use `CompiledRuleRetrievalException`, a fixed safe code/category/message with no echoed caller/artifact input or raw inner exception. B uses invalid-request for scope/required-identity mismatch and inconsistent-artifact for malformed input or missing applicable mandatory structure. Unavailable artifact, dependency, insufficient complete-packet budget and storage failures remain reserved for their implementing checkpoints.

Cancellation remains `OperationCanceledException` with the supplied token; it is never empty results, storage failure or partial success. A checks during preparation; B checks around snapshot validation, during index construction, selectors, identities, term hits, candidate selection and before returning. The existing full FR-022 validation call is synchronous and checks cancellation at its boundary. No partial index/set escapes. Read-only copies prevent later caller mutation; callers must not concurrently mutate assembly DTOs while copying them. Default string representations are fixed type names. Candidate source/content and prepared request are excluded from implicit JSON diagnostics; match evidence is service-facing, not ordinary player context.

## Later Retrieval Invariants

The [requirement matrix](../../design/FR_026_EXECUTION_PLAN.md#normative-requirement-and-gap-matrix) owns package/evidence routing. Applicability gates relevance and always-include; mandatory Kernel/operation-scoped GM Procedure cannot be omitted by query choice. Reviewed concept ID and weight survive matching; canonical/alias/phrase origin is compiler-only and cannot be invented from runtime `kind`.

Direct ranking, dependency reasons and prerequisite presentation are distinct. A selected root requires its complete applicable artifact-local transitive source closure. Whole-closure budgeting must fail for missing required structure rather than silently truncate. Unknown/unprepared required rules remain failure/Pending, not guessed Canon. Normal model-facing delivery reuses the [existing compact packet](RULE_COMPILATION_AND_RETRIEVAL.md#retrieval), not artifact JSON or compiler audit origins. Broad/no-match requests never trigger a full-corpus fallback. Runtime evidence and storage checks must be bounded, parameterized and separate from normal player context.
