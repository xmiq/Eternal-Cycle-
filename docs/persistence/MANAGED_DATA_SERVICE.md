# Managed Data Service

## Purpose

This document defines the storage- and protocol-neutral contract for a service that owns canonical campaign persistence on behalf of a runtime.

## Document Control

- **Owner:** Managed service capabilities, semantic operations, authority, evidence, failure, diagnostics, and administrative separation
- **Dependencies:** [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md), [Logical Data Namespace](LOGICAL_DATA_NAMESPACE.md), [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md), [Managed Operations](MANAGED_OPERATIONS.md), and [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md)
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

Long-running administrative work follows the [Managed Operations](MANAGED_OPERATIONS.md) contract. Initiation returns durable operation identity promptly; status and recovery do not depend on the initiating client remaining connected.

Readiness is multidimensional. It distinguishes transport, persistence connection, campaign schema, Domain Namespace, Rule Source, published release, active compatible release, requested campaign, and overall gameplay readiness. Expected first-run states are structured outcomes such as setup, migration, source, publication, activation, or campaign required; they are not generic exceptions.

### Campaign

The service supports authorized provisioning, open/resume, canonical record or context reads, validation, migration, and stable Campaign ID routing. Provisioning remains an administrative/bootstrap operation and need not be exposed to gameplay clients.

Campaign discovery preserves stable IDs without making players type them. Zero campaigns leads to authorized creation, one appropriate campaign may resolve automatically, and several campaigns are presented with meaningful names and descriptions.

### Turn persistence

The service supports stable transaction identity, exact Read and Affected Sets, expected-parent or equivalent optimistic concurrency, idempotent commit and retry, validation, atomic activation, read-back, and evidence sufficient for the turn-completion gate.

### Rules

The service identifies the active applicable Rule Release and returns a bounded, provenance-bearing Rule Packet. It acquires, compiles, validates, publishes, indexes, and updates reusable rules independently of the AI client as defined by [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md).

### Administration

Authorized administrative surfaces may provide source synchronization, compilation, candidate validation, publication, activation, migration, backup, recovery, and diagnostics. Ordinary gameplay capability does not imply administrative authority.

First-run initialization is an expected administrative state. A compliant service previews the implementation-owned scope, requires explicit approval, applies only versioned service-owned migrations or equivalent bounded operations, validates the result, and is repeat-safe. It exposes no arbitrary datastore command. Rule Source selection and initial publication follow the same separate authorization boundary.

## First-Run Flow

```text
inspect readiness
  -> preview bounded service-owned setup
  -> explicit approval
  -> initialize and validate
  -> select and persist Rule Source
  -> create or reuse durable publication operation
  -> observe minimum authoritative closure readiness
  -> begin safe gameplay when GameplayReady
  -> continue preparing remaining rules until FullRulesetReady
  -> create or resume campaign
```

A service with an available database but missing rule infrastructure is not ready. A configured source with no publication is distinct from a published but inactive release. Source unavailability may be `DEGRADED` only when a compatible active release remains safe to use.

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
- Expected setup failures retain semantic codes and actionable explanations through the interface boundary.

Managed administrative failures carry an operation name, stage, correlation ID, semantic error code, safe message, retry guidance, intervention guidance, and authorized-diagnostic availability. The ordinary response remains sanitized; the underlying exception chain and stack evidence are redacted and preserved in the service's authorized diagnostic store. If that store is unavailable, a service may use a protected physical fallback. Failure of either diagnostic sink never replaces the original operation failure or makes it appear successful.

Verbose development visibility is opt-in and changes only how much redacted evidence an authorized operator sees. It never disables redaction, weakens transaction behavior, or changes success into failure or failure into success.

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
- [Managed Operations](MANAGED_OPERATIONS.md)
- [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md)
- [Community Feedback and Diagnostics](../support/COMMUNITY_FEEDBACK_AND_DIAGNOSTICS.md)
- [Running Eternal Cycle](RUNNING_ETERNAL_CYCLE.md)
