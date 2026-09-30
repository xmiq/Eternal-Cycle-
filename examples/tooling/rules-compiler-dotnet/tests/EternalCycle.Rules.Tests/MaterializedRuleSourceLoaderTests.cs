using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class MaterializedRuleSourceLoaderTests
{
    [Fact]
    public void LoadsMinimalMaterializedPayload()
    {
        using var payload = new TemporaryPayload();
        payload.WriteText("rules/core.md", "# Core\n\nA rule.\n");
        payload.WriteManifest(Manifest(Source("core", "rules/core.md")));

        var snapshot = Load(payload.Root);

        Assert.Equal("manifest.json", snapshot.ManifestPath);
        Assert.Equal("eternal-cycle", snapshot.Manifest.RulesetId);
        var source = Assert.Single(snapshot.Sources);
        Assert.Equal("core", source.ManifestEntry.RuleSourceId);
        Assert.Equal("rules/core.md", source.ManifestEntry.Path);
        Assert.Equal("# Core\n\nA rule.\n", source.DecodedText);
        Assert.Equal("test-source", snapshot.SourceIdentity.Scheme);
        Assert.Equal("portable-tests", snapshot.CompilerIdentity.ImplementationId);
    }

    [Fact]
    public void LoadsCanonicalTenSourcePayload()
    {
        var root = FindRepositoryRoot();

        var snapshot = MaterializedRuleSourceLoader.Load(Request(
            root,
            "docs/rules/rule-source-manifest.json",
            "git-commit",
            "be948d5880b19c051d9833979a1114295a15a6b1"));

        Assert.Equal(10, snapshot.Sources.Count);
        Assert.Contains(snapshot.Sources, source =>
            source.ManifestEntry.RuleSourceId == "gm-host-bootstrap" &&
            source.ManifestEntry.Path.EndsWith("GM_HOST_BOOTSTRAP.txt", StringComparison.Ordinal));
        Assert.All(snapshot.Sources, source => Assert.NotEmpty(source.AuthoritativeBytes.ToArray()));
    }

    [Fact]
    public void RejectsMalformedManifestJson()
    {
        using var payload = new TemporaryPayload();
        payload.WriteText("manifest.json", "{");

        AssertCode("MANIFEST_JSON_INVALID", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsIncompatibleManifestVersion()
    {
        using var payload = ValidPayload();
        var manifest = Manifest(Source("core", "rules/core.md"));
        manifest["manifestFormatVersion"] = 2;
        payload.WriteManifest(manifest);

        AssertCode("MANIFEST_FORMAT_UNSUPPORTED", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsIncompatibleCompilerContractVersion()
    {
        using var payload = ValidPayload();
        var manifest = Manifest(Source("core", "rules/core.md"));
        manifest["compilerContractVersion"] = "2";
        payload.WriteManifest(manifest);

        AssertCode("COMPILER_CONTRACT_UNSUPPORTED", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsMissingDeclaredSource()
    {
        using var payload = new TemporaryPayload();
        payload.WriteManifest(Manifest(Source("missing", "rules/missing.md")));

        AssertCode("SOURCE_NOT_FOUND", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsDirectoryWhereSourceFileIsRequired()
    {
        using var payload = new TemporaryPayload();
        Directory.CreateDirectory(Path.Combine(payload.Root, "rules", "directory.md"));
        payload.WriteManifest(Manifest(Source("directory", "rules/directory.md")));

        AssertCode("SOURCE_NOT_FILE", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsDuplicateRuleSourceId()
    {
        using var payload = new TemporaryPayload();
        payload.WriteText("rules/one.md", "one");
        payload.WriteText("rules/two.md", "two");
        payload.WriteManifest(Manifest(
            Source("duplicate", "rules/one.md"),
            Source("duplicate", "rules/two.md")));

        AssertCode("RULE_SOURCE_ID_DUPLICATE", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsMissingDependencyTarget()
    {
        using var payload = ValidPayload();
        var source = Source("core", "rules/core.md");
        source["dependencies"] = Strings("absent");
        payload.WriteManifest(Manifest(source));

        AssertCode("DEPENDENCY_MISSING", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsDependencyCycle()
    {
        using var payload = new TemporaryPayload();
        payload.WriteText("rules/one.md", "one");
        payload.WriteText("rules/two.md", "two");
        var one = Source("one", "rules/one.md");
        var two = Source("two", "rules/two.md");
        one["dependencies"] = Strings("two");
        two["dependencies"] = Strings("one");
        payload.WriteManifest(Manifest(one, two));

        AssertCode("DEPENDENCY_CYCLE", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsRootedSourcePath()
    {
        using var payload = new TemporaryPayload();
        payload.WriteManifest(Manifest(Source("core", "C:/outside.md")));

        AssertCode("SOURCE_PATH_INVALID", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsTraversalPath()
    {
        using var payload = new TemporaryPayload();
        payload.WriteManifest(Manifest(Source("core", "../outside.md")));

        AssertCode("SOURCE_PATH_INVALID", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsNormalizedPathCollision()
    {
        using var payload = new TemporaryPayload();
        payload.WriteText("rules/shared.md", "shared");
        payload.WriteManifest(Manifest(
            Source("one", "rules/shared.md"),
            Source("two", "rules/shared.md")));

        AssertCode("SOURCE_PATH_DUPLICATE", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsPortableCaseOnlyPathCollision()
    {
        using var payload = new TemporaryPayload();
        payload.WriteText("rules/shared.md", "shared");
        payload.WriteManifest(Manifest(
            Source("one", "rules/shared.md"),
            Source("two", "RULES/SHARED.md")));

        AssertCode("SOURCE_PATH_DUPLICATE", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsFilesystemIndirectionInSourcePathWhenSupported()
    {
        using var payload = new TemporaryPayload();
        using var outside = new TemporaryPayload();
        outside.WriteText("outside.md", "outside");
        var link = Path.Combine(payload.Root, "linked");
        try
        {
            Directory.CreateSymbolicLink(link, outside.Root);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException or PlatformNotSupportedException)
        {
            return;
        }
        payload.WriteManifest(Manifest(Source("core", "linked/outside.md")));

        AssertCode("FILESYSTEM_INDIRECTION_REJECTED", () => Load(payload.Root));
    }

    [Theory]
    [InlineData("rules\\core.md")]
    [InlineData("rules//core.md")]
    [InlineData("./rules/core.md")]
    [InlineData("rules/./core.md")]
    [InlineData("/rules/core.md")]
    public void RejectsNonCanonicalCrossPlatformPaths(string path)
    {
        using var payload = new TemporaryPayload();
        payload.WriteManifest(Manifest(Source("core", path)));

        AssertCode("SOURCE_PATH_INVALID", () => Load(payload.Root));
    }

    [Fact]
    public void HashesExactAuthoritativeBytesBeforeTextNormalization()
    {
        using var payload = new TemporaryPayload();
        byte[] sourceBytes = [0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes("# Core\r\n\r\nRule.\r\n")];
        payload.WriteBytes("rules/core.md", sourceBytes);
        var manifestBytes = Encoding.UTF8.GetBytes(Manifest(Source("core", "rules/core.md")).ToJsonString() + "\r\n");
        payload.WriteBytes("manifest.json", manifestBytes);

        var snapshot = Load(payload.Root);
        var source = Assert.Single(snapshot.Sources);

        Assert.Equal(Sha256(manifestBytes), snapshot.ManifestSha256);
        Assert.Equal(Sha256(sourceBytes), source.SourceSha256);
        Assert.Equal(manifestBytes, snapshot.AuthoritativeManifestBytes.ToArray());
        Assert.Equal(sourceBytes, source.AuthoritativeBytes.ToArray());
        Assert.StartsWith("\uFEFF# Core\r\n", source.DecodedText, StringComparison.Ordinal);
    }

    [Fact]
    public void UsesExplicitSourceRootInsteadOfCurrentWorkingDirectory()
    {
        using var payload = ValidPayload();
        using var unrelated = new TemporaryPayload();
        var original = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = unrelated.Root;

            var snapshot = Load(payload.Root);

            Assert.Single(snapshot.Sources);
        }
        finally
        {
            Environment.CurrentDirectory = original;
        }
    }

    [Fact]
    public void RejectsUnknownManifestProperty()
    {
        using var payload = ValidPayload();
        var manifest = Manifest(Source("core", "rules/core.md"));
        manifest["acquisitionProvider"] = "git";
        payload.WriteManifest(manifest);

        AssertCode("MANIFEST_JSON_INVALID", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsDuplicateJsonProperty()
    {
        using var payload = ValidPayload();
        var json = Manifest(Source("core", "rules/core.md")).ToJsonString();
        var insertion = json.IndexOf("\"rulesetId\"", StringComparison.Ordinal);
        Assert.True(insertion > 0);
        json = json.Insert(insertion, "\"rulesetId\":\"duplicate\",");
        payload.WriteText("manifest.json", json);

        AssertCode("JSON_PROPERTY_DUPLICATE", () => Load(payload.Root));
    }

    [Fact]
    public void RejectsMissingRequiredStructuralProperty()
    {
        using var payload = ValidPayload();
        var manifest = Manifest(Source("core", "rules/core.md"));
        manifest.Remove("compilerContractVersion");
        payload.WriteManifest(manifest);

        AssertCode("MANIFEST_PROPERTY_REQUIRED", () => Load(payload.Root));
    }

    private static TemporaryPayload ValidPayload()
    {
        var payload = new TemporaryPayload();
        payload.WriteText("rules/core.md", "# Core\n");
        payload.WriteManifest(Manifest(Source("core", "rules/core.md")));
        return payload;
    }

    private static MaterializedRuleSourceSnapshot Load(string root) =>
        MaterializedRuleSourceLoader.Load(Request(root));

    private static MaterializedRuleSourceRequest Request(
        string root,
        string manifestPath = "manifest.json",
        string sourceScheme = "test-source",
        string sourceValue = "immutable-test-value") => new(
            root,
            manifestPath,
            new CompiledRulesSourceIdentity
            {
                Scheme = sourceScheme,
                Value = sourceValue
            },
            new CompiledRulesCompilerIdentity
            {
                ContractVersion = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
                ImplementationId = "portable-tests",
                ImplementationVersion = "1.0.0"
            });

    private static JsonObject Manifest(params JsonObject[] sources)
    {
        var sourceArray = new JsonArray();
        foreach (var source in sources)
        {
            sourceArray.Add(source);
        }
        return new JsonObject
        {
            ["manifestFormatVersion"] = 1,
            ["compilerContractVersion"] = CompiledRulesArtifactContract.CurrentCompilerContractVersion,
            ["rulesetId"] = "eternal-cycle",
            ["repositoryVersion"] = "test",
            ["sources"] = sourceArray
        };
    }

    private static JsonObject Source(string id, string path) => new()
    {
        ["ruleSourceId"] = id,
        ["path"] = path,
        ["layer"] = "Core",
        ["worldModelIds"] = new JsonArray(),
        ["moduleIds"] = new JsonArray(),
        ["campaignModes"] = Strings("*"),
        ["operations"] = Strings("*"),
        ["topics"] = new JsonArray(),
        ["dependencies"] = new JsonArray(),
        ["priority"] = 0,
        ["alwaysInclude"] = false,
        ["preparationTier"] = "Standard"
    };

    private static JsonArray Strings(params string[] values)
    {
        var result = new JsonArray();
        foreach (var value in values)
        {
            result.Add(value);
        }
        return result;
    }

    private static void AssertCode(string expectedCode, Action action)
    {
        var exception = Assert.Throws<MaterializedRuleSourceException>(action);
        Assert.Equal(expectedCode, exception.Code);
    }

    private static string Sha256(byte[] bytes) =>
        Convert.ToHexString(SHA256.HashData(bytes));

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

    private sealed class TemporaryPayload : IDisposable
    {
        public TemporaryPayload()
        {
            Root = Path.Combine(Path.GetTempPath(), "EternalCycle.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
        }

        public string Root { get; }

        public void WriteManifest(JsonObject manifest) =>
            WriteText("manifest.json", manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        public void WriteText(string relativePath, string text) =>
            WriteBytes(relativePath, Encoding.UTF8.GetBytes(text));

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
