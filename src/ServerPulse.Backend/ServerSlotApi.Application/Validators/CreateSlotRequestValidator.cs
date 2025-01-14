using FluentValidation;
using ServerSlotApi.Core.Dtos.Endpoints.ServerSlot.CreateSlot;

namespace ServerSlotApi.Application.Validators
{
    public class CreateSlotRequestValidator : AbstractValidator<CreateSlotRequest>
    {
        public CreateSlotRequestValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}