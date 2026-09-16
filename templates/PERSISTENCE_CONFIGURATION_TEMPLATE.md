# Persistence Configuration Template

Use this blank template to persist one portable campaign strategy after capability enumeration without embedding deployment secrets in universal rules or AI profiles.

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
- **Persistence Strategy:** `<DIRECT | MANAGED>`
- **Strategy selected at:** `<time and authorized selection>`
- **Selection capability evidence:** `<capability snapshot reference>`
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

Leave this section not applicable in MANAGED.

## MANAGED Fields

- **Managed service identity:** `<authorized service reference>`
- **Managed service contract version:** `<version>`
- **Service interface:** `<MCP | other compliant interface>`
- **Authorized interface profile:** `<operation and visibility scope>`
- **Campaign binding:** `<secret-managed service mapping>`
- **Logical Data Namespace ID:** `<stable logical namespace>`
- **World/Ruleset/Domain Model:** `<stable model identity>`
- **RuleSet and compatible version policy:** `<stable RuleSet, major/range policy, or exact policy>`
- **Pinned Rule Release:** `<published release ID or not pinned>`
- **Required Persistence Receipt fields:** `<receipt policy>`

Do not include database format, server topology, connection string, local/cloud classification, or backup locator. The service owns those concerns.

## Validation Notes

- [ ] Available compliant strategies were enumerated at initialization.
- [ ] Exactly one Persistence Strategy is active and persisted.
- [ ] Resume reuses the persisted strategy before considering environmental preference.
- [ ] Fields for the inactive mode are absent or explicitly not applicable.
- [ ] Canonical authority is explicit and not inferred from tool availability.
- [ ] DIRECT configuration selects one Database Format Adapter and a complete Storage Adapter Chain.
- [ ] MANAGED configuration selects one service contract/interface and contains no backend details.
- [ ] Credentials and locators are external protected references.
- [ ] Strategy change requires Migration and Versioning rather than silent configuration replacement.
- [ ] Save status can be derived from actual adapter evidence or validated Managed service completion evidence.

## Cross-References

- [Save Index Template](SAVE_INDEX_TEMPLATE.md)
- [Migration Manifest Template](MIGRATION_MANIFEST_TEMPLATE.md)
- [Validation Report Template](VALIDATION_REPORT_TEMPLATE.md)
