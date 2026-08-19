# Template Coverage Map

This map shows which blank logical contracts represent every major Eternal Cycle system and Campaign Persistence module. It is a coverage and selection guide, not a campaign schema, storage layout, or new mechanical owner.

All populated instances remain outside the repository. One implementation may use Markdown, paper, linked documents, databases, or another storage technology as long as authority, stable identity, typed references, Truth Layers, versions, and validation remain intact.

## Coverage Standard

A major system is covered when at least one template:

1. names the record owner and mechanical owner;
2. identifies required fields and optional extensions;
3. preserves dependencies, consumers, and cross-references;
4. includes validation notes and repository boundaries;
5. records specialist outcomes without redefining their rules.

Coverage does not require one file per canonical document. A record-family template may cover several closely related Profile or Instance types while routing every claim to its narrower owner.

## Completed Phase Coverage

| Foundation or phase | Primary templates | Coverage boundary |
| --- | --- | --- |
| Phase 0 - Foundation | [System Template](SYSTEM_TEMPLATE.md), [Campaign Canon and Rules Profile](CAMPAIGN_CANON_TEMPLATE.md) | reusable system proposals and campaign-permitted rule choices remain separate from canon governance |
| Phase 1 - Soul Engine | [Soul Continuity Record](SOUL_CONTINUITY_TEMPLATE.md), [Soul Avatar Profile](SOUL_AVATAR_TEMPLATE.md) | Soul identity, Incarnations, persistent structures, access, Avatar emergence, and expression |
| Phase 2 - Development System | [Development Profile](DEVELOPMENT_PROFILE_TEMPLATE.md), [Retained Cross-Life Development](RETAINED_CROSS_LIFE_DEVELOPMENT_TEMPLATE.md), [Character Record](CHARACTER_TEMPLATE.md) | six capability layers, distinct Tracks, bounded retained redevelopment, evidence, and present embodiment |
| Phase 3 - Skill Engine | [Skill Record](SKILL_TEMPLATE.md), [Development Profile](DEVELOPMENT_PROFILE_TEMPLATE.md) | bounded learned capability, tree position, adaptive emergence, crossover, and Skill Development |
| Phase 4 - Monster Evolution | [Species Reference](SPECIES_TEMPLATE.md), [Evolution Tree](EVOLUTION_TREE_TEMPLATE.md) | Species Development, ecology, forms, routes, requirements, mutations, and hidden branches |
| Phase 5 - Human Classes and Professions | [Human Framework Profile](HUMAN_FRAMEWORK_TEMPLATE.md), [Faction Profile](FACTION_TEMPLATE.md) | Classes, Professions, traditions, schools, Social Position, Institutions, change, and constraints |
| Phase 6 - Soul Weapons | [Soul Weapon Record](SOUL_WEAPON_TEMPLATE.md), [Inventory and Custody](INVENTORY_CUSTODY_TEMPLATE.md) | Weapon Soul personhood, vessel, bond, consent, forms, manifestations, passage, and separate custody |
| Phase 7 - Magic | [Magic Record](MAGIC_RECORD_TEMPLATE.md), [Development Profile](DEVELOPMENT_PROFILE_TEMPLATE.md) | all canonical magical Profile and Instance families plus Magical Development |
| Phase 8 - World Engine | [World-State Record](WORLD_STATE_TEMPLATE.md), [Location Record](LOCATION_TEMPLATE.md), [Faction Profile](FACTION_TEMPLATE.md), [Settlement Record](SETTLEMENT_TEMPLATE.md), [Dungeon Profile](DUNGEON_TEMPLATE.md), [World-Contact and Gate Event](GATE_EVENT_TEMPLATE.md), [Infrastructure Record](INFRASTRUCTURE_TEMPLATE.md) | world graph, specialist state, places, organizations, contact, persistent systems, causality, and simulation |
| Phase 9 - GM Toolkit | all domain templates plus [Save Index](SAVE_INDEX_TEMPLATE.md) and [Validation Report](VALIDATION_REPORT_TEMPLATE.md) | GM procedures consume the records they need; they do not own a parallel campaign-state format |
| Phase 10 - Campaign Persistence Engine | all control, entity, world, process, history, knowledge, migration, and validation templates below | logical persistence architecture without prescribing storage technology |

## Campaign Persistence Module Coverage

| Logical module | Template coverage | Notes |
| --- | --- | --- |
| Save Index and Protocol | [Save Index](SAVE_INDEX_TEMPLATE.md) | module registry, versions, Save Points, load state, validation, migration, and recovery |
| Context Assembly | [Context Packet](CONTEXT_PACKET_TEMPLATE.md) | non-authoritative Current Scene, Running, and Session context; source navigation, freshness, read evidence, Affected Set result, and cache refresh |
| Campaign Canon and Rules Profile | [Campaign Canon and Rules Profile](CAMPAIGN_CANON_TEMPLATE.md) | permitted premises, options, provisional rulings, retcons, conversions, and control agreements |
| Player State | [Character Record](CHARACTER_TEMPLATE.md), [Development Profile](DEVELOPMENT_PROFILE_TEMPLATE.md) | agency, current actor, embodiment, condition, access, capability, Soul, resources, and objectives |
| Souls and Incarnations | [Soul Continuity Record](SOUL_CONTINUITY_TEMPLATE.md), [Life Archive](LIFE_ARCHIVE_TEMPLATE.md), [Memory Continuity Record](MEMORY_CONTINUITY_TEMPLATE.md), [Soul Avatar Profile](SOUL_AVATAR_TEMPLATE.md), [Soul Weapon Record](SOUL_WEAPON_TEMPLATE.md) | one Soul, distinct lives, completed-life indexes, autobiographical continuity, incarnation-specific recall, protected structures, Avatars, and Weapon Soul relationships |
| Companions | [Character Record](CHARACTER_TEMPLATE.md), [Relationship Record](RELATIONSHIP_TEMPLATE.md) | autonomous person plus campaign role and relationships; companionship is not ownership |
| Actors | [Character Record](CHARACTER_TEMPLATE.md) | NPC and other agentive-person records use the same identity, embodiment, agency, knowledge, and capability boundaries |
| Autonomous Registry | [Autonomous Registry Record](AUTONOMOUS_REGISTRY_TEMPLATE.md) | persistent autonomous Individuals and bounded Groups, Models, Controllers, autonomy, reconstruction, lineage, networks, assignment, uncertainty, and last-confirmed state |
| Relationships | [Relationship Record](RELATIONSHIP_TEMPLATE.md) | identity, meetings, dimensions, commitments, shared work, change, death, and Reincarnation |
| Species | [Species Reference](SPECIES_TEMPLATE.md), [Evolution Tree](EVOLUTION_TREE_TEMPLATE.md) | observed Species, forms, populations, lineages, routes, local names, and uncertainty |
| World | [World-State Record](WORLD_STATE_TEMPLATE.md) | top-level graph and typed specialist references |
| Locations | [Location Record](LOCATION_TEMPLATE.md), [Settlement Record](SETTLEMENT_TEMPLATE.md), [Dungeon Profile](DUNGEON_TEMPLATE.md), [World-Contact and Gate Event](GATE_EVENT_TEMPLATE.md) | stable place identity plus specialized place and endpoint records |
| Factions and Institutions | [Faction Profile](FACTION_TEMPLATE.md), [Human Framework Profile](HUMAN_FRAMEWORK_TEMPLATE.md) | organization Versions, participation, governance, authority, information, interests, and continuity |
| Research | [Research Record](RESEARCH_TEMPLATE.md) | complete evidence lifecycle, competing theories, confidence, ethics, loss, and rediscovery |
| Magic | [Magic Record](MAGIC_RECORD_TEMPLATE.md) | campaign-specific Profiles, Instances, sources, access, effects, restrictions, and world handoffs |
| Infrastructure | [Infrastructure Record](INFRASTRUCTURE_TEMPLATE.md) | functions, dependencies, capacity, maintenance, access, failure, and recovery |
| Inventory and Custody | [Inventory and Custody](INVENTORY_CUSTODY_TEMPLATE.md) | item identity, quantity, condition, location, custody, claims, provenance, and transfer |
| Timeline | [Timeline Event](TIMELINE_EVENT_TEMPLATE.md) | five chronologies, precision, temporal relations, parallel events, and corrections |
| Campaign History | [Campaign History Entry](CAMPAIGN_HISTORY_TEMPLATE.md) | append-only established events, outcomes, sources, consequences, and authorized correction |
| Long-Horizon History | [Long-Horizon Summary](LONG_HORIZON_SUMMARY_TEMPLATE.md) | stable period scope, causal compression, continuity hooks, unknowns, historical references, revisions, and filtered transitions |
| Projects | [Project Record](PROJECT_TEMPLATE.md) | undertakings, authority, methods, dependencies, milestones, risks, outputs, and consequences |
| Mysteries | [Mystery Record](MYSTERY_TEMPLATE.md) | unresolved questions, hidden-truth status, clues, branches, actors, stakes, and resolution conditions |
| Knowledge | [Knowledge View](KNOWLEDGE_VIEW_TEMPLATE.md) | observer-specific access, memory, belief, interpretation, confidence, and correction |
| Secrets | [Knowledge View](KNOWLEDGE_VIEW_TEMPLATE.md) | protected view referencing one authoritative truth without leakage or duplication |
| Validation | [Persistence Validation Report](VALIDATION_REPORT_TEMPLATE.md) | read-only baselines, profiles, checks, findings, outcomes, and owner-routed repair |
| Migration History | [Migration Manifest](MIGRATION_MANIFEST_TEMPLATE.md) | Backup, Audit, Merge, Validation, activation, interruption, rollback, and traceability |

## Supporting Record Coverage

| Reusable record family | Template |
| --- | --- |
| Generic canonical system proposal | [System Template](SYSTEM_TEMPLATE.md) |
| Current person or actor | [Character Record Template](CHARACTER_TEMPLATE.md) |
| Persistent autonomous individual or bounded group | [Autonomous Registry Record Template](AUTONOMOUS_REGISTRY_TEMPLATE.md) |
| Capability assessment | [Development Profile Template](DEVELOPMENT_PROFILE_TEMPLATE.md) |
| Bounded learned capability | [Skill Record Template](SKILL_TEMPLATE.md) |
| Human social framework | [Human Framework Profile Template](HUMAN_FRAMEWORK_TEMPLATE.md) |
| Species and form range | [Species Reference Template](SPECIES_TEMPLATE.md) |
| Cross-campaign reusable species design | [Living Codex Species Entry Template](LIVING_CODEX_SPECIES_TEMPLATE.md) |
| Cross-campaign lineage and inheritance design | [Living Codex Inheritance Profile Template](LIVING_CODEX_INHERITANCE_PROFILE_TEMPLATE.md) |
| Monster Evolution routes | [Evolution Tree Template](EVOLUTION_TREE_TEMPLATE.md) |
| Soul continuity | [Soul Continuity Record Template](SOUL_CONTINUITY_TEMPLATE.md) |
| Autobiographical memory continuity and recall | [Memory Continuity Record Template](MEMORY_CONTINUITY_TEMPLATE.md) |
| Soul Avatar | [Soul Avatar Profile Template](SOUL_AVATAR_TEMPLATE.md) |
| Weapon Soul and Soul Weapon | [Soul Weapon Record Template](SOUL_WEAPON_TEMPLATE.md) |
| Magical Profile or Instance | [Magic Record Template](MAGIC_RECORD_TEMPLATE.md) |
| Faction or organization | [Faction Profile Template](FACTION_TEMPLATE.md) |
| Generic place | [Location Record Template](LOCATION_TEMPLATE.md) |
| Settlement | [Settlement Record Template](SETTLEMENT_TEMPLATE.md) |
| Dungeon | [Dungeon Profile Template](DUNGEON_TEMPLATE.md) |
| World contact and Gates | [World-Contact and Gate Event Record Template](GATE_EVENT_TEMPLATE.md) |
| Maintained infrastructure | [Infrastructure Record Template](INFRASTRUCTURE_TEMPLATE.md) |
| Item, lot, container, or custody | [Inventory and Custody Record Template](INVENTORY_CUSTODY_TEMPLATE.md) |

## Cross-Campaign GM Asset Coverage

| Reusable asset family | Template coverage | Boundary |
| --- | --- | --- |
| GM Living Codex Species Registry and inheritance | [Living Codex Species Entry](LIVING_CODEX_SPECIES_TEMPLATE.md), [Living Codex Inheritance Profile](LIVING_CODEX_INHERITANCE_PROFILE_TEMPLATE.md), [Species Reference](SPECIES_TEMPLATE.md), [Evolution Tree](EVOLUTION_TREE_TEMPLATE.md) | separately deployed reusable design; never a current campaign species, genealogy, population, or character record |

## Template Selection Rules

1. Start with the authoritative module, not the scene in which a fact appeared.
2. Use the narrowest template that owns the record and link supporting templates through typed references.
3. Use one stable identity for one subject across every view.
4. Do not duplicate a fact merely because several templates consume it.
5. Use `Unknown`, `Not Yet Verified`, `Estimated`, `Requires Source Recovery`, or another canonical uncertainty state when evidence is incomplete.
6. Keep populated records, saves, backups, migrations, validation reports, and play history outside this repository.
7. When no template fits, first verify whether the need is a missing record view, a missing canonical mechanic, or only a storage preference. A template cannot invent a mechanic.

## Coverage Result

Every completed gameplay phase and every logical Campaign Persistence module has a reusable blank contract. Later playtesting may reveal usability or representation issues; record evidence-backed candidates in the [Future Revisions register](../design/FUTURE_REVISIONS.md) rather than changing mechanics during standardization.
