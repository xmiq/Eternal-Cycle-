# Canonical Visual Context

## Purpose

This document defines the purpose-specific FR-011 projection and tool-handoff procedure used when an external or player-facing tool must depict canonical campaign content.

The primary current consumer is image generation. The contract may also support another representation tool when visual campaign fidelity is material. It does not require visual context for unrelated tools.

## Document Control

- **Owner:** visual-context relevance selection, Perspective and Secret filtering, Canon-versus-rendering labels, image-generation read discipline, and representation-tool handoff
- **Primary authorities:** [Context Assembly and Gameplay Turn Persistence](CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md), [Visual Identity](../persistence/VISUAL_IDENTITY.md), and [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
- **Dependencies:** Canonical Data Ownership, current Save Point, stable subject and Location IDs, Truth Layers, equipment, conditions, Species and form, Autonomous Models, and actual tool capability
- **Extensions:** execution profiles and image tools may implement stricter formatting without changing authority or persisting rendering choices
- **Consumers:** AI GMs, human GMs using representation tools, image-generation handoffs, validation, and continuity recovery
- **Repository boundary:** no generated image, campaign prompt, visual reference asset, entity description, private locator, or campaign-specific context belongs here

## Core Rule

> Any external tool producing a representation of canonical campaign state receives the smallest relevant canonical projection before generation.

Conversation context alone is insufficient when canonical persistence is available. If conversation says one thing and the current authoritative owner says another, the owner wins and the conflict is routed through ordinary continuity rules.

## Canonical Visual Context

A **Canonical Visual Context** is a non-authoritative Context Packet subtype assembled for one requested depiction.

```text
requested scene and Perspective
    -> relevant Entity, body, Autonomous, Model, item, and Location IDs
    -> current canonical owners
         Species and form
         Model Visual Identity
         Individual Visual Identity
         equipment and clothing
         condition and transformation
         Location and environment
         requested visible world state
    -> Perspective, Character Knowledge, and GM Secret filtering
    -> Canonical Visual Context
    -> representation-tool prompt
```

The projection owns no visual fact. It records source IDs, source versions, visibility, uncertainty, and freshness so that it can be discarded or rebuilt when owners change.

## Canonical Facts and Rendering Choices

Every handoff distinguishes:

- **Canonical Visual Fact** — established by a valid current or historical owner and visible within the requested Perspective.
- **Non-Canonical Rendering Choice** — composition or incidental appearance selected only to produce a usable image where Canon is silent.

Permitted rendering freedom may include camera angle, framing, lighting treatment, depth of field, general aesthetic, and incidental detail required to fill pixels. It must not contradict established facts, identify an Unknown object, reveal protected information, or imply a campaign fact unsupported by Canon.

The output image does not promote a rendering choice into Canon.

## Assembly Procedure

Before depicting canonical campaign content:

1. determine whether the request refers to canonical campaign subjects, places, events, or current state;
2. identify the requested scene, time, active Perspective, audience, and relevant stable IDs;
3. load the current Save Index, Campaign Version, and relevant authoritative owners through FR-011;
4. assemble current Species/form, Model, individual Visual Identity, equipment, condition, transformation, item, Location, environment, and visible world-state references;
5. perform targeted historical retrieval only when structured current state is insufficient and relevant established description is expected to exist;
6. separate established visible facts, uncertain or Unknown visible facts, Explicitly Unspecified details, and excluded GM Secrets;
7. label permitted rendering freedom as non-canonical;
8. hand only the filtered minimal projection to the image or representation tool;
9. generate without silently enriching the prompt with unsupported campaign-specific facts;
10. do not persist incidental output details;
11. if an authorized participant explicitly adopts a detail, route that bounded trait through its canonical owner and FR-011.

Do not load the entire campaign merely because the tool can accept a large prompt.

## Historical Lookup Fallback

When current structured Visual Identity is insufficient but historical Canon is likely to contain established recurring traits:

1. identify the exact subject and material missing trait scope;
2. follow targeted Timeline, Campaign History, Life Archive, Session, approved-reference, or prior-save indexes;
3. preserve source authority, effective time, uncertainty, and protected visibility;
4. use recovered facts for the present projection only when supported;
5. initiate authorized [Historical Visual Recovery](../persistence/VISUAL_IDENTITY.md#historical-visual-recovery) when a recurring persistent trait should be structured for future use.

Do not invent first and search later. Do not perform a broad historical scan when the missing detail cannot affect the requested representation.

## Perspective, Secrets, and Unknowns

Canonical Visual Context is filtered for the requested Perspective and audience.

Do not hand a tool:

- a hidden creature identity unavailable to the observer;
- an invisible mechanism;
- undiscovered markings;
- concealed equipment;
- the true identity of a distant or obscured structure;
- GM-only transformation information;
- inaccessible visual detail outside the requested viewpoint.

If Canon establishes only `distant regular geometric structure; identity unknown`, the projection may describe visible regular geometry, scale uncertainty, obstruction, and distance. It must not label the object a tower, ruin, city, spacecraft, temple, or other unsupported identity.

Tool prompts, caches, alt text, filenames, logs, and error reports must follow the same visibility boundary.

## Current Appearance Assembly

Current Appearance follows current owners at the requested time:

- current form defeats stale former morphology;
- equipment and clothing come from Inventory and Custody or their specialist owner;
- current injuries and effects come from condition owners;
- model morphology comes from the Model owner;
- individual distinctions come from Visual Identity;
- environment comes from Location, Infrastructure, and World State;
- former-Life appearance remains Historical unless the request depicts that Life.

The projection may repeat these facts as Derived content with source references. It never writes them back to Visual Identity.

## Model and Individual Assembly

For an Autonomous Individual or equivalent modeled subject:

1. load the Autonomous ID and current Model reference;
2. load the applicable Model Visual Identity once;
3. load the individual's Visual Identity distinctions;
4. load current condition, equipment, placement, and visible assignment state through their owners;
5. avoid copying common model morphology into the individual record.

Designation changes do not alter visual identity. Reconstruction, revision adoption, or body replacement follows Autonomous Registry continuity and provenance before the projection changes.

## Image Generation Is Normally Read-Only

An image request normally has:

```text
Affected Set = empty
```

Canonical reads and context assembly do not create a Save Point. The player-visible marker reflects the already-verified configured authority under FR-011; no new campaign version is fabricated merely because an image was generated.

If the request itself includes an explicit canonical change or the player later adopts a generated detail, the Affected Set becomes non-empty and normal automatic persistence applies. The truthful status remains:

- `💾` for validated local canonical authority;
- `☁️💾` for synchronized and verified cloud canonical authority;
- `⏳` while required persistence is incomplete;
- `⚠️` after persistence or validation failure.

## Explicit Canon Adoption

An authorized participant may adopt one bounded generated detail without adopting the entire image.

The GM must:

1. quote or identify the exact feature being adopted;
2. determine whether Visual Identity, Species/form, Model, equipment, condition, Location, or another owner properly owns it;
3. preserve the generated image or adoption statement only as provenance appropriate to campaign configuration;
4. reject contradictions or route them through Continuity Resolution;
5. update the complete Affected Set once;
6. validate and verify persistence before reporting durable adoption.

Approval is explicit. Prior generation, player enjoyment, repeated depiction, or visual attractiveness is insufficient.

## AI GM Procedure

An AI GM handling an image request:

1. classifies whether the request depicts canonical campaign content;
2. identifies relevant canonical IDs and Perspective;
3. assembles Canonical Visual Context from current owners;
4. retrieves targeted historical description only when structured state is insufficient;
5. preserves Unknowns, Unspecified detail, Character Knowledge, and GM Secrets;
6. separates Canonical Visual Facts from rendering freedom;
7. invokes the image tool with the filtered context;
8. treats generated incidental details as non-canonical;
9. persists only an explicitly adopted bounded trait through its proper owner.

If the required canonical records cannot be read, report the limitation instead of substituting conversation memory or visual invention.

## Tool-Handoff Contract

The handoff includes only:

- request and representation purpose;
- Perspective and audience;
- relevant stable subject and Location references where operationally appropriate;
- established visible facts with source-owner categories;
- current visible equipment, condition, and environment;
- uncertainty and Explicitly Unspecified details;
- protected exclusions represented without leaking their contents;
- rendering freedom and non-canonical-output warning.

Private database locators, credentials, unrestricted GM records, raw tables, unrelated campaign state, and hidden facts are excluded.

## Regression Cases

### A. Established Recurring Character

Historical Canon establishes coloration and body shape, but the current summary omits them. Targeted recovery confirms persistent traits, records their provenance under Visual Identity, validates the update, and later image requests retrieve them without another historical search.

### B. Unknown Eye Colour

No owner establishes eye colour. An image renders amber eyes as an incidental choice. The Visual Identity remains silent and the database stays Unspecified.

### C. Current Equipment

An explorer currently carries a communications backpack. The projection retrieves it from Inventory, equipment, Infrastructure, or another actual owner. It does not copy the backpack into permanent Visual Identity.

### D. Servitor Model

Shared limb architecture comes from the servitor Model Visual Identity. One unit's repair scar comes from its Individual Visual Identity. The projection combines both without duplicating model morphology.

### E. Evolution

A creature evolves into a different form. The current form owner supplies new morphology; stale old-form appearance is not rendered. Individual traits persist only where the new embodiment can still express them.

### F. Reincarnation

The same Soul enters a new body. The new depiction follows the current Incarnation, body, Species, and form. Former physical appearance does not transfer merely through Soul continuity.

### G. Unknown Structure

Canon establishes only distant regular geometry. The prompt preserves visible shape and uncertainty without naming a ruin, city, ship, tower, or temple.

### H. GM Secret

The database records the hidden identity of a visible figure. The player-facing context includes only perceptible form and observer-available Knowledge; the hidden identity never reaches the tool.

### I. Conversation Conflict

Recent conversation describes blue scales while validated current persistence establishes green scales. Green is used. The prose conflict is handled through Continuity Resolution rather than becoming prompt authority.

### J. Approved Image Trait

The player explicitly adopts one generated eye pattern. The GM routes that bounded trait to the correct Visual Identity, records provenance, persists it once, validates, and reports the resulting FR-011 status.

### K. Unapproved Image

An image contains decorative clasps, eye colour, and surface markings not established by Canon. No campaign mutation occurs, the Affected Set remains empty, and later images are not required to repeat them.

## Validation and Acceptance

A conforming runtime or campaign implementation verifies that:

- relevant canonical owners were read before depiction;
- the projection is relevance-filtered and carries source/freshness evidence;
- Visual Identity subject, Model, Species/form, equipment, condition, and Location references resolve;
- Model and Individual facts are not duplicated as current authority;
- Unspecified details remain non-canonical;
- conflicting history follows recovery or continuity adjudication;
- GM Secrets and observer-inaccessible facts are excluded from the handoff;
- Unknown objects remain unidentified;
- generated incidental details do not enter Canon;
- read-only image generation leaves the Affected Set empty;
- explicit adoption produces one owner-routed Affected Set and verified FR-011 persistence;
- Regression Cases A through K pass against the configured campaign implementation.

## Runtime Boundary

This repository provides the canonical projection, procedure, blank templates, and structural regression validation. It cannot force an external image host to retrieve campaign state, honor exclusions, or avoid retaining prompts. Each runtime must verify its actual tool capability and privacy boundary before use.

No runtime may claim canonical grounding merely because a prompt sounds consistent. Source reads and filtering must actually occur.

## Related Documents

- [Visual Identity](../persistence/VISUAL_IDENTITY.md)
- [Context Assembly and Gameplay Turn Persistence](CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [AI GM Workflow](AI_GM_WORKFLOW.md)
- [AI Play Protocol](AI_PLAY_PROTOCOL.md)
- [AI Capabilities and Limitations](AI_CAPABILITIES_AND_LIMITATIONS.md)
- [Canonical Data Ownership](../persistence/CANONICAL_DATA_OWNERSHIP.md)
- [Truth Layers](../persistence/TRUTH_LAYERS.md)
- [Autonomous Registry](../persistence/AUTONOMOUS_REGISTRY.md)
- [Canonical Visual Context Template](../../templates/CANONICAL_VISUAL_CONTEXT_TEMPLATE.md)
