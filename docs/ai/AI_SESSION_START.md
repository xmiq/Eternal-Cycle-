# AI Session Start

## Purpose

This procedure establishes a valid, bounded starting context before an AI GM presents or adjudicates play. It does not create a campaign, assume missing facts, or replace the Campaign State Model's required Read Set.

## Document Control

- **Owner:** this document owns the AI session-start sequence
- **Primary authorities:** [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md) and [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md)
- **Dependencies:** Save Index, Campaign Canon, Current Session, active versions, visibility, relevant records, and latest validation
- **Extensions:** storage-specific loading, retrieval indexes, participant interfaces, and session handoff tools
- **Consumers:** AI GM workflow, supervising GM, session interfaces, and load validation
- **Repository boundary:** no populated session brief, participant identity, campaign title, current location, character state, or Secret belongs here

## Entry Conditions

Before this procedure begins, the operator needs authorized access to:

- the rules repository or an identified Repository Version;
- the external Campaign Record's Save Index;
- the configured Codex Version and deployment identity when the campaign adopts a [GM Living Codex](../gm-living-codex/README.md);
- the participant-facing channel and its visibility boundary;
- any open Current Session or handoff state.

If no campaign exists, campaign creation occurs through an external campaign process using the canonical templates. This procedure does not populate those templates inside the repository.

## Start Procedure

### 1. Confirm identity and versions

Read the Save Index and confirm:

- Campaign ID and continuity boundary;
- Repository Version, Campaign Version, Persistence Model Version, and Storage Format Version;
- active Rules Profile;
- last confirmed Save Point;
- Current Session status;
- latest validation outcome and warnings;
- open migration, recovery, or continuity cases.

Do not collapse distinct versions into one label. A repository update does not silently migrate a campaign.

### 2. Confirm access and visibility

Identify the current interface's authorized audience. Determine which Truth Layers and record partitions it may read, summarize, or write. Use least-necessary access.

A player-facing channel must not receive GM Secrets, other characters' private Knowledge Views, hidden Research, or protected migration diagnostics merely because the AI can access them elsewhere.

### 3. Check load safety

Do not begin ordinary play when:

- the Save Index marks load as blocked or unknown;
- a required migration is incomplete;
- a blocking validation finding is open;
- an interrupted transaction lacks a safe recovery position;
- the active Campaign Version differs from the requested parent without resolution;
- protected information cannot be kept separate.

Warnings may be carried only within their authorized scope and disposition.

### 4. Load the Current Session boundary

Read unresolved player intent, prior Session Deltas, open adjudications, pending corrections, and the last presented but not yet resolved situation. Distinguish staged changes from activated state.

A transcript or recent message may help locate the boundary, but it does not silently override the Save Index or Campaign State Graph.

### 5. Build the initial Read Set

Always read:

1. Save Index;
2. Current Session;
3. the canonical owner for the immediate claim or situation.

Then load as relevant:

- Player State and current embodiment;
- Soul and Incarnation records;
- Relationships and Companions;
- Species, Skills, Development, Classes, Soul Weapons, and Magic;
- current Location, World, Factions, Infrastructure, and Inventory custody;
- Projects, Research, Mysteries, and Knowledge Views;
- Timeline, Campaign History, Pending Consequences, and Review Points.

Follow Typed References until additional records cannot materially change the next claim, consequence, uncertainty, disclosure, or choice. Loading everything is not a substitute for identifying relevance.

### 6. Reconcile freshness and status

For every material claim, confirm owner, effective time, source, status, Truth Layer, Persistence Level, version, and supersession. Classify missing or stale material honestly as `Unknown`, `Not Yet Verified`, `Estimated`, `Disputed`, or `Requires Source Recovery` as appropriate.

Never infer current state solely from the last mention in narration.

### 7. Advance to the present

Resolve only already-caused elapsed-time effects needed to reach the session boundary. Apply World Engine simulation, actor autonomy, ongoing costs, expiring conditions, and Pending Consequences through their owners. Pause at unresolved agency, uncertainty, or required Save Update boundaries.

### 8. Build the working session view

Create an Ephemeral or Derived View containing only what the operator needs:

- active versions and Save Point;
- current time, place, and embodiment;
- relevant actors and their available knowledge;
- player-facing known facts and material uncertainties;
- active pressures, Projects, Mysteries, obligations, and Pending Consequences;
- relevant canonical owners;
- open Provisional Rules, warnings, and stop conditions.

This view is navigation, not authority. It must carry freshness and source references and must be discarded or refreshed after material state change.

### 9. Declare readiness

The operator may enter play only after it can state internally or to the supervising interface:

- which versions and Save Point govern;
- which Read Set was loaded;
- which visibility boundary applies;
- which material gaps, warnings, or Provisional Rules remain;
- whether the state is Ready, Ready with declared limits, or blocked.

## Resume and Handoff Cases

For a shared-GM or interrupted-session handoff, verify the handoff against the active records. Do not accept a handoff summary as a higher authority than the Save Index, Timeline, Relationships, or current owner records. Preserve unresolved player intent without deciding it.

For a new AI implementation taking over an existing campaign, treat platform memory as optional evidence. Load the Campaign Record through the same procedure used by any other GM.

## Safeguards

- No session starts from model memory alone.
- Missing information remains missing.
- A concise working context does not delete omitted state.
- Retrieval scope does not alter visibility or authority.
- Opening narration cannot repair a stale or contradictory save.
- Session preparation cannot preselect player choices or encounter outcomes.

## Related Documents

- [AI GM Workflow](AI_GM_WORKFLOW.md)
- [Save Index Template](../../templates/SAVE_INDEX_TEMPLATE.md)
- [Truth Layers](../persistence/TRUTH_LAYERS.md)
- [Persistence Validation](../persistence/PERSISTENCE_VALIDATION.md)
