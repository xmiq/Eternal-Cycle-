# Community Feedback and Diagnostics

## Purpose

This procedure helps human and AI GMs classify a problem, gather minimal sanitized evidence, and route an actionable report without turning ordinary campaigns into tests or exposing private state.

## Classification

Classify the observation before reporting:

- ordinary campaign or world outcome;
- campaign-specific continuity or configuration issue;
- rules ambiguity or narrow gap suitable for a Provisional Ruling;
- probable Eternal Cycle rules defect;
- supplied reference-tooling defect;
- third-party runtime or implementation problem;
- feature request or question.

An unexpected or unfavorable result is not automatically a bug. A `NORMAL` campaign remains `NORMAL` when it discovers or reports a defect.

## Sanitized Diagnostic Contract

A runtime or Managed service may produce a report containing only relevant fields such as:

- Eternal Cycle and Repository Version;
- active RuleSet and Rule Release;
- immutable source identity or commit SHA;
- compiler and service implementation versions;
- runtime/model where known;
- persistence strategy, interface, and storage-adapter family;
- Logical Data Namespace ID, World/Ruleset, Campaign Mode, and migration version;
- source-update, compilation, validation, or persistence error codes;
- relevant Rule IDs and minimal reproduction facts.

Diagnostics never automatically include credentials, tokens, connection strings, private locators, complete Campaign Canon, GM Secrets, private conversations, personal information, or unrelated system data. The AI minimizes the report again before external use.

The portable reliability hierarchy is defined by [Errors and Portable Diagnostics](ERRORS_AND_PORTABLE_DIAGNOSTICS.md). A bounded **Error Dump** is an immediate local view and does not depend on a **Support Bundle**. A Support Bundle is an optional richer local artifact. **Support submission** is a third, separate action that requires explicit destination-aware consent. None is silently uploaded.

## Issue Report

Prepare:

```text
Eternal Cycle version:
Rule Release and source identity:
Runtime/model:
Persistence strategy:
Service implementation/interface/storage family:
World/Ruleset and Campaign Mode:
Problem classification:
Expected behavior:
Observed behavior:
Relevant Rule IDs:
Minimal reproduction:
Sanitized diagnostics:
Sensitive campaign data omitted: yes/no
Distribution provenance:
```

If an authorized issue-tracker integration exists, the runtime may offer submission and must obtain the user's instruction before sending. Without integration, prepare the report and provide the destination from [official support metadata](../../SUPPORT.md). Never silently submit.

## Distribution Provenance

Official support destinations apply to the identified official distribution. Forks and custom builds must identify their own repository and modification provenance and do not imply that the original maintainer supports altered behavior.

## Feedback Lifecycle

```text
Canonical Markdown
  -> official repository revision
  -> Rule Source Provider detects immutable revision
  -> acquire, compile, validate, publish
  -> activate under policy
  -> indexed runtime retrieval
  -> gameplay
  -> classified, sanitized feedback
  -> official issue or maintainer review
  -> authorized correction
  -> new canonical revision
```

## Related Documents

- [Official Support Metadata](../../SUPPORT.md)
- [Errors and Portable Diagnostics](ERRORS_AND_PORTABLE_DIAGNOSTICS.md)
- [Provisional Rulings](../gm/PROVISIONAL_RULINGS.md)
- [Managed Rule Publication](../rules/MANAGED_RULE_PUBLICATION.md)
- [AI Capabilities and Limitations](../ai/AI_CAPABILITIES_AND_LIMITATIONS.md)
