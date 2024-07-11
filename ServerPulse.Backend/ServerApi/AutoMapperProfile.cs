using AutoMapper;
using ServerApi.Domain.Dtos;
using ServerApi.Domain.Entities;
using Shared.Dtos;

namespace ServerApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ServerSlot, ServerSlotDto>();
            CreateMap<CreateServerSlotRequest, ServerSlot>();
        }
    }
}