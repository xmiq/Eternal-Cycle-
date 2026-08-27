# AI Runtime Model

## Purpose

This document defines the implementation-neutral architecture through which an AI Game Master operates Eternal Cycle. It connects the player, AI operator, execution profile, persistence implementation, canonical Campaign State, and rules repository without transferring authority among them.

The model is operational documentation. It does not define fictional mechanics, create campaign facts, prescribe a storage product, or replace the [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md) or [Campaign Persistence Engine](../persistence/README.md).

## Document Control

- **Owner:** this document owns AI runtime layer boundaries, boot flow, transaction flow, composition rules, and extension rules
- **Primary authorities:** [Game Master Responsibilities](../gm/GM_RESPONSIBILITIES.md), [Campaign Persistence Integration](../persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md), and [Persistence Authority](../persistence/PERSISTENCE_AUTHORITY.md)
- **Dependencies:** Repository Canon, Campaign Canon, Canonical Campaign State, active Campaign Configuration, AI capability disclosure, and applicable persistence validation
- **Extensions:** AI Execution Profiles, Direct Persistence Adapters, MCP persistence services, orchestration software, campaign interfaces, and deployment-specific configuration
- **Consumers:** AI GMs, supervising human GMs, runtime implementers, campaign custodians, persistence operators, and validation tools
- **Repository boundary:** no campaign identifier, save locator, credential, current character, live world state, transcript, or other populated campaign fact belongs here

## Core Rule

The AI GM operates through Eternal Cycle rules and Canonical Campaign State. It does not own either one.

The Campaign Persistence Engine defines what campaign information must persist, how authority and truth remain distinct, and what validation means. Campaign Configuration selects `DIRECT` or `MCP` persistence. Direct Adapters or an MCP persistence service define how the selected implementation fetches, writes, verifies, backs up, and recovers that information. No implementation component can outrank, reinterpret, or adjudicate the logical state it carries.

Conversation context, summaries, transcripts, caches, and model memory may help locate evidence. They never silently replace Canonical Campaign State.

Current release/version metadata determines Engine Status. An AI runtime must not infer that Eternal Cycle or a campaign is Alpha, a playtest, or experimental from historical filenames, archived audits, legacy phase names, or the availability of Provisional Rulings.

## Runtime Layers

The runtime has seven distinct layers.

| Layer | Responsibility | Must not do |
| --- | --- | --- |
| **1. Eternal Cycle rules** | Define fictional mechanics, canonical meanings, and reusable GM procedures | Store live campaign facts or adopt provider-specific behavior as a game rule |
| **2. Campaign Persistence Engine** | Define continuity, authority, truth layers, history, migration, versioning, validation, and save semantics | Require a particular database, cloud service, file format, or AI provider |
| **3. AI Runtime Model** | Define how an AI operator, profiles, adapters, rules, and campaign state relate | Adjudicate a mechanic or become campaign authority |
| **4. AI Execution Profile** | Define how one AI runtime boots, retrieves, adjudicates, presents, persists, validates, and fails | Redefine Eternal Cycle mechanics or embed campaign-specific configuration |
| **5. Persistence Implementation** | In `DIRECT`, compose Database Format and Storage Adapters; in `MCP`, invoke a semantic persistence service that owns its backend | Decide gameplay outcomes, expose hidden backend authority as fiction, promote theories into truth, or alter logical ownership |
| **6. Campaign Configuration** | Select versions, profile, Persistence Mode, adapters or MCP service, locators, permissions, and runtime metadata for one deployment | Become a universal runtime document or silently own the campaign facts it references |
| **7. Campaign State** | Hold every populated campaign fact under the Campaign Persistence Engine | Enter the canonical rules repository except as blank reusable templates |

These layers form a responsibility chain, not a descending authority ladder. Rules authority and campaign-fact authority remain the separate hierarchies defined by the [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md#rules-hierarchy) and [Persistence Authority](../persistence/PERSISTENCE_AUTHORITY.md).

## Responsibility Flow

```text
Player
  -> AI Game Master
  -> selected AI Execution Profile
  -> selected Persistence Mode
       -> DIRECT: Database Format and Storage Adapter Chain
       -> MCP: semantic MCP persistence service
  -> Campaign Persistence Engine semantics
  -> Canonical Campaign State

AI Game Master
  -> Eternal Cycle rules repository
```

The arrow means "uses or invokes," not "owns." The AI GM uses Repository Canon to adjudicate and uses Campaign State to establish current facts. The player retains authority over deliberate player-character intent. The GM retains only the responsibilities granted by the shared GM framework.

## Authority Boundaries

### Rules

Use the canonical rule-status hierarchy. An execution profile may sequence repository searches and Provisional Rule handling, but it cannot create a higher rule authority.

### Campaign Facts

Use the Campaign Persistence Authority Chain. A later message, newer cache, fluent summary, or adapter timestamp cannot silently overwrite a higher-authority campaign claim.

### Operations

An operational result establishes only what it proves. A successful upload does not prove semantic validity; a valid local transaction does not prove remote deployment; a generated response does not prove persistence. Every stage must satisfy its own owner and required validation.

## New-Campaign Bootstrap Flow

When the user asks to start a new Eternal Cycle game and does not explicitly request testing, the runtime:

1. reads the current `VERSION`, release metadata, root README status, and Roadmap;
2. classifies the engine as Released under current repository metadata;
3. selects Campaign Mode `NORMAL` under [Campaign Bootstrap](../gm/CAMPAIGN_BOOTSTRAP.md);
4. keeps First-Life Mode, Reincarnation setup, and other starting profiles independent from Campaign Mode;
5. resolves Campaign Configuration and creates external canonical state through the configured persistence mode;
6. validates the initial Save Point before entering state-changing Gameplay Context.

`VALIDATION` and `DEVELOPMENT` require explicit campaign authority. A Provisional Rule never changes Campaign Mode.

## Existing-Campaign Boot Flow

Before dependent play, an AI runtime:

1. loads Campaign Configuration through an authorized channel;
2. identifies the selected Repository Version, Rules Profile, Campaign Mode, AI Execution Profile, and Persistence Mode;
3. in `DIRECT`, resolves the complete adapter chain and asks the canonical Storage Adapter for the latest identified source;
4. in `MCP`, resolves the configured service and campaign binding, then requests semantic status and canonical records without opening a database;
5. reads the Save Index, active Campaign Version, Current Session, and open recovery, migration, continuity, or validation state;
6. validates enough of the source and required Read Set to establish an honest readiness state;
7. loads only the material campaign dependency closure and applicable Repository Canon;
8. establishes Gameplay Context only after boot requirements pass.

If the latest source, active version, visibility boundary, or required owner cannot be established, the runtime enters the appropriate clarification, source-recovery, continuity-resolution, or Operational Failure state. It does not substitute conversation memory.

## Player-Action Transaction Flow

The detailed turn gate, relevance-filtered Context Packet, automatic persistence requirement, cache hierarchy, and next-turn reload invariant are owned by [Context Assembly and Gameplay Turn Persistence](CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md). This runtime model supplies the surrounding layers and adapter boundaries.

For one bounded Gameplay Interaction, the runtime:

1. identifies the player's actual declared intent without replacing it;
2. loads the relevant rules and campaign Read Set;
3. separates world truth, actor knowledge, player knowledge, theories, rumours, Secrets, and unknowns;
4. resolves the immediate outcome through the applicable canonical owners;
5. advances only causally due independent world responses;
6. identifies the complete Affected Set and currently resolvable consequences;
7. constructs one owner-routed, idempotent Save Transaction;
8. in `DIRECT`, invokes the Database Format Adapter and configured Storage Adapters within their separate boundaries;
9. in `MCP`, submits the complete transaction through the semantic service and preserves its stable transaction identity;
10. runs or requests required semantic and implementation validation;
11. requires direct adapter evidence or a validated MCP Persistence Receipt from the configured authority;
12. refreshes the active Campaign Version and Derived Views;
13. presents only an outcome consistent with the verified state and the selected execution profile.

The core [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md) determines when a Gameplay Interaction must become durable before dependent adjudication. An AI Execution Profile may impose a stricter presentation gate, such as Save-Before-Delivery, without turning that constraint into fictional physics or a universal human-GM requirement.

## Persistence Mode and Composition

The [Portable Persistence Architecture](../persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md) defines two first-class modes. A campaign selects exactly one active mode.

### DIRECT

Multiple Direct Persistence Adapters may form an ordered Adapter Chain. Each member must declare its adapter class, what it owns, what it receives, what it returns, and what evidence proves completion.

For example:

```text
SQLite Database Format Adapter
  -> remote canonical deployment
  -> remote backup snapshot
```

In that chain:

- the SQLite adapter owns database transactions, foreign-key enforcement, integrity checks, expected-versus-actual comparison, and production of validated database bytes;
- the remote deployment adapter owns canonical remote identity, replacement, remote Read-Back Validation, backup propagation, and remote recovery;
- Campaign Configuration identifies the selected chain, ordering, locators, and required policies;
- the Campaign Persistence Engine continues to own logical meaning, authority, and validation requirements.

Composition never gives one adapter permission to skip another adapter's responsibility. A remote byte match cannot replace database semantic validation, and a valid database transaction cannot prove that remote canonical state was replaced. DuckDB or another approved format may replace SQLite only through its own complete Database Format Adapter.

### MCP

In MCP mode the runtime invokes one semantic persistence service. It does not compose client-side database or storage adapters around that service. The service owns its Microsoft SQL Server transaction, durability, backup, recovery, and deployment implementation. The runtime receives canonical records, semantic status, and validated Persistence Receipts without receiving connection strings, SQL, local/cloud topology, or backup locators.

MCP mode uses `💾` for a validated service receipt, `⏳` for incomplete service work, and `⚠️` for service, transaction, durability, or validation failure. It does not use `☁️💾`, because backend topology is not client authority.

## Gameplay and Development Contexts

**Gameplay Context** presents play while keeping schemas, credentials, connector details, migrations, and repository maintenance backstage. Material Operational Failure is reported plainly when it blocks valid play, but implementation detail is limited to what the player or operator needs to respond.

**Development Context** is entered only through an explicit request to inspect, design, configure, repair, migrate, audit, or reconcile the runtime or campaign. It may expose relevant architecture and diagnostics within authorization boundaries. It does not relax secrecy, invent missing data, or make an implementation detail canonical.

An execution profile defines its context-switch procedure. It cannot use a context label to bypass rules, campaign authority, or access control.

## Failure Boundaries

Failure is contained at the narrowest affected layer.

- A rules gap follows Canonical, Foundation, Provisional, or Unsupported handling.
- A campaign conflict follows Continuity Resolution.
- A missing source follows source recovery and remains unknown meanwhile.
- A failed Direct database transaction rolls back and leaves the prior validated state authoritative.
- A failed Direct deployment or read-back leaves the candidate unverified and blocks dependent use.
- A failed MCP transaction or missing receipt leaves the turn pending or failed even if the transport call itself succeeded.
- A failed backup follows the active execution profile and campaign policy without being reported as complete.
- A failed presentation does not reverse an already validated Save Point; it creates a delivery or recovery concern.

An AI must never claim that a fetch, save, backup, validation, or recovery succeeded unless the responsible layer actually completed and supplied the required evidence.

## Extension Rules

A future AI Execution Profile must:

1. identify the runtime it governs;
2. preserve the shared GM and persistence authority boundaries;
3. declare boot, action, presentation, save, correction, and failure behavior;
4. state observable capability and limitation requirements;
5. keep Campaign Configuration and populated Campaign State external;
6. link rather than duplicate gameplay mechanics.

A future Database Format or Storage Adapter must:

1. use the `<TECHNOLOGY>_<ADAPTER_CLASS>_ADAPTER.md` naming convention;
2. declare its exact adapter class and responsibility in a Direct Adapter Chain;
3. preserve stable authority and identity;
4. define freshness, concurrency, validation, read-back, and failure behavior;
5. avoid campaign-specific identifiers and credentials;
6. never adjudicate gameplay.

A future MCP persistence implementation must preserve the semantic read, transaction, idempotency, validation, receipt, hidden-backend, and failure contracts in [MCP Persistence Mode](../persistence/MCP_PERSISTENCE_MODE.md). It may replace backend technology only through an owner-authorized revision and migration; backend choice never changes fictional canon.

Replacing a profile or adapter changes runtime operation only. It does not alter fictional canon, campaign truth, or the Campaign Persistence Engine unless a separately authorized repository or campaign migration does so.

## Safeguards

- The AI GM does not own the rules or campaign state.
- Model memory and conversation context are non-authoritative.
- Current release metadata outranks historical lifecycle wording during bootstrap.
- A new campaign defaults to `NORMAL`; testing Campaign Modes require explicit selection.
- Provisional Rulings and First-Life Mode do not change Campaign Mode.
- Campaign Configuration selects implementations but does not become a fact owner.
- Direct Adapters and MCP persistence services carry and verify state; they do not interpret mechanics.
- Campaign Configuration selects exactly one Persistence Mode; a failure never authorizes silent fallback.
- Adapter convenience never collapses Truth Layers, Persistence Levels, versions, or record owners.
- A missing field remains missing rather than being filled for schema completeness.
- Operational success never substitutes for semantic validation.
- No private locator, credential, access token, or populated campaign fact belongs in this document.

## Simulation Architecture Interface

The [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md) governs the AI GM's conceptual flow. The runtime receives a declared action through the Player RPG Interface, resolves it against relevant Immutable Rules, updates objective state through the GM Simulation Engine, and filters the result through the active Perspective before delivery. The AI runtime operates these responsibilities but owns none of their truth.

Entity, Controller, and Perspective must remain separate in loaded state and generated narration. Model memory cannot supply an Entity's knowledge, infer a Controller assignment, or expose GM Secrets merely because those facts occur in context.

## Related Documents

- [AI Operating Procedures Index](README.md)
- [AI Capabilities and Limitations](AI_CAPABILITIES_AND_LIMITATIONS.md)
- [ChatGPT GM Universal Instructions](chatgpt/CHATGPT_GM_UNIVERSAL_INSTRUCTIONS.md)
- [Portable Persistence Architecture](../persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- [Direct Persistence Mode](../persistence/DIRECT_PERSISTENCE_MODE.md)
- [MCP Persistence Mode](../persistence/MCP_PERSISTENCE_MODE.md)
- [Direct Persistence Adapter Index](../persistence/adapters/README.md)
- [AI GM Workflow](AI_GM_WORKFLOW.md)
- [AI Save Protocol](AI_SAVE_PROTOCOL.md)
- [Campaign Persistence Integration](../persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md)
- [Structured Persistence Architecture](../persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md)
- [Persistence Validation](../persistence/PERSISTENCE_VALIDATION.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
