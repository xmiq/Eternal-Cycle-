# Rule Compilation and Retrieval

This family defines how a runtime compiles and retrieves small, provenance-bearing rule contexts from Eternal Cycle Repository Canon. The repository documents remain authoritative; a compiled index is a derived navigation artifact, never a replacement rules database.

## Document Control

- **Owner:** rule-source classification, compiled-index semantics, retrieval applicability, provenance, and normal context budget
- **Dependencies:** Repository Canon, Campaign Configuration, World/Ruleset identity, [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md), and [Context Assembly](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- **Extensions:** future world/ruleset packages and optional modules may register sources through the same metadata contract
- **Consumers:** human and AI GMs, runtime profiles, context assemblers, and the Eternal Cycle MCP reference service
- **Repository boundary:** no Campaign Canon, save locator, credential, private deployment configuration, or populated compiled packet belongs here

## Reading Order

1. [Runtime Rule Kernel](RUNTIME_RULE_KERNEL.md) - the compact invariant set included in every normal compiled rule context.
2. [Rule Compilation and Context-Efficient Retrieval](RULE_COMPILATION_AND_RETRIEVAL.md) - source authority, metadata, indexing, world isolation, selection, token budget, and failure rules.

The adjacent [`rule-source-manifest.json`](rule-source-manifest.json) is a reference compiler manifest, not an authority source. It identifies canonical Markdown inputs and applicability metadata for the included service implementation.

## Authority Boundary

Repository Markdown owns the rules. A compiler may split, hash, tag, rank, and retrieve those rules, but it may not silently rewrite them. Campaign facts are loaded separately from canonical persistence under the same Campaign ID. SQL, a vector index, or any other search backend may carry a derived index only; it does not outrank its repository sources.

## Related Documents

- [Canonical Rules Map](../README.md)
- [AI Operations Index](../ai/README.md)
- [Portable Persistence Architecture](../persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md)
- [Reference MCP Service](../../services/eternal-cycle-mcp/README.md)
