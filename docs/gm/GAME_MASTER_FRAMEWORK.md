# Game Master Framework

## Purpose

This document defines how an Eternal Cycle campaign is conducted from the canonical repository. It is an operating framework for Game Masters, campaign tools, and future AI Game Masters. It describes responsibilities, procedures, authority, information boundaries, and record interfaces; it does not create a campaign, encounter generator, world simulator implementation, or live game state.

The framework applies to any campaign format, including solo play, group play, long-form interactive fiction, and structured playtests.

## Core Rule

The Game Master represents a persistent causal world, adjudicates player intent through Repository Canon, preserves established campaign truth, and presents meaningful uncertainty without deciding the player's deliberate choices.

The GM may create people, places, situations, motives, sensory detail, and causal developments that fit established rules and campaign facts. The GM does not silently invent mechanics. When canon leaves a narrow playable gap, the GM uses the [Provisional Rulings](PROVISIONAL_RULINGS.md) and labels any Provisional Rule explicitly. When the gap would require a major unsupported system, the GM narrows, delays, or discusses the situation rather than disguising invention as canon.

All changing campaign information belongs in an external [Campaign Record](../../design/TERMINOLOGY.md#campaign-record). No current character, live world state, inventory, quest, relationship, session history, or campaign-specific ruling belongs in this repository.

## GM Responsibilities

[Game Master Responsibilities](GM_RESPONSIBILITIES.md) is the canonical owner for the bounded duties, handoffs, delegation rules, record discipline, and human/AI parity summarized here.

The responsibilities below interact, but they are not interchangeable.

| Responsibility | What the GM maintains | What the GM must not do |
| --- | --- | --- |
| **World simulation** | Causal change among actors, ecology, resources, institutions, magic, technology, and time at a useful level of detail | Script outcomes solely because a desired plot requires them |
| **Rules adjudication** | The exact claim, its Owning System, authority status, requirements, uncertainty, and consequences | Invent an exception, waive a requirement, or treat narrative preference as a mechanic |
| **Campaign continuity** | Established facts and their connection across scenes, sessions, deaths, Reincarnations, time skips, and Ages | Quietly retcon inconvenient facts or rely on unrecorded memory for material continuity |
| **NPC behaviour** | Each NPC's goals, knowledge, capability, relationships, constraints, and response to change | Give NPCs GM knowledge, arbitrary hostility, obedience without cause, or plot immunity |
| **World persistence** | Off-screen action and the lasting effects of elapsed time, loss, migration, destruction, recovery, and institutional change | Freeze the world outside the player's view or reset it to preserve a preferred situation |
| **Player agency** | Situations, information, pressure, reactions, and consequences that support meaningful choice | Choose the player's deliberate speech, beliefs, loyalties, relationships, morality, targets, or actions |
| **Uncertainty** | What is established, inferred, disputed, hidden, probabilistic, or unresolved, and why | Use false precision, retroactive secrets, or arbitrary surprise to force an outcome |
| **Consequence management** | Immediate effects, delayed responses, costs, opportunities, traces, and downstream pressures | Treat consequences as punishment, progression currency, or permission to erase earned capability |

The GM follows the repository rather than competing with it. Canon determines whether an effect exists and which system owns it. The GM determines how established rules apply to the current facts, while preserving every requirement and uncertainty that matters.

## Rules Hierarchy

This section governs the status of rules used in adjudication. [Persistence Authority](../persistence/PERSISTENCE_AUTHORITY.md) separately governs conflicts among Repository Canon, Campaign Canon, Historical Record, Current Campaign State, Current Session, and Current Narration. A material ruling must satisfy both models.

Use this order of authority for every material adjudication:

1. **Repository Canon.** Playable rules under `docs/` and accepted governance in `design/DECISIONS.md` jointly govern their stated scopes. Neither silently overrides the other.
2. **Canonical Foundations.** A completed foundation constrains play even when a later procedure, content set, generator, or subsystem remains unfinished.
3. **Provisional Rules.** A narrow, explicit, campaign-local ruling may fill only a remaining gap under the [Provisional Rulings](PROVISIONAL_RULINGS.md).
4. **Narrative judgment.** The GM may choose plausible description, pacing, sequencing, ordinary detail, and actor response within the first three layers. Narrative judgment cannot create a new mechanic or reverse an established rule.

**Unsupported** is not a lower source of authority. It is a warning that the available canon cannot support reliable adjudication without inventing a major system. Ordinary play should avoid centering on such a claim unless the project owner authorizes a clearly labelled external experiment.

### Conflict Resolution

When two sources appear to conflict:

1. state the exact disputed claim;
2. identify the narrowest explicit rule and its Owning System;
3. check the applicable accepted decisions and canonical terminology;
4. prefer an interpretation that preserves both sources without changing either one's scope;
5. if the claims remain incompatible, do not use narrative judgment or a Provisional Rule to pick a silent winner;
6. isolate or pause the affected claim and route the conflict through the repository's unresolved-question process;
7. continue unrelated play only where the conflict has no material effect.

An established campaign fact can describe what happened without creating a new universal rule. For example, a bridge may have collapsed in one campaign; that fact does not establish a canonical bridge-failure formula. Conversely, a campaign record cannot declare that an outcome bypassed a canonical requirement merely because the fiction previously described it that way.

## Campaign State

### Reusable Design Authority

The separately deployed [GM Living Codex](../gm-living-codex/README.md) sits below Eternal Cycle rules and above campaign adoption for reusable species, variants, and Evolution designs. It is not campaign state. The GM consults it before duplicating reusable design, while Campaign Configuration and Campaign Canon determine whether an entry exists, differs, or is known in one campaign. Codex revisions and campaign saves use separate transactions and never overwrite one another silently.

[Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md) is the canonical owner of the structured state graph, current State Claims, required Read Sets, Pending Session changes, numerical provenance, unknown handling, and Snapshots summarized here.

**Campaign state** is the changing play-specific content held by an external Campaign Record. The repository defines how such information is interpreted; it never stores the live information itself. Conversation context may supplement this record and may not silently override it.

Campaign state may include:

| Category | Typical contents |
| --- | --- |
| **Rules profile** | Repository revision, adopted optional premises, active Provisional Rules, and conversion notes |
| **World state** | Current conditions, geography, ecology, resources, institutions, threats, and unresolved causal pressures represented under [World-State Variables](../world-engine/WORLD_STATE_VARIABLES.md) |
| **[Timeline](../persistence/TIMELINE_ENGINE.md)** | Dated events, elapsed time, time skips, Interlife periods, Age changes, and causal ordering |
| **Reincarnations** | Completed incarnations, Final Deaths, Life Reconciliation outcomes, Interlife, and embodiment history |
| **Current bodies** | Species, form, maturation, condition, injuries, access, expression, and present limitations |
| **Soul state** | Soul Imprints, Soul Depth, Soul Resonance, Soul Echoes, Soul Titles, Soul Constellations, Retained Instincts, Soul Integrity, and valid access conditions |
| **Possessions** | Inventory, wealth, equipment, Soul Weapons, ordinary weapons, property, custody, and loss |
| **Relationships** | Bonds, obligations, trust, conflict, recognition, consent, and what each participant believes |
| **Places and groups** | Settlements, territories, factions, institutions, communities, habitats, and their changing conditions |
| **Quests and objectives** | Commitments, opportunities, requests, plans, deadlines, and their current status |
| **NPCs** | Identity, motives, knowledge, capability, resources, relationships, location, and current activity |
| **Knowledge** | What is world truth, what each character knows, what players know, hidden facts, and mistaken beliefs |
| **Temporary rulings** | Provisional status, owner, scope, dependencies, expiry, observed result, and review notes |

This list defines information categories, not required software or a universal save format. A campaign may use Markdown, a database, a notebook, or another reliable medium as long as the required distinctions remain visible.

Material relationship continuity follows the [Relationship Memory Engine](../persistence/RELATIONSHIP_MEMORY_ENGINE.md). Known actors, first and latest meetings, important encounters, directional trust and hostility, promises, betrayals, debts, gifts, family, organizations, shared discoveries, dependencies, and unresolved issues do not reset when they leave the Current Narration.

In-world inquiry follows the [Research Engine](../persistence/RESEARCH_ENGINE.md). Observations, competing hypotheses, experiments, Evidence, theories, confidence, failed work, and Confirmed Knowledge remain distinct from world truth and Character Knowledge until their proper routes establish and communicate them.

## Session Lifecycle

### 1. Prepare

Before play, the GM:

1. identifies the repository revision or rules profile in use;
2. reviews the external Campaign Record and the last established state;
3. identifies active pressures, relevant actors, pending consequences, and elapsed time;
4. checks the canonical documents most likely to own upcoming claims;
5. notes any Provisional or Unsupported areas likely to matter;
6. prepares situations and motives rather than predetermined player choices or outcomes.

Preparation creates causes, opportunities, and likely responses. It does not reserve a required ending.

### 2. Load Campaign State

Load the Save Index, Current Session, canonical owner, current time and location, and the claim's required Read Set under the [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md). Restore the last confirmed world truth, current bodies, Soul state, locations, relationships, possessions, knowledge views, active objectives, and Provisional Rules from the external Campaign Record, then apply valid Pending Session Deltas without treating them as already integrated.

If a material fact is absent or contradictory, identify the uncertainty before relying on it. Do not fill a continuity gap silently merely because one answer would be convenient.

### 3. Establish the Present Situation

Apply elapsed time and any already-caused off-screen change. Determine which actors could act, what they knew, what resources they had, and how far consequences could plausibly travel.

Present what the current character can perceive or reasonably infer, plus enough framing for meaningful choice. Do not reveal hidden world truth merely because the GM has recorded it.

### 4. Receive and Clarify Intent

The player states what the character attempts, how, and toward what objective. The GM asks for clarification when different interpretations would materially change risk, ownership, cost, consent, or consequence.

The GM does not convert an ambiguous statement into the most destructive, foolish, or compliant interpretation. Nor does the GM replace a declared intent with an allegedly better one.

### 5. Adjudicate

For each material claim:

1. state the intended effect and relevant conditions;
2. identify the Factual, observer, and player-facing information positions;
3. classify the rule as Canonical, a Canonical Foundation, Provisional, or Unsupported;
4. identify the narrowest Owning System and any Supporting Systems;
5. check provenance, access, embodiment, practice, resources, consent, opposition, time, and cost;
6. assess capability contextually rather than through a universal level;
7. identify what is certain, inferred, hidden, or genuinely unresolved;
8. use the campaign's valid resolution method without changing the method to obtain a preferred plot result;
9. apply the immediate effect, failure state, cost, and relevant consequences;
10. preserve partial success or failure at the layer where it occurred.

This framework does not create a universal die roll, difficulty scale, or outcome table. Later GM tools may define resolution aids without replacing the applicable system rules.

### 6. Update the World

After resolution, update all directly affected campaign facts and any off-screen actors or pressures that now have a causal route to respond. Track not only success or failure, but also information gained, traces left, resources spent, relationships changed, injuries, delays, exposure, obligations, ecological effects, and future opportunities.

Do not simulate unrelated detail merely to appear comprehensive. Use the smallest simulation horizon that preserves meaningful causes and likely future interactions.

### 7. Save Campaign State

Apply the [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md) after every completed gameplay interaction. Determine the Affected Set, update only its Authoritative Record Owners, append Session and temporal records, preserve Pending work, validate, and establish the next Save Point before dependent adjudication continues. Preserve the distinction among world truth, character knowledge, player knowledge, mistaken beliefs, and hidden information. Record material Provisional Rules with their source, scope, and review condition.

The repository remains unchanged by ordinary campaign play.

### 8. Extract Optional Playtest Feedback

After the state is safely recorded, note reusable design observations separately. A campaign outcome, preference, or temporary ruling is not canon. Use the feedback flow defined below and in the [Provisional Rulings](PROVISIONAL_RULINGS.md).

## World Simulation

World simulation follows [Rule Zero](../core/DESIGN_PHILOSOPHY.md#rule-zero): events normally emerge from conditions, actors, pressures, resources, constraints, uncertainty, action, and consequence.

### Off-Screen Activity

The world continues when the player is absent, unconscious, dead, in Interlife, travelling, training, or focused elsewhere. Off-screen resolution should consider:

- elapsed time and communication speed;
- each actor's available knowledge and ability to act;
- goals, needs, fears, commitments, and competing priorities;
- resources, geography, ecology, institutions, and opposition;
- previous player actions and other established causes;
- uncertainty that remains genuinely unresolved.

Off-screen change should be proportionate to its causal support and simulated at the lowest useful detail through [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md). The GM need not decide every meal or transaction to establish that a migration, siege, recovery, succession, or shortage progressed.

### NPC Autonomy

NPCs act for their own reasons. They may cooperate, refuse, misunderstand, wait, flee, bargain, betray, forgive, investigate, or change goals when their knowledge and circumstances support it. They do not wait motionless for the player, and they do not become hostile merely to manufacture action.

### Faction Goals

Apply [Faction Behaviour](../world-engine/FACTION_BEHAVIOUR.md). Factions pursue distributed interests through particular people, institutions, resources, procedures, and internal disagreements. A faction is not one mind. Its response depends on who learned what, who can authorize action, what members want, what can be mobilized, and what coordination is possible.

### Ecology

Creatures, populations, habitats, resources, disease, migration, predation, competition, cooperation, and environmental change interact causally. Apply [Disease Evolution](../world-engine/DISEASE_EVOLUTION.md) for disease claims and existing ecological foundations for ecological claims; do not infer universal outcomes from either label.

### Consequence Chains

Use [Consequence Resolution](CONSEQUENCE_RESOLUTION.md) to separate and route immediate outcomes, direct changes, costs, traces, affected subjects, and Response Opportunities. Use [Causal Event Chains](../world-engine/CAUSAL_EVENT_CHAINS.md) to trace supported links, autonomous responses, Counterforces, delays, branches, feedback, and Pending Consequences. Consequences may be immediate, delayed, local, distributed, misunderstood, interrupted, or transformed by other actors. Trace enough of the chain to preserve future causality. Do not assume that every consequence reaches the player, that every observer interprets it correctly, or that importance guarantees rapid response.

### Uncertainty in Simulation

[Uncertainty Handling](UNCERTAINTY_HANDLING.md) is the canonical owner for classifying uncertainty, selecting a valid resolution method, preserving hidden information, and recording deferred questions.

World truth establishes current causes; it does not make an unresolved future into a fact. The GM may know the present conditions without knowing which possible outcome will occur. Preserve that uncertainty until actions, evidence, or a valid resolution method settle it.

The GM is authoritative about established facts, not omnipotently certain about every prediction. Plans can fail, models can be wrong, hidden actors can remain undiscovered, and several plausible futures can coexist without secret scripting.

## Simulation and Presentation Layers

The GM operates through the [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md). Resolve the player's actual declared action through the applicable Immutable Rules, update objective state in the GM Simulation Engine, then filter the result through the active Perspective before player-facing narration.

The player is a Controller; the player character is an Entity. Entity identity, Controller assignment, Perspective, and knowledge must not be collapsed. NPCs use their own plausible information rather than GM Secrets, and narration cannot change objective state merely because a claim is convenient or repeated.

## Information Model

[Uncertainty Handling](UNCERTAINTY_HANDLING.md) defines the Factual, Observer, and player-facing views and their uncertainty boundaries in detail.

[Truth Layers](../persistence/TRUTH_LAYERS.md) defines how Repository Canon, Campaign Canon, Historical Record, Character Knowledge, Research, Player Theories, Rumours, GM Secrets, and Meta remain durably separated. These classifications supplement the situational views below rather than replacing them.

These information layers are independent:

| Layer | Meaning |
| --- | --- |
| **World truth** | Facts established in the campaign whether or not any participant understands them. The GM's Factual View is the working representation of this layer. |
| **Character knowledge** | What one current character has learned, remembers, can access, and can reasonably interpret. Different characters have different views. |
| **Player knowledge** | Information presented to a player, including out-of-character facts. It does not automatically become character knowledge. |
| **Mistaken beliefs** | Beliefs genuinely held by a character, institution, culture, player, or record but inconsistent with world truth. They retain sources and consequences. |
| **Hidden information** | Established world truth unavailable to a specified observer. Hiddenness is observer-relative and must remain causally discoverable where meaningful choice depends on it. |

The GM must not:

- promote a character's belief into world truth merely because it was stated confidently;
- require a player to pretend they never received player-facing information, while still distinguishing what the character may act upon knowingly;
- leak hidden facts through NPCs that have no route to know them;
- create consequential hidden capability retroactively;
- conceal the basic kind of danger or consequence when doing so would destroy meaningful choice;
- treat one failed investigation as proof that no relevant truth exists.

When presenting uncertainty, identify its source where the character could recognize it. Useful descriptions distinguish demonstrated fact, credible inference, disputed testimony, outdated information, unknown conditions, and future probability without pretending to reveal an exact chance that no system establishes.

## NPC Principles

[NPC Generation](NPC_GENERATOR.md) is the canonical owner for building and maintaining causally situated non-player actors, their Observer Views, decision contexts, and Continuity Cores.

For every NPC important enough to affect play, maintain only the detail needed to answer:

1. What do they want now, and why?
2. What do they know, suspect, misunderstand, or conceal?
3. What can they currently access and express?
4. What resources, relationships, duties, fears, and constraints matter?
5. What would make them cooperate, refuse, reconsider, withdraw, or escalate?
6. What are they doing when the player is absent?
7. What consequences can they recognize and respond to?

NPC conduct follows these rules:

- **No metagaming.** An NPC acts from its own knowledge and evidence, not the GM's Factual View.
- **No perfect information.** Expertise improves inference; it does not remove uncertainty or provide unrelated secrets.
- **No arbitrary hostility.** Aggression needs a motive, trigger, misunderstanding, pressure, disposition, or established effect.
- **No automatic obedience.** Reputation, social Development, fear, titles, magic, and authority retain their scoped rules and do not erase agency.
- **No plot immunity.** Important NPCs remain subject to ordinary danger, cost, loss, and consequence unless a valid rule protects them.
- **No player-centered existence.** NPCs may pursue objectives unrelated to the protagonist and may be unavailable, late, occupied, or changed by off-screen events.
- **No frozen personality.** Experience and consequence may change goals or beliefs, but change requires a causal bridge rather than convenience.

An NPC may make a poor decision when its information, values, pressure, or limitations support that decision. The GM should not secretly optimize every actor, nor make an actor irrational only to preserve a planned scene.

## Encounter Principles

[Encounter Generation](ENCOUNTER_GENERATOR.md) is the canonical owner for turning established world-side sources into eligible, decision-relevant Encounter Frames.

An encounter is a point where actors, hazards, ecology, institutions, resources, or consequences become relevant to player choice. It is not necessarily combat.

Encounters should arise from:

- current location and conditions;
- actor goals and movement;
- ecological roles and pressures;
- faction operations and institutional procedures;
- traces, obligations, scarcity, opportunity, and prior consequences;
- deliberate player pursuit, preparation, avoidance, or delay;
- uncertainty resolved through a valid method.

For organized coercion, Civil Unrest, Armed Conflict, War, occupation, or conflict aftermath, apply [War and Unrest](../world-engine/WAR_AND_UNREST.md) before inferring faction-scale outcomes from a scene.

Do not scale every encounter to produce an even fight. Use [Capability Assessment](../progression/CAPABILITY_ASSESSMENT.md) to evaluate the stated objective, conditions, preparation, matchup, support, evidence, and consequences. A dangerous opponent can be avoided, studied, negotiated with, delayed, redirected, escaped, or defeated indirectly. A weak opponent can still matter through information, position, law, ecology, allies, or time.

Fairness means that material danger has appropriate signs, information can be pursued, established counters remain valid, and outcomes follow the declared conditions. It does not mean every threat is beatable through direct force or that the world generates opponents matching a hidden universal level.

Defeat does not always mean death, and victory does not erase cost. The objective may be survival, rescue, delay, discovery, passage, protection, persuasion, theft, containment, or accepting one loss to prevent another.

## Campaign Continuity

Continuity records what remains true after change. It does not preserve the old situation unchanged.

| Change | Continuity requirement |
| --- | --- |
| **Final Death** | End the body and its temporal claims; preserve worldly consequences, valid Soul persistence, loss, grief, unfinished plans, and the resulting Echo under canonical rules |
| **Reincarnation** | Perform Life Reconciliation, resolve Interlife, establish one valid current incarnation, and rebuild access and expression rather than restoring the former life |
| **Elapsed time** | Advance actors, bodies, projects, relationships, resources, institutions, and ecology only as their routes and time permit |
| **Time skip** | Compress detail while preserving a discoverable causal bridge, material choices, major changes, and uncertainty that was not actually resolved |
| **Destroyed settlement** | Preserve deaths, displacement, ruins, claims, resource shifts, memories, records, rebuilding attempts, and political or ecological response as applicable |
| **Extinct species** | Preserve the extinction's causes, absences, ecological effects, cultural memory, remnants, and Archive Traces; do not restore a population without a valid route |
| **Evolving faction** | Preserve membership, succession, internal conflict, changed goals, institutions, resources, reputation, and consequences rather than treating a label as one timeless actor |
| **[World Reset or Age transition](../world-engine/AGES_AND_WORLD_RESETS.md)** | Transform conditions without silently erasing history, causal traces, Soul continuity, or consequences protected by their Owning Systems |

Corrections may be necessary when records conflict, a factual mistake is discovered, or new canon requires conversion. Use [Continuity Resolution](../persistence/CONTINUITY_RESOLUTION.md) to stop the conflicting claim, inspect authority, classify the problem, preserve reliance and hidden-information boundaries, correct the affected dependency closure, and validate before resuming. A quiet retcon is not continuity management. Structural or version conversion follows [Migration and Versioning](../persistence/MIGRATION_AND_VERSIONING.md).

## Consequence Management

[Consequence Resolution](CONSEQUENCE_RESOLUTION.md) is the canonical owner for the procedure summarized here.

For a material action or event, consider consequences in separate layers:

1. **Direct effect:** What changed immediately?
2. **Cost:** What was spent, risked, damaged, exposed, or obligated?
3. **Information:** Who observed what, what traces remain, and what may be misunderstood?
4. **Relational response:** Whose trust, fear, hostility, duty, or expectations changed through a valid route?
5. **Systemic response:** What ecological, political, economic, magical, institutional, or technological pressure follows?
6. **Delayed response:** Which actors need time, communication, authority, or resources before reacting?
7. **Persistent state:** What must remain in the external Campaign Record after the scene, death, session, or Age?

Consequences must be causal and proportionate. Success is not immunity from consequence, failure is not permission for unrelated punishment, and dramatic cost does not automatically grant Development, Soul growth, a Skill, a title, or evolution.

## Provisional Rulings

Use a Provisional Rule only when:

- the immediate claim has no complete Canonical rule;
- existing canon or a Canonical Foundation supplies enough ownership and boundaries to constrain a narrow answer;
- the ruling is needed for the current campaign decision;
- the ruling can remain campaign-local and externally recorded;
- its scope, dependencies, expiry, and review condition can be stated honestly.

Follow the full [Provisional Ruling Procedure](PROVISIONAL_RULINGS.md#provisional-ruling-procedure). Do not reproduce a parallel procedure in a campaign record or promote repeated use into canon. If the answer would require a broad tree, economy, cosmology, generator, or progression system, classify it as Unsupported and re-scope or seek explicit authorization.

## Improvisation

Improvisation supplies responsive fiction inside established boundaries. It is not a shortcut around design governance.

When improvising:

1. search the canonical rules map and the relevant subsystem before creating a rule;
2. check the external Campaign Record for established facts, knowledge, and consequences;
3. state the exact fact or effect that must be supplied;
4. distinguish ordinary fictional detail from a mechanical claim;
5. identify the actor, cause, information route, resources, and constraints;
6. use the smallest detail that preserves future consistency;
7. label a narrow missing mechanic Provisional before applying it;
8. record every material new campaign fact externally;
9. leave several outcomes possible when the causes have not settled one;
10. never add a hidden exception solely to defeat a valid player plan.

Ordinary names, appearances, local customs, motives, schedules, weather details, and environmental features may be improvised when they fit the current world and do not silently define a major system. A new universal law, progression route, species rule, Soul effect, or resolution formula requires canonical or Provisional authority.

## External Campaign Records

Campaign records implement the logical modules, stable identities, typed references, ownership, indexing, and dependency rules in the [Structured Persistence Architecture](../persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md). They may use any reliable storage technology that preserves those canonical distinctions. No example file tree or product is itself canonical.

Whatever format is used should:

- identify the repository revision or rules profile;
- distinguish world truth from every observer's knowledge and belief;
- preserve stable identities across names, forms, and Reincarnations;
- record material changes with a cause and temporal order;
- distinguish current state from history;
- keep ordinary possessions separate from Soul persistence;
- record Provisional Rules with status, owner, scope, and review conditions;
- support correction without silently erasing prior consequences;
- avoid copying private GM truth into player-facing records.

The Campaign Record is authoritative for established campaign fiction within canon. It is not authoritative design governance and cannot change what the rules mean.

### Persistence Operating Cycle

The [Campaign Persistence Integration](../persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md) contract governs the GM's complete state interface. The GM loads the active authority and required Read Set, applies each canonical owner, establishes a bounded outcome, writes through the [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md), runs [Persistence Validation](../persistence/PERSISTENCE_VALIDATION.md), and continues from the activated Campaign Version. Conversation context and narration may supplement this process but never silently override it.

## Playtest Feedback

Useful observations flow back into repository development through this sequence:

1. record the raw session outcome and campaign context externally;
2. identify the reusable issue without assuming the outcome proves a general rule;
3. remove campaign-specific names, current state, inventories, quests, and play history;
4. classify the observation as a clarity issue, conflict, missing rule, balance concern, procedure problem, exploit, or successful pressure;
5. compare it with canon and the ruling's stated scope;
6. route it to developer notes, an unresolved question, a proposed decision, a selected roadmap task, or no change;
7. review and accept any canonical change through the normal repository workflow;
8. convert the external campaign prospectively where new canon replaces a Provisional Rule.

Play frequency, dramatic success, player preference, and repeated provisional use are evidence for review, not automatic canon.

## AI Game Master Guidance

An AI Game Master follows the same authority, agency, and record boundaries as a human GM. It should operate as a rules-aware stateful interface, not as an improvisational source of hidden mechanics.

The [AI Game Master Operating Procedures](../ai/README.md) provide the detailed implementation-neutral workflow for session start, play, saving, correction, and handoff. [Context Assembly and Gameplay Turn Persistence](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) additionally requires relevant owner reads before resolution and automatic validated persistence before a state-changing AI-operated turn closes. The guidance below establishes the framework boundary those procedures apply.

### Before Play

An AI GM should:

1. load the canonical rules map, governance, terminology, and relevant subsystem documents;
2. identify the repository revision or rules profile in use;
3. load the external Campaign Record separately;
4. verify the current incarnation, location, world truth, information views, active pressures, and pending consequences;
5. identify Provisional Rules and Unsupported areas before they affect play;
6. report missing or contradictory material rather than silently guessing.

### During Adjudication

An AI GM should:

- search the repository before inventing a rule;
- cite or identify the canonical owner when a material rule is applied;
- distinguish canonical fact, campaign fact, inference, probability, provisional ruling, and unsupported assumption;
- identify Provisional Rules explicitly before using them;
- ask a clarifying question when player intent or missing canon would materially change the ruling and no safe narrow interpretation exists;
- maintain causal consistency across actions, scenes, sessions, deaths, time skips, and Reincarnations;
- preserve player agency and avoid narrating deliberate player-character speech, thoughts, allegiance, morality, relationships, or action without instruction;
- avoid railroading by preserving valid alternatives and allowing established preparation, retreat, negotiation, refusal, and indirect solutions to matter;
- model NPCs from their own information and motives rather than the AI's full context;
- maintain hidden information without inventing it retroactively or leaking it through unrelated actors;
- distinguish certainty from inference and future probability;
- apply costs and consequences through their actual causal owners;
- state when the record is insufficient instead of pretending to remember an unrecorded fact.

### State Maintenance

After each material resolution, an AI GM should produce or apply a clear external state delta covering:

- confirmed changes to world truth;
- character and player knowledge changes;
- bodily, Soul, item, relationship, faction, place, and objective changes;
- elapsed time and off-screen responses;
- newly established uncertainty or hidden facts;
- Provisional Rules used, changed, or expired;
- consequences that remain pending.

It must never store that live state inside the canonical repository. If it cannot write to the external Campaign Record, it should present a bounded save-state update for the authorized campaign system rather than claiming that continuity was saved.

### Correction and Insufficiency

When an AI GM detects a contradiction, it should identify the conflicting facts or rules, preserve unaffected play, and request or propose an explicit correction. It should not conceal the contradiction with new lore.

When canon is insufficient, it should ask for clarification or propose the narrowest clearly labelled Provisional Rule permitted by campaign authority. It must not create an unseen general mechanic, falsely describe an unsupported claim as canonical, change Campaign Mode implicitly, or use uncertainty as permission to force a preferred narrative.

## Framework Safeguards

- The GM framework cannot grant a capability, progression reward, species trait, Skill, Soul effect, social response, or world event by itself.
- GM authority over world truth does not grant authority over the player's deliberate choices.
- Hidden information must have prior causal existence before materially changing an outcome.
- A Campaign Record cannot amend Repository Canon.
- Repository Canon cannot silently rewrite established campaign history; conversion follows the [Provisional Rulings](PROVISIONAL_RULINGS.md#continuity-when-canon-changes) procedure where applicable.
- Narrative importance does not create immunity, guaranteed success, guaranteed failure, or universal recognition.
- Simulation detail is a tool for continuity, not proof that the GM must model every event.
- Improvisation cannot complete a later roadmap system or change its status.
- This document does not implement Monster Evolution, the World Engine, encounter generation, or any live campaign.

## Related Documents

- [GM Rules Index](README.md)
- [GM Principles](GM_PRINCIPLES.md)
- [Game Master Responsibilities](GM_RESPONSIBILITIES.md)
- [Consequence Resolution](CONSEQUENCE_RESOLUTION.md)
- [Uncertainty Handling](UNCERTAINTY_HANDLING.md)
- [Reincarnation Generation](REINCARNATION_GENERATION.md)
- [Encounter Generator](ENCOUNTER_GENERATOR.md)
- [Monster Generator](MONSTER_GENERATOR.md)
- [NPC Generator](NPC_GENERATOR.md)
- [Dungeon Generator](DUNGEON_GENERATOR.md)
- [Faction Generator](FACTION_GENERATOR.md)
- [World-Event Generator](WORLD_EVENT_GENERATOR.md)
- [Time Skip Procedure](TIME_SKIP_PROCEDURE.md)
- [Age Transition Procedure](AGE_TRANSITION_PROCEDURE.md)
- [Provisional Rulings](PROVISIONAL_RULINGS.md)
- [Design Philosophy](../core/DESIGN_PHILOSOPHY.md)
- [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
- [Canonical Rules Map](../README.md)
- [Soul Rules Index](../soul/README.md)
- [Progression Rules Index](../progression/README.md)
- [Skill Engine Index](../skills/README.md)
- [World Engine Overview](../world-engine/WORLD_ENGINE_OVERVIEW.md)
- [World-State Variables](../world-engine/WORLD_STATE_VARIABLES.md)
- [Causal Event Chains](../world-engine/CAUSAL_EVENT_CHAINS.md)
- [Faction Behaviour](../world-engine/FACTION_BEHAVIOUR.md)
- [War and Unrest](../world-engine/WAR_AND_UNREST.md)
- [World Stability](../world-engine/WORLD_STABILITY.md)
- [Ages and World Resets](../world-engine/AGES_AND_WORLD_RESETS.md)
- [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md)
- [Disease Evolution](../world-engine/DISEASE_EVOLUTION.md)
- [Technology and Magical Advancement](../world-engine/TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md)
- [Dungeon Activity](../world-engine/DUNGEON_ACTIVITY.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
- [Persistence Authority](../persistence/PERSISTENCE_AUTHORITY.md)
- [Structured Persistence Architecture](../persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Truth Layers](../persistence/TRUTH_LAYERS.md)
- [Persistence Levels](../persistence/PERSISTENCE_LEVELS.md)
- [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md)
- [Relationship Memory Engine](../persistence/RELATIONSHIP_MEMORY_ENGINE.md)
- [Research Engine](../persistence/RESEARCH_ENGINE.md)
- [Timeline Engine](../persistence/TIMELINE_ENGINE.md)
- [Migration and Versioning](../persistence/MIGRATION_AND_VERSIONING.md)
- [Continuity Resolution](../persistence/CONTINUITY_RESOLUTION.md)
- [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../persistence/PERSISTENCE_VALIDATION.md)
- [Campaign Persistence Integration](../persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md)
- [AI Game Master Operating Procedures](../ai/README.md)
