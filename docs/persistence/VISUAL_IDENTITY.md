# Visual Identity

## Purpose

This document defines the optional campaign-persistence representation for established visual traits whose continuity matters across scenes, sessions, tools, and historical recovery.

A **Visual Identity** is a sparse set of relatively persistent identifying facts about one valid visual subject. It prevents historically established appearance from being lost or replaced by generated improvisation without turning appearance into an exhaustive artistic specification.

## Document Control

- **Owner:** sparse individual or model-specific persistent visual traits, their effective intervals, provenance, uncertainty, revision, and explicit Canon adoption
- **Primary authorities:** [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md), [Campaign State Model](CAMPAIGN_STATE_MODEL.md), and [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- **Dependencies:** stable Entity, body, Incarnation, Autonomous ID, or Model identity; Species and form references; Truth Layers; Timeline; migration; and validation
- **Extensions:** [Canonical Visual Context](../ai/CANONICAL_VISUAL_CONTEXT.md) assembles visual facts with current state for authorized representation tasks
- **Consumers:** GMs, AI runtimes, image-generation handoffs, continuity recovery, Life Archive references, Derived views, and validation
- **Repository boundary:** no populated Visual Identity, image, campaign entity, visual reference asset, migration result, or current appearance belongs here

## Core Invariants

1. Visual Identity is optional and sparse.
2. Unspecified visual detail is not Canon.
3. One established trait has one authoritative owner.
4. Visual Identity stores only subject-specific persistent traits not better owned by Species, form, model, equipment, condition, Location, or another specialist domain.
5. Current Appearance is a Derived projection assembled from current owners; it is not a second mutable record.
6. A generated image is not an authoritative source merely because it exists.
7. Incidental rendering choices never enter Canon automatically.
8. Physical appearance is normally body-, Incarnation-, or Entity-specific rather than Soul-persistent.

## Sparse and Unspecified State

A Visual Identity may establish only:

- `dark grey scales`;
- `four arms`;
- `roughly child-sized`;
- `left horn broken`.

Nothing else is implied. Absence means **Unspecified**, not an ordinary default, a likely answer, a negative fact, or permission to persist whatever an image generator supplies.

Do not create placeholder trait rows merely to complete a schema. When a material detail may exist in inaccessible history, use `Requires Source Recovery` in the recovery process rather than fabricating the trait. When supported sources conflict, preserve `Disputed` or another established uncertainty state until adjudication.

## Visual Identity and Current Appearance

**Visual Identity** owns relatively persistent identifying traits such as:

- individual coloration not already owned by Species or form;
- stable body-plan distinctions;
- persistent markings or scars;
- characteristic morphology unique to the subject;
- permanent modifications;
- stable model morphology when the Model owner establishes it.

**Current Appearance** is a Derived view assembled for a particular time, place, and Perspective from owners including:

```text
current Species and form
  + applicable Model Visual Identity
  + individual Visual Identity
  + current equipment and clothing
  + current condition, injury, and effects
  + temporary transformation
  + carried visible items
  + Location and environmental state
  = Current Appearance projection
```

Do not copy current equipment, clothing, injuries, environmental residue, carried items, temporary forms, or active effects into Visual Identity merely to simplify a prompt. If a temporary change later becomes a persistent physical trait through established Canon, its owner may create or revise the applicable Visual Identity trait with provenance and an effective interval.

## Subject Scope and Identity

A Visual Identity record attaches to one stable visual-subject reference appropriate to the campaign schema, such as:

- a body or Incarnation whose physical identity changes at Reincarnation;
- a persistent Entity whose body continuity is established;
- an Autonomous Individual;
- an Autonomous Model;
- another established subject kind whose owner can preserve identity without duplication.

The record does not create a new person or model identity. Renaming, redesignation, Controller change, assignment change, or movement does not create another visual subject.

Where one Entity may change bodies or forms, record the subject scope and effective interval precisely. A body-scoped trait does not silently follow a Soul, Controller, replacement shell, copied mind, reconstructed unit, or later Incarnation.

## Species, Form, and Evolution Boundary

Species and Evolution owners retain reusable anatomy, ordinary coloration ranges, body plans, Species Traits, form architecture, and Evolution transitions. Visual Identity references those definitions and stores only individual distinctions or explicitly scoped persistent modifications.

When Evolution changes the current form:

1. the current embodiment or form owner changes;
2. form-derived appearance follows the new form;
3. each individual Visual Identity trait is reviewed for continued physical applicability;
4. inapplicable former traits remain Historical rather than being shown as current;
5. new persistent individual traits require an established source.

Do not copy a complete Species or Evolution description into every Visual Identity. Do not let an old Visual Identity override a current canonical form.

## Model and Individual Boundary

Reusable constructs, servitors, artificial life, summoned forms, and similar subjects may use two levels:

- **Model Visual Identity** owns shared model-specific morphology not already owned by Species, Magic, Infrastructure, or another design owner.
- **Individual Visual Identity** owns only individual distinctions, such as a repair patch, broken manipulator, unique marking, or permanent modification.

Many individuals may reference one Model Visual Identity. They do not copy it as authoritative individual state. A model revision does not silently rewrite existing individuals; adoption of a revision follows the Model and lineage rules already in force.

The [Autonomous Registry](AUTONOMOUS_REGISTRY.md) retains the Autonomous ID, Model reference, condition references, and other autonomy-specific state. Visual Identity never creates a parallel identity for the unit.

## Equipment, Condition, and Location Boundary

- **Inventory and Custody** owns equipment, clothing, visible carried items, item condition, and custody.
- **Condition/effect owners** own injuries, wounds, temporary marks, active transformations, suppression, contamination, and other current bodily state.
- **Locations, Infrastructure, and World State** own visible environment, structures, damage, weather, lighting conditions, and place-specific change.

Visual Identity may reference these owners when a persistent modification was caused by them, but it cannot duplicate their mutable current state. A Character view, autonomous record, scene packet, or image prompt may display those facts only as References or Derived content.

Locations and Infrastructure may expose their own established visual descriptors through their existing records. This subsystem does not create a second visual-location database.

## Trait Contract

Each established Visual Identity trait should preserve:

- stable Visual Identity ID and Trait ID;
- visual-subject kind and stable subject reference;
- bounded trait type and exact established content;
- effective start and optional end or supersession;
- persistence scope, such as body, Incarnation, Entity, Individual, or Model;
- authoritative owner;
- source event, adjudication, approved reference, recovery finding, or explicit Canon-adoption transaction;
- Truth Layer, visibility, and GM Secret restrictions;
- certainty or uncertainty state where material;
- revision and validation evidence.

Useful trait categories may include body plan, scale, coloration, marking, scar, morphology, texture, permanent modification, or another implementation-defined visual category. Categories aid retrieval; they do not supply default values.

## Provenance Classes

Use the repository's existing provenance architecture. A visual projection may distinguish origins such as:

- explicitly established gameplay trait;
- Species-derived visible fact;
- form- or Evolution-derived visible fact;
- Model-derived visible fact;
- individual Visual Identity trait;
- equipment- or condition-derived visible fact;
- approved visual reference;
- continuity-recovery finding;
- Unknown, Unspecified, Disputed, or Requires Source Recovery.

These labels describe source routes. Species-, form-, equipment-, condition-, and Location-derived facts remain owned by those domains.

## Historical Visual Recovery

Use targeted recovery when established persistent appearance exists only in historical Canon.

1. **Back up.** Preserve the current canonical campaign source before edits.
2. **Inspect current identity.** Resolve the exact Entity, body, Incarnation, Autonomous ID, or Model and its current owners.
3. **Audit relevant history.** Read only likely Timeline, Campaign History, Session, transcript, approved-reference, and prior-save sources.
4. **Extract supported traits.** Recover only descriptions explicitly supported as persistent facts.
5. **Classify change.** Distinguish actual Evolution, injury, repair, transformation, aging, or modification from contradictory prose.
6. **Preserve uncertainty.** Do not merge incompatible descriptions or choose an aesthetically preferred answer.
7. **Record provenance.** Link each recovered trait to its source and recovery finding.
8. **Route ownership.** Persist the trait under Visual Identity only when it is subject-specific; otherwise update or reference the proper Species, form, Model, equipment, condition, or Location owner.
9. **Validate.** Check identity, effective intervals, references, visibility, and conflicts.
10. **Reject incidental image detail.** A prior generated image contributes only when campaign records explicitly approved it as an authoritative visual reference or adopted a specific trait.

A targeted correction may use an ordinary Save Update when the existing persistence model already represents Visual Identity. Bulk reconstruction, schema adoption, or cross-version import follows [Migration and Versioning](MIGRATION_AND_VERSIONING.md). Do not automatically scan or rewrite every entity.

## Conflicting Descriptions

When historical descriptions differ:

- preserve both intervals when an established physical change explains the difference;
- prefer the properly established current owner for current appearance;
- retain conflicting unsupported claims as `Disputed` or route them through [Continuity Resolution](CONTINUITY_RESOLUTION.md);
- do not silently combine mutually exclusive coloration, anatomy, markings, or other traits;
- preserve reliance and provenance when a prior depiction materially affected play.

Recency, prose vividness, image quality, and repetition do not decide authority.

## Generated Images and Canon Adoption

Generating an image is normally a read-only representation task. The image may contain incidental choices required to render pixels, but those choices are **Non-Canonical Rendering Choices**.

An image becomes an approved visual reference only through explicit campaign authority. Individual details may be adopted without approving the whole image. When a player or GM with authority says, for example, `make that eye pattern canonical`:

1. identify the exact adopted trait;
2. identify its proper owner and subject scope;
3. record the image or decision as provenance without importing unrelated pixels;
4. determine the Affected Set;
5. persist and validate through FR-011.

Never write an incidental generated detail back automatically.

## Reincarnation and Life Archive

Visual Identity does not attach ordinary bodily appearance to the Soul. Reincarnation creates a new embodiment whose appearance follows its own body, Species, form, lineage, and current state.

A former Life's appearance may remain in Timeline, Campaign History, approved references, and [Life Archive](LIFE_ARCHIVE.md) retrieval. A Life Summary may index a historically identifying appearance reference where useful, but it does not make that appearance current or consciously remembered.

## Logical Persistence Contract

Storage technology and physical names are implementation choices. A normalized SQLite campaign may use structures equivalent to:

```sql
CREATE TABLE visual_identities (
    visual_identity_id TEXT PRIMARY KEY,
    subject_kind TEXT NOT NULL,
    subject_id TEXT NOT NULL,
    persistence_scope TEXT NOT NULL,
    record_version INTEGER NOT NULL CHECK (record_version > 0),
    provenance_id TEXT NOT NULL,
    UNIQUE (subject_kind, subject_id, persistence_scope)
);

CREATE TABLE visual_identity_traits (
    visual_trait_id TEXT PRIMARY KEY,
    visual_identity_id TEXT NOT NULL,
    trait_category TEXT NOT NULL,
    established_value TEXT NOT NULL,
    effective_from_event_id TEXT NOT NULL,
    effective_until_event_id TEXT,
    visibility_scope TEXT NOT NULL,
    provenance_id TEXT NOT NULL,
    superseded_by_trait_id TEXT,
    FOREIGN KEY (visual_identity_id)
        REFERENCES visual_identities(visual_identity_id),
    FOREIGN KEY (superseded_by_trait_id)
        REFERENCES visual_identity_traits(visual_trait_id)
);

CREATE INDEX idx_visual_identity_subject
    ON visual_identities(subject_kind, subject_id);
```

Polymorphic subject and provenance references require semantic validation when direct SQLite foreign keys cannot express them. A concrete schema may normalize each subject kind into dedicated relation tables. It must reject dangling references and overlapping competing current traits while preserving historical intervals.

Do not store absent traits as defaults. Do not serialize Species, equipment, condition, or Location state into `established_value`. The repository provides no populated campaign schema and requires no campaign migration by itself.

## Validation

Validate that:

- every Visual Identity and Trait ID is unique;
- every subject reference resolves to the correct Entity, body, Incarnation, Autonomous Individual, or Model;
- Model and Individual visual ownership remain distinct;
- a continuing subject does not receive duplicate active Visual Identities for the same scope;
- Species, form, Evolution, model, equipment, condition, and Location facts remain references to their owners;
- current equipment and temporary injury do not become persistent identity without an established transition;
- effective intervals and supersession preserve real physical change;
- conflicting current traits are reported rather than merged;
- recovered traits retain source provenance and authorized scope;
- absent traits remain Unspecified;
- generated-image details enter Canon only through explicit adoption;
- physical traits do not cross Reincarnation without an established mechanic;
- GM Secrets and undiscovered visual facts cannot leak through Derived projections.

## Safeguards

- Never require a complete appearance sheet.
- Never fill absent traits from genre expectation, a schema, a summary, or an image.
- Never let a generated image become an alternate source of campaign truth.
- Never copy Species, form, model, equipment, condition, or Location state for convenience.
- Never treat an approved image as approval of every visible detail.
- Never copy physical identity across Lives merely because the Soul continues.
- Never merge conflicting historical descriptions silently.
- Never create a populated Visual Identity in this repository.

## Related Documents

- [Canonical Visual Context](../ai/CANONICAL_VISUAL_CONTEXT.md)
- [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md)
- [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- [Autonomous Registry](AUTONOMOUS_REGISTRY.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Continuity Resolution](CONTINUITY_RESOLUTION.md)
- [Persistence Validation](PERSISTENCE_VALIDATION.md)
- [Context Assembly and Gameplay Turn Persistence](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [Visual Identity Template](../../templates/VISUAL_IDENTITY_TEMPLATE.md)
- [Canonical Visual Context Template](../../templates/CANONICAL_VISUAL_CONTEXT_TEMPLATE.md)
