# Context Assembly Conformance

Non-runtime host conformance and optional implementation guidance. [Runtime Context](CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) remains the executable owner; [Adoption](CONTEXT_ASSEMBLY_ADOPTION.md) owns setup. Each case asserts those existing duties, not additional player-turn authority. Schema guidance owns only optional cache structure. Cases A-Y and all acceptance predicates remain mandatory when claiming the applicable host capability; no mock proves external I/O.

## Storage-Neutral Logical Guidance

Direct formats or Managed services may persist packet metadata and transactional provenance using equivalent normalized structures:

```sql
CREATE TABLE context_packets (
    context_packet_id TEXT PRIMARY KEY,
    context_kind TEXT NOT NULL CHECK (context_kind IN ('scene', 'running', 'session')),
    interaction_id TEXT,
    parent_campaign_version TEXT NOT NULL,
    parent_save_point_id TEXT NOT NULL,
    audience_scope TEXT NOT NULL,
    generated_game_time TEXT,
    generated_at TEXT NOT NULL,
    freshness_status TEXT NOT NULL CHECK (freshness_status IN ('fresh', 'stale', 'invalid')),
    provenance_id TEXT NOT NULL
);

CREATE TABLE context_packet_references (
    context_packet_id TEXT NOT NULL,
    owner_domain TEXT NOT NULL,
    record_id TEXT NOT NULL,
    source_revision TEXT,
    relevance_reason TEXT NOT NULL,
    reference_path TEXT,
    PRIMARY KEY (context_packet_id, owner_domain, record_id),
    FOREIGN KEY (context_packet_id) REFERENCES context_packets(context_packet_id)
);

CREATE INDEX idx_context_packet_parent
    ON context_packets(parent_campaign_version, parent_save_point_id, freshness_status);
```

Context content may remain transient or be serialized separately. These optional Cache structures own only derivation metadata and references. They do not own copied campaign facts. Concrete campaign schemas may use established IDs and names instead.

## Regression Cases

### A. Skill Read

An action invokes a Skill. The runtime reads the current Skill identity, Development, embodiment, access, condition, and relevant opposition before resolution.

### B. Automatic Character Save

A turn changes body condition. The Entity/body owner updates automatically without the player saying `save`, validates, and is read from persistence next turn.

### C. Relationship Save

A promise changes a Relationship. The Relationship owner updates in the same automatic transaction; character or session summaries only reference it.

### D. Multiple-Domain Transaction

Movement while consuming an item changes Entity condition, Inventory, current placement, and Timeline. One Interaction ID produces one dependency-complete transaction, and every effect appears exactly once.

### E. Autonomous Entity

An autonomous unit receives and accepts a new assignment. Relevant Autonomous Registry and Project references update; unrelated autonomous units are not loaded.

### F. No-Change Turn

A perception request reveals nothing new and advances no meaningful time. The runtime records `Affected Set = empty` and performs no mutation.

### G. Failed Write

Adapter validation fails. The parent Save Point remains active, the turn does not close as saved, and retry retains transaction identity.

### H. Contradictory Conversation

Conversation says an item remains; authoritative Inventory says it was consumed. The persisted owner wins and the packet is regenerated.

### I. Next-Turn Reload

Turn N changes location and commits. Turn N+1 queries current placement and retrieves the new Location ID before resolution.

### J. Stale Running Summary

A Running Summary references an earlier Campaign Version. It is invalidated and rebuilt rather than used to overwrite Canon.

### K. Deep Historical Query

Immediate context omits old history. A question about a former companion follows Entity, Life, Historical Period, Relationship, and source IDs only as far as needed.

### L. Context Reset

A fresh runtime receives no old transcript. It loads the Save Index, verifies summaries, builds the relevant Read Set, and resumes from persisted state.

### M. Multi-Domain Automatic Save

One interaction changes Infrastructure, Research, equipment, network communication, an Autonomous Registry assignment, exploration Knowledge, Timeline, and Campaign History. The runtime reads the relevant owners, persists one dependency-complete Affected Set without a `save` command, proves the version and expected owner changes, reloads them next turn, and ends the delivered response with `💾` or `☁️💾` according to configured authority. Narration-only change fails this case.

### N. Canonical Target Discovery

The first assumed local path is absent while Campaign Configuration identifies a remote canonical database. The runtime resolves exact remote identity, fetches it, verifies campaign and version, creates only a Local Working Copy, and writes through the configured chain. It neither reports a false missing database nor creates a blank replacement.

### O. Local-Only Completion

A local-authoritative SQLite campaign commits, validates, reopens read-only, proves expected changes, activates its Save Point, and displays `💾`.

### P. Cloud-Authoritative Completion

SQLite candidate validation succeeds, Google Drive replaces the exact canonical file, remote read-back and required comparison succeed, configured backup verification succeeds, and the response displays `☁️💾`.

### Q. Cloud Pending and Failure

When local validation succeeds but required cloud work is incomplete, status is `⏳` and another state-changing turn is blocked. When synchronization or verification fails, status becomes `⚠️`; the validated local candidate remains available for retry but is not the active cloud authority.

### R. False Cloud Success Prevention

An upload request returns but remote read-back never verifies the candidate. The runtime cannot display `☁️💾`, cannot close the state-changing turn, and retains failure evidence.

### S. Manual Save and Status

`save` executes a pending transaction once without replaying the interaction. `save status` reports marker, version, configured authority, local/cloud state, pending state, and last verified boundaries without exposing protected data.

### T. Idempotent Retry

Cloud synchronization fails after a validated local commit. `retry save` resumes at remote deployment using the same Transaction ID, does not rerun owner writes, and reaches `☁️💾` only after remote verification.

### U. Unchanged Canonical Save

A non-empty Affected Set is narrated, but expected canonical owner rows, chronology, Campaign Version, and artifact evidence remain unchanged. Validation reports failure, status becomes `⚠️`, and `TURN_COMPLETE` is prohibited.

### V. Managed Receipt Completion

A Managed-authoritative campaign stages, validates, activates, and reads back a multi-domain transaction. The service returns completion evidence naming the expected Transaction ID and Campaign Version. The runtime displays `💾`; it does not expose or classify backend deployment.

### W. Managed Missing Receipt

The configured interface call returns but no validated completion evidence exists. The runtime displays `⏳` or `⚠️` according to service state, prohibits `TURN_COMPLETE`, and retries by the original Transaction ID without replaying gameplay.

### X. Direct DuckDB Conflict

A Direct DuckDB writer encounters an optimistic concurrency conflict. The candidate fails, the runtime reloads the active parent, and no last-writer-wins overwrite or false `💾` occurs.

### Y. World-Specific Rule Retrieval

A campaign bound to World A resolves the Runtime Rule Kernel, relevant Eternal Cycle Core rules, relevant World A and operation/topic rules, and Campaign A Canon. World B rules and another campaign's Canon are excluded, source provenance is retained, and the compiled-rule portion remains within the 8K normal-play target.

## Acceptance Criteria

FR-011 conformance requires all of the following:

- ordinary play requires no manual save command;
- every non-empty Affected Set persists automatically;
- material canonical reads occur before resolution;
- conversation context never substitutes for an authoritative read;
- failed reads and writes remain explicit;
- later turns retrieve prior committed changes when relevant;
- Derived packets cannot override Canon;
- packets expose relevant stable IDs, owners, and drill-down references internally;
- selection remains relevance-filtered;
- fresh sessions rebuild context from persistence;
- the configured canonical target is resolved before state-changing play and is never guessed from one local path;
- a non-empty Affected Set proves expected canonical change before completion;
- local-authoritative success displays `💾` only after local validation and read-back;
- cloud-authoritative success displays `☁️💾` only after required synchronization and remote verification;
- Managed-authoritative success displays `💾` only with validated service completion evidence and never exposes backend topology;
- Direct and Managed are selected explicitly, persisted, reused, and cannot silently fall back to each other;
- `⏳` and unresolved `⚠️` block subsequent state-changing play;
- manual `save`, `save status`, and `retry save` preserve idempotency;
- Derived context refresh follows canonical verification;
- host tests exercise Regression Cases A through U against the configured persistence mode;
- mode-specific host tests also exercise Regression Cases V through X;
- rule-retrieval tests exercise Regression Case Y, source provenance, and the 8K world-isolation target;
- the repository regression harness passes its mock-adapter state-machine cases.


## Owners

Cases A-L: runtime mandatory reads, automatic persistence, relevance, retry, freshness and next-turn/reset gates. M-U: Affected Set, target discovery, authority evidence, manual commands and unchanged-save detector. V-X: Managed receipt and Direct concurrency gates. Y: dependency-complete scoped Rule Context. Acceptance predicates repeat these owners rather than granting new mechanics.
