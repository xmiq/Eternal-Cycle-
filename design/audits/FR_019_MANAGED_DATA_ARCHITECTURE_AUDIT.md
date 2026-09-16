# FR-019 Managed Data Architecture and Rule Publication Audit

## Scope

This audit records the owner-authorized FR-017/FR-018 consolidation into vendor-neutral Direct and Managed persistence, storage-neutral namespaces, service-owned rule publication, optional reference tooling, and release-neutral support. It changes post-v1 architecture only; the historical `v1.0.0` release tag remains untouched.

## Repository State Before Work

The task began on `main` at `2f739d9` with `HEAD`, `origin/main`, and the working tree aligned. There were no modified, deleted, staged, or untracked files. No pre-existing work from another AI/runtime required classification.

At continuation after an interrupted run, the working tree contained only work produced for this objective:

- **retain:** new Direct/Managed, Logical Data Namespace, Managed rule-publication, support, and reference-tooling documents;
- **retain but modify:** the relocated .NET/MCP/T-SQL service, publication code, tests, templates, bootstrap text, and rule-retrieval text required additional dependency, freshness, compatibility, navigation, and governance work;
- **unrelated but valid:** none;
- **redundant:** no duplicate canonical documentation tree was retained;
- **incorrect:** remaining live uses of MCP as a universal top-level mode and Managed-as-SQL-Server wording were corrected rather than discarded;
- **unsafe:** none of the interrupted work contained campaign data, credentials, private locators, or destructive migrations.

No useful existing implementation was deleted. Tracked reference-service files were relocated from `services/eternal-cycle-mcp/` to `examples/tooling/managed-data/mcp-dotnet-tsql/` and evolved in place.

## Architectural Result

### DIRECT

The runtime directly operates one Database Format Adapter and the Storage Adapter Chain required by canonical authority. Existing SQLite, DuckDB, local-storage, and Google Drive contracts remain valid. Release 1 Direct campaigns do not migrate automatically.

### MANAGED

A Managed Data Service becomes canonical persistence authority. Its backend transaction, durability, backup, replication, recovery, and operational validation replace client Direct save machinery. A Managed client does not create a competing canonical local or cloud save.

Campaign initialization follows **Enumerate -> Select -> Persist -> Reuse**. Resume loads the persisted strategy before considering current environmental preference. Any change is an explicit validated migration.

### Managed Data Service Contract

The universal contract defines service/status, campaign, turn-persistence, rules, and administrative capability families; authority, completion evidence, failure, security, and diagnostics; and storage/interface-neutral behavior. Relational, document, key/value, graph, indexed-file, and other faithful structured implementations are permitted but not supplied by this revision.

### Managed Storage Adapter

A Managed Storage Adapter is an internal service component that maps the universal Managed contract to one structured backend. It is not a client-side Direct Adapter, is not visible as campaign authority, and cannot adjudicate gameplay. The supplied reference implements this boundary with Microsoft SQL Server/T-SQL; PostgreSQL, document, key/value, graph, and indexed-file variants remain specified extension families rather than implemented adapters.

### MCP Reference Interface

MCP is one optional interface realization. It exposes bounded semantic reads, commits, retry, rule context, capability, and diagnostic operations. It exposes no raw datastore surface. Legacy `MCP` mode configuration migrates to `MANAGED` strategy with `MCP` interface without changing campaign facts.

### Logical Data Namespace

Logical Data Namespace is the stable universal identity and isolation boundary. Native stores map it to an appropriate schema, database, collection, partition, graph, directory, or equivalent. Campaign ID, World/Ruleset, Logical Data Namespace, physical schema, and physical database remain distinct.

### T-SQL Mapping

The reference recommends `ec_` physical schemas. `ec_domain` contains shared service metadata and Derived Rule Releases. Configured `ec_<world>` schemas contain compatible campaign Canon with independent Campaign ID isolation. Existing `ec` deployments remain valid. Trusted mappings, strict identifier validation, safe quoting, and scoped migration are preserved.

### Managed Rule Publication

Canonical Markdown remains Rule Canon. The Managed service, not the AI GM, owns source acquisition, stable identities, parsing, explicit dependency generation, applicability indexes, source hashes, validation, versioned publication, activation, update checks, and bounded Rule Packet assembly. Git sources record an immutable commit SHA. Candidate or source failure preserves the last valid active release where safe.

Runtime Rule Packets remain world-isolated, dependency-complete, provenance-bearing, and bounded by the approximate 8,000-token reference target. Campaign Canon is read separately under FR-011.

### Diagnostics and Support

The reference service exposes implementation, interface, strategy, namespace, World/Ruleset, Rule Release, source identity, and update state without credentials, connection strings, private locators, Campaign Canon, GM Secrets, or conversations. Support guidance classifies campaign outcomes, rules gaps, engine defects, reference-tool defects, third-party defects, and requests. Reports are prepared but never submitted silently, and fork provenance does not imply official support.

## Reference Implementation

### Implemented

- .NET hosted service and MCP semantic gameplay interface;
- Microsoft SQL Server/T-SQL campaign and domain storage;
- trusted Campaign -> World -> Logical Data Namespace -> schema routing;
- campaign isolation, optimistic concurrency, idempotent commit/retry, validation, activation, read-back, and Persistence Receipts;
- Git source provider using immutable commit SHA;
- compiled candidate validation, versioned release publication, atomic activation, explicit dependencies, selector indexes, active-release fallback, and durable update-check state;
- already-published Rule Packet retrieval with campaign compatibility, world/module/mode/operation/topic filtering, dependencies, provenance, and 8K limit;
- startup, periodic, manual, and disabled/offline update policies;
- sanitized capability and diagnostic MCP tools.

### Specified Extension Points Only

- non-MCP Managed interfaces;
- PostgreSQL, MySQL/MariaDB, SQLite, DuckDB, document, key/value, graph, and indexed-file Managed adapters;
- GitLab, archive, enterprise-repository, and other Rule Source Providers;
- production administration UI, authentication/authorization provider, hosted backup operator, and monitoring stack.

No unsupplied adapter is claimed as implemented.

## Validation

- The reference .NET project builds with zero warnings and zero errors; its test project passes 36 of 36 tests.
- Managed publication tests cover candidate lifecycle, active-release preservation, automatic/manual activation, source outage and offline fallback, unchanged and changed sources, published-store runtime retrieval, explicit dependencies, major-version compatibility, immutable local-Git commit provenance, and sanitized diagnostics.
- The FR-017 harness passes 29 assertions and the FR-018 harness passes 19 assertions, preserving historical Direct, adapter, receipt, namespace-routing, schema-security, world-isolation, and 8K behaviors under corrected paths.
- The FR-019 harness passes all 48 requested requirement groups across strategy selection, Managed authority, namespaces, T-SQL routing, rule publication, source providers, 8K retrieval, support, diagnostics, and backward compatibility.
- Repository validation passes 7,011 relative links, 160 anchors, 183 canonical documents, 15 family indexes, 43 templates, 1,232 canonical terms, 191 roadmap tasks, 19 Future Revisions, campaign-data boundaries, and focused harnesses.

The final commit report records exact command results. These are local unit, fixture, structural, and static validations unless explicitly stated otherwise.

## Remaining Deployment Acceptance

The repository did not have live infrastructure for these tests. Deployment acceptance still requires:

- real Microsoft SQL Server transaction, constraint, isolation, atomic activation, and concurrent-client behavior;
- real schema and Domain Namespace migration from existing `ec` deployments;
- service restart during staged, validated, published, and active states;
- production backup, restore, disaster recovery, and retention;
- authenticated Git/GitHub fetch, outage, force-move, and long-running update scheduling;
- production identity, authorization, secret management, and administrative separation;
- multiple real MCP clients and host-specific interoperability;
- long-duration load, performance, 8K tokenizer calibration, and observability.

Unit tests and local Git fixtures do not prove those deployed behaviors.

## Outcome

FR-019 establishes the smallest coherent correction: Eternal Cycle defines behavior and contracts without vendor lock-in, while retaining one useful optional .NET/MCP/T-SQL implementation. No campaign data was added, no gameplay mechanic changed, and no later Future Revision was selected automatically.
