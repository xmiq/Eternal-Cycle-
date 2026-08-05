# Inventory and Custody Record Template

Use this storage-neutral template for one ordinary item, lot, container, or custody claim without confusing ownership, possession, access, use, legal claim, bond, or knowledge.

A populated Inventory Record belongs outside the repository. This blank template creates no item, quantity, owner, custodian, transfer, loss, or current location.

## Document Control

- **Template owner:** Inventory and Custody in the [Structured Persistence Architecture](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md)
- **Primary mechanical owners:** the item's specialist system, world causality, and Inventory for custody state
- **Dependencies:** stable item identity, condition, Location, containers, actors, claims, provenance, and Timeline
- **Extensions:** equipment, materials, Reagents, enchanted Hosts, Soul Weapon vessel custody, Project inputs, trade, and evidence
- **Consumers:** Character records, Locations, Projects, economy, crafting, Magic, Soul Weapons, Timeline, Save Updates, migration, and validation
- **Repository boundary:** no current possession, quantity, currency balance, item condition, owner, transfer, or container contents belongs here

## Usage Guidance

1. Give each unique item or fungible lot an appropriate stable identity.
2. Record quantity only when mechanically justified and sourced.
3. Separate physical location, custody, ownership, legal claim, access, permission, and actual use.
4. Append transfer, consumption, damage, loss, recovery, division, and combination events.
5. Treat a Weapon Soul as a person; Inventory may reference vessel custody but never owns the Weapon Soul.

## Required Fields

- **Inventory Record ID:** `<stable ID>`
- **Subject type:** `<unique item | fungible lot | container | custody claim | other supported record>`
- **Item or lot ID:** `<stable identity>`
- **Description and category:** `<evidence-based identification>`
- **Quantity:** `<established amount, estimate, unknown, or not applicable>`
- **Current condition:** `<state and evidence>`
- **Current Location:** `<Location or container reference>`
- **Current custodian:** `<Actor, Faction, Institution, or none>`
- **Effective interval:** `<Timeline reference>`
- **Truth Layer and visibility:** `<reference>`
- **Validation status:** `<result or not yet validated>`

## Claims and Access

- **Ownership:** `<recognized owner and basis>`
- **Competing claims:** `<claimants, law, custom, evidence, and dispute>`
- **Physical custody:** `<who can presently control the item>`
- **Access:** `<who can reach it and under what conditions>`
- **Permission to use:** `<granting authority, scope, and limits>`
- **Actual user:** `<current or latest use reference>`
- **Bond or personhood:** `<separate Soul Weapon, living vessel, or agentive-object record>`
- **Knowledge:** `<who knows location, nature, quantity, or claim>`

## Provenance and Change

- **Origin:** `<creation, acquisition, source, or unknown>`
- **Transfer history:** `<event references>`
- **Containers and contents:** `<typed parent-child references>`
- **Dependencies:** `<maintenance, fuel, ammunition, compatibility, permits, or other conditions>`
- **Consumption or depletion:** `<mechanically justified event and amount>`
- **Damage, repair, division, or combination:** `<identity and condition consequences>`
- **Loss, theft, destruction, or recovery:** `<event and evidence>`
- **Current valuation or exchange claim:** `<observer-, market-, and time-specific reference>`

## Optional Fields

- **Skill and embodiment requirements:** `<use does not imply capability>`
- **Magic or Enchantment:** `<separate Magic Record>`
- **Reagent properties:** `<Alchemy and Research references>`
- **Project allocation:** `<reserved input without changing custody by implication>`
- **Evidence function:** `<Research, Mystery, or legal record reference>`
- **Review Points:** `<inventory recount, condition check, or claim resolution>`

## Validation Notes

- [ ] Identity, quantity, condition, location, and custody have evidence.
- [ ] Ownership, custody, claim, access, permission, use, bond, and knowledge remain separate.
- [ ] Numerical changes have mechanically justified events and sources.
- [ ] Containers have no cycles, dangling contents, or duplicated quantities.
- [ ] Transfer updates both origin and destination records atomically.
- [ ] Reincarnation moves no ordinary inventory by default.
- [ ] Weapon Souls and other persons are never reduced to equipment records.

## Cross-References

- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
- [Character Record Template](CHARACTER_TEMPLATE.md)
- [Soul Weapon Record Template](SOUL_WEAPON_TEMPLATE.md)
