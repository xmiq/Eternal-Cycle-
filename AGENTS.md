# Eternal Cycle Repository Instructions

## Repository Purpose

This repository contains only the canonical rules, systems, templates, design documentation, and GM framework for **Eternal Cycle**.

Do not add campaign saves, current characters, current world state, inventories, active quests, story summaries, or playthrough-specific history.

## Required Workflow

Before making changes:

1. Read `design/ROADMAP.md`.
2. Read `design/DECISIONS.md`.
3. Read `design/TERMINOLOGY.md`.
4. Read `design/REPOSITORY_CONVENTIONS.md`.
5. Read `design/FUTURE_REVISIONS.md` when evaluating playtest evidence, balance concerns, or proposed post-roadmap work.
6. Identify the exact current task named in the roadmap.
7. If that task is marked `[!]`, consult `design/UNRESOLVED_QUESTIONS.md` and do not proceed until the blocking question is resolved or the project owner explicitly changes the target.
8. Inspect the relevant existing documents.
9. Work only on the named current task unless the project owner explicitly changes it.

After making changes:

1. Update the task status in `design/ROADMAP.md`.
2. Record new canonical decisions in `design/DECISIONS.md`.
3. Record unresolved design questions in `design/UNRESOLVED_QUESTIONS.md`.
4. Record balance concerns, alternatives, and experiments in `design/DEVELOPER_NOTES.md`.
5. Record evidence-backed candidates for later design work in `design/FUTURE_REVISIONS.md` without treating them as canon or roadmap authorization.
6. Check terminology, authority boundaries, internal links, and scope boundaries.
7. Review the full diff.
8. Commit only files related to the selected task.

## Status Rules

- `[ ]` — Not started.
- `[~]` — Intentionally started and substantially implemented.
- `[x]` — Complete, reviewed, linked, and internally consistent.
- `[!]` — Blocked by an unresolved design decision.

A passing mention, dependency, constraint, placeholder, or reference in another file does not make a task `[~]`. Never mark a task complete merely because a file exists.

## Scope Discipline

- Complete only the exact current roadmap task per commit unless the project owner explicitly targets a tightly coupled task group.
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
