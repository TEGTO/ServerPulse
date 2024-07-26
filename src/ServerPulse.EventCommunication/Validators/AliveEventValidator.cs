using FluentValidation;
using ServerPulse.EventCommunication.Events;

namespace EventCommunication.Validators
{
    public class AliveEventValidator : AbstractValidator<AliveEvent>
    {
        public AliveEventValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}
