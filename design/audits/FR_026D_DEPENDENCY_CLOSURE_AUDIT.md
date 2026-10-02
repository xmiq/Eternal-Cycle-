# FR-026D Complete Artifact-Local Dependency Closure Audit

## Authority and Checkpoint

The owner authorized D only beneath [FR-026](../V1_1_FUTURE_REVISION_PLAN.md#fr-026---compiled-rule-store-and-intelligent-retrieval) and its [A-H execution plan](../FR_026_EXECUTION_PLAN.md). Starting `main`, HEAD and GitHub `origin/main` were clean/synchronized at `ca219b354a4e079c18da9bd4cefc197c98e83daa`. [A](FR_026A_RETRIEVAL_CONTRACT_AUDIT.md), [B](FR_026B_CANDIDATE_MATCHING_AUDIT.md) and [C](FR_026C_RANKING_AUDIT.md) were complete; D-H pending. A-D are now complete; FR-026 remains selected/incomplete, E-H pending, FR-027 unselected. Phase 13 stays Active and Future Revisions stays last. VERSION remains `1.0.0`; `v1.0.0` remains `6938bd2f9e965faaeb1c612b1dd765237cadc8a8`, `v1.1.0-rc` remains `4eb2583027d316a962fd49fe318781a0e060183e`. No tag/release/archive action is authorized. This audit records the implementation tree, not a self-referential resulting commit SHA.

## Established Semantics and Changes

Read authority includes [format-1](../../docs/rules/COMPILED_RULES_ARTIFACT.md), FR-023 assembler and snippet compiler, FR-022 iterative graph validator, FR-025 lossless import/readback, A-C code/tests, and Managed `RuleCompilation.BuildDependencyClosure`/`IsDependencyApplicable`. Dependencies name Rule Sources, not snippets/anchors. The legacy complete transitive expansion includes all compatible chunks of a prerequisite source, orders prerequisites before dependents and does not rematch query topics. A selected snippet does not require its own unmatched siblings unless another selected source depends on that source.

New portable [CompiledRuleDependencies.cs](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRuleDependencies.cs) exposes `CompiledRuleDependencies.Expand(ranked)` and get-only closure/member/root-group types. [CompiledRuleCandidateIndex.cs](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRuleCandidateIndex.cs) internally retains the already-validated/frozen artifact on its candidate set and factors the existing world/module/mode/operation selector expression into a shared helper. B's matching, topic gates, candidates and scores are not changed. C code is unchanged. No second public raw-graph entry or full validation/copy per query is introduced.

Processing is **A prepare -> B eligible candidates -> C ranked roots -> D complete closure**. D checks exact Ruleset/semantic scope and source/snippet object membership in the retained immutable authority. A matching ID or claimed digest cannot substitute roots from another artifact. Missing/mixed roots use inconsistent-artifact; scope mismatch uses invalid-request. A package-internal test seam bypasses FR-022 only for defensive corrupt-graph fixtures, not production callers.

Dependency compatibility ignores query topics but retains B's concrete World Model, enabled module, mode and operation gates. Authored wildcards cannot enable a world/module implicitly. Incompatible, missing, empty, duplicate/malformed or cyclic reachable dependencies use the existing fixed dependency-failed result; cancellation keeps its token. No raw content/path/exception text enters failures, and no partial successful closure is returned. An inapplicable vocabulary hit excluded by topic can only reappear when expressly required and executable, labelled dependency-only, not an ordinary root. Incompatible world/module/mode/operation cannot be rescued by a dependency declaration.

### Algorithm, Ordering and Evidence

1. Index the selected snapshot's sources/snippets and verify root membership; snippets within each source are ordinally ordered.
2. Visit distinct root sources in C order using iterative DFS with active/done states. Dependency IDs are ordinal, prerequisites finish before dependents, and active revisits fail as cycles.
3. Collect every reachable direct-parent edge, including later visits to shared sources. Emit each union member once: full snippet sets for dependency sources; only ranked snippets for sources that are not themselves prerequisites.
4. Preserve C roots in their original rank order with their exact original objects. Build complete per-root prerequisite lists in union topological order; roots from one source share the list, and all groups share union member references.

`Root` is optional on a member; `IsRoot` and `IsDependency` may both be true. Root scores, contributions, reasons, priorities and applicability evidence are unchanged. Dependency-only members have no root/matches, zero relevance and no mandatory/explicit/always-include flags borrowed from a parent. All direct `RequiredByRuleSourceIds` are retained ordinally, not merely the first traversal parent. Explanation topology is O(reachable edges + union members), plus explicit per-root membership; parent arrays and same-source prerequisite lists are shared. No exponentially many paths or executable-content copies are retained. No arbitrary explanation cap truncates required members. The acquisition default remains 16 MiB; this is not a claim of unbounded graphs or a new D node limit. Per distinct root source, membership reachability may inspect the graph again to materialize E's complete group; it is not repeated FR-022 validation or query scoring.

Read-only views and B's deep snapshot prevent caller mutation. Cancellation is checked during indexing, roots, dependency iteration, union and group construction, around synchronous sorting and before return. Token budget does not affect membership. The result is service-side closure structure, not a final Rule Packet or model-facing artifact dump; full source/content and original request are excluded from implicit JSON diagnostics.

## Canonical Graph Inventory

| Dependent source | Required source | Maximum direct depth |
| --- | --- | --- |
| `gm-runtime-procedure` | `runtime-kernel` | 1 |
| `gm-host-bootstrap` | `runtime-kernel` | 1 |
| `core-context-assembly` | `core-persistence-authority` | 1 |

Ten sources; **three sources with dependencies / three edges / maximum direct count one / maximum transitive depth one**. Kernel is shared by two sources. No canonical transitive chain or diamond exists. The standard gameplay fixtures do not select host bootstrap; their procedure requires five Kernel snippets already included as mandatory roots. Therefore successful examples add **zero dependency-only members**, but retain five root-plus-dependency members and parent evidence. No dramatic canonical expansion is claimed.

## Canonical Query Evidence

Requests use C's existing operation/mode/topic fixtures and unchanged explicit artifact scope. No world/module is inferred. The Canon request queries `campaign canon` under operation `context.assemble`; context assembly is the request operation, not an invented prose search.

| Query | C roots | D result | Unique members | Dependency-only | Preserved score evidence |
| --- | --- | --- | --- | --- | --- |
| `fighting` | 15 | Complete | 15 | 0 | Four Development roots at 700; 11 query-independent roots at zero. |
| `conflict` | 15 | Complete | 15 | 0 | Four Development roots at 600; B's five excluded hits stay excluded. |
| `save` | 12 | Dependency-failed | No successful closure | Not applicable | Manual-persistence root remains 600. |
| `preparation` | 14 | Complete | 14 | 0 | Three Evolution roots at 600; no preparation-tier bonus. |
| `campaign canon`, `context.assemble` | 7 | Complete | 7 | 0 | Two persistence-authority roots at 900; five Kernel roots at zero; no traversed edge. |
| Unknown / empty | 11 / 11 | Complete | 11 / 11 | 0 / 0 | Every root remains zero; five Kernel members also required by procedure. |
| `fighting`, `conflict`, `save`, `preparation` | 19 | Dependency-failed | No successful closure | Not applicable | Four Development roots at 1,300, persistence at 600, three Evolution roots at 600, 11 at zero. |

The exact gameplay **save compatibility limitation** is existing metadata: `core-context-assembly` permits `gameplay.resolve` and requires `core-persistence-authority`, whose operations are `context.assemble`, `persistence.read`, `persistence.commit`, not `gameplay.resolve`. D must not omit this prerequisite or waive its operation selector. Save/combined tests expect bounded `RULE_RETRIEVAL_DEPENDENCY_FAILED`, compare original ranked evidence before/after, and confirm no successful partial result. The manifest remains unchanged. This finding is not a resolved corpus-acceptance claim; any selector correction requires a separately scoped owner review. It does not make the established dependency contract ambiguous or justify changing A-C eligibility.

C rank order remains authoritative in all examples. Combined positive-score order is the four Development IDs `contextual-capability-assessment`, `power-preparation-and-victory`, `why-there-is-no-universal-character-level`, `worked-examples`; then `core-context-assembly#manual-persistence-commands` (priority 300), then Evolution `gradual-evolution`, `route-readiness`, `the-evolution-transition` (priority 150). C's 11 zero-score roots follow. Successful gameplay presentation instead puts the Kernel source before the dependent GM procedure; this is topological presentation, not reordered root relevance. Shared Kernel members do not receive extra score.

## Synthetic and Safety Evidence

[CompiledRuleDependencyTests.cs](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/CompiledRuleDependencyTests.cs) has **61 executed cases**:

- Direct A -> B includes B's two snippets exactly once, then A; B is dependency-only. Transitive A -> B -> C -> D emits D/C/B/A and three complete prerequisites.
- Diamond A -> B/C -> D emits D/B/C/A once each; D retains both B/C parents. Shared A/B -> C retains both parents and C's original B-before-A rank when B has the higher reviewed score.
- Root-also-dependency B retains its 700 score/matches while recording A support; B's unmatched sibling is dependency-only. Same-source roots share prerequisite lists. Mandatory Kernel, procedure, explicit source/snippet and always-include roots expand without propagating flags.
- Fifteen compatibility cases cover query-topic necessity, world/module concrete and wildcard selection, mode and operation rejection/acceptance. A topic-excluded query hit returns only as a declared dependency with zero score. A one-token budget still returns complete closure; preparation tier is not relevance.
- A **4,096-source** valid chain passes FR-022 and iterative closure with 4,095 complete prerequisites. An eight-layer/eight-node dense graph has 65 members and 456 direct-edge explanations rather than enumerating its many paths.
- Seven defensive corrupt cases cover missing target, cycle, self-cycle, empty/malformed/duplicate dependency and empty target source. Six authority cases cover scope, absent root, same-ID different source/snippet, duplicate root and detached snapshot; no historical/source fallback.
- Repetition, shuffled source/snippet/dependency enumeration and `en-US`/`tr-TR`/`fr-FR` preserve output. Five deterministic cancellation points (pre-start, sources, snippets, roots, dependency traversal) preserve the supplied token without partial success. Frozen graph, root evidence, membership, groups and parents reject mutation. Canonical fixtures compare C objects and serialized evidence unchanged.

The first test build exposed two test-only mistakes (a nonexistent preparation enum value and an IReadOnlyList passed to an IList test wrapper). The first runnable suite passed 60/61; the immutability test attempted to clear a fixed-size fixture array rather than mutable caller input. Tests now use the actual enum/type and mutable fixture lists. All 61 rerun cases pass; production dependency semantics were not relaxed to make tests green. Local null/invariant guards were also tightened before complete-suite validation.

## Legacy Comparison and Compatibility

Legacy closure is already transitive and prerequisite-first with source-level full compatible chunk expansion, but uses recursive per-chunk DFS and generic exception text. Legacy selection then combines scored-root iteration, closure admission, budget and packet ordering. D reuses dependency granularity, complete transitivity, executable compatibility and topic-independent necessity, not legacy scoring or layer-specific selector shortcuts. B's uniform world/module selector safety is authoritative for portable artifacts. D replaces recursion in its new reference engine, retains all direct parents, and exposes complete immutable groups without budgeting. **Managed legacy code and runtime behavior are not modified.** F must later reproduce the portable semantics from authorized storage; this audit does not claim that adapter exists.

FR-025 approved semantic/exact-byte history and artifact-owned children remain intact; no storage model, active pointer or publication behavior changes. No SQL/index/migration 012 is added. No compiler, manifest, vocabulary, source prose, artifact format, Campaign state, provider/acquisition, readiness or gameplay change occurs. Exact canonical compilation remains **10 sources /154 snippets /773 terms /275,622 bytes**:

- Semantic digest: `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`.
- Serialized byte SHA-256: `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`.

FR-022 validation and byte-for-byte writer comparison pass; source/content hashes and identities are unchanged. Hashes are regression evidence, not production constants.

## Executed Validation

| Check executed against D | Result |
| --- | --- |
| Portable, standalone CLI and Managed Release builds, existing restored dependencies | Pass, zero warnings/errors; no new package/project. |
| D focused tests | 61/61, zero failures/skips. |
| A/B/C regressions | 291/291 (59/157/75). |
| Complete portable / CLI-process / SQL-enabled Managed suites | 921/921 /64/64 /208/208: **1,193 unique tests**, zero failures/skips. |
| FR-025 / FR-024 / FR-023 focused regressions | 264/264 /235/235 /70/70. |
| FR-022 contract/conformance + portable dependency boundary / CLI dependency boundary | 17/17 /1/1. |
| Nine repository structural harnesses | 333 assertions (13/29/19/48/59/81/35/32/17), all pass. |
| Pre-documentation full repository and deterministic distribution | Pass: 311 Markdown files, 7,499 links, 219 anchors, zero blocking questions/orphans/forbidden Campaign directories. |

Commands use `dotnet build` and `dotnet test -c Release --no-restore`; focused reruns reuse built outputs where appropriate. Real SQL fixtures set `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1` and use disposable LocalDB, not Campaign databases. Full repository validation uses `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate_repository.ps1`; no persistent machine policy change. It runs all nine structural harnesses and deterministic disposable distribution content/reproducibility checks. Focused totals overlap full-suite totals. Generated bin/obj/test/cache outputs remain ignored/excluded. Exact staged whitespace, focused D and post-documentation full repository checks are recorded after final diff review, before commit.

Post-documentation and exact-staged repository validation pass with 312 Markdown files, 7,514 links, 220 anchors, 192 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. All nine harnesses/distribution checks pass; zero blocking questions, orphaned documents or forbidden Campaign directories. Exact-stage focused D rerun passes 61/61; cached whitespace passes and working content matches the stage with no untracked files. Full diff review confirms D-only scope, unchanged A-C semantics/scores, no SQL/migration/runtime/budget/packet implementation, credentials, private machine paths, Campaign data or release edits. This result recording is restaged and focused/repository checks repeated before commit; no earlier-session result substitutes for this tree. Git LF-to-CRLF notices are informational.

## Classification and Stop Boundary

- **A - Reference implementation:** .NET frozen snapshot handoff, iterative DFS, read-only shared members/arrays, ordinal maps and deterministic synthetic/LocalDB regression fixtures.
- **B - Managed contract:** one authorized artifact, complete source dependency necessity, compatible executable selectors, preserved root evidence, direct-parent support, cancellation and fail-closed corruption; later storage implementations must reproduce the semantics, not .NET object identity mechanics.
- **C - Eternal Cycle-wide:** required rules cannot be silently omitted or supplied from another authority; dependency necessity is not player/query relevance. Campaign Canon, agency and ordinary gameplay remain unchanged.

Remaining limitations are explicit: the canonical save/combined operation compatibility finding above, not a silently successful packet; E's cost/admission/packet work, F's SQL/runtime bridge, G/H's broader quality/cost/integrated acceptance, and real remote/host deployment are not completed or validated by D. No new blocking D design question is introduced. No later package, FR-027 or release is selected. Stop after the focused validated commit and normal main push; VERSION/tags remain unchanged.
