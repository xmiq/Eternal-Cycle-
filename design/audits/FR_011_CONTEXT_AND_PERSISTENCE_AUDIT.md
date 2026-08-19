# FR-011 Context Assembly and Turn Persistence Implementation Audit

## Scope

This audit records implementation of **FR-011 — GM/AI Context Assembly, Mandatory Read Discipline, and Gameplay Turn Persistence** only. FR-015 memory continuity and FR-016 Soul-bound fate remain pending.

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

The repository has no universal executable campaign host or populated SQLite save. Its enforceable outputs are canonical procedures, execution-profile obligations, adapter contracts, blank templates, and structural validation. Actual I/O conformance must be tested by each runtime host against its configured campaign persistence chain; the repository does not claim those external transactions were executed.

## Unresolved Issues

No blocking FR-011 design question remains. Runtime-specific query code, concrete campaign schema names, and populated conformance fixtures belong to external implementations and campaign deployments.

## Related Documents

- [Context Assembly and Gameplay Turn Persistence](../../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [Context Packet Template](../../templates/CONTEXT_PACKET_TEMPLATE.md)
- [Save Update Protocol](../../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Roadmap](../ROADMAP.md)
