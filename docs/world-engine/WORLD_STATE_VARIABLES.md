# World-State Variables

## Purpose

This document defines how Eternal Cycle represents changing world conditions without placing campaign values in the canonical repository or reducing the world to one score.

It supplies the shared state grammar for every World Engine domain. It does not define the detailed variables, formulas, or procedures for populations, resources, economies, ecology, factions, war, disease, advancement, dungeons, World Stability, Ages, Gates, or [Simulation Abstraction](SIMULATION_ABSTRACTION.md).

## Core Rule

A **World-State Variable** is a scoped, causally meaningful description of one changing world condition or relationship. It states what is being tracked, for whom or what, where, over what time, under which owner, from what evidence, and with what uncertainty.

The repository defines variable rules and reusable formats. A campaign stores actual values and changes in its external [Campaign Record](../../design/TERMINOLOGY.md#campaign-record).

No World-State Variable is a universal currency, world health score, plot progress meter, or source of capability.

## World State and Canon

**World State** is the complete set of current facts and unresolved conditions that materially describe a campaign world at a particular time. It may include geography, populations, resources, relationships, institutions, ecology, magic, infrastructure, threats, beliefs, and pending consequences.

World State is campaign truth, not design canon.

Canonical rules answer questions such as:

- what kinds of conditions may matter;
- how a condition must be scoped;
- what evidence and uncertainty mean;
- how causes, counterforces, delays, persistence, and recovery are handled;
- how one domain hands a changed condition to another;
- which specialist system owns a claimed effect.

Campaign records answer questions such as:

- how many people currently inhabit a named valley;
- whether a named granary is full;
- what a current faction believes;
- which road is blocked;
- what price grain commands today;
- which disease is spreading;
- whether a particular Gate is open.

An example in this document illustrates a reusable rule. It does not establish a default world fact.

## The State Claim

Every material variable begins with a **World-State Claim**:

> For this subject, scope, time, and purpose, this condition is in this state or trend, supported by this evidence and uncertainty.

A valid claim identifies:

1. **Subject.** The population, resource, place, route, institution, relationship, process, or other world element being described.
2. **Scope.** The geographic, social, ecological, institutional, metaphysical, or functional boundary within which the claim applies.
3. **Time.** The moment, interval, season, generation, Age, or other relevant horizon.
4. **Purpose.** The decision or consequence for which the variable matters.
5. **State owner.** The World Engine domain or specialist system that defines the underlying fact.
6. **State type.** The kind of condition being represented.
7. **Current finding.** A qualitative state, local measure, bounded quantity, relation, or uncertainty statement.
8. **Trend.** Whether the condition is rising, falling, shifting, oscillating, stable, recovering, or unknown at the chosen scale.
9. **Evidence.** The observations, records, testimony, measurement, inference, or established causes supporting the finding.
10. **Dependencies.** Conditions that sustain, limit, expose, or transform the variable.
11. **Distribution.** Who benefits, controls, supplies, pays, is excluded, bears risk, or can respond.
12. **Persistence.** What keeps the condition present, what can change it, and what may remain afterward.
13. **Uncertainty.** What is unknown, disputed, hidden, stale, or resolution-dependent.

Omitting a field that cannot affect the current decision is acceptable. Omitting a dependency, affected group, or uncertainty that could reverse the conclusion is not.

## Variable Families

The following families describe different questions. They do not convert into one another through a universal rate.

### Stock

A **Stock** is something accumulated or presently available within a scope, such as stored food, habitable space, trained personnel, water, records, maintained infrastructure, or recoverable material.

A Stock must identify what counts, who can access it, its condition, and the time at which it is assessed. Possession on paper does not prove usable access.

### Flow

A **Flow** is movement or change across time or a boundary, such as births, deaths, migration, harvest, trade, taxation, Mana movement, information, repair, recruitment, or disease transmission.

A Flow does not imply an infinite source. It names origin, destination, route, losses, timing, and interruption where those facts matter.

### Capacity

A **World Capacity** is the bounded ability of a place, population, institution, ecology, or infrastructure to sustain a specified process under stated conditions.

Capacity is not individual capability and grants no Skill or Development. It depends on actual people, bodies, tools, resources, relationships, maintenance, knowledge, authority, and environment.

### Pressure

A **World Pressure** is a condition that makes some responses, transitions, or failures more likely without determining them. Scarcity, predation, debt, displacement, succession uncertainty, stigma, climate stress, military threat, and magical residue may all create pressures.

Pressure is directional evidence, not a command, event guarantee, or hidden plot meter. Actors and systems may resist, redirect, exploit, misunderstand, or amplify it.

### Relationship

A **State Relationship** records a material dependency, exchange, rivalry, alliance, jurisdiction, ecological link, information route, obligation, or other connection between scoped subjects.

Relationships are directional where appropriate. Dependence by one party does not prove equal dependence, trust, consent, knowledge, or control by another.

### Constraint

A **World Constraint** is a current condition that limits a specified world process or route. It may arise from geography, access, law, ecology, infrastructure, authority, compatibility, supply, information, conflict, or time.

A Constraint must name what it limits. It is not a general penalty and does not automatically create compensation or progress.

### Threshold Condition

A **State Threshold** is a local, evidence-based condition at which one process changes behavior, enters a new state, or requires a different resolution. Thresholds may be qualitative or quantitative.

They are not universal breakpoints. Crossing one does not grant an unrelated event, Skill, Evolution, war, collapse, or Reset unless that outcome has its own causal route.

### Legacy Condition

A **World Legacy** is a condition that remains after its initiating event or active support has ended. Ruined infrastructure, displaced populations, altered soil, debt, institutional precedent, trauma, religious memory, magical residue, and changed borders may persist on different timescales.

A Legacy is not immutable. It needs an actual persistence basis and may decay, be repaired, be reinterpreted, or create new dependencies.

## Variable Dimensions

Two variables with similar names may not be comparable unless their dimensions match.

### Subject and Population

State whose condition is represented. A city-wide food claim is not automatically true for a segregated district, migratory species, besieged garrison, or hidden community.

### Space

Name the relevant boundary: body, household, site, route, settlement, habitat, watershed, institution, region, world, or cross-world relation. Boundaries may overlap without becoming identical.

### Time

State the observation time, update interval, relevant delay, and expected persistence. A daily shortage, seasonal cycle, generational decline, and Age-scale trend require different interpretation.

### Access

Distinguish existence from effective access. A resource may be present but unreachable, forbidden, monopolized, unsafe, incompatible, unknown, or too costly to use.

### Condition and Quality

State whether what exists remains fit for its claimed purpose. Stored grain can spoil; a road can remain mapped but impassable; a school can exist without teachers; a Ritual site can remain intact but lose its source.

### Distribution

Aggregates must preserve consequential differences. A region can have enough food overall while one community starves because control, transport, law, prejudice, theft, or risk separates Stock from need.

### Agency and Control

Identify persons and groups who can consent, refuse, redirect, conceal, destroy, ration, share, or contest a condition. Gods, spirits, Weapon Souls, and other persons are actors, not Stocks.

### Evidence and Information

World truth, player knowledge, character knowledge, institutional records, rumor, propaganda, and mistaken belief remain distinct under the [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md#information-model).

## Qualitative and Quantitative Expression

World-State Variables are qualitative by default because different worlds, scales, species, and campaigns require different measures.

Local numbers are allowed when they improve decisions and have a defined source. A quantity must state:

- what is counted;
- unit and scale;
- observation time;
- collection method;
- known omissions;
- uncertainty or error;
- why the number matters.

Numbers do not become universal rules merely because they are precise. A settlement may count sacks, days of reserve, caloric equivalents, ritual portions, or accessible feeding sites. Another world may need different measures.

Useful qualitative findings may include:

- absent, scarce, constrained, adequate, abundant, or unknown;
- inaccessible, contested, controlled, distributed, or open;
- degrading, stable, volatile, recovering, or transforming;
- isolated, dependent, diversified, brittle, or redundant.

These words describe a scoped claim. They are not ranks and do not aggregate into a total world score.

## Evidence and Uncertainty

### World Truth

The GM maintains the best current account of what is actually true in the campaign, including hidden causes and unresolved uncertainty. World truth may itself be uncertain when the setting contains genuinely indeterminate processes or insufficient evidence.

### Observation

An observation records what a source perceived or measured under stated conditions. It may be accurate, incomplete, biased, manipulated, outdated, or misinterpreted.

### Estimate

An estimate combines evidence and assumptions. It should expose assumptions that could materially alter the decision.

### Belief

A belief belongs to a person, population, institution, or culture. Belief can drive real behavior without being factually true.

### Confidence

Confidence describes the support for one claim, not the importance of the subject. Use plain language such as established, supported, tentative, disputed, obscured, or unknown when a distinction matters.

No State Profile grants omniscience to players, characters, factions, institutions, or the GM's in-world agents.

## Trends, Delays, and Persistence

A state update distinguishes:

- **current condition:** what is true now;
- **direction:** how it is changing at the chosen scale;
- **driver:** what currently pushes the change;
- **counterforce:** what resists or redirects it;
- **delay:** why a cause has not yet produced its full consequence;
- **momentum:** why change may continue after the driver weakens;
- **persistence basis:** what keeps the condition present;
- **recovery route:** what could restore, replace, or reorganize the affected function;
- **legacy:** what remains after recovery or transformation.

Ending a cause does not rewind the state. Repair requires time, agency, access, resources, and an actual route.

## State Resolution

The same world may be represented at several resolutions without contradiction.

- **Focused:** enough detail for one immediate decision or contested process.
- **Local:** material conditions and relationships for a bounded community, site, route, habitat, or institution.
- **Regional:** aggregated patterns with preserved bottlenecks, minorities, dependencies, and exceptional actors.
- **Epochal:** long-horizon tendencies, transformations, and legacies with only decisive causal branches retained.

[Simulation Abstraction](SIMULATION_ABSTRACTION.md) defines the full change-of-resolution procedure. Resolution changes may summarize facts but cannot create, erase, average away, or retroactively decide a material dependency.

## World-State Profile

A **World-State Profile** is an external Campaign Record that groups only the variables relevant to a defined simulation question.

Recommended fields are:

```text
Question and purpose:
Scope and resolution:
Observation time:
Relevant subjects:
Variables and state types:
Current findings:
Trends:
Drivers and counterforces:
Dependencies and constraints:
Distribution and affected parties:
Delays and pending consequences:
Persistence, recovery, and legacies:
Evidence and information views:
Uncertainty:
Next review condition:
```

This format is a rule schema, not a repository template or campaign file. Use only fields that can alter a decision or consequence.

## Update Procedure

When elapsed time, an action, or an external event may change world state:

1. **State the simulation question.** Identify the decision, interval, or consequence being resolved.
2. **Choose scope and resolution.** Use the narrowest representation that preserves material causes and affected parties.
3. **Load established state.** Use the external Campaign Record; do not reconstruct convenient facts from desired outcomes.
4. **Identify inputs and owners.** Separate World Engine conditions from Soul, Development, Skill, Evolution, Human, Soul Weapon, Magic, and GM claims.
5. **Identify direct changes.** Apply established effects before inferring downstream responses.
6. **Identify drivers and counterforces.** Include autonomous actors, ecology, institutions, constraints, alternatives, and refusal.
7. **Apply delays and timing.** Resolve only consequences that have had time and a valid route to occur.
8. **Update affected variables.** Change each condition once under its owner; preserve distribution and uncertainty.
9. **Propagate material dependencies.** Hand changed inputs to other domains without assuming their outcomes.
10. **Record persistence and legacy.** Preserve damage, obligations, displacement, residue, precedent, and recovery needs.
11. **Update information views.** Distinguish what is true from what observers know or believe.
12. **Save externally.** Write current values, named facts, and pending consequences only to the Campaign Record.
13. **Set the next review condition.** Name the elapsed interval, threshold, action, discovery, or disruption that warrants another update.

If a missing later domain rule would determine the outcome, use the [Alpha Playtest Rules](../gm/ALPHA_PLAYTEST_RULES.md) narrowly. Do not invent a universal fallback model.

## Derived Variables and Indices

A campaign may derive a scoped indicator from several variables when it improves one decision. The indicator must state:

- the exact question it summarizes;
- included inputs and their owners;
- omitted factors;
- distribution hidden by aggregation;
- conditions under which it stops being reliable.

An index never replaces its inputs. A food-security estimate cannot become population health, political legitimacy, military readiness, or World Stability by implication.

No global power rating, civilization level, danger number, prosperity score, or world health meter is authoritative.

## Ownership Interfaces

### Souls and Reincarnation

The World Engine owns external time, changed surroundings, available embodiment contexts, witnesses, institutions, and worldly consequences. The [Soul Engine](../soul/README.md) owns persistent identity, memory, Resonance, Echoes, Titles, Avatars, and Reincarnation.

A person's Final Death may change world state through grief, succession, labor loss, conflict, testimony, inheritance, or ecology. It does not automatically change Soul Depth, create a Soul Title, or determine the next incarnation.

### Development and Skills

World conditions supply access, instruction, tools, practice environments, demand, feedback, suppression, and consequences. [Development](../progression/README.md) owns capability change, and the [Skill Engine](../skills/README.md) owns learned capabilities.

Population-level access to training is not a pooled Skill. Institutional capacity does not become the leader's personal Development.

### Monster Evolution

Ecology, scarcity, predation, climate, civilization, and magic may produce Evolutionary Pressures. [Monster Evolution](../monster-evolution/README.md) owns valid routes, conditions, transition, anatomy, and resulting forms.

A state variable can establish pressure or opportunity. It cannot declare an Evolution solely because a population count crossed a convenient threshold.

### Human Classes and Professions

The World Engine owns current distribution, access, demand, institutions, law, resources, conflict, and historical consequences. [Human Classes and Professions](../human/README.md) own the social frameworks through which expertise is organized and recognized.

Institutional survival does not prove that its practitioners retain capability, legitimacy, records, teachers, or current access.

### Soul Weapons

Weapon Souls enter world state as persons with their own knowledge, goals, relationships, custody circumstances, vulnerabilities, and choices. [Soul Weapons](../soul-weapons/README.md) own their personhood, bonds, forms, Evolution, Echoes, compatibility, and Manifestation.

The World Engine cannot count a Weapon Soul as transferable equipment, duplicate it as infrastructure, or turn custody into consent.

### Magic

[Magic and the World Engine](../magic/WORLD_ENGINE_INTERACTIONS.md) supplies bounded Magic-World Claims. The World Engine records and propagates their direct changes, distribution, dependencies, delays, feedback, persistence, and recovery.

World demand or narrative importance cannot manufacture Mana, affinity, a Spell, Ritual, Enchantment, Alchemical Process, Divine response, or prohibited effect.

### GM Framework

The [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md) owns campaign operation, information views, continuity, provisional rulings, and external records. World Engine procedures guide judgment; they do not replace it with automatic output.

## Worked Examples

### Bridge Loss and Food Access

A flood destroys the only reliable bridge into a mountain settlement.

The direct state change is route access, not immediate starvation. Relevant variables may include transport Flow, stored-food Stock, alternative-route Capacity, weather Constraint, price estimates, and distribution among households.

The GM applies delays. Merchants already inside the settlement can still sell. A monastery may control a footpath but refuse military use. Repair requires labor, material, authority, and safe access. Later resource, economy, population, and faction rules resolve their own effects.

### Monster Habitat Pressure

Mining noise and waste reduce viable nesting sites for a territorial species.

The World Engine records habitat access, disturbance Pressure, migration routes, reproduction context, and affected communities. It does not grant a Mutation or Evolution. Monster rules determine whether current bodies adapt, learned behavior changes, a valid Evolution Route appears, or the population declines.

### Magical Water Service

A Ritual network supplies clean water to several districts.

The Magic rules establish the source, Ritual Structure, operators, costs, Supply, failure, and maintenance. World-State Variables may represent service Capacity, distribution, dependency, excluded districts, operator access, source refusal risk, and recovery alternatives.

The network's existence does not create infinite water, universal magical access, or personal capability in the mayor.

### Reincarnation Across a Long Interlife

A continuing soul spends decades in Interlife.

The World Engine advances external conditions independently: institutions may change, languages drift, habitats move, and former allies age or die. The Soul Engine determines what persists with the soul and what memory is accessible. A later incarnation receives no automatic status, property, or current knowledge merely because the prior life knew the old world.

### Weapon Soul Refusal

A city relies on a site-bound Weapon Soul to regulate a defensive barrier. The Weapon Soul withdraws cooperation after officials violate a standing accord.

The refusal is an action by a person under Soul Weapon rules. The World Engine updates defensive Capacity, institutional legitimacy, public belief, repair alternatives, and faction responses. It cannot erase the refusal by classifying the Weapon Soul as infrastructure or assume the barrier fails in every respect without checking its actual sources.

### Misleading Prosperity Index

A kingdom's trade volume rises while one region loses water access and another relies on coerced labor.

A single prosperity label would hide distribution and incompatible facts. The campaign may retain trade Flow as one variable while separately tracking water Constraint, labor relationships, affected populations, political belief, and recovery routes.

No arithmetic average settles whether the kingdom is stable, just, healthy, or secure.

## Safeguards

- Keep every current value and named state outside the repository.
- Track only variables that can alter a decision, consequence, or future simulation.
- Never use one global world score, civilization level, danger rating, or progress meter.
- Never treat a Stock, Capacity, Pressure, or Threshold as a Skill, Development, Soul effect, Evolution, authority, or magical source.
- Preserve scale, time, access, quality, distribution, agency, evidence, and uncertainty.
- Apply direct effects before downstream inference.
- Preserve counterforces, delays, refusal, adaptation, and alternatives.
- Do not rewind consequences when a cause ends.
- Do not make exact numbers authoritative without defined units, evidence, omissions, and purpose.
- Do not let aggregation erase minorities, bottlenecks, exceptional actors, or hidden dependencies.
- Return changed conditions through the specialist system that owns the next claim.
- Use simulation procedures to support GM judgment, not replace it.

## Scope Boundaries

This document does not define:

- canonical campaign values or a default world;
- complete variable lists for any World Engine domain;
- population, food, market, migration, faction, war, disease, advancement, dungeon, Stability, Age, Reset, or Gate procedures beyond their owners;
- change-of-resolution and long-horizon procedures owned by [Simulation Abstraction](SIMULATION_ABSTRACTION.md);
- universal formulas, scales, thresholds, units, or update intervals;
- random-event or encounter generation;
- character sheets, world-state files, save formats, or campaign templates;
- a method for creating capability, magic, Evolution, Soul growth, or narrative destiny from world variables.

Those subjects remain with their roadmap tasks and owning systems.

## Related Documents

- [World Engine Index](README.md)
- [World Engine Overview](WORLD_ENGINE_OVERVIEW.md)
- [Design Philosophy](../core/DESIGN_PHILOSOPHY.md)
- [Magic and the World Engine](../magic/WORLD_ENGINE_INTERACTIONS.md)
- [World Stability](WORLD_STABILITY.md)
- [Ages and World Resets](AGES_AND_WORLD_RESETS.md)
- [World Gates and World-Contact Events](GATES_AND_WORLD_CONTACT.md)
- [Simulation Abstraction](SIMULATION_ABSTRACTION.md)
- [Soul Engine](../soul/README.md)
- [Development System](../progression/README.md)
- [Skill Engine](../skills/README.md)
- [Monster Evolution](../monster-evolution/README.md)
- [Human Classes and Professions](../human/README.md)
- [Soul Weapons](../soul-weapons/README.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Alpha Playtest Rules](../gm/ALPHA_PLAYTEST_RULES.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
