# Simulation Abstraction

## Purpose

This document defines how the World Engine changes simulation detail without changing world truth.

It provides a playable procedure for:

- choosing an appropriate resolution for one question;
- compressing repetitive or distant activity;
- expanding detail when a decision becomes immediate;
- advancing days, generations, or centuries without scripting outcomes;
- preserving autonomous actors, material exceptions, uncertainty, and specialist ownership;
- returning from a Time Skip with a causal bridge to the new present.

It is not an event generator, campaign timeline, prediction engine, encounter scaler, or substitute for GM judgment. Current Simulation Frames, changes, actors, intervals, and records remain external campaign state.

## Ownership

| Claim | Primary owner |
| --- | --- |
| Current world facts and scoped changing conditions | [World-State Variables](WORLD_STATE_VARIABLES.md) and the relevant World Engine domain |
| Causal propagation, branches, Counterforces, Pending Consequences, and Causal Horizons | [Causal Event Chains](CAUSAL_EVENT_CHAINS.md) |
| Choice of representational detail and movement between resolutions | This document |
| Time Skips, Age Claims, Age Transitions, World Resets, and World Revalidation | [Ages and World Resets](AGES_AND_WORLD_RESETS.md) |
| Campaign operation, information views, continuity, and provisional rulings | [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md) |
| Personal capability, Soul, Skill, Evolution, Class, Soul Weapon, and Magic outcomes | Their established specialist systems |
| Current Simulation Frames, world values, actors, decisions, deltas, and timelines | External Campaign Record |

Abstraction changes how much established causality is represented. It cannot take ownership of any effect merely because intermediate detail is omitted.

## Core Rule

Use the coarsest resolution that preserves every fact, choice, dependency, exception, and uncertainty capable of changing the current decision or future continuity.

Changing resolution may summarize or reveal detail. It cannot:

- create a cause;
- remove a consequence;
- decide an unresolved branch by convenience;
- give an actor knowledge, intent, capability, authority, or resources;
- average away a materially different population or condition;
- grant specialist progression;
- make the player central to off-screen events;
- turn elapsed time into an event.

The world remains one causal history at every resolution.

## Terms

### Simulation Question

The exact decision, interval, continuity claim, or future dependency the simulation must resolve.

Good questions are bounded:

- Does the settlement retain winter food access after the bridge fails?
- What materially changes during the soul's two-century Interlife?
- Which factions can respond before the Gate closes?
- What condition does the old Dungeon have when play returns?

"What happens everywhere?" is not a usable Simulation Question.

### Simulation Frame

A **Simulation Frame** states:

- the Simulation Question;
- the subjects and geographic or relational scope;
- the start state and observation time;
- the interval or Causal Horizon being considered;
- the selected Simulation Resolution;
- the relevant owners and information views;
- the Resolution Anchors and Material Exceptions that must be preserved;
- the next Simulation Review Point.

One campaign may use several overlapping Frames. A war, a disease outbreak, one reincarnating soul, and a distant Gate can be tracked at different resolutions without creating separate worlds.

### Simulation Resolution

**Simulation Resolution** is the amount and kind of causal detail represented for one Simulation Frame.

The canonical levels are:

1. Focused;
2. Local;
3. Regional;
4. Epochal.

They are descriptive levels, not universal time bands, map sizes, danger ranks, or complexity scores.

### Resolution Anchor

A **Resolution Anchor** is an established fact, actor, dependency, branch, threshold, obligation, or consequence that must remain explicit when detail is compressed because losing it could change a later decision or invalidate continuity.

Examples include:

- a bridge that is the only winter supply route;
- a dissenting faction able to prevent mobilization;
- one surviving breeding population;
- a Pending Consequence due during the interval;
- a Gate Channel whose closure would strand travelers;
- a living player's unresolved decision;
- a source whose consent sustains magical infrastructure.

### Material Exception

A **Material Exception** is a subject or condition that differs from its aggregate enough to change a conclusion, duty, risk, recovery route, or future branch.

A small minority, rare monster, isolated district, hidden reserve, exceptional specialist, unusual host population, unique Soul Weapon, or damaged endpoint may be a Material Exception. Rarity alone is insufficient; the difference must matter to the Simulation Question or continuity.

### Simulation Review Point

A **Simulation Review Point** is the next time or condition at which the current abstraction may no longer be sufficient.

Review Points are caused by material changes, not arbitrary calendar ticks. They may occur when:

- a Pending Consequence becomes due;
- an actor receives decisive information or can act;
- a Stock, Capacity, route, source, or institution approaches a relevant threshold;
- a season, migration, maturation, succession, election, harvest, disease wave, or maintenance cycle changes conditions;
- a conflict escalates, pauses, fragments, or ends;
- a project reaches validation, adoption, failure, or dependency;
- a Dungeon changes Activity Regime;
- a Gate Channel, Basis, or Closure condition changes;
- a possible Final Death, Reincarnation, Awakening, Evolution, or Reset requires its owner;
- play returns to the affected scope.

### Simulation Pass

A **Simulation Pass** advances one Frame from its current state to one Review Point while resolving only the causal detail required at the selected resolution.

### Simulation Delta

A **Simulation Delta** is the external record of material changes produced by one or more Simulation Passes. It preserves causes, actor choices, owner handoffs, changed conditions, uncertainty, Pending Consequences, legacies, and the next Review Point without reproducing every omitted event.

## Three Independent Choices

Do not infer resolution from map size or elapsed time alone. Select three things separately.

### Scope

Scope identifies the subjects, places, networks, relationships, and World Engine domains relevant to the question.

A scope may be:

- one person and an approaching hazard;
- a settlement and its food network;
- a migratory corridor shared by several species;
- an empire and its contested provinces;
- several Contact Domains linked by one World Gate;
- a world-scale Reset Footprint.

Scope can be geographically small but causally broad, or geographically broad but concerned with one narrow dependency.

### Interval and Causal Horizon

The interval states how much elapsed time is being considered. The [Causal Horizon](CAUSAL_EVENT_CHAINS.md#causal-horizon) states how far consequences must currently be traced.

A century-long interval may require only a narrow horizon for one durable ruin. A ten-minute crisis may require a broad horizon when it threatens a Gate, source, army, and city. Neither duration determines detail by itself.

### Resolution

Resolution states which facts remain individually represented and which may be summarized. It is chosen from the four canonical levels below and may change whenever a Review Point makes the current level insufficient or wasteful.

## Canonical Resolution Levels

### Focused Resolution

Focused Resolution represents the actors, sequence, timing, access, choices, and immediate conditions needed for one contested decision or sensitive transition.

Preserve:

- individual actors whose decisions matter;
- action order when order can change the result;
- direct effects, costs, interruption, consent, and failure;
- immediate evidence and information differences;
- exact specialist handoffs;
- player choice before deliberate player-character action;
- short delays capable of changing the outcome.

Typical uses include scenes, rescue attempts, negotiations, Final Death review, a Gate crossing, a Ritual interruption, an Evolution checkpoint, or the moment a threshold is crossed.

Focused does not mean combat-only, present-player-only, or necessarily brief. One delicate research project may use Focused Resolution at several decisive checkpoints across years while routine work between them remains compressed.

### Local Resolution

Local Resolution represents a bounded community, site, route, habitat, institution, project, or linked group through its material actors and relationships.

Preserve:

- the principal groups and decision routes;
- access to resources, infrastructure, information, and authority;
- bottlenecks, dependencies, dissent, and unequal distribution;
- representative cohorts or processes;
- Material Exceptions;
- changes that may force Focused Resolution.

Typical uses include a settlement winter, a Dungeon and nearby communities, one faction branch, a school, a migration route, a local outbreak, or one Gate endpoint.

Local Resolution may summarize repeated meals, patrols, lessons, transactions, or maintenance. It cannot assume that every resident, member, host, or worker experiences the aggregate result.

### Regional Resolution

Regional Resolution represents connected populations, territories, ecosystems, markets, institutions, wars, infrastructure networks, or contact systems through aggregate patterns and decisive substructures.

Preserve:

- major flows and network dependencies;
- materially distinct populations, territories, factions, and institutions;
- concentration of capability or control;
- exceptional actors capable of redirecting a branch;
- thresholds, feedback, lags, and path dependence;
- disputed information and competing strategies;
- local Frames that remain unresolved or materially different.

Typical uses include a kingdom-scale shortage, a war theater, regional disease spread, technology diffusion, ecological succession, or sustained exchange through a World Gate.

Regional Resolution does not turn a region into one actor, average culture, unified economy, technology level, or power score.

### Epochal Resolution

Epochal Resolution represents structural tendencies, transformations, survivorship, replacement, institutional succession, and World Legacies across generations or Ages.

Preserve:

- decisive causes and turning points;
- institutions, populations, ecologies, sources, and infrastructures whose continuity matters;
- succession, reproduction, maturation, maintenance, and knowledge transmission;
- Material Exceptions whose survival or action changes the later world;
- irreversible loss and incomplete recovery;
- unresolved branches that remain material;
- Age Boundaries, Reset Mechanisms, Gate Legacies, and specialist persistence under their owners;
- enough discoverable history to connect the start and end state.

Typical uses include a long Interlife, centuries of ecological and political change, the rise and decline of infrastructure, an Age Transition, or recovery after a World Reset.

Epochal Resolution is not permission to declare that empires inevitably rise, species naturally improve, technology always advances, Magic becomes stronger, or the world resets itself. Long duration supplies opportunities and losses, not a predetermined direction.

## Resolution Is Not a Ladder of Importance

The four levels do not rank events or beings.

- A quiet conversation may require Focused Resolution because one consent decision matters.
- A world war may remain Regional while play is elsewhere.
- A centuries-old Soul Weapon may be a Local Material Exception within an Epochal interval.
- A protagonist may be summarized at Local Resolution during routine travel while a distant harvest failure is resolved more closely.

Moving toward Focused Resolution adds represented detail. It does not make an event more real. Moving toward Epochal Resolution removes represented detail. It does not make individuals irrelevant.

## Choosing Resolution

Start at the coarsest level likely to answer the Simulation Question, then test whether it preserves every material feature.

### Expand When

Use finer resolution when:

- a player decision can still redirect the outcome;
- an autonomous actor has several materially different supported choices;
- order, timing, consent, interruption, or individual capability decides the claim;
- a Material Exception contradicts the aggregate tendency;
- a threshold or irreversible transition is reached;
- a specialist owner must determine Final Death, Reincarnation, Development, Skill, Evolution, Class, Soul Weapon, or Magic change;
- evidence is too uncertain to justify aggregation;
- a failure would create an unfair surprise without closer signs or opportunity;
- play enters or returns to the affected scope.

### Compress When

Use coarser resolution when:

- repeated processes have sufficiently similar causes, constraints, and outcomes;
- routine maintenance or recovery follows established routes;
- individual variation cannot change the current conclusion;
- branches have already resolved or can remain explicitly Pending;
- a representative flow or cohort preserves the material distribution;
- omitted detail would not change agency, ownership, consequence, or future continuity.

### Keep the Current Resolution When

Do not change resolution merely because:

- the scene became dramatic;
- the player left the area;
- a fixed amount of time elapsed;
- a named character appeared;
- the GM wants a particular outcome;
- a precise number would feel more objective;
- the world event is large.

## Mixed Resolution

One simulation may use several levels at once.

For example, during a regional war:

- the front remains Regional;
- one besieged city is Local;
- a player's negotiation over opening its gates is Focused;
- the century-long origin and aftermath of the conflict may be summarized Epochally.

Use separate linked Frames when their actors, owners, intervals, or Review Points differ. Exchange only established changes between them. A Focused victory may alter Regional conditions without deciding the entire war, while a Regional supply collapse may constrain a Focused battle without scripting its exact actions.

No campaign needs one global current resolution.

## Compression Rules

Compression is a representation operation, not a cause.

Before compressing:

1. state the Simulation Question and current Frame;
2. identify established start conditions;
3. list Resolution Anchors and Material Exceptions;
4. identify active actors, processes, branches, thresholds, and Pending Consequences;
5. identify specialist claims that may require handoff;
6. choose the next Review Point.

Compression may:

- summarize repeated links with the same relevant cause and receiving owner;
- use representative cohorts, flows, projects, or institutions;
- group routine choices where alternatives do not change the result;
- record a trend and its sustaining drivers instead of every fluctuation;
- omit nonmaterial names, conversations, transactions, and incidents;
- carry an uncertain branch forward with an explicit review condition.

Compression may not:

- treat a likely outcome as completed;
- assume perfect coordination or information;
- merge actors whose interests can materially diverge;
- replace distribution with an average that changes the conclusion;
- skip a decision the player retained authority to make;
- hide a threshold crossing or irreversible transition;
- invent progress, adaptation, recovery, decay, or collapse from elapsed time alone;
- erase the cause of a later fact.

## Expansion Rules

Expansion restores the detail needed for a new question from established state, anchors, deltas, and unresolved branches.

When expanding:

1. load the latest relevant Simulation Delta and current world state;
2. preserve every established outcome and Material Exception;
3. identify which previously aggregated actors or processes now matter individually;
4. restore only detail supported by prior causes, current evidence, and owner rules;
5. preserve information differences and hidden facts;
6. expose uncertainty where prior compression did not settle it;
7. establish the next Review Point at the finer resolution.

The GM may add previously immaterial texture, minor actors, ordinary incidents, and local variation when they are consistent with established history. Such elaboration cannot:

- supply a missing cause for an established outcome;
- retroactively invalidate a meaningful choice;
- create an unforeshadowed decisive capability or authority;
- erase a recorded absence, survivor, cost, or obligation;
- convert unspecified history into a trap selected for current convenience.

If material detail should have been preserved but was not, treat it as a record insufficiency or correction. Do not conceal the problem with retroactive lore.

## Review-Point Simulation

Long intervals are divided by causal Review Points rather than equal turns.

A stable decade may need one pass. A week containing a Gate breach, harvest failure, succession crisis, and outbreak may need several. Review frequency follows changing causes and choices, not a universal calendar.

At each Review Point:

1. resolve due direct effects and Pending Consequences;
2. update information available to actors;
3. let autonomous actors choose through their actual motives, capability, access, and decision routes;
4. apply Counterforces, delays, maintenance, depletion, recovery, and feedback;
5. hand specialist claims to their owners;
6. update affected World-State Variables once;
7. preserve distribution, Material Exceptions, uncertainty, and legacies;
8. decide whether the current Resolution remains sufficient;
9. set the next Review Point or stop at the Causal Horizon.

Do not resolve beyond the current horizon merely to prepare a dramatic reveal. Future branches remain future branches.

## Actor Agency During Compression

### Autonomous Actors

NPCs, factions, institutions, sources, monsters, populations, and other agentive beings continue through their own information, motives, commitments, procedures, relationships, and constraints.

Compression does not grant them:

- GM knowledge;
- optimal strategy;
- unanimous coordination;
- unlimited attention or resources;
- immunity from succession, disagreement, error, or refusal.

Representative actor decisions may be summarized only when the omitted variation cannot change the Simulation Question. Preserve dissenting or exceptional actors when they can redirect the result.

### Player Agency Checkpoints

The GM cannot compress through a deliberate player-character choice that could materially change the interval unless the player has already supplied a standing intention, plan, delegation, or acceptable range of conduct.

When such a choice becomes due:

- pause at a Review Point and return to play;
- ask for the player's decision;
- or apply the player's established instruction within its stated limits.

The GM may resolve involuntary exposure, ordinary bodily needs, previously authorized routine, and external events through their owners. It may not invent the player's allegiance, sacrifice, relationship, moral judgment, major project, training objective, Gate crossing, or acceptance of a Reincarnation candidate.

### Delegation and Standing Plans

A character may delegate authority or establish a standing plan before a Time Skip. The plan remains bounded by:

- what the character and delegates know;
- actual authority and consent;
- available capability and resources;
- interruption and changed conditions;
- explicit decision points that exceed the delegation.

A standing plan permits action; it does not guarantee success or make the character present everywhere.

## Simulation Pass Procedure

For each Simulation Pass:

1. **State the question.** Name the decision, interval, or continuity claim being resolved.
2. **Define the Frame.** Set scope, start state, interval, resolution, owners, evidence, and uncertainty.
3. **Preserve anchors.** List Resolution Anchors, Material Exceptions, open branches, and Pending Consequences.
4. **Choose the Review Point.** Select the earliest material time or condition within the interval.
5. **Resolve due direct effects.** Apply established events and owner results before downstream inference.
6. **Advance processes.** Apply only time-supported flows, maintenance, depletion, recovery, maturation, transmission, travel, and other processes.
7. **Resolve actor responses.** Use current information, motives, capacity, authority, alternatives, opposition, and response delay.
8. **Apply Counterforces and feedback.** Re-evaluate changed conditions rather than extrapolating a loop indefinitely.
9. **Hand off specialist claims.** Pause or use the appropriate owner when a personal or system-specific transition becomes material.
10. **Resolve supported branches.** Use established facts, actor choices, owner procedures, or an authorized uncertainty method; preserve unresolved futures.
11. **Update state once.** Record each changed condition under its owner with distribution, evidence, and uncertainty.
12. **Create the Simulation Delta.** Preserve causes, changes, choices, anchors, exceptions, legacies, and Pending Consequences externally.
13. **Reassess resolution.** Expand, compress, split, merge, or retain Frames according to the next question.
14. **Stop or continue.** End at the Causal Horizon or begin another pass at the next Review Point.

The procedure may be brief in play. A stable Local pass may need only a few sentences. Complexity is added only when a material claim requires it.

## Long-Horizon and Century Simulation

Centuries are resolved as a sequence of unequal causal intervals, not hundreds of annual turns and not one unsupported summary.

### Start with Continuities and Pressures

Identify what can plausibly persist or continue:

- populations and reproductive or replacement routes;
- resources, renewal, depletion, and supply dependencies;
- ecosystems, migration routes, disturbance, and succession;
- factions, institutions, offices, records, and succession procedures;
- technical and magical infrastructure, maintenance, source relationships, and knowledge transmission;
- conflicts, obligations, borders, law, belief, and social memory;
- Dungeons, World Gates, Reset mechanisms, and other sustained structures;
- protected Soul, Weapon Soul, and Archive continuity under their owners.

Persistence is a claim with a basis. Age, fame, importance, or previous existence does not make a structure self-maintaining.

### Select Turning Points Causally

Advance to the earliest Review Point capable of changing the trend. Typical turning points include succession, demographic replacement, resource exhaustion, climate shifts, disease waves, adoption thresholds, infrastructure failure, institutional schism, conflict cessation, Gate Closure, and world-transition mechanisms.

Do not begin with a desired later Age and invent convenient turning points backward. The successor state emerges from passes through established causes and choices.

### Preserve Generational Processes

Epochal compression must account for:

- births, deaths, maturation, care, and migration;
- leadership succession and institutional memory;
- teaching, tacit knowledge, copying error, preservation, and loss;
- maintenance, replacement, salvage, and incompatible infrastructure;
- adaptation and selection through the appropriate ecology, Development, Skill, or Evolution owner;
- changing languages, beliefs, laws, identities, and relationships through actual social routes;
- accumulation of damage, obligations, residue, and World Legacies.

Elapsed generations do not guarantee improvement, decline, complexity, unity, or moral progress.

### Return a Causal Bridge

At the end of a long interval, establish:

- the new present conditions relevant to play;
- the decisive changes and their causes;
- what persisted and why;
- what ended, transformed, or became inaccessible;
- surviving Material Exceptions;
- unresolved branches and Pending Consequences;
- what different observers know, believe, or misremember;
- discoverable records, ruins, absences, customs, descendants, and legacies.

The bridge should support investigation. It need not reveal all hidden history to the player.

## Domain Preservation Guide

This guide identifies what abstraction must preserve. It does not replace any domain's rules.

| Domain | May often be compressed as | Must remain explicit when material |
| --- | --- | --- |
| [Populations](POPULATIONS.md) | cohorts, demographic flows, dependency patterns, replacement | reproductive bottlenecks, distinct minorities, maturation lags, exceptional beings, local extirpation, extinction |
| [Resources and Food](RESOURCES_AND_FOOD.md) | supply routes, renewal and depletion trends, reserves, substitution | bottlenecks, seasonality, unequal access, quality, waste, single-source dependence, Food Security failure |
| [Economies](ECONOMIES.md) | production and allocation networks, exchange trends, obligations | Purchasing Access, currencies, concentration, coercion, unpaid costs, market fragmentation, critical infrastructure |
| [Ecology and Migration](ECOLOGY_AND_MIGRATION.md) | ranges, functions, succession, migration flows | keystone relationships, route barriers, invasive establishment, refuges, Novel Ecologies, irreversible loss |
| [Faction Behaviour](FACTION_BEHAVIOUR.md) | broad goals, mobilization, institutional continuity, relationships | internal dissent, decision routes, succession, key coalitions, legitimacy, actors capable of redirection |
| [War and Unrest](WAR_AND_UNREST.md) | theaters, campaigns, mobilization, control trends, cessation | decisive operations, civilian agency and harm, logistics, command conflict, territorial ambiguity, demobilization |
| [Disease Evolution](DISEASE_EVOLUTION.md) | transmission contexts, waves, host-state patterns, intervention trends | host differences, reservoirs, care access, agent change, vulnerable cohorts, uncertainty, public-health capacity |
| [Technology and Magical Advancement](TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md) | project stages, adoption, diffusion, maintenance, decline | inventors and institutions, validation failures, infrastructure, source agency, tacit Skill, lock-in, excluded users |
| [Dungeon Activity](DUNGEON_ACTIVITY.md) | Activity Regimes, access trends, ecology, extraction, maintenance | Sustaining Bases, agentive inhabitants, route changes, collapse, unique resources, Dungeon Legacies |
| [World Stability](WORLD_STABILITY.md) | scoped support and strain trends, response, forecasts | Stability Referent, Systemic Coupling, Buffers, displaced burdens, thresholds, affected groups, uncertainty |
| [Ages and World Resets](AGES_AND_WORLD_RESETS.md) | Age Signatures, transition arcs, uneven survivorship | Preconditions, Trigger, Mechanism, Footprint, actor intervention, World Revalidation, World Legacies |
| [World Gates](GATES_AND_WORLD_CONTACT.md) | Contact Phases, exchange patterns, Gate operation, adaptation | Gate Basis, channel differences, Contact Compatibility, autonomous actors, Closure, stranding, Gate Legacies |

## Specialist-System Handoffs

The World Engine supplies changed circumstances. Specialist systems determine their own outcomes.

### Souls and Reincarnation

During Interlife or another long absence, the world continues through Simulation Passes. Reincarnation candidate generation uses the world state that exists when the relevant stage is reached.

Compression does not:

- kill the current incarnation;
- declare Final Death;
- create or strengthen an Echo;
- add a Reincarnation candidate;
- heal a Soul Wound;
- grant Soul Depth, Resonance, a Title, Avatar emergence, or Archive access.

When a possible Final Death or cross-domain placement becomes material, expand enough to apply [Reincarnation](../soul/REINCARNATION.md) and [World Gate Soul Interactions](WORLD_GATE_SOUL_INTERACTIONS.md).

### Development and Skills

Training, work, experimentation, instruction, hardship, and use during a compressed interval require actual time allocation, access, embodiment, feedback, recovery, and the relevant [Development](../progression/README.md) or [Skill](../skills/README.md) procedure.

"Trained for ten years" is not a finished capability claim. Preserve objectives, methods, interruptions, Plateaus, maintenance, tradeoffs, and player agency where they can change the result.

### Monster Evolution

Ecological pressure and elapsed generations may change populations and create possible Evolution Routes. [Monster Evolution](../monster-evolution/README.md) still owns individual or lineage transitions, requirements, tradeoffs, and resulting forms.

Epochal compression cannot assume that monsters evolve upward, optimize perfectly, acquire a desired branch, or bypass hidden conditions.

### Human Structures

Classes, Professions, traditions, schools, institutions, offices, and recognition may spread, fragment, transform, or disappear through [Human systems](../human/README.md). A surviving label does not prove unchanged curriculum, capability, authority, membership, or legitimacy.

### Soul Weapons

Time, use, danger, custody, separation, or historical importance does not automatically awaken or evolve a Weapon Soul. Apply [Soul Weapon](../soul-weapons/README.md) personhood, consent, bond, continuity, damage, maintenance, and form rules at material Review Points.

### Magic

Mana, sources, Affinities, Spells, Rituals, Enchantments, Alchemy, Divine Magic, and restrictions remain with [Magic](../magic/README.md). Long operation must preserve supply, source response, maintenance, Drift, knowledge, infrastructure, failure, and local world-law compatibility.

No Epochal summary may turn widespread use into universal affinity, unlimited Mana, permanent source consent, or automatic magical advancement.

## Information and Uncertainty

Abstraction preserves the [GM information model](../gm/GAME_MASTER_FRAMEWORK.md#information-model).

Track separately:

- what became world truth during the interval;
- what remains genuinely unresolved because it is not yet due or material;
- what the GM has not specified because it remains immaterial;
- what actors observed;
- what actors inferred or believed;
- what the player knows;
- what evidence survives.

At the end of a pass, resolve historical branches only when the resulting state depends on them. A materially changed present needs a sufficient causal route, even if characters do not know it. An immaterial incident may remain unspecified until later elaboration.

Randomness may settle a supported uncertain branch through an authorized campaign method. It cannot supply missing causality, capability, consent, or authority.

## Time Skips

A [Time Skip](AGES_AND_WORLD_RESETS.md#time-skips-and-compression) changes narrative presentation. Simulation Passes change the world through elapsed processes and actor choices.

Before a Time Skip:

1. establish the start state and intended interval;
2. ask for player plans, limits, delegated authority, and desired interruption points;
3. identify active Frames, anchors, exceptions, branches, and Pending Consequences;
4. identify likely specialist checkpoints;
5. choose the first Review Point rather than assuming the end state.

During the skip, use as many or as few passes as material changes require. Pause when player agency or finer resolution becomes necessary.

After the skip, provide a causal bridge and update the external Campaign Record. Do not present the compressed interval as a list of unexplained facts or free rewards.

## External Simulation Records

A campaign may record a Simulation Frame or Delta in any reliable external format. Useful fields include:

```text
Simulation question:
Scope and interval:
Start state and evidence:
Resolution:
Relevant owners:
Resolution Anchors:
Material Exceptions:
Active actors and information:
Processes, drivers, and Counterforces:
Review Point:
Resolved changes and causes:
Specialist handoffs:
Unresolved branches and uncertainty:
Pending Consequences and legacies:
Information-view changes:
Next Frame or Review Point:
```

This is an external record interface, not a repository template. Record only what future play or continuity may need.

## Worked Examples

### Two Centuries of Interlife

A soul enters Interlife after a frontier war. The Simulation Question is what world conditions shape the next valid incarnation two centuries later.

The GM begins at Regional Resolution with anchors for a surviving monster refuge, a damaged magical road, two successor institutions, and a Pending Consequence involving soil depletion. Review Points occur at a refugee settlement's demographic replacement, the road's maintenance failure, a disease wave, and a faction schism. Epochal passes summarize ordinary generations while Local Frames preserve the refuge and road because each can change future candidate placement.

No annual turn is required. The resulting causal bridge identifies changed populations, lost infrastructure, new institutions, surviving ruins, disputed histories, and remaining uncertainty. Reincarnation then generates candidates from the current world through its own rules.

### War Zooms Into Negotiation

A war is tracked Regionally through supply, territorial control, civilian displacement, command coalitions, and Pending Consequences. A neutral city can open a route that changes both sides' logistics.

The city becomes a Local Frame. When the player enters negotiations with its divided council, the decision becomes Focused. The conversation resolves consent, authority, promises, threats, and information precisely. Its result returns to the Regional Frame as one changed route and several political obligations, not as automatic victory in the war.

### The Minority Hidden by an Average

A region's food supply appears adequate in aggregate. One island population lost access to the only grain route and cannot use the mainland's substitute crop.

The island is a Material Exception. Regional compression retains its distinct Food Security failure, transport bottleneck, and political neglect. The GM may summarize mainland recovery while resolving the island Locally. Averaging the supplies would have erased a population and changed later migration and unrest.

### A Century of Magical Lighting

A city adopts source-dependent magical lighting. Local passes resolve early validation, maintenance institutions, unequal district access, source negotiations, and replacement of older crafts. Regional passes later track diffusion to connected cities.

Decades of use do not make the system permanent. A succession dispute changes divine Mandates, a reagent route fails, and tacit maintenance knowledge concentrates in one school. Epochal resolution preserves those dependencies. When the school fragments, some districts retain adapted lights, others return to older methods, and abandoned Enchantments become hazards. No civilization magic level changed.

### Gate Contact and Closure

A reciprocal World Gate operates for forty years. The Gate Frame tracks communication, material, organism, and embodied-person channels separately. Regional passes summarize trade and migration, while Local Frames preserve an endpoint quarantine service, a stranded minority, and a faction trying to weaponize closure.

Review Points occur when disease controls change, migration creates a viable settlement, Gate Throughput falls, and the Gate Basis reaches its Closure condition. Closure resolves at Focused or Local detail as needed. The later Epochal pass preserves stranded populations, hybrid institutions, ecological transfers, broken supply assumptions, and Gate Legacies rather than returning both domains to their starting states.

### Off-Screen Dungeon Change

The player leaves a Dungeon region for twelve years. The Dungeon remains in an established Active Regime, nearby communities extract one renewable material, and an agentive inhabitant is negotiating access.

Local passes track the Sustaining Basis, extraction pressure, inhabitant decisions, maintenance, and route safety. Routine delves are compressed. A Review Point occurs when extraction exceeds renewal and another when the inhabitant withdraws cooperation. On return, the Dungeon is altered through recorded causes; it did not reset, restock, or scale itself to the player.

### Training Through a Time Skip

A reincarnated swordsman requests a six-year training skip. The player states the training goal, acceptable risks, teacher relationship, and instruction to pause if an old injury worsens.

Local Resolution can summarize routine practice, but Review Points preserve access changes, Plateaus, injury, teacher consent, competing obligations, and opportunities for feedback. When the injury becomes material, play pauses for a player decision. Development resolves the eventual capability. Six elapsed years alone grant nothing.

### Expanding an Old Summary

An earlier Epochal Delta established that a coastal state fragmented after repeated harvest failures, elite succession conflict, and migration. Years later, play enters one successor port that was not individually described.

The GM may create ordinary streets, families, offices, and local disputes consistent with those anchors. The GM cannot reveal that a previously unmentioned immortal admiral secretly caused the entire fragmentation, because that would replace the established causal bridge and retroactively create a decisive actor. If such an actor was genuinely material, the old record was insufficient and requires an explicit correction.

## Safeguards

- Never bind Focused, Local, Regional, or Epochal Resolution to universal durations, distances, populations, or power bands.
- Never use resolution as an encounter level, threat rank, importance score, or permission to override an owner.
- Never run every domain at one global resolution merely for convenience.
- Never compress through a material player choice without prior instruction or a return to play.
- Never give off-screen actors GM knowledge, perfect planning, unanimous goals, or unlimited resources.
- Never let elapsed time grant Development, Skill, Evolution, Soul growth, Awakening, affinity, authority, or recovery by itself.
- Never treat an aggregate trend as true for every member, district, species, faction, institution, source, or endpoint.
- Never erase Material Exceptions, bottlenecks, minorities, dissent, source agency, incompatible bodies, or unresolved dependencies.
- Never convert a possible future into settled history because the interval is long.
- Never extrapolate a reinforcing loop without rechecking constraints, feedback, maintenance, and changed state.
- Never use an Age Transition or World Reset as a default century-simulation shortcut.
- Never invent a decisive hidden cause while expanding a prior summary.
- Never reveal hidden world truth merely because the GM simulated it.
- Never require exhaustive bookkeeping when a shorter causal bridge preserves the same decisions and continuity.
- Never store current Frames, Deltas, actors, timelines, world values, or campaign history in this repository.

## Scope Boundaries

This document does not define:

- a canonical world, timeline, Time Skip, Age, Reset, Gate, war, outbreak, Dungeon, or campaign interval;
- current Simulation Frames, Deltas, Review Points, actors, branches, or world values;
- universal update intervals, formulas, probabilities, event tables, encounter tables, or random generators;
- complete procedures for any specialist World Engine domain;
- personal capability, Soul, Reincarnation, Skill, Evolution, Class, Soul Weapon, or Magic outcomes;
- a time-skip generator, Age-transition generator, world-event generator, or campaign-state template;
- mandatory historical events, progress curves, civilizational stages, or narrative arcs.

## Related Documents

- [World Engine Index](README.md)
- [World Engine Overview](WORLD_ENGINE_OVERVIEW.md)
- [World-State Variables](WORLD_STATE_VARIABLES.md)
- [Causal Event Chains](CAUSAL_EVENT_CHAINS.md)
- [Populations](POPULATIONS.md)
- [Resources and Food](RESOURCES_AND_FOOD.md)
- [Economies](ECONOMIES.md)
- [Ecology and Migration](ECOLOGY_AND_MIGRATION.md)
- [Faction Behaviour](FACTION_BEHAVIOUR.md)
- [War and Unrest](WAR_AND_UNREST.md)
- [Disease Evolution](DISEASE_EVOLUTION.md)
- [Technology and Magical Advancement](TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md)
- [Dungeon Activity](DUNGEON_ACTIVITY.md)
- [World Stability](WORLD_STABILITY.md)
- [Ages and World Resets](AGES_AND_WORLD_RESETS.md)
- [World Gates and World-Contact Events](GATES_AND_WORLD_CONTACT.md)
- [World Gate Interactions with Reincarnation and Soul Avatars](WORLD_GATE_SOUL_INTERACTIONS.md)
- [Soul Engine](../soul/README.md)
- [Development System](../progression/README.md)
- [Skill Engine](../skills/README.md)
- [Monster Evolution](../monster-evolution/README.md)
- [Human Classes and Professions](../human/README.md)
- [Soul Weapons](../soul-weapons/README.md)
- [Magic](../magic/README.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Alpha Playtest Rules](../gm/ALPHA_PLAYTEST_RULES.md)
- [Design Philosophy](../core/DESIGN_PHILOSOPHY.md)
