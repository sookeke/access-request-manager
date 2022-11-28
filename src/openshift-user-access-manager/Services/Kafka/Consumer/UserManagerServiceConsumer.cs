using UserAccessManager.Models;
using System.Net;

namespace UserAccessManager.Services.Kafka.Consumer
{
    public class UserManagerServiceConsumer : BackgroundService
    {
        private readonly IKafkaConsumer<string, AccessRequest> consumer;

        private readonly AccessRequestConfiguration config;
        public UserManagerServiceConsumer(IKafkaConsumer<string, AccessRequest> kafkaConsumer, AccessRequestConfiguration config)
        {
            consumer = kafkaConsumer;
            this.config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await consumer.Consume(new List<string> { config.KafkaCluster.ConsumerTopicName }, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{(int)HttpStatusCode.InternalServerError} ConsumeFailedOnTopic - {config.KafkaCluster.ConsumerTopicName}, {ex}");
            }
        }

        public override void Dispose()
        {
            consumer.Close();
            consumer.Dispose();

            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}