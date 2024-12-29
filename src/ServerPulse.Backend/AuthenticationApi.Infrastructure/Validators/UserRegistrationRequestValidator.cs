using AuthenticationApi.Dtos;
using FluentValidation;

namespace AuthenticationApi.Infrastructure.Validators
{
    public class UserRegistrationRequestValidator : AbstractValidator<UserRegistrationRequest>
    {
        public UserRegistrationRequestValidator()
        {
            RuleFor(x => x.RedirectConfirmUrl).MaximumLength(1024);
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(8).MaximumLength(256);
            RuleFor(x => x.ConfirmPassword).NotNull().NotEmpty().Must((model, field) => field == model.Password)
                .WithMessage("Passwords do not match.").MaximumLength(256);
        }
    }
}