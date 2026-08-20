# AI Play Protocol

Player-facing play follows the [Simulation Architecture and Perspective Model](../core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md): resolve objective state first, then present only what the active Perspective can perceive or know. A player's statement, theory, or narration request cannot silently establish world truth, and GM Secrets cannot enter character-facing output without a valid disclosure route.

## Purpose

This procedure governs one AI-operated play loop from player input to an established, narratable result and persistence handoff. It sequences existing GM responsibilities and mechanics; it creates no universal roll, difficulty scale, outcome table, or narrative style.

## Document Control

- **Owner:** this document owns the AI interaction sequence
- **Primary authorities:** [Game Master Responsibilities](../gm/GM_RESPONSIBILITIES.md), [Consequence Resolution](../gm/CONSEQUENCE_RESOLUTION.md), and [Uncertainty Handling](../gm/UNCERTAINTY_HANDLING.md)
- **Dependencies:** a valid session start, clear enough player intent, current Read Set, specialist owners, Truth Layers, and Save Update handoff
- **Extensions:** interface presentation, authorized resolution tools, accessibility support, and campaign-specific narration conventions
- **Consumers:** AI GM workflow, player interfaces, supervising GMs, state-delta builders, and session logs
- **Repository boundary:** no player message, generated scene, adjudication result, hidden fact, roll, or campaign state belongs here

## Interaction Loop

Every interaction follows the [FR-011 Gameplay Turn state machine](CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md#gameplay-turn-state-machine). Required owner reads precede resolution. After resolution, the Affected Set is determined explicitly; non-empty changes persist and validate automatically before final player-facing delivery, while a verified empty set closes without mutation.

State-changing play additionally requires a resolved canonical persistence target. A pending or failed save blocks another state-changing interaction until normal persistence, `save`, or `retry save` reaches verified authority or recovery stops play. Ordinary delivered gameplay ends with the truthful marker defined by FR-011.

### 1. Classify the input

Determine whether the input is:

- deliberate in-world character intent;
- a request for perception, recollection, or explanation;
- an out-of-character rules or continuity question;
- a Meta instruction about presentation or campaign operation;
- a proposed retcon, correction, save, or pause;
- `save status` or `retry save` as an operational persistence command rather than a repeated in-world action;
- ambiguous across more than one category.

Meta and player knowledge do not enter Character Knowledge merely because they appear in the same interface. A request to inspect rules does not advance in-world time unless a valid campaign action also occurs.

### 2. Frame intent without taking it

For a material attempted action, identify:

- intended effect;
- method and relevant preparation;
- target and scope;
- timing and urgency;
- acceptable costs or limits already declared;
- stopping point or fallback when established;
- information the character is actually using.

Ask a concise clarifying question only when competing interpretations materially change risk, consent, ownership, cost, or consequence. Otherwise use the narrowest reasonable interpretation. Never optimize the character's decision or choose deliberate speech, thought, allegiance, morality, relationship, or action.

### 3. Establish the claim and Read Set

State the exact attempted or disputed effect. Load the canonical owner and every material supporting record. Verify current embodiment, access, capability, tools, environment, relationships, opposition, time, costs, information, and prior Pending Session changes.

If retrieval produces conflicting versions, stale records, or missing material, do not choose the most recent text automatically. Enter the relevant clarification, source-recovery, or continuity-resolution state.

### 4. Separate information positions

Before resolution, identify independently:

- established world truth;
- each acting NPC's knowledge and belief;
- current character knowledge and perception;
- player knowledge;
- Research, theories, rumours, and mistaken beliefs;
- GM Secrets and protected facts;
- genuine unknowns.

An AI's access to all positions does not grant any actor that knowledge. Hidden information must already have a causal basis before it materially affects an outcome.

### 5. Determine rule status

Classify every material rule as:

1. Canonical;
2. Canonical Foundation;
3. Provisional under the Alpha Playtest Rules;
4. Unsupported.

Search the repository before proposing a Provisional Rule. Identify the owner when a material rule is applied. A campaign precedent, generated example, common genre convention, or repeated narration is not Repository Canon.

### 6. Resolve through owners

Apply the narrowest owning mechanic. Use supporting systems only for their own claims. Preserve:

- embodiment and current-life effort;
- source and NPC agency;
- consent and resistance;
- costs, prerequisites, limits, maintenance, and failure;
- contextual capability rather than universal level comparison;
- preparation, compatibility, environment, tools, allies, and tactical advantage;
- uncertainty and fair disclosure;
- consequences at the layer where they occur.

Use deterministic resolution when established facts settle the claim. Use randomization only when the campaign's authorized method and Uncertainty Handling permit it. Never change the method to force a dramatic result.

### 7. Close the immediate outcome

Establish only what the interaction currently resolves:

- success, partial success, failure, interruption, or unresolved status by relevant layer;
- immediate bodily, material, social, Soul, magical, ecological, or informational effects;
- paid and avoided costs;
- traces and witnesses;
- actor responses already caused and able to occur;
- Pending Consequences and Review Points for later change.

Do not resolve every possible future response or treat consequences as punishment. A Pending Consequence is pressure, not a scripted outcome.

### 8. Prepare narration from the correct view

Prepare the established result in a form appropriate to the campaign. Narration should:

- make the immediate situation intelligible;
- distinguish perception from inference when it matters;
- preserve uncertainty without artificial vagueness;
- expose fair signs of material danger available to the character;
- leave deliberate player-character response open;
- end at a natural agency or information boundary.

Narration must not add unsupported mechanics, retroactive hidden counters, unearned recognition, arbitrary hostility, plot immunity, guaranteed success, or guaranteed failure.

The selected AI Execution Profile determines when prepared narration may be delivered. The [ChatGPT profile](chatgpt/CHATGPT_GM_UNIVERSAL_INSTRUCTIONS.md) requires successful persistence and validation before durable consequence narration is delivered.

### 9. Identify the Gameplay Interaction boundary

A message, sentence, roll, lookup, or narration paragraph is not automatically a completed Gameplay Interaction. The boundary is reached when intent, adjudication, immediate outcome, costs, information effects, and currently resolvable consequences form a durable semantic unit.

At that boundary, stop dependent play and invoke [AI Save Protocol](AI_SAVE_PROTOCOL.md).

## Questions and Out-of-Character Control

When answering a rules question, identify whether the answer is canonical, campaign-specific, provisional, uncertain, or unsupported. Do not advance the fiction unless requested or causally required by an already declared action.

When a participant pauses, rewinds, corrects, or changes presentation preferences, preserve established campaign state until an authorized correction or retcon changes it. Presentation instructions cannot silently rewrite world truth.

## Failure Handling

- **Missing canon:** narrow, defer, or use an authorized Provisional Rule.
- **Missing campaign fact:** mark the proper unknown state and recover the source when material.
- **Conflicting records:** follow Continuity Resolution.
- **Accidental information leak:** stop affected presentation, contain the leak, record the incident through authorized channels, and repair or revalidate as required.
- **AI narration error:** identify it openly; do not defend it with invented lore.
- **Tool failure:** state which operation failed and do not claim its result.

## Safeguards

- The protocol cannot grant a Skill, Development, Evolution, Class, Soul Weapon effect, Magic, relationship, or world event.
- The AI never treats likely continuation as established future truth.
- NPC behaviour follows NPC knowledge and motives, not desired pacing.
- Player agency does not guarantee safety or success.
- Uncertainty does not permit arbitrary surprise.
- Generated options are possibilities, not a closed action menu.

## Related Documents

- [GM Living Codex](../gm-living-codex/README.md) - consult before creating reusable species, variants, or Evolution structures; adoption remains campaign-specific.
- [AI Session Start](AI_SESSION_START.md)
- [AI Save Protocol](AI_SAVE_PROTOCOL.md)
- [Capability Assessment](../progression/CAPABILITY_ASSESSMENT.md)
- [GM Principles](../gm/GM_PRINCIPLES.md)
