using Authentication.OAuth.Google;
using AuthenticationApi.Application;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Enums;

namespace AuthenticationApi
{
    public static class HostApplicationBuilderExtensions
    {
        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            var isOAuthEnabled = bool.Parse(builder.Configuration[$"FeatureManagement:{Features.OAUTH}"]! ?? "false");

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IEmailJobService, EmailJobService>();

            if (isOAuthEnabled)
            {
                builder.Services.AddScoped<GoogleOAuthService>();
                builder.Services.AddScoped<IGoogleOAuthHttpClient, GoogleOAuthHttpClient>();
                builder.Services.AddScoped(provider => new Dictionary<OAuthLoginProvider, IOAuthService>
                {
                    { OAuthLoginProvider.Google, provider.GetService<GoogleOAuthService>()! },
                });
            }
            else
            {
                builder.Services.AddScoped(provider => new Dictionary<OAuthLoginProvider, IOAuthService>());
            }

            return builder;
        }
    }
}
