using FluentValidation;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Infrastructure.Validators
{
    public class CreateServerSlotRequestValidator : AbstractValidator<CreateServerSlotRequest>
    {
        public CreateServerSlotRequestValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}