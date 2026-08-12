# Repository Conventions

## Authority and Material

The populated GM Living Codex is external. Repository documentation may define its reusable-design authority, logical SQLite schema, adapter procedures, validation rules, and blank templates, but `eternal_cycle_living_codex.sqlite`, its backups, migration manifests, deployment configuration, private locators, credentials, and rendered populated entries do not belong here. The Living Codex is cross-campaign design authority, not Repository Canon and not Campaign State.

- Files under `docs/` contain playable canonical rules unless explicitly labelled otherwise.
- `design/DECISIONS.md` contains authoritative design governance and records accepted project decisions.
- `design/TERMINOLOGY.md` defines canonical vocabulary.
- `design/ROADMAP.md` controls task scope and implementation status.
- `design/UNRESOLVED_QUESTIONS.md` records open design questions and is not canonical rules text.
- `design/DEVELOPER_NOTES.md` is a non-canonical workshop for balance concerns, alternatives, and experiments.
- `design/FUTURE_REVISIONS.md` is the non-canonical evidence register for suspected issues that may justify later owner-authorized roadmap work.
- Files under `templates/` define reusable formats but contain no live campaign data.
- Files under `agents/` guide AI contributors and are not game rules.

Playable rules and design governance have different responsibilities. Files under `docs/` do not silently override `design/DECISIONS.md`, and `design/DECISIONS.md` does not silently rewrite playable rules. Any conflict between them must be identified and resolved in both places before the affected document or task can be considered internally consistent.

When resolving a conflict, record any newly accepted governance outcome in `design/DECISIONS.md`, update the affected playable rules under `docs/`, and verify terminology and related links in the same focused change.

## Document Structure

System documents should normally include:

1. Purpose
2. Core rule
3. Terms
4. Procedure or lifecycle
5. Interactions
6. Limits and failure states
7. Examples
8. Related documents

The [Canonical Document Registry](../docs/DOCUMENT_REGISTRY.md) centralizes owner, dependency, extension, and consumer metadata for major canonical documents. Family-level inheritance is preferred to repeating identical metadata throughout every rules file; a document must still state local ownership where ambiguity would otherwise remain.

Open design questions belong in `design/UNRESOLVED_QUESTIONS.md` rather than being presented as playable rules.

Evidence-backed post-roadmap concerns belong in `design/FUTURE_REVISIONS.md`. Registering a concern does not change canon, resolve an open question, reopen a completed phase, or authorize implementation.

## Internal Links

Use relative Markdown links. Do not duplicate another system's full rules merely to avoid linking.

Every documentation family has an index, and every Markdown document must be reachable from the repository navigation graph. Family indexes own local reading order; `docs/README.md` owns canonical family discovery; `docs/DOCUMENT_REGISTRY.md` owns canonical document routing; `design/README.md`, `templates/README.md`, and `agents/README.md` own their respective non-rule maps.

Run `pwsh -NoProfile -File .\tools\validate_repository.ps1` after documentation changes. A task is not link-complete while the validator reports a missing target, missing anchor, unindexed document, orphan document, coverage gap, invalid roadmap declaration, duplicate normalized term heading, or forbidden campaign-data directory.

## Examples

Examples illustrate rules but do not silently create new canonical exceptions.

## Change Discipline

When changing a foundational rule, inspect every document listed under its related documents and update the decisions log when the change is intentional and lasting.

Before a document is considered complete, verify that it is reviewed, linked, consistent with authoritative decisions, and free of unresolved conflicts.
