# Compiled Rule Store Retrieval

## Document Control

- **Owner:** portable compiled Rule Store request scope, deterministic controlled-query preparation and retrieval-stage failure boundaries.
- **Dependencies:** [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md), [Controlled Retrieval Vocabulary](CONTROLLED_RETRIEVAL_VOCABULARY.md), [Acquisition and Import](COMPILED_RULES_ACQUISITION_AND_IMPORT.md), and [existing retrieval/packet rules](RULE_COMPILATION_AND_RETRIEVAL.md).
- **Extensions:** FR-026 candidate, ranking, dependency, packet and storage checkpoints; FR-027 may bind an authorized Campaign to this machinery later.
- **Consumers:** Managed providers, portable retrieval implementations, diagnostics and conformance tests.
- **Boundary:** Repository Canon retrieval only; no Campaign Canon, provider/source-file access, compiler invocation, trust approval or implicit activation.

## Implementation State

[FR-026 execution plan](../../design/FR_026_EXECUTION_PLAN.md) separates the remaining work. A establishes request preparation only. This document does not claim candidate lookup, ranking, dependency expansion, compact packet construction or durable retrieval is already implemented. Existing Managed legacy retrieval remains unchanged until its authorized adapter checkpoint.

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

The query is an explicit set of terms/phrases, not free-form prose to be interpreted. Later candidate lookup compares entire normalized entries to compiled term wording using ordinal equality. Thus `soul-bound` differs from `soul bound`, and `campaign canon` is not reduced to `campaign` or `canon`. The caller may formulate multiple entries; the engine does not infer them. A only normalizes and validates, not matches.

Normalized query entries are deduplicated and ordered ordinally. Concrete identifier sets are also deduplicated/ordered ordinally but preserve identifier case and exact identity; normalization must not alter semantic source/snippet IDs. Applicability selector comparison remains owned by the existing selector contract, not query normalization. Prepared collections are private copies with read-only views, so later caller mutation cannot change an in-flight request. No time, process, culture or physical path contributes to it.

## Result and Failure Boundary

A returns a prepared request, not a retrieval result, packet, score or readiness acknowledgement. Expected invalid request failures use `CompiledRuleRetrievalException`, a fixed safe code/category/message with no echoed caller input. The shared category vocabulary reserves unavailable artifact, inconsistent stored artifact, dependency failure, insufficient complete-packet budget and storage failure for their implementing checkpoints. Those outcomes are not executable paths in A.

Cancellation remains `OperationCanceledException` with the supplied token; it is never empty results, storage failure or a partial successful packet. A checks cancellation before and during preparation. Future stages must preserve it across storage and selection.

## Later Retrieval Invariants

The [requirement matrix](../../design/FR_026_EXECUTION_PLAN.md#normative-requirement-and-gap-matrix) owns package/evidence routing. Applicability gates relevance and always-include; mandatory Kernel/operation-scoped GM Procedure cannot be omitted by query choice. Reviewed concept ID and weight survive matching; canonical/alias/phrase origin is compiler-only and cannot be invented from runtime `kind`.

Direct ranking, dependency reasons and prerequisite presentation are distinct. A selected root requires its complete applicable artifact-local transitive source closure. Whole-closure budgeting must fail for missing required structure rather than silently truncate. Unknown/unprepared required rules remain failure/Pending, not guessed Canon. Normal model-facing delivery reuses the [existing compact packet](RULE_COMPILATION_AND_RETRIEVAL.md#retrieval), not artifact JSON or compiler audit origins. Broad/no-match requests never trigger a full-corpus fallback. Runtime evidence and storage checks must be bounded, parameterized and separate from normal player context.
