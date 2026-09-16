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

$bootstrap = Read-RepoFile 'docs/gm/CAMPAIGN_BOOTSTRAP.md'
$rulings = Read-RepoFile 'docs/gm/PROVISIONAL_RULINGS.md'
$runtime = Read-RepoFile 'docs/ai/AI_RUNTIME_MODEL.md'
$chatgpt = Read-RepoFile 'docs/ai/chatgpt/CHATGPT_GM_UNIVERSAL_INSTRUCTIONS.md'
$campaignTemplate = Read-RepoFile 'templates/CAMPAIGN_CANON_TEMPLATE.md'
$roadmap = Read-RepoFile 'design/ROADMAP.md'
$maintenanceAudit = Read-RepoFile 'design/audits/RELEASE_NEUTRAL_CAMPAIGN_STATUS_AUDIT.md'
$readme = Read-RepoFile 'README.md'
$version = (Read-RepoFile 'VERSION').Trim()

$legacyOwner = Join-Path $root 'docs/gm/ALPHA_PLAYTEST_RULES.md'
Assert-True (-not (Test-Path -LiteralPath $legacyOwner)) 'The obsolete live Alpha owner is absent.'
Assert-True ($rulings -match '^# Provisional Rulings' -and $rulings -match '## Provisional Ruling Procedure') 'One release-neutral Provisional Rulings owner exists.'

Assert-True ($bootstrap -match 'Campaign Mode: NORMAL' -and $bootstrap -match 'Start a new Eternal Cycle game') 'A normal new campaign defaults to NORMAL.'
Assert-True ($bootstrap -match 'Campaign Mode `VALIDATION`' -and $bootstrap -match 'explicit test objective') 'Explicit validation campaigns remain supported.'
Assert-True ($bootstrap -match 'Provisional ruling in normal play' -and $bootstrap -match 'Campaign Mode remains `NORMAL`') 'A Provisional Rule does not reclassify a normal campaign.'
Assert-True ($bootstrap -match 'archived Alpha-readiness audit' -and $bootstrap -match 'audit does not classify the campaign') 'Historical Alpha material does not classify current play.'
Assert-True ($bootstrap -match 'First-Life Mode without requesting a test' -and $bootstrap -match 'Campaign Mode remains `NORMAL`') 'First-Life Mode remains independent from testing status.'
Assert-True ($bootstrap -match 'Existing campaign resume' -and $bootstrap -match 'does not relabel it') 'Resume preserves authoritative Campaign Mode.'

Assert-True ($runtime -match '## New-Campaign Bootstrap Flow' -and $runtime -match 'defaults to `NORMAL`') 'The implementation-neutral runtime applies normal-default bootstrap.'
Assert-True ($chatgpt -match 'set Campaign Mode to `NORMAL`' -and $chatgpt -match 'Historical Alpha terminology never determines') 'The ChatGPT execution profile applies release-neutral bootstrap.'
Assert-True ($campaignTemplate -match '\*\*Campaign Mode:\*\* `<NORMAL \| VALIDATION \| DEVELOPMENT>`') 'Campaign Canon owns explicit Campaign Mode metadata.'

$markdown = Get-ChildItem -LiteralPath $root -Recurse -File -Filter '*.md' |
    Where-Object { $_.FullName -notlike '*\.git\*' }
$oldPathReferences = @($markdown | Select-String -Pattern '\]\([^\)\r\n]*ALPHA_PLAYTEST_RULES\.md')
Assert-True ($oldPathReferences.Count -eq 0) 'No Markdown link references the obsolete Alpha filename.'

$operationalMarkdown = @(
    Get-Item -LiteralPath (Join-Path $root 'README.md')
    Get-ChildItem -LiteralPath (Join-Path $root 'docs/ai') -Recurse -File -Filter '*.md'
    Get-ChildItem -LiteralPath (Join-Path $root 'docs/gm') -Recurse -File -Filter '*.md'
    Get-ChildItem -LiteralPath (Join-Path $root 'templates') -Recurse -File -Filter '*.md'
)
$staleOperationalPhrases = @(
    'Current Alpha Readiness',
    'Minimum Playable Alpha Scope',
    'Recommended Alpha Modes',
    'Alpha Campaign Layers',
    'ordinary alpha play',
    'safe alpha play'
)
$staleMatches = @()
foreach ($phrase in $staleOperationalPhrases) {
    $staleMatches += @($operationalMarkdown | Select-String -SimpleMatch $phrase)
}
Assert-True ($staleMatches.Count -eq 0) 'Stale live Alpha operational phrases are absent.'

Assert-True ($version -eq '1.0.0' -and $readme -match 'Status: Released') 'Release 1 metadata remains unchanged.'
Assert-True ($roadmap -match '\[x\] \*\*Release-neutral Provisional Rulings and campaign status cleanup\*\*') 'Roadmap records the maintenance item complete.'
Assert-True ($roadmap -match '\[∞\] \*\*Future Revisions\*\*') 'Future Revisions remains the permanent final Phase 13 item.'
Assert-True ($maintenanceAudit -match 'no `FR-018` was created') 'The maintenance audit preserves that this historical cleanup did not itself create a Future Revision.'

if ($failures.Count -gt 0) {
    Write-Output "Release-neutral campaign-mode harness: FAIL ($($failures.Count) failure(s))"
    $failures | ForEach-Object { Write-Output "- $_" }
    throw 'Release-neutral campaign-mode harness failed.'
}

Write-Output "Release-neutral campaign-mode harness: PASS ($passes assertions)"
