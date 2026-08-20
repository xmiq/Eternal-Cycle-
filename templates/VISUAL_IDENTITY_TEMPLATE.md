# Visual Identity Record Template

Use this blank storage-neutral contract for sparse persistent visual traits belonging to one valid body, Incarnation, Entity, Autonomous Individual, or Model. Populate it only in external Campaign State.

Omit every trait that Canon has not established. A blank field or absent trait grants no default and does not authorize a generated image to fill campaign truth.

## Document Control

- **Template owner:** [Visual Identity](../docs/persistence/VISUAL_IDENTITY.md)
- **Dependencies:** stable visual-subject identity, Species/form, Model, provenance, Timeline, Truth Layers, and validation
- **Consumers:** Canonical Visual Context, historical recovery, character and autonomous views, image-tool handoffs, and validation
- **Repository boundary:** no populated identity, trait, campaign image, visual reference, or migration finding belongs here

## Required Record Fields

- **Visual Identity ID:** `<stable ID>`
- **Subject kind:** `<body | Incarnation | Entity | Autonomous Individual | Autonomous Model | another established kind>`
- **Subject ID:** `<stable typed reference>`
- **Persistence scope:** `<body | Incarnation | Entity | Individual | Model>`
- **Authoritative owner:** `<Visual Identity or established Model owner>`
- **Record version:** `<positive revision>`
- **Truth Layer and visibility:** `<scope and protected exclusions>`
- **Record provenance:** `<creation, recovery, adoption, or correction source>`
- **Validation status:** `<run, result, warnings, or pending>`

## Established Trait Entry

Repeat only for an established trait.

- **Visual Trait ID:** `<stable ID>`
- **Trait category:** `<body plan | scale | coloration | marking | scar | morphology | texture | permanent modification | other bounded category>`
- **Established value:** `<only what Canon supports>`
- **Source class:** `<explicit gameplay | approved reference | continuity recovery | individual modification | another established route>`
- **Source reference:** `<event, record, adjudication, adoption transaction, or recovery finding>`
- **Effective from:** `<Timeline or event reference>`
- **Effective until or supersession:** `<reference or none>`
- **Visibility scope:** `<audience and Perspective restrictions>`
- **Uncertainty:** `<established | disputed | requires source recovery | another supported state>`
- **Related owner references:** `<Species/form, Model, condition, or other sources without copied mutable state>`

Do not create an entry for an Unspecified trait.

## Current Owner References

These are References only and do not become Visual Identity fields.

- **Current Species/form:** `<owner reference>`
- **Model Visual Identity:** `<reference or none>`
- **Current equipment/clothing:** `<Inventory or specialist references>`
- **Current condition/injury/effects:** `<condition owner references>`
- **Current transformation:** `<owner reference or none>`
- **Current Location/environment:** `<Location and World-State references>`

## Historical Recovery

- **Recovery scope:** `<targeted traits and subject>`
- **Backup:** `<verified reference>`
- **Sources audited:** `<stable source references>`
- **Supported persistent traits:** `<Trait IDs created or revised>`
- **Physical changes distinguished:** `<Evolution, injury, repair, aging, transformation, or none>`
- **Conflicts and uncertainty:** `<findings without silent merge>`
- **Generated images excluded:** `<confirmation or explicitly approved reference>`
- **Migration or Save Transaction:** `<ID>`

## Explicit Canon Adoption

- **Adopted feature:** `<exact bounded detail>`
- **Adoption authority and statement:** `<authorized reference>`
- **Source image/reference:** `<provenance reference without treating all pixels as Canon>`
- **Proper owner:** `<Visual Identity or another domain>`
- **Affected Set and transaction:** `<references>`
- **Persistence result:** `<validated local | verified cloud | pending | failed>`

## Validation Notes

- [ ] Subject ID resolves and does not create a parallel Entity or Model identity.
- [ ] Model and Individual scopes remain distinct.
- [ ] Every listed trait is explicitly established and provenance-bearing.
- [ ] Unspecified traits have no placeholder or default row.
- [ ] Species, form, Evolution, equipment, condition, and Location state remain with their owners.
- [ ] Effective intervals preserve actual physical change.
- [ ] Conflicting traits are not silently merged.
- [ ] Generated-image details are absent unless explicitly adopted.
- [ ] Bodily appearance does not cross Reincarnation without an established route.
- [ ] Protected traits cannot leak through audience-visible views.

## Cross-References

- [Canonical Visual Context](../docs/ai/CANONICAL_VISUAL_CONTEXT.md)
- [Canonical Data Ownership](../docs/persistence/CANONICAL_DATA_OWNERSHIP.md)
- [Autonomous Registry](../docs/persistence/AUTONOMOUS_REGISTRY.md)
- [Migration and Versioning](../docs/persistence/MIGRATION_AND_VERSIONING.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
