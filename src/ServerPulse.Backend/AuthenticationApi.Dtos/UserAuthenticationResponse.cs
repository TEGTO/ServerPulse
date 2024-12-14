namespace AuthenticationApi.Dtos
{
    public class UserAuthenticationResponse
    {
        public AuthToken? AuthToken { get; set; }
        public string? Email { get; set; }
    }
}
