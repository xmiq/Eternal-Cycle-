# GM Runtime Procedure

## Purpose

This is the authoritative minimum operating procedure for an Eternal Cycle Game Master. It is distinct from the [Runtime Rule Kernel](RUNTIME_RULE_KERNEL.md): the Kernel preserves authority and safety invariants, while this procedure orders the work required to resolve a gameplay interaction. Both are mandatory roots of every `gameplay.resolve` Rule Packet.

## Gameplay Interaction Lifecycle

For each actual new player gameplay input, the GM must:

1. identify the trusted Campaign ID, active Rule Release, campaign mode, and current interaction identity;
2. determine the rules and Campaign Canon needed for the declared action;
3. load the smallest dependency-complete Rule Packet and authoritative campaign Read Set;
4. resolve only the player's declared intent, preserving uncertainty and information boundaries;
5. simulate relevant independent world responses and determine the complete Affected Set;
6. when the Affected Set is non-empty, write each durable change through its canonical owner in one idempotent transaction;
7. require canonical validation and read-back evidence before treating durable consequences as completed;
8. refresh derived context only from validated canonical state;
9. narrate the resolved consequences and resulting situation; and
10. when another player-controlled decision is unresolved, stop and wait for actual new player input.

Narration alone never establishes Campaign Canon. If required rules, Canon, persistence, or validation are unavailable, the GM reports the operational boundary and does not guess, silently defer the save, or present the durable consequence as completed.

## One Input, One Interaction

A new player gameplay turn begins only with actual new player input. Assistant continuation, hidden reasoning, tool invocation or result, canonical read, rule retrieval, retry, persistence, validation, and world simulation performed while resolving that input remain inside the same interaction. They do not authorize another player action.

When the GM offers a name, route, response, target, allocation, or other decision owned by the player, it must yield. It may explain options and resolve consequences of a choice the player already made; it may not select an unresolved option through an internal continuation.

## Efficient Execution

The lifecycle is logical, not a requirement for one tool call per step. A Managed service may combine compatible reads, resolution support, persistence, and validation while preserving authority, dependency closure, transaction boundaries, and evidence. Rule and campaign responses should be minimal and lossless: omitted authoritative information remains unknown or pending, never inferred from conversation history.

## Related Documents

- [Context Assembly and Gameplay Turn Persistence](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [AI Play Protocol](../ai/AI_PLAY_PROTOCOL.md)
- [Save Update Protocol](../persistence/SAVE_UPDATE_PROTOCOL.md)
- [Persistence Authority](../persistence/PERSISTENCE_AUTHORITY.md)
