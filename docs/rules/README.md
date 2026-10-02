# Rule Compilation and Retrieval

This family defines how authorized tooling compiles and delivers small, provenance-bearing rule contexts from Eternal Cycle Repository Canon. In Managed mode the service owns compilation and the runtime retrieves published Rule Packets. The repository documents remain authoritative; a compiled index is a derived navigation artifact, never a replacement rules database.

## Document Control

- **Owner:** rule-source classification, compiled-index semantics, retrieval applicability, provenance, and normal context budget
- **Dependencies:** Repository Canon, Campaign Configuration, World/Ruleset identity, [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md), and [Context Assembly](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- **Extensions:** future world/ruleset packages and optional modules may register sources through the same metadata contract
- **Consumers:** human and AI GMs, runtime profiles, context assemblers, Managed services, and optional reference tooling
- **Repository boundary:** no Campaign Canon, save locator, credential, private deployment configuration, or populated compiled packet belongs here

## Reading Order

1. [Runtime Rule Kernel](RUNTIME_RULE_KERNEL.md) - the compact invariant set included in every normal compiled rule context.
2. [GM Runtime Procedure](GM_RUNTIME_PROCEDURE.md) - the mandatory read, resolve, persist, validate, narrate, and yield lifecycle for gameplay resolution.
3. [`GM_HOST_BOOTSTRAP.txt`](GM_HOST_BOOTSTRAP.txt) - the compact, versioned execution contract installed at the highest instruction level a host supports.
4. [Rule Compilation and Context-Efficient Retrieval](RULE_COMPILATION_AND_RETRIEVAL.md) - source authority, metadata, indexing, world isolation, selection, token budget, and failure rules.
5. [Compiled Rules Artifact](COMPILED_RULES_ARTIFACT.md) - provider-neutral format, stable snippet identity, provenance, applicability, dependencies, retrieval-metadata carrier, determinism, compatibility, and integrity.
6. [Controlled Retrieval Vocabulary](CONTROLLED_RETRIEVAL_VOCABULARY.md) - accepted reviewed input, exact enrichment/origins, automatic artifact/CLI integration, observational quality audit, curated topology, and integrated FR-024 acceptance.
7. [Compiled Rules Acquisition and Import](COMPILED_RULES_ACQUISITION_AND_IMPORT.md) - completed FR-025 bounded provider-neutral bytes/evidence, explicit validation/trust and lossless atomic/idempotent import; runtime retrieval remains separately governed.
8. [Managed Rule Publication](MANAGED_RULE_PUBLICATION.md) - Rule Source Providers, versioned candidates, validation, publication, activation, updates, offline fallback, and bounded runtime packets.

The adjacent [`rule-source-manifest.json`](rule-source-manifest.json) is a reference compiler manifest, not an authority source. It identifies its manifest format, compiler contract, RuleSet, repository version, canonical Markdown inputs, and applicability metadata for the included service implementation. The portable [`compiled-rules-artifact-v1.schema.json`](contracts/compiled-rules-artifact-v1.schema.json) validates format-1 structure; semantic conformance is demonstrated by the small [artifact fixtures](../../examples/conformance/compiled-rules-artifact/v1/) and reference validator. [Managed Rule Publication](MANAGED_RULE_PUBLICATION.md#release-channels-and-compatibility) defines Stable and Prerelease channel selection, immutable Git provenance, and compatibility failure behavior.

## Authority Boundary

Repository Markdown owns the rules. A compiler may split, hash, tag, rank, and retrieve those rules, but it may not silently rewrite them. Campaign facts are loaded separately from canonical persistence under the same Campaign ID. SQL, a vector index, or any other search backend may carry a derived index only; it does not outrank its repository sources.

## Related Documents

- [Canonical Rules Map](../README.md)
- [AI Operations Index](../ai/README.md)
- [Portable Persistence Architecture](../persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- [Managed Data Service](../persistence/MANAGED_DATA_SERVICE.md)
- [Standalone Rules Compiler](../../examples/tooling/rules-compiler-dotnet/README.md)
- [Reference Managed Tooling](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md)
