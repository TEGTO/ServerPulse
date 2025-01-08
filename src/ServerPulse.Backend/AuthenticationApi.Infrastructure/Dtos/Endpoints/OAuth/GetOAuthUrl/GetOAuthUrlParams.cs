using AuthenticationApi.Dtos.OAuth;

namespace AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.GetOAuthUrl
{
    public class GetOAuthUrlParams
    {
        public OAuthLoginProvider OAuthLoginProvider { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
        public string CodeVerifier { get; set; } = string.Empty;
    }
}
