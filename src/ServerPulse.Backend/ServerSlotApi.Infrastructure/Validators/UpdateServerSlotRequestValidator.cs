using FluentValidation;
using ServerSlotApi.Dtos;

namespace ServerSlotApi.Infrastructure.Validators
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
