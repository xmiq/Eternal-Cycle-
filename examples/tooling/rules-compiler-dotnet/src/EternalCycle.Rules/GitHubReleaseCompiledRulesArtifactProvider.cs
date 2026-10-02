using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EternalCycle.Rules;

public sealed class GitHubReleaseCompiledRulesArtifactProvider : ICompiledRulesArtifactProvider, IDisposable
{
    private const int MaximumMetadataBytes = 1024 * 1024;
    private const int MaximumAssetPages = 10;
    private const int MaximumRedirects = 3;
    private readonly string repositoryPath;
    private readonly string releaseTag;
    private readonly string assetName;
    private readonly bool allowPrerelease;
    private readonly string? accessToken;
    private readonly TimeSpan timeout;
    private readonly HttpClient client;

    public GitHubReleaseCompiledRulesArtifactProvider(
        string owner, string repository, string releaseTag, string assetName,
        bool allowPrerelease = false, string? accessToken = null, TimeSpan? timeout = null,
        HttpMessageHandler? transport = null)
    {
        if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(repository) ||
            string.IsNullOrWhiteSpace(releaseTag) || string.IsNullOrWhiteSpace(assetName))
            throw Failure(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
        // These are exact selector components, not URLs or arbitrary endpoints.
        // Escape tags (including slash-bearing tags) before placing them in a URI.
        if (!SafeSelector(owner, 39, false) || !SafeSelector(repository, 100, false) ||
            !SafeSelector(releaseTag, 256, true) || !SafeSelector(assetName, 256, false))
            throw Failure(CompiledRulesAcquisitionFailure.LocatorInvalid);
        if (accessToken is not null && (accessToken.Length is 0 or > 1024 ||
            accessToken.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-' and not '.')))
            throw Failure(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
        this.timeout = timeout ?? TimeSpan.FromMinutes(2);
        if (this.timeout <= TimeSpan.Zero || this.timeout > TimeSpan.FromMinutes(10))
            throw Failure(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);
        // Reject accidental unsafe standard-handler injection too. Custom handler
        // code remains trusted; no in-process provider can sandbox that code.
        if (transport is HttpClientHandler http && (http.AllowAutoRedirect || http.AutomaticDecompression != DecompressionMethods.None || http.UseCookies || http.Credentials is not null) ||
            transport is SocketsHttpHandler sockets && (sockets.AllowAutoRedirect || sockets.AutomaticDecompression != DecompressionMethods.None || sockets.UseCookies || sockets.Credentials is not null))
            throw Failure(CompiledRulesAcquisitionFailure.ProviderConfigurationInvalid);

        repositoryPath = $"/repos/{owner}/{repository}";
        this.releaseTag = releaseTag;
        this.assetName = assetName;
        this.allowPrerelease = allowPrerelease;
        this.accessToken = accessToken;
        // Own a client without default headers. Injected handlers are trusted
        // transport code and must preserve this no-redirect/no-decompression policy.
        client = new HttpClient(transport ?? new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
            Credentials = null,
            ConnectTimeout = TimeSpan.FromSeconds(30),
            MaxResponseHeadersLength = 16
        }, disposeHandler: transport is null) { Timeout = Timeout.InfiniteTimeSpan };
    }

    public async Task<AcquiredCompiledRulesArtifact> AcquireAsync(CompiledRulesAcquisitionLimits limits, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(limits);
        cancellationToken.ThrowIfCancellationRequested();
        using var deadline = new CancellationTokenSource(timeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, deadline.Token);
        var token = linked.Token;
        try
        {
            using var repository = await ReadMetadataAsync(repositoryPath, token);
            var repositoryId = PositiveId(repository.RootElement);
            var fullName = Text(repository.RootElement, "full_name", 140);
            if (!string.Equals(fullName, repositoryPath[7..], StringComparison.OrdinalIgnoreCase))
                throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);

            using var release = await ReadMetadataAsync($"{repositoryPath}/releases/tags/{Uri.EscapeDataString(releaseTag)}", token);
            var releaseId = PositiveId(release.RootElement);
            var tag = Text(release.RootElement, "tag_name", 256);
            var draft = release.RootElement.GetProperty("draft").GetBoolean();
            var prerelease = release.RootElement.GetProperty("prerelease").GetBoolean();
            if (tag != releaseTag || draft || (!allowPrerelease && prerelease) ||
                !DateTimeOffset.TryParse(Text(release.RootElement, "published_at", 64), CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out _))
                throw Failure(CompiledRulesAcquisitionFailure.Unavailable);

            var assetId = await FindAssetAsync(releaseId, token);
            var evidence = new CompiledRulesAcquisitionEvidence("github-release",
                new("github-release-asset", $"{Number(repositoryId)}/{Number(releaseId)}/{Number(assetId)}"),
                new Dictionary<string, string>
                {
                    ["requestedRepository"] = repositoryPath[7..],
                    ["requestedTag"] = releaseTag,
                    ["requestedAsset"] = assetName,
                    ["repositoryId"] = Number(repositoryId),
                    ["releaseId"] = Number(releaseId),
                    ["assetId"] = Number(assetId),
                    ["resolvedRepository"] = fullName,
                    ["resolvedTag"] = tag,
                    ["resolvedAsset"] = assetName
                });
            var acquired = await DownloadAsync(assetId, evidence, limits, token);
            token.ThrowIfCancellationRequested();
            return acquired;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Never expose a transport exception containing a signed URL/token.
            throw new OperationCanceledException("Artifact acquisition was cancelled.", cancellationToken);
        }
        catch (OperationCanceledException) when (deadline.IsCancellationRequested)
        {
            throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
        }
        catch (OperationCanceledException cancelled)
        {
            throw new OperationCanceledException("Artifact acquisition was cancelled.", cancelled.CancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or JsonException or
            InvalidOperationException or KeyNotFoundException or FormatException)
        {
            throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
        }
    }

    private async Task<long> FindAssetAsync(long releaseId, CancellationToken token)
    {
        long? selected = null;
        var ids = new HashSet<long>();
        // Construct pagination ourselves; never follow an untrusted Link URL.
        // Exhaust the bounded list so the first match cannot hide an ambiguity.
        for (var page = 1; page <= MaximumAssetPages; page++)
        {
            using var assets = await ReadMetadataAsync($"{repositoryPath}/releases/{Number(releaseId)}/assets?per_page=100&page={Number(page)}", token);
            if (assets.RootElement.ValueKind != JsonValueKind.Array || assets.RootElement.GetArrayLength() > 100)
                throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
            foreach (var asset in assets.RootElement.EnumerateArray())
            {
                var id = PositiveId(asset);
                var name = Text(asset, "name", 256);
                if (!ids.Add(id) || asset.GetProperty("size").GetInt64() < 0 ||
                    !string.Equals(Text(asset, "url", 512), ApiUri($"{repositoryPath}/releases/assets/{Number(id)}").AbsoluteUri, StringComparison.OrdinalIgnoreCase))
                    throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
                if (name != assetName) continue;
                if (selected is not null) throw Failure(CompiledRulesAcquisitionFailure.LocatorInvalid);
                if (Text(asset, "state", 32) != "uploaded") throw Failure(CompiledRulesAcquisitionFailure.Unavailable);
                selected = id;
            }
            if (assets.RootElement.GetArrayLength() < 100)
                return selected ?? throw Failure(CompiledRulesAcquisitionFailure.Unavailable);
        }
        throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
    }

    private async Task<JsonDocument> ReadMetadataAsync(string path, CancellationToken token)
    {
        using var request = Request(ApiUri(path), binary: false, credentials: true);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
        CheckStatus(response);
        CheckEncoding(response);
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if ((!string.Equals(mediaType, "application/json", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(mediaType, "application/vnd.github+json", StringComparison.OrdinalIgnoreCase)) ||
            response.Content.Headers.ContentLength > MaximumMetadataBytes)
            throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
        await using var stream = await response.Content.ReadAsStreamAsync(token);
        AcquiredCompiledRulesArtifact metadata;
        try { metadata = await AcquiredCompiledRulesArtifact.ReadAsync(stream, new("github-metadata"), new(MaximumMetadataBytes), token); }
        catch (CompiledRulesAcquisitionException exception) when (exception.Failure == CompiledRulesAcquisitionFailure.PayloadTooLarge)
        { throw Failure(CompiledRulesAcquisitionFailure.TransportFailed); }
        return JsonDocument.Parse(metadata.Bytes, new JsonDocumentOptions { MaxDepth = 16 });
    }

    private async Task<AcquiredCompiledRulesArtifact> DownloadAsync(long assetId, CompiledRulesAcquisitionEvidence evidence,
        CompiledRulesAcquisitionLimits limits, CancellationToken token)
    {
        var endpoint = ApiUri($"{repositoryPath}/releases/assets/{Number(assetId)}");
        var target = endpoint;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        for (var redirects = 0; ; redirects++)
        {
            token.ThrowIfCancellationRequested();
            CheckDownloadTarget(target, endpoint);
            if (!visited.Add(target.AbsoluteUri)) throw Failure(CompiledRulesAcquisitionFailure.LocatorInvalid);
            using var request = Request(target, binary: true, credentials: redirects == 0);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            if (response.StatusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther or
                HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect)
            {
                if (redirects == MaximumRedirects || response.Headers.Location is not { } location ||
                    !Uri.TryCreate(target, location, out var next))
                    throw Failure(CompiledRulesAcquisitionFailure.LocatorInvalid);
                target = next;
                continue;
            }
            CheckStatus(response);
            CheckEncoding(response);
            // Vendor API JSON means negotiation returned asset metadata, not a
            // file. Ordinary JSON/odd artifact media types remain B's concern.
            if (string.Equals(response.Content.Headers.ContentType?.MediaType, "application/vnd.github+json", StringComparison.OrdinalIgnoreCase))
                throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
            if (response.Content.Headers.ContentLength > limits.MaximumPayloadBytes)
                throw Failure(CompiledRulesAcquisitionFailure.PayloadTooLarge);
            await using var stream = await response.Content.ReadAsStreamAsync(token);
            return await AcquiredCompiledRulesArtifact.ReadAsync(stream, evidence, limits, token);
        }
    }

    private HttpRequestMessage Request(Uri uri, bool binary, bool credentials)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.UserAgent.ParseAdd("EternalCycle-Rules-Provider");
        request.Headers.Accept.Add(new(binary ? "application/octet-stream" : "application/vnd.github+json"));
        request.Headers.Add("Accept-Encoding", "identity");
        if (uri.Host == "api.github.com") request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        if (credentials && accessToken is not null) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static void CheckDownloadTarget(Uri uri, Uri endpoint)
    {
        // Deliberately narrower than *.githubusercontent.com. GitHub documents
        // this release-asset delivery host; future host changes fail closed.
        if (!uri.IsAbsoluteUri || uri.Scheme != Uri.UriSchemeHttps || uri.Port != 443 ||
            uri.UserInfo.Length != 0 || uri.Fragment.Length != 0 ||
            (uri.Host != "release-assets.githubusercontent.com" && uri != endpoint))
            throw Failure(CompiledRulesAcquisitionFailure.LocatorInvalid);
    }

    private static void CheckStatus(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.OK) return;
        // Rate limiting is transport unavailability, NOT evidence that an asset
        // does not exist. Do not echo bodies or implement hidden retries.
        if ((int)response.StatusCode == 429 || (response.StatusCode == HttpStatusCode.Forbidden &&
            (response.Headers.Contains("Retry-After") ||
             (response.Headers.TryGetValues("X-RateLimit-Remaining", out var values) && values.Contains("0")))))
            throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
        throw Failure(response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
            ? CompiledRulesAcquisitionFailure.Unavailable : CompiledRulesAcquisitionFailure.TransportFailed);
    }

    private static void CheckEncoding(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentEncoding.Any(value => !string.Equals(value, "identity", StringComparison.OrdinalIgnoreCase)))
            throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
    }

    private static long PositiveId(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
        var properties = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
            if (!properties.Add(property.Name)) throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
        var id = element.GetProperty("id").GetInt64();
        return id > 0 ? id : throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
    }

    private static string Text(JsonElement element, string property, int maximum)
    {
        var value = element.GetProperty(property).GetString();
        if (string.IsNullOrWhiteSpace(value) || value.Length > maximum || value.Any(char.IsControl))
            throw Failure(CompiledRulesAcquisitionFailure.TransportFailed);
        return value;
    }

    private static bool SafeSelector(string value, int maximum, bool slash) => value.Length <= maximum &&
        value is not "." and not ".." && !value.Contains("..", StringComparison.Ordinal) &&
        value.All(character => char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.' or '+' || (slash && character == '/')) &&
        (!slash || (!value.StartsWith('/') && !value.EndsWith('/') && !value.Contains("//", StringComparison.Ordinal)));

    private static Uri ApiUri(string path) => new("https://api.github.com" + path);
    private static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);
    private static CompiledRulesAcquisitionException Failure(CompiledRulesAcquisitionFailure failure) => new(failure);
    public override string ToString() => nameof(GitHubReleaseCompiledRulesArtifactProvider);
    public void Dispose() => client.Dispose();
}
