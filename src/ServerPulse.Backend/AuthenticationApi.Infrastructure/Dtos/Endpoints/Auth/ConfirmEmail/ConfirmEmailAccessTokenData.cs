namespace AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.ConfirmEmail
{
    public class ConfirmEmailAccessTokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryDate { get; set; }
    }
}
