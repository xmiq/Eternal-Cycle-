# Soul-Bound Companion Record Template

Use this storage-neutral blank template for one pair-specific Soul-Bound Companion Bond, its current Convergence Interval, and historical Reunion Manifestations. Populated records belong outside this repository.

## Document Control

- **Template owner:** Soul/fate domain for Bond identity and convergence continuity
- **Canonical rules:** [Soul-Bound Companion Fate and Reincarnation Continuity](../docs/soul/SOUL_BOUND_COMPANIONS.md)
- **Dependencies:** [Canonical Data Ownership](../docs/persistence/CANONICAL_DATA_OWNERSHIP.md), [Relationships](../docs/persistence/RELATIONSHIP_MEMORY_ENGINE.md), and [Memory Continuity](../docs/soul/MEMORY_CONTINUITY.md)
- **Repository boundary:** blank reusable structure only; no campaign Souls, companions, encounters, or routes

## Bond Identity

- **Bond ID:** `<stable ID>`
- **Soul A ID:** `<stable Soul ID in canonical pair order>`
- **Soul B ID:** `<stable Soul ID in canonical pair order>`
- **Pair-order rule:** `<deterministic ordering used to prevent reversed duplicates>`
- **Bond status:** `<active | suspended | damaged | disputed | released only under an explicit future rule>`
- **Formation event ID:** `<Timeline or Campaign History reference>`
- **Formation mechanism:** `<explicit canonical route>`
- **Formation basis and participation:** `<shared basis, consent or contested resolution, cost, and provenance>`
- **Ruleset and campaign version:** `<version references>`

## Current Convergence Interval

- **Interval ID:** `<stable ID>`
- **Opened by:** `<separation, Final Death, Reincarnation, migration, or other event reference>`
- **State:** `<Pending | Approaching | Fulfilled | Obstructed | Suspended | Disputed>`
- **Opened at:** `<chronology reference>`
- **Soul A current Life and Entity references:** `<stable IDs or Unknown>`
- **Soul B current Life and Entity references:** `<stable IDs or Unknown>`
- **Known obstacles:** `<world, route, embodiment, chronology, information, or other established obstacles>`
- **Possible path status:** `<established references or Unknown; never a fabricated plan>`
- **Protected route or timing:** `<GM Secret reference where applicable>`
- **Review point:** `<event or bounded review condition; not a countdown>`

## Reunion Manifestation

Repeat only for established historical manifestations.

- **Manifestation ID:** `<stable ID>`
- **Interval ID:** `<stable reference>`
- **Soul, Life, and Entity references:** `<both participants>`
- **Encounter event ID:** `<Timeline or Campaign History reference>`
- **Causal route:** `<world-owned route and provenance>`
- **Meaningful encounter basis:** `<interaction, consequence, recognition opportunity, or later discoverable connection>`
- **Recognition state:** `<references to Memory Continuity and Character Knowledge; do not copy>`
- **Relationship references:** `<current Relationship IDs; do not copy dimensions>`
- **Fulfillment validation:** `<validator run and result>`

## Interference or Damage

- **Effect ID:** `<stable specialist reference>`
- **Scope:** `<access, expression, route, timing, or status>`
- **Cause and resistance:** `<established source>`
- **Recovery or review conditions:** `<bounded conditions>`
- **Historical consequences:** `<typed references>`

## Revision and Migration

- **Source provenance:** `<formation and continuity sources>`
- **Migration ID:** `<stable migration reference>`
- **Prior Bond IDs or duplicate candidates:** `<references; never silently merge>`
- **Validation run:** `<stable validation reference>`
- **Unknown or Requires Source Recovery:** `<preserved gaps>`

## Validation Notes

- [ ] Exactly two distinct valid Soul IDs participate.
- [ ] Canonical pair ordering prevents a reversed duplicate.
- [ ] Formation has an explicit mechanism, event, and provenance.
- [ ] At most one active Convergence Interval exists.
- [ ] Eventual reunion is not represented as a percentage or deadline.
- [ ] Candidate, travel, Gate, world, and embodiment validity remain with their owners.
- [ ] Current Relationship state is referenced rather than copied.
- [ ] Recognition and recall remain with Memory Continuity and Character Knowledge.
- [ ] Fulfillment references a meaningful causal encounter.
- [ ] Multiple bonds remain independent and non-transitive.
- [ ] No automatic control, tracking, telepathy, shared death, resurrection, or restored social role is implied.
- [ ] The complete Affected Set was validated before activation.

## Cross-References

- [Soul-Bound Companion Fate and Reincarnation Continuity](../docs/soul/SOUL_BOUND_COMPANIONS.md)
- [Soul Continuity Template](SOUL_CONTINUITY_TEMPLATE.md)
- [Relationship Template](RELATIONSHIP_TEMPLATE.md)
- [Memory Continuity Template](MEMORY_CONTINUITY_TEMPLATE.md)
- [Life Archive Template](LIFE_ARCHIVE_TEMPLATE.md)
- [Timeline Event Template](TIMELINE_EVENT_TEMPLATE.md)
