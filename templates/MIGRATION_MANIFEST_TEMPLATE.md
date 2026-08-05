# Migration Manifest Template

Use this storage-neutral template to control one campaign persistence migration through Backup, Audit, Merge, and Validation without changing source material before evidence is preserved.

A populated Migration Manifest and every backup belong outside the repository. This blank template performs no migration, backup, merge, correction, or activation.

## Document Control

- **Template owner:** Migration History in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary procedural owner:** [Migration and Versioning](../docs/persistence/MIGRATION_AND_VERSIONING.md)
- **Dependencies:** source campaign, versions, complete backup, Audit Report, Merge Plan, Validation Run, conflicts, and activation authority
- **Extensions:** repository upgrades, storage migrations, source recovery, identity merge or split, rollback, and interrupted migration
- **Consumers:** Save Index, continuity resolution, validation, recovery, and campaign-version history
- **Repository boundary:** no campaign backup, source export, migration result, conflict, manifest instance, or storage credential belongs here

## Usage Guidance

1. Assign a unique Migration ID before work begins.
2. Complete and verify Stage 1 Backup before any edit.
3. Keep Stage 2 Audit read-only and classify source material without changing campaign state.
4. Merge only traceable affected records.
5. Validate the complete candidate before activation.
6. Preserve rollback and interruption instructions at every stage.

## Required Fields

### Manifest Identity

- **Migration ID:** `<unique stable ID>`
- **Campaign ID:** `<stable campaign reference>`
- **Repository Version:** `<source -> target>`
- **Campaign Version:** `<source -> candidate target>`
- **Persistence Model Version:** `<source -> target>`
- **Storage Format Version:** `<source -> target or unchanged>`
- **Source:** `<transcript, export, prior save, repository revision, storage system, or other source>`
- **Migration authority and roles:** `<initiator, auditor, merger, validator, activator>`
- **Current stage:** `<Backup | Audit | Merge | Validation | activation | complete | interrupted | rolled back>`
- **Started and last changed:** `<Timeline references>`

## Stage 1 - Backup

- **Backup ID:** `<unique ID>`
- **Backup date:** `<timestamp>`
- **Campaign Version:** `<captured version>`
- **Scope and contents:** `<complete continuity boundary>`
- **Storage locator:** `<external reference>`
- **Integrity verification:** `<method and result>`
- **Read-only confirmation:** `<result>`
- **Restore procedure:** `<authorized steps>`

No source edit may occur before this stage passes.

## Stage 2 - Audit

- **Audit Report ID:** `<stable ID>`
- **Sources read:** `<complete list and provenance>`
- **Source gaps:** `<missing, damaged, inaccessible, or conflicting material>`
- **Classified canonical events:** `<references>`
- **Research and knowledge:** `<references>`
- **Progression and capability:** `<references>`
- **Infrastructure and Inventory:** `<references>`
- **Relationships and identities:** `<references>`
- **Species, Locations, Factions, and world state:** `<references>`
- **Timeline and Campaign History:** `<references>`
- **Mysteries, retcons, and Meta:** `<separate classifications>`
- **Inconsistencies and warnings:** `<findings>`
- **Read-only audit confirmation:** `<result>`

## Stage 3 - Merge

- **Merge Plan ID:** `<stable ID>`
- **Affected Set:** `<records permitted to change>`
- **Audit trace per change:** `<source finding -> owner update>`
- **Identity merges or splits:** `<stable IDs, evidence, aliases, and dependent repairs>`
- **Truth Layer routing:** `<where each claim belongs>`
- **Conflicts:** `<classification, authority, disposition, and unresolved state>`
- **Files or records updated:** `<external locators>`
- **Unchanged records confirmation:** `<scope>`
- **Candidate Campaign Version:** `<version>`

## Stage 4 - Validation and Activation

- **Validation Run ID:** `<reference>`
- **Validation profile:** `Migration`
- **Results:** `<passed, passed with warnings, or failed>`
- **Relationships checked:** `<result>`
- **Timeline checked:** `<result>`
- **References and identities checked:** `<result>`
- **Projects and Research checked:** `<result>`
- **Species, Locations, Companions, and world records checked:** `<result>`
- **Knowledge and Secret separation checked:** `<result>`
- **Numerical changes checked:** `<result>`
- **Warnings and conflicts:** `<open findings>`
- **Activation authority and decision:** `<approved, blocked, rolled back, or pending>`
- **Activated Save Point:** `<ID or none>`

## Manifest Summary

- **Backup:** `<Backup ID>`
- **Files or records updated:** `<list>`
- **Validation results:** `<summary>`
- **Warnings:** `<summary>`
- **Conflicts:** `<summary>`
- **Notes:** `<bounded operational notes>`
- **Rollback reference:** `<backup and procedure>`
- **Final status:** `<complete | active with warnings | blocked | interrupted | rolled back>`

## Optional Fields

- **Storage-specific conversion notes:** `<implementation details that do not alter logical ownership>`
- **Integrity hashes or signatures:** `<verification references>`
- **Authorized branch or rehearsal:** `<nonactive candidate and disposal rules>`
- **Performance observations:** `<operational notes without canonical authority>`
- **Follow-up migration:** `<new Migration ID rather than silent extension>`

## Validation Notes

- [ ] Migration has one unique ID and separate version dimensions.
- [ ] A complete verified backup predates every edit.
- [ ] Audit was read-only and source-classified.
- [ ] Every merge change traces to the Audit Report and one owner.
- [ ] Meta, theories, rumours, and secrets were routed correctly.
- [ ] Validation ran after merge and before activation.
- [ ] Rollback remains possible until activation is accepted.
- [ ] Interrupted work records the exact safe resume point.

## Cross-References

- [Migration and Versioning](../docs/persistence/MIGRATION_AND_VERSIONING.md)
- [Continuity Resolution](../docs/persistence/CONTINUITY_RESOLUTION.md)
- [Save Index Template](SAVE_INDEX_TEMPLATE.md)
- [Validation Report Template](VALIDATION_REPORT_TEMPLATE.md)
- [Campaign History Template](CAMPAIGN_HISTORY_TEMPLATE.md)
