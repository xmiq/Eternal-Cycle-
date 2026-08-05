# Campaign Persistence Philosophy

## Purpose

The Campaign Persistence Engine preserves the causal identity of an Eternal Cycle campaign. It records enough established truth, history, information, relationships, work, uncertainty, and provenance for later play to continue from what actually happened rather than from whatever a narrator happens to recall.

This is a canonical game system. It defines what reliable continuity must preserve and how persistence relates to other systems. It does not define a universal file format, database, cloud service, template, or populated campaign.

## Core Rule

Persistence preserves **causality**, not merely variables.

A campaign remains continuous when later play can recover:

- what was established;
- which system or actor established it;
- when and where it applied;
- what caused it;
- who knew, believed, concealed, or misunderstood it;
- what it changed;
- what remained unresolved;
- which later facts depend on it.

A current value without that context may be insufficient. A history without a usable current state may also be insufficient. The Campaign Persistence Engine keeps both connected without making every detail equally permanent.

## The Three-System Relationship

The World Engine, Campaign Persistence Engine, and GM Toolkit have different jobs.

| System | Owns | Does not own |
| --- | --- | --- |
| **World Engine** | How world conditions, actors, processes, and consequences change through time | How a campaign stores or recovers those established changes |
| **Campaign Persistence Engine** | How established campaign reality, history, knowledge, and continuity are represented, retained, corrected, migrated, and validated | The specialist mechanics that produce an outcome or the GM judgment that adjudicates one |
| **GM Toolkit** | How a human or AI GM prepares, adjudicates, presents, and records play through canonical owners | Permission to overwrite established state, invent missing facts, or redefine persistence rules |

In short:

> The World Engine simulates reality. The Campaign Persistence Engine remembers reality. The GM Toolkit operates through that memory.

The relationship is a handoff, not a hierarchy of replacement. When a famine changes a settlement, the World Engine and its specialist domains establish the change. Persistence records the established causes, scope, observations, consequences, and current condition. The GM later reads that record before representing the settlement.

## Continuity Is Gameplay

Persistence is not clerical work added after play. What survives, changes, is remembered, is forgotten, is discovered, or remains disputed can shape future choices as directly as combat or magic.

### History Is Gameplay

Past events create claims, ruins, obligations, evidence, absences, traditions, traumas, opportunities, and misunderstandings. A destroyed bridge changes routes. A failed treaty changes expectations. A former incarnation's public actions may outlive the body while its legal authority does not.

History is not a decorative recap. It is a causal input to later play.

### Research Is Gameplay

Observation, competing explanations, experiments, evidence, confirmation, error, loss, and rediscovery create meaningful decisions. A theory can guide preparation while remaining uncertain. A disproved idea can persist socially. Confirmed knowledge can still be inaccessible to a particular actor.

Research is not a list of facts the GM reveals on schedule.

### Relationships Are Gameplay

First meetings, trust, hostility, promises, betrayal, family, shared work, debt, dependence, grief, and repair persist through consequences. Relationships change through actual interactions and elapsed processes; they do not reset when a character leaves the scene or changes body.

Relationship memory is not a single approval score.

### Infrastructure Is Gameplay

Roads, workshops, archives, farms, wards, institutions, supply routes, laboratories, and other maintained systems embody prior labor and dependency. Their condition, access, ownership, knowledge, and failure routes matter. Infrastructure cannot appear fully functional because a later scene needs it, nor vanish because it is inconvenient to track.

### Memory Is Gameplay

World records, character knowledge, Soul memory, institutional memory, oral tradition, rumours, player notes, and hidden facts are not one pooled information source. Who can access which account, with what confidence and distortion, changes available action.

Memory is not omniscience and forgetting is not permission to rewrite truth.

## What Persistence Means

Campaign persistence is the durable representation of campaign-specific reality under canonical rules.

It includes more than a last known snapshot. Depending on material relevance, reliable persistence may preserve:

- stable identities and continuity across aliases, forms, offices, deaths, and Reincarnations;
- current conditions and the dated history that produced them;
- actor-specific knowledge, beliefs, uncertainty, and secrets;
- relationships and their material turning points;
- research claims and their evidence states;
- projects, infrastructure, dependencies, and interruptions;
- possessions, custody, loss, and provenance;
- unresolved consequences, commitments, mysteries, and causal processes;
- campaign rules profile, revisions, provisional rulings, and migrations;
- enough validation metadata to detect contradiction and unsupported change.

Not every interaction needs equal detail. Persistence is proportional to future consequence, not narrative word count.

## Persistence Is Not Storage

The canonical model is logical rather than technological.

A campaign may implement it with:

- Markdown;
- a relational or document database;
- a local application;
- Git or another versioned store;
- cloud documents;
- a tabletop binder;
- a combination of systems.

Storage technology may provide useful features such as transactions, search, access control, history, or backups. Those features do not define Eternal Cycle canon. A format is valid only insofar as it preserves the distinctions and procedures required by this engine.

Changing storage technology must not change established campaign truth by itself.

## The Structured Record Is Campaign-External

Every populated Campaign Record remains outside the canonical rules repository.

This repository may contain:

- persistence rules;
- logical schemas;
- validation requirements;
- unpopulated reusable templates in their roadmap phase;
- examples that do not establish a live campaign.

It must not contain:

- a current player character;
- an active Soul state;
- a live world snapshot;
- a populated inventory;
- current relationships;
- active quests or projects;
- campaign research;
- a playthrough timeline;
- GM Secrets for a real campaign;
- migration backups or save archives.

Repository Canon defines the rules by which an external campaign record is interpreted. The record preserves one campaign's established reality. Neither silently becomes the other.

## Persistent Truth and Conversation Context

Conversation, narration, transcripts, and working memory may help a GM recover context. They are not a safe replacement for structured persistence.

Conversation context may:

- supply a source for audit;
- preserve recent wording or intent;
- reveal a possible omission;
- help reconstruct an authorized record update.

Conversation context may not silently:

- overwrite established structured state;
- promote an unsupported detail to campaign truth;
- erase an older consequence because it fell outside the current context window;
- merge player knowledge with character knowledge;
- create a number, item, relationship, Skill, or progression change;
- defend a contradiction merely because the conflicting narration is more recent.

The later authority and continuity documents define the exact resolution procedures. The philosophical boundary is already binding: remembered prose supplements persistence; it does not govern persistence by recency alone.

## Persistence Principles

### 1. Preserve Causes With Results

Material state should retain enough provenance to explain why it exists. A scar, title, ruined district, hostile faction, or confirmed discovery should not become an unsupported floating fact.

### 2. Preserve Change Without Freezing the World

Persistence protects continuity, not stasis. Actors may reconsider, relationships may heal, species may disappear, infrastructure may decay, and institutions may transform. Change requires a valid route and becomes part of the record.

### 3. Record Each Fact Once, Reference It Elsewhere

One authoritative record should own a campaign fact. Other sections reference that identity or event rather than maintaining unsynchronized copies. Views and summaries may derive from the owner but cannot silently diverge from it.

### 4. Keep Current State and History Connected

The current state answers what is true now. History explains how it became true. Neither should be rewritten to conceal the other.

### 5. Keep Knowledge Separate From Truth

A belief may be consequential without being correct. A secret may be true without being known. A research theory may be useful without being confirmed. Persistence must retain those differences.

### 6. Missing Means Unknown

An absent material fact remains unknown unless a canonical procedure supports establishing it as a previously immaterial open detail. Missing information does not authorize the most convenient answer.

Useful labels include `Unknown`, `Not Yet Verified`, `Estimated`, `Requires Source Recovery`, and `Player Theory`. Later documents define their exact placement and validation.

### 7. Numbers Require Causes

A numerical value changes only through an established event owned by the relevant mechanic. Recalculation may correct an error, but it must be traceable. Persistence never smooths, balances, or guesses numbers silently.

### 8. Detail Follows Materiality

Record enough to preserve future causal play. A named promise may need exact participants and conditions. An uneventful week of ordinary meals may need no itemized history. Compression cannot erase a decision, dependency, cost, or unresolved consequence.

### 9. Correction Is Explicit

Errors, outdated records, missing sources, authorized retcons, and intentional in-world deception are different conditions. The record must distinguish them rather than rewriting whichever layer is easiest.

### 10. Persistence Does Not Predetermine the Future

Recorded plans, pressures, trajectories, and probabilities are not guaranteed outcomes. Autonomous actors, new evidence, opposition, and changed conditions remain able to alter them through their owning systems.

## What the Engine Owns

The Campaign Persistence Engine owns rules for:

- campaign continuity across interactions and sessions;
- the authority and provenance of campaign records;
- logical separation of truth, knowledge, theory, secrecy, and meta material;
- persistence lifetimes and promotion between them;
- modular campaign-state organization;
- durable relationship memory;
- research history and confidence;
- campaign chronology;
- migration and versioning;
- contradiction and continuity resolution;
- bounded save updates;
- persistence validation.

These owners will be defined in later Phase 10 tasks. Naming them here does not implement them early.

## What the Engine Does Not Own

Persistence does not decide:

- whether a Soul Imprint forms;
- how Development is earned;
- whether a Skill succeeds or evolves;
- whether a monster changes form;
- whether a Class, Profession, or institution recognizes someone;
- whether a Weapon Soul awakens or consents;
- whether magic functions;
- how a population, economy, faction, disease, Dungeon, Gate, Age, or World Reset changes;
- what an autonomous actor chooses;
- what a player character deliberately says, thinks, believes, or does;
- which uncertain outcome a valid resolution method selects.

Those results come from their canonical owners. Persistence records the established inputs, outcome, state changes, information effects, and unresolved follow-up without recreating the mechanic.

## Cross-System Interfaces

| System | Persistence receives | Persistence must preserve |
| --- | --- | --- |
| [Soul Engine](../soul/README.md) | established identity, incarnation, Imprint, memory, bond, and Soul-state changes | distinction between Soul persistence, present access, embodiment, and world-bound loss |
| [Development](../progression/README.md) | established growth, rust, injury, access, reliability, and retained progression | evidence, current embodiment, current-life effort, and noninterchangeable tracks |
| [Skill Engine](../skills/README.md) | established Skills, representations, access, practice, evolution, fusion, and cleanup | provenance, ownership, requirements, expression, and retained history |
| [Monster Evolution](../monster-evolution/README.md) | established species, forms, routes, mutations, ecology, and transitions | form history, embodiment, world consequences, and no implied route unlocks |
| [Human Classes and Professions](../human/README.md) | established affiliations, offices, recognition, work, institutions, and social claims | distinction between learning, standing, authority, reputation, and world-bound continuity |
| [Soul Weapons](../soul-weapons/README.md) | established Weapon Soul identity, bond, consent, vessel, form, Echo, and manifestation state | personhood, independent agency, vessel continuity, and non-inventory status |
| [Magic](../magic/README.md) | established magical sources, access, effects, traces, infrastructure, knowledge, and restrictions | source ownership, costs, maintenance, uncertainty, and no generic magic state |
| [World Engine](../world-engine/README.md) | established current conditions, transitions, events, consequences, and pending processes | causal provenance, scope, uncertainty, off-screen continuity, and autonomous agency |
| [GM Toolkit](../gm/README.md) | adjudication traces, information views, session changes, provisional rulings, and corrections | authority status, affected owners, player agency, and bounded updates |

## Worked Examples

### A Reincarnated Craftsperson

A smith dies after training an apprentice and founding a workshop. Persistence records the Final Death, Life Reconciliation, eligible Soul Imprints, the apprentice's relationship and knowledge, workshop ownership, unfinished orders, tools left in the world, and the institution's later changes.

The next incarnation does not receive the workshop, professional standing, mature hands, or current inventory. The soul may retain eligible progression under its owners. The world remembers the former life through actual people, records, objects, and consequences.

### A Forgotten Promise

An NPC promised to protect a village, then left the story for several sessions. The promise remains part of the relationship and historical record. Whether the NPC fulfills, breaks, renegotiates, or becomes unable to fulfill it depends on later causes and choices.

The GM cannot treat the NPC as a stranger because the conversation no longer contains the first meeting.

### Competing Research

Two schools explain a magical blight differently. Each theory retains its observations, methods, evidence, advocates, contradictions, and confidence. A later experiment may support one, disprove part of both, or reveal that they describe different scopes.

Persistence does not convert the most recently narrated theory into world truth.

### Rebuilding a Road

A trade road is destroyed and later repaired. The current location record may say the route is usable again, while history preserves the destruction, displacement, costs, changed ownership, new tolls, altered ecology, and relationships formed during reconstruction.

Repair changes current state; it does not erase the period when the road was absent.

## Safeguards

- No populated campaign data enters this repository.
- Repository rules and campaign records remain separate authorities.
- Storage convenience cannot collapse truth, knowledge, secrecy, and theory into one field.
- Recency alone cannot override established continuity.
- Missing facts remain unknown rather than convenient inventions.
- Numbers do not change without mechanically justified events or explicit correction.
- A summary cannot erase an identity, relationship, promise, discovery, dependency, or consequence that remains material.
- Persistence does not grant progression, access, ownership, consent, authority, or success.
- Persistence cannot freeze actors into recorded plans or make forecasts deterministic.
- A migration cannot silently rewrite campaign history.
- Meta discussion never becomes campaign truth merely because it shares a storage location or transcript.
- Templates remain a Phase 11 concern and are not implemented by this philosophy.

## Scope Boundaries

This document establishes the philosophy and ownership of campaign persistence. It deliberately does not finalize:

- the authority hierarchy;
- truth-layer update rules;
- persistence lifetimes;
- module schemas;
- relationship fields;
- research states;
- chronology structures;
- migration manifests;
- correction procedures;
- save-update steps;
- validators or templates.

Those subjects remain separate roadmap tasks.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Game Master Responsibilities](../gm/GM_RESPONSIBILITIES.md)
- [World Engine](../world-engine/README.md)
- [World-State Variables](../world-engine/WORLD_STATE_VARIABLES.md)
- [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md)
- [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md)
- [Consequence Resolution](../gm/CONSEQUENCE_RESOLUTION.md)
- [Design Philosophy](../core/DESIGN_PHILOSOPHY.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
