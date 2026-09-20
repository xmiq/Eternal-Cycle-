# Runtime Rule Kernel

## Purpose

The Runtime Rule Kernel is the smallest mandatory invariant context for ordinary Eternal Cycle operation. It preserves authority and execution safety while specialist rules are retrieved by relevance. It is not the [GM Runtime Procedure](GM_RUNTIME_PROCEDURE.md), a summary of every mechanic, or permission to resolve a specialist claim without the required source. Every `gameplay.resolve` packet requires both the Kernel and the GM Runtime Procedure.

## Kernel Invariants

1. Repository Canon defines reusable rules; accepted design governance constrains those rules; Campaign Canon defines established campaign facts.
2. Conversation memory, generated summaries, compiled indexes, caches, and narration are non-authoritative when an authoritative source exists.
3. The runtime resolves the stable Campaign ID, World/Ruleset identity, Campaign Mode, and configured persistence authority before state-changing play.
4. The runtime reads the smallest dependency-complete rule and campaign Read Set before resolving an action. Missing information remains Unknown.
5. Player intent and current-incarnation agency are preserved. The GM resolves the declared action and world response without inventing player choices.
6. The world continues causally beyond the player. Relevance filtering changes what is loaded, not what can exist or act.
7. Every durable change has one authoritative logical owner. Other records may reference, derive, cache, index, or preserve history from it.
8. A non-empty Affected Set requires the configured canonical persistence transaction, validation, and evidence before ordinary turn completion.
9. Rule retrieval never grants arbitrary SQL, schema selection, campaign access, or permission to cross World/Ruleset, module, visibility, or GM Secret boundaries.
10. A gap follows existing Canonical, Foundation, Provisional, or Unsupported handling. Fluency is not permission to fabricate a rule or fact.
11. A new player gameplay turn begins only with actual new player input. Internal continuations, reasoning, tool work, retries, reads, saves, validation, retrieval, and same-input simulation never authorize an unresolved player choice.

## Retrieval Handoff

After loading this kernel, retrieve only the relevant Eternal Cycle Core, selected World/Ruleset, enabled optional-module, operation, and topic sources. Retrieve Campaign Canon separately through its authoritative persistence path using the same Campaign ID. If the required specialist source cannot fit or cannot be verified, narrow the operation, perform a targeted follow-up retrieval, or stop with an explicit operational failure.

## Related Documents

- [Rule Compilation and Context-Efficient Retrieval](RULE_COMPILATION_AND_RETRIEVAL.md)
- [GM Runtime Procedure](GM_RUNTIME_PROCEDURE.md)
- [AI Runtime Model](../ai/AI_RUNTIME_MODEL.md)
- [Context Assembly and Gameplay Turn Persistence](../ai/CONTEXT_ASSEMBLY_AND_TURN_PERSISTENCE.md)
- [Persistence Authority](../persistence/PERSISTENCE_AUTHORITY.md)
