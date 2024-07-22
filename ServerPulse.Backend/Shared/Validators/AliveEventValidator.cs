using FluentValidation;
using Shared.Dtos.ServerEvent;

namespace Shared.Validators
{
    public class AliveEventValidator : AbstractValidator<AliveEvent>
    {
        public AliveEventValidator()
        {
            RuleFor(x => x.SlotKey).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
