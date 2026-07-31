# World Engine

The World Engine defines how changing conditions, autonomous actors, institutions, environments, and established system outputs create persistent consequences across time. It is a reusable simulation framework, not a campaign, save file, scripted timeline, random-event generator, or replacement for GM judgment.

Read the completed Soul, Development, Skill, Monster Evolution, Human, Soul Weapon, and Magic indexes before adjudicating a world claim that depends on them. The World Engine receives established facts from those systems and returns changed circumstances through their existing owners.

## Canonical Reading Order

1. [World Engine Overview](WORLD_ENGINE_OVERVIEW.md) establishes Rule Zero, causal world autonomy, core domains, and the requirement to scale detail to play.
2. [World-State Variables](WORLD_STATE_VARIABLES.md) defines the shared qualitative state model, variable families, evidence, uncertainty, persistence, ownership, external record boundary, and update procedure used by every World Engine domain.
3. [Causal Event Chains](CAUSAL_EVENT_CHAINS.md) defines supported links, autonomous responses, branches, Counterforces, feedback, delays, Pending Consequences, Causal Horizons, and cross-system handoffs.
4. [Populations](POPULATIONS.md) defines demographic boundaries, units, composition, cohorts, entry and exit, life cycles, continuity, momentum, dependency, capability distribution, and uncertainty.
5. [Resources and Food](RESOURCES_AND_FOOD.md) defines Resource Claims, Effective Supply, scarcity, bottlenecks, renewal, depletion, substitution, reserves, sustenance, Food Security, source agency, and Waste Burdens.
6. [Economies](ECONOMIES.md) defines production and allocation networks, Exchange Claims, value, Price, currencies, Purchasing Access, Economic Capacity, obligations, taxation, concentration, shocks, and adaptation.
7. [Ecology and Migration](ECOLOGY_AND_MIGRATION.md) defines current ecosystem state, Habitat Connectivity, functions, disturbance, resilience, succession, Novel Ecologies, migration pressures and routes, displacement, settlement, and cross-region consequences.
8. [Faction Behaviour](FACTION_BEHAVIOUR.md) defines faction boundaries, distributed interests and information, decision routes, mobilization, capability, cohesion, dissent, adaptation, relationships, and continuity.
9. [War and Unrest](WAR_AND_UNREST.md) defines contestation, Civil Unrest, Armed Conflict, War, escalation, mobilization, Operational Capacity, territorial control, civilian agency, cessation, demobilization, and conflict legacies.
10. [Disease Evolution](DISEASE_EVOLUTION.md) defines Disease Processes, Etiology, hosts, exposure, transmission, Disease States, outbreaks, agent evolution, interventions, public health, Reincarnation boundaries, and disease legacies.
11. [Technology and Magical Advancement](TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md) defines Technical Systems, Discovery, Invention, Validation, Adoption, Diffusion, implementation, maintenance, automation, decline, recovery, and collective magical advancement.
12. [Dungeon Activity](DUNGEON_ACTIVITY.md) defines Dungeon classification, formation, boundaries, Sustaining Bases, Topology, activity regimes, inhabitants, resources, delving, collapse, and persistent legacies.
13. [World Stability](WORLD_STABILITY.md) defines scoped Stability Referents, supports, World-System Strain, Systemic Coupling, buffers, response, thresholds, qualitative findings, interventions, displacement, forecasts, and legacies without creating a universal meter or Reset trigger.

Later Phase 8 documents add domain-specific rules in roadmap order. A mention of a later domain here does not complete or partially implement its task.

## Core Ownership

| Claim | Primary owner |
| --- | --- |
| What a current world condition is, how it is scoped, and how it changes through causality | World Engine |
| Current campaign values, named actors, locations, incidents, and timelines | External Campaign Record |
| Soul identity, persistence, memory, and Reincarnation | [Soul Engine](../soul/README.md) |
| Individual capability change and reliability | [Development System](../progression/README.md) |
| Learned capabilities | [Skill Engine](../skills/README.md) |
| Monster forms, Evolution Routes, and species-level body change | [Monster Evolution](../monster-evolution/README.md) |
| Classes, Professions, institutions, offices, and social recognition | [Human Classes and Professions](../human/README.md) |
| Weapon Soul personhood, bond, form, and weapon-owned capability | [Soul Weapons](../soul-weapons/README.md) |
| Mana, affinities, Spells, Rituals, Enchantments, Alchemy, Divine Magic, and restrictions | [Magic](../magic/README.md) |
| Campaign operation, information views, continuity, and provisional rulings | [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md) |

One event may alter several world conditions while involving several specialist owners. Record each effect once. The World Engine can change access, environment, demand, opportunity, risk, recognition, and consequences; it cannot manufacture a Skill, Soul effect, Evolution, Class, Weapon Function, or magical capability.

## Repository Boundary

World Engine documents contain rules, schemas, procedures, examples, and safeguards. Actual World-State Profiles, current values, named populations, resource stocks, prices, factions, wars, diseases, dungeons, Gates, settlements, and timelines are campaign data and remain outside this repository.

## Authority

Files in this section contain playable canonical rules. Accepted governance remains in [Design Decisions](../../design/DECISIONS.md), canonical vocabulary remains in [Terminology](../../design/TERMINOLOGY.md), and implementation order remains controlled by the [Roadmap](../../design/ROADMAP.md). Any conflict must be resolved before an affected task can be complete.
