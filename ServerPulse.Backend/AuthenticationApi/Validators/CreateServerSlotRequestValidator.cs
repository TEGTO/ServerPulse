using AuthenticationApi.Domain.Dtos;
using FluentValidation;

namespace AuthenticationApi.Validators
{
    public class CreateServerSlotRequestValidator : AbstractValidator<CreateServerSlotRequest>
    {
        public CreateServerSlotRequestValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
            RuleFor(x => x.UserEmail).NotNull().NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}