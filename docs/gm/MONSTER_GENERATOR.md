# Monster Generator

## Purpose

This document defines how a Game Master creates a canon-compatible monster species sketch, individual, or bounded group for an Eternal Cycle campaign.

The generator applies the completed Monster Evolution, Development, Skill, Magic, Soul, and World Engine rules. It does not create a bestiary, universal stat block, combat role, random trait bundle, or campaign state inside this repository.

## Core Rule

Generate a monster from a world-valid origin, coherent current embodiment, actual lifecycle, ecology, history, and present conditions before considering how it might become relevant to play.

A Monster Profile describes what currently exists. It does not grant an Evolution, assign morality, decide behaviour, scale capability to a party, or make the monster an Encounter.

## Ownership

This document owns:

- selecting a generation scope;
- collecting a valid species or origin basis;
- building campaign-local species sketches through existing canonical owners;
- instantiating individual variation and history;
- separating Species Traits, Development, Skills, magic, Soul state, equipment, and support;
- producing external Monster Profiles;
- validating world placement and information views;
- handoffs to Encounters and other specialist procedures.

It does not own:

- the definition or resolution of Evolution Routes;
- Species Stages, Evolutionary Pressure, Mutation, Hybridization, adaptation, apex status, or extinction;
- Development or Skill acquisition;
- magical access or effects;
- Soul identity, Reincarnation, or Soul progression;
- actor decisions;
- Encounter selection or outcomes;
- current populations, ecology, factions, settlements, or world state;
- reusable canonical species entries.

[Monster Evolution Rules](../monster-evolution/README.md) remain authoritative for every form and transition claim. The generator organizes their application to campaign-local content.

## What "Monster" Means Here

"Monster" identifies the progression-tree and embodiment family governed by Monster Evolution and monster Skill rules in the current setting. It does not establish:

- personhood or its absence;
- intelligence;
- morality;
- hostility;
- wildness;
- social complexity;
- legal status;
- ecological role;
- magical nature;
- danger;
- inferiority or superiority to beings called human.

Cultures may classify the same being differently. Those labels belong to Observer Views, law, religion, scholarship, prejudice, or custom. The generator records such classifications without treating them as Factual proof of identity or worth.

## Key Terms

### Monster Generation Brief

A **Monster Generation Brief** is the external input that states what kind of profile is needed, its world context, established species or origin material, information views, canonical limits, and intended preparation horizon.

It states a question, not a desired answer.

### Monster Basis

A **Monster Basis** is the validated set of origin, lineage, form, lifecycle, ecological, structural, magical, social, and historical facts that permit a monster profile to exist.

The Basis may reference a reusable species definition, a campaign-local species sketch, a valid Evolution history, or another explicit origin. A genre label or desired combat function is not a Basis.

### Species Reference

A **Species Reference** is the reusable or campaign-local description that defines a species or form's ordinary anatomy, lifecycle, Species Potential, traits, needs, variation, ecology, and known route structure.

It describes a range. It is not a complete individual and does not make every member typical.

### Monster Seed

A **Monster Seed** is an unvalidated draft combination of a possible Monster Basis, individual history, and world placement.

It is preparation rather than fact until the relevant ownership, coherence, causality, and placement tests pass.

### Monster Profile

A **Monster Profile** is an external Campaign Record describing one monster individual or explicitly bounded group through provenance, current embodiment, history, capabilities, needs, relationships, information views, and present condition.

It is not a universal stat block, Species Reference, Encounter role, or Evolution menu.

### Individual Variation

**Individual Variation** is a difference among members of one species or form produced by inherited variation, lifecycle, condition, Development, Skills, experience, culture, environment, relationships, equipment, magic, Mutation, or another established source.

Variation does not require a new species, form, route, or special destiny.

## Generation Scopes

Choose one scope before generation.

### Species Sketch

Create a campaign-local Species Reference when the world needs a species or form not yet defined.

The sketch must pass the existing Monster Evolution owners and remain external unless separately reviewed as reusable canonical reference material. It cannot be invented as a shortcut to one desired individual ability.

### Individual From a Known Species

Instantiate one individual from an established Species Reference, then add a causal current-life history, variation, Development, Skills, relationships, and present state.

This is the ordinary scope.

### Unusual Individual

Create an individual whose Mutation, Hybrid Form, Evolution history, magical alteration, artificial origin, symbiosis, disease, injury, or other source materially differs from ordinary members.

The unusual element requires its own owner and provenance. "Rare" is not an explanation.

### Bounded Group

Create a specific pack, brood, swarm, household, work team, patrol, colony, or other bounded group.

The group requires actual membership, coordination, communication, relationships, roles, shared resources, and limits. Do not multiply one profile by a number or treat a species as one group mind.

### Reincarnation Candidate Support

Provide the embodiment facts needed to evaluate a world-valid monster [Reincarnation Candidate](REINCARNATION_GENERATION.md). The Reincarnation generator retains candidate eligibility, selection, and placement ownership.

The Monster generator cannot reserve, populate, or select the body merely because it defined the species.

## Monster Generation Brief

Use a brief such as:

```markdown
### Monster Generation Brief

Generation scope:
- species sketch, individual, unusual individual, bounded group, or candidate support:
- exact question the profile must answer:

World context:
- time, place, habitat, and useful simulation resolution:
- established ecology, resources, magical conditions, and populations:
- relevant societies, institutions, factions, and law:
- current pressures, disturbances, and World Engine state:

Basis:
- known Species Reference or origin:
- established form, Stage, route, alteration, or lineage history:
- required canonical owners:

Information:
- Factual View:
- relevant Observer Views:
- player-facing evidence:
- hidden, disputed, or unresolved claims:

Boundaries:
- canonical or provisional limits:
- preparation horizon:
- claims that must remain ungenerated:
```

The populated brief belongs in the external Campaign Record.

## Monster Basis Requirements

Every Monster Basis establishes enough of the following to support the current scope.

### Origin and Continuity

State how the species, form, or individual came to exist.

Possible sources include:

- ordinary reproduction or renewal;
- spawning, budding, fission, seeding, hatching, gestation, or assembly;
- a constructed or artificial origin;
- a magical or spiritual origin with a defined source;
- a valid Hybrid Origin;
- an established Mutation;
- a documented Evolution Transition;
- migration from another established domain;
- a Gate traversal with valid origin and transit;
- a Reincarnation embodiment after candidate selection.

Creation explains origin, not automatic maturity, knowledge, loyalty, or purpose.

### Current Form and Species Stage

Identify the present species or form, expected lifecycle, current Species Stage, maturation state, and any valid transition windows.

Do not use Stage as a universal rank. A juvenile may be physically limited while socially important, magically specialized, well protected, or unusually experienced through a valid history.

### Embodied Coherence

The body needs coherent:

- structure and material;
- movement and support;
- senses and information integration;
- metabolism, energy, mana, or another sustaining relation;
- regulation and recovery;
- communication routes;
- reproduction or renewal where relevant;
- environmental tolerances;
- waste, heat, residue, or other outputs;
- dependencies and vulnerabilities.

Fantasy anatomy may use magic, spirits, constructs, distributed bodies, living materials, or nonbiological regulation. It still needs explicit relations and limits.

### Ecology and Placement

Use [Monster Ecology](../monster-evolution/MONSTER_ECOLOGY.md) and the [World Engine](../world-engine/README.md) to establish:

- Habitat and Ecological Niche;
- resource inputs and waste routes;
- predators, prey, competitors, symbionts, hosts, and decomposers where material;
- Territory, Migration, reproduction, and seasonal patterns;
- Carrying Capacity and limiting factors;
- magical environmental relations;
- effects of and on settlements, infrastructure, and other populations.

No creature appears in a place because its visual theme fits. Placement requires a route and viable present conditions.

### Species Traits and Species Potential

Define ordinary traits and the range of current form expression under [Species Development](../progression/SPECIES_DEVELOPMENT.md).

For every material Species Trait, record:

- structure or source;
- access conditions;
- what it can currently express;
- cost, maintenance, and recovery;
- limits and incompatibilities;
- variation across Stage, health, environment, and individuals;
- what reliable use still requires Development or Skill.

A natural weapon is not mastered combat. A sensory organ is not perfect interpretation. A Channel is not a spell list.

### Evolution Structure

Use established [Monster Evolution](../monster-evolution/MONSTER_EVOLUTION.md), [Species Stages](../monster-evolution/SPECIES_STAGES.md), and [Branching Evolution](../monster-evolution/BRANCHING_EVOLUTION.md) rules.

A Species Reference may describe known possible routes, but:

- maps are descriptive rather than exhaustive;
- availability is contextual;
- hidden conditions require prior truth and fair clues;
- current Route Readiness belongs to the individual history;
- no branch is a mandatory upgrade;
- no current profile receives a future form's traits.

Use the [Evolution Tree Template](../../templates/EVOLUTION_TREE_TEMPLATE.md) when a route structure needs a dedicated external record.

### Mind, Agency, and Personhood

Assess actual cognition, awareness, preferences, communication, memory, relationships, self-direction, and continuity. Do not infer personhood from resemblance to humans, speech, tool use, civilization, body size, threat, or institutional recognition.

When personhood is uncertain:

- preserve the Factual question rather than choosing convenience;
- record observer beliefs separately;
- avoid treating silence or unfamiliar communication as absence;
- expose evidence through fair routes;
- resolve only through applicable canon and evidence.

Personhood affects consent and treatment. It does not grant universal intelligence, morality, capability, or legal recognition.

### Society and Culture

If the monster participates in a society, use [Intelligent Monster Societies](../monster-evolution/MONSTER_SOCIETIES.md).

Record culture, language or communication, education, work, institutions, religion, law, relationships, and collective resources only where the individual's history gives access. Species does not dictate one culture.

### Magical and Spiritual Relations

Separate:

- species organs or Channels;
- environmental Mana relations;
- learned magical Skills;
- magical Development;
- spells and rituals;
- granted, borrowed, artificial, or symbiotic effects;
- Soul phenomena;
- temporary conditions.

Use the [Magic Rules](../magic/README.md). A magical creature is not an unlimited Mana source or universal spellcaster.

## Species Sketch Procedure

Use this procedure only when no suitable Species Reference exists.

### Step 1: Begin From a World Need or Origin

Identify the established ecological, magical, constructed, spiritual, historical, or lineage route that supports the species.

Do not begin from an Encounter difficulty, desired loot, isolated power, visual gimmick, or vacant combat role.

### Step 2: Establish the Habitat and Resource Cycle

Define what sustains the species, where it can live, how resources renew or move, what it returns to the environment, and which limits constrain its population.

### Step 3: Define Lifecycle and Renewal

Establish formation, birth or construction, care, maturation, Stages, reproduction or replacement, ageing, dormancy, and death as applicable.

Avoid assuming mammalian, individual, sexual, biological, or human-like life cycles.

### Step 4: Build a Coherent Base Form

Connect structure, movement, senses, communication, energy, regulation, defense, feeding or sustenance, and recovery.

Each extraordinary trait needs a source and consequence. Do not add weaknesses as decorative "balance" if the body's real needs already provide limits.

### Step 5: Separate Traits From Learned Capabilities

List only inherited or form-owned Species Traits. Place learned methods in Skills, developed bodily expression in Physical or Species Development, and social techniques in their proper tracks.

### Step 6: Define Variation

Identify ordinary variation from ancestry, development, Stage, sex or role where applicable, environment, health, social practice, and individual history.

Variation prevents a Species Reference from becoming one cloned stat block.

### Step 7: Establish Ecology and Relationships

Map only material resource-web, competition, symbiosis, predation, Territory, Migration, and settlement relations. Include consequences of population increase, decline, or movement.

### Step 8: Address Cognition and Social Possibility

Define sensory and cognitive routes without forcing one personality or culture. State whether personhood is established, absent, variable, emergent, distributed, or genuinely unresolved under the setting's evidence.

### Step 9: Define Magical Relations

If magic is relevant, state Mana source, access route, Channels, regulation, costs, limits, environmental dependence, and what still requires Skills or training.

### Step 10: Establish Evolution Provenance

Define only routes already supported by lineage, structure, lifecycle, world history, or explicit transition systems. It is valid for a species sketch to have no known Evolution Route.

### Step 11: Test World Viability

Check resource demand, renewal, reproduction, population limits, disease, Counter-Ecology, climate, magic, societies, and other World Engine relations.

"Rare" does not solve impossible maintenance or reproduction.

### Step 12: Run the Phase 4 Safeguard Audit

Apply [Monster Evolution Safeguards](../monster-evolution/EVOLUTION_SAFEGUARDS.md). Consolidate cosmetic duplicates, remove progression rewards, separate owners, and reject unsupported forms.

### Step 13: Record the Species Reference Externally

Preserve sources, limits, variation, uncertainty, and owners. Do not add the campaign-local species to this canonical repository.

## Individual Generation Procedure

### Step 1: Select a Valid Species Reference

Choose an established species or complete the Species Sketch procedure. Confirm the species can exist at the intended time and place.

Extinct, inaccessible, displaced, or unsustainable species require an actual recovery, migration, Gate, reconstruction, or other route.

### Step 2: Establish Origin and Age

Record how and when this individual formed, its parentage or construction source where material, early dependencies, and elapsed history.

Do not assign a mature body without accounting for maturation, maintenance, survival, and prior relationships.

### Step 3: Determine Current Form and Condition

Set current Stage, form, health, injury, disease, adaptation, Mutation, Hybridization, magical alteration, equipment, symbiosis, and temporary conditions through their owners.

Do not choose the form by desired threat category.

### Step 4: Build a Causal Life History

Establish only the history needed to explain current traits and decisions:

- habitats and movement;
- care, teaching, and socialization;
- work, hunting, craft, study, ritual, conflict, or other meaningful practice;
- pressures, adaptations, and losses;
- relationships and institutions;
- access to tools, magic, language, and knowledge;
- important decisions and consequences;
- current obligations, needs, and goals.

A history is causal support, not a bundle of free competencies.

### Step 5: Assign Individual Variation by Source

Choose variation that follows origin and history. For each material difference, identify whether it is:

- inherited variation;
- Stage or lifecycle expression;
- present condition;
- Physical or Species Development;
- Skill;
- adaptation;
- Mutation;
- Hybridization;
- Evolution;
- magical, Soul, equipment, social, or environmental support.

If no source owns the difference, remove or redesign it.

### Step 6: Establish Development

Use the six-layer model and relevant [Development Tracks](../progression/README.md). Current-life effort, access, embodiment, reliability, and context remain necessary.

Do not fill every Track. Breadth, specialization, plateaus, rust, injury, inaccessible mastery, and ordinary inexperience are valid.

### Step 7: Establish Skills

Use the [Monster Skill Trees](../skills/MONSTER_SKILL_TREES.md) and other applicable Skill owners.

Skills require receiving-tree permission, acquisition history, expression routes, practice, and maintenance. Species Traits may enable a Skill but do not include mastered use.

### Step 8: Establish Magic and Equipment

Record magical access, spells, rituals, enchanted support, ordinary tools, armor, weapons, infrastructure, and supply through their actual sources.

Equipment remains equipment. It does not become anatomy or a Soul Weapon unless its own canonical transition occurs.

### Step 9: Establish Soul-State Facts Only When Relevant

Do not give every monster a special Soul history. If Soul Depth, Resonance, Echoes, Titles, Retained Instincts, Reincarnation, Soul Avatars, or Soul Weapons matter, use their owners and record only established access.

Former-life anatomy and traits do not return as current Species Traits.

### Step 10: Establish Mind, Identity, and Relationships

Define current preferences, values, fears, habits, communication, beliefs, mistakes, loyalties, attachments, rivalries, and social position from actual history.

Avoid a single species personality. An ecological need can shape a pressure without dictating one response.

### Step 11: Establish Current Objectives and Constraints

Record what the individual presently seeks, avoids, protects, owes, misunderstands, or waits for, plus what could make it reconsider.

These are current campaign facts, not permanent behaviour scripts.

### Step 12: Build a Contextual Capability Profile

Use [Capability Assessment](../progression/CAPABILITY_ASSESSMENT.md) only for questions likely to matter within the preparation horizon.

Do not convert body size, Evolution history, Skill breadth, magic, rarity, age, or institutional threat category into one rating.

### Step 13: Separate Information Views

Record:

- what is established in the Factual View;
- what the monster knows, suspects, misunderstands, and conceals;
- what relevant observers believe;
- player-facing evidence;
- hidden, disputed, outdated, and unresolved claims.

Unknown to the players does not mean undecided by the GM.

### Step 14: Validate World Placement

Confirm location, timing, travel or origin route, resources, shelter, social access, population relation, and current activity.

The individual does not wait motionless for player contact and does not appear wherever an Encounter would be convenient.

### Step 15: Create the External Monster Profile

Record the minimum durable facts needed for continuity. Omit decorative detail that has no causal or presentational use.

### Step 16: Hand Off Without Predetermining Play

If the monster may become decision-relevant, provide it as an established source to [Encounter Generation](ENCOUNTER_GENERATOR.md). The Encounter procedure determines eligibility and framing.

The Monster generator does not decide that contact, hostility, combat, alliance, capture, death, loot, or Evolution occurs.

## Monster Profile Template

```markdown
### Monster Profile: <external campaign identifier>

Profile scope:
- individual or bounded group:
- generation purpose and preparation horizon:

Monster Basis:
- Species Reference:
- origin and continuity route:
- current form and Species Stage:
- Evolution, Mutation, Hybrid, adaptation, or alteration history:

Current embodiment:
- structure, movement, senses, and communication:
- sustenance, regulation, recovery, and waste:
- Species Traits and expression limits:
- needs, dependencies, incompatibilities, and vulnerabilities:
- current health and conditions:

Ecology and placement:
- Habitat, niche, resource relations, Territory, and Migration:
- population or social context:
- current location, route, and activity:

Development and capability:
- relevant Development Tracks and six-layer distinctions:
- Skills and acquisition history:
- magic, tools, equipment, and support:
- reliability, preparation, bottlenecks, and Counterforces:

Mind and relationships:
- personhood status and evidence:
- communication, values, beliefs, and misunderstandings:
- relationships, culture, institutions, and obligations:
- current objectives, constraints, and reconsideration routes:

Soul relations, if material:
- established Soul state and access:
- Soul Weapon or other partner agency:

Information views:
- Factual View:
- monster's Observer View:
- other relevant Observer Views:
- player-facing evidence:
- hidden, disputed, outdated, or unresolved claims:

Handoffs and continuity:
- relevant canonical owners:
- potential Encounter Source route:
- Review Points and Pending Consequences:
```

The template is optional, qualitative, and external. It contains no combat total, level, loot budget, or mandatory field completion.

## Bounded Group Generation

A group profile begins from real members and coordination rather than one multiplied individual.

Record:

- membership and variation;
- kinship, association, command, cooperation, or temporary alignment;
- communication and information distribution;
- roles grounded in bodies, Skills, Professions, and current needs;
- shared and individually owned resources;
- cohesion, dissent, fear, trust, and exit routes;
- formation, dispersal, replacement, and leadership succession;
- collective capability and bottlenecks;
- what changes when members separate.

For swarms, colonies, distributed minds, symbioses, and collectives, establish whether personhood and agency belong to individuals, subunits, the whole, several levels, or remain unresolved. Do not assume "many bodies" means one mind or "one signal" means perfect coordination.

## Selection and Randomization

Random tools may vary details only among world-supported options.

### Good Uses

- selecting among plausible ordinary inherited variations;
- choosing a supported birth season, age band, route, diet emphasis, coloration, minor scar, or current activity;
- resolving an explicitly unsettled origin detail whose alternatives have equal canonical support;
- varying group composition within an established population and purpose;
- testing a generator for unintended repetition.

### Invalid Uses

- inventing a species or individual without origin and placement;
- assigning traits from unrelated forms;
- deciding personhood, morality, hostility, loyalty, consent, or intelligence by category;
- granting Evolution, Mutation, Hybridization, Soul history, magic, Skills, or a Soul Weapon;
- assigning a universal challenge rating;
- producing "rare" powers without provenance;
- deciding Encounter occurrence or outcome;
- replacing current ecology or population state.

Before any random selection, state the exact field, supported options, basis for weighting, and recording status. Resolve once. Do not reroll until the profile is stronger, stranger, more useful, or more favorable.

## Individual Variation Without Species Proliferation

Use the smallest truthful owner.

A different color, size, diet preference, scar pattern, trained technique, magical tool, cultural role, local adaptation, or one source-owned Mutation does not automatically require:

- a new species;
- a new Evolved Form;
- a branch name;
- a rarity tier;
- a unique Skill;
- a destiny.

Create a distinct form only when the [Form Distinction Test](../monster-evolution/EVOLUTION_SAFEGUARDS.md#the-form-distinction-test) shows a coherent change to Species Potential, embodiment, lifecycle, needs, and future routes that cannot be represented honestly by a smaller owner.

## Capability and Threat

Describe capability for an actual question.

A Monster Profile may establish that an individual is:

- an exceptional burrower;
- inexperienced in open terrain;
- reliable at communal ward maintenance;
- physically overwhelming at close range;
- unable to sustain pursuit;
- legally protected by a treaty;
- dependent on a magical habitat;
- supported by a coordinated family;
- unknown to a local institution.

These facts do not add into one threat score.

Institutional threat categories may exist for evacuation, hunting law, research, military planning, or public warning. Record issuer, purpose, evidence, assumptions, jurisdiction, and known errors. The category does not become the Factual View or scale an Encounter.

## Behaviour and Agency

Generation establishes current motives and constraints, not a behaviour script.

A monster acts from:

- needs and current condition;
- senses and information;
- prior learning and instincts;
- values, relationships, and obligations;
- ecological and social context;
- available capability and alternatives;
- fear, confidence, uncertainty, and consequence;
- new information received during play.

Predators do not attack everything. Territorial beings do not defend every boundary to death. Intelligent monsters do not share one culture. Non-sapient creatures still use actual senses and behavior rather than obeying plot needs.

Use [GM Responsibilities](GM_RESPONSIBILITIES.md) for actor stewardship. Participant choices remain unresolved until their decision points.

## Hidden Information and Discovery

A Monster Profile may contain hidden anatomy, capability, relationship, identity, route history, dependency, or motive when it has prior causal truth.

Fair evidence may include:

- tracks, residue, molts, waste, remains, nests, damage, or resource patterns;
- behavior and timing;
- songs, signals, writing, tools, structures, or trade;
- witness accounts and institutional records;
- magical traces;
- ecological absence or surplus;
- scars, instability, adaptation, or unusual needs;
- inconsistencies in public classification.

Evidence need not reveal the answer. It must not be invented after a player chooses a counter. Use [Uncertainty Handling](UNCERTAINTY_HANDLING.md) for genuinely unresolved claims.

## Cross-System Interfaces

### Soul Engine and Reincarnation

Soul identity is independent of monster classification. Reincarnation may place a soul into a valid monster embodiment through [Reincarnation Generation](REINCARNATION_GENERATION.md), but former Species Traits, forms, Evolution Routes, and current bodily statistics do not transfer.

Soul effects require established access. A monster does not receive a deeper or shallower Soul by species stereotype.

### Development System

Current embodiment defines available Species Potential, while current-life training, experience, condition, access, and practice determine expression and reliability. Retained progression may accelerate valid relearning but never fills a generated profile with inherited mastery.

### Skill Engine

Monster Skills use monster-tree permission and actual acquisition histories. Reincarnation crossover remains bounded. A body feature is not automatically a Skill, and a Skill does not rewrite anatomy.

### Monster Evolution

Phase 4 owns ecology, pressure, form routes, Stages, branching, hidden conditions, Mutation, Hybridization, apex claims, societies, adaptation, extinction, Soul interaction, and safeguards. The generator applies those owners and records their results without producing Evolution as a profile flourish.

### Human Classes and Professions

An intelligent monster may develop Professions and participate in institutions where access, embodiment, culture, and current-life history support it. Human progression does not become a default monster tree, and occupation does not change species ownership.

### Soul Weapons

A monster may partner with a Soul Weapon under the ordinary awakening, compatibility, agency, and manifestation rules. The generator cannot assign a Soul Weapon as equipment, a rarity feature, or an Encounter reward.

### Magic

Magical beings still require Mana relations, access, Channels, control, Skills, costs, and consequences. Natural magical traits remain distinct from learned Spellcraft and borrowed sources.

### World Engine

Populations, ecology, resources, economies, factions, war, disease, advancement, Dungeons, Stability, Ages, and Gates provide the world conditions for generation and placement. A Monster Profile cannot revise them by assertion.

### Encounter Generator

A completed profile may become an Encounter Source when its route and current activity intersect a player decision. Generation does not make contact inevitable, require hostility, or determine the Encounter Frame.

### Alpha Playtest Rules

If a narrow species or capability detail lacks complete implementation but has a Canonical Foundation, label the smallest necessary Provisional Rule externally. Do not use the Monster generator to establish a reusable species system, universal formula, or unsupported major mechanic.

## Worked Examples

### Known Burrowing Species, Ordinary Individual

An established burrowing species senses vibration, metabolizes mineral-rich fungus, and maintains communal tunnels. The generated individual is a young drainage worker with practised route memory, modest digging reliability, an old forelimb injury, and strong knowledge of one local water system.

The profile does not copy every species member. Vibration sensing is a Species Trait; interpreting water-machine rhythms is Skill and Professional Development; reduced endurance follows injury. Nothing makes the individual hostile or suitable for a particular party.

### New Mana-Filtering Species Sketch

A wetland receives regular mineral runoff and unstable ambient Mana. The species sketch begins from colonial filter organisms that bind both inputs, reproduce by seasonal fragmentation, and depend on slow water flow. They return altered sediment used by plants and become vulnerable when channels are dredged.

The species has no automatic spells and no known Evolution branch. Its filtering structure is a Species Trait with material and magical costs. Ecology, reproduction, waste, Carrying Capacity, and settlement consequences make it world-valid before an individual is generated.

### Evolved Individual With Lost Access

An established gliding species has a valid subterranean Evolution Route. One individual completed the transition after years in sealed caverns and now has compact limbs, pressure sensing, and no functional gliding membranes.

The profile records route provenance, transition history, new needs, former flight Skills as inaccessible, and current calibration. It does not retain every old trait or treat the Evolved Form as a higher rank.

### Intelligent Monster Diplomat

A metamorphic species supports several cultures. The generated adult is a trained translator from a river institution, belongs to a minority legal tradition, and carries authority only for a specific water-sharing negotiation.

Communication, Profession, legal access, relationships, and current objectives come from history and institutions rather than Species Traits. The profile leaves negotiation decisions open and does not assume the diplomat represents every member of the species.

### Ambush Predator That Does Not Attack

A solitary predator is concealed near a road because it follows displaced prey. It is injured, well fed, wary of fire, and currently seeks a sheltered route rather than a fight.

Its anatomy supports ambush, but its present motive does not. The profile may supply an Encounter Source if routes intersect; it does not force an attack to satisfy the word "predator."

### Monster Reincarnation Candidate Support

A seasonal spawning pool can produce several viable juvenile bodies in a distributed aquatic species. The Monster generator defines the species' senses, lifecycle, care network, cognition, ecology, and ordinary variation.

The Reincarnation generator separately determines whether one actual Embodiment Opportunity is reachable, vacant, compatible, and eligible. Defining the species does not create a candidate or give the arriving soul mature collective Skills.

### Hybrid Individual

A stable lineage arose through an established symbiotic Hybrid Origin. One member lacks part of the ordinary symbiont colony after disease.

The profile distinguishes inherited Hybrid Form, current disease loss, reduced regulation, learned compensation, and external medicine. It does not roll traits from both sources independently or classify every impairment as a new form.

## Human and AI GM Use

Human and AI GMs use the same ownership and evidence rules.

An AI GM should:

- retrieve the relevant Species Reference and canonical owners before generating traits;
- distinguish a species range from individual facts;
- identify the provenance of every unusual capability;
- keep Factual, Observer, and player-facing views separate;
- avoid familiar bestiary stereotypes when ecology or culture does not support them;
- avoid filling every optional field;
- state when a species sketch is campaign-local or Provisional;
- preserve participant agency and unknown future choices;
- store completed profiles only in the external Campaign Record;
- ask for clarification when origin, personhood, placement, or campaign continuity is materially contradictory.

Generative fluency does not make a coherent Monster Basis optional.

## Safeguards

- **No stat blocks:** Profiles use sourced qualitative capability, not universal numerical arrays.
- **No party scaling:** Species, individuals, groups, and placement follow world causality.
- **No combat-role anatomy:** Bodies do not acquire powers because an Encounter needs a defender, striker, caster, boss, or swarm.
- **No trait grab bag:** Every material trait has one source, owner, access route, and limit.
- **No cloned species:** A Species Reference describes ranges; individuals require history and variation.
- **No species proliferation:** Use the smallest truthful owner before creating a new form or name.
- **No automatic Evolution:** Generation cannot award routes, readiness, transitions, or resulting forms.
- **No inherited mastery:** Anatomy grants access, not Skill or reliable expression.
- **No universal apex:** Apex claims remain domain-, dependency-, ecology-, and Counter-Ecology-specific.
- **No rarity as provenance:** Uncommon claims still need origins, resources, and viable continuity.
- **No random personhood:** Personhood and agency follow established evidence and rules.
- **No species morality:** Ecology, anatomy, and progression tree do not dictate ethics or allegiance.
- **No mandatory hostility:** Current objectives and information govern behaviour.
- **No primitive default:** Monster societies are assessed through their own embodiments, cultures, institutions, and technologies.
- **No unlimited magic:** Magical creatures retain Mana relations, costs, Channels, Development, and Skill requirements.
- **No Soul exceptionalism by category:** Monster status neither grants nor denies Soul depth, continuity, or special access.
- **No free Soul Weapon:** A profile cannot assign one as loot or character equipment.
- **No retroactive hidden trait:** Concealed capabilities need prior factual existence and fair evidence.
- **No encounter by generation:** A generated monster is not automatically an Encounter Source that reaches the player.
- **No campaign data in canon:** Populated briefs, sketches, profiles, groups, and placements remain external.
- **No automation supremacy:** Random and generative tools assist bounded judgment rather than replace canonical owners.

## Scope Boundaries

This document does not define:

- a canonical bestiary or species catalog;
- universal monster statistics, challenge ratings, rarity tiers, or loot;
- named campaign monsters, populations, societies, habitats, or histories;
- Evolution Routes, transitions, stages, mutations, hybrids, apex forms, or adaptations;
- NPC identity and decision-context generation beyond a monster's embodiment profile;
- Encounter generation or outcomes;
- current campaign ecology or world state.

## Related Documents

- [Monster Evolution Rules](../monster-evolution/README.md)
- [Monster Ecology](../monster-evolution/MONSTER_ECOLOGY.md)
- [Monster Evolution](../monster-evolution/MONSTER_EVOLUTION.md)
- [Intelligent Monster Societies](../monster-evolution/MONSTER_SOCIETIES.md)
- [Monster Evolution Safeguards](../monster-evolution/EVOLUTION_SAFEGUARDS.md)
- [Species Development](../progression/SPECIES_DEVELOPMENT.md)
- [Monster Skill Trees](../skills/MONSTER_SKILL_TREES.md)
- [Magic Rules](../magic/README.md)
- [World Engine](../world-engine/README.md)
- [Encounter Generator](ENCOUNTER_GENERATOR.md)
- [Reincarnation Generation](REINCARNATION_GENERATION.md)
- [Uncertainty Handling](UNCERTAINTY_HANDLING.md)
- [Game Master Responsibilities](GM_RESPONSIBILITIES.md)
- [NPC Generator](NPC_GENERATOR.md)
