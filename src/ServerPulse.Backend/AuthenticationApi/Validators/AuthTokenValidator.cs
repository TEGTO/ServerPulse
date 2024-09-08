using AuthenticationApi.Domain.Dtos;
using FluentValidation;

namespace AuthenticationApi.Validators
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
