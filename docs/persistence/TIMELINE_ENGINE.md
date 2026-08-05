# Timeline Engine

## Purpose

This document defines how Eternal Cycle preserves chronology across world change, play, individual lives, Reincarnation, Time Skips, Age Transitions, World Resets, discovery, narration, and later correction.

Chronology is gameplay because timing, duration, sequence, simultaneity, absence, delay, inheritance, discovery, and historical uncertainty affect what can happen and what participants can know.

## Core Rule

The Timeline Engine records established temporal claims and relationships. It does not create the events it records.

Every material event has a stable identity, an owning system, a truth layer, a persistence level, a source, temporal claims with honest precision, causal references where established, and links to the records it changes.

The engine maintains five related but non-interchangeable chronologies:

1. World History;
2. Campaign History;
3. Session Log;
4. Personal Chronology;
5. Soul Chronology.

No one chronology silently substitutes for another.

## Timeline Is Not

The Timeline Engine is not:

- a world simulator;
- a universal calendar;
- a scripted history;
- a queue of predetermined outcomes;
- an encounter scheduler;
- a substitute for Campaign State;
- a complete transcript;
- proof of causation merely because one event precedes another;
- permission to invent exact dates;
- a way to make every event publicly known;
- a reason to erase contradictory testimony;
- a populated campaign record stored in this repository.

The [World Engine](../world-engine/README.md) owns autonomous change. Specialist systems own their events and effects. The [Campaign State Model](CAMPAIGN_STATE_MODEL.md) owns the current authoritative state graph. The Timeline Engine preserves when and in what order established changes occurred.

## Timeline Event

A **Timeline Event** is a stable temporal record for one material occurrence, process, transition, decision, finding, correction, or bounded state change.

Every material Timeline Event records:

- Event ID;
- concise event description;
- event kind;
- owning canonical system and Authoritative Record Owner;
- truth layer and Persistence Level;
- participants, affected entities, locations, and scopes by stable reference;
- source event, adjudication, decision, or imported record;
- occurrence time or interval;
- Temporal Precision and source calendar;
- chronological relations to other events;
- established causes, prerequisites, consequences, and dependencies;
- Character Knowledge and visibility references;
- state, Project, Relationship, Research, Species, Soul, item, faction, and location changes;
- record, discovery, narration, and integration times where material;
- uncertainty, disagreement, correction, or retcon status;
- validation and migration provenance.

An event may refer to a moment, an interval, an ongoing process, or a boundary whose exact date is unknown. Large developments should be decomposed when their parts have different owners, dates, causes, or consequences.

## Temporal Coordinates

An event can have several distinct temporal coordinates:

| Coordinate | Meaning |
| --- | --- |
| **Occurrence Time** | When the event or process happened in the world |
| **Effective Time** | When a rule, office, obligation, condition, or other result began to apply |
| **Record Time** | When a source or persistence record was created |
| **Discovery Time** | When an actor first learned or recovered the information |
| **Narration Time** | When play presented the event to the audience |
| **Integration Time** | When the established result entered validated Campaign State |

These times may differ. A ruin can be created centuries before a character discovers it, narrated in a later session, and integrated after the session closes. The event does not move to the date of discovery or narration.

If a coordinate is irrelevant or unknown, leave it absent or explicitly unknown. Do not copy one date into every field for convenience.

## Temporal Precision

Every temporal claim uses the narrowest honest precision:

| Precision | Meaning |
| --- | --- |
| **Exact** | The relevant calendar coordinate or duration is established exactly for the required scope |
| **Bounded** | The event occurred within established earliest and latest limits |
| **Approximate** | A supported estimate exists with stated uncertainty |
| **Relative** | Only relations such as before, after, during, or overlapping are established |
| **Disputed** | Credible sources or authorities disagree about the temporal claim |
| **Unknown** | Available evidence establishes no responsible temporal placement beyond any recorded constraints |

Precision is claim-specific. An event may have an exact day but an uncertain hour, or an exact sequence within a campaign while its conversion to a foreign calendar remains disputed.

Unknown precision remains unknown. The GM may use a bounded estimate for simulation only when labelled and justified; the estimate does not become Campaign Canon silently.

## Chronological Relations

Timeline Events may be related as:

- before;
- after;
- simultaneous within the required precision;
- overlapping;
- occurring during another interval;
- beginning or ending another state;
- possibly concurrent;
- order disputed;
- order unknown.

Relations are stored independently from display order. A sorted list is a view, not an owner of temporal truth.

### Causal Order

Chronological order and causal order are distinct.

- `A before B` does not prove `A caused B`.
- A cause may operate through a delayed process.
- Several causes may converge on one result.
- One event may produce consequences at different later times.
- A later discovery may explain an older event without retroactively causing it.

Causal links must come from the event's owning system, adjudication, or evidence. The Timeline Engine preserves those links but does not infer them from proximity alone.

## Five Chronologies

### World History

**World History** records established material events and processes in the world regardless of whether a player character witnessed, caused, or knew them.

It may include:

- ecological and demographic change;
- births, deaths, migrations, extinctions, and survivorship;
- faction, settlement, institutional, economic, technological, and magical change;
- wars, disease processes, Dungeon activity, disasters, and Gate contact;
- Age claims, Age Transitions, World Resets, and revalidation;
- creations, destructions, discoveries, and enduring consequences.

World History is not a requirement to simulate every person or second. [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md) determines resolution, while material events retain enough identity and causal structure to support later expansion.

World History may contain GM-hidden truth. Its existence does not grant Character Knowledge.

### Campaign History

**Campaign History** is the append-preserving record of material established changes relevant to the campaign's continuity.

It includes:

- player choices and adjudicated outcomes;
- material NPC and world actions;
- changes to current state;
- consequential discoveries and misunderstandings;
- deaths, Reincarnations, and lasting aftermath;
- Project, Relationship, Research, faction, location, Species, and infrastructure changes;
- authorized corrections and retcons;
- references to the World History and specialist records that own each result.

Campaign History is append-only except for factual correction through an authorized retcon. Even then, correction preserves the original entry, the replacement claim, authorization, reason, affected dependencies, and validation. It never quietly rewrites the past.

Campaign History is selective, not incomplete by accident. It records material continuity rather than every line of dialogue or presentation detail.

### Session Log

The **Session Log** records the order in which play, adjudication, revelation, correction, and integration occurred at the table or interface.

It records:

- Session ID and real-world sequence;
- starting Save Point;
- loaded Read Set and relevant versions;
- scenes and declared actions in play order;
- adjudications and Provisional Rulings;
- established Session Deltas;
- discoveries and information presentation;
- interruptions, unresolved claims, and pending conflicts;
- ending Save Point or recovery status.

Session order is not world chronology. A flashback, recovered document, parallel scene, retrospective ruling, or correction may be presented later while referring to an earlier Occurrence Time.

Transcripts may support the Session Log but do not replace its structured material changes.

### Personal Chronology

A **Personal Chronology** records one actor's embodied continuity across a particular life or other valid personal span.

It may include:

- birth, creation, awakening, or entry into the campaign;
- bodily age and developmental intervals;
- locations, travel, absence, captivity, sleep, stasis, or displacement;
- injuries, recovery, training, professions, Classes, Skills, and access changes;
- relationships, promises, discoveries, beliefs, and identity changes;
- Final Death or other ending condition;
- subjective-duration differences where canon establishes them.

Personal Chronology belongs to the relevant Incarnation or actor identity. It does not automatically merge with another incarnation merely because the same Soul participates.

Personal chronology and Character Knowledge remain separate. An actor may undergo an event without perceiving it, remember an event with the wrong date, or learn about an earlier event much later.

### Soul Chronology

A **Soul Chronology** records only established Soul-level continuity across incarnations.

It may include:

- Soul origin where known;
- Incarnation sequence;
- Final Death boundaries;
- Life Reconciliation;
- Interlife intervals;
- Reincarnation candidate and embodiment events;
- Soul Echo formation and material changes;
- Soul Depth, Soul Resonance, Soul Title, Soul Constellation, Retained Instinct, Soul Avatar, Archive, Soul Weapon bond, Soul Strain, and Soul Wound events through their owners;
- World Resets or Contact Domain changes crossed by the Soul;
- unknown or disputed prior lives.

Soul Chronology does not absorb a former life's Inventory, social office, world-bound Research, relationships, reputation, Character Knowledge, or bodily state. It links to Personal Chronologies and their surviving Historical consequences while preserving the [Reincarnation](../soul/REINCARNATION.md) boundary.

Interlife may have world duration, subjective duration, both, or unknown experience. Do not infer limitless activity from a long world interval.

## Event Identity and Scope

An Event ID remains stable across:

- summaries;
- different calendars;
- discovery by new actors;
- changed interpretation;
- migration;
- correction;
- archival views;
- references from several chronologies.

Do not create duplicate events merely because:

- two actors witnessed the same occurrence;
- different cultures named it differently;
- it appears in World History and Campaign History;
- a Session later reveals it;
- a Soul remembers it through an Echo;
- a correction changes one claim about it.

Create separate events when materially distinct actions, intervals, owners, or consequences require independent identity. Link parent, child, component, and consequence events instead of collapsing a war, migration, research program, or Age Transition into one undifferentiated timestamp.

## Calendars, Ages, and Conversion

Campaigns may use several calendars, local eras, regnal years, seasonal systems, ritual cycles, biological measures, or Age labels.

Every dated claim preserves:

- source calendar and expression;
- conversion basis;
- converted value where useful;
- conversion precision;
- disputed assumptions;
- calendar discontinuities or reforms;
- scope and culture of the label.

One display calendar may be chosen for convenience, but it never erases source dating.

An Age is a scoped historical claim, not a universal timestamp. Age boundaries may be uneven, overlapping, retrospective, or disputed under [Ages and World Resets](../world-engine/AGES_AND_WORLD_RESETS.md). Do not force all regions or cultures onto one boundary date.

## Parallel Events

World activity continues in parallel.

The Timeline Engine supports:

- simultaneous scenes;
- off-screen actions;
- asynchronous travel and communication;
- several factions acting during one interval;
- multiple incarnations or actors with separate Personal Chronologies;
- delayed consequences;
- uncertain synchronization across distant regions or Contact Domains.

When parallel branches interact, establish a **Synchronization Point** with:

- participating events and scopes;
- each branch's latest valid state;
- temporal precision;
- communication and travel delays;
- dependencies and unresolved conflicts;
- the resulting shared boundary.

Synchronization reconciles timing; it does not grant actors information about branches they could not observe.

## Time Skips

The [Time Skip Procedure](../gm/TIME_SKIP_PROCEDURE.md) owns authorization, Standing Instructions, Simulation Passes, Interruption Triggers, Return Horizons, and the Causal Bridge.

The Timeline Engine receives and preserves:

- skip start and intended horizon;
- actual elapsed interval;
- relevant parallel event branches;
- established interruption and return event;
- material interval events and processes;
- unresolved or abstracted intervals;
- resulting chronological and causal links;
- state integration references.

A Time Skip compresses narration, not time or causality. It grants no automatic training, recovery, research, production, relationship change, faction success, or Soul growth.

## Age Transitions and World Resets

The [Age Transition Procedure](../gm/AGE_TRANSITION_PROCEDURE.md) owns Boundary Findings. [Ages and World Resets](../world-engine/AGES_AND_WORLD_RESETS.md) owns Age claims, Reset qualification, mechanisms, footprints, survivorship, and revalidation.

The Timeline Engine records:

- transition evidence and scope;
- emerging, established, overlapping, absent, or disputed boundaries;
- predecessor and successor Age claims;
- World Reset Preconditions, Trigger, Mechanism, Footprint, Survivorship, and revalidation references where applicable;
- events before, during, and after the boundary;
- calendar discontinuities;
- surviving entities, records, obligations, and causal traces;
- uncertainty and retrospective reclassification.

A Reset does not clear history. Destroyed records may reduce in-world access while Historical continuity and authoritative provenance remain preserved according to visibility and migration rules.

## Reincarnation

The seven-stage Reincarnation transition creates linked events rather than one continuity-breaking replacement:

1. Final Death closes the Personal Chronology of the incarnation when established.
2. Severance distinguishes bodily, worldly, and Soul persistence.
3. Life Reconciliation records eligible Soul changes and surviving world consequences.
4. Interlife records a world interval and any established subjective experience.
5. candidate generation occurs under the world state valid at that time;
6. selection or emergence records the active Reincarnation Mode;
7. Embodiment opens a new Personal Chronology linked to the same Soul Chronology.

The former life remains history. A new incarnation does not inherit the old identity's dates, age, ownership, relationships, office, or knowledge by being later in the same Soul Chronology.

## Knowledge and Historical Discovery

Event truth, evidence of the event, Character Knowledge, Research, Rumours, Player Theories, and GM Secrets remain separate.

When an actor discovers an older event:

- preserve the event's original Occurrence Time;
- create or update a discovery event at the Discovery Time;
- update only the actor's Character Knowledge through a valid route;
- retain uncertainty, source, and possible deception;
- link Research if interpretation or confirmation remains open;
- do not reveal GM Secrets to participants without an in-world route.

A mistaken date or false history can exist as a belief without changing world truth. Later correction changes the belief record and may create social consequences even when the original event remains unchanged.

## Future, Planned, and Conditional Events

Plans, forecasts, prophecies, appointments, deadlines, expected births, intended attacks, Project milestones, and possible consequences are not completed history.

They remain conditional records with:

- proposer or source;
- expected interval;
- assumptions and dependencies;
- confidence and visibility;
- cancellation, fulfilment, interruption, or replacement state.

When the event occurs, create or confirm its Timeline Event and link the prior expectation. Do not silently convert a plan into an outcome because its date arrived.

## Correction and Authorized Retcon

When a chronology claim is wrong or conflicts with stronger authority:

1. stop relying on the conflicting claim;
2. identify the exact event, field, source, and affected chronologies;
3. classify the issue as record error, incomplete information, disputed evidence, mistaken belief, narration error, migration fault, or proposed retcon;
4. consult Repository Canon, Campaign Canon, Historical Record, Current Campaign State, Session material, and specialist owners in authority order;
5. preserve the original claim and its uses;
6. authorize any retcon through the campaign's established authority;
7. append a Correction Event with reason, authorization, replacement claim, effective scope, dependencies, and consequences;
8. update affected views and state;
9. validate chronology, references, knowledge boundaries, and downstream effects.

An authorized retcon changes canonical interpretation or fact within its stated scope. It does not erase that the earlier record existed, what participants previously knew, or consequences that the authorization leaves intact.

Typographical and format repairs that change no claim may be corrected administratively while retaining ordinary version history.

## Timeline Integration Procedure

After a material event:

1. identify the specialist owner and source;
2. assign or resolve stable Event IDs;
3. record the honest temporal coordinates and precision;
4. establish only supported chronological and causal relations;
5. update the relevant five chronologies by reference;
6. update Character Knowledge separately;
7. link current-state changes, consequences, Projects, Relationships, Research, and other records;
8. preserve pending or disputed claims explicitly;
9. validate references, ordering, overlaps, and authority;
10. integrate the result at the next valid Save Point.

This procedure describes the Timeline contribution to persistence. The [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md) owns the complete campaign update transaction.

## Worked Examples

### A Battle Revealed in a Later Session

A frontier battle occurs off-screen on the third day of winter. The World Engine and war owner establish its result. Two sessions later, survivors reach the player characters.

World History and Campaign History use the battle's winter Occurrence Time. The Session Log records when the news was presented. Each listener gains Character Knowledge at that later Discovery Time. The battle is not moved forward to the session in which it was narrated.

### Parallel Faction Operations

Two factions begin operations in different cities during the same week. Communication requires ten days. One succeeds before learning that the other failed.

The operations remain parallel events with separate causes and consequences. Their Synchronization Point occurs when credible news arrives, not at the earlier outcome. Neither faction may act on information it did not yet possess.

### Reincarnation Across Centuries

A monster incarnation reaches Final Death. Its Personal Chronology closes, Life Reconciliation occurs, and the Soul enters an Interlife lasting several centuries. A World Reset occurs during that interval.

The Soul Chronology links both incarnations and the Interlife. World History preserves the Reset and surviving consequences. The new Personal Chronology begins at Embodiment under revalidated conditions. The former Inventory, office, and relationships do not cross merely because the Soul timeline is continuous.

### Disputed Age Boundary

Coastal historians date an Age Transition from the first permanent Gate harbor. Inland scholars date it generations later, when new institutions reached them.

The Timeline Engine preserves both scoped Age claims, evidence, calendars, and boundaries. It does not invent one global transition date. Campaign presentation may use the coastal label while retaining inland exceptions.

### Correcting a Settlement's Destruction Date

A migrated summary says a settlement fell before an evacuation, but the source Session Log and survivor records establish that evacuation began first and destruction followed two days later.

The GM stops using the bad ordering, records a correction with source and authorization, updates affected Timeline relations and Current State, and checks downstream travel, survival, and Character Knowledge. The mistaken summary remains traceable; unrelated history is not rewritten.

### A Prophecy Does Not Schedule Reality

A prophecy names an eclipse as the fall of a dynasty. The eclipse date is established, but the fall remains a prediction.

The prophecy has a source, expected interval, assumptions, and Character Knowledge. At the eclipse, factions act according to their beliefs. The dynasty falls only if established choices and causes produce that result.

## Validation Rules

Timeline validation checks:

- unique stable Event IDs;
- valid entity and record references;
- owner and source for every material event;
- temporal coordinates distinguished correctly;
- honest precision and calendar provenance;
- impossible before/after cycles;
- interval starts not after their ends;
- parallel branches reconciled at valid Synchronization Points;
- causal links not inferred solely from sequence;
- Personal and Soul chronologies linked without identity collapse;
- Session order not mistaken for world order;
- Character Knowledge not inferred from event truth;
- plans not recorded as completed events;
- World Resets and Time Skips not used as erasure;
- corrections and retcons authorized and source-preserving;
- no unsupported numerical, Development, Skill, Research, Relationship, or world-state change;
- no populated Timeline record inside this repository.

Validation reports uncertainty and conflict. It does not invent a date or silently repair history.

## Safeguards

- Events retain stable identity across views, calendars, migration, and correction.
- Occurrence, effective, record, discovery, narration, and integration times remain distinct.
- Unknown, approximate, relative, and disputed dates are not promoted to exact dates.
- Chronological proximity does not establish causation.
- World, Campaign, Session, Personal, and Soul chronologies do not replace one another.
- Campaign History is append-only except through source-preserving authorized retcon.
- Time Skips compress presentation rather than causality.
- Age Transitions classify established history rather than creating it.
- World Resets preserve survivorship, traces, and prior history.
- Reincarnation links lives without transferring world-bound state.
- Discovery and narration do not move an event's Occurrence Time.
- Event truth does not grant Character Knowledge.
- Plans and prophecies do not schedule guaranteed outcomes.
- Timeline records do not grant progression, recovery, production, or relationship change.
- No campaign chronology belongs in this repository.

## Scope Boundaries

This document defines event identity, temporal coordinates and precision, five chronologies, parallel ordering, calendar conversion, historical correction, and interfaces with existing owners. It does not define world simulation, specialist event mechanics, Time Skip authorization, Age or Reset qualification, Reincarnation mechanics, complete save transactions, migration procedure, persistence validation procedure, or campaign content.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Persistence Levels](PERSISTENCE_LEVELS.md)
- [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md)
- [Research Engine](RESEARCH_ENGINE.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Time Skip Procedure](../gm/TIME_SKIP_PROCEDURE.md)
- [Age Transition Procedure](../gm/AGE_TRANSITION_PROCEDURE.md)
- [Reincarnation](../soul/REINCARNATION.md)
- [Ages and World Resets](../world-engine/AGES_AND_WORLD_RESETS.md)
- [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
