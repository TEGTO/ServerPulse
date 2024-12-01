using Authentication.Models;
using AutoMapper;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Dtos;

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

            CreateMap<UserUpdateDataRequest, UserUpdateModel>();
        }
    }
}