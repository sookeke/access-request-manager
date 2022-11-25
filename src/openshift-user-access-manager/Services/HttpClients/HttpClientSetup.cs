using IdentityModel.Client;
using Microsoft.Net.Http.Headers;
using UserAccessManager.Extensions;
using UserAccessManager.Services.Kafka;
using UserAccessManager.Services.ServiceNow;
using ServiceNow.Api;

namespace UserAccessManager.Services.HttpClients;
public static class HttpClientSetup
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services, AccessRequestConfiguration config)
    {
        services.AddHttpClient<IAccessTokenClient, AccessTokenClient>();

        var keyValues = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("username", config.ServiceNow.UserName),
            new KeyValuePair<string, string>("password",config.ServiceNow.Password),
            new KeyValuePair<string, string>("grant_type", "password")
        };
        services.AddHttpClientWithBaseAddress<IServiceNowTableClient, ServiceNowTableClient>(config.ServiceNow.ApiUrl)
            .ConfigureHttpClient(httpClient =>
            {
                httpClient.BaseAddress = new Uri(config.ServiceNow.ApiUrl);
                httpClient.DefaultRequestHeaders.Add(
                   HeaderNames.Accept, "application/json");
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Authorization, "basic");
                httpClient.SetBasicAuthentication(config.ServiceNow.UserName, config.ServiceNow.Password);
            });

        //services.AddHttpClient<ServiceNowClient>()
        //    .ConfigureHttpClient(httpClient =>
        //{
        //    httpClient.BaseAddress = new Uri(config.ServiceNow.ApiUrl);
        //    httpClient.DefaultRequestHeaders.Add(
        //       HeaderNames.Accept, "application/json");
        //    httpClient.DefaultRequestHeaders.Add(
        //        HeaderNames.Authorization, "basic");
        //    httpClient.SetBasicAuthentication(config.ServiceNow.UserName, config.ServiceNow.Password);
        //});

        return services;
    }
    public static IHttpClientBuilder AddHttpClientWithBaseAddress<TClient, TImplementation>(this IServiceCollection services, string baseAddress)
       where TClient : class
       where TImplementation : class, TClient
       => services.AddHttpClient<TClient, TImplementation>(client => client.BaseAddress = new Uri(baseAddress.EnsureTrailingSlash()));

    public static IHttpClientBuilder WithBearerToken<T>(this IHttpClientBuilder builder, T credentials) where T : ClientCredentialsTokenRequest
    {
        builder.Services.AddSingleton(credentials)
            .AddTransient<BearerTokenHandler<T>>();

        builder.AddHttpMessageHandler<BearerTokenHandler<T>>();

        return builder;
    }
}

