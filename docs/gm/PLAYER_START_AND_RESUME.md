# Player Start and Resume

## Purpose

This player-facing procedure keeps Eternal Cycle infrastructure behind the game interface while preserving explicit consent, stable campaign identity, and truthful readiness.

## Document Control

- **Owner:** player intent for starting, selecting, and resuming campaigns
- **Dependencies:** [Campaign Bootstrap](CAMPAIGN_BOOTSTRAP.md), [Running Eternal Cycle](../persistence/RUNNING_ETERNAL_CYCLE.md), and [Campaign Persistence Integration](../persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md)
- **Extensions:** runtime-specific presentation may phrase prompts differently without changing consent or authority
- **Consumers:** players, human GMs, AI GMs, and startup interfaces
- **Repository boundary:** examples are generic; no real Campaign ID or campaign state belongs here

## Normal Commands

Use ordinary intent:

> Start a new Eternal Cycle game.

> Continue my Eternal Cycle game.

Players normally do not provide database names, schemas, source paths, Rule Release IDs, Campaign IDs, MCP tool names, or migration filenames.

## First Run

The GM or runtime checks capabilities and readiness before state-changing play. If bounded EC-owned setup is needed, it explains what is missing and asks permission before invoking a separately authorized administrative operation.

A suitable prompt is:

> Eternal Cycle is not initialized for this persistence service. I can create or update only Eternal Cycle-owned structures and then configure an approved rules source. Would you like me to continue?

Approval is specific to that administrative action. It is not permission for arbitrary SQL, source access, or unrelated database changes.

## New Game

When no campaign exists, the runtime offers the authorized campaign-creation path and asks for a meaningful campaign name. It creates a stable internal Campaign ID, keeps that identifier backstage, then performs normal Campaign Bootstrap.

## Resume

- **No campaign:** explain that no resumable campaign exists and offer new-game creation.
- **One suitable campaign:** resume it without asking the player to type its internal ID.
- **Several campaigns:** show meaningful names and short descriptions and ask the player to choose.

Campaign isolation, routing, and concurrency still use the stable Campaign ID internally.

## Setup and Failure Messages

Expected setup conditions are not fictional events. They are concise operational messages with an actionable next step. Missing Canon is never filled from model memory merely to continue.

When the selected persistence authority cannot validate a required operation, state-changing play stops under the existing completion gate. Successful routine infrastructure details remain backstage.

## Related Documents

- [Campaign Bootstrap](CAMPAIGN_BOOTSTRAP.md)
- [Running Eternal Cycle](../persistence/RUNNING_ETERNAL_CYCLE.md)
- [AI Game Master Workflow](../ai/AI_GM_WORKFLOW.md)
