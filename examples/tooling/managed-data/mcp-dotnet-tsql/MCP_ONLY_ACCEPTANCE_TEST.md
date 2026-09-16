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

## First-Run Matrix

1. Connect to a clean database and verify `SETUP_REQUIRED`, not a generic invocation error.
2. Verify setup cannot run before explicit approval.
3. Approve setup and verify only configured Eternal Cycle schemas/tables are created.
4. Repeat setup and verify a safe no-op.
5. Verify diagnostics before rule publication reports an initialized but empty Rule Store.
6. Verify no source reports `RULE_SOURCE_REQUIRED`.
7. Select the official default, then repeat with an isolated custom compatible source to prove override behavior.
8. Publish and activate an initial release; verify `READY`.
9. Make the source unavailable and verify the active valid release remains usable with `DEGRADED` update status.
10. Submit an invalid candidate and verify it never replaces the active release.

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

Exercise missing campaign schema, missing rule schema, missing source, source unavailable, no published release, no active release, incompatible release, failed compilation, and unavailable persistence. Every expected state must return a sanitized semantic code and remedy. No case may expose connection strings, credentials, source credentials, filesystem details, Campaign Canon, or GM Secrets.

## Evidence and Pass Boundary

The run passes only when SQL inspection, service diagnostics, MCP traces, restart/resume behavior, and receipts agree. Unit-test fakes cannot establish this acceptance result. Document any host that hides stderr and confirm the optional sanitized file log contains useful category and exception-type evidence without secrets.
