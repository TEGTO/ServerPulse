using FluentValidation;
using Shared.Dtos.ServerSlot;

namespace ServerSlotApi.Validators
{
    public class CheckSlotKeyRequestValidator : AbstractValidator<CheckSlotKeyRequest>
    {
        public CheckSlotKeyRequestValidator()
        {
            RuleFor(x => x.SlotKey).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
