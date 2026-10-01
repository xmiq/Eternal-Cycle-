# Design and Governance Index

Files under `design/` govern project scope, accepted decisions, vocabulary, repository practice, open questions, future evidence, and non-canonical working notes. They do not contain playable campaign state. Playable rules remain under [`docs/`](../docs/README.md), and conflicts between rules and governance must be resolved in both places.

## Reading Order

1. [Development Roadmap](ROADMAP.md) - the single top-level Project Phase sequence, completed Release 1 history, current Phase 13 position, promoted objectives, status rules, dependencies, and completion authority.
2. [Design Decisions](DECISIONS.md) - authoritative accepted design governance.
3. [Canonical Terminology](TERMINOLOGY.md) - preferred rules vocabulary and definitions.
4. [Repository Conventions](REPOSITORY_CONVENTIONS.md) - authority, ownership, structure, linking, and change discipline.
5. [Unresolved Questions](UNRESOLVED_QUESTIONS.md) - blocking and non-blocking questions that have not become accepted decisions.
6. [Future Revisions](FUTURE_REVISIONS.md) - Phase 13 evidence candidates for later owner-authorized post-release work.
7. [v1.1 Future Revision Plan](V1_1_FUTURE_REVISION_PLAN.md) - bounded FR-022 through FR-036 task graph, dependencies, acceptance criteria, and complete planning traceability.
8. [Developer Notes](DEVELOPER_NOTES.md) - non-canonical checkpoints, watchlists, alternatives, and resume instructions.
9. [Development Feedback Method](DEVELOPMENT_METHOD.md) - the observe, classify, fix, and evidence loop for reference, Managed-contract, and Eternal Cycle-wide findings.
10. [Release and Version Provenance](RELEASE_VERSIONING.md) - immutable full releases, moving RC discovery, source identity, and display metadata.

## Audits

The [FR-024 Execution Plan](FR_024_EXECUTION_PLAN.md) bounds the currently selected vocabulary/audit objective into independently validated checkpoints. It does not create additional Future Revisions or authorize downstream runtime work.

The [Repository Audit Index](audits/README.md) lists repository, maintenance, and Release 1 reviews. An audit records evidence and documentation corrections; it cannot create gameplay mechanics or silently amend accepted governance.

## Authority Boundary

- `DECISIONS.md` governs accepted design outcomes.
- `TERMINOLOGY.md` governs canonical vocabulary.
- `ROADMAP.md` governs development progression.
- `UNRESOLVED_QUESTIONS.md`, `FUTURE_REVISIONS.md`, and `DEVELOPER_NOTES.md` are not playable rules.
- No populated save, character, inventory, quest, relationship, timeline, secret, or live world state belongs under `design/`.
