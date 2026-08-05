# Internal-Link and Navigation Audit

## Scope

This Phase 11 audit checks whether every repository document can be found, reached, and routed to its correct owner without relying on chat history. It covers local Markdown targets, anchors, directory indexes, canonical registry coverage, template coverage, contributor-role navigation, orphan documents, roadmap declarations, and the repository boundary. It does not add or reinterpret gameplay rules.

## Method

1. build a graph from every relative Markdown link in the repository;
2. resolve each target against its source document and reject links that escape the repository;
3. compare Markdown anchors with the target document's generated heading slugs;
4. require a family index for every direct subdirectory under `docs/`;
5. compare every canonical document against the [Canonical Document Registry](../../docs/DOCUMENT_REGISTRY.md);
6. compare every family document against its local index and the family indexes against the [Canonical Rules Map](../../docs/README.md);
7. compare every template against both the [Template Index](../../templates/README.md) and [Template Coverage Map](../../templates/TEMPLATE_COVERAGE.md);
8. compare every design, audit, and contributor-role document against its corresponding index;
9. find Markdown documents with no inbound repository link, excluding the root entry point;
10. verify roadmap declarations, terminology-heading uniqueness, and forbidden campaign-data directories.

The checks are implemented in [`tools/validate_repository.ps1`](../../tools/validate_repository.ps1) so later documentation changes can repeat the same audit rather than relying on a one-time report.

## Navigation Model

| Entry point | Responsibility |
| --- | --- |
| [Root README](../../README.md) | Repository purpose, subsystem discovery, contributor reading order, status, and validation entry point |
| [Design and Governance Index](../README.md) | Roadmap, decisions, terminology, conventions, open questions, future evidence, working notes, and audit discovery |
| [Canonical Rules Map](../../docs/README.md) | Canonical family discovery |
| [Canonical Document Registry](../../docs/DOCUMENT_REGISTRY.md) | Claim-owner, dependency, extension, and consumer routing for every canonical document |
| Family indexes under `docs/` | Local reading order and specialist-document discovery |
| [Template Index](../../templates/README.md) | Blank record-contract discovery and use guidance |
| [Template Coverage Map](../../templates/TEMPLATE_COVERAGE.md) | Completed-system and persistence-module coverage |
| [Agent Role Index](../../agents/README.md) | Contributor-role discovery subordinate to `AGENTS.md` |
| [Repository Audit Index](README.md) | Standardization-audit discovery and scope boundary |

Indexes own navigation and reading order only. They do not become alternate mechanical owners, and a successful link does not prove that the linked claim is correct.

## Initial Findings

- The root README was the expected graph entry point.
- Five contributor-role files had no inbound Markdown link because their index used code literals.
- The Core family was discoverable through the canonical map but lacked the family-index structure used by every other canonical family.
- Design governance and repository audits had no dedicated indexes.
- Link and coverage checks existed only as one-time audit commands rather than a reusable repository tool.
- Existing relative links and anchors resolved successfully before correction.

## Corrections

- contributor-role literals became relative Markdown links;
- the Core family gained a bounded index and registry entry;
- design-governance and audit indexes now provide explicit reading order and authority boundaries;
- the root README now links all major repository maps and the validation command;
- repository conventions and agent workflow now require the reusable validator;
- the validator enforces link, anchor, index, registry, template, orphan, roadmap, terminology-heading, and campaign-directory checks.

## Ownership and Scope

The validator tests repository structure; it cannot prove gameplay correctness, replace specialist review, establish campaign truth, or amend design governance. A passing result means the checked documentation contracts are structurally satisfied at that revision. Future rules changes still require the roadmap, ownership, decision, terminology, and full-diff workflow.

## Validation Results

- Repository validation: **pass**.
- Markdown files: **197**.
- Relative links checked: **5,804**, zero broken or repository-escaping targets.
- Markdown anchors checked: **112**, zero broken.
- Canonical documents indexed: **143**.
- Documentation family indexes: **12**.
- Templates indexed: **31**.
- Agent roles indexed: **5**.
- Canonical terminology headings checked: **1,058**, with zero normalized duplicates.
- Orphaned Markdown documents: **0**, excluding the root README entry point.
- Forbidden campaign-data directories: **0**.

## Open Questions

No blocking question was created. Existing non-blocking questions in [Unresolved Questions](../UNRESOLVED_QUESTIONS.md) and evidence candidates in [Future Revisions](../FUTURE_REVISIONS.md) remain unchanged.

## Result

**Pass**, provided the complete staged diff remains clean at commit time.

## Related Documents

- [Repository Conventions](../REPOSITORY_CONVENTIONS.md)
- [Canonical Terminology](../TERMINOLOGY.md)
- [Cross-Reference and Ownership Audit](CROSS_REFERENCE_AND_OWNERSHIP_AUDIT.md)
- [Rule Consistency Audit](RULE_CONSISTENCY_AUDIT.md)
- [Terminology Audit](TERMINOLOGY_AUDIT.md)
- [Development Roadmap](../ROADMAP.md)
