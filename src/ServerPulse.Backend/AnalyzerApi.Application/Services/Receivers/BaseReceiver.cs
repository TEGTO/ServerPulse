using AnalyzerApi.Application.Configuration;
using MessageBus.Interfaces;
using MessageBus.Models;
using Microsoft.Extensions.Options;

namespace AnalyzerApi.Application.Services.Receivers
{
    internal abstract class BaseReceiver
    {
        protected readonly IMessageConsumer messageConsumer;
        protected readonly int timeoutInMilliseconds;

        protected BaseReceiver(IMessageConsumer messageConsumer, IOptions<MessageBusSettings> options)
        {
            this.messageConsumer = messageConsumer;
            timeoutInMilliseconds = options.Value.ReceiveTimeoutInMilliseconds;
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