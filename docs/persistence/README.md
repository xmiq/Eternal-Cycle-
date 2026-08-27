# Campaign Persistence Engine

The Campaign Persistence Engine defines how an Eternal Cycle campaign preserves established reality across interactions, sessions, deaths, Reincarnations, Time Skips, Age Transitions, World Resets, rules revisions, and changes of Game Master or storage technology.

It is a canonical game system, not a populated campaign, save-file format, storage product, or substitute for the systems whose outcomes it records.

Repository-wide ownership, dependency, extension, and consumer metadata is maintained in the [Canonical Document Registry](../DOCUMENT_REGISTRY.md#campaign-persistence-engine).

## Canonical Reading Order

1. [Campaign Persistence Philosophy](CAMPAIGN_PERSISTENCE_PHILOSOPHY.md) establishes why persistence exists, what it owns, and how it relates to the World Engine and GM Toolkit.
2. [Persistence Authority](PERSISTENCE_AUTHORITY.md) defines the authority chain from Repository Canon through Current Narration, conflict routing, explicit correction, and the separation of authority from visibility, certainty, and agency.
3. [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md) defines storage-neutral modules, record contracts, stable identities, typed references, ownership, indexes, partitioning, and dependency closure.
4. [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md) defines one logical owner per mutable canonical fact, the persistent Entity identity anchor, normalized domain ownership, reference and historical boundaries, and the reserved Autonomous Registry domain.
5. [Life Archive and Old-Soul Indexing](LIFE_ARCHIVE.md) defines stable Life identity, Soul Overview indexing, finalized Life Summaries, Life Over views, and scalable historical retrieval.
   [Memory Continuity, Fading, and Recall](../soul/MEMORY_CONTINUITY.md) separately governs autobiographical persistence and incarnation-specific conscious access.
6. [Long-Horizon Simulation Summaries](LONG_HORIZON_SUMMARIES.md) defines stable Historical Period identity, causal compression, continuity hooks, historical retrieval, Time Skip and Interlife integration, filtered transition views, and non-owning current-state boundaries.
7. [Autonomous Registry](AUTONOMOUS_REGISTRY.md) defines persistent autonomous identity, models, Controller assignments, autonomy, groups, reconstruction, memory continuity, networks, last-confirmed state, migration, and validation.
8. [Visual Identity](VISUAL_IDENTITY.md) defines sparse established appearance facts, subject and Model ownership, historical recovery, reincarnation boundaries, and explicit Canon adoption without duplicating current equipment or conditions.
9. [Truth Layers](TRUTH_LAYERS.md) separates Repository Canon, Campaign Canon, Historical Record, Character Knowledge, Research, Player Theories, Rumours, GM Secrets, and Meta with explicit ownership, visibility, update, promotion, and migration rules.
10. [Persistence Levels](PERSISTENCE_LEVELS.md) defines Repository, Soul, Historical, Campaign, Session, and Ephemeral lifetimes, ownership, deletion, migration, promotion, and archival boundaries.
11. [Campaign State Model](CAMPAIGN_STATE_MODEL.md) defines the authoritative current-state graph, claim contract, required Read Sets, Session Deltas, numerical provenance, unknown handling, and Snapshot boundaries.
12. [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md) defines persistent participant identities, first and latest meetings, important encounters, directional dimensions, trust trajectories, commitments, recognition, and causal relationship change.
13. [Research Engine](RESEARCH_ENGINE.md) defines iterative Observation, Hypothesis, Experiment, Evidence, Theory, and Confirmed Knowledge with qualitative confidence, competing theories, loss, disproof, and Rediscovery.
14. [Timeline Engine](TIMELINE_ENGINE.md) defines stable events, temporal coordinates and precision, World and Campaign History, Session Logs, Personal and Soul chronologies, parallel ordering, Time Skips, Age boundaries, and source-preserving correction.
15. [Migration and Versioning](MIGRATION_AND_VERSIONING.md) defines Repository, Campaign, persistence-model, and storage versions plus the mandatory Backup, Audit, Merge, and Validation transaction, Migration Manifest, activation, interruption, rollback, and storage conversion.
16. [Continuity Resolution](CONTINUITY_RESOLUTION.md) defines narrow conflict containment, authority-ordered diagnosis, narration, save, uncertainty, retcon, and deception classifications, reliance review, source-preserving correction, and validated resumption.
17. [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md) defines the owner-routed, idempotent transaction after every completed gameplay interaction, including Affected Sets, Session Deltas, log and history appends, special record handling, atomic activation, interruption, and concurrency.
18. [Persistence Validation](PERSISTENCE_VALIDATION.md) defines immutable validation baselines, trigger-specific profiles, severity and outcomes, evidence-bearing findings, continuity defect detection, protected reporting, and owner-routed repair.
19. [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md) defines the first-class `DIRECT` and `MCP` modes, shared completion contract, configuration boundary, status evidence, and mode migration.
20. [Direct Persistence Mode](DIRECT_PERSISTENCE_MODE.md) defines runtime-operated Database Format and Storage Adapter composition, capability gates, and direct completion evidence.
21. [MCP Persistence Mode](MCP_PERSISTENCE_MODE.md) defines the semantic service boundary, Persistence Receipts, hidden backend, SQL Server reference implementation, and service-owned durability and recovery.
22. [Campaign Persistence Integration](CAMPAIGN_PERSISTENCE_INTEGRATION.md) defines the completed-system ownership matrix, operating cycle, specialist handoffs, cross-system state changes, correction routes, and blank-template boundary.

## Foundational Boundary

The [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md) defines how the state remembered here belongs to objective simulation while player-facing views remain Perspective-filtered. Entity identity, Controller assignment, Perspective, Character Knowledge, and GM Secrets remain separate claims.

The [GM Living Codex](../gm-living-codex/README.md) is an adjacent, separately persisted reusable-design authority. A campaign may reference an adopted Codex Version, stable entry ID, and explicit divergence, but Codex records never become Campaign State and campaign facts never rewrite the Codex automatically.

- The **World Engine** simulates reality.
- The **Campaign Persistence Engine** remembers established reality.
- The **GM Toolkit** reads and updates that memory while applying canonical rules.
- Specialist systems remain authoritative for the mechanics and outcomes they own.
- Populated campaign records remain outside this repository.

## Runtime Implementation Boundary

The Campaign Persistence Engine remains storage-neutral. The [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md) defines `DIRECT` and `MCP` implementations, while the [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md) defines how an AI GM, execution profile, selected mode, Campaign Configuration, Canonical Campaign State, and repository rules relate without changing persistence semantics.

[Context Assembly and Gameplay Turn Persistence](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) consumes this engine's Read Set, ownership, Save Update, validation, and adapter contracts to define one valid Gameplay Turn. It introduces no competing campaign owner or storage technology.

[Canonical Visual Context](../ai/CANONICAL_VISUAL_CONTEXT.md) is a purpose-specific Derived projection for image and representation handoffs. It reads Visual Identity and other current owners without becoming a new source of campaign truth.

Runtime-specific [execution profiles](../ai/README.md#runtime-specific-profiles) and persistence implementations are replaceable:

- an execution profile may order loading, adjudication, persistence, validation, and delivery but cannot define mechanics;
- a Direct Database Format or Storage Adapter may implement its bounded transaction, placement, backup, and Read-Back Validation role but cannot adjudicate gameplay;
- an MCP persistence service may implement semantic persistence and hide its backend but cannot adjudicate gameplay or expose raw SQL to the GM;
- external Campaign Configuration selects profile, Persistence Mode, and the Direct Adapter Chain or MCP service binding;
- campaign facts, discoveries, secrets, credentials, locators, and populated state remain outside universal repository documents.

Save-Before-Delivery is an execution-profile constraint, not fictional physics. Read-only validation observes a candidate or activated state without mutating it.

The [Direct Adapter Index](adapters/README.md) owns current SQLite, DuckDB, local-storage, and Google Drive adapter navigation. MCP clients use the separately documented service contract rather than wrapping the service in client-side storage adapters.

## Authority and Scope

Files in this section contain playable canonical rules. Accepted governance remains in [Design Decisions](../../design/DECISIONS.md), canonical vocabulary remains in [Terminology](../../design/TERMINOLOGY.md), and implementation order remains controlled by the [Roadmap](../../design/ROADMAP.md). Any conflict must be resolved before an affected task can be complete.
