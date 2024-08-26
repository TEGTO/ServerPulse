using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using Confluent.Kafka;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Services.Receivers.Event
{
    public record class EventReceiverTopicData<TWrapper>(string topicOriginName) where TWrapper : BaseEventWrapper;
    public class EventReceiver<TEvent, TWrapper> : BaseReceiver, IEventReceiver<TWrapper>
        where TEvent : BaseEvent where TWrapper : BaseEventWrapper
    {
        protected readonly EventReceiverTopicData<TWrapper> topicData;

        public EventReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration, EventReceiverTopicData<TWrapper> topicData) : base(messageConsumer, mapper, configuration)
        {
            this.topicData = topicData;
        }

        #region IEventReceiver<TWrapper> Members

        public virtual async IAsyncEnumerable<TWrapper> ConsumeEventAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.topicOriginName, key);
            await foreach (var response in ConsumMessageAsync(topic, cancellationToken))
            {
                var ev = ConvertToEventWrapper(response, mapper);
                if (ev != null)
                {
                    yield return ev;
                }
            }
        }
        public virtual async Task<IEnumerable<TWrapper>> GetCertainAmountOfEventsAsync(ReadCertainMessageNumberOptions options, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.topicOriginName, options.Key);
            var messageOptions = new ReadSomeMessagesOptions(topic, timeoutInMilliseconds, options.NumberOfMessages, options.StartDate, options.ReadNew);
            List<ConsumeResponse> responses = await messageConsumer.ReadSomeMessagesAsync(messageOptions, cancellationToken);
            return ConvertToEventWrappers(responses, mapper);
        }
        public virtual async Task<IEnumerable<TWrapper>> ReceiveEventsInRangeAsync(InRangeQueryOptions options, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.topicOriginName, options.Key);
            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            List<ConsumeResponse> responses = await messageConsumer.ReadMessagesInDateRangeAsync(messageOptions, cancellationToken);
            return ConvertToEventWrappers(responses, mapper);
        }
        public virtual async Task<TWrapper?> ReceiveLastEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.topicOriginName, key);
            var response = await ReceiveLastMessageByKeyAsync(topic, cancellationToken);
            if (response != null)
            {
                return ConvertToEventWrapper(response, mapper);
            }
            return null;
        }
        public virtual async Task<int> ReceiveEventAmountByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(topicData.topicOriginName, key);
            return await messageConsumer.GetAmountTopicMessagesAsync(topic, timeoutInMilliseconds, cancellationToken);
        }

        #endregion

        #region Protected Helpers

        protected virtual async IAsyncEnumerable<ConsumeResponse> ConsumMessageAsync(string topic, CancellationToken cancellationToken)
        {
            await foreach (var response in messageConsumer.ConsumeAsync(topic, timeoutInMilliseconds, Offset.End, cancellationToken))
            {
                yield return response;
            }
        }
        protected virtual IEnumerable<TWrapper> ConvertToEventWrappers(List<ConsumeResponse> responses, IMapper mapper)
        {
            List<TWrapper> events = new List<TWrapper>();
            foreach (var response in responses)
            {
                var ev = ConvertToEventWrapper(response, mapper);
                if (ev != null)
                {
                    events.Add(ev);
                }
            }
            return events;
        }
        protected virtual TWrapper? ConvertToEventWrapper(ConsumeResponse response, IMapper mapper)
        {
            if (response.TryDeserializeEventWrapper<TEvent, TWrapper>(mapper, out TWrapper ev))
            {
                return ev;
            }
            return null;
        }

        #endregion
    }
}