using FluentValidation;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.CheckSlotKey;

namespace ServerSlotApi.Application.Validators
{
    public class CheckSlotKeyRequestValidator : AbstractValidator<CheckSlotKeyRequest>
    {
        public CheckSlotKeyRequestValidator()
        {
            RuleFor(x => x.SlotKey).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
