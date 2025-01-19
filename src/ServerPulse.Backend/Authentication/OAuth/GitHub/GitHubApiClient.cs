using Helper.Services;
using Microsoft.Extensions.Options;

namespace Authentication.OAuth.GitHub
{
    public sealed class GitHubApiClient : IGitHubApiClient
    {
        private const string GITHUB_API_URL = "https://api.github.com";
        private readonly GitHubOAuthSettings oAuthSettings;
        private readonly IHttpHelper httpHelperService;

        public GitHubApiClient(IOptions<GitHubOAuthSettings> options, IHttpHelper httpHelperService)
        {
            this.httpHelperService = httpHelperService;
            oAuthSettings = options.Value;
        }

        public async Task<GitHubUserResult?> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
        {
            var endpoint = GITHUB_API_URL + "/user";

            var headers = new Dictionary<string, string>
            {
                { "User-Agent", oAuthSettings.AppName }
            };

            var response = await httpHelperService.SendGetRequestAsync<GitHubUserResult>(
               endpoint, accessToken: accessToken, headers: headers, cancellationToken: cancellationToken);

            return response;
        }
    }
}
