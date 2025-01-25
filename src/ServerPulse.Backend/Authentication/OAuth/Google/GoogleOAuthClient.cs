using Helper.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Authentication.OAuth.Google
{
    internal sealed class GoogleOAuthClient : IGoogleOAuthClient
    {
        private readonly IGoogleOAuthApi googleOAuthApi;
        private readonly GoogleOAuthSettings oAuthSettings;

        public GoogleOAuthClient(IOptions<GoogleOAuthSettings> options, IGoogleOAuthApi googleOAuthApi)
        {
            oAuthSettings = options.Value;
            this.googleOAuthApi = googleOAuthApi;
        }

        public string GenerateOAuthRequestUrl(string redirectUrl, string codeVerifier, string? scope = null)
        {
            if (string.IsNullOrEmpty(scope))
            {
                scope = oAuthSettings.Scope;
            }

            var codeChallenge = HashHelper.ComputeHash(codeVerifier);

            var queryParams = new Dictionary<string, string?>
            {
                {"client_id", oAuthSettings.ClientId },
                {"redirect_uri", redirectUrl },
                {"response_type", "code" },
                {"scope", scope },
                {"code_challenge", codeChallenge },
                {"code_challenge_method", "S256" },
                {"access_type", "offline" }
            };

            var url = QueryHelpers.AddQueryString(oAuthSettings.GoogleOAuthTokenUrl, queryParams);
            return url;
        }

        public async Task<GoogleOAuthTokenResult?> ExchangeAuthorizationCodeAsync(
            string code, string codeVerifier, string redirectUrl, CancellationToken cancellationToken)
        {
            var authParams = new Dictionary<string, string>
            {
                { "client_id", oAuthSettings.ClientId },
                { "client_secret", oAuthSettings.ClientSecret },
                { "code", code },
                { "code_verifier", codeVerifier },
                { "grant_type", "authorization_code" },
                { "redirect_uri", redirectUrl }
            };

            return await googleOAuthApi.ExchangeAuthorizationCodeAsync(authParams, cancellationToken);
        }
    }
}
