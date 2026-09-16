# Direct Persistence Adapter Index

These documents implement `DIRECT` persistence. Database Format Adapters own database behavior; Storage Adapters own artifact placement and transport. They are runtime-neutral and do not adjudicate gameplay.

## Database Format Adapters

- [SQLite Database Format Adapter](SQLITE_DATABASE_FORMAT_ADAPTER.md) - transactional campaign files, integrity, foreign keys, candidate validation, and read-only reopen.
- [DuckDB Database Format Adapter](DUCKDB_DATABASE_FORMAT_ADAPTER.md) - persistent DuckDB transactions, optimistic concurrency, single-writer-process boundary, checkpointing, and validation.

## Storage Adapters

- [Local Storage Adapter](LOCAL_STORAGE_ADAPTER.md) - exact local artifact identity, locking, atomic activation, read-back, backup, and recovery.
- [Google Drive Remote Storage Adapter](GOOGLE_DRIVE_REMOTE_STORAGE_ADAPTER.md) - exact remote identity, fetch-latest, canonical replacement, remote read-back, backup propagation, and security.

## Selection

Campaign Configuration selects one Database Format Adapter and the Storage Adapter chain required by the canonical authority. An adapter is not selected merely because a runtime exposes a similarly named tool.

Managed persistence does not use these client-side adapters. See the [Portable Persistence Architecture](../PORTABLE_PERSISTENCE_ARCHITECTURE.md), [Managed Data Service](../MANAGED_DATA_SERVICE.md), and [MCP Managed Service Interface](../MCP_PERSISTENCE_MODE.md).
