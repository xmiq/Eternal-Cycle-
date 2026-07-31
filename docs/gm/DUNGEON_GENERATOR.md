# Dungeon Generator

## Purpose

This document defines how a Game Master creates or reconstructs a campaign-local Dungeon through the completed [Dungeon Activity](../world-engine/DUNGEON_ACTIVITY.md) framework.

The generator produces a coherent external Dungeon Profile from world-supported causes. It is not a room table, map algorithm, encounter sequence, loot schedule, difficulty curve, adventure module, or source of campaign state inside this repository.

## Core Rule

Generate the causes that make a bounded place materially distinct before generating its contents.

A valid Dungeon begins with formation, boundary, Sustaining Basis, internal conditions, exchanges, and continuity. Topology, inhabitants, resources, hazards, Cores, and activity follow those causes. They do not appear because a Dungeon is expected to contain them.

## Ownership

This document owns:

- selecting a Dungeon-generation scope;
- building and validating a Dungeon Basis;
- deriving a causal topology draft and Access Routes;
- assembling existing world-system outputs into one external Dungeon Profile;
- testing Dungeon classification, coherence, information, and placement;
- establishing generator handoffs to Dungeon Activity, Encounters, and specialist systems;
- bounded randomization guidance for supported details.

It does not own:

- the definition of Dungeon, Formation, Boundary, Sustaining Basis, Core, Topology, Activity, Collapse, Transformation, or Legacy;
- current Population, Ecology, Resource, Economy, Faction, War, Disease, Technology, Magic, Soul, or Gate results;
- monster or NPC creation;
- faction or world-event generation;
- Encounter framing or outcome;
- rewards or progression;
- live Dungeon state in this repository.

## Key Terms

### Dungeon Generation Brief

A **Dungeon Generation Brief** is the external input for one generation pass, stating the exact site question, world context, generation scope, known evidence, information views, preparation horizon, and canonical limits.

It does not prescribe a layout, threat, purpose, reward, or desired Encounter.

### Dungeon Basis

A **Dungeon Basis** is the validated formation history, practical boundary, Sustaining Basis, distinct internal conditions or processes, boundary exchanges, and continuity that make one current Dungeon claim possible.

If those elements do not support the classification, the result is a different kind of site rather than a failed Dungeon.

### Dungeon Seed

A **Dungeon Seed** is an unvalidated preparation possibility combining a possible Dungeon Basis, current world placement, topology, participants, activity, resources, hazards, claims, and information.

It remains a draft until all source, coherence, ownership, causality, and continuity checks pass.

### Dungeon Generation Pass

A **Dungeon Generation Pass** is one bounded procedure that resolves only the Dungeon facts needed for a stated preparation horizon while preserving hidden truth, unresolved branches, specialist handoffs, and future change.

Later passes may expand detail when play makes another region, route, process, or history material. They cannot contradict established facts without a valid world change or explicit correction.

### Dungeon Region

A **Dungeon Region** is a bounded part of Dungeon Topology grouped because its conditions, functions, participants, sustaining relations, or Access Routes are usefully resolved together.

Region is a map and simulation abstraction. It does not imply a room, floor, biome, encounter tier, or independent Dungeon.

## Generation Scopes

### Classify an Existing Site

Test whether a known cave, ruin, settlement, machine, forest, organism, archive, fortress, magical field, or spatial system currently meets the Dungeon definition.

The result may be:

- a valid Dungeon;
- several overlapping Dungeons;
- one Dungeon nested inside a wider site;
- a former Dungeon that collapsed or transformed;
- a culturally disputed Dungeon label;
- an ordinary or extraordinary site that is not a Dungeon.

Classification is allowed to fail.

### Generate a New Campaign-Local Dungeon

Create a world-valid site from an established formation route and current placement. This scope still begins from existing world domains, resources, people, magic, history, and causality.

"New" means newly defined for the campaign, not newly created without a world history.

### Reconstruct an Old or Partly Known Dungeon

Begin from dated maps, ruins, testimony, records, Soul memory, Archive traces, surviving infrastructure, and current observations. Separate prior state from current truth.

Do not fill record gaps with whatever makes exploration dramatic.

### Generate One Dungeon Region

Expand only the region, route, interface, or process now relevant to play. Preserve established wider topology and dependencies while leaving immaterial details unresolved.

### Revalidate a Changed Dungeon

After Collapse, Transformation, settlement, extraction, war, disease, Age transition, Gate event, or another major change, rebuild the current model from preserved continuity and new causes rather than resetting the site.

## Dungeon Generation Brief

```markdown
### Dungeon Generation Brief

Question and scope:
- classify, create, reconstruct, expand, or revalidate:
- exact decision or preparation need:
- spatial and temporal horizon:
- useful simulation resolution:

World context:
- time, place, geography, climate, and surrounding routes:
- Populations, Ecology, Resources, Economy, law, factions, and conflict:
- Mana, Magic, Technology, Disease, Gates, Ages, and Stability:
- known history and current pressures:

Known site evidence:
- observed boundary and conditions:
- origin claims and surviving records:
- known participants, routes, resources, hazards, and activity:
- dated maps and prior classifications:

Information views:
- Factual View:
- relevant Observer Views:
- player-facing evidence:
- hidden, disputed, outdated, and unresolved claims:

Boundaries:
- canonical and Provisional limits:
- facts that must remain open:
- later generators or owners not authorized in this pass:
```

The populated brief belongs in the external Campaign Record.

## Dungeon Basis Requirements

### Formation

Establish how the place acquired its current distinct boundary, Sustaining Basis, conditions, exchanges, and continuity.

Formation may be:

- natural;
- constructed;
- ecological;
- living;
- magical;
- spiritual or Divine;
- technical;
- social or institutional;
- produced by war, disaster, disease, extraction, migration, Age change, Reset consequence, or Gate contact;
- layered across several causes and periods.

Origin does not determine present purpose, ownership, morality, activity, or inhabitants.

### Practical Boundary

Identify where conditions change enough to matter for access, simulation, or consequence.

A boundary may be sharp or gradual, stable or moving, visible or hidden, physical or relational, singular or layered. It need not block passage.

### Sustaining Basis

Identify what currently preserves the site's distinct conditions or recurring processes.

For every material component, record:

- source and function;
- inputs and dependencies;
- maintenance or renewal;
- current condition;
- redundancy or coupling;
- operator or participant agency;
- failure and recovery routes;
- evidence and uncertainty.

"Ancient magic" and "Dungeon energy" are incomplete unless the relevant Magic owner establishes the source relation.

### Distinct Internal Condition or Process

At least one internal pattern must materially differ from its surroundings. Examples include pressure, ecology, topology, magical conditions, habitation, preservation, regulation, extraction, transformation, governance, or outward influence.

Danger is neither required nor sufficient.

### Boundary Exchange

Identify how beings, matter, energy, Mana, information, disease, waste, authority, or consequence cross the boundary.

A perfectly sealed site requires an explicit mechanism and still may exchange gravity, heat, time, or information unless its owner establishes otherwise.

### Continuity

The site must persist or recur enough for its identity to matter beyond one isolated moment. Record what preserves identity through repair, occupation, dormancy, damage, or Transformation.

Continuity is not immunity to change.

## Generation Procedure

### Step 1: State the Site Question

Name what play or simulation needs to know.

Examples:

- Does this abandoned reservoir currently qualify as a Dungeon?
- What supports the sealed archive beneath the city?
- Which routes through the living leviathan are currently usable?
- What remains after a faction lost control of the fortress?

Do not begin from "How many floors?", "What boss lives here?", or "What reward should be at the end?"

### Step 2: Search Established State

Load existing geography, structures, settlements, populations, resources, ecology, magic, factions, law, history, records, maps, World Legacies, and prior consequences.

Do not create a second ruin, cave system, settlement, Gate, or ancient civilization when an established source already owns the site's origin.

### Step 3: Establish Formation History

Build the smallest causal timeline that explains the current site:

- original environment or construction;
- major inhabitants, maintainers, or controllers;
- sustaining systems and resource routes;
- damage, adaptation, repair, abandonment, occupation, or transformation;
- current relation to the surrounding world.

History supports current facts. It does not grant treasure, mystery, or importance by age alone.

### Step 4: Test Dungeon Classification

Identify:

1. practical boundary;
2. Sustaining Basis;
3. distinct internal condition or process;
4. boundary routes and exchanges;
5. continuity;
6. observer evidence and disagreement.

If the test fails, classify the site truthfully and stop Dungeon generation. Ordinary sites remain playable.

### Step 5: Build the Dungeon Basis

Record Formation, Boundary, Sustaining Basis, internal distinction, exchanges, and continuity. Assign every claim to its owner.

If one component is genuinely unresolved, preserve supported alternatives and a Resolution Trigger rather than choosing a convenient answer.

### Step 6: Establish Current Spatial Scope

Set the geographic, architectural, ecological, biological, magical, or relational extent currently supported.

Separate:

- the Dungeon Boundary;
- surrounding influence;
- legal or faction claims;
- rumors and mapped assumptions;
- linked but distinct Dungeons;
- world-contact Gates or Soul structures governed elsewhere.

### Step 7: Derive Dungeon Regions

Group space by material condition, function, participant use, sustaining relation, or route. Begin at the coarsest truthful resolution.

A region may be a watershed, district, organ, tunnel network, maintenance layer, habitat, dream interface, archive sector, or moving convoy of structures. It need not be rectangular or contiguous if the source supports another relation.

### Step 8: Derive Topology

Connect regions through actual paths, flows, interfaces, distances, barriers, cycles, and dependencies.

Topology follows construction, geology, growth, erosion, habitation, transit, ecology, magic, damage, and repair. It does not follow a linear difficulty sequence.

Record uncertain maps as Dungeon Map Claims rather than Factual Topology.

### Step 9: Establish Access Routes

For each relevant route, state:

- what kind of being, object, signal, resource, or effect can use it;
- direction and destination;
- timing and environmental conditions;
- body, sense, equipment, Skill, permission, relationship, key, ritual, or source requirements;
- supply, navigation, communication, and return conditions;
- barriers, observers, defenders, hazards, and Counterforces;
- current evidence and uncertainty.

An open door is not a usable route for every body or purpose.

### Step 10: Establish Dungeon Conditions

Add only conditions produced by the Dungeon Basis, surrounding world, inhabitants, infrastructure, damage, or ongoing processes.

For each material condition, record source, scope, timing, affected subjects, limits, counterplay, evidence, and owner.

Do not fill regions with one decorative hazard each.

### Step 11: Establish Activity Drivers and Regimes

Identify what currently sustains or changes internal processes:

- resource flow or depletion;
- maintenance or neglect;
- habitation, migration, reproduction, work, trade, worship, or conflict;
- environmental cycles;
- magical or technical operation;
- extraction, intrusion, damage, repair, disease, or source decisions;
- Gate, Age, or Reset conditions through their owners.

Classify activity descriptively. Dormancy, responsiveness, instability, expansion, contraction, and recovery may coexist in different regions.

### Step 12: Establish Inhabitants and Participants

Use existing campaign actors and Populations. If new individuals or monsters are needed, hand off to [NPC Generation](NPC_GENERATOR.md) or [Monster Generation](MONSTER_GENERATOR.md) with world-supported inputs.

Record:

- origin or arrival route;
- needs and habitats;
- current activity;
- relationships, territories, institutions, and claims;
- information and agency;
- effects on and dependence upon the Dungeon Basis;
- ability to enter, leave, reproduce, maintain, or survive.

Do not populate a site by room role or threat budget.

### Step 13: Establish Cores, Controllers, and Maintainers Only if Supported

If one component coordinates, anchors, supplies, records, or regulates a function, describe it as a Dungeon Core for that stated function.

Separate:

- Core function;
- actual control;
- ownership and custody;
- personhood and agency;
- information access;
- dependencies and failure effects;
- independent inhabitants and systems.

Do not create a Core merely to provide a weak point or final objective.

### Step 14: Establish Resources and Exchanges

For each material resource, establish:

- source, Stock or Flow, quality, and location;
- renewal, depletion, transformation, waste, and loss;
- access and extraction process;
- claimants, custody, law, consent, and affected parties;
- economic and ecological consequences;
- information and uncertainty.

Treasure is ordinary property, resource, record, artifact, or gift under its owners. It is not a Dungeon reward category.

### Step 15: Establish Structures, Hazards, Traps, and Protections

For every material feature, state:

- origin and intended or emergent function;
- source and current condition;
- trigger and target assumptions;
- maintenance and supply;
- evidence and detection routes;
- failure modes;
- compatible counters;
- affected subjects and consequences.

An abandoned defense may be degraded, misaligned, harmless, or more dangerous through damage. It does not remain perfectly tuned because it is a trap.

### Step 16: Establish Claims, Law, and Relationships

Record current custody, access, ownership claims, residence, territory, sacred relationships, contracts, treaties, regulation, salvage rules, occupation, and disputes.

Exploration and classification do not erase inhabitants or transfer rights. An external group calling the site a Dungeon does not prove its claim.

### Step 17: Establish Information Views

Separate:

- Factual current Dungeon model;
- inhabitant, maintainer, claimant, expert, and visitor Observer Views;
- dated Dungeon Map Claims;
- player-facing evidence;
- hidden established truth;
- false classifications and theories;
- Record Gaps;
- genuinely unresolved branches.

Old maps and prior-life memories are evidence about a former state, not automatic current truth.

### Step 18: Establish Current Placement and World Effects

Connect the Dungeon to surrounding:

- transport, settlement, law, institutions, and factions;
- Populations, Ecology, Migration, Resources, and Economy;
- Disease, Technology, Magic, war, and unrest;
- World Stability referents, Ages, Resets, Gates, and legacies.

The site exists independently of player discovery and continues to exchange consequences.

### Step 19: Test Capability and Fair Evidence

Use [Capability Assessment](../progression/CAPABILITY_ASSESSMENT.md) only for concrete access, traversal, investigation, survival, repair, negotiation, extraction, or containment questions.

No Dungeon has one difficulty. Different bodies, tools, information, permissions, relationships, Skills, and objectives create different findings.

Material hidden danger requires prior existence and perspective-appropriate evidence where meaningful choice depends on recognizing its category.

### Step 20: Run the Coherence Audit

Check:

- every region and condition follows a source;
- topology follows formation and current change;
- routes are actor- and condition-specific;
- Sustaining Basis inputs and dependencies are viable;
- inhabitants and resources have origins and continuity;
- Cores own only stated functions;
- hazards and protections have evidence and counterplay;
- claims and personhood are preserved;
- no content is level-scaled, respawned, or reward-shaped;
- all specialist claims are handed to their owners.

### Step 21: Create the External Dungeon Profile

Use the existing [Dungeon Profile](../world-engine/DUNGEON_ACTIVITY.md#dungeon-profile) schema, adding only fields needed for the preparation horizon.

Record sources, dates, evidence states, uncertainty, and Review Points. Populated Profiles and maps remain external.

### Step 22: Hand Off to Play and Simulation

Use:

- [Dungeon Activity](../world-engine/DUNGEON_ACTIVITY.md#dungeon-activity-procedure) to advance processes;
- [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md) for resolution and elapsed time;
- [Encounter Generation](ENCOUNTER_GENERATOR.md) when a source becomes relevant to a player decision;
- [Consequence Resolution](CONSEQUENCE_RESOLUTION.md) after resolved effects;
- specialist owners for every capability, actor, resource, magical, Soul, or world claim.

Generation does not predetermine entry, route choice, conflict, discovery, extraction, Collapse, or outcome.

## Dungeon Profile Extension

The canonical Dungeon Profile in [Dungeon Activity](../world-engine/DUNGEON_ACTIVITY.md#dungeon-profile) remains authoritative. A generator may use this external extension when provenance matters:

```markdown
### Dungeon Generation Record: <external campaign identifier>

Generation scope:
- question, preparation horizon, and simulation resolution:
- classify, create, reconstruct, expand, or revalidate:

Dungeon Basis:
- Formation timeline and evidence:
- practical Boundary and classification audience:
- Sustaining Basis components and dependencies:
- distinct conditions or processes:
- boundary exchanges:
- continuity and identity:

Generation provenance:
- established inputs:
- specialist handoffs and results:
- randomly selected fields and committed option sets:
- hidden, disputed, outdated, and unresolved claims:
- corrections and revalidation history:

Topology support:
- Dungeon Regions and why they are grouped:
- route and connection causes:
- dated Dungeon Map Claims:
- unresolved topology and Review Points:

Continuity:
- current Activity Drivers and Regimes:
- Pending Consequences and World Legacies:
- next revalidation condition:
```

This extension is not a second Profile and contains no required room count, floor sequence, threat rating, or reward budget.

## Topology and Map Generation

### Derive, Do Not Decorate

Begin from functions and processes:

- water follows gradients and channels;
- inhabitants build around movement, work, defense, care, and resources;
- organisms grow through anatomy and environment;
- mines follow deposits and extraction history;
- archives follow storage, access, preservation, and administration;
- fortresses follow terrain, doctrine, supply, and construction;
- magical spaces follow established sources, anchors, rules, and maintenance.

Visual variety may follow these causes. It cannot replace them.

### Preserve Multiple Kinds of Connection

Connections may carry different things:

- bodies;
- water, air, heat, waste, or food;
- Mana or magical influence;
- signals, sight, sound, memory, or dreams;
- authority, permission, or relationship;
- disease or ecology;
- maintenance access;
- Gate contact.

Two regions may be adjacent but inaccessible to one body, or distant while tightly connected by resource flow.

### Map Claims Are Observer Records

Every map identifies maker, date, method, purpose, scale, route assumptions, omissions, and uncertainty.

A map can be accurate and still unusable for a different body, season, direction, legal status, or objective. Player annotations update their record, not Dungeon Topology by declaration.

### Reconfiguration

Prepared topology changes only through established causes such as collapse, repair, growth, excavation, flooding, migration, source action, or magic.

Do not move a secret door, unvisited room, hazard, resource, or objective after player choices to preserve a planned route.

## Selection and Randomization

Random generation may select among world-supported details when deterministic causes do not settle them.

### Valid Fields

- which of several plausible formation periods or builders applies when genuinely unresolved;
- ordinary geological or architectural variation;
- a supported region connection or damaged route;
- current maintenance condition within established history;
- location of a resource within its real deposit or production area;
- which active inhabitant uses a shared route;
- timing within an established cycle;
- one of several supported false map assumptions;
- a minor sensory or visual feature caused by existing materials.

### Invalid Fields

- whether the site has a Core, boss, treasure room, final chamber, safe zone, or monster merely by convention;
- new inhabitants, resources, artifacts, Mana, structures, or hazards without sources;
- level-scaled opposition or a universal danger tier;
- personality, consent, hostility, or decisions of inhabitants;
- Evolution, Skills, Soul growth, Soul Weapon awakening, or rewards;
- hidden geometry after seeing a player's route;
- automatic replenishment or reset;
- current faction, population, ecology, or economic state outside its owner.

Before resolving, commit the exact field, supported options, causal weighting, evidence status, and recording procedure. The result establishes only that field.

Do not reroll until the Dungeon is larger, stranger, harder, richer, more symmetrical, or better suited to a prepared plot.

## Inhabitants and Agentive Dungeons

### Inhabitants

Inhabitants remain world participants, not contents.

Use Population, Ecology, Monster, NPC, Faction, and social owners to establish their:

- origin and continuity;
- bodies, needs, Development, and Skills;
- relationships, cultures, institutions, and territories;
- current information, objectives, and activity;
- rights, claims, consent, and conflict;
- effects on and dependence upon the Dungeon.

### Agentive Dungeon Claims

A responsive site is not automatically one person.

Identify whether agency belongs to:

- one living being;
- a spirit, construct, god, or Weapon Soul;
- a distributed mind;
- a colony, symbiosis, council, institution, or network of people;
- several independent controllers;
- an automated system without personhood;
- a mix whose boundaries remain uncertain.

Each agent knows and controls only what its actual senses, communication, authority, interfaces, and capabilities permit.

Dungeon classification does not give an agent ownership of all inhabitants, regions, resources, or visitors.

## Resources, Treasure, and Extraction

Generation may place only resources with sources and viable histories.

For hidden valuables, ask:

- Who produced, gathered, inherited, lost, stored, guarded, concealed, or abandoned them?
- Why are they still present?
- What preserved or degraded them?
- Who knows or claims them?
- What access and extraction are required?
- What is removed, damaged, exposed, or disturbed?
- Can they renew, and through which route?

Valuable information, sanctuary, cooperation, access, and services may be Dungeon Yields. They are not guaranteed rewards.

Removing a resource updates Stocks, Flows, ownership, ecology, Economy, behavior, and future availability through their owners. It does not trigger replacement loot.

## Hazards, Traps, and Fairness

Danger follows current conditions rather than visitor power.

A hazard or trap is fairly generated when it has:

- prior origin and current state;
- a supported trigger and affected subject;
- evidence available through compatible senses, expertise, records, or investigation where meaningful choice requires it;
- limits and failure modes;
- compatible avoidance, protection, interruption, repair, or acceptance routes where causally possible;
- consequences owned by the affected systems.

Fair does not mean harmless, solvable by every group, or designed as a test. A natural collapse may be indifferent to visitors. An old defense may target assumptions no longer true. An intelligent maintainer may adapt after receiving information.

## Dungeon Change After Generation

A generated Dungeon is a starting state, not a frozen module.

Advance it through:

- maintenance and decay;
- inhabitants and external actors;
- Population, Ecology, Resource, and Economy changes;
- extraction and waste;
- construction, repair, damage, and collapse;
- Disease, Magic, Technology, factions, war, and law;
- Gate, Age, Reset, and Stability relations;
- player actions and their consequences.

When change makes the current Profile misleading, perform a Revalidation pass. Preserve prior maps, ruins, absences, relationships, consequences, and Dungeon Legacies.

## Cross-System Interfaces

### Soul Engine and Reincarnation

Soul memory may preserve old maps, relationships, perspective, or recognition only through its owners. It does not update Topology, restore access, return property, create checkpoints, or immunize a new body against conditions.

### Development and Skills

Generation may establish opportunities for navigation, research, negotiation, repair, survival, craft, or combat. Development and Skills still require current embodied practice, evidence, integration, and maintenance. Rooms, danger, discovery, killing, and survival grant nothing automatically.

### Monster Evolution

Dungeon ecology may create pressure, resources, refuges, traps, or route opportunities. [Monster Generation](MONSTER_GENERATOR.md) establishes individual and species profiles, while Phase 4 retains every Evolution transition.

### Human Classes and Professions

Builders, delvers, maintainers, archivists, priests, engineers, salvagers, rulers, and residents use actual Classes, Professions, institutions, capability, offices, and law. Labels do not grant mastery, rights, or universal Dungeon expertise.

### Soul Weapons

Weapon Souls remain persons and partners. A site becomes a Vessel or a Weapon Soul becomes site-bound only through Soul Weapon rules. Dungeons do not manufacture Soul Weapons, universal keys, detectors, or loot partners.

### Magic

Every magical condition, topology, ward, trap, source, artifact, Ritual, Enchantment, or Alchemical process requires its Magic owner, Mana relation, maintenance, cost, limits, and consequences.

### World Engine

The World Engine provides every changing population, resource, ecological, economic, factional, conflict, disease, advancement, Stability, Age, Reset, and Gate context. The Dungeon Profile references those states without duplicating them.

### NPC and Faction Agency

[NPC Generation](NPC_GENERATOR.md) supplies individual actor profiles. Faction Behaviour supplies collective information and decision routes. A Dungeon generator cannot choose either actor's future actions.

### Encounter Generator

Regions, inhabitants, processes, hazards, opportunities, claims, and Pending Consequences may become Encounter Sources when their routes intersect a player decision. Dungeon generation does not schedule or resolve those Encounters.

### Alpha Playtest Rules

Use a narrow Provisional Rule only where a Canonical Foundation supports the missing claim. Do not use Dungeon generation to implement faction, world-event, Time Skip, Age-transition, or other major unfinished systems.

## Worked Examples

### Abandoned Irrigation Network

An underground water system began as ordinary infrastructure. Failed Enchantments, seasonal floods, fungal cultivation, nesting monsters, damaged repair machines, and outward water dependence now create a practical boundary, layered Sustaining Basis, distinct internal processes, and continuity.

Generation derives regions from reservoirs, pressure zones, farms, maintenance routes, and habitats. There is no Core, boss, treasure schedule, or automatic hostility. Removing one machine changes only its supported flow.

### Living Leviathan

Air chambers, organs, symbionts, a settlement, immune activity, and pressure communication inside a continent-scale person meet the Dungeon test for some practical purposes.

The leviathan's body owns core life processes and consent. The settlement and symbionts retain independent agency. Topology changes with movement, digestion, injury, and care rather than random room rearrangement.

### Subterranean Monster City

A guild calls a monster city a Dungeon and labels districts as floors. The site may meet the spatial Dungeon test, but citizens, law, trade, schools, temples, and infrastructure remain a society.

The generator records the guild classification as an Observer View and faction cause. It does not populate districts with enemies or make civic property Dungeon Yield.

### Archive With No Treasure Room

A volcanic archive preserves records through cooling Enchantments, trained caretakers, mineral insulation, and seasonal closure. Its most valuable outputs are authenticated knowledge and negotiated research access.

The Profile follows source maintenance, caretaker authority, heat, Mana storms, legal claims, and document condition. There is no need for a final vault or combat guardian.

### Gate-Overlapped Ruin

A damaged ruin periodically overlaps a foreign marsh through an established World Gate. The Gate owns contact channels and transfer; the ruin's surviving wards and new ecology own different Dungeon processes.

The generator keeps the Gate, Dungeon Boundary, and ecological exchange separate. Gate Closure may end foreign transfer while leaving contamination, inhabitants, damaged topology, and Dungeon identity.

### Fortress After Defeat

One occupying faction is removed from a fortified Dungeon. Survivors, prisoners, refugees, traps, damaged water systems, supply claims, and neighboring actors remain.

Revalidation changes control, activity, and access without resetting the site. Faction generation and simulation determine future collective action; the Dungeon generator preserves current facts and handoffs.

### Old Map, New Body

A reincarnated explorer remembers a former route through a labyrinth. The generator records that historical route as a dated Map Claim and compares it with current flood, settlement, excavation, and Reset evidence.

The memory may reveal organizing logic but cannot establish current access or body compatibility. Investigation can update the observer's map without moving the Dungeon to match it.

## Human and AI GM Use

Human and AI GMs use the same generator and ownership boundaries.

An AI GM should:

- search existing site, world, map, and history records before generating;
- state Dungeon Basis before topology or contents;
- cite each unusual spatial, magical, living, or agentive claim to an owner;
- distinguish Factual Topology from observer Map Claims;
- preserve inhabitants as actors rather than content;
- leave unsupported regions unresolved instead of filling every blank;
- avoid recognizable room-table patterns unless an actual builder or process produced them;
- keep current state and generated Profiles external;
- use Revalidation when established change occurs;
- ask for clarification when classification, prior maps, personhood, control, origin, or campaign continuity materially conflicts.

Generative abundance does not authorize infinite rooms, populations, resources, or history.

## Safeguards

- **No Dungeon by label:** Classification must pass boundary, Sustaining Basis, distinct condition, exchange, continuity, and evidence tests.
- **No mandatory Core:** Create a Core only for an established concentrated function.
- **No universal anatomy:** Floors, rooms, layers, bosses, traps, treasure, puzzles, and safe zones are optional.
- **No level scaling:** Conditions, inhabitants, and hazards follow world state.
- **No linear difficulty topology:** Connections follow formation, function, and change rather than progression order.
- **No room quotas:** Generate regions only when source and resolution justify them.
- **No random-source creation:** Tables cannot invent beings, Mana, artifacts, resources, or structures without provenance.
- **No quantum map:** Hidden routes and contents do not move after player choices.
- **No spawning convention:** Inhabitants need reproduction, migration, construction, summoning, transformation, or another valid route.
- **No resource respawn:** Stocks and Flows renew only through established processes.
- **No loot entitlement:** Exploration and danger guarantee no property or reward.
- **No inhabitant dehumanization:** Location does not erase personhood, society, rights, consent, or agency.
- **No universal Dungeon mind:** Responsive systems require actual agents, mechanisms, or distributed processes.
- **No Core omnipotence:** A Core controls and knows only what its functions and interfaces support.
- **No magic shortcut:** Every magical effect retains source, Mana, access, maintenance, cost, and limits.
- **No progression farming:** Entry, distance, depth, mapping, clearing, killing, extraction, suffering, death, and survival grant nothing by category.
- **No automatic clearing:** Removing one threat, Core, controller, or Population ends only its supported functions.
- **No automatic collapse result:** Collapse follows affected systems and preserves survivors, hazards, claims, and Legacies.
- **No reset after play:** Generated sites persist and change through causality.
- **No encounter scripting:** Generation does not guarantee contact, hostility, sequence, or outcome.
- **No campaign data in canon:** Populated briefs, seeds, profiles, maps, inhabitants, routes, resources, and histories remain external.
- **No automation supremacy:** Generation supports GM judgment and specialist owners rather than replacing them.

## Scope Boundaries

This document does not define:

- a canonical Dungeon catalog, map, or adventure;
- a universal origin, purpose, anatomy, Core, floor, encounter, loot, or reset model;
- live campaign Dungeons, maps, populations, hazards, resources, claims, or histories;
- monster, NPC, faction, or world-event generation;
- Dungeon Activity outcomes;
- exact magical, technical, ecological, economic, disease, conflict, Gate, or Soul effects;
- Time Skip or Age-transition procedures;
- progression, reward, or universal difficulty rules.

## Related Documents

- [Dungeon Activity](../world-engine/DUNGEON_ACTIVITY.md)
- [World Engine](../world-engine/README.md)
- [Causal Event Chains](../world-engine/CAUSAL_EVENT_CHAINS.md)
- [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md)
- [Resources and Food](../world-engine/RESOURCES_AND_FOOD.md)
- [Ecology and Migration](../world-engine/ECOLOGY_AND_MIGRATION.md)
- [Faction Behaviour](../world-engine/FACTION_BEHAVIOUR.md)
- [Monster Evolution](../monster-evolution/README.md)
- [Magic Rules](../magic/README.md)
- [Soul Weapons](../soul-weapons/README.md)
- [Encounter Generator](ENCOUNTER_GENERATOR.md)
- [Monster Generator](MONSTER_GENERATOR.md)
- [NPC Generator](NPC_GENERATOR.md)
- [Faction Generator](FACTION_GENERATOR.md)
- [Uncertainty Handling](UNCERTAINTY_HANDLING.md)
- [Consequence Resolution](CONSEQUENCE_RESOLUTION.md)
