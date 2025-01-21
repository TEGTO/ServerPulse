using Authentication.OAuth.GitHub;
using Authentication.OAuth.Google;
using AuthenticationApi.Application;
using AuthenticationApi.Application.Services;
using AuthenticationApi.Core.Enums;
using AuthenticationApi.Infrastructure.Services;
using Helper.DelegatingHandlers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Refit;
using System.Net.Http.Headers;

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

                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }).AddStandardResilienceHandler();

            builder.Services.AddTransient<AuthenticatedHttpClientHandler>();
            builder.Services.AddRefitClient<IGitHubApi>()
                .ConfigureHttpClient((sp, httpClient) =>
                {
                    var settings = sp.GetRequiredService<IOptions<GitHubOAuthSettings>>().Value;
                    httpClient.BaseAddress = new Uri(settings.GitHubApiUrl);

                    httpClient.DefaultRequestHeaders.Add("User-Agent", settings.AppName);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                })
                .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
                .AddStandardResilienceHandler();

            builder.Services.AddRefitClient<IGoogleOAuthApi>()
                .ConfigureHttpClient((sp, httpClient) =>
                {
                    var settings = sp.GetRequiredService<IOptions<GoogleOAuthSettings>>().Value;
                    httpClient.BaseAddress = new Uri(settings.GoogleOAuthUrl);

                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }).AddStandardResilienceHandler();

            return builder;
        }
    }
}
