# FR-002, FR-003, FR-008, and FR-009 Clarification Audit

## Scope

This audit records the focused Phase 12 implementation of Reincarnation Candidate Selection, Soul Depth Information Visibility, Soul Weapon Rarity and Equipment Relevance, and World Contact, Travel, and Reincarnation Discretion. It adds no campaign data and does not implement FR-010, FR-011, FR-015, or FR-016.

## FR-002 — Reincarnation Candidate Selection

[Reincarnation](../../docs/soul/REINCARNATION.md) now establishes contextual GM adjudication within existing candidate validity. [Reincarnation Generation](../../docs/gm/REINCARNATION_GENERATION.md) supplies the operating procedure. Prior lives, Final Death, Interlife, Soul and world state, cosmology, embodiment opportunities, consequences, themes, and player wishes may inform judgement; none creates eligibility, guarantees selection, or supplies a deterministic weight. Engineered deaths are not candidate currency.

## FR-003 — Soul Depth Information Visibility

[Soul Depth](../../docs/soul/SOUL_DEPTH.md) now uses hybrid qualitative visibility. Established properties may be stated explicitly. Unknown deeper structure remains unknown until existing signs or justified revealing mechanics support discovery. Player Knowledge and Character Knowledge remain distinct, and no numerical Soul Depth meter was introduced.

## FR-008 — Soul Weapon Rarity and Equipment Relevance

[Weapon Evolution](../../docs/soul-weapons/WEAPON_EVOLUTION.md) now permits deeply developed Soul Weapons to exceed conventional equipment substantially without imposing parity. Present capability still depends on growth, bond, embodiment, Skills, and context. Rarity, individual binding, non-transferability, logistics, and institutional needs preserve ordinary equipment as the world baseline.

## FR-009 — World Contact, Travel, and Reincarnation Discretion

[World Gates and World-Contact Events](../../docs/world-engine/GATES_AND_WORLD_CONTACT.md) separates World Knowledge, World Contact, Travel Route, and Reincarnation Possibility. [World Gate Soul Interactions](../../docs/world-engine/WORLD_GATE_SOUL_INTERACTIONS.md) applies that separation to cross-domain candidates. Contact may end, travel requires a valid route, and candidate possibility remains subject to FR-002 adjudication.

## Integration

The implementation references existing owners rather than duplicating mechanics. Reincarnation owns candidate validity and transition; Soul Depth owns qualitative capacity and disclosure; Weapon Evolution owns Soul Weapon capability; World Gates own contact and transit conditions. Existing decisions D-1237, D-1238, D-1242, D-1243, and D-1244 already authorize the changes, so no duplicate decisions were added.

## Persistence Impact

No schema, migration, save template, or populated campaign record changed. The rules clarify adjudication and presentation using existing persistence boundaries.

## Validation

Repository validation checks the four completed roadmap statuses, canonical invariants, terminology, navigation, relative Markdown links, repository boundaries, and absence of campaign data. The complete diff was reviewed before commit.

## Unresolved Issues

No blocking design question remains within this implementation scope. FR-010, FR-011, the refined remaining scope of FR-015, and FR-016 remain pending and unchanged.
