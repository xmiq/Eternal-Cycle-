using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace EternalCycle.Rules.Tests;

public sealed class RuleSnippetCompilerTests(ITestOutputHelper output)
{
    [Fact]
    public void CompilesMinimalDocumentTitleAsOneUnanchoredSnippet()
    {
        var candidate = Assert.Single(Compile(Source("core", "# Core\n\nA rule.")));

        Assert.Equal("core", candidate.SnippetId);
        Assert.Null(candidate.SourceAnchor);
        Assert.Equal("# Core\n\nA rule.", candidate.Content);
    }

    [Fact]
    public void CompilesMultipleSectionsInDocumentOrder()
    {
        var candidates = Compile(Source(
            "core",
            "# Core\n\nIntro.\n\n## First\n\nOne.\n\n## Second\n\nTwo."));

        Assert.Equal(["core", "core#first", "core#second"], candidates.Select(candidate => candidate.SnippetId));
        Assert.Equal([0, 1, 2], candidates.Select(candidate => candidate.SectionOrder));
    }

    [Fact]
    public void H1TitleAndH3ThroughH6RemainNestedWithinEstablishedH2Boundary()
    {
        var candidates = Compile(Source(
            "nested",
            "# Title\n## Two\n### Three\n#### Four\n##### Five\n###### Six"));

        Assert.Equal(["nested", "nested#two"], candidates.Select(candidate => candidate.SnippetId));
        Assert.Contains("### Three\n#### Four\n##### Five\n###### Six", candidates[1].Content, StringComparison.Ordinal);
    }

    [Fact]
    public void TextBeforeFirstHeadingRemainsUnanchored()
    {
        var candidates = Compile(Source("preface", "Preface.\n\n## First heading\nBody."));

        Assert.Equal(2, candidates.Count);
        Assert.Equal("preface", candidates[0].SnippetId);
        Assert.Equal("Preface.", candidates[0].Content);
        Assert.Equal("preface#first-heading", candidates[1].SnippetId);
    }

    [Fact]
    public void HeadingFreeNonEmptySourceCompiles()
    {
        var candidate = Assert.Single(Compile(Source("plain", "Plain operational text.\nSecond line.")));

        Assert.Equal("plain", candidate.SnippetId);
        Assert.Null(candidate.SourceAnchor);
        Assert.Equal("Plain operational text.\nSecond line.", candidate.Content);
    }

    [Fact]
    public void ConsecutiveHeadingsProduceNonEmptyHeadingSections()
    {
        var candidates = Compile(Source("empty-sections", "## One\n## Two\nText."));

        Assert.Equal(2, candidates.Count);
        Assert.Equal("## One", candidates[0].Content);
        Assert.Equal("## Two\nText.", candidates[1].Content);
    }

    [Fact]
    public void DuplicateAndCollidingHeadingsReceiveUniqueStableAnchors()
    {
        var candidates = Compile(Source(
            "duplicates",
            "## Repeat\nA.\n## Repeat\nB.\n## Repeat-1\nC.\n## Repeat\nD."));

        Assert.Equal(
            ["repeat", "repeat-1", "repeat-1-1", "repeat-2"],
            candidates.Select(candidate => candidate.SourceAnchor));
        Assert.Equal(4, candidates.Select(candidate => candidate.SnippetId).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void HeadingLikeTextInsideCodeFencesDoesNotSplit()
    {
        var candidates = Compile(Source(
            "fences",
            "## Real\n```markdown\n## Not a section\n```\n~~~text\n## Also not a section\n~~~\n## Next\nDone."));

        Assert.Equal(["real", "next"], candidates.Select(candidate => candidate.SourceAnchor));
        Assert.Contains("## Not a section", candidates[0].Content, StringComparison.Ordinal);
        Assert.Contains("## Also not a section", candidates[0].Content, StringComparison.Ordinal);
    }

    [Fact]
    public void UnicodeHeadingAndContentArePreserved()
    {
        var candidate = Assert.Single(Compile(Source("unicode", "## Échos 魂\nMémoire — 記憶")));

        Assert.Equal("échos-魂", candidate.SourceAnchor);
        Assert.Equal("## Échos 魂\nMémoire — 記憶", candidate.Content);
    }

    [Fact]
    public void HeadingFormattingAndPunctuationProduceContractIdentifiers()
    {
        var candidates = Compile(Source(
            "formatting",
            "## **Fate & Soul:** _Echoes_!\nRule.\n## Fate! ###\nOther."));

        Assert.Equal(["fate-soul-echoes", "fate"], candidates.Select(candidate => candidate.SourceAnchor));
    }

    [Fact]
    public void CrLfAndLfProduceIdenticalCandidates()
    {
        var lf = Compile(Source("lines", "# Title\n\n## Section\nRule.\n"));
        var crlf = Compile(Source("lines", "# Title\r\n\r\n## Section\r\nRule.\r\n"));

        Assert.Equal(ExecutableFingerprint(lf), ExecutableFingerprint(crlf));
        Assert.NotEqual(lf[0].SourceSha256, crlf[0].SourceSha256);
    }

    [Fact]
    public void Utf8BomDoesNotChangeExecutableCandidates()
    {
        var plain = Compile(Source("bom", "# Title\nRule."));
        var withBom = Compile(Source("bom", "\uFEFF# Title\nRule."));

        Assert.Equal(ExecutableFingerprint(plain), ExecutableFingerprint(withBom));
        Assert.NotEqual(plain[0].SourceSha256, withBom[0].SourceSha256);
    }

    [Fact]
    public void BoundaryBlankLinesAreRemovedButMeaningfulTrailingSpacesRemain()
    {
        var candidate = Assert.Single(Compile(Source(
            "whitespace",
            "\n \n# Title  \r\nBody  \r\n \r\n\r\n")));

        Assert.Equal("# Title  \nBody  ", candidate.Content);
        Assert.DoesNotContain('\r', candidate.Content);
        Assert.False(candidate.Content.StartsWith('\n'));
        Assert.False(candidate.Content.EndsWith('\n'));
    }

    [Fact]
    public void RepeatedCompilationProducesIdenticalCandidates()
    {
        var snapshot = Snapshot(Source("repeatable", "# Title\n## Section\nRule."));

        var first = RuleSnippetCompiler.Compile(snapshot);
        var second = RuleSnippetCompiler.Compile(snapshot);

        Assert.Equal(Fingerprint(first), Fingerprint(second));
    }

    [Fact]
    public void SourceRootRelocationDoesNotChangeSemanticCandidates()
    {
        var repositoryRoot = FindRepositoryRoot();
        var original = LoadCanonical(repositoryRoot);
        using var relocatedRoot = new TemporaryPayload();
        relocatedRoot.WriteBytes(original.ManifestPath, original.AuthoritativeManifestBytes.ToArray());
        foreach (var source in original.Sources)
        {
            relocatedRoot.WriteBytes(source.ManifestEntry.Path, source.AuthoritativeBytes.ToArray());
        }
        var relocated = LoadCanonical(relocatedRoot.Root);

        Assert.Equal(
            Fingerprint(RuleSnippetCompiler.Compile(original)),
            Fingerprint(RuleSnippetCompiler.Compile(relocated)));
    }

    [Fact]
    public void CandidateOrderingUsesManifestSourceThenTextualSectionOrder()
    {
        var candidates = Compile(
            Source("z-source", "## Zed\nZ."),
            Source("a-source", "## First\nA.\n## Second\nB."));

        Assert.Equal(
            ["z-source#zed", "a-source#first", "a-source#second"],
            candidates.Select(candidate => candidate.SnippetId));
        Assert.Equal([0, 1, 1], candidates.Select(candidate => candidate.SourceOrder));
        Assert.Equal([0, 0, 1], candidates.Select(candidate => candidate.SectionOrder));
    }

    [Fact]
    public void SourceApplicabilityMetadataIsInheritedWithoutNewDimensions()
    {
        var entry = new RuleSourceManifestEntry
        {
            RuleSourceId = "world",
            Path = "rules/world.md",
            Layer = RuleLayer.World,
            WorldModelIds = ["world-a"],
            CampaignModes = ["NORMAL"],
            Operations = ["gameplay.resolve"],
            Topics = ["magic"],
            Priority = 42,
            AlwaysInclude = true,
            PreparationTier = RulePreparationTier.CampaignRelevant
        };
        var candidate = Assert.Single(Compile(new TestSource(entry, "## World\nRule.")));

        Assert.Equal(RuleLayer.World, candidate.Applicability.Layer);
        Assert.Equal(["world-a"], candidate.Applicability.WorldModelIds);
        Assert.Equal(["NORMAL"], candidate.Applicability.CampaignModes);
        Assert.Equal(["gameplay.resolve"], candidate.Applicability.Operations);
        Assert.Equal(["magic"], candidate.Applicability.Topics);
        Assert.Equal(42, candidate.Applicability.Priority);
        Assert.True(candidate.Applicability.AlwaysInclude);
        Assert.Equal(RulePreparationTier.CampaignRelevant, candidate.Applicability.PreparationTier);
    }

    [Fact]
    public void SourceDependenciesRemainSourceLevelMetadata()
    {
        var root = Entry("root", "rules/root.md");
        var dependent = new RuleSourceManifestEntry
        {
            RuleSourceId = "dependent",
            Path = "rules/dependent.md",
            Layer = RuleLayer.Core,
            CampaignModes = ["*"],
            Operations = ["*"],
            Dependencies = ["root"]
        };
        var candidates = Compile(
            new TestSource(root, "Root."),
            new TestSource(dependent, "## One\nA.\n## Two\nB."));

        var dependentCandidates = candidates.Where(candidate => candidate.RuleSourceId == "dependent").ToArray();
        Assert.Equal(2, dependentCandidates.Length);
        Assert.All(dependentCandidates, candidate => Assert.Equal(["root"], candidate.DependencyRuleSourceIds));
    }

    [Fact]
    public void IdentityChangesOnlyWhenCanonicalIdentityInputChanges()
    {
        var original = Assert.Single(Compile(Source("identity", "## Alpha\nOriginal.")));
        var bodyChanged = Assert.Single(Compile(Source("identity", "## Alpha\nChanged.")));
        var headingChanged = Assert.Single(Compile(Source("identity", "## Beta\nOriginal.")));

        Assert.Equal(original.SnippetId, bodyChanged.SnippetId);
        Assert.NotEqual(original.ContentSha256, bodyChanged.ContentSha256);
        Assert.NotEqual(original.SnippetId, headingChanged.SnippetId);
    }

    [Fact]
    public void CanonicalTenSourcePayloadCompilesDeterministically()
    {
        var snapshot = LoadCanonical(FindRepositoryRoot());

        var candidates = RuleSnippetCompiler.Compile(snapshot);

        output.WriteLine("Canonical candidate count: {0}", candidates.Count);
        Assert.Equal(10, candidates.Select(candidate => candidate.RuleSourceId).Distinct(StringComparer.Ordinal).Count());
        Assert.NotEmpty(candidates);
        Assert.Equal(candidates.Count, candidates.Select(candidate => candidate.SnippetId).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(Fingerprint(candidates), Fingerprint(RuleSnippetCompiler.Compile(snapshot)));
    }

    [Fact]
    public void EmptySourceIsRejectedRatherThanEmittingAnEmptySnippet()
    {
        var exception = Assert.Throws<RuleSnippetCompilationException>(() =>
            Compile(Source("empty", " \r\n\t\r\n")));

        Assert.Equal("RULE_SOURCE_CONTENT_EMPTY", exception.Code);
        Assert.Equal("empty", exception.RuleSourceId);
    }

    private static IReadOnlyList<RuleSnippetCandidate> Compile(params TestSource[] sources) =>
        RuleSnippetCompiler.Compile(Snapshot(sources));

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
                ImplementationId = "tests",
                ImplementationVersion = "1"
            },
            documents);
    }

    private static TestSource Source(string id, string content) =>
        new(Entry(id, $"rules/{id}.md"), content);

    private static RuleSourceManifestEntry Entry(string id, string path) => new()
    {
        RuleSourceId = id,
        Path = path,
        Layer = RuleLayer.Core,
        CampaignModes = ["*"],
        Operations = ["*"],
        PreparationTier = RulePreparationTier.Standard
    };

    private static string Fingerprint(IReadOnlyList<RuleSnippetCandidate> candidates) =>
        JsonSerializer.Serialize(candidates);

    private static string ExecutableFingerprint(IReadOnlyList<RuleSnippetCandidate> candidates) =>
        JsonSerializer.Serialize(candidates.Select(candidate => new
        {
            candidate.SnippetId,
            candidate.RuleSourceId,
            candidate.SourcePath,
            candidate.SourceAnchor,
            candidate.Content,
            candidate.ContentSha256,
            candidate.EstimatedTokens,
            candidate.Applicability,
            candidate.DependencyRuleSourceIds,
            candidate.SourceOrder,
            candidate.SectionOrder
        }));

    private static MaterializedRuleSourceSnapshot LoadCanonical(string root) =>
        MaterializedRuleSourceLoader.Load(new(
            root,
            "docs/rules/rule-source-manifest.json",
            new CompiledRulesSourceIdentity
            {
                Scheme = "git-commit",
                Value = "bc56e2d5b9c2706cdd2f58b77e2ba09130115880"
            },
            new CompiledRulesCompilerIdentity
            {
                ContractVersion = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                ImplementationId = "portable-tests",
                ImplementationVersion = "1.0.0"
            }));

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
