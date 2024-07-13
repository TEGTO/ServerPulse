using FluentValidation;
using ServerSlotApi.Domain.Dtos;

namespace ServerSlotApi.Validators
{
    public class CreateServerSlotRequestValidator : AbstractValidator<CreateServerSlotRequest>
    {
        public CreateServerSlotRequestValidator()
        {
            RuleFor(x => x.ServerSlotName).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}