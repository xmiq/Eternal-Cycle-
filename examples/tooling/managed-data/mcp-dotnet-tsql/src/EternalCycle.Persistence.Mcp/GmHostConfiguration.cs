using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public enum GmHostConfigurationState
{
    Required,
    InstructionsPresented,
    UserConfirmed,
    Verified
}

public sealed record GmHostBootstrapArtifact(
    string RuleReleaseId,
    string SourceIdentity,
    string SourceHash,
    string Text);

public sealed record GmHostConfigurationStatus(
    GmHostConfigurationState State,
    GmHostBootstrapArtifact Bootstrap,
    DateTimeOffset? PresentedAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? VerifiedAt,
    string Guidance);

public sealed record GmHostConfigurationConfirmation(bool UserConfirmed);

public sealed record StoredGmHostConfiguration(
    string RulesetId,
    string BootstrapSourceHash,
    GmHostConfigurationState State,
    DateTimeOffset? PresentedAt,
    DateTimeOffset? ConfirmedAt,
    DateTimeOffset? VerifiedAt,
    long Revision);

public interface IGmHostConfigurationStore
{
    Task<StoredGmHostConfiguration?> GetAsync(string rulesetId, CancellationToken cancellationToken);

    Task<StoredGmHostConfiguration> SaveAsync(
        StoredGmHostConfiguration configuration,
        CancellationToken cancellationToken);
}

public interface IGmHostConfigurationService
{
    Task<ManagedOperationResult<GmHostConfigurationStatus>> PresentInstructionsAsync(
        CancellationToken cancellationToken);

    Task<ManagedOperationResult<GmHostConfigurationStatus>> ConfirmAsync(
        GmHostConfigurationConfirmation confirmation,
        CancellationToken cancellationToken);

    Task<GmHostConfigurationStatus> RecordVerifiedAsync(
        string hostAttestation,
        CancellationToken cancellationToken);
}

public sealed class GmHostConfigurationService(
    IPublishedRuleStore publishedRules,
    IGmHostConfigurationStore configurations,
    IOptions<ManagedRuleServiceOptions> options) : IGmHostConfigurationService
{
    private readonly ManagedRuleServiceOptions settings = options.Value;

    public async Task<ManagedOperationResult<GmHostConfigurationStatus>> PresentInstructionsAsync(
        CancellationToken cancellationToken)
    {
        var artifact = await LoadArtifactAsync(cancellationToken);
        var current = await configurations.GetAsync(settings.RulesetId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (current is null ||
            !string.Equals(current.BootstrapSourceHash, artifact.SourceHash, StringComparison.Ordinal))
        {
            current = await configurations.SaveAsync(
                new StoredGmHostConfiguration(
                    settings.RulesetId,
                    artifact.SourceHash,
                    GmHostConfigurationState.InstructionsPresented,
                    now,
                    null,
                    null,
                    0),
                cancellationToken);
        }
        else if (current.State == GmHostConfigurationState.Required)
        {
            current = await configurations.SaveAsync(
                current with
                {
                    State = GmHostConfigurationState.InstructionsPresented,
                    PresentedAt = now
                },
                cancellationToken);
        }

        return new(
            true,
            "GM_HOST_INSTRUCTIONS_PRESENTED",
            "Install the canonical bootstrap in the host's highest supported instruction field, then confirm naturally when it is done.",
            ToStatus(current, artifact));
    }

    public async Task<ManagedOperationResult<GmHostConfigurationStatus>> ConfirmAsync(
        GmHostConfigurationConfirmation confirmation,
        CancellationToken cancellationToken)
    {
        if (!confirmation.UserConfirmed)
        {
            return new(
                false,
                "GM_HOST_CONFIRMATION_REQUIRED",
                "Confirm only after the canonical bootstrap has been installed in the host configuration.",
                null,
                RetrySafe: true,
                UserInterventionRequired: true);
        }

        var artifact = await LoadArtifactAsync(cancellationToken);
        var current = await configurations.GetAsync(settings.RulesetId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var next = current is not null &&
                   string.Equals(current.BootstrapSourceHash, artifact.SourceHash, StringComparison.Ordinal) &&
                   current.State is GmHostConfigurationState.UserConfirmed or GmHostConfigurationState.Verified
            ? current
            : new StoredGmHostConfiguration(
                settings.RulesetId,
                artifact.SourceHash,
                GmHostConfigurationState.UserConfirmed,
                current?.PresentedAt ?? now,
                now,
                null,
                current?.Revision ?? 0);
        var saved = ReferenceEquals(next, current)
            ? current!
            : await configurations.SaveAsync(next, cancellationToken);

        return new(
            true,
            "GM_HOST_USER_CONFIRMED",
            "The host bootstrap is recorded as user-confirmed. Generic hosts cannot claim technical verification from user confirmation alone.",
            ToStatus(saved, artifact));
    }

    public async Task<GmHostConfigurationStatus> RecordVerifiedAsync(
        string hostAttestation,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(hostAttestation))
        {
            throw new ArgumentException("A host verification attestation is required.", nameof(hostAttestation));
        }

        var artifact = await LoadArtifactAsync(cancellationToken);
        var current = await configurations.GetAsync(settings.RulesetId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var saved = await configurations.SaveAsync(
            new StoredGmHostConfiguration(
                settings.RulesetId,
                artifact.SourceHash,
                GmHostConfigurationState.Verified,
                current?.PresentedAt ?? now,
                current?.ConfirmedAt,
                now,
                current?.Revision ?? 0),
            cancellationToken);
        return ToStatus(saved, artifact);
    }

    private async Task<GmHostBootstrapArtifact> LoadArtifactAsync(CancellationToken cancellationToken)
    {
        var release = await publishedRules.GetActiveAsync(settings.RulesetId, cancellationToken)
            ?? throw new ManagedServiceException(
                "GM_HOST_BOOTSTRAP_UNAVAILABLE",
                "The canonical GM Host Bootstrap is unavailable until a compatible Rule Release is active.");
        var chunks = release.Index.Chunks
            .Where(chunk => string.Equals(
                chunk.RuleSourceId,
                RuleCompiler.GmHostBootstrapSourceId,
                StringComparison.Ordinal))
            .OrderBy(chunk => chunk.ChunkId, StringComparer.Ordinal)
            .ToArray();
        if (chunks.Length == 0)
        {
            throw new ManagedServiceException(
                "GM_HOST_BOOTSTRAP_UNAVAILABLE",
                "The active Rule Release does not contain the canonical GM Host Bootstrap.");
        }

        var hashes = chunks.Select(chunk => chunk.SourceHash).Distinct(StringComparer.Ordinal).ToArray();
        if (hashes.Length != 1)
        {
            throw new ManagedServiceException(
                "GM_HOST_BOOTSTRAP_INVALID",
                "The active Rule Release contains inconsistent GM Host Bootstrap provenance.");
        }

        return new GmHostBootstrapArtifact(
            release.RuleReleaseId,
            release.SourceIdentity,
            hashes[0],
            string.Join("\n\n", chunks.Select(chunk => chunk.Content)));
    }

    private static GmHostConfigurationStatus ToStatus(
        StoredGmHostConfiguration configuration,
        GmHostBootstrapArtifact artifact) =>
        new(
            configuration.State,
            artifact,
            configuration.PresentedAt,
            configuration.ConfirmedAt,
            configuration.VerifiedAt,
            configuration.State switch
            {
                GmHostConfigurationState.InstructionsPresented =>
                    "Paste the exact bootstrap text into the host's highest supported instruction field, then confirm when complete.",
                GmHostConfigurationState.UserConfirmed =>
                    "The user confirmed installation; the host has not technically attested it.",
                GmHostConfigurationState.Verified =>
                    "A capable host integration attested the installed bootstrap.",
                _ => "Host configuration is required before gameplay."
            });
}

public sealed class SqlServerGmHostConfigurationStore(
    IOptions<SqlServerPersistenceOptions> options) : IGmHostConfigurationStore
{
    private readonly SqlServerPersistenceOptions settings = options.Value;

    public async Task<StoredGmHostConfiguration?> GetAsync(
        string rulesetId,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            SELECT bootstrap_source_hash, configuration_state, presented_at,
                   confirmed_at, verified_at, configuration_revision
            FROM {{schema}}.gm_host_configurations
            WHERE ruleset_id = @ruleset_id;
            """);
        command.Parameters.AddWithValue("@ruleset_id", rulesetId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new StoredGmHostConfiguration(
            rulesetId,
            reader.GetString(0),
            Enum.Parse<GmHostConfigurationState>(reader.GetString(1), ignoreCase: false),
            reader.IsDBNull(2) ? null : reader.GetDateTimeOffset(2),
            reader.IsDBNull(3) ? null : reader.GetDateTimeOffset(3),
            reader.IsDBNull(4) ? null : reader.GetDateTimeOffset(4),
            reader.GetInt64(5));
    }

    public async Task<StoredGmHostConfiguration> SaveAsync(
        StoredGmHostConfiguration configuration,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(settings.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = Command(connection, """
            SET XACT_ABORT ON;
            MERGE {{schema}}.gm_host_configurations WITH (HOLDLOCK) AS target
            USING (SELECT @ruleset_id AS ruleset_id) AS source
              ON target.ruleset_id = source.ruleset_id
            WHEN MATCHED THEN UPDATE SET
                bootstrap_source_hash = @bootstrap_source_hash,
                configuration_state = @configuration_state,
                presented_at = @presented_at,
                confirmed_at = @confirmed_at,
                verified_at = @verified_at,
                configuration_revision = target.configuration_revision + 1,
                updated_at = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN INSERT (
                ruleset_id, bootstrap_source_hash, configuration_state,
                presented_at, confirmed_at, verified_at,
                configuration_revision, updated_at
            ) VALUES (
                @ruleset_id, @bootstrap_source_hash, @configuration_state,
                @presented_at, @confirmed_at, @verified_at,
                1, SYSUTCDATETIME()
            );
            SELECT bootstrap_source_hash, configuration_state, presented_at,
                   confirmed_at, verified_at, configuration_revision
            FROM {{schema}}.gm_host_configurations
            WHERE ruleset_id = @ruleset_id;
            """);
        command.Parameters.AddWithValue("@ruleset_id", configuration.RulesetId);
        command.Parameters.AddWithValue("@bootstrap_source_hash", configuration.BootstrapSourceHash);
        command.Parameters.AddWithValue("@configuration_state", configuration.State.ToString());
        command.Parameters.AddWithValue("@presented_at", (object?)configuration.PresentedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@confirmed_at", (object?)configuration.ConfirmedAt ?? DBNull.Value);
        command.Parameters.AddWithValue("@verified_at", (object?)configuration.VerifiedAt ?? DBNull.Value);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        _ = await reader.ReadAsync(cancellationToken);
        return configuration with
        {
            BootstrapSourceHash = reader.GetString(0),
            State = Enum.Parse<GmHostConfigurationState>(reader.GetString(1), ignoreCase: false),
            PresentedAt = reader.IsDBNull(2) ? null : reader.GetDateTimeOffset(2),
            ConfirmedAt = reader.IsDBNull(3) ? null : reader.GetDateTimeOffset(3),
            VerifiedAt = reader.IsDBNull(4) ? null : reader.GetDateTimeOffset(4),
            Revision = reader.GetInt64(5)
        };
    }

    private SqlCommand Command(SqlConnection connection, string text) =>
        new(SqlServerSchemaIdentifier.Bind(text, settings.DomainSchema), connection)
        {
            CommandTimeout = settings.CommandTimeoutSeconds
        };
}
