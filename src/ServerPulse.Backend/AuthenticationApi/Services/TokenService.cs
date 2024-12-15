using Authentication.Models;
using Authentication.Token;
using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthenticationApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly ITokenHandler tokenHandler;
        private readonly UserManager<User> userManager;
        private readonly double refreshTokenExpiryDays;

        public TokenService(ITokenHandler tokenHandler, UserManager<User> userManager, IConfiguration configuration)
        {
            this.tokenHandler = tokenHandler;
            this.userManager = userManager;
            refreshTokenExpiryDays = double.Parse(configuration[Configuration.AUTH_REFRESH_TOKEN_EXPIRY_IN_DAYS]!);
        }

        public async Task<AccessTokenData> GenerateTokenAsync(User user, CancellationToken cancellationToken)
        {
            var tokenData = tokenHandler.CreateToken(user);
            tokenData.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

            if (string.IsNullOrEmpty(user.RefreshToken) || user.RefreshTokenExpiryDate < DateTime.UtcNow)
            {
                user.RefreshToken = tokenData.RefreshToken;
                user.RefreshTokenExpiryDate = tokenData.RefreshTokenExpiryDate;

                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to update user with new refresh token.");
                }
            }
            else
            {
                tokenData.RefreshToken = user.RefreshToken;
                tokenData.RefreshTokenExpiryDate = user.RefreshTokenExpiryDate;
            }

            return tokenData;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            return tokenHandler.GetPrincipalFromExpiredToken(token);
        }

        public async Task<AccessTokenData> RefreshAccessTokenAsync(
             AccessTokenData tokenData,
             User user,
             CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(user.RefreshToken) ||
                user.RefreshToken != tokenData.RefreshToken ||
                user.RefreshTokenExpiryDate < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            return await GenerateTokenAsync(user, cancellationToken);
        }
    }
}
