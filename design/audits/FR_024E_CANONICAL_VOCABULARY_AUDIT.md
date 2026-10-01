# FR-024E Canonical Vocabulary Curation Audit

## Scope and Authority

FR-024E began on `main` at `2ccfe9bcd0102cb72a35e678e581c5dfee06db5b`, synchronized with origin. FR-022/023 were closed; FR-024A-D were complete. E curates reviewed navigation metadata and tests beneath the existing [FR-024 objective](../FR_024_EXECUTION_PLAN.md); F remains pending. This is not FR-024 closure or a release.

All ten manifest-declared Rule Sources were read in full before curation, and their 154 source-bound snippet identities were inspected. The [canonical manifest](../../docs/rules/rule-source-manifest.json) now contains 62 concepts, 67 aliases, 72 phrases, and 122 exact bindings. No normative source prose, compiler implementation, format-1 contract, Managed behavior, schema, or migration changed.

## Curation Convention

- Concepts distinguish gameplay resolution/agency, Repository Canon/Campaign Canon, current state/history, compilation context/retrieval, Development/experience, current incarnation/retained potential, evolution routes/readiness/transition, and world continuation. Evolution convergence, divergence, regression, and traps remain separate.
- Alternatives are useful reviewed wording, not a synonym dump. Phrases stay intact; A's NFC, invariant lowercase, and whitespace normalization is reused without stemming or generation.
- Canonical terms carry weight 900, phrases 800, aliases 700. The deliberately generic aliases `conflict`, `preparation`, and `save` carry 600. These are reviewed importance values, not runtime scores.
- Bindings use the narrowest supported existing snippet. Only the heading-free, one-snippet GM Host Bootstrap uses Source scope; all other 121 bindings use Snippet scope. Every definition, alternative, and binding has concise review rationale.
- Combat/fighting terms locate contextual capability and victory rules, not an invented complete combat system. Experience points locates experience/anti-automatic-award safeguards, not a new universal XP pool. Compiled rules locates delivered context, not compiler implementation. No provider rule-preparation concept was fabricated from evolution preparation.
- Reviewed `conflict` ambiguity is preserved between combat context and claim conflict. Stronger phrases and disjoint target sets maintain the distinction; no winner or rank is assigned.

Two fixture-review corrections added precise associations supported by actual prose: combat context on contextual capability assessment, and rule gaps on the mandatory read gate. Initial draft convergence/regression wording was separated into distinct concepts after semantic review. No rules were edited to accommodate these associations.

## Before and After

The comparison uses unchanged explicit regression identities: source scheme `git-commit`, value `d5028af32cf16fe48878ffef0301867175f98f08`, compiler `eternal-cycle-dotnet` version `1.0.0`, contract `1`. Keeping them fixed isolates the metadata change. This fixture value is not a claim that the curated working tree existed at that historical commit, nor release provenance for E. Production callers must supply the real immutable source identity.

| Metric | Before E | After E |
| --- | ---: | ---: |
| Sources / snippets | 10 / 154 | 10 / 154 |
| Concepts / canonical terms | 0 / 0 | 62 / 62 |
| Aliases / phrases | 0 / 0 | 67 / 72 |
| Bindings | 0 | 122 |
| Effective associations / retained origins | 0 / 0 | 773 / 773 |
| Unique normalized terms | 0 | 200 |
| Covered / uncovered snippets | 0 / 154 | 122 / 32 |
| Coverage | 0/154 | 122/154 (79.22%) |
| Concepts with / without aliases | 0 / 0 | 62 / 0 |
| Concepts with / without targets | 0 / 0 | 62 / 0 |
| Multi-origin / source-only associations | 0 / 0 | 0 / 3 |
| Maximum associations per snippet | 0 | 24 |
| Maximum target snippets per term | 0 | 10 |
| Errors / warnings / information | 0 / 0 / 2 | 0 / 3 / 105 |
| Serialized artifact bytes | 223,929 | 275,622 |

Average associations per covered snippet is exactly `773/122`; average target snippets per term is `773/200`. More metadata is not inherently better: precise positive/negative topology and reviewed meaning are the acceptance evidence.

| Identity | Before E | After E |
| --- | --- | --- |
| Semantic artifact digest | `4B583A7904CE514B56CF3B497648DB5D778FC04DF2285D30A2E03823A32EA6A4` | `56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B` |
| Serialized artifact byte SHA-256 | `802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC` | `A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6` |
| Serialized audit byte SHA-256 | `E465014C8924AA55191DF7F509F27C3CE0642EB1064CEBBBD7985F87B7C52486` (D default policy) | `A54B088E9FDBDEB6889F8C151EFC9F5C5D04E7584F4EC7E22F4558FE5FDABEC6` (E review policy) |

Exact manifest provenance changes with the vocabulary; all ten original source hashes, source selectors/dependencies, snippet IDs, text, content hashes, token estimates, and ordering remain unchanged. Semantic digest and file byte hash are separate identities.

## Review Dispositions

The explicit [review policy](../../examples/tooling/rules-compiler-dotnet/fixtures/canonical-vocabulary/audit-policy.json) uses D's default breadth/token thresholds and twelve generic candidate terms. All warnings are retained, not suppressed:

- `conflict`: nine targets, four combat-context and five claim-conflict snippets, two concepts with differing target sets; weight 600 and stronger phrases distinguish meanings.
- `preparation`: three evolution-readiness targets only; neither generic combat context nor provider rule preparation inherits it.
- `save`: the exact player command reaches only `manual-persistence-commands`; automatic saving, status, and retry use distinct concepts/phrases.

There are zero broad-term, high-fan-out, unused-concept, or only-generic warnings. Default empty generic policy reports zero warnings; E deliberately supplies policy and tests all three dispositions.

All 105 information findings were reviewed by topology category: 99 multi-source terms connect the same defined concept across participating rule owners; one shared-term ambiguity and one different-target-set finding describe `conflict`; one Source binding and one inherited-only finding describe the sole bootstrap snippet; one partial-coverage finding records the intentional gaps; one large-snippet marker concerns `core-reincarnation#transition-procedure`. The latter is already an FR-023 boundary observation; E does not split or rewrite it. The bootstrap reaches one snippet, so there is no source-wide high fan-out.

The [post-E checkpoint](../../examples/tooling/rules-compiler-dotnet/fixtures/canonical-vocabulary/after-e.json) enumerates all 32 uncovered IDs. Nine root/title snippets and eight related-document lists remain navigation, not keyword targets. Fifteen remaining sections are context acceptance/control/regression/safeguard/storage guidance (5); Development scope/interactions (2); evolution scope/examples (2); authority safeguards/scope/layer overview/upward proposals/examples (5); and reincarnation examples (1). Some contain meaningful contracts, but stronger owner/procedure targets already carry those concepts. Their residual gaps are deliberately accepted rather than attaching generic words to duplicate overviews, safeguards, or examples. Existing source/dependency closure and always-include behavior are unchanged; this does not promise uncovered sections are independently searchable or dispensable. Future review may justify narrower bindings with evidence.

## Fixtures and Reproducibility

The [quality expectations](../../examples/tooling/rules-compiler-dotnet/fixtures/canonical-vocabulary/quality-expectations.json) have 65 stable cases, 115 positive targets, and 130 negative targets across every concept, plus precision cases for combat/Canon conflict overlap, campaign facts, and XP versus readiness. They assert exact concept/normalized-term associations on existing stable snippet IDs, never ranking positions. All referenced positive and negative targets must exist.

The [before-E fixture](../../examples/tooling/rules-compiler-dotnet/fixtures/canonical-vocabulary/before-e.json) captures the exact historical manifest and ten source byte sequences as Base64, with their hashes. Test-only payload preparation verifies canonical prose against those captured bytes ignoring checkout line-ending differences, then materializes the captured sources with either the exact old manifest or the current reviewed manifest in LF form. This protects the historical mixed-newline evidence from Git/platform checkout conversions, does not normalize compiler provenance, and rejects source prose changes. Historical A-D regression tests now use this frozen payload rather than pretending the live corpus is still vocabulary-free.

Portable and actual CLI tests prove repeated compile/audit bytes, source-root relocation, unrelated CWD, culture changes, every association's reviewed origin/weight/rationale, and library/CLI equality for both artifact and report. No generated vocabulary, network, Git acquisition, SQL, MCP, or campaign input participates in compilation.

## Validation and Boundaries

Both portable and CLI Release builds passed with zero warnings/errors. Executed tests passed with no failures/skips: 76 E portable and 4 E CLI cases; 288 complete portable, 58 complete CLI/process, and 181 complete Managed MCP cases (527 total suite cases); focused A/B/C/D portable counts 52/37/25/28; 70 FR-023 regressions; 16 FR-022 contract cases and one portable dependency-boundary case. Focused counts are subsets, not additional suite totals. Historical and curated byte checkpoints both pass.

Full repository validation passed all nine structural harnesses (333 assertions, including 32 FR-022), 296 Markdown files, 7,293 relative links, 198 anchors, 190 canonical documents, 15 family indexes, 43 templates, five agent roles, 1,273 terms, 208 roadmap tasks, and 36 Future Revision entries. There are zero blocking questions, orphan documents, and forbidden campaign-data directories. Governance, authority, schema/contract, navigation, deterministic distribution boundaries, and whitespace/scope checks pass. No archive was produced. The unsigned validator ran with an invocation-only policy override, not a machine policy change.

The initial test dependency restore was blocked by sandbox network restrictions; an authorized NuGet restore succeeded, and all reported tests were subsequently executed. No substantive curation/contract validation failed; fixture refinements and captured-byte portability checks are described above. Git's existing checkout newline notices are informational, not whitespace errors.

Real deployment acceptance and runtime retrieval quality/ranking are not claimed by exact offline association fixtures. FR-024F remains unstarted; FR-025/026 and release work remain unselected. No new dependency, glossary term, migration, VERSION change, tag movement, archive, or release is required. The owner-authorized workflow is one focused validated E commit followed by a normal `main` push, never a force-push.
