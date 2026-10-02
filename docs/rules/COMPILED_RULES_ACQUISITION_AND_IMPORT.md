# Compiled Rules Acquisition and Import

## Purpose and Status

This contract separates acquisition of an already-produced [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md) from shared validation, installation trust, import, publication, activation, and retrieval. Local/manual distribution, GitHub Releases, mirrors, private stores, and compatible custom providers are alternative byte sources, not different artifact authorities.

**Implementation checkpoint:** A supplies portable acquisition, B shared validation/explicit trust, C/D local/GitHub providers, and E custom-provider conformance. Storage integration and final acceptance remain pending in the [FR-025 execution plan](../../design/FR_025_EXECUTION_PLAN.md). Acquired, valid, trusted, imported, and published/active remain separate; providers do not parse, approve, import or activate artifacts.

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

## GitHub Release Asset Provider

The [D reference provider](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/GitHubReleaseCompiledRulesArtifactProvider.cs), `GitHubReleaseCompiledRulesArtifactProvider`, takes explicit owner, repository, release tag and asset name, optional prerelease permission, bearer token, whole-acquisition timeout and trusted HTTP handler. It requires no Git/checkout, compiler, manifest, installation layout, campaign, SDK or host. It uses standard .NET HTTP in the existing portable library; common A/B APIs remain unchanged. The provider owns/disposes its client/default handler; injected handlers remain caller-owned. Dispose the provider after use, reusing it across acquisitions where appropriate.

```text
explicit repository + tag + exact asset name
    -> repository and published release metadata
    -> bounded complete asset-list selection
    -> asset-ID endpoint / constrained redirects
    -> A exact-byte result
    -> caller invokes B validation / explicit trust
```

### Resolution and Evidence

Selectors are bounded ASCII components, not arbitrary URLs: owner at most 39 characters, repository 100, tag/asset name 256; letters/digits and `-_.+` are accepted, with nonempty slash-separated tag segments also allowed. Traversal/control/URL syntax is rejected; tags are URI-escaped. Tags and asset names match exactly; repository naming is case-insensitive. There is no latest-release search, fuzzy extension matching, fallback, asset-name convention for future releases or source-archive extraction. Draft/unpublished releases are refused; prereleases require explicit permission.

Resolution reads repository ID/full name, the exact published tag release, then constructs asset-list pagination itself (`per_page=100`, maximum ten pages). It ignores server Link/download URLs as routing instructions, bounds each metadata response to 1 MiB/depth 16, validates required fields/duplicate object properties, IDs and API asset URL consistency, and exhausts the bounded list before accepting one exact uploaded asset. Missing matches fail; repeated IDs, malformed entries, ambiguous names and exhausted pagination fail closed. Mutable pagination is not an atomic remote snapshot; actual downloaded bytes, not metadata size, determine acquisition identity.

Evidence uses `github-release`, resolved scheme `github-release-asset` with observed `repositoryId/releaseId/assetId`, and nine bounded requested/resolved repository/tag/name/ID fields. Numeric IDs identify observed objects, **not permanent or immutable bytes**; tags/assets can be replaced/deleted. Byte count and independent SHA-256 remain in A, and artifact source/compiler/manifest provenance is untouched. Expected byte-hash/publisher approval belongs to B policy, not D. Signed URLs, headers, tokens, timestamps and response bodies are not retained as evidence.

### HTTP and Credential Policy

Metadata is fetched only from constructed HTTPS `api.github.com` endpoints with GitHub JSON Accept, User-Agent and supported API-version headers. The numeric asset endpoint uses `Accept: application/octet-stream`; GitHub documents direct `200` and redirected `302` binary delivery. Metadata redirects are not followed. Asset redirects are handled manually, at most three, rejecting loops/missing Location, HTTP downgrade, userinfo, fragments, non-443 ports and arbitrary hosts. Only the exact original API asset endpoint or HTTPS `release-assets.githubusercontent.com` is allowed; relative CDN redirects are resolved and revalidated. This deliberately narrow documented delivery-host policy fails closed if GitHub changes hosts. It is not a DNS/IP pinning or rebinding defense.

Bearer authentication is private constructor configuration, never URL data or a public serialized setting. It is sent only to initial API requests, never replayed across any asset redirect. Requests contain no cookies/default credentials/referrer or carried response headers. Default automatic redirects, decompression and cookie handling are disabled; unsafe built-in injected handlers are rejected. Arbitrary custom handlers are trusted code and must preserve these policies; this API cannot sandbox their logging/egress. Operating-system TLS/DNS/proxy configuration and explicitly enabled external HTTP tracing remain deployment responsibilities, not permission to log signed URLs or secrets.

Requests ask for identity encoding and reject encoded responses; no ZIP or decompression path exists. Metadata must select JSON. Vendor GitHub JSON at the asset endpoint is rejected as wrong representation; ordinary JSON or odd file media types do not decide artifact validity. Declared oversized file content can fail early, but A's limit-plus-one stream reader independently enforces the actual bytes (default 16 MiB). No BOM/newline/JSON transformation occurs.

The whole acquisition, including metadata, pagination, redirects and body reads, has a two-minute default deadline, explicitly configurable up to ten minutes; default connect timeout is 30 seconds and headers are bounded. Caller cancellation propagates with a fixed message/token; provider deadline becomes transport failure. No automatic retries occur. Requests/responses/body streams are disposed on every outcome; cancellation remains cooperative for injected transport code.

### GitHub Failure Mapping

| Condition | Existing A category |
| --- | --- |
| Missing configuration, invalid token/timeout, unsafe built-in handler | `ProviderConfigurationInvalid` |
| Invalid selectors, ambiguous asset, rejected/looping/excess redirect | `LocatorInvalid` |
| 404/401/non-rate-limit 403, absent asset, draft/disallowed prerelease, unuploaded asset | `Unavailable` |
| 429 or 403 with Retry-After/zero remaining quota, other non-200 status, malformed/overbound metadata, negotiation/encoding failure, network/read/deadline failure | `TransportFailed` |
| Actual/declared asset bound exceeded | `PayloadTooLarge` |
| Caller/transport cancellation | `OperationCanceledException`, never partial success |

Safe codes/messages are A's unchanged fixed taxonomy. Rate-limit responses with explicit evidence are distinguishable from not-found by category; unknown 403 causes are not inferred from bodies. No raw API body, error HTML, signed redirect URL, token, header dump or exception is echoed. Tests prove C/D same-byte equivalence and intentional evidence-sensitive B policy; real Release/private-auth delivery is separate external acceptance, not implied by HTTP substitutes. See the [D audit](../../design/audits/FR_025D_GITHUB_PROVIDER_AUDIT.md).

## Custom Provider Implementer Boundary

An external implementation needs only the public `ICompiledRulesArtifactProvider.AcquireAsync(limits, cancellationToken)` contract. Select/configure the provider explicitly outside A/B; object keys, endpoints, credentials, transport lifetime and any deadline are provider responsibilities. There is no provider registry, priority, discovery or fallback chain. A logical object key need not be a file path or URL.

Return `AcquiredCompiledRulesArtifact` directly. For a stream, use its public `ReadAsync` helper with trusted `CompiledRulesAcquisitionLimits` and `CompiledRulesAcquisitionEvidence`; the provider must dispose its stream. For already buffered bytes, its constructor bounds and copies the payload before calculating SHA-256. Do not decode, repair, normalize, approve or store bytes during acquisition. Propagate cancellation and map expected acquisition failures to the existing fixed `CompiledRulesAcquisitionException` categories without raw transport messages. Transport/decompression limits and cooperative cancellation remain implementer obligations, not an in-process sandbox.

Provider kind and resolved identity scheme/value are open bounded strings, not built-in enums or interpreted locators. Optional metadata can carry opaque object versions/generations, subject to A's existing count/text/control/duplicate bounds. Remove secrets before constructing evidence; bounded evidence is not automatically safe to disclose. Resolved identities are observations/claims, not artifact provenance or trust. Only an explicitly supplied B policy may interpret them for authorization.

After acquisition, the caller invokes unchanged `CompiledRulesArtifactValidation.ValidateAsync` with trusted limits and a required `ICompiledRulesArtifactTrustPolicy`. B independently snapshots/rehashes bytes, validates FR-022, freezes the model and evaluates policy. A owns copies of provider source buffers and metadata; deliberate extraction of A's public `ReadOnlyMemory` backing array is detected before validation and cannot alter B's already-approved snapshot. Approved byte copies and read-only collections preserve the existing ordinary-alias guarantees, not protection against reflection/unsafe code.

The [E memory-store fixture](../../examples/tooling/rules-compiler-dotnet/tests/Shared/MemoryStoreCompiledRulesArtifactProvider.cs) implements only this surface; it is test-only, not a third production transport or URI scheme. It is also compiled in the existing CLI test assembly **without friend access**, proving public API sufficiency. [Conformance tests](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/CompiledRulesProviderConformanceTests.cs) compare actual C/D/E providers using identical canonical bytes through unchanged B, exercise opaque evidence-sensitive policies and validate bounds, failures, cancellation and mutation safety. The [E audit](../../design/audits/FR_025E_CUSTOM_PROVIDER_CONFORMANCE_AUDIT.md) records the matrix and limitations. There are no production corrections, importer/SQL changes or new plugin infrastructure in E.

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
- D acquires one exact published GitHub Release asset through bounded metadata, HTTPS/API and constrained credential-free redirects; it does not clone sources or assume every tag has an artifact. Its observed IDs/evidence and byte hash do not make tags/assets immutable or approve them.
- Consume one ordinary format-1 file. No archive extraction or decompression infrastructure is required; compressed responses, if accepted by a provider, must be bounded after decompression as well as in transport.
- Custom providers use the same limits, evidence, validation, trust, and importer. They need not imitate GitHub or assert equality with official rules. No provider may bypass validation or turn payload text into host instructions, filesystem authority, SQL, or campaign permission.
- Failures retain bounded safe codes, not unrestricted response bodies/exception text. Invalid/failed work preserves current active rules. No normal gameplay UX exposes technical locators or secrets.

## Related Documents

- [FR-025 Execution Plan](../../design/FR_025_EXECUTION_PLAN.md)
- [FR-025A Investigation Audit](../../design/audits/FR_025A_ACQUISITION_ARCHITECTURE_AUDIT.md)
- [FR-025B Validation and Trust Audit](../../design/audits/FR_025B_VALIDATION_TRUST_AUDIT.md)
- [FR-025C Local Provider Audit](../../design/audits/FR_025C_LOCAL_PROVIDER_AUDIT.md)
- [FR-025D GitHub Provider Audit](../../design/audits/FR_025D_GITHUB_PROVIDER_AUDIT.md)
- [FR-025E Custom Provider Conformance Audit](../../design/audits/FR_025E_CUSTOM_PROVIDER_CONFORMANCE_AUDIT.md)
- [Release and Version Provenance](../../design/RELEASE_VERSIONING.md)
- [Controlled Retrieval Vocabulary](CONTROLLED_RETRIEVAL_VOCABULARY.md)
- [Standalone Compiler Reference](../../examples/tooling/rules-compiler-dotnet/README.md)
