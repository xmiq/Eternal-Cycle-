# FR-026A Portable Retrieval Contract Audit

## Checkpoint and Authority

Owner-authorized FR-026 begins at clean synchronized `main` commit `2e6497c6f4720229f9a0fd0d2ffba65af1136167`. FR-025 is complete/Closed. The [execution plan and requirement matrix](../FR_026_EXECUTION_PLAN.md) were created before code changes from repository authority and inspected implementation. A-H are checkpoints beneath one selected incomplete FR-026, not additional governed revisions. VERSION and release/RC tags are outside this change.

## Findings and Scope

The compiled artifact and SQL import are already lossless and artifact-local. Existing Managed retrieval uses legacy source-compiled chunks, a Campaign route and fixed structural/preparation scoring, not FR-024 term matching. Its mandatory procedure, applicability, closure, Pending priority and source-grouped compact packet remain useful safety contracts. Imported candidates and active legacy Rule Releases remain different identities; no import makes itself active.

Migration 011 stores retrieval metadata in ordinal snippet JSON, not indexed term rows. F must inspect actual candidate query shape before proposing an index/migration. The new [request contract](../../docs/rules/COMPILED_RULE_STORE_RETRIEVAL.md) starts with explicit authorized semantic artifact scope and deliberately changes none of these existing stores or entry paths.

## Implemented A Boundary

`CompiledRuleRetrieval.cs` adds immutable scope, bounded request, internally constructed prepared request, safe fixed failure categories and `CompiledRuleRetrieval.Prepare`. Normalization delegates to FR-024 `NormalizeTerm`; there is no second algorithm. Explicit query phrases stay whole, punctuation/hyphens remain meaningful, and concrete IDs retain exact case. Set-like inputs are copied, deduplicated and ordered ordinally. Raw collection counts are bounded before deduplication; read-only views prevent input aliases changing later work.

Explicit Ruleset/semantic SHA scope distinguishes artifact histories without acquiring or loading them. Concrete selectors do not accept caller wildcards. Empty queries allow later structural/mandatory/explicit lookup, not a full-corpus request. The 8K estimated-token bound is inherited; no new estimator, top-K/rank knob or preparation control was added. Preparation returns neither a score nor a packet. Only invalid-request failures execute here; other fixed categories are reserved for later stages. Cancellation is propagated with the supplied token, before and during work, never converted into a successful empty result.

Default request/prepared string representations and fixed errors do not echo user terms or raw exceptions. No diagnostics/logger was introduced. The request is not a safe payload to serialize indiscriminately: explicitly disclosing its fields still requires the consuming service's disclosure policy. No SQL/MCP/Campaign/provider/host dependency or new interface/project is needed.

## Evidence and Validation

The initial focused run passed 58/59 because xUnit replaced an unpaired surrogate carried through attribute metadata before the test executed. The test now constructs the invalid string in its body; production normalization was not changed. Rerun: 59/59 passed. No production correction or artifact-contract change was required.

| A acceptance boundary | Executed focused evidence |
| --- | --- |
| Campaign/provider-independent explicit scope | Minimal request has no Campaign/provider/source-root/preparation/top-K field; different semantic histories remain distinct without lookup or authorization claims. |
| Exact FR-024 normalization | Case, NFC, Unicode whitespace, phrases, punctuation/hyphens, no generated/split terms, invalid control/surrogate and exact bounds. |
| Deterministic immutable request | Reversed/duplicate input, ordinal ordering, private read-only copies, preserved identity case and three cultures. |
| Safe bounded input | Scope/hash syntax, null fields/collections, concrete IDs, snippet components, collection counts before deduplication, inclusive 1/8K budget. |
| Fixed safe failure/cancellation | Six bounded category codes; invalid input has fixed message/no raw inner exception; cancellation before and during preparation retains token. |
| Canonical compatibility | Frozen canonical compile: 10 sources, 154 snippets, 773 terms, 275,622 bytes; FR-022 validation and byte equality. |

Canonical semantic digest remains `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`; serialized SHA-256 remains `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`. Hashes are test evidence, never production retrieval logic.

| Validation | Executed result |
| --- | --- |
| Portable, CLI and Managed Release builds (`dotnet build ... -c Release --no-restore`) | Pass, zero warnings/errors; existing restored dependencies reused, no new packages. |
| New A focused tests | 59/59, zero skips. |
| Complete portable / CLI-process / SQL-enabled Managed suites | 628/628 / 64/64 / 208/208: **900 unique tests**, zero failures/skips. |
| FR-025 portable acquisition/validation/local/GitHub/custom/import regressions | 264/264. |
| FR-024 input/enrichment/pipeline/audit/canonical/acceptance regressions | 235/235. |
| FR-023 loader/snippet/writer regressions | 70/70. |
| FR-022 conformance + portable boundary / CLI boundary | 17/17 / 1/1. |
| Nine structural harnesses, including FR-022 | 333 assertions: 13/29/19/48/59/81/35/32/17; all pass. |
| Full repository validation | Pass: 309 Markdown files, 7,481 links, 218 anchors; 192 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. Zero blocking questions, orphans or forbidden campaign directories. |
| Deterministic distribution regression | Pass through repository validator: disposable review ZIPs match, exclusions checked, temporary archives removed; no release artifact. |

Full-suite tests use the established Release projects; focused reruns overlap those totals. `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1` enables real generated/disposable LocalDB fixtures, including retained 011 upgrade, import/history/rollback and legacy publication cases. This does not certify remote deployment. Repository validation uses invocation-only `-ExecutionPolicy Bypass`, not a machine policy change. Generated bin/obj/test/cache artifacts remain ignored and unstaged.

Exact-staged focused 59-case and full repository/harness/distribution reruns pass. Working tree content matches the staged candidate; whitespace/scope review finds no unrelated implementation, credentials, Campaign Canon, generated artifacts or release edits. Counts above come from this task's executed checks, not previous-session evidence. Git's LF-to-CRLF checkout notices are informational, not whitespace errors. The audit result recording is restaged and checks repeated before commit.

## Classification and Remaining Work

- **A - Reference implementation:** .NET immutable/read-only types, concrete work bounds and fixed exception codes; existing SQL JSON/index layout observations. No schema migration or deployment change.
- **B - Managed contract:** explicit authorized artifact scope, no historical mixing, controlled compatible normalization, cancellation and safe failures; syntactic preparation never authorizes an artifact or establishes readiness.
- **C - Eternal Cycle-wide:** Repository Canon remains authority, Campaign Canon is separate, required unavailable rules cannot be guessed, and model-facing contexts remain compact rather than full-corpus dumps. No gameplay/agency rule changed.

Ranking, matching, dependency expansion, compact compiled packets, SQL retrieval/indexing, active-adapter integration and canonical retrieval-quality acceptance remain B-H, not completed A features. FR-027 stays pending/unselected. No live host, remote SQL, production retrieval performance or model-context savings were validated here; compatibility tests do not prove those later features. No prerequisite conflict or unresolved A decision remains.
