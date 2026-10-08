# FR-027C Trusted Player Interaction Acceptance

## Scope and Checkpoint

FR-027C is the internal control-plane checkpoint beneath the sole selected FR-027, not a new Future Revision. The [accepted A contract](../../docs/ai/CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md) governs it; [B acceptance](FR_027B_DURABLE_CAMPAIGN_BINDING_ACCEPTANCE.md) is historical routing evidence. A/B/C complete, D-G pending under the [execution plan](../FR_027_EXECUTION_PLAN.md); FR-027 remains selected/incomplete, Phase 13 Active.

Starting checkpoint: clean synchronized `main`, `e9376e1b713443725a90476b42d75508bf3e2628` (`feat: add durable campaign session binding`), upstream 0/0. VERSION `1.0.0`; tag objects v1.0.0 `6938bd2f9e965faaeb1c612b1dd765237cadc8a8`, v1.1.0-rc `4eb2583027d316a962fd49fe318781a0e060183e`. Completion commit is the commit containing this audit; its actual SHA/push is verified afterward, not invented as self-referential metadata. No campaign database, private transcript or historical operation is added/rewritten.

## Trust Model and Host Reality

Inspection of `Program.cs` establishes stdio transport and assembly-discovered MCP tools. Tool requests carry model-supplied arguments; neither configured B principal/session nor MCP client information attests an actual human message. No authenticated human-event adapter was found. A's trusted-boundary/acceptance sections explicitly require an authorized integration or a stop; they permit this implemented provider-neutral fail-closed checkpoint without pretending the host is compliant.

`ITrustedPlayerSubmissionIngress` is the sole host-event boundary. Its trusted deployment adapter resolves an actual event and immutable principal/logical-session/authority, submission occurrence/identity, target binding, origin classification, protected provenance/input reference, exact input hash/time and optional decision relationship. The service verifies scope, bounds, classification, hashes and relationship shape, then constructs `VerifiedPlayerSubmission`, which has no public constructor. The normal store cannot accept raw bytes, model booleans or arbitrary parsed assertions. The adapter itself is an authenticated integration trust boundary, not cryptographic proof supplied by these data fields.

No adapter is registered by production stdio. A model cannot obtain one through a configuration flag, test credential or tool parameter. No-adapter/unrecognized-event intake returns `TRUSTED_SUBMISSION_REQUIRED`, before storage. `TestPlayerIngress` exists only in the test assembly and deterministically redelivers protected generic fixture evidence. Live Unsloth/LM Studio human-origin integration is not proven or claimed. Ordinary players supply no internal IDs.

## Identity, Lifecycle and Owners

`PlayerInteractionService` and `IPlayerInteractionStore` accept verified input or recover authorized safe status. SQL stores one interaction aggregate per trusted logical-session/submission hash. An immutable evidence fingerprint includes original binding, input/provenance and decision reference; changed reuse fails `SUBMISSION_IDENTITY_CONFLICT`. Identical text from two separately attested submissions has distinct submission/interaction identities. Reconnect, tools, reasoning, receipt retry and assistant continuation never mint input.

Each aggregate pins exact scope hash, Binding ID/generation/Campaign, starting version and full B profile/readiness evidence, origin/protected references, original correlation, lifecycle/revision, decision/successor relations, recovery gate and unknown-persistence disposition. No raw user message, reasoning or copied Campaign Canon is stored. Existing transaction identity/receipt architecture remains authoritative; status references the current incomplete transaction from B evidence, and existing `source_interaction_id` retains the interaction relationship rather than a new transaction ledger.

The canonical states are RECEIVED, ENTRY_PENDING, OPEN, PERSISTING, AWAITING_PLAYER_INPUT, COMPLETED, BLOCKED and CANCELLED. YIELDED is derived from the last four named yielded dispositions in A (AWAITING, COMPLETED, BLOCKED, CANCELLED), not independently writable. Intake starts RECEIVED. An internal ownership validator encodes A's transition matrix; it is not a callable state setter.

C's narrow internal store methods prepare ENTRY_PENDING, record entry-only clarification, block, resume the original pre-entry gate, and cancel with deployment-granted recovery authority. Revisions and stable transition Request IDs protect replay. Entry preparation/clarification requires unchanged original version/profile, active binding and readiness. No blind rebase occurs. C cannot write OPEN, PERSISTING or gameplay COMPLETED; their state/owner semantics are represented for D/E, not implemented authorization. Status always reports entry/mutation authorization false.

Recovery cancellation is internal and separately configured (`AllowRecoveryCancellation`, default false), not an AI tool or a substitute player choice. It requires known original persistence and explicit decision identity/revision for abandonment. It preserves all committed effects/history. BLOCKED persistence uncertainty cannot be disguised as safe completion/cancellation. D/E must add their actual validated entry/receipt evidence, not reuse the ownership validator alone as a grant.

## Pending Decisions and Player Agency

Entry-only clarification records a protected question and allowed-response scope, original interaction/binding/Campaign/baseline, identity, provenance, Pending state/revision and related successor. It atomically leaves ENTRY_PENDING for AWAITING_PLAYER_INPUT. C has not admitted resolution, so this is a no-started-gameplay/known-persistence clarification, not E's general response-delivery enforcement.

A new real related submission must identify the exact Pending decision/revision; otherwise intake fails without inferring assent or silently abandoning it. It creates/reuses a new RECEIVED interaction and updates relationship/successor evidence atomically; the original stays yielded. **The decision remains Pending, with no selected option.** This is link acceptance, not semantic acceptance of an answer. Later D/E classification must atomically record answered/superseded disposition only when the trusted response actually supplies it. Rejection of all alternatives, correction, clarification or changed direction are not forced into an offered menu. C does not invent option C or transfer the decision.

Explicit recovery abandonment preserves the old decision as Abandoned, never Resolved by guessed choice. An active related response must be safely closed first. Historical status reads that interaction's own decision disposition, never a later question on the same binding. Missing decision evidence is a recovery failure and blocks both new intake and switching, not proof of a resolved choice. D/E still own semantic resolution, general gameplay yield and mutation denial.

Motivating regression executes with disposable SQL: entry clarification presents protected alternatives; an assistant continuation claims option A through an unrecognized delivery/model tool; no new trusted event/interaction exists, the decision remains Pending, switch is blocked and no gameplay transaction/OPEN grant is created. Conversation/proposal retrieval cannot satisfy ingress. This proves C's boundary, **not E's rejection across legacy writes or a real-host transcript replay**.

## Storage and Migration

New additive opt-in migration 014 is required: 013 contains bindings/receipts but no durable submission, lifecycle or decision ownership. 014 adds `player_interactions`, `player_pending_decisions`, `player_interaction_receipts` and an additional unique parent ownership index on B's table. Migrations 001-013 are unchanged. Default/template files render identically and are packaged into output/publish.

Submission uniqueness is scoped to the logical session, not text or binding (retargeting under the same ID conflicts). Binary-collated identities, exact binding generation/Campaign foreign keys, current unresolved-interaction uniqueness, one Pending decision per binding, same-binding origin/related-interaction foreign keys, state/revision/JSON projection checks and transition receipt ownership protect storage. Numeric checks explicitly reject failed conversion rather than accepting SQL UNKNOWN. Strict aggregate deserialization rejects missing/null required evidence with an internal JSON cause and safe recovery failure, not partial authority. Explicit sequence order selects the latest accepted interaction; SQL row enumeration is not identity. Protected-reference strings remain in authorized storage, never normal MCP/diagnostic projection.

Setup offers 014 only with both binding and player-interaction opt-ins. Repeat-safe real upgrade from supported 013 preserves bindings, active rules, save transactions and Canon records and creates zero inferred interactions. Evolution is forward additive; verified backup/controlled restore is the existing downgrade/recovery philosophy, not a promised down migration. No real deployment save is touched.

## Transaction-Safe B Integration and Concurrency

Intake and every lifecycle/decision mutation use Serializable transactions and B's exact transaction-owned `EC:CampaignBinding:<scope hash>` application lock. B's `ISqlCampaignBindingSwitchSafety` implementation receives that same connection/transaction, not a separate preflight or memory cache. Disabled/missing storage remains Unknown and blocked. RECEIVED/ENTRY_PENDING/OPEN/PERSISTING/BLOCKED, unknown persistence in any state, and Pending decisions block. B's existing non-Completed save check remains unchanged. Clear tracked state plus all other B eligibility/authorization checks permits an atomic successor; no input/write/decision is retargeted.

Real concurrent deliveries converge on one interaction. Distinct concurrent inputs have one accepted winner and a bounded active-interaction rejection, not a merge/queue policy. The submission-versus-switch race serializes: intake winning pins the predecessor and blocks switch; switch winning closes it and rejects old-target intake. Deterministic closed-target and accepted-blocker cases separately cover both resulting boundaries. Historical redelivery/transition receipts never select the successor or reopen old intent. All generations/history remain intact.

Rollback/cancellation injection after a related interaction insert proves aggregate, decision link, origin successor and retry evidence commit together or not at all. Cancellation remains cancellation. No ack is required to deduplicate: recreated service/ingress reads the original identity/current state and correlation.

## API, MCP and Diagnostics

Only `ec_get_player_interaction_status` is added to MCP. Optional exact interaction lookup and default latest-history lookup are authorized within configured scope; opaque IDs are locators, not grants. The projection distinguishes historical owning binding from current binding, state/revision/yield, initial Campaign Version, Pending/historical decision relationship, incomplete transaction and whether input/switch is blocked. Protected input/question/provenance references are omitted. No creation, arbitrary state setter, decision resolution, OPEN or blocker override tool is registered. `ec_begin_gameplay_interaction` remains absent.

Actual stdio tests list the surface, attempt a forged user-origin creation call (unavailable), recover a yielded decision in two separate processes and verify identical durable history/no extra input. A separate actual process without any human event returns `TRUSTED_SUBMISSION_REQUIRED` and stores nothing. This is reference process evidence, not authenticated external-host acceptance.

Thirteen registered safe failure categories cover required/invalid provenance, conflicting identity, unavailable binding/session, active input, invalid transition, decision conflict, stale state, unresolved persistence, missing schema, recovery/storage failure and separate interaction/decision switch blockers. Unauthorized lookups disclose no inaccessible content. SQL/JSON causes remain internal for diagnostics; public responses are bounded and sanitized. Existing diagnostics record correlation, interaction/Binding/Campaign, safe origin/submission hash, state/revision, decision/status, switch disposition and create/reuse/recovery outcome. Transition receipts retain owner, prior/next state and revision, joined to the original interaction correlation. No raw input, conversation, Secrets, connection string or reasoning is logged. Full GM Turn Trace remains F.

## Validation

Executed Release results before final staging:

| Check | Result |
| --- | --- |
| Complete portable suite, FR-022-026/boundary | 1,144 passed, zero failed/skipped |
| Complete CLI/process suite | 65 passed, zero failed/skipped |
| Complete SQL-enabled Managed suite, FR-020/021/025/026/B included | 423 passed, zero failed/skipped |
| C-only focused contract/real SQL/MCP cases | 80 passed: 37 contract and 43 SQL/process cases; zero failed/skipped |
| Explicit portable/CLI/reference Release builds | Passed, zero warnings/errors |
| Reference Release publish and 014 packaging | Passed; both 014 files and metadata match source bytes; LICENSE, NOTICE and independent worker included |
| Repository validator | Passed: 330 Markdown files, 7,771 relative links, 245 anchors; zero blocking questions, orphaned documents or forbidden campaign-data directories |
| Ten structural/regression harnesses | 365 assertions passed: 13/29/19/48/59/81/35/32/32/17 |
| Deterministic distribution | Two temporary review ZIPs matched; content/boundary checks passed; validation artifacts removed, no release archive published |

The full suites contain **1,632 unique tests**; focused cases are a subset, not an added total. SQL execution sets `ETERNAL_CYCLE_RUN_SQL_INTEGRATION=1` and creates/destroys real disposable LocalDB databases. No mock-only SQL claim or actual campaign database is used. Early focused runs exposed/fixed closed-predecessor refresh and optional nullable MCP argument handling; test-only fixture serialization/choice indexing expectations were corrected to the established safe behavior, not weakened trust requirements. Final review added real SQL regressions for historical decision isolation, contradictory ownership/JSON projections, incomplete aggregate evidence and missing decision evidence.

Executed commands use `dotnet build`/`dotnet test` in Release for the portable rules, compiler CLI and Managed reference projects, plus `dotnet publish` for the reference. The isolated filter is `FullyQualifiedName~PlayerInteractionContractTests|FullyQualifiedName~CompiledRulesArtifactSqlImportTests.C_`; it is a subset of the full Managed suite. Repository validation uses `pwsh -NoProfile -ExecutionPolicy Bypass -File tools/validate_repository.ps1`, including all ten harnesses and deterministic distribution checks. Exact-state whitespace and staged scope are checked before commit. No generated output, secret, campaign data, source corpus/compiler/retrieval change, old migration edit, VERSION or tag action belongs in the staged set.

## Classification, Acceptance and Handoff

- **A - reference implementation:** .NET contracts/options, stdio adapter limitations, SQL/LocalDB, migration 014, JSON projections, application lock and MCP status test processes.
- **B - Managed contract:** trusted actual-event ingress, immutable input/binding identity, deduplication/conflict rejection, owner/revision transitions, durable decision relation, known persistence and transaction-consistent switching/recovery.
- **C - Eternal Cycle invariant:** only real player input authorizes a new interaction; model continuation/conversation/proposals cannot manufacture choices. No implementation detail becomes a universal database/host rule.

Acceptance is `FR_027C_ACCEPTED`, internal control plane only. A permits the explicitly blocked live-host integration rather than false human-origin assurance. This does not mean gameplay is safely enabled: D must connect mandatory entry/read/rule/bootstrap evidence before OPEN; E must gate every gameplay write, actual response/yield, semantic decision disposition and completion against validated persistence. F owns the complete trace and G real host/end-to-end acceptance. Existing legacy write tools remain ungated by C, deliberately unchanged until E. Binding/input records alone never prove full FR-027 conformance.

No FR-028 onward, Campaign creation/adoption redesign, rule/vocabulary/ranking/budget/corpus change, compiler/import work, release/archive publication, VERSION or tag change. Immutable release and historical FR-026/R2 fixtures remain preserved. Stop after accepted C commit and normal main push; await explicit D selection.
