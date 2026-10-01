# FR-024 Controlled Retrieval Vocabulary Acceptance Audit

## Authority and Conclusion

The owner authorized FR-024F integration, acceptance, and closure from clean, synchronized `main` at `c5f1f2c3749f52e56abc89c99852c6adacc1a304`. [FR-024](../V1_1_FUTURE_REVISION_PLAN.md#fr-024---retrieval-vocabulary-and-compiler-audit) is complete and Closed after the checks below. A-F are execution checkpoints under that single objective, not additional Future Revisions. No next objective is selected.

F adds acceptance tests and closure/governance documentation only. No production implementation, canonical vocabulary, normative Rule Source prose, artifact schema, Managed runtime, or migration changed. The [E audit](FR_024E_CANONICAL_VOCABULARY_AUDIT.md) remains historical curation evidence. Accepted D-1358 through D-1362 already govern the behavior; this pass adds no new mechanics or identity semantics.

## Completed Architecture

| Checkpoint | Concrete owner | Accepted behavior |
| --- | --- | --- |
| A | `MaterializedRuleSourceLoader`, `RuleRetrievalVocabulary` | Optional closed reviewed manifest input; explicit source/compiler identity; exact authoritative bytes; normalized concepts, terms, and bindings. |
| B | `RuleVocabularyEnricher` | Exact source/anchor targets; deterministic effective associations; original snippet candidates and every reviewed origin retained. |
| C | `RuleCompilationPipeline`, existing assembler/validator/writer, thin CLI | Shared load/split/enrich/assemble/validate/serialize path; unchanged FR-022 format 1. |
| D | `RuleVocabularyAuditor`, `RuleVocabularyAuditWriter`, CLI `audit` | Separate deterministic observational topology/quality report; explicit reviewer policy; no mutation or runtime score. |
| E | Canonical manifest and quality fixtures | Authored concepts, alternatives, precise bindings, reviewed warnings, and positive/negative semantic topology. |
| F | Integrated acceptance tests and this audit | Mutations, provenance, deterministic canonical/historical bytes, process equality, compatibility, full validation, and governed closure. |

The [input/audit contract](../../docs/rules/CONTROLLED_RETRIEVAL_VOCABULARY.md), [artifact contract](../../docs/rules/COMPILED_RULES_ARTIFACT.md), [FR-023 audit](FR_023_REPRODUCIBLE_RULES_COMPILER_AUDIT.md), and [execution plan](../FR_024_EXECUTION_PLAN.md) remain the authority chain. Runtime selection is not part of this compiler pipeline.

## Acceptance Matrix

All results are Pass. Test names below identify executed behavior, not file-existence evidence. Portable tests live in [Rules.Tests](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/); actual-process tests live in [Compiler.Tests](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Compiler.Tests/).

| Requirement | Owner | Concrete implementation and executed evidence | Result / boundary |
| --- | --- | --- | --- |
| Original: approved alternative discovers representative rules | B/E | `ReviewedPositiveAndNegativeTopology`: 65 cases, 115 positive stable targets across all 62 concepts; fighting, character advancement, campaign facts, and other realistic terminology. | Pass; association existence, not rank. |
| Original: irrelevant broad matches rejected or visibly downgraded | B/D/E | 130 negative targets; exact missing/case/substring rejection; explicit generic/breadth diagnostics; reviewed generic weight 600 versus 900/800/700. | Pass; no query scoring. |
| Original: reviewable vocabulary provenance | A/B/C/F | Exact manifest hash, concept/term/binding identity, retained rationales; every canonical association checked; F traces representative emitted terms back to definitions. | Pass; richer evidence stays outside runtime terms. |
| Original: repeated audits deterministic | D/E/F | Actual CLI compile/audit/compile/audit, direct-library equality, repeated report writing, canonical set order, relocation, cultures, and unrelated CWD. | Pass; separate report, not rule authority. |
| Original: no private reasoning or campaign data | A/D/F | Closed input, bounded error output, generic reusable fixtures, safe report fields, complete diff and repository-boundary checks. | Pass; no conversations, credentials, host configuration, or campaign input. |
| Concepts, canonical terms, aliases, phrases, weights, rationales | A/B | 52 input and 37 enrichment cases cover types, exact weights, identities, rationales, invalid references/duplicates and closed properties. | Pass; no automatic authoring. |
| NFC/invariant lowercase/whitespace; phrase/punctuation preservation | A | `NormalizationPreservesPhrasesAndPunctuation`, invariant/idempotent tests and strict invalid-term cases. | Pass; no stemming or semantic equivalence. |
| Exact source-wide/snippet binding and target failures | B/F | Source inheritance, nullable unanchored target, sibling exclusions, missing anchors and duplicate candidate rejection; F invalid canonical input and candidate mutation. | Pass; no fuzzy/substring guessing or LLM inference. |
| Coalesced associations retain multiple origins | B/C/D | `OverlappingBindingsCoalesceAssociationWithoutLosingOrigins`, `OverlapKeepsAllOriginsButDoesNotDuplicateArtifactTerms`, and source-inheritance report tests. | Pass; generic fixture has 15 associations/17 origins; canonical corpus has no overlapping origins. |
| Shared terms retain distinct concepts and weights | B/C/E | Shared-word fixture, canonical conflict topology, per-association term-category mapping and exact origin tests. | Pass; no winner selection. |
| Existing artifact carrier and validation | C | `EveryEffectiveAssociationHasExactlyOneArtifactTerm`; actual FR-022 read/validate, integrity checks, contract fixtures. | Pass; normalized term, concept ID as kind, exact weight; no format 2. |
| Exact source hashes; snippet/content identity; applicability/dependencies | C/E/F | Historical/curated source and content comparisons plus F mutations; canonical source hashes and Rule Source records match the pre-E fixture. | Pass; reviewed metadata never changes executable meaning. |
| Vocabulary and exact manifest provenance affect identity | A/C/F | Seven integrated valid mutations and structured invalid mutations, detailed below. | Pass; semantic digest is not file SHA-256. |
| Historical no-vocabulary compatibility | B/C/D/E/CLI | Frozen exact-byte input compiles to the original 10/154 artifact and legacy identities; real CLI equals library. | Pass; live corpus remains curated. |
| Quality counts, diagnostics, warnings, and residual gaps | D/E | Exact summary/finding-count fixtures; all three warning dispositions and all 32 uncovered identities reproduced and reviewed below. | Pass; coverage is evidence, not a target to maximize. |
| Observational audit | D/F | Capture definitions, candidates/origins, artifact model and bytes; analyze; recompile; all unchanged. Generic warning-policy and no-source-files tests also pass. | Pass; audit cannot rewrite vocabulary or digest. |
| Deterministic portable artifact/report bytes | C/D/E/F | Repeat/relocation/culture/CWD/process/library/set-order matrix; no physical paths or volatile generated identity. | Pass; authoritative input byte changes remain detectable. |
| CLI failure propagation and output safety | C/D/E/F | Actual process tests: invalid definition/target fails compile and audit, bounded diagnostics, no partial output or parent; collision and cross-overwrite tests. | Pass; existing exit codes 3/4/5/6 retained. |
| UTF-8/property/collection serialization | C/D/F | Compact fixed property order, canonical collection ordering, no BOM or appended newline, stable report scalar/enum encoding. | Pass; one representation per writer. |
| Offline/portable boundary | A-F | Isolated manifest plus ten ordinary files, no Git layout; assembly dependencies and direct/CLI boundary regressions; acquisition APIs absent from production path. | Pass; no required Git/network/MCP/SQL/campaign service. Network isolation was not imposed. |
| Managed compatibility | FR-022/C/F | Complete 181-case Managed suite; format-1 unchanged; no Managed source edit in F. | Pass; no new vocabulary retrieval behavior. |
| Scope / future ownership | A-F | Production-path search and diff inspection; authored 62/67/72 vocabulary, no generated alternatives/search/score API. | Pass; FR-025 acquisition/import, FR-026 ranking/store, FR-031 cost instrumentation, FR-034 release work remain downstream. |

Optional term relationships remain supported by the unchanged FR-022 carrier/validator. The reviewed input contract does not author a synonym relationship graph; FR-024 does not manufacture one or change format 1 to accommodate compiler evidence.

## Canonical and Historical Evidence

The comparison holds explicit regression identities fixed: source scheme `git-commit`, value `d5028af32cf16fe48878ffef0301867175f98f08`; compiler `eternal-cycle-dotnet` version `1.0.0`, contract `1`. This isolates metadata/provenance behavior, not a claim that the curated corpus existed at that historical commit. Production callers must supply the actual selected immutable source identity.

The frozen pre-E manifest/source bytes survive checkout newline conversion through test-only preparation, which checks unchanged rule prose before materializing captured bytes. The production loader hashes the ordinary input bytes exactly; it never canonicalizes source bytes to conceal provenance.

| Metric | Historical no vocabulary | Accepted curated corpus |
| --- | ---: | ---: |
| Sources / snippets | 10 / 154 | 10 / 154 |
| Concepts / canonical terms | 0 / 0 | 62 / 62 |
| Aliases / phrases / bindings | 0 / 0 / 0 | 67 / 72 / 122 |
| Effective associations / retained origins | 0 / 0 | 773 / 773 |
| Unique normalized terms | 0 | 200 |
| Covered / uncovered snippets | 0 / 154 | 122 / 32 |
| Coverage | 0/154 | 122/154 (79.22%) |
| Concepts with / without aliases | 0 / 0 | 62 / 0 |
| Concepts with / without targets | 0 / 0 | 62 / 0 |
| Multi-origin / source-only associations | 0 / 0 | 0 / 3 |
| Maximum associations per snippet / targets per term | 0 / 0 | 24 / 10 |
| Errors / warnings / information | 0 / 0 / 2 | 0 / 3 / 105 |
| Artifact bytes | 223,929 | 275,622 |

Curated average associations per covered snippet is `773/122`; target snippets per term is `773/200`. More terms or coverage is not intrinsically higher quality.

| Identity | Historical | Curated |
| --- | --- | --- |
| Semantic artifact digest | `4B583A7904CE514B56CF3B497648DB5D778FC04DF2285D30A2E03823A32EA6A4` | `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B` |
| Serialized artifact SHA-256 | `802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC` | `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6` |

Curated audit: **435,079 bytes**, SHA-256 `A54B088E9FDBDEB6889F8C151EFC9F5C5D04E7584F4EC7E22F4558FE5FDABEC6`, under the explicit E review policy. Report hash, serialized artifact hash, and semantic artifact digest are distinct. Source hashes, content hashes, snippet IDs, source ordering, selectors, dependencies, and normalized executable text remain the original FR-023 semantics.

## Precision and Warning Review

The [quality fixture](../../examples/tooling/rules-compiler-dotnet/fixtures/canonical-vocabulary/quality-expectations.json) passes **65 cases, 115 positive expectations, and 130 negative expectations**, with no missing target or failed expectation. Cases cover every concept, phrases/aliases, source versus exact scope, campaign/repository authority, combat/claim conflict, XP/evolution readiness, gameplay/persistence, reincarnation/runtime reset, Development/context, and evolution convergence/divergence/regression/traps. They test associations on stable snippet IDs, never ranking positions.

The [explicit reviewer policy](../../examples/tooling/rules-compiler-dotnet/fixtures/canonical-vocabulary/audit-policy.json) retains all three `VOCABULARY_GENERIC_TERM` warnings:

| Term | Reproduced topology | Disposition |
| --- | --- | --- |
| conflict | Nine snippets: four combat-context and five canon-conflict; separate concept target sets. | Intentional shared alias at 600; stronger phrases distinguish meanings. Negative fixtures exclude cross-concept leakage. |
| preparation | Only route-readiness, gradual-evolution, and the-evolution-transition in core-monster-evolution. | Intentional evolutionary readiness alias at 600; not combat or provider rule preparation. |
| save | Only core-context-assembly#manual-persistence-commands. | Exact player command at 600; automatic saving, status, and retry use stronger specific concepts. |

No warning was removed, hidden, or newly introduced. No broad-term, high-fan-out, unused-concept, or only-generic warning exists. The sole source-wide binding is the heading-free GM bootstrap, reaching one snippet. All 105 information findings remain understood: 99 multi-source terms; conflict ambiguity/different-target-sets (two); bootstrap source-binding/inherited-only (two); partial coverage (one); large reincarnation transition snippet (one). The last is a boundary observation, not authorization to resplit FR-023 content.

## Review of 32 Uncovered Snippets

F inspected the actual compiled bodies, not just filenames/headings. The [post-E fixture](../../examples/tooling/rules-compiler-dotnet/fixtures/canonical-vocabulary/after-e.json) enumerates every stable ID. No high-value missed concept was found that justified altering the reviewed vocabulary in this closure package. No coverage changed.

| Count | Stable IDs / classification | Review disposition |
| ---: | --- | --- |
| 9 | Root/title IDs: core-context-assembly, core-design-philosophy, core-development, core-monster-evolution, core-persistence-authority, core-reincarnation, core-world-engine, gm-runtime-procedure, runtime-kernel. | Titles only; content-bearing owner snippets already have precise concepts. Avoid title-wide keywords. |
| 8 | `#related-documents` on the same sources except core-design-philosophy. | Link/navigation lists, not new rule or vocabulary targets. |
| 5 | core-context-assembly: #acceptance-criteria, #document-control, #regression-cases, #safeguards, #storage-neutral-logical-guidance. | Conformance cases, ownership/control, repeated safety limits, and optional cache/reference schema. Core gates, automatic persistence, read sets, and derived-context owner sections carry the discriminating concepts; do not smear save/data/state across the lists. |
| 2 | core-development: #scope-boundaries, #system-interactions. | Specialist exclusions and supporting-system handoffs, not new growth currencies. Current-life effort, retained potential, capability, and anti-grinding concepts already target their defining rules. |
| 2 | core-monster-evolution: #scope-boundaries, #worked-examples. | Exclusions plus illustrative gradual preparation, convergence, divergence, simplification, traps, and failed transition. Each substantive mechanism has a precise accepted owner target. |
| 5 | core-persistence-authority: #safeguards, #scope-boundaries, #the-authority-layers, #valid-upward-proposals, #worked-examples. | Repeated authority limits, specialist routing, heading-only layer wrapper, governed proposals, and conflict examples. Repository/campaign Canon, retcon, migration, uncertainty, history, and conflict already target their defining rules. |
| 1 | core-reincarnation#examples. | Rebirth, dormant mastery, Age transition, cross-domain candidate illustrations; accepted core transition, retained/crossover, Interlife, and World Contact targets define these subjects. |

Residual material is not declared dispensable or independently discoverable by a vocabulary term. Source navigation, existing dependencies, and always-include behavior remain unchanged; they do not promise every future query will load every residual section. A future evidence-based review may justify a narrower association. A 100% coverage target or generic source-wide binding would not establish that evidence.

## Mutation and Provenance Acceptance

F's [portable acceptance tests](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/RuleVocabularyAcceptanceTests.cs) mutate isolated captured canonical materialization, never the real manifest. Every valid mutation preserves source hashes, executable content, source/snippet identities, selectors, and dependencies. Each variant is independently repeatable.

| Mutation | Expected and observed effect |
| --- | --- |
| Canonical term change; alias addition; phrase removal; weight change | Exact manifest provenance, effective retrieval metadata, semantic digest, serialized artifact bytes, and audit bytes all change. No source prose or content identity changes. |
| Exact binding moved to a different real unbound anchor | Deterministic target topology and artifact/audit identity change; snippet content remains unchanged. This is a mutation fixture, not new curation approval. |
| Rationale-only change | Emitted runtime terms unchanged; richer origin rationale changes; exact manifest hash changes, therefore semantic digest, artifact bytes, and audit bytes change. |
| JSON formatting-only change | Normalized definitions, associations, and origins unchanged; exact manifest hash, digest, artifact bytes, and audit bytes change. |
| Malformed/missing concept reference | Structured `VOCABULARY_CONCEPT_MISSING` input error; compilation cannot produce an artifact. |
| Missing anchor | Structured `VOCABULARY_TARGET_MISSING` enrichment error; no guessed or partial output. |
| Duplicate candidate for an exact target | `VOCABULARY_TARGET_AMBIGUOUS` at the B candidate boundary; deterministic compilation itself never intentionally creates duplicate IDs. |

Reversing semantically unordered **in-memory** concept/alternative/binding/reference construction leaves artifact, origin evidence, and audit bytes unchanged while authoritative bytes stay fixed. Reordering authored JSON is a different operation: A/B normalize definition ordering, but its changed raw manifest hash intentionally changes artifact identity. The audit does not erase this distinction.

Representative canonical term `combat` (900), alias `fighting` (700), phrase `character advancement` (800), and source-wide `host instructions` (800) are traced from emitted artifact term to concept, normalized term/kind/weight, exact binding, and every rationale. Shared conflict is separately verified. The small generic integration fixture proves coalesced multiple origins rather than inventing a canonical overlap to test it.

## Determinism and CLI Evidence

- Repeated canonical compile/audit/compile/audit, relocation to separate ordinary-file roots, unrelated CWDs, Turkish and French cultures, actual CLI processes, and direct-library output all reproduce the curated hashes above.
- F's canonical set-order matrix reproduces both artifact and report bytes with unchanged input provenance. No filesystem enumeration, dictionary order, current clock, random/process/machine identity, or physical root enters semantic output.
- Observational audit captures the definition model, original candidates/origins, artifact model/digest, and bytes before analysis. They remain equal afterward, and recompilation produces identical bytes. D also audits successfully after fixture files are removed.
- Both writers emit compact UTF-8 without BOM or trailing newline; fixed property order, deterministic arrays, invariant scalars, and explicit enum representation are tested. Report output contains no physical path or generated timestamp.
- F adds four actual-process invalid canonical compile/audit cases. Invalid weight returns 3; invalid target returns 4. Each emits a bounded stable code, no stack trace, no artifact/report, and no new output parent. Existing collision tests reject manifest/source/directory/policy paths and artifact/report cross-overwrites and preserve existing output on failure.

F separately rewrites the ordinary ten source files to LF and CRLF **after** checkpoint preparation and loads those actual bytes directly. Anchors, snippet IDs, normalized text/content hashes, retrieval metadata, and manifest remain equal; all ten exact source hashes differ. Each variant independently repeats artifact/report bytes, rather than pretending the original mixed-newline baseline is all-LF.

| Variant | Semantic digest | Artifact byte SHA-256 |
| --- | --- | --- |
| LF | `01CAFF85FDFA6D4954B1B66D965627DAE0FAB9BA18FD79D85A3FA90F9FDAFE1A` | `084A4F41EA88BE16FC5EB792D9499E295BC525503B60220EF086EA447B5F994E` |
| CRLF | `216B8A260A63A85FB048E3FCF85E78048320D44CFC1DE3849C2BDC1395C7D059` | `EBAF75BB85A2E2EFCFBB1E56D47D7B358B82588B2A4359D09CEB6B2D018983E0` |

LF audit SHA-256: `885FFF0853456076A06CFF6F220D7494F0D31767BBC731DF0E0281DABFF0C30A`; CRLF audit SHA-256: `340395E0B84C1F646818A5EAC4096DFC0C8612E645386BF693F3890F03BD8641`. These are exact-byte acceptance inputs, not additional production serialization formats.

## Portability, Compatibility, and Cost Boundary

Isolated canonical input has exactly eleven ordinary files (manifest plus ten Rule Sources), no `.git`, installation layout, campaign configuration, host service, MCP, or SQL connection. Compiler/auditor execute over that payload and validated memory. Assembly/project boundary tests verify portable Rules has no Managed/SQL/hosting/MCP dependency and the CLI references Rules in the allowed direction. No network isolation was imposed, and no packet-level network-traffic claim is made; production-path inspection shows no acquisition/network API or service requirement.

FR-022 conformance/integrity and format-1 semantics remain unchanged. FR-023 and A-E suites remain compatible; the historical no-vocabulary artifact is reproduced by library and executable. Managed's full regression suite passes without adding vocabulary synthesis, fuzzy matching, ranking, or uncontrolled retrieval. No SQL schema or campaign migration is needed.

FR-024 supplies 154 source-bound snippets, 200 controlled normalized terms, 773 reviewed associations, and exact applicability/provenance for later indexed retrieval. It does **not** prove runtime token savings, query relevance scores, or retrieval speed. FR-026 owns searchable storage/retrieval, and FR-031 owns complete context-cost instrumentation. Quality heuristics and reviewed weights are not a ranking engine.

## Final Validation

Executed on the final candidate with no test failures or skips:

| Validation | Result |
| --- | --- |
| Portable library and standalone CLI Release builds | Pass; zero warnings/errors. |
| Complete portable suite | 305/305. |
| Complete CLI/process suite | 62/62. |
| Complete Managed MCP suite | 181/181. |
| New F portable / actual-process cases | 17/17 and 4/4; subsets of the complete suites. |
| Focused A/B/C/D/E portable | 52/37/25/28/76, all pass. |
| FR-023 regressions | 70/70. |
| FR-022 contract / portable dependency boundary | 16/16 and 1/1. |
| Structural harnesses | Nine harnesses, 333 assertions, including 32 FR-022; all pass. |
| Full repository validator | Pass: 297 Markdown files, 7,314 relative links, 200 anchors; zero blockers/orphans/forbidden campaign directories. |
| Whitespace and complete staged diff | Pass; no unrelated implementation, credentials, campaign data, or release changes. |

The complete suites total **548** tests; focused reruns are not added to that total. F initially corrected its new test helper to use the existing class-based artifact model instead of record-only syntax; no production defect or contract repair was required. Final governance validator/harness expectations change only from selected FR-024 to Closed FR-024 and pending FR-025 through FR-036, retaining all existing assertion counts and scope boundaries.

The final repository state also passes indexing/terminology/governance checks for 190 canonical documents, 15 family indexes, 43 templates, five agent roles, 1,273 terms, 208 roadmap tasks, and 36 Future Revision entries. No blocking question, orphan document, or campaign-data boundary failure remains. Git's checkout LF/CRLF notices are informational; whitespace checks pass.

Repository validation is run after all closure edits and repeated against the staged candidate. Its deterministic distribution check builds two disposable test ZIPs, validates their identical content/hash and exclusions, then removes them; no release/review archive is published by F. Generated build/test/cache files are not staged. The unsigned validator uses an invocation-only execution-policy override, not a machine policy change.

## Closure and Remaining Boundaries

The final audit, vocabulary contract, execution plan, roadmap, register, navigation, and validation expectations agree: **FR-024 complete/Closed, A-F complete**. Phase 13 remains Active, Future Revisions remains the final permanent rolling objective, and FR-025/026/034 plus all other pending objectives remain unselected. No unresolved FR-024 architectural question remains.

No runtime query/search/ranking/top-K, synonym generator, embedding, Compiled Rule Store, acquisition/import provider, release packaging, migration, VERSION change, or tag movement is included. `VERSION` remains `1.0.0`; historical `v1.0.0` and moving discovery `v1.1.0-rc` remain at their starting identities. A normal owner-requested `main` push follows the focused closure commit; no force-push or downstream implementation is authorized.

Live host/SQL deployment, runtime retrieval performance, and final v1.1 acceptance were not exercised and are not claimed by offline/compiler tests. Closure establishes only the approved FR-024 compiler/vocabulary/audit boundary.
