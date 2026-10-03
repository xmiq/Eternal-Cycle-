# FR-026E Closure-Aware Packet Audit

## Checkpoint and Scope

2026-10-03. Starting clean synchronized `main`: `ffa54e274eb940ffdc22ffd63798bad33833cc8a`. [FR-026](../FR_026_EXECUTION_PLAN.md) remains the sole selected incomplete objective. E is an execution checkpoint, not another Future Revision. VERSION `1.0.0`, immutable release `v1.0.0` (`6938bd2f9e965faaeb1c612b1dd765237cadc8a8`) and moving discovery pointer `v1.1.0-rc` (`4eb2583027d316a962fd49fe318781a0e060183e`) stay unchanged. Commit identity is the focused commit containing this audit, not a self-referential embedded SHA.

## Existing Contract Reviewed

Inspected A-D contracts, implementations, focused fixtures and audits; FR-021 GM Procedure/compact-packet regressions; [FR-022 artifact](../../docs/rules/COMPILED_RULES_ARTIFACT.md), [FR-024 vocabulary](../../docs/rules/CONTROLLED_RETRIEVAL_VOCABULARY.md), [FR-025 storage/readback](../../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md), and Managed `RuleCompilation.cs`/`RulePacketFormatter`.

Legacy `RuleContextResult` carries campaign/world/ruleset/repository/release/source identity, estimated and maximum tokens, and rich selected chunks. Its compact format-1 packet keeps that identity once and groups `RuleSourceId`/`Content` with double-LF joins. Rich chunk identities, hashes, applicability and diagnostics are not repeated in model context. The selector admits complete dependencies, charges unique stored chunk estimates, skips nonfitting optional closures and fails on required structure; no silent truncation/overflow is authorized. Kernel precedes optional selection; gameplay Procedure must survive the final gate. Legacy binding requires a Campaign route and published release, neither available from an explicit imported artifact.

E preserves grouped executable text, ordering and estimated-rule accounting. Its smallest portable envelope adaptation uses Ruleset/semantic artifact scope plus repository/immutable source identity, rather than fabricating campaign/world/release bindings. `PacketFormatVersion` remains 1: this is a scoped Derived compact view, not another artifact interchange format. No Managed types, caller, formatter or legacy selection behavior change. F must supply authorized durable scope/readiness and reproduce the same reference membership, decisions, ordering and totals; it must not substitute SQL row order or automatic activation.

## Implementation and Admission

New portable [CompiledRulePackets.cs](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRulePackets.cs) adds `CompiledRulePackets.Build`, `CompiledRulePacket`, grouped sections and immutable service result/member/decision/totals types. It consumes D's complete groups and B's frozen authority, not raw artifacts or a new graph. Local reference, group completeness, direct-parent support and order checks reject mixed/truncated/forged inputs without rematching, rescoring, recomputing closure or full FR-022 revalidation.

1. Reserve the unique complete closure of Kernel, operation-scoped Procedure, explicit required source/snippet and always-include roots, in their relative C order. Insufficient budget throws fixed `RULE_RETRIEVAL_BUDGET_INSUFFICIENT`; no packet.
2. Visit optional roots in C order. Sum only members not already admitted. Equality fits. Commit a whole increment or none; skip an oversized closure and continue. No knapsack, cost/relevance ratio, tier/layer boost or new ranking.
3. Filter D's prerequisite-first union by admitted IDs. Preserve ordinal snippets within each source; group source content with double LF. C decisions retain relevance order and the original root objects, contributions, priorities and flags.

Dependencies never gain fabricated relevance. Shared, transitive and diamond members appear/cost once. Root-also-dependency keeps C evidence and costs zero when already fully present. Rejected closure evaluation leaves no new dependency behind. Packet support filters out budget-excluded parents but retains all emitted direct parents; D's full pre-budget evidence remains available service-side. Support arrays are shared per source, not copied per snippet/path.

## Budget and Evidence

Budget = sum of unique emitted format-1 `estimatedTokens`, dependencies included. Existing compilation estimator is `max(1, (UTF8 byte count + 2) / 3)` on normalized executable text; E uses retained positive estimates rather than a new tokenizer. Headings within snippet content count. Source labels, packet JSON/envelope, double-LF joins and service explanations are separate overhead, not charged here. No exact provider-token or approximately 20K whole-context claim.

Checked Int64 costs prevent Int32 wrap; only affordable costs narrow to bounded admitted totals (1-8,000 caller ceiling). Two `Int32.MaxValue` estimates correctly cost 4,294,967,294: optional closure excluded, required closure fails. No clamp/saturation alters ranking or metadata.

`Packet` alone is model-facing. `Decisions` retain required/selected state, C evidence, incremental cost and remaining budget before admission, in C order. Required decisions occur during reservation, optional decisions afterward. Fixed `RULE_RETRIEVAL_CLOSURE_EXCEEDS_REMAINING_BUDGET` distinguishes E exclusion from B's unmatched/inapplicable counts and D failure. `Members` retain snippet/source identity, root evidence and actual direct support. Totals expose requested/used/remaining/required costs, selected roots, unique members, dependency-only and excluded roots. No audit-origin graph, acquisition evidence, executable content or physical path enters implicit diagnostics. Packet/nested collections are read-only; cancellation at validation, required/optional admission and presentation yields no partial success.

## Canonical Packets

Canonical topics are the corpus's declared topics, mode NORMAL; gameplay operation unless specified. All successful rows have zero dependency-only members. This is E-local deterministic evidence, not G quality/cost acceptance.

| Query / operation | Requested | Used | Remaining | Selected roots / unique members | Excluded roots |
| --- | ---: | ---: | ---: | ---: | ---: |
| fighting | 8,000 | 4,746 | 3,254 | 15 / 15 | 0 |
| fighting | 3,000 | 2,985 | 15 | 13 / 13 | 2 |
| fighting | 2,027 | 2,027 | 0 | 11 / 11 | 4 |
| conflict | 8,000 | 4,746 | 3,254 | 15 / 15 | 0 |
| conflict | 3,000 | 2,985 | 15 | 13 / 13 | 2 |
| conflict | 2,027 | 2,027 | 0 | 11 / 11 | 4 |
| preparation | 8,000 | 3,430 | 4,570 | 14 / 14 | 0 |
| preparation | 3,000 | 2,979 | 21 | 13 / 13 | 1 |
| preparation | 2,027 | 2,027 | 0 | 11 / 11 | 3 |
| campaign canon / context.assemble | 8,000 | 1,801 | 6,199 | 7 / 7 | 0 |
| campaign canon / context.assemble | 1,400 | 1,394 | 6 | 6 / 6 | 1 |
| campaign canon / context.assemble | 1,010 | 1,010 | 0 | 5 / 5 | 2 |
| unknown or empty, each | 8,000 | 2,027 | 5,973 | 11 / 11 | 0 |
| unknown or empty, each | 2,027 | 2,027 | 0 | 11 / 11 | 0 |

Exact full-cost budgets (4,746 / 3,430 / 1,801 / 2,027 respectively) admit all their roots with zero remaining. Below required reservation (2,026 gameplay; 1,009 context) fails, not an overflow packet.

The gameplay required 11 identities, in C decision order, are `gm-runtime-procedure`, its `#efficient-execution`, `#gameplay-interaction-lifecycle`, `#one-input-one-interaction`, `#purpose`, `#related-documents`; then `runtime-kernel`, its `#kernel-invariants`, `#purpose`, `#related-documents`, `#retrieval-handoff`. Packet presentation puts the five Kernel members before the six Procedure members. Context requires those five Kernel identities only.

Optional roots, before required decisions, in C order:

| Queries | Exact root identities | Score each | Incremental costs |
| --- | --- | ---: | --- |
| fighting / conflict | `core-development#contextual-capability-assessment`; `#power-preparation-and-victory`; `#why-there-is-no-universal-character-level`; `#worked-examples` (same source prefix) | 700 / 600 | 665; 293; 394; 1,367 |
| preparation | `core-monster-evolution#gradual-evolution`; `#route-readiness`; `#the-evolution-transition` (same prefix) | 600 | 316; 636; 451 |
| campaign canon | `core-persistence-authority#campaign-canon`; `#rules-authority-and-persistence-authority` (same prefix) | 900 | 384; 407 |

At the partial budgets, first two Development/Evolution roots or first Canon root survive; remaining optional roots are excluded. At required-only budgets all listed optional roots are excluded. Five inapplicable `conflict` hits stay B exclusions, not E budget exclusions. English `preparation` creates no preparation-tier bonus.

## Expected Failures and Synthetic Evidence

`save` yields 12 C roots; the combined fighting/conflict/save/preparation request yields 19. Both retain the 600-point `core-context-assembly#manual-persistence-commands` root, but D fails `RULE_RETRIEVAL_DEPENDENCY_FAILED` on declared persistence-authority operation incompatibility. E emits no packet. Manifest, selectors, vocabulary and dependencies are deliberately unchanged; this is preserved evidence, not a corpus correction.

Synthetic fixtures include a one-token mandatory Kernel and ten-token graph members unless specified. A->B fails optional admission at budget 20, fits wholly at 21/22. A->B->C->D and diamond fail at 40, fit at 41; leaf charged once with both diamond parents. A->C and B->C fit at 31, with increments 20 then 10 (plus Kernel 1), not two standalone 20-token charges. Independently ranked B in A->B has zero increment, original 800 score and one executable occurrence. Required/explicit/always A->B fails at 20 and succeeds at 21; two-token Procedure/Kernel fails at 1 and succeeds at 2. One-token Kernel-only success demonstrates the smallest legal budget. Oversized earlier optional A is skipped, later B fits at 11, and A-only C never leaks. Complete dependency sources include unmatched sibling snippets at their full cost. Corrupt membership, missing support, mixed identity, immutable nested evidence, mid-work cancellation, cultures, shuffled roots, relocated artifact files/unrelated CWD and newline estimator checks are covered.

## Compatibility and Validation

Compiler/artifact semantics stay unchanged: 10 sources, 154 snippets, 773 retrieval terms, 275,622 bytes; semantic `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`; serialized-byte SHA-256 `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`. Packet construction preserves frozen source/content hashes, identities, estimates and exact writer bytes.

Validation commands use `dotnet build/test -c Release --no-restore` (targeted reruns `--no-build`) on the portable library, CLI and Managed reference projects. SQL-enabled Managed tests set `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1` and use disposable test databases; no user database/history is touched.

- Portable/CLI/Managed Release builds: pass, zero warnings/errors.
- Focused E: 65 passing cases in [CompiledRulePacketTests.cs](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/CompiledRulePacketTests.cs).
- A/B/C/D: 59 / 157 / 75 / 61 passed, 352 total.
- Complete portable/CLI/SQL-enabled Managed suites: 986 / 64 / 208 passed, 1,258 unique tests; zero skipped/failed.
- FR-025 / FR-024 / FR-023 targeted regressions: 264 / 235 / 70 passed.
- FR-022/portable boundary: 17 passed; CLI dependency boundary: 1 passed.
- Nine structural harnesses: 333 assertions passed (FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 durable 81, FR-021 repair 35, FR-022 32, campaign-mode 17).
- `pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\validate_repository.ps1`: pass; 313 Markdown files, 7,526 relative links, 220 anchors, 192 canonical documents, 15 family indexes, 43 templates, 5 agent roles, 1,276 terms, 208 roadmap tasks and 36 Future Revision entries. Zero blocking questions, orphaned documents or forbidden campaign directories. Includes two identical deterministic temporary distributions and archive-content checks; generated artifacts are cleaned, not published. Without the process-local execution-policy option this environment rejects the unsigned script before it runs; no permanent policy change is made.
- Whitespace checks pass. Exact-state finalization stages only E files, checks cached scope/whitespace, reruns focused E and the full repository validator, and verifies audit counts before permitting commit/push. No A-D implementation or canonical artifact file changed.

## Limitations and Stop

No new SQL, migration 012, durable retrieval adapter, publication/readiness integration, Campaign binding, gameplay entry, Managed runtime switch or source/provider/compiler/vocabulary change. No user Campaign Canon, private locator or populated database. No F-H/G acceptance or live performance/host proof: those require their authorized packages and deployment evidence. No release/archive publication, VERSION/tag movement, new FR or FR-027 selection. Stop after E's validated commit and normal main push. Historical audits remain history, not rewritten as current completion claims.
