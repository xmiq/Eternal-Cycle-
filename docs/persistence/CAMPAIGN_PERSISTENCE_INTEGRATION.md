# Campaign Persistence Integration

## Purpose

This document defines how the completed Eternal Cycle systems exchange established campaign facts through the Campaign Persistence Engine without surrendering their own authority.

It is the final integration contract for persistence. It joins the Soul Engine, Development System, Skill Engine, Monster Evolution, Human Classes and Professions, Soul Weapons, Magic, World Engine, and GM Toolkit to one storage-neutral continuity process.

## Core Rule

An owning system establishes what happened and what that outcome means. The Campaign Persistence Engine records the established outcome, its cause, scope, history, information effects, and dependencies. The GM later reads that validated state before further adjudication or simulation.

The direction is always:

1. load authoritative continuity;
2. apply the relevant canonical owners;
3. establish an outcome;
4. persist only the affected dependency closure;
5. validate the candidate;
6. activate the next Campaign Version;
7. derive narration and task views from that active version.

Persistence never creates a mechanic because a field exists. Narration never changes state because a sentence was spoken. A specialist system never rewrites campaign history merely because its rules later change.

## Integrated Ownership Model

The integrated model separates six responsibilities.

| Responsibility | Owner | Persistence relationship |
| --- | --- | --- |
| Reusable meaning and mechanical possibility | Repository Canon and each specialist system | Persistence records the exact Repository Version and owner used |
| Campaign premises and authorized campaign choices | Campaign Canon | Persistence preserves the choice, authority, scope, and version |
| Autonomous change in world reality | World Engine and its specialist domains | Persistence records established changes, causes, timing, and unresolved pressures |
| Adjudication, presentation, and actor operation | GM Toolkit | The GM reads through declared Read Sets and writes through bounded Save Transactions |
| Established continuity | Campaign Persistence Engine | Persistence owns record identity, authority, history, truth separation, updates, migration, correction, and validation |
| Prose and task-specific presentation | Current Narration and Derived Views | Presentations read owners and never become competing owners |

The [Persistence Authority hierarchy](PERSISTENCE_AUTHORITY.md) determines which established claim governs. [Truth Layers](TRUTH_LAYERS.md) determine what kind of information a claim is and who may access it. [Persistence Levels](PERSISTENCE_LEVELS.md) determine how long it can survive. The [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md) determines where its logical owner belongs.

These classifications combine; none substitutes for another.

## Canonical Persistence Operating Cycle

### 1. Select the Active Authority

Read the Save Index and identify:

- Campaign ID;
- active Campaign Version;
- active Repository Version and rules profile;
- Persistence Model Version;
- last valid Save Point;
- open Session, Transaction, Migration, Continuity Case, or recovery state;
- latest applicable validation result;
- required Secret boundary.

Do not select authority by filename, timestamp, transcript recency, or storage location.

### 2. Build the Read Set

Use the [Campaign State Model](CAMPAIGN_STATE_MODEL.md) to load only the dependency closure needed for the intended adjudication or Simulation Pass.

The GM starts with the Save Index, relevant Player State, current Incarnation, location, time, and pending Session state. It then follows material references to Relationships, Species, Projects, Research, Timeline, Knowledge, Secrets, Inventory, factions, world processes, and specialist Profiles as required.

Missing required information remains missing. It triggers clarification, source recovery, a bounded unknown, or [Continuity Resolution](CONTINUITY_RESOLUTION.md), not invention.

### 3. Identify Claim Owners

For every material question, identify:

- the factual subject;
- the canonical Owning System;
- the campaign Authoritative Record Owner;
- Supporting Systems;
- relevant authority and Truth Layer;
- current embodiment, access, and restrictions;
- evidence and uncertainty;
- actor and player agency boundaries.

One interaction may contain several separately owned claims. Resolve each through its owner rather than searching for one system that controls the entire scene.

### 4. Adjudicate or Simulate

The GM applies [GM Responsibilities](../gm/GM_RESPONSIBILITIES.md), [Consequence Resolution](../gm/CONSEQUENCE_RESOLUTION.md), [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md), and the relevant specialist rules.

The World Engine advances autonomous actors and processes when world time or causal conditions require it. Specialist systems determine their own transitions. Persistence remains read-only during resolution except for clearly identified Pending Session material.

### 5. Establish the Outcome Boundary

Before persistence begins, separate:

- established direct outcomes;
- paid costs and consumed inputs;
- current conditions;
- information acquired by each observer;
- Relationship Events;
- historical and temporal placement;
- unresolved pressures and Review Points;
- unsupported possibilities;
- future actor choices not yet made.

Only established outcomes enter the transaction. A likely consequence is not a completed event.

### 6. Open the Save Transaction

Apply the [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md):

1. identify the Gameplay Interaction or bounded simulation result;
2. create the Transaction ID;
3. recheck the parent Campaign Version and Read Set;
4. build the Affected Set;
5. build the Session Delta and Write Set;
6. route each operation to its Authoritative Record Owner;
7. append Session, Timeline, and Campaign History records according to their separate criteria;
8. preserve unresolved claims as Pending, unknown, disputed, or source-recovery work.

World-only change may use the same transaction discipline even when no player action caused it. The Session Log records when it was resolved or integrated; it does not pretend the event occurred in that Session.

### 7. Validate the Candidate

Run the applicable [Persistence Validation](PERSISTENCE_VALIDATION.md) profile against one immutable candidate baseline.

Validation checks architecture, authority, identity, references, chronology, Relationships, Research, Knowledge, Secrets, numerical provenance, specialist ownership, versions, duplication, and drift. It reports defects and routes repair. It never edits the candidate it evaluates.

### 8. Activate or Stop

Activate the complete new Campaign Version only after a valid outcome. Otherwise:

- keep the parent version authoritative;
- preserve staged work and Transaction identity;
- contain any exposed Secret or conflicting claim;
- route repair through the proper owner;
- validate a new candidate.

### 9. Regenerate Views and Continue

After activation, regenerate affected Derived Views. Current Narration may then present the active state through the correct Observer View.

Never repair an authoritative owner by editing only a character sheet, summary, prompt, dashboard, or recap.

## Shared Event and Separate Claims

One material event may produce many owner-specific changes. Those changes share event identity and causal references without collapsing into one record.

For example, a magical bridge collapse may involve:

- Magic owning the magical failure mechanism;
- Infrastructure owning bridge condition and function;
- Locations owning access and route changes;
- World Engine domains owning migration, resource, and economic effects;
- Relationships owning promises, rescue, blame, and debts;
- Knowledge owning what each observer learned;
- Research owning hypotheses and evidence about the failure;
- Timeline owning occurrence and discovery placement;
- Campaign History owning durable consequences.

The event does not grant one module authority over the others. The Save Transaction connects their separate claims through stable references and updates only the affected owners.

## Soul Engine Interface

The [Life Archive](LIFE_ARCHIVE.md) indexes each Soul's active and completed Incarnations through stable Life IDs. Finalized Life Summaries reference established Soul, Development, Skill, Relationship, embodiment, Final Death, and historical outcomes without owning those facts. The Archive is player-visible out-of-character history, not current-character memory, and remains distinct from the metaphysical [Akashic Archive](../soul/AKASHIC_ARCHIVE.md).

The [Soul Engine](../soul/README.md) owns Soul identity, Reincarnation, Soul Depth, Soul Resonance, Soul Echoes, Soul Space, Soul Constellations, Soul Titles, Retained Instincts, Soul Avatars, Akashic Archive interactions, Soul harm, and their persistence eligibility.

Campaign persistence records:

- stable Soul ID;
- distinct Incarnation IDs and order;
- Final Death and Life Reconciliation events;
- Interlife intervals;
- eligible Soul Imprints and their owners;
- current access, suppression, strain, damage, and recovery claims;
- Echo, Title, Constellation, Avatar, Soul Space, Archive, and Soul Weapon references;
- evidence and history for every change.

Persistence cannot decide that a dramatic event creates Soul Depth, Resonance, a Title, an Echo, or an Avatar. It records those outcomes only after their owners establish them.

### Death and Reincarnation Handoff

Final Death and Reincarnation require linked but separate updates:

1. close the former body and Incarnation state;
2. preserve the death event, witnesses, possessions, relationships, social effects, and world consequences;
3. run Life Reconciliation through the Soul owners;
4. record eligible Soul persistence without treating it as present access;
5. preserve Interlife state and unresolved Soul conditions;
6. establish the new Incarnation and embodiment;
7. initialize current access and expression through the new body;
8. preserve former-life history without transferring Inventory, reputation, office, Character Knowledge, or relationships automatically;
9. update Soul and Personal Chronologies separately;
10. validate identity, order, transfer boundaries, and references before activation.

A former companion may recognize the new incarnation only through an established route. Shared Soul identity is not social recognition by itself.

## Development System Interface

[Retained Cross-Life Development](../progression/RETAINED_CROSS_LIFE_DEVELOPMENT.md) consumes Life Archive references and source-owned Development or Skill history. Soul continuity owns retained potential, current owners retain current capability, and effective acceleration profiles remain Derived/Cache records with provenance and recalculation boundaries.

The [Development System](../progression/README.md) owns multidimensional growth and the distinctions among Persistent Potential, Current Access, Embodied Expression, Practised Reliability, Contextual Effectiveness, and World Recognition.

Persistence records established Development Profiles, training and experiential evidence, plateaus, regression, injury, rust, suppression, retained Stat XP, current embodiment, and owner-supported numerical changes.

It must preserve:

- separate Development Tracks;
- breadth and depth distinctions;
- current-life effort;
- latent capability versus expressible capability;
- qualitative comparisons rather than one universal power score;
- Numerical Change Traces for material numbers.

A past-life master entering an unsuitable body retains only what the Development and Soul owners permit. A save field cannot convert Persistent Potential into immediate mastery.

## Skill Engine Interface

The [Skill Engine](../skills/README.md) owns Skill identity, tree membership, representations, access, adaptive creation, evolution, fusion, activation mode, hidden status, conceptual scope, and cleanup.

Persistence records:

- learned Skill identity and provenance;
- current representation and access;
- practice, reliability, restrictions, and embodiment dependencies;
- evolution or fusion events;
- human, monster, and bounded crossover routes;
- hidden information through appropriate views;
- obsolete, superseded, merged, or cleaned representations without deleting history.

Persistence cannot create a Skill from repeated text, copy both source and fused benefits, reveal a Hidden Skill to an unauthorized observer, or restore reliable expression without current-life practice.

## Monster Evolution Interface

Campaign records may reference a configured [Living Codex](../gm-living-codex/README.md) stable ID, Codex Version, and explicit Campaign Divergence. They do not copy reusable authority into campaign persistence or let Codex revisions silently mutate Campaign Canon. Living Codex migrations and campaign Save Transactions remain separate dependency-closed operations even when they use the same SQLite or Google Drive adapters.

[Monster Evolution](../monster-evolution/README.md) owns species identity, ecological and evolutionary pressures, stages, branches, hidden conditions, mutations, adaptation, hybridization, apex outcomes, extinction, replacement, and Soul interactions.

Persistence records current Species References, forms, lineages, populations, local observations, evidence, transition events, ecological dependencies, hidden-condition information boundaries, and extinction or replacement history.

It cannot infer a universal evolution tree from one creature, grant a stage because a field is filled, use kills as an automatic threshold, or treat every member of a species as mechanically identical.

When a monster reincarnates as a human, prior monster forms remain in history and eligible Soul persistence remains owner-bound. The human body does not silently keep former anatomy, species authority, or every monster Skill.

## Human Classes and Professions Interface

[Human Classes and Professions](../human/README.md) owns Classes, Professions, Martial Traditions, Magical Schools, social advancement, institutions, academies, Class evolution, and human progression limits.

Persistence records current-life participation, credentials, offices, institutional access, training history, recognition, obligations, sanctions, and supported capability references.

It keeps these separate from Skills, Development, reputation, legal authority, and Soul identity. Reincarnation does not restore office, certification, membership, property, or social standing without a current-world route.

Nonhuman actors may participate where the relevant structure permits. Persistence records actual access and recognition rather than assuming every society uses human institutions.

## Soul Weapons Interface

[Soul Weapons](../soul-weapons/README.md) owns dormant Weapon Souls, Awakening, Soul Intertwining, personhood, consent, compatibility, Weapon Evolution, Weapon Echoes, Legacy Weapons, vessel continuity, passage through Reincarnation, and Manifestations.

Persistence records the Weapon Soul as a person with stable identity, distinct from its vessel and from ordinary Inventory. It preserves:

- Weapon Soul and vessel IDs;
- bond and Passage Accord state;
- consent, refusal, trust, conflict, and communication;
- Awakening and Evolution events;
- Echoes and shared-event references;
- current vessel location, custody, condition, and accessibility;
- Reincarnation passage, suspension, severance, loss, and recovery;
- Manifestation conditions and consequences.

Inventory may reference a vessel's custody. It never owns the Weapon Soul. A save cannot awaken a weapon, compel consent, create compatibility, inherit mastery, or make Manifestation unrestricted.

## Magic Interface

[Magic](../magic/README.md) owns Mana relations, affinities, spell formation, rituals, enchanting, alchemy, divine and forbidden magic, magical Development, sources, costs, risks, and world interactions.

Persistence records current magical Profiles, source relations, access, learned procedures, active effects, Enchantments, alchemical work, infrastructure, restrictions, traces, consequences, and unresolved claims through their specialist owners.

It keeps Mana, access, reserve, capacity, control, affinity, authority, source response, Skill, Development, and recognition distinct. It does not create generic MP, universal spell access, automatic recovery, or magical superiority.

Hidden magical facts belong to factual owners and protected views. Research records inquiry into them; correct speculation alone does not make them Character Knowledge or Campaign Canon.

## World Engine Interface

The [World Engine](../world-engine/README.md) owns autonomous world simulation, Causal Event Chains, populations, resources, economies, ecology, factions, war, disease, advancement, Dungeons, World Stability, Ages, World Resets, World Gates, and Simulation Abstraction.

The World Engine simulates reality. Persistence remembers reality.

For each established world change, persistence records:

- subject and scope;
- prior condition;
- initiating cause and Causal Links;
- actors, resources, constraints, and counterforces;
- timing, delays, and uncertainty;
- resulting current condition;
- information distribution;
- Pending Consequences and Review Points;
- affected specialist references;
- historical significance where appropriate.

Simulation Abstraction may change detail but never truth. Persistence does not generate random events, force genre transitions, scale encounters, or resolve every off-screen possibility. Unknown branches remain unresolved until a valid procedure settles them.

### Time Skips Ages Resets and Gates

Long-duration and world-boundary procedures preserve continuity as follows:

- a [Time Skip](../gm/TIME_SKIP_PROCEDURE.md) compresses presentation while owner-led Simulation Passes establish change;
- an [Age Transition](../gm/AGE_TRANSITION_PROCEDURE.md) classifies established historical change rather than causing it;
- a [World Reset](../world-engine/AGES_AND_WORLD_RESETS.md) transforms world conditions through its exceptional rules without erasing protected causality or Soul continuity;
- a [World Gate](../world-engine/GATES_AND_WORLD_CONTACT.md) changes contact and movement possibilities without becoming a Soul Gate or an automatic migration of records.

Persistence records former and successor conditions, survivorship, exceptions, regional differences, chronology, world revalidation, identity continuity, information, and consequences. It does not replace these procedures with one reset flag.

## GM Toolkit Interface

The [GM Toolkit](../gm/README.md) operates through persistence.

A human or AI GM must:

- read the active Save Index and relevant Read Set before material adjudication;
- apply canonical owners rather than inventing mechanics;
- maintain separate world truth and Observer Views;
- preserve player and actor agency;
- identify Provisional Rules explicitly;
- resolve uncertainty through supported methods;
- convert completed interactions into Save Transactions;
- validate before relying on a changed state;
- stop and route conflicts rather than defending recent narration;
- keep all populated campaign records outside this repository.

The Toolkit may provide procedures, generators, prompts, and views. Generated preparation remains campaign-external and becomes authoritative only through play, simulation, an authorized campaign decision, or another valid route.

## Information and Visibility Integration

The [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md) supplies the responsibility boundary: persistence remembers Layer 2 objective state and Entity-relative views, while Layer 3 presentation consumes only Perspective-appropriate information. A delivered narration is not a competing state owner.

Every factual change and every information change are evaluated separately.

An event can exist in world truth while:

- no character knows it;
- one witness observes only part of it;
- another actor receives a Rumour;
- researchers form competing theories;
- players hold a Meta theory;
- the GM preserves a protected Secret;
- later evidence confirms or disproves a claim.

Save Updates identify each observer, exact claim, source, acquisition route, time, confidence, and disclosure boundary. Derived Views expose only authorized layers. Validation treats Knowledge and GM Secret leaks as defects rather than as convenient exposition.

## Relationship Research and Infrastructure Integration

### Relationships

The [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md) owns directional history and current relational dimensions. Events establish meetings, trust changes, hostility, communication, promises, betrayals, debts, gifts, kinship, organization ties, dependencies, and unresolved issues through valid causes.

Actor records reference those Relationships. They do not flatten them into one disposition or silently reset a known character.

### Research

The [Research Engine](RESEARCH_ENGINE.md) owns Observation, Hypothesis, Experiment, Evidence, Theory, confidence, confirmation, disproof, loss, and Rediscovery. Specialist and world owners establish the underlying facts.

Research remains distinct from Character Knowledge, Player Theories, Campaign Canon, and GM Secrets. Migration and Save Updates preserve competing and obsolete theories rather than retaining only the latest conclusion.

### Infrastructure and Projects

Persistence owns records of actual infrastructure and Project state: participants, authority, resources, methods, dependencies, work, condition, access, outputs, interruption, and Review Points. Development, Skills, Magic, institutions, resources, and the World Engine own their contributions.

Elapsed time, a project label, or a planned milestone never proves completion.

## Rules Revisions Provisional Rules and Migration

A [Provisional Rule](../gm/ALPHA_PLAYTEST_RULES.md) is recorded in the active campaign rules profile with scope, owner, dependencies, expiry, and review conditions. Its valid prior outcomes remain Campaign Canon unless explicitly converted or retconned.

When Repository Canon changes:

1. compare the active rules profile with the new Repository Version;
2. identify affected claims and dependency closure;
3. choose compatibility, prospective adoption, conversion, continued legacy use where permitted, or explicit retcon;
4. use [Migration and Versioning](MIGRATION_AND_VERSIONING.md) for structural or compatibility changes;
5. use [Continuity Resolution](CONTINUITY_RESOLUTION.md) for incompatible campaign claims;
6. preserve history, reliance, uncertainty, and Secrets;
7. validate before activating the target Campaign Version.

Repository improvement never silently rewrites established play.

## Correction Routing

Route defects by kind:

| Condition | Route |
| --- | --- |
| Ordinary established state change | Save Update Protocol |
| Incompatible narration or campaign claims | Continuity Resolution |
| Structural, import, version, or bulk conversion | Migration and Versioning |
| Invalid specialist outcome | Owning specialist system, then bounded persistence correction |
| Missing required source | Requires Source Recovery or explicit unknown |
| Invalid Derived View | Regenerate or repair the view from its owners |
| Repository rules and governance conflict | Repository workflow; neither silently overrides the other |
| Knowledge or Secret exposure | Containment, protected correction, reliance review, and revalidation |

Do not use an easier route merely to avoid preserving evidence or authorization.

## AI Runtime and Adapter Boundary

The Campaign Persistence Engine owns the logical continuity contract: what must persist, which record owns it, how authority and Truth Layers constrain it, and how Save, Migration, Continuity, and Validation procedures operate. It remains storage-neutral.

The [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md) owns the implementation-neutral relationship among the player, AI Game Master, execution profile, adapters, campaign configuration, Canonical Campaign State, and repository rules. Runtime layers implement this integration contract without outranking it.

Operational ownership is divided as follows:

| Concern | Owner |
| --- | --- |
| Logical campaign meaning, authority, history, and validation requirements | Campaign Persistence Engine |
| Runtime ordering and provider-specific operating constraints | selected AI Execution Profile |
| Storage transaction, deployment, backup, and Read-Back Validation procedures | selected Persistence Adapter or Adapter Chain |
| Deployment-specific profile, adapter selection, locators, versions, and active rulings | external Campaign Configuration |
| Populated characters, discoveries, relationships, secrets, timelines, and world state | external Canonical Campaign State |

An Adapter Chain may compose distinct responsibilities. For example, the [SQLite adapter](../ai/chatgpt/adapters/SQLITE_PERSISTENCE_ADAPTER.md) may own database transactions and integrity while the [Google Drive adapter](../ai/chatgpt/adapters/GOOGLE_DRIVE_PERSISTENCE_ADAPTER.md) owns canonical remote identity, deployment, backup, and remote read-back. Campaign Configuration selects and orders the chain; neither adapter adjudicates gameplay.

Save-Before-Delivery is an execution constraint of profiles that adopt it. It is not fictional physics, a new persistence authority, or a change to when an in-world event occurs. Validation remains read-only with respect to the state being evaluated.

Campaign discoveries never migrate into universal runtime files merely because a profile or adapter observed them. Runtime documents remain replaceable, and campaign-specific identifiers, URLs, credentials, facts, and rulings remain in protected external configuration or campaign records.

The canonical adapter naming convention is `<TECHNOLOGY>_PERSISTENCE_ADAPTER.md`. A future profile or adapter may be added only when it preserves these ownership boundaries and repository conventions.

## Storage and Blank Templates

Logical organization, authority, identity, truth layers, lifetimes, history, transactions, and validation are canonical. Markdown, databases, cloud drives, Git, paper records, and applications are implementation choices.

The repository's [blank templates](../../templates/README.md) derive from:

- the Common Record Contract;
- logical module ownership;
- stable IDs and typed references;
- the authority hierarchy;
- Truth Layers and visibility;
- Persistence Levels;
- Campaign State Read Sets and claims;
- Relationship, Research, and Timeline owners;
- Save, Migration, Continuity, and Validation protocols;
- this integration contract.

A template may omit fields irrelevant to its record type or scale. It cannot change ownership, flatten distinct claims, make an optional system mandatory, or introduce populated campaign data into this repository.

## Worked Examples

### Reincarnated Craft Master

A master artisan dies after founding a workshop. Final Death closes the Incarnation, Inventory remains in the world, Relationships preserve apprentices and obligations, Infrastructure preserves the workshop, and the Soul owner determines eligible retention.

The next incarnation retains only owner-approved Soul persistence. Development records Persistent Potential separately from current access and embodied expression. Human institutions do not restore credentials. The new character must train and obtain tools before reliable craft returns.

### Monster Form to Human Body

A monster with vibration-based senses reincarnates as a human. The former Species form, Evolution history, and Retained Instinct evidence remain linked to the Soul. The human body lacks the former sensory anatomy.

The Soul, Development, Skill, and embodiment owners determine whether a bounded analogue can be relearned. Persistence records the result; it does not copy the monster trait into Player State because an old record exists.

### Magical Disaster and Faction Response

A ritual failure contaminates a river. Magic establishes the failure and magical effects. Resources and Food, Ecology and Migration, Disease Evolution, Economies, and Faction Behaviour establish their own responses over time.

Persistence records one shared event identity, separate owner claims, what each faction knows, active Research theories, settlement effects, Timeline placement, and Pending Consequences. It does not decide that every downstream possibility happened at once.

### Rediscovered Soul Weapon

A new incarnation finds a former Soul Weapon vessel in a ruin. Location and Inventory identify the vessel; Soul Weapon rules establish Weapon Soul continuity, recognition, consent, compatibility, and available expression; Relationship memory preserves the partners' prior history.

Finding the vessel does not guarantee reunion, obedience, full manifestation, or inherited technique. The Save Transaction records only what the encounter establishes.

### Contradictory Narration

Narration calls a long-known ally a stranger. The GM stops using that claim, loads the Relationship and Timeline owners, and opens a Continuity Case.

If no memory loss, disguise, mistaken identity, or other valid cause exists, the statement is a Narration Error. The owner records remain authoritative, the view is corrected, Reliance Effects are reviewed, and validation runs before affected play resumes.

## Integration Audit Checklist

A Campaign Persistence implementation is internally integrated only when:

- [ ] one active Campaign Version and rules profile are identifiable;
- [ ] every material campaign fact has one Authoritative Record Owner;
- [ ] every mechanical outcome cites its canonical Owning System;
- [ ] the World Engine simulates and persistence records rather than replacing one another;
- [ ] the GM Toolkit reads through Read Sets and writes through Save Transactions;
- [ ] Soul and Incarnation identity remain distinct;
- [ ] current access never follows from persistence eligibility alone;
- [ ] Development, Skills, Evolution, Human structures, Soul Weapons, and Magic remain separate owners;
- [ ] actor and player agency remain intact;
- [ ] Truth Layers, visibility, and Persistence Levels remain explicit;
- [ ] Timeline, Relationships, Research, Knowledge, Secrets, Inventory, Projects, infrastructure, and world state retain their owners;
- [ ] numerical changes have mechanical provenance;
- [ ] unknowns, disputes, and missing sources are not invented away;
- [ ] Save Updates are bounded, idempotent, and atomic;
- [ ] Migrations have Backup, Audit, Merge, and Validation;
- [ ] corrections preserve history and Reliance Effects;
- [ ] validation detects defects without silently repairing them;
- [ ] Derived Views and narration do not become competing authority;
- [ ] populated campaign records remain outside the rules repository;
- [ ] storage implementations and templates preserve the logical model.

## Safeguards

- Persistence records outcomes; it does not grant them.
- Specialist owners remain authoritative for their mechanics.
- The World Engine remains authoritative for autonomous world change.
- The GM Toolkit never bypasses the active Campaign Version for remembered convenience.
- Conversation context may supplement but never silently override structured persistence.
- Current Narration and Derived Views never become write paths.
- One event may affect many owners, but one module never absorbs all claims.
- Current state never erases Historical Record.
- Reincarnation never transfers world-bound state without an established route.
- Truth, Knowledge, Research, Rumour, theory, Secret, and Meta remain distinct.
- Numerical fields never invent causes.
- Migration never starts without Backup or ends without Validation.
- Templates never redefine the persistence architecture.
- No populated campaign record belongs in this repository.

## Scope Boundaries

This document defines completed-system interfaces, the integrated operating cycle, owner handoffs, cross-system state changes, correction routes, and the blank-template boundary. It does not reproduce specialist mechanics, define a storage syntax, create a campaign, populate a save, create or populate a template, or automate adjudication.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Campaign Persistence Philosophy](CAMPAIGN_PERSISTENCE_PHILOSOPHY.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Persistence Levels](PERSISTENCE_LEVELS.md)
- [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md)
- [Research Engine](RESEARCH_ENGINE.md)
- [Timeline Engine](TIMELINE_ENGINE.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Continuity Resolution](CONTINUITY_RESOLUTION.md)
- [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](PERSISTENCE_VALIDATION.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Soul Rules Index](../soul/README.md)
- [Progression Rules Index](../progression/README.md)
- [Skill Engine Index](../skills/README.md)
- [Monster Evolution Index](../monster-evolution/README.md)
- [Human Classes and Professions Index](../human/README.md)
- [Soul Weapons Index](../soul-weapons/README.md)
- [Magic Index](../magic/README.md)
- [World Engine Index](../world-engine/README.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
