﻿using Authentication.Models;
using AuthenticationApi.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthenticationApi.Services
{
    public interface IAuthService
    {
        public Task<IdentityResult> RegisterUserAsync(RegisterUserModel model, CancellationToken cancellationToken);
        public Task<AccessTokenData> LoginUserAsync(LoginUserModel model, CancellationToken cancellationToken);
        public Task<AccessTokenData> RefreshTokenAsync(AccessTokenData accessData, CancellationToken cancellationToken);
        public Task<IEnumerable<IdentityError>> UpdateUserAsync(ClaimsPrincipal principal, UserUpdateModel updateModel, bool resetPassword, CancellationToken cancellationToken);
        public Task<AccessTokenData> LoginUserWithProviderAsync(ProviderLoginModel model, CancellationToken cancellationToken);
        public Task<string> GetEmailConfirmationTokenAsync(string email);
        public Task<IdentityResult> ConfirmEmailAsync(string email, string token);
        public Task<AccessTokenData> LoginUserAfterConfirmationAsync(string email, CancellationToken cancellationToken);
        public Task<bool> CheckEmailConfirmationAsync(string email);
    }
}