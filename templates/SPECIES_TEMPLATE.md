# Species Reference Template

Use this storage-neutral template to describe the ordinary range of one species or distinct form without turning that range into an individual stat block, universal behaviour, fixed culture, power tier, or automatic Evolution route.

The repository keeps this template blank. A campaign-local Species Reference remains in an external Campaign Record. A reusable cross-campaign design uses this template as a payload within the separately deployed [GM Living Codex](../docs/gm-living-codex/README.md); using this template does not grant either authority.

## Document Control

- **Template owner:** Species module of the [Campaign Persistence Engine](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md#species)
- **Primary mechanical owners:** [Species Development](../docs/progression/SPECIES_DEVELOPMENT.md), [Monster Evolution](../docs/monster-evolution/README.md), and the specialist owner of each referenced effect
- **Dependencies:** Development, Skills, ecology, Magic, Human or monster social frameworks, Souls, and persistence as applicable
- **Extensions:** [Evolution Tree Template](EVOLUTION_TREE_TEMPLATE.md), [Character Record Template](CHARACTER_TEMPLATE.md), campaign Population records, Research, and World-State records
- **Consumers:** character creation, Reincarnation candidate generation, monster generation, Evolution adjudication, ecology simulation, Research, and validation
- **Repository boundary:** no current population, discovered route, location, named lineage, or live campaign condition belongs in this blank file

This template organizes a Species Reference. It owns no anatomy, trait, route, capability, or world fact independently of the canonical source recorded for that claim.

## Usage Guidance

1. State whether the intended output is a campaign-local Species Reference or a separately governed reusable reference proposal.
2. For a reusable proposal, use the [Living Codex Species Entry Template](LIVING_CODEX_SPECIES_TEMPLATE.md) as the governing wrapper and this file only for its Species Reference payload.
3. Define the species or form boundary before listing traits. Similar appearance, ancestry, function, or name does not by itself establish one species.
4. Describe ranges, dependencies, variation, and evidence instead of a single ideal specimen.
5. Separate inherited or form-native potential from current expression, learned control, individual Development, and contextual effectiveness.
6. Link Evolution Routes, Skills, cultures, magical procedures, and current world state to their owners rather than embedding them as species bonuses.
7. Use explicit unknown and disputed states where the campaign has incomplete evidence.
8. Validate every populated campaign record before activation and every reusable proposal through Living Codex governance.

## Required Field Policy

**Required** fields must be present or carry an explicit `Unknown`, `Not Yet Verified`, `Disputed`, or `Requires Source Recovery` state. **Conditional** fields become required only when their stated feature exists. **Optional** fields grant nothing when omitted.

## Record Contract

### Required Identity

- **Record ID:** `<stable Species Reference ID>`
- **Record type:** `Species Reference`
- **Reference mode:** `<campaign-local | Living Codex payload proposal>`
- **Authoritative module:** `Species`
- **Species or form name:** `<primary current label or explicit unknown>`
- **Subject scope:** `<species, form, stage, lineage range, or another bounded taxon>`
- **Record version:** `<revision>`
- **Campaign or repository version:** `<applicable version reference>`

### Required Authority and Provenance

- **Persistence authority:** `<applicable authority layer>`
- **Truth layer:** `<Campaign Canon | Historical Record | Research | another valid layer>`
- **Canonical mechanical owners:** `<rule owner for each claim family>`
- **Status:** `<active | proposed | disputed | superseded | extinct in stated scope | unknown | owner-defined state>`
- **Effective scope:** `<world, Contact Domain, region, interval, population range, and conditions>`
- **Source basis:** `<observation, lineage evidence, Research, canonical reference, generation procedure, migration, correction, or other source>`
- **Established at:** `<Timeline reference or explicit unknown>`
- **Last integrated at:** `<Save Point, Transaction, repository revision, or not yet integrated>`
- **Uncertainty:** `<claim-specific evidence state>`
- **Typed references and dependents:** `<relation type -> stable record ID>`
- **Validation status:** `<Validation Run, review result, warnings, or not yet validated>`

## Classification and Boundary

### Required

- **Progression tree:** `<human | monster | another explicitly established receiving tree>`
- **Form boundary:** `<features that make this a distinct form rather than ordinary variation>`
- **Related forms or species:** `<typed relation and evidence>`
- **Source lineage:** `<inheritance, origin, creation, emergence, or explicit unknown>`
- **Ordinary continuity:** `<what persists through maturation, injury, seasonal change, adaptation, or other non-Evolution variation>`

### Conditional or Optional

- **Local names and classifications:** `<observer, language, scope, and equivalence>`
- **Disputed taxonomy:** `<competing claims, evidence, and authority>`
- **Convergent resemblance:** `<similar function with distinct provenance>`
- **Known Hybrid origin:** `<source lineages, process, integration, and owner record>`

Classification is descriptive. It is not a rarity grade, threat rank, moral category, or universal power level.

## Ordinary Embodiment

### Required

- **Body plan and scale range:** `<ordinary range and material variation>`
- **Movement modes:** `<available routes, constraints, and environmental dependencies>`
- **Manipulation routes:** `<limbs, appendages, interfaces, tools, or limitations>`
- **Sensory systems:** `<signals, ordinary range, noise, limitations, and interpretation needs>`
- **Cognition and attention:** `<ordinary structures, variation, and constraints>`
- **Communication routes:** `<signals, media, learning requirements, and barriers>`
- **Regulation and recovery:** `<temperature, respiration, circulation, repair, rest, or analogous processes>`
- **Essential organs, cores, Channels, or distributed structures:** `<function, dependencies, and failure consequences>`
- **Ordinary vulnerabilities:** `<causal weaknesses rather than universal counters>`

### Conditional or Optional

- **Natural weapons:** `<structure, source, potential, costs, and control requirements>`
- **Defensive structures:** `<source, maintenance, limits, and tradeoffs>`
- **Magical organs or metaphysical anatomy:** `<Mana Relation and specialist links>`
- **Composite, colony, or distributed body:** `<identity, coordination, component, and action limits>`
- **Environmental or symbiotic body dependency:** `<partner or condition and failure route>`

Anatomy creates possible expression. It does not grant learned technique, safe use, perfect perception, extra turns, or mastery.

## Lifecycle and Reproduction

### Required

- **Origin of individuals:** `<birth, hatching, budding, assembly, spontaneous formation, transformation, or another established route>`
- **Lifecycle sequence:** `<ordinary stages and transition windows>`
- **Maturation:** `<developmental changes, needs, variability, and constraints>`
- **Renewal requirements:** `<reproduction, recruitment, maintenance, habitat, social, magical, or material dependencies>`
- **Mortality and senescence:** `<ordinary causes, aging pattern, repair limits, or explicit unknown>`

### Conditional or Optional

- **Metamorphosis:** `<source, stages, vulnerability, reversibility, and distinction from Evolution>`
- **Inheritance:** `<what may pass, through which route, with what variation and expression conditions>`
- **Dormant stages or Refugia:** `<viability, activation, and ecological role>`
- **Care structures:** `<parental, communal, institutional, symbiotic, or environmental support>`

Elapsed time, reproduction count, or maturation alone does not establish Skill mastery, Evolution, social status, or Soul progression.

## Species Potential and Trait Expression

Repeat this block for each material Species Trait.

### Trait Record

- **Trait ID and name:** `<stable reference>`
- **Trait type:** `<Inherited Trait | Natural Weapon | Sensory System | Magical Organ | Current Instinct | cognition | another established type>`
- **Owning system:** `<Species Development or another specialist>`
- **Form basis:** `<anatomy, lifecycle, lineage, magic, symbiosis, or other source>`
- **Species Potential:** `<ordinary possible range>`
- **Expression conditions:** `<maturation, health, environment, resources, support, or other conditions>`
- **Individual variation:** `<known range and causes>`
- **Current access versus learned control:** `<what is native and what requires practice>`
- **Costs and dependencies:** `<maintenance, fatigue, materials, risks, and tradeoffs>`
- **Failure or suppression:** `<causes, symptoms, recovery, and uncertainty>`
- **Interactions:** `<Skills, Development, Magic, ecology, technology, or society>`

Traits are not portable Skills, automatic mastery, flat bonuses, or proof that every member expresses the same capability.

## Needs, Habitat, and Ecology

### Required

- **Sustenance and resource needs:** `<sources, quality, frequency, substitutions, and limits>`
- **Habitat requirements:** `<conditions, range, connectivity, shelter, nesting, or analogous needs>`
- **Ecological Role:** `<functions and relationships rather than an enemy category>`
- **Resource-web relationships:** `<producers, consumers, prey, predators, decomposers, competitors, symbionts, or other links>`
- **Carrying and density constraints:** `<limiting supports and pressures without a universal cap>`
- **Territory and movement patterns:** `<ordinary routes, variability, and causes>`
- **Disturbance and recovery:** `<responses, buffers, migration, adaptation, decline, or replacement>`

### Conditional or Optional

- **Counter-Ecology:** `<credible pressures and checks>`
- **Dungeon relationship:** `<Sustaining Basis, access, dependence, and consequences>`
- **Civilization interaction:** `<hunting, domestication, trade, conflict, stewardship, infrastructure, or coexistence>`
- **Disease relationships:** `<host, carrier, resistance, vulnerability, or ecological effects>`

Actual population size, territory, migration, scarcity, and extinction status are campaign World-State claims and remain external.

## Development and Skill Interfaces

### Required

- **Body Potential range:** `<ordinary embodied opportunities and limits>`
- **Physical Calibration needs:** `<movement, senses, regulation, natural tools, and recovery>`
- **Likely Development opportunities:** `<tracks supported by the form, without fixed outcomes>`
- **Bottlenecks and incompatibilities:** `<body, environment, cognition, tools, culture, or resources>`
- **Native Skill-tree owner:** `<human | monster | another established tree>`
- **Receiving routes:** `<ways learned capabilities may be expressed through this form>`

### Conditional or Optional

- **Species-native Skill traditions:** `<socially transmitted route references>`
- **Human or monster crossover:** `<explicit bounded Reincarnation route>`
- **Common accommodations:** `<tools, teaching, interfaces, or environments>`
- **Retained Development compatibility:** `<domain-specific comparison; never a universal transfer percentage>`

A species description may identify opportunity and constraint. It cannot assign identical Skills, Classes, Professions, Development, personality, or tactics to every member.

## Magic Interface

### Conditional Fields

- **Mana Relation:** `<source, medium, environment, and body relationship>`
- **Species-native magical structures:** `<organ, Channel, pattern, authority, or other source>`
- **Affinity tendencies:** `<target-specific evidence and variation>`
- **Access requirements:** `<embodiment, training, source, consent, tools, or environment>`
- **Costs, recovery, and ecological effects:** `<specialist references>`

No species has universal spell access, unlimited Mana, automatic Magical Development, or a guaranteed affinity rank.

## Evolution, Mutation, and Hybridization

### Conditional Fields

- **Species Stages:** `<stage structure and lifecycle owner>`
- **Evolution-tree reference:** `<EVOLUTION_TREE_TEMPLATE-derived record>`
- **Known Evolution Routes:** `<route IDs, provenance, clues, tradeoffs, and current knowledge scope>`
- **Hidden Evolution Conditions:** `<protected truth reference and fair clue routes>`
- **Mutations:** `<source, expression, stability, inheritance, load, and owner>`
- **Hybrid Forms:** `<source lineages, origin, integration, constraints, and reproduction>`
- **Apex domains:** `<bounded ecological domain, dependencies, counterplay, and population limits>`
- **Regression or traps:** `<causal route and changed conditions>`

The Species Reference describes possible structures. Pressure, kills, consumption, suffering, elapsed time, a universal level, or a listed branch never guarantees transition.

## Intelligence, Culture, and Society

### Conditional Fields

- **Personhood and agency considerations:** `<evidence, variation, and safeguards>`
- **Social organization range:** `<solitary, pair, kin, colony, settlement, institution, or other patterns>`
- **Communication and learning ecology:** `<routes, barriers, teachers, records, and institutions>`
- **Cultures and traditions:** `<separate references; never one species personality>`
- **Professions, Classes, or roles:** `<framework references where actually established>`
- **Law, religion, technology, and Magic:** `<social owner references>`
- **Relations with other species:** `<historical and current campaign references>`

Biology may shape pressures and access. It does not determine morality, ideology, allegiance, intelligence, culture, or individual choice.

## Soul and Reincarnation Interface

### Conditional Fields

- **Soul eligibility or unusual embodiment:** `<Soul Engine source and limits>`
- **Reincarnation candidate constraints:** `<valid body, world, species, and personhood conditions>`
- **Retained Instinct translation:** `<cue, body compatibility, access, and agency>`
- **Soul Resonance or Title interactions:** `<target-specific source and contextual effect>`
- **Soul Avatar association:** `<association evidence without predetermined form>`
- **Soul Weapon compatibility considerations:** `<purpose-specific interface rather than universal match>`

A former species does not persist as current anatomy. Soul history may support access, recognition, or relearning only through its own bounded owner.

## Population, History, and Knowledge

### Campaign-External Fields

- **Known populations and lineages:** `<stable Population IDs>`
- **Current distribution:** `<Location and World-State references>`
- **Population trend and uncertainty:** `<scope and evidence>`
- **Extinction, replacement, or recovery history:** `<Timeline and causal records>`
- **Research status:** `<observations, theories, confidence, confirmation, and disputes>`
- **Character and public knowledge:** `<observer-specific Knowledge and Rumour references>`
- **Names, myths, errors, and classifications:** `<source and truth layer>`

Do not place any populated version of these fields in the repository template.

## Validation Checklist

- [ ] The repository copy remains blank; any campaign-local record is external.
- [ ] Reusable reference material, if proposed, follows Living Codex governance and contains no live state.
- [ ] The species or form boundary is explicit and supported.
- [ ] Ordinary range is separated from individual variation and current population state.
- [ ] Anatomy, Species Potential, Trait Expression, learned control, and Practised Reliability remain distinct.
- [ ] No trait silently becomes a Skill, spell, Class, Profession, Soul effect, or Evolution.
- [ ] Lifecycle, reproduction, ecology, resources, and dependencies are causally coherent.
- [ ] Personhood and agency are preserved; species does not determine morality, allegiance, or behaviour.
- [ ] Human and monster progression trees remain distinct unless a bounded receiving route exists.
- [ ] Every Evolution Route has a valid owner and does not become an automatic menu or rank ladder.
- [ ] Mutations, Hybrid Forms, temporary changes, and ordinary variation retain separate provenance.
- [ ] Magic retains a Mana Relation, source, cost, access, embodiment, and specialist owner.
- [ ] Soul history grants no former anatomy, automatic mastery, or unrestricted crossover.
- [ ] No universal level, rarity tier, threat score, strongest species, mandatory branch, or encounter scaling appears.
- [ ] Current populations, locations, discoveries, extinctions, and world conditions remain external campaign data.
- [ ] Required fields, typed references, truth layers, uncertainty, and provenance validate.

## Canonical Dependencies

- [Species Development](../docs/progression/SPECIES_DEVELOPMENT.md)
- [Development System](../docs/progression/README.md)
- [Skill Engine](../docs/skills/README.md)
- [Monster Evolution](../docs/monster-evolution/README.md)
- [Monster Ecology](../docs/monster-evolution/MONSTER_ECOLOGY.md)
- [Monster Societies](../docs/monster-evolution/MONSTER_SOCIETIES.md)
- [Human Classes and Professions](../docs/human/README.md)
- [Magic](../docs/magic/README.md)
- [Soul Engine](../docs/soul/README.md)
- [World Engine](../docs/world-engine/README.md)
- [Monster Generator](../docs/gm/MONSTER_GENERATOR.md)
- [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
- [GM Living Codex Species Registry](../docs/gm-living-codex/SPECIES_REGISTRY.md)

## Extensions and Consumers

Use the [Evolution Tree Template](EVOLUTION_TREE_TEMPLATE.md) only for route structure, the [Living Codex Species Entry Template](LIVING_CODEX_SPECIES_TEMPLATE.md) only for reusable Codex governance, and the [Character Record Template](CHARACTER_TEMPLATE.md) only for one current individual. Population, ecology, Research, Location, and Timeline records consume the Species Reference without copying it into live world state.

Character creation, Reincarnation generation, monster generation, Skills, Magic, Evolution, and World simulation may consume only the fields relevant to their claim and must preserve uncertainty and owner boundaries.
