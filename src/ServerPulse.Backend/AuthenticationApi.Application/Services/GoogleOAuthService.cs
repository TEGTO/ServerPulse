using Authentication.OAuth.Google;
using AuthenticationApi.Core.Enums;
using AuthenticationApi.Core.Models;
using static Google.Apis.Auth.GoogleJsonWebSignature;

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

        public async Task<ProviderLoginModel> GetProviderModelOnCodeAsync(string code, string redirectUrl, CancellationToken cancellationToken)
        {
            var codeVerifier = await stringVerifier.GetStringVerifierAsync(cancellationToken);

            var tokenResult = await oauthClient.ExchangeAuthorizationCodeAsync(
                code, codeVerifier, redirectUrl, cancellationToken);

            var payload = new Payload();

            payload = await tokenValidator.ValidateAsync(tokenResult?.IdToken ?? "");

            return new ProviderLoginModel
            {
                Email = payload.Email,
                ProviderLogin = nameof(OAuthLoginProvider.Google),
                ProviderKey = payload.Subject
            };
        }
    }
}
