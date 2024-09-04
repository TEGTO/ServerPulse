using AnalyzerApi.Domain.Dtos.Responses;
using AnalyzerApi.Domain.Dtos.Wrappers;
using AnalyzerApi.Domain.Models;
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
            CreateMap<CustomEventStatistics, CustomEventStatisticsResponse>();
            CreateMap<SlotData, SlotDataResponse>();
        }
    }
}