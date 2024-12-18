using AnalyzerApi.Infrastructure;
using MessageBus.Interfaces;
using MessageBus.Models;

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
            return await messageConsumer.GetLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
        }

        protected static string GetTopic(string baseTopic, string key)
        {
            return baseTopic + key;
        }
    }
}