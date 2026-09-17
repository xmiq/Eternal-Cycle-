# Development Feedback Method

Eternal Cycle uses reference implementations and real deployments to test the portability and truth of its architecture. An implementation symptom does not become a universal requirement merely because it was observed first.

## Feedback Loop

```text
observe real failure
  -> identify the implementation symptom
  -> determine the underlying invariant
  -> classify the responsible layer
  -> fix at that layer
  -> add regression or acceptance evidence
```

The classification has three levels:

1. **Reference implementation only** - provider APIs, SQL dialect, process invocation, environment variables, queue technology, and deployment-specific behavior.
2. **Managed Service contract** - durable work identity, independent status, idempotent initiation, disconnect independence, bounded service policy, authorization semantics, progressive authoritative availability, and safe diagnostics.
3. **Eternal Cycle-wide** - rule authority before resolution, progressive gameplay readiness, release identity and provenance, and repository development governance.

Fixes belong at the narrowest layer that owns the invariant. A reference implementation may prove a portable contract, but it must not make .NET, MCP, SQL Server, T-SQL, Git, a particular filesystem, or one worker mechanism universal.

## Evidence

Automated unit and structural tests prove only their controlled boundaries. Real-client, real-network, and real-datastore acceptance remains explicitly unproven until executed and recorded. A regression report must distinguish implementation, automated proof, deployment proof, and remaining uncertainty.

## Related Documents

- [Repository Conventions](REPOSITORY_CONVENTIONS.md)
- [Future Revisions](FUTURE_REVISIONS.md)
- [Release and Version Provenance](RELEASE_VERSIONING.md)
- [Managed Operations](../docs/persistence/MANAGED_OPERATIONS.md)
