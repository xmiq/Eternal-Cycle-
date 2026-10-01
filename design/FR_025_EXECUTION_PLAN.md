# FR-025 Acquisition, Validation, and Import Execution Plan

## Authority and Current Checkpoint

The owner selected [FR-025](V1_1_FUTURE_REVISION_PLAN.md#fr-025---provider-neutral-artifact-acquisition-and-import) from clean synchronized `main` at `a34c268d239bc9b195200e2198bb899a60ad380b`, after FR-022/023/024 closure. A-G are execution checkpoints beneath that single Future Revision, not additional objectives or Project Phases. FR-025 remains selected and incomplete; FR-026 and every other objective remain unselected.

**A complete:** investigation, architecture reconciliation, [acquisition/import contract](../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md), portable provider/evidence/bounded-byte foundation, and contract regressions. B-G are pending and require explicit continuation. No real acquisition provider, shared trust validator, artifact importer, runtime retrieval change, or migration was implemented by A.

## Existing Architecture Map

| Responsibility | Current owner / implementation | Actual boundary |
| --- | --- | --- |
| Rule Source acquisition | Managed `IRuleSourceProvider`, `GitRuleSourceProvider`, persisted `RuleSourceConfiguration` | Acquires canonical source material and resolves Git provenance; not compiled artifacts. |
| Local filesystem input | FR-023 `MaterializedRuleSourceLoader`; Managed Git provider's local-repository path | Ordinary manifest-declared sources versus local committed Git inputs; neither is an artifact-file provider. |
| Git/GitHub input | Git source runner and provider | Temporary object acquisition, immutable commit, minimal ordinary manifest payload; not GitHub Releases asset download. |
| Compiled artifact acquisition | No production provider before A | A adds the portable interface/bounds only; real providers remain pending. |
| MCP/Managed interface | `ec_configure_rule_source`, `ec_publish_initial_rules`, operation status/readiness, `ec_get_rule_context` | Administrative source publication plus independent durable worker; runtime context reads already-published rules. No format-1 import endpoint. |
| Artifact parsing / validation | Portable `CompiledRulesArtifactContract.Read` / `Validate` | Closed JSON, compatibility, identity, ordering, dependencies, content/semantic integrity; no provider or installation trust policy. |
| Compilation / writing / audit | FR-023/024 portable pipeline and CLI | Materialized bytes to format 1; reviewed enrichment and observational audit; no acquisition/import. |
| Trust checks | Manifest compatibility, approval, trusted namespace/configuration, channel/activation policy | Existing source/publication controls, not one provider-neutral artifact acceptance policy. |
| Import | No format-1 storage importer | Existing candidate staging accepts a legacy `CompiledRuleIndex`, not artifact bytes/models. |
| Publication | `ManagedRulePublicationCoordinator`, `IPublishedRuleStore` | Source acquisition, legacy compilation, candidate stage/validate/publish, configured activation, progressive preparation. |
| Runtime storage | `SqlServerPublishedRuleStore`, migrations 002/006/007 | Shared Domain release/index/chunk/selector/dependency/preparation records; campaign facts live elsewhere. |
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

Missing behavior is real bounded artifact acquisition, shared strict byte-to-artifact validation/trust, custom-provider conformance, and lossless idempotent artifact storage. Git source acquisition is retained, not repurposed into a release-asset importer. No established FR-022/023/024 contract conflict was found.

The legacy physical key `(ruleset_id, source_identity)` and `CompiledRuleIndex` omit artifact semantic digest identity and full retrieval metadata. F must preserve those historical releases, add only genuinely necessary lossless adaptation, and reject unresolved conflicts rather than overwrite. The complete format-1 artifact must remain retained for FR-026; this does not authorize implementing searchable/ranked storage now.

## Dependency Direction and Trust

The provider interface, evidence, bounds, and shared validation belong in portable `EternalCycle.Rules`; they require only the established standard-library dependencies. Configured provider implementations may depend on filesystem/HTTP APIs in an appropriate adapter component. Managed may consume the portable result and retain storage-specific routing/transactions. Portable code must not depend on Managed, SQL, MCP, hosting, Git/GitHub SDKs, or campaign state.

A's owned byte hash is independent of provider metadata. B must recompute at its boundary and invoke the existing parser/validator rather than trust a provider's assertion or invent another format. Technical validity is not publisher authentication or installation approval. Explicit caller policy, not official naming/provider choice, determines acceptable Ruleset/source/compiler/scope. Valid custom/fork/house-rule artifacts need not match official semantic content. No signing/PKI infrastructure is required.

Provider locators stay in authorized configuration. A path/tag is mutable; byte hash identifies acquired bytes. Optional resolved identity is a bounded provider claim, not the artifact source identity. GitHub asset/release IDs do not prove immutable bytes. Reference exceptions use fixed safe messages; provider evidence requires field-level review/redaction before diagnostics. The [contract](../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md#failure-taxonomy) defines all failure categories, with later-stage execution deferred.

## Security Review

| Risk | A control / required later evidence |
| --- | --- |
| Unlimited payload or misleading length | A checks buffered payload before copying; shared stream reader stops at limit plus one, no seeking; default 16 MiB. B independently enforces bounds. |
| Traversal, links/reparse points, unintended local files, TOCTOU | No file traversal in A. C uses explicit ordinary-file policy, checks indirection and open-handle consistency, streams/hash actual bytes, tests races and path cases. FR-023 materialized-root logic is relevant precedent, not blindly transplanted. |
| Decompression bombs | No archives in this architecture. D must bound any accepted HTTP decompression; never trust compressed Content-Length as decoded size. |
| Redirects/content type/provider filenames | No network/output paths in A. D validates endpoint/redirect/credential and asset policy; filenames never select local output locations. |
| Ambiguous assets / mutable refs | D resolves exactly one published asset, rejects ambiguity, records actual resolution and hash; tag names never claim immutability. |
| Diagnostic leakage | A metadata/count/length/control limits, safe object display and fixed exceptions; no raw inner transport exception retained. Bounds do not replace provider redaction or authorized disclosure policy. |
| Partial writes / activation | A writes nothing. F tests atomic candidate storage and rollback/preservation; publication/activation remain separately authorized. |
| Same source but different artifact digest | No physical key change in A. F must detect/reconcile explicitly while preserving history and complete metadata; no silent upsert overwrite. |

## FR-025A - Investigation and Portable Acquisition Foundation

- **Status:** complete; [checkpoint audit](audits/FR_025A_ACQUISITION_ARCHITECTURE_AUDIT.md).
- **Inputs:** FR-022/023/024, Managed source/publication code, setup/operations/configuration, existing tests, release/distribution rules.
- **Outputs:** architecture map, contract, bounded provider/evidence/byte API, fixed failure taxonomy, contract tests, this plan, selected-objective governance.
- **Components:** `CompiledRulesAcquisition.cs`, `CompiledRulesAcquisitionTests`, documentation/navigation and governance validator expectations.
- **Acceptance/validation:** exact copied bytes/hash, arbitrary unvalidated input, distinct immutable-evidence claim, bounded metadata/payload, cancellation, fixed failures, fake custom provider, canonical unchanged; Release builds, complete portable/CLI/Managed compatibility, structural/full repository checks.
- **Exclusions/stop:** no parser/trust/import implementation, real provider, SQL change or downstream selection. Stop after focused validated commit and owner-requested normal `main` push.

## FR-025B - Shared Validation and Trust Gate

- **Status:** pending; **depends on A**.
- **Inputs:** acquired bytes/evidence, trusted limits and explicit installation compatibility/trust policy.
- **Outputs:** one validated import-eligible result retaining exact bytes/hash, existing artifact model/digest, separate evidence, and explicit policy decision; bounded original FR-022 diagnostics on rejection.
- **Components:** portable acquisition validation alongside `CompiledRulesArtifactContract`, portable tests/conformance fixtures, contract/audit updates.
- **Acceptance:** independent size/hash checks, strict UTF-8, existing parser/integrity/dependency/compatibility rejection, no provider approval bypass, explicit policy rejection, custom schemes, valid equal bytes identical semantics, invalid input produces no import-eligible value.
- **Validation:** focused malformed/oversize/UTF-8/hash/format/trust/mutation tests, FR-022, canonical compilation, complete portable suite/boundary, repository/whitespace checks.
- **Exclusions/stop:** no real provider or storage transaction. Stop at one validated B checkpoint; C/D/E may then be independently authorized.

## FR-025C - Explicit Local Artifact Provider

- **Status:** pending; **depends on A/B**.
- **Inputs:** explicit authorized ordinary-file locator, resource/access policy; no repository root or acquisition-state inference.
- **Outputs:** bounded exact bytes/hash and local evidence through B, without storage or activation.
- **Components:** a provider adapter using the A interface, local safety tests and small format-1 fixtures; project placement must preserve portable core independence.
- **Acceptance:** isolated file works without layout/Git; missing/directory/invalid paths fail; filesystem indirection/access policy enforced; actual opened bytes hash consistently; concurrent changes are detected or truthfully describe acquired bytes; oversize/cancel/failure cannot publish partial payloads.
- **Validation:** temporary-file/symlink/reparse and deterministic race substitutes, same bytes equal B identity, focused adapter plus portable/boundary/full repository checks.
- **Exclusions/stop:** no HTTP, archive, SQL import, acquisition CLI redesign. Stop after C; do not automatically start D.

## FR-025D - GitHub Releases Artifact Provider

- **Status:** pending; **depends on A/B**, uses C fixtures when available.
- **Inputs:** trusted repository/release/asset selector and optional secure provider configuration; explicit stable/prerelease policy.
- **Outputs:** exactly one bounded published artifact asset plus safe requested/resolved evidence through B. No clone/source compilation.
- **Components:** HTTP provider adapter, deterministic fake HTTP handlers/asset fixtures, configuration documentation; no provider package dependency in core.
- **Acceptance:** reject missing/duplicate/ambiguous assets; no tag-implies-asset assumption; endpoint/redirect/status/content/credential controls, timeout/cancel and decoded bounds; identity records actual evidence plus hash; equal local/remote payloads have equal validated semantic identity.
- **Validation:** deterministic HTTP success/failure/redirect/auth/oversize cases; provider/local equality and portable/boundary/full repository checks. Real Release/auth acceptance reported separately when available, never invented.
- **Exclusions/stop:** no Git fallback, release creation/packaging, SQL, ranking. Stop after D.

## FR-025E - Custom Provider Conformance

- **Status:** pending; **depends on A/B**, compares C/D if present.
- **Inputs:** documented provider contract and distinct third-party-style in-memory/custom fixture with no Managed/GitHub inheritance.
- **Outputs:** reusable provider conformance evidence demonstrating replaceability, equal validated identity and provider-failure isolation.
- **Components:** conformance fixtures/tests, implementer guide, plan/audit evidence.
- **Acceptance:** custom source/evidence schemes, equal bytes equal artifact identity, metadata cannot bypass limits/hash/FR-022/trust, cancellation/failure safe, no importer rewrite; unsupported provider claims do not rewrite artifact provenance.
- **Validation:** focused conformance matrix, complete portable/adapter suites, dependency/repository/whitespace checks.
- **Exclusions/stop:** no new production transport, importer or provider registry framework. Stop after E.

## FR-025F - Shared Import and Managed Storage Integration

- **Status:** pending; **depends on B/C/D/E**.
- **Inputs:** only B-approved artifacts, explicit authorized storage scope/policy, existing candidate/publication/activation lifecycle.
- **Outputs:** one shared import orchestration and storage receipt with scoped Ruleset/semantic identity; lossless artifact retention, duplicate handling and atomic candidate persistence independent of provider.
- **Components:** portable importer/storage contract where justified, Managed adapter/coordinator/store/configuration/tests, and only genuinely required additive migrations after inspection. Keep rules storage in the existing logical Domain.
- **Acceptance:** identical local/GitHub/custom artifacts reuse imported identity; formatting-only byte changes do not duplicate semantic imports; full provenance/metadata retained; conflicts explicit; failures before/within transaction expose no partial corpus and preserve active release; approval/namespace/campaign adoption/preparation boundaries unchanged. Do not collapse source and artifact identities or rewrite legacy release history.
- **Validation:** deterministic memory-store rollback/idempotency/concurrency/failure tests, complete Managed suite; real SQL transaction/read-back tests when configured, otherwise explicitly unproven. Any migration must be repeat-safe, preserve historical rows and have compatibility regression. Portable/adapter/boundary/full repository checks.
- **Exclusions/stop:** no FR-026 ranked/searchable projection, ordinary gameplay change, campaign creation or automatic incompatible adoption. Stop after F.

## FR-025G - Integrated Acceptance and Closure

- **Status:** pending; **depends on A-F**.
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

No new governed revision, scoring/query/ranking, Compiled Rule Store/search index, campaign persistence change, compiler/vocabulary change, release packaging, or hidden GitHub dependency is included. `VERSION` stays `1.0.0`; tags stay fixed; Phase 13 remains Active and Future Revisions `[∞]` last. After A's commit/push, wait for explicit B authorization.
