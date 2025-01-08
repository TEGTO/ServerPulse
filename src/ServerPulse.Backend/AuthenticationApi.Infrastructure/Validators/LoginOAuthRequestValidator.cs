using AuthenticationApi.Infrastructure.Dtos.Endpoints.OAuth.LoginOAuth;
using FluentValidation;

namespace UserApi.Validators
{
    public class LoginOAuthRequestValidator : AbstractValidator<LoginOAuthRequest>
    {
        public LoginOAuthRequestValidator()
        {
            RuleFor(x => x.Code).NotNull().NotEmpty().MaximumLength(1024);
            RuleFor(x => x.CodeVerifier).NotNull().NotEmpty().MaximumLength(1024);
            RuleFor(x => x.RedirectUrl).NotNull().NotEmpty().MaximumLength(1024);
        }
    }
}
