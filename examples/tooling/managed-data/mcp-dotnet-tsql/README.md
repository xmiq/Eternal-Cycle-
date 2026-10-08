# Eternal Cycle Managed Data Service — .NET / MCP / T-SQL Reference

This optional prebuilt tool demonstrates one implementation of the storage-neutral [Managed Data Service](../../../../docs/persistence/MANAGED_DATA_SERVICE.md) contract:

- .NET service runtime;
- MCP gameplay interface;
- Microsoft SQL Server / T-SQL storage adapter;
- Git-backed Rule Source Provider;
- `ec_domain` published Rule Store;
- validated campaign transactions and Persistence Receipts.

It is not the Eternal Cycle persistence architecture itself. A compliant Managed service may use another interface or structured datastore. This project is neither a campaign, rules authority, nor gameplay engine.

## Zero-to-Game Quick Start

### Player path

1. Ask a compatible AI client to connect to the installed Eternal Cycle Managed service.
2. Say: **“Start a new Eternal Cycle game.”**
3. If the service reports setup is required, review its plain-language EC-owned setup summary and approve only if it matches the intended database.
4. Select the packaged default Rule Source or another compatible source you choose.
5. After the service reports `READY`, continue character and campaign bootstrap normally.

To resume, say: **“Continue my Eternal Cycle game.”** One suitable campaign is selected automatically; several are presented by meaningful name. Internal Campaign IDs remain backstage.

### Administrator path

1. Install .NET 10 and provide an existing SQL Server database plus a least-privilege connection with permission to create or upgrade the configured Eternal Cycle schemas.
2. Set `EternalCycle:Persistence:ConnectionString` in protected host configuration.
3. Enable the separate setup surface with `EternalCycle:Administration:Enabled=true`. Natural informed user approval is the normal path; optionally enable a private operator confirmation as an additional deployment safeguard.
4. Add the built executable as an stdio MCP server in the compatible client, then call readiness or ask the AI to start a game.
5. Preview setup, obtain explicit user approval, and run the permission-gated initialization. The service applies only packaged migrations `001` through `010` to configured EC-owned scopes.
6. Select the packaged default from [`DISTRIBUTION.json`](../../../../DISTRIBUTION.json) metadata or another compatible Git source, including an existing local clone. The selection persists in `ec_domain`; source location does not change the technical validation pipeline.
7. Approve initial publication. The call returns a durable Operation ID promptly; an independent Managed Worker acquires/caches the source, resolves immutable provenance, compiles, validates, prepares the minimum closure, publishes, activates according to policy, and continues preparing remaining rules even if the MCP transport exits.
8. Retrieve the canonical GM Host Bootstrap, install it in the host's highest supported instruction field, and confirm naturally. Generic confirmation records `UserConfirmed`; only a capable host integration may record `Verified`.
9. Disable administrative setup after provisioning when ongoing administration is handled elsewhere.

The reference service normally launches that worker automatically. If the Windows host refuses Job Object breakaway, the response reports `WORKER_INDEPENDENT_LAUNCH_BLOCKED` and prepares `Continue Eternal Cycle Setup.bat` in the configured fallback directory (the current user's Desktop by default). Double-click it once. It contains the existing operation handoff and a locator for temporary protected effective configuration; it asks for no Operation ID, connection string, source, ref, Campaign ID, terminal command, or administrator privilege. A confirmed handoff removes the temporary configuration and launcher. A failed handoff preserves them for retry and diagnostics.

The ordinary player never needs SSMS, migration filenames, schema names, Git commands, `RepositoryRoot`, Rule Release IDs, or MCP tool names. Manual SQL and local-checkout configuration below remain advanced development and recovery paths.

## Implemented Capabilities

- semantic status, exact record read, complete commit, and idempotent retry tools;
- staged append-only candidates, expected-parent concurrency, validation, activation read-back, and receipts;
- trusted Campaign ID -> World Model -> Logical Data Namespace -> SQL schema routing;
- strict SQL identifier validation and quoting;
- Git commit-SHA source acquisition through an administrative Rule Source Provider;
- versioned rule candidates, validation, publication, atomic activation, and active-release fallback;
- runtime retrieval from an already-published Rule Release rather than repository compilation;
- source provenance, World/Ruleset/module/mode/operation/topic filtering, and an 8K packet ceiling;
- automatic Runtime Kernel plus GM Runtime Procedure selection for `gameplay.resolve`, with a compact model-facing Rule Packet and full service-side provenance;
- durable GM Host Configuration readiness with exact canonical bootstrap presentation and distinct user-confirmed versus verified states;
- constrained set-only patch updates for existing authoritative records through the normal concurrency, validation, activation, and receipt path;
- startup, periodic, manual, and offline update policies;
- sanitized capabilities and diagnostic reports;
- structured first-run readiness across transport, persistence, campaign schema, Rule Domain, source, publication, activation, and campaign state;
- permission-gated EC-owned migrations, persisted source selection, managed Git acquisition/cache, initial publication, and campaign discovery/creation;
- bounded rule-publication batches, stage-aware structured failures, and idempotent publication resume;
- durable queued/running/completed Managed Operations with deduplication, transport-independent worker execution, independent status discovery, renewable execution leases, orphan reconciliation, and service-owned timeouts;
- fresh-database worker startup that waits for authorized migration 007 instead of requiring the operation tables to pre-exist;
- progressive per-source readiness, dependency-aware minimum closure, manifest preparation tiers, and gameplay-driven priority boosts;
- SQL-first Managed-operation diagnostics with a zero-configuration protected local physical fallback;
- a database-independent configuration contract, static semantic error registry, bounded portable Error Dump, and final tool exception boundary;
- structured pre-migration recovery that does not query post-migration operation tables;
- optional sanitized general file logging for hosts that hide stderr.

The reference does not implement PostgreSQL, MySQL, document, graph, key/value, or indexed-file adapters. Those are contract-compatible extension families, not claimed implementations.

## Gameplay MCP Tools

| Tool | Purpose |
| --- | --- |
| `ec_service_capabilities` | Identify this reference implementation and its semantic capability set. |
| `ec_get_configuration_requirements` | Return exact safe configuration keys, environment names, status, defaults, accepted forms, and restart requirements without requiring a database. |
| `ec_persistence_status` | Return service-backed campaign status. |
| `ec_read_records` | Read exact canonical owner-domain and stable-record addresses. |
| `ec_commit_changes` | Stage, validate, activate, read back, and receipt one complete transaction. |
| `ec_patch_records` | Apply bounded set-only patches to existing authoritative records while preserving omitted fields and the ordinary transaction contract. |
| `ec_retry_persistence` | Resume a stable transaction without replaying gameplay. |
| `ec_get_rule_context` | Retrieve an already-published bounded Rule Packet. |
| `ec_get_diagnostics` | Return sanitized implementation, rules, source, namespace, and update provenance. |
| `ec_list_error_codes` | List the static semantic error registry without requiring a database or Campaign ID. |
| `ec_get_error_code` | Explain one stable error code and its retry, intervention, and recovery semantics. |
| `ec_get_error_dump` | Return a bounded sanitized structured-and-text diagnostic snapshot; partial evidence is valid. |
| `ec_get_readiness` | Return structured setup and gameplay readiness without mutation. |
| `ec_get_gm_host_configuration` | Return the exact canonical Host Bootstrap and record that setup presented its current revision. |
| `ec_confirm_gm_host_configuration` | Record natural user confirmation as `UserConfirmed`, never as technical verification. |
| `ec_get_operation_status` | Return durable operation state, current stage, causal status, and result identity. |
| `ec_list_managed_operations` | Rediscover recent operations after a client disconnect or lost immediate response. |
| `ec_stop_managed_operation` | Request orderly cancellation from the independent worker without editing durable state. |
| `ec_get_setup_plan` | Preview packaged EC-owned migrations. |
| `ec_list_campaigns` | List meaningful campaign names with stable internal IDs. |
| `ec_resolve_resume_campaign` | Select the sole campaign or return meaningful choices. |

Administrative tools are separately configuration-gated and require explicit informed user approval. An exact operator phrase is optional and, when configured, is an additional administrative safeguard rather than ordinary player UX:

| Tool | Purpose |
| --- | --- |
| `ec_initialize_service` | Apply and validate only packaged EC-owned migrations. |
| `ec_configure_rule_source` | Persist the packaged Stable/Prerelease default or another user-selected compatible Git source/ref. |
| `ec_publish_initial_rules` | Create or reuse a durable initial-publication operation and return promptly. |
| `ec_create_campaign` | Create one stable campaign identity in the trusted default namespace. |

No tool accepts arbitrary SQL, a caller-supplied schema, a connection string, or a credential. Source publication and schema mutation remain administrative responsibilities even when exposed through the explicitly gated setup surface.

## Durable Campaign Binding Checkpoint

FR-027B implements routing and C implements the trusted interaction control plane; [D-G](../../../../design/FR_027_EXECUTION_PLAN.md) still own entry, mutation/yield, trace and live acceptance. No `ec_begin_gameplay_interaction` tool or gameplay authorization is available from a binding or intake record. Legacy setup, persistence and retrieval remain unchanged; C does not gate existing gameplay writes.

The deployment configures `EternalCycle:CampaignBinding:Enabled`, `PrincipalId`, `LogicalSessionId`, a non-secret logical `AuthorityId`, the explicit `AuthorizedCampaignIds` array, and separate `AllowSoleCampaignResume`, `AllowSelection`, `AllowSwitch` grants. Configuration discovery exposes their exact environment keys without returning principal/session/access values. The configured logical session must survive reconnect; it is not a PID, conversation fingerprint, tool argument or attestation of a real player input. An empty access array grants no access. A multi-user host must provide an authenticated per-session adapter rather than sharing these single-session settings between users.

`ec_resolve_campaign_binding` recovers/verifies the current binding, or returns zero/one/multiple eligible campaign results and meaningful safe choice handles. `ec_select_campaign_binding` takes the selected current handle, stable Request ID, expected Binding ID/revision and actual selection approval. `ec_switch_campaign_binding` creates a successor, never retargets the old handle; `ec_suspend_campaign_binding` preserves history. Technical identities stay in machine evidence, not primary player UX. None accepts story text or a caller-supplied session/principal.

Additive [013](src/EternalCycle.Persistence.Mcp/Schema/013_campaign_session_binding.sql) and its [routed template](src/EternalCycle.Persistence.Mcp/Schema/013_campaign_session_binding.template.sql) create only binding aggregates and retry receipts. Setup offers this migration only when binding is enabled. No existing campaign is guessed or rewritten. Repeat-safe upgrade preserves all save/rule history; downgrade uses verified backup/controlled restore, not a promised down migration.

Evidence covers configured authority/namespace/world/schema/rules profile, active validated Campaign Version, active selected legacy release or compiled artifact, immutable source identity, minimum preparation, bootstrap hash/state/revision and incomplete save transaction reference. `UserConfirmed` is not `Verified`; binding validity is not full gameplay readiness or an interaction grant. D must still perform entry/rule/Canon verification.

Production switching and changed-profile reconfirmation now use C's `ISqlCampaignBindingSwitchSafety` under the same transaction-owned session lock as interaction admission. Disabled/missing interaction storage remains unknown and blocked. Unresolved interactions, Pending decisions and unknown persistence block; all non-Completed existing save transactions still conservatively block. The old B test substitute remains only historical successor-storage evidence. No old input, pending write or decision moves to a successor.

See [B acceptance](../../../../design/audits/FR_027B_DURABLE_CAMPAIGN_BINDING_ACCEPTANCE.md) for exact SQL/process tests and limits.

## Trusted Player Interaction Checkpoint

Enable `EternalCycle:PlayerInteraction:Enabled` together with binding to preview/apply additive [014](src/EternalCycle.Persistence.Mcp/Schema/014_player_interactions.sql) and its [template](src/EternalCycle.Persistence.Mcp/Schema/014_player_interactions.template.sql). This provisions storage only, not trusted origin or gameplay entry. `AllowRecoveryCancellation` separately grants trusted internal recovery coordinators cancellation with known persistence and explicit decision abandonment; default false. Neither setting is a player choice, test credential or human-event attestation.

An authorized host integration implements `ITrustedPlayerSubmissionIngress` and invokes `PlayerInteractionService.AcceptAsync` outside the model-facing tool surface. It resolves an authenticated actual event, stable principal/session/submission identity, exact binding, protected immutable input/hash and optional exact pending-decision relationship. No Git, network, model inference, conversation search or arbitrary tool token supplies origin. Separate actual submissions with identical text remain separate; redelivery reuses the original interaction and conflicting evidence fails.

**No production human-event adapter exists for this stdio transport.** No environment flag downgrades that boundary. The service returns `TRUSTED_SUBMISSION_REQUIRED` for intake without an adapter; the deterministic adapter is test-assembly-only. `ec_get_player_interaction_status` reads authorized safe status, current versus historical binding identity and decision disposition across reconnect. It cannot create submissions, set lifecycle state, resolve options or grant entry/mutation. Protected input/question references are not returned in that model projection.

Intake is RECEIVED. Internal owner methods prepare ENTRY_PENDING, record entry-only clarification with known persistence, block/resume the original entry gate, or explicitly cancel/abandon under recovery policy. C has no OPEN or gameplay completion writer. A linked reply leaves its decision Pending, not selected; later owners must classify the response and atomically seal answered/superseded disposition. D adds conditional entry/OPEN below; E must still connect every gameplay write/yield path. See [C acceptance](../../../../design/audits/FR_027C_TRUSTED_PLAYER_INTERACTION_ACCEPTANCE.md) for the full host boundary and actual SQL/process evidence.

## Mandatory Gameplay Entry Checkpoint

`ec_begin_gameplay_interaction` accepts a stable `RequestId`, the original `ExpectedRevision`, an optional owned `InteractionId`, and bounded `QueryTerms` hypotheses. No Campaign ID, rule identity, origin claim, choice or completeness flag is accepted. Deployment enables `EternalCycle:GameplayEntry:Enabled` alongside B/C and previews/applies additive [015](src/EternalCycle.Persistence.Mcp/Schema/015_gameplay_entry.sql) / [template](src/EternalCycle.Persistence.Mcp/Schema/015_gameplay_entry.template.sql). Default is disabled. `MaximumEstimatedRuleTokens` defaults to the existing 8,000 ceiling; required overflow blocks rather than trims.

The reference exact-owner adapter needs a trusted `EternalCycle:GameplayEntry:CurrentSessions:<campaign-id>:OwnerDomain` / `RecordId` map into existing Campaign Canon. It never guesses that address or writes a Current Session. The canonical payload projects `GameplayEntrySessionPlan`: the actual input's `InputSha256`, authoritative `CampaignMode`/`ModuleIds`, bounded canonical `QueryTerms`, explicit specialist `RequiredRuleSourceIds`/`RequiredSnippetIds`, and `Reads`. Each read declares a role, exact owner/address, visibility and optional expected revision, or explicit legitimate absence with a reason. Mandatory roles are PlayerState, Controller, Protagonist, Incarnation, Time, Scene and Placement, plus CurrentSession itself. RelevantOwner obligations cover additional declared-intent owners. This is a reference .NET projection, not a universal Canon layout, a new Canon owner or a source inference engine. A provider adapts its actual authoritative session representation through `IGameplayEntryCanonReader`; incompatible/missing representation fails closed. Preparing intent-bound canonical evidence and trusted host input is integration work, not a model-controlled plan argument or automatic Canon mutation.

The service reads at most 32 obligations and 64 KiB of authorized Canon through existing exact-record APIs before using scene signals. It fixes `gameplay.resolve` against the bound active compiled artifact (or explicitly selected legacy release), verifies mandatory Kernel/Procedure and required specialists, then rechecks binding/generation/revision, version/save readiness, bootstrap, pending decision and record revisions/hashes transactionally. Receipt plus OPEN commits atomically; preparation and failed/missing reads remain ENTRY_PENDING. No long SQL transaction spans context gathering. A successful duplicate returns original evidence and no new context, baseline or action. Entry status is historical evidence, never a standalone mutation grant; both mutation-authorized fields remain false.

**This is an opt-in entry integration checkpoint, not yet a supported ordinary gameplay route.** `ec_commit_changes`, `ec_patch_records`, `ec_retry_persistence` and their coordinator/store paths still lack E's interaction admission/yield checks. No real stdio human-event adapter has been added. Full GM Turn Trace remains F and scalable Canon closure FR-028. See [D acceptance](../../../../design/audits/FR_027D_MANDATORY_GAMEPLAY_ENTRY_ACCEPTANCE.md) for executed SQL/process tests, conservative baseline policy and E handoff.

## Build and Test Commands

```powershell
dotnet build examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/EternalCycle.Persistence.Mcp.csproj
dotnet test examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/EternalCycle.Persistence.Mcp.Tests.csproj
```

The normal test run does not require SQL Server. To run the genuine pre-007 LocalDB migration regression on Windows:

```powershell
$env:ETERNAL_CYCLE_RUN_SQL_INTEGRATION = "1"
dotnet test examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/EternalCycle.Persistence.Mcp.Tests.csproj --filter FullyQualifiedName~PreMigrationControlPlaneIntegrationTests
```

## T-SQL Namespaces

First-run bootstrap applies and validates these assets after approval. Advanced/manual recovery may apply:

1. [`001_initial.sql`](src/EternalCycle.Persistence.Mcp/Schema/001_initial.sql) for the legacy-compatible `ec` campaign schema, or render [`001_initial.template.sql`](src/EternalCycle.Persistence.Mcp/Schema/001_initial.template.sql) for a trusted World Data Namespace mapping;
2. [`002_rule_domain.sql`](src/EternalCycle.Persistence.Mcp/Schema/002_rule_domain.sql) for the reference `ec_domain` Domain Namespace, or render its template under an approved physical mapping policy.
3. [`003_campaign_directory.sql`](src/EternalCycle.Persistence.Mcp/Schema/003_campaign_directory.sql) to add player-meaningful campaign metadata to a prior default deployment.
4. [`004_rule_source_configuration.sql`](src/EternalCycle.Persistence.Mcp/Schema/004_rule_source_configuration.sql) to persist the selected Rule Source without storing source credentials.
5. [`005_managed_operation_diagnostics.sql`](src/EternalCycle.Persistence.Mcp/Schema/005_managed_operation_diagnostics.sql) to preserve redacted operation evidence with correlation and publication-stage metadata.
6. [`006_rule_source_compatibility.sql`](src/EternalCycle.Persistence.Mcp/Schema/006_rule_source_compatibility.sql) to add source-channel, discovery-ref, manifest-contract, and safe causal-diagnostic provenance.
7. [`007_durable_managed_operations.sql`](src/EternalCycle.Persistence.Mcp/Schema/007_durable_managed_operations.sql) to add durable operations, per-source preparation state and priority, and human-readable prerelease metadata while preserving source SHA identity.
8. [`008_diagnostic_operation_correlation.sql`](src/EternalCycle.Persistence.Mcp/Schema/008_diagnostic_operation_correlation.sql) to join durable Operation IDs directly to persisted diagnostic evidence.
9. [`009_managed_operation_execution_leases.sql`](src/EternalCycle.Persistence.Mcp/Schema/009_managed_operation_execution_leases.sql) to add renewable worker ownership, bounded orphan detection, and execution-attempt evidence without replacing operation identity.
10. [`010_gm_host_configuration.sql`](src/EternalCycle.Persistence.Mcp/Schema/010_gm_host_configuration.sql) to persist Ruleset-scoped bootstrap revision and `Required`, `InstructionsPresented`, `UserConfirmed`, or `Verified` readiness without storing host prompts or credentials.
11. [`011_compiled_artifact_import.sql`](src/EternalCycle.Persistence.Mcp/Schema/011_compiled_artifact_import.sql), or its routed [template](src/EternalCycle.Persistence.Mcp/Schema/011_compiled_artifact_import.template.sql), to retain artifact-owned format-1 candidates losslessly without changing legacy publication/history or active pointers.

New deployments should prefer the `ec_` discoverability convention, such as `ec_mainworld` or `ec_fantasyworld`. Existing `ec` deployments remain supported. Physical schema names do not become Campaign IDs, World IDs, RuleSet IDs, or Logical Data Namespace IDs.

`ec_domain` stores only shared service data and Derived rule publications/candidates: RuleSets, Rule Releases, compiled chunks, selectors, dependencies, active-release pointers, provenance, update checks, redacted Managed-operation diagnostics, and complete imported format-1 artifacts. World-specific Campaign Canon remains in the selected world schema.

## Compiled Artifact Import

FR-025F adds a configured `SqlServerCompiledRulesArtifactStore` implementing the [portable import contract](../../../../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md#shared-authorized-import). Construct it with trusted persistence options and the authorized Ruleset ID; pass only `ValidatedTrustedCompiledRulesArtifact` from B through `CompiledRulesArtifactImport.ImportAsync`. F adds no MCP command or default trust policy. The importer neither fetches Rule Sources nor publishes/activates its stored candidate.

Migration 011 adds artifact headers/exact bytes, ordinal source and snippet records, and artifact-local dependency edges. Full selectors, hashes, preparation metadata and retrieval term/kind/weight/relationship arrays remain in validated normative JSON, not lossy legacy chunks. A serializable transaction and deterministic key enforce atomic/idempotent import; readback must match both FR-022-valid exact bytes and the complete semantic projection. Formatting-only reimports retain the first approved bytes/hash. Raw SQL exceptions and connection details never appear in compact failures/receipts. Legacy source-based compilation/publication, active state and campaign schema remain unchanged.

Upgrade is additive/repeat-safe; no down migration is promised. Keep a verified deployment backup and use controlled restore for rollback. Missing 011 returns `ARTIFACT_IMPORT_SCHEMA_INCOMPATIBLE`; setup planning offers the migration, without making import capability a new gameplay-readiness prerequisite. [F's audit](../../../../design/audits/FR_025F_LOSSLESS_IMPORT_AUDIT.md) documents storage; [G's closure audit](../../../../design/audits/FR_025_ACQUISITION_AND_IMPORT_AUDIT.md) proves integrated local/simulated GitHub/custom import, rollback, offline approval and unchanged public current-rule context against real disposable LocalDB. No production import endpoint or FR-026 ranking is added.

Run the focused SQL suite only against its automatically created disposable test databases:

```powershell
$env:ETERNAL_CYCLE_RUN_SQL_INTEGRATION = "1"
dotnet test examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/EternalCycle.Persistence.Mcp.Tests.csproj -c Release --filter "FullyQualifiedName~CompiledRulesArtifactSqlImportTests|FullyQualifiedName~CompiledRulesArtifactImportPackagingTests"
```

With the opt-in absent, SQL cases skip and the database-free migration-packaging check still runs. Tests never select an existing campaign database.

## Trusted Configuration

The connection string remains external:

```text
EternalCycle:Persistence:ConnectionString
```

Reference routing supports:

```text
EternalCycle:Persistence:DomainSchema = ec_domain
EternalCycle:Persistence:DefaultDataNamespaceId = eternal-cycle-mainworld
EternalCycle:Persistence:DefaultSchema = ec
EternalCycle:Persistence:DefaultWorldModelId = eternal-cycle-standard
EternalCycle:Persistence:DefaultRulesetId = eternal-cycle-core

EternalCycle:Persistence:CampaignWorldModels:<campaign-id> = <world-model-id>
EternalCycle:Persistence:WorldDataNamespaces:<world-model-id> = <namespace-id>
EternalCycle:Persistence:DataNamespaces:<namespace-id>:SchemaName = <validated-schema>
EternalCycle:Persistence:DataNamespaces:<namespace-id>:SchemaModelVersion = <version>
EternalCycle:Persistence:DataNamespaces:<namespace-id>:RulesetId = <ruleset-id>
EternalCycle:Persistence:DataNamespaces:<namespace-id>:RulesetVersion = <version>
```

The FR-018 `WorldSchemas` mapping remains accepted as a compatibility input. New configuration should use explicit Logical Data Namespace IDs.

Rule publication configuration includes:

```text
EternalCycle:Rules:RulesetId = eternal-cycle-core
EternalCycle:Rules:CompilerVersion = 1
EternalCycle:Rules:ReleaseChannel = Stable | Prerelease
EternalCycle:Rules:MaximumEstimatedTokens = 8000
EternalCycle:Rules:UpdatePolicy = Disabled | Startup | Periodic | Manual
EternalCycle:Rules:ActivationPolicy = Automatic | Manual
EternalCycle:Rules:ManagedOperationTimeout = 00:30:00
EternalCycle:Rules:ManagedOperationPollInterval = 00:00:01
EternalCycle:Rules:ManagedOperationLeaseDuration = 00:00:30
EternalCycle:Rules:CampaignPinnedRuleReleaseIds:<campaign-id> = <published-release-id>
EternalCycle:Rules:GitSource:RepositoryRoot = <trusted Git checkout>
EternalCycle:Rules:GitSource:Ref = <trusted branch, tag, or commit>
EternalCycle:Rules:GitSource:ManifestPath = docs/rules/rule-source-manifest.json
EternalCycle:Rules:GitSource:FetchBeforeCheck = false
EternalCycle:Rules:GitSource:AcquisitionTimeout = 00:04:00
EternalCycle:Rules:GitSource:ProcessTimeout = 00:02:00
EternalCycle:Rules:GitSource:TerminationGracePeriod = 00:00:05
```

.NET configuration maps these names to environment variables naturally. For example, current v1.1 release-candidate testing uses `EternalCycle__Rules__ReleaseChannel=Prerelease`; the two timeout settings become `EternalCycle__Rules__GitSource__AcquisitionTimeout` and `EternalCycle__Rules__GitSource__ProcessTimeout`.

Administration configuration includes:

```text
EternalCycle:Administration:Enabled = false
EternalCycle:Administration:RequireOperatorConfirmation = false
EternalCycle:Administration:ApprovalPhrase = <optional private operator confirmation>
EternalCycle:Administration:ManagedRuleCacheDirectory = <optional managed cache>
EternalCycle:Administration:SanitizedLogFile = <optional log file>
```

Managed diagnostic configuration includes:

```text
EternalCycle:Diagnostics:VerboseErrors = false
EternalCycle:Diagnostics:FallbackLogFile = <optional protected path>
EternalCycle:Diagnostics:DisableAutomaticFallback = false
EternalCycle:Diagnostics:PersistenceTimeout = 00:00:10
```

Independent worker configuration includes:

```text
EternalCycle:ManagedWorker:ExecutablePath = <optional packaged-worker override>
EternalCycle:ManagedWorker:ControlDirectory = <optional non-authoritative local control directory>
EternalCycle:ManagedWorker:FallbackLauncherDirectory = <optional one-click launcher directory>
EternalCycle:ManagedWorker:FallbackHandoffTimeout = 00:00:30
```

The packaged `EternalCycle.ManagedWorker.exe` is a second app host for the same validated execution code. MCP persists or reuses the operation and launches the worker; SQL operation state, leases, attempts, diagnostics, and results remain authoritative. The local lock, stop, progress, handoff, and temporary fallback files are not a second operation store. The Windows automatic path uses a process created outside the restrictive host Job Object and a short-lived command trampoline so recursive transport-tree termination does not own the worker lifetime. If breakaway is refused, the service does not silently downgrade to a transport-owned process.

`VerboseErrors` defaults to `false`. It adds useful redacted exception detail to authorized administrative failures but never exposes credentials, changes transaction behavior, or controls whether diagnostics are preserved. If SQL diagnostic insertion fails, the automatic default fallback is `%LOCALAPPDATA%\EternalCycle\logs\managed-diagnostics.jsonl` on Windows or the platform-equivalent local application-data directory. `FallbackLogFile` is an advanced path override; `DisableAutomaticFallback=true` explicitly disables this safety net. The fallback retains the operation correlation ID. Failure of both sinks never masks the publication failure.

Call `ec_get_configuration_requirements` to discover all exact configuration and environment-variable names (`:` becomes `__`), validation status, accepted forms, effective Managed Rule Source cache root, and restart requirements. Sensitive values are never returned. A generic startup error is not a substitute for this contract.

The packaged `distribution-metadata.json` identifies the official repository, historical product release tag, compatible Stable and Prerelease Rule Source refs, and source manifest. Product version and Rule Source channel are independent: `VERSION` remains `1.0.0`, and the immutable `v1.0.0` tag remains historical provenance even though it predates the Managed manifest contract. Stable is the default and never falls forward to unreleased content. If no compatible Stable source is published, setup returns `RULE_SOURCE_INCOMPATIBLE`; an administrator must explicitly select Prerelease for current development testing.

Eternal Cycle full-release tags are immutable. The configured `v1.1.0-rc` tag is a deliberately moving discovery pointer and is not created or moved by this implementation task. For an official Prerelease snapshot, the service records Base Release, Discovery Tag, commits since base, derived display version such as `1.1.0-rc.47`, and the exact source commit. The commit SHA is identity; the display version is metadata and may collide across rewritten histories.

The reference project resolves distribution metadata from the repository root when built in a full checkout and from its shipped project-local `DISTRIBUTION.json` when extracted as a standalone tool; it does not depend on the caller's working directory or a machine-specific path. A persisted source selection takes precedence for managed acquisition. An existing local clone is a first-class source and is not treated as a validation bypass. The provider resolves the discovery ref once, records the exact commit SHA as immutable publication provenance, and materializes only the manifest, declared rule documents, and compact provenance metadata into its runtime cache. Moving the discovery ref later does not rewrite an existing Rule Release.

The manifest declares its format, compiler contract, RuleSet, repository version, and required sources. Missing or unsupported manifest contracts fail as non-retryable `RULE_SOURCE_INCOMPATIBLE`, rather than as transient network failures. `AcquisitionTimeout` bounds the complete acquisition while `ProcessTimeout` bounds each Git subprocess. Cancellation attempts to terminate the process tree and bounds cleanup and redirected-reader waits by `TerminationGracePeriod`. Exact immutable payloads can be reused offline. Provider-owned `.acquire-*` workspaces and interrupted partial snapshots are discarded conservatively before retry; valid snapshots, unrelated directories, and historical cache roots are not swept. Each Git invocation emits one structured terminal diagnostic containing a Git invocation ID, exact sanitized logical command, working directory, resolved executable and PID when available, start/end timing, elapsed time, exit evidence, outcome, timeout scope, cancellation source observed at that layer, and retry relevance. Credentials, tokens, authorization headers, sensitive query parameters, stdout, and unbounded stderr are excluded. `ParentToken` remains honest when only the durable parent can identify the originating cancellation. A failed source check or candidate preserves the active validated Rule Release. `Disabled` means an explicit administrator chose offline update behavior; it does not prevent an approved one-time initial publication.

Runtime Rule Source acquisition and repository distribution packaging are independent. The service cache is a minimal manifest-defined rules payload comparable to the rules content itself; it does not retain the reference MCP source, tests, audits, or development tooling. `tools/build_distribution.ps1` instead creates the full useful review/audit repository ZIP and intentionally includes authored reference tooling while excluding generated `bin/`, `obj/`, caches, temporary output, credentials, and campaign data.

General sanitized host logging is disabled unless `SanitizedLogFile` is set. It records timestamp, level, category, event, message, and exception type, but omits exception text. This is separate from the Managed-operation diagnostic fallback, which is available when SQL diagnostic persistence fails. Both paths exclude credentials, connection strings, Campaign Canon, and GM Secrets.

The service is licensed under the repository's [Apache License 2.0](../../../../LICENSE). Packaged output includes `LICENSE`, `NOTICE`, and `distribution-metadata.json`. `NOTICE` identifies the original project and explains that derivative distributions are independent and do not imply endorsement, warranty, or support. Dependencies retain their own licenses; this repository does not relicense third-party packages.

## Campaign Transactions

Candidate campaign records remain inactive until the complete Affected Set validates and atomically advances the campaign's active version. A validated Persistence Receipt is the Managed completion evidence. Retry resumes the same transaction and never reapplies gameplay effects.

## Security

- MCP callers never submit SQL, schemas, namespace mappings, source paths, or credentials.
- Campaign ID isolation is enforced independently of namespace routing.
- Configurable identifiers are strictly validated and safely quoted.
- Gameplay tools have no source-publication or database-administration authority.
- Setup operations require service-side enablement and explicit informed user approval; deployments may separately require operator confirmation. They can execute only packaged migrations and bounded domain operations.
- Diagnostic output excludes connection strings, credentials, locators, Campaign Canon, GM Secrets, and conversations.

## Explicit Compiled Artifact Runtime

FR-026F adds a separately selected compiled-artifact path, not an automatic replacement of `ec_get_rule_context`. Set `EternalCycle:CompiledRules:Enabled` (environment equivalent `EternalCycle__CompiledRules__Enabled`) only for an authorized deployment. It defaults to false and grants runtime reads solely within the configured `EternalCycle:Rules:RulesetId`. Import/admin rights alone do not enable it. Multi-user deployments must authenticate and establish grants before calling these reference APIs; this opt-in is not a multi-user authentication system.

When enabled, schema readiness/bootstrap additionally requires the packaged additive `012_compiled_artifact_publication.sql` (and configurable-schema template). Migration 011 remains the complete artifact storage format. 012 adds artifact-owned publication and one active compiled pointer per Ruleset, without changing the legacy release pointers or any Campaign. Supported bootstrap still requires its existing informed approval and operator safeguards.

Integration callers separately invoke `CompiledRuleRuntime.PublishAsync` and `ActivateAsync` with the exact Ruleset/semantic artifact scope and informed consent; existing administration enablement and optional operator confirmation apply. These are library integration APIs, not new administrative MCP tools. Broader administrative tool exposure remains FR-032. Import never publishes or activates. Historical imports remain readable through the existing authorized admin store API; runtime reads require the explicitly requested artifact to be published and active.

`ec_get_compiled_rule_context` accepts the portable `CompiledRuleRetrievalRequest`: explicit scope, operation, mode, World/modules/topics/query entries and packet budget. It uses the same A-E engine and returns only E's compact packet. Rich root/ranking/dependency/exclusion evidence stays in the service `GetContextAsync` result. A successful packet establishes selected rule-context readiness, not whole-game `GameplayReady`. Gameplay resolution also requires a durable UserConfirmed/Verified Host Bootstrap acknowledgment matching this artifact's bootstrap source hash. No Campaign-to-artifact binding is inferred.

Runtime uses six parameterized set-based SELECTs in one serializable transaction: scope, eligibility and the four existing FR-025 complete readback queries. Ordered JSON/edges reconstruct format 1 and are compared with retained exact bytes, byte hash and semantic digest before A-E runs. Gameplay additionally reads the stored host acknowledgment. There is no per-snippet query, provider reacquisition, term index or global cache. Cost/optimization acceptance remains FR-026G.

Imported artifacts are already complete and do not invent new progressive preparation records. Missing/inactive/corrupt selection, wrong scope, missing schema or host confirmation fail safely without partial content or fallback. Legacy source preparation/Pending/priority and all legacy retrieval remain intact. See the [F audit](../../../../design/audits/FR_026F_SQL_RUNTIME_RETRIEVAL_AUDIT.md) for real disposable SQL evidence and remaining acceptance limits.

## Gameplay Interaction Authority

The opt-in FR-027 reference gameplay route requires configured `CampaignBinding`, `PlayerInteraction` and `GameplayEntry`, a trusted host input adapter, and an authoritative Current Session projection. Stdio tool arguments are not authenticated player submissions. Missing ingress/projection fails closed; the model cannot prepare its own authority. The [E audit](../../../../design/audits/FR_027E_GAMEPLAY_MUTATION_DECISION_YIELD_ACCEPTANCE.md) inventories every mutation route and records actual SQL/process evidence.

`GameplayEntrySessionPlan.MutationScope` declares at most 32 exact record/action permissions (`Update`, `Create`, `Tombstone`) for the actual input. D freezes these into its existing receipt. Current Session cannot authorize its own rewrite. Owner revision/hash evidence must exist; `ec_read_gameplay_owners` can extend exact reads within that scope, including explicitly permitted creation absence, but cannot expand intent. This projection is a reference adapter contract, not a universal Canon schema or automatic intent classifier.

Existing `ec_commit_changes`, `ec_patch_records` and `ec_retry_persistence` now share the persistence store's final session-owned transactional authority checks. They require the original trusted OPEN interaction, current binding/generation, D receipt, unchanged profile/bootstrap, current verified baseline and exact authorized reads/actions. Staging freezes the transaction and enters PERSISTING; activation and validated acknowledgment recheck authority. Unknown/failed outcomes remain PERSISTING or BLOCKED with original recovery evidence. Retries reconcile only the admitted payload. A validated save unit returns `GameplayCompletionRequired=true`, `TurnMayComplete=false`; it is not completed narration.

- `ec_conclude_gameplay_interaction` verifies an explicit Affected Set and all validated receipts (or explicit no-change evidence), then atomically seals completion or records protected alternatives and yields before authorizing their presentation.
- `ec_resolve_player_decision` accepts identities/revisions only, never a model-authored answer. It consumes the new entered response's trusted classification. Clarification leaves the decision pending; choice, rejection, correction and changed direction preserve actual response provenance without executing a fictional option automatically. The originating interaction stays yielded.
- `ec_read_gameplay_owners` reads exact additional authorized owners and records versioned read evidence; it is not a read-only control-plane operation or FR-028 graph loader.
- `ec_reconcile_gameplay_persistence` recovers the original admitted transaction. The database remains authoritative after reconnect; no new input or effect is created.

There is no implicit administrative Canon-write bypass. A **separate** non-gameplay deployment may configure `EternalCycle:GameplayAuthority:AdministrativeCampaignIds` for exact authorized campaigns. This is trusted deployment configuration, not a tool argument. It is ignored whenever gameplay binding is enabled. Migration/bootstrap, rule import/publication, workers and read-only operations retain their existing separate authority. Do not grant this administrative policy to a player-hosted deployment to evade entry.

No new migration is needed: bounded progress/response evidence uses schema 014 aggregates/transition receipts, D scope uses schema 015 receipt JSON, and Canon uses the existing save transaction/receipt ledger. SQL history is preserved. Hosts must consume `CompletedNarrationAuthorized` and `QuestionPresentationAuthorized`; MCP cannot physically prevent arbitrary unsupported model prose. F owns full trace and G real-host acceptance.

## Validation Boundary

The repository tests use fakes, the official manifest, local Git fixtures, and an opt-in genuine LocalDB pre-007 fixture for minimal source materialization, durable worker recovery, publication, bounded write planning, progressive readiness, dynamic priority, configuration discovery, pre-migration repair, fail-safe errors, Error Dumps, fallback, activation, filtering, diagnostics, authorization, campaign selection, cancellation, and routing. They do not prove remote GitHub acquisition, external SQL Server authentication, backup/recovery, sustained process restart across a production deployment, or cross-client MCP interoperability. Run the [Managed/MCP-Only Acceptance Test](MCP_ONLY_ACCEPTANCE_TEST.md) before treating a deployment as ready.

## Troubleshooting

- `RULE_SCHEMA_MISSING`: preview and approve EC-owned initialization.
- `MIGRATION_REQUIRED`: use readiness and `ec_get_setup_plan`, obtain informed approval, then run supported bootstrap; operation tools remain callable but do not query missing tables. Partial unknown schemas require administrator review.
- `RULE_SOURCE_NOT_CONFIGURED`: select the packaged default or another compatible source you choose.
- `NO_PUBLISHED_RULE_RELEASE`: approve initial publication.
- `NO_ACTIVE_RULE_RELEASE`: follow the configured activation policy.
- `RULE_SOURCE_UNAVAILABLE`: repair source access; an existing active release remains usable in `DEGRADED` mode.
- `RULE_SOURCE_INCOMPATIBLE`: the selected immutable source lacks or violates the supported Managed manifest/compiler contract. Repeating the same immutable source cannot repair it; select a compatible released source or explicitly opt into the configured Prerelease source for authorized testing.
- `RULE_SOURCE_ACQUISITION_FAILED`: inspect network, Git executable, cache permissions, and the correlated authorized diagnostic.
- `RULE_SOURCE_ACQUISITION_TIMEOUT`: the complete acquisition exceeded its overall bound; inspect correlated command observations before changing policy.
- `RULE_SOURCE_PROCESS_TIMEOUT`: one Git subprocess exceeded its individual bound; inspect its safe command category and stage.
- `RULE_SOURCE_REF_NOT_FOUND`: verify that the requested ref exists in the selected source.
- `RULE_SOURCE_READ_FAILED`: verify the manifest and every declared source path at the resolved commit.
- `RULE_COMPILATION_FAILED` or `RULE_VALIDATION_FAILED`: inspect the immutable source revision and correlated diagnostic; blind retry is not expected to repair invalid content.
- `RULE_STORE_STAGE_FAILED`, `RULE_PUBLICATION_FAILED`, or `RULE_ACTIVATION_FAILED`: inspect SQL availability and the correlated diagnostic; retry resumes durable publication state rather than duplicating it.
- `RULE_CLOSURE_PENDING` or `RULE_CONTEXT_PENDING`: query operation/context status and allow priority preparation to complete; do not improvise the missing rule.
- `MANAGED_OPERATION_INTERRUPTED`: the service detected lost execution ownership after restart and will reclaim idempotent work or leave a retryable failure.
- `WORKER_INDEPENDENT_LAUNCH_BLOCKED`: automatic transport-independent launch was refused; double-click the prepared one-click launcher to resume the same operation. This is user intervention, not administrator intervention.
- `MANAGED_WORKER_NOT_FOUND` or `MANAGED_WORKER_FALLBACK_PREPARATION_FAILED`: repair the packaged reference-service installation or its local launcher directory without deleting the durable operation.
- `RULE_PUBLICATION_CANCELLED`: retry safely; the service reuses any candidate stage that committed before cancellation.
- `CAMPAIGN_NOT_FOUND`: list campaigns or use the authorized creation path.
- generic host error with hidden stderr: call `ec_get_error_code` and `ec_get_error_dump`, use the returned correlation ID, inspect the SQL diagnostic record or automatic protected physical fallback, and optionally enable verbose errors in a trusted development environment.

The [portable error and diagnostic hierarchy](../../../../docs/support/ERRORS_AND_PORTABLE_DIAGNOSTICS.md) separates static error lookup, bounded Error Dumps, normal diagnostics, optional Support Bundles, and explicitly authorized external submission.
