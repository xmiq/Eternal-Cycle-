# Managed Rule Publication

## Purpose

This document defines how a Managed Data Service acquires canonical Markdown, builds a validated Derived Rule Release, and serves bounded Rule Packets without making the AI compile or reconstruct the repository.

## Document Control

- **Owner:** Rule Source Provider, compilation candidate, validation, publication, activation, update checking, provenance, fallback, and runtime retrieval
- **Primary authority:** [Rule Compilation and Context-Efficient Retrieval](RULE_COMPILATION_AND_RETRIEVAL.md)
- **Dependencies:** immutable source identity, Runtime Rule Kernel, World/Ruleset applicability, Campaign Configuration, and Managed Data Service authorization
- **Consumers:** Managed services, administrators, runtime rule-context clients, diagnostics, and migration tooling
- **Repository boundary:** no populated Rule Store, source credential, private repository locator, or live release row belongs here

## Authority Chain

```text
Canonical Markdown
  -> deterministic compilation
  -> validated candidate
  -> published Rule Release
  -> atomic activation
  -> bounded Rule Packet
```

Markdown remains Rule Canon. Compiled chunks, indexes, embeddings, and datastore records are Derived. Editing a published record does not edit Canon.

## Managed Responsibility

In `MANAGED` mode the service, not the AI GM:

- locates and acquires canonical sources;
- parses and compiles them;
- preserves stable Rule Source and chunk identities;
- calculates dependencies, topics, triggers, World/Ruleset and module applicability;
- records provenance, immutable source identity, source hashes, and compiler version;
- validates and publishes candidates;
- atomically activates an approved release;
- detects stale sources;
- checks for updates under configured policy;
- expands dependencies and assembles bounded Rule Packets.

Normal gameplay asks for semantic context and consumes the already-published result. It does not crawl the repository or populate the Rule Store.

## Rule Source Provider

A Rule Source Provider identifies a configured authority, detects its current immutable source identity, and returns a verified snapshot. Providers may use Git/GitHub, GitLab, another Git remote, a trusted local checkout, packaged release archive, or authenticated enterprise repository.

For Git sources, the immutable commit SHA is the compiled-source identity. Branches, tags, or channels may discover candidates but never replace the recorded commit.

A fresh Managed installation distinguishes **not yet configured** from an administrator's explicit disabled/offline update policy. When no source is selected, readiness reports `RULE_SOURCE_REQUIRED`. The service may offer an official source from authoritative distribution metadata, accept another compatible provider/source, persist the approved selection, and reuse it. A Git reference implementation may maintain a service-owned cache so an ordinary player does not need to clone a repository or configure a local `RepositoryRoot`; local checkout support remains valid for advanced, offline, and development use.

## Rule Release

Each release records:

- RuleSet and Rule Release IDs;
- exact source identity and provider kind;
- compiler version and compilation time;
- Repository Version and source hashes;
- validation, publication, and activation states;
- applicability and dependency metadata;
- failure evidence where applicable.

Candidates are append-only release attempts. A failed acquisition, compilation, validation, publication, or activation leaves the current active validated release intact. Activation is atomic from runtime retrieval's perspective.

## Update Policies

A deployment may select:

- startup check;
- periodic check;
- manual check;
- disabled/offline operation.

Successful validation may activate automatically or await administrator approval. Scheduling is implementation-specific. Ordinary gameplay does not request update checks.

An explicitly approved initial-publication operation may run once while normal future-update policy is `Disabled` or `Manual`. This does not silently enable updates. It moves a fresh service through source selected, candidate compiled, validated, published, activated according to policy, and readiness rechecked.

If source access fails while a valid applicable release exists, gameplay may continue on that release and report update status as degraded. Integrity failure in the active release is a separate blocking condition.

## Campaign Compatibility

Campaign Configuration records the compatible RuleSet, World/Domain Model, active or allowed Rule Release policy, and migration state. A newly activated service release does not silently apply an incompatible major rule change to an existing campaign. Adoption follows campaign migration and validation rules.

## Runtime Rule Packet

A semantic retrieval request identifies campaign, operation, topics, and bounded budget. The service resolves trusted Campaign Mode, World/Ruleset, modules, active compatible Rule Release, explicit dependencies, and access policy. The returned Rule Packet includes:

- Runtime Rule Kernel;
- relevant Core rules;
- selected World/Ruleset rules;
- enabled optional modules;
- operation/topic-specific rules;
- dependency-complete provenance;
- estimated context size.

Campaign Canon and current scene/input remain separate context inputs. The normal compiled-rule ceiling remains approximately 8,000 estimated tokens, and no hidden full-repository prompt evades that measurement.

## T-SQL Reference Mapping

The reference implementation stores published reusable Rule Releases, chunks, applicability, dependencies, source provenance, update status, and active-release pointers in `ec_domain`. World schemas contain Campaign Canon only. Other Managed implementations use their Domain Namespace equivalent.

## DIRECT Delivery

Direct runtimes may use packaged compiled indexes, local indexes, repository-backed tooling, or another validated mechanism. They do not require MCP. Regardless of strategy, normal play consumes selective authoritative rules rather than reconstructing the entire repository in model context.

## Related Documents

- [Rule Compilation and Retrieval](RULE_COMPILATION_AND_RETRIEVAL.md)
- [Managed Data Service](../persistence/MANAGED_DATA_SERVICE.md)
- [Logical Data Namespace](../persistence/LOGICAL_DATA_NAMESPACE.md)
- [Community Feedback and Diagnostics](../support/COMMUNITY_FEEDBACK_AND_DIAGNOSTICS.md)
- [Running Eternal Cycle](../persistence/RUNNING_ETERNAL_CYCLE.md)
