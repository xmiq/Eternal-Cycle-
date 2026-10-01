using System.Buffers;
using System.Collections.ObjectModel;
using System.Security.Cryptography;

namespace EternalCycle.Rules;

// Providers are configured outside this contract. Locators, credentials, and
// transport options do not belong in portable artifact or import identity.
public interface ICompiledRulesArtifactProvider
{
    Task<AcquiredCompiledRulesArtifact> AcquireAsync(
        CompiledRulesAcquisitionLimits limits,
        CancellationToken cancellationToken);
}

public sealed class CompiledRulesAcquisitionLimits
{
    public const int DefaultMaximumPayloadBytes = 16 * 1024 * 1024;

    public CompiledRulesAcquisitionLimits(int maximumPayloadBytes = DefaultMaximumPayloadBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumPayloadBytes);
        MaximumPayloadBytes = maximumPayloadBytes;
    }

    public int MaximumPayloadBytes { get; }
}

public sealed class CompiledRulesAcquisitionIdentity
{
    public CompiledRulesAcquisitionIdentity(string scheme, string value)
    {
        CompiledRulesAcquisitionEvidence.CheckText(scheme, 128);
        CompiledRulesAcquisitionEvidence.CheckText(value, 1024);
        Scheme = scheme;
        Value = value;
    }

    public string Scheme { get; }
    public string Value { get; }

    public override string ToString() => nameof(CompiledRulesAcquisitionIdentity);
}

public sealed class CompiledRulesAcquisitionEvidence
{
    public const int MaximumMetadataEntries = 16;
    public const int MaximumMetadataKeyLength = 128;
    public const int MaximumMetadataValueLength = 1024;

    public CompiledRulesAcquisitionEvidence(
        string providerKind,
        CompiledRulesAcquisitionIdentity? resolvedIdentity = null,
        IEnumerable<KeyValuePair<string, string>>? metadata = null)
    {
        CheckText(providerKind, 128);
        ProviderKind = providerKind;
        ResolvedIdentity = resolvedIdentity;
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in metadata ?? [])
        {
            CheckText(pair.Key, MaximumMetadataKeyLength);
            CheckText(pair.Value, MaximumMetadataValueLength);
            if (values.Count == MaximumMetadataEntries || !values.TryAdd(pair.Key, pair.Value))
            {
                throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
            }
        }
        Metadata = new ReadOnlyDictionary<string, string>(values);
    }

    public string ProviderKind { get; }
    // This is provider-supplied evidence, not publisher authentication or a
    // replacement for the source identity inside the eventually validated artifact.
    public CompiledRulesAcquisitionIdentity? ResolvedIdentity { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    // Evidence is untrusted data, not a safe diagnostic message. Do not echo
    // provider values implicitly when this object appears in a log interpolation.
    public override string ToString() => nameof(CompiledRulesAcquisitionEvidence);

    internal static void CheckText(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximumLength || value.Any(char.IsControl))
        {
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
        }
    }
}

public sealed class AcquiredCompiledRulesArtifact
{
    private readonly byte[] bytes;

    public AcquiredCompiledRulesArtifact(
        ReadOnlySpan<byte> payload,
        CompiledRulesAcquisitionEvidence evidence,
        CompiledRulesAcquisitionLimits limits)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(limits);
        if (payload.Length > limits.MaximumPayloadBytes)
        {
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.PayloadTooLarge);
        }
        // Own the acquired bytes so changes to the provider's input buffer cannot
        // silently invalidate the recorded byte identity before shared validation.
        bytes = payload.ToArray();
        ByteSha256 = Convert.ToHexString(SHA256.HashData(bytes));
        Evidence = evidence;
    }

    public ReadOnlyMemory<byte> Bytes => bytes;
    public string ByteSha256 { get; }
    public CompiledRulesAcquisitionEvidence Evidence { get; }

    public override string ToString() => nameof(AcquiredCompiledRulesArtifact);

    public static async Task<AcquiredCompiledRulesArtifact> ReadAsync(
        Stream stream,
        CompiledRulesAcquisitionEvidence evidence,
        CompiledRulesAcquisitionLimits limits,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(limits);
        cancellationToken.ThrowIfCancellationRequested();
        if (!stream.CanRead)
        {
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
        }

        var buffer = ArrayPool<byte>.Shared.Rent(8192);
        try
        {
            using var payload = new MemoryStream(Math.Min(8192, limits.MaximumPayloadBytes));
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var remaining = limits.MaximumPayloadBytes - (int)payload.Length;
                // Probe only one extra byte at the limit; neither Content-Length
                // nor a seekable stream length can authorize unbounded buffering.
                var count = await stream.ReadAsync(
                    buffer.AsMemory(0, remaining == 0 ? 1 : Math.Min(8192, remaining)),
                    cancellationToken);
                if (count == 0)
                {
                    break;
                }
                if (count > remaining)
                {
                    throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.PayloadTooLarge);
                }
                payload.Write(buffer, 0, count);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(payload.GetBuffer().AsSpan(0, (int)payload.Length), evidence, limits);
        }
        catch (IOException)
        {
            // Raw transport exceptions can contain locators, headers, or secrets.
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.TransportFailed);
        }
        finally
        {
            // Do not retain untrusted transport contents in the shared buffer pool.
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }
}

public enum CompiledRulesAcquisitionFailure
{
    ProviderConfigurationInvalid,
    LocatorInvalid,
    Unavailable,
    TransportFailed,
    PayloadTooLarge,
    IntegrityMismatch,
    MalformedArtifact,
    UnsupportedArtifactFormat,
    SemanticValidationFailed,
    TrustRejected,
    ImportConflict,
    StorageFailed,
    TrustPolicyFailed,
    ValidationFailed
}

public sealed class CompiledRulesAcquisitionException : Exception
{
    public CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure failure) : this(failure, Describe(failure)) { }

    private CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure failure, (string Code, string Message) detail) : base(detail.Message)
    {
        Failure = failure;
        Code = detail.Code;
    }

    public CompiledRulesAcquisitionFailure Failure { get; }
    public string Code { get; }

    private static (string Code, string Message) Describe(CompiledRulesAcquisitionFailure failure) => failure switch
    {
        CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid => ("ARTIFACT_PROVIDER_CONFIGURATION_INVALID", "Artifact acquisition configuration or evidence is invalid."),
        CompiledRulesAcquisitionFailure.LocatorInvalid => ("ARTIFACT_LOCATOR_INVALID", "The artifact locator is invalid."),
        CompiledRulesAcquisitionFailure.Unavailable => ("ARTIFACT_UNAVAILABLE", "The requested artifact is unavailable or was not found."),
        CompiledRulesAcquisitionFailure.TransportFailed => ("ARTIFACT_TRANSPORT_FAILED", "Artifact byte acquisition failed."),
        CompiledRulesAcquisitionFailure.PayloadTooLarge => ("ARTIFACT_PAYLOAD_TOO_LARGE", "The artifact exceeds the configured byte limit."),
        CompiledRulesAcquisitionFailure.IntegrityMismatch => ("ARTIFACT_INTEGRITY_MISMATCH", "Artifact integrity verification failed."),
        CompiledRulesAcquisitionFailure.MalformedArtifact => ("ARTIFACT_MALFORMED", "The acquired artifact is malformed."),
        CompiledRulesAcquisitionFailure.UnsupportedArtifactFormat => ("ARTIFACT_FORMAT_UNSUPPORTED", "The artifact format or compiler contract is unsupported."),
        CompiledRulesAcquisitionFailure.SemanticValidationFailed => ("ARTIFACT_SEMANTIC_INVALID", "Artifact semantic validation failed."),
        CompiledRulesAcquisitionFailure.TrustRejected => ("ARTIFACT_TRUST_REJECTED", "Installation policy does not permit this artifact."),
        CompiledRulesAcquisitionFailure.ImportConflict => ("ARTIFACT_IMPORT_CONFLICT", "The artifact conflicts with existing imported state."),
        CompiledRulesAcquisitionFailure.StorageFailed => ("ARTIFACT_STORAGE_FAILED", "Validated artifact storage failed."),
        CompiledRulesAcquisitionFailure.TrustPolicyFailed => ("ARTIFACT_TRUST_POLICY_FAILED", "Artifact trust-policy evaluation could not complete."),
        CompiledRulesAcquisitionFailure.ValidationFailed => ("ARTIFACT_VALIDATION_FAILED", "Artifact validation could not complete."),
        _ => throw new ArgumentOutOfRangeException(nameof(failure))
    };
}
