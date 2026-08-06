# GM Living Codex Persistence Model

## Purpose

This document defines the authoritative persistence architecture for the [GM Living Codex](GM_LIVING_CODEX.md). It owns the separate Living Codex SQLite artifact, normalized logical schema, revision and migration evidence, validation requirements, and remote deployment contract.

The model stores reusable GM-approved design. It does not store Campaign State, player knowledge, current populations, individual characters, or one campaign's history. The [Campaign Persistence Engine](../persistence/README.md) remains the owner of campaign continuity.

## Document Control

- **Owner:** Living Codex storage structure, Codex migration evidence, Codex validation evidence, and full-save completion
- **Primary authorities:** [GM Living Codex Design](GM_LIVING_CODEX.md), [Species Registry](SPECIES_REGISTRY.md), and [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- **Dependencies:** SQLite transaction semantics, configured remote deployment, exact canonical identity, and authorized Codex governance
- **Extensions:** later Living Codex modules may add normalized tables and validation rules without changing this authority boundary
- **Consumers:** GM operators, AI execution profiles, Codex editors, persistence adapters, validation tools, and renderers
- **Repository boundary:** this repository contains the design and blank contracts only; populated databases, backups, manifests, locators, and credentials remain external

## Separate Canonical Store

The Living Codex uses one logical SQLite database separate from every campaign database.

Preferred filename:

```text
eternal_cycle_living_codex.sqlite
```

The configured canonical instance is the authoritative populated Living Codex database. Its filename alone does not confer authority. Exact deployment identity, Codex Version, validation status, and configuration establish which instance is canonical.

Rendered Markdown, generated species pages, search indexes, exports, and summaries are secondary views. They may improve discovery but cannot silently modify or outrank the database.

## Authority Boundary

The persistence model carries this design-authority order:

```text
Eternal Cycle Rules Repository
  -> validated GM Living Codex database
  -> Campaign Configuration
  -> Campaign Canon
```

The rules repository constrains every entry. The database carries accepted reusable design. Campaign Configuration selects a Codex version and any explicit campaign divergences. Campaign Canon records what is true in one campaign.

The Codex database is not another Truth Layer or Persistence Level inside a campaign. Campaign Migration IDs, Campaign Versions, Save Transactions, and Campaign Validation Runs remain distinct from Codex migrations, Codex Versions, Codex transactions, and Codex validation runs.

## Storage Responsibilities

### SQLite

SQLite owns:

- normalized reusable-design records;
- stable identifiers and foreign keys;
- bounded database transactions;
- structural constraints;
- revision, deprecation, migration, and validation records;
- database integrity and read-only reopen checks.

### Remote Deployment

The configured remote adapter owns:

- exact canonical file identity;
- fetch-latest behavior;
- canonical replacement;
- remote concurrency protection;
- canonical read-back;
- current and dated backup deployment;
- backup read-back and exact comparison.

### GM Authority

The GM owns the design decision to accept, revise, merge, split, deprecate, replace, or reject an entry. Storage success cannot make an unapproved draft canonical, and GM approval cannot substitute for a failed full save.

## Logical Schema

An implementation may refine column names, use integer surrogate keys, or add supporting tables and indexes. It must preserve stable public identifiers, typed relations, normalized many-to-many data, revision history, and the ownership described here.

### Entry and Species Tables

| Table | Required purpose |
|---|---|
| `codex_entries` | Stable identity, entry kind, lifecycle state, current revision, and common governance metadata. |
| `species` | Species-specific core fields keyed to one Codex entry and one immutable Codex Stable ID. |
| `species_aliases` | Preferred, former, translated, colloquial, and disambiguating names with scope and revision provenance. |
| `species_classifications` | Typed, revisioned classification assignments without making taxonomy part of identity. |
| `species_traits` | Typed links to reusable Innate, passive biological, sensory, magical, or Soul-related traits. |
| `habitats` | Reusable habitat definitions used by species and ecological relations. |
| `ecological_relationships` | Directional ecological roles such as predation, prey, competition, facilitation, and symbiosis. |

### Skill and Evolution Tables

| Table | Required purpose |
|---|---|
| `skills` | Stable references to characteristic Skill designs or repository-defined Skill identities used by Codex records. |
| `species_skills` | Typed species-to-Skill relations for Instinctive, Typical Learned, Cultural, Rare Species, and other approved routes. |
| `evolution_nodes` | Stable forms participating in reusable Evolution graphs. |
| `evolution_edges` | Directional Evolution Routes with provenance, role, restrictions, and lifecycle state. |
| `evolution_requirements` | Normalized environmental, behavioral, Development, magical, Soul, and other route requirements. |
| `evolution_traits` | Traits inherited, gained, altered, or lost through an Evolution Route. |
| `evolution_skills` | Skill routes granted, altered, restricted, or made available by an Evolved Form. |

### Variant and Generation Tables

| Table | Required purpose |
|---|---|
| `variants` | Stable reusable variants linked to a base entry and carrying an explicit distinct-species assessment. |
| `variant_differences` | Normalized, typed differences in anatomy, ecology, traits, Skills, Magic, and Evolution access. |
| `procedural_tags` | Controlled tag definitions for generation and discovery. |
| `species_tags` | Many-to-many species-to-tag assignments with relevance and provenance. |

### Governance Tables

| Table | Required purpose |
|---|---|
| `entry_revisions` | Immutable revision identity, parent revision, accepted change set, authority, rationale, and review time. |
| `deprecations` | Typed deprecation, replacement, merge, and split relations preserving retired identities. |
| `codex_migrations` | One record per attempted Codex migration, including parent and candidate versions, source, backup, affected set, result, and recovery state. |
| `codex_validation_runs` | Validation scope, profile, checks, findings, result, limitations, and evidence for a specific candidate or active version. |

Complex many-to-many values must use relation tables. Comma-separated identifiers, overloaded free-text lists, and name-only references are not valid substitutes.

## Representative Core Constraints

The physical schema must enforce semantics equivalent to:

```sql
PRAGMA foreign_keys = ON;

CREATE TABLE codex_entries (
    entry_id TEXT PRIMARY KEY,
    entry_kind TEXT NOT NULL,
    lifecycle_state TEXT NOT NULL,
    current_revision_id TEXT,
    created_at TEXT NOT NULL,
    last_reviewed_at TEXT NOT NULL
);

CREATE TABLE species (
    species_id TEXT PRIMARY KEY,
    entry_id TEXT NOT NULL UNIQUE,
    preferred_name TEXT NOT NULL,
    design_intent TEXT NOT NULL,
    FOREIGN KEY (entry_id) REFERENCES codex_entries(entry_id)
);

CREATE TABLE species_aliases (
    alias_id INTEGER PRIMARY KEY,
    species_id TEXT NOT NULL,
    alias_text TEXT NOT NULL,
    alias_kind TEXT NOT NULL,
    scope_note TEXT,
    revision_id TEXT NOT NULL,
    UNIQUE (species_id, alias_text, alias_kind, scope_note),
    FOREIGN KEY (species_id) REFERENCES species(species_id)
);

CREATE TABLE codex_migrations (
    migration_id TEXT PRIMARY KEY,
    parent_codex_version TEXT NOT NULL,
    candidate_codex_version TEXT NOT NULL,
    source_summary TEXT NOT NULL,
    backup_identifier TEXT NOT NULL,
    affected_set_summary TEXT NOT NULL,
    result TEXT NOT NULL,
    created_at TEXT NOT NULL
);
```

Implementations should add indexes, controlled-value tables, revision foreign keys, uniqueness rules, graph checks, and stricter field constraints appropriate to the complete schema. The example is a semantic floor, not a deployable complete migration.

## Stable Reference Rules

- Every species uses one immutable `EC-SPECIES-......` identity.
- Every relation uses stable IDs rather than mutable display names.
- Renaming creates alias and revision evidence; it does not update identity.
- A merge, split, deprecation, or replacement preserves every former ID through typed relations.
- An Evolution edge references stable source and destination nodes.
- A species-to-Skill relation identifies both the Skill and the relation type.
- A variant references its base entry and its own stable identity.
- No current row may point to an absent, retired without replacement, or ambiguous target unless the schema explicitly permits and labels that historical state.

## Codex Versions and Revisions

A **Codex Revision** records one accepted change to an entry or related design set. A **Codex Version** identifies a complete validated database state after one or more revisions are applied atomically.

Every accepted change records:

- immutable migration and revision identities;
- parent and candidate Codex Versions;
- source and design basis;
- GM approval authority;
- complete affected set;
- created, changed, retired, or replaced records;
- validation evidence;
- warnings and recovery status;
- timestamps without relying on timestamps as authority.

Revision history is append-preserving. Correcting current design creates a new revision; it does not erase the earlier accepted state.

## Full-Save Protocol

Every accepted Living Codex modification follows this complete sequence:

```text
fetch latest identified canonical Codex
  -> verify identity and active Codex Version
  -> create a complete recovery backup
  -> record a unique Codex Migration ID
  -> create a fresh Local Working Copy
  -> calculate the complete affected set
  -> begin one bounded SQLite transaction
  -> apply every related normalized record
  -> validate constraints, foreign keys, IDs, aliases, graph, and Skill references
  -> commit the SQLite transaction
  -> close and reopen the candidate read-only
  -> verify integrity and expected changes
  -> recheck the remote parent for staleness
  -> replace the canonical Google Drive file
  -> fetch and compare the canonical remote copy
  -> deploy or replace the verified current backup
  -> create the required dated recovery snapshot
  -> fetch and compare every required backup
  -> record migration and validation completion
```

A successful local SQLite commit is a validated candidate, not a completed Codex save. Full-save completion requires every configured canonical deployment, backup, and read-back stage.

## Pre-Transaction Backup

Before editing, preserve a complete byte-for-byte recovery source for the active validated Codex. The backup record identifies:

- backup identifier;
- source canonical identity;
- Codex Version;
- hash or strongest available exact evidence;
- creation time;
- operator or runtime;
- intended migration;
- storage location held outside this repository.

No edit occurs until this recovery basis exists and is verifiable under the configured policy.

## SQLite Transaction Procedure

1. fetch the latest configured canonical bytes;
2. reject an old cache or ambiguous source;
3. preserve the pre-transaction backup;
4. create a fresh Local Working Copy;
5. enable `PRAGMA foreign_keys = ON`;
6. verify expected parent Codex Version and migration preconditions;
7. calculate the dependency-closed affected set;
8. begin one bounded transaction;
9. apply all core, relation, revision, deprecation, and migration records;
10. run pre-commit constraints and expected-state checks;
11. commit and close the write connection;
12. reopen the candidate read-only;
13. run integrity, foreign-key, semantic, and expected-versus-actual validation;
14. pass only the validated candidate and evidence to remote deployment.

If a pre-commit step fails, roll back. If post-commit validation fails, discard or quarantine the candidate and preserve the prior canonical Codex.

## Required Validation

At minimum, validation confirms:

- `PRAGMA integrity_check` returns `ok`;
- `PRAGMA foreign_key_check` returns no rows;
- every Codex Stable ID is valid, unique, and unreused;
- aliases cannot create an unresolved identity collision;
- preferred names and aliases resolve through stable identity;
- merge, split, deprecation, and replacement relations are traceable;
- every Evolution node and edge has valid endpoints;
- Evolution graph restrictions and lifecycle states are coherent;
- every trait and Skill reference resolves through its owner;
- variants retain valid bases and explicit differences;
- all expected records exist after the transaction;
- no unrelated record changed or disappeared;
- the candidate reopens read-only;
- no campaign state, player theory, or deployment secret entered the database.

Database integrity does not prove canonical design quality. GM review, owner consistency, and expected-versus-actual comparison remain required.

## Read-Only Verification

Post-write validation uses a newly opened read-only connection against the closed candidate. It must not repair rows, run migrations, normalize values, update timestamps, create defaults, or fill missing information.

If validation discovers a defect, repair begins as a new candidate transaction with its own evidence. A validator never silently edits the candidate it is certifying.

## Google Drive Deployment

The logical remote structure should support roles equivalent to:

```text
GM Living Codex/
|-- Canonical/
|   `-- eternal_cycle_living_codex.sqlite
|-- Backup/
|   |-- latest/
|   |   `-- eternal_cycle_living_codex.sqlite
|   `-- dated/
|       `-- YYYY-MM-DD_Migration-XXX/
|           `-- eternal_cycle_living_codex.sqlite
`-- manifests/
    `-- Migration-XXX.md
```

Equivalent observed structures are valid when Campaign Configuration identifies the same roles. The deployment must maintain:

- exactly one identified canonical file;
- one verified current backup;
- dated recovery snapshots under the configured retention policy;
- one migration manifest per migration;
- canonical and backup read-back validation;
- no competing canonical copies.

No real Drive ID, private URL, credential, access token, sharing principal, or provider-internal locator belongs in this repository.

## Migration Manifest

Each external manifest records at least:

- Codex Migration ID;
- parent and candidate Codex Versions;
- source and design basis;
- backup identifier and verification evidence;
- canonical remote identity by authorized external reference;
- affected stable IDs and table families;
- files or logical records updated;
- validation profile and results;
- canonical and backup comparison methods;
- warnings, conflicts, recovery state, and notes;
- completion time and responsible operator or runtime.

The manifest may point to protected deployment identifiers held in configuration. Universal repository documentation never embeds those values.

## Staleness and Concurrency

Immediately before canonical replacement, compare the remote identity, parent Codex Version, modified revision, hash, or configured concurrency token with the source originally fetched.

If canonical state changed:

- do not overwrite;
- retain the candidate as non-authoritative evidence;
- fetch the new canonical parent;
- identify overlapping and independent affected records;
- reconcile through a new Codex migration;
- validate a proper descendant before deployment.

Last-writer-wins is forbidden.

## Failure Boundary

If fetch, backup, transaction, integrity, foreign-key, semantic, read-only, deployment, canonical read-back, backup, or backup read-back fails:

- preserve the last fully validated Codex;
- do not claim that the Codex update succeeded;
- do not activate the candidate;
- do not narrate durable consequences that require the failed new Codex design;
- report the exact operational boundary in Development Context;
- recover or use an explicitly authorized campaign-local fallback before continuing dependent play.

An optional proposal for future Codex promotion does not have to block unrelated campaign play. A consequence that depends on a newly accepted reusable entry does.

## Adapter Composition

One supported chain is:

```text
SQLite Living Codex store
  -> Google Drive canonical deployment
  -> Google Drive current and dated backups
```

In this chain:

- the [SQLite Persistence Adapter](../ai/chatgpt/adapters/SQLITE_PERSISTENCE_ADAPTER.md) owns local transaction and integrity behavior;
- the [Google Drive Persistence Adapter](../ai/chatgpt/adapters/GOOGLE_DRIVE_PERSISTENCE_ADAPTER.md) owns remote identity, replacement, read-back, and backup behavior;
- external Codex configuration owns actual locators, access, retention, and concurrency policy;
- this document owns Living Codex logical structure and full-save completion.

The adapters are replaceable implementation documents. Replacing an adapter cannot change Living Codex authority, species identity, design meaning, or validation semantics.

## Rendered Views

A renderer may expose approved Codex fields as Markdown, tables, search results, or generated reference pages. Every view must identify its source Codex Version and remain subordinate to the database.

Rendered views must not:

- include Campaign State or campaign-local divergences as base Codex truth;
- expose hidden GM design as player knowledge automatically;
- infer absent fields;
- become an alternate write path;
- survive as authority after their source version is superseded.

## Safeguards

- The Living Codex database is physically and logically separate from campaign databases.
- One exact configured database is canonical; filenames and caches do not establish authority.
- Populated databases and deployment artifacts remain outside this repository.
- Normalized relations replace comma-separated many-to-many fields.
- Stable references use IDs, not names.
- Every accepted change uses one dependency-closed SQLite transaction.
- A complete backup precedes editing.
- Read-only reopen validates the closed candidate.
- Remote canonical and required backups receive read-back and exact comparison.
- Stale writes and last-writer-wins are rejected.
- Missing information remains missing; storage does not invent a value.
- Adapters never adjudicate design or gameplay.
- Full-save failure preserves the prior validated Codex.

## Related Documents

- [GM Living Codex Index](README.md)
- [GM Living Codex Design](GM_LIVING_CODEX.md)
- [Species Registry](SPECIES_REGISTRY.md)
- [Campaign Persistence Engine](../persistence/README.md)
- [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md)
- [AI Save Protocol](../ai/AI_SAVE_PROTOCOL.md)
- [SQLite Persistence Adapter](../ai/chatgpt/adapters/SQLITE_PERSISTENCE_ADAPTER.md)
- [Google Drive Persistence Adapter](../ai/chatgpt/adapters/GOOGLE_DRIVE_PERSISTENCE_ADAPTER.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
