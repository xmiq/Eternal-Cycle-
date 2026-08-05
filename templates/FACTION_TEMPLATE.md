# Faction Profile Template

Use this storage-neutral template to record one scoped coordination among distinct actors without turning it into one mind, one morality, one alignment, one capability pool, or one guaranteed course of action.

The repository keeps this template blank. Every populated Faction Profile is external campaign state. Completing this form does not create participants, resources, authority, territory, hostility, capability, or an inevitable story role.

## Document Control

- **Template owner:** Factions and Institutions module of the [Campaign Persistence Engine](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md#factions-and-institutions)
- **Primary mechanical owner:** [Faction Behaviour](../docs/world-engine/FACTION_BEHAVIOUR.md)
- **Generation owner:** [Faction Generator](../docs/gm/FACTION_GENERATOR.md)
- **Dependencies:** Actors, Relationships, Populations, Institutions, Locations, Knowledge, resources, infrastructure, ecology, economy, law, and every specialist source named by a claim
- **Extensions:** Coalition scope, internal faction, faction change, conflict, institutional action, migration, Gate contact, Research, and other specialist world processes
- **Consumers:** world simulation, session preparation, NPC generation, encounters, diplomacy, conflict adjudication, Timeline, Campaign History, migration, and validation
- **Repository boundary:** no named live faction, participant, leader, objective, resource, relationship, decision, action, or current world value belongs in this blank file

This template organizes a current Faction Version. It does not own individual minds, personal capability, institutional mechanics, world resources, or specialist outcomes that it references.

## Usage Guidance

1. State the coordination question and scope before drawing the faction boundary.
2. Identify actual participants and action routes; a shared category or sentiment is not enough.
3. Separate membership, participation, representation, leadership, support, dependence, compliance, and affected status.
4. Preserve distributed knowledge. Never write "the faction knows" without a causal route to relevant decision and execution actors.
5. Separate interests, proposed objectives, accepted decisions, mobilization, attempted actions, and actual results.
6. Assess capacity and cohesion by function and issue rather than assigning a universal rating.
7. Version material changes instead of silently pretending name continuity preserves every structure.
8. Link every person, resource, capability, relationship, location, and world condition to its authoritative owner.
9. Validate the populated Profile after generation and after each material change.

## Required Field Policy

**Required** fields must be present or carry an explicit `Unknown`, `Not Yet Verified`, `Disputed`, or `Requires Source Recovery` state. **Conditional** fields become required only when their feature exists. **Optional** fields grant nothing when omitted.

## Record Contract

### Required Identity

- **Record ID:** `<stable Faction ID>`
- **Record type:** `Faction Profile`
- **Authoritative module:** `Factions and Institutions`
- **Current name:** `<primary current label or explicit unknown>`
- **Aliases and observer labels:** `<label, audience, interval, and equivalence>`
- **Faction Version:** `<current version identifier>`
- **Subject scope:** `<issue, place, population, relationship, and time horizon>`
- **Classification:** `<formal faction | informal network | internal faction | movement | Coalition | monster coordination | supernatural coordination | another supported form>`
- **Campaign version:** `<applicable version reference>`

### Required Authority and Provenance

- **Persistence authority:** `<Campaign Canon or other applicable authority>`
- **Truth layer:** `<Campaign Canon | Historical Record | Research | GM Secrets | another valid layer>`
- **Canonical mechanical owners:** `<Faction Behaviour and each specialist owner>`
- **Status:** `<forming | active | inactive | fragmented | dissolved | disputed | superseded | other established state>`
- **Effective interval:** `<Timeline references>`
- **Source basis:** `<generation, observed coordination, records, adjudication, migration, correction, or explicit unknown>`
- **Last integrated at:** `<Save Point or Transaction reference>`
- **Typed references and dependents:** `<relation type -> stable record ID>`
- **Validation status:** `<Validation Run, review result, warnings, or not yet validated>`

## Coordination Basis and Boundary

### Required Fields

- **Coordination question:** `<what shared action or issue makes this faction relevant>`
- **Scope:** `<where, when, among whom, and for which concerns the grouping acts as one coordination>`
- **Participants:** `<Actor, office, household, community, Institution, spirit, Weapon Soul, or other agentive-party references>`
- **Overlapping interests:** `<why particular participants have reason to coordinate>`
- **Action routes:** `<communication, deliberation, delegation, command, bargaining, contribution, or enforcement routes>`
- **Boundary conditions:** `<entry, exit, refusal, representation, secrecy, obligation, and outsider relationships>`
- **Continuity sources:** `<relationships, identity, procedures, records, assets, obligations, recurring interests, or successor recognition>`
- **Exclusions:** `<nearby Population, culture, Institution, state, movement, or audience not identical to this faction>`

### Conditional Fields

- **Parent Institution or larger body:** `<typed reference and scope>`
- **Internal factions:** `<Faction IDs and contested issues>`
- **Coalition participants:** `<distinct party IDs, objectives, contributions, decision routes, and exit conditions>`
- **Territorial or jurisdictional claims:** `<Location, law, Institution, audience, and enforcement references>`
- **Distributed or supernatural coordination:** `<actual senses, communication, Agency Routing, and owner-defined limits>`

A species, culture, Profession, Population, settlement, faith, state, or Institution is not automatically one faction. A faction may overlap any of them without absorbing their identity.

## Participation, Membership, and Representation

### Participant Relationship Record

Repeat for each material participant category or named actor.

- **Participant or category ID:** `<stable reference>`
- **Relationship to faction:** `<member | participant | leader | officeholder | representative | supporter | dependent | coerced actor | client | patron | affected nonparticipant | audience | other exact relation>`
- **Source and scope:** `<appointment, election, oath, kinship, employment, dependence, coercion, acclaim, custom, or other basis>`
- **Rights and access:** `<decision, information, resources, protection, voice, or other bounded access>`
- **Duties and expectations:** `<current obligations and whose expectations they are>`
- **Entry and exit:** `<routes, barriers, consequences, and current status>`
- **Current consent and willingness:** `<issue-specific state; never inferred from membership>`
- **Knowledge and secrecy:** `<what reaches this participant through which route>`

### Representation Record

- **Representative ID:** `<Actor or office reference>`
- **Source of mandate:** `<election, appointment, delegation, custom, acclaim, expertise, control, coercion, or other basis>`
- **Recognizing audience:** `<who accepts or disputes the mandate>`
- **Issue and jurisdiction:** `<exact scope>`
- **Instructions and discretion:** `<what may be decided or communicated>`
- **Information access:** `<records, reports, restrictions, and uncertainty>`
- **Resources and support:** `<what can actually be mobilized>`
- **Accountability and recall:** `<procedures and practical limits>`
- **Dissent and contested legitimacy:** `<actors, claims, and consequences>`

No leader, founder, majority, title-holder, representative, or official statement automatically speaks for every participant.

## Interests, Commitments, and Objectives

### Required Fields

- **Stated purposes:** `<public or formal claims and source>`
- **Active interests:** `<material, social, ecological, political, religious, magical, professional, existential, symbolic, or relational concerns>`
- **Protected commitments:** `<values, persons, relationships, assets, procedures, or limits some participants seek to preserve>`
- **Internal differences:** `<conflicting motives, priorities, costs, and time horizons>`
- **Current priorities:** `<issue-specific ordering and who supports it>`

### Faction Objective Record

Repeat for each material current objective.

- **Objective ID:** `<stable Project, Faction, or activity reference>`
- **Intended result:** `<specific outcome rather than a slogan>`
- **Scope and time horizon:** `<place, subjects, conditions, and interval>`
- **Sponsors and supporters:** `<participants and reasons>`
- **Authorization status:** `<proposed | accepted | contested | rejected | deferred | superseded>`
- **Opposition and vetoes:** `<participants, external actors, and routes>`
- **Implementers:** `<particular actors, offices, or partners>`
- **Dependencies and resources:** `<typed owner references>`
- **Acceptable costs and protected limits:** `<whose judgment and current boundaries>`
- **Risks and stopping conditions:** `<causal triggers>`
- **Success, failure, and revision evidence:** `<observable criteria and uncertainty>`

An objective is not an action. Public doctrine, leaders' preferences, operating priorities, and members' lived interests may all differ.

## Faction Information Position

Complete by material issue. Do not merge observer-specific knowledge into one mind.

- **Issue or claim:** `<bounded subject>`
- **World truth reference:** `<authoritative record or protected GM Secret>`
- **Observers and holders:** `<who observed, knows, suspects, misunderstands, or lacks access>`
- **Sources and records:** `<observation, report, Archive, witness, rumour, research, magical route, or other source>`
- **Communication route:** `<how information moves, with delay and access conditions>`
- **Interpretations and assumptions:** `<participant-specific meaning>`
- **Secrecy and access controls:** `<concealment, compartmentalization, incentives, enforcement, and copies>`
- **Distortion and uncertainty:** `<filters, errors, outdated claims, propaganda, and confidence>`
- **Decision access:** `<who can place or use the information on a decision route>`
- **Execution access:** `<who can act on it>`
- **Leak and discovery routes:** `<traces, witnesses, failures, and consequences>`

Player knowledge, GM world truth, public claims, and one participant's private knowledge remain independent.

## Decision Routes and Authority

### Required Fields

- **Decision routes:** `<formal procedure, office, consensus, vote, command, bargaining, custom, ritual, convention, patronage, threat, emergency practice, or spontaneous convergence>`
- **Agenda access:** `<who can raise an issue and how>`
- **Information requirements:** `<what normally reaches whom before a decision>`
- **Formal authority:** `<office, source, jurisdiction, and limits>`
- **Practical influence:** `<relationships, resources, expertise, coercion, reputation, or control>`
- **Vetoes and blockers:** `<formal and practical routes>`
- **Decision delay:** `<communication, procedure, embodiment, geography, trust, or resource causes>`
- **Contest and appeal:** `<dissent, review, resistance, noncompliance, or correction routes>`
- **Emergency variation:** `<temporary route, trigger, scope, and expiry>`

### Faction Action Claim Record

- **Claim:** `<what the faction allegedly knew, decided, promised, prohibited, supported, opposed, provided, or did>`
- **Relevant Faction Version and scope:** `<exact reference>`
- **Participants and representatives:** `<who was involved>`
- **Information position:** `<what relevant actors possessed>`
- **Authority and procedure:** `<how a faction-level claim arose>`
- **Support, dissent, and compliance:** `<who accepted, contested, ignored, or undermined it>`
- **Mobilization and execution:** `<actual people, means, timing, and attempt>`
- **Result and evidence:** `<Immediate Outcome, uncertainty, and Historical reference>`

Formal validity, legitimacy, factual truth, practical enforcement, expertise, and member support remain separate.

## Function-Specific Capacity

Create one capacity entry per function; never one faction power score.

- **Function:** `<negotiate, investigate, feed, transmit, patrol, enforce, conceal, heal, construct, research, fight, recover, or other bounded task>`
- **Contributing actors:** `<current Skills, Development, embodiment, health, access, willingness, and roles>`
- **Procedures and coordination:** `<Institutional or faction routes>`
- **Resources and infrastructure:** `<Inventory, Economy, Location, Infrastructure, Magic, Soul Weapon, or other owner references>`
- **Information and communication:** `<quality, delay, trust, security, and access>`
- **Permissions and legitimacy:** `<law, relationship, jurisdiction, consent, and audience>`
- **Environment and timing:** `<geography, ecology, season, pressure, and competing obligations>`
- **Dependencies and bottlenecks:** `<critical actors, routes, materials, maintenance, or replacement>`
- **Opposition and counterplay:** `<actors, constraints, vulnerabilities, and uncertainty>`
- **Current mobilization readiness:** `<what can actually be assembled now and why>`

Collective capability belongs to the coordination. It is not copied into each member, leader, founder, successor, or reincarnated Soul.

## Faction Cohesion and Dissent

Complete by issue and pressure.

- **Issue:** `<bounded objective, commitment, or crisis>`
- **Participants whose coordination matters:** `<references>`
- **Current willingness and ability to coordinate:** `<qualitative evidence>`
- **Supporting factors:** `<shared interest, trust, procedure, interdependence, legitimacy, habit, success, danger, coercion, or other cause>`
- **Straining factors:** `<unequal costs, secrecy, betrayal, obligations, succession, capture, shortage, communication failure, alternatives, or other cause>`
- **Dissent and alternatives:** `<positions, actors, procedures, and possible contributions>`
- **Compliance conditions:** `<voluntary, negotiated, coerced, partial, performative, or unknown>`
- **Failure and split risks:** `<delay, refusal, sabotage, exit, fragmentation, capture, or schism routes>`
- **Review Points:** `<changes requiring reassessment>`

Cohesion is not affection, obedience, loyalty, morality, unanimity, or a universal meter. Dissent is not automatic dysfunction.

## Relationships, Audiences, and Recognition

### Faction Relationship Record

- **Other party:** `<Faction, Institution, Population, Actor, spirit, god, Weapon Soul, or other party ID>`
- **Issue and scope:** `<where the relationship applies>`
- **Current form:** `<alliance, rivalry, trade, patronage, dependence, deterrence, infiltration, shared membership, coexistence, competition, opposition, or other exact relation>`
- **Promises, obligations, debts, betrayals, and unresolved issues:** `<Relationship and Timeline references>`
- **Contact and communication routes:** `<representatives, channels, delay, and reliability>`
- **Trust and expectations:** `<party-specific evidence and trajectory>`
- **Power and dependence:** `<function-specific sources, asymmetries, and alternatives>`
- **Current consent and boundaries:** `<where applicable>`
- **Change triggers:** `<events or conditions requiring review>`

### Audience Position

- **Audience:** `<members, subjects, rivals, dependents, Institution, culture, deity, spirit, monster, Soul Weapon, or affected Population>`
- **Public Reputation:** `<observer-specific recognition and evidence>`
- **Formal Authority:** `<source, jurisdiction, recognition, and enforcement>`
- **Cultural Legitimacy:** `<audience, meanings, disputes, and change>`
- **Symbolic Authority:** `<Title, history, office, ritual, or other source and limits>`
- **Trust, fear, and coercion:** `<separate current relationships>`
- **Expertise and credibility:** `<scope and supporting evidence>`

Recognition influences reactions and opportunities. It does not prove morality, truth, consent, competence, or universal authority.

## Mobilization, Action, and Consequence

Use the Faction Behaviour sequence for each material attempt.

- **Perceive:** `<who encountered which condition>`
- **Interpret:** `<meaning assigned under what information>`
- **Raise:** `<how the issue reached an agenda or action route>`
- **Decide:** `<who selected, rejected, deferred, or revised a response>`
- **Mobilize:** `<people, resources, permissions, support, logistics, and instructions actually assembled>`
- **Execute:** `<particular actors and actions attempted>`
- **Immediate Outcome:** `<specialist adjudication and uncertainty>`
- **Observe:** `<which evidence reached which participants>`
- **Adapt:** `<changed interpretation, objective, procedure, relationship, or Review Point>`
- **Direct state changes:** `<typed owner references>`
- **Pending Consequences:** `<downstream pressures and owners>`
- **Historical event:** `<Timeline and Campaign History references>`

Announcements, decisions, laws, promises, commands, and plans produce no world effect without an execution route. Record actual effects rather than intended outcomes.

## Continuity and Versioning

### Required Fields

- **Current Faction Version:** `<stable version ID>`
- **Continuity basis:** `<participants, interests, relationships, identity, records, procedures, assets, commitments, and successor recognition that survived>`
- **Formation history:** `<Historical references and uncertainty>`
- **Predecessors and successors:** `<typed IDs and continuity claims>`
- **Material changes since prior version:** `<boundary, leadership, interests, information, decisions, capacity, resources, identity, or relationships>`
- **Inherited obligations and consequences:** `<what persists and why>`
- **Lost or inaccessible structures:** `<what did not continue>`
- **Identity disputes:** `<claimants, evidence, audiences, and consequences>`

### Conditional Change Classification

- **Succession:** `<changed participants and surviving routes>`
- **Reform:** `<changed objectives or procedures and retained continuity>`
- **Capture:** `<narrow interest, controlled routes, resources, and outer continuity>`
- **Schism:** `<successor factions, divided assets, claims, relationships, and unresolved identity>`
- **Merger:** `<combined structures, surviving identities, dissent, and obligations>`
- **Fragmentation:** `<failed central routes and local coordination>`
- **Dissolution:** `<ended coordination and surviving records, debts, assets, members, and consequences>`
- **Revival:** `<new actors and reconstructed routes rather than automatic restoration>`

A name, emblem, legal charter, founder, prophecy, Soul Title, or public claim alone does not preserve faction continuity. Reincarnation does not restore office, membership, secrets, followers, property, or command as Soul property.

## Generation Provenance

### Conditional Fields

- **Generation scope:** `<classify, create, internal, Coalition, or revalidate>`
- **Preparation resolution:** `<abstraction level and relevant horizon>`
- **Established world inputs:** `<World, Population, Institution, Location, Resource, Relationship, and specialist references>`
- **Possible participants considered:** `<sources and exclusions>`
- **Factual View and Observer Views:** `<separated inputs>`
- **Randomly selected ordinary details:** `<committed field, supported options, causal weighting, and result>`
- **Hidden, disputed, outdated, and unresolved claims:** `<protected references>`
- **Corrections and Review Points:** `<known follow-up>`

Generation provenance explains how the Profile was assembled. It is not a second Profile and cannot script future choices, conflict, betrayal, reform, collapse, or success.

## Validation Checklist

- [ ] The repository copy remains blank; every populated Faction Profile is external.
- [ ] The coordination has a bounded scope, actual participants, overlapping interests, action routes, boundary conditions, and continuity.
- [ ] Population, culture, Institution, Profession, species, settlement, faith, state, and faction identities remain distinct.
- [ ] Membership, participation, leadership, representation, support, dependence, coercion, and affected status remain separate.
- [ ] Individual agency, knowledge, capability, Souls, Skills, Development, Classes, Species Traits, Soul Weapons, and Magic are not pooled.
- [ ] Interests, objectives, decisions, mobilization, execution, results, and consequences remain separate.
- [ ] Information is distributed through causal routes; no faction mind gains GM or player knowledge.
- [ ] Authority, influence, legitimacy, expertise, consent, compliance, and enforcement remain separate.
- [ ] Capacity and cohesion are function- and issue-specific, qualitative, sourced, and open to failure.
- [ ] Resources and member capability are accessible only through actual relationships, logistics, willingness, timing, and procedures.
- [ ] Relationships and audiences are issue-specific and do not predetermine alliance, hostility, betrayal, or loyalty.
- [ ] Faction Versions preserve traceable continuity without treating a name as sufficient.
- [ ] Reincarnated participants receive no automatic world-bound membership, office, property, secrets, or command.
- [ ] War, disease, advancement, dungeons, Stability, Gates, resets, and other specialist outcomes remain with their owners.
- [ ] No universal alignment, level, power score, cohesion meter, threat tier, guaranteed action, or scripted future appears.
- [ ] Truth Layers, provenance, versions, typed references, uncertainty, Historical events, and Pending Consequences are explicit.

## Canonical Dependencies

- [Faction Behaviour](../docs/world-engine/FACTION_BEHAVIOUR.md)
- [Faction Generator](../docs/gm/FACTION_GENERATOR.md)
- [World Engine](../docs/world-engine/README.md)
- [Institutions and Academies](../docs/human/INSTITUTIONS_AND_ACADEMIES.md)
- [Monster Societies](../docs/monster-evolution/MONSTER_SOCIETIES.md)
- [Social and Leadership Development](../docs/progression/SOCIAL_AND_LEADERSHIP_DEVELOPMENT.md)
- [Relationship Memory Engine](../docs/persistence/RELATIONSHIP_MEMORY_ENGINE.md)
- [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [Timeline Engine](../docs/persistence/TIMELINE_ENGINE.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)

## Extensions and Consumers

Use [NPC Generation](../docs/gm/NPC_GENERATOR.md) and the [Character Record Template](CHARACTER_TEMPLATE.md) for individual participants. Settlement, Dungeon, Location, Population, Relationship, Research, Project, Timeline, and world-state records retain their own facts and link this Profile by stable ID.

Faction simulation, encounter preparation, conflict, diplomacy, migration, continuity review, and validation consume only the current Faction Version and the Truth Layers available to their role. Consumers must never substitute a Profile summary for actual participants, information routes, decisions, or execution.
