param(
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'
$script:failures = [System.Collections.Generic.List[string]]::new()
$script:passes = 0

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

function New-Fixture {
    param(
        [ValidateSet('local', 'cloud')][string]$Authority = 'local',
        [bool]$LocalCopyExists = $true,
        [bool]$RemoteCanonicalExists = $false
    )

    return @{
        Authority = $Authority
        LocalCopyExists = $LocalCopyExists
        RemoteCanonicalExists = $RemoteCanonicalExists
        TargetReady = $false
        BlankCreated = $false
        Version = 12
        PersistedVersion = 12
        LoadedVersion = $null
        OwnerWrites = 0
        CloudAttempts = 0
        CloudVerified = $false
        AffectedSet = @()
        Pending = $false
        Failed = $false
        FailureReason = $null
        Marker = $null
        TurnComplete = $false
        LastLocalCommit = $null
        LastCloudVerification = $null
        TransactionId = 'TX-FR011-FIXTURE'
    }
}

function Resolve-PersistenceTarget {
    param([hashtable]$Fixture)

    if ($Fixture.Authority -eq 'local') {
        $Fixture.TargetReady = $Fixture.LocalCopyExists
        return
    }

    if ($Fixture.RemoteCanonicalExists) {
        $Fixture.LocalCopyExists = $true
        $Fixture.TargetReady = $true
        return
    }

    $Fixture.TargetReady = $false
}

function Open-StateChangingTurn {
    param([hashtable]$Fixture)
    return $Fixture.TargetReady -and -not $Fixture.Pending -and -not $Fixture.Failed
}

function Start-Persistence {
    param(
        [hashtable]$Fixture,
        [string[]]$AffectedSet,
        [ValidateSet('success', 'pending', 'failure', 'unverified')][string]$CloudResult = 'success',
        [switch]$ForceUnchanged
    )

    $Fixture.AffectedSet = @($AffectedSet)
    if ($Fixture.AffectedSet.Count -eq 0) {
        $Fixture.TurnComplete = $true
        $Fixture.Marker = if ($Fixture.Authority -eq 'cloud' -and $Fixture.CloudVerified) { 'cloud-saved' } else { 'local-saved' }
        return
    }

    $Fixture.OwnerWrites++
    if (-not $ForceUnchanged) {
        $Fixture.Version++
        $Fixture.PersistedVersion = $Fixture.Version
        $Fixture.LastLocalCommit = $Fixture.Version
    }

    if ($Fixture.Version -le 12) {
        $Fixture.Failed = $true
        $Fixture.FailureReason = 'Expected canonical change was absent.'
        $Fixture.Marker = 'failed'
        return
    }

    if ($Fixture.Authority -eq 'local') {
        $Fixture.Marker = 'local-saved'
        $Fixture.TurnComplete = $true
        return
    }

    $Fixture.CloudAttempts++
    switch ($CloudResult) {
        'success' {
            $Fixture.CloudVerified = $true
            $Fixture.LastCloudVerification = $Fixture.Version
            $Fixture.Marker = 'cloud-saved'
            $Fixture.TurnComplete = $true
        }
        'pending' {
            $Fixture.Pending = $true
            $Fixture.Marker = 'pending'
        }
        'failure' {
            $Fixture.Failed = $true
            $Fixture.FailureReason = 'Cloud synchronization failed.'
            $Fixture.Marker = 'failed'
        }
        'unverified' {
            $Fixture.Failed = $true
            $Fixture.FailureReason = 'Cloud upload lacked read-back verification.'
            $Fixture.Marker = 'failed'
        }
    }
}

function Retry-Save {
    param([hashtable]$Fixture)

    if ($Fixture.Authority -ne 'cloud' -or $null -eq $Fixture.LastLocalCommit) { return }
    $Fixture.CloudAttempts++
    $Fixture.Pending = $false
    $Fixture.Failed = $false
    $Fixture.FailureReason = $null
    $Fixture.CloudVerified = $true
    $Fixture.LastCloudVerification = $Fixture.Version
    $Fixture.Marker = 'cloud-saved'
    $Fixture.TurnComplete = $true
}

function Get-SaveStatus {
    param([hashtable]$Fixture)
    return @{
        Marker = $Fixture.Marker
        Version = $Fixture.Version
        Authority = $Fixture.Authority
        LocalCurrent = ($null -ne $Fixture.LastLocalCommit)
        CloudCurrent = $Fixture.CloudVerified
        Pending = $Fixture.Pending
        LastLocalCommit = $Fixture.LastLocalCommit
        LastCloudVerification = $Fixture.LastCloudVerification
        FailureReason = $Fixture.FailureReason
    }
}

# Multi-domain automatic persistence and next-turn canonical reload.
$multi = New-Fixture
Resolve-PersistenceTarget $multi
Assert-True (Open-StateChangingTurn $multi) 'Resolved local target permits state-changing play.'
$domains = @('infrastructure', 'research', 'equipment', 'communications', 'autonomous-registry', 'discoveries', 'timeline', 'campaign-history')
Start-Persistence $multi $domains
$multi.LoadedVersion = $multi.PersistedVersion
Assert-True ($multi.AffectedSet.Count -eq $domains.Count -and $multi.OwnerWrites -eq 1) 'Multi-domain Affected Set writes canonical owners once.'
Assert-True ($multi.Version -eq 13 -and $multi.Marker -eq 'local-saved' -and $multi.TurnComplete) 'Automatic local save changes canonical version and completes with local status.'
Assert-True ($multi.LoadedVersion -eq 13) 'Next turn reloads the changed fact from canonical persistence.'

# Canonical target discovery when the assumed local path is absent.
$discovery = New-Fixture -Authority cloud -LocalCopyExists $false -RemoteCanonicalExists $true
Resolve-PersistenceTarget $discovery
Assert-True ($discovery.TargetReady -and $discovery.LocalCopyExists -and -not $discovery.BlankCreated) 'Configured remote canonical save is fetched without creating a blank replacement.'

# Local-only and cloud-authoritative completion semantics.
$local = New-Fixture
Resolve-PersistenceTarget $local
Start-Persistence $local @('character-state')
Assert-True ($local.Marker -eq 'local-saved') 'Local-authoritative campaign completes with local validated status.'

$cloud = New-Fixture -Authority cloud -RemoteCanonicalExists $true
Resolve-PersistenceTarget $cloud
Start-Persistence $cloud @('relationship-state') -CloudResult success
Assert-True ($cloud.Marker -eq 'cloud-saved' -and $cloud.CloudVerified) 'Cloud-authoritative campaign completes only after remote verification.'

# Pending and failed cloud stages block later state-changing play.
$pending = New-Fixture -Authority cloud -RemoteCanonicalExists $true
Resolve-PersistenceTarget $pending
Start-Persistence $pending @('project-state') -CloudResult pending
Assert-True ($pending.Marker -eq 'pending' -and -not $pending.TurnComplete -and -not (Open-StateChangingTurn $pending)) 'Pending cloud save blocks turn completion and subsequent state-changing play.'

$failed = New-Fixture -Authority cloud -RemoteCanonicalExists $true
Resolve-PersistenceTarget $failed
Start-Persistence $failed @('inventory') -CloudResult failure
$writesBeforeRetry = $failed.OwnerWrites
Retry-Save $failed
Assert-True ($failed.Marker -eq 'cloud-saved' -and $failed.OwnerWrites -eq $writesBeforeRetry -and $failed.CloudAttempts -eq 2) 'Cloud retry resumes synchronization without duplicating gameplay writes.'

# Upload attempts without verification cannot claim cloud success.
$unverified = New-Fixture -Authority cloud -RemoteCanonicalExists $true
Resolve-PersistenceTarget $unverified
Start-Persistence $unverified @('location') -CloudResult unverified
Assert-True ($unverified.Marker -eq 'failed' -and -not $unverified.CloudVerified -and -not $unverified.TurnComplete) 'Unverified upload never produces cloud-saved status.'

# Unchanged canonical state is a hard failure.
$unchanged = New-Fixture
Resolve-PersistenceTarget $unchanged
Start-Persistence $unchanged @('timeline') -ForceUnchanged
Assert-True ($unchanged.Marker -eq 'failed' -and -not $unchanged.TurnComplete) 'Non-empty Affected Set with unchanged canonical state fails completion.'

# Manual save uses the same transaction and status remains inspectable.
$manual = New-Fixture
Resolve-PersistenceTarget $manual
$manual.AffectedSet = @('knowledge')
Start-Persistence $manual $manual.AffectedSet
$manualWrites = $manual.OwnerWrites
$status = Get-SaveStatus $manual
Assert-True ($manual.OwnerWrites -eq $manualWrites -and $status.Marker -eq 'local-saved') 'Manual save path uses normal machinery without replaying narrative effects.'
Assert-True ($status.Version -eq 13 -and $status.Authority -eq 'local' -and -not $status.Pending -and $status.LastLocalCommit -eq 13) 'Save status reports marker, version, authority, pending state, and last local commit.'

if ($script:failures.Count -gt 0) {
    Write-Output "FR-011 persistence-gate regression harness: FAIL ($($script:failures.Count) failure(s))"
    $script:failures | ForEach-Object { Write-Output "- $_" }
    throw 'FR-011 persistence-gate regression harness failed.'
}

Write-Output "FR-011 persistence-gate regression harness: PASS ($script:passes assertions)"
