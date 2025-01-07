using AuthenticationApi.Dtos;

namespace AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.LoginOAuth
{
    public class LoginOAuthResponse
    {
        public AccessTokenDataDto? AccessTokenData { get; set; }
        public string? Email { get; set; }
    }
}
