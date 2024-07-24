using AnalyzerApi.Domain.Models;
using AutoMapper;
using Shared.Dtos.Analyzation;

namespace AuthenticationApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AnalyzedData, AnalyzedDataReponse>();
        }
    }
}