# Managed/MCP-Only Acceptance Test

This acceptance test validates the optional .NET/MCP/T-SQL reference implementation as the AI's only Eternal Cycle data and rule interface. It is a deployment test, not a substitute for repository unit tests.

## Required Environment

- a real Microsoft SQL Server database reserved for the test;
- a built reference service configured through protected environment or host settings;
- a compatible MCP client and a local or small-context AI with an approximately 8K practical rule budget;
- one authorized administrator for setup actions;
- AI filesystem/repository access to Eternal Cycle Markdown disabled;
- AI direct SQL/database access disabled.

Record service build, configuration class, source revision, client version, and sanitized timestamps. Do not record credentials or campaign secrets.

Stable is the normal default and must not silently use unreleased content. Until a compatible Stable Managed Rule Source is published, current v1.1 development acceptance explicitly sets `EternalCycle__Rules__ReleaseChannel=Prerelease`. Record the discovery ref and the immutable resolved commit SHA separately; do not describe a published release only as `main`.

## First-Run Matrix

1. Connect to a clean database and verify `SETUP_REQUIRED`, not a generic invocation error.
2. Verify setup cannot run before explicit approval.
3. Approve setup and verify only configured Eternal Cycle schemas/tables are created.
4. Repeat setup and verify a safe no-op.
5. Verify diagnostics before rule publication reports an initialized but empty Rule Store.
6. Verify no source reports `RULE_SOURCE_REQUIRED`.
7. With the default Stable channel, verify an historical official source that lacks the Managed manifest returns non-retryable `RULE_SOURCE_INCOMPATIBLE` and never falls forward to Prerelease.
8. Explicitly select Prerelease, verify the official development discovery ref resolves to one immutable SHA, and confirm the persisted Rule Release records channel, discovery ref, SHA, manifest format, and compiler contract separately.
9. Repeat with an isolated custom compatible source to prove override behavior.
10. Publish and activate an initial release from the managed `--no-checkout` cache within the target client's operation timeout; verify one source-unique release and `READY`.
11. Move the discovery ref after publication and verify the existing release retains its original immutable SHA.
12. Make the source unavailable and verify the active valid release remains usable with `DEGRADED` update status.
13. Submit missing-manifest and unsupported-contract candidates and verify both are compatibility failures rather than network failures.
14. Cancel one publication after a durable stage, retry it, and verify the service resumes the same release identity without duplicate chunks, selectors, dependencies, or activation effects.
15. Force one publication-stage failure and verify the safe response carries operation, stage, code, correlation ID, retry guidance, intervention guidance, and diagnostic availability. Verify readiness retains its current broad state while exposing the latest relevant causal failure. Verify the SQL diagnostic or protected physical fallback contains useful redacted evidence with no secrets.

## MCP-Only Gameplay Run

1. Start with an already initialized service and verify `READY`.
2. Say, “Start a new Eternal Cycle game.” Use the supported permission-gated creation path.
3. Retrieve rules only through `ec_get_rule_context`.
4. Play multiple turns involving exploration, NPC/world reaction, experimentation or discovery, character Development, applicable world rules, and a Provisional Ruling where a narrow genuine gap exists.
5. Commit each complete Affected Set and retain validated receipts.
6. Stop and restart the AI/client/service as appropriate.
7. Say, “Continue my Eternal Cycle game.” Verify campaign discovery and continuity from canonical Managed state.
8. Inspect the tool trace and verify **zero Eternal Cycle repository/source-file reads by the AI** and **zero direct SQL calls by the AI**.
9. Verify every returned Rule Packet remains dependency-complete and at or below 8,000 estimated tokens.

## Campaign Discovery Cases

- zero campaigns: offer authorized creation;
- one campaign: select it without asking for an opaque ID;
- multiple campaigns: present meaningful names and descriptions;
- incorrect or unknown Campaign ID: return `CAMPAIGN_NOT_FOUND` without cross-campaign leakage.

## Failure Cases

Exercise missing campaign schema, missing rule schema, missing source, source unavailable, remote acquisition timeout, ref resolution, missing manifest, unsupported manifest/compiler contract, source read, failed compilation, failed validation, SQL staging/publication, activation, cancellation, no published release, no active release, incompatible release, and unavailable persistence. Every expected state must return a sanitized semantic code and remedy. Confirm the model can distinguish these cases through MCP readiness, setup responses, and authorized diagnostics without filesystem, shell, Git, source-code, or direct-SQL access. Confirm SQL diagnostic failure uses the physical fallback, and failure of both sinks still preserves the original operation error. No case may expose connection strings, credentials, source credentials, filesystem details, Campaign Canon, or GM Secrets, including in verbose mode.

## Evidence and Pass Boundary

The run passes only when SQL inspection, service diagnostics, MCP traces, restart/resume behavior, and receipts agree. Unit-test fakes cannot establish this acceptance result. Document any host that hides stderr and confirm the optional sanitized file log contains useful category and exception-type evidence without secrets.
