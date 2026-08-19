# Persistence Validation

## Purpose

This document defines how Eternal Cycle checks external campaign persistence for structural integrity, authority, continuity, chronology, identity, information separation, causal provenance, and compatibility before a state becomes authoritative or continues to guide play.

Persistence Validation protects continuity by finding defects and routing them to their owners. It does not repair the campaign by guessing.

## Core Rule

Run Persistence Validation:

- before every Save Point activation;
- after every Campaign Migration and before target activation;
- before closing a material Continuity Case;
- after recovery or rollback;
- when loading an uncertain or externally modified campaign;
- periodically across the full campaign at Review Points;
- whenever evidence suggests continuity drift, leakage, duplication, corruption, or invalid state.

Validation reports. It never silently repairs, invents, retcons, leaks, adjudicates, or overrides an Authoritative Record Owner.

## Validation Is Not

Persistence Validation is not:

- a world simulator;
- a rules adjudicator;
- a continuity editor;
- an automatic retcon engine;
- an optimizer or balance pass;
- a universal truth oracle;
- proof that an unknown value should exist;
- permission to load every GM Secret into every view;
- a substitute for Backup;
- a replacement for specialist validation inside Magic, Skills, Evolution, or another owner;
- a storage-specific linter only;
- populated campaign data stored in this repository.

A technically valid file may contain invalid campaign semantics. A semantically coherent campaign may use any storage technology capable of preserving the required meaning.

## Validation Principles

### Evidence Before Assertion

Every finding identifies:

- exact claim or record;
- expected invariant;
- observed evidence;
- source and version;
- affected authority, truth layer, Persistence Level, and owner;
- dependency closure;
- severity and confidence;
- repair route;
- whether protected content limits the visible report.

### Read-Only Evaluation

The validator reads an immutable candidate, Snapshot, Backup, or explicitly bounded live-state view. It does not edit the object it is validating.

### Owner-Routed Repair

A finding routes to:

- Save Update Protocol for an ordinary owner-routed correction;
- Continuity Resolution for incompatible claims;
- Migration and Versioning for structural, version, import, or bulk conversion defects;
- a specialist owner for mechanical invalidity;
- campaign authority for an Authorized Retcon or warning disposition;
- source recovery for unavailable evidence.

After repair, create a new candidate and rerun affected checks. Never mark the original validation Pass because an unrecorded repair was attempted.

### Least Necessary Visibility

Validators receive only the access required by their scope. Secret-aware checks may run inside a protected boundary and emit redacted findings that identify the defect without exposing the secret.

### Reproducible Scope

Every Validation Run identifies its input versions, scope, checks, exclusions, access, tool or procedure version, and time. Another authorized validator should be able to understand what was and was not checked.

## Validation Run

A **Validation Run** is one read-only application of a defined Validation Profile to one immutable Validation Baseline.

It records:

- Validation Run ID;
- purpose and trigger;
- Validation Profile and version;
- Validation Baseline;
- Repository Version;
- Campaign Version or candidate version;
- Persistence Model and Storage Format Versions where applicable;
- Migration, Continuity Case, Transaction, Session, or Recovery references;
- included modules, partitions, records, and dependency closure;
- excluded scope and reason;
- access and Secret boundary;
- checks executed, skipped, unsupported, or inconclusive;
- evidence and source references;
- Validation Findings;
- aggregate outcome;
- warnings and accepted dispositions;
- validator identity and method;
- start and completion times;
- required reruns and Review Points.

No result claims a wider scope than the run actually inspected.

## Validation Baseline

A **Validation Baseline** is the immutable state against which one run evaluates claims.

Depending on the trigger it may be:

- a staged Save Transaction candidate and its parent Campaign Version;
- a migrated target plus source version, Backup, Audit Report, and Merge Plan;
- a Continuity Case candidate plus last authoritative baseline;
- a loaded active Campaign Version plus Save Index;
- a recovery candidate plus verified Backup;
- a full campaign Snapshot at a periodic audit point.

The validator never combines files from different Campaign Versions merely because they are individually recent.

## Validation Profiles

FR-011 turn validation additionally confirms that required owner reads preceded resolution, the Affected Set was explicitly determined, every non-empty set used one owner-routed transaction, the resulting Save Point activated only after required verification, stale Context Packets were invalidated, and relevant Turn N state can be retrieved on Turn N+1. Repository structural validation checks this contract; runtime hosts must execute these cases against configured campaign persistence.

### Save Activation Profile

Checks one Save Transaction's complete candidate dependency closure before activation.

Required focus:

- parent version still governs;
- Transaction and Interaction identity;
- Affected Set and Write Set containment;
- owner, source, event, truth layer, and temporal scope;
- references and indexes;
- numerical traces;
- Session, Timeline, and Campaign History appends;
- Relationship, Research, Project, Mystery, Inventory, Soul, Knowledge, Secret, and world interfaces;
- atomic candidate completeness;
- no unrelated changes.

### Migration Profile

Runs after every Migration and before activation.

Required focus:

- verified Backup and unique Migration ID;
- Audit and Merge traceability;
- source-to-target completeness;
- version compatibility;
- stable identity and aliases;
- owner and truth-layer preservation;
- chronology, uncertainty, and Secret preservation;
- no dangling references;
- no unauthorized changes;
- rollback and Manifest completeness.

### Continuity Resolution Profile

Checks one resolved Continuity Case before play resumes from the affected claim.

Required focus:

- conflict classification;
- authority order;
- source-preserving correction;
- Reliance Effects;
- authorized scope;
- affected dependency closure only;
- Timeline and Campaign History correction;
- Knowledge and Secret treatment;
- numerical and specialist validity;
- explicit resumption boundary.

### Load Integrity Profile

Checks whether a campaign can be used safely for adjudication after loading, external editing, transfer, crash, or uncertain synchronization.

Required focus:

- Save Index and active Campaign Version agreement;
- repository and model compatibility;
- complete required modules and partitions;
- open transactions, migrations, freezes, and Pending Session state;
- reference resolution;
- stale Derived Views;
- integrity or storage errors;
- required Read Set availability;
- Secret access boundary.

### Periodic Full Audit Profile

Checks the complete campaign or declared partition set at a Review Point.

Required focus includes:

- duplicate identity and discovery detection;
- orphan and reference analysis;
- long-term chronology and relationship coherence;
- progression and numerical provenance;
- Knowledge and Secret boundary review;
- Memory Continuity identity, source-Life, cue, accessibility, and incarnation-specific Character Knowledge separation;
- Soul-Bound Companion pair identity, convergence obligation, meaningful-reunion evidence, route ownership, and current Relationship separation;
- Research theory and confirmation consistency;
- version ancestry and migration history;
- Continuity Drift;
- archival and Persistence Level health;
- repository boundary and external-storage separation.

Incremental validation does not permanently replace periodic full audit.

## Severity

Every finding receives one severity based on impact, not on ease of repair.

| Severity | Meaning |
| --- | --- |
| **Blocker** | Activation or affected adjudication would risk authority violation, data loss, identity collapse, Knowledge or Secret leakage, invalid mechanics, unrecoverable causality, or an unresolved required dependency |
| **Error** | A definite defect exists within scope; activation is normally blocked until repaired or formally reclassified through valid authority |
| **Warning** | A bounded non-blocking risk, ambiguity, stale view, incomplete optional record, or approaching Review Point exists and can be carried with explicit disposition |
| **Notice** | Informational result, expected exception, skipped non-required check, or maintenance observation that does not imply invalid continuity |

An easy-to-fix Secret leak is still a Blocker. A large but purely cosmetic formatting difference may be a Notice.

## Outcomes

| Outcome | Requirement |
| --- | --- |
| **Pass** | All required checks complete with no unresolved Blocker or Error and no undisposed Warning |
| **Pass With Warnings** | No Blocker or Error remains; every Warning has a valid owner, scope, disposition, and Review Point |
| **Fail** | One or more Blockers or Errors remain, or a required invariant is violated |
| **Inconclusive** | Required evidence, access, source, owner, or validation capability is unavailable |

Only Pass or an authorized Pass With Warnings may activate a Save or Migration target. Inconclusive does not mean probably valid.

## Validation Finding

Every **Validation Finding** records:

- Finding ID;
- run and baseline;
- severity;
- category;
- exact invariant;
- affected records and stable IDs;
- observed evidence;
- expected evidence or state;
- source, owner, authority, truth layer, and temporal scope;
- dependency and exposure impact;
- confidence and uncertainty;
- protected details or redaction marker;
- recommended repair route, not an invented repair;
- disposition, authorizer, and Review Point;
- linked repair transaction, migration, or Continuity Case;
- rerun result and closure.

Two findings about the same root defect may be linked rather than merged when they affect different owners or security boundaries.

## Structural Integrity Checks

### Required Records and Indexes

Verify:

- Save Index exists and identifies one active Campaign Version;
- required modules and partition indexes exist;
- active Repository and Persistence Model Versions are exact;
- open Session, Migration, Continuity, and Transaction status is coherent;
- record IDs are unique within their identity domain;
- aliases and supersession links resolve;
- derived indexes agree with their owners;
- required validation and version ancestry are present.

### Broken and Dangling References

A **Broken Reference** is a malformed, unreadable, unauthorized, or semantically invalid link.

A **Dangling Reference** points to an identity that should resolve in the validated scope but does not.

Check:

- forward and inverse typed references;
- parent and child Campaign Versions;
- Soul and Incarnation links;
- actor, companion, Relationship, Species, Location, faction, settlement, item, Project, Research, Mystery, event, source, and Knowledge references;
- containers and Inventory custody;
- Timeline relations and correction links;
- Secret indirection and audience-safe indexes;
- migration and transaction provenance.

An intentionally external or unloaded reference must declare its owner, location route, expected scope, access, and status. Otherwise it is dangling rather than merely deferred.

### Orphaned Records

An **Orphaned Record** has no valid authoritative owner, subject, parent, source, or required incoming relation and cannot be interpreted safely in its present scope.

Orphan detection does not delete the record. Route it for identity recovery, archival classification, source recovery, or Continuity Resolution.

Historical records may legitimately have no current-state reference when their subjects ended. They are not orphaned if identity, source, chronology, and authority remain valid.

## Duplicate Detection

### Duplicate NPCs and Actors

Detect possible duplicate actors using:

- stable IDs and aliases;
- Person Basis;
- origin and embodiment;
- Personal and Soul Chronology;
- locations and travel;
- first and latest meetings;
- Relationships and organizations;
- Knowledge and possessions;
- source provenance;
- contradictory simultaneous activity.

Matching names are only a candidate signal. Validation never performs an Identity Merge automatically.

### Duplicate Settlements and Locations

Compare:

- stable place identity;
- geography and boundaries;
- route topology;
- inhabitants and institutions;
- naming history;
- destruction, relocation, reconstruction, occupation, and succession;
- Timeline and source records.

One settlement renamed over time may be one identity. Two places with the same name may be distinct. A rebuilt settlement may preserve, split, or reject continuity according to established claims.

### Duplicate Discoveries

Detect whether apparently separate discoveries describe:

- one world fact discovered by several observers;
- one observation copied through several reports;
- independent confirmation;
- Rediscovery of forgotten knowledge;
- a distinct scoped fact;
- duplicated import or save retry.

Do not count copied evidence as independent, grant Research twice, merge observer Knowledge, or erase separate Discovery Times.

### Other Duplicate Identities

Apply stable-identity checks to:

- Souls and incarnations;
- factions and institutions;
- Species and forms;
- items and Soul Weapon vessels;
- Projects and Mysteries;
- Research questions, Evidence, and theories;
- Timeline Events;
- Transactions, Migrations, and Correction Events.

## Authority and Ownership Checks

Verify:

- authority follows Repository Canon, Campaign Canon, Historical Record, Current Campaign State, Current Session, and Current Narration;
- lower claims do not silently overwrite higher claims;
- every material claim has one Authoritative Record Owner;
- Derived Views do not own facts;
- specialist outcomes cite their canonical owners;
- Repository rules and campaign state remain in separate stores;
- campaign options, Provisional Rules, conversions, and retcons stay within allowed authority;
- no storage timestamp, filename, branch, or application status decides canon;
- governance decisions and playable rules do not conflict silently.

Also verify the invariants in [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md): one Entity identity anchor per persistent non-autonomous subject, one owner for each current-placement claim, current Relationship dimensions only under Relationships, individual-to-Species references without copied reusable definitions, and no active identity duplicated across ordinary Entity ownership and the reserved autonomous domain. Stored Derived records expose source identity, source version, freshness, scope, and audience.

An unresolved conflict between repository governance and playable rules is a repository-validation failure. An unresolved conflict between repository rules and a campaign claim routes through compatibility, Continuity Resolution, or Migration.

## State and Version Checks

Verify:

- active Save Index points to one validated Campaign Version;
- Campaign Version parentage has no impossible cycles;
- Repository, Persistence Model, and Storage Format compatibility is recorded;
- Confirmed Current State, Historical Record, and Pending Session state remain distinct;
- Session Deltas integrate once;
- Transaction retries are idempotent;
- no partial Write Set is active;
- superseded and archived claims do not govern current state;
- Snapshots and Derived Views identify their source version;
- rollback and recovery preserve ancestry and post-activation conflicts.

## Relationship Validation

Detect:

- known actors treated as strangers without a valid recognition, memory, identity, disguise, transformation, or information cause;
- first meeting after latest meeting;
- important encounters missing from participant chronology;
- directional dimensions incorrectly mirrored or averaged;
- trust changing without an event and scope;
- promises, betrayals, debts, gifts, family, organizations, and dependencies merged into one score;
- fulfilled, breached, released, disputed, or impossible commitments with no causal event;
- a new incarnation inheriting former relationship claims automatically;
- participant identity mismatch;
- closure that erases unresolved history.

Different participant perspectives are not defects merely because they disagree.

## Timeline Validation

For [Long-Horizon Summaries](LONG_HORIZON_SUMMARIES.md), also validate stable period identity, temporal ordering at claimed precision, scope and typed references, important established causal links, unknown preservation, acyclic nesting, player-view secrecy, current-state ownership, World Reset continuity, and revision provenance.

Detect:

- impossible before-and-after cycles;
- event intervals whose start follows their end;
- Session order used as Occurrence Time;
- discovery, narration, record, effective, and Integration Times collapsed incorrectly;
- unsupported exact dates;
- incompatible calendar conversion;
- parallel branches synchronized before communication or travel permits it;
- causal links inferred only from temporal proximity;
- World, Campaign, Session, Personal, and Soul chronologies collapsed;
- World Resets or Time Skips erasing history;
- Campaign History corrections without authorization and supersession;
- plans or prophecies recorded as completed events.

Overlapping or disputed dates are valid when explicitly represented with evidence and scope.

## Knowledge and Secret Validation

### Knowledge Leaks

Detect information reaching an observer without a valid perception, communication, memory, Research, disclosure, authorization, or inference route.

Check:

- NPC action against Observer View;
- party and institutional information routes;
- reincarnated Character Knowledge;
- publication and Research access;
- Derived Views and inverse references;
- player-facing summaries;
- migrated transcripts;
- tool prompts, caches, logs, and error messages where they are part of the storage implementation.

### GM Secret Leaks

A **GM Secret Leak** exposes protected factual, preparatory, identity, motive, location, causal, or Mystery information to an unauthorized participant, view, index, report, or tool.

GM Secret leaks are Blockers for the exposed scope. The validation report must be redacted so it does not repeat the secret while explaining the route, affected audience, containment requirement, and repair owner.

### Layer Consistency

Verify:

- Campaign Canon, Historical Record, Character Knowledge, Research, Player Theories, Rumours, GM Secrets, and Meta remain distinct;
- Research enters Campaign Canon only through In-World Confirmation;
- Player Theories do not become character beliefs without a route;
- Rumour circulation and truth remain separate;
- Meta does not become campaign fact;
- correction of world truth does not erase historical belief or deception;
- visibility labels agree with actual indexes and access.

## Research and Theory Validation

Detect:

- duplicate Research, claim, Observation, Experiment, Evidence, or Theory IDs;
- copied evidence counted as independent;
- confidence unsupported by evidence;
- Confirmed Knowledge lacking In-World Confirmation or factual-owner reference;
- several scopes collapsed into one confidence;
- contradictory theories merged or silently selected;
- a theory marked simultaneously active and Disproved within the same scope without chronology;
- Obsolete, Forgotten, and Disproved states confused;
- disproof erasing consequences or valid subclaims;
- Rediscovery granting old infrastructure, authority, or mastery automatically;
- failed experiments granting unsupported progression;
- Secret Research leaking through validation.

Competing theories are not defects merely because they contradict one another. The defect is loss of separate identity, evidence, scope, confidence, or unresolved status.

## Numerical and Mechanical Validation

Detect:

- numerical change without a Numerical Change Trace;
- prior plus change not reconciling with result where arithmetic applies;
- duplicate application of a Transaction or event;
- negative, impossible, or out-of-domain value under its owner;
- missing input, cost, source, or effect;
- silent rounding, balancing, averaging, or default zero;
- progression granted by time, repetition, suffering, killing, injury, death, title, or narration without an owning rule;
- persistent potential treated as current access or expression;
- former bodily statistics transferred through Reincarnation;
- Skill, Development, Class, Evolution, Soul Weapon, Magic, or world state changed outside its owner;
- current-life effort bypassed.

Validation confirms trace and owner consistency. It does not recalculate specialist outcomes from a universal formula.

## Identity and Reincarnation Validation

Verify:

- one stable Soul identity links distinct Incarnations;
- one Incarnation has one valid active-body state at a time;
- Final Death, Life Reconciliation, Interlife, and Embodiment order is valid;
- revival and Reincarnation do not both complete for one life;
- ordinary Inventory, office, reputation, and world-bound Knowledge do not transfer automatically;
- eligible Soul Imprints persist only through their owners;
- Echoes, Titles, Resonance, Depth, Constellations, Avatars, Retained Instincts, Soul Weapons, Strain, and Wounds have valid provenance;
- no conflicting report duplicates a Soul, death, Echo, or Soul Weapon;
- Personal and Soul Chronologies remain distinct.

## Inventory and Infrastructure Validation

Detect:

- duplicate unique items;
- quantity mismatches;
- one ordinary object in incompatible locations at one time;
- custody confused with ownership, access, or legal claim;
- consumption or destruction without event and history;
- container cycles or impossible capacity where an owner defines it;
- Soul Weapons reduced to ordinary Inventory;
- Infrastructure functioning without required people, knowledge, resources, maintenance, location, or source;
- destroyed or inaccessible infrastructure still granting capability.

## Autonomous Registry Validation

Detect:

- duplicate Autonomous IDs or one continuing subject active under both ordinary Entity and Autonomous Registry ownership;
- Model, Individual, and Group identity collapsed or unresolved;
- creator silently treated as current Controller;
- unresolved Controller, placement, assignment, Model, lineage, Network, Infrastructure, Relationship, capability, requirement, or event references;
- Individual quantity other than one;
- Group quantity changes without traces or promoted Individuals still counted anonymously;
- restoration reusing identity without continuity evidence or reconstruction silently inheriting identity;
- copies or forks sharing one current identity or lacking source and divergence provenance;
- memory-continuity claims without source, coverage, gap, or uncertainty;
- network membership treated as automatic shared control, Perspective, knowledge, or memory;
- last-confirmed state silently treated as verified current state;
- unknown autonomous state converted into destruction, completion, location, condition, loyalty, or control without Canon;
- autonomous Infrastructure duplicating Infrastructure-owned physical or operational state.

Report ambiguous identity, continuity, and category conflicts for owner review. Validation never repairs, merges, forks, promotes, destroys, or reclassifies autonomous subjects automatically.

## Projects and Mysteries Validation

Verify:

- Project objectives, participants, authority, methods, resources, dependencies, work, state, outputs, and Review Points;
- elapsed time alone did not complete work;
- closed Projects have a completion, abandonment, failure, transfer, or interruption cause;
- Mystery questions are bounded;
- clues retain source and Knowledge;
- false leads remain distinguishable from world truth;
- unknown answers were not invented retroactively;
- resolved Mysteries link the confirmation or correction route;
- duplicate discoveries and Projects are not created by retries.

## Species Locations Factions and World Validation

Verify:

- Species claims retain scope, evidence, form, population, and Evolution ownership;
- local observations do not become universal species rules;
- Locations retain stable identity through naming, destruction, reconstruction, occupation, and movement;
- factions retain Versions, participants, interests, authority routes, internal disagreement, and continuity claims;
- settlements and factions are not duplicated by labels;
- World Engine changes have causes, scope, counterforces, temporal progression, and Review Points;
- off-screen changes do not center on player convenience;
- Simulation Abstraction changed detail rather than truth;
- Time Skips, Age Transitions, and World Resets preserve causality and survivorship;
- current campaign world values remain external to Repository Canon.

## Continuity Drift

**Continuity Drift** is cumulative divergence between authoritative persistence and the claims actually used in narration, adjudication, Derived Views, summaries, tools, or participant expectations.

Detect drift by comparing:

- current active Campaign Version with loaded views;
- Session Log and integrated Session Deltas;
- current state with Campaign History and Timeline;
- declared rules profile with adjudication;
- actor recognition and Relationship history;
- Research and Knowledge sources;
- Inventory, location, and numerical traces;
- repository revision and conversion decisions;
- repeated narration claims;
- validation findings left open past Review Points.

Drift may arise from many individually small omissions. Frequency does not make the drift canonical.

Route affected claims through Continuity Resolution, Save Update, Migration, or specialist correction. Preserve Reliance Effects.

## Validation Report

Every run produces a **Validation Report** containing:

- run identity and trigger;
- baseline and versions;
- scope, exclusions, access, and limitations;
- checks executed, skipped, and inconclusive;
- Findings ordered by severity and dependency;
- aggregate outcome;
- affected activation, Migration, Continuity Case, Session, or adjudication;
- required repair owners and routes;
- redaction and Secret-handling notes;
- warning dispositions and Review Points;
- rerun requirements;
- report integrity and retention reference.

Reports are stored outside this rules repository. Audience-specific reports may redact details, but the protected authoritative report retains enough evidence for repair.

## Warning Disposition

A Warning may be carried only when:

- no Blocker or Error remains;
- the risk is bounded and understood;
- authority to accept it is identified;
- affected participants are informed where their choices or safety depend on it;
- an owner and Review Point exist;
- the warning does not permit data loss, Knowledge leakage, Secret exposure, invalid mechanics, identity collapse, silent retcon, or unsupported numerical state;
- the next required rerun is recorded.

Repeatedly renewing a warning without new evidence or repair creates a Continuity Drift finding.

## Validation Procedure

### Life Archive Profile

Life Archive validation checks unique Life IDs, one Life per Incarnation, valid Soul references, coherent known ordinals, no more than one active Life per Soul, no finalized summary for an active Life, valid completion basis, resolvable typed historical references, preserved unknowns, source provenance, and non-competing Historical Snapshot ownership. It also checks that player-visible summaries have not become Character Knowledge and that the Life Archive has not been conflated with the metaphysical Akashic Archive. Ambiguous recovered identities are findings, never auto-repaired facts.

### Retained Cross-Life Development Profile

Validate source Life/Soul consistency, capability and embodiment references, meaningful historical evidence, Familiarity Unlock versus Development Boost, Zero versus Unknown relevance, additive bounded stacking, Historical Territory tapering, prerequisites, assessment provenance, supersession, and Derived-profile freshness. Current attributes and Skill levels must not be copied from historical peaks; recalculation must not mutate source history; retained familiarity must not create Character Knowledge.

### Skill Consolidation and Historical Scope Profile

- confirm each active Skill and predecessor reference resolves through stable identity;
- confirm superseded predecessors remain Historical and do not compete as current Skill owners;
- confirm overlapping Development evidence is counted once while distinct evidence and peaks remain traceable;
- confirm lineage preserves source Lives, embodiments, expressions, Evolution, Fusion, and translation provenance;
- confirm resulting scope is supported by historical capability rather than labels alone;
- confirm ambiguous or mechanically distinct Skills remain separate;
- confirm current embodiment, access, prerequisites, costs, and authority remain enforced;
- confirm player-facing scope does not leak protected history or undiscovered capability.


1. **Identify trigger.** Save, Migration, Continuity Case, load, recovery, or periodic audit.
2. **Freeze the baseline.** Record exact versions, scope, and integrity.
3. **Select profile.** Include every required check and declare exclusions.
4. **Establish access.** Load only necessary protected data and define redaction.
5. **Validate structure.** IDs, modules, versions, references, ancestry, and atomicity.
6. **Validate authority.** Owners, truth layers, rule profiles, and supersession.
7. **Validate continuity.** state, history, Session, chronology, Relationships, identity, and consequences.
8. **Validate information.** Knowledge routes, Research, theories, Secrets, and Meta separation.
9. **Validate mechanics.** numerical traces and specialist-owner provenance.
10. **Detect drift and duplication.** compare baseline, prior reports, and material views.
11. **Record findings.** Never mutate the baseline.
12. **Determine outcome.** Pass, Pass With Warnings, Fail, or Inconclusive.
13. **Route repair.** Save Update, Continuity Resolution, Migration, specialist owner, authority, or source recovery.
14. **Rerun.** Validate a new candidate after repair.
15. **Activate or stop.** Only valid outcomes proceed within their scope.

## Worked Examples

### Duplicate NPC Names

Two NPC records share a name and profession. Their locations overlap, but their Person Bases, first meetings, and relationships differ.

Validation reports a duplicate-identity candidate, not a merge. The actor owner compares sources and may confirm two people. No state changes until a traceable resolution occurs.

### Duplicate Settlement After Import

An import creates separate records for Old Ford and Fordhaven. Timeline evidence shows one settlement was renamed after a flood.

Validation reports duplicate place continuity with evidence. Migration may perform an Identity Merge preserving both names and the flood transition. The validator does not merge them itself.

### Duplicate Discovery

Three reports claim separate discovery of one spell property. Two copy the same laboratory note; one is an independent experiment.

Validation preserves three Discovery Times and observer Knowledge but flags copied evidence counted twice. Research correction retains one original evidence source plus one independent replication.

### Broken Relationship Continuity

A companion record shows thirty meetings, while the active narrative introduces the companion as unknown and the Relationship record is missing.

Validation reports a Blocker affecting recognition and adjudication. Continuity Resolution determines whether the Relationship record was lost, the view is stale, or an in-world memory effect exists.

### Timeline Cycle

An event says a letter caused a revolt, but chronology places the letter after the revolt and no delayed-discovery model applies.

Validation reports an Error or Blocker depending on downstream use. It does not swap dates. Timeline and source owners determine whether the date, causal link, or identity is wrong.

### GM Secret Leak Through an Index

A player-visible NPC index includes a hidden cult identifier in an inverse-link field.

Validation blocks activation, contains the audience-visible view, and emits a redacted finding. Repair removes the unauthorized route while preserving the protected Secret and any Reliance Effects caused by exposure.

### Contradictory Theories

Two active theories explain a disease differently and both are `Supported`.

This is valid competition, not an Error. Validation fails only if the records were silently merged, evidence duplicated, one marked Confirmed without owner approval, or the contradiction omitted from the Research view.

### Invalid Strength Increase

A Save Transaction raises Strength without an event, Development owner, or Numerical Change Trace.

Validation blocks activation. The GM cannot invent training to explain it. The transaction must remove the unsupported change or route an actual missing event through Continuity Resolution.

### Continuity Drift Across Summaries

Several summaries gradually describe a former rival as a trusted friend, but no Relationship Events support the change.

A periodic audit reports drift between Derived Views and the Relationship owner. The summaries are corrected; trust does not change merely because the error was repeated.

## Safeguards

- Validate before every Save Point activation.
- Validate after every Migration and before target activation.
- Validate Continuity corrections before resuming affected play.
- Freeze an immutable baseline for each run.
- Report findings without modifying state.
- Route repairs through owners and create a new candidate.
- Treat duplicate names as signals, not proof.
- Detect duplicate NPCs, settlements, discoveries, and other stable identities.
- Detect broken, dangling, and orphaned records without deleting them.
- Validate Relationship, Timeline, Research, theory, numerical, identity, Inventory, Project, Mystery, Species, Location, faction, and world continuity.
- Treat Knowledge and GM Secret leaks as blocking.
- Preserve legitimate disagreement, uncertainty, competing theories, and observer perspectives.
- Detect Continuity Drift without promoting repeated error.
- Never invent values, sources, events, repairs, or retcons.
- Never override specialist owners or player agency.
- Never put populated Validation Reports or campaign state in this repository.

## Scope Boundaries

This document defines campaign persistence validation profiles, baselines, severity, outcomes, findings, defect classes, reports, dispositions, and repair routing. It does not implement storage-specific validators, repository release audits, specialist formulas, populated validation records, campaign templates, or automatic repair.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Persistence Levels](PERSISTENCE_LEVELS.md)
- [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md)
- [Research Engine](RESEARCH_ENGINE.md)
- [Timeline Engine](TIMELINE_ENGINE.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Continuity Resolution](CONTINUITY_RESOLUTION.md)
- [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
