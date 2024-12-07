using AnalyzerApi.Infrastructure.Dtos.Responses.Statistics;
using AnalyzerApi.Infrastructure.Models;
using AnalyzerApi.Infrastructure.Wrappers;
using AutoMapper;
using ServerPulse.EventCommunication.Events;

namespace AuthenticationApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ConfigurationEvent, ConfigurationEventWrapper>();
            CreateMap<PulseEvent, PulseEventWrapper>();
            CreateMap<LoadEvent, LoadEventWrapper>();
            CreateMap<CustomEvent, CustomEventWrapper>();

            CreateMap<LoadMethodStatistics, LoadMethodStatisticsResponse>();
            CreateMap<BaseStatistics, BaseStatisticsResponse>();
            CreateMap<ServerStatistics, ServerStatisticsResponse>();
            CreateMap<ServerLoadStatistics, ServerLoadStatisticsResponse>();
            CreateMap<LoadAmountStatistics, LoadAmountStatisticsResponse>();
            CreateMap<ServerCustomStatistics, CustomEventStatisticsResponse>();
        }
    }
}