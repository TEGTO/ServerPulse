namespace AuthenticationApi.Domain.Dtos
{
    public class UserAuthenticationRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
