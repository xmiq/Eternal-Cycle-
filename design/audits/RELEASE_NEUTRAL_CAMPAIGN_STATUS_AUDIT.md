# Release-Neutral Campaign Status Maintenance Audit

## Scope

This audit records the post-v1 Phase 13 maintenance that separates current campaign operation from Eternal Cycle's historical Alpha and playtest period. It changes no gameplay mechanic, campaign state, released `v1.0.0` history, or Future Revision identifier.

## Cause

The live GM owner was still named `ALPHA_PLAYTEST_RULES.md` and mixed provisional adjudication with pre-release readiness, scope, and campaign-mode guidance. Many current operational documents linked to that filename. A fresh runtime could therefore mistake historical lifecycle language for present authority and label an ordinary released campaign as a test campaign.

## Changes

- Replaced the live Alpha owner with [Provisional Rulings](../../docs/gm/PROVISIONAL_RULINGS.md).
- Added [Campaign Bootstrap](../../docs/gm/CAMPAIGN_BOOTSTRAP.md) as the owner of Engine Status lookup, new-versus-resume classification, Campaign Mode default, and legacy-mode interpretation.
- Established `NORMAL` as the only implicit Campaign Mode for a released new campaign.
- Kept `VALIDATION` and `DEVELOPMENT` as explicit modes.
- Kept First-Life Mode as an independent Soul-continuity setup profile.
- Made Campaign Canon the Campaign Mode owner and the Save Index a reference only.
- Updated current GM, AI, template, registry, terminology, and navigation surfaces.
- Added a focused regression harness to the full repository validator.

Using a Provisional Rule does not alter Campaign Mode. Current version, release, root-entry, and roadmap metadata determine Engine Status; historical filenames and audits do not.

## Historical Preservation

Historical roadmap tasks, decisions, developer notes, release notes, audits, and changelog statements retain accurate references to the pre-release Alpha and playtest period. Explicit playtest, validation, regression, development, evidence-gathering, and feedback procedures also remain available.

Legacy metadata preserves explicit testing intent. A generic pre-release Alpha label is historical provenance rather than a current mode declaration; ambiguity remains Unknown pending source recovery.

## Occurrence Classification

A case-insensitive pre-audit scan of `alpha`, `playtest`, `test campaign`, and `alpha mode` classified the repository as follows:

- **Historical or compatibility references retained:** 106 occurrences.
- **Explicit testing-mode or evidence references retained:** 41 occurrences.
- **Stale operational wording removed or rewritten in the maintenance diff:** 159 occurrences.

The first two counts describe the post-cleanup tree before this audit file was added. The corrected count is taken from removed matching text in the maintenance diff. Counts classify wording, not gameplay rules.

## Validation

- The focused campaign-mode harness passed 17 assertions.
- The obsolete live filename is absent and no Markdown link references it.
- Current operational surfaces contain none of the targeted stale readiness, scope, or Alpha-mode phrases.
- Normal start, explicit validation, provisional adjudication, historical-audit isolation, First-Life Mode, and resume cases are covered.
- `VERSION` remains `1.0.0`; Release 1 and the `v1.0.0` historical boundary are unchanged.
- Phase 13 remains active, no `FR-018` was created, and Future Revisions remains `[∞]`.
- No campaign data or schema migration was added.

## Result

Current campaign operation is release-neutral and explicit: released new campaigns default to `NORMAL`, testing requires authorization, and historical Alpha evidence remains historical.

## Related Documents

- [Roadmap](../ROADMAP.md)
- [Decisions](../DECISIONS.md)
- [Terminology](../TERMINOLOGY.md)
- [Provisional Rulings](../../docs/gm/PROVISIONAL_RULINGS.md)
- [Campaign Bootstrap](../../docs/gm/CAMPAIGN_BOOTSTRAP.md)
- [AI Runtime Model](../../docs/ai/AI_RUNTIME_MODEL.md)
