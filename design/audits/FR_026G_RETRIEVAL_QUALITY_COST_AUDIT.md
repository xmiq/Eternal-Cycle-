# FR-026G Retrieval Quality and Local Context-Cost Audit

## Checkpoint and Scope

Owner-authorized G starts from clean synchronized `main` at `20c0149487a082a03475ae57bf3ccef4c116d9b1`. [FR-026](../FR_026_EXECUTION_PLAN.md) remains selected/incomplete; G is an execution checkpoint, H pending, not a separate revision or closure. VERSION remains `1.0.0`; `v1.0.0` remains `6938bd2f9e965faaeb1c612b1dd765237cadc8a8`, `v1.1.0-rc` remains `4eb2583027d316a962fd49fe318781a0e060183e`. This audit belongs to the focused commit containing it rather than embedding a self-referential resulting SHA.

G adds tests, linked test fixtures, reports and governance documentation only. **No production code, migration, canonical rule prose, vocabulary, selector, dependency, priority, tier, estimator, ranking, admission, publication or authority changes.** No Campaign data, binding, telemetry infrastructure or release artifact. Existing decisions governing A-F suffice; no new normative decision or blocking design question.

## Evidence Architecture

[Shared test evidence](../../examples/tooling/rules-compiler-dotnet/tests/Shared/CompiledRuleQualityEvidence.cs) runs A preparation, B exact applicability/matching, C ranking, D complete closure and E admission without alternate algorithms. [Portable tests](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/CompiledRuleQualityTests.cs) assert expectations and serialize deterministic observations. [SQL tests](../../examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/CompiledRuleSqlQualityTests.cs) reuse F's complete semantic projection over real disposable LocalDB and the existing authorized import/publication/activation/bootstrap fixture.

The SQL-only observer uses SqlClient 7's typed `SqlClientCommandBefore` DiagnosticSource payloads, filtered to the fixture database. It retains at most 16 in-memory command observations per request, never connection strings, returned SQL data or timestamps. Only safe abstract counters and stable result hashes enter the checked-in report; no runtime diagnostics or API changes. Observed/unobserved requests have identical full results. No metric feeds back into selection or scoring.

## Fixture Methodology and Quality

[54 canonical fixtures](../../examples/tooling/rules-compiler-dotnet/fixtures/compiled-rule-retrieval/quality-cases.json) selectively reference FR-024's [reviewed topology](../../examples/tooling/rules-compiler-dotnet/fixtures/canonical-vocabulary/quality-expectations.json), rather than mechanically duplicating 65 association-only cases. Stable IDs record category, operation, explicit query entries, budget, required identities, reviewed evidence IDs, expected excluded sources and failures. The report expands these into exact term/concept/snippet expectations and original rationales. Requests declare NORMAL, no world/modules, and the ordinal union of authored topics; these are explicit test selectors, not hidden production defaults. All ten sources receive genuine controlled hits somewhere in the corpus.

Positive expectations mean applicable **vocabulary root/contribution**, not rank #1 or guaranteed optional budget admission. Negative expectations prohibit the particular concept/term hit, not unrelated legitimate mandatory/dependency inclusion. Expected failures preserve B/C evidence but return no executable packet. Required and dependency success denominators exclude failed requests, which must fail without partial output.

| Category | Cases | Positive satisfied/total | Negative satisfied/total |
| --- | ---: | ---: | ---: |
| Authority | 4 | 6/6 | 10/10 |
| Broad | 2 | 16/16 | 18/18 |
| Budget | 4 | 0/0 | 0/0 |
| Combat | 3 | 5/5 | 5/5 |
| Context | 1 | 2/2 | 2/2 |
| Development | 3 | 6/6 | 6/6 |
| Empty/unknown | 2 | 0/0 | 2/2 |
| Evolution | 3 | 6/6 | 6/6 |
| Exactness | 9 | 10/10 | 17/17 |
| Explicit identities | 3 | 0/0 | 0/0 |
| Incarnation | 3 | 6/6 | 6/6 |
| Knowledge | 2 | 2/2 | 6/6 |
| Overlap | 3 | 3/3 | 6/6 |
| Persistence | 4 | 6/6 | 8/8 |
| Procedure | 2 | 3/3 | 5/5 |
| Rules | 3 | 4/4 | 6/6 |
| Simulation | 3 | 6/6 | 6/6 |
| Total | 54 | 81/81 | 109/109 |

Across requests (not unique-corpus recall), B records 185 applicable and 36 inapplicable hits. Gameplay conflict excludes five persistence-authority matches; context conflict excludes four combat matches. Concept identity and independent 600-point contributions remain distinct. Partial phrases, substrings, stems, fuzzy spellings and removed hyphens do not infer terms. Case/NBSP/whitespace normalization and duplicated/reordered entries preserve results. Synthetic NFC composed/decomposed input retains two distinct concepts at 700 + 600 = 1,300 without duplicate score or content.

Every C candidate equals the sum of distinct term/concept weights and follows score descending, priority descending, ordinal snippet ID. Fighting has four 700-point roots; conflict four 600-point gameplay roots; their combined evidence is 1,300 each. Canon has two 900-point roots. Eleven gameplay mandatory/always-include roots normally score zero, not false positives. Synthetic fixtures isolate priority and ordinal ties, unequal preparation tiers without relevance bonuses, and unmatched always-include requirements. Valid Int32-count lists with weights <=1,000 cannot overflow Int64: maximum 2,147,483,647,000. Existing C corrupt-input/checked arithmetic tests are reused; G does not pretend an impossible valid overflow was exercised.

## Dependencies and Budgets

All 45 successful packets satisfy `used <= budget`, exact sum of unique stored estimates, every required root, complete admitted prerequisite groups and explicit service-side optional exclusions. **407/407 required-root** and **1,126/1,126 per-admitted-root prerequisite-member** assertions pass. The latter counts shared prerequisites per closure obligation, not unique stored rows. Thirty-six optional-root exclusions remain explained; no fragmented closure or complete-154-snippet fallback. Explicit bootstrap source and development snippet requirements retain zero relevance and required reasons; the whole development source correctly fails its 8K required budget.

Canonical graph maximum depth is one (three declared edges); G does not portray synthetic chains as canonical. Context requests can add substantial persistence-authority dependency-only material: compiled-rules request has six roots/30 members, 24 dependency-only members. Its cost is 1,010 required + 491 query increment + 6,385 dependency increment = 7,886. Shared/mandatory prerequisites already reserved are not charged to the incremental dependency category again.

Five reused synthetic shapes (direct, four-source chain, shared, diamond, root-also-dependency) compare full SQL/reference identity, rank, text, support and totals at full/exact/one-below optional closure budgets. Shared text/cost appears once; mandatory reservation remains intact when optional closures do not fit. Existing D tests for a 4,096-source chain and eight-by-eight layered dense graph (65 members, 456 direct support edges) ran through A-E/full suite validation, without duplicating those stress fixtures or routing them through G's canonical-only report inventory.

## Canonical Outcomes and Context Cost

Whole-corpus baseline is **56,398 repository estimated rule-context units**, the sum of all 154 positive stored snippet estimates. The existing compilation estimator `max(1, (UTF8 bytes + 2) / 3)` on normalized executable content is independently checked; G does not introduce a tokenizer. JSON/envelope, labels, joining separators, service evidence and other runtime context are excluded. These are not exact external-model tokens or an SLA.

| Query/operation | Budget | Roots/members | Used | Remaining | Optional exclusions | Avoided / 56,398 |
| --- | ---: | --- | ---: | ---: | ---: | --- |
| fighting / gameplay | 8,000 | 15/15 | 4,746 | 3,254 | 0 | 51,652/56,398 |
| fighting / gameplay | 3,000 | 13/13 | 2,985 | 15 | 2 | 53,413/56,398 |
| conflict / gameplay | 8,000 | 15/15 | 4,746 | 3,254 | 0 | 51,652/56,398 |
| conflict / gameplay | 3,000 | 13/13 | 2,985 | 15 | 2 | 53,413/56,398 |
| preparation / gameplay | 8,000 | 14/14 | 3,430 | 4,570 | 0 | 52,968/56,398 |
| preparation / gameplay | 3,000 | 13/13 | 2,979 | 21 | 1 | 53,419/56,398 |
| campaign canon / context | 8,000 | 7/7 | 1,801 | 6,199 | 0 | 54,597/56,398 |
| campaign canon / context | 1,400 | 6/6 | 1,394 | 6 | 1 | 55,004/56,398 |
| unknown / gameplay | 8,000 | 11/11 | 2,027 | 5,973 | 0 | 54,371/56,398 |
| empty / gameplay | 8,000 | 11/11 | 2,027 | 5,973 | 0 | 54,371/56,398 |
| broad successful / gameplay | 3,000 | 13/13 | 2,921 | 79 | 21 | 53,477/56,398 |

Gameplay mandatory floor **2,027** fits exactly, 2,026 fails. Context Kernel floor **1,010** fits exactly, 1,009 fails. Unknown and empty are measured separately; no full-corpus fallback. Fighting full has 2,027 required + 2,719 query increment + zero extra dependency cost. Context Canon full has 1,010 required + 791 query increment. Generic metadata coverage 122/154 from FR-024 is not a target for retrieval saturation.

Forty-five successful fixtures: packet estimates min 1,010, median 3,430, max 7,941, exact average 181,845/45; roots min 5, median 11, max 21, average 524/45; members min 5, median 15, max 31, average 709/45. Total avoided 2,356,065/2,537,910 compared with loading the full corpus for each of those 45 requests. These are fixture-set descriptive statistics, not population forecasts or a blended quality grade.

Nine expected failures: six dependency failures (save, four-term combined, gameplay procedure, player agency, GM Secrets and population/ecology) and three required-budget failures (gameplay/context one-below and complete development source). The additional gameplay terms select context-assembly roots which have the same already-declared incompatible persistence-authority operation prerequisite. This is frozen metadata/accepted fail-closed semantics, not an A-F implementation defect or authorization to repair selectors in G. All return no partial packet and match SQL; H must review their practical effect.

## SQL and Deterministic Reports

Every one of 54 canonical outcomes equals F using its complete projection: candidate counts, roots, scores, contributions, reasons, full source/snippet semantics, dependency groups/support, exact packet text/order, decisions, exclusions, totals and stable failure category. Runtime acquisition is absent; the selected imported artifact alone supplies data. A second unrelated active diamond artifact remains separate; scoped parameters and child-table `WHERE import_id = @id ORDER BY ...` reads load no historical rows. No SQL ranking, per-snippet/term loops, unbounded store scan or global cache was added.

Observed **six scoped set-based artifact reads**, plus gameplay's host-acknowledgment read: 16 cases use six commands, 38 use seven. Current golden observations document this strategy; the test's small command window protects bounded behavior, not a universal six-query provider contract. Verified materialized records per request are one header, ten sources, 154 snippets, three dependency rows, 773 associations in snippet JSON and 275,622 retained bytes. They are logical verified records, not instrumented wire bytes, physical pages or allocated memory. In-memory A-E sees the same artifact/counts without SQL commands. Candidate/closure counts peak at 41 in this fixture set; final packet members peak at 31. No observational timings were collected, so no warm/cold/millisecond or production-performance claim.

Reports are fixed-property compact UTF-8, no BOM/trailing newline, invariant values and ordinal fixture/expectation/category ordering. No clock, random/physical identity, campaign secrets, raw SQL or connection strings. Detailed evidence remains in test fixtures, never ordinary packets. Normal tests compare frozen golden bytes; explicit maintenance environment variables only generate replacement candidates after assertions pass. Portable reports reproduce across repeat, reversed fixture enumeration, cultures, relocated payloads, unrelated CWD and process executions. SQL reports reproduce across fresh disposable databases and exact-staged reruns; observer-disabled results agree.

| Report | Bytes | Serialized SHA-256 |
| --- | ---: | --- |
| [Canonical quality](../../examples/tooling/rules-compiler-dotnet/fixtures/compiled-rule-retrieval/expected-quality-report.json) | 549,512 | `0241854BCC8142F4BCAF4D0A6146603F25FA24AC7DFCE3472ACEF2CF47620281` |
| [SQL quality/counters](../../examples/tooling/rules-compiler-dotnet/fixtures/compiled-rule-retrieval/expected-sql-quality-report.json) | 17,213 | `1E106B6ED467DF6B2681EB70945A79E3B5A55FD2E83965F3B1F1CC1B6F04AA56` |

Canonical artifact remains **10 sources / 154 snippets / 773 terms / 275,622 bytes**. Semantic digest `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`; serialized SHA-256 `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`. Full-model before/after and writer/hash checks preserve authoritative bytes, snippet/content identities, metadata and FR-022 semantics. Report hashes are distinct from artifact hashes/digest.

## Executed Validation

Portable, CLI and Managed Release builds pass with zero warnings/errors, no added packages. G focused **65 portable / 9 SQL** cases pass: 54 canonical portable cases plus 11 report/identity tests; one SQL test loops all 54 canonical fixtures, five graph cases test three budgets each, two ties/tier/always-include cases and one Unicode/multi-concept case. Complete portable **1,051/1,051**, CLI/process **64/64**, SQL-enabled Managed **284/284** pass: **1,399 unique full-suite tests**, zero skips. Focused reruns overlap these totals.

Executed targeted compatibility: A-E **417/417**, FR-025 portable **264/264**, FR-024 **235/235**, FR-023 **70/70**, FR-022 conformance/portable dependency boundary **17/17**, CLI boundary **1/1**. The full Managed suite includes F, authorization, migration/import/publication/readiness/history/cancellation and worker recovery tests. No new instance of F's previously recorded short-lease worker flake occurred in these full runs; this is not proof that its historical intermittent behavior is resolved.

Initial G failures were test-only: nullable compilation-result warning; an independently reviewed advancement alias incorrectly expected to have no hits; unclassified frozen dependency/budget outcomes; misuse of the existing relocation helper's source-root argument; invented preparation-tier names in synthetic JSON. Corrected only G tests/fixtures, reran focused and full suites; no A-F production correction. Failed initial runs are not passing validation evidence.

Command families: `dotnet build ... -c Release --no-restore`; `dotnet test ... -c Release --no-restore` (or `--no-build` after that exact build); Managed with `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1`; `pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\validate_repository.ps1`; `git diff --cached --check`. Environment opt-in runs real generated/disposable LocalDB, not mocks. No persistent execution-policy change. Generated bin/obj/cache/test output stays ignored. Deterministic distribution validation creates two temporary review ZIPs and removes them; no retained/published release archive.

Repository validation passes: 315 Markdown files, 7,550 relative links, 223 anchors, 192 canonical documents, 15 family indexes, 43 templates, five roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. All nine structural harnesses pass **333 assertions** (13/29/19/48/59/81/35/32/17), including FR-022. Zero blockers, orphans or forbidden Campaign-data directories. Deterministic two-build distribution/content checks pass. The first documentation validation caught a missing audit-index link; G's index entry was added and the complete validator rerun successfully.

Exact-state scope review covers all intended staged files and parsed report contents, including exclusions for credentials, private machine paths, Campaign data, generated artifacts and production/release edits. Whitespace checks pass. Reviewed topology is loaded once per test process rather than repeatedly reread per measurement; this test-only optimization does not change report bytes. Final focused/report and real SQL quality reruns, repository validation and staged whitespace are repeated on the exact candidate before commit. Working files match the staged source, with no untracked content. Git LF-to-CRLF notices are informational rather than whitespace failures.

## Classification, Limits and Handoff

- **A - reference implementation:** typed .NET test observer, LocalDB query counters, test-only report writer, SQL strategy and finite golden fixtures. No schema migration or production telemetry; these mechanics are not provider mandates.
- **B - Managed contract evidence:** one authorized artifact, complete verified semantics, identical SQL/reference outcome, separate publication/readiness, bounded compact context and service-side evidence. Import/provider history and authority stay intact.
- **C - Eternal Cycle-wide evidence:** relevant reusable rules cannot be guessed, selector/closure/required-budget failure cannot yield a partial successful packet, mandatory procedure is not a topical false positive. No gameplay mechanics changed.

H still owns integrated FR-026 requirement closure and practical findings review. FR-031 still owns actual whole-runtime context accounting/budgets, envelopes/tool exchanges, Campaign Canon working sets and the approximately 20K ordinary-operation target; these local rule-context counters do not implement or close it. Real AI-host/MCP acceptance, remote SQL/multi-user authorization, production concurrency/load/latency and external-tokenizer equivalence remain unproven. FR-027 remains unselected; no Campaign binding or downstream objective started. Stop after exact-state acceptance, focused commit and normal `main` push; VERSION/tags/releases remain untouched.
