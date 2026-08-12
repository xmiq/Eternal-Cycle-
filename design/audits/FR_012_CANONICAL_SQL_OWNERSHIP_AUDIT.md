# FR-012 Canonical SQL Ownership Implementation Audit

## Scope

This audit records implementation of Phase 12 objective FR-012. It contains no populated campaign state and performs no campaign migration.

## Ambiguities Found

- The persistence architecture required one record owner but did not provide a complete fact-family ownership map.
- Player State, Companions, and Actors could be read as separate identity owners for the same persistent being.
- Locations listed occupants while character views also listed current location, without explicitly selecting one current-placement owner.
- Composite character templates could be mistaken for monolithic authoritative records.
- The future Autonomous Registry boundary was described architecturally but not reserved in campaign-data ownership.
- Historical repetition and convenient derived repetition needed a clearer distinction from conflicting mutable duplication.

## Implementation

[Canonical Data Ownership](../../docs/persistence/CANONICAL_DATA_OWNERSHIP.md) now owns the repository-wide campaign-data ownership contract. It defines:

- one authoritative logical owner per mutable fact;
- one Entity identity anchor for every persistent non-autonomous being whose continuity matters;
- normalized ownership for embodiment, Development, Skills, placement, Relationships, Inventory, conditions, factions, Projects, Research, Locations, Species, Infrastructure, history, Knowledge, and Secrets;
- separation of campaign individuals from reusable Living Codex definitions;
- Authoritative, Reference, Derived, Cache, and Historical Snapshot classifications;
- current-location and Relationship ownership invariants;
- a non-overlapping reserved domain for FR-014;
- identity-preserving category-change requirements;
- SQLite constraint and validation guidance without introducing stored procedures or a competing database technology.

The persistence index, architecture, state model, Save Update Protocol, and validation catalogue now link to that owner. Character, Relationship, Location, and Save Index templates clarify authority where duplication risk was material.

## Schema and Migration Impact

No executable campaign schema, migration set, or populated campaign database exists in this repository. The existing architecture is intentionally storage-neutral and the SQLite adapter defines implementation behavior rather than a reusable campaign schema.

No schema change or migration was created. Existing saves are not modified. A concrete campaign implementation maps its schema to the ownership contract and uses the established migration protocol only if that external audit finds structural conflict.

## Validation

- repository Markdown links and anchors;
- canonical index and document-registry coverage;
- one-owner, Entity-anchor, placement, Relationship, Species, historical, derived-data, and autonomous-boundary language;
- roadmap and Future Revision provenance;
- repository boundary against campaign data and populated databases;
- complete diff and whitespace validation.

## Phase 12 Preparation

- **FR-004:** Life Summaries are classified as indexes or Historical Snapshots rather than current-state owners.
- **FR-010:** Long-Horizon Summaries are classified as derived historical compression subordinate to detailed Canon.
- **FR-011:** stable IDs, owner domains, typed references, and non-authoritative cache rules provide the inputs for later Context Assembly.
- **FR-014:** autonomous persistence has a reserved owner domain and an identity-preserving transition invariant without a premature registry schema.

## Unresolved Ownership Questions

None block FR-012. Physical table names, exact constraints, and migrations remain implementation-specific decisions for each external campaign schema, governed by this logical contract and the existing migration protocol.

## Related Documents

- [Canonical Data Ownership](../../docs/persistence/CANONICAL_DATA_OWNERSHIP.md)
- [Campaign Persistence Engine](../../docs/persistence/README.md)
- [Persistence Validation](../../docs/persistence/PERSISTENCE_VALIDATION.md)
- [Simulation Architecture and Perspective Model](../../docs/core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
- [Development Roadmap](../ROADMAP.md)
- [Future Revisions](../FUTURE_REVISIONS.md)
