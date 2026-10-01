# Compiled Rules Acquisition and Import

## Purpose and Status

This contract separates acquisition of an already-produced [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md) from shared validation, installation trust, import, publication, activation, and retrieval. Local/manual distribution, GitHub Releases, mirrors, private stores, and compatible custom providers are alternative byte sources, not different artifact authorities.

**Implementation checkpoint:** FR-025A supplies the portable acquisition interface, bounded byte ownership, acquisition evidence, and failure taxonomy. FR-025B implements shared FR-022 validation, explicit trust evaluation, and an immutable import-eligible result. FR-025C adds the explicit local ordinary-file provider. GitHub acquisition, custom-provider conformance, and storage integration remain pending in the [FR-025 execution plan](../../design/FR_025_EXECUTION_PLAN.md). Acquired, valid, trusted, imported, and published/active are separate states; C only reads configured file bytes.

## Document Control

- **Owner:** provider-neutral compiled-artifact acquisition, resource bounds, acquisition evidence, shared validation/trust and import boundaries
- **Dependencies:** [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md), [Managed Rule Publication](MANAGED_RULE_PUBLICATION.md), [Logical Data Namespace](../persistence/LOGICAL_DATA_NAMESPACE.md), and [Managed Data Service](../persistence/MANAGED_DATA_SERVICE.md)
- **Extensions:** configured local, release-host, and custom providers; authorized storage adapters; later searchable projections under FR-026
- **Consumers:** Direct/Managed installers, service implementers, administrators, conformance tests, and future release tooling
- **Repository boundary:** no private locator, provider credential, populated import receipt, campaign state, or rule database belongs here

## Responsibility Chain

```text
configured provider
    -> bounded acquired bytes + acquisition evidence (untrusted)
    -> shared independent byte hash + FR-022 parsing/validation
    -> explicit installation compatibility/trust decision
    -> shared authorized import into rule storage
    -> separately authorized publication/activation
    -> FR-026 searchable representation and retrieval
```

FR-022 alone defines artifact structure, parser/semantic validation, identity, dependencies, integrity, and applicability. FR-023 compiles materialized Rule Sources; FR-024 authors/enriches/audits reviewed metadata. FR-025 acquires and imports their output. Source acquisition followed by compilation is not compiled-artifact acquisition. MCP is a possible control interface, not an acquisition contract or lifetime owner.

Providers do not validate artifact semantics or choose import/activation policy. There is one validation/import path after acquisition, not a GitHub importer alongside local/custom importers. The portable boundary requires no Git, GitHub, Windows, SQL Server, MCP, repository checkout, installation layout, or Campaign ID. Provider-specific configuration and credentials stay outside it.

## Portable Acquisition Contract

The [reference contract](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRulesAcquisition.cs) provides:

| Type | Meaning |
| --- | --- |
| `ICompiledRulesArtifactProvider` | An already-configured provider accepts trusted byte limits and cancellation, then returns acquired bytes/evidence or a structured failure. It does not expose a semantic-validity assertion. |
| `CompiledRulesAcquisitionLimits` | Positive installation-selected maximum payload bytes; the reference default is 16 MiB, comfortably above the current 275,622-byte artifact. This is a resource policy, not an artifact-format limit. |
| `AcquiredCompiledRulesArtifact` | An owned copy of exact bytes, independently calculated uppercase byte SHA-256, and separate acquisition evidence. Even malformed/empty/non-UTF-8 bytes can be acquired; shared validation must reject them later. |
| `CompiledRulesAcquisitionEvidence` | Open provider kind, optional resolved provider identity, and bounded ordinal metadata. No artifact provenance fields or trust grant. |
| `CompiledRulesAcquisitionIdentity` | Provider-reported scheme/value for resolved acquisition identity where established; absence is legitimate. A tag or requested path cannot masquerade as an immutable resolution. |
| `CompiledRulesAcquisitionFailure` / exception | Fixed category/code/message, without provider response bodies, payloads, locators, credentials, or raw inner exceptions. |

The bounded stream reader never relies on declared length, seeking, or filesystem layout. It reads at most the allowed bytes plus a one-byte overflow probe, rejects excess before appending it, returns its temporary buffer, and leaves stream ownership with the provider. Already-buffered construction checks size before copying. Cancellation propagates as cancellation, not transport failure. Providers must also bound transport/decompression and honor cancellation; the interface is not a sandbox for arbitrary in-process provider code.

Evidence is copied, read-only, and limited to 16 metadata entries, 128-character keys, and 1,024-character values; provider kind/identity scheme are at most 128 characters, identity value at most 1,024. Empty/control-containing values and duplicate keys fail structurally. Metadata is not artifact content and does not enter its digest. Default object string representations never echo evidence or payloads.

## Explicit Local File Provider

The [C reference provider](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/LocalCompiledRulesArtifactProvider.cs), `LocalCompiledRulesArtifactProvider`, implements the existing interface. Its only public configuration is an explicit fully qualified artifact path. Relative, drive-relative and control-containing paths fail; accepted paths are normalized once using the platform's `Path` APIs. There is no implicit base, CWD lookup, repository/installation discovery, directory scan, Git, HTTP, manifest requirement or Campaign ID.

```text
explicit absolute ordinary file
    -> component checks
    -> one read-only opened stream + handle checks
    -> existing A bounded streaming/hash result
    -> caller invokes B with explicit trust policy
```

### File Safety and Race Boundary

The provider rejects reparse/symbolic-link/device attributes on the root and every normalized parent/final component; parents must be directories and the final target must not be a directory. Windows device namespaces and alternate data streams are rejected. Normal platform paths (including filesystem share paths) retain ordinary OS access semantics, not a new URL/network provider. The opened handle's attributes and the stream's readable/seekable capabilities are checked before reading. Components are rechecked after opening and after reading. Like FR-023, indirection is rejected rather than followed; its manifest/root-specific private logic is not copied into a general filesystem framework or changed by C.

All bytes are acquired from the same opened `FileStream` through A's limit-plus-one reader. No pre-read length/timestamp selects content identity or overrides streaming bounds. A's default remains 16 MiB; overbound reads never yield truncated success. Bytes are not decoded, newline-normalized, BOM-stripped or rewritten. The reference uses read-only access and `FileShare.Read`, requests asynchronous sequential reads, and disposes the handle on every outcome. Cancellation is checked around filesystem steps and passed into reads; cancellation cannot become acquisition success.

These controls **do not prove race-free filesystem containment or a coherent file snapshot**. Portable checks cannot atomically lock every ancestor or detect a replace-and-restore link race between observations; handle attributes identify the opened object's type, not an atomic identity match with every inspected path. Ordinary content may change on platforms whose sharing semantics permit it. The authoritative hash covers the exact acquired bytes, even if concurrent mutation creates a mixed observation; B still must validate those bytes. Synchronous OS metadata/open calls cannot be forcibly cancelled while blocked. Not every platform exposes all special-object types before open; detected device/nonseekable objects are rejected, but C is not a sandbox against hostile filesystem races/special objects. Use an explicitly authorized ordinary file in controlled storage. Windows cases are executed evidence; non-Windows special-file/race behavior requires platform acceptance, not inferred proof.

### Local Evidence and Failures

Successful output is exactly `AcquiredCompiledRulesArtifact`, directly consumable by B. Minimal evidence is the constant provider kind `local-file`, no resolved immutable identity, and no locator metadata. The private configured path is not an artifact ID and is omitted from provider display/default serialization and returned evidence. Byte count/hash remain in A's result; FR-022 provenance is untouched. Same bytes at different paths have identical byte/semantic identity; changing bytes at the same path changes actual acquisition identity.

| Local condition | Existing A category / code |
| --- | --- |
| Missing/null/blank configuration | `ProviderConfigurationInvalid` / `ARTIFACT_PROVIDER_CONFIGURATION_INVALID` |
| Invalid/relative path, directory, non-directory parent, prohibited indirection/device/alternate stream or detected nonordinary handle | `LocatorInvalid` / `ARTIFACT_LOCATOR_INVALID` |
| Missing file/component, permission/security denial | `Unavailable` / `ARTIFACT_UNAVAILABLE` |
| OS open/read I/O failure | `TransportFailed` / `ARTIFACT_TRANSPORT_FAILED` |
| Streaming limit exceeded | `PayloadTooLarge` / `ARTIFACT_PAYLOAD_TOO_LARGE` |
| Cancellation | `OperationCanceledException`, not rejection or provider failure |

Failure code/message are fixed and retain no raw OS exception or inner exception. Diagnostic callers select those safe fields rather than exposing stack traces or private configuration. No permission to log local paths follows from successful acquisition. Malformed JSON, invalid UTF-8 and empty files are valid acquisition outcomes; only B determines artifact validity/trust. Local placement never implies approval, import, publication or readiness.

## Identity, Integrity, and Trust

Keep these claims separate:

- **Requested locator/ref:** mutable lookup input retained only in authorized provider configuration. A safe label may be supplied as bounded acquisition metadata after provider sanitization; raw locators are not required by the shared contract.
- **Resolved acquisition identity:** provider evidence such as an object digest or resolved revision. A release/asset numeric ID identifies an object but does not by itself prove its bytes immutable or authentic.
- **Acquired byte SHA-256:** independently identifies exact acquired bytes, regardless of provider. It is not the FR-022 semantic artifact digest and is not publisher authentication.
- **Artifact provenance:** immutable source scheme/value, manifest/source hashes, compiler/release identity inside the validated FR-022 artifact. Acquisition evidence never repairs, rewrites, or substitutes for it.
- **Installation trust:** explicit willingness and authorization to use a technically valid artifact under trusted Ruleset/source/compiler/namespace policy. No trust is inferred from GitHub, official naming, local placement, or a self-consistent hash.

The shared validator independently rechecks size/hash, strictly decodes the exact UTF-8 artifact bytes, invokes the existing FR-022 parser/validator, retains bounded underlying validation codes, and applies explicit compatibility/trust policy before emitting an import-eligible result. It fails closed on malformed, unsupported, integrity-invalid, or unauthorized input. No new PKI, signature scheme, semantic-equality requirement with official rules, or duplicate validator is authorized.

## Shared Validation and Explicit Trust

The [B reference implementation](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRulesArtifactValidation.cs) accepts an `AcquiredCompiledRulesArtifact`, trusted `CompiledRulesAcquisitionLimits`, a required `ICompiledRulesArtifactTrustPolicy`, and cancellation. It never reopens a provider stream, file, URL, or repository.

```text
independent byte-limit check
    -> owned byte snapshot + recomputed acquired-byte SHA-256
    -> strict UTF-8 decode without JSON rewriting
    -> CompiledRulesArtifactContract.Read (FR-022 parsing and validation)
    -> read-only validated artifact + separate bounded acquisition evidence
    -> explicit policy evaluation
    -> immutable validated/trusted import-eligible result
```

| Type / API | Boundary |
| --- | --- |
| `CompiledRulesArtifactValidation.ValidateAsync` | Orchestrates these stages; does not duplicate artifact validity rules or import anything. |
| `ValidatedCompiledRulesArtifact` | Complete format-1 model, exact privately owned bytes, byte hash, semantic digest, and separate acquisition evidence, available only after FR-022 success. |
| `ICompiledRulesArtifactTrustPolicy.EvaluateAsync` | Receives that validated view and cancellation; returns explicit approval, rejection, or evaluation failure. No default approval policy exists. |
| `CompiledRulesArtifactTrustDecision` | Closed outcome/reason evidence: explicit approval/rejection, configured expected-byte-hash match/mismatch, policy unavailable/exception/invalid decision. A hash match is not publisher authentication. |
| `CompiledRulesArtifactValidationResult` | Separates `IsArtifactValid` from `IsImportEligible`, carries bounded diagnostics and optional policy decision, and retains the validated view even after rejection. |
| `ValidatedTrustedCompiledRulesArtifact` | Exists only after validation and approval; shares the exact view seen by policy. Future F may import it under separately authorized scope; it is not an import receipt. |

Policy is supplied by the caller, outside validation. Identical bytes receive the same FR-022 validity regardless of provider evidence. Configured policy may deliberately approve one evidence set and reject another without changing artifact provenance or semantic identity. Policy must evaluate authorization, not reacquire/repair artifacts, store them, or publish/activate them. The interface does not sandbox arbitrary in-process policy code.

Expected invalid input never invokes policy. Unexpected validator failure is `ARTIFACT_VALIDATION_FAILED`; unexpected policy exceptions or null decisions produce `ARTIFACT_TRUST_POLICY_FAILED`, never approval or raw exception text. Cancellation remains cancellation with its token and a fixed safe message. Caller cancellation stops awaiting an asynchronous policy even if it ignores the token; it does not forcibly terminate third-party code. A cancelled evaluation cannot later return approval.

### Approval Immutability and Losslessness

FR-022 assembly DTOs have mutable collection interfaces. B freezes every source/snippet, selector, dependency, term, and relationship collection once into read-only copies, reusing immutable scalar/leaf values. Caller-owned acquisition metadata is already copied/read-only by A. Policy and the successful result see the same frozen model/evidence; all format-1 provenance, content, hashes, applicability, dependencies, and retrieval metadata remain intact. The unchanged FR-022 writer can serialize and revalidate that model without semantic loss.

B does not expose its owned byte buffer. `CopyBytes()` returns an independent copy; deliberate extraction/mutation of A's backing array before validation causes byte-hash rejection, and mutation after approval cannot alter B's snapshot. There is no extra artifact ID, JSON normalization, or reserialization on this path. This boundary protects ordinary managed aliases, not reflection/unsafe code acting outside the API contract.

The known F gaps remain **unimplemented**: legacy source-based storage uniqueness and incomplete format-1 retrieval metadata retention. B preserves the full input F needs but does not change that storage, its schema, or publication behavior.

### Safe Validation Diagnostics

B returns at most 16 underlying FR-022 diagnostics with explicit truncation, 256-character path/message limits, fixed category messages, and closed policy reasons. Only known format-1 field names and bounded numeric indices survive in locations; unknown/attacker-controlled property names become `$`. Artifact values, raw exception messages, and provider bodies never become explanations. Failure classification is deterministic: malformed JSON/duplicate properties, unsupported format/compiler, integrity failures, then other semantic failures.

The complete artifact and acquisition evidence are explicitly available to policy/future import code but excluded from default `System.Text.Json` serialization of the result/view wrappers. Safe `ToString()` values also omit them. Deliberately serializing the exposed model/evidence still requires authorized field selection/redaction; this is not a universal credential sanitizer, HTTP policy, or permission to disclose Rule payloads.

## Shared Import Requirement

After validation and explicit trust approval, one importer consumes the validated result and a separately authorized storage scope. Providers cannot supply a Campaign ID, datastore route, active-release pointer, SQL, or publication permission through evidence.

Within that scope, imported semantic identity is the validated Ruleset and FR-022 semantic digest, not a URL, path, tag, provider, or fetch count. Reacquiring/reimporting the same semantic artifact must return the same stored identity without duplicate releases. Different JSON formatting can produce a different byte hash but the same semantic import identity; distinct acquisition observations remain separate evidence.

Import must retain complete artifact provenance, snippets, selectors, dependencies, and retrieval metadata losslessly. Candidate storage must be atomic; invalid input causes no storage writes, and storage failure exposes no half-written corpus. Publication, activation, campaign adoption, and readiness remain separate gates under existing authorization, compatibility, preparation, and migration rules. A successful import is not itself gameplay readiness.

The legacy Managed store has source-unique releases and transactionally staged chunks, not a format-1 importer. FR-025 integration must preserve its history and active release and either reconcile a conflicting source identity explicitly or return `ARTIFACT_IMPORT_CONFLICT`; it must not silently overwrite an older release. Storage work is deferred, not claimed complete by this contract. FR-026 owns searchable projections/ranking, not alternate provider validation.

## Failure Taxonomy

| Category | Stable reference code | Owner |
| --- | --- | --- |
| Configuration/evidence invalid | `ARTIFACT_PROVIDER_CONFIGURATION_INVALID` | Provider/configuration boundary |
| Locator invalid | `ARTIFACT_LOCATOR_INVALID` | Provider |
| Unavailable/not found | `ARTIFACT_UNAVAILABLE` | Provider |
| Transport/read failure | `ARTIFACT_TRANSPORT_FAILED` | Provider/bounded reader |
| Oversized payload | `ARTIFACT_PAYLOAD_TOO_LARGE` | Acquisition and shared boundary |
| Integrity mismatch | `ARTIFACT_INTEGRITY_MISMATCH` | Shared validation |
| Malformed artifact | `ARTIFACT_MALFORMED` | Shared FR-022 validation |
| Unsupported format/compiler contract | `ARTIFACT_FORMAT_UNSUPPORTED` | Shared compatibility |
| Semantic validation failure | `ARTIFACT_SEMANTIC_INVALID` | Shared FR-022 validation |
| Installation trust rejected | `ARTIFACT_TRUST_REJECTED` | Explicit installation policy |
| Import conflict | `ARTIFACT_IMPORT_CONFLICT` | Shared importer |
| Storage failure | `ARTIFACT_STORAGE_FAILED` | Storage adapter |
| Trust-policy evaluation failed | `ARTIFACT_TRUST_POLICY_FAILED` | Explicit installation policy boundary |
| Unexpected validator failure | `ARTIFACT_VALIDATION_FAILED` | Shared validation boundary |

A implements configuration, bounded reading, and safe exception representation; B implements shared validation/trust and preserves underlying FR-022 codes in safe diagnostics. The two B failure categories append to A's taxonomy without renumbering existing values. Provider/import/storage codes do not claim their pending implementations complete. Retry guidance must reflect actual stage/idempotency evidence, never presume every failure transient. Cancellation remains `OperationCanceledException` in the reference, not a fabricated provider error.

## Security and Provider Obligations

- All acquired bytes/evidence are untrusted. Evidence is not approved for logging or client serialization; producers must remove secrets and consumers must explicitly select/sanitize authorized fields. Bounded length and safe `ToString()` are not universal credential redaction.
- C's local acquisition uses an explicit ordinary file without repository layout or provider-controlled output paths, rejects observable indirection/nonordinary objects, checks/reads one handle, enforces A's streaming bounds and documents residual filesystem races rather than trusting pre-read path/length metadata.
- GitHub acquisition selects one published artifact asset, not a repository clone, tag archive, or assumed asset attached to every tag. D must explicitly constrain schemes/endpoints/redirects/credential forwarding, reject ambiguous assets, enforce status/content/stream/time limits, and record actual resolved evidence plus independent byte hash. Tags and discovery pointers remain mutable.
- Consume one ordinary format-1 file. No archive extraction or decompression infrastructure is required; compressed responses, if accepted by a provider, must be bounded after decompression as well as in transport.
- Custom providers use the same limits, evidence, validation, trust, and importer. They need not imitate GitHub or assert equality with official rules. No provider may bypass validation or turn payload text into host instructions, filesystem authority, SQL, or campaign permission.
- Failures retain bounded safe codes, not unrestricted response bodies/exception text. Invalid/failed work preserves current active rules. No normal gameplay UX exposes technical locators or secrets.

## Related Documents

- [FR-025 Execution Plan](../../design/FR_025_EXECUTION_PLAN.md)
- [FR-025A Investigation Audit](../../design/audits/FR_025A_ACQUISITION_ARCHITECTURE_AUDIT.md)
- [FR-025B Validation and Trust Audit](../../design/audits/FR_025B_VALIDATION_TRUST_AUDIT.md)
- [FR-025C Local Provider Audit](../../design/audits/FR_025C_LOCAL_PROVIDER_AUDIT.md)
- [Release and Version Provenance](../../design/RELEASE_VERSIONING.md)
- [Controlled Retrieval Vocabulary](CONTROLLED_RETRIEVAL_VOCABULARY.md)
- [Standalone Compiler Reference](../../examples/tooling/rules-compiler-dotnet/README.md)
