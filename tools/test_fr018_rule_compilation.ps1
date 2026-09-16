param(
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$failures = [System.Collections.Generic.List[string]]::new()
$passes = 0

function Assert-True {
    param([bool]$Condition, [string]$Message)
    if ($Condition) {
        $script:passes++
        if (-not $Quiet) { Write-Output "PASS: $Message" }
    }
    else {
        $script:failures.Add($Message)
        if (-not $Quiet) { Write-Output "FAIL: $Message" }
    }
}

function Read-RepoFile {
    param([string]$Path)
    return Get-Content -Raw -LiteralPath (Join-Path $root $Path)
}

$rules = Read-RepoFile 'docs/rules/RULE_COMPILATION_AND_RETRIEVAL.md'
$kernel = Read-RepoFile 'docs/rules/RUNTIME_RULE_KERNEL.md'
$manifest = Read-RepoFile 'docs/rules/rule-source-manifest.json'
$routing = Read-RepoFile 'services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/SqlServerSchemaRouting.cs'
$contracts = Read-RepoFile 'services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/Contracts.cs'
$compiler = Read-RepoFile 'services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/RuleCompilation.cs'
$store = Read-RepoFile 'services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/SqlServerCampaignPersistenceStore.cs'
$tests = Read-RepoFile 'services/eternal-cycle-mcp/tests/EternalCycle.Persistence.Mcp.Tests/SchemaRoutingAndRuleCompilationTests.cs'
$roadmap = Read-RepoFile 'design/ROADMAP.md'
$future = Read-RepoFile 'design/FUTURE_REVISIONS.md'

Assert-True ($rules -match 'Canonical Markdown remains the rule source' -and $rules -match 'Derived compiled index') 'Repository Canon remains authoritative over the compiled index.'
Assert-True ($rules -match '8,000 estimated rule tokens' -and $compiler -match 'DefaultContextBudget = 8_000') 'Documentation and implementation share the 8K normal-play target.'
Assert-True ($rules -match 'World A request does not routinely receive World B' -and $tests -match 'I_WorldSpecificRetrievalExcludesOtherWorldAndStaysWithinEightK') 'World-specific 8K regression is implemented.'
Assert-True ($kernel -match 'Every durable change has one authoritative logical owner' -and $kernel -match 'Missing information remains Unknown') 'Runtime Rule Kernel preserves ownership and unknown-information invariants.'

foreach ($metadata in @('worldModelIds', 'moduleIds', 'campaignModes', 'operations', 'topics')) {
    Assert-True ($manifest -match [regex]::Escape('"' + $metadata + '"')) "Reference manifest includes metadata: $metadata"
}

Assert-True ($contracts -match 'DefaultSchema.*=.*"ec"' -and $routing -match 'worldSchemas') 'Schema routing preserves ec as a configurable default and supports world bindings.'
Assert-True ($routing -match 'GeneratedRegex\("\^\[A-Za-z_\]' -and $routing -match 'public static string Quote') 'Schema identifiers are strictly validated and quoted.'
Assert-True ($store -notmatch '(?m)\bec\.[A-Za-z_]' -and $store -match 'CreateCampaignCommand') 'Persistence queries route through configured schema binding rather than hard-coded ec.'
Assert-True ($tests -match 'A_DefaultSchemaUsesEc' -and $tests -match 'H_DefaultSchemaIsNotMandatory') 'Default schema and non-mandatory schema regressions exist.'
Assert-True ($tests -match 'C_CompatibleCampaignsCanShareSchemaAndRemainDistinct' -and $tests -match 'E_SharedSchemaDoesNotCollapseCampaignIdentity') 'Shared-schema campaign isolation regressions exist.'
Assert-True ($tests -match 'D_DifferentWorldModelsCanUseDifferentSchemas' -and $tests -match 'F_CrossSchemaRoutesRemainIsolated') 'Different-world and cross-schema isolation regressions exist.'
Assert-True ($tests -match 'G_MigrationRenderingTargetsOnlyTheSelectedSchema') 'Schema migration scoping regression exists.'
Assert-True (-not (Test-Path -LiteralPath (Join-Path $root 'docs/rules/persistence-rules.md'))) 'Rejected SQL-authority pointer is absent.'
Assert-True ($roadmap -match '\[x\] \*\*FR-018 — Rule Compilation and Context-Efficient Retrieval\*\*') 'Roadmap records FR-018 complete.'
Assert-True ($future -match '### FR-018 - Rule Compilation and Context-Efficient Retrieval' -and $future -match '\*\*Status:\*\* Closed') 'Future Revision register preserves FR-018 closure provenance.'

if ($failures.Count -gt 0) {
    Write-Output "FR-018 rule-compilation harness: FAIL ($($failures.Count) failure(s))"
    $failures | ForEach-Object { Write-Output "- $_" }
    throw 'FR-018 rule-compilation harness failed.'
}

Write-Output "FR-018 rule-compilation harness: PASS ($passes assertions)"
