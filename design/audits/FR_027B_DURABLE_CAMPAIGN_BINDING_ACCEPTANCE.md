# FR-027B Durable Campaign Binding Acceptance

## Scope and Checkpoint

FR-027B implements durable routing beneath the sole selected FR-027. The [A contract](../../docs/ai/CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md) remains the authority; [C-G](../FR_027_EXECUTION_PLAN.md#execution-packages) remain pending. Binding existence/validity is not Player Interaction authorization, mandatory gameplay entry, a trusted human submission, mutation/yield enforcement or live acceptance.

Starting checkpoint: clean `main`, `592b8d96a1d3df31084d02cf5db2357421fbe204` (`design: define campaign binding and gameplay entry`), upstream 0/0. VERSION remains `1.0.0`; tag objects remain v1.0.0 `6938bd2f9e965faaeb1c612b1dd765237cadc8a8` and v1.1.0-rc `4eb2583027d316a962fd49fe318781a0e060183e`. The completion commit is the commit containing this audit; the final report verifies its actual SHA and push rather than inventing a self-referential commit identity.

## Interruption Recovery

Inspection recovered the original HEAD/upstream and unchanged version/tags; no B commit had occurred. The working tree contained the service/store, opt-in migration and packaging, registration/configuration/errors, 35 tests, current-HEAD fixture correction, contract/governance progress and audit index. The staged index was empty. Existing correct implementation was reviewed and preserved rather than reset or duplicated.

Continuation finished the acceptance audit, developer notes and limitation record. It strengthened the existing campaign-loss fixture with distinct generic disposable Canon records and explicit no-mutation assertions, and seeded representative save history in the existing upgrade test rather than relying only on retained rule history. Neither change adds production matching behavior. Review also preserved the underlying SQL cause in schema/concurrency failure wrappers; a real missing-schema test verifies internal cause retention while public errors remain sanitized. Structural validation exposed that the existing registry scanner cannot recognize loop-generated definitions; the fifteen binding entries now use the repository's explicit literal registration style without weakening the checker. Final suite counts stay 35/1,144/65/343. No incomplete implementation is hidden behind the audit; C's intentionally absent gate remains fail-closed, not an interrupted placeholder.

## Architecture and API

[CampaignSessionBinding.cs](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/CampaignSessionBinding.cs) defines immutable logical identity/evidence/result records, `ICampaignBindingStore` and the small Managed `CampaignBindingService`. It consumes a trusted deployment scope, not a model-authored principal/session or conversation fingerprint. The principal plus stable logical session form a length-delimited hashed scope; authority/profile change does not silently create a new scope. Separate grants govern unique resume, selection/suspension and switching. An empty explicit Campaign access set grants nothing.

`CampaignBindingChange` accepts only a stable bounded Request ID, exact current choice handle, expected Binding ID/revision and explicit selection approval. It has no story, protagonist, semantic query or arbitrary Campaign ID field. The approval flag expresses routing choice under the deployment grant; it does not attest a real human gameplay submission. Real authenticated input-origin work belongs to C.

The [reference registration](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/Program.cs) exposes:

- `ec_resolve_campaign_binding`: recover/refresh the current binding, or return authorized meaningful choices.
- `ec_select_campaign_binding`: commit an explicit current choice with revision/idempotency evidence.
- `ec_switch_campaign_binding`: commit a verified successor only when the independent safety gate permits it.
- `ec_suspend_campaign_binding`: suspend without deleting history or changing Campaign Canon.

These are thin service mappings, not a parallel MCP-specific binding algorithm. No gameplay-entry tool is registered; every binding result reports `GameplayInteractionAuthorized = false`. Existing first-run creation/discovery and legacy persistence/retrieval ownership remain intact. Disabled/missing trusted configuration is denied before a binding-store lookup.

## Storage and Migration

[SqlServerCampaignBindingStore.cs](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/SqlServerCampaignBindingStore.cs) stores one aggregate per binding generation plus compact successful-change receipts. Additive [013](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/Schema/013_campaign_session_binding.sql) and its [routed template](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/Schema/013_campaign_session_binding.template.sql) are packaged for build/publish. Migration planning offers them only when binding is explicitly enabled. Existing migrations 001-012 are unmodified.

`campaign_session_bindings` owns scope hash, Binding ID, immutable Campaign ID/generation, ACTIVE/SUSPENDED/CLOSED state, revision, predecessor/successor references and bounded operational aggregate JSON. Evidence records configured authority/namespace/world/schema/rules profile, current validated Campaign Version, last validated commit timestamp, selected active rule representation/identity, immutable source identity, bootstrap hash/state/revision, minimum preparation and incomplete save reference. No raw conversation, campaign records, input text, embeddings or semantic fingerprints are stored.

Primary/unique keys protect scope/identity/generation. The filtered unique current-scope index covers ACTIVE and SUSPENDED, so discovery cannot replace a suspended owner. Scoped predecessor/successor foreign keys preserve history; JSON identity/state/revision/generation checks keep the projection consistent. Binary collation is explicit to match ordinal identifiers and avoid cross-database collation conflicts. `campaign_binding_receipts` maps scope/Request ID plus payload hash to the exact resulting binding and original correlation.

The upgrade is repeat-safe and creates no guessed bindings. A real pre-013 disposable database upgrades from the supported migration-012 state twice; bindings remain empty and existing save/rule history is intact. This is forward-only additive evolution: backup and controlled restore are the recovery/downgrade mechanism, not a promised down migration. No real deployment Campaign database is touched.

## Resolution and Freshness

The store reuses FR-020's Campaign directory, filtered to the exact authorized access set; names/descriptions are routing labels, never similarity inputs. Candidate eligibility is checked against canonical data inside the binding transaction. Safe labels include namespace/world; duplicate names receive distinct choice labels, while exact handles remain machine evidence. The handles are deterministic over scope, Campaign and current eligibility/profile evidence; stale/unavailable selections fail without redirecting.

- Zero eligible Campaigns returns `BINDING_UNBOUND`, no Campaign or binding creation, preserving approved setup ownership.
- Exactly one eligible Campaign auto-binds only under the explicit unique-resume policy. Otherwise it still requires a choice.
- Multiple eligible Campaigns returns `CAMPAIGN_SELECTION_REQUIRED` until explicit selection. Neither SQL order, recency nor story resemblance decides.
- An existing current binding is verified first and recovered from durable state. Normal save advancement refreshes evidence/revision while retaining Campaign/Binding ID/generation.
- Missing or revoked Campaign access, lost readiness or version regression suspends/fails safely without substitution. Revoked access returns no binding or alternative-choice details.
- A changed adopted authority/profile suspends and retains the prior adopted evidence until authorized explicit revalidation. A deliberate manual suspension is not undone by status/recovery.

Minimal readiness distinguishes a validated initial save, active legacy release or explicitly selected compiled artifact, minimum preparation, current bootstrap hash and host acknowledgment. `UserConfirmed` remains distinct from `Verified`; readiness/profile evidence is not complete FR-026 artifact verification or mandatory Canon reads. B invokes neither FR-026 retrieval nor `gameplay.resolve`; D must enforce those entry gates. A newer imported inactive artifact cannot become the binding's rules authority merely because it exists.

## Concurrency and Idempotency

All binding mutations use the existing SQL transaction pattern: Serializable isolation plus one transaction-owned application lock on the session hash. C's future interaction mutations must participate in the same lock, rather than racing a separate in-memory check. Durable constraints remain a second storage guard, not a second authority.

Simultaneous unique resume resolves to one current binding. Explicit mutations require the expected binding identity/revision; competing switches have one committed winner and one stable conflict. Successful changes write their receipt atomically with the aggregate. Lost-acknowledgment retries with the same Request ID and payload recover that exact binding; conflicting reuse fails rather than overwriting or retargeting. A replay referencing a CLOSED predecessor returns its historical identity with `BindingValid = false`, never switches the current binding back. Normal resolve without a change request also recovers the same Binding ID.

Cancellation and injected storage failure after predecessor closure roll back successor insertion, current ownership and receipt together. Previous save/rule/binding history remains unchanged. No pending save is reassigned to a successor. No new campaign, duplicate current binding or replacement selection is created by retry.

## Switching and C Safety Seam

Switching is explicit and separately authorized. The target must still be eligible; expected predecessor identity/revision must match; existing original-campaign persistence disposition must be known. Every non-Completed save transaction is conservatively treated as unresolved. Existing complete receipts do not prove every lost acknowledgment/current-session uncertainty; later interaction recovery must add that evidence.

`ISqlCampaignBindingSwitchSafety.CheckAsync` receives the same connection, transaction and binding. It must prove closed interaction, decision and recovery state under the same session lock. Missing or non-clear evidence returns `BINDING_SWITCH_BLOCKED`. **The production reference registers no fabricated always-clear gate: until C implements it, switching and changed-profile/manual-suspension reconfirmation fail closed.**

A deterministic test-only clear substitute proves B's successor mechanism: close the predecessor, insert generation +1, link predecessor/successor and record the retry receipt in one transaction. Campaign ID/generation of the old binding never change. A rollback restores the original current binding. The substitute proves atomic storage behavior only, not absence of real player work or future write admission. Pending decisions/old interaction writes are not implemented or transferred in B.

## Diagnostics and Disclosure

Fifteen stable binding errors are registered using existing Managed failure/guidance conventions. Results distinguish disabled/invalid session, unbound/selection required, unauthorized/unavailable, stale/conflicting, suspended/not-ready/profile-changed, switch conflict/blocked, schema-required and storage failure. SQL causes remain available internally for authorized diagnostics; callers receive safe bounded errors, not SQL messages or connection strings.

The existing recorder receives correlation/Request ID where present, operation/stage, hashed session scope, Binding/Campaign identity where authorized, expected/actual revision, state, predecessor/successor, resolution mode and validity. Durable transition links preserve old ownership; no raw principal/session, narrative, Canon dump or credentials enter diagnostic detail. A lost diagnostic cannot undo an already committed binding. This is operational evidence only, not FR-027F's complete GM Turn Trace.

## Regression Evidence

The [focused tests](../../examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/CampaignSessionBindingTests.cs) contain **35 executed cases: 10 contract tests and 25 SQL-enabled tests, zero skipped**. SQL-enabled cases create/destroy real disposable LocalDB databases, not mocks or the user's save. They cover zero/one/multiple eligibility, disabled/missing scope and grants, safe descriptors/label collisions, exact selection/stale handles, suspension/profile/readiness/version/access changes, principal/session isolation, successor history, lost acknowledgment/retry, conflicting payload/revision, concurrent resume/switch, uncertainty, upgrade, compiled profile selection, cancellation/rollback and bounded diagnostics.

Campaign-loss regression: two eligible Campaigns with distinct generic canonical protagonist/location records, one containing familiar story cues; service returns selection-required, creates no binding, then binds the explicitly chosen other Campaign. The API structurally cannot receive the supplied story as selection authority. Both Canon payloads, record count and Campaign versions remain unchanged; test data lives only in disposable SQL, not campaign-specific repository Canon or a replayed private transcript.

Single-campaign recovery: service/adapter objects are discarded and recreated; exact aggregate/Binding ID/Campaign ID recover without conversation. A separate actual stdio MCP process test initializes MCP, lists the registered binding tools, resolves the sole Campaign, exits, launches a new MCP process and recovers the original Binding ID from SQL. No gameplay-entry tool appears. This proves service/process recovery with the same explicit trusted reference scope, **not Unsloth/LM Studio trusted host identity propagation**.

The complete SQL-enabled suite initially found a pre-existing fixture mismatch: `OfficialRuleCandidateStagesPublishesAndActivatesAtomically` acquires current HEAD but still asserted pre-R2 154/1,004/803 publication rows. Independent [accepted R2 evidence](FR_026H_R2_AUTHORITY_CONTEXT_ACCEPTANCE.md) and the current publication regression establish 130/834/95. Only these three current-HEAD fixture counts and their explanatory comment were corrected. The isolated test passed, then the entire 343-test SQL-enabled suite passed. No normative source, compiler semantics, frozen 10/154/773 artifact fixture, current 11/130/729 corpus or expected retrieval result was altered to obtain green validation.

## Validation

Executed Release validation, reproduced in the continuation from the final source candidate:

| Check | Actual result |
| --- | --- |
| MCP/worker/portable dependency build | Passed, zero warnings/errors |
| Standalone compiler build | Passed, zero warnings/errors |
| MCP Release publish packaging | Passed; both 013 files, metadata, LICENSE, NOTICE and Managed Worker present |
| B focused contract/real SQL/MCP process | 35 passed, zero failed/skipped |
| Complete portable suite, including FR-022-026/boundary | 1,144 passed, zero failed/skipped |
| Complete CLI/process suite | 65 passed, zero failed/skipped |
| Complete SQL-enabled Managed suite, including FR-020/021/025/026 | 343 passed, zero failed/skipped |

These are **1,552 unique full-suite tests**; the focused 35 are a subset, not an extra total. Commands use the established project paths with `-c Release`; SQL execution explicitly enables `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1`. The complete SQL run follows the independently justified current-HEAD fixture correction.

The full repository validator passes **329 Markdown files, 7,757 relative links, 245 anchors, 197 canonical documents, 15 family indexes, 43 templates, five agent roles, 1,281 canonical terms, 208 roadmap tasks and 36 Future Revision entries**. Blocking questions, orphaned Markdown and forbidden campaign-data directories: zero. All ten structural harnesses pass: FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 81, control-plane 35, FR-022 32, FR-027A 32 and release-neutral 17; **365 assertions total**. The A harness remains structural, not live runtime acceptance.

Two temporary distribution builds have equal SHA-256 and pass content boundaries; the validator removes them. No review/release archive is published. Generated build/publish/test output stays ignored and outside source/distribution. Working and staged whitespace checks (`git diff --check`, `git diff --cached --check`) pass. Final exact staged validation reproduces focused B, the complete SQL-enabled Managed suite and repository/structural/distribution counts before commit. Portable/CLI suites were rerun from unchanged portable/CLI source after recovery. Scope review excludes canonical Rule Source prose/manifest, compiler/vocabulary/retrieval semantics, historical artifact fixtures/audits, migrations 001-012, VERSION and release/tag changes.

## Classification and Limits

- **A - reference implementation:** C# records/options, SQL aggregate/receipt tables and migration 013, application lock, configured single-session adapter, four MCP tools, LocalDB/process tests. These are not universal storage/host requirements.
- **B - Managed contract:** durable scoped binding, deterministic authorized resolution, fresh evidence, explicit selection/suspension/successor, atomic ownership, idempotent recovery and independently verified switch safety.
- **C - Eternal Cycle invariant:** no narrative/conversation-based Campaign choice, multiple eligible Campaigns require selection, reconnect never guesses a substitute and unresolved old writes cannot move to another Campaign.

Trusted reference configuration is opt-in and single-session, not an authenticated multi-user host adapter. Reconnecting without the same trusted logical scope cannot recover it by story and must fail/resolve explicitly. Real external host identity continuity, human-origin assurance, remote SQL deployment, pending player-decision state, full unknown receipt disposition, mandatory entry/mutation/yield enforcement and live gameplay acceptance remain unproven/later work. No capability is faked to close B.

## C Handoff and Stop

C must implement trusted actual-submission identity, durable Player Interaction/pending-decision state and safe retry/origin semantics, then implement the transaction-consistent `ISqlCampaignBindingSwitchSafety` gate under B's same session lock. Binding validity alone must never admit an interaction. D/E/F/G retain their original entry, mutation/yield, trace and live-acceptance boundaries. FR-028 onward and adoption orchestration remain unselected.

After exact validation passes, decision is `FR_027B_ACCEPTED`: A/B complete, C-G pending, FR-027 selected/incomplete, Phase 13 Active. Commit/push only this coherent B checkpoint; no VERSION/tag/release/history rewrite or downstream package starts.
