# FR-021 Durable Managed Operations and Progressive Rule Readiness Audit

> **Maintenance note:** The original reference cache and cancellation details recorded here were later refined by the [FR-021 Rule Source Live-Acceptance Repair](FR_021_RULE_SOURCE_LIVE_ACCEPTANCE_REPAIR_AUDIT.md). The portable durable-operation and progressive-readiness contracts remain authoritative.

## Scope

FR-021 responds to real LM Studio and Unsloth Studio deployment evidence: a valid first Rule Source acquisition and publication could outlive an MCP client's request deadline. The objective replaces synchronous first-run publication with durable service-owned work and makes authoritative rule availability progressive without weakening rule authority.

This audit covers the portable contract, the optional .NET/MCP/T-SQL/Git reference, migration 007, regressions, governance, and the boundary of real-infrastructure proof. It changes no gameplay mechanic, campaign persistence strategy, or released `v1.0.0` history.

## Interruption Recovery Classification

At resumption, the current working tree was treated as evidence rather than presumed completion.

### Complete in the Interrupted Pass

- durable operation model, SQL store, operation tools, processor, and registered hosted worker;
- startup recovery of `Running` and `Cancelling` work to reclaimable `Interrupted` state;
- migration 007 and project packaging;
- per-source preparation state, tiers, base priority, and dynamic boost;
- publication stages for minimum and remaining closure;
- readiness fields and Rule Context pending/priority behavior;
- campaign-optional diagnostics, approval fields, and prerelease metadata models;
- focused reference tests sufficient to prove the principal code paths in controlled fakes.

### Partially Complete at Resumption

- reference README and canonical operating documents had been updated, but canonical terminology, decisions, release governance, acceptance cases, registry coverage, and Phase 13 provenance were incomplete;
- progressive preparation existed in code and tests, but repository-level structural validation did not yet guard its connection to readiness and `ec_get_rule_context`;
- tests had passed once before interruption, but the final integrated tree had not been validated.

### Not Started at Resumption

- FR-021 roadmap and Future Revision closure;
- explicit release/version provenance governance;
- explicit three-level development feedback method;
- FR-021 structural harness and full integration into repository validation;
- this implementation audit and final real-acceptance matrix update.

## Durable Operation Design

`ec_publish_initial_rules` now validates authorization and source configuration, creates or reuses one durable operation, and returns its identity. `ec_get_operation_status` reads one operation; `ec_list_managed_operations` permits recovery when the initiating response or client state is lost.

The operation record carries state, stage, correlation, timestamps, bounded progress, safe detail, error and retry classification, approval/intervention claims, safe source and Ruleset identity, and the resulting Rule Release identity.

The initiating cancellation token reaches only durable enqueue. The hosted worker owns execution under a service timeout. It updates stages during acquisition, source resolution, manifest and document reads, compilation, staging, validation, minimum closure, publication, activation, remaining preparation, and outcome recording.

## Restart and Recovery

`ManagedOperationWorker` is registered through `AddHostedService` and invokes durable recovery before polling. SQL recovery changes orphaned `Running` or `Cancelling` rows to `Interrupted`; claiming permits queued or interrupted work to run through the existing source-unique, idempotent publication lifecycle. On a fresh database, the worker tolerates the not-yet-created operation store and retries after authorized migration rather than stopping the host. After any store outage it reruns recovery before claiming more work. Shutdown marks claimed work interrupted rather than successful. A service-owned timeout produces a retryable failure instead of inheriting the MCP request deadline.

This is deterministic at the contract and reference-code level. A real SQL Server kill/restart run remains external acceptance.

## Progressive Rule Readiness

Manifest sources carry semantic preparation tiers. Migration 007 persists each Rule Source's tier, base priority, dynamic boost, state, and failure evidence. The publication worker initializes those records, marks the minimum gameplay closure ready, publishes and activates it, then reads the next pending source from durable priority state until the selected Ruleset is fully prepared.

Readiness reports `ServiceReady`, `PersistenceReady`, `RuleKernelReady`, `CampaignBootstrapReady`, `GameplayReady`, and `FullRulesetReady` separately. Gameplay can be ready while unrelated rule modules remain pending.

`PublishedRuleContextProvider` calculates the selected dependency closure. If any required source is not ready, it raises those sources' durable priority and throws `RuleContextPendingException`; the MCP boundary returns pending instead of guessed rules. The same request succeeds after the closure is ready. Because remaining preparation asks the store for the next source on every iteration, gameplay priority changes actual work order rather than only metadata.

## Diagnostics and Authorization

Service diagnostics require no Campaign ID. An optional valid Campaign ID adds campaign route and state without borrowing another campaign. Current readiness and latest relevant causal operation remain distinct.

Administrative requests carry explicit informed user approval. Optional operator confirmation remains a deployment safeguard controlled by configuration. Ordinary players do not supply a magic phrase, SHA, Git ref, repository path, SQL schema, Campaign ID, Rule Release ID, or connection detail for normal start and Prerelease selection.

## RC Provenance

The official Prerelease source uses a moving `v1.1.0-rc` discovery pointer. The Git reference derives a display value such as `1.1.0-rc.47` from commits since immutable `v1.0.0`, while the resolved full source SHA remains Rule Release identity. SQL publication persists base release, discovery tag, count, display version, and immutable source commit separately. Moving the discovery pointer cannot mutate older release provenance.

No RC tag was created or moved by FR-021, and root `VERSION` remains `1.0.0`.

## Three-Level Generalization Audit

### Reference Implementation Only

- .NET `BackgroundService`, dependency injection, options, and cancellation APIs;
- SQL Server tables, indexes, transactions, locking, migration scripts, and T-SQL ordering;
- Git process invocation, `rev-list`, commit SHA, no-checkout cache, and timeout implementation;
- MCP tool attributes, method names, and stdio hosting.

### Managed Service Contract

- durable operation identity, lifecycle, idempotent initiation, request-lifetime independence, restart recovery, and status discovery;
- bounded server policy and structured causal diagnostics;
- natural user approval distinct from operator intervention;
- progressive authoritative rule availability, dependency closure, and dynamic priority;
- campaign-free service diagnostics.

### Eternal Cycle-Wide

- authoritative rules must be available before resolution;
- minimum authoritative closure may permit gameplay before full preparation;
- immutable full-release provenance and moving prerelease discovery are distinct;
- real-deployment findings are generalized by invariant and backed by correctly scoped evidence.

## Schema and Compatibility

Migration `007_durable_managed_operations` is additive and repeat-safe. It introduces Domain Namespace records for durable operations and rule-source preparation and adds nullable human-readable prerelease fields to Rule Releases. Existing chunks are backfilled as ready so an upgraded active publication does not become falsely unavailable. Existing campaign schemas and Campaign Canon are unchanged.

The reference project packages both fixed and trusted-schema template migrations. No campaign-specific migration is performed by the repository.

## Automated Test Coverage

The reference suite and structural harness cover the requested categories:

1. prompt initiation and Operation ID;
2. client-cancellation independence;
3. active-operation deduplication;
4. current stage;
5. resulting release;
6. structured failure;
7. restart recovery;
8. diagnostics without campaign;
9. optional campaign enrichment;
10. causal operation beside readiness;
11. user approval versus intervention;
12. no normal magic phrase;
13. approval cannot be silently omitted;
14. semantic Prerelease selection;
15. immutable SHA provenance;
16. moving-ref historical preservation;
17. deterministic `rc.N` derivation;
18. SHA rather than display identity;
19. immutable full-release governance;
20. minimum readiness before full readiness;
21. gameplay ready while full readiness is false;
22. unavailable closure pending and priority-raised;
23. successful retrieval after readiness;
24. dependency closure;
25. campaign-relevant priority over optional work;
26-29. retained FR-017 through FR-020 structural regressions;
30. Rule Source compatibility regressions;
31. diagnostic redaction;
32. SQL-to-file diagnostic fallback;
33. bounded SQL publication batching.

## Real-Deployment Boundary

The MCP-only acceptance plan records six required scenarios: LM Studio fresh start, Unsloth Studio fresh start, client disconnect, service restart, progressive gameplay, and moving RC update. They are not marked complete by unit tests.

No claim is made here that migration 007 has run on a live SQL Server, that either client has completed the revised flow, that slow real acquisition survives disconnect, or that a moving RC update has been published in production.

## Executed Validation

- .NET reference tests: **95 passed, 0 failed, 0 skipped**.
- Release build: **succeeded with 0 warnings and 0 errors**.
- FR-011 harness: **13 assertions passed**.
- FR-017 harness: **29 assertions passed**.
- FR-018 harness: **19 assertions passed**.
- FR-019 harness: **48 assertions passed**.
- FR-020 harness: **59 assertions passed** after updating superseded approval and durable-publication API expectations.
- FR-021 harness: **47 assertions passed**.
- Release-neutral campaign-mode harness: **17 assertions passed**.
- Repository validation: **passed** across 282 Markdown files, 7,095 relative links, 163 anchors, 186 indexed canonical documents, 43 templates, 1,250 terminology checks, 193 roadmap tasks, and 21 Future Revision entries.

The first repository-validation invocation was blocked by local PowerShell signing policy; the same canonical validator was then executed with process-scoped `-ExecutionPolicy Bypass`. One integration ordering mistake placed the new harness between an existing `if` and `else`; it was corrected before the successful final run.

## Final Status

- **FR-021:** complete in repository scope.
- **Phase 13:** active; rolling Future Revisions remains `[∞]`.
- **Version:** `1.0.0`.
- **Release:** no v1.1 release or RC tag created.
- **Open implementation defects:** none identified in the final integrated tree.
- **External acceptance:** Scenarios A-F remain unproven until executed against real infrastructure.
