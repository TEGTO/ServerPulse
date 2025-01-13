using AuthenticationApi.Core.Dtos.Endpoints.OAuth.LoginOAuth;
using FluentValidation;

namespace AuthenticationApi.Application.Validators
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
