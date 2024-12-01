namespace AuthenticationApi.Dtos
{
    public class UserUpdateDataRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string OldEmail { get; set; } = string.Empty;
        public string? NewEmail { get; set; }
        public string OldPassword { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
    }
}
