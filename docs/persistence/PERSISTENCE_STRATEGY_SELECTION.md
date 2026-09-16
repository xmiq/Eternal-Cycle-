# Persistence Strategy Selection

## Purpose

This document defines how a campaign selects and retains one canonical persistence strategy without binding Eternal Cycle to a runtime, protocol, or datastore.

## Document Control

- **Owner:** capability enumeration, strategy selection, persisted strategy identity, resume behavior, and strategy migration boundary
- **Primary authorities:** [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md), [Migration and Versioning](MIGRATION_AND_VERSIONING.md), and [Campaign Bootstrap](../gm/CAMPAIGN_BOOTSTRAP.md)
- **Consumers:** campaign bootstrap, session start, Campaign Configuration, Save Index, human and AI GMs, and migration tooling
- **Repository boundary:** no populated campaign identifier, service locator, credential, or live capability result belongs here

## Core Procedure

Persistence follows:

```text
Enumerate -> Select -> Persist -> Reuse
```

1. **Enumerate:** inspect the runtime's actually available compliant `DIRECT` and `MANAGED` capabilities.
2. **Select:** choose one viable strategy under campaign authority. Preference cannot make an unavailable or non-compliant strategy viable.
3. **Persist:** record the selected strategy, contract version, canonical authority, and reconnect information in protected Campaign Configuration and reference it from the Save Index.
4. **Reuse:** every later session loads that selection before considering environmental capabilities.

Existing Campaign Configuration outranks a newly convenient adapter or service. A runtime must not re-run preference selection on every resume, silently switch from `DIRECT` to `MANAGED`, or create a parallel authority when the configured strategy is temporarily unavailable.

## Strategy Requirements

### DIRECT

Select `DIRECT` only when the runtime can operate one compliant structured Database Format Adapter and the complete Storage Adapter Chain required by the canonical authority. The runtime owns adapter orchestration, transaction evidence, deployment, read-back, and configured recovery behavior.

### MANAGED

Select `MANAGED` only when an authorized Managed Data Service satisfies the required campaign, turn-persistence, rules, status, validation, and reconnect capabilities. The service becomes the canonical persistence authority. Client-side SQLite, DuckDB, local files, or cloud replicas do not become competing canonical saves.

MCP may carry the Managed contract, but protocol support alone is not sufficient. The selected service must report the required semantic capabilities and evidence.

## Resume

On resume:

1. load protected Campaign Configuration;
2. identify the persisted strategy and implementation binding;
3. verify that binding and its contract version;
4. reconnect to the selected authority;
5. stop in recovery or migration state if it cannot be resolved.

Do not choose another available strategy merely because reconnecting failed. Missing local files do not imply a missing Managed campaign, and a missing service does not authorize a new Direct save.

## Strategy Change

Changing `DIRECT` to `MANAGED`, `MANAGED` to `DIRECT`, or replacing the canonical implementation within either strategy is an explicit [migration](MIGRATION_AND_VERSIONING.md). It requires backup, source audit, controlled transfer, validation, activation, rollback planning, and provenance. Successful migration retires the prior authority; it does not leave both writable.

## Legacy Compatibility

- Release 1 SQLite and Google Drive campaigns remain `DIRECT` and require no conversion.
- A post-v1 configuration recorded as `MCP` maps to `MANAGED` with `MCP` as its service interface during migration. Its campaign identity and history remain unchanged.
- Existing T-SQL campaigns remain Managed reference deployments and retain their configured physical schema until an authorized namespace migration occurs.

## Validation

- Exactly one strategy is active.
- The selected strategy was available and compliant when selected.
- The selection is persisted and reused on resume.
- Inactive-strategy fields do not act as fallback authority.
- A strategy change has migration evidence.
- Managed mode has no competing Direct canonical save.

## Related Documents

- [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- [Direct Persistence Mode](DIRECT_PERSISTENCE_MODE.md)
- [Managed Data Service](MANAGED_DATA_SERVICE.md)
- [Logical Data Namespace](LOGICAL_DATA_NAMESPACE.md)
- [Persistence Configuration Template](../../templates/PERSISTENCE_CONFIGURATION_TEMPLATE.md)
- [Save Index Template](../../templates/SAVE_INDEX_TEMPLATE.md)
