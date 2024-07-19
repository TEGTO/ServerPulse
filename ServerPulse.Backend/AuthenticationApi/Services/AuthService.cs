﻿using Authentication.Models;
using Authentication.Services;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Domain.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthenticationApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> userManager;
        private readonly ITokenHandler tokenHandler;

        public AuthService(UserManager<User> userManager, ITokenHandler tokenHandler)
        {
            this.userManager = userManager;
            this.tokenHandler = tokenHandler;
        }

        public async Task<IdentityResult> RegisterUserAsync(User user, string password)
        {
            return await userManager.CreateAsync(user, password);
        }
        public async Task<AccessTokenData> LoginUserAsync(string login, string password, int refreshTokeExpiryInDays)
        {
            var user = await GetUserByLoginAsync(login);
            if (user == null || !await userManager.CheckPasswordAsync(user, password))
            {
                throw new UnauthorizedAccessException("Invalid authentication. Login or email address is not correct.");
            }
            var token = tokenHandler.CreateToken(user);
            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokeExpiryInDays);
            await userManager.UpdateAsync(user);
            return token;
        }
        public async Task<User?> GetUserByLoginAsync(string login)
        {
            var user = await userManager.FindByEmailAsync(login);
            user = user == null ? await userManager.FindByNameAsync(login) : user;
            return user;
        }
        public async Task<List<IdentityError>> UpdateUserAsync(UserUpdateData updateData)
        {
            var user = await userManager.FindByEmailAsync(updateData.OldEmail);
            List<IdentityError> identityErrors = new List<IdentityError>();
            if (!string.IsNullOrEmpty(updateData.UserName))
            {
                var result = await userManager.SetUserNameAsync(user, updateData.UserName);
                identityErrors.AddRange(result.Errors);
            }
            if (!string.IsNullOrEmpty(updateData.NewEmail) && !updateData.NewEmail.Equals(updateData.OldEmail))
            {
                var token = await userManager.GenerateChangeEmailTokenAsync(user, updateData.NewEmail);
                var result = await userManager.ChangeEmailAsync(user, updateData.NewEmail, token);
                identityErrors.AddRange(result.Errors);
            }
            if (!string.IsNullOrEmpty(updateData.NewPassword))
            {
                var result = await userManager.ChangePasswordAsync(user, updateData.OldPassword, updateData.NewPassword);
                identityErrors.AddRange(result.Errors);
            }
            return RemoveDuplicates(identityErrors);
        }
        public async Task<AccessTokenData> RefreshTokenAsync(AccessTokenData accessTokenData)
        {
            var principal = tokenHandler.GetPrincipalFromExpiredToken(accessTokenData.AccessToken);
            var user = await userManager.FindByNameAsync(principal.Identity.Name);
            if (user == null || user.RefreshToken != accessTokenData.RefreshToken
                || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                throw new InvalidDataException("Refresh token is not valid!");
            }
            return tokenHandler.CreateToken(user);
        }
        public async Task<bool> CheckAuthDataAsync(string login, string password)
        {
            var user = await GetUserByLoginAsync(login);
            if (user == null || !await userManager.CheckPasswordAsync(user, password))
            {
                return false;
            }
            return true;
        }
        private List<IdentityError> RemoveDuplicates(List<IdentityError> identityErrors)
        {
            identityErrors = identityErrors
            .GroupBy(e => e.Description)
            .Select(g => g.First())
            .ToList();
            return identityErrors;
        }
    }
}