# Infrastructure Record Template

Use this storage-neutral template for one maintained system whose function depends on connected people, knowledge, resources, sites, tools, institutions, supply, repair, and operating conditions.

A populated Infrastructure Record belongs outside the repository. This blank template creates no road, workshop, ward, laboratory, school, service, capacity, ownership, or current condition.

## Document Control

- **Template owner:** Infrastructure in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary mechanical owners:** [World Engine](../docs/world-engine/README.md) and each specialist function owner
- **Dependencies:** Locations, components, operators, knowledge, resources, access, institutions, maintenance, and Timeline
- **Extensions:** Magic, Research, Projects, economy, health, education, transport, communication, production, and settlement services
- **Consumers:** Settlements, Factions, Projects, Research, world simulation, Inventory, Timeline, Save Updates, and validation
- **Repository boundary:** no named facility, network, operator, resource stock, capacity, failure, project, or live service state belongs here

## Usage Guidance

1. Define the system by function and continuity, not prestige or visual size.
2. Record capacity separately for each function and condition.
3. Link people, sites, components, resources, knowledge, and institutions rather than absorbing them.
4. Preserve maintenance, supply, failure, recovery, access, and externalities.
5. Distinguish ownership, custody, operation, authority, service access, and benefit.

## Required Fields

- **Infrastructure ID:** `<stable ID>`
- **Name and type:** `<local label and functional category>`
- **Continuity Core:** `<what makes this the same maintained system across repair or change>`
- **Primary functions:** `<bounded services or outputs>`
- **Service scope:** `<places, populations, actors, and purposes>`
- **Locations and components:** `<typed references>`
- **Operators and responsible institutions:** `<distinct references>`
- **Current condition:** `<operational state with evidence>`
- **Effective interval:** `<Timeline reference>`
- **Truth Layer and visibility:** `<reference>`
- **Validation status:** `<result or not yet validated>`

## Function and Dependency Entry

- **Function ID:** `<stable local ID>`
- **Output or service:** `<exact function>`
- **Current capacity:** `<qualitative, function-specific finding>`
- **Demand and beneficiaries:** `<separate Population, Actor, or Institution references>`
- **Inputs:** `<resources, energy, Mana, labor, knowledge, tools, sites, and access>`
- **Dependencies:** `<other infrastructure, supply routes, relationships, law, and environment>`
- **Bottlenecks:** `<current limiting causes>`
- **Maintenance:** `<tasks, frequency, responsible actors, and required inputs>`
- **Failure modes:** `<warning, degradation, interruption, and consequences>`
- **Recovery and substitutes:** `<actual routes and limits>`
- **Externalities:** `<waste, ecology, inequality, risk, or other specialist handoffs>`

## Authority and Access

- **Ownership claims:** `<claimants and evidence>`
- **Custody and physical control:** `<current actors>`
- **Operational authority:** `<who may change which functions>`
- **Legitimacy and jurisdiction:** `<audience and scope>`
- **User access:** `<eligibility, cost, priority, exclusion, and alternatives>`
- **Knowledge custody:** `<procedures, records, secrets, and training>`
- **Security and sabotage exposure:** `<routes, safeguards, and uncertainty>`

## Optional Fields

- **Project history:** `<construction, expansion, repair, reform, or abandonment references>`
- **Magic Profile:** `<Enchantment, Ritual, source, Mana, or restriction references>`
- **Research and advancement:** `<Discovery, Validation, Adoption, and Diffusion references>`
- **Institutional memory:** `<people, procedures, archives, and vulnerabilities>`
- **Pending Consequences:** `<owner and Review Point>`

## Validation Notes

- [ ] No universal infrastructure level or settlement bonus is present.
- [ ] Capacity is function-, condition-, and scope-specific.
- [ ] Components, operators, institutions, resources, and knowledge retain separate owners.
- [ ] Maintenance, supply, access, failure, recovery, and externalities are represented.
- [ ] Ownership, custody, control, authority, legitimacy, and use remain separate.
- [ ] Elapsed time or resource expenditure alone does not complete improvement.
- [ ] World consequences route to their specialist owners.

## Cross-References

- [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- [Technology and Magical Advancement](../docs/world-engine/TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md)
- [Project Record Template](PROJECT_TEMPLATE.md)
- [Settlement Record Template](SETTLEMENT_TEMPLATE.md)
- [Inventory and Custody Template](INVENTORY_CUSTODY_TEMPLATE.md)
