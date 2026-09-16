# Logical Data Namespace

## Purpose

A **Logical Data Namespace**, or **Data Namespace**, is a stable, discoverable storage boundary for compatible Eternal Cycle information. It separates semantic identity from a datastore's physical layout.

## Document Control

- **Owner:** namespace identity, compatibility, routing, isolation, physical mapping, versioning, and migration
- **Dependencies:** Campaign ID, World/Ruleset/Domain Model, Persistence Model Version, and the selected persistence strategy
- **Consumers:** Direct adapters, Managed services, migration, Context Assembly, administration, and diagnostics
- **Repository boundary:** no populated namespace registry, physical locator, or credential belongs here

## Required Properties

A Data Namespace has:

- a stable logical identifier independent of its physical name;
- a declared data-model and compatibility version;
- discoverability through protected configuration or service metadata;
- campaign and cross-namespace isolation;
- indexed retrieval and stable references;
- safe routing and authorization;
- extension and migration rules;
- provenance linking physical mappings to logical identity.

Several compatible campaigns may share one Data Namespace while remaining isolated by Campaign ID. Different World Models may use different namespaces. Data Namespace, Campaign ID, World/Ruleset identity, physical schema, and database identity are never interchangeable.

## Native Mappings

An implementation uses the nearest faithful native boundary:

| Storage family | Example mapping |
| --- | --- |
| SQL Server or PostgreSQL | schema |
| MySQL or MariaDB | database or approved native namespace |
| Document store | database and collection namespace |
| Key/value store | partition and key prefix/namespace |
| Graph store | graph, database, labels, or equivalent boundary |
| Indexed files | directory plus validated index namespace |

These are examples, not mandatory products or layouts. A non-SQL store need not manufacture tables or schemas.

## Domain and World Namespaces

The **Domain Namespace** contains reusable service-level Derived material, including published compiled rules, release metadata, source provenance, namespace registry, and service model versions. It contains no world-specific Campaign Canon.

A **World Data Namespace** contains canonical campaign records for campaigns compatible with its data model. Reusable world-rule chunks remain in the Domain Namespace with applicability metadata rather than being copied into every world namespace.

## T-SQL Reference Mapping

The reference implementation recommends the discoverable `ec_` prefix:

- `ec_domain` for shared service and published-rule data;
- `ec_mainworld`, `ec_fantasyworld`, `ec_scifiworld`, or another approved `ec_<world>` mapping for compatible campaign data.

Example names do not define World IDs. `ec_domain` is reserved by the reference implementation for shared domain data. Existing post-v1 deployments using `ec` remain compatible as a legacy physical mapping. Administrators may use another validated mapping when hosting or security policy requires it.

All SQL identifiers are selected from trusted configuration, strictly validated, correctly quoted, and kept outside player or AI input.

## Migration

Physical rename, database relocation, or backend conversion preserves the Logical Data Namespace ID. A namespace-model migration targets only the selected namespace and does not scan unrelated worlds. Domain rule-store migrations and World Data Namespace migrations are independently versioned and validated.

## Related Documents

- [Portable Persistence Architecture](PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- [Managed Data Service](MANAGED_DATA_SERVICE.md)
- [Migration and Versioning](MIGRATION_AND_VERSIONING.md)
- [Reference T-SQL Service](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md)
