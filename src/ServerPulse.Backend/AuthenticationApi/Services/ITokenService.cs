﻿using Authentication.Models;
using System.Security.Claims;
using AuthenticationApi.Infrastructure;

namespace AuthenticationApi.Services
{
    public interface ITokenService
    {
        public Task<AccessTokenData> CreateNewTokenDataAsync(User user, DateTime refreshTokenExpiryDate, CancellationToken cancellationToken);
        public Task SetRefreshTokenAsync(User user, AccessTokenData accessTokenData, CancellationToken cancellationToken);
        public ClaimsPrincipal GetPrincipalFromToken(string token);
    }
}