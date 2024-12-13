using AnalyzerApi.Infrastructure;
using MessageBus.Interfaces;

namespace AnalyzerApi.Services.Receivers
{
    public abstract class BaseReceiver
    {
        protected readonly IMessageConsumer messageConsumer;
        protected readonly int timeoutInMilliseconds;

        protected BaseReceiver(IMessageConsumer messageConsumer, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
        }

        protected async Task<ConsumeResponse?> GetLastMessageByKeyAsync(string topic, CancellationToken cancellationToken)
        {
            return await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
        }

        protected static string GetTopic(string baseTopic, string key)
        {
            return baseTopic + key;
        }
    }
}