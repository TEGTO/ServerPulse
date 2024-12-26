using Authentication.Models;
using Authentication.OAuth;
using Authentication.OAuth.Google;
using AuthenticationApi.Infrastructure;
using Microsoft.AspNetCore.Identity;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace AuthenticationApi.Services
{
    public class GoogleOAuthService : IOAuthService
    {
        private readonly IGoogleOAuthHttpClient httpClient;
        private readonly ITokenService tokenService;
        private readonly IGoogleTokenValidator googleTokenValidator;
        private readonly UserManager<User> userManager;
        private readonly GoogleOAuthSettings oAuthSettings;

        public GoogleOAuthService(
            IGoogleOAuthHttpClient httpClient,
            ITokenService tokenService,
            IGoogleTokenValidator googleTokenValidator,
            UserManager<User> userManager,
            GoogleOAuthSettings oAuthSettings)
        {
            this.httpClient = httpClient;
            this.tokenService = tokenService;
            this.googleTokenValidator = googleTokenValidator;
            this.userManager = userManager;
            this.oAuthSettings = oAuthSettings;
        }

        public async Task<AccessTokenData> GetAccessOnCodeAsync(GetAccessOnCodeParams accessOnCodeParams, CancellationToken cancellationToken)
        {
            var tokenResult = await httpClient.ExchangeAuthorizationCodeAsync
                (accessOnCodeParams.Code, accessOnCodeParams.CodeVerifier, accessOnCodeParams.RedirectUrl, cancellationToken);

            Payload payload = new();

            payload = await googleTokenValidator.ValidateAsync(tokenResult?.IdToken ?? "", new ValidationSettings
            {
                Audience = [oAuthSettings.ClientId]
            });

            var user = await CreateUserFromOAuthAsync(payload.Email, payload.Subject);

            if (user == null) throw new InvalidOperationException("Failed to register user via oauth!");

            var tokenData = await tokenService.GenerateTokenAsync(user, cancellationToken);

            return tokenData;
        }

        public string GenerateOAuthRequestUrl(GenerateOAuthRequestUrlParams generateUrlParams)
        {
            return httpClient.GenerateOAuthRequestUrl(oAuthSettings.Scope, generateUrlParams.RedirectUrl, generateUrlParams.CodeVerifier);
        }

        private async Task<User?> CreateUserFromOAuthAsync(string email, string subject)
        {
            var user = await userManager.FindByLoginAsync(nameof(OAuthLoginProvider.Google), subject);

            if (user != null)
                return user;

            user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user);
            }

            var userLoginInfo = new UserLoginInfo(nameof(OAuthLoginProvider.Google), subject, nameof(OAuthLoginProvider.Google).ToUpper());

            var result = await userManager.AddLoginAsync(user, userLoginInfo);

            return result.Succeeded ? user : null;
        }
    }
}
