# FR-017 Portable Persistence and MCP Service Audit

## Scope

This audit records the owner-authorized Phase 13 implementation of **FR-017 — Portable Persistence Architecture and MCP Persistence Service**. The work begins after Release 1 at commit `07abaa7`; the `v1.0.0` tag and Release 1 metadata remain historical and unchanged.

No campaign state, populated database, credential, private locator, or gameplay mechanic entered the repository.

## Release 1 Coupling

The logical Campaign Persistence Engine was already storage-neutral, but operational guidance placed SQLite and Google Drive adapters beneath the ChatGPT execution profile and framed target resolution primarily as local versus cloud. This created three ambiguities:

1. a non-ChatGPT runtime could appear to require ChatGPT-owned adapters;
2. database format and artifact placement were both called Persistence Adapters without a first-class subtype boundary;
3. a runtime with MCP support had no canonical way to delegate the database, durability, backup, and recovery to a service.

## Implemented Architecture

The [Portable Persistence Architecture](../../docs/persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md) defines two first-class modes:

- `DIRECT` composes one Database Format Adapter with the Local or Remote Storage Adapters required by canonical authority;
- `MCP` invokes a semantic Eternal Cycle service that owns its hidden backend and returns canonical records, status, and validated Persistence Receipts.

Both modes retain FR-011 target resolution, mandatory reads, complete Affected Sets, one owner-routed transaction, expected-parent concurrency, idempotent retry, validation, activation, read-back, and evidence-based completion.

## Adapter Reclassification

The Release 1 adapter content was preserved and moved out of the ChatGPT profile:

- `SQLITE_PERSISTENCE_ADAPTER.md` became the runtime-neutral [SQLite Database Format Adapter](../../docs/persistence/adapters/SQLITE_DATABASE_FORMAT_ADAPTER.md);
- `GOOGLE_DRIVE_PERSISTENCE_ADAPTER.md` became the runtime-neutral [Google Drive Remote Storage Adapter](../../docs/persistence/adapters/GOOGLE_DRIVE_REMOTE_STORAGE_ADAPTER.md).

The [DuckDB Database Format Adapter](../../docs/persistence/adapters/DUCKDB_DATABASE_FORMAT_ADAPTER.md) and [Local Storage Adapter](../../docs/persistence/adapters/LOCAL_STORAGE_ADAPTER.md) complete the initial Direct taxonomy. No compatibility matrix or automatic format fallback was introduced.

## Reference MCP Service

The executable [Eternal Cycle MCP Persistence Service](../../services/eternal-cycle-mcp/README.md) uses the official C# MCP SDK and Microsoft SQL client. It exposes bounded semantic tools for status, exact record reads, complete commit, and idempotent retry. It exposes no arbitrary SQL or backend locator.

The SQL Server model provides:

- stable campaign identity and active Campaign Version;
- stable Transaction and idempotency identities;
- append-only candidate record versions;
- stable typed references;
- candidate validation before activation;
- authoritative reads that exclude unactivated and failed candidates, even when competing candidates share a proposed version;
- candidate validation scoped to the exact Transaction ID;
- activation read-back before receipt completion;
- validation, receipt, and recovery-point records.

The active pointer remains on the prior version during candidate validation. An activation with incomplete read-back remains resumable and cannot produce a successful receipt. Optional required SQL Server backup and `RESTORE VERIFYONLY` can participate in the service-side completion gate; scheduled durability remains a service deployment responsibility.

## Status and Host Boundary

Direct local and cloud markers retain their Release 1 meanings. MCP mode uses `💾` only for a validated Persistence Receipt and never uses `☁️💾`, because backend topology is hidden and is not client authority.

The reference service can enforce request validation, staging, concurrency, activation, read-back, durability policy, and receipt formation. An AI host must still invoke the configured tools, preserve stable transaction identity, and refuse ordinary state-changing completion without the receipt. The repository cannot force a host to make an MCP call it declines to make.

## Compatibility and Migration

Existing Release 1 Direct SQLite/Google Drive campaigns remain valid. Their Campaign Configuration must classify the existing components as `DIRECT` when adopting FR-017. No populated campaign migration is performed here.

Moving from SQLite to DuckDB, between Direct storage authorities, or between `DIRECT` and `MCP` is a normal Backup, Audit, Merge, Validation, activation, and rollback migration. Conversation context and summaries cannot replace the structured source.

## Validation

- Reference service build: pass with zero warnings.
- MCP service unit tests: pass, 8 of 8, covering idempotent commit, receipt enforcement, pending completion, retry without replay, exact Affected Set, stable reads, topology hiding, and configured durability behavior.
- FR-017 structural regression harness: pass, 26 assertions covering mode contracts, adapter taxonomy, SQL schema invariants, semantic tool surface, candidate isolation, exact transaction validation, no obsolete adapters, roadmap provenance, and hidden-backend safeguards.
- Full repository validation: pass across 260 Markdown files, 6,981 relative links, 157 anchors, 173 indexed canonical documents, 43 templates, 1,205 terminology checks, 188 roadmap tasks, and 17 Future Revision entries.
- Blocking unresolved questions, orphaned Markdown documents, forbidden campaign-data directories, and repository-boundary findings: zero.

No live SQL Server was provisioned because the reusable repository contains no populated campaign deployment. Deployment acceptance still requires applying the blank schema to an authorized test SQL Server and exercising transaction, conflict, backup, restore-verification, and recovery procedures under that deployment's policy.

## Result

FR-017 is complete as a post-v1 Phase 13 objective. Eternal Cycle v1.0.0 remains the historical Release 1 tag. Phase 13 remains active with the permanent owner-mediated Future Revisions objective, and no later revision is selected automatically.
