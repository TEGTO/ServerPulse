
namespace Authentication.OAuth.GitHub
{
    public interface IGitHubOAuthClient
    {
        public Task<GitHubOAuthTokenResult?> ExchangeAuthorizationCodeAsync(string code, string redirectUrl, CancellationToken cancellationToken);
        public string GenerateOAuthRequestUrl(string scope, string redirectUrl, string stateVerifier);
    }
}