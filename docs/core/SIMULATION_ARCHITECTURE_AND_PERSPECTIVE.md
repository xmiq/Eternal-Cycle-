# Simulation Architecture and Perspective Model

## Purpose

This document owns Eternal Cycle's conceptual responsibility architecture. It connects the rules repository, campaign simulation, and player-facing play without replacing any specialist mechanic or requiring the directory tree to mirror the architecture.

The model has three layers:

```text
Layer 1 - Immutable Rules
        ↓
Layer 2 - GM Simulation Engine
        ↓
Layer 3 - Player RPG Interface
```

The layers describe authority and information flow. They are not software modules, storage products, or new progression systems.

## Layer 1 - Immutable Rules

Immutable Rules define what is mechanically possible, which constraints apply, and how a result is resolved. Repository Canon under `docs/` and its governing decisions occupy this layer.

Attributes, Development, Skills, Magic, Souls, Reincarnation, Monster Evolution, Human structures, Soul Weapons, reproductive compatibility, lineage inheritance, and other specialist rules retain their existing owners. Layer 1 does not duplicate them.

Layer 1 is independent of any observer's knowledge. An undiscovered fact or hidden capability remains mechanically real when established by its owner.

## Layer 2 - GM Simulation Engine

The GM Simulation Engine applies Layer 1 to objective campaign state. It owns what exists, what is currently true, what changes, and what continues off-screen.

Its responsibilities include:

- entities and relevant aggregates;
- locations, populations, factions, ecosystems, and infrastructure;
- resources, projects, relationships, and timelines;
- controller relationships and active perspectives when persistent;
- autonomous processes and off-screen developments;
- objective campaign Canon and protected GM Secrets;
- entity-relative knowledge where a decision or continuity depends on it;
- consequences and their provenance.

The [World Engine](../world-engine/WORLD_ENGINE_OVERVIEW.md) owns causal simulation rules. The [Campaign Persistence Engine](../persistence/README.md) remembers established simulation state. The [GM Toolkit](../gm/README.md) applies both. Layer 2 is their architectural meeting point, not a replacement for any of them.

Player absence does not freeze objective reality. Established actors and processes may continue when existing rules, state, and causal support justify the change. Layer 2 may not alter state merely to simplify narration or match a player assumption.

## Layer 3 - Player RPG Interface

The Player RPG Interface presents the campaign through a selected Perspective. It includes narration, sensory information, dialogue, choices, visible consequences, discoverable rules information, known maps, beliefs, theories, and uncertainty.

Layer 3 answers what the active Perspective can perceive, know, infer, or be told now. It does not own objective truth and cannot silently rewrite Layer 2.

Exact simulation values may be represented approximately when they are not directly knowable. Existing interface rules still determine which values are visible; this architecture does not hide every number.

## Resolution Flow

```text
Player action or decision
        ↓
Player RPG Interface receives the declared action
        ↓
GM Simulation Engine loads relevant state
        ↓
Immutable Rules resolve what can occur
        ↓
GM Simulation Engine updates objective state
        ↓
Perspective and knowledge filter the result
        ↓
Player RPG Interface narrates discoverable consequences
```

This is a responsibility model. A trivial action need not invoke three literal programs.

## Truth Boundary

The established [Truth Layers](../persistence/TRUTH_LAYERS.md) remain authoritative:

- **Canon** records accepted campaign truth at its proper authority.
- **Character Knowledge** records what a character knows, believes, remembers, or misunderstands.
- **GM Secrets** protect established or deliberately prepared information from unauthorized disclosure.

Narration, observation, belief, suspicion, theory, inference, and misinformation do not become Canon through repetition. GM knowledge does not automatically become Character Knowledge. Conversation context may assist retrieval but cannot override structured campaign state.

When narration conflicts with persistent state, use [Continuity Resolution](../persistence/CONTINUITY_RESOLUTION.md). Stop relying on the conflicting claim, classify the defect, restore objective continuity, and change persistent state only if Canon genuinely changed.

## Entity, Controller, and Perspective

### Entity

An **Entity** is a persistent thing represented independently in campaign simulation. A character, NPC, creature, construct, remote body, familiar, summoned being, or independently represented infrastructure node may be an Entity.

An Entity whose continuity matters should have a stable identity appropriate to its subsystem. That identity survives ordinary changes to location, assignment, injury, equipment, controller, relationships, and temporary status. Transient projectiles, incidental particles, and anonymous crowd members do not require permanent identity.

### Controller

A **Controller** is the actor or authority currently permitted to make some decisions for an Entity. A player, GM, AI process, other Entity, faction process, magical compulsion, or no active Controller may occupy that role where rules permit.

Control is scoped. Reassignment does not create a new Entity or erase its history. One Controller may operate multiple Entities, and one Entity may have divided decision authority, only when established rules and campaign state permit it.

The player is an external Controller. A player character is an in-world Entity. They are not synonymous.

### Perspective

A **Perspective** is the sensory and informational viewpoint through which events are perceived or presented. It may depend on embodiment, sensory organs, remote senses, magical observation, communication, obstruction, equipment, status effects, Soul abilities, knowledge, or network links.

Perspective is not necessarily the Controller's location or the controlled Entity's ordinary senses. Shared senses, remote observation, possession, and distributed viewpoints are possible architectural consumers, not mechanics implemented here.

### Autonomy

Autonomy is distinct from control. A controlled Entity may retain authority over local decisions while accepting strategic direction. This document defines only that boundary; it does not define autonomy levels or implement the proposed Autonomous Registry.

## Identity Across Reincarnation

[Reincarnation](../soul/REINCARNATION.md) owns Soul continuity, death, and embodiment. This architecture preserves its distinctions:

```text
Player Controller
        ↓
incarnated Entity and body A
        ↓ death and Reincarnation
incarnated Entity and body B
```

Controller continuity, Soul identity, incarnated Entity identity, and body identity must be recorded according to their owners rather than collapsed into one identifier. A Soul is not forced to equal one permanent body, and a change of incarnation does not grant unsupported continuity to bodily or world-bound state.

## Perspective Filtering

Before player-facing presentation, the GM determines:

1. What objectively occurred?
2. Which Perspective receives information?
3. What can that Perspective perceive?
4. What relevant knowledge does the observing Entity possess?
5. What remains uncertain?
6. What is inferred rather than observed?
7. What information is hidden?
8. What representation is justified?

For example, an exact hidden position in Layer 2 may produce only an audible scrape in Layer 3. A monster may recognize a scent that the player character cannot identify. Neither the player's assumption nor the GM's complete knowledge may be substituted for the observing Entity's information.

## Entity-Relative Knowledge Boundary

Knowledge and belief belong to a knowing Entity or Perspective, not to the world globally. Objective fact, observation, belief, suspicion, theory, inference, misinformation, and unknown information remain distinguishable.

Current persistence may record the views needed for continuity. A future Knowledge System may add structured confidence, evidence, source, last confirmation, contradiction, testimony, propagation, or memory degradation. This document reserves that boundary but does not implement those records, values, or inference mechanics.

NPCs and other Entities act from plausible perception, knowledge, beliefs, motives, abilities, and communication. They may not use GM Secrets merely because the GM knows them.

## Persistent State and Provenance

The structured campaign save is authoritative over conversation context. Before adjudication, load the smallest dependency-complete set relevant to involved Entities, location, relationships, species, capabilities, projects, infrastructure, and materially relevant knowledge or Perspective.

Campaign persistence needs conceptual homes for:

- objective Entity state;
- stable Entity identifiers and typed references;
- persistent Controller assignments;
- active Perspective when continuity depends on it;
- Character Knowledge and protected GM Secrets;
- provenance for consequential state, knowledge, rulings, and corrections.

Provenance may identify an originating event, observation, source record, rule, adjudication, action, or migration. Exhaustive provenance is not required for every transient value. The current storage-neutral architecture can represent these boundaries without a new database migration.

Groups, populations, factions, and infrastructure may remain aggregates. They become individually tracked Entities only when continuity or resolution requires it; this architecture does not impose a universal entity-component model.

## Provisional Campaign Rules and Development

A [Provisional Rule](../gm/ALPHA_PLAYTEST_RULES.md#provisional) is explicit, campaign-scoped, identifiable, reviewable, and stored with campaign state or metadata. It may resolve a narrow gap but does not become Immutable Rules, Living Codex canon, repository policy, or a Future Revision through use.

```text
Gameplay reveals an issue
        ↓
campaign uses existing rules or a bounded Provisional Rule
        ↓
project maintainer reviews the evidence
        ↓
ignore | campaign-specific | documentation clarification | Future Revision
        ↓
owner-authorized roadmap work, if promoted
```

The gameplay GM does not edit repository requirements or the Future Revisions register. The project maintainer remains the mediator defined by [Future Revisions governance](../../design/FUTURE_REVISIONS.md).

## Reusable Designs and Campaign Instances

The [GM Living Codex](../gm-living-codex/GM_LIVING_CODEX.md) stores reusable approved designs. Layer 2 stores campaign instances. A reusable species, construct model, Evolution branch, or template does not become an actual campaign Entity until campaign state instantiates it. Campaign divergence does not silently modify the reusable design.

## GM and AI Operating Procedure

[FR-011 Context Assembly](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) provides the GM Simulation Engine with relevant objective records before resolution and filters final presentation only after required persistence. GM access through a Context Packet does not become Entity Knowledge, and conversation context cannot overwrite Objective State.

For a consequential interaction:

1. Load relevant canonical campaign state.
2. Identify involved Entities, Controllers, and the active Perspective.
3. Resolve the declared action through the appropriate Layer 1 owners.
4. Update Layer 2 objective state and causal handoffs.
5. Determine perceivable information and applicable Entity Knowledge.
6. Preserve uncertainty, protected information, and unsupported unknowns.
7. Produce Layer 3 narration without adding unstored facts.
8. Persist changes according to the [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md).

Operational plumbing remains backstage during ordinary play. If required persistence fails, do not assert dependent durable consequences as saved.

## Safeguards

- Player-facing narration cannot overwrite objective state.
- Player belief, theory, or repeated wording cannot become Canon without authority and evidence.
- GM Secrets cannot leak into Character Knowledge, NPC decisions, indexes, or summaries.
- Entity identity cannot reset merely because Controller, Perspective, assignment, or location changes.
- Player and player character cannot be treated as one universal identity.
- Off-screen state cannot change without an established cause or process.
- Layer 2 cannot bend reality for narrative convenience, encounter scaling, or desired drama.
- Layer 3 cannot reveal exact hidden values or facts without an information route.
- Missing knowledge remains unknown; it is not completed for narrative smoothness.
- This architecture does not implement possession, remote-body rules, multiplayer, distributed minds, an Autonomous Registry, or a complete Knowledge System.

## Worked Architectural Examples

### Familiar View

The player controls the character Entity while narration temporarily uses a familiar's sensory Perspective. The familiar remains a separate Entity; its information does not automatically become the character's knowledge unless an established link transmits it.

### Controller Reassignment

An established effect changes who may direct a construct. The construct keeps its Entity ID, damage, relationships, assignments, and history. Only the scoped Controller relationship changes.

### Reincarnation

The player's Controller continuity persists across lives. The prior body and current body are not treated as the same physical Entity, while the Soul continuity defined by Reincarnation remains intact.

### Hidden Faction Action

A faction acts off-screen using its information and resources. Layer 2 records the action and consequences. Layer 3 reveals only traces available to the active Perspective; a player theory about the culprit remains a theory.

## Related Documents

- [Design Philosophy](DESIGN_PHILOSOPHY.md)
- [World Engine Overview](../world-engine/WORLD_ENGINE_OVERVIEW.md)
- [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Truth Layers](../persistence/TRUTH_LAYERS.md)
- [Structured Persistence Architecture](../persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md)
- [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md)
- [Reincarnation](../soul/REINCARNATION.md)
- [GM Living Codex](../gm-living-codex/GM_LIVING_CODEX.md)
