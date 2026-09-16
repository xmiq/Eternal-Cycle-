# Campaign Bootstrap

## Purpose

This procedure defines how a human or AI GM starts a new Eternal Cycle campaign or resumes an existing one without confusing engine lifecycle, Campaign Mode, starting-life profile, and persistence configuration.

It creates no campaign inside this repository. Populated configuration, Campaign Canon, characters, world state, and saves remain external.

## Document Control

- **Owner:** new-versus-resume classification, engine-status lookup, Campaign Mode default, starting-profile separation, persistence-strategy selection, and bootstrap handoff
- **Primary authorities:** current root [README](../../README.md), [Version](../../VERSION), [Roadmap](../../design/ROADMAP.md), [Provisional Rulings](PROVISIONAL_RULINGS.md), and [Campaign Persistence Integration](../persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md)
- **Dependencies:** user intent, current Repository Version, external campaign authority, Campaign Configuration, Save Index, and persistence capability
- **Extensions:** interfaces may collect character, world, consent, accessibility, or presentation choices without changing these lifecycle rules
- **Consumers:** human GMs, AI Session Start, AI Execution Profiles, campaign creation interfaces, migration, and validation
- **Repository boundary:** no Campaign ID, participant identity, character, premise, locator, credential, or populated save belongs here

## Core Invariant

Eternal Cycle is released. Unless an authorized participant explicitly requests a supported testing mode, a newly created campaign is a normal campaign under the current Canonical rules.

```text
Engine Status: Released
Campaign Mode: NORMAL
```

Historical Alpha documents or labels do not alter this default.

## Lifecycle Authority

Determine **Engine Status** from current sources in this order:

1. canonical `VERSION` and release metadata;
2. the root README's current status;
3. the Roadmap's current repository status and phase;
4. current release-neutral operating procedures.

Historical audits, changelog entries, completed phase names, archived notes, and superseded filenames describe earlier states. They do not classify the current engine or a new campaign.

Engine Status and Campaign Mode are independent:

| Engine Status | Campaign Mode | Meaning |
| --- | --- | --- |
| Released | `NORMAL` | ordinary campaign using current Canon |
| Released | `VALIDATION` | explicitly selected playtest, regression, or validation campaign |
| Released | `DEVELOPMENT` | explicitly authorized experimental campaign |

## Classify the Request

Before creating or loading state, classify the request as one of:

- **New campaign** - no continuing Campaign ID or Canonical Campaign State is selected;
- **Resume campaign** - an existing campaign identity and canonical persistence authority are selected;
- **Migration or recovery** - existing continuity must be moved, repaired, or reconciled before play;
- **Development request** - the user is discussing rules or tooling rather than beginning Gameplay Context.

Do not create a replacement campaign because a local cache or assumed path is missing. Resolve Campaign Configuration and the Save Index first when existing continuity may exist.

## New Campaign Procedure

When the user asks to start a new game without an explicit testing request:

1. read current release/version and Roadmap status;
2. set Campaign Mode to `NORMAL`;
3. confirm only the setup choices needed to create valid external campaign records;
4. select a starting-life profile independently from Campaign Mode;
5. enumerate available compliant persistence capabilities, select `DIRECT` or `MANAGED`, persist the selection, and bind its canonical authority under [Enumerate -> Select -> Persist -> Reuse](../persistence/PERSISTENCE_STRATEGY_SELECTION.md);
6. create the external Campaign ID, Campaign Canon and Rules Profile, Save Index, required owner records, and initial Timeline boundary;
7. validate the canonical persistence target and blank-to-initial-state transaction;
8. enter play only after the initial canonical save is committed and validated;
9. present ordinary Eternal Cycle setup or gameplay without Alpha or playtest wording.

The GM may ask clarifying questions when a required premise, persistence choice, or player-controlled decision is missing. It must not fill missing campaign facts by genre assumption.

## Explicit Testing Modes

Select `VALIDATION` only when the user or authorized campaign authority explicitly requests a playtest, regression, validation, or test campaign.

Select `DEVELOPMENT` only when the request explicitly authorizes an experimental campaign or bounded non-canonical implementation.

Record the selected mode, authority, purpose, scope, and stop conditions in external Campaign Canon. Campaign Configuration and the Save Index may reference that record but do not own a duplicate current value. Testing mode cannot be inferred from:

- use of a Provisional Rule;
- a request to test an in-world character action;
- historical Alpha wording;
- the presence of validation tools;
- First-Life Mode;
- an AI runtime's uncertainty;
- a campaign created before release without an explicit testing purpose.

## Starting-Life Profiles

Starting-life profiles describe fictional and Soul-continuity setup. They do not classify campaign lifecycle.

**First-Life Mode** remains available under `NORMAL`, `VALIDATION`, or `DEVELOPMENT` as permitted by campaign authority. The same separation applies to a prior-life, monster-life, or mature-Soul starting profile.

The campaign's Reincarnation Mode remains owned by [Reincarnation](../soul/REINCARNATION.md) and is not Campaign Mode.

## Provisional Rulings During Normal Play

A `NORMAL` campaign may encounter one narrow undefined mechanic. Follow [Provisional Rulings](PROVISIONAL_RULINGS.md), persist the ruling in the external Rules Profile, and keep Campaign Mode `NORMAL`.

If the missing area requires a broad new system, narrow or stop. Do not change to `DEVELOPMENT` silently as a way to continue.

## Resume Procedure

When resuming an existing campaign:

1. resolve its stable identity and canonical persistence target;
2. load and reuse its persisted Persistence Strategy before considering newly available adapters or services;
3. read its Campaign Mode from authoritative campaign metadata when present;
4. preserve an explicitly selected testing mode;
5. treat pre-release Alpha wording as historical when it merely described the engine lifecycle;
6. classify ambiguous old metadata as `Unknown` or `Requires Source Recovery` rather than guessing;
7. follow [AI Session Start](../ai/AI_SESSION_START.md) or the equivalent human-GM load procedure;
8. never reclassify continuity from conversation wording alone.

An existing `NORMAL` campaign resumes as `NORMAL` unless authorized campaign governance changes it.

## Campaign Mode Changes

Campaign Mode changes require an explicit campaign-authority decision and a persisted metadata update. The change records:

- previous and new mode;
- authorizing participant or process;
- effective time;
- reason and scope;
- any validation or development objective;
- effects on Provisional Rules and feedback handling.

Mode changes do not rewrite prior Timeline events or engine history.

## Compatibility With Legacy Metadata

When migrating older setup records:

- preserve explicit playtest, regression, or experimental intent;
- map a generic pre-release lifecycle label to historical metadata, not automatically to current Campaign Mode;
- preserve First-Life Mode and other genuine setup choices;
- retain source wording and migration provenance where needed for audit;
- avoid a schema migration when an external Campaign Canon metadata field is sufficient;
- validate that no campaign was silently reclassified.

## Canonical Examples

### Normal new campaign

User request: `Start a new Eternal Cycle game.`

Result: engine `Released`, Campaign Mode `NORMAL`, ordinary setup, configured persistence, initial validated save.

### Explicit validation campaign

User request: `Start a validation campaign for Reincarnation handoff.`

Result: engine `Released`, Campaign Mode `VALIDATION`, explicit test objective and stop conditions.

### Provisional ruling in normal play

A `NORMAL` campaign encounters one undefined interaction. The GM records a narrow Provisional Rule. Campaign Mode remains `NORMAL`.

### Historical audit

The runtime discovers an archived Alpha-readiness audit. Current `VERSION`, README, and Roadmap still establish engine `Released`; the audit does not classify the campaign.

### First-Life setup

User selects First-Life Mode without requesting a test. Campaign Mode remains `NORMAL`.

### Existing campaign resume

An existing `NORMAL` campaign is loaded by a new AI runtime. The runtime reads authoritative campaign metadata and does not relabel it from historical repository language.

## Safeguards

- `NORMAL` is the only implicit Campaign Mode.
- Testing and development modes require explicit authority.
- Engine Status never substitutes for Campaign Mode.
- Campaign Mode never substitutes for Reincarnation Mode or a starting-life profile.
- Provisional Rulings do not change Campaign Mode.
- Persistence validation precedes successful new-campaign completion.
- Existing continuity is loaded or migrated, never replaced by an assumed blank save.
- Persistence Strategy is sticky after selection; unavailable authority creates recovery or migration work, not silent fallback.
- Historical records remain historically accurate.
- No populated campaign record enters this repository.

## Related Documents

- [Provisional Rulings](PROVISIONAL_RULINGS.md)
- [Game Master Framework](GAME_MASTER_FRAMEWORK.md)
- [AI Session Start](../ai/AI_SESSION_START.md)
- [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md)
- [ChatGPT GM Universal Instructions](../ai/chatgpt/CHATGPT_GM_UNIVERSAL_INSTRUCTIONS.md)
- [Campaign Canon and Rules Profile Template](../../templates/CAMPAIGN_CANON_TEMPLATE.md)
- [Save Index Template](../../templates/SAVE_INDEX_TEMPLATE.md)
- [Persistence Configuration Template](../../templates/PERSISTENCE_CONFIGURATION_TEMPLATE.md)
- [Migration and Versioning](../persistence/MIGRATION_AND_VERSIONING.md)
- [Persistence Strategy Selection](../persistence/PERSISTENCE_STRATEGY_SELECTION.md)
- [Continuity Resolution](../persistence/CONTINUITY_RESOLUTION.md)
