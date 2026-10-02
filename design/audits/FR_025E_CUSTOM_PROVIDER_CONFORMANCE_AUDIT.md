# FR-025E Custom Provider Conformance Audit

## Authority and Checkpoint

The owner authorized E only beneath selected [FR-025](../V1_1_FUTURE_REVISION_PLAN.md#fr-025---provider-neutral-artifact-acquisition-and-import). Starting `main`, HEAD and upstream were clean/synchronized at `79871c2af05bcf9d5f61e42bde239ed5c8a1c125`. **A-E complete; FR-025 selected/incomplete; F-G pending; FR-026 pending/unselected.** E is conformance evidence, not production transport, import or final closure. `VERSION` stays `1.0.0`; tag references stay `v1.0.0` at `6938bd2f9e965faaeb1c612b1dd765237cadc8a8` and `v1.1.0-rc` at `4eb2583027d316a962fd49fe318781a0e060183e`.

Authority review covered A-D contracts, code/tests/audits, artifact/compiler/vocabulary boundaries, governance, terminology and validation conventions. Existing D-1363 through D-1365 suffice; no new design decision, terminology, unresolved blocking question or Future Revision was created.

## External Implementer and Dependency Direction

```text
Local C  --+
GitHub D --+-> AcquiredCompiledRulesArtifact -> unchanged B validation/trust
Custom E --+                                  -> same import-eligible type
```

The [test-only MemoryStoreCompiledRulesArtifactProvider](../../examples/tooling/rules-compiler-dotnet/tests/Shared/MemoryStoreCompiledRulesArtifactProvider.cs) is authored against public A types only. Explicit logical object key/version and a configured stream opener simulate external storage; no file path, HTTP endpoint, provider discovery, compiler, artifact parser or trust logic is required. Optional opaque storage-generation evidence needs no core registration. It directly returns A's result, not a custom model needing adaptation. This does not define a production URI scheme or new transport implementation.

The fixture is linked into two existing test assemblies. Portable tests already have friend access for C's internal file seam; the [external conformance tests](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Compiler.Tests/ExternalProviderConformanceTests.cs) deliberately compile the identical fixture in the CLI test assembly without friend access. Successful build plus an executed friend-absence assertion prove public accessibility, not merely a source inspection claim. Only test-project compile includes changed; no production assembly/project/package, interface, provider switch, plugin registry or DI scanning was added.

**Production changes: zero.** A/B are unchanged; their signatures are sufficient. C/D, FR-022/023/024, CLI behavior and Managed publication/persistence/retrieval remain untouched. The core assembly has no reference to this fixture/test assemblies, Managed, SQL, hosting or MCP; existing dependency regressions also pass. C/D comparison infrastructure is isolated in the matrix test, not a dependency of the custom provider. Preparing canonical/minimal fixture bytes may use existing compiler/test data; acquiring them through E requires none of that infrastructure.

## Public Contract and Hidden-Assumption Review

| Surface reviewed | Classification and finding |
| --- | --- |
| Interface / request | Provider-neutral: already-configured `ICompiledRulesArtifactProvider.AcquireAsync` takes trusted limits and cancellation only. No provider selector, Campaign ID or host requirement. |
| Configuration / locator | Intentionally provider-specific outside A/B: C absolute path and D repository/tag/asset; E logical key/version. Common code interprets none as a path, URL or Git revision. |
| Provider identity | Provider-neutral: open bounded string, not a local/GitHub enum/switch. Unregistered `conformance-memory-store` works with B unchanged. |
| Resolved identity / evidence shape | Provider-neutral: open bounded scheme/value and optional copied ordinal metadata. `fixture-object-version` / `storageGeneration` remain opaque claims; B policy can inspect them, parser never interprets them. |
| Evidence validity / privacy | Provider-neutral: existing count/key/value/control/duplicate checks remain enforced. Bounded evidence is not universal sanitization or approved diagnostic disclosure. Providers must remove secrets. |
| Byte ownership / identity | Provider-neutral: A copies exact source bytes and computes SHA. B privately snapshots/rehashes and freezes approved model/evidence; deliberate A backing-array tampering fails before policy. No stronger A readonly-memory guarantee is claimed. |
| Failure taxonomy | Provider-neutral: existing fixed bounded categories/codes/messages; no custom errors, raw transport message or inner exception. Import/storage categories do not imply those stages are implemented. |
| Cancellation | Provider-neutral: supplied token before/during/after streaming; cancellation is never partial success or generic failure. Arbitrary noncooperative external code is not forcibly stopped by the interface. |
| Bounded streaming | Provider-neutral: public `AcquiredCompiledRulesArtifact.ReadAsync`, limit-plus-one probe, no length/seek requirement; provider owns stream disposal. Constructor available for bounded buffers. |
| Filesystem / HTTP | Intentionally provider-specific outside A/B: C/D policies remain in adapters, not common validation. E has no path/HTTP dependency and needs no builtin internals. |
| Artifact validity / trust | Provider-neutral: existing FR-022 validity and required explicit B policy. No provider-kind branches, evidence-to-provenance replacement, official-content equality or default approval. |

**Defects requiring correction: none.** No surface was weakened or broadened to accommodate the fixture. This is evidence of replaceability, not a guarantee that arbitrary provider/policy code obeys limits, removes secrets or has no side effects; the in-process contract is not a sandbox.

External implementers configure their transport outside A, return bounded exact bytes/evidence, dispose owned streams, map known transport/configuration failures to A and propagate cancellation. The caller separately supplies B's trusted limits and `ICompiledRulesArtifactTrustPolicy`. The [implementer guide](../../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md#custom-provider-implementer-boundary) links the public APIs and obligations without introducing another contract.

## Executable Cross-Provider Matrix

The [portable conformance tests](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/CompiledRulesProviderConformanceTests.cs) exercise actual C and D through their public configuration, using a temporary ordinary file and deterministic GitHub HTTP substitute, alongside the unrelated E stream fixture. All use identical canonical bytes; each calls unchanged B with a supplied policy and compares the complete serialized semantic model, not only a digest.

| Property | Local C | GitHub D | Custom E |
| --- | --- | --- | --- |
| Implements common provider contract | Pass | Pass | Pass |
| Returns standard A result | Pass | Pass | Pass |
| Exact bytes preserved | Pass | Pass | Pass |
| Independent byte SHA | Pass | Pass | Pass |
| B validator unchanged | Pass | Pass | Pass |
| Explicit trust evaluation required | Pass | Pass | Pass |
| Same validity / semantic identity / complete provenance | Pass | Pass | Pass |
| Equivalent approved bytes / common import-eligible type | Pass | Pass | Pass |
| Distinct acquisition evidence permitted | Pass | Pass | Pass |

Canonical result remains **10 sources / 154 snippets / 275,622 bytes**:

- semantic artifact digest: `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`;
- acquired/serialized byte SHA-256: `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`.

Provider metadata cannot alter Ruleset/source/compiler/manifest provenance, snippet IDs, source/content hashes, selectors, dependencies or retrieval terms. Generic approval gives identical import-eligible artifacts. A separately configured policy distinguishes custom version/generation without modifying B; both observations remain valid with equal semantic models even when one is rejected. Same-word metadata such as `rulesetId` or `source` has no artifact authority.

Malformed JSON, invalid UTF-8 and empty bytes acquire successfully but fail B before policy. Valid bytes may still be rejected; absent policy never becomes approval. No actual storage/imported identity is claimed: the matrix stops at B's existing import-eligible representation.

## Bounds, Failures, Cancellation and Mutation

The fixture uses A's reader without parallel buffering/copying/hash logic. Nonseekable fragmented streams expose no usable Length. Exact-limit success, limit-plus-one rejection, absent size and misleading opaque length claims demonstrate that actual streamed bytes control the bound. Oversize stops at one extra byte with no truncated result; B also enforces its own stricter limit before policy.

Empty/control-invalid configuration and overbound/duplicate metadata use `ProviderConfigurationInvalid`; missing logical objects `Unavailable`; open/partial-read IO exceptions `TransportFailed`; oversize `PayloadTooLarge`. Fixed code/message/no-inner diagnostics retain no injected raw secret text. Unreadable streams fail safely. Precancel opens nothing; cancellation while waiting after a partial read and cancellation at EOF cannot emit success. Owned streams dispose on success, failure and cancellation. No timed waits simulate progress; a bounded test watchdog detects failure to observe cancellation.

Provider source-buffer mutation after acquisition cannot alter A or B bytes. Metadata source mutations before/during/after policy cannot change copied evidence; exposed dictionary mutation throws. Approved copies are independent, and deliberately unwrapped A bytes cannot alter the already-approved snapshot. Pre-validation tampering rejects through `IntegrityMismatch` before trust. Reflection/unsafe memory manipulation beyond the documented API is not covered.

## Executed Validation

| Validation | Result |
| --- | --- |
| Portable and CLI Release builds | Pass, zero warnings/errors. |
| E portable conformance / non-friend external cases | 34/34 and 2/2 (36 new unique). |
| A-E portable focused suite | 246/246: A 34, B 58, C 39, D 81, E 34; external two executed separately. |
| Complete portable suite | 551/551. |
| Complete CLI/process suite | 64/64. |
| Complete Managed suite | 181/181. |
| FR-023 loader/snippet/writer regressions | 70/70. |
| FR-024 A-F regressions | 235/235. |
| FR-022 conformance / portable boundary | 17/17; CLI boundary included in full CLI suite. |
| Nine structural harnesses | 333/333: FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 operations 81, FR-021 repair 35, FR-022 32, campaign-mode 17. |
| Full repository / deterministic distribution validation | Pass: 304 Markdown files, 7,418 relative links, 211 anchors; 191 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. Zero blocking questions, orphaned documents and forbidden campaign-data directories. |

Complete suites total **796 unique tests**, zero failures/skips; focused reruns overlap. Existing commands use Release `dotnet build`/`dotnet test` with `--no-restore` and already available dependencies; no unknown previous result substitutes for this checkpoint's executed tests. Full validation uses `pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\validate_repository.ps1`. Final documentation is followed by complete diff review, explicit staging, staged whitespace, focused E reruns (34 plus two) and repository rerun (counts above); exact content must pass before commit/push. No generated build/cache/temp/log files, private locators/credentials, campaign data or release artifact belongs in the staged set. Existing Git LF-to-CRLF conversion notices are not whitespace failures.

## Classification, Limits and Stop Boundary

- **A - Reference implementation:** test-only C# stream fixture, public/non-friend compile proof, HTTP/file matrix substitutes, fixed failure/cancellation mapping and mutation probes.
- **B - Managed Service contract:** replaceable providers return the same bounded acquisition result and traverse mandatory independent validation/explicit trust; opaque evidence never grants storage/publication authority.
- **C - Eternal Cycle-wide:** provider freedom and Repository Canon authority remain intact; integrity/provider location/version do not establish trust or readiness; evidence never changes rule meaning or artifact identity.

No real third-party storage transport, live GitHub/private authentication, OS networking or SQL import was tested in E. Deterministic fixture conformance does not certify arbitrary deployments or policies. Historical A-D audits and limitations remain intact. F owns lossless atomic/idempotent import, source-unique conflicts, metadata retention and real SQL evidence; G owns final integrated acceptance. No migration, importer/storage, publication/activation, runtime retrieval/ranking, release packaging/archive/publication, VERSION or tag action is included. Stop after one focused E commit and normal owner-requested `main` push; select nothing next.
