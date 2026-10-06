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

The completed [FR-025 Execution Plan](FR_025_EXECUTION_PLAN.md) and [closure audit](audits/FR_025_ACQUISITION_AND_IMPORT_AUDIT.md) map integrated acquisition/import acceptance. [A acquisition](audits/FR_025A_ACQUISITION_ARCHITECTURE_AUDIT.md), [B validation/trust](audits/FR_025B_VALIDATION_TRUST_AUDIT.md), [C local](audits/FR_025C_LOCAL_PROVIDER_AUDIT.md), [D GitHub](audits/FR_025D_GITHUB_PROVIDER_AUDIT.md), [E custom conformance](audits/FR_025E_CUSTOM_PROVIDER_CONFORMANCE_AUDIT.md) and [F lossless import](audits/FR_025F_LOSSLESS_IMPORT_AUDIT.md) audits retain historical checkpoints. The completed [FR-026 Execution Plan](FR_026_EXECUTION_PLAN.md) retains A-H/R2 retrieval acceptance. The sole selected [FR-027 Execution Plan](FR_027_EXECUTION_PLAN.md) now records A contract/design complete, B-G pending; its [A audit](audits/FR_027A_CAMPAIGN_BINDING_GAMEPLAY_ENTRY_CONTRACT.md) defines later binding/entry/agency regression acceptance without claiming runtime enforcement.

The [FR-024 Execution Plan](FR_024_EXECUTION_PLAN.md) preserves completed vocabulary/audit checkpoints and their [closure evidence](audits/FR_024_CONTROLLED_RETRIEVAL_VOCABULARY_AUDIT.md). It does not create additional Future Revisions or select downstream runtime work.

The [Repository Audit Index](audits/README.md) lists repository, maintenance, and Release 1 reviews. An audit records evidence and documentation corrections; it cannot create gameplay mechanics or silently amend accepted governance.

## Authority Boundary

- `DECISIONS.md` governs accepted design outcomes.
- `TERMINOLOGY.md` governs canonical vocabulary.
- `ROADMAP.md` governs development progression.
- `UNRESOLVED_QUESTIONS.md`, `FUTURE_REVISIONS.md`, and `DEVELOPER_NOTES.md` are not playable rules.
- No populated save, character, inventory, quest, relationship, timeline, secret, or live world state belongs under `design/`.
