using AuthenticationApi.Core.Enums;

namespace AuthenticationApi.Core.Dtos.Endpoints.OAuth.LoginOAuth
{
    public class LoginOAuthRequest
    {
        public string QueryParams { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public OAuthLoginProvider OAuthLoginProvider { get; set; }
    }
}
