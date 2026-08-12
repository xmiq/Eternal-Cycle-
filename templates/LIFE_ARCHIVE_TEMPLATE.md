# Life Archive Template

Use this blank storage-neutral template to index one Soul's incarnations and produce concise finalized Life Summaries. Populated archives belong in external campaign state.

## Document Control

- **Template owner:** [Life Archive and Old-Soul Indexing](../docs/persistence/LIFE_ARCHIVE.md)
- **Dependencies:** stable Soul, Incarnation, Life, event, and source-owner identities
- **Optional fields:** every field marked optional or not established by Canon
- **Consumers:** player Life Over views, GM historical retrieval, migration, and validation

## Usage Guidance

1. Keep one stable Life ID per Incarnation.
2. Index active Lives without finalizing a summary.
3. Finalize only after a canonical terminal transition.
4. Reference owner records rather than copying mutable state.
5. Preserve unknowns and disclosure boundaries.

## Soul Overview

- **Soul ID:** `<stable Soul ID>`
- **Archive version:** `<version>`
- **Life count:** `<established count or Unknown>`
- **Active Life ID:** `<Life ID, Interlife, or none>`
- **Index entries:**

| Life ID | Ordinal | Identity | Principal embodiment | World/Age | Temporal bounds | Status | Final Death | Summary |
|---|---:|---|---|---|---|---|---|---|
| `<stable ID>` | `<positive integer or Unknown>` | `<name/alias or Unknown>` | `<Species/Form reference>` | `<reference or Unknown>` | `<known, Estimated, or Unknown>` | `<active/completed>` | `<event, not complete, or Unknown>` | `<reference or not finalized>` |

## Life Record

- **Life ID:** `<stable opaque ID>`
- **Soul ID:** `<stable Soul ID>`
- **Incarnation ID:** `<stable Incarnation ID>`
- **Ordinal:** `<positive integer or Unknown>`
- **Status:** `<active/completed>`
- **Identity and aliases:** `<references>`
- **Temporal scope:** `<world, Contact Domain, Age, interval, and uncertainty>`
- **Starting embodiment:** `<reference or Unknown>`
- **Major embodiment transitions:** `<ordered references; optional>`
- **Principal/final embodiment:** `<reference or Unknown>`
- **Finalization event:** `<event reference; completed Lives only>`
- **Creation/recovery provenance:** `<source or Migration ID>`

## Finalized Life Summary

Complete only for a completed Life.

- **Summary version:** `<positive integer>`
- **Finalized at:** `<campaign chronology or transaction time>`
- **Finalized by event:** `<terminal event reference>`
- **Who this incarnation became:** `<concise established summary>`
- **How it developed:** `<major Development, Skill, Class, Profession, or form references>`
- **What mattered:** `<major Relationships, companions, discoveries, factions, projects, creations, or infrastructure>`
- **How it ended:** `<Final Death/terminal transition reference and concise account>`
- **What it left behind:** `<world consequences and unresolved legacy>`
- **What changed for the Soul:** `<source-owned Soul consequences>`
- **Historical references:** `<typed stable references>`
- **Unknowns and source gaps:** `<explicit uncertainty>`
- **Visibility/redactions:** `<player-visible content and protected omissions>`
- **Revision provenance:** `<sources, correction, or Migration ID>`

## Historical Reference Entry

- **Reference type:** `<Timeline, History, Relationship, Skill, Development, Species, Soul Weapon, Research, Project, Infrastructure, Location, Faction, Mystery, Akashic Record, or extension>`
- **Referenced record ID:** `<stable ID>`
- **Historical role:** `<peak, milestone, participant, manifestation, consequence, unresolved legacy, or other bounded role>`
- **Embodiment interval:** `<reference; optional>`
- **Sort key:** `<chronology or display ordering; optional>`
- **Provenance:** `<source>`

## Life Over Presentation

This is a Derived player-facing view, not an authoritative record.

### `<Identity or Life label>`

- **Life:** `<Life ID / ordinal>`
- **Embodiment:** `<major forms>`
- **Era:** `<world/Age/period>`
- **Development:** `<defining peaks and changes>`
- **Bonds and legacies:** `<major historical relationships and consequences>`
- **Ending:** `<established terminal account>`
- **Soul consequence:** `<established result>`
- **Unresolved:** `<legacy hooks or Unknown>`

> Player access to this presentation does not imply current-character recall.

## Validation Notes

- [ ] Life ID is unique and maps to exactly one Soul and Incarnation.
- [ ] Ordinal is positive or explicitly unknown and does not conflict within the Soul.
- [ ] No Soul has more than one active Life.
- [ ] Active Lives have no finalized Life Summary.
- [ ] Completed Lives have a valid terminal/finalization basis.
- [ ] Every typed reference resolves or is reported as a Record Gap.
- [ ] Summary facts are Historical Snapshots, references, or Derived presentation rather than competing current-state owners.
- [ ] Unknown history remains unknown.
- [ ] Player visibility has not been promoted into Character Knowledge.
- [ ] Akashic Archive references remain distinct from the Life Archive.

## Cross-References

- [Life Archive and Old-Soul Indexing](../docs/persistence/LIFE_ARCHIVE.md)
- [Soul Continuity Record Template](SOUL_CONTINUITY_TEMPLATE.md)
- [Campaign History Entry Template](CAMPAIGN_HISTORY_TEMPLATE.md)
- [Timeline Event Template](TIMELINE_EVENT_TEMPLATE.md)
- [Persistence Validation Report Template](VALIDATION_REPORT_TEMPLATE.md)
