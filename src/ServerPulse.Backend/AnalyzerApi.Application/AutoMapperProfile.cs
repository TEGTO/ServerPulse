using AnalyzerApi.Core.Dtos.Responses.Events;
using AnalyzerApi.Core.Dtos.Responses.Statistics;
using AnalyzerApi.Core.Models.Statistics;
using AnalyzerApi.Core.Models.Wrappers;
using AutoMapper;
using EventCommunication;

namespace AnalyzerApi.Application
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ConfigurationEvent, ConfigurationEventWrapper>();
            CreateMap<PulseEvent, PulseEventWrapper>();
            CreateMap<LoadEvent, LoadEventWrapper>();
            CreateMap<CustomEvent, CustomEventWrapper>();

            CreateMap<ConfigurationEventWrapper, ConfigurationEventResponse>();
            CreateMap<PulseEventWrapper, PulseEventResponse>();
            CreateMap<LoadEventWrapper, LoadEventResponse>();
            CreateMap<CustomEventWrapper, CustomEventResponse>();

            CreateMap<BaseStatistics, BaseStatisticsResponse>();
            CreateMap<LoadMethodStatistics, LoadMethodStatisticsResponse>();
            CreateMap<ServerLifecycleStatistics, ServerLifecycleStatisticsResponse>();
            CreateMap<ServerLoadStatistics, ServerLoadStatisticsResponse>();
            CreateMap<LoadAmountStatistics, LoadAmountStatisticsResponse>();
            CreateMap<ServerCustomStatistics, ServerCustomStatisticsResponse>();
        }
    }
}