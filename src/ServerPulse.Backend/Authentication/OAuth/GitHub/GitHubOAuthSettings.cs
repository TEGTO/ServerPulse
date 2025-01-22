namespace Authentication.OAuth.GitHub
{
    public class GitHubOAuthSettings
    {
        public const string SETTINGS_SECTION = "AuthSettings:GitHubOAuth";

        public string GitHubOAuthApiUrl { get; set; } = string.Empty;
        public string GitHubApiUrl { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
    }
}
