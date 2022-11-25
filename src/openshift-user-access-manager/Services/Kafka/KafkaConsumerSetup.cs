using Confluent.Kafka;
using UserAccessManager.Extensions;
using UserAccessManager.Models;

namespace UserAccessManager.Services.Kafka;

public static class KafkaConsumerSetup
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, AccessRequestConfiguration config)
    {
        services.ThrowIfNull(nameof(services));
        config.ThrowIfNull(nameof(config));
        //var serviceProvider = services.BuildServiceProvider();
        //var logger = serviceProvider.GetService<ILogger<UserProvisioningHandler>>();
        //services.AddSingleton(typeof(ILogger), logger);

        var clientConfig = new ClientConfig()
        {
            BootstrapServers = config.KafkaCluster.BoostrapServers,
            SaslMechanism = SaslMechanism.OAuthBearer,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslOauthbearerTokenEndpointUrl = config.KafkaCluster.SaslOauthbearerTokenEndpointUrl,
            SaslOauthbearerMethod = SaslOauthbearerMethod.Oidc,
            SaslOauthbearerScope = "oidc",
            SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.Https,
            SslCaLocation = config.KafkaCluster.SslCaLocation
            //SslCertificateLocation = config.KafkaCluster.SslCertificateLocation,
            //SslKeyLocation = config.KafkaCluster.SslKeyLocation
        };
        var producerConfig = new ProducerConfig(clientConfig)
        {
            Acks = Acks.All,
            SaslOauthbearerClientId = config.KafkaCluster.SaslOauthbearerProducerClientId,
            SaslOauthbearerClientSecret = config.KafkaCluster.SaslOauthbearerProducerClientSecret,
            EnableIdempotence = true
        };

        var consumerConfig = new ConsumerConfig(clientConfig)
        {
            GroupId = "ocp-accessrequest-consumer-group",
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
        //var producerConfig = new ProducerConfig(clientConfig);
        services.AddSingleton(consumerConfig);
        services.AddSingleton(producerConfig);


        //services.AddSingleton(consumerConfig);

        services.AddScoped<IKafkaHandler<string, AccessRequest>, UserProvisioningHandler>();
        services.AddSingleton(typeof(IKafkaConsumer<,>), typeof(KafkaConsumer<,>));
        services.AddScoped<RetryPolicy>();
        services.AddHostedService<UserManagerServiceConsumer>();

        return services;
    }
}

