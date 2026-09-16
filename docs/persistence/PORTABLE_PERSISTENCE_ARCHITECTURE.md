# Portable Persistence Architecture

## Purpose

This document defines the portable execution boundary for the storage-neutral [Campaign Persistence Engine](README.md). Eternal Cycle specifies semantic capabilities, authority, lifecycle, and evidence rather than one database, protocol, runtime, or hosting product.

## Document Control

- **Owner:** persistence strategy boundary, shared invariants, selection, authority, evidence, and migration
- **Primary authorities:** [Campaign Persistence Philosophy](CAMPAIGN_PERSISTENCE_PHILOSOPHY.md), [Persistence Authority](PERSISTENCE_AUTHORITY.md), and [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md)
- **Dependencies:** Campaign Configuration, Save Index, runtime capabilities, Logical Data Namespace, migration, and validation
- **Extensions:** Direct adapters, Managed Data Services, service interfaces, storage implementations, and deployment policy
- **Consumers:** bootstrap, session start, AI Execution Profiles, human GMs, migration, validation, and tooling
- **Repository boundary:** no campaign binding, locator, credential, service endpoint, or populated state belongs here

## Strategies

A campaign selects exactly one canonical persistence strategy.

### DIRECT

The authorized runtime directly operates a structured Database Format Adapter and any Local or Remote Storage Adapters required by the canonical authority. SQLite and DuckDB are supplied examples. Direct mode retains local/cloud evidence, read-back, backup, and synchronization semantics.

See [Direct Persistence Mode](DIRECT_PERSISTENCE_MODE.md).

### MANAGED

An authorized [Managed Data Service](MANAGED_DATA_SERVICE.md) is the canonical persistence authority. The runtime uses semantic service operations and receives validated evidence without operating the service's database, files, replication, backup, or recovery infrastructure.

MCP is the supplied reference interface, not a universal mode or requirement. Managed implementations may use another compliant service interface and relational, document, key/value, graph, indexed-file, or other structured storage.

When Managed is selected, Direct local/cloud save machinery is inactive. A local working artifact or service cache cannot become a competing canonical save.

## Selection

Campaign initialization follows [Enumerate -> Select -> Persist -> Reuse](PERSISTENCE_STRATEGY_SELECTION.md). The runtime enumerates actually available compliant strategies, selects one under campaign authority, persists the choice and reconnect information, and reuses it on later sessions.

Existing configuration always wins over new environmental preference. Strategy change is an explicit validated migration, never a fallback response.

## Shared Invariants

Both strategies preserve:

- one stable Campaign ID and one active canonical authority;
- one authoritative logical owner per mutable fact;
- exact Read and Affected Sets;
- stable transaction and idempotency identity;
- expected-parent or equivalent concurrency control;
- validation before activation;
- canonical read-back and evidence before turn completion;
- retry without gameplay replay;
- Truth Layer, visibility, and GM Secret protection;
- storage-neutral migration and rollback provenance;
- honest `💾`, `⏳`, and `⚠️` status.

Implementation convenience never weakens these invariants.

## Logical Data Namespace

[Logical Data Namespace](LOGICAL_DATA_NAMESPACE.md) is the universal storage boundary. An adapter maps it to the nearest faithful native concept. A namespace may host several compatible campaigns isolated by Campaign ID. World/Ruleset identity, namespace identity, physical schema, and database identity remain separate.

## Rule Delivery

Canonical Markdown remains reusable Rule Canon. Normal play consumes selective provenance-bearing rules rather than loading and reconstructing the repository.

- In `MANAGED`, the service owns source acquisition, compilation, validation, versioned publication, activation, update checking, indexing, dependency expansion, and bounded Rule Packet assembly.
- In `DIRECT`, a packaged index, local compiler/index, repository-aware runtime, or another validated mechanism may provide selective rule delivery without requiring MCP.

See [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md).

## Completion Status

- Direct local authority uses `💾` only after local commit and validation.
- Direct cloud authority uses `☁️💾` only after required synchronization and remote verification.
- Managed authority uses `💾` only after validated service completion evidence.
- `⏳` means required work remains incomplete.
- `⚠️` means persistence, synchronization, service, or validation failure.

Managed clients do not classify hidden backend topology and therefore never display `☁️💾` for service internals.

## Compatibility

- Existing v1 SQLite and Google Drive campaigns remain `DIRECT`.
- Existing post-v1 configurations labelled `MCP` migrate non-destructively to `MANAGED` with MCP as their interface.
- Existing SQL Server `ec` deployments remain valid physical mappings.
- No campaign is silently converted, duplicated, or upgraded to an incompatible Rule Release.

## Safeguards

- No silent strategy fallback or switching.
- No simultaneous Direct and Managed canonical writers.
- No raw SQL or datastore administration in the universal gameplay contract.
- No vendor-specific term defines a universal authority.
- No compiled representation becomes Rule Canon.
- No service implementation adjudicates gameplay.
- No migration targets unrelated Data Namespaces.
- No populated campaign data enters this repository.

## Related Documents

- [Persistence Strategy Selection](PERSISTENCE_STRATEGY_SELECTION.md)
- [Direct Persistence Mode](DIRECT_PERSISTENCE_MODE.md)
- [Managed Data Service](MANAGED_DATA_SERVICE.md)
- [MCP Managed Service Interface](MCP_PERSISTENCE_MODE.md)
- [Logical Data Namespace](LOGICAL_DATA_NAMESPACE.md)
- [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Save Index Template](../../templates/SAVE_INDEX_TEMPLATE.md)
