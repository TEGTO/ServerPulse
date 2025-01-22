using Refit;

namespace Authentication.OAuth.Google
{
    public interface IGoogleOAuthApi
    {
        [Post(ExternalEndpoints.GOOGLE_OAUTH_API_TOKEN)]
        public Task<GoogleOAuthTokenResult?> ExchangeAuthorizationCodeAsync(
           [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> authParams,
           CancellationToken cancellationToken);
    }
}
