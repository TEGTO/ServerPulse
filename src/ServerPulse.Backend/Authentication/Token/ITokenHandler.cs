using Authentication.Models;
using System.Security.Claims;

namespace Authentication.Token
{
    public interface ITokenHandler
    {
        public AccessTokenData CreateToken(IEnumerable<Claim> claims);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}