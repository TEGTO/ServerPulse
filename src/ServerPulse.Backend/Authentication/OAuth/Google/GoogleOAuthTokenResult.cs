﻿using Newtonsoft.Json;

namespace Authentication.OAuth.Google
{
    public class GoogleOAuthTokenResult
    {
        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public double ExpiresInSeconds { get; set; }
        [JsonProperty("id_token")]
        public string? IdToken { get; set; }
        [JsonProperty("refresh_token")]
        public string? RefreshToken { get; set; }
        [JsonProperty("scope")]
        public string? Scope { get; set; }
        [JsonProperty("token_type")]
        public string? TokenType { get; set; }
    }
}
