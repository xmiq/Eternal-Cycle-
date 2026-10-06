# Compiled Rule Store Retrieval

## Document Control

- **Owner:** portable compiled Rule Store request scope, controlled-query preparation, exact candidate matching, applicability, deterministic ranking, complete dependency closure, closure-aware packets, authorized durable retrieval and retrieval-stage failure boundaries.
- **Dependencies:** [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md), [Controlled Retrieval Vocabulary](CONTROLLED_RETRIEVAL_VOCABULARY.md), [Acquisition and Import](COMPILED_RULES_ACQUISITION_AND_IMPORT.md), and [existing retrieval/packet rules](RULE_COMPILATION_AND_RETRIEVAL.md).
- **Extensions:** FR-026 quality/cost acceptance and closure; FR-027 may bind an authorized Campaign to this machinery later.
- **Consumers:** Managed providers, portable retrieval implementations, diagnostics and conformance tests.
- **Boundary:** Repository Canon retrieval only; no Campaign Canon, provider/source-file access, compiler invocation, trust approval or implicit activation.

## Implementation State

[FR-026 execution plan](../../design/FR_026_EXECUTION_PLAN.md) separates the work. A prepares; B matches applicable candidates; C ranks; D expands dependencies; E builds closure-aware compact packets. F supplies authorized durable readback and a separately selected runtime path; the [F audit](../../design/audits/FR_026F_SQL_RUNTIME_RETRIEVAL_AUDIT.md) records SQL/reference equivalence. Existing legacy Campaign retrieval remains unchanged. G quality/cost and H/R2 closure are complete; [R2 acceptance](../../design/audits/FR_026H_R2_AUTHORITY_CONTEXT_ACCEPTANCE.md) records the corrected ordinary corpus. FR-027 still owns Campaign binding.

## Explicit Rule Store Scope

A request names **Ruleset ID + semantic artifact SHA-256** from format-1. This identifies one imported semantic artifact, not a provider locator, discovery tag, acquired byte hash, source path, Campaign ID or current clock. Same snippet/source IDs in different historical artifacts cannot establish shared storage ownership.

The configured service must authorize that exact scope before lookup. A syntactically valid scope is not authorization, proof of existence, trust, readiness or publication. Imported candidates do not become active simply because they are queried. Legacy active Rule Release lookup remains distinct from the explicitly selected compiled path; neither substitutes for the other. FR-027 owns stable Campaign binding.

## Bounded Request

The portable reference accepts `CompiledRuleRetrievalRequest` and prepares an immutable `PreparedCompiledRuleRetrievalRequest` through `CompiledRuleRetrieval.Prepare`:

- explicit scope, concrete operation and Campaign mode (mode is a selector, not Campaign state);
- optional concrete World Model ID;
- enabled module IDs and topics;
- a set of explicit query terms/phrases;
- optional exact required Rule Source IDs and snippet IDs;
- maximum estimated tokens, 1 through the existing normal ceiling of 8,000.

No preparation-tier, ranking-boost, top-K, provider, Campaign, source-root or connection-string knob is exposed. Preparation/readiness is service state, not an excuse to skip rules or assign relevance. An empty query set is valid for structural/mandatory/explicit requests; it does not request the entire corpus.

Reference safety bounds: at most 16 query entries; at most 32 entries in each module/topic/required-source/required-snippet set. Each raw query entry is at most 2,048 characters and normalizes to 1-256 characters, exactly the reviewed vocabulary bounds. Concrete IDs follow format-1 identifier syntax/limits; snippet IDs use the established source ID plus optional `#anchor`. Semantic SHA-256 uses the contract's uppercase 64-hex representation. These bounds prevent pathological input, not permission to omit mandatory dependency closures later. Caller wildcards are rejected; `*` in compiled source selectors retains its established applicability meaning.

## Normalization and Matching Boundary

Preparation calls the existing FR-024 `RuleRetrievalVocabulary.NormalizeTerm`: NFC, invariant lowercase, Unicode whitespace collapse/trim and final NFC. Phrases stay whole. Punctuation, hyphens and singular/plural distinctions remain unchanged. No stemming, token/ngram expansion, substring guessing, embeddings, semantic similarity or synonym generation occurs.

The query is an explicit set of terms/phrases, not free-form prose to be interpreted. Candidate lookup compares entire prepared entries to compiled term wording using ordinal equality. Thus `soul-bound` differs from `soul bound`, and `campaign canon` is not reduced to `campaign` or `canon`. The caller may formulate multiple entries; the engine does not infer them. A only normalizes and validates; B matches without another normalization algorithm.

Normalized query entries are deduplicated and ordered ordinally. Concrete identifier sets are also deduplicated/ordered ordinally but preserve identifier case and exact identity; normalization must not alter semantic source/snippet IDs. Applicability selector comparison remains owned by the existing selector contract, not query normalization. Prepared collections are private copies with read-only views, so later caller mutation cannot change an in-flight request. No time, process, culture or physical path contributes to it.

## Applicable Unranked Candidates

`CompiledRuleCandidateIndex.Create(artifact)` makes a private deep read-only copy and validates that snapshot using FR-022, once per index rather than on each query. It indexes existing term wording, not executable text, paths, anchors or source documents. No acquisition, approval, import, publication or service authorization is performed. Consuming services must still authorize the selected scope. Invalid/duplicate/unsorted artifact identities or metadata fail safely rather than dictionary overwrite or silent repair.

`FindCandidates(preparedRequest)` requires exact index/request Ruleset and semantic digest equality. It returns `CompiledRuleCandidateSet`: the prepared request, scope, read-only candidates and compact snippet counts. Each candidate carries its immutable format-1 snippet/source, all matching `(term, kind, weight)` associations, applicability satisfaction and independent flags for controlled vocabulary, required source, required snippet, Runtime Kernel, operation-scoped GM Runtime Procedure and always-include. Overlapping reasons never duplicate a snippet. Same wording under different kinds/concepts retains separate evidence and weights; no alias-origin label or inferred relationship expansion is added.

Explicit source identity selects all its applicable snippets; explicit snippet identity selects exactly that snippet. Missing, wrong-case, empty-source or inapplicable required identities fail as invalid requests. They are not fake vocabulary hits. Applicable Runtime Kernel snippets are mandatory without query hits; at least one must exist for runtime lookup. `gameplay.resolve` additionally requires the existing `gm-runtime-procedure` source, always-included, applicable and containing snippets. Missing/inapplicable mandatory runtime structure is inconsistent for retrieval even if the general interchange artifact is format-valid. This is not a new format-1 requirement, readiness acknowledgement or complete-packet guarantee.

## Applicability Reference Semantics

All source selectors gate all candidate reasons, including strong matches, Kernel and always-include. Comparisons are ordinal case-insensitive, distinct from exact case-sensitive artifact/source/snippet identity and ordinal normalized term matching. Selector entries are alternatives within a dimension; dimensions are conjunctive:

- Empty arrays are unrestricted. A scoped World requires a concrete selected World Model; `*` matches any such World, not an absent selection.
- Scoped module selectors require at least one explicitly enabled matching module. `*` matches any enabled module, never implicit enabling. This applies to scoped Core/Kernel sources too, not just the Optional Module layer.
- Mode and operation match the concrete request, or authored `*`.
- Scoped topics require a matching request topic, or authored `*`; an empty request does not satisfy a concrete topic scope. Always-include means query-independent inclusion, not a topic/filter bypass. The canonical Kernel and mandatory procedure are topic-free and therefore need no caller topic.

This follows FR-026's explicit applicability gates. The legacy unconditional Kernel eligibility and always-include topic bypass are deliberately not copied; the legacy implementation itself remains unchanged. No priority/preparation/readiness policy enters these filters. Preparation tier, priority, selectors and source dependencies remain available on the frozen source for later packages.

Candidates preserve the artifact's validated ordinal snippet order; matches preserve ordinal term/kind order. Neither order is relevance ranking. No weight aggregation, priority sorting, preparation bonus, dependency expansion, truncation, top-K or packet construction occurs. Counts distinguish vocabulary-matched, applicable matched, inapplicable matched and unmatched snippets without returning a rejected-candidate dump. Evidence is bounded by the valid artifact and A's prepared query, with no arbitrary cap that loses legitimate associations. Empty/no-match queries return only applicable mandatory/always-include/explicit roots, never the whole corpus.

## Deterministic Ranking

`CompiledRuleRanking.Rank(candidateSet)` consumes B's immutable applicable candidates, never raw artifacts, query prose or source files. It ranks every input candidate exactly once without rematching, adding candidates or filtering zero scores. Scope, counts, candidate identities, graph metadata and inclusion reasons remain unchanged.

The vocabulary score is the exact sum of reviewed weights over B's distinct matched `(term, kind)` associations. Same wording under different legitimate concepts contributes once per concept; multiple query entries contribute their actual associations without an extra coverage bonus. Canonical/alias/phrase origins are not in the runtime carrier and cannot create additional boosts. Identical duplicate keys, conflicting duplicate weights or out-of-order match evidence violate B's strict term/kind invariant and fail rather than inflate a score.

Ordering is complete and storage-independent:

1. Vocabulary score descending.
2. Authored source priority descending, only when vocabulary scores tie.
3. Stable snippet ID using ordinal ascending comparison, equivalent to format-1 neutral artifact order for tied candidates.

Priority retains the established larger-is-higher direction but cannot outweigh reviewed relevance. Source layer, preparation tier, dependencies, estimated tokens and readiness add no score. Mandatory Kernel, operation-scoped procedure, always-include and explicit required identities retain separate inclusion flags, not fake vocabulary contributions. With no genuine match their score is zero; with a match they receive exactly its reviewed weight. Zero is valid and never means omission. An ordinary positive match therefore precedes even a higher-priority zero-score mandatory root; later dependency/presentation/budget stages still owe all required content.

`RankedCompiledRuleCandidate` retains the original frozen candidate, its unchanged read-only matches/reasons, `VocabularyScore` and `Priority`. These fields and `SnippetId` explain every numeric contribution and tie without reading executable prose. `RankedCompiledRuleCandidateSet` retains the original input/scope/counts and a read-only ordered collection. There is no compiler-audit origin dump, second serialization format or player-facing explanation payload. Existing disclosure policy applies to service-side term evidence.

Scores use checked signed 64-bit integer addition: format-1 weights are positive integers 1-1000, and even an Int32-sized association list times 1000 fits Int64. No floating point, rounding, saturation or arbitrary match cap applies. Local malformed ranking metadata, duplicate candidate identity, invalid weights/priority/reasons and impossible arithmetic failure use the existing fixed inconsistent-artifact error without raw input or inner exceptions. B remains the artifact/scope/eligibility gate; C does not repeat full validation, authorization or query matching.

Cancellation checks precede ranking, run during candidate/evidence accumulation and surround the synchronous sort. Cancellation retains the supplied token and exposes no partial success; sort comparison uses only exact integers and ordinal identities. Repeated runs, changed culture and shuffled candidate enumeration produce the same ordering/evidence. Preparation urgency and the reviewed English term `preparation` remain different signals.

## Complete Artifact-Local Dependency Closure

`CompiledRuleDependencies.Expand(rankedCandidates)` consumes C's roots and the exact validated, frozen artifact retained internally by B. It does not accept a second public graph, rematch queries, rescore roots or load Rule Sources. Scope and root/source/snippet authority are checked before traversal; matching IDs or a claimed digest alone cannot substitute another graph. No full FR-022 validation or graph copy is repeated per request.

Format-1 dependencies name **Rule Source IDs**, not anchors. Selecting any snippet of A requires **all snippets** of each declared dependency source B, recursively. Selecting A alone does not select its own unmatched siblings. If A itself is required by another source, its full snippet set is included and independently ranked A snippets retain their root status. A required source with no snippets, a missing target, malformed/duplicate dependency or cycle fails; no other artifact/history/source-file fallback is permitted.

Dependencies inherit necessity, not query relevance. Query topics are not reapplied to prerequisites: a necessary rule need not match the topic that selected its dependent. World Model, enabled module, Campaign mode and operation compatibility still apply using B's selector policy, including concrete world/module selection for wildcards. An incompatible prerequisite makes the complete request fail; it is not silently dropped or forced into another world/mode. Mandatory Kernel, operation-scoped procedure, explicit and always-include roots expand by exactly the same rules, without passing those root flags to dependencies.

`CompiledRuleDependencyClosure.Roots` preserves C's exact root ranking and original root objects, including scores, contributions, reasons, authored priorities and B applicability evidence. Each root has a complete read-only `Prerequisites` list for later whole-closure admission. `Members` is the deduplicated union in prerequisite-first source order. A member retains its frozen source/snippet, optional original `Root`, and sorted `RequiredByRuleSourceIds`; `IsRoot` and `IsDependency` can both be true. Dependency-only members have zero vocabulary score, no query/root reasons and no fabricated contributions. Presence is explained by direct parents, not relevance.

Ordering is explicit: traverse distinct root sources in C rank order; visit dependency IDs ordinally; finish each source after its prerequisites; emit that source's selected/full snippets by ordinal snippet ID. Root ranking remains separate from this topological presentation. Shared members appear once. Every reachable direct parent is retained ordinally, including parents discovered after a shared member was visited. No arbitrary first-parent explanation or enumeration of all graph paths occurs. Per-root prerequisite lists follow the union's topological order and share member objects; roots from one source share their prerequisite list. E can consume these complete groups without resolving closure again.

Traversal is iterative and cycle-safe. Metadata support is bounded by reachable direct edges, union members and explicit per-root membership, not the potentially exponential number of paths. Parent arrays are shared per source; prerequisite arrays are shared per root source. No arbitrary graph cap truncates required content. Artifact/acquisition bounds remain binding (the acquisition default is 16 MiB, not an unconditional retrieval-node limit). Cancellation checks cover indexing, roots, graph traversal, evidence and group construction; cancellation exposes no partial success. The token budget is carried but does not limit D membership or assign costs. E owns admission/truncation and final compact packets.

## Closure-Aware Budgets and Compact Packets

`CompiledRulePackets.Build(closure)` consumes D's immutable complete groups, without rematching, rescoring, resolving dependencies again or reading source files. Required roots are those carrying Kernel, operation-scoped GM Procedure, explicit required-source/required-snippet or always-include reasons. Reserve their complete deduplicated union first, in their relative C order. If it exceeds the requested budget, fail with `RULE_RETRIEVAL_BUDGET_INSUFFICIENT`; never omit, truncate or exceed the ceiling. Reserving structural requirements does not change C's relevance ranking or fabricate a score.

Then consider optional roots in C order. Compute each complete closure's incremental cost over members already admitted. Commit all newly needed members only if the entire increment fits, including equality. Otherwise admit none, record budget exclusion and continue to later roots. Shared/transitive/diamond prerequisites are charged once; an independently ranked member already present as a dependency keeps its original C evidence and costs zero if its whole closure is already present. No score-per-token optimization, preparation bonus or full-corpus fallback exists.

The budget is **estimated executable rule tokens**, exactly the sum of each unique emitted format-1 snippet's positive `estimatedTokens`. Compilation uses the existing deterministic `max(1, (UTF8 byte count + 2) / 3)` estimator on normalized executable text; E consumes approved stored estimates instead of retokenizing. Dependencies and headings within snippet content count; source IDs, envelope JSON, double-LF section separators, diagnostics and explanation metadata do not. This preserves legacy accounting, not a claim about exact model-provider tokenization or the whole approximately 20K context target. Costs use checked Int64 accumulation; admitted totals remain within A's 1-8,000 budget.

`CompiledRulePacket` preserves the existing compact format-1 source-grouped text shape: `PacketFormatVersion`, one `Scope` (Ruleset ID/semantic digest), `RepositoryVersion`, immutable `SourceIdentity`, `EstimatedTokens`, `MaximumEstimatedTokens`, and `Rules` sections containing only `RuleSourceId`/`Content`. The legacy envelope additionally requires Campaign/World/Ruleset runtime binding and a published Rule Release ID, which this explicit imported-artifact stage cannot truthfully supply. E uses artifact identity rather than inventing those fields; it does not replace the legacy formatter or introduce another artifact format. F owns authorized durable/runtime adaptation.

Filter D's union by admitted identities, preserving its prerequisite-first source order and ordinal snippet order within each source; group contiguous source text with double LF exactly as the existing compact formatter does. Root decisions remain separately in C ranking order. No packet timestamp, random ID, per-snippet provenance, vocabulary audit or acquisition evidence enters model context. A packet is a Derived view, not another rule authority or readiness acknowledgement.

`CompiledRulePacketResult` retains service-only evidence: original input, read-only emitted members, C root decisions and totals. Each decision retains the original score, reviewed contributions, priority, identity and inclusion flags, plus required/selected state, incremental cost and remaining budget at its admission phase. Required decisions are evaluated in the reservation phase; optional decisions in the later relevance phase. The fixed exclusion code `RULE_RETRIEVAL_CLOSURE_EXCEEDS_REMAINING_BUDGET` cannot be confused with B's inapplicable/nonmatching counts or a D failure. Member support retains all D direct parents actually emitted; excluded parents do not pretend to support packet content. Full pre-budget support remains in the original D input. No all-path expansion occurs.

Totals expose requested/used/remaining/required estimates, selected roots, unique members, dependency-only members and excluded roots. Nested collections are read-only; the frozen artifact remains unchanged. Local identity, supplied membership, complete dependency-source membership and presentation checks reject mixed/truncated/forged groups without a second graph traversal or full artifact revalidation. Cancellation during validation, admission or presentation returns no partial packet. A D failure supplies no usable input; the frozen pre-R2 artifact's gameplay `save` and combined-query incompatibility remains a dependency failure, not an E budget decision. The corrected [complete ordinary authority](../persistence/PERSISTENCE_AUTHORITY.md) makes both requests dependency-safe without changing A-E; optional complete roots may still be excluded by budget.

## Result and Failure Boundary

A returns a prepared request, B unranked candidates, C ranked candidates/explanations, D complete dependency closure and E a compact packet plus service evidence; none acknowledges readiness. Expected failures use `CompiledRuleRetrievalException`, a fixed safe code/category/message with no echoed caller/artifact input or raw inner exception. B uses invalid-request for scope/required-identity mismatch and inconsistent-artifact for malformed input or missing applicable mandatory structure; C uses inconsistent-artifact for local ranking invariant violations. D uses invalid-request for scope mismatch, inconsistent-artifact for absent/mixed roots or graph ownership, and dependency-failed for missing, empty, cyclic, malformed or incompatible reachable dependencies. E uses invalid-request for scope mismatch, inconsistent-artifact for local membership/estimate violations and insufficient-packet-budget for required overflow. No partial successful closure or packet escapes. F uses unavailable/inconsistent artifact and storage categories plus bounded authorization, migration, publication/activation and host-confirmation failures at its durable boundary.

Cancellation remains `OperationCanceledException` with the supplied token; it is never empty results, storage failure or partial success. A checks during preparation; B checks around snapshot validation, during index construction, selectors, identities, term hits, candidate selection and before returning. The existing full FR-022 validation call is synchronous and checks cancellation at its boundary. No partial index/set escapes. Read-only copies prevent later caller mutation; callers must not concurrently mutate assembly DTOs while copying them. Default string representations are fixed type names. Candidate source/content and prepared request are excluded from implicit JSON diagnostics; match evidence is service-facing, not ordinary player context.

## Durable Runtime Boundary

Authorize the explicit Ruleset/artifact scope before existence or content lookup. Administrative import/read, informed publication approval, activation approval and runtime retrieval are separate privileges. The reference's runtime opt-in is a deployment grant scoped to its configured Ruleset, not an artifact-provided permission or caller-supplied approval flag. Multi-user deployments must authenticate and establish grants before invoking this service; the single-user reference is not a multi-user authentication system.

Import remains a complete durable candidate only. Separate publication makes it eligible for activation; separate atomic activation selects one compiled artifact per Ruleset. A request must name that exact active digest. Historical candidates remain administratively readable, not substituted or searched as a mixed corpus. Legacy Campaign-pinned/active release resolution remains authoritative for the legacy entry path. Compiled retrieval is selected explicitly, not because an import exists. Corruption, missing selection or incompatible closure never falls back to legacy or another artifact.

The reference reuses migration 011's complete FR-025 readback: ordered artifact-owned sources/snippets and dependency edges reconstruct format 1, verify all normative fields against retained exact bytes, and check byte hash, semantic digest and ownership. Selection, publication, activation and reconstruction use one serializable transaction. SQL supplies data only; A-E alone prepare, match, filter, rank, expand closure and budget. No provider, original source file or compiler is required at runtime. The reference re-verifies per read without a global mutable cache; the [G audit](../../design/audits/FR_026G_RETRIEVAL_QUALITY_COST_AUDIT.md) records local cost evidence without production-performance acceptance.

A published imported artifact is already complete, including estimates and terms. It need not re-enter source acquisition/preparation or invent Pending rows. Missing/corrupt imported data fails closed. Existing source-based progressive Pending/priority behavior remains unchanged. Successful selected-closure retrieval establishes rule-context readiness, not whole-game GameplayReady, Campaign availability or full-ruleset readiness. For gameplay.resolve, durable host acknowledgment must match this artifact's canonical Host Bootstrap hash and be UserConfirmed or Verified; A-E still require Kernel/procedure closure and budget.

Normal delivery returns E's compact packet only. Scores, matches, dependency support, exclusions, provenance and verification evidence remain service-side. Expected failures expose fixed bounded categories; cancellation returns no partial packet. Administrative/debug disclosure needs its own authorized surface.

An opt-in additive reference migration is required only because 011 has no imported-artifact publication/active association. It adds no term index, reinterpreted legacy release, artifact field or Campaign binding and does not prescribe SQL for other providers. See [reference deployment guidance](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md#explicit-compiled-artifact-runtime).

## Remaining Acceptance

The [requirement matrix](../../design/FR_026_EXECUTION_PLAN.md#normative-requirement-and-gap-matrix) owns package/evidence routing. Applicability gates relevance and always-include; mandatory Kernel/operation-scoped GM Procedure cannot be omitted by query choice. Reviewed concept ID and weight survive matching; canonical/alias/phrase origin is compiler-only and cannot be invented from runtime `kind`.

Direct ranking, dependency reasons and prerequisite presentation are distinct. A selected root requires its complete applicable artifact-local transitive source closure. Whole-closure budgeting must fail for missing required structure rather than silently truncate. Unknown/unprepared required rules remain failure/Pending, not guessed Canon. Normal model-facing delivery reuses the [existing compact packet](RULE_COMPILATION_AND_RETRIEVAL.md#retrieval), not artifact JSON or compiler audit origins. Broad/no-match requests never trigger a full-corpus fallback. Runtime evidence and storage checks must be bounded, parameterized and separate from normal player context.
