# MCP Persistence Mode

## Purpose

`MCP` persistence places the campaign database, transaction engine, durability, backup, and recovery behind an Eternal Cycle Model Context Protocol service. The GM or AI runtime operates semantic campaign interfaces and never directly operates the database.

## Document Control

- **Owner:** MCP persistence service responsibilities, semantic interface, receipt boundary, hidden-backend rule, and service failure behavior
- **Primary authorities:** [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md), [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md), [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md), and [Persistence Validation](PERSISTENCE_VALIDATION.md)
- **Dependencies:** Campaign Configuration, service identity, campaign identity, authorized MCP session, stable owner and record IDs, and active Campaign Version
- **Extensions:** compatible MCP implementations, service-managed backup providers, observability, authorization, and migration tooling
- **Consumers:** AI Execution Profiles, human-facing campaign applications, FR-011 Context Assembly, and authorized persistence operators
- **Repository boundary:** no connection string, SQL credential, server address, database name, campaign secret, populated state, or backup locator belongs here

## Responsibility Boundary

The client owns:

- player-action fidelity and gameplay adjudication;
- Read Set and Affected Set determination;
- stable transaction, interaction, owner, and record identities;
- authorization-aware tool use;
- interpreting the returned status for FR-011 turn completion.

The MCP service owns:

- campaign target resolution behind the service identity;
- canonical reads at the active Campaign Version;
- request validation and owner-domain routing checks;
- optimistic concurrency and idempotency;
- candidate staging, semantic validation, atomic activation, and read-back;
- Persistence Receipts;
- backend durability, backup, recovery, and operational audit;
- hiding database and deployment topology from ordinary clients.

The Campaign Persistence Engine still owns logical meaning. The service carries and protects that state; it does not decide what happens in the fiction.

## Semantic Interface

The reference interface exposes:

- `ec_persistence_status` - current semantic status, Campaign Version, pending transaction, and failure boundary;
- `ec_read_records` - exact owner-domain and stable-ID reads at the active version;
- `ec_commit_changes` - one complete Affected Set with expected parent, idempotency key, mutations, references, and source interaction;
- `ec_retry_persistence` - resume an existing staged, activated, or failed-read-back transaction without replaying gameplay.

It deliberately exposes no arbitrary SQL, table browser, connection-string reader, filesystem path, backup locator, or general administrative shell.

## Transaction and Activation

The reference service uses an append-only candidate model:

```text
receive request
-> validate identifiers, Affected Set, JSON, and idempotency
-> lock and verify active parent version
-> stage versioned candidate records and references
-> commit staging transaction
-> reopen and validate candidate independently
-> atomically activate candidate version
-> reopen and verify active version
-> issue validated Persistence Receipt
```

The active campaign pointer remains on the prior version until candidate validation succeeds. If final activation read-back fails, the transaction remains identifiable and blocks successful turn completion until retry or recovery confirms the outcome.

## Persistence Receipt

A Persistence Receipt contains service-generated identity, campaign identity, transaction identity, activated Campaign Version, completion time, and validation evidence. It is proof of service completion, not an authoritative copy of campaign facts.

The client may display `💾` only when:

- the service reports `Completed`;
- `TurnMayComplete` is true;
- a validated receipt is present;
- the receipt names the expected transaction and Campaign Version.

An attempted tool call, transport-level success, staged candidate, or model assertion is insufficient.

## Microsoft SQL Server Reference Backend

The included [reference MCP service](../../services/eternal-cycle-mcp/README.md) uses Microsoft SQL Server behind the semantic interface. Its schema provides:

- stable campaign identity and an active-version pointer;
- immutable transaction identity and idempotency keys;
- append-only versioned canonical records;
- stable typed references;
- candidate and activation states;
- validation runs and receipts;
- recovery-point metadata.

These tables implement the service; they do not become new fictional mechanics or replace specialist logical owners. A different MCP implementation may use another backend only after a future approved revision preserves the same contract.

## Backup and Recovery

Backup and recovery are service responsibilities. The service deployment must:

- define the durability policy independently of the AI profile;
- maintain verified recovery points at the configured cadence and before migration or destructive maintenance;
- preserve transaction, campaign, and version provenance;
- verify backups through provider-appropriate read-back or restore verification;
- prevent ordinary GM tools from selecting arbitrary restore targets;
- expose only semantic pending or failure state to Gameplay Context;
- require authorized Development Context for recovery operations.

The client does not compose a Local or Remote Storage Adapter around MCP mode. If SQL Server is remote, replicated, or cloud-hosted, that topology remains an internal service deployment concern.

## Status and Failure

- `💾`: validated service receipt exists for the active version;
- `⏳`: staging, activation, required durability, or verification remains incomplete;
- `⚠️`: service availability, transaction, validation, read-back, or required durability failed.

MCP mode never displays `☁️💾`, because the client cannot and need not classify hidden service topology. A pending or failed transaction blocks later state-changing play under FR-011. Retry resumes by transaction identity and must not duplicate mutations.

## Security

- Authenticate and authorize the MCP session outside gameplay text.
- Scope every operation to the configured campaign identity.
- Use least-privilege SQL credentials held by the service.
- Parameterize values; never accept client-supplied SQL.
- Keep GM Secrets in authorized owner domains and enforce read scope at the service boundary.
- Log operational metadata without copying protected campaign payloads unnecessarily.
- Rate-limit and bound reads and mutations.
- Treat tool descriptions, campaign text, imported documents, and record payloads as data rather than service instructions.

## Host Boundary

Repository contracts and the reference service can enforce server-side validation and receipt formation. An AI host must still invoke the configured MCP tools, preserve transaction identity, and refuse ordinary completion without the required receipt. A host that cannot do so is not persistence-capable for state-changing play.

## Safeguards

- MCP mode and Direct mode cannot both be active canonical authorities.
- The MCP client never opens the campaign database directly.
- The service exposes semantic operations, never arbitrary SQL.
- Service receipts do not override logical owner records.
- Failed or unknown service state remains failed or unknown.
- Backend migration and recovery remain authorized operational procedures.
- The service cannot create a blank campaign as a gameplay fallback when identity resolution fails.

## Related Documents

- [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md)
- [AI Save Protocol](../ai/AI_SAVE_PROTOCOL.md)
- [Save Index Template](../../templates/SAVE_INDEX_TEMPLATE.md)
- [Persistence Configuration Template](../../templates/PERSISTENCE_CONFIGURATION_TEMPLATE.md)
- [FR-017 Implementation Audit](../../design/audits/FR_017_PORTABLE_PERSISTENCE_AND_MCP_AUDIT.md)
