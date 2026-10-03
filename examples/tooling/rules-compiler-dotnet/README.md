# Standalone Rules Compiler

The portable library also exposes [FR-026A-E compiled retrieval](../../../docs/rules/COMPILED_RULE_STORE_RETRIEVAL.md): `CompiledRuleRetrieval.Prepare` accepts explicit Ruleset/semantic artifact identity, concrete selectors and bounded controlled query entries. `CompiledRuleCandidateIndex.Create(artifact).FindCandidates(prepared)` returns immutable applicable **unranked** snippets with exact reviewed term/concept/weight evidence and separate mandatory/explicit inclusion reasons. `CompiledRuleRanking.Rank(candidates)` orders every candidate by exact reviewed-weight sum, then descending priority and ordinal snippet ID; zero-score roots remain present. `CompiledRuleDependencies.Expand(ranked)` returns complete per-root prerequisites and a deduplicated prerequisite-first union with all direct-parent evidence. Incompatible world/module/mode/operation prerequisites fail closed. `CompiledRulePackets.Build(closure)` reserves complete required closures, then greedily admits optional closures in C order using unique stored snippet estimates. Insufficient required budget fails; optional exclusions remain service evidence. Deliver only `result.Packet` as compact model context; `Decisions`, `Members` and `Totals` preserve diagnostics without per-snippet model envelopes. CLI/compiler output and Managed runtime behavior are unchanged. Durable lookup remains pending in the [FR-026 plan](../../../design/FR_026_EXECUTION_PLAN.md).

This reference .NET tool compiles an already-materialized Eternal Cycle Rule Source payload into the provider-neutral [format-1 Compiled Rules Artifact](../../../docs/rules/COMPILED_RULES_ARTIFACT.md). It is standalone compilation tooling: it does not acquire sources, contact a network, access campaign state, import artifacts, publish releases, or depend on MCP, SQL Server, or the Managed Data Service.

## Projects

- `src/EternalCycle.Rules` owns the portable manifest loader, deterministic snippet compiler, reviewed vocabulary enrichment, format-1 assembly, validation, and canonical writer. `RuleCompilationPipeline.Compile(snapshot)` returns the artifact, canonical bytes, and separate origin evidence through one shared pipeline.
- `src/EternalCycle.Rules.Compiler` is the thin command-line adapter over that portable pipeline.
- `tests/EternalCycle.Rules.Tests` validates portable compilation semantics.
- `tests/EternalCycle.Rules.Compiler.Tests` validates executable behavior, diagnostics, exit codes, filesystem handling, and byte reproducibility.

## Observational Audit

Use `audit` in place of `compile` with the same required options and a **separate** `--output` path. It runs the shared portable compilation pipeline in memory and writes only deterministic audit JSON, not another artifact. `RuleVocabularyAuditor.Analyze(snapshot, compilation, policy)` and `RuleVocabularyAuditWriter.Write(report)` are the direct-library equivalents.

Optional `--audit-policy <path>` accepts a closed JSON object, for example:

```json
{"minimumBroadTargets":8,"minimumBroadCoveragePercent":50,"largeSnippetTokens":2048,"genericTerms":[]}
```

All fields are optional; defaults are shown above. Generic terms are explicit reviewer input, not a built-in word list. The [audit contract](../../../docs/rules/CONTROLLED_RETRIEVAL_VOCABULARY.md#observational-quality-audit) defines exact metric arithmetic, severity, stable finding codes, and ordering. Ratios are unrounded numerator/denominator pairs; a zero denominator is undefined. Warnings never mutate metadata or invalidate ordinary compilation.

Reports include covered/uncovered snippets, unused concepts, term/concept/source target sets, coalesced origins, source-only inheritance, exclusive binding support, and explicit breadth/generic/token-boundary review signals. They contain no runtime query scores. Audit output uses existing atomic writes and input protection, may replace only another format-1 audit report, and cannot replace an artifact, policy, source, manifest, directory, or unrecognized prior file. Compile likewise refuses an existing audit report. No report or output parent is created on invalid input.

## Usage

```powershell
dotnet run --project .\src\EternalCycle.Rules.Compiler\EternalCycle.Rules.Compiler.csproj -- `
  compile `
  --source-root C:\Rules\Materialized `
  --manifest docs/rules/rule-source-manifest.json `
  --source-scheme content-tree-sha256 `
  --source-value 0123456789ABCDEF `
  --compiler-id eternal-cycle-dotnet `
  --compiler-version 1.0.0 `
  --output C:\Rules\Output\eternal-cycle-compiled-rules.json
```

Run with `--help` or `compile --help` for the option summary. The source scheme and value identify an immutable source selected outside this compiler; the compiler never infers them from Git, directory names, timestamps, or machine state.

The output parent directory is created when missing. An existing ordinary output file is atomically replaced through a temporary file in the same directory. The compiler rejects output paths that name a directory, the manifest, or a declared Rule Source file. Upstream validation failure produces no output artifact.

Successful diagnostics report the output path, Rule Source and snippet counts, semantic artifact digest, and separately labelled serialized-byte SHA-256. Artifact bytes remain the exact compact UTF-8 output of the canonical writer, without a byte-order mark or appended newline.

## Exit Codes

| Code | Category |
| ---: | --- |
| `0` | Success or help |
| `2` | Command or argument usage error |
| `3` | Materialized source, manifest, or audit-policy input error |
| `4` | Snippet compilation or reviewed vocabulary enrichment error |
| `5` | Artifact validation/emission or invalid audit evidence |
| `6` | Output path or filesystem write error |
| `70` | Unexpected internal failure or cancellation |

Structured portable error codes remain visible in bounded diagnostics. Expected failures do not print stack traces or rule content.

## Reproducibility

Identical authoritative manifest and source bytes plus identical explicit identities produce byte-identical artifacts. Physical source-root location, working directory, and culture do not enter artifact identity. Executable normalization does not erase exact-byte provenance: LF and CRLF source variants may compile to equivalent normalized snippets while retaining different source hashes, semantic artifact identities, and serialized bytes.

## Vocabulary Checkpoint

The completed [FR-025 contract](../../../docs/rules/COMPILED_RULES_ACQUISITION_AND_IMPORT.md) acquires bounded exact bytes through local/GitHub providers and shares `CompiledRulesArtifactValidation.ValidateAsync` with required explicit trust. E proves custom-provider replaceability. `CompiledRulesArtifactImport.ImportAsync` accepts only immutable B-approved output, an authorized Ruleset ID and a configured storage adapter; it returns a compact created/reused receipt. The [SQL reference](../managed-data/mcp-dotnet-tsql/README.md#compiled-artifact-import) stores complete format-1 semantics and exact first-approved bytes atomically without publication/activation. The portable library has no SQL/MCP dependency. No default trust, CLI acquisition/import command or compiler-output change is introduced; [G's audit](../../../design/audits/FR_025_ACQUISITION_AND_IMPORT_AUDIT.md) records integrated acceptance.

Normal `compile` automatically processes the optional reviewed [`retrievalVocabulary` manifest block](../../../docs/rules/CONTROLLED_RETRIEVAL_VOCABULARY.md). A validates input, B resolves exact bindings, and C projects each association into a format-1 term: normalized wording, reviewed concept ID as `kind`, and exact reviewed weight. Using concept categories preserves shared terms without conflicting weights. Canonical/alias/phrase origin and review rationale remain in the portable compiler result for observational audit, not in every artifact term.

No-vocabulary compilation retains exact legacy output. Reviewed metadata and exact manifest provenance participate in the existing semantic digest; even normalized-equivalent declarations with different authoritative bytes retain different provenance. D adds the separate observational `audit` command; normal compilation is unchanged. E adds the inspected canonical manifest vocabulary and [quality fixtures](fixtures/canonical-vocabulary/quality-expectations.json), with [explicit reviewer policy](fixtures/canonical-vocabulary/audit-policy.json) and a [before/after audit](../../../design/audits/FR_024E_CANONICAL_VOCABULARY_AUDIT.md). Apply that policy through `--audit-policy` when reviewing the corpus. The frozen pre-E payload preserves historical byte evidence across checkout newline conversions. The [FR-024 closure audit](../../../design/audits/FR_024_CONTROLLED_RETRIEVAL_VOCABULARY_AUDIT.md) records integrated mutation, determinism, CLI, quality, and compatibility acceptance. No scoring, synonym generation, or runtime retrieval is added; downstream implementation requires separate authorization.

The small [integration manifest](fixtures/reviewed-vocabulary/manifest.json) with [core](fixtures/reviewed-vocabulary/core.txt) and [operations](fixtures/reviewed-vocabulary/operations.txt) text is test material only, not canonical Eternal Cycle vocabulary or mechanics. It exercises shared wording, precise/source-wide bindings, overlapping origins, and CLI/library equality.
