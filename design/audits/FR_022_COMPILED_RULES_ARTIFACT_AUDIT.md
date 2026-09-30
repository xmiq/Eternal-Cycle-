# FR-022 Compiled Rules Artifact Audit

## Scope

This audit records the owner-authorized Phase 13 implementation of **FR-022 — Portable Compiled Rules Artifact Contract**. Work began from planning commit `d25862b` on `main` with a clean working tree. Eternal Cycle `VERSION` remains `1.0.0`; immutable `v1.0.0` and the moving `v1.1.0-rc` discovery tag were not changed by this objective.

FR-022 defines the contract and conformance boundary only. It does not implement the FR-023 offline compiler, FR-024 vocabulary compiler/audit, FR-025 acquisition/import providers, FR-026 runtime store/ranking, or FR-034 release packaging.

No Campaign Canon, save, current character, private locator, credential, GM Secret, live world state, or populated rule database entered the repository.

## Existing Foundation

FR-018 already established Repository Canon authority, `RuleSourceId`, source path/anchor/hash provenance, applicability selectors, dependencies, a Derived `CompiledRuleIndex`, bounded contexts, and compact Rule Packets. FR-019 established service-owned Managed publication. FR-021 added progressive preparation tiers and kept full provenance inside the service while making model-facing packets compact.

Those structures proved the needed concepts but did not define a provider-neutral portable artifact. The existing in-process index also carries volatile publication-time metadata and is not an interchange contract.

## Normative Contract

The canonical [Compiled Rules Artifact](../../docs/rules/COMPILED_RULES_ARTIFACT.md) defines format 1 with:

- separate artifact-format, compiler-contract, compiler-implementation, Ruleset, repository-release, immutable-source, manifest, and semantic-integrity identities;
- stable snippet identity from Rule Source ID plus optional source anchor;
- normalized executable text and per-snippet SHA-256;
- normalized source records owning path, hash, applicability, and source-level dependencies;
- existing Rule Layer, World Model, module, Campaign Mode, operation, topic, priority, always-include, and preparation-tier selectors;
- an optional retrieval term/weight/relationship carrier without vocabulary generation or ranking;
- strict dependency existence, self-reference, and cycle rules;
- ordinal set ordering, NFC identifier/term normalization, relative-path rules, LF content, invariant numbers, and no volatile identity fields;
- a length-prefixed UTF-8 semantic SHA-256 independent of JSON property ordering and insignificant whitespace;
- explicit compatibility, trust, failure, and downstream ownership boundaries.

The normative contract does not require GitHub, Git, C#, .NET, SQL Server, Windows, MCP, a database schema, an installation path, or any acquisition provider.

## Portable Schema and Fixtures

The [format-1 JSON Schema](../../docs/rules/contracts/compiled-rules-artifact-v1.schema.json) provides Draft 2020-12 structural validation with closed objects, required fields, enums, bounds, hash shapes, and normalized identity/path shapes.

Small provider-neutral fixtures cover:

- [minimal valid artifact](../../examples/conformance/compiled-rules-artifact/v1/valid-minimal.json): one source and one unanchored snippet;
- [representative valid artifact](../../examples/conformance/compiled-rules-artifact/v1/valid-representative.json): two ordered sources, dependency closure, selectors, anchored snippets, terms, and a retrieval relationship.

They contain generic rule text only and are not a second maintained rules corpus.

## Reference Validation

The optional C# [reference contract](../../examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRulesArtifact.cs) parses closed JSON, rejects duplicate properties, validates all semantic relationships, computes content and artifact digests, and returns stable code/path/message errors. It is reference tooling, not the specification.

The focused tests validate:

- both valid fixtures and their semantic digests;
- stable snippet identity;
- duplicate snippet rejection;
- missing dependency rejection;
- unsupported artifact and compiler contracts;
- malformed identity, path, hash, selectors, terms, and relationships;
- content normalization and content-hash mismatch;
- artifact-integrity mismatch;
- unknown and duplicate JSON properties;
- provider-neutral custom immutable-source schemes;
- null required structure without an unhandled exception;
- unsupported programmatic enum values.

No SQL or campaign-schema migration was required. FR-022 adds no imported store and changes no existing Rule Release or campaign data.

## Classification

### Eternal Cycle-Wide

- authority chain from Repository Canon through artifact, runtime representation, and Rule Packet;
- provider-neutral semantic format;
- stable identity and portable provenance;
- applicability, dependency, retrieval-carrier, determinism, integrity, compatibility, and trust rules.

### Managed Service

No new Managed-only runtime behavior is required by FR-022. A Managed importer must eventually honor the same validation and authority contract, but that belongs to FR-025 and FR-026.

### Reference Implementation

- C# data types and validator;
- `System.Text.Json` parsing;
- xUnit conformance tests and test-output fixture copying;
- PowerShell structural regression harness.

## Downstream Constraints

- **FR-023:** must emit format-1 identity, ordering, normalization, and digest deterministically without campaign or provider dependency.
- **FR-024:** must populate controlled kinds/terms/weights/relationships without changing snippet identity or rule meaning.
- **FR-025:** must validate identical bytes identically across local, GitHub, mirror, private, or custom providers and keep acquisition metadata outside artifact authority.
- **FR-026:** must import stable IDs and dependencies, retain provenance, explain ranking, and keep Rule Packets compact.
- **FR-034:** must package and publish artifacts reproducibly without treating archive/provider metadata as artifact semantics.

These constraints refine existing tasks; they do not create a new Future Revision.

## Validation

- Focused Compiled Rules artifact tests: pass, 16 of 16.
- Format-1 schema and fixture JSON parsing: pass through the FR-022 harness.
- FR-022 structural harness: pass, 32 assertions.
- Reference Release build and complete tests: pass, 179 of 179 with zero warnings or errors.
- Full repository validation: pass across 291 Markdown files, 7,239 relative links, 195 anchors, 189 indexed canonical documents, 43 templates, 1,272 terminology checks, 208 roadmap tasks, and 36 Future Revision entries.
- Existing FR-011, FR-017, FR-018, FR-019, FR-020, FR-021, control-plane, and release-neutral harnesses: pass alongside FR-022.
- Blocking unresolved questions, orphaned Markdown documents, and forbidden campaign-data directories: zero.
- Whitespace and complete-diff review: pass.

## Remaining Boundaries

FR-022 does not prove that a future compiler is reproducible, that an external provider delivers correct bytes, that an importer maps them correctly, that ranked retrieval is relevant, or that a release archive is deterministic. Those are the explicit acceptance responsibilities of FR-023 through FR-026 and FR-034.

No unresolved architectural question blocks FR-022.

## Result

FR-022 is complete. Phase 13 remains active, Future Revisions remains `[∞]`, and no next objective is selected automatically. FR-023 and FR-025 are now dependency-ready; independently planned FR-030 remains dependency-ready.
