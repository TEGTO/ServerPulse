using Helper.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Authentication.OAuth.GitHub
{
    public sealed class GitHubOAuthClient : IGitHubOAuthClient
    {
        private readonly IGitHubOAuthApi gitHubOAuthApi;
        private readonly GitHubOAuthSettings oAuthSettings;

        public GitHubOAuthClient(IOptions<GitHubOAuthSettings> options, IGitHubOAuthApi gitHubOAuthApi)
        {
            oAuthSettings = options.Value;
            this.gitHubOAuthApi = gitHubOAuthApi;
        }

        public string GenerateOAuthRequestUrl(string redirectUrl, string stateVerifier, string? scope = null)
        {
            if (string.IsNullOrEmpty(scope))
            {
                scope = oAuthSettings.Scope;
            }

            var state = HashHelper.ComputeHash(stateVerifier);

            var queryParams = new Dictionary<string, string?>
            {
                {"client_id", oAuthSettings.ClientId },
                {"redirect_uri", redirectUrl },
                {"scope", scope },
                {"state", state },
            };

            var url = QueryHelpers.AddQueryString(oAuthSettings.GitHubOAuthApiUrl + "/authorize", queryParams);
            return url;
        }

        public async Task<GitHubOAuthTokenResult?> ExchangeAuthorizationCodeAsync(
            string code, string redirectUrl, CancellationToken cancellationToken)
        {
            var authParams = new Dictionary<string, string>
            {
                { "client_id", oAuthSettings.ClientId },
                { "client_secret", oAuthSettings.ClientSecret },
                { "code", code },
                { "redirect_uri", redirectUrl },
            };

            return await gitHubOAuthApi.ExchangeAuthorizationCodeAsync(authParams, cancellationToken);
        }
    }
}
