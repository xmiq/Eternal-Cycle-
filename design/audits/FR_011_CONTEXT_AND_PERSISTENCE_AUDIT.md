# FR-011 Context Assembly and Turn Persistence Implementation Audit

## Scope

This audit records implementation and the gameplay-validation regression repair of **FR-011 — GM/AI Context Assembly, Mandatory Read Discipline, and Gameplay Turn Persistence** only. FR-015 memory continuity and FR-016 Soul-bound fate are complete under their own owners and are not changed here.

## Observed Regression

A live runtime preserved canonical read discipline and narrated coherent multi-domain changes, yet returned ordinary completed gameplay responses without executing the required persistence transaction. The canonical SQLite save therefore remained at its prior version while conversation text described newer Canon.

## Root Cause

The original contract stated that persistence preceded turn closure, but its execution boundary used a coarse `SAVE_COMPLETE` concept without requiring an evidence-bearing configured-authority state. It did not distinguish local completion from cloud-authoritative completion, expose a truthful player status, or make unchanged canonical state an explicit hard failure. A host could therefore treat narrative resolution as response completion while leaving the save obligation implicit.

## Completion-Gate Repair

The repaired state machine resolves the configured persistence target before state-changing play, completes required reads, resolves the interaction, determines an explicit Affected Set, writes each canonical owner once, validates and reads back the expected change, verifies the configured local or cloud authority, and only then refreshes Derived context and reaches `TURN_COMPLETE`. A non-empty Affected Set makes this sequence mandatory.

The exact target comes from Campaign Configuration and the Save Index. A missing expected Local Working Copy does not authorize a missing-database report or a blank replacement when configured remote canonical storage may exist.

## Player-Visible Status and Commands

- `💾` means the configured local canonical target committed and validated.
- `☁️💾` means the configured cloud canonical target synchronized and passed required remote verification.
- `⏳` means required persistence remains pending and blocks later state-changing play.
- `⚠️` means write, synchronization, expected-change, or validation failure.

The `save`, `save status`, and `retry save` commands use the same transaction state. Retry resumes the incomplete stage when possible and cannot replay narrative resolution or duplicate owner writes, costs, progression, items, Relationship updates, or chronology.

## Unchanged-Save Detection

When the Affected Set is non-empty, validation requires minimum sufficient evidence such as a version or revision increment, changed expected rows, chronology append, file/hash change, or manifest update. If expected canonical state remains unchanged, validation fails, ordinary turn completion is prohibited, and the status is `⚠️`.

## Regression Results

The executable implementation-neutral harness covers multi-domain automatic saving, next-turn canonical reload, remote target discovery from a missing local copy, local-only completion, verified cloud completion, pending cloud state, cloud failure, false cloud-success prevention, unchanged-save rejection, manual saving, status inspection, and idempotent cloud retry. Full repository validation executes this harness.

## Existing Read Behaviour Audited

The Campaign State Model already required Save Index-first, owner-routed, dependency-closed Read Sets. AI Session Start, AI GM Workflow, the runtime model, and adapter procedures already rejected conversation memory as authority. FR-011 preserves those rules and adds relevance metadata, freshness checks, Current Scene Context, deep-read paths, and a mandatory `READ_COMPLETE` gate.

## Context Assembly Architecture

The canonical contract defines a storage-neutral, SQLite-compatible Context Assembly Layer using views, parameterized queries, application query functions, indexed lookups, or disposable Derived caches. It introduces Current Scene Context, Recent Running Summary, and Session Summary while preserving Long-Horizon Summary, Life Summary, and full-record drill-down.

Packets carry stable IDs, owner domains, source revisions, relevance reasons, reference paths, audience scope, and parent Save Point metadata. They own no campaign facts.

## Mandatory Reads and Automatic Persistence

The Gameplay Turn state machine prevents material resolution before required reads and prevents state-changing turn closure before automatic Save Update success. Every resolved interaction receives an explicit Affected Set determination. Non-empty sets update authoritative owners exactly once, append chronology under existing rules, validate, perform required read-back, activate a Save Point, and then refresh Derived context. Empty sets cause no mutation only after verification.

## Failure, Retry, and Reload

Read failure cannot be replaced by conversational recollection. Write failure preserves the parent Save Point, Interaction ID, Transaction ID, staged operation set, and recovery evidence. Retry remains idempotent. Turn N+1 must query relevant owner state from the active persisted Save Point and invalidate caches tied to an older version.

## Specialist Integration

- FR-004 supplies relevant Life IDs and summary-to-detail paths without granting memory.
- FR-010 supplies relevant Historical Period IDs and continuity hooks without loading unrelated history.
- FR-012 remains the source of one-owner routing and Derived/Cache boundaries.
- FR-014 supplies selected relevant autonomous records without broad registry loading.
- FR-015 and FR-016 remain unimplemented compatibility boundaries.

## Existing Campaign Adoption

Adoption is backup-first and non-destructive: validate the existing owner graph, preserve working read queries, configure or regenerate caches, enable the execution-profile turn gate, test read/write/rollback/reload against a harmless fixture, and retain migration provenance. No populated campaign was migrated by this repository task.

## Regression Coverage

The canonical contract defines cases for Skill reads, automatic character and Relationship saves, multi-domain atomic writes, Autonomous Registry updates, verified no-change turns, failed writes, conversation/Canon conflicts, next-turn reload, stale cache regeneration, targeted historical drill-down, and context-reset recovery. Repository validation asserts the contract, template, navigation, governance closure, and pending-objective boundaries.

## Runtime Boundary

The repository has no universal executable campaign host or populated SQLite save. Its enforceable outputs are canonical procedures, execution-profile obligations, adapter contracts, blank templates, structural validation, and the implementation-neutral state-machine harness. The host remains responsible for invoking actual connectors and file writes and for supplying adapter evidence. No host may emit `💾` or `☁️💾` without that evidence, and actual I/O conformance must still be tested against the configured campaign persistence chain.

## Unresolved Issues

No blocking FR-011 design question remains. Runtime-specific query code, concrete campaign schema names, and populated conformance fixtures belong to external implementations and campaign deployments.

## Related Documents

- [Context Assembly and Gameplay Turn Persistence](../../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [Context Packet Template](../../templates/CONTEXT_PACKET_TEMPLATE.md)
- [Save Update Protocol](../../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Roadmap](../ROADMAP.md)
