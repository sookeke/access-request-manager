
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UserAccessManager.Services.Apicurio.Extension;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;

namespace UserAccessManager.Services.Apicurio;
public class ApiCurioRegistryClient: IApiCurioRegistryClient
{

    const string PathPrefix = "/apis/registry/v2";

    /// <summary>
    /// Initializes a new <see cref="ApiCurioRegistryClient"/>
    /// </summary>
    /// <param name="logger">The service used to perform logging</param>
    /// <param name="options">The current <see cref="ApiCurioRegistryClientOptions"/></param>
    /// <param name="httpClientFactory">The service used to create <see cref="System.Net.Http.HttpClient"/>s</param>
    /// <param name="jsonSerializer">The service used to serialize and deserialize JSON</param>
    public ApiCurioRegistryClient(ILogger<ApiCurioRegistryClient> logger, IOptions<ApiCurioRegistryClientOptions> options, IHttpClientFactory httpClientFactory)
    {
        this.Logger = logger;
        this.Options = options.Value;
        this.HttpClient = httpClientFactory.CreateClient(typeof(ApiCurioRegistryClient).Name);
    }

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the current <see cref="ApiCurioRegistryClientOptions"/>
    /// </summary>
    protected ApiCurioRegistryClientOptions Options { get; }

    /// <summary>
    /// Gets the <see cref="System.Net.Http.HttpClient"/> used to perform calls to the remote ApiCurio Registry API
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <summary>
    /// Gets the service used to serialize and deserialize JSON
    /// </summary>

    protected virtual async Task<string> FormatAsync(string value, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value));
        var formatted = value;
        return await Task.FromResult(this.Options.LineEndingFormatMode switch
        {
            LineEndingFormatMode.ConvertToUnix => formatted.ReplaceLineEndings("\r\n"),
            LineEndingFormatMode.ConvertToWindows => formatted.ReplaceLineEndings("\n"),
            _ => value,
        });
    }
    #region Artifacts

    /// <inheritdoc/>
    public virtual async Task<string> GetLatestArtifactAsync(string artifactId, string groupId = "default", CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(artifactId))
            throw new ArgumentNullException(nameof(artifactId));
        if (string.IsNullOrWhiteSpace(groupId))
            throw new ArgumentNullException(nameof(groupId));
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{PathPrefix}/groups/{groupId}/artifacts/{artifactId}");
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "");
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var content = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
        {
            this.Logger.LogError("An error occured while retrieving the latest version of the artifact with the specified id '{artifactId}': the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", artifactId, response.StatusCode, content);
            response.EnsureSuccessStatusCode();
        }
        return content;
    }

    /// <inheritdoc/>
    public virtual async Task<string?> GetArtifactContentByIdAsync(string contentId, CancellationToken cancellationToken = default)
    {
        var accessTokenClient = new HttpClient();
        var accessToken = await accessTokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = "https://sso-dev-5b7aa5-dev.apps.silver.devops.gov.bc.ca/auth/realms/DEMSPOC/protocol/openid-connect/token",
            ClientId = "registry-client-api",
            ClientSecret = "<redacted>",
            GrantType = "client_credentials"
        });
        if (string.IsNullOrWhiteSpace(contentId))
            throw new ArgumentNullException(nameof(contentId));
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{PathPrefix}/ids/contentIds/{contentId}/");
        request.Headers.Authorization =
           new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken!.AccessToken);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var content = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
        {
            this.Logger.LogError("An error occured while retrieving the artifact with the specified content id '{contentId}': the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", contentId, response.StatusCode, content);
            response.EnsureSuccessStatusCode();
        }
        return content;
    }

    /// <inheritdoc/>
    public virtual async Task<string?> GetArtifactContentByGlobalIdAsync(string globalId, CancellationToken cancellationToken = default)
    {
        var accessTokenClient = new HttpClient();
        var accessToken = await accessTokenClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        {
            Address = "https://sso-dev-5b7aa5-dev.apps.silver.devops.gov.bc.ca/auth/realms/DEMSPOC/protocol/openid-connect/token",
            ClientId = "registry-client-api",
            ClientSecret = "8f054922-1c53-4d9f-9aad-ab6fbafb4aa2",
            GrantType = "client_credentials"
        });
        if (string.IsNullOrWhiteSpace(globalId))
            throw new ArgumentNullException(nameof(globalId));
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{PathPrefix}/ids/globalIds/{globalId}/");
        request.Headers.Authorization =
   new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken!.AccessToken);
        using var response = await this.HttpClient.SendAsync(request, cancellationToken);
        var content = await response.Content?.ReadAsStringAsync(cancellationToken)!;
        if (!response.IsSuccessStatusCode)
        {
            this.Logger.LogError("An error occured while retrieving the artifact with the specified global id '{globalId}': the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", globalId, response.StatusCode, content);
            response.EnsureSuccessStatusCode();
        }
        return content;
    }

    public Task<string?> GetArtifactContentBySha256HashAsync(string contentHash, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ArtifactQueryResults> SearchForArtifactsAsync(SearchForArtifactsQuery? query = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ArtifactQueryResults> SearchForArtifactsByContentAsync(string content, SearchForArtifactsByContentQuery? query = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ArtifactQueryResults> ListArtifactsInGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateArtifactStateAsync(string artifactId, ArtifactState state, string groupId = "default", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Artifact> UpdateArtifactAsync(string artifactId, string content, string groupId = "default", string? version = null, string? name = null, string? description = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Artifact> CreateArtifactAsync(ArtifactType artifactType, string content, IfArtifactExistsAction ifExists, string? artifactId = null, string groupId = "default", bool canonical = false, string? version = null, string? name = null, string? description = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteArtifactAsync(string artifactId, string groupId = "default", CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAllArtifactsInGroupAsync(string groupId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    //public virtual async Task<string?> GetArtifactContentBySha256HashAsync(string contentHash, CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrWhiteSpace(contentHash))
    //        throw new ArgumentNullException(nameof(contentHash));
    //    using var request = new HttpRequestMessage(HttpMethod.Get, $"{PathPrefix}/ids/contentHashes/{contentHash}/");
    //    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
    //    var content = await response.Content?.ReadAsStringAsync(cancellationToken)!;
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        this.Logger.LogError("An error occured while retrieving the artifact with the specified content hash '{contentHash}': the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", contentHash, response.StatusCode, content);
    //        response.EnsureSuccessStatusCode();
    //    }
    //    return content;
    //}

    ///// <inheritdoc/>
    //public virtual async Task<ArtifactQueryResults> SearchForArtifactsAsync(SearchForArtifactsQuery? query = null, CancellationToken cancellationToken = default)
    //{
    //    var queryParameters = query?.ToDictionary();
    //    string? queryString = null;
    //    if (queryParameters != null)
    //        queryString = $"?{string.Join("&", queryParameters.Where(kvp => kvp.Value != null).Select(kvp => $"{kvp.Key}={kvp.Value}"))}"; ;
    //    var uri = $"{PathPrefix}/search/artifacts" + queryString;
    //    using var request = new HttpRequestMessage(HttpMethod.Get, uri);
    //    request.Headers.Accept.Add(new(MediaTypeNames.Application.Json));
    //    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
    //    var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        this.Logger.LogError("An error occured while searching for artifacts: the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", response.StatusCode, json);
    //        response.EnsureSuccessStatusCode();
    //    }
    //    return await this.JsonSerializer.DeserializeAsync<ArtifactQueryResults>(json, cancellationToken);
    //}

    ///// <inheritdoc/>
    //public virtual async Task<ArtifactQueryResults> SearchForArtifactsByContentAsync(string content, SearchForArtifactsByContentQuery? query = null, CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrWhiteSpace(content))
    //        throw new ArgumentNullException(nameof(content));
    //    var formattedContent = await this.FormatAsync(content, cancellationToken);
    //    var queryParameters = query?.ToDictionary();
    //    string? queryString = null;
    //    if (queryParameters != null)
    //        queryString = $"?{string.Join("&", queryParameters.Where(kvp => kvp.Value != null).Select(kvp => $"{kvp.Key}={kvp.Value}"))}"; ;
    //    var uri = $"{PathPrefix}/search/artifacts" + queryString;
    //    using var httpContent = new StringContent(formattedContent, Encoding.UTF8);
    //    using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = httpContent };
    //    request.Headers.Accept.Add(new(MediaTypeNames.Application.Json));
    //    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
    //    var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        this.Logger.LogError("An error occured while searching for artifacts: the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", response.StatusCode, json);
    //        response.EnsureSuccessStatusCode();
    //    }
    //    return await this.JsonSerializer.DeserializeAsync<ArtifactQueryResults>(json, cancellationToken);
    //}

    ///// <inheritdoc/>
    //public virtual async Task<ArtifactQueryResults> ListArtifactsInGroupAsync(string groupId, CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrWhiteSpace(groupId))
    //        throw new ArgumentNullException(nameof(groupId));
    //    using var request = new HttpRequestMessage(HttpMethod.Get, $"{PathPrefix}/groups/{groupId}/artifacts");
    //    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
    //    var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        this.Logger.LogError("An error occured while listing artifacts belonging to the group with the specified id '{groupId}': the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", groupId, response.StatusCode, json);
    //        response.EnsureSuccessStatusCode();
    //    }
    //    return await this.JsonSerializer.DeserializeAsync<ArtifactQueryResults>(json, cancellationToken);
    //}

    ///// <inheritdoc/>
    //public virtual async Task UpdateArtifactStateAsync(string artifactId, ArtifactState state, string groupId = "default", CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrWhiteSpace(artifactId))
    //        throw new ArgumentNullException(nameof(artifactId));
    //    if (string.IsNullOrWhiteSpace(groupId))
    //        throw new ArgumentNullException(nameof(groupId));
    //    var json = await this.JsonSerializer.SerializeAsync(new { state = EnumHelper.Stringify(state) }, cancellationToken);
    //    using var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
    //    using var request = new HttpRequestMessage(HttpMethod.Put, $"{PathPrefix}/groups/{groupId}/artifacts/{artifactId}/state") { Content = content };
    //    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
    //    json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        this.Logger.LogError("An error occured while updating the state of the artifact with the specified id '{artifactId}': the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", artifactId, response.StatusCode, content);
    //        response.EnsureSuccessStatusCode();
    //    }
    //}

    ///// <inheritdoc/>
    //public virtual async Task<Artifact> UpdateArtifactAsync(string artifactId, string content, string groupId = "default", string? version = null, string? name = null, string? description = null, CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrWhiteSpace(artifactId))
    //        throw new ArgumentNullException(nameof(artifactId));
    //    if (string.IsNullOrWhiteSpace(groupId))
    //        throw new ArgumentNullException(nameof(groupId));
    //    var formattedContent = await this.FormatAsync(content, cancellationToken);
    //    using var httpContent = new StringContent(formattedContent, Encoding.UTF8);
    //    using var request = new HttpRequestMessage(HttpMethod.Put, $"{PathPrefix}/groups/{groupId}/artifacts/{artifactId}") { Content = httpContent };
    //    if (!string.IsNullOrWhiteSpace(version))
    //        request.Headers.TryAddWithoutValidation("X-Registry-Version", version);
    //    if (!string.IsNullOrWhiteSpace(name))
    //        request.Headers.TryAddWithoutValidation("X-Registry-Name", name);
    //    if (!string.IsNullOrWhiteSpace(description))
    //        request.Headers.TryAddWithoutValidation("X-Registry-Description", description);
    //    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
    //    var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        this.Logger.LogError("An error occured while retrieving the artifact with the specified content hash '{contentHash}': the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", json, response.StatusCode, content);
    //        response.EnsureSuccessStatusCode();
    //    }
    //    return await JsonConvert.DeserializeObject<Artifact>(json);
    //}

    ///// <inheritdoc/>
    //public virtual async Task<Artifact> CreateArtifactAsync(ArtifactType artifactType, string content, IfArtifactExistsAction ifExists, string? artifactId = null, string groupId = "default", bool canonical = false, string? version = null, string? name = null, string? description = null, CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrWhiteSpace(groupId))
    //        throw new ArgumentNullException(nameof(groupId));
    //    var formattedContent = await this.FormatAsync(content, cancellationToken);
    //    using var httpContent = new StringContent(formattedContent, Encoding.UTF8);
    //    using var request = new HttpRequestMessage(HttpMethod.Post, $"{PathPrefix}/groups/{groupId}/artifacts?ifExists={EnumHelper.Stringify(ifExists)}&canonical={canonical}") { Content = httpContent };
    //    request.Headers.TryAddWithoutValidation("X-Registry-ArtifactType", EnumHelper.Stringify(artifactType));
    //    if (!string.IsNullOrWhiteSpace(artifactId))
    //        request.Headers.TryAddWithoutValidation("X-Registry-ArtifactId", artifactId);
    //    if (!string.IsNullOrWhiteSpace(version))
    //        request.Headers.TryAddWithoutValidation("X-Registry-Version", version);
    //    if (!string.IsNullOrWhiteSpace(name))
    //        request.Headers.TryAddWithoutValidation("X-Registry-Name", name);
    //    if (!string.IsNullOrWhiteSpace(description))
    //        request.Headers.TryAddWithoutValidation("X-Registry-Description", description);
    //    request.Headers.TryAddWithoutValidation("X-Registry-Content-Hash", HashHelper.SHA256Hash(formattedContent));
    //    request.Headers.TryAddWithoutValidation("X-Registry-Content-Algorithm", EnumHelper.Stringify(HashAlgorithm.SHA256));
    //    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
    //    var json = await response.Content?.ReadAsStringAsync(cancellationToken)!;
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        this.Logger.LogError("An error occured while creating a new artifact: the remote server responded with a non-success status code '{statusCode}'./r/Response content: {json}", json, response.StatusCode);
    //        response.EnsureSuccessStatusCode();
    //    }
    //    return JsonConvert.DeserializeObject<Artifact>(json);
    //}

    ///// <inheritdoc/>
    //public virtual async Task DeleteArtifactAsync(string artifactId, string groupId = "default", CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrWhiteSpace(artifactId))
    //        throw new ArgumentNullException(nameof(artifactId));
    //    if (string.IsNullOrWhiteSpace(groupId))
    //        throw new ArgumentNullException(nameof(groupId));
    //    using var request = new HttpRequestMessage(HttpMethod.Delete, $"{PathPrefix}/groups/{groupId}/artifacts/{artifactId}");
    //    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
    //    var content = await response.Content?.ReadAsStringAsync(cancellationToken)!;
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        this.Logger.LogError("An error occured while deleting the artifact with the specified id '{artifactId}': the remote server responded with a non-success status code '{statusCode}'./r/Response content: {content}", artifactId, response.StatusCode, content);
    //        response.EnsureSuccessStatusCode();
    //    }
    //}

    ///// <inheritdoc/>
    //public virtual async Task DeleteAllArtifactsInGroupAsync(string groupId, CancellationToken cancellationToken = default)
    //{
    //    if (string.IsNullOrWhiteSpace(groupId))
    //        throw new ArgumentNullException(nameof(groupId));
    //    using var request = new HttpRequestMessage(HttpMethod.Delete, $"{PathPrefix}/groups/{groupId}/artifacts");
    //    using var response = await this.HttpClient.SendAsync(request, cancellationToken);
    //    var content = await response.Content?.ReadAsStringAsync(cancellationToken)!;
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        this.Logger.LogError("An error occured while deleting artifacts belongs to the group with the specified id '{groupId}': the remote server responded with a non-success status code '{statusCode}'./r/Response content: {content}", groupId, response.StatusCode, content);
    //        response.EnsureSuccessStatusCode();
    //    }
    //}

    #endregion
}

