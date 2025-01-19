using Newtonsoft.Json;

namespace Authentication.OAuth.GitHub
{
    public class GitHubOAuthTokenResult
    {
        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }
        [JsonProperty("scope")]
        public string? Scope { get; set; }
        [JsonProperty("token_type")]
        public string? TokenType { get; set; }
    }
}
