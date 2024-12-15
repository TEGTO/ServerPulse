namespace AuthenticationApi.Dtos
{
    public class UserAuthenticationResponse
    {
        public AccessTokenDataDto? AuthToken { get; set; }
        public string? Email { get; set; }
    }
}
