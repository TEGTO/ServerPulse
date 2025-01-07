using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.RefreshToken;
using FluentValidation;

namespace UserApi.Validators
{
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.AccessToken).NotNull().NotEmpty().MaximumLength(2048);
            RuleFor(x => x.RefreshToken).NotNull().NotEmpty().MaximumLength(2048);
        }
    }
}
