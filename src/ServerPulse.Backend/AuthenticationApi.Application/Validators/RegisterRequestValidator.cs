using AuthenticationApi.Core.Dtos.Endpoints.Auth.Register;
using FluentValidation;

namespace AuthenticationApi.Application.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.RedirectConfirmUrl).MaximumLength(1024);
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(8).MaximumLength(256);
            RuleFor(x => x.ConfirmPassword).NotNull().NotEmpty().Must((model, field) => field == model.Password)
                .WithMessage("Passwords do not match.").MaximumLength(256);
        }
    }
}