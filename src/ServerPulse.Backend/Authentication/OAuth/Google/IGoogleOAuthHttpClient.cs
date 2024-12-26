namespace Authentication.OAuth.Google
{
    public interface IGoogleOAuthHttpClient
    {
        public Task<GoogleOAuthTokenResult?> ExchangeAuthorizationCodeAsync(string code, string codeVerifier, string redirectUrl, CancellationToken cancellationToken);
        public string GenerateOAuthRequestUrl(string scope, string redirectUrl, string codeVerifier);
        public Task<GoogleOAuthTokenResult?> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken);
    }
}