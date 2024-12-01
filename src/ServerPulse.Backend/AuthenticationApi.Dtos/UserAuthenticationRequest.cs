namespace AuthenticationApi.Dtos
{
    public class UserAuthenticationRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
