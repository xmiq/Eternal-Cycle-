# FR-025D GitHub Release Artifact Provider Audit

## Authority and Checkpoint

The owner authorized D only beneath the selected [FR-025 objective](../V1_1_FUTURE_REVISION_PLAN.md#fr-025---provider-neutral-artifact-acquisition-and-import). Starting `main`, HEAD and upstream were clean/synchronized at `c38585fe56d534a6d4949442b6af6ced9f7d16d0`. A-C were complete. **A-D are now complete; FR-025 remains selected/incomplete; E-G pending; FR-026 pending/unselected.** No additional objective was selected. `VERSION` stays `1.0.0`; tag references stay `v1.0.0` at `6938bd2f9e965faaeb1c612b1dd765237cadc8a8` and `v1.1.0-rc` at `4eb2583027d316a962fd49fe318781a0e060183e`.

This checkpoint adds only GitHub Release byte acquisition, tests and documentation. It includes no custom-provider acceptance, SQL/storage/import, migration, publication/activation, campaign binding, runtime retrieval/ranking, release asset naming/packaging/creation, archive or tag action. Decisions D-1363 through D-1365 already govern this boundary; no new terminology or blocking design question is required.

## Investigation and Dependency Direction

The A/B/C implementation, tests/audits, [shared acquisition contract](../../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md), FR-022 artifact authority, compiler/vocabulary closure, Managed configuration/error/setup/operations, source provider and release provenance were inspected. Repository search found no existing `HttpClient`/HTTP-handler/API Release downloader to reuse. Managed `GitRuleSourceProvider` owns source acquisition, subprocess lifecycle and minimal manifest materialization, not compiled assets. Repurposing it would introduce the wrong Git/Managed dependency and change existing behavior.

[GitHubReleaseCompiledRulesArtifactProvider](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/GitHubReleaseCompiledRulesArtifactProvider.cs) therefore uses standard .NET HTTP within the existing portable library, like C's filesystem adapter. No project, SDK, package, interface/factory layer or A/B contract change was added. Portable assembly/CLI boundaries remain passing with no SQL, MCP, hosting, Git executable, GitHub CLI or campaign dependency. Acquisition is absent from normal compiler/Managed execution paths; no runtime wiring is prematurely claimed.

### Official HTTP Evidence

- [GitHub release API](https://docs.github.com/en/rest/releases/releases) distinguishes published releases from standalone tags and supports exact tag lookup.
- [Release asset API](https://docs.github.com/en/rest/releases/assets) supports numeric asset endpoints, JSON metadata, paginated asset lists and binary Accept with direct 200 or redirect 302 delivery.
- [REST request guidance](https://docs.github.com/en/rest/using-the-rest-api/getting-started-with-the-rest-api) documents User-Agent, Accept and explicit API version; [supported API versions](https://docs.github.com/en/rest/about-the-rest-api/api-versions) confirms the pinned `2022-11-28` contract remains supported.
- [GitHub runner network guidance](https://docs.github.com/en/actions/reference/runners/self-hosted-runners) explicitly identifies `release-assets.githubusercontent.com` for release assets. D deliberately does not allow every GitHub content subdomain.
- [REST troubleshooting](https://docs.github.com/en/rest/using-the-rest-api/troubleshooting-the-rest-api) documents 403/429 rate limits and Retry-After/quota headers, and privacy-preserving 404 for inaccessible resources.

These are official contract research, **not a live D-provider asset download**. No release was created to obtain test data, and no private token was used.

## Implemented Resolution and Identity

```text
explicit owner/repository/tag/asset-name + optional prerelease/token/deadline
    -> bounded repository and exact published tag metadata
    -> exhaustive bounded exact asset-name selection
    -> numeric asset API endpoint / constrained manual redirects
    -> A exact bytes/hash + separate observed GitHub evidence
    -> caller explicitly invokes B validation/trust
```

Constructor configuration is private, bounded and fully specified; no CWD/install/repository discovery, latest/fuzzy asset search, Git checkout, compilation, archive extraction or local fallback occurs. Selector characters/limits and lifetime/ownership are documented in the [provider contract section](../../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md#github-release-asset-provider). Tag and asset-name matching are exact; case-insensitive repository/API-name comparison follows GitHub semantics. Draft/unpublished releases fail, prereleases require explicit permission, and malformed flags still fail when prereleases are allowed.

Metadata reads use A's existing bounded reader (1 MiB per response, JSON depth 16); required fields, duplicate object properties, positive IDs, asset state/size and expected API asset URL are checked. Unknown additive GitHub fields are not interpreted as instructions. Pagination is constructed locally, maximum ten 100-entry pages; Link URLs are ignored. Duplicate IDs, duplicate matching names, malformed entries and unexhausted page bounds fail closed. Selected asset size never replaces actual downloaded-byte identity. Remote enumeration is not an atomic snapshot or guarantee against concurrent release/asset replacement.

Evidence retains nine bounded requested/resolved repository/tag/name/ID fields plus provider kind `github-release` and observed `github-release-asset` identity `repositoryId/releaseId/assetId`. It retains no signed download URL, token/header/body, timestamp or remote message. Numeric IDs identify observed objects, not permanent/authentic/immutable bytes. Deletion/replacement of a release/tag/asset is not denied. A's independently computed exact-byte SHA and the validated FR-022 semantic/source/compiler identities remain separate. Expected-byte-hash acceptance belongs to B policy.

## HTTP Security, Bounds, and Ownership

- Default transport uses HTTPS, system TLS validation, no automatic redirects/decompression/cookies/default credentials, identity encoding, a 30-second connect timeout and 16 KiB header limit. Metadata is routed only to constructed `api.github.com` endpoints and must select JSON. The asset API requests binary bytes; vendor GitHub JSON is rejected as wrong representation, while ordinary/odd artifact media types leave validity to B.
- Asset redirects are manual, maximum three, with loop detection. Only the original exact HTTPS/443 API asset endpoint and HTTPS/443 `release-assets.githubusercontent.com` are permitted; relative CDN locations are revalidated. HTTP downgrade, userinfo, fragment, port, missing Location and arbitrary-host/API-path/query destinations fail before request. Metadata redirects are never followed. The narrow delivery-host allowlist may need a separately validated update if GitHub changes infrastructure; no IP pinning/DNS-rebinding guarantee is claimed.
- Optional bearer token is private explicit configuration and sent only to initial API requests, never any redirected request. No cookies/referrer or response headers are replayed. Public provider serialization/display has no configuration/credential fields. Unsafe standard injected handlers are rejected; custom handler code is trusted, caller-owned and must not add automatic redirects/decompression/credentials or unsafe logging. D cannot sandbox in-process transport code or OS DNS/proxy/TLS/external tracing behavior.
- Exact bytes stream through unchanged A bounds (default 16 MiB), independent of absent/incorrect Content-Length or metadata size; declared overbound content can fail before body read. Overbound stream consumes only limit plus one, never truncated success. Encoded responses are refused; no archive, decode, BOM stripping, newline/JSON normalization or decompression path exists.
- One linked whole-acquisition deadline covers all requests, pagination/redirects and body reads: default two minutes, explicit positive configuration up to ten minutes. Provider deadline is fixed transport failure; caller/other transport cancellation propagates with fixed safe message/token. No automatic retry or daemon is introduced. Cancellation of arbitrary injected code remains cooperative.
- Every request/response/body stream is disposed on success/status/metadata/redirect/size/read-failure/cancellation paths. The provider owns its client/default handler; injected transport is caller-owned. Reuse/dispose follows ordinary .NET client lifetime rather than creating a client per GET.

## Structured Failure and Diagnostic Policy

Existing A categories/codes are unchanged: missing/unsafe configuration is `ProviderConfigurationInvalid`; invalid selectors/ambiguous or rejected redirects `LocatorInvalid`; 404/401/ordinary 403/missing or disallowed release/asset `Unavailable`; 429 or explicit 403 rate headers, other status, malformed/overbound metadata, encoding/negotiation/network/read/deadline failure `TransportFailed`; oversized file `PayloadTooLarge`. Cancellation remains `OperationCanceledException`. Unknown 403 causes are not inferred from arbitrary bodies. Explicit rate-limit category differs from not-found; retry headers are not echoed/retained, and callers decide recovery.

Only fixed bounded A diagnostics are emitted, with no raw exception/inner exception, API JSON, error HTML, authorization/cookies or signed query URLs. Acquisition evidence remains authorized internal data rather than automatically public diagnostics; private repository names may still require caller disclosure policy. D does not create an error-dump endpoint or change Managed error semantics.

## Executed Acceptance and Compatibility

[81 focused cases](../../examples/tooling/rules-compiler-dotnet/tests/EternalCycle.Rules.Tests/GitHubReleaseCompiledRulesArtifactProviderTests.cs) exercise deterministic HTTP handlers and nonseekable instrumented streams; no live network, Git/SQL/MCP or campaign configuration is required. They prove exact published selection/pagination/ambiguity, safe status/rate/metadata failures, bounded stream and declared sizes, allowed/unsafe/relative redirects, credential stripping, private diagnostics/evidence, whole-body timeout/cancellation, disposal and byte preservation. Initial test compilation caught mock request-type and non-record DTO usage; those test errors were corrected before test execution. Review also added handler/metadata-case/casing and malformed-prerelease regressions; no established artifact contract conflict was found.

Valid bytes pass A then B with explicit approval; malformed/empty/non-UTF-8 bytes acquire but fail only in B; valid assets may fail B trust. Same bytes through C and D yield equal byte hash, validity, semantic digest, complete artifact/provenance and import-eligible bytes. A test-only evidence-sensitive B policy can distinguish them without provider trust logic. Same requested tag resolving a replacement asset/content produces changed observed ID/hash/semantic identity, never tag-implied immutability.

Canonical acquisition/B acceptance stays **10 sources / 154 snippets / 275,622 bytes**:

- semantic artifact digest: `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`;
- exact byte SHA-256: `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`.

| Executed validation | Result |
| --- | --- |
| Portable and CLI Release builds | Pass, zero warnings/errors. |
| A/B/C/D focused acquisition/gate/provider tests | 212/212: 34 / 58 / 39 / 81. |
| Complete portable suite | 517/517. |
| Complete CLI/process suite | 62/62. |
| Complete Managed suite | 181/181. |
| FR-023 loader/snippet/writer regressions | 70/70. |
| FR-024 A-F input/enrichment/pipeline/audit/curation/acceptance | 235/235. |
| FR-022 conformance / portable dependency boundary | 17/17; CLI boundary included in full CLI suite. |
| Nine structural harnesses | 333/333: FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 operations 81, FR-021 repair 35, FR-022 32, campaign-mode 17. |
| Full repository / deterministic distribution validation | Pass: 303 Markdown files, 7,403 relative links, 209 anchors; 191 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. Zero blocking questions, orphan documents or forbidden campaign-data directories. |

Complete suites total **760 unique tests**, zero failures/skips; focused reruns overlap. Existing Release build/test project commands use `--no-restore` with dependencies available, not substitute validation. The full repository command is `pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\validate_repository.ps1`. Final documentation is followed by complete diff review, explicit staging, staged whitespace, focused D (81/81) and repository reruns (counts above); exact content must pass before commit/push. No generated build/cache/temp/log/output, campaign data, credential, private locator or release artifact is staged. Git's existing LF-to-CRLF conversion notices are not whitespace failures.

## Classification and Remaining Acceptance

- **A - Reference implementation:** .NET HTTP adapter/configuration, exact tag/name selectors, numeric observation, host/header/redirect/resource limits, injected test handlers and fixed safe mappings.
- **B - Managed Service contract:** providers return bounded exact bytes/evidence to common validation/trust; acquisition cannot grant storage/publication authority or override artifact provenance; transport implementation remains replaceable.
- **C - Eternal Cycle-wide:** GitHub is one peer provider, not distribution authority; local/manual/custom freedom and Repository Canon remain intact; integrity/location/IDs do not establish trust or gameplay readiness.

No live published compiled-artifact/private-repository acquisition, actual redirect/CDN/TLS/proxy behavior, OS network failures or production trust configuration was exercised. Deterministic tests are D acceptance, not deployment acceptance. E custom conformance, F lossless atomic/idempotent SQL import and G integration remain pending; F's known source-unique storage/metadata-retention gaps were deliberately not repaired here. Stop after one focused D commit and normal owner-requested `main` push; VERSION, tags and immutable release history stay unchanged.
