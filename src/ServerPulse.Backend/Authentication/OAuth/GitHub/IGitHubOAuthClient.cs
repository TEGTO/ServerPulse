
namespace Authentication.OAuth.GitHub
{
    public interface IGitHubOAuthClient
    {
        public Task<GitHubOAuthTokenResult?> ExchangeAuthorizationCodeAsync(string code, string redirectUrl, CancellationToken cancellationToken);
        public string GenerateOAuthRequestUrl(string redirectUrl, string stateVerifier, string? scope = null);
        public bool VerifyState(string stateVerifier, string state);
    }
}