# GM Living Codex

## Purpose

This document defines the authoritative design for the **GM Living Codex**, a persistent cross-campaign library of reusable GM-approved world-design assets. Its first and primary module is the Species Registry.

The Codex prevents useful species, Evolution Routes, characteristic capability routes, variants, and ecological designs discovered or refined during play from being lost or needlessly reinvented. Entries such as Cave Gatherers, Forgekeepers, Echohorns, distinctive humanoid lineages, and non-humanoid player species are appropriate kinds of reusable design, but no example in this document creates a populated Codex record.

## Core Rule

The Living Codex records reusable GM design. It does not record what one campaign currently contains.

A Codex entry applies existing Eternal Cycle mechanics; it cannot create an exception to them. A campaign may adopt an entry, adapt it explicitly, or ignore it. Neither campaign play nor player belief silently changes Living Codex Canon.

## Authority Model

```text
Eternal Cycle Rules Repository
  -> GM Living Codex
  -> Campaign Configuration
  -> Campaign Canon
```

### Eternal Cycle Rules Repository

The repository defines how species, Development, Skills, Evolution, Magic, Souls, ecology, world simulation, GM procedures, and persistence work. A Codex entry must satisfy those owners and cannot replace or amend them.

### GM Living Codex

The Codex defines reusable GM-approved species and other reusable world assets. Its authority concerns the accepted reusable design, not whether that design exists in a particular campaign.

### Campaign Configuration

Campaign Configuration identifies the Codex version and entries a campaign may consult, along with explicit exclusions, variants, divergences, and deployment details. Those values remain campaign-external and do not belong in universal Codex documents.

### Campaign Canon

Campaign Canon establishes what is actually true in one campaign. It may use a Codex entry unchanged, use a regional or environmental variant, diverge from an entry, disable or add an Evolution branch, reinterpret culture or ecology, or create a new campaign-local species.

A lower layer never silently rewrites a higher layer. An explicit campaign divergence is not a silent override: it is a scoped Campaign Canon decision that preserves the base Codex reference and records the difference. A later Codex revision does not retroactively migrate an existing campaign.

## Distinct Authority Questions

The Codex authority order and the [Campaign Persistence authority hierarchy](../persistence/PERSISTENCE_AUTHORITY.md) answer different questions:

- the Codex order answers which reusable design a campaign starts from;
- Campaign Persistence answers which established claim currently governs within that campaign.

Once adopted, campaign records remain authoritative for that campaign's actual species, populations, discoveries, and divergences. The Codex remains the reusable baseline, not a competing campaign save.

## GM Authority

The GM determines what becomes Living Codex Canon through the Codex review and full-save procedure. Player theories, incomplete observations, mistaken beliefs, generated drafts, and campaign rumours do not become Codex entries unless the GM deliberately accepts a reusable design after canonical review.

The Codex records the GM's current accepted design rather than every earlier uncertainty. An entry may be:

- expanded or corrected;
- renamed while retaining stable identity;
- split or merged with traceable replacement relations;
- deprecated or replaced;
- assigned additional Evolution Routes;
- assigned reusable variants;
- assigned characteristic traits or Skill access routes.

Editing an entry changes Living Codex Canon prospectively. It does not silently rewrite Campaign Canon or erase revision history.

## What the Living Codex Is Not

The Living Codex is not:

- a player-facing encyclopedia or automatic Character Knowledge;
- one campaign's save, history, observation log, Research, rumours, theories, or GM Secrets;
- a universal law requiring every campaign to use every entry;
- a source of current population, location, faction, ecology, or world state;
- a boss archive, unique-NPC archive, or gallery of singular plot entities;
- a replacement for Species Development, Monster Evolution, Skills, Magic, Souls, or the World Engine;
- a reason to expose hidden design facts during play;
- a repository for credentials, provider identifiers, private URLs, or deployment secrets.

## Automatic Consultation

The GM should consult the latest configured Living Codex automatically when:

- generating an ecosystem;
- creating or identifying a species;
- creating a player species;
- selecting or creating Evolution options and branches;
- assigning species-characteristic traits or Skill routes;
- generating a regional or environmental variant;
- checking for redundant designs;
- deciding whether an encountered campaign-local design is reusable.

Consultation does not require reuse. The operating sequence is:

```text
Need a species, variant, or Evolution
  -> search the Living Codex
  -> reuse a suitable entry when appropriate
  -> create a campaign divergence when differences are limited
  -> create a new campaign-local design when materially distinct
  -> promote only an accepted reusable design
  -> validate and back up the complete Codex
```

## Inclusion Criteria

The Codex primarily includes reusable ecological or societal species that meaningfully interact with characters or the world. A suitable entry commonly has several of these qualities:

- recurring or broadly reusable;
- recognizable behavior without prescribing individual personality;
- meaningful ecology, communication, culture, or social role;
- useful Species Traits and bounded capability routes;
- defined Evolution possibilities;
- potential as a playable species;
- value for future procedural world generation.

Humans, elves, dwarves, beastkin, and other humanoids belong in the same registry when a distinct interpretation needs stable identification. Generic names are labels rather than unique identities: unrelated designs called `Elf` require separate stable Codex IDs.

## Exclusions

Do not normally include:

- unique bosses or singular villains;
- one-off horrors or plot-specific entities;
- temporary Mutations or transformations;
- environmental hazards without reusable species identity;
- unique divine beings;
- an individual merely because they are powerful or memorable.

A boss-associated design may enter only when it describes a reusable species beyond the unique boss. The unique individual and campaign history remain outside the Codex.

## Player Species and Individual State

Distinctive non-humanoid player species use the same Species Registry as every other species. The Codex does not maintain a separate player-species catalog.

Species-level design may include anatomy, baseline traits, Current Instincts, species Skill routes, Evolution branches, ecology, communication, renewal, typical magical architecture, and cultural possibilities. Individual-level facts remain in Campaign State, including personal Skills, earned mastery, Mutations, injuries, Soul Titles, Soul Weapons, relationships, memories, unique personal Evolution, and campaign history.

A personal capability becomes species-level only through deliberate GM adoption as reusable design. Frequency inside one campaign is not sufficient.

## Stable Identity

Every reusable entry uses a permanent stable identifier independent of its preferred name. The preferred species identifier form is `EC-SPECIES-000001`, incremented or otherwise allocated without reuse under the Codex indexing procedure.

Names, aliases, classifications, and taxonomy may change. Stable IDs survive renaming, correction, deprecation, merging, splitting, and replacement through explicit typed relations. Shared names never justify merging entries.

## Campaign Divergence and Promotion

A campaign may record a divergence from a Codex entry without modifying the base design. The divergence identifies:

- the base Codex ID and revision;
- the exact changed claims;
- the campaign scope and cause;
- affected traits, Skills, Evolution access, ecology, Magic, or interpretation;
- whether the divergence is temporary, regional, lineage-specific, or persistent.

A reusable divergence may later be proposed as a Codex variant or distinct species. Promotion requires GM approval, stable identity, canonical review, and the Codex full-save procedure. Campaign facts and evidence are summarized only as needed; the populated campaign record is never copied into this repository.

## Approved Implementation Plan

This plan is the authoritative order for implementing the Living Codex architecture. It is a maintenance plan, not a numbered development phase and not a claim that a populated deployment exists in this repository.

### Step 1 - Codex Philosophy and Authority

Define purpose, ownership, exclusions, campaign divergence, and the relationship among repository rules, Living Codex Canon, Campaign Configuration, and Campaign Canon.

### Step 2 - Stable Identity and Indexing

Define stable IDs, aliases, renaming, merging, splitting, deprecation, replacement, revision identity, and lookup behavior.

### Step 3 - Species Core Records

Define anatomy, cognition, lifecycle, ecology, behavior, communication, society, culture, Magic, and design intent.

### Step 4 - Traits and Capability Ownership

Define Innate Traits, passive biological traits, sensory traits, characteristic access, and boundaries with learned Skills and Development.

### Step 5 - Species and Evolution Skills

Define Instinctive Skills, Typical Learned Skills, Cultural Skills, Rare Species Skills, and Evolution Skills without granting them universally.

### Step 6 - Evolution Graph

Define branches, provenance, requirements, gained and lost traits, available Skills, restrictions, tradeoffs, consequences, and further Evolutions.

### Step 7 - Variants and Campaign Divergences

Define regional and environmental variants, campaign overrides, promotion rules, and the threshold for assigning a distinct species identity.

### Step 8 - Procedural-Generation Integration

Define search tags, ecological roles, reuse priority, design intent, redundancy checks, and automatic GM consultation procedures.

### Step 9 - Player-Species Integration

Define how playable species use the same Codex while personal state, earned capability, Soul history, and campaign continuity remain campaign-specific.

### Step 10 - SQLite Persistence Schema

Define normalized storage, constraints, stable references, revisions, migrations, and validation for one separate Living Codex database.

### Step 11 - Google Drive Deployment and Full-Save Protocol

Define the canonical copy, current backup, dated snapshots, manifests, remote replacement, read-back, exact comparison, and recovery behavior.

### Step 12 - Reproductive Compatibility

Define sparse directional species relationships, nonzero scoped probabilities, assistance methods, viability and fertility separation, Evolution and variant boundaries, and the full-save procedure. This step is only the compatibility scaffold; it does not define courtship, pregnancy, heredity, hybrid development, family structures, or the later lineage and inheritance system.

## Implementation and Deployment Boundary

The approved plan governs future Codex implementations and the design documents in this family. It does not place a populated database in Git, authorize a campaign migration, or require one storage provider for every implementation.

The canonical design requires one separate Living Codex SQLite database and a verified remote deployment workflow. The dedicated persistence document owns those operational details. Repository documents may include logical schemas and blank templates; actual Codex entries, database files, backups, manifests, configuration, and credentials remain external.

## Safeguards

- Repository rules always constrain Codex entries.
- Codex entries never become automatic player knowledge.
- Campaign divergence never silently overwrites a base entry.
- Codex revision never silently migrates an existing campaign.
- A memorable individual never becomes a species entry by fame alone.
- A generated draft never becomes Codex Canon by being produced.
- Shared names never establish shared identity.
- Personal Skills, Development, Souls, relationships, and history never become species defaults without explicit reusable adoption.
- The Codex never stores live campaign state or deployment secrets.
- The implementation plan creates no new numbered roadmap phase or release status.

## Related Documents

- [GM Living Codex Index](README.md)
- [Design Philosophy](../core/DESIGN_PHILOSOPHY.md)
- [Species Development](../progression/SPECIES_DEVELOPMENT.md)
- [Monster Evolution Rules](../monster-evolution/README.md)
- [Skill Engine](../skills/README.md)
- [Magic Rules](../magic/README.md)
- [Soul Rules](../soul/README.md)
- [World Engine](../world-engine/README.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Campaign Persistence Engine](../persistence/README.md)
- [AI Game Master Operations](../ai/README.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
