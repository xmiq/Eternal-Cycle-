# Autonomous Registry Record Template

Use this storage-neutral blank contract for one persistent autonomous Individual or bounded Group. A populated record belongs in external campaign state.

## Document Control

- **Template owner:** [Autonomous Registry](../docs/persistence/AUTONOMOUS_REGISTRY.md)
- **Primary authority:** autonomous identity and autonomy-specific state
- **Dependencies:** Entity, Controller, Perspective, model, placement, assignment, capabilities, requirements, networks, Relationships, Infrastructure, Timeline, and provenance
- **Repository boundary:** no populated model, unit, group, controller, assignment, report, or campaign fact belongs here

## Record Kind

- **Registry record kind:** `<Individual | Group>`
- **Autonomous ID:** `<stable permanent ID>`
- **Continuity Core:** `<what makes this the same Entity across ordinary change>`
- **Lifecycle status:** `<active | inactive | destroyed | unknown | restored | archived | disputed | other established state>`
- **Quantity:** `<1 for Individual; supported bounded value or explicit unknown for Group>`

## Names and Model

- **Preferred name/designation:** `<display label or unknown>`
- **Nicknames and aliases:** `<label, scope, source, and validity>`
- **Model ID:** `<stable reference or unknown>`
- **Model revision used:** `<reference and effective event>`
- **Model owner:** `<Project, Infrastructure, Magic, Species, campaign design, or other authoritative source>`

## Origin and Lineage

- **Creator reference:** `<Entity, faction, Project, event, or unknown>`
- **Creation event:** `<Timeline/Campaign History reference>`
- **Construction, summoning, batch, predecessor, or parent-system references:** `<typed references>`
- **Source memory identity/version:** `<reference or none>`
- **Creation lineage:** `<ordered references and provenance>`
- **Upgrade lineage:** `<modification events, prior/new configurations, and owner references>`

## Autonomy and Control

- **Autonomy classification:** `<Directed | Task-Bounded | Method-Selecting | Extended-Operation | Goal-Prioritizing | Independent Agent | explicit unknown>`
- **Autonomy scope and limits:** `<functions, conditions, overrides, evidence, and uncertainty>`
- **Current Controller assignments:** `<Controller IDs, scopes, authority, priority, limits, start, and review>`
- **Prior Controller references:** `<historical assignments; do not copy full history>`
- **Independence claim:** `<established | disputed | unknown, with source>`
- **Personhood/sapience claim:** `<established | disputed | unknown | culturally scoped, with owner and evidence>`

Creator and Controller are separate. Autonomy, independence, and personhood are separate.

## Perspective and Memory

- **Perspective references:** `<Perspective IDs, sensory or remote scope, and limits>`
- **Memory continuity:** `<Continuous | Backup-Restored | Partial | Fragmented | Reset | Copied | Forked | Unknown>`
- **Memory provenance:** `<source, coverage, gap, divergence point, and validation>`
- **Related copy/fork identities:** `<source and descendant Autonomous IDs>`

## Current Operational State

- **Authoritative placement reference:** `<placement relation ID; no duplicate current-location truth>`
- **Condition references:** `<body, construct, vehicle, Infrastructure, or effect owner IDs>`
- **Current assignment:** `<Assignment or Project reference, status, and review>`
- **Capability references:** `<Skill, Magic, equipment, Species, model, or Infrastructure function IDs>`
- **Resource requirements:** `<typed owner references and operating relevance>`
- **Maintenance requirements:** `<schedule, materials, facilities, environment, and owner references>`
- **Network memberships:** `<Network ID, role, direction, permissions, connection state, and interval>`

## Last Confirmed State

- **Last Confirmed Report ID:** `<stable report or event reference>`
- **Observation time:** `<Timeline reference or unknown>`
- **Report time:** `<Timeline reference or unknown>`
- **Report source:** `<Entity, sensor, network, record, or unknown>`
- **Confirmed facts:** `<bounded facts established by the report>`
- **Current uncertainty:** `<Unknown | Not Yet Verified | Estimated | Requires Source Recovery | another supported state>`

Do not convert silence or elapsed time into destruction, success, loyalty, location, or condition.

## Relationships and Cross-References

- **Relationship IDs:** `<Relationships owns current relationship state>`
- **Infrastructure IDs:** `<Infrastructure owns physical and operational state>`
- **Inventory/custody IDs:** `<Inventory owns items, quantities, supplies, and custody>`
- **Timeline/Campaign History IDs:** `<creation, upgrades, restoration, forks, control changes, and other events>`
- **Knowledge and Secret references:** `<audience-scoped IDs without leakage>`

## Group-Specific Fields

- **Group basis:** `<why anonymous aggregate identity remains sufficient>`
- **Quantity trace:** `<prior, change, result, source event, and transaction>`
- **Promoted Individual IDs:** `<members removed from anonymous quantity and granted identity>`
- **Split/merge references:** `<source and destination Group IDs with traces>`

Omit this section for an Individual except to reference a source Group promotion.

## Validation Notes

- [ ] Autonomous ID is unique and not duplicated under ordinary Entity ownership.
- [ ] Model and Individual or Group identity are distinct.
- [ ] Individual quantity is one.
- [ ] Creator and Controller are separate references.
- [ ] Controller scopes and authority sources resolve.
- [ ] Placement, assignment, model, capability, requirement, network, Relationship, Infrastructure, and event references resolve.
- [ ] Memory continuity and reconstruction claims have provenance.
- [ ] Copies and forks have distinct IDs and divergence references.
- [ ] Promoted individuals are no longer included anonymously in group quantity.
- [ ] Last-confirmed facts remain distinct from unknown current state.
- [ ] No capability, Inventory, Relationship, Infrastructure, placement, or history is duplicated as authority.

## Cross-References

- [Autonomous Registry](../docs/persistence/AUTONOMOUS_REGISTRY.md)
- [Canonical Data Ownership](../docs/persistence/CANONICAL_DATA_OWNERSHIP.md)
- [Simulation Architecture and Perspective Model](../docs/core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)
- [Save Update Protocol](../docs/persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)
