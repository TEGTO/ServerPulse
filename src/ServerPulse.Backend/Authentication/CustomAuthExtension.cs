using Authentication.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Authentication
{
    public static class CustomAuthExtension
    {
        public static IServiceCollection ConfigureIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings()
            {
                PrivateKey = configuration[JwtConfiguration.JWT_SETTINGS_PRIVATE_KEY]!,
                PublicKey = configuration[JwtConfiguration.JWT_SETTINGS_PUBLIC_KEY]!,
                Audience = configuration[JwtConfiguration.JWT_SETTINGS_AUDIENCE]!,
                Issuer = configuration[JwtConfiguration.JWT_SETTINGS_ISSUER]!,
                ExpiryInMinutes = Convert.ToDouble(configuration[JwtConfiguration.JWT_SETTINGS_EXPIRY_IN_MINUTES]!),
            };

            services.AddSingleton(jwtSettings);

            services.AddAuthorization();

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
                    IssuerSigningKey = jwtSettings.GetRsaPublicKeyFromSettings(),
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
            app.UseAuthorization();
            return app;
        }

        public static RsaSecurityKey? GetRsaPublicKeyFromSettings(this JwtSettings jwtSettings)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(jwtSettings.PublicKey.ToCharArray());

            return new RsaSecurityKey(rsa);
        }

        public static RsaSecurityKey? GetRsaPrivateKeyFromSettings(this JwtSettings jwtSettings)
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(jwtSettings.PrivateKey.ToCharArray());

            return new RsaSecurityKey(rsa);
        }
    }
}