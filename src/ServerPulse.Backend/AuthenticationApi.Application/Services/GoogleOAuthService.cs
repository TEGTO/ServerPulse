using Authentication.OAuth.Google;
using AuthenticationApi.Core.Enums;
using AuthenticationApi.Core.Models;
using Microsoft.Extensions.Options;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace AuthenticationApi.Application.Services
{
    public class GoogleOAuthService : IOAuthService
    {
        private readonly IGoogleOAuthClient oauthClient;
        private readonly IGoogleTokenValidator tokenValidator;
        private readonly IStringVerifierService stringVerifier;
        private readonly GoogleOAuthSettings oAuthSettings;

        public GoogleOAuthService(
            IGoogleOAuthClient oauthClient,
            IGoogleTokenValidator tokenValidator,
            IStringVerifierService stringVerifier,
            IOptions<GoogleOAuthSettings> options)
        {
            this.oauthClient = oauthClient;
            this.tokenValidator = tokenValidator;
            this.stringVerifier = stringVerifier;
            oAuthSettings = options.Value;
        }

        public async Task<string> GenerateOAuthRequestUrlAsync(string redirectUrl, CancellationToken cancellationToken)
        {
            var codeVerifier = await stringVerifier.GetStringVerifierAsync(cancellationToken);
            return oauthClient.GenerateOAuthRequestUrl(oAuthSettings.Scope, redirectUrl, codeVerifier);
        }

        public async Task<ProviderLoginModel> GetProviderModelOnCodeAsync(string code, string redirectUrl, CancellationToken cancellationToken)
        {
            var codeVerifier = await stringVerifier.GetStringVerifierAsync(cancellationToken);

            var tokenResult = await oauthClient.ExchangeAuthorizationCodeAsync(
                code, codeVerifier, redirectUrl, cancellationToken);

            var payload = new Payload();

            payload = await tokenValidator.ValidateAsync(tokenResult?.IdToken ?? "", new ValidationSettings
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
    }
}
