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

A later live run narrowed the evidence without identifying a cause. Operation `OP-1f57ee2497c8455eaa6e0c7e753fa9a2` and correlation `CORR-39ae3cbda162476796115ba62b6832a1` recorded one successful Git `Inspect` command after about 27 seconds, then another `Inspect` command cancelled after about 326.5 seconds by its parent token. The durable operation subsequently classified its own interruption as `HostShutdown`. Process monitoring also proved that the current provider used a temporary `.acquire-*` workspace below the effective `%TEMP%\EternalCycle\rule-cache` root and removed that workspace after cancellation. At the time this repair was requested, both the exact 326-second command and the initiator of `HostShutdown` were **unknown**.

## Repair

### Durable cancellation and identity

Source and publication layers now propagate cancellation. The durable operation processor distinguishes host shutdown, its own Managed Operation timeout, overall acquisition timeout, individual Git-process timeout, and an otherwise unexplained parent cancellation. Service-owned work remains independent of the initiating MCP request.

Every publication stage receives the durable Operation ID and Correlation ID. Structured diagnostics, operation status, and Error Dumps can be looked up by Operation ID, Correlation ID, or both. Every external Git invocation now produces one terminal record with a per-invocation identity, exact sanitized logical command, working directory, resolved executable and PID when available, UTC start and end, elapsed milliseconds, exit evidence, outcome, timeout scope, cancellation source observed at that layer, and retry relevance. Deterministic sanitization redacts URL credentials, sensitive query parameters, token/password options, authorization headers, and the existing diagnostic secret classes. Standard output is not persisted, and failure text remains subject to the existing bounded sanitizer.

The process runner distinguishes its own timeout from parent cancellation. It reports `GitProcessTimeout` only for its configured subprocess bound and `ParentToken` when the already-linked caller token cancelled the command. The durable parent diagnostic remains authoritative for a more precise cause such as `HostShutdown` or a Managed Operation timeout. The runner does not infer that cause retroactively.

### Runtime Rule Source acquisition

The reference Git provider follows a bounded conventional flow: locate the selected source, resolve the requested ref to an immutable commit SHA, read the manifest and its declared files, materialize those files as ordinary files, validate, compile, and publish. Remote Git object storage is temporary. The persistent cache retains only the manifest-defined rule payload and compatibility/provenance metadata.

An existing local clone is inspected in place and passes through the same technical validation pipeline. The packaged source is a default, not a policy privilege. Compatible forks, mirrors, modified rules, house rules, and future providers remain legitimate sources; compatibility validation does not enforce semantic equality with official Eternal Cycle rules.

Cleanup now removes only provider-owned temporary artifacts under the currently configured source cache: `.acquire-*` workspaces, known `.partial-*` artifacts, interrupted snapshot staging directories, and the exact legacy per-source `.git` directory. It preserves valid snapshots and unknown directories. It does not inspect or delete the historical `%LOCALAPPDATA%\EternalCycle` cache merely because the current default resolves below `%TEMP%`. Configuration discovery exposes the effective current cache root so an operator can identify the active location without process monitoring.

The KISS acquisition architecture was deliberately not changed. Git remains an external reference-process detail; the provider still resolves an immutable source in a temporary workspace, materializes ordinary manifest-declared files and provenance, retains only the minimal snapshot, and removes the temporary Git workspace. No schema migration was introduced because the existing Operation ID, Correlation ID, stage, and structured diagnostic detail can represent the additional evidence safely.

### Distribution archive

The Distribution Archive is deliberately separate from runtime acquisition. `tools/build_distribution.ps1` creates a deterministic full useful repository snapshot containing tracked and useful untracked source, documentation, reference implementations, tests, migrations, manifests, templates, license, notice, version, and distribution metadata. It excludes `.git`, `bin`, `obj`, caches, test output, logs, temporary files, credentials, and prior archives. `tools/test_distribution_archive.ps1` verifies required review material and rejects forbidden generated paths.

## Migration 008

Migration `008_diagnostic_operation_correlation` is retained. Migration 005 persisted `correlation_id` but could not preserve the distinct durable `operation_id` now carried by the diagnostic contract. Deriving every lookup through the operation store would lose the direct diagnostic identity when operation evidence is unavailable and would not persist the actual identifier emitted by the durable owner.

The migration adds one nullable `nvarchar(128)` column and one filtered index. Existing diagnostic rows remain valid with null Operation IDs. Both fixed-`ec_domain` and schema-template variants are packaged. No campaign table or campaign data changes. The migration is repeat-safe at the schema level; live LocalDB execution could not be proven in this run because the installed LocalDB instance failed to start.

## Architectural Classification

- **A — Reference implementation detail:** .NET process execution, Git commands, temporary acquisition repositories, filesystem payload layout, T-SQL migration syntax, PowerShell ZIP tooling, and SQL index implementation.
- **B — Managed Service / Rule Source Provider contract:** bounded acquisition and subprocess execution, technical source validation, immutable source provenance, propagated cancellation, durable Operation/Correlation identity, one sanitized and non-speculative terminal diagnostic per external source command, conservative ownership-aware temporary cleanup, and manifest-defined payload materialization.
- **C — Eternal Cycle-wide invariant:** users choose compatible rules; official rules are a default rather than a semantic gate; unknown causes remain unknown; runtime compiler payload and external-review distribution are different artifacts.

## Validation Evidence

- Release build: passed with zero warnings and zero errors.
- reference-service automated tests: 132 passed, 0 failed, 0 skipped. The focused Git diagnostics/cache subset contributed 36 passing tests and covers successful execution, nonzero exit, process timeout, parent cancellation, exact-command differentiation, sanitization, process identity, Operation/Correlation propagation, and conservative cleanup.
- opt-in LocalDB fixture: attempted in restricted and unrestricted execution; both failed before product SQL ran because the LocalDB automatic instance could not start. This is recorded as unproven real-infrastructure validation, not a product pass.
- FR-011 persistence-gate harness: 13 assertions passed.
- FR-017 portable-persistence harness: 29 assertions passed.
- FR-018 rule-compilation harness: 19 assertions passed.
- FR-019 Managed-data architecture harness: 48 assertions passed after replacing its stale cache-shape assertion.
- FR-020 first-run harness: 59 assertions passed after replacing its historical retained-`--no-checkout` assertion with the minimal-payload contract.
- FR-021 durable-operation/live-acceptance harness: 73 assertions passed, including migration 009 execution ownership and orphan recovery.
- FR-021 control-plane repair harness: 35 assertions passed.
- release-neutral campaign-mode harness: 17 assertions passed.
- repository validation: passed across 286 Markdown files, 7,146 relative links, 163 anchors, 187 indexed canonical documents, 43 templates, 1,261 terminology checks, 193 roadmap tasks, and 21 Future Revision entries.
- distribution check: a fresh 361-entry archive passed required-content validation with zero forbidden generated paths.

## Remaining Live Acceptance

Repository validation does not prove:

- the original external cancellation source;
- remote Git host latency, authentication, proxy, or credential behavior;
- migration 008 execution on a working LocalDB or production SQL Server;
- LM Studio or Unsloth Studio MCP interoperability;
- live service restart during acquisition or publication;
- remote moving-RC discovery and publication until the candidate is pushed and verified.
- whether the next live SQL/Error Dump trace identifies the exact long-running command and supplies enough correlated parent evidence to identify the cancellation initiator.

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
