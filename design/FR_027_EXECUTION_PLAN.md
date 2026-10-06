# FR-027 Campaign Binding and Gameplay Entry Execution Plan

## Authority and Current Checkpoint

FR-027 is the sole owner-selected incomplete Future Revision. Its [governed objective](V1_1_FUTURE_REVISION_PLAN.md#fr-027---stable-campaign-binding-and-managed-gameplay-entry) is **Stable Campaign Binding and Managed Gameplay Entry**. A-G are execution checkpoints beneath it, not new Future Revisions. The owner selected A from clean synchronized `main` at `8b0168da8654fb5b40666f3a2ed01ade54814d34`, after closed FR-026/R2.

**A Complete (contract only); B-G Pending. FR-027 remains selected/in progress and Roadmapped, not Closed.** The [canonical contract](../docs/ai/CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md) and [A audit](audits/FR_027A_CAMPAIGN_BINDING_GAMEPLAY_ENTRY_CONTRACT.md) are the authority for implementation. Runtime enforcement remains absent in A. Phase 13 stays Active; FR-028 through FR-036 stay pending/unselected. VERSION remains `1.0.0`; no tag/release action is authorized.

## Architectural Inventory

Current reference tooling already has campaign directory/unique-resume choices in `ManagedAdministration.cs`; active versions/record reads, full commit/set-only patch/retry and receipt checks in `Contracts.cs`, `PersistenceCoordinator.cs` and `SqlServerCampaignPersistenceStore.cs`; bootstrap readiness in `GmHostConfiguration.cs` and `ManagedReadiness.cs`; explicit compiled retrieval in `CompiledRuleRuntime.cs`, `SqlServerCompiledRuleRetrieval.cs` and `SqlServerCompiledRulesArtifactStore.cs`; and transport-independent administrative work in Managed Operations/Worker components. Schema 010 owns bootstrap readiness, 011 artifact import, 012 separate publication/activation. None is a durable player-input authorization/binding store. `SourceInteractionId` currently passes identifier validation, not an OPEN-state check. Reuse these foundations; do not replace the compiler, packet engine, persistence authority, or worker.

## Package Discipline

Reread only this plan, the canonical contract, the package's linked owners, its affected implementation/tests and previous package audit. Do not reread every historical FR audit for each checkpoint. Each package must leave a coherent tested boundary, record exact results/limits, review/stage the complete scoped diff, run repository/whitespace validation, commit and push normally when authorized, verify synchronization, and stop. Do not start the next package automatically.

Do not expose partially wired capabilities: persistence compatibility remains unchanged until E can enforce all gameplay writes, and the new entry route remains unavailable to gameplay until its full gates are connected. Internal seams in B/C/D are not advertised enforcement. Any required schema change is additive, justified by a demonstrated gap, preserves existing saves/transactions, and follows existing migrations; no migration number is reserved by A.

## Execution Packages

### FR-027A - Contract and Architecture

- **Status:** Complete, design only.
- **Scope:** durable routing/input semantics, state machine, trusted-origin boundary, entry/read/rule/agency/persistence gates, trace/errors, acceptance matrix and this decomposition.
- **Validation:** all structural harnesses, full repository links/indexes/governance, deterministic distribution and whitespace. No executable/SQL behavior or live-host acceptance.
- **Stop:** validated focused documentation/governance commit and normal main push. B-G not started.

### FR-027B - Durable Campaign Session Binding

- **Status:** Pending. **Prerequisites:** A; current FR-020 discovery, canonical configuration/Save Index and authorization.
- **Objective:** implement session-scoped durable bindings, safe zero/one/multiple discovery/selection, verified version/profile references, recovery/suspension and explicit switch generations. Decide the actual trusted logical-session adapter/configuration, never a transport PID or story fingerprint.
- **Likely files:** new small binding contract/store and tests within Managed reference tooling; `ManagedAdministration.cs`, schema/migration packaging only if required, existing routing/readiness integration, A contract and B audit. Portable rule library remains untouched.
- **Acceptance:** matrix S01-S06/S24/S30/S31/S34/S35/S40; exact campaign/access/generation isolation, meaningful selection, no semantic matching, atomic successor switching, no retargeted old handle; setup/adoption handoff only to verified authority. Persistence/entry callers cannot yet claim enforcement.
- **Validation:** focused binding tests; real disposable SQL restart/concurrency/upgrade-repeat tests if durable SQL is introduced; existing discovery/routing/readiness/migration and complete affected Managed suite; repository/structural/distribution/whitespace checks. No populated fixture checked in.
- **Exclusions/stop:** no Player Interaction executor, gameplay mutation gate, automatic Canon closure, Campaign creation redesign or adoption orchestrator. Stop after durable binding checkpoint.

### FR-027C - Trusted Player Interaction State and Authorization

- **Status:** Pending. **Prerequisites:** B; A trusted-input and state contracts, existing transaction identity.
- **Objective:** persist actual-submission deduplication, interaction/binding/version scope, gate transitions, pending decision and original-input recovery. Establish a tested trusted-ingress seam separate from model-controlled MCP arguments. Reuse existing access policy; opaque IDs are not grants.
- **Likely files:** focused Player Interaction model/coordinator/store, minimal additive migration if needed, host-adapter ingress contract/test substitute; binding store and authorized status projection; C audit.
- **Acceptance:** S11-S19/S25/S26/S27-S29/S32/S33/S36; same actual submission -> one interaction; conflicting duplicate rejected; identical text in separate submissions distinct; cannot synthesize new input through tools; pending decisions remain yielded/recoverable; transitions enforce atomic expected-state/generation checks. Explicitly record which real hosts can or cannot attest origin.
- **Validation:** focused state/duplicate/decision/cancellation/restart/race tests, durable SQL isolation/migration tests, existing leases/recovery/transaction compatibility and complete affected Managed suite; structural/repository/distribution/whitespace.
- **Exclusions/stop:** no rule-selection algorithm, new persistence lookup endpoint, history rewrite or gameplay-exposed entry claiming incomplete enforcement. Stop after internal control-plane checkpoint.

### FR-027D - Mandatory Entry and Existing Rule/Read Gates

- **Status:** Pending. **Prerequisites:** C; FR-026 actual retrieval, bootstrap readiness and existing exact-record read path.
- **Objective:** connect semantic Begin Gameplay Interaction to trusted binding/submission, explicit `gameplay.resolve`, automatic Kernel/Procedure, authoritative mode/profile/scene signals and minimum versioned read obligations. Preserve ENTRY_PENDING until evidence permits OPEN.
- **Likely files:** entry coordinator and reference tool adapter, `CompiledRuleRuntime.cs`/legacy context facade (reuse only), `ManagedReadiness.cs`, `GmHostConfiguration.cs`, exact-record reads, D audit and integration fixtures.
- **Acceptance:** S07/S09/S10/S23/S37/S38; generic retrieval never opens an interaction; correct one-artifact FR-026 packet; bootstrap hash/confirmation distinction; unmet reads/coverage/required overflow stop; supplemental retrieval resumes same input without semantic guesses. New route remains non-default until E closes alternate writes.
- **Validation:** focused entry/read/rule/visibility tests; reference/real SQL retrieval equivalence and existing FR-026/legacy/host-readiness suites; affected Release builds and full Managed suite; structural/repository/distribution/whitespace. No re-curation of canonical corpus.
- **Exclusions/stop:** no FR-028 active graph loader, new matching/ranking/budgeting or full runtime economics. Stop after entry internal integration checkpoint.

### FR-027E - Mutation, Pending Decision, Narration and Yield Enforcement

- **Status:** Pending. **Prerequisites:** D and C; complete existing commit/patch/retry/receipt path.
- **Objective:** gate every gameplay mutation at the durable persistence boundary; integrate open/read/coverage/scope/version authorization, transaction freezing, pending-decision yield and narration-completion evidence, including service-admitted host response delivery so presenting a choice cannot leave OPEN authority. Enable the supported entry route only when alternate gameplay writes cannot bypass it. Retain separately authorized administrative paths.
- **Likely files:** `PersistenceCoordinator.cs`, `SqlServerCampaignPersistenceStore.cs`, `Contracts.cs`, `PersistenceTools.cs`, interaction/binding coordinator, activation/receipt guards, exhaustive mutation-surface tests, E audit.
- **Acceptance:** S08/S12/S15-S23/S28/S29/S33/S36/S39; deny absent/wrong/stale/closed/choice-pending grants, full and patch alike; frozen retry only; commit-versus-yield races coherent; successful receipt/explicit empty result permits completion; no post-yield effect; manual commands/lost ack no replay; existing committed effects never undone by cancellation/switch.
- **Validation:** focused agency/entry-bypass/commit/patch/retry/denial/readback tests; real disposable SQL atomicity/concurrency/history/migration and full persistence/FR-011/017 regressions; complete Managed suite and all affected builds; repository/structural/distribution/whitespace. Denial tests verify no storage change, not just an error string.
- **Exclusions/stop:** no new FR-030 result surface, FR-029 history redesign, scheduler, mechanics, or adoption orchestration. Stop after all mutation surfaces and supported runtime route are coherent.

### FR-027F - Bounded GM Turn Trace and Operational Recovery

- **Status:** Pending. **Prerequisites:** E; existing safe diagnostics/error registry.
- **Objective:** connect bounded trace/semantic failures and durable session/decision/result recovery to supported authorized status surfaces. Document real-host ingress/setup guidance without leaking implementation details into successful gameplay.
- **Likely files:** `ManagedDiagnostics.cs`, `ServiceDiagnostics.cs`, `ErrorRegistry.cs`/existing diagnostic adapters, entry/interaction status projections, trace tests and F audit. Reuse diagnostics, not another logging subsystem.
- **Acceptance:** S03/S17/S20-S26/S30-S40; trace evidence identifies binding/input/rules/reads/transaction/version/yield without reasoning or raw campaign content; missing fields honest; error paths bounded, authorized and actionable; no campaign-existence leak; reconnect reports whether actual new input may enter.
- **Validation:** focused trace/redaction/access/pending/lost-delivery/restart tests, full affected diagnostics and Managed suite; repository/structural/distribution/whitespace. No whole-context cost/SLA claim.
- **Exclusions/stop:** no Admin UI/report implementation, FR-031 instrumentation/budget optimization or support-bundle expansion. Stop at connected operational evidence checkpoint.

### FR-027G - Integrated and Live Acceptance

- **Status:** Pending. **Prerequisites:** B-F; no downstream implementation.
- **Objective:** execute all S01-S40 with real disposable persistence, trusted input adapter and end-to-end entry/mutation/restart boundaries. Perform the two motivating regressions in the actual MCP-only host; investigate a failure within FR-027, never release by documentation alone.
- **Likely files:** integrated lifecycle/host fixtures, contract conformance harness, final FR-027 audit and governance closure; corrective runtime edits only where a demonstrated FR-027 defect requires them.
- **Acceptance:** complete matrix with exact evidence; two campaigns/story resemblance never select; alternatives/search before response never choose; real next input resumes decision; mandatory entry always precedes resolution; save gates/recovery/agency work across reconnect. State any host-origin support limitation explicitly. If live proof or an architectural prerequisite remains unavailable, preserve incomplete/blocked scope honestly rather than claiming it passed.
- **Validation:** complete portable/CLI/SQL-enabled Managed regression suites, affected Release/publish/schema packaging, existing FR-011/017/020/021/026 and provider compatibility, all structural/repository/distribution/whitespace checks; exact-state staged reruns; live-host procedure/results recorded separately. Existing authority/corpus identity must remain unchanged absent explicit owner authorization.
- **Exclusions/stop:** no FR-028/029/030/031, adoption, ranking, release/version/tag action. Close FR-027 only on actual acceptance under governance; commit/push, then stop without selecting another objective.

## Dependencies and Context Budget

`A -> B -> C -> D -> E -> F -> G`. This simple chain prevents supposedly independent packages from advertising a half-enforced entry gate. B/C each investigate only their durable data gap; combine a migration only if actually simpler and authorized in the selected checkpoint. The package aliases never enter the Future Revision registry as separate objectives.

Use compact deterministic fixtures: two generic campaigns, one trusted fake input adapter, a small decision, a frozen transaction, and explicit rule requests. Use existing canonical/SQL fixtures to prove rule compatibility rather than copying the corpus. G owns full/live acceptance; package tests prove only their implemented boundaries. A's acceptance matrix is a specification, not executable proof.
