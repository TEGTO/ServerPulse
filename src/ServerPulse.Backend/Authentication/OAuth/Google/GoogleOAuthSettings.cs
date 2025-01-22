namespace Authentication.OAuth.Google
{
    public class GoogleOAuthSettings
    {
        public const string SETTINGS_SECTION = "AuthSettings:GoogleOAuth";

        public string GoogleOAuthUrl { get; set; } = string.Empty;
        public string GoogleOAuthTokenUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
    }
}
