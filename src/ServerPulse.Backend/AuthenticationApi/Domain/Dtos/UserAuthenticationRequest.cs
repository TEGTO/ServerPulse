namespace AuthenticationApi.Domain.Dtos
{
    public class UserAuthenticationRequest
    {
        public string? Login { get; set; }
        public string? Password { get; set; }
    }
}
