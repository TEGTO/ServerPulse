using Authentication.Models;
using AuthenticationApi.Infrastructure;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.ConfirmEmail;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Login;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.RefreshToken;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Register;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.UserUpdate;
using AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.LoginOAuth;
using AuthenticationApi.Infrastructure.Models;
using AutoMapper;

namespace AuthenticationApi
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