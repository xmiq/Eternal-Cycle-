# Migration and Versioning

## Purpose

This document defines how an Eternal Cycle campaign moves between persistence structures, repository revisions, campaign versions, storage systems, recovered sources, and corrected data without losing authority, identity, history, uncertainty, secrecy, or causal traceability.

Campaign persistence is treated as an evolving database even when implemented through Markdown, a document store, a relational database, a shared drive, Git, or another technology.

## Core Rule

Every Campaign Migration follows four ordered stages:

> Backup -> Audit -> Merge -> Validation

Backup and Audit are read-only. Merge updates only records traceable to the Audit Report. Validation must pass all blocking checks before the target becomes the active Campaign Version.

Every migration has one unique Migration ID and one Migration Manifest.

## Migration Is Not

A Campaign Migration is not:

- a World Migration under ecology rules;
- an ordinary gameplay Save Update;
- a silent retcon;
- a bulk rewrite for formatting preference;
- a summary replacing source records;
- an excuse to fill missing values;
- a way to promote Research, Rumours, Player Theories, or Meta into Campaign Canon;
- a way to expose GM Secrets;
- a repository-rules update;
- proof that imported data is true;
- valid merely because software completed without error.

The procedure governs campaign-external records. No populated campaign backup, Audit Report, Migration Manifest, or migrated state belongs in this repository.

## When Migration Is Required

A Campaign Migration is required when a campaign must materially change its persistence representation or canonical compatibility, including:

- adopting a new Repository Version whose rules affect established campaign records;
- changing the logical schema or module partitioning;
- importing a transcript, previous chat, exported Markdown, database, spreadsheet, campaign log, or other source;
- combining legitimate records from several campaign stores;
- recovering structured state from incomplete or damaged material;
- converting campaign data to a different storage technology;
- separating incorrectly merged identities;
- reconciling a historical campaign format with the current persistence architecture;
- applying an authorized bulk correction whose effects cross many owners;
- rebuilding indexes or references when stable identity and meaning must be preserved.

An ordinary Save Update uses the later Save Update Protocol when the logical model, authority, and version compatibility remain valid. A large number of changed records does not by itself make an update a migration; a structural or compatibility boundary does.

## Version Model

### Repository Version

The **Repository Version** identifies the exact Eternal Cycle rules and governance revision against which a campaign is interpreted.

It may be represented by a release tag, commit identifier, content-addressed revision, or another exact immutable reference. A branch name, moving link, latest label, or file modification date is not sufficient by itself.

A campaign records:

- active Repository Version;
- previously active Repository Version where relevant;
- adopted optional premises;
- active Provisional Rules;
- compatibility notes;
- migration or conversion decisions.

Changing Repository Version does not silently rewrite Campaign Canon. The Audit determines whether rules changes require conversion, preservation under a compatibility ruling, an authorized retcon, or no campaign change.

### Campaign Version

The **Campaign Version** identifies one validated Campaign State at a Save Point.

It records:

- unique version identifier;
- parent Campaign Version or Versions;
- active Repository Version;
- creation and Integration Time;
- included Session Deltas, migrations, and corrections;
- validation result;
- storage integrity reference where available;
- status such as Active, Superseded, Archived, Invalid, or Recovery Candidate.

Campaign Versions preserve ancestry. A later version may supersede an earlier one without deleting it. Version labels need not use semantic versioning, but their identity and parentage must be unambiguous.

### Persistence Model Version

The **Persistence Model Version** identifies the logical architecture and record contracts used by a Campaign Version.

It is storage-neutral. A Markdown and database implementation can use the same model version; two Markdown stores can use different model versions.

Changes that alter required ownership, stable identity, truth-layer handling, reference semantics, chronology, validation, or module contracts require an explicit model-version assessment.

### Storage Format Version

A storage implementation may record a **Storage Format Version** for file layout, serialization, database schema, encoding, or application compatibility.

Storage Format Version is implementation metadata. It cannot override Campaign Canon, Repository Canon, or the Persistence Model Version.

## Compatibility Assessment

Before migration, classify the source and target relationship:

| Classification | Meaning |
| --- | --- |
| **Compatible** | The target can represent all material source claims without semantic conversion |
| **Compatible With Conversion** | Deterministic, reviewed conversion is required but preserves meaning and authority |
| **Requires Adjudication** | Canonical interpretation, ownership, conflict, or player-impact questions must be resolved before merge |
| **Requires Authorized Retcon** | The accepted target intentionally changes established Campaign Canon |
| **Incompatible** | The target cannot responsibly represent required continuity without unresolved loss or contradiction |
| **Unknown** | Evidence is insufficient to classify compatibility |

Software readability is not semantic compatibility. A parser successfully opening a file proves only technical access.

## Migration Roles

Every migration identifies:

- migration initiator;
- campaign authority approving scope;
- Backup custodian;
- auditor;
- merge operator;
- validator;
- specialist owners consulted;
- players or affected participants whose choices or consent are material;
- visibility and Secret-access boundaries.

One person or tool may perform several roles, but the Manifest still records which responsibility was exercised. Automation does not become authority by executing the process.

## Stage 1 - Backup

### Rule

Create a complete, immutable recovery copy before any Audit-derived edits or target mutation occurs.

No edits occur during Stage 1.

### Backup Record

Record:

- Backup ID;
- creation date and time;
- source Campaign Version;
- source Repository Version;
- source Persistence Model and Storage Format Versions where available;
- complete included scope;
- excluded scope and reason;
- storage location or retrieval route;
- integrity proof where the implementation supports one;
- encryption, access, retention, and recovery requirements;
- custodian;
- successful recovery check;
- known pre-existing corruption or inaccessible material.

The backup must include the authoritative campaign records, indexes, manifests, migration history, validation results, and required Secret material within the authorized security boundary. A screenshot or prose summary is not a complete backup.

### Backup Safeguards

- Never overwrite the sole recoverable source.
- Never place a populated backup in the canonical rules repository.
- Never weaken Secret access merely to simplify backup.
- Never report backup success without checking that the retained material can be read or restored.
- Preserve pre-existing defects rather than repairing the backup copy.
- Record unavailable material as missing; do not reconstruct it during Backup.

If a complete backup cannot be made, the migration is blocked unless the authorized campaign authority explicitly enters a documented recovery operation with the risks preserved. That operation still does not claim a complete backup exists.

## Stage 2 - Audit

### Rule

Read the source material and produce an Audit Report. Nothing in the source or target Campaign State is modified during Stage 2.

### Possible Sources

Audit sources may include:

- gameplay transcripts;
- exported Markdown;
- structured campaign records;
- previous chat or interface context;
- Session Logs;
- repository revision and decisions;
- prior Campaign Versions;
- campaign notes;
- database exports;
- backup contents;
- validation reports;
- authorized player or GM testimony;
- earlier Migration Manifests.

Each source records provenance, authority, completeness, visibility, date, version, and known limitations. A recent source is not automatically more authoritative than an older structured record.

### Audit Classification

The Audit Report classifies source material into:

- canonical events;
- Research;
- progression;
- Infrastructure;
- Relationships;
- Species;
- Locations;
- Timeline;
- Mysteries;
- retcons;
- Meta discussion;
- inconsistencies.

It also identifies as applicable:

- Souls and incarnations;
- actors and companions;
- factions and institutions;
- Magic;
- Inventory and custody;
- Projects;
- Knowledge, Rumours, Player Theories, and GM Secrets;
- unknown or missing material;
- duplicates and identity candidates;
- numerical claims and mechanical traces;
- current, historical, Session, and Ephemeral material;
- unaffected records.

Classification does not decide truth by label. Every claim retains its authority, truth layer, source, owner, temporal scope, uncertainty, and dependencies.

### Audit Report

The Audit Report records:

- Migration ID and audited source scope;
- source inventory and versions;
- authority and visibility assessment;
- extracted material claims and stable identity references;
- proposed source-to-target mappings;
- differences between source and target model;
- duplicates, gaps, contradictions, and unsupported assertions;
- changes that can be converted mechanically;
- questions requiring specialist or campaign adjudication;
- possible retcons requiring authorization;
- affected records and dependency closure;
- records expected to remain unchanged;
- proposed validation plan;
- warnings, blockers, and unresolved conflicts.

The Audit Report is a proposal and evidence map. It changes no Campaign Canon and authorizes no merge by itself.

### Audit Safeguards

- Do not infer absent values from a target schema.
- Do not treat Meta discussion as in-world fact.
- Do not promote theories or Rumours because they appear confidently in prose.
- Do not merge records by name alone.
- Do not flatten observer-specific Knowledge into world truth.
- Do not discard contradictions before adjudication.
- Do not expose protected content in an audience-visible report.
- Do not rewrite the source to make extraction easier.

## Stage 3 - Merge

### Rule

Update only affected records, and make every change traceable to an Audit Report finding or an explicitly authorized resolution attached to that finding.

### Merge Plan

Before applying changes, create a bounded Merge Plan containing:

- source and target Campaign Versions;
- target Repository and Persistence Model Versions;
- included Audit findings;
- excluded or deferred findings;
- record-level create, update, link, supersede, split, merge, archive, or correction operations;
- stable ID handling;
- expected authority and truth layer for each claim;
- chronology and Knowledge effects;
- numerical-change provenance;
- Secret-access handling;
- dependency order;
- validation checks;
- rollback condition.

The plan may be dry-run against a separate candidate copy. A dry run does not modify the active Campaign Version.

### Merge Rules

- Preserve stable IDs whenever subject identity is unchanged.
- Create a new ID only for a genuinely distinct subject, event, version, or record owner.
- Record alias and supersession links rather than replacing names globally.
- Update through the Authoritative Record Owner.
- Preserve source wording or evidence where later interpretation may matter.
- Keep Current State, Historical Record, Session material, and correction history distinct.
- Preserve uncertainty and missingness.
- Preserve Character Knowledge, Research, Rumours, Player Theories, GM Secrets, and Meta in their proper truth layers.
- Preserve Event IDs, temporal precision, and chronology.
- Preserve numerical values only with valid mechanical traces.
- Preserve relationships as directional, historical, and participant-specific.
- Preserve Soul and Incarnation boundaries through Reincarnation.
- Preserve specialist ownership rather than translating everything into generic fields.
- Update only the Audit-defined dependency closure.

### Identity Merge and Split

When records may describe the same subject, compare stable identifiers, origin, Person Basis, chronology, embodiment, relationships, locations, provenance, and contradictions.

An **Identity Merge** requires affirmative evidence that records represent one subject and preserves all former IDs as aliases or source references.

An **Identity Split** repairs one record that incorrectly combined distinct subjects. It assigns valid identities, redistributes only supported claims, preserves the former record and correction provenance, and reports uncertain attribution rather than guessing.

### Conflicts During Merge

If Merge encounters a conflict not resolved by the Audit:

1. stop the affected operation;
2. leave the active Campaign Version unchanged;
3. record the conflict and impacted dependency closure;
4. consult authority, specialist rules, sources, and affected participants;
5. return the resolution to the Audit Report and Merge Plan;
6. continue only after authorization and revalidation of the plan.

Unrelated validated operations may continue only when their independence is demonstrated and the candidate target remains recoverable.

## Stage 4 - Validation

### Rule

Validate the complete candidate Campaign Version before activation.

At minimum verify:

- Relationships;
- Timeline;
- references;
- Projects;
- Research;
- Species;
- Locations;
- companions;
- Knowledge separation;
- no dangling references.

Also verify:

- authority hierarchy;
- Repository, Campaign, and Persistence Model version compatibility;
- stable Soul, Incarnation, actor, event, faction, settlement, item, Project, Research, and other material identities;
- world, Campaign, Session, Personal, and Soul chronology consistency;
- Campaign History and correction provenance;
- numerical-change traces;
- Inventory custody and location;
- Secrets and audience boundaries;
- unknowns, disputes, warnings, and source-recovery markers;
- no unauthorized retcons;
- no unaffected record changed without an Audit trace;
- no migration artifacts entered Repository Canon.

Detailed validation semantics are owned by the later Persistence Validation task. This stage cannot be skipped while that later tooling remains unimplemented; use the strongest available manual and automated checks and record their scope.

### Validation Outcomes

| Outcome | Meaning |
| --- | --- |
| **Pass** | All blocking checks pass; warnings are recorded and accepted within authority |
| **Pass With Warnings** | No blocking defect remains, but bounded non-blocking risks require Review Points |
| **Fail** | A blocking defect, unresolved authority conflict, data loss, leak, invalid change, or broken dependency remains |
| **Inconclusive** | Required source, access, owner, or validation capability is unavailable |

Only Pass or an explicitly authorized Pass With Warnings may activate the target Campaign Version. Fail and Inconclusive remain candidate or recovery states.

## Migration Manifest

Every migration maintains one **Migration Manifest** containing at least:

- Migration ID;
- Repository Version;
- source Campaign Version;
- target Campaign Version;
- source and target Persistence Model Versions;
- source and target Storage Format Versions where applicable;
- Source;
- Backup;
- Audit Report;
- Merge Plan;
- Files Updated or storage-neutral records updated;
- Validation Results;
- Warnings;
- Conflicts;
- Notes;
- initiator, approver, operator, and validator;
- start, completion, and activation times;
- status;
- rollback or recovery reference;
- superseding migration where applicable.

`Files Updated` is a required human-readable field when files implement storage. Record-oriented systems may provide the equivalent stable record set while retaining the field's meaning.

The Manifest is append-preserving. Later corrections create a linked correction or superseding migration; they do not silently rewrite the original report.

## Migration ID

Every migration receives one unique, immutable Migration ID before Backup.

The identifier:

- is unique within the campaign;
- remains stable across all four stages;
- appears in Backup, Audit Report, Merge Plan, validation, target Campaign Version, and Migration History;
- is never reused after failure, cancellation, or rollback;
- does not encode authority or success merely through its format.

## Activation

After successful validation:

1. finalize the target Campaign Version;
2. link its parent and Migration ID;
3. record the active Repository and Persistence Model Versions;
4. mark the former active version Superseded rather than deleting it;
5. update the Save Index atomically or through the safest storage-equivalent operation;
6. verify the new active version can be loaded;
7. retain rollback and backup references;
8. communicate material conversion effects to authorized participants.

Activation changes which validated version is current. It does not change in-world time or create a Timeline Event unless the campaign itself contains an in-world process corresponding to the migration.

## Interrupted Migration

If interruption occurs before activation:

- the former active Campaign Version remains authoritative;
- the candidate target is marked Incomplete or Recovery Candidate;
- completed Backup and Audit artifacts remain read-only;
- applied candidate changes remain isolated from active state;
- the Manifest records the last completed operation and validation status;
- resumption begins by verifying source, backup, candidate, versions, and plan integrity;
- uncertain partial writes are not assumed successful.

Never resume from memory alone. Reload the Manifest, Audit Report, Merge Plan, and candidate validation state.

## Rollback and Recovery

Rollback changes the active pointer to a previously validated Campaign Version or restores from a verified Backup when no valid target can be activated.

Rollback:

- records a new recovery or migration event;
- never deletes the failed target or Manifest;
- preserves gameplay that occurred after activation as a conflict requiring continuity resolution rather than silently erasing it;
- verifies Repository and model compatibility;
- revalidates references and Secrets;
- records data loss, unavailable source, and warnings honestly.

Restoring bytes is not enough. The restored campaign must still pass authority, continuity, and semantic validation.

## Repository Upgrade Migration

When adopting a new Repository Version:

1. identify changed canonical decisions and playable rules;
2. audit only campaign claims materially affected by those changes;
3. classify each as already compatible, mechanically convertible, requiring adjudication, requiring authorized retcon, incompatible, or unknown;
4. preserve established fictional consequences where the new rule does not require changing them;
5. update future adjudication and campaign records through explicit conversion decisions;
6. record any retained legacy ruling and its scope;
7. validate against both the new Repository Version and campaign continuity.

New Repository Canon has higher authority for future rule interpretation, but it does not silently rewrite established Campaign Canon. Where conflict cannot coexist, resolve it explicitly through authority and continuity procedures.

## Storage Migration

Moving between Markdown, database, Google Drive, Git, local files, or another implementation must preserve:

- logical module ownership;
- stable identities;
- typed references;
- truth layers and visibility;
- Persistence Levels;
- versions and ancestry;
- chronology and provenance;
- migration history;
- validation meaning;
- backup and recovery access.

Storage-specific conveniences, folder names, row order, hyperlinks, filenames, or timestamps do not become Campaign Canon.

## Worked Examples

### Importing a Previous Chat

A long chat contains play, out-of-character planning, mistaken summaries, rules discussion, and established events.

Backup preserves the export unchanged. Audit classifies events, Meta, Character Knowledge, Research, Relationships, chronology, and inconsistencies. Merge creates only supported records, marks unresolved claims Requires Source Recovery, and leaves player brainstorming in Meta. Validation checks identity, references, timeline, knowledge boundaries, and numerical traces before activation.

### Adopting a New Reincarnation Rule

A repository revision clarifies that former social office never transfers automatically to a new incarnation. An older campaign treated a reincarnated ruler as legally restored.

Audit distinguishes the rule conflict from the campaign's established coronation and political consequences. Campaign authority may preserve the coronation as an in-world institutional choice while removing automatic Soul-based entitlement from future adjudication. Migration records that conversion without erasing the reign.

### Two NPCs With the Same Name

An import appears to contain two records named Sera. The Audit compares locations, meetings, Personal Chronologies, Relationships, and sources and finds they are distinct people.

Merge retains two IDs and adds disambiguating aliases. It does not merge them because names match. Validation checks that promises, Knowledge, and encounters reference the correct person.

### Interrupted Database Conversion

A storage migration stops after writing half of the candidate records.

The active prior Campaign Version remains authoritative. The target is marked Incomplete. Resumption checks the Backup, Manifest, completed operations, and candidate integrity, then either restarts the Merge safely or creates a new Migration ID. Partial writes never become active by timestamp.

### Incomplete Historical Source

Only a story summary survives for several early sessions. It says a settlement was destroyed but gives no date, cause, or survivor list.

Audit preserves the destruction claim at its supported authority and marks details Unknown or Requires Source Recovery. Merge does not invent a date, casualty count, or culprit. Validation preserves open dependencies and warnings.

## Validation Checklist

Before activation confirm:

- [ ] a verified Backup exists and predates edits;
- [ ] the Migration ID is unique and present in every stage artifact;
- [ ] the Audit was read-only and source-complete within declared limits;
- [ ] every merged change traces to the Audit Report and Merge Plan;
- [ ] Repository, Campaign, Persistence Model, and Storage Format Versions are recorded as applicable;
- [ ] stable identities and aliases are preserved;
- [ ] truth layers, visibility, and Secrets are preserved;
- [ ] Timeline and Campaign History remain source-preserving;
- [ ] Relationship, Research, Species, Location, companion, Project, Inventory, Knowledge, and Soul references validate;
- [ ] numerical changes retain mechanical traces;
- [ ] unknowns and conflicts were not guessed away;
- [ ] authorized retcons include scope and consequences;
- [ ] no dangling or orphaned references remain;
- [ ] no unaffected record changed without trace;
- [ ] the target can be loaded and recovered;
- [ ] the Migration Manifest is complete;
- [ ] no populated campaign artifact entered the canonical repository.

## Safeguards

- No migration begins without a unique Migration ID.
- No source or target edit occurs before Backup and Audit complete.
- No backup is claimed without a recovery check.
- No merge change lacks an Audit trace.
- No unresolved conflict is guessed away.
- No target activates before validation.
- No failed or interrupted target replaces the active Campaign Version.
- No version label overrides authority or truth.
- No storage timestamp decides canon.
- No Repository upgrade silently rewrites Campaign History.
- No migration merges identities by name alone.
- No migration promotes Meta, Rumours, theories, or Secrets into Campaign Canon.
- No missing number defaults to zero or a balanced value.
- No storage migration changes semantic ownership.
- No rollback silently erases play that occurred after activation.
- No populated backup, Manifest, Audit Report, or campaign state belongs in this repository.

## Scope Boundaries

This document defines campaign version identities, compatibility, the four-stage migration transaction, Migration Manifests, activation, interruption, rollback, repository upgrades, and storage conversion. It does not define storage syntax, populated records, ordinary Save Updates, the full Continuity Resolution procedure, validation algorithms, repository release versioning, or migration templates.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Persistence Authority](PERSISTENCE_AUTHORITY.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Truth Layers](TRUTH_LAYERS.md)
- [Persistence Levels](PERSISTENCE_LEVELS.md)
- [Campaign State Model](CAMPAIGN_STATE_MODEL.md)
- [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md)
- [Research Engine](RESEARCH_ENGINE.md)
- [Timeline Engine](TIMELINE_ENGINE.md)
- [Alpha Playtest Rules](../gm/ALPHA_PLAYTEST_RULES.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
