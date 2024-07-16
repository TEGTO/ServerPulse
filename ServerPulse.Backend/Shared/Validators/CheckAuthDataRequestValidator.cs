using FluentValidation;
using Shared.Dtos;

namespace Shared.Validators
{
    public class CheckAuthDataRequestValidator : AbstractValidator<CheckAuthDataRequest>
    {
        public CheckAuthDataRequestValidator()
        {
            RuleFor(x => x.Login).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.Password).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
