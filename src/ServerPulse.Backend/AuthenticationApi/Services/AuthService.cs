﻿using Authentication.Models;
using AuthenticationApi.Infrastructure;
using ExceptionHandling;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> userManager;
        private readonly ITokenService tokenService;
        private readonly double expiryInDays;

        public AuthService(UserManager<User> userManager, ITokenService tokenService, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
            expiryInDays = double.Parse(configuration[Configuration.AUTH_REFRESH_TOKEN_EXPIRY_IN_DAYS]!);
        }

        #region IAuthService Members

        public async Task<IdentityResult> RegisterUserAsync(RegisterUserModel registerModel, CancellationToken cancellationToken)
        {
            return await userManager.CreateAsync(registerModel.User, registerModel.Password);
        }

        public async Task<AccessTokenData> LoginUserAsync(LoginUserModel loginModel, CancellationToken cancellationToken)
        {
            var user = loginModel.User;

            if (!await userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                throw new UnauthorizedAccessException("Invalid authentication. Check Login or password.");
            }

            var refreshTokenExpiryDate = DateTime.UtcNow.AddDays(expiryInDays);

            var tokenData = await tokenService.CreateNewTokenDataAsync(user, refreshTokenExpiryDate, cancellationToken);

            var identityErrors = await tokenService.SetRefreshTokenAsync(user, tokenData, cancellationToken);
            if (Utilities.HasErrors(identityErrors.Errors, out var errorResponse)) throw new AuthorizationException(errorResponse);

            return tokenData;
        }

        public async Task<AccessTokenData> RefreshTokenAsync(RefreshTokenModel refreshTokenModel, CancellationToken cancellationToken)
        {
            var user = refreshTokenModel.User;
            var accessTokenData = refreshTokenModel.AccessTokenData;

            if (accessTokenData.RefreshToken == null ||
                user.RefreshToken != accessTokenData.RefreshToken ||
                user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token is not valid!");
            }

            var refreshTokenExpiryDate = DateTime.UtcNow.AddDays(expiryInDays);

            var tokenData = await tokenService.CreateNewTokenDataAsync(user, refreshTokenExpiryDate, cancellationToken);

            var identityErrors = await tokenService.SetRefreshTokenAsync(user, tokenData, cancellationToken);
            if (Utilities.HasErrors(identityErrors.Errors, out var errorResponse)) throw new AuthorizationException(errorResponse);

            return tokenData;
        }

        #endregion
    }
}