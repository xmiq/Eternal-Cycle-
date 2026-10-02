using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EternalCycle.Rules.Testing;
using Xunit;

namespace EternalCycle.Rules.Tests;

public sealed class GitHubReleaseCompiledRulesArtifactProviderTests
{
    private const string RepoPath = "/repos/fixture/rules";
    private const string AssetPath = RepoPath + "/releases/assets/30";

    [Fact]
    public async Task PublishedPublicAssetPassesAThenExplicitBApprovalWithoutRepositoryLayout()
    {
        using var transport = new Transport();
        using var provider = Provider(transport);
        var acquired = await provider.AcquireAsync(new(), default);
        Assert.Equal(Fixture(), acquired.Bytes.ToArray());
        Assert.Equal(Hash(Fixture()), acquired.ByteSha256);
        Assert.Equal("github-release", acquired.Evidence.ProviderKind);
        Assert.Equal("github-release-asset", acquired.Evidence.ResolvedIdentity!.Scheme);
        Assert.Equal("10/20/30", acquired.Evidence.ResolvedIdentity.Value);
        Assert.Equal("v-fixture", acquired.Evidence.Metadata["requestedTag"]);
        Assert.Equal("rules.json", acquired.Evidence.Metadata["resolvedAsset"]);
        var result = await Validate(acquired);
        Assert.True(result.IsImportEligible);
        Assert.Equal(Fixture(), result.TrustedArtifact!.CopyBytes());
        Assert.Equal(4, transport.Requests.Count);
        Assert.All(transport.Requests, request =>
        {
            Assert.Equal("GET", request.Method);
            Assert.Equal("https", request.Uri.Scheme);
            Assert.Null(request.Authorization);
            Assert.Equal("EternalCycle-Rules-Provider", request.UserAgent);
            Assert.Equal("2022-11-28", request.ApiVersion);
            Assert.Equal("identity", request.Encoding);
        });
        Assert.Equal("application/vnd.github+json", transport.Requests[0].Accept);
        Assert.Equal("application/octet-stream", transport.Requests[^1].Accept);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task RepositoryReleaseAndAssetNotFoundAreUnavailable(int stage)
    {
        using var transport = new Transport((request, _) => Task.FromResult(
            Stage(request.Uri) == stage ? Reply("private-response", HttpStatusCode.NotFound) : Default(request.Uri)));
        await Failure(transport, CompiledRulesAcquisitionFailure.Unavailable);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData(401, CompiledRulesAcquisitionFailure.Unavailable)]
    [InlineData(403, CompiledRulesAcquisitionFailure.Unavailable)]
    [InlineData(429, CompiledRulesAcquisitionFailure.TransportFailed)]
    [InlineData(500, CompiledRulesAcquisitionFailure.TransportFailed)]
    [InlineData(502, CompiledRulesAcquisitionFailure.TransportFailed)]
    [InlineData(408, CompiledRulesAcquisitionFailure.TransportFailed)]
    public async Task StatusFailuresUseExistingSafeTaxonomyWithoutReadingRemoteBody(int status, CompiledRulesAcquisitionFailure failure)
    {
        using var transport = new Transport((_, _) => Task.FromResult(Reply("private-response", (HttpStatusCode)status)));
        var error = await Failure(transport, failure);
        Assert.DoesNotContain("private-response", error.ToString());
        Assert.Equal(0, transport.Contents[0].Stream.BytesRead);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData("Retry-After", "60")]
    [InlineData("X-RateLimit-Remaining", "0")]
    public async Task ForbiddenRateLimitIsNotReportedAsMissingArtifact(string header, string value)
    {
        using var transport = new Transport((_, _) =>
        {
            var response = Reply("private-response", HttpStatusCode.Forbidden);
            response.Headers.TryAddWithoutValidation(header, value);
            return Task.FromResult(response);
        });
        await Failure(transport, CompiledRulesAcquisitionFailure.TransportFailed);
        Assert.Single(transport.Requests); // no implicit retry loop
        transport.AssertDisposed();
    }

    [Fact]
    public async Task TransportExceptionIsSanitized()
    {
        using var transport = new Transport((_, _) => throw new HttpRequestException("private-token signed-url"));
        var error = await Failure(transport, CompiledRulesAcquisitionFailure.TransportFailed);
        Assert.DoesNotContain("private-token", error.ToString());
        Assert.Null(error.InnerException);
    }

    [Theory]
    [InlineData("json")]
    [InlineData("object")]
    [InlineData("duplicate")]
    [InlineData("id")]
    [InlineData("identity")]
    [InlineData("control")]
    public async Task InvalidRepositoryMetadataFailsStructurally(string kind)
    {
        var metadata = kind switch
        {
            "json" => "{broken",
            "object" => "[]",
            "duplicate" => "{\"id\":10,\"id\":11,\"full_name\":\"fixture/rules\"}",
            "id" => "{\"id\":0,\"full_name\":\"fixture/rules\"}",
            "identity" => "{\"id\":10,\"full_name\":\"other/rules\"}",
            _ => "{\"id\":10,\"full_name\":\"fixture/rules\\u0000\"}"
        };
        using var transport = new Transport((_, _) => Task.FromResult(Reply(metadata)));
        await Failure(transport, CompiledRulesAcquisitionFailure.TransportFailed);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData("missing", CompiledRulesAcquisitionFailure.Unavailable)]
    [InlineData("ambiguous", CompiledRulesAcquisitionFailure.LocatorInvalid)]
    [InlineData("duplicate-id", CompiledRulesAcquisitionFailure.TransportFailed)]
    [InlineData("url", CompiledRulesAcquisitionFailure.TransportFailed)]
    [InlineData("size", CompiledRulesAcquisitionFailure.TransportFailed)]
    [InlineData("state", CompiledRulesAcquisitionFailure.Unavailable)]
    [InlineData("object", CompiledRulesAcquisitionFailure.TransportFailed)]
    public async Task AssetSelectionNeverGuessesOrFollowsMetadataUrls(string kind, CompiledRulesAcquisitionFailure failure)
    {
        var asset = Asset();
        var assets = new JsonArray(asset);
        switch (kind)
        {
            case "missing": asset["name"] = "different.json"; break;
            case "ambiguous": assets.Add(Asset(31)); break;
            case "duplicate-id": assets.Add(Asset()); break;
            case "url": asset["url"] = "https://evil.invalid/private-token"; break;
            case "size": asset["size"] = -1; break;
            case "state": asset["state"] = "starter"; break;
        }
        using var transport = new Transport((request, _) => Task.FromResult(Stage(request.Uri) == 2
            ? Reply(kind == "object" ? "{}" : assets.ToJsonString()) : Default(request.Uri)));
        await Failure(transport, failure);
        Assert.DoesNotContain(transport.Requests, request => request.Uri.AbsolutePath == AssetPath);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData("draft")]
    [InlineData("unpublished")]
    [InlineData("tag-mismatch")]
    [InlineData("prerelease")]
    public async Task OnlyExactPublishedPermittedReleasesAreAcquired(string kind)
    {
        var release = Release();
        if (kind == "draft") release["draft"] = true;
        if (kind == "unpublished") release["published_at"] = null;
        if (kind == "tag-mismatch") release["tag_name"] = "different";
        if (kind == "prerelease") release["prerelease"] = true;
        using var transport = new Transport((request, _) => Task.FromResult(Stage(request.Uri) == 1 ? Reply(release.ToJsonString()) : Default(request.Uri)));
        await Failure(transport, kind == "unpublished" ? CompiledRulesAcquisitionFailure.TransportFailed : CompiledRulesAcquisitionFailure.Unavailable);
        transport.AssertDisposed();
    }

    [Fact]
    public async Task ExplicitPrereleasePolicyAndEscapedSlashTagWork()
    {
        using var transport = new Transport((request, _) =>
        {
            if (Stage(request.Uri) != 1) return Task.FromResult(Default(request.Uri));
            Assert.EndsWith("/tags/release%2Fcandidate", request.Uri.AbsoluteUri);
            var release = Release();
            release["tag_name"] = "release/candidate";
            release["prerelease"] = true;
            return Task.FromResult(Reply(release.ToJsonString()));
        });
        using var provider = Provider(transport, tag: "release/candidate", allowPrerelease: true);
        Assert.Equal(Fixture(), (await provider.AcquireAsync(new(), default)).Bytes.ToArray());
        transport.AssertDisposed();
    }

    [Fact]
    public async Task PaginationFindsExactAssetWithoutTrustingLinkHeaders()
    {
        using var transport = new Transport((request, _) =>
        {
            if (Stage(request.Uri) != 2) return Task.FromResult(Default(request.Uri));
            var first = request.Uri.Query.EndsWith("page=1", StringComparison.Ordinal);
            var assets = first ? new JsonArray(Enumerable.Range(100, 100).Select(id => (JsonNode)Asset(id, $"other-{id}.json")).ToArray()) : new JsonArray(Asset());
            var response = Reply(assets.ToJsonString());
            response.Headers.Add("Link", "<https://evil.invalid/>; rel=next");
            return Task.FromResult(response);
        });
        using var provider = Provider(transport);
        Assert.Equal(Fixture(), (await provider.AcquireAsync(new(), default)).Bytes.ToArray());
        Assert.Equal(2, transport.Requests.Count(request => Stage(request.Uri) == 2));
        Assert.All(transport.Requests, request => Assert.Equal("api.github.com", request.Uri.Host));
        transport.AssertDisposed();
    }

    [Fact]
    public async Task PaginationLimitAndCrossPageAmbiguityFailClosed()
    {
        foreach (var ambiguous in new[] { false, true })
        {
            using var transport = new Transport((request, _) =>
            {
                if (Stage(request.Uri) != 2) return Task.FromResult(Default(request.Uri));
                var page = int.Parse(request.Uri.Query.Split("page=")[2], CultureInfo.InvariantCulture);
                var assets = new JsonArray(Enumerable.Range(page * 100, 100).Select(id => (JsonNode)Asset(id, $"other-{id}.json")).ToArray());
                if (ambiguous) assets[0] = Asset(page * 100, "rules.json");
                return Task.FromResult(Reply(assets.ToJsonString()));
            });
            await Failure(transport, ambiguous ? CompiledRulesAcquisitionFailure.LocatorInvalid : CompiledRulesAcquisitionFailure.TransportFailed);
            Assert.InRange(transport.Requests.Count, 4, 12);
            transport.AssertDisposed();
        }
    }

    [Theory]
    [InlineData("http://release-assets.githubusercontent.com/file")]
    [InlineData("https://evil.invalid/file?private-token")]
    [InlineData("https://release-assets.githubusercontent.com.evil.invalid/file")]
    [InlineData("https://api.github.com/user")]
    [InlineData("https://api.github.com" + AssetPath + "?private-token")]
    [InlineData("https://user:private-token@release-assets.githubusercontent.com/file")]
    [InlineData("https://release-assets.githubusercontent.com:8443/file")]
    [InlineData("https://release-assets.githubusercontent.com/file#private-token")]
    public async Task UnsafeRedirectIsRejectedBeforeSendingToTarget(string location)
    {
        using var transport = new Transport((request, _) => Task.FromResult(request.Uri.AbsolutePath == AssetPath ? Redirect(location) : Default(request.Uri)));
        var error = await Failure(transport, CompiledRulesAcquisitionFailure.LocatorInvalid);
        Assert.DoesNotContain("private-token", error.ToString());
        Assert.Equal(4, transport.Requests.Count);
        transport.AssertDisposed();
    }

    [Fact]
    public async Task AllowedCdnAndRelativeRedirectsNeverForwardCredentialsOrSignedUrlsIntoEvidence()
    {
        using var transport = new Transport((request, _) => Task.FromResult(request.Uri.AbsolutePath switch
        {
            AssetPath => Redirect("https://release-assets.githubusercontent.com/first?private-signature"),
            "/first" => Redirect("/second?private-signature"),
            "/second" => Binary(Fixture()),
            _ => Default(request.Uri)
        }));
        using var provider = Provider(transport, accessToken: "private-token");
        var acquired = await provider.AcquireAsync(new(), default);
        Assert.Equal(Fixture(), acquired.Bytes.ToArray());
        Assert.All(transport.Requests.Take(4), request => Assert.Equal("Bearer private-token", request.Authorization));
        Assert.All(transport.Requests.Skip(4), request =>
        {
            Assert.Null(request.Authorization);
            Assert.Null(request.ApiVersion);
        });
        Assert.DoesNotContain("private", JsonSerializer.Serialize(acquired.Evidence));
        Assert.DoesNotContain("private", JsonSerializer.Serialize(provider));
        Assert.DoesNotContain("private", provider.ToString());
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RedirectLoopOrExcessFailsWithOneBoundedSequence(bool loop)
    {
        var redirects = 0;
        using var transport = new Transport((request, _) => Task.FromResult(Stage(request.Uri) == 3 || request.Uri.Host != "api.github.com"
            ? Redirect(loop ? "https://api.github.com" + AssetPath : $"https://release-assets.githubusercontent.com/{++redirects}") : Default(request.Uri)));
        await Failure(transport, CompiledRulesAcquisitionFailure.LocatorInvalid);
        Assert.InRange(transport.Requests.Count, 4, 7);
        transport.AssertDisposed();
    }

    [Fact]
    public async Task MetadataRedirectIsNotAnArbitraryHostDiscoveryMechanism()
    {
        using var transport = new Transport((_, _) => Task.FromResult(Redirect("https://evil.invalid/")));
        await Failure(transport, CompiledRulesAcquisitionFailure.TransportFailed);
        Assert.Single(transport.Requests);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData("application/octet-stream")]
    [InlineData("application/json")]
    [InlineData("text/plain")]
    public async Task ArtifactValidityIsIndependentOfOrdinaryMediaType(string mediaType)
    {
        using var transport = new Transport((request, _) => Task.FromResult(Stage(request.Uri) == 3 ? Binary(Fixture(), mediaType) : Default(request.Uri)));
        using var provider = Provider(transport);
        Assert.True((await Validate(await provider.AcquireAsync(new(), default))).IsImportEligible);
        transport.AssertDisposed();
    }

    [Fact]
    public async Task ApiAssetMetadataRepresentationIsNotTreatedAsFileBytes()
    {
        using var transport = new Transport((request, _) => Task.FromResult(Stage(request.Uri) == 3
            ? Binary(Encoding.UTF8.GetBytes(Asset().ToJsonString()), "application/vnd.github+json") : Default(request.Uri)));
        await Failure(transport, CompiledRulesAcquisitionFailure.TransportFailed);
        Assert.Equal(0, transport.Contents[^1].Stream.BytesRead);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompressedTransportIsExplicitlyRejectedRatherThanSilentlyChangingBytes(bool metadata)
    {
        using var transport = new Transport((request, _) =>
        {
            var response = Default(request.Uri);
            if (Stage(request.Uri) == (metadata ? 0 : 3)) response.Content.Headers.ContentEncoding.Add("gzip");
            return Task.FromResult(response);
        });
        await Failure(transport, CompiledRulesAcquisitionFailure.TransportFailed);
        Assert.Equal(0, transport.Contents[^1].Stream.BytesRead);
        transport.AssertDisposed();
    }

    [Fact]
    public async Task OversizedDeclaredAssetIsRejectedBeforeReading()
    {
        using var transport = new Transport((request, _) =>
        {
            var response = Default(request.Uri);
            if (Stage(request.Uri) == 3) response.Content.Headers.ContentLength = 129;
            return Task.FromResult(response);
        });
        await Failure(transport, CompiledRulesAcquisitionFailure.PayloadTooLarge, new(128));
        Assert.Equal(0, transport.Contents[^1].Stream.BytesRead);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StreamedBoundIgnoresMissingOrIncorrectContentLength(bool incorrect)
    {
        using var transport = new Transport((request, _) =>
        {
            var response = Stage(request.Uri) == 3 ? Binary(new byte[500]) : Default(request.Uri);
            if (Stage(request.Uri) == 3 && incorrect) response.Content.Headers.ContentLength = 1;
            return Task.FromResult(response);
        });
        await Failure(transport, CompiledRulesAcquisitionFailure.PayloadTooLarge, new(128));
        Assert.Equal(129, transport.Contents[^1].Stream.BytesRead);
        transport.AssertDisposed();
    }

    [Fact]
    public async Task MetadataHasIndependentStreamAndDeclaredBounds()
    {
        foreach (var declared in new[] { false, true })
        {
            using var transport = new Transport((_, _) =>
            {
                var response = Binary(new byte[1024 * 1024 + 100], "application/json");
                if (declared) response.Content.Headers.ContentLength = 1024 * 1024 + 100;
                return Task.FromResult(response);
            });
            await Failure(transport, CompiledRulesAcquisitionFailure.TransportFailed);
            Assert.Equal(declared ? 0 : 1024 * 1024 + 1, transport.Contents[0].Stream.BytesRead);
            transport.AssertDisposed();
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public async Task CallerCancellationDuringMetadataOrAssetReadIsNotTransportFailure(int stage)
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var transport = new Transport(async (request, token) =>
        {
            if (Stage(request.Uri) != stage) return Default(request.Uri);
            if (stage == 0)
            {
                started.SetResult();
                await Task.Delay(Timeout.Infinite, token);
            }
            var response = Binary(Fixture());
            ((TrackingContent)response.Content).Stream.BeforeRead = async cancellation =>
            { started.TrySetResult(); await Task.Delay(Timeout.Infinite, cancellation); };
            return response;
        });
        using var provider = Provider(transport);
        using var cancellation = new CancellationTokenSource();
        var pending = provider.AcquireAsync(new(), cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        cancellation.Cancel();
        var error = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.Equal(cancellation.Token, error.CancellationToken);
        transport.AssertDisposed();
    }

    [Fact]
    public async Task PreCancellationMakesNoRequest()
    {
        using var transport = new Transport();
        using var provider = Provider(transport);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => provider.AcquireAsync(new(), cancellation.Token));
        Assert.Empty(transport.Requests);
    }

    [Fact]
    public async Task ProviderDeadlineIncludesStreamingNotOnlyResponseHeaders()
    {
        using var transport = new Transport((request, _) =>
        {
            var response = Default(request.Uri);
            if (Stage(request.Uri) == 3) ((TrackingContent)response.Content).Stream.BeforeRead = token => new(Task.Delay(Timeout.Infinite, token));
            return Task.FromResult(response);
        });
        using var provider = Provider(transport, timeout: TimeSpan.FromMilliseconds(500));
        var error = await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(new(), default));
        Assert.Equal(CompiledRulesAcquisitionFailure.TransportFailed, error.Failure);
        Assert.Equal(3, Stage(transport.Requests[^1].Uri));
        transport.AssertDisposed();
    }

    [Fact]
    public async Task RepositoryCasingAndMediaTypeCasingDoNotChangeExactByteIdentity()
    {
        using var transport = new Transport((request, _) =>
        {
            var response = Stage(request.Uri) switch
            {
                0 => Reply("{\"id\":10,\"full_name\":\"Fixture/Rules\"}"),
                2 => Reply(new JsonArray(new JsonObject { ["id"] = 30, ["name"] = "rules.json", ["size"] = 1, ["state"] = "uploaded",
                    ["url"] = "https://api.github.com/repos/Fixture/Rules/releases/assets/30" }).ToJsonString()),
                _ => Default(request.Uri)
            };
            if (Stage(request.Uri) != 3) response.Content.Headers.ContentType = new("Application/JSON");
            return Task.FromResult(response);
        });
        using var provider = Provider(transport);
        Assert.Equal(Hash(Fixture()), (await provider.AcquireAsync(new(), default)).ByteSha256);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData("html")]
    [InlineData("missing")]
    [InlineData("duplicate")]
    public async Task MetadataMediaTypeMustActuallySelectJson(string kind)
    {
        using var transport = new Transport((_, _) =>
        {
            var response = Reply(kind == "duplicate" ? "{\"id\":10,\"full_name\":\"fixture/rules\",\"full_name\":\"fixture/rules\"}"
                : "{\"id\":10,\"full_name\":\"fixture/rules\"}");
            response.Content.Headers.ContentType = kind == "html" ? new("text/html") : kind == "missing" ? null : new("application/json");
            return Task.FromResult(response);
        });
        await Failure(transport, CompiledRulesAcquisitionFailure.TransportFailed);
        transport.AssertDisposed();
    }

    [Fact]
    public async Task MalformedPrereleaseFlagIsRejectedEvenWhenPrereleasesAreAllowed()
    {
        using var transport = new Transport((request, _) =>
        {
            var release = Release();
            release["prerelease"] = "false";
            return Task.FromResult(Stage(request.Uri) == 1 ? Reply(release.ToJsonString()) : Default(request.Uri));
        });
        using var provider = Provider(transport, allowPrerelease: true);
        var error = await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(new(), default));
        Assert.Equal(CompiledRulesAcquisitionFailure.TransportFailed, error.Failure);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData("sockets")]
    [InlineData("http")]
    public void UnsafeStandardTransportSettingsCannotSilentlyBypassRedirectPolicy(string kind)
    {
        using HttpMessageHandler transport = kind == "sockets" ? new SocketsHttpHandler() : new HttpClientHandler();
        var error = Assert.Throws<CompiledRulesAcquisitionException>(() => new GitHubReleaseCompiledRulesArtifactProvider(
            "fixture", "rules", "v-fixture", "rules.json", transport: transport));
        Assert.Equal(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid, error.Failure);
    }

    [Fact]
    public async Task PartialReadFailureIsSanitizedAndDisposed()
    {
        using var transport = new Transport((request, _) =>
        {
            var response = Default(request.Uri);
            if (Stage(request.Uri) == 3)
            {
                var stream = ((TrackingContent)response.Content).Stream;
                stream.MaximumRead = 16;
                stream.BeforeRead = _ => stream.BytesRead > 0 ? throw new IOException("private-token") : ValueTask.CompletedTask;
            }
            return Task.FromResult(response);
        });
        var error = await Failure(transport, CompiledRulesAcquisitionFailure.TransportFailed);
        Assert.DoesNotContain("private-token", error.ToString());
        Assert.Equal(16, transport.Contents[^1].Stream.BytesRead);
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData("lf")]
    [InlineData("crlf")]
    [InlineData("bom")]
    [InlineData("empty")]
    public async Task ExactBytesAreNotDecodedOrNormalizedByProvider(string kind)
    {
        var bytes = kind switch { "lf" => "a\n"u8.ToArray(), "crlf" => "a\r\n"u8.ToArray(), "empty" => [], _ => new byte[] { 0xEF, 0xBB, 0xBF, 0xFF, 0 } };
        using var transport = new Transport((request, _) => Task.FromResult(Stage(request.Uri) == 3 ? Binary(bytes) : Default(request.Uri)));
        using var provider = Provider(transport);
        var acquired = await provider.AcquireAsync(new(), default);
        Assert.Equal(bytes, acquired.Bytes.ToArray());
        Assert.Equal(Hash(bytes), acquired.ByteSha256);
        Assert.Equal(CompiledRulesAcquisitionFailure.MalformedArtifact, (await Validate(acquired)).Failure);
        transport.AssertDisposed();
    }

    [Fact]
    public async Task ValidRemoteArtifactCanStillBeRejectedByTrustPolicy()
    {
        using var transport = new Transport();
        using var provider = Provider(transport);
        var acquired = await provider.AcquireAsync(new(), default);
        var result = await Validate(acquired, new Policy(_ => false));
        Assert.True(result.IsArtifactValid);
        Assert.Equal(CompiledRulesAcquisitionFailure.TrustRejected, result.Failure);
    }

    [Fact]
    public async Task SameBytesThroughLocalAndGitHubHaveEqualArtifactMeaningButDifferentEvidence()
    {
        var path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "EternalCycle-provider-equivalence-" + Guid.NewGuid().ToString("N")));
        File.WriteAllBytes(path, Fixture());
        try
        {
            using var transport = new Transport();
            using var provider = Provider(transport);
            var remote = await provider.AcquireAsync(new(), default);
            var local = await new LocalCompiledRulesArtifactProvider(path).AcquireAsync(new(), default);
            Assert.Equal(local.ByteSha256, remote.ByteSha256);
            var a = (await Validate(local)).TrustedArtifact!;
            var b = (await Validate(remote)).TrustedArtifact!;
            Assert.Equal(a.SemanticSha256, b.SemanticSha256);
            Assert.Equal(JsonSerializer.Serialize(a.Artifact), JsonSerializer.Serialize(b.Artifact));
            Assert.Equal(a.CopyBytes(), b.CopyBytes());
            var policy = new Policy(artifact => artifact.AcquisitionEvidence.ResolvedIdentity?.Value == "10/20/30");
            Assert.True((await Validate(remote, policy)).IsImportEligible);
            Assert.Equal(CompiledRulesAcquisitionFailure.TrustRejected, (await Validate(local, policy)).Failure);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task SameRequestedTagCanResolveDifferentObservedObjectsAndBytes()
    {
        var changed = JsonNode.Parse(Fixture())!;
        changed["compiler"]!["implementationVersion"] = "different";
        var model = CompiledRulesArtifactContract.Read(changed.ToJsonString()).Artifact!;
        changed["integrity"]!["artifactSha256"] = CompiledRulesArtifactContract.ComputeArtifactSha256(model);
        var replacement = Encoding.UTF8.GetBytes(changed.ToJsonString());
        var observations = new List<AcquiredCompiledRulesArtifact>();
        foreach (var second in new[] { false, true })
        {
            using var transport = new Transport((request, _) => Task.FromResult(Stage(request.Uri) switch
            {
                2 when second => Reply(new JsonArray(Asset(31)).ToJsonString()),
                3 when second => Binary(replacement),
                _ => Default(request.Uri)
            }));
            using var provider = Provider(transport);
            observations.Add(await provider.AcquireAsync(new(), default));
        }
        Assert.Equal(observations[0].Evidence.Metadata["requestedTag"], observations[1].Evidence.Metadata["requestedTag"]);
        Assert.NotEqual(observations[0].Evidence.ResolvedIdentity!.Value, observations[1].Evidence.ResolvedIdentity!.Value);
        Assert.NotEqual(observations[0].ByteSha256, observations[1].ByteSha256);
        Assert.NotEqual((await Validate(observations[0])).TrustedArtifact!.SemanticSha256, (await Validate(observations[1])).TrustedArtifact!.SemanticSha256);
    }

    [Fact]
    public async Task CanonicalRealSizePayloadAndCultureRetainEstablishedIdentity()
    {
        using var payload = new CanonicalVocabularyPayload();
        var bytes = RuleCompilationPipeline.Compile(payload.Load()).Bytes.ToArray();
        using var transport = new Transport((request, _) => Task.FromResult(Stage(request.Uri) == 3 ? Binary(bytes) : Default(request.Uri)));
        using var provider = Provider(transport);
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new("tr-TR");
            var acquired = await provider.AcquireAsync(new(), default);
            var result = (await Validate(acquired)).TrustedArtifact!;
            Assert.Equal(10, result.Artifact.RuleSources.Count);
            Assert.Equal(154, result.Artifact.Snippets.Count);
            Assert.Equal(275622, acquired.Bytes.Length);
            Assert.Equal("56074D713C27846313D0441A63AEC3794371053C8C85BC5FE5AF5891D53CE37B", result.SemanticSha256);
            Assert.Equal("A591012C4C8414A67F05A898941EFCBADBDFD2C0AD314640199B2B50086B6CD6", acquired.ByteSha256);
        }
        finally { CultureInfo.CurrentCulture = previous; }
        transport.AssertDisposed();
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("url")]
    [InlineData("traversal")]
    [InlineData("control")]
    [InlineData("token")]
    [InlineData("timeout")]
    public void ConfigurationIsExplicitBoundedAndNeverEchoesSecrets(string kind)
    {
        using var transport = new Transport();
        var error = Assert.Throws<CompiledRulesAcquisitionException>(() => new GitHubReleaseCompiledRulesArtifactProvider(
            kind == "missing" ? "" : kind == "url" ? "https://user:private-token@evil.invalid" : "fixture",
            kind == "traversal" ? "../rules" : "rules", kind == "control" ? "private-token\n" : "v-fixture", "rules.json",
            accessToken: kind == "token" ? "private-token\n" : null,
            timeout: kind == "timeout" ? Timeout.InfiniteTimeSpan : null, transport: transport));
        Assert.InRange(error.Message.Length, 1, 256);
        Assert.DoesNotContain("private-token", error.ToString());
        Assert.Null(error.InnerException);
        Assert.Empty(transport.Requests);
    }

    private static GitHubReleaseCompiledRulesArtifactProvider Provider(Transport transport, string tag = "v-fixture", bool allowPrerelease = false,
        string? accessToken = null, TimeSpan? timeout = null) => new("fixture", "rules", tag, "rules.json", allowPrerelease, accessToken, timeout, transport);
    private static byte[] Fixture() => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "ArtifactFixture", "valid-minimal.json"));
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static Task<CompiledRulesArtifactValidationResult> Validate(AcquiredCompiledRulesArtifact acquired, Policy? policy = null) =>
        CompiledRulesArtifactValidation.ValidateAsync(acquired, new(), policy ?? new());
    private static async Task<CompiledRulesAcquisitionException> Failure(Transport transport, CompiledRulesAcquisitionFailure failure, CompiledRulesAcquisitionLimits? limits = null)
    {
        using var provider = Provider(transport);
        var error = await Assert.ThrowsAsync<CompiledRulesAcquisitionException>(() => provider.AcquireAsync(limits ?? new(), default));
        Assert.Equal(failure, error.Failure);
        Assert.InRange(error.Message.Length, 1, 256);
        Assert.Null(error.InnerException);
        return error;
    }
    private static int Stage(Uri uri) => uri.AbsolutePath == RepoPath ? 0 : uri.AbsolutePath.Contains("/tags/", StringComparison.Ordinal) ? 1 : uri.AbsolutePath.EndsWith("/assets", StringComparison.Ordinal) ? 2 : 3;
    private static JsonObject Release() => new() { ["id"] = 20, ["tag_name"] = "v-fixture", ["draft"] = false, ["prerelease"] = false, ["published_at"] = "2026-01-01T00:00:00Z" };
    private static JsonObject Asset(int id = 30, string name = "rules.json") => new() { ["id"] = id, ["name"] = name, ["size"] = 1, ["state"] = "uploaded", ["url"] = $"https://api.github.com{RepoPath}/releases/assets/{id}" };
    private static HttpResponseMessage Default(Uri uri) => Stage(uri) switch
    {
        0 => Reply("{\"id\":10,\"full_name\":\"fixture/rules\"}"),
        1 => Reply(Release().ToJsonString()),
        2 => Reply(new JsonArray(Asset()).ToJsonString()),
        _ => Binary(Fixture())
    };
    private static HttpResponseMessage Reply(string json, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = new TrackingContent(Encoding.UTF8.GetBytes(json), "application/json") };
    private static HttpResponseMessage Binary(byte[] bytes, string mediaType = "application/octet-stream") => new(HttpStatusCode.OK) { Content = new TrackingContent(bytes, mediaType) };
    private static HttpResponseMessage Redirect(string target)
    {
        var response = Reply("", HttpStatusCode.Found);
        response.Headers.Location = new Uri(target, UriKind.RelativeOrAbsolute);
        return response;
    }

    private sealed class Policy(Func<ValidatedCompiledRulesArtifact, bool>? approve = null) : ICompiledRulesArtifactTrustPolicy
    {
        public ValueTask<CompiledRulesArtifactTrustDecision> EvaluateAsync(ValidatedCompiledRulesArtifact artifact, CancellationToken cancellationToken) =>
            ValueTask.FromResult(new CompiledRulesArtifactTrustDecision((approve?.Invoke(artifact) ?? true)
                ? CompiledRulesArtifactTrustReason.ExplicitApproval : CompiledRulesArtifactTrustReason.ExplicitRejection));
    }

    private sealed record RequestEvidence(Uri Uri, string Method, string Accept, string UserAgent, string? Authorization, string? ApiVersion, string Encoding);
    private sealed class Transport(Func<RequestEvidence, CancellationToken, Task<HttpResponseMessage>>? handler = null) : HttpMessageHandler
    {
        public List<RequestEvidence> Requests { get; } = [];
        public List<TrackingContent> Contents { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new(request.RequestUri!, request.Method.Method, request.Headers.Accept.Single().MediaType!,
                request.Headers.UserAgent.ToString(), request.Headers.Authorization?.ToString(),
                request.Headers.TryGetValues("X-GitHub-Api-Version", out var versions) ? versions.Single() : null,
                request.Headers.GetValues("Accept-Encoding").Single()));
            var response = handler is null ? Default(request.RequestUri!) : await handler(Requests[^1], cancellationToken);
            Contents.Add((TrackingContent)response.Content);
            return response;
        }
        public void AssertDisposed() => Assert.All(Contents, content =>
        {
            Assert.True(content.IsDisposed);
            Assert.True(content.Stream.IsDisposed);
        });
    }

    private sealed class TrackingContent : StreamContent
    {
        public ProbeStream Stream { get; }
        public bool IsDisposed { get; private set; }
        public TrackingContent(byte[] bytes, string mediaType) : this(new ProbeStream(bytes), mediaType) { }
        private TrackingContent(ProbeStream stream, string mediaType) : base(stream)
        { Stream = stream; Headers.ContentType = new(mediaType); }
        protected override void Dispose(bool disposing) { IsDisposed = true; base.Dispose(disposing); }
    }

    // Nonseekable content lets the tests exercise the real bounded reader rather
    // than a mock Content-Length shortcut. Hooks synchronize cancellation/failure.
    private sealed class ProbeStream(byte[] bytes) : MemoryStream(bytes)
    {
        public int BytesRead { get; private set; }
        public int MaximumRead { get; set; } = int.MaxValue;
        public bool IsDisposed { get; private set; }
        public Func<CancellationToken, ValueTask>? BeforeRead { get; set; }
        public override bool CanSeek => false;
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (BeforeRead is { } before) await before(cancellationToken);
            var count = await base.ReadAsync(buffer[..Math.Min(buffer.Length, MaximumRead)], cancellationToken);
            BytesRead += count;
            return count;
        }
        protected override void Dispose(bool disposing) { IsDisposed = true; base.Dispose(disposing); }
    }
}
