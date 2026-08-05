# Terminology Audit

## Scope

This Phase 11 audit checks canonical terminology across playable rules, governance, templates, indexes, and operating procedures. It standardizes documentation language only. It does not add a mechanic, change a definition, reinterpret an accepted decision, or resolve a gameplay-balance question.

## Method

1. parse every level-two term heading in [Canonical Terminology](../TERMINOLOGY.md);
2. compare headings after case, whitespace, underscore, and hyphen normalization;
3. compare complete definition bodies for accidental duplication;
4. inspect canonical headings and explicit field labels for near-match capitalization, spacing, and hyphenation;
5. trace high-risk neighboring concepts to their specialist owners and accepted decisions;
6. search active rules for deprecated labels recorded in governance;
7. distinguish named rules terms from ordinary-language uses of the same words;
8. validate all changed links and repository boundaries.

## Results

- Canonical term entries: **1,058**.
- Normalized duplicate headings: **0**.
- Identical definition groups: **0**.
- Case-mismatched canonical headings in rules and templates: **0**.
- Active rules using the deprecated `Weapon Avatar` label after correction: **0**.
- New gameplay terms or changed definitions: **0**.

## Documentation Standard

When text invokes a defined rules concept, it uses the spelling, capitalization, spacing, and hyphenation of that term's glossary heading. Plural forms retain significant capitalization. A lowercase common noun remains valid when the sentence is not naming the rules concept. This distinction avoids turning every use of words such as soul, title, state, trend, research, canon, or manifestation into a mechanical claim.

Historical governance may name a deprecated label when documenting why it was replaced. Active rules, templates, indexes, and procedures use the accepted replacement.

The repository intentionally preserves the exact glossary spelling of each term rather than imposing one regional spelling scheme. For example, `Practised Reliability`, `Professional Judgment`, `Specialisation`, and `Over-Specialization` keep their established canonical forms.

## High-Risk Distinctions Reviewed

| Concepts | Owning distinction | Result |
| --- | --- | --- |
| Soul Gate / World Gate | Soul Gates are scoped Soul Space connections; World Gates are world-contact interfaces. | Distinct and consistent. |
| Soul Avatar / Echo Delegation / Weapon Manifestation | Whole-soul synthesis, bounded transfer to one Echo, and external Weapon Soul expression remain separate. | One deprecated active-rule label corrected. |
| Soul Echo / Weapon Echo | Past-incarnation imprint and Weapon Soul memory structure remain separate persons, provenance, and access models. | Distinct and consistent. |
| Soul Title / social title, reputation, class, Skill, blessing | Persistent soul identity remains separate from offices, recognition, capabilities, and external grants. | One template hyphenation corrected. |
| Monster Evolution / Skill Evolution / Class Evolution / Weapon Evolution | Biological form, capability structure, human framework revision, and bonded weapon expression retain separate owners. | Distinct and consistent. |
| Repository Canon / Campaign Canon / Historical Record / Current Campaign State / Current Session / Current Narration | Authority layers retain exact order and named capitalization when invoked formally. | Formal labels corrected in GM, AI, registry, and template text. |
| Magical Learning Role / State Trend / Competing Theory | Existing singular glossary terms now appear explicitly in their owning rules. | Owner wording aligned; definitions unchanged. |

## Corrections

- `Magical Learning Role` now names the role list owned by Magical Schools.
- `State Trend` now names the corresponding World-State Claim field, section, and blank record label.
- `Competing Theory` now appears explicitly in the Research Engine while the collective section remains plural.
- `Weapon Manifestation` replaces the deprecated active-rule use of `Weapon Avatar`.
- formal persistence-authority labels now use canonical capitalization where they identify authority layers.
- stray `Soul-Title` and `Weapon-Soul` compounds now use the canonical open forms.
- the completed Phase 1 roadmap item now records the accepted Soul Avatar, Echo Delegation, and Weapon Manifestation distinction.

## Non-Changes

- Ordinary-language lowercase uses remain where they do not invoke a named rules concept.
- Historical decision text explaining a deprecated term remains intact.
- No term was merged, removed, redefined, or promoted into a universal mechanic.
- No accepted decision or unresolved question required amendment.

## Open Questions

The non-blocking questions in [Unresolved Questions](../UNRESOLVED_QUESTIONS.md) and evidence candidates in [Future Revisions](../FUTURE_REVISIONS.md) remain unchanged. None blocks this documentation audit.

## Validation

- Repository-wide Markdown files: **193**.
- Relative Markdown links: **5,755** checked, zero broken.
- Markdown anchors: **111** checked, zero broken.
- Canonical term entries: **1,058**, with zero normalized duplicate headings and zero identical definition groups.
- Active-rule `Weapon Avatar`, `Soul-Title`, and `Weapon-Soul` matches: zero.
- Repository boundaries and Phase 11 documentation-only scope remain intact.

## Result

**Pass**, provided repository-wide link validation and the complete staged diff remain clean at commit time.

## Related Documents

- [Canonical Terminology](../TERMINOLOGY.md)
- [Design Decisions](../DECISIONS.md)
- [Repository Conventions](../REPOSITORY_CONVENTIONS.md)
- [Canonical Document Registry](../../docs/DOCUMENT_REGISTRY.md)
- [Rule Consistency Audit](RULE_CONSISTENCY_AUDIT.md)
- [Development Roadmap](../ROADMAP.md)
