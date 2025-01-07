using AuthenticationApi.Dtos;

namespace AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.ConfirmEmail
{
    public class ConfirmEmailResponse
    {
        public AccessTokenDataDto? AccessTokenData { get; set; }
        public string? Email { get; set; }
    }
}
