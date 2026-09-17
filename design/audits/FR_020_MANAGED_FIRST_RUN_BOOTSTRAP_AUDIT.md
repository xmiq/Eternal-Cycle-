# FR-020 Managed First-Run Readiness and Bootstrap Audit

## Scope

This audit records the owner-authorized pre-v1.1 release-candidate repair prompted by a real local-AI, third-party stdio MCP, and Microsoft SQL Server deployment. It preserves FR-017 through FR-019, the `DIRECT`/`MANAGED` split, **Enumerate -> Select -> Persist -> Reuse**, Managed service rule ownership, MCP's optional-interface status, and the reference-only .NET/T-SQL implementation.

`VERSION` remains `1.0.0`. The immutable `v1.0.0` release history was not changed, Phase 13 remains active, and no next objective was selected.

## Confirmed Root Causes

1. `ec_get_rule_context` entered `SqlServerPublishedRuleStore.GetActiveAsync` and queried `ec_domain.active_rule_releases` before checking whether the Domain Namespace existed.
2. `ec_get_diagnostics` independently queried the same active-release and update-check structures before inspecting schema readiness.
3. Expected first-run absence therefore crossed the MCP boundary as an unclassified exception, which the host reduced to a generic invocation error.
4. The reference README required manual execution of `001_initial.sql` and `002_rule_domain.sql`; it offered no bounded approval-gated bootstrap.
5. `GitRuleSourceProvider` required a pre-existing local `RepositoryRoot`, so the official source could not be selected, acquired, persisted, and reused through a normal first-run flow.
6. Campaign persistence success proved only campaign-schema and transport readiness. It did not establish Rule Domain, source, publication, activation, or overall gameplay readiness.

The observed database containing campaign tables but no rule-domain tables is sufficient to explain both opaque failures. An initialized but empty Rule Store remains a separate supported diagnostic state.

## Implementation

### Readiness and semantic results

`ManagedReadiness.cs` now inspects transport response, persistence connection, campaign schema, Rule Domain schema, source configuration and availability, published releases, active compatibility, requested campaign, and overall readiness before optional rule-store queries. It produces `READY`, `SETUP_REQUIRED`, `MIGRATION_REQUIRED`, `RULE_SOURCE_REQUIRED`, `RULE_PUBLICATION_REQUIRED`, `RULE_ACTIVATION_REQUIRED`, `CAMPAIGN_REQUIRED`, `DEGRADED`, or `ERROR` with sanitized semantic codes.

`ServiceDiagnostics` consumes readiness instead of querying the Rule Store directly. `RuleContextTools` returns a semantic result envelope and never invokes the context provider when readiness blocks retrieval.

### Administrative boundary

`ManagedAdministration.cs` adds separately gated setup capabilities. Service configuration must enable administration, every mutation requires exact explicit approval, and callers cannot submit SQL, schemas, connection strings, or credentials. The bootstrap executor may apply only packaged migrations:

- `001` campaign persistence;
- `002` Rule Domain publication;
- `003` additive campaign display metadata;
- `004` durable Rule Source configuration;
- `005` redacted Managed-operation diagnostics;
- `006` additive Rule Source channel, compatibility-provenance, and causal-diagnostic fields.

Partial unknown schemas produce `MIGRATION_REQUIRED` for administrator review. Additive upgrades are repeat-safe. Post-migration inspection must find no remaining supported plan.

### Rule Source and publication

[`DISTRIBUTION.json`](../../DISTRIBUTION.json) is machine-readable authoritative distribution metadata for the reference package. The setup service may persist that official Git source or an approved custom source. `GitRuleSourceProvider` reuses the stored selection, retains advanced local `RepositoryRoot`, and can acquire or refresh a service-owned cache before resolving an immutable commit SHA.

Initial publication is an explicit approved administrative operation. It does not reinterpret `UpdatePolicy=Disabled` as “already initialized,” enable future updates, weaken candidate validation, or replace a previous valid release on failure.

### Campaign and logging UX

Campaign rows now support optional display names and descriptions. Discovery returns stable IDs plus meaningful presentation, auto-selects one resume candidate, requests a choice among several, and offers permission-gated creation when none exists. Stable IDs remain internal routing and isolation identities.

Optional sanitized file logging supports MCP hosts that hide stderr. It is disabled unless configured and writes message/category/event fields plus exception type, never exception text.

## Schema and Compatibility

The schema changes are additive. New installations receive display metadata in `001`; existing campaign schemas use idempotent `003`. Rule-source selection uses new idempotent `004` after `002`, diagnostics use `005`, and `006` adds channel, discovery-ref, manifest/compiler-contract, and safe causal-diagnostic columns. Prior source rows retain a Stable operational default; unavailable historical discovery and manifest-contract details remain `NULL` rather than being invented. No campaign data migration, database reset, destructive SQL, Direct persistence change, or Rule Packet budget change occurs.

The successful `ec_get_rule_context` response is now wrapped in a semantic result envelope. MCP clients that generated a rigid v1 response type for this optional post-v1 reference tool must regenerate or adapt that binding. Campaign transaction tools and validated receipt contracts are unchanged.

## Documentation

- [Running Eternal Cycle](../../docs/persistence/RUNNING_ETERNAL_CYCLE.md) owns runtime-neutral startup.
- [Player Start and Resume](../../docs/gm/PLAYER_START_AND_RESUME.md) owns normal player intent.
- [Reference Quick Start](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md) provides the zero-to-game deployment path.
- [MCP-Only Acceptance Test](../../examples/tooling/managed-data/mcp-dotnet-tsql/MCP_ONLY_ACCEPTANCE_TEST.md) forbids repository and direct SQL access by the AI.

## Validation

Repository-proven checks:

- .NET build: pass, zero warnings and errors;
- .NET tests: 81 passed, zero failed;
- FR-011 persistence gate: 13 assertions passed;
- FR-017 portable persistence: 29 assertions passed;
- FR-018 rule compilation: 19 assertions passed;
- FR-019 managed architecture: 48 assertions passed after updating its freshness check to the readiness-safe owner;
- FR-020 first-run bootstrap: 59 assertions passed, including repository-relative and standalone distribution metadata packaging, runtime loading, bounded staging, no-checkout acquisition, structured diagnostics, and resumable publication;
- reference MCP build from a different working directory: pass with zero warnings and errors;
- reference MCP publish to a temporary output directory: pass, with `distribution-metadata.json` present in the publish output;
- release-neutral campaign modes: 17 assertions passed;
- full repository validator: pass across 278 Markdown files, 7,059 relative links, 162 anchors, 185 indexed canonical documents, 43 templates, 1,243 terminology checks, 192 roadmap tasks, and 20 Future Revision entries.

Startup and periodic update checks now consult readiness before touching the Rule Store or Rule Source. Expected first-run states defer the background check instead of terminating the service, so diagnostics and permission-gated setup remain reachable under non-default update policies.

Unit tests use in-memory stores, fake readiness/bootstrap services, static migration inspection, and local temporary Git repositories. They prove contract behavior and compilation, not live SQL Server permissions, transactional DDL, remote Git/GitHub acquisition, real MCP schema serialization, authentication, backup/recovery, or client restart behavior.

## Build Regression Repair

The reference MCP project resolves the official `DISTRIBUTION.json` source from `MSBuildThisFileDirectory`, preferring the repository root relative to the project file and falling back to the shipped project-local metadata when the source directory is extracted as a standalone tool. The source target fails only when neither repository nor packaged source exists; it never requires a drive-root file. The project explicitly copies the selected metadata into both build output and publish output and fails those targets if the packaged file is absent. `OfficialDistributionMetadata.Load` continues to resolve relative metadata names from `AppContext.BaseDirectory`, and the managed first-run test proves the packaged test-output file is present and loadable. No developer-specific path is required.

## RC Publication Regression Repair

A subsequent real Managed/MCP and SQL Server acceptance run proved the repaired readiness and degraded diagnostics path, then exposed a separate initial-publication timeout. The managed cache was correctly created with `git clone --no-checkout`; the provider intentionally reads the manifest and rule documents from immutable Git objects, so its empty working tree was not a failed checkout.

Tracing the official source quantified the path:

- 8 declared rule documents and 164,840 source bytes;
- 11 Git processes for clone or fetch, ref resolution, manifest read, and eight document reads;
- 147 compiled chunks;
- 990 selector rows;
- 768 dependency rows;
- 1,906 sequential SQL commands in the prior candidate-staging transaction, before state publication and activation.

The sequential per-record SQL path was the credible implementation-level cause of the greater-than-300-second behavior. Candidate staging now uses bounded multi-row commands: one release command, two chunk batches, four selector batches, and two dependency batches for the official manifest, or 9 staging commands total. It retains the same transaction and rollback boundary.

Publication now reports source acquisition, ref resolution, manifest read, rule-document read, compilation, validation, staging, publication, activation, and update-check stages. Git operations have a bounded timeout and terminate their owned process tree on cancellation. A retry reuses durable source-unique Candidate, Validated, or Published state, including activation retry, instead of inserting a competing release.

Generic exception replacement was removed from this path. Failures receive a correlation ID and structured stage-specific code. Redacted exception type, message, inner chain, stack evidence, versions, relevant stable IDs, duration, and outcome are written to `ec_domain.managed_operation_diagnostics`. When SQL diagnostic persistence fails, the same evidence and correlation ID use a protected local JSON-lines fallback. Failure of both diagnostic sinks cannot mask the original operation. `EternalCycle:Diagnostics:VerboseErrors` defaults to `false`; enabling it changes authorized response visibility but never disables redaction.

The regression suite uses both the actual official manifest and a temporary remote-style `file://` Git source acquired into a real `--no-checkout` managed cache. It covers acquisition, ref, source-read, compile, validate, stage, publish, activate, cancellation, SQL-first diagnostics, file fallback, logging failure, sanitized and verbose output, secret redaction, and idempotent resume.

### Source compatibility and acquisition follow-up

The next real Unsloth Studio, local-Qwen, and SQL Server run confirmed that migration, correlation IDs, publication stages, SQL-first diagnostics, and safe structured failure responses operated as designed. It also proved two separate defects remained:

1. the official default selected immutable `v1.0.0`, but that historical release predates `docs/rules/rule-source-manifest.json` and cannot satisfy the Managed compiler contract;
2. the shared two-minute Git timeout was too short for first remote acquisition on the tested connection, and process cleanup could extend an internal timeout toward the MCP client's outer limit.

The repair leaves product `VERSION` and tag `v1.0.0` unchanged. Distribution metadata now declares compatible Stable and Prerelease Rule Source refs separately from the product release tag. Stable remains the default and fails honestly when no compatible Stable source exists; Prerelease is explicit and resolves its discovery ref to an immutable SHA before compilation. Published releases preserve channel, discovery ref, SHA, manifest format, and compiler contract as separate provenance.

The manifest now declares a portable format version, compiler contract, and RuleSet identity. Missing manifests, unsupported contracts, RuleSet mismatch, and missing declared sources produce non-retryable `RULE_SOURCE_INCOMPATIBLE`. Migration `006` persists this provenance and safe causal-diagnostic data without replacing existing releases or campaign data.

Remote clone and fetch now use a four-minute acquisition timeout while local ref and object operations retain a two-minute process timeout. Process-tree termination, exit waiting, and redirected-reader cleanup are bounded by a five-second grace period. Partial caches are replaced deterministically; complete no-checkout caches and locally verified immutable commits can be reused when the update policy permits. Readiness keeps its broad current classification and exposes the latest relevant causal failure separately so MCP-only clients can distinguish compatibility, acquisition, ref, and source-read problems without filesystem or shell access.

## Remaining Real-Infrastructure Acceptance

A new real deployment must still prove:

- clean and prior-schema SQL Server bootstrap with least-privilege permissions;
- migrations `005` and `006`, including diagnostic insertion/fallback and compatibility-provenance read-back under the deployment identity;
- schema-only migration scope and read-back;
- official remote Git acquisition, separately bounded local Git operations, cache refresh, and bounded cancellation cleanup;
- full initial publication within the target MCP client's timeout using explicit Prerelease selection and an immutable resolved source SHA;
- cancellation and retry against a live SQL Server without duplicate releases;
- source-unavailable fallback after a real successful publication;
- campaign creation and restart discovery;
- semantic MCP results in the target client;
- optional file logging when stderr is hidden;
- an approximately 8K local model using only MCP for rules and campaign state with zero repository-file and direct-SQL reads;
- service/client restart and canonical continuity.

The repository is ready for that RC test, not for a v1.1.0 release claim.

## GitHub Distribution Recommendation

Keep `v1.0.0` immutable and create a GitHub Release from that existing tag if one is still absent. Attach the versioned rules archive, checksum, release notes, and manifest. For v1.1.0, publish both source and a platform-labelled prebuilt Managed/MCP reference artifact; identify its exact canonical rules commit, build provenance, checksum, and supported runtime. Publish official Rule Source identity in `DISTRIBUTION.json` and the release manifest. Stable packages and the ordinary official-source selection default to the matching immutable release tag, while development builds identify an explicit development channel and still compile an exact commit. Cut and publish `v1.1.0` only after the real MCP-only acceptance matrix passes; never move `v1.0.0`.

## Result

FR-020 is complete as repository implementation. Phase 13 remains active through the permanent `[∞]` Future Revisions objective. Real deployment validation remains required before v1.1.0.
