# Knowledge View Template

Use this storage-neutral template to record what one observer can access, remember, believe, infer, or misunderstand without copying protected world truth or treating player knowledge as character knowledge.

A populated Knowledge View belongs outside the repository. This blank template creates no fact, memory, discovery, belief, secret, theory, or access.

## Document Control

- **Template owner:** Knowledge or Secrets in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary semantic owner:** [Truth Layers](../docs/persistence/TRUTH_LAYERS.md)
- **Dependencies:** observer identity, authoritative fact, source, access route, timing, interpretation, confidence, and visibility
- **Extensions:** Character Knowledge, Research, Player Theories, Rumours, GM Secrets, records, memory, and misinformation
- **Consumers:** narration, NPC behaviour, Research, Mysteries, Relationships, Factions, session preparation, and validation
- **Repository boundary:** no observer-specific knowledge, belief, secret, rumour, theory, or concealed campaign fact belongs here

## Usage Guidance

1. Create one view per observer or genuinely shared knowledge group.
2. Reference authoritative facts without copying protected content into lower-visibility records.
3. Separate access, memory, belief, interpretation, confidence, and truth.
4. Preserve mistaken beliefs and rumours until evidence changes them.
5. Never fill missing information with invented certainty.

## Required Fields

- **Knowing Entity ID:** `<stable Entity reference; not the Controller unless independently the knower>`
- **Perspective ID:** `<viewpoint used for this observation or unknown>`

- **Knowledge View ID:** `<stable ID>`
- **Observer ID:** `<person, Faction, Institution, Population, or authorized audience>`
- **Subject or claim ID:** `<authoritative fact, event, person, place, question, or rumour reference>`
- **Truth Layer:** `<Character Knowledge | Research | Player Theories | Rumours | GM Secrets | Meta | another valid layer>`
- **Visibility:** `<authorized audience and least-necessary scope>`
- **Acquisition source:** `<observation, testimony, record, memory, inference, Research, or unknown>`
- **Acquisition time:** `<Timeline reference>`
- **Current view status:** `<known | believed | suspected | remembered | forgotten | disputed | unknown>`
- **Validation status:** `<result or not yet validated>`

## View Content

- **Accessible representation:** `<what the observer can actually perceive or recall>`
- **Interpretation:** `<what the observer thinks it means>`
- **Confidence:** `<qualitative state and basis>`
- **Source reliability:** `<observer's assessment and actual evidence where visible>`
- **Known limitations:** `<sensory, cultural, linguistic, temporal, access, memory, or deception limits>`
- **Contradictory information:** `<view or evidence references>`
- **Unresolved questions:** `<unknowns and possible verification routes>`
- **Behavioral relevance:** `<how this view may inform choices without scripting them>`
- **Last reviewed:** `<event or Transaction reference>`

## Protected Reference

- **Authoritative owner:** `<world, actor, relationship, Research, or other record>`
- **Protected fact locator:** `<opaque or scoped reference appropriate to visibility>`
- **Disclosure conditions:** `<if established>`
- **Leak risk:** `<indexes, aliases, summaries, Derived Views, or metadata>`
- **Redaction or partition:** `<implementation-neutral protection>`

Meta information must never enter Campaign Canon. GM Secrets are protected views of established or deliberately prepared information, not a second world truth or an unwritten preferred future.

## Optional Fields

- **Controller reference:** `<only when control affected acquisition or disclosure>`
- **Rumour chain:** `<speaker, audience, transformations, and confidence>`
- **Player Theory:** `<player-authored interpretation, evidence, and status>`
- **Memory condition:** `<clarity, distortion, suppression, or source-owned effect>`
- **Publication or teaching:** `<audience, route, and changed views>`
- **Correction history:** `<prior view, evidence, and updated interpretation>`

## Validation Notes

- [ ] Observer, subject, layer, visibility, source, and time are explicit.
- [ ] Player knowledge, Character Knowledge, world truth, Research, rumours, secrets, and Meta remain separate.
- [ ] Protected content does not leak through labels, indexes, links, summaries, or Derived Views.
- [ ] Missing information remains `Unknown`, `Not Yet Verified`, `Estimated`, or `Requires Source Recovery` as appropriate.
- [ ] Belief and confidence do not silently become truth.
- [ ] NPC action uses only knowledge available to that NPC.
- [ ] Corrections preserve prior mistaken views and their consequences.

## Cross-References

- [Truth Layers](../docs/persistence/TRUTH_LAYERS.md)
- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [Research Record Template](RESEARCH_TEMPLATE.md)
- [Relationship Record Template](RELATIONSHIP_TEMPLATE.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
