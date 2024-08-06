using AnalyzerApi.Domain.Dtos;
using AnalyzerApi.Domain.Models;
using AutoMapper;
using ServerPulse.EventCommunication.Events;

namespace AuthenticationApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ServerStatistics, ServerStatisticsResponse>();
            CreateMap<LoadEvent, ServerLoadResponse>();
            CreateMap<ServerLoadStatistics, ServerLoadStatisticsResponse>();
        }
    }
}