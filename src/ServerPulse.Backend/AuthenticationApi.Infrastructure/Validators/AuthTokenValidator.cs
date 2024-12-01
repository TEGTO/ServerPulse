using AuthenticationApi.Dtos;
using FluentValidation;

namespace AuthenticationApi.Infrastructure.Validators
{
    public class AuthTokenValidator : AbstractValidator<AuthToken>
    {
        public AuthTokenValidator()
        {
            RuleFor(x => x.AccessToken).NotNull().NotEmpty().MaximumLength(2048);
            RuleFor(x => x.RefreshToken).NotNull().NotEmpty().MaximumLength(2048);
        }
    }
}
