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

$selection = Read-RepoFile 'docs/persistence/PERSISTENCE_STRATEGY_SELECTION.md'
$portable = Read-RepoFile 'docs/persistence/PORTABLE_PERSISTENCE_ARCHITECTURE.md'
$managed = Read-RepoFile 'docs/persistence/MANAGED_DATA_SERVICE.md'
$mcp = Read-RepoFile 'docs/persistence/MCP_PERSISTENCE_MODE.md'
$namespace = Read-RepoFile 'docs/persistence/LOGICAL_DATA_NAMESPACE.md'
$rulePublication = Read-RepoFile 'docs/rules/MANAGED_RULE_PUBLICATION.md'
$ruleRetrieval = Read-RepoFile 'docs/rules/RULE_COMPILATION_AND_RETRIEVAL.md'
$manifest = Read-RepoFile 'docs/rules/rule-source-manifest.json'
$support = Read-RepoFile 'docs/support/COMMUNITY_FEEDBACK_AND_DIAGNOSTICS.md'
$supportMetadata = Read-RepoFile 'SUPPORT.md'
$configuration = Read-RepoFile 'templates/PERSISTENCE_CONFIGURATION_TEMPLATE.md'
$bootstrap = Read-RepoFile 'docs/gm/CAMPAIGN_BOOTSTRAP.md'
$runtime = Read-RepoFile 'docs/ai/AI_RUNTIME_MODEL.md'
$roadmap = Read-RepoFile 'design/ROADMAP.md'
$future = Read-RepoFile 'design/FUTURE_REVISIONS.md'

$referenceRoot = 'examples/tooling/managed-data/mcp-dotnet-tsql'
$contracts = Read-RepoFile "$referenceRoot/src/EternalCycle.Persistence.Mcp/Contracts.cs"
$routing = Read-RepoFile "$referenceRoot/src/EternalCycle.Persistence.Mcp/SqlServerSchemaRouting.cs"
$compiler = Read-RepoFile "$referenceRoot/src/EternalCycle.Persistence.Mcp/RuleCompilation.cs"
$publication = Read-RepoFile "$referenceRoot/src/EternalCycle.Persistence.Mcp/ManagedRulePublication.cs"
$sourceProvider = Read-RepoFile "$referenceRoot/src/EternalCycle.Persistence.Mcp/GitRuleSourceProvider.cs"
$publishedStore = Read-RepoFile "$referenceRoot/src/EternalCycle.Persistence.Mcp/SqlServerPublishedRuleStore.cs"
$diagnostics = Read-RepoFile "$referenceRoot/src/EternalCycle.Persistence.Mcp/ServiceDiagnostics.cs"
$readiness = Read-RepoFile "$referenceRoot/src/EternalCycle.Persistence.Mcp/ManagedReadiness.cs"
$domainSchema = Read-RepoFile "$referenceRoot/src/EternalCycle.Persistence.Mcp/Schema/002_rule_domain.sql"
$referenceReadme = Read-RepoFile "$referenceRoot/README.md"
$publicationTests = Read-RepoFile "$referenceRoot/tests/EternalCycle.Persistence.Mcp.Tests/ManagedRulePublicationTests.cs"
$publicationRegressionTests = Read-RepoFile "$referenceRoot/tests/EternalCycle.Persistence.Mcp.Tests/ManagedPublicationRegressionTests.cs"
$routingTests = Read-RepoFile "$referenceRoot/tests/EternalCycle.Persistence.Mcp.Tests/SchemaRoutingAndRuleCompilationTests.cs"

# Strategy selection (1-5)
Assert-Requirement 1 ($selection -match 'Enumerate -> Select -> Persist -> Reuse' -and $selection -match 'Select `DIRECT`') 'DIRECT is capability-selected and persisted.'
Assert-Requirement 2 ($selection -match 'Select `MANAGED`' -and $configuration -match 'Persistence Strategy:.*DIRECT \| MANAGED') 'MANAGED is capability-selected and persisted.'
Assert-Requirement 3 ($selection -match 'every later session loads that selection' -and $bootstrap -match 'persisted Persistence Strategy') 'Resume reuses the persisted strategy.'
Assert-Requirement 4 ($selection -match 'must not.*silently switch') 'Silent strategy switching is forbidden.'
Assert-Requirement 5 ($selection -match 'explicit.*migration' -and $selection -match 'rollback') 'Strategy change requires validated migration.'

# Managed authority (6-8)
Assert-Requirement 6 ($managed -match 'replaces the client''s Direct local/cloud save machinery' -and $managed -match 'does not maintain a parallel canonical SQLite') 'Managed authority creates no competing Direct save.'
Assert-Requirement 7 ($managed -match 'validated completion evidence' -and $managed -match '`💾`') 'Managed completion evidence is authoritative.'
Assert-Requirement 8 ($managed -match '`⚠️` means service, transaction, durability, or validation failure') 'Managed failures are surfaced.'

# Data Namespace (9-12)
Assert-Requirement 9 ($namespace -match 'stable logical identifier independent of its physical name') 'Logical namespace survives physical renaming.'
Assert-Requirement 10 ($namespace -match 'Several compatible campaigns may share one Data Namespace' -and $namespace -match 'Campaign ID') 'Shared namespaces retain campaign isolation.'
Assert-Requirement 11 ($namespace -match 'Different World Models may use different namespaces') 'Different world namespaces remain isolated.'
Assert-Requirement 12 ($namespace -match 'never interchangeable' -and $routingTests -match 'LogicalNamespaceIdentityIsIndependentOfPhysicalSchemaName') 'Physical mappings do not become world identity.'

# T-SQL reference (13-18)
Assert-Requirement 13 ($domainSchema -match '\[ec_domain\]' -and $contracts -match 'DomainSchema.*ec_domain') 'ec_domain is supported.'
Assert-Requirement 14 ($namespace -match 'ec_<world>' -and $referenceReadme -match 'ec_mainworld') 'ec_<world> discoverability convention is supported.'
Assert-Requirement 15 ($contracts -match 'DataNamespaces' -and $routing -match 'SchemaName') 'Trusted configurable schema mappings are supported.'
Assert-Requirement 16 ($routing -match 'GeneratedRegex' -and $routingTests -match 'UnsafeSchemaIdentifiersAreRejected') 'Unsafe identifiers are rejected.'
Assert-Requirement 17 ($routingTests -match 'CrossSchemaRoutesRemainIsolated' -and $routingTests -match 'SharedSchemaDoesNotCollapseCampaignIdentity') 'Cross-schema and shared-schema campaigns remain isolated.'
Assert-Requirement 18 ($routingTests -match 'MigrationRenderingTargetsOnlyTheSelectedSchema' -and $routing -match 'RenderDomain') 'World and domain migrations are explicitly scoped.'

# Managed compiled rules (19-28)
Assert-Requirement 19 ($publication -match 'RuleCompiler.Compile' -and $publication -match 'RuleReleaseState.Candidate') 'Canonical source compiles to a candidate release.'
Assert-Requirement 20 ($publication -match 'ValidateCandidate' -and $publication -match 'RuleReleaseState.Validated') 'Validation precedes publication and activation.'
Assert-Requirement 21 ($domainSchema -match 'CREATE TABLE \[ec_domain\]\.rule_releases' -and $domainSchema -match 'CREATE TABLE \[ec_domain\]\.rule_chunks') 'Compiled publications are stored in ec_domain.'
Assert-Requirement 22 ($publication -match 'CandidateFailed' -and $publicationRegressionTests -match 'CompilerFailureIsNotMisclassifiedAsSourceUnavailable' -and $publicationRegressionTests -match 'ActiveReleasePreserved') 'Compilation failure preserves the active release.'
Assert-Requirement 23 ($publicationTests -match 'InvalidCandidatePreservesPreviousActiveRelease') 'Validation failure preservation has regression coverage.'
Assert-Requirement 24 ($publishedStore -match 'MERGE \{\{schema\}\}\.active_rule_releases' -and $publishedStore -match 'CommitAsync') 'Activation is atomic in the T-SQL reference.'
Assert-Requirement 25 ($publication -match 'PublishedRuleContextProvider' -and $publicationTests -match 'RuntimeRetrievalUsesPublishedStoreWithoutCallingSourceProvider') 'Runtime retrieval uses published rules, not repository compilation.'
Assert-Requirement 26 ($publication -match 'SourceIdentity' -and $sourceProvider -match 'commit SHA|sourceIdentity') 'Immutable source provenance is retained.'
Assert-Requirement 27 ($routingTests -match 'WorldSpecificRetrievalExcludesOtherWorldAndStaysWithinEightK') 'World applicability excludes unrelated worlds.'
Assert-Requirement 28 ($domainSchema -match 'rule_update_checks' -and $readiness -match 'rule_update_checks') 'Source freshness and update state are detectable through readiness-safe inspection.'

# Source provider (29-33)
Assert-Requirement 29 ($publicationTests -match 'UnchangedSourceDoesNotRepublish') 'Unchanged sources are not republished.'
Assert-Requirement 30 ($publicationTests -match 'ChangedSourceCreatesNewCandidateAndAtomicActivation') 'Changed sources create candidates.'
Assert-Requirement 31 ($publicationTests -match 'SourceFailureAndOfflineOperationPreserveActiveRelease') 'Source acquisition failure preserves active release.'
Assert-Requirement 32 ($publication -match 'RuleUpdatePolicy.Disabled' -and $rulePublication -match 'disabled/offline operation') 'Offline operation can serve a valid publication.'
Assert-Requirement 33 ($sourceProvider -match 'rev-parse' -and $sourceProvider -match 'CommitSha' -and $sourceProvider -match 'sourceIdentity') 'Git source identity resolves to an immutable commit SHA.'

# 8K retrieval (34-38)
Assert-Requirement 34 ($routingTests -match 'A_DefaultSchemaUsesEc' -and $routingTests -match 'H_DefaultSchemaIsNotMandatory') 'FR-018 scenarios A-H remain present.'
Assert-Requirement 35 ($routingTests -match 'I_WorldSpecificRetrievalExcludesOtherWorldAndStaysWithinEightK') 'World-specific retrieval scenario remains present.'
Assert-Requirement 36 ($compiler -match 'DefaultContextBudget = 8_000' -and $ruleRetrieval -match '8,000 estimated rule tokens') 'Normal Rule Packets retain the 8K target.'
Assert-Requirement 37 ($rulePublication -match 'no hidden full-repository prompt' -and $runtime -notmatch 'In `MANAGED`.*crawl the repository') 'The measured packet cannot hide full-repository loading.'
Assert-Requirement 38 ($compiler -match 'BuildDependencyClosure' -and $manifest -match '"dependencies"' -and $publicationTests -match 'RuntimeRetrievalIncludesExplicitDependencies') 'Dependencies and provenance remain explicit and tested.'

# Diagnostics/support (39-44)
Assert-Requirement 39 ($diagnostics -match 'ActiveRuleReleaseId' -and $diagnostics -match 'CanonicalSourceIdentity') 'Diagnostics identify implementation and rule provenance.'
Assert-Requirement 40 ($diagnostics -match 'Credentials, connection strings, locators' -and $publicationTests -match 'DiagnosticsAreSanitized') 'Sensitive configuration is redacted.'
Assert-Requirement 41 ($publicationTests -match 'DoesNotContain\("ConnectionString"' -and $publicationTests -match 'DoesNotContain\("Password"') 'Tests exclude connection strings and credentials.'
Assert-Requirement 42 ($diagnostics -match 'reference implementation' -and $managed -match 'another authorized interface') 'Reports distinguish reference and third-party implementations.'
Assert-Requirement 43 ($support -match 'Sensitive campaign data omitted' -and $support -match 'Sanitized diagnostics') 'Issue reports omit Campaign Canon.'
Assert-Requirement 44 ($support -match 'Without integration, prepare the report' -and $supportMetadata -match 'issues') 'Useful official support routing works without submission integration.'

# Backward compatibility (45-48)
Assert-Requirement 45 ($selection -match 'Release 1 SQLite and Google Drive campaigns remain `DIRECT`') 'Existing Direct SQLite campaigns remain valid.'
Assert-Requirement 46 ($roadmap -match 'Release-neutral Provisional Rulings' -and $bootstrap -match '`NORMAL`') 'NORMAL, VALIDATION, and DEVELOPMENT lifecycle behavior remains release-neutral.'
Assert-Requirement 47 ($support -match 'A `NORMAL` campaign remains `NORMAL`' -and $roadmap -match 'Provisional Rulings') 'Provisional rulings and reports do not create playtest campaigns.'
Assert-Requirement 48 ($roadmap -match '\[x\] \*\*FR-017' -and $roadmap -match '\[x\] \*\*FR-018' -and $future -match '### FR-019') 'FR-017/FR-018 history remains and FR-019 records the correction.'

if ($failures.Count -gt 0) {
    Write-Output "FR-019 managed-data architecture harness: FAIL ($($failures.Count) failure(s))"
    $failures | ForEach-Object { Write-Output "- $_" }
    throw 'FR-019 managed-data architecture harness failed.'
}

Write-Output "FR-019 managed-data architecture harness: PASS ($passes assertions)"
