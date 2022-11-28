using Polly;
using Polly.Retry;

namespace UserAccessManager.Services.Kafka.Consumer.ConsumerRetry;
public class RetryPolicy
{
    public AsyncRetryPolicy<Task> ImmediateConsumerRetry { get; }
    public AsyncRetryPolicy<Task> WaitForConsumerRetry { get; }
    private readonly AccessRequestConfiguration config;

    public AsyncRetryPolicy<Task> FinalWaitForConsumerRetry { get; }

    public RetryPolicy(AccessRequestConfiguration config)
    {
        this.config = config;
        ImmediateConsumerRetry = Policy.HandleResult<Task>(
            res => !res.IsCompleted || res.Exception != null)
            .WaitAndRetryAsync(this.config.RetryPolicy.InitialRetryTopicName.RetryCount, retryAttempt => TimeSpan.FromMinutes(this.config.RetryPolicy.InitialRetryTopicName.WaitAfterInMins),
            onRetry: (response, delay, retryCount, context) =>
            {
                context["retrycount"] = retryCount;
            });
        WaitForConsumerRetry = Policy.HandleResult<Task>(
            res => res.Status != TaskStatus.RanToCompletion && res.Exception != null)
          .WaitAndRetryAsync(this.config.RetryPolicy.MidRetryTopicName.RetryCount, retryAttempt => TimeSpan.FromMinutes(this.config.RetryPolicy.MidRetryTopicName.WaitAfterInMins), //retry nth times every nth minutes
            onRetry: (response, delay, retryCount, context) =>
            {
                context["retrycount"] = retryCount;
            });
        FinalWaitForConsumerRetry = Policy.HandleResult<Task>(
            res => res.Status != TaskStatus.RanToCompletion && res.Exception != null)
                .WaitAndRetryAsync(this.config.RetryPolicy.FinalRetryTopicName.RetryCount, retryAttempt => TimeSpan.FromMinutes(this.config.RetryPolicy.FinalRetryTopicName.WaitAfterInMins),
            onRetry: (response, delay, retryCount, context) =>
            {
                context["retrycount"] = retryCount;
            });
    }
}

