namespace AuthenticationApi.Dtos
{
    public class UserUpdateDataRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? OldPassword { get; set; }
        public string? Password { get; set; }
    }
}
