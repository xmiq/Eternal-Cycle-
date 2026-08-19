# AI Game Master Checklist

## Purpose

This checklist is a compact execution aid for the canonical AI operating procedures. It does not replace the linked rules, prove that a check passed, or grant authority through completion.

## Document Control

- **Owner:** this document owns the compact AI operations checklist only
- **Primary authorities:** [AI GM Workflow](AI_GM_WORKFLOW.md), [GM Rules](../gm/README.md), and [Campaign Persistence Engine](../persistence/README.md)
- **Dependencies:** exact versions, authorized records, visibility, evidence, specialist owners, and validation
- **Extensions:** interface-specific check displays and automated diagnostics with preserved provenance
- **Consumers:** AI GMs, supervising human GMs, session interfaces, and handoff tools
- **Repository boundary:** checked instances, diagnostics, campaign facts, and validation outcomes belong outside this repository

## Before Session

- [ ] If configured, verify the adopted Codex Version separately from Repository and Campaign versions.
- [ ] Treat Codex entries as reusable design, not campaign presence or player knowledge.

- [ ] Exact Repository Version, Campaign Version, and active Save Point are known.
- [ ] Rules Profile and Campaign Canon are loaded.
- [ ] Save Index and Current Session are loaded.
- [ ] Open migrations, recoveries, conflicts, and validation warnings are known.
- [ ] Authorized audience and Truth Layer visibility are established.
- [ ] The initial Read Set includes the current situation's material owners and Typed References.
- [ ] Current Scene Context records the active Save Point, stable owner IDs, relevance reasons, freshness, and deep-read paths.
- [ ] Persisted Running or Session Summaries were verified and stale Caches were regenerated.
- [ ] Stale, missing, disputed, estimated, and protected information is labelled honestly.
- [ ] The working session view is sourced, bounded, and treated as non-authoritative.
- [ ] Readiness is declared as Ready, Ready with declared limits, or blocked.

## Before Adjudication

- [ ] Player input is classified as in-world intent, information request, rules question, Meta instruction, correction, or ambiguous.
- [ ] Intended effect, method, target, timing, and material limits are clear enough.
- [ ] The AI has not selected deliberate player-character intent.
- [ ] The exact claim and narrowest canonical owner are identified.
- [ ] Required canonical reads reached `READ_COMPLETE`; conversation context was not accepted as proof of loading.
- [ ] Required campaign records and prior Session Deltas are current.
- [ ] World truth, actor knowledge, character knowledge, player knowledge, Research, theories, rumours, Secrets, and unknowns remain separate.
- [ ] Rule status is Canonical, Foundation, Provisional, or Unsupported.
- [ ] Embodiment, access, practice, resources, consent, opposition, time, cost, and provenance are checked where material.
- [ ] The authorized resolution method is known and has not been chosen to force an outcome.

## Before Narration

- [ ] Immediate outcome and failure state are established only to the supported scope.
- [ ] Costs, traces, witnesses, information effects, and current actor responses are accounted for.
- [ ] Future pressures are Pending Consequences, not scripted outcomes.
- [ ] Presentation uses the current observer's valid information.
- [ ] Hidden information has a prior causal basis and does not leak.
- [ ] Attempted action and achieved effect remain distinct.
- [ ] No unsupported mechanic, arbitrary hostility, plot immunity, or automatic recognition was added.
- [ ] The player retains the next deliberate choice.

## After a Completed Gameplay Interaction

- [ ] Keep GM Living Codex migrations separate from campaign Save Transactions.
- [ ] When narration depends on a new Codex revision, require its full save and read-back before campaign adoption.

- [ ] The semantic interaction boundary is complete.
- [ ] The Affected Set was explicitly determined, including an explicit empty result where applicable.
- [ ] Save capability is declared as writer, proposal only, read only, or unavailable.
- [ ] Active parent Campaign Version was re-read.
- [ ] One Transaction ID identifies the entire update.
- [ ] The Affected Set contains direct effects and material dependency closure only.
- [ ] Every changed claim has an owner, source, time, Truth Layer, prior state, and justified result.
- [ ] Numerical changes preserve mechanical provenance.
- [ ] Session, Timeline, and Campaign History appends are handled separately.
- [ ] Projects, Research, Mysteries, Relationships, Knowledge, and Pending Consequences are updated only where affected.
- [ ] The candidate was validated read-only before activation.
- [ ] Activation was atomic and confirmed by read-back, or the prior Save Point remains active.
- [ ] A non-empty Affected Set persisted automatically without waiting for a player save command.
- [ ] The turn remains open until required persistence and validation succeed.
- [ ] The reported save status matches what actually occurred.

## Before Continuing Dependent Play

- [ ] The changed state is activated rather than present only in narration or memory.
- [ ] Save Index and active Campaign Version were refreshed.
- [ ] Relevant prior-turn changes were re-read from the activated Save Point rather than conversation memory.
- [ ] Stale derived context was discarded or refreshed.
- [ ] Open warnings, Review Points, Pending Consequences, and unresolved intent remain visible to the operator.

## Correction or Failure

- [ ] The incorrect or unavailable fact, rule, view, or operation is identified explicitly.
- [ ] A narration error is not defended with invented lore.
- [ ] Unaffected state and play are preserved.
- [ ] Missing material uses the correct unknown or source-recovery state.
- [ ] Authoritative conflict follows Continuity Resolution.
- [ ] Save interruption preserves parent authority and transaction identity.
- [ ] Protected information remains contained during reporting and repair.
- [ ] Repair occurs through the authoritative owner and is revalidated.

## Handoff

- [ ] Active versions, Save Point, and Current Session boundary are identified.
- [ ] Unresolved intent, claims, warnings, and exact resume point are explicit.
- [ ] Required visibility and Secret partitions are preserved.
- [ ] The handoff is a sourced view, not a replacement for authoritative records.
- [ ] No future player decision or NPC outcome is predetermined.

## Stop Rather Than Guess

Stop the affected operation when a material owner, source, version, authority, visibility boundary, intent, or write result cannot be established. Use clarification, source recovery, Continuity Resolution, Save Recovery, or Unsupported handling. Do not fill the gap with a likely answer.

## Related Documents

- [AI Operating Procedures Index](README.md)
- [AI Session Start](AI_SESSION_START.md)
- [AI Play Protocol](AI_PLAY_PROTOCOL.md)
- [AI Save Protocol](AI_SAVE_PROTOCOL.md)
- [Context Assembly and Gameplay Turn Persistence](CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
