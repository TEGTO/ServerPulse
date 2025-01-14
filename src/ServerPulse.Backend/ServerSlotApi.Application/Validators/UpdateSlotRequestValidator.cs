using FluentValidation;
using ServerSlotApi.Core.Dtos.Endpoints.Slot.UpdateSlot;

namespace ServerSlotApi.Application.Validators
{
    public class UpdateSlotRequestValidator : AbstractValidator<UpdateSlotRequest>
    {
        public UpdateSlotRequestValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
