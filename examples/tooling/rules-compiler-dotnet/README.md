# Standalone Rules Compiler

This reference .NET tool compiles an already-materialized Eternal Cycle Rule Source payload into the provider-neutral [format-1 Compiled Rules Artifact](../../../docs/rules/COMPILED_RULES_ARTIFACT.md). It is standalone compilation tooling: it does not acquire sources, contact a network, access campaign state, import artifacts, publish releases, or depend on MCP, SQL Server, or the Managed Data Service.

## Projects

- `src/EternalCycle.Rules` owns the portable manifest loader, deterministic snippet compiler, format-1 assembly, validation, and canonical writer.
- `src/EternalCycle.Rules.Compiler` is the thin command-line adapter over that portable pipeline.
- `tests/EternalCycle.Rules.Tests` validates portable compilation semantics.
- `tests/EternalCycle.Rules.Compiler.Tests` validates executable behavior, diagnostics, exit codes, filesystem handling, and byte reproducibility.

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
| `3` | Materialized source or manifest input error |
| `4` | Snippet compilation error |
| `5` | Artifact assembly, contract, or serialization validation error |
| `6` | Output path or filesystem write error |
| `70` | Unexpected internal failure or cancellation |

Structured portable error codes remain visible in bounded diagnostics. Expected failures do not print stack traces or rule content.

## Reproducibility

Identical authoritative manifest and source bytes plus identical explicit identities produce byte-identical artifacts. Physical source-root location, working directory, and culture do not enter artifact identity. Executable normalization does not erase exact-byte provenance: LF and CRLF source variants may compile to equivalent normalized snippets while retaining different source hashes, semantic artifact identities, and serialized bytes.

## Vocabulary Checkpoint

The portable loader accepts the optional reviewed [`retrievalVocabulary` manifest block](../../../docs/rules/CONTROLLED_RETRIEVAL_VOCABULARY.md). FR-024A validates and normalizes that input while preserving exact manifest bytes. FR-024B's portable `RuleVocabularyEnricher.Enrich(snapshot, candidates)` resolves exact bindings and returns reviewed term associations with deterministic origin evidence beside unchanged candidates. It uses only the supplied in-memory snapshot and candidates; no source reread or acquisition is needed.

The CLI still emits the existing empty retrieval carrier: B does not wire enrichment into artifacts or normal CLI compilation. Pipeline integration, deterministic quality reports, and canonical curation remain later [FR-024 packages](../../../design/FR_024_EXECUTION_PLAN.md). No vocabulary generation, runtime ranking, or audit CLI option is claimed by this checkpoint.
