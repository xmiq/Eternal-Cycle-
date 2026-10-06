# FR-026H-R1 Authority Partition Review

## Checkpoint and Verdict

Owner-authorized **R1 only**, a design review beneath selected/incomplete FR-026. Starting `main`/`origin/main`: `d0d70e9a755d5bb3860dad86611f7c89e2fde734`, divergence `0/0`, VERSION `1.0.0`. Tag objects: `v1.0.0` = `6938bd2f9e965faaeb1c612b1dd765237cadc8a8`; `v1.1.0-rc` = `4eb2583027d316a962fd49fe318781a0e060183e`. Seven modified tracked design documents, two untracked H audits, nothing staged. Existing work is preserved.

Authority read in full: [Context Assembly](../../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md), [Persistence Authority](../../docs/persistence/PERSISTENCE_AUTHORITY.md), their [manifest](../../docs/rules/rule-source-manifest.json), [format-1 contract](../../docs/rules/COMPILED_RULES_ARTIFACT.md), FR-023 identity/compiler semantics, FR-026D/E implementations/contracts, all affected vocabulary bindings, and the existing H documentation. See the [integrated H audit](FR_026_COMPILED_RULE_RETRIEVAL_AUDIT.md) and [prior investigation](FR_026H_DEPENDENCY_GRANULARITY_INVESTIGATION.md). The established operation incompatibility is not reinvestigated or reclassified as a ranking/SQL defect.

**Result: the 2,572-unit routine subset requires additions.** This review supplies a clause-complete responsibility map and a conservative, representable source graph, not certification of an 8K corrective candidate. Retaining the omitted reusable/campaign authority boundaries, Codex/rule-status distinction, safeguards, scope limits and conflict classification raises the routine estimate to **4,903**. Player-agency, visibility and combined full closures do not fit. **H remains blocked; R2 is not ready or authorized.** No proof that all possible coherent source partitions fail is claimed, and finer dependencies are not established as unavoidable.

## Inventory Method

Each row below is one meaningful normative predicate, ordered step, typed field-set contract, or conformance assertion. An enumerated field set is one complete record/selection contract, not a license to discard its members. Separately enforceable prohibitions and ordered steps have separate rows. Repeated safeguards/conformance predicates retain their own identities and disposition even where another clause already establishes the behavior. Heading-only text and navigation are accounted for separately in the identity map.

Inventory coverage: **80 PA clauses** (68 P, seven G, five Q) and **193 CA clauses** (133 C, eight A, 52 T), **273 unique clauses with one destination each**. T contains four optional storage-contract units, 25 regression predicates and 23 acceptance predicates. All 18 core invariants, eight read steps, 12 automatic-persistence steps, eight adoption steps, ten authority conflict steps, 12 authority safeguards and ten context safeguards are accounted for. All 56 existing snippet identities and all 42 affected vocabulary binding targets are mapped below. These are review-unit counts, not artifact snippet counts or a substitute for reviewing the original prose.

The current source is `core-persistence-authority` for PA rows and `core-context-assembly` for CA rows. The current H2 anchor appears in each row; H3, list ordinal or semantic predicate identifies the unit within it. Clause IDs are review references, **not new snippet IDs or compiler syntax**. Rows are a map to the complete existing prose, not replacement abbreviated rules.

Roles: **F** foundational authority; **P** procedural execution; **K** runtime/ownership contract; **H** host conformance; **I** optional implementation contract; **E** interpretive conformance example. Need columns **G/M/X** mean ordinary gameplay/context, manual persistence, and adoption/retcon/conflict correction: `Y` required when that obligation is material, `C` conditional on encountering the branch, `-` not executable guidance for that consumer. H/I obligations remain mandatory for a host claiming that capability, rather than becoming optional runtime facts.

Destinations (all provisional, no files created):

| Key | Proposed owner and ordinary-file path | Executable source? |
| --- | --- | --- |
| P | `core-persistence-authority`, existing Persistence Authority path | Yes; routine authority, including conflict classification |
| C | `core-context-assembly`, existing Context Assembly path | Yes; routine read/resolve/save/recovery |
| G | `core-authority-governance`, `docs/persistence/AUTHORITY_GOVERNANCE.md` | Yes; upward review, adoption and authorized retcons |
| A | `core-context-adoption`, `docs/ai/CONTEXT_ASSEMBLY_ADOPTION.md` | Yes; existing-campaign adoption procedure |
| Q | `docs/persistence/PERSISTENCE_AUTHORITY_CONFORMANCE.md` | No RuleSourceId; preserved worked examples/navigation, owned assertions cross-reference P/G |
| T | `docs/ai/CONTEXT_ASSEMBLY_CONFORMANCE.md` | No RuleSourceId; preserved host tests and optional schema guidance, owned predicates cross-reference C/A/P/G |

All clauses are world-neutral, module-neutral and apply in all Campaign Modes. Executable operation sets are specified under Applicability, with row need/branch qualifications. Conformance T/Q is repository/host review, not permission to perform campaign SQL. A branch is not triggered merely by an operation label. Detailed specialist owners remain authoritative; a scope reference is not a substitute for loading their rules when material.

Dependency bundles below identify **clauses**, not undocumented compiler edges:

| Bundle | Required PA clauses / purpose |
| --- | --- |
| O | PA-01..06: fact/rule distinction, precedence, claim/scope/time, valid new events |
| R | PA-07..14, PA-33..35: reusable/campaign ownership, active profile, rule status and Codex boundary |
| F | PA-15..32: history, integrated state, session/draft and narration distinctions |
| V | PA-36..37: authority is not disclosure/Observer View |
| U | PA-38..39: honest uncertainty cannot be replaced by a guess |
| AGENCY | PA-40..42: records preserve choices, not scripts |
| TINT | PA-43..44: source/scope/dependents and explicit downward integration |
| CONFLICT | PA-47..57: exact-claim classification, routing and coherent resumption |
| GOV | PA-45..46, PA-58..62: higher-authority proposal/adoption/retcon processes |
| LIMIT | PA-68..80: safeguards and non-ownership boundaries |

Every context runtime source depending on P receives **all of P**, not only these explanatory bundles. Specialist procedures additionally receive G when required. Internal references name actual inventory IDs; dedicated owner contracts are denoted `Owner` and resolved explicitly in the completeness review. No packet budget is reduced by treating a bundle as a supported partial-source dependency.

## Persistence Authority Clause Inventory

| Clause | Current anchor/unit and obligation | Role; G/M/X | Requires | Destination |
| --- | --- | --- | --- | --- |
| PA-01 | `purpose`: incompatible campaign claims use authority, not recency, stale notes or convenience | F; Y/Y/Y | PA-03..05 | P |
| PA-02 | `purpose`: campaign-fact authority is distinct from GM rule-status hierarchy | F; Y/Y/Y | PA-33..35; GM Framework | P |
| PA-03 | `core-rule`: ordered Repository, Campaign, History, State, Session, Narration layers | F; Y/Y/Y | PA-07..32 | P |
| PA-04 | `core-rule`: lower authority never silently overwrites higher | F; Y/Y/Y | PA-03 | P |
| PA-05 | `core-rule`: compare the same subject/scope/time; replacement needs authorized correction/retcon/migration | F; Y/Y/Y | PA-03; PA-47..57; GOV conditional | P |
| PA-06 | `authority-is-claim-specific`: compare claims, not document prestige; inventory/reset premise, loss, present possession, new recovery and appearance have different owners; new valid recovery integrates rather than erases loss | F; Y/Y/Y | PA-03; PA-15..32; TINT | P |
| PA-07 | `repository-canon`: highest reusable rules and accepted governance authority | F; Y/Y/Y | PA-02 | P |
| PA-08 | `repository-canon`: includes docs rules, accepted decisions, terminology, explicit owners/scopes, safeguards/foundations | F; Y/Y/Y | PA-07 | P |
| PA-09 | `repository-canon`: interprets mechanical meaning; contains no populated campaign facts | F; Y/Y/Y | PA-07; PA-11 | P |
| PA-10 | `repository-canon`: changed repository does not rewrite history; recorded active profile remains until explicit conversion, subject to mandatory safety/approved correction | F; Y/Y/Y | PA-12..14; GOV conditional | P |
| PA-11 | `repository-canon` / Repository Conflicts: conflicting rules/governance neither silently wins; resolve both before calling affected material complete | P; C/C/Y | PA-02; Repository Conventions | P |
| PA-12 | `campaign-canon`: highest permitted campaign-specific authority; identity/profile/premises/setting/control/retcons/interpretations/conversions/stable IDs belong here | F; Y/Y/Y | PA-07..10 | P |
| PA-13 | `campaign-canon`: permitted options/external experiment/older version do not canonize incompatible mechanics | F; Y/Y/Y | PA-07; PA-12 | P |
| PA-14 | `campaign-canon`: changes need authorized decision, provenance, scope, date, affected records and migration consequences; narration/preference/theory/repeated error/file edits cannot authorize | F; Y/Y/Y | PA-12..13; GOV conditional | P |
| PA-15 | `historical-record`: durable ordered established events/transitions/discoveries/corrections/consequences | F; Y/Y/Y | PA-12 | P |
| PA-16 | `historical-record`: event support includes what/time/place/actors/owners/adjudication/changes/unresolved/supersession | F; Y/Y/Y | PA-15 | P |
| PA-17 | `historical-record`: append normally; correction adds trace/supersession, retcon records Campaign Canon action then propagates | F; Y/Y/Y | PA-14..16; TINT; GOV conditional | P |
| PA-18 | `historical-record`: dated testimony/belief/research/narration retains status rather than becoming fact | F; Y/Y/Y | PA-38..39; Truth Layers | P |
| PA-19 | `current-campaign-state`: latest authoritative structured now, within covered scopes | F; Y/Y/Y | PA-05; PA-12; PA-15 | P |
| PA-20 | `current-campaign-state`: bodies/Soul/relations/placement/assets/institutions/projects/research/world/processes use State Model and Structured Architecture logical owners | K; Y/Y/Y | PA-19; Owner | P |
| PA-21 | `current-campaign-state`: derived from Campaign Canon/history; material consequences require valid new event/correction/retcon/migration to change | F; Y/Y/Y | PA-12; PA-15..18; GOV conditional | P |
| PA-22 | `current-campaign-state`: stale state cannot erase later history; repair missed updates from authoritative history/adjudication | P; C/C/Y | PA-16..21; CONFLICT | P |
| PA-23 | `current-campaign-state`: outranks informal memory/narration for latest integrated condition | F; Y/Y/Y | PA-19; PA-29..32 | P |
| PA-24 | `current-campaign-state`: valid unintegrated session event is pending, not nonexistent | F; Y/Y/Y | PA-25..28 | P |
| PA-25 | `current-session`: bounded working facts/outcomes/decisions/traces/deltas/uncertainty since last confirmed integration | F; Y/Y/Y | PA-15; PA-19 | P |
| PA-26 | `current-session`: valid resolved change differs from proposal/draft/plan/hypothetical/uncommitted branch | F; Y/Y/Y | PA-25; PA-38..39 | P |
| PA-27 | `current-session`: no Campaign/Repository amendment; classify contradiction; incomplete recording requires recovery, not discarding session | F; Y/Y/Y | PA-07; PA-12..14; CONFLICT | P |
| PA-28 | `current-session`: Save Update integrates confirmed changes; identifiable Session Delta before atomic activation is neither draft nor validated Save Point | K; Y/Y/Y | PA-24..27; Save Update Protocol | P |
| PA-29 | `current-narration`: immediate presentation is lowest persistence authority | F; Y/Y/Y | PA-03 | P |
| PA-30 | `current-narration`: ordinary detail/new event permitted only with GM authority and no higher conflict; incomplete/observer-limited/metaphorical/mistaken/ambiguous/stale/contradicted/inconsistent presentations stay bounded | F; Y/Y/Y | PA-04..06; PA-36..39; GM Framework | P |
| PA-31 | `current-narration`: vivid/recent/repeated/momentarily accepted wording gains no authority | F; Y/Y/Y | PA-04 | P |
| PA-32 | `current-narration`: stop relying on conflicting prose; continuity resolution, not defending an already narrated error | P; C/C/Y | CONFLICT; Continuity Resolution | P |
| PA-33 | `rules-authority-and-persistence-authority`: Codex is reusable design below Repository Canon, not campaign Truth Layer; configuration stores adopted Codex Version, Campaign Canon presence/divergences; no silent later migration | F; Y/Y/Y | PA-07..14; Living Codex | P |
| PA-34 | `rules-authority-and-persistence-authority`: ownership table assigns mechanics, adopted premises, established events, integrated now, pending interaction and audience presentation to distinct owners | F; Y/Y/Y | PA-02..32; V | P |
| PA-35 | `rules-authority-and-persistence-authority`: higher persistence cannot invent mechanics; lower rule status cannot rewrite facts; both valid for material claim | F; Y/Y/Y | PA-02; PA-34 | P |
| PA-36 | `authority-does-not-equal-visibility`: every layer can contain restricted material; narration reveals only an Observer View, not all accessed truth | F; Y/Y/Y | PA-03; Truth Layers | P |
| PA-37 | `authority-does-not-equal-visibility`: Truth Layers owns visibility; authority answers which claim, not who may access | K; Y/Y/Y | PA-36; Truth Layers | P |
| PA-38 | `authority-does-not-equal-certainty`: Unknown/Not Yet Verified/Estimated/Disputed/Requires Source Recovery/Pending Resolution are valid authoritative statuses | F; Y/Y/Y | PA-05 | P |
| PA-39 | `authority-does-not-equal-certainty`: unsupported certainty/lower guess cannot overwrite higher unknown | F; Y/Y/Y | PA-04; PA-38 | P |
| PA-40 | `authority-does-not-equal-actor-control`: prior statements/obligations/plans/beliefs/pressures do not preselect deliberate choices | F; Y/Y/Y | PA-25; GM player control | P |
| PA-41 | `authority-does-not-equal-actor-control`: relations/faction plans/prophecy/Soul Titles/player objectives do not compel affection, obedience, execution, events or next player action | F; Y/Y/Y | PA-40 | P |
| PA-42 | `authority-does-not-equal-actor-control`: remember choices/conditions rather than converting records to scripts | F; Y/Y/Y | PA-40..41 | P |
| PA-43 | `valid-downward-updates`: migration, retcon, valid historical event, integrated session and state-to-narration propagation use explicit owning procedures | F; Y/Y/Y | O; F; GOV conditional | P |
| PA-44 | `valid-downward-updates`: every update states source/scope/affected records/unresolved conflicts; no silent overwrite | F; Y/Y/Y | PA-43; CONFLICT | P |
| PA-45 | `valid-upward-proposals`: narration/session/state/history/play evidence may propose higher review, not enact it | F; -/-/Y | O; R; F | G |
| PA-46 | `valid-upward-proposals`: higher layer changes only through its own authority process | P; -/-/Y | PA-14; PA-45; governance owners | G |
| PA-47 | `conflict-classification-procedure` / 1: exact subject/property/scope/time claim | P; C/C/Y | PA-05..06 | P |
| PA-48 | `conflict-classification-procedure` / 2: identify both layers, not file versus transcript | P; C/C/Y | PA-03; PA-47 | P |
| PA-49 | `conflict-classification-procedure` / 3: identify establishing/constraining specialist owner | P; C/C/Y | PA-34; Owner | P |
| PA-50 | `conflict-classification-procedure` / 4: check chronology/region/observer/meaning before claiming contradiction | P; C/C/Y | PA-05; PA-47..49 | P |
| PA-51 | `conflict-classification-procedure` / 5: separate fact/proposal/belief/theory/estimate/secret/narration status | P; C/C/Y | F; V; U | P |
| PA-52 | `conflict-classification-procedure` / 6: preserve higher claim, stop propagating lower conflict | P; C/C/Y | O; PA-50..51 | P |
| PA-53 | `conflict-classification-procedure` / 7: classify narration error/stale state/incomplete save/corruption/source gap/authorized retcon/migration/deception | P; C/C/Y | PA-47..52 | P |
| PA-54 | `conflict-classification-procedure` / 8: route owning correction/migration/save/continuity procedure | P; C/C/Y | PA-53; Owner; GOV when actually executing retcon/adoption | P |
| PA-55 | `conflict-classification-procedure` / 9: record change/reason/authorizer/dependent review | P; C/C/Y | PA-54; TINT | P |
| PA-56 | `conflict-classification-procedure` / 10: resume coherent state; unaffected play may continue, dispute not prematurely settled | P; C/C/Y | PA-47..55; U | P |
| PA-57 | `conflict-classification-procedure`: expanded continuity document does not change lower-recency versus higher-provenance result | K; C/C/Y | O; Continuity Resolution | P |
| PA-58 | `repository-revision-changes`: available update does not auto-change profile/outcomes/records/causality/capabilities/provisional review or authorize retcon | F; -/-/Y | PA-10; PA-14; F | G |
| PA-59 | `repository-revision-changes`: compare versions, decide adoption/conversion, migrate affected records, preserve interpretation, validate through Migration owner | P; -/-/Y | PA-58; TINT; Migration and Versioning | G |
| PA-60 | `authorized-retcons`: Campaign-Canon-approved deliberate correction/replacement, not discovery of false belief | F; -/-/Y | PA-12..18; U | G |
| PA-61 | `authorized-retcons`: grantor/prior claim/replacement/reason/scope/history/state/knowledge/relationship/progression/consequence dependencies/material player-choice remedy recorded | P; -/-/Y | PA-60; AGENCY; TINT | G |
| PA-62 | `authorized-retcons`: old record remains traceable superseded material for migration/continuity audit | F; -/-/Y | PA-17; PA-61 | G |
| PA-63 | `worked-examples` / lost item: established loss prevails absent valid recovery/reconstruction/illusion/replica/event; correct scene, never invent hidden recovery to defend prose | E; -/-/- | PA-06; PA-21; PA-26; PA-30..32; PA-39; CONFLICT | Q |
| PA-64 | `worked-examples` / unsaved gate: recover adjudication, append history/update state, validate dependent travel/communication/Reincarnation routes | E; -/-/- | PA-16..17; PA-24..28; PA-44; PA-49..56; Save Update/Validation | Q |
| PA-65 | `worked-examples` / false history: knowledge-belief remains consequential without replacing differently scoped historical truth | E; -/-/- | PA-05; PA-18; PA-36..39; Truth Layers | Q |
| PA-66 | `worked-examples` / Soul revision: compare former provisional ruling/consequences, authorized conversion, current-state update, preserve former profile's historical scenes | E; -/-/- | PA-10; PA-33..35; PA-58..59; Migration | Q |
| PA-67 | `worked-examples` / private death note: secrecy grants no authority; classify plan/draft/missing-evidence event/omission; cannot overwrite survival before validation | E; -/-/- | PA-19..28; PA-36..39; PA-47..56 | Q |
| PA-68 | `safeguards` / 1: lower never silently overwrites higher | F; Y/Y/Y | PA-04 | P |
| PA-69 | `safeguards` / 2: recency/repetition/drama/storage location creates no authority | F; Y/Y/Y | PA-03; PA-31 | P |
| PA-70 | `safeguards` / 3: Campaign cannot amend Repository Canon | F; Y/Y/Y | PA-13 | P |
| PA-71 | `safeguards` / 4: repository update needs adoption/migration, not historical rewrite | F; Y/Y/Y | PA-10; GOV when executing change | P |
| PA-72 | `safeguards` / 5: corrections/retcons preserve traceable history | F; Y/Y/Y | PA-17; GOV conditional | P |
| PA-73 | `safeguards` / 6: failed integration cannot erase valid event | F; Y/Y/Y | PA-24..28 | P |
| PA-74 | `safeguards` / 7: drafts/hypotheticals/branches not established outcomes | F; Y/Y/Y | PA-26 | P |
| PA-75 | `safeguards` / 8: narration must not defend/compound known error | F; Y/Y/Y | PA-32 | P |
| PA-76 | `safeguards` / 9: unknown stays unknown at every layer | F; Y/Y/Y | PA-38..39 | P |
| PA-77 | `safeguards` / 10: numerical changes need owned event or explicit correction | F; Y/Y/Y | PA-16; PA-21; PA-26; PA-44 | P |
| PA-78 | `safeguards` / 11: authority does not collapse truth/visibility/certainty/agency | F; Y/Y/Y | V; U; AGENCY | P |
| PA-79 | `safeguards` / 12: no populated authority record in reusable repository | K; Y/Y/Y | PA-09; Repository Conventions | P |
| PA-80 | `scope-boundaries`: precedence/routing is not architecture, truth/visibility/lifetimes/schemas/migration/full-continuity/save/validators/templates ownership; blank templates do not gain authority | K; Y/Y/Y | PA-02; PA-20; PA-37; dedicated owners | P |

## Context Assembly Clause Inventory

| Clause | Current anchor/unit and obligation | Role; G/M/X | Requires | Destination |
| --- | --- | --- | --- | --- |
| CA-01 | `purpose`: canonical read and automatic persistence form valid turn; compact persistence-backed context wraps existing Read Set/Save Update | K; Y/Y/Y | O; R; F; State/Save owners | C |
| CA-02 | `purpose`: no ordinary manual-save requirement; state-changing turn completes only after validated transaction available to later reads | K; Y/Y/Y | F; TINT; CA-57..68 | C |
| CA-03 | `document-control`: owns turn, packet, relevance/hierarchy/read/save/next-turn verification, not other state owners | K; Y/Y/Y | R; LIMIT; State/Data Ownership/Runtime | C |
| CA-04 | `document-control`: depends on State Model/Data Ownership/Save Update/Validation/AI Runtime and configured Direct/Managed provider | K; Y/Y/Y | LIMIT; named owners | C |
| CA-05 | `document-control`: parameterized queries/views/builders/disposable caches/visual packets extend navigation without new authority | K; Y/Y/Y | O; F; Visual Context | C |
| CA-06 | `document-control`: execution profiles/tools/boot/play/representation/save/recovery/debug/handoff consume this contract | K; Y/Y/Y | CA-01..05 | C |
| CA-07 | `document-control`: no populated state/packet/schema/credential/private locator/provider configuration in repository contract | K; Y/Y/Y | LIMIT; Repository Conventions | C |
| CA-08 | `runtime-boundary`: no universal host/populated DB; conforming host executes actual configured reads/writes/validation/readback | K; Y/Y/Y | CA-04; F; TINT | C |
| CA-09 | `runtime-boundary`: structural validation is not observed host I/O/conformance; acceptance cases must be satisfied | K; Y/Y/Y | CA-08; CA-R01..25; CA-A01..23 | C |
| CA-10 | `core-invariants` / 1: conversation is convenience, not Canon | K; Y/Y/Y | O; F | C |
| CA-11 | `core-invariants` / 2: required Canon read before material resolution | K; Y/Y/Y | O; F; CA-48..56 | C |
| CA-12 | `core-invariants` / 3: context relevance-filtered/persistence-backed | K; Y/Y/Y | CA-32..34 | C |
| CA-13 | `core-invariants` / 4: packets/Running/Session summaries are Derived/Caches, own no mutable Canon | K; Y/Y/Y | O; F; CA-41..47 | C |
| CA-14 | `core-invariants` / 5: every resolution explicitly determines Affected Set, including empty | K; Y/Y/Y | F; TINT; CA-55..56 | C |
| CA-15 | `core-invariants` / 6: non-empty triggers automatic persistence without command | K; Y/Y/Y | CA-14; CA-57..68 | C |
| CA-16 | `core-invariants` / 7: state-changing turn cannot close before successful validated transaction | K; Y/Y/Y | F; TINT; CA-15 | C |
| CA-17 | `core-invariants` / 8: explicit failed reads/writes, never concealed by conversation memory | K; Y/Y/Y | U; CONFLICT; CA-90..94 | C |
| CA-18 | `core-invariants` / 9: retry retains interaction/transaction identity, no duplicate effects | K; Y/Y/Y | F; TINT; CA-93..94 | C |
| CA-19 | `core-invariants` / 10: relevant Turn N+1 reads committed Turn N | K; Y/Y/Y | F; CA-95..101 | C |
| CA-20 | `core-invariants` / 11: successful plumbing backstage in ordinary Gameplay Context | K; Y/Y/Y | V; CA-78; CA-122 | C |
| CA-21 | `core-invariants` / 12: AI GM operates through, owns neither rules nor campaign state | K; Y/Y/Y | R; F; LIMIT | C |
| CA-22 | `core-invariants` / 13: resolve/verify configured canonical target before state-changing play | K; Y/Y/Y | F; CA-30..31 | C |
| CA-23 | `core-invariants` / 14: visible status is adapter evidence, not intent/narration | K; Y/Y/Y | F; U; CA-71..79 | C |
| CA-24 | `core-invariants` / 15: derived refresh after canonical validation/readback, no unsaved promotion | K; Y/Y/Y | F; TINT; CA-86..87 | C |
| CA-25 | `core-invariants` / 16: canonical depiction gets purpose-specific visibility-filtered visual context, not conversation authority | K; Y/Y/Y | V; U; CA-38..40 | C |
| CA-26 | `core-invariants` / 17: only real new player input begins new turn; tools/internal continuation stay originating interaction | K; Y/Y/Y | AGENCY; GM Runtime Procedure | C |
| CA-27 | `core-invariants` / 18: yield at unresolved player-owned choice, never choose via internal continuation | K; Y/Y/Y | AGENCY; CA-26 | C |
| CA-28 | `gameplay-turn-state-machine`: ordered target/context/read/resolve/Affected/write/validate/authority/derived/complete gates; externally read-resolve-save-verify-deliver | P; Y/Y/Y | O; F; TINT; CA-30..31; CA-48..68 | C |
| CA-29 | `gameplay-turn-state-machine`: actual input boundary; hidden reasoning/continuation/tools/results/retry/read/retrieval/persistence/validation/same-input simulation stay originating interaction; stop for next player-owned choice | P; Y/Y/Y | AGENCY; CA-26..28 | C |
| CA-30 | `persistence-target-resolution-gate`: read identity/strategy/authority/complete chain or interface/durability/version/Save Point/concurrency/open transaction/sync/working-copy freshness; locate/fetch/contact/verify; Direct remote lookup and Managed namespace/binding differ | P; Y/Y/Y | R; F; U; Configuration/Save Index/Portable Persistence | C |
| CA-31 | `persistence-target-resolution-gate`: no blank replacement because assumed local path missing; unresolved/ambiguous/inaccessible authority blocks changing play with truthful pending/failure | P; Y/Y/Y | CA-30; U; CONFLICT | C |
| CA-32 | `context-assembly-layer`: smallest complete Read Set materially affecting intent/resolution/consequence/uncertainty/disclosure; implementations navigate owners, not create owner | K; Y/Y/Y | O; R; F; V; U; CA-04..05 | C |
| CA-33 | `context-assembly-layer`: published dependency-complete Managed packet, no AI crawl/compile; Kernel/Core/selected World/enabled modules/operation-topic rules; separate same-Campaign Canon; at-most-8K target without omitted required rules/unrelated worlds/hidden uncounted prompt | K; Y/Y/Y | R; O; Rule Compilation and Retrieval | C |
| CA-34 | `relevance-selection`: include presence/intent/immediate consequence/relationships/assignments/hazards/invoked systems/history/visibility-memory-visual materiality; stop immaterial reference expansion, correctness before marginal brevity | P; Y/Y/Y | O; F; V; U; AGENCY; specialist owners | C |
| CA-35 | `current-scene-context`: relevant interaction/Save Point/time/location/perspective/entity/intent/boundary/entities/autonomous/groups/relationships/conditions/assets/projects/world/rules/events/source-owner-ID-version/deep-reference/freshness-provenance fields | K; Y/Y/Y | O; R; F; V; U; AGENCY; CA-34; State owners | C |
| CA-36 | `current-scene-context`: navigation IDs/owner paths/table names/transaction plumbing not ordinary narration | K; Y/Y/Y | V; PA-34; CA-35 | C |
| CA-37 | `current-scene-context`: GM archive/memory access grants no recall; player sees only bounded incarnation Character Knowledge Recall Manifestation | K; Y/Y/Y | V; U; Memory Continuity | C |
| CA-38 | `purpose-specific-visual-context`: smallest complete sourced visual projection with IDs/established traits/form/model/assets/conditions/environment/observer/secrets/unknown/rendering freedom | K; Y/Y/Y | R; F; V; U; Visual Context/Identity owners | C |
| CA-39 | `purpose-specific-visual-context`: packet owns no appearance; Current Appearance assembled from owners; unspecified/generated detail not Canon | K; Y/Y/Y | O; F; U; CA-38 | C |
| CA-40 | `purpose-specific-visual-context`: ordinary depiction empty Affected Set; explicit authorized trait adoption/correction/durable change uses owner Save Transaction | P; Y/Y/Y | TINT; PA-14; CA-55..68; Visual Identity | C |
| CA-41 | `hierarchical-context`: scene/Running/Session/Long-Horizon/Life/full read depth, smallest sufficient, direct alternate branches allowed; retrieval hierarchy not authority, owner records outrank all Derived | K; Y/Y/Y | O; F; CA-32 | C |
| CA-42 | `hierarchical-context` / Running: short-lived scene/intent/entities/committed changes/open action/observations/owner pointers; regenerate after relevant Canon change | K; Y/Y/Y | F; CA-41; CA-46..47 | C |
| CA-43 | `hierarchical-context` / Session: event/discovery/relation/project/infrastructure/Skill/Development/open-thread pointers, not History or specialist owner | K; Y/Y/Y | F; PA-15..20; CA-41 | C |
| CA-44 | `hierarchical-context` / History: relevant intervals/Life identity/retained development/Skills/relations/death/legacy/support queries only, not all eras/Lives per turn | K; Y/Y/Y | F; V; Archive/Long-Horizon owners | C |
| CA-45 | `freshness`: game-time/interval,parent Version/Save Point,model/migration versions,interaction/source IDs,revisions/hashes,visibility/audience,generation provenance detect staleness | K; Y/Y/Y | O; F; V; CA-41..44 | C |
| CA-46 | `freshness`: wall-clock age insufficient; relevant source changes invalidate/regenerate | P; Y/Y/Y | CA-45; F | C |
| CA-47 | `freshness`: owner wins conflict, mark stale/incorrect, repair from owners, never edit Canon to fit packet | P; Y/Y/Y | O; F; CONFLICT; CA-46 | C |
| CA-48 | `mandatory-read-gate` / 1: Save Index/version/Save Point/session/open recovery | P; Y/Y/Y | F; CA-30 | C |
| CA-49 | `mandatory-read-gate` / 2: intent/perspective/entity/time/placement | P; Y/Y/Y | F; V; AGENCY; CA-35 | C |
| CA-50 | `mandatory-read-gate` / 3: narrowest rule and campaign owner per material claim | P; Y/Y/Y | O; R; LIMIT | C |
| CA-51 | `mandatory-read-gate` / 4: build relevance-filtered initial Read Set | P; Y/Y/Y | CA-34..37; State Model | C |
| CA-52 | `mandatory-read-gate` / 6: verify identity/version/status/Truth Layer/visibility/effective-time/supersession | P; Y/Y/Y | O; F; V; U | C |
| CA-53 | `mandatory-read-gate` / 7: classify material gaps as Unknown/Not Yet Verified/Estimated/Disputed/Requires Source Recovery | P; Y/Y/Y | U; CONFLICT; CA-52 | C |
| CA-54 | `mandatory-read-gate`: State Model remains authoritative; no conversation substitute after failed read, only genuinely immaterial/canon-permitted uncertainty may proceed | K; Y/Y/Y | R; U; CA-48..53; State Model | C |
| CA-55 | `resolution-and-affected-set-gate`: determine every changed/created persistent owner; all 14 listed categories including bodies, development, placement, custody, effects, relations/knowledge, processes, autonomous/network/assignments, equipment, species/Soul/world/location/history/visual adoption | P; Y/Y/Y | F; TINT; Data Ownership/State Model | C |
| CA-56 | `resolution-and-affected-set-gate`: movement/time/spending/failure/discovery/observation/social reaction can change state; quiet narration does not prove empty set | K; Y/Y/Y | F; CA-55 | C |
| CA-57 | `automatic-persistence-gate` / 1: retain Interaction ID, assign/reuse Transaction ID | P; Y/Y/Y | F; Save Update | C |
| CA-58 | `automatic-persistence-gate` / 2: recheck parent Save Point/Read Set | P; Y/Y/Y | O; F; CA-48..54 | C |
| CA-59 | `automatic-persistence-gate` / 3: dependency-complete Affected Set/Session Delta | P; Y/Y/Y | F; TINT; CA-55..56 | C |
| CA-60 | `automatic-persistence-gate` / 4: stage once per Authoritative Record Owner | P; Y/Y/Y | TINT; CA-57..59; Data Ownership | C |
| CA-61 | `automatic-persistence-gate` / 5: chronology/history append only under own rules | P; Y/Y/Y | PA-15..18; Save Update/Timeline/History | C |
| CA-62 | `automatic-persistence-gate` / 6: atomic owner commit through configured chain/service | P; Y/Y/Y | TINT; CA-30; CA-60..61; Save Update | C |
| CA-63 | `automatic-persistence-gate` / 7: implementation and semantic validation | P; Y/Y/Y | O; R; F; CA-62; Validation | C |
| CA-64 | `automatic-persistence-gate` / 8: critical readback/expected comparison/version verification | P; Y/Y/Y | F; U; CA-63; CA-84..85 | C |
| CA-65 | `automatic-persistence-gate` / 9: verify configured cloud synchronization/Managed completion evidence | P; Y/Y/Y | F; U; CA-30; CA-64; provider contract | C |
| CA-66 | `automatic-persistence-gate` / 10: activate new Save Point only at configured authority boundary | P; Y/Y/Y | F; TINT; CA-65; Save Point | C |
| CA-67 | `automatic-persistence-gate` / 11: regenerate/invalidate derived summaries/scene from verified Canon | P; Y/Y/Y | F; CA-41..47; CA-66 | C |
| CA-68 | `automatic-persistence-gate` / 12 and postcondition: only then complete/deliver evidence marker; no command required; local-only call or unvalidated service success insufficient | P; Y/Y/Y | CA-57..67; CA-71..79 | C |
| CA-69 | `automatic-persistence-gate` / Patch: service reads existing JSON object, preserves omissions, supplied set-only values enter normal full owner-mutation/version/revision/transaction/validation/activation/readback/receipt path | K; Y/Y/Y | F; TINT; CA-57..68; Managed mutation owner | C |
| CA-70 | `automatic-persistence-gate` / Patch limits: no create/delete/identity-reference changes/arbitrary SQL/owner bypass/omission deletion; unsupported uses full mutation; same retry identity, failure activates nothing | K; Y/Y/Y | CA-69; CA-93..94 | C |
| CA-71 | `player-visible-persistence-status`: exactly one compact truthful end marker on ordinary response | K; Y/Y/Y | F; U; CA-68 | C |
| CA-72 | `player-visible-persistence-status`: local saved requires commit/validation/readback | K; Y/Y/Y | CA-63..66 | C |
| CA-73 | `player-visible-persistence-status`: cloud saved requires remote sync/readback/verification | K; Y/Y/Y | CA-65; provider contract | C |
| CA-74 | `player-visible-persistence-status`: pending means incomplete/recoverable pending; failure means required write/sync/change-proof/validation/readback failed | K; Y/Y/Y | U; CA-64..65; CA-85 | C |
| CA-75 | `player-visible-persistence-status`: Managed local-style marker means validated service expected Transaction/active Version receipt; no client backend/cloud classification | K; Y/Y/Y | CA-65; Managed completion contract | C |
| CA-76 | `player-visible-persistence-status`: narration/summary/local candidate/upload/requested sync/intent never prove saved | K; Y/Y/Y | F; U; CA-72..75 | C |
| CA-77 | `player-visible-persistence-status`: cloud local success stays pending through required remote/semantic/backup checks; cloud failure retains validated local candidate for idempotent retry | K; Y/Y/Y | CA-65; CA-74; CA-83 | C |
| CA-78 | `player-visible-persistence-status`: verified no-op may show existing authority marker, no new save; ordinary success shows only marker except requested status/debug/needed concise technical failure | K; Y/Y/Y | V; CA-71..77; CA-88..89 | C |
| CA-79 | `player-visible-persistence-status`: pending/unresolved failure blocks next changing turn until saved/recovered last validated authority/explicit stop | K; Y/Y/Y | F; U; CA-77; CA-90..94 | C |
| CA-80 | `manual-persistence-commands`: override/recovery, automatic persistence still mandatory | K; Y/Y/Y | CA-02; CA-57..68 | C |
| CA-81 | `manual-persistence-commands` / save: determine pending set, normal transaction immediately with same IDs, no repeated resolution/costs/events/progression/inventory/relations/effects; if empty verify current target, no invented event | P; Y/Y/Y | F; TINT; CA-55..68; CA-88; CA-93 | C |
| CA-82 | `manual-persistence-commands` / status: only authorized marker/version/Save Point/strategy/authority/chain-sync or service-evidence/pending/local/cloud/failure facts | P; Y/Y/Y | V; U; CA-30; CA-71..79 | C |
| CA-83 | `manual-persistence-commands` / retry: earliest incomplete stage, inspect committed first; cloud-only continuation after valid local commit; Managed same Transaction/idempotency key; no replay/resources/events/second Save Point | P; Y/Y/Y | F; TINT; CA-77; CA-93..94 | C |
| CA-84 | `canonical-change-proof-and-unchanged-save-detector`: active profile requires sufficient version/revision/owner-row/chronology/Index/Save Point/artifact-hash/remote-sync/readback evidence | K; Y/Y/Y | R; F; U; Validation/provider contract | C |
| CA-85 | `canonical-change-proof-and-unchanged-save-detector`: unchanged expected Canon is failure even if call succeeds; preserve retry evidence, failure marker, forbid complete | K; Y/Y/Y | CA-55; CA-64; CA-84 | C |
| CA-86 | `canon-before-derived-context`: owner write -> authority validation/readback -> Running -> Session -> Scene -> marker -> turn complete | P; Y/Y/Y | F; TINT; CA-62..68 | C |
| CA-87 | `canon-before-derived-context`: failure leaves/rebuilds summaries from last validated point, never prepared narration | K; Y/Y/Y | F; CA-86; CA-91 | C |
| CA-88 | `no-change-turns`: explicit ephemeral/transactional empty-set provenance, no mutation/plumbing Timeline event; failed action can spend time/assets/harm/relations/knowledge/process progress | K; Y/Y/Y | F; TINT; CA-55..56 | C |
| CA-89 | `no-change-turns`: rendering is no mutation/new save; existing verified marker allowed; explicit later trait adoption has non-empty set | K; Y/Y/Y | CA-38..40; CA-78; CA-88 | C |
| CA-90 | `failure-and-retry` / Read: honest source-recovery/continuity/clarification/blocked state; preserve intent/evidence, no fabricated missing value or claimed Canon read from memory | P; C/C/Y | O; U; CONFLICT; CA-54; recovery owners | C |
| CA-91 | `failure-and-retry` / Write: determined result not committed transaction; keep validated parent, staged IDs/changes/version/failure evidence; no committed dependent delivery | P; C/C/Y | F; U; CA-57..68; CA-85 | C |
| CA-92 | `failure-and-retry` / Retry: retain identity, verify applied operations before writing | P; C/C/Y | F; TINT; CA-91 | C |
| CA-93 | `failure-and-retry` / No duplicates: chronology/resources/inventory/relationships/Skills/Development/project/research/autonomous/time/consequences | K; C/C/Y | CA-92; Save Update | C |
| CA-94 | `failure-and-retry` / Concurrency: changed parent means rebuild relevant Read Set and resolve conflict before retry | P; C/C/Y | O; F; CONFLICT; CA-48..54 | C |
| CA-95 | `next-turn-verification` / 1: active Save Index/version | P; Y/Y/Y | F; CA-30 | C |
| CA-96 | `next-turn-verification` / 2: invalidate parent-version mismatch packets | P; Y/Y/Y | CA-45..47; CA-95 | C |
| CA-97 | `next-turn-verification` / 3: query relevant owners | P; Y/Y/Y | O; CA-34; CA-50 | C |
| CA-98 | `next-turn-verification` / 4: confirm relevant expected prior-turn state | P; Y/Y/Y | F; CA-84..85; CA-97 | C |
| CA-99 | `next-turn-verification` / 5: rebuild scene from persisted records, not conversation/prior packet | P; Y/Y/Y | CA-35; CA-95..98 | C |
| CA-100 | `next-turn-verification`: missing supposedly saved change is continuity/persistence defect, not narrative workaround | K; C/C/Y | O; U; CONFLICT; CA-98 | C |
| CA-101 | `session-start-and-context-reset` / 1: Configuration/latest Save Index | P; Y/Y/Y | R; F; CA-30 | C |
| CA-102 | `session-start-and-context-reset` / 2: persisted strategy and Direct authority or Managed interface/binding | P; Y/Y/Y | R; CA-30; CA-101 | C |
| CA-103 | `session-start-and-context-reset` / 3: persisted summaries only caches | P; Y/Y/Y | F; CA-41..44 | C |
| CA-104 | `session-start-and-context-reset` / 4: verify parent/source/visibility/freshness | P; Y/Y/Y | V; CA-45..47; CA-103 | C |
| CA-105 | `session-start-and-context-reset` / 5: discard/regenerate stale packets | P; Y/Y/Y | CA-104 | C |
| CA-106 | `session-start-and-context-reset` / 6: relevant canonical Read Set | P; Y/Y/Y | CA-48..54; CA-105 | C |
| CA-107 | `session-start-and-context-reset` / 7: validated Save Point/open interaction only; no prior transcript required on fresh runtime | P; Y/Y/Y | F; AGENCY; CA-101..106 | C |
| CA-108 | `specialist-integrations` / Life Archive: stable Life IDs/detail paths, GM access not current-character recall | K; Y/Y/Y | F; V; Life Archive/Memory | C |
| CA-109 | `specialist-integrations` / Long-Horizon: period IDs/tags/hooks/references, only material intervals | K; Y/Y/Y | F; CA-34; Long-Horizon | C |
| CA-110 | `specialist-integrations` / Ownership: packet references owners, scene/Running/Session not competing stores | K; Y/Y/Y | O; F; Data Ownership | C |
| CA-111 | `specialist-integrations` / Autonomous: relevant ID/model/controller/autonomy/placement/assignment/condition/report/network/resources/maintenance only, not every unit | K; Y/Y/Y | F; U; Autonomous Registry | C |
| CA-112 | `specialist-integrations` / Memory: GM Context not Character Memory; FR-011 retrieves, does not implement continuity/fading/recall | K; Y/Y/Y | V; LIMIT; Memory Continuity | C |
| CA-113 | `specialist-integrations` / Companion: causal identity/reincarnation/encounter/separation/recognition/history materiality only; retrieval creates no bond, reveals no protected identity/route/timing | K; Y/Y/Y | V; U; CA-34; Soul-Bound Companions | C |
| CA-114 | `specialist-integrations` / Visual: Identity owns sparse established traits, Derived request composes form/model/assets/conditions/location/perspective; invalidation, no new appearance owner | K; Y/Y/Y | F; V; CA-38..40; Visual Identity/Context | C |
| CA-115 | `existing-campaign-adoption` / 1: back up latest canonical save | P; -/-/Y | F; GOV; Migration | A |
| CA-116 | `existing-campaign-adoption` / 2: validate active version/owner graph | P; -/-/Y | R; F; CA-115; Validation | A |
| CA-117 | `existing-campaign-adoption` / 3: audit/preserve valid existing owner queries | P; -/-/Y | O; CA-48..54; CA-116 | A |
| CA-118 | `existing-campaign-adoption` / 4: configure/regenerate caches, no Canon rewrite | P; -/-/Y | F; CA-41..47; CA-117 | A |
| CA-119 | `existing-campaign-adoption` / 5: enable automatic turn transactions in host/profile | P; -/-/Y | CA-57..70; CA-118 | A |
| CA-120 | `existing-campaign-adoption` / 6: verify adapters/validation/readback requirements | P; -/-/Y | CA-08..09; CA-119; Validation | A |
| CA-121 | `existing-campaign-adoption` / 8 and boundary: provenance, activation only after validation; repository does not migrate populated campaign | P; -/-/Y | GOV; TINT; LIMIT; CA-115..120; CA-129 | A |
| CA-122 | `development-context-and-auditability`: authorized IDs/Read Set/revisions/relevance/Affected owners/adapters/validation/readback/Save Point/cache outcomes | K; Y/Y/Y | V; F; diagnostic owner | C |
| CA-123 | `storage-neutral-logical-guidance`: equivalent Direct/Managed metadata/provenance structures optional, not a mandated format | I; -/-/- | CA-03..05; LIMIT; provider contract | T |
| CA-124 | `storage-neutral-logical-guidance` / packet fields: ID/key, scene-running-session kind constraint, interaction, non-null parent Version/Save Point/audience/provenance, optional game time, generation and fresh-stale-invalid constraint | I; -/-/- | CA-41..47; CA-123 | T |
| CA-125 | `storage-neutral-logical-guidance` / references: packet/owner/record composite key, optional revision/path, required relevance, parent foreign key | I; -/-/- | CA-35..36; CA-124 | T |
| CA-126 | `storage-neutral-logical-guidance` / index and content: parent Version/Save Point/freshness index; transient/separate content allowed, derivation references only, no copied-fact ownership; existing IDs/names allowed | I; -/-/- | CA-123..125; O; F | T |
| CA-127 | `mandatory-read-gate` / 5: expand initial Read Set through material Typed References | P; Y/Y/Y | CA-51; CA-135; State Model | C |
| CA-128 | `mandatory-read-gate` / 8: READ_COMPLETE only when every material dependency resolved or validly bounded | P; Y/Y/Y | CA-48..54; CA-127; U | C |
| CA-129 | `existing-campaign-adoption` / 7: harmless fixture/isolated validation transaction covering read/write/rollback/reload | P; -/-/Y | CA-08..09; CA-115..120; Validation | A |
| CA-130 | `gameplay-turn-state-machine`: verified explicit empty set skips write via VALIDATED; determined no-op, not assumption | P; Y/Y/Y | CA-55..56; CA-88 | C |
| CA-131 | `gameplay-turn-state-machine`: prepared narration not delivered durable consequence; strict ordered authority verification; no background/async/decorative/later-save promise | K; Y/Y/Y | F; U; CA-28; CA-57..68 | C |
| CA-132 | `gameplay-turn-state-machine`: TURN_COMPLETE/final output invalid absent required write/validation/readback/expected-change evidence | K; Y/Y/Y | CA-131; CA-84..85 | C |
| CA-133 | `manual-persistence-commands` / status privacy: no credentials/private locators/GM Secrets/unrelated campaign data | K; Y/Y/Y | V; CA-82; diagnostics authorization | C |
| CA-134 | `development-context-and-auditability`: debug obeys visibility/Secrets; ordinary successful plumbing omitted unless asked or actionable Operational Failure | K; Y/Y/Y | V; CA-78; CA-122 | C |
| CA-135 | `current-scene-context` / References: each hop identifies authoritative domain; reference paths grant neither visibility nor Character Knowledge | K; Y/Y/Y | PA-34; V; CA-35; Truth Layers | C |
| CA-S01 | `safeguards` / 1: no whole-campaign default load | K; Y/Y/Y | CA-32..34 | C |
| CA-S02 | `safeguards` / 2: no material dependency omission to shorten context | K; Y/Y/Y | CA-33..34; CA-53 | C |
| CA-S03 | `safeguards` / 3: no narrative-summary-only save | K; Y/Y/Y | F; CA-60..68 | C |
| CA-S04 | `safeguards` / 4: no duplicate current-state owner caches | K; Y/Y/Y | CA-13; CA-110 | C |
| CA-S05 | `safeguards` / 5: failed action not automatically unchanged | K; Y/Y/Y | CA-56; CA-88 | C |
| CA-S06 | `safeguards` / 6: no asynchronous/background save promise | K; Y/Y/Y | CA-29; CA-68 | C |
| CA-S07 | `safeguards` / 7: no ordinary storage/protected IDs | K; Y/Y/Y | V; CA-36; CA-78 | C |
| CA-S08 | `safeguards` / 8: GM retrieval grants no Character Knowledge | K; Y/Y/Y | V; CA-37 | C |
| CA-S09 | `safeguards` / 9: no FR-015/016 implementation here | K; Y/Y/Y | CA-112..113; LIMIT | C |
| CA-S10 | `safeguards` / 10: no populated packets/campaign fixtures in repository | K; Y/Y/Y | CA-07; LIMIT | C |

### Explicit Host Conformance Predicates

These are not erased or weakened by removing test text from a runtime Rule Source. Each original H3 case remains verbatim in T, with its listed owning predicates. A host claiming conformance must still pass all applicable cases. This separation does not make a mode-specific required test optional.

| Clause | Current anchor/unit and complete test obligation | Role; G/M/X | Requires | Destination |
| --- | --- | --- | --- | --- |
| CA-R01 | `regression-cases` / A: read Skill identity/Development/embodiment/access/condition/opposition before resolving | H; -/-/- | CA-48..54; Skills/Development | T |
| CA-R02 | `regression-cases` / B: automatic body-owner save, validate, next-turn read, no save command | H; -/-/- | CA-55..68; CA-95..100 | T |
| CA-R03 | `regression-cases` / C: promise updates Relationship in same transaction, summaries reference only | H; -/-/- | CA-55..68; CA-110 | T |
| CA-R04 | `regression-cases` / D: movement/item changes all four listed owners, one interaction/complete transaction/effect once | H; -/-/- | CA-55..68; CA-93 | T |
| CA-R05 | `regression-cases` / E: accepted autonomous assignment updates Registry/Project, irrelevant units excluded | H; -/-/- | CA-55; CA-111 | T |
| CA-R06 | `regression-cases` / F: no new perception/time -> explicit empty set, no mutation | H; -/-/- | CA-88 | T |
| CA-R07 | `regression-cases` / G: validation failure keeps parent active, no saved completion, same-ID retry | H; -/-/- | CA-91..94 | T |
| CA-R08 | `regression-cases` / H: consumed inventory owner beats conversation, regenerate packet | H; -/-/- | O; F; CA-47; CA-54 | T |
| CA-R09 | `regression-cases` / I: next-turn persisted changed Location ID before resolution | H; -/-/- | CA-95..100 | T |
| CA-R10 | `regression-cases` / J: old-version Running Summary invalidated/rebuilt, no Canon overwrite | H; -/-/- | CA-45..47 | T |
| CA-R11 | `regression-cases` / K: historical companion query follows Entity/Life/Period/Relationship/source only as needed | H; -/-/- | CA-34; CA-44; CA-108..109 | T |
| CA-R12 | `regression-cases` / L: transcript-free reset restores Index/summary checks/Read Set/persisted state | H; -/-/- | CA-101..107 | T |
| CA-R13 | `regression-cases` / M: all listed infrastructure/research/equipment/network/autonomous/knowledge/chronology/history changes read, once persisted, proved/reloaded; correct saved marker; narration-only fails | H; -/-/- | CA-55..68; CA-71..79; CA-84..85; CA-95..100 | T |
| CA-R14 | `regression-cases` / N: absent local path -> exact configured remote fetch/identity-version verification/working copy/chain, no false missing/blank replacement | H; -/-/- | CA-30..31 | T |
| CA-R15 | `regression-cases` / O: local SQLite commit/validation/read-only reopen/change proof/activation/local marker | H; -/-/- | CA-62..66; CA-72; Direct SQLite | T |
| CA-R16 | `regression-cases` / P: validated SQLite candidate/Drive exact replacement/remote comparison/required backup/cloud marker | H; -/-/- | CA-65; CA-73; CA-77; Drive | T |
| CA-R17 | `regression-cases` / Q: incomplete cloud pending blocks next change; failed cloud retains validated local candidate, not active authority | H; -/-/- | CA-74; CA-77; CA-79 | T |
| CA-R18 | `regression-cases` / R: upload without verified readback cannot cloud-save/close turn, retain failure evidence | H; -/-/- | CA-64..68; CA-73..77 | T |
| CA-R19 | `regression-cases` / S: save pending once, no replay; status authorized marker/version/authority/sync/pending/verified boundaries | H; -/-/- | CA-80..83 | T |
| CA-R20 | `regression-cases` / T: cloud retry same transaction, no repeated owner writes; saved only after remote verification | H; -/-/- | CA-77; CA-83 | T |
| CA-R21 | `regression-cases` / U: narrated non-empty set but unchanged expected owner/chronology/version/artifact -> failure marker, no turn complete | H; -/-/- | CA-84..85 | T |
| CA-R22 | `regression-cases` / V: Managed stages/validates/activates/readbacks multi-domain, expected-ID/version receipt, local-style marker/no backend disclosure | H; -/-/- | CA-57..68; CA-75 | T |
| CA-R23 | `regression-cases` / W: interface returns no validated receipt -> pending/failure/no complete, original-ID retry/no replay | H; -/-/- | CA-65; CA-74..76; CA-83 | T |
| CA-R24 | `regression-cases` / X: DuckDB concurrency conflict fails candidate/reloads parent, no last-writer-wins/false saved | H; -/-/- | CA-91..94; Direct DuckDB | T |
| CA-R25 | `regression-cases` / Y: World A Kernel/Core/World-operation-topic rules plus Campaign A Canon, World B/Campaign B excluded; provenance and 8K target | H; -/-/- | CA-33; R; Retrieval/Truth Layers | T |
| CA-A01 | `acceptance-criteria` / 1: no ordinary manual-save requirement | H; -/-/- | CA-02; CA-80 | T |
| CA-A02 | `acceptance-criteria` / 2: every non-empty set automatically persists | H; -/-/- | CA-15 | T |
| CA-A03 | `acceptance-criteria` / 3: material reads before resolution | H; -/-/- | CA-11; CA-48..54 | T |
| CA-A04 | `acceptance-criteria` / 4: conversation never replaces owner read | H; -/-/- | CA-10; CA-54 | T |
| CA-A05 | `acceptance-criteria` / 5: explicit failed reads/writes | H; -/-/- | CA-17 | T |
| CA-A06 | `acceptance-criteria` / 6: later turn reads committed prior change when material | H; -/-/- | CA-19 | T |
| CA-A07 | `acceptance-criteria` / 7: Derived cannot override Canon | H; -/-/- | CA-13 | T |
| CA-A08 | `acceptance-criteria` / 8: internal relevant IDs/owners/drill-down references | H; -/-/- | CA-35..36 | T |
| CA-A09 | `acceptance-criteria` / 9: relevance-filtered selection | H; -/-/- | CA-32..34 | T |
| CA-A10 | `acceptance-criteria` / 10: fresh sessions rebuild from persistence | H; -/-/- | CA-101..107 | T |
| CA-A11 | `acceptance-criteria` / 11: target resolved before changing play, not guessed from one path | H; -/-/- | CA-30..31 | T |
| CA-A12 | `acceptance-criteria` / 12: non-empty set proves expected change before complete | H; -/-/- | CA-84..85 | T |
| CA-A13 | `acceptance-criteria` / 13: local saved only after validated readback | H; -/-/- | CA-72 | T |
| CA-A14 | `acceptance-criteria` / 14: cloud saved only after required sync/remote verification | H; -/-/- | CA-73 | T |
| CA-A15 | `acceptance-criteria` / 15: Managed saved only receipt/no backend disclosure | H; -/-/- | CA-75 | T |
| CA-A16 | `acceptance-criteria` / 16: persisted explicit Direct/Managed selection/reuse, no silent cross-mode fallback | H; -/-/- | CA-30; CA-102; Portable Persistence | T |
| CA-A17 | `acceptance-criteria` / 17: pending/unresolved failure blocks next changing play | H; -/-/- | CA-79 | T |
| CA-A18 | `acceptance-criteria` / 18: all three manual commands idempotent | H; -/-/- | CA-80..83 | T |
| CA-A19 | `acceptance-criteria` / 19: Derived refresh after verification | H; -/-/- | CA-86..87 | T |
| CA-A20 | `acceptance-criteria` / 20: configured-mode host tests exercise A-U | H; -/-/- | CA-R01..21; CA-09 | T |
| CA-A21 | `acceptance-criteria` / 21: mode-specific host tests exercise V-X | H; -/-/- | CA-R22..24; CA-09 | T |
| CA-A22 | `acceptance-criteria` / 22: retrieval tests exercise Y/provenance/8K-world isolation | H; -/-/- | CA-R25 | T |
| CA-A23 | `acceptance-criteria` / 23: repository mock-adapter state-machine harness passes | H; -/-/- | CA-08..09; FR-011 harness | T |

## Routine Subset Review

The prior O/F/V/U/AGENCY/TINT section subset totals 2,572. It lacks **Purpose 177, Repository Canon 415, Campaign Canon 384, rules/persistence/Codex distinction 407, conflict classification 462, safeguards 288, scope boundaries 182, title 8, and layer heading 8**: additions total **2,331**, yielding **4,903**. These are existing section estimates, not newly authored/recompiled bytes.

The missing rules are not just editor framing. Ordinary readers must distinguish reusable mechanics from campaign premises and facts, recorded profile from available version, private Codex design from campaign adoption, validated current state from pending event, and actor choice from recorded intent. Conflict classification is needed by freshness, mandatory reads, incomplete-save recovery and concurrency retry. Numerical changes require an owned event/correction. Visibility and uncertainty have specialist owners. These cannot be dropped merely because an invocation usually succeeds.

GM adjudication requires O/R/F/U/TINT and the GM Framework; GM Secrets require O/R/F/V/U and Truth Layers, not a special secret-authority rank; population/ecology requires O/R/F/U/AGENCY/TINT and World Engine owners. The retained full P conservatively supplies all these authority clauses. This does not relocate their specialist mechanics into authority prose.

**4,903 is a conservative complete-section estimate, not a proven semantic minimum.** It is not a certificate that every possible direct authority query carries all needed intra-source clauses: format 1 selects root snippets but includes complete *dependency* sources. In particular, a directly matched conflict procedure must not execute from its small matched-root packet while assuming unselected authority text. A consumer must explicitly require the necessary complete authority source through A's existing request contract, or stop for complete guidance. R2 acceptance must resolve/test this use case rather than silently introducing self-dependencies or relying on a link as executable text. No claim that the old direct-authority fixtures establish that guarantee is made.

## Specialist and Context Boundaries

**G:** upward proposals, revision adoption and retcons (PA-45..46, PA-58..62). These execute higher-authority changes, not ordinary facts. G depends on P; P states the constraints and routes to an owning process, but does not itself authorize/execute G. On encountering a proposed retcon or rules adoption, routine play stops that branch and loads complete specialist guidance before executing it. Identifying a discrepancy as a potential migration is not performing migration. Retcon additionally requires the dedicated continuity/migration and campaign authorization contracts.

**Conflict classification stays in P.** It is not an adoption-only specialist: ordinary freshness/read/retry can encounter contradictions. A separate conflict source required by C would simply add the same 462 to every complete context dependency closure. Removing it from that closure without a redesigned, explicitly gated execution boundary would leave a hidden assumption. Actual exceptional correction remains owner-routed and may require G; ordinary correction does not grant retcon permission.

**Manual save stays in C.** Its procedure is the same ordinary persistence transaction and failure/retry path; moving it out offers no genuinely different authority ancestor. Status/privacy and transaction identity remain needed even with no pending change.

**A:** only existing-campaign adoption (CA-115..121) genuinely requires both C's complete read/write contract and G's migration authority. A therefore depends on **C and G**, not merely a cheap authority summary. Routine C does not depend on A or G. A may be absent from ordinary gameplay safely because initiating a rules-profile adoption is not an ordinary player action. Current-profile reading and authorized appearance-trait adoption remain in C: the latter is an ordinary owner-routed fact change, not repository-profile migration.

**Q/T:** preserve every example, A-Y conformance case, acceptance predicate and optional Cache schema. They validate implementation and interpret owning rules; they are not every-turn instructions. PA-63's no hidden recovery follows valid established event requirements (PA-06/21/26/30), explicit uncertainty (PA-39) and correction routing. PA-64 follows Session recovery, typed owner/dependent validation; PA-65 follows claim/truth/observer distinction; PA-66 follows profile adoption and history preservation; PA-67 follows authority-versus-visibility/status/classification. These are explicit entailments, not deletion of inconvenient authority. If later authoring reveals an example adds an independent obligation, that obligation must remain executable in its owning source and its cost must be counted. Q is not permission to put a needed unique rule outside the manifest.

Storage guidance remains an optional provider implementation contract; all its field/null/key/foreign-key/index constraints remain in T if a provider implements that illustrative structure. Actual current schema and provider authorization are unchanged. No other C split is recommended merely to make the arithmetic smaller.

## Manual Save Decision

Canonical meaning is already explicit: **manual persistence override/recovery**, not an in-world action, general progression concept, or permission escalation. `save` flushes the current pending transaction using original identities; no pending work means verify current authority without an invented save event. `save status` discloses authorized bounded operational evidence; `retry save` resumes the incomplete stage without re-resolution or repeated effects. Automatic turn persistence remains mandatory.

Guidance legitimately applies to `gameplay.resolve`, `context.assemble`, and `persistence.commit` as presently declared for C. Operation-qualified query `save` during gameplay may retrieve those semantics; it does not create another player input, authorize SQL, activate another campaign, or bypass host-bootstrap/read/write gates. A separate save-only operation is not defined here. An actual authoritative `save status` read uses the existing service/adapter authorization boundary; retrieval eligibility is not tool capability.

The current gameplay fixture failure is therefore an incompatible dependency, **not desired rejection of a legitimate manual override**. The combined query has genuine multiple concepts; a packet lacking manual guidance is not proof that the manual command was fulfilled. If an executable task requires that guidance, existing explicit required-root/source requests must fail honestly when it cannot fit. R1 does not edit any fixture/request to obtain a better result.

## Source Graph and Applicability

All four proposed executable sources: layer Core, priority 300, `alwaysInclude=false`, preparation tier CampaignBootstrap. Do not change these merely to alter ordering. Empty world/module lists mean no world/module restriction; all modes `[*]`. Authority/persistence boundaries apply to every world, optional module and Normal/testing mode; restricting them would exclude an ancestor needed to interpret facts. Topics are exact filtering declarations, not fuzzy matching.

| Source | Material/responsibility | Operations | Topics | Complete dependencies | Consumers | Estimate |
| --- | --- | --- | --- | --- | --- | ---: |
| P `core-persistence-authority` | All present sections except upward/revision/retcon and worked-example/navigation companions | `context.assemble`, `gameplay.resolve`, `persistence.read`, `persistence.commit` | authority, canon, persistence | None; internally interpret full P when authority execution requires it | gameplay/context/manual read-save-retry/conflict; G/A | 4,903 |
| C `core-context-assembly` | All present runtime material except existing-campaign adoption and host/schema conformance | `context.assemble`, `gameplay.resolve`, `persistence.commit` | context, persistence, read-set, affected-set | P | gameplay/read/save/status/retry/scene/debug/reset | 10,555 |
| G `core-authority-governance` | Upward proposals, repository revisions, authorized retcons; new H1 `Authority Governance` | `context.assemble`, `persistence.read`, `persistence.commit` | authority, canon, persistence | P | campaign owner/migration/correction/repository-review branches; A | 621 |
| A `core-context-adoption` | Existing-campaign adoption; new H1 `Context Assembly Adoption` | `context.assemble`, `persistence.commit` | context, persistence, read-set, affected-set | C, G | authorized adoption/configuration/validation host | 254 |

G/A exclude gameplay because they **execute** profile/governance changes. Reusing established operations does not authorize a player to do so. Repository review itself has no invented MCP operation. Core P's new gameplay applicability is justified by required authority, not blanket activation of exceptional procedures. C remains query-selected; its full 10,555 is *not* mandatory for every selected C snippet. A's edge to C is complete and honestly costly. P's 19 snippets, C's 28, G's four (new title plus three H2s), A's two (new title plus one H2) are proposed counts, not a compiled artifact result.

G estimate = 184+207+222+8 new-title units = 621; A =245+9 new-title units =254. Counts use existing normalized executable text and hypothetical title text only. Links/control cross-reference authoring could increase costs. C =13,894-245-476-2,016-602=10,555. P =6,385-184-207-222-696-173=4,903. Companion content is preserved, but has no manifest selectors/dependencies/snippet identity. Sources remain ordinary files.

```text
existing Kernel / GM Runtime Procedure -> Kernel (unchanged)

selected C root -> complete P
G -> complete P
A -> complete C -> complete P
  -> complete G -> complete P (charged once)

P conditional governance route -> stop that branch;
  obtain G/owning procedure before executing authorized change

Q/T -> owner-clause references for conformance; not executable graph edges
```

No C->G, C->A, P->G or P->C edge. The DAG is acyclic and uses only validated source IDs; no snippet edge, self-edge, path-as-ID or optional undeclared structural property. Ordinary C dependency closure includes all routine authority/conflict text but not adoption/retcon procedure. The route/stop boundary must remain explicit in R2 authoring, not an implicit exception to dependency inclusion.

## Authority Completeness Review

| Context obligations | Authority obligations reachable through proposed dependencies | Additional dedicated owner when material |
| --- | --- | --- |
| CA-01..09, CA-21, CA-32..33: ownership, host/rule/Campaign distinction | C->P: O/R/F/LIMIT | State/Data Ownership/AI Runtime, Rule Compilation, provider authorization |
| CA-10..19, CA-22..24, CA-28..31: state, target, ordered turn/durability | C->P: O/R/F/U/TINT/LIMIT | Configuration/Save Index/Save Update/Save Point/Validation |
| CA-20, CA-25, CA-34..40, CA-108..114, CA-122: relevance/disclosure/depiction/specialists | C->P: O/R/F/V/U/AGENCY/LIMIT | Truth Layers/Memory/Visual Identity/Archive/Autonomous/Soul Companions |
| CA-26..29: real-input/choice/yield and adjudication | C->P: AGENCY/O/R/F/U | GM Framework and mandatory GM Runtime Procedure |
| CA-41..54, CA-90, CA-94..107: caches, reads, boot, gap/stale/concurrency correction | C->P: O/R/F/V/U/CONFLICT/LIMIT | State Model/Continuity Resolution/Source Recovery |
| CA-55..70, CA-84..89, CA-91..93: affected owners, world consequences, patches, save/no-op/readback | C->P: O/R/F/U/TINT/AGENCY/CONFLICT/LIMIT | World Engine and each changed specialist owner, normal mutation/provider contract |
| CA-71..83: truthful local/cloud/Managed evidence and manual recovery | C->P: O/R/F/V/U/TINT/CONFLICT/LIMIT | persistence strategy/provider completion/diagnostic authorization |
| CA-115..121: adoption | A->C->P plus A->G->P: every above obligation plus GOV | Migration/Versioning, campaign authorization, Validation |
| CA-S01..10: safeguards | C->P plus listed owning C predicates; no safeguard omitted | listed specialist owners |
| CA-123..126, CA-R01..25, CA-A01..23 | T references all corresponding C/A/P/G predicates, preserved complete host contract | implementation/conformance, not player execution |
| CA-127..135: distinct read/closure, adoption test, no-op, completion and disclosure predicates | C->P for routine predicates; A->C/G->P for CA-129, preserving the same authority bundles above | Read Set/Validation/Truth Layers/diagnostics owners |

This proves coverage/reachability for selected C/A **with the stated branch stop**. It does not prove every specialist subsystem is itself fully materialized by these two sources: Truth Layers, World Engine, actual Save Update and Migration mechanics remain their owners. A material specialist action still requires its rules. The six fixtures use the existing requested roots, not a fabricated assertion that two foundation documents contain all game mechanics. Direct P-root intra-source completeness and additional necessary specialist roots are remaining acceptance constraints. This is why a responsibility map is not a fully certified executable/budget architecture.

## Six Fixture Projections

Arithmetic only: normalized UTF-8 snippet estimator `max(1,(bytes+2)/3)` with integer division, distinct members charged once. Existing golden root scores/order and mandatory floor **2,027** (11 Kernel/GM snippets) are unchanged. `context.assemble` floor is **1,010**. These are not new A-E runs, emitted artifacts or SQL acceptance. Costs exclude new authored framing, packet envelopes, Campaign Canon and external tokenizer/whole-runtime costs. All six remain NORMAL/world-neutral/module-neutral gameplay queries with the existing explicit topic union.

| Fixture | Requested roots outside floor | Full intended closure incl. floor | Total | 8K headroom | Projected E admission at 8K |
| --- | --- | --- | ---: | ---: | --- |
| save | C `manual-persistence-commands` 576 | C root + complete P 4,903 | 7,506 | 494 | Root fits; full authority, no G/A |
| combined four terms | four Development 2,719; manual 576; three Evolution 1,403; five conflict-authority hits already within P | all requested roots + complete P | 11,628 | -3,628 | 7,545 used, 455 left; manual root excluded, so not fulfilled |
| GM adjudication | C `gameplay-turn-state-machine` 645; matched GM roots already in floor | C root + complete P | 7,575 | 425 | C root fits; no second charge for GM |
| player agency | C `core-invariants` 611 and `gameplay-turn-state-machine` 645; matched GM/Kernel already in floor; PA actor 203 within P | both C roots + complete P | 8,186 | -186 | 7,541 used, 459 left; state-machine root excluded |
| GM Secrets visibility | C scene 546, visual 333, relevance 403; World Engine `architectural-role` 144; PA visibility 208 within P | all three C roots + world root + complete P | 8,356 | -356 | 7,953 used, 47 left; relevance root excluded |
| population/ecology | C `resolution-and-affected-set-gate` 330; World Engine `core-domains` 377, `shared-state-model` 82 | C root + both world roots + complete P | 7,719 | 281 | All requested roots fit |

All C-root closures require O/R/F/U/TINT as applicable; agency adds AGENCY; visibility adds V; ecology preserves uncertainty and actor boundaries while routing resolved consequences to world owners. None requests a retcon/adoption, so none needs G/A. Full P nevertheless includes conflict classification because a routine read/integration cannot assume contradictions impossible. If actual resolution enters exceptional governance, stop and retrieve its complete rules rather than claim these projections cover it.

Current behavior for all six is `DependencyFailed`; widening the full old source yields at least **8,412 before any C root**. Current widened full unions are respectively 8,988 / 13,110 / 9,057 / 9,668 / 9,838 / 9,201. New projections reduce those complete unions by **1,482**, not by assuming away authority. New directly applicable agency/visibility hits are already members of the required P closure once a C root fits.

Combined admission detail: existing four 1,300-point Development roots cost 2,719 after required floor. The 600-point manual root would add **5,479**, which does not fit remaining 3,254. Five newly gameplay-applicable conflict-authority hits cost 1,396 without a C root; three Evolution roots cost 1,403. They fit: 2,027+2,719+1,396+1,403=7,545. This is the same optional-root omission danger identified in the prior widening experiment, not a repaired save request. A direct conflict-procedure hit alone also does not prove full authority execution readiness.

The earlier **7,894/8,000, 106 remaining** was a fixed-root projection with a 2,572 subset, excluding new direct hits/framing. Adding missing 2,331 to that *same admitted union* gives **10,225, -2,225 headroom**. The new **complete combined union** is **11,628**; the greedy packet is smaller only by dropping manual guidance. These three numbers measure different sets. Do not substitute 7,545's apparent spare capacity for requested-guidance acceptance. No budget/rank/request alteration is proposed to conceal this.

## Broader Regression Projections

| Query / operation | Floor | Query roots and dependencies | Total | 8K remaining | Current comparison |
| --- | ---: | --- | ---: | ---: | --- |
| empty / gameplay | 2,027 | None; no new always-include source | 2,027 | 5,973 | unchanged |
| unknown / gameplay | 2,027 | None; no generated/fuzzy vocabulary | 2,027 | 5,973 | unchanged |
| fighting / gameplay | 2,027 | four Development roots 2,719; no C/authority hit | 4,746 | 3,254 | unchanged |
| conflict / gameplay | 2,027 | four Development 2,719 plus five P conflict hits 1,396; no whole-P dependency from a C root | 6,142 | 1,858 | old 4,746; five previously inapplicable hits become applicable deliberately |
| preparation / gameplay | 2,027 | three Evolution roots 1,403 | 3,430 | 4,570 | unchanged |
| Campaign Canon / context.assemble | 1,010 | P `campaign-canon` 384 + `rules-authority-and-persistence-authority` 407 | 1,801 | 6,199 | unchanged selected text/score, no G edge |
| Campaign Canon / gameplay | 2,027 | same two newly applicable P roots 791 | 2,818 | 5,182 | old 2,027 with those hits inapplicable; deliberate applicability impact |

All above costs are retrieval projections, not permission to act from incomplete specialist/direct-authority context. Context Kernel is not interchangeable with gameplay floor. Requiring **whole P** explicitly costs 6,930 with gameplay floor or 5,913 with context floor. A complete adoption source request costs **1,010+10,555+4,903+621+254=17,343** under the proposed source graph; it cannot be certified at 8K merely by classifying it exceptional. Its full C dependency is semantically justified and not removed to improve the chart. No larger supported budget, partial dependency or format extension is introduced.

## Identity Migration Map

Every current source/snippet, including title-only/navigation text, has one disposition below. `C#anchor` means the full `core-context-assembly#anchor`; `P#anchor` means `core-persistence-authority#anchor`. "Preserved" is logical ID/anchor, **not unchanged source SHA**. Exact source hashes change for any future edited file. Body/content hash remains only if normalized snippet text is preserved. New link/framing changes must be audited rather than silently called identical.

| Current source C snippet/anchor | Proposed identity | Disposition |
| --- | --- | --- |
| `(unanchored)` | C unanchored | Preserved title ID |
| `purpose` | C#purpose | Preserved |
| `document-control` | C#document-control | Preserved ID; later references updated explicitly |
| `runtime-boundary` | C#runtime-boundary | Preserved ID; conformance links updated explicitly |
| `core-invariants` | C#core-invariants | Preserved |
| `gameplay-turn-state-machine` | C#gameplay-turn-state-machine | Preserved |
| `persistence-target-resolution-gate` | C#persistence-target-resolution-gate | Preserved |
| `context-assembly-layer` | C#context-assembly-layer | Preserved |
| `relevance-selection` | C#relevance-selection | Preserved |
| `current-scene-context` | C#current-scene-context | Preserved |
| `purpose-specific-visual-context` | C#purpose-specific-visual-context | Preserved |
| `hierarchical-context` | C#hierarchical-context | Preserved |
| `freshness` | C#freshness | Preserved |
| `mandatory-read-gate` | C#mandatory-read-gate | Preserved |
| `resolution-and-affected-set-gate` | C#resolution-and-affected-set-gate | Preserved |
| `automatic-persistence-gate` | C#automatic-persistence-gate | Preserved |
| `player-visible-persistence-status` | C#player-visible-persistence-status | Preserved |
| `manual-persistence-commands` | C#manual-persistence-commands | Preserved |
| `canonical-change-proof-and-unchanged-save-detector` | C#canonical-change-proof-and-unchanged-save-detector | Preserved |
| `canon-before-derived-context` | C#canon-before-derived-context | Preserved |
| `no-change-turns` | C#no-change-turns | Preserved |
| `failure-and-retry` | C#failure-and-retry | Preserved |
| `next-turn-verification` | C#next-turn-verification | Preserved |
| `session-start-and-context-reset` | C#session-start-and-context-reset | Preserved |
| `specialist-integrations` | C#specialist-integrations | Preserved |
| `existing-campaign-adoption` | core-context-adoption#existing-campaign-adoption | Moved; source/snippet identity intentionally changed, anchor preserved |
| `development-context-and-auditability` | C#development-context-and-auditability | Preserved |
| `storage-neutral-logical-guidance` | T document, same anchor | Retired runtime identity; implementation contract preserved |
| `regression-cases` | T document, same anchor and A-Y H3s | Retired runtime identity; conformance preserved |
| `acceptance-criteria` | T document, same anchor | Retired runtime identity; conformance preserved |
| `safeguards` | C#safeguards | Preserved |
| `related-documents` | C#related-documents | Preserved ID; later navigation updates counted |

| Current source P snippet/anchor | Proposed identity | Disposition |
| --- | --- | --- |
| `(unanchored)` | P unanchored | Preserved title ID |
| `purpose` | P#purpose | Preserved |
| `core-rule` | P#core-rule | Preserved |
| `authority-is-claim-specific` | P#authority-is-claim-specific | Preserved |
| `the-authority-layers` | P#the-authority-layers | Preserved heading-only ID |
| `repository-canon` | P#repository-canon | Preserved |
| `campaign-canon` | P#campaign-canon | Preserved |
| `historical-record` | P#historical-record | Preserved |
| `current-campaign-state` | P#current-campaign-state | Preserved |
| `current-session` | P#current-session | Preserved |
| `current-narration` | P#current-narration | Preserved |
| `rules-authority-and-persistence-authority` | P#rules-authority-and-persistence-authority | Preserved |
| `authority-does-not-equal-visibility` | P#authority-does-not-equal-visibility | Preserved |
| `authority-does-not-equal-certainty` | P#authority-does-not-equal-certainty | Preserved |
| `authority-does-not-equal-actor-control` | P#authority-does-not-equal-actor-control | Preserved |
| `valid-downward-updates` | P#valid-downward-updates | Preserved |
| `valid-upward-proposals` | core-authority-governance#valid-upward-proposals | Moved; source/snippet changed, anchor preserved |
| `conflict-classification-procedure` | P#conflict-classification-procedure | Preserved |
| `repository-revision-changes` | core-authority-governance#repository-revision-changes | Moved; source/snippet changed, anchor preserved |
| `authorized-retcons` | core-authority-governance#authorized-retcons | Moved; source/snippet changed, anchor preserved |
| `worked-examples` | Q document, same anchor and five H3s | Retired runtime identity; interpretive conformance preserved |
| `safeguards` | P#safeguards | Preserved |
| `scope-boundaries` | P#scope-boundaries | Preserved |
| `related-documents` | Q document, same anchor | Retired runtime identity; navigation preserved |

Source identities C/P are **split** responsibilities with retained routine IDs/paths, new A/G owners, Q/T non-runtime companions. Across the 56 current snippets: **47 preserved, four moved with new source identity, five retired runtime IDs with preserved documentation**. Two new unanchored A/G title snippet IDs would be introduced. Whole artifact source/snippet counts cannot be claimed until actual authoring/compiler validation. C->P stays; new A->C, A->G, G->P edges are intentional; no other current manifest source names C/P as a dependency. P gameplay applicability changes intentionally. Nothing is migrated in this review.

## Complete Vocabulary Impact Map

All 42 current bindings targeting C/P are snippet-specific; none is source-wide. These carry 78 concept references to 31 distinct definitions. Existing binding records have exact targets rather than invented independent binding GUIDs. Preserve concept definitions, normalized terms, kinds, weights, rationales and origin meaning. Map targets only; no curation/ranking change is justified by this review.

| Current source/anchor | Current reviewed concepts | Proposed target / disposition |
| --- | --- | --- |
| C / purpose | context-assembly, automatic-persistence | unchanged |
| C / runtime-boundary | repository-canon, rule-gap | unchanged |
| C / core-invariants | automatic-persistence, read-set, player-agency | unchanged |
| C / gameplay-turn-state-machine | gameplay-resolution, automatic-persistence, player-agency | unchanged |
| C / persistence-target-resolution-gate | campaign-binding | unchanged |
| C / session-start-and-context-reset | campaign-binding | unchanged |
| C / context-assembly-layer | context-assembly, rule-retrieval, compiled-rule-context, rule-dependencies | unchanged |
| C / relevance-selection | context-assembly, information-visibility | unchanged |
| C / current-scene-context | context-assembly, current-state, information-visibility | unchanged |
| C / purpose-specific-visual-context | visual-context, information-visibility | unchanged |
| C / hierarchical-context | derived-context | unchanged |
| C / freshness | derived-context | unchanged |
| C / canon-before-derived-context | derived-context | unchanged |
| C / mandatory-read-gate | read-set, uncertainty, rule-dependencies, rule-gap | unchanged |
| C / resolution-and-affected-set-gate | affected-set, world-state | unchanged |
| C / automatic-persistence-gate | automatic-persistence, affected-set, transaction-commit | unchanged |
| C / player-visible-persistence-status | save-status, save-evidence | unchanged |
| C / manual-persistence-commands | manual-save, save-status, save-retry | unchanged |
| C / canonical-change-proof-and-unchanged-save-detector | save-evidence | unchanged |
| C / no-change-turns | no-change-turn, visual-context | unchanged |
| C / failure-and-retry | save-retry, rule-gap | unchanged |
| C / next-turn-verification | save-evidence, read-set | unchanged |
| C / specialist-integrations | memory-continuity, visual-context, historical-record | unchanged |
| C / existing-campaign-adoption | rules-migration | core-context-adoption / same anchor; moved |
| C / development-context-and-auditability | audit-context | unchanged |
| P / purpose | repository-canon, canon-conflict | unchanged |
| P / core-rule | repository-canon, canon-conflict | unchanged |
| P / authority-is-claim-specific | repository-canon, canon-conflict | unchanged |
| P / repository-canon | repository-canon | unchanged |
| P / campaign-canon | campaign-canon, campaign-binding | unchanged |
| P / historical-record | historical-record | unchanged |
| P / current-campaign-state | current-state, historical-record | unchanged |
| P / current-session | affected-set, transaction-commit | unchanged |
| P / current-narration | canon-conflict | unchanged |
| P / conflict-classification-procedure | canon-conflict | unchanged |
| P / rules-authority-and-persistence-authority | repository-canon, campaign-canon | unchanged |
| P / authority-does-not-equal-visibility | information-visibility | unchanged |
| P / authority-does-not-equal-certainty | uncertainty | unchanged |
| P / authority-does-not-equal-actor-control | player-agency | unchanged |
| P / valid-downward-updates | historical-record, transaction-commit | unchanged |
| P / repository-revision-changes | rules-migration | core-authority-governance / same anchor; moved |
| P / authorized-retcons | retcon, historical-record | core-authority-governance / same anchor; moved |

**39 unchanged targets, three moved targets, no renamed anchor.** `valid-upward-proposals` is unbound; retired companion sections have no reviewed binding to redirect. No example/test/schema text is replaced by a cheaper unrelated rule. New title snippets acquire no invented terms. Although association *meaning* remains, applicability/topology intentionally changes: previously inapplicable conflict/agency/visibility/Campaign Canon hits may now be directly eligible for gameplay. Preserve that evidence instead of suppressing aliases to recover budget. Positive/negative fixture targets and direct-authority completeness must be reviewed in a later authorized candidate; old goldens stay historical.

## Historical Compatibility and Format-1 Gate

Existing imports/exact bytes/projections/digests remain immutable and valid as historical format-1 identities. New source bytes, source IDs for moved snippets, selectors, dependencies and binding targets necessarily produce new manifest/source provenance and semantic/serialized artifact identities. A logically preserved snippet ID is not permission to mutate a historical row. Migration 011's artifact-owned ordinals/dependencies and migration 012's explicit publication association already isolate these graphs. No runtime aliasing, old-ID redirection, historical data rewrite, new SQL migration or algorithm compatibility shim is indicated by this source partition alone.

Import is not publication or activation. Candidate/history selection remains explicit and authorized, SQL/reference readback remains lossless and artifact-local, and old selected artifacts retain their old closures/failures. R1 has not exercised a future artifact against SQL and makes no candidate runtime compatibility claim from a design diagram.

**Format-1 answer:** the stated graph is representable entirely by existing complete source-level dependencies. No schema change is proposed. It satisfies the reviewed C/A authority reachability with explicit specialist branch stop, but has not satisfied all packet/use-case acceptance. **Budget failure does not itself prove snippet-level dependencies unavoidable.** Source-level representability and semantic coverage are separate from eight-thousand-unit acceptance and direct-root execution sufficiency. E plus selective D is not disproved as a family of possible designs; this conservative member is not certified as the implementation solution.

## R2 Readiness and Maintainer Decisions

**No executable R2 plan is authorized or ready.** Supplying an implementation checklist that presumes all six 8K fixtures are repaired would contradict this review. The earlier investigation's R2-R4 checkpoints remain historical proposals, not tasks started by this R1 pass. Do not edit source files, manifest, fixtures or algorithms from this map yet.

Only two decisions need maintainer review:

1. Accept or revise this **responsibility/identity baseline**, including A/G source ownership, preserved Q/T conformance material, P's ordinary gameplay eligibility and three vocabulary target remaps. This is not approval of the measured architecture as an 8K repair.
2. Authorize a further **bounded design refinement** before R2: find smaller clause-complete coherent source closures for the agency/visibility/combined requirements, and settle complete direct-authority/exceptional adoption execution requests. Keep complete-source semantics and 8K unchanged; if that cannot be justified, return a separately scoped contract decision rather than silently remove rules. Do not equate successful optional-root omission with fulfillment.

Manual-save meaning, history immutability, automatic persistence and player agency are already answered by Canon and need no newly invented ruling. Accepting less than all optional hits can be normal, but dropping necessary manual/agency/visibility guidance is not acceptable merely for headroom. No decision is recorded as accepted in Design Decisions, and no FR-027/031 scope is opened. H's unresolved question stays active until an approved **implemented and validated** correction supports renewed acceptance.

Classification: **C/Eternal Cycle-wide** owns claim authority, read/save/visibility/agency and truthful absence of required guidance. **B/Managed Service** owns complete artifact-local dependencies, authorized readback, compact packet/fail-closed guarantees. **A/reference implementation** owns the .NET estimator and concrete artifact storage/compiler tooling. The proposed document/source boundaries are reusable-rule design, not generalization of .NET/SQL accidents.

## R1 Validation

Actually executed in R1 against the reviewable documentation tree:

| Check | Result |
| --- | --- |
| `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate_repository.ps1` | **FAIL, one issue only: `Blocking unresolved questions remain.`** Gate preserved, not waived or described as a full pass. |
| All nine structural harnesses, run by validator | **333 assertions pass**: FR-011 13, FR-017 29, FR-018 19, FR-019 48, FR-020 59, FR-021 operations 81, FR-021 control plane 35, FR-022 32, release-neutral modes 17. |
| Markdown, relative links/anchors, indexes/navigation/terminology/contracts | No errors reported by the validator. Aggregate counts are printed only on a passing run, so no new aggregate link/file count is claimed. |
| Deterministic distribution, run by validator | Two disposable builds match SHA-256 and archive content validation reports no errors. Builder/validator unchanged; no retained ZIP or release publication. |
| Read-only inventory consistency | 273 unique IDs, each with exactly one valid destination; explicit clause references resolve. PA 80 =68 P+7 G+5 Q; CA 193 =133 C+8 A+52 T. |
| Read-only identity/binding consistency | 24/24 P and 32/32 C anchors mapped; 42/42 binding targets and all concept sets match actual manifest, 78 concept references/31 definitions/no source-wide binding. |
| Read-only estimate/projection consistency | Current section counts/totals verified: P 24/6,385, C 32/13,894. Proposed P/C/G/A estimates, 13 full-union projections and six ordered arithmetic admission projections checked. Admission used/remaining: save 7,506/494; combined 7,545/455; adjudication 7,575/425; agency 7,541/459; visibility 7,953/47; ecology 7,719/281. These are arithmetic checks, not new compiler/A-E/SQL acceptance runs. |
| Whitespace | Tracked `git diff --check` passes; three untracked audits checked individually with `git diff --no-index --check -- NUL <audit>`, no whitespace findings. Exit 1 is the expected new-file difference. LF-to-CRLF notices are checkout policy, not whitespace defects. |
| Scope/release identity | Seven tracked design-document modifications, three untracked design audits; nothing staged. No diff in docs, examples, tools, templates, agents, DISTRIBUTION or VERSION. HEAD/upstream `0/0`, VERSION and both tag objects remain at the checkpoint. No commit/push. |

The validator and whitespace checks are repeated after recording these results, so the final documentation tree is checked as well. No 1,399-test suite, new artifact compilation, SQL acceptance, live host or external tokenizer acceptance is claimed. Historical H/investigation executed results remain historical. No executable code/fixture or analytical source file is added. The retained blocker is **semantic/budget correction readiness**, not a failed compiler or SQL regression in R1.

Still unproven: approved final source text, smallest coherent authority footprint, complete direct-authority/specialist/adoption execution packets, all-six 8K candidate acceptance, candidate deterministic compilation/SQL equality and live host behavior. No staging, commit, push, tag movement, R2 implementation or retained archive is performed.
