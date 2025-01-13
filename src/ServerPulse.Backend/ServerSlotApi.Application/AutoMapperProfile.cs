using AutoMapper;
using ServerSlotApi.Core.Dtos.Endpoints.ServerSlot.CreateSlot;
using ServerSlotApi.Core.Dtos.Endpoints.ServerSlot.GetSlotsByEmail;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.GetSlotById;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.UpdateSlot;
using ServerSlotApi.Core.Entities;

namespace ServerSlotApi.Application
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