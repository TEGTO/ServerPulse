using AnalyzerApi.Infrastructure.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using MessageBus.Models;

namespace AnalyzerApi.Services.SerializeStrategies
{
    public class CustomEventSerializeStrategy : IEventSerializeStrategy<CustomEventWrapper>
    {
        private readonly IMapper mapper;

        public CustomEventSerializeStrategy(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public CustomEventWrapper? SerializeResponse(ConsumeResponse response)
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
