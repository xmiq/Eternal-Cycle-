# Living Codex Species Registry

## Purpose

This document defines the Species Registry, the first and primary module of the [GM Living Codex](GM_LIVING_CODEX.md). It establishes stable species identity, reusable record structure, capability ownership, Evolution graphs, variants, campaign divergences, procedural-generation metadata, player-species integration, and review procedures.

The Registry stores reusable GM-approved design. It does not store current creatures, populations, discoveries, locations, player theories, or campaign history.

## Core Rule

A **Species Registry Entry** identifies one reusable species or sufficiently distinct form through a stable Codex ID and an owner-routed design record.

An entry describes a bounded range of possible embodiment, lifecycle, traits, capability access, ecology, social possibility, Magic, and Evolution. It does not describe every member, grant every listed capability, prove presence in a campaign, or make the design player knowledge.

Every claim remains owned by its canonical system:

- [Species Development](../progression/SPECIES_DEVELOPMENT.md) owns present-species structures, maturation, traits, and body-bound potential;
- [Monster Evolution](../monster-evolution/README.md) owns Evolution Routes, transitions, resulting forms, and route safeguards;
- the [Skill Engine](../skills/README.md) owns learned capabilities and Skill structures;
- [Magic](../magic/README.md) owns magical phenomena, access, sources, procedures, costs, and failure;
- the [Soul Engine](../soul/README.md) owns Soul identity, persistence, access, and Reincarnation;
- the [World Engine](../world-engine/README.md) owns current populations, ecology, resources, migration, and world consequences;
- the Living Codex owns the reusable design record and its revision history only.

## Stable Identity and Indexing

### Species IDs

Each species or independently indexed form receives one permanent **Codex Stable ID**. The preferred format is:

```text
EC-SPECIES-000001
```

IDs are immutable, unique within the Codex, never reused, and independent of name, language, classification, popularity, or campaign presence.

### Names and Aliases

An entry has one current preferred name and zero or more aliases. Every alias records its language or naming context, scope, status, and relation to the preferred name when known.

Two unrelated designs may share the same ordinary name. A label such as `Elf`, `Dragon`, or `Slime` never proves shared identity. Search may return several candidates and must compare stable IDs and design content.

Former names remain aliases after renaming. Offensive, misleading, obsolete, or erroneous labels may be retained in protected metadata when needed for traceability without remaining preferred display terms.

### Revision Identity

Each accepted change creates an append-preserving revision associated with:

- the stable entry ID;
- a unique revision identifier;
- its parent revision or revisions;
- the change basis and approving GM authority;
- affected record families;
- review and validation results;
- effective Codex version;
- supersession, deprecation, or replacement relations where applicable.

The current revision is selected explicitly. Modification time and latest-looking filename do not establish authority.

### Renaming

Renaming preserves the stable species ID. The prior preferred name becomes an alias with a recorded transition. Campaigns may continue using an older name without changing the underlying reference.

### Merging

Merge entries only when affirmative design evidence shows they describe the same reusable species boundary. Preserve every former ID as a retired identity reference, identify the surviving ID, reconcile conflicting claims through their owners, and mark possible campaign migration impact without identifying or rewriting campaigns automatically.

### Splitting

Split an entry when one record incorrectly combined materially distinct species or forms. Preserve the former record as deprecated, create distinct stable IDs, map only supported claims, and leave uncertain attribution explicit. Shared ancestry or appearance does not make either successor the automatic original.

### Deprecation and Replacement

Deprecation means an entry should not be selected for new reuse without review. It does not delete history or invalidate campaigns that already adopted it. Replacement identifies one or more preferred successor entries and the reason; it does not silently migrate a campaign.

## Species Boundary

Define why an entry represents one species, form, or stable lineage before cataloging features. Consider:

- coherent origin and renewal route;
- whole-body organization and lifecycle;
- material, biological, magical, spiritual, artificial, or symbiotic structure;
- ordinary variation and maturation;
- ability to maintain a repeatable lineage or another stable continuity route;
- distinction from temporary transformation, individual Mutation, injury, equipment, culture, profession, or cosmetic variation;
- relationship to parent, convergent, hybrid, constructed, or evolved forms.

Taxonomy may remain disputed in a campaign, but the Codex design boundary must be explicit enough for reuse. A disputed in-world classification does not force the Registry to merge designs.

## Species Core Record

Every entry supports the following logical fields. Storage may normalize them across tables, but no implementation may discard their meaning.

### Identity and Governance

- stable species ID;
- preferred name;
- aliases and former names;
- broad classifications and their sources;
- entry status and current revision;
- creation, review, deprecation, merge, split, and replacement metadata;
- design intent and inclusion basis;
- canonical mechanical owners and typed references.

### Sapience Cognition and Personhood

- sapience and cognition structures, variation, and uncertainty;
- attention, memory, learning, and decision routes;
- personhood considerations without category-based denial;
- distributed, colony, collective, artificial, or multiple-component boundaries;
- ordinary agency and communication constraints.

Species classification never determines morality, allegiance, culture, hostility, or individual worth.

### Anatomy Scale and Body Plan

- materials, body organization, size and scale ranges;
- movement and manipulation routes;
- sensory structures and signal limits;
- natural weapons and defenses;
- regulation, recovery, sustenance, waste, rest, and maintenance;
- Magical Organs, Channels, cores, interfaces, or metaphysical anatomy;
- ordinary vulnerabilities, dependencies, and Body Compatibility limits;
- meaningful individual and population variation.

Anatomy creates access. It does not grant Skill mastery, perfect control, extra actions, or universal superiority.

### Lifecycle and Renewal

- origin of individuals;
- lifecycle stages and transition windows;
- maturation, Metamorphosis, aging, senescence, dormancy, or renewal;
- reproductive, spawning, construction, symbiotic, magical, or other continuity routes at the level already owned by canon;
- care, environmental, resource, social, magical, or infrastructure requirements;
- mortality and ordinary failure routes.

Detailed reproductive systems and inheritance outcomes remain outside this Registry design. [Reproductive Compatibility](REPRODUCTIVE_COMPATIBILITY.md) adds only a sparse directional initiation, viability, and fertility scaffold, and the future lineage module owns resulting lineage and inherited expression. Compatibility rows remain separate relations rather than embedded assumptions in every species record.

### Habitat Ecology and Diet

- habitats and environmental tolerances;
- Ecological Role and niche;
- diet, sustenance, energy, Mana, or material intake;
- predators, prey, competitors, decomposers, hosts, symbionts, and dependencies;
- Territory, migration, dispersal, disturbance, and recovery tendencies;
- carrying and density constraints without universal population values;
- ecosystem functions, pressures, externalities, and Counter-Ecology.

The entry records reusable relationships and requirements. Current distributions, counts, shortages, extinctions, and migration events remain campaign state.

### Behavior Communication and Society

- characteristic behavior ranges and Current Instincts;
- communication media, learning requirements, and barriers;
- social structures and variation;
- culture, institutions, roles, traditions, law, religion, craft, and technology where applicable;
- relations with other species as reusable possibilities rather than current diplomacy;
- cultural possibilities without one mandatory species culture.

Biology may shape pressures and access. It does not prescribe personality or social destiny.

### Magical Architecture and Soul Interface

- typical Mana Relations, sources, Channels, Magical Organs, affinities, and environmental dependencies;
- what is structural, what is learned, and what requires external authority or support;
- Soul-related traits only where an existing Soul rule permits them;
- ordinary Reincarnation candidate constraints and unusual embodiment considerations;
- limits on Retained Instinct, Skill crossover, Soul Title, Soul Resonance, and Soul Weapon interfaces.

The entry cannot grant a Soul, affinity, spell, magical mastery, divine relationship, or Reincarnation privilege by category alone.

## Traits and Capability Ownership

### Innate Traits

An **Innate Trait** is a Species Trait available through the current form's valid origin without learned acquisition. It may be inherited, constructed, spawned, awakened, magically organized, or supplied through another established origin.

Innate does not mean fully mature, always expressed, perfectly controlled, heritable, or present in every member. Record expression conditions, variation, costs, suppression, and owner.

### Passive Biological Traits

Passive biological traits continuously or conditionally perform body functions such as insulation, filtration, repair, buoyancy, toxin processing, or pressure regulation. They remain Species Traits, consume actual resources, and can fail or be overwhelmed.

### Sensory Traits

Sensory traits provide a signal route. They do not grant learned interpretation, perfect range, attention, immunity to noise, or truthful conclusions. Record medium, resolution, limits, interference, fatigue, and calibration needs.

### Instinctive Skills

An **Instinctive Skill** is a bounded learned-capability pattern for which current species instinct supplies an initial receiving route or, where a local Skill scale uses that label, Level 0 expression. Species origin may make the basic pattern accessible; present embodiment, feedback, practice, and Development still determine reliable use.

Current Instinct and Instinctive Skill remain distinct. An orientation toward nesting or pursuit is not automatically a full construction or tracking Skill.

### Typical Learned Skills

These are Skills members commonly develop because embodiment, ecology, work, or recurring problems create opportunity. They are not granted at birth and not mandatory for every member.

### Cultural Skills

These are Skills transmitted through a culture, family, institution, profession, tradition, or teaching ecology. They belong to social learning rather than biology, may be unavailable to isolated members, and may be learned by outsiders with valid access.

### Rare Species Skills

These are Skills available to unusual members through bounded conditions such as atypical anatomy, a rare source, specialized Development, a hidden route, or exceptional instruction. Rarity is a distribution claim, not provenance or power.

### Evolution Skills

These are Skills given an initial receiving route, made newly available, or, where a local Skill scale uses that label, made available at Level 0 because an Evolved Form creates valid access. Evolution never grants mature mastery merely by listing the Skill.

### Capability Record Rule

For every listed capability, record:

- category and owning system;
- source and receiving route;
- whether access is ordinary, conditional, cultural, rare, or Evolution-bound;
- what the body supplies and what must be learned;
- variation, requirements, costs, failure, and counterplay;
- whether all, many, some, or exceptional members may access it, without turning that label into a numerical prevalence formula.

Do not list one capability in several categories to imply stacking. The smallest truthful owner controls.

## Evolution Graph

The Registry stores a descriptive directed graph of established Evolution Routes. It does not store a deterministic upgrade menu.

Each Evolution edge supports:

- stable route identity;
- source and destination species or forms;
- evolutionary role and route provenance;
- structural possibility, local prerequisites, and body compatibility;
- environmental and Evolutionary Pressures;
- behavior, relationship, Development, magical, and Soul requirements where applicable;
- Transition Window, process, agency, costs, vulnerability, and failure;
- inherited, retained, altered, suppressed, lost, and gained Species Traits;
- newly available Evolution Skills without inherited mastery;
- branch restrictions, commitment, convergence, divergence, and further routes;
- ecology, population, society, identity, and World Engine consequences;
- discoverability, clues, hidden-condition protection, and information scope;
- procedural-generation guidance and reuse intent.

One base form may have several branches:

```text
Cave Gatherer
  |-> Forgekeeper
  |-> Crystal Shepherd
  `-> Tunnel Warden
```

This diagram is an unpopulated illustration of branch structure, not a Codex entry. Forgekeeper is one possible outcome, not the inevitable destiny of every Cave Gatherer.

Before creating a route, search for an existing functionally equivalent edge. Cosmetic naming does not justify duplication. A new branch needs distinct provenance, form, tradeoffs, and consequences.

## Variants

A reusable variant records:

- its stable variant identity and base species ID;
- exact differences from the base;
- causal environment, origin, or persistent pressure;
- altered traits and expression;
- altered Skill access;
- altered Evolution access;
- altered ecology and magical architecture;
- separately established Reproductive Compatibility relations where the altered structures are materially relevant;
- ordinary variation and stability;
- reuse intent;
- whether it remains a variant or has become distinct enough for a species ID.

Do not create a variant for one individual's injury, learned Skill, equipment, temporary transformation, cosmetic difference, or current campaign condition.

Assign a distinct species ID when the form boundary, lifecycle, renewal route, whole-body organization, Evolution ownership, or reusable design identity is materially distinct enough that treating it as ordinary variation would obscure causality. No fixed difference count decides this.

## Campaign Divergences

A campaign-specific divergence remains in external Campaign State and references the base Codex entry and revision. It may alter local anatomy, ecology, culture, Magic, traits, Skills, Evolution access, or interpretation through an established campaign cause.

The base Codex entry remains unchanged. A reusable divergence may later be reviewed for promotion as a variant or species. Promotion creates a Codex revision and never copies live population, location, relationship, discovery, or player information into the Registry.

## Procedural-Generation Integration

Each entry supports searchable procedural metadata:

- habitat and climate tags;
- ecological roles and resource-web functions;
- body plan, scale, movement, senses, and material tags;
- cognition, communication, society, culture, and institution tags;
- magical architecture and Mana-relation tags;
- progression-tree and Evolution-route tags;
- player-species suitability considerations;
- design themes, intended contrasts, and redundancy notes;
- reuse priority;
- exclusions, required review, and incompatibilities.

Reuse priority is qualitative guidance such as `frequent`, `contextual`, `specialist`, or `manual review`. It does not rank power, force selection, establish campaign presence, or replace world causality.

The generator first narrows by actual world need, then searches relevant tags, reads candidate entries, compares owner constraints, and chooses reuse, explicit divergence, or new campaign-local design. Tags never create mechanics by matching.

## Player-Species Integration

Playable species use the same entry contract and IDs. A player-species flag or suitability note may identify presentation, agency, communication, embodiment, support, or interface considerations, but it does not grant campaign availability or waive Reincarnation candidate rules.

Character state references the adopted Codex entry and any Campaign Divergence, then stores the current individual's body, Development, Skills, Soul, relationships, equipment, condition, and history separately.

## Registry Operating Procedure

When a reusable species, variant, or Evolution design is needed:

1. state the world-design need and whether the request concerns a species, form, route, variant, or individual;
2. search stable IDs, names, aliases, classifications, tags, functions, and related forms;
3. compare candidate boundaries, current revisions, deprecations, and replacement links;
4. reuse a suitable entry when its design fits the intended world;
5. record an explicit campaign divergence when limited campaign-specific differences matter;
6. create a campaign-local Species Reference when no suitable reusable entry exists;
7. separate species structure from individual history, learned capability, and current world state;
8. route every trait, Skill, Evolution, magical, Soul, ecological, and social claim to its owner;
9. test stable identity, ordinary variation, lifecycle, resource needs, ecology, agency, and counterplay;
10. check for duplicate names, duplicate functions, unsupported combinations, and one-off content;
11. promote only a GM-approved reusable design, assigning stable identity and complete revision metadata;
12. apply the Living Codex full-save, validation, deployment, and backup procedure before claiming the entry is canonical.

## Worked Boundary Examples

### Two Species Called Elf

One design is a photosynthetic, colony-linked lineage; another is a long-lived humanoid people with no shared origin. The ordinary name is an alias in each context. Distinct body plans, origins, lifecycles, and design identities require distinct Codex Stable IDs.

### Unique Boss and Reusable Species

A singular cavern tyrant has a one-off divine alteration and personal Soul Title. The individual is excluded. If the underlying tunnel-dwelling species has reusable anatomy, ecology, Skills, and Evolution Routes, that species may receive its own entry without importing the boss's unique state.

### Campaign Variant

A campaign's Cave Gatherers adapt to a local crystal fault and gain a bounded route unavailable elsewhere. The campaign records the divergence. If later review finds the form coherent and reusable, the GM may promote it as a Crystal Shepherd variant or distinct species while preserving the base and campaign histories separately.

## Safeguards

- Do not treat a species name as stable identity.
- Do not merge entries by appearance, role, or alias alone.
- Do not list campaign populations, characters, locations, discoveries, or current conditions.
- Do not promote player theories, rumours, or generated drafts automatically.
- Do not turn traits into Skills or list anatomy as mastery.
- Do not make characteristic Skills mandatory for every member.
- Do not grant a future form's traits or Skills because an Evolution edge is recorded.
- Do not make branches deterministic, exhaustive, ranked, or universally available.
- Do not create variants from temporary or individual differences.
- Do not treat procedural tags as mechanics or guaranteed selection.
- Do not make a player species a separate mechanical category.
- Do not let a Codex revision silently rewrite Campaign Canon.

## Scope Boundaries

This document defines reusable species records and their interfaces. It does not define a species catalog, populate a Living Codex, establish a campaign species, assign current populations, implement storage, define Reproductive Compatibility, or define later lineage and inheritance rules.

## Related Documents

- [GM Living Codex Index](README.md)
- [GM Living Codex Design](GM_LIVING_CODEX.md)
- [Species Development](../progression/SPECIES_DEVELOPMENT.md)
- [Monster Evolution Rules](../monster-evolution/README.md)
- [Branching Evolution](../monster-evolution/BRANCHING_EVOLUTION.md)
- [Hybridization](../monster-evolution/HYBRIDIZATION.md)
- [Reproductive Compatibility](REPRODUCTIVE_COMPATIBILITY.md)
- [Monster Skill Trees](../skills/MONSTER_SKILL_TREES.md)
- [Skill Engine Safeguards](../skills/SKILL_ENGINE_SAFEGUARDS.md)
- [Magic Rules](../magic/README.md)
- [Soul Rules](../soul/README.md)
- [World Engine](../world-engine/README.md)
- [Monster Generator](../gm/MONSTER_GENERATOR.md)
- [Species Reference Template](../../templates/SPECIES_TEMPLATE.md)
- [Evolution Tree Template](../../templates/EVOLUTION_TREE_TEMPLATE.md)
- [Living Codex Species Entry Template](../../templates/LIVING_CODEX_SPECIES_TEMPLATE.md)
