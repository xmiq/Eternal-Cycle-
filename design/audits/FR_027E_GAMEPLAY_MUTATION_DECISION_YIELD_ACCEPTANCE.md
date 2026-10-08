# FR-027E Gameplay Mutation, Decision and Yield Acceptance

## Checkpoint and Decision

Owner-selected E only began from clean synchronized `main` at `3c034684faf7038c16bbfe7be7875193d2f625b6` (`feat: enforce mandatory gameplay interaction entry`). VERSION remains `1.0.0`; immutable `v1.0.0` remains `6938bd2f9e965faaeb1c612b1dd765237cadc8a8`; moving `v1.1.0-rc` remains `4eb2583027d316a962fd49fe318781a0e060183e`. Phase 13 stays Active; FR-027 remains selected/incomplete; no release or downstream objective is included. The [accepted contract](../../docs/ai/CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md), [A scenarios](FR_027A_CAMPAIGN_BINDING_GAMEPLAY_ENTRY_CONTRACT.md) and [execution plan](../FR_027_EXECUTION_PLAN.md) own semantics.

**FR_027E_ACCEPTED** is the service/reference checkpoint decision after the exact final validation below. It is not Unsloth/LM Studio, remote SQL or whole-game acceptance. A-E complete; F-G pending. Reference stdio has no authenticated human-event adapter. Production gameplay requires one and a compatible authoritative projection; no test adapter was registered in production.

## Mutation-Surface Inventory

| Surface | Classification and E boundary |
| --- | --- |
| `ec_commit_changes` / coordinator Commit / SQL store Commit | Gameplay Canon: full writes, entities, actor state, inventories/resources, relationships, scenes/locations and world/time state represented by canonical records share one final gate. No specialist owner bypass exists. |
| `ec_patch_records` / coordinator Patch | Gameplay Canon: existing set-only patches become frozen full mutations and reach the identical store gate. No new patch semantics or persistence schema. |
| `ec_retry_persistence` / store Retry / E reconciliation | Frozen originally admitted transaction only; no new input, effect, payload or retargeted campaign. Historical validated receipts remain readable after yield/completion. |
| Canon Create / Tombstone inside commit | Gameplay Canon: exact declared action plus verified presence/revision or explicitly permitted creation absence. A Current Session projection cannot authorize rewriting itself. No physical-delete tool. |
| Persistence status/record reads, interaction/entry receipts, rule retrieval and diagnostics | Read-only gameplay evidence, not renewed authority or a player submission. Existing access policies remain; E owner reads additionally persist exact control-plane read proof. |
| Campaign binding resolve/select/switch/suspend and trusted intake | Separate B/C control plane: current generation, actual host submission and existing switch blockers, not model choice or Canon writes. |
| Schema/bootstrap, campaign creation, artifact import, rule compilation/publication/activation, worker control | Existing explicitly authorized setup/administrative operations, not active Canon gameplay; their existing grants/approvals remain unchanged. Rule publication alone cannot grant gameplay authority. |
| Separate administrative Canon commit deployment | Exact trusted `GameplayAuthority.AdministrativeCampaignIds`, default empty, only when gameplay binding is disabled. Ignored in enabled gameplay deployments; no model argument can select it. |

All public canonical write paths converge on `SqlServerCampaignPersistenceStore`. Default/unconfigured legacy Canon writes now fail closed instead of becoming an undocumented administrative grant. A separately authorized operator may perform continuity maintenance using the explicit policy; the affected gameplay baseline then becomes stale. This intentional tightening does not remove supported setup/migration/import paths.

## Authority and Canon Scope

`GameplayAuthorityService` reuses the existing SQL Player Interaction aggregate, binding scope and persistence coordinator. New `SqlServerGameplayAuthorityStore.cs` is a partial of that same store, not another save engine. Trusted configured principal/logical session/authority and authorized campaigns never come from tool arguments. Each mutation verifies the original C interaction, exact Campaign/binding generation/access, current ACTIVE non-superseded binding, OPEN state, durable D entry ownership/revision and complete `gameplay.resolve` evidence, active profile/bootstrap, known persistence and absence of a pending decision.

D's authoritative reference Current Session projection carries at most 32 exact `GameplayMutationPermission` record/action pairs. These are frozen into its existing entry receipt. Empty scope preserves older D receipts but grants no writes. Only the compatible canonical projection/host adapter may establish intent-bound scope; no model-facing tool writes that projection. Minimum owner revision/payload-hash evidence is rechecked at the current verified baseline. `ec_read_gameplay_owners` extends evidence only within declared scope (32 addresses per request, 64 accumulated, bounded Canon bytes). A missing record is not empty except an explicitly permitted Create with verified absence. Material references require verified owner evidence or an authorized same-transaction creation. There is no graph traversal, semantic intent/answer matcher or FR-028 closure.

Own validated mutations update only their exact read evidence and advance the already-declared baseline. Unaffected owners are reverified; this is not silent rebasing after external changes. Incompatible version/owner/profile/bootstrap changes stop with existing structured stale evidence. The original binding and originating baseline remain available for history; no switch redirects pending work.

## Transactions and Persistence

Final admission, candidate record writes and PERSISTING/unknown state commit in the same short Serializable SQL transaction under B's session lock, acquired before campaign/version locks. Activation reacquires that same authority boundary and verifies original payload, owner evidence, parent version, binding/profile/bootstrap and decisions. Final receipt validation atomically records Completed/Validated persistence evidence, verifies actual resulting records/version and changes the interaction back to OPEN with known persistence. No transaction spans model reasoning, durability-service work or tool delivery.

An unrelated non-Completed campaign transaction blocks a new candidate, preventing competing rows at one candidate version. Duplicate stage/activation/finalization recheck durable status under the lock; completed or already activated work cannot regress to CandidateValidated. Frozen request collections are copied before awaits. Same operation identity/hash recovers the original save and receipt; conflicting identity/payload fails. Multiple legitimate save units retain contiguous resulting versions and ordered receipts; conclusion requires every one, not just the newest.

Individual gameplay save results truthfully report the validated persistence marker but return `GameplayCompletionRequired=true` and `TurnMayComplete=false`. Save validation and interaction completion are different grants. Manual save/retry commands cannot create a player turn or repeat effects. Administrative save results retain the pre-existing separate completion convention.

Pending, failed or lost acknowledgment leaves the original admitted unit PERSISTING/unknown or BLOCKED with PERSISTING recovery evidence. A bounded independent best-effort control-plane update marks BLOCKED after failure/cancellation; inability to write that status leaves the already-durable unknown evidence fail-closed. Only the same frozen admission may reconcile. Stage/activation/durability/readback failure never authorizes narration or a replacement transaction. Existing committed records are not undone when delivery, suspension or later validation fails.

## Decision, Yield and Completion

`ec_conclude_gameplay_interaction` requires stable Request/Interaction IDs, expected revision and explicit Affected Set. It compares the exact owner union of admitted original save requests, Completed status/hash/input ownership and every Validated receipt with the resulting baseline. Zero admissions plus explicitly empty Affected Set additionally requires unchanged baseline and verified owner reads; missing affected evidence, nonempty unsaved changes, failed/unknown work or a missing receipt blocks. No caller `completed=true` exists.

For a question, protected alternatives and allowed-response references are inserted into C's existing pending-decision store before the same transaction yields to AWAITING_PLAYER_INPUT. Only then is `QuestionPresentationAuthorized` returned. Otherwise the same atomic transition records COMPLETED and `CompletedNarrationAuthorized`. Competing conclusion requests serialize/idempotently recover one result. Historical question redelivery cannot present resolved alternatives as still pending. A completed interaction stays completed after restart.

Only new authenticated C ingress can attach `TrustedDecisionResponse` to an actual response. Its fingerprint preserves disposition, protected response/scope references and optional choice reference; absent response evidence serializes exactly like historical C submissions. The related reply gets its own interaction and must pass D. `ec_resolve_player_decision` has identities/revisions only, no answer or choice argument. It verifies exact pending decision/response ownership, baseline, revision and allowed scope. Choice, rejection, correction and change of direction preserve actual response provenance and resolve the boundary, but execute no fictional option themselves. Clarification remains pending/yielded; a later actual response can still enter and answer. Missing trusted classification fails closed rather than using conversation similarity. Originating interactions never regain forward authority.

YIELDED remains the derived disposition of AWAITING_PLAYER_INPUT, COMPLETED, BLOCKED or CANCELLED, not a new lifecycle state. Tool activity, Canon/rule/status reads, receipt lookup and reconnect neither mint trusted input nor select alternatives. Pending choices and unknown writes block unsafe switching; suspension revokes activation/acknowledgment authority. Control-plane cancellation cannot abandon uncertain persistence by declaring it known.

## Managed and MCP Surface

New narrow tools are `ec_conclude_gameplay_interaction`, `ec_resolve_player_decision`, `ec_read_gameplay_owners` and `ec_reconcile_gameplay_persistence`. Existing entry, recovery/status, full/patch/retry tools remain; their write paths are now gated in the store, not merely decorated. No submission-minting, arbitrary state setter, choice override, completion override or uncertainty override is exposed. Reconciliation returns a deliberate unsuccessful Managed result when the original save remains unvalidated; its actual persistence status remains available.

Official reference instructions require consuming admitted completion/presentation results. This is the strongest enforceable service boundary: MCP cannot physically prevent arbitrary unsupported natural-language text. A host must attest actual submissions/response classification, supply authoritative intent-bound projection and honor the disposition before delivery. Missing support stops; documentation does not claim it exists.

## Storage and Migration

**No new migration.** Existing 014 bounded interaction/decision JSON and transition receipts retain optional progress/response data; 015 entry JSON retains exact scope; existing Canon save/receipt tables own all mutations. Null optional response/progress fields are omitted, preserving historical intake fingerprints and aggregates. Evidence is bounded before persistence. Schemas 001-015 and their history remain unchanged, repeat application preserves completed state, and no upgrade/reset/history rewrite is required. Existing constraints/unique receipts and B's session lock retain ownership; reference test-only barriers are internal and unconfigured in production.

## Diagnostics and Classification

Existing recorder/error registry carry bounded call/original correlation, interaction/binding/Campaign IDs, entry receipt identity, request/transaction references, owner IDs/revisions, decision status, lifecycle/yield, persistence status and completion/presentation grants. Twelve new `GAMEPLAY_*` codes distinguish entry/authority, scope/read, decision/trusted-response, yielded/completed, unknown persistence, completion, Affected Set and conflicting replay. Existing B/C/D stale/ownership and persistence result semantics remain authoritative. No full GM Turn Trace is implemented.

Storage failures now persist/return safe generic reasons and log only transaction/error type, never raw exception messages or payloads. An internal non-serialized cause remains available; E diagnostic events record its type, not arbitrary payload-bearing text. No raw player input, full Canon, GM Secrets, credentials, protected alternative contents, hidden reasoning or conversations are emitted as E diagnostics. Diagnostic failure cannot manufacture or revoke authority.

- **A - Reference implementation:** .NET/SQL partial aggregate, Serializable transaction/session locks, Current Session projection/action map, LocalDB fixtures and stdio adapters.
- **B - Managed Service contract:** authoritative mutation/decision/yield/completion gates, frozen durable identities, receipt/empty-Affected-Set validation, original-outcome reconciliation and bounded evidence.
- **C - Eternal Cycle invariant:** no advancement without actual player authority; no choice without actual trusted input; no completed consequences without validated persistence. SQL layout, flags and .NET mechanics are not universal rules.

## Executable Evidence and Review

`GameplayAuthorityTests.cs` exercises contract/MCP surface and registered errors plus real disposable SQL. Denials cover unbound/fabricated/wrong-session/wrong-Campaign/pre-entry/superseded/suspended/stale-owner/version/bootstrap/out-of-scope/closed/choice-pending/uncertain authority and unchanged Canon. Full multi-owner, set-only patch, explicit creation absence and Tombstone routes use the existing ledger. Save retries, payload conflict, all ordered receipts, missing receipt, explicit empty versus omitted/mismatched affected sets, failed/unknown outcome and restart reconciliation are executable regressions.

The three-alternative motivating regression records protected decision references before presentation; assistant continuation/tool/lookup attempts cannot choose or persist option two across service recreation. Choice/rejection/correction/change-direction, clarification then a real later reply, missing trusted classification and stale decision revisions are tested separately. Repeated delivery is idempotent; no clarification is converted into choice.

Real SQL barriers/races exercise write versus valid second-Campaign switch, suspension, cancellation, version/owner/profile changes, question/completion/yield, duplicate or conflicting write identity, competing responses, response versus valid switch, competing completions and unknown acknowledgment/restart. Known B/C blocker codes vary according to the serialized winner (pending decision versus open response); invalid selection is not used as proof of authority blocking. Three actual MCP process cases prove legacy pre-entry denial, entered write/duplicate/conclusion recovery, pending-choice denial and unknown-outcome reconciliation before completed narration. The test-only authenticated ingress prepares existing database state; MCP never mints it.

Review corrected fixture bookkeeping (the actual binding-status column and receipt-query helper), one denial fixture's mismatched Affected Set so it reaches the intended permission gate, replaced invalid switch handles with valid second-Campaign choices, preserved existing precise blocker codes, tightened failure privacy and separated save-unit success from interaction completion. An unfinished persistence outcome reports unknown persistence, not a false yielded lifecycle. No historical tests, corpus semantics or canonical rule prose were weakened.

## Validation

Final exact-state validation:

| Check | Result |
| --- | --- |
| Portable library, standalone compiler and reference MCP Release builds | Passed, zero build warnings/errors |
| Reference MCP Release publish | Passed; exact distribution metadata, Apache LICENSE/NOTICE, worker and both 015 assets verified |
| Focused E tests | 65 passed: 14 contract and 51 SQL/process cases, including three actual MCP process cases; zero failed/skipped |
| Complete portable suite | 1,144 passed, zero failed/skipped |
| Complete CLI/process suite | 65 passed, zero failed/skipped |
| Complete SQL-enabled Managed suite | 538 passed, zero failed/skipped; final exact-code rerun also 538 passed |
| Unique complete-suite total | 1,747 passed; focused tests are included, not counted twice |
| Repository structural harnesses | 371 assertions passed across ten harnesses, including 38 FR-027 contract/connection assertions |
| Markdown/navigation | 332 files, 7,798 relative links, 246 anchors; passed |
| Repository indexes and governance | 197 canonical documents, 43 templates, 36 Future Revisions; zero blockers/orphaned documents/forbidden campaign-data directories |
| Deterministic distribution | Two builds of the staged full-source snapshot, 533 entries, byte-identical archives; content validation passed, zero bin/obj/cache/temp paths |
| Working/staged whitespace and protected-path review | Passed; VERSION, tags, migrations 001-015, compiler/canonical corpus and release metadata untouched |

Commands use Release `dotnet build/test/publish` for `EternalCycle.Rules`, `EternalCycle.Rules.Compiler` and `EternalCycle.Persistence.Mcp`; complete tests use the corresponding `.Tests` projects. Managed sets `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1`; the focused filter is `FullyQualifiedName~GameplayAuthorityContractTests|FullyQualifiedName~CompiledRulesArtifactSqlImportTests.E_`. All repository harnesses run through `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate_repository.ps1`. Final staged-state tests, validator and whitespace are repeated before commit.

Distribution verification uses `git checkout-index` to materialize the staged full useful source snapshot into a fresh temporary directory, then `tools/build_distribution.ps1` twice and `tools/test_distribution_archive.ps1`. This excludes unrelated untracked files without changing packaging semantics or removing reference tooling. It is validation, not release publication. No generated artifacts are staged. An unrelated untracked `EC.zip` appeared during work; it remains untouched, uncommitted and excluded from these snapshots. The tracked tree is clean after the focused commit; the full working tree still contains that unrelated file.

## Limits and F/G Handoff

F/G remain pending: F owns complete bounded GM Turn Trace and operational recovery presentation; G owns integrated actual-host acceptance. Production Unsloth/LM Studio authenticated human events/response evidence, authoritative Current Session projection integration and admitted narration delivery remain unproven. Real LocalDB does not prove remote SQL authentication, production concurrency/SLA or arbitrary model prose control. No live campaign database was used. No FR-028 graph closure, FR-029 history redesign, FR-030 result redesign, ranking/budget/corpus changes or downstream release work is included.

The supported real-host sequence is actual trusted input -> binding -> D entry -> exact scoped writes -> validated receipts -> explicit conclusion/disposition -> host delivery. For alternatives, stop after recorded yield; assistant/tool continuation must remain denied; only a later actual entered response may resolve. Repeat after transport restart, deliberately interrupt acknowledgment, reconcile the same transaction and verify no duplicate effects. These are G acceptance obligations, not claimed results here. Stop after this E commit/push; await owner selection of F only.
