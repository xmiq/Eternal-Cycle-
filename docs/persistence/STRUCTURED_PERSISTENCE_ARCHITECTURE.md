# Structured Persistence Architecture

## Purpose

This document defines the logical architecture of an Eternal Cycle Campaign Record. It identifies the modules, record contracts, ownership rules, identifiers, references, indexes, and dependency boundaries required to preserve a campaign without prescribing a storage technology.

The architecture is canonical. A file tree, database schema, document platform, application, binder, or cloud service is an implementation.

## Core Rule

The Campaign Record is a modular graph of authoritative records connected by stable references, not one undifferentiated narrative transcript and not a collection of independent copies.

Every material campaign fact has:

- one authoritative record owner;
- a stable subject identity;
- an authority classification;
- a canonical system owner where mechanics are involved;
- provenance;
- temporal and scope boundaries;
- typed references to dependencies;
- an explicit current status;
- a validation route.

Other modules may summarize or display that fact through Derived Views. They do not become competing owners.

## Logical Architecture, Not Storage Layout

The modules in this document are logical responsibilities.

One implementation might use one Markdown file per module. Another might use tables, documents, object collections, linked pages, or a service with an API. A small campaign may colocate several modules. A large campaign may partition one module by region, Age, subject, or archive period.

Those choices are valid only while they preserve:

- authoritative ownership;
- stable Record Identities;
- reference integrity;
- authority and information distinctions;
- chronology;
- migration traceability;
- validation;
- the repository boundary.

Physical proximity grants no authority. Two facts in one file may belong to different logical modules. Two records in different systems may form one valid module if their index and references remain coherent.

## Architectural Layers

The Campaign Record has four logical architectural layers.

| Layer | Purpose | Examples |
| --- | --- | --- |
| **Control** | Identifies the campaign, active rules, modules, versions, saves, validations, and migrations | Save Index and Protocol, Campaign Canon, Validation, Migration History |
| **Entity and state** | Preserves stable subjects and their current campaign conditions | Player State, Souls and Incarnations, Companions, Actors, Relationships, Species, Inventory |
| **World and activity** | Preserves current environments, organizations, systems, projects, and unresolved processes | World, Locations, Factions, Magic, Infrastructure, Projects |
| **Knowledge and history** | Preserves chronology, evidence, beliefs, research, mysteries, secrets, and historical consequences | Timeline, Campaign History, Knowledge, Research, Mysteries, Secrets |

These layers organize access and dependency. They do not define the [Persistence Authority Chain](PERSISTENCE_AUTHORITY.md), [Truth Layers](TRUTH_LAYERS.md), or [Persistence Levels](PERSISTENCE_LEVELS.md).

## The Common Record Contract

Every authoritative record exposes enough logical metadata to be found, interpreted, related, updated, and validated.

### Required Identity Fields

- **Record ID:** a stable, unique identifier within the campaign.
- **Record Type:** the logical kind of record.
- **Authoritative Module:** the one module that owns this record.
- **Subject ID or Scope:** the being, object, place, group, process, question, interval, or campaign scope described.
- **Record Version:** the revision of this record representation.

### Required Authority and Ownership Fields

- **Persistence authority:** which authority layer supports the claim.
- **Canonical owner:** the repository system that defines any mechanic represented.
- **Status:** active, pending, superseded, archived, disputed, unknown, or another state defined by the owning procedure.
- **Effective scope:** applicable time, place, subject, audience, and conditions.

### Required Provenance Fields

- **Source:** adjudication, event, decision, observation, migration, correction, or other established origin.
- **Established at:** campaign chronology reference or explicit unknown.
- **Last integrated at:** save or update reference.
- **Supersession:** any prior or later record this one corrects, replaces, or derives from.
- **Uncertainty:** an honest classification rather than unsupported precision.

### Required Relationship Fields

- **References:** typed outgoing links to related records.
- **Dependents:** discoverable incoming links or an index capable of finding them.
- **Canonical dependencies:** mechanics or rules required to interpret the record.
- **Validation status:** last check, result, and unresolved warnings where material.

The exact storage labels may differ. The logical information cannot disappear merely because one technology represents it implicitly.

## Record Identity

A **Record Identity** is stable even when a display name, body, title, owner, location, form, allegiance, or storage path changes.

### Identity Rules

- IDs are unique within one campaign continuity.
- A retired ID is not reused for a different subject.
- Renaming changes an alias, not identity.
- Reincarnation links one Soul identity to a new Incarnation identity; it does not reuse the former body identity.
- A faction Version references faction continuity without pretending every member or structure remained identical.
- A divided institution, copied artifact, replacement body, reconstructed settlement, or transformed species requires explicit continuity analysis rather than convenient ID reuse.
- Possible duplicates remain separate and flagged until evidence supports merger.
- A mistaken merge is corrected through traceable identity repair, not silent deletion.

Human-readable names should remain searchable aliases. They are not reliable primary keys.

## Record Ownership

Every fact has one **Authoritative Record Owner** inside the campaign architecture.

Ownership is chosen by the claim's subject and function, not by which module happens to mention it most often.

Examples:

- a character's current body condition is owned by Player State or the linked Incarnation record;
- a promise between two people is owned by Relationships;
- the historical event in which the promise was made is owned by Campaign History;
- a current road condition is owned by Locations or Infrastructure according to its scope;
- the discovery that the road is unsafe is owned by Knowledge for each observer;
- a theory about why it fails is owned by Research;
- the hidden factual cause is owned by the relevant world record and exposed through Secrets as a protected view, not duplicated as a second truth.

When two modules need the same fact, one owns it and the other stores a typed reference or Derived View.

The complete fact-family map, persistent Entity identity anchor, current-location invariant, and autonomous-domain reservation are defined in [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md). Player State, Companions, and Actors are role-facing modules over one persistent non-autonomous Entity identity when they concern the same being; they must not mint parallel identities.

## Typed References

A reference states why two records are related.

Useful reference types include:

- `identity-of`;
- `incarnation-of`;
- `participant-in`;
- `located-at`;
- `member-of`;
- `related-to`;
- `owns` or `held-in-custody-by`;
- `depends-on`;
- `caused-by`;
- `evidence-for`;
- `contradicts`;
- `supersedes`;
- `observed-by`;
- `recorded-in`;
- `affected-by`;
- `derived-from`.

A bare link may be sufficient only when its relationship is unambiguous from the owning schema. Material cross-module references should carry type, direction, and relevant scope.

### Reference Integrity

- Every referenced ID must resolve or be explicitly marked external, unknown, or awaiting recovery.
- Deleting a record requires review of all dependents.
- Archiving preserves resolvable identity.
- Merging duplicates preserves aliases, provenance, and incoming references.
- A summary cannot replace its sources.
- A secret reference must not leak protected content through a public label or inverse index.

## Derived Views

A **Derived View** is a generated or maintained presentation assembled from authoritative records for a particular task or audience.

Examples include:

- a character dashboard;
- a location briefing;
- an NPC relationship summary;
- a session preparation packet;
- a current threat list;
- a player-facing knowledge journal;
- a chronological recap.

Derived Views may improve usability. They must identify their source records or be reproducible from them. They cannot:

- become a competing source of truth;
- hide material uncertainty;
- silently change an authoritative value;
- expose GM Secrets to the wrong audience;
- merge distinct identities;
- flatten history into an unsupported current claim.

When a Derived View conflicts with its owner, repair or regenerate the view.

## Save Index and Protocol

The **Save Index** is the control-plane entry point for one campaign continuity.

It logically identifies:

- Campaign ID and title;
- current Campaign Version;
- active Repository Version and rules profile;
- last confirmed save or integration point;
- registered modules and partitions;
- module schema or representation versions;
- authority and visibility boundaries;
- pending Session Delta, if any;
- latest validation result;
- latest migration and available backup references;
- unresolved conflicts and Record Gaps;
- authorized GM and participant interfaces where applicable.

The Save Index does not duplicate module contents. It tells a GM or tool what exists, where its logical owners are, which versions apply, and whether the campaign is safe to load.

The **Protocol** portion points to the [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md), [Migration and Versioning](MIGRATION_AND_VERSIONING.md), [Continuity Resolution](CONTINUITY_RESOLUTION.md), and [Persistence Validation](PERSISTENCE_VALIDATION.md). It does not invent those procedures here.

## Campaign Canon and Rules Profile

This control module owns campaign-level commitments permitted by [Persistence Authority](PERSISTENCE_AUTHORITY.md), including:

- campaign identity and continuity boundary;
- active repository revision;
- adopted options and campaign premises;
- accepted Provisional Rules and status;
- authorized retcons;
- conversion decisions;
- control and consent agreements that materially affect continuity;
- campaign version history.

It never copies the full rules repository into a save. It records exact version references and campaign-specific choices.

## Player State

Player State owns current information needed to represent each player-controlled character without collapsing the character into one stat block.

It logically separates:

- control identity and player-agency boundary;
- current actor and Incarnation references;
- current embodiment and condition;
- current location and access;
- current Development and Skill expressions through their proper Profiles;
- current Soul access through linked Soul records;
- current goals, Standing Instructions, and delegated authority where explicitly established;
- current resources and custody through Inventory references;
- current information through Knowledge references;
- pending consequences and Review Points.

Player State does not own the mechanics it references and does not store a player's unexpressed intentions as character fact.

## Souls and Incarnations

This module preserves stable Soul identity and its relationship to distinct lives.

It may own or index:

- Soul identity;
- Incarnation identities and order;
- Final Death and Life Reconciliation references;
- Interlife intervals;
- eligible persistent Imprints;
- current access and suppression distinctions;
- Soul Echo, Title, Resonance, Depth, Constellation, Space, Avatar, and Weapon-bond references;
- unresolved Soul harm and recovery;
- world-bound losses and surviving consequences.

The [Soul Engine](../soul/README.md) remains the mechanical owner. This module records established campaign results and never treats all persistence as current access.

## Life Archive

The [Life Archive](LIFE_ARCHIVE.md) owns stable Life IDs, active/completed archive status, the Soul Overview index, finalized Life Summaries, historical-reference sets, and Life Over derivation. It references Souls, Incarnations, Final Death, Timeline, Campaign History, Relationships, Development, Skills, Species, and other owners rather than copying their mutable state.

The active Life points to current owners and is not finalized. A completed Life may preserve historical peaks and milestones. Player visibility of a summary does not create Character Knowledge.

## Companions

Companions owns the campaign role and continuity view for recurring allied, dependent, bonded, travelling, summoned, created, or otherwise accompanying persons and agents.

It references rather than replaces:

- actor identity;
- embodiment;
- Relationships;
- knowledge;
- location;
- goals and agency;
- custody and resources;
- party or travel arrangements;
- Soul Weapon personhood where applicable.

Companion is not an ownership claim or permanent loyalty state. A companion remains an autonomous person unless another canonical category explicitly says otherwise.

## Actors

Actors owns current campaign records for NPCs and other agentive beings not represented primarily through Player State or Companions.

It preserves stable identity, Person Basis, current embodiment, Observer View references, motives, commitments, activity, location, capability references, and Continuity Core without scripting future choices. [NPC Generation](../gm/NPC_GENERATOR.md) remains the procedure owner for generated NPC Profiles.

Actors, Companions, and Player State do not independently own duplicate identities. Persistent non-autonomous beings share the Entity identity anchor defined by [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md). Persistent autonomous entities and systems remain reserved for FR-014 rather than being forced into Actors.

## Autonomous Registry

The [Autonomous Registry](AUTONOMOUS_REGISTRY.md) owns persistent autonomous identity and autonomy-specific state for independently operating entities and systems. It distinguishes Model from Individual, supports bounded Groups, preserves Controller separation, reconstruction and memory lineage, networks, assignments, and last-confirmed uncertainty, and references specialist owners for all other mutable facts.

An autonomous subject cannot simultaneously hold a second ordinary Entity identity anchor. Infrastructure that is autonomous keeps Infrastructure state under Infrastructure and references one Autonomous ID for autonomy-specific identity and control.

## Relationships

Relationships owns durable intersubjective and organizational relationship records, including their participants, history references, present dimensions, commitments, conflicts, and unresolved issues.

It does not reduce relationships to one score or duplicate each participant's entire biography. The [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md) defines its semantics.

## Species

Species owns campaign-specific Species References, observed forms, populations, lineages, known routes, local names, uncertainties, and extinction or replacement history.

It references [Monster Evolution](../monster-evolution/README.md), [Species Development](../progression/SPECIES_DEVELOPMENT.md), ecology, research, and world records. It does not make a discovered species description universal, complete, or mechanically authoritative beyond its evidence and scope.

## World

World owns the top-level current world-state graph and links its domains without duplicating specialist records.

It indexes:

- Contact Domains and world boundaries;
- Ages and current Age Claims;
- regions and major scopes;
- populations, ecologies, resources, economies, factions, wars, diseases, advancement, Dungeons, Stability assessments, Gates, and Pending Consequences;
- active Simulation Frames and Review Points;
- world-scale uncertainties and causal dependencies.

The [World Engine](../world-engine/README.md) remains the simulation owner.

## Locations

Locations owns stable place identities and current place-specific conditions, access routes, boundaries, occupants, claims, environment, hazards, infrastructure, and historical references.

An occupant list is a Derived View when current placement is owned by subject-placement relations. A concrete schema may instead make a normalized placement relation part of Locations, but it must then be the sole owner of that current-placement claim. Independent current-location values in both subject and place records are forbidden.

A place can persist through renaming, damage, abandonment, reconstruction, occupation, magical change, or disputed ownership without treating every continuity claim as automatic.

## Factions and Institutions

This module owns campaign-specific faction and Institution identities, Versions, participants, authority routes, interests, current activities, relationships, information routes, and continuity claims.

It references [Faction Behaviour](../world-engine/FACTION_BEHAVIOUR.md), [Faction Generation](../gm/FACTION_GENERATOR.md), and [Institutions and Academies](../human/INSTITUTIONS_AND_ACADEMIES.md) rather than turning organizations into single minds.

## Research

Research owns questions, observations, hypotheses, experiments, evidence, theories, confidence, disputes, disproof, confirmation, loss, and rediscovery.

It references Knowledge for who knows what and the relevant world or specialist record for confirmed truth. The [Research Engine](RESEARCH_ENGINE.md) defines its lifecycle.

## Magic

Magic owns campaign-specific current Profiles, sources, access relationships, discovered procedures, active effects, infrastructure, restrictions, traces, and unresolved magical claims.

It preserves separate records or references for Mana, Affinities, Spells, Rituals, Enchantments, Alchemy, Divine Magic, Forbidden Magic, magical Development, and world effects. It does not create a generic magic score or duplicate the [Magic rules](../magic/README.md).

## Infrastructure

Infrastructure owns maintained systems whose function depends on connected people, knowledge, resources, access, sites, tools, institutions, supply, repair, and operating conditions.

Examples include roads, farms, workshops, archives, hospitals, wards, laboratories, schools, communication networks, and water systems. Records preserve dependencies, capacity by function, condition, access, maintenance, failures, and provenance rather than one infrastructure level.

## Inventory and Custody

Inventory owns ordinary possessions, quantities where established, condition, location, custody, claims, provenance, containers, loss, transfer, consumption, and dependencies.

It distinguishes ownership from custody, access, use, legal claim, bond, and knowledge. Soul Weapons remain persons and bonded partners under their own records; an Inventory reference may describe vessel location or custody without reducing them to equipment.

## Timeline

Timeline owns chronological placement and temporal relationships among events, intervals, sessions, lives, Ages, time skips, and parallel processes.

It does not replace the fuller Campaign History. The [Timeline Engine](TIMELINE_ENGINE.md) defines chronology semantics.

## Campaign History

Campaign History owns the durable account of established events and corrections under the Historical Record authority layer.

It preserves event identity, participants, causes, outcomes, information effects, state changes, consequences, sources, and supersession. It references specialist records rather than copying their full mechanics.

## Projects

Projects owns intentional and emergent undertakings with objectives, participants, authority, methods, resources, dependencies, milestones, current state, interruptions, risks, outputs, and consequences.

Training, research, construction, travel, reform, recovery, investigation, and institution-building remain governed by their specialist rules. Elapsed time alone never completes a Project.

## Mysteries

Mysteries owns bounded unresolved questions relevant to play, including known clues, open branches, interested actors, stakes, discovery routes, false leads with provenance, and resolution conditions.

A Mystery is not authority to invent a hidden answer later. Established hidden truth belongs to its factual owner; genuinely unresolved questions remain unresolved.

## Knowledge

Knowledge owns observer-specific access to facts, memories, records, beliefs, interpretations, estimates, and unknowns.

It references authoritative facts without copying protected truth into every observer record. [Truth Layers](TRUTH_LAYERS.md) defines the distinction among Character Knowledge, Research, Player Theories, Rumours, and related layers.

## Secrets

Secrets is the protected access view for established or deliberately prepared information not available to specified participants.

It does not create a second world truth. Every secret references an authoritative owner, identifies who may access it under [Truth Layers](TRUTH_LAYERS.md), and avoids leaking content through indexes, aliases, metadata, or Derived Views. A GM draft or preferred future is not an established secret merely because it is hidden.

## Validation

Validation owns check definitions, runs, results, warnings, unresolved conflicts, and repair references.

It checks architecture and campaign coherence without changing facts by itself. [Persistence Validation](PERSISTENCE_VALIDATION.md) defines validation profiles, findings, outcomes, and repair routing.

## Migration History

Migration History owns campaign-version transitions, backups, audits, manifests, merges, validation results, warnings, conflicts, and rollback references.

It does not store backups inside the rules repository and does not make a migration valid merely by recording one. [Migration and Versioning](MIGRATION_AND_VERSIONING.md) defines the migration procedure.

## Module Partitioning

A module may be partitioned when size, visibility, ownership, or access requires it.

Valid partition keys may include:

- campaign region;
- Contact Domain;
- Age;
- actor or Soul;
- institution;
- archive period;
- security boundary;
- record type.

Partitioning must preserve a module index, unique identities, cross-partition references, authority, chronology, and validation. Moving a record between partitions changes storage placement, not subject identity or campaign truth.

## Loading and Dependency Closure

A GM or tool need not load every campaign record before every action. It must load the **dependency closure** needed for the claim.

At minimum:

1. load the Save Index and active rules profile;
2. identify the current actors, location, time, and claim;
3. load authoritative records for directly affected subjects;
4. follow material typed references to Relationships, Species, Locations, Projects, Research, Timeline, Knowledge, Secrets, or other owners;
5. include unresolved consequences and pending Session changes;
6. stop and mark missing dependencies rather than invent them.

The [Campaign State Model](CAMPAIGN_STATE_MODEL.md) defines required Read Sets for common adjudications.

## Update Boundaries

Architecture identifies what an update may touch; the [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md) defines exactly how.

An update set should be:

- claim-bounded;
- owner-routed;
- reference-complete;
- history-aware;
- authority-valid;
- reversible through backup or correction where required;
- validated before being treated as a confirmed save.

Unrelated records are not rewritten for formatting convenience during a gameplay update.

## Worked Examples

### One Person, Several Records

A reincarnated player character has a stable Soul ID, a current Incarnation ID, a Player State record, Relationship records, Knowledge records, Inventory references, Skills and Development Profiles, and Campaign History events.

These are not duplicates. Each owns a different claim and references the others. Renaming the current body updates an alias without changing the Soul ID. Final Death closes the Incarnation state without deleting its history.

### A Secret Research Site

A hidden laboratory has a Location record, Infrastructure dependencies, an owning faction reference, current Magic records, Research projects, Inventory custody, and a Secret access view. The Secret record does not duplicate the laboratory's factual state; it controls exposure of references and interpretations.

Destroying the site updates Location and Infrastructure through a historical event, then routes effects to Research, Inventory, Relationships, faction activity, and Knowledge only where causally affected.

### Duplicate NPC Names

Two records called Mira appear after a transcript import. Their names are not enough to merge them. Keep separate IDs, compare Person Basis, relationships, locations, chronology, and source provenance, then either confirm distinct people or perform a traceable identity merge.

### A Derived Character Sheet

A character dashboard displays body condition, accessible Skills, carried items, relationships, and known research. It is a Derived View assembled from several owners. Editing the dashboard cannot silently change those records; an authorized update routes each change to its owner.

## Safeguards

- Logical modules are canonical; storage products are not.
- Every material fact has one authoritative record owner.
- Stable IDs, not names or paths, preserve identity.
- Derived Views never become competing truth.
- Cross-module facts use typed references rather than uncontrolled copying.
- Missing references remain Record Gaps or unknowns rather than invented replacements.
- Loading only part of a campaign does not authorize ignoring material dependencies.
- Secrets cannot leak through indexes, aliases, inverse links, or summaries.
- A module records specialist outcomes without redefining their mechanics.
- Architecture does not grant actor control, progression, resources, success, or future outcomes.
- Populated modules, saves, backups, and migrations remain outside this repository.
- [Blank templates](../../templates/README.md) derive from this architecture rather than silently changing it.

## Simulation Identity Extension Points

The campaign graph must have conceptual homes for objective Entity state, stable persistent identity, scoped Controller relationships, active Perspective when continuity depends on it, Entity Knowledge, and GM Secrets. These concerns remain distinct even when one storage record references several of them.

Current modules and typed references can represent this separation without a mandatory schema migration. A future Knowledge System or Autonomous Registry may normalize additional records, but it may not collapse Entity, Controller, Perspective, or truth-layer ownership. Groups and populations may remain aggregates until individual continuity matters.

## Scope Boundaries

This document defines logical organization and record interfaces. It delegates these semantics to their dedicated owners:

- truth-layer ownership, visibility, and promotion;
- persistence-level lifetime and deletion;
- complete module field requirements;
- Relationship and Research lifecycles;
- chronology rules;
- migration procedure;
- continuity correction;
- save transactions;
- validation algorithms;
- storage-specific templates.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Campaign Persistence Philosophy](CAMPAIGN_PERSISTENCE_PHILOSOPHY.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Campaign Persistence Integration](CAMPAIGN_PERSISTENCE_INTEGRATION.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Game Master Responsibilities](../gm/GM_RESPONSIBILITIES.md)
- [World Engine](../world-engine/README.md)
- [Canonical Rules Map](../README.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
- [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
