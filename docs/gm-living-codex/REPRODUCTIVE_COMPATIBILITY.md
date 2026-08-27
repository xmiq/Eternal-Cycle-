# Reproductive Compatibility

## Purpose

This document defines Step 12 of the [GM Living Codex implementation plan](GM_LIVING_CODEX.md#step-12---reproductive-compatibility): the initial reusable data model and operating rules for **Reproductive Compatibility** between species.

It defines only whether a directionally identified species pairing can begin an offspring-forming or equivalent renewal process under stated conditions, and what limited outcome probabilities have actually been established. It does not define lineage outcomes, gestation, broader development procedures, family, or social systems that may follow.

## Document Control

- **Owner:** reusable directional Reproductive Compatibility records, assistance-method records, established viability and fertility claims, and their Living Codex persistence contract
- **Primary authorities:** [GM Living Codex Design](GM_LIVING_CODEX.md), [Species Registry](SPECIES_REGISTRY.md), and [Living Codex Persistence Model](PERSISTENCE_MODEL.md)
- **Dependencies:** stable species identity, actual species renewal architecture, Magic, Souls, Monster Evolution, and GM design review
- **Extensions:** [Lineage and Evolutionary Inheritance](LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md) consumes successful-formation results without changing this scaffold's meaning
- **Consumers:** GM species design, ecosystem and population simulation, campaign configuration, Hybridization adjudication, AI GM operations, and Codex validation
- **Repository boundary:** no actual pairing, individual, result, campaign divergence, current population, or private deployment value belongs in this repository

## Core Principle

Reproductive Compatibility is a **sparse, directional, many-to-many species relationship**.

Every species pairing has a nonzero theoretical reproductive possibility in Eternal Cycle because Magic can bridge biological and metaphysical distance. That principle does not make every pairing naturally practical, commonly available, safe, stable, socially accepted, or likely. A possibility may be arbitrarily small and may require exceptional conditions or extraordinary mediation before it matters in play.

Magic acts through bounded mechanisms such as:

- developmental reconciliation;
- Mana-pattern patching;
- lineage stabilization;
- environmental support;
- Soul-pattern alignment;
- artificial, symbiotic, ritual, or divine mediation.

Magic does not erase biological difference, waive the [Magic](../magic/README.md) rules, grant the relevant Skills or infrastructure, or guarantee a result.

## Scope of the Percentage

A Reproductive Compatibility percentage is one purpose-specific probability for one directional pairing under one recorded baseline or assistance method. It is not:

- a universal compatibility score between species;
- a measure of attraction, consent, willingness, health, social approval, or relationship status;
- a Hybrid Compatibility Profile;
- a Development Compatibility Profile;
- a probability that every later stage succeeds;
- a species power rating;
- an automatic die roll that the GM must make;
- an inheritance distribution.

This scoped probability does not contradict the rule that cross-species compatibility has no categorical or universal answer. Anatomy, Magic, Souls, Development, Hybridization, contact, and tool use continue to assess their own claims for their own functions.

## Sparse Definition

Do not precompute a complete species matrix.

A directional record is created only when the pairing becomes narratively or procedurally relevant, including when:

- the pairing matters during gameplay;
- a lineage needs to be designed;
- a potential descendant or Hybrid Form needs an origin assessment;
- population or ecosystem simulation materially depends on the relationship;
- an Evolution or reusable variant changes the relevant structures;
- the GM explicitly expands the Living Codex.

An absent relationship row means **Not Yet Defined**.

It does not mean:

- zero;
- impossible;
- compatible;
- incompatible;
- probable;
- improbable;
- naturally viable;
- assisted support is known.

The GM must search first, but must not fill unrelated rows merely because one relationship is being considered.

## Nonzero Rule

Every established probability must satisfy:

```text
greater than 0 percent
and
less than or equal to 100 percent
```

Explicit `0%` values and negative values are forbidden. An extraordinarily difficult relationship uses an appropriately small positive value under a clearly stated basis rather than pretending that theoretical possibility is absent.

The constraint applies separately to every stored Natural Compatibility, Assisted Compatibility, Offspring Viability, and Offspring Fertility value. An unknown value remains `NULL` or its logical equivalent; it is never converted to zero.

## Directionality

Compatibility is directional.

`Species A -> Species B` may differ from `Species B -> Species A` because the source and partner roles may affect:

- gestational, host, seeding, or incubation direction;
- anatomy and scale;
- developmental environment;
- Mana architecture;
- Soul anchoring;
- reproductive method and role;
- material contribution or regulatory burden.

The existence of one direction creates no reverse row. If direction is genuinely irrelevant or the relationship is symmetrical, create and validate both directional records with matching values and a shared design basis. Symmetry is an established conclusion, not a schema default.

## Separate Probability Fields

### Natural Compatibility

**Natural Compatibility** is the probability of successful reproductive initiation or equivalent offspring formation for the directional pair under the recorded baseline conditions, without targeted magical, alchemical, divine, artificial, technological, or symbiotic assistance.

Ordinary ambient Magic and the species' own normal renewal structures may be part of the baseline when the record says so. A defined directional relationship requires this field. It may be extremely small, but it cannot be zero.

### Assisted Compatibility

**Assisted Compatibility** is the probability of successful initiation under one specifically identified assistance method and its complete recorded conditions.

It remains undefined until that method is established. Different methods use separate records because a ritual, artificial incubation system, and Soul-alignment practice may have different requirements, mechanisms, risks, and results.

### Offspring Viability

**Offspring Viability** is the probability that an offspring or equivalent formed under the identified baseline or assistance method develops into a viable being under the recorded conditions.

It remains undefined until relevant evidence or GM design establishes it. Successful initiation does not imply viability.

### Offspring Fertility

**Offspring Fertility** is the probability that a resulting viable offspring or equivalent can itself participate in a valid reproductive or renewal process under the recorded scope.

It remains undefined until relevant. Viability does not imply fertility, and fertility does not define what lineage, traits, or Evolution access a descendant expresses.

## Percentages Are Not Guarantees

A stored percentage describes bounded uncertainty under recorded conditions. It does not promise a result for an individual attempt, prove that all prerequisites are present, or authorize the GM to invent missing individual facts.

Actual resolution may also depend on campaign-specific bodies, health, environment, timing, resources, agency, intervention quality, and later reproduction rules. The GM must identify which established conditions are present before applying a value. A percentage does not compel a particular randomization method when campaign procedures use another valid uncertainty process.

## Assistance Classes

Use controlled assistance classes equivalent to:

| Stored value | Meaning |
|---|---|
| `none` | The natural baseline; no targeted assistance method. |
| `ambient_magic` | A naturally or institutionally maintained magical environment supports formation without one targeted intervention. |
| `ritual` | A bounded ritual supplies ordered magical reconciliation. |
| `alchemy` | Alchemical materials or processes alter, stabilize, or support formation. |
| `soul_alignment` | A valid Soul-mediated process aligns otherwise conflicting anchoring or identity patterns. |
| `divine_intervention` | A divine source performs a bounded intervention under its own authority and costs. |
| `artificial_incubation` | Constructed infrastructure supplies a required developmental environment. |
| `evolutionary_adaptation` | An established evolved structure or route supplies compatibility support. |
| `symbiotic_mediation` | One or more symbionts mediate formation, regulation, or development. |
| `reality_alteration` | An established reality-altering source temporarily or persistently changes relevant laws. |
| `mixed` | Several explicitly linked methods are required as one method set. |

Each assisted method records:

- assistance class;
- mechanism and source;
- required conditions and environment;
- relevant Skills, Professions, or institutions;
- required materials, infrastructure, or participants;
- known risks and failure routes;
- which probabilities it affects;
- design basis, revision, and review evidence.

Do not reduce assistance to an unexplained bonus. Assistance cannot supply authority, consent, Skill mastery, infrastructure, materials, or divine cooperation merely because its class is listed.

## Reproductive Distance

**Reproductive Distance** is the qualitative collection of differences relevant to one directional reproductive claim. It is not one universal number or mandatory equation.

When first establishing a pairing, consider:

- evolutionary distance and Source Lineages;
- anatomy, scale, and mass;
- reproductive or renewal mechanism;
- genetic, template, magical, or metaphysical structure;
- Mana architecture and Magical Organs;
- elemental composition;
- Soul structure and anchoring;
- developmental timing;
- gestation, host, or incubation requirements;
- environmental conditions;
- lineage stability;
- inherited-Evolution conflicts;
- species transformations;
- undeath, artificiality, or incorporeality;
- symbiotic dependencies.

Greater biological, anatomical, developmental, magical, elemental, evolutionary, or Soul-level distance generally lowers natural compatibility and later outcome probabilities while increasing exceptional conditions, intervention scale, precision, instability, and risk. This is adjudication guidance, not a formula. A specific Integration Bridge or shared origin may make a distant pairing more workable than superficial similarity suggests.

## Evolutions and Variants

Every Evolved Form and sufficiently distinct [Living Codex Variant](SPECIES_REGISTRY.md#variants) may have its own directional compatibility records.

Do not automatically inherit percentages from:

- a parent species;
- a prior Evolution;
- a regional or environmental variant;
- a campaign divergence;
- a superficially similar species.

Related values may inform the GM's first judgement. The new relationship remains Not Yet Defined until it becomes relevant and receives its own design basis.

A campaign-specific variant may override the Living Codex baseline only inside that campaign. It records the base Codex identity and revision, changed compatibility claim, cause, scope, and campaign authority. It never silently writes the divergence back to the base entry.

## Species and Individual Boundary

Living Codex values belong to species or reusable-variant design. They do not establish any individual's:

- attraction;
- consent;
- willingness;
- relationship status;
- health;
- personal fertility;
- access to assistance;
- guaranteed outcome.

Those are distinct campaign facts or later reproduction-system claims. Reproductive Compatibility never gives one person entitlement to another person's body, participation, care, descendants, or resources.

## Boundary With Hybridization

Reproductive Compatibility asks whether a directional process can begin and, where established, whether an offspring is viable or fertile under stated conditions.

The [Hybrid Compatibility Profile](../monster-evolution/HYBRIDIZATION.md#hybrid-compatibility-profile) asks whether specific Source Lineages can form one coherent body across origin, anatomy, regulation, lifecycle, Magic, cognition, Soul inhabitation, and ecology. It remains qualitative and form-specific.

A successful reproductive initiation may produce:

- no viable offspring;
- an offspring belonging to one established species;
- a later-defined mixed lineage;
- a Hybrid Form requiring its own full profile;
- another outcome owned by [Lineage and Evolutionary Inheritance](LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md).

Step 12 does not choose among these possibilities merely from the source and partner IDs. A high initiation probability cannot replace whole-form integration, Hybrid Stability, inheritance, or Species Potential adjudication.

## Boundary With Development Compatibility

The [Compatibility Profile](../progression/STAT_XP_AND_RETAINED_DEVELOPMENT.md#compatibility-profile) used by retained Development is a qualitative account of how retained familiarity maps into one current embodiment and route. It is not Reproductive Compatibility and receives no species-pair percentage.

Reproductive Compatibility cannot transfer Development, Stat XP, Skill mastery, memory, Soul Titles, Soul Weapons, reputation, or current-life capability to offspring.

## Boundary With Magic and Souls

Magic may bridge distance only through an existing source, route, mechanism, cost, capacity, control requirement, environment, and failure behavior. Similar affinity, abundant Mana, or a powerful caster does not create universal compatibility.

Soul alignment is one possible Assistance Class. It follows Soul integrity, identity, agency, access, and embodiment rules. It cannot merge souls, manufacture a soul, transfer Soul progression, or guarantee a suitable soul for a resulting body.

## Boundary With Population Simulation

The [World Engine population model](../world-engine/POPULATIONS.md#reproduction-and-recruitment) may consume established compatibility, viability, and fertility inputs when they materially affect a population. It still distinguishes:

1. reproductive initiation;
2. successful development;
3. birth, hatching, formation, or equivalent arrival;
4. survival;
5. maturation;
6. social or ecological integration.

Step 12 supplies no population count, growth rate, pair frequency, individual availability, institution, resource, or current outcome. All current values remain Campaign State.

## Normalized SQLite Model

The [Living Codex Database](PERSISTENCE_MODEL.md) must represent compatibility with normalized directional relations. An implementation may use surrogate keys, UUIDs, controlled-value tables, or revision views, but must preserve the following semantics.

### Directional Pair Table

```sql
CREATE TABLE species_reproductive_compatibility (
    compatibility_id TEXT PRIMARY KEY,
    source_species_id TEXT NOT NULL,
    partner_species_id TEXT NOT NULL,
    natural_compatibility_percent REAL NOT NULL
        CHECK (
            natural_compatibility_percent > 0
            AND natural_compatibility_percent <= 100
        ),
    baseline_conditions TEXT NOT NULL,
    design_basis TEXT NOT NULL,
    created_at TEXT NOT NULL,
    last_reviewed_at TEXT NOT NULL,
    UNIQUE (source_species_id, partner_species_id),
    FOREIGN KEY (source_species_id)
        REFERENCES species(species_id),
    FOREIGN KEY (partner_species_id)
        REFERENCES species(species_id)
);
```

The ordered `source_species_id` and `partner_species_id` pair enforces one natural baseline per direction. It does not create the reverse direction.

### Assistance Methods

```sql
CREATE TABLE compatibility_assistance_methods (
    assistance_method_id TEXT PRIMARY KEY,
    compatibility_id TEXT NOT NULL,
    assistance_class TEXT NOT NULL
        CHECK (assistance_class IN (
            'ambient_magic',
            'ritual',
            'alchemy',
            'soul_alignment',
            'divine_intervention',
            'artificial_incubation',
            'evolutionary_adaptation',
            'symbiotic_mediation',
            'reality_alteration',
            'mixed'
        )),
    assisted_compatibility_percent REAL NOT NULL
        CHECK (
            assisted_compatibility_percent > 0
            AND assisted_compatibility_percent <= 100
        ),
    mechanism TEXT NOT NULL,
    required_environment TEXT,
    required_materials_or_infrastructure TEXT,
    relevant_skills_or_institutions TEXT,
    known_risks TEXT,
    affects_compatibility INTEGER NOT NULL
        CHECK (affects_compatibility IN (0, 1)),
    affects_viability INTEGER NOT NULL
        CHECK (affects_viability IN (0, 1)),
    affects_fertility INTEGER NOT NULL
        CHECK (affects_fertility IN (0, 1)),
    design_basis TEXT NOT NULL,
    FOREIGN KEY (compatibility_id)
        REFERENCES species_reproductive_compatibility(compatibility_id)
);
```

The natural pair record represents the `none` class. Assisted methods use one row per established method or indivisible mixed method set. An absent method row means Assisted Compatibility is undefined.

The logical schema should normalize Skills, institutions, materials, risks, and environments into relation tables when they need stable identity or many-to-many reuse. The text columns above show required semantics, not permission to collapse established identities into comma-separated values.

### Conditions

```sql
CREATE TABLE compatibility_conditions (
    condition_id TEXT PRIMARY KEY,
    compatibility_id TEXT NOT NULL,
    assistance_method_id TEXT,
    condition_kind TEXT NOT NULL,
    condition_reference TEXT,
    condition_description TEXT NOT NULL,
    FOREIGN KEY (compatibility_id)
        REFERENCES species_reproductive_compatibility(compatibility_id),
    FOREIGN KEY (assistance_method_id)
        REFERENCES compatibility_assistance_methods(assistance_method_id)
);
```

Conditions may apply to the natural baseline or one assisted method. A condition record cannot silently create a species, Skill, institution, item, or campaign fact.

### Results

```sql
CREATE TABLE compatibility_results (
    result_id TEXT PRIMARY KEY,
    compatibility_id TEXT NOT NULL,
    assistance_method_id TEXT,
    resulting_species_id TEXT,
    offspring_viability_percent REAL
        CHECK (
            offspring_viability_percent IS NULL
            OR (
                offspring_viability_percent > 0
                AND offspring_viability_percent <= 100
            )
        ),
    offspring_fertility_percent REAL
        CHECK (
            offspring_fertility_percent IS NULL
            OR (
                offspring_fertility_percent > 0
                AND offspring_fertility_percent <= 100
            )
        ),
    result_conditions TEXT,
    design_basis TEXT NOT NULL,
    FOREIGN KEY (compatibility_id)
        REFERENCES species_reproductive_compatibility(compatibility_id),
    FOREIGN KEY (assistance_method_id)
        REFERENCES compatibility_assistance_methods(assistance_method_id),
    FOREIGN KEY (resulting_species_id)
        REFERENCES species(species_id)
);
```

`NULL` viability, fertility, or resulting species means Not Yet Defined. It does not mean zero, sterile, nonviable, or unknown species automatically. A natural result uses a null `assistance_method_id`; an assisted result references its exact method.

### Revisions

```sql
CREATE TABLE compatibility_revisions (
    compatibility_revision_id TEXT PRIMARY KEY,
    compatibility_id TEXT NOT NULL,
    parent_revision_id TEXT,
    codex_revision_id TEXT NOT NULL,
    change_basis TEXT NOT NULL,
    affected_records TEXT NOT NULL,
    reviewed_at TEXT NOT NULL,
    FOREIGN KEY (compatibility_id)
        REFERENCES species_reproductive_compatibility(compatibility_id),
    FOREIGN KEY (parent_revision_id)
        REFERENCES compatibility_revisions(compatibility_revision_id),
    FOREIGN KEY (codex_revision_id)
        REFERENCES entry_revisions(revision_id)
);
```

Implementations may normalize the affected set further. Revision identity, parentage, design basis, and traceability are mandatory.

## Schema Invariants

Validation must prove:

- every source and partner species ID exists;
- ordered pairs are unique and directional;
- no automatic reverse row or symmetry inference occurs;
- Natural Compatibility is present and greater than zero;
- every stored assisted, viability, and fertility value is greater than zero and no greater than 100;
- nullable result fields remain distinguishable from zero;
- each assistance method belongs to one directional pair;
- each condition and result references the correct pair and method;
- an Evolved Form or distinct variant uses its own identity and records;
- resulting species IDs exist when established;
- design basis and revision evidence exist;
- no complete matrix, invented result, campaign divergence, or individual fact entered the Codex.

## GM Operating Procedure

When Reproductive Compatibility becomes relevant:

1. load the latest configured canonical Living Codex;
2. search for the exact directional source-to-partner relationship;
3. apply an explicit Campaign Divergence when Campaign Canon owns one;
4. when the relationship is undefined, assess biological, magical, developmental, evolutionary, environmental, and Soul-level distance;
5. decide whether the natural directional relationship now needs definition;
6. define only assistance methods that are presently relevant and supported;
7. define Offspring Viability and Offspring Fertility only when presently relevant and supported;
8. ensure every established probability is greater than zero and no greater than 100;
9. record conditions, uncertainty, design basis, dependencies, and owner references;
10. calculate the complete Codex affected set, including pair, method, condition, result, revision, migration, and index records;
11. create the required full backup and apply the complete affected set in one SQLite transaction;
12. validate foreign keys, probability constraints, directionality, stable references, and expected changes;
13. close and reopen the SQLite candidate read-only, then verify the expected state;
14. replace the canonical Google Drive file and deploy the required current and dated backups;
15. fetch canonical and backup copies, compare hashes or exact contents, and record migration and validation results;
16. continue dependent gameplay only after successful persistence, while keeping storage plumbing backstage unless failure requires Development Context.

If a campaign needs an immediate local ruling but the reusable Codex update is optional, the GM may record an explicit campaign-local divergence through Campaign Persistence and defer Codex promotion. The campaign ruling does not become Living Codex Canon.

## Full-Save Requirement

A compatibility update follows the complete [Living Codex full-save protocol](PERSISTENCE_MODEL.md#full-save-protocol):

```text
fetch latest canonical Codex
  -> create complete backup
  -> begin bounded SQLite transaction
  -> apply pair, methods, conditions, results, revision, and migration records
  -> validate constraints and references
  -> commit
  -> reopen read-only and verify expected state
  -> replace canonical Google Drive database
  -> deploy or replace verified current and dated backups
  -> fetch all required remote copies
  -> compare hashes or exact contents
  -> record migration and validation
```

If any required stage fails:

- preserve the last validated Codex;
- do not claim the update succeeded;
- do not activate partial records;
- do not narrate durable consequences dependent on the failed update;
- report the failed boundary in Development Context;
- recover or use an authorized campaign-local fallback before resuming dependent play.

## Readable Species View

Generated Codex views expose established relationships only.

```markdown
## Established Reproductive Compatibility

| Partner | Direction | Natural | Assisted | Viability | Fertility | Assistance |
|---|---|---:|---:|---:|---:|---|
| `<established partner>` | `<source -> partner>` | `<value>` | `<value or Unknown>` | `<value or Unknown>` | `<value or Unknown>` | `<method or None>` |

«Unlisted pairings have not yet been defined. They are not presumed impossible or incompatible.»
```

A readable view identifies its Codex Version and does not expose protected GM information automatically. Backend values become player-visible only through an established information route.

## Lineage and Inheritance Interface

Reproductive Compatibility determines whether reproduction or equivalent formation can successfully begin. [Lineage and Evolutionary Inheritance](LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md) determines what lineage and Evolution expression may result afterward.

Step 12 reserves interfaces for future consideration of:

- lineage templates;
- mixed lineage;
- pure-line throwbacks;
- Mana equalization;
- inherited Evolution expression;
- born-evolved offspring;
- Level 0 Instinctive Skills where the owning local Skill scale uses that label;
- ancestral hereditary echoes distinct from Soul Echoes;
- inheritance distributions.

None of those systems is implemented here. Step 12 does not define courtship, attraction, consent procedure, pregnancy, gestation, incubation, birth, heredity, hybrid development, family structures, reproductive anatomy, or cultural marriage systems. Step 13 implements only the linked lineage and inherited-development layer, not those broader procedures.

## Safeguards

- Never precompute the complete species matrix.
- Never treat an absent row as zero, impossible, compatible, or likely.
- Never store zero or a negative probability.
- Never treat Magic as automatic success or permission.
- Never treat a percentage as a guarantee or a universal species score.
- Never infer reverse-direction symmetry.
- Never copy a parent species, prior Evolution, variant, or campaign divergence automatically.
- Never invent assisted, viability, fertility, or resulting-species values before they matter.
- Never let a campaign divergence silently overwrite the base Codex.
- Never expose hidden Codex values without a valid information route.
- Never let species data determine individual attraction, consent, willingness, health, or fertility.
- Never replace Hybridization, lineage, inheritance, Skills, Development, Magic, Souls, or population rules.
- Never save a partial compatibility change.
- Never skip canonical and backup deployment, read-back, and exact comparison.
- Never add populated compatibility records, campaign facts, deployment IDs, private URLs, or credentials to this repository.

## Related Documents

- [GM Living Codex Index](README.md)
- [GM Living Codex Design](GM_LIVING_CODEX.md)
- [Species Registry](SPECIES_REGISTRY.md)
- [Living Codex Persistence Model](PERSISTENCE_MODEL.md)
- [Lineage and Evolutionary Inheritance](LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md)
- [Hybridization](../monster-evolution/HYBRIDIZATION.md)
- [Species Development](../progression/SPECIES_DEVELOPMENT.md)
- [Magic](../magic/README.md)
- [Soul Engine](../soul/README.md)
- [World Engine Populations](../world-engine/POPULATIONS.md)
- [Campaign Persistence Engine](../persistence/README.md)
- [AI Save Protocol](../ai/AI_SAVE_PROTOCOL.md)
- [SQLite Database Format Adapter](../persistence/adapters/SQLITE_DATABASE_FORMAT_ADAPTER.md)
- [Google Drive Remote Storage Adapter](../persistence/adapters/GOOGLE_DRIVE_REMOTE_STORAGE_ADAPTER.md)
- [Canonical Terminology](../../design/TERMINOLOGY.md)
