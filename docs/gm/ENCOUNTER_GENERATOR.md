# Encounter Generator

## Purpose

This document defines how a Game Master turns established world conditions into playable encounters without scripting outcomes, scaling opposition to the participants, or placing campaign state in the canonical repository.

It is a procedure for finding and framing causal intersections. It is not a combat system, random encounter table, actor generator, reward schedule, or adventure plot.

## Core Rule

An encounter exists when established actors, processes, conditions, hazards, opportunities, or consequences intersect closely enough that a player decision can materially affect what happens next.

The generator may identify and frame that intersection. It cannot create a source without a causal route, decide a participant's response in advance, guarantee that the encounter occurs, or determine its outcome.

## Ownership

This document owns:

- encounter-source collection;
- conversion of established sources into candidate Encounter Seeds;
- eligibility checks for place, time, information, agency, and canonical support;
- selection of a useful Encounter Frame;
- presentation readiness;
- handoffs into specialist adjudication and consequence procedures;
- external encounter-record guidance.

It does not own:

- the existence, traits, Development, Skills, Classes, Evolution, motives, or decisions of participants;
- world processes, faction strategy, ecology, migration, war, disease, Dungeons, Gates, or other source systems;
- capability outcomes;
- uncertainty resolution;
- consequences after an outcome;
- treasure, progression, titles, awakenings, or rewards;
- campaign continuity records.

[Game Master Responsibilities](GM_RESPONSIBILITIES.md) governs authority and handoffs. [Capability Assessment](../progression/CAPABILITY_ASSESSMENT.md), [Uncertainty Handling](UNCERTAINTY_HANDLING.md), and [Consequence Resolution](CONSEQUENCE_RESOLUTION.md) retain their respective ownership.

## Key Terms

### Encounter

An **Encounter** is a bounded period in which at least one established world-side element has become materially relevant to a player-controlled character's available choices.

An Encounter may involve:

- conversation;
- observation;
- pursuit or evasion;
- negotiation;
- care, rescue, or cooperation;
- exploration;
- environmental danger;
- legal or institutional procedure;
- trade, obligation, or scarcity;
- ritual or magical conditions;
- combat;
- deliberate refusal or withdrawal.

An Encounter is not synonymous with combat, an enemy, a balanced challenge, a scene, or an event. A player may prevent, avoid, postpone, redirect, or leave it.

### Encounter Source

An **Encounter Source** is an established actor, process, condition, hazard, opportunity, obligation, trace, or Pending Consequence capable of reaching the current situation through a valid causal route.

The source exists before it becomes an Encounter. A migrating predator, inspection order, collapsing roof, arriving caravan, spreading rumor, expiring debt, and opening Gate can each be sources. None exists merely because the session needs activity.

### Encounter Seed

An **Encounter Seed** is an unselected possibility that describes how one or more Encounter Sources could intersect the current participants within a stated time and place.

A seed is preparation, not fact. It must pass eligibility before presentation and may cease to be valid when actors, routes, timing, or player choices change.

### Encounter Frame

An **Encounter Frame** is the current playable boundary around one eligible intersection. It records:

- the present time, place, and useful resolution;
- established sources and participants;
- each relevant observer's available information;
- active pressures and timing;
- the decision now available to the player;
- objectives, dependencies, Counterforces, and visible stakes;
- specialist owners and unresolved questions;
- conditions for expanding, compressing, splitting, or ending the Encounter.

The Frame is not a script. It organizes what is true and what is presently decision-relevant.

### Encounter Pressure

An **Encounter Pressure** is a condition that makes delay, action, refusal, or commitment materially consequential within an Encounter.

Pressure may be danger, dwindling time, social expectation, opportunity cost, uncertainty, exposure, resource need, legal procedure, another actor's movement, or an environmental process. Pressure is not permission to force a choice or manufacture urgency.

### Encounter Handoff

An **Encounter Handoff** routes a framed claim to its canonical owner for resolution or continued simulation. It identifies the exact question, established facts, observer view, player intent, applicable owner, and uncertainty without pre-deciding the result.

## Encounter Versus Related Concepts

| Concept | What it owns | Why it is not automatically an Encounter |
| --- | --- | --- |
| **World event** | A material occurrence or change in world state | It may happen off-screen or never become relevant to a player decision |
| **Causal Event Chain** | Links among causes, actors, Counterforces, and consequences | A chain can remain entirely outside the current play focus |
| **Scene** | A presentational span of play | A scene may contain several Encounters or no material decision |
| **Challenge** | A difficult objective or obstacle | An Encounter may be easy, welcome, informational, or cooperative |
| **Conflict** | Incompatible objectives, claims, or actions | Conflict can remain latent, institutional, or off-screen |
| **Combat** | Physical or magical violent action | Many Encounters contain no violence, and violence may be avoided |
| **Hazard** | A dangerous condition or process | A remote or irrelevant hazard is not yet an Encounter |
| **Participant** | An actor or affected subject in the Frame | Presence does not imply hostility, equality, or importance |

## Encounter Inputs

Generation begins from an external **Encounter Brief**. Use only fields relevant to the current preparation horizon.

```markdown
### Encounter Brief

Present:
- time and place:
- player-controlled characters and current embodiments:
- current activity, declared destination, or standing plan:

Relevant external state:
- nearby established actors:
- active processes and conditions:
- due Pending Consequences:
- obligations, opportunities, and traces:
- current world-system Review Points:

Information:
- Factual View relevant to preparation:
- Observer Views:
- player-facing evidence already established:
- protected or unresolved information:

Preparation horizon:
- spatial and temporal reach:
- useful simulation resolution:
- canonical and provisional limits:
```

This brief belongs in the external Campaign Record. The repository contains only the blank procedure.

## Encounter Source Families

Source families organize preparation. They do not assign probability or require one source from every family.

### Deliberate Player Pursuit

The players seek a person, place, creature, resource, answer, route, institution, confrontation, or opportunity.

The GM follows the declared pursuit through actual access, travel, information, timing, and Counterforces. Pursuit does not guarantee the target is available, unchanged, willing, or found.

### Actor Movement and Intent

An autonomous actor's goal, route, duty, fear, need, or response crosses the current situation.

The actor requires an established reason and only its own Observer View. A prepared actor may change course before contact when new information or pressure supports it.

### Ecology and Environmental Process

Migration, predation, spawning, competition, weather, fire, flood, collapse, mana flow, contamination, or another process reaches the location.

The process follows its owner and timescale. It does not wait for a party or adjust itself to their capability.

### Faction and Institutional Operation

Recruitment, inspection, taxation, policing, trade, worship, diplomacy, logistics, censorship, relief, mobilization, or internal dispute reaches the current participants.

The Encounter uses actual offices, decision routes, capacity, information, dissent, delay, and legitimacy. A faction label does not act as one omniscient mind.

### Prior Consequence or Obligation

A debt, promise, injury, trace, legal claim, relationship change, retaliation, reward already owed, warning, investigation, or Pending Consequence becomes due.

This source must preserve its original route and scope. It cannot be enlarged because the current moment would make a dramatic callback.

### Resource and Opportunity Intersection

A scarce input, market opening, shelter, information source, vulnerable route, cooperative need, salvage opportunity, or beneficial environmental condition becomes reachable.

Opportunity is allowed to be genuinely useful. The generator need not hide a trap or offset every benefit with a complication.

### Site and Infrastructure Activity

A settlement, road, ruin, Dungeon, shrine, workshop, Gate, transport route, ward, or other structure is operating, failing, changing, contested, or being used.

The site follows its actual maintenance, inhabitants, ownership, access, hazards, and current activity. Entering it does not trigger content by category alone.

### Information Intersection

An observation, rumor, record, signal, testimony, magical trace, or contradiction becomes available through a real carrier.

Information may itself require a decision: investigate, disclose, ignore, verify, conceal, trade, or act under uncertainty. A clue is not proof unless its evidence supports that status.

### Coincidental Intersection

Independent routes may genuinely cross without either side intending contact.

Coincidence requires compatible place and time. If whether the crossing occurs is unresolved, [Uncertainty Handling](UNCERTAINTY_HANDLING.md) may resolve that exact branch. Randomness cannot invent the travelers, route, motive, cargo, capability, or consequence.

## Eligibility Tests

An Encounter Seed is eligible only when every applicable test passes.

### 1. Causal Existence

Every source, participant, object, condition, and capability already exists or is produced through an owning system before presentation.

A later generator may create a needed actor or site profile from world-supported inputs. Until that dedicated generation is complete and validated, the Encounter Seed remains preparation rather than established fact.

### 2. Spatial and Temporal Reach

Routes, speed, access, communication, perception, scheduling, and delay permit the intersection within the preparation horizon.

Narrative usefulness does not teleport actors, accelerate institutions, suspend ecology, or make an opportunity wait indefinitely.

### 3. Actor Integrity

Every agentive participant has its own information, motives, priorities, relationships, capability, constraints, and current commitments.

The seed cannot assume hostility, cooperation, surrender, betrayal, persistence, or escalation merely to create a desired shape.

### 4. Process Integrity

Non-agentive hazards and processes follow established conditions, rates, triggers, Counterforces, and limits.

They do not target the protagonist, pause during dialogue, intensify to preserve difficulty, or disappear after providing spectacle unless their actual mechanisms support it.

### 5. Information Integrity

Factual, Observer, and player-facing views remain separate. Surprise requires prior causal existence and fair evidence where meaningful choice depends on recognizing a category of risk.

An unknown fact may remain hidden. It cannot be chosen retroactively after seeing the player's counter.

### 6. Decision Relevance

At least one player-controlled character can make or maintain a meaningful choice that may affect exposure, information, relationship, position, cost, timing, objective, or consequence.

If no player decision is due, continue world simulation or present ordinary description. Do not inflate every observation into an Encounter.

### 7. Canonical Support

The intended play can be adjudicated through Canonical rules, a Canonical Foundation plus narrow [Provisional Ruling](PROVISIONAL_RULINGS.md#provisional-ruling-procedure), or an explicitly approved experiment.

An Encounter must not quietly require an unsupported major subsystem.

### 8. Agency and Consent

The Frame leaves deliberate player intent with the player and participant choices with their owners. Any control, compulsion, binding, possession, coercion, consent, or personhood claim uses its canonical owner.

An Encounter Pressure may make every option costly. It cannot erase all options by narration when valid alternatives remain.

### 9. State and Continuity

The seed respects established injuries, resources, locations, relationships, deaths, schedules, prior choices, and Pending Consequences.

It cannot silently restore spent resources, resurrect participants, forget witnesses, reset a site, or repeat a resolved event.

## Generation Procedure

### Step 1: Set the Preparation Horizon

Choose the narrowest time, place, and simulation resolution useful for the next likely player decisions.

Do not prepare a continent of Encounters for a conversation in one room. Do not reduce a regional migration to one arbitrary roadside creature when the wider pressure is decision-relevant.

### Step 2: Load Established State

Read the current external Campaign Record and relevant canonical owners. Identify:

- participant embodiments, locations, conditions, and declared plans;
- nearby actors and their current activities;
- active processes and due Review Points;
- Pending Consequences and obligations;
- established information and protected truth;
- relevant access, law, ecology, resources, infrastructure, and timing.

Missing state is a [Record Gap](UNCERTAINTY_HANDLING.md#record-gap), not creative permission.

### Step 3: Collect Encounter Sources

List only sources capable of entering the preparation horizon. Record their owners, routes, timing, current state, and evidence.

Sources may combine when their routes genuinely intersect. Do not combine unrelated items merely to make a more elaborate scene.

### Step 4: Advance Sources to the Earliest Review Point

Use [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md) and [Causal Event Chains](../world-engine/CAUSAL_EVENT_CHAINS.md) to advance each source only as far as current causality permits.

Resolve actor choices, Counterforces, delays, and branches at their proper points. Do not assume every source reaches the player.

### Step 5: Draft Encounter Seeds

For each plausible intersection, state:

- which sources meet;
- where and when they could meet;
- why they reach one another;
- what is already true;
- what remains a participant choice or unresolved branch;
- which player decision could become due.

Drafting a seed does not commit it to world truth.

### Step 6: Apply Eligibility Tests

Reject or defer seeds that fail existence, reach, actor integrity, process integrity, information, decision relevance, support, agency, or continuity.

When a seed depends on an unresolved branch, define its Resolution Trigger under [Deferred Resolution](UNCERTAINTY_HANDLING.md#deferred-resolution). Do not resolve it early solely to finish preparation.

### Step 7: Select the Next Material Intersection

Prefer the earliest intersection that becomes materially relevant under current player action and world timing.

Selection is not a contest for the most dramatic seed. Consider:

- causally due timing;
- the player's declared direction;
- urgency that already exists;
- unresolved choices that block later simulation;
- information needed for fair commitment;
- the useful scope and pace of play.

Several seeds may remain prepared. Player action may reach none of them.

### Step 8: Establish Participant Objectives and Process Directions

For each agent, record what it currently seeks, avoids, protects, believes, and can attempt. For each process, record direction, rate, trigger, and Counterforces.

Objectives need not oppose one another. Participants may misunderstand the interaction, discover compatibility, or care about different outcomes.

### Step 9: Identify the Player Decision

State what the player can decide now at the character's current perspective.

Examples include:

- approach, observe, avoid, hide, warn, negotiate, attack, assist, follow, wait, retreat, or change route;
- disclose or conceal information;
- accept, refuse, alter, or delay an agreement;
- spend a resource, call an ally, prepare a counter, or accept a known risk.

Do not write the choice, emotion, belief, speech, or deliberate action for the player-controlled character.

### Step 10: Build the Encounter Frame

Set the immediate boundary:

- time, place, scale, and environment;
- participants and non-agentive processes;
- visible facts and concealed established truth;
- Encounter Pressures and timing;
- objectives, dependencies, bottlenecks, and Counterforces;
- plausible avoidance, withdrawal, de-escalation, and noncombat routes;
- specialist owners and exact questions;
- conditions that would change the Frame.

The Frame should be detailed enough for informed choice and no more detailed than the current decision requires.

### Step 11: Assess Relevant Capability

Use [Capability Assessment](../progression/CAPABILITY_ASSESSMENT.md) for the actual objectives and conditions.

Do not ask, “What level is this Encounter?” Ask questions such as:

- Can the scout detect the migration before entering its path?
- Can the group cross the bridge before structural failure?
- Can the envoy establish legal standing before the guards act?
- Can the predator impose close range before the travelers withdraw?
- Can the ritual be interrupted before release?

Preserve uncertainty and hidden capabilities in their proper views.

### Step 12: Test Fair Presentation

Before commitment, identify what each present character can perceive or reasonably infer about:

- the basic category of the situation;
- visible danger and scale;
- obvious timing or pressure;
- available routes and barriers;
- known law, custom, relationship, or obligation;
- uncertainty and ways to investigate it;
- foreseeable kinds of irreversible consequence.

Fair warning is perspective-appropriate evidence, not a complete solution or exact probability.

### Step 13: Present Without Resolving

Describe the present situation at the character's Observer View and stop where player intent is due.

Do not narrate an unchosen approach, attack, acceptance, refusal, fear, trust, or retreat. Do not reveal hidden truth merely because it appears in the Encounter Frame.

### Step 14: Clarify and Route Intent

Receive the player's intended action and desired effect. Clarify only material ambiguity, then create the necessary Encounter Handoffs.

Each handoff states:

1. the attempted action and desired effect;
2. relevant established facts;
3. current conditions and evidence;
4. the exact claim to resolve;
5. the canonical owner;
6. unresolved uncertainty;
7. the result or record update needed next.

### Step 15: Resolve Through Owning Systems

Apply deterministic rules, actor choices, specialist procedures, or an authorized uncertainty method. The Encounter generator supplies no universal roll and no replacement resolution mechanic.

An Encounter may alternate among several owners. Close each immediate outcome before propagating its consequences.

### Step 16: Propagate Consequences

Use [Consequence Resolution](CONSEQUENCE_RESOLUTION.md). Preserve direct effects, costs, traces, information routes, responses, environmental changes, delays, and Pending Consequences without applying any state change twice.

An outcome may create a new Encounter Source. It does not automatically prolong the current Encounter.

### Step 17: Close, Split, Expand, or Continue

At the next player decision or Causal Horizon:

- **continue** when the same sources and immediate boundary remain useful;
- **split** when participants, places, objectives, or timing create independently meaningful decisions;
- **expand** when a wider process becomes material;
- **compress** when only supported progression between Review Points matters;
- **close** when no immediate decision remains.

The Encounter ends because its decision boundary has changed, not because every opponent is defeated or every source is exhausted.

### Step 18: Record Externally

Record only material campaign facts:

- confirmed outcomes and costs;
- changed locations, conditions, resources, relationships, and information;
- actor responses already chosen;
- traces, obligations, delays, and Pending Consequences;
- unresolved branches and Resolution Triggers;
- relevant adjudication traces and Provisional Rules.

Do not store a played Encounter in this repository.

## Encounter Frame Template

```markdown
### Encounter Frame: <external campaign identifier>

Present boundary:
- time, place, scope, resolution:
- player-controlled observers:

Established sources:
- <source, owner, route, timing>

Participants and processes:
- <actor/process, current state, objective/direction, information>

Information views:
- Factual View:
- Observer Views:
- player-facing evidence:
- unresolved or protected information:

Decision and pressure:
- player decision now due:
- Encounter Pressures:
- visible stakes:
- timing or Review Point:

Capability context:
- objective-specific advantages:
- dependencies and bottlenecks:
- Counterforces and support:
- relevant uncertainty:

Agency routes:
- engagement:
- avoidance or withdrawal:
- noncombat or de-escalation:
- information-seeking:

Handoffs:
- <exact question -> canonical owner>

Boundary changes:
- continue when:
- split or expand when:
- close when:
```

The template is optional and external. Omit fields that cannot affect the present decision.

## Selection and Randomization

A generator may help organize several eligible seeds. It does not make them equally likely or interchangeable.

### Deterministic Selection

Use the causally earliest seed when established movement, timing, access, and player action settle which intersection occurs.

### Actor-Selected Intersection

An autonomous actor may choose whether, where, and how to seek contact using its own information, motives, capacity, and alternatives.

### Player-Selected Intersection

A player's route, pursuit, delay, preparation, or avoidance may select which sources become reachable. Preserve that effect rather than replacing it with a preferred seed.

### Uncertain Intersection

If several branches remain supported after evidence and capability are applied, [Uncertainty Handling](UNCERTAINTY_HANDLING.md) may resolve the exact intersection question.

Before randomization, commit:

- the question;
- supported seeds;
- causal weighting or exclusions;
- method;
- disclosure status;
- recording procedure.

The result establishes only the chosen intersection. It does not settle participant intent, surprise, initiative, outcome, loot, progression, or later consequences.

### Repeated Travel and Routine Exposure

Do not make repeated journeys independent draws when world conditions persist. Patrol routes, migration seasons, cleared paths, alerts, avoidance measures, and prior contacts change future exposure through causality.

Routine uneventful travel is valid. A quiet interval does not require a compensating threat.

## Encounter Composition

### Participants

Include only actors and affected subjects that can reach or influence the Frame. A distant commander, hidden patron, ecosystem, or institution may be causally relevant without being a present participant.

Do not generate extra opponents to fill roles, action counts, party size, or an expected difficulty budget.

### Objectives

Record each participant's actual objective separately. Common objectives include passage, food, safety, territory, compliance, information, rescue, delay, capture, deterrence, concealment, recognition, trade, or escape.

“Win the Encounter” is not an actor objective.

### Environment

Use established terrain, weather, structures, visibility, mana conditions, law, bystanders, routes, and resources. The environment can advantage, constrain, separate, expose, or protect participants without becoming a universal modifier.

### Timing

Track only timing that can change a decision: approach, warning, countdown, travel, reinforcement, exhaustion, process rate, legal deadline, or response delay.

Do not create urgency solely to prevent deliberation.

### Escalation and De-escalation

Escalation requires a route such as refusal, detection, fear, injury, reinforcement, environmental change, command, or failed communication. De-escalation requires its own route and may remain possible after hostility begins.

Neither direction is mandatory. An actor may withdraw, freeze, bargain, divide, surrender, seek help, or change objectives when its circumstances support that choice.

### Avoidance and Withdrawal

Avoidance may occur before presentation when player preparation prevents contact. Withdrawal may occur during play when access, timing, routes, capability, and opposition permit it.

A successful retreat is an outcome, not a failed attempt to consume prepared content. Pursuit is a new actor decision or process, not an automatic Encounter phase.

### Aftermath

Aftermath consists of the actual consequences, traces, responses, and continuing processes. It is not a compulsory final scene and does not award progress by category.

## Fairness Without Level Scaling

Eternal Cycle contains no universal character level and no encounter difficulty number.

An Encounter is fair when:

- its sources and capabilities have prior causal existence;
- characters receive perspective-appropriate evidence of materially recognizable danger;
- investigation and preparation can matter where circumstances permit;
- valid counters, allies, tools, terrain, law, and plans retain their effects;
- avoidance, refusal, withdrawal, or noncombat approaches remain possible when causally available;
- uncertainty is declared and resolved within its scope;
- outcomes and consequences follow actual conditions;
- overwhelming threats are allowed to remain overwhelming.

Fairness does not require:

- equal combat strength;
- a direct-force solution;
- success being possible after every commitment;
- every option having equal cost;
- danger matching party size;
- a warning that reveals concealed identity or exact capability;
- rescue after ignored or unknowable risk.

The GM should distinguish **dangerous**, **unfairly invented**, and **poorly communicated**. Only the latter two are design or adjudication failures by default.

## Noncombat and Mixed Encounters

The generator should not treat combat as the default state.

A noncombat Encounter may still have:

- opposed objectives;
- irreversible stakes;
- strict timing;
- capability asymmetry;
- uncertainty;
- costly failure;
- consequences that outlast violence.

A mixed Encounter may move among negotiation, movement, observation, ritual, rescue, institutional procedure, and combat. Each transition requires a causal trigger and a new player decision where intent matters.

Social Development does not become mind control. Combat capability does not settle legal standing. Magical access does not settle consent. Institutional authority does not guarantee compliance. Use each owner only for its own claims.

## Information, Surprise, and Ambush

Surprise is an information condition, not a free outcome.

For a concealed approach or ambush:

1. establish the actor, plan, route, timing, and concealment before the counter is known;
2. identify traces and Observer Views;
3. assess detection and positioning under actual conditions;
4. resolve only genuinely unsettled claims;
5. preserve prior precautions and counters;
6. stop for player intent as soon as the character has a meaningful response opportunity.

Failure to detect may change position, warning, or available responses. It does not automatically determine injury, capture, death, or every participant's action.

## Cross-System Interfaces

### Soul Engine

Soul Depth, Soul Resonance, Soul Echoes, Retained Instincts, Soul Titles, Soul Constellations, and Soul Avatars may affect attention, access, interpretation, relationship, or available action only through their owners. An intense Encounter does not automatically grant Soul growth, awaken an Echo, create a Title, or reveal a constellation.

### Reincarnation

Final Death ends the current body's participation according to [Reincarnation](../soul/REINCARNATION.md). The Encounter may preserve worldly consequences and hand off to the seven-stage transition; it cannot generate an immediate replacement body or continue as though death were a scene transition.

### Development and Skills

Current expression, practised reliability, preparation, embodiment, conditions, and task-specific Skill access affect capability. Participation, repetition, success, failure, suffering, killing, or survival does not automatically grant Development or a Skill.

### Monster Evolution

Monsters retain ecology, needs, senses, society, personhood, adaptation, and Evolution constraints. An Encounter cannot spawn a species by difficulty, force hostility, grant an Evolution, or reduce a monster to a combat role.

### Human Classes and Professions

Classes, Professions, institutions, offices, certifications, obligations, and social structures affect access and expectations within their scopes. The generator cannot assign a Class to fill an Encounter role or make a profession universally authoritative.

### Soul Weapons

Soul Weapons and Weapon Souls remain partners with agency, compatibility, manifestation limits, and established capabilities. Near-death pressure may be relevant to [Awakening Conditions](../soul-weapons/AWAKENING_CONDITIONS.md), but the Encounter generator cannot guarantee awakening or use danger as a farming route.

### Magic

Magic uses established Mana relations, magical access, Skills, embodiment, sources, conditions, costs, and consequences. The generator cannot add a convenient spell, suppress nonmagical solutions, or refill resources between Encounters.

### World Engine

The World Engine supplies populations, resources, economies, ecology, faction action, conflict, disease, advancement, Dungeon activity, Stability, Ages, Gates, and causal persistence. The Encounter generator selects a decision-relevant intersection; it does not rewrite the underlying state.

### Provisional Rulings

When a narrow unresolved claim is supported by a Canonical Foundation, use the [Provisional Ruling Procedure](PROVISIONAL_RULINGS.md#provisional-ruling-procedure). Label the ruling externally and do not let an Encounter quietly implement a later generator or major system.

## Worked Examples

### Migrating Apex Predator

A drought has shifted prey through a mountain pass, and an established apex predator follows. The party's declared route reaches the same pass after scouts find large tracks and abandoned nests.

The Encounter Source is migration, not a level-appropriate monster. The Frame offers observation, delay, an alternate route, warning nearby travelers, concealment, bait diversion, retreat, or direct confrontation. Capability assessment may find the predator overwhelmingly advantaged in close combat while the travelers hold informational and route-planning advantages. Avoiding contact does not cause another predator to appear as compensation.

### Routine Inspection

A river authority checks cargo under an established contamination order. Its officers seek compliance and public safety; one merchant believes the order is corrupt, while the player carries an undeclared but lawful magical specimen.

The Encounter is institutional and informational. Legal knowledge, documents, negotiation, disclosure, concealment, delay, and relationship consequences matter. The officers are not enemies, successful compliance need not reveal a trap, and social capability cannot erase their agency or duties.

### Flooding Dungeon Route

Seasonal water and failed drainage are raising a ruin's lower reservoir. The delvers hear stressed masonry and see wet marker lines before choosing whether to continue toward a sealed archive.

The water process is the primary source. The Dungeon does not activate because visitors entered. The Frame tracks route, rate, structural uncertainty, escape time, tools, and the archive objective. Combat may never occur. A collapse follows Dungeon and consequence owners rather than a generic failed-check penalty.

### Ambush That Never Happens

Bandits prepare beside the western road using information from a bribed courier. The players verify the leak and take an eastern ferry.

Their preparation removes spatial reach. The Encounter Seed becomes ineligible; the GM does not move the bandits to the ferry or replace them with another ambush. The bandits continue acting off-screen and may respond later if they learn what occurred.

### Monster Community Negotiation

A human settlement diverts a stream used by an intelligent burrower community. Scouts from both groups reach the worksite with incompatible assumptions but a shared need to prevent a sinkhole.

The Encounter combines resource pressure, ecology, faction action, and mistaken belief. It does not label the monster community hostile or primitive. Participants may negotiate, inspect, threaten, cooperate, withdraw, or seek other authorities. Any agreement updates actual water routes and faction relationships rather than ending as a self-contained social scene.

### Soul Weapon Refusal Under Pressure

A fighter attempts to draw a Soul Weapon against a former partner whom the Weapon Soul refuses to harm. Approaching guards create time pressure.

The Encounter Frame preserves the Weapon Soul's agency and established relationship. The generator cannot convert refusal into a temporary debuff, force manifestation, or grant a new weapon ability. The player can change objective, use another available method, negotiate, flee, surrender, or accept consequences.

### Quiet Road

The travelers use a maintained road during stable weather. No actor route, due consequence, ecological pressure, institutional procedure, or unresolved branch materially intersects them before arrival.

The GM advances travel and records ordinary resource or time changes if material. No Encounter is generated. Uneventful movement is a valid causal result.

## Human and AI GM Use

Human and AI GMs use the same procedure and authority.

An AI GM should:

- search canonical owners and external campaign records before proposing sources;
- distinguish retrieved facts, inferences, and unresolved branches;
- cite or name specialist handoffs internally when claims cross systems;
- avoid filling missing records with dramatic content;
- stop at player decisions;
- preserve actor-specific information and agency;
- disclose Provisional Rules explicitly;
- record material outcomes and pending branches externally;
- ask for clarification when player intent, participant boundaries, or a material record conflict cannot be resolved safely.

An AI GM's ability to generate many plausible scenes grants no additional authority. A human GM's intuition grants no exemption from causal existence, ownership, or player agency.

## Safeguards

- **No level scaling:** Sources follow world state, not participant power or party composition.
- **No mandatory combat:** An Encounter is not an enemy delivery mechanism.
- **No scripted outcomes:** Preparation may establish sources and intent, never guaranteed participant choices or results.
- **No quantum placement:** Prepared actors and hazards do not move to whichever route the player selects.
- **No encounter quotas:** Quiet play, travel, work, recovery, and observation need no compensating interruption.
- **No drama tax:** A real benefit need not conceal danger or demand an unrelated cost.
- **No random canon creation:** Randomization selects only among supported committed branches.
- **No universal roll:** Specialist owners and deterministic conditions remain authoritative.
- **No hidden power score:** Capability remains objective- and context-specific.
- **No arbitrary hostility:** Aggression requires motive, information, trigger, means, and route.
- **No protagonist gravity:** World processes and actors do not converge on the player without causality.
- **No surprise by retroactivity:** Hidden threats require prior existence and appropriate traces.
- **No erased precautions:** Preparation, avoidance, wards, scouts, law, and allies retain causal effects.
- **No blocked withdrawal by convention:** Escape and refusal are assessed from actual routes and opposition.
- **No compulsory escalation:** De-escalation, misunderstanding, divided objectives, and retreat remain possible when supported.
- **No recycled state:** Sites, opponents, resources, and consequences do not reset between Encounters.
- **No automatic rewards:** Participation does not grant loot, Development, Skills, Titles, Evolution, Soul growth, or awakening.
- **No actor generation by implication:** A named role in a seed is not an established person or creature.
- **No campaign data in canon:** Briefs, Frames, outcomes, and played history remain external.
- **No automation supremacy:** The procedure supports judgment; it does not replace it.

## Scope Boundaries

This document does not define:

- monster generation;
- NPC generation;
- Dungeon generation;
- faction generation;
- world-event generation;
- combat actions or initiative;
- social influence mechanics;
- loot or reward generation;
- universal difficulty or challenge ratings;
- campaign-specific Encounters, maps, participants, or histories.

[Monster Generation](MONSTER_GENERATOR.md), [NPC Generation](NPC_GENERATOR.md), [Dungeon Generation](DUNGEON_GENERATOR.md), [Faction Generation](FACTION_GENERATOR.md), and [World-Event Generation](WORLD_EVENT_GENERATOR.md) may supply canon-compatible source profiles or occurrences. They cannot alter this document's existence, eligibility, framing, agency, information, or non-scaling rules.

## Related Documents

- [Game Master Framework](GAME_MASTER_FRAMEWORK.md)
- [Game Master Responsibilities](GM_RESPONSIBILITIES.md)
- [Consequence Resolution](CONSEQUENCE_RESOLUTION.md)
- [Uncertainty Handling](UNCERTAINTY_HANDLING.md)
- [Provisional Rulings](PROVISIONAL_RULINGS.md)
- [Capability Assessment](../progression/CAPABILITY_ASSESSMENT.md)
- [Causal Event Chains](../world-engine/CAUSAL_EVENT_CHAINS.md)
- [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md)
- [Ecology and Migration](../world-engine/ECOLOGY_AND_MIGRATION.md)
- [Faction Behaviour](../world-engine/FACTION_BEHAVIOUR.md)
- [Dungeon Activity](../world-engine/DUNGEON_ACTIVITY.md)
- [Monster Generator](MONSTER_GENERATOR.md)
- [NPC Generator](NPC_GENERATOR.md)
- [Dungeon Generator](DUNGEON_GENERATOR.md)
- [Faction Generator](FACTION_GENERATOR.md)
- [World-Event Generator](WORLD_EVENT_GENERATOR.md)
- [Time Skip Procedure](TIME_SKIP_PROCEDURE.md)
