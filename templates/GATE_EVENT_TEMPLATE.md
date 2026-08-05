# World-Contact and Gate Event Record Template

Use this storage-neutral template to record one World-Contact Event and any World Gates that participate in it without turning contact into a scripted plot, treating every portal as a World Gate, or making one interface own all resulting world change.

The repository keeps this template blank. A populated World-Contact Record belongs in an external Campaign Record. Completing this form does not create contact, establish a Gate, open a route, authorize transit, prove compatibility, or make any consequence canonical.

## Document Control

- **Template owner:** World records in the [Campaign Persistence Engine](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary mechanical owner:** [World Gates and World-Contact Events](../docs/world-engine/GATES_AND_WORLD_CONTACT.md)
- **Dependencies:** Contact Domains, prior world causality, Locations, Timeline, information views, actors, Factions, channels, and every specialist system named by a consequence
- **Extensions:** World Gate Profiles, compatibility findings, Contact Phases, Cross-Domain Reincarnation Routes, Avatar Triggers, Dungeon overlap, Gate Closure, Gate Stranding, and Gate Legacies
- **Consumers:** world simulation, session preparation, travel and exposure adjudication, Factions, Research, Timeline, Campaign History, Save Updates, migration, continuity resolution, and validation
- **Repository boundary:** no current Gate, endpoint, domain, crossing, actor, discovery, compatibility result, faction response, closure, legacy, or other live world state belongs in this blank file

This template organizes a record family. Each World-Contact Event, World Gate, Contact Domain, endpoint, channel, actor, faction, route, consequence, and specialist record retains a stable identity and its actual owner.

## Usage Guidance

1. Establish the contact claim and prior world truth before selecting a dramatic interpretation.
2. Classify the route by function; do not infer a World Gate from appearance or local vocabulary.
3. Record one stable World-Contact Event ID and separate stable IDs for every participating World Gate and endpoint.
4. Describe channels, directionality, throughput, requirements, costs, and failure independently.
5. Test compatibility only for the subject, direction, purpose, and conditions currently in question.
6. Keep discovery, reach, use, operation, control, claim, authority, and consent separate.
7. Hand every downstream effect to its specialist owner and link the result rather than duplicating it here.
8. Preserve world truth, observations, beliefs, Research, rumours, secrets, and player knowledge through their proper Truth Layers.
9. Update only the Affected Set after contact changes; do not regenerate unrelated world state.
10. Validate populated campaign records before use and reusable proposals through repository governance.

## Required Field Policy

**Required** fields must be present or carry an explicit `Unknown`, `Not Yet Verified`, `Estimated`, `Disputed`, or `Requires Source Recovery` state. **Conditional** fields become required only when their feature exists. **Optional** fields grant nothing when omitted.

## Record Contract

### Required Identity

- **World-Contact Record ID:** `<stable record-family ID>`
- **World-Contact Event ID:** `<stable event ID>`
- **Record type:** `World-Contact and Gate Event Record`
- **Record mode:** `<campaign-local | reusable canonical proposal>`
- **Authoritative module:** `World`
- **Event label:** `<current label without treating a name as classification>`
- **Event status:** `<precursor | active contact | interrupted | closed | legacy-only | disputed | other factual state>`
- **Primary scope and horizon:** `<bounded region, actors, systems, and time window>`
- **Participating World Gate IDs:** `<zero or more stable Gate IDs>`
- **Record version:** `<revision>`
- **Campaign version:** `<applicable version reference>`

### Required Authority and Provenance

- **Persistence authority:** `<applicable authority layer>`
- **Truth layer:** `<Campaign Canon | Historical Record | Character Knowledge | Research | GM Secrets | another valid layer>`
- **Canonical mechanical owners:** `<owner for every claim family>`
- **Effective interval:** `<Timeline references>`
- **Source basis:** `<events, observations, records, Research, migration, correction, or explicit unknown>`
- **Last materially changed at:** `<Timeline or Transaction reference>`
- **Typed references and dependents:** `<relation type -> stable record ID>`
- **Validation status:** `<Validation Run, review result, warnings, or not yet validated>`

## Contact Claim

### Required Fields

- **Contact Domains:** `<two or more Contact Domain IDs>`
- **Material separation:** `<what made consequential exchange unavailable within the relevant scope and horizon>`
- **New or changed route:** `<what can now reach, perceive, influence, or cross>`
- **Qualifying consequence:** `<actual or credibly pending change supported by the route>`
- **Prior world truth:** `<established conditions before contact>`
- **Contact evidence:** `<direct effects, observations, records, or other evidence>`
- **Known unknowns:** `<missing or disputed facts>`
- **Non-contact alternatives considered:** `<ordinary discovery, ordinary route, magical transit, misinformation, or other classifications>`

A Contact Domain is a scoped side of the contact model, not automatically a universe, polity, culture, species, timeline, or metaphysically independent reality.

## Route Classification

Complete the classification before applying Gate rules.

- **Route type:** `<ordinary route | magical transit method | Dungeon Access Route | Soul Gate | World Gate | non-Gate World-Contact process | other established route>`
- **Functional evidence:** `<what the route actually does>`
- **Why this classification applies:** `<canonical criteria>`
- **Excluded classifications:** `<nearby mechanics and why they do not apply>`
- **Age or Reset relevance:** `<none | contributing condition | separately established transition or mechanism>`
- **Classification uncertainty:** `<evidence still needed>`
- **Owning document:** `<canonical source>`

A World-Contact Event need not contain a World Gate. A World Gate need not produce public first contact, conflict, transit, or an Age Transition.

## Contact Domain Entry

Create one entry per Contact Domain.

- **Contact Domain ID:** `<stable ID>`
- **Scoped boundary:** `<region, realm, population environment, isolated context, or other supported boundary>`
- **Internal diversity:** `<relevant populations, factions, ecologies, and conditions; never one unified side by default>`
- **Pre-contact accessibility:** `<what routes existed and for whom>`
- **Current relevant conditions:** `<linked Location, ecology, Magic, technology, law, and world-state records>`
- **Known participants:** `<Actor, Population, Faction, Institution, species, or other references>`
- **Knowledge of other domains:** `<observer-specific views>`
- **Naming and interpretation:** `<local labels without converting them into objective cosmology>`
- **Scope limits:** `<what this Domain entry does not represent>`

## Prior Causality and Origin

### Required Fields

- **Origin process:** `<opening, barrier removal, rediscovery, return, communication route, or other established cause>`
- **Causal Basis:** `<physical, magical, cosmological, technical, divine, living, institutional, or combined source>`
- **Precursor evidence:** `<signs genuinely produced by the Basis>`
- **Responsible actors:** `<persons or systems with actual causal roles>`
- **Triggering change:** `<threshold, action, alignment, failure, catastrophe, or other event>`
- **Dependencies:** `<supports required for continuing contact>`
- **Counterforces:** `<processes resisting, narrowing, redirecting, or ending contact>`
- **Alternative explanations:** `<competing Research or beliefs>`

Prophecy, player arrival, narrative timing, thematic preference, or later convenience is not a causal Basis.

## World Gate Profile

Complete one profile per World Gate that participates in the event. Skip this section when contact uses no World Gate.

### Gate Identity and Qualification

- **World Gate ID:** `<stable Gate ID>`
- **Gate names:** `<observer-specific labels>`
- **Gate identity continuity:** `<Basis, endpoints, functions, and causal history supporting one continuing Gate>`
- **Connected Contact Domains:** `<Domain IDs>`
- **Extraordinary separation:** `<why comparable current routes did not already provide mutual reach>`
- **Gate Basis:** `<actual sustaining process and owner>`
- **Qualification evidence:** `<endpoints, boundary, channels, operation, and world effect evidence>`
- **Public knowledge:** `<known, hidden, denied, disputed, or observer-specific>`
- **Personhood claim:** `<none unless another system independently establishes a person>`

### Gate Endpoint Entry

Create one entry per endpoint.

- **Endpoint ID:** `<stable ID>`
- **Contact Domain ID:** `<Domain reference>`
- **World-side location or condition:** `<Location, region, object, moving interface, or bounded condition>`
- **Spatial form:** `<fixed | mobile | concentrated | distributed | intermittent | other factual form>`
- **Current accessibility:** `<routes, timing, restrictions, and uncertainty>`
- **Current control and claims:** `<separate Actor, Faction, Institution, source, and legal references>`
- **Local conditions:** `<environmental, magical, technical, ecological, and social facts>`
- **Endpoint dependencies:** `<supports and failure risks>`
- **Evidence and knowledge views:** `<observer-specific sources>`

### Boundary and Footprint

- **Gate Boundary:** `<where Gate-specific transit, transformation, exclusion, detection, or failure rules apply>`
- **Boundary variation by channel:** `<differences or none>`
- **Direct Gate Footprint:** `<endpoints and routes directly affected>`
- **Propagated footprint:** `<linked specialist consequences beyond the Boundary>`
- **Causal attribution limits:** `<changes within the footprint not directly caused by the Gate>`
- **Current uncertainty:** `<unknown extent, timing, or behavior>`

The Gate Footprint is not ownership of every event inside it and does not replace specialist world-state records.

## Gate Channel Entry

Create one entry per supported transfer category and route. Do not infer one channel from another.

- **Channel ID:** `<stable ID>`
- **World Gate ID:** `<Gate reference>`
- **Channel category:** `<observation | communication | environmental exchange | energy and Mana flow | material transit | organism transit | embodied-person transit | causal influence without transit | other established category>`
- **Origin and destination endpoints:** `<ordered references>`
- **Directionality:** `<one-way | reciprocal | asymmetric | alternating | cyclic | conditional | uncertain>`
- **Current Throughput:** `<qualitative evidenced amount, frequency, speed, size, or complexity>`
- **Timing and availability:** `<continuous, intermittent, cyclical, scheduled, conditional, or unknown>`
- **Requirements:** `<size, composition, identity, permission, equipment, source, power, Ritual, or other conditions>`
- **Costs:** `<owner, bearer, cause, and recovery>`
- **Transit behavior:** `<duration, transformation, disorientation, contamination, damage, or other supported effect>`
- **Failure modes:** `<what can fail, warning signs, and outcomes>`
- **Safe withdrawal or recovery:** `<method or explicitly unavailable>`
- **Monitoring and evidence:** `<tests, observers, confidence, and limits>`
- **Current operation:** `<available | degraded | contested | suspended | closed | unknown>`

Throughput is channel-specific and never one Gate level. A communication route does not prove material transit; embodied crossing does not create a disembodied-Soul channel.

## Gate Operation and Lifecycle

### Opening

- **Opening Event ID:** `<Timeline reference>`
- **Opening pattern:** `<gradual | deliberate | unintended | barrier failure | cyclical | actor-opened | catastrophe | reactivation | other established cause>`
- **Basis state at opening:** `<supporting facts>`
- **Participants and agency:** `<who acted, intended, consented, resisted, or merely observed>`
- **Initial channels and limits:** `<Channel references>`
- **Immediate consequences:** `<specialist handoff references>`

### Current Operation

- **Operation state:** `<continuous | intermittent | cyclical | conditional | degrading | expanding | narrowing | redirected | contested | other factual state>`
- **Active supports:** `<Basis and dependency references>`
- **Operators:** `<actors who can affect which functions through what interfaces>`
- **Access controllers:** `<actors who can materially regulate which approaches or uses>`
- **Known interventions:** `<maintenance, negotiation, load change, sabotage, blockade, or adaptation>`
- **Expected change:** `<forecast with assumptions and confidence>`
- **Review Points:** `<conditions requiring re-evaluation>`

### Closure

- **Closure Event ID:** `<Timeline reference or none>`
- **Affected channels and endpoints:** `<exact scope>`
- **Closure type:** `<loss | suspension | sealing | redirection | ending>`
- **Cause and owner:** `<established process>`
- **Duration claim:** `<temporary | cyclical | conditional | effectively permanent | unknown>`
- **Contestation:** `<actors, claims, and material capacity>`
- **What remains:** `<crossed beings, materials, relationships, dependencies, and legacies>`
- **Reopening conditions:** `<if established; never assumed>`

Closure does not undo completed transit, erase consequences, restore prior conditions, or settle authority and ownership claims.

## Contact Compatibility Finding

Create one finding per subject, purpose, direction, and relevant context.

- **Finding ID:** `<stable ID>`
- **Subject:** `<person, species, material, Spell, technology, practice, message, crop, organism, or other exact subject>`
- **Purpose:** `<crossing, survival, communication, operation, reproduction, repair, use, or other scoped function>`
- **Direction and endpoints:** `<ordered route>`
- **Conditions tested:** `<current environment, Gate state, support, timing, and preparation>`
- **Relevant layers:** `<physical | environmental | biological | magical | technical | sensory and cognitive | linguistic and informational | social and legal>`
- **Evidence:** `<observation, experiment, event, or Research references>`
- **Result:** `<supported | conditional | incompatible | uncertain | untested, with explanation>`
- **Limits and exceptions:** `<scope boundaries>`
- **Adaptations or support:** `<separate Skill, Magic, equipment, infrastructure, treaty, or Research references>`
- **Failure and risk:** `<known consequences>`
- **Confidence and last review:** `<Research confidence and Timeline reference>`

There is no universal compatibility percentage. Compatibility for one function, direction, body, or item does not transfer automatically to another.

## Contact Process

The canonical Contact Phases are descriptive and may overlap, repeat, reverse, differ by actor, or be skipped.

### Actor-Specific Phase Entry

- **Actor or scope ID:** `<Faction, Institution, Population, person, region, or other reference>`
- **Current Contact Phase:** `<Precursor | Opening or breach | Discovery | Containment or uncontrolled contact | First exchange | Escalation or de-escalation | Institutional response | Adaptation | Durable outcome>`
- **Evidence:** `<observed actions and conditions>`
- **Knowledge and confidence:** `<what this actor knows or believes>`
- **Objectives and constraints:** `<linked actor or Faction records>`
- **Recent transition:** `<what changed and why>`
- **Possible next branches:** `<causal possibilities, not scripted outcomes>`
- **Response delay:** `<information, decision, mobilization, or implementation lag>`

The phases are not a countdown, mandatory ladder, civilization rank, or prediction that contact becomes permanent.

## Information and Communication

### Information View Entry

- **Observer ID:** `<person, group, Faction, Institution, or source>`
- **Truth layer:** `<appropriate layer>`
- **Observation:** `<what was directly perceived>`
- **Interpretation:** `<what the observer believes it means>`
- **Confidence:** `<Confirmed | Strong | Supported | Tentative | Speculative | Rumour | Disproved, where Research applies>`
- **Source and provenance:** `<message, sensor, witness, Archive Record, memory, propaganda, or other source>`
- **Known limitations:** `<delay, deception, translation, access, cognitive, sensory, or cultural limits>`
- **Contradictory evidence:** `<references>`
- **Visibility:** `<who may access this view>`

### Communication Route Entry

- **Communication Route ID:** `<stable ID>`
- **Supporting Gate Channel or other route:** `<reference>`
- **Participants:** `<distinct actors>`
- **Signal and interpretation method:** `<language, code, Magic, technology, nonverbal method, or other route>`
- **Direction and delay:** `<current facts>`
- **Authentication:** `<how source identity is assessed>`
- **Translation and conceptual limits:** `<known false equivalents or gaps>`
- **Interference and failure:** `<noise, censorship, deception, incompatibility, or other cause>`
- **Current reliability:** `<evidence-based qualitative finding>`

Fluency does not prove conceptual accuracy, shared values, authority, consent, or truthful intent. No Gate supplies universal translation.

## Discovery, Access, Control, and Authority

Create separate entries where different actors hold different claims.

- **Discovery:** `<who obtained credible evidence, when, and how>`
- **Reach:** `<who can approach which endpoint or information route>`
- **Use:** `<who can satisfy which Channel conditions>`
- **Operation:** `<who can affect which Gate functions through what interface>`
- **Control:** `<who can materially regulate what scope, for how long, and with what limits>`
- **Claim:** `<ownership, custody, stewardship, jurisdiction, sacred responsibility, or exclusion asserted>`
- **Authority:** `<whose decision is recognized by which audience and why>`
- **Consent:** `<which affected persons or sources agree to which use>`
- **Disputes:** `<competing claims, evidence, and current material consequences>`
- **Circumvention:** `<smuggling, corruption, alternate routes, dissent, or Counterforces>`

Discovery grants none of the other claims automatically. Technical operation, political control, legal claim, legitimacy, and consent remain independent.

## Participants and Autonomous Response

Create one entry per materially relevant actor or coordinated group.

- **Participant ID:** `<Actor, Faction, Institution, Population, species group, spirit, god, Weapon Soul, or other person/group>`
- **Contact Domain and location:** `<references>`
- **Role in contact:** `<discoverer, traveler, resident, operator, claimant, researcher, trader, refugee, defender, mediator, dissenter, or other observed role>`
- **Objectives and interests:** `<linked source record>`
- **Knowledge and beliefs:** `<information-view references>`
- **Decision route:** `<individual agency or Faction procedure>`
- **Capacity and constraints:** `<function-specific evidence>`
- **Current action:** `<Timeline reference>`
- **Relationships and obligations:** `<typed references>`
- **Response delay:** `<current lag>`
- **Possible independent action:** `<causal options rather than a script>`

Contact Domains are not unified sides. Off-screen actors continue to investigate, adapt, exploit, resist, cooperate, conceal, migrate, or disengage without waiting for the player.

## Causal Handoffs and Consequences

Create one entry for each direct effect or consequential branch.

### Handoff Entry

- **Handoff ID:** `<stable claim or Causal Event Chain node ID>`
- **Gate or contact input:** `<Event, Gate, Endpoint, Channel, crossing, information, closure, or legacy reference>`
- **Affected system:** `<Population | Resources and Food | Economy | Ecology and Migration | Disease | Faction | War and Unrest | Advancement | Dungeon | World Stability | Ages and Resets | Development | Skills | Classes | Monster Evolution | Magic | Soul Weapons | Soul systems | other owner>`
- **Authoritative owner:** `<canonical document and campaign module>`
- **Cause and mechanism:** `<supported causal route>`
- **Affected records:** `<typed references>`
- **Current outcome:** `<resolved fact, partial outcome, Pending Consequence, or unknown>`
- **Counterforces and feedback:** `<linked processes>`
- **Delay and horizon:** `<when effects may appear>`
- **Evidence and uncertainty:** `<source-specific view>`
- **Review Point:** `<condition for reassessment>`

The contact record supplies the route and evidence. It does not award Development, Skills, Classes, Evolutions, Magic, Soul Weapons, Soul growth, resources, victory, or social legitimacy.

## Soul-System Interface

Complete only when a Soul, Reincarnation, or Soul Avatar claim materially intersects the event.

- **Current Soul state:** `<living embodiment | possible death | Final Death and severance | Interlife | candidate generation | new embodiment | Avatar Expression>`
- **Ordinary Gate effects first:** `<travel, exposure, information, costs, and failure references>`
- **Soul singularity:** `<one continuing Soul and one active Incarnation preserved>`
- **Final Death finding:** `<separate Reincarnation owner reference or none>`
- **Cross-Domain Reincarnation Route:** `<established route ID, scope, and Basis or not applicable>`
- **Candidate reasoning:** `<separate Reincarnation Generation reference; never a Gate grant>`
- **Avatar Trigger:** `<actual history, Resonance, responsibility, and contributor references or none>`
- **Avatar Expression:** `<Soul Avatar Profile and event reference; no Gate operation by status>`
- **Soul Weapon:** `<distinct person, vessel, consent, and channel compatibility>`
- **Provenance limits:** `<dated Echo, Imprint, Archive, Title, or Association sources>`
- **Agency protections:** `<player, Echo, Weapon Soul, source, and contacted-person choices>`

A World Gate has no disembodied-Soul channel by implication. It is not Interlife, a Soul Gate, a Reincarnation menu, an Echo route, or an Avatar portal.

## Embodied Crossing Entry

Create one entry per material crossing or attempted crossing when the detail matters.

- **Crossing Event ID:** `<Timeline reference>`
- **Traveler or cargo IDs:** `<distinct Actor, Population, organism, material, vessel, or Inventory references>`
- **Channel and direction:** `<Channel reference>`
- **Permission and consent:** `<relevant separate states>`
- **Preparation:** `<equipment, Skills, Magic, information, allies, and infrastructure>`
- **Transit conditions:** `<actual requirements and current Gate state>`
- **Compatibility findings:** `<subject-specific references>`
- **Outcome:** `<crossed | withdrew | blocked | stranded | injured | altered by an explicit owner | lost | unknown>`
- **Costs and failures:** `<source-owned effects>`
- **Destination state:** `<Location, exposure, custody, knowledge, and relationship references>`
- **Return route:** `<available, conditional, absent, or unknown>`
- **Affected records:** `<typed Save Update references>`

Crossing does not automatically trigger Reincarnation, adaptation, Evolution, mastery, immunity, or a safe return.

## Closure, Stranding, and Gate Legacies

### Stranding Entry

- **Stranding ID:** `<stable ID>`
- **Lost or changed route:** `<Channel, endpoint, or closure reference>`
- **Affected persons, populations, systems, or dependencies:** `<typed references>`
- **Current location and condition:** `<external records>`
- **Separated relationships and obligations:** `<Relationship, Faction, Institution, contract, or family references>`
- **Available alternatives:** `<actual resources and routes>`
- **Immediate and pending consequences:** `<specialist handoffs>`
- **Agency and response:** `<choices by affected actors>`

### Gate Legacy Entry

- **Gate Legacy ID:** `<stable ID>`
- **Originating event and Gate:** `<references>`
- **Persistent consequence:** `<population, language, institution, law, belief, ecology, technology, Magic, infrastructure, debt, claim, memorial, grievance, dependency, or other state>`
- **Authoritative owner:** `<specialist record>`
- **Affected scope:** `<locations, actors, systems, and horizon>`
- **Current condition:** `<active, transformed, declining, disputed, hidden, or other factual state>`
- **Causal continuity:** `<how the consequence persists after operation changes>`
- **Knowledge and interpretation:** `<observer-specific views>`
- **Review Points:** `<conditions that may transform the Legacy>`

Closure does not erase stranding or Legacies. Reopening does not restore former relationships, laws, ecologies, dependencies, or expectations unchanged.

## Change and Event History

Append meaningful changes; do not overwrite prior world truth without an authorized correction.

### Change Entry

- **Change ID:** `<stable Timeline event or Transaction ID>`
- **Effective time:** `<Timeline reference>`
- **Affected Event, Gate, Endpoint, Channel, or claim:** `<exact IDs>`
- **Previous state:** `<reference>`
- **New state:** `<reference>`
- **Cause:** `<actor action, Basis change, load, maintenance, sabotage, closure, world change, migration, or correction>`
- **Authority and source:** `<record owner and evidence>`
- **Affected Set:** `<records requiring updates>`
- **Pending Consequences:** `<future checks and owners>`

## Validation Checklist

Before activating or updating a populated World-Contact Record, verify:

- [ ] Contact Domains, material separation, scope, and consequential route are established.
- [ ] The route is classified by function and nearby mechanics remain distinct.
- [ ] Prior causality and an actual Basis exist; no Gate was invented retroactively for convenience.
- [ ] Every World Gate, endpoint, channel, actor, and consequence has a stable identity.
- [ ] Each Gate has a supported Basis, endpoints, Boundary, channels, current operation, and evidence.
- [ ] Directionality, Throughput, requirements, costs, and failures are recorded per channel.
- [ ] Contact Compatibility is purpose-, subject-, direction-, and context-specific.
- [ ] Contact Phases are actor-specific descriptors rather than a mandatory ladder.
- [ ] Discovery, reach, use, operation, control, claim, authority, and consent remain separate.
- [ ] Contact Domains are not treated as unified actors or ranked by one power, technology, or magic score.
- [ ] Information views preserve observation, belief, Research, rumours, secrets, and uncertainty separately.
- [ ] Communication does not imply universal translation, authentication, shared concepts, or authority.
- [ ] Every consequence is handed to one specialist owner with a supported causal route.
- [ ] Off-screen actors and Counterforces continue without player presence.
- [ ] World Gates remain distinct from Soul Gates, Dungeon Access Routes, ordinary portals, Age Transitions, and World Resets.
- [ ] Reincarnation uses its ordinary seven stages and any Cross-Domain Route is separately established.
- [ ] Soul Avatar status grants no Gate operation, universal interpretation, parallel self, or authority.
- [ ] Embodied crossing preserves current embodiment and applies actual Channel and compatibility limits.
- [ ] Closure preserves completed effects, stranding, relationships, and Gate Legacies.
- [ ] Timeline, Campaign History, relevant world records, Pending Consequences, and validation results are updated externally.
- [ ] No populated campaign data has entered the canonical repository.

## Canonical Dependencies

- [World Engine Index](../docs/world-engine/README.md)
- [World Gates and World-Contact Events](../docs/world-engine/GATES_AND_WORLD_CONTACT.md)
- [World Gate Interactions with Reincarnation and Soul Avatars](../docs/world-engine/WORLD_GATE_SOUL_INTERACTIONS.md)
- [World Engine Overview](../docs/world-engine/WORLD_ENGINE_OVERVIEW.md)
- [World-State Variables](../docs/world-engine/WORLD_STATE_VARIABLES.md)
- [Causal Event Chains](../docs/world-engine/CAUSAL_EVENT_CHAINS.md)
- [Simulation Abstraction](../docs/world-engine/SIMULATION_ABSTRACTION.md)
- [Faction Behaviour](../docs/world-engine/FACTION_BEHAVIOUR.md)
- [Ecology and Migration](../docs/world-engine/ECOLOGY_AND_MIGRATION.md)
- [Disease Evolution](../docs/world-engine/DISEASE_EVOLUTION.md)
- [War and Unrest](../docs/world-engine/WAR_AND_UNREST.md)
- [Technology and Magical Advancement](../docs/world-engine/TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md)
- [Dungeon Activity](../docs/world-engine/DUNGEON_ACTIVITY.md)
- [World Stability](../docs/world-engine/WORLD_STABILITY.md)
- [Ages and World Resets](../docs/world-engine/AGES_AND_WORLD_RESETS.md)
- [Reincarnation](../docs/soul/REINCARNATION.md)
- [Soul Avatars](../docs/soul/SOUL_AVATARS.md)
- [Soul Avatar Profile Template](SOUL_AVATAR_TEMPLATE.md)
- [Game Master Framework](../docs/gm/GAME_MASTER_FRAMEWORK.md)
- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [Truth Layers](../docs/persistence/TRUTH_LAYERS.md)
- [Timeline Engine](../docs/persistence/TIMELINE_ENGINE.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)

## Optional Extensions

- Add storage-specific schema, front matter, maps, channel matrices, or automated views only when they preserve this logical contract.
- Add setting-specific Gate Bases, channel categories, compatibility layers, or world-contact processes only through ordinary canonical governance.
- Add private GM views by referencing the same stable facts through GM Secrets rather than duplicating public records.
- Add qualitative monitoring and validation without creating a universal Gate level, compatibility percentage, civilization rank, threat score, or scripted outcome.

## Consumers and Handoffs

- **Session preparation** reads only the Event, Gate, actor, location, information, and Pending Consequence records relevant to the expected scope.
- **World simulation** advances operation, actors, Counterforces, branches, delays, and specialist handoffs without centering the player.
- **Travel and exposure adjudication** uses exact Channel, direction, conditions, compatibility, preparation, and failure records.
- **Factions and institutions** retain their own decisions, legitimacy, access, control, claims, dissent, and response delays.
- **Soul and Reincarnation procedures** use the separate world-contact interface without inheriting Gate channels or skipping their own stages.
- **Timeline and Campaign History** preserve opening, discovery, crossing, closure, stranding, and Legacies.
- **Save Updates, migration, continuity resolution, and validation** update the Affected Set while preserving authority, provenance, uncertainty, and typed references.

This template records new causal routes. It does not decide what the world will do with them.
