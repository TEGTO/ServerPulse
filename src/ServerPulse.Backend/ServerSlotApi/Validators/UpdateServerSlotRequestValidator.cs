using FluentValidation;
using ServerSlotApi.Domain.Dtos;

namespace ServerSlotApi.Validators
{
    public class UpdateServerSlotRequestValidator : AbstractValidator<UpdateServerSlotRequest>
    {
        public UpdateServerSlotRequestValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.Name).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
