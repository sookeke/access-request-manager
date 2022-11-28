using System.Net;
using UserAccessManager.Services.Kafka.Models;

namespace UserAccessManager.Services.Kafka.Consumer.ConsumerRetry;
public class UserManagerRetryServiceConsumer : BackgroundService
{
    private readonly IKafkaConsumer<string, RetryAccessRequest> consumer;

    private readonly AccessRequestConfiguration config;
    public UserManagerRetryServiceConsumer(IKafkaConsumer<string, RetryAccessRequest> kafkaConsumer, AccessRequestConfiguration config)
    {
        consumer = kafkaConsumer;
        this.config = config;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var retryTopics = new List<string>() { config.KafkaCluster.InitialRetryTopicName, config.KafkaCluster.MidRetryTopicName, config.KafkaCluster.FinalRetryTopic };
            await consumer.RetryConsume(retryTopics, stoppingToken);
            // await this.consumer.FinalRetryConsume(new List<string>() { config.KafkaCluster.FinalRetryTopic }, stoppingToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{(int)HttpStatusCode.InternalServerError} ConsumeFailedOnTopic - {config.KafkaCluster.ConsumerTopicName}, {ex}");
        }
    }

    public override void Dispose()
    {
        consumer.CloseRetry();
        consumer.DisposeRetry();

        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
