# Context Assembly and Gameplay Turn Persistence

## Purpose

This document defines the FR-011 runtime contract that makes relevant canonical reading and automatic persistence part of a valid Gameplay Turn. It preserves the repository's established Read Set and Save Update procedures while connecting them through a compact, persistence-backed Context Assembly Layer.

The player does not issue a manual save command during ordinary play. A state-changing turn is not complete until its canonical transaction succeeds, validates, and is available to later reads.

## Document Control

- **Owner:** Gameplay Turn state, Context Packet structure, relevance selection, derived context hierarchy, mandatory read gate, automatic persistence gate, and next-turn verification
- **Dependencies:** [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md), [Canonical Data Ownership](../persistence/CANONICAL_DATA_OWNERSHIP.md), [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md), [Persistence Validation](../persistence/PERSISTENCE_VALIDATION.md), [AI Runtime Model](AI_RUNTIME_MODEL.md), and the configured Direct Adapters or Managed Data Service
- **Extensions:** runtime hosts may implement parameterized queries, views, application query functions, disposable caches, and purpose-specific [Canonical Visual Context](CANONICAL_VISUAL_CONTEXT.md) packets without changing authority
- **Consumers:** AI execution profiles, human-supervised runtime tools, session boot, play, representation handoffs, save, recovery, debugging, and handoff procedures
- **Repository boundary:** this contract contains no campaign state, populated packet, executable campaign schema, credential, private locator, or provider-specific configuration

## Runtime Boundary

This repository is rules- and contract-first. It contains no universal executable campaign host and no populated campaign database. It can enforce FR-011 through canonical procedures, execution-profile requirements, blank record contracts, adapter obligations, and structural regression validation. A conforming runtime host must execute the reads, writes, validation, and read-back against the configured campaign persistence.

Repository validation proves that the required contract and integration points exist and remain consistent. It cannot prove that an external host actually performed campaign I/O. A runtime must not claim FR-011 conformance unless its observed behavior satisfies the acceptance cases in [Context Assembly Conformance](CONTEXT_ASSEMBLY_CONFORMANCE.md).

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
17. A new player Gameplay Turn begins only with actual new player input; assistant continuation and internal tool work remain inside the originating interaction.
18. When resolution reaches an unresolved player-controlled choice, the GM yields and waits rather than selecting through an internal continuation.

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

`Player Action` means actual new player input. Hidden reasoning, assistant continuation, tool invocation or result, retry, canonical read, rule retrieval, persistence, validation, and same-input world simulation do not reopen the player-action boundary. They remain work inside the same interaction. Once the resulting situation requires a new player-owned choice, processing stops until that input arrives.

Prepared narration may exist before persistence, but it is not a delivered durable consequence. A strict execution profile uses `resolve target -> read -> resolve -> persist -> validate -> verify configured authority -> refresh derived context -> deliver`. No background promise, asynchronous stage, decorative status, or later-save claim satisfies this gate.

`TURN_COMPLETE` is invalid when a non-empty Affected Set lacks canonical write, required validation, configured-authority read-back, or expected-change evidence. Ordinary final gameplay output corresponds only to `TURN_COMPLETE`.

## Persistence Target Resolution Gate

Before state-changing play, the runtime reads Campaign Configuration and the Save Index to identify:

- campaign identity and persisted Persistence Strategy;
- configured canonical authority: Direct local, Direct cloud, or Managed service;
- complete Direct Adapter Chain or Managed service/interface contract and required durability policy;
- active Campaign Version, Save Point, and concurrency evidence;
- unresolved transaction or synchronization state;
- local working-copy role and freshness.

The runtime enters `PERSISTENCE_TARGET_READY` only after the configured target is located, fetched or contacted where required, and verified against campaign identity and version evidence. A missing assumed local path is not proof that the campaign database is missing. When configuration identifies Direct remote authority, the runtime must search or fetch that exact configured target before reporting absence. When configuration identifies Managed authority, the runtime resolves the service, interface, Logical Data Namespace, and campaign binding; it does not search for the service's backend datastore.

Never create a blank replacement database while a configured canonical save may exist remotely. Ambiguous identity, failed lookup, inaccessible authority, or unresolved prior persistence blocks state-changing play and produces a truthful pending or failure state.

## Context Assembly Layer

The **Context Assembly Layer** selects the smallest complete Read Set that can materially affect the current intent, resolution, consequence, uncertainty, or disclosure.

```text
Canonical Persistence
    -> Relevance Selection
-> Direct Parameterized Queries or Managed Semantic Reads
    -> provenance-bearing Rule Context retrieval
    -> Context Packet
    -> GM Resolution
```

Database stored procedures are not assumed. Implementations may use:

- views over authoritative records;
- parameterized owner queries;
- application-side context builders;
- indexed lookup and reference tables;
- versioned Derived or Cache records;
- structured packet serialization.

These mechanisms navigate authority. They do not create another owner.

Rule retrieval follows the separate [Rule Compilation and Context-Efficient Retrieval](../rules/RULE_COMPILATION_AND_RETRIEVAL.md) contract. In Managed mode, the service returns an already-published, dependency-complete Rule Packet; the AI does not crawl or compile the repository. The runtime loads the compact Runtime Rule Kernel, relevant Core rules, selected World/Ruleset rules, enabled modules, and operation/topic sources. Campaign Canon remains a separate authoritative read under the same Campaign ID. The normal rule target is at most 8,000 estimated tokens; relevance filtering must exclude unrelated worlds without omitting a materially required rule or hiding an uncounted repository prompt.

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
6. commit owner writes atomically through the configured Direct Adapter Chain or Managed service;
7. run implementation and semantic validation;
8. perform critical read-back, expected-versus-actual comparison, and version verification;
9. verify the configured canonical authority, including Direct cloud synchronization or Managed completion evidence where required;
10. activate the new Save Point only at the configured authority boundary;
11. regenerate or invalidate Running Summary, Session Summary, and Current Scene Context from verified Canon;
12. enter `TURN_COMPLETE` and deliver the final player-facing result with the evidence-derived status marker.

The player issues no save command. Direct local database success alone is insufficient when configuration requires remote deployment. Managed interface success alone is insufficient without validated service completion evidence.

### Constrained Patch Updates

A Managed service may accept a bounded set-only patch for existing authoritative JSON-object records when reconstructing an entire record would waste context or risk erasing omitted state. The service, not the model, reads each current record, preserves omitted fields, applies only supplied values, expands the result into the ordinary full owner-routed mutation path, and enforces the same expected Campaign Version, record revision, transaction identity, validation, activation, read-back, and receipt boundary.

This path cannot create or delete records, alter identity or reference-bearing fields, accept arbitrary SQL, bypass owner routing, or turn omission into deletion. Structural identity/reference changes and unsupported payloads use the full mutation contract. Retry retains the same transaction and idempotency identity, and a failed patch activates nothing.

## Player-Visible Persistence Status

Every ordinary Gameplay Context response ends with exactly one compact marker describing actual persistence state:

| Marker | Canonical meaning |
| --- | --- |
| **💾** | The configured **local** canonical target committed, validated, and passed required read-back. |
| **☁️💾** | The configured **cloud** canonical target synchronized and passed required remote read-back and verification. |
| **⏳** | Required persistence remains incomplete or a recoverable transaction is genuinely pending. |
| **⚠️** | A required write, synchronization, expected-change check, validation, or read-back failed. |

In Managed mode, `💾` means the configured service issued validated completion evidence for the expected transaction and active Campaign Version. Managed mode does not use `☁️💾`: remote/local backend topology is hidden behind the service and cannot be classified by the client.

The marker is evidence, not decoration. Prepared narration, an updated summary, a local candidate, an upload attempt, a requested synchronization, or intent to save cannot produce `💾` or `☁️💾`.

For a cloud-authoritative campaign, local SQLite success leaves status `⏳` until configured remote deployment, read-back, semantic verification, and any required backup verification complete. A cloud failure changes status to `⚠️` while preserving the validated local candidate for idempotent synchronization retry.

A verified empty Affected Set may display the marker for the already-verified configured canonical authority; it does not create a new save. Successful operation remains unobtrusive: show only the marker unless the player requests status, Development Context is active, or failure requires a concise technical explanation.

No subsequent state-changing Gameplay Turn may proceed while status is `⏳` or unresolved `⚠️`. Resolve it to the saved marker valid for the configured mode, recover the last validated authority, or explicitly stop dependent play.

## Manual Persistence Commands

Manual commands are an override and recovery interface. Automatic persistence remains mandatory.

### `save`

Determine any pending Affected Set and run the normal canonical transaction immediately. Reuse Interaction and Transaction identity; do not replay resolution or duplicate costs, chronology, Development, Inventory, Relationships, or other effects. If nothing is pending, verify the current configured canonical target and report its evidence-derived marker without inventing a save event.

### `save status`

Return only authorized operational facts:

- current marker;
- canonical Campaign Version and Save Point where available;
- persisted Persistence Strategy and canonical authority;
- Direct Adapter Chain and local-versus-cloud synchronization state, or Managed service/interface contract and completion-evidence state;
- whether an Affected Set or transaction remains pending;
- last successful local commit evidence;
- last successful canonical cloud synchronization and verification evidence where configured;
- brief sanitized failure reason when status is `⚠️`.

Do not expose credentials, private locators, GM Secrets, or unrelated campaign data.

### `retry save`

Resume the unresolved transaction from the earliest incomplete persistence stage. Inspect what already committed before writing. When a Direct local candidate is valid and only cloud synchronization failed, retry cloud deployment and verification without reapplying gameplay changes. In Managed mode, retry through the same service Transaction ID and idempotency key. The command cannot replay narration, consume resources again, duplicate events, or create a second Save Point for the same transaction.

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
2. identify the persisted Persistence Strategy and its Direct canonical source or Managed service/interface binding;
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
- [Portable Persistence Architecture](../persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- [Managed Data Service](../persistence/MANAGED_DATA_SERVICE.md)
- [MCP Managed Service Interface](../persistence/MCP_PERSISTENCE_MODE.md)
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
- [Rule Compilation and Retrieval](../rules/RULE_COMPILATION_AND_RETRIEVAL.md)
- [FR-011 Implementation Audit](../../design/audits/FR_011_CONTEXT_AND_PERSISTENCE_AUDIT.md)

### Existing Campaign Adoption

Existing-campaign setup is an administrative [host adoption procedure](CONTEXT_ASSEMBLY_ADOPTION.md), not an ordinary gameplay Rule Source. It retains backup, audit, configuration and validation duties. Stop a material profile conversion or retcon until complete governance, specialist authority and evidence are available; ordinary context retrieval does not execute adoption.
