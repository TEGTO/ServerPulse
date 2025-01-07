using AuthenticationApi.Infrastructure.Dtos.Endpoints.Auth.ConfirmEmail;
using FluentValidation;

namespace AuthenticationApi.Infrastructure.Validators
{
    public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
    {
        public ConfirmEmailRequestValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Token).NotNull().NotEmpty().MaximumLength(2048);
        }
    }
}
