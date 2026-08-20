# Character Record Template

Use this storage-neutral template to represent one current character without flattening identity, embodiment, capability, knowledge, relationships, or Soul continuity into one stat block. Populate it only in an external Campaign Record.

The template is a logical contract. A campaign may divide it among files, database records, paper sheets, or other storage as long as stable identities, authority, ownership, provenance, typed references, and validation remain intact.

## Document Control

- **Template owner:** [Campaign Persistence Engine](../docs/persistence/README.md)
- **Primary record modules:** one persistent Entity identity anchor exposed through Player State for a player-controlled character or Actors/Companions for another character; Souls and Incarnations for Soul continuity
- **Canonical mechanical owners:** the specialist systems linked under [Canonical Dependencies](#canonical-dependencies)
- **Extensions:** Species, Skills, Human frameworks, Soul systems, Soul Weapons, Magic, Relationships, Inventory, Research, Projects, Timeline, and World records
- **Consumers:** session preparation, adjudication, continuity review, Save Updates, migrations, validation, and authorized derived views
- **Repository boundary:** this blank contract belongs in the repository; every populated instance remains campaign-external

This template owns no mechanic. It indexes established claims and routes each one to its actual owner.

It is a composite record view, not permission to copy mutable domain state. Attributes, Skills, placement, Relationships, Inventory, conditions, and other independently changing facts remain authoritative under their owners in [Canonical Data Ownership](../docs/persistence/CANONICAL_DATA_OWNERSHIP.md).

## Usage Guidance

1. Create the populated record outside this repository and assign stable IDs before play depends on it.
2. Complete every required field. When a required fact is unavailable, record an explicit uncertainty state rather than inventing a value.
3. Keep Soul, Incarnation, actor, body, player-control, and display identities separate.
4. Reference authoritative specialist records instead of copying their full contents into this view.
5. Record current condition separately from persistent potential, ordinary capability, reputation, and temporary support.
6. Treat optional fields as absent unless the campaign establishes a relevant source. Blank optional sections grant nothing.
7. After a completed Gameplay Interaction, update only the Affected Set through the [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md).

## Required Field Policy

**Required** means the logical information must exist or carry an explicit `Unknown` or `Requires Source Recovery` state. **Conditional** means the field is required when its stated condition applies. **Optional** means it may be omitted without implying a negative fact.

Every claim that can change independently should retain its own source, effective scope, uncertainty, and update history even when a storage implementation displays several claims together.

## Common Record Contract

### Required Identity

- **Record ID:** `<stable unique record ID>`
- **Record type:** `Character`
- **Authoritative module:** `<Player State | Actors | another established module>`
- **Subject ID:** `<stable actor ID>`
- **Record version:** `<record revision>`
- **Campaign version:** `<active or candidate Campaign Version>`

### Required Authority and Ownership

- **Persistence authority:** `<Campaign Canon | Historical Record | Current Campaign State | Current Session>`
- **Truth layer:** `<Campaign Canon | Historical Record | Character Knowledge | Research | another valid layer>`
- **Canonical owner:** `<owner of the represented claim or Character index>`
- **Record status:** `<active | pending | superseded | archived | disputed | unknown | owner-defined state>`
- **Effective scope:** `<subject, time, place, audience, and conditions>`
- **Persistence level:** `<Soul | Historical | Campaign | Session | Ephemeral, as established per claim>`

### Required Provenance and References

- **Source:** `<event, adjudication, decision, observation, migration, correction, or other established origin>`
- **Established at:** `<Timeline Event ID or explicit unknown>`
- **Last integrated at:** `<Transaction ID or Save Point>`
- **Supersession:** `<prior, replacement, correction, or none>`
- **Uncertainty:** `<supported uncertainty classification>`
- **Typed outgoing references:** `<relation type -> record ID>`
- **Dependent index:** `<route for discovering incoming references>`
- **Canonical dependencies:** `<rule links or repository revision>`
- **Validation status:** `<Validation Run, result, warnings, or not yet validated>`

## Control and Agency

### Required

- **Character agency owner:** `<the character>`
- **Control mode:** `<player-controlled | GM-controlled | shared under an explicit procedure | other established mode>`
- **Current decision boundary:** `<what the controller may decide now>`
- **Delegated authority:** `<scope, source, limits, revocation, or none>`

### Conditional or Optional

- **Player-control identity reference:** `<Meta-layer reference; required for a player-controlled character>`
- **Standing Instructions:** `<instruction IDs, scope, expiry, interruption triggers, or none>`
- **Consent constraints:** `<relevant active boundaries and their source records>`
- **Unresolved agency conflict:** `<Continuity Case or adjudication reference>`

Do not store a player's private plan as Character Knowledge, world truth, or an in-world intention unless play establishes it through an authorized act.

## Current Identity

### Required

- **Actor ID:** `<stable identity across names and roles>`
- **Incarnation ID:** `<current life identity>`
- **Body ID:** `<current embodied subject>`
- **Primary current name:** `<name or explicit unknown>`
- **Identity status:** `<active | missing | dead pending confirmation | Final Death confirmed | other established state>`

### Conditional or Optional

- **Soul ID:** `<required only when a continuing Soul is established>`
- **Prior Incarnation references:** `<ordered IDs; never reuse a former body ID>`
- **Aliases and names:** `<name, observer or institution, scope, validity, and source>`
- **Pronouns or self-description:** `<current expression when material>`
- **Public identity references:** `<reputation, office, title, disguise, or mistaken-identity records>`
- **Identity disputes:** `<claims, evidence, authority, and Continuity Case references>`

## Current Embodiment and Condition

### Required

- **Species record:** `<Species Record ID>`
- **Current form or stage:** `<form reference and owning route>`
- **Carried Lineages:** `<Lineage Template references, campaign divergences, or unknown>`
- **Expressed Lineages:** `<currently expressed template references and scope>`
- **Species Expression:** `<current species-level architecture and provenance>`
- **Evolution Expression:** `<current Evolved Form architecture or none>`
- **Evolutionary Potential:** `<available branch references without implying readiness>`
- **Inherited Level 0 Instincts:** `<Skill references and profile provenance or none>`
- **Ancestral Echo manifestations:** `<campaign facts and source definition references or none>`
- **Life-cycle position:** `<supported description or unknown>`
- **Body Compatibility summary:** `<scope-specific finding or linked profile>`
- **Current condition:** `<stable condition reference plus active injuries, fatigue, impairment, recovery, or suppression>`
- **Senses and communication routes:** `<current embodied access references>`
- **Movement and manipulation routes:** `<current embodied access references>`
- **Essential needs and dependencies:** `<sustenance, environment, maintenance, symbiosis, support, or other established needs>`

### Conditional or Optional

- **Species Traits:** `<trait references and present expression>`
- **Mutations or Hybrid Form:** `<owner records, source, stability, and present expression>`
- **Evolution Route state:** `<route reference, current readiness evidence, commitments, or none>`
- **Temporary transformation or external body support:** `<source, duration, interruption, maintenance, and expiry>`
- **Medical, magical, or Soul-related constraints:** `<specialist record references>`
- **Visual Identity record:** `<sparse body- or Incarnation-scoped identity reference, or none established>`

Current anatomy and condition never prove a learned Skill, retained mastery, social authority, or future Evolution.
Visual Identity records only established identity-bearing appearance traits. Current equipment, injuries, environmental effects, and Species defaults remain with their specialist owners and are assembled into Current Appearance when needed.

## Position and Immediate Context

### Required

- **Current Location ID:** `<stable reference from the authoritative placement relation, or explicit unknown; this view is non-authoritative>`
- **Position precision:** `<exact | bounded | approximate | relative | disputed | unknown>`
- **Current chronological reference:** `<Timeline Event, interval, or Save Point>`
- **Immediate access constraints:** `<routes, permissions, barriers, custody, environment, or none>`

### Conditional or Optional

- **Current companions:** `<Companion or Actor references>`
- **Active encounter or scene:** `<Encounter or Session reference>`
- **Current environmental pressures:** `<World-State or Pending Consequence references>`

## Capability Profile References

Do not assign a universal character level, power score, or undifferentiated XP total. For each material objective, use a scoped [Capability Profile](../docs/progression/CAPABILITY_ASSESSMENT.md) or specialist record.

### Required Capability Separation

For every capability claim used in adjudication, preserve:

- **Capability or objective:** `<bounded claim>`
- **Canonical owner:** `<Development Track, Skill, Species, Magic, Soul Weapon, external support, or another owner>`
- **Persistent Potential:** `<source record or none established>`
- **Current Access:** `<available, partial, blocked, unknown, and cause>`
- **Embodied Expression:** `<what the present body and method permit>`
- **Practised Reliability:** `<evidence under relevant conditions>`
- **Contextual Effectiveness:** `<objective, environment, opposition, tools, allies, and limitations>`
- **World Recognition:** `<observer-specific recognition, authority, reputation, or uncertainty>`
- **Evidence state:** `<known | hidden | inferred | disputed | unknown>`

### Development Track References

- **Physical Development:** `<profile IDs>`
- **Skill Development:** `<profile IDs>`
- **Profession Development:** `<profile IDs>`
- **Magical Development:** `<profile IDs>`
- **Social and Leadership Development:** `<profile IDs>`
- **Species Development:** `<profile IDs>`
- **Stat XP and Retained Development:** `<domain-specific records kept separate from current statistics>`

### Conditional Support

- **Tools and equipment:** `<Inventory or custody references>`
- **Allies and group capability:** `<Actor, Relationship, Faction, or Project references>`
- **Infrastructure:** `<Location, Institution, Dungeon, or Project references>`
- **Temporary enhancement:** `<source, scope, duration, interruption, and maintenance>`

Record every gain once under its owner. Support may change access or effectiveness without becoming personal Development.

## Skills and Learned Practice

### Conditional Fields

- **Skill records:** `<Skill IDs and current Capability Representation>`
- **Skill-tree origin:** `<human | monster | shared | another established route>`
- **Current receiving route:** `<body, cognition, culture, tool, magical, or other route>`
- **Skill Imprint or retained familiarity:** `<source and access state>`
- **Active, passive, mixed, hidden, or conceptual expression:** `<specialist record references>`
- **Evolution, Fusion, or Adaptive Skill state:** `<owner record, evidence, and cleanup status>`
- **Maintenance, rust, suppression, or inaccessible mastery:** `<current causes and Review Points>`

A name in this section does not prove present expression or reliability. Use the [Skill Record Template](SKILL_TEMPLATE.md) for each authoritative campaign Skill record.

## Human, Monster, and Social Frameworks

### Conditional Fields

- **Human Classes:** `<Class Profile references, versions, and current relationships>`
- **Professions:** `<Profession Profile references, roles, standards, and accountability>`
- **Martial Traditions or Magical Schools:** `<framework and participation references>`
- **Institutions, academies, offices, credentials, or ranks:** `<current relationship and recognition records>`
- **Monster-native traditions or societies:** `<culture, learning, role, and institution references>`
- **Evolution branches or species stages:** `<Monster Evolution owner references>`

These fields record social and biological routes separately. A label grants no capability package, and Reincarnation restores no world-bound office, credential, membership, reputation, or rank.

## Magic

### Conditional Fields

- **Mana Relation:** `<source and current relationship>`
- **Affinity Profiles:** `<target-specific references>`
- **Spell Patterns and current access:** `<Spell Profile references>`
- **Ritual roles or prepared structures:** `<Ritual references>`
- **Enchantments, Alchemy, Divine, or Forbidden Magic relationships:** `<specialist records>`
- **Reserve, Capacity, Channels, control, costs, recovery, and constraints:** `<owner-specific current records>`

Never use this section as generic MP, spell slots, caster level, universal access, or proof that magical options are superior.

## Soul Continuity

### Conditional Fields

- **Soul record:** `<Soul ID and current Integrity reference>`
- **Reincarnation state:** `<Incarnation order, mode, transition, or none>`
- **Soul Depth:** `<current qualitative record>`
- **Soul Resonance links:** `<target-specific references and access>`
- **Soul Echoes:** `<Echo IDs, Presence, access, and consent>`
- **Retained Instincts:** `<Instinct IDs, cues, access, embodiment translation, and agency>`
- **Soul Titles:** `<Title IDs, expression state, reach, and observer-specific recognition>`
- **Soul Space:** `<access, Anchor, Regions, Thresholds, and current constraints>`
- **Soul Constellations:** `<participant and pattern references>`
- **Soul Avatars:** `<Soul Avatar Profile references and current expression access>`; use the [Soul Avatar Profile Template](SOUL_AVATAR_TEMPLATE.md)
- **Akashic Archive:** `<Archive relationship, attunement, records, and knowledge provenance>`
- **Soul harm, suppression, or recovery:** `<owner records and Review Points>`

Soul persistence never implies current access, bodily expression, Skill mastery, inventory passage, or social standing.

## Soul Weapons and Other Persons

### Conditional Fields

- **Weapon Soul IDs:** `<distinct person references>`
- **Vessel IDs and current custody:** `<Inventory or world records>`
- **Bond status:** `<active | Suspended | Severed | none, with Accord source>`
- **Passage Accord:** `<scope, consent, standing state, and last valid update>`
- **Compatibility Profiles:** `<purpose-specific references>`
- **Weapon Echoes, Evolution, or Manifestation:** `<specialist owner references>`
- **Communication, trust, refusal, and unresolved issues:** `<Relationship records>`

Use the [Soul Weapon Record Template](SOUL_WEAPON_TEMPLATE.md) for each linked partnership. Never list a Weapon Soul as owned equipment or copy its capabilities into the character's personal Development.

## Relationships, Recognition, and Obligations

### Conditional Fields

- **Relationship record references:** `<participant IDs and directional relation>`
- **Companion status:** `<Companion view reference>`
- **Family or kinship:** `<Relationship references>`
- **Faction and institution relationships:** `<membership, office, obligation, recognition, hostility, or other scoped claims>`
- **Promises, debts, gifts, betrayals, dependencies, and unresolved issues:** `<source-preserving record references>`
- **Reputations and social titles:** `<observer, scope, basis, trajectory, and current relevance>`

Known characters do not become strangers without an established in-world cause or an explicit continuity correction.

## Inventory, Custody, and Resources

### Conditional Fields

- **Inventory index:** `<owned, carried, stored, loaned, entrusted, disputed, lost, destroyed, or unknown item references>`
- **Custody and access:** `<holder, location, permissions, restrictions, and effective time>`
- **Currency or measured resources:** `<owner record and Numerical Change Trace>`
- **Property, infrastructure, or legal claims:** `<world-bound owner records>`
- **Consumables and charges:** `<current quantity only when mechanically established>`

Final Death does not move ordinary Inventory with the Soul. Never invent a number, restore a lost item from a summary, or treat possession as legal ownership.

## Knowledge, Research, and Secrets

### Conditional Fields

- **Character Knowledge:** `<claim, source, acquisition route, confidence, time, and observer>`
- **Research participation:** `<Research IDs, role, evidence access, and current theory>`
- **Known Mysteries:** `<Mystery IDs and legitimately available clues>`
- **Rumours or mistaken beliefs:** `<separate truth-layer references>`
- **Private or protected information:** `<visibility boundary without leaking GM Secrets>`

World truth, Character Knowledge, player knowledge, Research, Rumours, Player Theories, and GM Secrets remain independent.

## Goals, Projects, and Pending Consequences

### Conditional or Optional Fields

- **Expressed current goals:** `<established in-world or player-authorized goal references>`
- **Projects:** `<Project IDs, role, current established state, dependencies, and Review Points>`
- **Mysteries under active inquiry:** `<Mystery or Research references>`
- **Pending Consequences:** `<source, route, horizon, trigger, capable actors, uncertainty, and Review Point>`
- **Open choices:** `<decision boundary without a scripted outcome>`

A goal does not schedule success. A Pending Consequence is a pressure, not a predetermined event.

## Current Effects and Constraints

### Conditional Fields

- **Active effect:** `<effect ID, owner, source, target, scope, start, duration or termination, cost, maintenance, interruption, and current evidence>`
- **Injury or impairment:** `<condition record and recovery route>`
- **Suppression or inaccessible capability:** `<claim, cause, affected layer, and Review Point>`
- **Legal, social, environmental, or material constraint:** `<world record reference>`
- **Unknown or disputed condition:** `<status and source-recovery route>`

Temporary support, condition, and permanent Development remain separate even when they influence the same action.

## Update Notes

After each completed Gameplay Interaction:

1. identify the character claims in the Affected Set;
2. route each change to its Authoritative Record Owner;
3. preserve prior values, sources, and effective times;
4. update typed references rather than duplicating specialist records;
5. append Session, Timeline, and Campaign History records only where their own rules require;
6. validate the candidate before activation;
7. regenerate this view from the activated state if it is derived.

Do not rewrite unrelated character sections merely to make the record appear fresh.

## Validation Checklist

- [ ] The populated record is outside the canonical repository.
- [ ] Record, actor, Incarnation, body, Soul, player-control, and display identities are not silently collapsed.
- [ ] Every required field is present or carries an explicit supported unknown state.
- [ ] Every typed reference resolves within the declared validation scope or is explicitly pending, disputed, or requires source recovery.
- [ ] Every mechanical claim names its canonical owner and every record names one Authoritative Record Owner.
- [ ] Persistent Potential, Current Access, Embodied Expression, Practised Reliability, Contextual Effectiveness, and World Recognition remain distinct.
- [ ] Current embodiment and current-life effort remain necessary for present capability.
- [ ] No universal level, power score, Development currency, generic MP, or automatic capability package appears.
- [ ] Human and monster routes remain distinct unless a valid bounded crossover is recorded.
- [ ] Soul persistence does not restore ordinary Inventory, former anatomy, institutional standing, or current mastery.
- [ ] Weapon Souls and other persons retain distinct identity, consent, capability, and Relationships.
- [ ] Character Knowledge, player knowledge, world truth, Research, Rumours, Player Theories, GM Secrets, and Meta remain separated.
- [ ] Every material number has a mechanically justified source and Numerical Change Trace.
- [ ] Relationships, recognition, promises, debts, and known identities have not silently reset.
- [ ] Current location, chronology, condition, custody, and Pending Consequences agree with their authoritative records.
- [ ] Visual Identity is sparse, body-appropriate, and does not duplicate Species, equipment, condition, or Location state.
- [ ] No lower-authority narration silently overwrites Campaign State or history.
- [ ] The candidate passes the required [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md) profile before activation.

## Canonical Dependencies

- [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [Persistence Authority](../docs/persistence/PERSISTENCE_AUTHORITY.md)
- [Truth Layers](../docs/persistence/TRUTH_LAYERS.md)
- [Persistence Levels](../docs/persistence/PERSISTENCE_LEVELS.md)
- [Relationship Memory Engine](../docs/persistence/RELATIONSHIP_MEMORY_ENGINE.md)
- [Research Engine](../docs/persistence/RESEARCH_ENGINE.md)
- [Timeline Engine](../docs/persistence/TIMELINE_ENGINE.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
- [Campaign Persistence Integration](../docs/persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md)
- [Visual Identity](../docs/persistence/VISUAL_IDENTITY.md)
- [Development System](../docs/progression/README.md)
- [Skill Engine](../docs/skills/README.md)
- [Lineage and Evolutionary Inheritance](../docs/gm-living-codex/LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md)
- [Soul Engine](../docs/soul/README.md)
- [Monster Evolution](../docs/monster-evolution/README.md)
- [Human Classes and Professions](../docs/human/README.md)
- [Soul Weapons](../docs/soul-weapons/README.md)
- [Magic](../docs/magic/README.md)
- [World Engine](../docs/world-engine/README.md)
- [Game Master Rules](../docs/gm/README.md)

## Extensions and Consumers

Specialist templates may extend this contract by defining a referenced record in more detail. They must not duplicate the authoritative value here or turn an optional subsystem into a mandatory character feature.

The [Game Master Framework](../docs/gm/GAME_MASTER_FRAMEWORK.md), future implementation-neutral AI procedures, Save Updates, migrations, continuity review, and validation may consume this record. Consumers receive only the truth layers and visibility their role permits.
