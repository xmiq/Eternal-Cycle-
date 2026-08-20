# Canonical Visual Context Template

Use this internal blank contract to assemble the smallest Perspective-filtered canonical projection needed by an image or other authorized visual-representation tool. It is Derived context, not Campaign Canon.

## Document Control

- **Template owner:** [Canonical Visual Context](../docs/ai/CANONICAL_VISUAL_CONTEXT.md)
- **Dependencies:** active Save Point, requested Perspective, stable IDs, current visual owners, Truth Layers, and FR-011 freshness
- **Consumers:** AI or human GM tool handoff, image generation, debugging, and validation
- **Repository boundary:** no populated scene, prompt, entity, image, GM Secret, private locator, or campaign fact belongs here

## Request

- **Requested subject or scene:** `<bounded request>`
- **Representation purpose:** `<image generation or another authorized visual task>`
- **Requested time:** `<current Save Point, historical event, or explicit scope>`
- **Perspective:** `<observer/viewpoint>`
- **Audience:** `<authorized recipient>`
- **Parent Campaign Version and Save Point:** `<references>`

## Relevant References

- **Entity/body/Incarnation IDs:** `<typed references>`
- **Autonomous Individual and Model IDs:** `<typed references>`
- **Location and Infrastructure IDs:** `<typed references>`
- **Item/equipment IDs:** `<typed references>`
- **Species/form/Evolution IDs:** `<typed references>`

## Established Visible Facts

- `<fact -> authoritative owner, source ID, revision, effective time>`

## Current Equipment and State

- `<visible equipment, clothing, condition, injury, transformation, carried item -> owner reference>`

## Model/Species-Derived Visible Facts

- `<visible fact -> Model, Species, form, or Evolution owner reference>`

## Individual Visual Identity

- `<persistent individual trait -> Visual Trait ID>`

## Location and Environment

- `<visible place, Infrastructure, weather, lighting condition, or world-state fact -> owner reference>`

## Uncertain or Unknown

- `<visible but unidentified, disputed, approximate, obscured, or requires source recovery>`

## Explicitly Unspecified

- `<material detail Canon does not establish; omit exhaustive immaterial gaps>`

Unspecified detail is not Canon.

## GM-Secret Exclusions

- `<redacted exclusion class and filtering requirement; never copy the protected value>`

## Rendering Freedom

- **Permitted composition choices:** `<camera, framing, lighting treatment, aesthetic, or other non-factual direction>`
- **Incidental detail policy:** `Non-canonical incidental details permitted where necessary; they must not contradict Canon, identify Unknowns, reveal Secrets, or be persisted automatically.`

## Handoff Validation

- [ ] Relevant canonical owners were read from the active verified Save Point.
- [ ] Every visible fact has a source category and current effective scope.
- [ ] Species/form, Model, individual, equipment, condition, and Location ownership remain distinct.
- [ ] Conversation conflict was resolved in favor of canonical persistence or routed through Continuity Resolution.
- [ ] Unknown objects remain unidentified.
- [ ] GM Secrets and observer-inaccessible facts are excluded.
- [ ] Rendering freedom is labelled non-canonical.
- [ ] The receiving tool gets no unrelated state, private locator, or credential.
- [ ] Image generation itself has `Affected Set = empty` unless a separate explicit Canon adoption occurs.

## Post-Generation Result

- **Tool result:** `<success | partial | failed | unknown>`
- **Canonical mutation:** `<none by default | explicit adopted-trait transaction reference>`
- **Incidental details persisted:** `No`
- **Persistence marker:** `<existing verified authority when read-only | FR-011 result after explicit adoption>`

## Cross-References

- [Visual Identity](../docs/persistence/VISUAL_IDENTITY.md)
- [Context Assembly and Gameplay Turn Persistence](../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [Truth Layers](../docs/persistence/TRUTH_LAYERS.md)
- [Canonical Visual Context](../docs/ai/CANONICAL_VISUAL_CONTEXT.md)
