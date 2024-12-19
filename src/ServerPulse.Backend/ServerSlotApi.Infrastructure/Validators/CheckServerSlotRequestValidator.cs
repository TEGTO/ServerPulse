using FluentValidation;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Infrastructure.Validators
{
    public class CheckServerSlotRequestValidator : AbstractValidator<CheckSlotKeyRequest>
    {
        public CheckServerSlotRequestValidator()
        {
            RuleFor(x => x.SlotKey).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
