using Authentication.Models;
using AuthenticationApi.Dtos;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Infrastructure.Models;
using AutoMapper;

namespace AuthenticationApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserRegistrationRequest>();
            CreateMap<UserRegistrationRequest, User>()
                 .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Map Email to UserName
                 .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));   // Map Email to Email

            CreateMap<AccessTokenData, AccessTokenDataDto>();
            CreateMap<AccessTokenDataDto, AccessTokenData>();

            CreateMap<UserUpdateDataRequest, UserUpdateModel>()
           .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }
    }
}