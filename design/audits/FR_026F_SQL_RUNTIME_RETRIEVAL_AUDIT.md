# FR-026F Authorized SQL Runtime Retrieval Audit

## Authority and Checkpoint

Owner-authorized F only beneath [FR-026](../FR_026_EXECUTION_PLAN.md), from clean synchronized `main` at `2d8447c36f262eaa2285f6d6618deb8ab3014c80`. FR-025 is Closed; FR-026 remains selected/incomplete, A-F complete and G-H pending; FR-027 remains unselected. VERSION stays `1.0.0`. Immutable `v1.0.0` tag object remains `6938bd2f9e965faaeb1c612b1dd765237cadc8a8` (peeled commit `07abaa74a5cbe5ee121c46ff12e64bf7fb95d29e`); moving RC stays `4eb2583027d316a962fd49fe318781a0e060183e`. No tag or release action is part of F.

## Runtime Before F

The legacy `PublishedRuleContextProvider` resolves a Campaign route and pinned/active legacy Rule Release, applies configured mode/modules, uses the legacy compiler selector/closure and checks per-source progressive preparation. Missing preparation returns Pending and raises preparation priority. `ec_get_rule_context`, `RulePacketFormatter`, legacy publication/activation and readiness retain that path. Its legacy scoring is not C's reviewed-vocabulary score.

FR-025 import is separately authorized and atomic. Migration 011 stores lossless immutable artifact histories, but neither its successful receipt nor its admin read activates runtime rules. No imported-artifact publication/active pointer existed. Campaign history/source IDs are not permission to infer one.

## Storage Gap Analysis

| Format-1 information | Existing 011 storage/readback | F change |
| --- | --- | --- |
| Artifact identity, Ruleset/release/compiler, immutable source, manifest provenance, semantic digest | Header plus exact first-approved bytes/hash | Reuse and verify; no format/schema rewrite. |
| Ordered sources, exact source hashes, paths, layer/tier/priority/alwaysInclude/selectors | Artifact-owned ordered source JSON | Reuse complete reconstruction; no global source lookup. |
| Ordered snippets, content/hash/anchor/token estimate/applicability | Artifact-owned ordered snippet JSON | Reuse explicit ordinals and ownership validation. |
| All term wording/concept-kind/weight metadata | Complete snippet JSON, including 773 canonical associations | No flattening, new term index or SQL match logic. |
| Source dependency ownership/order | Artifact-owned dependency edges and source JSON | Reconcile against exact bytes; no legacy dependency inference. |
| Import/publication/active separation for compiled histories | No imported publication/active association | Add opt-in migration 012 only. |

`SqlServerCompiledRulesArtifactStore` becomes partial; its existing import/readback algorithms are unchanged. New [SQL access](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/SqlServerCompiledRuleRetrieval.cs) selects one exact `(RulesetId, SemanticSha256)` under the configured authorized Ruleset, then reuses `ReadCoreAsync` and FR-025 verification. Structured rows and retained bytes are both checked; neither can silently disagree. Byte hash, semantic digest, all normative fields, ordinals, identities, estimates, selectors, terms and graph ownership are verified before retrieval.

Migration 012 adds an idempotent composite parent index and `published_rule_artifacts` / `active_rule_artifacts` with artifact/publication ownership FKs. The existing import's Ruleset/semantic uniqueness remains. Publication persists one association per artifact; activation atomically changes one compiled pointer per Ruleset. Imports, old publication rows and legacy pointers remain intact. No old migration changes, data reset, term schema, Campaign binding or down-migration promise. Existing backup/restore upgrade policy applies. Default/template scripts are packaged. Only enabled compiled deployments require 012 in schema inspection/bootstrap; legacy readiness/plans remain unchanged. Real upgrade/repeat and constraint tests prove representative pre-011 history and legacy active state survive.

## Authority, Publication and Readiness

[CompiledRuleRuntime](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/CompiledRuleRuntime.cs) uses `EternalCycle:CompiledRules:Enabled` (default false) as a deployment runtime grant, scoped to configured Ruleset. Authorization precedes request preparation and any existence/content lookup. Import rights do not grant runtime access. This is the existing trusted single-user reference boundary, not a multi-user authentication layer; other deployments must authenticate and establish grants before service entry.

Separate library `PublishAsync` / `ActivateAsync` require administration enablement, informed user approval and any configured operator safeguard. Internal SQL primitives are not alternate public consent bypasses. No new administrative MCP interface is added; broader administrative tools remain FR-032. These gates do not evaluate import trust again. Activation requires publication; runtime requires both publication and exact active selection. Historical candidates remain readable under existing administrative read authorization but cannot mix with active retrieval.

`ec_get_compiled_rule_context` is a separate read-only/idempotent tool discovered by existing assembly registration. It accepts A's exact portable request, not a Campaign/provider locator or hidden defaults. It returns E's compact packet only; `GetContextAsync` retains rich service evidence for authorized integration/tests. No unselected compiled import changes `ec_get_rule_context`, and explicit compiled failure cannot fall back to legacy/another artifact.

Successful selected-closure retrieval is rule-context readiness, not whole-game `GameplayReady`. Imported artifacts are already complete: unavailable/corrupt imported content fails closed, not fabricated progressive Pending. Legacy per-source Pending/priority behavior is untouched. `gameplay.resolve` also requires durable UserConfirmed/Verified host acknowledgment matching this artifact's `gm-host-bootstrap` source hash. Context assembly does not acquire that gameplay gate. Mandatory Kernel/procedure, selector compatibility, closure and budget still come from A-E.

## SQL Query and Integrity Boundary

Runtime uses six parameterized scoped SELECTs: exact artifact lookup, publication/active eligibility, then four existing FR-025 header/source/snippet/dependency queries. All artifact reconstruction and eligibility occur in one serializable transaction; no half-import or concurrent activation can mix row sets. Gameplay additionally reads host configuration and verifies the selected bootstrap hash. Queries use fixed validated schema substitution and typed parameters, not user SQL fragments. Child queries use the exact import ID. Explicit `ORDER BY`/ordinal checks restore artifact order; physical insertion order is not authority.

Full verified readback yields a private immutable artifact snapshot for the unchanged engine:

```text
trusted imported artifact + separately approved publication/activation
    -> authorized exact scope -> coherent lossless SQL verification/readback
    -> A Prepare -> B FindCandidates -> C Rank -> D Expand -> E Build
    -> compact runtime packet (service evidence retained separately)
```

No SQL vocabulary matching/ranking/applicability/dependency/budget algorithm, per-snippet SQL loop, provider reacquisition, compiler call or mutable cache. Per-request complete verification favors integrity over premature optimization; G owns measured cost/performance evidence. Expected SQL/schema/readiness/authorization/integrity failures return fixed bounded categories without paths, identifiers, artifacts or raw exceptions. Cancellation during SQL or A-E yields no partial packet and remains cancellation. The blocked-command regression exposed SqlClient returning SqlException after cancellation; the supplied cancelled token now preserves the authoritative cancellation classification.

## Reference Equivalence Design

[CompiledRuleSqlRuntimeTests](../../examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/CompiledRuleSqlRuntimeTests.cs) provides reusable `CompiledRuleEquivalence`: one artifact/request feeds pure A-E and the same approved/imported artifact feeds SQL/runtime. Its explicit semantic projection compares scope, complete packet text/order, B counts, ranked root identities/scores/priorities/contributions/inclusion reasons, complete D prerequisites/support/source/snippet semantics, E member presentation, selected/excluded root decisions and every budget total. Successful counts alone cannot pass. Stable failure categories are compared rather than raw SQL/exception text.

Canonical tests import actual canonical compiler bytes through FR-025 approval. Every canonical source/snippet/hash/selector/dependency/estimate/term and both byte/semantic identities survive readback. Fixtures seed the required matching host acknowledgment before gameplay requests; authorization/publication/readiness are additional service gates, not altered A-E semantics.

## Canonical Results

| Request / budget | Reference and SQL roots/members | Used | Remaining | Budget exclusions |
| --- | --- | --- | --- | --- |
| fighting / 8,000 | 15 / 15 | 4,746 | 3,254 | 0 |
| fighting / 3,000 | 13 / 13 | 2,985 | 15 | 2 |
| conflict / 8,000 | 15 / 15 | 4,746 | 3,254 | 0 |
| conflict / 3,000 | 13 / 13 | 2,985 | 15 | 2 |
| preparation / 8,000 | 14 / 14 | 3,430 | 4,570 | 0 |
| preparation / 3,000 | 13 / 13 | 2,979 | 21 | 1 |
| campaign canon, context.assemble / 8,000 | 7 / 7 | 1,801 | 6,199 | 0 |
| campaign canon, context.assemble / 1,400 | 6 / 6 | 1,394 | 6 | 1 |
| unknown / 8,000 | 11 / 11 | 2,027 | 5,973 | 0 |
| empty / 8,000 | 11 / 11 | 2,027 | 5,973 | 0 |

Exact identities/order/evidence match in every row. Conflict retains all five B-inapplicable hits as excluded from eligibility. Both 900-point persistence-authority roots in context assembly retain contributions/scores. Required-only gameplay cost 2,027 and Context Kernel cost 1,010 succeed exactly; one below fails identically as insufficient packet budget. Canonical gameplay `save` and combined `fighting/conflict/save/preparation` fail identically in D: the required persistence-authority operation selector is incompatible. No manifest/selector fix, omitted edge, fallback or partial packet is introduced.

Canonical artifact remains 10 sources, 154 snippets, 773 term associations and 275,622 bytes. Semantic digest: `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`. Serialized-byte SHA-256: `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`. These are regression evidence, not production constants.

## Additional Real SQL Evidence

- Direct A -> B, transitive A -> B -> C -> D, shared A/B -> C, diamond A -> B/C -> D and root-also-dependency fixtures compare complete semantic results at exact, one-above and one-below budgets. Shared content/cost is charged once; root evidence survives dependency support. Ten selector cases preserve World/module/mode/operation incompatibility and topic-independent prerequisites.
- Multiple imported histories reuse source/snippet IDs with changed content/hash without mixing. Explicit activation controls runtime; historical admin reads remain unchanged. Reimport/concurrent publication/activation is idempotent. Import alone, publication and activation do not mutate legacy active/pinned releases or legacy packet/readiness.
- Local, simulated GitHub Release and custom providers converge through one approved importer; retrieval remains usable after the local source file disappears. Runtime never branches on provider or reacquires bytes.
- Nine safe disposable corruption variants (exact bytes/hash/header/source/terms/dependency/snippet/ownership/ordinal) fail consistently with no partial output or fallback. Wrong Ruleset and disabled grant deny before SQL/existence. Missing artifact/publication/activation/schema have bounded distinct failures. FKs reject cross-Ruleset publication/active ownership.
- Six concurrent reads while an unrelated artifact imports, repeated retrieval and changed culture produce identical projections. Descending physical reinsertion proves ordinal readback. Nested caller mutation cannot change packets or durable state. Pre-cancelled and genuinely blocked SQL reads cancel without partial packet; subsequent retrieval succeeds.
- Four host states plus changed bootstrap hash verify gameplay gating; context assembly remains independent. Opt-in schema readiness/bootstrap and actual 012 output packaging are tested without changing disabled legacy behavior.

## Validation Evidence

Executed Release tests before staging: F focused 68/68 (67 SQL cases plus error-registry coverage); full portable 986/986, CLI/process 64/64 and SQL-enabled Managed 275/275 (1,325 unique full-suite cases; zero skips). The full Managed suite includes real disposable LocalDB migrations/import/readback/publication/retrieval and existing worker/recovery/persistence regressions, not mocks alone.

Executed overlapping targeted compatibility subsets: A-E 417/417, FR-025 portable 264/264, FR-024 235/235, FR-023 70/70, FR-022 conformance/portable boundary 17/17 and CLI dependency boundary 1/1. These are subsets, not additional unique-test totals. Initial F execution had one blocked-cancellation failure; the token classification fix was rerun through focused and full suites before acceptance. Portable/CLI Release builds passed with zero warnings/errors.

Managed Release build/publish passed with zero warnings/errors. Publish readback verified packaged `distribution-metadata.json` against root `DISTRIBUTION.json`, LICENSE/NOTICE, both 012 scripts and the independent worker executable. The first packaging inspection used the wrong published metadata filename; corrected inspection passed without changing packaging.

Repository validation passed: 314 Markdown files, 7,536 relative links, 223 anchors, 192 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. All nine structural harnesses passed (333 assertions: 13/29/19/48/59/81/35/32/17). Zero blocking questions, orphaned documents or forbidden Campaign-data directories. Deterministic two-build distribution/content checks passed; transient ZIPs were removed by the existing validator and no release artifact was published.

Validation corrections/evidence: the first repository run found four literal service codes absent from the shared error registry. They and the portable retrieval failures now have bounded safe lookup guidance; a focused test covers every category. One subsequent full-suite run returned Interrupted instead of Succeeded in the unchanged `IndependentWorkerRecoversPersistedRunningWorkAndCompletesIt` test (274 passed, 1 failed). Its fixture uses a 100 ms execution lease; the exact scheduling cause was not established. The test then passed in isolation and the normal complete suite passed 275/275 without changing worker/test code. Retain this intermittent-test observation, not a fabricated F root cause or live-worker defect claim.

Exact-staged reruns passed: F 68/68; complete SQL-enabled Managed 275/275, zero failures/skips; all nine structural harnesses, repository validator and deterministic distribution/content checks; staged whitespace clean. Working files matched the index with no untracked files, and scope review found no source/compiler/A-E/corpus/011/version/release/tag edits, private paths, credentials, Campaign data or generated artifacts. This final audit-note update is documentation only; repository/whitespace checks are repeated after restaging it. No earlier historical count substitutes for this candidate's evidence.

Executed command families (from repository root): `dotnet build` / `dotnet publish` of the affected Release projects; `dotnet test -c Release --no-restore` on the complete portable and CLI/process projects; Managed `dotnet test -c Release --no-build --no-restore` with `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1`, both full suite and `--filter 'FullyQualifiedName~.F_'`; `pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\validate_repository.ps1`; `git diff --cached --check`. Build/publish files stay under ignored build directories. No source/distribution ZIP semantics change or release artifact publication.

## Classification and Stop

- **A - reference implementation:** .NET service/tool, configurable single-user grant, serializable SQL/JSON readback, migration 012 association/FKs, six-query strategy and LocalDB harness. Do not mandate this layout or these query mechanics for other providers.
- **B - Managed Service contract:** authorize before disclosure, select one coherent published/active artifact, retain complete verified semantics/history, separate import/publication/activation, fixed failures and cancellation, compact delivery with evidence service-side, no fallback that masks explicit corruption.
- **C - Eternal Cycle-wide:** Repository Canon retrieval cannot guess missing/incompatible rules, omit mandatory GM procedure or dependency/budget safety, or confuse Campaign Canon with the selected rule artifact. No AI/client transport lifetime or provider reacquisition dependency enters retrieval.

Remaining unproven: G final quality/context-cost/performance evidence; H integrated objective closure; production remote SQL authentication/concurrency, multi-user authorization and actual AI-host/MCP interoperability. No live deployment acceptance is inferred from disposable SQL tests. Legacy progressive readiness is preserved, not newly redesigned; imported completeness is a different lifecycle boundary. FR-027 alone owns Campaign binding/adoption. F stops after validated commit and normal `main` push; no G/H, release/package publication, VERSION/tag/corpus/compiler/acquisition change.
