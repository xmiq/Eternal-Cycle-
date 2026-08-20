# SQLite Persistence Adapter

## Purpose

This Persistence Adapter defines how a ChatGPT runtime uses SQLite as a structured logical campaign store while preserving the storage-neutral [Campaign Persistence Engine](../../../persistence/README.md).

It contains no schema for a particular campaign, no populated state, and no deployment locator. SQLite implements storage behavior; it does not adjudicate gameplay or own campaign meaning.

## Document Control

- **Owner:** this adapter owns SQLite open, transaction, integrity, expected-versus-actual, read-only reopen, staleness, rollback, and candidate-production behavior
- **Primary authorities:** [AI Runtime Model](../../AI_RUNTIME_MODEL.md), [AI Save Protocol](../../AI_SAVE_PROTOCOL.md), [Save Update Protocol](../../../persistence/SAVE_UPDATE_PROTOCOL.md), and [Persistence Validation](../../../persistence/PERSISTENCE_VALIDATION.md)
- **Dependencies:** external Campaign Configuration, one identified logical SQLite artifact, active parent Campaign Version, complete Affected Set, and authorized file access
- **Extensions:** deployment adapters may fetch and publish validated SQLite bytes without taking over database integrity
- **Consumers:** ChatGPT execution profile, Adapter Chains, campaign custodians, save operators, and recovery tools
- **Repository boundary:** no campaign identifier, database file, path, schema, credential, current value, or populated record belongs here

## Adapter Role

The SQLite adapter owns:

- opening the identified database safely;
- enforcing foreign keys;
- applying one bounded transaction;
- protecting the active parent from stale writes;
- checking database and referential integrity;
- comparing expected and actual state;
- producing validated candidate bytes or an explicit failure.

It does not own:

- gameplay adjudication;
- Campaign Persistence meaning or authority;
- remote file identity;
- remote upload or replacement;
- remote backup propagation;
- player-facing narration.

## Canonical SQLite Artifact

Campaign Configuration identifies exactly one logical canonical SQLite artifact for the active campaign deployment.

When another adapter provides remote storage, that outer adapter identifies and fetches the canonical remote artifact. The SQLite adapter receives the latest bytes and produces a validated candidate. A Local Working Copy is temporary and has no canonical authority until the complete Adapter Chain writes, activates, and verifies it.

The runtime must not select authority by filename similarity, local modification time, cache recency, or file size.

## Fetch or Open Latest

Before dependent gameplay or a write:

1. ask the configured outer deployment adapter for the latest identified canonical bytes, or open the configured local canonical artifact when SQLite is the outermost adapter;
2. verify expected campaign and storage identity through authorized metadata;
3. read the active Save Index, Campaign Version, Transaction state, and validation status;
4. reject an obsolete Local Working Copy;
5. preserve the source bytes or other recovery basis required by Campaign Configuration.

Conversation attachments and earlier downloads are caches until freshness is re-established.

Absence at an assumed local path is not evidence that no campaign database exists. When an outer adapter is configured, invoke its exact-identity fetch procedure before declaring the target unavailable. Never initialize a blank database as fallback while remote canonical lookup is unresolved.

## Pre-Action Read

Load the campaign records required by the action's canonical Read Set, including as relevant:

- Save Index, Current Session, versions, and open recovery;
- player, Soul, Incarnation, embodiment, Development, Skills, and Magic;
- species, Evolution, Classes, Soul Weapons, and Inventory;
- Relationships, actors, factions, locations, and infrastructure;
- Research, Projects, Mysteries, Knowledge, Secrets, and Timeline;
- world conditions, Pending Consequences, Review Points, and validation warnings.

Do not reconstruct a record from conversation history when the authoritative SQLite record exists.

## Complete Affected Set

Before writing, calculate the complete dependency-closed Affected Set.

Every planned operation identifies:

- Authoritative Record Owner;
- prior value or explicit unknown;
- mechanically established result;
- source event and time;
- Truth Layer and Persistence Level;
- required linked operations;
- expected post-transaction state.

Mutually dependent updates belong in one bounded transaction. Do not save current state, history, Relationships, chronology, costs, or progression piecemeal when partial success would create inconsistent continuity.

## Safe Write Procedure

1. obtain and preserve the latest canonical SQLite bytes;
2. create a fresh Local Working Copy;
3. open it with write access limited to the candidate;
4. enable foreign-key enforcement;
5. confirm the expected parent Campaign Version and transaction preconditions;
6. begin an appropriate bounded SQLite transaction;
7. apply the complete owner-routed Write Set;
8. run pre-commit structural and expected-state checks;
9. commit the database transaction;
10. close the write connection cleanly;
11. reopen the candidate read-only;
12. run required database, semantic, and expected-versus-actual validation;
13. close the read-only connection;
14. pass only the validated candidate bytes and evidence to the next adapter.

If any step before commit fails, roll back. If any post-commit candidate check fails, discard the candidate and preserve the prior canonical source.

## Required SQLite Validation

At minimum, the adapter verifies:

- `PRAGMA integrity_check` returns `ok`;
- `PRAGMA foreign_key_check` returns no violations;
- the expected campaign, Save Index, parent, and candidate version records exist;
- every expected changed record contains the established value;
- required linked records and references exist;
- current and superseded states remain coherent;
- no duplicate current authority was created;
- no unrelated authoritative record was removed or overwritten;
- the candidate can be reopened read-only.

A passing SQLite integrity check does not replace Campaign Persistence semantic validation.

## Expected-Versus-Actual Validation

Retain an immutable expected Affected Set before writing. After reopening read-only, compare actual state against that set.

The candidate passes only when:

- every expected create, update, append, close, or supersession exists;
- every required link and chronology append exists;
- numerical changes match their mechanical traces;
- no operation escaped the Affected Set;
- no unexpected destructive change occurred;
- current scene and campaign continuity remain coherent to the declared scope.

File size, row count, or successful commit alone is insufficient.

If the Affected Set is non-empty and all expected authoritative values, version evidence, chronology, and canonical bytes remain unchanged where change is required, validation fails. The adapter must return failure evidence rather than `SAVE_COMPLETE`.

## Read-Only Reopen

For FR-011, read-only reopen verifies critical expected owner changes and the resulting Campaign Version or Save Point before turn closure. Context Caches are refreshed only from this verified state. The adapter does not decide relevance or adjudication.

Post-write validation uses a newly opened read-only connection against the closed candidate, not the write connection's in-memory view.

The read-only reopen must not:

- repair records;
- run migrations;
- update validation timestamps inside campaign state;
- normalize values;
- create missing rows;
- change journal or campaign meaning.

A repair creates a new candidate through the proper owner and requires a fresh validation run.

## Staleness and Concurrency

Before publishing the candidate, compare the currently authoritative revision, Campaign Version, modified identity, hash, or configured concurrency token with the parent originally loaded.

If canonical state changed during adjudication:

- do not overwrite;
- retain the Transaction ID and candidate as non-authoritative evidence;
- fetch the new parent;
- classify the branch under the Save Update Protocol;
- reconcile, rerun, or reject the transaction;
- validate a proper descendant.

Last-writer-wins is forbidden.

## Failure and Rollback

If open, transaction, commit, integrity, semantic, expected-versus-actual, or read-only validation fails:

- roll back where the transaction remains open;
- discard or quarantine the invalid candidate;
- do not replace canonical state;
- preserve the previous validated source;
- do not report gameplay as durably completed;
- report the exact failed boundary without inventing a recovery result.

If downstream deployment or read-back fails, the SQLite candidate remains a validated candidate but not an activated canonical Save Point.

For a local-authoritative campaign, complete SQLite validation and read-only reopen may support marker `💾`. For a cloud-authoritative chain, the same candidate supports only `⏳` until the outer adapter verifies canonical synchronization; downstream failure supports `⚠️`. The SQLite adapter never emits `☁️💾`.

## Adapter Composition

A common Adapter Chain is:

```text
SQLite logical store
  -> Google Drive canonical deployment
  -> Google Drive backup snapshot
```

In that chain:

- SQLite owns database transactions, foreign-key enforcement, database integrity, and expected-versus-actual validation;
- Google Drive owns exact remote identity, fetch-latest, canonical replacement, remote read-back, backup propagation, and backup read-back;
- Campaign Configuration identifies the chain and required policies;
- the Campaign Persistence Engine owns logical authority and semantic requirements.

Neither adapter may treat the other's successful operation as proof that its own responsibility passed.

### Living Codex Specialization

The same SQLite transaction boundary may carry a separately configured [GM Living Codex database](../../../gm-living-codex/PERSISTENCE_MODEL.md). In that chain, the adapter opens exactly one identified Codex artifact, enforces Codex foreign keys and constraints, applies the complete reusable-design affected set, validates the candidate read-only, and passes only validated bytes onward.

The Living Codex database is not a campaign database. Codex Versions, revisions, migrations, validation runs, and stable species identities use their Codex owners rather than the Campaign Persistence Engine's Save Index, Campaign Version, Truth Layers, or campaign record owners. An implementation must choose the correct specialization explicitly and must never mix the two schemas or infer that a Codex entry is campaign truth.

## Schema Boundary

Schema design, inspection, migration, and repair occur only in Development Context.

Gameplay Context does not expose tables, columns, indexes, migration statements, query plans, file paths, or implementation diagnostics. Schema presence never grants a Skill, capability, item, relationship, fact, or progression outcome.

## Safeguards

- Exactly one logical SQLite artifact is selected by Campaign Configuration.
- A Local Working Copy has no authority.
- Foreign-key enforcement is mandatory during candidate writes.
- Related changes use one bounded transaction.
- Validation includes integrity, foreign keys, semantics, and expected-versus-actual comparison.
- Post-write checks reopen the candidate read-only.
- Stale writes and last-writer-wins are rejected.
- Failure preserves the prior canonical source.
- SQLite never adjudicates gameplay.
- No campaign-specific schema, identifier, path, or fact belongs here.

## Related Documents

- [ChatGPT GM Universal Instructions](../CHATGPT_GM_UNIVERSAL_INSTRUCTIONS.md)
- [Google Drive Persistence Adapter](GOOGLE_DRIVE_PERSISTENCE_ADAPTER.md)
- [GM Living Codex Persistence Model](../../../gm-living-codex/PERSISTENCE_MODEL.md)
- [AI Runtime Model](../../AI_RUNTIME_MODEL.md)
- [AI Capabilities and Limitations](../../AI_CAPABILITIES_AND_LIMITATIONS.md)
- [AI Save Protocol](../../AI_SAVE_PROTOCOL.md)
- [Campaign State Model](../../../persistence/CAMPAIGN_STATE_MODEL.md)
- [Save Update Protocol](../../../persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../../../persistence/PERSISTENCE_VALIDATION.md)
- [Canonical Terminology](../../../../design/TERMINOLOGY.md)
