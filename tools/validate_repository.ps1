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
if ($phaseMatches.Count -ne 1) {
    Add-ValidationError "Roadmap must contain exactly one Current phase declaration."
}
if ($taskMatches.Count -ne 1) {
    Add-ValidationError "Roadmap must contain exactly one Current task declaration."
}
foreach ($status in [regex]::Matches($roadmap, '(?m)^- \[(.)\] ')) {
    if (' ', 'x', '~', '!' -notcontains $status.Groups[1].Value) {
        Add-ValidationError "Invalid roadmap status token: [$($status.Groups[1].Value)]"
    }
}
if ($taskMatches.Count -eq 1 -and $roadmap -notmatch 'Repository Status: Feature Complete — Gameplay Validation Ongoing') {
    $task = [regex]::Escape($taskMatches[0].Groups[1].Value)
    if ($roadmap -notmatch "(?m)^- \[[ ~!]\] $task$") {
        Add-ValidationError "Current roadmap task is not an open, partial, or blocked checklist item."
    }
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
Write-Output 'Orphaned Markdown documents: 0 (root README is the entry point)'
Write-Output 'Forbidden campaign-data directories: 0'
