# Eternal Cycle Repository Instructions

## Repository Purpose

This repository contains only the canonical rules, systems, templates, design documentation, and GM framework for **Eternal Cycle**.

Do not add campaign saves, current characters, current world state, inventories, active quests, story summaries, or playthrough-specific history.

## Required Workflow

Before making changes:

1. Read `design/ROADMAP.md`.
2. Read `design/DECISIONS.md`.
3. Read `design/TERMINOLOGY.md`.
4. Inspect the relevant existing documents.
5. Select the earliest incomplete roadmap task that matches the user's request.
6. Do not begin unrelated roadmap phases unless explicitly instructed.

After making changes:

1. Update the task status in `design/ROADMAP.md`.
2. Record new canonical decisions in `design/DECISIONS.md`.
3. Record uncertain ideas in `design/DEVELOPER_NOTES.md`.
4. Check terminology, internal links, and scope boundaries.
5. Review the full diff.
6. Commit only files related to the selected task.

## Status Rules

- `[ ]` Not started.
- `[~]` In progress, incomplete, or contains placeholders.
- `[x]` Complete and internally consistent.
- `[!]` Blocked by a design decision.

Never mark a task complete merely because a file exists.

## Scope Discipline

- Complete one roadmap task, or one tightly coupled task group, per commit.
- Do not silently invent major systems.
- Do not rewrite unrelated files for style.
- Preserve distinctions between rules, examples, templates, and campaign data.
- Prefer links and references over duplicating rules across multiple files.
- Keep the repository usable without relying on chat history.

## Canonical Design Principles

- Development replaces a universal character level as the primary progression model.
- Reincarnation preserves soul-level progression but resets bodily and temporal assets.
- Humans and monsters normally have different progression trees.
- Reincarnation may eventually permit carefully limited progression crossover.
- Monster reincarnation and branching evolution are major focuses.
- World events emerge from interacting systems and consequences.
- No single strategy should be universally optimal.
- Death remains meaningful.
- Progress feels earned.
- Every major mechanic interacts with at least one other system.

## Agent Roles

Role instructions are stored in `agents/`. Use the narrowest applicable role. The roadmap keeper remains active in every task.

## Commit Convention

Use concise conventional messages, for example:

- `docs: define reincarnation foundations`
- `docs: establish soul resonance rules`
- `design: record monster evolution decisions`
- `chore: add repository validation`

Every commit must leave the repository internally consistent.
