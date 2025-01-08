using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.Login;
using FluentValidation;

namespace AuthenticationApi.Infrastructure.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Login).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(8).MaximumLength(256);
        }
    }
}