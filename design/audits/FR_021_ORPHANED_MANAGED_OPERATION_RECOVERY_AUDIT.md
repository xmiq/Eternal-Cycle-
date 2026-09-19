# FR-021 Orphaned Managed Operation Recovery Audit

## Scope

This audit records owner-authorized corrective maintenance to completed FR-021 after a real PC shutdown left initial Rule Publication reported as `Running` at `AcquireSource`. It creates no new Future Revision, changes no gameplay mechanic, leaves `VERSION` at `1.0.0`, and does not move release or release-candidate tags.

## Observed Defect

The durable operation and diagnostic identities survived shutdown. Diagnostics correctly recorded `MANAGED_OPERATION_INTERRUPTED`, but the operation row could remain `Running`. Active-operation deduplication therefore reused the row while no durable evidence proved that a worker still owned it. The original recovery query changed every `Running` row during one startup pass, but the data model itself had no bounded ownership claim, heartbeat, attempt evidence, or stale-writer guard.

This was a Managed Operation lifecycle defect. The known interruption cause was host shutdown; no Git, GitHub, network, repository-size, or provider-latency cause was inferred.

## Repair

The implementation-neutral contract now requires bounded renewable execution ownership or an equivalent provider mechanism. The SQL Server reference adds migration `009_managed_operation_execution_leases` with nullable owner and expiry fields plus an execution-attempt count.

The worker now:

1. claims queued or interrupted work with one unique owner token and bounded expiry;
2. renews ownership while domain work is active;
3. reconciles missing or expired ownership before every claim opportunity;
4. cancels local work if renewal proves ownership was lost;
5. permits only the current owner to update, fail, or complete the operation.

Status, recent-operation discovery, and equivalent initiation also reconcile expired ownership. Repeating `ec_publish_initial_rules` therefore reuses the same recoverable Operation ID rather than creating competing work or remaining permanently blocked. `Interrupted` recovery preserves the Operation ID, Correlation ID, original timestamps, committed domain stages, diagnostics, and source-unique publication identity; the attempt count records renewed execution.

## Deduplication and Idempotency

An unexpired worker-owned `Running` operation remains active and deduplicates equivalent initiation. Only absent or expired ownership is recoverable. Repeated reconciliation is idempotent, and stale owners cannot write after another worker reclaims the operation. Publication continues through its existing source-unique idempotent stages.

## Migration and Compatibility

Migration 009 is additive and repeat-safe. Existing operation rows gain an attempt count of zero. Existing `Running` rows have no owner lease and are intentionally classified as orphaned on the first post-migration reconciliation. No campaign table, Campaign Canon, Rule Release identity, Operation ID, Correlation ID, or existing diagnostic record is rewritten.

Deployments must run the ordinary approved setup/migration path before the updated worker can use the new columns. No user or AI edits SQL manually, deletes the operation, or invents another Campaign ID.

## Architectural Classification

- **Managed Service contract:** a `Running` durable operation has provable bounded execution ownership; lost ownership becomes recoverable; retry preserves identity and idempotency; live ownership retains deduplication.
- **Reference implementation:** .NET heartbeat tasks, SQL Server owner/expiry columns, migration 009, T-SQL locking, and configured lease duration.
- **Eternal Cycle-wide:** no gameplay or fictional rule changed.

## Validation

Focused automated coverage verifies:

- an unexpired claim remains `Running` and deduplicated;
- heartbeat renewal keeps a long-running worker claim live past its original expiry;
- an expired claim becomes `Interrupted`;
- repeated initiation preserves Operation ID and Correlation ID;
- the hosted worker reclaims and completes the same operation;
- execution-attempt evidence advances without duplicate operations;
- an expired owner cannot update or complete the operation before reconciliation;
- migration 009 and its template are packaged with the reference service.

Validation completed on 2026-09-19:

- the full .NET test project passed `125` of `125` tests;
- the FR-021 durable Managed-operation harness passed `73` assertions;
- repository validation passed, including every FR-011 through FR-021 harness, release-neutral campaign-mode checks, `7,146` relative Markdown links, `163` anchors, and distribution boundaries;
- `git diff --check` reported no whitespace errors.

The opt-in SQL Server integration test was attempted but the host's LocalDB automatic instance could not start. That environmental failure does not validate or invalidate migration 009; live SQL migration and power-cycle acceptance remain explicitly unproven.

## Remaining Live Boundary

Unit and structural tests do not prove migration 009 against the maintainer's live SQL Server or another real PC power cycle. That acceptance remains external: migrate the service, begin publication, terminate the host during `AcquireSource`, restart, and verify the same operation is reconciled and resumed without manual persistence repair.

## Result

FR-021 remains complete with corrective provenance. Phase 13 remains active, Future Revisions remains `[∞]`, and no later objective is selected automatically.

## Related Documents

- [Managed Operations](../../docs/persistence/MANAGED_OPERATIONS.md)
- [Managed Rule Publication](../../docs/rules/MANAGED_RULE_PUBLICATION.md)
- [Reference Managed Service](../../examples/tooling/managed-data/mcp-dotnet-tsql/README.md)
- [MCP-Only Acceptance Test](../../examples/tooling/managed-data/mcp-dotnet-tsql/MCP_ONLY_ACCEPTANCE_TEST.md)
