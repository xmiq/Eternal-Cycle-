# AI Game Master Operating Procedures

These documents define how an AI Game Master operates the completed Eternal Cycle rules, GM framework, and Campaign Persistence Engine. The family contains implementation-neutral architecture and procedures plus explicitly classified execution profiles and persistence adapters. None is an independent source of mechanics or campaign truth.

A human GM may use the same procedures. Automation changes retrieval and execution methods; it does not change authority, responsibility, player agency, or the standard of evidence.

Repository-wide ownership, dependency, extension, and consumer metadata is maintained in the [Canonical Document Registry](../DOCUMENT_REGISTRY.md#ai-game-master-operations).

## Document Control

- **Owner:** this index owns AI-procedure reading order and scope only
- **Primary authorities:** [Game Master Framework](../gm/GAME_MASTER_FRAMEWORK.md), [Game Master Responsibilities](../gm/GM_RESPONSIBILITIES.md), and [Campaign Persistence Integration](../persistence/CAMPAIGN_PERSISTENCE_INTEGRATION.md)
- **Dependencies:** Repository Canon, Campaign Canon, external Campaign Record, Truth Layers, required Read Sets, Save Updates, and validation
- **Extensions:** interface-specific retrieval, presentation, storage, and automation may implement these procedures without changing them
- **Consumers:** AI GMs, human GMs using automated assistants, campaign interfaces, state-maintenance tools, and validation tools
- **Repository boundary:** no prompt, model configuration, credential, transcript, save, current character, live world state, hidden campaign fact, or play history belongs here

## Reading Order

1. [AI Runtime Model](AI_RUNTIME_MODEL.md) - implementation-neutral runtime layers, authority boundaries, boot and action flows, adapter composition, and extension rules.
2. [AI Capabilities and Limitations](AI_CAPABILITIES_AND_LIMITATIONS.md) - non-authoritative memory, finite context, tool and write limits, information protection, validation limits, and failure handling.
3. [AI GM Workflow](AI_GM_WORKFLOW.md) - end-to-end operating cycle and stop conditions.
4. [AI Session Start](AI_SESSION_START.md) - version, authority, state, visibility, and readiness checks before play.
5. [AI Play Protocol](AI_PLAY_PROTOCOL.md) - intent, retrieval, adjudication, narration, consequence, and interaction-boundary procedure.
6. [AI Save Protocol](AI_SAVE_PROTOCOL.md) - operational use of the canonical Save Update Protocol, including write limitations and recovery.
7. [AI Checklist](AI_CHECKLIST.md) - compact gates for session start, adjudication, narration, saving, correction, and handoff.

## Authority Boundary

These documents sequence existing owners. They do not replace them.

| Question | Canonical owner |
| --- | --- |
| What rules and mechanics exist? | the applicable gameplay document under `docs/` together with accepted governance |
| What duties and limits apply to the GM? | [GM rules](../gm/README.md) |
| What is currently true in one campaign? | the external Campaign Record under the [Campaign State Model](../persistence/CAMPAIGN_STATE_MODEL.md) |
| Which information may reach which observer? | [Truth Layers](../persistence/TRUTH_LAYERS.md) |
| How is an interaction persisted? | [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md) |
| How are contradictions repaired? | [Continuity Resolution](../persistence/CONTINUITY_RESOLUTION.md) |
| How are temporary rulings handled? | [Alpha Playtest Rules](../gm/ALPHA_PLAYTEST_RULES.md) |

An AI-generated summary, cached context, retrieval result, prediction, or narration has no authority merely because it is recent or fluent. It remains a view, proposal, or lower-authority record until the proper owner establishes or activates it.

## Operating Guarantees

An AI GM must:

- search the repository before inventing a material rule;
- load the Save Index and the material Read Set before adjudication;
- distinguish Repository Canon, Campaign Canon, established state, history, knowledge, Research, theories, rumours, Secrets, and Meta;
- preserve player ownership of deliberate player-character intent;
- represent NPCs through their own knowledge, motives, capability, and constraints;
- identify uncertainty, missing sources, conflicts, and Provisional Rules explicitly;
- maintain causal continuity across scenes, sessions, deaths, Reincarnations, Time Skips, Ages, and World Resets;
- complete the canonical Save Update process after each completed Gameplay Interaction;
- state honestly when it cannot read, write, validate, or activate campaign state;
- leave the last valid Save Point authoritative after interruption or failure.

## Procedure Status

These are canonical operating procedures, but they grant no gameplay effect. Following a checklist cannot establish success, progression, knowledge, relationship change, world change, or a saved state. Each such claim still requires its factual owner, mechanical owner, evidence, authority, and persistence route.

## Related Documents

- [Documentation Map](../README.md)
- [GM Rules Index](../gm/README.md)
- [Campaign Persistence Engine Index](../persistence/README.md)
- [Template Index](../../templates/README.md)
