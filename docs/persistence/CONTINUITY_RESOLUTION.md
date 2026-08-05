# Continuity Resolution

## Purpose

This document defines how an Eternal Cycle campaign identifies, contains, classifies, resolves, records, validates, and communicates conflicts among narration, Session material, Campaign State, Historical Record, Campaign Canon, and Repository Canon.

Continuity Resolution protects causality and player agency. It corrects the campaign's representation without pretending the newest statement, most convenient summary, or loudest participant automatically controls what is true.

## Core Rule

When narration conflicts with persistence:

1. stop using the conflicting narration;
2. inspect Repository Canon;
3. inspect Campaign persistence;
4. classify the conflict;
5. resolve it through the authority hierarchy;
6. correct only the affected dependency closure;
7. validate before continuing from the disputed claim.

The GM must never defend incorrect continuity merely because it appeared most recently.

## Continuity Resolution Is Not

Continuity Resolution is not:

- a license for silent retcons;
- preference for newer prose over structured state;
- permission to invent missing facts;
- a way to remove inconvenient consequences;
- a global pause when only one claim is affected;
- proof that every conflicting belief is a record error;
- an opportunity to rebalance progression secretly;
- automatic restoration of a player's preferred interpretation;
- permission to expose GM Secrets;
- a substitute for Migration and Versioning;
- a populated continuity log stored in this repository.

## Continuity Conflict

A **Continuity Conflict** exists when two or more claims that cannot all govern the same subject, property, scope, and effective time appear to carry authority in play or persistence.

Apparent disagreement is not always a conflict. The following may coexist:

- world truth and a mistaken belief;
- two observer perspectives;
- a Rumour and its actual truth value;
- a plan and its later outcome;
- a former state and a current state separated by an event;
- approximate and exact claims covering different scopes;
- disputed historical testimony;
- a deliberate lie and the liar's Knowledge;
- a Soul's continuity and separate Incarnation identities.

Classify claims by owner, truth layer, subject, temporal scope, visibility, and status before deciding they conflict.

## Continuity Case

Every material resolution uses a **Continuity Case** stored in the external campaign records.

It records:

- Continuity Case ID;
- detection time and detector;
- exact conflicting claims;
- subjects, properties, scopes, and effective times;
- sources and versions;
- authority levels and Authoritative Record Owners;
- truth layers and visibility;
- loaded Read Set and dependency closure;
- immediate Continuity Freeze;
- conflict classification;
- evidence and uncertainty;
- participant choices or outcomes that relied on each claim;
- proposed and accepted resolution;
- required authorization;
- Correction Events, migration, or save updates;
- Knowledge and communication effects;
- validation results;
- warnings and unresolved questions;
- closure or Review Point.

A trivial presentation typo that changes no claim may be fixed through ordinary version history. A material conflict requires a Continuity Case.

## Continuity Freeze

A **Continuity Freeze** temporarily prevents the disputed claim and its unvalidated dependents from being used for further adjudication.

The freeze is narrow:

- unaffected play may continue when independence is established;
- existing world processes continue unless they depend on the disputed claim;
- no affected numerical value, identity, location, Relationship, Knowledge state, Project, or Timeline relation changes until resolved;
- hidden information remains protected;
- no source record is deleted or rewritten during diagnosis.

If safe independence cannot be demonstrated, pause the scene or Session at the nearest valid boundary. Do not improvise through a conflict and promise to repair it later.

## Required Read Set

Before resolving, load as applicable:

1. Repository Version and relevant Repository Canon;
2. Campaign Canon and active rules profile;
3. Historical Record and Campaign History;
4. active Campaign Version and Save Index;
5. Current Campaign State and Authoritative Record Owners;
6. Pending Session Deltas and Session Log;
7. Timeline Events and temporal scopes;
8. relevant Player, Soul, Incarnation, actor, Relationship, Species, Location, Project, Research, Knowledge, Secret, Inventory, and world records;
9. Migration Manifests, Correction Events, Provisional Rules, and authorized retcons;
10. source transcripts or other evidence needed to diagnose the exact claim.

Load the smallest dependency closure sufficient to resolve the conflict. Missing dependencies remain `Requires Source Recovery`; they are not reconstructed from narrative preference.

## Conflict Classifications

### Narration Error

A **Narration Error** is a lower-authority statement presented as current fiction that conflicts with established higher-authority continuity and lacks a valid event or decision changing it.

Examples include:

- calling a known NPC a stranger;
- placing a destroyed bridge intact without reconstruction;
- describing an Inventory item that was consumed;
- giving a body a former incarnation's anatomy;
- asserting a spell succeeded despite an already established failure;
- moving a character to a location with no travel event.

Resolution normally:

1. identifies the established claim;
2. withdraws the conflicting narration;
3. restates the scene from the last valid boundary;
4. preserves any Reliance Effects requiring fair remedy;
5. records a Correction Event when material;
6. updates no higher-authority state unless a separate valid event occurred.

Repeated narration does not become canon through repetition.

### Outdated Save

An **Outdated Save** is a valid older Campaign Version, Snapshot, Derived View, cache, or export presented as if it were the current authoritative state.

Resolution compares:

- Campaign Version IDs and parentage;
- Save Index;
- included Session Deltas and migrations;
- Integration Times;
- validation state;
- pending and recovery status;
- records changed after the older Save Point.

The newer timestamp does not automatically win. The active validated Campaign Version and authority chain determine current state.

If legitimate play continued from an outdated save, do not discard it casually. Classify the branch, preserve its Session material, and use [Migration and Versioning](MIGRATION_AND_VERSIONING.md) or an authorized branch-resolution process.

### Incomplete Information

**Incomplete Information** exists when the available record cannot responsibly settle the claim.

Use explicit labels:

- `Unknown`;
- `Not Yet Verified`;
- `Estimated`;
- `Requires Source Recovery`;
- `Player Theory`;
- `Rumour`;
- `Disputed` where supported accounts conflict.

Resolution may:

- recover sources;
- ask the campaign authority or participant who owns the missing choice;
- investigate in world;
- defer the claim;
- make a bounded Provisional Ruling where allowed;
- preserve several supported alternatives;
- continue only along branches independent of the missing fact.

Never invent data to fill gaps. Schema completeness, narrative smoothness, balance, genre expectation, and likely intent are not evidence.

### Authorized Retcon

An **Authorized Retcon** is an explicit Campaign Canon decision that intentionally replaces or reinterprets an established campaign claim within a stated scope.

It records:

- authorizing campaign authority;
- reason;
- original claim;
- replacement claim;
- effective and historical scope;
- participants consulted;
- meaningful choices affected;
- facts and consequences preserved;
- facts and consequences changed;
- Knowledge and memory treatment;
- Timeline Correction Events;
- version or migration requirements;
- validation.

A retcon is not authorized merely because the GM prefers it, a repository rule changed, a new summary contradicts the old one, or all participants have not objected.

Retroactive changes that materially alter player decisions, consent, identity, relationships, loss, success, progression, or safety require explicit discussion with affected participants under the campaign's authority agreement.

### Intentional In-World Deception

**Intentional In-World Deception** exists when an actor deliberately communicates, stages, conceals, falsifies, impersonates, edits, or otherwise presents a claim they believe will mislead another observer.

The engine distinguishes:

- world truth;
- deceiver's Knowledge and intent;
- deceptive act and source;
- target's observations and beliefs;
- evidence and detection routes;
- social, legal, political, Research, and Relationship consequences;
- GM Secret visibility where relevant.

A lie is not a continuity error because it conflicts with world truth. The deceptive event and resulting beliefs can both be valid Campaign Canon.

Do not retroactively classify an accidental narration mistake as deception to protect the narration. Deception requires an established actor, opportunity, method, intent, and causal route.

### Additional Diagnostic Classes

The five primary classifications may expose a narrower technical cause:

- migration fault;
- duplicate identity;
- incorrect owner;
- temporal mismatch;
- Derived View or cache drift;
- Provisional Rule conversion conflict;
- numerical transcription error;
- unauthorized edit;
- valid state change missing its event record.

These causes route to the primary classification and appropriate repair. They do not bypass authority.

## Authority Resolution

Apply the canonical authority order:

1. Repository Canon;
2. Campaign Canon;
3. Historical Record;
4. Current Campaign State;
5. Current Session;
6. Current Narration.

Higher authority constrains lower authority but does not silently rewrite it. If the conflict crosses Repository Canon and established Campaign Canon, determine whether the campaign is compatible, requires conversion, needs adjudication, requires an Authorized Retcon, or remains on an older rules profile.

Within one authority level:

1. identify the Authoritative Record Owner;
2. compare source provenance and version;
3. compare temporal and subject scope;
4. distinguish Confirmed, Pending, Stale, Disputed, Unknown, Estimated, Superseded, and Archived states;
5. follow explicit supersession and Correction Events;
6. preserve unresolved conflict when evidence remains insufficient.

Recency, vividness, repetition, narrative importance, secrecy, file order, storage product, and confidence of phrasing create no authority.

## Resolution Procedure

### 1. Detect and State the Conflict

Quote or identify the exact incompatible claims without revealing protected content to unauthorized participants. State which current adjudication depends on them.

### 2. Apply the Continuity Freeze

Freeze the disputed claim and affected dependents. Establish the last valid boundary from which unaffected play can continue.

### 3. Open the Continuity Case

Assign a stable Case ID, record sources, versions, authority, scope, and current risks, and preserve the original statements.

### 4. Load Authority and Dependency Closure

Read Repository rules first, then Campaign persistence in authority order. Follow typed references to all material owners.

### 5. Classify

Classify the issue as Narration Error, Outdated Save, Incomplete Information, Authorized Retcon, Intentional In-World Deception, or a supported combination. Record any narrower diagnostic cause.

### 6. Determine the Authoritative Baseline

Identify the last validated claim or explicitly unknown state that can govern resolution. A baseline may be a Campaign Version, Historical event, Current State claim, pending Session boundary, or unresolved set of alternatives.

### 7. Assess Reliance and Impact

Identify every material choice, consent decision, resource use, injury, progression change, relationship event, discovery, missed opportunity, downstream simulation, and participant expectation that relied on the conflicting claim.

### 8. Select the Narrowest Valid Remedy

Choose among:

- narration withdrawal and restatement;
- correction of a Derived View, cache, or transcription;
- source recovery;
- preservation as Unknown or Disputed;
- state update for a valid but unrecorded event;
- migration or branch merge;
- Provisional Ruling;
- Authorized Retcon;
- in-world continuation of deception and its consequences;
- rollback to a valid boundary with fair treatment of reliance.

### 9. Obtain Authorization

Use Campaign Canon authority for retcons, branch choices, conversion decisions, and remedies affecting meaningful player choices. Use specialist owners for mechanical claims. Use player authority for their unrecorded deliberate choices.

### 10. Apply Source-Preserving Corrections

Create Correction Events, update Authoritative Record Owners, preserve original claims and supersession, update Timeline and Campaign History, and change only the affected dependency closure.

### 11. Validate

Validate authority, identity, chronology, references, numerical traces, Relationships, Knowledge, Secrets, Projects, Research, Inventory, specialist mechanics, player agency, and unaffected records.

### 12. Communicate and Resume

Explain the player-relevant correction, what remains uncertain, and the valid point of resumption without leaking hidden truth. Close the Case or set a Review Point.

## Reliance Effects

A **Reliance Effect** is a material choice, cost, lost opportunity, interpretation, or downstream consequence produced because a participant or system reasonably acted on a presented claim.

Correcting the underlying claim does not automatically erase Reliance Effects.

Possible remedies include:

- rewind to the last valid decision point when practical and accepted;
- preserve a choice but recalculate only invalid mechanical effects;
- restore a spent resource when its expenditure was caused solely by GM error;
- offer a new informed choice where consent or agency was undermined;
- preserve an in-world consequence while correcting its attributed mechanism;
- record an Authorized Retcon with agreed scope;
- continue with a fair compensating opportunity that follows causality rather than granting unrelated rewards.

Do not use compensation to create progression, wealth, relationships, or success unrelated to the actual harm. Do not force a rewind that removes a player's preferred valid choice without consultation.

## Numerical State

Numbers change only through mechanically justified events.

Every correction to numerical state requires a [Numerical Change Trace](CAMPAIGN_STATE_MODEL.md#numerical-state) showing:

- subject and owner;
- prior authoritative value;
- conflicting or incorrect value;
- causal event and mechanic;
- inputs and costs;
- correct result;
- affected dependents;
- correction or retcon authority;
- validation.

Never silently invent a number. Never silently adjust progression. Never set an unknown value to zero, average two conflicting values, choose the more balanced result, or grant a bonus because correction is inconvenient.

If the trace cannot be recovered, preserve `Unknown`, `Estimated`, `Disputed`, or `Requires Source Recovery` and constrain adjudication accordingly.

## Relationships and Recognition

Known characters must never be treated as strangers without an in-world explanation.

When a relationship conflict appears, check:

- stable participant and Incarnation identities;
- first and latest meetings;
- Recognition State;
- memory access;
- disguise, transformation, impersonation, or deception;
- communication;
- promises, betrayals, debts, gifts, family, and organizations;
- directional Relationship Dimensions and Trust Trajectories;
- Timeline and source events.

Correction preserves history. It does not average perspectives, force recognition, restore trust, or transfer former-life relationships automatically.

## Reincarnation and Identity

Continuity resolution preserves separate Soul and Incarnation identities.

Common errors include:

- giving a new body former bodily statistics;
- transferring Inventory or social authority;
- treating a former acquaintance as automatically recognizing the new incarnation;
- copying world-bound Character Knowledge into the Soul;
- duplicating a Soul during conflicting death reports;
- allowing revival and Reincarnation to complete for the same life;
- erasing former-life consequences after a new Embodiment.

Resolve through Reincarnation, Campaign State, Relationship, Knowledge, Timeline, and Soul owners. Do not fix identity conflicts by collapsing lives into one character record.

## Knowledge and Secrets

Resolution distinguishes what the GM corrects from what characters discover.

- A Narration Error may be corrected out of character without creating an in-world memory.
- An Outdated Save correction may change Current State without informing every actor.
- Incomplete Information remains unknown to the relevant observer.
- an Authorized Retcon states how memories, records, and evidence are treated;
- Intentional In-World Deception remains discoverable through valid routes;
- GM Secrets stay protected unless the resolution authorizes disclosure.

Do not reveal the true murderer merely to explain that an NPC's accusation was a lie. Explain only enough to restore valid play.

## Current Session and Save Boundaries

If the conflict occurs before a Session Delta is integrated:

- preserve the Pending Session material;
- correct or invalidate only affected claims;
- retain valid choices and outcomes;
- update the Session Log;
- validate before the next Save Point.

If the conflict is discovered after integration:

- do not edit the active Campaign Version in place silently;
- create Correction Events and a new Campaign Version through the appropriate save or migration route;
- preserve ancestry and former records;
- assess intervening Reliance Effects.

The [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md) owns ordinary transactional integration. [Migration and Versioning](MIGRATION_AND_VERSIONING.md) owns structural, version, import, and broad conversion changes.

## AI and Human GM Guidance

A human or AI GM resolving continuity should:

- search canonical rules before inventing an explanation;
- load structured campaign records rather than relying on conversation memory;
- state uncertainty and source gaps honestly;
- prefer the narrowest correction;
- distinguish fact correction from Character Knowledge;
- ask for clarification when a player-owned choice is missing;
- avoid defending prior output for consistency's sake;
- preserve causal and emotional consequences;
- record Provisional status explicitly;
- never fabricate a citation, source, event, or numerical trace;
- resume from the last valid boundary.

## Worked Examples

### Known Companion Introduced as a Stranger

Narration presents a long-term companion as meeting the party for the first time. Relationship records contain many established encounters, and no disguise, memory loss, Reincarnation, or deception applies.

Classify Narration Error. Withdraw the introduction, restate the scene with valid recognition, preserve any player choices already made in reliance on the error, and create a material Correction Event if the error affected play.

### Old Inventory Snapshot

A character sheet shows a potion consumed last Session. The file has a newer modification time because it was exported later, but its parent Campaign Version predates the consumption event.

Classify Outdated Save or Derived View drift. The active validated Campaign Version governs. Repair the view; do not duplicate the potion or remove another item to balance the count.

### Unknown Cause of a Ruin

The campaign knows a settlement was destroyed but the only detailed source is unavailable. A new scene needs to know whether fire or a monster caused the collapse.

Classify Incomplete Information and mark `Requires Source Recovery`. The GM may describe established damage visible now, but cannot invent the cause. Investigation may create evidence without retroactively making an unsupported answer always known.

### Retconning an Unsafe Scene

Participants authorize removing a scene that violated a campaign safety agreement. The retcon identifies what no longer occurred, which subsequent facts depend on it, what memories and relationships change, and what unaffected choices remain.

This is an Authorized Retcon, not a Narration Error. Record the authority and scope, protect sensitive detail, update the dependency closure, create the next Campaign Version, and validate.

### A False Royal Order

An NPC forges a royal command, and a garrison acts on it. The order conflicts with the monarch's actual decision.

Classify Intentional In-World Deception, not continuity failure. Preserve the monarch's truth, the forgery event, the garrison's belief, orders issued, casualties, evidence, and later discovery routes. If narration had instead accidentally called a genuine order forged with no supporting event, that would be a Narration Error.

### Incorrect Strength Value

Two summaries list different current Strength values. Neither is authoritative, but the Development Profile and Session events preserve training, injury, recovery, and the last validated value.

Rebuild the Numerical Change Trace from the owner. If the trace supports one result, correct the views. If a required event is missing and unrecoverable, mark the value `Requires Source Recovery`; do not average the numbers.

### Repository Rule Changes

A new Repository Version changes how a Provisional magical capability is represented. Past play used the old capability to save a settlement.

Do not erase the rescue. Use Migration and Versioning to assess compatibility, preserve valid consequences, remove unsupported duplicate benefit, translate legitimate progress conservatively, and seek authorization for any substantial retroactive change.

## Validation Checklist

Before closing a Continuity Case verify:

- [ ] the exact conflict and scope are recorded;
- [ ] the Continuity Freeze covered all affected dependents and no unrelated claims;
- [ ] Repository Canon and Campaign persistence were inspected in authority order;
- [ ] the primary conflict classification is supported;
- [ ] the authoritative baseline or explicit unknown is identified;
- [ ] original claims and sources remain traceable;
- [ ] every meaningful Reliance Effect was assessed;
- [ ] required player, campaign, and specialist authorization exists;
- [ ] corrections affect only the dependency closure;
- [ ] numerical changes have mechanical traces;
- [ ] Relationships and identities remain coherent;
- [ ] Timeline and Campaign History preserve correction and supersession;
- [ ] Character Knowledge, Rumours, Research, Player Theories, GM Secrets, and Meta remain separate;
- [ ] no hidden information leaked through explanation or metadata;
- [ ] Save Point or Migration status is valid;
- [ ] the resumption boundary is explicit;
- [ ] unresolved questions and Review Points are recorded;
- [ ] no populated Continuity Case entered the rules repository.

## Safeguards

- Stop using conflicting narration before further affected adjudication.
- Search Repository Canon before inventing mechanics or explanations.
- Inspect structured Campaign persistence before relying on conversation memory.
- Recency, repetition, and confidence do not create authority.
- Narration Error, Outdated Save, Incomplete Information, Authorized Retcon, and Intentional In-World Deception remain distinct.
- Missing information remains explicitly unknown.
- Numerical changes require mechanical traces.
- Relationship history never resets silently.
- Reincarnation never collapses Soul and Incarnation identity.
- Corrections preserve original sources, supersession, and unaffected consequences.
- Reliance Effects and meaningful player choices receive explicit review.
- Retcons require authorization and bounded scope.
- Deception requires an in-world actor, method, intent, and causal route.
- Hidden information remains protected during correction.
- Structural conflicts use Migration and Versioning.
- No campaign Continuity Case belongs in this repository.

## Scope Boundaries

This document defines conflict detection, classification, containment, authority resolution, reliance review, correction, and resumption. It does not define specialist mechanics, create campaign authority agreements, specify storage formats, replace Migration and Versioning, implement the full Save Update Protocol, or define all persistence validation algorithms.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md)
- [Timeline Engine](TIMELINE_ENGINE.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md)
- [Alpha Playtest Rules](../gm/ALPHA_PLAYTEST_RULES.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Reincarnation](../soul/REINCARNATION.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
