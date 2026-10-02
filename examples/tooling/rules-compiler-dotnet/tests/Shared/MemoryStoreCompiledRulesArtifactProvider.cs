namespace EternalCycle.Rules.Testing;

// Compiled in both test assemblies, including one without friend access. Only
// public acquisition APIs belong here; storage/stream setup is fixture-owned.
internal sealed class MemoryStoreCompiledRulesArtifactProvider : ICompiledRulesArtifactProvider
{
    private readonly Func<Stream>? openStream;
    private readonly CompiledRulesAcquisitionEvidence evidence;

    public MemoryStoreCompiledRulesArtifactProvider(string objectKey, string objectVersion,
        Func<Stream>? openStream, IEnumerable<KeyValuePair<string, string>>? metadata = null)
    {
        evidence = new("conformance-memory-store", new("fixture-object-version", objectVersion),
            (metadata ?? []).Prepend(KeyValuePair.Create("logicalObjectKey", objectKey)));
        this.openStream = openStream;
    }

    public async Task<AcquiredCompiledRulesArtifact> AcquireAsync(
        CompiledRulesAcquisitionLimits limits, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(limits);
        cancellationToken.ThrowIfCancellationRequested();
        if (openStream is null) throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.Unavailable);
        try
        {
            // A leaves streams open. The provider owns disposal and uses A's
            // copy/hash/bounds rather than trusting stream length or metadata.
            await using var stream = openStream();
            return await AcquiredCompiledRulesArtifact.ReadAsync(stream, evidence, limits, cancellationToken);
        }
        catch (IOException)
        {
            // Open failures, like A's read failures, must not echo raw locators.
            throw new CompiledRulesAcquisitionException(CompiledRulesAcquisitionFailure.TransportFailed);
        }
        catch (OperationCanceledException cancelled)
        {
            throw new OperationCanceledException("Artifact acquisition was cancelled.",
                cancellationToken.IsCancellationRequested ? cancellationToken : cancelled.CancellationToken);
        }
    }
}
