# Release and Version Provenance

This document governs reusable Eternal Cycle release identity, discovery references, and human-readable prerelease metadata. It does not authorize a release or move a repository tag.

## Full Releases

A full release tag such as `v1.0.0` is immutable. Its source commit is permanent historical provenance. Published artifacts and Rule Releases that name it must continue to resolve to the same source identity.

## Release-Candidate Discovery

Eternal Cycle uses one deliberately moving discovery tag for the currently nominated release-candidate head of a target version, such as `v1.1.0-rc`. It is a discovery pointer, not immutable identity.

For a resolved prerelease source, a provider may derive human-readable metadata:

```text
Base Release:       v1.0.0
Discovery Tag:      v1.1.0-rc
Commits Since Base: 47
Display Version:    1.1.0-rc.47
Source Commit:      <full immutable source identity>
```

The exact resolved source commit is authoritative identity. `rc.N` is display metadata only: commit counts may collide after branch or history changes. Moving a discovery tag never changes the provenance of an earlier compiled or published Rule Release.

After the target full release is approved, its full tag becomes immutable. Later development uses the next target's moving discovery tag.

## Provider Boundary

The invariant is provider-neutral: stable full-release identity, a mutable prerelease discovery pointer, immutable resolved provenance, and non-authoritative display metadata. The current Git Rule Source Provider maps those concepts to tags, commit SHAs, and commits-since-base. Another provider may use different primitives if it preserves the same distinctions.

## Governance

Creating or moving a discovery tag and creating a full-release tag are repository-maintainer actions. Runtime source resolution may read those references but does not receive authority to change them. Root `VERSION` changes only through an authorized release process.

## Related Documents

- [Development Roadmap](ROADMAP.md)
- [Repository Conventions](REPOSITORY_CONVENTIONS.md)
- [Managed Rule Publication](../docs/rules/MANAGED_RULE_PUBLICATION.md)
- [Rule Compilation and Context-Efficient Retrieval](../docs/rules/RULE_COMPILATION_AND_RETRIEVAL.md)
