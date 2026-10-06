# FR-027A Campaign Binding and Gameplay Entry Contract Audit

## Decision and Scope

FR-027A is a contract/design checkpoint beneath the sole selected FR-027. The [canonical contract](../../docs/ai/CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md) is accepted; **binding storage, new tools, interaction/mutation enforcement and runtime integration are not implemented in A**. [B-G](../FR_027_EXECUTION_PLAN.md#execution-packages) remain pending. FR-026 stays Closed, Phase 13 Active, VERSION `1.0.0`, tags unchanged. No populated campaign data or private transcript is added.

Starting checkpoint: clean `main`, `8b0168da8654fb5b40666f3a2ed01ade54814d34` (`fix: partition complete ordinary rule authority`), upstream 0/0. Tag objects: v1.0.0 `6938bd2f9e965faaeb1c612b1dd765237cadc8a8`; v1.1.0-rc `4eb2583027d316a962fd49fe318781a0e060183e`. The completion commit is the commit containing this audit; its identity is verified after commit, not a guessed self-referential SHA.

## Established Problems and Evidence

The owner reports two live acceptance failures: campaign identity was lost and the GM chose among multiple campaigns using story resemblance; after offering alternatives the GM searched its own prior conversation and chose before the player answered. These are attributed user reports, not replayed live tests in A. No campaign-specific narrative is copied here.

Code inspection establishes a separate structural gap: `CampaignDiscovery.ResolveResume` in `ManagedAdministration.cs` implements zero/one/multiple discovery but no durable session binding. `PersistenceTools.cs` accepts Campaign/transaction requests; `PersistenceCoordinator.Validate` requires `SourceInteractionId` to be a nonempty identifier but does not verify a durable open Player Interaction. `CompiledRuleRuntime.GetContextAsync` enforces explicit artifact authorization, readiness and the correct FR-026 pipeline when called, but generic rule retrieval is not mandatory entry tied to a real input. These facts establish a missing control-plane connection, not a defect in FR-026's matching/ranking/closure or FR-017's receipt validation.

The [FR-021 bootstrap audit](FR_021_GM_RUNTIME_BOOTSTRAP_REPAIR_AUDIT.md) explicitly recorded its normative host-compliance boundary. The [FR-020 audit](FR_020_MANAGED_FIRST_RUN_BOOTSTRAP_AUDIT.md) records discovery UX rather than durable binding. The [FR-026 R2 acceptance](FR_026H_R2_AUTHORITY_CONTEXT_ACCEPTANCE.md) leaves binding to later work. A does not retroactively claim those completed objectives already enforced FR-027.

## Architecture Decision

The smallest logical control plane has a durable Campaign Session Binding, an input-scoped Player Interaction with gate evidence, and a pending-decision record/reference. Existing Save Index/Configuration, transaction receipts, bootstrap readiness, FR-026 rule retrieval and diagnostics remain their owners. No workflow engine, session framework, event-sourcing replacement or new persistence system is needed.

Binding pins one Campaign and generation within an authenticated logical session, verifies canonical version/profile and recovers durably. Existing valid binding is reused; unique authorized resume may auto-bind; multiple campaigns require safe meaningful choices. Semantic/story matching is prohibited. Switch requires explicit authorization, resolved/cancelled old work, known receipt disposition, closed pending decision, and a new verified generation. Unavailable/inaccessible selected campaigns fail without substitution.

Player Interaction denotes one actual submitted input, not a tool call or the Save Protocol's bounded adjudicated Gameplay Interaction. Trusted host input identity deduplicates retries, never text alone. RECEIVED/ENTRY_PENDING -> OPEN -> PERSISTING -> validated continuation or COMPLETED/AWAITING_PLAYER_INPUT; failures BLOCKED, explicit safe cancellation CANCELLED. YIELDED is the no-forward-authority disposition of closed/waiting/blocked/cancelled states, avoiding two contradictory flags. Multiple bounded saves are possible only for already-declared same-input work under existing Save Update boundaries; no new player choice is licensed.

`ec_begin_gameplay_interaction` is the reserved reference name for semantic Begin Gameplay Interaction, not an implemented tool. It binds/verifies a real submission, enforces bootstrap/profile/pending state, fixes `gameplay.resolve`, obtains automatic Kernel/Procedure and FR-026 specialist closure, and establishes required Canon/read revisions. Required follow-up reads leave ENTRY_PENDING, not provisional OPEN. Entry/read/coverage evidence gates durable write authorization for full commits, patches and every permitted future gameplay mutation. Generic retrieval alone cannot authorize them.

Input-origin assurance is explicit: the service cannot infer a human submission from an arbitrary model-authored string. A trusted host event/authorized human integration is required; unsupported deployments must ask for real confirmation through that channel or stop. This is a deployment capability to prove, not an unresolved excuse to weaken the control-plane invariant. Storage gate enforcement is provider-neutral; a service cannot physically inspect arbitrary model reasoning or prevent an unaffiliated model from outputting false prose. Host conformance and actual live acceptance remain necessary.

A/B alternatives remain proposals; AWAITING_PLAYER_INPUT forbids choice/simulation/mutation. Only a new actual related submission supplies the answer and opens a new linked interaction. Certain correction, investigation and clarification stay distinct, with existing authority/retcon permission preserved. Failed/unknown/pending persistence forbids completed durable narration. Same-ID retry/readback cannot replay effects. Delivery failure redelivers a validated result, not another turn. Post-yield reads never advance the world. Setup/resume/adoption retain their existing administrative boundaries.

The service-admitted host response disposition records a pending decision or validated completion before player-facing delivery, so prose does not leave an OPEN grant behind. Input-origin and response-delivery integration both require real-host proof; unaffiliated output cannot be policed by unseen service state. Entry selection uses versioned Canon owner signals, not conversation or a supposed new natural-language ranking algorithm.

## Acceptance Matrix

All scenarios below are **specified, not executed in A**. Later tests must assert durable state and no-write evidence as well as bounded errors; successful reference text alone is insufficient. Fixtures use generic identities outside the repository, not actual campaign records. Every gameplay scenario assumes authenticated trusted input ingress unless its purpose is to test absence of it.

| ID | Deterministic setup / attempted action | Required result and evidence | Package |
| --- | --- | --- | --- |
| S01 | No binding; one authorized resumable campaign; policy permits resume | Bind that campaign, verify baseline/profile, no typed ID or fabricated action | B/G |
| S02 | No binding; two resumable campaigns | Safe meaningful choices, no selection/mutation until real user choice | B/G |
| S03 | Transport/model loses context; durable scope/binding exists | Recover exact binding/input/receipt/pending state without story search | B/F/G |
| S04 | Bound campaign deleted, offline or inaccessible | Suspend/deny safely; never substitute another campaign or disclose inaccessible details | B/F/G |
| S05 | Binding absent; two campaigns; one resembles conversation | Reject semantic selection; choices returned; no gameplay write | B/G |
| S06 | Explicit authorized switch after prior work is safely closed | Verified successor generation, old history retained, old writes/choices not redirected | B/E/G |
| S07 | Real new gameplay submission; attempt resolution | Mandatory entry first, one bound input with verified entry/read evidence | D/G |
| S08 | Full commit or patch has only an arbitrary SourceInteractionId | Deny at persistence boundary; unchanged Canon and version | E/G |
| S09 | Generic rule endpoint called with gameplay.resolve | Packet is not entry; no OPEN/mutation authority | D/E/G |
| S10 | Entry with selected authorized active compiled artifact | Actual FR-026 reference semantics, Kernel/Procedure automatic, complete admitted specialists | D/G |
| S11 | Tool response lost and entry/read repeated for same submission | Same Player Interaction; no additional user turn/effects | C/G |
| S12 | Save/retry repeated under same input/transaction | Same frozen transaction and receipt, no duplicate costs/history/version | E/G |
| S13 | Assistant continuation/tool callback claims new input | No trusted new submission; no new interaction grant | C/G |
| S14 | Second genuine gameplay input after safe completion | Distinct Player Interaction on same valid binding, fresh canonical reads | C/D/G |
| S15 | GM presents alternatives A/B | Durable decision, AWAITING_PLAYER_INPUT/yield, neither alternative executed | C/E/G |
| S16 | No answer; GM tries choosing A | Denied choice/mutation/narration; pending decision unchanged | C/E/G |
| S17 | GM searches conversation and finds its own A proposal before reply | Read-only evidence cannot promote proposal; no Canon/Knowledge/Secrets change or successful selected-outcome narration | C/E/F/G |
| S18 | Actual new user submission chooses A in that decision | New linked interaction, canonical freshness checks, resolve only authorized A | C/E/G |
| S19 | User says A/B are both false; supplies correction or no replacement | Authorized certain correction if provided; otherwise investigate/clarify and yield, never invent C | C/E/G |
| S20 | Nonempty affected result persists and validates at configured authority | Matched receipt/result version authorizes completed narration and seals interaction | E/F/G |
| S21 | Persistence fails or validation/readback is incomplete | BLOCKED, pending/failure truthful, no completed consequences or next dependent mutation | E/F/G |
| S22 | Commit succeeded but acknowledgment vanished | Lookup/retry same stable identity, recover receipt, no replay or replacement save | E/F/G |
| S23 | Canonical version changed since resolution baseline | Stale rejection, reread/conflict handling, no blind delta rebase | D/E/G |
| S24 | Service/model restarts with valid durable session scope | Same campaign/binding generation recovered; readiness/access reverified | B/G |
| S25 | Restart while pending player decision | Same yielded question recovered, no inferred answer | C/F/G |
| S26 | Restart with no new user input | Status/recovery only; no new input grant or autonomous continuation | C/F/G |
| S27 | Two concurrent deliveries of same trusted submission | Exactly one interaction; conflicting content under same ID fails | C/G |
| S28 | Concurrent new input while prior one OPEN/PERSISTING | No competing forward grant; orderly completion/cancellation required | C/E/G |
| S29 | Commit races with yield/revoke/switch | Durable admission/order prevents post-yield new mutation; already committed outcome retained, unknown outcome reconciled | C/E/G |
| S30 | User asks status after yield | Authorized read-only receipt/diagnostics/verification; no time, simulation or new action | F/G |
| S31 | Start new game versus resume-plus-explicit-action | Existing approved setup/validated initial save before binding; unambiguous same-submission action enters once, no mandatory redundant prompt | B/D/G |
| S32 | Model invents submission origin or uses another principal's handle | Deny before existence/content disclosure; no input authority | C/F/G |
| S33 | Old interaction token, wrong campaign, patch exceeds owner/action scope | Deny before storage effects; token possession alone insufficient | C/E/G |
| S34 | Switch during unresolved receipt/open entry/pending choice | Reject until explicit cancellation/abandonment plus known persistence outcome; choices do not migrate | B/E/G |
| S35 | Authorized adoption/profile activation changes baseline | Refresh/suspend binding until verified profile/bootstrap; no auto-adoption orchestration | B/D/G |
| S36 | Two actual submissions have identical text; unrelated reply to pending question | First pair yields distinct IDs; unrelated reply never implies assent or silently discards choice | C/E/G |
| S37 | Required Canon read fails or material mechanic lacks admitted complete rules | Remain pending/block, retrieve/recheck same input or honest source recovery; never use memory/guess, no changed algorithm | D/G |
| S38 | Bootstrap hash changed; generic packet exists; required rule budget overflows | Readiness/entry fails honestly, UserConfirmed not Verified; no omitted Procedure or widened budget | D/G |
| S39 | Response delivery fails after completed commit; bounded world work considered | Redeliver same verified result read-only; same-input simulation only before yield, no fabricated background/player turn | E/F/G |
| S40 | Session scope lost or revoked, labels collide, trace requested | Authorized scope/explicit meaningful choice required; bounded visibility-safe trace, no hidden reasoning/raw conversation | B/F/G |

S05 and S17 are first-class motivating regressions, not optional examples. S10/S37/S38 must run against actual existing retrieval/read gates, not a mocked success boolean. G must replay the failures in a real host; no unsupported host result can be labeled passed.

## Trace, Errors, and Boundaries

The contract's [GM Turn Trace](../../docs/ai/CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md#gm-turn-trace) carries stable operational evidence, not chain-of-thought. Its [semantic failure taxonomy](../../docs/ai/CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md#semantic-failure-taxonomy) covers missing/ambiguous/inferred binding, unverified input, omitted entry, closed/choice-pending/unauthorized/wrong/stale mutation, missing rules/Canon, unresolved persistence, early narration and post-yield continuation. Registry strings/endpoints are deferred to implementation.

Classification: **A** reserved MCP name and inspected .NET/SQL mappings; **B** durable routing/input/entry/mutation/recovery contract; **C** player agency, noncanonical conversation/proposals, complete authority, validated persistence before successful narration. No .NET/SQL accident becomes a universal rule.

FR-028 retains bounded active Canon closure/missing-state improvements; FR-029 history separation; FR-030 compact receipt/lost-ack endpoints; FR-031 whole-runtime economics. Adoption remains administrative; FR-026 semantics, corpus/manifest/compiled identities and budgets are untouched. The new contract is not added to the runtime Rule Source manifest in A. Later entry integration must deliberately deliver required authority, not assume this documentation is already compiled.

## Validation and Remaining Limits

Actual validation completed with `pwsh -NoProfile -File tools/test_fr027_gameplay_entry_contract.ps1` and `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate_repository.ps1`. The A harness passed all 32 structural assertions. All nine existing harnesses passed unchanged semantic checks: FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 81, control-plane 35, FR-022 32, release-neutral 17; **365 assertions total including A**. Only the FR-022 current-selection assertion and repository status predicates changed to recognize selected/in-progress FR-027; no artifact rule/test was weakened.

The repository validator passed 328 Markdown documents, 7,736 relative links, 244 anchors, 197 indexed canonical documents, 15 family indexes, 43 templates, five roles, 1,281 terminology checks, 208 roadmap tasks and 36 Future Revision entries. Blocking questions, orphaned Markdown documents and forbidden campaign-data directories: zero. It also built two temporary distribution snapshots, compared their hashes for determinism, checked content boundaries and removed them; no release/review archive was published. Final exact staged validation confirms these counts before commit.

Working and staged whitespace checks use `git diff --check` / `git diff --cached --check` and pass. Complete diff scope review verifies documentation/governance and structural validation only, with no C#, SQL migration, runtime Rule Source manifest/prose, fixture, VERSION, release metadata or tag change. No full executable/SQL suite was run in A: those tests would not prove the design-only gates. No credentials, private locators or campaign data were introduced.

No unresolved architectural blocker prevents A: trusted ingress is a required capability with fail-closed behavior, not a claim that current MCP hosts can attest it. B/C must select/test its minimal mapping; G must prove the actual host. No live gameplay, binding durability, mutation rejection or human-origin assurance was executed in A. No downstream package, new migration, release, archive publication, version/tag change or historical campaign operation edit is part of this checkpoint.
