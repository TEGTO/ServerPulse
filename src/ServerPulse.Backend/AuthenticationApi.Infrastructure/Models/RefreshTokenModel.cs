using Authentication.Models;

namespace AuthenticationApi.Infrastructure
{
    public record class RefreshTokenModel(User User, AccessTokenData AccessTokenData);
}
