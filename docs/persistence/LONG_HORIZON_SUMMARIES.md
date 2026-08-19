# Long-Horizon Simulation Summaries

## Purpose

A **Long-Horizon Summary** preserves meaningful established change across months, years, decades, centuries, Ages, Interlife gaps, and World Resets without requiring exhaustive simulation or retrieval of every minor event.

It answers:

> What meaningfully changed during this historical interval, why, and what remains relevant?

It does not claim to answer what happened every day.

## Document Control

- **Owner:** stable Historical Period identity, compressed historical statement, explicit scope, causal links, continuity hooks, historical references, summary provenance, revision history, and Derived player-facing transition views
- **Dependencies:** [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md), [Timeline Engine](TIMELINE_ENGINE.md), [Canonical Data Ownership](CANONICAL_DATA_OWNERSHIP.md), [Life Archive](LIFE_ARCHIVE.md), [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md), and all specialist owners changed during the interval
- **Extensions:** FR-011 may retrieve relevant period indexes; FR-015 may consume historical context without granting memory; FR-016 may contribute referenced fate events without being implemented here
- **Consumers:** GMs, AI operators, Time Skips, Reincarnation, Age transitions, World Resets, migration, historical retrieval, validation, and filtered player presentation

## Core Invariants

1. Every summary has one stable **Historical Period ID** independent of its title or display order.
2. A summary compresses established historical results; it does not invent missing history or resolve future history in advance.
3. Important causal chains survive compression when Canon established them.
4. Current mutable state remains with its normal authoritative owners.
5. A summary owns its historical statement, scope, references, provenance, and revisions only.
6. Unknown, Estimated, Not Established, and Source Recovery Required claims remain honestly qualified.
7. A [Life Summary](LIFE_ARCHIVE.md#life-summary) and a Long-Horizon Summary remain distinct and may cross-reference each other many-to-many.
8. Player-facing Historical Transitions are Derived, Perspective-filtered views and never grant Character Knowledge.
9. Closed intervals change only through authorized correction, retcon, migration, or source recovery with provenance.
10. Summary nesting uses stable references and cannot form cycles.

## Historical Period Identity

A **Historical Period ID** is an opaque immutable identifier for one bounded historical claim. Implementations may use UUIDs or another established stable-ID convention.

Each period records, where established:

- start and end temporal markers with honest precision;
- relevant Age boundaries;
- world, Contact Domain, region, location, or other explicit scope;
- related Soul and Life IDs where material;
- the trigger or reason for compression;
- summary status and version;
- source and revision provenance.

Exact dates are optional. Approximate Ages, relative ordering, bounded intervals, and Unknown endpoints are valid when they match the evidence.

## Scope and Granularity

Scope is explicit and no broader than the resolved evidence.

- **Local Interval:** a settlement, Project, institution, ecosystem, or small region.
- **Period Summary:** a multi-year or multi-decade regional or thematic interval.
- **Era Summary:** a high-level century, Age, or major transition.
- **Cross-Domain Summary:** a multi-world or cosmological interval only when established cross-domain change is relevant.

These labels guide retrieval rather than impose a rigid hierarchy. A forty-year civil war may need deeper resolution than forty peaceful years. Use the minimum detail that preserves agency, causality, consequences, uncertainty, and meaningful retrieval.

## Compression Model

Long-horizon compression begins with established starting state and active processes. It selects material changes by asking whether omission would impair:

- explanation of current state;
- continuity of a significant Entity, Relationship, Project, institution, faction, species, infrastructure asset, or world process;
- a later choice, risk, obligation, opportunity, or mystery;
- interpretation of an Age transition, World Reset, Reincarnation gap, or prior-Life legacy;
- source recovery or validation.

A summary may include major world, faction, settlement, migration, ecological, species, infrastructure, research, technological, magical, cultural, economic, conflict, disaster, Project, Relationship, Gate, cosmological, Age, and Reset changes where relevant. Empty categories remain absent.

### Causal Compression

Preserve the shortest established chain sufficient to explain a material result. If Canon established:

```text
famine -> faction conflict -> evacuation -> settlement collapse
```

the summary may compress details but cannot report only that the settlement later became ruined. Distinguish established causation from correlation, inference, disputed attribution, and Unknown cause.

### Sparse History

Elapsed time is not evidence. Do not invent rulers, battles, dates, migrations, discoveries, motives, population values, or Relationship changes to make an interval look complete. A sparse period receives a sparse summary.

Do not retroactively over-simulate unseen centuries. Resolve only processes supported by established causes, pressures, actors, resources, and rules when the interval becomes relevant.

## Continuity Hooks

A **Continuity Hook** is a stable reference to something materially relevant beyond the compressed interval. Examples include surviving or ruined settlements, bloodlines, institutions, factions, infrastructure, artifacts, species consequences, unresolved Projects, mysteries, threats, grudges, legends, long-lived Entities, and persistent prior-Life consequences.

Hooks identify relevance; they do not create current facts. Their target owners remain authoritative. GMs should reuse established hooks where relevant rather than replacing history with unrelated invention, but every later scene need not reference the past.

## Specialist Resolution

The summary records outputs after specialist owners resolve them.

- Projects may complete, fail, stall, change ownership, become institutions, or remain unresolved; elapsed time alone completes none.
- Infrastructure may be maintained, expanded, decayed, destroyed, abandoned, automated, repurposed, or inherited according to established resources and actors.
- Relationships may persist, fade, transform, institutionalize, or become irrelevant only through established causes; this does not implement autobiographical memory.
- Factions, populations, species, ecology, research, magic, technology, economies, and conflicts remain governed by their existing owners.
- Long-lived Entities remain active campaign Entities when applicable rather than becoming mere background.

### Autonomous Registry

Relevant [Autonomous Registry](AUTONOMOUS_REGISTRY.md) entries participate through assignment, autonomy, resources, maintenance, network, Controller, condition, and last-confirmed evidence. Absence of a later report does not establish destruction.

Example: an exploration servitor last confirmed entering a ruin remains `Current fate Unknown` after eighty years unless a valid source establishes more.

## Time Skip Procedure

For a long interval:

1. load the relevant starting Canon and active Save Point;
2. define timeframe, scope, granularity, and trigger;
3. identify active processes, Pending Consequences, Review Points, and material specialist owners;
4. identify relevant Entities, Relationships, factions, ecosystems, Projects, Research, Infrastructure, Gates, and Autonomous Registry records;
5. resolve only meaningful changes at the minimum sufficient Simulation Frame;
6. stop at material agency, uncertainty, or Review Point boundaries;
7. preserve unknowns and source limitations;
8. write every current-state change to its authoritative owner;
9. append significant Timeline and Campaign History events under existing rules;
10. generate the Long-Horizon Summary from the established results;
11. attach source, Entity, Life, event, thread, scope, and continuity references;
12. validate the complete Affected Set and activate the Save Point;
13. derive a filtered Historical Transition when appropriate.

The summary never substitutes for steps 8 and 9. A completed long-horizon simulation is a major persistent change and follows existing save rules; automatic turn enforcement is owned by [FR-011](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md).

## Reincarnation and Interlife

When substantial time passes between Final Death and the next incarnation, resolve relevant world change before candidate placement whenever stale state could affect available embodiments, world conditions, or consequences.

The summary may reference the preceding and succeeding Life IDs, but it does not select the candidate, grant memory, or alter Soul continuity. The new incarnation enters the updated world, not an assumed pre-gap snapshot.

## Ages, Resets, and World Contact

An Age transition or World Reset does not erase prior Canon. Record:

- the established transition cause and boundaries;
- changed current-state owners;
- what survived, disappeared, transformed, or became Unknown;
- deliberately reset structures;
- migrations and significant events;
- enduring consequences and continuity hooks.

Historical truth remains historical truth even when current rules or world conditions change.

World Knowledge, World Contact, Travel Routes, and Reincarnation Possibility remain separate. A long interval may establish loss, decay, creation, or transformation of a Gate or channel only through existing rules.

## Life Archive Relationship

The structures answer different questions:

| Structure | Question | Scope |
| --- | --- | --- |
| **Life Summary** | What mattered during one incarnation? | one completed Life |
| **Long-Horizon Summary** | What changed across a historical interval? | part of one Life, several Lives, Interlife, or an Age-scale interval |

They cross-reference through stable IDs. Neither copies the other's full content or implies current-character recall.

Retrieval may follow either route:

```text
Long-Horizon Summary -> relevant Life Summary -> source records
Life Summary -> relevant Long-Horizon Summary -> source records
```

## Canonical and Player Views

The canonical/GM summary may contain established Campaign Canon, protected references, uncertainty, and GM Secrets. A **Historical Transition** is a Derived Layer 3 presentation filtered through current Perspective, Player Knowledge, Character Knowledge, and secrecy boundaries.

A player-facing transition should concisely present relevant major changes, survivals, losses, open threads, and what the current character can know. It must not expose protected causes or imply that player access equals character memory.

Prefer deriving the presentation from one canonical summary rather than maintaining a second competing truth record.

## Retrieval and Relevant History Index

Search summary indexes before loading full text. Implementations should support queries by:

- timeframe, temporal precision, and Age;
- world, Contact Domain, region, location, and scope type;
- faction, species, Project, Infrastructure, Life ID, Soul ID, Entity ID, and Autonomous ID;
- major event, unresolved thread, continuity hook, and historical tag or theme.

A compact **Relevant History Index** exposes Historical Period ID, bounds, scope, key stable references, tags, major events, unresolved threads, status, version, and provenance. It is an index, not FR-011 Context Assembly and not current Canon.

FR-011 [Context Assembly](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) retrieves only relevant periods through these IDs and drill-down paths. Ordinary scenes do not require loading unrelated century summaries.

## Summary Nesting and Freshness

A broader period may reference smaller summaries. Rollups summarize rather than duplicate complete text. Every edge has a bounded role, and validation rejects self-reference and cycles.

Closed summaries are immutable except through authorized:

- factual correction;
- source recovery;
- retcon;
- migration.

Later interpretation or discovery normally creates new records and references. A justified summary revision increments its version and records prior version, reason, authority, sources, and migration or transaction provenance.

## Logical SQLite Contract

This repository defines a storage-neutral logical contract, not an executable campaign schema. A SQLite implementation should normalize equivalent structures:

```sql
CREATE TABLE historical_periods (
    historical_period_id TEXT PRIMARY KEY,
    scope_type TEXT NOT NULL,
    start_marker TEXT,
    end_marker TEXT,
    temporal_precision TEXT NOT NULL,
    age_start_id TEXT,
    age_end_id TEXT,
    compression_trigger TEXT NOT NULL,
    status TEXT NOT NULL CHECK (status IN ('open', 'closed', 'superseded')),
    created_provenance_id TEXT NOT NULL
);

CREATE TABLE long_horizon_summaries (
    historical_period_id TEXT NOT NULL,
    summary_version INTEGER NOT NULL CHECK (summary_version > 0),
    summary_text TEXT NOT NULL,
    causality_text TEXT,
    unknowns_text TEXT,
    finalized_at TEXT NOT NULL,
    provenance_id TEXT NOT NULL,
    supersedes_version INTEGER,
    PRIMARY KEY (historical_period_id, summary_version),
    FOREIGN KEY (historical_period_id) REFERENCES historical_periods(historical_period_id)
);

CREATE TABLE historical_period_references (
    historical_period_id TEXT NOT NULL,
    reference_type TEXT NOT NULL,
    referenced_record_id TEXT NOT NULL,
    historical_role TEXT NOT NULL,
    provenance_id TEXT NOT NULL,
    PRIMARY KEY (historical_period_id, reference_type, referenced_record_id, historical_role),
    FOREIGN KEY (historical_period_id) REFERENCES historical_periods(historical_period_id)
);

CREATE TABLE historical_period_tags (
    historical_period_id TEXT NOT NULL,
    tag TEXT NOT NULL,
    PRIMARY KEY (historical_period_id, tag),
    FOREIGN KEY (historical_period_id) REFERENCES historical_periods(historical_period_id)
);

CREATE TABLE historical_period_links (
    parent_period_id TEXT NOT NULL,
    child_period_id TEXT NOT NULL,
    link_role TEXT NOT NULL,
    PRIMARY KEY (parent_period_id, child_period_id, link_role),
    CHECK (parent_period_id <> child_period_id),
    FOREIGN KEY (parent_period_id) REFERENCES historical_periods(historical_period_id),
    FOREIGN KEY (child_period_id) REFERENCES historical_periods(historical_period_id)
);

CREATE INDEX idx_historical_period_bounds ON historical_periods(start_marker, end_marker);
CREATE INDEX idx_historical_period_refs ON historical_period_references(reference_type, referenced_record_id);
CREATE INDEX idx_historical_period_tags ON historical_period_tags(tag, historical_period_id);
```

Concrete schemas may normalize scope, temporal markers, causal links, continuity hooks, unresolved threads, visibility, and revisions further. Heterogeneous references require semantic validation or dedicated foreign-key tables. Do not serialize many-to-many references into comma-separated fields. SQLite tables, indexes, views, parameterized queries, and application query functions are valid; stored procedures are not assumed.

## Migration and Source Recovery

Existing campaigns adopt the structure through [Migration and Versioning](MIGRATION_AND_VERSIONING.md):

1. back up the latest canonical save;
2. audit Timeline, Campaign History, Life Archive, saves, and available source material without writing;
3. identify meaningful periods without manufacturing eventfulness;
4. assign stable Historical Period IDs;
5. generate summaries only from established Canon;
6. preserve Unknown, Estimated, Not Established, and Source Recovery Required gaps;
7. link Life, Entity, event, thread, and owner records;
8. validate chronology, scope, ownership, visibility, causality, references, nesting, and provenance;
9. activate only the validated migration.

Recovered material first updates its proper event or domain owner. Revise a summary only where the recovered fact materially changes its compressed statement, preserving revision provenance. This repository migrates no populated campaign.

## Validation

Validation detects or reports:

- duplicate or missing Historical Period IDs;
- incoherent start/end ordering at the claimed precision;
- unresolved scope, Life, Entity, event, thread, or nested-summary references;
- cycles in summary nesting;
- mutable current state claimed as summary-owned truth;
- unsupported facts replacing unknown history;
- significant causal outcomes whose established causal references were discarded;
- leaked GM Secrets or invalid Character Knowledge in player-facing views;
- World Reset summaries that erase earlier historical Canon;
- closed-period revisions lacking authority and provenance;
- migrated or recovered summaries lacking source provenance.

Validation reports gaps and conflicts; it does not invent repairs.

## Canonical Cases

### Peaceful Local Interval

Five years pass around a farming settlement. Established records show routine harvest variation and maintenance but no material transformation. The summary remains short and records the maintained continuity rather than inventing local crises.

### Forty-Year Conflict

Scarcity intensifies faction competition, mobilization disrupts trade, repeated conflict causes evacuation, and an abandoned settlement becomes a fortified border ruin. The summary preserves that causal chain and references the faction, economy, conflict, location, and Timeline owners.

### Reincarnation Gap

A Life ends and 120 years pass. Relevant factions, species pressures, infrastructure, and embodiment opportunities are resolved and saved before candidate generation. The period references both Life context and current world owners without becoming either.

### Sparse Unknown Interval

Sources establish only that a city existed before a fifty-year gap and was abandoned afterward. Cause, date, and sequence remain Source Recovery Required; the summary does not invent a war.

### Lost Autonomous Unit

An exploration servitor was last confirmed entering a ruin before a thirty-year gap. No later source resolves its fate. The summary preserves the last-confirmed report and `Current fate Unknown`.

### Long-Lived Companion

A two-thousand-year being remains an active Entity across several player Lives. Period summaries reference its verified actions and continuities rather than treating it as a generic legend or granting current incarnations memory.

### World Reset

A Reset changes Mana conditions, geography, and institutions. Earlier history remains true; surviving ruins, altered species pressures, and lost travel routes become explicit continuity hooks while current owners receive the new state.

### Filtered Transition

The canonical summary records that a hidden faction caused a succession crisis. The player-facing Historical Transition reports the public succession and resulting unrest but omits the protected cause.

## Safeguards

- Do not generate a summary after every ordinary turn.
- Do not freeze the world during a Time Skip or resolve every minor process.
- Do not let elapsed time complete Projects, erase Relationships, destroy autonomous units, or delete Infrastructure by itself.
- Do not turn a summary into current state, Character Knowledge, a Life Summary, or FR-011 context ownership.
- Do not fabricate dense history, exact chronology, causal certainty, or future GM Secrets.
- Do not silently rewrite closed summaries.
- Do not add populated periods or campaign history to this repository.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Simulation Abstraction](../world-engine/SIMULATION_ABSTRACTION.md)
- [Time Skip Procedure](../gm/TIME_SKIP_PROCEDURE.md)
- [Ages and World Resets](../world-engine/AGES_AND_WORLD_RESETS.md)
- [Reincarnation](../soul/REINCARNATION.md)
- [Life Archive](LIFE_ARCHIVE.md)
- [Autonomous Registry](AUTONOMOUS_REGISTRY.md)
- [Timeline Engine](TIMELINE_ENGINE.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Persistence Validation](PERSISTENCE_VALIDATION.md)
- [Long-Horizon Summary Template](../../templates/LONG_HORIZON_SUMMARY_TEMPLATE.md)
- [FR-010 Implementation Audit](../../design/audits/FR_010_LONG_HORIZON_SUMMARIES_AUDIT.md)
