using AutoMapper;
using ServerSlotApi.Dtos.Endpoints.ServerSlot.CreateSlot;
using ServerSlotApi.Dtos.Endpoints.ServerSlot.GetSlotsByEmail;
using ServerSlotApi.Dtos.Endpoints.Slot.GetSlotById;
using ServerSlotApi.Dtos.Endpoints.Slot.UpdateSlot;
using ServerSlotApi.Infrastructure.Entities;

namespace ServerSlotApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreateSlotRequest, ServerSlot>();
            CreateMap<ServerSlot, CreateSlotResponse>();

            CreateMap<ServerSlot, GetSlotsByEmailResponse>();

            CreateMap<ServerSlot, GetSlotByIdResponse>();

            CreateMap<UpdateSlotRequest, ServerSlot>();
        }
    }
}