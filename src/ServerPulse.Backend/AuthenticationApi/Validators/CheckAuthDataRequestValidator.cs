using FluentValidation;
using Shared.Dtos.Auth;

namespace AuthenticationApi.Validators
{
    public class CheckAuthDataRequestValidator : AbstractValidator<CheckAuthDataRequest>
    {
        public CheckAuthDataRequestValidator()
        {
            RuleFor(x => x.Login).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(8).MaximumLength(256);
        }
    }
}
