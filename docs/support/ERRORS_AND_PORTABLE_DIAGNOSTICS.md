# Errors and Portable Diagnostics

## Purpose

This document defines the implementation-neutral error and diagnostic reliability contract for Eternal Cycle tooling. It keeps recovery usable before campaign selection, before schema migration, and when an optional diagnostic sink or support integration is unavailable.

## Document Control

- **Owner:** error identity, portable diagnostic levels, sanitization, consent, and distribution-aware support routing
- **Dependencies:** [Managed Data Service](../persistence/MANAGED_DATA_SERVICE.md), [Managed Operations](../persistence/MANAGED_OPERATIONS.md), and [Community Feedback and Diagnostics](COMMUNITY_FEEDBACK_AND_DIAGNOSTICS.md)
- **Extensions:** implementation-specific error registries, bounded error-dump renderers, protected diagnostic stores, optional support-bundle generators, and authorized submission clients
- **Consumers:** players, operators, human and AI GMs, Managed service implementations, and support tooling
- **Repository boundary:** no credential, secret, private locator, populated campaign fact, GM Secret, or automatically submitted report belongs here

## Reliability Hierarchy

Each level must remain useful when every higher level is absent or broken.

| Level | Capability | Dependency rule |
| --- | --- | --- |
| 0 | Emergency error string | Static, bounded, and available even when normal error handling fails. |
| 1 | Error-code lookup | Static registry; requires neither campaign state nor the canonical datastore. |
| 2 | Error Dump | Bounded, sanitized, portable snapshot assembled from whatever local evidence is safely available. |
| 3 | Normal diagnostics | Implementation-owned structured diagnostics, including a protected SQL sink or automatic local fallback. |
| 4 | Support Bundle | Optional richer package assembled only for authorized troubleshooting. It is not required for an Error Dump. |
| 5 | Support submission | Explicitly authorized transfer to an identified destination. It is never automatic. |

Lower levels do not call, upload through, or otherwise depend on higher levels. Failure while preparing a Support Bundle cannot suppress an Error Dump. Failure while submitting a report cannot replace the underlying operation error.

## Semantic Error Contract

An Eternal Cycle error exposes, when available:

- a stable error code;
- a safe message;
- correlation identity;
- operation and stage;
- retry guidance;
- intervention guidance;
- recovery capabilities;
- a sanitized bounded detail only when verbose diagnostics are authorized.

Known domain failures retain their semantic code across the outer tool boundary. Unexpected failures become the implementation's stable internal-error code rather than raw exceptions. If the outer error boundary itself fails, Level 0 returns a static emergency string containing no interpolated exception or secret.

Verbose mode adds authorized, redacted detail. It never disables sanitization, changes an error's semantic identity, or turns failure into success.

## Configuration Discovery

A Managed service exposes configuration requirements without requiring a database connection or campaign. A requirement reports:

- exact environment and configuration-key names;
- required or optional status;
- configured, missing, invalid, disabled, or unavailable status;
- accepted formats or controlled values;
- safe default behavior;
- whether restart is required;
- intended operator audience;
- non-secret examples.

Secret values are never echoed. Configuration discovery describes how to configure the service; it does not prove that an external dependency is healthy.

## Automatic Diagnostic Fallback

Normal structured diagnostics are preferred. When the preferred sink is unavailable, the service automatically attempts a bounded protected local fallback unless an operator explicitly disables it. Ordinary players do not need to enable the fallback with a magic phrase or manual setup step.

Fallback state is itself discoverable as enabled, disabled, active, degraded, or unavailable. A fallback write failure must not mask the original operation failure. General optional logging and the fail-safe diagnostic fallback are separate facilities.

## Error Dump Format

`Error Dump` is a provider-neutral concept. An implementation may expose it through MCP, HTTP, a CLI, local IPC, or another interface.

The current portable format is `eternal-cycle-error-dump/v1`. It contains only bounded fields such as:

- generated time and correlation ID;
- requested error code and registry explanation;
- service and distribution identity;
- readiness and configuration status;
- recent sanitized diagnostic or operation evidence when available;
- warnings identifying unavailable evidence;
- human-readable text equivalent to the structured content.

The renderer enforces item and text limits. Missing evidence produces a partial dump with warnings rather than a fabricated value or total failure. SQL evidence is queried only when its schema is known to exist. The dump performs no network submission and contains no support-upload side effect.

## Sanitization

Portable diagnostics exclude or redact:

- passwords, tokens, credentials, and connection strings;
- private source or storage locators;
- full Campaign Canon and GM Secrets;
- unrelated file contents, environment values, conversations, and personal information;
- raw stack traces or exception chains in ordinary output.

Unknown information remains unknown. Sanitization may remove detail; it must not invent replacement facts.

## Error Dump and Support Bundle

An Error Dump is an immediate bounded troubleshooting view. A Support Bundle is an optional, potentially broader artifact that may add logs, manifests, or environment evidence under an implementation-specific policy.

A Support Bundle:

- is not required for base recovery;
- requires an explicit user or operator action;
- must disclose its planned contents before creation when sensitive context could be included;
- remains local until separate submission consent is granted;
- may be unavailable without weakening Levels 0 through 3.

The repository does not currently mandate a universal Support Bundle file format.

## Support Destination and Consent

Distribution metadata identifies a support-destination type and distribution provenance. Official destinations apply only to the identified official distribution. A derivative or modified build must identify its own provenance and cannot imply endorsement, warranty, or support by the original author.

Creating a dump, creating a bundle, and submitting either are three separate actions. External submission requires explicit consent naming the destination. Tool availability or a configured issue tracker does not constitute consent.

## Pre-Migration Recovery

Before the latest Managed schema exists, a client can still:

1. discover configuration requirements;
2. inspect structured readiness;
3. look up semantic error codes;
4. create a partial Error Dump;
5. preview supported service-owned migrations;
6. obtain informed authorization;
7. apply and validate the migration;
8. continue through source selection and durable publication.

Operation-store queries must not be issued before the operation schema exists. They return a structured migration-required response with the applicable recovery capability instead.

## Related Documents

- [Support Index](README.md)
- [Community Feedback and Diagnostics](COMMUNITY_FEEDBACK_AND_DIAGNOSTICS.md)
- [Official Support Metadata](../../SUPPORT.md)
- [Running Eternal Cycle](../persistence/RUNNING_ETERNAL_CYCLE.md)
- [AI Capabilities and Limitations](../ai/AI_CAPABILITIES_AND_LIMITATIONS.md)
