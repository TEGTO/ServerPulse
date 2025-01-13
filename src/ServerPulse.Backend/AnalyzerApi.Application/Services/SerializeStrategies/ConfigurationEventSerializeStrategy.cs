using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using EventCommunication;
using MessageBus.Models;

namespace AnalyzerApi.Application.Services.SerializeStrategies
{
    internal class ConfigurationEventSerializeStrategy : IEventSerializeStrategy<ConfigurationEventWrapper>
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
