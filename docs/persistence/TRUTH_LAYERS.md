# Truth Layers

## Purpose

This document defines the canonical truth layers used by the Campaign Persistence Engine. A truth layer identifies what kind of informational claim a record contains, who owns that classification, who may see it, how it changes, and what a migration must preserve.

Truth layers prevent facts, memories, research, guesses, rumours, secrets, and out-of-character discussion from collapsing into one pool of presumed knowledge.

## Core Rule

Every material informational claim belongs to one of these layers:

1. **Repository Canon**
2. **Campaign Canon**
3. **Historical Record**
4. **Character Knowledge**
5. **Research**
6. **Player Theories**
7. **Rumours**
8. **GM Secrets**
9. **Meta**

Each layer preserves ownership, visibility, update authority, provenance, uncertainty where applicable, and migration behavior.

No claim changes layer through repetition, recency, confidence, secrecy, narrative importance, storage location, or convenience.

## Independent Classifications

Truth layer, persistence authority, visibility, certainty, and lifetime are separate dimensions.

| Dimension | Question |
| --- | --- |
| **Truth layer** | What kind of informational claim is this? |
| **Persistence authority** | Which conflicting campaign-fact source governs? |
| **Visibility** | Which participant or interface may access it? |
| **Certainty** | How strongly is this claim supported within its layer? |
| **Persistence level** | How long should this record survive and under whose continuity? |

An established hidden murder may be Campaign Canon in truth layer, Historical Record in authority and chronology, visible only through GM Secrets, confirmed in certainty, and Historical in lifetime. These are compatible classifications.

The [Persistence Authority Chain](PERSISTENCE_AUTHORITY.md) remains the conflict owner. [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md) remains the owner of evidence states, Observer Views, resolution methods, and fair secrecy. This document owns durable information-kind separation.

## Common Layer Contract

Every material truth-layer record identifies:

- **Claim ID:** the stable identity of the informational claim;
- **Subject and scope:** what, where, when, and for whom the claim applies;
- **Layer:** one of the nine canonical layers;
- **Owner:** the authoritative Persistence Module and responsible actor or process;
- **Source provenance:** how this layer received the claim;
- **Visibility rule:** who may access content and metadata;
- **Status:** established, active, superseded, disputed, unknown, disproved, or another owner-defined state;
- **confidence or support:** when the layer permits it;
- **references:** authoritative facts, observations, speakers, records, or Meta sources;
- **update route:** what can change it;
- **migration rule:** what must remain separate and protected during transfer.

Layer classification attaches to a claim, not necessarily a whole file. One physical document may contain several layers only if an implementation preserves their boundaries and access controls safely.

## Repository Canon

### Ownership

Repository Canon is owned jointly by canonical rules under `docs/` and accepted governance under `design/`, according to [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md). It contains reusable rules, definitions, ownership, and safeguards rather than populated campaign facts.

### Visibility

Repository Canon is ordinarily available to all campaign participants. A GM procedure may explain hidden-information handling, but the rule does not become a campaign secret.

### Update Rules

Repository Canon changes only through repository workflow, roadmap authority, accepted decisions, terminology review, validation, and commit history. Campaign events and repeated Provisional Rules do not amend it automatically.

### Migration Rules

A campaign migration records which repository version it adopts and how affected campaign records convert. The former rules profile remains traceable for historical interpretation. Copying repository text into a campaign does not create a new canonical version.

## Campaign Canon

### Ownership

Campaign Canon contains established campaign-specific reality and authorized campaign-level commitments permitted by Repository Canon. Its records are owned by their appropriate Persistence Modules under the campaign's authorized control process.

It may include adopted premises, established identities, current world truth, established events, authorized retcons, confirmed discoveries, current conditions, and factual hidden causes.

Campaign Canon is a truth classification. Current state and historical events may both contain Campaign Canon claims while occupying different modules and authority positions.

### Visibility

Campaign Canon is not automatically public. A fact may be known widely, known only to selected actors, physically inaccessible, or hidden from all current characters. Visibility is recorded separately.

### Update Rules

Campaign Canon changes through valid adjudicated events, supported World Engine transitions, authorized campaign decisions, In-World Confirmation, explicit corrections, authorized retcons, and validated migrations.

Every update identifies its owner, cause, time, scope, affected records, and information consequences. It cannot create a mechanic or overwrite Repository Canon.

### Migration Rules

Migration preserves campaign fact, provenance, scope, hiddenness, identity, chronology, and unresolved uncertainty. A conversion cannot publicize secrets, merge distinct claims, turn unknowns into facts, or make a summary more authoritative than its sources.

## Historical Record

### Ownership

Historical Record claims are owned by Campaign History and Timeline for established past events, transitions, discoveries, corrections, and consequences. Their mechanics remain owned by the relevant canonical systems.

History may preserve clearly labelled testimony, archival fragments, competing chronologies, and source disputes without promoting them to factual event truth.

### Visibility

History may be publicly known, privately recorded, lost, suppressed, misdated, or known only to particular observers. The GM's factual history and an in-world archive are not the same view.

### Update Rules

Historical Record is append-only except for traceable factual correction and authorized retcon. New evidence may change interpretation, dating, or confidence while preserving prior records and their effects.

Ending an effect does not delete the event. Correcting an error does not erase that the erroneous account once influenced decisions.

### Migration Rules

Migration preserves event IDs, temporal relationships, sources, corrections, supersession, affected identities, information effects, and current-state links. It cannot collapse events into a recap that removes material causality.

## Character Knowledge

### Ownership

Character Knowledge is owned by the Knowledge module for one specific character, incarnation, Weapon Soul, person-like actor, or other established observer.

It may contain direct observations, communications, memories, learned records, interpretations, estimates, beliefs, known Rumours, known Research, and explicit unknowns. It records what that observer can access, not everything a player or GM knows.

### Visibility

The controlling player ordinarily receives what the character can consciously access, subject to memory and presentation rules. Other participants do not gain it automatically. GM access supports adjudication but does not grant NPCs the same information.

### Update Rules

Character Knowledge changes only through valid perception, communication, instruction, record access, memory recovery, Soul-system access, evidence-based inference, deception, or an owned loss route.

[Memory Continuity](../soul/MEMORY_CONTINUITY.md) owns cross-incarnation autobiographical Memory identity and access history. This layer owns only the Recall Manifestation that a particular incarnation can consciously access; GM or player access to the underlying history does not create that manifestation.

The record distinguishes fact, belief, confidence, source, interpretation, and memory access. Learning a statement does not make it true.

### Migration Rules

Migration preserves observer identity, acquisition route, time, source, confidence, accessibility, memory status, and distinction from world truth. Knowledge cannot be pooled across party members, incarnations, Echoes, factions, or players without a valid route.

## Research

### Ownership

Research is owned by the Research module for a defined question, investigators, methods, observations, evidence, theories, and confidence.

Research records inquiry. It does not own world truth merely because a researcher is skilled, sincere, institutionally respected, or correct by coincidence.

### Visibility

Visibility follows access to sites, notes, publications, participants, institutions, languages, secrets, and communication routes. Different actors may hold different versions or only summaries.

### Update Rules

Research changes through observation, hypothesis, experiment, evidence, critique, replication, theory, confirmation, disproof, loss, and rediscovery under the [Research Engine](RESEARCH_ENGINE.md).

A conclusion becomes Campaign Canon only through **In-World Confirmation** sufficient for the exact claim. The relevant world or specialist owner establishes the corresponding fact. The Research record remains as the history of inquiry.

### Migration Rules

Migration preserves questions, competing theories, methods, evidence provenance, confidence, participants, objections, obsolete or disproved states, and access. It cannot collapse the most confident theory into truth or discard failed work that remains consequential.

## Player Theories

### Ownership

Player Theories are owned by the player or participant who proposes them. Campaign tools store them only as optional theory records.

A theory may concern identity, future events, mechanics, motives, history, research, or symbolism. Correctness does not change its layer by itself.

### Visibility

The owner chooses whether a Player Theory is private, shared with players, shared with the GM, or public out of character. A private theory need not be exposed for persistence convenience.

### Update Rules

Players may revise, branch, withdraw, or compare theories. The GM must not treat one as a declared character belief or action unless the player establishes that in play.

A Player Theory enters Character Knowledge only when the character independently forms or receives it through a valid route. It may align with Campaign Canon when confirmed, but the original record remains a Player Theory.

### Migration Rules

Migration preserves ownership and visibility. A Player Theory cannot be copied into Character Knowledge, Research, GM Secrets, or Campaign Canon because a destination lacks a separate field.

## Rumours

### Ownership

A Rumour is owned by Knowledge or a dedicated Rumour record as a circulating claim with speaker, audience, transmission route, wording or meaning, provenance where known, and current reach.

A Rumour can be true, false, mixed, outdated, distorted, planted, prophetic, or unverifiable. Its truth and social existence are separate claims.

### Visibility

Visibility follows actual circulation. A city-wide Rumour may remain unknown to a visitor; a whisper may be known only to two people. GM knowledge of a Rumour does not give it universal reach.

### Update Rules

Rumours change through telling, omission, translation, interpretation, evidence, authority, suppression, incentives, fear, and social networks. A recipient gains Character Knowledge of the Rumour, not automatic knowledge of its truth.

Verification may establish a separate Campaign Canon fact. Disproof may change belief and circulation without deleting the historical Rumour.

### Migration Rules

Migration preserves origin where known, transmitters, audiences, versions, reach, timing, believed confidence, independent factual status, and consequences. Similar wording does not prove one shared source.

## GM Secrets

### Ownership

GM Secrets is the protected access layer for information withheld from specified players or characters. Secrets owns access and disclosure metadata; the underlying claim remains owned by Campaign Canon, Historical Record, Research, a prepared possibility, or another appropriate layer.

Every GM Secret classifies its content as:

- established hidden fact;
- observer-limited record;
- unresolved question;
- prepared possibility;
- future intention of an autonomous actor;
- Meta planning note.

Secrecy does not make a preferred twist, draft motive, or possible future event true.

### Visibility

Access is restricted to authorized GMs, tools, or participants. Indexes, filenames, aliases, counts, backlinks, summaries, and validation reports must not leak protected content.

### Update Rules

The underlying owner changes the fact. Secrets changes access, disclosure status, audience, and reveal history. A secret becomes known through valid discovery, communication, evidence, or authorized disclosure, not because the GM needs a reveal.

### Migration Rules

Migration preserves access boundaries, content classification, owner references, audience exclusions, reveal conditions, and disclosure history. If a destination cannot protect a secret, migration is blocked or the record moves to a secure implementation; it is not made public.

## Meta

### Ownership

Meta contains out-of-character material about play, tools, preferences, feedback, prompts, plans, scheduling, technical operations, design discussion, and participant coordination that is not established campaign reality.

Meta may be owned by a participant, GM, campaign administrator, or tool according to purpose.

### Visibility

Visibility follows participant agreements. Some Meta material may be public to the table; private safety, feedback, or tool notes may require restricted access.

### Update Rules

Authorized participants may update Meta. A Meta instruction can authorize a separate campaign action, premise, retcon proposal, or player choice, but the resulting campaign record must be created through the proper authority and owner.

**Meta information never enters Campaign Canon.** A distinct process may use Meta input to establish a new Campaign Canon decision or in-world action, but the raw statement remains Meta.

### Migration Rules

Meta migrates separately from campaign truth. It must not be merged into Character Knowledge, Historical Record, Research, or GM Secrets because of shared transcripts or filenames. Sensitive participant material retains its access boundary or is omitted through an authorized privacy policy.

## Truth Promotion

**Truth Promotion** is an explicit, source-preserving creation or reclassification of a claim in a more authoritative factual layer after that layer's requirements are met.

Promotion never deletes the origin. It creates or updates the receiving layer and retains a source reference.

| From | Possible receiving layer | Required route |
| --- | --- | --- |
| Research | Campaign Canon | In-World Confirmation through the relevant factual owner |
| Rumour | Character Knowledge | The observer receives the Rumour |
| Rumour | Campaign Canon | Independent verification of the underlying claim |
| Player Theory | Character Knowledge | The character independently forms or receives the idea |
| Player Theory | Campaign Canon | Independent in-world establishment of the fact |
| GM Secret | Character Knowledge | Valid discovery or disclosure of an established hidden fact |
| Meta proposal | Campaign Canon | Separate authorized decision, retcon, or in-world action |
| Historical fragment | Historical Record fact | Corroboration sufficient for the exact claim |

The originating Research, Rumour, Theory, Secret, or Meta record keeps its own history.

## In-World Confirmation

**In-World Confirmation** is evidence and resolution sufficient under the relevant canonical owner to establish one exact campaign fact.

It requires:

- a precise claim;
- valid evidence or observation;
- source and method provenance;
- appropriate capability and access;
- treatment of alternatives and uncertainty;
- specialist adjudication where mechanics are involved;
- a recorded event or finding;
- updates to affected Knowledge without granting universal awareness.

Confirmation is claim-specific. One specimen reacting to fire does not prove a universal species weakness. One working spell under one Mana context does not establish compatibility in every Age.

## Knowledge Leakage

A **Knowledge Leak** occurs when information reaches an observer, participant, view, or tool without a valid perception, communication, memory, research, disclosure, authorization, or inference route.

Common leaks include:

- an NPC acting on GM-only truth;
- a player-facing summary exposing a hidden identifier or title;
- party knowledge pooling automatically;
- a new incarnation receiving world-bound memories without Soul access;
- a collaborator knowing an experiment they never observed;
- a public migration report listing secret conflicts;
- a Player Theory becoming the character's suspicion without player action;
- a Derived View exposing protected backlinks.

Fair play still presents what an observer can perceive, infer, investigate, or recognize while preserving source and uncertainty.

## Unknown Information

Missing information remains unknown. Use explicit labels:

- `Unknown` - no supported value is established;
- `Not Yet Verified` - a claim lacks sufficient confirmation;
- `Estimated` - a bounded inference with stated basis;
- `Requires Source Recovery` - material evidence may exist but is unavailable;
- `Player Theory` - an out-of-character hypothesis;
- `Rumour` - a circulating in-world claim;
- `Disputed` - incompatible supported accounts remain unresolved;
- `Disproved` - evidence has defeated the claim within scope.

`Unknown` is a valid statement about the record's epistemic condition, not a request for improvisation.

## Cross-Layer References

Cross-layer references preserve source and interpretation. A Character Knowledge record may say an observer heard Rumour `R-17` from actor `A-42`, believes it tentatively, and acted upon it. Campaign Canon may secretly mark the allegation false while Historical Record preserves that the Rumour caused a riot.

These claims coexist. No layer is deleted merely because another has a different truth value.

## Worked Examples

### The Player Guesses the Hidden Heir

A player correctly guesses that an innkeeper is the missing heir. The idea remains a Player Theory. The character cannot act from undiscovered evidence unless the player establishes an in-world route for suspicion or inquiry.

The GM neither changes the hidden fact because it was guessed nor grants Character Knowledge automatically.

### Research Confirms a Monster Lifecycle

Researchers observe eggs, track maturation, compare remains, and reproduce environmental conditions. The Research Engine eventually reaches sufficient In-World Confirmation for one scoped lifecycle claim.

The species fact enters Campaign Canon through Monster Evolution and ecology ownership. Experiments remain Research, observers gain only the Knowledge actually communicated, and uninformed settlements remain uninformed.

### A False Rumour Causes Real War

A forged message creates a Rumour that one kingdom poisoned another's wells. The accusation is false, but its circulation, belief, mobilization, and casualties are real Campaign Canon and Historical Record facts.

Disproving the accusation does not erase the war or relationships it changed.

### A Planned Betrayal Is Not Yet True

The GM records that an NPC may betray the party if a debt is called and fear outweighs loyalty. This is a protected prepared possibility and actor intention, not an established future event.

The NPC's later information, relationships, pressure, and choice still determine what happens.

### Meta Discussion Produces a Retcon

Players discuss out of character that a name collision is confusing. The discussion remains Meta. If the authorized process adopts a retcon, a separate Campaign Canon decision records the correction and migration scope.

The raw discussion never becomes in-world dialogue or Character Knowledge.

## Safeguards

- Every material informational claim has an explicit truth layer.
- Truth layer does not collapse authority, visibility, certainty, or lifetime.
- Research becomes Campaign Canon only through In-World Confirmation.
- Player Theories do not become character beliefs or facts automatically.
- Rumour truth and Rumour circulation remain separate.
- GM secrecy grants no factual authority.
- Future plans and prepared possibilities remain nonfacts until established.
- Meta information never enters Campaign Canon.
- Knowledge requires a valid acquisition route.
- Migration preserves layer, owner, provenance, visibility, and uncertainty.
- Unknown information remains unknown.
- Derived Views and indexes cannot leak protected content.
- Correcting truth does not erase the history of belief, deception, or consequence.
- No populated truth-layer record belongs in this repository.

## Architectural Application

The [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md) applies these layers across all Entities, not only the player character. Objective state belongs to the GM Simulation Engine. Character Knowledge is Entity-relative. GM Secrets protect information but do not create a second objective truth. The Player RPG Interface receives only Perspective-appropriate information.

Observation, belief, suspicion, theory, inference, and misinformation may disagree between Entities without changing Canon. Detailed per-Entity evidence and confidence structures remain a future Knowledge System concern.

## Scope Boundaries

This document does not define persistence lifetimes, complete Campaign State fields, the Research lifecycle in full, relationship memory, chronology structures, secret-storage technology, migration stages, save-update transactions, or validation algorithms.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Campaign Persistence Philosophy](CAMPAIGN_PERSISTENCE_PHILOSOPHY.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md)
- [Hidden Skills](../skills/HIDDEN_SKILLS.md)
- [Akashic Archive](../soul/AKASHIC_ARCHIVE.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
- [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
