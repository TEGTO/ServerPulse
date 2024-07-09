using FluentValidation;
using Shared.Dtos;

namespace Shared.Validators
{
    public class ServerSlotDtoValidator : AbstractValidator<ServerSlotDto>
    {
        public ServerSlotDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
            RuleFor(x => x.UserEmail).NotNull().NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}