using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using MessageBus.Models;

namespace AnalyzerApi.Application.Services.SerializeStrategies
{
    public sealed class LoadEventSerializeStrategy : IEventSerializeStrategy<LoadEventWrapper>
    {
        private readonly IMapper mapper;

        public LoadEventSerializeStrategy(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public LoadEventWrapper? SerializeResponse(ConsumeResponse response)
        {
            if (response.TryDeserializeEventWrapper<LoadEvent, LoadEventWrapper>(mapper, out LoadEventWrapper? ev))
            {
                return ev;
            }
            return null;
        }
    }
}
