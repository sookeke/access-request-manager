using Polly;
using Polly.Retry;

namespace UserAccessManager.Services.Kafka;
public class RetryPolicy
{
    public AsyncRetryPolicy<Task> ImmediateConsumerRetry { get; }

    public RetryPolicy()
    {
        ImmediateConsumerRetry = Policy.HandleResult<Task>(
            res => !res.IsCompleted || res.Exception != null)
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(20));
    }
}

