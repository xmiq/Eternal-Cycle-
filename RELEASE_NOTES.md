# Eternal Cycle v1.0.0 — Release 1

**Release date:** 2026-08-25

**Status:** Released

Release 1 is the first maintainer-approved release of the reusable Eternal Cycle engine. It contains rules, procedures, design governance, validators, and blank templates only; campaigns and populated saves remain external.

## Major Systems

- **Souls and Reincarnation:** persistent Soul identity, Final Death, Interlife, Soul Depth, Resonance, Echoes, Titles, retained instincts, Soul Avatars, and reincarnation continuity.
- **Development and Skills:** multidimensional embodied Development, distinct human and monster routes, Adaptive Skills, bounded retained development, cross-embodiment familiarity, consolidation, and history-based scope.
- **Species and Evolution:** ecological monster identity, branching Evolution, variants, hidden conditions, mutations, player species, and safeguards against automatic or farmed advancement.
- **Human paths and Soul Weapons:** Classes, Professions, institutions, emergent Weapon Souls, Soul Weapon partnership, evolution, manifestations, legacy, and optionality.
- **Magic and world simulation:** embodied Mana use, affinities, spells, rituals, enchanting, alchemy, divine and forbidden magic, ecology, factions, history, stability, Gates, and causal long-horizon simulation.
- **GM operation:** adjudication, uncertainty, consequence management, world simulation, generators, alpha-playtest guidance, and equal support for human and AI GMs.
- **GM Living Codex:** cross-campaign reusable species designs, stable identity, Evolution graphs, variants, reproductive compatibility, lineage, inheritance, dedicated persistence, and verified deployment procedures.
- **Campaign persistence:** authority and truth layers, structured state, Relationships, Research, Timeline, migration, continuity resolution, canonical ownership, Life Archive, Long-Horizon Summaries, Autonomous Registry, and visual identity.
- **AI runtime operation:** authoritative context assembly, mandatory reads, automatic affected-set persistence, local/cloud save evidence, `save`, `save status`, `retry save`, next-turn reload, and canonical visual context.
- **Continuity:** imperfect memory across Lives, persistent Relationships, autonomous identity, Soul-bound companion reunion, historical durability, uncertain Research, infrastructure, unresolved mysteries, and strict Canon, Character Knowledge, and GM Secret separation.

## Persistence Expectations

Conversation and model memory are supplementary and non-authoritative. A runtime reads relevant Canon before resolution, writes each changed fact through its authoritative owner, validates the configured local or cloud authority, and only then completes a state-changing turn. Derived summaries and context packets never replace canonical state.

Repository documentation defines the required contracts but cannot physically force an external host or connector to perform I/O. A runtime may claim saved state only from actual adapter evidence; absent or failed capabilities must be disclosed according to the AI runtime and persistence rules.

## Compatibility and Migration

Existing pre-v1 campaigns do not require a reset. Adopt Release 1 through the established `Backup -> Audit -> Merge -> Validation` process, preserve stable identities and provenance, update only affected records, and activate the target campaign version only after validation. The repository contains no populated campaign migration or save.

## Post-Release Development

Gameplay-derived work now enters [Phase 13 — Future Revisions](design/ROADMAP.md#phase-13--future-revisions). Evidence may be recorded in the [Future Revisions register](design/FUTURE_REVISIONS.md), but no candidate changes released mechanics without maintainer authorization.

## Related Documents

- [Release Manifest](RELEASE_MANIFEST.md)
- [Release 1 Audit](design/audits/RELEASE_1_AUDIT.md)
- [Changelog](CHANGELOG.md)
- [Migration and Versioning](docs/persistence/MIGRATION_AND_VERSIONING.md)
- [Repository Validation](tools/validate_repository.ps1)
