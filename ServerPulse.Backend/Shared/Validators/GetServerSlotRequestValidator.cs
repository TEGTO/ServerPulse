using FluentValidation;
using Shared.Dtos;

namespace Shared.Validators
{
    public class GetServerSlotRequestValidator : AbstractValidator<GetServerSlotRequest>
    {
        public GetServerSlotRequestValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotNull().NotEmpty().MaximumLength(256);
            RuleFor(x => x.ServerSlotId).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
