# DuckDB Database Format Adapter

## Purpose

This Database Format Adapter defines how a capable runtime may use a persistent DuckDB database in `DIRECT` mode without changing Campaign Persistence semantics.

## Document Control

- **Owner:** DuckDB connection, transaction, constraint, concurrency, checkpoint, read-back, and candidate behavior
- **Primary authorities:** [Direct Persistence Mode](../DIRECT_PERSISTENCE_MODE.md), [Save Update Protocol](../SAVE_UPDATE_PROTOCOL.md), and [Persistence Validation](../PERSISTENCE_VALIDATION.md)
- **Dependencies:** one exact persistent database identity, active parent version, complete Affected Set, one coordinated writer process, and authorized runtime support
- **Extensions:** Local or Remote Storage Adapters may activate validated database artifacts without interpreting them
- **Consumers:** direct-capable runtimes, campaign applications, custodians, migration tooling, and recovery tools
- **Repository boundary:** no live DuckDB file, campaign locator, credential, schema instance, or populated record belongs here

## Fit and Boundary

DuckDB supports ACID transactions and snapshot isolation. Its native embedded concurrency model is best treated as one coordinated read-write process, with separate processes limited to read-only access unless a separately approved deployment architecture supplies stronger coordination. A shared folder does not create safe multi-process write authority.

The adapter is suitable only when the runtime can preserve FR-011's small durable turn boundaries despite DuckDB's analytical orientation. Bulk-query convenience does not authorize batching unsaved gameplay across completed interactions.

## Required Procedure

1. resolve the exact persistent DuckDB artifact through Campaign Configuration and the selected Storage Adapter;
2. reject in-memory or cached connections when persistent authority is required;
3. acquire the configured single-writer lease;
4. read the Save Index, active Campaign Version, and required Read Set;
5. begin one bounded transaction;
6. verify the expected parent and record revisions;
7. apply the complete owner-routed Affected Set with parameterized statements;
8. verify primary-key, unique, foreign-key, check, and application-level semantic constraints;
9. commit or roll back as one unit;
10. checkpoint when the configured durability policy requires it;
11. close the writer and reopen through a fresh read-only connection;
12. compare expected and actual owner values, references, and Campaign Version;
13. pass validated evidence or candidate bytes to the selected Storage Adapter.

## Concurrency

- One configured process coordinates writes to a native DuckDB database.
- Optimistic write conflicts fail the candidate; they do not permit last-writer-wins.
- Retry reloads the active parent and revalidates the complete transaction.
- A read-only process may not promote its cached view to canonical state.
- Multi-process write deployment requires a future approved adapter or service architecture rather than an improvised file lock.

## Integrity and Read-Back

At minimum verify:

- the database opens in persistent mode;
- schema and migration versions match Campaign Configuration;
- the expected parent and candidate versions exist;
- every mutation has its expected record revision and payload;
- all required references resolve;
- no unrelated owner changed;
- the candidate can be reopened read-only;
- checkpoint and artifact evidence meet the configured Storage Adapter policy.

DuckDB constraints aid validation but do not replace Campaign Persistence semantic checks.

## Security

Use parameterized statements and fixed application-owned query shapes. Client or campaign text must never become executable SQL. Limit extension loading, external file access, memory, threads, and temporary storage according to deployment policy.

## Safeguards

- Persistent mode is mandatory for canonical state.
- One coordinated writer process owns mutation.
- A transaction conflict is a failure requiring reload, not permission to overwrite.
- In-memory databases, exported Parquet, and query results are non-authoritative unless a migration explicitly changes authority.
- DuckDB never adjudicates gameplay.

## Implementation References

- [DuckDB transaction documentation](https://duckdb.org/docs/stable/sql/statements/transactions)
- [DuckDB concurrency documentation](https://duckdb.org/docs/stable/connect/concurrency)
- [DuckDB constraint documentation](https://duckdb.org/docs/stable/sql/constraints)
- [DuckDB security guidance](https://duckdb.org/docs/stable/operations_manual/securing_duckdb/overview)

## Related Documents

- [Local Storage Adapter](LOCAL_STORAGE_ADAPTER.md)
- [SQLite Database Format Adapter](SQLITE_DATABASE_FORMAT_ADAPTER.md)
- [Migration and Versioning](../MIGRATION_AND_VERSIONING.md)
