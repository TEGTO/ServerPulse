using Authentication.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Authentication
{
    public static class CustomAuthExtension
    {
        public static IServiceCollection ConfigureIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings()
            {
                Key = configuration[JwtConfiguration.JWT_SETTINGS_KEY]!,
                Audience = configuration[JwtConfiguration.JWT_SETTINGS_AUDIENCE]!,
                Issuer = configuration[JwtConfiguration.JWT_SETTINGS_ISSUER]!,
                ExpiryInMinutes = Convert.ToDouble(configuration[JwtConfiguration.JWT_SETTINGS_EXPIRY_IN_MINUTES]!),
            };

            services.AddSingleton(jwtSettings);

            services.AddCustomAuthentication(jwtSettings);

            return services;
        }

        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, JwtSettings jwtSettings)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });
            return services;
        }

        public static IApplicationBuilder UseIdentity(this IApplicationBuilder app)
        {
            app.UseAuthentication();
            return app;
        }
    }
}