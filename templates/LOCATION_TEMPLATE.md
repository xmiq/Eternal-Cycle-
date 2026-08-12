# Location Record Template

Use this storage-neutral template for one stable place identity and its current conditions without treating a map label, owner, settlement, Dungeon, Gate endpoint, or historical name as the whole place.

A populated Location belongs in an external Campaign Record. This blank template establishes no place, boundary, occupant, route, claim, hazard, or world state.

## Document Control

- **Template owner:** Locations in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary mechanical owners:** [World Engine](../docs/world-engine/README.md) and the specialist systems linked by each condition
- **Dependencies:** place identity, boundaries, access, environment, occupants, claims, infrastructure, Timeline, and evidence
- **Extensions:** Settlements, Dungeons, World Gate endpoints, Infrastructure, Factions, ecology, and Research
- **Consumers:** travel, encounters, world simulation, Inventory, Projects, Relationships, Timeline, Save Updates, and validation
- **Placement ownership:** the campaign schema designates one authoritative current-placement relation; occupant lists here are Derived unless that relation is explicitly owned here
- **Repository boundary:** no named current place, resident, route, claim, resource, hazard, or condition belongs here

## Usage Guidance

1. Give the place a stable ID independent of names, owners, and current condition.
2. Define boundaries for the question at hand and preserve disputed boundaries.
3. Link occupants, infrastructure, resources, hazards, and claims to their owners.
4. Distinguish physical access, legal access, knowledge, control, and authority.
5. Append renaming, damage, abandonment, reconstruction, occupation, and transformation as history.

## Required Fields

- **Location ID:** `<stable ID>`
- **Names and naming authorities:** `<observer-specific labels>`
- **Location type:** `<region | site | route | settlement | structure | mobile place | Gate endpoint | Dungeon area | other supported type>`
- **Identity and Continuity Core:** `<what makes this the same place across change>`
- **Boundary and scope:** `<physical, functional, legal, magical, ecological, or disputed limits>`
- **Parent and contained Locations:** `<typed references>`
- **Current condition:** `<dated qualitative state>`
- **Effective interval:** `<Timeline reference>`
- **Truth Layer and visibility:** `<reference>`
- **Validation status:** `<result or not yet validated>`

## Current Place View

- **Environment:** `<climate, terrain, Mana, ecology, and habitability references>`
- **Access routes:** `<ordinary, magical, Dungeon, Gate, restricted, hidden, or unavailable routes>`
- **Occupants and populations:** `<Actor and Population references>`
- **Resources and services:** `<specialist records>`
- **Infrastructure:** `<Infrastructure IDs and dependencies>`
- **Hazards and protections:** `<source, scope, evidence, and countermeasures>`
- **Custody and control:** `<who physically controls what>`
- **Ownership and claims:** `<legal, customary, sacred, political, or disputed claims>`
- **Authority and jurisdiction:** `<recognized decisions by audience>`
- **Knowledge views:** `<who knows, believes, or cannot access what>`
- **Pending Consequences and Review Points:** `<references>`

## Optional Fields

- **Settlement Profile:** `<reference>`
- **Dungeon Profile:** `<reference>`
- **World Gate Endpoint:** `<Gate-event reference>`
- **Historical uses and names:** `<Campaign History references>`
- **Projects and Research:** `<current undertaking and discovery references>`
- **Relationships to other places:** `<route, dependency, rivalry, migration, trade, or ecological relation>`
- **Maps and Derived Views:** `<source and freshness; never the authoritative place itself>`

## Validation Notes

- [ ] Stable place identity is separate from name, owner, controller, occupants, and current condition.
- [ ] Boundaries and effective time are explicit.
- [ ] Routes do not silently inherit World Gate, Soul Gate, or Dungeon mechanics.
- [ ] Control, ownership, claim, authority, access, and knowledge remain separate.
- [ ] Occupants remain distinct persons or populations.
- [ ] Historical change is appended rather than resetting place identity without cause.
- [ ] Occupant indexes agree with authoritative subject-placement relations and do not become competing current-location owners.
- [ ] Every specialist condition links to its owner.

## Cross-References

- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [World-State Record Template](WORLD_STATE_TEMPLATE.md)
- [Settlement Record Template](SETTLEMENT_TEMPLATE.md)
- [Dungeon Profile Template](DUNGEON_TEMPLATE.md)
- [World-Contact and Gate Event Record Template](GATE_EVENT_TEMPLATE.md)
