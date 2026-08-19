# Life Archive and Old-Soul Indexing

## Purpose

The **Life Archive** is the canonical campaign-persistence contract for indexing a Soul's incarnations. It gives players and GMs compact access to completed-life history through:

```text
Soul Overview
    -> Life Summary
        -> Full Life Detail
```

It supports Souls with dozens or hundreds of lives without loading every historical record. It does not replace Timeline, Campaign History, Relationships, Development, Skills, or any current-state owner.

## Document Control

- **Owner:** stable Life identity, archive status, Soul Overview indexing, finalized Life Summaries, archive references, and Life Over derivation
- **Dependencies:** [Reincarnation](../soul/REINCARNATION.md), [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md), [Timeline](TIMELINE_ENGINE.md), [Campaign History](STRUCTURED_PERSISTENCE_ARCHITECTURE.md#campaign-history), and existing domain owners
- **Extensions:** retained-development, cross-embodiment Skill, Skill consolidation, and future long-horizon summary, context-assembly, memory, and Soul-bound companion rules may consume archive references without changing this ownership
- **Consumers:** players, GMs, migration, historical retrieval, validation, and external campaign implementations

## Akashic Archive Boundary

The Life Archive is not the [Akashic Archive](../soul/AKASHIC_ARCHIVE.md).

| Structure | Canonical responsibility |
|---|---|
| **Akashic Archive** | A world-deep metaphysical structure of incomplete, source-bound Archive Traces and Records. Access requires an in-world basis, interface, compatibility, and adjudication. |
| **Life Archive** | A campaign persistence index and historical-summary structure assembled from established campaign records. Player and GM access is operational, not an in-world supernatural reading. |

An Akashic Record may provide evidence used to recover a Life Archive fact. A Life Summary may reference an Akashic Record. Neither structure becomes the other, and neither grants automatic character memory.

## Core Invariants

1. Every indexed incarnation has one stable **Life ID** linked to one Soul and one Incarnation.
2. Names, aliases, and incarnation order do not serve as permanent identity.
3. A completed incarnation receives one concise finalized Life Summary.
4. The active incarnation may be indexed as active but has no finalized Life Summary.
5. Life Summaries are Historical Snapshots, indexes, and references, not owners of mutable current state.
6. Missing history remains unknown; archive completeness is never fabricated.
7. Player access does not create Character Knowledge, autobiographical recall, Skill access, or present capability.
8. Full Life Detail remains with the records that own it.

## Stable Life Identity

A **Life ID** permanently identifies one incarnation of one Soul. Storage implementations should use an opaque immutable identifier, such as a UUID or an implementation's established stable-ID convention. Display ordinals are separate mutable classifications.

A Life record minimally identifies:

- Life ID;
- Soul ID;
- Incarnation ID;
- active or completed archive status;
- established incarnation order, if known;
- source and creation provenance;
- summary reference when finalized.

One incarnation remains one Life through major Evolution, transformation, or embodiment change unless [Reincarnation](../soul/REINCARNATION.md) establishes Final Death and a new incarnation. An ordinal may remain unknown or be corrected after recovered history without changing the Life ID.

## Retrieval Hierarchy

### Soul Overview

The **Soul Overview** is a compact, pageable index. Each entry may expose:

- Life ID and established incarnation order;
- identity or principal name;
- starting, principal, or final Species/Form references where useful;
- world, Contact Domain, Age, or broad historical period where established;
- temporal bounds or lifespan with uncertainty;
- active/completed status;
- Final Death reference or status;
- concise identifying tags;
- Life Summary reference.

It must not copy complete biographies, mutable relationship state, current Skill state, or every historical detail.

### Life Summary

A **Life Summary** is a concise permanent historical snapshot finalized for a completed incarnation. Applicable sections may reference:

- identity, temporal bounds, birth or starting circumstances, and Final Death;
- starting, evolved, transformed, and final embodiments;
- major attribute peaks and Development milestones;
- meaningful Skill acquisition, peaks, Evolution, predecessor, successor, and merge history;
- Classes, Professions, institutions, and credentials held during that life;
- Soul Titles and Soul Weapon milestones;
- major Relationships, companions, factions, discoveries, research, creations, infrastructure, and Projects;
- defining achievements, failures, transformations, and world or cosmological interactions;
- unresolved legacy, mysteries, promises, enemies, projects, artifacts, and consequences;
- lasting Soul consequences established by source owners;
- historical manifestations of a future Soul-bound companion relation;
- provenance and references to Full Life Detail.

Optional fields remain absent or explicitly unknown. The summary is not a save dump.

### Full Life Detail

**Full Life Detail** is a retrieval path, not one monolithic archive record. It follows stable references into Timeline, Campaign History, Relationships, Development, Skills, Species, Soul Weapons, Research, Projects, Infrastructure, Locations, Factions, Mysteries, Akashic Records, and other owners.

## Active and Completed Lives

An active Life record is a navigation identity linked to current canonical state. It may carry index labels and references, but it remains `active`, has no finalization event, and cannot claim a finalized summary.

A completed Life record is closed only after a canonical terminal transition, normally Final Death. Closing current embodiment state and finalizing its Life Summary are linked operations, but the summary does not own the death, chronology, Development, Skills, or consequences it reports.

Only one active Life may exist for one Soul unless another canonical rule explicitly permits otherwise. Interlife means the preceding Life is completed and no successor Life is active yet.

## Finalization Procedure

When an incarnation reaches Final Death or another canonically valid terminal transition:

1. confirm the Soul, Incarnation, and stable Life identities;
2. close the active Life status through the authoritative death and incarnation records;
3. collect only established archive-relevant facts from their owners;
4. preserve source confidence, uncertainty, disputes, and Record Gaps;
5. create or update the concise Life Summary;
6. attach stable references to deeper records;
7. update the Soul Overview entry;
8. validate ownership, identity, chronology, references, visibility, and summary status;
9. commit the complete Affected Set through the [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md).

Finalization cannot invent a clean birth date, cause of death, numerical peak, relationship, or achievement merely to complete presentation.

## Historical Ownership

Life Archive repetition is legitimate only as a Historical Snapshot, derived presentation, index, or stable reference.

| Claim | Authoritative owner | Archive treatment |
|---|---|---|
| Current relationship dimensions | Relationships | Historical role or milestone reference for this Life |
| Current Skill/Development | Skill and Development records | Per-life peak, milestone, and source reference |
| Current location | placement owner | Location at a historical event |
| Current embodiment | active Incarnation/embodiment owner | Form interval or transition history |
| Current Soul Weapon state | Soul Weapon record | Meaningful historical milestone |
| Final Death | death event and Timeline/History | Resolved reference and concise account |
| Soul bond | future Soul/fate owner | Life-specific manifestation only |

Correction of an owner record may require a traceable summary correction. It never authorizes silently editing current state through the summary.

## Development and Skill History

The archive preserves queryable per-Life references needed by future objectives without implementing them.

Development indexing may identify a Development track, embodiment interval, attained or peak expression, evidence window, and source record. It does not compute retained-development bonuses or transfer current capability.

Skill indexing may identify Skill ID, embodiment, acquisition, meaningful peak, Evolution, merge, predecessor/successor relation, expression, and source records. It does not grant cross-embodiment transfer, perform a merge, broaden scope, or replace the Skill owner.

Consolidation may supersede a current Skill label without erasing prior-Life evidence. Life records retain stable references to the identity effective at the time, while typed lineage allows retained-development and scope adjudication to reach the active consolidated identity and its predecessors.

[Retained Cross-Life Development](../progression/RETAINED_CROSS_LIFE_DEVELOPMENT.md) consumes these references to construct source contributions, Embodiment Relevance assessments, Translation Bridges, and bounded Derived profiles. The Life Archive remains historical evidence and does not store the current effective bonus as archive truth.

## Embodiment History

A Life may reference multiple significant forms. Form intervals distinguish starting form, major Evolution, transformation, principal form, and final form. Temporary conditions are not separate Lives and need not be indexed unless historically defining.

Species references point to campaign Species or configured Living Codex identities. A Life record never becomes the reusable Species definition.

## Relationships and Soul-Bound Manifestations

Major Relationship entries use stable participant and Relationship references with a historical role or milestone. Full current dimensions remain with Relationships.

For future FR-016 integration, a Life Summary may record reunion, separation, recognition, non-recognition, alliance, conflict, kinship, or another established manifestation. It does not create or own a Soul bond, guarantee reunion, transfer a relationship, or imply current memory.

## Player Visibility and Knowledge

The Soul Overview, finalized Life Summaries, and Life Over presentation are available to the player as out-of-character campaign history. Protected GM Secrets and facts not authorized for player visibility remain excluded or redacted through Truth Layers.

Player-visible archive information is not Character Knowledge. The current incarnation may act on only the knowledge and memory established for that Entity. Future FR-015 owns autobiographical fading and recall; this contract does not implement it.

## Life Over Presentation

The **Life Over Presentation** is a non-authoritative readable view generated from one finalized Life Summary. It should answer:

- Who did this incarnation become?
- How did it develop?
- What mattered?
- How did it end?
- What did it leave behind?
- What changed for the Soul?

It preserves uncertainty and disclosure boundaries, omits raw database plumbing, and cites or exposes drill-down references where appropriate. Correct the source summary rather than treating edited presentation text as Canon.

## Retrieval for Long-Lived Souls

Implementations should support pagination and filters over Life ID, ordinal, status, Species/Form, world, Contact Domain, Age, temporal bounds, tags, major Skill, Relationship participant, major event, unresolved legacy, and future Soul-bond reference. Search results return compact index rows before summaries and deeper records.

This is archive retrieval, not FR-011 Context Assembly. Context Packets may consume these indexes but remain Derived.

## Logical SQLite Contract

The repository defines a storage-neutral contract, not an executable campaign schema. A SQLite implementation should normalize equivalent structures:

```sql
CREATE TABLE lives (
    life_id TEXT PRIMARY KEY,
    soul_id TEXT NOT NULL,
    incarnation_id TEXT NOT NULL UNIQUE,
    incarnation_ordinal INTEGER CHECK (incarnation_ordinal IS NULL OR incarnation_ordinal > 0),
    archive_status TEXT NOT NULL CHECK (archive_status IN ('active', 'completed')),
    finalization_event_id TEXT,
    created_provenance_id TEXT NOT NULL,
    UNIQUE (soul_id, incarnation_ordinal),
    FOREIGN KEY (soul_id) REFERENCES souls(soul_id),
    FOREIGN KEY (incarnation_id) REFERENCES incarnations(incarnation_id)
);

CREATE TABLE life_summaries (
    life_id TEXT PRIMARY KEY,
    summary_version INTEGER NOT NULL CHECK (summary_version > 0),
    finalized_at TEXT NOT NULL,
    finalized_by_event_id TEXT NOT NULL,
    summary_text TEXT NOT NULL,
    provenance_id TEXT NOT NULL,
    FOREIGN KEY (life_id) REFERENCES lives(life_id)
);

CREATE TABLE life_archive_references (
    life_id TEXT NOT NULL,
    reference_type TEXT NOT NULL,
    referenced_record_id TEXT NOT NULL,
    historical_role TEXT,
    sort_key TEXT,
    provenance_id TEXT NOT NULL,
    PRIMARY KEY (life_id, reference_type, referenced_record_id, historical_role),
    FOREIGN KEY (life_id) REFERENCES lives(life_id)
);

CREATE INDEX idx_lives_soul_order ON lives(soul_id, incarnation_ordinal);
CREATE INDEX idx_life_refs_lookup ON life_archive_references(reference_type, referenced_record_id);
```

Concrete schemas may normalize names, tags, form intervals, Development peaks, Skill history, unresolved legacy, visibility, and revision provenance into additional tables. They must preserve owner references and must not serialize many-to-many relations into ambiguous comma-separated fields.

Because some source domains may use heterogeneous ID spaces, `life_archive_references` requires semantic reference validation even where SQLite cannot express one polymorphic foreign key. Implementations with dedicated relation tables should use direct foreign keys.

## Migration and Recovered History

Existing campaigns adopt the archive through the canonical migration sequence:

1. back up the latest canonical save;
2. audit transcripts, exports, prior saves, Timeline, Campaign History, and other sources without writing;
3. identify Souls and distinct Incarnations without merging by name;
4. assign stable Life IDs and preserve uncertain ordinals;
5. create summaries only from established Canon;
6. preserve `Unknown`, `Estimated`, or `Requires Source Recovery` as appropriate;
7. link existing detailed records and record migration provenance;
8. validate identities, chronology, ownership, references, disclosure, and summary status;
9. activate only the validated migrated save.

Later source recovery may append references or create a traceable new summary revision. It does not silently rewrite historical provenance or auto-repair ambiguous identity.

## Save Update Discipline

Ordinary turns update current owners, Timeline, and Campaign History as required; they do not rewrite every Life Summary. Archive work occurs when:

- an active Life is established;
- a significant archive reference is deliberately prepared;
- a Life completes and is finalized;
- historical information is recovered or corrected;
- an archive query or migration exposes a validation defect.

All archive writes use a bounded Affected Set and the configured SQLite Adapter Chain. A local SQLite commit is not a completed remote Save Point when deployment adapters are active.

## Validation

Validation must detect or report:

- duplicate Life IDs or one Incarnation linked to multiple Lives;
- missing Soul or Incarnation identities;
- contradictory or duplicate ordinals within one Soul where ordinals are established;
- more than one active Life for one Soul;
- a finalized summary for an active Life;
- a completed Life lacking a finalization basis or required Final Death reference;
- dangling Species/Form, Skill, Development, Relationship, Timeline, History, Soul Weapon, or other references;
- current-state claims presented as archive-owned facts;
- player-visible archive data promoted into Character Knowledge without a valid route;
- unknown values replaced by unsupported facts;
- accidental conflation of Life Archive and Akashic Archive;
- summary revisions lacking provenance.

Validation reports ambiguity; it does not invent identity, chronology, or missing history to repair it.

## Compatibility Hooks Only

- **FR-001:** consumes per-Life Development references; no acceleration rule is implemented here.
- **FR-005/FR-006/FR-007:** consume Skill and embodiment history; no transfer, merge, or scope rule is implemented here.
- **FR-010:** [Long-Horizon Summaries](LONG_HORIZON_SUMMARIES.md) cross-reference Life IDs many-to-many while remaining separate Historical Period records; neither grants current-character memory.
- **FR-011:** [Context Assembly](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) retrieves Overview, Summary, then relevant detail through stable Life IDs without loading every Life or granting current-character memory.
- **FR-015:** [Memory Continuity](../soul/MEMORY_CONTINUITY.md) owns cross-incarnation autobiographical Memory identity and accessibility history, while Character Knowledge owns current conscious recall; archive visibility grants neither.
- **FR-016:** [Soul-Bound Companion Fate](../soul/SOUL_BOUND_COMPANIONS.md) owns persistent Bond identity and current convergence obligations; the archive indexes only Life-specific historical manifestations and references.

## Safeguards

- Do not make the Life Archive perfect memory or omniscient history.
- Do not duplicate every detailed record into summaries.
- Do not freeze active current state as a finalized summary.
- Do not transfer historical peaks into present capability.
- Do not expose protected GM Secrets through tags, indexes, or Life Over views.
- Do not identify Lives solely by names or ordinals.
- Do not fabricate completeness for old or migrated campaigns.
- Do not rewrite the archive every turn when no archive-relevant fact changed.
- Do not turn the Akashic Archive into campaign storage.
- Do not add populated Life records to this repository.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md)
- [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Persistence Validation](PERSISTENCE_VALIDATION.md)
- [Timeline Engine](TIMELINE_ENGINE.md)
- [Relationship Memory Engine](RELATIONSHIP_MEMORY_ENGINE.md)
- [Akashic Archive](../soul/AKASHIC_ARCHIVE.md)
- [Reincarnation](../soul/REINCARNATION.md)
- [Life Archive Template](../../templates/LIFE_ARCHIVE_TEMPLATE.md)
- [FR-004 Implementation Audit](../../design/audits/FR_004_LIFE_ARCHIVE_AUDIT.md)
