using UserAccessManager.Models;
using System.Net;

namespace UserAccessManager.Services.Kafka
{
    public class UserManagerServiceConsumer : BackgroundService
    {
        private readonly IKafkaConsumer<string, AccessRequest> consumer;

        private readonly AccessRequestConfiguration config;
        public UserManagerServiceConsumer(IKafkaConsumer<string, AccessRequest> kafkaConsumer, AccessRequestConfiguration config)
        {
            this.consumer = kafkaConsumer;
            this.config = config;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await this.consumer.Consume(this.config.KafkaCluster.ConsumerTopicName, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{(int)HttpStatusCode.InternalServerError} ConsumeFailedOnTopic - {this.config.KafkaCluster.ConsumerTopicName}, {ex}");
            }
        }

        public override void Dispose()
        {
            this.consumer.Close();
            this.consumer.Dispose();

            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}