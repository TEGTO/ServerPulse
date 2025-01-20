using Authentication.OAuth.GitHub;
using Authentication.OAuth.Google;
using AuthenticationApi.Application;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Enums;
using AuthenticationApi.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Refit;

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
                builder.Services.AddScoped<GitHubOAuthService>();

                builder.Services.AddScoped(provider => new Dictionary<OAuthLoginProvider, IOAuthService>
                {
                    { OAuthLoginProvider.Google, provider.GetService<GoogleOAuthService>()! },
                    { OAuthLoginProvider.GitHub, provider.GetService<GitHubOAuthService>()! },
                });
            }
            else
            {
                builder.Services.AddScoped(provider => new Dictionary<OAuthLoginProvider, IOAuthService>());
            }

            return builder;
        }

        public static IHostApplicationBuilder AddInfrastructureServices(this IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<IStringVerifierService, StringVerifierService>();

            builder.Services.AddRefitClient<IGitHubOAuthApi>()
                .ConfigureHttpClient((sp, httpClient) =>
                {
                    var settings = sp.GetRequiredService<IOptions<GitHubOAuthSettings>>().Value;
                    httpClient.BaseAddress = new Uri(settings.GitHubOAuthApiUrl);
                }).AddStandardResilienceHandler();

            builder.Services.AddRefitClient<IGitHubApi>()
            .ConfigureHttpClient((sp, httpClient) =>
            {
                var settings = sp.GetRequiredService<IOptions<GitHubOAuthSettings>>().Value;
                httpClient.BaseAddress = new Uri(settings.GitHubApiUrl);
            }).AddStandardResilienceHandler();

            builder.Services.AddRefitClient<IGoogleOAuthApi>()
           .ConfigureHttpClient((sp, httpClient) =>
           {
               var settings = sp.GetRequiredService<IOptions<GoogleOAuthSettings>>().Value;
               httpClient.BaseAddress = new Uri(settings.GoogleOAuthUrl);
           }).AddStandardResilienceHandler();

            return builder;
        }
    }
}
