using AuthenticationApi.Core.Dtos.Endpoints.Auth.UserUpdate;
using FluentValidation;

namespace AuthenticationApi.Application.Validators
{
    public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
    {
        public UserUpdateRequestValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.OldPassword).MinimumLength(8).When(x => !string.IsNullOrEmpty(x.OldPassword)).MaximumLength(256);
            RuleFor(x => x.Password).MinimumLength(8).When(x => !string.IsNullOrEmpty(x.Password)).MaximumLength(256);
        }
    }
}