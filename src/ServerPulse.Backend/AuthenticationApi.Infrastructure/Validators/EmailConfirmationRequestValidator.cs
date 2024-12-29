using AuthenticationApi.Dtos;
using FluentValidation;

namespace AuthenticationApi.Infrastructure.Validators
{
    public class EmailConfirmationRequestValidator : AbstractValidator<EmailConfirmationRequest>
    {
        public EmailConfirmationRequestValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Token).NotNull().NotEmpty().MaximumLength(2048);
        }
    }
}
