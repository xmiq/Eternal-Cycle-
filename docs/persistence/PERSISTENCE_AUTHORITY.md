# Persistence Authority

## Purpose

This document defines which source governs when two campaign-persistence sources make incompatible claims. It prevents recent narration, incomplete session notes, stale current-state summaries, or implementation convenience from silently rewriting higher-authority truth.

It governs **campaign-fact authority**. The [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md#rules-hierarchy) separately governs rule-status authority among repository rules, Canonical Foundations, Provisional Rules, and narrative judgment.

## Core Rule

Use this authority order:

1. **Repository Canon**
2. **Campaign Canon**
3. **Historical Record**
4. **Current Campaign State**
5. **Current Session**
6. **Current Narration**

Lower authority must never silently overwrite higher authority.

The chain does not mean every higher layer contains every fact. It means that when two layers make claims about the same subject, scope, and time, the lower claim cannot replace an incompatible higher claim without the authorized correction, retcon, or migration process.

## Authority Is Claim-Specific

Authority applies to an exact claim, not to an entire document by prestige.

For example:

- Repository Canon owns whether ordinary inventory survives Reincarnation.
- Campaign Canon may establish which optional campaign premise is adopted.
- Historical Record owns that a particular sword was lost on a particular date.
- Current Campaign State owns that the present incarnation therefore does not possess it.
- Current Session may record that the sword was rediscovered today.
- Current Narration may describe its appearance.

The narration is not allowed to imply that the sword was never lost. The session may, however, establish a new recovery event through valid rules. That new event is integrated into history and current state rather than treated as an overwrite.

## The Authority Layers

## Repository Canon

**Repository Canon** is the highest authority for the game's reusable rules and accepted design governance.

It includes:

- playable canonical rules under `docs/`;
- authoritative accepted decisions in `design/DECISIONS.md`;
- canonical terminology in `design/TERMINOLOGY.md`;
- explicit ownership and scope boundaries;
- applicable safeguards and completed foundations.

Repository Canon determines what a campaign fact can mean mechanically. It does not contain populated campaign facts.

Repository Canon cannot silently rewrite campaign history merely because the repository changes. A campaign records its active repository version and adopts a later revision through an explicit migration or conversion process. Until then, the campaign's recorded rules profile remains the basis for interpreting its prior outcomes, subject to any mandatory safety or owner-approved correction.

### Repository Conflicts

If playable rules and accepted governance conflict, the repository itself is internally inconsistent. Neither silently wins. Follow [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md): identify the conflict, resolve it in both authorities, and do not call the affected material complete beforehand.

## Campaign Canon

**Campaign Canon** is the highest campaign-specific authority permitted by Repository Canon.

It may include:

- the campaign identity and continuity boundary;
- the adopted repository revision and rules profile;
- owner-approved optional premises;
- foundational setting facts established for that campaign;
- participant and control agreements relevant to continuity;
- authorized retcons and their declared scope;
- campaign-specific interpretations explicitly permitted by canon;
- accepted conversions from prior rule versions;
- stable identity assignments used by all lower records.

Campaign Canon is not permission to override Repository Canon. A campaign may choose among options the rules permit, adopt a clearly external experiment, or remain on an older rules version. It may not label an incompatible mechanic canonical and thereby change Eternal Cycle's rules.

Campaign Canon changes only through an authorized campaign-level decision with recorded provenance, scope, date, affected records, and migration consequences. Casual narration, GM preference, player theory, repeated error, or file edits do not change it.

## Historical Record

The **Historical Record** is the durable, ordered account of established campaign events, transitions, discoveries, corrections, and consequences.

It answers questions such as:

- What happened?
- When and where did it happen?
- Which actors and systems established it?
- What sources or adjudications support it?
- What did it change?
- What remained unresolved?
- Has a later correction superseded the factual description?

History is normally append-only. An event does not disappear because its effects ended. A factual correction adds a traceable correction and supersession relationship rather than quietly replacing the old account. An authorized retcon is recorded as a Campaign Canon action and then propagated through affected history and state.

The Historical Record does not convert testimony, belief, research, or narration into fact merely because they are dated. Those materials retain their own information status.

## Current Campaign State

**Current Campaign State** is the latest authoritative structured representation of what is established now for the scopes it covers.

It may include current bodies, Soul state, relationships, locations, possessions, institutions, projects, research positions, world conditions, unresolved processes, and other live campaign facts. The [Structured Persistence Architecture](STRUCTURED_PERSISTENCE_ARCHITECTURE.md) and [Campaign State Model](CAMPAIGN_STATE_MODEL.md) define the logical modules and current-state graph.

Current state is derived from Campaign Canon and the Historical Record. It may summarize prior changes, but it cannot contradict their material consequences without a valid new event, correction, retcon, or migration.

A stale current-state record is not allowed to erase a later historical event. When history and current state disagree because an update was missed, the current state is repaired from the authoritative history and source adjudication.

Current state is authoritative over informal memory and narration for the latest integrated condition. It is not automatically authoritative over an unintegrated event that occurred validly in the Current Session; that event remains pending integration rather than nonexistent.

## Current Session

The **Current Session** contains the bounded working facts, resolved outcomes, player decisions, adjudication traces, state deltas, and uncertainties established since the last confirmed save integration.

Current Session material has limited authority:

- a valid resolved event may establish a real change pending integration;
- a proposal, draft, plan, hypothetical, or uncommitted branch does not;
- a session note cannot amend Campaign Canon;
- a session ruling cannot override Repository Canon;
- an apparent contradiction must be classified before integration;
- incomplete recording creates a recovery problem, not permission to discard the session.

The [Save Update Protocol](SAVE_UPDATE_PROTOCOL.md) defines how confirmed Session changes enter the Historical Record and Current Campaign State. Until atomic activation, the Session Delta remains identifiable and must not be mistaken for either a draft or a fully validated Save Point.

## Current Narration

**Current Narration** is the immediate presentation of scenes, sensations, dialogue, descriptions, summaries, and inferred continuity during play.

Narration can establish ordinary fictional detail when the GM has authority to do so and the detail does not conflict with higher layers. It can also faithfully present a newly adjudicated event. It remains the lowest persistence authority because presentation can be:

- incomplete;
- observer-limited;
- metaphorical;
- mistaken;
- ambiguous;
- generated from stale context;
- contradicted by the actual adjudication trace;
- accidentally inconsistent.

Narration never gains authority merely by being vivid, recent, repeated, or accepted momentarily without review.

When narration conflicts with persistence, stop relying on the conflicting wording and use the canonical continuity-resolution process. Do not defend the error because it already appeared in prose.

## Rules Authority and Persistence Authority

### Living Codex Boundary

The [GM Living Codex](../gm-living-codex/README.md) is not another campaign Truth Layer and does not enter the campaign-fact hierarchy. It supplies reusable design below Repository Canon. Once a campaign adopts an entry, Campaign Configuration records the selected Codex Version and Campaign Canon owns presence and divergences. A later Codex revision does not silently migrate or overwrite that campaign.

The two authority models answer different questions.

| Question | Owner |
| --- | --- |
| Which reusable mechanic applies? | Repository rule-status hierarchy |
| Which campaign option or premise was adopted? | Campaign Canon |
| What established event occurred? | Historical Record, supported by its owning adjudication |
| What is established now? | Current Campaign State after integration |
| What changed during this interaction? | Current Session pending save update |
| How is it presented to this audience? | Current Narration, bounded by information rules |

A higher persistence layer cannot invent a mechanic. A lower rule-status category cannot rewrite campaign fact. Both kinds of authority must be valid for a material claim.

## Authority Does Not Equal Visibility

A layer's authority does not determine who may see it.

- Repository Canon is generally shareable, but GM-facing guidance may still require contextual handling.
- Campaign Canon may contain public premises and private campaign setup.
- Historical truth may include hidden events.
- Current state may include both player-facing and GM-only records.
- Session material may contain private adjudication notes.
- Narration may reveal only one Observer View.

[Truth Layers](TRUTH_LAYERS.md) define visibility. Authority answers which claim governs; visibility answers who may access it.

## Authority Does Not Equal Certainty

An authoritative record may honestly say:

- `Unknown`;
- `Not Yet Verified`;
- `Estimated`;
- `Disputed`;
- `Requires Source Recovery`;
- `Pending Resolution`.

Recording uncertainty at the correct authority is stronger than inserting unsupported certainty. A high-authority unknown cannot be overwritten by a lower-authority guess.

## Authority Does Not Equal Actor Control

Campaign records may establish an actor's prior statements, obligations, plans, beliefs, and current pressures. They do not preselect future deliberate choices.

Similarly:

- a relationship record does not force affection or obedience;
- a faction plan does not guarantee execution;
- a prophecy does not own future events;
- a Soul Title does not compel conduct;
- a recorded player objective does not authorize the GM to choose the character's next action.

Persistence preserves agency by remembering choices and conditions, not by converting them into scripts.

## Valid Downward Updates

Higher authority normally constrains lower layers, but information also flows downward through explicit procedures.

Examples include:

- a Repository Canon migration reinterprets affected campaign fields;
- a Campaign Canon retcon identifies which historical and current-state claims change;
- a validated historical event updates current state;
- an integrated session appends history and changes current state;
- current state supplies the facts from which narration is framed.

Each update must state its source, scope, affected records, and unresolved conflicts. Downward propagation is not silent overwrite.

## Valid Upward Proposals

Lower layers may provide evidence for a higher-layer change without changing that layer automatically.

- Narration may reveal a possible continuity error.
- A Session may identify a missing campaign premise.
- Current state may expose an inconsistency in history.
- Historical play may reveal that a Provisional Rule deserves repository review.
- Campaign experience may motivate a proposed canonical change.

These are proposals, evidence, or audit inputs. The higher layer changes only through its own authority process.

## Conflict Classification Procedure

When two records appear incompatible:

1. **State the exact claim.** Identify subject, property, scope, and time.
2. **Identify both layers.** Do not compare an entire file to an entire transcript.
3. **Identify the specialist owner.** Determine which system established or constrains the fact.
4. **Check scope and chronology.** Apparent conflict may be change over time, different regions, different observers, or different meanings.
5. **Check authority status.** Separate established fact, proposal, belief, theory, estimate, secret, and narration.
6. **Preserve the higher claim.** Stop propagating the lower conflicting claim.
7. **Classify the discrepancy.** Possible classes include narration error, stale state, incomplete save, record corruption, source gap, authorized retcon, rules migration, or intentional in-world deception.
8. **Route the correction.** Use the owning correction, migration, save-update, or continuity procedure.
9. **Record the resolution.** Preserve what changed, why, who authorized it, and which dependents require review.
10. **Resume only from a coherent state.** Unaffected play may continue; the disputed claim cannot be treated as settled prematurely.

The later continuity-resolution document expands this procedure. The authority result is already fixed: lower recency does not defeat higher provenance.

## Repository Revision Changes

A repository update creates a new available rules version. It does not automatically:

- alter a campaign's active rules profile;
- rewrite outcomes that were valid under the prior profile;
- fix every campaign record;
- invalidate historical causality;
- grant new capabilities;
- remove Provisional Rules without review;
- authorize a retcon.

The campaign must compare versions, decide whether and how to adopt the change, migrate affected records, preserve historical interpretation where needed, and validate the result. Detailed migration rules are owned by the later migration task.

## Authorized Retcons

An **authorized retcon** is a deliberate campaign-level correction or replacement approved through Campaign Canon authority. It is not the same as discovering that an in-world belief was false.

At minimum, a retcon identifies:

- the authority granting it;
- the exact prior claim;
- the accepted replacement;
- the reason and effective scope;
- affected history and current state;
- knowledge, relationship, progression, and consequence dependencies;
- material player choices that require discussion or remedy.

The old record is not silently deleted. It remains traceable as superseded material so migration and continuity can be audited.

## Worked Examples

### Narration Gives Back a Lost Item

The Historical Record says an ordinary spear was destroyed. Current state omits it. A later narration says the character draws that spear.

This is a narration error unless a valid recovery, reconstruction, illusion, replica, or other event was established. Stop using the conflicting description, preserve the recorded loss, and correct the scene. Do not invent a hidden recovery after the fact merely to defend the prose.

### A Session Event Was Not Saved

The Current Session validly established that a gate closed, but the old current-state file still says it is open.

The session event does not vanish because integration was missed. Recover its adjudication trace, append the event to history, update the current state, and validate dependent travel, communication, and Reincarnation routes.

### A Character Believes a False History

A priest sincerely teaches that a city was founded by a god. The Historical Record establishes a different origin, while the character's knowledge records the doctrine.

There is no authority conflict because the claims occupy different truth and observer positions. The belief remains consequential without overwriting history.

### A New Rule Changes Soul Access

The repository clarifies a Soul-access safeguard after a campaign has already used a Provisional Rule.

The campaign records the new repository version, compares the old ruling and established consequences, selects an authorized conversion, updates affected current state, and preserves the historical fact that prior scenes used the former profile. The new rule does not silently rewrite every memory of play.

### A GM Secret Contradicts Current State

A private note says an NPC is dead, while the integrated Historical Record and current state establish that the NPC survived. The note has no special authority merely because it is secret.

Classify whether the note was a plan, outdated draft, hidden event with missing evidence, or actual record omission. Until validated, it cannot overwrite the established state.

## Safeguards

- Lower authority never silently overwrites higher authority.
- Recency, repetition, dramatic emphasis, or storage location does not create authority.
- Campaign Canon cannot amend Repository Canon.
- Repository updates require campaign adoption and migration rather than automatic historical rewrite.
- History remains traceable through corrections and retcons.
- Current state cannot erase a valid event merely because integration failed.
- Session drafts, hypotheticals, and branches are not established outcomes.
- Narration cannot defend or compound a known continuity error.
- Unknown information remains unknown at every authority layer.
- Numerical changes require an owned event or explicit correction.
- Authority does not collapse truth, visibility, certainty, or actor agency.
- No populated authority record belongs in this repository.

## Scope Boundaries

This document defines authority precedence and conflict routing. It does not define:

- the complete modular campaign architecture;
- truth-layer ownership and visibility rules;
- persistence-level lifetimes;
- record schemas;
- migration stages;
- continuity-resolution classifications in full;
- save-update transactions;
- validators or templates.

Those procedures are owned by their dedicated documents; [blank templates](../../templates/README.md) implement their record contracts without gaining authority over them.

## Related Documents

- [Campaign Persistence Engine Index](README.md)
- [Campaign Persistence Philosophy](CAMPAIGN_PERSISTENCE_PHILOSOPHY.md)
- [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md)
- [Provisional Rulings](../gm/PROVISIONAL_RULINGS.md)
- [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
- [Roadmap](../../design/ROADMAP.md)
