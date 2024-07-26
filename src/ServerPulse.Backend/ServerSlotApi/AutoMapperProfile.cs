using AutoMapper;
using ServerSlotApi.Domain.Dtos;
using ServerSlotApi.Domain.Entities;
using ServerSlotApi.Dtos;

namespace ServerSlotApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ServerSlot, ServerSlotResponse>();
            CreateMap<CreateServerSlotRequest, ServerSlot>();
            CreateMap<UpdateServerSlotRequest, ServerSlot>();
        }
    }
}