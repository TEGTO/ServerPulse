using AnalyzerApi.Infrastructure.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using MessageBus.Models;

namespace AnalyzerApi.Services.SerializeStrategies
{
    public class PulseEventSerializeStrategy : IEventSerializeStrategy<PulseEventWrapper>
    {
        private readonly IMapper mapper;

        public PulseEventSerializeStrategy(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public PulseEventWrapper? SerializeResponse(ConsumeResponse response)
        {
            if (response.TryDeserializeEventWrapper<PulseEvent, PulseEventWrapper>(mapper, out PulseEventWrapper ev))
            {
                return ev;
            }
            return null;
        }
    }
}
