using Refit;

namespace Authentication.OAuth.GitHub
{
    public interface IGitHubOAuthApi
    {
        [Post(ExternalEndpoints.GITHUB_OAUTH_API_ACCESS_TOKEN)]
        public Task<GitHubOAuthTokenResult?> ExchangeAuthorizationCodeAsync(
           [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> authParams,
           CancellationToken cancellationToken);
    }
}
