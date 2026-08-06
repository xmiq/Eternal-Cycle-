# GM Living Codex

The GM Living Codex is Eternal Cycle's persistent, cross-campaign library of reusable GM-approved design assets. Its first and primary module is the Species Registry. It preserves useful species, variants, evolution branches, characteristic capability routes, and ecological designs without turning them into one campaign's truth.

Repository-wide ownership, dependency, extension, and consumer metadata is maintained in the [Canonical Document Registry](../DOCUMENT_REGISTRY.md#gm-living-codex).

## Authority

Use this design-authority order:

```text
Eternal Cycle Rules Repository
  -> GM Living Codex
  -> Campaign Configuration
  -> Campaign Canon
```

The rules repository owns fictional mechanics. The Living Codex owns reusable GM-approved designs. Campaign Configuration selects, disables, or adapts those designs, and Campaign Canon records what is actually true in one campaign. An explicit campaign divergence may differ from a Codex entry without changing that entry.

The Living Codex is not player knowledge, a campaign save, a bestiary that every world must use, or a source of authority over the rules it applies.

## Reading Order

1. [GM Living Codex Design](GM_LIVING_CODEX.md) - purpose, authority, inclusion, consultation, campaign divergence, safeguards, and the approved twelve-step implementation plan.
2. [Species Registry](SPECIES_REGISTRY.md) - stable identity, species records, trait and Skill ownership, Evolution graphs, variants, procedural reuse, player species, and validation.

The persistence model and Reproductive Compatibility documents extend this index as their dedicated implementation steps are completed. Every Codex document remains subordinate to existing Species Development, Monster Evolution, Skill, Magic, Soul, World Engine, GM, and persistence owners.

## Operational Boundary

The repository stores the Codex design, logical schemas, procedures, and blank templates only. A populated Living Codex database, its backups, migration manifests, deployment configuration, and rendered species entries remain outside this repository.

## Related Documents

- [Game Master Rules](../gm/README.md)
- [Monster Evolution Rules](../monster-evolution/README.md)
- [Species Development](../progression/SPECIES_DEVELOPMENT.md)
- [Skill Engine](../skills/README.md)
- [Magic Rules](../magic/README.md)
- [Campaign Persistence Engine](../persistence/README.md)
- [AI Game Master Operations](../ai/README.md)
