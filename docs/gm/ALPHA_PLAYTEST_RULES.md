# Alpha Playtest Rules

## Purpose

This document defines how to begin and run an Eternal Cycle campaign while parts of the ruleset remain unfinished. It allows focused playtesting without allowing an improvised campaign ruling to become canonical merely because it was used successfully.

This is canonical GM guidance. It does not complete any system identified as incomplete in the [Roadmap](../../design/ROADMAP.md), supply missing content for a later phase, or weaken the repository boundary.

The canonical repository must not contain campaign saves, current characters, live world state, active quests, inventories, named campaign settlements or factions, playthrough summaries, session logs, campaign-specific house rules, or completed playtest records.

## Authority

The [Roadmap](../../design/ROADMAP.md) is the source of truth for implementation status. [Design Decisions](../../design/DECISIONS.md) and [Canonical Terminology](../../design/TERMINOLOGY.md) govern accepted design, while files under `docs/` contain playable canonical rules.

A status applies to a particular rule claim, procedure, or capability, not automatically to every subject mentioned in the same document. A completed Development foundation, for example, does not make a later Skill tree or magic system complete.

When statuses interact:

1. Canonical rules govern normally.
2. Canonical Foundations constrain any missing implementation.
3. Provisional Rules may fill only the remaining narrow gap.
4. Unsupported areas require avoidance or an explicitly authorized experiment.

No status permits one source to silently override another. A conflict between canonical rules and accepted design governance must be resolved through the repository process rather than patched during play.

## Rule Status Model

### Canonical

A **Canonical** rule is complete and authoritative within its stated scope.

Canonical rules:

- apply normally;
- take precedence over Provisional Rules;
- may not be silently overridden during play;
- retain their stated Owning Systems, limits, safeguards, and exceptions;
- may be changed only through the repository's normal design process.

A GM may interpret a Canonical rule where ordinary judgment is required, but interpretation cannot reverse its core rule or invent an exception solely to force a desired outcome.

### Canonical Foundation

A **Canonical Foundation** is an authoritative set of principles and boundaries for a system whose detailed implementation belongs to a later roadmap task.

The foundation itself is Canonical. Missing procedures, content, formulas, branches, or generators are not.

Current examples include:

- [Soul Weapon Foundations](../soul/SOUL_WEAPON_FOUNDATIONS.md);
- the [World Engine Overview](../world-engine/WORLD_ENGINE_OVERVIEW.md);
- [GM Principles](GM_PRINCIPLES.md);
- the candidate constraints and GM preparation in [Reincarnation](../soul/REINCARNATION.md), while later generation tooling remains incomplete.

A Canonical Foundation constrains improvisation. A provisional Soul Weapon ability, for example, must preserve the Weapon Soul as a distinct person and cannot ignore awakening, mutual transformation, persistence, or ownership merely because Phase 6 is incomplete.

### Provisional

A **Provisional Rule** is a temporary, campaign-local ruling used because a relevant canonical implementation is incomplete or a narrow situation is not yet covered.

A Provisional Rule:

- must respect Canonical rules, Canonical Foundations, terminology, and accepted decisions;
- applies only to the current playtest and its stated scope;
- must be recorded in the external Campaign Record;
- may be revised, replaced, or removed when evidence changes;
- cannot silently establish a permanent design decision;
- should introduce the smallest amount of new machinery needed for play;
- must state what causes it to expire or be reviewed.

Repeated use, player preference, dramatic success, or long duration does not promote a Provisional Rule into Canonical status. Adoption requires the normal design process.

### Unsupported

An **Unsupported** area lacks enough Canonical rules or Foundations to adjudicate reliably without inventing a major system.

The GM should not center ordinary alpha play on an Unsupported area. The project owner may explicitly authorize an experimental implementation, but it remains external, clearly labelled, and non-canonical until separately reviewed.

Unfinished does not automatically mean Unsupported. If existing canon establishes ownership, boundaries, costs, and safeguards, a narrow Provisional Rule may be enough. A gap becomes Unsupported when resolving it would require foundational assumptions, a broad progression structure, or repeated rulings whose interactions cannot be checked against canon.

## Current Alpha Readiness

### Complete Canonical Rules and Foundations

The following material is ready to constrain alpha play within its stated scope:

- repository governance through [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md), [Design Decisions](../../design/DECISIONS.md), [Canonical Terminology](../../design/TERMINOLOGY.md), and the [Roadmap](../../design/ROADMAP.md);
- [Design Philosophy](../core/DESIGN_PHILOSOPHY.md), including Rule Zero, earned progression, fair mystery, and meaningful consequences;
- the complete [Soul Engine](../soul/README.md), including [Reincarnation](../soul/REINCARNATION.md), [Soul Depth](../soul/SOUL_DEPTH.md), [Soul Resonance](../soul/SOUL_RESONANCE.md), [Soul Echoes](../soul/SOUL_ECHOES.md), [Soul Space](../soul/SOUL_SPACE.md), [Soul Constellations](../soul/SOUL_CONSTELLATIONS.md), [Soul Titles](../soul/SOUL_TITLES.md), [Soul Avatars](../soul/SOUL_AVATARS.md), [Retained Instincts](../soul/RETAINED_INSTINCTS.md), and the [Akashic Archive](../soul/AKASHIC_ARCHIVE.md);
- Soul-system ownership and protection through [Soul System Interactions](../soul/SOUL_SYSTEM_INTERACTIONS.md) and [Soul Engine Safeguards](../soul/SOUL_ENGINE_SAFEGUARDS.md);
- the complete [Development System](../progression/README.md), including [Physical Development](../progression/PHYSICAL_DEVELOPMENT.md), [Skill Development principles](../progression/SKILL_DEVELOPMENT.md), [Profession Development principles](../progression/PROFESSION_DEVELOPMENT.md), [Magical Development principles](../progression/MAGICAL_DEVELOPMENT.md), [Social and Leadership Development principles](../progression/SOCIAL_AND_LEADERSHIP_DEVELOPMENT.md), and [Species Development principles](../progression/SPECIES_DEVELOPMENT.md);
- [Stat XP and Retained Development](../progression/STAT_XP_AND_RETAINED_DEVELOPMENT.md), [Development Interactions](../progression/DEVELOPMENT_INTERACTIONS.md), [Capability Assessment](../progression/CAPABILITY_ASSESSMENT.md), and [Development Safeguards](../progression/DEVELOPMENT_SAFEGUARDS.md);
- the complete [Skill Engine](../skills/README.md), including human and monster Skill Trees, Reincarnation crossover, Adaptive Skills, Skill Evolution, Skill Fusion, active and passive expression, Hidden Skills, Conceptual Skills, and anti-proliferation safeguards;
- the canonical [Game Master Framework](GAME_MASTER_FRAMEWORK.md) and [GM Principles](GM_PRINCIPLES.md);
- Canonical Foundations for [Soul Weapons](../soul/SOUL_WEAPON_FOUNDATIONS.md) and the [World Engine](../world-engine/WORLD_ENGINE_OVERVIEW.md).

Development and Skill Engine rules are complete within their stated scopes. Later class, magic, species-evolution, institutional, content, formula, and generator tasks are not complete merely because their Development or Skill boundaries are canonical.

### Incomplete or Provisional Areas

The [Roadmap](../../design/ROADMAP.md) remains authoritative. Alpha play should expect provisional or unsupported gaps in:

- monster ecology, species stages, branching evolution, hidden conditions, mutations, hybrid forms, apex monsters, and monster civilizations;
- human classes, traditions, schools, institutions, advancement, and class evolution;
- detailed Soul Weapon awakening, growth, abilities, Weapon Echoes, compatibility, and Weapon Manifestations;
- mana, affinities, spell formation, rituals, enchanting, alchemy, divine magic, forbidden magic, and other complete magic procedures;
- detailed World Engine variables and procedures for populations, resources, economics, ecology, factions, war, disease, advancement, dungeons, World Stability, Gates, and long simulations;
- encounter, monster, NPC, dungeon, faction, world-event, time-skip, and Age-transition generators;
- character, species, Skill, Soul Weapon, faction, settlement, dungeon, Soul Avatar, and Gate-event templates not marked complete on the Roadmap.

Mentioning these areas in this framework does not change their roadmap status.

## Minimum Playable Alpha Scope

A useful first alpha usually contains:

- one player soul or a small party;
- one current incarnation per participating soul;
- one settlement or another bounded community or social context;
- one surrounding wilderness, district, ruin, route, habitat, or similarly bounded region;
- a limited number of local factions or organized interests;
- a small, coherent monster ecology where monsters are relevant;
- one immediate pressure, conflict, mystery, or opportunity;
- a bounded set of known Skills;
- a restricted Provisional magic list if magic is used;
- no immediate World Reset unless the test deliberately targets Reincarnation or Age transitions.

The scope is a recommendation, not a mandatory village scenario. A starting context may center on a monster nest, wilderness territory, city district, caravan, isolated ruin, artificial habitat, divine domain, underwater community, mobile group, or another valid setting.

Limited scope reduces the number of unsupported assumptions, keeps provisional machinery visible, and produces feedback tied to identifiable rules. The scope may expand after the GM knows which systems the playtest can support.

## Alpha Campaign Layers

### Canonical Repository

The repository contains only reusable rules, systems, templates, design decisions, terminology, roadmap controls, and GM guidance. It does not record what happened in a campaign.

### Campaign Record

The **Campaign Record** is stored outside this repository. It may contain:

- current characters and bodies;
- campaign Soul state;
- inventories and relationships;
- settlements and factions;
- quests and timelines;
- current world state;
- session records;
- Provisional Rules and campaign-specific house rules;
- playtest observations and feedback.

The Campaign Record is authoritative for that campaign's established fiction, subject to Canonical rules and the conversion procedure below. It is not authoritative design canon.

### Design Feedback

**Design Feedback** is a neutral observation prepared for possible repository review. Useful feedback may report that:

- a rule was unclear;
- two systems appeared to conflict;
- a safeguard failed;
- a procedure was too slow;
- an unfinished system was repeatedly needed;
- a Provisional Rule produced a useful or harmful result.

Feedback does not become Canonical automatically. It may result in no repository change, a developer note, an unresolved question, a proposed decision, or later canonical work.

## Provisional Ruling Procedure

When play reaches a missing or unclear rule, the GM follows this procedure:

1. **Identify the issue.** State the exact action, capability, consequence, or procedure that lacks a clear rule.
2. **Check canon.** Consult relevant canonical documents, terminology, decisions, and roadmap status.
3. **Determine ownership.** Identify the narrowest Owning System for the claimed effect.
4. **Identify boundaries.** Record applicable access, embodiment, cost, persistence, agency, compatibility, consequence, and safeguard constraints.
5. **Classify support.** Decide whether Canonical rules already resolve the issue, a Provisional Rule can resolve the narrow gap, or the area is Unsupported.
6. **Make the narrowest ruling.** Add only what the immediate test needs and avoid building an unseen general system.
7. **Record status and scope.** Put the ruling, owner, affected participants, conditions, and explicit Provisional status in the external Campaign Record.
8. **Set expiry or review.** State what event, new evidence, canon update, repeated use, or session boundary causes reconsideration.
9. **Review after play.** Compare the result with intended pressure, player understanding, safeguards, and actual consequences.
10. **Submit feedback separately.** Route useful findings through the canonical design process without copying live campaign state into the repository.

A useful external ruling entry records the issue, status, Owning System, canon consulted, scope, ruling, dependencies, expiry condition, observed result, and any Design Feedback. This is a recommended format, not a repository file.

### Prohibited Provisional Outcomes

A Provisional Rule must never:

- contradict an accepted decision or Canonical rule;
- create a universal character level, combat rating, power score, or progression currency;
- treat persistence as immediate access, expression, or reliability;
- bypass the current body, species, environment, or other embodiment requirements;
- trivialize Final Death or erase its worldly consequences;
- replace current-incarnation or player agency;
- merge human and monster progression without a valid crossover rule;
- grant automatic Skill mastery from memory, observation, a label, or prior ownership;
- make suffering, killing, repetition, time, injury, or death inherently profitable;
- move campaign data or provisional-ruling logs into this repository.

## Adjudicating Unfinished Systems

The completed [Skill Engine](../skills/README.md) applies directly. A Skill claim is not Provisional merely because it concerns a human tree, monster tree, crossover, adaptation, evolution, fusion, expression mode, hidden capability, or conceptual capability. Any remaining gap receives its status claim by claim under the complete Skill rules and the still-incomplete system that actually owns the missing effect.

### Monsters and Evolution

Until Phase 4 is complete:

- use biologically, ecologically, and metaphysically coherent traits;
- distinguish current Species Traits from learned Skills and retained soul history;
- do not grant evolution solely because XP, kills, consumption, age, or repetition reached an arbitrary total;
- require meaningful conditions, adaptation, current compatibility, gains, losses, needs, and consequences;
- treat proposed branches and requirements as Provisional;
- do not turn one campaign ruling into a universal evolution tree.

### Human Classes and Professions

Until Phase 5 is complete:

- treat classes as descriptive traditions, roles, curricula, or institutional packages rather than universal level ladders;
- use [Profession Development](../progression/PROFESSION_DEVELOPMENT.md) and [Skill Development](../progression/SKILL_DEVELOPMENT.md) for present capability;
- keep formal rank, Credential, Licence, reputation, access, and actual competence distinct;
- mark class packages, advancement paths, and undeveloped institutions as Provisional.

### Soul Weapons

Until Phase 6 is complete:

- use [Soul Weapon Foundations](../soul/SOUL_WEAPON_FOUNDATIONS.md) as the binding authority;
- allow awakening only through genuine shared pressure, resonance, and mutual transformation;
- preserve the Weapon Soul as a distinct person with agency, perspective, and its own capability;
- do not improvise unrestricted weapon evolution, inheritance, forms, or ability accumulation;
- treat detailed abilities, growth routes, Weapon Echoes, and manifestations as Provisional unless an existing rule owns them.

### Magic

Until Phase 7 is complete, establish only a small local magical model. State its:

- source and metaphysical assumptions;
- access requirements and receiving system;
- costs and recovery;
- limits and risks;
- environmental dependencies;
- visible effects and evidence.

Keep magical knowledge, potential, access, capacity, reserves, control, authority, and Embodied Expression distinct under [Magical Development](../progression/MAGICAL_DEVELOPMENT.md). Reincarnation does not grant unrestricted spell inheritance, and one local Provisional model is not universal magic canon.

### World Engine

Until Phase 8 is complete:

- simulate only the resolution needed for current decisions;
- use established conditions, causal chains, actors, pressures, counterforces, and consequences;
- track major populations or pressures instead of every person or transaction;
- preserve plausible off-screen change without predetermining outcomes;
- separate fictional world facts from the Provisional procedure used to simulate them;
- avoid turning a local abstraction into a universal economic, ecological, political, or historical formula.

### GM Tools

Until Phase 9 is complete:

- use transparent qualitative judgment;
- state material uncertainty and its cause;
- compare capability through [Capability Assessment](../progression/CAPABILITY_ASSESSMENT.md);
- preserve fair clues and established causality;
- do not invent authoritative generators or secret tables and present them as Canonical.

## Reincarnation During Alpha Play

The alpha framework adds no Reincarnation mechanics. Use [Reincarnation](../soul/REINCARNATION.md) and the related Soul Engine rules directly:

1. **Confirm Final Death.** Resolve applicable survival, recovery, resurrection, or substitution effects and preserve the death's consequences.
2. **Perform Life Reconciliation.** Apply the canonical severance and reconciliation procedure.
3. **Classify continuity.** Separate what ends with the body, what remains in the world, and what legitimately persists with the soul.
4. **Form the Soul Echo.** Create the completed incarnation's Echo through [Soul Echoes](../soul/SOUL_ECHOES.md), without treating death as an advancement reward.
5. **Resolve Interlife.** Advance external time and apply relevant world consequences at an appropriate level of detail.
6. **Generate valid candidates.** Use the embodiment, world-state, soul-compatibility, earned-access, and causal-placement constraints in Reincarnation; keep any incomplete selection tool narrow and Provisional.
7. **Apply the Reincarnation Mode.** Use the campaign's established Emergent, Constrained Choice, or Directed mode without reopening the result for optimization.
8. **Determine memory and access.** Apply [Soul Depth](../soul/SOUL_DEPTH.md), [Soul Resonance](../soul/SOUL_RESONANCE.md), Echo, [Retained Instinct](../soul/RETAINED_INSTINCTS.md), Soul Integrity, and other established access rules.
9. **Establish the new body.** Define its valid species, stage, context, anatomy, capabilities, limitations, and current conditions without importing former bodily statistics.
10. **Begin redevelopment.** Rebuild Current Access, Embodied Expression, and Practised Reliability through present-life effort under the [Development System](../progression/README.md).

If the alpha requires a missing generator or detailed species tree, that gap receives its own status. It does not alter the authority of the canonical transition.

## Recommended Alpha Modes

These are optional playtest configurations, not universal campaign modes.

### First-Life Mode

Begin with a relatively undeveloped soul and focus on current embodiment, local Development, relationships, consequences, and the base setting before testing persistence.

### Reincarnated Mode

Begin after at least one completed prior life and test memory access, Soul Echoes, Retained Instincts, Stat XP, Skill Imprints, incompatibility, and rebuilding.

### Monster-Life Mode

Begin in a monster body and test Species Development, instinct, ecology, social possibility, and the boundaries of Provisional evolution without assuming a complete tree.

### Reincarnation Stress Test

Run a bounded first life followed by Final Death, Life Reconciliation, Interlife, and a second incarnation. Short duration does not excuse staged death rewards or erase consequences.

### Mature-Soul Mode

Begin with several established Soul Imprints, prior-life facts, or Echoes and test selective access, internal complexity, and rebuilding. This mode requires more adjudication and carries a greater risk of assuming undefined prior progression, so each starting element needs provenance and a valid current status.

## Alpha Safety Limits

For early tests, the GM should normally:

- focus on no more than one or two unfinished major systems at once;
- keep Provisional ability, spell, trait, and item lists small;
- avoid initial Soul Avatar status unless testing Soul Avatars specifically;
- avoid beginning with a fully awakened Soul Weapon unless testing its foundation;
- avoid enormous accumulated Soul Imprint or Echo libraries;
- avoid unrestricted cross-species mastery;
- avoid large-scale war or centuries of detailed simulation unless testing those areas;
- record irreversible Provisional outcomes cautiously;
- tell players which mechanics are Canonical, foundational, Provisional, or Unsupported;
- stop and re-scope when provisional rulings begin creating a hidden major system.

These are risk controls, not universal prohibitions. A focused test may deliberately cross one when the project owner and players understand the objective, missing canon, and conversion risk.

## Continuity When Canon Changes

When a later Canonical rule replaces a Provisional mechanic:

1. Preserve established fictional consequences where practical.
2. Use the new Canonical mechanic for future resolution.
3. Translate existing capability and progress conservatively into valid new structures.
4. Do not remove legitimately earned progress merely because terminology or representation changed.
5. Do not preserve an exploit, duplicate benefit, invalid bypass, or unsupported capability merely because it occurred under a Provisional Rule.
6. Discuss substantial retroactive changes with affected players before applying them.
7. Record the conversion and any campaign-specific ruling in the external Campaign Record.
8. Resolve ambiguity through Rule Zero, current-incarnation agency, meaningful consequences, earned progression, embodiment, and the new rule's Owning System.

Canonical rules normally govern future resolution. They do not require rewriting every past fictional event. A past victory, injury, relationship, migration, or political consequence may remain part of the fiction even when the temporary mechanic that helped resolve it is retired.

## Post-Session Feedback Procedure

After a playtest session, review:

1. Which Canonical rules and Canonical Foundations were used?
2. Which Provisional Rules were needed, changed, or retired?
3. Did any claimed effect lack a clear Owning System?
4. Was any capability too easy to gain, retain, translate, or repeat?
5. Was retained progression too accessible or too disconnected from present effort?
6. Did embodiment, species, health, environment, tools, and preparation matter?
7. Did Final Death remain meaningful and preserve external consequences?
8. Did players understand uncertainty, hidden information, and rule status?
9. Did a Provisional Rule create an exploit, duplicate benefit, or agency problem?
10. Was a later roadmap system repeatedly required?
11. Should the observation become a developer note, unresolved question, proposed decision, or no repository change?

Record session-specific answers outside the repository. Any repository proposal should extract the general design issue without importing current characters, live world state, or playthrough history.

## Suggested External Record Format

An external playtest workspace may use a structure such as:

```text
campaign/
  CAMPAIGN_STATUS.md
  CHARACTERS/
  WORLD/
  SESSIONS/
  PROVISIONAL_RULINGS.md
  PLAYTEST_FEEDBACK.md
```

This example directory belongs outside the Eternal Cycle canonical repository. Do not create these files or folders here.

## Related Documents

- [Canonical Rules Map](../README.md)
- [GM Rules Index](README.md)
- [Game Master Framework](GAME_MASTER_FRAMEWORK.md)
- [GM Principles](GM_PRINCIPLES.md)
- [Design Philosophy](../core/DESIGN_PHILOSOPHY.md)
- [Soul Rules Index](../soul/README.md)
- [Progression Rules Index](../progression/README.md)
- [World Engine Overview](../world-engine/WORLD_ENGINE_OVERVIEW.md)
- [Roadmap](../../design/ROADMAP.md)
- [Design Decisions](../../design/DECISIONS.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
