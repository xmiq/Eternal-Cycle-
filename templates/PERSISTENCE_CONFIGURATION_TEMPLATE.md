# Persistence Configuration Template

Use this blank template to select one portable campaign Persistence Mode without embedding deployment secrets in universal rules or AI profiles.

A populated configuration belongs outside the repository.

## Document Control

- **Template owner:** [Portable Persistence Architecture](../docs/persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- **Dependencies:** Campaign identity, Save Index, authorized runtime, adapter or service contract, backup policy, and access control
- **Extensions:** deployment-specific secret stores, operator contacts, and rotation procedures
- **Consumers:** session start, persistence target resolution, save status, migration, validation, and recovery
- **Repository boundary:** no campaign identifier, locator, connection string, token, credential, or live service address belongs here

## Required Common Fields

- **Configuration ID:** `<stable external identity>`
- **Campaign ID:** `<stable campaign identity>`
- **Persistence Mode:** `<DIRECT | MCP>`
- **Repository Version:** `<rules revision>`
- **Persistence Model Version:** `<logical model version>`
- **Authorized runtime profile:** `<profile and version>`
- **Canonical authority statement:** `<one explicit authority>`
- **Validation policy:** `<required profile>`
- **Recovery policy reference:** `<external policy>`
- **Status:** `<active | migrating | blocked | retired>`

## DIRECT Fields

- **Database Format Adapter:** `<SQLite | DuckDB | future approved adapter>`
- **Database Format Adapter Version:** `<version>`
- **Storage Adapter Chain:** `<ordered local/remote adapters>`
- **Exact canonical artifact locator:** `<secret-managed reference>`
- **Concurrency policy:** `<writer and stale-parent policy>`
- **Required backup stages:** `<policy>`
- **Required read-back evidence:** `<hash, exact comparison, semantic validation>`

Leave this section not applicable in MCP mode.

## MCP Fields

- **MCP service identity:** `<authorized service reference>`
- **MCP service contract version:** `<version>`
- **Authorized MCP interface profile:** `<tool and visibility scope>`
- **Campaign binding:** `<secret-managed service mapping>`
- **Required Persistence Receipt fields:** `<receipt policy>`

Do not include database format, server topology, connection string, local/cloud classification, or backup locator. The service owns those concerns.

## Validation Notes

- [ ] Exactly one Persistence Mode is active.
- [ ] Fields for the inactive mode are absent or explicitly not applicable.
- [ ] Canonical authority is explicit and not inferred from tool availability.
- [ ] DIRECT configuration selects one Database Format Adapter and a complete Storage Adapter Chain.
- [ ] MCP configuration selects one service contract and contains no backend details.
- [ ] Credentials and locators are external protected references.
- [ ] Mode change requires Migration and Versioning rather than silent configuration replacement.
- [ ] Save status can be derived from actual adapter evidence or a validated MCP Persistence Receipt.

## Cross-References

- [Save Index Template](SAVE_INDEX_TEMPLATE.md)
- [Migration Manifest Template](MIGRATION_MANIFEST_TEMPLATE.md)
- [Validation Report Template](VALIDATION_REPORT_TEMPLATE.md)
