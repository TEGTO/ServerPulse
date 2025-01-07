using AuthenticationApi.Dtos;

namespace AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Login
{
    public class LoginResponse
    {
        public AccessTokenDataDto? AccessTokenData { get; set; }
        public string? Email { get; set; }
    }
}
