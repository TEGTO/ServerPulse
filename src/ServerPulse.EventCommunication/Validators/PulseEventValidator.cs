using FluentValidation;
using ServerPulse.EventCommunication.Events;

namespace EventCommunication.Validators
{
    public class PulseEventValidator : AbstractValidator<PulseEvent>
    {
        public PulseEventValidator()
        {
            RuleFor(x => x.Key).NotNull().NotEmpty().MaximumLength(256);
        }
    }
}