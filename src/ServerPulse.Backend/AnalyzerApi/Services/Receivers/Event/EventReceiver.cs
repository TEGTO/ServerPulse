using AnalyzerApi.Infrastructure.Configuration;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Infrastructure.TopicMapping;
using AnalyzerApi.Services.SerializeStrategies;
using Confluent.Kafka;
using MessageBus.Interfaces;
using MessageBus.Models;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace AnalyzerApi.Services.Receivers.Event
{
    public class EventReceiver<TWrapper> : BaseReceiver, IEventReceiver<TWrapper> where TWrapper : BaseEventWrapper
    {
        protected readonly IEventSerializeStrategy<TWrapper> eventSerializeStrategy;
        protected readonly EventTopicMapping<TWrapper> topicData;

        public EventReceiver(
            IMessageConsumer messageConsumer,
            IOptions<MessageBusSettings> options,
            IEventSerializeStrategy<TWrapper> eventSerializeStrategy,
            EventTopicMapping<TWrapper> topicData) : base(messageConsumer, options)
        {
            this.topicData = topicData;
            this.eventSerializeStrategy = eventSerializeStrategy;
        }

        #region IEventReceiver<TWrapper> Members

        public async IAsyncEnumerable<TWrapper> GetEventStreamAsync(string key, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var topic = GetTopic(topicData.TopicOriginName, key);

            await foreach (var response in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                var ev = eventSerializeStrategy.SerializeResponse(response);
                if (ev != null)
                {
                    yield return ev;
                }
            }
        }

        public async Task<int> GetEventAmountByKeyAsync(string key, CancellationToken cancellationToken)
        {
            var topic = GetTopic(topicData.TopicOriginName, key);
            return await messageConsumer.GetTopicMessageAmountAsync(topic, timeoutInMilliseconds, cancellationToken);
        }

        public async Task<IEnumerable<TWrapper>> GetCertainAmountOfEventsAsync(GetCertainMessageNumberOptions options, CancellationToken cancellationToken)
        {
            var topic = GetTopic(topicData.TopicOriginName, options.Key);

            var messageOptions = new GetSomeMessagesFromDateOptions(topic, timeoutInMilliseconds, options.NumberOfMessages, options.StartDate, options.ReadNew);
            var responses = await messageConsumer.GetSomeMessagesStartFromDateAsync(messageOptions, cancellationToken);

            return ConvertToEventWrappers(responses);
        }

        public async Task<IEnumerable<TWrapper>> GetEventsInRangeAsync(GetInRangeOptions options, CancellationToken cancellationToken)
        {
            var topic = GetTopic(topicData.TopicOriginName, options.Key);

            var messageOptions = new GetMessageInDateRangeOptions(topic, timeoutInMilliseconds, options.From, options.To);
            var responses = await messageConsumer.GetMessagesInDateRangeAsync(messageOptions, cancellationToken);

            return ConvertToEventWrappers(responses);
        }

        public async Task<TWrapper?> GetLastEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            var topic = GetTopic(topicData.TopicOriginName, key);

            var response = await GetLastMessageByKeyAsync(topic, cancellationToken);
            if (response != null)
            {
                return eventSerializeStrategy.SerializeResponse(response);
            }

            return null;
        }

        #endregion

        #region Protected Helpers

        protected IEnumerable<TWrapper> ConvertToEventWrappers(IEnumerable<ConsumeResponse> responses)
        {
            var events = new List<TWrapper>();

            foreach (var response in responses)
            {
                var ev = eventSerializeStrategy.SerializeResponse(response);
                if (ev != null)
                {
                    events.Add(ev);
                }
            }

            return events;
        }


        #endregion
    }
}