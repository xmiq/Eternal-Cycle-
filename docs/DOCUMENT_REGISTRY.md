# Canonical Document Registry

This registry identifies the responsibility and interface of every major document under `docs/`. It is a navigation and ownership map, not a new rules owner. When this registry and a canonical document disagree about a rule, the canonical document and [accepted design governance](../design/DECISIONS.md) must be reconciled; this registry cannot silently settle the conflict.

## Registry Contract

- **Owner** identifies the class of claims for which a document is authoritative.
- **Dependencies** are canonical foundations a document must preserve and may not redefine.
- **Extensions** are narrower rules, procedures, templates, or campaign-external records allowed to build on its output.
- **Consumers** are systems or operators expected to read and apply its output.

Every document listed beneath a family inherits that family's dependencies, extensions, and consumers. A refinement cell names only an additional or narrower interface. `None` means the family interface applies without refinement.

Indexes own reading order, navigation, and claim routing only. They do not override the documents they index. Examples, templates, GM outputs, AI procedures, and populated campaign records cannot create mechanics merely by consuming a document.

## Repository-Wide Rules Map

**Dependencies:** [Repository Conventions](../design/REPOSITORY_CONVENTIONS.md), [Terminology](../design/TERMINOLOGY.md), [Design Decisions](../design/DECISIONS.md), and every indexed family.

**Extensions:** family indexes and repository navigation.

**Consumers:** all readers, contributors, GMs, audits, and implementations.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Canonical Rules Map](README.md) | Repository-wide canonical navigation and family discovery | Extends into every family index; consumed before selecting a specialist owner. |
| **Canonical Document Registry** | Document ownership, dependency, extension, and consumer navigation | Audits every `docs/` document; creates no gameplay authority. |

## Core

**Dependencies:** accepted design governance and canonical terminology.

**Extensions:** every specialist rules family may narrow the philosophy without contradicting it.

**Consumers:** every canonical system, GM procedure, template, audit, and implementation.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Core Rules Index](core/README.md) | Core-family reading order and specialist-owner routing | Creates no independent mechanical claim. |
| [Design Philosophy](core/DESIGN_PHILOSOPHY.md) | Rule Zero, earned progression, meaningful consequence, fair mystery, and system-wide design constraints | Direct foundation for all families. |
| [Simulation Architecture and Perspective Model](core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md) | Three-layer responsibility architecture; Entity, Controller, Perspective, objective-state, and knowledge boundaries | Connects specialist rules, campaign simulation, persistence, GM operation, and player-facing presentation without replacing them. |

## Soul Engine

**Dependencies:** [Design Philosophy](core/DESIGN_PHILOSOPHY.md), canonical terminology, and accepted Soul decisions.

**Extensions:** Development, Skills, Monster Evolution, Soul Weapons, Magic, World Gates, GM procedures, persistence records, and blank Soul templates may consume established Soul outcomes without redefining them.

**Consumers:** every system that adjudicates identity, death, Reincarnation, cross-life persistence, Soul access, or Soul expression.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Soul Rules Index](soul/README.md) | Soul Engine reading order, guarantees, and claim routing | Routes consumers to the narrowest Soul owner. |
| [Soul Engine Overview](soul/SOUL_ENGINE_OVERVIEW.md) | Shared Soul model and subsystem boundaries | Consumed as the first Soul-system orientation. |
| [Soul Fundamentals](soul/SOUL_FUNDAMENTALS.md) | Soul identity, continuity, personhood, and foundational properties | Required by every other Soul document. |
| [Reincarnation](soul/REINCARNATION.md) | Final Death, transition, candidate selection, new embodiment, and cross-life transfer boundaries | Extended procedurally by Reincarnation Generation and recorded by persistence. |
| [Soul Depth](soul/SOUL_DEPTH.md) | Qualitative Soul capacity, integration, and Depth Horizons | Consumed by advanced Soul access and capability assessment. |
| [Soul Resonance](soul/SOUL_RESONANCE.md) | Contextual alignment among Soul identity, history, beings, places, and phenomena | Extended by bounded specialist triggers and expressions. |
| [Soul Echoes](soul/SOUL_ECHOES.md) | Past-life imprints, awakening, perspective, memory, and advisory expression | Consumed by Soul Avatars, persistence, and GM information handling. |
| [Soul Space](soul/SOUL_SPACE.md) | Internal Soul environment, access, structure, residents, and limits | Extended by Echo, Avatar, and Weapon Soul presence where their owners permit it. |
| [Soul Constellations](soul/SOUL_CONSTELLATIONS.md) | Persistent inter-Soul relationship patterns and recognition boundaries | Consumed by relationship persistence and world-side encounters. |
| [Soul Titles](soul/SOUL_TITLES.md) | Persistent Soul-identity titles, formation, expression, conflict, and transformation | Consumed by reactions, opportunities, symbolic authority, and persistence views. |
| [Retained Instincts](soul/RETAINED_INSTINCTS.md) | Cross-life procedural tendencies and embodiment-bounded recovery | Consumed by Development, Skill crossover, and Monster Evolution. |
| [Akashic Archive](soul/AKASHIC_ARCHIVE.md) | Archive records, access, interpretation, provenance, and safeguards | Consumed by research, mystery, and information procedures without granting automatic truth. |
| [Soul Avatars](soul/SOUL_AVATARS.md) | Soul Avatar emergence, identity, trigger, agency, manifestation, and continuity | Consumed by World-Gate interactions, GM adjudication, and Avatar templates. |
| [Soul Weapon Foundations](soul/SOUL_WEAPON_FOUNDATIONS.md) | Soul-side conditions and boundaries shared with the Soul Weapon family | Extended only by the dedicated Soul Weapon owners. |
| [Soul System Interactions](soul/SOUL_SYSTEM_INTERACTIONS.md) | Cross-Soul-system claim routing and interaction order | Consumed whenever more than one Soul subsystem applies. |
| [Soul Engine Safeguards](soul/SOUL_ENGINE_SAFEGUARDS.md) | Shared Soul exploit resistance, agency, consequence, and anti-duplication constraints | Required by every Soul extension and consumer. |

## Development System

**Dependencies:** Core philosophy, Soul embodiment and persistence outcomes, and accepted Development decisions.

**Extensions:** Skill trees, human frameworks, Monster Evolution, Magic, Soul Weapons, capability profiles, and campaign records may apply Development tracks without creating a universal level.

**Consumers:** any system assessing growth, access, embodied expression, practised reliability, contextual effectiveness, or world recognition.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Development Rules Index](progression/README.md) | Development reading order, shared guarantees, and track routing | Routes all growth claims to a specialist track. |
| [Development System](progression/DEVELOPMENT_SYSTEM.md) | Multidimensional Development philosophy and six-layer capability model | Required by every Development track. |
| [Physical Development](progression/PHYSICAL_DEVELOPMENT.md) | Body-conditioned physical adaptation, capacity, control, reliability, and regression | Consumed by embodiment, combat, labor, injury, and species systems. |
| [Skill Development](progression/SKILL_DEVELOPMENT.md) | Development evidence and practice for bounded learned capabilities | Extended by the Skill Engine, which owns Skill structures. |
| [Profession Development](progression/PROFESSION_DEVELOPMENT.md) | Development through sustained productive practice and professional judgment | Consumed by Professions, institutions, economies, and infrastructure. |
| [Magical Development](progression/MAGICAL_DEVELOPMENT.md) | Development of magical perception, control, formation, and reliability | Extended by Magic documents, which own magical phenomena. |
| [Social and Leadership Development](progression/SOCIAL_AND_LEADERSHIP_DEVELOPMENT.md) | Interpersonal, coordination, leadership, and social-practice growth | Consumed by human structures, factions, NPCs, and relationships. |
| [Species Development](progression/SPECIES_DEVELOPMENT.md) | Present-species adaptation and expression within embodied limits | Extended by Monster Evolution and species records. |
| [Stat XP and Retained Development](progression/STAT_XP_AND_RETAINED_DEVELOPMENT.md) | Non-compounding retained redevelopment potential and Stat XP boundaries | Consumed after Reincarnation; cannot bypass present training. |
| [Development Interactions](progression/DEVELOPMENT_INTERACTIONS.md) | Cross-track influence, ownership handoffs, and non-interchangeability | Consumed by mixed capability claims. |
| [Capability Assessment](progression/CAPABILITY_ASSESSMENT.md) | Contextual comparison without a universal score | Consumed by GMs, encounters, characters, and validation. |
| [Development Safeguards](progression/DEVELOPMENT_SAFEGUARDS.md) | Anti-grinding, diminishing returns, plateaus, snowball limits, and agency protections | Required by every Development extension. |

## Skill Engine

**Dependencies:** Development System, current embodiment, human and monster receiving routes, and accepted Skill decisions.

**Extensions:** Classes, Professions, Martial Traditions, Magical Schools, Monster Evolution, Soul Weapons, Magic, GM adjudication, and Skill records may reference established Skills without replacing Development.

**Consumers:** systems resolving learned capabilities, representation, action modes, transformation, crossover, concealment, or conceptual operation.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Skill Engine Index](skills/README.md) | Skill reading order, shared model, schemas, and claim routing | Routes a capability to one Skill owner. |
| [Human Skill Trees](skills/HUMAN_SKILL_TREES.md) | Human learning routes and tree organization | Consumed by human social frameworks and crossover. |
| [Monster Skill Trees](skills/MONSTER_SKILL_TREES.md) | Monster-native learning routes and tree organization | Consumed by species, evolution, and crossover. |
| [Reincarnation Skill Crossover](skills/REINCARNATION_SKILL_CROSSOVER.md) | Bounded cross-life Skill recovery, translation, receiving routes, and Direct Transfer | Consumes Reincarnation outcomes and Retained Instincts. |
| [Adaptive Skills](skills/ADAPTIVE_SKILLS.md) | Emergence of stable novel capabilities through meaningful adaptive practice | Consumed by Skill representation and Development evidence. |
| [Skill Evolution](skills/SKILL_EVOLUTION.md) | Qualitative transformation of an established Skill | Consumed by advanced training and specialist frameworks. |
| [Skill Fusion](skills/SKILL_FUSION.md) | Sustained integration of Component Skills into a distinct competency | Consumed by mixed disciplines; does not duplicate components. |
| [Active and Passive Skills](skills/ACTIVE_AND_PASSIVE_SKILLS.md) | Active, passive, and mixed expression modes | Consumed by action and consequence resolution. |
| [Hidden Skills](skills/HIDDEN_SKILLS.md) | Observer-relative Skill concealment and fair discovery | Consumed by information and uncertainty procedures. |
| [Conceptual Skills](skills/CONCEPTUAL_SKILLS.md) | Bounded Skills operating through principles, relations, meanings, or concepts | Consumed by advanced capability adjudication. |
| [Skill Engine Safeguards](skills/SKILL_ENGINE_SAFEGUARDS.md) | Capability qualification, one-owner representation, proliferation limits, and cleanup | Required by every Skill extension and record. |
| [Level 0 Instinct](skills/LEVEL_ZERO_INSTINCT.md) | Instinctive Skill access represented within existing Skill tracking | Consumed by species, Evolution, and lineage outcomes without granting mastery. |

## Monster Evolution

**Dependencies:** Soul, Development, Skills, Magic, world ecology, and accepted Monster Evolution decisions.

**Extensions:** species references, Evolution Trees, generated monster profiles, world populations, and campaign histories may instantiate established routes without placing live data in the repository.

**Consumers:** monster characters, populations, ecologies, GM generators, Reincarnation, and world simulation.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Monster Evolution Index](monster-evolution/README.md) | Reading order, adjudication sequence, ownership map, and guarantees | Routes ecology, route, form, and individual claims. |
| [Monster Ecology](monster-evolution/MONSTER_ECOLOGY.md) | Species niches, resource webs, relationships, carrying conditions, territory, and reproduction | Supplies ecological evidence to evolutionary pressure and World Engine owners. |
| [Evolutionary Pressures](monster-evolution/EVOLUTIONARY_PRESSURES.md) | Contextual pressures affecting viable forms and routes | Consumed by Monster Evolution and adaptation. |
| [Monster Evolution](monster-evolution/MONSTER_EVOLUTION.md) | Individual qualitative transformation process and qualification | Extended by stages, branches, hidden conditions, mutation, and apex outcomes. |
| [Species Stages](monster-evolution/SPECIES_STAGES.md) | Species-specific developmental stages and transition meaning | Consumed by species references and route adjudication. |
| [Branching Evolution](monster-evolution/BRANCHING_EVOLUTION.md) | Multiple viable Evolution routes, divergence, tradeoffs, and path dependence | Extended by Evolution Tree records. |
| [Hidden Evolution Conditions](monster-evolution/HIDDEN_EVOLUTION_CONDITIONS.md) | Fairly hidden requirements, provenance, clues, inquiry, and disclosure | Consumed by GM uncertainty and Evolution adjudication. |
| [Mutations](monster-evolution/MUTATIONS.md) | Individual or heritable structural variation distinct from full Evolution | Consumed by adaptation, hybridization, and species records. |
| [Apex Monsters](monster-evolution/APEX_MONSTERS.md) | Contextual apex status, maintenance, contest, and ecological consequences | Consumed by ecology and world simulation; not a universal rank. |
| [Monster Societies](monster-evolution/MONSTER_SOCIETIES.md) | Intelligent monster culture, institutions, coordination, and internal diversity | Consumed by factions, NPCs, human contact, and world simulation. |
| [Monster Adaptation](monster-evolution/MONSTER_ADAPTATION.md) | Within-life and population response short of Evolution | Consumed by ecology, Development, and evolutionary pressure. |
| [Hybridization](monster-evolution/HYBRIDIZATION.md) | Bounded mixed-lineage formation, inheritance, viability, and identity | Consumed by species and population records. |
| [Extinction and Replacement](monster-evolution/EXTINCTION_AND_REPLACEMENT.md) | Species decline, extinction, ecological release, and replacement | Consumed by world history, populations, and Ages. |
| [Soul Interactions](monster-evolution/SOUL_INTERACTIONS.md) | Handoffs between Soul history and present monster Evolution | Consumes Reincarnation and Retained Instincts without transferring forms automatically. |
| [Evolution Safeguards](monster-evolution/EVOLUTION_SAFEGUARDS.md) | Anti-farming, embodiment, route, information, and non-determinism safeguards | Required by every Monster Evolution extension. |

## Human Classes and Professions

**Dependencies:** Development, human Skill routes, Magic where relevant, social world state, and accepted human-framework decisions.

**Extensions:** campaign-local Classes, Professions, traditions, schools, institutions, credentials, and social positions may instantiate these structures outside the repository.

**Consumers:** human characters, NPCs, factions, settlements, economies, education, GM generation, and persistence records.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Human Framework Index](human/README.md) | Human-framework reading order, ownership boundaries, and claim resolution | Routes capability, recognition, authority, and membership separately. |
| [Human Class Philosophy](human/HUMAN_CLASS_PHILOSOPHY.md) | Shared purpose and boundaries of human Classes and structured paths | Required by all human-framework documents. |
| [Classes](human/CLASSES.md) | Recognized human role frameworks and participation conditions | Consumed by characters, institutions, and Class Evolution. |
| [Professions](human/PROFESSIONS.md) | Sustained productive practices, professional identity, and recognition | Consumed by economies, infrastructure, and Profession Development. |
| [Martial Traditions](human/MARTIAL_TRADITIONS.md) | Transmitted martial frameworks, lineages, methods, and obligations | Consumed by Skills, institutions, factions, and characters. |
| [Magical Schools](human/MAGICAL_SCHOOLS.md) | Social traditions for teaching and interpreting Magic | Consumes Magic rules; does not own magical effects. |
| [Social Advancement](human/SOCIAL_ADVANCEMENT.md) | Contextual changes in standing, office, trust, and authority | Consumed by factions, institutions, relationships, and world simulation. |
| [Institutions and Academies](human/INSTITUTIONS_AND_ACADEMIES.md) | Organized transmission, certification, membership, and institutional continuity | Consumed by Classes, Professions, Skills, factions, and advancement. |
| [Class Evolution](human/CLASS_EVOLUTION.md) | Change among recognized Class frameworks without automatic capability grants | Consumed by human character and institution records. |
| [Limits of Human Progression](human/LIMITS_OF_HUMAN_PROGRESSION.md) | Shared human-framework constraints, embodiment, access, and anti-status safeguards | Required by every human-framework extension. |

## Soul Weapons

**Dependencies:** Soul Weapon Foundations, Soul identity, Reincarnation, Development, Skills, embodiment, and accepted Soul Weapon decisions.

**Extensions:** blank Soul Weapon records, vessel histories, manifestations, campaign relationships, and specialist magical or crafting records may consume established outcomes.

**Consumers:** wielders, Weapon Souls, Skills, Magic, equipment and custody records, GM procedures, Reincarnation, and persistence.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Soul Weapon Index](soul-weapons/README.md) | Reading order, state model, claim resolution, and system interfaces | Routes vessel, personhood, bond, form, action, and passage claims. |
| [Dormant Weapon Souls](soul-weapons/DORMANT_WEAPON_SOULS.md) | Pre-Awakening identity potential, object history, and dormant boundaries | Supplies but does not guarantee Awakening candidates. |
| [Awakening Conditions](soul-weapons/AWAKENING_CONDITIONS.md) | Contextual Awakening qualification under meaningful shared pressure | Consumed by Soul Intertwining. |
| [Soul Intertwining](soul-weapons/SOUL_INTERTWINING.md) | Consensual enduring relationship between distinct Soul persons | Consumed by Evolution, Reincarnation passage, and relationship persistence. |
| [Weapon Personalities](soul-weapons/WEAPON_PERSONALITIES.md) | Weapon Soul identity, preferences, agency, communication, and change | Consumed by GM NPC handling and relationship records. |
| [Weapon Evolution](soul-weapons/WEAPON_EVOLUTION.md) | Qualitative partner-and-vessel change through shared history | Consumed by forms and manifestations without granting infinite growth. |
| [Weapon Echoes](soul-weapons/WEAPON_ECHOES.md) | Weapon-side historical imprints distinct from Soul Echoes | Consumed by memory, information, and relationship procedures. |
| [Legacy Weapons](soul-weapons/LEGACY_WEAPONS.md) | Awakened or significant weapon continuity without an active original bond | Consumed by inheritance, custody, trust, and history records. |
| [Soul Weapon Compatibility](soul-weapons/SOUL_WEAPON_COMPATIBILITY.md) | Contextual compatibility among persons, vessels, forms, and use | Consumed by expression and manifestation adjudication. |
| [Weapon Manifestations](soul-weapons/WEAPON_MANIFESTATIONS.md) | Bounded external forms and actions of a Weapon Soul | Consumed by encounters, Magic, and consequence resolution. |
| [Unconventional Soul Weapons](soul-weapons/UNCONVENTIONAL_SOUL_WEAPONS.md) | Application of Soul Weapon rules to meaningful nonstandard tools and forms | Extends classification without broadening Awakening guarantees. |

## Magic

**Dependencies:** Mana, embodiment, Development, Skills, source authority, world conditions, and accepted Magic decisions.

**Extensions:** local spells, rituals, enchantments, alchemical processes, divine relationships, restrictions, magical profiles, and campaign records may instantiate these rules outside the repository.

**Consumers:** magical actors, human schools, monster forms, Soul Weapons, world simulation, GM adjudication, research, and persistence.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Magic Index](magic/README.md) | Magic reading order, claim resolution, ownership boundaries, and guarantees | Routes Mana, access, formation, source, object, process, and world claims. |
| [Mana](magic/MANA.md) | Local world-side magical medium, availability, movement, condition, and consequences | Required by every magical phenomenon unless explicitly established otherwise. |
| [Magical Affinities](magic/MAGICAL_AFFINITIES.md) | Target-specific magical compatibility, formation, expression, and conflict | Consumed by spell, ritual, enchantment, alchemy, and embodiment claims. |
| [Spell Formation](magic/SPELL_FORMATION.md) | Bounded spell procedures, grammars, execution, costs, failure, and counterplay | Consumed by magical actors and schools. |
| [Rituals](magic/RITUALS.md) | Prepared multi-part magical structures, roles, integrity, interruption, and aftermath | Consumed by factions, institutions, sites, and world events. |
| [Enchanting](magic/ENCHANTING.md) | Host-anchored magical configurations, layers, maintenance, and Drift | Consumed by equipment, infrastructure, Magic records, and Soul Weapon boundary checks. |
| [Alchemy](magic/ALCHEMY.md) | Controlled material transformation, Reagents, Batches, application, and provenance | Consumed by Professions, economies, medicine, ecology, and inventory. |
| [Divine Magic](magic/DIVINE_MAGIC.md) | Source-bound divine relationships, Domains, Mandates, petitions, and miracles | Consumed by religions, actors, institutions, and world consequences. |
| [Forbidden Magic](magic/FORBIDDEN_MAGIC.md) | Scoped magical restriction, hazard, protected interest, enforcement, and review | Consumed by institutions, factions, knowledge, and GM procedures. |
| [Magic and World Engine Interactions](magic/WORLD_ENGINE_INTERACTIONS.md) | Handoff from established magical events to bounded world consequences | Consumed by every affected World Engine owner. |

## World Engine

**Dependencies:** all specialist mechanical outcomes, Core Rule Zero, persistent causality, and accepted World Engine decisions.

**Extensions:** campaign-external world-state records, event histories, locations, factions, simulations, and forecasts may instantiate current values without entering the repository.

**Consumers:** GM simulation, encounters, Reincarnation candidate sourcing, every actor, persistence, time skips, Age transitions, and AI procedures.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [World Engine Index](world-engine/README.md) | Reading order, ownership boundaries, and campaign-data boundary | Routes world claims to one specialist process owner. |
| [World Engine Overview](world-engine/WORLD_ENGINE_OVERVIEW.md) | Rule Zero, autonomous simulation, causal persistence, and shared operating model | Required by every World Engine document. |
| [World-State Variables](world-engine/WORLD_STATE_VARIABLES.md) | Qualitative representation and evidence of scoped world conditions | Consumed by all world records and simulation passes. |
| [Causal Event Chains](world-engine/CAUSAL_EVENT_CHAINS.md) | Branching, interruptible causal propagation and specialist handoffs | Consumed whenever changes cross systems or scopes. |
| [Populations](world-engine/POPULATIONS.md) | Demographic composition, life cycles, continuity, dependency, and distributed capability | Consumed by ecology, economy, disease, war, factions, and settlements. |
| [Resources and Food](world-engine/RESOURCES_AND_FOOD.md) | Resource access, supply, renewal, depletion, substitution, sustenance, and waste | Consumed by populations, economies, factions, war, and ecology. |
| [Economies](world-engine/ECONOMIES.md) | Production, allocation, exchange, prices, currencies, obligations, and distribution | Consumed by factions, settlements, infrastructure, advancement, and unrest. |
| [Ecology and Migration](world-engine/ECOLOGY_AND_MIGRATION.md) | Current ecosystem dynamics, disturbance, succession, movement, and displacement | Consumes Monster Ecology outputs and drives population and location changes. |
| [Faction Behaviour](world-engine/FACTION_BEHAVIOUR.md) | Distributed faction interests, information, decisions, mobilization, cohesion, and adaptation | Consumed by war, economy, advancement, events, and GM generation. |
| [War and Unrest](world-engine/WAR_AND_UNREST.md) | Contestation, unrest, armed conflict, operations, control, cessation, and legacies | Consumed by populations, factions, economy, locations, and history. |
| [Disease Evolution](world-engine/DISEASE_EVOLUTION.md) | Disease process, transmission, host states, outbreaks, agent change, care, and legacies | Consumed by populations, ecology, institutions, and history. |
| [Technology and Magical Advancement](world-engine/TECHNOLOGY_AND_MAGICAL_ADVANCEMENT.md) | Collective discovery, adoption, infrastructure, maintenance, decline, and recovery | Consumes Skill, Profession, institution, resource, and Magic outcomes. |
| [Dungeon Activity](world-engine/DUNGEON_ACTIVITY.md) | Dungeon formation, boundaries, topology, activity, ecology, access, and collapse | Consumed by locations, populations, encounters, resources, and GM generation. |
| [World Stability](world-engine/WORLD_STABILITY.md) | Scoped continuity, supports, strain, buffers, responses, and transformation findings | Consumed by Age and Reset assessment without becoming a universal meter. |
| [Ages and World Resets](world-engine/AGES_AND_WORLD_RESETS.md) | Historical Age classification, causal transitions, exceptional Resets, and survivorship | Consumed by chronology, Reincarnation, time compression, and persistence. |
| [World Gates and World Contact](world-engine/GATES_AND_WORLD_CONTACT.md) | Contact Domains, World Gates, channels, compatibility, crossing, closure, and legacies | Consumed by world simulation, travel, diplomacy, ecology, and Gate records. |
| [World Gate Soul Interactions](world-engine/WORLD_GATE_SOUL_INTERACTIONS.md) | Bounded Gate effects on Reincarnation reach and Soul Avatar triggers | Consumes established Gate and Soul outcomes without merging their owners. |
| [Simulation Abstraction](world-engine/SIMULATION_ABSTRACTION.md) | Resolution Frames, compression, expansion, off-screen passes, and Review Points | Consumed by GM simulation, Time Skips, and long-horizon persistence. |

## Game Master Toolkit

**Dependencies:** every relevant specialist rules owner, World Engine truth, Campaign Persistence authority, and accepted GM decisions.

**Extensions:** campaign-external preparations, profiles, rulings, records, and session procedures may apply these tools without entering the repository.

**Consumers:** human GMs, AI GMs, facilitators, playtesters, and campaign persistence operators.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [GM Toolkit Index](gm/README.md) | GM reading order, task routing, and campaign-data boundary | Routes a GM need to the narrowest procedure. |
| [GM Principles](gm/GM_PRINCIPLES.md) | Shared adjudication posture, agency, fair uncertainty, causality, and contextual assessment | Required by every GM tool. |
| [GM Responsibilities](gm/GM_RESPONSIBILITIES.md) | Bounded GM duties, delegation, ownership handoffs, and record discipline | Consumed by human and AI operators. |
| [Game Master Framework](gm/GAME_MASTER_FRAMEWORK.md) | Session lifecycle, authority hierarchy, continuity, information model, improvisation, and records | Extended by focused GM procedures and AI operations. |
| [Consequence Resolution](gm/CONSEQUENCE_RESOLUTION.md) | Immediate outcome boundary, cost, affected subjects, response, and persistence handoff | Consumed after actions and world events. |
| [Uncertainty Handling](gm/UNCERTAINTY_HANDLING.md) | Evidence, observer views, method selection, randomization, deferral, secrecy, and correction | Consumed whenever outcomes or information are uncertain. |
| [Reincarnation Generation](gm/REINCARNATION_GENERATION.md) | Procedure for sourcing and presenting world-valid Reincarnation candidates | Consumes Reincarnation and world-state owners. |
| [Encounter Generator](gm/ENCOUNTER_GENERATOR.md) | Causal encounter sourcing, eligibility, framing, routes, and non-scaling | Consumed during preparation and emergent contact. |
| [Monster Generator](gm/MONSTER_GENERATOR.md) | Procedure for creating world-valid individual monster profiles | Consumes Species, ecology, Development, Skills, and information owners. |
| [NPC Generator](gm/NPC_GENERATOR.md) | Procedure for creating autonomous person profiles and observer views | Consumed by world simulation and encounters. |
| [Dungeon Generator](gm/DUNGEON_GENERATOR.md) | Procedure for creating causally valid Dungeon profiles | Consumes Dungeon Activity, locations, ecology, resources, and factions. |
| [Faction Generator](gm/FACTION_GENERATOR.md) | Procedure for creating causally valid faction profiles and Versions | Consumes Faction Behaviour and relevant social owners. |
| [World-Event Generator](gm/WORLD_EVENT_GENERATOR.md) | Procedure for identifying and framing direct causal world events | Consumes Causal Event Chains and specialist owners. |
| [Time Skip Procedure](gm/TIME_SKIP_PROCEDURE.md) | Player-authorized narrative compression, Standing Instructions, Review Points, and return state | Consumes Simulation Abstraction and persistence. |
| [Age Transition Procedure](gm/AGE_TRANSITION_PROCEDURE.md) | Evidence-backed Age classification and transition return to play | Consumes Ages, chronology, and World Reset checkpoints. |
| [Alpha Playtest Rules](gm/ALPHA_PLAYTEST_RULES.md) | Campaign-local provisional ruling status, testing, recording, and review | Extended by external playtest observations; cannot alter Repository Canon automatically. |

## GM Living Codex

**Dependencies:** every referenced specialist rules owner, accepted design governance, GM authority, and the repository-versus-campaign boundary.

**Extensions:** a separately deployed Living Codex database, rendered views, campaign configuration references, and campaign-local divergences may instantiate these reusable designs without entering the repository.

**Consumers:** human and AI GMs, monster and ecosystem generators, player-species creation, Evolution adjudication, Campaign Configuration, and validation tools.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [GM Living Codex Index](gm-living-codex/README.md) | Living Codex reading order, authority boundary, operational scope, and family navigation | Routes reusable-design claims without creating campaign facts. |
| [GM Living Codex Design](gm-living-codex/GM_LIVING_CODEX.md) | Cross-campaign reusable-design authority, GM ownership, consultation, inclusion, divergence, stable identity, safeguards, and implementation order | Constrains all Codex modules and deployments while remaining subordinate to specialist mechanics. |
| [Living Codex Species Registry](gm-living-codex/SPECIES_REGISTRY.md) | Stable species identity, reusable record contracts, capability classification, Evolution graphs, variants, procedural metadata, and player-species integration | Consumes specialist mechanics and supplies reusable designs without storing individuals or live world state. |
| [Living Codex Persistence Model](gm-living-codex/PERSISTENCE_MODEL.md) | Separate Codex SQLite authority, normalized schema, stable references, revisions, migrations, validation, Google Drive deployment, backups, and full-save completion | Reuses adapter procedures while remaining distinct from Campaign Persistence and every populated campaign database. |
| [Reproductive Compatibility](gm-living-codex/REPRODUCTIVE_COMPATIBILITY.md) | Sparse directional species relationships, scoped nonzero probabilities, assistance methods, viability and fertility claims, schema constraints, and operating procedure | Supplies reusable design inputs without defining individuals, complete reproduction, inheritance, Hybrid Forms, or population outcomes. |
| [Lineage, Hybridization, and Evolutionary Inheritance](gm-living-codex/LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md) | Reusable Lineage Templates, Inheritance Profiles, Species and Evolution Expression, Mana equalization, inherited instinct, and stabilization | Consumes successful formation and supplies reusable outcomes without storing genealogy or individual lineage state. |

## Campaign Persistence Engine

**Dependencies:** Repository Canon, World Engine outcomes, GM procedures, every specialist record owner, and accepted persistence decisions.

**Extensions:** storage-specific implementations and populated campaign records may instantiate the logical architecture outside the repository.

**Consumers:** GMs, AI operators, campaign custodians, migration tools, validation tools, templates, and continuity resolution.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [Persistence Index](persistence/README.md) | Persistence reading order, authority boundary, and repository scope | Routes continuity and storage-neutral record claims. |
| [Campaign Persistence Philosophy](persistence/CAMPAIGN_PERSISTENCE_PHILOSOPHY.md) | Persistence as causal memory and its boundary with simulation and operation | Required by every persistence document. |
| [Persistence Authority](persistence/PERSISTENCE_AUTHORITY.md) | Authority hierarchy from Repository Canon through Current Narration | Consumed by every read, write, conflict, migration, and validation procedure. |
| [Structured Persistence Architecture](persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md) | Logical modules, common contracts, stable identity, typed references, and dependency closure | Extended by blank templates and storage-specific implementations. |
| [Canonical Data Ownership](persistence/CANONICAL_DATA_OWNERSHIP.md) | Mutable-fact ownership, persistent non-autonomous Entity identity anchors, normalized domain boundaries, current-placement ownership, data classifications, and the reserved autonomous domain | Consumed by campaign schemas, Save Updates, migrations, validation, templates, and future context or summary systems. |
| [Autonomous Registry](persistence/AUTONOMOUS_REGISTRY.md) | Persistent autonomous identity, Models, Individuals, Groups, autonomy, Controller assignments, reconstruction, memory and upgrade lineage, networks, last-confirmed state, migration, and validation | Consumed by campaign schemas, world simulation, Relationships, Infrastructure, Projects, future context assembly, and validation. |
| [Truth Layers](persistence/TRUTH_LAYERS.md) | Ownership, visibility, update, promotion, and migration of distinct information layers | Consumed by knowledge, research, secrets, narration, and validation. |
| [Persistence Levels](persistence/PERSISTENCE_LEVELS.md) | Repository, Soul, Historical, Campaign, Session, and Ephemeral lifetimes | Consumed by record creation, promotion, deletion, archival, and migration. |
| [Campaign State Model](persistence/CAMPAIGN_STATE_MODEL.md) | Authoritative state graph, claims, provenance, Read Sets, deltas, unknowns, and snapshots | Consumed before adjudication and saving. |
| [Relationship Memory Engine](persistence/RELATIONSHIP_MEMORY_ENGINE.md) | Persistent relationship identity, encounters, dimensions, commitments, and trajectory | Consumed by NPCs, factions, Souls, and social play. |
| [Research Engine](persistence/RESEARCH_ENGINE.md) | Observation-to-confirmation inquiry, confidence, competing theories, disproof, and rediscovery | Consumed by Knowledge views, mysteries, and world discovery. |
| [Timeline Engine](persistence/TIMELINE_ENGINE.md) | Stable event identity and world, campaign, session, personal, and Soul chronologies | Consumed by history, time skips, Ages, migration, and continuity. |
| [Migration and Versioning](persistence/MIGRATION_AND_VERSIONING.md) | Campaign-version transformation through Backup, Audit, Merge, Validation, and activation | Consumed by custodians and storage implementations. |
| [Continuity Resolution](persistence/CONTINUITY_RESOLUTION.md) | Conflict containment, classification, authority resolution, correction, and resumption | Consumed when narration and persistence disagree. |
| [Save Update Protocol](persistence/SAVE_UPDATE_PROTOCOL.md) | Affected Set, Session Delta, owner-routed Write Set, validation, and atomic Save Point | Consumed after completed Gameplay Interactions. |
| [Persistence Validation](persistence/PERSISTENCE_VALIDATION.md) | Read-only baselines, profiles, findings, severity, outcomes, and repair routing | Consumed by saves, loads, migrations, corrections, and audits. |
| [Persistence Integration](persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md) | Completed-system ownership map and load-to-activation operating cycle | Consumed by GM, AI, template, and implementation workflows. |

## AI Game Master Operations

**Dependencies:** Game Master Toolkit, Campaign Persistence Engine, Repository Canon, information permissions, and relevant specialist owners.

**Extensions:** implementation-specific adapters may execute these procedures but may not change their authority or claim broader capability than available.

**Consumers:** implementation-neutral AI GMs, supervisors, campaign custodians, and human handoff operators.

| Document | Owner | Interface refinement |
| --- | --- | --- |
| [AI Operations Index](ai/README.md) | AI procedure reading order, scope, parity, and authority boundary | Routes operators through the complete cycle. |
| [AI Runtime Model](ai/AI_RUNTIME_MODEL.md) | Runtime layers, authority boundaries, boot and action flows, adapter composition, and extension rules | Separates rules, persistence semantics, execution profiles, adapters, configuration, and campaign state. |
| [AI Capabilities and Limitations](ai/AI_CAPABILITIES_AND_LIMITATIONS.md) | Cross-runtime capability disclosure, limitation handling, and failure safeguards | Prevents memory, fluency, cache, tool access, or generated completeness from becoming authority. |
| [ChatGPT GM Universal Instructions](ai/chatgpt/CHATGPT_GM_UNIVERSAL_INSTRUCTIONS.md) | ChatGPT-specific context, boot, action, delivery, persistence, correction, and failure behavior | Applies strict Save-Before-Delivery without changing fictional rules or shared GM authority. |
| [SQLite Persistence Adapter](ai/chatgpt/adapters/SQLITE_PERSISTENCE_ADAPTER.md) | SQLite transaction, integrity, expected-state, read-only reopen, staleness, rollback, and candidate production | Implements a logical structured store without adjudicating or owning campaign meaning. |
| [Google Drive Persistence Adapter](ai/chatgpt/adapters/GOOGLE_DRIVE_PERSISTENCE_ADAPTER.md) | Remote identity, fetch, replacement, read-back, backup, concurrency, folder hygiene, and deployment recovery | Deploys and verifies canonical bytes without adjudicating or owning campaign meaning. |
| [AI GM Workflow](ai/AI_GM_WORKFLOW.md) | End-to-end load, adjudicate, narrate, persist, recover, and handoff sequence | Consumed as the main AI operating loop. |
| [AI Session Start](ai/AI_SESSION_START.md) | Version, Save Index, visibility, Read Set, freshness, readiness, and resume checks | Required before AI-facilitated play. |
| [AI Play Protocol](ai/AI_PLAY_PROTOCOL.md) | Intent classification, owner retrieval, information separation, resolution, narration, and closure | Consumed for each Gameplay Interaction. |
| [AI Save Protocol](ai/AI_SAVE_PROTOCOL.md) | Writer-mode disclosure and faithful Save Update Protocol execution | Consumed after material interactions when persistence is available. |
| [AI Checklist](ai/AI_CHECKLIST.md) | Compact verification gates for start, play, save, correction, continuation, and handoff | Consumed as an operational guard, not an authority substitute. |

## Interface Resolution

When two documents appear to own the same claim:

1. identify the exact claim rather than comparing whole scenes or records;
2. use the narrowest specialist owner listed here and in the relevant family index;
3. treat other documents as dependencies, extensions, or consumers for that claim;
4. inspect accepted design governance;
5. preserve explicit unknowns when available evidence cannot establish the owner or outcome;
6. resolve any true canonical conflict in both governance and rules before continuing.

This sequence routes claims; it does not decide campaign outcomes or permit one family to overwrite another.

## Repository Boundary

This registry contains no campaign state. Populated characters, Souls, species, Skills, Soul Weapons, magical instances, locations, factions, encounters, world conditions, histories, secrets, and saves remain outside the canonical repository.

## Related Documents

- [Canonical Rules Map](README.md)
- [Repository Conventions](../design/REPOSITORY_CONVENTIONS.md)
- [Design Decisions](../design/DECISIONS.md)
- [Terminology](../design/TERMINOLOGY.md)
- [Template Coverage Map](../templates/TEMPLATE_COVERAGE.md)
- [Cross-Reference and Ownership Audit](../design/audits/CROSS_REFERENCE_AND_OWNERSHIP_AUDIT.md)
