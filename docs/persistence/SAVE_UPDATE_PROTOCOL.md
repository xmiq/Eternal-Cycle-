# Save Update Protocol

## Purpose

This document defines how an Eternal Cycle campaign converts each completed gameplay interaction into a bounded, source-preserving, validated update to external Campaign State.

The protocol preserves causality by recording what changed, why it changed, who owns the result, what remains pending, and which histories and information views must receive the event.

## Core Rule

During ordinary gameplay, [FR-011](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) invokes this protocol automatically after resolution. A player command is not required. Every interaction receives an explicit Affected Set determination; a state-changing turn remains open until the transaction succeeds, validates, and satisfies configured read-back requirements.

After every completed gameplay interaction:

1. determine the affected persistence sections;
2. construct the Session Delta;
3. update only the affected Authoritative Record Owners;
4. append the Session Log;
5. append Timeline Events;
6. append Campaign History when the interaction is materially durable;
7. record new Projects, Mysteries, discoveries, disproved theories, consequences, and Review Points;
8. validate the complete candidate state;
9. establish the next Save Point atomically.

Do not rewrite unrelated records.

## Save Update Is Not

A Save Update is not:

- a transcript dump;
- a whole-campaign rewrite;
- a Migration;
- a place to invent unrecorded values;
- a way to resolve an existing Continuity Conflict silently;
- a summary replacing specialist records;
- a universal post-scene reward;
- automatic world simulation beyond established causes;
- permission to update every possible consequence immediately;
- storage-specific file synchronization;
- campaign data stored in this repository.

## Gameplay Interaction

A **Gameplay Interaction** is one bounded unit of play whose material intent, adjudication, immediate outcome, costs, information effects, and currently resolvable consequences have been established enough to persist.

An interaction may be:

- one character action and response;
- a negotiated exchange;
- an investigation step;
- an encounter exchange;
- a completed scene;
- one World Engine Simulation Pass;
- a Project interval;
- a Time Skip segment;
- a Final Death or one stable Reincarnation stage;
- another bounded resolution with a clear before and after state.

A chat message, sentence, die roll, rules lookup, clarification, or draft is not automatically a completed interaction. One interaction may span several messages. One message may summarize several separately traceable interactions.

The GM identifies the smallest boundary at which further dependent adjudication would require the new state. Saving too early records unresolved drafts; saving too late permits play to depend on unpersisted continuity.

## Save Transaction

Every update uses one **Save Transaction** with a unique Transaction ID.

The transaction records:

- Transaction ID;
- Session ID and Interaction ID;
- parent Campaign Version and Save Point;
- active Repository and Persistence Model Versions;
- actor intent and ownership where relevant;
- adjudication and source events;
- preconditions and loaded Read Set;
- Affected Set;
- Session Delta;
- ordered Write Set;
- Timeline and Campaign History append decisions;
- Knowledge and Secret visibility effects;
- numerical-change traces;
- unresolved claims and Pending Consequences;
- validation plan and result;
- candidate Campaign Version;
- activation status;
- interruption, retry, or recovery state.

The same Transaction ID is never reused for a different interaction. A retry of the same transaction retains the same identity and must not duplicate events, costs, items, progression, relationships, or consequences.

## Preconditions

Before opening a Save Transaction, confirm:

- the current active Campaign Version and Save Point;
- no blocking Migration is active;
- no unresolved Continuity Freeze covers the proposed update;
- the interaction has a resolved or explicitly Pending status;
- all material Authoritative Record Owners are identifiable;
- the required Read Set was loaded;
- player-owned intent is established where necessary;
- specialist mechanics produced the claimed result;
- visibility and Secret boundaries are known;
- the storage implementation can preserve pending work or complete atomic activation.

If a precondition fails, preserve the interaction as Pending Session material and resolve the blocker. Do not create a false confirmed Save Point.

## Affected Set

The **Affected Set** is the smallest dependency-complete set of campaign claims and records that the interaction creates, changes, closes, supersedes, or materially references.

Every proposed mutable fact is routed through [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md) before entering the Write Set. Writes to Derived Views, caches, summaries, or non-owning modules are rejected; the owner changes once, after which affected views are invalidated or regenerated.

Build it in two passes.

### Direct Effects

Identify direct effects established by adjudication:

- actor and body state;
- Soul or Incarnation state;
- location, movement, access, or custody;
- item creation, transfer, use, damage, loss, or recovery;
- Skill, Development, Class, Profession, Monster Evolution, Soul Weapon, or Magic outcomes through their owners;
- Relationship Events and commitments;
- observations, discoveries, communication, beliefs, and secrecy;
- Project, Research, or Mystery changes;
- faction, settlement, ecology, population, resource, economy, disease, Dungeon, Gate, Stability, Age, or other world effects;
- elapsed time and Timeline Events;
- Provisional Rulings, corrections, or Review Points.

### Dependency Closure

Follow only material dependencies from the direct effects:

- current-state claims derived from the changed fact;
- inverse or custody references;
- actor, Soul, Incarnation, Species, Location, faction, item, and Project indexes;
- Relationship and Knowledge records affected by witnessed or communicated events;
- Research evidence and confidence affected by valid observations;
- Pending Consequences now created, changed, triggered, or closed;
- Timeline relations;
- Campaign History where durability warrants it;
- validation and Save Index references.

A possible future response is not automatically in the current Affected Set. Record a Pending Consequence or Review Point unless its cause, timing, and immediate effect are already established.

## Update Classification

For each affected claim, classify the operation:

| Operation | Meaning |
| --- | --- |
| **Create** | Establish a genuinely new stable record or identity |
| **Amend** | Change a current claim through a new valid event while preserving prior state and source |
| **Append** | Add an event, evidence item, meeting, observation, log entry, or history record |
| **Close** | End an active condition, Project, commitment, incarnation, effect, or process through a valid cause |
| **Supersede** | Replace which current claim governs while retaining the former claim and link |
| **Archive** | Move inactive material out of current loading without deleting identity or history |
| **Correct** | Apply an authorized source-preserving Correction Event |
| **Pending** | Preserve an established but unintegrated or unresolved change |
| **No Change** | Confirm the interaction caused no persistent change to this owner |

Delete is not an ordinary gameplay operation. Destruction, death, forgetting, consumption, closure, and loss are in-world events whose records and consequences remain. Storage deletion follows Persistence Level, privacy, archival, and migration rules.

## Session Delta

The **Session Delta** is the claim-bounded set of established changes since the parent Save Point.

For this interaction it records:

- source Interaction and Event IDs;
- created, amended, appended, closed, superseded, corrected, and Pending claims;
- direct and downstream causal classification;
- owners and typed references;
- prior and resulting states;
- numerical traces;
- truth-layer and visibility changes;
- Timeline coordinates;
- Pending Consequences and Review Points;
- validation dependencies;
- exclusions and reasons.

The Session Delta is not a prose recap. It must be sufficient to reproduce the candidate state from the parent Campaign Version without rereading the whole conversation.

## Save Update Procedure

### 1. Identify the Interaction Boundary

Confirm what intent, adjudication, outcome, costs, and immediate consequences are established. Separate unresolved future action into Pending claims or later interactions.

### 2. Open the Transaction

Assign Transaction and Interaction IDs. Record the parent Campaign Version, Save Point, Session, Repository Version, and relevant rules profile.

### 3. Recheck the Read Set

Confirm the parent state, current time and location, current Session Deltas, canonical owner, and all material actor, Relationship, Species, Location, Project, Research, Timeline, Knowledge, Inventory, Soul, and world dependencies.

If the parent changed since adjudication, stop and revalidate rather than overwriting concurrent work.

### 4. Capture the Adjudicated Result

Record:

- declared intent and actor authority;
- method and target;
- relevant conditions and information views;
- canonical owners consulted;
- capability, access, embodiment, resources, consent, opposition, time, and costs;
- resolution method;
- immediate result and failure state;
- evidence and uncertainty;
- immediate consequences;
- Provisional status where applicable.

This preserves provenance without duplicating specialist mechanics.

### 5. Build the Affected Set

Map direct effects to owners, then follow the material dependency closure. Explicitly list records that remain unchanged when omission might otherwise be mistaken for oversight.

### 6. Build the Session Delta

Represent each operation with before state, event, owner, after state or Pending state, truth layer, temporal scope, visibility, and references.

### 7. Stage Owner Updates

Apply the Write Set to an isolated candidate state or transaction boundary. Update each claim through its Authoritative Record Owner.

Do not activate partial owner updates. A body injury, item consumption, spell effect, witness Knowledge, and Timeline Event from one interaction must not split into contradictory active versions.

### 8. Append the Session Log

Every completed gameplay interaction appends a Session Log entry containing:

- Interaction and Transaction IDs;
- presentation order;
- player or autonomous-actor intent;
- adjudication summary;
- Provisional Rulings;
- resulting Session Delta;
- unresolved claims;
- validation and Save Point outcome.

The Session Log records play order, not necessarily Occurrence Time.

### 9. Append Timeline Events

Create or update Timeline Events for material in-world occurrences, processes, discoveries, corrections, and state transitions.

Preserve Occurrence, effective, record, discovery, narration, and integration times separately. A purely Meta clarification or formatting repair creates no in-world Timeline Event.

### 10. Append Campaign History When Appropriate

Append Campaign History when the interaction establishes durable continuity, including:

- meaningful choices and outcomes;
- material creation, loss, injury, death, recovery, or transformation;
- important meetings, commitments, betrayals, discoveries, and disclosures;
- Project or Research milestones;
- significant faction, settlement, ecological, economic, magical, Species, or world changes;
- Final Death, Life Reconciliation, Reincarnation, Age, Reset, or Gate events;
- corrections and authorized retcons;
- changes likely to explain future state or decisions.

Routine reversible state may remain in current owner records and Session Log when no durable historical account is needed. Omitting it from Campaign History does not erase its valid current state.

### 11. Update Special Persistence Families

Apply the explicit handling rules below for Projects, Mysteries, Research, Relationships, Knowledge, Secrets, Inventory, numerical state, Souls, and world change.

### 12. Validate the Candidate

Run the applicable [Persistence Validation](PERSISTENCE_VALIDATION.md) profile over the full candidate dependency closure.

### 13. Activate the Save Point

If validation passes:

1. assign the candidate Campaign Version;
2. mark the included Session Delta integrated;
3. update affected Pending claims to Confirmed where justified;
4. preserve unresolved Pending claims explicitly;
5. atomically update the Save Index to the new active version;
6. record Integration Time and validation result;
7. verify the active state can be loaded;
8. mark the transaction Committed.

If validation fails, the parent Campaign Version remains active and the transaction remains Failed, Blocked, or Recovery Pending.

## Special Persistence Families

### Projects

For each affected Project, record:

- new Project identity when one has genuinely begun;
- objective, participants, authority, methods, resources, dependencies, and location;
- actual work performed;
- milestone, interruption, failure, completion, abandonment, or continuation;
- outputs and consequences;
- next Review Point.

Time, stated intent, or one preparatory conversation alone does not complete a Project.

### Mysteries

For each affected Mystery, record:

- new bounded question;
- clues and source provenance;
- discovered, hidden, false, obsolete, or disproved leads;
- interested actors and stakes;
- Character Knowledge and Player Theory distinctions;
- resolution condition;
- closed, transformed, or still-open state.

A new Mystery does not authorize inventing a hidden answer. Unknown truth remains unknown.

### Research

Record separately:

- new Observation Records;
- hypotheses proposed or retired;
- experiments and deviations;
- Evidence Records;
- confidence changes by exact claim;
- competing, Obsolete, Forgotten, or Disproved theories;
- completed discoveries;
- In-World Confirmation and Confirmed Knowledge;
- publication, secrecy, and Character Knowledge;
- Rediscovery and Review Points.

A completed discovery enters Campaign Canon only through its factual owner. A disproved theory remains in Research history and does not erase consequences caused by belief in it.

### Relationships

Record:

- participants and Incarnation scope;
- latest meeting when one occurred;
- important encounter references;
- directional Relationship Dimensions changed or tested;
- trust scope and trajectory;
- communication;
- promises, betrayals, debts, gifts, family, organizational, and dependency changes;
- recognition and Knowledge;
- unresolved issues.

Proximity, repeated dialogue, gifts, danger, or elapsed time grants no automatic trust or affection. Record the participants' interpretations and actual causal event.

### Autonomous Registry

When an interaction involves an autonomous subject, determine whether Registry-owned identity, autonomy, Controller assignment, operational assignment, model or upgrade lineage, memory continuity, network membership, independence or personhood claim, or last-confirmed state changed. Route placement, condition, capabilities, Inventory, Relationships, Infrastructure, Projects, and history to their own authoritative domains.

Preserve unknown current state separately from the last Confirmed Report. Individual quantity remains one; bounded Group quantity changes require numerical traces, and promotion to individual identity updates the Group and new Individual in one Affected Set. Significant creation, restoration, reconstruction, fork, upgrade, Controller, and independence events append to Timeline or Campaign History as appropriate.

### Knowledge and Secrets

For every information effect, record:

- exact claim;
- observer or authorized audience;
- source and acquisition route;
- truth layer;
- confidence or belief where applicable;
- time learned;
- disclosure restrictions;
- resulting action only when separately established.

Update GM Secrets only through protected views. Never copy hidden truth into player-visible indexes, summaries, inverse links, or logs.

### Inventory and Custody

Record item identity, quantity where established, condition, location, custody, ownership claim, access, provenance, transfer, use, damage, consumption, loss, and containers.

Do not infer ownership from custody or reduce a Weapon Soul to Inventory. Every numerical quantity change requires its event and source.

### Numerical State

Every material number uses a Numerical Change Trace. The update records prior value, pending adjustments, event, owner, mechanic, inputs, costs, change, resulting value, dependents, and validation.

No silent progression adjustment, balancing correction, average, rounding invention, or default zero is permitted.

### Souls and Reincarnation

Final Death and Reincarnation usually require several linked owner updates:

- close the current body's active state;
- preserve world consequences and ordinary items;
- perform Life Reconciliation;
- update eligible Soul records through their owners;
- create Echo, Title, Resonance, Depth, Weapon, Strain, Wound, or other changes only when established;
- record Interlife and world passage;
- open the new Incarnation and Personal Chronology only at valid Embodiment.

One transaction may cover one stable transition stage. Do not leave Final Death half-activated across contradictory records.

### World Change

The World Engine establishes autonomous change. The Save Update records only established results and their causal dependencies across affected populations, resources, ecology, economy, factions, conflict, disease, advancement, Dungeons, Stability, Ages, Resets, Gates, locations, and infrastructure.

Use Pending Consequences and Review Points for later responses. Do not resolve every possible reaction immediately or freeze unaffected off-screen activity.

## Pending Consequences

A **Pending Consequence** records a caused future pressure whose exact resolution, timing, target, or expression remains open.

It includes:

- source event;
- affected owner and scope;
- causal route;
- earliest and latest relevant horizon where known;
- trigger or Review Point;
- actors capable of affecting it;
- uncertainty;
- status;
- closure event.

Pending is not predetermined. A witness intending to report a crime creates an information pressure, not guaranteed arrest.

## No-Op Interactions

FR-011 requires the runtime to determine `Affected Set = empty` explicitly before using this path. Apparent triviality or failure does not prove no change: time, resources, Knowledge, Relationships, injuries, and process progress still require inspection.

A **No-Op Update** occurs when the protocol determines that a completed interaction produced no persistent campaign-state change.

Examples may include:

- a rules clarification with no changed ruling or choice;
- an action abandoned before commitment or cost;
- a failed search that produced no new evidence, trace, time cost, exposure, or information;
- repeated viewing of already known information with no new interpretation or consequence.

The Session Log records the interaction and No-Op conclusion when material to play order or future understanding. Do not rewrite state records, create fake Timeline Events, grant progression, or increment counters merely to prove the save system ran.

If time passed, attention shifted, information was confirmed, a choice was revealed, or another consequence occurred, the interaction is not a complete No-Op for those owners.

## Compound and Batched Interactions

Several tightly coupled operations may use one transaction when:

- they form one declared action or indivisible adjudication;
- intermediate state cannot be used by another actor;
- one failure must roll back the entire set;
- all owners and effects can validate together.

Long scenes, encounters, Time Skips, Reincarnation, and world simulation should create intermediate Save Points at stable decision or recovery boundaries.

Batching for performance is allowed only when every completed interaction first receives a durable ordered Session Delta and Transaction identity. Do not continue dependent play while earlier changes exist only in model or human memory.

## Concurrency and Branches

Before activation, compare the transaction's parent Campaign Version with the active Save Index.

If they differ:

- do not overwrite the active version;
- identify whether the changes are duplicate, independent, conflicting, or sequential;
- preserve both candidate branches;
- apply Continuity Resolution for claim conflicts;
- use Migration and Versioning for a structural or branch merge;
- validate a new descendant before activation.

Last writer wins is not a canonical conflict policy.

## Atomicity

Atomicity means participants never observe a Save Point where only part of one committed Session Delta is active.

Storage implementations may use:

- database transactions;
- write-ahead journals;
- immutable version directories;
- copy-on-write records;
- Git commits;
- staged files plus atomic index replacement;
- another mechanism with equivalent semantics.

The technology is not canonical. The guarantees are:

- one parent;
- one bounded Write Set;
- all-or-nothing activation;
- recoverable Pending state;
- validated references;
- idempotent retry;
- traceable result.

## Interruption and Recovery

If interruption occurs before activation:

- keep the parent Campaign Version active;
- preserve the Transaction ID, Session Delta, staged operations, and last completed step;
- mark the transaction Interrupted or Recovery Pending;
- do not assume an attempted write succeeded;
- reload the parent and candidate before resuming;
- recheck the Read Set, active version, and conflicts;
- retry idempotently or abandon the candidate with reason;
- never duplicate costs, events, Inventory, progression, or history.

If the Session must continue before persistence recovers, it may proceed only from the last active Save Point or from an explicitly durable Pending Session state whose dependencies are loaded. Conversation memory alone is insufficient.

## Validation Before Activation

At minimum verify:

- Transaction and Interaction IDs are unique and correctly linked;
- parent Campaign Version still governs;
- every changed claim belongs to the Affected Set;
- every operation has an owner, source, event, and truth layer;
- every typed reference resolves;
- Timeline and Session order remain distinct and coherent;
- Campaign History received every materially durable event;
- numerical changes have complete traces;
- Relationships preserve direction and history;
- Research confidence and confirmation routes are valid;
- Projects and Mysteries preserve state and Review Points;
- Inventory quantities, custody, and locations reconcile;
- Soul and Incarnation boundaries remain valid;
- Character Knowledge and GM Secrets do not leak;
- Pending Consequences have causes and review conditions;
- unknowns remain unknown;
- no unrelated record changed;
- no campaign state entered the canonical rules repository.

Validation checks but does not invent repairs. A failure returns the transaction for correction or Continuity Resolution.

## Worked Examples

### Consuming a Healing Potion

A character drinks one potion and receives the valid bodily effect.

The Affected Set includes Inventory quantity and custody, body condition, the use event, witnesses who observed it, elapsed time if material, and any trace or container. One numerical trace removes the potion. The healing owner records the bodily change. Session Log and Timeline append the event. Campaign History receives it only if the injury, resource, or scene is materially durable.

### Discovering a New Species Trait

A researcher observes a monster surviving a toxin.

The update creates an Observation Record, updates the Research Project and relevant Character Knowledge, links Species and location references, and schedules a Review Point. It does not immediately change the Species fact or create Confirmed Knowledge from one observation.

### A Promise Between Rivals

Two rivals agree to a temporary truce.

The update appends the meeting, communication, exact Promise Record, each participant's interpretation, scoped trust or hostility effects only where supported, Timeline Event, and Campaign History. It does not set a universal friendship score or guarantee compliance.

### Starting a Workshop Project

A group secures a site, appoints workers, commits materials, and begins construction.

The update creates a Project, changes Inventory custody, location access, faction and Relationship obligations, and Infrastructure state only to the extent work actually begins. It records costs, dependencies, risks, and a Review Point. The workshop is not complete because the Project was named.

### Failed Investigation With a Trace

A character fails to identify a hidden passage but leaves obvious tool marks and spends an hour.

The search yields no discovery, but the update records elapsed time, changed location traces, tool or stamina costs if established, observer Knowledge, and a Pending Consequence if patrols may notice. It is not a No-Op.

### Interrupted Final Death Save

The system records bodily Final Death but fails before Life Reconciliation and world consequences stage completely.

The parent Save Point remains active and the transaction remains Recovery Pending. On resume, the GM reloads the adjudicated death, staged operations, and all owners, then commits one valid transition boundary. It does not duplicate the death, form two Echoes, or leave the body dead while the active Incarnation still acts.

### Concurrent Edits

A human GM updates an NPC Relationship while an automated tool stages a world event from the same parent Campaign Version.

The first valid transaction activates. The second detects the changed parent, checks whether the relationship and world event are independent, and creates a new descendant or conflict case. It never overwrites by file modification time.

## Safeguards

- Run the protocol after every completed gameplay interaction.
- Save at the smallest boundary needed before dependent adjudication.
- Update only the Affected Set and dependency closure.
- Route each claim through its Authoritative Record Owner.
- Append Session Log for every completed interaction.
- Append Timeline for material in-world events and Campaign History for durable continuity.
- Explicitly record Projects, Mysteries, discoveries, disproved theories, Pending Consequences, and Review Points.
- Preserve truth layers, Knowledge, and Secret boundaries.
- Require Numerical Change Traces.
- Preserve Relationship direction, history, and participant agency.
- Preserve Soul and Incarnation separation.
- Use one unique Transaction ID and idempotent retry.
- Activate all or nothing from one parent Campaign Version.
- Keep the prior Save Point active after interruption or failure.
- Never use last-writer-wins conflict resolution.
- Never rewrite unrelated records for formatting or convenience.
- Never create fake changes for a No-Op interaction.
- Never store populated transactions, saves, or campaign state in this repository.

## Scope Boundaries

This document defines ordinary post-interaction save transactions, Affected Sets, Session Deltas, owner-routed writes, history appends, special record handling, atomicity, interruption, concurrency, and Save Point activation. It does not define storage syntax, Migration, the full persistence validation catalogue, specialist mechanics, campaign templates, or populated campaign records.

## Related Documents

- [Retained Cross-Life Development](../progression/RETAINED_CROSS_LIFE_DEVELOPMENT.md) defines when a new Life contribution or embodiment change requires source-ledger and derived-profile recalculation. Retained effective profiles are derived state and must not overwrite their authoritative Life Archive, Development, Skill, or embodiment sources.

- [Campaign Persistence Engine Index](README.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Persistence Levels](PERSISTENCE_LEVELS.md)
- [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md)
- [Research Engine](RESEARCH_ENGINE.md)
- [Timeline Engine](TIMELINE_ENGINE.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Continuity Resolution](CONTINUITY_RESOLUTION.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Consequence Resolution](../gm/CONSEQUENCE_RESOLUTION.md)
- [Reincarnation](../soul/REINCARNATION.md)
- [World Engine](../world-engine/README.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
### Life Archive Updates

Ordinary interactions do not rewrite finalized Life Summaries. Include the [Life Archive](LIFE_ARCHIVE.md) in the Affected Set when establishing an active Life index, finalizing a completed Life, recovering historical incarnation evidence, or correcting an archive reference. Life completion atomically closes the active Life, creates or revises its summary, updates the Soul Overview, and validates deeper references without transferring current state into the archive.

### Soul-Bound Companion Updates

Include the [Soul-Bound Companion](../soul/SOUL_BOUND_COMPANIONS.md) owner when formation, interference, separation, convergence state, or a Reunion Manifestation materially changes. Persist current Relationships, Memory or Knowledge, world routes, Timeline, Campaign History, and Life Archive references only through their own owners in the same Affected Set. A nearby encounter that changes none of these facts does not rewrite the bond.
