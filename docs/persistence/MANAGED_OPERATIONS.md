# Managed Operations

## Purpose

This document defines the implementation-neutral contract for service-owned work that may outlive an interactive client request. Initial Managed Rule Publication is the first required use case.

## Document Control

- **Owner:** durable operation identity, lifecycle, idempotent initiation, status, recovery, timeout, authorization, and result evidence
- **Dependencies:** [Managed Data Service](MANAGED_DATA_SERVICE.md), [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md), and [Community Feedback and Diagnostics](../support/COMMUNITY_FEEDBACK_AND_DIAGNOSTICS.md)
- **Extensions:** workers, schedulers, queues, relational stores, document stores, or other durable provider-native mechanisms
- **Consumers:** Managed service implementations, administrative clients, AI runtimes, diagnostics, and recovery tooling
- **Repository boundary:** no live operation row, credential, private locator, campaign fact, or provider-specific queue configuration belongs here

## Core Contract

An initiating request validates authority, creates or reuses a durable operation, and returns its identity promptly. Long-running work continues under service policy rather than the initiating client's request lifetime.

```text
authorize and validate
  -> create or reuse operation
  -> return operation identity and Queued/Running state
  -> service executes durable stages
  -> client reads status independently
```

The implementation must not substitute an in-memory detached task for durability. SQL Server and a hosted worker are one reference mapping, not an Eternal Cycle requirement.

## State Model

Supported semantic states are:

- `Queued` - accepted and awaiting execution;
- `Running` - claimed by the service;
- `Succeeded` - completed with validated result identity;
- `Failed` - completed unsuccessfully with safe causal evidence;
- `Cancelling` - explicit service-side cancellation is in progress;
- `Cancelled` - cancellation completed;
- `Interrupted` - execution ownership was lost and deterministic recovery or retry is required.

Persisted `Running` or `Cancelling` work must not remain phantom-active after service restart. Startup recovery marks it `Interrupted`, then either resumes through idempotent domain stages or leaves a retryable terminal diagnosis. Client disconnect or request cancellation does not cancel the durable operation.

A service may start before its administrative bootstrap has created the durable operation store. Its worker waits and retries store recovery under bounded polling instead of crashing the host or substituting an in-memory queue. After any later store outage, it reruns interruption recovery before claiming more work so a previously claimed operation cannot remain phantom-`Running`.

## Operation Record

A safe operation view includes:

- stable Operation ID and operation kind;
- correlation ID and deduplication identity;
- state and current domain stage;
- created, started, updated, and completion times;
- meaningful bounded progress where available;
- safe status detail and error code;
- retry safety;
- separate `userApprovalRequired` and `administrativeInterventionRequired` claims;
- safe Ruleset or source identity;
- result identity after completion.

The service must support status lookup by Operation ID and bounded discovery of recent relevant operations. Diagnostics may correlate by Operation ID, correlation ID, or both without requiring a Campaign ID. Losing the immediate response does not make recovery impossible. Retry safety and intervention claims must agree between an outer response envelope and its inner operation record.

Before the durable operation schema exists, operation initiation, status, and listing remain semantically callable but do not query absent operation tables. They return a structured `MIGRATION_REQUIRED` outcome, configuration state where relevant, and the bounded setup recovery capability. Successful schema migration then enables normal durable operation behavior without changing the client's conceptual workflow.

## Idempotency and Concurrency

Equivalent active requests reuse one operation when competing execution would be unsafe or wasteful. The operation's deduplication identity includes the semantic work target and configuration revision needed to distinguish materially different work.

Domain execution remains idempotent. Rule publication reuses durable `Candidate -> Validated -> Published -> Active` state and source-unique Rule Release identity. Operation deduplication does not replace domain idempotency.

## Timeouts

Interactive request deadlines do not define server execution policy. The service retains bounded operation, network-acquisition, local-process, compilation, datastore, and cleanup limits. A long legitimate operation may therefore outlive the request that initiated it while remaining finite and cancellable by service policy.

Cancellation classification belongs to the durable operation owner. Lower publication and provider layers propagate cancellation instead of converting it into an ordinary result. The owner distinguishes host shutdown, its own Managed Operation timeout, provider acquisition timeout, provider subprocess timeout, explicit administrative cancellation when such a capability exists, and an unexpected parent cancellation. Implementations must not claim an explicit administrative-cancel path exists until they expose and authorize one.

Host shutdown leaves work `Interrupted` and recoverable. A Managed Operation timeout and an unexpected parent cancellation leave safe causal evidence with the operation's true Operation ID and correlation ID. Unknown parent-token origins remain unknown until evidence identifies them.

## Authorization

Administrative initiation requires explicit informed user approval. An AI may set an approval field only after the user actually authorizes the explained action. A deployment may additionally require operator intervention, but that is a separate condition and must not be presented as a magic phrase that an ordinary player must discover or type.

## Diagnostics and Failure

Current readiness and latest causal operation evidence are separate:

- readiness reports what is usable now;
- operation status reports what happened, at which stage, and whether retry is safe.

Unknown infrastructure causes remain unknown. Safe output does not speculate about network policy, authentication, or provider health without evidence. Protected diagnostics retain implementation detail according to the service's security policy.

Semantic failures cross the interface unchanged: a migration requirement remains a migration requirement rather than becoming a generic tool exception. Error-code lookup and a partial Error Dump remain available without an operation record or Campaign ID.

## Generalization Boundary

The universal contract requires durable identity, independent execution, explicit state, idempotency, recovery, bounded policy, status discovery, and safe evidence. It does not require .NET, MCP, SQL Server, T-SQL, Git, a filesystem, or a particular worker technology.

## Related Documents

- [Managed Data Service](MANAGED_DATA_SERVICE.md)
- [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md)
- [Running Eternal Cycle](RUNNING_ETERNAL_CYCLE.md)
- [MCP Managed Service Interface](MCP_PERSISTENCE_MODE.md)
- [Errors and Portable Diagnostics](../support/ERRORS_AND_PORTABLE_DIAGNOSTICS.md)
