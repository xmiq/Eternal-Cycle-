# FR-025 Acquisition and Import Closure Audit

## Authority and Checkpoint

Owner-authorized FR-025G continues clean synchronized `main` at `caedf05a99a7fe75c6817a9670acca922abdf3d9`. This is integrated acceptance beneath FR-025, not a new objective. VERSION remains `1.0.0`; immutable release and moving RC tags are outside this task. FR-026 is unselected.

## Requirement Matrix Before G Tests

This matrix was reconstructed before adding G tests from the [FR-025 objective](../V1_1_FUTURE_REVISION_PLAN.md#fr-025---provider-neutral-artifact-acquisition-and-import), [shared contract](../../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md), [execution plan](../FR_025_EXECUTION_PLAN.md), and A-F audits. No blocked/conflicting normative requirement was found. Paths below name actual implementation/test files, not new authority.

Portable files live in `examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules`; SQL files/tests live in `examples/tooling/managed-data/mcp-dotnet-tsql`. A-F evidence is retained in their linked package audits.

| Requirement | Owner / implementation | Existing evidence and initial classification | G evidence required |
| --- | --- | --- | --- |
| Provider-neutral byte acquisition and explicit composition | A; CompiledRulesAcquisition.cs | A contract tests; proven | Compose C/D/E without a provider switch in B/F. |
| Explicit local ordinary-file acquisition, containment and link safety | C; LocalCompiledRulesArtifactProvider.cs | C focused tests; proven | Canonical local file through B/F. |
| Exact GitHub Release asset and safe HTTPS/auth/redirect behavior | D; GitHubReleaseCompiledRulesArtifactProvider.cs | D deterministic HTTP tests; proven | Simulated Release through B/F; live delivery remains unproven. |
| Replaceable custom provider and source freedom | E; shared MemoryStoreCompiledRulesArtifactProvider.cs | E public-only/non-friend conformance; proven | Custom bytes through unchanged B/F. |
| Bounded exact bytes, independent SHA and acquisition identity | A/B; acquisition and validation | A bounds/read/copy tests and B hash checks; proven | Exact bytes/hash across A, B, F and readback. |
| Separate bounded untrusted acquisition evidence | A/B; evidence model and explicit policy | A/B/E opacity tests; proven | Evidence-insensitive convergence and evidence-sensitive rejection. |
| Structured bounded failures and disclosure safety | A-F; fixed codes/messages | Existing parser/provider/policy/SQL privacy cases; proven | Integrated malformed/policy/storage failures do not disclose private input. |
| Acquisition cancellation and provider isolation | A/C/D/E | Focused cancellation/failure tests; proven | Mid-import cancellation after B approval. |
| Existing FR-022 validity gate and compatibility | B; CompiledRulesArtifactValidation.cs | B and FR-022 contract tests; proven | Malformed C/D/E acquisition stops before F. |
| Explicit trust; validity is not approval | B; ICompiledRulesArtifactTrustPolicy | B approval/rejection/failure cases; proven | Valid trust-rejected artifact cannot invoke F or change SQL. |
| Immutable approved semantic graph and exact bytes | B; ValidatedTrustedCompiledRulesArtifact | B mutation tests and F trusted-only surface; proven | Provider/input disappearance cannot affect approved import. |
| Authorized Ruleset scope, no alternate raw import | F; CompiledRulesArtifactImport.cs / configured SQL store | F scope/API tests; proven | Full-chain unauthorized scope leaves SQL unchanged. |
| Lossless normative format-1 projection | F; SqlServerCompiledRulesArtifactStore.cs | Full-field representative/canonical F round trips; proven | Full-chain canonical semantic and exact-byte equality. |
| Complete retrieval kind/concept/wording/weight/ownership/order | F; snippet JSON with explicit ordinal | F rich terms/shared kinds/relationships/empty metadata cases; proven | Canonical 773 terms counted in SQL and full readback equality. |
| Artifact-local source/snippet/dependency history ownership | F; migration 011 | F repeated source IDs, ordinal/case/FK cases; proven | Changed source/content through provider/B/F retains both histories. |
| Exact first-approved byte retention separate from semantics | F; exact_bytes / byte_sha256 | F formatting-only reimport and readback corruption tests; proven | Canonical byte SHA retained; retries converge. |
| Artifact-based deterministic identity and idempotency | F; Ruleset + semantic digest key | F sequential/concurrent/key safety tests; proven | Same-provider and cross-provider end-to-end retries. |
| Integrity conflict cannot overwrite history | F; complete VerifyReadback comparison | F corruption/conflicting projection tests; proven | Retain existing regression; no new duplicate corruption test needed. |
| Atomicity and cancellation rollback | F; serializable transaction | F five injected stages and cancellation; proven | Trusted full-chain input fails/cancels after snippets; prior state intact. |
| Publication/activation remain separately authorized | F and existing Managed publication | F active-pointer and legacy flow tests; requires G integration evidence | Unchanged SQL history and existing public current-rule context. |
| Pre-011 upgrade, repeat safety and existing state | F; additive 011 / bootstrap planner | F real LocalDB upgrade and packaged template tests; proven | Representative history upgrade followed by full-chain import. |
| Cross-provider convergence without evidence identity leakage | C/D/E/B/F | E byte equivalence and F canonical C/D/E; requires G integration evidence | Explicit retry, exact child/term/dependency counts and rejected-provider isolation. |
| Offline precompiled import, provider unavailable after approval | B/F input boundary | Architectural boundary exists; requires G integration evidence | Delete local provider payload after approval, then SQL import/readback. |
| Compiler, vocabulary, legacy publication/campaign compatibility | FR-022/023/024 and F | Prior regression evidence; requires G final validation | Complete portable/CLI/SQL-enabled Managed and repository suites. |
| Portable dependency direction, no discovery or runtime/release scope | A-F library/project boundaries | Boundary and structural tests; proven | Rerun boundaries; inspect G production-free diff. |

## Final Architecture and Scope

```text
Local C ----+
GitHub D ---+--> A exact bounded bytes --> B FR-022 validity --> explicit trust
Custom E ---+                                                 |
                                                    immutable approved input
                                                              |
                                                    F authorized transaction
                                                              |
                                                    durable imported candidate
                                                              |
                                           separate publication / activation
```

Providers and compiler tooling consume portable `EternalCycle.Rules`. The configured SQL importer consumes its approved wrapper; the portable library has no dependency back into SQL, MCP, hosting, campaigns or Managed persistence. No provider registry, fallback ordering or arbitrary-URL downloader was added. GitHub is an explicit Release adapter, not privileged artifact authority. Existing source compilation/publication remains independent and unchanged.

G changes only integration tests, documentation/governance and status assertions in existing validators. The F SQL fixture is made partial so G reuses its disposable pre-011 database and hooks. No production correction or new migration was needed. An initial G test-only snapshot cast failed on SQL NULL for empty tables; `COALESCE` now records `[]`. Failed initial runs are not counted as passed evidence. Sandbox-blocked NuGet restore was rerun with authorized network access; all Release builds then succeeded.

## Final Acceptance Matrix

Every initial matrix requirement is accepted below; no normative requirement is blocked, orphaned or partially accepted. The test owners are the existing portable acquisition/validation/local/GitHub/conformance/import test classes (A-F), [G integration cases](../../examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/CompiledRulesAcquisitionImportAcceptanceTests.cs), and [F SQL cases](../../examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/CompiledRulesArtifactSqlImportTests.cs). Implementation locations remain those in the initial matrix.

| Requirement(s) | Owner | Executed deterministic evidence | Result / boundary |
| --- | --- | --- | --- |
| Neutral acquisition / explicit composition | A/E/G | Public-only custom provider and G canonical C/D/E use identical B/F. | Pass; no discovery or provider-specific importer. |
| Local ordinary files / path safety | C/G | 39 C cases; G canonical file, malformed file and disappearance. | Pass; C's documented transient-filesystem-race limitations remain. |
| GitHub Release / safe transport | D/G | 81 D cases; simulated Release into real SQL in G. | Pass; not live GitHub/private-auth/CDN acceptance. |
| Custom replaceability / source freedom | E/G | 34 portable conformance cases plus external non-friend tests; G custom import. | Pass; not certification of arbitrary external code. |
| Bounded exact bytes / hash / acquisition identity | A/B/G | A bounds/stream/copy; B independent hash; G canonical A/B/F/readback equality. | Pass; transport identity does not replace source provenance. |
| Separate acquisition evidence / policy sensitivity | B/E/G | IntegratedEvidenceSensitiveRejectionDoesNotChangeArtifactOrOtherProviderImports. | Pass; rejected provider has zero F calls/rows; other two converge. |
| Structured bounded safe failures | A-F/G | IntegratedMalformedAcquisitionNeverReachesPolicyOrImporter (C/D/E); IntegratedPrivateEvidenceAndPolicyExceptionRemainOutsideDiagnostics; integrated injected storage failure. | Pass; paths/private URLs/tokens/raw payload/exception detail absent from normal diagnostics. |
| Provider failures and acquisition cancellation | A/C/D/E | Existing missing/transport/bounds/cancellation tests and E error matrix. | Pass; failure cannot authorize B/F. |
| FR-022 validity/compatibility gate | B/G | FR-022 16 conformance cases and B 58 cases; G malformed acquired bytes never reach policy/F. | Pass; no validator weakening. |
| Explicit trust / validity versus approval | B/G | Evidence rejection remains valid but ineligible; policy failure is distinct; no receipt/write. | Pass; no default trust. |
| Immutable approved bytes/model | B/F/G | B mutation/collection freezing; IntegratedOfflineApprovedArtifactSurvivesSourceAndProviderDisappearance. | Pass; F needs no provider or compiler input. |
| Exact authorized Ruleset scope | F/G | IntegratedUnauthorizedScopeCannotReachStorage; independent SQL scope check. | Pass; zero F calls at orchestration and no SQL rows. |
| Complete normative projection / provenance / applicability | F/G | Ordered full-model equality in canonical G readback and F rich representative case. | Pass; no recompile or provider-derived fields. |
| Retrieval terms / kinds/concepts / weights / ownership / ordering | F/G | G canonical SQL count 773; full ordered equality; F shared wording/distinct kinds/weights/relationships/empty metadata. | Pass; no search/ranking. |
| Artifact-local source/snippet/dependency history | F/G | IntegratedChangedSourceHistoryRemainsArtifactOwned plus F case/FK/order tests. | Pass; repeated source IDs and changed hashes coexist. |
| First-approved exact bytes / separate semantic identity | F/G | G exact round trip; F formatting-only reimport retains first bytes/hash. | Pass; acquired-byte variants are not falsely reported retained. |
| Deterministic identity / idempotency / concurrency | F/G | G local twice then GitHub/custom reuses receipt; F sequential/concurrent imports. | Pass; one artifact, no duplicate children. |
| Digest-key conflict integrity | F | ProjectionCorruptionFailsReadbackAndIdempotentImportWithoutOverwrite; full VerifyReadback. | Pass; no blind key match or historical overwrite. |
| Atomicity / cancellation | F/G | Five F injected stages; G failure/cancellation after snippets from approved provider input. | Pass; rollback preserves old import and active/history state. |
| Publication and activation separation / current public behavior | F/G | IntegratedCanonicalProvidersRetryAfterUpgradeWithoutPublishingOrChangingCurrentContext compares all seven legacy tables and existing PublishedRuleContextProvider response. | Pass; stored import is not publication acknowledgement or gameplay readiness. |
| Migration/history preservation | F/G | G bootstrap plans/applies only 011, repeats it, preserves two legacy releases/current context; F legacy stage/publish/activate still works afterward. | Pass; additive forward upgrade, no promised down migration. |
| Cross-provider convergence and retry | C/D/E/B/F/G | Same canonical bytes, three distinct evidence kinds, four full imports including repeat local. | Pass; same ID/model/bytes; one header, ten sources, 154 snippets, 773 terms. |
| Offline precompiled input / vanished provider | B/F/G | Source fixture disposed before acquisition; local artifact removed after B; provider reacquisition fails while approved import/readback succeeds. | Pass; no original files, Git or provider required by F. |
| Historical compiler/vocabulary/Managed/campaign compatibility | FR-022/023/024/F/G | Complete portable/CLI/SQL-enabled Managed suites, unchanged canonical hashes and all structural/regression harnesses. | Pass; no campaign schema/data or source compiler change. |
| Dependency and scope boundaries | A-G | Portable and CLI dependency checks, reviewed production-free G diff, repository boundary checks. | Pass; no FR-026/034 or release action. |

## Canonical and Cross-Provider Evidence

| Metric | Result |
| --- | --- |
| Rule Sources / snippets | 10 / 154 |
| Retrieval terms in format-1 / counted from SQL snippet JSON | 773 / 773 |
| Retained exact bytes | 275,622 |
| Semantic artifact SHA-256 | `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B` |
| Serialized-byte SHA-256 | `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6` |

Local ordinary file, deterministic GitHub Release transport and the public-only memory-store conformance provider produce the same semantic model/provenance and exact bytes. Four approved imports (local twice, GitHub, custom) return one created and three reused compact receipts. SQL contains one artifact header, ten owned sources, 154 owned snippets and the exact artifact-declared dependency edge count. Retrieval terms are retained in those snippet JSON rows, not duplicated as a second term table. Full ordered model equality covers every normative field and all 773 terms, not merely counts or digest keys.

The evidence-sensitive policy rejects custom acquisition before F while approving identical local/GitHub bytes. Evidence can inform trust without mutating artifact provenance or durable identity. Malformed C/D/E bytes never reach trust policy or F; explicit rejection/failure cannot produce an approved wrapper. Wrong scope cannot reach the store, which also enforces scope independently. Receipt/evidence serialization remains bounded and does not expose raw private inputs.

Changed source hashes/content remain owned by their respective import IDs; prior exact bytes/model and active legacy state remain unchanged. Injected failure and cancellation after snippet/retrieval insertion expose no partial artifact/children and preserve the prior import. Existing F conflict, concurrent idempotency, dependency/applicability, case-sensitive identity, FK and richer retrieval tests remain authoritative rather than duplicated in G.

## Storage, Migration and Compatibility

F uses the existing logical Domain with additive migration 011's four artifact-owned tables. Header/source/snippet normative JSON plus ordered dependency edges are a lossless semantic projection; exact first-approved bytes/hash provide independent audit/reverification. Import ID deterministically frames authorized Ruleset and semantic digest; acquisition evidence and observation timestamps cannot change identity. Complete readback validates FR-022, compares the retained bytes' semantics against the projection, and gates commit/reuse. Serializable key-range locking plus durable uniqueness protects retries/concurrency.

G executes real LocalDB upgrade from the supported pre-011 fixture, with active and historical releases, old chunks/selectors/dependencies and update state snapshotted in deterministic key order. Applying/repeating 011 and importing canonical data do not change those snapshots or the existing public current-context result. Historical migrations are untouched. Existing campaign and publication tests pass. No reset, campaign-specific migration, new campaign, publication/activation shortcut, new import endpoint or readiness prerequisite was introduced. Verified backup/controlled restore remains the rollback policy; no automatic downgrade is claimed.

## Executed Validation

| Validation | Result |
| --- | --- |
| Portable / standalone CLI / Managed Release restore-build | Pass; zero build warnings/errors. |
| G focused real LocalDB | 11/11, zero skips. |
| A-F portable focused | 264/264. |
| Complete portable / CLI-process / SQL-enabled Managed | 569/569 / 64/64 / 208/208; **841 unique tests**, zero failures/skips. |
| FR-023 loader/snippet/writer regressions | 70/70. |
| FR-024 input/enrichment/pipeline/audit/canonical/acceptance | 235/235. |
| FR-022 conformance and portable boundary / CLI boundary | 17/17 / 1/1. |
| Reference publish packaging | Pass; 011 default/template, distribution metadata, Apache LICENSE and NOTICE present; license/notice bytes match source. Ignored build output only. |
| SQL import/migration/legacy publication focused | 61/61, including G and F, opt-in SQL enabled. |
| Nine structural harnesses | 333/333: 13, 29, 19, 48, 59, 81, 35, 32 and 17. |
| Full repository validator | Pass: 306 Markdown files, 7,452 relative links, 215 anchors; 191 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. Zero blocking questions, orphaned Markdown or forbidden campaign-data directories. |
| Whitespace / scope / distribution | Pass; deterministic temporary review ZIP content/bytes checked, no retained archive. Exact staged G/SQL/repository reruns are required before commit. |

Commands use the existing Release test projects. SQL evidence sets `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1`; fixtures create/drop only generated disposable LocalDB databases. Focused reruns are overlapping evidence, not extra unique tests. Repository validation uses process-local `-ExecutionPolicy Bypass` for the environment's unsigned-script restriction, without changing machine policy. Its deterministic distribution regression creates two temporary review ZIPs, checks identical bytes/content boundaries and removes them; this is not FR-034 packaging or a retained/published release archive.

## Classification and Operational Limits

- **A - Reference implementation:** .NET adapters, test fixtures, SQL/JSON ordinals and range locking, LocalDB, migration/publish packaging and validator status expectations.
- **B - Managed Service contract:** one valid/trusted/authorized immutable import path, losslessness, atomicity/idempotency, durable history/diagnostics and independent publication/activation.
- **C - Eternal Cycle-wide:** transport choice cannot redefine Canon or artifact authority; acquisition/hash consistency is not trust/readiness; import cannot silently rewrite history.

Live GitHub Release/private-auth/CDN delivery, arbitrary third-party provider correctness, remote/cloud SQL deployment/authentication, network-loss behavior and production-scale performance remain unproven. Existing C transient-race/coherent-snapshot limits remain documented. These are operational limits, not unimplemented normative FR-025 criteria. Real LocalDB results do not certify remote deployment or AI-host acceptance. No unresolved FR-025 architectural question remains.

## FR-026 Handoff and Stop

FR-026 may rely on durable trusted format-1 candidates, complete snippet content, applicability, source dependencies, 773 canonical retrieval terms/kinds/concept identities/weights, deterministic ordinals, preserved artifact/source/snippet identity, independent histories and exact-byte readback. Active publication remains independently controlled.

FR-026 still owns the Rule Store abstraction, runtime query semantics, ranked retrieval, dependency expansion, compact packets, performance and retrieval acceptance. None is implemented here. FR-025 A-G is complete/Closed; Phase 13 remains Active with Future Revisions `[∞]` last. FR-026 remains pending/unselected; no next objective is selected. No VERSION/tag, compiler/vocabulary, campaign, release/retained-archive or FR-034 change. Stop after the validated focused commit and authorized normal `main` push.
