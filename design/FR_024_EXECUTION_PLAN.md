# FR-024 Execution Plan

## Authority and Checkpoint

FR-024 remains one governed Future Revision: [Retrieval Vocabulary and Compiler Audit](V1_1_FUTURE_REVISION_PLAN.md#fr-024---retrieval-vocabulary-and-compiler-audit). The owner selected it after FR-023 closure at `cc064d3f2644c4c55ac75787fd8ef43c1cfe3941`. A-F below are bounded execution checkpoints, not new Future Revisions or Project Phases. No other objective is selected.

**Current checkpoint:** FR-024A input model and validation implemented. FR-024B-F not started. FR-024 is incomplete and remains Roadmapped in the register while active in the roadmap. Stop after each validated focused commit; do not automatically continue to the next package.

## Existing Capability and Gaps

FR-022 already supplies format-1 terms, kinds, bounded weights, local term relationships, canonical ordering, semantic integrity, schema, and conformance fixtures. FR-023 supplies safe materialized input, exact-byte provenance, deterministic snippets, candidate retrieval carriers, validated assembly, canonical writing, and an offline CLI. Existing Managed selectors/topic retrieval are compatibility constraints, not the new ranking engine.

Missing work is reviewed vocabulary input, precise deterministic enrichment, inspectable term origin, CLI equivalence, deterministic coverage/ambiguity/breadth/boundary diagnostics, curated corpus mappings, and positive/negative quality fixtures. The artifact carrier is sufficient; no format conflict was found. Runtime ranking and searchable storage belong to FR-026; acquisition/import belongs to FR-025.

## FR-024A - Reviewed Vocabulary Input

- **Purpose:** establish controlled concepts, alternatives, explicit binding scope, weights, review rationale, and conservative normalization.
- **Inputs:** already-materialized source manifest and ordinary source files; explicit immutable source/compiler identities.
- **Outputs:** optional validated `retrievalVocabulary` in the materialized snapshot; [input contract](../docs/rules/CONTROLLED_RETRIEVAL_VOCABULARY.md); structured validation errors.
- **Dependencies:** completed FR-022/FR-023 only.
- **Files/components:** `RuleSourceManifest`, `MaterializedRuleSourceLoader`, portable vocabulary types/tests, this plan, navigation, and governance validation.
- **Acceptance:** closed required JSON; strict IDs/references/scopes/weights; duplicate normalized terms rejected within a concept; deterministic normalization/order; exact manifest-byte provenance; legacy absent-vocabulary artifacts unchanged. Anchors are shape-checked here, not resolved against snippets yet.
- **Validation:** portable Release build and focused input tests; complete portable and CLI suites; FR-022 conformance and structural harness; Managed suite because the shared manifest model changes; dependency boundaries; full repository validator and whitespace.
- **Stop boundary:** no term emission/enrichment, audit generator, CLI option, canonical corpus curation, or runtime behavior.

## FR-024B - Deterministic Snippet Enrichment

- **Purpose:** attach only justified vocabulary to precise existing snippet candidates and retain explainable term origin.
- **Inputs:** validated A snapshot and FR-023 snippet candidates.
- **Outputs:** enriched candidates using the existing format-1 carrier, controlled kind/relationship mapping, and deterministic origin evidence referencing the reviewed manifest definitions.
- **Dependencies:** A; preserve FR-023 snippet boundaries and identities.
- **Files/components:** portable enrichment API, candidate/retrieval models only where necessary, focused fixtures/tests, and input-contract extension.
- **Acceptance:** exact source/anchor binding existence; explicit source inheritance distinguishable from precise binding; no blanket copying or synonym generation; canonical concept versus Alias/Phrase origins reviewable; deterministic merge/collision handling; malformed/unresolved references fail. Resolve any carrier limitation through governance rather than silently changing format 1.
- **Validation:** focused enrichment/negative tests; repeated/order/culture/relocation comparisons; FR-022 carrier/digest and complete portable regression tests; dependency-boundary and repository validation.
- **Stop boundary:** portable enrichment only; no CLI wiring, audit report, corpus-wide vocabulary, or ranking.

## FR-024C - Pipeline and Identity Integration

- **Purpose:** make library and standalone CLI compilation consume the same enrichment without duplicating FR-023 logic.
- **Inputs:** A snapshot, B enrichment, existing assembler/writer/CLI.
- **Outputs:** validated enriched format-1 artifacts and equivalent CLI/library bytes with manifest-bound provenance.
- **Dependencies:** B.
- **Files/components:** portable pipeline entry points, thin compiler CLI, assembly/writer integration tests, and reference compiler documentation.
- **Acceptance:** identical input produces identical metadata/digest/bytes; vocabulary mutation changes the appropriate manifest/semantic identity but not source-bound snippet IDs; no acquisition identity inference; legacy artifacts remain reproducible; reviewed origins remain inspectable without polluting normal runtime packets.
- **Validation:** portable and CLI Release builds/suites; FR-022 conformance; CLI/library equivalence; relocated/culture/repeated compilation; Managed suite if shared behavior is touched; repository/whitespace validation.
- **Stop boundary:** emission integration only; no new executable, retrieval query/ranking engine, or audit-quality thresholds.

## FR-024D - Deterministic Quality Audit

- **Purpose:** produce reviewable compiler evidence, not executable rule content.
- **Inputs:** reviewed definitions, B origin evidence, candidates, and C artifacts.
- **Outputs:** deterministic metrics and bounded machine-readable audit/report exposed through the existing CLI where useful; errors, warnings, and information are distinct.
- **Dependencies:** C (and B origin evidence).
- **Files/components:** portable audit API, report writer, minimal CLI report surface, deterministic report tests, and audit documentation.
- **Acceptance:** stable counts/identities/order with no timestamps/paths/random data; coverage, duplicate/collision/ambiguity, over-broad terms, unreachable definitions, inherited-only vocabulary, and poor snippet-boundary signals are inspectable. Explain heuristics and avoid arbitrary acceptance thresholds before corpus evidence. No private reasoning, secrets, or campaign data.
- **Validation:** fixed positive/negative audit fixtures, deterministic ordering/locale/repeated output, report schema/structure checks as appropriate, complete portable/CLI suites, repository validation.
- **Stop boundary:** report machinery, not canonical vocabulary curation or runtime ranking. Diagnostics cannot invent or rewrite rules.

## FR-024E - Canonical Vocabulary and Quality Fixtures

- **Purpose:** review actual corpus associations rather than generate an uncontrolled dictionary.
- **Inputs:** current canonical ten-source corpus, C compiler, D audit, and canonical terminology.
- **Outputs:** bounded reviewed manifest vocabulary and corpus-derived positive/negative fixtures; baseline metrics with explainable review dispositions.
- **Dependencies:** D.
- **Files/components:** canonical rule-source manifest, focused conformance/test fixtures, quality evidence, and relevant navigation.
- **Acceptance:** approved alternative demonstrates discoverability through exact metadata inspection, not an FR-026 ranking substitute; combat/progression/persistence/Canon/retrieval representatives follow actual rule meaning; broad irrelevant associations rejected or explicitly downgraded in metadata/audit. Report actual concepts/alternatives/coverage/terms-per-snippet/breadth/ambiguity/warnings. Preserve the current 10-source/154-snippet baseline unless legitimate source-boundary evidence requires explicit review.
- **Validation:** curated expected associations and negative non-equivalence fixtures; canonical audit metrics; direct/CLI equality; repeated/relocated/culture reproducibility; FR-022/FR-023 regressions; complete relevant suites and repository validation.
- **Stop boundary:** corpus metadata/evidence only; no runtime query scores, top-K, store/import, or gameplay changes.

## FR-024F - Integrated Acceptance and Closure

- **Purpose:** validate the whole governed objective and close it only after all acceptance criteria have evidence.
- **Inputs:** A-E implementation, canonical corpus, fixtures, contracts, and reports.
- **Outputs:** final FR-024 audit and closure links; roadmap/register state updated only if acceptance passes.
- **Dependencies:** all preceding packages.
- **Files/components:** final audit, regressions, governance/navigation, and only genuinely necessary FR-024 corrections.
- **Acceptance:** approved alternative targets representative rule; irrelevant broad matches rejected/visibly downgraded; reviewable provenance; identical audit output; no private reasoning or campaign data. Portable library/CLI remain offline and dependency-free; FR-022 semantics and Managed compatibility remain intact.
- **Validation:** portable library and standalone CLI Release builds; complete portable/CLI/Managed suites; FR-022 structural harness; positive/negative vocabulary and deterministic audit tests; canonical 10/154 compilation; CLI/library equality; source relocation, repeated/culture reproducibility; dependency boundary; full repository validator; full diff/whitespace review.
- **Stop boundary:** one closure commit, no next FR selection, push, release artifact, VERSION change, or tag movement.

## Acceptance Evidence Map

| Governed requirement | Current state | Completing packages |
| --- | --- | --- |
| Reviewable controlled concepts, alternatives, weights | Input model implemented; compiled terms pending | A, B, C |
| Representative approved alternative locates its rule | Not yet demonstrated; no enrichment yet | B, E, F |
| Irrelevant broad associations rejected/downgraded | Explicit precision model exists; quality evidence pending | B, D, E, F |
| Vocabulary provenance reviewable | Input rationale and exact manifest hash retained; emitted origin evidence pending | A, B, C, F |
| Deterministic compiler-quality audit | Not started | D, E, F |
| No private reasoning/campaign data | Input boundary enforced and fixtures generic; final report review pending | A-F |
| Offline reproducibility and CLI equivalence | Existing FR-023 behavior preserved; enriched integration pending | B, C, E, F |

## Resume Instructions

Read this plan, the input contract, `RuleRetrievalVocabulary.cs`, and the materialized snapshot/candidate types before FR-024B. Do not reread unrelated gameplay systems or restart A. Reuse the existing `RuleSnippetCandidate.Retrieval`, artifact validator, and canonical writer. First test exact binding-target resolution and origin mapping with isolated fixtures; do not begin corpus curation or CLI work in B.

Keep `VERSION` at `1.0.0`, preserve both existing release tags, and keep Future Revisions `[∞]` last. FR-024 remains the selected objective; package continuation still requires an explicit next execution request.
