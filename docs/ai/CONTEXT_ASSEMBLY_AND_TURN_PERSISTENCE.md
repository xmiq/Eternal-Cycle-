# Context Assembly and Gameplay Turn Persistence

## Purpose

This document defines the FR-011 runtime contract that makes relevant canonical reading and automatic persistence part of a valid Gameplay Turn. It preserves the repository's established Read Set and Save Update procedures while connecting them through a compact, persistence-backed Context Assembly Layer.

The player does not issue a manual save command during ordinary play. A state-changing turn is not complete until its canonical transaction succeeds, validates, and is available to later reads.

## Document Control

- **Owner:** Gameplay Turn state, Context Packet structure, relevance selection, derived context hierarchy, mandatory read gate, automatic persistence gate, and next-turn verification
- **Dependencies:** [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md), [Canonical Data Ownership](../persistence/CANONICAL_DATA_OWNERSHIP.md), [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md), [Persistence Validation](../persistence/PERSISTENCE_VALIDATION.md), [AI Runtime Model](AI_RUNTIME_MODEL.md), and configured Persistence Adapters
- **Extensions:** runtime hosts may implement parameterized queries, views, application query functions, disposable caches, and purpose-specific [Canonical Visual Context](CANONICAL_VISUAL_CONTEXT.md) packets without changing authority
- **Consumers:** AI execution profiles, human-supervised runtime tools, session boot, play, representation handoffs, save, recovery, debugging, and handoff procedures
- **Repository boundary:** this contract contains no campaign state, populated packet, executable campaign schema, credential, private locator, or provider-specific configuration

## Runtime Boundary

This repository is rules- and contract-first. It contains no universal executable campaign host and no populated campaign database. It can enforce FR-011 through canonical procedures, execution-profile requirements, blank record contracts, adapter obligations, and structural regression validation. A conforming runtime host must execute the reads, writes, validation, and read-back against the configured campaign persistence.

Repository validation proves that the required contract and integration points exist and remain consistent. It cannot prove that an external host actually performed campaign I/O. A runtime must not claim FR-011 conformance unless its observed behavior satisfies the acceptance cases in this document.

## Core Invariants

1. Conversation context is a convenience layer only; it is not Canon.
2. Required canonical state is read before material resolution.
3. Context Assembly is relevance-filtered and persistence-backed.
4. Context Packets, Running Summaries, and Session Summaries are Derived Data or Caches and own no mutable Canon.
5. Every resolved interaction receives an explicit Affected Set determination, including an explicit empty result.
6. A non-empty Affected Set triggers persistence automatically without a player command.
7. A state-changing turn cannot close until its Save Transaction succeeds and validates.
8. Failed reads and writes remain explicit; conversation memory cannot conceal them.
9. Retry preserves Interaction and Transaction identity and cannot duplicate effects.
10. Turn N+1 reads the committed Turn N state when relevant.
11. Successful persistence plumbing remains backstage during ordinary Gameplay Context.
12. The AI GM operates through rules and campaign state; it does not own either.
13. The configured canonical persistence target is resolved and verified before state-changing play.
14. Player-visible persistence status is derived from adapter evidence, never intent or prepared narration.
15. Derived context refresh follows canonical validation and read-back; it never promotes unsaved narration.
16. A canonical representation request receives a purpose-specific, visibility-filtered Canonical Visual Context before depiction; conversation context is not visual authority.

## Gameplay Turn State Machine

The canonical state sequence is:

```text
TURN_OPEN
    -> PERSISTENCE_TARGET_REQUIRED
    -> PERSISTENCE_TARGET_READY
    -> CONTEXT_REQUIRED
    -> READ_REQUIRED
    -> READ_COMPLETE
    -> RESOLUTION_COMPLETE
    -> AFFECTED_SET_DETERMINED
    -> SAVE_REQUIRED       [when Affected Set is non-empty]
    -> CANONICAL_WRITE_COMPLETE
    -> VALIDATION_COMPLETE
    -> CANONICAL_AUTHORITY_VERIFIED
    -> DERIVED_CONTEXT_REFRESHED
    -> TURN_COMPLETE
```

A verified empty Affected Set proceeds from `AFFECTED_SET_DETERMINED` to `VALIDATED` without a write. This is a determined no-op, not an assumption.

The externally meaningful flow is:

```text
Player Action
    -> Assemble Relevant Context
    -> Read Required Canonical State
    -> Resolve and Simulate
    -> Determine Affected Set
    -> Persist Required Changes
    -> Validate and Verify
    -> Deliver the Final Player-Facing Result
```

Prepared narration may exist before persistence, but it is not a delivered durable consequence. A strict execution profile uses `resolve target -> read -> resolve -> persist -> validate -> verify configured authority -> refresh derived context -> deliver`. No background promise, asynchronous stage, decorative status, or later-save claim satisfies this gate.

`TURN_COMPLETE` is invalid when a non-empty Affected Set lacks canonical write, required validation, configured-authority read-back, or expected-change evidence. Ordinary final gameplay output corresponds only to `TURN_COMPLETE`.

## Persistence Target Resolution Gate

Before state-changing play, the runtime reads Campaign Configuration and the Save Index to identify:

- campaign and canonical artifact identity;
- configured canonical authority: local or cloud;
- complete Adapter Chain and required backup policy;
- active Campaign Version, Save Point, and concurrency evidence;
- unresolved transaction or synchronization state;
- local working-copy role and freshness.

The runtime enters `PERSISTENCE_TARGET_READY` only after the configured target is located, fetched where required, and verified against campaign identity and version evidence. A missing assumed local path is not proof that the campaign database is missing. When configuration identifies remote authority, the runtime must search or fetch that exact configured target before reporting absence.

Never create a blank replacement database while a configured canonical save may exist remotely. Ambiguous identity, failed lookup, inaccessible authority, or unresolved prior persistence blocks state-changing play and produces a truthful pending or failure state.

## Context Assembly Layer

The **Context Assembly Layer** selects the smallest complete Read Set that can materially affect the current intent, resolution, consequence, uncertainty, or disclosure.

```text
Canonical Persistence
    -> Relevance Selection
    -> SQLite-Compatible Views, Parameterized Queries, or Application Query Functions
    -> Context Packet
    -> GM Resolution
```

SQLite stored procedures are not assumed. Implementations may use:

- views over authoritative records;
- parameterized owner queries;
- application-side context builders;
- indexed lookup and reference tables;
- versioned Derived or Cache records;
- structured packet serialization.

These mechanisms navigate authority. They do not create another owner.

## Relevance Selection

Include a record when it can materially affect the present claim through:

- physical presence or current placement;
- direct mention, target, or player intent;
- recent interaction or unresolved immediate consequence;
- direct Relationship, commitment, debt, hostility, or promise;
- active assignment, Project, Research, Mystery, or Infrastructure dependency;
- immediate hazard, resource, condition, or opposition;
- invoked Skill, Development, equipment, magic, species, form, or Soul rule;
- relevant recent event or explicit historical callback;
- required visibility, Character Knowledge, or GM Secret boundary.
- relevant [Memory Continuity](../soul/MEMORY_CONTINUITY.md) records when autobiographical recall or a possible cue can materially affect resolution.
- relevant [Visual Identity](../persistence/VISUAL_IDENTITY.md), Species or form, Model, equipment, condition, Location, and environment records when canonical depiction is requested.

Do not load every row connected by several references merely because it exists. Stop dependency expansion when further records cannot materially change the action or its presentation. Context correctness outranks marginal brevity.

## Current Scene Context

The **Current Scene Context** is the smallest packet for one active interaction. Include only relevant fields, such as:

- Interaction ID and active Save Point;
- current game time and active Location ID;
- active Perspective and player-character Entity ID;
- current intent and unresolved action boundary;
- present or directly relevant Entity, Autonomous ID, Group, and Relationship references;
- relevant conditions, hazards, resources, Inventory, Projects, Research, Infrastructure, and world pressures;
- likely canonical rule dependencies;
- recent meaningful events;
- source owner, stable record ID, source version, and optional short reference path;
- deep-read references for omitted detail;
- freshness and provenance metadata.

Internal source navigation may identify a logical owner or implementation source, but table names, IDs, and transaction plumbing remain outside ordinary player narration.

### Reference Paths

A packet may carry short navigation paths without copying records:

```text
Entity ID -> Relationship ID -> related Group ID
Autonomous ID -> assignment reference -> Project ID -> Location ID
Entity ID -> Skill ID -> Development source references
Life ID -> Long-Horizon Historical Period ID -> source event IDs
```

Each hop identifies its authoritative domain. A reference path grants neither visibility nor Character Knowledge.

Loading a Memory Record, Life Summary, or complete historical source for GM adjudication does not grant recall. Player-facing assembly includes only the bounded Recall Manifestation available through the current incarnation's Character Knowledge.

## Purpose-Specific Visual Context

When the requested output depicts canonical campaign content, assemble a [Canonical Visual Context](CANONICAL_VISUAL_CONTEXT.md) rather than handing the image or rendering tool an unsourced prose recollection. The packet is a Derived projection over the smallest complete visual Read Set. It records stable subject IDs, established visual facts, current-form and Model references, present equipment and conditions, environmental facts, observer visibility, secret exclusions, genuine unknowns, and permitted non-canonical rendering freedom.

The packet owns no appearance fact. Current Appearance is assembled from authoritative owners at request time. Missing visual traits remain unspecified, and a generated choice does not become Canon merely because it appears in an image. Ordinary image generation has an empty Affected Set. Only an explicit authorized adoption, correction, or other durable campaign change enters the normal owner-routed Save Transaction.

## Hierarchical Context

Begin with the smallest sufficient level and drill deeper only when required:

```text
Current Scene Context
    -> Recent Running Summary
    -> Session Summary
    -> Long-Horizon Summary
    -> Life Summary
    -> Full Canonical Records
```

This is a retrieval hierarchy, not an authority hierarchy. Full owner records outrank every Derived packet. Different branches may be followed directly when the question warrants it.

### Recent Running Summary

The **Recent Running Summary** is a short-lived Cache for immediate continuity. It may identify the current scene, active intent, relevant Entities, recent committed changes, unresolved immediate action, important observations, and owner references. It is regenerated after relevant Canon changes.

### Session Summary

The **Session Summary** is a wider Derived index of major session events, discoveries, Relationship changes, Project or Infrastructure changes, significant Skill or Development events, unresolved threads, and stable record pointers. It is not Campaign History and cannot replace specialist owners.

### Historical Sources

Load [Long-Horizon Summaries](../persistence/LONG_HORIZON_SUMMARIES.md) only for materially relevant historical intervals. Load [Life Archive](../persistence/LIFE_ARCHIVE.md) only for relevant prior-Life identity, retained-development, Skill history, Relationship callbacks, Final Death, legacy, or other supported queries. Do not load every era or Life each turn.

## Freshness

Every stored Derived packet records enough metadata to detect staleness:

- generated game time or interval;
- parent Campaign Version and Save Point;
- Persistence Model and Migration versions where relevant;
- Interaction ID and source record IDs;
- source revisions, hashes, or equivalent change tokens where practical;
- visibility and audience scope;
- generation provenance.

Wall-clock age alone does not determine freshness. If a relevant source changes, invalidate or regenerate the packet. When packet content conflicts with authoritative data:

1. authoritative data wins;
2. mark the packet stale or incorrect;
3. regenerate or repair it from owners;
4. never edit Canon to match the packet.

## Mandatory Read Gate

Before material resolution:

1. read the Save Index, active Campaign Version, Save Point, Current Session, and open recovery state;
2. identify player intent, active Perspective, Entity, current time, and current placement;
3. identify the narrowest canonical rule and campaign owner for each material claim;
4. build the initial relevance-filtered Read Set;
5. expand through material Typed References;
6. verify source identity, version, status, Truth Layer, visibility, effective time, and supersession;
7. classify material gaps as Unknown, Not Yet Verified, Estimated, Disputed, or Requires Source Recovery;
8. enter `READ_COMPLETE` only when all material dependencies are resolved or validly bounded.

The established [Campaign State Model read discipline](../persistence/CAMPAIGN_STATE_MODEL.md#required-read-discipline) remains authoritative. FR-011 wraps and verifies it; it does not replace historically successful owner reads.

If a required read fails, do not substitute conversation memory. Continue only when the missing record is genuinely immaterial or Canon permits resolution under explicit uncertainty.

## Resolution and Affected Set Gate

After resolution, determine every persistent owner changed or materially created. Check at least:

- Entity identity or body state;
- Skills and Development;
- current placement;
- Inventory and Custody;
- conditions, injuries, healing, and effects;
- Relationships and Character Knowledge;
- Research, Projects, Infrastructure, and Mysteries;
- Autonomous Registry assignment, condition, Controller, network, or last-confirmed state;
- network, communication, delegation, and assignment state;
- equipment and other durable resources;
- Species, Evolution, Soul, and world state;
- Location and other durable exploration or discovery state;
- Timeline, Session Log, and Campaign History.
- explicit adoption, correction, or removal of a canonical Visual Identity trait.

Movement, time, expenditure, failed attempts, discoveries, meaningful observations, and social reactions may change state. Do not infer an empty Affected Set from a quiet narration.

## Automatic Persistence Gate

For a non-empty Affected Set:

1. retain the Interaction ID and assign or reuse its Save Transaction ID;
2. recheck the parent Save Point and Read Set;
3. build the dependency-complete Affected Set and Session Delta;
4. stage each update once through its Authoritative Record Owner;
5. append Session Log, Timeline, and Campaign History only under their existing rules;
6. commit owner writes atomically through the configured adapter chain;
7. run implementation and semantic validation;
8. perform critical read-back, expected-versus-actual comparison, and version verification;
9. verify the configured canonical authority, including required cloud synchronization and read-back;
10. activate the new Save Point only at the configured authority boundary;
11. regenerate or invalidate Running Summary, Session Summary, and Current Scene Context from verified Canon;
12. enter `TURN_COMPLETE` and deliver the final player-facing result with the evidence-derived status marker.

The player issues no save command. Local SQLite success alone is insufficient when campaign configuration requires remote deployment and read-back through the Google Drive adapter.

## Player-Visible Persistence Status

Every ordinary Gameplay Context response ends with exactly one compact marker describing actual persistence state:

| Marker | Canonical meaning |
| --- | --- |
| **💾** | The configured **local** canonical target committed, validated, and passed required read-back. |
| **☁️💾** | The configured **cloud** canonical target synchronized and passed required remote read-back and verification. |
| **⏳** | Required persistence remains incomplete or a recoverable transaction is genuinely pending. |
| **⚠️** | A required write, synchronization, expected-change check, validation, or read-back failed. |

The marker is evidence, not decoration. Prepared narration, an updated summary, a local candidate, an upload attempt, a requested synchronization, or intent to save cannot produce `💾` or `☁️💾`.

For a cloud-authoritative campaign, local SQLite success leaves status `⏳` until configured remote deployment, read-back, semantic verification, and any required backup verification complete. A cloud failure changes status to `⚠️` while preserving the validated local candidate for idempotent synchronization retry.

A verified empty Affected Set may display the marker for the already-verified configured canonical authority; it does not create a new save. Successful operation remains unobtrusive: show only the marker unless the player requests status, Development Context is active, or failure requires a concise technical explanation.

No subsequent state-changing Gameplay Turn may proceed while status is `⏳` or unresolved `⚠️`. Resolve it to `💾` or `☁️💾`, recover the last validated authority, or explicitly stop dependent play.

## Manual Persistence Commands

Manual commands are an override and recovery interface. Automatic persistence remains mandatory.

### `save`

Determine any pending Affected Set and run the normal canonical transaction immediately. Reuse Interaction and Transaction identity; do not replay resolution or duplicate costs, chronology, Development, Inventory, Relationships, or other effects. If nothing is pending, verify the current configured canonical target and report its evidence-derived marker without inventing a save event.

### `save status`

Return only authorized operational facts:

- current marker;
- canonical Campaign Version and Save Point where available;
- configured canonical authority and Adapter Chain role;
- local-versus-cloud synchronization state;
- whether an Affected Set or transaction remains pending;
- last successful local commit evidence;
- last successful canonical cloud synchronization and verification evidence where configured;
- brief sanitized failure reason when status is `⚠️`.

Do not expose credentials, private locators, GM Secrets, or unrelated campaign data.

### `retry save`

Resume the unresolved transaction from the earliest incomplete persistence stage. Inspect what already committed before writing. When local SQLite is valid and only cloud synchronization failed, retry cloud deployment and verification without reapplying gameplay changes. The command cannot replay narration, consume resources again, duplicate events, or create a second Save Point for the same transaction.

## Canonical Change Proof and Unchanged-Save Detector

For a non-empty Affected Set, completion evidence uses the minimum sufficient combination required by the active persistence profile, such as:

- Campaign Version or record revision increment;
- expected owner rows changed exactly as staged;
- required Timeline or Campaign History append;
- active Save Index and Save Point update;
- canonical artifact hash or exact-byte change where expected;
- remote identity, synchronization, and read-back evidence.

If expected persistent Canon was established but the configured canonical state remains unchanged, validation fails. The runtime prohibits `TURN_COMPLETE`, preserves retry evidence, and reports `⚠️`. A successful adapter call with absent expected state is not success.

## Canon Before Derived Context

The required ordering is:

```text
canonical owner writes
    -> configured-authority validation and read-back
    -> Running Summary update
    -> Session Summary update
    -> Current Scene Context refresh
    -> persistence marker
    -> TURN_COMPLETE
```

If persistence fails, derived summaries retain or rebuild from the last validated Save Point. They must not present prepared narration as canonical state.

## No-Change Turns

A no-change turn records an explicit `Affected Set = empty` result in ephemeral or transactional provenance as appropriate. It performs no canonical mutation and creates no Timeline event for database plumbing. A failed action is not automatically a no-op if it consumed time or resources, caused harm, changed a Relationship, established Knowledge, or advanced a process.

Canonical image generation is normally a no-change operation: assembling and rendering a Canonical Visual Context does not mutate campaign state. Its response may report the already-verified persistence marker for the configured authority, but it must not imply that rendering itself created a save. If the player explicitly adopts a rendered trait, that later adoption is a state-changing interaction with a non-empty Affected Set.

## Failure and Retry

### Read Failure

Enter Source Recovery, Continuity Resolution, clarification, or another honest blocked state. Preserve the requested intent and loaded evidence. Do not fabricate the missing value or claim that remembered conversation text was read from Canon.

### Write or Validation Failure

Narrative result determined is not the same as canonical transaction committed. Keep the last validated Save Point active, retain the staged Interaction ID, Transaction ID, expected changes, parent version, and failure evidence, and do not deliver dependent durable consequences as committed.

### Idempotent Retry

A retry of the same interaction retains transaction identity and verifies already-applied operations before writing. It must not duplicate:

- Timeline or Campaign History events;
- resource expenditure or Inventory transfer;
- Relationship changes;
- Skill or Development progress;
- Project or Research progress;
- Autonomous Registry changes;
- time advancement or consequence activation.

If concurrency changed the parent version, rebuild the relevant Read Set and resolve the conflict before retry.

## Next-Turn Verification

Turn N+1 must retrieve relevant Turn N changes from the active persisted Save Point, not from conversation memory or the prior packet. A conforming runtime:

1. reads the active Save Index and version;
2. invalidates packets whose parent version differs;
3. queries relevant owner records;
4. confirms expected prior-turn state when it affects resolution;
5. rebuilds Current Scene Context from those records.

Failure to retrieve a supposedly saved change is a persistence or continuity defect, not permission to narrate around it.

## Session Start and Context Reset

On new chat, model reset, transcript truncation, restored session, or GM handoff:

1. load Campaign Configuration and the latest Save Index;
2. identify the active adapter chain and canonical source;
3. load any persisted Running or Session Summary only as a Cache;
4. verify cache parent version, source revisions, visibility, and freshness;
5. discard or regenerate stale packets;
6. build the relevant canonical Read Set;
7. resume only from the validated Save Point and open interaction boundary.

A fresh runtime instance must recover continuity from persistence without prior conversation messages.

## Specialist Integrations

### FR-004 Life Archive

Use stable Life IDs and summary-to-detail paths for relevant prior-Life queries. Loading archive material gives GM context but does not grant current-character recall.

### FR-010 Long-Horizon Summaries

Use Historical Period IDs, tags, continuity hooks, and source references for relevant historical intervals. Do not inject unrelated era summaries into an immediate scene.

### FR-012 Canonical Data Ownership

Packets read from and point to authoritative owners. Current Scene Context, Running Summary, and Session Summary never become competing mutable stores.

### FR-014 Autonomous Registry

When relevant, select Autonomous ID, Model reference, Controller, autonomy, placement reference, assignment, condition, last Confirmed Report, network, resources, and maintenance dependencies. Do not load every autonomous unit.

### FR-015 and FR-016 Boundaries

GM Context is not Character Memory. FR-011 does not implement memory continuity, fading, or recall. It retrieves [Soul-Bound Companion](../soul/SOUL_BOUND_COMPANIONS.md) records only when identity, Reincarnation, encounter causality, separation, recognition, or historical callbacks make them relevant; retrieval neither creates a bond nor reveals protected identity, route, or timing to a character.

### Canonical Visual Context

[Visual Identity](../persistence/VISUAL_IDENTITY.md) owns sparse established appearance facts. [Canonical Visual Context](CANONICAL_VISUAL_CONTEXT.md) assembles those facts with current Species or form, Model, Inventory, condition, Location, environment, Perspective, and visibility records for one representation request. It is Derived Data, is invalidated by relevant source changes, and never becomes another appearance owner.

## Existing Campaign Adoption

An existing campaign adopts FR-011 through the normal migration and configuration process:

1. back up the latest canonical save;
2. validate the active Campaign Version and owner graph;
3. audit existing read procedures and preserve valid owner queries;
4. configure or regenerate context caches without rewriting Canon;
5. enable automatic turn transaction behavior in the execution profile or host;
6. verify adapter, validation, and read-back requirements;
7. run a harmless fixture or isolated validation transaction covering read, write, rollback, and reload;
8. record migration/setup provenance and activate only after validation.

This repository performs no migration of a populated campaign.

## Development Context and Auditability

An authorized Development Context may inspect:

- Interaction and Transaction IDs;
- Read Set and source revisions;
- Context Packet and relevance reasons;
- Affected Set and owner-routed operations;
- adapter and validation outcomes;
- read-back evidence and resulting Save Point;
- cache invalidation or regeneration.

Debug output obeys visibility and GM Secret boundaries. Ordinary Gameplay Context omits successful plumbing unless the user asks or an Operational Failure requires action.

## SQLite-Compatible Logical Guidance

Implementations may persist packet metadata and transactional provenance using equivalent normalized structures:

```sql
CREATE TABLE context_packets (
    context_packet_id TEXT PRIMARY KEY,
    context_kind TEXT NOT NULL CHECK (context_kind IN ('scene', 'running', 'session')),
    interaction_id TEXT,
    parent_campaign_version TEXT NOT NULL,
    parent_save_point_id TEXT NOT NULL,
    audience_scope TEXT NOT NULL,
    generated_game_time TEXT,
    generated_at TEXT NOT NULL,
    freshness_status TEXT NOT NULL CHECK (freshness_status IN ('fresh', 'stale', 'invalid')),
    provenance_id TEXT NOT NULL
);

CREATE TABLE context_packet_references (
    context_packet_id TEXT NOT NULL,
    owner_domain TEXT NOT NULL,
    record_id TEXT NOT NULL,
    source_revision TEXT,
    relevance_reason TEXT NOT NULL,
    reference_path TEXT,
    PRIMARY KEY (context_packet_id, owner_domain, record_id),
    FOREIGN KEY (context_packet_id) REFERENCES context_packets(context_packet_id)
);

CREATE INDEX idx_context_packet_parent
    ON context_packets(parent_campaign_version, parent_save_point_id, freshness_status);
```

Context content may remain transient or be serialized separately. These optional Cache structures own only derivation metadata and references. They do not own copied campaign facts. Concrete campaign schemas may use established IDs and names instead.

## Regression Cases

### A. Skill Read

An action invokes a Skill. The runtime reads the current Skill identity, Development, embodiment, access, condition, and relevant opposition before resolution.

### B. Automatic Character Save

A turn changes body condition. The Entity/body owner updates automatically without the player saying `save`, validates, and is read from persistence next turn.

### C. Relationship Save

A promise changes a Relationship. The Relationship owner updates in the same automatic transaction; character or session summaries only reference it.

### D. Multiple-Domain Transaction

Movement while consuming an item changes Entity condition, Inventory, current placement, and Timeline. One Interaction ID produces one dependency-complete transaction, and every effect appears exactly once.

### E. Autonomous Entity

An autonomous unit receives and accepts a new assignment. Relevant Autonomous Registry and Project references update; unrelated autonomous units are not loaded.

### F. No-Change Turn

A perception request reveals nothing new and advances no meaningful time. The runtime records `Affected Set = empty` and performs no mutation.

### G. Failed Write

Adapter validation fails. The parent Save Point remains active, the turn does not close as saved, and retry retains transaction identity.

### H. Contradictory Conversation

Conversation says an item remains; authoritative Inventory says it was consumed. The persisted owner wins and the packet is regenerated.

### I. Next-Turn Reload

Turn N changes location and commits. Turn N+1 queries current placement and retrieves the new Location ID before resolution.

### J. Stale Running Summary

A Running Summary references an earlier Campaign Version. It is invalidated and rebuilt rather than used to overwrite Canon.

### K. Deep Historical Query

Immediate context omits old history. A question about a former companion follows Entity, Life, Historical Period, Relationship, and source IDs only as far as needed.

### L. Context Reset

A fresh runtime receives no old transcript. It loads the Save Index, verifies summaries, builds the relevant Read Set, and resumes from persisted state.

### M. Multi-Domain Automatic Save

One interaction changes Infrastructure, Research, equipment, network communication, an Autonomous Registry assignment, exploration Knowledge, Timeline, and Campaign History. The runtime reads the relevant owners, persists one dependency-complete Affected Set without a `save` command, proves the version and expected owner changes, reloads them next turn, and ends the delivered response with `💾` or `☁️💾` according to configured authority. Narration-only change fails this case.

### N. Canonical Target Discovery

The first assumed local path is absent while Campaign Configuration identifies a remote canonical database. The runtime resolves exact remote identity, fetches it, verifies campaign and version, creates only a Local Working Copy, and writes through the configured chain. It neither reports a false missing database nor creates a blank replacement.

### O. Local-Only Completion

A local-authoritative SQLite campaign commits, validates, reopens read-only, proves expected changes, activates its Save Point, and displays `💾`.

### P. Cloud-Authoritative Completion

SQLite candidate validation succeeds, Google Drive replaces the exact canonical file, remote read-back and required comparison succeed, configured backup verification succeeds, and the response displays `☁️💾`.

### Q. Cloud Pending and Failure

When local validation succeeds but required cloud work is incomplete, status is `⏳` and another state-changing turn is blocked. When synchronization or verification fails, status becomes `⚠️`; the validated local candidate remains available for retry but is not the active cloud authority.

### R. False Cloud Success Prevention

An upload request returns but remote read-back never verifies the candidate. The runtime cannot display `☁️💾`, cannot close the state-changing turn, and retains failure evidence.

### S. Manual Save and Status

`save` executes a pending transaction once without replaying the interaction. `save status` reports marker, version, configured authority, local/cloud state, pending state, and last verified boundaries without exposing protected data.

### T. Idempotent Retry

Cloud synchronization fails after a validated local commit. `retry save` resumes at remote deployment using the same Transaction ID, does not rerun owner writes, and reaches `☁️💾` only after remote verification.

### U. Unchanged Canonical Save

A non-empty Affected Set is narrated, but expected canonical owner rows, chronology, Campaign Version, and artifact evidence remain unchanged. Validation reports failure, status becomes `⚠️`, and `TURN_COMPLETE` is prohibited.

## Acceptance Criteria

FR-011 conformance requires all of the following:

- ordinary play requires no manual save command;
- every non-empty Affected Set persists automatically;
- material canonical reads occur before resolution;
- conversation context never substitutes for an authoritative read;
- failed reads and writes remain explicit;
- later turns retrieve prior committed changes when relevant;
- Derived packets cannot override Canon;
- packets expose relevant stable IDs, owners, and drill-down references internally;
- selection remains relevance-filtered;
- fresh sessions rebuild context from persistence;
- the configured canonical target is resolved before state-changing play and is never guessed from one local path;
- a non-empty Affected Set proves expected canonical change before completion;
- local-authoritative success displays `💾` only after local validation and read-back;
- cloud-authoritative success displays `☁️💾` only after required synchronization and remote verification;
- `⏳` and unresolved `⚠️` block subsequent state-changing play;
- manual `save`, `save status`, and `retry save` preserve idempotency;
- Derived context refresh follows canonical verification;
- host tests exercise Regression Cases A through U against the configured persistence chain;
- the repository regression harness passes its mock-adapter state-machine cases.

## Safeguards

- Do not load the entire campaign by default.
- Do not omit a material dependency merely to shorten context.
- Do not save only the narrative summary.
- Do not create duplicate current-state owners in packet caches.
- Do not treat a failed action as automatically unchanged.
- Do not promise asynchronous or background saving.
- Do not expose storage plumbing or protected IDs during ordinary play.
- Do not grant Character Knowledge from GM retrieval.
- Do not implement FR-015 or FR-016 here.
- Do not add populated packets or campaign fixtures to this repository.

## Related Documents

- [AI Runtime Model](AI_RUNTIME_MODEL.md)
- [Canonical Visual Context](CANONICAL_VISUAL_CONTEXT.md)
- [Visual Identity](../persistence/VISUAL_IDENTITY.md)
- [AI Game Master Workflow](AI_GM_WORKFLOW.md)
- [AI Session Start](AI_SESSION_START.md)
- [AI Play Protocol](AI_PLAY_PROTOCOL.md)
- [AI Save Protocol](AI_SAVE_PROTOCOL.md)
- [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md)
- [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../persistence/PERSISTENCE_VALIDATION.md)
- [Context Packet Template](../../templates/CONTEXT_PACKET_TEMPLATE.md)
- [FR-011 Implementation Audit](../../design/audits/FR_011_CONTEXT_AND_PERSISTENCE_AUDIT.md)
