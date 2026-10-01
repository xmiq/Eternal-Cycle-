# FR-025A Acquisition Architecture and Foundation Audit

## Scope and Conclusion

The owner selected FR-025 after FR-022/023/024 closure and authorized only A's investigation, contract definition and smallest stable foundation. Work started from clean synchronized `main` at `a34c268d239bc9b195200e2198bb899a60ad380b`. **A is complete; FR-025 is selected and incomplete.** [B-G](../FR_025_EXECUTION_PLAN.md) remain pending, and no next objective is selected.

A adds no artifact parser, trust-policy execution, production local/GitHub provider, import/storage transaction, runtime retrieval, schema migration, release or campaign change. `VERSION` remains `1.0.0`; `v1.0.0` remains `6938bd2f9e965faaeb1c612b1dd765237cadc8a8` and `v1.1.0-rc` remains `4eb2583027d316a962fd49fe318781a0e060183e`.

## Investigation Evidence

The [execution plan](../FR_025_EXECUTION_PLAN.md#existing-architecture-map) maps acquisition, local/Git/GitHub/Managed input, parsing, validation/trust, import, publication, storage and retrieval separately. Investigation read the governed objective, artifact/compiler/vocabulary contracts and closure audits, FR-019 service architecture, FR-020 startup/setup, FR-021 operations/runtime procedure, release provenance and relevant implementation/configuration/tests.

| Inspected owner | Established behavior / limitation |
| --- | --- |
| [Managed publication coordinator/provider contract](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/ManagedRulePublication.cs) | `IRuleSourceProvider` returns source documents. The coordinator compiles a legacy index, reuses source identity and candidate states, validates/publishes, and separately activates/prepares. It does not parse/acquire format-1 artifact bytes. |
| [Git provider](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/GitRuleSourceProvider.cs) | Configured Git sources/local repositories, immutable revision resolution, bounded subprocess/acquisition lifecycle and minimal manifest payload materialization. This is not GitHub Releases asset download; the implemented provider selection is Git, not every future source family listed by the contract. |
| [Managed configuration/administration](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/ManagedAdministration.cs) | Persists source location/ref/manifest/channel under approval and separate service/domain setup. `ec_configure_rule_source` and `ec_publish_initial_rules` select and compile sources, not artifact import. |
| [Portable materialized loader](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/MaterializedRuleSource.cs) | Validates ordinary source-root/manifest files with exact-byte provenance and filesystem containment. It is not a compiled-artifact file reader. |
| [FR-022 parser/validator](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRulesArtifact.cs) | Closed JSON, duplicate/unknown properties, depth 64, format/compiler compatibility, canonical identity/dependencies, content hashes and semantic digest. `Read(string)` assumes the caller already has text; it supplies no bounded external-byte acquisition or installation approval. |
| [Portable pipeline](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/RuleCompilationPipeline.cs) | Shared source split/enrichment/validated artifact/writer; exact provenance and compiler/audit behavior already accepted. No provider or importer. |
| [SQL published store](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/SqlServerPublishedRuleStore.cs) | Atomic candidate transaction includes release/chunk/selector/dependency batches; activation transaction switches states/pointer. Staging stores legacy index JSON, not the complete format-1 artifact. |
| [Rule Domain schema](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/Schema/002_rule_domain.sql) | Unique `(ruleset_id, source_identity)`, release-scoped chunks/selectors/dependencies and active pointer. An artifact-digest key or lossless format-1 retrieval carrier is not already implemented by this schema. |
| [Rule context tool](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/RuleContextTools.cs) | Readiness-gated published-context retrieval; no byte import, provider selection or vocabulary ranking. |

Publication/compatibility tests establish candidate retry, active-release preservation, source failure categories, source-independent runtime reads, dependency closure and local Git provenance. SQL write-plan/batch tests establish parameterized nonempty batches. Those tests are not evidence that a nonexistent artifact importer is atomic/idempotent. Transaction code was inspected; no real SQL deployment was exercised in A.

## Reconciliation and Import Gaps

No conflict with FR-022 artifact semantics, FR-023 compilation or FR-024 enrichment/audit was found. A reuses their authority without duplicating a parser or changing format 1. Existing Managed source compilation remains supported and unchanged.

Two concrete future storage gaps were found: legacy publication deduplicates by source identity rather than the artifact semantic digest, and its chunk/index model does not retain all format-1 retrieval metadata. F must provide one lossless shared import path, preserve legacy history and active rules, and explicitly reconcile or reject conflicting source-unique keys. A does not silently relabel the legacy index as an artifact or redesign its schema. No current architectural question blocks the bounded A package; storage implementation/physical design remains deliberately assigned to F.

Successful import will mean an authorized, atomic, lossless stored artifact identity/receipt in rule storage, not campaign creation or automatically ready gameplay. Publication/activation, campaign compatibility/adoption and progressive closure readiness remain existing separate gates. FR-026 owns searchable projection/ranking; it cannot replace FR-025 provider-independent validation.

## Implemented Foundation

The [shared contract](../../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md) and [portable implementation](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRulesAcquisition.cs) add one configured-provider interface, trusted byte-limit policy, owned exact bytes and SHA-256, separate bounded provider evidence/resolved identity, and fixed safe failures.

The reference default is **16 MiB**, not a normative format limit. Stream buffering stops at that policy plus a one-byte overflow probe; declared length, seeking and Content-Length are not trusted. Construction rejects oversized existing buffers before copying. Provider streams remain provider-owned. Pooled bytes are cleared on return. Cancellation before/during a blocked read propagates unchanged; raw `IOException` text/inner exception is not echoed in the safe failure. No disk or datastore output occurs.

Evidence permits custom kind/identity schemes without GitHub or Managed inheritance. Metadata is copied/read-only, ordinally ordered, bounded to 16 entries, 128-character keys and 1,024-character values. Provider kind/scheme/value have explicit bounds; blank/control/duplicate data is rejected. Evidence remains an untrusted provider claim. Safe object display and fixed errors do not turn arbitrary public fields into authorized diagnostics; providers must remove secrets and consumers must explicitly sanitize/select permitted fields before disclosure.

Raw locator/configuration stays provider-owned; an optional sanitized label may be metadata. A path/tag alone is never immutable identity. Byte hash independently identifies acquired bytes; provider resolution is separate evidence; the artifact's own source/compiler/manifest identity remains unchanged. Hash consistency is integrity, not publisher authentication or installation trust. B will independently verify size/hash/UTF-8/FR-022 semantics and require explicit trusted acceptance policy before an import-eligible result exists.

The taxonomy has twelve safe categories: configuration, locator, unavailable, transport, too large, integrity, malformed, unsupported format/compiler, semantic validation, trust rejection, import conflict and storage failure. Later-stage codes are contract vocabulary only; A does not implement those deferred stages or fabricate retry/approval claims.

## Contract Acceptance

The [34 focused tests](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/CompiledRulesAcquisitionTests.cs) pass:

| Requirement | Executed evidence | Result |
| --- | --- | --- |
| Provider-neutral bytes without semantic authority | Fake local/custom configured providers acquire identical arbitrary non-artifact bytes through the same contract; no format parser/provider approval embedded in acquisition. | Pass |
| Exact owned bytes and independent deterministic hash | Input-buffer mutation cannot alter stored bytes; repeats agree; LF/CRLF changes byte identity; empty bytes remain acquired, not approved. | Pass |
| Separate immutable-resolution evidence | Custom opaque identity distinct from provider, byte hash and artifact source; absent resolution supported, metadata copied/readonly/ordinal. | Pass |
| Bounded metadata/configuration | Count/key/value/control/duplicate rejection, fixed messages without attacker values, positive limit validation and provider-kind rejection. | Pass |
| Bounded stream and buffered payload | Non-seekable fragmented reads at limit; oversize stops at limit plus one; constructor rejects overbound; stream left open. | Pass |
| Cancellation / failures | Precancel reads nothing; blocked read observes cancellation; unreadable stream fails structurally; transport secret text absent from safe exception; all 12 categories/code/message bounds verified. | Pass |
| Existing canonical compatibility | Accepted 275,622-byte artifact fits default policy, bytes equal input, existing FR-022 parser/digest unchanged. | Pass |

A adds no trusted artifact result/import interface prematurely. B owns that actual gate; F owns importer/storage behavior. A's fake provider is a contract fixture, not completion of the real C/D providers or E's full replaceability/import conformance.

## Security and Classification

The [plan's security table](../FR_025_EXECUTION_PLAN.md#security-review) records traversal/indirection/TOCTOU, size, decompression, redirects/content/filenames, mutable/ambiguous assets, diagnostics and atomicity. A has no filesystem path traversal, HTTP, archive extraction, provider output filename, or storage command to mitigate. C/D/F must test their actual risks before completion. No archive handling or signing infrastructure was invented.

- **A - Reference:** C# models/interface, standard-library/pool mechanics, 16 MiB default and tests.
- **B - Managed contract:** one pre-import validation/trust path, scoped lossless atomic/idempotent import, preserved operation/authorization/diagnostic boundaries.
- **C - Eternal Cycle-wide:** provider freedom, unchanged Repository Canon and semantic identity, separate acquisition/artifact provenance, integrity is not trust, no gameplay authority inferred from import.

## Compatibility and Validation

Curated canonical regression remains **10 sources / 154 snippets / 275,622 bytes**:

- semantic artifact digest: `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`;
- exact serialized byte SHA-256: `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`.

These are fixture evidence, not production constants. Authoritative manifest/source bytes, snippet identity, normalized content, source/content hashes and vocabulary remain unchanged. The historical no-vocabulary baseline is still validated by the existing portable/CLI regressions; A introduces no acquisition side effect into compilation.

| Validation executed | Result |
| --- | --- |
| Portable library / CLI Release builds | Pass, zero warnings/errors. |
| New A contract tests | 34/34. |
| Complete portable suite | 339/339. |
| Complete CLI/process suite | 62/62. |
| Complete Managed MCP suite | 181/181; no Managed source changed. |
| Focused A plus FR-023 materialized/snippet/writer regressions | 104/104 (34 new plus 70 established). |
| Focused FR-024 A-F portable regressions | 235/235. |
| FR-022 contract plus portable dependency boundary | 17/17 (16 plus one). |
| Structural harnesses | 333/333 across nine harnesses: FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 operations 81, FR-021 repair 35, FR-022 32, campaign-mode 17. |
| Full repository validator after final governance/navigation | Pass: 300 Markdown files, 7,364 links, 204 anchors, 191 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. Zero blocking questions, orphaned documents or forbidden campaign-data directories; deterministic distribution/content checks pass. |
| Exact-state focused tests / whitespace | Release focused acquisition rerun 34/34; `git diff --check` passes. Git reports only the existing LF-to-CRLF worktree conversion notices, not whitespace errors. |

Complete suites total **582** tests, with no failures/skips; focused reruns are subsets, not additional unique tests. Final focused, repository and staged whitespace checks must pass again on the exact state containing this evidence before commit. No new project/package dependency was introduced; portable code still has no Managed/SQL/hosting/MCP/network/Git dependency.

Executed commands use Release configuration: portable and CLI `dotnet build`, portable/CLI/Managed `dotnet test`, focused portable test filters, and `pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\validate_repository.ps1`. `--no-restore` reuses already available dependencies, not substitute validation; focused test reruns rebuild the portable assembly rather than claiming unexecuted tests. No real external provider/storage deployment is implied by these results.

## Governance and Stop Boundary

Roadmap, register, plan and validator agree: FR-025 is the sole selected in-progress objective, A complete and B-G pending; FR-022/023/024 remain Closed. Phase 13 stays Active, Future Revisions `[∞]` last. D-1363/1364 record approved acquisition/import boundaries; three terms clarify provider/evidence/byte hash without changing artifact terminology.

The owner authorized one focused A commit and normal `main` push. No tag, VERSION, archive, release, migration or campaign changes are included. No live GitHub artifact download, private authentication/redirect policy, local-file race/reparse acquisition or real SQL import transaction was exercised; those are pending provider/storage acceptance, not evidence A may claim. Stop after push; do not implement B or FR-026.
