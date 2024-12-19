namespace AuthenticationApi.Infrastructure
{
    public class UserUpdateModel
    {
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? OldPassword { get; set; }
        public string? Password { get; set; }
    }
}
