param([switch]$Quiet)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$failures = [System.Collections.Generic.List[string]]::new()
$passes = 0

function Read-RepoFile([string]$Path) {
    Get-Content -Raw -LiteralPath (Join-Path $root $Path)
}

function Assert-Requirement([int]$Id, [bool]$Condition, [string]$Message) {
    if ($Condition) {
        $script:passes++
        if (-not $Quiet) { Write-Output "PASS [$Id]: $Message" }
    }
    else {
        $script:failures.Add("[$Id] $Message")
        if (-not $Quiet) { Write-Output "FAIL [$Id]: $Message" }
    }
}

$document = Read-RepoFile 'docs/rules/COMPILED_RULES_ARTIFACT.md'
$schemaText = Read-RepoFile 'docs/rules/contracts/compiled-rules-artifact-v1.schema.json'
$minimalText = Read-RepoFile 'examples/conformance/compiled-rules-artifact/v1/valid-minimal.json'
$representativeText = Read-RepoFile 'examples/conformance/compiled-rules-artifact/v1/valid-representative.json'
$reference = Read-RepoFile 'examples/tooling/rules-compiler-dotnet/src/EternalCycle.Rules/CompiledRulesArtifact.cs'
$tests = Read-RepoFile 'examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/CompiledRulesArtifactContractTests.cs'
$testProject = Read-RepoFile 'examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/EternalCycle.Persistence.Mcp.Tests.csproj'
$decisions = Read-RepoFile 'design/DECISIONS.md'
$terms = Read-RepoFile 'design/TERMINOLOGY.md'
$roadmap = Read-RepoFile 'design/ROADMAP.md'
$future = Read-RepoFile 'design/FUTURE_REVISIONS.md'
$audit = Read-RepoFile 'design/audits/FR_022_COMPILED_RULES_ARTIFACT_AUDIT.md'

$schema = $schemaText | ConvertFrom-Json -Depth 100
$minimal = $minimalText | ConvertFrom-Json -Depth 100
$representative = $representativeText | ConvertFrom-Json -Depth 100

Assert-Requirement 1 ($document -match 'Repository Canon\s*\n\s*\|\s*\n\s*\|?\s*v deterministic compilation' -and $document -match 'Compiled Rules Artifact') 'The normative authority chain keeps the artifact Derived from Repository Canon.'
Assert-Requirement 2 ($document -match 'normative contract requires no GitHub' -and $document -match 'SQL Server' -and $document -match 'C#' -and $document -match 'Windows') 'The normative contract explicitly rejects provider and implementation lock-in.'
Assert-Requirement 3 ($document -match 'RuleSourceId.*sourceAnchor' -and $document -match 'Array position, build time') 'Stable snippet identity excludes volatile and packaging inputs.'
Assert-Requirement 4 ($document -match 'LF \(`U\+000A`\)' -and $document -match 'no byte-order mark') 'Executable content normalization is explicit.'
Assert-Requirement 5 ($document -match 'Every target must exist in the same artifact' -and $document -match 'graph must be acyclic') 'Portable dependencies require existing acyclic Rule Source references.'
Assert-Requirement 6 ($document -match 'FR-024 owns vocabulary generation' -and $document -match 'FR-026 owns runtime ranking') 'Retrieval metadata is a carrier and downstream algorithm ownership remains separate.'
Assert-Requirement 7 ($document -match 'length-prefixed UTF-8 semantic projection' -and $document -match 'EC-COMPILED-RULES-V1') 'The artifact digest has an implementation-independent deterministic definition.'
Assert-Requirement 8 ($document -match 'Compiled rule text remains data' -and $document -match 'never host code') 'Compiled rules remain validated data rather than executable host authority.'

Assert-Requirement 9 ($schema.'$schema' -eq 'https://json-schema.org/draft/2020-12/schema' -and $schema.'$id' -eq 'urn:eternal-cycle:compiled-rules-artifact:1') 'The portable schema parses as Draft 2020-12 JSON with a stable contract identity.'
Assert-Requirement 10 ($schema.properties.artifactFormatVersion.const -eq 1 -and $schema.'$defs'.compiler.properties.contractVersion.const -eq '1') 'Schema compatibility fixes artifact format 1 and compiler contract 1.'
Assert-Requirement 11 ($schema.additionalProperties -eq $false -and $schema.'$defs'.snippet.additionalProperties -eq $false) 'Schema objects are closed against accidental provider or implementation fields.'
Assert-Requirement 12 ($schemaText -notmatch '(?i)github\.com|sqlserver|mssql|windows|dotnet|providerUrl|downloadUrl') 'The schema names no mandatory acquisition provider, host, database, or runtime.'
Assert-Requirement 13 ($schema.'$defs'.applicability.properties.layer.enum.Count -eq 4 -and $schema.'$defs'.applicability.properties.preparationTier.enum.Count -eq 6) 'Schema applicability reuses the established layer and preparation-tier vocabularies.'

Assert-Requirement 14 ($minimal.artifactFormatVersion -eq 1 -and $minimal.ruleSources.Count -eq 1 -and $minimal.snippets.Count -eq 1) 'The minimal fixture is intentionally small and structurally representative.'
Assert-Requirement 15 ($minimal.snippets[0].snippetId -eq $minimal.snippets[0].ruleSourceId -and $null -eq $minimal.snippets[0].sourceAnchor) 'The minimal fixture proves stable unanchored snippet identity.'
Assert-Requirement 16 ($representative.ruleSources.Count -eq 2 -and $representative.snippets.Count -eq 2) 'The representative fixture contains multiple sources and snippets.'
Assert-Requirement 17 ($representative.ruleSources[0].dependencyRuleSourceIds.Count -eq 1 -and $representative.ruleSources[0].dependencyRuleSourceIds[0] -eq $representative.ruleSources[1].ruleSourceId) 'The representative fixture contains a resolvable dependency.'
Assert-Requirement 18 ($representative.snippets[0].retrieval.terms.Count -gt 1 -and $representative.snippets[0].retrieval.relationships.Count -eq 1) 'The representative fixture exercises retrieval terms and relationships.'
Assert-Requirement 19 ($minimal.integrity.artifactSha256 -match '^[0-9A-F]{64}$' -and $representative.integrity.artifactSha256 -match '^[0-9A-F]{64}$') 'Both fixtures carry uppercase SHA-256 semantic digests.'

Assert-Requirement 20 ($reference -match 'CurrentArtifactFormatVersion = 1' -and $reference -match 'CurrentCompilerContractVersion = "1"') 'The reference validator exposes the supported compatibility boundary.'
Assert-Requirement 21 ($reference -match 'DUPLICATE_JSON_PROPERTY' -and $reference -match 'UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow') 'The reference validator rejects duplicate and unknown JSON properties.'
Assert-Requirement 22 ($reference -match 'DEPENDENCY_TARGET_MISSING' -and $reference -match 'DEPENDENCY_CYCLE') 'The reference validator checks dependency targets and cycles.'
Assert-Requirement 23 ($reference -match 'ComputeContentSha256' -and $reference -match 'ComputeArtifactSha256' -and $reference -match 'DigestWriter') 'The reference validator checks content and semantic artifact integrity.'
Assert-Requirement 24 ($reference -match 'STRUCTURE_REQUIRED' -and $reference -match 'RULE_LAYER_UNSUPPORTED' -and $reference -match 'PREPARATION_TIER_UNSUPPORTED') 'Malformed required structure and enum values fail semantically rather than escaping validation.'
Assert-Requirement 25 ($tests -match 'ValidConformanceFixturesPass' -and $tests -match 'DuplicateSnippetIdentityIsRejected' -and $tests -match 'MissingDependencyTargetIsRejected') 'Focused tests cover valid fixtures, duplicate identity, and missing dependencies.'
Assert-Requirement 26 ($tests -match 'UnsupportedFormatAndCompilerContractsAreRejected' -and $tests -match 'ArtifactIntegrityMismatchIsRejected' -and $tests -match 'SourceIdentitySchemeDoesNotSelectAnAcquisitionProvider') 'Focused tests cover compatibility, integrity, and provider-neutral source identity.'
Assert-Requirement 27 ($testProject -match 'examples\\conformance\\compiled-rules-artifact\\v1\\\*\.json' -and $testProject -match 'Fixtures\\CompiledRulesArtifact') 'Conformance fixtures are copied into the reference test output without runtime provider coupling.'

Assert-Requirement 28 ($decisions -match 'D-1355' -and $decisions -match 'D-1356' -and $decisions -match 'D-1357') 'Accepted decisions record derivative authority, stable identity, and retrieval-metadata boundaries.'
Assert-Requirement 29 ($terms -match '## Compiled Rules Artifact' -and $terms -match '## Artifact Format Version' -and $terms -match '## Rule Snippet ID' -and $terms -match '## Artifact Integrity Digest') 'Canonical terminology names the artifact contract without redundant synonyms.'
Assert-Requirement 30 ($roadmap -match '- \[x\] \*\*FR-022' -and $roadmap -match '- \[x\] \*\*FR-023' -and $roadmap -match '- \[x\] \*\*FR-024') 'Roadmap preserves FR-022/FR-023/FR-024 closure without selecting downstream work.'
Assert-Requirement 31 ($future -match '(?s)## Closed.*### FR-022 - Portable Compiled Rules Artifact Contract.*\*\*Status:\*\* Closed' -and $future -match '(?s)## Closed.*### FR-023 - Deterministic Offline Rules Compiler.*\*\*Status:\*\* Closed' -and $future -match '(?s)## Closed.*### FR-024 - Retrieval Vocabulary and Compiler Audit.*\*\*Status:\*\* Closed' -and $future -match 'FR-025 through FR-036 are Roadmapped') 'Future Revision provenance preserves completed objectives while downstream work remains Roadmapped.'
Assert-Requirement 32 ($audit -match 'No SQL or campaign-schema migration was required' -and $audit -match 'No unresolved architectural question blocks FR-022') 'The audit records migration scope and a clear unresolved-question result.'

if ($failures.Count -gt 0) {
    Write-Output "FR-022 compiled-rules artifact harness: FAIL ($($failures.Count) failure(s))"
    $failures | ForEach-Object { Write-Output "- $_" }
    throw 'FR-022 compiled-rules artifact harness failed.'
}

Write-Output "FR-022 compiled-rules artifact harness: PASS ($passes assertions)"
