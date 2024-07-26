using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Authentication.Services
{
    public interface ITokenHandler
    {
        public AccessTokenData CreateToken(IdentityUser user);
        public JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims);
        public List<Claim> GetClaims(IdentityUser user);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        public SigningCredentials GetSigningCredentials();
        public AccessTokenData RefreshToken(IdentityUser user, AccessTokenData token);
    }
}