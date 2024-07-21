using FluentValidation;
using Shared.Dtos.ServerSlot;

namespace Shared.Validators
{
    public class CheckServerSlotRequestValidator : AbstractValidator<CheckSlotKeyRequest>
    {
        public CheckServerSlotRequestValidator()
        {
            RuleFor(x => x.SlotKey).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
