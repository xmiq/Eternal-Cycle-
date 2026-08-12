# Simulation Architecture Integration Audit

## Scope

This audit records the gameplay-validation maintenance review that introduced the [Simulation Architecture and Perspective Model](../../docs/core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md). It is a point-in-time architecture audit, not a new mechanic, populated campaign record, or authorization for future systems.

## Systems Reviewed

- Core architecture and design governance
- AI Runtime and AI GM operating procedures
- Game Master Framework and uncertainty handling
- Campaign Persistence authority, Truth Layers, structured state, and continuity resolution
- World Engine autonomous simulation and Simulation Abstraction
- Reincarnation and Soul/body continuity
- GM Living Codex reusable-design boundary
- relationship continuity and blank Knowledge View records
- Future Revisions `FR-014` and `FR-015`

## Confirmed Boundaries

| Concern | Canonical result |
| --- | --- |
| Mechanical possibility | Immutable Rules and their specialist owners |
| Objective campaign truth and change | GM Simulation Engine, operated through World Engine and GM procedures |
| Durable campaign memory | Campaign Persistence Engine |
| Player-facing presentation | Player RPG Interface after Perspective Filtering |
| Identity | Entity remains separate from Controller and Perspective |
| Player role | external Controller, not the player-character Entity |
| Reincarnation | Controller, Soul, incarnated Entity, and body distinctions remain owned by existing rules |
| Knowledge | Entity-relative and separate from objective truth |
| Reusable designs | GM Living Codex, distinct from campaign instances |

## Corrections Made

- Added one authoritative three-layer architecture owner.
- Added explicit Entity, Controller, Perspective, autonomy, and persistent-identity boundaries.
- Connected AI play to objective resolution followed by Perspective Filtering.
- Added persistence extension points without imposing a speculative schema migration.
- Clarified that Truth Layers apply to all relevant Entities, not only the player character.
- Updated the blank Knowledge View contract to name the knowing Entity and Perspective explicitly.
- Updated `FR-014` to depend on this architecture while remaining unimplemented.
- Narrowed `FR-015` to the detailed Knowledge System features that remain future work.

## Schema Finding

No existing canonical schema required migration. The storage-neutral Campaign State Graph already supports stable IDs, typed references, Truth Layers, and owner-routed extensions. A detailed Knowledge System or Autonomous Registry may later normalize additional records, but no evidence supports inventing those tables during this maintenance task.

## Deferred Questions

The following remain Future Revision scope rather than contradictions:

- per-Entity belief and evidence records;
- confidence and contradiction models;
- testimony, misinformation propagation, and information sharing;
- memory degradation and inference systems;
- autonomy levels and autonomous-entity registry structures;
- possession, remote-body, shared-sense, distributed-mind, and multiplayer mechanics.

These do not block the conceptual architecture.

## Validation Result

The integrated model preserves Repository Canon, Campaign Canon, Character Knowledge, GM Secrets, structured-save authority, player agency, Reincarnation distinctions, off-screen causal simulation, and the repository/campaign boundary. No campaign data or populated schema artifact was introduced.

## Related Documents

- [Simulation Architecture and Perspective Model](../../docs/core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
- [Truth Layers](../../docs/persistence/TRUTH_LAYERS.md)
- [Campaign State Model](../../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [AI Runtime Model](../../docs/ai/AI_RUNTIME_MODEL.md)
- [Future Revisions](../FUTURE_REVISIONS.md)
