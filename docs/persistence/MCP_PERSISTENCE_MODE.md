# MCP Managed Service Interface

## Purpose

This document defines MCP as the supplied reference interface for the storage-neutral [Managed Data Service](MANAGED_DATA_SERVICE.md) contract. MCP is not a universal Persistence Strategy and does not require Microsoft SQL Server.

## Document Control

- **Owner:** MCP transport/tool realization, semantic request boundary, Persistence Receipt, hidden backend, and client-visible failure behavior
- **Primary authority:** [Managed Data Service](MANAGED_DATA_SERVICE.md)
- **Dependencies:** Campaign ID, service authorization, stable transactions, Logical Data Namespace routing, and validated service evidence
- **Extensions:** other MCP implementations may use any compliant backend; other Managed interfaces may replace MCP entirely
- **Consumers:** MCP-capable runtimes and the optional [.NET / MCP / T-SQL reference tooling](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md)
- **Repository boundary:** no live endpoint, credential, connection string, private namespace mapping, or campaign binding belongs here

## Semantic Interface

An MCP realization exposes domain operations rather than arbitrary backend access. The supplied reference tool family includes capabilities equivalent to:

- service capabilities and sanitized status;
- campaign persistence status;
- exact canonical record reads;
- complete owner-routed commit;
- idempotent retry;
- bounded published Rule Packet retrieval;
- sanitized diagnostics.

Administrative source synchronization, compilation, publication, activation, migration, backup, and recovery are separately authorized and need not be exposed through the gameplay MCP server.

The interface exposes no raw SQL, table browser, filesystem shell, unrestricted query, connection string, database path, token, or backup locator.

## Receipt Boundary

Transport success is not persistence success. A state-changing turn completes only when the service returns a validated Persistence Receipt for the expected Campaign ID, Transaction ID, Campaign Version, and validation evidence.

`💾` represents that service evidence. `⏳` represents incomplete service work. `⚠️` represents service, transaction, durability, or validation failure. MCP clients do not display `☁️💾` for hidden server topology.

## Backend Independence

An MCP implementation may use SQL Server, PostgreSQL, a document store, a key/value store, a graph store, indexed files, or another compliant structured implementation. The interface does not expose or require the choice.

The supplied reference service happens to use .NET, MCP, T-SQL, and Microsoft SQL Server. Its SQL schemas and identifiers are implementation details mapped from Logical Data Namespaces.

## Rules

Normal Managed gameplay calls semantic rule-context retrieval and receives an already-published, provenance-bearing Rule Packet. The AI does not ask the MCP service to crawl or compile the repository during play. Rule acquisition, compilation, validation, publication, activation, and update checks belong to the Managed service's administrative pipeline.

## Retry and Failure

Retry uses the existing transaction identity and resumes the earliest incomplete stage. It does not replay narration or duplicate gameplay effects. A missing or failed service does not authorize Direct fallback. Source-update failure may leave update status degraded while the last valid Rule Release remains available.

## Security

- Authenticate and authorize service sessions outside gameplay text.
- Scope every operation by Campaign ID and authorized Logical Data Namespace.
- Keep gameplay and administrative capabilities separate.
- Return sanitized errors and diagnostics.
- Never accept player-supplied backend identifiers or raw operations.
- Preserve GM Secret and least-necessary read boundaries.

## Compatibility

Legacy campaign configuration with Persistence Mode `MCP` maps to Strategy `MANAGED` and Interface `MCP` through an explicit metadata migration. Campaign state and transaction history do not change.

## Related Documents

- [Managed Data Service](MANAGED_DATA_SERVICE.md)
- [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- [Logical Data Namespace](LOGICAL_DATA_NAMESPACE.md)
- [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md)
- [Reference Tooling](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md)
