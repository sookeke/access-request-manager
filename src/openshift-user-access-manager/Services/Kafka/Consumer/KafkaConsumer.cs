using Confluent.Kafka;
using Octokit;
using UserAccessManager.Services.Kafka.Consumer.ConsumerRetry;

namespace UserAccessManager.Services.Kafka.Consumer;
public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
{
    private readonly ConsumerConfig config;
    private IKafkaHandler<TKey, TValue> handler;
    private IConsumer<TKey, TValue> consumer;
    private IConsumer<TKey, TValue> consumerRetry;
    private RetryPolicy retryPolicy;
    private IEnumerable<string> topics;
    private readonly AccessRequestConfiguration appConfig;

    private readonly IServiceScopeFactory serviceScopeFactory;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public KafkaConsumer(IServiceScopeFactory serviceScopeFactory, RetryPolicy retryPolicy, ConsumerConfig config, AccessRequestConfiguration appConfig)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        this.serviceScopeFactory = serviceScopeFactory;
        this.retryPolicy = retryPolicy;
        this.config = config;
        this.appConfig = appConfig;
    }
    /// <summary>
    /// for production use of sasl/oauthbearer
    /// implement authentication callbackhandler for token retrival and refresh
    /// https://docs.confluent.io/platform/current/kafka/authentication_sasl/authentication_sasl_oauth.html#production-use-of-sasl-oauthbearer
    /// https://techcommunity.microsoft.com/t5/fasttrack-for-azure/event-hub-kafka-endpoint-azure-ad-authentication-using-c/ba-p/2586185
    /// https://github.com/Azure/azure-event-hubs-for-kafka/issues/97
    /// https://github.com/confluentinc/confluent-kafka-dotnet/blob/master/test/Confluent.Kafka.IntegrationTests/Tests/OauthBearerToken_PublishConsume.cs
    /// </summary>
    /// <param name="config"></param>

    public async Task Consume(IEnumerable<string> topics, CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
        consumer = new ConsumerBuilder<TKey, TValue>(config).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
        this.topics = topics;

        await Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }
    public async Task RetryConsume(IEnumerable<string> topics, CancellationToken stoppingToken)
    {
        config.GroupId = $"retry-topic-consumer-group";
        using var scope = serviceScopeFactory.CreateScope();

        handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
        consumerRetry = new ConsumerBuilder<TKey, TValue>(config).SetValueDeserializer(new KafkaDeserializer<TValue>()).Build();
        this.topics = topics;

        await Task.Run(() => StartRetryConsumerLoop(stoppingToken), stoppingToken);
    }
    /// <summary>
    /// This will close the consumer, commit offsets and leave the group cleanly.
    /// </summary>
    public void Close() => consumer.Close();
    /// <summary>
    /// Releases all resources used by the current instance of the consumer
    /// </summary>
    public void Dispose() => consumer.Dispose();
    public void CloseRetry() => consumerRetry.Close();
    /// <summary>
    /// Releases all resources used by the current instance of the consumer
    /// </summary>
    public void DisposeRetry() => consumerRetry.Dispose();

    private async Task StartConsumerLoop(CancellationToken cancellationToken)
    {
        consumer.Subscribe(topics);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(cancellationToken);
                if (result != null)
                {
                    var consumerResult = await handler.HandleAsync(consumer.MemberId, result.Message.Key, result.Message.Value);

                    if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
                    {
                        consumer.Commit(result);
                        consumer.StoreOffset(result);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                Console.WriteLine($"Consume error: {e.Error.Reason}");

                if (e.Error.IsFatal)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e}");
                break;
            }
        }
    }
    private async Task StartRetryConsumerLoop(CancellationToken cancellationToken)
    {
        consumerRetry.Subscribe(topics);
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = consumerRetry.Consume(cancellationToken);
                await RetryConsumerResult(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                Console.WriteLine($"Consume error: {e.Error.Reason}");

                if (e.Error.IsFatal)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e}");
                break;
            }
        }
    }

    private async Task RetryConsumerResult(ConsumeResult<TKey, TValue> result)
    {
        if (result != null)
        {
            if (result.Topic == appConfig.KafkaCluster.InitialRetryTopicName)
            {
                var retryContext = new Polly.Context { { "retrycount", 0 } };
                var consumerResult = await retryPolicy.ImmediateConsumerRetry.ExecuteAsync(
                    async context => await handler.HandleRetryAsync(consumerRetry.MemberId, result.Message.Key, result.Message.Value, (int)context["retrycount"], result.Topic), retryContext);

                if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
                {
                    consumerRetry.Commit(result);
                    consumerRetry.StoreOffset(result);
                }
            }
            else if (result.Topic == appConfig.KafkaCluster.MidRetryTopicName)
            {
                var retryContext = new Polly.Context { { "retrycount", 0 } };
                var consumerResult = await retryPolicy.WaitForConsumerRetry.ExecuteAsync(
                    async context => await handler.HandleRetryAsync(consumerRetry.MemberId, result.Message.Key, result.Message.Value, (int)context["retrycount"], result.Topic), retryContext);

                if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
                {
                    consumerRetry.Commit(result);
                    consumerRetry.StoreOffset(result);
                }
            }
            else if (result.Topic == appConfig.KafkaCluster.FinalRetryTopic)
            {
                var retryContext = new Polly.Context { { "retrycount", 0 } };
                var consumerResult = await retryPolicy.FinalWaitForConsumerRetry.ExecuteAsync(
                    async context => await handler.HandleRetryAsync(consumerRetry.MemberId, result.Message.Key, result.Message.Value, (int)context["retrycount"], result.Topic), retryContext);

                if (consumerResult.Status == TaskStatus.RanToCompletion && consumerResult.Exception == null)
                {
                    consumerRetry.Commit(result);
                    consumerRetry.StoreOffset(result);
                }
            }
        }
    }

}