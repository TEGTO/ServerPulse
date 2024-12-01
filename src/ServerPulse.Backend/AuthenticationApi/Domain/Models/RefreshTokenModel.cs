using AuthData.Domain.Entities;
using Authentication.Models;

namespace AuthenticationApi.Domain.Models
{
    public record class RefreshTokenModel(User User, AccessTokenData AccessTokenData);
}
