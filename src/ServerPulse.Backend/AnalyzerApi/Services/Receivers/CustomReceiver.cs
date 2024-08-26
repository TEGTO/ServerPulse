using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Services.Interfaces;
using AutoMapper;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Services.Receivers
{
    public class CustomReceiver : BaseEventReceiver, ICustomReceiver
    {
        private readonly string customEventTopic;

        public CustomReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration)
            : base(messageConsumer, mapper, configuration)
        {
            customEventTopic = configuration[Configuration.KAFKA_CUSTOM_TOPIC]!;
        }

        #region ICustomReceiver Members

        public async IAsyncEnumerable<CustomEventWrapper> ConsumeCustomEventAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(customEventTopic, key);

            await foreach (var response in ConsumMessageAsync(topic, cancellationToken))
            {
                var ev = ConvertCustomEventWrapper(response, mapper);
                if (ev != null)
                {
                    yield return ev;
                }
            }
        }

        public async Task<IEnumerable<CustomEventWrapper>> ReceiveEventsInRangeAsync(InRangeQueryOptions options, CancellationToken cancellationToken)
        {
            string topic = GetTopic(customEventTopic, options.Key);
            var messageOptions = new MessageInRangeQueryOptions(topic, timeoutInMilliseconds, options.From, options.To);
            List<ConsumeResponse> responses = await messageConsumer.ReadMessagesInDateRangeAsync(messageOptions, cancellationToken);
            return ConvertCustomEventWrappers(responses, mapper);
        }

        public async Task<IEnumerable<CustomEventWrapper>> GetCertainAmountOfEvents(ReadCertainMessageNumberOptions options, CancellationToken cancellationToken)
        {
            string topic = GetTopic(customEventTopic, options.Key);
            var messageOptions = new ReadSomeMessagesOptions(topic, timeoutInMilliseconds, options.NumberOfMessages, options.StartDate, options.ReadNew);
            List<ConsumeResponse> responses = await messageConsumer.ReadSomeMessagesAsync(messageOptions, cancellationToken);
            return ConvertCustomEventWrappers(responses, mapper);
        }

        public async Task<CustomEventWrapper?> ReceiveLastCustomEventByKeyAsync(string key, CancellationToken cancellationToken)
        {
            string topic = GetTopic(customEventTopic, key);
            var response = await ReceiveLastMessageByKeyAsync(topic, cancellationToken);
            if (response != null)
            {
                return ConvertCustomEventWrapper(response, mapper);
            }
            return null;
        }

        #endregion

        #region Private Helpers

        private IEnumerable<CustomEventWrapper> ConvertCustomEventWrappers(List<ConsumeResponse> responses, IMapper mapper)
        {
            List<CustomEventWrapper> events = new List<CustomEventWrapper>();
            foreach (var response in responses)
            {
                var ev = ConvertCustomEventWrapper(response, mapper);
                if (ev != null)
                {
                    events.Add(ev);
                }
            }
            return events;
        }

        private CustomEventWrapper? ConvertCustomEventWrapper(ConsumeResponse response, IMapper mapper)
        {
            if (response.TryDeserializeEventWrapper<CustomEvent, CustomEventWrapper>(mapper, out CustomEventWrapper ev))
            {
                ev.SerializedMessage = response.Message;
                return ev;
            }
            return null;
        }

        #endregion
    }
}