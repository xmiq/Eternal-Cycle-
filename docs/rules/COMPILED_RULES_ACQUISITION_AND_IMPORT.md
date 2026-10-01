# Compiled Rules Acquisition and Import

## Purpose and Status

This contract separates acquisition of an already-produced [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md) from shared validation, installation trust, import, publication, activation, and retrieval. Local/manual distribution, GitHub Releases, mirrors, private stores, and compatible custom providers are alternative byte sources, not different artifact authorities.

**Implementation checkpoint:** FR-025A supplies the portable acquisition interface, bounded byte ownership, acquisition evidence, and failure taxonomy. Shared validation/trust, real local/GitHub providers, provider conformance, and storage integration remain pending in the [FR-025 execution plan](../../design/FR_025_EXECUTION_PLAN.md). Acquiring bytes alone does not parse, approve, import, publish, or activate them.

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

## Identity, Integrity, and Trust

Keep these claims separate:

- **Requested locator/ref:** mutable lookup input retained only in authorized provider configuration. A safe label may be supplied as bounded acquisition metadata after provider sanitization; raw locators are not required by the shared contract.
- **Resolved acquisition identity:** provider evidence such as an object digest or resolved revision. A release/asset numeric ID identifies an object but does not by itself prove its bytes immutable or authentic.
- **Acquired byte SHA-256:** independently identifies exact acquired bytes, regardless of provider. It is not the FR-022 semantic artifact digest and is not publisher authentication.
- **Artifact provenance:** immutable source scheme/value, manifest/source hashes, compiler/release identity inside the validated FR-022 artifact. Acquisition evidence never repairs, rewrites, or substitutes for it.
- **Installation trust:** explicit willingness and authorization to use a technically valid artifact under trusted Ruleset/source/compiler/namespace policy. No trust is inferred from GitHub, official naming, local placement, or a self-consistent hash.

The future shared validator must independently recheck size/hash, strictly decode the exact UTF-8 artifact bytes, invoke the existing FR-022 parser/validator, retain bounded underlying validation codes, and apply explicit compatibility/trust policy before emitting an import-eligible result. It must fail closed on malformed, unsupported, integrity-invalid, or unauthorized input. No new PKI, signature scheme, semantic-equality requirement with official rules, or duplicate validator is authorized.

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

A implements configuration, bounded reading, and safe exception representation; the later-stage codes reserve the agreed taxonomy, not implemented validation/import behavior. Retry guidance must reflect actual stage/idempotency evidence, never presume every failure transient. Cancellation remains `OperationCanceledException` in the reference, not a fabricated provider error.

## Security and Provider Obligations

- All acquired bytes/evidence are untrusted. Evidence is not approved for logging or client serialization; producers must remove secrets and consumers must explicitly select/sanitize authorized fields. Bounded length and safe `ToString()` are not universal credential redaction.
- Local acquisition uses an explicit ordinary file without repository layout or provider-controlled output paths. C must inspect indirection/access policy, reject directories and unintended filesystem targets, read/hash the same opened handle, enforce streaming bounds, and handle concurrent-file changes honestly rather than trusting earlier path/length checks.
- GitHub acquisition selects one published artifact asset, not a repository clone, tag archive, or assumed asset attached to every tag. D must explicitly constrain schemes/endpoints/redirects/credential forwarding, reject ambiguous assets, enforce status/content/stream/time limits, and record actual resolved evidence plus independent byte hash. Tags and discovery pointers remain mutable.
- Consume one ordinary format-1 file. No archive extraction or decompression infrastructure is required; compressed responses, if accepted by a provider, must be bounded after decompression as well as in transport.
- Custom providers use the same limits, evidence, validation, trust, and importer. They need not imitate GitHub or assert equality with official rules. No provider may bypass validation or turn payload text into host instructions, filesystem authority, SQL, or campaign permission.
- Failures retain bounded safe codes, not unrestricted response bodies/exception text. Invalid/failed work preserves current active rules. No normal gameplay UX exposes technical locators or secrets.

## Related Documents

- [FR-025 Execution Plan](../../design/FR_025_EXECUTION_PLAN.md)
- [FR-025A Investigation Audit](../../design/audits/FR_025A_ACQUISITION_ARCHITECTURE_AUDIT.md)
- [Release and Version Provenance](../../design/RELEASE_VERSIONING.md)
- [Controlled Retrieval Vocabulary](CONTROLLED_RETRIEVAL_VOCABULARY.md)
- [Standalone Compiler Reference](../../examples/tooling/rules-compiler-dotnet/README.md)
