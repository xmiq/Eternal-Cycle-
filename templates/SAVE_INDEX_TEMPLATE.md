# Save Index Template

Use this storage-neutral template as the control-plane entry point for one campaign continuity. It identifies what authoritative records exist, which versions apply, and whether a save is safe to load without copying module contents.

A populated Save Index belongs outside the repository. This blank template creates no campaign, save point, module state, participant authority, backup, or validation outcome.

## Document Control

- **Template owner:** Save Index and Protocol in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary procedural owners:** [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md), [Migration and Versioning](../docs/persistence/MIGRATION_AND_VERSIONING.md), and [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
- **Dependencies:** Campaign Canon, module registry, versions, Save Points, validation, migration, backups, authority, and visibility
- **Extensions:** storage locators, automation metadata, participant interfaces, and implementation-specific indexes
- **Consumers:** session start, GM workflow, Save Updates, migration, continuity resolution, validation, and recovery
- **Repository boundary:** no campaign ID, title, save, module locator, participant, warning, backup, or live version state belongs here

## Usage Guidance

1. Load this index before any campaign adjudication.
2. Register logical modules and partitions, not copies of their contents.
3. Reference exact repository, campaign, persistence-model, and storage-format versions separately.
4. Preserve authority and least-necessary visibility for every locator.
5. Do not activate a Save Point while a required migration, conflict, or blocking validation finding remains open.

## Required Fields

- **Save Index ID:** `<stable ID>`
- **Campaign ID and title:** `<stable campaign identity and display label>`
- **Campaign continuity boundary:** `<what belongs to this continuity>`
- **Campaign Version:** `<active version>`
- **Repository Version:** `<exact rules revision>`
- **Rules Profile:** `<Campaign Canon reference>`
- **Persistence Model Version:** `<logical schema version>`
- **Storage Format Version:** `<implementation version or not applicable>`
- **Last confirmed Save Point:** `<stable ID and effective time>`
- **Configured canonical authority:** `<local | cloud and adapter-chain reference>`
- **Resolved canonical target:** `<verified locator reference or unresolved>`
- **Local persistence state:** `<current | ahead of cloud | pending | failed | not applicable>`
- **Cloud persistence state:** `<current and verified | pending | failed | not configured>`
- **Last successful local commit:** `<version and time or unavailable>`
- **Last successful cloud verification:** `<version and time or unavailable>`
- **Current Session state:** `<none | open | interrupted | pending integration>`
- **Latest Validation Run:** `<ID, outcome, and warnings>`
- **Latest Migration:** `<ID or none>`
- **Available backup references:** `<IDs and locations>`
- **Load status:** `<safe | safe with warnings | blocked | unknown>`

## Module Registry Entry

- **Module name:** `<logical module>`
- **Authoritative owner:** `<one owner>`
- **Record or partition locator:** `<storage-neutral reference>`
- **Schema or representation version:** `<version>`
- **Truth Layers present:** `<layers>`
- **Visibility and authorized interfaces:** `<actors or tools and scope>`
- **Dependencies:** `<typed module references>`
- **Last integrated Transaction:** `<ID>`
- **Last validated:** `<Validation Run ID>`
- **Status:** `<available | partial | missing | migrating | blocked>`

## Open Control State

- **Pending Session Delta:** `<ID or none>`
- **Open Save Transaction:** `<ID or none>`
- **Pending Affected Set:** `<owner-domain references or none>`
- **Persistence status:** `<local validated | cloud validated | pending | failed>`
- **Unresolved conflicts:** `<IDs, owners, and severity>`
- **Record Gaps:** `<IDs and required source recovery>`
- **Pending migrations:** `<IDs and activation state>`
- **Continuity warnings:** `<finding references>`
- **Recovery instructions:** `<last safe point and authorized procedure>`

## Optional Fields

- **Authorized GM interfaces:** `<scope and visibility>`
- **Participant interfaces:** `<player-facing views and permissions>`
- **Derived View registry:** `<non-authoritative outputs and freshness>`
- **Ownership map reference:** `<canonical ownership contract and implementation mapping>`
- **Branch or concurrency state:** `<authorized branches and merge requirements>`
- **Storage integrity metadata:** `<hashes, manifests, or implementation-specific checks>`

## Validation Notes

- [ ] Every registered module has one authoritative owner and a resolvable locator.
- [ ] Every mutable fact family maps to one logical owner; references, caches, summaries, and Historical Snapshots are classified separately.
- [ ] Versions are explicit and not collapsed into one number.
- [ ] The active Repository Version matches Campaign Canon or has a pending migration.
- [ ] No module contents are duplicated into the index.
- [ ] Visibility metadata leaks no protected Secret.
- [ ] Open transactions, deltas, migrations, conflicts, gaps, and warnings are represented honestly.
- [ ] The configured canonical authority is explicit, its exact target is resolved before state-changing play, and no missing Local Working Copy is mistaken for a missing remote canonical save.
- [ ] Local and cloud completion states reflect actual commit and verification evidence; a cloud-authoritative save is not current merely because the local transaction committed.
- [ ] The stated load status matches the latest validation outcome.

## Cross-References

- [Persistence Authority](../docs/persistence/PERSISTENCE_AUTHORITY.md)
- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [Campaign Canon Template](CAMPAIGN_CANON_TEMPLATE.md)
- [Migration Manifest Template](MIGRATION_MANIFEST_TEMPLATE.md)
- [Validation Report Template](VALIDATION_REPORT_TEMPLATE.md)
