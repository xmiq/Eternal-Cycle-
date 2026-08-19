# Future Revisions

This register records evidence-driven issues considered during Phase 12 — Gameplay Validation & Maintenance and later development. It is the permanent rolling final Phase 12 objective, but it remains non-canonical design tracking: an entry does not change a playable rule, authorize implementation, reopen a completed phase, or establish a roadmap objective.

## Authority and Ownership

- **Owner:** the project owner governs admission, priority, promotion, closure, and removal of entries.
- **Dependencies:** [Design Decisions](DECISIONS.md), [Developer Notes](DEVELOPER_NOTES.md), [Unresolved Questions](UNRESOLVED_QUESTIONS.md), and the canonical documents named by each entry.
- **Extensions:** evidence reports, approved future roadmaps, and focused design changes may extend an entry without making the register canonical rules text.
- **Consumers:** project maintainers, playtest reviewers, rules auditors, and future roadmap authors.

[D-1174](DECISIONS.md#d-1174--future-revisions-require-evidence-before-canon) governs this register. Files under `docs/` and accepted decisions remain authoritative within their respective responsibilities.

## Register Boundary

Use this file for a suspected usability, balance, coherence, or maintainability issue that needs gameplay evidence before design work is authorized.

Gameplay does not write to this register directly. A gameplay GM may surface an observation or preserve campaign-local evidence, but only the project owner may mediate that material into a Future Revision entry, change its priority or status, merge or remove it, or authorize promotion into roadmap work. Development agents must not infer authorization from gameplay text alone.

Use [Unresolved Questions](UNRESOLVED_QUESTIONS.md) when an unanswered question blocks or materially shapes current roadmap work. Use [Developer Notes](DEVELOPER_NOTES.md) for exploratory alternatives, observations, and workshop material that have not qualified for this register. Use [Phase 12 in the Roadmap](ROADMAP.md#phase-12--gameplay-validation--maintenance) only after the project owner authorizes implementation. An approved objective is inserted immediately before Future Revisions; its `FR-###` identifier remains stable provenance and never becomes a phase number.

An entry must not:

- prescribe a rule change as though accepted;
- treat preference, speculation, or one unusual outcome as proof of a defect;
- contain campaign identities, saves, transcripts, current state, or other playthrough records;
- copy protected GM information or participant-private material into the repository;
- resolve an open question, alter priority, or reopen a phase without owner authorization.

Playtest evidence remains outside this repository. Register entries cite anonymized evidence identifiers or describe the evidence still required.

## Entry Lifecycle

1. **Candidate** - a bounded concern is recorded with affected systems, plausible gameplay impact, and evidence needs.
2. **Gathering Evidence** - playtests or audits are intentionally observing the concern under more than one relevant context where practical.
3. **Ready for Review** - evidence is sufficient for the project owner to decide whether design work is warranted.
4. **Roadmapped** - the owner has authorized a future roadmap task; the register links to that task without implementing it here.
5. **Closed** - evidence did not support revision, another change resolved the concern, or an authorized revision was completed and validated.

Status changes require a short reason and evidence reference. Moving an entry to **Roadmapped** requires explicit project-owner approval. Closing an implemented entry requires links to the accepted decision, changed rules, and validation result.

## Priority

Priority describes the cost of leaving an evidence-supported issue unresolved; it does not prove that the issue exists.

- **Critical** - may break authority, continuity, agency, safety, or basic playability across ordinary use.
- **High** - may repeatedly distort meaningful choices, invalidate a major path, or impose severe operating burden.
- **Medium** - may create recurring confusion, uneven incentives, or avoidable friction without preventing play.
- **Low** - primarily affects clarity, convenience, edge cases, or optional refinement.

## Evidence Standard

Evidence should identify the rule version, relevant canonical systems, test conditions, expected behavior, observed behavior, frequency, consequences, and plausible confounders. Prefer repeated observations across different bodies, species, contexts, or GMs where the issue claims broad effect.

Anecdotes may create a Candidate. They do not by themselves authorize a revision. Missing evidence stays explicitly missing, and contradictory evidence is preserved rather than averaged away.

## Open Register

### FR-016 - Soul-Bound Companion Fate & Reincarnation Continuity

- **Status:** Roadmapped
- **Issue:** A companion may be Soul-bound without canonical rules distinguishing persistent fate, eventual reunion, current-incarnation identity, recognition, memory, and current Relationship state across asynchronous deaths and radically different lives.
- **Affected systems:** Souls, Reincarnation, Final Death, Interlife, Soul Resonance, companions, Relationships, Campaign Persistence, Timeline, Campaign History, FR-002, FR-004, FR-009, FR-012, and FR-015.
- **Gameplay impact:** Without a bounded model, a bond may be forgotten across lives, mistaken for mind control or inherited romance, used as an unrestricted destination selector, duplicated per incarnation, or burden the GM with a prewritten destiny route.
- **Evidence needed:** Cross-life companion cases involving asynchronous lifespans, delayed reunion, different worlds or species, uncertain recognition, changed social roles, conflict, multiple independent bonds, and routes that remain unresolved until later play.
- **Approved direction:** Soul binding establishes a persistent fate relation between distinct Souls. Fate guarantees eventual reunion, but the GM controls timing, route, circumstances, and Reincarnation adjudication under existing Canon. Reunion may be delayed and does not guarantee recognition or autobiographical memory. Current incarnations remain distinct people; current Relationships are not inherited and no affection, obedience, trust, loyalty, attraction, or agreement is forced. The meaningful long-term arc trends net positive without requiring comfort or conflict-free lives. The base bond grants no automatic telepathy, tracking, shared death, resurrection, Soul Space transfer, communication, or other unrelated power. Each bond is independent; multiple bonds do not imply bonds among every participant. Persistence places bond identity at the Soul/fate level, current Relationship state with Relationships, incarnation identity with Entities, manifestations with the future Life Archive, and significant events with Timeline and Campaign History. An unknown reunion route remains unresolved rather than fabricated.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Explicitly promoted by the project maintainer for later Phase 12 implementation; this planning update creates no mechanics, schema, migration, or campaign state.
- **Authorized roadmap link:** [FR-016 — Soul-Bound Companion Fate & Reincarnation Continuity](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Architecture dependencies:** FR-002 candidate adjudication, FR-004 Life Archive indexing, FR-009 world-access discretion, FR-012 canonical ownership, and FR-015 memory continuity.

## Roadmapped

FR-016 is the only roadmapped pending Phase 12 objective. FR-001 through FR-012, FR-014, and FR-015 are closed below where present. Promotion authorizes future planning and implementation work only and does not select an execution order.

All currently known FR-001 through FR-016 entries are explicitly Roadmapped or Closed. A detailed Knowledge System beyond FR-015's approved memory scope remains unpromoted, and new gameplay findings still enter this register through maintainer mediation.

## Closed

### FR-015 - Memory Continuity, Fading, and Recall

- **Status:** Closed
- **Issue:** Define natural autobiographical memory continuity across reincarnations without treating structured history, player access, or GM context as perfect character recall.
- **Affected systems:** Souls, Reincarnation, Life Archive, Character Knowledge, Context Assembly, Relationships, Timeline, retained development, Skills, and persistence validation.
- **Gameplay impact:** Without an owner and procedure, reincarnation could alternate arbitrarily between perfect memory and total amnesia, leak player knowledge, or turn Skill familiarity into recollection.
- **Evidence needed:** Cross-Life cases involving meaningful fading, sensory and relational cues, fragmentary or conflicted recall, radical embodiment changes, Life Archive access, retained Skill familiarity, and current-life identity.
- **Approved direction:** Memory persistence depends on significance and reinforcement, may fade within and across Lives, and may return through relevant cues. Player-visible Life Archive access remains separate from Character Knowledge. Skill familiarity and retained-development acceleration remain distinct under FR-001 and FR-005.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Implemented through the canonical Memory Continuity contract, blank template, ownership map, recall procedure, migration and validation guidance, system integrations, terminology, decisions, navigation, and structural regression validation.
- **Still unpromoted:** a detailed per-Entity belief, confidence, evidence, testimony, misinformation propagation, contradiction-resolution, information-sharing, or inference engine.
- **Authorized roadmap link:** [FR-015 — Memory Continuity, Fading, and Recall](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Memory Continuity, Fading, and Recall](../docs/soul/MEMORY_CONTINUITY.md), [Memory Continuity Record Template](../templates/MEMORY_CONTINUITY_TEMPLATE.md), and [FR-015 Implementation Audit](audits/FR_015_MEMORY_CONTINUITY_AUDIT.md)

### FR-011 - GM/AI Context Assembly, Mandatory Read Discipline, and Gameplay Turn Persistence

- **Status:** Closed
- **Issue:** Canonical reads generally worked, but automatic saving was unreliable and conversational continuity could be mistaken for persisted state.
- **Affected systems:** GM Toolkit, AI Runtime, Campaign Persistence Engine, Save Update Protocol, adapters, templates, validation, and navigation.
- **Gameplay impact:** Unsaved narration could appear durable until a later reload lost it, forcing manual save commands and breaking continuity.
- **Evidence needed:** Runtime cases covering Skill reads, automatic character and Relationship saves, multi-domain writes, no-change turns, failed writes, contradictory conversation, next-turn reload, stale caches, deep history, and context reset.
- **Approved direction:** Preserve owner-routed Read Sets, assemble compact relevance-filtered Context Packets from authoritative persistence, determine every Affected Set explicitly, persist non-empty sets automatically, validate and read back before turn closure, and rebuild later context from committed state.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Implemented through the canonical turn-state and Context Assembly contract, blank packet template, AI and persistence integrations, runtime-host acceptance boundary, migration guidance, terminology, decisions, and structural regression validation.
- **Authorized roadmap link:** [FR-011 — GM/AI Context Assembly, Mandatory Read Discipline, and Gameplay Turn Persistence](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Context Assembly and Gameplay Turn Persistence](../docs/ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md), [Context Packet Template](../templates/CONTEXT_PACKET_TEMPLATE.md), and [FR-011 Implementation Audit](audits/FR_011_CONTEXT_AND_PERSISTENCE_AUDIT.md)

### FR-010 - Long-Horizon Simulation Summaries

- **Status:** Closed
- **Issue:** Regional and epochal simulation risked excessive bookkeeping or compression that erased causality, agency, uncertainty, and material exceptions.
- **Affected systems:** Simulation Abstraction, Time Skips, Age transitions, World Engine, Campaign Persistence Engine, Life Archive, Reincarnation, Autonomous Registry, and GM/AI procedures.
- **Gameplay impact:** Long campaigns could become impractical, lose prior-Life consequences, or produce unsupported historical completion.
- **Evidence needed:** Multi-year through Age-scale cases covering peaceful intervals, causal conflict, Reincarnation gaps, sparse sources, autonomous uncertainty, long-lived Entities, World Resets, and protected player presentation.
- **Approved direction:** Use stable, scoped, queryable Long-Horizon Summaries derived from established owner records; preserve material causal chains, unknowns, continuity hooks, provenance, and drill-down references without duplicating current state or Life Summaries.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Implemented and validated through the canonical period-summary contract, blank template, logical SQLite guidance, Time Skip and Reincarnation integration, migration and validation rules, terminology, decisions, navigation, and structural validation.
- **Authorized roadmap link:** [FR-010 — Long-Horizon Simulation Summaries](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Long-Horizon Simulation Summaries](../docs/persistence/LONG_HORIZON_SUMMARIES.md), [Long-Horizon Summary Template](../templates/LONG_HORIZON_SUMMARY_TEMPLATE.md), and [FR-010 Implementation Audit](audits/FR_010_LONG_HORIZON_SUMMARIES_AUDIT.md)

### FR-002 - Reincarnation Candidate Selection

- **Status:** Closed
- **Issue:** Repeated species choices and Final Death circumstances needed bounded influence without becoming deterministic or farmable.
- **Affected systems:** Reincarnation, candidate generation, Final Death, Soul identity, and GM procedures.
- **Gameplay impact:** Unclear influence weakened continuity; excessive influence could reward engineered death or collapse meaningful choice.
- **Evidence needed:** Candidate outcomes across repeated species families, varied deaths, player wishes, exploitation attempts, and world constraints.
- **Approved direction:** Use contextual GM adjudication within existing eligibility, embodiment, world-state, Soul-compatibility, and exceptional-unlock constraints; contextual inputs and player wishes inform but never command or guarantee selection.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** Medium
- **Status reason:** Implemented and validated through canonical Reincarnation and GM candidate-adjudication rules, anti-farming safeguards, terminology, and structural validation.
- **Authorized roadmap link:** [FR-002 — Reincarnation Candidate Selection](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Reincarnation](../docs/soul/REINCARNATION.md), [Reincarnation Generation](../docs/gm/REINCARNATION_GENERATION.md), and [FR-002/FR-003/FR-008/FR-009 Audit](audits/FR_002_FR_003_FR_008_FR_009_CLARIFICATION_AUDIT.md)

### FR-003 - Soul Depth Information Visibility

- **Status:** Closed
- **Issue:** Depth Horizons needed enough visibility for informed play without becoming an omniscient progression ladder.
- **Affected systems:** Soul Depth, Soul Resonance, information views, uncertainty handling, and campaign presentation.
- **Gameplay impact:** Poor disclosure could make access arbitrary or reduce qualitative growth to a visible meter.
- **Evidence needed:** Direct, in-world, and tone-dependent disclosure cases measuring comprehension, mystery, planning, and metagaming pressure.
- **Approved direction:** Established properties may be explicit; unknown deeper structure remains hidden or appears through valid signs and revealing mechanics, with exact information requiring justified access.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** Medium
- **Status reason:** Implemented and validated through hybrid Soul Depth visibility, qualitative safeguards, knowledge separation, examples, terminology, and structural validation.
- **Authorized roadmap link:** [FR-003 — Soul Depth Information Visibility](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Soul Depth](../docs/soul/SOUL_DEPTH.md) and [FR-002/FR-003/FR-008/FR-009 Audit](audits/FR_002_FR_003_FR_008_FR_009_CLARIFICATION_AUDIT.md)

### FR-008 - Soul Weapon Rarity and Equipment Relevance

- **Status:** Closed
- **Issue:** Exceptional Soul Weapons risked making ordinary equipment, crafting, logistics, and non-partner paths appear irrelevant.
- **Affected systems:** Soul Weapons, equipment, Professions, Skills, embodiment, Magic, and resource logistics.
- **Gameplay impact:** An optional relationship could become functionally mandatory or narrow long-term equipment choices.
- **Evidence needed:** Campaigns without Soul Weapons, inaccessible or unsuitable partners, unconventional partners, and conventional-equipment specialists.
- **Approved direction:** Developed Soul Weapons may substantially exceed conventional equipment; rarity, individual binding, limited transfer, actual growth, and social logistics preserve the ordinary baseline without artificial parity.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** Medium
- **Status reason:** Implemented and validated through Weapon Evolution capability and equipment-baseline boundaries, index integration, and structural validation.
- **Authorized roadmap link:** [FR-008 — Soul Weapon Rarity and Equipment Relevance](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Weapon Evolution](../docs/soul-weapons/WEAPON_EVOLUTION.md) and [FR-002/FR-003/FR-008/FR-009 Audit](audits/FR_002_FR_003_FR_008_FR_009_CLARIFICATION_AUDIT.md)

### FR-009 - World Contact, Travel, and Reincarnation Discretion

- **Status:** Closed
- **Issue:** Contact and cross-domain reach needed clear separation from travel and selectable Reincarnation destinations.
- **Affected systems:** World Gates, world-contact events, Reincarnation, Soul Avatars, information views, and GM procedures.
- **Gameplay impact:** Ambiguity could turn contact into excessive bookkeeping or an unrestricted destination menu.
- **Evidence needed:** Asymmetric contacts, closures, stranded actors, incompatible routes, and included or excluded candidate domains.
- **Approved direction:** World knowledge, contact, travel routes, and Reincarnation possibility are independent claims; prior contact and player wishes inform contextual adjudication without guaranteeing current access or selection.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** Medium
- **Status reason:** Implemented and validated through World Gate distinctions, cross-domain Reincarnation guidance, terminology, and structural validation.
- **Authorized roadmap link:** [FR-009 — World Contact, Travel, and Reincarnation Discretion](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [World Gates and World-Contact Events](../docs/world-engine/GATES_AND_WORLD_CONTACT.md), [World Gate Soul Interactions](../docs/world-engine/WORLD_GATE_SOUL_INTERACTIONS.md), and [FR-002/FR-003/FR-008/FR-009 Audit](audits/FR_002_FR_003_FR_008_FR_009_CLARIFICATION_AUDIT.md)

### FR-006 - Adaptive Skill Consolidation and Merge Rules

- **Status:** Closed
- **Issue:** Adaptive Skill adjudication may create many narrow permanent Skills or incentives to seek harmful repetition as an optimal formation route.
- **Affected systems:** Adaptive Skills, Capability Representation, Development, Skill lineage, persistence, and safeguards.
- **Gameplay impact:** Unchecked proliferation obscures meaningful capability and can double-count Development.
- **Evidence needed:** Formation and consolidation cases across training, experimentation, repeated exposure, distinct mechanisms, ambiguous overlap, and harmful repetition.
- **Approved direction:** Allow justified emergence, then routinely consolidate substantially redundant records through capability evidence, stable lineage, overlap-safe Development reconciliation, and GM review while preserving meaningful distinctions.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Implemented and validated through the consolidation contract, Skill template extension, persistence and migration integration, canonical cases, decisions, terminology, and structural validation.
- **Authorized roadmap link:** [FR-006 — Adaptive Skill Consolidation and Merge Rules](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Skill Consolidation and Historical Scope](../docs/skills/SKILL_CONSOLIDATION_AND_SCOPE.md), [Skill Template](../templates/SKILL_TEMPLATE.md), and [FR-006/FR-007 Audit](audits/FR_006_FR_007_SKILL_CONSOLIDATION_AUDIT.md)

### FR-007 - Conceptual Skill Scope and Historical Interpretation

- **Status:** Closed
- **Issue:** Broad and Conceptual Skill labels may be interpreted as unrestricted capability or made too opaque for consistent use.
- **Affected systems:** Conceptual Skills, Skill Evolution, consolidation, Development, retained provenance, GM adjudication, and player views.
- **Gameplay impact:** Name-based powers can eclipse earned expertise, while unclear scope can make legitimate mastery unusable.
- **Evidence needed:** Broad and Conceptual Skill claims with acquisition history, successful and failed applications, lineage, enabling mechanics, counters, and rejected linguistic interpretations.
- **Approved direction:** Interpret capability through acquisition, Development, applications, failures, lineage, Evolution, embodiment expressions, cross-Life translation, and current enabling mechanics using four history-based application categories.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** Medium
- **Status reason:** Implemented and validated through history-based scope rules, earned-extension guidance, Conceptual Skill integration, canonical cases, persistence, decisions, terminology, and structural validation.
- **Authorized roadmap link:** [FR-007 — Conceptual Skill Scope and Historical Interpretation](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Skill Consolidation and Historical Scope](../docs/skills/SKILL_CONSOLIDATION_AND_SCOPE.md), [Conceptual Skills](../docs/skills/CONCEPTUAL_SKILLS.md), and [FR-006/FR-007 Audit](audits/FR_006_FR_007_SKILL_CONSOLIDATION_AUDIT.md)

### FR-001 - Retained Development and Embodiment Relevance

- **Status:** Closed
- **Issue:** Define bounded retained-development acceleration from qualifying previous lives without making current embodiment or current-life effort irrelevant.
- **Affected systems:** Development System, retained Stat XP, Reincarnation, embodiment, training, Skills, Life Archive, and capability assessment.
- **Gameplay impact:** Poor calibration could make reincarnation feel unrewarding or turn later lives into automatic rebuilds.
- **Evidence needed:** Comparative records of early-, middle-, and old-soul redevelopment across suitable and unsuitable bodies, including practice time, plateaus, access limits, and contextual effectiveness.
- **Approved direction:** Each meaningful previous Life may contribute while rebuilding a capability within its demonstrated territory. Embodiment Relevance modifies contribution per capability, and bounded additive saturation prevents runaway accumulation without making old-Soul history cosmetic.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Implemented and validated through the retained cross-Life contract, bounded stacking model, persistence guidance, template, integrations, decisions, and structural validation.
- **Authorized roadmap link:** [FR-001 — Retained Development and Embodiment Relevance](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Retained Cross-Life Development](../docs/progression/RETAINED_CROSS_LIFE_DEVELOPMENT.md), [Retained Development Template](../templates/RETAINED_CROSS_LIFE_DEVELOPMENT_TEMPLATE.md), and [FR-001/FR-005 Audit](audits/FR_001_FR_005_RETAINED_DEVELOPMENT_AUDIT.md)

### FR-005 - Cross-Embodiment Skill Transfer

- **Status:** Closed
- **Issue:** Reincarnation crossover and Direct Transfer may be misapplied in ways that erase human-versus-monster receiving routes or appear to grant instant mastery.
- **Affected systems:** Skill crossover, human Skills, monster Skills, Retained Instincts, species development, embodiment, Reincarnation, and Life Archive.
- **Gameplay impact:** Miscalibration could make species and present bodies cosmetic or make retained history unusably constrained.
- **Evidence needed:** Cross-species recovery cases documenting receiving routes, translation costs, practice, failed expressions, and preservation of tree distinctions.
- **Approved direction:** Separate Familiarity Unlock from embodiment-dependent Development Boost, preserve stable Skill identity and expression lineage, retain hard prerequisites, and permit developed translated expressions to become later-Life bridges.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Implemented and validated with zero-boost familiarity, translation chains, Mana limits, Level 0 boundaries, provenance, persistence, and worked cases.
- **Authorized roadmap link:** [FR-005 — Cross-Embodiment Skill Transfer](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Retained Cross-Life Development](../docs/progression/RETAINED_CROSS_LIFE_DEVELOPMENT.md), [Reincarnation Skill Crossover](../docs/skills/REINCARNATION_SKILL_CROSSOVER.md), and [FR-001/FR-005 Audit](audits/FR_001_FR_005_RETAINED_DEVELOPMENT_AUDIT.md)

### FR-004 - Life Archive and Old-Soul Indexing

- **Status:** Closed
- **Issue:** Accumulated Soul systems, histories, retained routes, and cross-life relationships may create excessive choice and record-management burden in very old campaigns.
- **Affected systems:** Soul Engine, Campaign Persistence Engine, Reincarnation generation, templates, GM Toolkit, Development, Skills, Relationships, Timeline, and Campaign History.
- **Gameplay impact:** Important identity and continuity may become difficult to retrieve, while minor options crowd out present-life priorities.
- **Evidence needed:** Long-horizon campaign tests or representative stress tests measuring preparation time, retrieval failures, option overload, and which summaries preserve meaningful distinctions.
- **Approved direction:** Create a structured Life Archive with a player-visible Life Summary for each completed incarnation and a `Soul Overview -> Life Summary -> Full Life Detail` retrieval hierarchy. Summaries index major peaks, relationships, discoveries, projects, failures, consequences, and retained-development provenance without implying current-character recall or duplicating every detail.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Implemented and validated through the canonical Life Archive contract, normalized logical SQLite guidance, blank template, persistence integrations, migration and validation procedures, terminology, decisions, and structural repository validation. No populated campaign was migrated.
- **Authorized roadmap link:** [FR-004 — Life Archive and Old-Soul Indexing](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Life Archive and Old-Soul Indexing](../docs/persistence/LIFE_ARCHIVE.md), [Life Archive Template](../templates/LIFE_ARCHIVE_TEMPLATE.md), and [FR-004 Implementation Audit](audits/FR_004_LIFE_ARCHIVE_AUDIT.md)

### FR-014 - Autonomous Registry

- **Status:** Closed
- **Issue:** Independently operating campaign entities and systems could be scattered across Infrastructure, Inventory, Relationships, world state, and ad hoc records, obscuring identity, assignment, memory continuity, networks, maintenance, and autonomous action.
- **Affected systems:** Campaign Persistence Engine, GM Toolkit, World Engine, AI Save Protocol, SQLite persistence, Relationships, Infrastructure, summons, familiars, undead, constructs, remote bodies, and autonomous Infrastructure.
- **Gameplay impact:** Autonomous units could be forgotten, duplicated, treated as Models rather than identities, lose assignments or network relations, or be reconstructed without distinguishing restoration from replacement.
- **Evidence needed:** Owner-mediated gameplay evidence involving autonomous identities, Groups, Models, memory continuity, condition, assignments, networks, maintenance, upgrades, or personhood state.
- **Approved direction:** Define a separate registry preserving stable identity, Model-versus-Individual distinction, Controller, autonomy, Groups, references to condition and location, assignments, capabilities, networks, requirements, creation and upgrade lineage, memory continuity, personhood claims, confirmed reporting, and provenance.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Implemented and validated through the canonical Registry contract, blank template, persistence Read and Affected Set integration, migration-audit procedure, terminology, decisions, and structural validation. No campaign entities were migrated.
- **Authorized roadmap link:** [FR-014 — Autonomous Registry](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Autonomous Registry](../docs/persistence/AUTONOMOUS_REGISTRY.md), [Autonomous Registry Template](../templates/AUTONOMOUS_REGISTRY_TEMPLATE.md), and [FR-014 Implementation Audit](audits/FR_014_AUTONOMOUS_REGISTRY_AUDIT.md)
- **Architecture dependency:** [Simulation Architecture and Perspective Model](../docs/core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md)

### FR-012 - Canonical SQL Ownership and Anti-Duplication

- **Status:** Closed
- **Issue:** Broad template coverage could cause overlapping records, unclear write ownership, or excessive maintenance when instantiated in real campaigns.
- **Affected systems:** Templates, Structured Persistence Architecture, Save Update Protocol, validation, and campaign operations.
- **Gameplay impact:** Users could duplicate facts, update the wrong record, or omit persistence because ownership was unclear.
- **Evidence needed:** Storage-neutral campaign implementations using the full and reduced template sets, tracking duplicate claims, broken references, update time, unused fields, and validation findings.
- **Approved direction:** Give every mutable canonical fact one authoritative logical owner. One Entity identity anchor represents each persistent player-relevant non-autonomous being; normalized domain owners hold related facts; summaries and context packets only reference, derive, cache, index, or preserve history. FR-014 owns persistent autonomous entities, with identity-preserving migration required across that boundary.
- **Suggested future phase:** Phase 12 — Gameplay Validation & Maintenance.
- **Priority:** High
- **Status reason:** Implemented and validated through the canonical ownership map, persistence integrations, focused template clarifications, and structural repository validation.
- **Authorized roadmap link:** [FR-012 — Canonical SQL Ownership and Anti-Duplication](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Canonical Data Ownership](../docs/persistence/CANONICAL_DATA_OWNERSHIP.md) and [FR-012 Implementation Audit](audits/FR_012_CANONICAL_SQL_OWNERSHIP_AUDIT.md)

### FR-013 - Lineage and Evolutionary Inheritance

- **Status:** Closed
- **Issue:** Reproductive Compatibility established a bounded formation scaffold without reusable rules for resulting lineage and inherited Evolution expression.
- **Affected systems:** GM Living Codex, Species Registry, Reproductive Compatibility, Species Development, Monster Evolution, Hybridization, Skills, Magic, Souls, World Engine populations, and Campaign Persistence.
- **Gameplay impact:** Ad hoc rulings risked inherited mastery, strongest-trait copying, blurred species identity, and inconsistent descendant architecture.
- **Evidence needed:** Gameplay cases involving mixed lineage, throwbacks, Mana equalization, inherited Evolution expression, Born-Evolved outcomes, Level 0 Instincts, Ancestral Echoes, and inheritance distributions.
- **Suggested future phase:** Lineage, inheritance, and descendant-expression design.
- **Priority:** High
- **Status reason:** Implemented and validated as GM Living Codex Step 13.
- **Authorized roadmap link:** [Phase 12 — Gameplay Validation & Maintenance](ROADMAP.md#phase-12--gameplay-validation--maintenance)
- **Closure references:** [Lineage and Evolutionary Inheritance](../docs/gm-living-codex/LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md), [Level 0 Instinct](../docs/skills/LEVEL_ZERO_INSTINCT.md), and [Living Codex Persistence Model](../docs/gm-living-codex/PERSISTENCE_MODEL.md)

## Blank Entry Contract

```markdown
### FR-000 - Concise Issue Name

- **Status:** Candidate | Gathering Evidence | Ready for Review | Roadmapped | Closed
- **Issue:** State the suspected problem without prescribing a solution.
- **Affected systems:** Link or name every likely canonical owner.
- **Gameplay impact:** Describe what meaningful play, continuity, agency, usability, or balance may be affected.
- **Evidence needed:** Define observations that could support or challenge the concern.
- **Evidence references:** Use anonymized external identifiers; do not add campaign data here.
- **Suggested future phase:** Name a bounded review area, not an assumed solution.
- **Priority:** Critical | High | Medium | Low
- **Status reason:** Required whenever the status changes.
- **Authorized roadmap link:** Required only after project-owner approval.
- **Closure references:** Required for a closed implemented revision.
```

## Related Documents

- [Development Roadmap](ROADMAP.md)
- [Design Decisions](DECISIONS.md)
- [Developer Notes](DEVELOPER_NOTES.md)
- [Unresolved Questions](UNRESOLVED_QUESTIONS.md)
- [Repository Conventions](REPOSITORY_CONVENTIONS.md)
- [Alpha Playtest Rules](../docs/gm/ALPHA_PLAYTEST_RULES.md)
- [Game Master Framework](../docs/gm/GAME_MASTER_FRAMEWORK.md)
- [Campaign Persistence Engine](../docs/persistence/README.md)
