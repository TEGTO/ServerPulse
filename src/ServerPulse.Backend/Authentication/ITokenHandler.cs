using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Authentication.Services
{
    public interface ITokenHandler
    {
        public AccessTokenData CreateToken(IdentityUser user);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}