using FluentValidation;
using MessageBus.Dtos;

namespace Shared.Validators
{
    public class BaseEventValidator : AbstractValidator<BaseEvent>
    {
        public BaseEventValidator()
        {
            RuleFor(x => x.SeverSlotId).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
