# Persistence Levels

## Purpose

This document defines how long campaign records are intended to survive, who owns their retention, when they may be deleted or archived, how they migrate, and what permits material to move between lifetimes.

Persistence Level is a record-lifecycle classification. It does not determine truth, authority, visibility, certainty, importance, or mechanical persistence by itself.

## Core Rule

Every campaign-persistence record uses one of six levels:

1. **Repository**
2. **Soul**
3. **Historical**
4. **Campaign**
5. **Session**
6. **Ephemeral**

A level states the record's intended lifetime and continuity owner. Promotion to a longer-lived level requires the receiving owner's rules, provenance, and validation. Naming or storing a record at a level never grants the corresponding in-world effect.

## Levels Are Independent Classifications

| Classification | Governing question |
| --- | --- |
| [Truth Layer](TRUTH_LAYERS.md) | What kind of informational claim is this? |
| [Persistence Authority](PERSISTENCE_AUTHORITY.md) | Which conflicting campaign-fact source governs? |
| **Persistence Level** | How long should this record survive and under whose continuity? |
| Visibility | Who may access this representation? |
| Certainty | How well supported is the claim? |
| Canonical owner | Which game system defines the represented mechanic? |

A false Rumour can be Historical because its circulation caused lasting consequences. A confirmed current injury can be Campaign because it persists only while the body and condition do. A Soul Echo record can be Soul only after the Soul Engine establishes that Echo. A private Meta safety note may be Session or longer under participant policy without becoming campaign truth.

## Common Level Contract

Every non-Ephemeral record identifies:

- **Persistence Level**;
- **lifetime start**;
- **retention owner**;
- **retention condition**;
- **end, archive, or review condition**;
- **deletion authority**;
- **promotion and demotion provenance**;
- **migration treatment**;
- **dependencies that must survive with it**;
- **privacy or access constraints**.

Ephemeral material may use a lighter contract, but its producer must still know when a Materiality Check is required before disposal.

## Repository Level

### Lifetime

Repository-level material survives as part of Eternal Cycle's reusable rules and governance across campaigns. Its continuity follows repository versions and commit history rather than one playthrough.

### Ownership

Repository-level records are owned by the canonical repository workflow:

- playable rules under `docs/`;
- accepted governance under `design/`;
- canonical terminology;
- reusable unpopulated templates when their roadmap phase is complete;
- repository validation and release records.

Populated campaign data can never be Repository level.

### Deletion and Archiving

Repository content changes through the governed repository process. Removal from the current version does not rewrite prior versions or campaign rules profiles. Version history preserves provenance according to the repository implementation.

### Migration

Repository migration means changing repository representation, structure, or version while preserving canonical meaning and history. Campaigns adopt a repository revision through their own migration; repository content is referenced rather than copied into campaign truth.

### Promotion

Campaign observations, Provisional Rules, feedback, or repeated play never promote themselves to Repository level. They may become design inputs and enter only through the roadmap, decision, review, validation, and commit process.

## Soul Level

### Lifetime

Soul-level campaign records survive Final Death and may continue through Interlife, Reincarnation, Ages, and World Resets only to the extent established by the [Soul Engine](../soul/README.md) and other relevant owners.

Soul-level retention is campaign data. It does not belong in the rules repository.

### Ownership

The Soul Engine owns whether an identity, Imprint, memory, Skill history, Stat XP, Soul Echo, Soul Title, Soul Resonance, Soul Depth, Retained Instinct, Soul Constellation, Soul Space state, Soul Avatar relationship, or Soul Weapon bond is eligible to persist.

The persistence architecture records the established result. It cannot classify ordinary bodily or worldly material as Soul in order to preserve it.

### Deletion and Archiving

Deleting a storage record cannot cause Soul harm. In-world loss, suppression, fracture, erasure, or annihilation requires its canonical source, warning, resistance, scope, and consequence rules.

Conversely, an accidentally deleted save record does not mean the soul forgot. It creates a Record Gap requiring backup recovery, audit, or explicit reconstruction.

Obsolete access snapshots may be archived after a new incarnation is integrated, but the persistent Soul identity and protected history remain resolvable.

### Migration

Migration preserves Soul identity, incarnation links, provenance, persistence-versus-access distinctions, sealed or unknown states, harm, bond consent, and all relevant ownership. It cannot flatten inaccessible Imprints into current capability or merge distinct souls.

### Promotion

Only a canonical Soul process can move an eligible current-life outcome into Soul-level persistence. Examples include Life Reconciliation or an established Soul Weapon passage. Session completion, dramatic importance, player preference, a database flag, or a GM summary cannot do so.

## Historical Level

### Lifetime

Historical-level records survive for the remaining life of the campaign continuity because later state, knowledge, relationships, claims, and consequences may depend on them.

They include material events, corrections, discoveries, migrations, disclosures, losses, transformations, and other history required to explain current or later reality.

### Ownership

Campaign History, Timeline, Migration History, and other explicitly append-only owners retain Historical records. Specialist systems remain owners of represented mechanics.

### Deletion and Archiving

Historical facts are not deleted because they became old, inconvenient, resolved, or irrelevant to the current scene.

They may be:

- compacted into an archive while retaining IDs and causal summaries;
- partitioned by Age, region, or period;
- superseded by traceable correction;
- access-redacted under an authorized privacy requirement while preserving an integrity marker;
- removed from an active working set while remaining recoverable.

An authorized retcon appends its authority and supersession; it does not silently erase the former account.

### Migration

Migration preserves event identity, chronology, causes, sources, corrections, disclosure, affected records, and dependency links. Compression cannot remove an event needed to justify a current number, relationship, possession, discovery, identity, or consequence.

### Promotion

A Session event becomes Historical only after it is established, integrated, and validated. Campaign state that ends may produce a Historical closure event and archive, but the mutable state record itself need not be copied wholesale into history.

## Campaign Level

### Lifetime

Campaign-level records persist across interactions and sessions while their subject, state, obligation, project, relationship, or process remains part of the campaign continuity.

They represent current or durable campaign conditions that may change through play.

### Ownership

The appropriate [Persistence Module](STRUCTURED_PERSISTENCE_ARCHITECTURE.md) owns each Campaign-level record. Examples include current bodies, locations, relationships, inventories, projects, institutions, world conditions, active research, knowledge, and unresolved consequences.

### Deletion and Archiving

A Campaign record leaves the active set only through an established end condition, merge, correction, retention policy, or archival transition.

Before removal:

1. establish what ended or changed;
2. preserve any material Historical event;
3. update dependents and references;
4. retain stable identity where future recognition is possible;
5. preserve unresolved consequences;
6. validate no dangling references remain.

Absence from the current state does not imply that the subject never existed.

### Migration

Migration preserves the latest authoritative state, its history link, subject identity, current owner, uncertainty, visibility, and dependencies. Stale summaries do not override later Historical events.

### Promotion

Session changes become Campaign level through the Save Update Protocol. Ephemeral descriptions become Campaign level only when they establish a material fact through valid GM authority and are captured in the Session Delta first.

Campaign-level bodily, social, material, or world state cannot move to Soul level without a Soul-system route.

## Session Level

### Lifetime

Session-level records begin after the last confirmed save integration and survive until the current interaction's established changes are either:

- integrated and validated;
- explicitly rejected as drafts or invalid outcomes;
- preserved for source recovery;
- superseded by an authorized correction.

A Session is a logical update boundary, not necessarily one calendar meeting or chat thread.

### Ownership

Current Session owns:

- player intents and clarifications;
- adjudication traces;
- established immediate outcomes;
- Session Deltas;
- new uncertainty;
- provisional facts pending integration;
- source transcript references;
- rejected branches and drafts when needed for audit.

### Deletion and Archiving

Session material cannot be discarded merely because narration moved on.

After a successful save update:

- established events route to Historical records;
- current changes route to Campaign records;
- eligible Soul changes route only through Soul owners;
- useful session metadata may enter a Session Log;
- purely duplicative working material may be deleted according to policy;
- source material needed for audit remains referenced or retained.

Failed or interrupted integration leaves the Session pending. It does not revert established play automatically.

### Migration

A campaign with an open Session migrates only when the pending delta is explicitly included, safely isolated, or first integrated. The destination must distinguish confirmed baseline state from unintegrated changes.

### Promotion

Promotion from Session is a routed integration, not one blanket status change. Each changed claim goes to its Authoritative Record Owner and appropriate Historical, Campaign, or Soul level. Validation must pass before the new save is confirmed.

## Ephemeral Level

### Lifetime

Ephemeral material exists only long enough to support immediate presentation, calculation, search, drafting, or decision framing. It may disappear after use if it has no material continuity value.

Examples include:

- temporary formatting;
- discarded wording variants;
- cache entries;
- unselected, unsupported generator seeds;
- intermediate calculations reproducible from authoritative inputs;
- transient UI state;
- descriptions with no material distinguishing effect;
- working queries.

### Ownership

The producing GM, tool, or interface owns Ephemeral material until disposal or Promotion Gate review.

### Deletion

Ephemeral material may be deleted after a **Materiality Check** confirms that it did not establish or materially affect:

- identity;
- player choice;
- adjudication;
- resource or numerical state;
- relationship;
- knowledge;
- chronology;
- project or obligation;
- world condition;
- consequence;
- uncertainty that must remain fair;
- a later dependency.

If any applies, the material must enter Session level through the correct owner before disposal.

### Migration

Ephemeral material normally does not migrate. If required to explain an open Session, reproduce a calculation, preserve participant consent, or audit a disputed result, it is first promoted to an appropriate Session or Meta record with provenance.

### Promotion

Ephemeral material never jumps directly to Campaign Canon or Historical truth. It passes through a **Promotion Gate** that establishes materiality, authority, owner, source, scope, truth layer, and Session record.

## Promotion and Demotion

### Promotion Gate

A **Promotion Gate** asks:

1. What exact claim or record must survive longer?
2. What event or authority established it?
3. Which module and canonical system own it?
4. What truth layer and visibility apply?
5. What target Persistence Level is justified?
6. What provenance and dependencies must travel with it?
7. Does the receiving owner accept it?
8. Has validation passed?

Promotion preserves the source record or a traceable reference. It cannot launder an unsupported claim into truth.

### Demotion

Long-lived records are not demoted merely because they are inconvenient to load.

Valid reductions include:

- moving Historical records from active storage to a resolvable archive;
- replacing Campaign state with a closure event after the condition ends;
- removing integrated working copies from Session;
- deleting Ephemeral caches;
- applying an authorized privacy-retention policy with integrity and dependency review.

Demotion never changes in-world truth by itself.

## Deletion Is Not In-World Erasure

Storage deletion, archival, redaction, forgetting, destruction of an in-world record, memory loss, Soul harm, and an authorized retcon are different events.

| Change | Owner |
| --- | --- |
| Storage cleanup | Persistence implementation and retention policy |
| Campaign record archival | Persistence Level and module owner |
| In-world document destruction | World and item owners |
| Character forgetting | Memory and Knowledge owners |
| Soul-memory loss | Soul Engine |
| Historical correction | Persistence Authority and continuity procedures |
| Retcon | Campaign Canon authority |

No implementation operation may impersonate an in-world mechanic.

## Privacy and Required Removal

Participant privacy, safety, or legal requirements may require deleting or redacting stored information. The campaign should preserve continuity with the least revealing integrity marker possible, such as:

- a stable redacted Record ID;
- an access-restricted deletion notice;
- a statement that material is unavailable;
- affected dependency references;
- the authority and date of removal.

Privacy removal does not authorize reconstructing sensitive content from inference. If continuity cannot be maintained safely, record the resulting gap honestly.

## Cross-Level Dependencies

Longer-lived records may depend on shorter-lived current state, but the dependency must remain intelligible after that state changes.

Examples:

- a Historical battle event references the then-current body through an Incarnation ID;
- a Soul Echo references a former Incarnation and Final Death event;
- a Campaign relationship references Historical promises;
- a Session injury delta references the current body and owning mechanics;
- an Ephemeral calculation references Session inputs but is discarded after its result and method are recorded.

Archiving one level cannot create dangling identities in another.

## Worked Examples

### Ordinary Sword and Soul Weapon

An ordinary sword is Campaign-level Inventory. If destroyed, its loss becomes Historical and it leaves current Inventory.

A bonded Soul Weapon's vessel location may be Campaign level, its shared events Historical, and its eligible Weapon Soul and Bond continuity Soul level. The labels record distinct established claims; they do not make the weapon indestructible or always accessible.

### Final Death During an Open Session

The death event, Life Reconciliation, losses, surviving worldly effects, and eligible Soul changes begin in Session. Integration routes the death to Historical, closes Campaign-level body state, updates relationships and world consequences, and moves only canonically eligible Imprints to Soul-level records.

Nothing is promoted merely because the scene was dramatic.

### Temporary Combat Estimate

A tool estimates whether a bridge can bear a charge. Intermediate arithmetic is Ephemeral. The declared inputs, resolution method, and established collapse or survival outcome are Session. After save integration, the event is Historical and the bridge's current condition is Campaign.

### Archived Settlement

A settlement is abandoned and no longer needs an active detailed Profile. Its identity, destruction, displacement, relationships, and consequences remain Historical. A lightweight Campaign reference may remain if ruins, claims, routes, or return are still possible.

The record is archived, not erased from reality.

## Safeguards

- The six levels classify lifetime, not power, truth, authority, visibility, or importance.
- Populated Repository-level campaign data is forbidden.
- Soul level requires an established Soul-system persistence route.
- Storage deletion cannot cause in-world erasure or Soul harm.
- Historical records are append-only except for traceable correction and retcon.
- Campaign records close only after history, dependents, and unresolved consequences are preserved.
- Session changes survive until integration, rejection, correction, or recovery is explicit.
- Ephemeral material undergoes a Materiality Check before disposal.
- Promotion passes through the receiving owner and validation.
- Migration preserves levels and open Session boundaries.
- Archives retain resolvable identities and dependencies.
- Privacy removal creates no permission to infer or reconstruct protected content.
- No level grants progression, capability, ownership, consent, or success.

## Scope Boundaries

This document defines lifetime, ownership, deletion, migration, promotion, and demotion. It does not define the complete Campaign State schema, module-specific lifecycles, migration stages, continuity repair, save-update transaction, validation algorithms, or templates.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Campaign Persistence Philosophy](CAMPAIGN_PERSISTENCE_PHILOSOPHY.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Soul Engine](../soul/README.md)
- [Reincarnation](../soul/REINCARNATION.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
