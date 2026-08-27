# Portable Persistence Architecture

## Purpose

This document defines the portable execution boundary for the storage-neutral [Campaign Persistence Engine](README.md). A campaign chooses one first-class **Persistence Mode**: `DIRECT` or `MCP`. Both modes preserve the same authority, ownership, versioning, migration, validation, and FR-011 turn-completion guarantees.

The mode changes how an authorized runtime obtains persistence services. It does not change what campaign facts mean.

## Document Control

- **Owner:** persistence-mode selection, adapter-role separation, cross-mode invariants, and portability boundaries
- **Primary authorities:** [Persistence Authority](PERSISTENCE_AUTHORITY.md), [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md), [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md), and [Persistence Validation](PERSISTENCE_VALIDATION.md)
- **Dependencies:** Campaign Configuration, Save Index, active Campaign Version, Affected Set, validation profile, and execution capability
- **Extensions:** approved Database Format Adapters, Storage Adapters, MCP persistence services, deployment policies, and migration tooling
- **Consumers:** human and AI GMs, AI Execution Profiles, campaign applications, persistence operators, and FR-011 Context Assembly
- **Repository boundary:** no campaign locator, connection string, credential, populated database, active receipt, or deployment secret belongs here

## Shared Logical Contract

Every mode must:

1. resolve the configured canonical authority before state-changing play;
2. read the active Campaign Version and required canonical Read Set;
3. accept one stable Transaction ID, idempotency key, expected parent, and complete Affected Set;
4. route each mutation to its Authoritative Record Owner;
5. reject stale parents, duplicate identities, dangling references, and unproved change;
6. stage or transact the complete change atomically;
7. validate expected state and required references;
8. activate one canonical descendant;
9. read the activated result back;
10. return evidence that truthfully determines `saved`, `pending`, or `failed` status.

A mode cannot weaken [Save-Before-Delivery](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md), replace Campaign Configuration, or move gameplay adjudication into a storage component.

## Mode 1: DIRECT

In `DIRECT` mode, the runtime operates an explicit adapter pipeline:

```text
Campaign Persistence Engine
  -> Database Format Adapter
  -> Local Storage Adapter and/or Remote Storage Adapter
  -> validated canonical target
```

The Database Format Adapter owns database syntax, bounded transactions, integrity, and format-specific read-back. Storage Adapters own placement, identity, transport, synchronization, backup, and recovery for an artifact. A runtime may use SQLite, DuckDB, or a later approved format only when it can faithfully execute that format's adapter contract.

The [Direct Persistence Mode](DIRECT_PERSISTENCE_MODE.md) defines composition and evidence requirements.

## Mode 2: MCP

In `MCP` mode, the runtime does not operate a campaign database or storage artifact. It calls a configured semantic persistence service:

```text
GM or AI runtime
  -> Eternal Cycle MCP persistence interface
  -> service-owned transaction and validation
  -> service-owned Microsoft SQL Server deployment
  -> service-owned durability, backup, and recovery
```

The service accepts canonical record addresses, reads, Affected Sets, mutations, references, and transaction identities. It does not expose arbitrary SQL and does not adjudicate gameplay. Backend locality, topology, connection details, backup targets, and recovery mechanisms remain backstage.

The [MCP Persistence Mode](MCP_PERSISTENCE_MODE.md) defines the service contract. The repository includes a reference [Eternal Cycle MCP service](../../services/eternal-cycle-mcp/README.md).

## Campaign Configuration

Campaign Configuration selects exactly one mode and provides only the fields relevant to it.

```text
Persistence Mode: DIRECT | MCP

DIRECT:
  Database Format Adapter
  Local Storage Adapter or Remote Storage Adapter chain
  exact canonical artifact identity
  concurrency, backup, and verification policy

MCP:
  service identity
  campaign identity
  authorized interface profile
  service contract version
  no client-side database or storage adapter chain
```

Configuration values belong outside the universal repository. A runtime must not guess a mode from an available tool, filename, product name, or remembered prior session.

## Evidence and Save Status

`DIRECT` mode derives status from the configured Adapter Chain:

- `💾` means a local-authoritative target committed and validated;
- `☁️💾` means a cloud-authoritative target synchronized and verified;
- `⏳` means the configured chain remains incomplete;
- `⚠️` means a required write, synchronization, or validation stage failed.

`MCP` mode derives status only from service evidence:

- `💾` means the service returned a validated Persistence Receipt for the active Campaign Version;
- `⏳` means service-side staging, activation, durability, or verification remains incomplete;
- `⚠️` means the service reported or exposed a transaction, validation, durability, or availability failure.

MCP clients do not use `☁️💾`: server topology is intentionally hidden, and a local client has no authority to classify the service's backend as local or cloud. No mode may report success from intention, an attempted call, or an unverified response.

## Mode Changes

Changing mode is a controlled persistence migration, not an ordinary configuration edit.

The migration must:

1. preserve the source as a validated backup;
2. record source mode, format, versions, and authority;
3. audit every authoritative record and stable reference;
4. import into the destination without changing campaign meaning;
5. validate counts, identities, references, chronology, and expected hashes or equivalent evidence;
6. activate the destination authority exactly once;
7. retain migration provenance and an authorized rollback route;
8. prevent concurrent writes to the retired source.

Conversation memory, summaries, or rendered exports are not sufficient migration sources when canonical structured state exists.

## Failure Boundaries

- A Direct Adapter failure is reported at the responsible format or storage stage.
- An MCP service failure is reported as a semantic persistence failure; backend implementation details remain hidden unless an authorized operator enters Development Context.
- A runtime capability gap blocks state-changing play when the selected mode cannot be operated faithfully.
- A failed mode does not authorize silently falling back to the other mode.
- A pending or failed transaction is resumed by stable identity; gameplay effects are not replayed.

## Safeguards

- The Campaign Persistence Engine remains database- and transport-neutral.
- `DIRECT` and `MCP` are mutually exclusive canonical modes for one active campaign authority.
- Database Format Adapters and Storage Adapters never adjudicate gameplay.
- The MCP service never grants raw SQL authority to a GM or player.
- An MCP client never receives backend credentials, paths, database names, or backup locators through ordinary gameplay tools.
- A Persistence Receipt proves service completion; it does not become a second owner of campaign facts.
- Switching runtimes does not require switching modes when the new runtime supports the configured contract.
- Switching modes requires migration and validation, never implicit cache promotion.

## Related Documents

- [Direct Persistence Mode](DIRECT_PERSISTENCE_MODE.md)
- [MCP Persistence Mode](MCP_PERSISTENCE_MODE.md)
- [Persistence Adapter Index](adapters/README.md)
- [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md)
- [Context Assembly and Turn Persistence](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Save Index Template](../../templates/SAVE_INDEX_TEMPLATE.md)
