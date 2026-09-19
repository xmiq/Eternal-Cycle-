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
$operations = Read-RepoFile "$source/ManagedOperations.cs"
$operationStore = Read-RepoFile "$source/SqlServerManagedOperationStore.cs"
$preparation = Read-RepoFile "$source/RulePreparation.cs"
$publication = Read-RepoFile "$source/ManagedRulePublication.cs"
$readiness = Read-RepoFile "$source/ManagedReadiness.cs"
$administration = Read-RepoFile "$source/ManagedAdministration.cs"
$diagnostics = Read-RepoFile "$source/ServiceDiagnostics.cs"
$program = Read-RepoFile "$source/Program.cs"
$gitProvider = Read-RepoFile "$source/GitRuleSourceProvider.cs"
$publishedStore = Read-RepoFile "$source/SqlServerPublishedRuleStore.cs"
$schema = Read-RepoFile "$source/Schema/007_durable_managed_operations.sql"
$schemaTemplate = Read-RepoFile "$source/Schema/007_durable_managed_operations.template.sql"
$project = Read-RepoFile "$source/EternalCycle.Persistence.Mcp.csproj"
$manifest = Read-RepoFile 'docs/rules/rule-source-manifest.json'
$tests = Read-RepoFile "$testsRoot/ManagedOperationsAndProgressiveReadinessTests.cs"
$firstRunTests = Read-RepoFile "$testsRoot/ManagedFirstRunTests.cs"
$compatibilityTests = Read-RepoFile "$testsRoot/ManagedRuleSourceCompatibilityTests.cs"
$controlPlaneTests = Read-RepoFile "$testsRoot/ManagedControlPlaneRepairTests.cs"
$errorDump = Read-RepoFile "$source/ErrorDump.cs"
$managedDiagnostics = Read-RepoFile "$source/ManagedDiagnostics.cs"
$managedConfiguration = Read-RepoFile "$source/ManagedConfiguration.cs"
$migration008 = Read-RepoFile "$source/Schema/008_diagnostic_operation_correlation.sql"
$migration008Template = Read-RepoFile "$source/Schema/008_diagnostic_operation_correlation.template.sql"
$migration009 = Read-RepoFile "$source/Schema/009_managed_operation_execution_leases.sql"
$migration009Template = Read-RepoFile "$source/Schema/009_managed_operation_execution_leases.template.sql"
$managedContract = Read-RepoFile 'docs/persistence/MANAGED_OPERATIONS.md'
$publicationContract = Read-RepoFile 'docs/rules/MANAGED_RULE_PUBLICATION.md'
$retrievalContract = Read-RepoFile 'docs/rules/RULE_COMPILATION_AND_RETRIEVAL.md'
$versioning = Read-RepoFile 'design/RELEASE_VERSIONING.md'
$method = Read-RepoFile 'design/DEVELOPMENT_METHOD.md'
$acceptance = Read-RepoFile "$reference/MCP_ONLY_ACCEPTANCE_TEST.md"
$roadmap = Read-RepoFile 'design/ROADMAP.md'
$future = Read-RepoFile 'design/FUTURE_REVISIONS.md'
$decisions = Read-RepoFile 'design/DECISIONS.md'
$distributionBuilder = Read-RepoFile 'tools/build_distribution.ps1'
$distributionValidator = Read-RepoFile 'tools/test_distribution_archive.ps1'
$version = (Read-RepoFile 'VERSION').Trim()

Assert-Requirement 1 ($operations -match 'enum ManagedOperationState' -and $operations -match 'Queued' -and $operations -match 'Interrupted') 'Durable operations expose the required explicit states.'
Assert-Requirement 2 ($operations -match 'EnqueueOrReuseAsync' -and $operationStore -match 'SERIALIZABLE') 'Equivalent initiation is durably deduplicated.'
Assert-Requirement 3 ($operations -match 'ec_get_operation_status' -and $operations -match 'ec_list_managed_operations') 'Status and recent-operation discovery tools are registered.'
Assert-Requirement 4 ($program -match 'AddHostedService<ManagedOperationWorker>') 'The background worker is registered with the host.'
Assert-Requirement 5 ($operations -match 'RecoverInterruptedAsync' -and $operations -match 'ClaimNextAsync') 'Worker startup recovers and reclaims persisted work.'
Assert-Requirement 6 ($operationStore -match "N'Interrupted'" -and $operationStore -match "N'Running', N'Cancelling'") 'Restart recovery prevents phantom Running or Cancelling state.'
Assert-Requirement 7 ($operations -match 'ManagedOperationTimeout' -and $operations -match 'CreateLinkedTokenSource') 'Service-owned timeout policy is independent of initiation.'
Assert-Requirement 8 ($operations -match 'CancellationToken.None' -and $tests -match 'IgnoresLaterClientCancellation') 'Client cancellation cannot erase accepted durable work.'
Assert-Requirement 9 ($operations -match 'ResultRuleReleaseId' -and $tests -match 'ThenCompletedRelease') 'Completion exposes the resulting Rule Release.'
Assert-Requirement 10 ($operations -match 'ErrorCode' -and $tests -match 'PreservesStructuredCausalStatus') 'Failure retains safe structured causal status.'

Assert-Requirement 11 ($schema -match 'managed_operations' -and $schema -match 'rule_source_preparation') 'Migration 007 adds both durable operations and preparation state.'
Assert-Requirement 12 ($schema -match 'base_priority' -and $schema -match 'priority_boost') 'Preparation stores base and dynamic priority separately.'
Assert-Requirement 13 ($project -match '007_durable_managed_operations.sql' -and $project -match '007_durable_managed_operations.template.sql') 'Migration 007 and its template are packaged.'
Assert-Requirement 14 ($schemaTemplate -match '\{\{schema\}\}' -and $schemaTemplate -match '\{\{schema_name\}\}' -and $schemaTemplate -notmatch '\{\{DOMAIN_SCHEMA\}\}') 'Migration 007 uses the renderer-supported trusted Domain Namespace tokens.'
Assert-Requirement 15 ($preparation -match 'RuntimeKernel' -and $preparation -match 'OptionalRare') 'Portable preparation tiers cover kernel through optional rules.'
Assert-Requirement 16 ($preparation -match 'GetNextPendingAsync' -and $preparation -match 'priority_boost DESC') 'Remaining preparation reads current dynamic priority from durable state.'
Assert-Requirement 17 ($publication -match 'PrepareMinimumClosure' -and $publication -match 'PrepareRemainingAsync') 'Minimum and remaining preparation are distinct publication stages.'
Assert-Requirement 18 ($publication -match 'GetNextPendingAsync' -and $publication -match 'MarkReadyAsync') 'Background publication consumes durable preparation state end to end.'
Assert-Requirement 19 ($publication -match 'RaisePriorityAsync' -and $publication -match 'RuleContextPendingException') 'Unavailable context raises durable priority and refuses speculative resolution.'
Assert-Requirement 20 ($retrievalContract -match 'structured pending result' -and $retrievalContract -match 'does not substitute conversational memory or a guessed rule') 'The implementation-neutral retrieval contract exposes pending closure without speculative substitution.'

Assert-Requirement 21 ($readiness -match 'ServiceReady' -and $readiness -match 'PersistenceReady') 'Service and persistence readiness are explicit.'
Assert-Requirement 22 ($readiness -match 'RuleKernelReady' -and $readiness -match 'CampaignBootstrapReady') 'Kernel and bootstrap readiness are explicit.'
Assert-Requirement 23 ($readiness -match 'FullRulesetReady' -and $tests -match 'GameplayCanBeReadyBeforeFullRuleset') 'Gameplay readiness is demonstrably independent from full preparation.'
Assert-Requirement 24 ($tests -match 'KernelOrBootstrapGapBlocksGameplay') 'Missing minimum closure blocks play without false authority.'
Assert-Requirement 25 ($tests -match 'UnavailableRequestedClosureBecomesPendingAndRaisesPriorityThenSucceeds') 'Rule context pending, priority, and later success are tested end to end.'
Assert-Requirement 26 ($tests -match 'DynamicPriorityBoostChangesTheNextPendingRuleSource') 'Gameplay priority changes actual next preparation work.'
Assert-Requirement 27 ($tests -match 'CampaignRelevantPreparationPrecedesOptionalRare') 'Campaign-relevant rules outrank unrelated optional rules.'

Assert-Requirement 28 ($diagnostics -match 'string\? CampaignId' -and $tests -match 'ServiceDiagnosticsDoNotRequireCampaignAndCampaignCanEnrichThem') 'Service diagnostics work without a campaign and accept optional enrichment.'
Assert-Requirement 29 ($readiness -match 'LatestRelevantFailure' -and $diagnostics -match 'LatestManagedOperation') 'Current readiness and service diagnostics preserve latest causal operation evidence.'
Assert-Requirement 30 ($administration -match 'UserApproved' -and $administration -match 'AdministrativeInterventionRequired') 'User approval and operator intervention are distinct.'
Assert-Requirement 31 ($administration -match 'RequireOperatorConfirmation' -and $firstRunTests -match 'BootstrapDistinguishesUserApprovalFromOperatorIntervention') 'Magic phrases are optional operator safeguards, not normal approval UX.'
Assert-Requirement 32 ($firstRunTests -match 'StableDoesNotFallForwardAndPrereleaseRequiresExplicitSelection') 'Normal prerelease selection remains semantic and explicit.'

Assert-Requirement 33 ($gitProvider -match 'rev-list' -and $gitProvider -match 'CommitsSinceBase') 'The Git reference derives commits-since-base metadata.'
Assert-Requirement 34 ($publishedStore -match 'source_identity' -and $publishedStore -match 'display_version') 'Immutable source identity and display metadata persist separately.'
Assert-Requirement 35 ($compatibilityTests -match 'MovingRefDoesNotRewriteProvenance') 'Moving discovery refs cannot rewrite old publication provenance.'
Assert-Requirement 36 ($tests -match 'PrereleaseDisplayVersionIsDeterministicButShaRemainsIdentity') 'Derived rc.N is deterministic but not canonical identity.'
Assert-Requirement 37 ($versioning -match 'full release tag.*immutable' -and $versioning -match 'moving discovery tag') 'Eternal Cycle-wide release governance distinguishes immutable release and moving RC tags.'
Assert-Requirement 38 ($version -eq '1.0.0') 'Root VERSION remains 1.0.0.'

Assert-Requirement 39 ($managedContract -match 'does not require \.NET, MCP, SQL Server, T-SQL, Git') 'Managed Operations remains implementation-neutral.'
Assert-Requirement 40 ($method -match 'Reference implementation only' -and $method -match 'Managed Service contract' -and $method -match 'Eternal Cycle-wide') 'The three-level Generalization Audit is explicit.'
Assert-Requirement 41 ($method -match 'observe real failure' -and $method -match 'add regression or acceptance evidence') 'The deployment feedback loop is canonical.'
Assert-Requirement 42 ($acceptance -match '### A\. LM Studio' -and $acceptance -match '### F\. Moving RC Update') 'All six real-deployment scenarios are documented without claiming execution.'
Assert-Requirement 43 ($roadmap -match '\[x\] \*\*FR-021' -and $roadmap -match '\[∞\].*Future Revisions') 'FR-021 is complete while Phase 13 rolling governance remains active.'
Assert-Requirement 44 ($future -match '### FR-021' -and $future -match 'Status:\*\* Closed') 'Future Revision provenance records FR-021 closure.'
Assert-Requirement 45 ($manifest -match 'preparationTier') 'Rule Source manifest carries portable preparation metadata.'
Assert-Requirement 46 ($publicationContract -match 'GameplayReady' -and $publicationContract -match 'FullRulesetReady') 'Canonical publication guidance distinguishes progressive readiness levels.'
Assert-Requirement 47 ($operations -match 'var recovered = await store\.RecoverInterruptedAsync' -and $tests -match 'HostedWorkerWaitsForDurableStoreSetupThenRecoversAndCompletes') 'Fresh-database worker startup waits for durable setup and reconciles ownership before each claim opportunity.'
Assert-Requirement 48 ($publication -match 'catch \(OperationCanceledException\)' -and $publication -match 'throw;') 'Publication propagates cancellation to its durable owner.'
Assert-Requirement 49 ($operations -match 'MANAGED_OPERATION_INTERRUPTED' -and $operations -match 'MANAGED_OPERATION_TIMEOUT' -and $operations -match 'MANAGED_OPERATION_CANCELLED_UNEXPECTED') 'The durable owner distinguishes host shutdown, its timeout, and unexplained parent cancellation.'
Assert-Requirement 50 ($gitProvider -match 'RULE_SOURCE_ACQUISITION_TIMEOUT' -and $gitProvider -match 'RULE_SOURCE_PROCESS_TIMEOUT') 'Overall acquisition and individual Git-process timeouts are distinct.'
Assert-Requirement 51 ($gitProvider -match 'execution\.OperationId' -and $gitProvider -match 'execution\.CorrelationId' -and $gitProvider -notmatch 'OP-\{Guid') 'Git observations use supplied durable identity rather than invented operation correlation.'
Assert-Requirement 52 ($migration008 -match 'ADD operation_id nvarchar\(128\) NULL' -and $migration008 -match 'WHERE operation_id IS NOT NULL' -and $migration008Template -match '\{\{schema\}\}') 'Migration 008 additively persists operation correlation for fixed and routed schemas.'
Assert-Requirement 53 ($project -match '008_diagnostic_operation_correlation.sql' -and $project -match '008_diagnostic_operation_correlation.template.sql') 'Migration 008 and its template are packaged.'
Assert-Requirement 54 ($managedDiagnostics -match 'string\? OperationId' -and $errorDump -match 'GetByCorrelationAsync' -and $controlPlaneTests -match 'ErrorDumpFindsTrueDurableEvidenceByOperationCorrelationOrBoth') 'Diagnostics and Error Dumps support durable Operation ID and Correlation ID lookup.'
Assert-Requirement 55 ($managedConfiguration -match 'EffectiveValue' -and $controlPlaneTests -match 'ConfigurationDiscoveryRequiresNoDatabaseAndNeverReturnsSensitiveValues') 'Configuration discovery exposes safe effective values while retaining the sensitive-value boundary.'
Assert-Requirement 56 ($gitProvider -match 'RULE_SOURCE_REF_NOT_FOUND' -and $compatibilityTests -match 'MissingRemoteRefFailsSpecificallyWithoutWaitingForTimeout') 'Missing refs fail quickly with a specific non-retryable error.'
Assert-Requirement 57 ($gitProvider -match 'PayloadMetadataFile' -and $gitProvider -match 'manifest\.Sources' -and $compatibilityTests -match 'Assert\.False\(Directory\.Exists\(Path\.Combine\(payloadPath, "tests"\)\)\)') 'Persistent runtime materialization contains only manifest-declared files and provenance.'
Assert-Requirement 58 ($compatibilityTests -match 'UserSelectedLocalCloneWithCommittedRuleChangesUsesSameValidationPipeline') 'A user-selected modified local clone is a first-class source using normal validation.'
Assert-Requirement 59 ($publicationContract -match 'semantic identity' -and $publicationContract -match 'Existing local clones are inspected in place') 'Managed Rule Publication documents technical validation without official semantic conformity.'
Assert-Requirement 60 ($decisions -match 'Rule Source Choice Is User-Owned' -and $decisions -match 'Review Distributions and Runtime Rule Payloads Are Independent') 'Governance preserves source freedom and separates review archives from runtime payloads.'
Assert-Requirement 61 ($distributionBuilder -match "'bin'" -and $distributionBuilder -match "'obj'" -and $distributionValidator -match 'mcp-dotnet-tsql/src' -and $distributionValidator -match 'Forbidden generated or local path') 'Distribution tooling retains review source while excluding generated bin/obj/cache paths.'
Assert-Requirement 62 ($tests -match 'HostShutdownLeavesDurableOperationInterruptedAndRecoverable' -and $tests -match 'UnexpectedParentCancellationIsNotMisreportedAsShutdownOrTimeout') 'Cancellation ownership and retry behavior have direct regression coverage.'
Assert-Requirement 63 ($migration009 -match 'execution_owner_id' -and $migration009 -match 'execution_lease_expires_at' -and $migration009 -match 'execution_attempt_count') 'Migration 009 adds bounded execution ownership and attempt evidence.'
Assert-Requirement 64 ($migration009Template -match '\{\{schema\}\}' -and $migration009Template -match '\{\{schema_name\}\}' -and $project -match '009_managed_operation_execution_leases') 'Migration 009 supports routed schemas and is packaged.'
Assert-Requirement 65 ($operationStore -match 'RenewExecutionLeaseAsync' -and $operations -match 'MaintainExecutionOwnershipAsync') 'The active worker renews its durable execution claim.'
Assert-Requirement 66 ($operationStore -match 'execution_lease_expires_at <= SYSUTCDATETIME\(\)' -and $operationStore -match 'execution_owner_id IS NULL') 'Only missing or expired execution ownership is reconciled as interrupted.'
Assert-Requirement 67 ($operationStore -match 'AND execution_owner_id = @execution_owner_id' -and $operationStore -match 'execution_attempt_count = execution_attempt_count \+ 1') 'State writes are ownership-guarded and retries record another attempt.'
Assert-Requirement 68 ($tests -match 'LiveExecutionLeasePreservesRunningDeduplication' -and $tests -match 'ActiveProcessorRenewsExecutionLeaseAndRemainsDeduplicated') 'Live worker ownership remains Running and preserves active-operation deduplication.'
Assert-Requirement 69 ($tests -match 'RepeatInitiationRecoversExpiredRunningPublicationWithoutChangingIdentity' -and $tests -match 'Assert\.Equal\(original\.CorrelationId, recovered\.CorrelationId\)') 'Power-loss recovery reuses the original Operation ID and Correlation ID.'
Assert-Requirement 70 ($managedContract -match 'bounded, renewable execution ownership' -and $decisions -match 'Managed Execution Ownership Is Bounded and Recoverable') 'The repair is classified at the Managed Service contract layer rather than as fictional Canon.'
Assert-Requirement 71 ($administration -match 'RULE_PUBLICATION_RECOVERY_QUEUED' -and $firstRunTests -match 'RepeatPublicationReportsRecoveryForExpiredOperationOwnership') 'Repeat initiation reports recoverable interruption rather than falsely describing an orphan as active.'
Assert-Requirement 72 ($readiness -match 'execution_owner_id' -and $readiness -match 'execution_lease_expires_at' -and $readiness -match 'execution_attempt_count') 'Readiness requires migration 009 before operation tools query lease-aware storage.'
Assert-Requirement 73 ($operationStore -match 'AND execution_lease_expires_at > SYSUTCDATETIME\(\)' -and $tests -match 'ExpiredExecutionLeaseRejectsStaleStateMutation') 'An expired owner cannot write completion before reconciliation.'

if ($failures.Count -gt 0) {
    Write-Output "FR-021 durable Managed-operation harness: FAIL ($($failures.Count) failure(s))"
    $failures | ForEach-Object { Write-Output "- $_" }
    throw 'FR-021 durable Managed-operation harness failed.'
}

Write-Output "FR-021 durable Managed-operation harness: PASS ($passes assertions)"
