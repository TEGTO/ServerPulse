using AuthenticationApi.Domain.Dtos;
using FluentValidation;

namespace AuthenticationApi.Validators
{
    public class UserUpdateDataRequestValidator : AbstractValidator<UserUpdateDataRequest>
    {
        public UserUpdateDataRequestValidator()
        {
            RuleFor(x => x.UserName).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.OldEmail).NotNull().NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.NewEmail).EmailAddress().When(x => !string.IsNullOrEmpty(x.NewEmail)).MaximumLength(256);
            RuleFor(x => x.OldPassword).NotNull().NotEmpty().MinimumLength(8).MaximumLength(256);
            RuleFor(x => x.NewPassword).MinimumLength(8).When(x => !string.IsNullOrEmpty(x.NewPassword)).MaximumLength(256);
        }
    }
}