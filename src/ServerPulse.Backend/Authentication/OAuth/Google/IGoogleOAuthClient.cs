namespace Authentication.OAuth.Google
{
    public interface IGoogleOAuthClient
    {
        public string GenerateOAuthRequestUrl(string scope, string redirectUrl, string codeVerifier);
        public Task<GoogleOAuthTokenResult?> ExchangeAuthorizationCodeAsync(
            string code, string codeVerifier, string redirectUrl, CancellationToken cancellationToken);
    }
}