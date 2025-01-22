namespace Authentication.OAuth.Google
{
    public interface IGoogleOAuthClient
    {
        public string GenerateOAuthRequestUrl(string redirectUrl, string codeVerifier, string? scope = null);

        public Task<GoogleOAuthTokenResult?> ExchangeAuthorizationCodeAsync(
            string code, string codeVerifier, string redirectUrl, CancellationToken cancellationToken);
    }
}