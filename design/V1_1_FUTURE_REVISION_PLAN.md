# Eternal Cycle v1.1 Future Revision Plan

## Purpose

This document is the owner-authorized Phase 13 implementation plan for the remaining v1.1 work. It reconciles the planning requirements numbered `#1` through `#34` and Canon-integrity cases `8.1` through `8.9` against the released architecture and completed FR-011, FR-012, and FR-017 through FR-021 work.

This is planning governance, not blanket implementation authorization. Completed task markers preserve implementation history; each remaining change still requires an explicit owner selection, focused validation, audit, and commit.

## Existing Foundation

The plan does not reopen or duplicate these completed foundations:

- [FR-011 Context Assembly and Turn Persistence](../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) owns mandatory reads, Affected Sets, persistence completion, retry, and save status.
- [FR-012 Canonical Data Ownership](../docs/persistence/CANONICAL_DATA_OWNERSHIP.md) owns one-authority-per-mutable-fact and reference/derived/history boundaries.
- [FR-018 Rule Compilation](../docs/rules/RULE_COMPILATION_AND_RETRIEVAL.md) owns Repository Canon authority, runtime chunks, applicability, dependency closure, and bounded selection.
- [FR-019 Managed Data Architecture](../docs/persistence/MANAGED_DATA_SERVICE.md) owns provider-neutral `DIRECT` and `MANAGED` boundaries, Logical Data Namespaces, and service-owned Rule Publication.
- [FR-020 Managed First Run](../docs/persistence/RUNNING_ETERNAL_CYCLE.md) owns readiness, source selection, initial publication, campaign discovery, and setup diagnostics.
- [FR-021 Managed Operations](../docs/persistence/MANAGED_OPERATIONS.md) owns durable work, progressive rule readiness, independent execution, compact Rule Packets, the mandatory [GM Runtime Procedure](../docs/rules/GM_RUNTIME_PROCEDURE.md), GM Host Configuration, and constrained existing-record patches.

Repository inspection found that the current compiler remains embedded in the reference MCP publication path, the official runtime input is still a source manifest rather than a portable precompiled artifact, rule relevance is primarily selector/topic based rather than vocabulary-ranked, campaign reads are address lists rather than an active dependency closure, and no dedicated administration client or long-campaign Canon-integrity suite exists. Those are the principal remaining boundaries.

## Status Categories

- **Implemented:** the requirement has dedicated current implementation and validation. A later task may preserve or measure it without reopening ownership.
- **Partial:** a valid foundation exists, but the complete v1.1 behavior or release boundary is absent.
- **Pending:** no dedicated implementation satisfies the requirement.
- **Roadmapped:** one or more tasks below own the remaining work.

## Dependency Graph

```text
FR-022 Portable Compiled Rules Artifact Contract
  +-> FR-023 Deterministic Offline Rules Compiler
  |     +-> FR-024 Vocabulary and Compiler Audit
  |     +-> FR-034 v1.1 Artifact Packaging and Release Workflow
  +-> FR-025 Provider-Neutral Artifact Acquisition and Import
        +-> FR-026 Compiled Rule Store and Intelligent Retrieval
              +-> FR-027 Stable Campaign Binding and Gameplay Entry
              |     +-> FR-028 Active Campaign Canon Closure
              |           +-> FR-029 Active State and Correction History Separation
              +-> FR-032 Managed Administrative Read Surface

FR-030 Persistence Result and Lost-Acknowledgement Recovery
  +-> FR-031 Context-Economics Instrumentation
  +-> FR-032 Managed Administrative Read Surface

FR-024 + FR-026 + FR-032
  -> FR-033 Administration Client and Portable Diagnostic Report

FR-023 + FR-025 + FR-026
  -> FR-034 v1.1 Artifact Packaging and Release Workflow

FR-027 + FR-028 + FR-029 + FR-030
  -> FR-035 Canon-Integrity Regression Suite

FR-031 + FR-033 + FR-034 + FR-035
  -> FR-036 Long-Campaign and v1.1 Integrated Acceptance
```

FR-023 and FR-025 may proceed in parallel after FR-022. FR-030 may proceed in parallel with the compiled-rule stream. No task may infer selection merely from being dependency-ready.

## Roadmapped Tasks

### FR-022 - Portable Compiled Rules Artifact Contract

- **Status:** Complete. See the [Compiled Rules Artifact](../docs/rules/COMPILED_RULES_ARTIFACT.md) and [FR-022 audit](audits/FR_022_COMPILED_RULES_ARTIFACT_AUDIT.md).
- **Primary classification:** Eternal Cycle-wide contract.
- **Scope:** Define a storage- and provider-neutral, versioned artifact for derived snippets, stable snippet identity, executable content, provenance, retrieval vocabulary, weights, dependencies, applicability, compiler/format versions, manifests, integrity, and compatibility.
- **Boundaries:** Define schemas and conformance fixtures only. Do not implement the compiler, GitHub acquisition, SQL import, or release publication.
- **Hard dependencies:** Completed FR-018 and FR-019 authority boundaries.
- **Acceptance:** The contract distinguishes Canon from derivatives; validates a minimal conforming artifact; rejects missing provenance, dangling dependencies, incompatible versions, and integrity failures; and names no mandatory language, database, host, or acquisition provider.
- **Traceability:** `#2`, `#34`; WP1 and WP8.

### FR-023 - Deterministic Offline Rules Compiler

- **Status:** Complete. See the [Standalone Rules Compiler](../examples/tooling/rules-compiler-dotnet/README.md) and [FR-023 acceptance audit](audits/FR_023_REPRODUCIBLE_RULES_COMPILER_AUDIT.md).
- **Primary classification:** Reference implementation proving an Eternal Cycle-wide reproducibility contract.
- **Scope:** Add a separate compiler tool that reads the canonical Rule Source manifest and ordinary materialized rule files, creates useful heading/content snippets, preserves provenance and applicability, emits the FR-022 artifact, and runs before gameplay.
- **Boundaries:** Do not add ranked runtime retrieval, artifact acquisition providers, or release publishing.
- **Hard dependencies:** FR-022.
- **Acceptance:** Repeated compilation of the same input is byte-identical or equivalently reproducible; stable unchanged snippets retain identity; changed sources invalidate appropriate provenance; source-root escape and malformed manifests fail; the compiler can run without the MCP service or campaign data.
- **Traceability:** `#1`, `#2`, `#34`; WP1.

### FR-024 - Retrieval Vocabulary and Compiler Audit

- **Execution state:** Owner-selected and in progress; FR-024A input and FR-024B exact enrichment/origin-evidence checkpoints complete. [FR-024 execution packages](FR_024_EXECUTION_PLAN.md) bound the remaining artifact/CLI integration, audit, corpus, and closure work without creating new objectives.
- **Primary classification:** Reference implementation and regression/diagnostic tooling.
- **Scope:** Add reviewable normalized terms, controlled alternatives, optional weights/relationships, vocabulary-quality checks, and a compiler report for collisions, over-broad terms, weak coverage, and poor snippet boundaries.
- **Boundaries:** Do not train a model, scrape uncontrolled synonym sources, or make diagnostics authoritative rule content.
- **Hard dependencies:** FR-023.
- **Acceptance:** A representative rule is discoverable by an approved alternative term; irrelevant broad matches are rejected or visibly downgraded; vocabulary provenance is reviewable; repeated audits are deterministic; the report contains no private reasoning or campaign data.
- **Traceability:** `#1`, `#4`, `#30`, `#34`; WP1 and WP2.

### FR-025 - Provider-Neutral Artifact Acquisition and Import

- **Primary classification:** Managed Service contract with reference providers.
- **Scope:** Separate artifact-byte acquisition from shared validation and import. Support local files and a GitHub Releases default while preserving extension points for mirrors, private repositories, and compatible custom providers.
- **Boundaries:** Do not privilege GitHub inside the artifact contract, implement rule ranking, or publish v1.1 assets.
- **Hard dependencies:** FR-022. It may proceed in parallel with FR-023.
- **Acceptance:** Identical local and downloaded artifacts pass the same validator/importer and produce the same imported identity; provider failure cannot bypass integrity or compatibility checks; acquisition metadata remains distinct from artifact authority; an alternate test provider requires no importer rewrite.
- **Traceability:** `#23`, `#24`, `#25`, `#26`, `#34`; WP8.

### FR-026 - Compiled Rule Store and Intelligent Retrieval

- **Primary classification:** Managed Service behavior plus reference implementation.
- **Scope:** Import FR-022 artifacts into the normal searchable Rule Domain, replace giant-document search records with reviewed snippets, rank normalized/weighted vocabulary with structural selectors, expand dependencies, retain diagnostics, and return the existing compact model-facing packet.
- **Boundaries:** Repository Canon remains authoritative. Do not retrieve Campaign Canon, alter the player-turn contract, or expose service-only metadata in ordinary packets.
- **Hard dependencies:** FR-023, FR-024, and FR-025.
- **Acceptance:** Alternative terminology retrieves the intended snippet with explainable evidence; operation/world/module filters and mandatory sources remain correct; dependency closure is complete; giant source blobs are not the normal search unit; compact packet regressions do not exceed their approved envelope; unavailable required rules fail or remain Pending.
- **Traceability:** `#3`, `#4`, `#5`, `#30`, `#31`, `#32`, `#34`; WP2 and WP3.

### FR-027 - Stable Campaign Binding and Managed Gameplay Entry

- **Primary classification:** Eternal Cycle-wide gameplay invariant and Managed Service contract.
- **Scope:** Establish an authoritative active Campaign binding and supported gameplay entry that carries interaction identity, invokes `gameplay.resolve`, obtains mandatory rules, and demotes conversation search to non-authoritative diagnostics.
- **Boundaries:** Preserve the existing GM Runtime Procedure, Unknown rules, and player-agency contract rather than rewriting them.
- **Hard dependencies:** FR-026 and completed FR-021.
- **Acceptance:** A bound session resumes the same Campaign ID without conversational inference; missing or conflicting binding stops safely; normal entry automatically obtains the Kernel and GM Runtime Procedure; internal continuation cannot become new player input; missing facts remain unretrieved/Unknown rather than invented.
- **Traceability:** `#6`, `#15`, `#16`, `#17`, `#18`; WP4.

### FR-028 - Active Campaign Canon Working-Set Closure

- **Primary classification:** Managed Service contract grounded in existing Canon ownership.
- **Scope:** Build the campaign-side dependency closure for current actors, relationships, unresolved state, temporal facts, and typed references required by one interaction, without loading an entire campaign.
- **Boundaries:** Do not create a second Canon owner, use semantic similarity as existence proof, or collapse current and historical state.
- **Hard dependencies:** FR-027 plus completed FR-011 and FR-012.
- **Acceptance:** A narrow request expands to structurally required records; established but irrelevant records remain stored without entering the packet; required missing records trigger further retrieval or failure; not-retrieved, nonexistent, Unknown, contradicted, and unresolved-player-choice states remain distinguishable; closure cost scales with the active situation rather than campaign age.
- **Traceability:** `#7`, `#9`, `#17`, `#33`; WP5; Canon cases `8.3`, `8.4`, and `8.6`.

### FR-029 - Active State and Correction History Separation

- **Primary classification:** Eternal Cycle-wide persistence ownership with Managed/reference migration work.
- **Scope:** Audit active campaign records for accumulated corrections or historical prose, define bounded current-state shapes, move correction/audit history to existing historical owners, and retain references and provenance.
- **Boundaries:** Do not erase valid history, create duplicate current owners, or reopen FR-012 ownership rules. Existing patch semantics remain the ordinary small-update path.
- **Hard dependencies:** FR-028.
- **Acceptance:** Active records no longer grow through unbounded correction arrays; migrated history remains traceable and chronologically scoped; current ownership stays singular; patches preserve omitted state; migration is backup-first, idempotent, validated, and compatible with existing campaigns.
- **Traceability:** `#9`, `#10`, `#11`, `#12`, `#33`; WP6; Canon cases `8.4` and `8.7`.

### FR-030 - Persistence Result and Lost-Acknowledgement Recovery

- **Primary classification:** Managed Service contract plus reference implementation.
- **Scope:** Keep routine commit success compact and add a lightweight transaction/idempotency lookup that reports committed, failed, or pending/unknown without replaying mutations.
- **Boundaries:** Do not weaken automatic persistence, receipt validation, concurrency, retry idempotency, or cloud/local authority semantics.
- **Hard dependencies:** Completed FR-011 and FR-017. It may proceed independently of FR-022 through FR-029.
- **Acceptance:** Lost tool output can be reconciled from stable identity; committed work is not duplicated; pending and failed states are distinct; successful responses omit mutation echoes and verbose diagnostics; retry resumes the same transaction; result lookup has bounded output and authorization.
- **Traceability:** `#13`, `#14`, `#31`, `#32`; WP7.

### FR-031 - Context-Economics Instrumentation and Budgets

- **Primary classification:** Managed Service observability and regression tooling.
- **Scope:** Measure model-facing request/response bytes and estimated tokens for rule retrieval, Campaign reads, persistence, diagnostics, and complete gameplay interactions; enforce/report practical budgets around the approximately 20K ordinary-operation target.
- **Boundaries:** Do not capture private reasoning, treat 20K as a per-turn target, or silently omit required authority to meet a metric.
- **Hard dependencies:** FR-026, FR-027, FR-028, and FR-030.
- **Acceptance:** Diagnostics attribute payload cost by operation and interaction; test thresholds catch regressions; required closure failure remains explicit; ordinary representative play retains final-response headroom; the existing rule-only ceiling is measured separately from total interaction cost.
- **Traceability:** `#5`, `#13`, `#31`, `#32`, `#33`; WP3, WP7, and WP10.

### FR-032 - Managed Administrative Read Surface

- **Primary classification:** Managed Service contract with reference MCP tools.
- **Scope:** Complete safe administration/query capabilities for campaigns, bindings, versions, records/revisions, transactions/receipts, validation/recovery, Rule Releases, compiled artifacts, configuration, and diagnostics.
- **Boundaries:** Read access and narrowly authorized operations only; no arbitrary SQL, unrestricted mutation, secret disclosure, or administration requirement for ordinary play.
- **Hard dependencies:** FR-025, FR-026, and FR-030.
- **Acceptance:** Every listed state has a bounded authorized query; identifiers and versions are visible without database archaeology; pagination/filtering prevent unbounded output; sensitive values remain redacted; existing gameplay tools remain unchanged.
- **Traceability:** `#27`, `#29`, `#30`, `#32`; WP9.

### FR-033 - Administration Client and Portable Diagnostic Report

- **Primary classification:** Reference implementation and portable diagnostics.
- **Scope:** Add a simple developer administration client that invokes MCP directly, displays the FR-032 surfaces, reproduces request-to-response flows outside an AI context, and exports a bounded self-contained Markdown report with structured JSON where useful.
- **Boundaries:** Prefer a conventional CLI unless evidence justifies richer UI. Do not add another service protocol, arbitrary datastore access, or chain-of-thought capture.
- **Hard dependencies:** FR-024, FR-026, and FR-032.
- **Acceptance:** A developer can call representative MCP operations, inspect resulting state, and export a sanitized report containing request identity, normalization/matches/ranking/dependencies, selections/rejections, timings, versions, warnings, and validation evidence; the report is portable and secret-safe.
- **Traceability:** `#27`, `#28`, `#29`, `#30`, `#32`; WP2 and WP9.

### FR-034 - v1.1 Artifact Packaging and Release Workflow

- **Primary classification:** Release governance and reference distribution tooling.
- **Scope:** Produce deliberate Rules-only, Compiled Rules, and target-labelled MCP artifacts with manifests/checksums; publish through GitHub Releases as the supported default while keeping provider-neutral acquisition; explicitly require no `All.zip`.
- **Boundaries:** Automatic Git source archives and the existing full review snapshot are not runtime artifacts. This task does not authorize the v1.1 release or move immutable tags.
- **Hard dependencies:** FR-023, FR-025, and FR-026.
- **Acceptance:** Deterministic package builds contain only declared material; Compiled Rules users need not run the compiler; MCP packages identify targets and provenance; local and GitHub acquisition validate identically; forbidden caches, credentials, private state, and campaign data are rejected; release documentation names each artifact purpose.
- **Traceability:** `#19`, `#20`, `#21`, `#22`, `#23`, `#24`, `#25`, `#26`, `#34`; WP8.

### FR-035 - Canon-Integrity Regression Suite

- **Primary classification:** Regression/testing with Eternal Cycle-wide invariants.
- **Scope:** Implement the deterministic scene fixture and concrete actor, Unknown, chronology, fabrication, closure, persistence-round-trip, and multi-turn drift regressions; generalize shared fixture support only after concrete cases pass.
- **Boundaries:** Fixture names and events never enter production logic or Canon. Tests use supported persistence and runtime entry paths where practical.
- **Hard dependencies:** FR-027, FR-028, FR-029, and FR-030.
- **Acceptance:** Every case `8.1` through `8.9` has a named test; repeated partial interactions preserve actor counts, statuses, unresolved facts, quantities, roles, witness knowledge, location/time, and chronology; patches change only intended facts; the suite fails if narration alone or omitted reads mutate Canon.
- **Traceability:** `#8`, `#9`, `#12`, `#16`, `#17`, `#33`, `#34`; WP5, WP6, and WP10; Canon cases `8.1` through `8.9`.

### FR-036 - Long-Campaign and v1.1 Integrated Acceptance

- **Primary classification:** Integrated regression, deployment acceptance, and release-readiness evidence.
- **Scope:** Exercise substantial historical growth while ordinary gameplay uses stable binding, compiled retrieval, active Campaign closure, patches, acknowledgement recovery, diagnostics, and packaged artifacts within measured context budgets.
- **Boundaries:** This task gathers release evidence; it does not itself declare v1.1 released, change `VERSION`, or create an immutable tag without a separate owner-authorized release gate.
- **Hard dependencies:** FR-031, FR-033, FR-034, and FR-035.
- **Acceptance:** Campaign history grows without linear active-context growth; a multi-turn representative run stays within the approved practical envelope or reports a justified boundary; official and local artifacts import consistently; Canon-integrity suites pass; real-host limitations are separated from automated proof; a release-readiness audit identifies any blockers.
- **Traceability:** `#6` through `#18`, `#31`, `#32`, `#33`, `#34`; WP10.

## Master Requirement Traceability

| Requirement | Disposition | Owning task or evidence |
| --- | --- | --- |
| `#1` Offline Rules Compiler | Compiler complete; vocabulary and compiler-quality audit pending | FR-023, FR-024 |
| `#2` Portable Compiled Rules artifact | Contract and deterministic compiler complete | FR-022, FR-023 |
| `#3` Compiled snippets as normal runtime domain | Partial runtime chunks exist | FR-026 |
| `#4` Intelligent indexed retrieval | Partial selector/topic retrieval exists | FR-024, FR-026 |
| `#5` Compact Rule Packets | Implemented by FR-021; retain measurement | [FR-021 audit](audits/FR_021_GM_RUNTIME_BOOTSTRAP_REPAIR_AUDIT.md), FR-026, FR-031 |
| `#6` Mandatory gameplay procedure | Implemented by FR-021; integrate stable entry | [GM Runtime Procedure](../docs/rules/GM_RUNTIME_PROCEDURE.md), FR-027, FR-036 |
| `#7` Active Campaign Canon closure | Pending | FR-028 |
| `#8` Canon-integrity regression program | Pending | FR-035 |
| `#9` Temporal and causal integrity | Canonical contract exists; runtime regressions pending | [Timeline Engine](../docs/persistence/TIMELINE_ENGINE.md), FR-028, FR-029, FR-035 |
| `#10` Canon ownership normalization | Implemented by FR-012; migration audit remains scoped | [Canonical Data Ownership](../docs/persistence/CANONICAL_DATA_OWNERSHIP.md), FR-029 |
| `#11` Active state versus correction history | Pending | FR-029 |
| `#12` Patch-first persistence | Implemented by FR-021; round-trip proof pending | [FR-021 audit](audits/FR_021_GM_RUNTIME_BOOTSTRAP_REPAIR_AUDIT.md), FR-029, FR-035 |
| `#13` Compact success responses | Substantially implemented; explicit regression pending | FR-030, FR-031 |
| `#14` Lost-acknowledgement recovery | Partial retry exists; status lookup pending | FR-030 |
| `#15` Stable Campaign ID binding | Partial configuration exists; active binding pending | FR-027 |
| `#16` Player-turn boundary | Canonical contract implemented; integrated enforcement pending | FR-021 evidence, FR-027, FR-035, FR-036 |
| `#17` Explicit Unknown handling | Canonical contract exists; closure integration pending | FR-027, FR-028, FR-035 |
| `#18` Conversation-search demotion | Canonical bootstrap implements rule; integrated entry proof pending | FR-027, FR-036 |
| `#19` Rules-only release artifact | Pending | FR-034 |
| `#20` Compiled-Rules release artifact | Pending | FR-034 |
| `#21` MCP release artifact | Partial publishable reference exists; deliberate package pending | FR-034 |
| `#22` No required `All` artifact | Accepted packaging boundary; enforce in release tooling | FR-034 |
| `#23` GitHub Releases default | Pending for v1.1 | FR-025, FR-034 |
| `#24` Acquisition separated from validation/import | Partial Rule Source separation exists; artifact path pending | FR-025, FR-034 |
| `#25` Local-file acquisition | Pending for compiled artifacts | FR-025 |
| `#26` Future/custom providers | Rule Source freedom exists; artifact-provider extension pending | FR-025 |
| `#27` MCP administration interface | Partial tools exist; coherent read surface/client pending | FR-032, FR-033 |
| `#28` Direct MCP invocation from administration | Pending | FR-033 |
| `#29` Portable diagnostic/compile report | Error Dump exists; compiler/retrieval report pending | FR-032, FR-033 |
| `#30` Compiler and retrieval diagnostics | Partial execution diagnostics exist | FR-024, FR-026, FR-033 |
| `#31` Approximately 20K practical context target | Pending measured contract | FR-031, FR-036 |
| `#32` Context-economics instrumentation | Pending | FR-026, FR-030, FR-031, FR-033, FR-036 |
| `#33` Long-campaign scalability regression | Pending | FR-028, FR-029, FR-031, FR-035, FR-036 |
| `#34` Compiler/release validation | Partial repository/distribution validation exists | FR-022 through FR-026, FR-034 through FR-036 |

## Canon-Integrity Traceability

| Case | Primary owner | Supporting tasks |
| --- | --- | --- |
| `8.1` Representative deterministic fixture | FR-035 | FR-029 |
| `8.2` Actor conservation | FR-035 | FR-028 |
| `8.3` Partial-read and Unknown semantics | FR-035 | FR-028 |
| `8.4` Chronology integrity | FR-035 | FR-028, FR-029 |
| `8.5` Fabrication resistance | FR-035 | FR-027, FR-028 |
| `8.6` Active-scene closure | FR-028, FR-035 | FR-027 |
| `8.7` Persistence round-trip integrity | FR-035 | FR-029, FR-030 |
| `8.8` Multi-turn drift stress | FR-035 | FR-036 |
| `8.9` Generalized fixture framework | FR-035 | FR-036 |

## Conceptual Work-Area Traceability

| Planning origin | Final tasks |
| --- | --- |
| WP1 - Compiled Rules Contract and Compiler Foundation | FR-022, FR-023, FR-024 |
| WP2 - Compiler Audit and Search Quality | FR-024, FR-026, FR-033 |
| WP3 - Compiled Rule Domain and Compact Retrieval | FR-026, FR-031 |
| WP4 - Gameplay Entry, Campaign Binding, and Authority | FR-027 |
| WP5 - Active Canon Working Set and Scene Integrity | FR-028, FR-035 |
| WP6 - Canon Storage and Patch-First Persistence | FR-029, FR-035 |
| WP7 - Persistence Completion and Lost Acknowledgement | FR-030, FR-031 |
| WP8 - Release Artifacts and Provider-Neutral Distribution | FR-022, FR-025, FR-034 |
| WP9 - MCP Administration and Diagnostics | FR-024, FR-032, FR-033 |
| WP10 - Integrated Runtime and Long-Campaign Acceptance | FR-031, FR-035, FR-036 |

## Decomposition Rationale

- WP1 was split so the portable contract can stabilize before compiler implementation, while vocabulary quality remains independently reviewable.
- WP3 was split between runtime retrieval and cross-operation context economics because instrumentation should measure several surfaces without bloating retrieval implementation.
- WP4 remains one bounded integration task because binding, gameplay entry, turn identity, and conversation-search demotion fail together at the host/runtime boundary.
- WP5 and WP6 remain separate because retrieval closure and persisted record normalization have different owners, migration risk, and tests.
- WP7 remains a focused persistence-recovery task, with metrics delegated to FR-031.
- WP8 was split between reusable acquisition/import and release packaging so provider contracts can be tested without publishing a release.
- WP9 was split between the service's safe read surface and a replaceable client/reporting implementation.
- WP10 was split into deterministic Canon-integrity regressions and final long-campaign/deployment acceptance so concrete failures shape test abstractions before the release gate.

## Planning Completion Boundary

This plan authorizes the existence and ordering of FR-022 through FR-036 only. It does not by itself select a current implementation task, change `VERSION`, move a release tag, publish an artifact, alter runtime behavior, or add campaign data. After each completed task, the next implementation begins only when the project owner explicitly selects one remaining Roadmapped revision.
