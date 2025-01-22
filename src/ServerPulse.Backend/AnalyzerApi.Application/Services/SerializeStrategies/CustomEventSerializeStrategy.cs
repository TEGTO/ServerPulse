using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using MessageBus.Models;

namespace AnalyzerApi.Application.Services.SerializeStrategies
{
    public sealed class CustomEventSerializeStrategy : IEventSerializeStrategy<CustomEventWrapper>
    {
        private readonly IMapper mapper;

        public CustomEventSerializeStrategy(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public CustomEventWrapper? SerializeResponse(ConsumeResponse response)
        {
            if (response.TryDeserializeEventWrapper<CustomEvent, CustomEventWrapper>(
                mapper, out CustomEventWrapper? ev) && ev != null)
            {
                ev.SerializedMessage = response.Message;
                return ev;
            }
            return null;
        }
    }
}
