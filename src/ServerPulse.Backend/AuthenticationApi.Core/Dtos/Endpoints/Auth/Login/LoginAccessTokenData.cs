namespace AuthenticationApi.Core.Dtos.Endpoints.Auth.Login
{
    public class LoginAccessTokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryDate { get; set; }
    }
}
