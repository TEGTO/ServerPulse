using AutoMapper;
using ServerSlotApi.Domain.Dtos;
using ServerSlotApi.Domain.Entities;
using Shared.Dtos;

namespace ServerSlotApi
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