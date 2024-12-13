using AnalyzerApi.Infrastructure.Configurations;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Models.Wrappers;
using AnalyzerApi.Services.SerializeStrategies;
using Confluent.Kafka;
using MessageBus.Interfaces;
using System.Runtime.CompilerServices;

namespace AnalyzerApi.Services.Receivers.Event
{
    public class EventReceiver<TWrapper> : BaseReceiver, IEventReceiver<TWrapper> where TWrapper : BaseEventWrapper
    {
        protected readonly IEventSerializeStrategy<TWrapper> eventSerializeStrategy;
        protected readonly EventReceiverTopicConfiguration<TWrapper> topicData;

        public EventReceiver(
            IMessageConsumer messageConsumer,
            IConfiguration configuration,
            IEventSerializeStrategy<TWrapper> eventSerializeStrategy,
            EventReceiverTopicConfiguration<TWrapper> topicData) : base(messageConsumer, configuration)
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
            return await messageConsumer.GetAmountTopicMessagesAsync(topic, timeoutInMilliseconds, cancellationToken);
        }

        public async Task<IEnumerable<TWrapper>> GetCertainAmountOfEventsAsync(ReadCertainMessageNumber options, CancellationToken cancellationToken)
        {
            var topic = GetTopic(topicData.TopicOriginName, options.Key);

            var messageOptions = new ReadSomeMessagesOptions(topic, timeoutInMilliseconds, options.NumberOfMessages, options.StartDate, options.ReadNew);
            var responses = await messageConsumer.ReadSomeMessagesAsync(messageOptions, cancellationToken);

            return ConvertToEventWrappers(responses);
        }

        public async Task<IEnumerable<TWrapper>> GetEventsInRangeAsync(InRangeQuery options, CancellationToken cancellationToken)
        {
            var topic = GetTopic(topicData.TopicOriginName, options.Key);

            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            var responses = await messageConsumer.ReadMessagesInDateRangeAsync(messageOptions, cancellationToken);

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

        protected IEnumerable<TWrapper> ConvertToEventWrappers(List<ConsumeResponse> responses)
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