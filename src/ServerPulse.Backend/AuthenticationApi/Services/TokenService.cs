using Authentication.Models;
using Authentication.Token;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AuthenticationApi.Infrastructure;

namespace AuthenticationApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenHandler tokenHandler;
        private readonly UserManager<User> userManager;

        public TokenService(ITokenHandler tokenHandler, UserManager<User> userManager)
        {
            this.tokenHandler = tokenHandler;
            this.userManager = userManager;
        }

        public Task<AccessTokenData> CreateNewTokenDataAsync(User user, DateTime refreshTokenExpiryDate, CancellationToken cancellationToken)
        {
            var tokenData = tokenHandler.CreateToken(user);
            tokenData.RefreshTokenExpiryDate = refreshTokenExpiryDate;
            return Task.FromResult(tokenData);
        }
        public async Task SetRefreshTokenAsync(User user, AccessTokenData accessTokenData, CancellationToken cancellationToken)
        {
            user.RefreshToken = accessTokenData.RefreshToken;
            user.RefreshTokenExpiryTime = accessTokenData.RefreshTokenExpiryDate;
            await userManager.UpdateAsync(user);
        }
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            return tokenHandler.GetPrincipalFromExpiredToken(token);
        }
    }
}
