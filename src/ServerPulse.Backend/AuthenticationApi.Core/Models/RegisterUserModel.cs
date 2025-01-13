using AuthenticationApi.Core.Entities;

namespace AuthenticationApi.Core.Models
{
    public record class RegisterUserModel
    {
        public required User User { get; set; }
        public required string Password { get; set; }
    }
}
