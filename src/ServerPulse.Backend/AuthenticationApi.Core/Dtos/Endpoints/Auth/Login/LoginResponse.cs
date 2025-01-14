namespace AuthenticationApi.Core.Dtos.Endpoints.Auth.Login
{
    public class LoginResponse
    {
        public LoginAccessTokenData? AccessTokenData { get; set; }
        public string? Email { get; set; }
    }
}
