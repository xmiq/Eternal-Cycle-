# World Engine Overview

## Purpose

The World Engine models how conditions and decisions create further consequences across time.

It follows [Rule Zero](../core/DESIGN_PHILOSOPHY.md#rule-zero): world change emerges from established causes, pressures, actors, counterforces, and consequences before authorial convenience. It remains a rules framework; current world facts belong in an external Campaign Record.

## Shared State Model

[World-State Variables](WORLD_STATE_VARIABLES.md) defines how every domain scopes, evidences, updates, and preserves changing conditions without creating a universal world score or storing campaign data in this repository.

## Causal Chains

[Causal Event Chains](CAUSAL_EVENT_CHAINS.md) defines how an established condition links to plausible reactions, Counterforces, branches, delays, feedback, and persistent consequences:

> Dragon extinction → wyvern expansion → griffin migration → livestock loss → village fortification → military recruitment → taxation pressure.

A chain is not destiny. Counterforces, adaptation, intervention, and chance may redirect it.

## Core Domains

- [population and demographics](POPULATIONS.md);
- [food and natural resources](RESOURCES_AND_FOOD.md);
- [ecology and migration](ECOLOGY_AND_MIGRATION.md);
- [trade and economies](ECONOMIES.md);
- [factions](FACTION_BEHAVIOUR.md) and institutions;
- [war and unrest](WAR_AND_UNREST.md);
- [disease and medicine](DISEASE_EVOLUTION.md);
- [technology and magical advancement](TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md);
- [dungeons and supernatural regions](DUNGEON_ACTIVITY.md);
- [World Stability](WORLD_STABILITY.md), systemic strain, response, and transformation;
- [Ages and World Resets](AGES_AND_WORLD_RESETS.md), historical transitions, revalidation, and long continuity;
- [World Gates and World-Contact Events](GATES_AND_WORLD_CONTACT.md), extraordinary contact routes, channel-specific exchange, compatibility, closure, and persistent contact consequences;
- [World Gate interactions with Reincarnation and Soul Avatars](WORLD_GATE_SOUL_INTERACTIONS.md), bounded cross-domain placement reach, Avatar Trigger relevance, provenance, and continuity safeguards;
- climate and disasters;
- culture and religion.

## Simulation Resolution

[Simulation Abstraction](SIMULATION_ABSTRACTION.md) defines the complete procedure for choosing detail, compressing intervals, expanding into play, and preserving agency and continuity. The canonical resolutions are:

- **Focused:** actors, sequence, timing, and choices for one contested decision or sensitive transition.
- **Local:** material relationships within a bounded community, site, route, habitat, institution, or project.
- **Regional:** aggregate patterns across connected populations, territories, networks, ecosystems, institutions, or conflicts.
- **Epochal:** structural tendencies, transformations, survivorship, succession, and legacies across generations or Ages.

Scope, elapsed interval, and resolution are chosen independently. Use the coarsest resolution that preserves every material choice, dependency, exception, and uncertainty. The engine should produce playable consequences, not unnecessary bookkeeping.

## Magic Interface

[Magic and the World Engine](../magic/WORLD_ENGINE_INTERACTIONS.md) defines how an established magical cause enters this framework through direct effects, footprint, distribution, responses, delays, feedback, persistence, and recovery. World Engine domains and Simulation Abstraction resolve the resulting world-side changes without taking ownership of Magic.

## Related Documents

- [World Engine Index](README.md)
- [World-State Variables](WORLD_STATE_VARIABLES.md)
- [Causal Event Chains](CAUSAL_EVENT_CHAINS.md)
- [Populations](POPULATIONS.md)
- [Resources and Food](RESOURCES_AND_FOOD.md)
- [Economies](ECONOMIES.md)
- [Ecology and Migration](ECOLOGY_AND_MIGRATION.md)
- [Faction Behaviour](FACTION_BEHAVIOUR.md)
- [War and Unrest](WAR_AND_UNREST.md)
- [Disease Evolution](DISEASE_EVOLUTION.md)
- [Technology and Magical Advancement](TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md)
- [Dungeon Activity](DUNGEON_ACTIVITY.md)
- [World Stability](WORLD_STABILITY.md)
- [Ages and World Resets](AGES_AND_WORLD_RESETS.md)
- [World Gates and World-Contact Events](GATES_AND_WORLD_CONTACT.md)
- [World Gate Interactions with Reincarnation and Soul Avatars](WORLD_GATE_SOUL_INTERACTIONS.md)
- [Simulation Abstraction](SIMULATION_ABSTRACTION.md)
- [Design Philosophy](../core/DESIGN_PHILOSOPHY.md)
- [Magic and the World Engine](../magic/WORLD_ENGINE_INTERACTIONS.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Roadmap](../../design/ROADMAP.md)
