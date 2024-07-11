using FluentValidation;
using Shared.Dtos;

namespace Shared.Validators
{
    public class CheckAuthDataRequestValidator : AbstractValidator<CheckAuthDataRequest>
    {
        public CheckAuthDataRequestValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
