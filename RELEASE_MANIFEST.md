# Release Manifest

| Field | Value |
| --- | --- |
| Product | Eternal Cycle |
| Version | 1.0.0 |
| Release name | Release 1 |
| Release date | 2026-08-25 |
| Release commit | Commit referenced by annotated tag `v1.0.0` |
| Validation | Repository validation and FR-011 regression harness pass |
| Roadmap | Phase 12 complete; Phase 13 active |
| Compatibility | Existing campaigns use audited Backup, Merge, Validation, and activation; no automatic campaign rewrite |
| Artifact | `Eternal Cycle v1.0.0.zip` |

The annotated `v1.0.0` tag is the authoritative, non-self-referential identifier for the release commit. The artifact is generated from that tag and excludes Git metadata, caches, credentials, temporary files, and campaign state.

## Scope

The release contains the reusable rules engine, design governance, GM and AI procedures, logical persistence contracts, validators, and blank templates. It does not contain a campaign, populated database, private locator, credential, or live world state.

## Related Documents

- [Release 1 Notes](RELEASE_NOTES.md)
- [Release 1 Audit](design/audits/RELEASE_1_AUDIT.md)
- [Changelog](CHANGELOG.md)
- [Development Roadmap](design/ROADMAP.md)
