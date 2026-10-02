# FR-026C Deterministic Ranking Audit

## Authority and Checkpoint

Owner-authorized C only starts at clean `main` / `origin/main` commit `ece4fe7718f65a98ca52887e2d954f760a1aea12`, checked against the configured GitHub branch directly. The [execution plan](../FR_026_EXECUTION_PLAN.md), [A audit](FR_026A_RETRIEVAL_CONTRACT_AUDIT.md), [B audit](FR_026B_CANDIDATE_MATCHING_AUDIT.md), FR-022 metadata and FR-024 reviewed strengths were inspected before implementation. A/B are preserved, not redesigned. FR-026 remains selected/incomplete; C is a checkpoint, not a new revision. Phase 13 stays Active and Future Revisions stays last. VERSION is `1.0.0`; release `v1.0.0` stays `6938bd2`, moving discovery tag `v1.1.0-rc` stays `4eb2583`. No tag/release action is authorized.

## Architecture and Exact Formula

```text
A prepared request -> B immutable applicable unranked candidates
                   -> C exact reviewed-weight scores and ordered explanations
```

New portable `CompiledRuleRanking.cs` consumes only `CompiledRuleCandidateSet`. `Rank` emits `RankedCompiledRuleCandidateSet` with unchanged input/scope/counts and an ordered read-only collection. Each `RankedCompiledRuleCandidate` references the existing frozen candidate, without copying executable text or metadata into a second graph. No project, dependency, interface, provider, file read, compilation, SQL adapter or migration is added. The [retrieval contract](../../docs/rules/COMPILED_RULE_STORE_RETRIEVAL.md#deterministic-ranking) owns the portable semantics.

For candidate c, let M(c) be B's distinct matched `(term, kind, weight)` evidence:

```text
VocabularyScore(c) = sum(weight for each distinct (term, kind) in M(c))
order = VocabularyScore descending -> authored Priority descending
        -> SnippetId ordinal ascending
```

No hidden numeric component exists. The score is checked Int64 addition, not float, saturation or a normalized relevance percentage. Format-1 weights are integers 1-1000; negative/zero weights are invalid, while an empty match set legitimately scores zero. An Int32-sized list at maximum weight has at most 2,147,483,647,000 score, safely within Int64 but not Int32. The capacity test verifies this bound and 1,000 legitimate same-word/different-concept contributions without allocating a multi-million-entry fixture merely to cross Int32. Impossible checked overflow maps to the existing fixed inconsistent-artifact failure; there is no arbitrary valid-association cap.

| Available signal | C decision and reason |
| --- | --- |
| Reviewed weight | Primary relevance, unchanged once per distinct matched term/concept. |
| Concept multiplicity | Same wording under distinct legitimate kinds counts separately; B retains both reviewed associations. |
| Multiple query entries | Sum their actual distinct contributions; no extra topic-coverage or frequency bonus. Prepared query duplication cannot multiply evidence. |
| Canonical/alias/phrase origin | Not present in runtime format-1 terms; no inferred origin label or bonus. `kind` is the reviewed concept ID. |
| Authored priority | Higher is preferred only on equal vocabulary scores; compatible numeric direction with legacy ordering without overriding relevance. |
| Preparation tier/readiness | Carried on the frozen source but excluded from relevance. Urgency/availability is not query correspondence. |
| Mandatory Kernel/procedure | Independent reason, zero if unmatched; genuine reviewed matches contribute normally. No forced giant score. |
| Explicit source/snippet | Independent reason, no fake match; all selected candidates survive regardless of score. |
| Always-include | Independent reason, not an applicability waiver or relevance boost. |
| Layer, dependencies, token expense | No score/order component, closure expansion, readiness check or truncation. |

Priority is not added to the sum: even priority 10,000 with no match follows a one-point reviewed match at priority -10,000. Stable snippet identity resolves the final tie; format-1 already orders snippet IDs ordinally, so this matches its neutral order without depending on candidate enumeration. The comparator uses `CompareTo`, not potentially overflowing subtraction. Preparation-tier changes may change artifact semantic scope but leave ranking/evidence over the same associations unchanged.

## Evidence, Safety and Compatibility

`Matches`, `Reasons`, `VocabularyScore`, `Priority` and `SnippetId` expose exactly why X precedes Y. Match weights are the individual contributions; the score is their sum; reasons explain membership rather than numeric bonuses. The source/snippet graph remains available service-side for D but is excluded from implicit ranking JSON diagnostics. No explanation writer/protocol, compiler-origin audit payload or normal player-facing diagnostics is introduced. Bounds come from the valid artifact and A query, not a new cap that hides legitimate contributions.

B owns artifact validity, scope and applicability. C neither repeats FR-022 validation nor rematches query terms; it cannot resurrect B's excluded candidates. Local scoring guards reject null evidence, invalid priority/weights/reasons, duplicate candidate identity, inconsistent source ownership, mismatched vocabulary reason and strict-order/duplicate term-kind violations. Even equal-weight duplicated keys fail; different concept keys remain valid. Fixed `RULE_RETRIEVAL_ARTIFACT_INCONSISTENT` has no raw input or inner exception. Internal friend-assembly fixtures test corrupt states without adding a public bypass of B.

Ranking reuses B's frozen immutable graph and read-only match collection. Caller DTO mutations after B cannot affect ranking. The new ranked list has its own read-only view. Cancellation checks occur before work, within candidate/evidence loops and before/after synchronous sorting. An already-cancelled token takes precedence over invalid input; during-work cancellation retains its token and exposes no partial successful result. The comparator does not throw cancellation inside framework sort wrapping. Caller cancellation during the synchronous sort is observed at its exit boundary.

Legacy Managed `RuleCompilation.Select` orders layer first, then its score (authored priority plus preparation 6000/5000/4000/3000/2000/1000, operation +100 and matching topics +10), then ordinal chunk ID. There is **no raw-prose frequency/keyword scan or synonym generator in that inspected scorer**. C reuses larger-is-higher priority and ordinal identity, not its layer/preparation/operation/topic boosts. Existing legacy code itself is unchanged; F owns eventual adaptation. No claim of matching old rankings is made where the new reviewed-weight authority differs.

## Canonical Ranked Evidence

Executed canonical requests use `NORMAL`, no world/modules and the explicit union of authored source topics. Operation is `gameplay.resolve` unless shown otherwise. All directly matched entries below have reason `ControlledVocabulary`; none is an extra inferred candidate. Within each group, listed IDs are in ranked order.

| Query | Candidates | Applicable hits | B-excluded hits | Positive score components and ranking |
| --- | ---: | ---: | ---: | --- |
| `fighting` | 15 | 4 | 0 | D1-D4 below: each `fighting/combat-context:700`, score 700, priority 150. |
| `conflict` | 15 | 4 | 5 | D1-D4: each `conflict/combat-context:600`, score 600, priority 150. The five excluded hits do not enter C. |
| `save` | 12 | 1 | 0 | `core-context-assembly#manual-persistence-commands`: `save/manual-save:600`, score 600, priority 300. |
| `preparation` | 14 | 3 | 0 | E1-E3 below: each `preparation/evolution-readiness:600`, score 600, priority 150. No preparation-tier bonus. |
| `campaign canon`, `context.assemble` | 7 | 2 | 0 | `core-persistence-authority#campaign-canon`, then `core-persistence-authority#rules-authority-and-persistence-authority`: each `campaign canon/campaign-canon:900`, score 900, priority 300; then five Kernel roots. |
| `unreviewed` or empty query | 11 | 0 | 0 | All eleven gameplay roots below score zero, priority 1000, ordinal ties; no full-corpus fallback. |
| `fighting`, `conflict`, `save`, `preparation` together | 19 | 8 | 5 | D1-D4 each score 1300 = fighting 700 + conflict 600 (priority 150); then manual-persistence-commands score 600 (priority 300); then E1-E3 score 600 (priority 150); then eleven zero-score roots. |

The reviewed development group D1-D4 is:

1. `core-development#contextual-capability-assessment`
2. `core-development#power-preparation-and-victory`
3. `core-development#why-there-is-no-universal-character-level`
4. `core-development#worked-examples`

The reviewed evolution group E1-E3 is:

1. `core-monster-evolution#gradual-evolution`
2. `core-monster-evolution#route-readiness`
3. `core-monster-evolution#the-evolution-transition`

For the four individual gameplay queries and their combined query, the following zero-score roots follow the positive candidates. All have priority 1000. The first six carry `GmRuntimeProcedure | AlwaysInclude`; the last five carry `RuntimeKernel | AlwaysInclude`. They are not eleven search hits and are not removed because they score zero:

1. `gm-runtime-procedure`
2. `gm-runtime-procedure#efficient-execution`
3. `gm-runtime-procedure#gameplay-interaction-lifecycle`
4. `gm-runtime-procedure#one-input-one-interaction`
5. `gm-runtime-procedure#purpose`
6. `gm-runtime-procedure#related-documents`
7. `runtime-kernel`
8. `runtime-kernel#kernel-invariants`
9. `runtime-kernel#purpose`
10. `runtime-kernel#related-documents`
11. `runtime-kernel#retrieval-handoff`

The context-only Canon example includes the five Kernel roots, not the gameplay-only procedure. Tests compare C membership exactly to B even with shuffled candidate enumeration; representative table counts are regression evidence, not production constants. Fixtures also cover explicit required identities both unmatched and matched, always-include separately, genuinely matched Kernel/procedure, shared wording under multiple concepts, controlled breadth and multiple query aggregation. These results prove explainable reference semantics, not G/H's final gameplay-quality/packet acceptance.

## Canonical Artifact and Executed Validation

Compile/write regression remains **10 sources / 154 snippets / 773 terms / 275,622 bytes**. Semantic digest is `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`; serialized byte SHA-256 is `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`. Exact artifact bytes, source/content hashes, snippet IDs and FR-022 validation are unchanged. C does not change compiler, reviewed vocabulary, manifest or format semantics.

| Validation executed in this task | Result |
| --- | --- |
| Portable, CLI and Managed Release builds with existing restored dependencies | Pass, zero warnings/errors; no package added. |
| C focused cases | 75/75, zero failures/skips; includes eight representative canonical requests. |
| A/B focused regressions | 216/216 (59 + 157). |
| Complete portable / CLI-process / SQL-enabled Managed | 860/860 / 64/64 / 208/208: **1,132 unique tests**, zero failures/skips. |
| FR-025 / FR-024 / FR-023 focused regressions | 264/264 / 235/235 / 70/70. |
| FR-022 contract + portable dependency boundary / CLI dependency boundary | 17/17 / 1/1. |
| Nine structural harnesses | 333 assertions (13/29/19/48/59/81/35/32/17), all pass through repository validation. |
| Pre-documentation full repository/distribution validation | Pass: 310 Markdown files, 7,489 links, 218 anchors; zero blocking questions, orphans or forbidden campaign directories; deterministic disposable review ZIPs and exclusions checked. |

Release commands use `dotnet build`/`dotnet test -c Release --no-restore`, reusing restored dependencies. Targeted filters name actual A/B/C, FR-022/023/024/025 and boundary test classes; focused totals overlap full-suite counts. SQL validation sets `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1` and executes real disposable LocalDB fixtures, not campaign data. Repository validation uses invocation-only `-ExecutionPolicy Bypass`; no persistent machine policy change. Generated bin/obj/test/cache files remain ignored and excluded from source/distribution.

The first C run passed 69/75. Five test expectations compared the entire serialized scope after intentionally changing preparation tier, which correctly changes artifact identity; they now compare candidate ranking/evidence instead. One expected zero-score root order omitted the authored priority tie-break. Only tests were corrected; production ranking did not change to manufacture a pass. The rerun passed 75/75; full suites and additional focused checks above followed.

Post-documentation and exact-staged full repository validation pass: 311 Markdown files, 7,499 links, 219 anchors; 192 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. All nine harnesses and deterministic distribution checks pass; zero blocking questions, orphans or forbidden campaign directories. Exact-stage C rerun passes 75/75. Complete diff review confirms C-only scope, no eligibility change, closure, budgeting, SQL/migration, debug/temporary artifacts, credentials, private machine paths, Campaign data or release edits. Working content matches the staged tree with no untracked files; cached whitespace passes. This result recording is restaged and focused/repository checks repeated before commit. No earlier-session result is treated as proof of this tree. Git LF-to-CRLF notices are informational, not whitespace failures.

## Classification and Remaining Boundaries

- **A - Reference implementation:** .NET get-only/read-only result views, Int64 sum, ordinal comparator, local guards and deterministic test fixtures; no SQL/migration/provider/host implementation change.
- **B - Managed contract:** eligible inputs only, reviewed association contributions, complete deterministic ties, preserved scope/identity/reasons, bounded safe failures and cancellation; later storage adapters must reproduce the portable semantics rather than a SQL-dependent scorer.
- **C - Eternal Cycle-wide:** navigation metadata cannot alter rule meaning or waive applicability; mandatory availability and actual query relevance remain distinct. Campaign Canon and player agency are untouched.

Dependencies remain on the candidate graph but are not expanded or scored. The token budget is preserved but never consumed/truncated; no packet, SQL query/index or migration 012 is added. F's runtime/SQL integration, G's broader canonical quality/cost evidence, H's integrated acceptance and real host/remote deployment/performance remain unvalidated future work, not completed C capabilities. No prerequisite conflict or unresolved C decision remains. A-C are complete; D-H pending; FR-026 selected/incomplete and FR-027 unselected. Stop after exact-state validation, one focused commit and normal main push; no new objective, release, archive, VERSION or tag action.
