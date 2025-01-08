using AuthenticationApi.Dtos.OAuth;

namespace AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.LoginOAuth
{
    public class LoginOAuthRequest
    {
        public string Code { get; set; } = string.Empty;
        public string CodeVerifier { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public OAuthLoginProvider OAuthLoginProvider { get; set; }
    }
}
