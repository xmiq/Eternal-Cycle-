# FR-027D Mandatory Gameplay Entry Acceptance

## Checkpoint and Decision

Owner-selected D only began from clean synchronized `main` at `1ce4c24844a869f4a2d7adc14d35ed84b200876f` (`feat: add trusted player interaction lifecycle`). VERSION is `1.0.0`; immutable `v1.0.0` remains `6938bd2f9e965faaeb1c612b1dd765237cadc8a8`, moving discovery pointer `v1.1.0-rc` remains `4eb2583027d316a962fd49fe318781a0e060183e`. No tag, release or campaign-history action is included.

**FR_027D_ACCEPTED.** The operation, automatic actual retrieval/reads, durable receipt/OPEN transaction, fail-closed authority and real SQL/MCP recovery pass the validation below. D is an opt-in service/reference entry checkpoint, not supported ordinary gameplay, real-host attestation or closure of FR-027. A-D remain complete; E-G remain pending; Phase 13 Active; FR-028 onward unselected. The [A contract](../../docs/ai/CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md), [A scenarios](FR_027A_CAMPAIGN_BINDING_GAMEPLAY_ENTRY_CONTRACT.md) and [execution plan](../FR_027_EXECUTION_PLAN.md) retain authority.

## API and Authority

`GameplayEntryService.BeginAsync` / `ec_begin_gameplay_interaction` consume stable Request ID, original expected interaction revision, optional owned Interaction ID and bounded model hint terms. They cannot accept Campaign, session/principal, artifact, mode, world, choice, trusted-origin or completion flags. B/C configured scope, durable trusted submission and immutable binding/generation own these facts. No production human-event ingress has been added; absent admitted input returns `TRUSTED_SUBMISSION_REQUIRED`. The test ingress exists only in the test assembly.

`IGameplayEntryStore` shares C's SQL aggregate and session transaction lock. Prepare checks exact scope/access/current ACTIVE binding, original campaign/profile/save/bootstrap evidence, known persistence, state/revision and any related Pending decision. Receipt recovery addresses the original stable request before selecting current input; conflicting Request/payload reuse fails. Preparation transitions RECEIVED to ENTRY_PENDING with one durable row, never provisional OPEN. Existing internal C pre-entry preparation is also accepted only at its expected revision. No public generic state setter exists.

## Rules and Minimum Canon

`GameplayEntryCanonReader` reads the deployment's exact Current Session owner address through `ICampaignPersistenceStore.ReadRecordsAsync`. It validates the bounded `GameplayEntrySessionPlan` reference projection: actual trusted input SHA, authoritative mode/modules, canonical retrieval signals, explicit required specialist source/snippet IDs and exact read obligations. Mode/modules must also agree with deployment configuration. Scene signals are used only after these canonical reads; model terms remain separately recorded hypotheses. There is no sentence parser, semantic classifier, story matching or inferred specialist coverage.

Required roles are CurrentSession, PlayerState, Controller, Protagonist, Incarnation, Time, Scene and Placement; RelevantOwner covers further declared-intent owners. At most 32 obligations /65,536 UTF-8 Canon bytes are supplied. Missing address/record, restricted visibility, missing roles, duplicates, stale input/version/revision, malformed projection and oversized context block. Existing uncertainty text remains unchanged in supplied records; a successful physical read is not a declaration that its claims became Certain. LegitimatelyAbsent requires explicit owning projection evidence, no address/revision/hash and a bounded reason. Unavailable, Stale, Incomplete and Failed remain distinct bounded statuses. Internal causes are retained without disclosing raw exceptions. Required-read failures return safe interaction/read evidence, not a partial executable context.

The Current Session projection is **A - reference adapter behavior**, not a new universal Canon owner/schema or an automatic writer. A deployment must map/adapt its actual authoritative representation and intent-bound obligations; incompatible or missing representation fails closed. Preparing that compatible evidence is not delegated to a model-facing tool. The narrow `IGameplayEntryCanonReader` seam permits other providers, but cannot waive minimum evidence at the final store. No full Canon traversal, record ranking, historical reconstruction or FR-028 closure exists.

After Canon, the service automatically invokes existing `CompiledRuleRuntime.GetContextAsync` with fixed `gameplay.resolve`, the bound active semantic artifact, immutable source/profile, configured world/mode/modules and explicit specialists. Existing FR-026 A-E normalization, eligibility, ranking, dependencies and budgets remain untouched. Actual packet members must contain established Kernel and GM Runtime Procedure source identities, not guessed section anchors. Required overflow/missing authority propagates existing FR-026 failure codes. Optional exclusions are recorded, never permission to execute omitted authority.

Explicitly selected SourceRelease bindings use the existing `PublishedRuleContextProvider` and compact formatter with equivalent mandatory Kernel/Procedure, profile and required-ID checks. Unready preparation returns existing `RULE_CONTEXT_PENDING`; unavailable legacy authority is a bounded entry failure. Selected compiled failure never falls back to legacy. The legacy regression proves changed rule identity cannot reuse old input; explicit B reconfirmation and actual new C submission are required by existing policy.

## Durable Baseline and Atomicity

Migration 014 had interaction/decision/transition aggregates but no bounded context-preparation or validated entry receipt. Additive opt-in **015_gameplay_entry** adds only one scoped entry row per existing interaction, unique stable Request ID/hash, bounded strict JSON, positive preparation revision and foreign-key ownership. Both default and routed template ship to build/publish output. B/C/D opt-in controls planning; repeat application preserves 014, saves, decisions, rule history and active publications. Migrations 001-014 are untouched. No automatic down migration is promised; use existing verified backup/controlled restore policy.

Gathering Canon/rules holds no long SQL transaction. Finalization reacquires B/C's session lock in a Serializable transaction and rechecks current binding identity/generation/revision, original version/last validated save, active rule/profile identity, bootstrap hash/state/revision and pending persistence/decision linkage. The same existing effective-record query rechecks required owner addresses/revisions/payload hashes under that transaction. Minimum evidence and returned-record correspondence are independently checked; extra unauthorized records or incomplete provider evidence cannot pass.

The original baseline is deliberately conservative: any change to captured version/profile/bootstrap evidence blocks rather than silently rebasing input. Unresolved persistence blocks even if an active campaign header exists. Receipt and OPEN/revision advance commit together. Cancelled, blocked, stale, suspended or superseded preparation cannot finalize. The receipt records original interaction/binding/generation, baseline/profile/readiness, retrieval scope/IDs/closure/budget/exclusions, owner evidence, pending-decision protected references, revision/time and original correlation. It stores neither packets nor Canon dumps.

## Retry, Decisions and Races

Same request/fingerprint recovers the original receipt; no second input/entry row, context reinterpretation, refreshed baseline, world simulation or Canon write. Only the first winning acknowledgment supplies executable context. Receipt-only recovery is an authorized retrieval/evidence reference, not a new OPEN grant. Recovery after cancellation returns CANCELLED and the historical receipt with no executable context. Interrupted preparation remains ENTRY_PENDING and the same request can finish after restart. Conflicting request/payload and stale revisions fail deterministically.

A related actual C reply can enter while its decision remains Pending. Protected question/response-scope references are returned only in authorized entry context. No alternative is inferred, no decision is resolved, and the original AWAITING_PLAYER_INPUT interaction stays yielded. D does not classify semantic answers or seal completion.

Real SQL tests coordinate preparation barriers with concurrent requests. Duplicate calls converge on one receipt; entry/switch obeys C's blocker and remains on the original Campaign. Suspension/cancellation, version advancement, record revision change, bootstrap revision change, active artifact change and unresolved save outcome prevent stale OPEN. Cancellation after receipt/OPEN writes but before commit rolls both back; same original request can subsequently finish. Evidence/history is never deleted to recover.

## Diagnostics and Failures

Existing recorder/error registry are reused, not a GM Turn Trace subsystem. Diagnostics contain current call correlation, original interaction correlation/operation identity, binding/campaign, phase, selected artifact, fixed mode, closure/read statuses, expected/actual revision, baseline version/readiness, final state and retry outcome. They omit raw player text, hint strings, Canon payloads, protected question content, full packets, credentials and reasoning. Diagnostic failure cannot revoke a committed entry or replace the original blocking result.

Registered `GAMEPLAY_ENTRY_*` codes cover disabled/invalid request, conflict, stale baseline, profile mismatch, incomplete rules and unavailable/stale/incomplete/failed Canon. Existing B/C, bootstrap, progressive-preparation and FR-026 codes preserve authority, readiness, decision, persistence and retrieval distinctions. `GAMEPLAY_ENTRY_OPEN` and `GAMEPLAY_ENTRY_RECOVERED` are successful dispositions, not errors. Unauthorized/missing pre-entry authority returns no partial disclosure.

## Executable Evidence

Focused tests are in `GameplayEntryTests.cs`: contract/tool/input boundary, registry, packaging, real SQL entry and exact packet equality against FR-026, absence/read failures, missing mandatory Procedure/specialist/required budget, duplicates/lost ack/restart, races, rollback, decision continuity, session isolation, 014 upgrade/repeat/opt-in, explicit legacy compatibility, safe inner-cause retention, final minimum-evidence/extra-disclosure denial and historical receipt recovery.

Actual MCP child processes list the new tool, reject absence of trusted input without creating campaign/interaction/entry state, enter existing test-attested input through automatic rules/reads, then restart/retry and recover the same receipt. The model-facing schema has no Campaign authority argument and no submission-minting tool. Existing B/C process expectations now recognize D registration while retaining all no-intake/ownership/decision assertions; historical audits and frozen corpus fixtures are unchanged.

Final validation against the reviewed implementation:

| Check | Result |
| --- | --- |
| Portable library, standalone compiler and reference MCP Release builds | Passed; zero build warnings/errors |
| Reference MCP Release publish | Passed; both 015 assets, distribution-metadata.json, worker, LICENSE and NOTICE present |
| Focused D tests | 50 passed: 12 contract cases and 38 disposable SQL cases, including two actual MCP process cases |
| Complete portable suite | 1,144 passed, zero failed/skipped |
| Complete CLI/process suite | 65 passed, zero failed/skipped |
| Complete SQL-enabled Managed suite | 473 passed, zero failed/skipped |
| Unique complete-suite total | 1,682 passed; focused cases are included, not counted again |
| Repository structural harnesses | 365 assertions passed across ten harnesses |
| Markdown/navigation validation | 331 files, 7,786 links, 246 anchors; passed |
| Repository indexes/governance/boundary checks | 197 canonical documents, 43 templates, 36 Future Revisions; zero blocking questions, orphaned documents or forbidden campaign-data directories |
| Deterministic distribution validation | Passed byte/content checks using temporary review archives; no release artifact published |
| Working/staged whitespace and scope checks | Passed; VERSION, tags, historical migrations and rule/compiler corpus untouched |

Tests use `dotnet test <project> -c Release --logger 'console;verbosity=minimal'` for `EternalCycle.Rules.Tests`, `EternalCycle.Rules.Compiler.Tests` and `EternalCycle.Persistence.Mcp.Tests`; the Managed run sets `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1`. The focused filter is `FullyQualifiedName~GameplayEntryContractTests|FullyQualifiedName~CompiledRulesArtifactSqlImportTests.D_`. Release builds use `dotnet build <project> -c Release --no-restore`, and MCP packaging uses `dotnet publish <project> -c Release --no-restore`. Full repository validation uses `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate_repository.ps1`; whitespace uses `git diff --check` and `git diff --cached --check`.

Final review found SQL CHECK's nullable semantics could admit a missing/nonnumeric preparation revision. The draft 015 constraint now explicitly requires a non-null converted positive revision, following 014's existing pattern. Four real SQL regressions reject missing, malformed, zero and negative values without changing the original pending entry; valid recovery still succeeds. Both migration variants and complete Managed validation were rerun after that correction. This is a D storage-integrity repair, not a historical migration edit.

## Compatibility and E Handoff

Complete portable/CLI and SQL-enabled Managed suites cover FR-020 setup, FR-021 durable worker/leases/recovery and fail-safe diagnostics, FR-025 import/trust/history, FR-026 canonical/historical retrieval, B binding and C lifecycle. No compiler, vocabulary, ranking, dependency, budget, source corpus or artifact identity changes are included. Reference adapter wiring is shared; no new worker, protocol, save system or Campaign data is introduced.

**E still must gate `ec_commit_changes`, `ec_patch_records`, `ec_retry_persistence`, `PersistenceCoordinator` and durable store activation/write paths with entry/state/owner/version/decision scope; seal decision/yield/completion before delivery; and preserve frozen retry behavior.** Those existing paths still accept SourceInteractionId without D authorization checks. Both returned mutation-authorized fields remain false. Existing generic rule tools/readiness are not entry grants. No currently supported route is falsely advertised as fully FR-027-enforced.

## Classification and Limits

- **A - Reference:** .NET coordinator/reader, SQL session lock and migration 015, Current Session JSON projection/address map, LocalDB fixtures, stdio tool and bounded structured responses.
- **B - Managed contract:** existing trusted input/binding, mandatory required rules/minimum owner reads, durable baseline/evidence, non-provisional preparation, atomic freshness plus OPEN, idempotent recovery and bounded failures.
- **C - Eternal Cycle invariant:** no resolution without actual input, authoritative Campaign identity, required rules and Canon; no invented choice or saved consequence.

Unsloth/LM Studio real-host ingress, remote/production SQL, full mutation/yield enforcement, complete GM Turn Trace and integrated/live acceptance remain unproven or pending E/F/G. A test ingress is not production attestation. FR-028 scalable Canon closure and FR-031 whole-context economics are not implemented. No performance/SLA or release claim. Next authorized boundary is E only, awaiting owner selection; do not begin it in D.
