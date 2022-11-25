namespace UserAccessManager.Services.Kafka;
public class AccessRequestConfiguration
{
    public static bool IsProduction() => EnvironmentName == Environments.Production;
    public static bool IsDevelopment() => EnvironmentName == Environments.Development;
    private static readonly string? EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

    public AddressAutocompleteClientConfiguration AddressAutocompleteClient { get; set; } = new();
    public KafkaClusterConfiguration KafkaCluster { get; set; } = new();
    public ServiceNowConfiguration ServiceNow { get; set; } = new();
    public GithubClientConfiguration GithubClient { get; set; } = new();

    public KubernateConfigConfiguration KubernateConfig { get; set; } = new();

    // ------- Configuration Objects -------

    public class AddressAutocompleteClientConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class KubernateConfigConfiguration
    {
        public string KubeConfigLocation { get; set; } = string.Empty;
    }
    public class GithubClientConfiguration
    {
        public string RepositoryName { get; set; } = string.Empty;
        public string RepositoryOwner { get; set; } = string.Empty;
        public string Branch { get; set; } = string.Empty;
        public string PersonalAccessToken { get; set; } = string.Empty;
    }
    public class ServiceNowConfiguration
    {
        public string ApiUrl { get; set; } = string.Empty;
        public string ServiceNowOauthClientId { get; set; } = string.Empty;
        public string ServiceNowOauthClientSecret { get; set; } = string.Empty;
        public string TokenUrl { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class KafkaClusterConfiguration
    {
        public string Url { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string BoostrapServers { get; set; } = string.Empty;
        public string ConsumerTopicName { get; set; } = string.Empty;
        public string ProducerTopicName { get; set; } = string.Empty;
        public string SaslOauthbearerTokenEndpointUrl { get; set; } = string.Empty;
        public string SaslOauthbearerProducerClientId { get; set; } = string.Empty;
        public string SaslOauthbearerProducerClientSecret { get; set; } = string.Empty;
        public string SaslOauthbearerConsumerClientId { get; set; } = string.Empty;
        public string SaslOauthbearerConsumerClientSecret { get; set; } = string.Empty;
        public string SslCaLocation { get; set; } = string.Empty;
        public string SslCertificateLocation { get; set; } = string.Empty;
        public string SslKeyLocation { get; set; } = string.Empty;
    }
}
