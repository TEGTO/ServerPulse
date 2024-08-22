using AnalyzerApi.Domain.Dtos.Wrappers;
using AutoMapper;
using Confluent.Kafka;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Services
{
    public abstract class BaseEventReceiver
    {
        protected readonly IMessageConsumer messageConsumer;
        protected readonly IMapper mapper;
        protected readonly int timeoutInMilliseconds;

        protected BaseEventReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration)
        {
            this.messageConsumer = messageConsumer;
            this.mapper = mapper;
            this.timeoutInMilliseconds = int.Parse(configuration[Configuration.KAFKA_TIMEOUT_IN_MILLISECONDS]!);
        }

        protected async IAsyncEnumerable<TWrapper> ConsumeEventAsync<TEvent, TWrapper>(string topic, CancellationToken cancellationToken)
            where TEvent : BaseEvent
            where TWrapper : BaseEventWrapper
        {
            await foreach (var response in ConsumMessageAsync(topic, cancellationToken))
            {
                if (response.TryDeserializeEventWrapper<TEvent, TWrapper>(mapper, out TWrapper ev))
                {
                    yield return ev;
                }
            }
        }

        protected async IAsyncEnumerable<ConsumeResponse> ConsumMessageAsync(string topic, CancellationToken cancellationToken)
        {
            await foreach (var response in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                yield return response;
            }
        }

        protected async Task<TWrapper?> ReceiveLastEventByKeyAsync<TEvent, TWrapper>(string topic, CancellationToken cancellationToken)
            where TEvent : BaseEvent
            where TWrapper : BaseEventWrapper
        {
            ConsumeResponse? response = await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
            if (response != null && response.TryDeserializeEventWrapper<TEvent, TWrapper>(mapper, out TWrapper ev))
            {
                return ev;
            }
            return null;
        }

        protected async Task<ConsumeResponse?> ReceiveLastMessageByKeyAsync(string topic, CancellationToken cancellationToken)
        {
            return await messageConsumer.ReadLastTopicMessageAsync(topic, timeoutInMilliseconds, cancellationToken);
        }

        protected string GetTopic(string baseTopic, string key)
        {
            return baseTopic + key;
        }
    }
}