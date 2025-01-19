using Helper.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Authentication.OAuth.GitHub
{
    public sealed class GitHubOAuthClient : IGitHubOAuthClient
    {
        private const string TOKEN_SERVER_ENDPOINT = "https://github.com/login/oauth/access_token";
        private const string OAUTH_SERVER_ENDPOINT = "https://github.com/login/oauth/authorize";
        private readonly IHttpHelper httpHelperService;
        private readonly GitHubOAuthSettings oAuthSettings;

        public GitHubOAuthClient(IOptions<GitHubOAuthSettings> options, IHttpHelper httpHelperService)
        {
            oAuthSettings = options.Value;
            this.httpHelperService = httpHelperService;
        }

        public string GenerateOAuthRequestUrl(string scope, string redirectUrl, string stateVerifier)
        {
            var state = HashHelper.ComputeHash(stateVerifier);

            var queryParams = new Dictionary<string, string?>
            {
                {"client_id", oAuthSettings.ClientId },
                {"redirect_uri", redirectUrl },
                {"scope", scope },
                {"state", state },
            };

            var url = QueryHelpers.AddQueryString(OAUTH_SERVER_ENDPOINT, queryParams);
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

            return await httpHelperService.SendPostRequestAsync<GitHubOAuthTokenResult>(
                TOKEN_SERVER_ENDPOINT, authParams, cancellationToken: cancellationToken);
        }
    }
}
