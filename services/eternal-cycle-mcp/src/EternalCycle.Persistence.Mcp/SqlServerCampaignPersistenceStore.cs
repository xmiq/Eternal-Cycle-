using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public sealed class SqlServerCampaignPersistenceStore(
    IOptions<SqlServerPersistenceOptions> options,
    IDurabilityService durabilityService,
    ILogger<SqlServerCampaignPersistenceStore> logger) : ICampaignPersistenceStore
{
    private readonly SqlServerPersistenceOptions settings = options.Value;

    public async Task<PersistenceStatus> GetStatusAsync(
        string campaignId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, null, """
            SELECT
                c.active_version,
                c.last_validated_commit_at,
                t.transaction_id,
                t.status,
                t.failure_reason
            FROM ec.campaigns AS c
            OUTER APPLY (
                SELECT TOP (1)
                    st.transaction_id,
                    st.status,
                    st.failure_reason,
                    st.started_at
                FROM ec.save_transactions AS st
                WHERE st.campaign_id = c.campaign_id
                ORDER BY st.started_at DESC
            ) AS t
            WHERE c.campaign_id = @campaign_id;
            """);
        AddParameter(command, "@campaign_id", campaignId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException("The configured MCP campaign identity does not exist.");
        }

        var version = reader.GetInt64(0);
        DateTimeOffset? lastCommit = reader.IsDBNull(1)
            ? null
            : reader.GetFieldValue<DateTimeOffset>(1);
        var transactionId = reader.IsDBNull(2) ? null : reader.GetString(2);
        var transactionStatus = reader.IsDBNull(3) ? null : reader.GetString(3);
        var failureReason = reader.IsDBNull(4) ? null : reader.GetString(4);

        var isPending = transactionStatus is "Staged" or "CandidateValidated" or "ActivatedPendingReadback";
        var isFailed = transactionStatus is "FailedValidation" or "FailedActivation" or "FailedReadback" or "FailedDurability";

        return new PersistenceStatus(
            isFailed ? PersistenceMarkers.Failed : isPending ? PersistenceMarkers.Pending : PersistenceMarkers.Saved,
            "MCP",
            "Eternal Cycle MCP Persistence Service",
            campaignId,
            version,
            isPending,
            isPending || isFailed ? transactionId : null,
            lastCommit,
            isFailed ? failureReason : null);
    }

    public async Task<ReadRecordsResult> ReadRecordsAsync(
        string campaignId,
        IReadOnlyList<RecordAddress> records,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        var activeVersion = await GetActiveVersionAsync(connection, null, campaignId, false, cancellationToken);
        var results = new List<CanonicalRecord>(records.Count);

        foreach (var address in records)
        {
            var record = await ReadEffectiveRecordAsync(
                connection,
                null,
                campaignId,
                address.OwnerDomain,
                address.RecordId,
                activeVersion,
                cancellationToken);

            if (record is not null && !record.Tombstone)
            {
                results.Add(record);
            }
        }

        return new ReadRecordsResult(campaignId, activeVersion, results);
    }

    public async Task<CommitResult> CommitAsync(
        CommitCampaignRequest request,
        string requestHash,
        CancellationToken cancellationToken)
    {
        var existing = await FindTransactionAsync(
            request.CampaignId,
            request.TransactionId,
            request.IdempotencyKey,
            cancellationToken);

        if (existing is not null)
        {
            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return Failure(existing.CandidateVersion, "The transaction or idempotency key was reused with a different request.");
            }

            return await ResumeAsync(request.CampaignId, existing.TransactionId, cancellationToken);
        }

        try
        {
            await StageCandidateAsync(request, requestHash, cancellationToken);
            return await ResumeAsync(request.CampaignId, request.TransactionId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Eternal Cycle persistence transaction {TransactionId} failed before activation.", request.TransactionId);
            await MarkFailedAsync(request.CampaignId, request.TransactionId, "FailedValidation", exception.Message, cancellationToken);
            return Failure(null, exception.Message);
        }
    }

    public Task<CommitResult> RetryAsync(
        string campaignId,
        string transactionId,
        CancellationToken cancellationToken) =>
        ResumeAsync(campaignId, transactionId, cancellationToken);

    private async Task StageCandidateAsync(
        CommitCampaignRequest request,
        string requestHash,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var activeVersion = await GetActiveVersionAsync(
            connection,
            transaction,
            request.CampaignId,
            true,
            cancellationToken);
        if (activeVersion != request.ExpectedParentVersion)
        {
            throw new InvalidOperationException(
                $"Stale parent version. Expected {request.ExpectedParentVersion}, canonical state is {activeVersion}.");
        }

        var candidateVersion = checked(activeVersion + 1);
        var requestJson = JsonSerializer.Serialize(request);

        await using (var insertTransaction = CreateCommand(connection, transaction, """
            INSERT INTO ec.save_transactions (
                transaction_id,
                campaign_id,
                idempotency_key,
                request_hash,
                request_json,
                parent_version,
                candidate_version,
                source_interaction_id,
                affected_set_json,
                status,
                started_at
            ) VALUES (
                @transaction_id,
                @campaign_id,
                @idempotency_key,
                @request_hash,
                @request_json,
                @parent_version,
                @candidate_version,
                @source_interaction_id,
                @affected_set_json,
                N'Staged',
                SYSUTCDATETIME()
            );
            """))
        {
            AddParameter(insertTransaction, "@transaction_id", request.TransactionId);
            AddParameter(insertTransaction, "@campaign_id", request.CampaignId);
            AddParameter(insertTransaction, "@idempotency_key", request.IdempotencyKey);
            AddParameter(insertTransaction, "@request_hash", requestHash);
            AddParameter(insertTransaction, "@request_json", requestJson);
            AddParameter(insertTransaction, "@parent_version", activeVersion);
            AddParameter(insertTransaction, "@candidate_version", candidateVersion);
            AddParameter(insertTransaction, "@source_interaction_id", request.SourceInteractionId);
            AddParameter(insertTransaction, "@affected_set_json", JsonSerializer.Serialize(request.AffectedOwnerDomains));
            await insertTransaction.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var mutation in request.Mutations)
        {
            var current = await ReadEffectiveRecordAsync(
                connection,
                transaction,
                request.CampaignId,
                mutation.OwnerDomain,
                mutation.RecordId,
                activeVersion,
                cancellationToken);
            var currentRevision = current?.RecordRevision;

            if (currentRevision != mutation.ExpectedRevision)
            {
                throw new InvalidOperationException(
                    $"Revision conflict for {mutation.OwnerDomain}/{mutation.RecordId}. Expected {mutation.ExpectedRevision?.ToString() ?? "absent"}, canonical revision is {currentRevision?.ToString() ?? "absent"}.");
            }

            var payload = mutation.Tombstone ? "{}" : mutation.PayloadJson;
            var payloadHash = ComputeHash(payload);
            var nextRevision = checked((currentRevision ?? 0) + 1);

            await using var insertRecord = CreateCommand(connection, transaction, """
                INSERT INTO ec.canonical_record_versions (
                    campaign_id,
                    owner_domain,
                    record_id,
                    campaign_version,
                    record_revision,
                    payload_json,
                    payload_hash,
                    is_tombstone,
                    transaction_id,
                    recorded_at
                ) VALUES (
                    @campaign_id,
                    @owner_domain,
                    @record_id,
                    @campaign_version,
                    @record_revision,
                    @payload_json,
                    @payload_hash,
                    @is_tombstone,
                    @transaction_id,
                    SYSUTCDATETIME()
                );
                """);
            AddParameter(insertRecord, "@campaign_id", request.CampaignId);
            AddParameter(insertRecord, "@owner_domain", mutation.OwnerDomain);
            AddParameter(insertRecord, "@record_id", mutation.RecordId);
            AddParameter(insertRecord, "@campaign_version", candidateVersion);
            AddParameter(insertRecord, "@record_revision", nextRevision);
            AddParameter(insertRecord, "@payload_json", payload);
            AddParameter(insertRecord, "@payload_hash", payloadHash);
            AddParameter(insertRecord, "@is_tombstone", mutation.Tombstone);
            AddParameter(insertRecord, "@transaction_id", request.TransactionId);
            await insertRecord.ExecuteNonQueryAsync(cancellationToken);
        }

        foreach (var mutation in request.Mutations)
        {
            foreach (var reference in mutation.References ?? [])
            {
                var target = await ReadCandidateRecordAsync(
                    connection,
                    transaction,
                    request.CampaignId,
                    reference.TargetOwnerDomain,
                    reference.TargetRecordId,
                    candidateVersion,
                    request.TransactionId,
                    cancellationToken) ?? await ReadEffectiveRecordAsync(
                    connection,
                    transaction,
                    request.CampaignId,
                    reference.TargetOwnerDomain,
                    reference.TargetRecordId,
                    activeVersion,
                    cancellationToken);
                if (target is null || target.Tombstone)
                {
                    throw new InvalidOperationException(
                        $"Dangling reference from {mutation.OwnerDomain}/{mutation.RecordId} to {reference.TargetOwnerDomain}/{reference.TargetRecordId}.");
                }

                await using var insertReference = CreateCommand(connection, transaction, """
                    INSERT INTO ec.record_references (
                        campaign_id,
                        source_owner_domain,
                        source_record_id,
                        campaign_version,
                        relation_type,
                        target_owner_domain,
                        target_record_id,
                        transaction_id
                    ) VALUES (
                        @campaign_id,
                        @source_owner_domain,
                        @source_record_id,
                        @campaign_version,
                        @relation_type,
                        @target_owner_domain,
                        @target_record_id,
                        @transaction_id
                    );
                    """);
                AddParameter(insertReference, "@campaign_id", request.CampaignId);
                AddParameter(insertReference, "@source_owner_domain", mutation.OwnerDomain);
                AddParameter(insertReference, "@source_record_id", mutation.RecordId);
                AddParameter(insertReference, "@campaign_version", candidateVersion);
                AddParameter(insertReference, "@relation_type", reference.RelationType);
                AddParameter(insertReference, "@target_owner_domain", reference.TargetOwnerDomain);
                AddParameter(insertReference, "@target_record_id", reference.TargetRecordId);
                AddParameter(insertReference, "@transaction_id", request.TransactionId);
                await insertReference.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<CommitResult> ResumeAsync(
        string campaignId,
        string transactionId,
        CancellationToken cancellationToken)
    {
        var stored = await FindTransactionAsync(campaignId, transactionId, null, cancellationToken)
            ?? throw new InvalidOperationException("The requested persistence transaction does not exist.");

        if (stored.Status == "Completed")
        {
            return await ReadCompletedResultAsync(campaignId, transactionId, cancellationToken);
        }

        if (stored.Status is "ActivatedPendingReadback" or "FailedReadback" or "FailedDurability")
        {
            return await FinalizeActivationReadbackAsync(stored, cancellationToken);
        }

        if (stored.Status is "FailedActivation")
        {
            return Failure(stored.CandidateVersion, stored.FailureReason ?? "Activation failed and requires continuity resolution.");
        }

        try
        {
            await ValidateCandidateAsync(stored, cancellationToken);
            await ActivateCandidateAsync(stored, cancellationToken);
            var refreshed = await FindTransactionAsync(campaignId, transactionId, null, cancellationToken)
                ?? throw new InvalidOperationException("Activated transaction could not be reloaded.");
            return await FinalizeActivationReadbackAsync(refreshed, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Eternal Cycle persistence transaction {TransactionId} failed validation or activation.", transactionId);
            var failureStatus = exception.Message.Contains("active version", StringComparison.OrdinalIgnoreCase)
                ? "FailedActivation"
                : "FailedValidation";
            await MarkFailedAsync(campaignId, transactionId, failureStatus, exception.Message, cancellationToken);
            return Failure(stored.CandidateVersion, exception.Message);
        }
    }

    private async Task ValidateCandidateAsync(StoredTransaction stored, CancellationToken cancellationToken)
    {
        var request = JsonSerializer.Deserialize<CommitCampaignRequest>(stored.RequestJson)
            ?? throw new InvalidOperationException("Stored transaction request is unreadable.");

        await using var connection = await OpenConnectionAsync(cancellationToken);
        foreach (var mutation in request.Mutations)
        {
            var candidate = await ReadCandidateRecordAsync(
                connection,
                null,
                stored.CampaignId,
                mutation.OwnerDomain,
                mutation.RecordId,
                stored.CandidateVersion,
                stored.TransactionId,
                cancellationToken);
            var expectedHash = ComputeHash(mutation.Tombstone ? "{}" : mutation.PayloadJson);

            if (candidate is null ||
                candidate.CampaignVersion != stored.CandidateVersion ||
                candidate.Tombstone != mutation.Tombstone ||
                !string.Equals(ComputeHash(candidate.PayloadJson), expectedHash, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Candidate read-back failed for {mutation.OwnerDomain}/{mutation.RecordId}.");
            }
        }

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using (var validation = CreateCommand(connection, transaction, """
            INSERT INTO ec.validation_runs (
                validation_id,
                campaign_id,
                transaction_id,
                campaign_version,
                outcome,
                evidence,
                completed_at
            ) VALUES (
                @validation_id,
                @campaign_id,
                @transaction_id,
                @campaign_version,
                N'PASS',
                N'Candidate records and expected hashes were read back successfully.',
                SYSUTCDATETIME()
            );

            UPDATE ec.save_transactions
            SET status = N'CandidateValidated', failure_reason = NULL
            WHERE campaign_id = @campaign_id AND transaction_id = @transaction_id;
            """))
        {
            AddParameter(validation, "@validation_id", $"VAL-{Guid.NewGuid():N}");
            AddParameter(validation, "@campaign_id", stored.CampaignId);
            AddParameter(validation, "@transaction_id", stored.TransactionId);
            AddParameter(validation, "@campaign_version", stored.CandidateVersion);
            await validation.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task ActivateCandidateAsync(StoredTransaction stored, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var activeVersion = await GetActiveVersionAsync(
            connection,
            transaction,
            stored.CampaignId,
            true,
            cancellationToken);
        if (activeVersion != stored.ParentVersion)
        {
            throw new InvalidOperationException(
                $"The active version changed from {stored.ParentVersion} to {activeVersion} before activation.");
        }

        var receiptId = $"RCPT-{Guid.NewGuid():N}";
        await using var command = CreateCommand(connection, transaction, """
            UPDATE ec.campaigns
            SET active_version = @candidate_version
            WHERE campaign_id = @campaign_id AND active_version = @parent_version;

            IF @@ROWCOUNT <> 1
                THROW 51000, 'Candidate activation lost its parent-version lock.', 1;

            UPDATE ec.save_transactions
            SET status = N'ActivatedPendingReadback', activated_at = SYSUTCDATETIME(), failure_reason = NULL
            WHERE campaign_id = @campaign_id AND transaction_id = @transaction_id;

            INSERT INTO ec.persistence_receipts (
                receipt_id,
                campaign_id,
                transaction_id,
                campaign_version,
                status,
                validation_evidence,
                completed_at
            ) VALUES (
                @receipt_id,
                @campaign_id,
                @transaction_id,
                @candidate_version,
                N'PendingReadback',
                N'Candidate validation passed; activation committed; final read-back pending.',
                SYSUTCDATETIME()
            );
            """);
        AddParameter(command, "@candidate_version", stored.CandidateVersion);
        AddParameter(command, "@parent_version", stored.ParentVersion);
        AddParameter(command, "@campaign_id", stored.CampaignId);
        AddParameter(command, "@transaction_id", stored.TransactionId);
        AddParameter(command, "@receipt_id", receiptId);
        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<CommitResult> FinalizeActivationReadbackAsync(
        StoredTransaction stored,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = await OpenConnectionAsync(cancellationToken);
            var activeVersion = await GetActiveVersionAsync(
                connection,
                null,
                stored.CampaignId,
                false,
                cancellationToken);
            if (activeVersion != stored.CandidateVersion)
            {
                throw new InvalidOperationException("Activated campaign version was not confirmed by read-back.");
            }

            var durabilityEvidence = await durabilityService.VerifyCompletionAsync(
                stored.CampaignId,
                stored.CandidateVersion,
                cancellationToken);

            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
            await using var command = CreateCommand(connection, transaction, """
                UPDATE ec.persistence_receipts
                SET
                    status = N'Validated',
                    validation_evidence = @validation_evidence,
                    completed_at = SYSUTCDATETIME()
                WHERE campaign_id = @campaign_id AND transaction_id = @transaction_id;

                UPDATE ec.save_transactions
                SET status = N'Completed', completed_at = SYSUTCDATETIME(), failure_reason = NULL
                WHERE campaign_id = @campaign_id AND transaction_id = @transaction_id;

                UPDATE ec.campaigns
                SET last_validated_commit_at = SYSUTCDATETIME()
                WHERE campaign_id = @campaign_id;
                """);
            AddParameter(command, "@campaign_id", stored.CampaignId);
            AddParameter(command, "@transaction_id", stored.TransactionId);
            AddParameter(command, "@validation_evidence", $"Candidate and activated campaign version were read back successfully. {durabilityEvidence}");
            await command.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await ReadCompletedResultAsync(stored.CampaignId, stored.TransactionId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            var failureStatus = exception.Message.Contains("recovery", StringComparison.OrdinalIgnoreCase) ||
                exception.Message.Contains("durability", StringComparison.OrdinalIgnoreCase)
                ? "FailedDurability"
                : "FailedReadback";
            await MarkFailedAsync(stored.CampaignId, stored.TransactionId, failureStatus, exception.Message, cancellationToken);
            return Failure(stored.CandidateVersion, exception.Message);
        }
    }

    private async Task<CommitResult> ReadCompletedResultAsync(
        string campaignId,
        string transactionId,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, null, """
            SELECT
                receipt_id,
                campaign_version,
                validation_evidence,
                completed_at
            FROM ec.persistence_receipts
            WHERE campaign_id = @campaign_id
              AND transaction_id = @transaction_id
              AND status = N'Validated';
            """);
        AddParameter(command, "@campaign_id", campaignId);
        AddParameter(command, "@transaction_id", transactionId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return Failure(null, "The transaction has no validated persistence receipt.");
        }

        var receipt = new PersistenceReceipt(
            reader.GetString(0),
            campaignId,
            transactionId,
            reader.GetInt64(1),
            PersistenceMarkers.Saved,
            reader.GetString(2),
            reader.GetFieldValue<DateTimeOffset>(3));
        return new CommitResult(PersistenceMarkers.Saved, "Completed", true, receipt.CampaignVersion, receipt, null);
    }

    private async Task<StoredTransaction?> FindTransactionAsync(
        string campaignId,
        string? transactionId,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, null, """
            SELECT TOP (1)
                transaction_id,
                campaign_id,
                request_hash,
                request_json,
                parent_version,
                candidate_version,
                status,
                failure_reason
            FROM ec.save_transactions
            WHERE campaign_id = @campaign_id
              AND (
                    (@transaction_id IS NOT NULL AND transaction_id = @transaction_id)
                 OR (@idempotency_key IS NOT NULL AND idempotency_key = @idempotency_key)
              )
            ORDER BY started_at DESC;
            """);
        AddParameter(command, "@campaign_id", campaignId);
        AddParameter(command, "@transaction_id", transactionId);
        AddParameter(command, "@idempotency_key", idempotencyKey);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new StoredTransaction(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetInt64(4),
                reader.GetInt64(5),
                reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetString(7))
            : null;
    }

    private async Task MarkFailedAsync(
        string campaignId,
        string transactionId,
        string status,
        string reason,
        CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(connection, null, """
            UPDATE ec.save_transactions
            SET status = @status, failure_reason = @failure_reason
            WHERE campaign_id = @campaign_id AND transaction_id = @transaction_id;
            """);
        AddParameter(command, "@status", status);
        AddParameter(command, "@failure_reason", reason.Length > 2000 ? reason[..2000] : reason);
        AddParameter(command, "@campaign_id", campaignId);
        AddParameter(command, "@transaction_id", transactionId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<long> GetActiveVersionAsync(
        SqlConnection connection,
        SqlTransaction? transaction,
        string campaignId,
        bool lockForUpdate,
        CancellationToken cancellationToken)
    {
        var lockHint = lockForUpdate ? " WITH (UPDLOCK, HOLDLOCK)" : string.Empty;
        await using var command = CreateCommand(
            connection,
            transaction,
            $"SELECT active_version FROM ec.campaigns{lockHint} WHERE campaign_id = @campaign_id;");
        AddParameter(command, "@campaign_id", campaignId);
        var value = await command.ExecuteScalarAsync(cancellationToken);
        return value is null or DBNull
            ? throw new InvalidOperationException("The configured MCP campaign identity does not exist. Provisioning is an administrative operation; gameplay cannot create a blank replacement.")
            : Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task<CanonicalRecord?> ReadEffectiveRecordAsync(
        SqlConnection connection,
        SqlTransaction? transaction,
        string campaignId,
        string ownerDomain,
        string recordId,
        long atVersion,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            SELECT TOP (1)
                records.owner_domain,
                records.record_id,
                records.record_revision,
                records.campaign_version,
                records.payload_json,
                records.is_tombstone
            FROM ec.canonical_record_versions AS records
            INNER JOIN ec.save_transactions AS transactions
                ON transactions.campaign_id = records.campaign_id
               AND transactions.transaction_id = records.transaction_id
            WHERE records.campaign_id = @campaign_id
              AND records.owner_domain = @owner_domain
              AND records.record_id = @record_id
              AND records.campaign_version <= @campaign_version
              AND transactions.status IN (
                    N'ActivatedPendingReadback',
                    N'Completed',
                    N'FailedReadback',
                    N'FailedDurability'
              )
            ORDER BY records.campaign_version DESC;
            """);
        AddParameter(command, "@campaign_id", campaignId);
        AddParameter(command, "@owner_domain", ownerDomain);
        AddParameter(command, "@record_id", recordId);
        AddParameter(command, "@campaign_version", atVersion);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new CanonicalRecord(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt64(2),
                reader.GetInt64(3),
                reader.GetString(4),
                reader.GetBoolean(5))
            : null;
    }

    private async Task<CanonicalRecord?> ReadCandidateRecordAsync(
        SqlConnection connection,
        SqlTransaction? transaction,
        string campaignId,
        string ownerDomain,
        string recordId,
        long candidateVersion,
        string transactionId,
        CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(connection, transaction, """
            SELECT
                owner_domain,
                record_id,
                record_revision,
                campaign_version,
                payload_json,
                is_tombstone
            FROM ec.canonical_record_versions
            WHERE campaign_id = @campaign_id
              AND owner_domain = @owner_domain
              AND record_id = @record_id
              AND campaign_version = @campaign_version
              AND transaction_id = @transaction_id;
            """);
        AddParameter(command, "@campaign_id", campaignId);
        AddParameter(command, "@owner_domain", ownerDomain);
        AddParameter(command, "@record_id", recordId);
        AddParameter(command, "@campaign_version", candidateVersion);
        AddParameter(command, "@transaction_id", transactionId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken)
            ? new CanonicalRecord(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetInt64(2),
                reader.GetInt64(3),
                reader.GetString(4),
                reader.GetBoolean(5))
            : null;
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private SqlCommand CreateCommand(SqlConnection connection, SqlTransaction? transaction, string text) =>
        new(text, connection, transaction) { CommandTimeout = settings.CommandTimeoutSeconds };

    private static void AddParameter(SqlCommand command, string name, object? value) =>
        command.Parameters.AddWithValue(name, value ?? DBNull.Value);

    private static string ComputeHash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static CommitResult Failure(long? version, string reason) =>
        new(PersistenceMarkers.Failed, "Failed", false, version, null, reason);

    private sealed record StoredTransaction(
        string TransactionId,
        string CampaignId,
        string RequestHash,
        string RequestJson,
        long ParentVersion,
        long CandidateVersion,
        string Status,
        string? FailureReason);
}
