# Canonical Data Ownership

## Purpose

This document defines the authoritative logical ownership of mutable campaign facts. It makes the [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md) enforceable across storage implementations without prescribing one physical schema.

The governing invariant is:

> One mutable canonical fact has exactly one authoritative logical owner.

Other records may reference, index, derive, cache, summarize, or preserve history from that owner. They do not become another current-state authority.

## Document Control

- **Owner:** canonical campaign-data ownership, identity-anchor boundaries, current-state duplication rules, and ownership classification
- **Primary authorities:** [Persistence Authority](PERSISTENCE_AUTHORITY.md), [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md), and [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- **Dependencies:** stable Record Identity, Typed References, Truth Layers, Persistence Levels, and the [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
- **Extensions:** storage schemas, adapters, migrations, templates, and future Phase 12 objectives may implement or consume this map without changing its ownership
- **Consumers:** Save Updates, migrations, persistence validation, GM procedures, AI runtimes, campaign schemas, and derived interfaces
- **Repository boundary:** no populated identity, campaign fact, database, migration result, or deployment locator belongs here

## Ownership Classes

| Classification | Meaning | May be edited as current Canon? |
|---|---|---|
| **Authoritative** | The one logical record or relation that owns a mutable current fact. | Yes, only through its owner and an authorized transaction. |
| **Reference** | A stable typed ID pointing to an authoritative record. | The reference may change through its own owner; it cannot rewrite the target. |
| **Derived** | A value calculated or assembled from authoritative records. | No. Regenerate or repair it from its sources. |
| **Cache** | Stored derived data retained for efficient retrieval with source version and freshness evidence. | No. Invalidate or rebuild it when sources change. |
| **Historical Snapshot** | What was established at a specific past event, interval, transaction, or version. | No as current state. Corrections preserve provenance and supersession. |

These are logical classifications. A storage implementation need not add a classification column to every table when ownership is already unambiguous in schema and documentation.

## Stable Identity and Role Records

### Non-Autonomous Entity Identity Anchor

Every persistent non-autonomous creature or character whose individual continuity matters has one stable Entity identity anchor. This includes a player-character Entity and may include companions, NPCs, recurring creatures, allies, enemies, sapient beings, and materially encountered non-autonomous creatures.

Player State, Companions, and Actors may remain separate logical modules or physical tables. When they describe the same persistent being, they must reference one Entity identity rather than create independent identities. Role, control mode, display name, current body, allegiance, and location may change without replacing that anchor.

Anonymous background beings may remain aggregates or transient records until individual continuity matters. Promotion to persistent identity requires provenance and must not duplicate a previously established individual.

### Player, Controller, and Entity

The player is external to the fiction and is not a character row merely because they control a character. A player-character Entity is the persistent in-world subject. Controller assignments and Perspectives are separate scoped relations. Changing Controller or Perspective does not replace Entity identity.

### Autonomous Reservation

Persistent independently operating entities and systems belong to the [Autonomous Registry](AUTONOMOUS_REGISTRY.md). FR-012 reserves that ownership domain, and FR-014 defines its storage-neutral contract without creating populated campaign records.

An ordinary Entity record may reference an autonomous subject when interaction requires it, but cannot become a second authoritative identity. If an Entity crosses the autonomous boundary, a controlled migration or reclassification preserves its stable identity and retires the former category assignment. A category change must never leave two canonical identities for one continuing Entity.

## Canonical Ownership Map

| Fact family | Authoritative logical owner | Referencing or derived consumers | Boundary |
|---|---|---|---|
| Campaign identity, active versions, module registry, and save activation | Save Index and Campaign Canon | adapters, manifests, dashboards | Deployment metadata does not decide Canon. |
| Persistent non-autonomous character or creature identity | Entity identity anchor exposed through Player State, Companions, or Actors | Relationships, Skills, locations, factions, Timeline, views | One being has one anchor; roles are not duplicate identities. |
| Player control and decision scope | Controller or Player State control relation | character views, session interface | Player is not the Entity. |
| Soul and Incarnation continuity | Souls and Incarnations | Entity anchor, Relationships, Timeline, Soul views | A new body or life does not overwrite an older Incarnation. |
| Stable Life identity and finalized per-incarnation historical summary | Life Archive | Soul Overview, Life Over view, future context and summary systems | Life Summary is a Historical Snapshot and index, not current-state authority. |
| Cross-incarnation autobiographical Memory identity and accessibility history | Memory Continuity | Character Knowledge, Life Archive, Soul views, Context Assembly | Current conscious recall belongs to the receiving incarnation's Knowledge record; source facts remain with their owners. |
| Soul-Bound Companion Bond identity, pair membership, formation provenance, and current convergence obligation | Soul/fate bond domain | Reincarnation, World Engine, Life Archive, Relationships, Timeline, Context Assembly | Current Relationships, recognition, routes, embodiments, and encounter events remain with their specialist owners. |
| Current embodiment, species, form, and lineage expression | Current Incarnation or embodiment relation | Entity view, capability views, Species references | References reusable definitions; does not own species design. |
| Subject-specific persistent visual traits | [Visual Identity](VISUAL_IDENTITY.md) scoped to the valid body, Incarnation, Entity, Autonomous Individual, or Model | character views, autonomous views, visual context, historical indexes | Sparse traits only; Species/form, Model, equipment, condition, Location, and rendering choices remain with their owners. |
| Current attributes and Development | relevant Development records | character sheets, summaries, adjudication packets | No copied mutable totals in identity rows. |
| Retained cross-Life source history | Soul continuity plus source Development/Skill records | Life Archive indexes, relevance assessments | Historical evidence is not current capability. |
| Effective retained acceleration | Derived retained-development profile | player view, adjudication packet | Recalculable Cache/Derived state; never owns source history or current values. |
| Current Skills, Development, and established scope | Skill records and Development interfaces | character sheets, species routes, class views | Consolidation supersedes redundant current records through stable lineage; predecessor and merge events remain Historical rather than competing current owners. |
| Current physical location of an Entity | the Entity's typed placement relation or its owning subject-state module | Location occupant index, scenes, encounters, summaries | Location occupant lists are Derived Views unless the schema designates the placement relation there as the sole owner. |
| Relationship identity and current dimensions | Relationships | character, companion, faction, scene, and summary views | Milestones may also be Historical events; current trust or hostility is not copied. |
| Item identity, quantity, condition, custody, and item placement | Inventory and Custody | character equipment view, locations, projects, Timeline | Carried/equipped lists are references or Derived Views. |
| Current bodily conditions, injuries, and effects | condition/effect records linked to current embodiment | character and scene views | Historical injuries remain event facts after current effects end. |
| Faction and Institution identity, membership, authority, and current activity | Factions and Institutions | Entity, Relationship, Location, and summary views | Reputation or relationship claims route to their specialist owner. |
| Project identity, participation, dependencies, work, and state | Projects | Entity, faction, infrastructure, Research, summaries | A participant list is a typed relation, not copied project state. |
| Research questions, evidence, theories, confidence, and confirmation | Research | Knowledge, Entity, Project, and summary views | Confirmed world facts reference their factual owner. |
| Location identity, boundaries, environment, hazards, access, claims, and place condition | Locations | placement relations, factions, projects, infrastructure, maps | Occupancy derived from subject placement must not compete with it. |
| Campaign-specific Species references, observed forms, populations, and divergences | campaign Species module | individuals, Research, ecology, Evolution, views | Reusable Species design remains in Repository Canon or Living Codex. |
| Reusable Living Codex Species and Evolution definitions | separate GM Living Codex database | campaign Species references and adopted configurations | Never owns an individual's mutable campaign state. |
| Infrastructure identity, current condition, capacity, dependencies, and operation | Infrastructure | Locations, Projects, factions, world state, summaries | Autonomous infrastructure later routes identity and autonomy through FR-014. |
| Event occurrence, chronology, historical state changes, and corrections | Timeline and Campaign History | all current-state domains and summaries | Historical facts do not compete with current state. |
| Current world truth and campaign facts | owning Campaign State domain under Campaign Canon | Knowledge, Research, Secrets, narration | Canon is claim-specific, not one duplicate world-fact blob. |
| Character Knowledge and belief | Knowledge records scoped to the knowing Entity | Perspective-filtered interfaces, Research, narration | Does not rewrite world truth. |
| GM Secrets and protected preparation | Secrets with references to factual owners and access rules | authorized GM views only | A secret is not a second world truth. |
| Persistent autonomous entity or system state | Autonomous Registry | Entities, Controllers, locations, networks, views | FR-014 owns autonomy-specific identity and state without duplicating specialist domains. |
| Scene packets, dashboards, summaries, indexes, and retrieval aids | no canonical owner; they are Derived or Cache records | GM and player interfaces | Source IDs, source version, freshness, and audience are required. |
| Canonical Visual Context and representation-tool prompts | no canonical owner; they are Derived Context Packets | image generation and other authorized visual representation tools | Perspective-filtered assembly never promotes rendering choices into Canon. |

## Reference Rules

Cross-domain use relies on stable typed references:

```text
Entity identity
  -> embodiment relation -> Species or form reference
  -> placement relation -> Location identity
  -> participant relation -> Relationship identity
  -> custody relation -> Item identity
```

A reference stores the target ID, relation type, scope, and provenance needed for integrity. It does not copy mutable names, descriptions, relationship values, coordinates, species traits, or current conditions as authoritative fields.

Visual representations follow the same rule. A Current Appearance projection references Species/form, Model, Visual Identity, equipment, condition, Location, and environment owners. It does not copy those facts into one competing appearance record.

Human-readable labels may accompany references for diagnostics or display only when explicitly Derived or cached. A label mismatch never changes target identity.

## Current Location Invariant

Each locatable subject has one authoritative current-placement claim for a given scope and time. Storage may place that relation with the subject, in a normalized placement table, or under Locations, but the implementation must designate exactly one owner.

- Location occupant lists are derived from authoritative placement relations unless they are the schema's sole placement owner.
- Scene summaries and later FR-011 context packets repeat placement non-authoritatively.
- Projects and Infrastructure may reference where work occurs but do not relocate participants.
- Timeline Events preserve where a subject was when an event occurred as Historical Snapshots.
- Unknown, approximate, distributed, disputed, or multi-site placement remains explicit rather than being forced into a false coordinate.

## Relationship Invariant

Relationships owns current relationship identity, directional dimensions, commitments, trajectory, and unresolved issues. Character rows, companion views, faction sheets, and summaries reference or derive this state. Timeline and Campaign History may preserve relationship milestones as historical events without owning current trust, hostility, debt, recognition, or obligation.

## Species and Individual Boundary

An individual references campaign Species, form, lineage, and reusable Living Codex definitions through stable IDs and explicit campaign divergences. The Entity or character record owns neither reusable anatomy nor universal species capability. The Living Codex owns no individual's current body, Development, Skills, condition, location, relationship, or history.

## Historical and Derived Repetition

Anti-duplication does not erase valid history.

- An event may record that an Entity occupied a Location at occurrence time.
- A migration manifest may record source and target values.
- An audit may preserve the defect it observed.
- A future Life Summary may index one completed Incarnation.
- A [Long-Horizon Summary](LONG_HORIZON_SUMMARIES.md) may compress established changes across an interval.

Historical Snapshots retain time, source, authority, and supersession. Derived summaries retain source IDs, source version, freshness, scope, and audience. Neither accepts direct edits as a shortcut to changing current state.

## Storage and SQLite Requirements

A SQLite campaign implementation should enforce the ownership map with the least disruptive suitable mechanisms:

- one primary stable identity per persistent subject;
- foreign keys enabled for authoritative references;
- unique constraints where one active owner or relation is required;
- partial unique indexes where lifecycle state makes uniqueness conditional;
- check constraints for owner kinds and reference roles where useful;
- views for derived occupancy, sheets, summaries, and indexes;
- transactions covering the complete Affected Set;
- migration and read-only validation before activating schema changes.

SQLite views and caches do not become owners. SQLite has no native stored procedures; application queries or adapter procedures may route writes to owners.

This repository currently defines no executable campaign schema or populated save. FR-012 therefore requires no reusable schema migration. A concrete campaign implementation maps its existing tables to this ownership contract and uses the established [Migration and Versioning](MIGRATION_AND_VERSIONING.md) procedure if structural change is necessary.

## Ownership-Aware Save Procedure

For every changed fact:

1. identify the fact's authoritative logical owner;
2. resolve the stable subject and related-record IDs;
3. reject a write routed to a view, cache, summary, or non-owning module;
4. include the owner and every affected reference, index, and historical event in the Affected Set;
5. update the authoritative fact once;
6. append legitimate Historical records without replacing current state;
7. invalidate or regenerate affected Derived Views and caches;
8. validate identity, foreign references, owner uniqueness, and expected changes;
9. activate only the complete validated Save Point.

## Validation Requirements

Persistence validation checks, where the implementation exposes enough structure:

- every mutable canonical fact resolves to one declared owner;
- every persistent non-autonomous subject resolves to one Entity identity anchor;
- every Visual Identity resolves to one valid scoped subject, and Model and Individual visual ownership remain distinct;
- Player, Controller, Entity, and Perspective remain distinct;
- foreign keys and typed references resolve;
- duplicate persistent identities are reported for owner review, never merged automatically;
- current placement has no incompatible simultaneous authoritative claims;
- Relationship participants exist and current dimensions occur only under Relationships;
- individual Species and form references resolve without copying reusable definitions;
- Visual Identity contains no copied current equipment, temporary condition, complete Species/form definition, or automatically adopted generated-image detail;
- Derived and cache records identify source IDs, source version, freshness, scope, and audience;
- Historical Snapshots identify their time and do not govern current state;
- a subject is not active simultaneously under ordinary Entity ownership and the reserved autonomous domain;
- owner changes preserve identity, provenance, migration evidence, and rollback;
- unknown information remains unknown rather than being filled to satisfy a relation.

Conflicting ownership is a validation failure. Validation reports the owners and claims involved but does not pick a winner, merge identities, or invent a repair.

## Compatibility with Approved Phase 12 Work

- **FR-004:** [Life Summaries](LIFE_ARCHIVE.md) index and historically summarize owner records; they do not become current-state owners.
- **FR-010:** [Long-Horizon Summaries](LONG_HORIZON_SUMMARIES.md) preserve compressed established history and references; they cannot fabricate or override detailed Canon.
- **FR-011:** Context Assembly can follow stable IDs, owner declarations, and typed reference paths, while every context packet remains Derived or Cache data.
- **FR-014:** the [Autonomous Registry](AUTONOMOUS_REGISTRY.md) occupies the reserved, non-overlapping owner domain and preserves identity across category changes.

No mechanics or schemas for those objectives are implemented here.

## Safeguards

- Never create a second authoritative copy for convenience.
- Never treat a display label as identity.
- Never collapse Player, Controller, Perspective, Soul, Incarnation, body, and Entity identity.
- Never let a character record define its Species.
- Never store current relationship dimensions outside Relationships as authority.
- Never maintain independent current-location truths in both subject and place records.
- Never let a summary, index, cache, view, transcript, or narration overwrite its source.
- Never erase legitimate Historical Snapshots as duplication.
- Never force autonomous subjects into ordinary character ownership or duplicate their Registry identity.
- Never perform campaign migration inside this repository.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- [Visual Identity](VISUAL_IDENTITY.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md)
- [Timeline Engine](TIMELINE_ENGINE.md)
- [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](PERSISTENCE_VALIDATION.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
- [GM Living Codex Persistence Model](../gm-living-codex/PERSISTENCE_MODEL.md)
- [SQLite Database Format Adapter](adapters/SQLITE_DATABASE_FORMAT_ADAPTER.md)
- [Managed Data Service](MANAGED_DATA_SERVICE.md)
- [MCP Managed Service Interface](MCP_PERSISTENCE_MODE.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
