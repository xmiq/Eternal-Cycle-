# Game Master Rules

The GM rules define how Eternal Cycle canon is applied during play without placing any live campaign state in this repository.

Repository-wide ownership, dependency, extension, and consumer metadata is maintained in the [Canonical Document Registry](../DOCUMENT_REGISTRY.md#game-master-toolkit).

## Reading Order

1. [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md) - conceptual rules, simulation, interface, identity, and information responsibilities.
2. [GM Principles](GM_PRINCIPLES.md) - concise commitments for fair, systemic, agency-preserving play.
3. [Game Master Responsibilities](GM_RESPONSIBILITIES.md) - bounded duties, ownership handoffs, operating discipline, delegation, records, and human/AI parity.
4. [Game Master Framework](GAME_MASTER_FRAMEWORK.md) - authority, session procedure, continuity, world simulation, information boundaries, records, and AI GM guidance.
5. [Campaign Bootstrap](CAMPAIGN_BOOTSTRAP.md) - released-engine status, `NORMAL` campaign default, explicit testing modes, starting-profile separation, new-campaign persistence, and resume compatibility.
6. [Consequence Resolution](CONSEQUENCE_RESOLUTION.md) - immediate-outcome closure, causal layers, proportionality, handoffs, persistence, and external recording.
7. [Uncertainty Handling](UNCERTAINTY_HANDLING.md) - information views, uncertainty sources, evidence, deterministic and random resolution, deferral, fair secrecy, and correction.
8. [Reincarnation Generation](REINCARNATION_GENERATION.md) - world-grounded candidate sourcing, contextual adjudication, eligibility, personhood, mode-specific presentation, selection handoff, and revalidation.
9. [Encounter Generator](ENCOUNTER_GENERATOR.md) - causal source collection, eligibility, decision framing, non-scaling, agency routes, adjudication handoffs, and external records.
10. [Monster Generator](MONSTER_GENERATOR.md) - world-valid species sketches, individual histories, sourced variation, embodiment, capability, information, placement, and external profiles.
11. [NPC Generator](NPC_GENERATOR.md) - person-basis validation, proportional actor detail, bounded knowledge, independent decisions, relationships, continuity, and external profiles.
12. [Dungeon Generator](DUNGEON_GENERATOR.md) - Dungeon Basis validation, causal Topology, actor-specific access, activity, inhabitants, resources, hazards, claims, and revalidation.
13. [Faction Generator](FACTION_GENERATOR.md) - Faction Basis validation, participants, interests, information and decision routes, function-specific capacity, continuity, and revalidation.
14. [World-Event Generator](WORLD_EVENT_GENERATOR.md) - causal Basis validation, direct Event Boundaries, footprint, timing, uncertainty, owner handoffs, and external records.
15. [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md) - resolution choice, off-screen advancement, Time Skips, player-agency checkpoints, and long-horizon continuity.
16. [Time Skip Procedure](TIME_SKIP_PROCEDURE.md) - player-authorized Skip Mandates, Standing Instructions, interruption, causal advancement, return states, and Causal Bridges.
17. [Age Transition Procedure](AGE_TRANSITION_PROCEDURE.md) - evidence-backed historical classification, scoped and disputed boundaries, World Reset checkpoints, targeted revalidation, and return to play.
18. [Provisional Rulings](PROVISIONAL_RULINGS.md) - release-neutral campaign-local adjudication for narrow gaps in current Canon; use does not imply testing status.
19. [Campaign Persistence Integration](../persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md) - the load, owner-resolution, Save Update, validation, activation, and presentation contract used by human and AI GMs.
20. [AI Game Master Operating Procedures](../ai/README.md) - implementation-neutral workflow, session-start, play, save, and checklist procedures for automated operation of the same GM responsibilities.
21. [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md) - the implementation-neutral relationship among AI execution, Campaign Persistence, adapters, campaign state, and repository rules; runtime profiles remain replaceable extensions.

## Cross-Campaign Reusable Design

Use the [GM Living Codex](../gm-living-codex/README.md) before creating reusable species, variants, or Evolution structures. The Codex is a separate GM-approved design authority, not campaign state or player knowledge, and it remains subordinate to the mechanics in this repository.

## Authority and Boundary

The framework applies Repository Canon; it does not create an independent source of mechanics. Provisional rulings remain subordinate to Canonical rules and Foundations and are stored in an external Campaign Record.

AI execution profiles and configured Direct or Managed persistence implementations carry out these responsibilities without gaining mechanical or campaign authority. MCP may provide one Managed interface; it is not a universal requirement. The [AI operating index](../ai/README.md) classifies shared procedures and runtime-specific profiles while preserving human and AI GM parity.

Current characters, bodies, Soul state, inventories, relationships, settlements, factions, quests, timelines, sessions, and live world state do not belong in this repository.
