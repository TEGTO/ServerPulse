using Refit;

namespace Authentication.OAuth.Google
{
    public interface IGoogleOAuthApi
    {
        [Post("")]
        public Task<GoogleOAuthTokenResult?> ExchangeAuthorizationCodeAsync(
           [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> authParams,
           CancellationToken cancellationToken);
    }
}
