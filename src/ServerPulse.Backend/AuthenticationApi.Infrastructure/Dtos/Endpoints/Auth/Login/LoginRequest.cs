namespace AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Login
{
    public class LoginRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
