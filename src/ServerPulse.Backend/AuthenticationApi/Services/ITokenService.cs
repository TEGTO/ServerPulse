using Authentication.Models;
using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthenticationApi.Services
{
    public interface ITokenService
    {
        public Task<AccessTokenData> CreateNewTokenDataAsync(User user, DateTime refreshTokenExpiryDate, CancellationToken cancellationToken);
        public Task<IdentityResult> SetRefreshTokenAsync(User user, AccessTokenData accessTokenData, CancellationToken cancellationToken);
        public ClaimsPrincipal GetPrincipalFromToken(string token);
    }
}