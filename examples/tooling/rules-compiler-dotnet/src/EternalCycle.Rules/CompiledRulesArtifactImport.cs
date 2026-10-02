using System.Security.Cryptography;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace EternalCycle.Rules;

// A configured store is an authorization boundary, not an acquisition provider.
// Every write surface accepts only the approval wrapper that B can construct.
public interface ICompiledRulesArtifactImportStore
{
    Task<CompiledRulesImportReceipt> ImportAsync(ValidatedTrustedCompiledRulesArtifact approved, CancellationToken cancellationToken);
    Task<StoredCompiledRulesArtifact?> ReadAsync(string importId, CancellationToken cancellationToken);
}

public sealed record CompiledRulesImportReceipt(
    string ImportId, string RulesetId, string SemanticSha256, string RetainedByteSha256,
    bool Created, int SourceCount, int SnippetCount, int RetrievalTermCount, int RetrievalRelationshipCount);

public enum CompiledRulesImportFailure { InputInvalid, IntegrityConflict, SchemaIncompatible, StorageFailed }

public sealed class CompiledRulesImportException : Exception
{
    public CompiledRulesImportException(CompiledRulesImportFailure failure) : base(failure switch
    {
        CompiledRulesImportFailure.InputInvalid => "The approved artifact does not match the authorized import scope.",
        CompiledRulesImportFailure.IntegrityConflict => "Stored artifact integrity or semantic content conflicts with the approved artifact.",
        CompiledRulesImportFailure.SchemaIncompatible => "The configured rule storage requires a supported import migration.",
        CompiledRulesImportFailure.StorageFailed => "The artifact storage transaction or verification failed.",
        _ => throw new ArgumentOutOfRangeException(nameof(failure))
    })
    {
        Failure = failure;
        Code = failure switch
        {
            CompiledRulesImportFailure.InputInvalid => "ARTIFACT_IMPORT_INPUT_INVALID",
            CompiledRulesImportFailure.IntegrityConflict => "ARTIFACT_IMPORT_CONFLICT",
            CompiledRulesImportFailure.SchemaIncompatible => "ARTIFACT_IMPORT_SCHEMA_INCOMPATIBLE",
            _ => "ARTIFACT_STORAGE_FAILED"
        };
    }

    public CompiledRulesImportFailure Failure { get; }
    public string Code { get; }
}

public sealed class StoredCompiledRulesArtifact
{
    private readonly ValidatedCompiledRulesArtifact validated;
    internal StoredCompiledRulesArtifact(string importId, ValidatedCompiledRulesArtifact validated)
    {
        ImportId = importId;
        this.validated = validated;
    }

    public string ImportId { get; }
    public string ByteSha256 => validated.ByteSha256;
    public string SemanticSha256 => validated.SemanticSha256;
    [JsonIgnore] public CompiledRulesArtifact Artifact => validated.Artifact;
    public byte[] CopyBytes() => validated.CopyBytes();
    public override string ToString() => nameof(StoredCompiledRulesArtifact);
}

public static class CompiledRulesArtifactImport
{
    public static async Task<CompiledRulesImportReceipt> ImportAsync(
        ValidatedTrustedCompiledRulesArtifact approved, string authorizedRulesetId,
        ICompiledRulesArtifactImportStore store, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        RequireScope(approved, authorizedRulesetId);
        cancellationToken.ThrowIfCancellationRequested();
        try { return await store.ImportAsync(approved, cancellationToken); }
        catch (CompiledRulesImportException) { throw; }
        catch (OperationCanceledException)
        {
            throw new OperationCanceledException("Artifact import was cancelled.", cancellationToken);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            throw new CompiledRulesImportException(CompiledRulesImportFailure.StorageFailed);
        }
    }

    public static void RequireScope(ValidatedTrustedCompiledRulesArtifact approved, string authorizedRulesetId)
    {
        if (approved is null || string.IsNullOrWhiteSpace(authorizedRulesetId) ||
            !string.Equals(approved.Artifact.Ruleset.RulesetId, authorizedRulesetId, StringComparison.Ordinal))
            throw new CompiledRulesImportException(CompiledRulesImportFailure.InputInvalid);
    }

    public static string CreateImportId(CompiledRulesArtifact artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);
        // Length framing separates Ruleset scope from semantic digest. Neither a
        // transport locator nor an observation timestamp contributes to identity.
        var ruleset = artifact.Ruleset.RulesetId;
        var length = Encoding.UTF8.GetByteCount(ruleset).ToString(CultureInfo.InvariantCulture);
        var identity = $"EC-IMPORT-1:{length}:{ruleset}:{artifact.Integrity.ArtifactSha256}";
        return "ARTIFACT-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    public static CompiledRulesImportReceipt Receipt(StoredCompiledRulesArtifact stored, bool created) => new(
        stored.ImportId, stored.Artifact.Ruleset.RulesetId, stored.SemanticSha256, stored.ByteSha256, created,
        stored.Artifact.RuleSources.Count, stored.Artifact.Snippets.Count,
        stored.Artifact.Snippets.Sum(snippet => snippet.Retrieval.Terms.Count),
        stored.Artifact.Snippets.Sum(snippet => snippet.Retrieval.Relationships.Count));

    // Readback is verification, not another import or trust entry point. Compare
    // the complete canonical semantic projection, never only a digest-key match.
    public static StoredCompiledRulesArtifact VerifyReadback(string importId, string reconstructedJson, byte[] exactBytes, string byteSha256)
    {
        var read = CompiledRulesArtifactContract.Read(reconstructedJson);
        if (!read.IsValid || CreateImportId(read.Artifact!) != importId ||
            Convert.ToHexString(SHA256.HashData(exactBytes)) != byteSha256)
            throw new CompiledRulesImportException(CompiledRulesImportFailure.IntegrityConflict);
        CompiledRulesArtifactReadResult original;
        try { original = CompiledRulesArtifactContract.Read(new UTF8Encoding(false, true).GetString(exactBytes)); }
        catch (DecoderFallbackException) { throw new CompiledRulesImportException(CompiledRulesImportFailure.IntegrityConflict); }
        if (!original.IsValid || !Equivalent(read.Artifact!, original.Artifact!))
            throw new CompiledRulesImportException(CompiledRulesImportFailure.IntegrityConflict);
        return new(importId, new(exactBytes.ToArray(), byteSha256, read.Artifact!, new("stored-artifact")));
    }

    public static bool Equivalent(CompiledRulesArtifact first, CompiledRulesArtifact second)
    {
        var left = CompiledRulesArtifactWriter.Write(first);
        var right = CompiledRulesArtifactWriter.Write(second);
        return left.IsValid && right.IsValid && left.Bytes.Span.SequenceEqual(right.Bytes.Span);
    }
}
