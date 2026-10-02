# FR-026 Compiled Rule Store Retrieval Execution Plan

## Authority and Checkpoint

Owner-authorized [FR-026](V1_1_FUTURE_REVISION_PLAN.md#fr-026---compiled-rule-store-and-intelligent-retrieval) begins from clean, synchronized `main` at `2e6497c6f4720229f9a0fd0d2ffba65af1136167`. FR-022 through FR-025 are complete/Closed. FR-026 is the sole selected incomplete objective; A-H below are execution checkpoints, not new Future Revisions. Phase 13 stays Active, Future Revisions stays last, VERSION stays `1.0.0`, and both release/RC tags remain unchanged.

**Current state: A/B complete; C-H pending.** This plan and matrix were recorded with A-H pending before A implementation. The [A audit](audits/FR_026A_RETRIEVAL_CONTRACT_AUDIT.md) and [B audit](audits/FR_026B_CANDIDATE_MATCHING_AUDIT.md) record executed evidence, not file-based inference. Later checkpoints require separate owner authorization; this task stops after B's validated commit and normal `main` push.

## Recovered Architecture

The normative objective traces master requirements `#3`, `#4`, `#5`, `#30`, `#31`, `#32`, and `#34` (WP2/WP3): useful compiled snippets, controlled indexed retrieval, existing compact packets, deterministic diagnostics, the practical approximately 20K context target, context-cost instrumentation, and integrated compatibility validation. FR-026 contributes bounded rule contexts; FR-031/036 retain whole-context measurement/acceptance, not an invented 20K rule-packet ceiling. [FR-022](../docs/rules/COMPILED_RULES_ARTIFACT.md) owns the artifact; [FR-023 acceptance](audits/FR_023_REPRODUCIBLE_RULES_COMPILER_AUDIT.md) owns compilation; [FR-024 acceptance](audits/FR_024_CONTROLLED_RETRIEVAL_VOCABULARY_AUDIT.md) owns reviewed metadata; [FR-025 acceptance](audits/FR_025_ACQUISITION_AND_IMPORT_AUDIT.md) owns acquisition/trust/import. None owns this missing runtime ranking.

Actual reference components inspected:

- Portable `CompiledRulesArtifact.cs` retains ordinal source/snippet identities, applicability, priorities, preparation tiers, source dependencies and retrieval terms/relationships. FR-024 emits normalized wording, concept ID in `kind`, and weight. Canonical/alias/phrase origins and rationale remain compiler-only evidence; runtime must not invent them.
- `SqlServerCompiledRulesArtifactStore.cs` and additive migration `011_compiled_artifact_import.sql` retain exact first-approved bytes plus complete ordinal artifact-owned source/snippet JSON and dependency rows. Snippet JSON retains all 773 canonical terms. There is no separate term index. Complete readback validates projection against approved bytes. Repeated source IDs across histories are supported.
- Import is not publication/activation. Legacy `active_rule_releases` identifies a published Rule Release, not an imported artifact. There is no imported-artifact active pointer or portable artifact retrieval adapter. Explicit authorized artifact selection is therefore the first usable boundary; do not infer a Campaign binding or make an import active.
- Managed `RuleCompilation.cs` already selects chunks using world/module/mode/operation/topic selectors, mandatory Kernel and operation-scoped GM Runtime Procedure, source dependency closure, prerequisite-first ordering and an 8,000 estimated-token ceiling. These safety invariants are reusable. Its legacy priority/preparation/topic score does not use reviewed vocabulary and is not the new ranking specification. The Kernel's unconditional legacy eligibility is not permission to bypass compiled applicability.
- `PublishedRuleContextProvider` loads a Campaign-pinned/active legacy index, selects a closure, checks per-source readiness and raises pending-source priority. Reuse Pending/failure semantics, not its Campaign dependency in the portable query API.
- `RulePacketFormatter` already emits packet/release/source identity once and grouped executable text under source IDs; rich chunk provenance stays service-side. Preserve this model-facing format. Service results may retain snippet IDs/reasons without adding per-snippet audit data to ordinary model context.

The old large-document context problem is not solved by importing a 275 KB artifact and injecting it into the model. Retrieval must select a small, applicable, dependency-complete context from durable compiled metadata without recompiling or rereading Rule Sources.

## Normative Requirement and Gap Matrix

This covers the repository acceptance criteria plus the owner's detailed FR-026 requirements. Prompt section ranges are traceability, not replacement authority. All rows initially require new FR-026 evidence unless marked existing. Package ownership is deliberately bounded.

| Requirement / trace | Existing implementation | Remaining gap | Dependency / owner | Required acceptance evidence |
| --- | --- | --- | --- | --- |
| Explicit selected artifact, no history mixing (9-10, 37, 45-46; #3) | 011 unique Ruleset/semantic identity and artifact-local children | Campaign-independent request/store scope and isolated lookup | FR-025; A, F | Explicit Ruleset + semantic digest; A/B sharing source IDs never mix; missing/unauthorized scope fails. |
| Active versus imported/published (10, 46) | Legacy release active pointer; import never changes it | Small authorized bridge where needed, not a second store or automatic activation | FR-025; F | Legacy publication/readiness remains intact; explicit imported retrieval works without Campaign; publication/activation stays separate. |
| Small bounded request (11, 47) | Campaign-bound legacy request, 8K ceiling | Portable selectors, controlled query entries, explicit required identities | A | Immutable prepared input, bounds, exact IDs, no provider/path/Campaign fields. |
| Compatible normalization and phrases (12-14; #4) | FR-024 NormalizeTerm: NFC, invariant lowercase, collapsed whitespace | Runtime query preparation and exact term lookup | A, B | Punctuation/hyphens/phrases preserved; no substring/fuzzy/stemming/generated expansion. |
| Concepts/weights preserved (14, 43) | Format-1 `(term, kind, weight)`; kind is reviewed concept ID | Match associations without flattening same wording across concepts | FR-024; B, C | Distinct concept/weight matches survive; do not claim alias origin absent from artifact. |
| Applicability before relevance (18-20) | Legacy world/module/mode/operation/topic filters | Apply format-1 source selectors to snippet candidates | B | Strong inapplicable matches excluded; module/world isolation; wildcard only in declared source metadata. |
| Mandatory/always-include sources (19; planning acceptance) | Kernel, procedure mandatory for gameplay.resolve; applicability gates other alwaysInclude | Compiled equivalent, budget consumption, no caller omission | B, E | Applicable mandatory sources included without query hit; inapplicable alwaysInclude cannot bypass filters. |
| Preparation versus relevance (20) | Progressive preparation states and tier-based loading | Do not treat readiness/tier as unreviewed relevance boost | B, C, F | Tier carried; unavailable required closure Pending/failure, priority raised where supported; no guessing/full readiness requirement. |
| Deterministic rank and ties (15-17, 44; #4) | Legacy fixed priority/preparation/topic score | Transparent reviewed-weight score and stable tie specification | B; C | Exact matches, shared terms, priority/ties, culture/input-order equivalence, bounded score explanations. |
| Complete source dependencies (21-22) | FR-022 valid graph; legacy full-source transitive closure | Artifact-local compiled closure with corruption checks | B; D | Transitive prerequisites include all applicable dependency snippets, no duplicates/cycles/missing targets; reason separate from rank. |
| Closure-aware budget (23-24) | Legacy closure admission; mandatory overflow fails | Compiled closure admission using existing estimated tokens | C, D; E | Never truncate a dependency; optional closure skipped whole; required overflow fails, no partial successful packet. |
| Existing compact packet (7, 25-26; #5) | Source-grouped formatter and minimal release/source envelope | Adapt selected compiled snippets, no competing format | E, F | Prerequisite-first presentation versus ranking order explicit; normal 8K ceiling; no artifact/audit/acquisition dump. |
| Zero/broad results (27-28) | Legacy empty topics can broaden selection | Exact curated associations, no full-corpus fallback | B, C, E; G | No match yields only applicable mandatory rules or explicit no-match, never all sources; conflict/preparation/save bounded. |
| Curated quality (29-30) | 65 cases, 115 positive/130 negative association expectations | Representative retrieval/ranking and dependency fixtures | B-E; G | Intended eligible snippets discoverable; negatives do not leak; topology not mistaken for rank #1. |
| Bounded explainability (17, 43; #30) | Full legacy chunk metadata and compiler audit | Service-only matched term/concept/weight, score, inclusion reason | C-E | Direct/mandatory/explicit/dependency reasons distinguishable; no invented compiler-only origins. |
| Cheap metadata access (31-33, 42; #3/#31/#32) | 011 snippet JSON; no term index | Actual parameterized candidate query and justified index review | B-E; F, G | Query/row/candidate/result/packet counts; no executable-content full-text scan or artifact dump into model context. FR-031/036 retain the approximately 20K whole-context target/measurement. |
| One SQL/reference semantic engine (33) | Portable format-1 validator; legacy selector engine | Storage supplies candidates, portable engine owns semantics | B-E; F | Shuffled SQL rows and in-memory inputs give identical IDs/scores/reasons/closure/order. |
| Safe corruption/failure/cancellation (34-36, 47) | FR-022/B/F integrity; fixed importer failures | Retrieval taxonomy, cancellation propagation, targeted storage checks | A-F | Invalid/unavailable/inconsistent/dependency/budget/storage distinct; cancellation not empty or partial success; safe bounded messages. |
| Compiler/provider independence (39-41) | Closed compiler, trust and import stages | No runtime source-file/Git/provider/LLM calls | A-F | Dependency/API/source checks; durable artifact sufficient without source/provider. |
| Gameplay boundaries (37-38) | Mandatory GM Procedure, knowledge/agency/Unknown boundaries | Machinery only; no mandatory gameplay entry or Campaign binding | A-H | Existing behavior unchanged; FR-027 pending/unselected; Repository Canon distinct from Campaign Canon. |
| Canonical compatibility (52; #34) | 10 sources /154 snippets /773 terms, canonical hashes below | Preserve compiler/vocabulary/import/schema011/history behavior | A-H | Complete portable/CLI/SQL suites plus frozen canonical compile/hash/count checks. |
| Governance and exact-state validation (1-4, 48-58) | Phase 13 owner-mediated selection, repository validators | Bounded plan, package audits, stage/review/test/commit/push | A-H | Single FR-026 selected; no downstream/release/tag action; A only in first commit. |

## Execution Packages

Each package rereads only this matrix, its linked authority and directly affected files. Complete one package, run its validations, record an audit, review/stage/validate the exact tree, commit, and stop. Keep tests small and deterministic; canonical fixtures are final regression evidence, not production constants.

### FR-026A - Requirement Audit and Portable Query Contract

- **Status:** complete; [A audit](audits/FR_026A_RETRIEVAL_CONTRACT_AUDIT.md). B is complete; C-H remain pending.
- **Prerequisites:** closed FR-022/023/024/025; no further infrastructure.
- **Files:** this plan; portable `CompiledRuleRetrieval.cs` and focused tests; retrieval contract/navigation; governance and existing status assertions.
- **Scope:** explicit Ruleset/semantic artifact scope, concrete selectors, bounded query terms/phrases, exact required IDs, immutable prepared request, shared FR-024 normalization and fixed safe failure taxonomy. No matching/ranking engine or store implementation.
- **Acceptance:** deterministic immutable preparation, safe validation/cancellation, no Campaign/provider/storage dependency, unchanged canonical/legacy behavior.
- **Validation:** portable/CLI Release builds; focused A and full portable/CLI/SQL-enabled Managed suites; FR-022/023/024/025 regressions, boundaries, structural/repository/distribution and whitespace checks; focused/repository rerun staged.
- **Stop:** A commit/push only; B-H pending; FR-026 remains incomplete.

### FR-026B - Candidate Lookup and Applicability

- **Status:** complete; [B audit](audits/FR_026B_CANDIDATE_MATCHING_AUDIT.md). C-H remain pending.
- **Prerequisite:** A.
- **Files:** portable candidate/index API and tests; format-1 applicability models, A request contract; no SQL changes.
- **Scope:** reusable artifact-scoped candidate view, exact normalized whole query-entry matches, explicit identities, applicable mandatory roots, source selectors. Empty queries mean structural/mandatory/explicit selection, not all snippets. Preserve distinct `(term, kind, weight)` matches.
- **Acceptance:** world/module/mode/operation/topic gates including alwaysInclude; valid source wildcard semantics; explicit missing/inapplicable identity failure; no fuzzy/substring/auto vocabulary. Preparation tier carried, not rank score.
- **Validation:** focused candidate/applicability fixtures and canonical topology subset, shuffled insertion/culture/history isolation, A regression/full portable/boundary; CLI/Managed compatibility and repository checks as shared changes warrant.
- **Stop:** candidates/match evidence only, no ranking/closure/budget/SQL.

### FR-026C - Deterministic Ranking and Bounded Evidence

- **Prerequisite:** B.
- **Files:** portable ranking/result/evidence types, focused tests and contract section.
- **Scope:** define transparent controlled-match weight/priority ranking and ordinal ties from actual candidate fields. Distinguish mandatory/explicit/direct reasons; no invented alias-origin label or preparation relevance boost.
- **Acceptance:** repeated/culture/shuffled candidates produce same scores/order; identical wording under multiple concepts stays explainable; bounded matched evidence and arithmetic limits.
- **Validation:** focused rank/tie/weight/duplicate tests, A/B/full portable, semantic-boundary and repository checks.
- **Stop:** ordered roots, no dependency expansion or packet truncation.

### FR-026D - Artifact-Local Dependency Closure

- **Prerequisites:** B, C.
- **Files:** portable closure types/engine/tests; source graph adapters, contract section.
- **Scope:** transitive source dependency expansion, all applicable snippets of required source, duplicate-free prerequisite order and inclusion reasons; targeted corrupt/missing/cyclic/inapplicable dependency rejection.
- **Acceptance:** dependencies do not need rank hits; no cross-artifact reference; root and prerequisite reasons/order distinguished; cancellation cannot expose partial success.
- **Validation:** small graph cases plus canonical dependency-heavy cases, shuffled inputs/history isolation, A-C/full portable and repository checks.
- **Stop:** complete closures only; no budget admission, SQL or packet format.

### FR-026E - Compact Packet Admission and Presentation

- **Prerequisites:** C, D.
- **Files:** portable selection/budget engine, existing Managed `RulePacketFormatter` adaptation only if needed, compact packet tests and documentation.
- **Scope:** whole-closure admission under existing 8K estimated-token ceiling; mandatory/explicit required overflow fails; deterministic prerequisite presentation and source-grouped existing model packet. Zero matches do not load all rules.
- **Acceptance:** closure completeness, no duplicate token charge, existing authority envelope, detailed reasons service-only, missing required readiness never becomes guessed content.
- **Validation:** overflow/exact fit/shared dependencies/mandatory gameplay.resolve/no-match/broad query fixtures, packet size regression, full portable/CLI/Managed compatibility and structural/repository checks.
- **Stop:** in-memory compiled retrieval/packet semantics; no new campaign entry or durable query adapter.

### FR-026F - Durable Rule Store Adapter and Publication Boundary

- **Prerequisites:** B-E and FR-025 migration 011.
- **Files:** `SqlServerCompiledRulesArtifactStore`/new narrow retrieval adapter, published store/preparation interfaces, Managed tests; additive schema/template migration only if actual query shape justifies it.
- **Scope:** authorized explicit Ruleset/semantic lookup maps to import identity; parameterized snippet/term/dependency access feeding the portable engine. Review JSON term access/index needs with evidence before migrating. Reconcile the normal Rule Domain/publication integration without automatic activation or a competing store; retain legacy publication and progressive readiness semantics. Active lookup must resolve one authorized artifact, not infer a Campaign.
- **Acceptance:** in-memory/SQL semantic equality, A/B history isolation, corruption/unavailable/cancellation, pending closure priority, preserved active/published history. Term-index work must justify its measured/explainable access path and preserve 011 data.
- **Validation:** real disposable SQL upgrade/repeat/history/index/query/readback cases, complete SQL-enabled Managed/portable/CLI suites and repository/distribution checks.
- **Stop:** durable explicit/authorized rule retrieval only; no FR-027 binding/entry, admin surface or release work.

### FR-026G - Canonical Quality and Local Cost Acceptance

- **Prerequisites:** A-F.
- **Files:** curated FR-026 request fixtures/tests; FR-024 quality fixture reader reuse; diagnostic/cost audit.
- **Scope:** representative natural alternative query entries and structural operation requests across combat, save, evolution/preparation, Canon, knowledge, mandatory procedure and dependencies. Review all 65 topology cases for suitability; do not turn all 115 positives into top-rank assertions or confuse prerequisite inclusion with unwanted direct matches.
- **Acceptance:** eligible positives discoverable, direct-match negatives excluded, ambiguous/generic reviewed mappings bounded, compact contexts do not contain the full corpus. Record rows/candidates/queries/packet counts without arbitrary speed thresholds or FR-031-wide instrumentation.
- **Validation:** full canonical retrieval/SQL/reference/culture/shuffle/budget quality fixtures, all compatibility suites, deterministic distribution/repository checks.
- **Stop:** quality/cost evidence only; no corpus vocabulary/compiler changes or gameplay wiring.

### FR-026H - Integrated Acceptance and Closure

- **Prerequisites:** A-G accepted; all matrix rows evidence-backed.
- **Files:** final FR-026 audit, remaining integrated tests, plan/status/nav/validator assertions.
- **Scope:** review every requirement and exact staged tree; repair only actual FR-026 defects. Close only after integrated artifact/import/retrieval/filter/rank/closure/readiness/packet/history acceptance and owner authorization.
- **Validation:** full portable/CLI/SQL-enabled Managed Release suites, FR-022/023/024/025 and all structural/repository/links/distribution checks, staged focus rerun, version/tag/scope review.
- **Stop:** FR-026 closure commit/push, no automatic FR-027 selection or release action; distinguish disposable SQL evidence from live host/deployment acceptance.

## Compatibility Evidence to Preserve

Canonical frozen fixture: 10 sources, 154 snippets, 773 format-1 terms, 275,622 bytes; semantic digest `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`; serialized SHA-256 `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`. These are test/audit evidence, never production constants. No A/B compiler, manifest, vocabulary, migration, publication, gameplay or acquisition change is authorized.

No blocking prerequisite conflict was found. Later ranking/index/active-adapter details belong to their packages and must be specified/tested before implementation, not asserted as already working here.
