# Direct Persistence Mode

## Purpose

`DIRECT` persistence permits an authorized runtime to operate the configured campaign store through explicit Database Format and Storage Adapters. It is portable across AI and human runtimes; it is not a ChatGPT profile.

## Document Control

- **Owner:** Direct Adapter Chain composition, capability checks, stage evidence, and direct-mode failure boundaries
- **Primary authorities:** [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md), [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md), and [Persistence Validation](PERSISTENCE_VALIDATION.md)
- **Dependencies:** Campaign Configuration, Save Index, one Database Format Adapter, at least one canonical Storage Adapter, and authorized runtime tools
- **Extensions:** SQLite, DuckDB, local filesystems, Google Drive, and future approved adapters
- **Consumers:** capable AI runtimes, campaign applications, human operators, and migration tooling
- **Repository boundary:** no live artifact, path, cloud identifier, credential, or populated campaign record belongs here

## Adapter Classes

### Database Format Adapter

Owns:

- opening and identifying the configured logical database;
- parameterized reads and owner-routed writes;
- transactions and rollback;
- format constraints and integrity checks;
- stale-write protection;
- read-only or fresh-connection validation;
- production of a validated candidate or local canonical result.

It does not own remote placement, fictional meaning, or gameplay adjudication.

### Local Storage Adapter

Owns exact local artifact identity, access policy, atomic replacement where applicable, local read-back, backup placement, locking, and recovery. It does not interpret database contents.

### Remote Storage Adapter

Owns exact remote identity, fetch-latest, concurrency protection, canonical replacement, remote read-back, backup propagation, and recovery. It does not perform database transactions or semantic adjudication.

## Valid Chains

Examples include:

```text
SQLite Database Format Adapter
  -> Local Storage Adapter

SQLite Database Format Adapter
  -> Google Drive Remote Storage Adapter

DuckDB Database Format Adapter
  -> Local Storage Adapter
```

An adapter appears only when it owns a real stage. Do not add a Local Storage Adapter merely because a remote candidate was temporarily downloaded; that file remains a non-authoritative Local Working Copy.

## Runtime Capability Gate

Before state-changing play, the runtime must prove it can:

- invoke every configured adapter;
- resolve the exact canonical target;
- preserve transaction and idempotency identities;
- perform required validation and read-back;
- retain pending candidates for retry;
- protect credentials and GM Secrets;
- report truthful persistence status.

If any required capability is unavailable, `PERSISTENCE_TARGET_READY` is false. The runtime may continue read-only work where safe, but it cannot claim durable state change.

## Direct Transaction Flow

```text
resolve exact canonical target
-> fetch or open latest authoritative state
-> verify active parent and format identity
-> create bounded candidate or transaction
-> apply complete Affected Set
-> validate format constraints and semantic expectations
-> commit candidate
-> reopen or refetch independently
-> verify expected active state
-> complete required backup/deployment stages
-> report evidence-based status
```

Each adapter returns evidence only for its own responsibility. Successful SQL commit does not prove remote deployment; successful upload does not prove database integrity.

## Format Selection

SQLite is appropriate for portable transactional campaign files and has a complete [SQLite adapter](adapters/SQLITE_DATABASE_FORMAT_ADAPTER.md).

DuckDB is supported through the [DuckDB adapter](adapters/DUCKDB_DATABASE_FORMAT_ADAPTER.md) when the deployment respects its single-writer-process and optimistic-concurrency constraints. Its analytical strengths do not waive small-turn transaction or validation requirements.

A future format requires an approved adapter defining identity, transactions, integrity, concurrency, read-back, migration, and failure behavior. Mere query support is insufficient.

## Direct Status

- local-authoritative completion: `💾`;
- remote/cloud-authoritative completion after remote verification: `☁️💾`;
- incomplete required stage: `⏳`;
- failed required stage: `⚠️`.

The selected canonical authority determines completion. A local candidate in a remote-authoritative chain remains pending or failed until the remote stage succeeds.

## Safeguards

- One active Database Format Adapter owns database behavior.
- One configured adapter chain owns canonical placement.
- Caches and Local Working Copies never gain authority by recency.
- A runtime cannot combine independently authoritative direct targets.
- Adapter retries resume by transaction identity and do not replay gameplay.
- Format-specific convenience never duplicates mutable logical ownership.
- Mode fallback requires an authorized migration.

## Related Documents

- [Persistence Adapter Index](adapters/README.md)
- [MCP Persistence Mode](MCP_PERSISTENCE_MODE.md)
- [Campaign Configuration Template](../../templates/PERSISTENCE_CONFIGURATION_TEMPLATE.md)
- [AI Capabilities and Limitations](../ai/AI_CAPABILITIES_AND_LIMITATIONS.md)
