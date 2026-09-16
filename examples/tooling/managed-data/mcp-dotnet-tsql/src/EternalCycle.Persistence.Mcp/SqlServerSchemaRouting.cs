using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public sealed record CampaignSchemaRoute(
    string CampaignId,
    string WorldModelId,
    string DataNamespaceId,
    string SchemaName,
    string SchemaModelVersion,
    string RulesetId,
    string RulesetVersion);

public interface ICampaignSchemaResolver
{
    CampaignSchemaRoute Resolve(string campaignId);
}

public sealed class ConfiguredCampaignSchemaResolver : ICampaignSchemaResolver
{
    private readonly SqlServerPersistenceOptions settings;
    private readonly IReadOnlyDictionary<string, string> campaignWorldModels;
    private readonly IReadOnlyDictionary<string, string> worldDataNamespaces;
    private readonly IReadOnlyDictionary<string, SqlDataNamespaceOptions> dataNamespaces;
    private readonly IReadOnlyDictionary<string, WorldSchemaOptions> worldSchemas;

    public ConfiguredCampaignSchemaResolver(IOptions<SqlServerPersistenceOptions> options)
    {
        settings = options.Value;
        SqlServerSchemaIdentifier.Validate(settings.DomainSchema);
        SqlServerSchemaIdentifier.Validate(settings.DefaultSchema);
        RequireValue(settings.DefaultDataNamespaceId, nameof(settings.DefaultDataNamespaceId));
        RequireValue(settings.DefaultWorldModelId, nameof(settings.DefaultWorldModelId));
        RequireValue(settings.DefaultRulesetId, nameof(settings.DefaultRulesetId));
        RequireValue(settings.DefaultSchemaModelVersion, nameof(settings.DefaultSchemaModelVersion));
        RequireValue(settings.DefaultRulesetVersion, nameof(settings.DefaultRulesetVersion));

        campaignWorldModels = new Dictionary<string, string>(
            settings.CampaignWorldModels,
            StringComparer.Ordinal);
        worldDataNamespaces = new Dictionary<string, string>(
            settings.WorldDataNamespaces,
            StringComparer.Ordinal);
        dataNamespaces = new Dictionary<string, SqlDataNamespaceOptions>(
            settings.DataNamespaces,
            StringComparer.Ordinal);
        worldSchemas = new Dictionary<string, WorldSchemaOptions>(
            settings.WorldSchemas,
            StringComparer.Ordinal);

        foreach (var (campaignId, worldModelId) in campaignWorldModels)
        {
            RequireValue(campaignId, "CampaignWorldModels campaign ID");
            RequireValue(worldModelId, $"CampaignWorldModels[{campaignId}]");
        }

        foreach (var (worldModelId, dataNamespaceId) in worldDataNamespaces)
        {
            RequireValue(worldModelId, "WorldDataNamespaces world-model ID");
            RequireValue(dataNamespaceId, $"WorldDataNamespaces[{worldModelId}]");
        }

        foreach (var (dataNamespaceId, binding) in dataNamespaces)
        {
            RequireValue(dataNamespaceId, "DataNamespaces namespace ID");
            ArgumentNullException.ThrowIfNull(binding);
            SqlServerSchemaIdentifier.Validate(binding.SchemaName);
            RequireValue(binding.SchemaModelVersion, $"DataNamespaces[{dataNamespaceId}].SchemaModelVersion");
            RequireValue(binding.RulesetId, $"DataNamespaces[{dataNamespaceId}].RulesetId");
            RequireValue(binding.RulesetVersion, $"DataNamespaces[{dataNamespaceId}].RulesetVersion");
        }

        foreach (var (worldModelId, binding) in worldSchemas)
        {
            RequireValue(worldModelId, "WorldSchemas world-model ID");
            ArgumentNullException.ThrowIfNull(binding);
            SqlServerSchemaIdentifier.Validate(binding.SchemaName);
            RequireValue(binding.SchemaModelVersion, $"WorldSchemas[{worldModelId}].SchemaModelVersion");
            RequireValue(binding.RulesetVersion, $"WorldSchemas[{worldModelId}].RulesetVersion");
        }
    }

    public CampaignSchemaRoute Resolve(string campaignId)
    {
        RequireValue(campaignId, nameof(campaignId));

        var worldModelId = campaignWorldModels.TryGetValue(campaignId, out var configuredWorld)
            ? configuredWorld
            : settings.DefaultWorldModelId;

        var hasConfiguredNamespace = worldDataNamespaces.TryGetValue(worldModelId, out var configuredNamespace);
        var dataNamespaceId = hasConfiguredNamespace
            ? configuredNamespace!
            : settings.DefaultDataNamespaceId;

        // Preserve the FR-018 world-to-schema configuration as a compatibility input.
        if (!hasConfiguredNamespace && worldSchemas.TryGetValue(worldModelId, out var legacyBinding))
        {
            return new CampaignSchemaRoute(
                campaignId,
                worldModelId,
                worldModelId,
                SqlServerSchemaIdentifier.Validate(legacyBinding.SchemaName),
                legacyBinding.SchemaModelVersion,
                settings.DefaultRulesetId,
                legacyBinding.RulesetVersion);
        }

        if (!hasConfiguredNamespace &&
            !string.Equals(worldModelId, settings.DefaultWorldModelId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"World model '{worldModelId}' has no trusted Logical Data Namespace mapping.");
        }

        if (dataNamespaces.TryGetValue(dataNamespaceId, out var namespaceBinding))
        {
            return new CampaignSchemaRoute(
                campaignId,
                worldModelId,
                dataNamespaceId,
                SqlServerSchemaIdentifier.Validate(namespaceBinding.SchemaName),
                namespaceBinding.SchemaModelVersion,
                namespaceBinding.RulesetId,
                namespaceBinding.RulesetVersion);
        }

        if (!string.Equals(worldModelId, settings.DefaultWorldModelId, StringComparison.Ordinal) ||
            !string.Equals(dataNamespaceId, settings.DefaultDataNamespaceId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"World model '{worldModelId}' and Data Namespace '{dataNamespaceId}' have no trusted SQL mapping.");
        }

        return new CampaignSchemaRoute(
            campaignId,
            worldModelId,
            dataNamespaceId,
            SqlServerSchemaIdentifier.Validate(settings.DefaultSchema),
            settings.DefaultSchemaModelVersion,
            settings.DefaultRulesetId,
            settings.DefaultRulesetVersion);
    }

    public static bool IsValidOptions(SqlServerPersistenceOptions options)
    {
        try
        {
            _ = new ConfiguredCampaignSchemaResolver(Options.Create(options));
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return false;
        }
    }

    private static string RequireValue(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
        {
            throw new ArgumentException("Configured identifiers and versions must contain 1 to 128 non-whitespace characters.", name);
        }

        return value;
    }
}

public static partial class SqlServerSchemaIdentifier
{
    public const string Token = "{{schema}}";

    public static string Validate(string schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName) ||
            schemaName.Length > 128 ||
            !ValidIdentifier().IsMatch(schemaName))
        {
            throw new ArgumentException(
                "SQL schema identifiers must begin with a letter or underscore and contain only letters, digits, or underscores.",
                nameof(schemaName));
        }

        return schemaName;
    }

    public static string Quote(string schemaName) => $"[{Validate(schemaName)}]";

    public static string Bind(string commandText, string schemaName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandText);
        if (!commandText.Contains(Token, StringComparison.Ordinal))
        {
            throw new ArgumentException("Schema-scoped SQL must contain the canonical schema token.", nameof(commandText));
        }

        return commandText.Replace(Token, Quote(schemaName), StringComparison.Ordinal);
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidIdentifier();
}

public static class SqlServerSchemaMigration
{
    public const string SchemaNameToken = "{{schema_name}}";

    public static string Render(string template, CampaignSchemaRoute route)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        ArgumentNullException.ThrowIfNull(route);

        var rendered = SqlServerSchemaIdentifier.Bind(template, route.SchemaName);
        return rendered.Replace(
            SchemaNameToken,
            SqlServerSchemaIdentifier.Validate(route.SchemaName),
            StringComparison.Ordinal);
    }

    public static string RenderDomain(string template, string domainSchema)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(template);
        var validated = SqlServerSchemaIdentifier.Validate(domainSchema);
        var rendered = SqlServerSchemaIdentifier.Bind(template, validated);
        return rendered.Replace(SchemaNameToken, validated, StringComparison.Ordinal);
    }
}
