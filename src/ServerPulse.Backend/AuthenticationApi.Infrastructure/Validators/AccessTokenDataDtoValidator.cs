using AuthenticationApi.Dtos;
using FluentValidation;

namespace AuthenticationApi.Infrastructure.Validators
{
    public class AccessTokenDataDtoValidator : AbstractValidator<AccessTokenDataDto>
    {
        public AccessTokenDataDtoValidator()
        {
            RuleFor(x => x.AccessToken).NotNull().NotEmpty().MaximumLength(2048);
            RuleFor(x => x.RefreshToken).NotNull().NotEmpty().MaximumLength(2048);
        }
    }
}
