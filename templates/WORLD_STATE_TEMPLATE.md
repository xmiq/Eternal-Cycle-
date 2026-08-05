# World-State Record Template

Use this storage-neutral template to index one bounded current world scope without turning the World Engine into a campaign, one global score, or a player-centered script.

A populated World-State Record belongs outside the repository. This blank template creates no Age, population, economy, faction, conflict, disease, Dungeon, Gate, Stability claim, or future event.

## Document Control

- **Template owner:** World in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary mechanical owner:** [World Engine](../docs/world-engine/README.md)
- **Dependencies:** scoped World-State Claims, specialist records, Timeline, Causal Event Chains, Simulation Frames, and evidence
- **Extensions:** Locations, Settlements, Factions, Dungeons, Gates, Ages, Resets, and every specialist world domain
- **Consumers:** GM simulation, session preparation, actors, Research, Projects, Timeline, Save Updates, migration, and validation
- **Repository boundary:** no current population, resource, economy, war, disease, advancement, Dungeon, Gate, Stability, or world value belongs here

## Usage Guidance

1. Define scope and horizon before selecting state.
2. Index specialist facts by typed reference; do not duplicate them into one world sheet.
3. Preserve direct evidence, estimates, forecasts, beliefs, and secrets separately.
4. Use Simulation Frames to vary detail without changing truth.
5. Advance causes, Counterforces, delays, feedback, and off-screen actors independently of the player.
6. Record Pending Consequences and Review Points rather than scripting outcomes.

## Required Fields

- **World-State Record ID:** `<stable ID>`
- **World or Contact Domain ID:** `<stable scope ID>`
- **Scope and horizon:** `<region, systems, actors, and time window>`
- **Current Age Claims:** `<IDs, evidence, and disputed boundaries>`
- **Authoritative module:** `World`
- **Effective time:** `<World chronology reference>`
- **Truth Layer and visibility:** `<reference>`
- **Source basis:** `<specialist records and evidence>`
- **Simulation Frame:** `<frame ID and detail level>`
- **Validation status:** `<result or not yet validated>`

## World Graph Index

- **Regions and Locations:** `<typed references>`
- **Populations:** `<Population records and current scopes>`
- **Resources and Food:** `<supply, access, renewal, depletion, reserves, and Waste references>`
- **Economies:** `<production, allocation, exchange, Price, obligation, and adaptation references>`
- **Ecology and Migration:** `<habitats, relationships, routes, pressures, and Novel Ecology references>`
- **Factions and Institutions:** `<Profiles and active Versions>`
- **War and Unrest:** `<conflict, control, cessation, and Legacy references>`
- **Disease:** `<agent, exposure, outbreak, care, and evolution references>`
- **Technology and Magic advancement:** `<Discovery, Validation, Adoption, Diffusion, infrastructure, and decline references>`
- **Dungeons:** `<Dungeon Profile references>`
- **World Stability:** `<scoped Referents, supports, strains, couplings, buffers, and forecasts>`
- **World Gates and contact:** `<Event and Gate Profile references>`
- **Ages and World Resets:** `<Claims, mechanisms, footprints, survivorship, and Revalidation references>`

## Causality and Simulation

- **Active Causal Event Chains:** `<node and branch references>`
- **Counterforces:** `<actors and processes resisting or redirecting change>`
- **Feedback loops:** `<reinforcing or balancing relationships>`
- **Delays:** `<information, mobilization, biological, ecological, economic, or institutional lags>`
- **Off-screen activity:** `<actor and Faction actions with evidence>`
- **Pending Consequences:** `<owner, trigger, horizon, and Review Point>`
- **Uncertainty:** `<unknowns, estimates, disputed claims, and required evidence>`
- **Next simulation boundary:** `<event, interval, or question that warrants advancement>`

## Optional Fields

- **World boundaries and Contact Domains:** `<scope relations>`
- **Major dependencies:** `<cross-domain supports and bottlenecks>`
- **Current crises and recoveries:** `<specialist references>`
- **Dormant pressures:** `<established but inactive causes>`
- **Forecasts:** `<assumptions, branches, confidence, and expiry>`
- **Revalidation requirements:** `<Age, Reset, Gate, or major-law changes>`

## Validation Notes

- [ ] Every fact has one specialist owner and the world record is an index, not a competing copy.
- [ ] Scope, horizon, effective time, evidence, and uncertainty are explicit.
- [ ] No universal world score, civilization level, threat rank, or encounter scaling exists.
- [ ] Actors and systems continue without player presence.
- [ ] Forecasts remain conditional and do not become scripted future truth.
- [ ] Simulation abstraction changes detail rather than truth or mechanical outcomes.
- [ ] World Resets preserve causality, survivorship, and Revalidation.
- [ ] All current values remain in the external Campaign Record.

## Cross-References

- [World Engine Overview](../docs/world-engine/WORLD_ENGINE_OVERVIEW.md)
- [World-State Variables](../docs/world-engine/WORLD_STATE_VARIABLES.md)
- [Causal Event Chains](../docs/world-engine/CAUSAL_EVENT_CHAINS.md)
- [Simulation Abstraction](../docs/world-engine/SIMULATION_ABSTRACTION.md)
- [Settlement Record Template](SETTLEMENT_TEMPLATE.md)
- [Faction Profile Template](FACTION_TEMPLATE.md)
- [Dungeon Profile Template](DUNGEON_TEMPLATE.md)
- [World-Contact and Gate Event Record Template](GATE_EVENT_TEMPLATE.md)
