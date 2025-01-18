using Authentication.OAuth.Google;
using AuthenticationApi.Core.Enums;
using AuthenticationApi.Core.Models;
using Microsoft.Extensions.Options;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace AuthenticationApi.Application.Services
{
    public class GoogleOAuthService : IOAuthService
    {
        private readonly IGoogleOAuthClient httpClient;
        private readonly IGoogleTokenValidator googleTokenValidator;
        private readonly IStringVerifierService codeVerifierService;
        private readonly GoogleOAuthSettings oAuthSettings;

        public GoogleOAuthService(
            IGoogleOAuthClient httpClient,
            IGoogleTokenValidator googleTokenValidator,
            IStringVerifierService codeVerifierService,
            IOptions<GoogleOAuthSettings> options)
        {
            this.httpClient = httpClient;
            this.googleTokenValidator = googleTokenValidator;
            this.codeVerifierService = codeVerifierService;
            oAuthSettings = options.Value;
        }

        public async Task<ProviderLoginModel> GetProviderModelOnCodeAsync(string code, string redirectUrl, CancellationToken cancellationToken)
        {
            var codeVerifier = await codeVerifierService.GetStringVerifierAsync(cancellationToken);

            var tokenResult = await httpClient.ExchangeAuthorizationCodeAsync(
                code, codeVerifier, redirectUrl, cancellationToken);

            var payload = new Payload();

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

        public async Task<string> GenerateOAuthRequestUrlAsync(string redirectUrl, CancellationToken cancellationToken)
        {
            var codeVerifier = await codeVerifierService.GetStringVerifierAsync(cancellationToken);
            return httpClient.GenerateOAuthRequestUrl(oAuthSettings.Scope, redirectUrl, codeVerifier);
        }
    }
}
