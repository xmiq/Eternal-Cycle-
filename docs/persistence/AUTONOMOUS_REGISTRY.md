# Autonomous Registry

## Purpose

The Autonomous Registry is the authoritative Campaign Persistence domain for persistent independently operating entities and systems. It preserves identity, autonomy-specific state, controller separation, model and modification lineage, memory continuity, uncertainty, and provenance across ordinary change.

It can represent servitors, golems, familiars, undead, summons, drones, artificial life, remote bodies, autonomous vehicles, autonomous infrastructure, magical constructs, networked agents, and equivalent persistent autonomous subjects. It is not servitor-specific, a generic character note, a Living Codex module, or a populated campaign registry.

## Document Control

- **Owner:** autonomous identity, autonomy classification, controller assignments, operational assignment, model and individual distinction, reconstruction continuity, autonomous memory continuity, network membership, and last-confirmed state
- **Primary authorities:** [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md), [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md), and [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
- **Dependencies:** stable Entity identity, Typed References, Locations, Relationships, Infrastructure, Inventory, Projects, Timeline, uncertainty, Save Updates, and validation
- **Extensions:** concrete schemas may normalize additional autonomous types or mechanics without creating competing ownership
- **Consumers:** GMs, AI runtimes, campaign schemas, migrations, world simulation, Relationships, Projects, Infrastructure, Context Assembly, and validation tools
- **Repository boundary:** populated autonomous entries, models, networks, assignments, controllers, reports, campaign migration results, and current state remain outside this repository

## Core Ownership Boundary

```text
Persistent non-autonomous being
  -> ordinary Entity identity anchor

Persistent autonomous entity or system
  -> Autonomous Registry identity anchor
```

One continuing subject has one authoritative identity anchor. Ordinary character persistence may reference an Autonomous ID but cannot create another canonical identity for it. The Registry references specialist owners for Skills, Magic, equipment, resources, Infrastructure, Relationships, placement, and history rather than copying their mutable state.

The player remains external to the fiction. Entity, Controller, and Perspective remain distinct. A Controller change never replaces autonomous identity.

## Inclusion and Persistence Threshold

Create an Autonomous Registry identity when an independently operating subject's individual or bounded-group continuity matters to current or future adjudication. Relevant signals include persistent assignment, independent action, recurring interaction, controller change, meaningful condition, memory continuity, network membership, upgrades, personhood questions, or historical consequences.

Do not create individual records for every anonymous unit. Homogeneous units may remain a bounded group while individual identity does not matter. A promoted member receives a new stable individual identity linked to the source group and promotion event; the group quantity and history update through the same transaction.

## Model and Individual

An **Autonomous Model** is a reusable construction, summoning, animation, body, or operating design. An **Autonomous Individual** is one continuing in-world Entity.

- Many individuals may reference one Model ID.
- A model revision does not silently rewrite existing individuals.
- An individual may receive upgrades or replacement bodies without changing identity when continuity is established.
- Two individuals built from one model remain distinct.
- Model capability is availability or design intent, not proof of one individual's current access, condition, equipment, or mastery.

Models may reside in campaign design, Projects, Infrastructure, Magic, Species, or an adopted reusable authority as appropriate. The Registry owns only the model reference and individual-specific lineage facts.

## Logical Record Model

A storage implementation may choose table names and keys consistent with its existing schema. The logical model must remain normalized.

| Logical record | Required purpose |
|---|---|
| **Autonomous Identity** | Permanent Autonomous ID, identity kind, individual or group status, display designation references, continuity core, lifecycle status, provenance, and record version. |
| **Model Reference** | Stable Model ID, revision used, adoption time, and source owner. |
| **Controller Assignment** | Controller ID, scope, authority source, start and end, priority or conflict rules, limits, and provenance. Creator is a separate origin reference. |
| **Autonomy Profile** | Current autonomy classification, supported operating scope, restrictions, evidence, effective time, and uncertainty. |
| **Placement Reference** | Typed reference to the one authoritative current-placement relation and its precision. Distributed systems link explicit component placements. |
| **Operational Assignment** | Current or historical assignment identity, goal or duty, issuer, scope, targets, dependencies, start, review, completion, and status. |
| **Condition Reference** | Typed references to authoritative body, construct, vehicle, Infrastructure, or effect condition owners. |
| **Capability Reference** | Typed references to Skills, Magic, equipment, Species, model functions, Infrastructure functions, or other owners. |
| **Resource and Maintenance Requirement** | Typed requirements and schedules referencing Inventory, resource, Infrastructure, Project, or environmental owners. |
| **Network Membership** | Network ID, relation type, role, permissions, information-sharing scope, connection state, and effective interval. |
| **Creation Lineage** | Creator, Model, Project, batch, predecessor, memory source, summoning or creation event, and other origin references. |
| **Upgrade Lineage** | Significant modification event, prior configuration, new configuration, replaced components or bodies, capability references, and provenance. |
| **Memory Continuity Claim** | Continuity state, source, coverage, divergence point, confidence, validation, and related identities. |
| **Personhood and Independence Claim** | Operational independence and personhood claims with scope, authority, evidence, disagreement, and uncertainty. |
| **Confirmed Report** | Last confirmed event or report ID, report time, observation time, source, covered facts, and uncertainty. |

Names, nicknames, designations, aliases, and model labels may change. The Autonomous ID does not.

## Autonomy Classification

Autonomy is qualitative and scoped. Use the smallest classification that accurately describes established operating independence:

| Classification | Meaning |
|---|---|
| **Directed** | Executes immediate commands with no established independent task continuation. |
| **Task-Bounded** | Performs a bounded assigned task independently within explicit methods or limits. |
| **Method-Selecting** | Selects methods and local responses while pursuing an assigned goal. |
| **Extended-Operation** | Sustains operations and adapts across long periods without routine supervision. |
| **Goal-Prioritizing** | Independently weighs, sequences, defers, or rejects goals within established constraints. |
| **Independent Agent** | Operates without a current Controller and exercises functionally independent agency. |

Classification does not determine intelligence, sapience, morality, legal status, Soul possession, independence, or personhood. Record uncertainty and mixed scope when autonomy differs by function.

## Controller, Creator, and Assignment

Creator, Controller, and assignment issuer are separate relations.

- One autonomous Entity may have no Controller, one Controller, or several Controllers with non-overlapping or explicitly resolved scopes.
- Controller authority records source, limits, duration, revocation, priority, and resistance or override routes when mechanics permit them.
- Losing communication does not automatically end a Controller relation or assignment.
- An assignment records what the Entity is doing; it does not prove the Controller's present intent.
- Independence changes control state, not physical model or identity.

Controller records reference a valid Entity, faction, Institution, system, or other established authority. The Registry does not duplicate that subject's identity.

## Perspective

An autonomous Entity may hold one or more Perspectives when it perceives or independently processes world state. Perspectives reference sensory routes, location or remote channels, access, limits, and information boundaries through the existing architecture.

Not every autonomous system has a human-like viewpoint. Network access does not imply shared Perspective, shared knowledge, shared memory, or global awareness. The Registry references Perspective and Knowledge owners rather than creating a new perception system.

## Memory Continuity

Autonomous memory continuity is explicit and provenance-bearing. Supported qualitative states include:

- **Continuous** — established uninterrupted continuity;
- **Backup-Restored** — restored from an identified prior state with a known gap;
- **Partial** — bounded portions persist or were restored;
- **Fragmented** — surviving material is discontinuous or internally damaged;
- **Reset** — prior operational memory is not retained;
- **Copied** — memory derives from another identity while this Entity is distinct;
- **Forked** — two or more distinct identities descend from one memory state;
- **Unknown** — available evidence cannot establish continuity.

A concrete implementation may refine these labels while preserving their distinctions. Memory continuity is evidence for identity adjudication, not the sole universal definition of personhood.

This subsystem does not implement FR-015 reincarnation memory. It tracks an autonomous Entity's own operational memory across repair, replacement, copying, and reconstruction.

## Restoration, Reconstruction, Copies, and Forks

### Restoration

A repaired or rebuilt subject retains its Autonomous ID only when the campaign establishes sufficient identity continuity through the applicable mechanics, continuity core, memory evidence, body or system continuity, social recognition, and authorized adjudication.

### Reconstruction as a New Individual

A new subject built from the same Model receives a new Autonomous ID. Shared components, design, creator, or copied memories become lineage references rather than identity reuse.

### Copies and Forks

When one memory or system state produces simultaneous descendants:

- each continuing descendant receives a distinct Autonomous ID;
- all retain the source identity or memory-state reference;
- creation events and divergence points are recorded;
- copied history is distinguished from independently lived history;
- neither descendant silently overwrites or merges with the source or sibling.

An uncertain identity claim remains disputed or unknown pending owner review. Validation never resolves the philosophy automatically.

## Individuals, Groups, and Quantity

An Individual has conceptual quantity one. Quantity changes cannot create, destroy, split, or merge identified individuals.

A bounded Group record may carry quantity when members are homogeneous and individual continuity is immaterial. Group transactions record recruitment, construction, casualties, losses, splits, merges, and promotions with source events and numerical traces.

When one member becomes distinct:

1. establish the promotion event and provenance;
2. create a stable Autonomous ID for the individual;
3. link the source Group ID and any known lineage;
4. adjust group quantity once;
5. migrate individual-specific condition, assignment, memory, and Relationships only when supported;
6. validate that the group and individual no longer claim the same unit.

## Location and Distributed Operation

The Registry follows the [Current Location Invariant](CANONICAL_DATA_OWNERSHIP.md#current-location-invariant). It references the authoritative placement relation and does not maintain a competing current-location field.

Remote bodies, vehicles, networks, and distributed systems may have several component placements only when their architecture supports them. Record one Entity identity plus distinct Body, component, or endpoint IDs and placement relations. A replacement shell does not create a new identity; an independently operating fork does.

## Capabilities, Resources, and Maintenance

Capabilities reference their owners. The Registry may index relevant Skill, Magic, equipment, Species, model-function, Infrastructure-function, or other capability IDs but does not duplicate their definitions or mutable state.

Resource and maintenance requirements reference authoritative Inventory, resource, Infrastructure, Project, ritual, environmental, or condition records. Examples include Mana, fuel, food, repair material, replacement components, Soul energy, ritual upkeep, service intervals, and environmental dependencies. The Registry records dependency and operational relevance, not a second inventory.

## Networks

Network records represent command, communication, shared sensing, Mana, distributed cognition, Infrastructure, swarm, or other established connections.

Membership preserves individual identity unless Canon establishes a genuinely collective Entity. Each edge records role, direction, permissions, information-sharing scope, connection state, effective interval, and provenance. Network membership alone grants neither shared memory nor control.

## Independence and Personhood

Operational autonomy, independence, sapience, and personhood are separate claims.

- **Operational autonomy** describes independent action within a scope.
- **Independence** describes absence or rejection of controlling authority.
- **Sapience or personhood** follows applicable mechanics, culture, law, evidence, and adjudication.

Claims may be established, disputed, unknown, culturally scoped, or governed elsewhere. The Registry preserves the claim and its authority without forcing a universal philosophical answer.

## Relationships, Infrastructure, and History

- [Relationships](RELATIONSHIP_MEMORY_ENGINE.md) owns trust, hostility, recognition, commitments, and shared history involving Autonomous IDs.
- Infrastructure owns physical and operational infrastructure state. The Registry owns autonomy-specific identity and control state for autonomous Infrastructure and cross-references the Infrastructure ID.
- Inventory owns items, custody, quantities, and supplies.
- Timeline and Campaign History own creation, destruction, restoration, upgrades, controller changes, independence, forks, merges, and other significant events.
- The Registry owns current autonomous state and lineage references; it does not replace event history.

## Last Confirmed State and Uncertainty

The Registry separates current confirmed facts from assumptions after last contact.

A Confirmed Report records source, observation time, report time, covered facts, and uncertainty. When contact is lost, retain the last confirmed condition, location, assignment, and report while marking later current claims `Unknown`, `Estimated`, `Not Yet Verified`, or another supported uncertainty state.

Silence does not establish destruction, mission completion, loyalty, location, condition, resource exhaustion, or controller change. Off-screen simulation may establish change only through ordinary causal and persistence rules.

## Read Set

For an interaction involving an autonomous subject, load the smallest dependency-complete set that can change adjudication, consequences, uncertainty, or player choice. It commonly includes:

- Autonomous Identity, Model reference, Autonomy Profile, Controller assignments, and last Confirmed Report;
- authoritative placement and condition references;
- current assignment or Project;
- relevant capability, resource, maintenance, and network references;
- relevant Relationships, Knowledge, Perspective, Timeline, and Infrastructure;
- applicable Repository Canon and Campaign Canon.

Do not load unrelated Registry entries or every record connected to a model, network, or Location.

## Save Procedure

After a completed interaction involving an autonomous subject:

1. determine whether identity, autonomy, Controller, assignment, condition reference, placement reference, network, requirement, lineage, memory continuity, independence, personhood claim, or confirmed state changed;
2. route each fact to its [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md) owner;
3. update Registry-owned facts once and update other domains only when their facts changed;
4. append material creation, destruction, restoration, upgrade, fork, controller, or independence events;
5. preserve last-confirmed facts separately from unknown present state;
6. update creation, upgrade, copy, fork, or restoration lineage when required;
7. invalidate affected Derived Views and caches;
8. validate identity, exclusivity, references, quantities, lineage, uncertainty, and expected changes;
9. activate only the complete Save Point under the [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md).

## Existing-Campaign Audit and Migration

Applying FR-014 to an existing campaign follows [Migration and Versioning](MIGRATION_AND_VERSIONING.md): Backup, Audit, Merge, Validation, activation, and rollback.

The Audit stage inventories all established autonomous subjects and candidate models. Campaign-specific examples that may require review include Gatherer Servitors, an Observer Servitor, a Herald Servitor, an Autonomous Maintenance Servitor, an Ascender Exploration Servitor, and earlier monitoring or general-purpose servitors. These names are migration prompts only; they do not create reusable entries or assert current campaign facts.

For each source record, classify from Campaign Canon:

- Model, Individual, or bounded Group;
- active, inactive, destroyed, unknown, restored, or upgraded state;
- ordinary Entity versus autonomous ownership;
- stable identity and aliases;
- creator, Controllers, assignment, placement, and last confirmation;
- creation, upgrade, reconstruction, copy, fork, and memory lineage;
- Relationships, networks, requirements, Infrastructure, and history references;
- conflicts, duplicates, missing provenance, and source-recovery needs.

Merge only the affected records. Never infer identity continuity, destruction, controller authority, or current state from a label. Validate before activation. This reusable repository performs no populated campaign migration.

## Validation Requirements

Where the storage implementation exposes sufficient structure, validate:

- Autonomous IDs and active identity anchors are unique;
- Model and Individual identities are distinct and references resolve;
- creator and current Controller are separate claims;
- Controller scope and authority sources resolve;
- placement, assignment, capability, requirement, network, Infrastructure, Relationship, and event references resolve;
- memory-continuity claims include provenance and divergence where applicable;
- restoration and reconstruction use the correct same-identity or new-identity outcome;
- copies and forks have distinct IDs and traceable ancestry;
- Individual quantity is one;
- Group quantity changes have traces and promoted individuals are not still counted as anonymous members;
- one continuing subject is not active under both ordinary Entity and Autonomous Registry ownership;
- significant creation, upgrade, restoration, fork, and independence events remain traceable;
- last-confirmed facts remain separate from unknown present state;
- network membership does not imply unrecorded control, Perspective, knowledge, or memory sharing;
- no Derived View, report, or cache becomes an owner.

Ambiguous identity conflicts are reported for owner review. Validators do not merge, split, restore, destroy, or reclassify an Entity automatically.

## FR-011 Compatibility

[Context Assembly and Gameplay Turn Persistence](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) selects only relevant Autonomous IDs, Models, Controllers, autonomy, placement, assignments, condition, last Confirmed Reports, networks, resources, and maintenance dependencies. Automatic turn persistence routes changes back to this owner and its referenced specialist owners without loading every autonomous unit.

Context Assembly retrieves Autonomous IDs, Model IDs, Controllers, placements, assignments, condition references, last-confirmed state, Relationship IDs, Network IDs, and authoritative source paths only when relevant. Every generated Context Packet remains Derived or Cache data under FR-012; this registry retains autonomous-state ownership.

## Safeguards

- One autonomous subject has one authoritative identity anchor.
- Model identity never substitutes for individual identity.
- Repair or reconstruction never assumes continuity.
- Creator never silently becomes permanent Controller.
- Autonomy never proves personhood.
- Quantity never hides identified individuals.
- Unknown current state remains unknown.
- Network membership never implies total shared state.
- Capabilities, resources, Relationships, Infrastructure, placement, and history remain with their owners.
- FR-015 memory rules remain separate.
- No campaign-specific Registry data belongs in this repository.

## Related Documents

- [Campaign Persistence Engine](README.md)
- [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](PERSISTENCE_VALIDATION.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md)
- [Timeline Engine](TIMELINE_ENGINE.md)
- [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
- [Autonomous Registry Template](../../templates/AUTONOMOUS_REGISTRY_TEMPLATE.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
