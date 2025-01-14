using Authentication.Models;
using AuthenticationApi.Core.Entities;
using System.Security.Claims;

namespace AuthenticationApi.Application.Services
{
    public interface ITokenService
    {
        public Task<AccessTokenData> GenerateTokenAsync(User user, CancellationToken cancellationToken);
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        public Task<AccessTokenData> RefreshAccessTokenAsync(AccessTokenData tokenData, User user, CancellationToken cancellationToken);
    }
}