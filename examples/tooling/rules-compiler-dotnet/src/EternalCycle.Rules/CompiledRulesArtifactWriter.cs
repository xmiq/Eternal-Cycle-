using System.Buffers;
using System.Text.Json;

namespace EternalCycle.Rules;

public sealed record CompiledRulesArtifactWriteResult(
    ReadOnlyMemory<byte> Bytes,
    IReadOnlyList<CompiledRulesArtifactValidationError> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

public static class CompiledRulesArtifactWriter
{
    public static CompiledRulesArtifactWriteResult Write(CompiledRulesArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        var errors = CompiledRulesArtifactContract.Validate(artifact);
        if (errors.Count != 0)
        {
            return new(ReadOnlyMemory<byte>.Empty, errors);
        }

        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
        {
            Indented = false,
            SkipValidation = false
        });
        WriteArtifact(writer, artifact);
        writer.Flush();
        return new(buffer.WrittenMemory.ToArray(), errors);
    }

    private static void WriteArtifact(Utf8JsonWriter writer, CompiledRulesArtifact artifact)
    {
        writer.WriteStartObject();
        writer.WriteNumber("artifactFormatVersion", artifact.ArtifactFormatVersion);
        WriteCompiler(writer, artifact.Compiler);
        WriteRuleset(writer, artifact.Ruleset);
        WriteRuleSources(writer, artifact.RuleSources);
        WriteSnippets(writer, artifact.Snippets);
        WriteIntegrity(writer, artifact.Integrity);
        writer.WriteEndObject();
    }

    private static void WriteCompiler(Utf8JsonWriter writer, CompiledRulesCompilerIdentity compiler)
    {
        writer.WriteStartObject("compiler");
        writer.WriteString("contractVersion", compiler.ContractVersion);
        writer.WriteString("implementationId", compiler.ImplementationId);
        writer.WriteString("implementationVersion", compiler.ImplementationVersion);
        writer.WriteEndObject();
    }

    private static void WriteRuleset(Utf8JsonWriter writer, CompiledRulesRulesetIdentity ruleset)
    {
        writer.WriteStartObject("ruleset");
        writer.WriteString("rulesetId", ruleset.RulesetId);
        writer.WriteString("repositoryVersion", ruleset.RepositoryVersion);
        writer.WriteStartObject("source");
        writer.WriteString("scheme", ruleset.Source.Scheme);
        writer.WriteString("value", ruleset.Source.Value);
        writer.WriteEndObject();
        writer.WriteString("manifestPath", ruleset.ManifestPath);
        writer.WriteString("manifestSha256", ruleset.ManifestSha256);
        writer.WriteEndObject();
    }

    private static void WriteRuleSources(
        Utf8JsonWriter writer,
        IList<CompiledRulesArtifactRuleSource> sources)
    {
        writer.WriteStartArray("ruleSources");
        foreach (var source in sources)
        {
            writer.WriteStartObject();
            writer.WriteString("ruleSourceId", source.RuleSourceId);
            writer.WriteString("sourcePath", source.SourcePath);
            writer.WriteString("sourceSha256", source.SourceSha256);
            WriteApplicability(writer, source.Applicability);
            WriteStrings(writer, "dependencyRuleSourceIds", source.DependencyRuleSourceIds);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private static void WriteApplicability(
        Utf8JsonWriter writer,
        CompiledRulesArtifactApplicability applicability)
    {
        writer.WriteStartObject("applicability");
        writer.WriteString("layer", applicability.Layer.ToString());
        WriteStrings(writer, "worldModelIds", applicability.WorldModelIds);
        WriteStrings(writer, "moduleIds", applicability.ModuleIds);
        WriteStrings(writer, "campaignModes", applicability.CampaignModes);
        WriteStrings(writer, "operations", applicability.Operations);
        WriteStrings(writer, "topics", applicability.Topics);
        writer.WriteNumber("priority", applicability.Priority);
        writer.WriteBoolean("alwaysInclude", applicability.AlwaysInclude);
        writer.WriteString("preparationTier", applicability.PreparationTier.ToString());
        writer.WriteEndObject();
    }

    private static void WriteSnippets(
        Utf8JsonWriter writer,
        IList<CompiledRulesArtifactSnippet> snippets)
    {
        writer.WriteStartArray("snippets");
        foreach (var snippet in snippets)
        {
            writer.WriteStartObject();
            writer.WriteString("snippetId", snippet.SnippetId);
            writer.WriteString("ruleSourceId", snippet.RuleSourceId);
            if (snippet.SourceAnchor is null)
            {
                writer.WriteNull("sourceAnchor");
            }
            else
            {
                writer.WriteString("sourceAnchor", snippet.SourceAnchor);
            }
            writer.WriteString("content", snippet.Content);
            writer.WriteString("contentSha256", snippet.ContentSha256);
            writer.WriteNumber("estimatedTokens", snippet.EstimatedTokens);
            WriteRetrieval(writer, snippet.Retrieval);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private static void WriteRetrieval(Utf8JsonWriter writer, CompiledRulesRetrievalMetadata retrieval)
    {
        writer.WriteStartObject("retrieval");
        writer.WriteStartArray("terms");
        foreach (var term in retrieval.Terms)
        {
            writer.WriteStartObject();
            writer.WriteString("term", term.Term);
            writer.WriteString("kind", term.Kind);
            writer.WriteNumber("weight", term.Weight);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();

        writer.WriteStartArray("relationships");
        foreach (var relationship in retrieval.Relationships)
        {
            writer.WriteStartObject();
            writer.WriteString("fromTerm", relationship.FromTerm);
            writer.WriteString("toTerm", relationship.ToTerm);
            writer.WriteString("kind", relationship.Kind);
            writer.WriteNumber("weight", relationship.Weight);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static void WriteIntegrity(Utf8JsonWriter writer, CompiledRulesArtifactIntegrity integrity)
    {
        writer.WriteStartObject("integrity");
        writer.WriteString("algorithm", integrity.Algorithm);
        writer.WriteString("artifactSha256", integrity.ArtifactSha256);
        writer.WriteEndObject();
    }

    private static void WriteStrings(Utf8JsonWriter writer, string propertyName, IList<string> values)
    {
        writer.WriteStartArray(propertyName);
        foreach (var value in values)
        {
            writer.WriteStringValue(value);
        }
        writer.WriteEndArray();
    }
}
