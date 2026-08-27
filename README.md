# Eternal Cycle

**Eternal Cycle** is a rules-first fantasy role-playing and interactive-novel framework built around endless reincarnation, persistent soul progression, branching monster evolution, emergent Soul Weapons, and a causally simulated world.

## Repository Boundary

This repository is the canonical home of the game's rules and design framework. It contains:

- system rules;
- design decisions;
- GM procedures;
- reusable templates;
- development roadmaps;
- terminology and consistency guidance.

It does **not** contain campaign saves, active characters, live world state, inventories, quests, or playthrough history.

## Project Status

**Eternal Cycle v1.0.0 — Release 1. Status: Released.** Phases 0 through 12 are complete. Post-release evidence and owner-authorized evolution continue through **Phase 13 — Future Revisions**. FR-017 adds portable `DIRECT` and `MCP` persistence in post-v1 development without changing the historical v1.0.0 tag.

## Core Pillars

1. **Death changes the character without erasing the journey.** Bodies and worldly assets are temporary; soul development persists.
2. **Development is multidimensional.** There is no single universal level that fully represents a being's power.
3. **Monster lives matter.** Monsters possess cultures, societies, instincts, and branching evolutionary futures.
4. **Weapons may awaken.** A weapon or meaningful tool can develop a distinct Weapon Soul through identity-defining shared history, but no object is guaranteed to do so.
5. **The world reacts.** Ecology, politics, economics, war, disease, magic, and technology form causal chains.
6. **The story remains open-ended.** Civilizations and Ages may end, but the protagonist's soul continues.

## Rules Map

- [`RELEASE_NOTES.md`](RELEASE_NOTES.md) - Release 1 capabilities, compatibility guidance, and documented runtime boundary.
- [`RELEASE_MANIFEST.md`](RELEASE_MANIFEST.md) - version, release identity, validation, roadmap state, and artifact metadata.
- [`CHANGELOG.md`](CHANGELOG.md) - concise version history.
- [`design/README.md`](design/README.md) - design-governance reading order, authority boundaries, and repository audit map.
- [`docs/README.md`](docs/README.md) - complete map of canonical rules and section indexes.
- [`docs/DOCUMENT_REGISTRY.md`](docs/DOCUMENT_REGISTRY.md) - claim owners, dependencies, extensions, and consumers for every canonical document family.
- [`templates/README.md`](templates/README.md) - reusable blank record contracts, ownership boundaries, validation guidance, and complete [system coverage map](templates/TEMPLATE_COVERAGE.md).
- [`agents/README.md`](agents/README.md) - focused contributor roles subordinate to `AGENTS.md`.
- [`docs/core/README.md`](docs/core/README.md) - system-wide design constraints and Core reading order.
- [`docs/core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md`](docs/core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md) - three-layer rules, simulation, and player-interface architecture with Entity, Controller, Perspective, and knowledge boundaries.
- [`docs/soul/README.md`](docs/soul/README.md) - Soul Engine rules and reading order.
- [`docs/soul/MEMORY_CONTINUITY.md`](docs/soul/MEMORY_CONTINUITY.md) - cross-incarnation autobiographical continuity, fading, cues, fragments, and recall boundaries.
- [`docs/soul/SOUL_BOUND_COMPANIONS.md`](docs/soul/SOUL_BOUND_COMPANIONS.md) - pair-specific fate bonds that guarantee eventual reunion while preserving causal routes, memory boundaries, and current-life agency.
- [`docs/progression/README.md`](docs/progression/README.md) - Development and progression rules.
- [`docs/progression/RETAINED_CROSS_LIFE_DEVELOPMENT.md`](docs/progression/RETAINED_CROSS_LIFE_DEVELOPMENT.md) - bounded retained-development stacking and cross-embodiment Skill familiarity.
- [`docs/skills/README.md`](docs/skills/README.md) - Skill Engine rules and reading order.
- [`docs/skills/SKILL_CONSOLIDATION_AND_SCOPE.md`](docs/skills/SKILL_CONSOLIDATION_AND_SCOPE.md) - routine redundant-Skill consolidation, lineage preservation, and history-based scope interpretation.
- [`docs/monster-evolution/README.md`](docs/monster-evolution/README.md) - Monster Evolution reading order, adjudication sequence, ownership map, and safeguards.
- [`docs/human/README.md`](docs/human/README.md) - Human Class and Profession philosophy, rules, reading order, and ownership boundaries.
- [`docs/soul-weapons/README.md`](docs/soul-weapons/README.md) - complete Soul Weapon state model, reading order, claim resolution, system interfaces, and safeguards.
- [`docs/magic/README.md`](docs/magic/README.md) - complete Magic reading order, claim resolution, ownership boundaries, guarantees, and campaign-record boundary.
- [`docs/magic/MANA.md`](docs/magic/MANA.md) - Mana as the local, causal, world-side foundation of magical change.
- [`docs/magic/MAGICAL_AFFINITIES.md`](docs/magic/MAGICAL_AFFINITIES.md) - target-specific magical compatibility, formation, expression, conflict, and persistence.
- [`docs/magic/SPELL_FORMATION.md`](docs/magic/SPELL_FORMATION.md) - bounded spell procedures, local grammars, formation lifecycle, costs, failure, counterplay, and persistence.
- [`docs/magic/RITUALS.md`](docs/magic/RITUALS.md) - prepared magical structures, differentiated roles, substitution, integrity, interruption, and aftermath.
- [`docs/magic/ENCHANTING.md`](docs/magic/ENCHANTING.md) - host-anchored magical configurations, functions, layers, maintenance, Drift, personhood, and Soul Weapon boundaries.
- [`docs/magic/ALCHEMY.md`](docs/magic/ALCHEMY.md) - controlled magical material processes, Reagents, provenance, Batches, application, ecology, and anti-copying safeguards.
- [`docs/magic/DIVINE_MAGIC.md`](docs/magic/DIVINE_MAGIC.md) - source-bound divine relationships, Domains, Jurisdictions, Mandates, petitions, blessings, miracles, agency, and safeguards.
- [`docs/magic/FORBIDDEN_MAGIC.md`](docs/magic/FORBIDDEN_MAGIC.md) - scoped magical restrictions, hazards, authority, protected interests, controlled practice, knowledge, enforcement, review, and safeguards.
- [`docs/magic/WORLD_ENGINE_INTERACTIONS.md`](docs/magic/WORLD_ENGINE_INTERACTIONS.md) - causal handoff from established magical changes to bounded, persistent world consequences.
- [`docs/gm/README.md`](docs/gm/README.md) - Game Master rules, operating framework, and reading order.
- [`docs/gm/GM_RESPONSIBILITIES.md`](docs/gm/GM_RESPONSIBILITIES.md) - bounded GM duties, ownership handoffs, delegation, records, and human/AI parity.
- [`docs/gm/GAME_MASTER_FRAMEWORK.md`](docs/gm/GAME_MASTER_FRAMEWORK.md) - campaign procedure, authority, continuity, information boundaries, external records, and AI GM guidance.
- [`docs/gm/CAMPAIGN_BOOTSTRAP.md`](docs/gm/CAMPAIGN_BOOTSTRAP.md) - released-engine status, normal-default campaign creation, explicit testing modes, starting profiles, and resume compatibility.
- [`docs/gm-living-codex/README.md`](docs/gm-living-codex/README.md) - cross-campaign GM-approved reusable design assets, authority boundaries, consultation rules, and implementation plan.
- [`docs/gm-living-codex/LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md`](docs/gm-living-codex/LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md) - lineage templates, inherited expression, Mana equalization, Level 0 Instincts, and stabilization.
- [`docs/ai/README.md`](docs/ai/README.md) - AI runtime architecture, shared operating procedures, execution profiles, persistence adapters, and authority boundaries.
- [`docs/ai/AI_RUNTIME_MODEL.md`](docs/ai/AI_RUNTIME_MODEL.md) - implementation-neutral runtime layers, authority boundaries, transaction flow, adapter composition, failure boundaries, and extension rules.
- [`docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md`](docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) - FR-011 relevance-filtered canonical context, mandatory reads, automatic validated persistence, turn closure, and reset-resilient reload.
- [`docs/ai/CANONICAL_VISUAL_CONTEXT.md`](docs/ai/CANONICAL_VISUAL_CONTEXT.md) - purpose-specific canonical image context, observer filtering, Current Appearance assembly, and non-canonical rendering boundaries.
- [`docs/ai/AI_CAPABILITIES_AND_LIMITATIONS.md`](docs/ai/AI_CAPABILITIES_AND_LIMITATIONS.md) - operational limits for memory, context, tools, validation, information protection, numerical state, and failure handling.
- [`docs/ai/chatgpt/CHATGPT_GM_UNIVERSAL_INSTRUCTIONS.md`](docs/ai/chatgpt/CHATGPT_GM_UNIVERSAL_INSTRUCTIONS.md) - ChatGPT execution profile for Gameplay and Development Contexts, strict Save-Before-Delivery, correction, and failure behavior.
- [`docs/persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md`](docs/persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md) - post-v1 `DIRECT` and `MCP` persistence architecture and shared completion contract.
- [`docs/persistence/adapters/README.md`](docs/persistence/adapters/README.md) - runtime-neutral SQLite, DuckDB, local-storage, and Google Drive Direct Adapter contracts.
- [`services/eternal-cycle-mcp/README.md`](services/eternal-cycle-mcp/README.md) - executable Microsoft SQL Server-backed MCP persistence reference service.
- [`docs/gm/CONSEQUENCE_RESOLUTION.md`](docs/gm/CONSEQUENCE_RESOLUTION.md) - bounded resolution of immediate outcomes, costs, traces, affected subjects, responses, persistence, and causal handoffs.
- [`docs/gm/UNCERTAINTY_HANDLING.md`](docs/gm/UNCERTAINTY_HANDLING.md) - information views, uncertainty sources, evidence, deterministic and random resolution, deferral, fair secrecy, and correction.
- [`docs/gm/REINCARNATION_GENERATION.md`](docs/gm/REINCARNATION_GENERATION.md) - world-grounded candidate sourcing, eligibility, personhood, Reincarnation Modes, selection handoff, and pre-embodiment revalidation.
- [`docs/gm/ENCOUNTER_GENERATOR.md`](docs/gm/ENCOUNTER_GENERATOR.md) - causal encounter sourcing, eligibility, framing, non-scaling, agency routes, specialist handoffs, and external records.
- [`docs/gm/MONSTER_GENERATOR.md`](docs/gm/MONSTER_GENERATOR.md) - world-valid species sketches, individual histories, embodiment, ecology, capability, information, placement, and profile safeguards.
- [`docs/gm/NPC_GENERATOR.md`](docs/gm/NPC_GENERATOR.md) - person-basis validation, causal identity, actor-specific information, independent decisions, relationships, continuity, and external profiles.
- [`docs/gm/DUNGEON_GENERATOR.md`](docs/gm/DUNGEON_GENERATOR.md) - Dungeon Basis validation, causal topology, access, activity, inhabitants, resources, hazards, information, revalidation, and external profiles.
- [`docs/gm/FACTION_GENERATOR.md`](docs/gm/FACTION_GENERATOR.md) - coordination-basis validation, participants, interests, decision routes, information, capacity, continuity, versioning, and external profiles.
- [`docs/gm/WORLD_EVENT_GENERATOR.md`](docs/gm/WORLD_EVENT_GENERATOR.md) - causal event-basis validation, direct event boundaries, footprint, timing, uncertainty, handoffs, and external records.
- [`docs/gm/TIME_SKIP_PROCEDURE.md`](docs/gm/TIME_SKIP_PROCEDURE.md) - player-authorized compression, Standing Instructions, Review Points, agency pauses, return states, and causal bridges.
- [`docs/gm/AGE_TRANSITION_PROCEDURE.md`](docs/gm/AGE_TRANSITION_PROCEDURE.md) - evidence-backed Age classification, scoped boundaries, competing claims, targeted revalidation, and return to play.
- [`docs/gm/PROVISIONAL_RULINGS.md`](docs/gm/PROVISIONAL_RULINGS.md) - release-neutral campaign-local adjudication for narrow gaps in current Canon.
- [`docs/persistence/README.md`](docs/persistence/README.md) - Campaign Persistence Engine reading order, ownership boundary, and repository scope.
- [`docs/persistence/CAMPAIGN_PERSISTENCE_PHILOSOPHY.md`](docs/persistence/CAMPAIGN_PERSISTENCE_PHILOSOPHY.md) - persistence as causal memory rather than a storage format or variable snapshot.
- [`docs/persistence/PERSISTENCE_AUTHORITY.md`](docs/persistence/PERSISTENCE_AUTHORITY.md) - campaign-fact authority from Repository Canon through Current Narration.
- [`docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md`](docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md) - storage-neutral campaign modules, record ownership, identity, references, and dependency closure.
- [`docs/persistence/CANONICAL_DATA_OWNERSHIP.md`](docs/persistence/CANONICAL_DATA_OWNERSHIP.md) - authoritative ownership map for mutable campaign facts, identity anchors, normalized references, historical snapshots, and derived data.
- [`docs/persistence/LIFE_ARCHIVE.md`](docs/persistence/LIFE_ARCHIVE.md) - stable Life identities, player-visible Life Summaries, and scalable old-Soul historical retrieval.
- [`docs/persistence/AUTONOMOUS_REGISTRY.md`](docs/persistence/AUTONOMOUS_REGISTRY.md) - persistent autonomous identity, models, Controllers, groups, reconstruction, memory lineage, networks, uncertainty, migration, and validation.
- [`docs/persistence/VISUAL_IDENTITY.md`](docs/persistence/VISUAL_IDENTITY.md) - sparse canonical appearance traits, Model and Individual separation, historical recovery, and explicit adoption rules.
- [`docs/persistence/TRUTH_LAYERS.md`](docs/persistence/TRUTH_LAYERS.md) - canonical separation of facts, history, character knowledge, research, theories, rumours, secrets, and Meta.
- [`docs/persistence/PERSISTENCE_LEVELS.md`](docs/persistence/PERSISTENCE_LEVELS.md) - canonical lifetimes for Repository, Soul, Historical, Campaign, Session, and Ephemeral records.
- [`docs/persistence/CAMPAIGN_STATE_MODEL.md`](docs/persistence/CAMPAIGN_STATE_MODEL.md) - authoritative campaign state graph, claim provenance, read discipline, deltas, unknowns, and snapshots.
- [`docs/persistence/RELATIONSHIP_MEMORY_ENGINE.md`](docs/persistence/RELATIONSHIP_MEMORY_ENGINE.md) - multidimensional, directional, history-preserving relationship continuity.
- [`docs/persistence/RESEARCH_ENGINE.md`](docs/persistence/RESEARCH_ENGINE.md) - iterative evidence-based inquiry, confidence, confirmation, competing theories, and rediscovery.
- [`docs/persistence/TIMELINE_ENGINE.md`](docs/persistence/TIMELINE_ENGINE.md) - stable temporal identity, five distinct chronologies, parallel events, uncertain dating, and source-preserving correction.
- [`docs/persistence/MIGRATION_AND_VERSIONING.md`](docs/persistence/MIGRATION_AND_VERSIONING.md) - versioned Backup, Audit, Merge, Validation, activation, rollback, and storage-neutral conversion.
- [`docs/persistence/CONTINUITY_RESOLUTION.md`](docs/persistence/CONTINUITY_RESOLUTION.md) - bounded conflict diagnosis, authority resolution, reliance protection, correction, and resumption.
- [`docs/persistence/SAVE_UPDATE_PROTOCOL.md`](docs/persistence/SAVE_UPDATE_PROTOCOL.md) - bounded post-interaction transactions, owner-routed updates, atomic Save Points, and recovery.
- [`docs/persistence/PERSISTENCE_VALIDATION.md`](docs/persistence/PERSISTENCE_VALIDATION.md) - immutable validation baselines, trigger-specific profiles, defect detection, protected reporting, severity, and owner-routed repair.
- [`docs/persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md`](docs/persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md) - completed-system ownership, operating cycle, specialist handoffs, correction routes, and the template boundary.
- [`docs/world-engine/README.md`](docs/world-engine/README.md) - World Engine reading order, ownership boundaries, and campaign-data boundary.
- [`docs/world-engine/WORLD_ENGINE_OVERVIEW.md`](docs/world-engine/WORLD_ENGINE_OVERVIEW.md) - causal world-simulation foundations.
- [`docs/world-engine/WORLD_STATE_VARIABLES.md`](docs/world-engine/WORLD_STATE_VARIABLES.md) - shared rules for scoped, evidenced, persistent world conditions without repository-stored values.
- [`docs/world-engine/CAUSAL_EVENT_CHAINS.md`](docs/world-engine/CAUSAL_EVENT_CHAINS.md) - branching, interruptible causal propagation through autonomous actors, delays, feedback, and cross-system handoffs.
- [`docs/world-engine/POPULATIONS.md`](docs/world-engine/POPULATIONS.md) - scoped demographic rules for composition, life cycles, continuity, momentum, dependency, and distributed capability.
- [`docs/world-engine/RESOURCES_AND_FOOD.md`](docs/world-engine/RESOURCES_AND_FOOD.md) - access, quality, supply chains, renewal, depletion, substitution, sustenance, Food Security, and waste.
- [`docs/world-engine/ECONOMIES.md`](docs/world-engine/ECONOMIES.md) - contextual production, allocation, exchange, prices, currencies, obligations, distribution, and adaptation.
- [`docs/world-engine/ECOLOGY_AND_MIGRATION.md`](docs/world-engine/ECOLOGY_AND_MIGRATION.md) - current ecosystem dynamics, disturbance, recovery, migration routes, displacement, arrival, and cross-region consequences.
- [`docs/world-engine/FACTION_BEHAVIOUR.md`](docs/world-engine/FACTION_BEHAVIOUR.md) - distributed faction interests, information, decisions, mobilization, cohesion, adaptation, and continuity.
- [`docs/world-engine/WAR_AND_UNREST.md`](docs/world-engine/WAR_AND_UNREST.md) - causal contestation, unrest, organized violence, operations, territorial control, cessation, and conflict legacies.
- [`docs/world-engine/DISEASE_EVOLUTION.md`](docs/world-engine/DISEASE_EVOLUTION.md) - qualitative disease causality, transmission, outbreak dynamics, agent evolution, care, public health, and legacies.
- [`docs/world-engine/TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md`](docs/world-engine/TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md) - contextual technical change from discovery and invention through adoption, infrastructure, maintenance, decline, and recovery.
- [`docs/world-engine/DUNGEON_ACTIVITY.md`](docs/world-engine/DUNGEON_ACTIVITY.md) - causal Dungeon formation, boundaries, activity, ecology, access, extraction, collapse, and persistent consequences without automatic resets or level scaling.
- [`docs/world-engine/WORLD_STABILITY.md`](docs/world-engine/WORLD_STABILITY.md) - scoped continuity and transformation under strain without a universal world-health meter or automatic Reset trigger.
- [`docs/world-engine/AGES_AND_WORLD_RESETS.md`](docs/world-engine/AGES_AND_WORLD_RESETS.md) - contextual Ages, causal transitions, exceptional World Resets, uneven survivorship, and consequence-preserving revalidation.
- [`docs/world-engine/GATES_AND_WORLD_CONTACT.md`](docs/world-engine/GATES_AND_WORLD_CONTACT.md) - bounded World Gates, world-contact processes, channel-specific transit, compatibility, asymmetry, closure, and persistent contact consequences.
- [`docs/world-engine/WORLD_GATE_SOUL_INTERACTIONS.md`](docs/world-engine/WORLD_GATE_SOUL_INTERACTIONS.md) - bounded interaction among World Gates, Reincarnation placement, Soul Avatar Triggers, provenance, and continuity.
- [`docs/world-engine/SIMULATION_ABSTRACTION.md`](docs/world-engine/SIMULATION_ABSTRACTION.md) - canonical simulation resolution, compression, expansion, off-screen advancement, Time Skips, and century-scale continuity.

## Start Here

Contributors and coding agents should read, in order:

1. [`AGENTS.md`](AGENTS.md)
2. [`design/ROADMAP.md`](design/ROADMAP.md)
3. [`design/DECISIONS.md`](design/DECISIONS.md)
4. [`design/TERMINOLOGY.md`](design/TERMINOLOGY.md)
5. [`design/REPOSITORY_CONVENTIONS.md`](design/REPOSITORY_CONVENTIONS.md)
6. [`design/UNRESOLVED_QUESTIONS.md`](design/UNRESOLVED_QUESTIONS.md)
7. [`design/FUTURE_REVISIONS.md`](design/FUTURE_REVISIONS.md)
8. [`docs/core/DESIGN_PHILOSOPHY.md`](docs/core/DESIGN_PHILOSOPHY.md)

## Repository Validation

Run [`tools/validate_repository.ps1`](tools/validate_repository.ps1) from the repository root after documentation changes:

```powershell
pwsh -NoProfile -File .\tools\validate_repository.ps1
```

The validator checks local Markdown links and anchors, navigation and registry coverage, template coverage, orphan documents, roadmap declarations, duplicate terminology headings, and forbidden campaign-data directories.

## Current Status

**Eternal Cycle v1.0.0 — Release 1**

**Status: Released**

Phases 0 through 12 are complete and reviewed. The [Release 1 Audit](design/audits/RELEASE_1_AUDIT.md) records the final gate, and [Phase 13 — Future Revisions](design/ROADMAP.md#phase-13--future-revisions) is the active owner-mediated post-release stage. Gameplay evidence may enter the [Future Revisions register](design/FUTURE_REVISIONS.md), but released mechanics remain unchanged until the project owner authorizes implementation.
