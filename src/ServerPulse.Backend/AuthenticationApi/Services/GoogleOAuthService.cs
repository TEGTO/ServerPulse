﻿using Authentication.OAuth.Google;
using AuthenticationApi.Dtos.OAuth;
using AuthenticationApi.Infrastructure.Models;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace AuthenticationApi.Services
{
    public class GoogleOAuthService : IOAuthService
    {
        private readonly IGoogleOAuthHttpClient httpClient;
        private readonly IGoogleTokenValidator googleTokenValidator;
        private readonly GoogleOAuthSettings oAuthSettings;

        public GoogleOAuthService(
            IGoogleOAuthHttpClient httpClient,
            IGoogleTokenValidator googleTokenValidator,
            GoogleOAuthSettings oAuthSettings)
        {
            this.httpClient = httpClient;
            this.googleTokenValidator = googleTokenValidator;
            this.oAuthSettings = oAuthSettings;
        }

        public async Task<ProviderLoginModel> GetProviderModelOnCodeAsync(OAuthAccessCodeParams requestParams, CancellationToken cancellationToken)
        {
            var tokenResult = await httpClient.ExchangeAuthorizationCodeAsync
                (requestParams.Code, requestParams.CodeVerifier, requestParams.RedirectUrl, cancellationToken);

            Payload payload = new();

            payload = await googleTokenValidator.ValidateAsync(tokenResult?.IdToken ?? "", new ValidationSettings
            {
                Audience = [oAuthSettings.ClientId],
            });

            return new ProviderLoginModel
            {
                Email = payload.Email,
                ProviderLogin = nameof(OAuthLoginProvider.Google),
                ProviderKey = payload.Subject
            };
        }

        public string GenerateOAuthRequestUrl(OAuthRequestUrlParams requestParams)
        {
            return httpClient.GenerateOAuthRequestUrl(oAuthSettings.Scope, requestParams.RedirectUrl, requestParams.CodeVerifier);
        }
    }
}
