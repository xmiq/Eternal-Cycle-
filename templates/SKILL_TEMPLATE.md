# Skill Record Template

Use this storage-neutral template to record one bounded learned capability without turning its name into capability, collapsing techniques into Skills, or treating persistent familiarity as current mastery.

The repository keeps this template blank. A populated campaign Skill record belongs in an external Campaign Record. A reusable Skill definition may enter `docs/` only through ordinary canonical governance; completing this form does not make it canon.

## Document Control

- **Template owner:** Player State Skill profiles of the [Campaign Persistence Engine](../docs/persistence/STRUCTURED_PERSISTENCE_ARCHITECTURE.md#player-state)
- **Primary mechanical owner:** [Skill Development](../docs/progression/SKILL_DEVELOPMENT.md) and the applicable specialist document in the [Skill Engine](../docs/skills/README.md)
- **Dependencies:** Development, embodiment, a valid human, monster, or other receiving tree, and every system that owns a requirement or supporting effect
- **Extensions:** Adaptive emergence, active or passive expression, Skill Evolution, Skill Fusion, hiddenness, conceptual operation, and reincarnation crossover
- **Consumers:** Character records, capability assessment, training, Skill trees, Classes, Professions, Evolution, Magic, Soul Weapons, GM adjudication, and persistence validation
- **Repository boundary:** no holder-specific progress, current access, training history, hidden discovery, or campaign ruling belongs in this blank file

This template owns the organization of a Skill record. It does not own anatomy, statistics, species traits, spells, magical access, Class benefits, Soul effects, equipment effects, or world recognition supplied by another system.

## Usage Guidance

1. Apply the [Capability Representation](../docs/skills/SKILL_ENGINE_SAFEGUARDS.md#capability-representation) test before creating a Skill record.
2. Record the smallest truthful learned capability: tactic, habit, technique, Skill, branch, Discipline, or another owner-defined representation.
3. Name one Skill identity and one origin tree. A broad label is not permission to perform every related action.
4. Separate persistent familiarity, present access, embodied execution, reliability, effectiveness, and recognition.
5. Link prerequisites and supporting effects to their owners instead of copying them into the Skill.
6. Use specialist extensions only when their canonical conditions are independently met.
7. Preserve unknown, hidden, disputed, and inaccessible states without inventing detail to fill a field.
8. Validate populated campaign records before activation and reusable proposals through repository governance.

## Required Field Policy

**Required** fields must be present or carry an explicit `Unknown`, `Not Yet Verified`, `Disputed`, or `Requires Source Recovery` state. **Conditional** fields become required only when their feature exists. **Optional** fields grant nothing when omitted.

## Record Contract

### Required Identity

- **Record ID:** `<stable Skill ID>`
- **Record type:** `Skill Record`
- **Record mode:** `<campaign-local | reusable canonical proposal>`
- **Authoritative module:** `Skills`
- **Skill name:** `<current bounded label>`
- **Holder:** `<Character or other capable Actor ID; campaign-local records only>`
- **Skill identity:** `<exact function and method family represented>`
- **Capability representation:** `<technique | Skill | branch | Composite Skill | Specialisation | other established form>`
- **Record version:** `<revision>`
- **Campaign or repository version:** `<applicable version reference>`

### Required Authority and Provenance

- **Persistence authority:** `<applicable authority layer>`
- **Truth layer:** `<Campaign Canon | Historical Record | Character Knowledge | Research | another valid layer>`
- **Canonical mechanical owner:** `<Skill Development or named specialist owner>`
- **Status:** `<candidate | learning | accessible | practised | latent | inaccessible | suppressed | rusted | superseded | other established state>`
- **Source:** `<instruction, practice, observation, experimentation, adaptation, retained history, or other established source>`
- **Origin tree:** `<human | monster | shared | another explicitly established tree>`
- **Origin context:** `<species, body, culture, Discipline, Profession, Class, institution, ecology, or incarnation reference>`
- **First established at:** `<Timeline or Historical Record reference>`
- **Last materially changed at:** `<Timeline, Transaction, or explicit unknown>`
- **Typed references and dependents:** `<relation type -> stable record ID>`
- **Validation status:** `<Validation Run, review result, warnings, or not yet validated>`

## Capability Boundary

### Required Fields

- **Purpose:** `<recurring function the Skill serves>`
- **Included operations:** `<bounded methods and applications>`
- **Excluded operations:** `<nearby capabilities this Skill does not grant>`
- **Scale and context:** `<ordinary target, range, duration, environment, and complexity>`
- **Evidence that this is a Skill:** `<meaningful practice, feedback, adjustment, variation, integration, and reliability>`
- **Distinction from adjacent records:** `<why this is not only a tactic, habit, technique, trait, spell, tool effect, office, or reputation>`

### Conditional Fields

- **Parent Discipline or Skill:** `<typed reference and relationship>`
- **Techniques:** `<Technique IDs and their narrower functions>`
- **Branches:** `<branch IDs and qualification boundaries>`
- **Specialisations:** `<narrow scope, refinement, and tradeoffs>`
- **Composite components:** `<component Skill IDs and coordination requirement>`
- **Transferable Principles:** `<principle, source evidence, and limits>`

The Skill label summarizes earned capability. It does not create effects, waive prerequisites, or establish mastery beyond the evidence recorded here.

## Requirements and Dependencies

### Required Fields

- **Foundational capabilities:** `<Skill, Track, knowledge, judgment, or control references>`
- **Embodiment:** `<anatomy, scale, senses, cognition, coordination, or form requirements>`
- **Tools and materials:** `<required interfaces, quality, maintenance, and substitutions>`
- **Environment:** `<conditions, space, medium, signals, hazards, and exclusions>`
- **Information and instruction:** `<languages, models, teachers, records, or feedback routes>`
- **Resource and recovery costs:** `<attention, stamina, time, materials, mana, risk, or other owner-defined costs>`
- **Access and authority:** `<permission, legal status, relationship, institution, magical Access, or other gate>`

### Conditional Fields

- **Species structures or Trait Expressions:** `<Species Reference and owner links>`
- **Magical requirements:** `<Mana Relation, affinity, Channel, spell, or magical procedure references>`
- **Class, Profession, or institutional requirements:** `<framework and current relationship references>`
- **Soul requirements:** `<Soul Link, Resonance, Title, Echo, Depth, or other exact owner reference>`
- **Soul Weapon requirements:** `<bond, form, consent, interface, or technique reference>`
- **Group requirements:** `<roles, communication, trust, Formation, or Relationship references>`

Losing a requirement may suppress expression without erasing legitimately retained familiarity. No prerequisite becomes part of the Skill merely because the Skill depends on it.

## Development and Evidence

### Required Fields

- **Learning route:** `<training, instruction, experimentation, adaptation, or mixed route>`
- **Meaningful practice:** `<what is practised and why it produces relevant adaptation>`
- **Feedback and correction:** `<signals, teacher, consequence, measurement, or review route>`
- **Variation tested:** `<conditions, opponents, tools, pressure, or cases beyond one rehearsal>`
- **Integrated understanding:** `<execution, judgment, diagnosis, theory, or tacit knowledge actually established>`
- **Practised Reliability evidence:** `<what can be repeated, under which conditions, with what known failures>`
- **Current learning priorities:** `<specific limitations or Review Points>`

### Conditional Fields

- **Skill XP and Skill Imprint:** `<persistent familiarity, source history, and access state; never a spendable currency>`
- **Mastery claim:** `<bounded scope, varied evidence, adaptability, and known exceptions>`
- **Plateau:** `<limiting cause, evidence, and possible learning routes>`
- **Diminishing returns:** `<what familiar practice no longer teaches>`
- **Rust or regression:** `<lost calibration or reliability, cause, and recovery evidence>`
- **Suppression:** `<external or internal barrier and interruption conditions>`
- **Inaccessible mastery:** `<retained potential without a valid present route>`
- **Maladaptation:** `<harmful pattern, fair evidence, consequence, and correction route>`

Repetition, time, suffering, killing, survival, or one successful performance does not establish Development without meaningful adaptation and integration.

## Six-Layer Capability Profile

Complete each layer independently. Do not collapse them into one rank or power score.

- **Persistent Potential:** `<Skill XP, Skill Imprint, retained principles, and provenance>`
- **Current Access:** `<what the present incarnation can recall, understand, receive, or reconstruct>`
- **Embodied Expression:** `<what the present body, senses, cognition, tools, Magic, and environment can execute>`
- **Practised Reliability:** `<what remains dependable under stated pressure and variation>`
- **Contextual Effectiveness:** `<objective, matchup, environment, preparation, team, counterplay, and uncertainty>`
- **World Recognition:** `<rank, certification, reputation, tradition, observer, and accuracy>`

A difference in one layer does not imply superiority in the others.

## Expression Profile

### Required Fields

- **Expression mode:** `<active | passive | mixed>`
- **Initiation:** `<deliberate choice, qualifying cue, maintained state, or other valid route>`
- **Selection and targeting:** `<what the holder can choose and what remains constrained>`
- **Maintenance:** `<attention, posture, resource, tool, environment, or coordination demands>`
- **Interruption and disruption:** `<surprise, overload, fatigue, injury, conflicting input, suppression, or other cause>`
- **Failure states:** `<misread, mistiming, overcommitment, partial effect, collateral consequence, or other causal failure>`
- **Recovery:** `<rest, recalibration, correction, treatment, retraining, or resource route>`

### Conditional Fields

- **Internalised expression:** `<evidence that reduced attention reflects practice rather than a free effect>`
- **Background Load:** `<attention, filtering, competing demand, and overload consequence>`
- **Conscious Override:** `<what can be interrupted or redirected and at what cost>`
- **Reflex or conditioned response:** `<cue, scope, false-positive risk, and agency safeguard>`
- **Sensory filter or background analysis:** `<signal, discrimination, uncertainty, and overload>`
- **Group expression:** `<participants, roles, communication, failure, and individual action limits>`

Passive does not mean costless, permanent, infallible, or beyond player choice. Active does not mean unrestricted or independent of preparation.

## Optional Specialist Extensions

### Adaptive Skill Extension

Complete only after the [Adaptive Skills](../docs/skills/ADAPTIVE_SKILLS.md) emergence and representation tests pass.

- **Candidate record:** `<recurring need and proposed bounded function>`
- **Emergence evidence:** `<engagement, variation, feedback, adjustment, integration, consequence, and reliability>`
- **Existing representations checked:** `<why tactic, habit, technique, branch, trait, or Evolution matter is insufficient>`
- **Recognition and naming:** `<when and by whom the stable capability was recognized>`
- **Failed or malformed routes:** `<overfitting, injury, trauma, context dependence, or other evidence>`

### Skill Evolution Extension

Complete only when the [Skill Evolution](../docs/skills/SKILL_EVOLUTION.md) owner establishes a transformed capability.

- **Predecessor Skill:** `<stable Skill ID and version>`
- **Continuity retained:** `<function, principles, techniques, and history that remain>`
- **Qualitative transformation:** `<new relationship among methods, scope, judgment, or expression>`
- **Evidence and conditions:** `<meaningful use, integration, pressure, insight, and present requirements>`
- **Tradeoffs and exclusions:** `<what narrowed, changed, became costly, or no longer applies>`
- **Lineage cleanup:** `<active, retained, archived, or superseded predecessor records>`

Evolution is not an automatic level-up, rarity increase, or permission to accumulate every predecessor effect.

### Skill Fusion Extension

Complete only when the [Skill Fusion](../docs/skills/SKILL_FUSION.md) integration test passes.

- **Component Skills:** `<stable IDs and exact contributions>`
- **Integrated function:** `<distinct recurring purpose requiring coordinated practice>`
- **Integration evidence:** `<feedback, adaptation, variation, and reliability of the combination itself>`
- **Component disposition:** `<still active, narrowed, archived, or superseded>`
- **Shared-cost and duplicate-gain check:** `<one event, one owned Development gain per adaptation>`
- **Failure at component boundaries:** `<coordination, timing, access, or incompatibility risks>`

Possessing the components does not automatically create the Fusion, and the Fusion does not erase component ownership by default.

### Skill Consolidation Extension

Complete when [Skill Consolidation](../docs/skills/SKILL_CONSOLIDATION_AND_SCOPE.md) establishes that several records substantially represent one capability.

- **Consolidation event:** `<stable event or transaction reference>`
- **Resulting active Skill:** `<stable Skill ID>`
- **Predecessor Skills:** `<typed stable Skill references>`
- **Identity disposition:** `<new coherent identity | established identity absorbs duplicates>`
- **Capability-overlap evidence:** `<function, method, mechanism, application, Development, and embodiment evidence>`
- **Development reconciliation:** `<shared evidence counted once, distinct evidence preserved, uncertainty>`
- **Preserved scope:** `<coherent union of historically established applications>`
- **Excluded scope:** `<linguistically related or mechanically distinct capabilities not granted>`
- **Predecessor disposition:** `<superseded current record with retained historical provenance>`
- **Cross-Life lineage:** `<Life, embodiment, expression, and Translation Bridge references>`
- **Validation:** `<identity, lineage, duplicate-gain, scope, and reference checks>`

Consolidation is not Fusion or Evolution. It creates no free Development and deletes no predecessor history.

### Historical Scope Interpretation

Use for broad, merged, ambiguous, or Conceptual Skill claims.

- **Proposed application:** `<exact operation, target, route, scale, and context>`
- **Classification:** `<established application | reasonable extension | related but distinct capability | unsupported interpretation>`
- **Supporting history:** `<acquisition, practice, successes, relevant failures, predecessors, Evolution, embodiments, and translations>`
- **Current enabling mechanics:** `<body, tool, Mana, access, authority, environment, or other owner references>`
- **Further Development required:** `<experimentation, training, Evolution, separate capability, or none>`
- **Known limits and uncertainty:** `<bounded evidence>`

The Skill name is descriptive. It grants no application unsupported by history and present mechanics.

### Hidden Skill Extension

Complete the factual record even when some audiences cannot see it. Apply the [Hidden Skills](../docs/skills/HIDDEN_SKILLS.md) visibility rules to each audience separately.

- **Visibility profile:** `<GM truth, holder knowledge, player knowledge, social knowledge, and system display>`
- **Cause of hiddenness:** `<undiscovered, concealed, obscured, misdiagnosed, inaccessible, or other established cause>`
- **Discovery Evidence:** `<fair clues, observations, tests, and possible false diagnoses>`
- **Disclosure changes:** `<event, audience, and resulting knowledge update>`
- **Protected fields:** `<GM Secret references rather than leaked content>`

Hiddenness changes information, not mechanical existence or power.

### Conceptual Skill Extension

Complete only under [Conceptual Skills](../docs/skills/CONCEPTUAL_SKILLS.md).

- **Conceptual Domain:** `<bounded domain>`
- **Conceptual Access:** `<how the holder can engage it>`
- **Interpretation:** `<current model, evidence, culture, and contradictions>`
- **Permitted claims:** `<specific operations rather than a universal verb>`
- **Scope, cost, precision, opposition, context, contradiction, embodiment, authority, and consequence:** `<one bounded entry for each>`
- **Magical expression:** `<separate Magic references where Conceptual Spellcasting exists>`

A conceptual label grants no universal authority over everything that could be described by the same word.

### Reincarnation Crossover Extension

Complete only under [Reincarnation Skill Crossover](../docs/skills/REINCARNATION_SKILL_CROSSOVER.md).

- **Source Skill and incarnation:** `<provenance>`
- **What persisted:** `<Skill Imprint, Skill XP, principle, or other exact source>`
- **Proposed current function:** `<bounded result>`
- **Receiving Route:** `<present body, tree, toolset, Magic, Profession, institution, or other owner>`
- **Compatibility assessment:** `<conceptual, anatomical, sensory, cognitive, environmental, metaphysical, tool, cultural, and training factors>`
- **Crossover outcome:** `<direct | principle | partial | translated | latent | inaccessible | distorted | harmful | impossible>`
- **Translation loss and present work:** `<differences, risks, practice, and evidence still required>`
- **Resulting Skill identity:** `<same Skill, translated Skill, principle support, or no present Skill>`

Reincarnation preserves source history; it does not perform the crossover or bypass current-life training.

## Knowledge, Visibility, and Recognition

### Required Fields

- **World truth:** `<factual Skill state or protected GM Secret reference>`
- **Holder knowledge:** `<known, suspected, mistaken, forgotten, or hidden>`
- **Player knowledge:** `<known, uncertain, theory, or withheld by agreed procedure>`
- **Observer recognition:** `<observer, evidence, interpretation, and uncertainty>`
- **Public or institutional label:** `<name, rank, certification, reputation, and scope>`
- **Misidentification or dispute:** `<claim, source, consequences, and correction route>`

World Recognition may create access, trust, scrutiny, employment, rivalry, or misunderstanding. It does not alter the Skill's underlying capability by itself.

## Persistence and History

### Required Fields

- **Acquisition history:** `<Timeline and evidence references>`
- **Material changes:** `<development, adaptation, Evolution, Fusion, suppression, recovery, or correction references>`
- **Current expression state:** `<current access, embodiment, reliability, and limitations>`
- **Persistent Soul state:** `<Skill Imprint and Skill XP reference, or none>`
- **Historical names and versions:** `<aliases, lineages, and effective intervals>`
- **Supersession and cleanup:** `<records retained, archived, merged, split, or replaced>`

Campaign History records what occurred. The current Skill record states what is presently authoritative. Neither may silently overwrite the other.

## Validation Checklist

- [ ] The repository copy remains blank; populated campaign records are external.
- [ ] The capability passes the Capability Representation and ownership tests.
- [ ] Skill identity, origin tree, purpose, included operations, and exclusions are bounded.
- [ ] A label, rank, title, Class, species, spell, item, Soul Weapon, or reputation grants no unearned capability.
- [ ] Persistent Potential, Current Access, Embodied Expression, Practised Reliability, Contextual Effectiveness, and World Recognition remain separate.
- [ ] Skill XP is persistent familiarity, not currency, a universal score, or present mastery.
- [ ] Current-life practice, feedback, adaptation, embodiment, and requirements remain necessary.
- [ ] Human and monster trees remain distinct unless a valid receiving route owns bounded crossover.
- [ ] Techniques, branches, Specialisations, Composite Skills, and Disciplines are not duplicated as full Skills without evidence.
- [ ] Active, passive, mixed, and internalised expressions retain costs, disruption, limits, and agency.
- [ ] Adaptive emergence, Evolution, Fusion, hiddenness, conceptual operation, and crossover use only their specialist owner when applicable.
- [ ] One adaptation awards Development once under one owner; representation cleanup prevents duplicate stacking.
- [ ] Tools, traits, Magic, Classes, Professions, Soul systems, and world recognition remain linked rather than absorbed.
- [ ] Failure, counters, uncertainty, rust, suppression, and inaccessible mastery remain possible.
- [ ] No universal level, power score, rarity tier, infinite growth, automatic mastery, or unrestricted transfer appears.
- [ ] Truth layers, visibility, provenance, typed references, versions, and validation status are explicit.

## Canonical Dependencies

- [Skill Development](../docs/progression/SKILL_DEVELOPMENT.md)
- [Development System](../docs/progression/README.md)
- [Skill Engine](../docs/skills/README.md)
- [Human Skill Trees](../docs/skills/HUMAN_SKILL_TREES.md)
- [Monster Skill Trees](../docs/skills/MONSTER_SKILL_TREES.md)
- [Active and Passive Skills](../docs/skills/ACTIVE_AND_PASSIVE_SKILLS.md)
- [Adaptive Skills](../docs/skills/ADAPTIVE_SKILLS.md)
- [Skill Evolution](../docs/skills/SKILL_EVOLUTION.md)
- [Skill Fusion](../docs/skills/SKILL_FUSION.md)
- [Skill Consolidation and Historical Scope](../docs/skills/SKILL_CONSOLIDATION_AND_SCOPE.md)
- [Hidden Skills](../docs/skills/HIDDEN_SKILLS.md)
- [Conceptual Skills](../docs/skills/CONCEPTUAL_SKILLS.md)
- [Reincarnation Skill Crossover](../docs/skills/REINCARNATION_SKILL_CROSSOVER.md)
- [Skill Engine Safeguards](../docs/skills/SKILL_ENGINE_SAFEGUARDS.md)
- [Campaign State Model](../docs/persistence/CAMPAIGN_STATE_MODEL.md)
- [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md)

## Extensions and Consumers

Use the [Character Record Template](CHARACTER_TEMPLATE.md) for the holder's current overview and link this Skill record rather than copying its history. Species, Class, Profession, Magic, Soul Weapon, and Evolution records may consume only the fields relevant to their own interfaces.

Capability assessment, training, encounters, research, continuity, and save updates consume the current authoritative record while preserving hidden information, uncertainty, and specialist ownership.
