using AuthenticationApi.Core.Enums;

namespace AuthenticationApi.Core.Dtos.Endpoints.OAuth.GetOAuthUrl
{
    public class GetOAuthUrlParams
    {
        public OAuthLoginProvider OAuthLoginProvider { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
