[CmdletBinding()]
param(
    [string]$Root
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Root)) {
    $Root = Split-Path -Parent $PSScriptRoot
}

$rootPath = (Resolve-Path -LiteralPath $Root).Path.TrimEnd([IO.Path]::DirectorySeparatorChar)
$errors = [System.Collections.Generic.List[string]]::new()

function Add-ValidationError {
    param([string]$Message)
    $script:errors.Add($Message)
}

function Get-RepositoryPath {
    param([string]$Path)
    return $Path.Substring($script:rootPath.Length + 1).Replace('\', '/')
}

function Get-GitHubSlug {
    param([string]$Heading)

    $slug = $Heading.Trim().ToLowerInvariant()
    $slug = [regex]::Replace($slug, '<[^>]+>', '')
    $slug = [regex]::Replace($slug, '[`*_~]', '')
    $slug = [regex]::Replace($slug, '[^\p{L}\p{Nd}\s-]', '')
    return [regex]::Replace($slug, '\s', '-')
}

function Get-HeadingSlugs {
    param([string]$Path)

    $slugs = @{}
    $seen = @{}
    foreach ($line in Get-Content -LiteralPath $Path) {
        if ($line -notmatch '^#{1,6}\s+(.+?)\s*$') {
            continue
        }

        $base = Get-GitHubSlug $Matches[1]
        if ($seen.ContainsKey($base)) {
            $seen[$base]++
            $slug = "$base-$($seen[$base])"
        }
        else {
            $seen[$base] = 0
            $slug = $base
        }
        $slugs[$slug] = $true
    }
    return $slugs
}

function New-PathSet {
    return ,([System.Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase))
}

$markdownFiles = @(Get-ChildItem -LiteralPath $rootPath -Recurse -File -Filter '*.md' |
    Where-Object { $_.FullName -notlike '*\.git\*' })
$knownMarkdown = New-PathSet
$incoming = @{}
$targetsBySource = @{}

foreach ($file in $markdownFiles) {
    $null = $knownMarkdown.Add($file.FullName)
    $incoming[$file.FullName] = 0
    $targetsBySource[$file.FullName] = New-PathSet
}

$relativeLinks = 0
$anchorLinks = 0
$anchorCache = @{}
$linkPattern = '!?\[[^\]]*\]\((?<destination><[^>]+>|[^\s\)]+)(?:\s+[^\)]*)?\)'

foreach ($file in $markdownFiles) {
    $content = Get-Content -Raw -LiteralPath $file.FullName
    foreach ($match in [regex]::Matches($content, $linkPattern)) {
        $destination = $match.Groups['destination'].Value.Trim('<', '>')
        if ($destination -match '^[a-z][a-z0-9+.-]*:') {
            continue
        }

        $parts = $destination.Split('#', 2)
        $pathPart = [Uri]::UnescapeDataString($parts[0])
        $hasAnchor = $parts.Count -eq 2 -and -not [string]::IsNullOrWhiteSpace($parts[1])

        if ([string]::IsNullOrWhiteSpace($pathPart)) {
            $targetPath = $file.FullName
        }
        elseif ([IO.Path]::IsPathRooted($pathPart)) {
            Add-ValidationError "Absolute local link in $(Get-RepositoryPath $file.FullName): $destination"
            continue
        }
        else {
            $targetPath = [IO.Path]::GetFullPath((Join-Path $file.DirectoryName $pathPart))
            $relativeLinks++
        }

        $insideRoot = $targetPath.Equals($rootPath, [StringComparison]::OrdinalIgnoreCase) -or
            $targetPath.StartsWith($rootPath + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)
        if (-not $insideRoot) {
            Add-ValidationError "Link escapes repository in $(Get-RepositoryPath $file.FullName): $destination"
            continue
        }

        if (-not (Test-Path -LiteralPath $targetPath)) {
            Add-ValidationError "Missing target in $(Get-RepositoryPath $file.FullName): $destination"
            continue
        }

        $null = $targetsBySource[$file.FullName].Add($targetPath)
        if ($knownMarkdown.Contains($targetPath) -and
            -not $targetPath.Equals($file.FullName, [StringComparison]::OrdinalIgnoreCase)) {
            $incoming[$targetPath]++
        }

        if (-not $hasAnchor) {
            continue
        }

        $anchorLinks++
        if ((Get-Item -LiteralPath $targetPath).PSIsContainer -or
            [IO.Path]::GetExtension($targetPath) -ne '.md') {
            Add-ValidationError "Anchor targets a non-Markdown file in $(Get-RepositoryPath $file.FullName): $destination"
            continue
        }

        if (-not $anchorCache.ContainsKey($targetPath)) {
            $anchorCache[$targetPath] = Get-HeadingSlugs $targetPath
        }
        $wantedAnchor = [Uri]::UnescapeDataString($parts[1]).ToLowerInvariant()
        if (-not $anchorCache[$targetPath].ContainsKey($wantedAnchor)) {
            Add-ValidationError "Missing anchor in $(Get-RepositoryPath $file.FullName): $destination"
        }
    }
}

function Assert-TargetsLinked {
    param(
        [string]$Source,
        [System.IO.FileInfo[]]$Expected,
        [string]$Label
    )

    $sourcePath = Join-Path $script:rootPath $Source
    if (-not (Test-Path -LiteralPath $sourcePath)) {
        Add-ValidationError "Missing navigation source: $Source"
        return
    }

    foreach ($target in $Expected) {
        if (-not $script:targetsBySource[$sourcePath].Contains($target.FullName)) {
            Add-ValidationError "$Label does not link $(Get-RepositoryPath $target.FullName)"
        }
    }
}

$docsRoot = Join-Path $rootPath 'docs'
$registryPath = Join-Path $docsRoot 'DOCUMENT_REGISTRY.md'
$docsExpected = @($markdownFiles | Where-Object {
    $_.FullName.StartsWith($docsRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -and
    -not $_.FullName.Equals($registryPath, [StringComparison]::OrdinalIgnoreCase)
})
Assert-TargetsLinked 'docs/DOCUMENT_REGISTRY.md' $docsExpected 'Canonical Document Registry'

$familyIndexes = @(Get-ChildItem -LiteralPath $docsRoot -Directory | ForEach-Object {
    $index = Join-Path $_.FullName 'README.md'
    if (-not (Test-Path -LiteralPath $index)) {
        Add-ValidationError "Missing documentation family index: docs/$($_.Name)/README.md"
        return
    }
    Get-Item -LiteralPath $index
})
Assert-TargetsLinked 'docs/README.md' $familyIndexes 'Canonical Rules Map'

foreach ($index in $familyIndexes) {
    $familyFiles = @(Get-ChildItem -LiteralPath $index.DirectoryName -File -Filter '*.md' |
        Where-Object { -not $_.FullName.Equals($index.FullName, [StringComparison]::OrdinalIgnoreCase) })
    Assert-TargetsLinked (Get-RepositoryPath $index.FullName) $familyFiles "$(Get-RepositoryPath $index.FullName)"
}

$designFiles = @(Get-ChildItem -LiteralPath (Join-Path $rootPath 'design') -File -Filter '*.md' |
    Where-Object { $_.Name -ne 'README.md' })
Assert-TargetsLinked 'design/README.md' $designFiles 'Design index'

$auditFiles = @(Get-ChildItem -LiteralPath (Join-Path $rootPath 'design/audits') -File -Filter '*.md' |
    Where-Object { $_.Name -ne 'README.md' })
Assert-TargetsLinked 'design/audits/README.md' $auditFiles 'Audit index'

$templateFiles = @(Get-ChildItem -LiteralPath (Join-Path $rootPath 'templates') -File -Filter '*.md' |
    Where-Object { $_.Name -ne 'README.md' })
Assert-TargetsLinked 'templates/README.md' $templateFiles 'Template index'

$coverageFiles = @($templateFiles | Where-Object { $_.Name -ne 'TEMPLATE_COVERAGE.md' })
Assert-TargetsLinked 'templates/TEMPLATE_COVERAGE.md' $coverageFiles 'Template coverage map'

$agentFiles = @(Get-ChildItem -LiteralPath (Join-Path $rootPath 'agents') -File -Filter '*.md' |
    Where-Object { $_.Name -ne 'README.md' })
Assert-TargetsLinked 'agents/README.md' $agentFiles 'Agent index'

$rootRequirements = @(
    'agents/README.md',
    'design/README.md',
    'docs/README.md',
    'docs/DOCUMENT_REGISTRY.md',
    'templates/README.md'
) | ForEach-Object { Get-Item -LiteralPath (Join-Path $rootPath $_) }
Assert-TargetsLinked 'README.md' $rootRequirements 'Root README'

$rootReadme = Join-Path $rootPath 'README.md'
$orphans = @($markdownFiles | Where-Object {
    $_.FullName -ne $rootReadme -and $incoming[$_.FullName] -eq 0
})
foreach ($orphan in $orphans) {
    Add-ValidationError "Orphaned Markdown document: $(Get-RepositoryPath $orphan.FullName)"
}

$terminologyPath = Join-Path $rootPath 'design/TERMINOLOGY.md'
$termHeadings = @(Get-Content -LiteralPath $terminologyPath |
    Where-Object { $_ -match '^## ' } |
    ForEach-Object { $_.Substring(3).Trim() })
$normalizedTerms = @($termHeadings | ForEach-Object {
    ($_ -replace '[\s_-]+', ' ').Trim().ToLowerInvariant()
})
foreach ($duplicate in $normalizedTerms | Group-Object | Where-Object { $_.Count -gt 1 }) {
    Add-ValidationError "Duplicate normalized terminology heading: $($duplicate.Name)"
}

$roadmapPath = Join-Path $rootPath 'design/ROADMAP.md'
$roadmap = Get-Content -Raw -LiteralPath $roadmapPath
$phaseMatches = [regex]::Matches($roadmap, '(?m)^\*\*Current phase: (.+)\*\*$')
$taskMatches = [regex]::Matches($roadmap, '(?m)^\*\*Current task: (.+)\*\*$')
$roadmapStatuses = [regex]::Matches($roadmap, '(?m)^- \[(.)\] ')
if ($phaseMatches.Count -ne 1) {
    Add-ValidationError "Roadmap must contain exactly one Current phase declaration."
}
if ($taskMatches.Count -ne 1) {
    Add-ValidationError "Roadmap must contain exactly one Current task declaration."
}
foreach ($status in $roadmapStatuses) {
    if (' ', 'x', '~', '!', '∞' -notcontains $status.Groups[1].Value) {
        Add-ValidationError "Invalid roadmap status token: [$($status.Groups[1].Value)]"
    }
}
$finalRepositoryState = $roadmap -match '(?m)^\*\*Repository Status: Feature Complete — Gameplay Validation Ongoing\*\*$'
if ($finalRepositoryState) {
    $unfinished = @($roadmapStatuses | Where-Object { $_.Groups[1].Value -notin @('x', '∞') })
    $phase12Active = $phaseMatches.Count -eq 1 -and $phaseMatches[0].Groups[1].Value -eq 'Phase 12 — Gameplay Validation & Maintenance'
    if ($unfinished.Count -gt 0 -and -not $phase12Active) {
        Add-ValidationError "Feature-complete roadmap contains $($unfinished.Count) unfinished checklist item(s)."
    }
    if ($phaseMatches.Count -eq 1 -and $phaseMatches[0].Groups[1].Value -ne 'Phase 12 — Gameplay Validation & Maintenance') {
        Add-ValidationError "Feature-complete roadmap must identify Phase 12 — Gameplay Validation & Maintenance as the current phase."
    }
}
elseif ($taskMatches.Count -eq 1) {
    $task = [regex]::Escape($taskMatches[0].Groups[1].Value)
    if ($roadmap -notmatch "(?m)^- \[[ ~!]\] $task$") {
        Add-ValidationError "Current roadmap task is not an open, partial, or blocked checklist item."
    }
}

foreach ($requiredText in @(
    '## Phase 12 — Gameplay Validation & Maintenance',
    '**Status: Active**',
    '**Selected implementation objective:** None.',
    '**Approved pending objectives:** FR-011, the refined remaining scope of FR-015, and FR-016.',
    '- [x] **FR-001 — Retained Development and Embodiment Relevance**',
    '- [x] **FR-002 — Reincarnation Candidate Selection**',
    '- [x] **FR-003 — Soul Depth Information Visibility**',
    '- [x] **FR-004 — Life Archive and Old-Soul Indexing**',
    '- [x] **FR-005 — Cross-Embodiment Skill Transfer**',
    '- [x] **FR-006 — Adaptive Skill Consolidation and Merge Rules**',
    '- [x] **FR-007 — Conceptual Skill Scope and Historical Interpretation**',
    '- [x] **FR-008 — Soul Weapon Rarity and Equipment Relevance**',
    '- [x] **FR-009 — World Contact, Travel, and Reincarnation Discretion**',
    '- [x] **FR-010 — Long-Horizon Simulation Summaries**',
    '- [ ] **FR-011 — GM/AI Context Assembly, Mandatory Read Discipline, and Gameplay Turn Persistence**',
    '- [x] **FR-012 — Canonical SQL Ownership and Anti-Duplication**',
    '- [x] **FR-014 — Autonomous Registry**',
    '- [ ] **FR-015 — Memory Continuity, Fading, and Recall**',
    '- [ ] **FR-016 — Soul-Bound Companion Fate & Reincarnation Continuity**',
    '- [∞] **Future Revisions**',
    'Phase 12 does not complete because current objectives pass validation',
    'Phase 13 — Future Revisions'
)) {
    if ($roadmap -notmatch [regex]::Escape($requiredText)) {
        Add-ValidationError "Roadmap lacks required Phase 12 governance: $requiredText"
    }
}
if ($roadmap -match '(?m)^## Phase 13') {
    Add-ValidationError 'Phase 13 must not begin before explicit maintainer release-readiness authorization.'
}
$phase12Section = [regex]::Match($roadmap, '(?ms)^## Phase 12 — Gameplay Validation & Maintenance\s*(?<body>.*)\z')
if (-not $phase12Section.Success) {
    Add-ValidationError 'Phase 12 must be the final top-level phase in the current roadmap.'
}
elseif ($phase12Section.Groups['body'].Value -notmatch '(?s)- \[∞\] \*\*Future Revisions\*\*.*\z') {
    Add-ValidationError 'Future Revisions must remain the final rolling Phase 12 objective.'
}

$unresolvedPath = Join-Path $rootPath 'design/UNRESOLVED_QUESTIONS.md'
$unresolved = Get-Content -Raw -LiteralPath $unresolvedPath
$blockingSection = [regex]::Match($unresolved, '(?ms)^## Blocking\s*(?<body>.*?)(?=^## )')
if (-not $blockingSection.Success) {
    Add-ValidationError "Unresolved Questions lacks a Blocking section."
}
elseif ($blockingSection.Groups['body'].Value -match '(?m)^- ') {
    Add-ValidationError "Blocking unresolved questions remain."
}

$futurePath = Join-Path $rootPath 'design/FUTURE_REVISIONS.md'
$future = Get-Content -Raw -LiteralPath $futurePath
$openRegister = [regex]::Match($future, '(?ms)^## Open Register\s*(?<body>.*?)(?=^## Roadmapped)')
$futureEntries = @()
$roadmappedEntries = @()
$closedEntries = @()
if (-not $openRegister.Success) {
    Add-ValidationError "Future Revisions lacks an Open Register section."
}
else {
    $futureEntries = @([regex]::Matches(
        $openRegister.Groups['body'].Value,
        '(?ms)^### (?<id>FR-[0-9]{3}) - .+?(?=^### FR-[0-9]{3} - |\z)'
    ))
    $requiredFutureFields = @(
        'Status',
        'Issue',
        'Affected systems',
        'Gameplay impact',
        'Evidence needed',
        'Suggested future phase',
        'Priority'
    )
    foreach ($entry in $futureEntries) {
        foreach ($field in $requiredFutureFields) {
            $escapedField = [regex]::Escape($field)
            if ($entry.Value -notmatch "(?m)^- \*\*$escapedField`:\*\* .+") {
                Add-ValidationError "$($entry.Groups['id'].Value) lacks required field: $field"
            }
        }
    }
}

$roadmappedRegister = [regex]::Match($future, '(?ms)^## Roadmapped\s*(?<body>.*?)(?=^## Closed)')
if (-not $roadmappedRegister.Success) {
    Add-ValidationError "Future Revisions lacks a Roadmapped section."
}
else {
    $roadmappedEntries = @([regex]::Matches(
        $roadmappedRegister.Groups['body'].Value,
        '(?ms)^### (?<id>FR-[0-9]{3}) - .+?(?=^### FR-[0-9]{3} - |\z)'
    ))
    foreach ($entry in $roadmappedEntries) {
        foreach ($field in @('Status', 'Issue', 'Affected systems', 'Gameplay impact', 'Evidence needed', 'Suggested future phase', 'Priority', 'Status reason', 'Authorized roadmap link')) {
            $escapedField = [regex]::Escape($field)
            if ($entry.Value -notmatch "(?m)^- \*\*$escapedField`:\*\* .+") {
                Add-ValidationError "$($entry.Groups['id'].Value) lacks required roadmapped field: $field"
            }
        }
        if ($entry.Value -notmatch '(?m)^- \*\*Status:\*\* Roadmapped$') {
            Add-ValidationError "$($entry.Groups['id'].Value) in Roadmapped does not have Roadmapped status."
        }
    }
}

$closedRegister = [regex]::Match($future, '(?ms)^## Closed\s*(?<body>.*?)(?=^## Blank Entry Contract)')
if (-not $closedRegister.Success) {
    Add-ValidationError "Future Revisions lacks a Closed section."
}
else {
    $closedEntries = @([regex]::Matches(
        $closedRegister.Groups['body'].Value,
        '(?ms)^### (?<id>FR-[0-9]{3}) - .+?(?=^### FR-[0-9]{3} - |\z)'
    ))
    foreach ($entry in $closedEntries) {
        foreach ($field in @('Status', 'Issue', 'Affected systems', 'Gameplay impact', 'Evidence needed', 'Suggested future phase', 'Priority', 'Status reason', 'Authorized roadmap link', 'Closure references')) {
            $escapedField = [regex]::Escape($field)
            if ($entry.Value -notmatch "(?m)^- \*\*$escapedField`:\*\* .+") {
                Add-ValidationError "$($entry.Groups['id'].Value) lacks required closed field: $field"
            }
        }
        if ($entry.Value -notmatch '(?m)^- \*\*Status:\*\* Closed$') {
            Add-ValidationError "$($entry.Groups['id'].Value) in Closed does not have Closed status."
        }
    }
}

$allFutureIds = @($futureEntries + $roadmappedEntries + $closedEntries | ForEach-Object { $_.Groups['id'].Value })
foreach ($duplicate in $allFutureIds | Group-Object | Where-Object { $_.Count -gt 1 }) {
    Add-ValidationError "Duplicate Future Revision ID across lifecycle sections: $($duplicate.Name)"
}

foreach ($requiredText in @(
    '### FR-011 - GM/AI Context Assembly, Mandatory Read Discipline, and Gameplay Turn Persistence',
    'A gameplay turn that changes canonical persistent state is not complete until the required persistence write has succeeded and been validated.',
    'Saving is automatic during normal gameplay, not optional housekeeping or a player command.',
    'Conversation context is a convenience layer and never proves that required Canon was loaded.',
    'During ordinary gameplay, the player can play continuously without issuing a manual save command while every canonical persistent change commits and later reloads correctly.',
    '**Status:** Roadmapped'
)) {
    if ($future -notmatch [regex]::Escape($requiredText)) {
        Add-ValidationError "FR-011 planning amendment lacks required invariant: $requiredText"
    }
}

$codexRequired = @(
    'docs/gm-living-codex/README.md',
    'docs/gm-living-codex/GM_LIVING_CODEX.md',
    'docs/gm-living-codex/SPECIES_REGISTRY.md',
    'docs/gm-living-codex/PERSISTENCE_MODEL.md',
    'docs/gm-living-codex/REPRODUCTIVE_COMPATIBILITY.md',
    'docs/gm-living-codex/LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md',
    'docs/skills/LEVEL_ZERO_INSTINCT.md',
    'templates/LIVING_CODEX_SPECIES_TEMPLATE.md',
    'templates/LIVING_CODEX_INHERITANCE_PROFILE_TEMPLATE.md'
)
foreach ($relative in $codexRequired) {
    if (-not (Test-Path -LiteralPath (Join-Path $rootPath $relative))) {
        Add-ValidationError "Missing Living Codex foundation file: $relative"
    }
}

$codexPlanPath = Join-Path $rootPath 'docs/gm-living-codex/GM_LIVING_CODEX.md'
if (Test-Path -LiteralPath $codexPlanPath) {
    $codexPlan = Get-Content -Raw -LiteralPath $codexPlanPath
    foreach ($step in 1..13) {
        if ($codexPlan -notmatch "(?m)^### Step $step (?:-|—) ") {
            Add-ValidationError "Living Codex implementation plan lacks Step $step."
        }
    }
}

$codexPersistencePath = Join-Path $rootPath 'docs/gm-living-codex/PERSISTENCE_MODEL.md'
if (Test-Path -LiteralPath $codexPersistencePath) {
    $codexPersistence = Get-Content -Raw -LiteralPath $codexPersistencePath
    foreach ($requiredText in @(
        'eternal_cycle_living_codex.sqlite',
        'separate from every campaign database',
        'reopen the candidate read-only',
        'replace the canonical Google Drive file',
        'lineage_templates',
        'inheritance_profile_sources',
        'inheritance_outcomes',
        'inherited_instincts',
        'ancestral_echo_definitions',
        'profile_status <> ''complete''',
        'outcome_weight >= 0'
    )) {
        if ($codexPersistence -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Living Codex persistence model lacks required invariant: $requiredText"
        }
    }
}

$lineagePath = Join-Path $rootPath 'docs/gm-living-codex/LINEAGE_AND_EVOLUTIONARY_INHERITANCE.md'
if (Test-Path -LiteralPath $lineagePath) {
    $lineage = Get-Content -Raw -LiteralPath $lineagePath
    foreach ($requiredText in @(
        'Compatibility percentages never become inheritance weights.',
        'An absent profile means **Not Yet Defined**.',
        'There is no universal halving rule.',
        'A **Born-Evolved** individual',
        'An **Ancestral Echo**',
        'campaign Save Transaction'
    )) {
        if ($lineage -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Lineage and Evolutionary Inheritance lacks required invariant: $requiredText"
        }
    }
}

$levelZeroPath = Join-Path $rootPath 'docs/skills/LEVEL_ZERO_INSTINCT.md'
if (Test-Path -LiteralPath $levelZeroPath) {
    $levelZero = Get-Content -Raw -LiteralPath $levelZeroPath
    foreach ($requiredText in @('existing Skill system', 'not reached ordinary Level 1 capability', 'No separate Skill Seed subsystem.', 'does not grant')) {
        if ($levelZero -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Level 0 Instinct lacks required invariant: $requiredText"
        }
    }
}

$simulationArchitecturePath = Join-Path $rootPath 'docs/core/SIMULATION_ARCHITECTURE_AND_PERSPECTIVE.md'
if (-not (Test-Path -LiteralPath $simulationArchitecturePath)) {
    Add-ValidationError 'Missing Simulation Architecture and Perspective Model.'
}
else {
    $simulationArchitecture = Get-Content -Raw -LiteralPath $simulationArchitecturePath
    foreach ($requiredText in @(
        'Layer 1 - Immutable Rules',
        'Layer 2 - GM Simulation Engine',
        'Layer 3 - Player RPG Interface',
        'Entity, Controller, and Perspective',
        'The player is an external Controller.',
        'Knowledge and belief belong to a knowing Entity or Perspective',
        'does not define autonomy levels or implement the proposed Autonomous Registry',
        'does not implement those records, values, or inference mechanics',
        'Player-facing narration cannot overwrite objective state.'
    )) {
        if ($simulationArchitecture -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Simulation Architecture lacks required invariant: $requiredText"
        }
    }
}

$canonicalOwnershipPath = Join-Path $rootPath 'docs/persistence/CANONICAL_DATA_OWNERSHIP.md'
if (-not (Test-Path -LiteralPath $canonicalOwnershipPath)) {
    Add-ValidationError 'Missing Canonical Data Ownership contract.'
}
else {
    $canonicalOwnership = Get-Content -Raw -LiteralPath $canonicalOwnershipPath
    foreach ($requiredText in @(
        'One mutable canonical fact has exactly one authoritative logical owner.',
        'Non-Autonomous Entity Identity Anchor',
        'The player is external to the fiction',
        'Persistent independently operating entities and systems belong to the [Autonomous Registry](AUTONOMOUS_REGISTRY.md).',
        'Current Location Invariant',
        'Relationship Invariant',
        'Species and Individual Boundary',
        'Historical and Derived Repetition',
        'This repository currently defines no executable campaign schema or populated save.',
        'FR-011:',
        'FR-014:'
    )) {
        if ($canonicalOwnership -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Canonical Data Ownership lacks required invariant: $requiredText"
        }
    }
}

$autonomousRegistryPath = Join-Path $rootPath 'docs/persistence/AUTONOMOUS_REGISTRY.md'
if (-not (Test-Path -LiteralPath $autonomousRegistryPath)) {
    Add-ValidationError 'Missing Autonomous Registry contract.'
}
else {
    $autonomousRegistry = Get-Content -Raw -LiteralPath $autonomousRegistryPath
    foreach ($requiredText in @(
        'One continuing subject has one authoritative identity anchor.',
        'An **Autonomous Model**',
        'An **Autonomous Individual**',
        'Directed',
        'Independent Agent',
        'Creator, Controller, and assignment issuer are separate relations.',
        'Backup-Restored',
        'each continuing descendant receives a distinct Autonomous ID;',
        'An Individual has conceptual quantity one.',
        'Last Confirmed State and Uncertainty',
        'This subsystem does not implement FR-015 reincarnation memory.',
        'This reusable repository performs no populated campaign migration.',
        'FR-011 Compatibility'
    )) {
        if ($autonomousRegistry -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Autonomous Registry lacks required invariant: $requiredText"
        }
    }
}

$lifeArchivePath = Join-Path $rootPath 'docs/persistence/LIFE_ARCHIVE.md'
if (-not (Test-Path -LiteralPath $lifeArchivePath)) {
    Add-ValidationError 'Missing Life Archive contract.'
}
else {
    $lifeArchive = Get-Content -Raw -LiteralPath $lifeArchivePath
    foreach ($requiredText in @(
        'Soul Overview',
        'Life Summary',
        'Full Life Detail',
        'The Life Archive is not the [Akashic Archive]',
        'Player-visible archive information is not Character Knowledge.',
        'An active Life record',
        'A completed Life record',
        'CREATE TABLE lives',
        'CREATE TABLE life_summaries',
        'CREATE TABLE life_archive_references',
        'FR-001:',
        'FR-005/FR-006/FR-007:',
        'FR-010:',
        'FR-011:',
        'FR-015:',
        'FR-016:'
    )) {
        if ($lifeArchive -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Life Archive lacks required invariant: $requiredText"
        }
    }
}

$longHorizonPath = Join-Path $rootPath 'docs/persistence/LONG_HORIZON_SUMMARIES.md'
if (-not (Test-Path -LiteralPath $longHorizonPath)) {
    Add-ValidationError 'Missing Long-Horizon Simulation Summary contract.'
}
else {
    $longHorizon = Get-Content -Raw -LiteralPath $longHorizonPath
    foreach ($requiredText in @(
        'Every summary has one stable **Historical Period ID**',
        'Important causal chains survive compression',
        'Elapsed time is not evidence.',
        '## Continuity Hooks',
        '## Time Skip Procedure',
        '## Reincarnation and Interlife',
        'An Age transition or World Reset does not erase prior Canon.',
        'A [Life Summary](LIFE_ARCHIVE.md#life-summary) and a Long-Horizon Summary remain distinct',
        '## Retrieval and Relevant History Index',
        'CREATE TABLE historical_periods',
        'CHECK (parent_period_id <> child_period_id)',
        'This repository migrates no populated campaign.',
        '### Lost Autonomous Unit',
        '### Filtered Transition',
        'FR-011 Context Assembly'
    )) {
        if ($longHorizon -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Long-Horizon Summaries lacks required invariant: $requiredText"
        }
    }
}

$longHorizonTemplatePath = Join-Path $rootPath 'templates/LONG_HORIZON_SUMMARY_TEMPLATE.md'
if (-not (Test-Path -LiteralPath $longHorizonTemplatePath)) {
    Add-ValidationError 'Missing Long-Horizon Summary template.'
}
else {
    $longHorizonTemplate = Get-Content -Raw -LiteralPath $longHorizonTemplatePath
    foreach ($requiredText in @(
        '**Historical Period ID:**',
        '## Causal Links',
        '## Surviving Continuities',
        '## Unresolved Threads and Unknowns',
        '## Derived Historical Transition',
        'Nested period references are acyclic.',
        'Player-facing content exposes no GM Secret'
    )) {
        if ($longHorizonTemplate -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Long-Horizon Summary template lacks required invariant: $requiredText"
        }
    }
}

$retainedDevelopmentPath = Join-Path $rootPath 'docs/progression/RETAINED_CROSS_LIFE_DEVELOPMENT.md'
if (-not (Test-Path -LiteralPath $retainedDevelopmentPath)) {
    Add-ValidationError 'Missing Retained Cross-Life Development contract.'
}
else {
    $retainedDevelopment = Get-Content -Raw -LiteralPath $retainedDevelopmentPath
    foreach ($requiredText in @(
        'Past development creates familiarity and retained developmental advantage, not automatic present capability.',
        'Embodiment Relevance',
        'Historical Territory',
        'Familiarity Unlock',
        'Translation Bridge',
        'effective = cap * raw / (cap + raw)',
        'Zero is legitimate and does not erase familiarity.',
        'Familiarity Unlock does not:',
        'Mana is not universal compatibility.',
        'CREATE TABLE retained_development_sources',
        'CREATE TABLE retained_relevance_assessments',
        'CREATE TABLE retained_effective_profiles',
        'Retained familiarity does not imply autobiographical memory'
    )) {
        if ($retainedDevelopment -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Retained Cross-Life Development lacks required invariant: $requiredText"
        }
    }
}

$skillConsolidationPath = Join-Path $rootPath 'docs/skills/SKILL_CONSOLIDATION_AND_SCOPE.md'
if (-not (Test-Path -LiteralPath $skillConsolidationPath)) {
    Add-ValidationError 'Missing Skill Consolidation and Historical Scope contract.'
}
else {
    $skillConsolidation = Get-Content -Raw -LiteralPath $skillConsolidationPath
    foreach ($requiredText in @(
        'Skills should consolidate regularly when accumulated play establishes that several records represent substantially the same underlying capability.',
        'Are these records meaningfully developing the same capability?',
        'Two Level 10 predecessors do not automatically create Level 20.',
        'Skill Consolidation is not Skill Fusion.',
        'Established Application',
        'Reasonable Extension',
        'Related but Distinct Capability',
        'Unsupported Interpretation',
        'Why can this Skill do this?',
        'Boundary Cutting',
        'Life 1 developed `Heat Resistance`',
        'This repository performs no populated campaign migration.',
        '### A. Clear Redundant Merge',
        '### B. Meaningful Distinction',
        '### C. Excessively Broad Merge',
        '### D. Conceptual-Name Abuse',
        '### E. Earned Extension',
        '### F. Cross-Life Provenance',
        '### G. Repeated Injury'
    )) {
        if ($skillConsolidation -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Skill Consolidation lacks required invariant: $requiredText"
        }
    }
}

$compatibilityPath = Join-Path $rootPath 'docs/gm-living-codex/REPRODUCTIVE_COMPATIBILITY.md'

$phase12ClarificationContracts = @(
    @{
        Path = 'docs/soul/REINCARNATION.md'
        Name = 'FR-002 Reincarnation Candidate Selection'
        Required = @('#### Candidate Adjudication', 'A wish informs adjudication but does not command it.', 'predictable candidate-weighting exploit')
    },
    @{
        Path = 'docs/gm/REINCARNATION_GENERATION.md'
        Name = 'FR-002 GM Candidate Adjudication'
        Required = @('## Contextual Candidate Adjudication', 'bounded judgement rather than a mandatory weight equation', 'No reusable formula maps a chosen death circumstance to a guaranteed species or destination.')
    },
    @{
        Path = 'docs/soul/SOUL_DEPTH.md'
        Name = 'FR-003 Soul Depth Visibility'
        Required = @('## Hybrid Information Visibility', 'Soul Depth remains qualitative.', 'Player Knowledge and Character Knowledge remain separate.', 'Soul Depth: 47/100')
    },
    @{
        Path = 'docs/soul-weapons/WEAPON_EVOLUTION.md'
        Name = 'FR-008 Soul Weapon Equipment Baseline'
        Required = @('## Exceptional Capability and the Equipment Baseline', 'may substantially outperform ordinary', 'Soul Weapon status alone does not establish present superiority.', 'cannot ordinarily be manufactured or issued across an army.')
    },
    @{
        Path = 'docs/world-engine/GATES_AND_WORLD_CONTACT.md'
        Name = 'FR-009 World Access Distinctions'
        Required = @('## Knowledge, Contact, Travel, and Reincarnation', 'World Knowledge', 'World Contact', 'Travel Route', 'Reincarnation Possibility', 'Prior contact is not permanent access.', 'unrestricted Reincarnation destination menu')
    }
)

foreach ($contract in $phase12ClarificationContracts) {
    $contractPath = Join-Path $rootPath $contract.Path
    if (-not (Test-Path -LiteralPath $contractPath)) {
        Add-ValidationError "Missing $($contract.Name) contract."
        continue
    }
    $contractContent = Get-Content -Raw -LiteralPath $contractPath
    foreach ($requiredText in $contract.Required) {
        if ($contractContent -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "$($contract.Name) lacks required invariant: $requiredText"
        }
    }
}

if (Test-Path -LiteralPath $compatibilityPath) {
    $compatibility = Get-Content -Raw -LiteralPath $compatibilityPath
    foreach ($requiredText in @(
        'An absent relationship row means **Not Yet Defined**.',
        'natural_compatibility_percent > 0',
        'natural_compatibility_percent <= 100',
        'assisted_compatibility_percent > 0',
        'offspring_viability_percent > 0',
        'offspring_fertility_percent > 0',
        'UNIQUE (source_species_id, partner_species_id)'
    )) {
        if ($compatibility -notmatch [regex]::Escape($requiredText)) {
            Add-ValidationError "Reproductive Compatibility lacks required invariant: $requiredText"
        }
    }
}

$committedDatabaseArtifacts = @(Get-ChildItem -LiteralPath $rootPath -Recurse -File |
    Where-Object { $_.FullName -notlike '*\.git\*' -and $_.Extension -in @('.sqlite', '.sqlite3', '.db') })
foreach ($artifact in $committedDatabaseArtifacts) {
    Add-ValidationError "Populated database artifact is forbidden in the rules repository: $(Get-RepositoryPath $artifact.FullName)"
}

$forbiddenDirectories = @('campaign', 'campaigns', 'saves', 'world-state', 'player-data')
foreach ($directory in Get-ChildItem -LiteralPath $rootPath -Directory) {
    if ($forbiddenDirectories -contains $directory.Name.ToLowerInvariant()) {
        Add-ValidationError "Forbidden campaign-data directory exists: $($directory.Name)/"
    }
}

if ($errors.Count -gt 0) {
    Write-Output "Repository validation: FAIL ($($errors.Count) issue(s))"
    $errors | Sort-Object -Unique | ForEach-Object { Write-Output "- $_" }
    exit 1
}

Write-Output 'Repository validation: PASS'
Write-Output "Markdown files: $($markdownFiles.Count)"
Write-Output "Relative links checked: $relativeLinks"
Write-Output "Anchors checked: $anchorLinks"
Write-Output "Canonical documents indexed: $($docsExpected.Count)"
Write-Output "Documentation family indexes: $($familyIndexes.Count)"
Write-Output "Templates indexed: $($templateFiles.Count)"
Write-Output "Agent roles indexed: $($agentFiles.Count)"
Write-Output "Canonical terms checked: $($termHeadings.Count)"
Write-Output "Roadmap tasks checked: $($roadmapStatuses.Count)"
Write-Output "Future Revision entries checked: $($futureEntries.Count + $roadmappedEntries.Count + $closedEntries.Count)"
Write-Output 'Living Codex foundation: Steps 1-13 invariants checked'
Write-Output 'Simulation Architecture: three layers and identity/knowledge boundaries checked'
Write-Output 'Canonical Data Ownership: owner map and anti-duplication boundaries checked'
Write-Output 'Autonomous Registry: identity, autonomy, memory, group, uncertainty, and migration boundaries checked'
Write-Output 'Life Archive: identity, summary, ownership, knowledge, migration, and retrieval boundaries checked'
Write-Output 'Retained Development: stacking, relevance, Skill transfer, territory, prerequisites, and memory boundaries checked'
Write-Output 'Skill Consolidation: lineage, Development reconciliation, scope, cross-Life provenance, and canonical cases checked'
Write-Output 'Phase 12 clarifications: Reincarnation selection, Soul Depth visibility, Soul Weapon baseline, and world-access distinctions checked'
Write-Output 'Blocking unresolved questions: 0'
Write-Output 'Orphaned Markdown documents: 0 (root README is the entry point)'
Write-Output 'Forbidden campaign-data directories: 0'
