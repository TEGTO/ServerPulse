using Authentication.Rsa;
using Authentication.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Authentication
{
    internal sealed class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IRsaKeyManager rsaKeyManager;
        private readonly JwtSettings jwtSettings;

        public ConfigureJwtBearerOptions(IRsaKeyManager rsaKeyManager, IOptions<JwtSettings> options)
        {
            this.rsaKeyManager = rsaKeyManager;
            jwtSettings = options.Value;
        }

        public void Configure(JwtBearerOptions options)
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = rsaKeyManager.PublicKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true
            };
        }

        public void Configure(string? name, JwtBearerOptions options)
        {
            Configure(options);
        }
    }
}
