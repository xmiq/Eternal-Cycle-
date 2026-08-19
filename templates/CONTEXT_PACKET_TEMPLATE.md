# Context Packet Template

Use this blank template for a Current Scene Context, Recent Running Summary, or Session Summary. Populated packets belong in external campaign persistence and remain Derived Data or Cache records.

## Document Control

- **Template owner:** [Context Assembly and Gameplay Turn Persistence](../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- **Required fields:** packet identity, kind, parent campaign version, parent Save Point, audience, provenance, freshness, and source references
- **Optional fields:** every context section not material to the interaction
- **Consumers:** runtime context builders, AI or human-supervised GM tools, debugging, handoff, cache validation, and session recovery

## Packet Identity

- **Context Packet ID:** `<stable cache identity>`
- **Context kind:** `<Current Scene, Recent Running, or Session>`
- **Interaction ID:** `<ID or not applicable>`
- **Parent Campaign Version:** `<version>`
- **Parent Save Point ID:** `<ID>`
- **Persistence Model/Migration Version:** `<versions where relevant>`
- **Generated game time:** `<canonical temporal reference or Unknown>`
- **Generated-at operational time:** `<timestamp>`
- **Audience/visibility scope:** `<GM, player, character, protected, or configured scope>`
- **Freshness status:** `<Fresh, Stale, or Invalid>`
- **Provenance:** `<query/build operation and source revisions>`

## Active Boundary

- **Active Perspective:** `<Perspective reference>`
- **Active Entity ID:** `<player-character or other active Entity reference>`
- **Active Location ID:** `<authoritative placement reference>`
- **Current intent:** `<bounded player intent>`
- **Unresolved action:** `<none or exact open boundary>`
- **Stop conditions:** `<material read, authority, visibility, or persistence blockers>`

## Relevant Canonical Facts

| Owner domain | Stable record ID | Relevant claim or compact value | Source revision | Relevance reason | Visibility |
|---|---|---|---|---|---|
| `<authoritative owner>` | `<ID>` | `<minimal working fact>` | `<revision>` | `<presence, intent, dependency, hazard, history, or other reason>` | `<scope>` |

Facts copied here are Derived. Their owner records remain authoritative.

## Relevant Entities and References

| Subject/reference | Stable ID | Owner domain | Reference path | Deep-read target |
|---|---|---|---|---|
| `<Entity, Relationship, Skill, Project, Autonomous, Life, Historical Period, or other>` | `<ID>` | `<owner>` | `<short typed path>` | `<owner record or index>` |

## Immediate Conditions and Pressures

- `<relevant condition, hazard, resource, opposition, time pressure, or Pending Consequence with owner reference>`

## Recent Committed Changes

- `<Interaction/Transaction ID, changed owner IDs, resulting Save Point, and relevance>`

Do not include uncommitted narrative drafts as committed changes.

## Unresolved Immediate Threads

- `<thread, uncertainty state, source reference, and review condition>`

## Historical Drill-Down

- **Life IDs:** `<only materially relevant IDs>`
- **Historical Period IDs:** `<only materially relevant IDs>`
- **Timeline/Campaign History references:** `<only materially relevant IDs>`

## Read Set Evidence

| Owner/source | Record ID | Version/revision | Read result | Material claim supported |
|---|---|---|---|---|
| `<owner>` | `<ID>` | `<version>` | `<loaded, missing, stale, disputed, or blocked>` | `<claim>` |

## Affected Set Result

- **Status:** `<Not Yet Determined, Empty, or Non-Empty>`
- **Owner records:** `<stable IDs and operations; omit until determined>`
- **Transaction ID:** `<ID when persistence required>`
- **Validation/read-back:** `<pending, passed, failed, or not applicable>`
- **Resulting Save Point:** `<ID only after activation>`

## Cache Refresh

- **Invalidated packet IDs:** `<IDs or none>`
- **Regenerated from Save Point:** `<ID>`
- **Freshness evidence:** `<source revisions/hash/change token as available>`

## Development Context Diagnostics

This section is protected operational output and is omitted from ordinary player-facing play.

- **Queries/views/functions used:** `<implementation-specific references>`
- **Adapter chain:** `<configured logical chain without credentials>`
- **Read warnings:** `<warnings>`
- **Write/validation warnings:** `<warnings>`
- **Retry state:** `<Interaction ID, Transaction ID, parent version, and recovery state>`

## Validation Notes

- [ ] Packet parent version and Save Point match active Canon.
- [ ] Every material fact has a stable owner reference.
- [ ] Selection is relevant and dependency-complete.
- [ ] GM Context and Character Knowledge remain separate.
- [ ] No GM Secret leaks into an unauthorized audience.
- [ ] Derived content does not claim current-state ownership.
- [ ] Stale content is invalidated rather than used to alter Canon.
- [ ] Affected Set was determined explicitly.
- [ ] Non-empty changes committed and validated before turn closure.
- [ ] Next-turn retrieval can follow stable IDs to owner records.

## Cross-References

- [Context Assembly and Gameplay Turn Persistence](../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation Report Template](VALIDATION_REPORT_TEMPLATE.md)
- [Save Index Template](SAVE_INDEX_TEMPLATE.md)
