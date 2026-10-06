# FR-026H Dependency Granularity Investigation

## Executive Finding

**Later R1 review:** the [clause-complete authority review](FR_026H_R1_AUTHORITY_PARTITION_REVIEW.md) finds the 2,572-unit projection omits required obligations. Its conservative routine source costs 4,903 and does not fit all supported closures at 8K; R2 is not ready. This investigation's measurements and provisional recommendation remain historical evidence, not a certified partition or implementation authorization.

**Investigation complete; architectural correction not approved or implemented. FR-026H remains blocked.** This supplements the [blocked integrated acceptance audit](FR_026_COMPILED_RULE_RETRIEVAL_AUDIT.md), not its historical test results. The [blocking question](../UNRESOLVED_QUESTIONS.md#blocking) remains active pending maintainer review.

The defect is a mismatch between the canonical Rule Source boundaries and the executable dependency contract. Gameplay-eligible context-assembly snippets depend on an authority source whose operation selector excludes gameplay. Making that whole source gameplay-eligible still cannot admit even one dependent context root within 8K: its complete prerequisite plus mandatory gameplay context costs 8,412 before the root itself. Source-level selectors and dependencies cannot express the smaller, claim-specific authority obligations evident in the prose.

FR-026D correctly fails closed; SQL reproduces the reference failure. Neither SQL nor ranking nor the estimator is the established cause. Canonical retrieval of supported guidance remains unacceptable even though implementation parity and defensive tests pass.

**Recommendation for approval:** split authority into semantically bounded routine and exceptional/governance sources, with selective partitioning of context assembly where dependency obligations differ. Preserve format-1 whole-source dependency semantics, authority, complete closures, existing ranking and budgets. This is alternative E with selective D, not selector widening in isolation. Do not apply one routine prerequisite indiscriminately to adoption, conflict correction or governance material. If an authoritative partition cannot preserve all obligations economically, return for an explicit versioned finer-dependency design; do not reinterpret format 1.

## Recovered Repository and Scope

- Branch/HEAD/upstream: `main`, `d0d70e9a755d5bb3860dad86611f7c89e2fde734`, local upstream divergence `0/0`.
- VERSION: `1.0.0`. Tag objects unchanged: `v1.0.0` at `6938bd2f9e965faaeb1c612b1dd765237cadc8a8`; `v1.1.0-rc` at `4eb2583027d316a962fd49fe318781a0e060183e`.
- Preserved the eight unstaged blocked-H documentation files: seven tracked modifications and the untracked integrated audit. No staged content, partial production implementation, commit or push was found.
- This pass adds this investigation and narrowly links its recommendation from those documents. No canonical source, manifest, vocabulary, schema, production code, executable fixture, VERSION or tag is changed. Temporary analytical code is removed after use.
- A-G remain complete; H blocked; FR-026 selected/incomplete; FR-027 and FR-031 unselected. The corrective checkpoint names below are not new Future Revisions or authorization to execute them.

## Failure Chain

1. An explicit artifact-scoped `gameplay.resolve` request is prepared by A. B finds reviewed vocabulary hits and applicable context-assembly roots; C ranks them deterministically.
2. The [manifest](../../docs/rules/rule-source-manifest.json) gives `core-context-assembly` a source dependency on `core-persistence-authority`.
3. [Format 1](../../docs/rules/COMPILED_RULES_ARTIFACT.md#rule-sources) carries only source-level `dependencyRuleSourceIds`. Compiler stages preserve these declarations; there is no snippet dependency or snippet applicability override.
4. [D's contract](../../docs/rules/COMPILED_RULE_STORE_RETRIEVAL.md#complete-artifact-local-dependency-closure) requires all snippets of a dependency source and its transitive dependencies. Its applicability checks include operation, World, module and mode; topic filtering cannot prune prerequisites.
5. Authority admits `context.assemble`, `persistence.read` and `persistence.commit`, not `gameplay.resolve`. The request has NORMAL mode and no World/module restriction. Operation is the failing selector.
6. D returns `RULE_RETRIEVAL_DEPENDENCY_FAILED` before E can discard an optional root for budget. No partial packet is emitted. F's verified SQL readback feeds the same A-E code, so it fails identically.

This source edge is executable, not merely a documentation citation. Removing it, ignoring applicability for dependencies, or retrieving only matched authority sections would change the accepted contract. Raising the budget alone cannot repair the selector failure.

## Semantic Dependency Analysis

### Reading Basis and Boundaries

Read the complete [context-assembly contract](../../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md) and [persistence authority](../../docs/persistence/PERSISTENCE_AUTHORITY.md), plus the actual other matched source sections, Runtime Kernel, GM Runtime Procedure, format/compiler/vocabulary/storage/retrieval contracts and implementations. The following map is an authored investigation, not a new normative rule or generated vocabulary.

Authority is about **campaign-fact precedence**, not the SQL engine, save transaction implementation or schema. Its real subdivisions are claim scope, authority layers, visibility/certainty/agency, valid integration, conflict correction and version/retcon governance. A split into "reasoning versus storage mechanics" is not supported: the authority document expressly leaves save mechanics and schemas to specialist owners.

The map identifies authority needed when a snippet's substantive behavior is executed. "No independent requirement" does not authorize ignoring authority when another included rule requires it. Conditional branches require their full relevant procedure before execution, or must stop and route to it; merely linking an omitted procedure is not execution context. The [Runtime Kernel](../../docs/rules/RUNTIME_RULE_KERNEL.md) and [GM procedure](../../docs/rules/GM_RUNTIME_PROCEDURE.md) already supply baseline ownership, no-guessing, read/write gates and player agency, but not the complete historical/session claim hierarchy.

Context assembly also names Campaign State Model, Canonical Data Ownership, Save Update Protocol, Persistence Validation and specialist visual/memory contracts. This authority map does **not** prove that those other prerequisites are fully represented in the present ten-source corpus. Corrective design must check their material obligations rather than asserting that one smaller authority source completes every specialist procedure.

### Authority Reference Bundles

Anchors below are exact sections of Persistence Authority. Bundle letters are shorthand for this analysis only; they are not compiler identities or proposed dependency syntax. Estimates sum existing compiled snippets.

| Bundle | Exact authority sections | Units | Obligation |
| --- | --- | ---: | --- |
| O | `core-rule`, `authority-is-claim-specific` | 444 | Claim-specific precedence; lower recency cannot defeat higher provenance. |
| F | `historical-record`, `current-campaign-state`, `current-session`, `current-narration` | 1,379 | Distinguish durable history/current state, valid pending session events, drafts and presentation. |
| V | `authority-does-not-equal-visibility` | 208 | Authority does not grant disclosure or Character Knowledge. |
| U | `authority-does-not-equal-certainty` | 124 | Preserve authoritative unknowns and uncertainty. |
| A | `authority-does-not-equal-actor-control` | 203 | Remembered plans and conditions do not select future choices. |
| T | `valid-downward-updates` | 214 | Integrate established changes with source, scope, affected records and unresolved conflicts. |
| G | `repository-canon`, `campaign-canon`, `repository-revision-changes`, `authorized-retcons`, `valid-upward-proposals` | 1,412 | Adoption, migration, retcon and proposed higher-authority changes, not routine permission to rewrite canon. |
| C | `conflict-classification-procedure` | 462 | Classify incompatible claims and route correction; resume only coherently. |
| R | `rules-authority-and-persistence-authority` | 407 | Separate reusable-rule authority, adopted Living Codex/profile and campaign fact authority. |
| S | `safeguards` | 288 | Cross-cutting protections, including governance; must be accounted for clause by clause. |

O+F+V+U+A+T totals **2,572**. This is a conservative routine-authority candidate, not a certified mathematical minimum or a finished authoritative source. Purpose/scope framing, the relevant S clauses and Repository/Campaign Canon boundaries must remain explicit in an approved partition; existing Kernel coverage must be checked rather than assumed. Added framing or further prerequisites have real cost.

### Every Context-Assembly Snippet

All 32 entries are covered: the unanchored H1 plus 31 H2 snippets; H3 material stays inside its H2. IDs are `core-context-assembly` plus the displayed anchor. "Yes" means a real semantic dependence on the listed authority obligations, not on all 24 present authority snippets.

| Anchor / snippet | Units | Requires authority? | Exact reference bundles and reason |
| --- | ---: | --- | --- |
| Unanchored H1 | 16 | No independent requirement | Title only; no executable claim. |
| `purpose` | 171 | Yes | O/F/T: final narration is not validated integration; reads and automatic writes connect a valid turn. |
| `document-control` | 392 | No independent requirement | Ownership/dependency/navigation metadata; named specialist requirements still govern their procedures. |
| `runtime-boundary` | 250 | No independent requirement | External-host conformance and observation limits, not claim precedence. |
| `core-invariants` | 611 | Yes | O/F/V/U/A/T: caches own no canon, read/write evidence, disclosure and player-input boundaries. |
| `gameplay-turn-state-machine` | 645 | Yes | O/F/V/U/A/T: pending resolution versus committed fact; player choice cannot arise from tool continuation. |
| `persistence-target-resolution-gate` | 471 | Yes | O/F; G when selecting/adopting a rules profile or resolving migration. Save Index/configuration owns target discovery, not authority prose. |
| `context-assembly-layer` | 491 | Yes | O/F/V/U: projections navigate owners; Rule Context and Campaign Canon stay distinct. R when interpreting reusable/Codex authority. |
| `relevance-selection` | 403 | Yes | O/F/V/U/A: select materially relevant claims, histories, uncertainty and actor conditions without granting recall or control. |
| `current-scene-context` | 546 | Yes | O/F/V/U/A: perspective, unresolved player choice, owner/version evidence and visibility-limited historical recall. |
| `purpose-specific-visual-context` | 333 | Yes | O/F/V/U; T for explicit adoption/correction. Unspecified rendering and an image do not establish canon. |
| `hierarchical-context` | 497 | Yes | O/F/V/U: retrieval hierarchy is not authority; derived summaries cannot replace owner history or grant information. |
| `freshness` | 239 | Yes | O/F/T: repair derived context from authoritative claims, never inverse overwrite. C for an unresolved substantive contradiction. |
| `mandatory-read-gate` | 382 | Yes | O/F/V/U: verify claim owner, supersession, audience and uncertainty. C when material records conflict. |
| `resolution-and-affected-set-gate` | 330 | Yes | O/F/T/V/U/A: owner-routed world/actor consequences and discoveries; explicit no-op versus durable change. |
| `automatic-persistence-gate` | 733 | Yes | O/F/T/U: drafts and intended saves are not validated fact. Transaction/validation details remain specialist-owned. |
| `player-visible-persistence-status` | 677 | Yes | O/F/T/U: commit/authority evidence, not narration or intention, controls a truthful status. |
| `manual-persistence-commands` | 576 | Yes | O/F/T/V/U: save/status/retry are idempotent control work, not replayed events, new player turns or new disclosure authority. |
| `canonical-change-proof-and-unchanged-save-detector` | 262 | Yes | O/F/T/U: version/readback and actual owned change establish completion; an absent change is failure. |
| `canon-before-derived-context` | 149 | Yes | O/F/T: save canonical owners before updating summaries. |
| `no-change-turns` | 276 | Yes | O/F/T; V/U for representation: proven empty Affected Set, not false promotion of incidental detail. |
| `failure-and-retry` | 380 | Yes | O/F/T/U: pending versus failed/committed change, same interaction identity, no invented recovery. C if recovery reveals an actual claim conflict. |
| `next-turn-verification` | 187 | Yes | O/F/T: later reads must see the established prior change, not unsaved narrative memory. |
| `session-start-and-context-reset` | 233 | Yes | O/F/V/U: recover authoritative campaign/session state; G if adopting or migrating a profile, not merely reloading it. |
| `specialist-integrations` | 592 | Yes | O/F/V/U/A; R for reusable-rule/Codex interpretation. Visual, memory, development and world owner contracts are additional requirements. |
| `existing-campaign-adoption` | 245 | Yes | O/F/T/G; C when contradictions occur. Migration/provenance obligations cannot be replaced by the routine bundle. |
| `development-context-and-auditability` | 178 | Yes | O/F/V/U: authorized technical inspection does not turn GM information into player knowledge or invent facts. |
| `storage-neutral-logical-guidance` | 476 | No independent requirement | Illustrative derived-cache DDL does not define fact authority or authorize campaign SQL; baseline owner rules still apply if implemented. |
| `regression-cases` | 2,016 | Uncertain as one runtime unit | Mixed offline conformance cases exercise O/F/V/U/A/T and specialist host/profile guarantees. Preserve all cases; review clause ownership before making them one executable prerequisite. |
| `acceptance-criteria` | 602 | Yes | O/F/V/U/A/T: requirements for conformance, not a separate transaction engine. |
| `safeguards` | 194 | Yes | O/F/V/U/A/T: non-owner summaries, read/write gates, pending failures, agency and privacy. |
| `related-documents` | 341 | No independent requirement | Navigation cannot replace substantive required rule text. |

The uncertain row is deliberate: conformance examples spanning multiple actions are not proof of one routine gameplay dependency signature. Further authoring review, not a cost-driven classification, must resolve it. "No independent requirement" sections may move to non-runtime companion documentation without losing their canonical role.

### Reverse Authority Map

All 24 existing authority snippets are accounted for below. The routine references are the six fixture obligations; exceptional references cover the other context sections above. No section is designated disposable canon.

| Authority anchor / snippet | Units | Needed for context assembly? | Semantic disposition |
| --- | ---: | --- | --- |
| Unanchored H1 | 8 | Framing only | Preserve a correct title for any new source; not a separate authority rule. |
| `purpose` | 177 | Scope framing | Defines campaign-fact versus rule authority; new standalone routine source needs this distinction, not necessarily this whole snippet as a dependency. |
| `core-rule` | 176 | Routine, O | All authority-sensitive reads, context and integration. |
| `authority-is-claim-specific` | 268 | Routine, O | Compare exact claim/scope/time, including valid new events rather than historical overwrite. |
| `the-authority-layers` | 8 | Heading only | No independent behavior. |
| `repository-canon` | 415 | Governance and interpretation, G | Active profile, adoption and repository conflicts; baseline boundary must also remain explicit in routine framing. |
| `campaign-canon` | 384 | Governance and interpretation, G | Adopted premises/control agreements/retcons; ordinary current-state use is not permission to change these. |
| `historical-record` | 318 | Routine, F | Historical callbacks, discoveries and integration remain traceable; beliefs are not facts. |
| `current-campaign-state` | 426 | Routine, F | Latest integrated owner state cannot erase valid history or unintegrated session events. |
| `current-session` | 322 | Routine, F | Valid pending outcomes versus drafts; incomplete recording does not discard the event. |
| `current-narration` | 313 | Routine, F | Presentation cannot overrule higher claims or defend continuity errors. |
| `rules-authority-and-persistence-authority` | 407 | Conditional, R | Rule Context versus Campaign Canon and Living Codex adoption/version boundaries. Not a blanket prerequisite for every cache/status section. |
| `authority-does-not-equal-visibility` | 208 | Routine where disclosure, V | Scene, knowledge, visual, read, debug and integration privacy. |
| `authority-does-not-equal-certainty` | 124 | Routine where gaps, U | Read gates, unknowns, pending/failure reporting and uncertain world state. |
| `authority-does-not-equal-actor-control` | 203 | Routine where actors, A | Player choice, scene and world/actor consequences, not a save command's technical implementation. |
| `valid-downward-updates` | 214 | Routine integration, T | Source/scope/affected records; no silent overwrite. Migration examples add exceptional obligations. |
| `valid-upward-proposals` | 184 | Governance branch, G | Audit/playtest evidence can propose higher-authority change, not enact it. |
| `conflict-classification-procedure` | 462 | Conflict branch, C | Read/freshness/retry/adoption contradictions require full classification and correction routing before disputed play resumes. |
| `repository-revision-changes` | 207 | Adoption/migration branch, G | Session boot/profile changes and existing-campaign adoption; not automatic every-turn migration. |
| `authorized-retcons` | 222 | Correction branch, G | Deliberate authorized replacement with provenance/agency consequences; not normal new events. |
| `worked-examples` | 696 | Interpretive/conformance evidence | Loss, unsaved event, false belief, profile migration and contradictory secret illustrate retained obligations; not all examples are executable prerequisites of every runtime root. |
| `safeguards` | 288 | Routine and exceptional, S | Map every clause to routine protections, governance or repository boundary. Do not silently drop the numerical-change, history, uncertainty or no-populated-record safeguards. |
| `scope-boundaries` | 182 | Scope framing | Explicitly does not own schemas, transaction stages or specialist visibility; approved split must preserve those non-ownership limits. |
| `related-documents` | 173 | Navigation only | Retain links, not an executable substitute for the referenced procedure. |

The minimum is **operation/branch dependent**, not one global subset shared by all 32 context snippets. Routine authority plus selected context may suffice for one request while adoption/conflict/Codex integration needs further rules. The candidate bundles expose these distinctions; certifying a final minimal complete closure requires maintainer-reviewed clause coverage and the actual authored source graph. It has not been certified in this investigation.

## Six Fixture Analysis

All six use explicit canonical artifact identity, `gameplay.resolve`, NORMAL, no World/module selection, and the fixture's explicit topic union. Budgets tested: 8,000 and 3,000. Mandatory context is the 11 Runtime Kernel/GM-procedure snippets costing 2,027, including actual-new-input/yield and commit-before-delivery safeguards. Scores below are the unchanged vocabulary contribution from C; context roots also retain source priority 300. These are rule queries within one operation, not requests to run several operations simultaneously.

### Save

- Fixture `persistence-save-failure`, query `save`: `core-context-assembly#manual-persistence-commands`, score 600, cost 576.
- Chain: manual-command root -> complete authority source -> incompatible operation. Widened closure plus required floor costs 8,988.
- Intended guidance: `save`, `save status`, `retry save` flush/inspect/retry the same interaction without duplicating effects. This is a deliberate supported override/debug behavior, not a new player action or permission to bypass the automatic save gate. It may remain gameplay-retrievable under that meaning; no blanket exposure of admin tools, storage credentials or write permissions follows.
- Required authority is claim scope, durable/pending/draft distinction, integration, visibility and honest uncertainty (O/F/T/V/U); actor control remains supplied by the mandatory player-turn contract.

### Combined Four-Term Query

- Fixture `broad-known-failure`, terms `fighting`, `conflict`, `save`, `preparation`: the same manual root (600), four Development roots (1,300 each), three Evolution roots (600 each).
- Development roots: `contextual-capability-assessment` (665), `power-preparation-and-victory` (293), `why-there-is-no-universal-character-level` (394), `worked-examples` (1,367). These cover contextual capability and earned/prepared development, not guaranteed victory.
- Evolution roots: `gradual-evolution` (316), `route-readiness` (636), `the-evolution-transition` (451). Preparation/readiness associations are supported; this query does not request an unauthorized forced evolution.
- Same failed manual-root chain. Original non-authority union costs 6,725. Widening also admits five 600-point authority hits for `conflict`: `authority-is-claim-specific`, `conflict-classification-procedure`, `core-rule`, `current-narration`, `purpose`; full widened union costs 13,110.
- Intended guidance combines relevant development/preparation and save control; complete optional-hit inclusion is not guaranteed under a budget. Actual necessary guidance must be asserted, not inferred from a successful packet containing only other hits.

### GM Adjudication

- Fixture `gameplay-procedure`, query `gm adjudication`: context `gameplay-turn-state-machine` (800, 645) plus already-required GM `efficient-execution` (147), `gameplay-interaction-lifecycle` (422), `purpose` (124), each score 800.
- State-machine root -> authority -> operation failure. Full widened union 9,057; the matched mandatory GM rules are not charged twice.
- Legitimate gameplay lifecycle: read, resolve once, determine changes, persist/verify, deliver and yield. O/F/V/U/A/T supplies the fact/agency boundaries, not a new lifecycle algorithm.

### Player Agency

- Fixture `player-agency`, query `wait for player input`: context `core-invariants` (611) and `gameplay-turn-state-machine` (645), both score 800. Additional 800-point hits are mandatory GM lifecycle/one-input and Kernel invariants.
- Both context roots have the same failed edge. Widening adds authority `authority-does-not-equal-actor-control` (203, score 800); full union 9,668. Single-context-root floor/whole-authority costs are 9,023 and 9,057.
- Legitimate requirement: tool/retry work cannot invent the player's next choice. O/F/V/U/A/T, especially A, is relevant. Mandatory baseline agency alone does not make losing requested detailed context guidance acceptable.

### GM Secrets Visibility

- Fixture `knowledge-visibility`, query `gm secrets`: context `current-scene-context` (546), `purpose-specific-visual-context` (333), `relevance-selection` (403), all score 800; World Engine `architectural-role` (144, score 800).
- Each context root -> authority -> operation failure. Their separate floor/whole-authority costs are 8,958 / 8,745 / 8,815. Widening adds authority visibility (208, 800); full union 9,838.
- Legitimate requirement: GM access/private facts, player knowledge and canonical depiction remain distinct; reference paths do not grant recall/disclosure. O/F/V/U are essential; scene/relevance also needs actor-choice boundaries, visual adoption needs T. Visibility's specialist owner still governs detailed policy.

### Population and Ecology

- Fixture `world-state-phrase`, query `population and ecology`: context `resolution-and-affected-set-gate` (330, 800), World Engine `core-domains` (377, 800), `shared-state-model` (82, 800).
- Affected-set root -> authority -> operation failure. That root plus floor/whole-authority costs 8,742; full union including world roots is 9,201.
- Legitimate requirement: resolved world/actor consequences have durable owners and an explicit Affected Set. O/F/T/V/U/A is required; ecology is not an excuse to persist speculation as certain Canon or choose a player action.

## Current Cost Structure and Measurements

Estimates use the existing per-snippet normalized UTF-8 estimator, `max(1, (byteCount + 2) / 3)` with integer division. The whole frozen artifact totals **56,398**. Authority has 24 snippets / **6,385**; context assembly has 32 / **13,894**. Gameplay floor **2,027** and context-Kernel floor **1,010** are different operations, not interchangeable savings. Gameplay floor and authority are disjoint: **2,027 + 6,385 = 8,412**.

No external-tokenizer equivalence is claimed. Packet envelopes/separators, Campaign Canon, conversation and other runtime context are excluded. Fractions below use the historical whole-corpus denominator, not a predicted cost of a reauthored corpus.

### Executed Sensitivity Models

A disposable local .NET runner loaded/compiled the existing canonical fixture, checked exact prose equivalence to current sources, and asserted its semantic digest and serialized hash. It ran the real A-E engine on three in-memory states for six queries at two budgets: **36 invocations**. These states were current, A (only add gameplay to authority operations), and C (only remove context's dependency). Hypothetical DTOs were re-digested and passed the existing format-1 validator; no candidate bytes were emitted, imported or published.

| Case | A: complete ranked union | A: 8K used / exclusions | A: 3K used / exclusions | C: 8K used / exclusions | C: 3K used / exclusions |
| --- | ---: | ---: | ---: | ---: | ---: |
| save | 8,988 | 2,027 / 1 | 2,027 / 1 | 2,603 / 0 | 2,603 / 0 |
| combined | 13,110 | 7,545 / 1 | 2,985 / 11 | 6,725 / 0 | 2,985 / 6 |
| GM adjudication | 9,057 | 2,027 / 1 | 2,027 / 1 | 2,672 / 0 | 2,672 / 0 |
| agency | 9,668 | 2,230 / 2 | 2,230 / 2 | 3,283 / 0 | 2,638 / 1 |
| visibility | 9,838 | 2,379 / 3 | 2,379 / 3 | 3,453 / 0 | 2,906 / 2 |
| ecology | 9,201 | 2,486 / 1 | 2,486 / 1 | 2,816 / 0 | 2,816 / 0 |

All 12 current-state invocations fail at D. All A packets exclude **every requested context-assembly root**, at both budgets. A's success status is therefore not a repair. C's smaller results omit needed authority; they are unsafe cost lower bounds, not accepted packets. Widening creates extra optional authority hits in combined/agency/visibility, changing query topology in addition to dependency compatibility.

B (budget increase) still needs an operation correction first. With A, complete representative unions range **8,988-13,110 / 56,398**, rather than a universal new minimum of 8,412. The 8,412 floor is before any dependent root; the cheapest blocked context root already needs 8,742. These larger ceilings were calculated, not executed as supported above-8K requests. They introduce all authority examples/navigation/governance regardless of the ordinary branch and defer the granularity problem into FR-031's whole-context accounting.

### Fixed-Root Partition and Finer-Dependency Projections

Separately, the runner performed **24 arithmetic admission projections** (two candidate authority sets, six queries, two budgets), replaying required-first, whole-closure admission on the unchanged root set. These are **not** fully reauthored format-1 artifacts or format-2 prototypes. They exclude new direct authority hits, new source framing and additional specialist edges. No source partition has been validated by these projections.

E+D candidate: O/F/V/U/A/T, 2,572, shared across the routine roots. F sensitivity: remove A from the manual-root prerequisite; remove A/T from the visual-root prerequisite; remove T from scene/relevance prerequisites. Those exact-section differences test possible savings, not a complete general finer-dependency contract. Other roots retain 2,572. Their semantic acceptability is subject to the reviewed scope map.

| Case | E+D full fixed-root union | E+D 8K used | E+D 3K used | F targeted 8K used | Requested context roots in E+D 8K |
| --- | ---: | ---: | ---: | ---: | ---: |
| save | 5,175 | 5,175 | 2,027 | 4,972 | 1 |
| combined | 9,297 | 7,894 | 2,985 | 7,691 | 1 |
| GM adjudication | 5,244 | 5,244 | 2,027 | 5,244 | 1 |
| agency | 5,855 | 5,855 | 2,027 | 5,855 | 2 |
| visibility | 6,025 | 6,025 | 2,171 | 5,811 | 3 |
| ecology | 5,388 | 5,388 | 2,486 | 5,388 | 1 |

All projected context roots fit at 8K; none fit at 3K. Combined's three optional Evolution roots are excluded at 8K, preserving whole-closure behavior. E+D combined leaves only **106** units; framing, direct hits or required specialist rules could change that result. F's full unions are 4,972 / 9,094 / 5,244 / 5,855 / 5,811 / 5,388; its 3K results and absent context roots equal E+D. F saves 203 or 214 units in affected cases, not enough cost evidence alone to justify broad contract evolution.

E+D's representative 8K fractions are **5,175 / 56,398**, **7,894 / 56,398**, **5,244 / 56,398**, **5,855 / 56,398**, **6,025 / 56,398**, **5,388 / 56,398**. Requested-root closures including the floor are 5,175 for save, 5,244 for GM, 5,210/5,244 for agency, 5,145/4,932/5,002 for visibility, and 4,929 for ecology. These are less authority over-retrieval than 6,385, but still include shared routine sections not individually needed by every root.

A 3K request cannot obtain these complete routine closures. The legitimate outcome is whole optional-root exclusion or an explicit required-root budget failure, not an incomplete dependency or claimed fulfilled guidance. Future acceptance should also use A's existing explicit required-snippet mechanism when the task needs that guidance, rather than treating any successful empty-of-request packet as success.

## Alternatives and Comparison Matrix

| Option | Correctness / authority / closure | Applicability and cost | Contract / identity / history | Complexity and owner |
| --- | --- | --- | --- | --- |
| A - widen whole authority | Precedence retained; full dependency retained; requested guidance still absent at 8K. | Operation failure removed, but oversized roots excluded and new authority hits admitted. | Format 1 unchanged; manifest/digest/bytes change; old artifacts untouched. | Small canonical metadata edit, insufficient FR-026H correction. |
| B - raise budget | Complete only after selector correction; no authority deletion. | 8,988-13,110 full unions; unnecessary governance/example cost and whole-context pressure. | Packet envelope/API or request policy change; artifact changes if A added. | Simple but abandons approved compact target rather than repairing modeling; owner/FR-031 boundary. |
| C - remove edge | Unsafe for substantive context claims; Kernel alone is not the full session/history hierarchy. | Cheap 2,603-6,725 full unions, without necessary prerequisite evidence. | Format unchanged but semantic artifact changes; old history retained. | Small edit, rejected on authority/completeness. |
| D - split context alone | Removes false dependencies from title/navigation/evidence, not necessary positive obligations. | Six gameplay cases still require the 6,385 authority source; insufficient alone. | Physical paths/IDs/bindings/hashes change, format 1 remains. | Canonical authoring plus compiler/fixture review; useful complement to E. |
| E - split authority + selective D | Can preserve routine and conditional authority if all clauses/branches are mapped; not yet certified. | Projected requested guidance fits 8K; routine over-retrieval bounded, 3K cannot fulfill it. | Format 1 retained; source/snippet identities and artifact provenance change deliberately; histories coexist. | Moderate reviewed source authoring/fixtures, FR-026H canonical corrective scope with FR-023/024/025 compatibility validation. |
| F - finer targets | Precise complete closures possible; cannot waive operation compatibility or conditional authority. | Small measured savings versus E+D; final selectors/graph still need design. | Requires explicit FR-022 evolution, preferably new format; compiler/validator/storage/runtime coexistence. | Highest breadth; earlier contract/compiler/import ownership must be expressly reopened/expanded. |
| G - separate provenance/executable edges | Principled distinction exists in documentation, but converting the present executable edge is unsafe without actual required text. | A non-executable edge has no packet cost, which is not proof of sufficient authority. | New metadata meaning/version if carried by artifact; cannot reinterpret existing format-1 dependencies. | Not a standalone fix; explicit contract design if needed. |
| H - authored compact authority capsule or Kernel expansion | Potentially correct after normative author review, not automatic summary or hidden context. | Could reduce framing/duplication; no measured complete reauthored model. | New canonical prose/provenance, risk duplicate authority; source/manifest/digest changes. | Avoid broader rewriting initially; prefer verbatim semantic partition before deliberate capsule consolidation. |

## Rejected Alternatives

Reject A/B/C as the correction, D alone as insufficient, and G as a relabeling of an executable obligation. Reject selective dependency snippets or selector bypass under format 1; it would silently change FR-022/D semantics. Reject vocabulary tuning, hidden extra prompts, chained uncounted packets, whole-corpus fallback or loading a summary that drops material authority. H may be considered only as explicit canonical authoring with equivalent safeguards, not runtime-generated condensation.

## Recommendation

E+D is preferred because it aligns logical source ownership and applicability with actual authority while retaining the accepted format and retrieval implementation. The maintainer must approve the scope/partition and deliberate `save` disposition, including how conflict/adoption branches obtain full exceptional context. This investigation does not supply that approval. If that partition cannot pass semantic and bounded acceptance, F is a separately approved contract evolution, not an implicit fallback implementation.

## Contract and Identity Consequences

### Recommended Existing-Format Correction

- **FR-022:** unchanged source-level dependency and strict format-1 semantics. A new canonical artifact is valid under the existing contract; validators must not be relaxed.
- **FR-023:** current loader forbids duplicate normalized source paths and has no section-selection input. Logical sections cannot simply be declared several times at one path. Use separately materialized ordinary source documents with real manifest IDs. Existing H2 splitting/H3 retention and exact-byte hashes remain unchanged; no hidden slicing or compiler heuristic.
- **FR-024:** preserve reviewed concepts, terms, weights, rationales and origin meaning. Update exact source/anchor binding targets only where mapped sections move; regenerate evidence honestly. Do not route bindings to cheaper unrelated rules to evade closure. Preserve old goldens as historical evidence and add intentional new candidate goldens.
- **FR-025:** migration 011 already owns sources/snippets/ordinals/dependencies per artifact and retains exact approved bytes, acquired-byte hash and normative JSON projection. A source restructure using format 1 should require **no SQL migration**; prove this with real import/readback/history tests rather than promising it without execution.
- **FR-026:** A-E algorithms/estimator/whole-source inclusion unchanged; F resolves exactly selected authorized artifact. G/H evidence changes because canonical topology/identity changed. Required applicable closures still cannot be partially admitted. Publication/activation remains separately authorized; import alone does not change active behavior or readiness.

| Identity | Expected consequence of a source partition |
| --- | --- |
| RuleSourceId | Retain IDs only for truthful retained logical ownership; moved sections need explicitly mapped IDs, not aliases hiding changed ownership. |
| SnippetId | Source ID plus anchor/content identity rules stay authoritative. A moved source changes snippet ID even if text/anchor is unchanged; retained source/anchor can preserve ID where contract permits. |
| Anchor | Preserve H2 wording where meaningful; duplicate/renamed headings need explicit mapping and fixture review. |
| Source SHA-256 | Any changed physical source bytes change exact provenance; moving/removing sections changes the retained source hash too. |
| Content hash | Unchanged normalized section text preserves content hash; new framing or scope changes may legitimately change it and must be reviewed. |
| Manifest provenance | New paths, selectors, source IDs, dependencies and vocabulary targets change exact manifest hash. |
| Semantic digest / serialized SHA-256 | Expected to change deliberately; neither is forced to the old frozen identity. Serialized hash remains distinct from semantic digest. |
| Vocabulary/quality fixtures | Map stable old-to-new targets; distinguish intentional new topology from historical goldens; keep negative associations negative. |
| Storage/import identity | New semantic artifact/history; retain old exact bytes, structured rows, counts and digest without rewriting them. |
| Publication identity | New candidate does not inherit active eligibility by existence; separately publish/activate explicitly under F's rules. |
| Compatibility | Old format-1 artifacts retain their old closures/failures; new format-1 artifact uses the same runtime semantics with a different graph. No cross-artifact row mixing. |

### Finer-Dependency Contract Alternative

Format 1 **forbids** undeclared structural properties; source targets must name sources, not anchors masquerading as IDs. The current semantic digest and validation do not cover new snippet edges. Consequently a "compatible optional field" silently added to format 1 is not a valid extension.

Prefer a new artifact format for F if approved. Choose an explicit stable snippet identity (or explicitly validated source+anchor) based on the existing identity contract, not physical paths. Specify source-wide edges versus snippet edges, ordering/digest coverage, mixed-graph cycles/self-dependencies/missing endpoints, transitive closure and applicability for each endpoint. A target subset still inherits source selectors unless the new contract explicitly introduces snippet applicability; fine edges alone cannot make the existing authority source gameplay-compatible.

FR-023 then needs explicit input/emission semantics, not inferred heading dependencies. FR-025's current dependency rows target source ordinals with artifact-local foreign keys; snippet targets are not representable there as-is. Reusing JSON/exact bytes alone must not bypass structured verification. Determine a smallest additive versioned projection/migration, preserve format-1 readback and test upgrade/idempotency. FR-026 would dispatch both validated formats without changing old whole-source semantics. This is FR-022/023/025/026 contract work, not a narrow F/runtime tweak; maintainer scope expansion is required. No schema or format change is performed here.

### Authority Versus Provenance

Document ownership links, manifest/source hashes, immutable source identity and rule-status hierarchy already serve non-executable authority/provenance roles. Executable dependencies answer a different question: which actual rule text must accompany an included root. The manifest's blanket edge fails to reflect that distinction precisely, but D does not conflate them accidentally: its declared meaning is explicit. Any future separation must name the two relations unambiguously and still carry all material execution authority. Calling the existing edge provenance cannot justify omission.

## Historical Compatibility and Classification

Keep imported historical format-1 artifacts immutable, including the frozen 10-source/154-snippet/773-term artifact and old failed quality cases. New canonical candidates coexist as distinct artifacts. No rewrite of old imports, active rows, campaign saves, historical tags or previous audit judgments. A retained older active artifact remains older until explicit authorized publication/activation; corrupt or incompatible selected artifacts must not trigger silent fallback to another artifact or legacy data.

Classification: **A/reference** covers the .NET analytical runner and current SQL projection implementation; these are not universal requirements. **B/Managed contract** covers authorized artifact isolation, lossless verified readback, complete dependency semantics and bounded deterministic retrieval. **C/Eternal Cycle-wide** covers claim-specific authority, visibility/agency, player-turn/save guarantees and truthful readiness. The canonical source-boundary repair must express C's obligations faithfully through B, without generalizing SQL/.NET implementation details into canon.

## Implementation Decomposition

These are proposed execution checkpoints under the still-selected FR-026, not new FR entries. **Do not start them until the maintainer authorizes the selected architecture.** Each starts by verifying the accepted checkpoint and rereading only its dependencies, this map and directly affected contracts. Stop on a format-contract conflict rather than expanding scope silently.

### FR-026H-R1 - Authoritative Partition Design

- **Objective/inputs:** approved E+D direction; current complete authority/context prose; this 32/24 map, six cases and named specialist owner contracts.
- **Outputs/files:** reviewed clause-to-source/branch map, exact proposed physical source/ID/path/selector/dependency list, old/new anchor identity map, `save` disposition and non-runtime documentation boundary. Design/audit/decision documents only.
- **Dependencies:** maintainer approval; no code or canonical prose partition yet.
- **Acceptance/tests:** account for every normative clause, including S and Repository/Campaign Canon framing; routine rules do not silently lose conflict/adoption routing or specialist obligations. Prove no routine/global edge drags exceptional material by default, and no exceptional operation executes without its authority. Documentation/link/structural checks; current blocker remains until the accepted correction is defined through governance.
- **Exclusions/stop:** no schema, compiler, ranking, vocabulary curation or source rewrite. Stop for review of the exact source/branch map; if source partition cannot preserve correctness, return an explicitly scoped format-evolution decision instead.

### FR-026H-R2 - Canonical Source Partition and Candidate Artifact

- **Objective/inputs:** accepted R1 map; unchanged portable format-1 pipeline and current canonical payload/approved vocabulary.
- **Outputs/files:** bounded physical rule sources and companion non-runtime references, manifest dependency/applicability changes justified by ownership, exact retargeted vocabulary bindings, identity map and candidate artifact/cost evidence. Primarily `docs/ai/`, `docs/persistence/`, manifest, canonical/quality fixtures and focused compiler tests.
- **Dependencies:** R1. Keep prose semantics; explicitly review any necessary title/scope clarification rather than normalizing it away.
- **Acceptance/tests:** current format validator accepts candidate; no lost authority clause, duplicate path/ID, unresolved target, cycle or inappropriate World/module/operation dependency. Repeated/relocated/culture/CWD compilation and audit remain deterministic. Six 8K cases contain their intended roots and complete reviewed authority; explicit required-root requests fail safely below closure cost. Reviewed negative topology stays negative. FR-022/023/024 tests, A-E focused retrieval, boundary harnesses and repository validator; record new counts/digests, never hardcode old 154 as compiler logic.
- **Exclusions/stop:** no A-E algorithm, ranking, budget, format, SQL schema, acquisition or runtime-authority change. Stop at a reviewed valid candidate; if actual costs invalidate the projection, re-open R1 before weakening completeness.

### FR-026H-R3 - SQL, History and Compatibility Acceptance

- **Objective/inputs:** R2 candidate and historical frozen artifact; existing importer, migration 011/012, publication/readiness/auth boundaries and F comparison harness.
- **Outputs/files:** real disposable SQL/reference equivalence, old/new history-isolation and publication/activation tests; honest new G/H quality evidence alongside retained old evidence. Primarily shared quality fixtures, portable/Managed tests and audit updates.
- **Dependencies:** R2, no campaign binding. Use existing storage unless an actual incompatibility proves a newly authorized change necessary.
- **Acceptance/tests:** lossless sources/snippets/selectors/dependencies/terms/order readback; all six packet identities/text/evidence/budgets/failures equal reference; old artifacts retain old identity/results; import does not activate; wrong/unauthorized/corrupt selections fail safely; legacy unselected routing unchanged. Run portable, CLI, SQL-enabled Managed suites and migration/import/publication/recovery regressions; exact counts and no skipped SQL acceptance claims.
- **Exclusions/stop:** no FR-027, G-style score tuning, production performance target, new provider or release work. Stop on cross-artifact mixing, missing authority, output mismatch or an unexpected storage-contract gap; do not silently repair by schema/format expansion.

### FR-026H-R4 - Renewed Integrated H Acceptance

- **Objective/inputs:** reviewed R1-R3 artifacts and evidence, every original FR-026 requirement, current exact candidate tree and historical H/G audits.
- **Outputs/files:** complete requirement matrix and corrective/closure audit; governance closure only if normative acceptance and maintainer authorization are satisfied. Roadmap/plan/UQ/audit indexes updated honestly.
- **Dependencies:** R1-R3 passed. This is the final integration validation package, not a new retrieval implementation.
- **Acceptance/tests:** all items below; affected Release build/publish packaging; complete portable/CLI/real SQL suites; focused A-G/legacy/recovery/auth/import checks; all structural harnesses, deterministic distribution, full repository validator with zero blockers, exact staged diff/whitespace. Update any test totals from actual executions, not the old 1,399. Confirm VERSION/tags and no campaign/private data; no automatic RC/release action.
- **Exclusions/stop:** do not begin FR-027/031 or other objectives. If any material H criterion fails, retain H incomplete and document the blocker; if it passes and closure is authorized, stop immediately after the agreed finalization, without downstream work.

## Renewed H Acceptance Plan

1. Test each of the six supported requests individually at 8K. Assert intended exact context roots and reviewed authority text, not just packet success/counts. Give manual save its explicit idempotent override/debug meaning, without replay, new campaign, new player choice or permission escalation.
2. Test full optional closure admission and explicit required-root requests at exact cost/one below/3K. A legitimate smaller packet may exclude optional roots; required guidance failure must remain explicit and cannot lead to gameplay resolving without necessary rules.
3. Preserve deterministic query preparation, exact vocabulary, selectors, scoring, complete direct/transitive/shared/diamond dependencies, single cost per member, bounded exclusions/evidence and no whole-corpus fallback. Review newly direct-matched authority roots after partitioning.
4. Re-run positive and negative curated quality fixtures. Replace candidate goldens intentionally while retaining old failure history; do not relabel omitted guidance as successful fulfillment or remove failure cases to hide regression.
5. Prove real SQL/reference equality and retained artifact isolation/readback/authorization/publication/readiness/legacy behavior. No provider reacquisition, hidden context prompt or Campaign binding.
6. Record new canonical source/snippet/term counts, source/content identities, manifest/digest/byte hashes, report bytes and bounded costs; explain intentional changes. Check all specialist obligations from the semantic map, not only Persistence Authority.
7. Full validation must pass with the blocking question resolved by an actual approved implemented correction. Unit/LocalDB evidence does not establish live MCP/AI-host, remote SQL or whole-runtime acceptance. Close only with the owner-authorized integrated decision; otherwise stop blocked.

## Validation and Limitations

Analytical evidence executed in this investigation: actual canonical compilation/identity checks, **36 A-E sensitivity invocations**, and **24 fixed-root cost projections**. Every run checked the frozen identity below and section totals; the runner exited successfully. No hypothetical artifact or SQL state was persisted. The temporary runner is not a permanent compiler/production feature.

Canonical artifact unchanged: **10 sources / 154 snippets / 773 terms / 275,622 bytes**. Semantic digest `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B`; serialized-byte SHA-256 `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6`. Exact-byte provenance uses the repository's frozen canonical fixture and explicit compiler/source identities, with current normalized-prose equivalence checked; no claim that Windows checkout CRLF bytes equal the frozen byte hashes.

| Investigation validation | Actually executed result |
| --- | --- |
| Disposable analytical runner | Canonical compilation/prose/hash checks succeeded; 36 A-E sensitivity invocations and 24 fixed-root arithmetic projections completed, exit 0. Temporary sources/build output removed; no artifact or SQL candidate persisted. |
| Nine repository structural harnesses | **333 assertions pass**: FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 operations 81, FR-021 control plane 35, FR-022 32, release-neutral campaign mode 17. |
| Full repository validator | `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate_repository.ps1`: **FAIL, exactly one issue: `Blocking unresolved questions remain.`** This is the preserved governance decision gate, not a waived error or a repository pass. |
| Links/navigation/contracts/distribution boundaries | No such error reported by the full validator. Its deterministic archive checks create and remove disposable validation ZIPs; no retained release artifact. Aggregate document/link counts are not printed on this failing run and are not invented. |
| Whitespace | Tracked `git diff --check` passes. Both untracked audits checked with `git diff --no-index --check -- NUL <audit>`: no whitespace findings; exit 1 denotes the expected new-file difference. LF-to-CRLF notices reflect checkout policy, not whitespace defects. |
| Exact scope/identity review | Seven tracked documentation modifications and two untracked audits; no staged files, production/fixture diff, remaining analytical sources or unintended untracked output. HEAD/upstream, VERSION and both tag objects unchanged. No commit/push. |

The earlier **1,399-test** integrated run belongs to the preserved H audit; it is not rerun or re-counted here because production and executable fixtures are unchanged. This investigation does not claim new SQL or live-host acceptance. The final validator is rerun after recording these results so the reviewable documentation state receives the same checks.

Unproven: approved physical partition and all framing/specialist dependencies; final actual selector/vocabulary topology and cost; universal semantic minimality; candidate format-1 or hypothetical format-2 acceptance; real SQL behavior of a future restructured artifact; live host/remote SQL behavior; external tokenizer and approximately 20K whole-runtime accounting. Cost projections guide the decision but cannot authorize authority omission. Maintainer decision remains necessary; no architecture correction or H closure is claimed.
