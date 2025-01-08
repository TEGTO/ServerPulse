using FluentValidation;
using ServerSlotApi.Dtos.Endpoints.Slot.CheckSlotKey;

namespace ServerSlotApi.Infrastructure.Validators
{
    public class CheckSlotKeyRequestValidator : AbstractValidator<CheckSlotKeyRequest>
    {
        public CheckSlotKeyRequestValidator()
        {
            RuleFor(x => x.SlotKey).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
