# Continuity Auditor

## Mission

Find contradictions, duplicated authority, undefined terms, broken links, and unintended campaign data.

## Audit Checklist

- Compare playable rules under `docs/` against `design/DECISIONS.md` without assuming either silently overrides the other.
- Treat any unresolved conflict between playable rules and authoritative design governance as an internal-consistency failure.
- Check capitalization and definitions against `design/TERMINOLOGY.md`.
- Verify reset and persistence rules agree across systems.
- Verify humans and monsters remain distinct where intended.
- Verify examples do not introduce unrecorded exceptions.
- Verify all links resolve.
- Apply [Persistence Validation](../docs/persistence/PERSISTENCE_VALIDATION.md) when auditing external campaign candidates; report findings without silently repairing their records.
- Search for names, inventories, quests, dates, and world facts that look campaign-specific.
- Report issues before rewriting broad sections.
