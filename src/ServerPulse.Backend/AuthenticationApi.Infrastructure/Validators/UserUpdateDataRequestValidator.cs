using AuthenticationApi.Dtos;
using FluentValidation;

namespace AuthenticationApi.Infrastructure.Validators
{
    public class UserUpdateDataRequestValidator : AbstractValidator<UserUpdateDataRequest>
    {
        public UserUpdateDataRequestValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.OldPassword).MinimumLength(8).When(x => !string.IsNullOrEmpty(x.OldPassword)).MaximumLength(256);
            RuleFor(x => x.Password).MinimumLength(8).When(x => !string.IsNullOrEmpty(x.Password)).MaximumLength(256);
        }
    }
}