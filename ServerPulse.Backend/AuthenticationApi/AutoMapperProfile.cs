using Authentication.Models;
using AutoMapper;
using AuthenticationApi.Domain.Dtos;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Domain.Models;

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
        }
    }
}