namespace AuthenticationApi.Dtos
{
    public class UserRegistrationRequest
    {
        public string RedirectConfirmUrl { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
