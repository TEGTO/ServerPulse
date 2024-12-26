namespace AuthenticationApi.Dtos.OAuth
{
    public class UserOAuthenticationRequest
    {
        public string Code { get; set; } = string.Empty;
        public string CodeVerifier { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public OAuthLoginProvider OAuthLoginProvider { get; set; }
    }
}
