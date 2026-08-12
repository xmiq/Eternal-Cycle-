# AI Game Master Workflow

## Purpose

This procedure sequences an AI Game Master's work from initialization through handoff. It applies the GM Framework and Campaign Persistence Engine; it does not create a new adjudication method, resolution mechanic, campaign schema, or narrative mandate.

## Document Control

- **Owner:** this document owns AI operating sequence and readiness transitions
- **Primary authorities:** [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md) and [Campaign Persistence Integration](../persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md)
- **Dependencies:** repository and Campaign versions, Save Index, Current Session, Read Sets, specialist owners, Save Updates, and validation
- **Extensions:** [Session Start](AI_SESSION_START.md), [Play Protocol](AI_PLAY_PROTOCOL.md), [Save Protocol](AI_SAVE_PROTOCOL.md), and interface-specific implementations
- **Consumers:** AI GMs, supervising human GMs, orchestration tools, and handoff processes
- **Repository boundary:** the procedure stores no campaign state, prompt, transcript, credentials, model memory, or implementation configuration

## Operating States

The operator maintains one honest operating state:

| State | Meaning | Permitted action |
| --- | --- | --- |
| **Ready** | required canon, campaign state, visibility, and validation are sufficient for the next bounded operation | continue through the workflow |
| **Ready with declared limits** | immaterial gaps exist and their limits are explicit | continue only where those gaps cannot change adjudication or disclosure |
| **Clarification required** | player intent, authority, scope, or a material input is ambiguous | ask the narrowest useful question |
| **Source recovery required** | a material record may exist but is unavailable | recover the source or defer the affected claim |
| **Continuity resolution required** | authoritative records conflict or narration contradicts them | follow Continuity Resolution before dependent play |
| **Save recovery required** | a transaction was interrupted, conflicted, or failed validation | keep the parent Save Point active and resume recovery |
| **Unsupported** | resolution would require a major mechanic absent from canon | narrow, defer, or seek an authorized governance decision |

An AI GM never reports `Ready` merely because enough prose exists to improvise an answer.

## End-to-End Cycle

### 1. Establish the operating authority

1. Identify the exact Repository Version and Campaign Version, plus the configured Codex Version when the campaign uses the GM Living Codex.
2. Load the Campaign Canon or Rules Profile.
3. Confirm the active Save Point, Current Session state, open migrations, validation warnings, and unresolved conflicts.
4. Confirm which participant or interface may authorize play, rulings, record access, saves, and corrections.
5. Separate repository materials from the external Campaign Record.

If versions or authority are materially unknown, stop at the appropriate non-ready state.

### 2. Build the working context

Follow [AI Session Start](AI_SESSION_START.md). Load the Save Index and Current Session first, then build the smallest complete Read Set for the current situation. Expand along material Typed References only when they can change the claim, consequence, uncertainty, disclosure, or player choice.

Retrieved text is not automatically current or authoritative. Check owner, version, effective time, Truth Layer, visibility, source, status, and supersession before use.

### 3. Advance the world to the present boundary

Apply already-caused off-screen activity, elapsed time, Pending Consequences, and Review Points through their canonical owners. Use the World Engine's selected Simulation Frame. Do not invent a predetermined future, freeze absent actors, or resolve beyond the next material agency or uncertainty boundary.

### 4. Present the perceivable situation

Present only information available through the current character's valid observer view. Distinguish direct perception, remembered information, inference, uncertainty, and deliberate in-world deception. Provide enough context for meaningful choice without exposing GM Secrets or turning possible actions into a mandatory menu.

### 5. Receive and clarify intent

Use [AI Play Protocol](AI_PLAY_PROTOCOL.md) to identify the intended effect, method, target, timing, limits, and stopping conditions. Ask only when ambiguity can materially change risk, consent, cost, ownership, or consequence. Never select deliberate speech, belief, allegiance, morality, relationship, or action for the player character.

### 6. Resolve the interaction

1. Identify factual and information-layer inputs.
2. Search for the narrowest canonical owner.
3. Classify the rule status as Canonical, Canonical Foundation, Provisional, or Unsupported.
4. Check embodiment, access, practice, resources, consent, opposition, time, cost, and provenance where material.
5. Use only the campaign's authorized resolution method.
6. Establish immediate outcome, failure state, costs, traces, information effects, and currently resolvable consequences.
7. Preserve uncertainty and hand every cross-system effect to its owner.

A retrieved example may illustrate a rule but cannot replace the rule's requirements.

### 7. Prepare the established presentation

Prepared narration renders the established situation and outcome from the appropriate observer view. It may add ordinary sensory and connective detail that does not change material facts. It may not introduce a new mechanic, hidden countermeasure, relationship state, numerical change, historical event, or guaranteed future outcome.

Clearly separate an attempted action from an achieved effect. Do not disguise an estimate as certainty or an unresolved question as a secret answer.

Preparing text does not determine when it may be delivered. The selected AI Execution Profile owns presentation ordering and may require validated persistence before delivery.

### 8. Persist before dependent play or a stricter delivery gate

At the semantic boundary of every completed Gameplay Interaction, follow [AI Save Protocol](AI_SAVE_PROTOCOL.md). Determine the Affected Set, stage one owner-routed Session Delta, append the required Session, Timeline, and Campaign History records, validate the candidate, and activate atomically when authorized.

Do not continue dependent adjudication while the change exists only in narration, conversation context, or uncommitted model memory.

A proposed reusable Codex addition is not part of the campaign Save Transaction. Persist it through the [Living Codex full-save protocol](../gm-living-codex/PERSISTENCE_MODEL.md#full-save-protocol) only when GM-approved. If durable narration depends on the new Codex revision, validate and deploy that revision before the campaign transaction that adopts it; otherwise record a campaign-local divergence and defer Codex promotion.

For inherited outcomes, resolve [Reproductive Compatibility](../gm-living-codex/REPRODUCTIVE_COMPATIBILITY.md) first and [Lineage and Evolutionary Inheritance](../gm-living-codex/LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md) only after successful formation. Never infer a missing profile, expose protected weights automatically, or place an individual's genealogy in the reusable Codex.

### 9. Re-enter or hand off

After activation:

1. refresh the Save Index and active Campaign Version;
2. discard stale derived context;
3. carry forward unresolved player intent, Pending Consequences, Review Points, and declared limitations;
4. continue at the next interaction or produce a bounded handoff view.

A handoff identifies the active versions, Save Point, Current Session boundary, unresolved claims, protected-information requirements, and exact resume point. It does not copy every campaign record or predetermine the next decision.

## Retrieval and Tool Discipline

- Search indexes before broad corpus retrieval.
- Prefer authoritative owners over summaries and examples.
- Treat cached material as stale until version and effective time are checked.
- Use least-necessary access for Secrets and observer-specific views.
- Keep tool output, private reasoning traces, diagnostics, and implementation metadata out of Campaign Canon.
- Report unavailable tools or failed writes; never simulate successful access.
- Re-read the active record after any external write before relying on it.

## Stop Conditions

Stop the affected operation when:

- the Repository Version, Campaign Version, or active Save Point is materially unknown;
- the required canonical owner or Campaign Record cannot be loaded;
- two authoritative sources conflict;
- protected information cannot be isolated from an unauthorized view;
- intent remains materially ambiguous after a reasonable clarification attempt;
- a Provisional Rule would exceed the Alpha Playtest Rules;
- a save candidate fails a blocking validation check;
- a concurrent update changes the parent Campaign Version;
- continuing would rely on a write that has not been confirmed.

Unrelated play may continue only when the stopped claim cannot affect it.

## Safeguards

- Fluency, confidence, repetition, and context recency never create authority.
- Automation never expands GM authority.
- Memory never replaces the Campaign State Graph.
- Retrieval never promotes a theory, rumour, or Secret into public truth.
- The workflow never grants progression or resolves a mechanic by checklist completion.
- A failed operation leaves the last valid authority intact.

## Related Documents

- [AI Operating Procedures Index](README.md)
- [AI Checklist](AI_CHECKLIST.md)
- [Persistence Authority](../persistence/PERSISTENCE_AUTHORITY.md)
- [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md)
