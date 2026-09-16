# Managed Data Service

## Purpose

This document defines the storage- and protocol-neutral contract for a service that owns canonical campaign persistence on behalf of a runtime.

## Document Control

- **Owner:** Managed service capabilities, semantic operations, authority, evidence, failure, diagnostics, and administrative separation
- **Dependencies:** [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md), [Logical Data Namespace](LOGICAL_DATA_NAMESPACE.md), [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md), and [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md)
- **Extensions:** MCP, HTTP, local IPC, or another authorized interface; relational, document, key/value, graph, or indexed-file storage implementations
- **Consumers:** Campaign Configuration, AI and human GM clients, service implementers, diagnostics, and migration tooling
- **Repository boundary:** no endpoint, credential, connection string, populated campaign record, or private diagnostic belongs here

## Authority

When `MANAGED` is active, the selected service is the canonical campaign persistence authority. It replaces the client's Direct local/cloud save machinery. The client does not maintain a parallel canonical SQLite database or Google Drive replica, classify hidden backend topology, or infer success from transport completion.

The service carries and validates campaign state. It does not adjudicate mechanics, replace Repository Canon, choose player actions, or promote derived data into Canon.

## Required Capability Families

### Service and status

A compliant service can report:

- service contract and implementation identity;
- supported capability set;
- active storage adapter family without exposing secrets;
- campaign binding and Logical Data Namespace identity;
- persistence and rules-release status;
- sanitized health and failure information.

### Campaign

The service supports authorized provisioning, open/resume, canonical record or context reads, validation, migration, and stable Campaign ID routing. Provisioning remains an administrative/bootstrap operation and need not be exposed to gameplay clients.

### Turn persistence

The service supports stable transaction identity, exact Read and Affected Sets, expected-parent or equivalent optimistic concurrency, idempotent commit and retry, validation, atomic activation, read-back, and evidence sufficient for the turn-completion gate.

### Rules

The service identifies the active applicable Rule Release and returns a bounded, provenance-bearing Rule Packet. It acquires, compiles, validates, publishes, indexes, and updates reusable rules independently of the AI client as defined by [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md).

### Administration

Authorized administrative surfaces may provide source synchronization, compilation, candidate validation, publication, activation, migration, backup, recovery, and diagnostics. Ordinary gameplay capability does not imply administrative authority.

## Storage Neutrality

A Managed implementation may use a relational database, document database, key/value store, graph store, indexed file-backed store, or another structured technology. It must faithfully implement stable identity, Logical Data Namespaces, campaign isolation, versioning, concurrency, validation, indexed retrieval, migration, durability, and recovery.

Eternal Cycle does not require a store to imitate SQL concepts. An implementation maps the semantic contract to its native strengths.

## Interface Neutrality

MCP is the supplied reference interface, not a universal requirement. A different interface is compliant when it preserves the same domain operations, authority, access control, evidence, idempotency, diagnostics, and failure semantics without exposing raw backend operations to gameplay clients.

## Completion Evidence

`💾` means the selected Managed service returned validated completion evidence for the expected transaction and active Campaign Version. `⏳` means service work remains incomplete. `⚠️` means service, transaction, durability, or validation failure. The client never reports `☁️💾` for a Managed authority because backend replication is service-owned and hidden.

## Failure and Offline Behavior

- A failed campaign write does not complete the turn.
- A failed rule-source check does not invalidate an already verified active Rule Release.
- A failed rule candidate never replaces the active release.
- Retry resumes stable transaction or publication identity and does not replay gameplay.
- Loss of the selected service blocks dependent state-changing play; it does not authorize silent Direct fallback.
- Sanitized diagnostics distinguish degraded update status from unavailable gameplay authority.

## Security

- No arbitrary SQL or general backend query surface is part of the gameplay contract.
- Data Namespace and Campaign ID authorization are independent checks.
- Administrative capabilities are separately authorized.
- Credentials, tokens, connection strings, private locators, full Campaign Canon, and GM Secrets are excluded from ordinary status and diagnostics.
- Physical mappings are selected only through trusted configuration and safely validated by the implementation.

## Reference Implementation

The repository includes an optional [.NET / MCP / T-SQL reference implementation](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md). It demonstrates one compliant combination; it does not define the universal architecture. Its MCP interface, .NET runtime, SQL Server adapter, `ec_domain` rule store, and Git source provider are replaceable implementation choices.

## Related Documents

- [Persistence Strategy Selection](PERSISTENCE_STRATEGY_SELECTION.md)
- [Logical Data Namespace](LOGICAL_DATA_NAMESPACE.md)
- [MCP Managed Service Interface](MCP_PERSISTENCE_MODE.md)
- [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md)
- [Community Feedback and Diagnostics](../support/COMMUNITY_FEEDBACK_AND_DIAGNOSTICS.md)
