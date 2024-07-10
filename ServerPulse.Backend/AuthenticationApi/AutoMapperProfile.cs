using Authentication.Models;
using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Domain.Models;
using AutoMapper;
using Shared.Dtos;

namespace AuthenticationApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserRegistrationRequest>();
            CreateMap<UserRegistrationRequest, User>();
            CreateMap<AccessTokenData, AuthToken>();
            CreateMap<AuthToken, AccessTokenData>();
            CreateMap<UserUpdateDataRequest, UserUpdateData>();
            CreateMap<ServerSlot, ServerSlotDto>();
            CreateMap<CreateServerSlotRequest, ServerSlot>();
        }
    }
}