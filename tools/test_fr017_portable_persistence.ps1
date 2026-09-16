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

$portable = Read-RepoFile 'docs/persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md'
$mcp = Read-RepoFile 'docs/persistence/MCP_PERSISTENCE_MODE.md'
$direct = Read-RepoFile 'docs/persistence/DIRECT_PERSISTENCE_MODE.md'
$schema = Read-RepoFile 'services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/Schema/001_initial.sql'
$tools = Read-RepoFile 'services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/PersistenceTools.cs'
$coordinator = Read-RepoFile 'services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/PersistenceCoordinator.cs'
$store = Read-RepoFile 'services/eternal-cycle-mcp/src/EternalCycle.Persistence.Mcp/SqlServerCampaignPersistenceStore.cs'
$roadmap = Read-RepoFile 'design/ROADMAP.md'
$future = Read-RepoFile 'design/FUTURE_REVISIONS.md'

Assert-True ($portable -match 'first-class \*\*Persistence Mode\*\*: `DIRECT` or `MCP`|first-class \*\*Persistence Mode\*\*') 'Portable architecture defines first-class persistence modes.'
Assert-True ($portable -match 'Changing mode is a controlled persistence migration') 'Mode changes require controlled migration.'
Assert-True ($direct -match 'Database Format Adapter' -and $direct -match 'Storage Adapter') 'Direct mode separates format and storage adapters.'
Assert-True ($mcp -match 'validated Persistence Receipt' -and $mcp -match 'exposes no arbitrary SQL') 'MCP mode requires receipts and forbids arbitrary SQL.'
Assert-True ($mcp -match 'MCP mode never displays `☁️💾`') 'MCP status hides backend topology.'

foreach ($required in @(
    'CREATE TABLE ec.campaigns',
    'CREATE TABLE ec.save_transactions',
    'CREATE TABLE ec.canonical_record_versions',
    'CREATE TABLE ec.record_references',
    'CREATE TABLE ec.validation_runs',
    'CREATE TABLE ec.persistence_receipts',
    'CREATE TABLE ec.recovery_points',
    'UNIQUE (campaign_id, idempotency_key)',
    'candidate_version = parent_version + 1'
)) {
    Assert-True ($schema -match [regex]::Escape($required)) "SQL Server schema preserves: $required"
}

foreach ($toolName in @('ec_persistence_status', 'ec_read_records', 'ec_commit_changes', 'ec_retry_persistence')) {
    Assert-True ($tools -match [regex]::Escape($toolName)) "Semantic MCP tool exists: $toolName"
}
Assert-True ($tools -notmatch '(?i)connectionstring|SELECT\s|INSERT\s|UPDATE\s|DELETE\s') 'MCP tool surface exposes no SQL or connection string.'
Assert-True ($coordinator -match 'validated receipt' -and $coordinator -match 'AffectedOwnerDomains must exactly match') 'Coordinator enforces receipt and Affected Set boundaries.'
Assert-True ($store -match 'INNER JOIN \{\{schema\}\}\.save_transactions AS transactions' -and $store -match 'transactions\.status IN') 'Authoritative reads exclude unactivated or failed candidate transactions through schema-routed SQL.'
Assert-True ($store -match 'ReadCandidateRecordAsync' -and $store -match 'transaction_id = @transaction_id') 'Candidate validation is scoped to the exact transaction identity.'
Assert-True ($store -notmatch '(?m)\bec\.[A-Za-z_]') 'Application persistence SQL contains no mandatory ec schema qualifier.'

Assert-True (-not (Test-Path -LiteralPath (Join-Path $root 'docs/ai/chatgpt/adapters/SQLITE_PERSISTENCE_ADAPTER.md'))) 'Obsolete ChatGPT SQLite adapter is absent.'
Assert-True (-not (Test-Path -LiteralPath (Join-Path $root 'docs/ai/chatgpt/adapters/GOOGLE_DRIVE_PERSISTENCE_ADAPTER.md'))) 'Obsolete ChatGPT Google Drive adapter is absent.'
Assert-True ($roadmap -match '\[x\] \*\*FR-017 — Portable Persistence Architecture and MCP Persistence Service\*\*') 'Roadmap records FR-017 complete.'
Assert-True ($future -match '### FR-017 - Portable Persistence Architecture and MCP Persistence Service' -and $future -match '\*\*Status:\*\* Closed') 'Future Revision register preserves FR-017 closure provenance.'

if ($failures.Count -gt 0) {
    Write-Output "FR-017 portable-persistence harness: FAIL ($($failures.Count) failure(s))"
    $failures | ForEach-Object { Write-Output "- $_" }
    throw 'FR-017 portable-persistence harness failed.'
}

Write-Output "FR-017 portable-persistence harness: PASS ($passes assertions)"
