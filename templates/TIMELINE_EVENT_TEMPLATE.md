# Timeline Event Template

Use this storage-neutral template to place one event or interval within world, campaign, session, personal, and Soul chronologies without treating narration order, prophecy, or a calendar label as objective causal order.

A populated Timeline Event belongs outside the repository. This blank template creates no event, date, sequence, Age, time skip, death, Reincarnation, or historical fact.

## Document Control

- **Template owner:** Timeline in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary semantic owner:** [Timeline Engine](../docs/persistence/TIMELINE_ENGINE.md)
- **Dependencies:** stable event identity, temporal coordinates, precision, chronological relations, sources, and affected records
- **Extensions:** Campaign History, sessions, personal lives, Soul chronology, Ages, time skips, World Resets, and parallel events
- **Consumers:** every campaign module, session logs, Campaign History, Save Updates, migration, continuity resolution, and validation
- **Repository boundary:** no campaign event, date, interval, session, life, Age transition, or live chronology belongs here

## Usage Guidance

1. Give each event a stable identity independent of its summary or date label.
2. Record temporal precision honestly; do not invent exact dates.
3. Separate chronology from causal claims.
4. Link parallel, overlapping, contained, interrupted, and uncertain events explicitly.
5. Correct facts through authorized supersession rather than silent replacement.

## Required Fields

- **Timeline Event ID:** `<stable ID>`
- **Event or interval type:** `<event | interval | session | life | Age | time skip | Reset | other supported type>`
- **Scope:** `<world, campaign, session, person, Soul, place, or process>`
- **Start coordinate:** `<calendar, relative order, or unknown>`
- **End coordinate:** `<coordinate, ongoing, instantaneous, or unknown>`
- **Temporal precision:** `<exact | bounded | approximate | relative | disputed | unknown>`
- **Chronologies affected:** `<World History | Campaign History | Session Log | Personal Chronology | Soul Chronology>`
- **Sources:** `<records, observations, adjudication, migration, or correction>`
- **Truth Layer and visibility:** `<reference>`
- **Validation status:** `<result or not yet validated>`

## Temporal Relations

- **Before:** `<Event IDs>`
- **After:** `<Event IDs>`
- **During or overlaps:** `<Event IDs>`
- **Contains or contained by:** `<Event IDs>`
- **Simultaneous within precision:** `<Event IDs>`
- **Parallel but unsynchronized:** `<Event IDs and uncertainty>`
- **Causal predecessors:** `<separate causal claim references>`
- **Causal successors:** `<separate causal claim references>`
- **Sequence conflicts:** `<competing sources and required resolution>`

## Event Summary and Handoffs

- **Participants:** `<typed references>`
- **Locations:** `<typed references>`
- **Established occurrence:** `<bounded factual summary>`
- **Immediate result:** `<specialist owner references>`
- **Affected records:** `<Affected Set>`
- **Campaign History entry:** `<reference when historically material>`
- **Knowledge effects:** `<who learned what and when>`
- **Pending Consequences:** `<future owner and Review Point>`

## Optional Fields

- **Calendar conversion:** `<source calendar, target calendar, method, and uncertainty>`
- **Age Claim:** `<classification and evidence>`
- **Time Skip:** `<elapsed interval and separately simulated changes>`
- **Reincarnation stages:** `<linked Final Death through embodiment events>`
- **Authorized correction:** `<previous coordinate, accepted coordinate, authority, and reason>`
- **Planned or conditional event:** `<plan or forecast layer; never scheduled truth>`

## Validation Notes

- [ ] Event identity, scope, coordinates, precision, and sources are explicit.
- [ ] Temporal and causal relations remain separate.
- [ ] No chronology cycle or impossible containment exists.
- [ ] Parallel events preserve independent clocks and uncertainty.
- [ ] Prophecy, plan, forecast, and narration order do not become future truth.
- [ ] Time skips retain off-screen causality and do not grant automatic progress.
- [ ] Corrections preserve supersession and authority.

## Cross-References

- [Timeline Engine](../docs/persistence/TIMELINE_ENGINE.md)
- [Campaign History Template](CAMPAIGN_HISTORY_TEMPLATE.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Soul Continuity Record Template](SOUL_CONTINUITY_TEMPLATE.md)
- [World-State Record Template](WORLD_STATE_TEMPLATE.md)
