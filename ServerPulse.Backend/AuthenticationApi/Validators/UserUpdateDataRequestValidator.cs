using AuthenticationApi.Domain.Dtos;
using FluentValidation;

namespace AuthenticationApi.Validators
{
    public class UserUpdateDataRequestValidator : AbstractValidator<UserUpdateDataRequest>
    {
        public UserUpdateDataRequestValidator()
        {
            RuleFor(x => x.OldEmail).NotNull().NotEmpty().EmailAddress();
            RuleFor(x => x.NewEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.NewEmail));
            RuleFor(x => x.OldPassword).NotNull().NotEmpty().MinimumLength(8);
            RuleFor(x => x.NewPassword).MinimumLength(8).When(x => !string.IsNullOrEmpty(x.NewPassword));
        }
    }
}