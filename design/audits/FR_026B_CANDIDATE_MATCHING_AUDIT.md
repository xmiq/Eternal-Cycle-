# FR-026B Exact Candidate Matching and Applicability Audit

## Authority and Starting State

Owner-authorized B only begins from clean synchronized `main` at `9ec817a3efb0aacc27bf161cff3799872fdb9cf0`. The configured GitHub branch was checked directly and matched. [FR-026 plan/matrix](../FR_026_EXECUTION_PLAN.md) and the [A audit](FR_026A_RETRIEVAL_CONTRACT_AUDIT.md) control this execution checkpoint beneath one selected incomplete FR-026. FR-025 remains Closed; FR-027 is pending/unselected. VERSION remains `1.0.0`; full release `v1.0.0` remains `6938bd2`, RC discovery tag `v1.1.0-rc` remains `4eb2583`. No tag or release action is authorized here.

## Implementation and Boundaries

The portable `CompiledRuleCandidateIndex` freezes and FR-022-validates one artifact snapshot, then indexes its existing retrieval wording. It reuses the existing approval deep-copy helper through internal visibility; no format, compiler, importer, trust, SQL, legacy publication or runtime behavior changes. Validation precedes dictionary construction, so malformed identities/order/duplicates fail rather than silently coalescing inconsistent artifacts. Approved bytes, acquisition evidence, paths, source files, Campaign data and runtime hosting are not matcher inputs.

```text
FR-026A prepared request + exact compiled artifact scope
  -> existing controlled term index
  -> conjunctive source applicability
  -> mandatory / explicit / always-include / matched reasons
  -> immutable applicable unranked candidate set
```

Only entire prepared query entries match ordinally. A already calls FR-024 normalization; B does not normalize again or reinterpret a query as prose. Canonical, alias and phrase associations use exactly their compiled `(term, kind, weight)` representation. `kind` preserves concept identity, not unavailable compiler-origin labels. Same wording under different kinds retains both reviewed weights; repeated query input and overlapping reasons never duplicate snippets or identical evidence. Retrieval relationships do not generate additional terms. Phrases, punctuation and hyphens stay whole; `saved`, `saving`, `combative`, partial/reordered phrases and unrelated prose are not matches merely because of similar characters.

Source/snippet identities and semantic scope are exact/case-sensitive; selectors retain ordinal case-insensitive comparison. Empty selector arrays are unrestricted. Scoped worlds require a concrete selected world, scoped modules a genuinely enabled matching module, including scoped Core/Kernel sources. Authored `*` admits any concrete world/enabled module; it cannot invent one. Mode/operation match their concrete values or authored wildcard. Scoped topics require intersection or authored `*`; empty request topics do not satisfy a concrete topic scope. All dimensions gate every candidate reason.

The FR-026 requirement matrix requires these gates even for always-include and Kernel. Accordingly B does not copy legacy unconditional Kernel eligibility or the always-include topic bypass. The canonical mandatory Kernel/procedure are topic-free, so normal `gameplay.resolve` requires neither a query hit nor caller topic to include them. A lookup needs at least one applicable Runtime Kernel snippet; gameplay resolution also needs the existing always-included applicable `gm-runtime-procedure` with snippets. Missing/inapplicable mandatory structure fails as inconsistent for runtime retrieval, without redefining general format-1 validity. The legacy runtime is not routed through B and remains unchanged.

Explicit source IDs select all applicable snippets of that source; explicit snippet IDs select only that exact snippet. Missing/wrong-case/empty-source/inapplicable required identities fail as invalid requests, with no fake controlled association. Candidate flags distinguish these reasons, mandatory Kernel, operation-scoped procedure, always-include and vocabulary. Source priority, preparation tier, selectors and dependencies remain immutable and available for later stages. They do not assign relevance or readiness here. Dependencies are not expanded.

Neutral output follows the validated artifact's ordinal snippet order, with matches in ordinal term/kind order. It never sorts by weight, number of hits, layer, priority or preparation tier. All legitimate matches remain available, including the 100-concept stress fixture; no arbitrary truncation loses future scoring evidence. Compact counts distinguish matched/applicable/excluded/unmatched snippets rather than returning every rejected snippet. Evidence is bounded by the valid artifact and A request bounds, not a new top-K policy.

Candidate collections, graph arrays and prepared input have private read-only copies. Fixed string representations and failure codes do not echo data or raw exceptions; source/content and prepared request are excluded from implicit JSON diagnostics. Rich match evidence remains service-side, not normal player output. Caller assembly DTOs must not be concurrently mutated while copying. Cancellation retains the supplied token and never returns a partial successful index/set; full synchronous FR-022 validation checks cancellation at its boundary, and index/matching loops check cooperatively.

## Canonical Matching Evidence

All 65 existing curated fixtures run against actual compiled metadata: 115 positive topology expectations and 130 distinct negative expectations, evaluated under each positive source's valid operation/topics. These prove controlled association discovery, not rank #1. Negative assertions inspect the specific controlled concept/term, distinguishing legitimate other-concept or mandatory inclusion from an unwanted direct match.

Representative `gameplay.resolve` / `NORMAL` requests below explicitly declare the union of canonical source topics, no world/modules. Counts include 11 query-independent Kernel/procedure roots, with overlapping reasons counted once:

| Query entry | Candidates | Vocabulary-matched snippets | Applicable matched | Inapplicable matched |
| --- | ---: | ---: | ---: | ---: |
| `fighting` | 15 | 4 | 4 | 0 |
| `conflict` | 15 | 9 | 4 | 5 |
| `save` | 12 | 1 | 1 | 0 |
| `preparation` | 14 | 3 | 3 | 0 |
| `campaign canon` | 11 | 2 | 0 | 2 |
| `character knowledge` | 15 | 5 | 4 | 1 |
| `gm adjudication` | 12 | 4 | 4 | 0 |
| `unreviewed` | 11 | 0 | 0 | 0 |
| Empty query | 11 | 0 | 0 | 0 |

`campaign canon` is deliberately not an eligible direct hit under this operation: its persistence-authority source is scoped to context/persistence operations. The curated positive fixtures retrieve it under those applicable operations. This demonstrates that relevance cannot override operation applicability, not a missing association. Broad reviewed words remain controlled mappings; neither prose scans nor a whole-154-snippet fallback occur.

The frozen canonical compile remains **10 sources / 154 snippets / 773 terms / 275,622 bytes**. Semantic digest: `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`. Serialized byte SHA-256: `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`. FR-022 accepts it and writer bytes remain identical. These are regression evidence, never matcher constants.

## Executed Validation

| Check | Result |
| --- | --- |
| Portable, CLI and Managed Release builds, existing restored dependencies | Pass; zero build warnings/errors, no new packages. |
| B focused cases, including exact-staged rerun | 157/157; also included in the complete portable suite. |
| A/B combined focused rerun | 216/216 (A 59 + B 157), zero failures/skips. |
| Complete portable / CLI-process / SQL-enabled Managed | 785/785 / 64/64 / 208/208: **1,057 unique tests**, zero failures/skips. |
| FR-025 / FR-024 / FR-023 focused regressions | 264/264 / 235/235 / 70/70. |
| FR-022 conformance + portable dependency boundary / CLI dependency boundary | 17/17 / 1/1; no Managed, SQL, MCP or hosting dependency in the portable assembly. |
| Nine structural harnesses | 333 assertions (13/29/19/48/59/81/35/32/17), all pass through the repository validator. |
| Full repository/distribution validation before documentation checkpoint | Pass: 309 Markdown files, 7,481 links, 218 anchors; zero blockers/orphans/forbidden campaign directories; deterministic disposable review ZIPs and exclusions checked. |

The first focused implementation run passed 151/151. Following snapshot-before-validation hardening, the cancellation fixture did not trigger because .NET optimized list traversal through its indexer rather than enumerator. That test-only fixture now cancels through either access path; rerun passed 153/153, followed by additional capacity/relationship/ordering cases and the passing 785-case suite. No FR-022 semantic or production cancellation weakening was used to make it pass. Two initially over-narrow test-name filters were corrected to actual boundary/contract names; a no-match filter is not claimed as executed validation.

SQL tests use `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1` with real disposable LocalDB fixtures. Repository validation uses invocation-only `-ExecutionPolicy Bypass`, not a persistent system-policy change. Generated bin/obj/cache/test artifacts stay ignored and outside the staged/distribution source. No retained archive or release artifact is produced. Focused reruns overlap full-suite totals.

Final staged repository validation passes: 310 Markdown files, 7,489 links, 218 anchors; 192 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. Zero blockers, orphans or forbidden campaign directories. All nine harnesses and deterministic distribution checks pass again. Working files match the staged source tree with no untracked files; cached whitespace and complete diff/scope review find no scoring, closure, packets, SQL/migrations, unrelated edits, credentials, private machine paths, Campaign data, generated artifacts or release changes. Git's LF-to-CRLF notices are informational, not whitespace failures. This validation-result recording is restaged and focused/repository checks repeated before commit.

## Classification and Stop Boundary

- **A - Reference implementation:** .NET term index, shared deep-copy helper, immutable evidence graph, count diagnostics and focused fixtures. No projects, dependencies, SQL adapters or migration were added.
- **B - Managed contract:** artifact-local exact associations, applicable mandatory/explicit reasons, immutable evidence, safe mismatch/cancellation, no mixed history or readiness/authorization claims.
- **C - Eternal Cycle-wide:** controlled metadata cannot change rules or grant unavailable rules; retrieval relevance cannot waive authority/applicability; Campaign Canon remains separate. No gameplay rule or agency behavior changes.

B **does not prove ranking quality**, complete dependency closure, packet sufficiency, readiness integration, production query cost, remote SQL or real host acceptance. C owns ranking, D dependency closure, E packet admission, F SQL/runtime integration and readiness, G quality/cost and H final acceptance. C-H and FR-027 are not started. Existing schema 011/history, compiler/vocabulary corpus and immutable releases are untouched. No B prerequisite conflict or blocking ownership question remains. Stop after this checkpoint's validated commit and normal `main` push, without version/tag/release action.
