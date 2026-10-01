# FR-025B Shared Validation and Explicit Trust Audit

## Authority, Recovery, and Scope

The owner authorized recovery/completion of B beneath the single selected [FR-025 objective](../V1_1_FUTURE_REVISION_PLAN.md#fr-025---provider-neutral-artifact-acquisition-and-import). Starting branch/HEAD/upstream were `main` at `6c9f7ae50ca4be6939aafcc2671f189f29710758`. GitHub's branch matched. Nothing was staged: three modified files and two untracked B files, no unrelated/generated files. The tracked partial diff was 61 additions and 22 deletions, excluding the new files.

Recovered production work was `CompiledRulesAcquisition.cs`, `CompiledRulesArtifact.cs`, and the new `CompiledRulesArtifactValidation.cs`; recovered test work was the test project fixture wiring and new `CompiledRulesArtifactValidationTests.cs`. The entire partial diff/new files were inspected. No unfinished method, TODO, commented-out test, duplicate implementation, or stale caller was found. **No interrupted work was discarded or restarted.** Existing implementation was coherent; six additional test cases and stronger safe-path/index-mutation assertions completed the evidence matrix, then documentation/governance was finished.

The [original A audit](FR_025A_ACQUISITION_ARCHITECTURE_AUDIT.md) remains historical. Its storage findings were checked against the existing Managed staging/store architecture. B implements neither a provider nor importer. FR-025 remains selected/incomplete; **A/B complete, C-G pending, FR-026 pending/unselected**. No migration, Managed source change, campaign data, CLI command, archive/release action, package dependency, or tag movement is included. `VERSION` remains `1.0.0`; tag references remain `v1.0.0` at `6938bd2f9e965faaeb1c612b1dd765237cadc8a8` and `v1.1.0-rc` at `4eb2583027d316a962fd49fe318781a0e060183e`.

## Implemented Boundary

The [portable implementation](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRulesArtifactValidation.cs) uses the established [acquisition/import contract](../../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md#shared-validation-and-explicit-trust):

```text
AcquiredCompiledRulesArtifact + trusted limits + required caller policy
    -> size check before copying
    -> private exact byte snapshot + SHA-256 verification
    -> strict UTF-8 without JSON rewriting
    -> existing FR-022 Read/Validate
    -> frozen validated model + separate bounded evidence
    -> explicit trust decision
    -> immutable ValidatedTrustedCompiledRulesArtifact
```

No provider-specific branch changes validity. No file/URL/stream/repository is reopened. No database, storage scope, publication or campaign binding is selected. There is no implicit approval policy or signing framework. `ICompiledRulesArtifactTrustPolicy` is a genuine replaceable boundary, demonstrated by fixture policies implemented outside the validator. A policy receives only successfully validated state and must evaluate authorization, not acquire, repair, store or activate rules.

`CompiledRulesArtifactValidationResult.IsArtifactValid` and `IsImportEligible` are distinct. Rejection/evaluation failure retain the valid view but expose no trusted result. Approval, rejection, and failure use closed outcome/reason values, never unrestricted policy explanations. Missing policy is a programmer argument error, not default approval. Null decision becomes a failed decision; unexpected synchronous/asynchronous exceptions become `ARTIFACT_TRUST_POLICY_FAILED`. Policy/caller cancellation remains cancellation with a fixed safe message and the corresponding token. Caller cancellation stops waiting on an uncooperative asynchronous policy, not the policy's own execution; it cannot subsequently turn that cancelled evaluation into approval.

## Verified FR-022 Malformed-Input Defects

The existing format/model/digest remain authoritative. Inspection confirmed the following narrow implementation defects; valid format-1 semantics were not changed:

| Finding | Confirmed defect and repair | Executed regression |
| --- | --- | --- |
| Null source/snippet/retrieval entries | Existing validation could proceed to dereference entries despite an earlier error. Reject null top-level entries before traversal and null term/relationship entries before ordering/normalization. | Structured `STRUCTURE_REQUIRED` rejection, no throw/trust invocation. |
| Null source/manifest paths | `Split` preceded the null/empty check. Check before splitting and retain `PATH_INVALID`. | Both null path cases rejected by FR-022 and B. |
| Duplicate/null source identities | After identity errors, `ToDictionary` could still throw for invalid keys. Traverse dependencies only when identities are usable and unique. | Duplicate source and null source ID retain existing identity/duplicate codes, no trust invocation. |
| Recursive dependency traversal | Graph depth is independent of JSON nesting. The old recursive DFS used unbounded process-stack depth for a flat external graph. Replace only traversal with an explicit DFS stack; visiting/completed states and cycle semantics remain unchanged. | 5,000-source acyclic and cyclic graphs complete deterministically; missing/self/cycle rejection also passes. |

The old stack-overflow failure was **not deliberately reproduced in a crashing process**. The unbounded recursive risk is established by code inspection, while the repaired deep-graph behavior is executed evidence. No historical production failure or new format limit is inferred. Eight malformed DTO cases and two deep-graph cases are included in B's 58 focused tests; existing FR-022 conformance tests also pass.

## Identity, Immutability, and Losslessness

Acquired-byte SHA-256 is recomputed from the exact B-owned snapshot and compared with A's independent hash before parsing. The FR-022 semantic digest remains independent of JSON formatting. Separate bounded provider/resolution evidence never rewrites artifact/compiler/source/manifest provenance or enters semantic identity. Hash consistency is integrity, not publisher authentication. Test-only expected-hash and configured-evidence policies demonstrate replaceability, not a new default production policy.

FR-022 DTO collection interfaces are mutable. B freezes all source/snippet, dependency, five selector, retrieval-term and retrieval-relationship collections once, retaining every field and ordinal sequence. Scalar/leaf objects have only immutable/init-only state and need no gratuitous second deep copy. A already copies caller metadata into a read-only ordinal dictionary. Policy and the eventual trusted result share the same frozen model/evidence and private byte snapshot. `CopyBytes()` creates an independent caller buffer; no `ReadOnlyMemory` alias exposes B's backing bytes. Reflection/unsafe code and arbitrary policy side effects are outside this in-process API guarantee.

Tests mutate caller bytes, deliberately unwrap A's buffer through `MemoryMarshal`, attempt policy collection clear/index replacement, mutate caller-owned metadata during and after approval, and alter returned byte copies. Pre-validation byte tampering fails before policy; post-approval mutations cannot change the approved view. Policy mutation exceptions fail closed. Revalidation/writer JSON-tree equality proves complete format-1 information survives, including representative retrieval relationships and selectors as well as canonical reviewed terms.

**F gaps remain pending and unchanged:**

1. Legacy Managed release uniqueness is source-based rather than the intended scoped Ruleset/semantic artifact identity. F must explicitly reconcile/preserve history or reject conflict, not silently overwrite.
2. Existing legacy `CompiledRuleIndex`/storage does not retain all format-1 retrieval metadata. B retains it completely for F's lossless integration; it does not modify storage, add a migration, or claim import idempotency/atomicity.

## Failure and Diagnostic Boundary

B uses existing categories for oversized payload, byte/integrity mismatch, malformed UTF-8/JSON, unsupported format/compiler, semantic validation and trust rejection. `TrustPolicyFailed` and `ValidationFailed` append to A's enum without changing its prior numeric values or codes. Expected parser failures retain bounded original FR-022 codes; unexpected parser/policy exception messages are never echoed. Malformed precedes unsupported, then integrity, then other semantic failures when multiple errors coexist.

Diagnostics are at most 16 entries with explicit truncation; paths and messages are capped at 256 characters. Only known format-1 field names and bounded numeric indices survive location sanitization; unknown property names become `$`, not a truncated secret. Fixed explanations contain no payload values. Huge secret-bearing unknown/duplicate properties, 64 repeated known properties, malicious exception messages and secret-bearing acquisition evidence are covered by tests.

The default `System.Text.Json` serialization of result/view wrappers excludes raw artifact and acquisition evidence; safe `ToString()` also omits them. Explicit policy/import access retains complete information. Deliberate direct serialization of exposed models/evidence still requires field-level authorization/redaction: B is not a universal secret detector or provider-specific HTTP sanitizer. It adds no diagnostic publication endpoint.

## Executed Acceptance and Validation

| Requirement/evidence | Result |
| --- | --- |
| B focused matrix: approval/rejection/failure; invalid input never calls policy; cancellation; provider independence; aliases/mutation; losslessness; bounded disclosure; malformed DTO/deep graph | **58/58** pass. |
| Portable library and standalone CLI Release builds | Pass, zero warnings/errors. |
| Complete portable suite | **397/397** pass. |
| Complete CLI/process suite | **62/62** pass. |
| Complete Managed MCP suite | **181/181** pass; no Managed source/storage behavior changed. |
| Focused FR-025A and FR-023 loader/snippet/writer regressions | **104/104** pass: 34 A, 70 FR-023. |
| Focused FR-024 input/enrichment/pipeline/audit/curation/acceptance regressions | **235/235** pass. |
| FR-022 conformance and portable dependency boundary | **17/17** pass: 16 FR-022, one portable assembly boundary; CLI boundary also passes in complete CLI suite. |
| Structural harnesses | **333/333** pass: FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 operations 81, FR-021 repair 35, FR-022 32, campaign-mode 17. |
| Full repository validator and deterministic distribution/content checks | Pass: 301 Markdown files, 7,376 links, 206 anchors, 191 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. Zero blocking questions, orphaned documents or forbidden campaign-data directories. |

Complete suites total **640 unique tests**, zero failures/skips. Focused reruns are overlapping subsets, not extra unique tests. Commands are existing Release `dotnet build`/`dotnet test` project invocations with `--no-restore` (dependencies already available), focused `FullyQualifiedName` filters, and `pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\validate_repository.ps1`. No interrupted-run result substitutes for these recovered-tree executions.

### Exact-State Repository Evidence

Final documentation/index/governance edits are followed by full diff review, explicit staging, staged whitespace validation, focused B rerun (58/58) and repository validator (the counts above). These checks must all pass on the exact staged content before the one focused commit and owner-authorized normal `main` push. No generated `bin`/`obj`, temporary validation/distribution output, private locator, credential or campaign data is staged. Existing Git LF-to-CRLF conversion notices are not whitespace errors.

## Canonical Compatibility

Executed canonical compilation/validation remains **10 sources / 154 snippets / 275,622 bytes**:

- FR-022 semantic artifact digest: `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`;
- exact serialized byte SHA-256: `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`.

Approved bytes equal compiled input; writing the frozen canonical model reproduces those exact bytes. Existing historical no-vocabulary and curated compiler/CLI evidence remains passing. Repeated deterministic policy evaluation, Turkish culture, unrelated working directory, equivalent JSON reformatting, and differing provider evidence preserve the appropriate independent identities. No compiler, vocabulary, canonical Rule Source, runtime retrieval or campaign mechanic was changed.

## Classification and Remaining Acceptance

- **A - Reference implementation:** .NET interfaces/wrappers, one-time read-only model snapshot, strict decoding, explicit DFS, fixed diagnostic limits, serialization exclusions and tests.
- **B - Managed Service contract:** independent shared validation before explicit trust; no provider bypass; approved/import input cannot drift; structured bounded failures and preserved complete artifact/evidence for separately authorized storage.
- **C - Eternal Cycle-wide:** acquired, valid, trusted, imported and active are distinct; provider freedom and Repository Canon authority remain intact; integrity is not authentication or gameplay readiness.

There is no unresolved design question blocking B. Real local-file acquisition/reparse/race checks, GitHub asset/auth/redirect/timeout behavior, full custom-provider conformance, and real SQL lossless/atomic/idempotent import are **not exercised or claimed**; they belong to C-F and integrated G acceptance. Deterministic policy behavior does not prove a deployment's actual trust configuration. Stop after B commit/push; do not select or implement C-G or FR-026.
