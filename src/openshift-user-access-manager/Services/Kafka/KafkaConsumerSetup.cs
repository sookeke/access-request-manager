using Confluent.Kafka;
using UserAccessManager.Extensions;
using UserAccessManager.Models;
using UserAccessManager.Services.Kafka.Consumer;
using UserAccessManager.Services.Kafka.Consumer.ConsumerRetry;
using UserAccessManager.Services.Kafka.Models;

namespace UserAccessManager.Services.Kafka;

public static class KafkaConsumerSetup
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, AccessRequestConfiguration config)
    {
        services.ThrowIfNull(nameof(services));
        config.ThrowIfNull(nameof(config));

        var clientConfig = new ClientConfig()
        {
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerTokenEndpointUrl = config.KafkaCluster.SaslOauthbearerTokenEndpointUrl,
            SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
            SaslOauthbearerScope = "oidc",
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https,
            SslCaLocation = config.KafkaCluster.SslCaLocation,
        };
        var producerConfig = new ProducerConfig
        {
            Acks = Acks.All,
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslOauthbearerClientId = config.KafkaCluster.SaslOauthbearerProducerClientId,
            SaslOauthbearerClientSecret = config.KafkaCluster.SaslOauthbearerProducerClientSecret,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerTokenEndpointUrl = config.KafkaCluster.SaslOauthbearerTokenEndpointUrl,
            SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
            SaslOauthbearerScope = "oidc",
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https,
            SslCaLocation = config.KafkaCluster.SslCaLocation,
            SslCertificateLocation = config.KafkaCluster.SslCertificateLocation,
            EnableIdempotence = true,
            RetryBackoffMs = 1000,
            MessageSendMaxRetries = 5
        };
        
        var consumerConfig = new ConsumerConfig(clientConfig)
        {
            GroupId = "ocp-accessrequest-consumer-group2",
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            SaslOauthbearerClientId = config.KafkaCluster.SaslOauthbearerConsumerClientId,
            SaslOauthbearerClientSecret = config.KafkaCluster.SaslOauthbearerConsumerClientSecret,
            EnableAutoOffsetStore = false,
            AutoCommitIntervalMs = 4000,
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl            
        };
       
        services.AddSingleton(producerConfig);


        services.AddSingleton(consumerConfig);

        services.AddScoped<IKafkaHandler<string, AccessRequest>, UserProvisioningHandler>();  
        services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));
        services.AddSingleton(typeof(IKafkaProducer<,>), typeof(KafkaProducer<,>));
        services.AddScoped<RetryPolicy>();
        services.AddHostedService<UserManagerServiceConsumer>();

        services.AddScoped<IKafkaHandler<string, RetryAccessRequest>, RetryProvisioningHandler>();
        services.AddHostedService<UserManagerRetryServiceConsumer>();

        return services;
    }
}

