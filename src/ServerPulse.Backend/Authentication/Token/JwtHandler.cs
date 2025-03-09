using Authentication.Models;
using Authentication.Rsa;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Authentication.Token
{
    internal sealed class JwtHandler : ITokenHandler
    {
        private readonly JwtSettings jwtSettings;
        private readonly IRsaKeyManager rsaKeyManager;

        public JwtHandler(IOptions<JwtSettings> options, IRsaKeyManager rsaKeyManager)
        {
            jwtSettings = options.Value;
            this.rsaKeyManager = rsaKeyManager;
        }

        #region ITokenHandler Members

        public AccessTokenData CreateToken(IEnumerable<Claim> claims)
        {
            var signingCredentials = GetSigningCredentials();

            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            var token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
            var refreshToken = GenerateRefreshToken();

            return new AccessTokenData { AccessToken = token, RefreshToken = refreshToken };
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = rsaKeyManager.PublicKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.RsaSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid Token");
            }

            return principal;
        }

        #endregion

        #region Private Helpers

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var tokenOptions = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.ExpiryInMinutes)),
                signingCredentials: signingCredentials);

            return tokenOptions;
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private SigningCredentials GetSigningCredentials()
        {
            return new SigningCredentials(rsaKeyManager.PrivateKey, SecurityAlgorithms.RsaSha256);
        }

        #endregion
    }
}