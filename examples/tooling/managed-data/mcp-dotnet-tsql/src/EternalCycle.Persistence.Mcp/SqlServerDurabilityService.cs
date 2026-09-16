using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public sealed class SqlServerDurabilityService(
    IOptions<SqlServerPersistenceOptions> options,
    ICampaignSchemaResolver schemaResolver) : IDurabilityService
{
    private readonly SqlServerPersistenceOptions settings = options.Value;

    public async Task<string> VerifyCompletionAsync(
        string campaignId,
        long campaignVersion,
        CancellationToken cancellationToken)
    {
        if (!settings.RequireRecoveryPointForCompletion)
        {
            return "SQL Server commit durability is the configured completion boundary; recovery points follow the server-managed schedule.";
        }

        if (string.IsNullOrWhiteSpace(settings.RecoveryPointDirectory))
        {
            throw new InvalidOperationException("Required recovery-point durability is not configured.");
        }

        var builder = new SqlConnectionStringBuilder(settings.ConnectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            throw new InvalidOperationException("Required recovery-point durability cannot resolve the SQL Server database identity.");
        }

        var safeCampaignId = string.Concat(campaignId.Select(character =>
            char.IsLetterOrDigit(character) || character is '-' or '_' ? character : '_'));
        var recoveryPointId = $"RP-{Guid.NewGuid():N}";
        var backupPath = Path.Combine(
            settings.RecoveryPointDirectory,
            $"eternal-cycle-{safeCampaignId}-v{campaignVersion}-{recoveryPointId}.bak");
        var quotedDatabase = $"[{builder.InitialCatalog.Replace("]", "]]", StringComparison.Ordinal)}]";

        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.CommandTimeout = settings.CommandTimeoutSeconds;
            command.CommandText = $"""
                DECLARE @target nvarchar(4000) = @backup_path;
                BACKUP DATABASE {quotedDatabase}
                    TO DISK = @target
                    WITH COPY_ONLY, INIT, CHECKSUM, NAME = @backup_name;
                RESTORE VERIFYONLY FROM DISK = @target WITH CHECKSUM;
                """;
            command.Parameters.AddWithValue("@backup_path", backupPath);
            command.Parameters.AddWithValue("@backup_name", $"Eternal Cycle {campaignId} version {campaignVersion}");
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var record = connection.CreateCommand())
        {
            record.CommandTimeout = settings.CommandTimeoutSeconds;
            var route = schemaResolver.Resolve(campaignId);
            record.CommandText = SqlServerSchemaIdentifier.Bind("""
                INSERT INTO {{schema}}.recovery_points (
                    recovery_point_id,
                    campaign_id,
                    campaign_version,
                    provider,
                    provider_reference,
                    content_hash,
                    status,
                    created_at,
                    verified_at
                ) VALUES (
                    @recovery_point_id,
                    @campaign_id,
                    @campaign_version,
                    N'SQL_SERVER_BACKUP',
                    @provider_reference,
                    NULL,
                    N'Verified',
                    SYSUTCDATETIME(),
                    SYSUTCDATETIME()
                );
                """, route.SchemaName);
            record.Parameters.AddWithValue("@recovery_point_id", recoveryPointId);
            record.Parameters.AddWithValue("@campaign_id", campaignId);
            record.Parameters.AddWithValue("@campaign_version", campaignVersion);
            record.Parameters.AddWithValue("@provider_reference", backupPath);
            await record.ExecuteNonQueryAsync(cancellationToken);
        }

        return $"Required SQL Server recovery point {recoveryPointId} was created and verified.";
    }
}
