using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Authentication.Token
{
    public class JwtHandler : ITokenHandler
    {
        private readonly JwtSettings jwtSettings;

        public JwtHandler(JwtSettings jwtSettings)
        {
            this.jwtSettings = jwtSettings;
        }

        #region ITokenHandler Members

        public AccessTokenData CreateToken<TKey>(IdentityUser<TKey> user) where TKey : IEquatable<TKey>
        {
            var signingCredentials = GetSigningCredentials();

            var claims = GetClaims(user);
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid Token");
            }

            return principal;
        }

        #endregion

        #region Private Helpers

        private static List<Claim> GetClaims<TKey>(IdentityUser<TKey> user) where TKey : IEquatable<TKey>
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? throw new ArgumentNullException(nameof(user), "Email could not be null!")),
                new Claim(ClaimTypes.Name, user.UserName ?? throw new ArgumentNullException(nameof(user), "UserName could not be null!")),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString() ?? throw new ArgumentNullException(nameof(user), "Id could not be null!"))
            };

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
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
            var key = Encoding.UTF8.GetBytes(jwtSettings.Key);
            var secretKey = new SymmetricSecurityKey(key);
            return new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        }

        #endregion
    }
}