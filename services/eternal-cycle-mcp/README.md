# Eternal Cycle MCP Persistence Service

This reference service implements the canonical [MCP Persistence Mode](../../docs/persistence/MCP_PERSISTENCE_MODE.md) over Microsoft SQL Server. It is post-v1 infrastructure for FR-017, not a campaign, GM, rules engine, or populated save.

## Responsibilities

The service:

- exposes semantic MCP tools for status, exact record reads, complete commits, and idempotent retry;
- exposes provenance-bearing, world-isolated rule-context retrieval from a derived Repository Canon index;
- verifies stable campaign, owner, record, interaction, and transaction identities;
- stages append-only candidate record versions;
- validates expected record revisions, payloads, Affected Sets, and typed references;
- atomically activates one descendant Campaign Version;
- performs independent read-back before issuing a validated Persistence Receipt;
- owns backend durability, recovery-point policy, and operational audit behind the MCP boundary.

It does not adjudicate gameplay, expose arbitrary SQL, create a blank campaign during play, or reveal connection strings and backend topology through ordinary tool results.

## Tools

| Tool | Purpose |
| --- | --- |
| `ec_persistence_status` | Return evidence-based semantic status for one configured campaign. |
| `ec_read_records` | Read exact owner-domain and stable-record addresses at the active Campaign Version. |
| `ec_commit_changes` | Stage, validate, activate, and read back one complete owner-routed transaction. |
| `ec_retry_persistence` | Resume an existing transaction without replaying gameplay changes. |
| `ec_get_rule_context` | Retrieve the Runtime Rule Kernel and relevant Core, World/Ruleset, module, operation, and topic chunks within the configured normal-play budget. |

## Build and Test

```powershell
dotnet build services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/EternalCycle.Persistence.Mcp.csproj
dotnet test services/eternal-cycle-mcp/tests/EternalCycle.Persistence.Mcp.Tests/EternalCycle.Persistence.Mcp.Tests.csproj
```

The service targets .NET 10 and pins the official Model Context Protocol SDK and Microsoft SQL client packages in the project file.

## Provisioning

1. Create an empty Microsoft SQL Server database through an authorized administrative process.
2. For the standard Eternal Cycle world, apply the default [`Schema/001_initial.sql`](src/EternalCycle.Persistence.Mcp/Schema/001_initial.sql) to schema `ec`. For another approved world/schema binding, render [`001_initial.template.sql`](src/EternalCycle.Persistence.Mcp/Schema/001_initial.template.sql) through `SqlServerSchemaMigration`; never use player- or model-supplied identifier text.
3. Insert each campaign identity and version metadata into the selected schema's `campaigns` table through an authorized migration or import. Gameplay tools intentionally cannot bootstrap a blank campaign.
4. Configure a least-privilege service identity for every selected schema.
5. Configure service-owned backup, restore-verification, retention, and recovery-point policy.
6. Supply the connection string outside the repository.

The .NET configuration key is:

```text
EternalCycle:Persistence:ConnectionString
```

Schema routing is trusted deployment configuration, not MCP input:

```text
EternalCycle:Persistence:DefaultSchema = ec
EternalCycle:Persistence:DefaultWorldModelId = eternal-cycle-standard
EternalCycle:Persistence:DefaultSchemaModelVersion = 1
EternalCycle:Persistence:DefaultRulesetVersion = 1.0.0

EternalCycle:Persistence:CampaignWorldModels:<campaign-id> = <world-model-id>
EternalCycle:Persistence:WorldSchemas:<world-model-id>:SchemaName = <validated-schema>
EternalCycle:Persistence:WorldSchemas:<world-model-id>:SchemaModelVersion = <version>
EternalCycle:Persistence:WorldSchemas:<world-model-id>:RulesetVersion = <version>
```

An unlisted campaign uses the standard default binding for backward compatibility. A campaign mapped to a non-default World Model must have a complete World Schema binding. Multiple compatible campaigns may share one schema and remain isolated by parameterized Campaign ID; different World Models may route to different schemas. Campaign ID, World Model, SQL schema, and SQL database remain distinct identities.

Rule retrieval uses:

```text
EternalCycle:Rules:RepositoryRoot = <trusted repository checkout>
EternalCycle:Rules:ManifestPath = docs/rules/rule-source-manifest.json
EternalCycle:Rules:MaximumEstimatedTokens = 8000
EternalCycle:Rules:DefaultCampaignMode = NORMAL
EternalCycle:Rules:CampaignModes:<campaign-id> = <configured-mode>
EternalCycle:Rules:CampaignOptionalModules:<campaign-id>:<index> = <module-id>
```

The manifest, source paths, Campaign Mode, and optional-module selection are operator-controlled Campaign Configuration. MCP callers provide the operation and topics but cannot activate a mode or module. The compiler rejects paths outside the configured repository root. It emits a Derived index with source path, heading anchor, hash, and Repository Version; the Markdown sources remain authoritative.

For environment-variable configuration, use the platform's .NET hierarchical-key convention. Do not commit the resulting value.

## Run

```powershell
dotnet run --project services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/EternalCycle.Persistence.Mcp.csproj
```

The server uses MCP stdio transport. Logs are written to standard error so standard input/output remain protocol-safe.

## Transaction Model

Candidate record versions are appended under a stable transaction. The active campaign pointer remains on the prior version until candidate validation succeeds. Activation then advances the pointer atomically, and a fresh read verifies it. Only a completed read-back produces `💾` and a Persistence Receipt.

If activation committed but final read-back did not complete, retry resumes the existing transaction. It does not apply record mutations again.

Every application query resolves the Campaign ID through trusted World Model configuration, validates and quotes the resulting schema identifier, and continues to parameterize the Campaign ID and all data values. MCP callers never submit schema names or SQL. The default `ec` deployment remains compatible, but `ec` is not a universal requirement.

Schema/model migrations are rendered and applied to one authorized World Model/schema binding at a time. A standard-world migration must not scan or modify unrelated custom schemas. Campaigns sharing a schema necessarily share that schema's structural contract.

## Backup and Recovery

The service boundary owns backup and recovery. The schema records recovery-point identity and verification state; deployment must connect those records to SQL Server-native, managed-service, or approved external backup automation. Restore remains an authorized Development Context operation and is intentionally absent from ordinary GM tools.

No MCP client composes Google Drive or local-file adapters around this service. Backend deployment choices are private service configuration.

## Current Validation Boundary

The repository unit tests validate request discipline, exact Affected Sets, receipt enforcement, idempotent retry, pending-state blocking, topology hiding, default and configured schema routing, shared-schema and cross-schema isolation, migration rendering, identifier rejection, rule provenance, World isolation, and the 8K rule-context target without requiring a populated SQL Server. Structural validation also requires effective reads to exclude unactivated and failed candidates and candidate validation to use exact transaction identity. Deployment acceptance additionally requires applying each selected schema to a test SQL Server and exercising commit, competing-candidate isolation, conflict, read-back, backup, recovery, and schema-scoped migration procedures under the selected operational policy.
