using FluentValidation;
using ServerApi.Domain.Dtos;

namespace ServerApi.Validators
{
    public class CreateServerSlotRequestValidator : AbstractValidator<CreateServerSlotRequest>
    {
        public CreateServerSlotRequestValidator()
        {
            RuleFor(x => x.ServerSlotName).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}