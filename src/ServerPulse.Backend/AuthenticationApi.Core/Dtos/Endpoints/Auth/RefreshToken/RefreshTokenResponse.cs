namespace AuthenticationApi.Core.Dtos.Endpoints.Auth.RefreshToken
{
    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryDate { get; set; }
    }
}
