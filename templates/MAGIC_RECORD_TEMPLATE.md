# Magic Record Template

Use this storage-neutral template for a campaign-specific magical Profile, Instance, relationship, process, restriction, or world condition without creating generic MP, universal spell access, or a second progression system.

A populated record belongs in an external Campaign Record. A reusable magical rule requires ordinary canonical governance; this form grants no Mana, affinity, Spell, authority, source relationship, or magical capability.

## Document Control

- **Template owner:** Magic in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary mechanical owner:** [Magic Rules](../docs/magic/README.md)
- **Dependencies:** Mana Relation, source, access, embodiment, Skills, Magical Development, world laws, environment, and type-specific owners
- **Extensions:** Mana Context, Affinity, Spell, Ritual, Enchantment, Alchemy, Divine Magic, Forbidden Magic, and Magic-World Profiles
- **Consumers:** Character and NPC records, Skills, Classes, Monster Evolution, Soul Weapons, Inventory, Infrastructure, Research, world simulation, and validation
- **Repository boundary:** no named source, Reserve, active Spell, live Ritual, enchanted item, Batch, divine relationship, restriction case, or current world condition belongs here

## Usage Guidance

1. Select one record type and its exact specialist owner.
2. Establish a valid Mana Relation before resolving a magical effect.
3. Separate source, Access, Reserve, Capacity, control, affinity, authority, Skill, Development, and recognition.
4. Record current embodiment, world-law compatibility, costs, lifecycle, maintenance, failure, traces, and counterplay.
5. Preserve person-like source agency, affected-party consent, and alternatives.
6. Hand world consequences to the World Engine rather than absorbing them into Magic.

## Required Fields

- **Magic Record ID:** `<stable ID>`
- **Record type:** `<Mana Context | Affinity Profile | Spell Profile | Spell Instance | Ritual Profile | Ritual State | Enchantment Profile | Enchantment Layer | Alchemical Profile | Batch | Divine Profile | Divine relationship | Forbidden Magic Profile | restriction case | Magic-World Profile>`
- **Subject and scope:** `<exact person, source, Host, Pattern, process, site, material, question, or region>`
- **Authoritative owner:** `<specialist Magic document and campaign module>`
- **Effective interval:** `<Timeline reference>`
- **Truth Layer and visibility:** `<reference>`
- **Source basis and provenance:** `<events, observation, Research, instruction, or unknown>`
- **Repository and Campaign versions:** `<references>`
- **Validation status:** `<result or not yet validated>`

## Common Magical Claim

- **Intended or observed change:** `<target, scope, duration, and effect>`
- **Mana Relation:** `<source, medium, movement, conversion, and world-law relationship>`
- **Sources and participants:** `<distinct IDs, agency, and roles>`
- **Access:** `<relationship, permission, channel, tool, body, site, or other route>`
- **Embodiment:** `<organs, senses, cognition, Channels, health, and compatibility>`
- **Skill and Magical Development:** `<separate references>`
- **Environment and world laws:** `<current conditions>`
- **Supply and Capacity:** `<source-owned limits; never generic MP>`
- **Cost and bearer:** `<cause, timing, recovery, and externalities>`
- **Lifecycle:** `<formation, activation, maintenance, termination, aftermath, and persistence>`
- **Failure and counterplay:** `<warning, interruption, resistance, recovery, and consequences>`
- **Traces and information:** `<observable evidence, uncertainty, concealment, and Research>`

## Type-Specific Fields

- **Mana Context:** `<state, sources, movement, storage, conversion, depletion, ecology, and Drift>`
- **Affinity:** `<Target, Basis, scope, responsiveness, expressions, conflicts, and evidence>`
- **Spell:** `<Formation Grammar, Pattern, targeting, requirements, expression, termination, and Instance state>`
- **Ritual:** `<roles, components, site, sequence, transfers, substitutions, integrity, interruption, and aftermath>`
- **Enchantment:** `<Host, Functions, Patterns, Anchors, Interfaces, Supply, Layers, maintenance, and Drift>`
- **Alchemy:** `<Reagents, provenance, Recipe, stages, Process Windows, Batch, Yield, Byproducts, toxicity, and application>`
- **Divine Magic:** `<Source, Domain, Jurisdiction, Access, Mandate, Covenant, petition, response, withdrawal, and evidence>`
- **Forbidden Magic:** `<underlying mechanic, hazard evidence, protected interest, authority, jurisdiction, controlled practice, custody, enforcement, and review>`
- **Magic-World:** `<established magical input, affected world domains, causal branches, Counterforces, delays, and Pending Consequences>`

## Optional Fields

- **Local terminology:** `<culture-specific labels and translation limits>`
- **Institution and Magical School:** `<social interpretation, teaching, verification, and recognition>`
- **Soul interaction:** `<identity, Resonance, Echo, Title, Avatar, or Archive interface without ownership transfer>`
- **Monster interaction:** `<Species Trait, Mutation, Evolution, ecology, and embodiment references>`
- **Soul Weapon interaction:** `<distinct Weapon Soul, vessel, form, consent, and compatibility>`
- **Restriction or secrecy:** `<knowledge custody and visibility>`
- **Review Points:** `<conditions requiring reassessment>`

## Validation Notes

- [ ] A valid Mana Relation and exact specialist owner are identified.
- [ ] Mana is not XP, spell slots, generic MP, or a second health bar.
- [ ] Access, Reserve, Capacity, control, affinity, authority, Skill, Development, and recognition remain distinct.
- [ ] Present embodiment, current world laws, practice, costs, failure, and recovery remain relevant.
- [ ] The record grants no universal spell access, infinite growth, or magical superiority.
- [ ] Classes, Souls, Monster Evolution, Skills, and Soul Weapons retain ownership.
- [ ] Person-like sources and affected persons retain agency, consent, and resistance.
- [ ] World consequences are linked to their World Engine owners.

## Cross-References

- [Magic Claim Resolution](../docs/magic/README.md#claim-resolution-sequence)
- [Magical Development](../docs/progression/MAGICAL_DEVELOPMENT.md)
- [Skill Record Template](SKILL_TEMPLATE.md)
- [Human Framework Profile Template](HUMAN_FRAMEWORK_TEMPLATE.md)
- [World-State Record Template](WORLD_STATE_TEMPLATE.md)
