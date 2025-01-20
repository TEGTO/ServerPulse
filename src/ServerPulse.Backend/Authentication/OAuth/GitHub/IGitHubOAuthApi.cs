using Refit;

namespace Authentication.OAuth.GitHub
{
    public interface IGitHubOAuthApi
    {
        [Post("")]
        public Task<GitHubOAuthTokenResult?> ExchangeAuthorizationCodeAsync(
           [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> authParams,
           CancellationToken cancellationToken);
    }
}
