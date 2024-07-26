using FluentValidation;
using ServerPulse.EventCommunication.Events;

namespace EventCommunication.Validators
{
    public class BaseEventValidator : AbstractValidator<BaseEvent>
    {
        public BaseEventValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
