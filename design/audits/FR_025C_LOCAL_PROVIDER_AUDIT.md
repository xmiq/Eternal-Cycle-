# FR-025C Local Compiled-Rules Provider Audit

## Authority and Checkpoint

The owner authorized C only beneath the selected [FR-025 objective](../V1_1_FUTURE_REVISION_PLAN.md#fr-025---provider-neutral-artifact-acquisition-and-import). Work started from clean synchronized `main` at `94ed474e6eb6ec184cb1bf698aee5f3736293a7a`, after [A](FR_025A_ACQUISITION_ARCHITECTURE_AUDIT.md) and [B](FR_025B_VALIDATION_TRUST_AUDIT.md). FR-025 remains selected/incomplete: **A-C complete; D-G pending; FR-026 pending/unselected**. `VERSION` stays `1.0.0`; tag references remain `v1.0.0` at `6938bd2f9e965faaeb1c612b1dd765237cadc8a8` and `v1.1.0-rc` at `4eb2583027d316a962fd49fe318781a0e060183e`.

No Git/HTTP acquisition, custom-provider acceptance, import/storage, publication/activation, runtime retrieval, migration, campaign state, release artifact or tag action is included. Decisions D-1363 through D-1365 already govern acquisition, trust and immutable approval; C adds reference behavior, not a new format or terminology. No unresolved design question blocks this package.

## Provider and Dependency Boundary

[LocalCompiledRulesArtifactProvider](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/LocalCompiledRulesArtifactProvider.cs) is a sealed implementation of A's `ICompiledRulesArtifactProvider` in the existing portable `EternalCycle.Rules` library. One explicit absolute file path is configuration; trusted limits and cancellation are per-call inputs. There is no global setting, implicit base, repository discovery, installation search, manifest, Campaign ID or new provider-specific acquired model. No project/package dependency was added.

```text
explicit absolute ordinary file
    -> component checks
    -> one read-only FileStream / handle checks
    -> A bounded reader + independent exact-byte SHA-256/evidence
    -> caller's B validation + explicit trust policy
```

Production C never invokes FR-022 parsing/validation or B policy, and never decodes or rewrites content. Provider success is acquired bytes, not valid/trusted/imported/active rules. Existing compiler/CLI/Managed call paths are unchanged; no acquisition command or setup endpoint is wired prematurely.

An internal `Func<string, FileStream>` open seam and a test-assembly friend declaration exercise real-handle cancellation, I/O failure and deterministic path replacement without sleeps or a filesystem abstraction framework. Public consumers can only configure the path; A/B public contracts remain unchanged.

## Filesystem Policy and Reuse Decision

FR-023's `MaterializedRuleSourceLoader` was inspected. Its private root/manifest-relative traversal, containment rules, synchronous unbounded source read and source-specific errors are not a reusable ordinary-artifact acquisition API. C follows its `ReparsePoint` rejection rule rather than weakening it. Extracting a framework for a two-line attribute test would add indirection without meaningful reuse; FR-023 code is left untouched and its regressions pass.

The configured path must be fully qualified on the current platform, is normalized once by `Path.GetFullPath`, and is split into reusable root/parent/final components with native separators. Empty/null/blank configuration, relative/drive-relative/control-containing paths and malformed platform paths fail safely. Windows device namespaces and alternate streams are rejected; ordinary filesystem share paths retain OS access semantics. No drive-letter or repository-root assumption defines the common contract.

Each checked component rejects observable reparse/link/device attributes; parents must be directories and the final target must be a nondirectory. Checks run before open, after open and after read. After opening, `File.GetAttributes(stream.SafeFileHandle)` inspects the actual object, and readable/seekable capability is required. The same handle supplies every acquired byte. Production access is read-only with `FileShare.Read`, asynchronous sequential reads and guaranteed disposal. No pre-read length or modification time establishes identity or bypasses A's bound.

### Guarantees and Residual Limits

- Detected final/parent symlinks and Windows junctions are rejected, including persistent swaps between initial check/open and during reading. Returned success has passed all component checks; failed/cancelled work returns no partial result.
- A's limit-plus-one streaming bound is reused unchanged, default 16 MiB. No unbounded `ReadAllBytes`, BOM removal, newline normalization, JSON rewriting or semantic interpretation occurs in production C.
- The acquired hash identifies the actual bytes observed on the opened stream. File sharing restricts normal concurrent writes/deletion where enforced, but is not a universal snapshot guarantee. Controlled in-place mutation demonstrates a mixed read gets the hash of those exact bytes, not old metadata; B must still validate it.
- Component observations and handle-type checks are not atomic ancestor locking or proof of path/handle inode equality. A hostile replace-and-restore indirection race can evade observations. Hard links/mounts are not universally exposed as reparse points; ordinary OS permissions and an explicitly authorized file in controlled storage remain necessary.
- Synchronous OS metadata/open calls cannot be forcibly cancelled while blocked; cancellation is observed around them and during asynchronous reads. Portable attributes cannot preclassify every special object on every platform; observable device/nonseekable objects are rejected, but native special-file/race safety is not inferred from these tests.
- Current execution evidence is Windows. Unix/macOS special-file behavior, network-share semantics and hostile filesystem races have not been exercised. No race-free filesystem sandbox or coherent snapshot is claimed.

## Evidence, Privacy, and Identity

C returns A's exact `AcquiredCompiledRulesArtifact` with provider kind `local-file`, absent resolved immutable identity, and no locator metadata. The path stays private configuration, not a public property, default provider serialization/display, exception message or evidence field. There is no unnecessary duplicated byte hash/size field: those remain in the standard A result. Safe diagnostics use fixed `Code`/`Message`; raw exception/stack/configuration disclosure requires separate authorized handling.

No path, timestamp, filename or file length is immutable artifact identity. Identical bytes from different files have identical acquired SHA, FR-022 validity, semantic digest and artifact/source provenance. Replacing bytes at one configured path changes acquired identity and B's result. Acquisition evidence never overwrites artifact provenance or approves local content.

## Failure Mapping

| Condition | Existing category / code |
| --- | --- |
| Missing/null/blank provider configuration | `ProviderConfigurationInvalid` / `ARTIFACT_PROVIDER_CONFIGURATION_INVALID` |
| Invalid/relative locator, directory/non-directory parent, prohibited link/reparse/device/alternate stream or detected nonordinary handle | `LocatorInvalid` / `ARTIFACT_LOCATOR_INVALID` |
| Missing file/component, access/security denial | `Unavailable` / `ARTIFACT_UNAVAILABLE` |
| OS open/read I/O failure | `TransportFailed` / `ARTIFACT_TRANSPORT_FAILED` |
| Byte bound exceeded | `PayloadTooLarge` / `ARTIFACT_PAYLOAD_TOO_LARGE` |
| Pre/during/post-read cancellation | `OperationCanceledException` with the token, no acquisition success |

All mapped errors use A's fixed bounded messages and omit raw OS text/inner exceptions. No category/code was added or renumbered. The initial test compilation exposed one conditional throw-expression syntax error, corrected before execution; the final code/tests build cleanly. This was test syntax, not an artifact/provider contract defect.

## Executed Acceptance

The [39 focused cases](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/LocalCompiledRulesArtifactProviderTests.cs) pass with zero skips. Real final-file and parent-directory symlinks, persistent pre-open/post-read swaps and a Windows junction were executed successfully, not inferred from capability alone. Capability-based attributes explicitly skip symlink cases when creation is unavailable and Windows cases on other platforms; none skipped here.

Isolated tests contain only a manually supplied artifact, no checkout/manifest/install layout. Valid input proceeds directly through A and B approval. Malformed, invalid-UTF-8 and empty bytes acquire successfully, then B rejects before policy; a valid local artifact can be rejected by explicit policy. Different-path and same-path replacement tests prove independent identity. Exact LF/CRLF/BOM/arbitrary bytes are retained. Real sparse overbound files and observed streams prove limit-plus-one rejection without pre-read length use or truncated success. Cancellation while a read is deterministically blocked and cancellation after bytes arrive both prevent success and release the handle.

Permission/open/read errors and detected nonseekable objects use controlled substitutes; these are mapping/cleanup evidence, not a claim that a live OS permission-denial or POSIX device was reproduced. Mutation substitutes use a real unbuffered handle with deliberately permissive sharing to expose inter-read writes that production Windows sharing normally prohibits. Failure evidence contains no configured private locator or secret-bearing injected OS message.

## Compatibility and Validation

Canonical real-size acquisition/B approval remains **10 sources / 154 snippets / 275,622 bytes**:

- semantic artifact digest: `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`;
- exact acquired/serialized byte SHA-256: `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`.

| Executed validation | Result |
| --- | --- |
| Portable and CLI Release builds | Pass, zero warnings/errors. |
| Focused A/B/C | 131/131: A 34, B 58, C 39. |
| Complete portable suite | 436/436. |
| Complete CLI/process suite | 62/62. |
| Complete Managed MCP suite | 181/181. |
| FR-023 loader/snippet/writer regressions | 70/70; original filesystem implementation unchanged. |
| FR-024 input/enrichment/pipeline/audit/curation/acceptance regressions | 235/235. |
| FR-022 conformance plus portable assembly boundary | 17/17; CLI boundary also passes in its full suite. |
| Nine structural harnesses | 333/333: FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 operations 81, FR-021 repair 35, FR-022 32, campaign-mode 17. |
| Full repository / deterministic distribution checks | Pass: 302 Markdown files, 7,389 links, 207 anchors, 191 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions; zero blocking questions, orphaned documents or forbidden campaign-data directories. |

Complete suites total **679 unique tests**, zero failures/skips; focused reruns overlap and are not extra unique tests. Existing Release `dotnet build`/`dotnet test` commands use `--no-restore` with available dependencies. The existing full command is `pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\validate_repository.ps1`. Final documentation is followed by full diff review, explicit staging, staged whitespace check, focused C rerun (39/39) and repository validator (counts above); the exact staged content must pass before commit/push. No generated `bin`/`obj`, temporary fixture/output, private path/credential or campaign data enters the commit/distribution.

## Classification and Stop Boundary

- **A - Reference implementation:** .NET provider, fully qualified path choice, component/handle checks, FileShare/stream options, fixed local evidence, internal test seam and Windows test setup.
- **B - Managed Service contract:** configured providers return bounded exact bytes/evidence to the same mandatory validation/trust path; acquisition cannot grant import/activation authority or fabricate immutable provenance.
- **C - Eternal Cycle-wide:** local/manual distribution does not require a repository/runtime host or change Repository Canon; location is not authority, integrity is not trust, and readiness still requires later gates.

FR-022/023/024/A/B semantics, vocabulary, Managed publication/storage and campaigns remain unchanged. F's source-based uniqueness and incomplete retrieval-metadata retention remain pending; C neither solves them nor drops B's data. GitHub/provider acceptance and real SQL import remain unproven/deferred to D-G. Stop after the focused C commit and owner-requested normal `main` push; do not begin D-G or FR-026.
