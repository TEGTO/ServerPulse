using AuthData.Domain.Entities;

namespace AuthenticationApi.Domain.Models
{
    public record class LoginUserModel(User User, string Password);
}
