using Authentication.OAuth.GitHub;
using AuthenticationApi.Core.Enums;
using AuthenticationApi.Core.Models;
using Microsoft.Extensions.Options;

namespace AuthenticationApi.Application.Services
{
    public class GitHubOAuthService : IOAuthService
    {
        private readonly IGitHubOAuthClient oauthClient;
        private readonly IGitHubApiClient apiClient;
        private readonly IStringVerifierService stringVerifier;
        private readonly GitHubOAuthSettings oAuthSettings;

        public GitHubOAuthService(
            IGitHubOAuthClient oauthClient,
            IGitHubApiClient apiClient,
            IStringVerifierService stringVerifier,
            IOptions<GitHubOAuthSettings> options)
        {
            this.oauthClient = oauthClient;
            this.apiClient = apiClient;
            this.stringVerifier = stringVerifier;
            oAuthSettings = options.Value;
        }

        public async Task<string> GenerateOAuthRequestUrlAsync(string redirectUrl, CancellationToken cancellationToken)
        {
            var stateVerifier = await stringVerifier.GetStringVerifierAsync(cancellationToken);
            return oauthClient.GenerateOAuthRequestUrl(oAuthSettings.Scope, redirectUrl, stateVerifier);
        }

        public async Task<ProviderLoginModel> GetProviderModelOnCodeAsync(string code, string redirectUrl, CancellationToken cancellationToken)
        {
            var tokenResult = await oauthClient.ExchangeAuthorizationCodeAsync(
                code, redirectUrl, cancellationToken);

            if (tokenResult == null || string.IsNullOrEmpty(tokenResult.AccessToken))
            {
                throw new InvalidOperationException("Can't get the user aceess token!");
            }

            var result = await apiClient.GetUserInfoAsync(tokenResult.AccessToken, cancellationToken);

            if (result == null)
            {
                throw new InvalidOperationException("Can't get the user!");
            }

            return new ProviderLoginModel
            {
                Email = result.Email,
                ProviderLogin = nameof(OAuthLoginProvider.Google),
                ProviderKey = result.Id.ToString()
            };
        }
    }
}
