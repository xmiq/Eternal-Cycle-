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

1. Before database access, call `ec_get_configuration_requirements`. Verify exact environment names and validation status are returned, and verify no sensitive value is echoed.
2. Connect to a clean database and verify `SETUP_REQUIRED`, not a generic invocation error.
3. Call `ec_list_error_codes`, `ec_get_error_code`, and `ec_get_error_dump`. Verify error lookup and a bounded partial dump work without campaign state or initialized SQL diagnostics.
4. Verify setup cannot run before explicit approval.
5. Approve setup and verify only configured Eternal Cycle schemas/tables are created.
6. Repeat setup and verify a safe no-op.
7. Verify diagnostics before rule publication reports an initialized but empty Rule Store.
8. Verify no source reports `RULE_SOURCE_REQUIRED`.
9. With the default Stable channel, verify an historical official source that lacks the Managed manifest returns non-retryable `RULE_SOURCE_INCOMPATIBLE` and never falls forward to Prerelease.
10. Explicitly select Prerelease, verify the official development discovery ref resolves to one immutable SHA, and confirm the persisted Rule Release records channel, discovery ref, SHA, manifest format, and compiler contract separately.
11. Repeat with an existing local clone and an isolated modified compatible source. Verify both use the same technical validation/publication pipeline and are not described as unsafe merely because they are not the packaged default.
12. Initiate publication and verify the call returns promptly with a durable Operation ID, without requiring the client request to remain open for acquisition, compilation, validation, publication, or activation.
13. Poll or rediscover the operation independently. Verify the Runtime Rule Kernel and Campaign Bootstrap closure become ready, `GameplayReady` becomes true while `FullRulesetReady` may remain false, and remaining rules continue preparation under service-owned timeout policy.
14. Move the discovery ref after publication and verify the existing release retains its original immutable SHA.
15. Make the source unavailable and verify the active valid release remains usable with `DEGRADED` update status.
16. Submit missing-manifest and unsupported-contract candidates and verify both are compatibility failures rather than network failures.
17. Stop the client during publication, reconnect, and verify recent-operation discovery recovers the authoritative status without cancelling service-owned work.
18. Restart the service while an operation is `Running`; verify it cannot remain phantom-running, is recovered as `Interrupted`, and is safely reclaimed through idempotent publication or left retryable.
19. Cancel one publication after a durable stage, retry it, and verify the service resumes the same release identity without duplicate chunks, selectors, dependencies, or activation effects.
20. Force one publication-stage failure and verify the safe response carries operation, stage, code, correlation ID, retry guidance, intervention guidance, and diagnostic availability. Verify readiness retains its current broad state while exposing the latest relevant causal failure. Verify the SQL diagnostic or automatic protected physical fallback contains useful redacted evidence with no secrets.

## Genuine Pre-007 Upgrade Regression

Use a database initialized through migrations `001` to `006` with one preserved campaign, then connect the current service without manually applying migrations `007` or `008`.

1. Call `ec_get_configuration_requirements`; verify configuration is discoverable without schema access.
2. Call `ec_get_readiness`; expect `MIGRATION_REQUIRED` and a supported recovery capability, not a missing-table exception.
3. Call `ec_get_setup_plan`; verify only unapplied migrations `007` and `008` are planned.
4. Call `ec_get_diagnostics`; verify its scope is explicitly pre-migration and it does not query operation tables.
5. Call `ec_get_error_dump`; verify a bounded partial dump is returned even though post-007 evidence is unavailable.
6. Call `ec_get_operation_status` and `ec_list_managed_operations`; verify structured `MIGRATION_REQUIRED` responses and no operation-table query failure.
7. Attempt initialization without approval; verify no mutation.
8. Give natural informed approval and call `ec_initialize_service`; verify migrations `007` and `008` apply and validate without requiring a literal magic phrase or raw SQL.
9. Verify the pre-existing campaign remains intact.
10. Call readiness; expect `RULE_SOURCE_REQUIRED` rather than migration failure.
11. Configure a compatible source, approve publication, and call `ec_publish_initial_rules`; verify a durable queued Operation ID is returned.
12. Rediscover the operation independently and verify it transitions under the hosted worker.

The external LM Studio acceptance sequence uses these same MCP calls. The user must not be asked for repository access, direct database access, migration filenames, schema names, or developer paths.

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

Exercise missing campaign schema, missing rule schema, pre-007 operation lookup, missing source, source unavailable, remote acquisition timeout, ref resolution, missing manifest, unsupported manifest/compiler contract, source read, failed compilation, failed validation, SQL staging/publication, activation, cancellation, no published release, no active release, incompatible release, and unavailable persistence. Every expected state must return a sanitized semantic code and remedy. Confirm the model can distinguish these cases through MCP readiness, setup responses, static error lookup, Error Dumps, and authorized diagnostics without filesystem, shell, Git, source-code, or direct-SQL access. Confirm SQL diagnostic failure uses the automatic physical fallback, and failure of both sinks still preserves the original operation error through the outer tool boundary. No case may expose connection strings, credentials, source credentials, filesystem details, Campaign Canon, or GM Secrets, including in verbose mode.

## Evidence and Pass Boundary

The run passes only when SQL inspection, service diagnostics, MCP traces, restart/resume behavior, and receipts agree. Unit-test fakes and the repository's LocalDB migration fixture cannot establish external-client interoperability. Document any host that hides stderr and confirm the automatic diagnostic fallback contains correlated redacted evidence; optional general host logging remains a separate operator choice.

## Required Real-Deployment Scenarios

These scenarios remain pending until executed against their named real infrastructure. Repository tests and fakes do not establish them.

### A. LM Studio Fresh Campaign

Start a new campaign through LM Studio. Natural informed approval authorizes setup without a magic phrase. Initial publication returns an Operation ID promptly; minimum authoritative rules become playable before full preparation completes. The player supplies no Campaign ID, SHA, Git instruction, SQL instruction, mirror, or infrastructure diagnosis.

### B. Unsloth Studio Fresh Campaign

Repeat Scenario A through Unsloth Studio. Its interactive request deadline must not cancel the durable publication operation.

### C. Client Disconnect and Restart

Begin publication, close the client, reconnect, and discover the existing operation. Service state remains authoritative and no duplicate initial publication is launched.

### D. Managed Service Restart

Restart the reference service during publication. Persisted work is detected as interrupted and safely resumed or left retryable; no operation remains `Running` forever without an owner.

### E. Progressive Gameplay

Begin from a cold Rule Store, prepare the minimum authoritative gameplay closure, and start play while optional rules remain pending. Request an unexpected mechanic and verify its dependency closure receives higher preparation priority, returns `PENDING` rather than invented rules, then succeeds only after becoming `READY`.

### F. Moving RC Update

Resolve `v1.1.0-rc` to SHA A and publish its derived `rc.N` metadata. Move the discovery tag to SHA B, detect and publish the new derived `rc.M`, and verify the earlier Rule Release remains permanently bound to SHA A.
