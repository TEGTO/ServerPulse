namespace AuthenticationApi.Core.Dtos.Endpoints.Auth.ConfirmEmail
{
    public class ConfirmEmailResponse
    {
        public ConfirmEmailAccessTokenData? AccessTokenData { get; set; }
        public string? Email { get; set; }
    }
}
