using AnalyzerApi.Domain.Dtos;
using AnalyzerApi.Domain.Models;
using AutoMapper;

namespace AuthenticationApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ServerStatistics, ServerStatisticsResponse>();
        }
    }
}