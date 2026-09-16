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
$readiness = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/ManagedReadiness.cs"
$administration = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/ManagedAdministration.cs"
$diagnostics = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/ServiceDiagnostics.cs"
$ruleTools = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/RuleContextTools.cs"
$source = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/GitRuleSourceProvider.cs"
$logging = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/SanitizedFileLogger.cs"
$program = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/Program.cs"
$schema001 = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/Schema/001_initial.template.sql"
$schema002 = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/Schema/002_rule_domain.template.sql"
$schema003 = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/Schema/003_campaign_directory.template.sql"
$schema004 = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/Schema/004_rule_source_configuration.template.sql"
$tests = Read-RepoFile "$reference/tests/EternalCycle.Persistence.Mcp.Tests/ManagedFirstRunTests.cs"
$readme = Read-RepoFile "$reference/README.md"
$acceptance = Read-RepoFile "$reference/MCP_ONLY_ACCEPTANCE_TEST.md"
$running = Read-RepoFile 'docs/persistence/RUNNING_ETERNAL_CYCLE.md'
$player = Read-RepoFile 'docs/gm/PLAYER_START_AND_RESUME.md'
$managed = Read-RepoFile 'docs/persistence/MANAGED_DATA_SERVICE.md'
$publication = Read-RepoFile 'docs/rules/MANAGED_RULE_PUBLICATION.md'
$publicationRuntime = Read-RepoFile "$reference/src/EternalCycle.Persistence.Mcp/ManagedRulePublication.cs"
$distribution = Read-RepoFile 'DISTRIBUTION.json'
$roadmap = Read-RepoFile 'design/ROADMAP.md'
$future = Read-RepoFile 'design/FUTURE_REVISIONS.md'
$version = (Read-RepoFile 'VERSION').Trim()

# Structured readiness and errors (1-8)
foreach ($state in @('Ready','SetupRequired','MigrationRequired','RuleSourceRequired','RulePublicationRequired','RuleActivationRequired','CampaignRequired','Degraded','Error')) {
    Assert-Requirement ($passes + 1) ($readiness -match "\b$state\b") "Readiness state $state is represented."
}
Assert-Requirement 10 ($readiness -match 'RULE_SCHEMA_MISSING' -and $readiness -match 'NO_PUBLISHED_RULE_RELEASE') 'Missing schema and empty publication are distinct.'
Assert-Requirement 11 ($readiness -match 'RULE_SOURCE_NOT_CONFIGURED' -and $readiness -match 'RULE_SOURCE_UNAVAILABLE') 'Missing and unavailable sources are distinct.'
Assert-Requirement 12 ($readiness -match 'CAMPAIGN_NOT_FOUND' -and $readiness -match 'RULESET_INCOMPATIBLE') 'Campaign absence and incompatibility are semantic states.'

# Permission-gated bootstrap (13-20)
Assert-Requirement 13 ($administration -match 'administration.Enabled' -and $administration -match 'ADMINISTRATION_DISABLED') 'Administrative setup is service-side gated.'
Assert-Requirement 14 ($administration -match 'ADMINISTRATIVE_APPROVAL_REQUIRED' -and $administration -match 'ApprovalPhrase') 'Setup requires explicit approval.'
Assert-Requirement 15 ($administration -match '001_initial.template.sql' -and $administration -match '002_rule_domain.template.sql') 'Bootstrap uses packaged 001/002 migrations.'
Assert-Requirement 16 ($administration -match '003_campaign_directory.template.sql' -and $administration -match '004_rule_source_configuration.template.sql') 'Additive upgrades are versioned and packaged.'
Assert-Requirement 17 (($schema003 + $schema004) -match 'IF COL_LENGTH|IF OBJECT_ID') 'Upgrade migrations are repeat-safe.'
Assert-Requirement 18 (($schema001 + $schema002 + $schema003 + $schema004) -notmatch 'DROP\s+(TABLE|SCHEMA|DATABASE)') 'Bootstrap migrations contain no destructive drop.'
Assert-Requirement 19 ($administration -notmatch 'request\.Sql|sqlText|arbitrary SQL') 'Administrative requests expose no raw SQL input.'
Assert-Requirement 20 ($tests -match 'BootstrapRequiresEnabledAdministrationAndExactApproval' -and $tests -match 'ApprovedBootstrapIsRepeatSafe') 'Approval and repeat safety have unit coverage.'

# Diagnostics and rule context (21-25)
Assert-Requirement 21 ($diagnostics -match 'IManagedReadinessService' -and $diagnostics -notmatch 'GetActiveAsync') 'Diagnostics no longer require an operational Rule Store.'
Assert-Requirement 22 ($ruleTools -match 'ManagedOperationResult' -and $ruleTools -match 'report.ErrorCode') 'Rule context preserves semantic failure results.'
Assert-Requirement 23 ($tests -match 'DiagnosticsRemainUsefulBeforeRuleSchemaExists') 'Pre-schema diagnostics have regression coverage.'
Assert-Requirement 24 ($tests -match 'InitializedEmptyRuleStoreRequiresPublication') 'Empty Rule Store state has regression coverage.'
Assert-Requirement 25 ($tests -match 'RuleContextReturnsActionableNoPublishedReleaseResult') 'No-release rule context is actionable and tested.'

# Rule source and initial publication (26-33)
Assert-Requirement 26 ($distribution -match 'https://github.com/xmiq/Eternal-Cycle-\.git') 'Official source identity comes from distribution metadata.'
Assert-Requirement 27 ($source -match 'IRuleSourceConfigurationStore' -and $source -match 'ManagedRuleCacheDirectory') 'Persisted sources and managed cache are supported.'
Assert-Requirement 28 ($source -match 'RepositoryRoot') 'Advanced local RepositoryRoot remains supported.'
Assert-Requirement 29 ($administration -match 'UseOfficialDefault' -and $administration -match 'SourceLocation') 'Official default can be overridden.'
Assert-Requirement 30 ($publication -match 'not yet configured' -and $publication -match 'Disabled') 'Never-initialized and explicitly disabled update states remain distinct.'
Assert-Requirement 31 ($tests -match 'OfficialSourceDefaultCanBeOverriddenAndSelectionPersists') 'Source selection persistence and override are tested.'
Assert-Requirement 32 ($tests -match 'PersistedLocalSourceIsReusedWithoutRepositoryRootConfiguration') 'Persisted source reuse is tested with a local Git fixture.'
Assert-Requirement 33 ($tests -match 'ExplicitInitialPublicationCanReachActiveState') 'Approved initial publication has regression coverage.'

# Campaign UX and logging (34-38)
Assert-Requirement 34 ($administration -match 'ec_list_campaigns' -and $administration -match 'ec_resolve_resume_campaign') 'Campaign discovery tools are present.'
Assert-Requirement 35 ($administration -match 'ec_create_campaign' -and $administration -match 'Destructive = true') 'Campaign creation is an explicit administrative action.'
Assert-Requirement 36 ($tests -match 'ResumeDiscoveryHidesOpaqueIdsWhenSelectionIsUnnecessary') 'Zero/one/many resume behavior is tested.'
Assert-Requirement 37 ($logging -match 'formatter\(state, null\)' -and $logging -match 'exception.GetType\(\)\.Name') 'File logging omits exception text and keeps exception category.'
Assert-Requirement 38 ($program -match 'SanitizedLogFile' -and $readme -match 'disabled unless') 'Sanitized file logging is optional and documented.'

# Documentation and acceptance (39-47)
Assert-Requirement 39 ($readme -match 'Zero-to-Game Quick Start' -and $readme -match 'Start a new Eternal Cycle game') 'Reference quick start gives a zero-to-game path.'
Assert-Requirement 40 ($running -match 'Enumerate -> Select -> Persist -> Reuse' -and $running -match 'SQL Server, MCP, GitHub, ChatGPT, Unsloth') 'Running guide is implementation-neutral.'
Assert-Requirement 41 ($player -match 'Start a new Eternal Cycle game' -and $player -match 'Continue my Eternal Cycle game') 'Player start/resume commands are explicit.'
Assert-Requirement 42 ($acceptance -match 'zero Eternal Cycle repository/source-file reads' -and $acceptance -match 'zero direct SQL') 'MCP-only acceptance forbids repository and direct database access by the AI.'
Assert-Requirement 43 ($acceptance -match '8,000 estimated tokens' -and $acceptance -match 'restart') 'Acceptance covers 8K retrieval and restart continuity.'
Assert-Requirement 44 ($managed -match 'structured outcomes' -and $managed -match 'First-Run Flow') 'Managed service contract owns readiness and first-run flow.'
Assert-Requirement 45 ($version -eq '1.0.0') 'VERSION remains 1.0.0 during pre-v1.1 RC work.'
Assert-Requirement 46 ($roadmap -match '\[x\] \*\*FR-020' -and $roadmap -match 'Phase 13') 'FR-020 is recorded without closing Phase 13.'
Assert-Requirement 47 ($future -match '### FR-020' -and $roadmap -match '\[∞\].*Future Revisions') 'FR-020 provenance and rolling Future Revisions remain present.'
Assert-Requirement 48 ($publicationRuntime -match 'ManagedReadinessState\.RulePublicationRequired' -and $publicationRuntime -match 'service readiness remains available') 'Background update checks defer safely until first-run readiness permits publication.'

if ($failures.Count -gt 0) {
    Write-Output "FR-020 managed first-run harness: FAIL ($($failures.Count) failure(s))"
    $failures | ForEach-Object { Write-Output "- $_" }
    throw 'FR-020 managed first-run harness failed.'
}

Write-Output "FR-020 managed first-run harness: PASS ($passes assertions)"
