using AuthData.Domain.Entities;

namespace AuthenticationApi.Domain.Models
{
    public record class RegisterUserModel(User User, string Password);
}
