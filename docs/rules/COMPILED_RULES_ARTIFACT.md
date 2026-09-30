# Compiled Rules Artifact

The Compiled Rules Artifact is the provider-neutral, versioned interchange contract for a deterministic, search-oriented derivative of Eternal Cycle Repository Canon. It carries executable rule snippets, stable identity, source provenance, applicability, dependencies, retrieval metadata, and integrity evidence without becoming an independent source of rule meaning.

```text
Repository Canon
      |
      v deterministic compilation
Compiled Rules Artifact
      |
      v validation and import
Runtime Searchable Representation
      |
      v bounded retrieval
Rule Packet
```

## Document Control

- **Owner:** artifact semantics, format compatibility, stable snippet identity, portable provenance, deterministic normalization, and conformance validation
- **Dependencies:** [Rule Compilation and Context-Efficient Retrieval](RULE_COMPILATION_AND_RETRIEVAL.md), [Managed Rule Publication](MANAGED_RULE_PUBLICATION.md), and the canonical [`rule-source-manifest.json`](rule-source-manifest.json) contract
- **Extensions:** FR-023 may emit this contract, FR-024 may populate reviewed retrieval vocabulary, FR-025 may acquire and import it, FR-026 may store and retrieve it, and FR-034 may package it
- **Consumers:** offline compilers, artifact validators and importers, Direct or Managed runtimes, diagnostics, and release tooling
- **Repository boundary:** the artifact contains no Campaign Canon, Campaign ID, save locator, credential, private provider locator, GM Secret, or executable host code

## Authority

Repository Markdown remains Rule Canon. Compilation may normalize, split, identify, hash, classify, and annotate source text, but it may not alter its executable meaning. An artifact is valid only while its source identity, manifest hash, Rule Source hashes, and snippet hashes continue to identify the Canon from which it was derived.

Retrieval terms, weights, relationships, selectors, indexes, and ordering are Derived metadata. They may help find a snippet but cannot add, remove, or reinterpret the rule text carried by that snippet. A runtime representation and a Rule Packet remain further Derived views.

## Provider Neutrality

Artifact semantic identity does not include its acquisition location or transport. The same valid bytes obtained from a local file, release host, mirror, private repository, removable medium, or future provider have the same validation and import meaning.

The normative contract requires no GitHub URL, Git implementation, filesystem installation layout, SQL schema, SQL Server, C#, .NET, Windows process, MCP service, or administration interface. A source identity uses an open `scheme` and immutable `value`; supported schemes are a validator or deployment compatibility decision, not an acquisition-provider privilege.

## Format 1

Artifact format `1` is UTF-8 JSON conforming to the [format-1 JSON Schema](contracts/compiled-rules-artifact-v1.schema.json) and the semantic rules in this document. The schema provides portable structural validation. Semantic validation additionally verifies ordering, identity relationships, dependencies, normalization, hashes, and compatibility.

The top-level object contains exactly:

| Property | Meaning |
| --- | --- |
| `artifactFormatVersion` | Independent artifact representation version. Format 1 uses the integer `1`. |
| `compiler` | Compiler contract compatibility plus implementation identity for diagnostics and reproduction. |
| `ruleset` | Ruleset, repository, immutable source, source-manifest path, and source-manifest hash. |
| `ruleSources` | Normalized source records that own provenance, applicability, and dependencies. |
| `snippets` | Executable normalized text units and their retrieval metadata. |
| `integrity` | Semantic digest algorithm and digest. |

Unknown or duplicate JSON properties are invalid. Required arrays may be empty only where this document explicitly permits it. At least one Rule Source and one snippet are required.

### Version and Identity Separation

The following identities are independent:

- `artifactFormatVersion` identifies the serialized artifact contract.
- `compiler.contractVersion` identifies the input/output behavior a compiler implements. Format 1 requires compiler contract `"1"`.
- `compiler.implementationId` and `compiler.implementationVersion` identify the emitting implementation without defining artifact authority.
- `ruleset.rulesetId` identifies the coherent rule family.
- `ruleset.repositoryVersion` identifies the human-facing repository or rules release represented by the source.
- `ruleset.source.scheme` and `ruleset.source.value` identify the immutable source snapshot.
- `ruleset.manifestPath` and `ruleset.manifestSha256` identify the manifest within that snapshot.
- `integrity.artifactSha256` identifies the artifact's semantic content.

Product version, artifact format, compiler contract, compiler implementation version, source release, immutable source identity, and imported Rule Release identity must not substitute for one another.

## Rule Sources

Each `ruleSources` entry contains:

- stable `ruleSourceId`;
- normalized repository-relative `sourcePath`;
- SHA-256 of the authoritative source bytes selected by the Rule Source manifest;
- `applicability` metadata;
- zero or more source-level `dependencyRuleSourceIds`.

`ruleSourceId` is the stable dependency and provenance identity already established by the Rule Source manifest. Source titles or presentation may change without changing this ID. A path change or source-content change affects provenance but does not by itself create a new Rule Source identity.

Dependencies remain source-level in format 1. Every target must exist in the same artifact, self-dependency is forbidden, and the graph must be acyclic. A later runtime may expand those source dependencies to the corresponding snippets without inventing a second dependency authority.

### Applicability

Format 1 carries the selector concepts already used by Eternal Cycle:

- `layer`: `RuntimeKernel`, `Core`, `World`, or `OptionalModule`;
- `worldModelIds`;
- `moduleIds`;
- `campaignModes`;
- `operations`;
- `topics`;
- bounded integer `priority` from `-10000` through `10000`;
- `alwaysInclude`;
- `preparationTier`: `RuntimeKernel`, `CampaignBootstrap`, `ImmediateGameplayCore`, `CampaignRelevant`, `Standard`, or `OptionalRare`.

A `World` source requires at least one World Model selector. An `OptionalModule` source requires at least one module selector. `*` is permitted in selector arrays where the existing applicability model uses an unrestricted match. Selectors classify applicability; they cannot rewrite snippet content.

## Snippets

Each snippet contains:

- `snippetId`;
- owning `ruleSourceId`;
- optional `sourceAnchor`;
- normalized executable `content`;
- `contentSha256`;
- positive `estimatedTokens`;
- a `retrieval` metadata carrier.

### Stable Snippet Identity

Format 1 computes a snippet ID as:

```text
RuleSourceId                         when sourceAnchor is absent
RuleSourceId + "#" + sourceAnchor    when sourceAnchor is present
```

The anchor is a stable compiler-defined source subdivision identifier. Array position, build time, artifact digest, token estimate, retrieval vocabulary, provider, and imported database key do not determine snippet identity. Recompilation of unchanged source boundaries therefore retains identity even when packaging or acquisition changes.

Format 1 permits one unanchored snippet per Rule Source or several uniquely anchored snippets. A compiler that changes a source boundary must do so deliberately because the changed anchor changes snippet identity.

### Executable Content

Snippet content is non-empty Unicode text encoded as UTF-8 by the JSON artifact. Before hashing or emission it must:

- use LF (`U+000A`) line endings only;
- contain no byte-order mark;
- contain no leading or trailing newline;
- preserve executable wording and meaningful internal whitespace from Repository Canon.

`contentSha256` is SHA-256 over the exact UTF-8 bytes of normalized `content`, encoded as 64 uppercase hexadecimal characters. Compiled rule text remains data supplied to the GM/runtime; it is never host code, SQL, a filesystem instruction, or a permission grant.

## Retrieval Metadata Carrier

Format 1 can carry reviewed terms and relationships needed by later compiler and retrieval work:

```text
term:          normalized text
kind:          controlled-vocabulary category
weight:        integer 1..1000

relationship:
  fromTerm:    term declared by this snippet
  toTerm:      different term declared by this snippet
  kind:        controlled relationship category
  weight:      integer 1..1000
```

Terms are NFC-normalized, trimmed, bounded to 256 characters, and use single ordinary spaces for internal whitespace. Relationships may refer only to terms declared by the same snippet and may not self-reference.

FR-022 defines this portable carrier only. FR-024 owns vocabulary generation, controlled kinds, review, collision analysis, and coverage audit. FR-026 owns runtime ranking, dependency expansion, retrieval diagnostics, and compact Rule Packet assembly. Empty term and relationship arrays are valid.

## Canonical Ordering and Normalization

Format 1 uses ordinal, locale-independent comparison. Set-like arrays must contain unique values in strictly increasing ordinal order. This applies to:

- `ruleSources` by `ruleSourceId`;
- every applicability selector array;
- each source's dependency IDs;
- `snippets` by `snippetId`;
- retrieval terms by `term`, then `kind`;
- retrieval relationships by `fromTerm`, then `toTerm`, then `kind`.

Identifiers are NFC-normalized, begin with a letter or digit, are at most 128 characters, and contain no whitespace, control character, `#`, `/`, `\`, or `:`. Paths are normalized repository-relative paths using `/`, with no empty, `.`, or `..` segment, drive/scheme separator, backslash, or leading slash. Hashes use uppercase hexadecimal.

No timestamp, random identifier, host path, locale-formatted value, provider download metadata, or array position participates in semantic identity. JSON object property order and insignificant JSON whitespace do not affect the semantic digest.

## Semantic Integrity Digest

Format 1 uses `SHA-256`. The digest is calculated over a length-prefixed UTF-8 semantic projection rather than raw JSON bytes, so property ordering and formatting cannot change artifact identity.

Each projected value is written as:

```text
<ASCII decimal UTF-8 byte length>:<UTF-8 bytes>
```

Booleans are lowercase `true` or `false`; integers use invariant decimal notation; null `sourceAnchor` projects as an empty string. A list projects its count followed by each item. The projection begins with `EC-COMPILED-RULES-V1` and then writes these values in order:

1. artifact format version;
2. compiler contract version, implementation ID, and implementation version;
3. Ruleset ID, repository version, source scheme, source value, manifest path, and manifest SHA-256;
4. Rule Source count, then each source in artifact order: ID, path, source SHA-256, layer, `worldModelIds`, `moduleIds`, `campaignModes`, `operations`, `topics`, priority, `alwaysInclude`, preparation tier, and dependency list;
5. snippet count, then each snippet in artifact order: ID, Rule Source ID, anchor or empty string, content, content SHA-256, estimated tokens, term count and each term's text/kind/weight, relationship count and each relationship's from/to/kind/weight;
6. integrity algorithm.

The `artifactSha256` value itself is excluded. Enum values use the exact case-sensitive spellings defined in this document. The final digest is 64 uppercase hexadecimal characters.

The [minimal](../../examples/conformance/compiled-rules-artifact/v1/valid-minimal.json) and [representative](../../examples/conformance/compiled-rules-artifact/v1/valid-representative.json) fixtures provide executable digest examples.

## Compatibility and Failure

An importer must reject an artifact before import when:

- the artifact format or compiler contract is unsupported;
- JSON structure is unknown, duplicated, incomplete, or malformed;
- required identity or provenance is absent or malformed;
- a path escapes or fails normalized relative-path rules;
- a source, snippet, selector, term, or relationship identity is duplicated or out of canonical order;
- a snippet references an absent source;
- a dependency target is absent, self-referential, or cyclic;
- applicability violates layer requirements;
- content normalization, content hash, manifest hash form, source hash form, or artifact digest is invalid;
- retrieval metadata violates its bounded shape or local references.

Format 1 compatibility means support for artifact format `1` and compiler contract `"1"`; it does not imply campaign compatibility, ruleset selection, source trust, or permission to activate an imported release. Acquisition metadata cannot repair an invalid artifact or override its identity.

## Trust Boundary

Validation occurs before import or activation. Consumers treat all strings as data, enforce bounded sizes and normalized paths, and never interpret an artifact as SQL, executable code, a command line, a filesystem grant, or a campaign authorization. Import cannot cross Ruleset, campaign, namespace, or permission boundaries merely because an artifact contains a matching string.

Repository Canon remains available for audit. A source/hash mismatch or unsupported contract is an explicit compatibility failure, not permission to infer intended content.

## Rich Artifact, Compact Packet

The artifact deliberately carries more portable metadata than an ordinary model-facing response. An importer may retain provenance, hashes, selectors, dependencies, and retrieval evidence while a Rule Packet carries only the smallest sufficient dependency-complete rule text and compact identity required by [runtime retrieval](RULE_COMPILATION_AND_RETRIEVAL.md#retrieval).

Artifact richness does not authorize verbose runtime delivery. FR-026 and FR-031 retain ownership of retrieval and context-economics behavior.

## Existing Index Relationship

The current reference `CompiledRuleIndex` proves source classification and bounded retrieval but is an in-process derived representation with publication-time metadata. It is not the portable artifact specification. FR-023 may adapt canonical inputs into this format, and FR-025/FR-026 may adapt a validated artifact into runtime storage, without changing the authority or semantics defined here.

The format-1 [reference validator](../../examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/CompiledRulesArtifact.cs) and [tests](../../examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/CompiledRulesArtifactContractTests.cs) demonstrate the contract in C#. Their language and implementation choices are reference-only; this document and the portable schema define the normative boundary.

## Downstream Boundaries

- **FR-023:** emits reproducible artifacts from Canon; it does not redefine format-1 semantics.
- **FR-024:** defines and audits controlled retrieval vocabulary; artifact fields alone do not generate trusted vocabulary.
- **FR-025:** acquires and imports identical artifact bytes through replaceable providers; providers do not enter semantic identity.
- **FR-026:** imports snippets into searchable storage and performs explainable retrieval and dependency closure.
- **FR-034:** packages release artifacts reproducibly; archive layout and provider publication are not format-1 semantics.
