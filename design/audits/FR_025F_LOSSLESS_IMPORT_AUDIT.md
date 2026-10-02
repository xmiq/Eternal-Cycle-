# FR-025F Lossless Artifact Import Audit

## Authority and Storage Gap Analysis

F alone is owner-authorized beneath [FR-025](../FR_025_EXECUTION_PLAN.md). Starting `main` and upstream were clean at `2986f60c7ca7a730dbb32033de6fde8b4f03007d`. VERSION and release/discovery tags must remain unchanged. FR-025 remains selected/incomplete; G and FR-026 are not authorized here.

Inspection before migration design covered migrations 002, 006, 007 and 010, `SqlServerPublishedRuleStore`, the publication coordinator, setup migration planning, portable artifact/approval/writer contracts, existing SQL integration fixtures and A-E audits.

| Existing storage | Complete format-1 gap / F resolution |
| --- | --- |
| `rule_releases`, unique `(ruleset_id, source_identity)` | Source identity is not semantic artifact identity. Keep the legacy constraint/history; imported artifacts have their own Ruleset/digest boundary in the same Rule Domain. |
| `compiled_index_json`, `rule_chunks` | Legacy index carries compilation time and chunk metadata, not all compiler/manifest/artifact identity, content hashes and retrieval carrier fields. It cannot be relabelled as lossless format 1. |
| `rule_chunk_selectors` | Case-insensitive legacy selector deduplication is not the format-1 ordinal set contract. Retain full applicability JSON within artifact-owned source records. |
| `rule_dependencies` | Expanded chunk edges differ from format-1 source dependencies. Store artifact-local source edges with explicit ordinals and both endpoint foreign keys. |
| chunk metadata / preparation rows | Does not retain complete reviewed term/kind/weight/relationship arrays. Store the entire normative retrieval object in each artifact-owned snippet, without inventing compiler-only origin evidence. |
| `active_rule_releases`, release states and update/preparation history | Import writes none of these. Existing source compilation, staging, publication, activation, history and readiness continue unchanged. |

Exact first-approved bytes are retained alongside the lossless semantic projection for byte provenance, export and projection verification. This bounded duplication is deliberate. Reformatting can share semantic identity while having different byte SHA; semantic idempotency retains the first bytes/hash rather than claiming subsequent bytes were also retained. No acquisition-event/evidence table is required, so provider metadata and potentially private locators are not persisted. No timestamps enter import identity.

## Implemented Boundary and Identity

The portable [import API](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRulesArtifactImport.cs) and [SQL adapter](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/SqlServerCompiledRulesArtifactStore.cs) accept only `ValidatedTrustedCompiledRulesArtifact`. B alone constructs that approved wrapper. Orchestration and adapter both require exact authorized Ruleset scope; no raw-byte/JSON/parsed/valid-only or provider-specific write overload exists. Neither trust nor acquisition runs again during storage. There is no new MCP tool, import CLI or default approval policy.

`ARTIFACT-` plus SHA-256 of a domain-separated, UTF-8 length-framed Ruleset ID and semantic digest is the deterministic storage ID. Ruleset/digest also has durable uniqueness. Source ID, semantic digest, retained exact-byte SHA, acquisition evidence and publication ID remain distinct. Imported audit time is incidental, not an identity input. Repeated same semantic input reuses one receipt/representation; `Created` distinguishes this invocation. Formatting-only variants retain the first bytes/hash and do not manufacture another history. Equal bytes through different providers likewise reuse that identity without storing provider evidence/private locators.

Reuse verifies complete stored semantics against the incoming approved model, not merely a matching key/digest. Readback checks source and snippet ownership/ordinal continuity, reconstructs ordered source dependencies, invokes the unchanged FR-022 parser/validator, rehashes exact bytes and compares complete canonical semantic serialization. Corruption/contradiction fails as `ARTIFACT_IMPORT_CONFLICT`; no repair/upsert overwrites historical content. This is a practical integrity check, not a claim to prove the impossibility of cryptographic collisions.

## Storage, Ordering and Atomicity

Migration [011 template](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/Schema/011_compiled_artifact_import.template.sql) adds four tables in the existing configured Rule Domain:

- `imported_rule_artifacts`: complete format-1 header JSON, exact first-approved bytes/hash, semantic/Ruleset identity and incidental audit time.
- `imported_rule_sources`: artifact-local source ID and explicit ordinal; all source fields remain in normative JSON except dependencies represented as ordered foreign-key edges.
- `imported_rule_snippets`: source owner and explicit snippet ordinal, complete snippet JSON including all terms/kinds/weights/relationships. No audit-only origin evidence is invented.
- `imported_rule_dependencies`: source and target within the same artifact, dependency ordinal and uniqueness.

This JSON/relational hybrid avoids repeating format-1 as a lossy second C# model or prematurely designing FR-026 indexes. Explicit ordinal reads restore every sequence. BIN2 source/key collation prevents case-insensitive database defaults from merging ordinal identities. Semantic snippet IDs remain complete JSON values rather than being truncated to an arbitrary SQL key size; FR-022 enforces uniqueness. SQL enforces parent/owner/source uniqueness, nonnegative ordinals, valid JSON and artifact-local dependency endpoints.

One serializable transaction takes `UPDLOCK, HOLDLOCK` on the deterministic primary-key range. Competing same-artifact imports wait and verify/reuse; one creates children. New writes use set-based header/source/snippet/dependency insertion, followed by full verification within the transaction. Commit precedes success. Failure/cancellation rolls back all new records; previous imports, source hashes, retrieval metadata, publication history and active pointers are untouched. Readback uses a consistent transaction. The compact receipt contains ID, Ruleset, semantic and retained-byte hashes, created/reused state and source/snippet/term/relationship counts, not full records or publication acknowledgement.

Fixed failures distinguish invalid input/scope, integrity conflict, missing import schema and storage/transaction failure. Raw SQL/connection exceptions are not retained as inner exceptions or returned diagnostics. Cancellation remains `OperationCanceledException` with a fixed safe message. B's private bytes and frozen complete model remain authoritative through F; mutations of extracted byte copies and exposed collections cannot change approval or readback.

## Migration and Existing Publication Compatibility

011 is necessary because the legacy release identity/projection cannot losslessly represent an imported artifact or multiple artifact histories with repeated source IDs. It is additive and repeat-safe, packaged as both rendered/default SQL and routed template, and offered by the existing trusted setup planner. No historical migration is edited. There is no conversion, deletion or recreation of existing rules/campaign data; legacy source uniqueness and active publication remain intact.

Actual SQL tests upgrade representative 001-010 state, preserve an active historical source publication, rerun 011, import new candidates and then successfully use the existing staging/publication/activation path again. Existing gameplay readiness and preparation requirements remain unchanged: import capability is not a new condition for playing with already-published rules. The clean downstream integration point is complete imported artifact readback for separately authorized publication/projection, not automatic promotion or a second runtime retriever.

No down migration is offered. This follows forward migration plus verified backup, Canon audit, controlled validation/restore rather than promising downgrade support. Deployment against remote SQL Server, external authentication/permissions and actual campaign backups still require infrastructure acceptance.

## Acceptance Evidence

| Requirement group | Executed proof |
| --- | --- |
| Approved-only input, scope and provider neutrality | Portable write-signature/constructor checks, null/wrong-scope/precancel tests; actual C file, D HTTP substitute and E custom fixture traverse unchanged B into the same SQL importer. |
| Canonical complete round trip | All model fields compared, all retained exact bytes equal, FR-022 validity/digest checked; one header, 10 sources and 154 snippets, not three provider copies. |
| Exact bytes / semantic idempotency | Sequential and formatting-only reimports return the first retained hash/bytes and no duplicate sources/snippets/dependencies; provider metadata does not select storage. |
| Changed artifact and repeated source IDs | Two valid artifacts with source ID X and different source hashes/content coexist independently; old readback and active pointer remain unchanged. |
| Retrieval, applicability and dependencies | Multiple/shared wording with distinct kinds/weights, relationships, empty retrieval on another snippet, operation selectors and ordered source edges all survive full comparison. |
| Integrity conflict | Corrupted projection, wrong key, bytes or hash cannot become readback/idempotent success; old rows are not overwritten. |
| Atomicity | SQL failure injection after header, sources, snippets/retrieval, dependencies and verification rolls back each new import, preserves all prior rows and active release, and leaks no raw exception detail. |
| Cancellation / concurrency | Mid-import cancellation leaves all four tables empty; two actual concurrent imports converge to one created receipt and one reused result with no duplicate children. |
| Durable ownership / order | Real foreign keys reject orphan snippets and targets present only in another artifact; explicit readback ordering and case-distinct sources pass under default case-insensitive LocalDB collation. |
| Migration / packaging / legacy flow | Representative upgrade, repeat migration, planned capabilities, preserved active/history and later source publication pass; default/template equality and publish assets verified. |

Canonical output is unchanged, not recompiled differently to fit persistence:

| Metric | Result |
| --- | --- |
| Rule Sources / snippets | 10 / 154 |
| Retrieval terms in normative format-1 carrier | 773 |
| Exact artifact bytes | 275,622 |
| Semantic SHA-256 | `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B` |
| Serialized-byte SHA-256 | `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6` |

## Executed Validation

| Validation | Result |
| --- | --- |
| Portable, CLI and Managed restore / Release builds | Pass; all dependencies already available, zero build warnings/errors. |
| F portable focused / A-F combined | 18/18 / 264/264. |
| F SQL integration plus database-free packaging | 15 real SQL cases plus one packaging case, included in the complete SQL-enabled suite. |
| Complete portable / CLI-process / SQL-enabled Managed | 569/569 / 64/64 / 197/197, zero failures/skips; **830 unique tests**. |
| FR-023 loader/snippet/writer | 70/70. |
| FR-024 input/enrichment/pipeline/audit/canonical/acceptance | 235/235. |
| FR-022 contract / portable boundary | 17/17; separate CLI dependency-boundary case 1/1. |
| Managed publish build output | Pass; 011 default/template, distribution metadata, Apache LICENSE and NOTICE present in ignored build output. No release publication/archive generated. |
| Nine structural harnesses | 333/333: 13, 29, 19, 48, 59, 81, 35, 32 and 17 respectively. |
| Full repository validator | Pass: 305 Markdown files, 7,439 relative links, 214 anchors; 191 canonical documents, 15 family indexes, 43 templates, 5 roles, 1,276 terms, 208 roadmap tasks and 36 Future Revisions. Zero blocking questions, orphaned Markdown and forbidden campaign-data directories. |
| Diff / whitespace / scope | Reviewed; no compiler/source/vocabulary, campaign, VERSION, tag/release or generated-artifact changes. |

Release `dotnet test` uses the existing projects. SQL proof sets `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1` and creates/drops only automatically named disposable LocalDB databases; no campaign database is selected. Without opt-in the SQL cases skip, while packaging remains database-free. Repository validation uses `pwsh -NoProfile -ExecutionPolicy Bypass -File .\tools\validate_repository.ps1`; the initial normal invocation was blocked by the environment's unsigned-script policy, not a repository failure. Bypass is process-local, not a changed machine policy.

Finalization requires exact staged-tree whitespace/scope review, focused F reruns and repository validation before the focused commit and normal branch push. Focused reruns overlap the full suites, not additional unique test totals. Generated build/publish outputs remain ignored; no release/archive is produced. Existing Git LF-to-CRLF conversion notices are not whitespace failures.

The first broader SQL-enabled run exposed stale preexisting opt-in expectations: migration planning stopped at 009 despite existing 010, and the source corpus expected 147 chunks rather than the already-established 154/1,004-selector/803-dependency baseline. Only test expectations were corrected to that baseline and the additive 011 plan; production compilation/publication behavior was not changed. The corrected complete suite and strengthened cross-artifact ownership test were rerun successfully. Initial focused build/test defects were corrected before these results; no failed run is counted as passed evidence.

## Classification and Remaining Boundary

- **A - Reference implementation:** portable C# types, SQL Server serializable range locking, JSON/ordinal layout, additive migration/packaging, and disposable LocalDB/test hooks.
- **B - Managed Service contract:** approved-only authorized input; complete immutable artifact ownership; atomic/idempotent import and safe failure/cancellation; preserved history/current active rules and separate publication/activation.
- **C - Eternal Cycle-wide:** provider choice cannot alter Canon/artifact authority, trust or readiness; repeated imports cannot silently rewrite history. Transport evidence is not semantic identity.

Real LocalDB storage/migration/concurrency proof is available; live remote SQL Server permissions/deployment, real GitHub Release/private authentication, arbitrary custom providers and live host acceptance are not certified. Local HTTP/custom substitutes prove importer equivalence, not those transports. No unresolved contract/design question remains from F.

FR-025 remains selected/incomplete; A-F complete, G pending, FR-026 unselected. No automatic publish/activate, campaign adoption, new campaign, ranking/search/index, acquisition workflow redesign, compiler/vocabulary change, release packaging, VERSION or tag movement is included. Stop after the validated focused commit and authorized normal `main` push; start nothing next.
