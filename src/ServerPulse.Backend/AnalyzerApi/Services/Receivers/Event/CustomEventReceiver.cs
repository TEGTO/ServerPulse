using AnalyzerApi.Domain.Dtos.Wrappers;
using AutoMapper;
using MessageBus.Interfaces;
using ServerPulse.EventCommunication.Events;

namespace AnalyzerApi.Services.Receivers.Event
{
    public class CustomEventReceiver : EventReceiver<CustomEvent, CustomEventWrapper>
    {
        private readonly string customEventTopic;

        public CustomEventReceiver(IMessageConsumer messageConsumer, IMapper mapper, IConfiguration configuration, EventReceiverTopicData<CustomEventWrapper> topicData) : base(messageConsumer, mapper, configuration, topicData)
        {
            customEventTopic = configuration[Configuration.KAFKA_CUSTOM_TOPIC]!;
        }

        protected override CustomEventWrapper? ConvertToEventWrapper(ConsumeResponse response, IMapper mapper)
        {
            if (response.TryDeserializeEventWrapper<CustomEvent, CustomEventWrapper>(mapper, out CustomEventWrapper ev))
            {
                ev.SerializedMessage = response.Message;
                return ev;
            }
            return null;
        }
    }
}