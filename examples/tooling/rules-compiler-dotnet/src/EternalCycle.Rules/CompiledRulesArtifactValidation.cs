using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace EternalCycle.Rules;

public interface ICompiledRulesArtifactTrustPolicy
{
    ValueTask<CompiledRulesArtifactTrustDecision> EvaluateAsync(
        ValidatedCompiledRulesArtifact artifact,
        CancellationToken cancellationToken);
}

public enum CompiledRulesArtifactTrustOutcome { Approved, Rejected, Failed }

public enum CompiledRulesArtifactTrustReason
{
    ExplicitApproval,
    ExplicitRejection,
    ExpectedByteHashMatched,
    ExpectedByteHashMismatch,
    PolicyUnavailable,
    PolicyException,
    InvalidPolicyDecision
}

public sealed class CompiledRulesArtifactTrustDecision
{
    public CompiledRulesArtifactTrustDecision(CompiledRulesArtifactTrustReason reason)
    {
        Reason = reason;
        Outcome = reason switch
        {
            CompiledRulesArtifactTrustReason.ExplicitApproval or CompiledRulesArtifactTrustReason.ExpectedByteHashMatched => CompiledRulesArtifactTrustOutcome.Approved,
            CompiledRulesArtifactTrustReason.ExplicitRejection or CompiledRulesArtifactTrustReason.ExpectedByteHashMismatch => CompiledRulesArtifactTrustOutcome.Rejected,
            CompiledRulesArtifactTrustReason.PolicyUnavailable or CompiledRulesArtifactTrustReason.PolicyException or CompiledRulesArtifactTrustReason.InvalidPolicyDecision => CompiledRulesArtifactTrustOutcome.Failed,
            _ => throw new ArgumentOutOfRangeException(nameof(reason))
        };
    }

    public CompiledRulesArtifactTrustOutcome Outcome { get; }
    public CompiledRulesArtifactTrustReason Reason { get; }
    public override string ToString() => $"{Outcome}: {Reason}";
}

public sealed class ValidatedCompiledRulesArtifact
{
    private readonly byte[] bytes;

    internal ValidatedCompiledRulesArtifact(byte[] bytes, string byteSha256, CompiledRulesArtifact artifact, CompiledRulesAcquisitionEvidence evidence)
    {
        this.bytes = bytes;
        ByteSha256 = byteSha256;
        Artifact = Freeze(artifact);
        AcquisitionEvidence = evidence;
    }

    public string ByteSha256 { get; }
    public string SemanticSha256 => Artifact.Integrity.ArtifactSha256;
    // The complete model/evidence is available to policy/import code, not an
    // implicit diagnostic payload when this wrapper is serialized or logged.
    [JsonIgnore] public CompiledRulesArtifact Artifact { get; }
    [JsonIgnore] public CompiledRulesAcquisitionEvidence AcquisitionEvidence { get; }

    // ReadOnlyMemory over an array can be unwrapped with MemoryMarshal. Never
    // expose the approved buffer; callers explicitly request an independent copy.
    public byte[] CopyBytes() => bytes.ToArray();
    public override string ToString() => nameof(ValidatedCompiledRulesArtifact);

    // FR-022 DTOs intentionally support assembly through IList. Freeze every
    // collection once without reserialization, normalization, or provenance loss.
    private static CompiledRulesArtifact Freeze(CompiledRulesArtifact artifact) => new()
    {
        ArtifactFormatVersion = artifact.ArtifactFormatVersion,
        Compiler = artifact.Compiler,
        Ruleset = artifact.Ruleset,
        Integrity = artifact.Integrity,
        RuleSources = Array.AsReadOnly(artifact.RuleSources.Select(source => new CompiledRulesArtifactRuleSource
        {
            RuleSourceId = source.RuleSourceId,
            SourcePath = source.SourcePath,
            SourceSha256 = source.SourceSha256,
            DependencyRuleSourceIds = Array.AsReadOnly(source.DependencyRuleSourceIds.ToArray()),
            Applicability = new()
            {
                Layer = source.Applicability.Layer,
                WorldModelIds = Array.AsReadOnly(source.Applicability.WorldModelIds.ToArray()),
                ModuleIds = Array.AsReadOnly(source.Applicability.ModuleIds.ToArray()),
                CampaignModes = Array.AsReadOnly(source.Applicability.CampaignModes.ToArray()),
                Operations = Array.AsReadOnly(source.Applicability.Operations.ToArray()),
                Topics = Array.AsReadOnly(source.Applicability.Topics.ToArray()),
                Priority = source.Applicability.Priority,
                AlwaysInclude = source.Applicability.AlwaysInclude,
                PreparationTier = source.Applicability.PreparationTier
            }
        }).ToArray()),
        Snippets = Array.AsReadOnly(artifact.Snippets.Select(snippet => new CompiledRulesArtifactSnippet
        {
            SnippetId = snippet.SnippetId,
            RuleSourceId = snippet.RuleSourceId,
            SourceAnchor = snippet.SourceAnchor,
            Content = snippet.Content,
            ContentSha256 = snippet.ContentSha256,
            EstimatedTokens = snippet.EstimatedTokens,
            Retrieval = new()
            {
                Terms = Array.AsReadOnly(snippet.Retrieval.Terms.ToArray()),
                Relationships = Array.AsReadOnly(snippet.Retrieval.Relationships.ToArray())
            }
        }).ToArray())
    };
}

public sealed class ValidatedTrustedCompiledRulesArtifact
{
    internal ValidatedTrustedCompiledRulesArtifact(ValidatedCompiledRulesArtifact validated, CompiledRulesArtifactTrustDecision decision)
    {
        Validated = validated;
        TrustDecision = decision;
    }

    public string ByteSha256 => Validated.ByteSha256;
    public string SemanticSha256 => Validated.SemanticSha256;
    public CompiledRulesArtifactTrustDecision TrustDecision { get; }
    [JsonIgnore] public ValidatedCompiledRulesArtifact Validated { get; }
    [JsonIgnore] public CompiledRulesArtifact Artifact => Validated.Artifact;
    [JsonIgnore] public CompiledRulesAcquisitionEvidence AcquisitionEvidence => Validated.AcquisitionEvidence;
    public byte[] CopyBytes() => Validated.CopyBytes();
    public override string ToString() => nameof(ValidatedTrustedCompiledRulesArtifact);
}

public sealed class CompiledRulesArtifactValidationResult
{
    internal CompiledRulesArtifactValidationResult(
        ValidatedCompiledRulesArtifact? validated = null,
        CompiledRulesArtifactTrustDecision? decision = null,
        CompiledRulesAcquisitionFailure? failure = null,
        IReadOnlyList<CompiledRulesArtifactValidationError>? diagnostics = null,
        bool diagnosticsTruncated = false)
    {
        ValidatedArtifact = validated;
        TrustDecision = decision;
        Failure = failure;
        if (failure is { } category)
        {
            var error = new CompiledRulesAcquisitionException(category);
            Code = error.Code;
            Message = error.Message;
        }
        if (validated is not null && decision?.Outcome == CompiledRulesArtifactTrustOutcome.Approved && failure is null)
        {
            TrustedArtifact = new(validated, decision);
        }
        Diagnostics = diagnostics ?? Array.AsReadOnly(Array.Empty<CompiledRulesArtifactValidationError>());
        DiagnosticsTruncated = diagnosticsTruncated;
    }

    public bool IsArtifactValid => ValidatedArtifact is not null;
    public bool IsImportEligible => TrustedArtifact is not null;
    public CompiledRulesAcquisitionFailure? Failure { get; }
    public string? Code { get; }
    public string? Message { get; }
    public CompiledRulesArtifactTrustDecision? TrustDecision { get; }
    public IReadOnlyList<CompiledRulesArtifactValidationError> Diagnostics { get; }
    public bool DiagnosticsTruncated { get; }
    [JsonIgnore] public ValidatedCompiledRulesArtifact? ValidatedArtifact { get; }
    [JsonIgnore] public ValidatedTrustedCompiledRulesArtifact? TrustedArtifact { get; }
    public override string ToString() => Code ?? (IsImportEligible ? "ARTIFACT_IMPORT_ELIGIBLE" : nameof(CompiledRulesArtifactValidationResult));
}

public static partial class CompiledRulesArtifactValidation
{
    public const int MaximumDiagnostics = 16;
    public const int MaximumDiagnosticPathLength = 256;
    public const int MaximumDiagnosticMessageLength = 256;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static async Task<CompiledRulesArtifactValidationResult> ValidateAsync(
        AcquiredCompiledRulesArtifact acquired,
        CompiledRulesAcquisitionLimits limits,
        ICompiledRulesArtifactTrustPolicy trustPolicy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acquired);
        ArgumentNullException.ThrowIfNull(limits);
        ArgumentNullException.ThrowIfNull(trustPolicy);
        cancellationToken.ThrowIfCancellationRequested();
        if (acquired.Bytes.Length > limits.MaximumPayloadBytes)
        {
            return new(failure: CompiledRulesAcquisitionFailure.PayloadTooLarge);
        }

        // Snapshot before hashing/parsing. A's public ReadOnlyMemory is not an
        // immutability guarantee against deliberate backing-array extraction.
        var bytes = acquired.Bytes.ToArray();
        var byteHash = Convert.ToHexString(SHA256.HashData(bytes));
        if (!string.Equals(byteHash, acquired.ByteSha256, StringComparison.Ordinal))
        {
            return new(failure: CompiledRulesAcquisitionFailure.IntegrityMismatch);
        }

        string json;
        try { json = StrictUtf8.GetString(bytes); }
        catch (DecoderFallbackException)
        {
            return Malformed("UTF8_INVALID", "Artifact bytes are not valid UTF-8.");
        }
        if (string.IsNullOrWhiteSpace(json))
        {
            return Malformed("JSON_INVALID", "The acquired artifact is not valid contract JSON.");
        }

        CompiledRulesArtifactReadResult read;
        try { read = CompiledRulesArtifactContract.Read(json); }
        catch (Exception exception) when (exception is not OperationCanceledException and not OutOfMemoryException)
        {
            // Unexpected validator failure is never approval or a raw exception
            // diagnostic. Ordinary invalid input uses the FR-022 errors below.
            return new(failure: CompiledRulesAcquisitionFailure.ValidationFailed);
        }
        cancellationToken.ThrowIfCancellationRequested();
        if (!read.IsValid)
        {
            return new(
                failure: Classify(read.Errors),
                diagnostics: Array.AsReadOnly(read.Errors.Take(MaximumDiagnostics).Select(Sanitize).ToArray()),
                diagnosticsTruncated: read.Errors.Count > MaximumDiagnostics);
        }

        var validated = new ValidatedCompiledRulesArtifact(bytes, byteHash, read.Artifact!, acquired.Evidence);
        CompiledRulesArtifactTrustDecision decision;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var evaluation = trustPolicy.EvaluateAsync(validated, cancellationToken);
            // Completed policies need no Task allocation. For asynchronous third-
            // party policies, cancellation can stop waiting even if they ignore it.
            decision = (evaluation.IsCompletedSuccessfully
                ? evaluation.Result
                : await evaluation.AsTask().WaitAsync(cancellationToken))
                ?? new(CompiledRulesArtifactTrustReason.InvalidPolicyDecision);
        }
        catch (OperationCanceledException cancellation)
        {
            throw new OperationCanceledException("Artifact trust-policy evaluation was cancelled.", cancellation.CancellationToken);
        }
        catch (Exception)
        {
            decision = new(CompiledRulesArtifactTrustReason.PolicyException);
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(validated, decision, decision.Outcome switch
        {
            CompiledRulesArtifactTrustOutcome.Approved => null,
            CompiledRulesArtifactTrustOutcome.Rejected => CompiledRulesAcquisitionFailure.TrustRejected,
            _ => CompiledRulesAcquisitionFailure.TrustPolicyFailed
        });
    }

    private static CompiledRulesArtifactValidationResult Malformed(string code, string message) => new(
        failure: CompiledRulesAcquisitionFailure.MalformedArtifact,
        diagnostics: Array.AsReadOnly(new[] { new CompiledRulesArtifactValidationError(code, "$", message) }));

    // Classification only consumes existing FR-022 codes; it adds no validity
    // rules. This precedence remains deterministic when several errors coexist.
    private static CompiledRulesAcquisitionFailure Classify(IReadOnlyList<CompiledRulesArtifactValidationError> errors)
    {
        if (errors.Any(error => error.Code is "JSON_INVALID" or "DUPLICATE_JSON_PROPERTY"))
            return CompiledRulesAcquisitionFailure.MalformedArtifact;
        if (errors.Any(error => error.Code is "FORMAT_VERSION_UNSUPPORTED" or "COMPILER_CONTRACT_UNSUPPORTED"))
            return CompiledRulesAcquisitionFailure.UnsupportedArtifactFormat;
        if (errors.Any(error => error.Code is "SHA256_INVALID" or "CONTENT_HASH_MISMATCH" or "ARTIFACT_HASH_MISMATCH" or "INTEGRITY_ALGORITHM_UNSUPPORTED"))
            return CompiledRulesAcquisitionFailure.IntegrityMismatch;
        return CompiledRulesAcquisitionFailure.SemanticValidationFailed;
    }

    private static CompiledRulesArtifactValidationError Sanitize(CompiledRulesArtifactValidationError error) => new(
        error.Code,
        error.Path.Length <= MaximumDiagnosticPathLength && SafePath().IsMatch(error.Path) ? error.Path : "$",
        error.Message.Length <= MaximumDiagnosticMessageLength && !error.Message.Any(char.IsControl)
            ? error.Message : "Artifact contract validation failed.");

    // Reject dynamic JSON property names rather than truncating a secret into a
    // diagnostic. Only format-1 field names and bounded numeric indices survive.
    [GeneratedRegex(@"^\$(?:\.(?:artifactFormatVersion|compiler|contractVersion|implementationId|implementationVersion|ruleset|rulesetId|repositoryVersion|source|scheme|value|manifestPath|manifestSha256|ruleSources|ruleSourceId|sourcePath|sourceSha256|applicability|layer|worldModelIds|moduleIds|campaignModes|operations|topics|priority|alwaysInclude|preparationTier|dependencyRuleSourceIds|snippets|snippetId|sourceAnchor|content|contentSha256|estimatedTokens|retrieval|terms|term|kind|weight|relationships|fromTerm|toTerm|integrity|algorithm|artifactSha256)(?:\[[0-9]{1,10}\])?)*$", RegexOptions.CultureInvariant | RegexOptions.NonBacktracking)]
    private static partial Regex SafePath();
}
