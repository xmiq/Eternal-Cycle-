# Dungeon Profile Template

Use this storage-neutral template to record one bounded place or spatial system whose sustained conditions create materially distinct access, habitation, hazard, resource, transformation, or outward-consequence patterns.

The repository keeps this template blank. Every populated Dungeon Profile, map, inhabitant list, route, resource, hazard, expedition, and current condition is external campaign state. Completing this form does not create a Core, floor, monster, trap, treasure, boss, reset, or encounter.

## Document Control

- **Template owner:** Locations and World modules of the [Campaign Persistence Engine](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md#locations)
- **Primary mechanical owner:** [Dungeon Activity](../docs/world-engine/DUNGEON_ACTIVITY.md)
- **Generation owner:** [Dungeon Generator](../docs/gm/DUNGEON_GENERATOR.md)
- **Dependencies:** Location identity, World-State Claims, Populations, ecology, resources, Magic, technology, factions, institutions, Relationships, Knowledge, Timeline, and every specialist source named by a claim
- **Extensions:** maps, expeditions, encounters, conflict, disease, advancement, World Stability, Ages, World Resets, Gates, and Simulation Frames
- **Consumers:** session preparation, exploration, travel, world simulation, encounters, NPC and Faction generation, Research, extraction, continuity review, migration, and validation
- **Repository boundary:** no named Dungeon, live map, current inhabitant, route, resource, hazard, claim, expedition, or campaign history belongs in this blank file

This template owns the current Dungeon Profile and place-level Dungeon identity claim. It links rather than duplicates specialist facts and never treats Dungeon classification as ownership, agency, hostility, difficulty, or reward.

## Usage Guidance

1. State the practical question, classification audience, scope, time, and simulation resolution.
2. Establish the Dungeon Basis before topology or contents: Formation, Boundary, Sustaining Basis, distinct conditions, exchanges, and continuity.
3. Derive regions and routes from actual functions, materials, processes, inhabitants, and history.
4. Separate factual Topology from every observer's dated Dungeon Map Claim.
5. Preserve each inhabitant as an actor or population under its own rules, not as Dungeon content.
6. Name the source, function, inputs, dependencies, maintenance, failure, recovery, and uncertainty of every unusual claim.
7. Record activity, extraction, damage, death, repair, reconfiguration, and external consequences persistently; nothing resets by convention.
8. Leave unsupported areas unresolved instead of filling every blank.
9. Revalidate the Profile whenever material change makes its current version misleading.

## Required Field Policy

**Required** fields must be present or carry an explicit `Unknown`, `Not Yet Verified`, `Disputed`, or `Requires Source Recovery` state. **Conditional** fields become required only when their feature exists. **Optional** fields grant nothing when omitted.

## Record Contract

### Required Identity

- **Record ID:** `<stable Dungeon or Location ID>`
- **Record type:** `Dungeon Profile`
- **Authoritative module:** `Locations and World`
- **Current name:** `<primary current label or explicit unknown>`
- **Aliases and classifications:** `<label, audience, purpose, interval, and accuracy>`
- **Dungeon Profile version:** `<current version>`
- **World and Contact Domain:** `<stable references>`
- **Observation time:** `<Timeline reference>`
- **Preparation horizon:** `<exact current decision or simulation need>`
- **Simulation resolution:** `<Focused | Local | Regional | Epochal>`
- **Campaign version:** `<applicable version reference>`

### Required Authority and Provenance

- **Persistence authority:** `<Campaign Canon or other applicable authority>`
- **Truth layer:** `<Campaign Canon | Historical Record | Research | GM Secrets | another valid layer>`
- **Canonical mechanical owners:** `<Dungeon Activity and each specialist owner>`
- **Status:** `<active | dormant | reactivating | changing | collapsing | transformed | inaccessible | disputed | other established state>`
- **Effective interval:** `<Timeline references>`
- **Source basis:** `<observation, generation, map, record, adjudication, migration, correction, or explicit unknown>`
- **Last integrated at:** `<Save Point or Transaction reference>`
- **Typed references and dependents:** `<relation type -> stable record ID>`
- **Validation status:** `<Validation Run, review result, warnings, or not yet validated>`

## Classification, Scope, and Dungeon Basis

### Required Fields

- **Classification audience:** `<who uses or disputes the Dungeon label and why>`
- **Practical scope:** `<place, regions, processes, relationships, and timescale included>`
- **Dungeon Boundary:** `<transition where material conditions change enough to matter>`
- **Sustaining Basis summary:** `<current distributed and concentrated causes>`
- **Distinct internal conditions or processes:** `<what materially differs from surrounding space>`
- **Boundary exchanges:** `<beings, resources, energy, Mana, information, disease, waste, authority, or consequences crossing the boundary>`
- **Continuity basis:** `<what lets the Dungeon identity matter beyond one isolated scene>`
- **Evidence and uncertainty:** `<Factual View, observer claims, disputes, and unknowns>`
- **Exclusions:** `<nearby cave, ruin, settlement, realm, body, Gate, Soul Space, or other system not identical to this Dungeon claim>`

The label is contextual. A cave, ruin, monster settlement, magical site, prison, sacred forest, living body, or dangerous building is not automatically a Dungeon, and classification does not alter personhood or world truth.

## Formation, Origin, and Continuity

### Required Fields

- **Dungeon Formation process:** `<how boundary, basis, conditions, exchanges, and continuity arose>`
- **Formation timeline:** `<Historical and Timeline references>`
- **Origin layers:** `<natural, constructed, ecological, living, magical, spiritual, Divine, technical, social, conflict, disease, migration, Gate, Age, Reset, or other sourced causes>`
- **Original functions and participants:** `<source records and uncertainty>`
- **Prior forms and classifications:** `<versions, intervals, and observer positions>`
- **Transformations:** `<causes, affected structures, surviving continuity, and losses>`
- **Current identity claim:** `<why this remains the same, changed, divided, merged, or successor Dungeon>`
- **Destroyed, abandoned, replaced, or inaccessible elements:** `<typed owner references>`

Origin does not predetermine present purpose, morality, ownership, inhabitants, activity, or permanence. A name alone does not preserve continuity.

## Dungeon Boundary and Access Routes

### Boundary Record

- **Boundary type:** `<architectural, geological, ecological, hydrological, atmospheric, magical, legal, perceptual, spiritual, biological, relational, or mixed>`
- **Shape and behavior:** `<sharp, gradual, porous, conditional, seasonal, directional, nested, mobile, disputed, or other factual finding>`
- **Detectability:** `<visible, concealed, mapped, instrument-dependent, sense-dependent, misunderstood, or unknown>`
- **Conditions that change across it:** `<exact claims and owners>`
- **Current stability and change:** `<drivers, Counterforces, and Review Points>`

### Dungeon Access Route

Repeat for each material route and traveler type.

- **Route ID:** `<stable Location or route reference>`
- **Origin, destination, and direction:** `<regions and directionality>`
- **Usable by:** `<body, object, signal, resource, agent, or effect and compatibility>`
- **Embodiment requirements:** `<size, movement, senses, life stage, equipment, and environment>`
- **Timing and conditions:** `<season, tide, pressure, atmosphere, Mana, schedule, and delays>`
- **Permission and relationship requirements:** `<law, key, ritual, source consent, language, trust, or other route>`
- **Support and return:** `<navigation, communication, shelter, food, water, repair, evacuation, and return conditions>`
- **Barriers and observation:** `<guards, hazards, defenders, secrecy, disease, and evidence>`
- **Current state:** `<open | restricted | blocked | damaged | forgotten | redirected | disputed | unknown>`
- **Change cause and Review Point:** `<what could alter the route>`

An entrance is not usable merely because it exists. Dungeon Access Routes are neither Soul Gates nor World Gates unless those systems independently establish them.

## Sustaining Basis and Optional Core

### Sustaining Basis Component

Repeat for each material component.

- **Component ID:** `<stable object, process, person, source, system, or relationship reference>`
- **Source and function:** `<what it actually sustains>`
- **Inputs and outputs:** `<Stocks, Flows, Mana, labor, information, waste, and exchanges>`
- **Dependencies and coupling:** `<other components, routes, actors, environments, and standards>`
- **Maintenance or renewal:** `<who or what maintains it, timing, costs, and access>`
- **Current condition:** `<function, damage, degradation, inactivity, uncertainty, and evidence>`
- **Redundancy and substitutes:** `<overlap, fallback, and failure propagation>`
- **Operator or participant agency:** `<choices, consent, authority, senses, and limits>`
- **Failure and recovery:** `<direct effect, secondary consequences, repair, replacement, and Legacy>`

### Optional Dungeon Core Record

Complete only when a concentrated component materially coordinates, anchors, supplies, records, or regulates part of the Sustaining Basis.

- **Core ID and type:** `<mechanism, reservoir, organ, node, Ritual focus, relic, construct, spirit, god, Weapon Soul, person, disputed interpretation, or other sourced form>`
- **Exact functions:** `<bounded operations>`
- **Interfaces and reach:** `<regions, processes, senses, signals, and action limits>`
- **Knowledge and uncertainty:** `<what it can observe, record, infer, or misunderstand>`
- **Agency and consent:** `<if a person; include rights and relationship boundaries>`
- **Dependencies, maintenance, vulnerabilities, and counterplay:** `<actual routes>`
- **Failure consequences:** `<functions lost, retained distributed support, affected parties, and recovery>`

A Core is optional. The label grants no personhood, omniscience, ownership, unlimited production, universal control, guaranteed weakness, or automatic clearing condition.

## Dungeon Topology and Map Claims

### Factual Topology

- **Dungeon Regions:** `<Location IDs grouped by shared function, condition, habitat, structure, or process>`
- **Connections:** `<route IDs and what each connection carries>`
- **Barriers and interfaces:** `<source, function, target assumptions, condition, and access>`
- **Distances and timing:** `<body- and route-specific measures with uncertainty>`
- **Nested or overlapping systems:** `<boundaries, owners, and non-equivalence>`
- **Current reconfiguration:** `<cause, affected regions, timing, evidence, and persistence>`
- **Unresolved topology:** `<protected unknowns and fair discovery routes>`

### Dungeon Region Record

- **Region ID and boundary:** `<stable Location reference>`
- **Function and formation:** `<why this region exists>`
- **Conditions and processes:** `<specialist references>`
- **Participants and claims:** `<Actors, Populations, Factions, Institutions, and Relationships>`
- **Access and connections:** `<route references>`
- **Resources, infrastructure, hazards, and protections:** `<owner references>`
- **Activity Regime and Review Point:** `<current pattern and change trigger>`

### Dungeon Map Claim

- **Map ID, maker, and date:** `<stable record and Timeline reference>`
- **Purpose, audience, method, scale, and resolution:** `<why and how it was made>`
- **Body and route assumptions:** `<who can use the map>`
- **Represented regions and connections:** `<claims rather than automatic truth>`
- **Omissions, uncertainty, errors, and outdated elements:** `<known and suspected>`
- **Source evidence and corrections:** `<observations, instruments, testimony, and revisions>`
- **Current relationship to Factual Topology:** `<supported | partial | disputed | obsolete | unknown>`

Floors and depth are map conventions unless a real process makes them meaningful. They do not create sequential difficulty or a final objective. Hidden topology never moves after a player choice to preserve a planned route.

## Dungeon Conditions

Create one scoped condition record per material feature.

- **Condition ID and owner:** `<stable reference and specialist system>`
- **Region, subjects, time, and purpose:** `<scope>`
- **Current finding and trend:** `<physical, ecological, magical, technical, social, spatial, legal, or other condition>`
- **Source and Sustaining Basis:** `<why it exists>`
- **Access, distribution, and affected parties:** `<who encounters or avoids it>`
- **Drivers and Counterforces:** `<current causes and responses>`
- **Dependencies, delays, and persistence:** `<time behavior>`
- **Evidence and observer views:** `<world truth, Knowledge, map claims, Research, and uncertainty>`
- **Failure, recovery, and Legacy:** `<actual routes>`
- **Review Point:** `<condition requiring reassessment>`

No condition is universal to Dungeons. Record only what can alter a current decision, process, or persistent consequence.

## Participants, Inhabitants, and Agency

### Participant Record

- **Actor, Population, Faction, Institution, or source ID:** `<stable reference>`
- **Relationship to Dungeon:** `<inhabitant | visitor | maintainer | controller | claimant | source | steward | extractor | researcher | affected party | other exact relation>`
- **Origin and continuity:** `<how the relationship arose and persists>`
- **Location, territory, and routes:** `<current scope>`
- **Needs, goals, knowledge, and relationships:** `<owned records>`
- **Current activity and consent:** `<actor-specific state>`
- **Capabilities and limits:** `<Skill, Development, species, Magic, equipment, and contextual references>`
- **Dependence on and effects upon Dungeon:** `<two-way causal claims>`
- **Rights, law, claims, and conflict:** `<Relationship, Faction, Institution, and world references>`

### Agentive Dungeon Claim

- **Claimed agent:** `<one person, distributed mind, colony, council, network, automation, several controllers, or mixed structure>`
- **Identity and personhood evidence:** `<owner references>`
- **Senses and information routes:** `<what can actually be known>`
- **Control interfaces and functions:** `<what can actually be changed>`
- **Communication and decision routes:** `<timing, access, authority, and uncertainty>`
- **Persons and processes outside its control:** `<explicit exclusions>`

Inhabitants are world participants, not encounters or contents. Dungeon classification grants no collective hostility, guilt, ownership by a Core, inability to negotiate, or permission to kill or displace.

## Dungeon Activity and Regimes

### Activity Process

- **Process ID and function:** `<habitation, movement, feeding, growth, decay, work, trade, worship, governance, conflict, construction, repair, ecology, Magic, or another actual process>`
- **Region and participants:** `<scope and actors>`
- **Inputs, outputs, and exchanges:** `<sources and destinations>`
- **Activity Drivers:** `<resource, population, maintenance, visitor, season, source, decision, damage, Gate, Age, Reset, or other current causes>`
- **Counterforces and constraints:** `<responses, limits, and opposition>`
- **Cycle, timing, delay, and persistence:** `<time behavior>`
- **Current result and uncertainty:** `<established state>`
- **External effects and Pending Consequences:** `<owner handoffs>`

### Activity Regime

- **Regime ID and scope:** `<regions or processes covered>`
- **Descriptive finding:** `<continuously maintained | seasonal | responsive | expanding | contracting | intermittently unstable | dormant but sustained | reactivating | collapsing | transforming | recovering | other supported pattern>`
- **Enabling range:** `<conditions under which the pattern holds>`
- **What continues, stops, degrades, or remains aware:** `<especially during dormancy>`
- **Reactivation or transition routes:** `<supply, repair, inhabitants, source consent, season, containment, migration, Age, or other cause>`
- **Evidence and Review Point:** `<observation and change trigger>`

Activity Regimes are descriptive, non-ranked, and region-specific where needed. Time, entry, extraction, death, or a calendar interval does not trigger activity or replenishment by itself.

## Resources, Exchanges, and Extraction

### Dungeon Exchange

- **Exchange ID:** `<stable flow reference>`
- **Source, destination, route, and timing:** `<boundary relationship>`
- **Subject:** `<beings, resource, energy, Mana, information, disease, waste, authority, or consequence>`
- **Scale and uncertainty:** `<qualitative or justified local measure>`
- **Compatibility and transformation:** `<what changes in transit or use>`
- **Control, consent, and interception:** `<Actors, sources, law, and Relationships>`
- **Loss, waste, side effects, and affected parties:** `<causal outcomes>`

### Resource Claim and Dungeon Yield

- **Resource ID and source:** `<actual Stock or Flow owner>`
- **Quality, condition, renewal, and limits:** `<current state>`
- **Access and extraction route:** `<body, Skill, tool, permission, danger, labor, and time>`
- **Custody, ownership, claims, and affected parties:** `<separate facts>`
- **Transformation, transport, loss, and waste:** `<process and consequences>`
- **Dungeon Yield:** `<actual bounded output obtained after resolution>`
- **Post-extraction state:** `<changed Stock, route, ecology, behavior, economy, secrecy, and future availability>`

Resources, valuables, records, services, sanctuary, and cooperation require sources and owners. Exploration, danger, secrecy, death, or clearing creates no loot entitlement or replacement flow.

## Hazards, Protections, and Fair Evidence

Repeat for each trap, ward, barrier, patrol, puzzle, defense, or environmental hazard.

- **Hazard or protection ID:** `<stable reference>`
- **Origin, builder, or causal source:** `<terrain, ecology, construction, Magic, living process, damage, disease, or other owner>`
- **Function and intended subjects:** `<if designed; otherwise actual process>`
- **Trigger and exposure route:** `<what activates or encounters it>`
- **Current condition and maintenance:** `<active, degraded, altered, abandoned, repaired, unknown>`
- **Evidence and discoverability:** `<signals available through compatible senses, expertise, records, or investigation>`
- **Limits and failure modes:** `<where it does not apply or can malfunction>`
- **Avoidance, protection, interruption, repair, or acceptance routes:** `<where causally available>`
- **Consequences and affected owners:** `<injury, environment, relationships, law, resources, or other results>`

Danger follows current conditions, not visitor power. Fair evidence supports meaningful choice where possible; it does not require every hazard to be harmless or solvable by every group.

## Claims, Control, and Relationships

- **Physical custody and occupation:** `<who controls which place or object now>`
- **Legal ownership and jurisdiction:** `<source, scope, recognition, enforcement, and disputes>`
- **Territorial and resource claims:** `<Actors, Populations, Factions, Institutions, and affected parties>`
- **Maintenance and stewardship obligations:** `<promises, roles, resources, and accountability>`
- **Sacred, ancestral, ecological, or Soul relationships:** `<source, participants, meaning, consent, and limits>`
- **Trade, diplomacy, conflict, and access agreements:** `<Relationship and Faction records>`
- **Exploration, salvage, conquest, and classification claims:** `<claimants, evidence, law, opposition, and consequences>`
- **Person-like sources or living bodies:** `<bodily integrity, agency, consent, and owner safeguards>`

Entry, discovery, mapping, clearing, conquest, extraction, inheritance, or killing does not automatically grant ownership, legitimacy, or authority.

## Consequences, Reconfiguration, and Legacy

- **Direct state changes:** `<affected Location, Population, Resource, Ecology, Economy, Faction, Magic, conflict, disease, or other owner references>`
- **Dungeon Reconfiguration:** `<cause, topology or function changed, timing, evidence, and new routes>`
- **Expansion or contraction:** `<Boundary, Basis, conditions, affected land, beings, and Counterforces>`
- **Damage, repair, collapse, recovery, or transformation:** `<causal sequence and surviving functions>`
- **Bodies, remains, possessions, waste, disease, and displacement:** `<persistent owner records>`
- **Pending Consequences:** `<causal pressure, owner, Causal Horizon, and Review Point>`
- **Dungeon Legacies:** `<ruins, altered ecology, records, claims, relationships, magical residue, routes, or other durable remainders>`
- **Historical events:** `<Timeline and Campaign History references>`
- **Next revalidation condition:** `<when Profile accuracy must be reassessed>`

Removing one threat, inhabitant group, Core, controller, or visible process ends only the functions it supported. It does not clear, reset, freeze, refill, or erase the site.

## Knowledge, Maps, Research, and Visibility

- **Factual View:** `<best current world account and protected GM Secret references>`
- **Observer Views:** `<Character, Population, Faction, Institution, source, and player Knowledge records>`
- **Public classification and legend:** `<audience, origin, accuracy, incentives, and consequences>`
- **Map Claims:** `<dated references rather than copied topology>`
- **Research:** `<observations, hypotheses, experiments, evidence, theories, confidence, disproof, and confirmation>`
- **Hidden, disputed, outdated, and unresolved claims:** `<protected state and discovery routes>`
- **Reincarnated or Echo testimony:** `<historical perspective, access, changed embodiment, and current verification>`

An old map, Soul memory, Archive record, official claim, or popular name is evidence, not automatic current Topology or truth.

## Generation Provenance

### Conditional Fields

- **Generation mode:** `<classify | create | reconstruct | expand | revalidate>`
- **Question, scope, horizon, and resolution:** `<brief reference>`
- **Established world inputs:** `<time, place, geography, populations, ecology, resources, economy, law, factions, conflict, Magic, technology, disease, Gates, Ages, Stability, and history as applicable>`
- **Dungeon Basis established:** `<Formation, Boundary, Sustaining Basis, distinct conditions, exchanges, and continuity>`
- **Specialist handoffs and results:** `<owner, question, returned claim, and uncertainty>`
- **Randomly selected ordinary details:** `<committed field, supported options, causal weighting, result, and evidence state>`
- **Facts left open:** `<unsupported regions, contents, participants, routes, and choices>`
- **Corrections, revalidation history, and Review Points:** `<traceable changes>`

Generation establishes a starting state, not a scripted adventure. It cannot predetermine entry, route choice, contact, hostility, discovery, extraction, collapse, or outcome.

## Validation Checklist

- [ ] The repository copy remains blank; every populated Dungeon Profile and map is external.
- [ ] The Dungeon claim establishes Formation, Boundary, Sustaining Basis, distinct conditions, exchanges, continuity, evidence, and uncertainty.
- [ ] Classification remains observer-specific and does not establish one universal Dungeon species, purpose, mind, morality, or owner.
- [ ] A Core, floors, rooms, monsters, traps, bosses, treasure, quests, safe zones, and reset points are absent unless independently sourced.
- [ ] Every unusual spatial, magical, living, technical, agentive, Gate, Age, or Reset claim has a canonical owner.
- [ ] Factual Topology, Dungeon Regions, Access Routes, and dated observer Map Claims remain distinct.
- [ ] Hidden routes, conditions, and contents have prior causal existence and do not move after player choices.
- [ ] Inhabitants retain personhood, agency, populations, society, resources, rights, relationships, and independent owners.
- [ ] A Core or responsive agent knows and controls only what its actual senses, interfaces, authority, and capabilities permit.
- [ ] Resources, inhabitants, structures, and effects have sources; death and elapsed time create no respawn or refresh.
- [ ] Dungeon Yield records actual resolved output, not guaranteed loot or compensation.
- [ ] Hazards follow world state rather than visitor level and preserve discoverable evidence where agency requires it.
- [ ] Entry, mapping, clearing, conquest, extraction, and survival grant no automatic ownership, progression, Soul growth, Evolution, or reward.
- [ ] Damage, extraction, bodies, remains, waste, disease, displacement, topology, claims, and Legacies persist through causality.
- [ ] Removing one Core, controller, threat, or population ends only supported functions and never resets the site.
- [ ] No universal difficulty, depth tier, threat rank, room quota, reward budget, Dungeon XP, or scripted encounter appears.
- [ ] Truth Layers, versions, sources, typed references, Historical events, Pending Consequences, and Review Points are explicit.

## Canonical Dependencies

- [Dungeon Activity](../docs/world-engine/DUNGEON_ACTIVITY.md)
- [Dungeon Generator](../docs/gm/DUNGEON_GENERATOR.md)
- [World Engine](../docs/world-engine/README.md)
- [World-State Variables](../docs/world-engine/WORLD_STATE_VARIABLES.md)
- [Causal Event Chains](../docs/world-engine/CAUSAL_EVENT_CHAINS.md)
- [Simulation Abstraction](../docs/world-engine/SIMULATION_ABSTRACTION.md)
- [Populations](../docs/world-engine/POPULATIONS.md)
- [Resources and Food](../docs/world-engine/RESOURCES_AND_FOOD.md)
- [Ecology and Migration](../docs/world-engine/ECOLOGY_AND_MIGRATION.md)
- [Faction Behaviour](../docs/world-engine/FACTION_BEHAVIOUR.md)
- [Magic](../docs/magic/README.md)
- [Monster Evolution](../docs/monster-evolution/README.md)
- [Soul Weapons](../docs/soul-weapons/README.md)
- [Encounter Generator](../docs/gm/ENCOUNTER_GENERATOR.md)
- [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Timeline Engine](../docs/persistence/TIMELINE_ENGINE.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)

## Extensions and Consumers

Use the [Settlement Record Template](SETTLEMENT_TEMPLATE.md), [Faction Profile Template](FACTION_TEMPLATE.md), [Character Record Template](CHARACTER_TEMPLATE.md), [Species Reference Template](SPECIES_TEMPLATE.md), and [World-Contact and Gate Event Record Template](GATE_EVENT_TEMPLATE.md) for records a Dungeon view references but does not own. Gate-event records remain separate even when a Gate overlaps Dungeon space.

Session preparation, exploration, encounters, extraction, world simulation, Research, continuity review, and validation consume only relevant Profile fields and permitted Truth Layers. Update the Affected Set after play instead of regenerating or resetting unrelated Dungeon state.
