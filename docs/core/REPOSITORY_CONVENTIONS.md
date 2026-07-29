# Repository Conventions

## Canonical and Non-Canonical Material

- Files under `docs/` are canonical rules unless explicitly labelled otherwise.
- Files under `design/` record process, decisions, terminology, and unresolved ideas.
- Files under `templates/` define reusable formats but contain no live campaign data.
- Files under `agents/` guide AI contributors and are not game rules.

## Document Structure

System documents should normally include:

1. Purpose
2. Core rule
3. Terms
4. Procedure or lifecycle
5. Interactions
6. Limits and failure states
7. Examples
8. Open questions, only when the file is explicitly incomplete
9. Related documents

## Internal Links

Use relative Markdown links. Do not duplicate another system's full rules merely to avoid linking.

## Examples

Examples illustrate rules but do not silently create new canonical exceptions.

## Change Discipline

When changing a foundational rule, inspect every document listed under its related documents and update the decisions log when the change is intentional and lasting.
