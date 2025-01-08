using FluentValidation;
using ServerSlotApi.Dtos.Endpoints.ServerSlot.CreateSlot;

namespace ServerSlotApi.Infrastructure.Validators
{
    public class CreateSlotRequestValidator : AbstractValidator<CreateSlotRequest>
    {
        public CreateSlotRequestValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}