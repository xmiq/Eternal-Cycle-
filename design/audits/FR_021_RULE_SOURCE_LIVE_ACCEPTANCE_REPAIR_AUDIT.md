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

Subsequent lifecycle testing identified a separate architectural cause for repeated operation interruption: the tested AI host terminated its Python/MCP process tree shortly after response generation, while long-running operation execution still lived in that MCP process. Durable SQL recovery worked and reclaimed the same operation through execution attempt 4, but transport disappearance unnecessarily became executor disappearance. This evidence did not identify a Git defect and did not justify redesigning Rule Source acquisition.

A later run used the transport-independent worker's one-click fallback on the same user network and the same durable operation. Remote RC and base-release fetches completed in about 1.74 and 1.84 seconds, ref resolution and commit-distance inspection completed in about 81 and 88 milliseconds, and individual manifest-declared rule reads completed in about 50 to 114 milliseconds. The network environment had not materially changed. Unsloth-hosted execution is therefore the leading explanation for the historical abnormal Git behavior, but the exact mechanism remains unproven and the audit does not retroactively assign one cause to every older slow invocation. What is proven is that current acquisition completed quickly and that network or Git performance was not the blocker in this run.

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

### Transport-independent Managed Worker

Long-running Managed Operation execution now runs through a packaged `EternalCycle.ManagedWorker.exe` app host instead of the MCP hosted-service loop. MCP creates or reuses the durable operation, then launches a worker for that exact Operation ID. The worker exact-claims through migration 009 ownership, renews its lease, runs the existing publication executor, records progress and result in SQL, and exits at terminal state. Reconnecting MCP reads the same durable record. Transport termination does not cancel the worker or increment its execution attempt.

On Windows, the reference launcher creates a short-lived `cmd.exe` trampoline with `CREATE_BREAKAWAY_FROM_JOB`, `CREATE_NEW_PROCESS_GROUP`, and no inherited handles. The trampoline launches the worker and exits, breaking both restrictive host Job Object ownership and ordinary recursive parent-tree ancestry. There is no unsafe fallback that silently launches inside a Job Object after breakaway refusal. Non-Windows process hosting remains a reference-deployment responsibility rather than a new universal process API.

Execution leases and a per-operation local executor lock prevent duplicate local workers. A separate stop monitor observes a launch-specific filesystem signal while main work is blocked and propagates orderly cancellation through the existing operation owner. Active/progress files are best-effort operator views only; SQL remains authoritative. Stale launch-specific stop files cannot stop a later launch, and progress-file failure cannot change canonical operation outcome.

If automatic independent launch is refused, the existing operation stays queued/recoverable and the response uses `WORKER_INDEPENDENT_LAUNCH_BLOCKED` with `userInterventionRequired=true` and `administrativeInterventionRequired=false`. The reference prepares `Continue Eternal Cycle Setup.bat` plus a hidden temporary effective-configuration sidecar. The batch embeds all worker handoff parameters, so the user supplies no Operation ID, correlation ID, worker path, connection string, source, ref, Campaign ID, environment setting, terminal command, or administrator action. The worker must acquire its operation lock and reach the durable store before writing the handoff acknowledgement. The batch then deletes the sidecar and itself. A failed launch or unconfirmed handoff preserves retry material and never creates a replacement operation or campaign. Configuration values are absent from the batch, logs, diagnostics, and MCP message.

No schema migration was required. Operation identity, correlation, execution ownership, attempt evidence, state, stage, progress, errors, and results already have durable owners. Local launch/control artifacts do not become another persistence model.

### Live fallback acceptance and Rule Store Stage repair

The first live Unsloth test of the one-click fallback resumed durable operation `OP-1f57ee2497c8455eaa6e0c7e753fa9a2` with correlation `CORR-39ae3cbda162476796115ba62b6832a1` at execution attempt 9. It preserved the requested prerelease ref, resolved immutable SHA `520B5525DD10E3395C5BB35ECF43E95EE0FE0230`, compiled candidate `RULE-ff2a7b8cff364277a55703c590c7f588`, and advanced that same operation through acquisition, manifest reading, rule materialization, compilation, and into `Stage`. This is live evidence that the fallback can hand the existing durable operation to independent execution. It does not yet prove terminal publication and activation or every fallback-cleanup behavior.

The live failure at 55 percent was `RULE_STORE_STAGE_FAILED`, caused before SQL execution. Each of the chunk, selector, and dependency batch writers created a schema-bound command with `string.Empty` and only assigned the generated multi-row SQL afterward. `SqlServerSchemaIdentifier.Bind` correctly rejected the empty command immediately, so every non-empty first batch failed; the official candidate's 147 chunks merely exercised the unconditional defect first. The repair now constructs a complete non-empty batch statement before command creation and schema binding, while preserving the existing row parameters, batch sizes, transaction, and execution order. The same concrete defect was removed from all three writers rather than deferred to the next table.

`StageCandidateAsync` owns the release-row insert and all three batch families in one SQL transaction and commits only after every batch succeeds. Because the live exception occurred before the first chunk command executed and before commit, disposal rolled the transaction back; no special SQL cleanup or operation replacement is required. Retry continues through the existing durable operation contract. No migration was needed because neither schema nor durable state ownership changed.

The regression compiles the actual official manifest and asserts its measured 147 chunks, 990 selectors, and 768 dependencies. It builds all eight resulting multi-row batch statements across full and partial boundaries, proves each is non-empty before schema binding, and rejects zero-row generation at the producer boundary. An opt-in SQL Server integration test additionally stages, validates, publishes, and activates that candidate shape, verifies stored row counts, and verifies rollback after a deliberate duplicate-chunk failure. The installed LocalDB automatic instance could not start on this machine, so that real-SQL path remains available but was not proven here.

### Distribution archive

The Distribution Archive is deliberately separate from runtime acquisition. `tools/build_distribution.ps1` creates a deterministic full useful repository snapshot containing tracked and useful untracked source, documentation, reference implementations, tests, migrations, manifests, templates, license, notice, version, and distribution metadata. It excludes `.git`, `bin`, `obj`, caches, test output, logs, temporary files, credentials, and prior archives. `tools/test_distribution_archive.ps1` verifies required review material and rejects forbidden generated paths.

## Migration 008

Migration `008_diagnostic_operation_correlation` is retained. Migration 005 persisted `correlation_id` but could not preserve the distinct durable `operation_id` now carried by the diagnostic contract. Deriving every lookup through the operation store would lose the direct diagnostic identity when operation evidence is unavailable and would not persist the actual identifier emitted by the durable owner.

The migration adds one nullable `nvarchar(128)` column and one filtered index. Existing diagnostic rows remain valid with null Operation IDs. Both fixed-`ec_domain` and schema-template variants are packaged. No campaign table or campaign data changes. The migration is repeat-safe at the schema level; live LocalDB execution could not be proven in this run because the installed LocalDB instance failed to start.

## Architectural Classification

- **A — Reference implementation detail:** .NET process execution, Git commands, temporary acquisition repositories, filesystem payload layout, T-SQL migration syntax, PowerShell ZIP tooling, and SQL index implementation.
- **B — Managed Service / Rule Source Provider contract:** Durable Managed Operation execution is independent of ephemeral transport lifetime; bounded ownership, identity, recovery, idempotency, and safe fallback remain authoritative alongside bounded acquisition, immutable source provenance, propagated cancellation, sanitized command evidence, conservative cleanup, and manifest-defined payload materialization.
- **C — Eternal Cycle-wide invariant:** gameplay and rule availability do not depend on an AI client keeping a tool transport alive; when Eternal Cycle can prepare a required recovery action completely, it minimizes player intervention. Users still choose compatible rules, unknown causes remain unknown, and runtime compiler payload and external-review distribution remain distinct artifacts.

## Validation Evidence

- Release build: passed with zero warnings and zero errors.
- reference-service automated tests: 150 passed, 0 failed, 0 skipped. The focused Managed Rule Publication subset contributed 12 passing tests; the focused Managed Worker lifecycle subset contributed 16 passing tests and continues to cover transport-independent continuation, duplicate-executor exclusion, responsive stop handling, launch isolation, same-operation fallback handoff, successful self-deletion, failed-handoff retry preservation, configuration loading, app-host packaging, and real Windows parent-tree termination. The Stage regression plus opt-in SQL integration fixtures contributed 22 passing default-run tests when SQL integration was not enabled.
- opt-in LocalDB publication fixture: attempted explicitly and failed before product SQL ran because the LocalDB automatic instance could not start. This is recorded as unproven real-infrastructure validation, not a product pass. The deterministic official-candidate batching regression passed.
- FR-011 persistence-gate harness: 13 assertions passed.
- FR-017 portable-persistence harness: 29 assertions passed.
- FR-018 rule-compilation harness: 19 assertions passed.
- FR-019 Managed-data architecture harness: 48 assertions passed after replacing its stale cache-shape assertion.
- FR-020 first-run harness: 59 assertions passed after replacing its historical retained-`--no-checkout` assertion with the minimal-payload contract.
- FR-021 durable-operation/live-acceptance harness: 81 assertions passed, including migration 009 execution ownership, orphan recovery, transport-independent execution, fallback semantics, and unchanged `VERSION`.
- FR-021 control-plane repair harness: 35 assertions passed.
- release-neutral campaign-mode harness: 17 assertions passed.
- repository validation: passed across 286 Markdown files, 7,147 relative links, 163 anchors, 187 indexed canonical documents, 43 templates, 1,263 terminology checks, 193 roadmap tasks, and 21 Future Revision entries.
- `win-x64` release publish: passed and contained `EternalCycle.ManagedWorker.exe`, the shared MCP runtime DLL/dependency/runtime-configuration artifacts, distribution metadata, `LICENSE`, and `NOTICE`.
- distribution check: a fresh 362-entry archive passed required-content validation with zero forbidden generated paths.

## Remaining Live Acceptance

Repository validation does not prove:

- the exact Git command represented by the earlier approximately 326-second legacy `Inspect` diagnostic;
- remote Git host latency, authentication, proxy, or credential behavior;
- migration 008 execution on a working LocalDB or production SQL Server;
- LM Studio interoperability;
- automatic Job Object breakaway under the actual Unsloth host;
- terminal Unsloth publication after the now-proven one-click handoff reaches `Stage`;
- one-click fallback self-deletion under the actual Unsloth host;
- real SQL Server Stage, Publish, and Activate completion through the independently hosted worker;
- remote moving-RC discovery and publication until a validated candidate is deliberately accepted and pushed.

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
