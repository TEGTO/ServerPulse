namespace AuthenticationApi.Domain.Dtos
{
    public class UserAuthenticationResponse
    {
        public AuthToken? AuthToken { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}
