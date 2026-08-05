# Rule Consistency Audit

## Scope

This Phase 11 audit compares the completed canonical rules under `docs/` against accepted design governance, canonical terminology, roadmap completion state, repository boundaries, and the cross-system ownership map. It corrects documentation drift only; it does not rebalance, reinterpret, or expand gameplay mechanics.

## Method

1. review every canonical family and its index through the [Canonical Document Registry](../../docs/DOCUMENT_REGISTRY.md);
2. search canonical rules and templates for future-tense references to completed phases, placeholders, incomplete-system claims, and stale Phase 11 boundaries;
3. test system-wide invariants against their specialist owners;
4. inspect persistence authority, Truth Layers, and repository-versus-campaign separation;
5. inspect claims that could imply universal levels, currencies, automatic progression, or duplicated ownership;
6. replace roadmap-time promises with stable document links where the owning system now exists;
7. validate all changed links and rerun the drift searches.

## Invariant Matrix

| Invariant | Primary owner | Cross-system result |
| --- | --- | --- |
| No universal character level, power score, or shared progression currency | [Development System](../../docs/progression/DEVELOPMENT_SYSTEM.md) | Pass. Local ranks and measures remain scoped; all repository-wide matches are prohibitions or contextual examples. |
| Present embodiment and current-life effort remain necessary | [Development System](../../docs/progression/DEVELOPMENT_SYSTEM.md), [Reincarnation](../../docs/soul/REINCARNATION.md) | Pass. Retained history affects potential and relearning without restoring current statistics, mature anatomy, access, or reliability. |
| Stat XP persists without preserving bodily statistics or compounding itself | [Stat XP and Retained Development](../../docs/progression/STAT_XP_AND_RETAINED_DEVELOPMENT.md) | Pass. Soul, Skill, Evolution, Gate, and persistence consumers route back to the same bounded owner. |
| Human and monster Skill routes remain distinct with bounded crossover | [Human Skill Trees](../../docs/skills/HUMAN_SKILL_TREES.md), [Monster Skill Trees](../../docs/skills/MONSTER_SKILL_TREES.md), [Reincarnation Skill Crossover](../../docs/skills/REINCARNATION_SKILL_CROSSOVER.md) | Pass. Monster Evolution owns bodily routes; human frameworks own social structures; no crossover text grants automatic mastery. |
| Monster Evolution remains ecological, embodied, branching, and non-automatic | [Monster Evolution](../../docs/monster-evolution/MONSTER_EVOLUTION.md), [Evolution Safeguards](../../docs/monster-evolution/EVOLUTION_SAFEGUARDS.md) | Pass. Pressure, kills, consumption, suffering, time, and retained history do not function as universal Evolution currency. |
| Human Classes and Professions organize capability without granting it automatically | [Human Class Philosophy](../../docs/human/HUMAN_CLASS_PHILOSOPHY.md), [Limits of Human Progression](../../docs/human/LIMITS_OF_HUMAN_PROGRESSION.md) | Pass. Development, Skills, recognition, authority, and membership remain separate. |
| Soul Weapons remain optional persons and partners rather than equipment tiers | [Soul Weapon Foundations](../../docs/soul/SOUL_WEAPON_FOUNDATIONS.md), [Soul Weapon Index](../../docs/soul-weapons/README.md) | Pass. No mandatory bond, automatic Awakening, universal compatibility, inherited mastery, or independent source laundering was found. |
| Magic remains source-, embodiment-, Skill-, and world-dependent | [Mana](../../docs/magic/MANA.md), [Magic Index](../../docs/magic/README.md) | Pass. No generic MP, universal spell list, spell slots by default, unlimited Mana, or automatic magical superiority was introduced. |
| World simulation remains autonomous, causal, persistent, and campaign-external | [World Engine Overview](../../docs/world-engine/WORLD_ENGINE_OVERVIEW.md) | Pass. No universal world score, scripted history, encounter scaling, or repository-stored live world state was found. |
| GM tools apply owners without replacing agency or inventing mechanics | [Game Master Framework](../../docs/gm/GAME_MASTER_FRAMEWORK.md), [GM Responsibilities](../../docs/gm/GM_RESPONSIBILITIES.md) | Pass. Generators remain bounded procedures and provisional rulings remain explicitly non-canonical. |
| Persistence remembers established reality without overriding its mechanical owners | [Persistence Authority](../../docs/persistence/PERSISTENCE_AUTHORITY.md), [Persistence Integration](../../docs/persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md) | Pass. The authority order remains Repository Canon, Campaign Canon, Historical Record, Current Campaign State, Current Session, and Current Narration. |
| Templates represent existing contracts without creating truth or rules | [Template Coverage Map](../../templates/TEMPLATE_COVERAGE.md) | Pass. Blank templates remain external-state contracts and no populated campaign record entered the repository. |

## Corrected Documentation Drift

### Development to Skill and Evolution Ownership

Early Development documents still described Phase 3 and Phase 4 as future work. Those passages now link directly to the completed [Skill Engine](../../docs/skills/README.md) and [Monster Evolution](../../docs/monster-evolution/README.md) owners. The original boundaries, safeguards, and procedures are unchanged.

### Skill Tree Boundaries

Human and monster Skill-tree documents now route Classes, institutions, species traits, and biological Evolution to their completed owners instead of referring to later phases. No tree structure or crossover rule changed.

### Soul Weapon to Magic Ownership

Soul Weapon compatibility and unconventional-vessel text no longer treats Magic as incomplete. Unsupported magical contributions now route to the completed [Magic framework](../../docs/magic/README.md), with the existing Alpha Playtest procedure reserved for a genuinely missing narrow mechanic.

### Magic to World Engine Ownership

Magic's world-interaction documents now route detailed simulation variables and procedures to the completed [World Engine](../../docs/world-engine/README.md) rather than promising later Phase 8 work.

### GM and Template Boundaries

GM procedures no longer describe Phase 9 generators or Phase 11 templates as forthcoming. They preserve the same rule: generators and blank formats cannot establish canon, invent mechanics, or populate unsupported state.

### Persistence and Template Boundaries

Campaign Persistence documents now refer to the existing [blank templates](../../templates/README.md) in present tense. The logical architecture remains canonical, storage technology remains an implementation choice, and templates remain subordinate representations.

## Search Results

- No `TODO`, `TBD`, placeholder, or incomplete-phase marker was found in canonical rules or templates.
- No future-tense claim that a completed numbered phase still needs to define its system remains.
- Remaining numbered phase references identify historical ownership, completed framework scope, or template coverage rather than missing work.
- Remaining generic references to later systems or phases are governance boundaries that also apply to any owner-authorized future roadmap.
- Universal-level, power-score, generic-MP, spell-slot, automatic-Awakening, mandatory-Soul-Weapon, and guaranteed-Evolution matches are prohibitions or safeguards.
- No populated campaign record, save directory, current character, or live world-state file was found.

## Open Questions

The non-blocking questions in [Unresolved Questions](../UNRESOLVED_QUESTIONS.md) remain intentionally unresolved. None contradicts current canon or blocks this documentation audit. Evidence candidates remain in [Future Revisions](../FUTURE_REVISIONS.md) without changing rules.

## Result

**Pass**, provided repository-wide link validation and the complete staged diff remain clean at commit time.

## Related Documents

- [Canonical Document Registry](../../docs/DOCUMENT_REGISTRY.md)
- [Cross-Reference and Ownership Audit](CROSS_REFERENCE_AND_OWNERSHIP_AUDIT.md)
- [Design Decisions](../DECISIONS.md)
- [Terminology](../TERMINOLOGY.md)
- [Repository Conventions](../REPOSITORY_CONVENTIONS.md)
- [Development Roadmap](../ROADMAP.md)
