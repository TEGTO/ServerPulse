﻿using Authentication.Models;
using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthenticationApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> userManager;
        private readonly ITokenService tokenService;

        public AuthService(UserManager<User> userManager, ITokenService tokenService)
        {
            this.userManager = userManager;
            this.tokenService = tokenService;
        }

        #region IAuthService Members

        public async Task<IdentityResult> RegisterUserAsync(RegisterUserModel model, CancellationToken cancellationToken)
        {
            return await userManager.CreateAsync(model.User, model.Password);
        }

        public async Task<AccessTokenData> LoginUserAsync(LoginUserModel model, CancellationToken cancellationToken)
        {
            var user = await GetUserByLoginAsync(model.Login)
                ?? throw new UnauthorizedAccessException("Invalid login or password.");

            if (!await userManager.CheckPasswordAsync(user, model.Password))
            {
                throw new UnauthorizedAccessException("Invalid authentication. Check Login or password.");
            }

            return await tokenService.GenerateTokenAsync(user, cancellationToken);
        }

        public async Task<AccessTokenData> RefreshTokenAsync(AccessTokenData accessData, CancellationToken cancellationToken)
        {
            var principal = tokenService.GetPrincipalFromExpiredToken(accessData.AccessToken);

            var user = await GetUserFromPrincipalAsync(principal)
                ?? throw new UnauthorizedAccessException("User not found! Invalid user identity!");

            return await tokenService.RefreshAccessTokenAsync(accessData, user, cancellationToken);
        }

        public async Task<IEnumerable<IdentityError>> UpdateUserAsync(ClaimsPrincipal principal, UserUpdateModel updateModel, bool resetPassword, CancellationToken cancellationToken)
        {
            var user = await GetUserFromPrincipalAsync(principal)
                ?? throw new UnauthorizedAccessException("User not found!");

            var errors = new List<IdentityError>();

            if (!string.IsNullOrEmpty(updateModel.UserName))
            {
                errors.AddRange(await UpdateUserNameAsync(user, updateModel.UserName));
            }

            if (!string.IsNullOrEmpty(updateModel.Email))
            {
                errors.AddRange(await UpdateEmailAsync(user, updateModel.Email));
            }

            if (!string.IsNullOrEmpty(updateModel.Password))
            {
                errors.AddRange(await UpdatePasswordAsync(user, updateModel.OldPassword, updateModel.Password, resetPassword));
            }

            return errors.DistinctBy(e => e.Description).ToList();
        }

        #endregion

        #region Private Helpers

        private async Task<User?> GetUserFromPrincipalAsync(ClaimsPrincipal principal)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrEmpty(userId) ? await GetUserByLoginAsync(userId) : null;
        }

        private async Task<User?> GetUserByLoginAsync(string login)
        {
            return await userManager.Users.SingleOrDefaultAsync(u =>
                u.Email == login || u.UserName == login || u.Id == login);
        }

        private async Task<IEnumerable<IdentityError>> UpdateUserNameAsync(User user, string newUserName)
        {
            if (user.UserName == newUserName) return Enumerable.Empty<IdentityError>();

            var result = await userManager.SetUserNameAsync(user, newUserName);
            return result.Errors;
        }

        private async Task<IEnumerable<IdentityError>> UpdateEmailAsync(User user, string newEmail)
        {
            if (user.Email == newEmail) return Enumerable.Empty<IdentityError>();

            var token = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            var result = await userManager.ChangeEmailAsync(user, newEmail, token);
            return result.Errors;
        }

        private async Task<IEnumerable<IdentityError>> UpdatePasswordAsync(User user, string? oldPassword, string newPassword, bool resetPassword)
        {
            if (resetPassword)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, newPassword);
                return result.Errors;
            }

            if (!string.IsNullOrEmpty(oldPassword))
            {
                var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
                return result.Errors;
            }

            return new List<IdentityError>
            {
                new IdentityError { Description = "Password update failed due to missing old password." }
            };
        }

        #endregion
    }
}