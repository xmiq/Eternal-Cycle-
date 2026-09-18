# FR-021 Rule Source Live-Acceptance Repair Audit

## Scope

This audit records owner-authorized corrective maintenance to completed FR-021 after an interrupted implementation pass and later clarifications. It creates no new Future Revision, changes no gameplay mechanic, leaves `VERSION` at `1.0.0`, and does not rewrite immutable release history.

## Interrupted-Work Recovery

The resumed tree already contained most of the focused repair: propagated cancellation, durable-owner cancellation classification, real Operation/Correlation identity, operation-aware Error Dump lookup, migration 008, safe effective configuration reporting, source-acquisition diagnostics, a simplified Git provider, regression tests, and initial operator documentation. That work was reviewed and retained rather than restarted.

The remaining integration work corrected retry metadata and timeout elapsed reporting, completed source-freedom and artifact-boundary documentation, justified migration 008, added governance and this audit, completed distribution tooling, and reran validation.

## Confirmed Faults

Repository review confirmed these defects in the prior reference implementation:

- a publication layer could convert `OperationCanceledException` into an ordinary cancelled result before the durable operation owner classified it;
- lower source work could create unrelated standalone correlation identity instead of receiving the durable Operation ID and Correlation ID;
- persisted diagnostics had no field for the durable Operation ID and could be queried only indirectly by correlation;
- configuration discovery described safe defaults but did not consistently expose effective non-sensitive values;
- a missing Git ref used generic retry semantics instead of requiring corrected input;
- the retained no-checkout cache stored Git repository machinery unrelated to compiler input;
- the repository lacked deterministic tooling for a full useful external-review archive.

The approximately 6 minute 45 second cancellation observed in the external client is not assigned a root cause. Available evidence does not prove GitHub latency, repository size, network or proxy behavior, cache shape, a missing ref, or any other hypothesis. The repaired diagnostics preserve observed cancellation ownership and leave unknown causes unknown.

## Repair

### Durable cancellation and identity

Source and publication layers now propagate cancellation. The durable operation processor distinguishes host shutdown, its own Managed Operation timeout, overall acquisition timeout, individual Git-process timeout, and an otherwise unexplained parent cancellation. Service-owned work remains independent of the initiating MCP request.

Every publication stage receives the durable Operation ID and Correlation ID. Structured diagnostics, operation status, and Error Dumps can be looked up by Operation ID, Correlation ID, or both. Safe Git observations report command category, elapsed time, timeout scope, outcome, and observed cancellation source without exposing raw arguments, credentials, or private locators.

### Runtime Rule Source acquisition

The reference Git provider follows a bounded conventional flow: locate the selected source, resolve the requested ref to an immutable commit SHA, read the manifest and its declared files, materialize those files as ordinary files, validate, compile, and publish. Remote Git object storage is temporary. The persistent cache retains only the manifest-defined rule payload and compatibility/provenance metadata.

An existing local clone is inspected in place and passes through the same technical validation pipeline. The packaged source is a default, not a policy privilege. Compatible forks, mirrors, modified rules, house rules, and future providers remain legitimate sources; compatibility validation does not enforce semantic equality with official Eternal Cycle rules.

### Distribution archive

The Distribution Archive is deliberately separate from runtime acquisition. `tools/build_distribution.ps1` creates a deterministic full useful repository snapshot containing tracked and useful untracked source, documentation, reference implementations, tests, migrations, manifests, templates, license, notice, version, and distribution metadata. It excludes `.git`, `bin`, `obj`, caches, test output, logs, temporary files, credentials, and prior archives. `tools/test_distribution_archive.ps1` verifies required review material and rejects forbidden generated paths.

## Migration 008

Migration `008_diagnostic_operation_correlation` is retained. Migration 005 persisted `correlation_id` but could not preserve the distinct durable `operation_id` now carried by the diagnostic contract. Deriving every lookup through the operation store would lose the direct diagnostic identity when operation evidence is unavailable and would not persist the actual identifier emitted by the durable owner.

The migration adds one nullable `nvarchar(128)` column and one filtered index. Existing diagnostic rows remain valid with null Operation IDs. Both fixed-`ec_domain` and schema-template variants are packaged. No campaign table or campaign data changes. The migration is repeat-safe at the schema level; live LocalDB execution could not be proven in this run because the installed LocalDB instance failed to start.

## Architectural Classification

- **A — Reference implementation detail:** .NET process execution, Git commands, temporary acquisition repositories, filesystem payload layout, T-SQL migration syntax, PowerShell ZIP tooling, and SQL index implementation.
- **B — Managed Service / Rule Source Provider contract:** bounded acquisition and subprocess execution, technical source validation, immutable source provenance, propagated cancellation, durable Operation/Correlation identity, operation-aware diagnostics, and manifest-defined payload materialization.
- **C — Eternal Cycle-wide invariant:** users choose compatible rules; official rules are a default rather than a semantic gate; unknown causes remain unknown; runtime compiler payload and external-review distribution are different artifacts.

## Validation Evidence

- Release build: passed with zero warnings and zero errors.
- reference-service automated tests: 120 passed, 0 failed, 0 skipped.
- opt-in LocalDB fixture: attempted in restricted and unrestricted execution; both failed before product SQL ran because the LocalDB automatic instance could not start. This is recorded as unproven real-infrastructure validation, not a product pass.
- FR-011 persistence-gate harness: 13 assertions passed.
- FR-017 portable-persistence harness: 29 assertions passed.
- FR-018 rule-compilation harness: 19 assertions passed.
- FR-019 Managed-data architecture harness: 48 assertions passed after replacing its stale cache-shape assertion.
- FR-020 first-run harness: 59 assertions passed after replacing its historical retained-`--no-checkout` assertion with the minimal-payload contract.
- FR-021 durable-operation/live-acceptance harness: 62 assertions passed.
- FR-021 control-plane repair harness: 35 assertions passed.
- release-neutral campaign-mode harness: 17 assertions passed.
- repository validation: passed across 285 Markdown files, 7,138 relative links, 163 anchors, 187 indexed canonical documents, 43 templates, 1,260 terminology checks, 193 roadmap tasks, and 21 Future Revision entries.
- deterministic distribution check: two unchanged-tree builds produced matching hashes; required review content was present and forbidden generated paths were absent.

## Remaining Live Acceptance

Repository validation does not prove:

- the original external cancellation source;
- remote Git host latency, authentication, proxy, or credential behavior;
- migration 008 execution on a working LocalDB or production SQL Server;
- LM Studio or Unsloth Studio MCP interoperability;
- live service restart during acquisition or publication;
- remote moving-RC discovery and publication until the candidate is pushed and verified.

## Result

FR-021 remains complete with corrective provenance. Phase 13 remains active and Future Revisions remains `[∞]`. No later objective is selected automatically.

## Related Documents

- [Managed Rule Publication](../../docs/rules/MANAGED_RULE_PUBLICATION.md)
- [Managed Operations](../../docs/persistence/MANAGED_OPERATIONS.md)
- [Running Eternal Cycle](../../docs/persistence/RUNNING_ETERNAL_CYCLE.md)
- [Reference Managed Service](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md)
- [MCP-Only Acceptance Test](../../examples/tooling/managed-data/mcp-dotnet-tsql/MCP_ONLY_ACCEPTANCE_TEST.md)
- [FR-021 Durable Managed Operations Audit](FR_021_DURABLE_MANAGED_OPERATIONS_AUDIT.md)
- [FR-021 Pre-Migration Control-Plane Repair Audit](FR_021_PRE_MIGRATION_CONTROL_PLANE_REPAIR_AUDIT.md)
