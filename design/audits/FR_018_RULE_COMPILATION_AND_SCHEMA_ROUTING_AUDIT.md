# FR-018 Rule Compilation and Schema Routing Audit

## Scope

This audit records the owner-authorized Phase 13 implementation of **FR-018 — Rule Compilation and Context-Efficient Retrieval** and the required correction to FR-017 SQL Server schema routing. Work began from `efab550` on `main`; Eternal Cycle v1.0.0 and tag `v1.0.0` remain historical and unchanged.

No campaign save, populated database, private locator, credential, current character, live world state, or gameplay history entered the repository.

## Pre-existing Uncommitted Changes

### What Was Found

Initial `git status` showed exactly one untracked directory: `docs/rules/`. It contained:

- `INDEX.md`, which declared an `EternalCycle.ec_domain.config` SQL key/value table authoritative over repository rules and supplied raw query examples;
- `persistence-rules.md`, which deprecated canonical Markdown persistence rules in favor of that same table.

No tracked-file diff, staged change, or other uncommitted artifact existed.

### Intended Change

The local AI was attempting to make rules queryable for a smaller local runtime and to avoid loading a large Markdown corpus repeatedly. That retrieval objective was valid and directly informed FR-018.

### Retained

The useful intent was retained as:

- a dedicated `docs/rules/` family;
- a compact Runtime Rule Kernel;
- a manifest-driven rule compiler and derived index;
- operation, topic, Campaign Mode, optional-module, and World/Ruleset metadata;
- an MCP semantic rule-context tool;
- bounded query-oriented retrieval and regression tests.

### Modified

The proposal's single-table lookup model became a source-provenanced compiled index whose chunks retain canonical path, heading anchor, source hash, Repository Version, and applicability metadata. Rule retrieval now combines the kernel, relevant Core rules, selected World/Ruleset rules, enabled modules, and operation/topic rules while handing Campaign Canon to FR-011 under the same Campaign ID.

### Rejected or Reverted

The two original untracked pointer files were removed after their intent was understood. Their following claims were rejected:

- SQL outranks repository Markdown;
- canonical rules belong in a campaign/deployment `config` table;
- one hard-coded database and schema define Eternal Cycle;
- generic key namespaces can replace specialist rule ownership and repository governance;
- raw SQL examples are the runtime rule interface.

These claims conflicted with Repository Canon authority, storage-neutral Campaign Persistence, FR-012 ownership, FR-017's semantic MCP boundary, release history, and repository conventions. Nothing was silently discarded: the valid queryability objective was implemented in a safer architecture, while the authority inversion and topology coupling were explicitly rejected.

## Rule Compilation and Retrieval

The canonical [Rule Compilation and Context-Efficient Retrieval](../../docs/rules/RULE_COMPILATION_AND_RETRIEVAL.md) contract establishes:

- Markdown Repository Canon as the source of authority;
- a compact mandatory [Runtime Rule Kernel](../../docs/rules/RUNTIME_RULE_KERNEL.md);
- a replaceable Derived compiled index;
- source path, anchor, hash, and Repository Version provenance;
- Core, World/Ruleset, optional-module, Campaign Mode, operation, topic, and priority metadata;
- Campaign Canon retrieval as a separate FR-011 input under the same Campaign ID;
- an 8,000 estimated-token normal-play ceiling;
- explicit failure when the kernel, trusted source root, manifest, or required specialist rule cannot be satisfied.

The reference MCP service exposes `ec_get_rule_context`. The service accepts semantic selection inputs but not source paths, SQL, schema identifiers, or campaign facts. The compiler rejects manifest sources that escape the trusted repository root.

## Schema Architecture Correction

The FR-017 reference store previously embedded `ec.` in every application query. FR-018 now distinguishes:

```text
Campaign ID
  -> World/Ruleset/Domain Model
  -> compatible SQL Schema
  -> SQL Database
```

`ec` remains the default/reference schema for the standard Eternal Cycle World Model. Trusted configuration may map campaigns to other World Models and World Models to strictly validated schemas and model/ruleset versions. Compatible campaigns may share one schema and remain isolated by parameterized Campaign ID. Different World Models may use different schemas.

Every application query uses a canonical schema token bound through `SqlServerSchemaIdentifier` after strict validation and correct identifier quoting. Schema text never comes from an MCP request, player input, narration, or record payload. Campaign and data values remain SQL parameters.

The original `001_initial.sql` remains the backward-compatible default `ec` reference. `001_initial.template.sql` supports a selected custom schema only through the trusted migration renderer. Rendering targets one World Model/schema binding and does not enumerate or modify unrelated schemas.

## Compatibility and Migration

No populated campaign schema or save was migrated. Existing standard `ec` campaigns remain compatible through the default route. Deployments that adopt a custom World Model add trusted campaign-to-world and world-to-schema configuration, render the template for that schema, provision campaign identity administratively, and validate under normal migration rules.

The common semantic MCP contract remains unchanged for status, exact reads, commits, and retry. Rule retrieval is read-only. No arbitrary SQL interface was introduced.

## Regression Coverage

The .NET suite covers:

- default `ec` routing;
- valid configured non-`ec` routing;
- two distinct campaigns sharing one compatible schema;
- different World Models using different schemas;
- cross-campaign and cross-schema route isolation;
- schema-scoped migration rendering;
- default-is-not-mandatory behavior;
- unsafe schema identifier rejection;
- Runtime Rule Kernel, Core, and selected World rule inclusion;
- unrelated World and topic exclusion;
- source hash provenance;
- Campaign ID preservation;
- the 8K world-specific retrieval target.

The tests do not provision a live SQL Server. Deployment acceptance still requires applying the selected rendered schema and exercising transaction, conflict, backup, recovery, authorization, and migration behavior under an authorized test deployment.

## Validation

- MCP service build: pass with zero warnings.
- MCP service tests: pass, 22 of 22.
- FR-017 portable-persistence harness: pass after updating its structural assertion for schema-routed SQL.
- FR-018 rule-compilation harness: pass.
- Full repository validation: pass.
- Broken links, orphaned Markdown documents, forbidden campaign-data directories, blocking unresolved questions, and unsafe retained SQL-authority pointers: zero.

## Remaining Boundaries

- Exact model tokenization remains runtime-specific; the reference compiler uses a conservative deterministic estimate and enforces an 8K maximum.
- A host must still call rule retrieval and persistence tools faithfully. Repository contracts cannot force a host to invoke a tool it declines to invoke.
- No populated World package is added here. Future worlds register their own canonical sources and trusted World Model configuration through owner-authorized work.

No unresolved design question remains for FR-018.

## Result

FR-018 is complete. Phase 13 remains active with no automatically selected next objective. The permanent Future Revisions item remains `[∞]`, and the historical v1.0.0 release is unchanged.
