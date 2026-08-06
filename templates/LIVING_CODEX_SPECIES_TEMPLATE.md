# Living Codex Species Entry Template

Use this blank template to propose or review one reusable species design for the [GM Living Codex Species Registry](../docs/gm-living-codex/SPECIES_REGISTRY.md). It extends the [Species Reference Template](SPECIES_TEMPLATE.md) and [Evolution Tree Template](EVOLUTION_TREE_TEMPLATE.md); it does not duplicate the facts those contracts own.

The repository keeps this template blank. Populated Living Codex entries belong in the separately deployed Living Codex database, and campaign-local species remain in external Campaign Records.

## Document Control

- **Template owner:** GM Living Codex Species Registry
- **Primary mechanical owners:** Species Development, Monster Evolution, Skills, Magic, Souls, and the World Engine for their respective claims
- **Dependencies:** [GM Living Codex](../docs/gm-living-codex/GM_LIVING_CODEX.md), [Species Registry](../docs/gm-living-codex/SPECIES_REGISTRY.md), [Species Reference Template](SPECIES_TEMPLATE.md), and [Evolution Tree Template](EVOLUTION_TREE_TEMPLATE.md)
- **Extensions:** Living Codex persistence records, variants, Evolution edges, procedural tags, and later compatibility records
- **Consumers:** human and AI GMs, species and ecosystem generators, player-species creation, Evolution adjudication, Campaign Configuration, and validation
- **Repository boundary:** no populated Codex entry, current population, campaign divergence, player character, location, discovery, or deployment locator belongs in this blank file

## Usage Guidance

1. Complete or reference one Species Reference payload for anatomy, lifecycle, ecology, variation, culture, Magic, and specialist interfaces.
2. Complete or reference Evolution Tree records only for established reusable routes.
3. Use this template for Codex identity, governance, inclusion, characteristic capability classification, reuse metadata, and validation.
4. Keep each fact under one owner and use stable typed references rather than copying it into several sections.
5. Treat every proposed entry as non-canonical until GM approval and the full Living Codex save procedure pass.
6. Keep campaign evidence summarized and anonymized; never copy live state into the proposal.

## Required Field Policy

Every **Required** field must be present for an accepted entry. A proposal may use `Not Yet Defined` only where the owning Codex procedure permits later definition; missing required identity, boundary, design basis, or validation blocks acceptance. **Conditional** fields become required when the feature exists. **Optional** omission grants nothing.

## Stable Identity

- **Species ID:** `<EC-SPECIES-000001>`
- **Preferred name:** `<current GM-approved display name>`
- **Aliases and former names:** `<typed alias records with language, context, and status>`
- **Entry type:** `<species | independently indexed form | stable lineage>`
- **Entry status:** `<proposed | active | deprecated | merged | split | replaced>`
- **Current revision ID:** `<stable revision reference>`
- **Parent revision:** `<revision reference or none>`
- **Created and last reviewed:** `<Codex metadata references>`
- **Approving authority:** `<authorized GM role or process>`

## Inclusion and Boundary

- **Reusable design need:** `<why future campaigns benefit from this entry>`
- **Species or form boundary:** `<origin, lifecycle, whole-body, continuity, and distinction>`
- **Broad classifications:** `<classification records and sources>`
- **Related species and forms:** `<typed relation -> Codex Stable ID>`
- **Convergent or shared-name cautions:** `<why similar labels or functions do not imply identity>`
- **Exclusion check:** `<why this is not only a unique boss, NPC, plot entity, temporary change, or campaign condition>`
- **Design intent:** `<themes, contrasts, use cases, and non-goals>`

## Species Core Payload

- **Species Reference:** `<linked Species Reference payload using SPECIES_TEMPLATE.md>`
- **Sapience and cognition:** `<range, structures, learning, agency, personhood considerations>`
- **Anatomy, scale, and body plan:** `<ordinary range and meaningful variation>`
- **Lifecycle and renewal:** `<origin, stages, maturation, continuity routes, and dependencies>`
- **Habitat and ecological niche:** `<requirements, roles, diet, predators, prey, symbioses, pressures>`
- **Behavior and communication:** `<ranges, instincts, media, barriers, and learning>`
- **Social and cultural possibilities:** `<structures and variation without one species personality>`
- **Magical architecture:** `<Mana Relations, organs, Channels, access, costs, and owner links>`
- **Soul interface:** `<only established species-level Soul considerations and limits>`

## Traits and Capability Routes

Repeat the relevant record block. A capability appears once under its narrowest truthful category.

### Innate Trait Record

- **Trait ID and name:** `<stable reference>`
- **Subtype:** `<inherited | passive biological | sensory | natural weapon | Magical Organ | another Species Trait>`
- **Owner and origin:** `<canonical owner and form source>`
- **Ordinary availability:** `<range and variation>`
- **Expression conditions:** `<maturation, health, environment, resources, or support>`
- **Costs, limits, suppression, and failure:** `<bounded consequences>`
- **Learned-control boundary:** `<what requires Skill or Development>`

### Characteristic Skill Route

- **Skill ID and name:** `<stable Skill reference>`
- **Category:** `<Instinctive | Typical Learned | Cultural | Rare Species | Evolution>`
- **Skill owner:** `<Skill Engine owner>`
- **Species receiving route:** `<anatomy, instinct, ecology, culture, or evolved form>`
- **Access population:** `<all | many | some | exceptional, with qualitative basis>`
- **Current-life requirements:** `<practice, instruction, feedback, Development, tools, or institutions>`
- **Limits and failure:** `<embodiment, access, reliability, cost, and counterplay>`
- **Non-grant statement:** `<what listing the route does not award>`

## Evolution Graph

- **Evolution graph reference:** `<one or more EVOLUTION_TREE_TEMPLATE-derived records>`
- **Source nodes:** `<Codex Stable IDs>`
- **Route identities:** `<stable edge IDs>`
- **Destination nodes:** `<Codex Stable IDs>`
- **Multiple branches supported:** `<yes, no, or not applicable with basis>`
- **Restrictions and commitments:** `<route-specific references>`
- **Gained and lost traits:** `<owner-routed references>`
- **Evolution Skill access:** `<Skill references without mastery grants>`
- **Further routes and graph status:** `<known, hidden, deprecated, replaced, or open>`

## Variants and Divergences

### Reusable Variant

- **Variant ID:** `<stable variant reference>`
- **Base species ID:** `<Codex Stable ID>`
- **Causal differences:** `<environment, origin, pressure, or stable structure>`
- **Altered traits, Skills, Evolution, ecology, and Magic:** `<typed references>`
- **Reuse scope:** `<contexts and limits>`
- **Species-distinction review:** `<why this remains a variant or needs a species ID>`

### Campaign Divergence Boundary

- **Permitted divergence points:** `<claims a campaign may adapt explicitly>`
- **Promotion considerations:** `<evidence needed for later reusable review>`
- **External-state reminder:** `Campaign divergences remain outside the Living Codex until promoted.`

## Procedural Generation

- **Search tags:** `<habitat, ecology, body, senses, cognition, society, Magic, route, and theme tags>`
- **Ecological roles:** `<bounded reusable functions>`
- **Reuse priority:** `<frequent | contextual | specialist | manual review>`
- **Redundancy notes:** `<nearby entries and meaningful distinction>`
- **Player-species considerations:** `<agency, communication, interface, support, and availability cautions>`
- **Generation exclusions:** `<contexts in which this entry is unsuitable>`

## Revision and Replacement

- **Change basis:** `<design correction, reusable promotion, expansion, merge, split, or deprecation>`
- **Affected record families:** `<exact set>`
- **Previous claim references:** `<stable revision references>`
- **Accepted claim references:** `<stable revision references>`
- **Aliases preserved:** `<yes/no and references>`
- **Merge, split, deprecation, or replacement links:** `<typed relations>`
- **Campaign migration note:** `<possible impact; never an automatic migration>`

## Validation Notes

- [ ] Stable ID is unique, immutable, correctly formatted, and not reused.
- [ ] Names and aliases do not substitute for identity evidence.
- [ ] The entry is reusable and not merely a unique person, boss, plot entity, or live campaign condition.
- [ ] Species boundary, origin, lifecycle, and design intent are explicit.
- [ ] Species Reference and Evolution records use their existing templates and owners.
- [ ] Traits, instincts, Skills, Development, Magic, Souls, and Evolution remain distinct.
- [ ] Characteristic Skills are access routes rather than mandatory packages or inherited mastery.
- [ ] Evolution graph supports contextual branches without becoming deterministic, exhaustive, or ranked.
- [ ] Variants remain distinct from individual variation and campaign divergence.
- [ ] Procedural tags do not create mechanics, power ranks, or guaranteed selection.
- [ ] Player-species use does not waive Reincarnation, embodiment, agency, or campaign-availability rules.
- [ ] No campaign state, theory, Secret, personal capability, current population, or deployment credential entered the record.
- [ ] GM approval, Codex transaction, validation, deployment, and backup are required before activation.

## Cross-References

- [GM Living Codex](../docs/gm-living-codex/GM_LIVING_CODEX.md)
- [Species Registry](../docs/gm-living-codex/SPECIES_REGISTRY.md)
- [Species Development](../docs/progression/SPECIES_DEVELOPMENT.md)
- [Monster Evolution](../docs/monster-evolution/README.md)
- [Monster Skill Trees](../docs/skills/MONSTER_SKILL_TREES.md)
- [Magic](../docs/magic/README.md)
- [World Engine](../docs/world-engine/README.md)
- [Monster Generator](../docs/gm/MONSTER_GENERATOR.md)
- [Species Reference Template](SPECIES_TEMPLATE.md)
- [Evolution Tree Template](EVOLUTION_TREE_TEMPLATE.md)
