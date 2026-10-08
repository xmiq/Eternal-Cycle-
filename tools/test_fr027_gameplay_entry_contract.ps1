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

# This verifies the design checkpoint, not executable or live-host enforcement.
$contract = Read-RepoFile 'docs/ai/CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md'
$plan = Read-RepoFile 'design/FR_027_EXECUTION_PLAN.md'
$audit = Read-RepoFile 'design/audits/FR_027A_CAMPAIGN_BINDING_GAMEPLAY_ENTRY_CONTRACT.md'
$roadmap = Read-RepoFile 'design/ROADMAP.md'
$future = Read-RepoFile 'design/FUTURE_REVISIONS.md'
$decisions = Read-RepoFile 'design/DECISIONS.md'
$terms = Read-RepoFile 'design/TERMINOLOGY.md'
$registry = Read-RepoFile 'docs/DOCUMENT_REGISTRY.md'
$index = Read-RepoFile 'docs/ai/README.md'
$manifest = Read-RepoFile 'docs/rules/rule-source-manifest.json'

Assert-Requirement 1 ($contract -match 'D-G entry, mutation, trace and live acceptance' -and $contract -match 'not Player Interaction authorization' -and $audit -match 'specified, not executed in A') 'Contract, binding and intake acceptance are distinct from gameplay and live acceptance.'
Assert-Requirement 2 ($contract -match 'Every actual newly submitted player gameplay input creates exactly one' -and $contract -match 'service-owned interaction authorization') 'Actual input and service-owned mutation authorization are required.'
Assert-Requirement 3 ($contract -match 'semantic similarity' -and $contract -match 'Conversation search cannot choose among campaigns') 'Campaign selection rejects story and conversation inference.'
Assert-Requirement 4 ($contract -match 'last-verified time is optional' -and $contract -match 'Binding identity and generation' -and $contract -match 'Rules/profile reference') 'Binding records minimal identity, version, profile and readiness evidence.'
Assert-Requirement 5 ($contract -match 'Zero candidates' -and $contract -match 'One candidate may auto-bind' -and $contract -match 'Multiple candidates return meaningful') 'Zero/one/multiple discovery has safe explicit outcomes.'
Assert-Requirement 6 ($contract -match 'without substituting another candidate' -and $contract -match 'Never silently rebase a resolved delta') 'Unavailable and stale binding cannot guess or reroute.'
Assert-Requirement 7 ($contract -match 'create a successor binding generation' -and $contract -match 'reconcile any unknown commit') 'Switch preserves original write/decision authority and history.'
Assert-Requirement 8 ($contract -match 'trusted session scope is unavailable' -and $contract -match 'Transport/client/model/service restart does not create another input') 'Recovery uses durable scope without manufacturing input.'
Assert-Requirement 9 ($contract -match 'Player Interaction is an authorization envelope' -and $contract -match 'not a replacement for the existing Gameplay Interaction') 'Input identity does not redefine existing adjudication/save units.'
Assert-Requirement 10 ($contract -match 'model cannot mint new authority' -and $contract -match 'Two separate actual submissions with identical text') 'Trusted ingress and delivery idempotency do not rely on text alone.'
Assert-Requirement 11 ($contract -match 'cannot supply trustworthy input-origin evidence' -and $contract -match 'real user-confirmed submission') 'Unsupported host-origin assurance fails honestly.'
$states = @('RECEIVED', 'ENTRY_PENDING', 'OPEN', 'PERSISTING', 'AWAITING_PLAYER_INPUT', 'COMPLETED', 'BLOCKED', 'CANCELLED')
Assert-Requirement 12 (@($states | Where-Object { $contract -notmatch ('(?m)^\| ' + $_ + ' \|') }).Count -eq 0) 'State table covers transitions, evidence, allowed work and prohibitions.'
Assert-Requirement 13 ($contract -match 'YIELDED is a disposition' -and $contract -match 'Only one forward-authorized Player Interaction') 'Yield and concurrent interaction authority cannot disagree.'
Assert-Requirement 14 ($contract -match 'Presenting alternatives creates a pending player decision' -and $contract -match 'Retrieving the GM.s prior proposal cannot promote it') 'Proposals and conversation evidence do not authorize a choice.'
Assert-Requirement 15 ($contract -match 'Certain correction supplies' -and $contract -match 'Investigative correction supplies' -and $contract -match 'Player clarification required creates') 'Correction classes preserve authority and missing facts.'
Assert-Requirement 16 ($contract -match 'ec_begin_gameplay_interaction' -and $contract -match 'first authoritative \*gameplay\* operation' -and $contract -match 'not entry authorization') 'Dedicated mandatory entry is not generic retrieval.'
Assert-Requirement 17 ($contract -match 'trusted actual submission identity' -and $contract -match 'Ordinary players supply no Campaign ID') 'Entry inputs are minimal trusted context, not player backend identifiers.'
Assert-Requirement 18 ($contract -match 'Fix operation `gameplay.resolve`' -and $contract -match 'Invoke existing FR-026 A->E' -and $contract -match 'Required Kernel and GM Runtime Procedure are automatic') 'Entry reuses actual FR-026 selection and mandatory Procedure.'
Assert-Requirement 19 ($contract -match 'Required follow-up reads' -and $contract -match 'ENTRY_PENDING persists until required reads/coverage are evidenced' -and $contract -match 'No ranking, vocabulary, dependency, or packet ceiling changes') 'Read/coverage gates cannot weaken selection or grant provisional authority.'
Assert-Requirement 20 ($contract -match 'Host Bootstrap is compact' -and $contract -match 'never equates confirmation with technical verification') 'Host readiness remains distinct from Procedure and verification.'
Assert-Requirement 21 ($contract -match 'record identity/revision' -and $contract -match 'FR-028 owns scalable bounded active-Canon closure') 'Minimum reads preserve provenance and the downstream closure boundary.'
Assert-Requirement 22 ($contract -match 'full commits, constrained patches, permitted creates/deletes' -and $contract -match 'durable write/activation boundary' -and $contract -match 'OPEN = true') 'All gameplay mutation surfaces require durable state checks.'
Assert-Requirement 23 ($contract -match 'pending, failed, or unknown' -or $contract -match 'Pending, failed, or unknown') 'Unresolved persistence cannot authorize ordinary completion.'
Assert-Requirement 24 ($contract -match 'Manual `save`, `save status`, and `retry save` reuse' -and $contract -match 'Lost acknowledgment follows FR-017' -and $contract -match 'Delivery failure can redeliver') 'Receipt retry and response redelivery cannot replay effects.'
Assert-Requirement 25 ($contract -match 'Read-only receipt/status reconciliation' -and $contract -match 'introduces no scheduler') 'Post-yield reads and world autonomy grant no fabricated turn.'
Assert-Requirement 26 ($contract -match 'GM Turn Trace' -and $contract -match 'not chain-of-thought' -and $contract -match 'not implemented error-registry strings') 'Trace and taxonomy are bounded design, not hidden reasoning or implemented codes.'
$scenarioIds = @([regex]::Matches($audit, '(?m)^\| (S\d{2}) \|') | ForEach-Object { $_.Groups[1].Value })
$expectedIds = @(1..40 | ForEach-Object { 'S{0:D2}' -f $_ })
Assert-Requirement 27 (($scenarioIds -join ',') -ceq ($expectedIds -join ',')) 'All 40 unique ordered later acceptance scenarios are present.'
Assert-Requirement 28 ($audit -match 'S05 and S17 are first-class motivating regressions' -and $audit -match 'not replayed live tests in A') 'Both motivating regressions preserve evidence attribution.'
Assert-Requirement 29 ($plan -match 'A Complete \(contract only\); B Complete \(binding only\); C Complete \(trusted interaction control plane\); D-G Pending' -and @([regex]::Matches($plan, '(?m)^### FR-027[A-G] - ')).Count -eq 7 -and $plan -match 'A -> B -> C -> D -> E -> F -> G') 'Bounded A-G checkpoints preserve dependencies and stopping boundaries.'
Assert-Requirement 30 ($roadmap -match '- \[~\] \*\*FR-027' -and $roadmap -match 'Selected implementation objective:\*\* FR-027' -and $roadmap -match '- \[x\] \*\*FR-026' -and $future -match 'FR-028 through FR-036 are unselected') 'Only FR-027 is selected/in progress while FR-026 remains closed.'
Assert-Requirement 31 ($decisions -match 'D-1374' -and $decisions -match 'D-1375' -and $decisions -match 'D-1376' -and $terms -match '## Campaign Session Binding' -and $terms -match '## Player Interaction ID' -and $registry -match 'CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md' -and $index -match 'CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY.md') 'Accepted decisions, terminology and canonical navigation are connected.'
Assert-Requirement 32 ($manifest -notmatch 'CAMPAIGN_BINDING_AND_GAMEPLAY_ENTRY' -and $contract -match 'FR-029 correction-history separation' -and $contract -match 'FR-030 compact persistence/lost-ack APIs' -and $contract -match 'FR-031 end-to-end economics') 'A adds no compiled source or downstream implementation authority.'

if ($failures.Count -gt 0) {
    Write-Output "FR-027A contract harness: FAIL ($($failures.Count) failure(s))"
    $failures | ForEach-Object { Write-Output "- $_" }
    throw 'FR-027A contract harness failed.'
}

Write-Output "FR-027A contract harness: PASS ($passes structural assertions; no runtime acceptance)"
