# FR-023 Reproducible Rules Compiler Acceptance Audit

## Scope

This audit records final integration and acceptance of **FR-023 — Deterministic Offline Rules Compiler**. Implementation was delivered through bounded checkpoints A through E and accepted together in F. The objective began after the completed FR-022 format-1 contract and closes without changing Eternal Cycle `VERSION`, release tags, campaign data, runtime persistence, or release packaging.

FR-023 is a reference implementation proving an Eternal Cycle-wide reproducibility contract. It compiles an already-materialized Rule Source payload; it does not acquire that payload, import the resulting artifact, rank runtime retrieval, generate reviewed vocabulary, or publish a release.

## Delivered Pipeline

```text
ordinary materialized Rule Source files
        |
        v
MaterializedRuleSourceLoader
        |
        v
RuleSnippetCompiler
        |
        v
CompiledRulesArtifactAssembler
        |
        v
FR-022 validation
        |
        v
CompiledRulesArtifactWriter
        |
        v
standalone CLI output
```

- **A — Portable contract extraction:** moved the provider-neutral artifact contract into `EternalCycle.Rules`; Managed and standalone consumers depend on that library rather than owning duplicate normative logic.
- **B — Materialized input:** reads exact manifest and declared source bytes beneath an explicit root, validates structure and dependencies, rejects filesystem escape, and accepts caller-supplied immutable source and compiler identities.
- **C — Snippet compilation:** deterministically splits ordinary Markdown into useful heading/content snippets, preserves heading-free sources, derives stable anchors and identities, and carries applicability without creating retrieval vocabulary.
- **D — Artifact assembly and writing:** maps snapshots and candidates into the FR-022 format-1 model, validates before emission, and writes one compact canonical UTF-8 representation.
- **E — Standalone CLI:** exposes the B-to-D pipeline as an executable with explicit inputs, stable failure categories, safe output replacement, and no Managed or provider dependency.

## Acceptance Matrix

| Authoritative FR-023 requirement | Owner | Evidence | Result |
| --- | --- | --- | --- |
| Separate compiler tool | A, E | Portable `EternalCycle.Rules` library plus `EternalCycle.Rules.Compiler` executable | Satisfied jointly |
| Read the Rule Source manifest and ordinary materialized files | B | `MaterializedRuleSourceLoader`; minimal and canonical-payload tests | Satisfied by B |
| Create useful heading/content snippets | C | Heading, nesting, preamble, heading-free, duplicate-heading, fenced-code, and Unicode regressions | Satisfied by C |
| Preserve source provenance and applicability | B, C, D | Exact manifest/source bytes and hashes, inherited selectors, source dependencies, format-1 mapping | Satisfied jointly |
| Emit the FR-022 portable artifact | D | Assembler, FR-022 validation gate, and canonical writer | Satisfied by D |
| Run before gameplay without campaign state | A, E | Standalone process accepts only materialized rules and explicit identities | Satisfied jointly |
| Repeat compilation reproducibly | C, D, E | Candidate, artifact, writer, and process-level byte-identity tests | Satisfied jointly |
| Retain stable identity for unchanged snippets | C | Stable anchor and identity regressions independent of location and repeated compilation | Satisfied by C |
| Invalidate provenance when authoritative source bytes change | B, D | Exact-byte SHA-256 tests and LF/CRLF provenance distinction | Satisfied jointly |
| Reject source-root escape | B | Rooted, traversal, normalized-collision, case-collision, and filesystem-indirection tests | Satisfied by B |
| Reject malformed manifests | B, E | Closed JSON parsing, structural/dependency checks, and CLI input-failure mapping | Satisfied jointly |
| Require no MCP service or campaign data | A, E | Project-reference boundary and process-level minimal-payload compilation | Satisfied jointly |
| Do not add ranked runtime retrieval | — | FR-024/FR-026 retain vocabulary and runtime retrieval ownership | Out of FR-023 scope |
| Do not add acquisition providers | — | FR-025 retains acquisition and import ownership | Out of FR-023 scope |
| Do not add release publication | — | FR-034 retains packaging and release ownership | Out of FR-023 scope |

No authoritative FR-023 requirement was unsatisfied during final integration. FR-023F required no compiler correction.

## Provider and Dependency Boundary

The portable library has no project dependency. The standalone CLI depends only on the portable library. The Managed reference may depend on the portable library, but neither portable project depends on Managed MCP, SQL Server, campaign persistence, Git, GitHub, a network client, or a machine-specific repository layout.

An isolated minimal payload compiles through the real executable from an unrelated working directory. The canonical payload also compiles through the same interface. Acquisition provenance is explicit caller input rather than inferred from a provider, path, timestamp, process, or machine.

## Determinism and Provenance

The accepted pipeline distinguishes:

- exact authoritative manifest and source bytes;
- manifest and source SHA-256 provenance;
- normalized executable snippet content and its hash;
- stable Rule Source plus anchor snippet identity;
- the FR-022 semantic artifact digest; and
- the separate serialized artifact byte SHA-256.

Identical authoritative inputs and explicit identities produce byte-identical output across repeated runs, relocated source roots, unrelated working directories, and culture changes. Filesystem enumeration, current time, random state, process identity, machine identity, and physical source paths do not enter artifact semantics. LF and CRLF variants may normalize to equivalent executable snippets while retaining distinct exact source hashes, semantic artifact identities, and serialized bytes.

## Canonical Corpus Evidence

The actual standalone executable compiled the current canonical manifest-defined corpus with:

- 10 Rule Sources;
- 154 snippets;
- semantic digest `4B583A7904CE514B56CF3B497648DB5D778FC04DF2285D30A2E03823A32EA6A4`;
- serialized artifact byte SHA-256 `802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC`;
- serialized size 223,929 bytes.

The artifact passed FR-022 validation. Process output matched direct portable-writer bytes exactly, and repeat/relocation regressions remained byte-identical.

## Compatibility and Validation

- Portable library and standalone CLI Release builds: pass with zero warnings or errors.
- Complete portable tests: 70 of 70 pass, comprising 25 materialized-input, 21 snippet-compilation, and 24 artifact assembly/writer tests.
- Standalone CLI tests: 18 of 18 pass, including real-process minimal and canonical compilation.
- Complete Managed MCP reference suite: 181 of 181 pass.
- Focused FR-022 contract tests: 16 of 16 pass within the Managed suite.
- FR-022 structural harness: 32 of 32 assertions pass.
- Full repository validation and whitespace review: pass.

No schema or campaign migration was required. No new canonical terminology or gameplay decision was introduced; accepted FR-022 authority, identity, and retrieval-metadata decisions remain sufficient.

## Traceability and Downstream Boundaries

FR-023 completes the compiler portion of master requirement `#1`, the emission portion of `#2`, and its bounded compiler-validation contribution to `#34`. It completes the compiler foundation assigned to WP1 while leaving vocabulary quality to FR-024.

- **FR-024:** controlled retrieval vocabulary, weights, collision/coverage checks, and compiler audit remain pending.
- **FR-025:** provider-neutral artifact acquisition, validation, and import remain pending.
- **FR-026:** Compiled Rule Store, ranked retrieval, dependency expansion, and compact runtime packets remain pending.
- **FR-034:** deterministic release assets, archive layout, and publication workflow remain pending.

No downstream objective was selected or started by this closure.

## Result

FR-023 is complete and closed. Phase 13 remains active, Future Revisions remains `[∞]`, and no next objective is selected automatically.
