namespace AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.LoginOAuth
{
    public class LoginOAuthResponse
    {
        public LoginOAuthAccessTokenData? AccessTokenData { get; set; }
        public string? Email { get; set; }
    }
}
