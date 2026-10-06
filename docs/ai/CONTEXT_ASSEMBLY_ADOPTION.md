# Context Assembly Adoption

Administrative host procedure; not a runtime Rule Source or an implemented adoption orchestrator. The host enforces the complete [Context Assembly](CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) I/O contract and [conformance](CONTEXT_ASSEMBLY_CONFORMANCE.md). The GM retains semantic compatibility, identity, retcon, consent, conflict and disclosure decisions; deterministic reads/checks/transactions do not decide them.

## Existing Campaign Adoption

An existing campaign adopts FR-011 through the normal migration and configuration process:

1. back up the latest canonical save;
2. validate the active Campaign Version and owner graph;
3. audit existing read procedures and preserve valid owner queries;
4. configure or regenerate context caches without rewriting Canon;
5. enable automatic turn transaction behavior in the execution profile or host;
6. verify adapter, validation, and read-back requirements;
7. run a harmless fixture or isolated validation transaction covering read, write, rollback, and reload;
8. record migration/setup provenance and activate only after validation.

This repository performs no migration of a populated campaign.


## Exceptional Boundary

For material conversion use [Migration and Versioning](../persistence/MIGRATION_AND_VERSIONING.md)'s Backup, Audit, Merge and Validation, [Authority Governance](../persistence/AUTHORITY_GOVERNANCE.md), and material specialist procedures. Phased administrative reasoning must reload exact protected, versioned evidence and approvals; isolated candidate work never activates partial Canon. Only validated, authorized, expected-parent atomic activation followed by required canonical verification permits dependent play. Lost acknowledgment is reconciled, never replayed. Indivisible decisions without complete fitting authority/evidence remain blocked. This document is a host contract, not an 8K model-packet or general adoption acceptance claim. The [R1.6 design](../../design/audits/FR_026H_R1_6_ADOPTION_WORKFLOW_ATOMICITY_REVIEW.md) supplies the reviewed later orchestration boundary; no executor or Campaign binding is implemented here.
