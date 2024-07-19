using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Authentication.Services
{
    public interface ITokenHandler
    {
        AccessTokenData CreateToken(IdentityUser user);
        JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims);
        List<Claim> GetClaims(IdentityUser user);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        SigningCredentials GetSigningCredentials();
        AccessTokenData RefreshToken(IdentityUser user, AccessTokenData token);
    }
}