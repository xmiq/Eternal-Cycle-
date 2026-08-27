# Provisional Rulings

## Purpose

This document defines how an Eternal Cycle campaign handles a narrow mechanic that current Canon does not resolve. It preserves play without allowing an improvised campaign ruling to become reusable Canon merely because it was useful.

Eternal Cycle is released software. A campaign created under the released rules is a normal Eternal Cycle campaign unless an authorized participant explicitly selects a testing mode. Using this procedure does not change that Campaign Mode.

## Document Control

- **Owner:** rule-status classification, narrow Provisional Rulings, campaign-local recording, review, conversion, and testing-mode separation
- **Primary authorities:** [Roadmap](../../design/ROADMAP.md), [Design Decisions](../../design/DECISIONS.md), [Canonical Terminology](../../design/TERMINOLOGY.md), and [Campaign Bootstrap](CAMPAIGN_BOOTSTRAP.md)
- **Dependencies:** current Repository Canon, relevant Owning Systems, external Campaign Record, Campaign Mode, and authorized campaign governance
- **Extensions:** explicit validation or development campaigns may add test objectives and feedback records without changing this procedure
- **Consumers:** human and AI GMs, campaign bootstrap, adjudication, continuity resolution, migration, and Future Revision intake
- **Repository boundary:** no populated ruling, campaign fact, participant record, playtest result, current character, save, or live world state belongs here

## Core Rules

1. Canonical rules govern normally.
2. Canonical Foundations constrain a genuinely missing implementation.
3. A Provisional Rule may fill only the smallest playable gap.
4. Unsupported areas require narrowing, deferral, or an explicitly authorized experiment.
5. A Provisional Rule never changes Campaign Mode by itself.
6. Repetition, duration, preference, or success never promotes a ruling into Repository Canon.

The repository's current release metadata and Roadmap determine engine lifecycle status. Historical Alpha filenames, archived audits, old phase names, and pre-release descriptions do not classify a current campaign.

## Engine Status and Campaign Mode

**Engine Status** describes the published Eternal Cycle rules repository. Read the current `VERSION`, root README, release metadata, and Roadmap rather than inferring status from historical documents.

**Campaign Mode** is explicit campaign-control metadata owned by external Campaign Canon. Campaign Configuration and the Save Index may reference it but do not own a duplicate current value. Campaign Mode does not change rules authority, fictional physics, or persistence ownership.

Canonical Campaign Modes are:

- **`NORMAL`** - the default for a new campaign using released Eternal Cycle rules;
- **`VALIDATION`** - an explicitly requested playtest, regression, or validation campaign;
- **`DEVELOPMENT`** - an explicitly authorized experimental campaign that may test bounded non-canonical work.

The words *playtest* and *regression campaign* select `VALIDATION` when used as an explicit campaign request. The word *experimental* selects `DEVELOPMENT` only when the user or campaign authority clearly requests that mode. Tool availability, provisional adjudication, a historical audit, or an old metadata label never selects either testing mode.

A released engine may run a `VALIDATION` campaign. A `NORMAL` campaign may use a Provisional Rule. These facts do not conflict.

## Rule Status Model

### Canonical

A **Canonical** rule is complete and authoritative within its stated scope.

Canonical rules:

- apply normally;
- take precedence over Provisional Rules;
- retain their stated owners, limits, safeguards, and exceptions;
- may not be silently overridden during play;
- change only through repository governance.

Ordinary GM judgment may interpret a Canonical rule where the rule calls for judgment. Interpretation cannot reverse the rule or invent an exception to force a preferred outcome.

### Canonical Foundation

A **Canonical Foundation** is an authoritative set of principles and boundaries whose detailed implementation may not cover the immediate narrow claim.

The Foundation remains Canonical. Missing procedures, formulas, branches, content, or interfaces do not. A Provisional Rule constrained by a Foundation must preserve its ownership, embodiment, cost, persistence, agency, compatibility, safeguards, uncertainty, and consequence boundaries.

### Provisional

A **Provisional Rule** is a temporary, campaign-local ruling for one narrow gap.

A Provisional Rule:

- respects every applicable Canonical rule and Foundation;
- has an identified Owning System;
- states its exact scope and affected claims;
- is recorded in the external Campaign Record;
- states its review, expiry, or supersession condition;
- may be revised, withdrawn, or replaced through campaign authority;
- does not enter the rules repository as campaign state;
- does not change a `NORMAL` campaign into `VALIDATION` or `DEVELOPMENT`.

### Unsupported

An **Unsupported** area lacks enough Canonical rules or Foundations for reliable adjudication without inventing a major system.

Ordinary campaign play should not center on an Unsupported area. Narrow, delay, or discuss the situation. A `DEVELOPMENT` campaign may test an explicitly authorized external implementation, but that implementation remains non-canonical until separately reviewed and accepted.

Unusual is not the same as Unsupported. If established rules already define ownership, access, costs, limits, and safeguards, the GM may be able to resolve the claim canonically or through one narrow Provisional Rule.

## Provisional Ruling Procedure

When play reaches a missing or unclear mechanic:

1. **Identify the issue.** State the exact action, capability, consequence, or procedure that lacks a clear rule.
2. **Check current Canon.** Consult the current Repository Version, relevant documents, terminology, decisions, and Roadmap status.
3. **Determine ownership.** Identify the narrowest Owning System for the claimed effect.
4. **Identify boundaries.** Record applicable access, embodiment, cost, persistence, agency, compatibility, consequence, information, and safeguard constraints.
5. **Classify support.** Decide whether Canon resolves the issue, a Provisional Rule can fill the narrow gap, or the area is Unsupported.
6. **Make the narrowest ruling.** Add only what the immediate claim requires. Do not build an unseen general system.
7. **Record status and scope.** Persist the ruling, owner, affected participants, conditions, and explicit Provisional status in the external Campaign Record.
8. **Set review or expiry.** Identify the evidence, Canon update, repeated pressure, session boundary, or Review Point that triggers reconsideration.
9. **Resolve play.** Apply the ruling without changing Campaign Mode or representing it as Repository Canon.
10. **Route reusable evidence separately.** Send qualifying observations through owner-mediated Future Revision governance without copying live campaign state into the repository.

A useful ruling record contains:

- stable Ruling ID;
- missing narrow claim;
- Owning System and Canon consulted;
- temporary ruling;
- scope and affected records;
- dependencies and safeguards;
- effective interval;
- review, expiry, or supersession condition;
- observed outcome and any separately classified Design Feedback.

## Prohibited Provisional Outcomes

A Provisional Rule must never:

- contradict a Canonical rule or accepted decision;
- create a universal character level, combat rating, power score, or progression currency;
- treat retained potential as present access, expression, or reliability;
- bypass body, species, environment, materials, authority, or other embodiment requirements;
- trivialize Final Death or erase worldly consequences;
- replace player or current-incarnation agency;
- merge human and monster progression without a valid crossover owner;
- grant automatic Skill mastery from memory, observation, labels, or prior ownership;
- make suffering, killing, repetition, time, injury, or death inherently profitable;
- convert an unknown fact into certainty merely to smooth narration;
- create another authoritative owner for mutable campaign state;
- move a populated ruling or feedback record into this repository.

## Applying Completed Systems

Completed rules apply directly within their stated scopes. A combined or unusual action is not Provisional merely because several systems interact.

When one claim depends on a genuinely missing effect:

1. apply every completed owner normally;
2. isolate only the missing effect;
3. rule only that missing effect provisionally;
4. preserve all cross-system costs, limits, uncertainty, and consequences;
5. refuse the claim when the missing portion is too broad to rule narrowly.

The procedure does not reopen completed phases or make old roadmap gaps current again.

## Campaign Bootstrap

When a user asks to start a new Eternal Cycle game without selecting a testing purpose:

```text
Engine Status: Released
Campaign Mode: NORMAL
```

The GM follows [Campaign Bootstrap](CAMPAIGN_BOOTSTRAP.md), selects persistence under current rules, creates the external Campaign Record, and begins ordinary setup. It does not mention Alpha or playtesting unless the user requested `VALIDATION` or `DEVELOPMENT`.

### First-Life Mode

**First-Life Mode** remains a valid starting-life profile. It begins with a relatively undeveloped Soul and emphasizes current embodiment, local Development, Relationships, consequences, and the base setting before substantial cross-life continuity exists.

First-Life Mode is independent of Campaign Mode. Valid combinations include:

```text
Campaign Mode: NORMAL
Starting profile: First-Life Mode
```

and, when explicitly requested:

```text
Campaign Mode: VALIDATION
Starting profile: First-Life Mode
```

First-Life Mode is not an Alpha label and does not imply testing.

### Other Starting Profiles

A campaign may instead begin with an established prior Life, a monster embodiment, or a mature Soul when current rules and campaign setup support it. These are setup choices, not engine lifecycle labels.

A bounded Reincarnation stress test is a `VALIDATION` objective only when explicitly requested as a test. Ordinary play may still include Final Death and Reincarnation without changing Campaign Mode.

## Explicit Testing Campaigns

Testing remains available by explicit selection.

A `VALIDATION` campaign should record:

- the tested rule or procedure;
- expected evidence;
- scope and stop conditions;
- permitted Provisional Rules;
- feedback and migration expectations.

A `DEVELOPMENT` campaign additionally identifies the authorized non-canonical material being tested and keeps it external to Repository Canon.

Testing mode does not weaken Canonical rules, campaign persistence, player agency, information boundaries, or repository scope. A test objective may focus pressure; it cannot script the result.

## Continuity When Canon Changes

When Canon replaces a Provisional mechanic:

1. preserve established fictional consequences where practical;
2. use the new Canonical mechanic for future resolution;
3. translate existing capability and progress conservatively into valid owners;
4. preserve legitimately earned progress without inventing access or duplicate benefits;
5. retire exploits, unsupported bypasses, and invalid capabilities;
6. discuss substantial retroactive effects with affected participants;
7. record the conversion in the external Campaign Record;
8. use [Migration and Versioning](../persistence/MIGRATION_AND_VERSIONING.md) when structured records or versions change.

Campaign Mode does not change automatically during conversion.

## Legacy Metadata Compatibility

Older campaign records may contain labels such as `Alpha`, `Alpha Mode`, or `Playtest Campaign` because the engine itself was pre-release.

Interpret them as follows:

- an explicit testing objective, validation scope, regression plan, or experimental authority remains an explicit testing campaign;
- a label that only mirrored the engine's former pre-release lifecycle is historical metadata and does not override current Campaign Mode;
- ambiguous metadata remains `Unknown` or `Requires Source Recovery` until campaign authority resolves it;
- migration records the interpretation and provenance rather than silently rewriting history;
- no existing campaign is automatically reclassified merely to remove old wording.

## Feedback Procedure

After any session that used a Provisional Rule, review:

1. Which Canonical rules and Foundations applied?
2. What exact gap required the ruling?
3. Did the ruling preserve ownership, embodiment, costs, agency, uncertainty, and consequence?
4. Did it create an exploit, duplicate benefit, or hidden major system?
5. Should it expire, continue narrowly, or be replaced?
6. Does the evidence qualify for owner review under [Future Revisions](../../design/FUTURE_REVISIONS.md)?

Session-specific answers stay outside the repository. A repository proposal extracts only the reusable design issue.

## Safeguards

- `NORMAL` is the default Campaign Mode after release.
- Testing modes require explicit selection.
- A Provisional Rule does not select or change Campaign Mode.
- Current lifecycle metadata outranks historical Alpha terminology.
- First-Life Mode remains independent from campaign testing status.
- Missing information remains missing.
- Provisional adjudication never becomes a substitute for repository governance.
- Historical outcomes and current mutable state remain distinct.
- No campaign data enters this repository.

## Related Documents

- [Campaign Bootstrap](CAMPAIGN_BOOTSTRAP.md)
- [GM Rules Index](README.md)
- [Game Master Framework](GAME_MASTER_FRAMEWORK.md)
- [GM Principles](GM_PRINCIPLES.md)
- [Uncertainty Handling](UNCERTAINTY_HANDLING.md)
- [Campaign Canon and Rules Profile Template](../../templates/CAMPAIGN_CANON_TEMPLATE.md)
- [AI Session Start](../ai/AI_SESSION_START.md)
- [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md)
- [Continuity Resolution](../persistence/CONTINUITY_RESOLUTION.md)
- [Migration and Versioning](../persistence/MIGRATION_AND_VERSIONING.md)
- [Roadmap](../../design/ROADMAP.md)
- [Future Revisions](../../design/FUTURE_REVISIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
