using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EternalCycle.Rules.Testing;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class CompiledRulesArtifactWriterTests(ITestOutputHelper output)
{
    [Fact]
    public void MinimalInputProducesValidFormatOneArtifact()
    {
        var compiled = Compile(Snapshot(Source("core", "# Core\n\nA rule.")));

        Assert.True(compiled.Assembly.IsValid);
        Assert.True(compiled.Write.IsValid);
        Assert.Empty(CompiledRulesArtifactContract.Validate(compiled.Artifact));
        Assert.Equal(1, compiled.Artifact.ArtifactFormatVersion);
        Assert.Single(compiled.Artifact.RuleSources);
        Assert.Single(compiled.Artifact.Snippets);
    }

    [Fact]
    public void RepresentativeMultiSourceInputProducesValidArtifact()
    {
        var root = Source("runtime", "# Runtime\n\nAuthority.", priority: 1000, alwaysInclude: true);
        var dependent = Source("development", "## Progress\nEarned progress.", dependencies: ["runtime"]);
        var compiled = Compile(Snapshot(root, dependent));

        Assert.Empty(CompiledRulesArtifactContract.Validate(compiled.Artifact));
        Assert.Equal(2, compiled.Artifact.RuleSources.Count);
        Assert.Equal(["runtime"], compiled.Artifact.RuleSources.Single(source => source.RuleSourceId == "development").DependencyRuleSourceIds);
    }

    [Fact]
    public void AssemblyMapsSnapshotIdentityProvenanceAndApplicability()
    {
        var entry = new RuleSourceManifestEntry
        {
            RuleSourceId = "world-rules",
            Path = "rules/world.md",
            Layer = RuleLayer.World,
            WorldModelIds = ["world-b", "world-a"],
            CampaignModes = ["NORMAL", "FIRST_LIFE"],
            Operations = ["world.advance", "gameplay.resolve"],
            Topics = ["weather", "history"],
            Priority = 45,
            AlwaysInclude = true,
            PreparationTier = RulePreparationTier.CampaignRelevant
        };
        var snapshot = Snapshot(new TestSource(entry, "## World\nRule."));
        var artifact = Compile(snapshot).Artifact;
        var source = Assert.Single(artifact.RuleSources);

        Assert.Equal(snapshot.CompilerIdentity.ContractVersion, artifact.Compiler.ContractVersion);
        Assert.Equal(snapshot.CompilerIdentity.ImplementationId, artifact.Compiler.ImplementationId);
        Assert.Equal(snapshot.CompilerIdentity.ImplementationVersion, artifact.Compiler.ImplementationVersion);
        Assert.Equal(snapshot.Manifest.RulesetId, artifact.Ruleset.RulesetId);
        Assert.Equal(snapshot.Manifest.RepositoryVersion, artifact.Ruleset.RepositoryVersion);
        Assert.Equal(snapshot.SourceIdentity.Scheme, artifact.Ruleset.Source.Scheme);
        Assert.Equal(snapshot.SourceIdentity.Value, artifact.Ruleset.Source.Value);
        Assert.Equal(snapshot.ManifestPath, artifact.Ruleset.ManifestPath);
        Assert.Equal(snapshot.ManifestSha256, artifact.Ruleset.ManifestSha256);
        Assert.Equal(snapshot.Sources[0].SourceSha256, source.SourceSha256);
        Assert.Equal(["world-a", "world-b"], source.Applicability.WorldModelIds);
        Assert.Equal(["FIRST_LIFE", "NORMAL"], source.Applicability.CampaignModes);
        Assert.Equal(["gameplay.resolve", "world.advance"], source.Applicability.Operations);
        Assert.Equal(["history", "weather"], source.Applicability.Topics);
        Assert.Equal(45, source.Applicability.Priority);
        Assert.True(source.Applicability.AlwaysInclude);
        Assert.Equal(RulePreparationTier.CampaignRelevant, source.Applicability.PreparationTier);
    }

    [Fact]
    public void CanonicalTenSourcePayloadProducesValid154SnippetArtifact()
    {
        var snapshot = LoadCanonical(FindRepositoryRoot());
        var compiled = Compile(snapshot);

        output.WriteLine("Canonical Rule Sources: {0}", compiled.Artifact.RuleSources.Count);
        output.WriteLine("Canonical snippets: {0}", compiled.Artifact.Snippets.Count);
        output.WriteLine("Semantic digest: {0}", compiled.Artifact.Integrity.ArtifactSha256);
        output.WriteLine("Serialized byte SHA-256: {0}", Sha256(compiled.Write.Bytes.Span));
        Assert.Equal(10, compiled.Artifact.RuleSources.Count);
        Assert.Equal(154, compiled.Artifact.Snippets.Count);
        // Optional vocabulary input must not perturb the legacy absent-vocabulary artifact.
        Assert.Equal("4B583A7904CE514B56CF3B497648DB5D778FC04DF2285D30A2E03823A32EA6A4", compiled.Artifact.Integrity.ArtifactSha256);
        Assert.Equal("802172680FCE33E7F5F9B75EBEBE1D695B702A22B82F1918601DB7B9F44926EC", Sha256(compiled.Write.Bytes.Span));
        Assert.Empty(CompiledRulesArtifactContract.Validate(compiled.Artifact));
    }

    [Fact]
    public void RepeatedAssemblyProducesSameSemanticArtifact()
    {
        var snapshot = Snapshot(Source("repeat", "## One\nRule."));

        var first = Compile(snapshot);
        var second = Compile(snapshot);

        Assert.Equal(first.Artifact.Integrity.ArtifactSha256, second.Artifact.Integrity.ArtifactSha256);
        Assert.Equal(first.Write.Bytes.ToArray(), second.Write.Bytes.ToArray());
    }

    [Fact]
    public void RepeatedSerializationProducesByteIdenticalCompactUtf8()
    {
        var compiled = Compile(Snapshot(Source("writer", "# Writer\n\nRule.")));

        var second = CompiledRulesArtifactWriter.Write(compiled.Artifact);
        var bytes = compiled.Write.Bytes.ToArray();

        Assert.True(second.IsValid);
        Assert.Equal(bytes, second.Bytes.ToArray());
        Assert.False(bytes.AsSpan().StartsWith(new byte[] { 0xEF, 0xBB, 0xBF }));
        Assert.DoesNotContain((byte)'\r', bytes);
        Assert.DoesNotContain((byte)'\n', bytes);
    }

    [Fact]
    public void RelocatedIdenticalPayloadProducesByteIdenticalArtifact()
    {
        var original = LoadCanonical(FindRepositoryRoot());
        using var relocatedRoot = new TemporaryPayload();
        relocatedRoot.WriteBytes(original.ManifestPath, original.AuthoritativeManifestBytes.ToArray());
        foreach (var source in original.Sources)
        {
            relocatedRoot.WriteBytes(source.ManifestEntry.Path, source.AuthoritativeBytes.ToArray());
        }

        var relocated = LoadCanonical(relocatedRoot.Root);
        var originalCompiled = Compile(original);
        var relocatedCompiled = Compile(relocated);

        Assert.Equal(originalCompiled.Artifact.Integrity.ArtifactSha256, relocatedCompiled.Artifact.Integrity.ArtifactSha256);
        Assert.Equal(originalCompiled.Write.Bytes.ToArray(), relocatedCompiled.Write.Bytes.ToArray());
    }

    [Fact]
    public void WorkingDirectoryDoesNotAffectOutput()
    {
        using var payload = CanonicalPayloadCopy();
        using var otherDirectory = new TemporaryPayload();
        var originalDirectory = Environment.CurrentDirectory;
        try
        {
            var first = Compile(LoadCanonical(payload.Root));
            Environment.CurrentDirectory = otherDirectory.Root;
            var second = Compile(LoadCanonical(payload.Root));

            Assert.Equal(first.Write.Bytes.ToArray(), second.Write.Bytes.ToArray());
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
        }
    }

    [Fact]
    public void CurrentCultureDoesNotAffectOutput()
    {
        var snapshot = Snapshot(Source("culture", "## Échos\nRègle."));
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");
            var turkish = Compile(snapshot);
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ja-JP");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ja-JP");
            var japanese = Compile(snapshot);

            Assert.Equal(turkish.Write.Bytes.ToArray(), japanese.Write.Bytes.ToArray());
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void UnorderedCandidateAndRetrievalInsertionDoesNotAffectCanonicalOutput()
    {
        var snapshot = Snapshot(
            Source("one", "## A\nOne."),
            Source("two", "## B\nTwo."));
        var candidates = RuleSnippetCompiler.Compile(snapshot).ToArray();
        candidates[0] = candidates[0] with { Retrieval = Retrieval(reverse: false) };
        var reordered = candidates.Reverse().ToArray();
        reordered[^1] = reordered[^1] with { Retrieval = Retrieval(reverse: true) };

        var first = Compile(snapshot, candidates);
        var second = Compile(snapshot, reordered);

        Assert.Equal(first.Write.Bytes.ToArray(), second.Write.Bytes.ToArray());
    }

    [Fact]
    public void LfAndCrLfPreserveExecutableEquivalenceButDistinctProvenance()
    {
        var lf = Compile(Snapshot(Source("lines", "# Title\n\n## Section\nRule.\n")));
        var crlf = Compile(Snapshot(Source("lines", "# Title\r\n\r\n## Section\r\nRule.\r\n")));

        Assert.Equal(SnippetSemantics(lf.Artifact), SnippetSemantics(crlf.Artifact));
        Assert.NotEqual(lf.Artifact.RuleSources[0].SourceSha256, crlf.Artifact.RuleSources[0].SourceSha256);
        Assert.NotEqual(lf.Artifact.Integrity.ArtifactSha256, crlf.Artifact.Integrity.ArtifactSha256);
        Assert.NotEqual(lf.Write.Bytes.ToArray(), crlf.Write.Bytes.ToArray());
    }

    [Fact]
    public void LfVariantIsIndependentlyByteReproducible()
    {
        var snapshot = Snapshot(Source("lines", "# Title\n\n## Section\nRule.\n"));

        Assert.Equal(Compile(snapshot).Write.Bytes.ToArray(), Compile(snapshot).Write.Bytes.ToArray());
    }

    [Fact]
    public void CrLfVariantIsIndependentlyByteReproducible()
    {
        var snapshot = Snapshot(Source("lines", "# Title\r\n\r\n## Section\r\nRule.\r\n"));

        Assert.Equal(Compile(snapshot).Write.Bytes.ToArray(), Compile(snapshot).Write.Bytes.ToArray());
    }

    [Fact]
    public void OutputContainsNoVolatileOrPhysicalRootData()
    {
        using var payload = CanonicalPayloadCopy();
        var json = Encoding.UTF8.GetString(Compile(LoadCanonical(payload.Root)).Write.Bytes.Span);

        Assert.DoesNotContain(payload.Root, json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Environment.MachineName, json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("generatedAt", json, StringComparison.Ordinal);
        Assert.DoesNotContain("timestamp", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("processId", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EstablishedSemanticDigestValidates()
    {
        var artifact = Compile(Snapshot(Source("digest", "## Digest\nRule."))).Artifact;

        Assert.Equal(CompiledRulesArtifactContract.ComputeArtifactSha256(artifact), artifact.Integrity.ArtifactSha256);
        Assert.Empty(CompiledRulesArtifactContract.Validate(artifact));
    }

    [Fact]
    public void ExecutableContentMutationChangesContentAndArtifactIdentity()
    {
        var original = Compile(Snapshot(Source("identity", "## Stable\nOriginal.")));
        var changed = Compile(Snapshot(Source("identity", "## Stable\nChanged.")));

        Assert.Equal(original.Artifact.Snippets[0].SnippetId, changed.Artifact.Snippets[0].SnippetId);
        Assert.NotEqual(original.Artifact.Snippets[0].ContentSha256, changed.Artifact.Snippets[0].ContentSha256);
        Assert.NotEqual(original.Artifact.Integrity.ArtifactSha256, changed.Artifact.Integrity.ArtifactSha256);
    }

    [Fact]
    public void ExactByteOnlyMutationChangesProvenanceAndArtifactIdentity()
    {
        var plain = Compile(Snapshot(Source("bom", "# Title\nRule.")));
        var withBom = Compile(Snapshot(Source("bom", "\uFEFF# Title\nRule.")));

        Assert.Equal(SnippetSemantics(plain.Artifact), SnippetSemantics(withBom.Artifact));
        Assert.NotEqual(plain.Artifact.RuleSources[0].SourceSha256, withBom.Artifact.RuleSources[0].SourceSha256);
        Assert.NotEqual(plain.Artifact.Integrity.ArtifactSha256, withBom.Artifact.Integrity.ArtifactSha256);
        Assert.NotEqual(plain.Write.Bytes.ToArray(), withBom.Write.Bytes.ToArray());
    }

    [Fact]
    public void TamperedCandidateFailsWithStructuredAssemblyError()
    {
        var snapshot = Snapshot(Source("tamper", "## Stable\nOriginal."));
        var candidates = RuleSnippetCompiler.Compile(snapshot).ToArray();
        candidates[0] = candidates[0] with { Content = "Tampered." };

        var result = CompiledRulesArtifactAssembler.Assemble(snapshot, candidates);

        Assert.False(result.IsValid);
        Assert.Null(result.Artifact);
        Assert.Contains(result.Errors, error => error.Code == "CANDIDATE_MISMATCH");
    }

    [Fact]
    public void InvalidArtifactCannotBeSerializedAsSuccessfulOutput()
    {
        var artifact = Compile(Snapshot(Source("invalid", "Rule."))).Artifact;
        artifact.Snippets[0] = new()
        {
            SnippetId = "invalid",
            RuleSourceId = "invalid",
            SourceAnchor = null,
            Content = "Changed without a matching hash.",
            ContentSha256 = artifact.Snippets[0].ContentSha256,
            EstimatedTokens = 1,
            Retrieval = new()
        };

        var result = CompiledRulesArtifactWriter.Write(artifact);

        Assert.False(result.IsValid);
        Assert.True(result.Bytes.IsEmpty);
        Assert.Contains(result.Errors, error => error.Code == "CONTENT_HASH_MISMATCH");
    }

    [Fact]
    public void SourceDependenciesUseDeterministicFormatOneRepresentation()
    {
        var snapshot = Snapshot(
            Source("a", "A."),
            Source("b", "B."),
            Source("dependent", "D.", dependencies: ["b", "a"]));
        var compiled = Compile(snapshot);

        Assert.Equal(
            ["a", "b"],
            compiled.Artifact.RuleSources.Single(source => source.RuleSourceId == "dependent").DependencyRuleSourceIds);
        Assert.Empty(CompiledRulesArtifactContract.Validate(compiled.Artifact));
    }

    [Fact]
    public void SerializationRoundTripPreservesFormatOneSemanticsAndCanonicalBytes()
    {
        var compiled = Compile(Snapshot(Source("roundtrip", "## Round trip\nRule.")));
        var read = CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(compiled.Write.Bytes.Span));

        Assert.True(read.IsValid);
        Assert.Equal(compiled.Artifact.Integrity.ArtifactSha256, read.Artifact!.Integrity.ArtifactSha256);
        var rewritten = CompiledRulesArtifactWriter.Write(read.Artifact);
        Assert.Equal(compiled.Write.Bytes.ToArray(), rewritten.Bytes.ToArray());
    }

    [Fact]
    public void GeneratedJsonUsesFormatOneSchemaShapeAndPropertyOrder()
    {
        var compiled = Compile(Snapshot(Source("shape", "Rule.")));
        using var document = JsonDocument.Parse(compiled.Write.Bytes);
        var root = document.RootElement;

        Assert.Equal(
            ["artifactFormatVersion", "compiler", "ruleset", "ruleSources", "snippets", "integrity"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["ruleSourceId", "sourcePath", "sourceSha256", "applicability", "dependencyRuleSourceIds"],
            root.GetProperty("ruleSources")[0].EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            ["snippetId", "ruleSourceId", "sourceAnchor", "content", "contentSha256", "estimatedTokens", "retrieval"],
            root.GetProperty("snippets")[0].EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Null, root.GetProperty("snippets")[0].GetProperty("sourceAnchor").ValueKind);
        Assert.True(CompiledRulesArtifactContract.Read(Encoding.UTF8.GetString(compiled.Write.Bytes.Span)).IsValid);
    }

    [Fact]
    public void ExplicitRetrievalMetadataSurvivesAssemblyWithoutGeneratedVocabulary()
    {
        var snapshot = Snapshot(Source("retrieval", "Rule."));
        var candidates = RuleSnippetCompiler.Compile(snapshot).ToArray();
        candidates[0] = candidates[0] with { Retrieval = Retrieval(reverse: true) };

        var artifact = Compile(snapshot, candidates).Artifact;
        var retrieval = artifact.Snippets[0].Retrieval;

        Assert.Equal(["memory", "soul"], retrieval.Terms.Select(term => term.Term));
        Assert.Equal([25, 100], retrieval.Terms.Select(term => term.Weight));
        Assert.Single(retrieval.Relationships);
        Assert.Equal("memory", retrieval.Relationships[0].FromTerm);
        Assert.Equal("soul", retrieval.Relationships[0].ToTerm);
    }

    [Fact]
    public void DefaultCompilationDoesNotGenerateRetrievalVocabulary()
    {
        var artifact = Compile(Snapshot(Source("plain", "Rule."))).Artifact;

        Assert.All(artifact.Snippets, snippet =>
        {
            Assert.Empty(snippet.Retrieval.Terms);
            Assert.Empty(snippet.Retrieval.Relationships);
        });
    }

    private static Compiled Compile(
        MaterializedRuleSourceSnapshot snapshot,
        IReadOnlyList<RuleSnippetCandidate>? candidates = null)
    {
        candidates ??= RuleSnippetCompiler.Compile(snapshot);
        var assembly = CompiledRulesArtifactAssembler.Assemble(snapshot, candidates);
        Assert.True(assembly.IsValid, Errors(assembly.Errors));
        var artifact = Assert.IsType<CompiledRulesArtifact>(assembly.Artifact);
        var write = CompiledRulesArtifactWriter.Write(artifact);
        Assert.True(write.IsValid, Errors(write.Errors));
        return new(assembly, artifact, write);
    }

    private static MaterializedRuleSourceSnapshot Snapshot(params TestSource[] sources)
    {
        var documents = new List<MaterializedRuleSourceDocument>(sources.Length);
        var entries = new List<RuleSourceManifestEntry>(sources.Length);
        foreach (var source in sources)
        {
            var bytes = Encoding.UTF8.GetBytes(source.Content);
            entries.Add(source.Entry);
            documents.Add(new(
                source.Entry,
                bytes,
                Encoding.UTF8.GetString(bytes),
                MaterializedRuleSourceLoader.ComputeSha256(bytes)));
        }

        byte[] manifestBytes = "{}"u8.ToArray();
        return new(
            "manifest.json",
            manifestBytes,
            MaterializedRuleSourceLoader.ComputeSha256(manifestBytes),
            new RuleSourceManifest
            {
                ManifestFormatVersion = 1,
                CompilerContractVersion = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                RulesetId = "test-rules",
                RepositoryVersion = "test",
                Sources = entries
            },
            new CompiledRulesSourceIdentity { Scheme = "test", Value = "immutable" },
            new CompiledRulesCompilerIdentity
            {
                ContractVersion = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                ImplementationId = "portable-tests",
                ImplementationVersion = "1.0.0"
            },
            documents);
    }

    private static TestSource Source(
        string id,
        string content,
        IReadOnlyList<string>? dependencies = null,
        int priority = 0,
        bool alwaysInclude = false) => new(
            new RuleSourceManifestEntry
            {
                RuleSourceId = id,
                Path = $"rules/{id}.md",
                Layer = RuleLayer.Core,
                CampaignModes = ["*"],
                Operations = ["*"],
                Dependencies = dependencies?.ToList() ?? [],
                Priority = priority,
                AlwaysInclude = alwaysInclude,
                PreparationTier = RulePreparationTier.Standard
            },
            content);

    private static CompiledRulesRetrievalMetadata Retrieval(bool reverse)
    {
        var terms = new List<CompiledRulesRetrievalTerm>
        {
            new() { Term = "memory", Kind = "alternative", Weight = 25 },
            new() { Term = "soul", Kind = "canonical", Weight = 100 }
        };
        if (reverse)
        {
            terms.Reverse();
        }
        return new()
        {
            Terms = terms,
            Relationships =
            [
                new()
                {
                    FromTerm = "memory",
                    ToTerm = "soul",
                    Kind = "related",
                    Weight = 50
                }
            ]
        };
    }

    private static string SnippetSemantics(CompiledRulesArtifact artifact) =>
        JsonSerializer.Serialize(artifact.Snippets.Select(snippet => new
        {
            snippet.SnippetId,
            snippet.RuleSourceId,
            snippet.SourceAnchor,
            snippet.Content,
            snippet.ContentSha256,
            snippet.EstimatedTokens
        }));

    private static string Sha256(ReadOnlySpan<byte> bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

    private static string Errors(IReadOnlyList<CompiledRulesArtifactValidationError> errors) =>
        string.Join(Environment.NewLine, errors.Select(error => $"{error.Code} {error.Path}: {error.Message}"));

    private static MaterializedRuleSourceSnapshot LoadCanonical(string root) =>
        CanonicalVocabularyPayload.LoadHistorical(root);

    private static TemporaryPayload CanonicalPayloadCopy()
    {
        var original = LoadCanonical(FindRepositoryRoot());
        var payload = new TemporaryPayload();
        payload.WriteBytes(original.ManifestPath, original.AuthoritativeManifestBytes.ToArray());
        foreach (var source in original.Sources)
        {
            payload.WriteBytes(source.ManifestEntry.Path, source.AuthoritativeBytes.ToArray());
        }
        return payload;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "VERSION")) &&
                File.Exists(Path.Combine(directory.FullName, "docs", "rules", "rule-source-manifest.json")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new InvalidOperationException("The Eternal Cycle repository root could not be found from the test output directory.");
    }

    private sealed record Compiled(
        CompiledRulesArtifactAssemblyResult Assembly,
        CompiledRulesArtifact Artifact,
        CompiledRulesArtifactWriteResult Write);

    private sealed record TestSource(RuleSourceManifestEntry Entry, string Content);

    private sealed class TemporaryPayload : IDisposable
    {
        public TemporaryPayload()
        {
            Root = Path.Combine(Path.GetTempPath(), "EternalCycle.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public void WriteBytes(string relativePath, byte[] bytes)
        {
            var path = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
            {
                Directory.Delete(Root, recursive: true);
            }
        }
    }
}
