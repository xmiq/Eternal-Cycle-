# Campaign State Model

## Purpose

This document defines the authoritative model of a campaign's established current condition. It explains how modular records form one Campaign State Graph, what every current-state claim must contain, what a GM must load before adjudication, how Session changes relate to confirmed state and history, and how unknown or numerical information is handled.

It defines a logical model, not a save-file format or populated campaign.

## Core Rule

The structured Campaign State Graph is the authoritative representation of established campaign state.

Each mutable claim in that graph resolves to exactly one logical owner under [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md). Graph connectivity does not authorize duplicate current-state values in multiple modules.

Conversation context, narration, transcripts, summaries, model memory, and participant recollection may supplement it as evidence or navigation. They may never silently override it.

Before adjudicating a material action, the GM reads the Save Index and the smallest complete set of relevant Player, Relationship, Species, Location, Project, Research, Timeline, and other owning records needed for the claim.

## Campaign State Graph

The **Campaign State Graph** is the connected set of current authoritative records and Typed References describing what is established now, what remains pending, and what history or rules make those records intelligible.

It is a graph because current facts depend on other facts:

- an incarnation references one Soul and one current body;
- a body references species, form, condition, Development, Skills, location, and effects;
- a possession references an item identity, custody, location, and provenance;
- a relationship references participants and Historical encounters;
- a project references actors, resources, knowledge, location, methods, and time;
- a world condition references scopes, causes, trends, and Pending Consequences;
- a Knowledge claim references an observer and source;
- a current value references the event or process that changed it.

No single summary can replace this network when those dependencies matter.

## State, History, and Session

Three representations must remain distinct.

| Representation | Question | Authority and lifetime |
| --- | --- | --- |
| **Historical Record** | What established events and corrections occurred? | Historical authority and level |
| **Confirmed Current State** | What is established now after the last validated integration? | Current Campaign State authority, usually Campaign level |
| **Pending Session State** | What valid changes have occurred since that integration? | Current Session authority and Session level |

The current state is not a complete history. History is not a substitute for a usable current state. Pending Session changes are neither mere drafts nor fully integrated state.

When loading during an open Session, the effective working state is:

> Confirmed Current State + valid Pending Session Deltas - superseded working assumptions

This expression is conceptual, not numerical. Every applied delta still follows its own owner, chronology, authority, and uncertainty.

## Campaign State Claim

A **Campaign State Claim** is one exact assertion about a current campaign subject.

Each material claim records:

- **Claim ID**;
- **Subject ID**;
- **Property or relationship**;
- **Current value, condition, status, or explicit unknown**;
- **effective time and scope**;
- **Authoritative Record Owner**;
- **canonical system owner** where mechanics are involved;
- **truth layer**;
- **Persistence Level**;
- **source event, adjudication, decision, or migration**;
- **last confirmed integration point**;
- **dependencies and Typed References**;
- **visibility and observer boundaries**;
- **certainty, estimate, dispute, or Record Gap status**;
- **pending consequences or review conditions**;
- **supersession and correction references**.

The model does not require one physical record per claim. An implementation may group related claims, but it must preserve claim-level ownership and provenance where conflict or update is possible.

## State Status

Every material current-state record uses an explicit **State Status**.

| Status | Meaning |
| --- | --- |
| **Confirmed** | Integrated and validated at the current Save Point |
| **Pending** | Established during the Current Session but not yet integrated |
| **Stale** | Known not to include a later established change |
| **Disputed** | Supported incompatible claims remain unresolved |
| **Unknown** | No supported current value is established |
| **Estimated** | A bounded current inference with recorded basis |
| **Requires Source Recovery** | A material source may exist but cannot currently be loaded |
| **Superseded** | A newer valid claim or correction now governs |
| **Archived** | No longer active but retained for identity, history, or dependency |

`Pending` is not weaker fiction than `Confirmed`; it is less integrated. `Stale` is not authority to guess. `Unknown` is not zero, absent, false, or safe.

## Authoritative Module Map

The [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md) owns the module boundaries summarized here.

| State family | Authoritative module or interface |
| --- | --- |
| Campaign identity, active rules, versions, last save, module registry | Save Index and Campaign Canon |
| Player control, current actor, embodiment, access, condition, current objectives | Player State |
| Persistent Soul identity and incarnation links | Souls and Incarnations |
| Recurring accompanying actors and party role | Companions |
| NPC and other actor continuity | Actors |
| Persistent autonomous identity, autonomy, Controllers, model and memory lineage, assignments, networks, and last-confirmed state | Autonomous Registry |
| Trust, hostility, promises, debts, bonds, dependencies, shared history | Relationships |
| Species references, forms, populations, known routes, local uncertainty | Species |
| World domains, Ages, regions, pressures, active processes | World |
| Places, boundaries, routes, occupants, claims, conditions | Locations |
| Factions, institutions, Versions, participants, activities | Factions and Institutions |
| Current magical Profiles, sources, access, effects, infrastructure, restrictions | Magic |
| Maintained systems, dependencies, condition, access, function | Infrastructure |
| Possessions, custody, quantity, condition, location, claims, provenance | Inventory and Custody |
| Questions, methods, evidence, theories, confidence | Research |
| Observer-specific facts, beliefs, current conscious recall, rumours, unknowns | Knowledge |
| Cross-incarnation autobiographical Memory identity, associations, reinforcement, and accessibility history | Memory Continuity, with current recall manifested through Knowledge |
| Protected information and disclosure metadata | Secrets |
| Active undertakings, methods, resources, milestones, interruptions | Projects |
| Chronological placement and parallel time | Timeline |
| Established event history and corrections | Campaign History |
| Unresolved questions and discovery routes | Mysteries |
| Validation results and repair status | Validation |
| Backups, migrations, manifests, conflicts | Migration History |

A quest is not a separate universal mechanic. A request, commitment, objective, opportunity, obligation, Project, Mystery, or faction plan is represented by the module that owns its actual function.

## Required Read Discipline

The [FR-011 Context Assembly Layer](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) operationalizes this existing discipline through relevance selection, stable source navigation, freshness metadata, and a mandatory pre-resolution read gate. It does not replace or weaken the owner map below.

The GM must establish a **Read Set** before materially adjudicating or updating campaign state.

### Always Read

For every material interaction:

1. **Save Index** - Campaign ID, active Repository Version, Campaign Version, last Save Point, module registry, pending Session status, validation warnings, and open migration state.
2. **Current Session** - player intent, prior Session Deltas, unresolved adjudications, and corrections since the baseline.
3. **Canonical owner** - the repository rule and accepted governance for the claimed effect.
4. **Current time and location** - including any unresolved temporal or location conflict.

### Read When Relevant

| Claim involves | Required records |
| --- | --- |
| Player-controlled action | Player State, current Incarnation, embodiment, Character Knowledge, current location |
| Bodily capability or harm | body condition, Species, Development, Skills, active effects, equipment, environment |
| Soul access or Reincarnation | Soul and Incarnation records, Final Death or Interlife state, relevant Soul systems, Timeline, world candidate context |
| Another person or companion | Actor or Companion, Relationships, Observer View, location, current activity, commitments |
| Autonomous entity or system | Autonomous Registry, Model reference, Controller assignments, authoritative placement and condition, current assignment, last Confirmed Report, and only relevant Relationship, Network, capability, resource, maintenance, Infrastructure, Perspective, Knowledge, or Timeline records |
| Social response | Relationships, actor Knowledge, relevant faction or institution, authority, reputation, Soul Titles where perceivable |
| Monster form or behavior | Species, form, ecology, Evolution history, Development, Skills, Knowledge, location |
| Item use or transfer | Inventory and Custody, item or Weapon Soul identity, condition, access, ownership or claim, location |
| Magic | Magic Profile, Mana context, Affinity, procedure, source, cost, active effect, environment, relevant Knowledge |
| Place or travel | Locations, routes, access, Infrastructure, current world conditions, Timeline |
| Project or Time Skip | Projects, participants, methods, resources, Standing Instructions, interruptions, Timeline, world processes |
| Research or discovery | Research, Knowledge, Species or subject record, sources, prior evidence, location, Timeline |
| Mystery or secret | Mystery, authorized Secrets view, relevant Knowledge, evidence, disclosure constraints |
| Faction or institution | faction Version, participants, information routes, interests, capacity, Relationships, current actions |
| World change | World, affected domain records, Causal Event Chains, Simulation Frame, Pending Consequences, Timeline |
| Historical claim | Timeline, Campaign History, sources, corrections, observer records, current dependents |
| Numerical change | prior value, owning mechanic, source event, delta, costs, constraints, validation history |

### Dependency Closure

The initial Read Set expands along every material Typed Reference. A project record that depends on a missing laboratory, reagent supply, researcher, permission, or spell procedure is not ready for resolution merely because the Project itself loaded.

Stop expansion when additional records cannot change the current claim, consequences, uncertainty, or player choice materially. This prevents both under-loading and performative loading of the entire campaign.

## Pre-Adjudication State Check

Before resolving a material action, the GM confirms:

1. campaign and rules versions are known;
2. no migration or failed validation blocks the claim;
3. baseline and Pending Session state are separated;
4. actor identity, current embodiment, location, and time resolve;
5. relevant Relationships and Character Knowledge are loaded;
6. required Species, Project, Research, and Timeline records are loaded as appropriate;
7. Inventory, infrastructure, magic, and world dependencies resolve where relevant;
8. the exact canonical owner is identified;
9. numerical inputs have provenance;
10. unknowns and disputes are explicit;
11. protected information is available only to authorized views;
12. the player's intent is clear enough for the stakes.

If a missing record is material, use `Requires Source Recovery`, narrow the claim, ask for clarification, or defer. Do not improvise a value to keep the scene moving.

## Numerical State

Numbers change only through mechanically justified events.

Every material numerical change records a **Numerical Change Trace**:

- subject and quantity;
- prior confirmed value or explicit unknown;
- Pending changes already applied;
- exact delta or replacement;
- source event and time;
- canonical mechanic and owner;
- inputs, costs, limits, and rounding where applicable;
- resulting value;
- affected dependents;
- validator result.

The persistence system never:

- invents a starting value because a field expects one;
- silently rebalance a value;
- infer Stat XP, Skill XP, health, Mana, money, inventory, population, price, trust, or time from narrative tone;
- apply one delta twice because it appears in both Session and history;
- smooth conflicting records into an average;
- convert a qualitative rule into a number without authority;
- treat an absent number as zero.

When a source value is unknown, preserve `Unknown`, `Estimated`, or `Requires Source Recovery` and record the basis of any bounded operational estimate.

## Qualitative State

Qualitative claims require provenance too.

Terms such as `wounded`, `hostile`, `unstable`, `trusted`, `scarce`, `accessible`, `dormant`, or `confirmed` must identify:

- the subject and scope;
- the owning system;
- the evidence or event;
- the observer where relative;
- effective conditions;
- what would change or review the status.

Qualitative state is not permission for arbitrary GM interpretation. It avoids false precision while remaining testable and contextual.

## Unknown and Incomplete State

Missing information remains unknown.

Use:

- `Unknown` when no supported value exists;
- `Not Yet Verified` when a claim awaits confirmation;
- `Estimated` when a bounded inference is operationally necessary;
- `Requires Source Recovery` when evidence may exist outside the loaded record;
- `Disputed` when supported accounts conflict;
- `Player Theory`, `Rumour`, or `Research` when that layer owns the claim;
- `Open Detail` only under [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md) when its earlier value was immaterial.

An unknown current owner, location, item count, relationship state, species trait, date, or project result is not filled by genre expectation or the most likely answer unless a valid resolution method establishes it prospectively.

## State Transitions

A **State Transition** connects one valid state to another through an owned event or process.

Every material transition identifies:

1. prior state;
2. initiating cause or actor intent;
3. canonical owner;
4. relevant conditions and opposition;
5. established Immediate Outcome;
6. direct state changes;
7. costs, traces, Knowledge changes, and affected relationships;
8. Historical event reference;
9. Pending Consequences and downstream handoffs;
10. new current state after integration.

State does not change merely because time passed. Time allows processes and actors to act through [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md); those established transitions update state.

## Session Delta

A **Session Delta** is the claim-bounded set of established changes since the last Save Point.

Each delta separates:

- new records;
- updated claims;
- closed or archived claims;
- Historical events to append;
- Knowledge and visibility changes;
- Numerical Change Traces;
- new Projects, Mysteries, Research, or Relationships;
- disproved or superseded claims;
- unresolved questions;
- Pending Consequences;
- validation requirements.

A delta references owners rather than rewriting whole modules. The [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md) defines integration order and confirmation.

## Save Point and Snapshot

A **Save Point** is the last fully integrated and validated campaign version identified by the Save Index.

A **State Snapshot** is a read-consistent representation of the Campaign State Graph at one Save Point or explicit pending boundary. It is useful for backup, load, comparison, and migration.

A Snapshot is not a second owner of its facts. If extracted views conflict with the authoritative records included in the same Save Point, validation fails. A Snapshot cannot become current merely by being newer on disk.

## Identity and Reincarnation

Campaign state distinguishes:

- player control identity;
- Soul identity;
- Incarnation identity;
- body identity and form;
- legal and social identities;
- aliases;
- Relationship participants;
- historical identity claims.

Final Death closes current body state and routes consequences. Reincarnation creates a new Incarnation and body link after valid candidate resolution. It does not overwrite the former life, transfer Inventory or standing, pool Knowledge, or expose all Soul records as current access.

The Player State may point to the new Incarnation while Campaign History and Soul records preserve prior lives.

## Relationships and Known Characters

When an actor has a prior relationship record, the GM loads it before portraying a new encounter.

A known character is not treated as a stranger merely because:

- the last meeting occurred in another chat;
- the actor changed clothing, title, location, or affiliation;
- the player character Reincarnated;
- a name is absent from recent conversation context;
- a summary omitted the relationship.

Recognition may still fail through valid disguise, memory limits, identity uncertainty, changed embodiment, or lack of information. That failure requires an in-world explanation and its owning rules.

The [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md) owns relationship dimensions and change.

## Species and Forms

When a campaign adopts reusable design, the state record may reference its Codex Stable ID and Codex Version plus any explicit Campaign Divergence. That reference does not import the Codex database into Campaign State, and a later Codex revision does not silently change the adopted campaign form.

Actual individuals store their Carried Lineages, Expressed Lineages, Species Expression, Evolution Expression, Evolutionary Potential, inherited Level 0 Instincts, Ancestral Echo manifestations, contributors, genealogy, and outcome provenance in campaign state. Reusable Lineage Templates and Inheritance Profiles remain in the separate Living Codex.

Species state separates:

- Species Reference;
- current individual form;
- Species Stage;
- Mutation or hybridization;
- known and hidden Evolution Routes;
- local population and ecology;
- character or researcher Knowledge;
- former incarnations and Retained Instincts.

A known species label does not grant complete anatomy, Skill Trees, routes, or weaknesses. The GM reads the individual and species records appropriate to the current claim.

## Projects and Infrastructure

Projects and infrastructure retain:

- objective and current state;
- participants and authority;
- methods and Skills;
- resources and supply;
- dependencies and access;
- elapsed work and feedback;
- interruptions;
- maintenance;
- outputs;
- uncertainty;
- world consequences.

Time, narration, or a checkbox does not finish a project. Every result must follow its specialist owner and established State Transitions.

## Parallel and Off-Screen State

The Campaign State Graph supports multiple actors, regions, projects, lives, and processes advancing in parallel.

The GM preserves:

- independent clocks where needed;
- communication delays;
- actor-specific Knowledge;
- concurrent Session or Simulation branches only when the campaign procedure supports them;
- later convergence through explicit temporal and causal links;
- unresolved off-screen outcomes until a valid method settles them.

The player character's current scene does not freeze or automatically reveal other state.

## Time Skips, Ages, and World Resets

A Time Skip, Age Transition, or World Reset does not authorize one bulk replacement of Campaign State.

Use the relevant procedures to:

- identify starting state;
- advance each material process through its owner;
- preserve Player Agency Checkpoints;
- append Historical events;
- update current records by affected scope;
- retain surviving identities, relationships, infrastructure, knowledge, and consequences;
- classify unknown or disputed results;
- produce a Causal Bridge;
- validate the new Save Point.

Unaffected records remain unchanged. Changed storage scope does not imply changed world scope.

## Provisional Rules

Provisional Rules are Campaign Canon control records with explicit owner, scope, dependencies, status, expiry, and review under [Alpha Playtest Rules](../gm/ALPHA_PLAYTEST_RULES.md).

State produced by a valid Provisional Rule remains established campaign fact under the active rules profile. Later repository adoption requires migration; it does not quietly erase prior outcomes.

## Worked Examples

### A Door in Recent Narration

Conversation says a sealed archive door is open. Location state says it remains sealed, and no Session event records opening it.

The GM treats the prose as a narration error, not a state update. If the player intended to open it, adjudicate that action now from the actual lock, magic, access, Knowledge, and equipment records.

### A Missing Coin Total

Inventory records purchases but the latest coin total is absent. The GM does not invent a plausible balance. Mark it `Requires Source Recovery`, audit the prior confirmed total and transaction events, then reconstruct a Numerical Change Trace or agree an authorized correction.

### Meeting a Former Ally After Reincarnation

The ally's Actor and Relationship records preserve prior meetings, promises, grief, and current Knowledge. The new Incarnation may not remember or be recognizable. The ally's reaction depends on evidence, beliefs, Soul-related recognition routes, and present circumstances.

Neither side is treated as a stranger by default, and neither receives automatic recognition.

### Research During a Time Skip

The Project records methods, resources, participants, and Standing Instructions. Research records observations and theories. Timeline records elapsed intervals. World state supplies laboratory conditions and interruptions.

The GM advances supported work through owner-specific transitions. Elapsed years alone do not create confirmed knowledge or a fixed numerical yield.

### Parallel Gate Closure

A World Gate closes while the player is elsewhere. The World Event and Timeline establish closure, Locations update both endpoints, world contact and migration routes change, affected Projects and factions receive only causally available information, and the current incarnation's options update when relevant.

The event need not appear in the player's immediate narration to be Campaign Canon.

## Safeguards

- The structured Campaign State Graph is authoritative over conversation context.
- Every material State Claim has one record owner and source provenance.
- The GM loads the Save Index and Dependency Closure before adjudication.
- Current state, history, and Pending Session changes remain separate.
- Numbers change only through mechanically justified, traceable events.
- Missing numbers and facts never default to zero or convenience.
- Qualitative claims retain owner, evidence, scope, and review conditions.
- A State Snapshot is a read view, not a competing owner.
- Reincarnation creates new embodiment links rather than overwriting former lives.
- Known actors and relationships never reset silently.
- Time Skips and Resets update affected records through causes, not bulk replacement.
- Parallel world state does not freeze around player focus.
- State records do not grant mechanics, agency, Knowledge, progression, or success.
- No populated Campaign State Graph, Snapshot, or Session Delta belongs in this repository.

## Entity, Control, and View State

Where continuity requires them, the graph records an Entity's stable identity separately from current Controller assignment and active Perspective. Controller reassignment does not replace the Entity record. A Perspective reference does not imply physical co-location, control, or global knowledge. Reincarnation links Soul continuity to successive embodiments without overwriting prior-life entities or bodies.

Read Sets include relevant Entity, Controller, Perspective, and knowledge records when they materially affect adjudication. Conversation context may supplement retrieval but cannot silently change any of them.

## Scope Boundaries

This document defines the state graph, claim contract, status model, Read Sets, numerical provenance, unknown handling, transitions, deltas, and snapshots. It does not fully define Relationship memory, Research progression, chronology, migration, continuity repair, save integration, validation algorithms, or templates.

## Related Documents

- [Life Archive and Old-Soul Indexing](LIFE_ARCHIVE.md)

- [Campaign Persistence Engine Index](README.md)
- [Campaign Persistence Philosophy](CAMPAIGN_PERSISTENCE_PHILOSOPHY.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Persistence Levels](PERSISTENCE_LEVELS.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Game Master Responsibilities](../gm/GM_RESPONSIBILITIES.md)
- [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md)
- [World-State Variables](../world-engine/WORLD_STATE_VARIABLES.md)
- [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
- [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
