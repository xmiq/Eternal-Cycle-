using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace EternalCycle.Persistence.Mcp;

public sealed record CampaignSchemaRoute(
    string CampaignId,
    string WorldModelId,
    string SchemaName,
    string SchemaModelVersion,
    string RulesetVersion);

public interface ICampaignSchemaResolver
{
    CampaignSchemaRoute Resolve(string campaignId);
}

public sealed class ConfiguredCampaignSchemaResolver : ICampaignSchemaResolver
{
    private readonly SqlServerPersistenceOptions settings;
    private readonly IReadOnlyDictionary<string, string> campaignWorldModels;
    private readonly IReadOnlyDictionary<string, WorldSchemaOptions> worldSchemas;

    public ConfiguredCampaignSchemaResolver(IOptions<SqlServerPersistenceOptions> options)
    {
        settings = options.Value;
        SqlServerSchemaIdentifier.Validate(settings.DefaultSchema);
        RequireValue(settings.DefaultWorldModelId, nameof(settings.DefaultWorldModelId));
        RequireValue(settings.DefaultSchemaModelVersion, nameof(settings.DefaultSchemaModelVersion));
        RequireValue(settings.DefaultRulesetVersion, nameof(settings.DefaultRulesetVersion));

        campaignWorldModels = new Dictionary<string, string>(
            settings.CampaignWorldModels,
            StringComparer.Ordinal);
        worldSchemas = new Dictionary<string, WorldSchemaOptions>(
            settings.WorldSchemas,
            StringComparer.Ordinal);

        foreach (var (campaignId, worldModelId) in campaignWorldModels)
        {
            RequireValue(campaignId, "CampaignWorldModels campaign ID");
            RequireValue(worldModelId, $"CampaignWorldModels[{campaignId}]");
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

        if (worldSchemas.TryGetValue(worldModelId, out var binding))
        {
            return new CampaignSchemaRoute(
                campaignId,
                worldModelId,
                SqlServerSchemaIdentifier.Validate(binding.SchemaName),
                binding.SchemaModelVersion,
                binding.RulesetVersion);
        }

        if (!string.Equals(worldModelId, settings.DefaultWorldModelId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"World model '{worldModelId}' has no trusted SQL schema binding.");
        }

        return new CampaignSchemaRoute(
            campaignId,
            worldModelId,
            SqlServerSchemaIdentifier.Validate(settings.DefaultSchema),
            settings.DefaultSchemaModelVersion,
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
}
