[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ArchivePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$archiveFullPath = (Resolve-Path -LiteralPath $ArchivePath).Path
$requiredEntries = @(
    'AGENTS.md',
    'README.md',
    'VERSION',
    'LICENSE',
    'NOTICE',
    'DISTRIBUTION.json',
    'design/ROADMAP.md',
    'docs/README.md',
    'docs/rules/rule-source-manifest.json',
    'templates/README.md',
    'tools/validate_repository.ps1',
    'tools/build_distribution.ps1',
    'examples/tooling/managed-data/mcp-dotnet-tsql/src/EternalCycle.Persistence.Mcp/EternalCycle.Persistence.Mcp.csproj',
    'examples/tooling/managed-data/mcp-dotnet-tsql/tests/EternalCycle.Persistence.Mcp.Tests/EternalCycle.Persistence.Mcp.Tests.csproj'
)
$forbiddenSegments = @(
    '.git', '.idea', '.vs', '.vscode', '.cache', '.mypy_cache', '.pytest_cache',
    '__pycache__', 'bin', 'node_modules', 'obj', 'TestResults'
)
$errors = [Collections.Generic.List[string]]::new()

$stream = [IO.File]::OpenRead($archiveFullPath)
try {
    $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Read, $false)
    try {
        $names = @($archive.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
        $nameSet = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($name in $names) {
            $null = $nameSet.Add($name)
        }
        foreach ($required in $requiredEntries) {
            if (-not $nameSet.Contains($required)) {
                $errors.Add("Missing required entry: $required")
            }
        }

        foreach ($name in $names) {
            $segments = $name -split '/'
            if ($segments | Where-Object { $forbiddenSegments -contains $_ }) {
                $errors.Add("Forbidden generated or local path: $name")
            }
            if ($name -like '*.log' -or
                $name -like '*.tmp' -or
                $name -like '*.temp' -or
                $name -like '*.user' -or
                $name -eq '.env' -or
                $name -like '.env.*' -or
                $name -like 'Eternal Cycle*.zip') {
                $errors.Add("Forbidden transient or sensitive file: $name")
            }
        }

        if ($names.Count -lt 100) {
            $errors.Add("Archive contains only $($names.Count) entries; a full review snapshot was expected.")
        }
    }
    finally {
        $archive.Dispose()
    }
}
finally {
    $stream.Dispose()
}

if ($errors.Count -gt 0) {
    Write-Output "Distribution validation: FAIL ($($errors.Count) issue(s))"
    $errors | Sort-Object -Unique | ForEach-Object { Write-Output "- $_" }
    exit 1
}

$hash = (Get-FileHash -LiteralPath $archiveFullPath -Algorithm SHA256).Hash
Write-Output 'Distribution validation: PASS'
Write-Output "Entries: $($names.Count)"
Write-Output 'Forbidden bin/obj/cache/temp paths: 0'
Write-Output "SHA-256: $hash"
