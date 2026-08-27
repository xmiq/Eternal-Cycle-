# Eternal Cycle MCP Persistence Service

This reference service implements the canonical [MCP Persistence Mode](../../docs/persistence/MCP_PERSISTENCE_MODE.md) over Microsoft SQL Server. It is post-v1 infrastructure for FR-017, not a campaign, GM, rules engine, or populated save.

## Responsibilities

The service:

- exposes semantic MCP tools for status, exact record reads, complete commits, and idempotent retry;
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

## Build and Test

```powershell
dotnet build services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/EternalCycle.Persistence.Mcp.csproj
dotnet test services/eternal-cycle-mcp/tests/EternalCycle.Persistence.Mcp.Tests/EternalCycle.Persistence.Mcp.Tests.csproj
```

The service targets .NET 10 and pins the official Model Context Protocol SDK and Microsoft SQL client packages in the project file.

## Provisioning

1. Create an empty Microsoft SQL Server database through an authorized administrative process.
2. Apply [`Schema/001_initial.sql`](src/EternalCycle.Persistence.Mcp/Schema/001_initial.sql).
3. Insert the campaign identity and version metadata into `ec.campaigns` through an authorized migration or import. Gameplay tools intentionally cannot bootstrap a blank campaign.
4. Configure a least-privilege service identity for the `ec` schema.
5. Configure service-owned backup, restore-verification, retention, and recovery-point policy.
6. Supply the connection string outside the repository.

The .NET configuration key is:

```text
EternalCycle:Persistence:ConnectionString
```

For environment-variable configuration, use the platform's .NET hierarchical-key convention. Do not commit the resulting value.

## Run

```powershell
dotnet run --project services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/EternalCycle.Persistence.Mcp.csproj
```

The server uses MCP stdio transport. Logs are written to standard error so standard input/output remain protocol-safe.

## Transaction Model

Candidate record versions are appended under a stable transaction. The active campaign pointer remains on the prior version until candidate validation succeeds. Activation then advances the pointer atomically, and a fresh read verifies it. Only a completed read-back produces `💾` and a Persistence Receipt.

If activation committed but final read-back did not complete, retry resumes the existing transaction. It does not apply record mutations again.

## Backup and Recovery

The service boundary owns backup and recovery. The schema records recovery-point identity and verification state; deployment must connect those records to SQL Server-native, managed-service, or approved external backup automation. Restore remains an authorized Development Context operation and is intentionally absent from ordinary GM tools.

No MCP client composes Google Drive or local-file adapters around this service. Backend deployment choices are private service configuration.

## Current Validation Boundary

The repository unit tests validate request discipline, exact Affected Sets, receipt enforcement, idempotent retry, pending-state blocking, and topology hiding without requiring a populated SQL Server. Structural validation also requires effective reads to exclude unactivated and failed candidates and candidate validation to use exact transaction identity. Deployment acceptance additionally requires applying the schema to a test SQL Server and exercising commit, competing-candidate isolation, conflict, read-back, backup, and recovery procedures under the selected operational policy.
