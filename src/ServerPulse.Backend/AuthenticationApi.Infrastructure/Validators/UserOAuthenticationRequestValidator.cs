using AuthenticationApi.Dtos.OAuth;
using FluentValidation;

namespace UserApi.Validators
{
    public class UserOAuthenticationRequestValidator : AbstractValidator<UserOAuthenticationRequest>
    {
        public UserOAuthenticationRequestValidator()
        {
            RuleFor(x => x.Code).NotNull().NotEmpty().MaximumLength(1024);
            RuleFor(x => x.CodeVerifier).NotNull().NotEmpty().MaximumLength(1024);
            RuleFor(x => x.RedirectUrl).NotNull().NotEmpty().MaximumLength(1024);
        }
    }
}
