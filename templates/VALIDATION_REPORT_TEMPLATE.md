# Persistence Validation Report Template

Use this storage-neutral template to record one read-only validation run, its reproducible scope, findings, outcome, and owner-routed repair references without changing campaign facts.

A populated report belongs outside the repository. This blank template performs no check, repairs no record, resolves no conflict, and authorizes no Save Point.

## Document Control

- **Template owner:** Validation in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary procedural owner:** [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
- **Dependencies:** baseline, Validation Profile, scope, check definitions, evidence, findings, versions, and repair owners
- **Extensions:** Save Activation, Migration, Continuity Resolution, Load Integrity, and Periodic Full Audit Profiles
- **Consumers:** Save Index, Save Updates, migration, continuity resolution, repair workflows, and recovery
- **Repository boundary:** no campaign Validation Run, finding, warning, conflict, repair, or activation outcome belongs here

## Usage Guidance

1. Establish a frozen Validation Baseline before checking.
2. Select the narrowest profile that covers the decision.
3. Evaluate read-only with least-necessary visibility.
4. Record evidence before assertions and route repairs to authoritative owners.
5. Re-run affected checks after repair; never edit truth from inside validation.

## Required Fields

### Validation Run

- **Validation Run ID:** `<unique stable ID>`
- **Campaign ID:** `<stable reference>`
- **Profile:** `<Save Activation | Migration | Continuity Resolution | Load Integrity | Periodic Full Audit>`
- **Purpose and scope:** `<records, modules, intervals, and checks>`
- **Validation Baseline:** `<Save Point, Campaign Version, Repository Version, and snapshot reference>`
- **Persistence and storage versions:** `<references>`
- **Validator and authority:** `<person or tool plus authorized visibility>`
- **Started and completed:** `<timestamps>`
- **Reproduction information:** `<inputs, configuration, and check versions>`
- **Overall outcome:** `<Passed | Passed with Warnings | Failed | Incomplete>`

## Check Register

- **Check ID:** `<stable definition ID>`
- **Category:** `<structure | reference | duplicate | authority | version | relationship | timeline | knowledge | Research | numerical | identity | Inventory | Infrastructure | Project | Mystery | world | continuity drift | other>`
- **Scope:** `<records and layers examined>`
- **Required visibility:** `<least necessary>`
- **Method:** `<reproducible read-only procedure>`
- **Result:** `<passed | warning | failed | not run | inconclusive>`
- **Evidence:** `<record and source references>`
- **Finding IDs:** `<zero or more references>`

## Validation Finding

- **Finding ID:** `<unique ID>`
- **Severity:** `<Informational | Warning | Error | Blocking>`
- **Claim:** `<exact inconsistency, gap, risk, or leak>`
- **Affected records:** `<typed references>`
- **Evidence:** `<sources and reproduction>`
- **Authoritative owner:** `<record owner responsible for repair decision>`
- **Truth Layers involved:** `<visibility and leak considerations>`
- **Required disposition:** `<accept | investigate | repair | recover source | authorize retcon | block activation | other valid route>`
- **Repair reference:** `<separate Transaction, Migration, or owner action>`
- **Status:** `<open | accepted | repaired pending recheck | resolved | waived with authority>`
- **Recheck result:** `<Validation Run reference or none>`

## Required Coverage Summary

- **Required records and indexes:** `<result>`
- **Broken, dangling, and orphaned references:** `<result>`
- **Duplicate actors, places, discoveries, and identities:** `<result>`
- **Authority, ownership, Truth Layers, and visibility:** `<result>`
- **State and version consistency:** `<result>`
- **Relationships and recognition:** `<result>`
- **Timeline and Campaign History:** `<result>`
- **Knowledge, Research, theories, and Secrets:** `<result>`
- **Numerical and mechanical changes:** `<result>`
- **Soul identity and Reincarnation:** `<result>`
- **Inventory, custody, and Infrastructure:** `<result>`
- **Projects and Mysteries:** `<result>`
- **Species, Locations, Factions, and world state:** `<result>`
- **Continuity drift:** `<result>`

## Report and Disposition

- **Warnings:** `<open findings and operational effect>`
- **Blocking findings:** `<IDs>`
- **Repairs completed:** `<external owner actions>`
- **Checks re-run:** `<IDs and results>`
- **Residual risk:** `<known limitations>`
- **Save activation recommendation:** `<permit | permit with warnings | block | not applicable>`
- **Next required validation:** `<trigger or schedule>`

## Optional Fields

- **Automation log:** `<external locator and safe diagnostic summary>`
- **Performance metrics:** `<duration and resource observations without affecting truth>`
- **Sampled records:** `<when a permitted check uses sampling, with limitations>`
- **Comparison to prior run:** `<new, persistent, repaired, or regressed findings>`
- **Supplemental checks:** `<project-specific checks that preserve the canonical profile>`

## Validation Notes

- [ ] The run has a frozen baseline, reproducible scope, and exact versions.
- [ ] Evaluation was read-only and used least-necessary visibility.
- [ ] Every finding includes evidence, severity, owner, and disposition.
- [ ] Validation itself changed no campaign fact.
- [ ] Repairs occurred through owner-controlled transactions or migrations.
- [ ] Protected information did not leak through the report.
- [ ] The overall outcome matches open finding severity.
- [ ] Activation remains blocked when required checks fail.

## Cross-References

- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Migration Manifest Template](MIGRATION_MANIFEST_TEMPLATE.md)
- [Save Index Template](SAVE_INDEX_TEMPLATE.md)
- [Continuity Resolution](../docs/persistence/CONTINUITY_RESOLUTION.md)
