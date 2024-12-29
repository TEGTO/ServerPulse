namespace AuthenticationApi.Infrastructure.Models
{
    public class LoginUserModel
    {
        public required string Login { get; set; }
        public required string Password { get; set; }
    }
}
