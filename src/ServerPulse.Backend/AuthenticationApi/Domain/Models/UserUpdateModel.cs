namespace AuthenticationApi.Domain.Models
{
    public class UserUpdateModel
    {
        public required string UserName { get; set; }
        public required string OldEmail { get; set; }
        public string? NewEmail { get; set; }
        public required string OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
