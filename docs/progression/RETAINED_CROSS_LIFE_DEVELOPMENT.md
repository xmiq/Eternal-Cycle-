# Retained Cross-Life Development

## Purpose

This document integrates **FR-001 — Retained Development and Embodiment Relevance** with **FR-005 — Cross-Embodiment Skill Transfer**. It defines how established development from multiple Lives can accelerate current redevelopment without restoring finished power, bypassing embodiment, or replacing present effort.

## Document Control

- **Owner:** cross-Life contribution eligibility, Embodiment Relevance, bounded stacking, historical-territory limits, recalculation, and the shared attribute/Skill retained-development procedure
- **Dependencies:** [Stat XP and Retained Development](STAT_XP_AND_RETAINED_DEVELOPMENT.md), [Reincarnation Skill Crossover](../skills/REINCARNATION_SKILL_CROSSOVER.md), [Life Archive](../persistence/LIFE_ARCHIVE.md), and [Canonical Data Ownership](../persistence/CANONICAL_DATA_OWNERSHIP.md)
- **Extensions:** [Skill Consolidation and Historical Scope](../skills/SKILL_CONSOLIDATION_AND_SCOPE.md) consumes provenance without redefining this framework
- **Consumers:** Development, Skills, Reincarnation, Species, Magic, persistence, GMs, and player-facing capability views

## Core Rule

> Past development creates familiarity and retained developmental advantage, not automatic present capability.

A new incarnation begins with the capabilities permitted by its current embodiment and Reincarnation outcome. Qualifying historical development can make familiar territory easier to rebuild. It does not copy prior attributes, Skill levels, anatomy, access, reliability, or authority into the new body.

Retained Development belongs to Soul continuity. Current attributes belong to current Development records. Current Skill state belongs to the current Skill owner. The Life Archive supplies historical evidence and indexes; it does not own current values.

## Terms

### Historical Contribution

One source Life's evidence-bearing contribution toward redevelopment of one bounded capability. It identifies the source Life, source embodiment or expression, demonstrated extent, relevance to the target route, and provenance.

### Embodiment Relevance

A capability-specific assessment of how much historical practice can inform the current route. It compares actual mechanisms rather than Species names.

### Historical Territory

The extent and conditions of capability legitimately demonstrated by one or more source Lives. Retained acceleration is strongest inside comparable Historical Territory and cannot treat an unmatched numerical peak as universally transferable mastery.

### Familiarity Unlock

Established prior experience with a Skill identity or bounded principle that may support rediscovery when a valid Receiving Route exists. Familiarity Unlock is not current Skill access, Level 0 Instinct, or a prerequisite waiver.

### Translation Bridge

A historically developed expression that connects an older Skill route to a later embodiment. A translated Human expression of a Slime Skill may become more relevant to a later Elf than the original Slime route while preserving one Skill identity and full provenance.

### Effective Retained Acceleration

The bounded current redevelopment advantage derived from qualifying Historical Contributions after relevance, territory, access, prerequisites, and saturation are applied. It is Derived state and may be recalculated.

## Historical Contribution Eligibility

A source Life contributes only when canonical history establishes meaningful development of the capability. Relevant evidence may include:

- sustained use across meaningful variation;
- an attained Development or Skill milestone;
- demonstrated reliability, correction, or mastery;
- significant improvement within that Life;
- successful embodiment translation;
- a source-owned peak or historically important expression.

Possessing a name, surviving briefly, repeating trivial actions, suffering, dying, or rebuilding only already-familiar foundations does not create a fresh contribution. One event can matter when it establishes genuine integration, but drama alone is not evidence.

## Embodiment Relevance Assessment

Assess relevance for the exact capability and target route. Consider only factors that materially affect it:

- body plan, locomotion, limbs, scale, mass, and force generation;
- material composition, organs, senses, nervous or control architecture;
- cognition, timing, feedback, and attention structure;
- Mana Channels or another legitimately developed substitute;
- tools, interfaces, equipment, environment, and external support;
- source and target Skill expressions;
- evolutionary or transformational form intervals within a Life.

Species identity may provide evidence but never determines relevance by itself. Human to Elf Strength may be highly relevant; Human to Slime Strength may transfer only narrow regulation principles. A winged bird's Flight may have zero current boost in a mundane stone body.

### Relevance States

Use an evidence-bearing qualitative state unless a campaign has an authorized numeric profile:

- **Direct:** substantially the same developmental mechanism;
- **High:** most practical learning transfers with bounded recalibration;
- **Moderate:** meaningful shared mechanisms and meaningful translation work;
- **Low:** narrow principles transfer while most practical calibration does not;
- **Zero:** no current developmental mechanism can use the history;
- **Unknown:** insufficient evidence; do not guess.

Zero is legitimate and does not erase familiarity. Unknown is not zero. A reusable Codex statement, Campaign Canon, mechanical rule, or GM adjudication may establish relevance with provenance. Do not precompute a Species matrix.

## Bounded Stacking

Every qualifying source Life may contribute. The system does not inspect only the immediately preceding Life or blindly select the highest peak.

### Qualitative Default

Aggregate relevant Lives into a **Retained Contribution Profile**:

1. identify each source Life's demonstrated territory;
2. discount it by current Embodiment Relevance and translation loss;
3. give the strongest distinct contribution full qualitative weight;
4. give overlapping later contributions diminishing marginal weight;
5. preserve additional Lives as breadth, reliability, translation, and recovery evidence even after acceleration saturates;
6. cap the result below effortless or automatic redevelopment.

Use these effective states where useful: **None**, **Faint**, **Established**, **Strong**, **Extensive**, and **Saturated**. They are contextual acceleration descriptions, not universal percentages or Soul ranks. Saturated still requires meaningful current practice and cannot bypass current ceilings.

### Optional Numeric Realization

If a campaign already uses percentage development rates, each source contributes a nonnegative additive raw amount `c_i` after relevance and territory are assessed. Combine them through bounded saturation:

```text
raw = sum(c_i)
effective = cap * raw / (cap + raw)
```

`cap` is a campaign/rules-profile ceiling for the bounded capability, not a universal Eternal Cycle constant. This formula means:

- every meaningful source can help;
- contributions add rather than multiply;
- equal additional histories have diminishing marginal effect;
- effective acceleration remains below the configured cap;
- no source earns interest from prior acceleration.

A campaign may use another documented monotonic saturation function if it preserves these invariants. Approximate examples such as `+10%` or `+8%` illustrate relative intent; they do not establish universal constants.

## Historical Territory and New Ground

Each contribution applies only while current development remains within territory comparable to what that source Life established. As the current incarnation approaches a source's relevant peak, that source contribution tapers. Beyond all comparable historical territory, retained acceleration ends for that frontier and ordinary new Development governs.

Several Lives with different peaks can form a layered profile. A lower-peak but highly relevant Life may dominate early rebuilding; a less relevant high-peak Life may offer narrower support later. Repeated mastery improves relearning richness and reliability, but no Life's peak becomes a current target guaranteed to be reached.

## Attributes and Stat XP

For attributes and embodied Development:

1. identify a bounded Stat XP Domain rather than a generic attribute label alone;
2. query per-Life Development evidence;
3. compare source adaptation with the current embodiment and training route;
4. aggregate qualifying contributions through bounded stacking;
5. apply acceleration only to productive current Development;
6. taper each source beyond its relevant Historical Territory.

A Human who reached Strength 20 and later reincarnates as an Elf may rebuild similar force-production patterns substantially faster. As a Slime, only low-relevance principles such as load regulation or effort pacing may transfer. Neither incarnation starts at Strength 20.

Attributes and Skills share the contribution framework but need not use identical local measurements. Attributes emphasize embodied adaptation; Skills may also preserve concepts, timing, decisions, sensory interpretation, and techniques.

## Skill Familiarity and Development Boost

Skill continuity separates two outcomes.

### Familiarity Unlock

The Soul has legitimate history with the Skill identity or a bounded transferable principle. This can support rediscovery and recognition of productive routes. It can persist across radically different bodies even when current boost is zero.

Familiarity Unlock does not:

- make the Skill currently usable;
- create anatomy, Mana, equipment, senses, or authority;
- bypass tree, Class, Evolution, institutional, or environmental prerequisites;
- restore prior Skill level or Practised Reliability;
- automatically create Level 0 Instinct.

Level 0 exists only when [Level 0 Instinct](../skills/LEVEL_ZERO_INSTINCT.md) has a valid explicit provenance and receiving route. Otherwise inaccessible prior history remains in retained Skill provenance rather than a second hidden Skill seed.

### Development Boost

A boost exists only when the current embodiment and Receiving Route can reuse practical historical experience. Direct or similar expressions may receive strong acceleration; radical translation may receive low or zero acceleration while familiarity remains.

Current use still requires attempts, practice, experimentation, adaptation, instruction, feedback, and consequence. Historical level is evidence of territory, not the current level.

## Translation Chains

Assess the full expression lineage, not only origin versus current body.

```text
Slime Movement
    -> Human Mana-assisted movement expression
        -> Elf movement expression
```

The Human expression can become a Translation Bridge because it developed the same underlying Skill through a more humanoid route. The Elf may therefore receive meaningful acceleration from the Human Life even if the original Slime expression remains low-relevance.

One stable Skill identity survives when the bounded learned competence remains continuous. Record embodiment-specific expressions, predecessor sources, translation losses, and applications without creating unrelated duplicate Skills. Translation cannot broaden the Skill beyond historically established function.

## Mana Translation

Mana may supply a valid alternative mechanism only when existing Magic rules establish access, Channel capacity, control, costs, and feedback. A Human might translate Slime Movement through a Mana tendril or controlled low-friction effect. The character must still develop that magical route.

Mana is not universal compatibility. If no applicable Mana capability exists, the route remains latent or zero-boost. A magical substitute retains its own costs, failure, suppression, and world-law dependencies.

## Recalculation

Effective Retained Acceleration is Derived state. Recalculate it when a material target condition changes, including:

- Evolution or transformation;
- major anatomy, cognition, sensory, or control change;
- new or lost Mana capability;
- a new tool, interface, or required environment;
- recovered source history;
- a corrected embodiment or Skill lineage;
- supersession of a relevance assessment.

Recalculation creates a new assessment with provenance and supersedes the old current applicability. It does not rewrite source Life history.

## Persistence Contract

Campaign implementations should normalize equivalent logical records:

```sql
CREATE TABLE retained_development_sources (
    retained_source_id TEXT PRIMARY KEY,
    soul_id TEXT NOT NULL,
    source_life_id TEXT NOT NULL,
    source_embodiment_id TEXT NOT NULL,
    capability_type TEXT NOT NULL CHECK (capability_type IN ('development', 'skill')),
    capability_id TEXT NOT NULL,
    demonstrated_extent TEXT NOT NULL,
    provenance_id TEXT NOT NULL,
    FOREIGN KEY (soul_id) REFERENCES souls(soul_id),
    FOREIGN KEY (source_life_id) REFERENCES lives(life_id)
);

CREATE TABLE retained_relevance_assessments (
    assessment_id TEXT PRIMARY KEY,
    retained_source_id TEXT NOT NULL,
    target_incarnation_id TEXT NOT NULL,
    target_embodiment_id TEXT NOT NULL,
    relevance_state TEXT NOT NULL CHECK (relevance_state IN ('direct','high','moderate','low','zero','unknown')),
    unlock_state TEXT NOT NULL CHECK (unlock_state IN ('not_applicable','familiar','accessible','latent','inaccessible')),
    contribution_value REAL CHECK (contribution_value IS NULL OR contribution_value >= 0),
    assessment_basis TEXT NOT NULL,
    provenance_id TEXT NOT NULL,
    supersedes_assessment_id TEXT,
    FOREIGN KEY (retained_source_id) REFERENCES retained_development_sources(retained_source_id),
    FOREIGN KEY (target_incarnation_id) REFERENCES incarnations(incarnation_id)
);

CREATE TABLE retained_effective_profiles (
    profile_id TEXT PRIMARY KEY,
    soul_id TEXT NOT NULL,
    target_incarnation_id TEXT NOT NULL,
    capability_type TEXT NOT NULL CHECK (capability_type IN ('development', 'skill')),
    capability_id TEXT NOT NULL,
    qualitative_state TEXT,
    effective_value REAL CHECK (effective_value IS NULL OR effective_value >= 0),
    bound_profile_id TEXT NOT NULL,
    calculation_provenance_id TEXT NOT NULL,
    calculated_at TEXT NOT NULL,
    is_current INTEGER NOT NULL CHECK (is_current IN (0,1)),
    FOREIGN KEY (soul_id) REFERENCES souls(soul_id),
    FOREIGN KEY (target_incarnation_id) REFERENCES incarnations(incarnation_id)
);
```

Join tables connect an effective profile to all included assessments and connect Skill sources to expression/predecessor histories. Concrete schemas should use direct foreign keys where domain tables permit them. Effective profiles are Derived/Cache records; historical sources remain authoritative evidence and current capability remains with current Development or Skill owners.

## Existing-Campaign Adoption

1. back up the canonical save;
2. audit the Life Archive and source-owned Development, Skill, and embodiment records;
3. identify meaningful historical contributions without awarding credit for names alone;
4. preserve source Life and embodiment references;
5. establish only justified relevance assessments and leave unresolved cases Unknown;
6. construct provenance-bearing contribution and translation lineages;
7. derive bounded current profiles without changing current levels;
8. validate identity, ownership, prerequisites, bounds, and knowledge separation;
9. activate through the normal migration and save protocol.

No retroactive numeric bonus may be invented merely because a past Life existed. This reusable repository performs no populated campaign migration.

## Player Presentation

Expose only established information appropriate to the player-facing interface:

```text
Skill: Slime Movement
Prior-life familiarity: Established
Current embodiment relevance: Low
Effect: Familiarity unlock only
```

```text
Capability: Swordsmanship
Prior-life familiarity: Extensive
Current embodiment relevance: High
Effect: Accelerated redevelopment within established territory
```

Unknown or GM-protected calculation details remain unknown or hidden. Player inspection is not the source of the effect and does not grant Character Knowledge.

## GM Procedure

1. identify the bounded capability currently being developed;
2. query the Soul Overview and relevant Life Summary indexes;
3. drill into only necessary Development, Skill, and embodiment sources;
4. verify meaningful contribution and provenance;
5. assess the current Receiving Route and Embodiment Relevance;
6. separate Familiarity Unlock from Development Boost;
7. apply bounded stacking within each source's Historical Territory;
8. enforce anatomy, Mana, tools, access, and all hard prerequisites;
9. resolve current practice through the current Development or Skill owner;
10. store current progress normally and refresh Derived profiles only when needed.

## Worked Cases

### Similar Embodiment

A Human Life reaches Strength 20. An Elf incarnation starts with Elf bodily capability, not Strength 20. Similar force production and control make the Human contribution highly relevant, so productive Elf training redevelops faster within comparable territory.

### Radical Embodiment

The same Human history informs a Slime only through low-relevance regulation or load principles. Slime force generation must be learned through its own material and control architecture, producing a smaller contribution.

### Unlock Without Boost

A Slime learned Slime Movement. A Human retains familiarity with its movement problem but lacks slime anatomy. Familiarity may unlock reconstruction; physiological boost may be zero. A Mana tendril or low-friction route becomes usable only after the Human develops the required magic and practises the translation.

### Translation Chain

The Human develops a reliable Mana-assisted expression of Slime Movement. A later Elf can use the same Skill identity. The Human expression is a relevant Translation Bridge and supplies meaningful acceleration; the original Slime route remains in provenance.

### Hard Prerequisite

A bird's Winged Flight remains familiar to a mundane stone body with no wings, flight mechanism, or Mana substitute. Current use and boost are zero. Familiarity does not create flight.

### New Capability Appears

The stone incarnation later develops a valid magical-flight system. The prior assessment is superseded, relevance is recalculated, and flight history may now support learning. Historical records are unchanged.

### Repeated Lives

Several Lives develop Swordsmanship to different extents through different bodies. Their relevant contributions add, overlapping contributions diminish, and the effective profile approaches saturation. Additional Lives still enrich reliability, breadth, and translation without multiplicative runaway.

### Beyond Historical Mastery

The current incarnation rebuilds beyond all comparable prior Swordsmanship territory. Retained acceleration tapers out. Further growth is new Development governed by current practice.

## Validation

Validation checks:

- source Life, Soul, capability, and embodiment references resolve and share the correct Soul;
- source history demonstrates meaningful Development;
- Familiarity Unlock and Development Boost remain distinct;
- zero boost and Unknown relevance remain valid distinct states;
- current attributes and Skill levels were not copied from historical peaks;
- contributions are additive before saturation and never recursively compounded;
- each contribution stops or tapers beyond its demonstrated relevant territory;
- assessment and calculation provenance exist;
- current prerequisites and Receiving Routes are satisfied before expression;
- recalculation supersedes applicability without altering history;
- Skill identity and translation lineage remain traceable through future merge provenance;
- Retained familiarity does not imply autobiographical memory or create Character Knowledge;
- Life Archive evidence remains historical rather than current-state authority.

## Safeguards

- No automatic starting attributes or restored Skill levels.
- No multiplicative stacking, retained-interest gain, or bonus earned from bonus-assisted rebuilding alone.
- No trivial-Life, injury, suffering, death, or Reincarnation farming.
- No same-Species shortcut or complete pairwise relevance matrix.
- No anatomy, Mana, tool, Class, Evolution, or environmental prerequisite bypass.
- No universal abstraction of a translated Skill beyond established function.
- No conversion of familiarity into Level 0 without its own valid rule.
- No inference of autobiographical memory.
- No populated campaign records in this repository.

## Related Documents

- [Stat XP and Retained Development](STAT_XP_AND_RETAINED_DEVELOPMENT.md)
- [Development System](DEVELOPMENT_SYSTEM.md)
- [Reincarnation Skill Crossover](../skills/REINCARNATION_SKILL_CROSSOVER.md)
- [Skill Consolidation and Historical Scope](../skills/SKILL_CONSOLIDATION_AND_SCOPE.md)
- [Level 0 Instinct](../skills/LEVEL_ZERO_INSTINCT.md)
- [Reincarnation](../soul/REINCARNATION.md)
- [Life Archive](../persistence/LIFE_ARCHIVE.md)
- [Canonical Data Ownership](../persistence/CANONICAL_DATA_OWNERSHIP.md)
- [Retained Cross-Life Development Template](../../templates/RETAINED_CROSS_LIFE_DEVELOPMENT_TEMPLATE.md)
- [FR-001 and FR-005 Implementation Audit](../../design/audits/FR_001_FR_005_RETAINED_DEVELOPMENT_AUDIT.md)
