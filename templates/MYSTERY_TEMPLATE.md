# Mystery Record Template

Use this storage-neutral template for one bounded unresolved question relevant to play without using mystery as permission to invent a hidden answer later.

A populated Mystery belongs outside the repository. This blank template creates no hidden truth, clue, false lead, suspect, discovery, answer, or player objective.

## Document Control

- **Template owner:** Mysteries in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary authority owners:** Truth Layers, Research, Knowledge, and the specialist factual owner
- **Dependencies:** bounded question, established hidden truth or explicit unresolved state, clues, actors, stakes, routes, and evidence
- **Extensions:** investigation Projects, Research, rumours, secrets, false leads, competing theories, and resolution
- **Consumers:** session preparation, NPC behaviour, Research, Knowledge, Projects, Timeline, Save Updates, and validation
- **Repository boundary:** no campaign mystery, secret answer, clue, suspect, theory, or current investigation belongs here

## Usage Guidance

1. State the unresolved question and scope precisely.
2. Reference established hidden truth when one exists; otherwise label the answer genuinely unresolved.
3. Preserve clues, false leads, theories, rumours, and facts in separate layers.
4. Give interested actors independent knowledge, goals, and routes.
5. Define fair discovery and resolution conditions without scripting player action.

## Required Fields

- **Mystery ID:** `<stable ID>`
- **Unresolved question:** `<bounded question>`
- **Scope and stakes:** `<affected actors, systems, and consequences>`
- **Truth status:** `<established hidden truth | genuinely unresolved | partially established | disputed>`
- **Authoritative factual owner:** `<protected reference or explicit none yet>`
- **Known clues:** `<Clue IDs and discovery states>`
- **Interested actors:** `<IDs, knowledge, interests, and actions>`
- **Discovery routes:** `<possible evidence routes without guaranteed success>`
- **Resolution conditions:** `<what would settle which part of the question>`
- **Truth Layer and visibility:** `<reference>`
- **Validation status:** `<result or not yet validated>`

## Clue or Lead Entry

- **Clue ID:** `<stable ID>`
- **Type:** `<evidence | testimony | trace | rumour | false lead | missing record | contradiction | other>`
- **Source and provenance:** `<where it came from>`
- **Available to:** `<Knowledge View references>`
- **What it supports:** `<claim or branch>`
- **What it does not prove:** `<limits>`
- **Reliability and uncertainty:** `<evidence-based finding>`
- **Discovery event:** `<Timeline reference or undiscovered>`
- **Related Research:** `<Question, Hypothesis, or Theory references>`

## Open Branches

- **Branch ID:** `<stable ID>`
- **Question or possibility:** `<bounded branch>`
- **Evidence for and against:** `<references>`
- **Actors pursuing it:** `<references>`
- **Available routes:** `<actions the world permits, not a menu obligation>`
- **Risks and consequences:** `<causal references>`
- **Status:** `<open | narrowed | disproved | resolved | inaccessible>`

## Optional Fields

- **Protected answer view:** `<GM Secret reference without leaking content>`
- **Investigation Project:** `<Project reference>`
- **Competing theories:** `<Research references>`
- **False-lead provenance:** `<who or what produced it and why>`
- **Partial resolutions:** `<settled subquestions and remaining scope>`
- **World changes:** `<events that alter evidence or stakes>`

## Validation Notes

- [ ] The question, scope, stakes, truth status, and owner are explicit.
- [ ] A genuinely unresolved answer is not secretly improvised as established truth.
- [ ] Clues, false leads, rumours, theories, knowledge, and facts remain distinct.
- [ ] Protected truth does not leak through indexes or metadata.
- [ ] Discovery routes are fair but do not script success or player action.
- [ ] NPCs act only on their own knowledge and interests.
- [ ] Resolution updates factual, Research, Knowledge, Timeline, and History records as appropriate.

## Cross-References

- [Truth Layers](../docs/persistence/TRUTH_LAYERS.md)
- [Research Record Template](RESEARCH_TEMPLATE.md)
- [Knowledge View Template](KNOWLEDGE_VIEW_TEMPLATE.md)
- [Project Record Template](PROJECT_TEMPLATE.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
