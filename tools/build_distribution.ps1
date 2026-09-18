[CmdletBinding()]
param(
    [string]$Root,
    [string]$OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Root)) {
    $Root = Split-Path -Parent $PSScriptRoot
}

$rootPath = (Resolve-Path -LiteralPath $Root).Path.TrimEnd([IO.Path]::DirectorySeparatorChar)
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $rootPath 'Eternal Cycle.zip'
}
$outputFullPath = [IO.Path]::GetFullPath($OutputPath)
$outputDirectory = Split-Path -Parent $outputFullPath
if (-not [string]::IsNullOrWhiteSpace($outputDirectory)) {
    [IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
}

$excludedDirectories = @(
    '.git', '.idea', '.vs', '.vscode', '.cache', '.mypy_cache', '.pytest_cache',
    '__pycache__', 'bin', 'node_modules', 'obj', 'TestResults'
)
$excludedFiles = @('.DS_Store', 'Thumbs.db')

function Get-DistributionFiles {
    Get-ChildItem -LiteralPath $script:rootPath -Recurse -File -Force |
        Where-Object {
            $fullPath = $_.FullName
            if ($fullPath.Equals($script:outputFullPath, [StringComparison]::OrdinalIgnoreCase)) {
                return $false
            }

            $relative = $fullPath.Substring($script:rootPath.Length + 1)
            $segments = $relative -split '[\\/]'
            if ($segments | Where-Object { $script:excludedDirectories -contains $_ }) {
                return $false
            }

            if ($script:excludedFiles -contains $_.Name -or
                $_.Name -like '*.log' -or
                $_.Name -like '*.tmp' -or
                $_.Name -like '*.temp' -or
                $_.Name -like '*.user' -or
                $_.Name -like '*.suo' -or
                $_.Name -eq '.env' -or
                $_.Name -like '.env.*' -or
                $_.Name -like 'Eternal Cycle*.zip') {
                return $false
            }

            return $true
        } |
        Sort-Object { $_.FullName.Substring($script:rootPath.Length + 1).Replace('\', '/') }
}

$files = @(Get-DistributionFiles)
if ($files.Count -eq 0) {
    throw 'No repository files were selected for the distribution archive.'
}

$temporaryPath = "$outputFullPath.partial-$([Guid]::NewGuid().ToString('N'))"
try {
    $stream = [IO.File]::Open($temporaryPath, [IO.FileMode]::CreateNew, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    try {
        $archive = [IO.Compression.ZipArchive]::new($stream, [IO.Compression.ZipArchiveMode]::Create, $false)
        try {
            $fixedTimestamp = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)
            foreach ($file in $files) {
                $entryName = $file.FullName.Substring($rootPath.Length + 1).Replace('\', '/')
                $entry = $archive.CreateEntry($entryName, [IO.Compression.CompressionLevel]::Optimal)
                $entry.LastWriteTime = $fixedTimestamp
                $input = [IO.File]::Open($file.FullName, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
                try {
                    $output = $entry.Open()
                    try {
                        $input.CopyTo($output)
                    }
                    finally {
                        $output.Dispose()
                    }
                }
                finally {
                    $input.Dispose()
                }
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }

    [IO.File]::Move($temporaryPath, $outputFullPath, $true)
}
finally {
    if (Test-Path -LiteralPath $temporaryPath) {
        Remove-Item -LiteralPath $temporaryPath -Force
    }
}

$hash = (Get-FileHash -LiteralPath $outputFullPath -Algorithm SHA256).Hash
Write-Output "Distribution archive: $outputFullPath"
Write-Output "Files: $($files.Count)"
Write-Output "SHA-256: $hash"
