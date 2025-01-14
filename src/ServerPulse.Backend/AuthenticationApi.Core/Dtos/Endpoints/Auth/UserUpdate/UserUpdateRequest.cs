namespace AuthenticationApi.Core.Dtos.Endpoints.Auth.UserUpdate
{
    public class UserUpdateRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? OldPassword { get; set; }
        public string? Password { get; set; }
    }
}
