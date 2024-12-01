using AutoMapper;
using ServerSlotApi.Dtos;
using ServerSlotApi.Infrastructure.Entities;

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