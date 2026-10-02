# FR-025 Acquisition, Validation, and Import Execution Plan

## Authority and Current Checkpoint

The owner selected [FR-025](V1_1_FUTURE_REVISION_PLAN.md#fr-025---provider-neutral-artifact-acquisition-and-import) from clean synchronized `main` at `a34c268d239bc9b195200e2198bb899a60ad380b`, after FR-022/023/024 closure. A-G are execution checkpoints beneath that single Future Revision, not additional objectives or Project Phases. FR-025 is complete/Closed; FR-026 and every other objective remain unselected.

**A-G complete:** A established the [acquisition/import contract](../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md); [B](audits/FR_025B_VALIDATION_TRUST_AUDIT.md) implements shared validation/explicit trust; [C](audits/FR_025C_LOCAL_PROVIDER_AUDIT.md) acquires local files and [D](audits/FR_025D_GITHUB_PROVIDER_AUDIT.md) exact GitHub Release assets. [E](audits/FR_025E_CUSTOM_PROVIDER_CONFORMANCE_AUDIT.md) proves custom-provider replaceability. [F](audits/FR_025F_LOSSLESS_IMPORT_AUDIT.md) implements trusted-only lossless atomic/idempotent import with additive migration 011. [G's closure audit](audits/FR_025_ACQUISITION_AND_IMPORT_AUDIT.md) records integrated acceptance with no production correction, runtime retrieval change or publication bypass.

## Existing Architecture Map

| Responsibility | Current owner / implementation | Actual boundary |
| --- | --- | --- |
| Rule Source acquisition | Managed `IRuleSourceProvider`, `GitRuleSourceProvider`, persisted `RuleSourceConfiguration` | Acquires canonical source material and resolves Git provenance; not compiled artifacts. |
| Local filesystem input | FR-023 `MaterializedRuleSourceLoader`; Managed Git provider; C `LocalCompiledRulesArtifactProvider` | C reads one explicit absolute ordinary artifact file into A bytes/evidence; materialized manifest sources and committed Git inputs remain separate. |
| Git/GitHub input | Managed Git source runner/provider; D standard .NET HTTP artifact adapter | Source materialization remains separate; D resolves/downloads an exact published Release asset without Git or local compilation. |
| Compiled artifact acquisition | A portable interface/bounds; C local, D GitHub and E test-only custom providers | Same byte result; E proves replaceability without production transport or public-contract changes. |
| MCP/Managed interface | `ec_configure_rule_source`, `ec_publish_initial_rules`, operation status/readiness, `ec_get_rule_context` | Administrative source publication plus independent durable worker; runtime context reads already-published rules. No format-1 import endpoint. |
| Artifact parsing / validation | Portable `CompiledRulesArtifactContract.Read` / `Validate`, B `CompiledRulesArtifactValidation.ValidateAsync` | FR-022 owns validity; B owns independent byte checks, orchestration, safe diagnostics, explicit policy evaluation and frozen approval state, not acquisition or storage. |
| Compilation / writing / audit | FR-023/024 portable pipeline and CLI | Materialized bytes to format 1; reviewed enrichment and observational audit; no acquisition/import. |
| Trust checks | B `ICompiledRulesArtifactTrustPolicy` plus existing source/publication controls | Required caller policy receives only FR-022-valid artifacts; validity is not approval. No default production policy, storage or activation authority. |
| Import | Portable `CompiledRulesArtifactImport`, configured `SqlServerCompiledRulesArtifactStore` | Only B-approved input plus explicit Ruleset scope; complete format-1 candidate import/readback, no publication or activation. |
| Publication | `ManagedRulePublicationCoordinator`, `IPublishedRuleStore` | Source acquisition, legacy compilation, candidate stage/validate/publish, configured activation, progressive preparation. |
| Rule storage | Legacy `SqlServerPublishedRuleStore`; F artifact-owned tables, migration 011 | Same logical Domain, preserved old publication/history and complete imported candidates; no second runtime/search representation. Campaign facts live elsewhere. |
| Retrieval | `PublishedRuleContextProvider`, `RuleCompiler.Retrieve`, compact formatter | Existing selectors, dependencies, mandatory sources and budget; no FR-026 vocabulary ranking. |

The [A investigation audit](audits/FR_025A_ACQUISITION_ARCHITECTURE_AUDIT.md) records inspected code and tests. Historical closure audits remain [FR-022](audits/FR_022_COMPILED_RULES_ARTIFACT_AUDIT.md), [FR-023](audits/FR_023_REPRODUCIBLE_RULES_COMPILER_AUDIT.md), and [FR-024](audits/FR_024_CONTROLLED_RETRIEVAL_VOCABULARY_AUDIT.md).

## Target Architecture and Gaps

```text
local file / GitHub Release asset / compatible custom provider
    -> same bounded acquired bytes + separate acquisition evidence
    -> shared byte hash + existing FR-022 parser/validator
    -> explicit installation compatibility/trust approval
    -> one storage-neutral import orchestration
    -> lossless versioned rule storage adapter
    -> separate publication/activation/preparation
    -> FR-026 searchable projection and retrieval
```

Final integrated objective acceptance is complete. Shared validation/trust, all providers and F's lossless storage compose without a production correction. Git source acquisition is retained, not repurposed into a release-asset importer. No established FR-022/023/024 contract conflict was found.

The legacy physical key `(ruleset_id, source_identity)` and `CompiledRuleIndex` omit artifact semantic digest identity and full retrieval metadata. F preserves them unchanged and uses artifact-local source/snippet/dependency storage plus exact first-approved bytes. Full retrieval metadata is retained in normative snippet JSON. Conflicting readback fails rather than overwriting history. This does not authorize implementing searchable/ranked storage now.

## Dependency Direction and Trust

The provider interface, evidence, bounds, and shared validation belong in portable `EternalCycle.Rules`; they require only the established standard-library dependencies. Configured provider implementations may depend on filesystem/HTTP APIs in an appropriate adapter component. Managed may consume the portable result and retain storage-specific routing/transactions. Portable code must not depend on Managed, SQL, MCP, hosting, Git/GitHub SDKs, or campaign state.

A's owned byte hash is independent of provider metadata. B recomputes at its boundary and invokes the existing parser/validator rather than trusting a provider's assertion or inventing another format. Technical validity is not publisher authentication or installation approval. Explicit caller policy, not official naming/provider choice, determines acceptable Ruleset/source/compiler/scope. Valid custom/fork/house-rule artifacts need not match official semantic content. No signing/PKI infrastructure is required.

Provider locators stay in authorized configuration. A path/tag is mutable; byte hash identifies acquired bytes. Optional resolved identity is a bounded provider claim, not the artifact source identity. GitHub asset/release IDs do not prove immutable bytes. Reference exceptions use fixed safe messages; provider evidence requires field-level review/redaction before diagnostics. The [contract](../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md#failure-taxonomy) defines all failure categories, with later-stage execution deferred.

## Security Review

| Risk | A control / required later evidence |
| --- | --- |
| Unlimited payload or misleading length | A checks buffered payload before copying; shared stream reader stops at limit plus one, no seeking; default 16 MiB. B independently enforces bounds before its byte snapshot. |
| Traversal, links/reparse points, unintended local files, TOCTOU | C checks every explicit absolute component, rejects links/reparse/device/directory inputs, checks/reads one handle through A and rechecks components. Persistent link swaps and controlled mutation tests pass. Transient races, coherent snapshots and blocked OS calls are not guaranteed. FR-023 remains unchanged. |
| Decompression bombs | D disables decompression, requests identity encoding and rejects encoded responses; no archive support. A still bounds actual bytes. |
| Redirects/content type/provider filenames | D constructs API endpoints, allows only exact API asset/CDN redirects, never replays credentials across redirects, bounds metadata/deadline/headers and disposes responses. Filenames select no local output. |
| Ambiguous assets / mutable refs | D exhausts bounded explicit asset pagination, rejects ambiguity and records observed IDs/hash; tag/numeric IDs never claim immutable bytes. Remote listings are not atomic snapshots. |
| Diagnostic leakage | A bounds evidence; B bounds FR-022 diagnostics and allowlists locations, contains policy exceptions, and excludes raw models/evidence from default result serialization. Explicit evidence disclosure still requires authorization/redaction. |
| Mutation after approval | B privately snapshots/rehashes exact bytes, freezes all model collections once, and shares immutable policy/import views. Tests mutate byte/evidence aliases and policy-visible lists. |
| Partial writes / activation | A writes nothing. F tests atomic candidate storage and rollback/preservation; publication/activation remain separately authorized. |
| Same source but different artifact digest | F owns each source under its imported artifact, preserving old hashes/metadata and legacy releases; no silent upsert overwrite. |

## FR-025A - Investigation and Portable Acquisition Foundation

- **Status:** complete; [checkpoint audit](audits/FR_025A_ACQUISITION_ARCHITECTURE_AUDIT.md).
- **Inputs:** FR-022/023/024, Managed source/publication code, setup/operations/configuration, existing tests, release/distribution rules.
- **Outputs:** architecture map, contract, bounded provider/evidence/byte API, fixed failure taxonomy, contract tests, this plan, selected-objective governance.
- **Components:** `CompiledRulesAcquisition.cs`, `CompiledRulesAcquisitionTests`, documentation/navigation and governance validator expectations.
- **Acceptance/validation:** exact copied bytes/hash, arbitrary unvalidated input, distinct immutable-evidence claim, bounded metadata/payload, cancellation, fixed failures, fake custom provider, canonical unchanged; Release builds, complete portable/CLI/Managed compatibility, structural/full repository checks.
- **Exclusions/stop:** no parser/trust/import implementation, real provider, SQL change or downstream selection. Stop after focused validated commit and owner-requested normal `main` push.

## FR-025B - Shared Validation and Trust Gate

- **Status:** complete; **depends on A**; [checkpoint audit](audits/FR_025B_VALIDATION_TRUST_AUDIT.md).
- **Inputs:** acquired bytes/evidence, trusted limits and explicit installation compatibility/trust policy.
- **Outputs:** one validated import-eligible result retaining exact bytes/hash, existing artifact model/digest, separate evidence, and explicit policy decision; bounded original FR-022 diagnostics on rejection.
- **Components:** `CompiledRulesArtifactValidation.cs`, narrow malformed-input hardening in `CompiledRulesArtifactContract`, 58 focused portable tests using existing conformance fixtures, contract/audit updates.
- **Acceptance:** independent size/hash checks, strict UTF-8, existing parser/integrity/dependency/compatibility rejection, no provider approval bypass, explicit policy rejection, custom schemes, valid equal bytes identical semantics, invalid input produces no import-eligible value.
- **Validation:** 58/58 focused malformed/oversize/UTF-8/hash/format/trust/mutation tests; 397 portable, 62 CLI/process, and 181 Managed tests pass; FR-022/023/024/A focused reruns, canonical unchanged, dependency boundaries and full structural/repository/distribution checks pass. Exact-state reruns are recorded in the audit.
- **Exclusions/stop:** no real provider or storage transaction. Stop at one validated B checkpoint; C/D/E may then be independently authorized.

## FR-025C - Explicit Local Artifact Provider

- **Status:** complete; **depends on A/B**; [checkpoint audit](audits/FR_025C_LOCAL_PROVIDER_AUDIT.md).
- **Inputs:** explicit authorized ordinary-file locator, resource/access policy; no repository root or acquisition-state inference.
- **Outputs:** bounded exact bytes/hash and local evidence through B, without storage or activation.
- **Components:** `LocalCompiledRulesArtifactProvider.cs` in the existing portable library, internal file-open test seam, existing A/B contracts and conformance fixtures; no new project/package or dependency on Managed/SQL/MCP/Git/network.
- **Acceptance:** isolated file works without layout/Git; missing/directory/invalid paths fail; filesystem indirection/access policy enforced; actual opened bytes hash consistently; concurrent changes are detected or truthfully describe acquired bytes; oversize/cancel/failure cannot publish partial payloads.
- **Validation:** 39 focused cases, including actual symlinks/junction and controlled cancellation/mutation/open/read failures, pass with zero skips on Windows; complete portable 436, CLI/process 62 and Managed 181 tests pass. FR-022/023/024/A/B, dependency, structural/repository and deterministic distribution checks pass; exact-state evidence is in the audit.
- **Exclusions/stop:** no HTTP, archive, SQL import, acquisition CLI redesign. Stop after C; do not automatically start D.

## FR-025D - GitHub Releases Artifact Provider

- **Status:** complete; **depends on A/B**, compares C; [checkpoint audit](audits/FR_025D_GITHUB_PROVIDER_AUDIT.md).
- **Inputs:** trusted repository/release/asset selector and optional secure provider configuration; explicit stable/prerelease policy.
- **Outputs:** exactly one bounded published artifact asset plus safe requested/resolved evidence through B. No clone/source compilation.
- **Components:** `GitHubReleaseCompiledRulesArtifactProvider` in portable `EternalCycle.Rules`, standard .NET HTTP only, explicit bounded selectors/private token/deadline, deterministic HTTP fixtures; no new project/package or common A/B API change.
- **Acceptance:** reject missing/duplicate/ambiguous assets; no tag-implies-asset assumption; endpoint/redirect/status/content/credential controls, timeout/cancel and decoded bounds; identity records actual evidence plus hash; equal local/remote payloads have equal validated semantic identity.
- **Validation:** 81 focused HTTP resolution/redirect/credential/bounds/cancel/disposal/privacy/C-D-B cases; complete portable 517, CLI 62 and Managed 181 tests pass. FR-022/023/024/A-C, boundary, structural/repository and deterministic distribution checks pass. Real Release/private-auth/DNS/proxy acceptance remains unperformed; no live artifact release was created for testing.
- **Exclusions/stop:** no Git fallback, release creation/packaging, SQL, ranking. Stop after D.

## FR-025E - Custom Provider Conformance

- **Status:** complete; **depends on A/B**, compares C/D; [checkpoint audit](audits/FR_025E_CUSTOM_PROVIDER_CONFORMANCE_AUDIT.md).
- **Inputs:** documented provider contract and distinct third-party-style in-memory/custom fixture with no Managed/GitHub inheritance.
- **Outputs:** reusable provider conformance evidence demonstrating replaceability, equal validated identity and provider-failure isolation.
- **Components:** test-only `MemoryStoreCompiledRulesArtifactProvider` linked into two existing test assemblies, `CompiledRulesProviderConformanceTests`, non-friend `ExternalProviderConformanceTests`, implementer guidance and audit; no production/project/package dependency change.
- **Acceptance:** custom source/evidence schemes, equal bytes equal artifact identity, metadata cannot bypass limits/hash/FR-022/trust, cancellation/failure safe, no importer rewrite; unsupported provider claims do not rewrite artifact provenance.
- **Validation:** 34 portable plus two non-friend conformance cases pass; A-E 246 portable cases, complete portable 551 / CLI 64 / Managed 181 pass (796 unique). FR-022/023/024, dependency, structural/repository/distribution and exact staged-state checks are recorded in the audit. Canonical C/D/E matrix retains equal complete artifact identity, hashes and approved bytes.
- **Exclusions/stop:** no new production transport, importer or provider registry framework. Stop after E.

## FR-025F - Shared Import and Managed Storage Integration

- **Status:** complete; **depends on B/C/D/E**; [checkpoint audit](audits/FR_025F_LOSSLESS_IMPORT_AUDIT.md).
- **Inputs:** only B-approved artifacts, explicit authorized storage scope/policy, existing candidate/publication/activation lifecycle.
- **Outputs:** one shared import orchestration and storage receipt with scoped Ruleset/semantic identity; lossless artifact retention, duplicate handling and atomic candidate persistence independent of provider.
- **Components:** `CompiledRulesArtifactImport`, `SqlServerCompiledRulesArtifactStore`, shared approval fixtures, portable/SQL tests, additive migration 011 and setup/packaging. Rules storage remains in the existing logical Domain; no publication coordinator/runtime change.
- **Acceptance:** identical local/GitHub/custom artifacts reuse imported identity; formatting-only byte changes do not duplicate semantic imports; full provenance/metadata retained; conflicts explicit; failures before/within transaction expose no partial corpus and preserve active release; approval/namespace/campaign adoption/preparation boundaries unchanged. Do not collapse source and artifact identities or rewrite legacy release history.
- **Validation:** 18 portable input/identity/immutability/failure cases; 15 real disposable LocalDB import/migration/rollback/cancellation/concurrency cases plus one database-free packaging check. Complete portable/CLI/SQL-enabled Managed suites, FR-022/023/024 and acquisition regressions, boundary/structural/full repository checks are recorded in the audit. Existing opt-in SQL fixture expectations now reflect the already-established 154-chunk corpus and 010/011 upgrade path.
- **Exclusions/stop:** no FR-026 ranked/searchable projection, ordinary gameplay change, campaign creation or automatic incompatible adoption. Stop after F.

## FR-025G - Integrated Acceptance and Closure

- **Status:** complete; **depends on A-F**. [Closure evidence](audits/FR_025_ACQUISITION_AND_IMPORT_AUDIT.md).
- **Inputs:** providers, common validation/trust/import, fixtures and actual storage evidence; original governed FR-025 acceptance.
- **Outputs:** durable requirement matrix and security/compatibility audit; closure only after evidence supports it.
- **Components:** integrated tests, audit/governance/navigation, only narrow defects in FR-025 if found.
- **Acceptance:** local/download/custom equal imported identity, no provider bypass, separate transport/artifact provenance, bounded failure behavior, atomic/idempotent lossless import, existing rules/campaign authority intact; all original criteria mapped to concrete behavior. Real infrastructure limits reported explicitly.
- **Validation:** relevant Release builds, all portable/CLI/adapter/Managed tests, FR-022/023/024 regressions, boundary/security/conformance/import suites, all structural harnesses, full repository and exact staged-state checks after governance changes.
- **Exclusions/stop:** no FR-026/034 implementation, VERSION/tag/release/archive action. Close only FR-025, select nothing next, perform only the owner-authorized normal branch push, then stop.

## Acceptance Strategy and Classification

Equal bytes from all providers must traverse the same parser, trust gate and importer. B proves technical/trust rejection; C/D prove concrete byte acquisition; E proves another implementation needs no shared rewrite; F proves atomic/idempotent storage; G integrates them. A proves none of those deferred stages by file existence.

- **A - Reference implementation:** .NET interface/data types, 16 MiB default, pooled-buffer stream mechanics, filesystem/HTTP adapters, SQL transaction details and test tools.
- **B - Managed Service contract:** shared pre-import validation/trust, lossless scoped/idempotent/atomic storage, authorization, independent durable administrative execution and safe diagnostics.
- **C - Eternal Cycle-wide:** Repository Canon remains authority; provider choice does not redefine artifact/source identity or restrict valid custom rules; acquisition and hash integrity are not trust or gameplay readiness.

No new governed revision, scoring/query/ranking, Compiled Rule Store/search index, campaign persistence change, compiler/vocabulary change, release packaging, or hidden GitHub dependency is included. `VERSION` stays `1.0.0`; tags stay fixed; Phase 13 remains Active and Future Revisions `[∞]` last. Stop after G's validated closure commit/push, selecting nothing else.
