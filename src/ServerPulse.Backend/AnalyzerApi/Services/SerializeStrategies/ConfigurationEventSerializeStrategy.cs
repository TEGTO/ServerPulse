using AnalyzerApi.Infrastructure.Models.Wrappers;
using AutoMapper;
using EventCommunication.Events;
using MessageBus.Interfaces;

namespace AnalyzerApi.Services.SerializeStrategies
{
    public class ConfigurationEventSerializeStrategy : IEventSerializeStrategy<ConfigurationEventWrapper>
    {
        private readonly IMapper mapper;

        public ConfigurationEventSerializeStrategy(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public ConfigurationEventWrapper? SerializeResponse(ConsumeResponse response)
        {
            if (response.TryDeserializeEventWrapper<ConfigurationEvent, ConfigurationEventWrapper>(mapper, out ConfigurationEventWrapper ev))
            {
                return ev;
            }
            return null;
        }
    }
}
