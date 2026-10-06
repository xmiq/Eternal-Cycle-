# FR-026 Integrated Retrieval Acceptance Audit

## Decision and Recovered Checkpoint

**Later R1 design review:** [clause inventory, source/identity maps and revised projections](FR_026H_R1_AUTHORITY_PARTITION_REVIEW.md). The provisional routine subset requires additions, the conservative graph still misses 8K acceptance, and R2 is not ready. This note does not revise the executed H results below.

**BLOCKED. FR-026 remains selected/incomplete; A-G remain complete, H is not complete.** Deterministic failure is correct implementation behavior but is not sufficient acceptance of a canonical corpus that rejects supported gameplay rule requests. No closure commit or push is authorized by this result.

H and its recovery instructions continue from clean synchronized `main` at `d0d70e9a755d5bb3860dad86611f7c89e2fde734`. Recovery inspection found no staged, unstaged or untracked H files, no H commit, and no intervening push. The repository retained the accepted A-G implementation, not partially written H implementation. No interrupted-run test result is counted as proof; all executed results below were obtained during this recovery pass. Local and remote HEAD still identify that G commit. VERSION is `1.0.0`; local and remote tag objects remain `6938bd2f9e965faaeb1c612b1dd765237cadc8a8` (`v1.0.0`) and `4eb2583027d316a962fd49fe318781a0e060183e` (`v1.1.0-rc`). No history is rewritten.

Completed during H: checkpoint recovery, authority/code/fixture review, all six dependency-failure dispositions below, existing integrated canonical SQL/reference and quality reruns, compatibility suites, builds/publish checks, and this blocked acceptance matrix. Not completed: unconditional normative acceptance, new final H adversarial fixtures, FR-026 closure/governance transition, exact-stage closure commit, or push. Existing A-G tests are reused as regression evidence; none is renamed or represented as a newly implemented H acceptance test.

## Authority and Architecture

The [FR-026 objective](../V1_1_FUTURE_REVISION_PLAN.md#fr-026---compiled-rule-store-and-intelligent-retrieval), [execution plan](../FR_026_EXECUTION_PLAN.md), [retrieval contract](../../docs/rules/COMPILED_RULE_STORE_RETRIEVAL.md), and decisions D-1367 through D-1372 govern the engine. [Format 1](../../docs/rules/COMPILED_RULES_ARTIFACT.md), [FR-023](FR_023_REPRODUCIBLE_RULES_COMPILER_AUDIT.md), [FR-024](FR_024_CONTROLLED_RETRIEVAL_VOCABULARY_AUDIT.md), and [FR-025](FR_025_ACQUISITION_AND_IMPORT_AUDIT.md) remain closed prerequisites, not permission to redefine runtime applicability. Historical package audits remain unchanged.

```text
trusted imported format-1 artifact -> artifact-owned durable storage
    -> authorized explicit Ruleset + semantic identity, published/active selection
    -> coherent verified SQL readback
    -> A prepare -> B exact applicable matches -> C rank
    -> D complete source closure -> E bounded compact packet
    -> runtime consumer (matching host bootstrap required for gameplay)
G observes quality/cost; H judges acceptance, not a second retrieval engine.
```

| Package | Responsibility / retained evidence |
| --- | --- |
| A | [Bounded immutable request preparation](FR_026A_RETRIEVAL_CONTRACT_AUDIT.md). |
| B | [Exact reviewed matching and all-selector applicability](FR_026B_CANDIDATE_MATCHING_AUDIT.md). |
| C | [Checked reviewed weights, priority and ordinal ranking](FR_026C_RANKING_AUDIT.md). |
| D | [Complete artifact-local dependencies](FR_026D_DEPENDENCY_CLOSURE_AUDIT.md). |
| E | [Required reservation, whole optional closures and compact presentation](FR_026E_CLOSURE_AWARE_PACKET_AUDIT.md). |
| F | [Authorized durable/runtime retrieval and readiness](FR_026F_SQL_RUNTIME_RETRIEVAL_AUDIT.md). |
| G | [Deterministic quality and local context-cost evidence](FR_026G_RETRIEVAL_QUALITY_COST_AUDIT.md). |
| H | This integrated review finds a canonical modeling blocker; closure withheld. |

## Dependency-Failure Review

All six fixtures in [G's authored cases](../../examples/tooling/rules-compiler-dotnet/fixtures/compiled-rule-retrieval/quality-cases.json) request `gameplay.resolve`, `NORMAL`, budget 8,000, no world or modules, and the explicit union of canonical topics. Each listed root is a real reviewed exact match, correctly B-applicable under the existing manifest. Each requires the same source edge: **`core-context-assembly -> core-persistence-authority`**. The prerequisite has no world/module restriction and permits `NORMAL` through `*`. The incompatible dimension is solely **operation**: its operations are `context.assemble`, `persistence.read`, `persistence.commit`, excluding `gameplay.resolve`.

Each context-assembly root below has priority 300. All selected context-assembly roots are listed, not only the first root at which traversal fails. Other roots retain their existing scores, including mandatory Kernel/procedure; they cannot rescue the incomplete request.

| G fixture / query | Context-assembly roots and match scores | H disposition |
| --- | --- | --- |
| `persistence-save-failure` / `save` | `#manual-persistence-commands`, 600 | Same modeling incompatibility. A standalone manual save is persistence work, not necessarily a new gameplay action; that distinction alone cannot establish the gameplay blocker. Nevertheless the manifest explicitly admits this source during gameplay, and this failure is not a documented prohibition on reading save rules. Retain the case; no invented rerouting. |
| `broad-known-failure` / `fighting`, `conflict`, `save`, `preparation` | `#manual-persistence-commands`, 600 | Same incompatibility. Query terms do not request four simultaneous executable operations or confer authority; A accepts multiple reviewed entries. The save match invalidates the whole otherwise applicable closure. Not evidence that combined queries are intrinsically unsupported. |
| `gameplay-procedure` / `gm adjudication` | `#gameplay-turn-state-machine`, 800 | **Supported gameplay request blocked.** This text defines the read/resolve/persist/yield lifecycle. It is not an administration-only procedure or forbidden operation combination. |
| `player-agency` / `wait for player input` | `#core-invariants`, `#gameplay-turn-state-machine`, 800 each | **Supported gameplay safeguard request blocked.** These roots govern actual-input boundaries and yielding; no new player action or permission is requested. |
| `knowledge-visibility` / `gm secrets` | `#current-scene-context`, `#purpose-specific-visual-context`, `#relevance-selection`, 800 each | **Supported gameplay context request blocked.** This is reusable visibility guidance, not retrieval of private Campaign Secrets. The source governs filtering during gameplay and representation; no protected campaign data is an input. |
| `world-state-phrase` / `population and ecology` | `#resolution-and-affected-set-gate`, 800 | **Supported gameplay consequence request blocked.** The authored association reaches world-state/Affected Set guidance, which explicitly includes relevant world changes after resolution. It does not request a different World Model or unsupported simulation host. |

All `#` anchors above have prefix `core-context-assembly`. Exact reviewed term/concept/weight evidence is retained in [G's deterministic report](../../examples/tooling/rules-compiler-dotnet/fixtures/compiled-rule-retrieval/expected-quality-report.json); H reran the golden comparison without enabling report regeneration.

### Acceptance Judgment

The dependency relation is justified by the [context-assembly contract](../../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md#context-assembly-layer): gameplay reads and updates Campaign Canon under ownership, visibility, uncertainty and persistence constraints. [Persistence Authority](../../docs/persistence/PERSISTENCE_AUTHORITY.md#authority-is-claim-specific) governs those claims; its rule text does not prohibit application during gameplay. The [GM Runtime Procedure](../../docs/rules/GM_RUNTIME_PROCEDURE.md#gameplay-interaction-lifecycle) requires the smallest dependency-complete rules, relevant Canon, Affected Set, validated persistence and player-agency yield. These requirements make at least the four ordinary gameplay/safeguard cases valid supported retrieval purposes.

The prerequisite's restrictive operation metadata therefore does not accurately model its necessity for a source explicitly applicable to gameplay. This is a **canonical Rule Source applicability/dependency modeling issue**, not an SQL readback, ranking, normalization, budget, or dependency-traversal implementation defect. D correctly honors D-1370 and returns `RULE_RETRIEVAL_DEPENDENCY_FAILED`; F reproduces it without fallback or partial packet. That safety behavior passes. FR-026's separate normative acceptance that intended alternative terminology retrieves applicable rules does **not** pass for these supported requests. They cannot all be accepted merely because G expected deterministic errors.

No existing contract prescribes splitting or relabeling these gameplay requests as a workaround. `context.assemble` is an available separate operation, not permission to fabricate one successful `gameplay.resolve` closure from incompatible packets. H does not recommend omission, another artifact, legacy fallback, dropping a term, forcing a prerequisite, or returning partial context.

There is also a concrete correction constraint, not a proposed semantic fix: G's verified authority source has 24 snippets costing 6,385 estimate units. Gameplay mandatory Kernel/procedure costs 2,027. Their disjoint union already costs **8,412**, before adding a context-assembly root. Simply adding `gameplay.resolve` to the prerequisite would not prove useful admission under the 8K ceiling: a whole optional closure would be excluded, and a required one would overflow. This is an arithmetic implication of accepted D/E semantics, not a claim that such a modified artifact was compiled or tested. Neither selectors nor budget/granularity were changed.

### Ownership and Minimum Next Action

- **A - reference implementation:** .NET compatibility checks and SQL reproduce the authored incompatibility; no reference implementation accident is generalized.
- **B - Managed contract:** every selected source needs a complete executable compatible closure under the approved envelope. SQL cannot substitute or weaken the shared engine.
- **C - Eternal Cycle-wide authoring/process:** derived Rule Source classifications must faithfully support canonical operating procedures. Correcting canonical applicability/dependency granularity requires scoped maintainer approval and source-level review, not a green-test selector edit.

The minimum next package is an owner-authorized **FR-026 canonical closure-model correction checkpoint**, still beneath FR-026, not a new Future Revision. Review operation applicability and source-level dependency granularity against actual authority text and the 8K envelope; decide the smallest coherent correction before implementing it. Preserve D/E safety. Then regenerate intentionally changed artifact/report evidence with immutable provenance, retain old baselines as history, test all six cases and positive/negative topology, SQL/reference equality, required closure/cost and compatibility, and rerun H. Do not rewrite old imported artifacts or Campaign data. No particular selector change, larger ceiling, new format or migration is preapproved by this audit.

## Final Requirement Matrix

`Regression-pass` means the named accepted boundary was inspected and its existing tests ran on H's unchanged production tree. It does not claim full FR-026 closure or new H adversarial coverage. `Blocked` identifies failed normative integrated acceptance; remaining final H-only coverage/closure stays uncompleted.

Portable implementation/tests are under [Rules](../../examples/tooling/rules-compiler-dotnet/); SQL/runtime implementation/tests are under [Managed reference](../../examples/tooling/managed-data/mcp-dotnet-tsql/). These short names identify concrete files, not independent authority.

| Normative requirement | Owner / implementation | Executed evidence | H result |
| --- | --- | --- | --- |
| Explicit Ruleset/semantic identity, exact artifact scope | A/B/F; `CompiledRuleRetrieval`, candidate index, SQL scope lookup | A request scope, B history isolation; `F_HistorySameSourceIdentitiesIdempotencyAndSelectionIsolation`, G scoped queries | Regression-pass. |
| Immutable bounded query, safe failures | A; `CompiledRuleRetrieval.Prepare` | 59 A cases: bounded raw counts, exact IDs, private read-only sets, cancellation | Regression-pass. |
| Controlled reviewed terms, whole phrase/alias matching, no inference | A/B; FR-024 normalization and candidate index | B reviewed topology; G exactness, normalized inputs, partial/substrings/stems/fuzzy/hyphen negatives | Regression-pass; corpus delivery blocked below. |
| Applicability, world/module/mode/operation/topic isolation | B/D; candidate compatibility | B selector matrix, D executable compatibility; F SQL selectors | Engine regression-pass; canonical prerequisite operation modeling blocked. |
| Mandatory Kernel/procedure, explicit identities, always-include | B/E | Missing mandatory structure, exact required identities, every independent reason; G 407 required-root obligations | Regression-pass on successful fixtures; no closure claim for failed requests. |
| Distinct concept identity, exact reviewed weights | B/C; `(term, kind)` matches | Same wording/two concepts, G Unicode 700+600, F full metadata readback | Regression-pass; no origin synthesis. |
| Checked Int64 score, priority, ordinal tie-break; no other bonus | C; `CompiledRuleRanking` | 75 C cases, G synthetic ties/tier/always-include, exact weight sums | Regression-pass. |
| Complete direct/transitive source dependencies | D; `CompiledRuleDependencies.Expand` | 61 D cases and F/G direct/chain/shared/diamond graphs | Regression-pass; all snippets of a prerequisite, artifact-local. |
| Shared/root-also-dependency, parent evidence, no duplicate text/cost | D/E | Shared/diamond/root-dependency fixtures; G 1,126 obligations | Regression-pass for successful closures. |
| Deep chain, missing/cyclic/incompatible fail closed | D | 4,096-source chain, dense graph, defensive corrupt graphs, canonical/F failure equality | Safety regression-pass; legitimate canonical requests still blocked. |
| Bounded budget and required reservation | E; `CompiledRulePackets.Build` | Exact 2,027/1,010 floors and one-below failures; overflow fixtures | Regression-pass; no partial packet. |
| Whole optional admission, skip then continue, shared charge once | E | 65 E cases and F/G graph budget equivalence | Regression-pass; correction must address 8,412 prerequisite floor. |
| Prerequisite-first presentation distinct from rank | D/E | Exact executable content/order, complete support and ranked root projection | Regression-pass. |
| Compact packet, service evidence not model text | E/F; packet/tool boundary | `CompactModelPayloadGroupsTextAndKeepsAllRichEvidenceServiceSide`, F canonical compact tool response | Regression-pass. |
| Lossless durable retrieval and integrity | F; 011 readback/verification | `F_ReadbackRetainsEveryCanonicalFieldAndExactBytes`, nine corruption variants | Regression-pass; 773 terms retained. |
| Runtime/admin/publication authorization before disclosure | F; `CompiledRuleRuntime` | Disabled/wrong Ruleset before SQL, separate informed consent/operator gates | Regression-pass for trusted single-user reference; not remote multi-user authentication. |
| Import/publication/activation separation and idempotency | FR-025/F; 011/012 | Import does not publish, publication does not activate, repeated/concurrent activation tests | Regression-pass. |
| Explicit active artifact, no newest/mixed history | F; serializable exact lookup and eligibility | History/reimport, switch back, missing/inactive scope cases | Regression-pass. |
| Selected host readiness; not whole-game readiness | F/FR-021; bootstrap hash acknowledgment | Four F configuration states/stale hash, non-gameplay difference; legacy readiness regressions | Existing tested readiness scenarios pass; final H acceptance not closed. |
| Historical artifact-local terms/dependencies/content/order | FR-025/F | Same source/snippet IDs with changed bytes; reverse row reinsertion; exact semantic projection | Regression-pass. |
| Provider independence and disappearance | FR-025/F | Local/simulated GitHub/custom convergence, offline readback/retrieval after source disappearance | Regression-pass; no runtime reacquisition. |
| Transactional coherent read, bounded set-based SQL | F/G | Serializable readback, six artifact-scoped queries plus gameplay acknowledgment, no N+1/history mixing | Regression-pass; complete artifact loaded per request, no selective database cost claim. |
| Corruption and identity mismatch fail without partial/fallback | FR-025/F | Bytes/hash/header/source/terms/edge/snippet/ownership/ordinal corruption and scope failures | Regression-pass; no implicit repair. |
| Cancellation across SQL and A-E | A-F | Work-boundary cancellations, actually blocked SQL read, no successful partial result | Regression-pass. |
| Relevant concurrency/idempotency | FR-025/F | Concurrent import, publication/activation and six concurrent reads alongside unrelated import | Regression-pass on disposable LocalDB, not production load acceptance. |
| Determinism and immutability | A-G | Culture, CWD, relocation, enumeration, process/golden report and fresh database repetitions | Regression-pass. |
| Useful canonical alternative-term retrieval | B-G plus H review | 54 G cases, all six known dependency failures reproduced by F | **Blocked**: at least four supported gameplay rule purposes cannot produce a packet. |
| Positive and negative quality evidence | B/G | 81/81 direct positives, 109/109 direct negatives | Matching regression-pass; these are not 81 delivered-packet successes. |
| Required roots and prerequisite quality | D/E/G | 407/407 and 1,126/1,126 on 45 successful packets | Regression-pass; denominator excludes failed requests. |
| Over-retrieval, unknown and empty independently | B/E/G | Separate unknown/empty cases: 11/11 members, 2,027 cost; successful maximum 31/154 members | Regression-pass; no whole-corpus/fuzzy fallback. |
| Local context-cost evidence | E/G | 56,398 corpus; fighting full 4,746/constrained 2,985, deterministic reports | Regression-pass; not external tokenizer or FR-031 accounting. |
| Legacy retrieval/readiness unchanged, no silent fallback | F/FR-021 | Legacy context/state snapshots, Pending priority, mandatory procedure, explicit corrupted compiled rejection | Regression-pass. |
| FR-022/023/024/025 compatibility and frozen artifact | A-G/import/compiler | Full portable/CLI/SQL suites, frozen artifact exact bytes/counts/digests | Regression-pass; no contract/compiler/vocabulary edits. |
| Governance, integrated closure, downstream handoff | H | This matrix and dependency disposition | **Blocked**; no FR-027 selection or accepted whole-objective handoff. |

## Canonical SQL and Reference Outcomes

H re-executed `F_CanonicalFullSemanticEquivalence`, `F_CanonicalFailureEquivalence`, and `G_AllCanonicalQualityFixturesMatchSQLWithBoundedArtifactLocalReads`. The latter loops all 54 cases through actual imported/published/active LocalDB data, including all six reviewed failures. Its complete F projection compares packet/text/identity/order, B counts, ranked roots/scores/contributions/reasons, D full source/snippet semantics/support/groups, E decisions/exclusions/totals; not counts alone. Failed results compare stable failure code/category, never raw exceptions. New semantic outcomes were not invented from static golden files.

| Query | Budget | Roots/members | Used | Remaining | Excluded |
| --- | ---: | --- | ---: | ---: | ---: |
| fighting | 8,000 | 15/15 | 4,746 | 3,254 | 0 |
| fighting | 3,000 | 13/13 | 2,985 | 15 | 2 |
| conflict | 8,000 | 15/15 | 4,746 | 3,254 | 0 |
| conflict | 3,000 | 13/13 | 2,985 | 15 | 2 |
| preparation | 8,000 | 14/14 | 3,430 | 4,570 | 0 |
| preparation | 3,000 | 13/13 | 2,979 | 21 | 1 |
| Campaign Canon/context | 8,000 | 7/7 | 1,801 | 6,199 | 0 |
| Campaign Canon/context | 1,400 | 6/6 | 1,394 | 6 | 1 |
| unknown | 8,000 | 11/11 | 2,027 | 5,973 | 0 |
| empty | 8,000 | 11/11 | 2,027 | 5,973 | 0 |

Conflict's five inapplicable hits remain excluded; context's two 900-point roots retain exact contributions. Gameplay 2,027 and context Kernel 1,010 fit exactly; 2,026/1,009 fail without partial output. `save`, the combined four-term query, and all four additional cases fail identically in SQL and reference D. Their identical failure confirms implementation parity, not usefulness acceptance.

G's report remains 54 cases, 17 categories, all ten sources represented, 185 applicable/36 inapplicable hits; 45 successful packets and nine observed expected failures. H reclassifies six dependency failures as the corpus closure-model finding above; three budget failures remain correct defensive outcomes. The 81/109/407/1,126 assertion denominators are unchanged evidence, not a cosmetic acceptance grade. Full fighting avoids **51,652/56,398** units; constrained fighting avoids **53,413/56,398**. The estimator sums stored normalized-content UTF-8 estimates; envelope/separators/other context and external tokenizer differences are excluded.

Canonical artifact stays **10 sources / 154 snippets / 773 retrieval terms / 275,622 bytes**. Semantic digest: `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`. Serialized byte SHA-256: `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`. The actual compile/readback tests validate both; these are not production constants.

## Validation Executed During H

| Check | Actual result before recording blocker governance |
| --- | --- |
| Portable library, CLI, Managed Release builds | All pass; zero warnings/errors. |
| Complete portable / CLI-process / SQL-enabled Managed | 1,051/1,051 / 64/64 / 284/284; **1,399 unique tests**, zero failures/skips. |
| Focused portable A-G retrieval regression classes | 482/482; includes 417 A-E and 65 G cases. |
| Focused Managed `F_`/`G_` name filter | 78/78; overlapping full-suite evidence, not extra unique tests. |
| Legacy GM corrective / first-run / progressive-readiness classes | 49/49; overlapping full-suite evidence. |
| FR-022/023/024/025, boundary, migrations/import/history | Included in the executed complete portable, CLI and SQL-enabled Managed suites; no claim that each historical focused count was separately rerun. |
| Managed Release publish/package verification | Pass; metadata, Apache LICENSE/NOTICE and both 012 scripts byte-match source; independent worker executable present. Ignored build output only. |
| Repository validator before audit/governance edits | Pass: 315 Markdown, 7,550 links, 223 anchors, 192 canonical documents, 15 family indexes, 43 templates, five roles, 1,276 terms, 208 tasks, 36 FR entries; zero recorded blocking questions/orphans/forbidden Campaign directories. |
| Nine structural harnesses / deterministic distribution | 333 assertions pass (13/29/19/48/59/81/35/32/17); validator checks two disposable deterministic archives and removes them. No retained release archive. |

Commands use `dotnet build/test/publish -c Release --no-restore`; focused tests reuse those built outputs with `--no-build`. SQL runs set `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1` and create/drop generated disposable LocalDB databases. No Campaign database is touched. Repository validation uses `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate_repository.ps1` without persistent machine-policy changes. Report-generation environment variables are not set; tests compare existing golden bytes.

### Final Documentation Checkpoint

After assembling the blocked audit and governance state, all three Release builds were rerun successfully with zero warnings/errors. The complete portable, CLI/process and SQL-enabled Managed suites were rerun: **1,051/1,051, 64/64 and 284/284**, again zero failures/skips. Repetition does not increase the 1,399 unique-test total. Production code and tests are unchanged, so these runs cover the same exact executable state as the focused checks above.

The final repository validator returns **FAIL with exactly one issue: `Blocking unresolved questions remain.`** All nine structural harnesses still pass their 333 assertions; no link, navigation, contract, forbidden-data or deterministic distribution error is reported. This is the existing governance gate honestly rejecting the newly recorded blocker, not an implementation regression or a weakened check. The earlier 315-document/7,550-link/223-anchor totals belong to the pre-audit successful run; the failing final run does not print those totals and is not represented as a pass.

Full tracked diff review and `git diff --check` found no whitespace errors. The new audit was also checked as a no-index diff against `NUL`, with no whitespace findings (that command reports the expected new-file difference). Git's LF-to-CRLF warnings reflect repository checkout policy, not content errors. Scope is eight documentation files only: this new audit, its index, execution plan, roadmap, Future Revisions, v1.1 plan, blocking question and Developer Notes. No accidental generated or interrupted source files are included. No files were staged because acceptance is blocked and no closure commit will be made; there is no staged-state success claim.

Final remote verification confirms `main` remains `d0d70e9a755d5bb3860dad86611f7c89e2fde734`, equal to local HEAD/upstream (`0/0`), with both tag objects unchanged. VERSION remains `1.0.0`. The eight focused documentation edits are retained uncommitted for review; no commit or push occurred. A green regression suite and a blocked normative H decision are compatible: current tests deliberately characterize frozen failure outcomes.

## Corrective Architecture Investigation

The subsequent [dependency-granularity investigation](FR_026H_DEPENDENCY_GRANULARITY_INVESTIGATION.md) maps all 32 context-assembly and 24 authority snippets, evaluates A-H alternatives, and records 36 real A-E sensitivity invocations plus 24 explicitly limited arithmetic projections. It recommends semantically bounded authority sources with selective context-source partitioning, preserving format-1 whole-source semantics, subject to maintainer approval and fresh corrective acceptance. Selector widening alone excludes the requested context roots at 8K. No source partition or contract change is implemented; the blocker remains active. This supplements, not replaces, the historical H validation above.

## Limits, Governance and Stop

No production code, migration, canonical prose, vocabulary, selectors, dependencies, ranking, estimator, budget, importer, packet, readiness or publication changes. No new canonical decision is made. The [blocking question](../UNRESOLVED_QUESTIONS.md#blocking) and current governance now expose the acceptance issue instead of falsely closing it. Historical audits and golden failures are preserved.

Still unproven: live MCP/AI-host acceptance; remote SQL deployment/authentication and multi-user grants; production-scale concurrency/performance; live GitHub/private-auth/CDN transport; external-tokenizer equivalence; whole-runtime context accounting; Campaign binding. F's previously recorded short-lease worker flake did not recur in H's full run; that is not proof of its historical cause or resolution. All current normative FR-026 acceptance cannot be claimed, and no new final H adversarial suite was completed after the blocking finding.

FR-027 may inspect the implemented A-G interfaces/history, but receives **no completed-FR-026 handoff** and remains pending/unselected. The same applies to FR-031. Phase 13 stays Active and Future Revisions stays `[∞]`. Stop at the documented blocker: preserve the focused audit edits for review, do not create a closure commit or push, and do not start a correction package or downstream objective without owner authorization. VERSION, release/RC tags and canonical artifact history remain untouched.
