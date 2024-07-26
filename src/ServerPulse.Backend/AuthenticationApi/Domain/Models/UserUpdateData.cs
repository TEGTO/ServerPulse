namespace AuthenticationApi.Domain.Models
{
    public class UserUpdateData
    {
        public string? UserName { get; set; }
        public string? OldEmail { get; set; }
        public string? NewEmail { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
