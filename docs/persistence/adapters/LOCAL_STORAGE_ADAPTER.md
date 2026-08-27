# Local Storage Adapter

## Purpose

This Storage Adapter defines exact local artifact identity, activation, read-back, backup, and recovery for `DIRECT` persistence. It does not define a database format.

## Document Control

- **Owner:** canonical local locator, file locking, candidate placement, atomic activation, local read-back, backup identity, and local recovery
- **Primary authorities:** [Direct Persistence Mode](../DIRECT_PERSISTENCE_MODE.md), [Migration and Versioning](../MIGRATION_AND_VERSIONING.md), and [Persistence Validation](../PERSISTENCE_VALIDATION.md)
- **Dependencies:** Campaign Configuration, one Database Format Adapter, authorized filesystem access, and a recovery policy
- **Extensions:** operating-system-specific locking, snapshots, checksums, and encrypted storage
- **Consumers:** direct-capable runtimes, campaign applications, custodians, and recovery tools
- **Repository boundary:** no machine path, campaign file, credential, live hash, or populated backup belongs here

## Exact Identity

Campaign Configuration identifies one canonical local artifact. Similar filenames, newest modification time, prior downloads, temporary working copies, and backup files do not select authority.

The adapter must distinguish:

- canonical artifact;
- staged candidate;
- temporary Local Working Copy;
- verified current backup;
- historical recovery snapshot.

## Activation Procedure

1. resolve and lock the exact canonical artifact;
2. verify its identity and active parent evidence;
3. preserve the required pre-write backup;
4. receive a candidate validated by the Database Format Adapter;
5. recheck that the canonical parent has not changed;
6. activate by an atomic filesystem operation appropriate to the platform;
7. reopen the exact canonical artifact independently;
8. compare hash or exact bytes and rerun required format validation;
9. verify the backup and record operational evidence;
10. release locks and return completion evidence.

An overwrite that can expose a partial file is not atomic activation.

## Status

When local storage is the configured canonical authority, full commit and read-back support `💾`. An unactivated candidate supports `⏳`; failed activation, read-back, or required backup supports `⚠️`.

## Safeguards

- A Local Working Copy never becomes authoritative by convenience.
- The canonical target is never inferred from filename similarity.
- Writer locking and parent-version checks are both required.
- Backup files cannot compete with the canonical locator.
- Failure preserves the prior validated artifact and retry identity.
- Local storage never adjudicates gameplay or interprets payloads.

## Related Documents

- [SQLite Database Format Adapter](SQLITE_DATABASE_FORMAT_ADAPTER.md)
- [DuckDB Database Format Adapter](DUCKDB_DATABASE_FORMAT_ADAPTER.md)
- [Google Drive Remote Storage Adapter](GOOGLE_DRIVE_REMOTE_STORAGE_ADAPTER.md)
