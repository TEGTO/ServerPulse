﻿using Authentication.OAuth.Google;
using AuthenticationApi.Core.Enums;
using AuthenticationApi.Core.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthenticationApi.Application.Services
{
    public class GoogleOAuthService : IOAuthService
    {
        private readonly IGoogleOAuthClient oauthClient;
        private readonly IGoogleTokenValidator tokenValidator;
        private readonly IStringVerifierService stringVerifier;

        public GoogleOAuthService(
            IGoogleOAuthClient oauthClient,
            IGoogleTokenValidator tokenValidator,
            IStringVerifierService stringVerifier)
        {
            this.oauthClient = oauthClient;
            this.tokenValidator = tokenValidator;
            this.stringVerifier = stringVerifier;
        }

        public async Task<string> GenerateOAuthRequestUrlAsync(string redirectUrl, CancellationToken cancellationToken)
        {
            var codeVerifier = await stringVerifier.GetStringVerifierAsync(cancellationToken);
            return oauthClient.GenerateOAuthRequestUrl(redirectUrl, codeVerifier);
        }

        public async Task<ProviderLoginModel> GetProviderModelOnCodeAsync(string queryParams, string redirectUrl, CancellationToken cancellationToken)
        {
            var codeVerifier = await stringVerifier.GetStringVerifierAsync(cancellationToken);
            var code = QueryHelpers.ParseQuery(queryParams)["code"].FirstOrDefault();

            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("Code is null or empty.");
            }

            var tokenResult = await oauthClient.ExchangeAuthorizationCodeAsync(
                code, codeVerifier, redirectUrl, cancellationToken);

            var payload = await tokenValidator.ValidateAsync(tokenResult?.IdToken ?? "");

            return new ProviderLoginModel
            {
                Email = payload.Email,
                ProviderLogin = nameof(OAuthLoginProvider.Google),
                ProviderKey = payload.Subject
            };
        }
    }
}
