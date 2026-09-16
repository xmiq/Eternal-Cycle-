# Eternal Cycle Managed Data Service — .NET / MCP / T-SQL Reference

This optional prebuilt tool demonstrates one implementation of the storage-neutral [Managed Data Service](../../../../docs/persistence/MANAGED_DATA_SERVICE.md) contract:

- .NET service runtime;
- MCP gameplay interface;
- Microsoft SQL Server / T-SQL storage adapter;
- Git-backed Rule Source Provider;
- `ec_domain` published Rule Store;
- validated campaign transactions and Persistence Receipts.

It is not the Eternal Cycle persistence architecture itself. A compliant Managed service may use another interface or structured datastore. This project is neither a campaign, rules authority, nor gameplay engine.

## Implemented Capabilities

- semantic status, exact record read, complete commit, and idempotent retry tools;
- staged append-only candidates, expected-parent concurrency, validation, activation read-back, and receipts;
- trusted Campaign ID -> World Model -> Logical Data Namespace -> SQL schema routing;
- strict SQL identifier validation and quoting;
- Git commit-SHA source acquisition through an administrative Rule Source Provider;
- versioned rule candidates, validation, publication, atomic activation, and active-release fallback;
- runtime retrieval from an already-published Rule Release rather than repository compilation;
- source provenance, World/Ruleset/module/mode/operation/topic filtering, and an 8K packet ceiling;
- startup, periodic, manual, and offline update policies;
- sanitized capabilities and diagnostic reports.

The reference does not implement PostgreSQL, MySQL, document, graph, key/value, or indexed-file adapters. Those are contract-compatible extension families, not claimed implementations.

## Gameplay MCP Tools

| Tool | Purpose |
| --- | --- |
| `ec_service_capabilities` | Identify this reference implementation and its semantic capability set. |
| `ec_persistence_status` | Return service-backed campaign status. |
| `ec_read_records` | Read exact canonical owner-domain and stable-record addresses. |
| `ec_commit_changes` | Stage, validate, activate, read back, and receipt one complete transaction. |
| `ec_retry_persistence` | Resume a stable transaction without replaying gameplay. |
| `ec_get_rule_context` | Retrieve an already-published bounded Rule Packet. |
| `ec_get_diagnostics` | Return sanitized implementation, rules, source, namespace, and update provenance. |

Source synchronization, compilation, publication, activation, migrations, backup, and recovery are administrative responsibilities. They are intentionally absent from the ordinary gameplay tool surface.

## Build and Test

```powershell
dotnet build examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/EternalCycle.Persistence.Mcp.csproj
dotnet test examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/EternalCycle.Persistence.Mcp.Tests.csproj
```

## T-SQL Namespaces

Apply:

1. [`001_initial.sql`](src/EternalCycle.Persistence.Mcp/Schema/001_initial.sql) for the legacy-compatible `ec` campaign schema, or render [`001_initial.template.sql`](src/EternalCycle.Persistence.Mcp/Schema/001_initial.template.sql) for a trusted World Data Namespace mapping;
2. [`002_rule_domain.sql`](src/EternalCycle.Persistence.Mcp/Schema/002_rule_domain.sql) for the reference `ec_domain` Domain Namespace, or render its template under an approved physical mapping policy.

New deployments should prefer the `ec_` discoverability convention, such as `ec_mainworld` or `ec_fantasyworld`. Existing `ec` deployments remain supported. Physical schema names do not become Campaign IDs, World IDs, RuleSet IDs, or Logical Data Namespace IDs.

`ec_domain` stores only shared service data and Derived rule publications: RuleSets, Rule Releases, compiled chunks, selectors, dependencies, active-release pointers, provenance, and update checks. World-specific Campaign Canon remains in the selected world schema.

## Trusted Configuration

The connection string remains external:

```text
EternalCycle:Persistence:ConnectionString
```

Reference routing supports:

```text
EternalCycle:Persistence:DomainSchema = ec_domain
EternalCycle:Persistence:DefaultDataNamespaceId = eternal-cycle-mainworld
EternalCycle:Persistence:DefaultSchema = ec
EternalCycle:Persistence:DefaultWorldModelId = eternal-cycle-standard
EternalCycle:Persistence:DefaultRulesetId = eternal-cycle-core

EternalCycle:Persistence:CampaignWorldModels:<campaign-id> = <world-model-id>
EternalCycle:Persistence:WorldDataNamespaces:<world-model-id> = <namespace-id>
EternalCycle:Persistence:DataNamespaces:<namespace-id>:SchemaName = <validated-schema>
EternalCycle:Persistence:DataNamespaces:<namespace-id>:SchemaModelVersion = <version>
EternalCycle:Persistence:DataNamespaces:<namespace-id>:RulesetId = <ruleset-id>
EternalCycle:Persistence:DataNamespaces:<namespace-id>:RulesetVersion = <version>
```

The FR-018 `WorldSchemas` mapping remains accepted as a compatibility input. New configuration should use explicit Logical Data Namespace IDs.

Rule publication configuration includes:

```text
EternalCycle:Rules:RulesetId = eternal-cycle-core
EternalCycle:Rules:CompilerVersion = 1
EternalCycle:Rules:MaximumEstimatedTokens = 8000
EternalCycle:Rules:UpdatePolicy = Disabled | Startup | Periodic | Manual
EternalCycle:Rules:ActivationPolicy = Automatic | Manual
EternalCycle:Rules:CampaignPinnedRuleReleaseIds:<campaign-id> = <published-release-id>
EternalCycle:Rules:GitSource:RepositoryRoot = <trusted Git checkout>
EternalCycle:Rules:GitSource:Ref = <trusted branch, tag, or commit>
EternalCycle:Rules:GitSource:ManifestPath = docs/rules/rule-source-manifest.json
EternalCycle:Rules:GitSource:FetchBeforeCheck = false
```

The provider resolves the ref to an immutable commit SHA and reads the manifest and sources at that commit. A failed source check or candidate preserves the active validated Rule Release. `Disabled` permits offline serving from the existing published store. Automatic checks never require an AI gameplay request.

## Campaign Transactions

Candidate campaign records remain inactive until the complete Affected Set validates and atomically advances the campaign's active version. A validated Persistence Receipt is the Managed completion evidence. Retry resumes the same transaction and never reapplies gameplay effects.

## Security

- MCP callers never submit SQL, schemas, namespace mappings, source paths, or credentials.
- Campaign ID isolation is enforced independently of namespace routing.
- Configurable identifiers are strictly validated and safely quoted.
- Gameplay tools have no source-publication or database-administration authority.
- Diagnostic output excludes connection strings, credentials, locators, Campaign Canon, GM Secrets, and conversations.

## Validation Boundary

The repository tests use fakes and local fixtures for publication, fallback, activation, filtering, diagnostics, and routing. They do not prove live SQL Server transaction, concurrency, migration, backup/recovery, service restart, Git remote synchronization, authentication, long-running scheduling, or cross-client MCP interoperability. Those remain deployment acceptance requirements.
