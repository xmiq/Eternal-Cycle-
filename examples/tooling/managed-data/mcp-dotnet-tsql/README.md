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
4. Accept the official Eternal Cycle Git source when offered, or ask the administrator to select another compatible source.
5. After the service reports `READY`, continue character and campaign bootstrap normally.

To resume, say: **“Continue my Eternal Cycle game.”** One suitable campaign is selected automatically; several are presented by meaningful name. Internal Campaign IDs remain backstage.

### Administrator path

1. Install .NET 10 and provide an existing SQL Server database plus a least-privilege connection with permission to create or upgrade the configured Eternal Cycle schemas.
2. Set `EternalCycle:Persistence:ConnectionString` in protected host configuration.
3. Enable the separate setup surface with `EternalCycle:Administration:Enabled=true` and set a private approval phrase.
4. Add the built executable as an stdio MCP server in the compatible client, then call readiness or ask the AI to start a game.
5. Preview setup, obtain explicit user approval, and run the permission-gated initialization. The service applies only packaged migrations `001` through `005` to configured EC-owned scopes.
6. Select the official source from packaged [`DISTRIBUTION.json`](../../../../DISTRIBUTION.json) metadata or provide a compatible custom Git source. The selection persists in `ec_domain`.
7. Approve initial publication. The service acquires/caches the source, resolves an immutable commit, compiles, validates, publishes, activates according to policy, and verifies readiness.
8. Disable administrative setup after provisioning when ongoing administration is handled elsewhere.

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
- startup, periodic, manual, and offline update policies;
- sanitized capabilities and diagnostic reports;
- structured first-run readiness across transport, persistence, campaign schema, Rule Domain, source, publication, activation, and campaign state;
- permission-gated EC-owned migrations, persisted source selection, managed Git acquisition/cache, initial publication, and campaign discovery/creation;
- bounded rule-publication batches, stage-aware structured failures, and idempotent publication resume;
- SQL-first Managed-operation diagnostics with a protected local physical fallback;
- optional sanitized general file logging for hosts that hide stderr.

The reference does not implement PostgreSQL, MySQL, document, graph, key/value, or indexed-file adapters. Those are contract-compatible extension families, not claimed implementations.

## Gameplay MCP Tools

| Tool | Purpose |
| --- | --- |
| `ec_service_capabilities` | Identify this reference implementation and its semantic capability set. |
| `ec_persistence_status` | Return service-backed campaign status. |
| `ec_read_records` | Read exact canonical owner-domain and stable-record addresses. |
| `ec_commit_changes` | Stage, validate, activate, read back, and receipt one complete transaction. |
| `ec_retry_persistence` | Resume a stable transaction without replaying gameplay. |
| `ec_get_rule_context` | Retrieve an already-published bounded Rule Packet. |
| `ec_get_diagnostics` | Return sanitized implementation, rules, source, namespace, and update provenance. |
| `ec_get_readiness` | Return structured setup and gameplay readiness without mutation. |
| `ec_get_setup_plan` | Preview packaged EC-owned migrations. |
| `ec_list_campaigns` | List meaningful campaign names with stable internal IDs. |
| `ec_resolve_resume_campaign` | Select the sole campaign or return meaningful choices. |

Administrative tools are separately configuration-gated and require an exact explicit approval phrase:

| Tool | Purpose |
| --- | --- |
| `ec_initialize_service` | Apply and validate only packaged EC-owned migrations. |
| `ec_configure_rule_source` | Persist an explicit Stable or Prerelease official channel, or a compatible custom Git source/ref. |
| `ec_publish_initial_rules` | Run explicit initial acquire/compile/validate/publish/activate workflow. |
| `ec_create_campaign` | Create one stable campaign identity in the trusted default namespace. |

No tool accepts arbitrary SQL, a caller-supplied schema, a connection string, or a credential. Source publication and schema mutation remain administrative responsibilities even when exposed through the explicitly gated setup surface.

## Build and Test

```powershell
dotnet build examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/EternalCycle.Persistence.Mcp.csproj
dotnet test examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/EternalCycle.Persistence.Mcp.Tests.csproj
```

## T-SQL Namespaces

First-run bootstrap applies and validates these assets after approval. Advanced/manual recovery may apply:

1. [`001_initial.sql`](src/EternalCycle.Persistence.Mcp/Schema/001_initial.sql) for the legacy-compatible `ec` campaign schema, or render [`001_initial.template.sql`](src/EternalCycle.Persistence.Mcp/Schema/001_initial.template.sql) for a trusted World Data Namespace mapping;
2. [`002_rule_domain.sql`](src/EternalCycle.Persistence.Mcp/Schema/002_rule_domain.sql) for the reference `ec_domain` Domain Namespace, or render its template under an approved physical mapping policy.
3. [`003_campaign_directory.sql`](src/EternalCycle.Persistence.Mcp/Schema/003_campaign_directory.sql) to add player-meaningful campaign metadata to a prior default deployment.
4. [`004_rule_source_configuration.sql`](src/EternalCycle.Persistence.Mcp/Schema/004_rule_source_configuration.sql) to persist the selected Rule Source without storing source credentials.
5. [`005_managed_operation_diagnostics.sql`](src/EternalCycle.Persistence.Mcp/Schema/005_managed_operation_diagnostics.sql) to preserve redacted operation evidence with correlation and publication-stage metadata.
6. [`006_rule_source_compatibility.sql`](src/EternalCycle.Persistence.Mcp/Schema/006_rule_source_compatibility.sql) to add source-channel, discovery-ref, manifest-contract, and safe causal-diagnostic provenance.

New deployments should prefer the `ec_` discoverability convention, such as `ec_mainworld` or `ec_fantasyworld`. Existing `ec` deployments remain supported. Physical schema names do not become Campaign IDs, World IDs, RuleSet IDs, or Logical Data Namespace IDs.

`ec_domain` stores only shared service data and Derived rule publications: RuleSets, Rule Releases, compiled chunks, selectors, dependencies, active-release pointers, provenance, update checks, and redacted Managed-operation diagnostics. World-specific Campaign Canon remains in the selected world schema.

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
EternalCycle:Administration:ApprovalPhrase = <private explicit confirmation phrase>
EternalCycle:Administration:ManagedRuleCacheDirectory = <optional managed cache>
EternalCycle:Administration:SanitizedLogFile = <optional log file>
```

Managed diagnostic configuration includes:

```text
EternalCycle:Diagnostics:VerboseErrors = false
EternalCycle:Diagnostics:FallbackLogFile = <optional protected path>
EternalCycle:Diagnostics:PersistenceTimeout = 00:00:10
```

`VerboseErrors` defaults to `false`. It adds useful redacted exception detail to authorized administrative failures but never exposes credentials, changes transaction behavior, or controls whether diagnostics are preserved. If SQL diagnostic insertion fails, the default fallback is `%LOCALAPPDATA%\EternalCycle\logs\managed-diagnostics.jsonl` on Windows or the platform-equivalent local application-data directory. The fallback retains the operation correlation ID. Failure of both sinks never masks the publication failure.

The packaged `distribution-metadata.json` identifies the official repository, historical product release tag, compatible Stable and Prerelease Rule Source refs, and source manifest. Product version and Rule Source channel are independent: `VERSION` remains `1.0.0`, and the immutable `v1.0.0` tag remains historical provenance even though it predates the Managed manifest contract. Stable is the default and never falls forward to unreleased content. If no compatible Stable source is published, setup returns `RULE_SOURCE_INCOMPATIBLE`; an administrator must explicitly select Prerelease for current development testing.

The reference project resolves distribution metadata from the repository root when built in a full checkout and from its shipped project-local `DISTRIBUTION.json` when extracted as a standalone tool; it does not depend on the caller's working directory or a machine-specific path. A persisted source selection takes precedence for managed acquisition; `RepositoryRoot` remains an advanced/offline override. The provider resolves the discovery ref once, records the exact commit SHA as immutable publication provenance, and reads the manifest and declared documents at that commit. Moving the discovery ref later does not rewrite an existing Rule Release.

The manifest declares its format, compiler contract, RuleSet, repository version, and required sources. Missing or unsupported manifest contracts fail as non-retryable `RULE_SOURCE_INCOMPATIBLE`, rather than as transient network failures. Remote clone and fetch use the longer `AcquisitionTimeout`; local `rev-parse`, `show`, and object inspection use `ProcessTimeout`. Cancellation attempts to terminate the process tree and bounds cleanup and redirected-reader waits by `TerminationGracePeriod`. Valid no-checkout caches and locally available immutable revisions are reused when update policy permits, while incomplete clone directories are discarded before retry. A failed source check or candidate preserves the active validated Rule Release. `Disabled` means an explicit administrator chose offline update behavior; it does not prevent an approved one-time initial publication.

General sanitized host logging is disabled unless `SanitizedLogFile` is set. It records timestamp, level, category, event, message, and exception type, but omits exception text. This is separate from the Managed-operation diagnostic fallback, which is available when SQL diagnostic persistence fails. Both paths exclude credentials, connection strings, Campaign Canon, and GM Secrets.

## Campaign Transactions

Candidate campaign records remain inactive until the complete Affected Set validates and atomically advances the campaign's active version. A validated Persistence Receipt is the Managed completion evidence. Retry resumes the same transaction and never reapplies gameplay effects.

## Security

- MCP callers never submit SQL, schemas, namespace mappings, source paths, or credentials.
- Campaign ID isolation is enforced independently of namespace routing.
- Configurable identifiers are strictly validated and safely quoted.
- Gameplay tools have no source-publication or database-administration authority.
- Setup operations require both service-side enablement and an exact approval phrase; they can execute only packaged migrations and bounded domain operations.
- Diagnostic output excludes connection strings, credentials, locators, Campaign Canon, GM Secrets, and conversations.

## Validation Boundary

The repository tests use fakes, the official manifest, and local Git fixtures for no-checkout acquisition, publication, bounded write planning, fallback, activation, filtering, diagnostics, readiness, approval, campaign selection, cancellation, and routing. They do not prove live SQL Server migration/transaction/concurrency, remote GitHub acquisition, authentication, backup/recovery, long-running scheduling, or cross-client MCP interoperability. Run the [Managed/MCP-Only Acceptance Test](MCP_ONLY_ACCEPTANCE_TEST.md) before treating a deployment as ready.

## Troubleshooting

- `RULE_SCHEMA_MISSING`: preview and approve EC-owned initialization.
- `MIGRATION_REQUIRED`: run supported bootstrap; partial unknown schemas require administrator review.
- `RULE_SOURCE_NOT_CONFIGURED`: select the official source or a compatible override.
- `NO_PUBLISHED_RULE_RELEASE`: approve initial publication.
- `NO_ACTIVE_RULE_RELEASE`: follow the configured activation policy.
- `RULE_SOURCE_UNAVAILABLE`: repair source access; an existing active release remains usable in `DEGRADED` mode.
- `RULE_SOURCE_INCOMPATIBLE`: the selected immutable source lacks or violates the supported Managed manifest/compiler contract. Repeating the same immutable source cannot repair it; select a compatible released source or explicitly opt into the configured Prerelease source for authorized testing.
- `RULE_SOURCE_ACQUISITION_FAILED`: inspect network, Git executable, cache permissions, and the correlated authorized diagnostic.
- `RULE_SOURCE_OPERATION_TIMEOUT`: distinguish remote acquisition from local Git work using the reported stage and latest relevant causal diagnostic; tune the corresponding bounded timeout only after checking network or process health.
- `RULE_SOURCE_REF_RESOLUTION_FAILED`: verify the configured ref exists in the selected source.
- `RULE_SOURCE_READ_FAILED`: verify the manifest and every declared source path at the resolved commit.
- `RULE_COMPILATION_FAILED` or `RULE_VALIDATION_FAILED`: inspect the immutable source revision and correlated diagnostic; blind retry is not expected to repair invalid content.
- `RULE_STORE_STAGE_FAILED`, `RULE_PUBLICATION_FAILED`, or `RULE_ACTIVATION_FAILED`: inspect SQL availability and the correlated diagnostic; retry resumes durable publication state rather than duplicating it.
- `RULE_PUBLICATION_CANCELLED`: retry safely; the service reuses any candidate stage that committed before cancellation.
- `CAMPAIGN_NOT_FOUND`: list campaigns or use the authorized creation path.
- generic host error with hidden stderr: use the returned correlation ID, inspect the SQL diagnostic record or protected physical fallback, and optionally enable verbose errors in a trusted development environment.
