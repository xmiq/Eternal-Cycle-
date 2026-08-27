# AI Save Protocol

## Purpose

This procedure explains how an AI operator invokes and reports the canonical [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md). The persistence document remains the mechanical and procedural owner. This file adds no save format, database design, automatic merge rule, or permission to alter campaign state.

Under [FR-011](CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md), this procedure is invoked automatically after every resolved Gameplay Interaction. The player does not need to request saving. A non-empty Affected Set must commit, validate, and satisfy configured read-back requirements before the turn closes or its durable result is finally delivered.

## Document Control

- **Owner:** this document owns the AI-facing save-operation sequence and write-capability disclosure
- **Primary authorities:** [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md), [Persistence Validation](../persistence/PERSISTENCE_VALIDATION.md), and [Persistence Authority](../persistence/PERSISTENCE_AUTHORITY.md)
- **Dependencies:** completed Gameplay Interaction, parent Campaign Version, Save Index, Affected Set, owner-routed Session Delta, validation, and activation authority
- **Extensions:** Direct Adapters, MCP persistence services, transaction tools, diff views, approval interfaces, and recovery automation
- **Consumers:** AI GM workflow, campaign storage interfaces, supervising GMs, validation tools, and handoff processes
- **Repository boundary:** no Transaction, save candidate, campaign delta, backup, validation result, current value, or storage credential belongs here

## Capability Declaration

Before saving, the AI operator determines which mode applies:

| Mode | Meaning | Required behavior |
| --- | --- | --- |
| **Authorized writer** | the operator can stage, validate, and request or perform activation through an approved campaign interface | follow the full transaction and verify the read-back |
| **Proposal only** | the operator can prepare a bounded candidate but cannot write or activate it | present the proposed delta and state explicitly that it is not saved |
| **Read only** | the operator can inspect but cannot stage campaign changes | report the affected records and required update without claiming persistence |
| **Unavailable** | the Campaign Record or save interface cannot be reached | preserve the interaction boundary externally if authorized, mark Save Recovery Required, and stop dependent play |

A generated file, message, or summary is not an activated Save Point unless the authorized persistence interface confirms it.

Under FR-011, `Proposal only`, `Read only`, and `Unavailable` cannot close a state-changing Gameplay Turn as committed. They preserve the bounded result or recovery evidence and surface the capability failure; ordinary dependent play resumes only from the last validated Save Point or after an authorized writer completes and verifies the transaction.

## Persistence Target Readiness

Before state-changing play or save execution:

1. read Campaign Configuration and the Save Index;
2. determine whether Persistence Mode is `DIRECT` or `MCP`;
3. in `DIRECT`, resolve the exact target identity and complete Database Format and Storage Adapter Chain;
4. in `MCP`, resolve the configured service identity, campaign binding, contract version, and semantic status;
5. verify campaign identity, Campaign Version, Save Point, concurrency evidence, and open Transaction state;
6. classify any Local Working Copy as canonical, candidate, cache, or stale according to configuration.

Do not declare the campaign database missing from one failed local-path lookup. Do not create a blank replacement while a configured Direct remote authority or MCP campaign binding may exist. In MCP mode, the client does not search for, open, or classify a backend database.

## Save Operation

### 1. Confirm the semantic boundary

Verify that one Gameplay Interaction has established intent, adjudication, immediate outcome, costs, information effects, and currently resolvable consequences. Do not save unresolved draft narration as established state.

### 2. Re-read the parent

Load the active Save Index and confirm:

- parent Campaign Version and Save Point;
- active Repository Version and Rules Profile;
- open Current Session state;
- prior Pending Session Deltas;
- migration, warning, conflict, and recovery status.

Do not rely on a cached parent identifier.

### 3. Assign transaction identity

Use one unique Transaction ID for the interaction, Session Delta, Write Set, validation, activation, and recovery. A retry reuses that identity and must be idempotent.

### 4. Determine the Affected Set

Start with direct established effects, then follow only material dependency closure. Identify every changed claim's Authoritative Record Owner, canonical mechanic, source event, effective time, Truth Layer, Persistence Level, and prior state.

Check as applicable:

- body, Soul, Development, Skills, Classes, Evolution, Soul Weapons, and Magic;
- Inventory quantity, condition, location, custody, claims, and containers;
- Relationships, recognition, promises, debts, and participant views;
- Species, Locations, Factions, Infrastructure, and world state;
- Projects, Research, Mysteries, Knowledge, and Secrets;
- Session Log, Timeline, Campaign History, Pending Consequences, and Review Points.

Do not rewrite unrelated records for formatting, synchronization, or apparent completeness.

A completed long-horizon simulation includes changed specialist owners, required chronology, and its [Long-Horizon Summary](../persistence/LONG_HORIZON_SUMMARIES.md) in one dependency-complete Affected Set. The summary does not replace those owner writes. This follows current Save Update rules and does not implement FR-011 enforcement.

### 5. Build the Session Delta

For each operation, record the exact prior claim or explicit unknown, justified change, resulting claim, owner, source, time, visibility, and dependencies. Preserve numerical provenance through the owning mechanic. Never infer missing numbers, round for convenience, rebalance silently, or award progression from bookkeeping.

Classify No-Op interactions honestly. A No-Op creates no fake event, cost, evidence, progression, or world change, though a material interaction may still receive ordered Session logging.

### 6. Append chronology and history correctly

- Append Session order for every completed Gameplay Interaction.
- Append Timeline for material in-world events.
- Append Campaign History for materially durable continuity.

These records are distinct. One append does not substitute for another.

### 7. Stage without activation

Apply the Write Set to a candidate descended from the confirmed parent. In `DIRECT`, stage through the Database Format Adapter. In `MCP`, submit the semantic mutation request with the stable Transaction ID and idempotency key. Keep the parent Save Point active until the selected implementation validates and activates the candidate. Preserve all-or-nothing scope and do not expose partially integrated state as authoritative.

### 8. Validate the candidate

Run the required Save Activation profile against a frozen baseline. Verify at minimum:

- authority, versions, ownership, and references;
- Timeline, Campaign History, and Session order;
- Relationships and identity continuity;
- Truth Layers, Knowledge, Research, and Secret separation;
- numerical and mechanical provenance;
- Inventory, Infrastructure, Projects, Mysteries, and world handoffs;
- no duplicate, dangling, orphaned, or silently reset records;
- no write outside the Affected Set.

Validation is read-only. Repairs create a new candidate through the proper owners and require revalidation.

### 9. Activate or preserve the parent

Before activation, compare the candidate's parent version with the active Campaign Version. If unchanged and validation permits activation, activate the complete candidate atomically and update the Save Index. Then read the active Save Index and changed records back to confirm success.

If a Direct authority is cloud, local candidate validation does not activate the Save Point. Activation follows required remote replacement, synchronization, refetch, comparison, semantic verification, and required backup verification. In MCP mode, activation requires a validated Persistence Receipt from the configured service; transport success or a staged candidate is insufficient. If the parent changed, expected state remained unchanged, validation failed, activation was interrupted, or configured-authority read-back cannot confirm success, preserve the last confirmed boundary and enter Save Recovery Required.

### 10. Report the outcome

Report only what is operationally true:

- **Saved:** activation and read-back confirmed, with new Campaign Version and Save Point.
- **Saved with authorized warnings:** activation confirmed and warning dispositions are explicit.
- **Proposed, not saved:** a candidate exists but no authorized activation occurred.
- **Blocked:** a conflict, validation finding, missing source, or authority issue prevents activation.
- **Recovery pending:** interruption or uncertain write status requires idempotent recovery.

Do not say `saved`, `updated`, or `committed` when only narration or a proposed delta exists.

Map the verified outcome to the player-visible marker:

- `💾` only when local is configured canonical authority and commit, validation, expected-change proof, and read-back pass;
- `☁️💾` only when cloud is configured canonical authority and synchronization plus required remote verification pass;
- `💾` in MCP mode only when the configured service returns a validated Persistence Receipt for the expected transaction and active Campaign Version;
- `⏳` while required stages remain incomplete;
- `⚠️` when a required write, synchronization, validation, expected-change check, or read-back fails.

Local success in a Direct cloud-authoritative chain is `⏳`, not `💾`. An upload attempt without verified read-back is never `☁️💾`. MCP mode never uses `☁️💾`; its backend topology is not client authority.

## Manual Commands

### `save`

Use the current pending Affected Set and Transaction ID. Execute the same owner-routed transaction without replaying adjudication. If no change is pending, verify the active configured authority and return its current marker and version; do not create a gameplay event or gratuitous Campaign Version.

### `save status`

Report the current marker, Persistence Mode, Campaign Version, Save Point where authorized, configured canonical authority, pending Affected Set or Transaction, and a brief sanitized failure reason. In Direct mode include relevant local/cloud synchronization and verification state. In MCP mode include the validated receipt or pending transaction identity allowed by policy, but no backend topology, database name, connection detail, or backup locator. Do not disclose private locators, credentials, or GM Secrets.

### `retry save`

Resume the existing Transaction from its earliest incomplete stage after inspecting actual state. Reuse idempotency keys and verify already-applied owner operations. If Direct local changes are valid and only cloud synchronization failed, retry only the cloud and downstream verification stages. In MCP mode, call the service retry operation with the same Transaction ID; never resubmit the gameplay action as a new transaction.

Manual commands do not repeat narration, time, costs, Development, Inventory, Timeline, Campaign History, Relationship, Research, Project, Infrastructure, or Autonomous Registry effects.

## Concurrency and Retry

Never use last-writer-wins. If the active Campaign Version changed, classify the candidate as duplicate, independent, sequential, or conflicting under the Save Update Protocol. Create a valid descendant, use Continuity Resolution, or migrate as required.

Before reporting success for a non-empty Affected Set, prove the expected canonical change through owner values and sufficient version, revision, chronology, Save Index, hash, or exact-comparison evidence. An unchanged canonical target where change was expected is a failed transaction even if a tool returned success.

On retry, reload parent and candidate, preserve the Transaction ID, resume from the last confirmed step, and ensure no event, cost, item, progression change, Relationship change, or history append is duplicated.

## Proposal-Only Handoff

When the operator cannot write, provide a bounded update package to the authorized campaign system containing:

- parent Campaign Version and Save Point;
- Transaction ID;
- completed interaction boundary;
- Affected Set and owner-routed operations;
- required Session, Timeline, and Campaign History appends;
- unknowns, warnings, conflicts, and protected-view requirements;
- validation required before activation.

The package remains a proposal. Dependent play pauses until activation is confirmed or the campaign explicitly adopts another valid recovery route.

## Safeguards

- Saving objective Entity state, Controller assignments, Perspectives, and Entity Knowledge must preserve their separate owners under the [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md).

- Saving never creates a mechanic or repairs a rule gap.
- Conversation context never silently overrides structured state.
- Unrelated records remain untouched.
- Protected information never appears in unauthorized save summaries.
- Interrupted operations never imply partial success.
- The canonical repository never receives populated campaign records.

## Related Documents

- [Living Codex Persistence Model](../gm-living-codex/PERSISTENCE_MODEL.md) - separate reusable-design transactions, validation, deployment, and backup; never fold these writes into campaign state.
- [Lineage and Evolutionary Inheritance](../gm-living-codex/LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md) - reusable profiles use Codex migrations; actual lineage outcomes use campaign Save Transactions.
- [AI Play Protocol](AI_PLAY_PROTOCOL.md)
- [Save Index Template](../../templates/SAVE_INDEX_TEMPLATE.md)
- [Persistence Validation Report Template](../../templates/VALIDATION_REPORT_TEMPLATE.md)
- [Continuity Resolution](../persistence/CONTINUITY_RESOLUTION.md)
- [Portable Persistence Architecture](../persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- [MCP Persistence Mode](../persistence/MCP_PERSISTENCE_MODE.md)
