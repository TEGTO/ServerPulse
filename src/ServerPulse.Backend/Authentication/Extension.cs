using Authentication.OAuth.GitHub;
using Authentication.OAuth.Google;
using Authentication.Rsa;
using Authentication.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Authentication
{
    public static class Extension
    {
        public static IServiceCollection AddOAuthServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GoogleOAuthSettings>(configuration.GetSection(GoogleOAuthSettings.SETTINGS_SECTION));
            services.Configure<GitHubOAuthSettings>(configuration.GetSection(GitHubOAuthSettings.SETTINGS_SECTION));

            services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
            services.AddScoped<IGoogleOAuthClient, GoogleOAuthClient>();

            services.AddScoped<IGitHubOAuthClient, GitHubOAuthClient>();

            return services;
        }

        public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(JwtSettings.SETTINGS_SECTION).Get<JwtSettings>();

            ArgumentNullException.ThrowIfNull(jwtSettings);

            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SETTINGS_SECTION));

            services.AddSingleton<IRsaKeyManager, RsaKeyManager>();
            services.AddSingleton<ITokenHandler, JwtHandler>();

            services.AddAuthorization();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer();

            services.ConfigureOptions<ConfigureJwtBearerOptions>();

            return services;
        }

        public static IApplicationBuilder UseIdentity(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}