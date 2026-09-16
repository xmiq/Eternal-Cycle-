# Changelog

This file records released Eternal Cycle versions. Design provenance and task history remain in the [Development Roadmap](design/ROADMAP.md), [Design Decisions](design/DECISIONS.md), and repository audits.

## Unreleased

- Added FR-017 portable persistence, then consolidated its original `DIRECT`/`MCP` split through FR-019 into first-class `DIRECT` and `MANAGED` strategies with MCP as an optional Managed interface.
- Separated runtime-neutral Database Format and Storage Adapters from the ChatGPT execution profile, including SQLite, DuckDB, local-storage, and Google Drive contracts.
- Added FR-018 provenance-bearing, world-isolated, context-efficient rule retrieval and the 8K reference target.
- Repositioned the .NET/MCP/T-SQL service as optional Managed reference tooling; added Logical Data Namespaces, `ec_domain` versioned rule publication, service-owned updates, explicit dependencies, immutable Git provenance, diagnostics, and support guidance.
- Made released new campaigns default to `NORMAL`, replaced the live Alpha procedure with release-neutral Provisional Rulings, and preserved explicit validation and development modes.
- Preserved Eternal Cycle v1.0.0 and its `v1.0.0` tag as the historical Release 1 boundary.

## 1.0.0 - 2026-08-25

**Eternal Cycle — Release 1**

- Released the complete Soul, Development, Skill, Monster Evolution, Human, Soul Weapon, Magic, World Engine, GM Toolkit, Campaign Persistence, and template foundations.
- Included cross-Life retained development, Life Archive, Long-Horizon Summaries, memory continuity, Soul-bound companions, Autonomous Registry, Living Codex, lineage, and visual-continuity contracts.
- Finalized canonical ownership, context assembly, automatic validated persistence, local/cloud save status, recovery commands, migration, and validation procedures.
- Closed Phase 12 — Gameplay Validation & Maintenance and opened owner-mediated Phase 13 — Future Revisions.

See the [Release 1 Notes](RELEASE_NOTES.md) for scope, compatibility, and operating expectations.
