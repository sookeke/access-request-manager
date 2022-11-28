namespace UserAccessManager.Services.Kafka.Consumer
{
    /// <typeparam name="Tv">Indicates the message's value for Kafka Topic</typeparam>
    public interface IKafkaHandler<Tk, Tv>
    {
        /// <summary>
        /// Provide mechanism to handle the consumer message from Kafka
        /// </summary>
        /// <param name="key">Indicates the message's key for Kafka Topic</param>
        /// <param name="value">Indicates the message's value for Kafka Topic</param>
        /// <returns></returns>
        Task<Task> HandleAsync(string consumerName, Tk key, Tv value);
        Task<Task> HandleRetryAsync(string consumerName, Tk key, Tv value, int retryCount, string topicName);
    }
}