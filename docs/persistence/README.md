# Campaign Persistence Engine

The Campaign Persistence Engine defines how an Eternal Cycle campaign preserves established reality across interactions, sessions, deaths, Reincarnations, Time Skips, Age Transitions, World Resets, rules revisions, and changes of Game Master or storage technology.

It is a canonical game system, not a populated campaign, save-file format, storage product, or substitute for the systems whose outcomes it records.

## Canonical Reading Order

1. [Campaign Persistence Philosophy](CAMPAIGN_PERSISTENCE_PHILOSOPHY.md) establishes why persistence exists, what it owns, and how it relates to the World Engine and GM Toolkit.
2. [Persistence Authority](PERSISTENCE_AUTHORITY.md) defines the authority chain from Repository Canon through Current Narration, conflict routing, explicit correction, and the separation of authority from visibility, certainty, and agency.
3. [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md) defines storage-neutral modules, record contracts, stable identities, typed references, ownership, indexes, partitioning, and dependency closure.
4. [Truth Layers](TRUTH_LAYERS.md) separates Repository Canon, Campaign Canon, Historical Record, Character Knowledge, Research, Player Theories, Rumours, GM Secrets, and Meta with explicit ownership, visibility, update, promotion, and migration rules.
5. [Persistence Levels](PERSISTENCE_LEVELS.md) defines Repository, Soul, Historical, Campaign, Session, and Ephemeral lifetimes, ownership, deletion, migration, promotion, and archival boundaries.
6. [Campaign State Model](CAMPAIGN_STATE_MODEL.md) defines the authoritative current-state graph, claim contract, required Read Sets, Session Deltas, numerical provenance, unknown handling, and Snapshot boundaries.
7. [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md) defines persistent participant identities, first and latest meetings, important encounters, directional dimensions, trust trajectories, commitments, recognition, and causal relationship change.

Later Phase 10 documents will define research, chronology, migration, continuity correction, update procedures, validation, and repository-wide integration in roadmap order.

## Foundational Boundary

- The **World Engine** simulates reality.
- The **Campaign Persistence Engine** remembers established reality.
- The **GM Toolkit** reads and updates that memory while applying canonical rules.
- Specialist systems remain authoritative for the mechanics and outcomes they own.
- Populated campaign records remain outside this repository.

## Authority and Scope

Files in this section contain playable canonical rules. Accepted governance remains in [Design Decisions](../../design/DECISIONS.md), canonical vocabulary remains in [Terminology](../../design/TERMINOLOGY.md), and implementation order remains controlled by the [Roadmap](../../design/ROADMAP.md). Any conflict must be resolved before an affected task can be complete.
