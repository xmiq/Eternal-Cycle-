# Future Revisions

This register records evidence-driven issues that may require design work after Phase 11. It is non-canonical design tracking: an entry does not change a playable rule, authorize implementation, reopen a completed phase, or establish a roadmap task.

## Authority and Ownership

- **Owner:** the project owner governs admission, priority, promotion, closure, and removal of entries.
- **Dependencies:** [Design Decisions](DECISIONS.md), [Developer Notes](DEVELOPER_NOTES.md), [Unresolved Questions](UNRESOLVED_QUESTIONS.md), and the canonical documents named by each entry.
- **Extensions:** evidence reports, approved future roadmaps, and focused design changes may extend an entry without making the register canonical rules text.
- **Consumers:** project maintainers, playtest reviewers, rules auditors, and future roadmap authors.

[D-1174](DECISIONS.md#d-1174--future-revisions-require-evidence-before-canon) governs this register. Files under `docs/` and accepted decisions remain authoritative within their respective responsibilities.

## Register Boundary

Use this file for a suspected usability, balance, coherence, or maintainability issue that needs gameplay evidence before design work is authorized.

Gameplay does not write to this register directly. A gameplay GM may surface an observation or preserve campaign-local evidence, but only the project owner may mediate that material into a Future Revision entry, change its priority or status, merge or remove it, or authorize promotion into roadmap work. Development agents must not infer authorization from gameplay text alone.

Use [Unresolved Questions](UNRESOLVED_QUESTIONS.md) when an unanswered question blocks or materially shapes current roadmap work. Use [Developer Notes](DEVELOPER_NOTES.md) for exploratory alternatives, observations, and workshop material that have not qualified for this register. Use the [Roadmap](ROADMAP.md) only after the project owner authorizes implementation.

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

### FR-001 - Retained Stat XP Pace and Embodiment Relevance

- **Status:** Candidate
- **Issue:** The qualitative pace of redevelopment may prove either too weak to make retained Stat XP meaningful or so strong that new embodiment and current-life effort lose importance.
- **Affected systems:** Development System, retained Stat XP, Reincarnation, embodiment, training, and capability assessment.
- **Gameplay impact:** Poor calibration could make reincarnation feel unrewarding or turn later lives into automatic rebuilds.
- **Evidence needed:** Comparative records of early-, middle-, and old-soul redevelopment across suitable and unsuitable bodies, including practice time, plateaus, access limits, and contextual effectiveness.
- **Suggested future phase:** Development calibration and reincarnation progression review.
- **Priority:** High
- **Related open question:** [Retained Stat XP acceleration](UNRESOLVED_QUESTIONS.md#non-blocking).

### FR-002 - Reincarnation Candidate Weighting and Meaningful Death

- **Status:** Candidate
- **Issue:** Repeated species choices and the circumstances of Final Death may need bounded influence on candidate formation, but any influence could become a deterministic route or farmable bonus.
- **Affected systems:** Reincarnation, candidate generation, Final Death, Soul identity, and GM procedures.
- **Gameplay impact:** An unclear relationship may weaken continuity between lives; an overstrong relationship may reward engineered death or collapse meaningful choice.
- **Evidence needed:** Candidate-generation outcomes across repeated species families and varied death circumstances, including player expectations, attempted exploitation, and world-basis constraints.
- **Suggested future phase:** Reincarnation candidate calibration.
- **Priority:** Medium
- **Related open questions:** [Species-family weighting and Final Death influence](UNRESOLVED_QUESTIONS.md#non-blocking).

### FR-003 - Soul Depth Information Visibility

- **Status:** Candidate
- **Issue:** Depth Horizons may be too opaque to support informed play or too explicit to preserve discovery and in-world interpretation.
- **Affected systems:** Soul Depth, Soul Resonance, information views, uncertainty handling, and campaign presentation.
- **Gameplay impact:** The wrong disclosure pattern could produce arbitrary-feeling access changes or turn qualitative Soul growth into a visible progression ladder.
- **Evidence needed:** Playtests using direct disclosure, in-world signs, and tone-dependent disclosure, measuring comprehension, mystery, planning, and metagaming pressure.
- **Suggested future phase:** Soul information and presentation review.
- **Priority:** Medium
- **Related open question:** [Depth Horizon visibility](UNRESOLVED_QUESTIONS.md#non-blocking).

### FR-004 - Old-Soul Option and Record Load

- **Status:** Candidate
- **Issue:** Accumulated Soul systems, histories, retained routes, and cross-life relationships may create excessive choice and record-management burden in very old campaigns.
- **Affected systems:** Soul Engine, Campaign Persistence Engine, Reincarnation generation, templates, and GM Toolkit.
- **Gameplay impact:** Important identity and continuity may become difficult to retrieve, while minor options crowd out present-life priorities.
- **Evidence needed:** Long-horizon campaign tests or representative stress tests measuring preparation time, retrieval failures, option overload, and which summaries preserve meaningful distinctions.
- **Suggested future phase:** Long-horizon Soul and persistence usability review.
- **Priority:** High

### FR-005 - Human and Monster Crossover Distinction

- **Status:** Candidate
- **Issue:** Reincarnation crossover and Direct Transfer may be misapplied in ways that erase human-versus-monster receiving routes or appear to grant instant mastery.
- **Affected systems:** Skill crossover, human Skills, monster Skills, Retained Instincts, species development, embodiment, and Reincarnation.
- **Gameplay impact:** Miscalibration could make species and present bodies cosmetic or make retained history feel unusably constrained.
- **Evidence needed:** Cross-species recovery cases documenting receiving routes, translation costs, practice, failed expressions, and whether players and GMs preserve tree distinctions consistently.
- **Suggested future phase:** Cross-species progression usability and calibration review.
- **Priority:** High

### FR-006 - Adaptive Skill Granularity and Harmful Incentives

- **Status:** Candidate
- **Issue:** Adaptive Skill adjudication may create either many narrow permanent Skills or incentives to seek injury, abuse, and artificial hardship as an optimal formation route.
- **Affected systems:** Adaptive Skills, Capability Representation, Development, hardship, injury, and Skill safeguards.
- **Gameplay impact:** Skill lists may become noisy, or harmful repetition may be treated as more efficient than meaningful practice and adaptation.
- **Evidence needed:** Formation attempts across training, experimentation, environmental pressure, and harmful conditions, including rejected candidates and Representation Cleanup outcomes.
- **Suggested future phase:** Adaptive Skill playability and safeguard review.
- **Priority:** High

### FR-007 - Conceptual Skill Rarity and Legibility

- **Status:** Candidate
- **Issue:** Conceptual Skills may prove too easy to claim, too difficult to recognize, or too abstract for consistent counterplay.
- **Affected systems:** Conceptual Skills, mastery, evidence, information views, and consequence resolution.
- **Gameplay impact:** Broad conceptual claims could overshadow ordinary expertise, while excessive opacity could make earned mastery unusable.
- **Evidence needed:** Qualification and use cases from different domains, with provenance, limits, counters, failure, observer understanding, and rejected claims.
- **Suggested future phase:** Advanced Skill validation review.
- **Priority:** Medium

### FR-008 - Soul Weapons and Ordinary Equipment Relevance

- **Status:** Candidate
- **Issue:** Soul Weapon partnership may unintentionally make ordinary tools, replacement equipment, crafting, logistics, and non-partner combat paths feel secondary.
- **Affected systems:** Soul Weapons, equipment, Professions, Skills, embodiment, Magic, and resource logistics.
- **Gameplay impact:** A major optional relationship could become functionally mandatory or narrow equipment choices over long play.
- **Evidence needed:** Campaign comparisons with no Soul Weapon, an inaccessible or unsuitable Soul Weapon, unconventional partners, and ordinary equipment specialists.
- **Suggested future phase:** Soul Weapon option and equipment ecology review.
- **Priority:** Medium

### FR-009 - World-Gate Channel and Destination Usability

- **Status:** Candidate
- **Issue:** Separate World-Gate Channels and cross-domain candidate reach may be difficult to adjudicate without either excessive bookkeeping or treating every contacted destination as selectable.
- **Affected systems:** World Gates, world-contact events, Reincarnation, Soul Avatars, information views, and GM procedures.
- **Gameplay impact:** Contact may become mechanically blurry, administratively heavy, or an unrestricted travel and reincarnation menu.
- **Evidence needed:** Contact events with asymmetric channels, closures, stranded actors, incompatible routes, and candidate searches that include and exclude contacted domains for established reasons.
- **Suggested future phase:** World-contact usability review.
- **Priority:** Medium

### FR-010 - Long-Horizon Simulation Compression

- **Status:** Candidate
- **Issue:** Regional and epochal simulation may still demand too much bookkeeping or may compress away player agency, causal thresholds, and material exceptions.
- **Affected systems:** Simulation Abstraction, Time Skips, Age transitions, World Engine, Campaign Persistence Engine, and GM Toolkit.
- **Gameplay impact:** Long campaigns may become impractical to run or may produce unsupported historical summaries.
- **Evidence needed:** Multi-decade and century-scale tests recording preparation time, Review Point effectiveness, missed dependencies, later corrections, and player decisions preserved or lost.
- **Suggested future phase:** Long-horizon simulation and chronology review.
- **Priority:** High

### FR-011 - GM and AI Operating Burden

- **Status:** Candidate
- **Issue:** Authority checks, Read Sets, specialist handoffs, validation, and save procedures may consume more time than the play they protect.
- **Affected systems:** GM Toolkit, AI operating procedures, Campaign Persistence Engine, templates, and repository navigation.
- **Gameplay impact:** Correct operation may be abandoned, inconsistently applied, or become inaccessible to smaller campaigns and human GMs.
- **Evidence needed:** Session-start, adjudication, save, correction, migration, and handoff timing from human and AI-assisted play at several campaign sizes, including skipped steps and resulting defects.
- **Suggested future phase:** GM and persistence workflow usability review.
- **Priority:** High

### FR-012 - Template Usability and Record Duplication

- **Status:** Candidate
- **Issue:** Broad template coverage may cause overlapping records, unclear write ownership, or excessive maintenance when instantiated in real campaigns.
- **Affected systems:** Templates, structured persistence architecture, Save Update Protocol, validation, and campaign operations.
- **Gameplay impact:** Users may duplicate facts, update the wrong record, or omit persistence because the record set is too costly to maintain.
- **Evidence needed:** Storage-neutral campaign implementations using the full and reduced template sets, tracking duplicate claims, broken references, update time, unused fields, and validation findings.
- **Suggested future phase:** Persistence template usability review.
- **Priority:** High

### FR-014 - Autonomous Registry

- **Status:** Candidate
- **Issue:** Independently operating campaign entities and systems may be scattered across infrastructure, inventory, relationships, world state, and ad hoc records, making identity, assignment, memory continuity, network state, maintenance, and autonomous action difficult to preserve consistently.
- **Affected systems:** Campaign Persistence Engine, GM Toolkit, World Engine, AI Save Protocol, SQLite persistence, Google Drive persistence, relationships, infrastructure, summons, familiars, undead, constructs, remote bodies, and autonomous infrastructure.
- **Gameplay impact:** Autonomous units may be forgotten, duplicated, treated as models rather than persistent identities, lose assignments or network relationships, or be rebuilt without a clear distinction between restoration and replacement.
- **Evidence needed:** Owner-mediated gameplay evidence involving independently acting units or systems whose identities, groups, models, memory continuity, condition, assignments, networks, maintenance needs, upgrade lineage, or personhood state cannot be represented cleanly in the current structured save.
- **Suggested future phase:** Campaign Persistence Engine extension — Autonomous Registry.
- **Priority:** High
- **Status reason:** Owner identified the need through gameplay but has not yet promoted it into implementation work.

### FR-015 - Entity Knowledge, Controller, and Perspective Separation

- **Status:** Candidate
- **Issue:** The current engine separates truth layers but may need a more explicit reusable model for entity-specific knowledge, controller assignment, and active perspectives so objective state is not conflated with who controls, observes, remembers, or believes it.
- **Affected systems:** Simulation architecture, Campaign Persistence Engine, GM Toolkit, AI Runtime, relationships, uncertainty handling, player interface, remote bodies, possession, delegated control, and possible future multi-perspective play.
- **Gameplay impact:** Knowledge may leak between actors, controller changes may accidentally reset identity, companions may be treated as extensions of the player, and future remote-body or delegated-control mechanics may require ad hoc schema changes.
- **Evidence needed:** Owner-mediated gameplay cases involving contradictory beliefs, controller changes, remote or delegated bodies, independent companions, hidden information, or multiple valid perspectives on the same objective state.
- **Suggested future phase:** Knowledge and perspective-model review after the core Simulation Architecture step.
- **Priority:** High
- **Status reason:** The architectural need is plausible and partially overlaps the approved Simulation Architecture step, but a dedicated Knowledge System is not yet authorized.

## Roadmapped

No Future Revision entry is currently roadmapped.

The Autonomous Registry and dedicated Knowledge System remain Future Revision candidates and are not authorized for implementation.

## Closed

### FR-013 - Lineage and Evolutionary Inheritance

- **Status:** Closed
- **Issue:** Reproductive Compatibility established a bounded formation scaffold without reusable rules for resulting lineage and inherited Evolution expression.
- **Affected systems:** GM Living Codex, Species Registry, Reproductive Compatibility, Species Development, Monster Evolution, Hybridization, Skills, Magic, Souls, World Engine populations, and Campaign Persistence.
- **Gameplay impact:** Ad hoc rulings risked inherited mastery, strongest-trait copying, blurred species identity, and inconsistent descendant architecture.
- **Evidence needed:** Gameplay cases involving mixed lineage, throwbacks, Mana equalization, inherited Evolution expression, Born-Evolved outcomes, Level 0 Instincts, Ancestral Echoes, and inheritance distributions.
- **Suggested future phase:** Lineage, inheritance, and descendant-expression design.
- **Priority:** High
- **Status reason:** Implemented and validated as GM Living Codex Step 13.
- **Authorized roadmap link:** [Gameplay-Validation Maintenance Sequence](ROADMAP.md#gameplay-validation-maintenance-sequence)
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
