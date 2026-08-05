# Campaign History Entry Template

Use this storage-neutral template to preserve the durable account of one established event and its corrections under Historical Record authority without copying every specialist record involved.

A populated history entry belongs outside the repository. This blank template creates no event, participant, cause, outcome, correction, or historical interpretation.

## Document Control

- **Template owner:** Campaign History in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary authority owners:** [Persistence Authority](../docs/persistence/PERSISTENCE_AUTHORITY.md) and [Timeline Engine](../docs/persistence/TIMELINE_ENGINE.md)
- **Dependencies:** Timeline Event, participants, causes, outcomes, information effects, state changes, consequences, and sources
- **Extensions:** world history, personal history, Soul chronology, institutional records, public accounts, and authorized corrections
- **Consumers:** continuity, narration, Research, Relationships, Timeline, migration, validation, and Derived Views
- **Repository boundary:** no campaign event, historical account, correction, source, or live consequence belongs here

## Usage Guidance

1. Create an entry only for established events material to continuity.
2. Reference specialist outcomes instead of restating their mechanics.
3. Preserve source disagreement and observer interpretation.
4. Append authorized factual corrections; do not rewrite history silently.
5. Keep future plans, rumours, theories, and preferred outcomes out of Historical Record.

## Required Fields

- **History Entry ID:** `<stable ID>`
- **Timeline Event ID:** `<stable event reference>`
- **Event identity and scope:** `<bounded description>`
- **Participants and roles:** `<typed references>`
- **Locations:** `<typed references>`
- **Causes:** `<evidenced causal references>`
- **Established outcomes:** `<specialist record references>`
- **State changes:** `<Affected Set and before/after references>`
- **Consequences:** `<immediate, delayed, and pending references>`
- **Information effects:** `<Knowledge View changes>`
- **Sources and provenance:** `<records, witnesses, adjudication, or migration>`
- **Effective time:** `<Timeline coordinate>`
- **Validation status:** `<result or not yet validated>`

## Interpretations and Corrections

- **Public or institutional accounts:** `<observer-specific Knowledge references>`
- **Disputed details:** `<claims, evidence, and confidence>`
- **Unknown details:** `<gaps and recovery routes>`
- **Correction ID:** `<authorized correction or none>`
- **Superseded claim:** `<exact prior statement>`
- **Accepted correction:** `<exact replacement>`
- **Authority and reason:** `<authorization and evidence>`
- **Downstream repairs:** `<affected records and Derived Views>`

## Optional Fields

- **Relationship consequences:** `<references>`
- **Research and discovery consequences:** `<references>`
- **World and faction consequences:** `<references>`
- **Soul and Reincarnation consequences:** `<references>`
- **Legacy:** `<persistent consequence and owner>`
- **Narrative summary:** `<Derived View with freshness marker>`

## Validation Notes

- [ ] The event exists in Timeline with compatible coordinates.
- [ ] Participants, causes, outcomes, and consequences use typed references.
- [ ] Historical Record contains established facts, not future plans or unconfirmed theories.
- [ ] Interpretations remain observer-specific.
- [ ] Corrections are authorized, traceable, and propagated.
- [ ] Append-only history is preserved except for explicit factual correction.
- [ ] Specialist mechanics are referenced rather than duplicated.

## Cross-References

- [Persistence Authority](../docs/persistence/PERSISTENCE_AUTHORITY.md)
- [Timeline Event Template](TIMELINE_EVENT_TEMPLATE.md)
- [Knowledge View Template](KNOWLEDGE_VIEW_TEMPLATE.md)
- [Continuity Resolution](../docs/persistence/CONTINUITY_RESOLUTION.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
