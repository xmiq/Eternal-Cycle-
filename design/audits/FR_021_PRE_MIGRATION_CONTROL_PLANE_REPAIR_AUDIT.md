# FR-021 Pre-Migration Control-Plane Repair Audit

## Scope

This audit records owner-authorized corrective maintenance to completed FR-021. It does not create FR-022, change `VERSION`, move a tag, publish a release, or select another Phase 13 objective.

## Interrupted-State Recovery

The resumed tree already contained the durable publication worker, migration 007, per-source preparation state, progressive readiness, context-priority behavior, service diagnostics, natural approval, and provenance work from FR-021. The interrupted corrective pass had additionally implemented most control-plane source changes and tests but had not completed documentation, governance, structural validation, packaging verification, or the final audit.

Existing correct work was preserved. Partially integrated behavior was repaired in place rather than restarted.

## Observed Failure Class

A database legitimately initialized through migrations 001-006 required migration 007. Several recovery paths could nevertheless depend on post-007 operation or preparation tables. An external MCP client could therefore receive a generic invocation failure while trying to discover readiness, diagnostics, or operation status needed to authorize the migration.

The defect was architectural rather than the absence of a migration:

- configuration was not available as an explicit database-independent contract;
- readiness and diagnostics were insufficiently guarded by schema capability;
- operation tools could assume their durable store already existed;
- final tool error handling could lose semantic recovery information;
- fallback diagnostics were not established early enough as a zero-configuration safety net;
- migration 007's template used placeholder names unsupported by the repository renderer.

## Repair

### Base control plane

Before migration 007, the reference service now supports:

- `ec_get_configuration_requirements` without database access;
- structured readiness and setup planning;
- pre-migration diagnostics that avoid post-007 tables;
- operation status and listing responses that return `MIGRATION_REQUIRED` rather than querying an absent store;
- static error-code listing and lookup;
- bounded partial Error Dumps;
- natural informed approval followed by packaged migration and validation.

### Semantic failure boundary

All MCP tool calls pass through a final exception boundary. Known `ManagedServiceException` failures retain their code and recovery semantics. Unexpected failures become `INTERNAL_ERROR`. If response construction itself fails, a static emergency string remains available. Verbose mode adds only redacted bounded detail.

### Diagnostics

SQL remains the preferred Managed-operation diagnostic owner when its schema is available. The service establishes an automatic protected local JSONL fallback before database-dependent services start. A configured override and explicit disable control remain available; ordinary users need neither. Optional general host logging stays separate.

### Error Dump and support hierarchy

`eternal-cycle-error-dump/v1` is a bounded structured-and-text snapshot. It tolerates missing SQL and operation evidence, returns warnings for partial data, redacts sensitive content, and performs no network work. It does not depend on the optional future Support Bundle concept. External submission remains a distinct explicit-consent action.

### Migration and operation integrity

Migration 007 remains additive. Its template now uses renderer-supported `{{schema}}` and `{{schema_name}}` tokens. The operation enqueue transaction ensures its referenced RuleSet identity exists before inserting the foreign-key-bound operation, fixing a first-publication failure found by the genuine SQL fixture. Existing campaign rows survive upgrade.

## Licensing and Distribution Provenance

The repository now carries Apache License 2.0 and a `NOTICE` identifying xmiq as the original project author. Official distribution metadata declares license, copyright, distribution identity, and support-destination type. Reference-service build and publish output include `LICENSE`, `NOTICE`, and `distribution-metadata.json`.

Derivative distributions remain permitted under the license but must identify their own provenance and may not imply original-author endorsement, warranty, or support. NuGet and other third-party dependencies retain their own licenses; the project license does not relicense them.

## Validation Architecture

Coverage includes:

- configuration requirements and secret non-disclosure;
- static registry completeness and unknown-code behavior;
- verbose and non-verbose semantic errors;
- outer-boundary and emergency fallback behavior;
- automatic fallback logging;
- Error Dump bounds, partial evidence, sanitization, text output, no-network behavior, provenance, and Support Bundle separation;
- genuine migrations-001-through-006 LocalDB upgrade through migration 007;
- pre-007 readiness, setup plan, diagnostics, Error Dump, and operation recovery;
- no-mutation denial before approval;
- campaign preservation after migration;
- source configuration and durable operation creation after migration;
- license/notice/distribution consistency and package-copy declarations.

## Executed Validation

- .NET Release build: passed with zero warnings and zero errors.
- reference-service tests: 109 passed, 0 failed, 0 skipped.
- opt-in genuine LocalDB pre-007 integration: 1 passed after granting the test process access to create a temporary LocalDB instance; the initial restricted sandbox attempt could not start LocalDB and was not treated as product evidence.
- FR-021 durable Managed-operation harness: 47 assertions passed.
- FR-021 control-plane repair harness: 35 assertions passed.
- repository validation: passed across 284 Markdown files, 7,127 relative links, 163 anchors, 187 indexed canonical documents, 43 templates, 1,258 terminology checks, 193 roadmap tasks, and 21 Future Revision entries.
- full-checkout Release publish: passed; output contained loadable `distribution-metadata.json`, `LICENSE`, and `NOTICE`.
- simulated standalone-project build: passed with zero warnings and zero errors; output contained the same three distribution files.
- whitespace and patch check: passed; Git reported only expected line-ending conversion notices.

## Third-Party Material Review

The tracked-source extension and directory scan found no vendored binaries, images, fonts, archives, or third-party source directory. The reference projects declare NuGet package dependencies rather than vendoring their code. Those dependencies retain their own package licenses and are not relicensed by Eternal Cycle's Apache-2.0 license. No contradictory repository license declaration was found. This is a reasonable repository-level review, not a legal opinion or a provenance claim about packages fetched outside the repository.

## Real-Infrastructure Boundary

The LocalDB fixture proves real T-SQL execution for the pre-007 upgrade path on the test host. It does not prove:

- LM Studio or Unsloth MCP interoperability;
- remote GitHub acquisition and authentication;
- production SQL Server permissions, backup, failover, or recovery;
- sustained process restart under a production supervisor;
- external issue submission or Support Bundle interoperability.

Those remain explicit acceptance work. The repository makes no claim that they passed.

## Result

FR-021 remains complete with corrective provenance. Phase 13 remains active, Future Revisions remains `[∞]`, and no later objective is selected automatically.

## Related Documents

- [FR-021 Durable Managed Operations Audit](FR_021_DURABLE_MANAGED_OPERATIONS_AUDIT.md)
- [Managed Data Service](../../docs/persistence/MANAGED_DATA_SERVICE.md)
- [Managed Operations](../../docs/persistence/MANAGED_OPERATIONS.md)
- [Errors and Portable Diagnostics](../../docs/support/ERRORS_AND_PORTABLE_DIAGNOSTICS.md)
- [Reference Managed Service](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md)
- [Managed/MCP-Only Acceptance Test](../../examples/tooling/managed-data/mcp-dotnet-tsql/MCP_ONLY_ACCEPTANCE_TEST.md)
