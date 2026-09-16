# Rule Compilation and Context-Efficient Retrieval

## Purpose

This document defines the FR-018 rule compiler, derived index, and relevance-filtered retrieval contract. The goal is to give a runtime enough verified rules for one operation without loading the whole repository or mixing unrelated worlds into an ordinary context.

## Document Control

- **Owner:** compiled rule metadata, source provenance, applicability filters, selection order, budget enforcement, and retrieval failure behavior
- **Primary authorities:** [Runtime Rule Kernel](RUNTIME_RULE_KERNEL.md), [Repository Conventions](../../design/REPOSITORY_CONVENTIONS.md), [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md), and [Context Assembly](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- **Dependencies:** stable Repository Version, World/Ruleset identity, Campaign ID, Campaign Mode, operation, topics, and enabled optional modules
- **Extensions:** approved world packages, optional modules, alternate derived-index technologies, and runtime-specific tokenizers
- **Consumers:** context assemblers, AI Execution Profiles, human GM tools, and the reference MCP service
- **Repository boundary:** compiled rule artifacts contain no Campaign Canon, private locator, credential, save payload, or GM Secret

## Authority

Canonical Markdown remains the rule source. Compilation is a deterministic transformation for discovery and delivery. Every compiled chunk carries its source path, optional heading anchor, source hash, Repository Version, and applicability metadata. When compiled text conflicts with its source, the source wins and the index is stale.

SQL or another query engine may store a compiled index, but it does not become rule authority. The pre-FR-018 proposal that placed authoritative rules in a campaign SQL `config` table is rejected because it inverted Repository Canon and coupled universal rules to one deployment topology.

## Runtime Composition

An ordinary context is assembled in this order:

```text
Runtime Rule Kernel
  + relevant Eternal Cycle Core rules
  + selected World/Ruleset rules
  + explicitly enabled optional-module rules
  + operation/topic-specific rules
  + relevant Campaign Canon from canonical persistence
```

The compiled-rule result owns only the first five layers. FR-011 retrieves Campaign Canon separately through the configured persistence mode using the same Campaign ID. A runtime must not treat a rule chunk as a campaign fact or append campaign facts to the reusable rule index.

## Applicability Metadata

Each source declares:

- stable Rule Source ID;
- canonical source path;
- layer: Runtime Kernel, Core, World, or Optional Module;
- applicable World/Ruleset IDs where scoped;
- optional-module IDs where scoped;
- Campaign Modes;
- operations;
- topics;
- priority and mandatory-kernel status.

An absent world scope means world-neutral. A World source must name its World/Ruleset. An Optional Module source must name its module and may also be world-scoped. `*` may express deliberate applicability across values; it does not waive visibility or authority checks.

## Compilation

The compiler:

1. reads the trusted manifest and canonical source files inside the configured repository root;
2. rejects source paths that escape that root;
3. splits Markdown into heading-addressable chunks;
4. calculates a source hash and conservative token estimate;
5. preserves metadata and source anchors;
6. emits a Derived compiled index bound to one Repository Version.

Compilation never edits sources. Repository or manifest changes invalidate the prior index and require recompilation. A runtime may use a tokenizer specific to its model, but it must not understate the configured normal-play budget.

## Retrieval

For one request, the runtime:

1. resolves the Campaign ID to its trusted World/Ruleset configuration;
2. includes the Runtime Rule Kernel;
3. filters Core sources by operation and topic;
4. includes only sources for the selected World/Ruleset;
5. includes only explicitly enabled optional modules;
6. applies Campaign Mode, operation, and topic filters;
7. orders eligible chunks by layer, relevance, priority, and stable identity;
8. includes complete chunks until the configured budget is reached;
9. returns source provenance and the calculated estimate;
10. hands campaign-fact retrieval to FR-011 under the same Campaign ID.

The reference normal-play ceiling is **8,000 estimated rule tokens**. It is a regression target, not permission to omit a necessary rule. If the mandatory kernel exceeds the budget, retrieval fails. If a specialist source does not fit, the runtime narrows the request or performs a separate targeted retrieval rather than silently improvising.

## World Isolation

Campaign ID, World/Ruleset/Domain Model, SQL Schema, and SQL Database are distinct. The campaign's trusted World/Ruleset binding selects applicable world rules. A World A request does not routinely receive World B sources, even when campaigns share a database or when two worlds use structurally compatible persistence.

Multiple campaigns may share one compatible SQL schema while remaining isolated by Campaign ID. Conversely, different worlds may use different schemas. Rule selection follows World/Ruleset identity, not a guessed schema name.

## Optional Modules and Campaign Modes

Optional-module rules are excluded unless Campaign Configuration enables the module. Campaign Mode filters operational rules for `NORMAL`, `VALIDATION`, or `DEVELOPMENT` without changing fictional Canon. Development-only diagnostics do not enter an ordinary Gameplay Context merely because they are indexed.

## Failure and Freshness

Retrieval fails explicitly when:

- the repository root or manifest cannot be resolved;
- a source escapes the trusted root;
- source identity, World/Ruleset binding, or Campaign ID is invalid;
- the kernel cannot fit within the budget;
- the compiled Repository Version or source hash is stale where verification is required;
- a required specialist rule cannot be retrieved.

The runtime does not fill a missing rule from memory, another world's rules, campaign narration, or a stale SQL copy.

## Regression Contract

The implementation must cover:

- kernel inclusion;
- operation and topic filtering;
- optional-module exclusion and inclusion;
- Campaign Mode filtering;
- source path, anchor, hash, and Repository Version provenance;
- an 8K normal-play ceiling;
- World A retrieval that includes relevant Core and World A rules but excludes World B;
- preservation of the requested Campaign ID for the subsequent Campaign Canon read;
- failure when trusted configuration or mandatory kernel constraints cannot be satisfied.

## Safeguards

- Repository Canon remains authoritative.
- A compiled index is Derived and replaceable.
- No campaign-specific fact enters the manifest or index.
- No runtime accepts player-supplied schema or source paths.
- Retrieval cannot grant access to another campaign, world, optional module, or protected truth layer.
- Relevance filtering cannot waive required rules or Campaign Canon reads.
- The compiler never uses an index write as a rules change.

## Related Documents

- [Rule Compilation Index](README.md)
- [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md)
- [Context Assembly and Gameplay Turn Persistence](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [Portable Persistence Architecture](../persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- [MCP Persistence Mode](../persistence/MCP_PERSISTENCE_MODE.md)
- [FR-018 Implementation Audit](../../design/audits/FR_018_RULE_COMPILATION_AND_SCHEMA_ROUTING_AUDIT.md)
