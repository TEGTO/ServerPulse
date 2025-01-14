using Authentication.Models;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.ConfirmEmail;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.RefreshToken;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;
using AuthenticationApi.Core.Dtos.Endpoints.Auth.UserUpdate;
using AuthenticationApi.Core.Dtos.Endpoints.OAuth.LoginOAuth;
using AuthenticationApi.Core.Entities;
using AuthenticationApi.Core.Models;
using AutoMapper;

namespace AuthenticationApi.Application
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email)) // Map Email to UserName
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));   // Map Email to Email
            CreateMap<User, RegisterRequest>();

            CreateMap<UserUpdateRequest, UserUpdateModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            CreateMap<AccessTokenData, RefreshTokenResponse>();
            CreateMap<RefreshTokenRequest, AccessTokenData>();

            CreateMap<AccessTokenData, ConfirmEmailAccessTokenData>();

            CreateMap<AccessTokenData, LoginAccessTokenData>();

            CreateMap<AccessTokenData, LoginOAuthAccessTokenData>();
        }
    }
}