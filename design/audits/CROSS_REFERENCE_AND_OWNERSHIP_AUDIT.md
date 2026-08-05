# Cross-Reference and Ownership Audit

## Scope

This Phase 11 audit covers every Markdown document under `docs/`, all family indexes, repository navigation, and the boundary among canonical rules, design governance, procedures, templates, and campaign-external records.

## Method

1. inventory every document under `docs/`;
2. identify the narrowest class of claims each document owns;
3. group documents by shared dependencies, permitted extensions, and consumers;
4. record those interfaces in the [Canonical Document Registry](../../docs/DOCUMENT_REGISTRY.md);
5. verify that every inventoried document appears exactly once as a registry subject;
6. inspect navigation and known stale Phase 11 references;
7. validate relative links, anchors, repository boundaries, and roadmap scope.

## Findings

### Resolved: Uneven Interface Metadata

Canonical documents usually stated their local ownership in prose, related-document lists, or family indexes, but formatting varied and dependencies, extensions, and consumers were rarely all explicit. Adding repeated headers to every rule document would have created substantial duplication and future drift.

The central registry now gives every document one explicit claim owner and an inherited family interface. Refinements record narrower handoffs where needed. Family indexes remain the source of reading order and specialist claim routing.

### Resolved: Registry Discoverability

The registry is linked from root and canonical navigation. Family indexes identify it as the repository-wide metadata source, allowing a reader to move from system reading order to cross-system ownership without relying on chat history.

### Resolved: Stale Phase 11 Navigation

References that still described already completed templates or the Future Revisions task as pending were corrected. These were documentation-status defects, not gameplay-rule changes.

### Preserved: Distributed Canonical Ownership

No new super-owner was created. The registry routes claims to existing specialist documents and cannot override canonical rules or accepted design governance. Shared examples remain illustrative, GM tools remain procedural, templates remain blank contracts, and populated records remain external.

## Coverage Result

- Every Markdown document under `docs/` is represented in the registry, including indexes and AI operating procedures.
- Every document has an explicit claim owner.
- Every document inherits explicit dependencies, extensions, and consumers from its family.
- Cross-family refinements identify major specialist handoffs without copying mechanics.
- No orphaned canonical document or unowned major claim family was found.

## Scope Boundary

This audit did not judge whether rule statements agree in every detail, normalize terminology, test every link, or assess balance. Those are separate roadmap tasks. It changed navigation and ownership metadata only and introduced no gameplay mechanic, canonical decision, or campaign data.

## Result

**Pass**, provided repository-wide link and coverage validation remain clean at commit time.

## Related Documents

- [Canonical Document Registry](../../docs/DOCUMENT_REGISTRY.md)
- [Canonical Rules Map](../../docs/README.md)
- [Repository Conventions](../REPOSITORY_CONVENTIONS.md)
- [Development Roadmap](../ROADMAP.md)
- [Template Coverage Map](../../templates/TEMPLATE_COVERAGE.md)
