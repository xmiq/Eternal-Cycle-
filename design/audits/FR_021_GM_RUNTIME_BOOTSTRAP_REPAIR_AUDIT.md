# FR-021 GM Runtime Bootstrap and Context-Efficiency Repair Audit

## Scope

This audit records owner-authorized corrective maintenance to completed FR-021 after MCP-only gameplay acceptance. It creates no new Future Revision, changes no gameplay mechanic, leaves `VERSION` at `1.0.0`, and does not move or rewrite release or RC tags.

The known successful publication evidence remains intact: operation `OP-23dd42069add4bbd97f04d5689c51f25`, correlation `CORR-fa56ad54c0e049f680a13b5496f1b34e`, Rule Release `RULE-2b9bf61cd17c45aba71323450deaedb0`, and immutable source `520B5525DD10E3395C5BB35ECF43E95EE0FE0230`.

## Established Causes

Repository inspection established four contract gaps:

- the Runtime Rule Kernel preserved invariants but did not itself order the complete GM interaction lifecycle;
- `gameplay.resolve` did not structurally require a versioned GM Runtime Procedure, creating a circular dependency on the model knowing which topic to request;
- readiness did not represent the user-owned host instruction step or distinguish acknowledgment from technical verification;
- model-facing rule responses repeated service-useful per-chunk metadata, and the only reference write surface encouraged complete serialized-record replacement for small changes.

The live context-window rejection of one attempted save was a host rejection before tool execution, not proof of a persistence transaction defect. Historical Git/lifecycle findings are outside this repair and remain recorded without a newly invented cause.

## Repair

### Three-layer GM operation

`docs/rules/GM_HOST_BOOTSTRAP.txt` is the compact canonical host-level execution contract. `docs/rules/GM_RUNTIME_PROCEDURE.md` owns the detailed read, resolve, Affected Set, persist, validate, narrate, and yield lifecycle. The remaining specifications stay in their specialist repository documents.

The official Rule Source manifest versions both artifacts. Candidate validation rejects a release without the Kernel, an always-included `gameplay.resolve` procedure, or the Host Bootstrap. Gameplay selection automatically roots both the Kernel and procedure without a caller topic and fails rather than silently omitting the procedure.

### Setup and readiness

Migration 010 adds a Ruleset-scoped `gm_host_configurations` record keyed to the active bootstrap source hash. Readiness exposes `Required`, `InstructionsPresented`, `UserConfirmed`, and `Verified`. Setup returns the exact published bootstrap, records presentation, and accepts natural user confirmation. Only an integration capable of actual inspection or attestation may record `Verified`; a changed source hash returns readiness to `Required`.

### Compact Rule Packet

The reference transport now groups selected text by stable Rule Source ID and carries one packet format, campaign/world/ruleset identity, repository/release/source identity, and token totals. It omits repeated per-chunk paths, hashes, selectors, dependencies, and preparation metadata from the LLM-facing envelope. The published Rule Store retains all of that provenance.

The deterministic regression fixture serialized the full selected context at 1,755 characters and the compact packet at 608 characters, a 65.4 percent reduction. The live pre-repair observation was approximately 5.6K transport characters around roughly 879 tokens of rule content; live post-repair measurement remains part of acceptance.

### Constrained persistence patch

The reference `ec_patch_records` surface accepts set-only object patches for existing records. It loads authoritative current payloads, preserves omitted fields, rejects identity/reference fields, creates, deletes, and tombstones, expands each patch into a full mutation, and delegates to the existing expected-parent/revision, idempotency, transaction, validation, activation, read-back, and receipt path. No campaign schema migration was required for patches and no arbitrary SQL or generic destructive JSON surface was added.

## Player-Turn Boundary

The canonical bootstrap, Runtime Kernel, GM Runtime Procedure, and AI operating contracts now agree that only actual new player input starts a player turn. Reasoning, continuation, tool work, retry, retrieval, persistence, validation, and same-input simulation remain inside that interaction. An unresolved player-controlled decision requires a yield; an internal continuation cannot select it.

This is a normative host/runtime contract. The reference service can guarantee required rule delivery and persistence evidence, but it cannot physically force an arbitrary model host to obey a System Prompt it cannot inspect.

## Classification

- **C — Eternal Cycle-wide:** compact Host Bootstrap; authoritative GM Runtime Procedure; actual-input turn boundary; player-agency yield; canonical reads before resolution; persistence and validation before completed narration; compact dependency-complete context.
- **B — Managed Service contract:** automatic mandatory procedure closure; host-configuration readiness and canonical bootstrap retrieval; compact Rule Packet with retained provenance; smallest sufficient Read Set; constrained patch semantics through the existing transaction.
- **A — reference implementation:** .NET MCP tools and serializers; SQL Server migration 010 and tables; concrete compact-packet shape; C# patch expansion; observed character counts and live host context limit.

## Validation and Remaining Acceptance

The reference suite covers canonical artifact size/content, exact setup presentation, natural confirmation, verification separation, gameplay-readiness blocking, mandatory procedure selection without a topic, omission failure, compact-envelope reduction, turn/yield language, full-mutation compatibility, omitted-state preservation, concurrency/revision propagation, idempotent retry, failed-patch atomicity, and identity/reference rejection.

Validation completed successfully:

- Release build: zero warnings and zero errors;
- reference tests: 163 passed, zero failed, zero skipped;
- FR-011, FR-017, FR-018, FR-019, FR-020, FR-021 durable-operation, FR-021 control-plane, and release-neutral harnesses: 301 assertions passed in total;
- repository validation: 288 Markdown files, 7,169 relative links, 164 anchors, 188 canonical documents, 1,268 terminology checks, 193 roadmap tasks, and 21 Future Revision entries passed with no orphaned Markdown document or forbidden campaign-data directory;
- distribution validation: 369 entries, no forbidden generated/cache/temp path, SHA-256 `BF7DDAC47ED0131095D9A25FF3FCE2FD685DB29272E2E0DC0C8D38340FE2F416`;
- Release publish: distribution metadata, Apache license, notice, and both migration 010 assets were present.

No live SQL Server deployment of migration 010 was performed during this pass. Repository/schema tests and packaging validate its structure and delivery, while real migration execution remains deployment acceptance.

Live acceptance remains required. In the actual MCP-only host, setup must present the exact bootstrap; after the user installs and confirms it, real player input must retrieve the Kernel, procedure, relevant rules, and campaign Read Set, preserve agency, persist and validate any durable consequences, narrate only afterward, and yield at the next player-owned choice without avoidable context exhaustion.
