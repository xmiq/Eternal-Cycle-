# Campaign Binding and Mandatory Gameplay Entry

## Purpose and Status

This contract defines authoritative campaign routing and player-input authorization. It sequences existing owners; it grants no new mechanic, player choice, campaign fact, or persistence authority.

**Contract accepted in FR-027A; durable binding implemented in FR-027B only.** The [execution plan](../../design/FR_027_EXECUTION_PLAN.md) separates binding from C-G input, entry, mutation and live acceptance. Existing tools must not be described as enforcing gameplay authorization merely because they accept an Interaction ID. The reference gameplay-entry operation below remains reserved, not an available MCP tool.

## Document Control

- **Owner:** Campaign Session Binding, Player Interaction authorization, mandatory entry, pending-decision control, narration/yield gates, and operational trace semantics
- **Primary authorities:** [AI Runtime Model](AI_RUNTIME_MODEL.md), [Persistence Authority](../persistence/PERSISTENCE_AUTHORITY.md), [GM Responsibilities](../gm/GM_RESPONSIBILITIES.md), and [GM Runtime Procedure](../rules/GM_RUNTIME_PROCEDURE.md)
- **Dependencies:** [Managed Data Service](../persistence/MANAGED_DATA_SERVICE.md), [Context Assembly](CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md), [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md), [Canonical Data Ownership](../persistence/CANONICAL_DATA_OWNERSHIP.md), [FR-026 retrieval](../rules/COMPILED_RULE_STORE_RETRIEVAL.md), and validated Campaign Configuration/Save Index
- **Extensions:** authorized Managed interfaces and host integrations; Direct runtimes enforce equivalent input, routing, read, persistence, and agency boundaries without pretending to have a Managed service
- **Consumers:** GM runtimes, service implementers, host adapters, persistence coordinators, recovery, and authorized diagnostics
- **Repository boundary:** logical contracts and blank scenarios only; no actual binding, participant, campaign input, secret, or deployment locator

## Core Invariants

Every actual newly submitted player gameplay input creates exactly one authorized **Player Interaction**. Before resolution it binds to exactly one authoritative Campaign and passes mandatory gameplay entry. Gameplay mutations require authorization derived from that open interaction. Missing binding, entry, required authority, or player choice is a stop boundary, not permission to infer it.

Conversation is supplementary presentation/evidence, never campaign selection or canonical state authority. Protagonist/NPC names, locations, recent events, narrative resemblance, semantic similarity, a "most likely campaign," and model memory cannot resolve Campaign identity. Conversation search cannot choose among campaigns. Read-only discovery of safe campaign descriptions does not authorize selection.

Presenting alternatives creates a pending player decision; it never grants permission to select any alternative. GM alternatives, hypotheses, suggestions, proposed corrections, and possible interpretations are not automatically Canon, Character Knowledge, GM Secrets, or player decisions. Retrieving the GM's prior proposal cannot promote it.

Only actual new player gameplay input creates a Player Interaction. Reasoning, hidden continuation, retrieval, reads, tool calls/results, retries, persistence, validation, receipt lookup, narration, transport repair, and world simulation resolving the current input stay inside it. No new player input means no new gameplay interaction authorization.

## Identities and Authority

| Identity | Question answered | Owner and lifetime |
| --- | --- | --- |
| Campaign ID | Which external continuity exists? | Save Index/Campaign Canon; not a transcript or display name |
| Campaign Session Binding ID | Which campaign does this authorized logical gameplay session use? | Durable service control plane; outlives transport and many inputs |
| Player Interaction ID | Which actual submitted gameplay input authorizes this work? | Durable control plane; one immutable submission, one binding generation |
| Gameplay Interaction | Which bounded adjudicated unit can be persisted? | Existing Save Update Protocol; not redefined as a chat message |
| Save Transaction ID | Which bounded owner-routed update is staged/validated/activated? | Existing persistence protocol; stable across retry |
| Durable Managed Operation ID | Which service-owned administrative work is executing? | Managed Operations; never player-input authority |

Player Interaction is an authorization envelope, not a replacement for the existing Gameplay Interaction or Session ID. A scene may span multiple real inputs; each needs its own entry. A single declared input may require multiple bounded resolution/save boundaries under the existing protocol, all traceable to the same Player Interaction, with distinct stable Transaction IDs per bounded update. This does not authorize further player decisions or simulation after yield. An Interaction ID supplied in an existing transaction is provenance, not proof that entry occurred.

### Trusted Input Boundary

The host/integration supplies evidence of an actual user submission, a stable submission identity scoped to the authorized logical session, and its immutable input or bounded protected reference. The model cannot mint new authority by changing a tool argument. Repeated delivery of the same submission reuses one Player Interaction; conflicting content under the same submission identity fails. Two separate actual submissions with identical text are distinct inputs, not text-hash deduplication.

Authenticate the principal, session scope, and submission origin outside gameplay prose, following the [MCP interface security boundary](../persistence/MCP_PERSISTENCE_MODE.md#security). A host may attest the user-event origin or an authorized human-facing integration may register it. Arbitrary model assertions such as "the user chose A" are insufficient. Where a deployment cannot supply trustworthy input-origin evidence, it cannot claim autonomous entry enforcement: it must obtain a real user-confirmed submission through an authorized channel or stop. A cannot establish that any current host supports this; B/C/G must test it. No magic phrase, model name, message broker, or new general session framework is prescribed.

Bindings are scoped to the authenticated authority plus a stable logical gameplay-session context. That context is not a PID, connection handle, tool request, model conversation memory, or story fingerprint. Multiple sessions can exist; losing the trusted scope must not pick the most recently used binding. Recover it through trusted configuration/authorized session selection, or ask for meaningful campaign selection. Any opaque binding handle is a locator, never a bearer grant by itself.

## Campaign Session Binding

### Minimum Logical Record

| Field group | Required semantics |
| --- | --- |
| Binding identity and generation | Stable ID with immutable generation; never retarget an existing generation to another Campaign |
| Authorized session scope | Principal/access context and stable logical session; enough to prevent cross-user/session reuse, not private conversation contents |
| Campaign reference | One exact Campaign ID and its configured persistence authority/namespace reference |
| Verified baseline | Last verified active Campaign Version/Save Point; entry captures its expected baseline separately |
| Rules/profile reference | Adopted Repository/rules profile and selected release/artifact identity where applicable, derived from canonical configuration and authorized activation; display RC numbering is not immutable identity |
| State | ACTIVE, SUSPENDED, or CLOSED; unresolved discovery is not a valid binding |
| Provenance | Creation/selection authority, reason (explicit selection, policy-authorized unique resume, or durable recovery), predecessor/successor switch references |
| Readiness evidence | Verification identity/revisions, relevant bootstrap hash/state, outstanding setup/recovery/decision reference |

Store references and evidence, not copied mutable protagonist, scene, inventory, or world facts. The Save Index and specialist owners remain authoritative. A last-verified time is optional diagnostic freshness; version/revision checks, not wall-clock age, establish validity. Access/readiness is rechecked at entry even for ACTIVE bindings.

### Resolution and Lifecycle

1. Recover a valid binding within the trusted session scope before enumerating campaigns. Verify access, active Save Index, adopted profile, persistence completion, and readiness.
2. If no binding exists, discover only authorized resumable campaigns through existing FR-020 policy. Zero candidates uses existing setup/new-game flow; do not create a blank replacement. One candidate may auto-bind where policy permits, including any required consent. Multiple candidates return meaningful safe names/descriptions and stable choice handles internally; no similarity score or internal ID as primary player UX.
3. Explicit user selection resolves a choice handle against the current authorized discovery result. Duplicate labels require meaningful safe disambiguation, not guessing. A choice learned from a real submission may establish a binding and, if unambiguous gameplay intent is included, enter that same submission once without an extra artificial turn.
4. Stale verified versions invalidate dependent packets and require canonical reread. Before resolution, reconcile safe freshness and recapture the baseline; after staged resolution/write, stale baseline blocks mutation until conflict resolution. Never silently rebase a resolved delta. A profile/authority change suspends use until its authorized migration/activation and readiness are verified.
5. A deleted, unavailable, inaccessible, or contradictory selected campaign suspends the binding and fails honestly without substituting another candidate. Access revocation denies disclosure as well as writes.
6. Explicit switching is a separately authorized control operation. Reject a switch while entry/resolution/persistence or unresolved recovery is open. First explicitly cancel/close incompatible work, reconcile any unknown commit, and explicitly abandon any pending decision; then create a successor binding generation with verified target readiness. Do not redirect writes or move choices to another campaign. Administrative switch authority does not grant player action/consent authority.

Cancellation preserves validated committed effects; it is not rollback or permission to erase history. A failed switch leaves the existing binding/history intact or explicitly suspended, never partially retargeted. Concurrent selection/switch must use expected binding generation so only one successor wins coherently. Campaign-wide expected-version checks still protect concurrent distinct sessions.

### Setup, Resume, and Adoption

`Start new game` follows [FR-020 readiness/setup](../persistence/RUNNING_ETERNAL_CYCLE.md), bounded approval, canonical campaign creation, initial validated save, and only then a binding. Setup activity is not fictitious gameplay and does not choose a player-character action. A setup-only request does not open resolution authority. If a real submission also supplies unambiguous initial gameplay intent, entry can process it after successful setup without demanding another message.

`Continue the game` recovers/reuses a binding or follows zero/one/multiple discovery above. A pure resume can return current verified presentation read-only; it does not invent an action. Resume plus an explicit action can use mandatory entry for that same actual submission once bound. Multiple-campaign ambiguity requires a real choice first.

Validated adoption remains a separate [host/specialist workflow](CONTEXT_ASSEMBLY_ADOPTION.md). Its eventual atomic activation refreshes or establishes the binding's adopted profile/version; later gameplay uses entry. This contract neither orchestrates adoption nor places it inside `gameplay.resolve`.

## Binding Implementation Checkpoint

The [B acceptance audit](../../design/audits/FR_027B_DURABLE_CAMPAIGN_BINDING_ACCEPTANCE.md) records the reference's opt-in durable binding, authenticated deployment/session scope, exact authorized choices, version/profile/readiness verification, suspension and successor history. Binding validity is not Player Interaction authorization. The reference's configured session adapter is not proof of real-host human submission origin.

Switch and changed-profile reconfirmation require a transaction-scoped interaction/decision safety check plus known persistence disposition. Until C supplies that check, the deployed reference fails closed; a deterministic test substitute proves successor atomicity without pretending real interaction storage exists. Missing safety evidence is not a switch permission. No campaign write or pending decision is transferred.

## Player Interaction State Machine

The control plane durably records submission identity/origin, binding generation/Campaign ID, starting version/profile, current state and gate evidence, pending-decision relation, authorized owner/action scope, and transaction/result references. Sensitive input may be a protected reference rather than diagnostic prose. No raw-input trace dump is required.

RECEIVED may reserve the submission identity while binding/selection is unresolved, but has no forward gameplay authority. Entry must attach exactly one verified binding generation before OPEN; an unresolved intake is never treated as an authorized campaign interaction. Once attached, that input cannot be rebound to a different campaign.

**YIELDED is a disposition, not an additional mutable state.** COMPLETED, AWAITING_PLAYER_INPUT, BLOCKED, and CANCELLED all revoke ordinary forward resolution/mutation; a yielded response cannot reopen them. This avoids two disagreeing "waiting/yield" flags. BLOCKED recovery is limited to the original input, authorized scope, and verified unmet gate.

| State | Permitted work | Transition actor/evidence | Forbidden |
| --- | --- | --- | --- |
| RECEIVED | Deduplicate trusted submission, authorize identity; no gameplay work | Trusted ingress registers immutable submission; service -> ENTRY_PENDING | Model-minted submission, resolution/mutation |
| ENTRY_PENDING | Mandatory entry, binding/readiness/rule checks, exact required follow-up reads | Service -> OPEN only after all entry/read gates; -> BLOCKED on unmet gate; -> AWAITING_PLAYER_INPUT for material intent clarification | Resolution/mutation on a provisional entry result |
| OPEN | Declared-intent resolution, relevant rules/reads, bounded same-input simulation, explicit Affected Set | Authorized GM proposes result; service -> PERSISTING for nonempty affected unit; -> COMPLETED for verified empty result; -> AWAITING_PLAYER_INPUT for required choice; -> BLOCKED on gap | Choosing unsubmitted intent, bypassing read/coverage gates |
| PERSISTING | Frozen bounded delta, existing transaction validation/readback/activation, same-ID retry/reconciliation | Persistence evidence -> OPEN only if further already-declared work genuinely requires the validated state; otherwise -> COMPLETED or AWAITING_PLAYER_INPUT; failure/unknown -> BLOCKED | Replay, changed payload under same transaction, new choices, ordinary successful narration before validation |
| AWAITING_PLAYER_INPUT | Read-only status/evidence/verification; pending decision remains durable | New trusted submission relates to decision and starts a new Player Interaction; original remains yielded, with successor/disposition reference; explicit abandonment -> CANCELLED | Resolving an alternative, advancing simulation, gameplay mutation, auto-reopening old input |
| COMPLETED | Read-only receipt/trace lookup and redelivery of validated result | Service seals after complete input resolution, all required receipts or explicit empty Affected Set, and narration authorization | Further forward resolution/mutation; delivery failure is not a new input |
| BLOCKED | Safe status, evidence recovery; same transaction's bounded retry only where originally authorized | Service revalidates and resumes original unmet gate at ENTRY_PENDING/OPEN/PERSISTING; explicit valid cancellation -> CANCELLED | Broadened intent, successful dependent narration, opening competing state-changing input while persistence is unresolved |
| CANCELLED | Read-only history/receipts; reconcile already-started commit if necessary | Authorized user/operator cancellation, cancellation evidence, and known persistence disposition | New effects, erasing committed history, concealing unknown transaction outcome |

An entry-only clarification may yield with `Affected Set = empty`; no fictitious save is required. Before presenting a player decision, all independently established effects of the current resolved unit must be persisted/validated or reported as operationally blocked. The pending-decision control record is not fictional Canon and contains no executed alternative.

Only one forward-authorized Player Interaction exists per binding generation. Concurrent same-submission entry calls converge; a second actual gameplay submission cannot silently supersede open work. Report the boundary and require orderly completion/cancellation. Different campaign sessions still use existing optimistic concurrency. Cancellation of a transport is not cancellation of player intent; reconnect never increments or invents input identity.

## Mandatory Entry Contract

### Operation and Inputs

Managed services expose the semantic **Begin Gameplay Interaction** operation; the reserved reference name is `ec_begin_gameplay_interaction`. It is the first authoritative *gameplay* operation for each actual new input. Read-only session recovery/discovery and separately authorized setup may precede it but cannot resolve that input. Generic `ec_get_rule_context` / `ec_get_compiled_rule_context` calls, even with `gameplay.resolve`, are retrieval, not entry authorization.

Minimum inputs are trusted resolvable session/binding context, trusted actual submission identity and immutable input/reference, and an optional pending-decision reference. Bounded intent/topics/mechanic hints may aid retrieval but cannot broaden submitted intent or certify rule coverage. The service allocates or resolves the Player Interaction ID idempotently. Ordinary players supply no Campaign ID, Ruleset, release, SHA, database details, or internal selector. Backend routing and authorization are trusted control-plane concerns.

### Entry Sequence and Result

1. Authenticate session/submission origin and deduplicate. Recover exactly one binding or return safe setup/selection/failure; do not resolve gameplay while ambiguous.
2. Verify Campaign Configuration/Save Index, active version/profile, selected canonical authority, outstanding transaction/continuity state, access, and Host Bootstrap readiness. Capture immutable binding generation and expected baseline.
3. Check pending decision: only a genuinely related new user input can provide its choice/clarification. An unrelated action does not silently abandon it; ask whether to resolve/abandon it. Status requests stay read-only.
4. Fix operation `gameplay.resolve`, mode/world/modules and release/artifact scope from authoritative configuration. Derive bounded retrieval signals from the actual input, canonical scene/state and active mechanics. Record the origin of signals; model hints remain hypotheses requiring applicable owner coverage. No new semantic classifier, fuzzy ranking, or source-guessing algorithm is specified.
5. Invoke existing FR-026 A->E preparation, matching, applicability, ranking, complete dependencies and whole-closure budgeting against that one authorized active artifact. Required Kernel and GM Runtime Procedure are automatic; no caller topic is needed. Retain the established legacy route only where canonical configuration explicitly selects it, with equivalent mandatory safety, not as fallback for failed selected compiled retrieval.
6. Establish the minimum Canon baseline/read obligations below. Supply already verified required records if available and bounded, otherwise explicit exact follow-up record addresses/revisions and missing-state reasons. ENTRY_PENDING persists until required reads/coverage are evidenced, never an OPEN token with an unresolved required-read flag.
7. Record entry/read/coverage evidence and issue interaction-bound resolution/mutation scope only when gates pass. Return compact executable Rule Packet plus minimum authorized Canon context/read obligations, entry disposition, identity/version references, scope, pending state, and trace reference. Do not return entire artifact, campaign, or service-side ranking tables.

Repeated entry for the same submission finishes/reports the same gates; it creates no new input. Follow-up reads are authorized, versioned, owner-scoped reads, not a bypass that accepts a model's bare "read complete" claim. Service evidence proves supplied records/revisions and required rule IDs; the GM remains responsible for actual semantic application. A generic context receipt alone grants no mutation authorization.

Scene/active-mechanic selection signals require verified owner reads at the captured baseline, performed within entry before using those signals. If they are unavailable, expose the required read rather than borrowing conversation or stale summary state. FR-026 consumes explicit reviewed query entries/selectors, not a raw sentence treated as though the existing engine parsed intent; formulated signals retain their input/owner/hint provenance and cannot certify coverage alone.

### Rule Coverage and Host Readiness

The Host Bootstrap is compact host-level instruction; the GM Runtime Procedure orders each input; specialist packets provide applicable owners. Entry reuses current bootstrap presentation/UserConfirmed/Verified distinctions and expected hash, never equates confirmation with technical verification or duplicates the complete Procedure into a system prompt.

Selection signals are not proof of coverage. When a newly material mechanic, selector mismatch, excluded required authority, missing specialist, incomplete dependency, or required-overflow becomes apparent, stop the affected resolution, retrieve applicable complete authority under the same input, verify baseline/coverage, then resume. Never proceed because a ranking engine returned some packet. Optional exclusion is not permission to execute an omitted owner; unknown mechanics cannot be guessed. No ranking, vocabulary, dependency, or packet ceiling changes are authorized by FR-027.

## Minimum Canon Read Boundary

Entry establishes exact Campaign identity, active version/Save Point, configuration/profile, Current Session/open recovery and pending decision, relevant Controller/player/protagonist and Incarnation identities, current time/scene/placement, and authoritative owner records required by declared intent. For each material read retain owner address, record identity/revision, validity/visibility, and missing/uncertain indication; no conversation summary can satisfy it.

Identity references do not establish their current facts. Resolve required owner records using the existing [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md#required-read-discipline) and mandatory Read Set gate. Not retrieved is not nonexistent; Unknown, Not Yet Verified, Estimated, Disputed, and Requires Source Recovery remain distinct established statuses. Material missing authority blocks or seeks clarification/source recovery; only existing owning rules may permit bounded uncertainty, not the entry service's guess.

FR-027 establishes authoritative identity and mandatory required-read evidence. **FR-028 owns scalable bounded active-Canon closure**, structural expansion and improved missing-state representation. A does not implement automatic closure, scan the whole campaign, or promise that every relevant record can be predicted at initial entry. New material dependencies extend required reads before affected resolution.

## Mutation, Persistence, Narration, and Yield

### Mutation Authorization

All gameplay full commits, constrained patches, permitted creates/deletes, and future mutation surfaces must verify the same service-owned interaction authorization. A handle is not authority by itself. Verify authenticated scope, open binding generation and Campaign, Player Interaction state, entry/read/rule gates, expected active baseline/record revisions, permitted owner/action scope, player decision state, and stable transaction/payload identity. Do so at the durable write/activation boundary, not only in a tool decorator, preventing concurrent yield/switch/revoke bypass.

Reject absent/closed/stale interaction, wrong Campaign, pending choice, yielded state, exceeded scope, stale baseline, or unmet rule/read gate before changing Canon. No endpoint accepts a caller-provided `OPEN = true`. Creates/deletes are only those permitted by existing owners; storage deletion remains exceptional, not ordinary fictional destruction. Administrative setup/migration/adoption retain separately authorized existing boundaries and cannot masquerade as gameplay to bypass the input gate.

PERSISTING and limited BLOCKED recovery may apply only the originally authorized frozen transaction. Durable admission prevents new proposals after yield; work already atomically committed cannot be revoked retrospectively. Yield waits for a known persistence disposition, not an untracked racing commit. Service-side control records and diagnostic evidence are not gameplay mutations or new Canon owners.

### Pending Decisions and Corrections

A pending decision records a stable identity, originating Player Interaction/binding generation, safe question/options or protected reference, unresolved claim/control scope, source baseline, visibility, and status/successor relation. Options need not be exhaustive and are proposals. Presenting them seals forward authority for that input as AWAITING_PLAYER_INPUT. No assistant continuation/search/retry may choose or persist an option.

The compliant host must deliver questions/results through a service-admitted response disposition: record the pending decision and revoke forward authority before presenting alternatives, or seal validated completion before delivering a completed result. A prose-only "I am waiting" cannot leave the interaction OPEN. This requires an enforced delivery/yield boundary in the integration, not another optional instruction to call a tool. A host that bypasses it cannot claim FR-027 conformance; the service cannot police unaffiliated free-text output it never receives.

A new actual submission explicitly or unambiguously answers the identified decision, then enters a new interaction linked to it. Ambiguous replies require clarification; unrelated replies do not infer assent. Revalidate changed baseline/options before applying a response. User may reject all offered alternatives or provide a correction; do not force a closed menu or invent option C. An administrative operator cannot substitute their own choice for a player's decision merely by having storage access.

Accepting that real answer records the decision's answered/superseded disposition and successor interaction atomically; it does not reopen the original yielded input or establish the proposed fictional outcome. Resolution and validated persistence still belong to the successor's normal gates. A repeated delivery of the same answer reuses that successor; it cannot answer the decision again.

Certain correction supplies an explicit replacement and scope; use existing owner correction/validation, without silently authorizing an incompatible retcon or changing unrelated facts. Investigative correction supplies a defect but no replacement; inspect Canon/evidence, preserve Unknown, and yield if alternatives remain. Player clarification required creates the pending decision above. **FR-029 owns correction-history storage**, not this input/control classification.

### Completion and Recovery

`resolution -> persistence -> validation/read-back -> narration authorization -> completion/yield` is mandatory for durable consequences. Completed narration is authorized only for the established result backed by all required configured-authority receipts and expected resulting version, or verified `Affected Set = empty`. Pending, failed, or unknown persistence forbids ordinary successful completion. Read-only current-state presentation and truthful uncertainty/failure are not false narration of a saved consequence.

Manual `save`, `save status`, and `retry save` reuse the existing interaction/transaction; they do not grant new action scope or replay effects. Lost acknowledgment follows FR-017: inspect the same stable transaction/idempotency identity, recover validated completion or resume only its incomplete stage. Unknown completion remains blocked; never submit a replacement transaction to escape it. FR-030 owns new compact result/lookup implementations.

COMPLETED seals forward authorization before response delivery. Delivery failure can redeliver/reconcile the same verified outcome without a new action or save. Pending-decision delivery can likewise be recovered as a question, not a decision. Read-only receipt/status reconciliation, diagnostics, canonical verification and support evidence are allowed after yield under existing access limits; resolution, player selection, time/world advancement and new gameplay mutations are not.

World autonomy remains canonical: same-input causal simulation uses that input's authorized scope and save boundaries. A separately scheduled non-player mechanism must have its own existing explicit authority, identity, bounds and persistence; it cannot reuse a yielded player interaction. This contract introduces no scheduler or new background-simulation capability.

## Restart and Reconnect

Authenticated recovery returns the exact bound Campaign, current canonical version/profile, binding state/generation, open interaction and entry gates, pending decision, transaction outcome, and whether a new gameplay input may enter. This uses durable control records and canonical receipts, not conversation reconstruction.

Transport/client/model/service restart does not create another input, reset a pending choice, change Campaign, invalidate a validated receipt, or silently reauthorize cancelled/completed work. Resume original gated work only after canonical freshness, ownership and access checks. An unknown persistence outcome blocks new dependent gameplay until reconciled. If trusted session scope is unavailable, recover through authorized selection rather than story recognition. Existing independently executing [Managed Operations](../persistence/MANAGED_OPERATIONS.md) retain their own recovery/leases; Player Interaction does not replace their executor model.

## GM Turn Trace

This compact operational trace is **not chain-of-thought**. Retain interaction/submission reference, binding generation/Campaign, starting version/profile, entry operation/status, selected rule identities with bounded inclusion reasons and dependency completeness, required/read owner revisions and gaps, any supplementary conversation-search purpose (evidence/continuity, never campaign selection/choice), resolution disposition, explicit Affected Set, persistence operation/Transaction ID/receipt status, resulting active version, narration authorization, final state/yield reason and recovery references.

Use stable references and bounded summaries; large detail stays in authorized diagnostics. Omit private reasoning, raw conversations/inputs, credentials, private locators, full Canon/Secrets and other users' data. GM versus player/admin audiences remain distinct. Unknown/not-observed trace fields remain explicit, not fabricated successful evidence. Future Admin reports consume this trace; A creates no report endpoint or new logging store.

## Semantic Failure Taxonomy

These are semantic categories, **not implemented error-registry strings**. Map later into existing safe error/lookup conventions with bounded authorized evidence and recovery guidance; deny before disclosing inaccessible campaign/artifact existence or contents.

| Category | Required safe outcome |
| --- | --- |
| Binding absent / selection ambiguous | Setup or meaningful authorized choice; no mutation |
| Semantic campaign selection attempted | Reject inferred selection; real explicit choice required |
| Submission origin unverified / identity conflict | No new interaction authority; obtain trustworthy input evidence |
| Entry omitted / read or rule gate incomplete | Deny resolution/mutation; perform original mandatory gates |
| Interaction closed / decision pending / continuation after yield | No new effect or choice; wait for actual input |
| Mutation unauthorized / wrong Campaign / exceeded scope | Deny before write/disclosure; do not reroute |
| Stale baseline / binding generation / profile | Canonical reconciliation, no blind replay/rebase |
| Required rules or Canon unavailable | Stop affected action; recover complete authority or honest uncertainty where permitted |
| Persistence pending / failed / unknown | No successful completion; same-identity reconciliation/retry |
| Narration before validated persistence | Reject successful-completion claim; retain verified authority |
| Switch blocked / access revoked / campaign unavailable | Preserve history and reject substitution; authorized recovery |

## Acceptance and Ownership Boundaries

The [FR-027A audit matrix](../../design/audits/FR_027A_CAMPAIGN_BINDING_GAMEPLAY_ENTRY_CONTRACT.md#acceptance-matrix) defines deterministic later service/host acceptance, including the campaign-loss and GM-alternatives regressions. Structural document validation in A does not count as these executable/live tests passing.

- **A - reference implementation:** reserved MCP tool name, eventual .NET types/SQL migrations/LocalDB and host-adapter mechanics. No such implementation is added by A.
- **B - Managed Service contract:** durable binding/input identity and gates, authorization, recovery, mandatory entry and diagnostic evidence, independent of interface/backend.
- **C - Eternal Cycle invariant:** real input owns deliberate choices; no semantic campaign guessing; conversation/proposals are not Canon; complete authority and validated persistence precede completed consequences.

FR-026 selection semantics remain unchanged. FR-028 owns full bounded Canon closure; FR-029 correction-history separation; FR-030 compact persistence/lost-ack APIs; FR-031 end-to-end economics; adoption remains its own administrative host/specialist workflow. No whole-runtime affordability, live-host compliance, new gameplay, release or downstream implementation is claimed here.
