# Settlement Record Template

Use this storage-neutral template to record one inhabited or serviced place as a connected Location without reducing its people, ecology, economy, infrastructure, institutions, factions, or history to one settlement level.

The repository keeps this template blank. Every populated Settlement Record is external campaign state. Completing this form does not create a population, resource, service, government, market, magical source, conflict, or predetermined future.

## Document Control

- **Template owner:** Locations module of the [Campaign Persistence Engine](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md#locations)
- **Primary mechanical owners:** the applicable domains of the [World Engine](../docs/world-engine/README.md), with each linked specialist retaining its own claims
- **Dependencies:** Location identity, Populations, ecology, resources, Infrastructure, Economies, Factions, Institutions, law, Magic, Knowledge, Timeline, and world causality as applicable
- **Extensions:** districts, sites, migration, conflict, disease, advancement, Dungeons, Gates, World Stability, Ages, and Simulation Frames
- **Consumers:** session preparation, travel, world simulation, encounters, NPC and Faction generation, migration, Research, Projects, Timeline, continuity review, and validation
- **Repository boundary:** no named settlement, resident, current resource, price, officeholder, faction, service, hazard, project, or live world value belongs in this blank file

This template owns the settlement's place identity and current location-level view. It links rather than duplicates the authoritative records for people, organizations, resources, capabilities, systems, events, and beliefs.

## Usage Guidance

1. Establish the place boundary, continuity, scope, observation time, and simulation resolution before recording conditions.
2. Link populations and persons by stable ID; never turn residents into one actor, culture, opinion, or capability pool.
3. Record access, distribution, condition, dependencies, and uncertainty for every material resource and service.
4. Keep governance, institutions, factions, law, legitimacy, enforcement, and public belief separate.
5. Treat settlement infrastructure as maintained networks of people, knowledge, resources, sites, tools, and routes, not passive bonuses.
6. Preserve ecological relationships and migration routes; a settlement is habitat and an ecological actor, not outside nature.
7. Separate world truth, observer knowledge, Research, Rumours, and GM Secrets.
8. Use World-State Claims and Simulation Frames only at the resolution needed for current decisions.
9. Version reconstruction, abandonment, occupation, merger, division, or boundary change through explicit continuity analysis.

## Required Field Policy

**Required** fields must be present or carry an explicit `Unknown`, `Not Yet Verified`, `Disputed`, or `Requires Source Recovery` state. **Conditional** fields become required only when their feature exists. **Optional** fields grant nothing when omitted.

## Record Contract

### Required Identity

- **Record ID:** `<stable Location or Settlement ID>`
- **Record type:** `Settlement Record`
- **Authoritative module:** `Locations`
- **Current name:** `<primary current label or explicit unknown>`
- **Aliases and historical names:** `<label, source, audience, interval, and equivalence>`
- **Settlement version:** `<current version>`
- **Location type:** `<camp | hamlet | village | town | city | station | colony | mobile settlement | distributed settlement | site network | other descriptive form>`
- **World and Contact Domain:** `<stable references>`
- **Observation time:** `<Timeline reference>`
- **Current simulation resolution:** `<Focused | Local | Regional | Epochal>`
- **Campaign version:** `<applicable version reference>`

### Required Authority and Provenance

- **Persistence authority:** `<Campaign Canon or other applicable authority>`
- **Truth layer:** `<Campaign Canon | Historical Record | Research | GM Secrets | another valid layer>`
- **Canonical mechanical owners:** `<owner for each claim family>`
- **Status:** `<inhabited | seasonal | mobile | abandoned | occupied | damaged | reconstructing | disputed | inaccessible | other established state>`
- **Effective interval:** `<Timeline references>`
- **Source basis:** `<observation, map, census, generation, adjudication, migration, correction, or explicit unknown>`
- **Last integrated at:** `<Save Point or Transaction reference>`
- **Typed references and dependents:** `<relation type -> stable record ID>`
- **Validation status:** `<Validation Run, review result, warnings, or not yet validated>`

## Place Boundary and Continuity

### Required Fields

- **Spatial boundary:** `<physical, legal, ecological, infrastructural, social, mobile, or network boundary>`
- **Included districts and sites:** `<Location IDs and relation types>`
- **Excluded or overlapping places:** `<Location IDs and boundary disputes>`
- **Position and scale:** `<relative geography, extent, vertical or distributed structure, and uncertainty>`
- **Access routes:** `<roads, paths, waterways, flight, portals, tunnels, migration routes, or other established routes>`
- **Approach and entry conditions:** `<terrain, law, permissions, hazards, time, cost, and accessibility>`
- **Internal movement:** `<routes, barriers, congestion, interfaces, and body-specific access>`
- **Continuity basis:** `<site, inhabitants, infrastructure, institutions, identity, records, relationships, or other surviving structures>`

### Conditional Fields

- **Mobile settlement route:** `<movement pattern, carriers, dependencies, stopping sites, and continuity>`
- **Distributed settlement topology:** `<nodes, links, communication, shared functions, and failure>`
- **Subterranean, aerial, aquatic, extradimensional, or unusual environment:** `<actual laws, access, embodiment, and maintenance>`
- **Jurisdictional boundary:** `<claims, recognizing parties, practical enforcement, and disputes>`

Renaming, damage, abandonment, reconstruction, occupation, or relocation does not automatically preserve or end identity. Record the continuity claim and evidence.

## Physical Environment and Ecology

### Required Fields

- **Terrain and built form:** `<current physical arrangement and sources>`
- **Climate and seasonal pattern:** `<relevant cycles, variation, and uncertainty>`
- **Water, atmosphere, temperature, pressure, light, and other environmental conditions:** `<scope and habitability>`
- **Habitats and ecological functions:** `<ecosystem and species references>`
- **Resident and visiting non-person populations:** `<Population references and roles>`
- **Disturbance regime:** `<fire, flood, storms, predation, magical Drift, maintenance, construction, or other recurring processes>`
- **Waste and externalities:** `<flows, destinations, affected parties, accumulation, and mitigation>`
- **Ecological dependencies:** `<pollination, decomposition, water regulation, prey, symbiosis, soil, Mana, or other functions>`
- **Environmental hazards and safe routes:** `<cause, exposure, evidence, countermeasures, and uncertainty>`

### Conditional Fields

- **Magical or spiritual ecology:** `<Mana Fields, Currents, sources, persons, residue, and specialist references>`
- **Novel Ecology:** `<origin, participating processes, dependencies, and impacts>`
- **Invasive pressure:** `<receiving ecology, disruptive relationship, Counterforces, and evidence>`
- **Dungeon or Gate influence:** `<specialist record, footprint, routes, and current status>`

Civilization is part of ecology. Construction, farming, roads, wards, sewage, markets, and sacred protections create habitats, barriers, corridors, refuge, waste, and dependencies through their actual effects.

## Populations and Residency

Create separate [Population](../docs/world-engine/POPULATIONS.md) references for each demographic question.

### Required Fields

- **Resident populations:** `<Population IDs, membership rules, time, and uncertainty>`
- **Temporary and mobile populations:** `<travelers, seasonal workers, migrants, pilgrims, armies, visitors, or other scoped groups>`
- **Hidden, excluded, unrecognized, or inaccessible populations:** `<protected references and evidence>`
- **Composition differences that matter:** `<species, life stage, body, health, work, legal status, mobility, language, affiliation, access, or other decision-relevant distinction>`
- **Entry and exit flows:** `<birth, formation, death, migration, displacement, discovery, status change, and owner references>`
- **Care and dependency networks:** `<households, institutions, species structures, and Relationships>`
- **Population continuity pressures:** `<habitat, care, teaching, reproduction, recruitment, law, movement, and future viability>`

### Settlement and Integration Fields

- **Arrival groups:** `<Population and migration references>`
- **Habitat and Sustenance compatibility:** `<species-specific profiles and uncertainty>`
- **Shelter, territory, work, care, and institutional access:** `<distribution and barriers>`
- **Legal status and recognition:** `<law, office, audience, and practical enforcement>`
- **Language, custom, communication, and discrimination:** `<separate cultural and relationship claims>`
- **Internal organization and disagreements:** `<Faction and Actor references>`
- **Return, onward, seasonal, and divided-household routes:** `<current options and constraints>`
- **Intergenerational and source-region effects:** `<Timeline and world references>`

Population counts do not create one opinion, culture, action, Skill, Soul, capability, or political mandate. Arrival does not guarantee settlement or integration.

## Resources, Food, and Sustenance

Create one scoped Resource Claim per material use.

- **Resource or Sustenance Profile ID:** `<stable reference>`
- **Purpose and users:** `<who needs or uses it and why>`
- **Stock or Flow:** `<current finding, unit if justified, observation time, and uncertainty>`
- **Source and renewal:** `<origin, extraction, production, regeneration, seasonality, and limits>`
- **Access and control:** `<custody, law, relationship, compatibility, price, danger, and exclusions>`
- **Quality and condition:** `<fitness for use, contamination, spoilage, or processing need>`
- **Transformation and labor:** `<Skills, Professions, tools, Magic, infrastructure, and time>`
- **Storage and transport:** `<capacity, route, losses, maintenance, and security>`
- **Distribution:** `<who receives, controls, pays, is excluded, or bears risk>`
- **Dependencies and substitutes:** `<critical inputs, alternatives, compatibility, and tradeoffs>`
- **Waste and externalities:** `<outputs, affected places and populations, and recovery>`
- **Current pressure and Review Point:** `<scarcity, surplus, volatility, interruption, or other cause>`

Presence does not prove usable access. Aggregate adequacy cannot hide unequal distribution, incompatible sustenance, or a blocked route.

## Infrastructure and Services

Create one Infrastructure record per maintained function.

- **Infrastructure ID and function:** `<stable reference and bounded service>`
- **Sites and network:** `<Location nodes, routes, interfaces, and coverage>`
- **Operators and users:** `<Actors, Professions, Institutions, populations, and access>`
- **Knowledge and standards:** `<procedures, records, training, and compatibility>`
- **Tools, organisms, sources, and materials:** `<typed owner references>`
- **Function-specific capacity:** `<current service under stated conditions; never an infrastructure level>`
- **Distribution and exclusion:** `<who benefits, controls, pays, or lacks access>`
- **Condition and maintenance:** `<repair, inspection, replacement, labor, and timing>`
- **Dependencies and bottlenecks:** `<source, route, specialist, standard, environment, and alternatives>`
- **Failure modes and consequences:** `<interruption, degradation, safety, traces, and affected systems>`
- **Fallbacks and recovery:** `<maintained alternatives and actual restoration routes>`
- **Technical Dependency, Path Dependence, or Lock-In:** `<scope, cause, and practical switching barriers>`

Examples may include shelter, water, sanitation, roads, ports, farms, workshops, archives, hospitals, wards, laboratories, schools, communication, waste systems, and emergency services. Listing a service does not prove that it functions or reaches everyone.

## Economy and Material Access

### Required or Conditional Fields

- **Forms of allocation and exchange:** `<markets, gifting, rationing, obligation, household provision, command, commons, credit, barter, or other actual routes>`
- **Production Claims:** `<goods or services, inputs, actors, capacity, and constraints>`
- **Exchange Claims:** `<parties, objects, terms, access, friction, and completion>`
- **Allocation Claims:** `<controller, recipients, rule, enforcement, distribution, and consequence>`
- **Ownership and Control Claims:** `<legal, practical, customary, contested, and custodial states>`
- **Exchange media and currencies:** `<scope, issuer, acceptance, convertibility, and failure>`
- **Purchasing or participation access:** `<income, status, relationships, information, transport, discrimination, and alternatives>`
- **Prices and value evidence:** `<time, place, transaction conditions, assumptions, and uncertainty>`
- **Contracts, credit, debt, tax, and public provision:** `<Institution and Relationship references>`
- **Concentration and inequality:** `<control, dependence, distribution, and affected populations>`
- **Informal, hidden, and illicit activity:** `<protected records, routes, and consequences>`

The settlement has no single wealth, prosperity, technology, or economy score. Value and access are claim-specific and can differ sharply among populations.

## Governance, Institutions, Factions, and Law

### Required Fields

- **Institutions:** `<Institution IDs, roles, procedures, resources, memory, and current capacity>`
- **Factions:** `<Faction Profile IDs and exact scopes>`
- **Offices and representatives:** `<Actor IDs, mandates, jurisdiction, information, accountability, and disputes>`
- **Decision routes:** `<formal and practical procedures>`
- **Law and custom:** `<rule, source, subjects, jurisdiction, interpretation, enforcement, and exceptions>`
- **Public provision and obligations:** `<who is responsible, how support moves, and current limits>`
- **Authority, legitimacy, expertise, consent, and enforcement:** `<separate audience-specific claims>`
- **Disputes and excluded parties:** `<claims, actors, procedures, risks, and uncertainty>`

### Conditional Fields

- **External rule, occupation, tribute, or protection:** `<Faction, Institution, conflict, and Relationship references>`
- **Religious, divine, spiritual, or Soul-Title recognition:** `<source, Jurisdiction, audience, and limits>`
- **Monster-human or multispecies governance interfaces:** `<bodies, communication, law, culture, and personhood safeguards>`
- **Emergency authority:** `<trigger, scope, duration, oversight, and expiry>`

A settlement is not one faction or government. A government decision is not implemented until particular actors mobilize and execute it.

## Magic, Technology, and Specialized Systems

Link only established systems and effects.

- **Magic-World Profiles:** `<scope, established cause, direct change, source, footprint, distribution, dependencies, failure, and uncertainty>`
- **Technical Systems:** `<function, validated procedure, users, inputs, standards, support, and limits>`
- **Magical Infrastructure:** `<source, Access, Channels, operators, maintenance, footprint, agency, and failure>`
- **Research and adoption:** `<Research, Institution, Profession, replication, criticism, and access references>`
- **Compatibility and interfaces:** `<species, body, language, tool, magical law, standard, and exclusions>`
- **Automation:** `<delegated function, owner, oversight, inputs, failure, displaced work, and accountability>`
- **Knowledge distribution:** `<who understands use, repair, validation, or only output>`
- **Dependency and alternatives:** `<critical routes, lock-in, substitutions, and consequences>`

Magic and technology do not create one advancement level, grant personal Development, erase labor, bypass source agency, or guarantee equitable access.

## Safety, Conflict, Disease, and Hazards

### Conditional Fields

- **Ordinary safety and emergency routes:** `<Actors, Institutions, infrastructure, communication, and access>`
- **Hazards:** `<cause, exposure, vulnerability, warning, response, persistence, and uncertainty>`
- **Civil Unrest, Armed Conflict, War, or occupation:** `<specialist records, participants, objectives, control, effects, and aftermath>`
- **Disease Processes:** `<transmission, hosts, environment, intervention, knowledge, and specialist references>`
- **Crime, coercion, and interpersonal harm:** `<Actor, Relationship, law, Faction, and consequence references>`
- **Dungeon activity:** `<Dungeon record, Activity Regime, routes, effects, and nearby relationships>`
- **Gate or world-contact conditions:** `<Gate Channel, parties, interface, traffic, closure, and legacies>`
- **World Stability or Reset effects:** `<qualified specialist records and current footprint>`
- **Preparedness and recovery:** `<actual plans, people, resources, maintenance, communication, and tested limits>`

Threats are not level-scaled to players. Safety is not one rating, and the presence of danger does not predetermine an encounter or disaster.

## Relationships, Culture, and Daily Life

### Conditional Fields

- **Relationship networks:** `<household, neighborhood, professional, religious, factional, ecological, and interspecies references>`
- **Cultures and traditions:** `<separate historical practices, variation, participants, meanings, and change>`
- **Languages and communication:** `<routes, access, interpretation, embodiment, and barriers>`
- **Care, education, work, worship, recreation, and gathering:** `<Actors, Institutions, sites, access, and distribution>`
- **Social positions and recognition:** `<audience, source, rights, duties, legitimacy, and contest>`
- **Public events and routines:** `<Timeline, Institution, Population, and resource dependencies>`
- **Tensions and solidarities:** `<Relationship and Faction records rather than settlement personality>`

Place influences opportunity and pressure. It does not assign one morality, culture, personality, opinion, or life path to every resident.

## World-State Claims and Simulation

Create only decision-relevant [World-State Variables](../docs/world-engine/WORLD_STATE_VARIABLES.md).

### World-State Claim

- **Question and purpose:** `<decision or consequence being supported>`
- **Subject, scope, time, and resolution:** `<bounded claim>`
- **State owner and type:** `<Stock, Flow, Capacity, Pressure, Relationship, Constraint, Threshold Condition, Legacy Condition, or specialist type>`
- **Current finding and trend:** `<qualitative or justified local measure>`
- **Drivers and Counterforces:** `<causes, actors, and conditions>`
- **Dependencies and distribution:** `<what sustains or limits it and who is affected>`
- **Delay, momentum, and persistence:** `<time behavior>`
- **Recovery route and Legacy:** `<actual possibilities and remaining consequences>`
- **Evidence and information views:** `<world truth, observations, beliefs, and uncertainty>`
- **Review Point:** `<condition requiring reassessment>`

### Simulation Frame

- **Simulation Frame ID:** `<stable external reference>`
- **Question, scope, interval, and Causal Horizon:** `<why simulation is needed>`
- **Resolution:** `<Focused | Local | Regional | Epochal>`
- **Loaded state and Material Exceptions:** `<authoritative references>`
- **Active actors, processes, dependencies, and Pending Consequences:** `<inputs>`
- **Review Point:** `<event, threshold, decision, or time condition>`
- **Simulation Delta:** `<established changes, causes, uncertainty, handoffs, legacies, and next review>`

Simulation summarizes ordinary activity without granting omniscience, erasing minorities, scripting outcomes, or averaging away a material exception.

## Knowledge, Research, and Visibility

- **World truth:** `<authoritative facts or protected GM Secret references>`
- **Character and player knowledge:** `<observer-specific Knowledge records>`
- **Public records, maps, censuses, and claims:** `<source, access, omissions, age, and reliability>`
- **Research:** `<observation, hypothesis, evidence, theory, confidence, confirmation, disproof, and owner references>`
- **Rumours and mistaken beliefs:** `<source, spread, audience, and consequences>`
- **Concealed places, populations, systems, and risks:** `<protected references and fair discovery routes>`
- **Information infrastructure:** `<messengers, archives, signs, magical channels, language, and access>`

Missing information remains unknown. A map, census, law, legend, or official report is evidence, not automatic world truth.

## History, Change, and Continuity

### Required Fields

- **Formation or earliest supported history:** `<Historical references and uncertainty>`
- **Material events:** `<Timeline IDs for migration, construction, disaster, conflict, reform, abandonment, occupation, contact, or other changes>`
- **Prior settlement versions:** `<stable IDs, intervals, and continuity relationships>`
- **Destroyed, lost, replaced, or transformed structures:** `<owner records and consequences>`
- **Surviving legacies:** `<infrastructure, ecology, populations, institutions, debts, records, relationships, residue, names, or memories>`
- **Authorized corrections and retcons:** `<provenance and supersession>`
- **Pending Consequences:** `<causal pressure and Review Points>`

### Conditional Continuity Changes

- **Expansion or contraction:** `<changed boundary, populations, systems, and identity evidence>`
- **Merger or division:** `<predecessor and successor places, claims, assets, residents, and histories>`
- **Abandonment and resettlement:** `<ended and renewed functions, surviving place identity, and new relationships>`
- **Destruction and reconstruction:** `<what was lost, what remained, who rebuilt, and which identity is claimed>`
- **Occupation or rule change:** `<world continuity versus political control>`
- **Age transition or Reset:** `<specialist persistence, loss, inheritance, and changed law>`

Campaign History is append-only except for authorized factual correction. Current narration cannot restore a destroyed district, erased service, extinct population, or former government without an in-world cause.

## Validation Checklist

- [ ] The repository copy remains blank; every populated Settlement Record is external.
- [ ] Place identity, boundary, scope, time, resolution, version, and continuity evidence are explicit.
- [ ] Residents remain persons and scoped populations rather than one actor, culture, opinion, or capability pool.
- [ ] Location, Population, Resource, Infrastructure, Economy, Faction, Institution, Magic, and specialist facts retain separate owners.
- [ ] Stocks, Flows, capacity, access, quality, distribution, control, dependencies, waste, and uncertainty remain distinct.
- [ ] Infrastructure and services identify operators, inputs, maintenance, coverage, failure, and affected parties.
- [ ] Governance, faction, office, authority, legitimacy, law, consent, expertise, and enforcement are not collapsed.
- [ ] Ecology includes settlement-created habitat, barriers, corridors, waste, dependencies, and disturbance.
- [ ] Migration distinguishes arrival, settlement, integration, return, onward movement, and source-region effects.
- [ ] Economy has no universal wealth or prosperity score; prices and access are scoped and evidenced.
- [ ] Magic and technology preserve sources, embodiment, Skills, labor, maintenance, agency, footprints, and alternatives.
- [ ] Conflict, disease, Dungeons, Gates, Stability, and Resets remain under their specialist owners.
- [ ] World-State Claims and Simulation Frames preserve causality, resolution, Material Exceptions, and Review Points.
- [ ] World truth, Knowledge, Research, Rumours, records, and GM Secrets remain separate.
- [ ] Destruction, abandonment, reconstruction, occupation, division, merger, and renaming receive explicit continuity analysis.
- [ ] No settlement level, encounter scaling, scripted event, universal technology grade, or live campaign value appears.

## Canonical Dependencies

- [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [World Engine](../docs/world-engine/README.md)
- [World-State Variables](../docs/world-engine/WORLD_STATE_VARIABLES.md)
- [Populations](../docs/world-engine/POPULATIONS.md)
- [Resources and Food](../docs/world-engine/RESOURCES_AND_FOOD.md)
- [Economies](../docs/world-engine/ECONOMIES.md)
- [Ecology and Migration](../docs/world-engine/ECOLOGY_AND_MIGRATION.md)
- [Faction Behaviour](../docs/world-engine/FACTION_BEHAVIOUR.md)
- [Technology and Magical Advancement](../docs/world-engine/TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md)
- [Simulation Abstraction](../docs/world-engine/SIMULATION_ABSTRACTION.md)
- [Magic-World Interactions](../docs/magic/WORLD_ENGINE_INTERACTIONS.md)
- [Relationship Memory Engine](../docs/persistence/RELATIONSHIP_MEMORY_ENGINE.md)
- [Timeline Engine](../docs/persistence/TIMELINE_ENGINE.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)

## Extensions and Consumers

Use the [Faction Profile Template](FACTION_TEMPLATE.md), [Character Record Template](CHARACTER_TEMPLATE.md), [Species Reference Template](SPECIES_TEMPLATE.md), [Dungeon Profile Template](DUNGEON_TEMPLATE.md), and [World-Contact and Gate Event Record Template](GATE_EVENT_TEMPLATE.md) for their own records. A settlement view links those records and never becomes their competing owner.

Session preparation, encounters, travel, world simulation, migration, continuity review, Research, and validation consume only the modules and Truth Layers relevant to their question. Update the Affected Set after play rather than rewriting the whole settlement.
