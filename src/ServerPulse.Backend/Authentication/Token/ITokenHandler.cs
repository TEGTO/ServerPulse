using Authentication.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Authentication.Token
{
    public interface ITokenHandler
    {
        public AccessTokenData CreateToken<TKey>(IdentityUser<TKey> user) where TKey : IEquatable<TKey>;
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}