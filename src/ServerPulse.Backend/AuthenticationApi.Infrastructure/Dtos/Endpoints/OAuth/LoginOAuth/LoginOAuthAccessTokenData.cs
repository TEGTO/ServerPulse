namespace AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.LoginOAuth
{
    public class LoginOAuthAccessTokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryDate { get; set; }
    }
}
