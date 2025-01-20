using Authentication.OAuth.GitHub;
using AuthenticationApi.Core.Enums;
using AuthenticationApi.Core.Models;

namespace AuthenticationApi.Application.Services
{
    public class GitHubOAuthService : IOAuthService
    {
        private readonly IGitHubOAuthClient oauthClient;
        private readonly IGitHubApi gitHubApi;
        private readonly IStringVerifierService stringVerifier;

        public GitHubOAuthService(
            IGitHubOAuthClient oauthClient,
            IGitHubApi gitHubApi,
            IStringVerifierService stringVerifier)
        {
            this.oauthClient = oauthClient;
            this.gitHubApi = gitHubApi;
            this.stringVerifier = stringVerifier;
        }

        public async Task<string> GenerateOAuthRequestUrlAsync(string redirectUrl, CancellationToken cancellationToken)
        {
            var stateVerifier = await stringVerifier.GetStringVerifierAsync(cancellationToken);
            return oauthClient.GenerateOAuthRequestUrl(redirectUrl, stateVerifier);
        }

        public async Task<ProviderLoginModel> GetProviderModelOnCodeAsync(string code, string redirectUrl, CancellationToken cancellationToken)
        {
            var tokenResult = await oauthClient.ExchangeAuthorizationCodeAsync(
                code, redirectUrl, cancellationToken);

            if (tokenResult == null || string.IsNullOrEmpty(tokenResult.AccessToken))
            {
                throw new InvalidOperationException("Can't get the user aceess token!");
            }

            var result = await gitHubApi.GetUserInfoAsync(tokenResult.AccessToken, cancellationToken);

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
