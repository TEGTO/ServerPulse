using FluentValidation;
using Shared.Dtos.ServerSlot;

namespace Shared.Validators
{
    public class CheckServerSlotRequestValidator : AbstractValidator<CheckServerSlotRequest>
    {
        public CheckServerSlotRequestValidator()
        {
            RuleFor(x => x.SlotId).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
