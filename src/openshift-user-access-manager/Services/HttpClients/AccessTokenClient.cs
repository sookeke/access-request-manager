namespace UserAccessManager.Services.HttpClients;

using IdentityModel.Client;

public class AccessTokenClient : IAccessTokenClient
{
    private readonly HttpClient client;

    public AccessTokenClient(HttpClient client) => this.client = client ?? throw new ArgumentNullException(nameof(client));

    public async Task<string> GetAccessTokenAsync(ClientCredentialsTokenRequest request)
    {
        var response = await this.client.RequestClientCredentialsTokenAsync(request);
        return response.AccessToken;
    }
}
