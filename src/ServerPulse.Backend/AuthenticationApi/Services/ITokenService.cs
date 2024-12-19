using Authentication.Models;
using AuthenticationApi.Infrastructure;
using System.Security.Claims;

namespace AuthenticationApi.Services
{
    public interface ITokenService
    {
        public Task<AccessTokenData> GenerateTokenAsync(User user, CancellationToken cancellationToken);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        public Task<AccessTokenData> RefreshAccessTokenAsync(AccessTokenData tokenData, User user, CancellationToken cancellationToken);
    }
}