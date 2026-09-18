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

$reference = 'examples/tooling/managed-data/mcp-dotnet-tsql'
$source = "$reference/src/EternalCycle.Persistence.Mcp"
$testsRoot = "$reference/tests/EternalCycle.Persistence.Mcp.Tests"
$configuration = Read-RepoFile "$source/ManagedConfiguration.cs"
$diagnostics = Read-RepoFile "$source/ManagedDiagnostics.cs"
$readiness = Read-RepoFile "$source/ManagedReadiness.cs"
$operations = Read-RepoFile "$source/ManagedOperations.cs"
$administration = Read-RepoFile "$source/ManagedAdministration.cs"
$program = Read-RepoFile "$source/Program.cs"
$registry = Read-RepoFile "$source/ErrorRegistry.cs"
$dump = Read-RepoFile "$source/ErrorDump.cs"
$boundary = Read-RepoFile "$source/McpToolExceptionBoundary.cs"
$schemaTemplate = Read-RepoFile "$source/Schema/007_durable_managed_operations.template.sql"
$project = Read-RepoFile "$source/EternalCycle.Persistence.Mcp.csproj"
$unitTests = Read-RepoFile "$testsRoot/ManagedControlPlaneRepairTests.cs"
$sqlTests = Read-RepoFile "$testsRoot/PreMigrationControlPlaneIntegrationTests.cs"
$support = Read-RepoFile 'docs/support/ERRORS_AND_PORTABLE_DIAGNOSTICS.md'
$acceptance = Read-RepoFile "$reference/MCP_ONLY_ACCEPTANCE_TEST.md"
$decisions = Read-RepoFile 'design/DECISIONS.md'
$terms = Read-RepoFile 'design/TERMINOLOGY.md'
$audit = Read-RepoFile 'design/audits/FR_021_PRE_MIGRATION_CONTROL_PLANE_REPAIR_AUDIT.md'
$distribution = Read-RepoFile 'DISTRIBUTION.json'
$localDistribution = Read-RepoFile "$source/DISTRIBUTION.json"
$notice = Read-RepoFile 'NOTICE'

Assert-Requirement 1 ($configuration -match 'ec_get_configuration_requirements' -and $configuration -match 'requiring a database') 'Configuration discovery is a database-independent MCP capability.'
Assert-Requirement 2 ($configuration -match 'EnvironmentVariableName' -and $configuration -match 'RestartRequired' -and $configuration -match 'Audience') 'Configuration discovery includes exact environment names, restart, and audience metadata.'
Assert-Requirement 3 ($configuration -match 'definition.Sensitive \? null' -and $unitTests -match 'ConfigurationDiscoveryRequiresNoDatabaseAndNeverReturnsSensitiveValues') 'Sensitive configuration values are structurally excluded and tested.'
Assert-Requirement 4 ($configuration -match 'DisableAutomaticFallback' -and $configuration -match 'FallbackLogFile') 'Automatic fallback exposes explicit advanced override and disable controls.'

Assert-Requirement 5 ($program -match 'SanitizedDiagnosticFallbackBootstrap.Establish' -and $program.IndexOf('SanitizedDiagnosticFallbackBootstrap.Establish') -lt $program.IndexOf('AddOptions<SqlServerPersistenceOptions>')) 'Diagnostic fallback is established before database-dependent services.'
Assert-Requirement 6 ($diagnostics -match 'managed-diagnostics.jsonl' -and $diagnostics -match 'DisableAutomaticFallback') 'The fallback has a zero-configuration protected default and explicit disable behavior.'
Assert-Requirement 7 ($readiness -match 'MIGRATION_REQUIRED' -and $readiness -match 'ConfigurationRequired') 'Readiness distinguishes schema migration and host configuration recovery.'
Assert-Requirement 8 ($operations -match 'PreMigrationBlockAsync' -and $operations -match 'ManagedReadinessState.MigrationRequired') 'Managed-operation tools return structured pre-migration recovery.'
Assert-Requirement 9 ($administration -match 'configuration\?\.GetReport' -and $administration -match 'UserApproved') 'Administrative setup checks configuration and natural informed approval.'
Assert-Requirement 10 ($schemaTemplate -match '\{\{schema\}\}' -and $schemaTemplate -match '\{\{schema_name\}\}' -and $schemaTemplate -notmatch '\{\{DOMAIN_SCHEMA\}\}') 'Migration 007 uses only supported schema renderer tokens.'

Assert-Requirement 11 ($registry -match 'ec_list_error_codes' -and $registry -match 'ec_get_error_code') 'Static error listing and lookup tools are registered.'
Assert-Requirement 12 ($registry -match 'Found' -and $registry -match 'not registered' -and $unitTests -match 'ErrorRegistryListLookupAndUnknownLookupRequireNoDatabase') 'Unknown error codes return a safe non-throwing lookup result.'
Assert-Requirement 13 ($boundary -match 'INTERNAL_ERROR' -and $boundary -match 'EmergencyErrorText.Create') 'The outer tool boundary has stable unexpected-error and emergency fallbacks.'
Assert-Requirement 14 ($program -match 'AddCallToolFilter' -and $program -match 'IMcpToolInvocationGuard') 'The fail-safe boundary protects the actual MCP call path.'
Assert-Requirement 15 ($boundary -match 'VerboseErrors' -and $boundary -match 'DiagnosticRedactor.Redact') 'Verbose diagnostics remain opt-in and redacted.'

Assert-Requirement 16 ($dump -match 'FormatVersion = 1' -and $dump -match 'MaximumEvidenceRecords = 10' -and $dump -match 'MaximumTextCharacters = 24000') 'Error Dump v1 is explicitly bounded.'
Assert-Requirement 17 ($dump -match '"Partial"' -and $dump -match 'ErrorDumpSourceStatus' -and $unitTests -match 'ErrorDumpIsPortableBoundedSanitizedAndPartialWithoutDatabase') 'Error Dump supports tested partial evidence with per-source availability.'
Assert-Requirement 18 ($dump -match 'BuildText' -and $unitTests -match 'Eternal Cycle Error Dump') 'Error Dump has first-class copy/paste text output.'
Assert-Requirement 19 ($dump -notmatch 'HttpClient|HttpRequest|Upload|SubmitAsync' -and $unitTests -match 'ErrorDumpHasNoSupportBundleOrNetworkDependency') 'Error Dump performs no support or network submission.'
Assert-Requirement 20 ($support -match 'Error Dump and Support Bundle' -and $support -match 'External submission requires explicit consent') 'Documentation keeps dump, optional bundle, and submission as separate consent boundaries.'

$sourceText = (Get-ChildItem -LiteralPath (Join-Path $root $source) -Filter '*.cs' -File | ForEach-Object { Get-Content -Raw -LiteralPath $_.FullName }) -join "`n"
$thrownCodes = [regex]::Matches($sourceText, 'ManagedServiceException\s*\(\s*"([A-Z0-9_]+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
$registeredCodes = [regex]::Matches($registry, 'Def\("([A-Z0-9_]+)"') | ForEach-Object { $_.Groups[1].Value }
$missingCodes = @($thrownCodes | Where-Object { $_ -notin $registeredCodes })
$duplicateCodes = @($registeredCodes | Group-Object | Where-Object Count -gt 1)
Assert-Requirement 21 ($missingCodes.Count -eq 0) "Every literal ManagedServiceException code is registered; missing: $($missingCodes -join ', ')."
Assert-Requirement 22 ($duplicateCodes.Count -eq 0) 'The static error registry contains no duplicate codes.'

Assert-Requirement 23 ($sqlTests -match 'ETERNAL_CYCLE_RUN_SQL_INTEGRATION' -and $sqlTests -match '001_initial.template.sql' -and $sqlTests -match '006_rule_source_compatibility.template.sql') 'A genuine opt-in pre-007 SQL fixture starts from migrations 001 through 006.'
Assert-Requirement 24 ($sqlTests -match 'ManagedReadinessState.MigrationRequired' -and $sqlTests -match '007_durable_managed_operations') 'The SQL fixture verifies migration-required recovery and migration 007 application.'
Assert-Requirement 25 ($sqlTests -match 'the-second-turn' -and $sqlTests -match 'PublishInitialRulesAsync') 'The SQL fixture preserves a campaign and reaches durable publication initiation.'
Assert-Requirement 26 ($acceptance -match '## Genuine Pre-007 Upgrade Regression' -and $acceptance -match 'ec_get_configuration_requirements' -and $acceptance -match 'ec_get_error_dump') 'External acceptance documents the exact pre-007 control-plane sequence.'

$licensePath = Join-Path $root 'LICENSE'
$localLicensePath = Join-Path $root "$source/LICENSE"
$noticePath = Join-Path $root 'NOTICE'
$localNoticePath = Join-Path $root "$source/NOTICE"
$licenseHash = if (Test-Path -LiteralPath $licensePath) { (Get-FileHash -LiteralPath $licensePath -Algorithm SHA256).Hash } else { '' }
$localLicenseHash = if (Test-Path -LiteralPath $localLicensePath) { (Get-FileHash -LiteralPath $localLicensePath -Algorithm SHA256).Hash } else { '' }
$noticeHash = if (Test-Path -LiteralPath $noticePath) { (Get-FileHash -LiteralPath $noticePath -Algorithm SHA256).Hash } else { '' }
$localNoticeHash = if (Test-Path -LiteralPath $localNoticePath) { (Get-FileHash -LiteralPath $localNoticePath -Algorithm SHA256).Hash } else { '' }
Assert-Requirement 27 ($licenseHash -eq 'C71D239DF91726FC519C6EB72D318EC65820627232B2F796219E87DCF35D0AB4' -and $licenseHash -eq $localLicenseHash) 'Root and standalone-package LICENSE files are identical standard Apache-2.0 text.'
Assert-Requirement 28 ($notice -match 'Copyright 2026 xmiq' -and $noticeHash -eq $localNoticeHash) 'Root and standalone-package NOTICE files preserve original authorship identically.'
Assert-Requirement 29 ($distribution -eq $localDistribution -and $distribution -match '"license": "Apache-2.0"' -and $distribution -match '"supportDestinationType": "IssueTracker"') 'Distribution metadata copies agree on license and support provenance.'
Assert-Requirement 30 ($project -match 'CopyToPublishDirectory="PreserveNewest"' -and $project -match 'Link="LICENSE"' -and $project -match 'Link="NOTICE"') 'Build and publish output package license, notice, and distribution metadata.'

Assert-Requirement 31 ($decisions -match 'D-1335' -and $decisions -match 'D-1342') 'Canonical decisions record the control-plane, diagnostics, consent, provenance, and licensing invariants.'
Assert-Requirement 32 ($terms -match '## Error Dump' -and $terms -match '## Support Bundle' -and $terms -match '## Distribution Identity') 'Canonical terminology defines portable diagnostic and provenance concepts.'
Assert-Requirement 33 ($audit -match 'genuine migrations-001-through-006 LocalDB upgrade' -and $audit -match 'LM Studio or Unsloth MCP interoperability') 'The audit distinguishes repository evidence from unproven external acceptance.'
Assert-Requirement 34 ($readiness -match 'PerUserApplicationData' -and $readiness -match 'OperatorConfiguredPath' -and $readiness -notmatch 'DiagnosticFallbackLocation\s*=>\s*diagnosticFallback\?\.ResolvedPath') 'Public readiness reports a safe fallback location class rather than a private filesystem path.'
Assert-Requirement 35 ($diagnostics -match 'EternalCycle:Diagnostics:FallbackLogFile' -and $diagnostics -notmatch 'configuration\["EternalCycle:Administration:SanitizedLogFile"\]') 'Diagnostic fallback configuration remains separate from optional general host logging.'

if ($failures.Count -gt 0) {
    Write-Output "FR-021 control-plane repair harness: FAIL ($($failures.Count) failure(s))"
    $failures | ForEach-Object { Write-Output "- $_" }
    throw 'FR-021 control-plane repair harness failed.'
}

Write-Output "FR-021 control-plane repair harness: PASS ($passes assertions)"
