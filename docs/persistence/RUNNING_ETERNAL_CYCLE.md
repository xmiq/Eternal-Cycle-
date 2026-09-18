# Running Eternal Cycle

## Purpose

This guide explains how a runtime discovers, selects, initializes, and reuses an Eternal Cycle persistence strategy without making any particular AI host, protocol, source forge, or datastore mandatory.

## Document Control

- **Owner:** runtime-neutral startup, capability discovery, setup handoff, and strategy reuse
- **Dependencies:** [Persistence Strategy Selection](PERSISTENCE_STRATEGY_SELECTION.md), [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md), [Managed Data Service](MANAGED_DATA_SERVICE.md), [Managed Operations](MANAGED_OPERATIONS.md), and [Campaign Bootstrap](../gm/CAMPAIGN_BOOTSTRAP.md)
- **Extensions:** runtime profiles, Direct Adapter instructions, Managed service quick starts, and deployment-specific configuration
- **Consumers:** players, human and AI GMs, runtime integrators, and administrators
- **Repository boundary:** no endpoint, credential, populated Campaign ID, or live state belongs here

## The Startup Rule

Every environment follows:

```text
Enumerate -> Select -> Persist -> Reuse
```

1. **Enumerate** available persistence and rule-delivery capabilities.
2. **Select** `DIRECT` or `MANAGED` using required capabilities and user consent.
3. **Persist** the selected strategy, implementation identity, and non-secret campaign routing in Campaign Configuration.
4. **Reuse** that selection on resume until an explicit validated migration changes it.

SQL Server, MCP, GitHub, ChatGPT, Unsloth, SQLite, DuckDB, and Google Drive are implementations or hosts, not universal requirements.

## Managed Startup

A Managed client first asks the selected service for configuration requirements, structured capabilities, and readiness. Configuration discovery requires no campaign or datastore connection and reports exact safe setting names without echoing credentials. A healthy transport or datastore connection alone does not mean gameplay is ready.

The service may report:

| State | Meaning | Normal response |
| --- | --- | --- |
| `READY` | Persistence and the minimum authoritative closure required for the requested play are ready. | Start or resume play; remaining rules may continue preparation. |
| `SETUP_REQUIRED` | EC-owned persistence or rule structures are missing. | Explain the bounded setup plan and request approval. |
| `MIGRATION_REQUIRED` | Existing EC-owned structures require a supported upgrade or administrator review. | Stop state-changing play and migrate. |
| `RULE_SOURCE_REQUIRED` | No Rule Source has been selected. | Offer the packaged default and compatible user-selected alternatives. |
| `RULE_PUBLICATION_REQUIRED` | A source exists but no validated release is published. | Request approval, create or reuse a durable publication operation, and return its ID promptly. |
| `RULE_ACTIVATION_REQUIRED` | A release is published but not active. | Follow the configured activation policy. |
| `RULE_PREPARATION_PENDING` | An active release exists but the minimum or requested authoritative closure is still being prepared. | Query operation or context status; never guess the missing rule. |
| `CAMPAIGN_REQUIRED` | The requested campaign is absent. | Discover campaigns or offer authorized creation. |
| `DEGRADED` | Gameplay can use the last active release, but a noncritical source/update facility is unavailable. | Continue with a clear diagnostic state. |
| `ERROR` | A blocking connection, compatibility, or validation failure exists. | Stop dependent play and surface a sanitized remedy. |

Administrative initialization is never inferred from connection success. The client previews the exact EC-owned scope, explains it in ordinary language, obtains explicit approval, and invokes a separately authorized setup capability. Repeated setup must be idempotent or safely report that no migration is needed.

If the connected database predates the latest service migration, the base control plane stays usable. Readiness reports `MIGRATION_REQUIRED`; diagnostics avoid missing post-migration tables; operation tools return the migration recovery route; and an authorized operator can preview and apply the next bounded service-owned migration. The client does not need raw SQL, filesystem access, a special approval phrase, or a Campaign ID.

If no Rule Source is configured, the service may offer the official Eternal Cycle repository described by authoritative distribution metadata. A user may instead authorize another compatible source or an offline local checkout. The successful choice is persisted and reused. The service, not the AI GM, acquires and compiles that source.

Readiness is granular: `ServiceReady`, `PersistenceReady`, `RuleKernelReady`, `CampaignBootstrapReady`, `GameplayReady`, and `FullRulesetReady` are distinct claims. `GameplayReady` may become true before `FullRulesetReady`; a later action still waits when its dependency closure is `PENDING` or `FAILED`.

Initial publication is a durable [Managed Operation](MANAGED_OPERATIONS.md). The initiating client may disconnect after receiving the Operation ID. On reconnect it reads status or recent operations; it does not restart publication blindly. Service diagnostics require no Campaign ID, while an optional Campaign ID adds campaign-specific routing and readiness.

When startup or administration fails, the client preserves the semantic error code and follows its advertised recovery capability. Static error lookup and bounded Error Dumps remain available before migration. Preferred structured diagnostics use an automatic protected fallback when their primary sink is unavailable. Support Bundle creation and external report submission are later, optional, explicitly authorized actions.

## Direct Startup

A Direct runtime enumerates supported Database Format and Storage Adapters, selects a compatible Adapter Chain, validates authority and durability, writes the selection to Campaign Configuration, and reuses it on resume. Direct mode does not require a Managed service or MCP. Its runtime remains responsible for transaction, validation, read-back, and configured local or cloud completion evidence.

## Starting and Resuming

The normal player intents are:

> Start a new Eternal Cycle game.

> Continue my Eternal Cycle game.

The runtime keeps source locators, schema names, Campaign IDs, migration files, and tool names backstage. With no campaigns, it offers authorized creation. With one suitable campaign, it resumes it without asking the player to type an internal ID. With several, it presents meaningful names and descriptions while retaining stable IDs for routing and isolation.

## Capability Gaps

When nothing is configured, an AI or human operator must inspect actual capabilities instead of guessing. Missing information remains missing. A runtime does not create a blank replacement save, invent a Rule Source, silently switch persistence strategies, or claim readiness from conversation memory.

## Related Documents

- [Player Start and Resume](../gm/PLAYER_START_AND_RESUME.md)
- [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md)
- [Managed Operations](MANAGED_OPERATIONS.md)
- [Direct Persistence Mode](DIRECT_PERSISTENCE_MODE.md)
- [AI Session Start](../ai/AI_SESSION_START.md)
- [Errors and Portable Diagnostics](../support/ERRORS_AND_PORTABLE_DIAGNOSTICS.md)
