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

A cache created with `git clone --no-checkout` intentionally has no populated working tree. The provider resolves a ref to an immutable commit and reads the manifest and declared sources from Git objects. Missing working-tree files in that cache are not evidence of failed acquisition.

## Release Channels and Compatibility

The product/service version, Rule Source channel, discovery ref, immutable source commit, compiled Rule Release identity, and manifest/compiler contract are separate claims.

- `Stable` is the default. It selects only a compatible released source and never silently falls forward to development content.
- `Prerelease` is an explicit administrative opt-in. It may discover from a moving development ref, but compilation first resolves that ref to one immutable snapshot.

The resulting Rule Release records both the discovery ref and exact source identity. Later movement of a branch cannot change an already-published release's provenance. A historical product tag remains immutable even when it predates the Managed compilation contract; absence of a compatible Stable source is reported honestly rather than repaired by relabeling development content.

The source manifest declares a supported manifest format, compiler contract, RuleSet identity, repository version, and required canonical documents. The service validates that contract before compilation. Missing manifests, unsupported format or compiler contracts, RuleSet mismatch, and missing required sources are `RULE_SOURCE_INCOMPATIBLE`, not transient availability failures. Retrying the same immutable incompatible source is not safe; administrators must select a compatible source or channel.

Network clone and fetch operations use a separately bounded acquisition timeout. Local ref resolution and object reads use a shorter process timeout. Cancellation bounds process-tree termination and redirected-output cleanup. A complete no-checkout cache may be reused, and an already-present immutable revision may satisfy an update-policy-compatible request without unnecessary network access. Partial clone directories have no authority and are replaced deterministically.

### Eternal Cycle release-candidate provenance

Full-release tags such as `v1.0.0` are immutable. Eternal Cycle release-candidate discovery tags intentionally move; development toward 1.1 uses `v1.1.0-rc` as a human and tool discovery pointer.

For an official Prerelease source, publication records:

- immutable Base Release tag;
- moving Discovery Tag;
- commits from the Base Release to the resolved source;
- derived display version such as `1.1.0-rc.47`;
- exact resolved commit SHA.

The SHA is authoritative identity and provenance. The derived display version is not globally unique and never replaces it. Moving the discovery tag may nominate another candidate, but it cannot alter a historical Rule Release's stored source commit. No task may move the discovery tag merely by documenting or testing this convention.

## Rule Release

Each release records:

- RuleSet and Rule Release IDs;
- exact source identity and provider kind;
- selected source channel and discovery ref;
- manifest format and compiler contract versions;
- compiler version and compilation time;
- Repository Version and source hashes;
- validation, publication, and activation states;
- applicability and dependency metadata;
- failure evidence where applicable.

Candidate identity is stable and source-unique. Durable `Candidate`, `Validated`, or `Published` work resumes on retry instead of creating a competing release for the same source identity. A failed acquisition, compilation, validation, publication, or activation leaves the current active validated release intact. Activation is atomic from runtime retrieval's perspective.

The observable publication stages are source acquisition, ref resolution, manifest read, rule-document read, compilation, candidate staging, validation, minimum-closure preparation, publication, activation, remaining-rule preparation, and update-check recording. Initial publication runs as a [Managed Operation](../persistence/MANAGED_OPERATIONS.md): initiation returns durable identity promptly, a service-owned worker advances the stages, and independent status lookup reports progress and result identity. Client timeout or disconnect does not cancel that operation. Service restart detects interrupted ownership and resumes through the existing idempotent release lifecycle.

Cancellation under service policy stops dependent work, terminates owned source processes where supported, preserves completed durable stages, and permits idempotent retry. A safe interface response identifies the failed stage and correlation ID without exposing protected diagnostics.

## Progressive Rule Readiness

Rule publication is not an all-or-nothing gameplay gate. The service tracks preparation for each Rule Source in a release and exposes distinct readiness:

- **ServiceReady** - the service interface is responding;
- **PersistenceReady** - required service and campaign persistence structures are usable;
- **RuleKernelReady** - the Runtime Rule Kernel closure is validated and available;
- **CampaignBootstrapReady** - the minimum authoritative closure needed to create or resume safe play is available;
- **GameplayReady** - persistence, active compatible release, kernel, bootstrap closure, and requested campaign conditions permit play;
- **FullRulesetReady** - every selected Rule Source is prepared.

`GameplayReady` may be true while `FullRulesetReady` is false. This never permits guessing. A requested operation's Rule IDs and dependency closure must be ready before resolution. If they are not, retrieval returns `PENDING`, raises the closure's preparation priority, and waits for authoritative availability. Failed preparation returns `FAILED` with safe causal evidence.

## Preparation Order

The portable priority order is:

1. Runtime Rule Kernel;
2. Campaign Bootstrap and starting-play requirements;
3. immediate gameplay core;
4. campaign-relevant modules;
5. remaining standard rules;
6. optional, world-specific, or rare rules.

Manifest preparation tier, ordinary priority, selectors, applicability, and dependency closure refine this order. A gameplay request may dynamically boost a pending required closure above unrelated work. The service re-evaluates durable pending priority between preparation units; a stored boost is not merely diagnostic metadata.

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

The reference implementation stores published reusable Rule Releases, chunks, applicability, dependencies, source provenance, update status, active-release pointers, and sanitized Managed-operation diagnostics in `ec_domain`. Candidate rows, chunks, selectors, and dependencies are staged in one transaction; bounded multi-row writes avoid pathological per-record round trips without changing atomicity. World schemas contain Campaign Canon only. Other Managed implementations use their Domain Namespace equivalent.

## DIRECT Delivery

Direct runtimes may use packaged compiled indexes, local indexes, repository-backed tooling, or another validated mechanism. They do not require MCP. Regardless of strategy, normal play consumes selective authoritative rules rather than reconstructing the entire repository in model context.

## Related Documents

- [Rule Compilation and Retrieval](RULE_COMPILATION_AND_RETRIEVAL.md)
- [Managed Data Service](../persistence/MANAGED_DATA_SERVICE.md)
- [Managed Operations](../persistence/MANAGED_OPERATIONS.md)
- [Logical Data Namespace](../persistence/LOGICAL_DATA_NAMESPACE.md)
- [Community Feedback and Diagnostics](../support/COMMUNITY_FEEDBACK_AND_DIAGNOSTICS.md)
- [Running Eternal Cycle](../persistence/RUNNING_ETERNAL_CYCLE.md)
